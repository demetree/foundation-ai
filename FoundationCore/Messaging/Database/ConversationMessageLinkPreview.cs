using System;
using System.Collections.Generic;

namespace Foundation.Messaging.Database
{
    public partial class ConversationMessageLinkPreview
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int conversationMessageId { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string imageUrl { get; set; }
        public string siteName { get; set; }
        public DateTime fetchedDateTime { get; set; }
        public int versionNumber { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual ConversationMessage conversationMessage { get; set; }
        public virtual ICollection<ConversationMessageLinkPreviewChangeHistory> ConversationMessageLinkPreviewChangeHistories { get; set; }
    }
}
