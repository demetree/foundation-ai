using System;

namespace Foundation.Messaging.Database
{
    /// <summary>
    /// Per-tenant configuration for push delivery providers.
    /// Stores provider-specific settings (SMTP credentials, API keys, etc.) as JSON.
    /// </summary>
    public partial class PushProviderConfiguration
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public string providerId { get; set; }
        public bool enabled { get; set; }
        public string configurationJson { get; set; }
        public DateTime dateTimeModified { get; set; }
        public int modifiedByUserId { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }
    }
}
