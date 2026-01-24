//
// Monitored Application Service
//
// Provides functionality to fetch health status from remote Foundation-based applications.
// Handles HTTP calls with proper timeout and error handling.
//
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
        /// Make an authenticated HTTP GET request to a remote application
        /// </summary>
        Task<HttpResponseMessage> MakeAuthenticatedRequestAsync(string appName, string relativePath, string authToken);
    }


    public class MonitoredApplicationService : IMonitoredApplicationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MonitoredApplicationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<MonitoredApplicationConfig> _applications;

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);


        public MonitoredApplicationService(
            IConfiguration configuration,
            ILogger<MonitoredApplicationService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;

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
            string appName, string relativePath, string authToken)
        {
            var app = GetApplicationByName(appName);
            if (app == null)
            {
                throw new ArgumentException($"Application '{appName}' is not configured");
            }

            var client = _httpClientFactory.CreateClient("MonitoredApps");
            client.Timeout = DefaultTimeout;

            var url = $"{app.Url.TrimEnd('/')}/{relativePath.TrimStart('/')}";
            _logger.LogDebug("Making authenticated request to {Url}", url);

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            //
            // Forward the user's JWT token for authentication
            //
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }

            return await client.SendAsync(request);
        }
    }
}
