using System;
using System.Threading.Tasks;
using Foundation.BMC.Services;
using Foundation.Controllers;
using Foundation.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    ///
    /// Read-only controller for the Parts Universe visualization data.
    ///
    /// All data is served from an in-memory cache that is precomputed at server
    /// startup by <see cref="PartsUniverseService"/>. Typical response time is
    /// under 1ms since no database round-trip is needed.
    ///
    /// </summary>
    [ApiController]
    [Route("api/parts-universe")]
    public class PartsUniverseController : SecureWebAPIController
    {
        private readonly PartsUniverseService _partsUniverseService;
        private readonly ILogger<PartsUniverseController> _logger;


        public PartsUniverseController(
            PartsUniverseService partsUniverseService,
            ILogger<PartsUniverseController> logger
        ) : base("BMC", "PartsUniverse")
        {
            _partsUniverseService = partsUniverseService;
            _logger = logger;
        }


        /// <summary>
        ///
        /// GET /api/parts-universe
        ///
        /// Returns the full precomputed visualization payload containing ranked parts,
        /// Sankey, bubble, heatmap, and chord data along with summary stats.
        ///
        /// Returns 503 if the data has not yet been computed.
        ///
        /// </summary>
        [HttpGet]
        public IActionResult GetPartsUniverseData()
        {
            PartsUniversePayload payload = _partsUniverseService.GetCachedPayload();

            if (payload == null)
            {
                return StatusCode(503, new { message = "Parts Universe data is still being computed. Please try again shortly." });
            }

            return Ok(payload);
        }


        /// <summary>
        ///
        /// POST /api/parts-universe/refresh
        ///
        /// Triggers a re-computation of the Parts Universe data from the database.
        /// This is an admin-level operation that replaces the in-memory cache.
        ///
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshPartsUniverseData()
        {
            try
            {
                await _partsUniverseService.RefreshAsync().ConfigureAwait(false);

                return Ok(new { message = "Parts Universe data refreshed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PartsUniverseController] Refresh failed.");

                return StatusCode(500, new { message = "Refresh failed. Check server logs." });
            }
        }
    }
}
