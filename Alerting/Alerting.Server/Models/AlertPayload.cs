//
// Alert Payload Model
//
// Represents the inbound payload for triggering an alert via the API.
//
using System;

namespace Alerting.Server.Models
{
    /// <summary>
    /// Payload for triggering an alert via the REST API.
    /// </summary>
    public class AlertPayload
    {
        /// <summary>
        /// Unique key for this incident (used for de-duplication).
        /// If an open incident with this key exists, it will be updated rather than creating a new one.
        /// </summary>
        public string IncidentKey { get; set; }

        /// <summary>
        /// Short summary of the incident (e.g., "Database connection timeout").
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Detailed description of the incident.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Severity level: Critical, High, Medium, Low. Defaults to High if not specified.
        /// </summary>
        public string Severity { get; set; } = "High";

        /// <summary>
        /// The source system's payload as JSON for reference (stored for forensics).
        /// </summary>
        public string SourcePayloadJson { get; set; }
    }

    /// <summary>
    /// Response from triggering an alert.
    /// </summary>
    public class AlertResponse
    {
        /// <summary>
        /// Whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The incident key (either provided or generated).
        /// </summary>
        public string IncidentKey { get; set; }

        /// <summary>
        /// The incident ID if created/updated.
        /// </summary>
        public int? IncidentId { get; set; }

        /// <summary>
        /// Human-readable message about the result.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Whether this was a new incident or an update to an existing one.
        /// </summary>
        public bool IsNew { get; set; }
    }
}
