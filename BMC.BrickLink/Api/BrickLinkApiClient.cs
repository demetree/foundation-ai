using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BMC.BrickLink.Api.Models.Responses;


namespace BMC.BrickLink.Api
{
    /// <summary>
    ///
    /// Complete strongly-typed client for the BrickLink Store API v1.
    ///
    /// Handles OAuth 1.0 authentication, throttling, and serialisation.
    /// This is a pure HTTP-level service with no database dependencies — higher-level
    /// consumers compose it with EF/database access as needed.
    ///
    /// Key characteristics:
    ///   - Uses OAuth 1.0 HMAC-SHA1 signatures on every request (via OAuthHelper)
    ///   - Requires 4 credentials: consumer key/secret (app-level) + token key/secret (per-user)
    ///   - Base URL: https://api.bricklink.com/api/store/v1/
    ///   - Responses use a { "meta": {...}, "data": {...} } envelope
    ///   - Conservative throttling with exponential backoff on 429
    ///
    /// Usage:
    ///     var client = new BrickLinkApiClient(
    ///         "consumer-key", "consumer-secret",
    ///         "token-value", "token-secret",
    ///         msg => Console.WriteLine(msg));
    ///     var item = await client.GetItemAsync("SET", "75192-1");
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    ///
    /// </summary>
    public class BrickLinkApiClient : IDisposable
    {
        private const string BaseUrl = "https://api.bricklink.com/api/store/v1";
        private const int ThrottleDelayMs = 500;

        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _tokenValue;
        private readonly string _tokenSecret;
        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;
        private readonly Action<string> _log;

        private readonly JsonSerializerOptions _jsonOptions;


        /// <summary>
        /// Creates a new BrickLinkApiClient.
        /// </summary>
        /// <param name="consumerKey">App-level consumer key (from appsettings.json).</param>
        /// <param name="consumerSecret">App-level consumer secret (from appsettings.json).</param>
        /// <param name="tokenValue">Per-user token value (stored encrypted in DB).</param>
        /// <param name="tokenSecret">Per-user token secret (stored encrypted in DB).</param>
        /// <param name="log">Optional logging callback.</param>
        /// <param name="httpClient">Optional HttpClient for dependency injection. If null, a new one is created.</param>
        public BrickLinkApiClient(string consumerKey,
                                  string consumerSecret,
                                  string tokenValue,
                                  string tokenSecret,
                                  Action<string> log = null,
                                  HttpClient httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(consumerKey))
                throw new ArgumentException("Consumer key is required.", nameof(consumerKey));

            if (string.IsNullOrWhiteSpace(consumerSecret))
                throw new ArgumentException("Consumer secret is required.", nameof(consumerSecret));

            if (string.IsNullOrWhiteSpace(tokenValue))
                throw new ArgumentException("Token value is required.", nameof(tokenValue));

            if (string.IsNullOrWhiteSpace(tokenSecret))
                throw new ArgumentException("Token secret is required.", nameof(tokenSecret));

            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _tokenValue = tokenValue;
            _tokenSecret = tokenSecret;
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

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BMC-BrickLink-Api/1.0");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }


        #region Catalog — Items

        /// <summary>Get details about a specific catalog item.</summary>
        /// <param name="type">Item type: MINIFIG, PART, SET, BOOK, GEAR, CATALOG, INSTRUCTION, UNSORTED_LOT, ORIGINAL_BOX.</param>
        /// <param name="number">Item number (e.g. "75192-1" for a set, "3001" for a part).</param>
        public async Task<BrickLinkItem> GetItemAsync(string type, string number)
        {
            string url = $"{BaseUrl}/items/{type}/{number}";

            var response = await GetAsync<BrickLinkResponse<BrickLinkItem>>(url);

            return response.Data;
        }


        /// <summary>Get the image URL for an item in a specific colour.</summary>
        /// <param name="type">Item type.</param>
        /// <param name="number">Item number.</param>
        /// <param name="colorId">BrickLink colour ID.</param>
        public async Task<BrickLinkItem> GetItemImageAsync(string type, string number, int colorId)
        {
            string url = $"{BaseUrl}/items/{type}/{number}/images/{colorId}";

            var response = await GetAsync<BrickLinkResponse<BrickLinkItem>>(url);

            return response.Data;
        }

        #endregion


        #region Catalog — Supersets & Subsets

        /// <summary>Get a list of items that contain the specified item (e.g. find sets that contain a part).</summary>
        /// <param name="type">Item type.</param>
        /// <param name="number">Item number.</param>
        /// <param name="colorId">Optional colour ID to filter by.</param>
        public async Task<List<BrickLinkSupersetEntry>> GetSuperSetsAsync(string type, string number, int? colorId = null)
        {
            string url = $"{BaseUrl}/items/{type}/{number}/supersets";

            if (colorId.HasValue)
            {
                url += $"?color_id={colorId.Value}";
            }

            var response = await GetAsync<BrickLinkListResponse<BrickLinkSupersetEntry>>(url);

            return response.Data ?? new List<BrickLinkSupersetEntry>();
        }


        /// <summary>
        /// Get a list of items contained within the specified item (part-out a set).
        /// Also known as "Get Subsets" in the BrickLink API.
        /// </summary>
        /// <param name="type">Item type.</param>
        /// <param name="number">Item number.</param>
        /// <param name="colorId">Optional colour ID to filter by.</param>
        /// <param name="breakMinifigs">If true, break down minifigs into their component parts.</param>
        /// <param name="breakSubsets">If true, break down sub-sets into their component parts.</param>
        public async Task<List<BrickLinkSubsetEntry>> GetSubSetsAsync(string type, string number,
                                                                      int? colorId = null,
                                                                      bool? breakMinifigs = null,
                                                                      bool? breakSubsets = null)
        {
            string url = $"{BaseUrl}/items/{type}/{number}/subsets";

            var queryParams = new List<string>();

            if (colorId.HasValue)
            {
                queryParams.Add($"color_id={colorId.Value}");
            }

            if (breakMinifigs.HasValue)
            {
                queryParams.Add($"break_minifigs={breakMinifigs.Value.ToString().ToLowerInvariant()}");
            }

            if (breakSubsets.HasValue)
            {
                queryParams.Add($"break_subsets={breakSubsets.Value.ToString().ToLowerInvariant()}");
            }

            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }

            var response = await GetAsync<BrickLinkListResponse<BrickLinkSubsetEntry>>(url);

            return response.Data ?? new List<BrickLinkSubsetEntry>();
        }

        #endregion


        #region Catalog — Price Guide

        /// <summary>
        /// Get the price guide for a specific item.
        /// 
        /// This is the most valuable endpoint — returns real secondary market pricing
        /// with min/max/average prices and individual data points.
        /// </summary>
        /// <param name="type">Item type.</param>
        /// <param name="number">Item number.</param>
        /// <param name="colorId">Optional colour ID. If omitted, returns aggregate pricing across all colours.</param>
        /// <param name="guideType">
        /// "stock" for currently in-stock prices, "sold" for last 6 months sales.
        /// If omitted, defaults to "stock".
        /// </param>
        /// <param name="newOrUsed">
        /// "N" for new, "U" for used.
        /// If omitted, defaults to "N".
        /// </param>
        /// <param name="countryCode">Optional ISO country code to filter sellers by country.</param>
        /// <param name="region">
        /// Optional region filter: "africa", "asia", "eu", "europe", "middle_east",
        /// "north_america", "oceania", "south_america".
        /// </param>
        /// <param name="currencyCode">Optional ISO 4217 currency code (e.g. "USD", "EUR", "GBP").</param>
        public async Task<BrickLinkPriceGuide> GetPriceGuideAsync(string type, string number,
                                                                   int? colorId = null,
                                                                   string guideType = null,
                                                                   string newOrUsed = null,
                                                                   string countryCode = null,
                                                                   string region = null,
                                                                   string currencyCode = null)
        {
            string url = $"{BaseUrl}/items/{type}/{number}/price";

            var queryParams = new List<string>();

            if (colorId.HasValue)
            {
                queryParams.Add($"color_id={colorId.Value}");
            }

            if (!string.IsNullOrEmpty(guideType))
            {
                queryParams.Add($"guide_type={Uri.EscapeDataString(guideType)}");
            }

            if (!string.IsNullOrEmpty(newOrUsed))
            {
                queryParams.Add($"new_or_used={Uri.EscapeDataString(newOrUsed)}");
            }

            if (!string.IsNullOrEmpty(countryCode))
            {
                queryParams.Add($"country_code={Uri.EscapeDataString(countryCode)}");
            }

            if (!string.IsNullOrEmpty(region))
            {
                queryParams.Add($"region={Uri.EscapeDataString(region)}");
            }

            if (!string.IsNullOrEmpty(currencyCode))
            {
                queryParams.Add($"currency_code={Uri.EscapeDataString(currencyCode)}");
            }

            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }

            var response = await GetAsync<BrickLinkResponse<BrickLinkPriceGuide>>(url);

            return response.Data;
        }

        #endregion


        #region Catalog — Known Colours

        /// <summary>Get all colours that a specific item is known to exist in.</summary>
        /// <param name="type">Item type.</param>
        /// <param name="number">Item number.</param>
        public async Task<List<BrickLinkKnownColor>> GetKnownColorsAsync(string type, string number)
        {
            string url = $"{BaseUrl}/items/{type}/{number}/colors";

            var response = await GetAsync<BrickLinkListResponse<BrickLinkKnownColor>>(url);

            return response.Data ?? new List<BrickLinkKnownColor>();
        }

        #endregion


        #region Reference Data — Colours

        /// <summary>Get a list of all colours defined in the BrickLink catalog.</summary>
        public async Task<List<BrickLinkColor>> GetColorListAsync()
        {
            string url = $"{BaseUrl}/colors";

            var response = await GetAsync<BrickLinkListResponse<BrickLinkColor>>(url);

            return response.Data ?? new List<BrickLinkColor>();
        }


        /// <summary>Get details about a specific colour.</summary>
        /// <param name="colorId">BrickLink colour ID.</param>
        public async Task<BrickLinkColor> GetColorAsync(int colorId)
        {
            string url = $"{BaseUrl}/colors/{colorId}";

            var response = await GetAsync<BrickLinkResponse<BrickLinkColor>>(url);

            return response.Data;
        }

        #endregion


        #region Reference Data — Categories

        /// <summary>Get a list of all part categories.</summary>
        public async Task<List<BrickLinkCategory>> GetCategoryListAsync()
        {
            string url = $"{BaseUrl}/categories";

            var response = await GetAsync<BrickLinkListResponse<BrickLinkCategory>>(url);

            return response.Data ?? new List<BrickLinkCategory>();
        }


        /// <summary>Get details about a specific category.</summary>
        /// <param name="categoryId">BrickLink category ID.</param>
        public async Task<BrickLinkCategory> GetCategoryAsync(int categoryId)
        {
            string url = $"{BaseUrl}/categories/{categoryId}";

            var response = await GetAsync<BrickLinkResponse<BrickLinkCategory>>(url);

            return response.Data;
        }

        #endregion


        #region Catalog — Item Mapping

        /// <summary>
        /// Get Element ID mappings for a specific item.
        /// Maps between BrickLink catalog item numbers and LEGO Element IDs.
        /// </summary>
        /// <param name="elementId">LEGO Element ID to look up.</param>
        public async Task<List<BrickLinkItemMapping>> GetItemMappingAsync(string elementId)
        {
            string url = $"{BaseUrl}/item_mapping/{elementId}";

            var response = await GetAsync<BrickLinkListResponse<BrickLinkItemMapping>>(url);

            return response.Data ?? new List<BrickLinkItemMapping>();
        }

        #endregion


        #region Internal HTTP Helpers

        /// <summary>
        /// Performs a GET request to a BrickLink API endpoint with OAuth 1.0 authentication.
        ///
        /// All requests include:
        ///   - OAuth 1.0 Authorization header (HMAC-SHA1 signature)
        ///   - Conservative throttle delay (500ms between requests)
        ///   - Exponential backoff on 429 rate limit responses
        ///
        /// The URL passed to this method may include query parameters — the OAuth signature
        /// is computed against the base URL only (without query string), as per OAuth spec.
        /// </summary>
        private async Task<T> GetAsync<T>(string fullUrl)
        {
            await Task.Delay(ThrottleDelayMs);

            //
            // Split the URL into base URL and query string for OAuth signing
            // OAuth 1.0 signatures are computed on the base URL only
            //
            string baseUrlForSigning = fullUrl;
            int queryStart = fullUrl.IndexOf('?');

            if (queryStart >= 0)
            {
                baseUrlForSigning = fullUrl.Substring(0, queryStart);
            }

            //
            // Generate the OAuth Authorization header
            //
            string authHeader = OAuthHelper.GenerateAuthorizationHeader(
                "GET",
                baseUrlForSigning,
                _consumerKey,
                _consumerSecret,
                _tokenValue,
                _tokenSecret);

            _log($"  → GET {baseUrlForSigning.Replace(BaseUrl, "")}");

            int retries = 0;

            while (true)
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, fullUrl))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", authHeader);

                    using (HttpResponseMessage response = await _httpClient.SendAsync(request))
                    {
                        string body = await response.Content.ReadAsStringAsync();

                        //
                        // HTTP-level errors
                        //
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == (HttpStatusCode)429 && retries < 3)
                            {
                                int waitSeconds = (int)Math.Pow(2, retries + 1);
                                _log($"  ⏳ Rate limited — waiting {waitSeconds}s before retry...");
                                await Task.Delay(waitSeconds * 1000);
                                retries++;

                                //
                                // Re-generate the auth header with a new nonce/timestamp for the retry
                                //
                                authHeader = OAuthHelper.GenerateAuthorizationHeader(
                                    "GET",
                                    baseUrlForSigning,
                                    _consumerKey,
                                    _consumerSecret,
                                    _tokenValue,
                                    _tokenSecret);

                                continue;
                            }

                            throw new BrickLinkApiException(
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
        }

        #endregion


        public void Dispose()
        {
            if (_ownsHttpClient)
                _httpClient?.Dispose();
        }
    }
}
