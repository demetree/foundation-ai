using Foundation.Auditor;
using Foundation.Cache;
using Foundation.Messaging.Database;
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Foundation Conversation Service - provides business logic for the conversation/messaging system.
    /// 
    /// This is a Foundation-level service that can be used by any module. It handles:
    /// - Conversation lifecycle (create, archive, query)
    /// - Membership (add/remove users)
    /// - Messaging (send, edit, delete, get messages and threads)
    /// - Read tracking (mark read, unread counts)
    /// - Search (messages and conversations)
    /// - Reactions and Pins
    /// 
    /// User resolution is handled through IMessagingUserResolver, allowing each module
    /// to provide its own user lookup implementation.
    /// 
    /// </summary>
    public class ConversationService
    {
        private readonly IMessagingUserResolver _userResolver;
        private readonly MemoryCacheManager _cache;

        private const float MEMBERSHIP_CACHE_TTL_MINUTES = 5f;
        private const float TYPE_CACHE_TTL_MINUTES = 60f;
        private const float MESSAGES_CACHE_TTL_MINUTES = 3f;
        private const float CONVERSATION_LIST_CACHE_TTL_MINUTES = 2f;


        public ConversationService(IMessagingUserResolver userResolver)
        {
            _userResolver = userResolver;
            _cache = new MemoryCacheManager();
        }


        #region DTOs / Summary Classes

        public class ConversationSummary
        {
            public int id { get; set; }
            public string conversationType { get; set; }
            public int? conversationTypeId { get; set; }
            public int priority { get; set; }
            public DateTime dateTimeCreated { get; set; }
            public string entity { get; set; }
            public int? entityId { get; set; }
            public string externalURL { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public bool isPublic { get; set; }
            public bool isArchived { get; set; }
            public int? createdByUserId { get; set; }
            public string createdByUserName { get; set; }
            public int unreadCount { get; set; }
            public int memberCount { get; set; }
            public bool isMember { get; set; }
            public string lastMessagePreview { get; set; }
            public DateTime? lastMessageDateTime { get; set; }
            public string lastMessageUserName { get; set; }
            public int lastMessageReactionCount { get; set; }
            public List<ConversationMember> members { get; set; }
        }

        public class ConversationMember
        {
            public int conversationUserId { get; set; }
            public int userId { get; set; }
            public string displayName { get; set; }
            public string accountName { get; set; }
            public DateTime dateTimeAdded { get; set; }
        }

        public class MessageSummary
        {
            public int id { get; set; }
            public int conversationId { get; set; }
            public int? conversationChannelId { get; set; }
            public int userId { get; set; }
            public string userDisplayName { get; set; }
            public int? parentConversationMessageId { get; set; }
            public DateTime dateTimeCreated { get; set; }
            public string message { get; set; }
            public string entity { get; set; }
            public int? entityId { get; set; }
            public string externalURL { get; set; }
            public int versionNumber { get; set; }
            public bool acknowledged { get; set; }
            public int replyCount { get; set; }
            public List<AttachmentSummary> attachments { get; set; }
            public List<ReactionSummary> reactions { get; set; }
            public int? forwardedFromMessageId { get; set; }
            public string forwardedFromUserDisplayName { get; set; }
            public List<LinkPreviewService.LinkPreviewSummary> linkPreviews { get; set; }
        }

        public class AttachmentSummary
        {
            public int id { get; set; }
            public string fileName { get; set; }
            public string mimeType { get; set; }
            public long contentLength { get; set; }
            public Guid objectGuid { get; set; }
        }

        public class ReactionSummary
        {
            public string reaction { get; set; }
            public int count { get; set; }
            public List<string> userNames { get; set; }
            public bool currentUserReacted { get; set; }
        }

        public class UnreadCountSummary
        {
            public int conversationId { get; set; }
            public int unreadCount { get; set; }
        }

        public class ReactionResult
        {
            public int reactionId { get; set; }
            public int conversationId { get; set; }
            public int userId { get; set; }
            public string userDisplayName { get; set; }
            public string reaction { get; set; }
            public string action { get; set; }   // "added" or "removed"
        }

        public class PinResult
        {
            public int pinId { get; set; }
            public int conversationId { get; set; }
            public int conversationMessageId { get; set; }
            public int pinnedByUserId { get; set; }
            public string pinnedByUserName { get; set; }
            public DateTime dateTimePinned { get; set; }
            public string messagePreview { get; set; }
        }

        /// <summary>
        /// Input DTO for attachment metadata when sending a message.
        /// Carries the storage GUID along with the original file name, MIME type, and size.
        /// </summary>
        public class AttachmentInput
        {
            public Guid attachmentGuid { get; set; }
            public string fileName { get; set; }
            public string mimeType { get; set; }
            public long contentSize { get; set; }
        }

        #endregion


        #region Conversation Lifecycle

        /// <summary>
        /// Creates a direct message conversation between the current user and one or more recipients.
        /// </summary>
        public async Task<ConversationSummary> CreateDirectMessageAsync(SecurityUser securityUser, List<string> recipientAccountNames, string initialMessage = null)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            if (recipientAccountNames == null || recipientAccountNames.Count == 0)
            {
                throw new Exception("At least one recipient is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser senderUser = await _userResolver.GetUserAsync(securityUser);

                if (senderUser == null)
                {
                    throw new Exception("Could not resolve sending user.");
                }

                //
                // Resolve all recipient users
                //
                List<MessagingUser> recipientUsers = new List<MessagingUser>();

                foreach (string accountName in recipientAccountNames)
                {
                    MessagingUser recipient = await _userResolver.GetUserByAccountNameAsync(accountName, securityUser.securityTenant.objectGuid);

                    if (recipient == null)
                    {
                        throw new Exception($"Could not find user with account name '{accountName}'.");
                    }

                    recipientUsers.Add(recipient);
                }


                //
                // Get the "Direct Message" conversation type
                //
                int? directMessageTypeId = await GetConversationTypeIdAsync(db, "Direct Message");

                if (directMessageTypeId.HasValue == false)
                {
                    throw new Exception("Unable to find conversation type for 'Direct Message'");
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    //
                    // Create the conversation record
                    //
                    Conversation conversation = new Conversation();
                    conversation.createdByUserId = senderUser.id;
                    conversation.conversationTypeId = directMessageTypeId;
                    conversation.priority = 100;
                    conversation.dateTimeCreated = DateTime.UtcNow;
                    conversation.userId = recipientUsers.Count == 1 ? recipientUsers[0].id : (int?)null;
                    conversation.tenantGuid = securityUser.securityTenant.objectGuid;
                    conversation.objectGuid = Guid.NewGuid();
                    conversation.active = true;
                    conversation.deleted = false;

                    db.Conversations.Add(conversation);
                    await db.SaveChangesAsync();

                    //
                    // Add the sender as a member
                    //
                    AddConversationUserRecord(db, conversation.id, senderUser.id, securityUser.securityTenant.objectGuid);

                    //
                    // Add each recipient as a member
                    //
                    foreach (MessagingUser recipient in recipientUsers)
                    {
                        AddConversationUserRecord(db, conversation.id, recipient.id, securityUser.securityTenant.objectGuid);
                    }


                    //
                    // Send the initial message if provided
                    //
                    if (string.IsNullOrWhiteSpace(initialMessage) == false)
                    {
                        CreateMessageRecord(db, conversation.id, senderUser.id, initialMessage, null, securityUser);
                    }


                    await db.SaveChangesAsync();
                    transaction.Commit();

                    return await GetConversationSummaryAsync(securityUser, conversation.id);
                }
            }
        }



        /// <summary>
        /// Creates a conversation linked to a specific entity.
        /// </summary>
        public async Task<ConversationSummary> CreateEntityConversationAsync(SecurityUser securityUser, string entityName, int entityId, List<string> participantAccountNames = null, string initialMessage = null)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new Exception("Entity name is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser senderUser = await _userResolver.GetUserAsync(securityUser);

                if (senderUser == null)
                {
                    throw new Exception("Could not resolve sending user.");
                }

                int? regularTypeId = await GetConversationTypeIdAsync(db, "Regular");


                using (var transaction = db.Database.BeginTransaction())
                {
                    Conversation conversation = new Conversation();
                    conversation.createdByUserId = senderUser.id;
                    conversation.conversationTypeId = regularTypeId;
                    conversation.priority = 100;
                    conversation.dateTimeCreated = DateTime.UtcNow;
                    conversation.entity = entityName;
                    conversation.entityId = entityId;
                    conversation.tenantGuid = securityUser.securityTenant.objectGuid;
                    conversation.objectGuid = Guid.NewGuid();
                    conversation.active = true;
                    conversation.deleted = false;

                    db.Conversations.Add(conversation);
                    await db.SaveChangesAsync();

                    AddConversationUserRecord(db, conversation.id, senderUser.id, securityUser.securityTenant.objectGuid);

                    if (participantAccountNames != null)
                    {
                        foreach (string accountName in participantAccountNames)
                        {
                            MessagingUser participant = await _userResolver.GetUserByAccountNameAsync(accountName, securityUser.securityTenant.objectGuid);

                            if (participant != null)
                            {
                                AddConversationUserRecord(db, conversation.id, participant.id, securityUser.securityTenant.objectGuid);
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(initialMessage) == false)
                    {
                        CreateMessageRecord(db, conversation.id, senderUser.id, initialMessage, null, securityUser);
                    }

                    await db.SaveChangesAsync();
                    transaction.Commit();

                    return await GetConversationSummaryAsync(securityUser, conversation.id);
                }
            }
        }



        /// <summary>
        /// Gets a single conversation with its members and last message preview.
        /// </summary>
        public async Task<ConversationSummary> GetConversationSummaryAsync(SecurityUser securityUser, int conversationId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                    MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                    if (user == null) return null;

                    Conversation conversation = await (from c in db.Conversations
                                                       where
                                                       c.id == conversationId &&
                                                       c.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                       c.active == true &&
                                                       c.deleted == false
                                                       select c)
                                                       .AsNoTracking()
                                                       .FirstOrDefaultAsync();

                    if (conversation == null) return null;

                    bool isMember = await (from cu in db.ConversationUsers
                                           where
                                           cu.conversationId == conversationId &&
                                           cu.userId == user.id &&
                                           cu.active == true &&
                                           cu.deleted == false
                                           select cu)
                                          .AsNoTracking()
                                          .AnyAsync();

                    if (isMember == false) return null;

                    ConversationSummary summary = new ConversationSummary
                    {
                        id = conversation.id,
                        conversationTypeId = conversation.conversationTypeId,
                        priority = conversation.priority,
                        dateTimeCreated = conversation.dateTimeCreated,
                        entity = conversation.entity,
                        entityId = conversation.entityId,
                        externalURL = conversation.externalURL,
                        name = conversation.name,
                        description = conversation.description,
                        isPublic = conversation.isPublic ?? false,
                        isArchived = !conversation.active,
                        createdByUserId = conversation.createdByUserId,
                    };

                    if (conversation.conversationTypeId.HasValue)
                    {
                        ConversationType ct = await (from x in db.ConversationTypes
                                                     where x.id == conversation.conversationTypeId.Value
                                                     select x)
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync();

                        if (ct != null) summary.conversationType = ct.name;
                    }

                    //
                    // Get members - resolve user info through user resolver
                    //
                    List<ConversationUser> memberRecords = await (from cu in db.ConversationUsers
                                                                  where
                                                                  cu.conversationId == conversationId &&
                                                                  cu.active == true &&
                                                                  cu.deleted == false
                                                                  select cu)
                                                                 .AsNoTracking()
                                                                 .ToListAsync();

                    summary.members = new List<ConversationMember>();

                    foreach (ConversationUser cu in memberRecords)
                    {
                        MessagingUser memberUser = await _userResolver.GetUserByIdAsync(cu.userId, securityUser.securityTenant.objectGuid);

                        summary.members.Add(new ConversationMember
                        {
                            conversationUserId = cu.id,
                            userId = cu.userId,
                            displayName = memberUser?.displayName,
                            accountName = memberUser?.accountName,
                            dateTimeAdded = cu.dateTimeAdded
                        });
                    }

                    if (conversation.createdByUserId.HasValue)
                    {
                        ConversationMember creator = summary.members?.FirstOrDefault(m => m.userId == conversation.createdByUserId.Value);
                        summary.createdByUserName = creator?.displayName;
                    }

                    summary.memberCount = summary.members?.Count ?? 0;

                    //
                    // Get last message preview
                    //
                    ConversationMessage lastMsg = await (from cm in db.ConversationMessages
                                                         where
                                                         cm.conversationId == conversationId &&
                                                         cm.active == true &&
                                                         cm.deleted == false
                                                         orderby cm.dateTimeCreated descending
                                                         select cm)
                                                        .AsNoTracking()
                                                        .FirstOrDefaultAsync();

                    if (lastMsg != null)
                    {
                        MessagingUser lastMsgUser = await _userResolver.GetUserByIdAsync(lastMsg.userId, securityUser.securityTenant.objectGuid);

                        summary.lastMessagePreview = StripHtmlForPreview(lastMsg.message, 100);
                        summary.lastMessageDateTime = lastMsg.dateTimeCreated;
                        summary.lastMessageUserName = lastMsgUser?.displayName;
                    }

                    summary.unreadCount = await GetUnreadCountForConversationInternalAsync(db, conversationId, user.id);

                    return summary;
                }
        }



        /// <summary>
        /// Gets all conversations that the current user is a member of.
        /// </summary>
        public async Task<List<ConversationSummary>> GetConversationsForUserAsync(SecurityUser securityUser, bool includeArchived = false)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            //
            // Check conversation list cache
            //
            string convListCacheKey = $"msg_convlist:{securityUser.securityTenant.objectGuid}:{securityUser.accountName}:{(includeArchived ? 1 : 0)}";
            var cachedList = _cache.Get<List<ConversationSummary>>(convListCacheKey);
            if (cachedList != null)
            {
                return cachedList;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                if (user == null) return null;

                Guid tenantGuid = securityUser.securityTenant.objectGuid;

                //
                // 1. Get all conversation IDs the user is a member of
                //
                List<int> conversationIds = await (from cu in db.ConversationUsers
                                                   where
                                                   cu.userId == user.id &&
                                                   cu.tenantGuid == tenantGuid &&
                                                   cu.active == true &&
                                                   cu.deleted == false
                                                   select cu.conversationId)
                                                  .ToListAsync();

                if (conversationIds.Count == 0) return new List<ConversationSummary>();

                //
                // 2. Batch-load all conversation records
                //
                List<Conversation> conversations = await (from c in db.Conversations
                                                           where
                                                           conversationIds.Contains(c.id) &&
                                                           c.tenantGuid == tenantGuid &&
                                                           (includeArchived || c.active == true) &&
                                                           c.deleted == false
                                                           select c)
                                                          .AsNoTracking()
                                                          .ToListAsync();

                if (conversations.Count == 0) return new List<ConversationSummary>();

                List<int> activeConversationIds = conversations.Select(c => c.id).ToList();

                //
                // 3. Batch-load all members across all conversations
                //
                List<ConversationUser> allMemberRecords = await (from cu in db.ConversationUsers
                                                                  where
                                                                  activeConversationIds.Contains(cu.conversationId) &&
                                                                  cu.active == true &&
                                                                  cu.deleted == false
                                                                  select cu)
                                                                 .AsNoTracking()
                                                                 .ToListAsync();

                var membersByConversation = allMemberRecords
                    .GroupBy(cu => cu.conversationId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                //
                // 4. Batch-load conversation types
                //
                List<int> typeIds = conversations
                    .Where(c => c.conversationTypeId.HasValue)
                    .Select(c => c.conversationTypeId.Value)
                    .Distinct()
                    .ToList();

                Dictionary<int, string> typeNames = new Dictionary<int, string>();
                if (typeIds.Count > 0)
                {
                    typeNames = (await (from ct in db.ConversationTypes
                                        where typeIds.Contains(ct.id)
                                        select ct)
                                       .AsNoTracking()
                                       .ToListAsync())
                                      .ToDictionary(ct => ct.id, ct => ct.name);
                }

                //
                // 5. Batch-load last message per conversation (using a subquery approach)
                //    Get the most recent message for each conversation in one query
                //
                var lastMessageIds = await (from cm in db.ConversationMessages
                                             where
                                             activeConversationIds.Contains(cm.conversationId) &&
                                             cm.active == true &&
                                             cm.deleted == false
                                             group cm by cm.conversationId into g
                                             select new { ConversationId = g.Key, MaxId = g.Max(x => x.id) })
                                            .ToListAsync();

                Dictionary<int, ConversationMessage> lastMessages = new Dictionary<int, ConversationMessage>();
                if (lastMessageIds.Count > 0)
                {
                    List<int> maxIds = lastMessageIds.Select(x => x.MaxId).ToList();
                    lastMessages = (await (from cm in db.ConversationMessages
                                            where maxIds.Contains(cm.id)
                                            select cm)
                                           .AsNoTracking()
                                           .ToListAsync())
                                          .ToDictionary(cm => cm.conversationId);
                }

                //
                // 6. Batch-load unread counts per conversation
                //
                var unreadCounts = (await (from cmu in db.ConversationMessageUsers
                                           join cm in db.ConversationMessages on cmu.conversationMessageId equals cm.id
                                           where
                                           activeConversationIds.Contains(cm.conversationId) &&
                                           cmu.userId == user.id &&
                                           cmu.acknowledged == false &&
                                           cmu.active == true &&
                                           cmu.deleted == false &&
                                           cm.active == true &&
                                           cm.deleted == false
                                           group cmu by cm.conversationId into g
                                           select new { ConversationId = g.Key, Count = g.Count() })
                                          .ToListAsync())
                                         .ToDictionary(x => x.ConversationId, x => x.Count);


                //
                // Assemble summaries from batch-loaded data
                //
                List<ConversationSummary> summaries = new List<ConversationSummary>();

                foreach (Conversation conversation in conversations)
                {
                    ConversationSummary summary = new ConversationSummary
                    {
                        id = conversation.id,
                        conversationTypeId = conversation.conversationTypeId,
                        priority = conversation.priority,
                        dateTimeCreated = conversation.dateTimeCreated,
                        entity = conversation.entity,
                        entityId = conversation.entityId,
                        externalURL = conversation.externalURL,
                        name = conversation.name,
                        description = conversation.description,
                        isPublic = conversation.isPublic ?? false,
                        isArchived = !conversation.active,
                        createdByUserId = conversation.createdByUserId,
                    };

                    // Conversation type name
                    if (conversation.conversationTypeId.HasValue && typeNames.TryGetValue(conversation.conversationTypeId.Value, out string typeName))
                    {
                        summary.conversationType = typeName;
                    }

                    // Members — resolve display names through the user resolver (cached)
                    summary.members = new List<ConversationMember>();
                    if (membersByConversation.TryGetValue(conversation.id, out List<ConversationUser> members))
                    {
                        foreach (ConversationUser cu in members)
                        {
                            MessagingUser memberUser = await _userResolver.GetUserByIdAsync(cu.userId, tenantGuid);
                            summary.members.Add(new ConversationMember
                            {
                                conversationUserId = cu.id,
                                userId = cu.userId,
                                displayName = memberUser?.displayName,
                                accountName = memberUser?.accountName,
                                dateTimeAdded = cu.dateTimeAdded
                            });
                        }
                    }

                    if (conversation.createdByUserId.HasValue)
                    {
                        ConversationMember creator = summary.members?.FirstOrDefault(m => m.userId == conversation.createdByUserId.Value);
                        summary.createdByUserName = creator?.displayName;
                    }

                    summary.memberCount = summary.members?.Count ?? 0;

                    // Last message preview
                    if (lastMessages.TryGetValue(conversation.id, out ConversationMessage lastMsg))
                    {
                        MessagingUser lastMsgUser = await _userResolver.GetUserByIdAsync(lastMsg.userId, tenantGuid);

                        summary.lastMessagePreview = StripHtmlForPreview(lastMsg.message, 100);
                        summary.lastMessageDateTime = lastMsg.dateTimeCreated;
                        summary.lastMessageUserName = lastMsgUser?.displayName;

                        // Reaction count for the last message
                        int reactionCount = await (from r in db.ConversationMessageReactions
                                                   where r.conversationMessageId == lastMsg.id && r.active == true
                                                   select r).CountAsync();
                        summary.lastMessageReactionCount = reactionCount;
                    }

                    // Unread count
                    unreadCounts.TryGetValue(conversation.id, out int unread);
                    summary.unreadCount = unread;

                    summaries.Add(summary);
                }

                var result = summaries.OrderByDescending(s => s.lastMessageDateTime ?? s.dateTimeCreated).ToList();

                //
                // Store in conversation list cache
                //
                if (result.Count > 0)
                {
                    _cache.Set(convListCacheKey, result, CONVERSATION_LIST_CACHE_TTL_MINUTES);
                }

                return result;
            }
        }



        /// <summary>
        /// Gets all conversations linked to a specific entity.
        /// </summary>
        public async Task<List<ConversationSummary>> GetConversationsForEntityAsync(SecurityUser securityUser, string entityName, int entityId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                if (user == null) return null;

                List<int> conversationIds = await (from c in db.Conversations
                                                   join cu in db.ConversationUsers on c.id equals cu.conversationId
                                                   where
                                                   c.entity == entityName &&
                                                   c.entityId == entityId &&
                                                   cu.userId == user.id &&
                                                   c.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                   c.active == true &&
                                                   c.deleted == false &&
                                                   cu.active == true &&
                                                   cu.deleted == false
                                                   select c.id)
                                                  .ToListAsync();

                List<ConversationSummary> summaries = new List<ConversationSummary>();

                foreach (int conversationId in conversationIds)
                {
                    ConversationSummary summary = await GetConversationSummaryAsync(securityUser, conversationId);
                    if (summary != null) summaries.Add(summary);
                }

                return summaries.OrderByDescending(s => s.lastMessageDateTime ?? s.dateTimeCreated).ToList();
            }
        }


        /// <summary>
        /// Archives a conversation (soft delete).
        /// </summary>
        public async Task<bool> ArchiveConversationAsync(SecurityUser securityUser, int conversationId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                Conversation conversation = await (from c in db.Conversations
                                                    where
                                                    c.id == conversationId &&
                                                    c.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                    c.active == true &&
                                                    c.deleted == false
                                                    select c)
                                                   .FirstOrDefaultAsync();

                if (conversation == null)
                {
                    throw new Exception("Could not find conversation.");
                }

                conversation.active = false;
                await db.SaveChangesAsync();

                return true;
            }
        }


        /// <summary>
        /// Renames a conversation.
        /// </summary>
        public async Task<bool> RenameConversationAsync(SecurityUser securityUser, int conversationId, string newName)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new Exception("Conversation name cannot be empty.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                Conversation conversation = await (from c in db.Conversations
                                                    where
                                                    c.id == conversationId &&
                                                    c.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                    c.active == true &&
                                                    c.deleted == false
                                                    select c)
                                                   .FirstOrDefaultAsync();

                if (conversation == null)
                {
                    throw new Exception("Could not find conversation.");
                }

                conversation.name = newName.Trim();
                await db.SaveChangesAsync();

                // Invalidate cache
                string cacheKey = $"conversations_{securityUser.securityTenant.objectGuid}_{securityUser.id}";
                _cache.Remove(cacheKey);

                return true;
            }
        }

        #endregion


        #region Membership

        /// <summary>
        /// Adds a user to a conversation.
        /// </summary>
        public async Task<bool> AddUserToConversationAsync(SecurityUser securityUser, int conversationId, string recipientAccountName)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser recipientUser = await _userResolver.GetUserByAccountNameAsync(recipientAccountName, securityUser.securityTenant.objectGuid);

                if (recipientUser == null)
                {
                    throw new Exception($"Could not find user with account name '{recipientAccountName}'.");
                }

                bool alreadyMember = await (from cu in db.ConversationUsers
                                             where
                                             cu.conversationId == conversationId &&
                                             cu.userId == recipientUser.id &&
                                             cu.active == true &&
                                             cu.deleted == false
                                             select cu)
                                            .AnyAsync();

                if (alreadyMember) return true;

                AddConversationUserRecord(db, conversationId, recipientUser.id, securityUser.securityTenant.objectGuid);
                await db.SaveChangesAsync();

                EvictMembershipCache(conversationId);

                return true;
            }
        }


        /// <summary>
        /// Removes a user from a conversation.
        /// </summary>
        public async Task<bool> RemoveUserFromConversationAsync(SecurityUser securityUser, int conversationId, int userIdToRemove)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                ConversationUser cu = await (from x in db.ConversationUsers
                                              where
                                              x.conversationId == conversationId &&
                                              x.userId == userIdToRemove &&
                                              x.active == true &&
                                              x.deleted == false
                                              select x)
                                             .FirstOrDefaultAsync();

                if (cu == null)
                {
                    throw new Exception("User is not a member of this conversation.");
                }

                cu.active = false;
                cu.deleted = true;
                await db.SaveChangesAsync();

                EvictMembershipCache(conversationId);

                return true;
            }
        }


        /// <summary>
        /// Gets the list of members in a conversation.
        /// Uses a 5-minute cache to avoid repeated DB queries.
        /// </summary>
        public async Task<List<ConversationMember>> GetConversationMembersAsync(SecurityUser securityUser, int conversationId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            //
            // Check cache first
            //
            string cacheKey = $"msg_conv_members:{conversationId}";

            var cached = _cache.Get<List<ConversationMember>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            using (MessagingContext db = new MessagingContext())
            {
                List<ConversationUser> memberRecords = await (from cu in db.ConversationUsers
                                                              where
                                                              cu.conversationId == conversationId &&
                                                              cu.active == true &&
                                                              cu.deleted == false
                                                              select cu)
                                                             .AsNoTracking()
                                                             .ToListAsync();

                List<ConversationMember> members = new List<ConversationMember>();

                foreach (ConversationUser cu in memberRecords)
                {
                    MessagingUser memberUser = await _userResolver.GetUserByIdAsync(cu.userId, securityUser.securityTenant.objectGuid);

                    members.Add(new ConversationMember
                    {
                        conversationUserId = cu.id,
                        userId = cu.userId,
                        displayName = memberUser?.displayName,
                        accountName = memberUser?.accountName,
                        dateTimeAdded = cu.dateTimeAdded
                    });
                }

                _cache.Set(cacheKey, members, MEMBERSHIP_CACHE_TTL_MINUTES);

                return members;
            }
        }

        #endregion


        #region Messaging

        /// <summary>
        /// Sends a new message in a conversation.
        /// </summary>
        public async Task<MessageSummary> SendMessageAsync(SecurityUser securityUser, int conversationId, string message, int? parentMessageId = null, string entity = null, int? entityId = null, List<AttachmentInput> attachments = null, int? channelId = null)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            if (string.IsNullOrWhiteSpace(message) && (attachments == null || attachments.Count == 0))
            {
                throw new Exception("Message content or at least one attachment is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser senderUser = await _userResolver.GetUserAsync(securityUser);

                if (senderUser == null)
                {
                    throw new Exception("Could not resolve sending user.");
                }

                bool isMember = await (from cu in db.ConversationUsers
                                       where
                                       cu.conversationId == conversationId &&
                                       cu.userId == senderUser.id &&
                                       cu.active == true &&
                                       cu.deleted == false
                                       select cu)
                                      .AnyAsync();

                if (isMember == false)
                {
                    throw new Exception("User is not a member of this conversation.");
                }


                using (var transaction = db.Database.BeginTransaction())
                {
                    ConversationMessage newMessage = CreateMessageRecord(db, conversationId, senderUser.id, message, parentMessageId, securityUser, entity, entityId, channelId);

                    //
                    // Save now so the message gets its DB-generated ID before we reference it in ConversationMessageUser records
                    //
                    await db.SaveChangesAsync();

                    //
                    // Create ConversationMessageUser records for all other members (for read tracking)
                    //
                    List<int> otherMemberIds = await (from cu in db.ConversationUsers
                                                      where
                                                      cu.conversationId == conversationId &&
                                                      cu.userId != senderUser.id &&
                                                      cu.active == true &&
                                                      cu.deleted == false
                                                      select cu.userId)
                                                     .ToListAsync();

                    foreach (int memberId in otherMemberIds)
                    {
                        ConversationMessageUser cmu = new ConversationMessageUser();
                        cmu.conversationMessageId = newMessage.id;
                        cmu.userId = memberId;
                        cmu.dateTimeCreated = DateTime.UtcNow;
                        cmu.acknowledged = false;
                        cmu.dateTimeAcknowledged = DateTime.MinValue;
                        cmu.tenantGuid = securityUser.securityTenant.objectGuid;
                        cmu.objectGuid = Guid.NewGuid();
                        cmu.active = true;
                        cmu.deleted = false;

                        db.ConversationMessageUsers.Add(cmu);
                    }

                    //
                    // Create ConversationMessageAttachment records for any uploaded attachments
                    //
                    List<AttachmentSummary> attachmentSummaries = new List<AttachmentSummary>();

                    if (attachments != null && attachments.Count > 0)
                    {
                        foreach (AttachmentInput attachmentInput in attachments)
                        {
                            ConversationMessageAttachment attachment = new ConversationMessageAttachment();
                            attachment.conversationMessageId = newMessage.id;
                            attachment.userId = senderUser.id;
                            attachment.dateTimeCreated = DateTime.UtcNow;
                            attachment.contentFileName = attachmentInput.fileName ?? attachmentInput.attachmentGuid.ToString();
                            attachment.contentSize = attachmentInput.contentSize;
                            attachment.contentData = new byte[0];     // content is stored on disk via IAttachmentStorageProvider, not inline
                            attachment.contentMimeType = attachmentInput.mimeType ?? "application/octet-stream";
                            attachment.versionNumber = 1;
                            attachment.objectGuid = attachmentInput.attachmentGuid;
                            attachment.tenantGuid = securityUser.securityTenant.objectGuid;
                            attachment.active = true;
                            attachment.deleted = false;

                            db.ConversationMessageAttachments.Add(attachment);

                            attachmentSummaries.Add(new AttachmentSummary
                            {
                                fileName = attachment.contentFileName,
                                mimeType = attachment.contentMimeType,
                                contentLength = attachment.contentSize,
                                objectGuid = attachmentInput.attachmentGuid
                            });
                        }
                    }

                    await db.SaveChangesAsync();
                    transaction.Commit();

                    //
                    // Invalidate message and conversation list caches
                    //
                    EvictMessageCache(conversationId);
                    EvictConversationListCacheForMembers(db, conversationId, securityUser.securityTenant.objectGuid);

                    return new MessageSummary
                    {
                        id = newMessage.id,
                        conversationId = conversationId,
                        conversationChannelId = channelId,
                        userId = senderUser.id,
                        userDisplayName = senderUser.displayName,
                        parentConversationMessageId = parentMessageId,
                        dateTimeCreated = newMessage.dateTimeCreated,
                        message = newMessage.message,
                        entity = entity,
                        entityId = entityId,
                        versionNumber = newMessage.versionNumber,
                        acknowledged = true,
                        replyCount = 0,
                        attachments = attachmentSummaries,
                        reactions = new List<ReactionSummary>(),
                        forwardedFromMessageId = newMessage.forwardedFromMessageId,
                        linkPreviews = new List<LinkPreviewService.LinkPreviewSummary>()
                    };
                }
            }
        }


        /// <summary>
        /// Forwards an existing message to a different conversation, preserving provenance.
        /// Creates a new message record in the target conversation with the forwarding metadata set.
        /// </summary>
        public async Task<MessageSummary> ForwardMessageAsync(SecurityUser securityUser, int sourceMessageId, int targetConversationId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser senderUser = await _userResolver.GetUserAsync(securityUser);
                if (senderUser == null)
                {
                    throw new Exception("Could not resolve sending user.");
                }

                bool isMember = await (from cu in db.ConversationUsers
                                       where
                                       cu.conversationId == targetConversationId &&
                                       cu.userId == senderUser.id &&
                                       cu.active == true &&
                                       cu.deleted == false
                                       select cu)
                                      .AnyAsync();

                if (isMember == false)
                {
                    throw new Exception("User is not a member of the target conversation.");
                }

                ConversationMessage sourceMessage = await (from cm in db.ConversationMessages
                                                           where
                                                           cm.id == sourceMessageId &&
                                                           cm.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                           cm.active == true &&
                                                           cm.deleted == false
                                                           select cm)
                                                          .FirstOrDefaultAsync();

                if (sourceMessage == null)
                {
                    throw new Exception("Source message not found.");
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    ConversationMessage forwardedMessage = CreateMessageRecord(db, targetConversationId, senderUser.id, sourceMessage.message, null, securityUser);
                    forwardedMessage.forwardedFromMessageId = sourceMessageId;
                    forwardedMessage.forwardedFromUserId = sourceMessage.userId;

                    await db.SaveChangesAsync();

                    List<int> otherMemberIds = await (from cu in db.ConversationUsers
                                                      where
                                                      cu.conversationId == targetConversationId &&
                                                      cu.userId != senderUser.id &&
                                                      cu.active == true &&
                                                      cu.deleted == false
                                                      select cu.userId)
                                                     .ToListAsync();

                    foreach (int memberId in otherMemberIds)
                    {
                        ConversationMessageUser cmu = new ConversationMessageUser();
                        cmu.conversationMessageId = forwardedMessage.id;
                        cmu.userId = memberId;
                        cmu.dateTimeCreated = DateTime.UtcNow;
                        cmu.acknowledged = false;
                        cmu.dateTimeAcknowledged = DateTime.MinValue;
                        cmu.tenantGuid = securityUser.securityTenant.objectGuid;
                        cmu.objectGuid = Guid.NewGuid();
                        cmu.active = true;
                        cmu.deleted = false;

                        db.ConversationMessageUsers.Add(cmu);
                    }

                    await db.SaveChangesAsync();
                    transaction.Commit();

                    EvictMessageCache(targetConversationId);
                    EvictConversationListCacheForMembers(db, targetConversationId, securityUser.securityTenant.objectGuid);

                    MessagingUser originalSender = await _userResolver.GetUserByIdAsync(sourceMessage.userId, securityUser.securityTenant.objectGuid);

                    return new MessageSummary
                    {
                        id = forwardedMessage.id,
                        conversationId = targetConversationId,
                        userId = senderUser.id,
                        userDisplayName = senderUser.displayName,
                        dateTimeCreated = forwardedMessage.dateTimeCreated,
                        message = forwardedMessage.message,
                        versionNumber = forwardedMessage.versionNumber,
                        acknowledged = true,
                        replyCount = 0,
                        attachments = new List<AttachmentSummary>(),
                        reactions = new List<ReactionSummary>(),
                        forwardedFromMessageId = sourceMessageId,
                        forwardedFromUserDisplayName = originalSender?.displayName,
                        linkPreviews = new List<LinkPreviewService.LinkPreviewSummary>()
                    };
                }
            }
        }



        /// <summary>
        /// Edits an existing message.
        /// </summary>
        public async Task<MessageSummary> EditMessageAsync(SecurityUser securityUser, int messageId, string newContent)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            if (string.IsNullOrWhiteSpace(newContent))
            {
                throw new Exception("Message content is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser senderUser = await _userResolver.GetUserAsync(securityUser);
                if (senderUser == null)
                {
                    throw new Exception("Could not resolve user.");
                }

                ConversationMessage msg = await (from cm in db.ConversationMessages
                                                  where
                                                  cm.id == messageId &&
                                                  cm.userId == senderUser.id &&
                                                  cm.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                  cm.active == true &&
                                                  cm.deleted == false
                                                  select cm)
                                                 .FirstOrDefaultAsync();

                if (msg == null)
                {
                    throw new Exception("Could not find message, or you are not the author.");
                }

                //
                // Save change history record manually (Foundation-level, no module-specific toolset)
                //
                ConversationMessageChangeHistory ch = new ConversationMessageChangeHistory();
                ch.conversationMessageId = msg.id;
                ch.versionNumber = msg.versionNumber;
                ch.timeStamp = DateTime.UtcNow;
                ch.userId = senderUser.id;
                ch.data = msg.message;
                ch.tenantGuid = securityUser.securityTenant.objectGuid;
                db.ConversationMessageChangeHistories.Add(ch);

                msg.message = newContent;
                msg.versionNumber = msg.versionNumber + 1;

                await db.SaveChangesAsync();

                //
                // Invalidate message cache for this conversation
                //
                EvictMessageCache(msg.conversationId);

                return new MessageSummary
                {
                    id = msg.id,
                    conversationId = msg.conversationId,
                    userId = msg.userId,
                    userDisplayName = senderUser.displayName,
                    parentConversationMessageId = msg.parentConversationMessageId,
                    dateTimeCreated = msg.dateTimeCreated,
                    message = msg.message,
                    entity = msg.entity,
                    entityId = msg.entityId,
                    versionNumber = msg.versionNumber,
                    acknowledged = true,
                    replyCount = 0,
                    linkPreviews = new List<LinkPreviewService.LinkPreviewSummary>()
                };
            }
        }



        /// <summary>
        /// Soft-deletes a message.  Only the message author can delete.
        /// </summary>
        public async Task<bool> DeleteMessageAsync(SecurityUser securityUser, int messageId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser senderUser = await _userResolver.GetUserAsync(securityUser);

                if (senderUser == null)
                {
                    throw new Exception("Could not resolve user.");
                }

                ConversationMessage msg = await (from cm in db.ConversationMessages
                                                  where
                                                  cm.id == messageId &&
                                                  cm.userId == senderUser.id &&
                                                  cm.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                  cm.active == true &&
                                                  cm.deleted == false
                                                  select cm)
                                                 .FirstOrDefaultAsync();

                if (msg == null)
                {
                    throw new Exception("Could not find message, or you are not the author.");
                }

                msg.active = false;
                msg.deleted = true;
                await db.SaveChangesAsync();

                //
                // Invalidate message and conversation list caches
                //
                EvictMessageCache(msg.conversationId);
                EvictConversationListCacheForMembers(db, msg.conversationId, securityUser.securityTenant.objectGuid);

                return true;
            }
        }


        /// <summary>
        /// Returns the conversationId for a given message.
        /// Used by the controller to determine the correct SignalR group for broadcasting events.
        /// </summary>
        public async Task<int> GetConversationIdForMessageAsync(int messageId)
        {
            using (MessagingContext db = new MessagingContext())
            {
                int conversationId = await (from cm in db.ConversationMessages
                                            where cm.id == messageId
                                            select cm.conversationId)
                                           .FirstOrDefaultAsync();

                return conversationId;
            }
        }


        /// <summary>
        /// Gets messages in a conversation with pagination.
        /// </summary>
        public async Task<List<MessageSummary>> GetMessagesAsync(SecurityUser securityUser, int conversationId, int page = 1, int pageSize = 50, DateTime? beforeDateTime = null, int? channelId = null)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            //
            // Check message cache (only for requests without a beforeDateTime cursor, since those are deterministic)
            // Key includes accountName because the 'acknowledged' field is per-user
            //
            string msgCacheKey = null;
            if (beforeDateTime.HasValue == false)
            {
                msgCacheKey = $"msg_messages:{securityUser.securityTenant.objectGuid}:{securityUser.accountName}:{conversationId}:p{page}:ps{pageSize}:ch{channelId ?? 0}";
                var cached = _cache.Get<List<MessageSummary>>(msgCacheKey);
                if (cached != null)
                {
                    return cached;
                }
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                if (user == null) return null;

                bool isMember = await (from cu in db.ConversationUsers
                                       where
                                       cu.conversationId == conversationId &&
                                       cu.userId == user.id &&
                                       cu.active == true &&
                                       cu.deleted == false
                                       select cu)
                                      .AnyAsync();

                if (isMember == false) return null;

                var query = from cm in db.ConversationMessages
                            where
                            cm.conversationId == conversationId &&
                            cm.active == true &&
                            cm.deleted == false
                            select cm;

                //
                // Filter by channel if specified
                //
                if (channelId.HasValue)
                {
                    query = query.Where(x => x.conversationChannelId == channelId.Value);
                }

                if (beforeDateTime.HasValue)
                {
                    query = query.Where(x => x.dateTimeCreated < beforeDateTime.Value);
                }

                var rawMessages = await query
                    .OrderByDescending(x => x.dateTimeCreated)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();


                //
                // Batch pre-fetch all related data for the page of messages.
                // This replaces per-message DB queries (N+1) with a fixed number of batch queries.
                //
                List<int> messageIds = rawMessages.Select(m => m.id).ToList();


                //
                // 1. Batch acknowledgements — which of these messages has the current user already read?
                //
                HashSet<int> acknowledgedMessageIds = new HashSet<int>(
                    await (from cmu in db.ConversationMessageUsers
                           where
                           messageIds.Contains(cmu.conversationMessageId) &&
                           cmu.userId == user.id &&
                           cmu.acknowledged == true
                           select cmu.conversationMessageId)
                          .ToListAsync());


                //
                // 2. Batch reply counts — how many replies does each message have?
                //
                Dictionary<int, int> replyCounts = (await (from cm in db.ConversationMessages
                                                            where
                                                            cm.parentConversationMessageId.HasValue &&
                                                            messageIds.Contains(cm.parentConversationMessageId.Value) &&
                                                            cm.active == true &&
                                                            cm.deleted == false
                                                            group cm by cm.parentConversationMessageId.Value into g
                                                            select new { ParentId = g.Key, Count = g.Count() })
                                                           .ToListAsync())
                                                          .ToDictionary(x => x.ParentId, x => x.Count);


                //
                // 3. Batch attachments for all messages on this page
                //
                List<ConversationMessageAttachment> allAttachments = await (from a in db.ConversationMessageAttachments
                                                                            where
                                                                            messageIds.Contains(a.conversationMessageId) &&
                                                                            a.active == true &&
                                                                            a.deleted == false
                                                                            select a)
                                                                           .AsNoTracking()
                                                                           .ToListAsync();

                Dictionary<int, List<AttachmentSummary>> attachmentsByMessageId = allAttachments
                    .GroupBy(a => a.conversationMessageId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(a => new AttachmentSummary
                        {
                            id = a.id,
                            fileName = a.contentFileName,
                            mimeType = a.contentMimeType,
                            contentLength = a.contentSize,
                            objectGuid = a.objectGuid
                        }).ToList()
                    );


                //
                // 4. Batch reactions for all messages on this page
                //
                List<ConversationMessageReaction> allReactions = await (from r in db.ConversationMessageReactions
                                                                        where
                                                                        messageIds.Contains(r.conversationMessageId) &&
                                                                        r.active == true &&
                                                                        r.deleted == false
                                                                        select r)
                                                                       .AsNoTracking()
                                                                       .ToListAsync();

                Dictionary<int, List<ConversationMessageReaction>> reactionsByMessageId = allReactions
                    .GroupBy(r => r.conversationMessageId)
                    .ToDictionary(g => g.Key, g => g.ToList());


                //
                // Batch-load link previews for all messages
                //
                LinkPreviewService linkPreviewService = new LinkPreviewService();
                Dictionary<int, List<LinkPreviewService.LinkPreviewSummary>> linkPreviewsByMessageId = await linkPreviewService.GetPreviewsForMessagesAsync(messageIds);


                //
                // Assemble MessageSummary results from the batch-loaded data
                //
                List<MessageSummary> messages = new List<MessageSummary>();

                foreach (var rm in rawMessages)
                {
                    MessagingUser msgUser = await _userResolver.GetUserByIdAsync(rm.userId, securityUser.securityTenant.objectGuid);

                    //
                    // Acknowledged = sender's own messages are always acknowledged,
                    // otherwise check the batch-loaded set
                    //
                    bool isAcknowledged = rm.userId == user.id || acknowledgedMessageIds.Contains(rm.id);

                    replyCounts.TryGetValue(rm.id, out int replyCount);

                    attachmentsByMessageId.TryGetValue(rm.id, out List<AttachmentSummary> attachmentSummaries);

                    //
                    // Build reaction summaries from batch-loaded reactions
                    //
                    List<ReactionSummary> reactionSummaries = new List<ReactionSummary>();

                    if (reactionsByMessageId.TryGetValue(rm.id, out List<ConversationMessageReaction> messageReactions))
                    {
                        var grouped = messageReactions.GroupBy(r => r.reaction);

                        foreach (var group in grouped)
                        {
                            List<string> reactionUserNames = new List<string>();

                            foreach (var reactionRecord in group)
                            {
                                MessagingUser reactionUser = await _userResolver.GetUserByIdAsync(reactionRecord.userId, securityUser.securityTenant.objectGuid);
                                reactionUserNames.Add(reactionUser?.displayName ?? "Unknown");
                            }

                            reactionSummaries.Add(new ReactionSummary
                            {
                                reaction = group.Key,
                                count = group.Count(),
                                userNames = reactionUserNames,
                                currentUserReacted = group.Any(r => r.userId == user.id)
                            });
                        }
                    }

                    messages.Add(new MessageSummary
                    {
                        id = rm.id,
                        conversationId = rm.conversationId,
                        conversationChannelId = rm.conversationChannelId,
                        userId = rm.userId,
                        userDisplayName = msgUser?.displayName,
                        parentConversationMessageId = rm.parentConversationMessageId,
                        dateTimeCreated = rm.dateTimeCreated,
                        message = rm.message,
                        entity = rm.entity,
                        entityId = rm.entityId,
                        versionNumber = rm.versionNumber,
                        acknowledged = isAcknowledged,
                        replyCount = replyCount,
                        attachments = attachmentSummaries ?? new List<AttachmentSummary>(),
                        reactions = reactionSummaries,
                        forwardedFromMessageId = rm.forwardedFromMessageId,
                        forwardedFromUserDisplayName = rm.forwardedFromUserId.HasValue
                            ? (await _userResolver.GetUserByIdAsync(rm.forwardedFromUserId.Value, securityUser.securityTenant.objectGuid))?.displayName
                            : null,
                        linkPreviews = linkPreviewsByMessageId.ContainsKey(rm.id) ? linkPreviewsByMessageId[rm.id] : new List<LinkPreviewService.LinkPreviewSummary>()
                    });
                }

                messages.Reverse();

                //
                // Store in cache if this was a cacheable request
                //
                if (msgCacheKey != null && messages.Count > 0)
                {
                    _cache.Set(msgCacheKey, messages, MESSAGES_CACHE_TTL_MINUTES);
                }

                return messages;
            }
        }



        /// <summary>
        /// Gets all replies to a specific message (a thread).
        /// </summary>
        public async Task<List<MessageSummary>> GetThreadAsync(SecurityUser securityUser, int parentMessageId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                List<ConversationMessage> threadMsgs = await (from cm in db.ConversationMessages
                                                              where
                                                              cm.parentConversationMessageId == parentMessageId &&
                                                              cm.active == true &&
                                                              cm.deleted == false
                                                              orderby cm.dateTimeCreated ascending
                                                              select cm)
                                                             .AsNoTracking()
                                                             .ToListAsync();

                //
                // Batch-resolve all users in the thread
                //
                List<int> userIds = threadMsgs.Select(cm => cm.userId).Distinct().ToList();
                List<MessagingUser> users = await _userResolver.GetUsersByIdsAsync(userIds, securityUser.securityTenant.objectGuid);
                Dictionary<int, MessagingUser> userLookup = users.ToDictionary(u => u.id, u => u);

                List<MessageSummary> result = new List<MessageSummary>();

                foreach (ConversationMessage cm in threadMsgs)
                {
                    userLookup.TryGetValue(cm.userId, out MessagingUser msgUser);

                    result.Add(new MessageSummary
                    {
                        id = cm.id,
                        conversationId = cm.conversationId,
                        userId = cm.userId,
                        userDisplayName = msgUser?.displayName,
                        parentConversationMessageId = cm.parentConversationMessageId,
                        dateTimeCreated = cm.dateTimeCreated,
                        message = cm.message,
                        entity = cm.entity,
                        entityId = cm.entityId,
                        versionNumber = cm.versionNumber,
                        forwardedFromMessageId = cm.forwardedFromMessageId,
                        forwardedFromUserDisplayName = cm.forwardedFromUserId.HasValue
                            ? (await _userResolver.GetUserByIdAsync(cm.forwardedFromUserId.Value, securityUser.securityTenant.objectGuid))?.displayName
                            : null,
                        linkPreviews = new List<LinkPreviewService.LinkPreviewSummary>()
                    });
                }

                return result;
            }
        }

        #endregion


        #region Read Tracking

        /// <summary>
        /// Marks a specific message as read by the current user.
        /// </summary>
        public async Task<bool> MarkMessageReadAsync(SecurityUser securityUser, int messageId)
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

                ConversationMessageUser cmu = await (from x in db.ConversationMessageUsers
                                                      where
                                                      x.conversationMessageId == messageId &&
                                                      x.userId == user.id &&
                                                      x.acknowledged == false &&
                                                      x.active == true &&
                                                      x.deleted == false
                                                      select x)
                                                     .FirstOrDefaultAsync();

                if (cmu != null)
                {
                    cmu.acknowledged = true;
                    cmu.dateTimeAcknowledged = DateTime.UtcNow;
                    await db.SaveChangesAsync();

                    //
                    // Invalidate caches — read status affects message display and unread counts
                    //
                    ConversationMessage msg = await (from cm in db.ConversationMessages
                                                     where cm.id == messageId
                                                     select cm)
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync();

                    if (msg != null)
                    {
                        EvictMessageCache(msg.conversationId);
                        _cache.RemoveByPattern($"msg_convlist:{securityUser.securityTenant.objectGuid}:{securityUser.accountName}:");
                    }
                }

                return true;
            }
        }


        /// <summary>
        /// Marks all messages in a conversation as read by the current user.
        /// </summary>
        public async Task<bool> MarkConversationReadAsync(SecurityUser securityUser, int conversationId)
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

                List<ConversationMessageUser> unreadMessages = await (from cmu in db.ConversationMessageUsers
                                                                       join cm in db.ConversationMessages on cmu.conversationMessageId equals cm.id
                                                                       where
                                                                       cm.conversationId == conversationId &&
                                                                       cmu.userId == user.id &&
                                                                       cmu.acknowledged == false &&
                                                                       cmu.active == true &&
                                                                       cmu.deleted == false
                                                                       select cmu)
                                                                      .ToListAsync();

                DateTime now = DateTime.UtcNow;

                foreach (ConversationMessageUser cmu in unreadMessages)
                {
                    cmu.acknowledged = true;
                    cmu.dateTimeAcknowledged = now;
                }

                await db.SaveChangesAsync();

                //
                // Invalidate caches — read status changed affects message cache and conversation list
                //
                EvictMessageCache(conversationId);
                _cache.RemoveByPattern($"msg_convlist:{securityUser.securityTenant.objectGuid}:{securityUser.accountName}:");

                return true;
            }
        }


        /// <summary>
        /// Gets unread message counts for all of the current user's conversations.
        /// </summary>
        public async Task<List<UnreadCountSummary>> GetUnreadCountsAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                if (user == null) return null;

                List<int> conversationIds = await (from cu in db.ConversationUsers
                                                   where
                                                   cu.userId == user.id &&
                                                   cu.active == true &&
                                                   cu.deleted == false
                                                   select cu.conversationId)
                                                  .ToListAsync();

                List<UnreadCountSummary> counts = await (from cmu in db.ConversationMessageUsers
                                                          join cm in db.ConversationMessages on cmu.conversationMessageId equals cm.id
                                                          where
                                                          conversationIds.Contains(cm.conversationId) &&
                                                          cmu.userId == user.id &&
                                                          cmu.acknowledged == false &&
                                                          cmu.active == true &&
                                                          cmu.deleted == false &&
                                                          cm.active == true &&
                                                          cm.deleted == false
                                                          group cm by cm.conversationId into g
                                                          select new UnreadCountSummary
                                                          {
                                                              conversationId = g.Key,
                                                              unreadCount = g.Count()
                                                          })
                                                         .AsNoTracking()
                                                         .ToListAsync();

                return counts;
            }
        }

        #endregion


        #region Search

        /// <summary>
        /// Searches messages across the user's conversations.
        /// </summary>
        public async Task<List<MessageSummary>> SearchMessagesAsync(SecurityUser securityUser, string query, int? conversationId = null, string entityName = null, int pageSize = 25)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false || string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                if (user == null) return null;

                List<int> userConversationIds = await (from cu in db.ConversationUsers
                                                       where
                                                       cu.userId == user.id &&
                                                       cu.active == true &&
                                                       cu.deleted == false
                                                       select cu.conversationId)
                                                      .ToListAsync();

                var searchQuery = from cm in db.ConversationMessages
                                  where
                                  userConversationIds.Contains(cm.conversationId) &&
                                  cm.message.Contains(query) &&
                                  cm.tenantGuid == securityUser.securityTenant.objectGuid &&
                                  cm.active == true &&
                                  cm.deleted == false
                                  select cm;

                if (conversationId.HasValue)
                {
                    searchQuery = searchQuery.Where(x => x.conversationId == conversationId.Value);
                }

                if (string.IsNullOrWhiteSpace(entityName) == false)
                {
                    searchQuery = searchQuery.Where(x => x.entity == entityName);
                }

                var results = await searchQuery
                    .OrderByDescending(x => x.dateTimeCreated)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                List<MessageSummary> summaries = new List<MessageSummary>();

                foreach (ConversationMessage r in results)
                {
                    MessagingUser msgUser = await _userResolver.GetUserByIdAsync(r.userId, securityUser.securityTenant.objectGuid);

                    summaries.Add(new MessageSummary
                    {
                        id = r.id,
                        conversationId = r.conversationId,
                        userId = r.userId,
                        userDisplayName = msgUser?.displayName,
                        parentConversationMessageId = r.parentConversationMessageId,
                        dateTimeCreated = r.dateTimeCreated,
                        message = r.message,
                        entity = r.entity,
                        entityId = r.entityId,
                        versionNumber = r.versionNumber,
                        forwardedFromMessageId = r.forwardedFromMessageId,
                        forwardedFromUserDisplayName = r.forwardedFromUserId.HasValue
                            ? (await _userResolver.GetUserByIdAsync(r.forwardedFromUserId.Value, securityUser.securityTenant.objectGuid))?.displayName
                            : null,
                        linkPreviews = new List<LinkPreviewService.LinkPreviewSummary>()
                    });
                }

                return summaries;
            }
        }

        #endregion


        #region Reactions

        /// <summary>
        /// Adds a reaction to a message.
        /// </summary>
        public async Task<ReactionResult> AddReactionAsync(SecurityUser securityUser, int messageId, string reaction)
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

                ConversationMessage msg = await (from cm in db.ConversationMessages
                                                 where cm.id == messageId && cm.active == true && cm.deleted == false
                                                 select cm)
                                                .FirstOrDefaultAsync();

                if (msg == null)
                {
                    throw new Exception("Message not found.");
                }

                bool alreadyReacted = await (from r in db.ConversationMessageReactions
                                              where
                                              r.conversationMessageId == messageId &&
                                              r.userId == user.id &&
                                              r.reaction == reaction &&
                                              r.active == true &&
                                              r.deleted == false
                                              select r)
                                             .AnyAsync();

                if (alreadyReacted)
                {
                    throw new Exception("You have already reacted with this reaction.");
                }

                ConversationMessageReaction newReaction = new ConversationMessageReaction();
                newReaction.conversationMessageId = messageId;
                newReaction.userId = user.id;
                newReaction.reaction = reaction;
                newReaction.dateTimeCreated = DateTime.UtcNow;
                newReaction.tenantGuid = securityUser.securityTenant.objectGuid;
                newReaction.objectGuid = Guid.NewGuid();
                newReaction.active = true;
                newReaction.deleted = false;

                db.ConversationMessageReactions.Add(newReaction);
                await db.SaveChangesAsync();

                return new ReactionResult
                {
                    reactionId = newReaction.id,
                    conversationId = msg.conversationId,
                    userId = user.id,
                    userDisplayName = user.displayName,
                    reaction = reaction
                };
            }
        }


        /// <summary>
        /// Removes a reaction from a message.
        /// </summary>
        public async Task<ReactionResult> RemoveReactionAsync(SecurityUser securityUser, int reactionId)
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

                ConversationMessageReaction existingReaction = await (from r in db.ConversationMessageReactions
                                                                      where
                                                                      r.id == reactionId &&
                                                                      r.userId == user.id &&
                                                                      r.active == true &&
                                                                      r.deleted == false
                                                                      select r)
                                                                     .FirstOrDefaultAsync();

                if (existingReaction == null)
                {
                    throw new Exception("Reaction not found or you do not own it.");
                }

                ConversationMessage msg = await (from cm in db.ConversationMessages
                                                 where cm.id == existingReaction.conversationMessageId
                                                 select cm)
                                                .FirstOrDefaultAsync();

                string reactionText = existingReaction.reaction;

                existingReaction.active = false;
                existingReaction.deleted = true;
                await db.SaveChangesAsync();

                return new ReactionResult
                {
                    reactionId = reactionId,
                    conversationId = msg?.conversationId ?? 0,
                    userId = user.id,
                    userDisplayName = user.displayName,
                    reaction = reactionText
                };
            }
        }



        /// <summary>
        /// Toggles a reaction on a message: removes it if the user already reacted
        /// with the same emoji, otherwise adds it.
        /// </summary>
        public async Task<ReactionResult> ToggleReactionAsync(SecurityUser securityUser, int messageId, string reaction)
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

                ConversationMessage msg = await (from cm in db.ConversationMessages
                                                 where cm.id == messageId && cm.active == true && cm.deleted == false
                                                 select cm)
                                                .FirstOrDefaultAsync();

                if (msg == null)
                {
                    throw new Exception("Message not found.");
                }

                //
                // Check if the user already has this reaction on the message
                //
                ConversationMessageReaction existing = await (from r in db.ConversationMessageReactions
                                                               where
                                                               r.conversationMessageId == messageId &&
                                                               r.userId == user.id &&
                                                               r.reaction == reaction &&
                                                               r.active == true &&
                                                               r.deleted == false
                                                               select r)
                                                              .FirstOrDefaultAsync();

                if (existing != null)
                {
                    //
                    // Remove the existing reaction
                    //
                    int removedId = existing.id;
                    existing.active = false;
                    existing.deleted = true;
                    await db.SaveChangesAsync();

                    return new ReactionResult
                    {
                        reactionId = removedId,
                        conversationId = msg.conversationId,
                        userId = user.id,
                        userDisplayName = user.displayName,
                        reaction = reaction,
                        action = "removed"
                    };
                }
                else
                {
                    //
                    // Add a new reaction
                    //
                    ConversationMessageReaction newReaction = new ConversationMessageReaction();
                    newReaction.conversationMessageId = messageId;
                    newReaction.userId = user.id;
                    newReaction.reaction = reaction;
                    newReaction.dateTimeCreated = DateTime.UtcNow;
                    newReaction.tenantGuid = securityUser.securityTenant.objectGuid;
                    newReaction.objectGuid = Guid.NewGuid();
                    newReaction.active = true;
                    newReaction.deleted = false;

                    db.ConversationMessageReactions.Add(newReaction);
                    await db.SaveChangesAsync();

                    return new ReactionResult
                    {
                        reactionId = newReaction.id,
                        conversationId = msg.conversationId,
                        userId = user.id,
                        userDisplayName = user.displayName,
                        reaction = reaction,
                        action = "added"
                    };
                }
            }
        }

        #endregion


        #region Pins

        /// <summary>
        /// Pins a message in a conversation.
        /// </summary>
        public async Task<PinResult> PinMessageAsync(SecurityUser securityUser, int messageId)
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

                ConversationMessage msg = await (from cm in db.ConversationMessages
                                                 where cm.id == messageId && cm.active == true && cm.deleted == false
                                                 select cm)
                                                .FirstOrDefaultAsync();

                if (msg == null)
                {
                    throw new Exception("Message not found.");
                }

                bool alreadyPinned = await (from p in db.ConversationPins
                                             where
                                             p.conversationMessageId == messageId &&
                                             p.conversationId == msg.conversationId &&
                                             p.active == true &&
                                             p.deleted == false
                                             select p)
                                            .AnyAsync();

                if (alreadyPinned)
                {
                    throw new Exception("Message is already pinned.");
                }

                ConversationPin pin = new ConversationPin();
                pin.conversationId = msg.conversationId;
                pin.conversationMessageId = messageId;
                pin.pinnedByUserId = user.id;
                pin.dateTimePinned = DateTime.UtcNow;
                pin.tenantGuid = securityUser.securityTenant.objectGuid;
                pin.objectGuid = Guid.NewGuid();
                pin.active = true;
                pin.deleted = false;

                db.ConversationPins.Add(pin);
                await db.SaveChangesAsync();

                return new PinResult
                {
                    pinId = pin.id,
                    conversationId = msg.conversationId,
                    conversationMessageId = messageId,
                    pinnedByUserId = user.id,
                    pinnedByUserName = user.displayName,
                    dateTimePinned = pin.dateTimePinned,
                    messagePreview = msg.message?.Length > 100 ? msg.message.Substring(0, 100) + "..." : msg.message
                };
            }
        }


        /// <summary>
        /// Unpins a message.
        /// </summary>
        public async Task<bool> UnpinMessageAsync(SecurityUser securityUser, int pinId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                ConversationPin pin = await (from p in db.ConversationPins
                                              where
                                              p.id == pinId &&
                                              p.tenantGuid == securityUser.securityTenant.objectGuid &&
                                              p.active == true &&
                                              p.deleted == false
                                              select p)
                                             .FirstOrDefaultAsync();

                if (pin == null)
                {
                    throw new Exception("Pin not found.");
                }

                pin.active = false;
                pin.deleted = true;
                await db.SaveChangesAsync();

                return true;
            }
        }


        /// <summary>
        /// Gets all pinned messages in a conversation.
        /// </summary>
        public async Task<List<PinResult>> GetPinnedMessagesAsync(SecurityUser securityUser, int conversationId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                var pins = await (from p in db.ConversationPins
                                  join cm in db.ConversationMessages on p.conversationMessageId equals cm.id
                                  where
                                  p.conversationId == conversationId &&
                                  p.tenantGuid == securityUser.securityTenant.objectGuid &&
                                  p.active == true &&
                                  p.deleted == false
                                  orderby p.dateTimePinned descending
                                  select new { p, cm })
                                 .AsNoTracking()
                                 .ToListAsync();

                //
                // Batch-resolve user info for pinners
                //
                List<int> pinnerIds = pins.Select(p => p.p.pinnedByUserId).Distinct().ToList();
                List<MessagingUser> pinUsers = await _userResolver.GetUsersByIdsAsync(pinnerIds, securityUser.securityTenant.objectGuid);
                Dictionary<int, MessagingUser> pinUserLookup = pinUsers.ToDictionary(u => u.id, u => u);

                List<PinResult> results = new List<PinResult>();

                foreach (var item in pins)
                {
                    pinUserLookup.TryGetValue(item.p.pinnedByUserId, out MessagingUser pinUser);

                    results.Add(new PinResult
                    {
                        pinId = item.p.id,
                        conversationId = item.p.conversationId,
                        conversationMessageId = item.p.conversationMessageId,
                        pinnedByUserId = item.p.pinnedByUserId,
                        pinnedByUserName = pinUser?.displayName,
                        dateTimePinned = item.p.dateTimePinned,
                        messagePreview = item.cm.message?.Length > 100 ? item.cm.message.Substring(0, 100) + "..." : item.cm.message
                    });
                }

                return results;
            }
        }

        #endregion


        #region Internal Helpers

        private async Task<int?> GetConversationTypeIdAsync(MessagingContext db, string typeName)
        {
            string cacheKey = $"msg_conv_type:{typeName}";

            if (_cache.IsSet(cacheKey))
            {
                return _cache.Get<int?>(cacheKey);
            }

            ConversationType ct = await (from x in db.ConversationTypes
                                         where x.name == typeName &&
                                         x.active == true &&
                                         x.deleted == false
                                         select x)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();

            int? result = ct?.id;
            _cache.Set(cacheKey, result, TYPE_CACHE_TTL_MINUTES);

            return result;
        }


        /// <summary>
        /// Evicts the membership cache for a specific conversation.
        /// </summary>
        private void EvictMembershipCache(int conversationId)
        {
            string cacheKey = $"msg_conv_members:{conversationId}";
            _cache.Remove(cacheKey);
        }


        /// <summary>
        /// Evicts the message cache for all pages/channels of a conversation.
        /// </summary>
        private void EvictMessageCache(int conversationId)
        {
            _cache.RemoveByPattern($"msg_messages:.*:{conversationId}:");
        }


        /// <summary>
        /// Evicts the conversation list cache for all members of a conversation.
        /// Called when a new message is sent or deleted, so all members' list caches
        /// reflect the updated last-message preview and unread count.
        /// </summary>
        private void EvictConversationListCacheForMembers(MessagingContext db, int conversationId, Guid tenantGuid)
        {
            // Evict conversation list caches scoped to this tenant.
            // The short TTL (2 min) keeps the blast radius small.
            _cache.RemoveByPattern($"msg_convlist:{tenantGuid}:");
        }


        private void AddConversationUserRecord(MessagingContext db, int conversationId, int userId, Guid tenantGuid)
        {
            ConversationUser cu = new ConversationUser();
            cu.conversationId = conversationId;
            cu.userId = userId;
            cu.dateTimeAdded = DateTime.UtcNow;
            cu.tenantGuid = tenantGuid;
            cu.objectGuid = Guid.NewGuid();
            cu.active = true;
            cu.deleted = false;

            db.ConversationUsers.Add(cu);
        }


        private ConversationMessage CreateMessageRecord(MessagingContext db, int conversationId, int userId, string message, int? parentMessageId, SecurityUser securityUser, string entity = null, int? entityId = null, int? channelId = null)
        {
            ConversationMessage cm = new ConversationMessage();
            cm.conversationId = conversationId;
            cm.conversationChannelId = channelId;
            cm.userId = userId;
            cm.message = message;
            cm.parentConversationMessageId = parentMessageId;
            cm.dateTimeCreated = DateTime.UtcNow;
            cm.entity = entity;
            cm.entityId = entityId;
            cm.versionNumber = 1;
            cm.tenantGuid = securityUser.securityTenant.objectGuid;
            cm.objectGuid = Guid.NewGuid();
            cm.active = true;
            cm.deleted = false;

            db.ConversationMessages.Add(cm);

            return cm;
        }


        private static async Task<int> GetUnreadCountForConversationInternalAsync(MessagingContext db, int conversationId, int userId)
        {
            return await (from cmu in db.ConversationMessageUsers
                          join cm in db.ConversationMessages on cmu.conversationMessageId equals cm.id
                          where
                          cm.conversationId == conversationId &&
                          cmu.userId == userId &&
                          cmu.acknowledged == false &&
                          cmu.active == true &&
                          cmu.deleted == false &&
                          cm.active == true &&
                          cm.deleted == false
                          select cmu)
                         .CountAsync();
        }

        #endregion


        #region Channel Management

        /// <summary>
        /// DTO used to return channel summary information to the UI layer.
        /// </summary>
        public class ChannelSummary
        {
            public int id { get; set; }
            public int conversationId { get; set; }
            public string name { get; set; }
            public string topic { get; set; }
            public bool isPrivate { get; set; }
            public bool isPinned { get; set; }
            public Guid objectGuid { get; set; }
            public int messageCount { get; set; }
            public string lastMessagePreview { get; set; }
        }


        /// <summary>
        /// Creates a new channel within a conversation.
        /// </summary>
        public async Task<ChannelSummary> CreateChannelAsync(SecurityUser securityUser, int conversationId, string name, string topic = null, bool isPrivate = false)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                //
                // Verify the conversation exists and the user is a member.
                //
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                if (user == null)
                {
                    throw new Exception("Could not resolve user.");
                }

                ConversationUser membership = await (from cu in db.ConversationUsers
                                                       where
                                                       cu.conversationId == conversationId &&
                                                       cu.userId == user.id &&
                                                       cu.active == true &&
                                                       cu.deleted == false
                                                       select cu)
                                                      .AsNoTracking()
                                                      .FirstOrDefaultAsync();

                if (membership == null)
                {
                    throw new Exception("User is not a member of this conversation.");
                }


                ConversationChannel channel = new ConversationChannel();

                channel.conversationId = conversationId;
                channel.name = name;
                channel.topic = topic;
                channel.isPrivate = isPrivate;
                channel.isPinned = false;
                channel.versionNumber = 1;
                channel.tenantGuid = securityUser.securityTenant.objectGuid;
                channel.objectGuid = Guid.NewGuid();
                channel.active = true;
                channel.deleted = false;

                db.ConversationChannels.Add(channel);
                await db.SaveChangesAsync();

                return new ChannelSummary
                {
                    id = channel.id,
                    conversationId = channel.conversationId,
                    name = channel.name,
                    topic = channel.topic,
                    isPrivate = channel.isPrivate,
                    isPinned = channel.isPinned,
                    objectGuid = channel.objectGuid,
                };
            }
        }


        /// <summary>
        /// Updates the metadata of an existing channel.
        /// </summary>
        public async Task<ChannelSummary> UpdateChannelAsync(SecurityUser securityUser, int channelId, string name, string topic = null, bool? isPrivate = null)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                ConversationChannel channel = await (from x in db.ConversationChannels
                                                       where
                                                       x.id == channelId &&
                                                       x.active == true &&
                                                       x.deleted == false &&
                                                       x.tenantGuid == securityUser.securityTenant.objectGuid
                                                       select x)
                                                      .FirstOrDefaultAsync();

                if (channel == null)
                {
                    throw new Exception("Channel not found.");
                }

                //
                // Save change history record before applying changes
                //
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                ConversationChannelChangeHistory ch = new ConversationChannelChangeHistory();
                ch.conversationChannelId = channel.id;
                ch.versionNumber = channel.versionNumber;
                ch.timeStamp = DateTime.UtcNow;
                ch.userId = user?.id ?? 0;
                ch.data = System.Text.Json.JsonSerializer.Serialize(new
                {
                    channel.name,
                    channel.topic,
                    channel.isPrivate,
                    channel.isPinned
                });
                ch.tenantGuid = securityUser.securityTenant.objectGuid;
                db.ConversationChannelChangeHistories.Add(ch);

                channel.name = name;

                if (topic != null)
                {
                    channel.topic = topic;
                }

                if (isPrivate.HasValue == true)
                {
                    channel.isPrivate = isPrivate.Value;
                }

                channel.versionNumber++;

                await db.SaveChangesAsync();

                return new ChannelSummary
                {
                    id = channel.id,
                    conversationId = channel.conversationId,
                    name = channel.name,
                    topic = channel.topic,
                    isPrivate = channel.isPrivate,
                    isPinned = channel.isPinned,
                    objectGuid = channel.objectGuid,
                };
            }
        }


        /// <summary>
        /// Soft-deletes a channel by setting deleted = true.
        /// Returns the conversationId for the deleted channel so the caller can broadcast.
        /// </summary>
        public async Task<int> DeleteChannelAsync(SecurityUser securityUser, int channelId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                ConversationChannel channel = await (from x in db.ConversationChannels
                                                       where
                                                       x.id == channelId &&
                                                       x.active == true &&
                                                       x.deleted == false &&
                                                       x.tenantGuid == securityUser.securityTenant.objectGuid
                                                       select x)
                                                      .FirstOrDefaultAsync();

                if (channel == null)
                {
                    throw new Exception("Channel not found.");
                }

                int conversationId = channel.conversationId;

                channel.deleted = true;
                channel.active = false;

                await db.SaveChangesAsync();

                return conversationId;
            }
        }


        /// <summary>
        /// Gets all channels for a specific conversation.
        /// </summary>
        public async Task<List<ChannelSummary>> GetChannelsAsync(SecurityUser securityUser, int conversationId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                List<ConversationChannel> channels = await (from x in db.ConversationChannels
                              where
                              x.conversationId == conversationId &&
                              x.active == true &&
                              x.deleted == false &&
                              x.tenantGuid == securityUser.securityTenant.objectGuid
                              select x)
                             .AsNoTracking()
                             .ToListAsync();

                //
                // Batch-load message counts and last messages for all channels
                //
                List<int> channelIds = channels.Select(c => c.id).ToList();

                Dictionary<int, int> messageCounts = (await (from cm in db.ConversationMessages
                                                             where
                                                             cm.conversationChannelId.HasValue &&
                                                             channelIds.Contains(cm.conversationChannelId.Value) &&
                                                             cm.active == true &&
                                                             cm.deleted == false
                                                             group cm by cm.conversationChannelId.Value into g
                                                             select new { ChannelId = g.Key, Count = g.Count() })
                                                            .ToListAsync())
                                                           .ToDictionary(x => x.ChannelId, x => x.Count);

                var lastMsgIds = await (from cm in db.ConversationMessages
                                        where
                                        cm.conversationChannelId.HasValue &&
                                        channelIds.Contains(cm.conversationChannelId.Value) &&
                                        cm.active == true &&
                                        cm.deleted == false
                                        group cm by cm.conversationChannelId.Value into g
                                        select new { ChannelId = g.Key, MaxId = g.Max(x => x.id) })
                                       .ToListAsync();

                Dictionary<int, ConversationMessage> lastMessages = new Dictionary<int, ConversationMessage>();
                if (lastMsgIds.Count > 0)
                {
                    List<int> maxIds = lastMsgIds.Select(x => x.MaxId).ToList();
                    var lastMsgRecords = await (from cm in db.ConversationMessages
                                                where maxIds.Contains(cm.id)
                                                select cm)
                                               .AsNoTracking()
                                               .ToListAsync();

                    foreach (var lm in lastMsgRecords)
                    {
                        if (lm.conversationChannelId.HasValue)
                        {
                            lastMessages[lm.conversationChannelId.Value] = lm;
                        }
                    }
                }

                List<ChannelSummary> summaries = new List<ChannelSummary>();

                foreach (var channel in channels)
                {
                    messageCounts.TryGetValue(channel.id, out int msgCount);

                    string lastPreview = null;
                    if (lastMessages.TryGetValue(channel.id, out ConversationMessage lastMsg))
                    {
                        lastPreview = StripHtmlForPreview(lastMsg.message, 100);
                    }

                    summaries.Add(new ChannelSummary
                    {
                        id = channel.id,
                        conversationId = channel.conversationId,
                        name = channel.name,
                        topic = channel.topic,
                        isPrivate = channel.isPrivate,
                        isPinned = channel.isPinned,
                        objectGuid = channel.objectGuid,
                        messageCount = msgCount,
                        lastMessagePreview = lastPreview
                    });
                }

                return summaries;
            }
        }

        #endregion


        #region Top-Level Channels

        /// <summary>
        /// Creates a top-level channel conversation that users can discover and join.
        /// </summary>
        public async Task<ConversationSummary> CreateChannelConversationAsync(SecurityUser securityUser, string name, string description, bool isPublic)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("Channel name is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser senderUser = await _userResolver.GetUserAsync(securityUser);
                if (senderUser == null)
                {
                    throw new Exception("Could not resolve current user.");
                }

                int? channelTypeId = await GetConversationTypeIdAsync(db, "Channel");

                using (var transaction = db.Database.BeginTransaction())
                {
                    Conversation conversation = new Conversation();
                    conversation.createdByUserId = senderUser.id;
                    conversation.conversationTypeId = channelTypeId;
                    conversation.priority = 100;
                    conversation.dateTimeCreated = DateTime.UtcNow;
                    conversation.name = name.Trim();
                    conversation.description = description?.Trim();
                    conversation.isPublic = isPublic;
                    conversation.tenantGuid = securityUser.securityTenant.objectGuid;
                    conversation.objectGuid = Guid.NewGuid();
                    conversation.active = true;
                    conversation.deleted = false;

                    db.Conversations.Add(conversation);
                    await db.SaveChangesAsync();

                    // Creator auto-joins the channel
                    AddConversationUserRecord(db, conversation.id, senderUser.id, securityUser.securityTenant.objectGuid);

                    await db.SaveChangesAsync();
                    transaction.Commit();

                    return await GetConversationSummaryAsync(securityUser, conversation.id);
                }
            }
        }


        /// <summary>
        /// Returns all public channel conversations in the tenant that the user can browse/join.
        /// </summary>
        public async Task<List<ConversationSummary>> GetPublicChannelsAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return new List<ConversationSummary>();
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                if (user == null) return new List<ConversationSummary>();

                int? channelTypeId = await GetConversationTypeIdAsync(db, "Channel");

                List<Conversation> channels = await (from c in db.Conversations
                                                      where
                                                      c.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                      c.conversationTypeId == channelTypeId &&
                                                      c.isPublic == true &&
                                                      c.active == true &&
                                                      c.deleted == false
                                                      orderby c.name
                                                      select c)
                                                     .AsNoTracking()
                                                     .ToListAsync();

                //
                // Batch-load membership and member counts for all channels
                //
                List<int> channelConversationIds = channels.Select(c => c.id).ToList();

                HashSet<int> userMembershipIds = new HashSet<int>(
                    await (from cu in db.ConversationUsers
                           where
                           channelConversationIds.Contains(cu.conversationId) &&
                           cu.userId == user.id &&
                           cu.active == true &&
                           cu.deleted == false
                           select cu.conversationId)
                          .ToListAsync());

                Dictionary<int, int> memberCounts = (await (from cu in db.ConversationUsers
                                                            where
                                                            channelConversationIds.Contains(cu.conversationId) &&
                                                            cu.active == true &&
                                                            cu.deleted == false
                                                            group cu by cu.conversationId into g
                                                            select new { ConversationId = g.Key, Count = g.Count() })
                                                           .ToListAsync())
                                                          .ToDictionary(x => x.ConversationId, x => x.Count);

                List<ConversationSummary> summaries = new List<ConversationSummary>();

                foreach (Conversation channel in channels)
                {
                    memberCounts.TryGetValue(channel.id, out int memberCount);

                    summaries.Add(new ConversationSummary
                    {
                        id = channel.id,
                        conversationType = "Channel",
                        conversationTypeId = channel.conversationTypeId,
                        name = channel.name,
                        description = channel.description,
                        isPublic = channel.isPublic ?? false,
                        memberCount = memberCount,
                        dateTimeCreated = channel.dateTimeCreated,
                        createdByUserId = channel.createdByUserId,
                        isMember = userMembershipIds.Contains(channel.id),
                        members = new List<ConversationMember>()
                    });
                }

                return summaries;
            }
        }


        /// <summary>
        /// Joins a channel conversation by adding the user as a member.
        /// </summary>
        public async Task<ConversationSummary> JoinChannelAsync(SecurityUser securityUser, int conversationId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                if (user == null) return null;

                Conversation conversation = await (from c in db.Conversations
                                                    where
                                                    c.id == conversationId &&
                                                    c.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                    c.active == true &&
                                                    c.deleted == false
                                                    select c)
                                                   .FirstOrDefaultAsync();

                if (conversation == null) return null;

                // Check not already a member
                bool alreadyMember = await (from cu in db.ConversationUsers
                                             where
                                             cu.conversationId == conversationId &&
                                             cu.userId == user.id &&
                                             cu.active == true &&
                                             cu.deleted == false
                                             select cu)
                                            .AnyAsync();

                if (!alreadyMember)
                {
                    AddConversationUserRecord(db, conversationId, user.id, securityUser.securityTenant.objectGuid);
                    await db.SaveChangesAsync();
                }

                return await GetConversationSummaryAsync(securityUser, conversationId);
            }
        }


        /// <summary>
        /// Leaves a channel conversation by removing the user's membership.
        /// </summary>
        public async Task<bool> LeaveChannelAsync(SecurityUser securityUser, int conversationId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return false;
            }

            using (MessagingContext db = new MessagingContext())
            {
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                if (user == null) return false;

                ConversationUser membership = await (from cu in db.ConversationUsers
                                                      where
                                                      cu.conversationId == conversationId &&
                                                      cu.userId == user.id &&
                                                      cu.active == true &&
                                                      cu.deleted == false
                                                      select cu)
                                                     .FirstOrDefaultAsync();

                if (membership != null)
                {
                    membership.active = false;
                    membership.deleted = true;
                    await db.SaveChangesAsync();
                }

                return true;
            }
        }

        #endregion


        /// <summary>
        /// Checks whether a user is an active member of a conversation.
        /// Used by MessagingHub to guard SignalR group joins.
        /// </summary>
        public async Task<bool> IsUserInConversationAsync(SecurityUser securityUser, int conversationId)
        {
            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) return false;

            using (MessagingContext db = new MessagingContext())
            {
                return await db.ConversationUsers.AnyAsync(cu =>
                    cu.conversationId == conversationId &&
                    cu.userId == user.id &&
                    cu.active && !cu.deleted);
            }
        }


        /// <summary>
        /// Strips HTML tags from a message and returns a plain-text preview, truncated to maxLength.
        /// Used for conversation list and channel list message previews.
        /// </summary>
        private static string StripHtmlForPreview(string html, int maxLength)
        {
            if (string.IsNullOrEmpty(html)) return html;

            // Remove HTML tags
            string text = Regex.Replace(html, @"<[^>]+>", " ");

            // Decode HTML entities
            text = System.Net.WebUtility.HtmlDecode(text);

            // Collapse whitespace
            text = Regex.Replace(text, @"\s+", " ").Trim();

            if (text.Length > maxLength)
            {
                return text.Substring(0, maxLength) + "...";
            }

            return text;
        }


        /// <summary>
        /// Marks a message as unread for the current user by resetting the acknowledged flag.
        /// This allows users to defer follow-up on messages they've already seen.
        /// </summary>
        public async Task<bool> MarkMessageUnreadAsync(SecurityUser securityUser, int conversationMessageId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);

            if (user == null)
            {
                throw new Exception("Could not resolve user.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                Guid tenantGuid = securityUser.securityTenant.objectGuid;

                //
                // Find the existing ConversationMessageUser record for this user and message
                //
                ConversationMessageUser messageUser = await (from mu in db.ConversationMessageUsers
                                                             where
                                                             mu.conversationMessageId == conversationMessageId &&
                                                             mu.userId == user.id &&
                                                             mu.tenantGuid == tenantGuid &&
                                                             mu.active == true &&
                                                             mu.deleted == false
                                                             select mu)
                                                            .FirstOrDefaultAsync();

                if (messageUser == null)
                {
                    return false;
                }

                messageUser.acknowledged = false;
                await db.SaveChangesAsync();

                return true;
            }
        }


        // =================================================================
        // Phase 2A — Thread Unread Tracking
        // =================================================================


        /// <summary>
        /// Updates the last-read position for a user within a specific thread.
        /// Called when the user views messages in a thread panel, enabling per-thread unread badges.
        /// </summary>
        public async Task UpdateThreadReadPositionAsync(SecurityUser securityUser, int conversationId, int parentMessageId, int lastReadMessageId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) throw new Exception("Could not resolve user.");

            using (MessagingContext db = new MessagingContext())
            {
                Guid tenantGuid = securityUser.securityTenant.objectGuid;

                ConversationThreadUser threadUser = await (from tu in db.ConversationThreadUsers
                                                           where
                                                           tu.conversationId == conversationId &&
                                                           tu.parentConversationMessageId == parentMessageId &&
                                                           tu.userId == user.id &&
                                                           tu.tenantGuid == tenantGuid &&
                                                           tu.active == true &&
                                                           tu.deleted == false
                                                           select tu)
                                                          .FirstOrDefaultAsync();

                if (threadUser == null)
                {
                    threadUser = new ConversationThreadUser
                    {
                        tenantGuid = tenantGuid,
                        conversationId = conversationId,
                        parentConversationMessageId = parentMessageId,
                        userId = user.id,
                        lastReadMessageId = lastReadMessageId,
                        lastReadDateTime = DateTime.UtcNow,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    db.ConversationThreadUsers.Add(threadUser);
                }
                else
                {
                    threadUser.lastReadMessageId = lastReadMessageId;
                    threadUser.lastReadDateTime = DateTime.UtcNow;
                }

                await db.SaveChangesAsync();
            }
        }


        /// <summary>
        /// Gets the unread reply count for a specific thread for the current user.
        /// Returns the number of replies that are newer than the user's last-read position.
        /// </summary>
        public async Task<int> GetThreadUnreadCountAsync(SecurityUser securityUser, int conversationId, int parentMessageId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false) return 0;

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) return 0;

            using (MessagingContext db = new MessagingContext())
            {
                Guid tenantGuid = securityUser.securityTenant.objectGuid;

                ConversationThreadUser threadUser = await (from tu in db.ConversationThreadUsers
                                                           where
                                                           tu.conversationId == conversationId &&
                                                           tu.parentConversationMessageId == parentMessageId &&
                                                           tu.userId == user.id &&
                                                           tu.tenantGuid == tenantGuid &&
                                                           tu.active == true &&
                                                           tu.deleted == false
                                                           select tu)
                                                          .FirstOrDefaultAsync();

                int lastReadId = threadUser?.lastReadMessageId ?? 0;

                return await (from m in db.ConversationMessages
                              where
                              m.conversationId == conversationId &&
                              m.parentConversationMessageId == parentMessageId &&
                              m.id > lastReadId &&
                              m.tenantGuid == tenantGuid &&
                              m.active == true &&
                              m.deleted == false
                              select m)
                             .CountAsync();
            }
        }


        // =================================================================
        // Phase 2B — Channel Admin Roles
        // =================================================================


        /// <summary>
        /// Updates a user's role within a conversation (e.g., Owner, Admin, Member).
        /// Only users with Owner or Admin roles should be able to call this.
        /// </summary>
        public async Task<bool> UpdateUserRoleAsync(SecurityUser securityUser, int conversationId, int targetUserId, string newRole)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                Guid tenantGuid = securityUser.securityTenant.objectGuid;

                ConversationUser targetMembership = await (from cu in db.ConversationUsers
                                                            where
                                                            cu.conversationId == conversationId &&
                                                            cu.userId == targetUserId &&
                                                            cu.tenantGuid == tenantGuid &&
                                                            cu.active == true &&
                                                            cu.deleted == false
                                                            select cu)
                                                           .FirstOrDefaultAsync();

                if (targetMembership == null) return false;

                targetMembership.role = newRole;
                await db.SaveChangesAsync();
                return true;
            }
        }


        /// <summary>
        /// Gets the current user's role within a conversation.
        /// Returns null if the user is not a member.
        /// </summary>
        public async Task<string> GetUserRoleAsync(SecurityUser securityUser, int conversationId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false) return null;

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) return null;

            using (MessagingContext db = new MessagingContext())
            {
                return await (from cu in db.ConversationUsers
                              where
                              cu.conversationId == conversationId &&
                              cu.userId == user.id &&
                              cu.tenantGuid == securityUser.securityTenant.objectGuid &&
                              cu.active == true &&
                              cu.deleted == false
                              select cu.role)
                             .FirstOrDefaultAsync();
            }
        }


        // =================================================================
        // Phase 2C — Message Bookmarks
        // =================================================================


        /// <summary>
        /// Adds a bookmark for the current user on a specific message.
        /// Returns the created bookmark, or null if the message doesn't exist.
        /// </summary>
        public async Task<MessageBookmark> AddBookmarkAsync(SecurityUser securityUser, int conversationMessageId, string note)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) throw new Exception("Could not resolve user.");

            using (MessagingContext db = new MessagingContext())
            {
                Guid tenantGuid = securityUser.securityTenant.objectGuid;

                // Check if already bookmarked
                bool alreadyBookmarked = await (from b in db.MessageBookmarks
                                                 where
                                                 b.userId == user.id &&
                                                 b.conversationMessageId == conversationMessageId &&
                                                 b.tenantGuid == tenantGuid &&
                                                 b.active == true &&
                                                 b.deleted == false
                                                 select b)
                                                .AnyAsync();

                if (alreadyBookmarked) return null;

                MessageBookmark bookmark = new MessageBookmark
                {
                    tenantGuid = tenantGuid,
                    userId = user.id,
                    conversationMessageId = conversationMessageId,
                    note = note,
                    dateTimeCreated = DateTime.UtcNow,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                db.MessageBookmarks.Add(bookmark);
                await db.SaveChangesAsync();

                return bookmark;
            }
        }


        /// <summary>
        /// Removes a bookmark by its ID for the current user.
        /// </summary>
        public async Task<bool> RemoveBookmarkAsync(SecurityUser securityUser, int bookmarkId)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("User with tenant is required.");
            }

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) throw new Exception("Could not resolve user.");

            using (MessagingContext db = new MessagingContext())
            {
                MessageBookmark bookmark = await (from b in db.MessageBookmarks
                                                   where
                                                   b.id == bookmarkId &&
                                                   b.userId == user.id &&
                                                   b.tenantGuid == securityUser.securityTenant.objectGuid &&
                                                   b.active == true &&
                                                   b.deleted == false
                                                   select b)
                                                  .FirstOrDefaultAsync();

                if (bookmark == null) return false;

                bookmark.active = false;
                bookmark.deleted = true;
                await db.SaveChangesAsync();

                return true;
            }
        }


        /// <summary>
        /// Gets all active bookmarks for the current user.
        /// </summary>
        public async Task<List<MessageBookmark>> GetBookmarksAsync(SecurityUser securityUser)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return new List<MessageBookmark>();
            }

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) return new List<MessageBookmark>();

            using (MessagingContext db = new MessagingContext())
            {
                return await (from b in db.MessageBookmarks
                              where
                              b.userId == user.id &&
                              b.tenantGuid == securityUser.securityTenant.objectGuid &&
                              b.active == true &&
                              b.deleted == false
                              orderby b.dateTimeCreated descending
                              select b)
                             .AsNoTracking()
                             .ToListAsync();
            }
        }
    }
}
