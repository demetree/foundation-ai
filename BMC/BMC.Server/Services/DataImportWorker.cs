using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Foundation.BMC.Database;
using BMC.LDraw.Import;
using BMC.Rebrickable.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Foundation.BMC.Services
{
    //
    // AI-generated: Background worker that checks LDraw.org and Rebrickable for
    // upstream data changes on an hourly basis.  When changes are detected, it
    // downloads the updated files and runs the existing import services to upsert
    // new data into the BMC database.
    //
    // Change detection is lightweight — HTTP HEAD requests compare Last-Modified
    // timestamps against a persisted state file, so no data is downloaded unless
    // something actually changed.
    //

    public class DataImportWorker : BackgroundService
    {
        private const string LOG_PREFIX = "[DataImportWorker]";
        private const string STATE_FILENAME = "import-state.json";

        //
        // Rebrickable CSV file names (in the order they are downloaded)
        //
        private static readonly string[] REBRICKABLE_FILES = new string[]
        {
            "themes.csv.gz",
            "colors.csv.gz",
            "part_categories.csv.gz",
            "parts.csv.gz",
            "part_relationships.csv.gz",
            "elements.csv.gz",
            "sets.csv.gz",
            "minifigs.csv.gz",
            "inventories.csv.gz",
            "inventory_parts.csv.gz",
            "inventory_sets.csv.gz",
            "inventory_minifigs.csv.gz"
        };

        private const string REBRICKABLE_CDN_BASE = "https://cdn.rebrickable.com/media/downloads/";
        private const string LDRAW_OFFICIAL_URL = "https://library.ldraw.org/library/updates/complete.zip";
        private const string LDRAW_UNOFFICIAL_URL = "https://library.ldraw.org/library/unofficial/ldrawunf.zip";


        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataImportWorker> _logger;
        private readonly HttpClient _httpClient;

        private readonly PartsUniverseService _partsUniverseService;
        private readonly SetExplorerService _setExplorerService;
        private readonly MinifigGalleryService _minifigGalleryService;
        private readonly LDrawFileService _ldrawFileService;


        public DataImportWorker(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<DataImportWorker> logger,
            PartsUniverseService partsUniverseService,
            SetExplorerService setExplorerService,
            MinifigGalleryService minifigGalleryService,
            LDrawFileService ldrawFileService
        )
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
            _partsUniverseService = partsUniverseService;
            _setExplorerService = setExplorerService;
            _minifigGalleryService = minifigGalleryService;
            _ldrawFileService = ldrawFileService;

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(15);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BMC-Worker/1.0");
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //
            // Check if data import is enabled — if not, exit immediately.
            // The worker is always registered in DI but respects the config flag.
            //
            bool enabled = _configuration.GetValue<bool>("DataImport:Enabled", true);

            if (enabled == false)
            {
                _logger.LogInformation("{Prefix} Data import is disabled via configuration — worker will not run.", LOG_PREFIX);
                return;
            }

            //
            // Delay briefly to let the host finish starting up
            //
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);

            int intervalMinutes = _configuration.GetValue<int>("DataImport:IntervalMinutes", 60);

            //
            // Auto-bootstrap: check the persisted state to determine if bootstrap
            // is needed.  Uses BootstrapComplete flag instead of counting DB rows,
            // so a partially-completed bootstrap will always resume on next start.
            //
            bool autoBootstrap = _configuration.GetValue<bool>("DataImport:AutoBootstrap", true);

            if (autoBootstrap)
            {
                try
                {
                    string cachePath = _configuration.GetValue<string>("DataImport:DataCachePath", "./import-cache");
                    Directory.CreateDirectory(cachePath);
                    ImportState bootState = await LoadStateAsync(cachePath).ConfigureAwait(false);

                    //
                    // Safety net: if the state file says bootstrap is done but the
                    // database is effectively empty, the database was probably deleted
                    // without cleaning the cache folder.  Reset and re-bootstrap.
                    //
                    if (bootState.BootstrapComplete == true)
                    {
                        bool dbEmpty = await IsDatabaseEmptyAsync(stoppingToken).ConfigureAwait(false);

                        if (dbEmpty)
                        {
                            _logger.LogWarning(
                                "{Prefix} State file says bootstrap is complete, but database is empty — resetting bootstrap flag.",
                                LOG_PREFIX
                            );
                            bootState.BootstrapComplete = false;
                            bootState.CompletedSteps.Clear();
                            await SaveStateAsync(cachePath, bootState).ConfigureAwait(false);
                        }
                    }

                    if (bootState.BootstrapComplete == false)
                    {
                        int completedCount = bootState.CompletedSteps.Count;
                        bool isResume = completedCount > 0;

                        _logger.LogInformation(
                            "{Prefix} ═══════════════════════════════════════════════════════",
                            LOG_PREFIX
                        );

                        if (isResume)
                        {
                            _logger.LogInformation(
                                "{Prefix} BOOTSTRAP RESUME: {Count} steps already completed — picking up where we left off.",
                                LOG_PREFIX, completedCount
                            );
                        }
                        else
                        {
                            _logger.LogInformation(
                                "{Prefix} FIRST-RUN BOOTSTRAP: Starting full data import.",
                                LOG_PREFIX
                            );
                        }

                        _logger.LogInformation(
                            "{Prefix} This will download all Rebrickable CSVs and LDraw data files.",
                            LOG_PREFIX
                        );
                        _logger.LogInformation(
                            "{Prefix} This may take several minutes on first run.",
                            LOG_PREFIX
                        );
                        _logger.LogInformation(
                            "{Prefix} ═══════════════════════════════════════════════════════",
                            LOG_PREFIX
                        );

                        await RunImportCycleAsync(stoppingToken, isBootstrap: true).ConfigureAwait(false);

                        _logger.LogInformation(
                            "{Prefix} Bootstrap complete. Entering normal hourly check cycle.",
                            LOG_PREFIX
                        );

                        //
                        // Refresh the precomputation caches now that the database is populated.
                        // These services fire early during startup and may have cached empty results
                        // from an empty database before the bootstrap had a chance to run.
                        //
                        _logger.LogInformation(
                            "{Prefix} Refreshing precomputation caches after bootstrap...",
                            LOG_PREFIX
                        );

                        try
                        {
                            await _partsUniverseService.RefreshAsync().ConfigureAwait(false);
                            await _setExplorerService.RefreshAsync().ConfigureAwait(false);
                            await _minifigGalleryService.RefreshAsync().ConfigureAwait(false);

                            _logger.LogInformation(
                                "{Prefix} Precomputation caches refreshed.",
                                LOG_PREFIX
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(
                                ex,
                                "{Prefix} Failed to refresh precomputation caches after bootstrap (non-fatal).",
                                LOG_PREFIX
                            );
                        }
                    }
                    else
                    {
                        _logger.LogInformation(
                            "{Prefix} Bootstrap already completed — skipping.",
                            LOG_PREFIX
                        );
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("{Prefix} Bootstrap cancelled during shutdown — will resume on next start.", LOG_PREFIX);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Prefix} Bootstrap import failed — will continue with normal cycle.", LOG_PREFIX);
                }
            }

            _logger.LogInformation(
                "{Prefix} Starting hourly cycle — will check for updates every {Interval} minutes",
                LOG_PREFIX, intervalMinutes
            );

            while (stoppingToken.IsCancellationRequested == false)
            {
                //
                // Wait for the configured interval before the next check
                //
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken).ConfigureAwait(false);

                try
                {
                    await RunImportCycleAsync(stoppingToken, isBootstrap: false).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("{Prefix} Shutting down.", LOG_PREFIX);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Prefix} Import cycle failed — will retry next cycle.", LOG_PREFIX);
                }
            }
        }


        /// <summary>
        /// AI-developed: Checks whether the database is effectively empty.
        /// Uses BrickParts count as the indicator — if Rebrickable import has run,
        /// there should be 50,000+ parts.
        /// Used as a safety net to detect a wiped database when the state file
        /// still claims bootstrap is complete.
        /// </summary>
        private async Task<bool> IsDatabaseEmptyAsync(CancellationToken ct)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();

            BMCContext context = scope.ServiceProvider.GetRequiredService<BMCContext>();

            int partCount = await context.BrickParts
                .CountAsync(ct)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "{Prefix} Database check: {Count} parts in BrickParts table.",
                LOG_PREFIX, partCount
            );

            // Threshold of 100 — a handful of manually-added parts doesn't count as "populated"
            return partCount < 100;
        }


        // ─────────────────────────────────────────────────────────
        //  Main import cycle
        // ─────────────────────────────────────────────────────────

        private async Task RunImportCycleAsync(CancellationToken ct, bool isBootstrap)
        {
            _logger.LogInformation("{Prefix} ── Import cycle starting at {Time} ──", LOG_PREFIX, DateTime.UtcNow);

            string cachePath = _configuration.GetValue<string>("DataImport:DataCachePath", "./import-cache");
            Directory.CreateDirectory(cachePath);

            //
            // Load persisted state
            //
            ImportState state = await LoadStateAsync(cachePath).ConfigureAwait(false);

            bool rebrickableEnabled = _configuration.GetValue<bool>("DataImport:Rebrickable:Enabled", true);
            bool ldrawEnabled = _configuration.GetValue<bool>("DataImport:LDraw:Enabled", true);
            bool trackUnofficial = _configuration.GetValue<bool>("DataImport:LDraw:TrackUnofficial", true);

            //
            // 1. Rebrickable
            //
            if (rebrickableEnabled)
            {
                await CheckAndImportRebrickableAsync(cachePath, state, ct, isBootstrap).ConfigureAwait(false);
            }

            //
            // 2. LDraw Official
            //
            if (ldrawEnabled)
            {
                await CheckAndImportLDrawAsync(
                    url: LDRAW_OFFICIAL_URL,
                    label: "LDraw Official",
                    stateKey: "ldraw_official",
                    cachePath: cachePath,
                    state: state,
                    ct: ct,
                    isBootstrap: isBootstrap
                ).ConfigureAwait(false);
            }

            //
            // 3. LDraw Unofficial
            //
            if (ldrawEnabled && trackUnofficial)
            {
                await CheckAndImportLDrawAsync(
                    url: LDRAW_UNOFFICIAL_URL,
                    label: "LDraw Unofficial",
                    stateKey: "ldraw_unofficial",
                    cachePath: cachePath,
                    state: state,
                    ct: ct,
                    isBootstrap: isBootstrap
                ).ConfigureAwait(false);
            }

            //
            // Mark bootstrap complete if this was a bootstrap run
            //
            if (isBootstrap)
            {
                state.BootstrapComplete = true;
                _logger.LogInformation("{Prefix} All bootstrap steps completed — marking bootstrap as done.", LOG_PREFIX);
            }

            //
            // Persist updated state
            //
            await SaveStateAsync(cachePath, state).ConfigureAwait(false);

            _logger.LogInformation("{Prefix} ── Import cycle complete ──", LOG_PREFIX);
        }


        // ─────────────────────────────────────────────────────────
        //  Rebrickable: check + import
        // ─────────────────────────────────────────────────────────

        private async Task CheckAndImportRebrickableAsync(string cachePath, ImportState state, CancellationToken ct, bool isBootstrap)
        {
            _logger.LogInformation("{Prefix} Checking Rebrickable CDN for updates...", LOG_PREFIX);

            string rebrickableCachePath = Path.Combine(cachePath, "rebrickable");
            Directory.CreateDirectory(rebrickableCachePath);

            List<string> changedFiles = new List<string>();

            //
            // HEAD check each CSV file
            //
            foreach (string fileName in REBRICKABLE_FILES)
            {
                ct.ThrowIfCancellationRequested();

                string url = REBRICKABLE_CDN_BASE + fileName;
                string stateKey = "rebrickable_" + fileName;

                try
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url))
                    {
                        using (HttpResponseMessage response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false))
                        {
                            response.EnsureSuccessStatusCode();

                            string lastModified = response.Content.Headers.LastModified?.ToString("o") ?? "";

                            if (state.Timestamps.TryGetValue(stateKey, out string previousTimestamp) == false
                                || previousTimestamp != lastModified)
                            {
                                _logger.LogInformation(
                                    "{Prefix}   {File}: CHANGED (was: {Old}, now: {New})",
                                    LOG_PREFIX, fileName, previousTimestamp ?? "(first run)", lastModified
                                );
                                changedFiles.Add(fileName);
                                state.Timestamps[stateKey] = lastModified;
                            }
                            else
                            {
                                _logger.LogDebug("{Prefix}   {File}: unchanged", LOG_PREFIX, fileName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{Prefix}   HEAD check failed for {File}", LOG_PREFIX, fileName);
                }
            }

            //
            // During bootstrap: also count steps that haven't been marked complete yet,
            // even if the CDN timestamps haven't changed (files may already be cached).
            //
            bool hasIncompleteBootstrapSteps = false;

            if (isBootstrap)
            {
                foreach (string stepKey in REBRICKABLE_STEP_KEYS)
                {
                    if (state.CompletedSteps.Contains(stepKey) == false)
                    {
                        hasIncompleteBootstrapSteps = true;
                        break;
                    }
                }
            }

            if (changedFiles.Count == 0 && hasIncompleteBootstrapSteps == false)
            {
                _logger.LogInformation("{Prefix} Rebrickable: no changes detected.", LOG_PREFIX);
                return;
            }

            //
            // Download changed files (or all files during bootstrap if they aren't cached)
            //
            List<string> filesToDownload = isBootstrap
                ? new List<string>(REBRICKABLE_FILES)
                : changedFiles;

            foreach (string fileName in filesToDownload)
            {
                ct.ThrowIfCancellationRequested();

                string outputPath = Path.Combine(rebrickableCachePath, fileName.Replace(".gz", ""));

                //
                // During bootstrap, skip downloading files that are already cached on disk
                //
                if (isBootstrap && changedFiles.Contains(fileName) == false && File.Exists(outputPath))
                {
                    _logger.LogDebug("{Prefix}   {File}: using cached file", LOG_PREFIX, fileName);
                    continue;
                }

                await DownloadAndDecompressGzFileAsync(
                    url: REBRICKABLE_CDN_BASE + fileName,
                    outputPath: outputPath,
                    ct: ct
                ).ConfigureAwait(false);
            }

            _logger.LogInformation(
                "{Prefix} Rebrickable: importing data...",
                LOG_PREFIX
            );

            //
            // Run import in FK-dependency order using a scoped context.
            // Each step is individually tracked — if the server is interrupted,
            // already-completed steps are skipped on the next run.
            //
            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                BMCContext context = scope.ServiceProvider.GetRequiredService<BMCContext>();
                context.Database.SetCommandTimeout(300);

                Action<string> log = (string msg) => _logger.LogInformation("{Prefix} {Message}", LOG_PREFIX, msg);
                RebrickableImportService importService = new RebrickableImportService(context, log);

                string csvDir = rebrickableCachePath;

                string themesFile = Path.Combine(csvDir, "themes.csv");
                string coloursFile = Path.Combine(csvDir, "colors.csv");
                string categoriesFile = Path.Combine(csvDir, "part_categories.csv");
                string partsFile = Path.Combine(csvDir, "parts.csv");
                string relationshipsFile = Path.Combine(csvDir, "part_relationships.csv");
                string elementsFile = Path.Combine(csvDir, "elements.csv");
                string setsFile = Path.Combine(csvDir, "sets.csv");
                string minifigsFile = Path.Combine(csvDir, "minifigs.csv");
                string inventoriesFile = Path.Combine(csvDir, "inventories.csv");
                string inventoryPartsFile = Path.Combine(csvDir, "inventory_parts.csv");
                string inventorySetsFile = Path.Combine(csvDir, "inventory_sets.csv");
                string inventoryMinifigsFile = Path.Combine(csvDir, "inventory_minifigs.csv");

                //
                // Import in FK-dependency order with per-step tracking.
                // During bootstrap: skip steps already completed (resume support).
                // During normal cycle: always re-import changed files (no step gating).
                //

                await RunTrackedStepAsync("rebrickable_themes", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(themesFile))
                    {
                        await importService.ImportThemesAsync(themesFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                await RunTrackedStepAsync("rebrickable_colors", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(coloursFile))
                    {
                        await importService.ImportColoursAsync(coloursFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                await RunTrackedStepAsync("rebrickable_part_categories", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(categoriesFile))
                    {
                        await importService.ImportPartCategoriesAsync(categoriesFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                await RunTrackedStepAsync("rebrickable_parts", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(partsFile))
                    {
                        await importService.ImportPartsAsync(partsFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                await RunTrackedStepAsync("rebrickable_part_relationships", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(relationshipsFile))
                    {
                        await importService.ImportPartRelationshipsAsync(relationshipsFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                await RunTrackedStepAsync("rebrickable_elements", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(elementsFile))
                    {
                        await importService.ImportElementsAsync(elementsFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                await RunTrackedStepAsync("rebrickable_sets", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(setsFile))
                    {
                        await importService.ImportSetsAsync(setsFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                await RunTrackedStepAsync("rebrickable_minifigs", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(minifigsFile))
                    {
                        await importService.ImportMinifigsAsync(minifigsFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                await RunTrackedStepAsync("rebrickable_inventory_parts", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(inventoriesFile) && File.Exists(inventoryPartsFile))
                    {
                        await importService.ImportInventoryPartsAsync(inventoriesFile, inventoryPartsFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                await RunTrackedStepAsync("rebrickable_inventory_sets", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(inventoriesFile) && File.Exists(inventorySetsFile))
                    {
                        await importService.ImportInventorySetsAsync(inventoriesFile, inventorySetsFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                await RunTrackedStepAsync("rebrickable_inventory_minifigs", state, cachePath, isBootstrap, async () =>
                {
                    if (File.Exists(inventoriesFile) && File.Exists(inventoryMinifigsFile))
                    {
                        await importService.ImportInventoryMinifigsAsync(inventoriesFile, inventoryMinifigsFile).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

                //
                // Post-import reconciliation
                //
                await RunTrackedStepAsync("rebrickable_reconcile", state, cachePath, isBootstrap, async () =>
                {
                    await importService.ReconcileMovedPartsAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }

            _logger.LogInformation("{Prefix} Rebrickable import complete.", LOG_PREFIX);
        }


        //
        // AI-developed: Step-tracking keys for Rebrickable import steps.
        // Used during bootstrap to detect incomplete imports.
        //
        private static readonly string[] REBRICKABLE_STEP_KEYS = new string[]
        {
            "rebrickable_themes",
            "rebrickable_colors",
            "rebrickable_part_categories",
            "rebrickable_parts",
            "rebrickable_part_relationships",
            "rebrickable_elements",
            "rebrickable_sets",
            "rebrickable_minifigs",
            "rebrickable_inventory_parts",
            "rebrickable_inventory_sets",
            "rebrickable_inventory_minifigs",
            "rebrickable_reconcile"
        };


        /// <summary>
        /// AI-developed: Runs a single import step with optional skip-if-complete logic.
        /// During bootstrap, already-completed steps are skipped for resume support.
        /// During normal hourly cycles, steps always run (no gating).
        /// After successful completion, marks the step as done and saves state.
        /// </summary>
        private async Task RunTrackedStepAsync(
            string stepKey,
            ImportState state,
            string cachePath,
            bool isBootstrap,
            Func<Task> action
        )
        {
            if (isBootstrap && state.CompletedSteps.Contains(stepKey))
            {
                _logger.LogInformation(
                    "{Prefix}   BOOTSTRAP RESUME: Skipping '{Step}' (already completed)",
                    LOG_PREFIX, stepKey
                );
                return;
            }

            if (isBootstrap)
            {
                _logger.LogInformation(
                    "{Prefix}   BOOTSTRAP: Running '{Step}'...",
                    LOG_PREFIX, stepKey
                );
            }

            await action().ConfigureAwait(false);

            //
            // Mark step as completed and save state immediately.
            // This creates a checkpoint that survives crashes.
            //
            if (isBootstrap)
            {
                state.CompletedSteps.Add(stepKey);
                await SaveStateAsync(cachePath, state).ConfigureAwait(false);
            }
        }


        // ─────────────────────────────────────────────────────────
        //  LDraw: check + import
        // ─────────────────────────────────────────────────────────

        private async Task CheckAndImportLDrawAsync(
            string url,
            string label,
            string stateKey,
            string cachePath,
            ImportState state,
            CancellationToken ct,
            bool isBootstrap
        )
        {
            _logger.LogInformation("{Prefix} Checking {Label}...", LOG_PREFIX, label);

            try
            {
                //
                // During bootstrap, skip this source if it was already completed.
                //
                if (isBootstrap && state.CompletedSteps.Contains(stateKey))
                {
                    _logger.LogInformation(
                        "{Prefix}   BOOTSTRAP RESUME: Skipping '{Step}' (already completed)",
                        LOG_PREFIX, stateKey
                    );
                    return;
                }

                //
                // HEAD check
                //
                string lastModified = "";
                long contentLength = 0;

                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    using (HttpResponseMessage response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();

                        lastModified = response.Content.Headers.LastModified?.ToString("o") ?? "";
                        contentLength = response.Content.Headers.ContentLength ?? 0;
                    }
                }

                string compositeKey = $"{lastModified}|{contentLength}";

                if (state.Timestamps.TryGetValue(stateKey, out string previousValue)
                    && previousValue == compositeKey)
                {
                    //
                    // Timestamps match — data was already imported successfully.
                    // During bootstrap, just mark the step complete (no re-download needed).
                    //
                    if (isBootstrap)
                    {
                        _logger.LogInformation(
                            "{Prefix}   {Label}: unchanged — marking bootstrap step complete (no re-download needed).",
                            LOG_PREFIX, label
                        );
                        state.CompletedSteps.Add(stateKey);
                        await SaveStateAsync(cachePath, state).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogInformation("{Prefix}   {Label}: unchanged.", LOG_PREFIX, label);
                    }

                    return;
                }

                _logger.LogInformation(
                    "{Prefix}   {Label}: {Status} (was: {Old}, now: {New})",
                    LOG_PREFIX, label,
                    isBootstrap ? "BOOTSTRAP" : "CHANGED",
                    previousValue ?? "(first run)", compositeKey
                );

                //
                // Download and extract
                //
                string ldrawCachePath = Path.Combine(cachePath, stateKey.Replace("_", "-"));
                Directory.CreateDirectory(ldrawCachePath);

                Action<string> log = (string msg) => _logger.LogInformation("{Prefix} {Message}", LOG_PREFIX, msg);

                LDrawDownloadService downloadService = new LDrawDownloadService(log);
                string extractedPath = await downloadService.DownloadAndExtractAsync(
                    outputDir: ldrawCachePath,
                    forceDownload: true
                ).ConfigureAwait(false);

                //
                // Import into database
                //
                using (IServiceScope scope = _scopeFactory.CreateScope())
                {
                    BMCContext context = scope.ServiceProvider.GetRequiredService<BMCContext>();
                    context.Database.SetCommandTimeout(300);

                    LDrawImportService importService = new LDrawImportService(context, log);

                    //
                    // Import colours from LDConfig.ldr
                    //
                    string configFile = Path.Combine(extractedPath, "LDConfig.ldr");
                    if (File.Exists(configFile))
                    {
                        await importService.ImportColoursAsync(configFile).ConfigureAwait(false);
                    }

                    //
                    // Import part headers from parts/*.dat
                    //
                    string partsDir = Path.Combine(extractedPath, "parts");
                    if (Directory.Exists(partsDir))
                    {
                        await importService.ImportPartHeadersAsync(partsDir).ConfigureAwait(false);
                    }

                    //
                    // Copy data files to configured LDraw data path
                    //
                    string ldrawDataPath = _configuration.GetValue<string>("DataImport:LDraw:DataPath", "");
                    if (string.IsNullOrEmpty(ldrawDataPath) == false)
                    {
                        importService.CopyDataFiles(extractedPath, ldrawDataPath);

                        //
                        // Hot-reload: add the new/updated files into the in-memory cache
                        // so the LDrawController and ModelExportService see them immediately
                        // without requiring a server restart.
                        //
                        _logger.LogInformation("{Prefix}   Refreshing in-memory LDraw file cache...", LOG_PREFIX);
                        _ldrawFileService.IngestDirectory(ldrawDataPath);
                    }
                }

                //
                // Update state and mark step complete
                //
                state.Timestamps[stateKey] = compositeKey;

                if (isBootstrap)
                {
                    state.CompletedSteps.Add(stateKey);
                    await SaveStateAsync(cachePath, state).ConfigureAwait(false);
                }

                _logger.LogInformation("{Prefix} {Label} import complete.", LOG_PREFIX, label);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Prefix} {Label} check/import failed.", LOG_PREFIX, label);
            }
        }


        // ─────────────────────────────────────────────────────────
        //  Helper: download and decompress a .csv.gz file
        // ─────────────────────────────────────────────────────────

        private async Task DownloadAndDecompressGzFileAsync(string url, string outputPath, CancellationToken ct)
        {
            _logger.LogInformation("{Prefix}   Downloading {Url}...", LOG_PREFIX, url);

            using (HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                using (Stream compressedStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                using (FileStream outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920))
                {
                    await gzipStream.CopyToAsync(outStream, 81920, ct).ConfigureAwait(false);
                }
            }

            long fileSize = new FileInfo(outputPath).Length;
            _logger.LogInformation("{Prefix}   Saved {Path} ({Size} bytes)", LOG_PREFIX, Path.GetFileName(outputPath), fileSize);
        }


        // ─────────────────────────────────────────────────────────
        //  State persistence
        // ─────────────────────────────────────────────────────────

        private async Task<ImportState> LoadStateAsync(string cachePath)
        {
            string statePath = Path.Combine(cachePath, STATE_FILENAME);

            if (File.Exists(statePath) == false)
            {
                _logger.LogInformation("{Prefix} No previous state file found — first run.", LOG_PREFIX);
                return new ImportState();
            }

            try
            {
                string json = await File.ReadAllTextAsync(statePath).ConfigureAwait(false);
                ImportState loaded = JsonSerializer.Deserialize<ImportState>(json);
                _logger.LogInformation("{Prefix} Loaded state with {Count} tracked timestamps.", LOG_PREFIX, loaded.Timestamps.Count);
                return loaded;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Prefix} Failed to load state file — starting fresh.", LOG_PREFIX);
                return new ImportState();
            }
        }


        private async Task SaveStateAsync(string cachePath, ImportState state)
        {
            string statePath = Path.Combine(cachePath, STATE_FILENAME);

            try
            {
                state.LastRunUtc = DateTime.UtcNow;

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(state, options);
                await File.WriteAllTextAsync(statePath, json).ConfigureAwait(false);

                _logger.LogInformation("{Prefix} State saved to {Path}", LOG_PREFIX, statePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Prefix} Failed to save state file (non-fatal).", LOG_PREFIX);
            }
        }
    }


    // ─────────────────────────────────────────────────────────
    //  State model
    // ─────────────────────────────────────────────────────────

    //
    // AI-generated: Persisted state for tracking upstream data timestamps.
    // Stored as import-state.json in the cache directory.
    //

    public class ImportState
    {
        public Dictionary<string, string> Timestamps { get; set; } = new Dictionary<string, string>();
        public HashSet<string> CompletedSteps { get; set; } = new HashSet<string>();
        public bool BootstrapComplete { get; set; } = false;
        public DateTime LastRunUtc { get; set; }
    }
}
