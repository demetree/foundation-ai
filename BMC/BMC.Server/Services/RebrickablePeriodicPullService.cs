using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BMC.Rebrickable.Sync;
using Foundation.BMC.Database;

namespace Foundation.BMC.Services
{
    /// <summary>
    /// Background service that periodically pulls Rebrickable collections for users
    /// with sync enabled and a configured pull interval.
    ///
    /// Checks every 60 seconds for RebrickableUserLink records where:
    ///   - syncEnabled == true
    ///   - syncDirectionFlags is "RealTime" or "ImportOnly"
    ///   - pullIntervalMinutes is set
    ///   - lastPullDate + pullIntervalMinutes < now
    ///
    /// Uses IServiceScopeFactory to create scoped instances of RebrickableSyncService
    /// since BackgroundService runs as a singleton.
    /// </summary>
    public class RebrickablePeriodicPullService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RebrickablePeriodicPullService> _logger;

        private static readonly Random _jitter = new();
        private const int CheckIntervalSeconds = 60;


        public RebrickablePeriodicPullService(
            IServiceScopeFactory scopeFactory,
            ILogger<RebrickablePeriodicPullService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Rebrickable periodic pull service started.");

            // Initial delay to let the app fully start
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndPullAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Rebrickable periodic pull loop");
                }

                await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Rebrickable periodic pull service stopped.");
        }


        private async Task CheckAndPullAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BMCContext>();

            var now = DateTime.UtcNow;

            // Find all links due for a pull
            var dueLinks = await db.RebrickableUserLinks
                .AsNoTracking()
                .Where(link =>
                    link.active == true
                    && link.deleted == false
                    && link.syncEnabled == true
                    && link.pullIntervalMinutes.HasValue
                    && link.pullIntervalMinutes.Value > 0
                    && (link.syncDirectionFlags == RebrickableSyncService.MODE_REALTIME
                        || link.syncDirectionFlags == RebrickableSyncService.MODE_IMPORT_ONLY)
                )
                .ToListAsync(ct);

            // Filter in-memory for lastPullDate check (DateTime arithmetic in EF can be tricky)
            var readyToPull = dueLinks.Where(link =>
            {
                if (!link.lastPullDate.HasValue)
                    return true; // Never pulled — always due

                var nextPullAt = link.lastPullDate.Value.AddMinutes(link.pullIntervalMinutes.Value);
                return now >= nextPullAt;
            }).ToList();

            if (readyToPull.Count == 0) return;

            _logger.LogInformation("Rebrickable periodic pull: {Count} tenant(s) due for pull", readyToPull.Count);

            foreach (var link in readyToPull)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    // Add jitter (0-10s) to avoid thundering herd
                    var jitterMs = _jitter.Next(0, 10000);
                    await Task.Delay(jitterMs, ct);

                    // Create a new scope for each pull to get a fresh DbContext
                    using var pullScope = _scopeFactory.CreateScope();
                    var syncService = pullScope.ServiceProvider.GetRequiredService<RebrickableSyncService>();

                    _logger.LogInformation(
                        "Rebrickable periodic pull starting for tenant {TenantGuid} (interval: {Interval}m, last: {LastPull})",
                        link.tenantGuid, link.pullIntervalMinutes, link.lastPullDate);

                    var result = await syncService.PullFullCollectionAsync(link.tenantGuid, ct);

                    _logger.LogInformation(
                        "Rebrickable periodic pull completed for tenant {TenantGuid}: " +
                        "created={Created}, updated={Updated}, errors={Errors}",
                        link.tenantGuid, result.TotalCreated, result.TotalUpdated, result.ErrorCount);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Rebrickable periodic pull failed for tenant {TenantGuid}",
                        link.tenantGuid);
                }
            }
        }
    }
}
