using System;


namespace Foundation.Community.Middleware
{
    /// <summary>
    /// 
    /// Scoped service that holds the resolved tenant information for the current HTTP request.
    /// 
    /// Populated by TenantResolutionMiddleware early in the pipeline, then injected
    /// into controllers and services that need to scope queries by tenant.
    /// 
    /// </summary>
    public class TenantContext
    {
        /// <summary>
        /// The objectGuid of the resolved SecurityTenant.
        /// This is the value used to filter data in tenant-scoped tables (tenantGuid FK).
        /// </summary>
        public Guid? TenantGuid { get; set; }

        /// <summary>
        /// The SecurityTenant record ID (primary key).
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// The tenant's display name (from SecurityTenant.name).
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// Whether a tenant was successfully resolved for this request.
        /// </summary>
        public bool IsResolved => TenantGuid.HasValue;
    }
}
