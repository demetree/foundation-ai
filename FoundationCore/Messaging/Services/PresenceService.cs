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
    /// Foundation Presence Service - manages user online/offline/away status for the messaging system.
    /// 
    /// This service provides:
    /// - Status updates (Online, Away, DoNotDisturb, Offline)
    /// - Custom status messages
    /// - Connection tracking (how many active connections per user)
    /// - Last seen and last activity timestamps
    /// - Presence queries for UI display
    /// 
    /// This is a Foundation-level service that can be used by any module.
    /// User resolution is handled through IMessagingUserResolver, allowing each module
    /// to provide its own user lookup implementation.
    /// 
    /// </summary>
    public class PresenceService
    {
        //
        // Standard status values
        //
        public const string STATUS_ONLINE = "Online";
        public const string STATUS_AWAY = "Away";
        public const string STATUS_DO_NOT_DISTURB = "DoNotDisturb";
        public const string STATUS_OFFLINE = "Offline";
        public const string STATUS_BUSY = "Busy";


        private readonly IMessagingUserResolver _userResolver;
        private readonly MemoryCacheManager _cache;

        private const float PRESENCE_CACHE_TTL_MINUTES = 0.5f;   // 30 seconds


        public PresenceService(IMessagingUserResolver userResolver)
        {
            _userResolver = userResolver;
            _cache = new MemoryCacheManager();
        }


        #region DTOs

        public class PresenceSummary
        {
            public int userId { get; set; }
            public string displayName { get; set; }
            public string accountName { get; set; }
            public string status { get; set; }
            public string customStatusMessage { get; set; }
            public DateTime lastSeenDateTime { get; set; }
            public DateTime lastActivityDateTime { get; set; }
            public int connectionCount { get; set; }
        }

        #endregion


        #region Status Updates


        /// <summary>
        /// Sets the user's presence status (Online, Away, DoNotDisturb, Offline, Busy).
        /// Creates the UserPresence record if it doesn't exist.
        /// Returns the updated PresenceSummary.
        /// </summary>
        public async Task<PresenceSummary> SetStatusAsync(SecurityUser securityUser, string status, string customStatusMessage = null)
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

                UserPresence presence = await GetOrCreatePresenceAsync(db, user.id, securityUser.securityTenant.objectGuid);

                presence.status = status;
                presence.lastSeenDateTime = DateTime.UtcNow;
                presence.lastActivityDateTime = DateTime.UtcNow;

                if (customStatusMessage != null)
                {
                    presence.customStatusMessage = customStatusMessage;
                }

                await db.SaveChangesAsync();

                PresenceSummary summary = ToPresenceSummary(presence, user);
                EvictPresenceCache(user.id, securityUser.securityTenant.objectGuid);
                return summary;
            }
        }



        /// <summary>
        /// Sets a custom status message for the user.
        /// </summary>
        public async Task<PresenceSummary> SetCustomStatusMessageAsync(SecurityUser securityUser, string customStatusMessage)
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

                UserPresence presence = await GetOrCreatePresenceAsync(db, user.id, securityUser.securityTenant.objectGuid);

                presence.customStatusMessage = customStatusMessage;
                presence.lastSeenDateTime = DateTime.UtcNow;

                await db.SaveChangesAsync();

                PresenceSummary summary = ToPresenceSummary(presence, user);
                EvictPresenceCache(user.id, securityUser.securityTenant.objectGuid);
                return summary;
            }
        }



        /// <summary>
        /// Clears the user's custom status message (keeps their base status).
        /// </summary>
        public async Task<PresenceSummary> ClearCustomStatusMessageAsync(SecurityUser securityUser)
        {
            return await SetCustomStatusMessageAsync(securityUser, null);
        }

        #endregion


        #region Connection Tracking


        /// <summary>
        /// Records a new SignalR connection for the user.  Increments the connection count and sets status to Online.
        /// Called from the MessagingHub OnConnectedAsync override or from the controller on login.
        /// </summary>
        public async Task<PresenceSummary> RecordConnectionAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                if (user == null) return null;

                UserPresence presence = await GetOrCreatePresenceAsync(db, user.id, securityUser.securityTenant.objectGuid);

                presence.connectionCount = presence.connectionCount + 1;
                presence.status = STATUS_ONLINE;
                presence.lastSeenDateTime = DateTime.UtcNow;
                presence.lastActivityDateTime = DateTime.UtcNow;

                await db.SaveChangesAsync();

                PresenceSummary summary = ToPresenceSummary(presence, user);
                EvictPresenceCache(user.id, securityUser.securityTenant.objectGuid);
                return summary;
            }
        }



        /// <summary>
        /// Records a SignalR disconnection for the user.  Decrements the connection count.
        /// If the connection count drops to zero, sets status to Offline.
        /// Called from the MessagingHub OnDisconnectedAsync override.
        /// </summary>
        public async Task<PresenceSummary> RecordDisconnectionAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                if (user == null) return null;

                UserPresence presence = await GetOrCreatePresenceAsync(db, user.id, securityUser.securityTenant.objectGuid);

                presence.connectionCount = Math.Max(0, presence.connectionCount - 1);
                presence.lastSeenDateTime = DateTime.UtcNow;

                //
                // If no connections remain, the user is offline
                //
                if (presence.connectionCount == 0)
                {
                    presence.status = STATUS_OFFLINE;
                }

                await db.SaveChangesAsync();

                PresenceSummary summary = ToPresenceSummary(presence, user);
                EvictPresenceCache(user.id, securityUser.securityTenant.objectGuid);
                return summary;
            }
        }



        /// <summary>
        /// Updates the lastActivityDateTime for a heartbeat (called periodically by the client to indicate activity).
        /// </summary>
        public async Task RecordActivityAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                if (user == null) return;

                UserPresence presence = await (from p in db.UserPresences
                                                where
                                                p.userId == user.id &&
                                                p.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                p.active == true &&
                                                p.deleted == false
                                                select p)
                                               .FirstOrDefaultAsync();

                if (presence != null)
                {
                    presence.lastActivityDateTime = DateTime.UtcNow;
                    presence.lastSeenDateTime = DateTime.UtcNow;

                    await db.SaveChangesAsync();

                    EvictPresenceCache(user.id, securityUser.securityTenant.objectGuid);
                }
            }
        }

        #endregion


        #region Queries


        /// <summary>
        /// Gets the presence status of a single user.
        /// Uses a 30-second cache to reduce DB round trips.
        /// </summary>
        public async Task<PresenceSummary> GetUserPresenceAsync(SecurityUser securityUser, int queryUserId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            //
            // Check cache first
            //
            string cacheKey = $"msg_presence:{securityUser.securityTenant.objectGuid}:{queryUserId}";

            var cached = _cache.Get<PresenceSummary>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            using (MessagingContext db = new MessagingContext())
            {
                UserPresence presence = await (from p in db.UserPresences
                                                where
                                                p.userId == queryUserId &&
                                                p.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                p.active == true &&
                                                p.deleted == false
                                                select p)
                                               .AsNoTracking()
                                               .FirstOrDefaultAsync();

                //
                // Resolve user info through the user resolver
                //
                MessagingUser queryUser = await _userResolver.GetUserByIdAsync(queryUserId, securityUser.securityTenant.objectGuid);

                if (queryUser == null) return null;

                PresenceSummary summary;

                if (presence == null)
                {
                    //
                    // No presence record — user has never connected, return offline
                    //
                    summary = new PresenceSummary
                    {
                        userId = queryUser.id,
                        displayName = queryUser.displayName,
                        accountName = queryUser.accountName,
                        status = STATUS_OFFLINE,
                        connectionCount = 0,
                        lastSeenDateTime = DateTime.MinValue,
                        lastActivityDateTime = DateTime.MinValue
                    };
                }
                else
                {
                    summary = new PresenceSummary
                    {
                        userId = presence.userId,
                        displayName = queryUser.displayName,
                        accountName = queryUser.accountName,
                        status = presence.status,
                        customStatusMessage = presence.customStatusMessage,
                        lastSeenDateTime = presence.lastSeenDateTime,
                        lastActivityDateTime = presence.lastActivityDateTime,
                        connectionCount = presence.connectionCount
                    };
                }

                _cache.Set(cacheKey, summary, PRESENCE_CACHE_TTL_MINUTES);
                return summary;
            }
        }



        /// <summary>
        /// Gets presence status for users that are members of a specific conversation.
        /// Useful for showing who is online in a chat window.
        /// </summary>
        public async Task<List<PresenceSummary>> GetConversationPresencesAsync(SecurityUser securityUser, int conversationId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                //
                // Get all user IDs in the conversation
                //
                List<int> memberUserIds = await (from cu in db.ConversationUsers
                                                  where
                                                  cu.conversationId == conversationId &&
                                                  cu.active == true &&
                                                  cu.deleted == false
                                                  select cu.userId)
                                                 .ToListAsync();

                //
                // Get presence records for those users
                //
                List<UserPresence> presenceRecords = await (from p in db.UserPresences
                                                             where
                                                             memberUserIds.Contains(p.userId) &&
                                                             p.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                             p.active == true &&
                                                             p.deleted == false
                                                             select p)
                                                            .AsNoTracking()
                                                            .ToListAsync();

                Dictionary<int, UserPresence> presenceLookup = presenceRecords.ToDictionary(p => p.userId, p => p);


                //
                // Batch-resolve user info for all members
                //
                List<MessagingUser> resolvedUsers = await _userResolver.GetUsersByIdsAsync(memberUserIds, securityUser.securityTenant.objectGuid);
                Dictionary<int, MessagingUser> userLookup = resolvedUsers.ToDictionary(u => u.id, u => u);

                List<PresenceSummary> summaries = new List<PresenceSummary>();

                foreach (int userId in memberUserIds)
                {
                    if (!userLookup.TryGetValue(userId, out MessagingUser user)) continue;

                    if (presenceLookup.TryGetValue(userId, out UserPresence presence))
                    {
                        summaries.Add(new PresenceSummary
                        {
                            userId = user.id,
                            displayName = user.displayName,
                            accountName = user.accountName,
                            status = presence.status,
                            customStatusMessage = presence.customStatusMessage,
                            lastSeenDateTime = presence.lastSeenDateTime,
                            lastActivityDateTime = presence.lastActivityDateTime,
                            connectionCount = presence.connectionCount
                        });
                    }
                    else
                    {
                        summaries.Add(new PresenceSummary
                        {
                            userId = user.id,
                            displayName = user.displayName,
                            accountName = user.accountName,
                            status = STATUS_OFFLINE,
                            connectionCount = 0,
                            lastSeenDateTime = DateTime.MinValue,
                            lastActivityDateTime = DateTime.MinValue
                        });
                    }
                }

                return summaries;
            }
        }



        /// <summary>
        /// Gets all users in the tenant who are not Offline (i.e. Online, Away, Busy, DoNotDisturb).
        /// Used by the "Online Now" UI section.
        /// </summary>
        public async Task<List<PresenceSummary>> GetOnlineUsersAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return new List<PresenceSummary>();
            }

            using (MessagingContext db = new MessagingContext())
            {
                List<UserPresence> onlinePresences = await (from p in db.UserPresences
                                                             where
                                                             p.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                             p.status != STATUS_OFFLINE &&
                                                             p.connectionCount > 0 &&
                                                             p.active == true &&
                                                             p.deleted == false
                                                             select p)
                                                            .AsNoTracking()
                                                            .ToListAsync();

                List<PresenceSummary> summaries = new List<PresenceSummary>();

                //
                // Batch-resolve user info
                //
                List<int> userIds = onlinePresences.Select(p => p.userId).ToList();
                List<MessagingUser> resolvedUsers = await _userResolver.GetUsersByIdsAsync(userIds, securityUser.securityTenant.objectGuid);
                Dictionary<int, MessagingUser> userLookup = resolvedUsers.ToDictionary(u => u.id, u => u);

                foreach (UserPresence presence in onlinePresences)
                {
                    if (userLookup.TryGetValue(presence.userId, out MessagingUser user))
                    {
                        summaries.Add(ToPresenceSummary(presence, user));
                    }
                }

                return summaries.OrderBy(s => s.displayName).ToList();
            }
        }


        /// <summary>
        /// Gets all users in the tenant with their presence status.
        /// Online/Away/Busy/DND users show their real status; users without a presence record show as Offline.
        /// Results sorted by status priority (Online first, Offline last), then alphabetically.
        /// Used by the People Directory panel.
        /// </summary>
        public async Task<List<PresenceSummary>> GetAllUserPresencesAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return new List<PresenceSummary>();
            }

            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            //
            // Get all users in the tenant
            //
            List<MessagingUser> allUsers = await _userResolver.GetAllUsersAsync(tenantGuid);

            if (allUsers.Count == 0)
            {
                return new List<PresenceSummary>();
            }

            //
            // Get all presence records for the tenant
            //
            using (MessagingContext db = new MessagingContext())
            {
                List<UserPresence> presenceRecords = await (from p in db.UserPresences
                                                             where
                                                             p.tenantGuid == tenantGuid &&
                                                             p.active == true &&
                                                             p.deleted == false
                                                             select p)
                                                            .AsNoTracking()
                                                            .ToListAsync();

                Dictionary<int, UserPresence> presenceLookup = presenceRecords.ToDictionary(p => p.userId, p => p);

                List<PresenceSummary> summaries = new List<PresenceSummary>();

                foreach (MessagingUser user in allUsers)
                {
                    if (presenceLookup.TryGetValue(user.id, out UserPresence presence))
                    {
                        summaries.Add(ToPresenceSummary(presence, user));
                    }
                    else
                    {
                        summaries.Add(new PresenceSummary
                        {
                            userId = user.id,
                            displayName = user.displayName,
                            accountName = user.accountName,
                            status = STATUS_OFFLINE,
                            connectionCount = 0,
                            lastSeenDateTime = DateTime.MinValue,
                            lastActivityDateTime = DateTime.MinValue
                        });
                    }
                }

                //
                // Sort by status priority (Online → Away → Busy → DND → Offline), then alphabetically
                //
                return summaries.OrderBy(s => GetStatusSortOrder(s.status))
                                .ThenBy(s => s.displayName)
                                .ToList();
            }
        }


        private static int GetStatusSortOrder(string status)
        {
            switch (status)
            {
                case STATUS_ONLINE: return 0;
                case STATUS_AWAY: return 1;
                case STATUS_BUSY: return 2;
                case STATUS_DO_NOT_DISTURB: return 3;
                case STATUS_OFFLINE: return 4;
                default: return 5;
            }
        }


        /// <summary>
        /// Marks users as away if their last activity exceeds the idle threshold.
        /// This should be called periodically by a background job or heartbeat sweep.
        /// </summary>
        public async Task<List<PresenceSummary>> SweepIdleUsersAsync(Guid tenantGuid, int idleThresholdMinutes = 5)
        {
            using (MessagingContext db = new MessagingContext())
            {
                DateTime idleCutoff = DateTime.UtcNow.AddMinutes(-idleThresholdMinutes);

                //
                // Find users that are Online but have been idle past the threshold
                //
                List<UserPresence> idlePresences = await (from p in db.UserPresences
                                                           where
                                                           p.tenantGuid == tenantGuid &&
                                                           p.status == STATUS_ONLINE &&
                                                           p.lastActivityDateTime < idleCutoff &&
                                                           p.active == true &&
                                                           p.deleted == false
                                                           select p)
                                                          .ToListAsync();


                List<PresenceSummary> changedUsers = new List<PresenceSummary>();

                foreach (UserPresence presence in idlePresences)
                {
                    presence.status = STATUS_AWAY;

                    //
                    // Resolve user info from the user resolver
                    //
                    MessagingUser user = await _userResolver.GetUserByIdAsync(presence.userId, tenantGuid);

                    if (user != null)
                    {
                        changedUsers.Add(ToPresenceSummary(presence, user));
                        EvictPresenceCache(presence.userId, tenantGuid);
                    }
                }


                if (changedUsers.Count > 0)
                {
                    await db.SaveChangesAsync();
                }

                return changedUsers;
            }
        }


        /// <summary>
        /// Cross-tenant cleanup of stale presence records.
        /// 
        /// Marks users as Offline and resets connectionCount to 0 when their lastActivityDateTime
        /// exceeds the specified threshold.  This handles cases where SignalR disconnect detection
        /// fails (e.g. network loss without clean TCP close, laptop sleep, power loss).
        /// 
        /// Returns results grouped by tenantGuid so the caller can broadcast PresenceChanged
        /// events to the correct tenant groups.
        /// 
        /// Called periodically by PresenceCleanupService (BackgroundService).
        /// </summary>
        public async Task<Dictionary<Guid, List<PresenceSummary>>> CleanupStalePresenceAsync(int staleThresholdMinutes = 3)
        {
            var result = new Dictionary<Guid, List<PresenceSummary>>();

            using (MessagingContext db = new MessagingContext())
            {
                DateTime staleCutoff = DateTime.UtcNow.AddMinutes(-staleThresholdMinutes);

                //
                // Find all non-Offline users across all tenants whose heartbeat has gone stale
                //
                List<UserPresence> stalePresences = await (from p in db.UserPresences
                                                            where
                                                            p.status != STATUS_OFFLINE &&
                                                            p.lastActivityDateTime < staleCutoff &&
                                                            p.active == true &&
                                                            p.deleted == false
                                                            select p)
                                                           .ToListAsync();

                if (stalePresences.Count == 0)
                {
                    return result;
                }

                //
                // Group by tenant so we can resolve users and broadcast per-tenant
                //
                var groupedByTenant = stalePresences.GroupBy(p => p.tenantGuid);

                foreach (var tenantGroup in groupedByTenant)
                {
                    Guid tenantGuid = tenantGroup.Key;
                    var changedUsers = new List<PresenceSummary>();

                    foreach (UserPresence presence in tenantGroup)
                    {
                        presence.status = STATUS_OFFLINE;
                        presence.connectionCount = 0;
                        presence.lastSeenDateTime = DateTime.UtcNow;

                        MessagingUser user = await _userResolver.GetUserByIdAsync(presence.userId, tenantGuid);

                        if (user != null)
                        {
                            changedUsers.Add(ToPresenceSummary(presence, user));
                            EvictPresenceCache(presence.userId, tenantGuid);
                        }
                    }

                    if (changedUsers.Count > 0)
                    {
                        result[tenantGuid] = changedUsers;
                    }
                }

                if (stalePresences.Count > 0)
                {
                    await db.SaveChangesAsync();
                }
            }

            return result;
        }

        #endregion


        #region Private Helpers


        private static async Task<UserPresence> GetOrCreatePresenceAsync(MessagingContext db, int userId, Guid tenantGuid)
        {
            UserPresence presence = await (from p in db.UserPresences
                                            where
                                            p.userId == userId &&
                                            p.tenantGuid == tenantGuid &&
                                            p.active == true &&
                                            p.deleted == false
                                            select p)
                                           .FirstOrDefaultAsync();

            if (presence == null)
            {
                presence = new UserPresence();
                presence.userId = userId;
                presence.tenantGuid = tenantGuid;
                presence.status = STATUS_OFFLINE;
                presence.lastSeenDateTime = DateTime.UtcNow;
                presence.lastActivityDateTime = DateTime.UtcNow;
                presence.connectionCount = 0;
                presence.objectGuid = Guid.NewGuid();
                presence.active = true;
                presence.deleted = false;

                db.UserPresences.Add(presence);
            }

            return presence;
        }


        private static PresenceSummary ToPresenceSummary(UserPresence presence, MessagingUser user)
        {
            return new PresenceSummary
            {
                userId = presence.userId,
                displayName = user?.displayName,
                accountName = user?.accountName,
                status = presence.status,
                customStatusMessage = presence.customStatusMessage,
                lastSeenDateTime = presence.lastSeenDateTime,
                lastActivityDateTime = presence.lastActivityDateTime,
                connectionCount = presence.connectionCount
            };
        }

        /// <summary>
        /// Evicts cached presence for a specific user.
        /// </summary>
        private void EvictPresenceCache(int userId, Guid tenantGuid)
        {
            string cacheKey = $"msg_presence:{tenantGuid}:{userId}";
            _cache.Remove(cacheKey);
        }

        #endregion
    }
}
