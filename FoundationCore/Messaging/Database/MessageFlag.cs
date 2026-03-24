using System;

namespace Foundation.Messaging.Database
{
    /// <summary>
    /// Tracks flags/reports on messages (abuse, inappropriate content, etc.)
    /// for administrative review.
    /// </summary>
    public partial class MessageFlag
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int conversationMessageId { get; set; }
        public int flaggedByUserId { get; set; }
        public string reason { get; set; }
        public string details { get; set; }
        public string status { get; set; }
        public int? reviewedByUserId { get; set; }
        public DateTime? dateTimeReviewed { get; set; }
        public string resolutionNotes { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }
    }
}
