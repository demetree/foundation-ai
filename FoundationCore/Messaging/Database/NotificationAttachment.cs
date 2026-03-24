using System;

namespace Foundation.Messaging.Database
{
    /// <summary>
    ///
    /// Foundation-level NotificationAttachment entity.
    ///
    /// Represents a file attached to a notification. The contentData byte array field 
    /// can hold inline binary data, or be left null when using an IAttachmentStorageProvider
    /// that stores content externally (identified by objectGuid).
    ///
    /// AI-developed as part of Foundation.Messaging refactoring, March 2026.
    ///
    /// </summary>
    public partial class NotificationAttachment
    {
        public int id { get; set; }
        public Guid tenantGuid { get; set; }
        public int notificationId { get; set; }
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

        public virtual Notification notification { get; set; }
    }
}
