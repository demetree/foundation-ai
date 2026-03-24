using Foundation.Cache;
using Foundation.Messaging.Database;
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Foundation Notification Service - provides business logic for the notification system.
    /// 
    /// This is a Foundation-level service that can be used by any module. It handles:
    /// - Notification creation and distribution to specific users
    /// - Notification retrieval with filtering
    /// - Acknowledgement tracking
    /// - Notification type management
    /// - Notification attachments
    /// 
    /// Module-specific distribution patterns (e.g., notify all users in an organization,
    /// department, or team) should be implemented in module-specific subclasses or wrappers
    /// that call CreateNotificationAsync with the resolved list of user IDs.
    /// 
    /// User resolution is handled through IMessagingUserResolver, allowing each module
    /// to provide its own user lookup implementation.
    /// 
    /// AI-developed as part of Foundation.Messaging refactoring, March 2026.
    /// 
    /// </summary>
    public class NotificationService
    {
        private readonly IMessagingUserResolver _userResolver;
        private readonly PushDeliveryService _pushDeliveryService;
        private readonly INotificationDistributionStrategy _distributionStrategy;
        private readonly MemoryCacheManager _cache;

        private const float TYPE_CACHE_TTL_MINUTES = 60f;


        public NotificationService(IMessagingUserResolver userResolver, PushDeliveryService pushDeliveryService = null, INotificationDistributionStrategy distributionStrategy = null)
        {
            _userResolver = userResolver;
            _pushDeliveryService = pushDeliveryService;
            _distributionStrategy = distributionStrategy ?? new DefaultNotificationDistributionStrategy();
            _cache = new MemoryCacheManager();
        }


        #region DTOs / Summary Classes

        public class NotificationSummary
        {
            public int id { get; set; }
            public int notificationId { get; set; }
            public string message { get; set; }
            public DateTime dateTimeDistributed { get; set; }
            public string entity { get; set; }
            public int? entityId { get; set; }
            public string externalURL { get; set; }
            public bool acknowledged { get; set; }
            public int? notificationTypeId { get; set; }
            public string notificationType { get; set; }
            public int? sendingUserId { get; set; }
            public string sendingUserName { get; set; }
            public List<NotificationAttachmentSummary> attachments { get; set; }
        }

        public class NotificationAttachmentSummary
        {
            public int attachmentId { get; set; }
            public Guid objectGuid { get; set; }
            public string fileName { get; set; }
            public string mimeType { get; set; }
            public long contentLength { get; set; }
        }

        public class NotificationTypeInfo
        {
            public int id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }

        #endregion


        #region Notification Retrieval

        /// <summary>
        /// Gets notifications for the current user, optionally filtered by acknowledgement status and date range.
        /// </summary>
        public async Task<List<NotificationSummary>> GetNotificationsForUserAsync(SecurityUser securityUser, bool unacknowledgedOnly = true, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                if (user == null)
                {
                    return null;
                }

                //
                // Build the base query for notification distributions belonging to the current user.
                //
                bool? acknowledged = null;

                if (unacknowledgedOnly == true)
                {
                    acknowledged = false;
                }

                DateTime defaultDate = DateTime.UtcNow;

                List<NotificationSummary> notifications = await (from nd in db.NotificationDistributions
                                                                 join n in db.Notifications on nd.notificationId equals n.id
                                                                 where
                                                                 nd.userId == user.id &&
                                                                 (acknowledged.HasValue == false || nd.acknowledged == acknowledged) &&
                                                                 nd.active == true &&
                                                                 nd.deleted == false &&
                                                                 n.active == true &&
                                                                 n.deleted == false &&
                                                                 n.distributionCompleted == true &&
                                                                 (startDate.HasValue == false || n.dateTimeDistributed >= startDate.Value) &&
                                                                 (endDate.HasValue == false || n.dateTimeDistributed <= endDate.Value)
                                                                 orderby n.priority, n.dateTimeDistributed
                                                                 select new NotificationSummary
                                                                 {
                                                                     id = nd.id,
                                                                     notificationId = n.id,
                                                                     message = n.message,
                                                                     dateTimeDistributed = n.dateTimeDistributed.HasValue == true ? n.dateTimeDistributed.Value : defaultDate,
                                                                     entity = n.entity,
                                                                     entityId = n.entityId,
                                                                     externalURL = n.externalURL,
                                                                     acknowledged = nd.acknowledged,
                                                                     notificationTypeId = n.notificationTypeId,
                                                                     sendingUserId = n.createdByUserId,
                                                                 })
                                                                 .AsNoTracking()
                                                                 .ToListAsync();


                //
                // Load attachments for each notification.
                //
                foreach (NotificationSummary notification in notifications)
                {
                    notification.attachments = await (from x in db.NotificationAttachments
                                                      where
                                                      x.notificationId == notification.notificationId &&
                                                      x.active == true &&
                                                      x.deleted == false
                                                      select new NotificationAttachmentSummary
                                                      {
                                                          attachmentId = x.id,
                                                          objectGuid = x.objectGuid,
                                                          fileName = x.contentFileName,
                                                          mimeType = x.contentMimeType,
                                                          contentLength = x.contentSize
                                                      })
                                                      .AsNoTracking()
                                                      .ToListAsync();
                }


                //
                // Resolve notification type names.
                //
                Dictionary<int, NotificationType> notificationTypes = await (from x in db.NotificationTypes
                                                                              where x.active == true &&
                                                                              x.deleted == false
                                                                              select x)
                                                                             .AsNoTracking()
                                                                             .ToDictionaryAsync(x => x.id, x => x);

                foreach (NotificationSummary ns in notifications)
                {
                    if (ns.notificationTypeId.HasValue == true &&
                        notificationTypes.ContainsKey(ns.notificationTypeId.Value) == true)
                    {
                        ns.notificationType = notificationTypes[ns.notificationTypeId.Value].name;
                    }
                }


                //
                // Resolve sending user display names through the user resolver.
                //
                List<int> sendingUserIds = (from x in notifications
                                             where x.sendingUserId.HasValue == true
                                             select x.sendingUserId.Value)
                                            .Distinct()
                                            .ToList();

                Dictionary<int, string> userDisplayNames = new Dictionary<int, string>();

                foreach (int sendingUserId in sendingUserIds)
                {
                    MessagingUser senderUser = await _userResolver.GetUserByIdAsync(sendingUserId, securityUser.securityTenant.objectGuid);

                    if (senderUser != null)
                    {
                        userDisplayNames[sendingUserId] = senderUser.displayName;
                    }
                }

                foreach (NotificationSummary ns in notifications)
                {
                    if (ns.sendingUserId.HasValue == true &&
                        userDisplayNames.ContainsKey(ns.sendingUserId.Value) == true)
                    {
                        ns.sendingUserName = userDisplayNames[ns.sendingUserId.Value];
                    }
                }


                return notifications;
            }
        }

        #endregion


        #region Notification Creation

        /// <summary>
        /// Creates a notification and distributes it to the specified list of user IDs.
        /// 
        /// This is the core Foundation method — module-specific distribution patterns 
        /// (e.g., all users in an org/dept/team) should resolve the user ID list first 
        /// and then call this method.
        /// </summary>
        public async Task<int> CreateNotificationAsync(
            SecurityUser securityUser,
            List<int> recipientUserIds,
            string message,
            string entity = null,
            int? entityId = null,
            string externalURL = null,
            string notificationType = null,
            int priority = 10,
            bool distribute = true)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            if (recipientUserIds == null || recipientUserIds.Count == 0)
            {
                throw new Exception("At least one recipient user ID is required.");
            }


            //
            // Revert to default priority if something out of range was sent.
            //
            if (priority < 0 || priority > 100)
            {
                priority = 10;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser senderUser = await _userResolver.GetUserAsync(securityUser);

                if (senderUser == null)
                {
                    throw new Exception("Could not resolve sending user.");
                }


                using (var transaction = db.Database.BeginTransaction())
                {
                    //
                    // Create the notification record.
                    //
                    Notification notification = new Notification();

                    notification.createdByUserId = senderUser.id;
                    notification.dateTimeCreated = DateTime.UtcNow;
                    notification.userId = recipientUserIds.Count == 1 ? recipientUserIds[0] : (int?)null;
                    notification.priority = priority;
                    notification.message = message;
                    notification.entity = entity;
                    notification.entityId = entityId;
                    notification.externalURL = externalURL;

                    //
                    // Resolve notification type if specified.
                    //
                    if (notificationType != null)
                    {
                        notification.notificationTypeId = await GetNotificationTypeIdAsync(db, notificationType);
                    }

                    if (distribute == true)
                    {
                        notification.distributionCompleted = true;
                        notification.dateTimeDistributed = DateTime.UtcNow;
                    }
                    else
                    {
                        notification.distributionCompleted = false;
                        notification.dateTimeDistributed = null;
                    }

                    notification.tenantGuid = securityUser.securityTenant.objectGuid;
                    notification.objectGuid = Guid.NewGuid();
                    notification.active = true;
                    notification.deleted = false;

                    db.Notifications.Add(notification);
                    await db.SaveChangesAsync();


                    //
                    // Create distribution records for each recipient if the distribute parameter is true.
                    //
                    if (distribute == true)
                    {
                        foreach (int recipientUserId in recipientUserIds)
                        {
                            NotificationDistribution notificationDistribution = new NotificationDistribution();

                            notificationDistribution.notificationId = notification.id;
                            notificationDistribution.userId = recipientUserId;
                            notificationDistribution.acknowledged = false;
                            notificationDistribution.dateTimeAcknowledged = null;
                            notificationDistribution.tenantGuid = securityUser.securityTenant.objectGuid;
                            notificationDistribution.objectGuid = Guid.NewGuid();
                            notificationDistribution.active = true;
                            notificationDistribution.deleted = false;

                            db.NotificationDistributions.Add(notificationDistribution);
                        }

                        await db.SaveChangesAsync();
                    }


                    transaction.Commit();


                    //
                    // Push delivery to external channels (email, SMS, etc.).
                    // Runs AFTER the DB transaction commits so the notification data is persisted.
                    // Each recipient is independently failable — one failure doesn't block others.
                    //
                    if (_pushDeliveryService != null && distribute == true)
                    {
                        foreach (int recipientUserId in recipientUserIds)
                        {
                            try
                            {
                                SecurityUser recipientSecurityUser = await _userResolver.GetSecurityUserByMessagingUserIdAsync(recipientUserId, securityUser.securityTenant.objectGuid);
                                if (recipientSecurityUser != null)
                                {
                                    await _pushDeliveryService.DeliverNotificationAsync(
                                        recipientSecurityUser,
                                        notification.id,
                                        message,
                                        senderUser.displayName);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Push delivery failed for user {recipientUserId}: {ex.Message}");
                            }
                        }
                    }

                    return notification.id;
                }
            }
        }


        /// <summary>
        /// Creates a notification to a single user identified by account name.
        /// Convenience wrapper around CreateNotificationAsync.
        /// </summary>
        public async Task<int> CreateNotificationToUserAsync(
            SecurityUser securityUser,
            string recipientAccountName,
            string message,
            string entity = null,
            int? entityId = null,
            string externalURL = null,
            string notificationType = null,
            int priority = 10,
            bool distribute = true)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            MessagingUser recipientUser = await _userResolver.GetUserByAccountNameAsync(recipientAccountName, securityUser.securityTenant.objectGuid);

            if (recipientUser == null)
            {
                throw new Exception($"Could not find user with account name '{recipientAccountName}'.");
            }

            return await CreateNotificationAsync(securityUser, new List<int> { recipientUser.id }, message, entity, entityId, externalURL, notificationType, priority, distribute);
        }


        /// <summary>
        /// Creates a notification using the registered distribution strategy.
        /// 
        /// The strategy resolves recipients from the NotificationDistributionContext,
        /// which can represent module-specific scopes like organizations, departments, 
        /// or project teams.
        /// </summary>
        public async Task<int> CreateNotificationWithStrategyAsync(
            SecurityUser securityUser,
            NotificationDistributionContext context,
            string message,
            string entity = null,
            int? entityId = null,
            string externalURL = null,
            string notificationType = null,
            int priority = 10)
        {
            List<int> recipientUserIds = await _distributionStrategy.ResolveRecipientsAsync(securityUser, context);
            int notificationId = await CreateNotificationAsync(securityUser, recipientUserIds, message, entity, entityId, externalURL, notificationType, priority);
            await _distributionStrategy.OnDistributionCompleteAsync(securityUser, notificationId, recipientUserIds);
            return notificationId;
        }

        #endregion


        #region Acknowledgement

        /// <summary>
        /// Acknowledges a notification distribution for the current user.
        /// </summary>
        public async Task<bool> AcknowledgeNotificationAsync(SecurityUser securityUser, int notificationDistributionId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                if (user == null)
                {
                    throw new Exception("Could not resolve user.");
                }


                NotificationDistribution nd = await (from x in db.NotificationDistributions
                                                      where
                                                      x.id == notificationDistributionId &&
                                                      x.userId == user.id &&
                                                      x.active == true &&
                                                      x.deleted == false &&
                                                      x.tenantGuid == securityUser.securityTenant.objectGuid
                                                      select x)
                                                     .FirstOrDefaultAsync();

                if (nd == null)
                {
                    throw new Exception("Could not find notification distribution.");
                }

                nd.acknowledged = true;
                nd.dateTimeAcknowledged = DateTime.UtcNow;

                await db.SaveChangesAsync();

                return true;
            }
        }


        /// <summary>
        /// Acknowledges all unacknowledged notifications for the current user.
        /// </summary>
        public async Task<int> AcknowledgeAllNotificationsAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                if (user == null)
                {
                    throw new Exception("Could not resolve user.");
                }


                List<NotificationDistribution> unacknowledgedDistributions = await (from x in db.NotificationDistributions
                                                                                     where
                                                                                     x.userId == user.id &&
                                                                                     x.acknowledged == false &&
                                                                                     x.active == true &&
                                                                                     x.deleted == false &&
                                                                                     x.tenantGuid == securityUser.securityTenant.objectGuid
                                                                                     select x)
                                                                                    .ToListAsync();

                DateTime now = DateTime.UtcNow;

                foreach (NotificationDistribution nd in unacknowledgedDistributions)
                {
                    nd.acknowledged = true;
                    nd.dateTimeAcknowledged = now;
                }

                await db.SaveChangesAsync();

                return unacknowledgedDistributions.Count;
            }
        }

        #endregion


        #region Notification Types

        /// <summary>
        /// Gets all active notification types.
        /// </summary>
        public async Task<List<NotificationTypeInfo>> GetNotificationTypesAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                return await (from x in db.NotificationTypes
                              where x.active == true &&
                              x.deleted == false
                              select new NotificationTypeInfo
                              {
                                  id = x.id,
                                  name = x.name,
                                  description = x.description,
                              })
                             .AsNoTracking()
                             .ToListAsync();
            }
        }

        #endregion


        #region Unread Count

        /// <summary>
        /// Gets the count of unacknowledged notifications for the current user.
        /// </summary>
        public async Task<int> GetUnacknowledgedCountAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return 0;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                if (user == null)
                {
                    return 0;
                }


                return await (from nd in db.NotificationDistributions
                              join n in db.Notifications on nd.notificationId equals n.id
                              where
                              nd.userId == user.id &&
                              nd.acknowledged == false &&
                              nd.active == true &&
                              nd.deleted == false &&
                              n.active == true &&
                              n.deleted == false &&
                              n.distributionCompleted == true
                              select nd)
                             .CountAsync();
            }
        }

        #endregion


        #region Private Helpers

        /// <summary>
        /// Resolves a notification type ID from its name.
        /// Results are cached for 60 minutes since notification types rarely change.
        /// </summary>
        private async Task<int?> GetNotificationTypeIdAsync(MessagingContext db, string notificationType)
        {
            string cacheKey = $"msg_notif_type:{notificationType}";

            if (_cache.IsSet(cacheKey))
            {
                return _cache.Get<int?>(cacheKey);
            }

            NotificationType nt = await (from x in db.NotificationTypes
                                          where x.name == notificationType &&
                                          x.active == true &&
                                          x.deleted == false
                                          select x)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync();

            int? result = nt?.id;
            _cache.Set(cacheKey, result, TYPE_CACHE_TTL_MINUTES);

            return result;
        }

        #endregion
    }
}
