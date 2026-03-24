using System;

namespace Foundation.Messaging.Database
{
    public partial class ConversationMessageUser
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int conversationMessageId { get; set; }
        public int userId { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public bool acknowledged { get; set; }
        public DateTime dateTimeAcknowledged { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual ConversationMessage conversationMessage { get; set; }
    }
}
