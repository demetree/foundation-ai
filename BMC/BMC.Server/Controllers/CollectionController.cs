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
    /// Custom composite controller for "My Collection" workflows.
    /// Provides higher-level endpoints that aggregate data across multiple tables
    /// (UserCollection, UserCollectionPart, UserWishlistItem, LegoSetPart, etc.)
    /// to power the premium My Collection UI.
    ///
    /// All endpoints are tenant-scoped and require at minimum BMC read permission.
    /// Write operations require the "BMC Collection Writer" custom role.
    /// </summary>
    public class CollectionController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly ILogger<CollectionController> _logger;


        public CollectionController(BMCContext context, ILogger<CollectionController> logger) : base("BMC", "Collection")
        {
            _context = context;
            _logger = logger;

            _context.Database.SetCommandTimeout(60);
        }


        #region DTOs

        /// <summary>
        /// Summary DTO returned by the /mine endpoint — one per collection.
        /// </summary>
        public class CollectionSummaryDto
        {
            public int id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public bool isDefault { get; set; }
            public int uniquePartCount { get; set; }
            public int totalBrickCount { get; set; }
            public int wishlistItemCount { get; set; }
            public int importedSetCount { get; set; }
        }

        /// <summary>
        /// Denormalized part row for the collection parts grid.
        /// </summary>
        public class CollectionPartDto
        {
            public int id { get; set; }
            public int brickPartId { get; set; }
            public string partName { get; set; }
            public string ldrawPartId { get; set; }
            public string ldrawTitle { get; set; }
            public string categoryName { get; set; }
            public string geometryFilePath { get; set; }
            public int brickColourId { get; set; }
            public string colourName { get; set; }
            public string colourHex { get; set; }
            public int quantityOwned { get; set; }
            public int quantityUsed { get; set; }
        }

        /// <summary>
        /// Request body for adding a part to a collection.
        /// </summary>
        public class AddPartRequest
        {
            public int brickPartId { get; set; }
            public int brickColourId { get; set; }
            public int quantity { get; set; } = 1;
        }

        /// <summary>
        /// Summary of a set import operation.
        /// </summary>
        public class ImportSetResult
        {
            public int partsAdded { get; set; }
            public int partsUpdated { get; set; }
            public int totalQuantityAdded { get; set; }
        }

        #endregion


        /// <summary>
        /// GET /api/collection/mine
        ///
        /// Returns the current user's collections with summary statistics.
        /// If the user has no collections, auto-creates a default one named "My Collection".
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/collection/mine")]
        public async Task<IActionResult> GetMyCollections(CancellationToken cancellationToken = default)
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
            // Query all active, non-deleted collections for this tenant
            //
            List<UserCollection> collections = await _context.UserCollections
                .Where(c => c.tenantGuid == userTenantGuid && c.active == true && c.deleted == false)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            //
            // Auto-create a default collection if the user has none
            //
            if (collections.Count == 0)
            {
                var defaultCollection = new UserCollection
                {
                    name = "My Collection",
                    description = "Default brick collection",
                    isDefault = true,
                    tenantGuid = userTenantGuid,
                    objectGuid = Guid.NewGuid(),
                    versionNumber = 1,
                    active = true,
                    deleted = false
                };

                _context.UserCollections.Add(defaultCollection);
                await _context.SaveChangesAsync(cancellationToken);

                collections = new List<UserCollection> { defaultCollection };
            }

            //
            // Build summary DTOs with aggregate counts
            //
            List<int> collectionIds = collections.Select(c => c.id).ToList();

            // Part counts per collection
            var partCounts = await _context.UserCollectionParts
                .Where(p => collectionIds.Contains(p.userCollectionId) && p.active == true && p.deleted == false)
                .GroupBy(p => p.userCollectionId)
                .Select(g => new
                {
                    collectionId = g.Key,
                    uniqueParts = g.Count(),
                    totalBricks = g.Sum(p => (int)(p.quantityOwned ?? 0))
                })
                .ToListAsync(cancellationToken);

            // Wishlist counts per collection
            var wishlistCounts = await _context.UserWishlistItems
                .Where(w => collectionIds.Contains(w.userCollectionId) && w.active == true && w.deleted == false)
                .GroupBy(w => w.userCollectionId)
                .Select(g => new { collectionId = g.Key, count = g.Count() })
                .ToListAsync(cancellationToken);

            // Import counts per collection
            var importCounts = await _context.UserCollectionSetImports
                .Where(i => collectionIds.Contains(i.userCollectionId) && i.active == true && i.deleted == false)
                .GroupBy(i => i.userCollectionId)
                .Select(g => new { collectionId = g.Key, count = g.Count() })
                .ToListAsync(cancellationToken);

            var summaries = collections.Select(c =>
            {
                var pc = partCounts.FirstOrDefault(x => x.collectionId == c.id);
                var wc = wishlistCounts.FirstOrDefault(x => x.collectionId == c.id);
                var ic = importCounts.FirstOrDefault(x => x.collectionId == c.id);

                return new CollectionSummaryDto
                {
                    id = c.id,
                    name = c.name,
                    description = c.description,
                    isDefault = c.isDefault,
                    uniquePartCount = pc?.uniqueParts ?? 0,
                    totalBrickCount = pc?.totalBricks ?? 0,
                    wishlistItemCount = wc?.count ?? 0,
                    importedSetCount = ic?.count ?? 0
                };
            }).ToList();

            return Ok(summaries);
        }


        /// <summary>
        /// GET /api/collection/{id}/parts
        ///
        /// Returns the parts in a specific collection, denormalized with part/colour info.
        /// Supports pagination and text search.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/collection/{id}/parts")]
        public async Task<IActionResult> GetCollectionParts(
            int id,
            string search = null,
            int? pageSize = null,
            int? pageNumber = null,
            CancellationToken cancellationToken = default)
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
            // Verify the collection belongs to this tenant
            //
            bool collectionExists = await _context.UserCollections
                .AnyAsync(c => c.id == id && c.tenantGuid == userTenantGuid && c.active == true && c.deleted == false, cancellationToken);

            if (!collectionExists)
            {
                return NotFound("Collection not found.");
            }

            //
            // Query parts with denormalized part + colour info
            //
            IQueryable<CollectionPartDto> query = _context.UserCollectionParts
                .Where(cp => cp.userCollectionId == id && cp.active == true && cp.deleted == false)
                .Select(cp => new CollectionPartDto
                {
                    id = cp.id,
                    brickPartId = cp.brickPartId,
                    partName = cp.brickPart.name,
                    ldrawPartId = cp.brickPart.ldrawPartId,
                    ldrawTitle = cp.brickPart.ldrawTitle,
                    categoryName = cp.brickPart.brickCategory != null ? cp.brickPart.brickCategory.name : null,
                    geometryFilePath = cp.brickPart.geometryFilePath,
                    brickColourId = cp.brickColourId,
                    colourName = cp.brickColour.name,
                    colourHex = cp.brickColour.hexRgb,
                    quantityOwned = cp.quantityOwned,
                    quantityUsed = cp.quantityUsed
                });

            //
            // Text search across part name, LDraw ID, title, colour name
            //
            if (!string.IsNullOrWhiteSpace(search))
            {
                string lower = search.ToLower();
                query = query.Where(p =>
                    p.partName.ToLower().Contains(lower) ||
                    p.ldrawPartId.ToLower().Contains(lower) ||
                    (p.ldrawTitle != null && p.ldrawTitle.ToLower().Contains(lower)) ||
                    p.colourName.ToLower().Contains(lower));
            }

            query = query.OrderBy(p => p.partName).ThenBy(p => p.colourName);

            //
            // Pagination
            //
            if (pageNumber.HasValue && pageSize.HasValue && pageNumber.Value >= 1 && pageSize.Value > 0)
            {
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            List<CollectionPartDto> parts = await query.AsNoTracking().ToListAsync(cancellationToken);

            return Ok(parts);
        }


        /// <summary>
        /// POST /api/collection/{id}/add-part
        ///
        /// Adds a part+colour to a collection, or increments the quantity if it already exists.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/collection/{id}/add-part")]
        public async Task<IActionResult> AddPart(int id, [FromBody] AddPartRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
            // Verify collection ownership
            //
            bool collectionExists = await _context.UserCollections
                .AnyAsync(c => c.id == id && c.tenantGuid == userTenantGuid && c.active == true && c.deleted == false, cancellationToken);

            if (!collectionExists)
            {
                return NotFound("Collection not found.");
            }

            //
            // Check if this part+colour combo already exists in the collection
            //
            UserCollectionPart existing = await _context.UserCollectionParts
                .FirstOrDefaultAsync(p =>
                    p.userCollectionId == id &&
                    p.brickPartId == request.brickPartId &&
                    p.brickColourId == request.brickColourId &&
                    p.active == true &&
                    p.deleted == false, cancellationToken);

            if (existing != null)
            {
                // Increment quantity
                existing.quantityOwned = (existing.quantityOwned ?? 0) + Math.Max(request.quantity, 1);
                await _context.SaveChangesAsync(cancellationToken);

                return Ok(new { action = "updated", quantityOwned = existing.quantityOwned });
            }
            else
            {
                // Create new entry
                var newPart = new UserCollectionPart
                {
                    userCollectionId = id,
                    brickPartId = request.brickPartId,
                    brickColourId = request.brickColourId,
                    quantityOwned = Math.Max(request.quantity, 1),
                    quantityUsed = 0,
                    tenantGuid = userTenantGuid,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.UserCollectionParts.Add(newPart);
                await _context.SaveChangesAsync(cancellationToken);

                return Ok(new { action = "created", quantityOwned = newPart.quantityOwned });
            }
        }


        /// <summary>
        /// DELETE /api/collection/{id}/remove-part/{partId}
        ///
        /// Removes a UserCollectionPart entry from a collection (soft-delete).
        /// </summary>
        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/collection/{id}/remove-part/{partId}")]
        public async Task<IActionResult> RemovePart(int id, int partId, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
            // Verify collection ownership
            //
            bool collectionExists = await _context.UserCollections
                .AnyAsync(c => c.id == id && c.tenantGuid == userTenantGuid && c.active == true && c.deleted == false, cancellationToken);

            if (!collectionExists)
            {
                return NotFound("Collection not found.");
            }

            //
            // Find and soft-delete the part entry
            //
            UserCollectionPart part = await _context.UserCollectionParts
                .FirstOrDefaultAsync(p => p.id == partId && p.userCollectionId == id && p.deleted == false, cancellationToken);

            if (part == null)
            {
                return NotFound("Part not found in this collection.");
            }

            part.deleted = true;
            part.active = false;
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { action = "removed" });
        }


        /// <summary>
        /// POST /api/collection/{id}/import-set/{legoSetId}
        ///
        /// Imports all parts from a LEGO set into a collection.
        /// For each LegoSetPart, creates or increments a UserCollectionPart entry,
        /// and creates a UserCollectionSetImport record.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/collection/{id}/import-set/{legoSetId}")]
        public async Task<IActionResult> ImportSet(int id, int legoSetId, int quantity = 1, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
            // Verify collection ownership
            //
            bool collectionExists = await _context.UserCollections
                .AnyAsync(c => c.id == id && c.tenantGuid == userTenantGuid && c.active == true && c.deleted == false, cancellationToken);

            if (!collectionExists)
            {
                return NotFound("Collection not found.");
            }

            //
            // Verify the LEGO set exists
            //
            bool setExists = await _context.LegoSets
                .AnyAsync(s => s.id == legoSetId && s.active == true && s.deleted == false, cancellationToken);

            if (!setExists)
            {
                return NotFound("LEGO set not found.");
            }

            //
            // Load the set's parts inventory
            //
            List<LegoSetPart> setParts = await _context.LegoSetParts
                .Where(sp => sp.legoSetId == legoSetId && sp.active == true && sp.deleted == false)
                .ToListAsync(cancellationToken);

            if (setParts.Count == 0)
            {
                return BadRequest("This set has no parts in the catalog.");
            }

            //
            // Load existing collection parts for upsert logic
            //
            List<UserCollectionPart> existingParts = await _context.UserCollectionParts
                .Where(cp => cp.userCollectionId == id && cp.active == true && cp.deleted == false)
                .ToListAsync(cancellationToken);

            int partsAdded = 0;
            int partsUpdated = 0;
            int totalQtyAdded = 0;

            foreach (LegoSetPart sp in setParts)
            {
                int qtyToAdd = (sp.quantity ?? 1) * Math.Max(quantity, 1);

                UserCollectionPart match = existingParts.FirstOrDefault(
                    p => p.brickPartId == sp.brickPartId && p.brickColourId == sp.brickColourId);

                if (match != null)
                {
                    match.quantityOwned = (match.quantityOwned ?? 0) + qtyToAdd;
                    partsUpdated++;
                }
                else
                {
                    var newPart = new UserCollectionPart
                    {
                        userCollectionId = id,
                        brickPartId = sp.brickPartId,
                        brickColourId = sp.brickColourId,
                        quantityOwned = qtyToAdd,
                        quantityUsed = 0,
                        tenantGuid = userTenantGuid,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    _context.UserCollectionParts.Add(newPart);
                    existingParts.Add(newPart);  // track for subsequent duplicates within same set
                    partsAdded++;
                }

                totalQtyAdded += qtyToAdd;
            }

            //
            // Record the import
            //
            var importRecord = new UserCollectionSetImport
            {
                userCollectionId = id,
                legoSetId = legoSetId,
                quantity = Math.Max(quantity, 1),
                importedDate = DateTime.UtcNow,
                tenantGuid = userTenantGuid,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.UserCollectionSetImports.Add(importRecord);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new ImportSetResult
            {
                partsAdded = partsAdded,
                partsUpdated = partsUpdated,
                totalQuantityAdded = totalQtyAdded
            });
        }


        /// <summary>
        /// GET /api/collection/{id}/imported-sets
        ///
        /// Returns the list of sets that have been imported into a collection.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/collection/{id}/imported-sets")]
        public async Task<IActionResult> GetImportedSets(int id, CancellationToken cancellationToken = default)
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
            // Verify collection ownership
            //
            bool collectionExists = await _context.UserCollections
                .AnyAsync(c => c.id == id && c.tenantGuid == userTenantGuid && c.active == true && c.deleted == false, cancellationToken);

            if (!collectionExists)
            {
                return NotFound("Collection not found.");
            }

            var imports = await _context.UserCollectionSetImports
                .Where(i => i.userCollectionId == id && i.active == true && i.deleted == false)
                .Select(i => new
                {
                    i.id,
                    i.legoSetId,
                    setName = i.legoSet.name,
                    setNumber = i.legoSet.setNumber,
                    i.quantity,
                    i.importedDate
                })
                .OrderByDescending(i => i.importedDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(imports);
        }


        /// <summary>
        /// GET /api/collection/{id}/wishlist
        ///
        /// Returns wishlist items for a collection, denormalized.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/collection/{id}/wishlist")]
        public async Task<IActionResult> GetWishlist(int id, CancellationToken cancellationToken = default)
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

            bool collectionExists = await _context.UserCollections
                .AnyAsync(c => c.id == id && c.tenantGuid == userTenantGuid && c.active == true && c.deleted == false, cancellationToken);

            if (!collectionExists)
            {
                return NotFound("Collection not found.");
            }

            var wishlist = await _context.UserWishlistItems
                .Where(w => w.userCollectionId == id && w.active == true && w.deleted == false)
                .Select(w => new
                {
                    w.id,
                    w.brickPartId,
                    partName = w.brickPart.name,
                    ldrawPartId = w.brickPart.ldrawPartId,
                    brickColourId = (int?)w.brickColourId,
                    colourName = w.brickColour != null ? w.brickColour.name : "Any colour",
                    colourHex = w.brickColour != null ? w.brickColour.hexRgb : null,
                    w.quantityDesired,
                    w.notes
                })
                .OrderBy(w => w.partName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(wishlist);
        }
    }
}
