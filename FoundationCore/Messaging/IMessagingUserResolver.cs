using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foundation.Messaging
{

    /// <summary>
    /// 
    /// Represents a user for messaging purposes.
    /// This is a Foundation-level DTO that decouples messaging services from module-specific User entities.
    /// 
    /// </summary>
    public class MessagingUser
    {
        public int id { get; set; }
        public string accountName { get; set; }
        public string displayName { get; set; }
        public Guid objectGuid { get; set; }
        public Guid tenantGuid { get; set; }
    }


    /// <summary>
    /// 
    /// Interface for resolving user information within the messaging system.
    /// 
    /// Each Foundation module implements this interface to bridge between its own User entity
    /// and the Foundation-level MessagingUser.  For example, Catalyst wraps its existing 
    /// NotificationService.GetUserAsync calls, while Basecamp would query its own User table.
    /// 
    /// </summary>
    public interface IMessagingUserResolver
    {
        /// <summary>
        /// Resolves a MessagingUser from a SecurityUser (the authenticated user).
        /// </summary>
        Task<MessagingUser> GetUserAsync(Foundation.Security.Database.SecurityUser securityUser);

        /// <summary>
        /// Resolves a MessagingUser by account name and tenant GUID.
        /// </summary>
        Task<MessagingUser> GetUserByAccountNameAsync(string accountName, Guid tenantGuid);

        /// <summary>
        /// Resolves a MessagingUser by user ID and tenant GUID.
        /// </summary>
        Task<MessagingUser> GetUserByIdAsync(int userId, Guid tenantGuid);

        /// <summary>
        /// Resolves multiple MessagingUsers by their IDs in a single batch query.
        /// </summary>
        Task<List<MessagingUser>> GetUsersByIdsAsync(List<int> userIds, Guid tenantGuid);

        /// <summary>
        /// Resolves a SecurityUser from a MessagingUser ID and tenant GUID.
        /// Used by PushDeliveryService to read user notification settings.
        /// </summary>
        Task<Foundation.Security.Database.SecurityUser> GetSecurityUserByMessagingUserIdAsync(int messagingUserId, Guid tenantGuid);

        /// <summary>
        /// Returns all active users in the specified tenant.
        /// Used by the People Directory panel to show all users with their presence status.
        /// </summary>
        Task<List<MessagingUser>> GetAllUsersAsync(Guid tenantGuid);

        /// <summary>
        /// Searches for users by name or account name within a tenant.
        /// </summary>
        Task<List<MessagingUser>> SearchUsersAsync(string searchTerm, Guid tenantGuid, int maxResults = 20);
    }
}
