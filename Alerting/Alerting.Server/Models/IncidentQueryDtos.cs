//
// Incident Query DTOs
//
// Data transfer objects for incident query results from external integrations.
//
using System;
using System.Collections.Generic;

namespace Alerting.Server.Models
{
    /// <summary>
    /// Result of querying incidents for an integration.
    /// </summary>
    public class IncidentQueryResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<IncidentSummaryDto> Incidents { get; set; } = new List<IncidentSummaryDto>();
    }

    /// <summary>
    /// Summary of an incident for list responses.
    /// </summary>
    public class IncidentSummaryDto
    {
        public int IncidentId { get; set; }
        public string IncidentKey { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string ServiceName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    /// <summary>
    /// Result of querying incident status by key.
    /// </summary>
    public class IncidentStatusResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? IncidentId { get; set; }
        public string IncidentKey { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string AcknowledgedByName { get; set; }
        public string ResolvedByName { get; set; }
    }
}
