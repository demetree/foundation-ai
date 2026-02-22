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
    /// Background service that precomputes a lean list of all LEGO sets at server
    /// startup and holds it in memory for instant serving to the Set Explorer UI.
    ///
    /// The payload is lightweight (~150 bytes per set × ~20K sets ≈ 3 MB) and
    /// sorted newest-first (year desc, part-count desc) so the client can
    /// display the most relevant sets immediately without any additional sorting.
    ///
    /// Follows the same pattern as <see cref="PartsUniverseService"/>.
    ///
    /// </summary>
    public class SetExplorerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SetExplorerService> _logger;

        private List<SetExplorerItemDTO> _cachedSets;
        private readonly object _cacheLock = new object();


        public SetExplorerService(
            IServiceScopeFactory scopeFactory,
            ILogger<SetExplorerService> logger
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        /// <summary>
        ///
        /// Returns the cached set list, or null if computation has not yet completed.
        ///
        /// </summary>
        public List<SetExplorerItemDTO> GetCachedSets()
        {
            lock (_cacheLock)
            {
                return _cachedSets;
            }
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //
            // Delay briefly to let the rest of the server finish starting
            //
            await Task.Delay(3000, stoppingToken).ConfigureAwait(false);

            _logger.LogInformation("[SetExplorerService] Starting set list computation...");

            try
            {
                await ComputeAndCacheAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[SetExplorerService] Computation cancelled during shutdown.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SetExplorerService] Failed to compute Set Explorer data.");
            }
        }


        /// <summary>
        ///
        /// Forces a re-computation from the database and updates the in-memory cache.
        ///
        /// </summary>
        public async Task RefreshAsync()
        {
            _logger.LogInformation("[SetExplorerService] Manual refresh triggered.");

            await ComputeAndCacheAsync(CancellationToken.None).ConfigureAwait(false);
        }


        private async Task ComputeAndCacheAsync(CancellationToken ct)
        {
            Stopwatch sw = Stopwatch.StartNew();

            using IServiceScope scope = _scopeFactory.CreateScope();

            BMCContext context = scope.ServiceProvider.GetRequiredService<BMCContext>();

            context.Database.SetCommandTimeout(60);

            //
            // Load all active, non-deleted sets with their theme (for theme name).
            // Project directly to the lean DTO to minimize memory usage.
            //
            List<SetExplorerItemDTO> sets = await context.LegoSets
                .AsNoTracking()
                .Where(ls => ls.active && !ls.deleted)
                .Include(ls => ls.legoTheme)
                .OrderByDescending(ls => ls.year)
                .ThenByDescending(ls => ls.partCount)
                .ThenBy(ls => ls.name)
                .Select(ls => new SetExplorerItemDTO
                {
                    Id = ls.id,
                    Name = ls.name,
                    SetNumber = ls.setNumber,
                    Year = ls.year,
                    PartCount = ls.partCount,
                    ImageUrl = ls.imageUrl,
                    ThemeId = ls.legoThemeId,
                    ThemeName = ls.legoTheme != null ? ls.legoTheme.name : null
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            sw.Stop();

            lock (_cacheLock)
            {
                _cachedSets = sets;
            }

            _logger.LogInformation(
                "[SetExplorerService] Loaded {Count} sets in {Elapsed}ms",
                sets.Count,
                sw.ElapsedMilliseconds
            );
        }
    }


    /// <summary>
    /// Lean DTO containing only the fields required by the Set Explorer card grid.
    /// Excludes URLs (bricklink, rebrickable), guids, and audit flags to minimise payload size.
    /// </summary>
    public class SetExplorerItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SetNumber { get; set; }
        public int Year { get; set; }
        public int PartCount { get; set; }
        public string ImageUrl { get; set; }
        public int? ThemeId { get; set; }
        public string ThemeName { get; set; }
    }
}
