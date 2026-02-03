//
// Dashboard Service Implementation
//
// Aggregates data from multiple sources for the Alerting Command Center dashboard.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alerting.Server.Models;
using Foundation.Alerting.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Aggregates data for the Alerting Command Center dashboard.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly AlertingContext _context;
        private readonly IEscalationService _escalationService;
        private readonly ILogger<DashboardService> _logger;

        // Well-known status type IDs
        private const int StatusTriggered = 1;
        private const int StatusAcknowledged = 2;
        private const int StatusResolved = 3;

        // Well-known severity type IDs
        private const int SeverityCritical = 1;
        private const int SeverityHigh = 2;
        private const int SeverityMedium = 3;
        private const int SeverityLow = 4;
        private const int SeverityInfo = 5;

        public DashboardService(
            AlertingContext context,
            IEscalationService escalationService,
            ILogger<DashboardService> logger)
        {
            _context = context;
            _escalationService = escalationService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a complete dashboard summary with all metrics and data.
        /// </summary>
        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var sevenDaysAgo = now.AddDays(-7);

            //
            // Execute queries sequentially - DbContext is NOT thread-safe
            // Sequential execution prevents concurrent access errors
            //
            var incidentMetrics = await GetIncidentMetricsAsync(todayStart).ConfigureAwait(false);
            var onCallSummary = await GetOnCallSummaryAsync().ConfigureAwait(false);
            var recentActivity = await GetRecentActivityAsync().ConfigureAwait(false);
            var configCounts = await GetConfigurationCountsAsync().ConfigureAwait(false);
            var performance = await GetPerformanceMetricsAsync(sevenDaysAgo, now).ConfigureAwait(false);

            //
            // Determine operational status based on active incident count
            //
            var status = incidentMetrics.ActiveCount switch
            {
                0 => OperationalStatus.Healthy,
                < 5 => OperationalStatus.Degraded,
                _ => OperationalStatus.Critical
            };

            return new DashboardSummaryDto
            {
                Status = status,
                GeneratedAt = now,
                IncidentMetrics = incidentMetrics,
                OnCallSummary = onCallSummary,
                RecentActivity = recentActivity,
                ConfigCounts = configCounts,
                Performance = performance
            };
        }

        #region Private Data Retrieval Methods

        private async Task<IncidentMetricsDto> GetIncidentMetricsAsync(DateTime todayStart)
        {
            var activeQuery = _context.Incidents
                .Where(i => i.active && !i.deleted && i.incidentStatusTypeId != StatusResolved);

            var triggeredCount = await activeQuery
                .CountAsync(i => i.incidentStatusTypeId == StatusTriggered)
                .ConfigureAwait(false);

            var acknowledgedCount = await activeQuery
                .CountAsync(i => i.incidentStatusTypeId == StatusAcknowledged)
                .ConfigureAwait(false);

            var resolvedTodayCount = await _context.Incidents
                .CountAsync(i => i.active && !i.deleted 
                    && i.incidentStatusTypeId == StatusResolved 
                    && i.resolvedAt >= todayStart)
                .ConfigureAwait(false);

            // Severity breakdown for active incidents
            var criticalCount = await activeQuery.CountAsync(i => i.severityTypeId == SeverityCritical).ConfigureAwait(false);
            var highCount = await activeQuery.CountAsync(i => i.severityTypeId == SeverityHigh).ConfigureAwait(false);
            var mediumCount = await activeQuery.CountAsync(i => i.severityTypeId == SeverityMedium).ConfigureAwait(false);
            var lowCount = await activeQuery.CountAsync(i => i.severityTypeId == SeverityLow).ConfigureAwait(false);
            var infoCount = await activeQuery.CountAsync(i => i.severityTypeId == SeverityInfo).ConfigureAwait(false);

            return new IncidentMetricsDto
            {
                ActiveCount = triggeredCount + acknowledgedCount,
                TriggeredCount = triggeredCount,
                AcknowledgedCount = acknowledgedCount,
                ResolvedTodayCount = resolvedTodayCount,
                CriticalCount = criticalCount,
                HighCount = highCount,
                MediumCount = mediumCount,
                LowCount = lowCount,
                InfoCount = infoCount
            };
        }

        private async Task<List<OnCallScheduleSummaryDto>> GetOnCallSummaryAsync()
        {
            var schedules = await _context.OnCallSchedules
                .Where(s => s.active && !s.deleted)
                .Include(s => s.ScheduleLayers.Where(l => l.active && !l.deleted))
                    .ThenInclude(l => l.ScheduleLayerMembers.Where(m => m.active && !m.deleted))
                .OrderBy(s => s.name)
                .ToListAsync()
                .ConfigureAwait(false);

            var result = new List<OnCallScheduleSummaryDto>();

            foreach (var schedule in schedules)
            {
                var onCallUserGuids = await _escalationService
                    .GetCurrentOnCallUsersAsync(schedule.id)
                    .ConfigureAwait(false);

                var onCallUsers = new List<OnCallUserDto>();
                foreach (var userGuid in onCallUserGuids)
                {
                    // Find the member info from schedule layers
                    var member = schedule.ScheduleLayers
                        .SelectMany(l => l.ScheduleLayerMembers)
                        .FirstOrDefault(m => m.securityUserObjectGuid == userGuid);

                    var layer = schedule.ScheduleLayers
                        .FirstOrDefault(l => l.ScheduleLayerMembers.Any(m => m.securityUserObjectGuid == userGuid));

                    onCallUsers.Add(new OnCallUserDto
                    {
                        UserObjectGuid = userGuid,
                        DisplayName = "User", // Would need to resolve from Security module
                        Email = null, // Would need to resolve from Security module
                        LayerName = layer?.name
                    });
                }

                result.Add(new OnCallScheduleSummaryDto
                {
                    ScheduleId = schedule.id,
                    ScheduleObjectGuid = schedule.objectGuid,
                    ScheduleName = schedule.name ?? "Unnamed Schedule",
                    Timezone = schedule.timeZoneId,
                    OnCallUsers = onCallUsers
                });
            }

            return result;
        }

        private async Task<List<RecentActivityDto>> GetRecentActivityAsync()
        {
            var recent = await _context.IncidentTimelineEvents
                .Include(e => e.incident)
                    .ThenInclude(i => i.severityType)
                .Include(e => e.incidentEventType)
                .Where(e => e.active && !e.deleted)
                .OrderByDescending(e => e.timestamp)
                .Take(10)
                .ToListAsync()
                .ConfigureAwait(false);

            return recent.Select(e => new RecentActivityDto
            {
                Id = e.id,
                Timestamp = e.timestamp,
                EventType = e.incidentEventType?.name ?? "Unknown",
                IncidentId = e.incidentId,
                IncidentTitle = e.incident?.title ?? "Unknown Incident",
                ActorName = null, // Would need to resolve from Security module using actorObjectGuid
                SeverityName = e.incident?.severityType?.name ?? "Unknown"
            }).ToList();
        }

        private async Task<ConfigurationCountsDto> GetConfigurationCountsAsync()
        {
            var servicesCount = await _context.Services
                .CountAsync(s => s.active && !s.deleted)
                .ConfigureAwait(false);

            var integrationsCount = await _context.Integrations
                .CountAsync(i => i.active && !i.deleted)
                .ConfigureAwait(false);

            var schedulesCount = await _context.OnCallSchedules
                .CountAsync(s => s.active && !s.deleted)
                .ConfigureAwait(false);

            var policiesCount = await _context.EscalationPolicies
                .CountAsync(p => p.active && !p.deleted)
                .ConfigureAwait(false);

            return new ConfigurationCountsDto
            {
                ServicesCount = servicesCount,
                IntegrationsCount = integrationsCount,
                SchedulesCount = schedulesCount,
                PoliciesCount = policiesCount
            };
        }

        private async Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(DateTime from, DateTime to)
        {
            //
            // Get resolved incidents from last 7 days for MTTA/MTTR calculation
            //
            var resolvedIncidents = await _context.Incidents
                .Where(i => i.active && !i.deleted
                    && i.incidentStatusTypeId == StatusResolved
                    && i.resolvedAt >= from && i.resolvedAt <= to)
                .ToListAsync()
                .ConfigureAwait(false);

            decimal? mttaMinutes = null;
            decimal? mttrMinutes = null;

            if (resolvedIncidents.Any())
            {
                // MTTA: Time from creation to acknowledgment
                var acknowledgedIncidents = resolvedIncidents
                    .Where(i => i.acknowledgedAt.HasValue)
                    .ToList();

                if (acknowledgedIncidents.Any())
                {
                    var ttaValues = acknowledgedIncidents
                        .Select(i => (i.acknowledgedAt!.Value - i.createdAt).TotalMinutes)
                        .ToList();
                    mttaMinutes = (decimal)ttaValues.Average();
                }

                // MTTR: Time from creation to resolution
                var resolvedWithTime = resolvedIncidents
                    .Where(i => i.resolvedAt.HasValue)
                    .ToList();

                if (resolvedWithTime.Any())
                {
                    var ttrValues = resolvedWithTime
                        .Select(i => (i.resolvedAt!.Value - i.createdAt).TotalMinutes)
                        .ToList();
                    mttrMinutes = (decimal)ttrValues.Average();
                }
            }

            //
            // Calculate trends (compare last 7 days vs previous 7 days)
            //
            var previousFrom = from.AddDays(-7);
            var previousResolvedIncidents = await _context.Incidents
                .Where(i => i.active && !i.deleted
                    && i.incidentStatusTypeId == StatusResolved
                    && i.resolvedAt >= previousFrom && i.resolvedAt < from)
                .ToListAsync()
                .ConfigureAwait(false);

            string mttaTrend = "stable";
            string mttrTrend = "stable";

            if (previousResolvedIncidents.Any() && resolvedIncidents.Any())
            {
                // Calculate previous period MTTA
                var prevAcked = previousResolvedIncidents
                    .Where(i => i.acknowledgedAt.HasValue)
                    .ToList();
                if (prevAcked.Any() && mttaMinutes.HasValue)
                {
                    var prevMtta = (decimal)prevAcked
                        .Select(i => (i.acknowledgedAt!.Value - i.createdAt).TotalMinutes)
                        .Average();
                    mttaTrend = mttaMinutes.Value < prevMtta ? "improving" : 
                                mttaMinutes.Value > prevMtta ? "worsening" : "stable";
                }

                // Calculate previous period MTTR
                var prevResolved = previousResolvedIncidents
                    .Where(i => i.resolvedAt.HasValue)
                    .ToList();
                if (prevResolved.Any() && mttrMinutes.HasValue)
                {
                    var prevMttr = (decimal)prevResolved
                        .Select(i => (i.resolvedAt!.Value - i.createdAt).TotalMinutes)
                        .Average();
                    mttrTrend = mttrMinutes.Value < prevMttr ? "improving" :
                                mttrMinutes.Value > prevMttr ? "worsening" : "stable";
                }
            }

            return new PerformanceMetricsDto
            {
                MttaMinutes = mttaMinutes.HasValue ? Math.Round(mttaMinutes.Value, 1) : null,
                MttrMinutes = mttrMinutes.HasValue ? Math.Round(mttrMinutes.Value, 1) : null,
                MttaTrend = mttaTrend,
                MttrTrend = mttrTrend,
                IncidentsResolvedLast7Days = resolvedIncidents.Count
            };
        }

        #endregion
    }
}
