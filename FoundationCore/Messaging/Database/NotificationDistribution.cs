using System;

namespace Foundation.Messaging.Database
{
    /// <summary>
    ///
    /// Foundation-level NotificationDistribution entity.
    ///
    /// Represents a per-user delivery record for a notification.  Each user who should 
    /// receive a notification gets their own distribution record, enabling individual 
    /// acknowledgement tracking.
    ///
    /// The userId field references a module-specific user and is resolved through 
    /// IMessagingUserResolver at the service layer.
    ///
    /// AI-developed as part of Foundation.Messaging refactoring, March 2026.
    ///
    /// </summary>
    public partial class NotificationDistribution
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int notificationId { get; set; }
        public int userId { get; set; }
        public bool acknowledged { get; set; }
        public DateTime? dateTimeAcknowledged { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual Notification notification { get; set; }
    }
}
