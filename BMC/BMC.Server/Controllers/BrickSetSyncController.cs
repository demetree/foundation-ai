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
using BMC.BrickSet.Sync;


namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Controller for managing BrickSet integration settings and sync operations.
    ///
    /// Endpoints:
    ///   POST  /api/brickset-sync/connect           — Login + store encrypted credentials
    ///   POST  /api/brickset-sync/disconnect         — Clear credentials
    ///   GET   /api/brickset-sync/status             — Current sync status for UI display
    ///   GET   /api/brickset-sync/hash-health        — Validate stored userHash
    ///   POST  /api/brickset-sync/enrich/{setNumber} — Enrich a single set with BrickSet data
    ///   POST  /api/brickset-sync/enrich-reviews     — Cache reviews for a set
    ///   GET   /api/brickset-sync/quota              — Check daily API call budget
    ///   GET   /api/brickset-sync/transactions       — Paginated transaction history
    ///   PUT   /api/brickset-sync/settings           — Update sync direction
    /// </summary>
    public class BrickSetSyncController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly BrickSetSyncService _syncService;
        private readonly ILogger<BrickSetSyncController> _logger;


        public BrickSetSyncController(
            BMCContext context,
            BrickSetSyncService syncService,
            ILogger<BrickSetSyncController> logger) : base("BMC", "BrickSetSync")
        {
            _context = context;
            _syncService = syncService;
            _logger = logger;
        }


        #region DTOs

        public class ConnectRequest
        {
            public string username { get; set; }
            public string password { get; set; }
            public string syncDirection { get; set; } = "EnrichOnly";
        }

        public class UpdateSettingsRequest
        {
            public string syncDirection { get; set; }
        }

        public class EnrichReviewsRequest
        {
            public int legoSetId { get; set; }
            public int brickSetId { get; set; }
        }

        public class TransactionDto
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
            public int? apiCallsRemaining { get; set; }
        }

        #endregion


        // ═══════════════════════════════════════════════════════════════════════
        //  CONNECTION
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/brickset-sync/connect
        ///
        /// Login to BrickSet with username/password, obtain userHash,
        /// encrypt and store credentials.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickset-sync/connect")]
        public async Task<IActionResult> Connect([FromBody] ConnectRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null || string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.password))
            {
                return BadRequest("Username and password are required.");
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
                userTenantGuid, request.username, request.password,
                request.syncDirection ?? BrickSetSyncService.DIRECTION_ENRICH_ONLY,
                cancellationToken);

            if (!success)
            {
                return BadRequest(new { error = error });
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "BrickSet connected");

            return Ok(new { connected = true });
        }


        /// <summary>
        /// POST /api/brickset-sync/disconnect
        ///
        /// Clear stored credentials and disable sync.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickset-sync/disconnect")]
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

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "BrickSet disconnected");

            return Ok(new { connected = false });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  STATUS & HEALTH
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickset-sync/status
        ///
        /// Returns the current BrickSet sync status: connection state, sync direction,
        /// last sync dates, error info, transaction counts, and API quota.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickset-sync/status")]
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

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Get BrickSet sync status");

            return Ok(status);
        }


        /// <summary>
        /// GET /api/brickset-sync/hash-health
        ///
        /// Validate the stored userHash.  If expired, attempt to re-authenticate
        /// using stored encrypted credentials.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickset-sync/hash-health")]
        public async Task<IActionResult> HashHealth(CancellationToken cancellationToken = default)
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
                return Problem("Your user account is not configured with a tenant.");
            }

            var (valid, error) = await _syncService.ValidateAndRefreshHashAsync(userTenantGuid, cancellationToken);

            return Ok(new { valid = valid, error = error });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  ENRICHMENT
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/brickset-sync/enrich/{setNumber}
        ///
        /// Enrich a single set with BrickSet data — pricing, subtheme, availability,
        /// ratings, and instructions URL.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickset-sync/enrich/{setNumber}")]
        public async Task<IActionResult> EnrichSet(string setNumber, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (string.IsNullOrWhiteSpace(setNumber))
            {
                return BadRequest("Set number is required.");
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

            bool enriched = await _syncService.EnrichSetAsync(userTenantGuid, setNumber, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"BrickSet enrich set {setNumber} — {(enriched ? "success" : "not found")}");

            return Ok(new { enriched = enriched, setNumber = setNumber });
        }


        /// <summary>
        /// POST /api/brickset-sync/enrich-reviews
        ///
        /// Fetch and cache BrickSet reviews for a set.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickset-sync/enrich-reviews")]
        public async Task<IActionResult> EnrichReviews([FromBody] EnrichReviewsRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null)
            {
                return BadRequest("Request body is required.");
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

            int reviewsCached = await _syncService.EnrichSetReviewsAsync(
                userTenantGuid, request.legoSetId, request.brickSetId, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"BrickSet enrich reviews — {reviewsCached} cached for set ID {request.legoSetId}");

            return Ok(new { reviewsCached = reviewsCached, legoSetId = request.legoSetId });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  QUOTA
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickset-sync/quota
        ///
        /// Check the daily API call budget from BrickSet.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickset-sync/quota")]
        public async Task<IActionResult> GetQuota(CancellationToken cancellationToken = default)
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
                return Problem("Your user account is not configured with a tenant.");
            }

            int? callsToday = await _syncService.GetQuotaRemainingAsync(userTenantGuid, cancellationToken);

            return Ok(new { apiCallsToday = callsToday });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  TRANSACTIONS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickset-sync/transactions
        ///
        /// Returns paginated transaction history.
        /// Sorted newest-first.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickset-sync/transactions")]
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

            IQueryable<TransactionDto> query = _context.BrickSetTransactions
                .Where(t => t.tenantGuid == userTenantGuid && t.active == true && t.deleted == false)
                .Select(t => new TransactionDto
                {
                    id = t.id,
                    transactionDate = t.transactionDate ?? DateTime.MinValue,
                    direction = t.direction,
                    methodName = t.methodName,
                    requestSummary = t.requestSummary,
                    success = t.success,
                    errorMessage = t.errorMessage,
                    triggeredBy = t.triggeredBy,
                    recordCount = t.recordCount,
                    apiCallsRemaining = t.apiCallsRemaining
                });

            // Filter by direction (Pull/Push)
            if (!string.IsNullOrWhiteSpace(direction))
            {
                query = query.Where(t => t.direction == direction);
            }

            // Filter by success/failure
            if (success.HasValue)
            {
                query = query.Where(t => t.success == success.Value);
            }

            // Always newest first
            query = query.OrderByDescending(t => t.transactionDate);

            // Count before paging
            int totalCount = await query.CountAsync(cancellationToken);

            // Page
            int ps = Math.Max(pageSize ?? 50, 1);
            int pn = Math.Max(pageNumber ?? 1, 1);
            query = query.Skip((pn - 1) * ps).Take(ps);

            List<TransactionDto> transactions = await query.AsNoTracking().ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"Get BrickSet transactions — page={pn}, results={transactions.Count}");

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
        /// PUT /api/brickset-sync/settings
        ///
        /// Update sync direction.
        /// </summary>
        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickset-sync/settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

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
                return Problem("Your user account is not configured with a tenant.");
            }

            var link = await _syncService.GetUserLinkAsync(userTenantGuid, cancellationToken);
            if (link == null)
            {
                return BadRequest("Not connected to BrickSet. Connect first.");
            }

            // Validate sync direction
            string[] validDirections = { BrickSetSyncService.DIRECTION_NONE,
                                         BrickSetSyncService.DIRECTION_ENRICH_ONLY,
                                         BrickSetSyncService.DIRECTION_FULL };

            if (!string.IsNullOrEmpty(request.syncDirection) && !validDirections.Contains(request.syncDirection))
            {
                return BadRequest($"Invalid sync direction. Valid options: {string.Join(", ", validDirections)}");
            }

            if (!string.IsNullOrEmpty(request.syncDirection))
            {
                link.syncDirection = request.syncDirection;
                link.syncEnabled = request.syncDirection != BrickSetSyncService.DIRECTION_NONE;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Updated BrickSet settings — direction={link.syncDirection}");

            return Ok(new
            {
                syncDirection = link.syncDirection,
                syncEnabled = link.syncEnabled
            });
        }
    }
}
