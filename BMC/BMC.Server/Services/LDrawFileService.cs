using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Foundation.BMC.Services
{
    /// <summary>
    /// Shared in-memory cache of all LDraw geometry files (.dat, .ldr).
    ///
    /// At application startup, the entire LDraw parts library is loaded into
    /// memory so that file lookups are O(1) dictionary hits with zero disk I/O.
    ///
    /// This eliminates:
    ///   - Disk I/O bottlenecks during 3D model rendering
    ///   - Semaphore contention from concurrent file reads
    ///   - 30+ second delays for 404s caused by directory scanning
    ///
    /// Typical memory footprint: 200-400 MB for a complete LDraw library.
    ///
    /// Usage:
    ///   - Register as a singleton via DI
    ///   - Inject into LDrawController and ModelExportService
    ///   - Call TryGetFile(path) for O(1) content lookups
    /// </summary>
    public class LDrawFileService : IHostedService
    {
        //
        // File content cache — keyed by lowercase filename, value is raw text.
        // Using ConcurrentDictionary for thread-safe reads.
        //
        private readonly ConcurrentDictionary<string, string> _filesByName
            = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //
        // Full path cache — keyed by lowercase relative path (e.g. "parts/3001.dat")
        //
        private readonly ConcurrentDictionary<string, string> _filesByPath
            = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly IConfiguration _configuration;
        private readonly ILogger<LDrawFileService> _logger;

        private bool _isLoaded;


        public LDrawFileService(IConfiguration configuration, ILogger<LDrawFileService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


        /// <summary>
        /// Returns true once the library is fully loaded into memory.
        /// </summary>
        public bool IsLoaded => _isLoaded;

        /// <summary>
        /// Number of files in the cache.
        /// </summary>
        public int FileCount => _filesByPath.Count;


        /// <summary>
        /// Starts the background preload of the entire LDraw parts library.
        /// Called automatically by the hosting framework at app startup.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            string dataPath = _configuration.GetValue<string>("LDraw:DataPath");

            if (string.IsNullOrEmpty(dataPath) || Directory.Exists(dataPath) == false)
            {
                _logger.LogWarning("LDraw:DataPath not configured or missing — LDraw file cache will be empty.");
                _isLoaded = true;
                return Task.CompletedTask;
            }

            //
            // Run the preload on a background thread so it doesn't block startup
            //
            _ = Task.Run(() => PreloadLibrary(dataPath, cancellationToken), cancellationToken);

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


        /// <summary>
        /// Looks up an LDraw file by any path or filename.
        ///
        /// Resolution order:
        ///   1. Exact relative path match (e.g. "parts/3001.dat")
        ///   2. Bare filename match (e.g. "3001.dat")
        ///   3. Path suffix match (strips first directory component)
        ///   4. Standard directory prefixes with bare filename
        ///
        /// Returns null if the file is not in the library.
        /// </summary>
        public string TryGetFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            string normalised = path.Replace('\\', '/').Trim();

            //
            // 1. Exact relative path
            //
            if (_filesByPath.TryGetValue(normalised, out string content))
            {
                return content;
            }

            //
            // 2. Bare filename (last component)
            //
            string bareFileName = Path.GetFileName(normalised);

            if (_filesByName.TryGetValue(bareFileName, out content))
            {
                return content;
            }

            //
            // 3. Strip first directory and try as relative path
            //    e.g. "parts/s/3001s01.dat" → try "s/3001s01.dat" in each standard dir
            //
            int firstSlash = normalised.IndexOf('/');

            if (firstSlash >= 0)
            {
                string suffix = normalised.Substring(firstSlash + 1);

                if (_filesByPath.TryGetValue(suffix, out content))
                {
                    return content;
                }

                string[] dirs = { "parts", "p", "parts/s", "p/48", "p/8", "models" };

                foreach (string dir in dirs)
                {
                    if (_filesByPath.TryGetValue(dir + "/" + suffix, out content))
                    {
                        return content;
                    }
                }
            }

            //
            // 4. Try each standard directory + bare filename
            //
            {
                string[] dirs = { "parts", "p", "parts/s", "p/48", "p/8", "models" };

                foreach (string dir in dirs)
                {
                    if (_filesByPath.TryGetValue(dir + "/" + bareFileName, out content))
                    {
                        return content;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Scans a directory tree for .dat and .ldr files and adds (or updates)
        /// them in the in-memory cache.  Called by DataImportWorker after new
        /// LDraw data has been written to disk.
        ///
        /// Files are keyed the same way as the initial preload:
        ///   - _filesByPath: relative path from <paramref name="basePath"/> (e.g. "parts/3001.dat")
        ///   - _filesByName: bare filename, first-occurrence wins for new keys,
        ///     but existing keys ARE updated so changes to known files take effect.
        ///
        /// Thread-safe (uses ConcurrentDictionary).
        /// </summary>
        /// <returns>Number of files added or updated.</returns>
        public int IngestDirectory(string basePath)
        {
            if (string.IsNullOrEmpty(basePath) || Directory.Exists(basePath) == false)
            {
                _logger.LogWarning("IngestDirectory: path does not exist — {Path}", basePath);
                return 0;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int added = 0;
            int updated = 0;

            string[] extensions = { "*.dat", "*.ldr" };

            foreach (string pattern in extensions)
            {
                foreach (string file in Directory.EnumerateFiles(basePath, pattern, SearchOption.AllDirectories))
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        string relativePath = Path.GetRelativePath(basePath, file).Replace('\\', '/');
                        string bareFileName = Path.GetFileName(file).ToLowerInvariant();

                        bool isNew = _filesByPath.ContainsKey(relativePath) == false;

                        //
                        // AddOrUpdate: always overwrite — we want the latest content
                        //
                        _filesByPath[relativePath] = content;
                        _filesByName[bareFileName] = content;

                        if (isNew)
                        {
                            added++;
                        }
                        else
                        {
                            updated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to ingest LDraw file: {File}", file);
                    }
                }
            }

            stopwatch.Stop();

            _logger.LogInformation(
                "LDraw cache hot-reload: {Added} added, {Updated} updated from {Path} in {Elapsed:N1}s (total cache: {Total:N0})",
                added, updated, basePath, stopwatch.Elapsed.TotalSeconds, _filesByPath.Count);

            return added + updated;
        }


        /// <summary>
        /// Loads all .dat and .ldr files from the LDraw data directory into memory.
        /// </summary>
        private void PreloadLibrary(string dataPath, CancellationToken cancellationToken)
        {
            _logger.LogInformation("LDraw file preload starting from: {DataPath}", dataPath);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int count = 0;
            long totalBytes = 0;

            try
            {
                string[] extensions = { "*.dat", "*.ldr" };

                foreach (string pattern in extensions)
                {
                    foreach (string file in Directory.EnumerateFiles(dataPath, pattern, SearchOption.AllDirectories))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger.LogWarning("LDraw file preload cancelled.");
                            break;
                        }

                        try
                        {
                            string content = File.ReadAllText(file);
                            string relativePath = Path.GetRelativePath(dataPath, file).Replace('\\', '/');
                            string lowerFileName = Path.GetFileName(file).ToLowerInvariant();

                            //
                            // Store by relative path (e.g. "parts/3001.dat")
                            //
                            _filesByPath.TryAdd(relativePath, content);

                            //
                            // Store by bare filename (first occurrence wins)
                            //
                            _filesByName.TryAdd(lowerFileName, content);

                            count++;
                            totalBytes += content.Length * 2;  // approximate UTF-16 in-memory size
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Failed to preload LDraw file: {File}", file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during LDraw file preload.");
            }

            stopwatch.Stop();
            _isLoaded = true;

            _logger.LogInformation(
                "LDraw file preload complete: {Count:N0} files, {SizeMB:N1} MB in {Elapsed:N1}s",
                count,
                totalBytes / (1024.0 * 1024.0),
                stopwatch.Elapsed.TotalSeconds);
        }
    }
}
