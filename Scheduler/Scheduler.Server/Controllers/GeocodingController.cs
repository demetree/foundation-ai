//
// GeocodingController.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// API controller that exposes the GeocodingService for resolving addresses into
// geographic coordinates.  Used by the client-side location map component's
// "Resolve from Address" feature.
//
using Foundation.Controllers;
using Foundation.Auditor;
using Foundation.Security;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Server.Services;
using System;
using System.Threading.Tasks;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    public partial class GeocodingController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 0;

        private readonly GeocodingService _geocodingService;


        public GeocodingController(GeocodingService geocodingService) : base("Scheduler", "Geocoding")
        {
            _geocodingService = geocodingService;
        }


        /// <summary>
        /// Request model for the geocoding resolve endpoint.
        /// </summary>
        public class GeocodeRequest
        {
            public string AddressLine1 { get; set; }
            public string City { get; set; }
            public int? StateProvinceId { get; set; }
            public string PostalCode { get; set; }
            public int? CountryId { get; set; }
        }


        /// <summary>
        ///
        /// Resolves an address into geographic coordinates using the server-side geocoding service.
        ///
        /// The endpoint accepts address components including stateProvinceId and countryId as
        /// foreign keys — the server resolves these to names before calling the geocoding API.
        ///
        /// </summary>
        [Route("api/Geocoding/Resolve")]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        public async Task<IActionResult> Resolve([FromBody] GeocodeRequest request)
        {
            StartAuditEventClock();

            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                GeocodingResult result = await _geocodingService.GeocodeAddressAsync(
                    addressLine1: request.AddressLine1,
                    city: request.City,
                    stateProvinceId: request.StateProvinceId,
                    postalCode: request.PostalCode,
                    countryId: request.CountryId
                );

                if (result == null)
                {
                    await CreateAuditEventAsync(
                        AuditEngine.AuditType.Miscellaneous,
                        $"Geocoding: No results found for address '{request.AddressLine1}, {request.City}'.");

                    return NotFound(new { message = "Address could not be resolved. Try providing more details." });
                }

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Miscellaneous,
                    $"Geocoding: Resolved '{request.AddressLine1}, {request.City}' → ({result.Latitude}, {result.Longitude}).");

                return Ok(result);
            }
            catch (TimeoutException)
            {
                //
                // Rate limiter queue is full — return 429 with a friendly message
                //
                return StatusCode(429, new { message = "Geocoding is busy, please try again in a moment." });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    "Geocoding failed unexpectedly.",
                    ex.ToString());

                return StatusCode(500, new { message = "An error occurred while resolving the address." });
            }
        }
    }
}
