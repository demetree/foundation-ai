//
// Monitored Application Service
//
// Provides functionality to fetch health status from remote Foundation-based applications.
// Handles HTTP calls with proper timeout and error handling.
//
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    /// Configuration model for a monitored application
    /// </summary>
    public class MonitoredApplicationConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsSelf { get; set; } = false;
    }


    /// <summary>
    /// Result of a health status check for a monitored application
    /// </summary>
    public class MonitoredApplicationStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsSelf { get; set; } = false;
        public bool IsAvailable { get; set; } = false;
        public string Status { get; set; } = "Unknown";
        public string? Error { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
        public object? HealthData { get; set; }
    }


    /// <summary>
    /// Service for managing and querying monitored applications
    /// </summary>
    public interface IMonitoredApplicationService
    {
        /// <summary>
        /// Get list of configured monitored applications
        /// </summary>
        List<MonitoredApplicationConfig> GetConfiguredApplications();

        /// <summary>
        /// Get health status for a specific application
        /// </summary>
        Task<MonitoredApplicationStatus> GetApplicationStatusAsync(string appName);

        /// <summary>
        /// Get health status for all configured applications
        /// </summary>
        Task<List<MonitoredApplicationStatus>> GetAllApplicationStatusesAsync();

        /// <summary>
        /// Get a specific application config by name
        /// </summary>
        MonitoredApplicationConfig GetApplicationByName(string appName);

        /// <summary>
        /// Make an authenticated HTTP GET request to a remote application.
        /// Will obtain a fresh token from the target server using cached credentials.
        /// </summary>
        /// <param name="appName">Name of the target application</param>
        /// <param name="relativePath">Relative path for the API endpoint</param>
        /// <param name="userObjectGuid">Object GUID of the current user (from sub claim)</param>
        Task<HttpResponseMessage> MakeAuthenticatedRequestAsync(string appName, string relativePath, string userObjectGuid);
    }


    public class MonitoredApplicationService : IMonitoredApplicationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MonitoredApplicationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICredentialCacheService _credentialCache;
        private readonly IMemoryCache _tokenCache;
        private readonly List<MonitoredApplicationConfig> _applications;

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(30);


        public MonitoredApplicationService(
            IConfiguration configuration,
            ILogger<MonitoredApplicationService> logger,
            IHttpClientFactory httpClientFactory,
            ICredentialCacheService credentialCache,
            IMemoryCache tokenCache)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _credentialCache = credentialCache;
            _tokenCache = tokenCache;

            //
            // Load configured applications from appsettings
            //
            _applications = new List<MonitoredApplicationConfig>();
            var appsSection = configuration.GetSection("MonitoredApplications");
            if (appsSection.Exists())
            {
                appsSection.Bind(_applications);
            }

            _logger.LogInformation("Loaded {Count} monitored applications", _applications.Count);
        }


        public List<MonitoredApplicationConfig> GetConfiguredApplications()
        {
            return _applications.ToList();
        }


        public async Task<MonitoredApplicationStatus> GetApplicationStatusAsync(string appName)
        {
            var app = _applications.FirstOrDefault(a =>
                a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));

            if (app == null)
            {
                return new MonitoredApplicationStatus
                {
                    Name = appName,
                    IsAvailable = false,
                    Status = "Not Found",
                    Error = $"Application '{appName}' is not configured"
                };
            }

            return await FetchHealthStatusAsync(app);
        }


        public async Task<List<MonitoredApplicationStatus>> GetAllApplicationStatusesAsync()
        {
            var tasks = _applications.Select(app => FetchHealthStatusAsync(app));
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }


        private async Task<MonitoredApplicationStatus> FetchHealthStatusAsync(MonitoredApplicationConfig app)
        {
            var result = new MonitoredApplicationStatus
            {
                Name = app.Name,
                Url = app.Url,
                IsSelf = app.IsSelf,
                CheckedAt = DateTime.UtcNow
            };

            try
            {
                //
                // For "self" applications, we could optionally call the local endpoint
                // but we'll still use HTTP for consistency
                //
                var client = _httpClientFactory.CreateClient("MonitoredApps");
                client.Timeout = DefaultTimeout;

                var url = $"{app.Url.TrimEnd('/')}/api/SystemHealth/status";
                _logger.LogDebug("Fetching health status from {Url}", url);

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    result.HealthData = JsonSerializer.Deserialize<JsonElement>(json);
                    result.IsAvailable = true;
                    result.Status = "Healthy";
                }
                else
                {
                    result.IsAvailable = false;
                    result.Status = "Error";
                    result.Error = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                }
            }
            catch (TaskCanceledException)
            {
                result.IsAvailable = false;
                result.Status = "Timeout";
                result.Error = "Request timed out";
                _logger.LogWarning("Timeout fetching health from {AppName} at {Url}", app.Name, app.Url);
            }
            catch (HttpRequestException ex)
            {
                result.IsAvailable = false;
                result.Status = "Unavailable";
                result.Error = ex.Message;
                _logger.LogWarning(ex, "Failed to fetch health from {AppName} at {Url}", app.Name, app.Url);
            }
            catch (Exception ex)
            {
                result.IsAvailable = false;
                result.Status = "Error";
                result.Error = ex.Message;
                _logger.LogError(ex, "Unexpected error fetching health from {AppName}", app.Name);
            }

            return result;
        }


        public MonitoredApplicationConfig GetApplicationByName(string appName)
        {
            return _applications.FirstOrDefault(a =>
                a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));
        }


        public async Task<HttpResponseMessage> MakeAuthenticatedRequestAsync(
            string appName, string relativePath, string userObjectGuid)
        {
            var app = GetApplicationByName(appName);

            if (app == null)
            {
                throw new ArgumentException($"Application '{appName}' is not configured");
            }

            //
            // Get or obtain a token for the target application
            //
            string token = await GetTokenForRemoteAppAsync(app, userObjectGuid);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Unable to obtain token for {AppName} - user may need to re-login", appName);
                // Return unauthorized response
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
                {
                    ReasonPhrase = "Unable to obtain token for remote application. Please re-login."
                };
            }

            var client = _httpClientFactory.CreateClient("MonitoredApps");
            client.Timeout = DefaultTimeout;

            var url = $"{app.Url.TrimEnd('/')}/{relativePath.TrimStart('/')}";
            _logger.LogDebug("Making authenticated request to {Url}", url);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return await client.SendAsync(request);
        }


        /// <summary>
        /// Gets a token for the remote application, either from cache or by requesting a new one
        /// </summary>
        private async Task<string> GetTokenForRemoteAppAsync(MonitoredApplicationConfig app, string userObjectGuid)
        {
            string cacheKey = $"RemoteToken_{app.Name}_{userObjectGuid}";

            //
            // Check if we have a cached token for this app+user
            //
            if (_tokenCache.TryGetValue(cacheKey, out string cachedToken))
            {
                _logger.LogDebug("Using cached token for {AppName}", app.Name);
                return cachedToken;
            }

            //
            // Get the user's cached credentials
            //
            var credentials = _credentialCache.GetCachedCredentials(userObjectGuid);
            if (credentials == null)
            {
                _logger.LogWarning("No cached credentials found for user {UserObjectGuid}", userObjectGuid);
                return null;
            }

            //
            // Request a token from the remote application's /connect/token endpoint
            //
            try
            {
                var client = _httpClientFactory.CreateClient("MonitoredApps");
                client.Timeout = DefaultTimeout;

                var tokenUrl = $"{app.Url.TrimEnd('/')}/connect/token";
                _logger.LogDebug("Requesting token from {TokenUrl} for user {Username}", tokenUrl, credentials.Username);

                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", credentials.Username),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("client_id", $"{app.Name.ToLowerInvariant()}_spa"),
                    new KeyValuePair<string, string>("scope", "openid profile email roles")
                });

                var response = await client.PostAsync(tokenUrl, tokenRequest);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

                    if (tokenResponse.TryGetProperty("access_token", out var accessTokenElement))
                    {
                        var token = accessTokenElement.GetString();

                        //
                        // Cache the token
                        //
                        _tokenCache.Set(cacheKey, token, TokenCacheDuration);
                        _logger.LogInformation("Obtained and cached token for {AppName}", app.Name);

                        return token;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to obtain token from {AppName}: {StatusCode} - {Error}", 
                        app.Name, response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining token from {AppName}", app.Name);
            }

            return null;
        }
    }
}
