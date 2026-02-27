using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Custom read-only controller for the Parts Catalog UI.
    /// 
    /// Provides lightweight, paginated endpoints that only return parts with
    /// LDraw geometry data (geometryFilePath IS NOT NULL), making the catalog
    /// fast even with 50K+ parts in the database from the Rebrickable import.
    ///
    /// Uses IMemoryCache to cache repeated requests — categories and part types
    /// are cached for 10 minutes (they rarely change), and paginated part results
    /// are cached for 2 minutes (good enough for browsing bursts).
    ///
    /// All endpoints are read-only and require BMC read permission.
    /// </summary>
    public class PartsCatalogController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private static readonly TimeSpan PageCacheDuration = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan SidebarCacheDuration = TimeSpan.FromMinutes(10);

        private readonly BMCContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PartsCatalogController> _logger;

        public PartsCatalogController(
            BMCContext context,
            IMemoryCache cache,
            ILogger<PartsCatalogController> logger
        ) : base("BMC", "PartsCatalog")
        {
            _context = context;
            _cache = cache;
            _logger = logger;

            _context.Database.SetCommandTimeout(30);
        }


        #region DTOs

        /// <summary>
        /// Lightweight part DTO for the catalog grid — flat fields only.
        /// </summary>
        public class CatalogPartDto
        {
            public int id { get; set; }
            public string name { get; set; }
            public string ldrawPartId { get; set; }
            public string ldrawTitle { get; set; }
            public string ldrawCategory { get; set; }
            public int brickCategoryId { get; set; }
            public string categoryName { get; set; }
            public int partTypeId { get; set; }
            public string partTypeName { get; set; }
            public string geometryFilePath { get; set; }
            public string keywords { get; set; }
            public string author { get; set; }
            public float? widthLdu { get; set; }
            public float? heightLdu { get; set; }
            public float? depthLdu { get; set; }
            public float? massGrams { get; set; }
            public int versionNumber { get; set; }
            public int setCount { get; set; }
        }

        /// <summary>
        /// Paginated result wrapper.
        /// </summary>
        public class CatalogPageResult
        {
            public int totalCount { get; set; }
            public int pageSize { get; set; }
            public int pageNumber { get; set; }
            public int totalPages { get; set; }
            public List<CatalogPartDto> items { get; set; }
        }

        /// <summary>
        /// Category with renderable part count — used for the sidebar.
        /// </summary>
        public class CatalogCategoryDto
        {
            public int id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public int partCount { get; set; }
        }

        /// <summary>
        /// Part type with renderable part count — used for the sidebar.
        /// </summary>
        public class CatalogPartTypeDto
        {
            public int id { get; set; }
            public string name { get; set; }
            public int partCount { get; set; }
        }

        /// <summary>
        /// Lightweight set appearance DTO — flat fields, no navigation properties.
        /// Used by the catalog-part-detail "Appears In Sets" panel.
        /// </summary>
        public class SetAppearanceDto
        {
            public string setName { get; set; }
            public string setNumber { get; set; }
            public int year { get; set; }
            public int partCount { get; set; }
            public string imageUrl { get; set; }
            public string colourName { get; set; }
            public string colourHex { get; set; }
            public int ldrawColourCode { get; set; }
            public int quantity { get; set; }
            public bool isSpare { get; set; }
            public int legoSetId { get; set; }
        }

        /// <summary>
        /// Response wrapper for set appearances — includes total count
        /// so the client can show "X total, showing Y newest" in the badge.
        /// </summary>
        public class SetAppearancesResult
        {
            public int totalCount { get; set; }
            public List<SetAppearanceDto> items { get; set; }
        }

        #endregion


        /// <summary>
        /// GET /api/parts-catalog
        ///
        /// Returns a paginated, filtered list of parts that have LDraw geometry data.
        /// Supports text search across name, ldrawPartId, ldrawTitle, keywords, and author.
        /// Results are cached for 2 minutes per unique combination of parameters.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/parts-catalog")]
        public async Task<IActionResult> GetCatalogParts(
            string search = null,
            int? categoryId = null,
            int? partTypeId = null,
            int pageSize = 48,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            // Clamp pagination params
            if (pageSize <= 0) pageSize = 48;
            if (pageSize > 200) pageSize = 200;
            if (pageNumber < 1) pageNumber = 1;

            //
            // Build a cache key from the normalised parameters
            //
            string cacheKey = $"catalog:parts:{search ?? ""}:{categoryId}:{partTypeId}:{pageSize}:{pageNumber}";

            if (_cache.TryGetValue(cacheKey, out CatalogPageResult cached))
            {
                return Ok(cached);
            }

            //
            // Base query: only parts with LDraw geometry data, active and not deleted
            //
            IQueryable<BrickPart> query = _context.BrickParts
                .Where(bp => bp.geometryFilePath != null && bp.active == true && bp.deleted == false);

            //
            // Category filter
            //
            if (categoryId.HasValue)
            {
                query = query.Where(bp => bp.brickCategoryId == categoryId.Value);
            }

            //
            // Part type filter
            //
            if (partTypeId.HasValue)
            {
                query = query.Where(bp => bp.partTypeId == partTypeId.Value);
            }

            //
            // Text search across multiple fields
            //
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

            //
            // Get total count before pagination
            //
            int totalCount = await query.CountAsync(cancellationToken);
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));

            if (pageNumber > totalPages) pageNumber = totalPages;

            //
            // Project to lightweight DTO, ordered and paginated
            //
            List<CatalogPartDto> items = await query
                .OrderBy(bp => bp.name)
                .ThenBy(bp => bp.ldrawPartId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(bp => new CatalogPartDto
                {
                    id = bp.id,
                    name = bp.name,
                    ldrawPartId = bp.ldrawPartId,
                    ldrawTitle = bp.ldrawTitle,
                    ldrawCategory = bp.ldrawCategory,
                    brickCategoryId = bp.brickCategoryId,
                    categoryName = bp.brickCategory != null ? bp.brickCategory.name : null,
                    partTypeId = bp.partTypeId,
                    partTypeName = bp.partType != null ? bp.partType.name : null,
                    geometryFilePath = bp.geometryFilePath,
                    keywords = bp.keywords,
                    author = bp.author,
                    widthLdu = bp.widthLdu,
                    heightLdu = bp.heightLdu,
                    depthLdu = bp.depthLdu,
                    massGrams = bp.massGrams,
                    versionNumber = bp.versionNumber,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            CatalogPageResult result = new CatalogPageResult
            {
                totalCount = totalCount,
                pageSize = pageSize,
                pageNumber = pageNumber,
                totalPages = totalPages,
                items = items
            };

            //
            // Cache for 2 minutes — good enough for browsing bursts,
            // short enough that new imports are reflected quickly
            //
            _cache.Set(cacheKey, result, PageCacheDuration);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, $"Catalog browse — page={pageNumber}, search='{search ?? ""}', results={totalCount}");

            return Ok(result);
        }


        /// <summary>
        /// GET /api/parts-catalog/all
        ///
        /// Returns the full list of renderable parts (those with LDraw geometry data)
        /// with a setCount field indicating how many distinct sets each part appears in.
        /// Designed for client-side caching in IndexedDB with full in-memory filter/sort.
        /// Cached server-side for 10 minutes.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/parts-catalog/all")]
        public async Task<IActionResult> GetAllCatalogParts(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            const string cacheKey = "catalog:all-parts";

            if (_cache.TryGetValue(cacheKey, out List<CatalogPartDto> cached))
            {
                return Ok(cached);
            }

            //
            // Step 1: Pre-compute set counts per part.
            // GroupBy brickPartId, count distinct legoSetId values.
            //
            Dictionary<int, int> setCountsByPartId = await _context.LegoSetParts
                .Where(sp => sp.active == true && sp.deleted == false)
                .GroupBy(sp => sp.brickPartId)
                .Select(g => new { PartId = g.Key, SetCount = g.Select(sp => sp.legoSetId).Distinct().Count() })
                .ToDictionaryAsync(x => x.PartId, x => x.SetCount, cancellationToken);

            //
            // Step 2: Load all renderable parts.
            //
            List<CatalogPartDto> items = await _context.BrickParts
                .Where(bp => bp.geometryFilePath != null && bp.active == true && bp.deleted == false)
                .OrderByDescending(bp => bp.id)   // deterministic order; client re-sorts
                .Select(bp => new CatalogPartDto
                {
                    id = bp.id,
                    name = bp.name,
                    ldrawPartId = bp.ldrawPartId,
                    ldrawTitle = bp.ldrawTitle,
                    ldrawCategory = bp.ldrawCategory,
                    brickCategoryId = bp.brickCategoryId,
                    categoryName = bp.brickCategory != null ? bp.brickCategory.name : null,
                    partTypeId = bp.partTypeId,
                    partTypeName = bp.partType != null ? bp.partType.name : null,
                    geometryFilePath = bp.geometryFilePath,
                    keywords = bp.keywords,
                    author = bp.author,
                    widthLdu = bp.widthLdu,
                    heightLdu = bp.heightLdu,
                    depthLdu = bp.depthLdu,
                    massGrams = bp.massGrams,
                    versionNumber = bp.versionNumber,
                    setCount = 0  // populated below
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            //
            // Step 3: Merge set counts into the DTOs.
            //
            foreach (var item in items)
            {
                if (setCountsByPartId.TryGetValue(item.id, out int count))
                {
                    item.setCount = count;
                }
            }

            _cache.Set(cacheKey, items, SidebarCacheDuration);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, $"Full catalog loaded (IndexedDB sync) — {items.Count} parts");

            return Ok(items);
        }


        /// <summary>
        /// GET /api/parts-catalog/categories
        ///
        /// Returns categories that have at least one renderable part (geometryFilePath IS NOT NULL),
        /// along with the count of renderable parts in each category.
        /// Sorted by count descending. Cached for 10 minutes.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/parts-catalog/categories")]
        public async Task<IActionResult> GetCatalogCategories(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            const string cacheKey = "catalog:categories";

            if (_cache.TryGetValue(cacheKey, out List<CatalogCategoryDto> cached))
            {
                return Ok(cached);
            }

            List<CatalogCategoryDto> categories = await _context.BrickCategories
                .Where(c => c.active == true && c.deleted == false)
                .Select(c => new CatalogCategoryDto
                {
                    id = c.id,
                    name = c.name,
                    description = c.description,
                    partCount = c.BrickParts.Count(bp => bp.geometryFilePath != null && bp.active == true && bp.deleted == false)
                })
                .Where(c => c.partCount > 0)
                .OrderByDescending(c => c.partCount)
                .ThenBy(c => c.name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, categories, SidebarCacheDuration);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, $"Catalog categories loaded — {categories.Count} categories");

            return Ok(categories);
        }


        /// <summary>
        /// GET /api/parts-catalog/part-types
        ///
        /// Returns part types that have at least one renderable part,
        /// along with the count of renderable parts for each type.
        /// Sorted by count descending. Cached for 10 minutes.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/parts-catalog/part-types")]
        public async Task<IActionResult> GetCatalogPartTypes(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            const string cacheKey = "catalog:part-types";

            if (_cache.TryGetValue(cacheKey, out List<CatalogPartTypeDto> cached))
            {
                return Ok(cached);
            }

            List<CatalogPartTypeDto> partTypes = await _context.PartTypes
                .Where(pt => pt.active == true && pt.deleted == false)
                .Select(pt => new CatalogPartTypeDto
                {
                    id = pt.id,
                    name = pt.name,
                    partCount = pt.BrickParts.Count(bp => bp.geometryFilePath != null && bp.active == true && bp.deleted == false)
                })
                .Where(pt => pt.partCount > 0)
                .OrderByDescending(pt => pt.partCount)
                .ThenBy(pt => pt.name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, partTypes, SidebarCacheDuration);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, $"Catalog part types loaded — {partTypes.Count} types");

            return Ok(partTypes);
        }


        /// <summary>
        /// GET /api/parts-catalog/{partId}/set-appearances
        ///
        /// Returns a limited, sorted list of sets that contain the specified part,
        /// along with the total count of all set appearances.
        ///
        /// Designed for the catalog-part-detail "Appears In Sets" panel so that
        /// only a lightweight payload is transferred instead of the full entity graph.
        ///
        /// Cached for 2 minutes per unique combination of parameters.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/parts-catalog/{partId}/set-appearances")]
        public async Task<IActionResult> GetSetAppearances(
            int partId,
            int limit = 100,
            string sortBy = "year",
            string sortDir = "desc",
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            // Clamp limit
            if (limit <= 0) limit = 100;
            if (limit > 500) limit = 500;

            // Normalise sort params
            sortBy = (sortBy ?? "year").ToLower();
            sortDir = (sortDir ?? "desc").ToLower();

            string cacheKey = $"catalog:set-appearances:{partId}:{limit}:{sortBy}:{sortDir}";

            if (_cache.TryGetValue(cacheKey, out SetAppearancesResult cached))
            {
                return Ok(cached);
            }

            //
            // Base query: active, non-deleted set parts for this brick part
            //
            IQueryable<LegoSetPart> query = _context.LegoSetParts
                .Where(sp => sp.brickPartId == partId
                          && sp.active == true
                          && sp.deleted == false
                          && sp.legoSet != null
                          && sp.legoSet.active == true
                          && sp.legoSet.deleted == false);

            //
            // Total count before sorting / limiting
            //
            int totalCount = await query.CountAsync(cancellationToken);

            //
            // Apply sort
            //
            bool asc = sortDir == "asc";

            query = sortBy switch
            {
                "set" => asc
                    ? query.OrderBy(sp => sp.legoSet.name)
                    : query.OrderByDescending(sp => sp.legoSet.name),

                "setnum" => asc
                    ? query.OrderBy(sp => sp.legoSet.setNumber)
                    : query.OrderByDescending(sp => sp.legoSet.setNumber),

                "colour" => asc
                    ? query.OrderBy(sp => sp.brickColour.name)
                    : query.OrderByDescending(sp => sp.brickColour.name),

                "qty" => asc
                    ? query.OrderBy(sp => sp.quantity)
                    : query.OrderByDescending(sp => sp.quantity),

                // "year" or anything else defaults to year
                _ => asc
                    ? query.OrderBy(sp => sp.legoSet.year).ThenBy(sp => sp.legoSet.name)
                    : query.OrderByDescending(sp => sp.legoSet.year).ThenBy(sp => sp.legoSet.name),
            };

            //
            // Project to lightweight DTO and take the limit
            //
            List<SetAppearanceDto> items = await query
                .Take(limit)
                .Select(sp => new SetAppearanceDto
                {
                    setName = sp.legoSet.name,
                    setNumber = sp.legoSet.setNumber,
                    year = sp.legoSet.year,
                    partCount = sp.legoSet.partCount,
                    imageUrl = sp.legoSet.imageUrl,
                    colourName = sp.brickColour != null ? sp.brickColour.name : null,
                    colourHex = sp.brickColour != null ? sp.brickColour.hexRgb : null,
                    ldrawColourCode = sp.brickColour != null ? sp.brickColour.ldrawColourCode : 0,
                    quantity = sp.quantity ?? 0,
                    isSpare = sp.isSpare,
                    legoSetId = sp.legoSetId,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            SetAppearancesResult result = new SetAppearancesResult
            {
                totalCount = totalCount,
                items = items
            };

            _cache.Set(cacheKey, result, PageCacheDuration);

            await CreateAuditEventAsync(
                AuditEngine.AuditType.ReadList,
                $"Set appearances for part {partId} — {totalCount} total, returned {items.Count}");

            return Ok(result);
        }
    }
}
