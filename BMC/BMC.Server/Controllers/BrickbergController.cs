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
using Foundation.BMC.Services;
using BMC.BrickLink.Sync;
using BMC.BrickEconomy.Sync;
using BMC.BrickOwl.Sync;


namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Controller for the Brickberg Terminal — a Bloomberg-style financial dashboard for LEGO sets
    /// and minifigures.
    ///
    /// Aggregates data from BrickLink, BrickEconomy, and Brick Owl into a single unified response.
    /// All API calls are fired in parallel on the server for maximum performance.
    ///
    /// Endpoints:
    ///   GET  /api/brickberg/set/{setNumber}            — Unified market data for a LEGO set
    ///   GET  /api/brickberg/minifig/{minifigNumber}    — Minifig valuation data
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
        private readonly MarketDataCacheService _cacheService;
        private readonly ILogger<BrickbergController> _logger;


        public BrickbergController(
            BMCContext context,
            BrickLinkSyncService blSyncService,
            BrickEconomySyncService beSyncService,
            BrickOwlSyncService boSyncService,
            MarketDataCacheService cacheService,
            ILogger<BrickbergController> logger) : base("BMC", "Brickberg")
        {
            _context = context;
            _blSyncService = blSyncService;
            _beSyncService = beSyncService;
            _boSyncService = boSyncService;
            _cacheService = cacheService;
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

        public class BrickbergMinifigResponse
        {
            public BrickbergSourceStatus brickLink { get; set; } = new();
            public BrickbergSourceStatus brickEconomy { get; set; } = new();
            public object priceGuideNew { get; set; }
            public object priceGuideUsed { get; set; }
            public object valuation { get; set; }
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
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var response = new BrickbergSetResponse();

            // ── Create clients (null if not connected) ──
            var blClient = await CreateBrickLinkClientSafe(tenantGuid, cancellationToken);
            var beClient = await CreateBrickEconomyClientSafe(tenantGuid, cancellationToken);
            var boClient = await CreateBrickOwlClientSafe(tenantGuid, cancellationToken);

            response.brickLink.connected = blClient != null;
            response.brickEconomy.connected = beClient != null;
            response.brickOwl.connected = boClient != null;

            // ── Fire all data queries in parallel using local variables ──
            // Each task captures results in local variables to avoid race conditions.
            // Each source is wrapped in cache-aware fetch: on cache hit, the API client
            // is not used and no quota is consumed.

            // BrickLink: new + used sold price guides (cached independently per condition)
            object localPriceNew = null;
            object localPriceUsed = null;
            bool blLoaded = false;
            string blError = null;

            async Task FetchBrickLinkAsync()
            {
                if (blClient == null) return;
                try
                {
                    using (blClient)
                    {
                        var newTask = _cacheService.GetOrFetchAsync<object>(
                            "BrickLink", "SET", setNumber, "N",
                            () => blClient.GetPriceGuideAsync("SET", setNumber, null, "sold", "N", null, null),
                            cancellationToken);

                        var usedTask = _cacheService.GetOrFetchAsync<object>(
                            "BrickLink", "SET", setNumber, "U",
                            () => blClient.GetPriceGuideAsync("SET", setNumber, null, "sold", "U", null, null),
                            cancellationToken);

                        await Task.WhenAll(newTask, usedTask);

                        localPriceNew = newTask.Result;
                        localPriceUsed = usedTask.Result;
                        blLoaded = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Brickberg: BrickLink price guide failed for {SetNumber}", setNumber);
                    blError = "BrickLink price guide unavailable.";
                }
            }

            // BrickEconomy: AI valuation (cached with no condition qualifier)
            object localValuation = null;
            bool beLoaded = false;
            string beError = null;

            async Task FetchBrickEconomyAsync()
            {
                if (beClient == null) return;
                try
                {
                    using (beClient)
                    {
                        localValuation = await _cacheService.GetOrFetchAsync<object>(
                            "BrickEconomy", "SET", setNumber, null,
                            async () =>
                            {
                                var result = await beClient.GetSetAsync(setNumber, null);
                                await _beSyncService.IncrementQuotaAsync(tenantGuid, 1, cancellationToken);
                                return result;
                            },
                            cancellationToken);
                        beLoaded = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Brickberg: BrickEconomy valuation failed for {SetNumber}", setNumber);
                    beError = "BrickEconomy valuation unavailable.";
                }
            }

            // Brick Owl: BOID lookup → availability (cached as a single entry)
            object localAvailability = null;
            string localBoid = null;
            bool boLoaded = false;
            string boError = null;

            async Task FetchBrickOwlAsync()
            {
                if (boClient == null) return;
                try
                {
                    using (boClient)
                    {
                        // Cache the BOID lookup separately — avoids ID mapping API call on cache hit
                        var idLookup = await _cacheService.GetOrFetchAsync<object>(
                            "BrickOwl", "BOID", setNumber, null,
                            () => boClient.CatalogIdLookupAsync(setNumber, "Set", null),
                            cancellationToken);
                        string boid = ExtractBoid(idLookup);

                        if (!string.IsNullOrEmpty(boid))
                        {
                            localBoid = boid;
                            localAvailability = await _cacheService.GetOrFetchAsync<object>(
                                "BrickOwl", "SET", setNumber, null,
                                () => boClient.CatalogAvailabilityAsync(boid),
                                cancellationToken);
                            boLoaded = true;
                        }
                        else
                        {
                            boError = "No Brick Owl BOID found for this set.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Brickberg: Brick Owl lookup failed for {SetNumber}", setNumber);
                    boError = "Brick Owl data unavailable.";
                }
            }

            // Run all three in parallel
            await Task.WhenAll(FetchBrickLinkAsync(), FetchBrickEconomyAsync(), FetchBrickOwlAsync());

            // Assign results to response (single-threaded now, no race)
            response.priceGuideNew = localPriceNew;
            response.priceGuideUsed = localPriceUsed;
            response.brickLink.loaded = blLoaded;
            response.brickLink.error = blError;

            response.valuation = localValuation;
            response.brickEconomy.loaded = beLoaded;
            response.brickEconomy.error = beError;

            response.availability = localAvailability;
            response.brickOwlBoid = localBoid;
            response.brickOwl.loaded = boLoaded;
            response.brickOwl.error = boError;

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity,
                $"Brickberg set data: {setNumber} — BL={blLoaded}, BE={beLoaded}, BO={boLoaded}");

            return Ok(response);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  MINIFIG MARKET DATA
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/brickberg/minifig/{minifigNumber}
        ///
        /// Returns unified market data for a LEGO minifigure by aggregating:
        /// - BrickLink: new and used sold price guides
        /// - BrickEconomy: AI-powered minifig valuation
        ///
        /// Brick Owl is not used here because minifig BOID lookup is not reliable.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickberg/minifig/{minifigNumber}")]
        public async Task<IActionResult> GetMinifigMarketData(string minifigNumber,
            CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var response = new BrickbergMinifigResponse();

            var blClient = await CreateBrickLinkClientSafe(tenantGuid, cancellationToken);
            var beClient = await CreateBrickEconomyClientSafe(tenantGuid, cancellationToken);

            response.brickLink.connected = blClient != null;
            response.brickEconomy.connected = beClient != null;

            // Local variables for thread safety — cache-wrapped fetches
            object localPriceNew = null;
            object localPriceUsed = null;
            bool blLoaded = false;
            string blError = null;

            async Task FetchBrickLinkAsync()
            {
                if (blClient == null) return;
                try
                {
                    using (blClient)
                    {
                        var newTask = _cacheService.GetOrFetchAsync<object>(
                            "BrickLink", "MINIFIG", minifigNumber, "N",
                            () => blClient.GetPriceGuideAsync("MINIFIG", minifigNumber, null, "sold", "N", null, null),
                            cancellationToken);

                        var usedTask = _cacheService.GetOrFetchAsync<object>(
                            "BrickLink", "MINIFIG", minifigNumber, "U",
                            () => blClient.GetPriceGuideAsync("MINIFIG", minifigNumber, null, "sold", "U", null, null),
                            cancellationToken);

                        await Task.WhenAll(newTask, usedTask);

                        localPriceNew = newTask.Result;
                        localPriceUsed = usedTask.Result;
                        blLoaded = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Brickberg: BrickLink minifig price guide failed for {MinifigNumber}", minifigNumber);
                    blError = "BrickLink price guide unavailable.";
                }
            }

            object localValuation = null;
            bool beLoaded = false;
            string beError = null;

            async Task FetchBrickEconomyAsync()
            {
                if (beClient == null) return;
                try
                {
                    using (beClient)
                    {
                        localValuation = await _cacheService.GetOrFetchAsync<object>(
                            "BrickEconomy", "MINIFIG", minifigNumber, null,
                            async () =>
                            {
                                var result = await beClient.GetMinifigAsync(minifigNumber, null);
                                await _beSyncService.IncrementQuotaAsync(tenantGuid, 1, cancellationToken);
                                return result;
                            },
                            cancellationToken);
                        beLoaded = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Brickberg: BrickEconomy minifig valuation failed for {MinifigNumber}", minifigNumber);
                    beError = "BrickEconomy valuation unavailable.";
                }
            }

            await Task.WhenAll(FetchBrickLinkAsync(), FetchBrickEconomyAsync());

            response.priceGuideNew = localPriceNew;
            response.priceGuideUsed = localPriceUsed;
            response.brickLink.loaded = blLoaded;
            response.brickLink.error = blError;

            response.valuation = localValuation;
            response.brickEconomy.loaded = beLoaded;
            response.brickEconomy.error = beError;

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity,
                $"Brickberg minifig data: {minifigNumber} — BL={blLoaded}, BE={beLoaded}");

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


        // ═══════════════════════════════════════════════════════════════
        //  Cache Metrics
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns cache hit/miss metrics and database-level entry statistics.
        /// Useful for monitoring cache effectiveness from the System Health dashboard.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/brickberg/cache-stats")]
        public async Task<IActionResult> GetCacheStats(CancellationToken cancellationToken = default)
        {
            var (tenantGuid, error) = await ResolveTenantAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            if (error != null) return error;

            var stats = await _cacheService.GetStatsAsync(cancellationToken);

            return Ok(stats);
        }
    }
}
