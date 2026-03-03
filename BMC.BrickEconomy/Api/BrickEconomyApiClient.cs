using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BMC.BrickEconomy.Api.Models.Responses;


namespace BMC.BrickEconomy.Api
{
    /// <summary>
    ///
    /// Complete strongly-typed client for the BrickEconomy API v1.
    ///
    /// Handles API key authentication, throttling, and serialisation.
    /// This is a pure HTTP-level service with no database dependencies — higher-level
    /// consumers compose it with EF/database access as needed.
    ///
    /// Key characteristics:
    ///   - Uses API key authentication via x-apikey header
    ///   - Base URL: https://www.brickeconomy.com/api/v1/
    ///   - Rate limit: 100 requests/day (Premium membership required)
    ///   - Quota resets daily at 00:00 UTC
    ///   - Responses use a { "data": {...} } envelope
    ///   - All endpoints support ?currency= for multi-currency support
    ///   - AI/ML-powered valuations with 89% accuracy on new sets
    ///
    /// Usage:
    ///     var client = new BrickEconomyApiClient("your-api-key", msg => Console.WriteLine(msg));
    ///     var set = await client.GetSetAsync("75192-1");
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    ///
    /// </summary>
    public class BrickEconomyApiClient : IDisposable
    {
        private const string BaseUrl = "https://www.brickeconomy.com/api/v1";
        private const int ThrottleDelayMs = 1000;

        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;
        private readonly Action<string> _log;

        private readonly JsonSerializerOptions _jsonOptions;


        /// <summary>
        /// Creates a new BrickEconomyApiClient.
        /// </summary>
        /// <param name="apiKey">BrickEconomy Premium API key (from user profile).</param>
        /// <param name="log">Optional logging callback.</param>
        /// <param name="httpClient">Optional HttpClient for dependency injection. If null, a new one is created.</param>
        public BrickEconomyApiClient(string apiKey, Action<string> log = null, HttpClient httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key is required.", nameof(apiKey));

            _apiKey = apiKey;
            _log = log ?? (_ => { });

            if (httpClient != null)
            {
                _httpClient = httpClient;
                _ownsHttpClient = false;
            }
            else
            {
                _httpClient = new HttpClient();
                _ownsHttpClient = true;
            }

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BMC-BrickEconomy-Api/1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("x-apikey", _apiKey);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }


        #region Set Valuation

        /// <summary>
        /// Get detailed valuation data for a specific LEGO set.
        ///
        /// Returns retail price, current market value, AI-forecast value,
        /// growth metrics, availability status, and price event history.
        /// </summary>
        /// <param name="setNumber">Set number (e.g. "75192-1").</param>
        /// <param name="currency">Optional ISO 4217 currency code (e.g. "USD", "EUR", "GBP", "CAD", "AUD").</param>
        public async Task<BrickEconomySet> GetSetAsync(string setNumber, string currency = null)
        {
            if (string.IsNullOrWhiteSpace(setNumber))
                throw new ArgumentException("Set number is required.", nameof(setNumber));

            string url = $"{BaseUrl}/set/{Uri.EscapeDataString(setNumber)}";

            if (!string.IsNullOrEmpty(currency))
            {
                url += $"?currency={Uri.EscapeDataString(currency)}";
            }

            var response = await GetAsync<BrickEconomyResponse<BrickEconomySet>>(url, "set");

            return response.Data;
        }

        #endregion


        #region Minifig Valuation

        /// <summary>
        /// Get valuation data for a specific LEGO minifigure.
        ///
        /// Returns current value, forecast value, growth metrics,
        /// and set appearances.
        /// </summary>
        /// <param name="minifigNumber">Minifig ID (e.g. "sw0001a").</param>
        /// <param name="currency">Optional ISO 4217 currency code.</param>
        public async Task<BrickEconomyMinifig> GetMinifigAsync(string minifigNumber, string currency = null)
        {
            if (string.IsNullOrWhiteSpace(minifigNumber))
                throw new ArgumentException("Minifig number is required.", nameof(minifigNumber));

            string url = $"{BaseUrl}/minifig/{Uri.EscapeDataString(minifigNumber)}";

            if (!string.IsNullOrEmpty(currency))
            {
                url += $"?currency={Uri.EscapeDataString(currency)}";
            }

            var response = await GetAsync<BrickEconomyResponse<BrickEconomyMinifig>>(url, "minifig");

            return response.Data;
        }

        #endregion


        #region Collection

        /// <summary>
        /// Get all sets in the user's BrickEconomy collection with current valuations.
        ///
        /// Returns purchase info, condition, and current market value for each set.
        /// </summary>
        /// <param name="currency">Optional ISO 4217 currency code.</param>
        public async Task<List<BrickEconomyCollectionSet>> GetCollectionSetsAsync(string currency = null)
        {
            string url = $"{BaseUrl}/collection/sets";

            if (!string.IsNullOrEmpty(currency))
            {
                url += $"?currency={Uri.EscapeDataString(currency)}";
            }

            var response = await GetAsync<BrickEconomyListResponse<BrickEconomyCollectionSet>>(url, "collection/sets");

            return response.Data ?? new List<BrickEconomyCollectionSet>();
        }


        /// <summary>
        /// Get all minifigs in the user's BrickEconomy collection with current valuations.
        /// </summary>
        /// <param name="currency">Optional ISO 4217 currency code.</param>
        public async Task<List<BrickEconomyCollectionMinifig>> GetCollectionMinifigsAsync(string currency = null)
        {
            string url = $"{BaseUrl}/collection/minifigs";

            if (!string.IsNullOrEmpty(currency))
            {
                url += $"?currency={Uri.EscapeDataString(currency)}";
            }

            var response = await GetAsync<BrickEconomyListResponse<BrickEconomyCollectionMinifig>>(url, "collection/minifigs");

            return response.Data ?? new List<BrickEconomyCollectionMinifig>();
        }

        #endregion


        #region Sales Ledger

        /// <summary>
        /// Get the user's sales ledger — transaction history for bought/sold sets and minifigs.
        /// </summary>
        public async Task<List<BrickEconomySalesLedgerEntry>> GetSalesLedgerAsync()
        {
            string url = $"{BaseUrl}/salesledger";

            var response = await GetAsync<BrickEconomyListResponse<BrickEconomySalesLedgerEntry>>(url, "salesledger");

            return response.Data ?? new List<BrickEconomySalesLedgerEntry>();
        }

        #endregion


        #region Internal HTTP Helpers

        /// <summary>
        /// Performs a GET request to a BrickEconomy API endpoint.
        ///
        /// All requests include:
        ///   - x-apikey header (set in constructor)
        ///   - Accept: application/json header
        ///   - Conservative throttle delay (1000ms between requests — budget is 100/day)
        ///   - Exponential backoff on 429 rate limit responses
        /// </summary>
        private async Task<T> GetAsync<T>(string url, string endpointName)
        {
            await Task.Delay(ThrottleDelayMs);

            _log($"  → GET /{endpointName}");

            int retries = 0;

            while (true)
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(url))
                {
                    string body = await response.Content.ReadAsStringAsync();

                    //
                    // HTTP-level errors
                    //
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == (HttpStatusCode)429 && retries < 3)
                        {
                            int waitSeconds = (int)Math.Pow(2, retries + 2);
                            _log($"  ⏳ Rate limited (daily quota) — waiting {waitSeconds}s before retry...");
                            await Task.Delay(waitSeconds * 1000);
                            retries++;
                            continue;
                        }

                        throw new BrickEconomyApiException(
                            response.StatusCode,
                            response.ReasonPhrase,
                            body);
                    }

                    //
                    // Deserialise the response
                    //
                    T result = JsonSerializer.Deserialize<T>(body, _jsonOptions);

                    return result;
                }
            }
        }

        #endregion


        public void Dispose()
        {
            if (_ownsHttpClient)
                _httpClient?.Dispose();
        }
    }
}
