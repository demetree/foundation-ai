//
// Alerting Metrics Provider
//
// Provides Alerting-specific business metrics for the System Health dashboard.
//
using Foundation.Alerting.Database;
using Foundation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Provides Alerting application-specific metrics for System Health dashboard
    /// </summary>
    public class AlertingMetricsProvider : IApplicationMetricsProvider
    {
        private readonly ILogger<AlertingMetricsProvider> _logger;

        public string ApplicationName => "Alerting";

        public AlertingMetricsProvider(ILogger<AlertingMetricsProvider> logger)
        {
            _logger = logger;
        }


        public async Task<IEnumerable<ApplicationMetric>> GetMetricsAsync()
        {
            var metrics = new List<ApplicationMetric>();

            try
            {
                await using var context = new AlertingContext();

                //
                // Count active incidents (Triggered + Acknowledged)
                //
                var activeIncidents = await context.Incidents
                    .Where(i => i.incidentStatusTypeId != 3 && i.active && !i.deleted) // 3 = Resolved
                    .CountAsync()
                    .ConfigureAwait(false);

                var state = activeIncidents switch
                {
                    0 => MetricState.Healthy,
                    < 5 => MetricState.Warning,
                    _ => MetricState.Critical
                };

                metrics.Add(new ApplicationMetric
                {
                    Name = "Active Incidents",
                    Value = activeIncidents.ToString("N0"),
                    State = state,
                    DataType = MetricDataType.Number,
                    Category = "Incidents",
                    Description = "Incidents that are triggered or acknowledged",
                    Icon = "fa-bell"
                });

                //
                // Count by severity (for active incidents)
                //
                var criticalCount = await context.Incidents
                    .Where(i => i.severityTypeId == 1 && i.incidentStatusTypeId != 3 && i.active && !i.deleted)
                    .CountAsync()
                    .ConfigureAwait(false);

                metrics.Add(new ApplicationMetric
                {
                    Name = "Critical Incidents",
                    Value = criticalCount.ToString("N0"),
                    State = criticalCount > 0 ? MetricState.Critical : MetricState.Healthy,
                    DataType = MetricDataType.Number,
                    Category = "Incidents",
                    Description = "P1/Critical severity active incidents",
                    Icon = "fa-exclamation-triangle"
                });

                //
                // Count resolved today
                //
                var today = DateTime.UtcNow.Date;
                var resolvedToday = await context.Incidents
                    .Where(i => i.resolvedAt != null && i.resolvedAt >= today && i.active && !i.deleted)
                    .CountAsync()
                    .ConfigureAwait(false);

                metrics.Add(new ApplicationMetric
                {
                    Name = "Resolved Today",
                    Value = resolvedToday.ToString("N0"),
                    State = MetricState.Healthy,
                    DataType = MetricDataType.Number,
                    Category = "Incidents",
                    Description = "Incidents resolved today (UTC)",
                    Icon = "fa-check-circle"
                });

                //
                // Count configured services
                //
                var serviceCount = await context.Services
                    .Where(s => s.active && !s.deleted)
                    .CountAsync()
                    .ConfigureAwait(false);

                metrics.Add(new ApplicationMetric
                {
                    Name = "Monitored Services",
                    Value = serviceCount.ToString("N0"),
                    State = serviceCount > 0 ? MetricState.Healthy : MetricState.Warning,
                    DataType = MetricDataType.Number,
                    Category = "Configuration",
                    Description = "Number of services configured for alerting"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alerting metrics");
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

