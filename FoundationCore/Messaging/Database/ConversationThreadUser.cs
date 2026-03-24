using System;

namespace Foundation.Messaging.Database
{
    /// <summary>
    /// Tracks a user's last-read position within a message thread (reply chain).
    /// Enables per-thread unread badges in the UI.
    /// </summary>
    public partial class ConversationThreadUser
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int conversationId { get; set; }
        public int parentConversationMessageId { get; set; }
        public int userId { get; set; }
        public int? lastReadMessageId { get; set; }
        public DateTime? lastReadDateTime { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }
    }
}
