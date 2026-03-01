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
using BMC.Rebrickable.Sync;


namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Controller for managing Rebrickable integration settings and sync operations.
    ///
    /// Endpoints:
    ///   POST  /api/rebrickable-sync/connect      — Validate token + store credentials
    ///   POST  /api/rebrickable-sync/disconnect    — Clear credentials, set mode to None
    ///   GET   /api/rebrickable-sync/status        — Current sync status for UI display
    ///   POST  /api/rebrickable-sync/pull          — Manual full pull
    ///   POST  /api/rebrickable-sync/pull-sets     — Pull only sets
    ///   GET   /api/rebrickable-sync/transactions  — Paginated transaction history
    ///   PUT   /api/rebrickable-sync/settings      — Update integration mode / pull interval
    /// </summary>
    public class RebrickableSyncController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly RebrickableSyncService _syncService;
        private readonly ILogger<RebrickableSyncController> _logger;


        public RebrickableSyncController(
            BMCContext context,
            RebrickableSyncService syncService,
            ILogger<RebrickableSyncController> logger) : base("BMC", "RebrickableSync")
        {
            _context = context;
            _syncService = syncService;
            _logger = logger;
        }


        #region DTOs

        public class ConnectRequest
        {
            public string apiToken { get; set; }
            public string integrationMode { get; set; } = "RealTime";
        }

        public class UpdateSettingsRequest
        {
            public string integrationMode { get; set; }
            public int? pullIntervalMinutes { get; set; }
        }

        public class TransactionDto
        {
            public int id { get; set; }
            public DateTime transactionDate { get; set; }
            public string direction { get; set; }
            public string httpMethod { get; set; }
            public string endpoint { get; set; }
            public string requestSummary { get; set; }
            public int responseStatusCode { get; set; }
            public bool success { get; set; }
            public string errorMessage { get; set; }
            public string triggeredBy { get; set; }
        }

        #endregion


        /// <summary>
        /// POST /api/rebrickable-sync/connect
        ///
        /// Validate the API token by calling Rebrickable, then store credentials and set integration mode.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/connect")]
        public async Task<IActionResult> Connect([FromBody] ConnectRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null || string.IsNullOrWhiteSpace(request.apiToken))
            {
                return BadRequest("API token is required.");
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

            var (success, error) = await _syncService.ConnectWithTokenAsync(
                userTenantGuid, request.apiToken, request.integrationMode, cancellationToken);

            if (!success)
            {
                return BadRequest(new { error = error });
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Rebrickable connected");

            return Ok(new { connected = true });
        }


        /// <summary>
        /// POST /api/rebrickable-sync/disconnect
        ///
        /// Clear stored credentials and set integration mode to None.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/disconnect")]
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

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Rebrickable disconnected");

            return Ok(new { connected = false });
        }


        /// <summary>
        /// GET /api/rebrickable-sync/status
        ///
        /// Returns the current sync status: connection state, integration mode, last sync dates,
        /// error info, and transaction counts.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/status")]
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

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Get Rebrickable sync status");

            return Ok(status);
        }


        /// <summary>
        /// POST /api/rebrickable-sync/pull
        ///
        /// Trigger a full pull from Rebrickable — imports sets, set lists, part lists, and lost parts.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.OnePerFiveSeconds, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/pull")]
        public async Task<IActionResult> PullFull(CancellationToken cancellationToken = default)
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

            var result = await _syncService.PullFullCollectionAsync(userTenantGuid, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Full Rebrickable pull — created={result.TotalCreated}, updated={result.TotalUpdated}, errors={result.ErrorCount}");

            return Ok(result);
        }


        /// <summary>
        /// POST /api/rebrickable-sync/pull-sets
        ///
        /// Pull only sets from Rebrickable.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.OnePerFiveSeconds, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/pull-sets")]
        public async Task<IActionResult> PullSets(CancellationToken cancellationToken = default)
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

            var result = await _syncService.PullSetsAsync(userTenantGuid, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Rebrickable set pull — created={result.SetsCreated}, updated={result.SetsUpdated}");

            return Ok(result);
        }


        /// <summary>
        /// GET /api/rebrickable-sync/transactions
        ///
        /// Returns paginated transaction history for the Communications Panel.
        /// Sorted newest-first.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/transactions")]
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

            IQueryable<TransactionDto> query = _context.RebrickableTransactions
                .Where(t => t.tenantGuid == userTenantGuid && t.active == true && t.deleted == false)
                .Select(t => new TransactionDto
                {
                    id = t.id,
                    transactionDate = t.transactionDate,
                    direction = t.direction,
                    httpMethod = t.httpMethod,
                    endpoint = t.endpoint,
                    requestSummary = t.requestSummary,
                    responseStatusCode = t.responseStatusCode,
                    success = t.success,
                    errorMessage = t.errorMessage,
                    triggeredBy = t.triggeredBy
                });

            // Filter by direction (Push/Pull)
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
                $"Get Rebrickable transactions — page={pn}, results={transactions.Count}");

            return Ok(new
            {
                totalCount = totalCount,
                pageSize = ps,
                pageNumber = pn,
                results = transactions
            });
        }


        /// <summary>
        /// PUT /api/rebrickable-sync/settings
        ///
        /// Update integration mode and pull interval.
        /// </summary>
        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/settings")]
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
                return BadRequest("Not connected to Rebrickable. Connect first.");
            }

            // Validate integration mode
            string[] validModes = { RebrickableSyncService.MODE_NONE, RebrickableSyncService.MODE_REALTIME,
                                    RebrickableSyncService.MODE_PUSH_ONLY, RebrickableSyncService.MODE_IMPORT_ONLY };

            if (!string.IsNullOrEmpty(request.integrationMode) && !validModes.Contains(request.integrationMode))
            {
                return BadRequest($"Invalid integration mode. Valid options: {string.Join(", ", validModes)}");
            }

            if (!string.IsNullOrEmpty(request.integrationMode))
            {
                link.syncDirectionFlags = request.integrationMode;
                link.syncEnabled = request.integrationMode != RebrickableSyncService.MODE_NONE;
            }

            if (request.pullIntervalMinutes.HasValue)
            {
                link.pullIntervalMinutes = request.pullIntervalMinutes.Value;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Updated Rebrickable settings — mode={link.syncDirectionFlags}, interval={link.pullIntervalMinutes}min");

            return Ok(new
            {
                integrationMode = link.syncDirectionFlags,
                pullIntervalMinutes = link.pullIntervalMinutes,
                syncEnabled = link.syncEnabled
            });
        }
    }
}
