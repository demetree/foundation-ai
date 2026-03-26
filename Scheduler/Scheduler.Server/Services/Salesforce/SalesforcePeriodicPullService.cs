//
// SalesforcePeriodicPullService.cs
//
// Background service that periodically pulls Salesforce data for tenants
// with sync enabled and a configured pull interval.
//
// Modeled on RebrickablePeriodicPullService.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration.
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Foundation.Scheduler.Database;
using Scheduler.Salesforce.Sync;


namespace Foundation.Scheduler.Services.Salesforce
{
    /// <summary>
    /// Background service that periodically pulls Salesforce data for tenants
    /// with sync enabled and a configured pull interval.
    ///
    /// Checks every 60 seconds for SalesforceTenantLink records where:
    ///   - syncEnabled == true
    ///   - syncDirectionFlags is "RealTime" or "ImportOnly"
    ///   - pullIntervalMinutes is set
    ///   - lastPullDate + pullIntervalMinutes < now
    ///
    /// Uses IServiceScopeFactory to create scoped instances of SalesforceSyncService
    /// since BackgroundService runs as a singleton.
    /// </summary>
    public class SalesforcePeriodicPullService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SalesforcePeriodicPullService> _logger;

        private static readonly Random _jitter = new();
        private const int CheckIntervalSeconds = 60;


        public SalesforcePeriodicPullService(
            IServiceScopeFactory scopeFactory,
            ILogger<SalesforcePeriodicPullService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Salesforce periodic pull service started.");

            // Initial delay to let the app fully start
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

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
                    _logger.LogError(ex, "Error in Salesforce periodic pull loop");
                }

                await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Salesforce periodic pull service stopped.");
        }


        private async Task CheckAndPullAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var now = DateTime.UtcNow;

            // Find all links due for a pull
            var dueLinks = await db.SalesforceTenantLinks
                .AsNoTracking()
                .Where(link =>
                    link.active == true
                    && link.deleted == false
                    && link.syncEnabled == true
                    && link.pullIntervalMinutes.HasValue
                    && link.pullIntervalMinutes.Value > 0
                    && (link.syncDirectionFlags == SyncDirection.RealTime
                        || link.syncDirectionFlags == SyncDirection.ImportOnly)
                )
                .ToListAsync(ct);

            // Filter in-memory for lastPullDate check (DateTime arithmetic in EF can be tricky)
            var readyToPull = dueLinks.Where(link =>
            {
                if (!link.lastPullDate.HasValue)
                {
                    return true; // Never pulled — always due
                }

                var nextPullAt = link.lastPullDate.Value.AddMinutes(link.pullIntervalMinutes.Value);
                return now >= nextPullAt;
            }).ToList();

            if (readyToPull.Count == 0) return;

            _logger.LogInformation("Salesforce periodic pull: {Count} tenant(s) due for pull", readyToPull.Count);

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
                    var syncService = pullScope.ServiceProvider.GetRequiredService<SalesforceSyncService>();
                    var pullDb = pullScope.ServiceProvider.GetRequiredService<SchedulerContext>();

                    _logger.LogInformation(
                        "Salesforce periodic pull starting for tenant {TenantGuid} (interval: {Interval}m, last: {LastPull})",
                        link.tenantGuid, link.pullIntervalMinutes, link.lastPullDate);

                    var config = await syncService.LoadConfigForTenantAsync(link.tenantGuid, ct);
                    if (config == null) continue;

                    var result = await syncService.PullAllAsync(config, link.lastPullDate, ct);

                    // Update the lastPullDate
                    var tracked = await pullDb.SalesforceTenantLinks.FindAsync(new object[] { link.id }, ct);
                    if (tracked != null)
                    {
                        tracked.lastPullDate = DateTime.UtcNow;
                        await pullDb.SaveChangesAsync(ct);
                    }

                    _logger.LogInformation(
                        "Salesforce periodic pull completed for tenant {TenantGuid}: " +
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
                        "Salesforce periodic pull failed for tenant {TenantGuid}",
                        link.tenantGuid);
                }
            }
        }
    }
}
