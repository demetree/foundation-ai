using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BMC.BrickSet.Api.Models.Responses;


namespace BMC.BrickSet.Api
{
    /// <summary>
    ///
    /// Complete strongly-typed client for the BrickSet REST API v3.
    ///
    /// Handles authentication, throttling, and serialisation.
    /// This is a pure HTTP-level service with no database dependencies — higher-level
    /// consumers compose it with EF/database access as needed.
    ///
    /// Key differences from the Rebrickable client:
    ///   - BrickSet uses query/form parameters for auth (not Authorization header)
    ///   - BrickSet returns { "status": "success|error", "matches": N, "[data]": [...] }
    ///   - Rate limit is 100 calls/day on getSets (test key) — very conservative throttling
    ///   - User auth uses a session-based "userHash" from the login method
    ///
    /// Usage:
    ///     var client = new BrickSetApiClient("your-api-key", msg => Console.WriteLine(msg));
    ///     var themes = await client.GetThemesAsync();
    ///
    /// </summary>
    public class BrickSetApiClient : IDisposable
    {
        private const string BaseUrl = "https://brickset.com/api/v3.asmx";
        private const int ThrottleDelayMs = 500;

        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;
        private readonly Action<string> _log;

        private readonly JsonSerializerOptions _jsonOptions;


        /// <summary>
        /// Tracks today's API usage.  Updated after calls to GetKeyUsageStatsAsync.
        /// </summary>
        public int? TodayApiCallCount { get; private set; }


        /// <summary>
        /// Creates a new BrickSetApiClient.
        /// </summary>
        /// <param name="apiKey">BrickSet API key (app-level, from appsettings.json).</param>
        /// <param name="log">Optional logging callback.</param>
        /// <param name="httpClient">Optional HttpClient for dependency injection. If null, a new one is created.</param>
        public BrickSetApiClient(string apiKey, Action<string> log = null, HttpClient httpClient = null)
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

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BMC-BrickSet-Api/1.0");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }


        #region Utility — Key & Hash Validation

        /// <summary>Validate the API key.</summary>
        public async Task<bool> CheckKeyAsync()
        {
            var response = await GetAsync<BrickSetCheckKeyResponse>("checkKey",
                new Dictionary<string, string>());

            return response.IsSuccess;
        }


        /// <summary>Validate a user hash.</summary>
        public async Task<bool> CheckUserHashAsync(string userHash)
        {
            var response = await GetAsync<BrickSetCheckUserHashResponse>("checkUserHash",
                new Dictionary<string, string> { { "userHash", userHash } });

            return response.IsSuccess;
        }


        /// <summary>Get API key usage statistics for quota tracking.</summary>
        public async Task<BrickSetKeyUsageStatsResponse> GetKeyUsageStatsAsync()
        {
            var response = await GetAsync<BrickSetKeyUsageStatsResponse>("getKeyUsageStats",
                new Dictionary<string, string>());

            // Update the cached today count
            if (response.ApiKeyUsage != null && response.ApiKeyUsage.Count > 0)
            {
                TodayApiCallCount = response.ApiKeyUsage[response.ApiKeyUsage.Count - 1].Count;
            }

            return response;
        }

        #endregion


        #region Authentication

        /// <summary>
        /// Login to BrickSet and obtain a userHash for collection operations.
        /// The userHash is session-based and may expire — store it and be prepared to re-authenticate.
        /// </summary>
        public async Task<string> LoginAsync(string username, string password)
        {
            var parameters = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };

            var response = await GetAsync<BrickSetLoginResponse>("login", parameters);

            if (!response.IsSuccess || string.IsNullOrEmpty(response.Hash))
            {
                throw new BrickSetApiException(response.Message ?? "Login failed — no hash returned");
            }

            _log($"  ✓ Logged in to BrickSet as {username}");
            return response.Hash;
        }

        #endregion


        #region Reference Data — Themes

        /// <summary>Get all LEGO themes from BrickSet.</summary>
        public async Task<List<BrickSetTheme>> GetThemesAsync()
        {
            var response = await GetAsync<BrickSetThemesResponse>("getThemes",
                new Dictionary<string, string>());

            return response.Themes ?? new List<BrickSetTheme>();
        }


        /// <summary>Get subthemes for a specific theme.</summary>
        public async Task<List<BrickSetSubtheme>> GetSubthemesAsync(string theme)
        {
            var response = await GetAsync<BrickSetSubthemesResponse>("getSubthemes",
                new Dictionary<string, string> { { "theme", theme } });

            return response.Subthemes ?? new List<BrickSetSubtheme>();
        }


        /// <summary>Get years with sets for a specific theme.</summary>
        public async Task<List<BrickSetYear>> GetYearsAsync(string theme)
        {
            var response = await GetAsync<BrickSetYearsResponse>("getYears",
                new Dictionary<string, string> { { "theme", theme } });

            return response.Years ?? new List<BrickSetYear>();
        }

        #endregion


        #region Reference Data — Sets

        /// <summary>
        /// Get sets with optional filters.  This is the core method — returns pricing,
        /// ratings, dimensions, availability, and all the rich data.
        ///
        /// WARNING: Test API keys are limited to 100 calls/day on getSets.
        /// </summary>
        public async Task<BrickSetSetsResponse> GetSetsAsync(
            string userHash = null,
            string theme = null, string subtheme = null,
            string setNumber = null, int? year = null,
            string tag = null, string orderBy = null,
            int? pageSize = null, int? pageNumber = null,
            string extendedData = null)
        {
            var parameters = new Dictionary<string, string>();

            //
            // BrickSet's getSets uses a JSON "params" parameter for filters
            //
            var setParams = new Dictionary<string, object>();

            if (theme != null) setParams["theme"] = theme;
            if (subtheme != null) setParams["subtheme"] = subtheme;
            if (setNumber != null) setParams["setNumber"] = setNumber;
            if (year.HasValue) setParams["year"] = year.Value;
            if (tag != null) setParams["tag"] = tag;
            if (orderBy != null) setParams["orderBy"] = orderBy;
            if (pageSize.HasValue) setParams["pageSize"] = pageSize.Value;
            if (pageNumber.HasValue) setParams["pageNumber"] = pageNumber.Value;
            if (extendedData != null) setParams["extendedData"] = extendedData;

            parameters["params"] = JsonSerializer.Serialize(setParams);

            if (!string.IsNullOrEmpty(userHash))
            {
                parameters["userHash"] = userHash;
            }

            return await GetAsync<BrickSetSetsResponse>("getSets", parameters);
        }


        /// <summary>
        /// Get a single set by its set number (e.g. "75192-1").
        /// Convenience wrapper around GetSetsAsync.
        /// </summary>
        public async Task<BrickSetSet> GetSetByNumberAsync(string setNumber, string userHash = null)
        {
            var response = await GetSetsAsync(userHash: userHash, setNumber: setNumber);

            if (response.Sets == null || response.Sets.Count == 0)
            {
                return null;
            }

            return response.Sets[0];
        }

        #endregion


        #region Reference Data — Images & Instructions

        /// <summary>Get additional images for a set.</summary>
        public async Task<List<BrickSetImage>> GetAdditionalImagesAsync(int setId)
        {
            var response = await GetAsync<BrickSetImagesResponse>("getAdditionalImages",
                new Dictionary<string, string> { { "setID", setId.ToString() } });

            return response.AdditionalImages ?? new List<BrickSetImage>();
        }


        /// <summary>Get instruction PDF links for a set (by internal set ID).</summary>
        public async Task<List<BrickSetInstruction>> GetInstructionsAsync(int setId)
        {
            var response = await GetAsync<BrickSetInstructionsResponse>("getInstructions",
                new Dictionary<string, string> { { "setID", setId.ToString() } });

            return response.Instructions ?? new List<BrickSetInstruction>();
        }


        /// <summary>Get instruction PDF links for a set (by set number, e.g. "75192-1").</summary>
        public async Task<List<BrickSetInstruction>> GetInstructions2Async(string setNumber)
        {
            var response = await GetAsync<BrickSetInstructionsResponse>("getInstructions2",
                new Dictionary<string, string> { { "setNumber", setNumber } });

            return response.Instructions ?? new List<BrickSetInstruction>();
        }

        #endregion


        #region Reference Data — Reviews

        /// <summary>Get community reviews for a set.</summary>
        public async Task<List<BrickSetReview>> GetReviewsAsync(int setId)
        {
            var response = await GetAsync<BrickSetReviewsResponse>("getReviews",
                new Dictionary<string, string> { { "setID", setId.ToString() } });

            return response.Reviews ?? new List<BrickSetReview>();
        }

        #endregion


        #region User Collection

        /// <summary>
        /// Set collection status for a set (owned, wanted, quantity, rating, notes).
        /// Requires a valid userHash.
        /// </summary>
        public async Task<BrickSetApiResponse> SetCollectionAsync(
            string userHash, int setId,
            bool? owned = null, bool? wanted = null,
            int? qtyOwned = null, int? rating = null,
            string notes = null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "userHash", userHash },
                { "SetID", setId.ToString() }
            };

            //
            // BrickSet uses a JSON "params" parameter
            //
            var collectionParams = new Dictionary<string, object>();
            if (owned.HasValue) collectionParams["own"] = owned.Value ? "1" : "0";
            if (wanted.HasValue) collectionParams["want"] = wanted.Value ? "1" : "0";
            if (qtyOwned.HasValue) collectionParams["qtyOwned"] = qtyOwned.Value;
            if (rating.HasValue) collectionParams["rating"] = rating.Value;
            if (notes != null) collectionParams["notes"] = notes;

            parameters["params"] = JsonSerializer.Serialize(collectionParams);

            return await GetAsync<BrickSetApiResponse>("setCollection", parameters);
        }

        #endregion


        #region Internal HTTP Helpers

        /// <summary>
        /// Performs a GET request to a BrickSet API method.
        ///
        /// BrickSet API methods are invoked as:
        ///   GET https://brickset.com/api/v3.asmx/{method}?apiKey={key}&amp;param1=value1&amp;...
        ///
        /// All methods include the apiKey automatically.
        /// </summary>
        private async Task<T> GetAsync<T>(string method, Dictionary<string, string> parameters) where T : BrickSetApiResponse
        {
            await Task.Delay(ThrottleDelayMs);

            string url = BuildUrl(method, parameters);
            _log($"  → GET {method}");

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
                            int waitSeconds = (int)Math.Pow(2, retries + 1);
                            _log($"  ⏳ Rate limited — waiting {waitSeconds}s before retry...");
                            await Task.Delay(waitSeconds * 1000);
                            retries++;
                            continue;
                        }

                        throw new BrickSetApiException(
                            response.StatusCode,
                            response.ReasonPhrase,
                            body);
                    }

                    //
                    // Deserialise the response
                    //
                    T result = JsonSerializer.Deserialize<T>(body, _jsonOptions);

                    //
                    // BrickSet API-level errors (HTTP 200 but status != "success")
                    //
                    if (result != null && !result.IsSuccess)
                    {
                        throw new BrickSetApiException(result.Message ?? "Unknown BrickSet API error");
                    }

                    return result;
                }
            }
        }


        /// <summary>
        /// Builds a full API URL for a BrickSet method with parameters.
        /// The apiKey is always included automatically.
        /// </summary>
        private string BuildUrl(string method, Dictionary<string, string> parameters)
        {
            var sb = new System.Text.StringBuilder(BaseUrl);
            sb.Append('/').Append(method);
            sb.Append("?apiKey=").Append(Uri.EscapeDataString(_apiKey));

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    sb.Append('&').Append(kvp.Key).Append('=').Append(Uri.EscapeDataString(kvp.Value));
                }
            }

            return sb.ToString();
        }

        #endregion


        public void Dispose()
        {
            if (_ownsHttpClient)
                _httpClient?.Dispose();
        }
    }
}
