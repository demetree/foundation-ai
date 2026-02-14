using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Foundation.Controllers;
using Foundation.Security;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Serves raw LDraw geometry files (.dat, .ldr) for 3D rendering in the Parts Catalog.
    /// Files are read from the configured LDraw data directory (appsettings.json → LDraw:DataPath).
    ///
    /// Two route patterns are supported:
    ///   1.  GET /api/ldraw/file?path=parts/3001.dat         (query-param, authenticated)
    ///   2.  GET /api/ldraw/file/parts/3001.dat               (path-based,  authenticated — used by Three.js LDrawLoader)
    ///
    /// The path-based route includes smart file resolution: when the exact path isn't found,
    /// the server searches standard LDraw subdirectories (parts/, p/, p/48/, p/8/, parts/s/, models/).
    /// This eliminates the Three.js LDrawLoader's trial-and-error approach, which would otherwise
    /// generate up to 8 HTTP requests per sub-file reference.
    ///
    /// File I/O is guarded by a semaphore to prevent concurrent disk reads under heavy load.
    /// Results are cached in memory so that subsequent requests for the same file are served instantly.
    /// </summary>
    public class LDrawController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        //
        // Static in-memory cache — keyed by normalised relative path, value is file content.
        // Survives across requests for the lifetime of the application.
        //
        private static readonly ConcurrentDictionary<string, string> _fileCache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //
        // Filename-to-path index — maps each filename (case-insensitive) to its relative path
        // within the LDraw data directory.  Built lazily on first request and cached for the
        // lifetime of the application.  Enables O(1) file resolution without disk scanning.
        //
        private static ConcurrentDictionary<string, string> _fileIndex;
        private static readonly object _indexLock = new object();

        //
        // Semaphore to guard concurrent file I/O — allows up to 8 parallel reads.
        //
        private static readonly SemaphoreSlim _ioSemaphore = new SemaphoreSlim(8, 8);

        private readonly IConfiguration _configuration;
        private readonly ILogger<LDrawController> _logger;


        public LDrawController(IConfiguration configuration, ILogger<LDrawController> logger) : base("BMC", "LDraw")
        {
            _configuration = configuration;
            _logger = logger;
        }


        /// <summary>
        /// Returns the raw LDraw file content via query parameter (authenticated).
        /// Example: GET /api/ldraw/file?path=parts/3001.dat
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.OneHundredPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/ldraw/file")]
        public async Task<IActionResult> GetLDrawFile(string path, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            return await ServeFile(path, cancellationToken);
        }


        /// <summary>
        /// Returns the raw LDraw file content via path segments.
        /// Used by the Three.js LDrawLoader which resolves sub-files by trial and error,
        /// generating many concurrent HTTP requests per part.
        ///
        /// Includes smart file resolution: when the exact path isn't found, the server
        /// searches registered LDraw paths using a cached filename-to-path index.
        ///
        /// Example: GET /api/ldraw/file/parts/3001.dat
        /// Example: GET /api/ldraw/file/p/48/4-4cyli.dat
        /// Example: GET /api/ldraw/file/LDConfig.ldr
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.OneHundredPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/ldraw/file/{**filePath}")]
        public async Task<IActionResult> GetLDrawFileByPath(string filePath, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            return await ServeFile(filePath, cancellationToken);
        }


        /// <summary>
        /// Shared file-serving logic.
        /// Validates the path, checks the in-memory cache, resolves the file via smart lookup,
        /// and reads from disk (guarded by semaphore) if needed.
        /// </summary>
        private async Task<IActionResult> ServeFile(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path parameter is required.");
            }

            //
            // Path traversal protection — reject any path containing '..' or absolute path characters
            //
            if (path.Contains("..") || Path.IsPathRooted(path))
            {
                _logger.LogWarning("Rejected LDraw file request with suspicious path: {Path}", path);
                return BadRequest("Invalid path.");
            }

            string dataPath = _configuration.GetValue<string>("LDraw:DataPath");

            if (string.IsNullOrEmpty(dataPath))
            {
                _logger.LogError("LDraw:DataPath is not configured in appsettings.json.");
                return StatusCode(500, "LDraw data path is not configured.");
            }

            //
            // Normalise separators for consistent cache keys and path resolution
            //
            string normalisedPath = path.Replace('\\', '/');

            //
            // Check the in-memory content cache first — if we have it, return immediately without any disk I/O
            //
            if (_fileCache.TryGetValue(normalisedPath, out string cachedContent))
            {
                return Content(cachedContent, "text/plain");
            }

            //
            // Resolve the full file path — first try exact match, then smart resolution
            //
            string fullPath = ResolveFilePath(dataPath, normalisedPath);

            if (fullPath == null)
            {
                return NotFound($"LDraw file not found: {normalisedPath}");
            }

            //
            // Safety check: ensure the resolved path stays within the data directory
            //
            string normalisedDataPath = Path.GetFullPath(dataPath);

            if (fullPath.StartsWith(normalisedDataPath, StringComparison.OrdinalIgnoreCase) == false)
            {
                _logger.LogWarning("Rejected LDraw file request that resolved outside data directory: {FullPath}", fullPath);
                return BadRequest("Invalid path.");
            }

            //
            // Read from disk, guarded by the semaphore to limit concurrent I/O
            //
            try
            {
                await _ioSemaphore.WaitAsync(cancellationToken);

                try
                {
                    //
                    // Double-check content cache after acquiring the semaphore
                    //
                    if (_fileCache.TryGetValue(normalisedPath, out string justCachedContent))
                    {
                        return Content(justCachedContent, "text/plain");
                    }

                    string content = await System.IO.File.ReadAllTextAsync(fullPath, cancellationToken);

                    //
                    // Store in memory cache for subsequent requests
                    //
                    _fileCache.TryAdd(normalisedPath, content);

                    return Content(content, "text/plain");
                }
                finally
                {
                    _ioSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading LDraw file: {Path}", fullPath);
                return StatusCode(500, "Error reading file.");
            }
        }


        /// <summary>
        /// Resolves a requested path to an actual file on disk.
        ///
        /// Resolution strategy:
        ///   1. Try the exact path first (e.g. "parts/3001.dat" → "{dataPath}/parts/3001.dat")
        ///   2. If not found, use the cached filename-to-path index to find the file
        ///      by its filename alone, regardless of which subdirectory it's in.
        ///   3. Also try the path suffix (everything after the first directory component)
        ///      in case the LDrawLoader prepended the wrong directory prefix.
        ///
        /// This eliminates the LDrawLoader's trial-and-error approach which would otherwise
        /// send 4-8 sequential HTTP requests per sub-file (trying parts/, p/, models/, etc.).
        /// </summary>
        private string ResolveFilePath(string dataPath, string normalisedPath)
        {
            //
            // 1. Try exact path first — this is the fast path for correct requests
            //
            string exactPath = Path.GetFullPath(Path.Combine(dataPath, normalisedPath));

            if (System.IO.File.Exists(exactPath))
            {
                return exactPath;
            }

            //
            // 2. Build or retrieve the filename index
            //
            EnsureFileIndexBuilt(dataPath);

            //
            // 3. Try looking up the path suffix (strip the first directory component).
            //    e.g. "parts/48/4-4cyli.dat" → "48/4-4cyli.dat"
            //    This handles cases where the LDrawLoader prepends "parts/" but the file is in "p/"
            //
            int firstSlash = normalisedPath.IndexOf('/');

            if (firstSlash >= 0)
            {
                string pathSuffix = normalisedPath.Substring(firstSlash + 1);

                // Try each standard LDraw directory with this suffix
                string[] searchDirs = { "p", "parts", "parts/s", "p/48", "p/8", "models" };

                foreach (string dir in searchDirs)
                {
                    string candidate = Path.GetFullPath(Path.Combine(dataPath, dir, pathSuffix));

                    if (System.IO.File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }

            //
            // 4. Fall back to filename-only lookup in the index
            //
            string fileName = Path.GetFileName(normalisedPath).ToLowerInvariant();

            if (_fileIndex != null && _fileIndex.TryGetValue(fileName, out string indexedRelativePath))
            {
                string indexedFullPath = Path.GetFullPath(Path.Combine(dataPath, indexedRelativePath));

                if (System.IO.File.Exists(indexedFullPath))
                {
                    return indexedFullPath;
                }
            }

            return null;
        }


        /// <summary>
        /// Lazily builds the filename-to-path index by scanning the LDraw data directory.
        /// The index maps lowercase filenames to their relative paths.
        /// Built once and cached for the lifetime of the application.
        /// </summary>
        private void EnsureFileIndexBuilt(string dataPath)
        {
            if (_fileIndex != null)
            {
                return;
            }

            lock (_indexLock)
            {
                if (_fileIndex != null)
                {
                    return;
                }

                _logger.LogInformation("Building LDraw file index for: {DataPath}", dataPath);

                var index = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    //
                    // Only index LDraw file types
                    //
                    string[] extensions = { "*.dat", "*.ldr", "*.mpd" };

                    foreach (string pattern in extensions)
                    {
                        foreach (string file in Directory.EnumerateFiles(dataPath, pattern, SearchOption.AllDirectories))
                        {
                            string relativePath = Path.GetRelativePath(dataPath, file).Replace('\\', '/');
                            string lowerFileName = Path.GetFileName(file).ToLowerInvariant();

                            //
                            // Store the first occurrence of each filename — priority goes to
                            // files found first (which is typically alphabetical by directory).
                            // If a filename exists in multiple directories, the first match wins.
                            //
                            index.TryAdd(lowerFileName, relativePath);
                        }
                    }

                    _logger.LogInformation("LDraw file index built: {Count} files indexed.", index.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error building LDraw file index.");
                }

                _fileIndex = index;
            }
        }
    }
}
