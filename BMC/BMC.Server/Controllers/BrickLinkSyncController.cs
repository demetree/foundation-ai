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
using BMC.BrickLink.Sync;


namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Controller for managing BrickLink integration settings and data enrichment.
    ///
    /// BrickLink is the world's largest LEGO secondary marketplace and provides the only
    /// source for real secondary market pricing at the individual part level.
    ///
    /// Endpoints:
    ///   POST  /api/bricklink-sync/connect         — Validate OAuth tokens + store encrypted credentials
    ///   POST  /api/bricklink-sync/disconnect       — Clear stored credentials
    ///   GET   /api/bricklink-sync/status           — Current connection status
    ///   GET   /api/bricklink-sync/token-health     — Validate stored tokens
    ///   GET   /api/bricklink-sync/price-guide/{type}/{no}  — Get price guide for an item
    ///   GET   /api/bricklink-sync/item/{type}/{no} — Get catalog item details
    ///   GET   /api/bricklink-sync/subsets/{type}/{no}      — Part-out a set
    ///   GET   /api/bricklink-sync/supersets/{type}/{no}    — Find sets containing a part
    ///   GET   /api/bricklink-sync/transactions     — Paginated transaction history
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class BrickLinkSyncController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        private const int MAX_PAGE_SIZE = 200;

        private readonly BMCContext _context;
        private readonly BrickLinkSyncService _syncService;
        private readonly ILogger<BrickLinkSyncController> _logger;


        public BrickLinkSyncController(
            BMCContext context,
            BrickLinkSyncService syncService,
            ILogger<BrickLinkSyncController> logger) : base("BMC", "BrickLinkSync")
        {
            _context = context;
            _syncService = syncService;
            _logger = logger;
        }


        #region DTOs

        public class BrickLinkConnectRequest
        {
            public string tokenValue { get; set; }
            public string tokenSecret { get; set; }
            public string syncDirection { get; set; } = "Pull";
        }

        public class BrickLinkTransactionDto
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
        /// POST /api/bricklink-sync/connect
        ///
        /// Validate the provided OAuth tokens by making a test API call,
        /// then store the encrypted token credentials.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/bricklink-sync/connect")]
        public async Task<IActionResult> Connect([FromBody] BrickLinkConnectRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.tokenValue) || string.IsNullOrWhiteSpace(request.tokenSecret))
            {
                return BadRequest("Token value and token secret are required.");
            }

            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            var (success, connectError) = await _syncService.ConnectAsync(
                tenantGuid, request.tokenValue, request.tokenSecret,
                request.syncDirection, cancellationToken);

            if (!success)
            {
                return BadRequest(new { error = connectError });
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "BrickLink connected");

            return Ok(new { connected = true });
        }


        /// <summary>
        /// POST /api/bricklink-sync/disconnect
        ///
        /// Clear stored BrickLink credentials.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/bricklink-sync/disconnect")]
        public async Task<IActionResult> Disconnect(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            await _syncService.DisconnectAsync(tenantGuid, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "BrickLink disconnected");

            return Ok(new { connected = false });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  STATUS & HEALTH
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/bricklink-sync/status
        ///
        /// Returns the current BrickLink connection status.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/bricklink-sync/status")]
        public async Task<IActionResult> GetStatus(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var status = await _syncService.GetSyncStatusAsync(tenantGuid, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Get BrickLink sync status");

            return Ok(status);
        }


        /// <summary>
        /// GET /api/bricklink-sync/token-health
        ///
        /// Validate the stored OAuth tokens by making a test API call.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/bricklink-sync/token-health")]
        public async Task<IActionResult> TokenHealth(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync("BMC Collection Writer", cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return Ok(new { valid = false, error = "No stored BrickLink credentials. Please connect your account." });
            }

            try
            {
                using (client)
                {
                    var colors = await client.GetColorListAsync();
                    return Ok(new { valid = true, colorCount = colors?.Count ?? 0 });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickLink token health check failed for tenant {TenantGuid}", tenantGuid);
                return Ok(new { valid = false, error = "Token validation failed. Please reconnect." });
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  CATALOG & PRICE GUIDE (data endpoints using stored credentials)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/bricklink-sync/price-guide/{type}/{no}
        ///
        /// Get the BrickLink price guide for a specific item.
        /// Returns real secondary market pricing with min/max/average prices.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/bricklink-sync/price-guide/{type}/{no}")]
        public async Task<IActionResult> GetPriceGuide(string type, string no,
            int? colorId = null, string guideType = null, string newOrUsed = null,
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "BrickLink connection required. Please connect your BrickLink account first." });
            }

            try
            {
                using (client)
                {
                    var priceGuide = await client.GetPriceGuideAsync(type, no,
                        colorId, guideType, newOrUsed, null, currencyCode);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"BrickLink price guide: {type}/{no}");

                    return Ok(priceGuide);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickLink price guide failed for {Type}/{No}", type, no);
                return Problem("Failed to fetch BrickLink price guide. Please try again later.");
            }
        }


        /// <summary>
        /// GET /api/bricklink-sync/item/{type}/{no}
        ///
        /// Get catalog item details from BrickLink.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/bricklink-sync/item/{type}/{no}")]
        public async Task<IActionResult> GetItem(string type, string no, CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "BrickLink connection required. Please connect your BrickLink account first." });
            }

            try
            {
                using (client)
                {
                    var item = await client.GetItemAsync(type, no);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"BrickLink item: {type}/{no}");

                    return Ok(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickLink item lookup failed for {Type}/{No}", type, no);
                return Problem("Failed to fetch BrickLink item. Please try again later.");
            }
        }


        /// <summary>
        /// GET /api/bricklink-sync/subsets/{type}/{no}
        ///
        /// Get the subset (part-out) of a set — lists all parts that make up the set.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/bricklink-sync/subsets/{type}/{no}")]
        public async Task<IActionResult> GetSubsets(string type, string no,
            bool? breakMinifigs = null, bool? breakSubsets = null,
            CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "BrickLink connection required. Please connect your BrickLink account first." });
            }

            try
            {
                using (client)
                {
                    var subsets = await client.GetSubSetsAsync(type, no, null, breakMinifigs, breakSubsets);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"BrickLink subsets: {type}/{no}");

                    return Ok(subsets);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickLink subsets failed for {Type}/{No}", type, no);
                return Problem("Failed to fetch BrickLink subsets. Please try again later.");
            }
        }


        /// <summary>
        /// GET /api/bricklink-sync/supersets/{type}/{no}
        ///
        /// Find sets that contain the specified part.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/bricklink-sync/supersets/{type}/{no}")]
        public async Task<IActionResult> GetSupersets(string type, string no,
            int? colorId = null,
            CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var client = await _syncService.CreateClientAsync(tenantGuid, cancellationToken);
            if (client == null)
            {
                return BadRequest(new { error = "BrickLink connection required. Please connect your BrickLink account first." });
            }

            try
            {
                using (client)
                {
                    var supersets = await client.GetSuperSetsAsync(type, no, colorId);

                    await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"BrickLink supersets: {type}/{no}");

                    return Ok(supersets);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickLink supersets failed for {Type}/{No}", type, no);
                return Problem("Failed to fetch BrickLink supersets. Please try again later.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  TRANSACTIONS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/bricklink-sync/transactions
        ///
        /// Returns paginated transaction history.
        /// Sorted newest-first.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/bricklink-sync/transactions")]
        public async Task<IActionResult> GetTransactions(
            int? pageSize = 50, int? pageNumber = 1,
            string direction = null, bool? success = null,
            CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            IQueryable<BrickLinkTransactionDto> query = _context.BrickLinkTransactions
                .Where(t => t.tenantGuid == tenantGuid && t.active == true && t.deleted == false)
                .Select(t => new BrickLinkTransactionDto
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

            List<BrickLinkTransactionDto> transactions = await query.AsNoTracking().ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"Get BrickLink transactions — page={pn}, results={transactions.Count}");

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
