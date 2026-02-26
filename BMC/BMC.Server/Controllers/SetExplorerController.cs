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
    /// Read-only controller for the Set Explorer visualization.
    ///
    /// Returns the full precomputed list of LEGO sets from an in-memory cache
    /// that is built at server startup by <see cref="SetExplorerService"/>.
    /// The client caches this payload in IndexedDB for instant subsequent loads.
    ///
    /// Typical response time is under 1ms since no database round-trip is needed.
    ///
    /// </summary>
    [ApiController]
    [Route("api/set-explorer")]
    public class SetExplorerController : SecureWebAPIController
    {
        private readonly SetExplorerService _setExplorerService;
        private readonly ILogger<SetExplorerController> _logger;


        public SetExplorerController(
            SetExplorerService setExplorerService,
            ILogger<SetExplorerController> logger
        ) : base("BMC", "SetExplorer")
        {
            _setExplorerService = setExplorerService;
            _logger = logger;
        }


        /// <summary>
        ///
        /// GET /api/set-explorer
        ///
        /// Returns the full precomputed list of all active, non-deleted LEGO sets
        /// as lean DTOs sorted newest-first (year desc, partCount desc).
        ///
        /// Returns 503 if the data has not yet been computed.
        ///
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSetExplorerData()
        {
            StartAuditEventClock();

            var sets = _setExplorerService.GetCachedSets();

            if (sets == null)
            {
                return StatusCode(503, new { message = "Set Explorer data is still being computed. Please try again shortly." });
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.LoadPage, $"Set Explorer data loaded — {sets.Count} sets");

            return Ok(sets);
        }
    }
}
