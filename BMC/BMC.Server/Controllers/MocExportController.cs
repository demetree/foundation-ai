using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.BMC.Database;
using Foundation.BMC.Services;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    ///
    /// Controller for MOC (My Own Creation) export operations.
    ///
    /// Provides endpoints for downloading project data as model files
    /// in various formats (.ldr, .mpd, .io).
    ///
    /// Supports two export modes:
    ///   - Round-trip: uses stored source file data for maximum fidelity
    ///   - Native: reconstructs the model from PlacedBrick entities
    ///
    /// All endpoints require authentication and tenant membership.
    ///
    /// AI-developed code — initial implementation March 2026
    ///
    /// </summary>
    public class MocExportController : SecureWebAPIController
    {
        //
        // Constants
        //
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;


        //
        // Dependencies
        //
        private readonly BMCContext _context;
        private readonly ModelExportService _exportService;
        private readonly ILogger<MocExportController> _logger;


        /// <summary>
        /// Constructor — injects BMC context, export service, and logger.
        /// </summary>
        public MocExportController(BMCContext context, ModelExportService exportService, ILogger<MocExportController> logger)
            : base("BMC", "MocExport")
        {
            _context = context;
            _exportService = exportService;
            _logger = logger;
        }


        /// <summary>
        ///
        /// GET /api/moc/export/formats
        ///
        /// Returns the list of file formats available for export.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/export/formats")]
        public IActionResult GetExportFormats()
        {
            var formats = new[]
            {
                new { extension = ".ldr", name = "LDraw Model", description = "Single LDraw model file with parts and build steps" },
                new { extension = ".mpd", name = "LDraw Multi-Part Document", description = "LDraw file with submodels in a single document" },
                new { extension = ".io", name = "BrickLink Studio", description = "BrickLink Studio project file (password-protected ZIP with embedded LDraw)" }
            };

            return Ok(formats);
        }


        /// <summary>
        ///
        /// GET /api/moc/export/{projectId}/ldr
        ///
        /// Download a project as an LDraw .ldr file.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/export/{projectId}/ldr")]
        public async Task<IActionResult> ExportLdr(int projectId, CancellationToken cancellationToken = default)
        {
            return await ExportInFormat(projectId, "ldr", cancellationToken);
        }


        /// <summary>
        ///
        /// GET /api/moc/export/{projectId}/mpd
        ///
        /// Download a project as an LDraw Multi-Part Document .mpd file.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/export/{projectId}/mpd")]
        public async Task<IActionResult> ExportMpd(int projectId, CancellationToken cancellationToken = default)
        {
            return await ExportInFormat(projectId, "mpd", cancellationToken);
        }


        /// <summary>
        ///
        /// GET /api/moc/export/{projectId}/io
        ///
        /// Download a project as a BrickLink Studio .io file.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/export/{projectId}/io")]
        public async Task<IActionResult> ExportIo(int projectId, CancellationToken cancellationToken = default)
        {
            return await ExportInFormat(projectId, "io", cancellationToken);
        }


        /// <summary>
        /// Common export handler for all formats.
        /// </summary>
        private async Task<IActionResult> ExportInFormat(int projectId, string format, CancellationToken cancellationToken)
        {
            StartAuditEventClock();

            //
            // Security check
            //
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            //
            // Execute the export
            //
            try
            {
                ModelExportService.ExportResult result = null;

                switch (format)
                {
                    case "ldr":
                        result = await _exportService.ExportToLdrAsync(projectId, userTenantGuid, cancellationToken);
                        break;

                    case "mpd":
                        result = await _exportService.ExportToMpdAsync(projectId, userTenantGuid, cancellationToken);
                        break;

                    case "io":
                        result = await _exportService.ExportToIoAsync(projectId, userTenantGuid, cancellationToken);
                        break;

                    default:
                        return BadRequest($"Unsupported export format: '{format}'");
                }

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.ReadEntity,
                    $"MOC export — Project id={projectId} exported as .{format}",
                    projectId.ToString());

                _logger.LogInformation(
                    "MOC export complete — Project id={Id}, format=.{Format}, size={Size} bytes",
                    projectId,
                    format,
                    result.FileData.Length);

                return File(result.FileData, result.MimeType, result.FileName);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "MOC export error for project {Id}, format .{Format}", projectId, format);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MOC export failed for project {Id}, format .{Format}", projectId, format);
                return Problem("An error occurred while exporting the project. Please try again or contact support.");
            }
        }
    }
}
