//
// Incident Controller
//
// REST API for authenticated users to manage incidents.
//
using System;
using System.Threading.Tasks;
using Alerting.Server.Services;
using Foundation.Security;
using Foundation.Alerting.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;

namespace Alerting.Server.Controllers
{
    /// <summary>
    /// API for authenticated users to manage incidents.
    /// </summary>
    [ApiController]
    [Route("api/incidents")]
    [Authorize]
    public class IncidentController : ControllerBase
    {
        private readonly IAlertingService _alertingService;
        private readonly AlertingContext _context;
        private readonly ILogger<IncidentController> _logger;

        public IncidentController(
            IAlertingService alertingService,
            AlertingContext context,
            ILogger<IncidentController> logger)
        {
            _alertingService = alertingService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get active incidents for the current tenant.
        /// </summary>
        /// <param name="serviceId">Optional filter by service.</param>
        /// <param name="severityId">Optional filter by severity.</param>
        /// <param name="includeResolved">Include resolved incidents (default: false).</param>
        /// <returns>List of incidents.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<IncidentDto>), 200)]
        public async Task<IActionResult> GetIncidents(
            [FromQuery] int? serviceId = null,
            [FromQuery] int? severityId = null,
            [FromQuery] bool includeResolved = false)
        {
            var tenantGuid = GetTenantGuid();

            var query = _context.Incidents
                .Include(i => i.service)
                .Include(i => i.severityType)
                .Include(i => i.incidentStatusType)
                .Where(i => i.tenantGuid == tenantGuid && i.active && !i.deleted);

            if (!includeResolved)
            {
                query = query.Where(i => i.incidentStatusTypeId != 3); // 3 = Resolved
            }

            if (serviceId.HasValue)
            {
                query = query.Where(i => i.serviceId == serviceId.Value);
            }

            if (severityId.HasValue)
            {
                query = query.Where(i => i.severityTypeId == severityId.Value);
            }

            var incidents = await query
                .OrderByDescending(i => i.createdAt)
                .Take(100) // Limit for performance
                .Select(i => new IncidentDto
                {
                    Id = i.id,
                    ObjectGuid = i.objectGuid,
                    IncidentKey = i.incidentKey,
                    Title = i.title,
                    Description = i.description,
                    ServiceId = i.serviceId,
                    ServiceName = i.service.name,
                    SeverityId = i.severityTypeId,
                    SeverityName = i.severityType.name,
                    StatusId = i.incidentStatusTypeId,
                    StatusName = i.incidentStatusType.name,
                    CreatedAt = i.createdAt,
                    AcknowledgedAt = i.acknowledgedAt,
                    ResolvedAt = i.resolvedAt
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return Ok(incidents);
        }

        /// <summary>
        /// Get incident details with timeline.
        /// </summary>
        /// <param name="id">Incident ID.</param>
        /// <returns>Incident with timeline events and notes.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IncidentDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetIncident(int id)
        {
            var incident = await _alertingService.GetIncidentWithTimelineAsync(id).ConfigureAwait(false);

            if (incident == null)
            {
                return NotFound();
            }

            var dto = new IncidentDetailDto
            {
                Id = incident.id,
                ObjectGuid = incident.objectGuid,
                IncidentKey = incident.incidentKey,
                Title = incident.title,
                Description = incident.description,
                ServiceId = incident.serviceId,
                ServiceName = incident.service?.name,
                SeverityId = incident.severityTypeId,
                SeverityName = incident.severityType?.name,
                StatusId = incident.incidentStatusTypeId,
                StatusName = incident.incidentStatusType?.name,
                CreatedAt = incident.createdAt,
                AcknowledgedAt = incident.acknowledgedAt,
                ResolvedAt = incident.resolvedAt,
                Timeline = incident.IncidentTimelineEvents?.Select(e => new TimelineEventDto
                {
                    Id = e.id,
                    EventType = e.incidentEventType?.name,
                    Timestamp = e.timestamp,
                    ActorObjectGuid = e.actorObjectGuid,
                    DetailsJson = e.detailsJson
                }).ToList() ?? new List<TimelineEventDto>(),
                Notes = incident.IncidentNotes?.Select(n => new NoteDto
                {
                    Id = n.id,
                    Content = n.content,
                    CreatedAt = n.createdAt,
                    AuthorObjectGuid = n.authorObjectGuid
                }).ToList() ?? new List<NoteDto>()
            };

            return Ok(dto);
        }

        /// <summary>
        /// Acknowledge an incident.
        /// </summary>
        /// <param name="id">Incident ID.</param>
        /// <returns>Updated incident.</returns>
        [HttpPost("{id}/acknowledge")]
        [ProducesResponseType(typeof(IncidentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AcknowledgeIncident(int id)
        {
            try
            {
                var actorGuid = GetUserObjectGuid();
                var incident = await _alertingService.AcknowledgeAsync(id, actorGuid).ConfigureAwait(false);

                _logger.LogInformation("Incident {IncidentId} acknowledged by user {UserGuid}", id, actorGuid);

                return Ok(MapToDto(incident));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Resolve an incident.
        /// </summary>
        /// <param name="id">Incident ID.</param>
        /// <returns>Updated incident.</returns>
        [HttpPost("{id}/resolve")]
        [ProducesResponseType(typeof(IncidentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ResolveIncident(int id)
        {
            try
            {
                var actorGuid = GetUserObjectGuid();
                var incident = await _alertingService.ResolveAsync(id, actorGuid).ConfigureAwait(false);

                _logger.LogInformation("Incident {IncidentId} resolved by user {UserGuid}", id, actorGuid);

                return Ok(MapToDto(incident));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Add a note to an incident.
        /// </summary>
        /// <param name="id">Incident ID.</param>
        /// <param name="request">Note content.</param>
        /// <returns>Created note.</returns>
        [HttpPost("{id}/notes")]
        [ProducesResponseType(typeof(NoteDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddNote(int id, [FromBody] AddNoteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Content))
            {
                return BadRequest(new { error = "Content is required" });
            }

            try
            {
                var authorGuid = GetUserObjectGuid();
                var note = await _alertingService.AddNoteAsync(id, request.Content, authorGuid).ConfigureAwait(false);

                return CreatedAtAction(nameof(GetIncident), new { id }, new NoteDto
                {
                    Id = note.id,
                    Content = note.content,
                    CreatedAt = note.createdAt,
                    AuthorObjectGuid = note.authorObjectGuid
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get incident statistics for the dashboard.
        /// </summary>
        /// <returns>Incident counts by status and severity.</returns>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(IncidentStatsDto), 200)]
        public async Task<IActionResult> GetStats()
        {
            var tenantGuid = GetTenantGuid();

            var byStatus = await _alertingService.GetIncidentCountsByStatusAsync(tenantGuid).ConfigureAwait(false);
            var bySeverity = await _alertingService.GetActiveIncidentCountsBySeverityAsync(tenantGuid).ConfigureAwait(false);

            return Ok(new IncidentStatsDto
            {
                CountsByStatus = byStatus,
                CountsBySeverity = bySeverity,
                TotalActive = bySeverity.Values.Sum()
            });
        }

        #region Helpers

        private Guid GetTenantGuid()
        {
            // Get from claims or use a default for single-tenant scenarios
            var claim = User.FindFirst("tenant_guid");
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }

        private Guid GetUserObjectGuid()
        {
            // Get from claims
            var claim = User.FindFirst("sub") ?? User.FindFirst("object_guid");
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }

        private static IncidentDto MapToDto(Incident incident)
        {
            return new IncidentDto
            {
                Id = incident.id,
                ObjectGuid = incident.objectGuid,
                IncidentKey = incident.incidentKey,
                Title = incident.title,
                Description = incident.description,
                ServiceId = incident.serviceId,
                ServiceName = incident.service?.name,
                SeverityId = incident.severityTypeId,
                SeverityName = incident.severityType?.name,
                StatusId = incident.incidentStatusTypeId,
                StatusName = incident.incidentStatusType?.name,
                CreatedAt = incident.createdAt,
                AcknowledgedAt = incident.acknowledgedAt,
                ResolvedAt = incident.resolvedAt
            };
        }

        #endregion
    }

    #region DTOs

    public class IncidentDto
    {
        public int Id { get; set; }
        public Guid ObjectGuid { get; set; }
        public string IncidentKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int SeverityId { get; set; }
        public string SeverityName { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class IncidentDetailDto : IncidentDto
    {
        public List<TimelineEventDto> Timeline { get; set; }
        public List<NoteDto> Notes { get; set; }
    }

    public class TimelineEventDto
    {
        public int Id { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? ActorObjectGuid { get; set; }
        public string DetailsJson { get; set; }
    }

    public class NoteDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? AuthorObjectGuid { get; set; }
    }

    public class AddNoteRequest
    {
        public string Content { get; set; }
    }

    public class IncidentStatsDto
    {
        public Dictionary<string, int> CountsByStatus { get; set; }
        public Dictionary<string, int> CountsBySeverity { get; set; }
        public int TotalActive { get; set; }
    }

    #endregion
}
