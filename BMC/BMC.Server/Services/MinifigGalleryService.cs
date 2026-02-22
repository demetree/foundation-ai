using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.BMC.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Foundation.BMC.Services
{
    /// <summary>
    ///
    /// Background service that precomputes a lean list of all LEGO minifigs at
    /// server startup and holds it in memory for instant serving to the
    /// Minifig Gallery UI.
    ///
    /// Year is derived from the most recent set each minifig appears in
    /// (MAX of LegoSet.year via the LegoSetMinifig junction table).
    /// Minifigs not linked to any set receive Year = 0 and sort to the end.
    ///
    /// Sorted newest-first (year desc, name asc) so the client can display
    /// the most relevant minifigs immediately.
    ///
    /// Follows the same pattern as <see cref="SetExplorerService"/>.
    ///
    /// </summary>
    public class MinifigGalleryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MinifigGalleryService> _logger;

        private List<MinifigGalleryItemDTO> _cachedMinifigs;
        private readonly object _cacheLock = new object();


        public MinifigGalleryService(
            IServiceScopeFactory scopeFactory,
            ILogger<MinifigGalleryService> logger
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        /// <summary>
        ///
        /// Returns the cached minifig list, or null if computation has not yet completed.
        ///
        /// </summary>
        public List<MinifigGalleryItemDTO> GetCachedMinifigs()
        {
            lock (_cacheLock)
            {
                return _cachedMinifigs;
            }
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //
            // Delay briefly to let the rest of the server finish starting
            //
            await Task.Delay(4000, stoppingToken).ConfigureAwait(false);

            _logger.LogInformation("[MinifigGalleryService] Starting minifig list computation...");

            try
            {
                await ComputeAndCacheAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[MinifigGalleryService] Computation cancelled during shutdown.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MinifigGalleryService] Failed to compute Minifig Gallery data.");
            }
        }


        /// <summary>
        ///
        /// Forces a re-computation from the database and updates the in-memory cache.
        ///
        /// </summary>
        public async Task RefreshAsync()
        {
            _logger.LogInformation("[MinifigGalleryService] Manual refresh triggered.");

            await ComputeAndCacheAsync(CancellationToken.None).ConfigureAwait(false);
        }


        private async Task ComputeAndCacheAsync(CancellationToken ct)
        {
            Stopwatch sw = Stopwatch.StartNew();

            using IServiceScope scope = _scopeFactory.CreateScope();

            BMCContext context = scope.ServiceProvider.GetRequiredService<BMCContext>();

            context.Database.SetCommandTimeout(60);

            //
            // Step 1: Load all active, non-deleted minifigs as lean DTOs.
            //
            List<MinifigGalleryItemDTO> minifigs = await context.LegoMinifigs
                .AsNoTracking()
                .Where(mf => mf.active && !mf.deleted)
                .Select(mf => new MinifigGalleryItemDTO
                {
                    Id = mf.id,
                    Name = mf.name,
                    FigNumber = mf.figNumber,
                    PartCount = mf.partCount,
                    ImageUrl = mf.imageUrl,
                    Year = 0
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            //
            // Step 2: Build a lookup of minifigId → max year from the junction table.
            // This is a separate query to avoid the correlated subquery that
            // EF Core cannot translate inside a Select projection.
            //
            Dictionary<int, int> yearLookup = await context.LegoSetMinifigs
                .AsNoTracking()
                .Where(lsm => lsm.active && !lsm.deleted)
                .Join(
                    context.LegoSets.AsNoTracking().Where(ls => ls.active && !ls.deleted),
                    lsm => lsm.legoSetId,
                    ls => ls.id,
                    (lsm, ls) => new { lsm.legoMinifigId, ls.year }
                )
                .GroupBy(x => x.legoMinifigId)
                .Select(g => new { MinifigId = g.Key, MaxYear = g.Max(x => x.year) })
                .ToDictionaryAsync(x => x.MinifigId, x => x.MaxYear, ct)
                .ConfigureAwait(false);

            //
            // Step 3: Merge the year into each DTO and sort.
            //
            foreach (var mf in minifigs)
            {
                if (yearLookup.TryGetValue(mf.Id, out int year))
                {
                    mf.Year = year;
                }
            }

            minifigs = minifigs
                .OrderByDescending(mf => mf.Year)
                .ThenBy(mf => mf.Name)
                .ToList();

            sw.Stop();

            lock (_cacheLock)
            {
                _cachedMinifigs = minifigs;
            }

            _logger.LogInformation(
                "[MinifigGalleryService] Loaded {Count} minifigs in {Elapsed}ms",
                minifigs.Count,
                sw.ElapsedMilliseconds
            );
        }
    }


    /// <summary>
    /// Lean DTO containing only the fields required by the Minifig Gallery card grid.
    /// Year is derived from the most recent set each minifig appears in.
    /// </summary>
    public class MinifigGalleryItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FigNumber { get; set; }
        public int PartCount { get; set; }
        public string ImageUrl { get; set; }
        public int Year { get; set; }
    }
}
