using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    /// All endpoints are read-only and require BMC read permission.
    /// </summary>
    public class PartsCatalogController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly ILogger<PartsCatalogController> _logger;

        public PartsCatalogController(BMCContext context, ILogger<PartsCatalogController> logger) : base("BMC", "PartsCatalog")
        {
            _context = context;
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

        #endregion


        /// <summary>
        /// GET /api/parts-catalog
        ///
        /// Returns a paginated, filtered list of parts that have LDraw geometry data.
        /// Supports text search across name, ldrawPartId, ldrawTitle, keywords, and author.
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
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            // Clamp pagination params
            if (pageSize <= 0) pageSize = 48;
            if (pageSize > 200) pageSize = 200;
            if (pageNumber < 1) pageNumber = 1;

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

            return Ok(result);
        }


        /// <summary>
        /// GET /api/parts-catalog/categories
        ///
        /// Returns categories that have at least one renderable part (geometryFilePath IS NOT NULL),
        /// along with the count of renderable parts in each category.
        /// Sorted by count descending.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/parts-catalog/categories")]
        public async Task<IActionResult> GetCatalogCategories(CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
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

            return Ok(categories);
        }


        /// <summary>
        /// GET /api/parts-catalog/part-types
        ///
        /// Returns part types that have at least one renderable part,
        /// along with the count of renderable parts for each type.
        /// Sorted by count descending.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/parts-catalog/part-types")]
        public async Task<IActionResult> GetCatalogPartTypes(CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
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

            return Ok(partTypes);
        }
    }
}
