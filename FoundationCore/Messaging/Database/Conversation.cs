using System;
using System.Collections.Generic;

namespace Foundation.Messaging.Database
{
    public partial class Conversation
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int? createdByUserId { get; set; }
        public int? conversationTypeId { get; set; }
        public int priority { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public string entity { get; set; }
        public int? entityId { get; set; }
        public string externalURL { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool? isPublic { get; set; }

        public int? userId { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual ICollection<ConversationChannel> ConversationChannels { get; set; } = new List<ConversationChannel>();
        public virtual ICollection<ConversationMessage> ConversationMessages { get; set; } = new List<ConversationMessage>();
        public virtual ICollection<ConversationPin> ConversationPins { get; set; } = new List<ConversationPin>();
        public virtual ICollection<ConversationUser> ConversationUsers { get; set; } = new List<ConversationUser>();
        public virtual ConversationType conversationType { get; set; }
    }
}
