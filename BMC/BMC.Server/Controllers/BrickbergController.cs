using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.BMC.Database;
using BMC.BrickLink.Sync;
using BMC.BrickEconomy.Sync;
using BMC.BrickOwl.Sync;


namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Controller for the Brickberg Terminal — a Bloomberg-style financial dashboard for LEGO sets.
    ///
    /// Aggregates data from BrickLink, BrickEconomy, and Brick Owl into a single unified response.
    /// All API calls are fired in parallel on the server for maximum performance.
    ///
    /// Endpoints:
    ///   GET  /api/brickberg/set/{setNumber}  — Unified market data for a LEGO set
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class BrickbergController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly BrickLinkSyncService _blSyncService;
        private readonly BrickEconomySyncService _beSyncService;
        private readonly BrickOwlSyncService _boSyncService;
        private readonly ILogger<BrickbergController> _logger;


        public BrickbergController(
            BMCContext context,
            BrickLinkSyncService blSyncService,
            BrickEconomySyncService beSyncService,
            BrickOwlSyncService boSyncService,
            ILogger<BrickbergController> logger) : base("BMC", "Brickberg")
        {
            _context = context;
            _blSyncService = blSyncService;
            _beSyncService = beSyncService;
            _boSyncService = boSyncService;
            _logger = logger;
        }


        #region DTOs

        public class BrickbergSourceStatus
        {
            public bool connected { get; set; }
            public bool loaded { get; set; }
            public string error { get; set; }
        }

        public class BrickbergSetResponse
        {
            public BrickbergSourceStatus brickLink { get; set; } = new();
            public BrickbergSourceStatus brickEconomy { get; set; } = new();
            public BrickbergSourceStatus brickOwl { get; set; } = new();
            public object priceGuideNew { get; set; }
            public object priceGuideUsed { get; set; }
            public object valuation { get; set; }
            public object availability { get; set; }
            public string brickOwlBoid { get; set; }
        }

        #endregion


        // ═══════════════════════════════════════════════════════════════════════
        //  SET MARKET DATA
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickberg/set/{setNumber}
        ///
        /// Returns unified market data for a LEGO set by aggregating:
        /// - BrickLink: new and used sold price guides
        /// - BrickEconomy: AI-powered valuation with growth/forecast
        /// - Brick Owl: marketplace availability (via BOID lookup)
        ///
        /// All sources are queried in parallel. Individual source failures
        /// are captured gracefully — the response always returns with
        /// per-source status indicators.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickberg/set/{setNumber}")]
        public async Task<IActionResult> GetSetMarketData(string setNumber,
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

            var response = new BrickbergSetResponse();

            // ── Create clients (null if not connected) ──
            var blClientTask = CreateBrickLinkClientSafe(userTenantGuid, cancellationToken);
            var beClientTask = CreateBrickEconomyClientSafe(userTenantGuid, cancellationToken);
            var boClientTask = CreateBrickOwlClientSafe(userTenantGuid, cancellationToken);

            await Task.WhenAll(blClientTask, beClientTask, boClientTask);

            var blClient = blClientTask.Result;
            var beClient = beClientTask.Result;
            var boClient = boClientTask.Result;

            response.brickLink.connected = blClient != null;
            response.brickEconomy.connected = beClient != null;
            response.brickOwl.connected = boClient != null;

            // ── Fire all data queries in parallel ──
            var tasks = new System.Collections.Generic.List<Task>();

            // BrickLink: new + used sold price guides
            if (blClient != null)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (blClient)
                        {
                            // Fire both price guide calls in parallel
                            var newTask = blClient.GetPriceGuideAsync("SET", setNumber,
                                null, "sold", "N", null, null);
                            var usedTask = blClient.GetPriceGuideAsync("SET", setNumber,
                                null, "sold", "U", null, null);

                            await Task.WhenAll(newTask, usedTask);

                            response.priceGuideNew = newTask.Result;
                            response.priceGuideUsed = usedTask.Result;
                            response.brickLink.loaded = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Brickberg: BrickLink price guide failed for {SetNumber}", setNumber);
                        response.brickLink.error = ex.Message;
                    }
                }, cancellationToken));
            }

            // BrickEconomy: AI valuation
            if (beClient != null)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (beClient)
                        {
                            var valuation = await beClient.GetSetAsync(setNumber, null);
                            response.valuation = valuation;
                            response.brickEconomy.loaded = true;

                            await _beSyncService.IncrementQuotaAsync(userTenantGuid, 1, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Brickberg: BrickEconomy valuation failed for {SetNumber}", setNumber);
                        response.brickEconomy.error = ex.Message;
                    }
                }, cancellationToken));
            }

            // Brick Owl: BOID lookup → availability
            if (boClient != null)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (boClient)
                        {
                            // Step 1: Map set number to BOID
                            var idLookup = await boClient.CatalogIdLookupAsync(setNumber, "Set", null);

                            // The API returns a dictionary-like object; extract the first BOID
                            string boid = ExtractBoid(idLookup);
                            if (!string.IsNullOrEmpty(boid))
                            {
                                response.brickOwlBoid = boid;

                                // Step 2: Get availability for that BOID
                                var availability = await boClient.CatalogAvailabilityAsync(boid);
                                response.availability = availability;
                                response.brickOwl.loaded = true;
                            }
                            else
                            {
                                response.brickOwl.error = "No Brick Owl BOID found for this set.";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Brickberg: Brick Owl lookup failed for {SetNumber}", setNumber);
                        response.brickOwl.error = ex.Message;
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity,
                $"Brickberg set data: {setNumber} — BL={response.brickLink.loaded}, BE={response.brickEconomy.loaded}, BO={response.brickOwl.loaded}");

            return Ok(response);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        private async Task<dynamic> CreateBrickLinkClientSafe(Guid tenantGuid, CancellationToken ct)
        {
            try
            {
                return await _blSyncService.CreateClientAsync(tenantGuid, ct);
            }
            catch
            {
                return null;
            }
        }

        private async Task<dynamic> CreateBrickEconomyClientSafe(Guid tenantGuid, CancellationToken ct)
        {
            try
            {
                return await _beSyncService.CreateClientAsync(tenantGuid, ct);
            }
            catch
            {
                return null;
            }
        }

        private async Task<dynamic> CreateBrickOwlClientSafe(Guid tenantGuid, CancellationToken ct)
        {
            try
            {
                return await _boSyncService.CreateClientAsync(tenantGuid, ct);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extract the first BOID from a Brick Owl catalog ID lookup response.
        /// The API returns varying formats — this safely extracts the first value.
        /// </summary>
        private static string ExtractBoid(object idLookupResult)
        {
            if (idLookupResult == null) return null;

            // The result is typically a JObject or dynamic with a "boids" array or similar.
            // Use dynamic dispatch to extract the first BOID regardless of shape.
            try
            {
                dynamic result = idLookupResult;

                // Try common response shapes
                if (result is System.Collections.IEnumerable enumerable)
                {
                    foreach (dynamic item in enumerable)
                    {
                        string boid = item?.boid?.ToString() ?? item?.ToString();
                        if (!string.IsNullOrEmpty(boid)) return boid;
                    }
                }

                // Direct boid property
                string directBoid = result?.boid?.ToString();
                if (!string.IsNullOrEmpty(directBoid)) return directBoid;
            }
            catch
            {
                // Fall through
            }

            return null;
        }
    }
}
