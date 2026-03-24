using System;
using System.Collections.Generic;

namespace Foundation.Messaging.Database
{
    /// <summary>
    ///
    /// Foundation-level NotificationType entity.
    ///
    /// Defines categories of notifications (e.g., "System", "DirectMessage", "Alert").
    /// Each notification can optionally reference a type for filtering and display.
    ///
    /// AI-developed as part of Foundation.Messaging refactoring, March 2026.
    ///
    /// </summary>
    public partial class NotificationType
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
