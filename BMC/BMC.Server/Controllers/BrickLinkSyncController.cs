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
using BMC.BrickLink.Api;
using BMC.BrickLink.Api.Models.Responses;


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
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class BrickLinkSyncController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BrickLinkSyncController> _logger;


        public BrickLinkSyncController(
            BMCContext context,
            IConfiguration configuration,
            ILogger<BrickLinkSyncController> logger) : base("BMC", "BrickLinkSync")
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }


        #region DTOs

        public class BrickLinkConnectRequest
        {
            public string tokenValue { get; set; }
            public string tokenSecret { get; set; }
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
            StartAuditEventClock();

            if (request == null || string.IsNullOrWhiteSpace(request.tokenValue) || string.IsNullOrWhiteSpace(request.tokenSecret))
            {
                return BadRequest("Token value and token secret are required.");
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
            // Get the app-level consumer credentials from appsettings
            //
            string consumerKey = _configuration["BrickLink:ConsumerKey"];
            string consumerSecret = _configuration["BrickLink:ConsumerSecret"];

            if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerSecret))
            {
                return Problem("BrickLink consumer credentials are not configured on the server.");
            }

            //
            // Validate the tokens by making a test API call
            //
            try
            {
                using (var client = new BrickLinkApiClient(consumerKey, consumerSecret,
                    request.tokenValue, request.tokenSecret,
                    msg => _logger.LogInformation(msg)))
                {
                    // Try to get the color list — a simple, low-cost endpoint to validate tokens
                    var colors = await client.GetColorListAsync();

                    _logger.LogInformation(
                        "BrickLink token validation succeeded for tenant {TenantGuid} — {ColorCount} colors retrieved",
                        userTenantGuid, colors.Count);
                }
            }
            catch (BrickLinkApiException ex)
            {
                _logger.LogWarning(ex, "BrickLink token validation failed for tenant {TenantGuid}", userTenantGuid);

                return BadRequest(new { error = $"BrickLink token validation failed: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during BrickLink token validation for tenant {TenantGuid}", userTenantGuid);

                return BadRequest(new { error = "Failed to connect to BrickLink. Please check your credentials." });
            }

            //
            // TODO: Store encrypted token credentials in BrickLinkUserLink table
            //       (requires database migration to create the table)
            //
            _logger.LogInformation("BrickLink connected for tenant {TenantGuid} — token storage pending table creation", userTenantGuid);

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
            // TODO: Remove BrickLinkUserLink entry for this tenant
            //       (requires database migration to create the table)
            //
            _logger.LogInformation("BrickLink disconnected for tenant {TenantGuid}", userTenantGuid);

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
            // TODO: Look up BrickLinkUserLink for this tenant to determine connection state
            //       (requires database migration to create the table)
            //
            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Get BrickLink sync status");

            return Ok(new
            {
                isConnected = false,
                lastSyncDate = (DateTime?)null,
                lastSyncError = (string)null,
                message = "BrickLink integration is installed but not yet connected. Store credentials pending database table creation."
            });
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
            // TODO: Retrieve stored tokens and validate against BrickLink API
            //       (requires database migration to create BrickLinkUserLink table)
            //

            return Ok(new { valid = false, error = "BrickLink token storage not yet configured." });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  CATALOG & PRICE GUIDE (public enrichment endpoints)
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
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            //
            // TODO: Retrieve stored tokens from BrickLinkUserLink
            //       For now, use app-level credentials only (will fail without user tokens)
            //
            string consumerKey = _configuration["BrickLink:ConsumerKey"];
            string consumerSecret = _configuration["BrickLink:ConsumerSecret"];

            if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerSecret))
            {
                return Problem("BrickLink consumer credentials are not configured on the server.");
            }

            //
            // Placeholder — once BrickLinkUserLink table exists, retrieve user tokens from DB
            //
            return BadRequest(new { error = "BrickLink connection required. Please connect your BrickLink account first." });
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
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            //
            // Placeholder — once BrickLinkUserLink table exists, retrieve user tokens from DB
            //
            return BadRequest(new { error = "BrickLink connection required. Please connect your BrickLink account first." });
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
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            //
            // Placeholder — once BrickLinkUserLink table exists, retrieve user tokens from DB
            //
            return BadRequest(new { error = "BrickLink connection required. Please connect your BrickLink account first." });
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
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            //
            // Placeholder — once BrickLinkUserLink table exists, retrieve user tokens from DB
            //
            return BadRequest(new { error = "BrickLink connection required. Please connect your BrickLink account first." });
        }
    }
}
