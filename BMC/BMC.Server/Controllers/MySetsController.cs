using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Custom controller for "My Sets" — managing the user's owned LEGO sets.
    ///
    /// Endpoints:
    ///   GET    /api/my-sets               — All owned sets with denormalized set info
    ///   GET    /api/my-sets/{id}          — Single ownership with full set detail
    ///   POST   /api/my-sets              — Add a set to the collection
    ///   PUT    /api/my-sets/{id}         — Update ownership (quantity, rating, notes, status)
    ///   DELETE /api/my-sets/{id}         — Remove set from collection (soft-delete)
    ///   GET    /api/my-sets/stats        — Summary statistics
    ///
    /// All endpoints are tenant-scoped. Write operations check "BMC Collection Writer" role.
    /// </summary>
    public class MySetsController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly ILogger<MySetsController> _logger;


        public MySetsController(BMCContext context, ILogger<MySetsController> logger)
            : base("BMC", "UserSetOwnership")
        {
            _context = context;
            _logger = logger;

            _context.Database.SetCommandTimeout(60);
        }


        #region DTOs

        /// <summary>
        /// Summary DTO for set ownership listing.
        /// </summary>
        public class OwnedSetDto
        {
            public int id { get; set; }
            public int legoSetId { get; set; }
            public string setName { get; set; }
            public string setNumber { get; set; }
            public string imageUrl { get; set; }
            public int year { get; set; }
            public int partCount { get; set; }
            public string themeName { get; set; }
            public string rebrickableUrl { get; set; }
            public string status { get; set; }
            public DateTime? acquiredDate { get; set; }
            public int? personalRating { get; set; }
            public string notes { get; set; }
            public int quantity { get; set; }
            public bool isPublic { get; set; }
        }

        /// <summary>
        /// Collection statistics.
        /// </summary>
        public class CollectionStatsDto
        {
            public int totalSets { get; set; }
            public int uniqueSets { get; set; }
            public int totalParts { get; set; }
            public int totalThemes { get; set; }
            public double averageRating { get; set; }
        }

        /// <summary>
        /// Request body for adding a set.
        /// </summary>
        public class AddSetRequest
        {
            public int legoSetId { get; set; }
            public int quantity { get; set; } = 1;
            public string status { get; set; } = "Owned";
            public string notes { get; set; }
            public int? personalRating { get; set; }
            public bool isPublic { get; set; } = false;
        }

        /// <summary>
        /// Request body for updating ownership.
        /// </summary>
        public class UpdateSetRequest
        {
            public int? quantity { get; set; }
            public string status { get; set; }
            public string notes { get; set; }
            public int? personalRating { get; set; }
            public bool? isPublic { get; set; }
        }

        #endregion


        // ─────────────────────────────────────────────────────────────────
        // GET /api/my-sets — all owned sets for the current tenant
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all set ownerships with denormalized set info (name, image, theme, etc.).
        /// Supports optional search, status filter, and pagination.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-sets")]
        public async Task<IActionResult> GetAll(
            string search = null,
            string status = null,
            int? themeId = null,
            string sortBy = null,
            int? pageSize = null,
            int? pageNumber = null,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var query = _context.UserSetOwnerships
                .Where(o => o.tenantGuid == tenantGuid && o.active == true && o.deleted == false)
                .Join(_context.LegoSets.Where(s => s.active == true && s.deleted == false),
                    o => o.legoSetId,
                    s => s.id,
                    (o, s) => new { ownership = o, set = s });

            // Search by set name or set number
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(x =>
                    x.set.name.Contains(term)
                    || x.set.setNumber.Contains(term)
                    || (x.ownership.notes != null && x.ownership.notes.Contains(term)));
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.ownership.status == status);
            }

            // Filter by theme
            if (themeId.HasValue)
            {
                query = query.Where(x => x.set.legoThemeId == themeId.Value);
            }

            // Sorting
            switch (sortBy?.ToLowerInvariant())
            {
                case "name":
                    query = query.OrderBy(x => x.set.name);
                    break;
                case "year":
                    query = query.OrderByDescending(x => x.set.year);
                    break;
                case "rating":
                    query = query.OrderByDescending(x => x.ownership.personalRating ?? 0);
                    break;
                case "acquired":
                    query = query.OrderByDescending(x => x.ownership.acquiredDate ?? DateTime.MinValue);
                    break;
                case "parts":
                    query = query.OrderByDescending(x => x.set.partCount);
                    break;
                default:
                    query = query.OrderByDescending(x => x.ownership.id);
                    break;
            }

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Pagination
            if (pageNumber.HasValue && pageSize.HasValue && pageNumber > 0 && pageSize > 0)
            {
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            // Left join to get theme name
            var results = await query
                .Select(x => new OwnedSetDto
                {
                    id = x.ownership.id,
                    legoSetId = x.set.id,
                    setName = x.set.name,
                    setNumber = x.set.setNumber,
                    imageUrl = x.set.imageUrl,
                    year = x.set.year,
                    partCount = x.set.partCount,
                    themeName = x.set.legoTheme != null ? x.set.legoTheme.name : null,
                    rebrickableUrl = x.set.rebrickableUrl,
                    status = x.ownership.status,
                    acquiredDate = x.ownership.acquiredDate,
                    personalRating = x.ownership.personalRating,
                    notes = x.ownership.notes,
                    quantity = x.ownership.quantity,
                    isPublic = x.ownership.isPublic
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"Get my sets — {results.Count} of {totalCount} sets");

            return Ok(new { items = results, totalCount });
        }


        // ─────────────────────────────────────────────────────────────────
        // GET /api/my-sets/stats — collection statistics
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns summary statistics for the user's set collection.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-sets/stats")]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var ownerships = _context.UserSetOwnerships
                .Where(o => o.tenantGuid == tenantGuid && o.active == true && o.deleted == false);

            int uniqueSets = await ownerships.CountAsync(cancellationToken);
            int totalSets = await ownerships.SumAsync(o => o.quantity, cancellationToken);

            int totalParts = await ownerships
                .Join(_context.LegoSets, o => o.legoSetId, s => s.id, (o, s) => new { o, s })
                .SumAsync(x => x.s.partCount * x.o.quantity, cancellationToken);

            int totalThemes = await ownerships
                .Join(_context.LegoSets, o => o.legoSetId, s => s.id, (o, s) => s.legoThemeId)
                .Distinct()
                .CountAsync(cancellationToken);

            double avgRating = uniqueSets > 0
                ? await ownerships
                    .Where(o => o.personalRating.HasValue && o.personalRating > 0)
                    .Select(o => (double)o.personalRating.Value)
                    .DefaultIfEmpty(0)
                    .AverageAsync(cancellationToken)
                : 0;

            return Ok(new CollectionStatsDto
            {
                totalSets = totalSets,
                uniqueSets = uniqueSets,
                totalParts = totalParts,
                totalThemes = totalThemes,
                averageRating = Math.Round(avgRating, 1)
            });
        }


        // ─────────────────────────────────────────────────────────────────
        // GET /api/my-sets/{id} — single ownership detail
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a single owned set with full details.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-sets/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var result = await _context.UserSetOwnerships
                .Where(o => o.id == id && o.tenantGuid == tenantGuid && o.active == true && o.deleted == false)
                .Join(_context.LegoSets,
                    o => o.legoSetId,
                    s => s.id,
                    (o, s) => new OwnedSetDto
                    {
                        id = o.id,
                        legoSetId = s.id,
                        setName = s.name,
                        setNumber = s.setNumber,
                        imageUrl = s.imageUrl,
                        year = s.year,
                        partCount = s.partCount,
                        themeName = s.legoTheme != null ? s.legoTheme.name : null,
                        rebrickableUrl = s.rebrickableUrl,
                        status = o.status,
                        acquiredDate = o.acquiredDate,
                        personalRating = o.personalRating,
                        notes = o.notes,
                        quantity = o.quantity,
                        isPublic = o.isPublic
                    })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                return NotFound("Set ownership not found.");
            }

            return Ok(result);
        }


        // ─────────────────────────────────────────────────────────────────
        // POST /api/my-sets — add a set to the collection
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a LEGO set to the user's collection. If the set is already owned,
        /// increments the quantity.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-sets")]
        public async Task<IActionResult> AddSet([FromBody] AddSetRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null) return BadRequest();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            // Verify the LEGO set exists
            var legoSet = await _context.LegoSets
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.id == request.legoSetId && s.active == true && s.deleted == false, cancellationToken);

            if (legoSet == null) return NotFound("LEGO set not found.");

            // Check for existing ownership (upsert)
            var existing = await _context.UserSetOwnerships
                .FirstOrDefaultAsync(o =>
                    o.legoSetId == request.legoSetId
                    && o.tenantGuid == tenantGuid
                    && o.active == true
                    && o.deleted == false, cancellationToken);

            string action;
            int ownershipId;

            if (existing != null)
            {
                existing.quantity += Math.Max(request.quantity, 1);
                if (request.personalRating.HasValue) existing.personalRating = request.personalRating;
                if (!string.IsNullOrWhiteSpace(request.notes)) existing.notes = request.notes;
                action = "updated";
                ownershipId = existing.id;
            }
            else
            {
                var newOwnership = new UserSetOwnership
                {
                    legoSetId = request.legoSetId,
                    quantity = Math.Max(request.quantity, 1),
                    status = string.IsNullOrWhiteSpace(request.status) ? "Owned" : request.status.Trim(),
                    notes = request.notes,
                    personalRating = request.personalRating,
                    isPublic = request.isPublic,
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.UserSetOwnerships.Add(newOwnership);
                await _context.SaveChangesAsync(cancellationToken);
                action = "created";
                ownershipId = newOwnership.id;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                $"Add set — {legoSet.setNumber}, qty={request.quantity}", ownershipId.ToString());

            return Ok(new { action, id = ownershipId });
        }


        // ─────────────────────────────────────────────────────────────────
        // PUT /api/my-sets/{id} — update ownership
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Updates ownership details: quantity, status, notes, rating, visibility.
        /// </summary>
        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-sets/{id}")]
        public async Task<IActionResult> UpdateSet(int id, [FromBody] UpdateSetRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null) return BadRequest();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var ownership = await _context.UserSetOwnerships
                .FirstOrDefaultAsync(o =>
                    o.id == id
                    && o.tenantGuid == tenantGuid
                    && o.active == true
                    && o.deleted == false, cancellationToken);

            if (ownership == null) return NotFound("Set ownership not found.");

            if (request.quantity.HasValue) ownership.quantity = Math.Max(request.quantity.Value, 1);
            if (request.status != null) ownership.status = request.status.Trim();
            if (request.notes != null) ownership.notes = request.notes;
            if (request.personalRating.HasValue) ownership.personalRating = request.personalRating.Value;
            if (request.isPublic.HasValue) ownership.isPublic = request.isPublic.Value;

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Update set ownership — id={id}", id.ToString());

            return Ok(new { id = ownership.id, quantity = ownership.quantity, status = ownership.status });
        }


        // ─────────────────────────────────────────────────────────────────
        // DELETE /api/my-sets/{id} — remove from collection
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Soft-deletes a set ownership record.
        /// </summary>
        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-sets/{id}")]
        public async Task<IActionResult> RemoveSet(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var ownership = await _context.UserSetOwnerships
                .FirstOrDefaultAsync(o =>
                    o.id == id
                    && o.tenantGuid == tenantGuid
                    && o.active == true
                    && o.deleted == false, cancellationToken);

            if (ownership == null) return NotFound("Set ownership not found.");

            ownership.deleted = true;
            ownership.active = false;
            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                $"Remove set from collection — id={id}", id.ToString());

            return Ok(new { action = "removed" });
        }


        // ─────────────────────────────────────────────────────────────────
        // GET /api/my-sets/themes — distinct themes for filtering
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns distinct themes from the user's owned sets for filter UI.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-sets/themes")]
        public async Task<IActionResult> GetThemes(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var themes = await _context.UserSetOwnerships
                .Where(o => o.tenantGuid == tenantGuid && o.active == true && o.deleted == false)
                .Join(_context.LegoSets, o => o.legoSetId, s => s.id, (o, s) => s)
                .Where(s => s.legoThemeId != null)
                .Select(s => new { id = s.legoThemeId.Value, name = s.legoTheme.name })
                .Distinct()
                .OrderBy(t => t.name)
                .ToListAsync(cancellationToken);

            return Ok(themes);
        }


        // ─────────────────────────────────────────────────────────────────
        // Helper: get tenant GUID or fail
        // ─────────────────────────────────────────────────────────────────

        private async Task<Guid> GetTenantGuidOrFail(CancellationToken cancellationToken)
        {
            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            try
            {
                return await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error,
                    $"Non-tenant user attempted to access my-sets — {securityUser?.accountName}",
                    securityUser?.accountName, ex);
                return Guid.Empty;
            }
        }
    }
}
