using System;

namespace Foundation.Messaging.Database
{
    public partial class ConversationUser
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int conversationId { get; set; }
        public int userId { get; set; }
        public string role { get; set; } = "Member";
        public DateTime dateTimeAdded { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual Conversation conversation { get; set; }
    }
}
