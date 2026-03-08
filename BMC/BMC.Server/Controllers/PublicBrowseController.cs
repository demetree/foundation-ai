using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Auditor;
using Foundation.BMC.Database;
using Foundation.BMC.Services;
using Foundation.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    ///
    /// Public browse controller — serves read-only, anonymous data for the
    /// publicly accessible browse features: Parts Catalog, Set Explorer,
    /// Set Detail, Theme Explorer, Minifig Gallery, Colour Library,
    /// and Parts Universe.
    ///
    /// This controller does NOT extend SecureWebAPIController, keeping it
    /// fully outside the Foundation enterprise authorization pipeline.
    /// It does, however, create audit events via AuditEngine.Instance so
    /// that anonymous usage analytics are available in Foundation monitoring.
    ///
    /// All endpoints are heavily cached in memory and require no authentication.
    ///
    /// </summary>
    [ApiController]
    [Route("api/public/browse")]
    [AllowAnonymous]
    public class PublicBrowseController : ControllerBase
    {
        private readonly BMCContext _context;
        private readonly SetExplorerService _setExplorerService;
        private readonly MinifigGalleryService _minifigGalleryService;
        private readonly PartsUniverseService _partsUniverseService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PublicBrowseController> _logger;

        private static readonly TimeSpan ShortCache = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan MediumCache = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan LongCache = TimeSpan.FromHours(1);

        private DateTime _auditStartTime;


        public PublicBrowseController(
            BMCContext context,
            SetExplorerService setExplorerService,
            MinifigGalleryService minifigGalleryService,
            PartsUniverseService partsUniverseService,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<PublicBrowseController> logger
        )
        {
            _context = context;
            _setExplorerService = setExplorerService;
            _minifigGalleryService = minifigGalleryService;
            _partsUniverseService = partsUniverseService;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;

            _context.Database.SetCommandTimeout(30);
        }


        #region Audit helpers

        /// <summary>
        /// Records the start of request processing for audit timing.
        /// </summary>
        private void StartAuditClock()
        {
            _auditStartTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates an audit event via AuditEngine.Instance for anonymous usage tracking.
        /// Uses user="Anonymous" and module="BMC" / entity="PublicBrowse" so that
        /// Foundation monitoring tools can analyse anonymous traffic patterns.
        /// </summary>
        private async Task CreateAnonymousAuditEventAsync(AuditEngine.AuditType auditType, string message)
        {
            try
            {
                string clientIp = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
                string userAgent = HttpContext?.Request?.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
                string resource = HttpContext?.Request?.Path.Value ?? "Unknown";

                await AuditEngine.Instance.CreateAuditEventAsync(
                    _auditStartTime,
                    DateTime.UtcNow,
                    true,
                    AuditEngine.AuditAccessType.WebBrowser,
                    auditType,
                    "Anonymous",                     // user
                    $"anon-{clientIp}",              // session
                    clientIp,                        // source
                    userAgent,                       // user agent
                    "BMC",                           // module
                    "PublicBrowse",                  // module entity
                    resource,                        // resource
                    Environment.MachineName,         // host system
                    null,                            // primary key
                    null,                            // thread id
                    message,                         // message
                    null,                            // entity before state
                    null,                            // entity after state
                    null                             // error messages
                );
            }
            catch (Exception ex)
            {
                // Never let audit failures break the public browse experience
                _logger.LogWarning(ex, "[PublicBrowseController] Audit event creation failed for: {Message}", message);
            }
        }

        #endregion


        #region Parts Catalog

        /// <summary>
        /// GET /api/public/browse/catalog
        ///
        /// Paginated, filterable parts catalog for anonymous users.
        /// Mirrors the authenticated PartsCatalogController.GetCatalogParts endpoint.
        /// </summary>
        [HttpGet("catalog")]
        public async Task<IActionResult> GetCatalogParts(
            string search = null,
            int? categoryId = null,
            int? partTypeId = null,
            int pageSize = 48,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            if (pageSize <= 0) pageSize = 48;
            if (pageSize > 200) pageSize = 200;
            if (pageNumber < 1) pageNumber = 1;

            string cacheKey = $"public:catalog:{search ?? ""}:{categoryId}:{partTypeId}:{pageSize}:{pageNumber}";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            IQueryable<BrickPart> query = _context.BrickParts
                .Where(bp => bp.geometryOriginalFileName != null && bp.active == true && bp.deleted == false);

            if (categoryId.HasValue)
                query = query.Where(bp => bp.brickCategoryId == categoryId.Value);

            if (partTypeId.HasValue)
                query = query.Where(bp => bp.partTypeId == partTypeId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string lower = search.ToLower();
                query = query.Where(bp =>
                    bp.name.ToLower().Contains(lower) ||
                    bp.ldrawPartId.ToLower().Contains(lower) ||
                    (bp.ldrawTitle != null && bp.ldrawTitle.ToLower().Contains(lower)) ||
                    (bp.keywords != null && bp.keywords.ToLower().Contains(lower)) ||
                    (bp.author != null && bp.author.ToLower().Contains(lower)));
            }

            int totalCount = await query.CountAsync(cancellationToken);
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));

            if (pageNumber > totalPages) pageNumber = totalPages;

            var items = await query
                .OrderBy(bp => bp.name)
                .ThenBy(bp => bp.ldrawPartId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(bp => new
                {
                    bp.id, bp.name, bp.ldrawPartId, bp.ldrawTitle, bp.ldrawCategory,
                    bp.brickCategoryId,
                    categoryName = bp.brickCategory != null ? bp.brickCategory.name : null,
                    bp.partTypeId,
                    partTypeName = bp.partType != null ? bp.partType.name : null,
                    bp.geometryOriginalFileName, bp.keywords, bp.author,
                    bp.widthLdu, bp.heightLdu, bp.depthLdu, bp.massGrams, bp.versionNumber
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new { totalCount, pageSize, pageNumber, totalPages, items };

            _cache.Set(cacheKey, result, ShortCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"[Anonymous] Catalog browse — page={pageNumber}, search='{search ?? ""}', results={totalCount}");

            return Ok(result);
        }


        /// <summary>
        /// GET /api/public/browse/catalog/all
        ///
        /// Full parts dump for client-side IndexedDB caching.
        /// </summary>
        [HttpGet("catalog/all")]
        public async Task<IActionResult> GetAllCatalogParts(CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            const string cacheKey = "public:catalog:all-parts";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            // Step 1: Pre-compute set counts
            var setCountsByPartId = await _context.LegoSetParts
                .Where(sp => sp.active == true && sp.deleted == false)
                .GroupBy(sp => sp.brickPartId)
                .Select(g => new { PartId = g.Key, SetCount = g.Select(sp => sp.legoSetId).Distinct().Count() })
                .ToDictionaryAsync(x => x.PartId, x => x.SetCount, cancellationToken);

            // Step 2: Load all renderable parts
            var items = await _context.BrickParts
                .Where(bp => bp.geometryOriginalFileName != null && bp.active == true && bp.deleted == false)
                .OrderByDescending(bp => bp.id)
                .Select(bp => new
                {
                    bp.id, bp.name, bp.ldrawPartId, bp.ldrawTitle, bp.ldrawCategory,
                    bp.brickCategoryId,
                    categoryName = bp.brickCategory != null ? bp.brickCategory.name : null,
                    bp.partTypeId,
                    partTypeName = bp.partType != null ? bp.partType.name : null,
                    bp.geometryOriginalFileName, bp.keywords, bp.author,
                    bp.widthLdu, bp.heightLdu, bp.depthLdu, bp.massGrams, bp.versionNumber,
                    setCount = 0
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Step 3: Merge set counts — project to final shape
            var result = items.Select(item => new
            {
                item.id, item.name, item.ldrawPartId, item.ldrawTitle, item.ldrawCategory,
                item.brickCategoryId, item.categoryName, item.partTypeId, item.partTypeName,
                item.geometryOriginalFileName, item.keywords, item.author,
                item.widthLdu, item.heightLdu, item.depthLdu, item.massGrams, item.versionNumber,
                setCount = setCountsByPartId.TryGetValue(item.id, out int sc) ? sc : 0
            }).ToList();

            _cache.Set(cacheKey, result, MediumCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"[Anonymous] Full catalog loaded — {result.Count} parts");

            return Ok(result);
        }


        /// <summary>
        /// GET /api/public/browse/catalog/categories
        /// </summary>
        [HttpGet("catalog/categories")]
        public async Task<IActionResult> GetCatalogCategories(CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            const string cacheKey = "public:catalog:categories";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            var categories = await _context.BrickCategories
                .Where(c => c.active == true && c.deleted == false)
                .Select(c => new
                {
                    c.id, c.name, c.description,
                    partCount = c.BrickParts.Count(bp => bp.geometryOriginalFileName != null && bp.active == true && bp.deleted == false)
                })
                .Where(c => c.partCount > 0)
                .OrderByDescending(c => c.partCount)
                .ThenBy(c => c.name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, categories, MediumCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"[Anonymous] Catalog categories loaded — {categories.Count} categories");

            return Ok(categories);
        }


        /// <summary>
        /// GET /api/public/browse/catalog/part-types
        /// </summary>
        [HttpGet("catalog/part-types")]
        public async Task<IActionResult> GetCatalogPartTypes(CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            const string cacheKey = "public:catalog:part-types";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            var partTypes = await _context.PartTypes
                .Where(pt => pt.active == true && pt.deleted == false)
                .Select(pt => new
                {
                    pt.id, pt.name,
                    partCount = pt.BrickParts.Count(bp => bp.geometryOriginalFileName != null && bp.active == true && bp.deleted == false)
                })
                .Where(pt => pt.partCount > 0)
                .OrderByDescending(pt => pt.partCount)
                .ThenBy(pt => pt.name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, partTypes, MediumCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"[Anonymous] Catalog part types loaded — {partTypes.Count} types");

            return Ok(partTypes);
        }


        /// <summary>
        /// GET /api/public/browse/catalog/part-colours
        /// </summary>
        [HttpGet("catalog/part-colours")]
        public async Task<IActionResult> GetPartColours(int top = 10, CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            if (top <= 0) top = 10;
            if (top > 20) top = 20;

            string cacheKey = $"public:catalog:part-colours:{top}";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            var renderablePartIds = (await _context.BrickParts
                .Where(bp => bp.geometryOriginalFileName != null && bp.active == true && bp.deleted == false)
                .Select(bp => bp.id)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            var partColourCounts = await _context.LegoSetParts
                .Where(sp => sp.active == true && sp.deleted == false
                          && sp.brickColour != null
                          && sp.brickColour.hexRgb != null)
                .GroupBy(sp => new { sp.brickPartId, sp.brickColour.hexRgb })
                .Select(g => new
                {
                    PartId = g.Key.brickPartId,
                    HexRgb = g.Key.hexRgb,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            var result = partColourCounts
                .Where(x => renderablePartIds.Contains(x.PartId))
                .GroupBy(x => x.PartId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.Count)
                          .Take(top)
                          .Select(x => x.HexRgb)
                          .ToArray()
                );

            _cache.Set(cacheKey, result, MediumCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"[Anonymous] Part colours loaded — {result.Count} parts with colour data");

            return Ok(result);
        }


        /// <summary>
        /// GET /api/public/browse/catalog/{partId}/set-appearances
        /// </summary>
        [HttpGet("catalog/{partId}/set-appearances")]
        public async Task<IActionResult> GetSetAppearances(
            int partId,
            int limit = 100,
            string sortBy = "year",
            string sortDir = "desc",
            CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            if (limit <= 0) limit = 100;
            if (limit > 500) limit = 500;

            sortBy = (sortBy ?? "year").ToLower();
            sortDir = (sortDir ?? "desc").ToLower();

            string cacheKey = $"public:set-appearances:{partId}:{limit}:{sortBy}:{sortDir}";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            IQueryable<LegoSetPart> query = _context.LegoSetParts
                .Where(sp => sp.brickPartId == partId
                          && sp.active == true
                          && sp.deleted == false
                          && sp.legoSet != null
                          && sp.legoSet.active == true
                          && sp.legoSet.deleted == false);

            int totalCount = await query.CountAsync(cancellationToken);

            bool asc = sortDir == "asc";

            query = sortBy switch
            {
                "set" => asc ? query.OrderBy(sp => sp.legoSet.name) : query.OrderByDescending(sp => sp.legoSet.name),
                "setnum" => asc ? query.OrderBy(sp => sp.legoSet.setNumber) : query.OrderByDescending(sp => sp.legoSet.setNumber),
                "colour" => asc ? query.OrderBy(sp => sp.brickColour.name) : query.OrderByDescending(sp => sp.brickColour.name),
                "qty" => asc ? query.OrderBy(sp => sp.quantity) : query.OrderByDescending(sp => sp.quantity),
                _ => asc
                    ? query.OrderBy(sp => sp.legoSet.year).ThenBy(sp => sp.legoSet.name)
                    : query.OrderByDescending(sp => sp.legoSet.year).ThenBy(sp => sp.legoSet.name),
            };

            var items = await query
                .Take(limit)
                .Select(sp => new
                {
                    setName = sp.legoSet.name,
                    setNumber = sp.legoSet.setNumber,
                    year = sp.legoSet.year,
                    partCount = sp.legoSet.partCount,
                    imageUrl = sp.legoSet.imageUrl,
                    colourName = sp.brickColour != null ? sp.brickColour.name : null,
                    colourHex = sp.brickColour != null ? sp.brickColour.hexRgb : null,
                    ldrawColourCode = sp.brickColour != null ? sp.brickColour.ldrawColourCode ?? 0 : 0,
                    quantity = sp.quantity ?? 0,
                    isSpare = sp.isSpare,
                    legoSetId = sp.legoSetId,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new { totalCount, items };

            _cache.Set(cacheKey, result, ShortCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"[Anonymous] Set appearances for part {partId} — {totalCount} total, returned {items.Count}");

            return Ok(result);
        }


        /// <summary>
        /// GET /api/public/browse/catalog/{partId}/detail
        ///
        /// Returns lightweight part detail for anonymous viewing.
        /// </summary>
        [HttpGet("catalog/{partId}/detail")]
        public async Task<IActionResult> GetPartDetail(int partId, CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            string cacheKey = $"public:part-detail:{partId}";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            var part = await _context.BrickParts
                .Where(bp => bp.id == partId && bp.active == true && bp.deleted == false)
                .Select(bp => new
                {
                    bp.id, bp.name, bp.ldrawPartId, bp.ldrawTitle, bp.ldrawCategory,
                    bp.brickCategoryId,
                    categoryName = bp.brickCategory != null ? bp.brickCategory.name : null,
                    bp.partTypeId,
                    partTypeName = bp.partType != null ? bp.partType.name : null,
                    bp.geometryOriginalFileName, bp.keywords, bp.author,
                    bp.widthLdu, bp.heightLdu, bp.depthLdu, bp.massGrams,
                    bp.versionNumber,
                    bp.rebrickableImgUrl,
                    bp.rebrickablePartNum,
                    bp.rebrickablePartUrl
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (part == null)
            {
                return NotFound(new { message = "Part not found." });
            }

            // Get available colours for this part
            var colours = await _context.BrickPartColours
                .Where(bpc => bpc.brickPartId == partId && bpc.active == true && bpc.deleted == false)
                .Select(bpc => new
                {
                    bpc.brickColourId,
                    colourName = bpc.brickColour != null ? bpc.brickColour.name : null,
                    colourHex = bpc.brickColour != null ? bpc.brickColour.hexRgb : null,
                    ldrawColourCode = bpc.brickColour != null ? bpc.brickColour.ldrawColourCode : null,
                    finishName = bpc.brickColour != null && bpc.brickColour.colourFinish != null
                        ? bpc.brickColour.colourFinish.name : null
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new { part, colours };

            _cache.Set(cacheKey, result, ShortCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadEntity,
                $"[Anonymous] Part detail loaded — part {partId} ({part.name})");

            return Ok(result);
        }

        #endregion


        #region Set Explorer

        /// <summary>
        /// GET /api/public/browse/sets
        ///
        /// Returns the full precomputed set list from SetExplorerService.
        /// </summary>
        [HttpGet("sets")]
        public async Task<IActionResult> GetSets()
        {
            StartAuditClock();

            var sets = _setExplorerService.GetCachedSets();

            if (sets == null)
            {
                return StatusCode(503, new { message = "Set data is still being computed. Please try again shortly." });
            }

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.LoadPage,
                $"[Anonymous] Set Explorer data loaded — {sets.Count} sets");

            return Ok(sets);
        }


        /// <summary>
        /// GET /api/public/browse/sets/{id}
        ///
        /// Returns set detail including parts, minifigs, and subset info.
        /// </summary>
        [HttpGet("sets/{id}")]
        public async Task<IActionResult> GetSetDetail(int id, CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            string cacheKey = $"public:set-detail:{id}";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            var set = await _context.LegoSets
                .Where(ls => ls.id == id && ls.active && !ls.deleted)
                .Select(ls => new
                {
                    ls.id, ls.name, ls.setNumber, ls.year, ls.partCount, ls.imageUrl,
                    themeId = ls.legoThemeId,
                    themeName = ls.legoTheme != null ? ls.legoTheme.name : null,
                    ls.rebrickableUrl,
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (set == null)
            {
                return NotFound(new { message = "Set not found." });
            }

            // Get parts in this set
            var parts = await _context.LegoSetParts
                .Where(sp => sp.legoSetId == id && sp.active == true && sp.deleted == false)
                .Select(sp => new
                {
                    sp.brickPartId,
                    partName = sp.brickPart != null ? sp.brickPart.name : null,
                    ldrawPartId = sp.brickPart != null ? sp.brickPart.ldrawPartId : null,
                    hasGeometry = sp.brickPart != null && sp.brickPart.geometryOriginalFileName != null,
                    categoryName = sp.brickPart != null && sp.brickPart.brickCategory != null
                        ? sp.brickPart.brickCategory.name
                        : (sp.brickPart != null ? sp.brickPart.ldrawCategory : null),
                    colourName = sp.brickColour != null ? sp.brickColour.name : null,
                    colourHex = sp.brickColour != null ? sp.brickColour.hexRgb : null,
                    ldrawColourCode = sp.brickColour != null ? sp.brickColour.ldrawColourCode ?? 0 : 0,
                    quantity = sp.quantity ?? 0,
                    sp.isSpare,
                    imgUrl = sp.brickPart != null ? sp.brickPart.rebrickableImgUrl : null
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Get minifigs in this set
            var minifigs = await _context.LegoSetMinifigs
                .Where(sm => sm.legoSetId == id && sm.active == true && sm.deleted == false)
                .Select(sm => new
                {
                    sm.legoMinifigId,
                    name = sm.legoMinifig != null ? sm.legoMinifig.name : null,
                    imageUrl = sm.legoMinifig != null ? sm.legoMinifig.imageUrl : null,
                    figNumber = sm.legoMinifig != null ? sm.legoMinifig.figNumber : null,
                    sm.quantity
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new { set, parts, minifigs };

            _cache.Set(cacheKey, result, ShortCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadEntity,
                $"[Anonymous] Set detail loaded — set {id} ({set.name})");

            return Ok(result);
        }

        #endregion


        #region Themes

        /// <summary>
        /// GET /api/public/browse/themes
        ///
        /// Returns all active LEGO themes with set counts.
        /// </summary>
        [HttpGet("themes")]
        public async Task<IActionResult> GetThemes(CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            const string cacheKey = "public:themes";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            var themes = await _context.LegoThemes
                .Where(t => t.active && !t.deleted)
                .Select(t => new
                {
                    t.id, t.name,
                    parentThemeId = t.legoThemeId,
                    setCount = t.LegoSets.Count(s => s.active && !s.deleted)
                })
                .OrderByDescending(t => t.setCount)
                .ThenBy(t => t.name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, themes, MediumCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"[Anonymous] Theme listing loaded — {themes.Count} themes");

            return Ok(themes);
        }


        /// <summary>
        /// GET /api/public/browse/themes/{id}
        ///
        /// Returns theme detail with its sets.
        /// </summary>
        [HttpGet("themes/{id}")]
        public async Task<IActionResult> GetThemeDetail(int id, CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            string cacheKey = $"public:theme-detail:{id}";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            var theme = await _context.LegoThemes
                .Where(t => t.id == id && t.active && !t.deleted)
                .Select(t => new
                {
                    t.id, t.name,
                    parentThemeId = t.legoThemeId,
                    parentThemeName = t.legoTheme != null ? t.legoTheme.name : null
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (theme == null)
            {
                return NotFound(new { message = "Theme not found." });
            }

            // Get child themes
            var childThemes = await _context.LegoThemes
                .Where(t => t.legoThemeId == id && t.active && !t.deleted)
                .Select(t => new
                {
                    t.id, t.name,
                    setCount = t.LegoSets.Count(s => s.active && !s.deleted)
                })
                .OrderBy(t => t.name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Get sets in this theme (direct children only)
            var sets = await _context.LegoSets
                .Where(s => s.legoThemeId == id && s.active && !s.deleted)
                .OrderByDescending(s => s.year)
                .ThenByDescending(s => s.partCount)
                .Select(s => new
                {
                    s.id, s.name, s.setNumber, s.year, s.partCount, s.imageUrl
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new { theme, childThemes, sets };

            _cache.Set(cacheKey, result, MediumCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadEntity,
                $"[Anonymous] Theme detail loaded — theme {id} ({theme.name}), {sets.Count} sets");

            return Ok(result);
        }

        #endregion


        #region Minifig Gallery

        /// <summary>
        /// GET /api/public/browse/minifigs
        ///
        /// Returns the full precomputed minifig list from MinifigGalleryService.
        /// </summary>
        [HttpGet("minifigs")]
        public async Task<IActionResult> GetMinifigs()
        {
            StartAuditClock();

            var minifigs = _minifigGalleryService.GetCachedMinifigs();

            if (minifigs == null)
            {
                return StatusCode(503, new { message = "Minifig data is still being computed. Please try again shortly." });
            }

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.LoadPage,
                $"[Anonymous] Minifig Gallery data loaded — {minifigs.Count} minifigs");

            return Ok(minifigs);
        }


        /// <summary>
        /// GET /api/public/browse/minifigs/{id}
        ///
        /// Returns minifig detail with the sets it appears in.
        /// </summary>
        [HttpGet("minifigs/{id}")]
        public async Task<IActionResult> GetMinifigDetail(int id, CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            string cacheKey = $"public:minifig-detail:{id}";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            var minifig = await _context.LegoMinifigs
                .Where(mf => mf.id == id && mf.active && !mf.deleted)
                .Select(mf => new
                {
                    mf.id, mf.name, mf.figNumber, mf.imageUrl
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (minifig == null)
            {
                return NotFound(new { message = "Minifig not found." });
            }

            // Get sets this minifig appears in
            var sets = await _context.LegoSetMinifigs
                .Where(sm => sm.legoMinifigId == id && sm.active == true && sm.deleted == false
                          && sm.legoSet != null && sm.legoSet.active && !sm.legoSet.deleted)
                .Select(sm => new
                {
                    sm.legoSetId,
                    setName = sm.legoSet != null ? sm.legoSet.name : null,
                    setNumber = sm.legoSet.setNumber,
                    year = sm.legoSet.year,
                    imageUrl = sm.legoSet.imageUrl,
                    sm.quantity
                })
                .OrderByDescending(s => s.year)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new { minifig, sets };

            _cache.Set(cacheKey, result, ShortCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadEntity,
                $"[Anonymous] Minifig detail loaded — minifig {id} ({minifig.name})");

            return Ok(result);
        }

        #endregion


        #region Colour Library

        /// <summary>
        /// GET /api/public/browse/colours
        ///
        /// Returns all LEGO colours with part counts and finish info.
        /// </summary>
        [HttpGet("colours")]
        public async Task<IActionResult> GetColours(CancellationToken cancellationToken = default)
        {
            StartAuditClock();

            const string cacheKey = "public:colours";

            if (_cache.TryGetValue(cacheKey, out object cached))
            {
                return Ok(cached);
            }

            var colours = await _context.BrickColours
                .Where(c => c.active == true && c.deleted == false)
                .Select(c => new
                {
                    c.id, c.name, c.hexRgb, c.hexEdgeColour,
                    c.ldrawColourCode,
                    finishName = c.colourFinish != null ? c.colourFinish.name : null,
                    partCount = c.BrickPartColours.Count(bpc => bpc.active == true && bpc.deleted == false),
                    c.isTransparent
                })
                .OrderBy(c => c.name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, colours, LongCache);

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"[Anonymous] Colour library loaded — {colours.Count} colours");

            return Ok(colours);
        }

        #endregion


        #region Parts Universe

        /// <summary>
        /// GET /api/public/browse/parts-universe
        ///
        /// Returns the precomputed Parts Universe visualization payload.
        /// </summary>
        [HttpGet("parts-universe")]
        public async Task<IActionResult> GetPartsUniverse()
        {
            StartAuditClock();

            var payload = _partsUniverseService.GetCachedPayload();

            if (payload == null)
            {
                return StatusCode(503, new { message = "Parts Universe data is still being computed. Please try again shortly." });
            }

            await CreateAnonymousAuditEventAsync(AuditEngine.AuditType.LoadPage,
                "[Anonymous] Parts Universe data loaded");

            return Ok(payload);
        }

        #endregion


        #region LDraw Geometry Files

        //
        // Static in-memory cache — keyed by normalised relative path, value is file content.
        // Survives across requests for the lifetime of the application.
        // Shared with LDrawController (same file data, different access path).
        //
        private static readonly ConcurrentDictionary<string, string> _ldrawFileCache
            = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //
        // Filename-to-path index for smart file resolution.
        //
        private static ConcurrentDictionary<string, string> _ldrawFileIndex;
        private static readonly object _ldrawIndexLock = new object();
        private static readonly SemaphoreSlim _ldrawIoSemaphore = new SemaphoreSlim(8, 8);

        //
        // Only serve files with these extensions — prevent abuse
        //
        private static readonly HashSet<string> AllowedExtensions
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".dat", ".ldr", ".mpd" };


        /// <summary>
        /// GET /api/public/browse/ldraw/{**filePath}
        ///
        /// Serves raw LDraw geometry files (.dat, .ldr) for the anonymous 3D part viewer.
        /// Files are static, immutable geometry — zero CPU rendering cost.
        /// Uses in-memory caching and semaphore-guarded disk I/O.
        /// </summary>
        [HttpGet("ldraw/{**filePath}")]
        public async Task<IActionResult> GetPublicLDrawFile(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest("Path parameter is required.");
            }

            // Path traversal protection
            if (filePath.Contains("..") || Path.IsPathRooted(filePath))
            {
                _logger.LogWarning("[PublicBrowse] Rejected LDraw file request with suspicious path: {Path}", filePath);
                return BadRequest("Invalid path.");
            }

            // Extension whitelist
            string ext = Path.GetExtension(filePath);
            if (!AllowedExtensions.Contains(ext))
            {
                return BadRequest("Unsupported file type.");
            }

            string dataPath = _configuration.GetValue<string>("LDraw:DataPath");

            if (string.IsNullOrEmpty(dataPath))
            {
                _logger.LogError("[PublicBrowse] LDraw:DataPath is not configured.");
                return StatusCode(500, "LDraw data path is not configured.");
            }

            string normalisedPath = filePath.Replace('\\', '/');

            // Check in-memory cache first
            if (_ldrawFileCache.TryGetValue(normalisedPath, out string cachedContent))
            {
                return Content(cachedContent, "text/plain");
            }

            // Resolve the full file path with smart lookup
            string fullPath = ResolveLDrawFilePath(dataPath, normalisedPath);

            if (fullPath == null)
            {
                return NotFound($"LDraw file not found: {normalisedPath}");
            }

            // Safety check: resolved path must stay within data directory
            string normalisedDataPath = Path.GetFullPath(dataPath);
            if (!fullPath.StartsWith(normalisedDataPath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("[PublicBrowse] Rejected LDraw file outside data directory: {FullPath}", fullPath);
                return BadRequest("Invalid path.");
            }

            // Read from disk, guarded by semaphore
            try
            {
                await _ldrawIoSemaphore.WaitAsync(cancellationToken);

                try
                {
                    // Double-check cache after acquiring semaphore
                    if (_ldrawFileCache.TryGetValue(normalisedPath, out string justCached))
                    {
                        return Content(justCached, "text/plain");
                    }

                    string content = await System.IO.File.ReadAllTextAsync(fullPath, cancellationToken);
                    _ldrawFileCache.TryAdd(normalisedPath, content);

                    return Content(content, "text/plain");
                }
                finally
                {
                    _ldrawIoSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PublicBrowse] Error reading LDraw file: {Path}", fullPath);
                return StatusCode(500, "Error reading file.");
            }
        }


        /// <summary>
        /// Resolves a requested LDraw path to an actual file on disk.
        /// Mirrors the resolution strategy in LDrawController.
        /// </summary>
        private string ResolveLDrawFilePath(string dataPath, string normalisedPath)
        {
            // 1. Try exact path
            string exactPath = Path.GetFullPath(Path.Combine(dataPath, normalisedPath));
            if (System.IO.File.Exists(exactPath)) return exactPath;

            // 2. Build/retrieve filename index
            EnsureLDrawFileIndexBuilt(dataPath);

            // 3. Try path suffix with standard LDraw directories
            int firstSlash = normalisedPath.IndexOf('/');
            if (firstSlash >= 0)
            {
                string pathSuffix = normalisedPath.Substring(firstSlash + 1);
                string[] searchDirs = { "p", "parts", "parts/s", "p/48", "p/8", "models" };

                foreach (string dir in searchDirs)
                {
                    string candidate = Path.GetFullPath(Path.Combine(dataPath, dir, pathSuffix));
                    if (System.IO.File.Exists(candidate)) return candidate;
                }
            }

            // 4. Fall back to filename-only lookup
            string fileName = Path.GetFileName(normalisedPath).ToLowerInvariant();
            if (_ldrawFileIndex != null && _ldrawFileIndex.TryGetValue(fileName, out string indexedRelativePath))
            {
                string indexedFullPath = Path.GetFullPath(Path.Combine(dataPath, indexedRelativePath));
                if (System.IO.File.Exists(indexedFullPath)) return indexedFullPath;
            }

            return null;
        }


        /// <summary>
        /// Lazily builds the filename-to-path index for the LDraw data directory.
        /// </summary>
        private void EnsureLDrawFileIndexBuilt(string dataPath)
        {
            if (_ldrawFileIndex != null) return;

            lock (_ldrawIndexLock)
            {
                if (_ldrawFileIndex != null) return;

                _logger.LogInformation("[PublicBrowse] Building LDraw file index for: {DataPath}", dataPath);

                var index = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    string[] extensions = { "*.dat", "*.ldr", "*.mpd" };

                    foreach (string pattern in extensions)
                    {
                        foreach (string file in Directory.EnumerateFiles(dataPath, pattern, SearchOption.AllDirectories))
                        {
                            string relativePath = Path.GetRelativePath(dataPath, file).Replace('\\', '/');
                            string lowerFileName = Path.GetFileName(file).ToLowerInvariant();
                            index.TryAdd(lowerFileName, relativePath);
                        }
                    }

                    _logger.LogInformation("[PublicBrowse] LDraw file index built: {Count} files indexed.", index.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[PublicBrowse] Error building LDraw file index.");
                }

                _ldrawFileIndex = index;
            }
        }

        #endregion
    }
}
