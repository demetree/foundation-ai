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
    /// Foundation Messaging Admin Service — provides management-level oversight of
    /// the messaging system for the 'Catalyst Message Administrator' role.
    /// 
    /// Capabilities:
    ///   - Conversation oversight: list all conversations, view details
    ///   - Message oversight: search/view any message across conversations
    ///   - Message flagging: create, review, resolve flags on reported messages
    ///   - Audit logging: record administrative actions
    ///   - Delivery log queries: view push delivery history
    ///   - Usage metrics: message counts, active users, conversation activity
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 3C, March 2026.
    /// 
    /// </summary>
    public class MessagingAdminService
    {
        private readonly IMessagingUserResolver _userResolver;
        private readonly MemoryCacheManager _cache;


        public MessagingAdminService(IMessagingUserResolver userResolver)
        {
            _userResolver = userResolver;
            _cache = new MemoryCacheManager();
        }


        #region DTOs


        public class MessageFlagSummary
        {
            public int id { get; set; }
            public int conversationMessageId { get; set; }
            public string messagePreview { get; set; }
            public int flaggedByUserId { get; set; }
            public string flaggedByUserName { get; set; }
            public string reason { get; set; }
            public string details { get; set; }
            public string status { get; set; }
            public int? reviewedByUserId { get; set; }
            public string reviewedByUserName { get; set; }
            public DateTime? dateTimeReviewed { get; set; }
            public string resolutionNotes { get; set; }
            public DateTime dateTimeCreated { get; set; }
        }


        public class AuditLogEntry
        {
            public int id { get; set; }
            public int performedByUserId { get; set; }
            public string performedByUserName { get; set; }
            public string action { get; set; }
            public string entityType { get; set; }
            public int? entityId { get; set; }
            public string details { get; set; }
            public DateTime dateTimeCreated { get; set; }
        }


        public class DeliveryLogSummary
        {
            public int id { get; set; }
            public int userId { get; set; }
            public string userName { get; set; }
            public string providerId { get; set; }
            public string destination { get; set; }
            public string sourceType { get; set; }
            public bool success { get; set; }
            public string errorMessage { get; set; }
            public DateTime dateTimeCreated { get; set; }
        }


        public class MessagingMetrics
        {
            public int totalConversations { get; set; }
            public int totalMessages { get; set; }
            public int totalActiveUsers { get; set; }
            public int messagesToday { get; set; }
            public int messagesThisWeek { get; set; }
            public int openFlags { get; set; }
            public int deliveryAttemptsToday { get; set; }
            public int deliveryFailuresToday { get; set; }
        }


        #endregion


        #region Message Flags


        /// <summary>
        /// Creates a flag on a message for administrative review.
        /// </summary>
        public async Task<MessageFlagSummary> CreateFlagAsync(
            SecurityUser securityUser,
            int conversationMessageId,
            string reason,
            string details)
        {
            Guid tenantGuid = securityUser.securityTenant.objectGuid;
            MessagingUser msgUser = await _userResolver.GetUserAsync(securityUser);

            using (MessagingContext db = new MessagingContext())
            {
                MessageFlag flag = new MessageFlag
                {
                    tenantGuid = tenantGuid,
                    conversationMessageId = conversationMessageId,
                    flaggedByUserId = msgUser.id,
                    reason = reason,
                    details = details,
                    status = "open",
                    dateTimeCreated = DateTime.UtcNow,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                db.MessageFlags.Add(flag);
                await db.SaveChangesAsync();

                //
                // Log the flag creation
                //
                await LogAuditActionAsync(db, tenantGuid, msgUser.id, "FlagCreated", "ConversationMessage", conversationMessageId, $"Reason: {reason}");

                return new MessageFlagSummary
                {
                    id = flag.id,
                    conversationMessageId = conversationMessageId,
                    flaggedByUserId = msgUser.id,
                    flaggedByUserName = msgUser.displayName,
                    reason = reason,
                    details = details,
                    status = "open",
                    dateTimeCreated = flag.dateTimeCreated
                };
            }
        }


        /// <summary>
        /// Gets all message flags for the tenant, with optional status filter.
        /// </summary>
        public async Task<List<MessageFlagSummary>> GetFlagsAsync(SecurityUser securityUser, string statusFilter = null)
        {
            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                IQueryable<MessageFlag> query = db.MessageFlags
                    .Where(f => f.tenantGuid == tenantGuid && f.active && !f.deleted);

                if (!string.IsNullOrWhiteSpace(statusFilter))
                {
                    query = query.Where(f => f.status == statusFilter);
                }

                List<MessageFlag> flags = await query
                    .OrderByDescending(f => f.dateTimeCreated)
                    .Take(200)
                    .ToListAsync();

                //
                // Batch-resolve all user IDs at once (eliminates N+1 queries)
                //
                List<int> allUserIds = flags.Select(f => f.flaggedByUserId)
                    .Union(flags.Where(f => f.reviewedByUserId.HasValue).Select(f => f.reviewedByUserId.Value))
                    .Distinct().ToList();
                List<MessagingUser> resolvedUsers = await _userResolver.GetUsersByIdsAsync(allUserIds, tenantGuid);
                Dictionary<int, MessagingUser> userLookup = resolvedUsers.ToDictionary(u => u.id, u => u);

                //
                // Batch-fetch message previews
                //
                List<int> messageIds = flags.Select(f => f.conversationMessageId).Distinct().ToList();
                Dictionary<int, string> messagePreviews = await db.ConversationMessages
                    .Where(m => messageIds.Contains(m.id) && m.tenantGuid == tenantGuid)
                    .Select(m => new { m.id, m.message })
                    .ToDictionaryAsync(
                        m => m.id,
                        m => m.message != null && m.message.Length > 100 ? m.message.Substring(0, 100) + "…" : m.message);

                List<MessageFlagSummary> results = new List<MessageFlagSummary>();

                foreach (MessageFlag flag in flags)
                {
                    userLookup.TryGetValue(flag.flaggedByUserId, out MessagingUser flagger);
                    MessagingUser reviewer = flag.reviewedByUserId.HasValue && userLookup.TryGetValue(flag.reviewedByUserId.Value, out var r) ? r : null;
                    messagePreviews.TryGetValue(flag.conversationMessageId, out string preview);

                    results.Add(new MessageFlagSummary
                    {
                        id = flag.id,
                        conversationMessageId = flag.conversationMessageId,
                        messagePreview = preview,
                        flaggedByUserId = flag.flaggedByUserId,
                        flaggedByUserName = flagger?.displayName,
                        reason = flag.reason,
                        details = flag.details,
                        status = flag.status,
                        reviewedByUserId = flag.reviewedByUserId,
                        reviewedByUserName = reviewer?.displayName,
                        dateTimeReviewed = flag.dateTimeReviewed,
                        resolutionNotes = flag.resolutionNotes,
                        dateTimeCreated = flag.dateTimeCreated
                    });
                }

                return results;
            }
        }


        /// <summary>
        /// Resolves a message flag (admin review).
        /// </summary>
        public async Task<MessageFlagSummary> ResolveFlagAsync(
            SecurityUser securityUser,
            int flagId,
            string resolutionStatus,
            string resolutionNotes)
        {
            Guid tenantGuid = securityUser.securityTenant.objectGuid;
            MessagingUser adminUser = await _userResolver.GetUserAsync(securityUser);

            using (MessagingContext db = new MessagingContext())
            {
                MessageFlag flag = await db.MessageFlags
                    .FirstOrDefaultAsync(f => f.id == flagId && f.tenantGuid == tenantGuid && f.active && !f.deleted);

                if (flag == null)
                {
                    throw new InvalidOperationException("Flag not found.");
                }

                flag.status = resolutionStatus;
                flag.resolutionNotes = resolutionNotes;
                flag.reviewedByUserId = adminUser.id;
                flag.dateTimeReviewed = DateTime.UtcNow;

                await db.SaveChangesAsync();

                await LogAuditActionAsync(db, tenantGuid, adminUser.id, "FlagResolved", "MessageFlag", flagId, $"Status: {resolutionStatus}");

                return new MessageFlagSummary
                {
                    id = flag.id,
                    conversationMessageId = flag.conversationMessageId,
                    flaggedByUserId = flag.flaggedByUserId,
                    reason = flag.reason,
                    details = flag.details,
                    status = flag.status,
                    reviewedByUserId = flag.reviewedByUserId,
                    reviewedByUserName = adminUser.displayName,
                    dateTimeReviewed = flag.dateTimeReviewed,
                    resolutionNotes = flag.resolutionNotes,
                    dateTimeCreated = flag.dateTimeCreated
                };
            }
        }


        #endregion


        #region Audit Log


        /// <summary>
        /// Queries the messaging audit log.
        /// </summary>
        public async Task<List<AuditLogEntry>> GetAuditLogAsync(
            SecurityUser securityUser,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string actionFilter = null,
            int maxResults = 100)
        {
            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                IQueryable<MessagingAuditLog> query = db.MessagingAuditLogs
                    .Where(a => a.tenantGuid == tenantGuid && a.active && !a.deleted);

                if (startDate.HasValue)
                    query = query.Where(a => a.dateTimeCreated >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(a => a.dateTimeCreated <= endDate.Value);

                if (!string.IsNullOrWhiteSpace(actionFilter))
                    query = query.Where(a => a.action == actionFilter);

                List<MessagingAuditLog> entries = await query
                    .OrderByDescending(a => a.dateTimeCreated)
                    .Take(maxResults)
                    .ToListAsync();

                //
                // Batch-resolve all performer user IDs at once (eliminates N+1 queries)
                //
                List<int> performerIds = entries.Select(e => e.performedByUserId).Distinct().ToList();
                List<MessagingUser> resolvedPerformers = await _userResolver.GetUsersByIdsAsync(performerIds, tenantGuid);
                Dictionary<int, MessagingUser> performerLookup = resolvedPerformers.ToDictionary(u => u.id, u => u);

                List<AuditLogEntry> results = new List<AuditLogEntry>();

                foreach (MessagingAuditLog entry in entries)
                {
                    performerLookup.TryGetValue(entry.performedByUserId, out MessagingUser performer);

                    results.Add(new AuditLogEntry
                    {
                        id = entry.id,
                        performedByUserId = entry.performedByUserId,
                        performedByUserName = performer?.displayName,
                        action = entry.action,
                        entityType = entry.entityType,
                        entityId = entry.entityId,
                        details = entry.details,
                        dateTimeCreated = entry.dateTimeCreated
                    });
                }

                return results;
            }
        }


        /// <summary>
        /// Writes an audit entry.  Called internally and can be called from controllers.
        /// </summary>
        public async Task LogAuditActionAsync(
            SecurityUser securityUser,
            string action,
            string entityType,
            int? entityId,
            string details,
            string ipAddress = null)
        {
            Guid tenantGuid = securityUser.securityTenant.objectGuid;
            MessagingUser msgUser = await _userResolver.GetUserAsync(securityUser);

            using (MessagingContext db = new MessagingContext())
            {
                await LogAuditActionAsync(db, tenantGuid, msgUser.id, action, entityType, entityId, details, ipAddress);
            }
        }


        /// <summary>
        /// Internal audit log writer (uses existing context).
        /// </summary>
        private async Task LogAuditActionAsync(
            MessagingContext db,
            Guid tenantGuid,
            int performedByUserId,
            string action,
            string entityType,
            int? entityId,
            string details,
            string ipAddress = null)
        {
            try
            {
                MessagingAuditLog entry = new MessagingAuditLog
                {
                    tenantGuid = tenantGuid,
                    performedByUserId = performedByUserId,
                    action = action,
                    entityType = entityType,
                    entityId = entityId,
                    details = details?.Length > 4000 ? details.Substring(0, 4000) : details,
                    ipAddress = ipAddress,
                    dateTimeCreated = DateTime.UtcNow,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                db.MessagingAuditLogs.Add(entry);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write messaging audit log: {ex.Message}");
            }
        }


        #endregion


        #region Delivery Logs


        /// <summary>
        /// Queries push delivery logs for administrative review.
        /// </summary>
        public async Task<List<DeliveryLogSummary>> GetDeliveryLogsAsync(
            SecurityUser securityUser,
            int? userId = null,
            string providerId = null,
            bool? successOnly = null,
            DateTime? startDate = null,
            int maxResults = 100)
        {
            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                IQueryable<PushDeliveryLog> query = db.PushDeliveryLogs
                    .Where(d => d.tenantGuid == tenantGuid && d.active && !d.deleted);

                if (userId.HasValue)
                    query = query.Where(d => d.userId == userId.Value);

                if (!string.IsNullOrWhiteSpace(providerId))
                    query = query.Where(d => d.providerId == providerId);

                if (successOnly.HasValue)
                    query = query.Where(d => d.success == successOnly.Value);

                if (startDate.HasValue)
                    query = query.Where(d => d.dateTimeCreated >= startDate.Value);

                List<PushDeliveryLog> logs = await query
                    .OrderByDescending(d => d.dateTimeCreated)
                    .Take(maxResults)
                    .ToListAsync();

                List<DeliveryLogSummary> results = new List<DeliveryLogSummary>();

                foreach (PushDeliveryLog log in logs)
                {
                    MessagingUser user = await _userResolver.GetUserByIdAsync(log.userId, tenantGuid);

                    results.Add(new DeliveryLogSummary
                    {
                        id = log.id,
                        userId = log.userId,
                        userName = user?.displayName,
                        providerId = log.providerId,
                        destination = log.destination,
                        sourceType = log.sourceType,
                        success = log.success,
                        errorMessage = log.errorMessage,
                        dateTimeCreated = log.dateTimeCreated
                    });
                }

                return results;
            }
        }


        #endregion


        #region Admin Message Search


        public class AdminMessageResult
        {
            public int id { get; set; }
            public int conversationId { get; set; }
            public string conversationName { get; set; }
            public int userId { get; set; }
            public string userDisplayName { get; set; }
            public string message { get; set; }
            public DateTime dateTimeCreated { get; set; }
            public int? parentConversationMessageId { get; set; }
            public string entity { get; set; }
            public int? entityId { get; set; }
            public bool isDeleted { get; set; }
        }


        /// <summary>
        /// Searches ALL messages across ALL conversations for the tenant.
        /// Unlike the regular user search, this has no membership restriction.
        /// Intended for Catalyst Message Administrator role only.
        /// </summary>
        public async Task<List<AdminMessageResult>> AdminSearchMessagesAsync(
            SecurityUser securityUser,
            string query = null,
            int? conversationId = null,
            int? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int maxResults = 100)
        {
            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                //
                // Base query: all messages for the tenant (no membership filter)
                //
                IQueryable<ConversationMessage> searchQuery = db.ConversationMessages
                    .Where(m => m.tenantGuid == tenantGuid && m.active);

                //
                // Apply filters
                //
                if (!string.IsNullOrWhiteSpace(query))
                {
                    searchQuery = searchQuery.Where(m => m.message.Contains(query));
                }

                if (conversationId.HasValue)
                {
                    searchQuery = searchQuery.Where(m => m.conversationId == conversationId.Value);
                }

                if (userId.HasValue)
                {
                    searchQuery = searchQuery.Where(m => m.userId == userId.Value);
                }

                if (startDate.HasValue)
                {
                    searchQuery = searchQuery.Where(m => m.dateTimeCreated >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    searchQuery = searchQuery.Where(m => m.dateTimeCreated <= endDate.Value);
                }

                List<ConversationMessage> messages = await searchQuery
                    .OrderByDescending(m => m.dateTimeCreated)
                    .Take(maxResults)
                    .AsNoTracking()
                    .ToListAsync();

                //
                // Pre-fetch conversation names for the results
                //
                List<int> conversationIds = messages.Select(m => m.conversationId).Distinct().ToList();
                Dictionary<int, string> conversationNames = await db.Conversations
                    .Where(c => conversationIds.Contains(c.id) && c.tenantGuid == tenantGuid)
                    .ToDictionaryAsync(c => c.id, c => c.name ?? $"Conversation #{c.id}");

                //
                // Build results
                //
                List<AdminMessageResult> results = new List<AdminMessageResult>();

                foreach (ConversationMessage msg in messages)
                {
                    MessagingUser sender = await _userResolver.GetUserByIdAsync(msg.userId, tenantGuid);

                    conversationNames.TryGetValue(msg.conversationId, out string convName);

                    results.Add(new AdminMessageResult
                    {
                        id = msg.id,
                        conversationId = msg.conversationId,
                        conversationName = convName ?? $"Conversation #{msg.conversationId}",
                        userId = msg.userId,
                        userDisplayName = sender?.displayName ?? $"User #{msg.userId}",
                        message = msg.message,
                        dateTimeCreated = msg.dateTimeCreated,
                        parentConversationMessageId = msg.parentConversationMessageId,
                        entity = msg.entity,
                        entityId = msg.entityId,
                        isDeleted = msg.deleted
                    });
                }

                //
                // Log the admin search
                //
                MessagingUser adminUser = await _userResolver.GetUserAsync(securityUser);
                await LogAuditActionAsync(db, tenantGuid, adminUser.id, "AdminSearchMessages", null, null, $"Query: {query ?? "(browse)"}, Results: {results.Count}");

                return results;
            }
        }


        #endregion


        #region Metrics


        /// <summary>
        /// Gets high-level messaging usage metrics for the tenant.
        /// </summary>
        public async Task<MessagingMetrics> GetMetricsAsync(SecurityUser securityUser)
        {
            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            //
            // Check cache first (2-minute TTL to reduce DB round-trips)
            //
            string cacheKey = $"msg_metrics:{tenantGuid}";
            var cached = _cache.Get<MessagingMetrics>(cacheKey);
            if (cached != null) return cached;

            using (MessagingContext db = new MessagingContext())
            {
                DateTime today = DateTime.UtcNow.Date;
                DateTime weekStart = today.AddDays(-(int)today.DayOfWeek);

                MessagingMetrics metrics = new MessagingMetrics();

                metrics.totalConversations = await db.Conversations
                    .CountAsync(c => c.tenantGuid == tenantGuid && c.active && !c.deleted);

                metrics.totalMessages = await db.ConversationMessages
                    .CountAsync(m => m.tenantGuid == tenantGuid && m.active && !m.deleted);

                metrics.messagesToday = await db.ConversationMessages
                    .CountAsync(m => m.tenantGuid == tenantGuid && m.active && !m.deleted && m.dateTimeCreated >= today);

                metrics.messagesThisWeek = await db.ConversationMessages
                    .CountAsync(m => m.tenantGuid == tenantGuid && m.active && !m.deleted && m.dateTimeCreated >= weekStart);

                metrics.openFlags = await db.MessageFlags
                    .CountAsync(f => f.tenantGuid == tenantGuid && f.active && !f.deleted && f.status == "open");

                metrics.deliveryAttemptsToday = await db.PushDeliveryLogs
                    .CountAsync(d => d.tenantGuid == tenantGuid && d.dateTimeCreated >= today);

                metrics.deliveryFailuresToday = await db.PushDeliveryLogs
                    .CountAsync(d => d.tenantGuid == tenantGuid && d.dateTimeCreated >= today && !d.success);

                //
                // Active users: distinct message senders in the last 7 days
                //
                DateTime sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                metrics.totalActiveUsers = await db.ConversationMessages
                    .Where(m => m.tenantGuid == tenantGuid && m.active && !m.deleted && m.dateTimeCreated >= sevenDaysAgo)
                    .Select(m => m.userId)
                    .Distinct()
                    .CountAsync();

                _cache.Set(cacheKey, metrics, 2f);
                return metrics;
            }
        }


        #endregion


        #region Analytics


        public class MessagingAnalytics
        {
            public List<DailyMessageCount> messagesOverTime { get; set; }
            public List<UserActivity> topUsers { get; set; }
            public List<ChannelActivity> topChannels { get; set; }
            public List<UserMessageFlow> userFlows { get; set; }
        }


        public class DailyMessageCount
        {
            public string date { get; set; }
            public int count { get; set; }
        }


        public class UserActivity
        {
            public int userId { get; set; }
            public string displayName { get; set; }
            public int messageCount { get; set; }
            public int conversationCount { get; set; }
            public DateTime lastActive { get; set; }
        }


        public class ChannelActivity
        {
            public int conversationId { get; set; }
            public string name { get; set; }
            public int messageCount { get; set; }
            public int memberCount { get; set; }
        }


        public class UserMessageFlow
        {
            public string fromUser { get; set; }
            public string toUser { get; set; }
            public int weight { get; set; }
        }


        /// <summary>
        /// Gets analytics data for chart visualizations:
        /// messages over time, top users, top channels, and user-to-user flows.
        /// </summary>
        public async Task<MessagingAnalytics> GetAnalyticsAsync(SecurityUser securityUser)
        {
            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            //
            // Check cache first (10-minute TTL — analytics data doesn't need to be real-time)
            //
            string cacheKey = $"msg_analytics:{tenantGuid}";
            var cached = _cache.Get<MessagingAnalytics>(cacheKey);
            if (cached != null) return cached;

            using (MessagingContext db = new MessagingContext())
            {
                MessagingAnalytics analytics = new MessagingAnalytics();

                DateTime thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-30);

                //
                // Messages over time (last 30 days, grouped by date)
                //
                analytics.messagesOverTime = (await db.ConversationMessages
                    .Where(m => m.tenantGuid == tenantGuid && m.active && !m.deleted && m.dateTimeCreated >= thirtyDaysAgo)
                    .GroupBy(m => new { m.dateTimeCreated.Year, m.dateTimeCreated.Month, m.dateTimeCreated.Day })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Day,
                        count = g.Count()
                    })
                    .OrderBy(d => d.Year).ThenBy(d => d.Month).ThenBy(d => d.Day)
                    .ToListAsync())
                    .Select(d => new DailyMessageCount
                    {
                        date = $"{d.Year:D4}-{d.Month:D2}-{d.Day:D2}",
                        count = d.count
                    })
                    .ToList();

                //
                // Top users by message count (last 30 days)
                //
                var topUserRaw = await db.ConversationMessages
                    .Where(m => m.tenantGuid == tenantGuid && m.active && !m.deleted && m.dateTimeCreated >= thirtyDaysAgo)
                    .GroupBy(m => m.userId)
                    .Select(g => new
                    {
                        userId = g.Key,
                        messageCount = g.Count(),
                        lastActive = g.Max(m => m.dateTimeCreated)
                    })
                    .OrderByDescending(u => u.messageCount)
                    .Take(15)
                    .ToListAsync();

                //
                // Get conversation counts per user (separate query to avoid EF translation issues)
                //
                List<int> topUserIds = topUserRaw.Select(u => u.userId).ToList();
                var userConvCounts = await db.ConversationMessages
                    .Where(m => m.tenantGuid == tenantGuid && m.active && !m.deleted
                        && m.dateTimeCreated >= thirtyDaysAgo && topUserIds.Contains(m.userId))
                    .Select(m => new { m.userId, m.conversationId })
                    .Distinct()
                    .GroupBy(m => m.userId)
                    .Select(g => new { userId = g.Key, count = g.Count() })
                    .ToListAsync();

                Dictionary<int, int> convCountMap = userConvCounts.ToDictionary(x => x.userId, x => x.count);

                analytics.topUsers = new List<UserActivity>();
                foreach (var u in topUserRaw)
                {
                    MessagingUser user = await _userResolver.GetUserByIdAsync(u.userId, tenantGuid);
                    convCountMap.TryGetValue(u.userId, out int convCount);
                    analytics.topUsers.Add(new UserActivity
                    {
                        userId = u.userId,
                        displayName = user?.displayName ?? $"User #{u.userId}",
                        messageCount = u.messageCount,
                        conversationCount = convCount,
                        lastActive = u.lastActive
                    });
                }

                //
                // Top channels by message count
                //
                var topChannelData = await db.ConversationMessages
                    .Where(m => m.tenantGuid == tenantGuid && m.active && !m.deleted && m.dateTimeCreated >= thirtyDaysAgo)
                    .GroupBy(m => m.conversationId)
                    .Select(g => new
                    {
                        conversationId = g.Key,
                        messageCount = g.Count()
                    })
                    .OrderByDescending(c => c.messageCount)
                    .Take(10)
                    .ToListAsync();

                List<int> channelIds = topChannelData.Select(c => c.conversationId).ToList();

                Dictionary<int, string> channelNames = await db.Conversations
                    .Where(c => channelIds.Contains(c.id) && c.tenantGuid == tenantGuid)
                    .ToDictionaryAsync(c => c.id, c => c.name ?? $"Conversation #{c.id}");

                Dictionary<int, int> memberCounts = (await db.ConversationUsers
                    .Where(cu => channelIds.Contains(cu.conversationId) && cu.active && !cu.deleted)
                    .GroupBy(cu => cu.conversationId)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToListAsync())
                    .ToDictionary(x => x.Id, x => x.Count);

                analytics.topChannels = topChannelData.Select(c =>
                {
                    channelNames.TryGetValue(c.conversationId, out string name);
                    memberCounts.TryGetValue(c.conversationId, out int members);
                    return new ChannelActivity
                    {
                        conversationId = c.conversationId,
                        name = name ?? $"Conversation #{c.conversationId}",
                        messageCount = c.messageCount,
                        memberCount = members
                    };
                }).ToList();

                //
                // User-to-user message flows for Sankey diagram.
                // For each conversation, get users who sent messages; pair them as sender → other participants.
                //
                var recentMessages = await db.ConversationMessages
                    .Where(m => m.tenantGuid == tenantGuid && m.active && !m.deleted && m.dateTimeCreated >= thirtyDaysAgo)
                    .GroupBy(m => new { m.conversationId, m.userId })
                    .Select(g => new
                    {
                        g.Key.conversationId,
                        g.Key.userId,
                        count = g.Count()
                    })
                    .ToListAsync();

                //
                // Group by conversation to find participants
                //
                var convParticipants = recentMessages
                    .GroupBy(m => m.conversationId)
                    .Where(g => g.Select(x => x.userId).Distinct().Count() >= 2)
                    .ToList();

                //
                // Build user ID → display name lookup for participants
                //
                HashSet<int> allUserIds = new HashSet<int>(recentMessages.Select(m => m.userId));
                List<MessagingUser> resolvedSankeyUsers = await _userResolver.GetUsersByIdsAsync(allUserIds.ToList(), tenantGuid);
                Dictionary<int, string> userNames = resolvedSankeyUsers.ToDictionary(u => u.id, u => u.displayName ?? $"User #{u.id}");
                // Add fallback entries for any IDs that weren't resolved
                foreach (int uid in allUserIds)
                {
                    if (!userNames.ContainsKey(uid))
                    {
                        userNames[uid] = $"User #{uid}";
                    }
                }

                //
                // Generate flows: for each conversation, pair sender with each other participant.
                // Weight = sender's message count in that conversation.
                //
                Dictionary<string, int> flowWeights = new Dictionary<string, int>();

                foreach (var conv in convParticipants)
                {
                    List<int> participantIds = conv.Select(x => x.userId).Distinct().ToList();
                    foreach (var sender in conv)
                    {
                        foreach (int recipientId in participantIds)
                        {
                            if (recipientId == sender.userId) continue;

                            string fromName = userNames.GetValueOrDefault(sender.userId, $"User #{sender.userId}");
                            string toName = userNames.GetValueOrDefault(recipientId, $"User #{recipientId}");
                            string key = $"{fromName}→{toName}";

                            if (flowWeights.ContainsKey(key))
                                flowWeights[key] += sender.count;
                            else
                                flowWeights[key] = sender.count;
                        }
                    }
                }

                analytics.userFlows = flowWeights
                    .OrderByDescending(kv => kv.Value)
                    .Take(30)
                    .Select(kv =>
                    {
                        string[] parts = kv.Key.Split('→');
                        return new UserMessageFlow
                        {
                            fromUser = parts[0],
                            toUser = parts[1],
                            weight = kv.Value
                        };
                    })
                    .ToList();

                _cache.Set(cacheKey, analytics, 10f);
                return analytics;
            }
        }


        #endregion
    }
}
