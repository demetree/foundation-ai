//
// WebDavContext.cs
//
// Per-request context that carries the authenticated user and tenant GUID
// through the WebDAV middleware pipeline.
//
using System;
using Foundation.Security.Database;

namespace Scheduler.WebDAV.Services
{
    /// <summary>
    /// Encapsulates the authenticated identity for the current WebDAV request.
    /// Stored in HttpContext.Items by BasicAuthMiddleware and consumed by WebDAV handlers.
    /// </summary>
    public class WebDavContext
    {
        /// <summary>
        /// The HttpContext.Items key used to store/retrieve this object.
        /// </summary>
        public const string CONTEXT_KEY = "WebDavContext";

        /// <summary>
        /// The authenticated Foundation SecurityUser.
        /// </summary>
        public SecurityUser User { get; set; }

        /// <summary>
        /// The tenant GUID resolved from the user's SecurityTenant.
        /// All file operations are scoped to this tenant.
        /// </summary>
        public Guid TenantGuid { get; set; }

        /// <summary>
        /// The SecurityUser.id — needed for audit trail on write operations.
        /// </summary>
        public int SecurityUserId { get; set; }
    }
}
