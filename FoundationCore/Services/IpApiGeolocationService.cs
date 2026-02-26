//
// IpApiGeolocationService — ip-api.com implementation of IIpGeolocationService
//
// Uses the free ip-api.com JSON endpoint to resolve IP addresses to geographic locations.
// Includes in-memory caching via ExpiringCache to minimize redundant external API calls,
// and detection of private/local IP addresses that cannot be geolocated.
//
// Free tier limitations:
//   - HTTP only (no HTTPS on free tier)
//   - 45 requests per minute rate limit
//   - Non-commercial use only
//
// To swap to a different provider, implement IIpGeolocationService and register it in DI.
//
// AI-assisted development - February 2026
//

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Foundation.Concurrent;

namespace Foundation.Services
{
    /// <summary>
    ///
    /// Geolocation provider implementation using the free ip-api.com service.
    ///
    /// This provider uses HTTP (not HTTPS) because the free tier does not support HTTPS.
    /// All lookups are cached in an ExpiringCache for 24 hours to minimize external calls
    /// and respect the 45 requests/minute rate limit.
    ///
    /// </summary>
    public class IpApiGeolocationService : IIpGeolocationService
    {
        //
        // Constants
        //
        private const string API_BASE_URL = "http://ip-api.com/json/";
        private const string API_FIELDS = "?fields=status,message,countryCode,country,city,lat,lon";
        private const int CACHE_EXPIRATION_SECONDS = 86400;


        //
        // In-memory cache to avoid redundant lookups for the same IP within a process lifetime.
        // Keyed by IP address string, cached for 24 hours.
        //
        private static readonly ExpiringCache<string, IpGeolocationResult> _lookupCache =
            new ExpiringCache<string, IpGeolocationResult>(expirationInSeconds: CACHE_EXPIRATION_SECONDS);

        //
        // HTTP client for making API calls.  Static to allow connection reuse.
        //
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private readonly ILogger<IpApiGeolocationService> _logger;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logger for recording lookup activity and errors</param>
        public IpApiGeolocationService(ILogger<IpApiGeolocationService> logger)
        {
            _logger = logger;
        }


        /// <summary>
        ///
        /// Looks up the geographic location of the given IP address using ip-api.com.
        ///
        /// Private/local IP addresses (127.x, 10.x, 192.168.x, 172.16-31.x, ::1) are detected
        /// and returned as unsuccessful lookups without making an external API call.
        ///
        /// Results are cached for 24 hours to reduce API calls.
        ///
        /// </summary>
        /// <param name="ipAddress">The IPv4 or IPv6 address to look up</param>
        /// <returns>The geolocation result</returns>
        public async Task<IpGeolocationResult> LookupAsync(string ipAddress)
        {
            //
            // Validate input
            //
            if (string.IsNullOrWhiteSpace(ipAddress) == true)
            {
                return CreateFailureResult("IP address is null or empty");
            }

            //
            // Check for private/local IP addresses that cannot be geolocated
            //
            if (IsPrivateOrLocalIp(ipAddress) == true)
            {
                return CreateFailureResult("Private or local IP address");
            }

            //
            // Check the in-memory cache first
            //
            if (_lookupCache.TryGetValue(ipAddress, out IpGeolocationResult cachedResult) == true)
            {
                return cachedResult;
            }

            //
            // Make the external API call
            //
            IpGeolocationResult result = await CallApiAsync(ipAddress);

            //
            // Cache the result regardless of success/failure to avoid hammering the API
            // for IPs that consistently fail
            //
            _lookupCache.TryAdd(ipAddress, result);

            return result;
        }


        /// <summary>
        /// Makes the HTTP call to ip-api.com and parses the JSON response.
        /// </summary>
        /// <param name="ipAddress">The IP address to look up</param>
        /// <returns>The parsed geolocation result</returns>
        private async Task<IpGeolocationResult> CallApiAsync(string ipAddress)
        {
            try
            {
                string requestUrl = API_BASE_URL + ipAddress + API_FIELDS;

                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode == false)
                {
                    _logger.LogWarning("ip-api.com returned HTTP {StatusCode} for IP {IpAddress}",
                                       (int)response.StatusCode,
                                       ipAddress);

                    return CreateFailureResult("HTTP " + (int)response.StatusCode);
                }

                string responseBody = await response.Content.ReadAsStringAsync();

                //
                // Parse the JSON response
                //
                IpGeolocationResult result = ParseApiResponse(responseBody, ipAddress);

                return result;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("ip-api.com request timed out for IP {IpAddress}", ipAddress);

                return CreateFailureResult("Request timed out");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "ip-api.com request failed for IP {IpAddress}", ipAddress);

                return CreateFailureResult("HTTP request failed: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during ip-api.com lookup for IP {IpAddress}", ipAddress);

                return CreateFailureResult("Unexpected error: " + ex.Message);
            }
        }


        /// <summary>
        /// Parses the JSON response from ip-api.com into an IpGeolocationResult.
        /// </summary>
        /// <param name="json">The raw JSON response body</param>
        /// <param name="ipAddress">The IP address that was looked up (for logging)</param>
        /// <returns>The parsed result</returns>
        private IpGeolocationResult ParseApiResponse(string json, string ipAddress)
        {
            try
            {
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    JsonElement root = document.RootElement;

                    //
                    // Check the status field — ip-api.com returns "success" or "fail"
                    //
                    string status = "";

                    if (root.TryGetProperty("status", out JsonElement statusElement) == true)
                    {
                        status = statusElement.GetString() ?? "";
                    }

                    if (status != "success")
                    {
                        string message = "";

                        if (root.TryGetProperty("message", out JsonElement messageElement) == true)
                        {
                            message = messageElement.GetString() ?? "";
                        }

                        _logger.LogDebug("ip-api.com returned status '{Status}' for IP {IpAddress}: {Message}",
                                         status, ipAddress, message);

                        return CreateFailureResult("Provider returned: " + message);
                    }

                    //
                    // Extract the location fields
                    //
                    IpGeolocationResult result = new IpGeolocationResult();

                    result.IsSuccess = true;

                    if (root.TryGetProperty("countryCode", out JsonElement countryCodeElement) == true)
                    {
                        result.CountryCode = countryCodeElement.GetString();
                    }

                    if (root.TryGetProperty("country", out JsonElement countryElement) == true)
                    {
                        result.CountryName = countryElement.GetString();
                    }

                    if (root.TryGetProperty("city", out JsonElement cityElement) == true)
                    {
                        result.City = cityElement.GetString();
                    }

                    if (root.TryGetProperty("lat", out JsonElement latElement) == true)
                    {
                        result.Latitude = latElement.GetDouble();
                    }

                    if (root.TryGetProperty("lon", out JsonElement lonElement) == true)
                    {
                        result.Longitude = lonElement.GetDouble();
                    }

                    return result;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse ip-api.com response for IP {IpAddress}", ipAddress);

                return CreateFailureResult("JSON parse error: " + ex.Message);
            }
        }


        /// <summary>
        ///
        /// Determines whether the given IP address is a private or local address
        /// that cannot be geolocated by external services.
        ///
        /// </summary>
        /// <param name="ipAddress">The IP address to check</param>
        /// <returns>True if the IP is private/local, false otherwise</returns>
        private bool IsPrivateOrLocalIp(string ipAddress)
        {
            //
            // IPv6 loopback
            //
            if (ipAddress == "::1")
            {
                return true;
            }

            //
            // IPv4 loopback and private ranges
            //
            if (ipAddress.StartsWith("127.") == true)
            {
                return true;
            }

            if (ipAddress.StartsWith("10.") == true)
            {
                return true;
            }

            if (ipAddress.StartsWith("192.168.") == true)
            {
                return true;
            }

            //
            // 172.16.0.0 - 172.31.255.255 range
            //
            if (ipAddress.StartsWith("172.") == true)
            {
                string[] parts = ipAddress.Split('.');

                if (parts.Length >= 2)
                {
                    if (int.TryParse(parts[1], out int secondOctet) == true)
                    {
                        if (secondOctet >= 16 && secondOctet <= 31)
                        {
                            return true;
                        }
                    }
                }
            }

            //
            // Link-local
            //
            if (ipAddress.StartsWith("169.254.") == true)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Creates a standardized failure result.
        /// </summary>
        /// <param name="reason">The reason for the failure</param>
        /// <returns>An IpGeolocationResult with IsSuccess = false</returns>
        private IpGeolocationResult CreateFailureResult(string reason)
        {
            IpGeolocationResult result = new IpGeolocationResult();

            result.IsSuccess = false;
            result.FailureReason = reason;

            return result;
        }
    }
}
