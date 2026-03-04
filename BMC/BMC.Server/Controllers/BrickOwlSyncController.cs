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
    /// Controller for managing Brick Owl integration settings and sync operations.
    ///
    /// Brick Owl is a major LEGO marketplace with cross-platform ID mapping capabilities
    /// (BrickLink ↔ Brick Owl ↔ LEGO design IDs). Uses API key authentication.
    ///
    /// The syncDirection setting controls integration behavior:
    ///   - "Pull" — import catalog/availability data from Brick Owl
    ///   - "Push" — push collection changes to Brick Owl
    ///   - "Both" — bidirectional sync
    ///
    /// Endpoints:
    ///   POST  /api/brickowl-sync/connect             — Validate API key + store encrypted
    ///   POST  /api/brickowl-sync/disconnect           — Clear credentials
    ///   GET   /api/brickowl-sync/status               — Current sync status for UI display
    ///   GET   /api/brickowl-sync/key-health           — Validate stored API key
    ///   GET   /api/brickowl-sync/catalog/{boid}       — Catalog lookup by BOID
    ///   GET   /api/brickowl-sync/catalog-id           — Map external ID to BOID
    ///   GET   /api/brickowl-sync/availability/{boid}  — Pricing + availability
    ///   GET   /api/brickowl-sync/collection           — User's collection lots
    ///   GET   /api/brickowl-sync/wishlists            — User's wishlists
    ///   GET   /api/brickowl-sync/wishlists/{id}/items — Items in a wishlist
    ///   GET   /api/brickowl-sync/transactions         — Paginated transaction history
    ///   PUT   /api/brickowl-sync/settings             — Update sync direction
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class BrickOwlSyncController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        private const int MAX_PAGE_SIZE = 200;

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

        public class BrickOwlUpdateSettingsRequest
        {
            public string syncDirection { get; set; }
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
        /// Validate API key via a test catalog lookup, then store encrypted.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/connect")]
        public async Task<IActionResult> Connect([FromBody] BrickOwlConnectRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.apiKey))
            {
                return BadRequest("API key is required.");
            }

            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            var (success, connectError) = await _syncService.ConnectAsync(
                tenantGuid, request.apiKey, request.syncDirection, cancellationToken);

            if (!success)
            {
                return BadRequest(new { error = connectError });
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Brick Owl connected");

            return Ok(new { connected = true });
        }


        /// <summary>
        /// POST /api/brickowl-sync/disconnect
        ///
        /// Clear stored API key and disable sync.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/disconnect")]
        public async Task<IActionResult> Disconnect(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            await _syncService.DisconnectAsync(tenantGuid, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Brick Owl disconnected");

            return Ok(new { connected = false });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  STATUS & HEALTH
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickowl-sync/status
        ///
        /// Returns the current Brick Owl sync status.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/status")]
        public async Task<IActionResult> GetStatus(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var status = await _syncService.GetSyncStatusAsync(tenantGuid, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Get Brick Owl sync status");

            return Ok(status);
        }


        /// <summary>
        /// GET /api/brickowl-sync/key-health
        ///
        /// Validate the stored API key by making a test catalog lookup.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/key-health")]
        public async Task<IActionResult> KeyHealth(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            var (valid, healthError) = await _syncService.ValidateStoredKeyAsync(tenantGuid, cancellationToken);

            return Ok(new { valid = valid, error = healthError });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  CATALOG OPERATIONS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickowl-sync/catalog/{boid}
        ///
        /// Catalog lookup by Brick Owl ID (BOID).
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/catalog/{boid}")]
        public async Task<IActionResult> CatalogLookup(string boid, CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect first." });
            }

            try
            {
                using (client)
                {
                    var result = await client.CatalogLookupAsync(boid);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"Brick Owl catalog lookup: {boid}");

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brick Owl catalog lookup failed for BOID {Boid}", boid);
                return Problem("Failed to fetch Brick Owl catalog data. Please try again later.");
            }
        }


        /// <summary>
        /// GET /api/brickowl-sync/catalog-id
        ///
        /// Map an external ID (e.g., BrickLink ID, LEGO set number) to a Brick Owl BOID.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/catalog-id")]
        public async Task<IActionResult> CatalogIdLookup(string id, string type = null, string idType = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("ID parameter is required.");
            }

            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect first." });
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
                return Problem("Failed to look up Brick Owl ID. Please try again later.");
            }
        }


        /// <summary>
        /// GET /api/brickowl-sync/availability/{boid}
        ///
        /// Get pricing and availability information for an item by BOID.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/availability/{boid}")]
        public async Task<IActionResult> CatalogAvailability(string boid, CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect first." });
            }

            try
            {
                using (client)
                {
                    var result = await client.CatalogAvailabilityAsync(boid);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"Brick Owl availability: {boid}");

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brick Owl availability lookup failed for BOID {Boid}", boid);
                return Problem("Failed to fetch Brick Owl availability data. Please try again later.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  COLLECTION & WISHLISTS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickowl-sync/collection
        ///
        /// Get the user's Brick Owl collection lots.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/collection")]
        public async Task<IActionResult> GetCollectionLotsAsync(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect first." });
            }

            try
            {
                using (client)
                {
                    var lots = await client.GetOrderListAsync();

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Brick Owl collection lots");

                    return Ok(lots);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brick Owl collection lots failed");
                return Problem("Failed to fetch Brick Owl collection. Please try again later.");
            }
        }


        /// <summary>
        /// GET /api/brickowl-sync/wishlists
        ///
        /// Get the user's Brick Owl wishlists.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/wishlists")]
        public async Task<IActionResult> GetWishlistsAsync(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect first." });
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
                _logger.LogWarning(ex, "Brick Owl wishlists failed");
                return Problem("Failed to fetch Brick Owl wishlists. Please try again later.");
            }
        }


        /// <summary>
        /// GET /api/brickowl-sync/wishlists/{wishlistId}/items
        ///
        /// Get items within a specific Brick Owl wishlist.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/wishlists/{wishlistId}/items")]
        public async Task<IActionResult> GetWishlistItemsAsync(string wishlistId, CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "Brick Owl connection required. Please connect first." });
            }

            try
            {
                using (client)
                {
                    var items = await client.GetWishlistItemsAsync(wishlistId);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"Brick Owl wishlist items: {wishlistId}");

                    return Ok(items);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brick Owl wishlist items failed for wishlist {WishlistId}", wishlistId);
                return Problem("Failed to fetch Brick Owl wishlist items. Please try again later.");
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
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            IQueryable<BrickOwlTransactionDto> query = _context.BrickOwlTransactions
                .Where(t => t.tenantGuid == tenantGuid && t.active == true && t.deleted == false)
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

            int ps = Math.Clamp(pageSize ?? 50, 1, MAX_PAGE_SIZE);
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


        // ═══════════════════════════════════════════════════════════════════════
        //  SETTINGS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// PUT /api/brickowl-sync/settings
        ///
        /// Update sync direction.
        /// </summary>
        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickowl-sync/settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] BrickOwlUpdateSettingsRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            var link = await _syncService.GetUserLinkAsync(tenantGuid, cancellationToken);
            if (link == null)
            {
                return BadRequest("Not connected to Brick Owl. Connect first.");
            }

            // Validate sync direction
            string[] validDirections = { BrickOwlSyncService.DIRECTION_PULL,
                                         BrickOwlSyncService.DIRECTION_PUSH,
                                         BrickOwlSyncService.DIRECTION_BOTH };

            if (!string.IsNullOrEmpty(request.syncDirection) && !validDirections.Contains(request.syncDirection))
            {
                return BadRequest($"Invalid sync direction. Valid options: {string.Join(", ", validDirections)}");
            }

            if (!string.IsNullOrEmpty(request.syncDirection))
            {
                link.syncDirection = request.syncDirection;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Updated Brick Owl settings — direction={link.syncDirection}");

            return Ok(new
            {
                syncDirection = link.syncDirection,
                syncEnabled = link.syncEnabled
            });
        }
    }
}
