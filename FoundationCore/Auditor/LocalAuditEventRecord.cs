//
// LocalAuditEventRecord
//
// Flat POCO for persisting audit events in the local SQLite buffer.
// Each row captures a complete AuditEngine.EventDetails snapshot that
// will later be flushed to the Auditor SQL Server database.
//
// AI-assisted development - February 2026
//
using System;
using System.Collections.Generic;

namespace Foundation.Auditor
{
    /// <summary>
    /// Local representation of an audit event for the IndexedDB write-ahead buffer.
    /// Mirrors <see cref="AuditEngine.EventDetails"/> in a flat, serialisable form.
    /// </summary>
    public class LocalAuditEventRecord
    {
        public int Id { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime StopTime { get; set; }

        public bool CompletedSuccessfully { get; set; }

        /// <summary>
        /// Numeric value of <see cref="AuditEngine.AuditAccessType"/>.
        /// </summary>
        public int AccessType { get; set; }

        /// <summary>
        /// Numeric value of <see cref="AuditEngine.AuditType"/>.
        /// </summary>
        public int AuditType { get; set; }

        public string User { get; set; }

        public string Session { get; set; }

        public string Source { get; set; }

        public string UserAgent { get; set; }

        public string Module { get; set; }

        public string ModuleEntity { get; set; }

        public string Resource { get; set; }

        public string HostSystem { get; set; }

        public string PrimaryKey { get; set; }

        public int? ThreadId { get; set; }

        public string Message { get; set; }

        public string EntityBeforeState { get; set; }

        public string EntityAfterState { get; set; }

        /// <summary>
        /// JSON-serialized <c>List&lt;string&gt;</c> of error messages.
        /// Null when there are no errors.
        /// </summary>
        public string ErrorMessagesJson { get; set; }

        /// <summary>
        /// UTC timestamp of when this event was buffered locally.
        /// Used for ordering during flush and diagnostics.
        /// </summary>
        public DateTime BufferedAtUtc { get; set; }
    }
}
