using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.BMC.Database;
using BMC.LDraw.Render;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Server-side LDraw part rendering controller.
    /// Renders parts to PNG images using the software rasterizer and returns them as image/png responses.
    /// Also provides lightweight search and colour-lookup endpoints for the Part Renderer UI.
    /// </summary>
    public class PartRendererController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _db;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PartRendererController> _logger;

        /// <summary>
        /// Lazy-initialised RenderService — created on first render request.
        /// Shared across requests for the lifetime of each controller instance.
        /// </summary>
        private RenderService _renderService;

        public PartRendererController(
            BMCContext db,
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<PartRendererController> logger
        ) : base("BMC", "PartRenderer")
        {
            _db = db;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
        }


        // ════════════════════════════════════════════════════════════════
        //  Render Endpoint
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/part-renderer/render
        ///
        /// Renders a part and returns it in the requested format (PNG, WebP, or SVG).
        /// The image is cached in memory for 10 minutes keyed by all parameters.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/render")]
        public async Task<IActionResult> Render(
            string partNumber,
            int colourCode = 4,
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
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(partNumber))
            {
                return BadRequest("partNumber is required.");
            }

            // Normalise & clamp
            width = Math.Clamp(width, 64, 3840);
            height = Math.Clamp(height, 64, 3840);
            elevation = Math.Clamp(elevation, -90f, 90f);
            azimuth = Math.Clamp(azimuth, -360f, 360f);
            quality = Math.Clamp(quality, 1, 100);
            format = (format ?? "png").ToLowerInvariant();
            antiAlias = (antiAlias ?? "none").ToLowerInvariant();

            // Parse anti-alias mode
            AntiAliasMode aaMode = AntiAliasMode.None;
            if (antiAlias == "2x") aaMode = AntiAliasMode.SSAA2x;
            else if (antiAlias == "4x") aaMode = AntiAliasMode.SSAA4x;

            // Safety: clamp SSAA to 2× for large resolutions to prevent memory exhaustion
            if (aaMode == AntiAliasMode.SSAA4x && (width > 2560 || height > 2560))
                aaMode = AntiAliasMode.SSAA2x;

            // Check cache
            string cacheKey = $"part-render:{partNumber}:{colourCode}:{width}x{height}:{elevation}:{azimuth}:{renderEdges}:{smoothShading}:{antiAlias}:{format}:{quality}:{backgroundHex}:{gradientTopHex}:{gradientBottomHex}";
            if (_cache.TryGetValue(cacheKey, out byte[] cachedBytes))
            {
                string cachedType = format == "webp" ? "image/webp" : format == "svg" ? "image/svg+xml" : "image/png";
                return File(cachedBytes, cachedType);
            }

            // Resolve part file
            string datPath = await ResolvePartFile(partNumber, cancellationToken);
            if (datPath == null) return NotFound($"Part '{partNumber}' not found.");

            try
            {
                // 60-second render timeout
                using var renderCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                renderCts.CancelAfter(TimeSpan.FromSeconds(60));
                var renderToken = renderCts.Token;

                byte[] result;
                string contentType;

                if (format == "svg")
                {
                    string svg = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToSvg(datPath, width, height, colourCode, elevation, azimuth, renderEdges, smoothShading);
                    }, renderToken);

                    result = System.Text.Encoding.UTF8.GetBytes(svg);
                    contentType = "image/svg+xml";
                }
                else if (format == "webp")
                {
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToWebP(datPath, width, height, colourCode, elevation, azimuth,
                            renderEdges, smoothShading, aaMode, backgroundHex, gradientTopHex, gradientBottomHex, quality);
                    }, renderToken);
                    contentType = "image/webp";
                }
                else
                {
                    // PNG (default)
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToPng(datPath, width, height, colourCode, elevation, azimuth,
                            renderEdges, smoothShading, aaMode, backgroundHex, gradientTopHex, gradientBottomHex);
                    }, renderToken);
                    contentType = "image/png";
                }

                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
                return File(result, contentType);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering part {PartNumber} with colour {ColourCode}", partNumber, colourCode);
                return StatusCode(500, "Error rendering part.");
            }
        }


        // ════════════════════════════════════════════════════════════════
        //  Turntable GIF Endpoint
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/part-renderer/turntable
        ///
        /// Renders a 360° turntable animation as an animated GIF.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/turntable")]
        public async Task<IActionResult> Turntable(
            string partNumber,
            int colourCode = 4,
            int width = 256,
            int height = 256,
            int frameCount = 36,
            float elevation = 30f,
            int frameDelayMs = 80,
            bool renderEdges = true,
            bool smoothShading = true,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            width = Math.Clamp(width, 64, 512);
            height = Math.Clamp(height, 64, 512);
            frameCount = Math.Clamp(frameCount, 4, 72);
            frameDelayMs = Math.Clamp(frameDelayMs, 20, 500);

            string cacheKey = $"part-turntable:{partNumber}:{colourCode}:{width}x{height}:{frameCount}:{elevation}:{frameDelayMs}:{renderEdges}:{smoothShading}";
            if (_cache.TryGetValue(cacheKey, out byte[] cached))
            {
                return File(cached, "image/gif");
            }

            string datPath = await ResolvePartFile(partNumber, cancellationToken);
            if (datPath == null) return NotFound($"Part '{partNumber}' not found.");

            try
            {
                using var renderCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                renderCts.CancelAfter(TimeSpan.FromSeconds(60));

                byte[] gif = await Task.Run(() =>
                {
                    string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                    EnsureRenderService(dataPath);
                    return _renderService.RenderTurntableGif(datPath, width, height, colourCode, frameCount,
                        elevation, frameDelayMs, renderEdges, smoothShading);
                }, renderCts.Token);

                _cache.Set(cacheKey, gif, TimeSpan.FromMinutes(10));
                return File(gif, "image/gif");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering turntable for {PartNumber}", partNumber);
                return StatusCode(500, "Error rendering turntable.");
            }
        }


        // ════════════════════════════════════════════════════════════════
        //  Exploded View Endpoint
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/part-renderer/exploded
        ///
        /// Renders an exploded view with parts pushed radially outward.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/exploded")]
        public async Task<IActionResult> Exploded(
            string partNumber,
            int colourCode = 4,
            int width = 512,
            int height = 512,
            float elevation = 30f,
            float azimuth = -45f,
            float explosionFactor = 1.0f,
            bool renderEdges = true,
            bool smoothShading = true,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            width = Math.Clamp(width, 64, 3840);
            height = Math.Clamp(height, 64, 3840);
            explosionFactor = Math.Clamp(explosionFactor, 0.1f, 5.0f);

            string cacheKey = $"part-exploded:{partNumber}:{colourCode}:{width}x{height}:{elevation}:{azimuth}:{explosionFactor}:{renderEdges}:{smoothShading}";
            if (_cache.TryGetValue(cacheKey, out byte[] cached))
            {
                return File(cached, "image/png");
            }

            string datPath = await ResolvePartFile(partNumber, cancellationToken);
            if (datPath == null) return NotFound($"Part '{partNumber}' not found.");

            try
            {
                using var renderCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                renderCts.CancelAfter(TimeSpan.FromSeconds(60));

                byte[] png = await Task.Run(() =>
                {
                    string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                    EnsureRenderService(dataPath);
                    return _renderService.RenderExplodedView(datPath, explosionFactor, width, height, colourCode,
                        elevation, azimuth, renderEdges, smoothShading);
                }, renderCts.Token);

                _cache.Set(cacheKey, png, TimeSpan.FromMinutes(10));
                return File(png, "image/png");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering exploded view for {PartNumber}", partNumber);
                return StatusCode(500, "Error rendering exploded view.");
            }
        }


        // ════════════════════════════════════════════════════════════════
        //  Build Step Endpoints
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/part-renderer/step-count
        ///
        /// Returns the number of STEP directives in an LDraw part or model.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/step-count")]
        public async Task<IActionResult> StepCount(
            string partNumber,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            string cacheKey = $"part-stepcount:{partNumber}";
            if (_cache.TryGetValue(cacheKey, out int cachedCount))
            {
                return Ok(new { stepCount = cachedCount });
            }

            string datPath = await ResolvePartFile(partNumber, cancellationToken);
            if (datPath == null) return NotFound($"Part '{partNumber}' not found.");

            try
            {
                int count = await Task.Run(() =>
                {
                    string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                    EnsureRenderService(dataPath);
                    return _renderService.GetStepCount(datPath);
                }, cancellationToken);

                _cache.Set(cacheKey, count, TimeSpan.FromMinutes(30));
                return Ok(new { stepCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting step count for {PartNumber}", partNumber);
                return StatusCode(500, "Error getting step count.");
            }
        }

        /// <summary>
        /// GET /api/part-renderer/render-step
        ///
        /// Renders a single build step (cumulative) as a PNG.
        /// Step indices are 0-based.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/render-step")]
        public async Task<IActionResult> RenderStep(
            string partNumber,
            int stepIndex = 0,
            int colourCode = 4,
            int width = 512,
            int height = 512,
            float elevation = 30f,
            float azimuth = -45f,
            bool renderEdges = true,
            bool smoothShading = true,
            string antiAlias = "none",
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            width = Math.Clamp(width, 64, 3840);
            height = Math.Clamp(height, 64, 3840);
            stepIndex = Math.Max(0, stepIndex);

            AntiAliasMode aaMode = AntiAliasMode.None;
            if (antiAlias == "2x") aaMode = AntiAliasMode.SSAA2x;
            else if (antiAlias == "4x") aaMode = AntiAliasMode.SSAA4x;

            if (aaMode == AntiAliasMode.SSAA4x && (width > 2560 || height > 2560))
                aaMode = AntiAliasMode.SSAA2x;

            string cacheKey = $"part-step:{partNumber}:{stepIndex}:{colourCode}:{width}x{height}:{elevation}:{azimuth}:{renderEdges}:{smoothShading}:{antiAlias}";
            if (_cache.TryGetValue(cacheKey, out byte[] cached))
            {
                return File(cached, "image/png");
            }

            string datPath = await ResolvePartFile(partNumber, cancellationToken);
            if (datPath == null) return NotFound($"Part '{partNumber}' not found.");

            try
            {
                using var renderCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                renderCts.CancelAfter(TimeSpan.FromSeconds(60));

                byte[] png = await Task.Run(() =>
                {
                    string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                    EnsureRenderService(dataPath);
                    return _renderService.RenderStep(datPath, stepIndex, width, height, colourCode,
                        elevation, azimuth, renderEdges, smoothShading, aaMode);
                }, renderCts.Token);

                _cache.Set(cacheKey, png, TimeSpan.FromMinutes(10));
                return File(png, "image/png");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering step {Step} for {PartNumber}", stepIndex, partNumber);
                return StatusCode(500, "Error rendering step.");
            }
        }


        // ════════════════════════════════════════════════════════════════
        //  Batch Render Endpoint
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/part-renderer/batch-render
        ///
        /// Renders a part at multiple sizes and returns a ZIP archive of PNGs.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/batch-render")]
        public async Task<IActionResult> BatchRender(
            [FromBody] BatchRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (request?.Sizes == null || request.Sizes.Count == 0 || request.Sizes.Count > 20)
                return BadRequest("Provide between 1 and 20 sizes.");

            string datPath = await ResolvePartFile(request.PartNumber, cancellationToken);
            if (datPath == null) return NotFound($"Part '{request.PartNumber}' not found.");

            // Parse options
            AntiAliasMode aaMode = AntiAliasMode.None;
            if (request.AntiAlias == "2x") aaMode = AntiAliasMode.SSAA2x;
            else if (request.AntiAlias == "4x") aaMode = AntiAliasMode.SSAA4x;

            try
            {
                using var renderCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                renderCts.CancelAfter(TimeSpan.FromSeconds(120));  // 2 minutes for batch

                using var zipStream = new System.IO.MemoryStream();
                using (var zip = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var size in request.Sizes)
                    {
                        int w = Math.Clamp(size.Width, 64, 3840);
                        int h = Math.Clamp(size.Height, 64, 3840);

                        // Clamp SSAA for large sizes
                        AntiAliasMode sizeAA = aaMode;
                        if (sizeAA == AntiAliasMode.SSAA4x && (w > 2560 || h > 2560))
                            sizeAA = AntiAliasMode.SSAA2x;

                        byte[] png = await Task.Run(() =>
                        {
                            string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                            EnsureRenderService(dataPath);
                            return _renderService.RenderToPng(datPath, w, h, request.ColourCode,
                                request.Elevation, request.Azimuth, request.RenderEdges, request.SmoothShading,
                                sizeAA, request.BackgroundHex, request.GradientTopHex, request.GradientBottomHex);
                        }, renderCts.Token);

                        string entryName = $"{request.PartNumber}_{w}x{h}.png";
                        var entry = zip.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Fastest);
                        using var entryStream = entry.Open();
                        await entryStream.WriteAsync(png, 0, png.Length, renderCts.Token);
                    }
                }

                zipStream.Position = 0;
                var zipBytes = zipStream.ToArray();
                return File(zipBytes, "application/zip", $"{request.PartNumber}_renders.zip");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Batch render cancelled or timed out.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch rendering {PartNumber}", request.PartNumber);
                return StatusCode(500, "Error during batch render.");
            }
        }


        // ════════════════════════════════════════════════════════════════
        //  Upload + Render Endpoint
        // ════════════════════════════════════════════════════════════════
        //  Upload Build Step Endpoints
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/part-renderer/step-count-upload
        ///
        /// Returns the number of STEP directives in an uploaded LDraw file.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/step-count-upload")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> StepCountUpload(
            Microsoft.AspNetCore.Http.IFormFile file,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string[] lines;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string content = await reader.ReadToEndAsync();
                lines = content.Split('\n');
            }

            try
            {
                int count = await Task.Run(() =>
                {
                    string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                    EnsureRenderService(dataPath);
                    return _renderService.GetStepCount(lines, file.FileName);
                }, cancellationToken);

                return Ok(new { stepCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting step count for uploaded file {FileName}", file.FileName);
                return StatusCode(500, "Error getting step count.");
            }
        }


        /// <summary>
        /// POST /api/part-renderer/render-step-upload
        ///
        /// Renders a single build step from an uploaded LDraw file as PNG.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/render-step-upload")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> RenderStepUpload(
            Microsoft.AspNetCore.Http.IFormFile file,
            int stepIndex = 0,
            int colourCode = 4,
            int width = 512,
            int height = 512,
            float elevation = 30f,
            float azimuth = -45f,
            bool renderEdges = true,
            bool smoothShading = true,
            string antiAlias = "none",
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            width = Math.Clamp(width, 64, 3840);
            height = Math.Clamp(height, 64, 3840);
            stepIndex = Math.Max(0, stepIndex);

            AntiAliasMode aaMode = AntiAliasMode.None;
            if (antiAlias == "2x") aaMode = AntiAliasMode.SSAA2x;
            else if (antiAlias == "4x") aaMode = AntiAliasMode.SSAA4x;

            if (aaMode == AntiAliasMode.SSAA4x && (width > 2560 || height > 2560))
                aaMode = AntiAliasMode.SSAA2x;

            string[] lines;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string content = await reader.ReadToEndAsync();
                lines = content.Split('\n');
            }

            string fileName = file.FileName;

            try
            {
                using var renderCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                renderCts.CancelAfter(TimeSpan.FromSeconds(60));

                byte[] png = await Task.Run(() =>
                {
                    string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                    EnsureRenderService(dataPath);
                    return _renderService.RenderStep(lines, fileName, stepIndex, width, height, colourCode,
                        elevation, azimuth, renderEdges, smoothShading, aaMode);
                }, renderCts.Token);

                return File(png, "image/png");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering step {Step} for uploaded file {FileName}", stepIndex, file.FileName);
                return StatusCode(500, "Error rendering step.");
            }
        }


        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/part-renderer/render-upload
        ///
        /// Accepts an uploaded .dat, .ldr, or .mpd file and renders it
        /// with the same options as the standard render endpoint.
        /// No caching — uploads are transient.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/render-upload")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB max
        public async Task<IActionResult> RenderUpload(
            Microsoft.AspNetCore.Http.IFormFile file,
            int colourCode = 4,
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
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided.");
            }

            string ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (ext != ".dat" && ext != ".ldr" && ext != ".mpd")
            {
                return BadRequest("Unsupported file type. Accepted formats: .dat, .ldr, .mpd");
            }

            // Normalise & clamp
            width = Math.Clamp(width, 64, 3840);
            height = Math.Clamp(height, 64, 3840);
            elevation = Math.Clamp(elevation, -90f, 90f);
            azimuth = Math.Clamp(azimuth, -360f, 360f);
            quality = Math.Clamp(quality, 1, 100);
            format = (format ?? "png").ToLowerInvariant();
            antiAlias = (antiAlias ?? "none").ToLowerInvariant();

            AntiAliasMode aaMode = AntiAliasMode.None;
            if (antiAlias == "2x") aaMode = AntiAliasMode.SSAA2x;
            else if (antiAlias == "4x") aaMode = AntiAliasMode.SSAA4x;

            // Safety: clamp SSAA to 2× for large resolutions
            if (aaMode == AntiAliasMode.SSAA4x && (width > 2560 || height > 2560))
                aaMode = AntiAliasMode.SSAA2x;

            // Read uploaded file into lines (in-memory, no temp file)
            string[] lines;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string content = await reader.ReadToEndAsync();
                lines = content.Split('\n');
            }

            string fileName = file.FileName;

            try
            {
                // 60-second render timeout
                using var renderCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                renderCts.CancelAfter(TimeSpan.FromSeconds(60));
                var renderToken = renderCts.Token;

                byte[] result;
                string contentType;

                if (format == "svg")
                {
                    string svg = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToSvg(lines, fileName, width, height, colourCode, elevation, azimuth, renderEdges, smoothShading);
                    }, renderToken);

                    result = System.Text.Encoding.UTF8.GetBytes(svg);
                    contentType = "image/svg+xml";
                }
                else if (format == "webp")
                {
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToWebP(lines, fileName, width, height, colourCode, elevation, azimuth,
                            renderEdges, smoothShading, aaMode, backgroundHex, gradientTopHex, gradientBottomHex, quality);
                    }, renderToken);
                    contentType = "image/webp";
                }
                else if (format == "gif")
                {
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderTurntableGif(lines, fileName, width, height, colourCode, 36,
                            elevation, 80, renderEdges, smoothShading);
                    }, renderToken);
                    contentType = "image/gif";
                }
                else
                {
                    // PNG (default)
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToPng(lines, fileName, width, height, colourCode, elevation, azimuth,
                            renderEdges, smoothShading, aaMode, backgroundHex, gradientTopHex, gradientBottomHex);
                    }, renderToken);
                    contentType = "image/png";
                }

                return File(result, contentType);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering uploaded file {FileName}", file.FileName);
                return StatusCode(500, "Error rendering uploaded file.");
            }
        }


        // ════════════════════════════════════════════════════════════════
        //  Search Endpoint
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/part-renderer/search
        ///
        /// Lightweight part search for the renderer UI.
        /// Returns up to 20 results matching across title, name, category, and keywords.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/search")]
        public async Task<IActionResult> Search(string q, int take = 20, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new object[0]);
            }

            take = Math.Clamp(take, 1, 50);
            string pattern = $"%{q}%";

            var results = await _db.BrickParts
                .Where(p => p.active && !p.deleted &&
                    (EF.Functions.Like(p.ldrawTitle, pattern) ||
                     EF.Functions.Like(p.name, pattern) ||
                     EF.Functions.Like(p.ldrawCategory, pattern) ||
                     EF.Functions.Like(p.keywords, pattern) ||
                     EF.Functions.Like(p.ldrawPartId, pattern)))
                .OrderBy(p => p.ldrawTitle)
                .Take(take)
                .Select(p => new
                {
                    p.id,
                    p.name,
                    p.ldrawPartId,
                    p.ldrawTitle,
                    p.ldrawCategory
                })
                .ToListAsync(cancellationToken);

            return Ok(results);
        }


        // ════════════════════════════════════════════════════════════════
        //  Colours Endpoint
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/part-renderer/colours/{partNumber}
        ///
        /// Returns the known colours for a given part — used to populate the colour picker.
        /// Merges two data sources for maximum coverage:
        ///   1. BrickPartColours — direct part-colour mappings
        ///   2. LegoSetParts — colours derived from actual set appearances
        /// This matches the catalog-part-detail pattern and avoids empty results
        /// for parts that only have colour data via set appearances.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/colours/{partNumber}")]
        public async Task<IActionResult> Colours(string partNumber, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(partNumber))
            {
                return BadRequest("partNumber is required.");
            }

            var part = await _db.BrickParts
                .Where(p => p.active && !p.deleted &&
                    (p.name == partNumber || p.ldrawPartId == partNumber || p.ldrawPartId == partNumber + ".dat"))
                .Select(p => new { p.id })
                .FirstOrDefaultAsync(cancellationToken);

            if (part == null)
            {
                return NotFound($"Part '{partNumber}' not found.");
            }

            //
            // Source 1: BrickPartColour records (direct part-colour mappings)
            //
            var bpcColours = await _db.BrickPartColours
                .Where(bpc => bpc.brickPartId == part.id && bpc.active && !bpc.deleted)
                .Select(bpc => bpc.brickColour)
                .Where(c => c != null)
                .Select(c => new
                {
                    c.id,
                    c.ldrawColourCode,
                    c.name,
                    c.hexRgb,
                    c.isTransparent
                })
                .ToListAsync(cancellationToken);

            //
            // Source 2: LegoSetPart records — colours from actual set appearances.
            // This is often a richer source than the direct BrickPartColour table.
            //
            var setPartColours = await _db.LegoSetParts
                .Where(sp => sp.brickPartId == part.id && sp.active && !sp.deleted)
                .Select(sp => sp.brickColour)
                .Where(c => c != null)
                .Select(c => new
                {
                    c.id,
                    c.ldrawColourCode,
                    c.name,
                    c.hexRgb,
                    c.isTransparent
                })
                .ToListAsync(cancellationToken);

            //
            // Merge and deduplicate by colour id, matching the catalog-part-detail pattern
            //
            var merged = bpcColours
                .Concat(setPartColours)
                .GroupBy(c => c.id)
                .Select(g => g.First())
                .OrderBy(c => c.name)
                .Select(c => new
                {
                    c.ldrawColourCode,
                    c.name,
                    c.hexRgb,
                    c.isTransparent
                })
                .ToList();

            return Ok(merged);
        }


        // ════════════════════════════════════════════════════════════════
        //  All Colours Endpoint
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/part-renderer/all-colours
        ///
        /// Returns every colour in the BrickColours table — used for the "All Colours"
        /// mode in the renderer UI, allowing users to render in any colour regardless
        /// of known part-colour associations.  Cached for 10 minutes.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/part-renderer/all-colours")]
        public async Task<IActionResult> AllColours(CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            const string cacheKey = "part-renderer:all-colours";

            if (_cache.TryGetValue(cacheKey, out object cachedColours))
            {
                return Ok(cachedColours);
            }

            var colours = await _db.BrickColours
                .Where(c => c.active && !c.deleted)
                .OrderBy(c => c.name)
                .Select(c => new
                {
                    c.ldrawColourCode,
                    c.name,
                    c.hexRgb,
                    c.isTransparent
                })
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, colours, TimeSpan.FromMinutes(10));

            return Ok(colours);
        }


        // ════════════════════════════════════════════════════════════════
        //  Private Helpers
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Look up a part in the database and resolve the .dat file path.
        /// Returns null if not found.
        /// </summary>
        private async Task<string> ResolvePartFile(string partNumber, CancellationToken cancellationToken)
        {
            var part = await _db.BrickParts
                .Where(p => p.active && !p.deleted &&
                    (p.name == partNumber || p.ldrawPartId == partNumber || p.ldrawPartId == partNumber + ".dat"))
                .Select(p => new { p.ldrawPartId })
                .FirstOrDefaultAsync(cancellationToken);

            if (part == null) return null;

            string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
            if (string.IsNullOrEmpty(dataPath)) return null;

            string ldrawId = part.ldrawPartId;
            if (!ldrawId.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
            {
                ldrawId += ".dat";
            }

            return FindPartFile(dataPath, ldrawId);
        }

        private void EnsureRenderService(string dataPath)
        {
            if (_renderService == null)
            {
                _renderService = new RenderService(dataPath);
            }
        }

        /// <summary>
        /// Find a .dat file within the LDraw library directory structure.
        /// </summary>
        private static string FindPartFile(string libraryPath, string fileName)
        {
            string[] searchDirs = new string[]
            {
                Path.Combine(libraryPath, "parts"),
                Path.Combine(libraryPath, "p"),
                Path.Combine(libraryPath, "models"),
                Path.Combine(libraryPath, "parts", "s"),
                libraryPath,
            };

            foreach (string dir in searchDirs)
            {
                string fullPath = Path.Combine(dir, fileName);
                if (System.IO.File.Exists(fullPath)) return fullPath;
            }

            return null;
        }
    }


    public class BatchRenderRequest
    {
        public string PartNumber { get; set; }
        public int ColourCode { get; set; } = 4;
        public float Elevation { get; set; } = 30f;
        public float Azimuth { get; set; } = -45f;
        public bool RenderEdges { get; set; } = true;
        public bool SmoothShading { get; set; } = true;
        public string AntiAlias { get; set; } = "none";
        public string BackgroundHex { get; set; } = "";
        public string GradientTopHex { get; set; } = "";
        public string GradientBottomHex { get; set; } = "";
        public List<BatchRenderSize> Sizes { get; set; }
    }

    public class BatchRenderSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
