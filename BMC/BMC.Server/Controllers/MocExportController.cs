using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Foundation.Auditor;
using Foundation.Concurrent;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.BMC.Database;
using Foundation.BMC.Services;

using BMC.LDraw.Render;

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
        // Static thumbnail cache — 5 minute sliding expiry, shared across all requests.
        // Keyed by projectId.  No compression needed since PNG data is already compressed.
        //
        private static readonly ExpiringCache<int, byte[]> _thumbnailCache
            = new ExpiringCache<int, byte[]>(expirationInSeconds: 300, useSlidingExpiration: true, useCompression: false);


        //
        // Dependencies
        //
        private readonly BMCContext _context;
        private readonly ModelExportService _exportService;
        private readonly GlbCacheService _glbCacheService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MocExportController> _logger;


        /// <summary>
        /// Lazy-initialised RenderService — created on first STL export request.
        /// </summary>
        private RenderService _renderService;


        /// <summary>
        /// Constructor — injects BMC context, export service, configuration, and logger.
        /// </summary>
        public MocExportController(
            BMCContext context,
            ModelExportService exportService,
            GlbCacheService glbCacheService,
            IConfiguration configuration,
            ILogger<MocExportController> logger)
            : base("BMC", "MocExport")
        {
            _context = context;
            _exportService = exportService;
            _glbCacheService = glbCacheService;
            _configuration = configuration;
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
                new { extension = ".io", name = "BrickLink Studio", description = "BrickLink Studio project file (password-protected ZIP with embedded LDraw)" },
                new { extension = ".stl", name = "STL (3D Print)", description = "Stereolithography file for 3D printing or CAD import" }
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


        // ────────────────────────────────────────────────────────────────
        //  Viewer Data Endpoints
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        ///
        /// GET /api/moc/project/{projectId}/viewer-mpd
        ///
        /// Returns a self-contained LDraw MPD as text/plain for the client-side 3D viewer.
        ///
        /// The MPD includes all placed bricks with positions and colours,
        /// submodel FILE blocks, and any custom part geometry from the stored .io archive
        /// inlined as additional FILE blocks.
        ///
        /// Standard LDraw library parts are NOT inlined — the client's LDrawLoader
        /// resolves those via /api/ldraw/file/ with IndexedDB caching.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/project/{projectId}/viewer-mpd")]
        public async Task<IActionResult> GetViewerMpd(int projectId, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

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

            try
            {
                string mpdContent = await _exportService.GenerateViewerMpdAsync(projectId, userTenantGuid, cancellationToken);

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.ReadEntity,
                    $"MOC viewer data — Project id={projectId}",
                    projectId.ToString());

                return Content(mpdContent, "text/plain", System.Text.Encoding.UTF8);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate viewer MPD for project {Id}", projectId);
                return Problem("An error occurred while generating the viewer data.");
            }
        }


        /// <summary>
        ///
        /// GET /api/moc/project/{projectId}/summary
        ///
        /// Returns a lightweight JSON summary of a project for the viewer UI:
        /// name, description, part count, step count, submodel count, source format.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/project/{projectId}/summary")]
        public async Task<IActionResult> GetProjectSummary(int projectId, CancellationToken cancellationToken = default)
        {
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

            var summary = await _exportService.GetProjectSummaryAsync(projectId, userTenantGuid, cancellationToken);

            if (summary == null)
            {
                return NotFound($"Project {projectId} not found.");
            }

            return Ok(summary);
        }


        /// <summary>
        ///
        /// GET /api/moc/project/{projectId}/thumbnail
        ///
        /// Returns the project's stored thumbnail image (PNG)
        /// from the Project.thumbnailData blob.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.NoLimit, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/project/{projectId}/thumbnail")]
        public async Task<IActionResult> GetProjectThumbnail(int projectId, CancellationToken cancellationToken = default)
        {
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
            // Check the in-memory cache first
            //
            if (_thumbnailCache.TryGetValue(projectId, out byte[] cachedData))
            {
                return File(cachedData, "image/png");
            }

            //
            // Cache miss — query the database
            //
            Project project = await _context.Projects
                .Where(p => p.id == projectId && p.tenantGuid == userTenantGuid && p.active == true && p.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null || project.thumbnailData == null || project.thumbnailData.Length == 0)
            {
                return NotFound();
            }

            //
            // Store in the cache for subsequent requests
            //
            _thumbnailCache.TryAdd(projectId, project.thumbnailData);

            return File(project.thumbnailData, "image/png");
        }


        // ────────────────────────────────────────────────────────────────
        //  STL Export
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        ///
        /// GET /api/moc/export/{projectId}/stl
        ///
        /// Export a project's assembled geometry as an STL file.
        ///
        /// Generates the full MPD from PlacedBrick entities, resolves all geometry
        /// via the LDraw library, and exports as either binary (compact) or ASCII
        /// (human-readable) STL.
        ///
        /// This can take significant time for large models — uses a 120-second timeout.
        ///
        /// AI-developed code — March 2026
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/export/{projectId}/stl")]
        public async Task<IActionResult> ExportStl(
            int projectId,
            string format = "binary",
            CancellationToken cancellationToken = default)
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

            format = (format ?? "binary").ToLowerInvariant();
            bool ascii = format == "ascii";

            try
            {
                //
                // Step 1: Generate the full MPD content from the project's PlacedBrick entities
                //
                _logger.LogInformation("STL export starting for project {Id}, format={Format}", projectId, format);

                string mpdContent = await _exportService.GenerateViewerMpdAsync(projectId, userTenantGuid, cancellationToken);

                if (string.IsNullOrWhiteSpace(mpdContent))
                {
                    return NotFound("Project has no geometry data to export.");
                }

                //
                // Step 2: Get the project name for the export filename
                //
                Project project = await _context.Projects
                    .Where(p => p.id == projectId && p.tenantGuid == userTenantGuid && p.active == true && p.deleted == false)
                    .FirstOrDefaultAsync(cancellationToken);

                string projectName = project?.name ?? "export";
                string safeFileName = string.Join("_", projectName.Split(System.IO.Path.GetInvalidFileNameChars())).Trim();

                if (string.IsNullOrWhiteSpace(safeFileName))
                {
                    safeFileName = "export";
                }

                //
                // Step 3: Convert MPD text to lines and run the STL export on a background thread
                // with a 120-second timeout for large models
                //
                string[] mpdLines = mpdContent.Split('\n');
                string fileName = safeFileName + ".mpd";

                using var exportCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                exportCts.CancelAfter(TimeSpan.FromSeconds(120));

                byte[] stlBytes = await Task.Run(() =>
                {
                    string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                    EnsureRenderService(dataPath);
                    return _renderService.ExportToStl(mpdLines, fileName, colourCode: -1, ascii: ascii);
                }, exportCts.Token);

                if (stlBytes == null || stlBytes.Length == 0)
                {
                    return NotFound("Project geometry could not be resolved — the project may not have any placed bricks.");
                }

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.ReadEntity,
                    $"MOC STL export — Project id={projectId}, format={format}, size={stlBytes.Length}",
                    projectId.ToString());

                _logger.LogInformation(
                    "STL export complete — Project id={Id}, format={Format}, size={Size} bytes",
                    projectId,
                    format,
                    stlBytes.Length);

                string contentType = ascii ? "text/plain" : "application/octet-stream";
                string stlFileName = safeFileName + ".stl";

                return File(stlBytes, contentType, stlFileName);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("STL export timed out or was cancelled for project {Id}", projectId);
                return StatusCode(499, "STL export timed out or was cancelled. Large models may take longer — please try again.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "STL export error for project {Id}", projectId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "STL export failed for project {Id}", projectId);
                return Problem("An error occurred while exporting the project to STL. Please try again or contact support.");
            }
        }


        // ────────────────────────────────────────────────────────────────
        //  GLB Viewer Data (Pre-compiled binary glTF for fast 3D loading)
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        ///
        /// GET /api/moc/project/{projectId}/viewer-glb?edgeLines=true
        ///
        /// Returns a pre-compiled GLB (binary glTF) for the client-side 3D viewer.
        ///
        /// The GLB contains:
        ///   - One node per build step (with extras.stepIndex metadata)
        ///   - Triangles grouped by colour for minimal draw calls
        ///   - PBR materials tuned per LDraw finish type
        ///   - Optional edge lines (LINES-mode primitives) for the outlined LEGO look
        ///   - Smooth per-vertex normals
        ///   - Y-axis flipped for glTF coordinate system
        ///
        /// This replaces the viewer-mpd text-based loading pipeline for dramatically
        /// faster model load times (sub-second vs. 30+ seconds for large sets).
        ///
        /// AI-developed code — March 2026
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/project/{projectId}/viewer-glb")]
        public async Task<IActionResult> GetViewerGlb(
            int projectId,
            bool edgeLines = true,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

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

            try
            {
                //
                // Use the two-tier cache: RAM → DB → Build-on-miss.
                // First request builds (~11s for Titanic), all subsequent requests
                // serve from RAM cache (sub-millisecond) or DB (milliseconds).
                //
                _logger.LogInformation("GLB viewer request for project {Id}, edgeLines={EdgeLines}", projectId, edgeLines);

                byte[] glbBytes = await _glbCacheService.GetOrBuildGlbAsync(
                    projectId, userTenantGuid, edgeLines, cancellationToken);

                if (glbBytes == null || glbBytes.Length == 0)
                {
                    return NotFound("Project geometry could not be compiled — the project may not have any placed bricks.");
                }

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.ReadEntity,
                    $"MOC GLB viewer — Project id={projectId}, edgeLines={edgeLines}, size={glbBytes.Length}",
                    projectId.ToString());

                _logger.LogInformation(
                    "GLB viewer export complete — Project id={Id}, edgeLines={EdgeLines}, size={Size} bytes",
                    projectId,
                    edgeLines,
                    glbBytes.Length);

                //
                // Cache for 1 hour — client will invalidate on project changes
                //
                Response.Headers["Cache-Control"] = "private, max-age=3600";

                return File(glbBytes, "model/gltf-binary", "model.glb");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GLB export timed out or was cancelled for project {Id}", projectId);
                return StatusCode(499, "GLB export timed out or was cancelled. Large models may take longer — please try again.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "GLB export error for project {Id}", projectId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GLB export failed for project {Id}", projectId);
                return Problem("An error occurred while compiling the project to GLB. Please try again or contact support.");
            }
        }


        // ────────────────────────────────────────────────────────────────
        //  Server-Side Model Rendering
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        ///
        /// GET /api/moc/export/{projectId}/render
        ///
        /// Server-side render of the fully assembled MOC model.
        ///
        /// Generates the MPD from PlacedBrick entities, resolves all geometry
        /// via the LDraw library, and renders to PNG, WebP, SVG, or turntable GIF.
        ///
        /// Supports full render configuration: size, camera angle, format,
        /// anti-aliasing, background, PBR ray tracing.
        ///
        /// AI-developed code — March 2026
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/export/{projectId}/render")]
        public async Task<IActionResult> RenderProject(
            int projectId,
            int width = 512,
            int height = 512,
            float elevation = 30f,
            float azimuth = -45f,
            bool renderEdges = true,
            bool smoothShading = true,
            string antiAlias = "none",
            string format = "png",
            int quality = 90,
            string backgroundHex = null,
            string gradientTopHex = null,
            string gradientBottomHex = null,
            string renderer = "rasterizer",
            bool enablePbr = true,
            float exposure = 1.0f,
            float aperture = 0f,
            int step = 0,
            float zoom = 1.0f,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

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

            // Normalise & clamp
            width = Math.Clamp(width, 64, 3840);
            height = Math.Clamp(height, 64, 3840);
            elevation = Math.Clamp(elevation, -90f, 90f);
            azimuth = Math.Clamp(azimuth, -360f, 360f);
            quality = Math.Clamp(quality, 1, 100);
            exposure = Math.Clamp(exposure, 0.1f, 10f);
            aperture = Math.Clamp(aperture, 0f, 10f);
            zoom = Math.Clamp(zoom, 0.5f, 3.0f);
            format = (format ?? "png").ToLowerInvariant();
            antiAlias = (antiAlias ?? "none").ToLowerInvariant();

            AntiAliasMode aaMode = AntiAliasMode.None;
            if (antiAlias == "2x") aaMode = AntiAliasMode.SSAA2x;
            else if (antiAlias == "4x") aaMode = AntiAliasMode.SSAA4x;

            if (aaMode == AntiAliasMode.SSAA4x && (width > 2560 || height > 2560))
                aaMode = AntiAliasMode.SSAA2x;

            try
            {
                // Generate the full MPD from project data
                string mpdContent = await _exportService.GenerateViewerMpdAsync(projectId, userTenantGuid, cancellationToken);

                if (string.IsNullOrWhiteSpace(mpdContent))
                {
                    return NotFound("Project has no geometry data to render.");
                }

                string[] mpdLines = mpdContent.Split('\n');
                string fileName = "project.mpd";

                // 120-second render timeout for large models
                using var renderCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                renderCts.CancelAfter(TimeSpan.FromSeconds(120));

                byte[] result;
                string contentType;

                RendererType rendererType = string.Equals(renderer, "raytrace", System.StringComparison.OrdinalIgnoreCase)
                    ? RendererType.RayTracer
                    : RendererType.Rasterizer;

                if (format == "svg")
                {
                    string svg = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToSvg(mpdLines, fileName, width, height, -1, elevation, azimuth, renderEdges, smoothShading);
                    }, renderCts.Token);

                    result = System.Text.Encoding.UTF8.GetBytes(svg);
                    contentType = "image/svg+xml";
                }
                else if (format == "webp")
                {
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToWebP(mpdLines, fileName, width, height, -1, elevation, azimuth,
                            renderEdges, smoothShading, aaMode, backgroundHex, gradientTopHex, gradientBottomHex, quality,
                            enablePbr: enablePbr, exposure: exposure, aperture: aperture, zoom: zoom);
                    }, renderCts.Token);
                    contentType = "image/webp";
                }
                else if (format == "gif")
                {
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderTurntableGif(mpdLines, fileName, width, height, -1, 36,
                            elevation, 80, renderEdges, smoothShading);
                    }, renderCts.Token);
                    contentType = "image/gif";
                }
                else if (step > 0)
                {
                    // PNG — step-aware render (renders all parts up to the selected step)
                    // Step param is 1-based from the client UI. The server's GeometryResolver
                    // counts the pre-STEP content as step 0, but Three.js LDrawLoader groups
                    // do not — offset by an additional 1 to align.
                    int stepIndex = Math.Max(0, step - 2);
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderStep(mpdLines, fileName, stepIndex, width, height, -1,
                            elevation, azimuth, renderEdges, smoothShading, aaMode);
                    }, renderCts.Token);
                    contentType = "image/png";
                }
                else
                {
                    // PNG (default — full model)
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToPng(mpdLines, fileName, width, height, -1, elevation, azimuth,
                            renderEdges, smoothShading, aaMode, backgroundHex, gradientTopHex, gradientBottomHex,
                            rendererType: rendererType, enablePbr: enablePbr, exposure: exposure, aperture: aperture, zoom: zoom);
                    }, renderCts.Token);
                    contentType = "image/png";
                }

                if (result == null || result.Length == 0)
                {
                    return NotFound("Project geometry could not be rendered — the project may not have any placed bricks.");
                }

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.ReadEntity,
                    $"MOC render — Project id={projectId}, {width}x{height}, format={format}, size={result.Length}",
                    projectId.ToString());

                return File(result, contentType);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Render timed out or was cancelled.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "MOC render error for project {Id}", projectId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MOC render failed for project {Id}", projectId);
                return Problem("An error occurred while rendering the project.");
            }
        }


        /// <summary>
        /// Lazy-initialise the RenderService with the configured LDraw data path.
        /// Same pattern as PartRendererController.
        /// </summary>
        private void EnsureRenderService(string dataPath)
        {
            if (_renderService == null)
            {
                _renderService = new RenderService(dataPath);
            }
        }
    }
}
