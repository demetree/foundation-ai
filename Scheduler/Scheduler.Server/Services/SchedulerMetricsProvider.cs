//
// Scheduler Metrics Provider
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Provides Scheduler-specific business metrics for the System Health dashboard.
// Implements IApplicationMetricsProvider to surface operational metrics for admins.
//
// Metrics are system-wide (not tenant-scoped) because the System Health dashboard
// is an administrative tool that shows global operational totals across all tenants.
//
using Foundation.Scheduler.Database;
using Foundation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler.Server.Services
{
    /// <summary>
    /// Provides Scheduler application-specific metrics for System Health dashboard.
    /// Registered as a Singleton — uses IServiceScopeFactory to resolve scoped DbContext.
    /// </summary>
    public class SchedulerMetricsProvider : IApplicationMetricsProvider
    {
        private readonly ILogger<SchedulerMetricsProvider> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public string ApplicationName => "Scheduler";

        public SchedulerMetricsProvider(ILogger<SchedulerMetricsProvider> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }


        public async Task<IEnumerable<ApplicationMetric>> GetMetricsAsync()
        {
            var metrics = new List<ApplicationMetric>();

            try
            {
                //
                // Resolve SchedulerContext from a DI scope so it picks up the configured
                // connection string, interceptors, and logging providers.
                //
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

                //
                // Count active scheduled events
                // (system-wide — not tenant-scoped; admin health dashboard shows global totals)
                //
                var totalEvents = await context.ScheduledEvents
                    .CountAsync()
                    .ConfigureAwait(false);

                metrics.Add(new ApplicationMetric
                {
                    Name = "Scheduled Events",
                    Value = totalEvents.ToString("N0"),
                    State = MetricState.Healthy,
                    DataType = MetricDataType.Number,
                    Category = "Events",
                    Description = "Total number of scheduled events in the system"
                });

                //
                // Count currently active events (in progress right now)
                //
                var now = DateTime.UtcNow;
                var activeEvents = await context.ScheduledEvents
                    .Where(e => e.startDateTime <= now && e.endDateTime >= now)
                    .CountAsync()
                    .ConfigureAwait(false);

                metrics.Add(new ApplicationMetric
                {
                    Name = "Active Events",
                    Value = activeEvents.ToString("N0"),
                    State = MetricState.Healthy,
                    DataType = MetricDataType.Number,
                    Category = "Events",
                    Description = "Events currently in progress",
                    Icon = "fa-play-circle"
                });

                //
                // Count resources
                //
                var resourceCount = await context.Resources
                    .Where(r => r.active && !r.deleted)
                    .CountAsync()
                    .ConfigureAwait(false);

                metrics.Add(new ApplicationMetric
                {
                    Name = "Active Resources",
                    Value = resourceCount.ToString("N0"),
                    State = resourceCount > 0 ? MetricState.Healthy : MetricState.Warning,
                    DataType = MetricDataType.Number,
                    Category = "Resources",
                    Description = "Number of active resources available for scheduling"
                });

                //
                // Count today's events
                //
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);
                var todaysEvents = await context.ScheduledEvents
                    .Where(e => e.startDateTime >= today && e.startDateTime < tomorrow)
                    .CountAsync()
                    .ConfigureAwait(false);

                metrics.Add(new ApplicationMetric
                {
                    Name = "Today's Events",
                    Value = todaysEvents.ToString("N0"),
                    State = MetricState.Healthy,
                    DataType = MetricDataType.Number,
                    Category = "Events",
                    Description = "Events scheduled for today"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scheduler metrics");
                metrics.Add(new ApplicationMetric
                {
                    Name = "Error",
                    Value = "Failed to retrieve metrics",
                    State = MetricState.Critical,
                    DataType = MetricDataType.Text
                });
            }

            return metrics;
        }
    }
}
