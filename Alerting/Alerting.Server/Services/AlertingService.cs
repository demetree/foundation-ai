//
// Alerting Service Implementation
//
// Core incident lifecycle management for the alerting system.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Alerting.Server.Models;
using Foundation.Alerting.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Core service for incident lifecycle management.
    /// </summary>
    public class AlertingService : IAlertingService
    {
        private readonly AlertingContext _context;
        private readonly ILogger<AlertingService> _logger;

        // Cache for lookup tables
        private Dictionary<string, int> _severityCache;
        private Dictionary<string, int> _statusCache;
        private Dictionary<string, int> _eventTypeCache;

        // Well-known status IDs (from seed data)
        private const int StatusTriggered = 1;
        private const int StatusAcknowledged = 2;
        private const int StatusResolved = 3;

        // Well-known event type IDs (from seed data)
        private const int EventTriggered = 1;
        private const int EventAcknowledged = 2;
        private const int EventEscalated = 3;
        private const int EventResolved = 4;
        private const int EventNoteAdded = 5;
        private const int EventReassigned = 6;

        public AlertingService(AlertingContext context, ILogger<AlertingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Triggers a new incident or updates an existing one.
        /// </summary>
        public async Task<AlertResponse> TriggerAsync(string integrationKey, AlertPayload payload)
        {
            try
            {
                // Validate and lookup integration by API key hash
                var apiKeyHash = HashApiKey(integrationKey);
                var integration = await _context.Integrations
                    .Include(i => i.service)
                        .ThenInclude(s => s.escalationPolicy)
                            .ThenInclude(ep => ep.EscalationRules)
                    .FirstOrDefaultAsync(i => i.apiKeyHash == apiKeyHash && i.active && !i.deleted)
                    .ConfigureAwait(false);

                if (integration == null)
                {
                    _logger.LogWarning("Invalid integration key provided");
                    return new AlertResponse
                    {
                        Success = false,
                        Message = "Invalid integration key"
                    };
                }

                var service = integration.service;
                var tenantGuid = integration.tenantGuid;

                // Generate incident key if not provided
                var incidentKey = string.IsNullOrWhiteSpace(payload.IncidentKey)
                    ? $"{service.name}-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}"[..50]
                    : payload.IncidentKey;

                // Check for existing open incident with this key
                var existingIncident = await _context.Incidents
                    .FirstOrDefaultAsync(i => i.tenantGuid == tenantGuid
                        && i.incidentKey == incidentKey
                        && i.incidentStatusTypeId != StatusResolved
                        && i.active && !i.deleted)
                    .ConfigureAwait(false);

                if (existingIncident != null)
                {
                    // Update existing incident (e.g., for de-duplication)
                    existingIncident.description = payload.Description ?? existingIncident.description;
                    existingIncident.sourcePayloadJson = payload.SourcePayloadJson;
                    existingIncident.versionNumber++;

                    await _context.SaveChangesAsync().ConfigureAwait(false);

                    _logger.LogInformation("Updated existing incident {IncidentId} for key {IncidentKey}",
                        existingIncident.id, incidentKey);

                    return new AlertResponse
                    {
                        Success = true,
                        IncidentId = existingIncident.id,
                        IncidentKey = incidentKey,
                        Message = "Existing incident updated",
                        IsNew = false
                    };
                }

                // Resolve severity
                var severityId = await GetSeverityIdAsync(payload.Severity ?? "High").ConfigureAwait(false);

                // Get first escalation rule if policy exists
                EscalationRule firstRule = null;
                DateTime? nextEscalationAt = null;

                if (service.escalationPolicy != null)
                {
                    firstRule = service.escalationPolicy.EscalationRules
                        .Where(r => r.active && !r.deleted)
                        .OrderBy(r => r.ruleOrder)
                        .FirstOrDefault();

                    if (firstRule != null)
                    {
                        nextEscalationAt = DateTime.UtcNow.AddMinutes(firstRule.delayMinutes);
                    }
                }

                // Create new incident
                var incident = new Incident
                {
                    tenantGuid = tenantGuid,
                    incidentKey = incidentKey,
                    serviceId = service.id,
                    title = payload.Title ?? "Alert triggered",
                    description = payload.Description,
                    severityTypeId = severityId,
                    incidentStatusTypeId = StatusTriggered,
                    createdAt = DateTime.UtcNow,
                    escalationRuleId = firstRule?.id,
                    currentRepeatCount = 0,
                    nextEscalationAt = nextEscalationAt,
                    sourcePayloadJson = payload.SourcePayloadJson,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.Incidents.Add(incident);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                // Add triggered timeline event
                await AddTimelineEventAsync(incident, EventTriggered, null, new
                {
                    severity = payload.Severity,
                    source = integration.name
                }).ConfigureAwait(false);

                _logger.LogInformation("Created new incident {IncidentId} for service {ServiceName}",
                    incident.id, service.name);

                return new AlertResponse
                {
                    Success = true,
                    IncidentId = incident.id,
                    IncidentKey = incidentKey,
                    Message = "Incident created",
                    IsNew = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering alert");
                return new AlertResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Acknowledges an incident.
        /// </summary>
        public async Task<Incident> AcknowledgeAsync(int incidentId, Guid actorObjectGuid)
        {
            var incident = await _context.Incidents
                .FirstOrDefaultAsync(i => i.id == incidentId && i.active && !i.deleted)
                .ConfigureAwait(false);

            if (incident == null)
            {
                throw new ArgumentException($"Incident {incidentId} not found");
            }

            if (incident.incidentStatusTypeId == StatusResolved)
            {
                throw new InvalidOperationException("Cannot acknowledge a resolved incident");
            }

            incident.incidentStatusTypeId = StatusAcknowledged;
            incident.acknowledgedAt = DateTime.UtcNow;
            incident.currentAssigneeObjectGuid = actorObjectGuid;
            incident.nextEscalationAt = null; // Stop escalation
            incident.versionNumber++;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            await AddTimelineEventAsync(incident, EventAcknowledged, actorObjectGuid, null).ConfigureAwait(false);

            _logger.LogInformation("Incident {IncidentId} acknowledged by {ActorGuid}", incidentId, actorObjectGuid);

            return incident;
        }

        /// <summary>
        /// Resolves an incident.
        /// </summary>
        public async Task<Incident> ResolveAsync(int incidentId, Guid actorObjectGuid)
        {
            var incident = await _context.Incidents
                .FirstOrDefaultAsync(i => i.id == incidentId && i.active && !i.deleted)
                .ConfigureAwait(false);

            if (incident == null)
            {
                throw new ArgumentException($"Incident {incidentId} not found");
            }

            if (incident.incidentStatusTypeId == StatusResolved)
            {
                return incident; // Already resolved
            }

            incident.incidentStatusTypeId = StatusResolved;
            incident.resolvedAt = DateTime.UtcNow;
            incident.nextEscalationAt = null;
            incident.versionNumber++;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            await AddTimelineEventAsync(incident, EventResolved, actorObjectGuid, null).ConfigureAwait(false);

            _logger.LogInformation("Incident {IncidentId} resolved by {ActorGuid}", incidentId, actorObjectGuid);

            return incident;
        }

        /// <summary>
        /// Adds a note to an incident.
        /// </summary>
        public async Task<IncidentNote> AddNoteAsync(int incidentId, string content, Guid authorObjectGuid)
        {
            var incident = await _context.Incidents
                .FirstOrDefaultAsync(i => i.id == incidentId && i.active && !i.deleted)
                .ConfigureAwait(false);

            if (incident == null)
            {
                throw new ArgumentException($"Incident {incidentId} not found");
            }

            var note = new IncidentNote
            {
                tenantGuid = incident.tenantGuid,
                incidentId = incidentId,
                authorObjectGuid = authorObjectGuid,
                content = content,
                createdAt = DateTime.UtcNow,
                versionNumber = 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.IncidentNotes.Add(note);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            await AddTimelineEventAsync(incident, EventNoteAdded, authorObjectGuid, new { noteId = note.id }).ConfigureAwait(false);

            return note;
        }

        /// <summary>
        /// Gets active incidents for a tenant.
        /// </summary>
        public async Task<List<Incident>> GetActiveIncidentsAsync(Guid tenantGuid, int? serviceId = null)
        {
            var query = _context.Incidents
                .Include(i => i.service)
                .Include(i => i.severityType)
                .Include(i => i.incidentStatusType)
                .Where(i => i.tenantGuid == tenantGuid
                    && i.incidentStatusTypeId != StatusResolved
                    && i.active && !i.deleted);

            if (serviceId.HasValue)
            {
                query = query.Where(i => i.serviceId == serviceId.Value);
            }

            return await query
                .OrderByDescending(i => i.createdAt)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an incident with its timeline.
        /// </summary>
        public async Task<Incident> GetIncidentWithTimelineAsync(int incidentId)
        {
            return await _context.Incidents
                .Include(i => i.service)
                .Include(i => i.severityType)
                .Include(i => i.incidentStatusType)
                .Include(i => i.IncidentTimelineEvents.OrderByDescending(e => e.timestamp))
                    .ThenInclude(e => e.incidentEventType)
                .Include(i => i.IncidentNotes.OrderByDescending(n => n.createdAt))
                .FirstOrDefaultAsync(i => i.id == incidentId && i.active && !i.deleted)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets incident counts by status.
        /// </summary>
        public async Task<Dictionary<string, int>> GetIncidentCountsByStatusAsync(Guid tenantGuid)
        {
            return await _context.Incidents
                .Where(i => i.tenantGuid == tenantGuid && i.active && !i.deleted)
                .GroupBy(i => i.incidentStatusType.name)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets active incident counts by severity.
        /// </summary>
        public async Task<Dictionary<string, int>> GetActiveIncidentCountsBySeverityAsync(Guid tenantGuid)
        {
            return await _context.Incidents
                .Include(i => i.severityType)
                .Where(i => i.tenantGuid == tenantGuid
                    && i.incidentStatusTypeId != StatusResolved
                    && i.active && !i.deleted)
                .GroupBy(i => i.severityType.name)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Severity, x => x.Count)
                .ConfigureAwait(false);
        }

        #region Private Helpers

        private async Task AddTimelineEventAsync(Incident incident, int eventTypeId, Guid? actorGuid, object details)
        {
            var timelineEvent = new IncidentTimelineEvent
            {
                tenantGuid = incident.tenantGuid,
                incidentId = incident.id,
                incidentEventTypeId = eventTypeId,
                timestamp = DateTime.UtcNow,
                actorObjectGuid = actorGuid,
                detailsJson = details != null ? JsonSerializer.Serialize(details) : null,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.IncidentTimelineEvents.Add(timelineEvent);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task<int> GetSeverityIdAsync(string severityName)
        {
            if (_severityCache == null)
            {
                _severityCache = await _context.SeverityTypes
                    .Where(s => s.active && !s.deleted)
                    .ToDictionaryAsync(s => s.name, s => s.id, StringComparer.OrdinalIgnoreCase)
                    .ConfigureAwait(false);
            }

            if (_severityCache.TryGetValue(severityName, out var id))
            {
                return id;
            }

            // Default to "High" if not found
            return _severityCache.TryGetValue("High", out var highId) ? highId : 2;
        }

        private static string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToBase64String(bytes);
        }

        #endregion
    }
}
