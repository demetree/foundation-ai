using System;
using System.Collections.Generic;

namespace Foundation.Messaging.Database
{
    public partial class ConversationType
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }

        public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}
