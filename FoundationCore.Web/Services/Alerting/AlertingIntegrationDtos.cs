//
// Alerting Integration DTOs
//
// Data transfer objects for Alerting API communication.
//
using System;
using System.Collections.Generic;

namespace Foundation.Web.Services.Alerting
{
    #region Incident DTOs

    /// <summary>
    /// Request to raise a new incident in Alerting.
    /// </summary>
    public class RaiseIncidentRequest
    {
        /// <summary>
        /// Severity level: "Critical", "High", "Medium", "Low", "Info".
        /// </summary>
        public string Severity { get; set; } = "Medium";

        /// <summary>
        /// Short title describing the incident.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Detailed description of the incident.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional deduplication key. If an open incident exists with same key,
        /// this will be treated as an update instead of a new incident.
        /// </summary>
        public string DeduplicationKey { get; set; }

        /// <summary>
        /// Optional source system identifier.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Optional URL to view more details about this incident.
        /// </summary>
        public string DetailsUrl { get; set; }

        /// <summary>
        /// Optional custom data to include with the incident.
        /// </summary>
        public Dictionary<string, string> CustomFields { get; set; }
    }

    /// <summary>
    /// Response from raising an incident.
    /// </summary>
    public class IncidentResponse
    {
        public string IncidentKey { get; set; }
        public int IncidentId { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsNew { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Current status of an incident.
    /// </summary>
    public class IncidentStatusResponse
    {
        public string IncidentKey { get; set; }
        public int IncidentId { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string AcknowledgedBy { get; set; }
        public string ResolvedBy { get; set; }
        public int EscalationLevel { get; set; }
    }

    /// <summary>
    /// Summary of an incident for listings.
    /// </summary>
    public class IncidentSummary
    {
        public string IncidentKey { get; set; }
        public int IncidentId { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Filter for querying incidents.
    /// </summary>
    public class IncidentFilter
    {
        public DateTime? Since { get; set; }
        public DateTime? Until { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public int? Limit { get; set; } = 50;
    }

    /// <summary>
    /// Response wrapper from Alerting API incident query.
    /// </summary>
    public class IncidentQueryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<IncidentSummary> Incidents { get; set; }
    }

    #endregion

    #region Registration DTOs

    public class RegistrationResponse
    {
        public Guid IntegrationGuid { get; set; }
        public Guid ServiceGuid { get; set; }
        public string ServiceName { get; set; }
        public string ApiKey { get; set; }
        public string Message { get; set; }
    }

    #endregion

    #region Webhook DTOs

    /// <summary>
    /// Payload sent by Alerting when incident state changes.
    /// </summary>
    public class IncidentWebhookPayload
    {
        public string EventType { get; set; }
        public DateTime EventTime { get; set; }
        public string IncidentKey { get; set; }
        public int IncidentId { get; set; }
        public string Status { get; set; }
        public string PreviousStatus { get; set; }
        public string Severity { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ServiceName { get; set; }
        public string ActionBy { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
    }

    #endregion
}
