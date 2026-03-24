using System;

namespace Foundation.Messaging.Database
{
    public partial class ConversationMessageChangeHistory
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int conversationMessageId { get; set; }
        public int versionNumber { get; set; }
        public DateTime timeStamp { get; set; }
        public int userId { get; set; }
        public string data { get; set; }

        public virtual ConversationMessage conversationMessage { get; set; }
    }
}
