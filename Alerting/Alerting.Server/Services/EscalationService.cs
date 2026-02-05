//
// Escalation Service Implementation
//
// Processes escalation rules and resolves on-call targets.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Alerting.Server.Services.Notifications;
using Foundation.Alerting.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Processes escalation rules and resolves on-call schedules.
    /// </summary>
    public class EscalationService : IEscalationService
    {
        private readonly AlertingContext _context;
        private readonly INotificationDispatcher _dispatcher;
        private readonly ILogger<EscalationService> _logger;

        // Well-known status/event IDs
        private const int StatusTriggered = 1;
        private const int StatusAcknowledged = 2;
        private const int StatusResolved = 3;
        private const int EventEscalated = 2;  // Fixed: was incorrectly 3

        public EscalationService(
            AlertingContext context,
            INotificationDispatcher dispatcher,
            ILogger<EscalationService> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        /// <summary>
        /// Processes all pending escalations.
        /// </summary>
        public async Task<int> ProcessPendingEscalationsAsync()
        {
            var now = DateTime.UtcNow;

            // Find incidents needing escalation
            var incidentsToEscalate = await _context.Incidents
                .Include(i => i.escalationRule)
                    .ThenInclude(r => r.escalationPolicy)
                        .ThenInclude(p => p.EscalationRules)
                .Include(i => i.service)
                .Where(i => i.nextEscalationAt != null
                    && i.nextEscalationAt <= now
                    && i.incidentStatusTypeId == StatusTriggered
                    && i.active && !i.deleted)
                .ToListAsync()
                .ConfigureAwait(false);

            var processed = 0;

            foreach (var incident in incidentsToEscalate)
            {
                try
                {
                    await EscalateIncidentAsync(incident, now).ConfigureAwait(false);
                    processed++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to escalate incident {IncidentId}", incident.id);
                }
            }

            if (processed > 0)
            {
                _logger.LogInformation("Processed {Count} escalations", processed);
            }

            return processed;
        }

        /// <summary>
        /// Gets current on-call users for a schedule.
        /// </summary>
        public async Task<List<Guid>> GetCurrentOnCallUsersAsync(int scheduleId, DateTime? atTime = null)
        {
            var checkTime = atTime ?? DateTime.UtcNow;

            var schedule = await _context.OnCallSchedules
                .Include(s => s.ScheduleLayers.Where(l => l.active && !l.deleted))
                    .ThenInclude(l => l.ScheduleLayerMembers.Where(m => m.active && !m.deleted))
                .FirstOrDefaultAsync(s => s.id == scheduleId && s.active && !s.deleted)
                .ConfigureAwait(false);

            if (schedule == null)
            {
                _logger.LogWarning("Schedule {ScheduleId} not found", scheduleId);
                return new List<Guid>();
            }

            var onCallUsers = new List<Guid>();

            foreach (var layer in schedule.ScheduleLayers.OrderBy(l => l.layerLevel))
            {
                var members = layer.ScheduleLayerMembers.OrderBy(m => m.position).ToList();
                if (members.Count == 0) continue;

                // Calculate current member based on rotation
                var layerStart = layer.rotationStart;
                var rotationDays = layer.rotationDays;

                // Days since rotation start
                var daysSinceStart = (checkTime.Date - layerStart.Date).TotalDays;
                var rotationCycles = (int)(daysSinceStart / rotationDays);
                var currentPosition = rotationCycles % members.Count;

                var currentMember = members.ElementAtOrDefault(currentPosition);
                if (currentMember != null)
                {
                    onCallUsers.Add(currentMember.securityUserObjectGuid);
                }
            }

            return onCallUsers.Distinct().ToList();
        }

        /// <summary>
        /// Resolves targets for an escalation rule.
        /// </summary>
        public async Task<List<Guid>> ResolveEscalationTargetsAsync(EscalationRule rule)
        {
            var targets = new List<Guid>();

            switch (rule.targetType?.ToUpperInvariant())
            {
                case "USER":
                    if (rule.targetObjectGuid.HasValue)
                    {
                        targets.Add(rule.targetObjectGuid.Value);
                    }
                    break;

                case "TEAM":
                    // In a full implementation, would query Security database for team members
                    // For now, return the team guid as a placeholder
                    if (rule.targetObjectGuid.HasValue)
                    {
                        _logger.LogInformation("Team target {TeamGuid} - team member lookup not yet implemented",
                            rule.targetObjectGuid.Value);
                        targets.Add(rule.targetObjectGuid.Value);
                    }
                    break;

                case "SCHEDULE":
                    if (rule.targetObjectGuid.HasValue)
                    {
                        // Find schedule by objectGuid
                        var schedule = await _context.OnCallSchedules
                            .FirstOrDefaultAsync(s => s.objectGuid == rule.targetObjectGuid.Value
                                && s.active && !s.deleted)
                            .ConfigureAwait(false);

                        if (schedule != null)
                        {
                            var onCallUsers = await GetCurrentOnCallUsersAsync(schedule.id).ConfigureAwait(false);
                            targets.AddRange(onCallUsers);
                        }
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown target type {TargetType} for rule {RuleId}",
                        rule.targetType, rule.id);
                    break;
            }

            return targets;
        }

        #region Private Methods

        private async Task EscalateIncidentAsync(Incident incident, DateTime now)
        {
            var currentRule = incident.escalationRule;
            if (currentRule == null)
            {
                _logger.LogWarning("Incident {IncidentId} has no escalation rule", incident.id);
                incident.nextEscalationAt = null;
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return;
            }

            // Resolve targets and create notifications
            var targetUsers = await ResolveEscalationTargetsAsync(currentRule).ConfigureAwait(false);

            foreach (var userGuid in targetUsers)
            {
                await CreateNotificationAsync(incident, currentRule, userGuid).ConfigureAwait(false);
            }

            // Add escalated timeline event
            var escalationNote = $"Escalation step {currentRule.ruleOrder} fired - notified {targetUsers.Count} target(s)";
            await AddTimelineEventAsync(incident, EventEscalated, null, new
            {
                ruleId = currentRule.id,
                ruleOrder = currentRule.ruleOrder,
                targetCount = targetUsers.Count
            }, notes: escalationNote, source: "system").ConfigureAwait(false);

            // Determine next step
            if (incident.currentRepeatCount < currentRule.repeatCount)
            {
                // Repeat current rule
                incident.currentRepeatCount++;
                var delay = currentRule.repeatDelayMinutes ?? currentRule.delayMinutes;
                incident.nextEscalationAt = now.AddMinutes(delay);
            }
            else
            {
                // Move to next rule
                var policy = currentRule.escalationPolicy;
                var allRules = policy?.EscalationRules?
                    .Where(r => r.active && !r.deleted)
                    .OrderBy(r => r.ruleOrder)
                    .ToList();

                var nextRule = allRules?
                    .SkipWhile(r => r.id != currentRule.id)
                    .Skip(1)
                    .FirstOrDefault();

                if (nextRule != null)
                {
                    incident.escalationRuleId = nextRule.id;
                    incident.currentRepeatCount = 0;
                    incident.nextEscalationAt = now.AddMinutes(nextRule.delayMinutes);
                }
                else
                {
                    // No more rules - stop escalation
                    incident.nextEscalationAt = null;
                    _logger.LogInformation("Incident {IncidentId} exhausted all escalation rules", incident.id);
                }
            }

            incident.versionNumber++;
            await _context.SaveChangesAsync().ConfigureAwait(false);

            _logger.LogInformation("Escalated incident {IncidentId} to {TargetCount} targets",
                incident.id, targetUsers.Count);
        }

        private async Task CreateNotificationAsync(Incident incident, EscalationRule rule, Guid userGuid)
        {
            // Delegate to the notification dispatcher which handles:
            // - Creating IncidentNotification record
            // - Loading user preferences (DND, quiet hours)
            // - Determining enabled channels
            // - Sending via each channel provider
            // - Recording delivery attempts
            await _dispatcher.DispatchAsync(incident, userGuid, rule?.id);
        }

        private async Task AddTimelineEventAsync(Incident incident, int eventTypeId, Guid? actorGuid, object details, string notes = null, string source = "system")
        {
            var timelineEvent = new IncidentTimelineEvent
            {
                tenantGuid = incident.tenantGuid,
                incidentId = incident.id,
                incidentEventTypeId = eventTypeId,
                timestamp = DateTime.UtcNow,
                actorObjectGuid = actorGuid,
                detailsJson = details != null ? JsonSerializer.Serialize(details) : null,
                notes = notes,
                source = source,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.IncidentTimelineEvents.Add(timelineEvent);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
