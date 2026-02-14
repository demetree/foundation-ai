//
// GeocodingService.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Server-side geocoding service that resolves address components into latitude/longitude
// coordinates using the Nominatim (OpenStreetMap) API.
//
// Features:
//   - Accepts stateProvinceId and countryId as foreign keys and resolves names from the database
//   - Built-in rate limiting (1 req/sec via SemaphoreSlim + timestamp tracking)
//   - Configurable Nominatim base URL for future self-hosted instance support
//   - Structured query parameters for improved geocoding accuracy
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Scheduler.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Scheduler.Server.Services
{
    /// <summary>
    /// Result of a geocoding operation.
    /// </summary>
    public class GeocodingResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string DisplayName { get; set; }
        public double Confidence { get; set; }
    }


    /// <summary>
    ///
    /// Service responsible for resolving address components into geographic coordinates
    /// using the Nominatim (OpenStreetMap) geocoding API.
    ///
    /// Rate limiting is enforced via a static SemaphoreSlim to ensure only one request
    /// is made at a time across all users, with a minimum 1-second delay between requests.
    ///
    /// </summary>
    public class GeocodingService
    {
        //
        // Static rate limiting primitives — shared across all instances
        //
        private static readonly SemaphoreSlim _rateLimitSemaphore = new SemaphoreSlim(1, 1);
        private static DateTime _lastRequestTimestamp = DateTime.MinValue;

        //
        // Rate limiting configuration
        //
        private const int MINIMUM_DELAY_BETWEEN_REQUESTS_MS = 1100;     // Slightly over 1 second to be safe
        private const int SEMAPHORE_WAIT_TIMEOUT_MS = 10000;             // 10 second queue timeout

        //
        // Default Nominatim endpoint
        //
        private const string DEFAULT_NOMINATIM_BASE_URL = "https://nominatim.openstreetmap.org";
        private const string USER_AGENT = "Scheduler/1.0 (k2research.ca)";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GeocodingService> _logger;
        private readonly SchedulerContext _db;
        private readonly string _nominatimBaseUrl;


        /// <summary>
        /// Constructs the GeocodingService with required dependencies.
        /// </summary>
        public GeocodingService(
            IHttpClientFactory httpClientFactory,
            ILogger<GeocodingService> logger,
            SchedulerContext db,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _db = db;

            //
            // Read the Nominatim base URL from configuration, defaulting to the public instance.
            // This allows swapping to a self-hosted instance via appsettings.json.
            //
            _nominatimBaseUrl = configuration.GetValue<string>("Geocoding:NominatimBaseUrl")
                                ?? DEFAULT_NOMINATIM_BASE_URL;
        }


        /// <summary>
        ///
        /// Resolves address components into geographic coordinates using the Nominatim API.
        ///
        /// StateProvinceId and CountryId are resolved to their name strings from the database
        /// before being sent to the geocoding API.
        ///
        /// Returns null if the address could not be resolved.
        ///
        /// </summary>
        /// <param name="addressLine1">Street address</param>
        /// <param name="city">City name</param>
        /// <param name="stateProvinceId">Optional FK to the StateProvince table</param>
        /// <param name="postalCode">Postal/ZIP code</param>
        /// <param name="countryId">Optional FK to the Country table</param>
        /// <returns>GeocodingResult with coordinates, or null if not found</returns>
        /// <exception cref="TimeoutException">Thrown when the rate limiter queue is too deep</exception>
        public async Task<GeocodingResult> GeocodeAddressAsync(
            string addressLine1,
            string city,
            int? stateProvinceId,
            string postalCode,
            int? countryId)
        {
            //
            // Resolve stateProvinceId and countryId to name strings from the database
            //
            string stateProvinceName = null;
            string countryName = null;

            if (stateProvinceId.HasValue)
            {
                StateProvince stateProvince = await _db.StateProvinces
                    .Where(sp => sp.id == stateProvinceId.Value && sp.deleted == false)
                    .FirstOrDefaultAsync();

                stateProvinceName = stateProvince?.name;
            }

            if (countryId.HasValue)
            {
                Country country = await _db.Countries
                    .Where(c => c.id == countryId.Value && c.deleted == false)
                    .FirstOrDefaultAsync();

                countryName = country?.name;
            }


            //
            // Acquire the rate limiting semaphore.  If we can't acquire within the timeout,
            // the queue is too deep — throw a timeout exception that the controller will
            // translate into a 429 response.
            //
            bool acquired = await _rateLimitSemaphore.WaitAsync(SEMAPHORE_WAIT_TIMEOUT_MS);

            if (!acquired)
            {
                _logger.LogWarning("Geocoding rate limiter queue is full. Request rejected.");
                throw new TimeoutException("Geocoding is busy, please try again in a moment.");
            }

            try
            {
                //
                // Enforce minimum delay between requests to respect Nominatim's usage policy
                //
                TimeSpan timeSinceLastRequest = DateTime.UtcNow - _lastRequestTimestamp;

                if (timeSinceLastRequest.TotalMilliseconds < MINIMUM_DELAY_BETWEEN_REQUESTS_MS)
                {
                    int delayMs = MINIMUM_DELAY_BETWEEN_REQUESTS_MS - (int)timeSinceLastRequest.TotalMilliseconds;
                    await Task.Delay(delayMs);
                }

                //
                // Build the structured query URL
                //
                string requestUrl = BuildNominatimUrl(addressLine1, city, stateProvinceName, postalCode, countryName);

                _logger.LogInformation("Geocoding request: {Url}", requestUrl);


                //
                // Make the HTTP request
                //
                HttpClient client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd(USER_AGENT);

                HttpResponseMessage response = await client.GetAsync(requestUrl);
                _lastRequestTimestamp = DateTime.UtcNow;

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Nominatim returned status {StatusCode} for geocoding request.", response.StatusCode);
                    return null;
                }

                string responseBody = await response.Content.ReadAsStringAsync();

                //
                // Parse the Nominatim JSON response
                //
                return ParseNominatimResponse(responseBody);
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }


        /// <summary>
        /// Builds the Nominatim structured search URL from address components.
        /// </summary>
        private string BuildNominatimUrl(
            string street, string city, string state, string postalCode, string country)
        {
            List<string> queryParts = new List<string>();

            queryParts.Add("format=json");
            queryParts.Add("limit=1");
            queryParts.Add("addressdetails=1");

            if (!string.IsNullOrWhiteSpace(street))
                queryParts.Add($"street={Uri.EscapeDataString(street)}");

            if (!string.IsNullOrWhiteSpace(city))
                queryParts.Add($"city={Uri.EscapeDataString(city)}");

            if (!string.IsNullOrWhiteSpace(state))
                queryParts.Add($"state={Uri.EscapeDataString(state)}");

            if (!string.IsNullOrWhiteSpace(postalCode))
                queryParts.Add($"postalcode={Uri.EscapeDataString(postalCode)}");

            if (!string.IsNullOrWhiteSpace(country))
                queryParts.Add($"country={Uri.EscapeDataString(country)}");

            return $"{_nominatimBaseUrl}/search?{string.Join("&", queryParts)}";
        }


        /// <summary>
        /// Parses the Nominatim JSON response array and extracts the first result.
        /// </summary>
        private GeocodingResult ParseNominatimResponse(string responseBody)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                {
                    _logger.LogInformation("Nominatim returned no results.");
                    return null;
                }

                JsonElement firstResult = root[0];

                //
                // Extract coordinates
                //
                if (!firstResult.TryGetProperty("lat", out JsonElement latElement) ||
                    !firstResult.TryGetProperty("lon", out JsonElement lonElement))
                {
                    _logger.LogWarning("Nominatim result missing lat/lon properties.");
                    return null;
                }

                double latitude = double.Parse(latElement.GetString());
                double longitude = double.Parse(lonElement.GetString());

                //
                // Extract display name
                //
                string displayName = firstResult.TryGetProperty("display_name", out JsonElement displayNameElement)
                    ? displayNameElement.GetString()
                    : "Unknown location";

                //
                // Calculate a confidence score based on the importance value Nominatim provides (0-1 range)
                //
                double confidence = 0.5;

                if (firstResult.TryGetProperty("importance", out JsonElement importanceElement))
                {
                    if (importanceElement.ValueKind == JsonValueKind.Number)
                    {
                        confidence = importanceElement.GetDouble();
                    }
                    else if (importanceElement.ValueKind == JsonValueKind.String)
                    {
                        double.TryParse(importanceElement.GetString(), out confidence);
                    }
                }

                //
                // Round coordinates to 6 decimal places (approx 0.11m precision)
                //
                return new GeocodingResult
                {
                    Latitude = Math.Round(latitude, 6),
                    Longitude = Math.Round(longitude, 6),
                    DisplayName = displayName,
                    Confidence = Math.Round(confidence, 4)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Nominatim response.");
                return null;
            }
        }
    }
}
