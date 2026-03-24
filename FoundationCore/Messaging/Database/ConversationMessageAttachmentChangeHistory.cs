using System;

namespace Foundation.Messaging.Database
{
    public partial class ConversationMessageAttachmentChangeHistory
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int conversationMessageAttachmentId { get; set; }
        public int versionNumber { get; set; }
        public DateTime timeStamp { get; set; }
        public int userId { get; set; }
        public string data { get; set; }

        public virtual ConversationMessageAttachment conversationMessageAttachment { get; set; }
    }
}
