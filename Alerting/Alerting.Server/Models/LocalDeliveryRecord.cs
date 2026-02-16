//
// Local Delivery Record
//
// POCO for the IndexedDB local audit buffer. Flat structure optimized for
// JSON serialization — no EF navigation properties.
//
// AI-assisted development - February 2026
//
using System;

namespace Alerting.Server.Models
{
    /// <summary>
    /// Represents a notification delivery attempt stored in the local IndexedDB buffer.
    /// This is the serialization shape written to and read from the local SQLite store.
    /// </summary>
    public class LocalDeliveryRecord
    {
        /// <summary>
        /// Auto-incremented primary key in the local store.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique correlation ID mapping to the server-side objectGuid.
        /// Used to deduplicate when flushing to SQL Server.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Tenant context for multi-tenant isolation.
        /// </summary>
        public string TenantGuid { get; set; }

        /// <summary>
        /// Foreign key to the parent IncidentNotification (server-side ID).
        /// </summary>
        public int IncidentNotificationId { get; set; }

        /// <summary>
        /// Channel type ID (Email=1, SMS=2, Voice=3, Teams=4, Push=5).
        /// </summary>
        public int ChannelTypeId { get; set; }

        /// <summary>
        /// Attempt sequence number.
        /// </summary>
        public int AttemptNumber { get; set; }

        /// <summary>
        /// When the delivery was attempted (UTC).
        /// </summary>
        public DateTime AttemptedAt { get; set; }

        /// <summary>
        /// Current status: Pending, Sent, or Failed.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Error message if the delivery failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Provider response or external message ID.
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// Recipient address (email, phone number, etc.).
        /// </summary>
        public string RecipientAddress { get; set; }

        /// <summary>
        /// Notification subject line.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Full body content for forensic auditing.
        /// </summary>
        public string BodyContent { get; set; }

        /// <summary>
        /// Whether this record has been flushed to the central SQL Server.
        /// </summary>
        public bool FlushedToServer { get; set; }
    }
}
