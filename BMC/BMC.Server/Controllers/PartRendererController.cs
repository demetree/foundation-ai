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
        /// Renders a part to a PNG image and returns it as an image/png response.
        /// The image is cached in memory for 10 minutes keyed by part+colour+size.
        /// </summary>
        /// <param name="partNumber">Part name or LDraw part ID (e.g. "3001")</param>
        /// <param name="colourCode">LDraw colour code (default: 4 = red)</param>
        /// <param name="width">Image width in pixels (max 1024)</param>
        /// <param name="height">Image height in pixels (max 1024)</param>
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

            // Clamp dimensions and angles
            width = Math.Clamp(width, 64, 1024);
            height = Math.Clamp(height, 64, 1024);
            elevation = Math.Clamp(elevation, -90f, 90f);
            azimuth = Math.Clamp(azimuth, -360f, 360f);

            // Check cache
            string cacheKey = $"part-render:{partNumber}:{colourCode}:{width}x{height}:{elevation}:{azimuth}";
            if (_cache.TryGetValue(cacheKey, out byte[] cachedPng))
            {
                return File(cachedPng, "image/png");
            }

            // Look up the part in the database
            var part = await _db.BrickParts
                .Where(p => p.active && !p.deleted &&
                    (p.name == partNumber || p.ldrawPartId == partNumber || p.ldrawPartId == partNumber + ".dat"))
                .Select(p => new { p.ldrawPartId })
                .FirstOrDefaultAsync(cancellationToken);

            if (part == null)
            {
                return NotFound($"Part '{partNumber}' not found.");
            }

            // Resolve the .dat file
            string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
            if (string.IsNullOrEmpty(dataPath))
            {
                _logger.LogError("LDraw:DataPath is not configured.");
                return StatusCode(500, "LDraw data path is not configured.");
            }

            string ldrawId = part.ldrawPartId;
            if (!ldrawId.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
            {
                ldrawId += ".dat";
            }

            string datPath = FindPartFile(dataPath, ldrawId);
            if (datPath == null)
            {
                return NotFound($"LDraw file not found for part '{partNumber}' ({ldrawId}).");
            }

            try
            {
                // Render on a background thread to avoid blocking the request thread
                byte[] pngBytes = await Task.Run(() =>
                {
                    EnsureRenderService(dataPath);
                    return _renderService.RenderToPng(datPath, width, height, colourCode, elevation, azimuth);
                }, cancellationToken);

                // Cache for 10 minutes
                _cache.Set(cacheKey, pngBytes, TimeSpan.FromMinutes(10));

                return File(pngBytes, "image/png");
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
