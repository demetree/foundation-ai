//
// IIpGeolocationService — Provider-agnostic interface for IP-to-location lookups
//
// This interface allows different geolocation providers (ip-api.com, MaxMind GeoLite2,
// ipinfo.io, etc.) to be swapped in via DI configuration without changing business logic.
//
// AI-assisted development - February 2026
//

using System;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    ///
    /// Result of an IP geolocation lookup.
    /// Contains the geographic coordinates and location name data for an IP address.
    ///
    /// </summary>
    public class IpGeolocationResult
    {
        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string City { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        /// <summary>
        /// Indicates whether the lookup was successful.  A failed lookup (e.g., private IP, rate limit, provider error)
        /// will have IsSuccess = false and the location fields may be null.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Optional failure reason when IsSuccess is false.
        /// </summary>
        public string FailureReason { get; set; }
    }


    /// <summary>
    ///
    /// Provider-agnostic interface for IP-to-location lookups.
    ///
    /// Implementations should handle their own rate limiting, caching, and error recovery.
    /// Callers should never assume that every lookup will succeed — always check IsSuccess
    /// on the returned result.
    ///
    /// </summary>
    public interface IIpGeolocationService
    {
        /// <summary>
        /// Looks up the geographic location of the given IP address.
        /// </summary>
        /// <param name="ipAddress">The IPv4 or IPv6 address to look up</param>
        /// <returns>The geolocation result, with IsSuccess indicating whether the lookup succeeded</returns>
        Task<IpGeolocationResult> LookupAsync(string ipAddress);
    }
}
