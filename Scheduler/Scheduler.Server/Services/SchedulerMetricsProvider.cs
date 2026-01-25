//
// Scheduler Metrics Provider
//
// Provides Scheduler-specific business metrics for the System Health dashboard.
// This is an example implementation of IApplicationMetricsProvider.
//
using Foundation.Scheduler.Database;
using Foundation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler.Server.Services
{
    /// <summary>
    /// Provides Scheduler application-specific metrics for System Health dashboard
    /// </summary>
    public class SchedulerMetricsProvider : IApplicationMetricsProvider
    {
        private readonly ILogger<SchedulerMetricsProvider> _logger;

        public string ApplicationName => "Scheduler";

        public SchedulerMetricsProvider(ILogger<SchedulerMetricsProvider> logger)
        {
            _logger = logger;
        }


        public async Task<IEnumerable<ApplicationMetric>> GetMetricsAsync()
        {
            var metrics = new List<ApplicationMetric>();

            try
            {
                await using var context = new SchedulerContext();

                //
                // Count active scheduled events
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
