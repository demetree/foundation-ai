//
// BasicAuthMiddleware.cs
//
// HTTP Basic Authentication middleware for the WebDAV server.
//
// Extracts credentials from the Authorization header, validates them against
// the Foundation Security database using SecurityLogic.AuthenticateLocalCredentials,
// and resolves the user's tenant GUID via SecurityFramework.UserTenantGuidAsync.
//
// On success, stores a WebDavContext in HttpContext.Items for downstream handlers.
// On failure, returns 401 Unauthorized with a WWW-Authenticate challenge.
//
using System;
using System.Text;
using System.Threading.Tasks;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Scheduler.WebDAV.Services;

namespace Scheduler.WebDAV.Middleware
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BasicAuthCredentialCache _cache;
        private readonly ILogger<BasicAuthMiddleware> _logger;

        private const string REALM = "Scheduler WebDAV";

        public BasicAuthMiddleware(
            RequestDelegate next,
            BasicAuthCredentialCache cache,
            ILogger<BasicAuthMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            //
            // OPTIONS requests may come unauthenticated from WebDAV clients
            // discovering server capabilities.  Let them through so the
            // OptionsHandler can respond with DAV headers.
            //
            if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            //
            // Extract the Authorization header
            //
            string authHeader = context.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                await ChallengeAsync(context);
                return;
            }


            //
            // Decode Base64 credentials
            //
            string encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            string decodedCredentials;

            try
            {
                decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            }
            catch
            {
                await ChallengeAsync(context);
                return;
            }

            int separatorIndex = decodedCredentials.IndexOf(':');
            if (separatorIndex < 0)
            {
                await ChallengeAsync(context);
                return;
            }

            string username = decodedCredentials.Substring(0, separatorIndex);
            string password = decodedCredentials.Substring(separatorIndex + 1);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await ChallengeAsync(context);
                return;
            }


            //
            // Check the credential cache first
            //
            BasicAuthCredentialCache.CachedCredential cached = _cache.TryGet(username, password);

            if (cached != null)
            {
                //
                // Cached — set context and continue
                //
                context.Items[WebDavContext.CONTEXT_KEY] = new WebDavContext
                {
                    User = cached.User,
                    TenantGuid = cached.TenantGuid,
                    SecurityUserId = cached.User.id
                };

                await _next(context);
                return;
            }


            //
            // Validate against the Security database
            //
            try
            {
                SecurityUser user = SecurityLogic.AuthenticateLocalCredentials(username, password);

                Guid tenantGuid = await SecurityFramework.UserTenantGuidAsync(user);

                //
                // Cache the validated credential
                //
                _cache.Set(username, password, user, tenantGuid);

                //
                // Set the per-request context
                //
                context.Items[WebDavContext.CONTEXT_KEY] = new WebDavContext
                {
                    User = user,
                    TenantGuid = tenantGuid,
                    SecurityUserId = user.id
                };

                await _next(context);
            }
            catch (SecurityLogic.LocalUserNotFoundException)
            {
                _logger.LogWarning("WebDAV login failed — user not found: {Username}", username);
                await ChallengeAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WebDAV login failed for user: {Username}", username);
                await ChallengeAsync(context);
            }
        }


        private static async Task ChallengeAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{REALM}\"";
            await context.Response.WriteAsync("Unauthorized");
        }
    }
}
