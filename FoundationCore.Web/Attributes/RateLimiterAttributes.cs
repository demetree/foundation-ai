using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Foundation.Controllers
{
    public enum RateLimitOption
    {
        OnePerMinute = 1,
        TwoPerMinute = 2,
        FivePerMinute = 3,
        TenPerMinute = 4,
        ThirtyPerMinute = 5,
        OneHundredPerMinute = 6,
        OnePerSecond = 7,
        TwoPerSecond = 8,
        FivePerSecond = 9,
        TenPerSecond = 10,
        TwentyPerSecond = 11,
        FiftyPerSecond = 12,
        OneHundredPerSecond = 13,
        FiveHundredPerSecond = 14,
        NoLimit = 100
    }

    public enum RateLimitScope
    {
        PerUser = 1,
        PerClientIp = 2,
        Global = 3
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<string, TokenBucket> Buckets = new ConcurrentDictionary<string, TokenBucket>();

        public RateLimitOption LimitOption { get; set; }
        public RateLimitScope Scope { get; set; } = RateLimitScope.PerUser; // Default to per-user
        public string KeyPrefix { get; set; } = "RateLimit";
        public string ErrorMessage { get; set; } = "Too many requests. Rate limit exceeded. Try again in {0} seconds.";

        public string[] ExemptRoles { get; set; } = Array.Empty<string>();

        public RateLimitAttribute(RateLimitOption limitOption)
        {
            LimitOption = limitOption;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (LimitOption == RateLimitOption.NoLimit ||
                    (Scope == RateLimitScope.PerUser && context.HttpContext.User.Identity?.IsAuthenticated == true &&
                     ExemptRoles.Any(role => context.HttpContext.User.IsInRole(role))))
            {
                await base.OnActionExecutionAsync(context, next);
                return;
            }

            ILogger<RateLimitAttribute> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimitAttribute>>();

            string key = GenerateKey(context);

            (int requests, int seconds) = GetRateLimitParameters(LimitOption);

            var bucket = Buckets.GetOrAdd(key, _ => new TokenBucket(requests, seconds));

            if (bucket.IsExpired())
            {
                Buckets.TryRemove(key, out _);
                bucket = Buckets.GetOrAdd(key, _ => new TokenBucket(requests, seconds));
            }

            if (!bucket.TryConsume())
            {
                logger?.LogWarning("Rate limit exceeded for key: {Key}. Try again in {Seconds} seconds.", key, bucket.NextRefillSeconds());
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.Headers.Add("Retry-After", bucket.NextRefillSeconds().ToString());
                
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = string.Format(ErrorMessage, bucket.NextRefillSeconds()),
                    retryAfter = bucket.NextRefillSeconds()
                });

                await context.HttpContext.Response.CompleteAsync();
                context.Result = new EmptyResult();
                return;
            }

            //
            // Add limit remaining headers when not rejecting.
            //
            context.HttpContext.Response.Headers.Add("X-RateLimit-Limit", requests.ToString());
            context.HttpContext.Response.Headers.Add("X-RateLimit-Remaining", bucket.Tokens.ToString());

            await base.OnActionExecutionAsync(context, next);
        }


        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            //
            // Add rate limit headers for successful requests.  Possibly redundant, but will make sure that the headers are attached.
            //
            if (context.HttpContext.Items.TryGetValue("RateLimitBucket", out var bucketObj) &&
                context.HttpContext.Items.TryGetValue("RateLimitRequests", out var requestsObj) &&
                bucketObj is TokenBucket bucket && requestsObj is int requests)
            {
                context.HttpContext.Response.Headers.Add("X-RateLimit-Limit", requests.ToString());
                context.HttpContext.Response.Headers.Add("X-RateLimit-Remaining", bucket.Tokens.ToString());
            }

            await base.OnResultExecutionAsync(context, next);
        }


        private string GenerateKey(ActionExecutingContext context)
        {
            string clientIdentifier;

            switch (Scope)
            {
                case RateLimitScope.PerUser:
                    
                    // Use user ID or name from claims if authenticated, otherwise fall back to IP
                    clientIdentifier = context.HttpContext.User.Identity?.IsAuthenticated == true
                        ? context.HttpContext.User.Identity.Name ?? context.HttpContext.User.FindFirst("sub")?.Value ?? "anonymous"
                        : context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    break;

                case RateLimitScope.Global:

                    clientIdentifier = "global";
                    break;

                case RateLimitScope.PerClientIp:
                default:
                    clientIdentifier = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    break;
            }

            var routeData = context.RouteData;

            string controller = routeData.Values["controller"]?.ToString() ?? "unknown";
            string action = routeData.Values["action"]?.ToString() ?? "unknown";
            string projectGuid = routeData.Values["projectGuid"]?.ToString() ?? "none";

            return $"{KeyPrefix}:{clientIdentifier}:{controller}:{action}:{projectGuid}";
        }

        private (int requests, int seconds) GetRateLimitParameters(RateLimitOption option)
        {
            return option switch
            {
                RateLimitOption.OnePerMinute => (1, 60),
                RateLimitOption.TwoPerMinute => (2, 60),
                RateLimitOption.FivePerMinute => (5, 60),
                RateLimitOption.TenPerMinute => (10, 60),
                RateLimitOption.ThirtyPerMinute => (30, 60),
                RateLimitOption.OneHundredPerMinute => (100, 60),
                RateLimitOption.OnePerSecond => (1, 1),
                RateLimitOption.TwoPerSecond => (2, 1),
                RateLimitOption.FivePerSecond => (5, 1),
                RateLimitOption.TenPerSecond => (10, 1),
                RateLimitOption.TwentyPerSecond => (20, 1),
                RateLimitOption.FiftyPerSecond => (50, 1),
                RateLimitOption.OneHundredPerSecond => (100, 1),
                RateLimitOption.FiveHundredPerSecond => (500, 1),
                RateLimitOption.NoLimit => (int.MaxValue, 1),
                _ => throw new ArgumentException($"Invalid {nameof(RateLimitOption)}: {option}")
            };
        }

        private class TokenBucket
        {
            private readonly int _capacity;
            private readonly double _secondsPerToken;
            private int _tokens;
            private DateTime _lastRefill;
            private readonly TimeSpan _expiry = TimeSpan.FromMinutes(10);

            public TokenBucket(int capacity, int seconds)
            {
                _capacity = capacity;
                _secondsPerToken = seconds / (double)capacity;
                _tokens = capacity;
                _lastRefill = DateTime.UtcNow;
            }

            public bool TryConsume()
            {
                Refill();
                lock (this)
                {
                    if (_tokens > 0)
                    {
                        _tokens--;
                        return true;
                    }
                    return false;
                }
            }

            private void Refill()
            {
                lock (this)
                {
                    var now = DateTime.UtcNow;
                    var secondsElapsed = (now - _lastRefill).TotalSeconds;
                    var newTokens = (int)(secondsElapsed / _secondsPerToken);

                    if (newTokens > 0)
                    {
                        _tokens = Math.Min(_capacity, _tokens + newTokens);
                        _lastRefill = now;
                    }
                }
            }

            public int NextRefillSeconds()
            {
                return (int)Math.Ceiling(_secondsPerToken);
            }

            public bool IsExpired()
            {
                return (DateTime.UtcNow - _lastRefill) > _expiry;
            }

            public int Tokens => _tokens;
        }
    }
}