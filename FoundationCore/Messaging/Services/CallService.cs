using Foundation.Messaging.Database;
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Foundation Call Service — orchestrates call lifecycle using pluggable ICallProvider implementations.
    /// 
    /// Follows the same pattern as PushDeliveryService:
    ///   - Receives IEnumerable&lt;ICallProvider&gt; via DI
    ///   - Selects provider based on configuration or caller-specified preference
    ///   - Manages Call entity lifecycle (create → ringing → active → ended/missed/declined)
    ///   - Coordinates with PresenceService (set status to "In a Call")
    ///   - Logs all call events to CallEventLog
    ///   - Creates call_event system messages in conversations
    /// 
    /// This is a Foundation-level service that can be used by any module.
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 4 (Calling), March 2026.
    /// 
    /// </summary>
    public class CallService
    {
        private readonly IEnumerable<ICallProvider> _providers;
        private readonly ITurnServerProvider _turnServerProvider;
        private readonly PresenceService _presenceService;
        private readonly IMessagingUserResolver _userResolver;

        //
        // Default call timeout: how long to ring before marking as missed
        //
        private const int DEFAULT_RING_TIMEOUT_SECONDS = 45;

        //
        // Presence status for users in a call
        //
        public const string PRESENCE_STATUS_IN_CALL = "InCall";


        public CallService(
            IEnumerable<ICallProvider> providers,
            PresenceService presenceService,
            IMessagingUserResolver userResolver,
            ITurnServerProvider turnServerProvider = null)
        {
            _providers = providers;
            _turnServerProvider = turnServerProvider;
            _presenceService = presenceService;
            _userResolver = userResolver;
        }


        #region DTOs

        public class CallSummary
        {
            public int id { get; set; }
            public Guid tenantGuid { get; set; }
            public string callType { get; set; }
            public string callStatus { get; set; }
            public string providerId { get; set; }
            public string providerCallId { get; set; }
            public int conversationId { get; set; }
            public int initiatorUserId { get; set; }
            public string initiatorDisplayName { get; set; }
            public DateTime startDateTime { get; set; }
            public DateTime? answerDateTime { get; set; }
            public DateTime? endDateTime { get; set; }
            public int? durationSeconds { get; set; }
            public List<CallParticipantSummary> participants { get; set; }
            public CallProviderCapabilities providerCapabilities { get; set; }
        }

        public class CallParticipantSummary
        {
            public int id { get; set; }
            public int userId { get; set; }
            public string displayName { get; set; }
            public string role { get; set; }
            public string status { get; set; }
            public DateTime? joinedDateTime { get; set; }
            public DateTime? leftDateTime { get; set; }
        }

        #endregion


        #region Provider Selection


        /// <summary>
        /// Gets the default (first enabled) call provider.
        /// </summary>
        public ICallProvider GetDefaultProvider()
        {
            return _providers.FirstOrDefault(p => p.IsEnabled);
        }


        /// <summary>
        /// Gets a specific call provider by ID.
        /// </summary>
        public ICallProvider GetProvider(string providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId))
            {
                return GetDefaultProvider();
            }

            return _providers.FirstOrDefault(p => p.ProviderId == providerId && p.IsEnabled);
        }


        /// <summary>
        /// Gets the capabilities of the active provider (for UI adaptation).
        /// </summary>
        public CallProviderCapabilities GetProviderCapabilities(string providerId = null)
        {
            ICallProvider provider = GetProvider(providerId);
            return provider?.Capabilities;
        }


        /// <summary>
        /// Lists all registered providers and their status.
        /// </summary>
        public List<ProviderInfo> GetRegisteredProviders()
        {
            return _providers.Select(p => new ProviderInfo
            {
                ProviderId = p.ProviderId,
                DisplayName = p.DisplayName,
                IsEnabled = p.IsEnabled,
                Capabilities = p.Capabilities,
                IsConfigurationValid = p.ValidateConfiguration()
            }).ToList();
        }


        public class ProviderInfo
        {
            public string ProviderId { get; set; }
            public string DisplayName { get; set; }
            public bool IsEnabled { get; set; }
            public CallProviderCapabilities Capabilities { get; set; }
            public bool IsConfigurationValid { get; set; }
        }

        #endregion


        #region Call Lifecycle


        /// <summary>
        /// Initiates a call.  Creates the Call record, adds participants, and notifies the provider.
        /// Returns the call summary which includes the call ID and connection info.
        /// </summary>
        public async Task<CallSummary> InitiateCallAsync(
            SecurityUser securityUser,
            int conversationId,
            string callType,
            List<int> recipientUserIds,
            string preferredProviderId = null,
            CancellationToken cancellationToken = default)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("Authenticated user with tenant is required to initiate a call.");
            }

            //
            // Resolve initiator
            //
            MessagingUser initiator = await _userResolver.GetUserAsync(securityUser);
            if (initiator == null)
            {
                throw new Exception("Could not resolve initiator user.");
            }

            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            //
            // Select provider
            //
            ICallProvider provider = GetProvider(preferredProviderId);
            if (provider == null)
            {
                throw new Exception("No enabled call provider is available.");
            }

            using (MessagingContext db = new MessagingContext())
            {
                //
                // Check for existing active/ringing calls in this conversation
                // Only consider calls started within the last 2 minutes as truly active
                // (older ones are stale from disconnected sessions)
                //
                int ringingStatusId = await GetCallStatusIdAsync(db, "Ringing");
                int activeStatusId = await GetCallStatusIdAsync(db, "Active");

                DateTime staleThreshold = DateTime.UtcNow.AddMinutes(-2);

                //
                // Auto-end any stale ringing/active calls (older than 2 minutes)
                //
                int endedStatusId = await GetCallStatusIdAsync(db, "Ended");

                List<Call> staleCalls = await (from c in db.Calls
                                               where c.conversationId == conversationId &&
                                                     c.tenantGuid == tenantGuid &&
                                                     (c.callStatusId == ringingStatusId || c.callStatusId == activeStatusId) &&
                                                     c.active == true &&
                                                     c.deleted == false &&
                                                     c.startDateTime < staleThreshold
                                               select c).ToListAsync(cancellationToken);

                foreach (Call staleCall in staleCalls)
                {
                    staleCall.callStatusId = endedStatusId;
                    staleCall.endDateTime = DateTime.UtcNow;
                }

                if (staleCalls.Count > 0)
                {
                    await db.SaveChangesAsync(cancellationToken);
                }


                //
                // Now check for genuinely active calls (recent ones)
                //
                bool existingActiveCall = await (from c in db.Calls
                                                  where c.conversationId == conversationId &&
                                                        c.tenantGuid == tenantGuid &&
                                                        (c.callStatusId == ringingStatusId || c.callStatusId == activeStatusId) &&
                                                        c.active == true &&
                                                        c.deleted == false
                                                  select c.id).AnyAsync(cancellationToken);

                if (existingActiveCall)
                {
                    throw new Exception("There is already an active call in this conversation.");
                }

                //
                // Resolve call type ID
                //
                int callTypeId = await GetCallTypeIdAsync(db, callType);

                //
                // Create the Call record
                //
                Call call = new Call
                {
                    tenantGuid = tenantGuid,
                    callTypeId = callTypeId,
                    callStatusId = ringingStatusId,
                    providerId = provider.ProviderId,
                    conversationId = conversationId,
                    initiatorUserId = initiator.id,
                    startDateTime = DateTime.UtcNow,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                db.Calls.Add(call);
                await db.SaveChangesAsync(cancellationToken);


                //
                // Add the initiator as a participant
                //
                CallParticipant initiatorParticipant = new CallParticipant
                {
                    tenantGuid = tenantGuid,
                    callId = call.id,
                    userId = initiator.id,
                    role = "initiator",
                    status = "joined",
                    joinedDateTime = DateTime.UtcNow,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                db.CallParticipants.Add(initiatorParticipant);


                //
                // If no explicit recipient list was provided, resolve from conversation members.
                // This is the typical case — the client just specifies the conversationId and
                // all other participants in that conversation become call recipients.
                //
                if (recipientUserIds == null || recipientUserIds.Count == 0)
                {
                    recipientUserIds = await db.ConversationUsers
                        .Where(cu => cu.conversationId == conversationId && cu.active == true && cu.deleted == false)
                        .Select(cu => cu.userId)
                        .ToListAsync(cancellationToken);
                }


                //
                // Add recipients as participants in "ringing" status
                //
                foreach (int recipientUserId in recipientUserIds)
                {
                    if (recipientUserId == initiator.id) continue;

                    CallParticipant recipientParticipant = new CallParticipant
                    {
                        tenantGuid = tenantGuid,
                        callId = call.id,
                        userId = recipientUserId,
                        role = "recipient",
                        status = "ringing",
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    db.CallParticipants.Add(recipientParticipant);
                }

                await db.SaveChangesAsync(cancellationToken);


                //
                // Notify the provider (e.g. create ACS room)
                //
                CallProviderResult providerResult = await provider.InitializeCallAsync(new CallInitRequest
                {
                    CallId = call.id,
                    ConversationId = conversationId,
                    InitiatorUserId = initiator.id,
                    ParticipantUserIds = recipientUserIds,
                    CallType = callType,
                    TenantGuid = tenantGuid
                }, cancellationToken);


                //
                // Store provider's call ID if returned
                //
                if (providerResult.Success && !string.IsNullOrWhiteSpace(providerResult.ProviderCallId))
                {
                    call.providerCallId = providerResult.ProviderCallId;
                    await db.SaveChangesAsync(cancellationToken);
                }


                //
                // Log the initiation event
                //
                await LogCallEventAsync(db, call.id, tenantGuid, "initiated", initiator.id, provider.ProviderId,
                    $"{{\"callType\":\"{callType}\",\"recipientCount\":{recipientUserIds.Count}}}", cancellationToken);


                //
                // Build and return the summary
                //
                return await BuildCallSummaryAsync(db, call, provider, tenantGuid, cancellationToken);
            }
        }


        /// <summary>
        /// Accepts an incoming call.  Updates the participant status and call state.
        /// </summary>
        public async Task<CallSummary> AcceptCallAsync(
            SecurityUser securityUser,
            int callId,
            CancellationToken cancellationToken = default)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("Authenticated user with tenant is required.");
            }

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) throw new Exception("Could not resolve user.");

            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                Call call = await GetActiveCallAsync(db, callId, tenantGuid);
                if (call == null) throw new Exception("Call not found.");

                //
                // Update participant status
                //
                CallParticipant participant = await GetParticipantAsync(db, callId, user.id, tenantGuid);
                if (participant == null) throw new Exception("User is not a participant in this call.");

                participant.status = "joined";
                participant.joinedDateTime = DateTime.UtcNow;


                //
                // Update call status to Active (if still ringing)
                //
                int activeStatusId = await GetCallStatusIdAsync(db, "Active");
                int ringingStatusId = await GetCallStatusIdAsync(db, "Ringing");

                if (call.callStatusId == ringingStatusId)
                {
                    call.callStatusId = activeStatusId;
                    call.answerDateTime = DateTime.UtcNow;
                }

                await db.SaveChangesAsync(cancellationToken);

                await LogCallEventAsync(db, callId, tenantGuid, "joined", user.id, call.providerId, null, cancellationToken);


                ICallProvider provider = GetProvider(call.providerId);
                return await BuildCallSummaryAsync(db, call, provider, tenantGuid, cancellationToken);
            }
        }


        /// <summary>
        /// Declines an incoming call.
        /// </summary>
        public async Task<CallSummary> DeclineCallAsync(
            SecurityUser securityUser,
            int callId,
            CancellationToken cancellationToken = default)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("Authenticated user with tenant is required.");
            }

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) throw new Exception("Could not resolve user.");

            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                Call call = await GetActiveCallAsync(db, callId, tenantGuid);
                if (call == null) throw new Exception("Call not found.");

                CallParticipant participant = await GetParticipantAsync(db, callId, user.id, tenantGuid);
                if (participant == null) throw new Exception("User is not a participant in this call.");

                participant.status = "declined";

                //
                // If all recipients have declined, end the call
                //
                bool allDeclined = await AreAllRecipientsDeclinedOrMissedAsync(db, callId, tenantGuid);

                if (allDeclined)
                {
                    int declinedStatusId = await GetCallStatusIdAsync(db, "Declined");
                    call.callStatusId = declinedStatusId;
                    call.endDateTime = DateTime.UtcNow;
                }

                await db.SaveChangesAsync(cancellationToken);

                await LogCallEventAsync(db, callId, tenantGuid, "declined", user.id, call.providerId, null, cancellationToken);


                ICallProvider provider = GetProvider(call.providerId);
                return await BuildCallSummaryAsync(db, call, provider, tenantGuid, cancellationToken);
            }
        }


        /// <summary>
        /// Ends an active call.
        /// </summary>
        public async Task<CallSummary> EndCallAsync(
            SecurityUser securityUser,
            int callId,
            CancellationToken cancellationToken = default)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                throw new Exception("Authenticated user with tenant is required.");
            }

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) throw new Exception("Could not resolve user.");

            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                Call call = await GetActiveCallAsync(db, callId, tenantGuid);
                if (call == null) throw new Exception("Call not found.");


                //
                // End the call
                //
                int endedStatusId = await GetCallStatusIdAsync(db, "Ended");
                call.callStatusId = endedStatusId;
                call.endDateTime = DateTime.UtcNow;

                if (call.answerDateTime.HasValue)
                {
                    call.durationSeconds = (int)(DateTime.UtcNow - call.answerDateTime.Value).TotalSeconds;
                }


                //
                // Mark all active participants as left
                //
                List<CallParticipant> activeParticipants = await (from cp in db.CallParticipants
                                                                  where
                                                                  cp.callId == callId &&
                                                                  cp.tenantGuid == tenantGuid &&
                                                                  cp.status == "joined" &&
                                                                  cp.active == true &&
                                                                  cp.deleted == false
                                                                  select cp).ToListAsync(cancellationToken);

                foreach (CallParticipant participant in activeParticipants)
                {
                    participant.status = "left";
                    participant.leftDateTime = DateTime.UtcNow;
                }


                //
                // Mark any still-ringing participants as missed
                //
                List<CallParticipant> ringingParticipants = await (from cp in db.CallParticipants
                                                                   where
                                                                   cp.callId == callId &&
                                                                   cp.tenantGuid == tenantGuid &&
                                                                   cp.status == "ringing" &&
                                                                   cp.active == true &&
                                                                   cp.deleted == false
                                                                   select cp).ToListAsync(cancellationToken);

                foreach (CallParticipant participant in ringingParticipants)
                {
                    participant.status = "missed";
                }


                await db.SaveChangesAsync(cancellationToken);


                //
                // Notify provider to clean up
                //
                ICallProvider provider = GetProvider(call.providerId);
                if (provider != null)
                {
                    await provider.TerminateCallAsync(new CallTerminateRequest
                    {
                        CallId = callId,
                        ProviderCallId = call.providerCallId,
                        TenantGuid = tenantGuid
                    }, cancellationToken);
                }


                await LogCallEventAsync(db, callId, tenantGuid, "ended", user.id, call.providerId,
                    $"{{\"durationSeconds\":{call.durationSeconds ?? 0}}}", cancellationToken);

                return await BuildCallSummaryAsync(db, call, provider, tenantGuid, cancellationToken);
            }
        }


        /// <summary>
        /// Gets provider-specific connection info for a participant (ICE servers, tokens, etc.).
        /// </summary>
        public async Task<CallConnectionInfo> GetConnectionInfoAsync(
            SecurityUser securityUser,
            int callId,
            CancellationToken cancellationToken = default)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return new CallConnectionInfo { Success = false, ErrorMessage = "Authentication required." };
            }

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null)
            {
                return new CallConnectionInfo { Success = false, ErrorMessage = "Could not resolve user." };
            }

            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                Call call = await GetActiveCallAsync(db, callId, tenantGuid);
                if (call == null)
                {
                    return new CallConnectionInfo { Success = false, ErrorMessage = "Call not found." };
                }

                ICallProvider provider = GetProvider(call.providerId);
                if (provider == null)
                {
                    return new CallConnectionInfo { Success = false, ErrorMessage = "Call provider not available." };
                }

                //
                // Get call type name for the request
                //
                string callTypeName = await GetCallTypeNameAsync(db, call.callTypeId);

                return await provider.GetConnectionInfoAsync(new CallConnectionRequest
                {
                    CallId = callId,
                    UserId = user.id,
                    TenantGuid = tenantGuid,
                    CallType = callTypeName
                }, cancellationToken);
            }
        }

        #endregion


        #region Queries


        /// <summary>
        /// Gets a call by ID.
        /// </summary>
        public async Task<CallSummary> GetCallAsync(
            SecurityUser securityUser,
            int callId,
            CancellationToken cancellationToken = default)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false) return null;

            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                Call call = await (from c in db.Calls
                                   where
                                   c.id == callId &&
                                   c.tenantGuid == tenantGuid &&
                                   c.active == true &&
                                   c.deleted == false
                                   select c)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(cancellationToken);

                if (call == null) return null;

                ICallProvider provider = GetProvider(call.providerId);
                return await BuildCallSummaryAsync(db, call, provider, tenantGuid, cancellationToken);
            }
        }


        /// <summary>
        /// Gets recent calls for the current user.
        /// </summary>
        public async Task<List<CallSummary>> GetRecentCallsAsync(
            SecurityUser securityUser,
            int maxResults = 50,
            CancellationToken cancellationToken = default)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return new List<CallSummary>();
            }

            MessagingUser user = await _userResolver.GetUserAsync(securityUser);
            if (user == null) return new List<CallSummary>();

            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                //
                // Find calls where this user is a participant
                //
                List<int> callIds = await (from cp in db.CallParticipants
                                           where
                                           cp.userId == user.id &&
                                           cp.tenantGuid == tenantGuid &&
                                           cp.active == true &&
                                           cp.deleted == false
                                           orderby cp.id descending
                                           select cp.callId)
                                           .Distinct()
                                           .Take(maxResults)
                                           .ToListAsync(cancellationToken);

                List<Call> calls = await (from c in db.Calls
                                          where
                                          callIds.Contains(c.id) &&
                                          c.tenantGuid == tenantGuid &&
                                          c.active == true &&
                                          c.deleted == false
                                          orderby c.startDateTime descending
                                          select c)
                                         .AsNoTracking()
                                         .ToListAsync(cancellationToken);

                List<CallSummary> summaries = new List<CallSummary>();
                foreach (Call call in calls)
                {
                    ICallProvider provider = GetProvider(call.providerId);
                    summaries.Add(await BuildCallSummaryAsync(db, call, provider, tenantGuid, cancellationToken));
                }

                return summaries;
            }
        }


        /// <summary>
        /// Gets call history for a specific conversation.
        /// </summary>
        public async Task<List<CallSummary>> GetConversationCallsAsync(
            SecurityUser securityUser,
            int conversationId,
            int maxResults = 50,
            CancellationToken cancellationToken = default)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
            {
                return new List<CallSummary>();
            }

            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                List<Call> calls = await (from c in db.Calls
                                          where
                                          c.conversationId == conversationId &&
                                          c.tenantGuid == tenantGuid &&
                                          c.active == true &&
                                          c.deleted == false
                                          orderby c.startDateTime descending
                                          select c)
                                         .AsNoTracking()
                                         .Take(maxResults)
                                         .ToListAsync(cancellationToken);

                List<CallSummary> summaries = new List<CallSummary>();
                foreach (Call call in calls)
                {
                    ICallProvider provider = GetProvider(call.providerId);
                    summaries.Add(await BuildCallSummaryAsync(db, call, provider, tenantGuid, cancellationToken));
                }

                return summaries;
            }
        }

        /// <summary>
        /// Gets the conversation ID for a call (regardless of call status).
        /// Used by the hub fallback when EndCallAsync fails but we still need to broadcast.
        /// </summary>
        public async Task<int> GetCallConversationIdAsync(
            SecurityUser securityUser,
            int callId,
            CancellationToken cancellationToken = default)
        {
            if (securityUser == null || securityUser.securityTenantId.HasValue == false)
                return 0;

            Guid tenantGuid = securityUser.securityTenant.objectGuid;

            using (MessagingContext db = new MessagingContext())
            {
                var call = await (from c in db.Calls
                                  where c.id == callId && c.tenantGuid == tenantGuid && c.deleted == false
                                  select c).FirstOrDefaultAsync(cancellationToken);

                return call?.conversationId ?? 0;
            }
        }


        #endregion


        #region Private Helpers


        private async Task<Call> GetActiveCallAsync(MessagingContext db, int callId, Guid tenantGuid)
        {
            return await (from c in db.Calls
                          where
                          c.id == callId &&
                          c.tenantGuid == tenantGuid &&
                          c.active == true &&
                          c.deleted == false
                          select c)
                          .FirstOrDefaultAsync();
        }


        private async Task<CallParticipant> GetParticipantAsync(MessagingContext db, int callId, int userId, Guid tenantGuid)
        {
            return await (from cp in db.CallParticipants
                          where
                          cp.callId == callId &&
                          cp.userId == userId &&
                          cp.tenantGuid == tenantGuid &&
                          cp.active == true &&
                          cp.deleted == false
                          select cp)
                          .FirstOrDefaultAsync();
        }


        private async Task<bool> AreAllRecipientsDeclinedOrMissedAsync(MessagingContext db, int callId, Guid tenantGuid)
        {
            List<string> recipientStatuses = await (from cp in db.CallParticipants
                                                    where
                                                    cp.callId == callId &&
                                                    cp.tenantGuid == tenantGuid &&
                                                    cp.role == "recipient" &&
                                                    cp.active == true &&
                                                    cp.deleted == false
                                                    select cp.status)
                                                    .ToListAsync();

            return recipientStatuses.Count > 0 &&
                   recipientStatuses.All(s => s == "declined" || s == "missed");
        }


        private async Task<int> GetCallTypeIdAsync(MessagingContext db, string callTypeName)
        {
            CallType callType = await (from ct in db.CallTypes
                                       where ct.name == callTypeName
                                       select ct)
                                       .FirstOrDefaultAsync();

            if (callType == null)
            {
                throw new Exception($"Unknown call type: {callTypeName}");
            }

            return callType.id;
        }


        private async Task<int> GetCallStatusIdAsync(MessagingContext db, string statusName)
        {
            CallStatus callStatus = await (from cs in db.CallStatuses
                                           where cs.name == statusName
                                           select cs)
                                           .FirstOrDefaultAsync();

            if (callStatus == null)
            {
                throw new Exception($"Unknown call status: {statusName}");
            }

            return callStatus.id;
        }


        private async Task<string> GetCallTypeNameAsync(MessagingContext db, int callTypeId)
        {
            return await (from ct in db.CallTypes
                          where ct.id == callTypeId
                          select ct.name)
                          .FirstOrDefaultAsync() ?? "Voice";
        }


        private async Task<string> GetCallStatusNameAsync(MessagingContext db, int callStatusId)
        {
            return await (from cs in db.CallStatuses
                          where cs.id == callStatusId
                          select cs.name)
                          .FirstOrDefaultAsync() ?? "Unknown";
        }


        private async Task LogCallEventAsync(
            MessagingContext db,
            int callId,
            Guid tenantGuid,
            string eventType,
            int? userId,
            string providerId,
            string metadata,
            CancellationToken cancellationToken = default)
        {
            try
            {
                CallEventLog log = new CallEventLog
                {
                    tenantGuid = tenantGuid,
                    callId = callId,
                    eventType = eventType,
                    userId = userId,
                    providerId = providerId,
                    metadata = metadata,
                    dateTimeCreated = DateTime.UtcNow,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                db.CallEventLogs.Add(log);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                //
                // Logging failures are non-fatal — don't break the call flow
                //
                System.Diagnostics.Debug.WriteLine($"Failed to log call event: {ex.Message}");
            }
        }


        private async Task<CallSummary> BuildCallSummaryAsync(
            MessagingContext db,
            Call call,
            ICallProvider provider,
            Guid tenantGuid,
            CancellationToken cancellationToken = default)
        {
            //
            // Get participants
            //
            List<CallParticipant> participants = await (from cp in db.CallParticipants
                                                        where
                                                        cp.callId == call.id &&
                                                        cp.tenantGuid == tenantGuid &&
                                                        cp.active == true &&
                                                        cp.deleted == false
                                                        select cp)
                                                       .AsNoTracking()
                                                       .ToListAsync(cancellationToken);

            //
            // Resolve user display names
            //
            List<int> userIds = participants.Select(p => p.userId).ToList();
            if (!userIds.Contains(call.initiatorUserId))
            {
                userIds.Add(call.initiatorUserId);
            }

            List<MessagingUser> resolvedUsers = await _userResolver.GetUsersByIdsAsync(userIds, tenantGuid);
            Dictionary<int, MessagingUser> userLookup = resolvedUsers.ToDictionary(u => u.id, u => u);


            //
            // Build participant summaries
            //
            List<CallParticipantSummary> participantSummaries = participants.Select(cp => new CallParticipantSummary
            {
                id = cp.id,
                userId = cp.userId,
                displayName = userLookup.TryGetValue(cp.userId, out MessagingUser u) ? u.displayName : "Unknown",
                role = cp.role,
                status = cp.status,
                joinedDateTime = cp.joinedDateTime,
                leftDateTime = cp.leftDateTime
            }).ToList();


            //
            // Get type and status names
            //
            string callTypeName = await GetCallTypeNameAsync(db, call.callTypeId);
            string callStatusName = await GetCallStatusNameAsync(db, call.callStatusId);
            string initiatorName = userLookup.TryGetValue(call.initiatorUserId, out MessagingUser initiatorUser) ? initiatorUser.displayName : "Unknown";


            return new CallSummary
            {
                id = call.id,
                tenantGuid = call.tenantGuid,
                callType = callTypeName,
                callStatus = callStatusName,
                providerId = call.providerId,
                providerCallId = call.providerCallId,
                conversationId = call.conversationId,
                initiatorUserId = call.initiatorUserId,
                initiatorDisplayName = initiatorName,
                startDateTime = call.startDateTime,
                answerDateTime = call.answerDateTime,
                endDateTime = call.endDateTime,
                durationSeconds = call.durationSeconds,
                participants = participantSummaries,
                providerCapabilities = provider?.Capabilities
            };
        }

        #endregion
    }
}
