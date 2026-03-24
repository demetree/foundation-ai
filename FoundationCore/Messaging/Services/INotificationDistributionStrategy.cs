using Foundation.Security.Database;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Defines a pluggable strategy for distributing notifications to recipients.
    /// 
    /// Modules implement this interface to provide custom distribution patterns:
    ///   - Catalyst: distribute to all users in a tenant, organization, or department
    ///   - Basecamp: distribute to project contacts, shift supervisors, etc.
    /// 
    /// The NotificationService delegates recipient resolution to the strategy,
    /// while keeping the core create/persist/broadcast pipeline in Foundation.
    /// 
    /// </summary>
    public interface INotificationDistributionStrategy
    {
        /// <summary>
        /// Resolves the list of messaging user IDs that should receive a notification
        /// based on the distribution context.
        /// 
        /// Parameters:
        ///   securityUser - The user sending the notification
        ///   context      - Module-specific distribution metadata (e.g., org ID, dept ID, project ID)
        /// 
        /// Returns the list of user IDs to distribute to.
        /// </summary>
        Task<List<int>> ResolveRecipientsAsync(SecurityUser securityUser, NotificationDistributionContext context);


        /// <summary>
        /// Optional post-distribution hook for module-specific side effects.
        /// For example, logging to a module-specific audit table, sending webhook calls,
        /// or triggering module-specific in-app events.
        /// 
        /// Default implementation does nothing.
        /// </summary>
        Task OnDistributionCompleteAsync(SecurityUser securityUser, int notificationId, List<int> recipientUserIds)
        {
            return Task.CompletedTask;
        }
    }


    /// <summary>
    /// Context object passed to INotificationDistributionStrategy.ResolveRecipientsAsync().
    /// Contains module-agnostic fields plus an extensible metadata dictionary for module-specific data.
    /// </summary>
    public class NotificationDistributionContext
    {
        /// <summary>
        /// The type of distribution: "Direct", "Organization", "Department", "Team", "Broadcast", "Custom".
        /// </summary>
        public string DistributionType { get; set; } = "Direct";

        /// <summary>
        /// Explicit list of user IDs for "Direct" distribution.
        /// When set, the strategy can simply return this list.
        /// </summary>
        public List<int> ExplicitRecipientUserIds { get; set; }

        /// <summary>
        /// Module-specific scope identifier (e.g., organization ID, project ID).
        /// Interpretation depends on DistributionType and the module's strategy implementation.
        /// </summary>
        public int? ScopeId { get; set; }

        /// <summary>
        /// Optional secondary scope (e.g., department ID within an organization).
        /// </summary>
        public int? SecondaryScopeId { get; set; }

        /// <summary>
        /// Extensible key-value metadata for module-specific distribution parameters.
        /// For example, Basecamp might include { "projectId": "123", "role": "Supervisor" }.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }


    /// <summary>
    /// Default distribution strategy that simply returns the explicit recipient list.
    /// Used when no module-specific strategy is registered.
    /// </summary>
    public class DefaultNotificationDistributionStrategy : INotificationDistributionStrategy
    {
        public Task<List<int>> ResolveRecipientsAsync(SecurityUser securityUser, NotificationDistributionContext context)
        {
            return Task.FromResult(context.ExplicitRecipientUserIds ?? new List<int>());
        }
    }
}
