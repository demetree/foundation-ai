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
            width = Math.Clamp(width, 64, 1024);
            height = Math.Clamp(height, 64, 1024);
            elevation = Math.Clamp(elevation, -90f, 90f);
            azimuth = Math.Clamp(azimuth, -360f, 360f);
            quality = Math.Clamp(quality, 1, 100);
            format = (format ?? "png").ToLowerInvariant();
            antiAlias = (antiAlias ?? "none").ToLowerInvariant();

            // Parse anti-alias mode
            AntiAliasMode aaMode = AntiAliasMode.None;
            if (antiAlias == "2x") aaMode = AntiAliasMode.SSAA2x;
            else if (antiAlias == "4x") aaMode = AntiAliasMode.SSAA4x;

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
                byte[] result;
                string contentType;

                if (format == "svg")
                {
                    string svg = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToSvg(datPath, width, height, colourCode, elevation, azimuth, renderEdges, smoothShading);
                    }, cancellationToken);

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
                    }, cancellationToken);
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
                    }, cancellationToken);
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
                byte[] gif = await Task.Run(() =>
                {
                    string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                    EnsureRenderService(dataPath);
                    return _renderService.RenderTurntableGif(datPath, width, height, colourCode, frameCount,
                        elevation, frameDelayMs, renderEdges, smoothShading);
                }, cancellationToken);

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

            width = Math.Clamp(width, 64, 1024);
            height = Math.Clamp(height, 64, 1024);
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
                byte[] png = await Task.Run(() =>
                {
                    string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                    EnsureRenderService(dataPath);
                    return _renderService.RenderExplodedView(datPath, explosionFactor, width, height, colourCode,
                        elevation, azimuth, renderEdges, smoothShading);
                }, cancellationToken);

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
        //  Upload + Render Endpoint
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
        [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB max
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
            width = Math.Clamp(width, 64, 1024);
            height = Math.Clamp(height, 64, 1024);
            elevation = Math.Clamp(elevation, -90f, 90f);
            azimuth = Math.Clamp(azimuth, -360f, 360f);
            quality = Math.Clamp(quality, 1, 100);
            format = (format ?? "png").ToLowerInvariant();
            antiAlias = (antiAlias ?? "none").ToLowerInvariant();

            AntiAliasMode aaMode = AntiAliasMode.None;
            if (antiAlias == "2x") aaMode = AntiAliasMode.SSAA2x;
            else if (antiAlias == "4x") aaMode = AntiAliasMode.SSAA4x;

            // Save to temp
            string tempDir = Path.Combine(Path.GetTempPath(), "bmc-ldraw-uploads");
            Directory.CreateDirectory(tempDir);
            string tempFile = Path.Combine(tempDir, $"{Guid.NewGuid()}{ext}");

            try
            {
                using (var stream = new FileStream(tempFile, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                byte[] result;
                string contentType;

                if (format == "svg")
                {
                    string svg = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToSvg(tempFile, width, height, colourCode, elevation, azimuth, renderEdges, smoothShading);
                    }, cancellationToken);

                    result = System.Text.Encoding.UTF8.GetBytes(svg);
                    contentType = "image/svg+xml";
                }
                else if (format == "webp")
                {
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToWebP(tempFile, width, height, colourCode, elevation, azimuth,
                            renderEdges, smoothShading, aaMode, backgroundHex, gradientTopHex, gradientBottomHex, quality);
                    }, cancellationToken);
                    contentType = "image/webp";
                }
                else if (format == "gif")
                {
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderTurntableGif(tempFile, width, height, colourCode, 36,
                            elevation, 80, renderEdges, smoothShading);
                    }, cancellationToken);
                    contentType = "image/gif";
                }
                else
                {
                    // PNG (default)
                    result = await Task.Run(() =>
                    {
                        string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                        EnsureRenderService(dataPath);
                        return _renderService.RenderToPng(tempFile, width, height, colourCode, elevation, azimuth,
                            renderEdges, smoothShading, aaMode, backgroundHex, gradientTopHex, gradientBottomHex);
                    }, cancellationToken);
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
            finally
            {
                // Always clean up temp file
                if (System.IO.File.Exists(tempFile))
                {
                    try { System.IO.File.Delete(tempFile); } catch { /* best-effort cleanup */ }
                }
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
}
