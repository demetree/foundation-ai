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
    /// Configuration for the service account used for inter-application authentication
    /// when user credentials are not available (e.g., after server restart).
    /// Configured in appsettings.json under "ServiceAccount" section.
    /// </summary>
    public class ServiceAccountConfig
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
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
        /// Get health status for a specific application (unauthenticated)
        /// </summary>
        Task<MonitoredApplicationStatus> GetApplicationStatusAsync(string appName);

        /// <summary>
        /// Get health status for a specific application with authentication
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="userObjectGuid">Object GUID of current user for authentication</param>
        Task<MonitoredApplicationStatus> GetApplicationStatusAsync(string appName, string userObjectGuid);

        /// <summary>
        /// Get health status for all configured applications (unauthenticated)
        /// </summary>
        Task<List<MonitoredApplicationStatus>> GetAllApplicationStatusesAsync();

        /// <summary>
        /// Get health status for all configured applications with authentication
        /// </summary>
        /// <param name="userObjectGuid">Object GUID of current user for authentication</param>
        Task<List<MonitoredApplicationStatus>> GetAllApplicationStatusesAsync(string userObjectGuid);

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

        /// <summary>
        /// Get aggregated application metrics from all monitored applications
        /// </summary>
        /// <param name="userObjectGuid">Object GUID of current user for authentication</param>
        Task<ApplicationMetricsResponse> GetAllApplicationMetricsAsync(string userObjectGuid);
    }


    public class MonitoredApplicationService : IMonitoredApplicationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MonitoredApplicationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICredentialCacheService _credentialCache;
        private readonly IMemoryCache _tokenCache;
        private readonly List<MonitoredApplicationConfig> _applications;
        private readonly ServiceAccountConfig _serviceAccount;

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(30);
        private const string SERVICE_ACCOUNT_MARKER = "__SERVICE_ACCOUNT__";


        public MonitoredApplicationService(IConfiguration configuration,
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

            IConfigurationSection appsSection = configuration.GetSection("MonitoredApplications");

            if (appsSection.Exists())
            {
                appsSection.Bind(_applications);
            }

            //
            // Load service account configuration for fallback authentication
            //
            _serviceAccount = new ServiceAccountConfig();

            IConfigurationSection serviceAccountSection = configuration.GetSection("ServiceAccount");

            if (serviceAccountSection.Exists())
            {
                serviceAccountSection.Bind(_serviceAccount);
            }

            _logger.LogInformation("Loaded {Count} monitored applications, ServiceAccount fallback: {Enabled}", 
                                   _applications.Count, 
                                   !string.IsNullOrEmpty(_serviceAccount.Username) && _serviceAccount.Enabled);
        }


        public List<MonitoredApplicationConfig> GetConfiguredApplications()
        {
            return _applications.ToList();
        }


        public async Task<MonitoredApplicationStatus> GetApplicationStatusAsync(string appName)
        {
            MonitoredApplicationConfig app = _applications.FirstOrDefault(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));

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

            return await FetchHealthStatusAsync(app, null);
        }


        public async Task<MonitoredApplicationStatus> GetApplicationStatusAsync(string appName, string userObjectGuid)
        {
            MonitoredApplicationConfig app = _applications.FirstOrDefault(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));

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

            return await FetchHealthStatusAsync(app, userObjectGuid);
        }


        public async Task<List<MonitoredApplicationStatus>> GetAllApplicationStatusesAsync()
        {
            // Use service account marker for background/automated calls
            var tasks = _applications.Select(app => FetchHealthStatusAsync(app, SERVICE_ACCOUNT_MARKER));

            MonitoredApplicationStatus[] results = await Task.WhenAll(tasks);

            return results.ToList();
        }


        public async Task<List<MonitoredApplicationStatus>> GetAllApplicationStatusesAsync(string userObjectGuid)
        {
            var tasks = _applications.Select(app => FetchHealthStatusAsync(app, userObjectGuid));

            MonitoredApplicationStatus[] results = await Task.WhenAll(tasks);

            return results.ToList();
        }


        private async Task<MonitoredApplicationStatus> FetchHealthStatusAsync(MonitoredApplicationConfig app, string userObjectGuid)
        {
            MonitoredApplicationStatus result = new MonitoredApplicationStatus
            {
                Name = app.Name,
                Url = app.Url,
                IsSelf = app.IsSelf,
                CheckedAt = DateTime.UtcNow
            };

            try
            {
                HttpClient client = _httpClientFactory.CreateClient("MonitoredApps");
                client.Timeout = DefaultTimeout;

                string url = $"{app.Url.TrimEnd('/')}/api/SystemHealth/status";
                _logger.LogDebug("Fetching health status from {Url}", url);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                //
                // Add authentication if userObjectGuid is provided
                //
                if (!string.IsNullOrEmpty(userObjectGuid))
                {
                    string token = await GetTokenForRemoteAppAsync(app, userObjectGuid);

                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }
                }

                HttpResponseMessage response = await client.SendAsync(request);

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
            return _applications.FirstOrDefault(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));
        }


        public async Task<HttpResponseMessage> MakeAuthenticatedRequestAsync(string appName, string relativePath, string userObjectGuid)
        {
            MonitoredApplicationConfig app = GetApplicationByName(appName);

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

                //
                // Return unauthorized response
                //
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
                {
                    ReasonPhrase = "Unable to obtain token for remote application. Please re-login."
                };
            }

            HttpClient client = _httpClientFactory.CreateClient("MonitoredApps");
            client.Timeout = DefaultTimeout;

            string url = $"{app.Url.TrimEnd('/')}/{relativePath.TrimStart('/')}";
            _logger.LogDebug("Making authenticated request to {Url}", url);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.SendAsync(request);

            //
            // If we got a 401 Unauthorized, the token may have expired on the remote server.
            // Invalidate our cached token, get a fresh one, and retry once.
            //
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Token expired for {AppName}, invalidating cache and refreshing", appName);

                InvalidateCachedToken(app, userObjectGuid);

                //
                // Get a fresh token
                //
                string freshToken = await GetTokenForRemoteAppAsync(app, userObjectGuid);

                if (string.IsNullOrEmpty(freshToken))
                {
                    _logger.LogWarning("Unable to refresh token for {AppName} after 401", appName);
                    return response;
                }

                _logger.LogInformation("Successfully refreshed token for {AppName}, retrying request", appName);

                //
                // Create a new request (HttpRequestMessage cannot be reused after sending)
                //
                HttpRequestMessage retryRequest = new HttpRequestMessage(HttpMethod.Get, url);
                retryRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", freshToken);

                response = await client.SendAsync(retryRequest);
            }

            return response;
        }


        /// <summary>
        /// Invalidates any cached tokens for the specified app and user.
        /// Called when a 401 response indicates the token has expired.
        /// </summary>
        private void InvalidateCachedToken(MonitoredApplicationConfig app, string userObjectGuid)
        {
            bool useServiceAccountDirectly = userObjectGuid == SERVICE_ACCOUNT_MARKER;

            //
            // Remove user-specific token from cache
            //
            string userCacheKey = useServiceAccountDirectly
                ? $"RemoteToken_{app.Name}_ServiceAccount"
                : $"RemoteToken_{app.Name}_{userObjectGuid}";

            _tokenCache.Remove(userCacheKey);
            _logger.LogDebug("Invalidated cached token for {AppName} (key: {CacheKey})", app.Name, userCacheKey);

            //
            // Also remove service account token if it exists (it may have been used as fallback)
            //
            if (!useServiceAccountDirectly)
            {
                string serviceAccountCacheKey = $"RemoteToken_{app.Name}_ServiceAccount";
                _tokenCache.Remove(serviceAccountCacheKey);
            }
        }


        /// <summary>
        /// Fetches and aggregates application metrics from all monitored applications
        /// </summary>
        public async Task<ApplicationMetricsResponse> GetAllApplicationMetricsAsync(string userObjectGuid)
        {
            var response = new ApplicationMetricsResponse();

            foreach (MonitoredApplicationConfig app in _applications)
            {
                try
                {
                    HttpResponseMessage httpResponse = await MakeAuthenticatedRequestAsync(app.Name, "api/SystemHealth/metrics", userObjectGuid);

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        string json = await httpResponse.Content.ReadAsStringAsync();
                        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                        
                        ApplicationMetricsResponse appMetrics = JsonSerializer.Deserialize<ApplicationMetricsResponse>(json, jsonOptions);

                        if (appMetrics?.Applications != null)
                        {
                            response.Applications.AddRange(appMetrics.Applications);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to get metrics from {AppName}: {StatusCode}", app.Name, httpResponse.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching metrics from {AppName}", app.Name);
                }
            }

            return response;
        }


        /// <summary>
        /// Gets a token for the remote application, either from cache or by requesting a new one.
        /// Tries user credentials first, falls back to service account if unavailable.
        /// </summary>
        private async Task<string> GetTokenForRemoteAppAsync(MonitoredApplicationConfig app, string userObjectGuid)
        {
            //
            // If SERVICE_ACCOUNT_MARKER is used, skip user credential lookup and go directly to service account
            //
            bool useServiceAccountDirectly = userObjectGuid == SERVICE_ACCOUNT_MARKER;
            
            string cacheKey = useServiceAccountDirectly 
                ? $"RemoteToken_{app.Name}_ServiceAccount" 
                : $"RemoteToken_{app.Name}_{userObjectGuid}";

            //
            // Check if we have a cached token for this app+user
            //
            if (_tokenCache.TryGetValue(cacheKey, out string cachedToken))
            {
                _logger.LogDebug("Using cached token for {AppName}", app.Name);
                return cachedToken;
            }

            //
            // If not using service account marker, try to get user's cached credentials first
            //
            if (!useServiceAccountDirectly)
            {
                CachedCredentials credentials = _credentialCache.GetCachedCredentials(userObjectGuid);
                
                if (credentials != null)
                {
                    //
                    // Try with user credentials
                    //
                    string token = await RequestTokenAsync(app, credentials.Username, credentials.Password, cacheKey);

                    if (!string.IsNullOrEmpty(token))
                    {
                        return token;
                    }

                    _logger.LogWarning("Failed to get token using user credentials for {UserObjectGuid}, trying service account", userObjectGuid);
                }
                else
                {
                    _logger.LogDebug("No cached credentials found for user {UserObjectGuid}, trying service account", userObjectGuid);
                }
            }

            //
            // Fallback to service account if configured
            //
            if (_serviceAccount.Enabled && 
                !string.IsNullOrEmpty(_serviceAccount.Username) && 
                !string.IsNullOrEmpty(_serviceAccount.Password))
            {
                string serviceAccountCacheKey = $"RemoteToken_{app.Name}_ServiceAccount";
                
                //
                // Check if we have a cached service account token
                //
                if (_tokenCache.TryGetValue(serviceAccountCacheKey, out string cachedServiceToken))
                {
                    _logger.LogDebug("Using cached service account token for {AppName}", app.Name);
                    return cachedServiceToken;
                }

                _logger.LogInformation("Falling back to service account for {AppName} authentication", app.Name);
                
                string token = await RequestTokenAsync(app, _serviceAccount.Username, _serviceAccount.Password, serviceAccountCacheKey);

                if (!string.IsNullOrEmpty(token))
                {
                    return token;
                }

                _logger.LogWarning("Service account authentication also failed for {AppName}", app.Name);
            }
            else
            {
                _logger.LogWarning("No service account configured and user credentials unavailable - cannot authenticate to {AppName}", app.Name);
            }

            return null;
        }


        /// <summary>
        /// 
        /// Requests a token from a remote application's OAuth endpoint
        /// 
        /// </summary>
        private async Task<string> RequestTokenAsync(MonitoredApplicationConfig app, string username, string password, string cacheKey)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient("MonitoredApps");
                client.Timeout = DefaultTimeout;

                string tokenUrl = $"{app.Url.TrimEnd('/')}/connect/token";
                _logger.LogDebug("Requesting token from {TokenUrl} for user {Username}", tokenUrl, username);

                FormUrlEncodedContent tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("client_id", $"{app.Name.ToLowerInvariant()}_spa"),
                    new KeyValuePair<string, string>("scope", "openid profile email roles")
                });

                HttpResponseMessage response = await client.PostAsync(tokenUrl, tokenRequest);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JsonElement tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

                    if (tokenResponse.TryGetProperty("access_token", out var accessTokenElement))
                    {
                        string token = accessTokenElement.GetString();

                        //
                        // Cache the token
                        //
                        _tokenCache.Set(cacheKey, token, TokenCacheDuration);
                        _logger.LogInformation("Obtained and cached token for {AppName} (user: {Username})", app.Name, username);

                        return token;
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();

                    _logger.LogWarning("Failed to obtain token from {AppName} for {Username}: {StatusCode} - {Error}", 
                        app.Name, username, response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining token from {AppName} for {Username}", app.Name, username);
            }

            return null;
        }
    }
}
