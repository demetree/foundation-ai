using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BMC.BrickOwl.Api.Models.Responses;


namespace BMC.BrickOwl.Api
{
    /// <summary>
    ///
    /// Complete strongly-typed client for the Brick Owl API v1.
    ///
    /// Handles API key authentication, throttling, and serialisation.
    /// This is a pure HTTP-level service with no database dependencies.
    ///
    /// Key characteristics:
    ///   - Uses API key authentication via "key" query parameter
    ///   - Base URL: https://api.brickowl.com/v1/
    ///   - Rate limit: 600 requests/minute (standard), 100/min for bulk
    ///   - Responses are JSON
    ///   - Items use BOIDs (Brick Owl IDs) as primary identifiers
    ///   - Supports cross-platform ID mapping (BrickLink, LEGO design IDs)
    ///
    /// Usage:
    ///     var client = new BrickOwlApiClient("your-api-key", msg => Console.WriteLine(msg));
    ///     var item = await client.CatalogLookupAsync("210158");
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    ///
    /// </summary>
    public class BrickOwlApiClient : IDisposable
    {
        private const string BaseUrl = "https://api.brickowl.com/v1";
        private const int ThrottleDelayMs = 200;

        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;
        private readonly Action<string> _log;

        private readonly JsonSerializerOptions _jsonOptions;


        /// <summary>
        /// Creates a new BrickOwlApiClient.
        /// </summary>
        /// <param name="apiKey">Brick Owl API key (from user profile).</param>
        /// <param name="log">Optional logging callback.</param>
        /// <param name="httpClient">Optional HttpClient for dependency injection.</param>
        public BrickOwlApiClient(string apiKey, Action<string> log = null, HttpClient httpClient = null)
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

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BMC-BrickOwl-Api/1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }


        #region Catalog

        /// <summary>
        /// Look up a catalog item by its BOID (Brick Owl ID).
        /// </summary>
        /// <param name="boid">Brick Owl ID of the item.</param>
        public async Task<BrickOwlCatalogItem> CatalogLookupAsync(string boid)
        {
            if (string.IsNullOrWhiteSpace(boid))
                throw new ArgumentException("BOID is required.", nameof(boid));

            string url = BuildUrl("/catalog/lookup", $"boid={Uri.EscapeDataString(boid)}");

            return await GetAsync<BrickOwlCatalogItem>(url, "catalog/lookup");
        }


        /// <summary>
        /// Map an external ID (BrickLink item number, LEGO set number, design ID, etc.)
        /// to one or more Brick Owl BOIDs.
        /// </summary>
        /// <param name="id">External ID to look up (e.g. "3001", "75192-1").</param>
        /// <param name="type">Item type: Part, Set, Minifigure, Gear, Sticker, Minibuild, Instructions, Packaging.</param>
        /// <param name="idType">Optional ID type: bl_item_no, set_number, design_id, ldraw, etc.</param>
        public async Task<BrickOwlIdLookupResult> CatalogIdLookupAsync(string id, string type, string idType = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID is required.", nameof(id));

            string queryParams = $"id={Uri.EscapeDataString(id)}&type={Uri.EscapeDataString(type)}";

            if (!string.IsNullOrEmpty(idType))
            {
                queryParams += $"&id_type={Uri.EscapeDataString(idType)}";
            }

            string url = BuildUrl("/catalog/id_lookup", queryParams);

            return await GetAsync<BrickOwlIdLookupResult>(url, "catalog/id_lookup");
        }


        /// <summary>
        /// Get pricing and availability for a specific item.
        /// Note: This endpoint may require special approval from Brick Owl.
        /// </summary>
        /// <param name="boid">Brick Owl ID of the item.</param>
        public async Task<BrickOwlAvailability> CatalogAvailabilityAsync(string boid)
        {
            if (string.IsNullOrWhiteSpace(boid))
                throw new ArgumentException("BOID is required.", nameof(boid));

            string url = BuildUrl("/catalog/availability", $"boid={Uri.EscapeDataString(boid)}");

            return await GetAsync<BrickOwlAvailability>(url, "catalog/availability");
        }

        #endregion


        #region Collection

        /// <summary>
        /// Get all lots in the user's personal collection.
        /// </summary>
        public async Task<List<BrickOwlCollectionLot>> GetCollectionLotsAsync()
        {
            string url = BuildUrl("/collection/lots");

            return await GetAsync<List<BrickOwlCollectionLot>>(url, "collection/lots");
        }

        #endregion


        #region Inventory

        /// <summary>
        /// Get all lots in the user's store inventory.
        /// </summary>
        public async Task<List<BrickOwlInventoryLot>> GetInventoryListAsync()
        {
            string url = BuildUrl("/inventory/list");

            return await GetAsync<List<BrickOwlInventoryLot>>(url, "inventory/list");
        }

        #endregion


        #region Orders

        /// <summary>
        /// Get a list of orders for the user's store.
        /// </summary>
        public async Task<List<BrickOwlOrder>> GetOrderListAsync()
        {
            string url = BuildUrl("/order/list");

            return await GetAsync<List<BrickOwlOrder>>(url, "order/list");
        }


        /// <summary>
        /// Get full details for a specific order.
        /// </summary>
        /// <param name="orderId">Brick Owl order ID.</param>
        public async Task<BrickOwlOrder> GetOrderAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("Order ID is required.", nameof(orderId));

            string url = BuildUrl("/order/view", $"order_id={Uri.EscapeDataString(orderId)}");

            return await GetAsync<BrickOwlOrder>(url, "order/view");
        }


        /// <summary>
        /// Get all items in a specific order.
        /// </summary>
        /// <param name="orderId">Brick Owl order ID.</param>
        public async Task<List<BrickOwlOrderItem>> GetOrderItemsAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("Order ID is required.", nameof(orderId));

            string url = BuildUrl("/order/items", $"order_id={Uri.EscapeDataString(orderId)}");

            return await GetAsync<List<BrickOwlOrderItem>>(url, "order/items");
        }

        #endregion


        #region Wishlist

        /// <summary>
        /// Get all wishlists for the user.
        /// </summary>
        public async Task<List<BrickOwlWishlist>> GetWishlistsAsync()
        {
            string url = BuildUrl("/wishlist/list");

            return await GetAsync<List<BrickOwlWishlist>>(url, "wishlist/list");
        }


        /// <summary>
        /// Get all items in a specific wishlist.
        /// </summary>
        /// <param name="wishlistId">Wishlist ID.</param>
        public async Task<List<BrickOwlWishlistItem>> GetWishlistItemsAsync(string wishlistId)
        {
            if (string.IsNullOrWhiteSpace(wishlistId))
                throw new ArgumentException("Wishlist ID is required.", nameof(wishlistId));

            string url = BuildUrl("/wishlist/items", $"wishlist_id={Uri.EscapeDataString(wishlistId)}");

            return await GetAsync<List<BrickOwlWishlistItem>>(url, "wishlist/items");
        }

        #endregion


        #region Internal HTTP Helpers

        /// <summary>
        /// Builds a full URL with the API key appended.
        /// </summary>
        private string BuildUrl(string path, string additionalQuery = null)
        {
            string url = $"{BaseUrl}{path}?key={Uri.EscapeDataString(_apiKey)}";

            if (!string.IsNullOrEmpty(additionalQuery))
            {
                url += $"&{additionalQuery}";
            }

            return url;
        }


        /// <summary>
        /// Performs a GET request to a Brick Owl API endpoint.
        ///
        /// All requests include:
        ///   - API key as query parameter
        ///   - Conservative throttle delay (200ms between requests)
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

                        throw new BrickOwlApiException(
                            response.StatusCode,
                            response.ReasonPhrase,
                            body);
                    }

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
