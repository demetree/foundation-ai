//
// Alerting Service Interface
//
// Defines the core incident lifecycle operations for the alerting system.
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alerting.Server.Models;
using Foundation.Alerting.Database;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Core service for incident lifecycle management.
    /// </summary>
    public interface IAlertingService
    {
        /// <summary>
        /// Triggers a new incident or updates an existing one based on incidentKey.
        /// </summary>
        /// <param name="integrationKey">The API key from the Integration record (unhashed).</param>
        /// <param name="payload">The alert payload.</param>
        /// <returns>Response indicating success/failure and incident details.</returns>
        Task<AlertResponse> TriggerAsync(string integrationKey, AlertPayload payload);

        /// <summary>
        /// Acknowledges an incident, stopping escalation until resolved or escalation timeout.
        /// </summary>
        /// <param name="incidentId">The incident ID.</param>
        /// <param name="actorObjectGuid">The user performing the acknowledgment.</param>
        /// <returns>The updated incident.</returns>
        Task<Incident> AcknowledgeAsync(int incidentId, Guid actorObjectGuid);

        /// <summary>
        /// Resolves an incident, stopping all escalation and marking it complete.
        /// </summary>
        /// <param name="incidentId">The incident ID.</param>
        /// <param name="actorObjectGuid">The user resolving the incident.</param>
        /// <returns>The updated incident.</returns>
        Task<Incident> ResolveAsync(int incidentId, Guid actorObjectGuid);

        /// <summary>
        /// Adds a note to an incident's timeline.
        /// </summary>
        /// <param name="incidentId">The incident ID.</param>
        /// <param name="content">The note content.</param>
        /// <param name="authorObjectGuid">The user adding the note.</param>
        /// <returns>The created note.</returns>
        Task<IncidentNote> AddNoteAsync(int incidentId, string content, Guid authorObjectGuid);

        /// <summary>
        /// Gets active (non-resolved) incidents for a tenant.
        /// </summary>
        /// <param name="tenantGuid">The tenant GUID.</param>
        /// <param name="serviceId">Optional filter by service.</param>
        /// <returns>List of active incidents.</returns>
        Task<List<Incident>> GetActiveIncidentsAsync(Guid tenantGuid, int? serviceId = null);

        /// <summary>
        /// Gets an incident by ID with its timeline events.
        /// </summary>
        /// <param name="incidentId">The incident ID.</param>
        /// <returns>The incident with timeline, or null if not found.</returns>
        Task<Incident> GetIncidentWithTimelineAsync(int incidentId);

        /// <summary>
        /// Gets incident counts by status for dashboard display.
        /// </summary>
        /// <param name="tenantGuid">The tenant GUID.</param>
        /// <returns>Dictionary of status name to count.</returns>
        Task<Dictionary<string, int>> GetIncidentCountsByStatusAsync(Guid tenantGuid);

        /// <summary>
        /// Gets incident counts by severity for active incidents.
        /// </summary>
        /// <param name="tenantGuid">The tenant GUID.</param>
        /// <returns>Dictionary of severity name to count.</returns>
        Task<Dictionary<string, int>> GetActiveIncidentCountsBySeverityAsync(Guid tenantGuid);
    }
}
