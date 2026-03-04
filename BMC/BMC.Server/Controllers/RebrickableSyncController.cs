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
    ///   POST  /api/rebrickable-sync/reauthenticate — Refresh token without losing settings
    ///   GET   /api/rebrickable-sync/status        — Current sync status for UI display
    ///   GET   /api/rebrickable-sync/token-health  — Validate stored token
    ///   POST  /api/rebrickable-sync/pull          — Manual full pull
    ///   POST  /api/rebrickable-sync/pull-sets     — Pull only sets
    ///   GET   /api/rebrickable-sync/transactions  — Paginated transaction history
    ///   PUT   /api/rebrickable-sync/settings      — Update integration mode / pull interval
    /// </summary>
    public class RebrickableSyncController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        private const int MAX_PAGE_SIZE = 200;

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
            public string username { get; set; }
            public string password { get; set; }
            public string userToken { get; set; }
            public string authMode { get; set; } = "ApiToken";
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
            public string methodName { get; set; }
            public string httpMethod { get; set; }
            public string endpoint { get; set; }
            public string requestSummary { get; set; }
            public int responseStatusCode { get; set; }
            public bool success { get; set; }
            public string errorMessage { get; set; }
            public string triggeredBy { get; set; }
            public int? recordCount { get; set; }
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
            if (request == null || string.IsNullOrWhiteSpace(request.apiToken))
            {
                return BadRequest("API token is required.");
            }

            // Validate required fields based on auth mode
            string authMode = request.authMode ?? RebrickableSyncService.AUTH_API_TOKEN;

            if (authMode == RebrickableSyncService.AUTH_TOKEN_ONLY)
            {
                if (string.IsNullOrWhiteSpace(request.userToken))
                    return BadRequest("User token is required for Token Only mode.");
            }
            else // LoginOnce or SessionOnly both need username + password
            {
                if (string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.password))
                    return BadRequest("Username and password are required for this auth mode.");
            }

            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            // Route to the correct service method based on auth mode
            (bool success, string connectError) result;

            if (authMode == RebrickableSyncService.AUTH_TOKEN_ONLY)
            {
                result = await _syncService.ConnectWithDirectTokenAsync(
                    tenantGuid, request.apiToken, request.userToken,
                    request.integrationMode, cancellationToken);
            }
            else if (authMode == RebrickableSyncService.AUTH_SESSION_ONLY)
            {
                result = await _syncService.ConnectSessionOnlyAsync(
                    tenantGuid, request.apiToken, request.username, request.password,
                    request.integrationMode, cancellationToken);
            }
            else
            {
                result = await _syncService.ConnectWithTokenAsync(
                    tenantGuid, request.apiToken, request.username, request.password,
                    request.integrationMode, cancellationToken);
            }

            if (!result.success)
            {
                return BadRequest(new { error = result.connectError });
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, $"Rebrickable connected ({authMode})");

            return Ok(new { connected = true, authMode = authMode });
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
            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            await _syncService.DisconnectAsync(tenantGuid, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Rebrickable disconnected");

            return Ok(new { connected = false });
        }


        /// <summary>
        /// POST /api/rebrickable-sync/reauthenticate
        ///
        /// Refresh the stored token without losing sync settings.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/reauthenticate")]
        public async Task<IActionResult> Reauthenticate([FromBody] ConnectRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.apiToken))
            {
                return BadRequest("API token is required.");
            }

            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            var (success, reauthError) = await _syncService.ReauthenticateAsync(
                tenantGuid, request.apiToken, request.username, request.password,
                request.userToken, request.authMode ?? RebrickableSyncService.AUTH_API_TOKEN,
                cancellationToken);

            if (!success)
            {
                return BadRequest(new { error = reauthError });
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Rebrickable re-authenticated");

            return Ok(new { reauthenticated = true });
        }


        /// <summary>
        /// GET /api/rebrickable-sync/token-health
        ///
        /// Validate the currently stored token by calling the Rebrickable profile endpoint.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/token-health")]
        public async Task<IActionResult> TokenHealth(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            var (valid, healthError) = await _syncService.ValidateStoredTokenAsync(tenantGuid, cancellationToken);

            return Ok(new { valid = valid, error = healthError });
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
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var status = await _syncService.GetSyncStatusAsync(tenantGuid, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Get Rebrickable sync status");

            return Ok(status);
        }


        /// <summary>
        /// POST /api/rebrickable-sync/pull
        ///
        /// Trigger a full pull from Rebrickable — imports sets, set lists, part lists, and lost parts.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/pull")]
        public async Task<IActionResult> PullFull(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            var result = await _syncService.PullFullCollectionAsync(tenantGuid, cancellationToken);

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
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/rebrickable-sync/pull-sets")]
        public async Task<IActionResult> PullSets(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            var result = await _syncService.PullSetsAsync(tenantGuid, cancellationToken);

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
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            IQueryable<TransactionDto> query = _context.RebrickableTransactions
                .Where(t => t.tenantGuid == tenantGuid && t.active == true && t.deleted == false)
                .Select(t => new TransactionDto
                {
                    id = t.id,
                    transactionDate = t.transactionDate ?? DateTime.MinValue,
                    direction = t.direction,
                    methodName = t.requestSummary,
                    httpMethod = t.httpMethod,
                    endpoint = t.endpoint,
                    requestSummary = t.requestSummary,
                    responseStatusCode = t.responseStatusCode ?? 0,
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
            if (request == null)
            {
                return BadRequest();
            }

            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            var link = await _syncService.GetUserLinkAsync(tenantGuid, cancellationToken);
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
