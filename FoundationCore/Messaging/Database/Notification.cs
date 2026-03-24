using System;
using System.Collections.Generic;

namespace Foundation.Messaging.Database
{
    /// <summary>
    ///
    /// Foundation-level Notification entity.
    ///
    /// Mirrors the Catalyst Notification table but decoupled from module-specific entities.
    /// User references are resolved through IMessagingUserResolver at the service layer.
    ///
    /// AI-developed as part of Foundation.Messaging refactoring, March 2026.
    ///
    /// </summary>
    public partial class Notification
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int? notificationTypeId { get; set; }
        public int? createdByUserId { get; set; }
        public string message { get; set; }
        public int priority { get; set; }
        public string entity { get; set; }
        public int? entityId { get; set; }
        public string externalURL { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public DateTime? dateTimeDistributed { get; set; }
        public bool distributionCompleted { get; set; }

        public int? userId { get; set; }
        public int versionNumber { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual ICollection<NotificationAttachment> NotificationAttachments { get; set; } = new List<NotificationAttachment>();
        public virtual ICollection<NotificationDistribution> NotificationDistributions { get; set; } = new List<NotificationDistribution>();
        public virtual NotificationType notificationType { get; set; }
    }
}
