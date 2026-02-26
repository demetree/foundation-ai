using System.Threading.Tasks;
using Foundation.Auditor;
using Foundation.BMC.Services;
using Foundation.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    ///
    /// Read-only controller for the Minifig Gallery visualization.
    ///
    /// Returns the full precomputed list of LEGO minifigs from an in-memory cache
    /// that is built at server startup by <see cref="MinifigGalleryService"/>.
    /// The client caches this payload in IndexedDB for instant subsequent loads.
    ///
    /// Typical response time is under 1ms since no database round-trip is needed.
    ///
    /// </summary>
    [ApiController]
    [Route("api/minifig-gallery")]
    public class MinifigGalleryController : SecureWebAPIController
    {
        private readonly MinifigGalleryService _minifigGalleryService;
        private readonly ILogger<MinifigGalleryController> _logger;


        public MinifigGalleryController(
            MinifigGalleryService minifigGalleryService,
            ILogger<MinifigGalleryController> logger
        ) : base("BMC", "MinifigGallery")
        {
            _minifigGalleryService = minifigGalleryService;
            _logger = logger;
        }


        /// <summary>
        ///
        /// GET /api/minifig-gallery
        ///
        /// Returns the full precomputed list of all active, non-deleted LEGO minifigs
        /// as lean DTOs sorted newest-first (year desc, name asc).
        ///
        /// Returns 503 if the data has not yet been computed.
        ///
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMinifigGalleryData()
        {
            StartAuditEventClock();

            var minifigs = _minifigGalleryService.GetCachedMinifigs();

            if (minifigs == null)
            {
                return StatusCode(503, new { message = "Minifig Gallery data is still being computed. Please try again shortly." });
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.LoadPage, $"Minifig Gallery data loaded — {minifigs.Count} minifigs");

            return Ok(minifigs);
        }
    }
}
