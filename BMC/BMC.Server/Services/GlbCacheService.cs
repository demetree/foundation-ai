using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Foundation.BMC.Database;

using BMC.LDraw.Render;


namespace Foundation.BMC.Services
{
    /// <summary>
    ///
    /// Two-tier cache for pre-compiled GLB (binary glTF) viewer data.
    ///
    /// Tier 1: In-memory ConcurrentDictionary — sub-millisecond access.
    /// Tier 2: Database (CompiledGlb table) — survives restarts, millisecond access.
    /// Build:  On full miss, generates the GLB via RenderService.ExportToGlb().
    ///
    /// Cache key: (projectId, includesEdgeLines).
    /// Invalidation: projectVersionNumber — if the project version has advanced,
    /// the cached entry is stale and a fresh build is triggered.
    ///
    /// Concurrency: SemaphoreSlim per projectId prevents duplicate concurrent builds.
    ///
    /// AI-developed — March 2026
    ///
    /// </summary>
    public class GlbCacheService
    {
        //
        // RAM cache — keyed by (projectId, includesEdgeLines)
        //
        private static readonly ConcurrentDictionary<(int projectId, bool edgeLines), GlbCacheEntry> _ramCache
            = new ConcurrentDictionary<(int, bool), GlbCacheEntry>();

        //
        // Build semaphores — one per projectId to prevent duplicate concurrent builds.
        // Uses a ConcurrentDictionary to lazily create semaphores on demand.
        //
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _buildLocks
            = new ConcurrentDictionary<int, SemaphoreSlim>();


        //
        // RAM cache size limit — evict oldest entries when exceeded
        //
        private const int DEFAULT_MAX_RAM_ENTRIES = 50;
        private readonly int _maxRamEntries;


        //
        // Dependencies
        //
        private readonly BMCContext _context;
        private readonly ModelExportService _exportService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GlbCacheService> _logger;


        /// <summary>
        /// Constructor — injected by DI.
        /// </summary>
        public GlbCacheService(
            BMCContext context,
            ModelExportService exportService,
            IConfiguration configuration,
            ILogger<GlbCacheService> logger)
        {
            _context = context;
            _exportService = exportService;
            _configuration = configuration;
            _logger = logger;

            _maxRamEntries = configuration.GetValue("GlbCache:MaxRamEntries", DEFAULT_MAX_RAM_ENTRIES);
        }


        /// <summary>
        /// Get a compiled GLB for the given project.
        ///
        /// Resolution order:
        ///   1. RAM cache (fastest — ConcurrentDictionary lookup)
        ///   2. Database (CompiledGlb table)
        ///   3. Build from scratch (GeometryResolver → GlbExporter)
        ///
        /// Results are stored back into both tiers for future requests.
        /// </summary>
        public async Task<byte[]> GetOrBuildGlbAsync(
            int projectId,
            Guid tenantGuid,
            bool includeEdgeLines,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = (projectId, includeEdgeLines);

            //
            // Get the project's current version number for invalidation checks
            //
            Project project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.id == projectId && p.tenantGuid == tenantGuid && p.active == true && p.deleted == false)
                .Select(p => new Project { id = p.id, versionNumber = p.versionNumber })
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
            {
                return null;
            }

            int currentVersion = project.versionNumber;

            //
            // Tier 1: RAM cache
            //
            if (_ramCache.TryGetValue(cacheKey, out GlbCacheEntry ramEntry)
                && ramEntry.ProjectVersionNumber == currentVersion)
            {
                _logger.LogDebug("GLB cache HIT (RAM) — Project {Id}, version {V}, edgeLines={E}",
                    projectId, currentVersion, includeEdgeLines);

                ramEntry.LastAccessedUtc = DateTime.UtcNow;
                return ramEntry.GlbData;
            }

            //
            // Tier 2: Database
            // Read using raw ADO.NET with SequentialAccess to prevent EF Core from
            // choking on the massive varbinary(max) LOB allocation overhead.
            //
            byte[] dbGlbData = null;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = @"
                    SELECT TOP 1 glbData 
                    FROM BMC.CompiledGlb 
                    WHERE projectId = @projectId 
                      AND tenantGuid = @tenantGuid 
                      AND includesEdgeLines = @edgeLines 
                      AND projectVersionNumber = @version 
                      AND active = 1 
                      AND deleted = 0";
                
                var pId = command.CreateParameter();
                pId.ParameterName = "@projectId";
                pId.Value = projectId;
                command.Parameters.Add(pId);

                var pTenant = command.CreateParameter();
                pTenant.ParameterName = "@tenantGuid";
                pTenant.Value = tenantGuid;
                command.Parameters.Add(pTenant);

                var pEdge = command.CreateParameter();
                pEdge.ParameterName = "@edgeLines";
                pEdge.Value = includeEdgeLines;
                command.Parameters.Add(pEdge);

                var pVer = command.CreateParameter();
                pVer.ParameterName = "@version";
                pVer.Value = currentVersion;
                command.Parameters.Add(pVer);

                await _context.Database.OpenConnectionAsync(cancellationToken);
                using (var reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.SequentialAccess, cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        if (!reader.IsDBNull(0))
                        {
                            using var ms = new System.IO.MemoryStream();
                            using var dbStream = reader.GetStream(0);
                            await dbStream.CopyToAsync(ms, cancellationToken);
                            dbGlbData = ms.ToArray();
                        }
                    }
                }
            }

            if (dbGlbData != null && dbGlbData.Length > 0)
            {
                _logger.LogInformation("GLB cache HIT (DB) — Project {Id}, version {V}, edgeLines={E}, size={S} bytes",
                    projectId, currentVersion, includeEdgeLines, dbGlbData.Length);

                //
                // Promote to RAM cache
                //
                StoreInRamCache(cacheKey, dbGlbData, currentVersion);

                return dbGlbData;
            }
            //
            // Full miss — build the GLB under a per-project semaphore to prevent duplicates
            //
            SemaphoreSlim buildLock = _buildLocks.GetOrAdd(projectId, _ => new SemaphoreSlim(1, 1));

            await buildLock.WaitAsync(cancellationToken);

            try
            {
                //
                // Double-check after acquiring the lock — another thread may have built it
                //
                if (_ramCache.TryGetValue(cacheKey, out GlbCacheEntry rechecked)
                    && rechecked.ProjectVersionNumber == currentVersion)
                {
                    rechecked.LastAccessedUtc = DateTime.UtcNow;
                    return rechecked.GlbData;
                }

                //
                // Build the GLB
                //
                _logger.LogInformation("GLB cache MISS — Building for Project {Id}, version {V}, edgeLines={E}",
                    projectId, currentVersion, includeEdgeLines);

                byte[] glbData = await BuildGlbAsync(projectId, tenantGuid, includeEdgeLines, cancellationToken);

                if (glbData == null || glbData.Length == 0)
                {
                    return null;
                }

                //
                // Store in both tiers
                //
                await StoreInDatabaseAsync(projectId, tenantGuid, currentVersion, includeEdgeLines, glbData, cancellationToken);
                StoreInRamCache(cacheKey, glbData, currentVersion);

                _logger.LogInformation(
                    "GLB built and cached — Project {Id}, version {V}, edgeLines={E}, size={Size} bytes",
                    projectId, currentVersion, includeEdgeLines, glbData.Length);

                return glbData;
            }
            finally
            {
                buildLock.Release();
            }
        }


        /// <summary>
        /// Build a GLB from scratch — generates the MPD, runs GeometryResolver + GlbExporter.
        /// </summary>
        private async Task<byte[]> BuildGlbAsync(
            int projectId,
            Guid tenantGuid,
            bool includeEdgeLines,
            CancellationToken cancellationToken)
        {
            //
            // Generate the full MPD from project data
            //
            string mpdContent = await _exportService.GenerateViewerMpdAsync(projectId, tenantGuid, cancellationToken);

            if (string.IsNullOrWhiteSpace(mpdContent))
            {
                return null;
            }

            //
            // Run the GLB export on a background thread (CPU-intensive geometry work)
            //
            string[] mpdLines = mpdContent.Split('\n');

            byte[] glbData = await Task.Run(() =>
            {
                string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                RenderService renderService = new RenderService(dataPath);
                return renderService.ExportToGlb(mpdLines, "project.mpd", colourCode: -1, includeEdgeLines: includeEdgeLines);
            }, cancellationToken);

            return glbData;
        }


        /// <summary>
        /// Store the compiled GLB in the database for persistence across restarts.
        /// Replaces any existing entry for the same project + edge lines combination.
        /// </summary>
        private async Task StoreInDatabaseAsync(
            int projectId,
            Guid tenantGuid,
            int versionNumber,
            bool includeEdgeLines,
            byte[] glbData,
            CancellationToken cancellationToken)
        {
            try
            {
                //
                // Remove any stale entries for this project + edge lines combination
                //
                var staleEntries = await _context.CompiledGlbs
                    .Where(g => g.projectId == projectId
                        && g.tenantGuid == tenantGuid
                        && g.includesEdgeLines == includeEdgeLines)
                    .ToListAsync(cancellationToken);

                if (staleEntries.Count > 0)
                {
                    _context.CompiledGlbs.RemoveRange(staleEntries);
                }

                //
                // Insert the fresh compiled GLB
                //
                CompiledGlb newEntry = new CompiledGlb
                {
                    tenantGuid = tenantGuid,
                    projectId = projectId,
                    projectVersionNumber = versionNumber,
                    includesEdgeLines = includeEdgeLines,
                    glbData = glbData,
                    glbSizeBytes = glbData.Length,
                    compiledAt = DateTime.UtcNow,
                    active = true,
                    deleted = false,
                    objectGuid = Guid.NewGuid()
                };

                _context.CompiledGlbs.Add(newEntry);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                //
                // Database persistence is best-effort — the GLB is already in RAM.
                // Log the error but don't fail the request.
                //
                _logger.LogWarning(ex, "Failed to persist compiled GLB to database for project {Id}", projectId);
            }
        }


        /// <summary>
        /// Store a GLB in the RAM cache. Evicts oldest entries if the cache is full.
        /// </summary>
        private void StoreInRamCache((int projectId, bool edgeLines) key, byte[] glbData, int versionNumber)
        {
            GlbCacheEntry entry = new GlbCacheEntry
            {
                GlbData = glbData,
                ProjectVersionNumber = versionNumber,
                LastAccessedUtc = DateTime.UtcNow
            };

            _ramCache[key] = entry;

            //
            // Evict oldest entries if over the limit
            //
            if (_ramCache.Count > _maxRamEntries)
            {
                EvictOldestRamEntries();
            }
        }


        /// <summary>
        /// Simple LRU eviction — remove the least-recently-accessed entries
        /// until we're at 80% capacity.
        /// </summary>
        private void EvictOldestRamEntries()
        {
            int targetCount = (int)(_maxRamEntries * 0.8);

            var sorted = _ramCache
                .OrderBy(kvp => kvp.Value.LastAccessedUtc)
                .ToList();

            for (int i = 0; i < sorted.Count && _ramCache.Count > targetCount; i++)
            {
                _ramCache.TryRemove(sorted[i].Key, out _);
            }
        }


        /// <summary>
        /// Invalidate cached GLBs for a project (call when project is modified).
        /// Removes from RAM cache only — DB entries are invalidated lazily via version mismatch.
        /// </summary>
        public void InvalidateProject(int projectId)
        {
            var keysToRemove = _ramCache.Keys.Where(k => k.projectId == projectId).ToList();

            foreach (var key in keysToRemove)
            {
                _ramCache.TryRemove(key, out _);
            }
        }


        /// <summary>
        /// RAM cache entry — holds the GLB bytes, version number, and LRU timestamp.
        /// </summary>
        private class GlbCacheEntry
        {
            public byte[] GlbData { get; set; }
            public int ProjectVersionNumber { get; set; }
            public DateTime LastAccessedUtc { get; set; }
        }
    }
}
