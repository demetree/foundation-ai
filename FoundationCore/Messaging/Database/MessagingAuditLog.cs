using System;

namespace Foundation.Messaging.Database
{
    /// <summary>
    /// Tracks significant administrative actions on the messaging system
    /// (message deletions, user bans, flag resolutions, etc.).
    /// </summary>
    public partial class MessagingAuditLog
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int performedByUserId { get; set; }
        public string action { get; set; }
        public string entityType { get; set; }
        public int? entityId { get; set; }
        public string details { get; set; }
        public string ipAddress { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }
    }
}
