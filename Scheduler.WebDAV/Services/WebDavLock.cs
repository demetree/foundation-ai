//
// WebDavLock.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Data model for a WebDAV lock record, stored in Foundation.IndexedDB (SQLite).
//
using System;

namespace Scheduler.WebDAV.Services
{
    /// <summary>
    /// Represents a single WebDAV lock on a document.
    /// Stored in a Foundation.IndexedDB SQLite database for persistence across restarts.
    /// </summary>
    public class WebDavLock
    {
        /// <summary>Auto-increment primary key (internal).</summary>
        public long Id { get; set; }

        /// <summary>Opaque lock token URI (e.g., "urn:uuid:&lt;Guid&gt;").</summary>
        public string LockToken { get; set; }

        /// <summary>The database ID of the locked document.</summary>
        public int DocumentId { get; set; }

        /// <summary>Tenant GUID — ensures lock isolation between tenants.</summary>
        public Guid TenantGuid { get; set; }

        /// <summary>The username who owns the lock.</summary>
        public string Owner { get; set; }

        /// <summary>Lock depth: 0 for the resource only, -1 for infinity.</summary>
        public int Depth { get; set; }

        /// <summary>Lock scope: "exclusive" or "shared".</summary>
        public string LockScope { get; set; } = "exclusive";

        /// <summary>Lock type — always "write" per RFC 4918.</summary>
        public string LockType { get; set; } = "write";

        /// <summary>When this lock expires and should be automatically cleaned up.</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>When the lock was originally created.</summary>
        public DateTime CreatedAt { get; set; }
    }
}
