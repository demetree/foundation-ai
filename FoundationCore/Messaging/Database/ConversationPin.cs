using System;

namespace Foundation.Messaging.Database
{
    public partial class ConversationPin
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int conversationId { get; set; }
        public int conversationMessageId { get; set; }
        public int pinnedByUserId { get; set; }
        public DateTime dateTimePinned { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual Conversation conversation { get; set; }
        public virtual ConversationMessage conversationMessage { get; set; }
    }
}
