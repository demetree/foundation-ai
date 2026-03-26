//
// SalesforceClient.cs
//
// Provides low-level HTTP methods for querying and mutating Salesforce objects
// via the REST API using SOQL and sObject endpoints.
//
// Supports: Accounts, Contacts, and Events.
//
// AI Assisted Development:  This file was ported from the Compactica SalesForceIntegration project
// and adapted for the Scheduler platform to conform to Compactica coding standards.
//

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace Scheduler.Salesforce.Api
{
    public class SalesforceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SalesforceClient> _logger;


        public SalesforceClient(HttpClient httpClient, ILogger<SalesforceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        #region Query Methods


        /// <summary>
        ///
        /// Retrieves Salesforce Account records.
        /// Returns the raw JSON response from the Salesforce REST API.
        ///
        /// </summary>
        public async Task<string> GetAccountsAsync(string accessToken, string instanceUrl, string apiVersion = "v56.0", DateTime? modifiedSince = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(accessToken) == true || string.IsNullOrWhiteSpace(instanceUrl) == true)
            {
                throw new ArgumentException("Access token and instance URL must be provided.");
            }

            string soqlQuery = "SELECT Id, Name, Website, Phone, Type, Industry, BillingStreet, BillingCity, BillingState, BillingPostalCode, BillingCountry, IsDeleted FROM Account";

            if (modifiedSince.HasValue == true)
            {
                string sinceUtc = modifiedSince.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                soqlQuery += $" WHERE LastModifiedDate > {sinceUtc}";
            }

            string requestUrl = $"{instanceUrl}/services/data/{apiVersion}/query/?q={Uri.EscapeDataString(soqlQuery)}";

            return await ExecuteGetAsync(accessToken, requestUrl, "GetAccountsAsync", ct);
        }


        /// <summary>
        ///
        /// Retrieves Salesforce Contact records (including soft-deleted via queryAll).
        /// Returns the raw JSON response from the Salesforce REST API.
        ///
        /// </summary>
        public async Task<string> GetContactsAsync(string accessToken, string instanceUrl, string apiVersion = "v56.0", DateTime? modifiedSince = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(accessToken) == true || string.IsNullOrWhiteSpace(instanceUrl) == true)
            {
                throw new ArgumentException("Access token and instance URL must be provided.");
            }

            string soqlQuery = "SELECT Id, FirstName, LastName, Email, Phone, Title, IsDeleted, Account.Name, AccountId, MobilePhone FROM Contact";

            if (modifiedSince.HasValue == true)
            {
                string sinceUtc = modifiedSince.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                soqlQuery += $" WHERE LastModifiedDate > {sinceUtc}";
            }

            string requestUrl = $"{instanceUrl}/services/data/{apiVersion}/queryAll/?q={Uri.EscapeDataString(soqlQuery)}";

            return await ExecuteGetAsync(accessToken, requestUrl, "GetContactsAsync", ct);
        }


        /// <summary>
        ///
        /// Retrieves Salesforce Event records.
        /// Returns the raw JSON response from the Salesforce REST API.
        ///
        /// </summary>
        public async Task<string> GetEventsAsync(string accessToken, string instanceUrl, string apiVersion = "v56.0", DateTime? modifiedSince = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(accessToken) == true || string.IsNullOrWhiteSpace(instanceUrl) == true)
            {
                throw new ArgumentException("Access token and instance URL must be provided.");
            }

            string soqlQuery = "SELECT Id, Subject, Description, StartDateTime, EndDateTime, Location, WhatId, IsAllDayEvent, IsDeleted FROM Event";

            if (modifiedSince.HasValue == true)
            {
                string sinceUtc = modifiedSince.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                soqlQuery += $" WHERE LastModifiedDate > {sinceUtc}";
            }

            string requestUrl = $"{instanceUrl}/services/data/{apiVersion}/queryAll/?q={Uri.EscapeDataString(soqlQuery)}";

            return await ExecuteGetAsync(accessToken, requestUrl, "GetEventsAsync", ct);
        }


        #endregion


        #region sObject Mutation Methods


        /// <summary>
        ///
        /// Creates a new sObject record in Salesforce.
        /// Returns the Salesforce ID of the created record.
        ///
        /// </summary>
        public async Task<string> CreateSObjectAsync(string accessToken, string instanceUrl, string sObjectType, string jsonPayload, string apiVersion = "v56.0", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(accessToken) == true || string.IsNullOrWhiteSpace(instanceUrl) == true)
            {
                throw new ArgumentException("Access token and instance URL must be provided.");
            }

            string requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/{sObjectType}/";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request, ct);

                if (response.IsSuccessStatusCode == false)
                {
                    string errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Salesforce Create {SObjectType} failed. Status: {StatusCode}. Response: {Response}", sObjectType, response.StatusCode, errorContent);
                    throw new Exception($"Salesforce Create {sObjectType} failed with status code {response.StatusCode}. Response: {errorContent}");
                }

                string content = await response.Content.ReadAsStringAsync(ct);
                return content;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Exception creating Salesforce {SObjectType}", sObjectType);
                throw;
            }
        }


        /// <summary>
        ///
        /// Updates an existing sObject record in Salesforce by its Salesforce ID.
        ///
        /// </summary>
        public async Task UpdateSObjectAsync(string accessToken, string instanceUrl, string sObjectType, string salesforceId, string jsonPayload, string apiVersion = "v56.0", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(accessToken) == true || string.IsNullOrWhiteSpace(instanceUrl) == true)
            {
                throw new ArgumentException("Access token and instance URL must be provided.");
            }

            string requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/{sObjectType}/{salesforceId}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request, ct);

                if (response.IsSuccessStatusCode == false)
                {
                    string errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Salesforce Update {SObjectType}/{Id} failed. Status: {StatusCode}. Response: {Response}", sObjectType, salesforceId, response.StatusCode, errorContent);
                    throw new Exception($"Salesforce Update {sObjectType}/{salesforceId} failed with status code {response.StatusCode}. Response: {errorContent}");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Exception updating Salesforce {SObjectType}/{Id}", sObjectType, salesforceId);
                throw;
            }
        }


        /// <summary>
        ///
        /// Deletes an sObject record in Salesforce by its Salesforce ID.
        ///
        /// </summary>
        public async Task DeleteSObjectAsync(string accessToken, string instanceUrl, string sObjectType, string salesforceId, string apiVersion = "v56.0", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(accessToken) == true || string.IsNullOrWhiteSpace(instanceUrl) == true)
            {
                throw new ArgumentException("Access token and instance URL must be provided.");
            }

            string requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/{sObjectType}/{salesforceId}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request, ct);

                if (response.IsSuccessStatusCode == false)
                {
                    string errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Salesforce Delete {SObjectType}/{Id} failed. Status: {StatusCode}. Response: {Response}", sObjectType, salesforceId, response.StatusCode, errorContent);
                    throw new Exception($"Salesforce Delete {sObjectType}/{salesforceId} failed with status code {response.StatusCode}. Response: {errorContent}");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Exception deleting Salesforce {SObjectType}/{Id}", sObjectType, salesforceId);
                throw;
            }
        }


        #endregion


        #region Private Helpers


        private async Task<string> ExecuteGetAsync(string accessToken, string requestUrl, string codeContext, CancellationToken ct)
        {
            //
            // Build a per-request message to avoid mutating shared HttpClient default headers.
            //
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request, ct);

                if (response.IsSuccessStatusCode == false)
                {
                    string errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("{Context} - Salesforce API call failed with status code {StatusCode}. Response: {Response}", codeContext, response.StatusCode, errorContent);
                    throw new Exception($"Salesforce API call failed with status code {response.StatusCode}. Response: {errorContent}");
                }

                string content = await response.Content.ReadAsStringAsync(ct);
                return content;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "{Context} - Exception occurred while calling Salesforce API", codeContext);
                throw;
            }
        }


        #endregion
    }
}
