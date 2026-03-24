using System;

namespace Foundation.Messaging.Database
{
    /// <summary>
    /// Tracks every external push delivery attempt (email, SMS, etc.).
    /// Used for delivery history, retry tracking, and administrative reporting.
    /// </summary>
    public partial class PushDeliveryLog
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int userId { get; set; }
        public string providerId { get; set; }
        public string destination { get; set; }
        public string sourceType { get; set; }
        public int? sourceNotificationId { get; set; }
        public int? sourceConversationMessageId { get; set; }
        public bool success { get; set; }
        public string externalId { get; set; }
        public string errorMessage { get; set; }
        public int attemptNumber { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public Guid objectGuid { get; set; }
        public bool active { get; set; }
        public bool deleted { get; set; }
    }
}
