using System;
using System.Collections.Generic;

namespace Alerting.Server.Models
{
    /// <summary>
    /// DTO for Service Health Matrix dashboard view.
    /// Provides at-a-glance health status for each service.
    /// </summary>
    public class ServiceHealthDto
    {
        public int ServiceId { get; set; }
        public Guid ServiceObjectGuid { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        
        /// <summary>
        /// Health status: "Healthy", "Degraded", or "Critical"
        /// </summary>
        public string Status { get; set; } = "Healthy";
        
        /// <summary>
        /// Total active (unresolved) incidents for this service.
        /// </summary>
        public int ActiveIncidentCount { get; set; }
        
        /// <summary>
        /// Count of Critical severity incidents.
        /// </summary>
        public int CriticalCount { get; set; }
        
        /// <summary>
        /// Count of High severity incidents.
        /// </summary>
        public int HighCount { get; set; }
        
        /// <summary>
        /// Count of Medium severity incidents.
        /// </summary>
        public int MediumCount { get; set; }
        
        /// <summary>
        /// Name of the assigned escalation policy, or null if not assigned.
        /// </summary>
        public string? EscalationPolicyName { get; set; }
        
        /// <summary>
        /// List of on-call user objectGuids for frontend resolution.
        /// </summary>
        public List<Guid> OnCallUserGuids { get; set; } = new();
    }
}
