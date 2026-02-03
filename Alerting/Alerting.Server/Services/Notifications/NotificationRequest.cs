//
// Notification Request Model
//
// Contains all information needed to send a notification to a user about an incident.
//
using System;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Encapsulates all information needed to send a notification.
    /// </summary>
    public class NotificationRequest
    {
        /// <summary>
        /// The user's security object GUID (from Security.SecurityUser).
        /// </summary>
        public Guid UserObjectGuid { get; set; }

        /// <summary>
        /// User's email address for email notifications.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// User's phone number in E.164 format for SMS/Voice.
        /// </summary>
        public string UserPhoneNumber { get; set; }

        /// <summary>
        /// Firebase Cloud Messaging token for push notifications.
        /// </summary>
        public string PushToken { get; set; }

        /// <summary>
        /// User's display name for personalization.
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// The incident being notified about.
        /// </summary>
        public IncidentInfo Incident { get; set; }

        /// <summary>
        /// The tenant GUID for multi-tenant context.
        /// </summary>
        public Guid TenantGuid { get; set; }
    }

    /// <summary>
    /// Incident information for notification content.
    /// </summary>
    public class IncidentInfo
    {
        public int Id { get; set; }
        public string IncidentKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SeverityName { get; set; }
        public int SeverityId { get; set; }
        public string ServiceName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StatusName { get; set; }
    }
}
