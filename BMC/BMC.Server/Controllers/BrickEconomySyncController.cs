using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.BMC.Database;
using BMC.BrickEconomy.Api;


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
        private readonly IConfiguration _configuration;
        private readonly ILogger<BrickEconomySyncController> _logger;


        public BrickEconomySyncController(
            BMCContext context,
            IConfiguration configuration,
            ILogger<BrickEconomySyncController> logger) : base("BMC", "BrickEconomySync")
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }


        #region DTOs

        public class BrickEconomyConnectRequest
        {
            public string apiKey { get; set; }
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

            //
            // Validate the API key by making a test call
            //
            try
            {
                using (var client = new BrickEconomyApiClient(request.apiKey,
                    msg => _logger.LogInformation(msg)))
                {
                    // Try to look up a well-known set to validate the API key
                    var testSet = await client.GetSetAsync("10294-1");

                    if (testSet != null)
                    {
                        _logger.LogInformation(
                            "BrickEconomy API key validation succeeded for tenant {TenantGuid} — test set: {SetName}",
                            userTenantGuid, testSet.Name);
                    }
                }
            }
            catch (BrickEconomyApiException ex)
            {
                _logger.LogWarning(ex, "BrickEconomy API key validation failed for tenant {TenantGuid}", userTenantGuid);

                return BadRequest(new { error = $"BrickEconomy API key validation failed: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during BrickEconomy API key validation for tenant {TenantGuid}", userTenantGuid);

                return BadRequest(new { error = "Failed to connect to BrickEconomy. Please check your API key." });
            }

            //
            // TODO: Store encrypted API key in BrickEconomyUserLink table
            //       (requires database migration to create the table)
            //
            _logger.LogInformation("BrickEconomy connected for tenant {TenantGuid} — key storage pending table creation", userTenantGuid);

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

            //
            // TODO: Remove BrickEconomyUserLink entry
            //
            _logger.LogInformation("BrickEconomy disconnected for tenant {TenantGuid}", userTenantGuid);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "BrickEconomy disconnected");

            return Ok(new { connected = false });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  STATUS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickeconomy-sync/status
        ///
        /// Returns the current BrickEconomy connection status.
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

            //
            // TODO: Look up BrickEconomyUserLink for this tenant
            //
            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Get BrickEconomy sync status");

            return Ok(new
            {
                isConnected = false,
                lastSyncDate = (DateTime?)null,
                lastSyncError = (string)null,
                dailyQuotaUsed = 0,
                dailyQuotaLimit = 100,
                message = "BrickEconomy integration is installed but not yet connected."
            });
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

            //
            // TODO: Retrieve stored API key from BrickEconomyUserLink
            //
            return BadRequest(new { error = "BrickEconomy connection required. Please connect your BrickEconomy account first." });
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

            //
            // TODO: Retrieve stored API key from BrickEconomyUserLink
            //
            return BadRequest(new { error = "BrickEconomy connection required. Please connect your BrickEconomy account first." });
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

            //
            // TODO: Retrieve stored API key from BrickEconomyUserLink
            //
            return BadRequest(new { error = "BrickEconomy connection required. Please connect your BrickEconomy account first." });
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

            //
            // TODO: Retrieve stored API key from BrickEconomyUserLink
            //
            return BadRequest(new { error = "BrickEconomy connection required. Please connect your BrickEconomy account first." });
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

            //
            // TODO: Retrieve stored API key from BrickEconomyUserLink
            //
            return BadRequest(new { error = "BrickEconomy connection required. Please connect your BrickEconomy account first." });
        }
    }
}
