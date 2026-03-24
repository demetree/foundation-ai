using System;
using System.Collections.Generic;

namespace Foundation.Messaging.Database
{
    public partial class ConversationMessageAttachment
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int conversationMessageId { get; set; }
        public int userId { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public string contentFileName { get; set; }
        public long contentSize { get; set; }
        public byte[] contentData { get; set; }
        public string contentMimeType { get; set; }
        public int versionNumber { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual ICollection<ConversationMessageAttachmentChangeHistory> ConversationMessageAttachmentChangeHistories { get; set; } = new List<ConversationMessageAttachmentChangeHistory>();
        public virtual ConversationMessage conversationMessage { get; set; }
    }
}
