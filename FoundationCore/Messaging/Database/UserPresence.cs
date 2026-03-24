using System;

namespace Foundation.Messaging.Database
{
    public partial class UserPresence
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int userId { get; set; }
        public string status { get; set; }
        public string customStatusMessage { get; set; }
        public DateTime lastSeenDateTime { get; set; }
        public DateTime lastActivityDateTime { get; set; }
        public int connectionCount { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }
    }
}
