//
// SalesforceWebApiService.cs
//
// Handles OAuth2 password-grant authentication with Salesforce and provides
// a generic SOQL query method with automatic pagination.
//
// AI Assisted Development:  This file was ported from the Compactica SalesForceIntegration project
// and adapted for the Scheduler platform to conform to Compactica coding standards.
//

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scheduler.Salesforce.Auth;
using Scheduler.Salesforce.Models;


namespace Scheduler.Salesforce.Api
{
    public class SalesforceWebApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SalesforceWebApiService> _logger;
        private readonly ITokenCacheService _tokenCacheService;


        public SalesforceWebApiService(
            HttpClient httpClient,
            ILogger<SalesforceWebApiService> logger,
            ITokenCacheService tokenCacheService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenCacheService = tokenCacheService ?? throw new ArgumentNullException(nameof(tokenCacheService));
        }


        /// <summary>
        ///
        /// Authenticates with Salesforce using the OAuth2 username-password grant
        /// and caches the resulting token.
        ///
        /// </summary>
        public async Task<(string accessToken, string instanceUrl)> AuthenticateAsync(SalesforceConfig config, CancellationToken ct = default)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            try
            {
                Dictionary<string, string> formData = new Dictionary<string, string>
                {
                    {"grant_type", "password"},
                    {"client_id", config.ClientId},
                    {"client_secret", config.ClientSecret},
                    {"username", config.Username},
                    {"password", config.Password + config.SecurityToken}
                };

                FormUrlEncodedContent content = new FormUrlEncodedContent(formData);
                HttpResponseMessage response = await _httpClient.PostAsync(config.LoginUrl, content, ct);

                if (response.IsSuccessStatusCode == false)
                {
                    string errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Salesforce login response error: {ErrorContent}", errorContent);
                    response.EnsureSuccessStatusCode();
                }

                string responseString = await response.Content.ReadAsStringAsync(ct);
                JsonDocument tokenResponse = JsonDocument.Parse(responseString);

                string accessToken = tokenResponse.RootElement.GetProperty("access_token").GetString();
                string instanceUrl = tokenResponse.RootElement.GetProperty("instance_url").GetString();

                await _tokenCacheService.SetAccessTokenAsync(config.TenantGuid, accessToken, DateTime.UtcNow.AddHours(4));
                await _tokenCacheService.SetInstanceUrlAsync(config.TenantGuid, instanceUrl);

                _logger.LogInformation("Successfully obtained Salesforce auth token for tenant {TenantGuid}", config.TenantGuid);

                return (accessToken, instanceUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to obtain Salesforce authorization token for tenant {TenantGuid}", config.TenantGuid);
                throw;
            }
        }


        /// <summary>
        ///
        /// Gets a valid access token for the given tenant, re-authenticating if necessary.
        ///
        /// </summary>
        public async Task<(string accessToken, string instanceUrl)> GetValidTokenAsync(SalesforceConfig config, CancellationToken ct = default)
        {
            string cachedToken = await _tokenCacheService.GetAccessTokenAsync(config.TenantGuid);
            string cachedInstanceUrl = await _tokenCacheService.GetInstanceUrlAsync(config.TenantGuid);

            if (string.IsNullOrEmpty(cachedToken) == false && string.IsNullOrEmpty(cachedInstanceUrl) == false)
            {
                return (cachedToken, cachedInstanceUrl);
            }

            return await AuthenticateAsync(config, ct);
        }


        /// <summary>
        ///
        /// Executes a SOQL query against Salesforce with automatic pagination.
        /// Returns the raw JSON records array as a list of JsonElement objects.
        ///
        /// </summary>
        public async Task<List<JsonElement>> QueryRecordsAsync(string accessToken, string instanceUrl, string soqlQuery, string apiVersion = "v56.0", CancellationToken ct = default)
        {
            List<JsonElement> recordsToReturn = new List<JsonElement>();
            string codeContext = "QueryRecords";

            try
            {
                string restQuery = $"{instanceUrl}/services/data/{apiVersion}/query?q={Uri.EscapeDataString(soqlQuery.Trim())}";
                bool isDone = false;

                while (isDone == false)
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, restQuery);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await _httpClient.SendAsync(request, ct);

                    if (response.IsSuccessStatusCode == true)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync(ct);
                        JsonDocument result = JsonDocument.Parse(responseBody);

                        if (result.RootElement.TryGetProperty("records", out JsonElement records) == true &&
                            records.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement record in records.EnumerateArray())
                            {
                                recordsToReturn.Add(record.Clone());
                            }
                        }

                        if (result.RootElement.TryGetProperty("nextRecordsUrl", out JsonElement nextUrl) == true &&
                            nextUrl.ValueKind != JsonValueKind.Null)
                        {
                            restQuery = $"{instanceUrl}{nextUrl.GetString()}";
                            isDone = false;
                            _logger.LogInformation("{Context} - Next records URL: {NextUrl}", codeContext, nextUrl.GetString());
                        }
                        else
                        {
                            isDone = true;
                        }
                    }
                    else
                    {
                        _logger.LogError("{Context} - Query Failure - StatusCode: {StatusCode} - Reason: {Reason}", codeContext, (int)response.StatusCode, response.ReasonPhrase);
                        isDone = true;
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "{Context} - Exception occurred during query", codeContext);
            }

            _logger.LogInformation("{Context} - EXIT - records returned: {Count}", codeContext, recordsToReturn.Count);
            return recordsToReturn;
        }
    }
}
