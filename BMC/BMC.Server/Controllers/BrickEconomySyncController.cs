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
using BMC.BrickEconomy.Sync;


namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Controller for managing BrickEconomy integration and set/minifig valuation.
    ///
    /// BrickEconomy provides AI/ML-powered valuation data for LEGO sets and minifigures,
    /// including current market value, growth metrics, and future price forecasts.
    ///
    /// Endpoints:
    ///   POST  /api/brickeconomy-sync/connect          — Validate API key
    ///   POST  /api/brickeconomy-sync/disconnect        — Clear stored API key
    ///   GET   /api/brickeconomy-sync/status            — Current connection status
    ///   GET   /api/brickeconomy-sync/set/{setNumber}   — Get set valuation
    ///   GET   /api/brickeconomy-sync/minifig/{minifigNumber} — Get minifig valuation
    ///   GET   /api/brickeconomy-sync/collection/sets   — Get user's collection with valuations
    ///   GET   /api/brickeconomy-sync/collection/minifigs — Get user's minifig collection
    ///   GET   /api/brickeconomy-sync/salesledger       — Get sales transaction history
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class BrickEconomySyncController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly BrickEconomySyncService _syncService;
        private readonly ILogger<BrickEconomySyncController> _logger;


        public BrickEconomySyncController(
            BMCContext context,
            BrickEconomySyncService syncService,
            ILogger<BrickEconomySyncController> logger) : base("BMC", "BrickEconomySync")
        {
            _context = context;
            _syncService = syncService;
            _logger = logger;
        }


        #region DTOs

        public class BrickEconomyConnectRequest
        {
            public string apiKey { get; set; }
        }

        public class BrickEconomyTransactionDto
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
            public int? dailyQuotaRemaining { get; set; }
        }

        #endregion


        // ═══════════════════════════════════════════════════════════════════════
        //  CONNECTION
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/brickeconomy-sync/connect
        ///
        /// Validate the provided API key by making a test API call,
        /// then store the encrypted key.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickeconomy-sync/connect")]
        public async Task<IActionResult> Connect([FromBody] BrickEconomyConnectRequest request, CancellationToken cancellationToken = default)
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

            var (success, error) = await _syncService.ConnectAsync(userTenantGuid, request.apiKey, cancellationToken);

            if (!success)
            {
                return BadRequest(new { error });
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "BrickEconomy connected");

            return Ok(new { connected = true });
        }


        /// <summary>
        /// POST /api/brickeconomy-sync/disconnect
        ///
        /// Clear stored BrickEconomy API key.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickeconomy-sync/disconnect")]
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

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "BrickEconomy disconnected");

            return Ok(new { connected = false });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  STATUS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickeconomy-sync/status
        ///
        /// Returns the current BrickEconomy connection status with quota info.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickeconomy-sync/status")]
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

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Get BrickEconomy sync status");

            return Ok(status);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  SET & MINIFIG VALUATION
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickeconomy-sync/set/{setNumber}
        ///
        /// Get the AI-powered valuation for a specific LEGO set.
        /// Returns current value, forecast value, growth metrics, and price history.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickeconomy-sync/set/{setNumber}")]
        public async Task<IActionResult> GetSetValuation(string setNumber, string currency = null,
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
                return BadRequest(new { error = "BrickEconomy connection required. Please connect your BrickEconomy account first." });
            }

            try
            {
                using (client)
                {
                    var set = await client.GetSetAsync(setNumber, currency);

                    await _syncService.IncrementQuotaAsync(userTenantGuid, 1, cancellationToken);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"BrickEconomy set valuation: {setNumber}");

                    return Ok(set);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickEconomy set valuation failed for {SetNumber}", setNumber);
                return Problem($"Failed to fetch BrickEconomy set valuation: {ex.Message}");
            }
        }


        /// <summary>
        /// GET /api/brickeconomy-sync/minifig/{minifigNumber}
        ///
        /// Get the valuation for a specific LEGO minifigure.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickeconomy-sync/minifig/{minifigNumber}")]
        public async Task<IActionResult> GetMinifigValuation(string minifigNumber, string currency = null,
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
                return BadRequest(new { error = "BrickEconomy connection required. Please connect your BrickEconomy account first." });
            }

            try
            {
                using (client)
                {
                    var minifig = await client.GetMinifigAsync(minifigNumber, currency);

                    await _syncService.IncrementQuotaAsync(userTenantGuid, 1, cancellationToken);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"BrickEconomy minifig valuation: {minifigNumber}");

                    return Ok(minifig);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickEconomy minifig valuation failed for {MinifigNumber}", minifigNumber);
                return Problem($"Failed to fetch BrickEconomy minifig valuation: {ex.Message}");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  COLLECTION
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickeconomy-sync/collection/sets
        ///
        /// Get the user's BrickEconomy set collection with current valuations.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickeconomy-sync/collection/sets")]
        public async Task<IActionResult> GetCollectionSets(string currency = null,
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
                return BadRequest(new { error = "BrickEconomy connection required. Please connect your BrickEconomy account first." });
            }

            try
            {
                using (client)
                {
                    var sets = await client.GetCollectionSetsAsync(currency);

                    await _syncService.IncrementQuotaAsync(userTenantGuid, 1, cancellationToken);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "BrickEconomy collection sets");

                    return Ok(sets);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickEconomy collection sets failed");
                return Problem($"Failed to fetch BrickEconomy collection: {ex.Message}");
            }
        }


        /// <summary>
        /// GET /api/brickeconomy-sync/collection/minifigs
        ///
        /// Get the user's BrickEconomy minifig collection with current valuations.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickeconomy-sync/collection/minifigs")]
        public async Task<IActionResult> GetCollectionMinifigs(string currency = null,
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
                return BadRequest(new { error = "BrickEconomy connection required. Please connect your BrickEconomy account first." });
            }

            try
            {
                using (client)
                {
                    var minifigs = await client.GetCollectionMinifigsAsync(currency);

                    await _syncService.IncrementQuotaAsync(userTenantGuid, 1, cancellationToken);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "BrickEconomy collection minifigs");

                    return Ok(minifigs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickEconomy collection minifigs failed");
                return Problem($"Failed to fetch BrickEconomy minifig collection: {ex.Message}");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  SALES LEDGER
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickeconomy-sync/salesledger
        ///
        /// Get the user's sales ledger — transaction history for sets and minifigs.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickeconomy-sync/salesledger")]
        public async Task<IActionResult> GetSalesLedger(CancellationToken cancellationToken = default)
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
                return BadRequest(new { error = "BrickEconomy connection required. Please connect your BrickEconomy account first." });
            }

            try
            {
                using (client)
                {
                    var ledger = await client.GetSalesLedgerAsync();

                    await _syncService.IncrementQuotaAsync(userTenantGuid, 1, cancellationToken);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "BrickEconomy sales ledger");

                    return Ok(ledger);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickEconomy sales ledger failed");
                return Problem($"Failed to fetch BrickEconomy sales ledger: {ex.Message}");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  TRANSACTIONS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickeconomy-sync/transactions
        ///
        /// Returns paginated transaction history.
        /// Sorted newest-first.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickeconomy-sync/transactions")]
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

            IQueryable<BrickEconomyTransactionDto> query = _context.BrickEconomyTransactions
                .Where(t => t.tenantGuid == userTenantGuid && t.active == true && t.deleted == false)
                .Select(t => new BrickEconomyTransactionDto
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
                    dailyQuotaRemaining = t.dailyQuotaRemaining
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

            List<BrickEconomyTransactionDto> transactions = await query.AsNoTracking().ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"Get BrickEconomy transactions — page={pn}, results={transactions.Count}");

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
