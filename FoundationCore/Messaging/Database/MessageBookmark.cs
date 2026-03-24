using System;

namespace Foundation.Messaging.Database
{
    /// <summary>
    /// A personal message bookmark. Allows users to save messages for later reference
    /// with an optional note explaining why it was bookmarked.
    /// </summary>
    public partial class MessageBookmark
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int userId { get; set; }
        public int conversationMessageId { get; set; }
        public string note { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual ConversationMessage conversationMessage { get; set; }
    }
}
