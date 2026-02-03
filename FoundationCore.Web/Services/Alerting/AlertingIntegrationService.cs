//
// Alerting Integration Service Implementation
//
// HTTP client wrapper for communicating with the Alerting API.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Foundation.Web.Services.Alerting
{
    /// <summary>
    /// Implementation of <see cref="IAlertingIntegrationService"/> using HTTP client.
    /// </summary>
    public class AlertingIntegrationService : IAlertingIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly AlertingIntegrationOptions _options;
        private readonly ILogger<AlertingIntegrationService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AlertingIntegrationService(
            HttpClient httpClient,
            IOptions<AlertingIntegrationOptions> options,
            ILogger<AlertingIntegrationService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Configure base URL
            if (!string.IsNullOrEmpty(_options.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
            }

            // Configure timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        }

        public bool IsConfigured => 
            !string.IsNullOrEmpty(_options.BaseUrl) && 
            !string.IsNullOrEmpty(_options.ApiKey);


        public async Task<RegistrationResponse> RegisterAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_options.BaseUrl))
            {
                throw new InvalidOperationException("Alerting BaseUrl is not configured");
            }

            var request = new
            {
                ServiceName = _options.ServiceName,
                Description = $"Auto-registered {_options.ServiceName} integration",
                ServiceUrl = _options.ServiceUrl,
                CallbackUrl = _options.CallbackUrl
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/integrations/register");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to register with Alerting: {StatusCode} - {Content}",
                    response.StatusCode, content);
                throw new HttpRequestException($"Registration failed: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<RegistrationResponse>(content, _jsonOptions);

            // Store API key if path is configured
            if (!string.IsNullOrEmpty(_options.ApiKeyFilePath) && !string.IsNullOrEmpty(result?.ApiKey))
            {
                try
                {
                    var directory = Path.GetDirectoryName(_options.ApiKeyFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    await File.WriteAllTextAsync(_options.ApiKeyFilePath, result.ApiKey, cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation("Alerting API key stored to {Path}", _options.ApiKeyFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to store Alerting API key to file");
                }
            }

            _logger.LogInformation("Successfully registered with Alerting as service {ServiceName} (ID: {ServiceId})",
                result?.ServiceName, result?.ServiceId);

            return result;
        }


        public async Task<IncidentResponse> RaiseIncidentAsync(RaiseIncidentRequest request, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            var payload = new
            {
                severity = request.Severity,
                title = request.Title,
                description = request.Description,
                deduplicationKey = request.DeduplicationKey,
                source = request.Source ?? _options.ServiceName,
                detailsUrl = request.DetailsUrl,
                customFields = request.CustomFields
            };

            using var httpRequest = CreateApiRequest(HttpMethod.Post, "api/alerts");
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to raise incident: {StatusCode} - {Content}", response.StatusCode, content);
                throw new HttpRequestException($"Failed to raise incident: {response.StatusCode}");
            }

            return JsonSerializer.Deserialize<IncidentResponse>(content, _jsonOptions);
        }


        public async Task<IncidentStatusResponse> GetIncidentStatusAsync(string incidentKey, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            if (string.IsNullOrEmpty(incidentKey))
            {
                throw new ArgumentNullException(nameof(incidentKey));
            }

            using var httpRequest = CreateApiRequest(HttpMethod.Get, $"api/incidents/{Uri.EscapeDataString(incidentKey)}/status");

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get incident status: {StatusCode} - {Content}", response.StatusCode, content);
                throw new HttpRequestException($"Failed to get incident status: {response.StatusCode}");
            }

            return JsonSerializer.Deserialize<IncidentStatusResponse>(content, _jsonOptions);
        }


        public async Task<List<IncidentSummary>> GetMyIncidentsAsync(IncidentFilter filter = null, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            var queryParams = new List<string>();
            if (filter?.Since.HasValue == true)
                queryParams.Add($"since={filter.Since.Value:O}");
            if (filter?.Until.HasValue == true)
                queryParams.Add($"until={filter.Until.Value:O}");
            if (!string.IsNullOrEmpty(filter?.Status))
                queryParams.Add($"status={Uri.EscapeDataString(filter.Status)}");
            if (!string.IsNullOrEmpty(filter?.Severity))
                queryParams.Add($"severity={Uri.EscapeDataString(filter.Severity)}");
            if (filter?.Limit.HasValue == true)
                queryParams.Add($"limit={filter.Limit}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

            using var httpRequest = CreateApiRequest(HttpMethod.Get, $"api/incidents/mine{queryString}");

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get incidents: {StatusCode} - {Content}", response.StatusCode, content);
                throw new HttpRequestException($"Failed to get incidents: {response.StatusCode}");
            }

            return JsonSerializer.Deserialize<List<IncidentSummary>>(content, _jsonOptions);
        }


        public async Task<IncidentStatusResponse> ResolveIncidentAsync(string incidentKey, string resolution = null, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            if (string.IsNullOrEmpty(incidentKey))
            {
                throw new ArgumentNullException(nameof(incidentKey));
            }

            var payload = new { resolution };

            using var httpRequest = CreateApiRequest(HttpMethod.Post, $"api/incidents/{Uri.EscapeDataString(incidentKey)}/resolve");
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to resolve incident: {StatusCode} - {Content}", response.StatusCode, content);
                throw new HttpRequestException($"Failed to resolve incident: {response.StatusCode}");
            }

            return JsonSerializer.Deserialize<IncidentStatusResponse>(content, _jsonOptions);
        }


        #region Private Helpers

        private void EnsureConfigured()
        {
            if (string.IsNullOrEmpty(_options.BaseUrl))
            {
                throw new InvalidOperationException("Alerting BaseUrl is not configured");
            }

            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                throw new InvalidOperationException("Alerting ApiKey is not configured. Call RegisterAsync first.");
            }
        }

        private HttpRequestMessage CreateApiRequest(HttpMethod method, string path)
        {
            var request = new HttpRequestMessage(method, path);
            request.Headers.Add("X-Api-Key", _options.ApiKey);
            return request;
        }

        #endregion
    }
}
