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
using BMC.BrickOwl.Sync;


namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Controller for managing Brick Owl integration and marketplace data.
    ///
    /// Brick Owl is the second-largest LEGO marketplace, supporting catalog lookups,
    /// cross-platform ID mapping (BrickLink ↔ Brick Owl ↔ LEGO), pricing/availability,
    /// store inventory management, order tracking, and wishlists.
    ///
    /// Endpoints:
    ///   POST  /api/brickowl-sync/connect               — Validate API key
    ///   POST  /api/brickowl-sync/disconnect              — Clear stored API key
    ///   GET   /api/brickowl-sync/status                  — Current connection status
    ///   GET   /api/brickowl-sync/catalog/lookup/{boid}   — Look up item by BOID
    ///   GET   /api/brickowl-sync/catalog/id-lookup       — Map external IDs to BOIDs
    ///   GET   /api/brickowl-sync/catalog/availability/{boid} — Get pricing/availability
    ///   GET   /api/brickowl-sync/collection              — Get personal collection
    ///   GET   /api/brickowl-sync/wishlists               — Get wishlists
    ///   GET   /api/brickowl-sync/wishlist/{id}/items     — Get wishlist items
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class BrickOwlSyncController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly BrickOwlSyncService _syncService;
        private readonly ILogger<BrickOwlSyncController> _logger;


        public BrickOwlSyncController(
            BMCContext context,
            BrickOwlSyncService syncService,
            ILogger<BrickOwlSyncController> logger) : base("BMC", "BrickOwlSync")
        {
            _context = context;
            _syncService = syncService;
            _logger = logger;
        }


        #region DTOs

        public class BrickOwlConnectRequest
        {
            public string apiKey { get; set; }
            public string syncDirection { get; set; } = "Pull";
        }

        public class BrickOwlTransactionDto
        {
            public int id { get; set; }
            public DateTime transactionDate { get; set; }
            public string direction { get; set; }
            public string methodName { get; set; }
            public string requestSummary { get; set; }
            public bool success { get; set; }
            public string errorMessage { get; set; }
            public string triggeredBy { get; set; }
            public int? recordCount { get; set; }
        }

        #endregion


        // ═══════════════════════════════════════════════════════════════════════
        //  CONNECTION
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/brickowl-sync/connect
        ///
        /// Validate the provided API key by making a test catalog lookup,
        /// then store the encrypted key.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/connect")]
        public async Task<IActionResult> Connect([FromBody] BrickOwlConnectRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null || string.IsNullOrWhiteSpace(request.apiKey))
            {
                return BadRequest("API key is required.");
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
                return Problem("Your user account is not configured with a tenant.");
            }

            var (success, error) = await _syncService.ConnectAsync(
                userTenantGuid, request.apiKey, request.syncDirection, cancellationToken);

            if (!success)
            {
                return BadRequest(new { error });
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Brick Owl connected");

            return Ok(new { connected = true });
        }


        /// <summary>
        /// POST /api/brickowl-sync/disconnect
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/disconnect")]
        public async Task<IActionResult> Disconnect(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

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
                return Problem("Your user account is not configured with a tenant.");
            }

            await _syncService.DisconnectAsync(userTenantGuid, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Brick Owl disconnected");

            return Ok(new { connected = false });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  STATUS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickowl-sync/status
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/status")]
        public async Task<IActionResult> GetStatus(CancellationToken cancellationToken = default)
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
                return Problem("Your user account is not configured with a tenant.");
            }

            var status = await _syncService.GetSyncStatusAsync(userTenantGuid, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Get Brick Owl sync status");

            return Ok(status);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  CATALOG
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickowl-sync/catalog/lookup/{boid}
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/catalog/lookup/{boid}")]
        public async Task<IActionResult> CatalogLookup(string boid, CancellationToken cancellationToken = default)
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
                return Problem("Your user account is not configured with a tenant.");
            }

            var client = await _syncService.CreateClientAsync(userTenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect your Brick Owl account first." });
            }

            try
            {
                using (client)
                {
                    var item = await client.CatalogLookupAsync(boid);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"Brick Owl catalog lookup: {boid}");

                    return Ok(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brick Owl catalog lookup failed for BOID {Boid}", boid);
                return Problem($"Failed to fetch Brick Owl catalog data: {ex.Message}");
            }
        }


        /// <summary>
        /// GET /api/brickowl-sync/catalog/id-lookup
        ///
        /// Map an external ID to Brick Owl BOIDs.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/catalog/id-lookup")]
        public async Task<IActionResult> CatalogIdLookup(string id, string type, string idType = null,
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
                return Problem("Your user account is not configured with a tenant.");
            }

            var client = await _syncService.CreateClientAsync(userTenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect your Brick Owl account first." });
            }

            try
            {
                using (client)
                {
                    var result = await client.CatalogIdLookupAsync(id, type, idType);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"Brick Owl ID lookup: {id}");

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brick Owl ID lookup failed for {Id}", id);
                return Problem($"Failed Brick Owl ID lookup: {ex.Message}");
            }
        }


        /// <summary>
        /// GET /api/brickowl-sync/catalog/availability/{boid}
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/catalog/availability/{boid}")]
        public async Task<IActionResult> CatalogAvailability(string boid, CancellationToken cancellationToken = default)
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
                return Problem("Your user account is not configured with a tenant.");
            }

            var client = await _syncService.CreateClientAsync(userTenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect your Brick Owl account first." });
            }

            try
            {
                using (client)
                {
                    var availability = await client.CatalogAvailabilityAsync(boid);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"Brick Owl availability: {boid}");

                    return Ok(availability);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brick Owl availability failed for BOID {Boid}", boid);
                return Problem($"Failed to fetch Brick Owl availability: {ex.Message}");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  COLLECTION & WISHLISTS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickowl-sync/collection
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/collection")]
        public async Task<IActionResult> GetCollection(CancellationToken cancellationToken = default)
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
                return Problem("Your user account is not configured with a tenant.");
            }

            var client = await _syncService.CreateClientAsync(userTenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect your Brick Owl account first." });
            }

            try
            {
                using (client)
                {
                    var collection = await client.GetCollectionLotsAsync();

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Brick Owl collection");

                    return Ok(collection);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brick Owl collection fetch failed");
                return Problem($"Failed to fetch Brick Owl collection: {ex.Message}");
            }
        }


        /// <summary>
        /// GET /api/brickowl-sync/wishlists
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/wishlists")]
        public async Task<IActionResult> GetWishlists(CancellationToken cancellationToken = default)
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
                return Problem("Your user account is not configured with a tenant.");
            }

            var client = await _syncService.CreateClientAsync(userTenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect your Brick Owl account first." });
            }

            try
            {
                using (client)
                {
                    var wishlists = await client.GetWishlistsAsync();

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Brick Owl wishlists");

                    return Ok(wishlists);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brick Owl wishlists fetch failed");
                return Problem($"Failed to fetch Brick Owl wishlists: {ex.Message}");
            }
        }


        /// <summary>
        /// GET /api/brickowl-sync/wishlist/{id}/items
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/wishlist/{id}/items")]
        public async Task<IActionResult> GetWishlistItems(string id, CancellationToken cancellationToken = default)
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
                return Problem("Your user account is not configured with a tenant.");
            }

            var client = await _syncService.CreateClientAsync(userTenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect your Brick Owl account first." });
            }

            try
            {
                using (client)
                {
                    var items = await client.GetWishlistItemsAsync(id);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"Brick Owl wishlist items: {id}");

                    return Ok(items);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brick Owl wishlist items failed for {Id}", id);
                return Problem($"Failed to fetch Brick Owl wishlist items: {ex.Message}");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  TRANSACTIONS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickowl-sync/transactions
        ///
        /// Returns paginated transaction history.
        /// Sorted newest-first.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/transactions")]
        public async Task<IActionResult> GetTransactions(
            int? pageSize = 50, int? pageNumber = 1,
            string direction = null, bool? success = null,
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
                return Problem("Your user account is not configured with a tenant.");
            }

            IQueryable<BrickOwlTransactionDto> query = _context.BrickOwlTransactions
                .Where(t => t.tenantGuid == userTenantGuid && t.active == true && t.deleted == false)
                .Select(t => new BrickOwlTransactionDto
                {
                    id = t.id,
                    transactionDate = t.transactionDate ?? DateTime.MinValue,
                    direction = t.direction,
                    methodName = t.methodName,
                    requestSummary = t.requestSummary,
                    success = t.success,
                    errorMessage = t.errorMessage,
                    triggeredBy = t.triggeredBy,
                    recordCount = t.recordCount
                });

            if (!string.IsNullOrWhiteSpace(direction))
            {
                query = query.Where(t => t.direction == direction);
            }

            if (success.HasValue)
            {
                query = query.Where(t => t.success == success.Value);
            }

            query = query.OrderByDescending(t => t.transactionDate);

            int totalCount = await query.CountAsync(cancellationToken);

            int ps = Math.Max(pageSize ?? 50, 1);
            int pn = Math.Max(pageNumber ?? 1, 1);
            query = query.Skip((pn - 1) * ps).Take(ps);

            List<BrickOwlTransactionDto> transactions = await query.AsNoTracking().ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"Get Brick Owl transactions — page={pn}, results={transactions.Count}");

            return Ok(new
            {
                totalCount = totalCount,
                pageSize = ps,
                pageNumber = pn,
                results = transactions
            });
        }
    }
}
