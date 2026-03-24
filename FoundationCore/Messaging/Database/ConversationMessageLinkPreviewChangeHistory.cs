using System;

namespace Foundation.Messaging.Database
{
    public partial class ConversationMessageLinkPreviewChangeHistory
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int conversationMessageLinkPreviewId { get; set; }
        public int versionNumber { get; set; }
        public DateTime timeStamp { get; set; }
        public int userId { get; set; }
        public string data { get; set; }

        public virtual ConversationMessageLinkPreview conversationMessageLinkPreview { get; set; }
    }
}
