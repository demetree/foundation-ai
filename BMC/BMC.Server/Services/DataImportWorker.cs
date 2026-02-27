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


        public DataImportWorker(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<DataImportWorker> logger
        )
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(15);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BMC-Worker/1.0");
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //
            // Delay briefly to let the host finish starting up
            //
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);

            int intervalMinutes = _configuration.GetValue<int>("DataImport:IntervalMinutes", 60);

            _logger.LogInformation(
                "{Prefix} Starting — will check for updates every {Interval} minutes",
                LOG_PREFIX, intervalMinutes
            );

            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    await RunImportCycleAsync(stoppingToken).ConfigureAwait(false);
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

                //
                // Wait for the configured interval before the next check
                //
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken).ConfigureAwait(false);
            }
        }


        // ─────────────────────────────────────────────────────────
        //  Main import cycle
        // ─────────────────────────────────────────────────────────

        private async Task RunImportCycleAsync(CancellationToken ct)
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
                await CheckAndImportRebrickableAsync(cachePath, state, ct).ConfigureAwait(false);
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
                    ct: ct
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
                    ct: ct
                ).ConfigureAwait(false);
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

        private async Task CheckAndImportRebrickableAsync(string cachePath, ImportState state, CancellationToken ct)
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

            if (changedFiles.Count == 0)
            {
                _logger.LogInformation("{Prefix} Rebrickable: no changes detected.", LOG_PREFIX);
                return;
            }

            _logger.LogInformation(
                "{Prefix} Rebrickable: {Count} files changed — downloading and importing...",
                LOG_PREFIX, changedFiles.Count
            );

            //
            // Download changed files
            //
            foreach (string fileName in changedFiles)
            {
                ct.ThrowIfCancellationRequested();
                await DownloadAndDecompressGzFileAsync(
                    url: REBRICKABLE_CDN_BASE + fileName,
                    outputPath: Path.Combine(rebrickableCachePath, fileName.Replace(".gz", "")),
                    ct: ct
                ).ConfigureAwait(false);
            }

            //
            // Run import in FK-dependency order using a scoped context
            //
            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                BMCContext context = scope.ServiceProvider.GetRequiredService<BMCContext>();
                context.Database.SetCommandTimeout(300);

                Action<string> log = (string msg) => _logger.LogInformation("{Prefix} {Message}", LOG_PREFIX, msg);
                RebrickableImportService importService = new RebrickableImportService(context, log);

                string csvDir = rebrickableCachePath;

                //
                // Import in FK-dependency order
                // Only import targets whose CSV files were downloaded (or already cached)
                //
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

                if (File.Exists(themesFile))
                {
                    await importService.ImportThemesAsync(themesFile).ConfigureAwait(false);
                }

                if (File.Exists(coloursFile))
                {
                    await importService.ImportColoursAsync(coloursFile).ConfigureAwait(false);
                }

                if (File.Exists(categoriesFile))
                {
                    await importService.ImportPartCategoriesAsync(categoriesFile).ConfigureAwait(false);
                }

                if (File.Exists(partsFile))
                {
                    await importService.ImportPartsAsync(partsFile).ConfigureAwait(false);
                }

                if (File.Exists(relationshipsFile))
                {
                    await importService.ImportPartRelationshipsAsync(relationshipsFile).ConfigureAwait(false);
                }

                if (File.Exists(elementsFile))
                {
                    await importService.ImportElementsAsync(elementsFile).ConfigureAwait(false);
                }

                if (File.Exists(setsFile))
                {
                    await importService.ImportSetsAsync(setsFile).ConfigureAwait(false);
                }

                if (File.Exists(minifigsFile))
                {
                    await importService.ImportMinifigsAsync(minifigsFile).ConfigureAwait(false);
                }

                if (File.Exists(inventoriesFile) && File.Exists(inventoryPartsFile))
                {
                    await importService.ImportInventoryPartsAsync(inventoriesFile, inventoryPartsFile).ConfigureAwait(false);
                }

                if (File.Exists(inventoriesFile) && File.Exists(inventorySetsFile))
                {
                    await importService.ImportInventorySetsAsync(inventoriesFile, inventorySetsFile).ConfigureAwait(false);
                }

                if (File.Exists(inventoriesFile) && File.Exists(inventoryMinifigsFile))
                {
                    await importService.ImportInventoryMinifigsAsync(inventoriesFile, inventoryMinifigsFile).ConfigureAwait(false);
                }

                //
                // Post-import reconciliation
                //
                await importService.ReconcileMovedPartsAsync().ConfigureAwait(false);
            }

            _logger.LogInformation("{Prefix} Rebrickable import complete.", LOG_PREFIX);
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
            CancellationToken ct
        )
        {
            _logger.LogInformation("{Prefix} Checking {Label}...", LOG_PREFIX, label);

            try
            {
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
                    _logger.LogInformation("{Prefix}   {Label}: unchanged.", LOG_PREFIX, label);
                    return;
                }

                _logger.LogInformation(
                    "{Prefix}   {Label}: CHANGED (was: {Old}, now: {New})",
                    LOG_PREFIX, label, previousValue ?? "(first run)", compositeKey
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
                    }
                }

                //
                // Update state
                //
                state.Timestamps[stateKey] = compositeKey;

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
        public DateTime LastRunUtc { get; set; }
    }
}
