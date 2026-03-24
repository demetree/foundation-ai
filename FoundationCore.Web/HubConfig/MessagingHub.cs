using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation.Messaging;
using Foundation.Messaging.Services;
using Foundation.Security;
using Foundation.Security.Database;

namespace Foundation.HubConfig
{

    #region Payload Classes

    public class MessagePayload
    {
        public int conversationId { get; set; }
        public int? conversationChannelId { get; set; }
        public int messageId { get; set; }
        public int userId { get; set; }
        public string userDisplayName { get; set; }
        public string message { get; set; }
        public int? parentConversationMessageId { get; set; }
        public string entity { get; set; }
        public int? entityId { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public bool hasAttachments { get; set; }
    }

    public class MessageEditPayload
    {
        public int conversationId { get; set; }
        public int messageId { get; set; }
        public int userId { get; set; }
        public string newContent { get; set; }
        public DateTime dateTimeEdited { get; set; }
    }

    public class MessageDeletePayload
    {
        public int conversationId { get; set; }
        public int messageId { get; set; }
        public int userId { get; set; }
    }

    public class UserJoinPayload
    {
        public int conversationId { get; set; }
        public int userId { get; set; }
        public string userDisplayName { get; set; }
    }

    public class UserLeavePayload
    {
        public int conversationId { get; set; }
        public int userId { get; set; }
        public string userDisplayName { get; set; }
    }

    public class TypingPayload
    {
        public int conversationId { get; set; }
        public int userId { get; set; }
        public string userDisplayName { get; set; }
    }

    public class ReactionPayload
    {
        public int conversationId { get; set; }
        public int messageId { get; set; }
        public int reactionId { get; set; }
        public int userId { get; set; }
        public string userDisplayName { get; set; }
        public string reaction { get; set; }
    }

    public class ReadReceiptPayload
    {
        public int conversationId { get; set; }
        public int messageId { get; set; }
        public int userId { get; set; }
    }

    public class NotificationPayload
    {
        public int notificationDistributionId { get; set; }
        public int notificationId { get; set; }
        public string message { get; set; }
        public string entity { get; set; }
        public int? entityId { get; set; }
        public string notificationType { get; set; }
        public string sendingUserName { get; set; }
        public int priority { get; set; }
        public DateTime dateTimeCreated { get; set; }
    }

    public class PresencePayload
    {
        public int userId { get; set; }
        public string userDisplayName { get; set; }
        public string status { get; set; }
        public string customStatusMessage { get; set; }
    }

    public class ChannelPayload
    {
        public int conversationId { get; set; }
        public int channelId { get; set; }
        public string name { get; set; }
        public string topic { get; set; }
        public bool isPrivate { get; set; }
        public bool isPinned { get; set; }
    }


    //
    // Call signaling payloads
    //

    public class CallOfferPayload
    {
        public int callId { get; set; }
        public int conversationId { get; set; }
        public string callType { get; set; }                // "Voice", "Video", "ScreenShare"
        public int initiatorUserId { get; set; }
        public string initiatorDisplayName { get; set; }
        public string providerId { get; set; }
        public CallProviderCapabilities providerCapabilities { get; set; }
    }

    public class CallAnswerPayload
    {
        public int callId { get; set; }
        public int conversationId { get; set; }
        public int userId { get; set; }
        public string userDisplayName { get; set; }
    }

    public class CallDeclinePayload
    {
        public int callId { get; set; }
        public int conversationId { get; set; }
        public int userId { get; set; }
        public string userDisplayName { get; set; }
    }

    public class CallEndPayload
    {
        public int callId { get; set; }
        public int conversationId { get; set; }
        public int endedByUserId { get; set; }
        public string endedByDisplayName { get; set; }
        public int? durationSeconds { get; set; }
        public string reason { get; set; }                  // "normal", "missed", "declined", "failed"
    }

    public class CallParticipantPayload
    {
        public int callId { get; set; }
        public int conversationId { get; set; }
        public int userId { get; set; }
        public string userDisplayName { get; set; }
        public string action { get; set; }                  // "joined" or "left"
    }

    public class IceCandidatePayload
    {
        public int callId { get; set; }
        public int fromUserId { get; set; }
        public int toUserId { get; set; }
        public string candidate { get; set; }               // JSON-serialized RTCIceCandidate
        public string sdpMid { get; set; }
        public int? sdpMLineIndex { get; set; }
    }

    public class SdpPayload
    {
        public int callId { get; set; }
        public int fromUserId { get; set; }
        public int toUserId { get; set; }
        public string type { get; set; }                    // "offer" or "answer"
        public string sdp { get; set; }                     // The SDP string
    }

    public class CallMissedPayload
    {
        public int callId { get; set; }
        public int conversationId { get; set; }
        public int initiatorUserId { get; set; }
        public string initiatorDisplayName { get; set; }
        public string callType { get; set; }
    }

    public class LinkPreviewReadyPayload
    {
        public int conversationId { get; set; }
        public int messageId { get; set; }
        public System.Collections.Generic.List<LinkPreviewService.LinkPreviewSummary> previews { get; set; }
    }

    public class ScheduledMessageReleasedPayload
    {
        public int conversationId { get; set; }
        public int messageId { get; set; }
        public int userId { get; set; }
        public string messageHtml { get; set; }
        public System.DateTime dateTimeCreated { get; set; }
    }

    #endregion


    /// <summary>
    /// Client-side interface defining the methods that the server can call on connected clients.
    /// Each connected client must implement handlers for these methods.
    /// </summary>
    public interface IMessagingHub
    {
        // Real-time message delivery
        Task ReceiveMessage(MessagePayload message);
        Task ReceiveMessageEdit(MessageEditPayload edit);
        Task ReceiveMessageDelete(MessageDeletePayload delete);

        // Conversation membership changes
        Task UserJoinedConversation(UserJoinPayload payload);
        Task UserLeftConversation(UserLeavePayload payload);

        // Typing indicators
        Task UserTyping(TypingPayload payload);
        Task UserStoppedTyping(TypingPayload payload);

        // Reactions
        Task ReactionAdded(ReactionPayload payload);
        Task ReactionRemoved(ReactionPayload payload);

        // Read receipts
        Task MessageRead(ReadReceiptPayload payload);

        // Notifications (replaces HTTP polling)
        Task ReceiveNotification(NotificationPayload notification);

        // Presence
        Task PresenceChanged(PresencePayload payload);

        // Channel lifecycle events
        Task ChannelCreated(ChannelPayload payload);
        Task ChannelUpdated(ChannelPayload payload);
        Task ChannelDeleted(ChannelPayload payload);

        // Call signaling
        Task IncomingCall(CallOfferPayload payload);
        Task CallAccepted(CallAnswerPayload payload);
        Task CallDeclined(CallDeclinePayload payload);
        Task CallEnded(CallEndPayload payload);
        Task CallParticipantJoined(CallParticipantPayload payload);
        Task CallParticipantLeft(CallParticipantPayload payload);
        Task IceCandidate(IceCandidatePayload payload);
        Task SdpOffer(SdpPayload payload);
        Task SdpAnswer(SdpPayload payload);
        Task CallMissed(CallMissedPayload payload);

        // Link previews
        Task LinkPreviewReady(LinkPreviewReadyPayload payload);

        // Scheduled messages
        Task ScheduledMessageReleased(ScheduledMessageReleasedPayload payload);
    }


    /// <summary>
    /// Foundation Messaging Hub - provides real-time communication for the messaging system.
    /// 
    /// This hub manages:
    /// - Real-time message delivery to conversation participants
    /// - Typing indicators
    /// - Presence status broadcasts
    /// - Notification delivery (replacing HTTP polling)
    /// 
    /// Connection group strategy:
    ///   - conversation:{conversationId}     - all users in a conversation
    ///   - tenant:{tenantGuid}               - all users in a tenant (for broadcasts)
    ///   - user:{userObjectGuid}             - individual user (for DMs, notifications)
    /// 
    /// </summary>
    public class MessagingHub : Foundation.HubConfig.Hub<IMessagingHub>
    {
        private readonly PresenceService _presenceService;
        private readonly ConversationService _conversationService;
        private readonly CallService _callService;
        private readonly IMessagingUserResolver _userResolver;


        public MessagingHub(PresenceService presenceService, ConversationService conversationService, CallService callService, IMessagingUserResolver userResolver)
        {
            _presenceService = presenceService;
            _conversationService = conversationService;
            _callService = callService;
            _userResolver = userResolver;
        }


        /// <summary>
        /// When a client connects, record the connection and set the user's status to Online.
        /// Broadcasts a PresenceChanged event to the tenant so the "Online Now" section updates in real-time.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            try
            {
                SecurityUser securityUser = await ResolveSecurityUserAsync();

                if (securityUser != null)
                {
                    PresenceService.PresenceSummary result = await _presenceService.RecordConnectionAsync(securityUser);

                    //
                    // Broadcast presence change to the tenant group
                    //
                    if (result != null && securityUser.securityTenant != null)
                    {
                        string tenantGroup = $"tenant:{securityUser.securityTenant.objectGuid}";

                        await Clients.Group(tenantGroup).PresenceChanged(new PresencePayload
                        {
                            userId = result.userId,
                            userDisplayName = result.displayName,
                            status = result.status,
                            customStatusMessage = result.customStatusMessage
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"MessagingHub: Failed to record connection presence: {ex.Message}");
            }
        }


        /// <summary>
        /// When a client disconnects, decrement the connection count.
        /// If no connections remain, status is set to Offline and a PresenceChanged event is broadcast.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                SecurityUser securityUser = await ResolveSecurityUserAsync();

                if (securityUser != null)
                {
                    PresenceService.PresenceSummary result = await _presenceService.RecordDisconnectionAsync(securityUser);

                    //
                    // Broadcast presence change to the tenant group
                    //
                    if (result != null && securityUser.securityTenant != null)
                    {
                        string tenantGroup = $"tenant:{securityUser.securityTenant.objectGuid}";

                        await Clients.Group(tenantGroup).PresenceChanged(new PresencePayload
                        {
                            userId = result.userId,
                            userDisplayName = result.displayName,
                            status = result.status,
                            customStatusMessage = result.customStatusMessage
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"MessagingHub: Failed to record disconnection presence: {ex.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }


        /// <summary>
        /// Resolves the SecurityUser from the SignalR connection context's JWT claims.
        /// Uses the same pattern as SecureWebAPIController.GetSecurityUserAsync().
        /// </summary>
        private async Task<SecurityUser> ResolveSecurityUserAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            string userObjectGuidString = Context.User.Claims
                .Where(c => c.Type == "sub")
                .Select(c => c.Value)
                .FirstOrDefault();

            if (userObjectGuidString != null && Guid.TryParse(userObjectGuidString, out Guid userObjectGuid))
            {
                return await SecurityLogic.GetUserRecordAsync(userObjectGuid);
            }

            return null;
        }


        /// <summary>
        /// Called by the client to join a conversation group so they receive real-time updates.
        /// </summary>
        public async Task JoinConversation(int conversationId)
        {
            //
            // Validate membership before allowing group join
            //
            SecurityUser securityUser = await ResolveSecurityUserAsync();
            if (securityUser == null) return;

            bool isMember = await _conversationService.IsUserInConversationAsync(securityUser, conversationId);
            if (!isMember)
            {
                _logger?.LogDebug($"MessagingHub: Connection {Context.ConnectionId} denied join for conversation:{conversationId} (not a member)");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");

            _logger?.LogDebug($"MessagingHub: Connection {Context.ConnectionId} joined conversation:{conversationId}");
        }


        /// <summary>
        /// Called by the client to leave a conversation group.
        /// </summary>
        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");

            _logger?.LogDebug($"MessagingHub: Connection {Context.ConnectionId} left conversation:{conversationId}");
        }


        /// <summary>
        /// Called by the client to join their tenant group for broadcast notifications.
        /// Validates that the requested tenant matches the caller's authenticated tenant.
        /// </summary>
        public async Task JoinTenant(string tenantGuid)
        {
            SecurityUser securityUser = await ResolveSecurityUserAsync();
            if (securityUser == null) return;

            //
            // Validate caller owns this tenant (case-insensitive GUID comparison)
            //
            if (securityUser.securityTenant == null ||
                !Guid.TryParse(tenantGuid, out Guid requestedTenantGuid) ||
                securityUser.securityTenant.objectGuid != requestedTenantGuid)
            {
                _logger?.LogDebug($"MessagingHub: Connection {Context.ConnectionId} denied join for tenant:{tenantGuid} (caller tenant: {securityUser.securityTenant?.objectGuid})");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{tenantGuid}");

            _logger?.LogDebug($"MessagingHub: Connection {Context.ConnectionId} joined tenant:{tenantGuid}");
        }


        /// <summary>
        /// Called by the client to join their personal user group for direct notifications and DMs.
        /// Validates that the requested user GUID matches the caller's own identity from the JWT.
        /// </summary>
        public async Task JoinUser(string userObjectGuid)
        {
            //
            // Validate caller is joining their own user group, not someone else's
            // Uses case-insensitive GUID comparison to handle format differences
            //
            string callerObjectGuid = Context.User?.Claims
                .Where(c => c.Type == "sub")
                .Select(c => c.Value)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(callerObjectGuid) ||
                !Guid.TryParse(callerObjectGuid, out Guid callerGuid) ||
                !Guid.TryParse(userObjectGuid, out Guid requestedGuid) ||
                callerGuid != requestedGuid)
            {
                _logger?.LogDebug($"MessagingHub: Connection {Context.ConnectionId} denied join for user:{userObjectGuid} (caller sub: {callerObjectGuid})");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userObjectGuid}");

            _logger?.LogDebug($"MessagingHub: Connection {Context.ConnectionId} joined user:{userObjectGuid}");
        }


        /// <summary>
        /// Called by the client to broadcast a typing indicator to other conversation participants.
        /// </summary>
        public async Task SendTypingIndicator(TypingPayload payload)
        {
            await Clients.OthersInGroup($"conversation:{payload.conversationId}").UserTyping(payload);
        }


        /// <summary>
        /// Called by the client to broadcast that they stopped typing.
        /// </summary>
        public async Task SendStoppedTypingIndicator(TypingPayload payload)
        {
            await Clients.OthersInGroup($"conversation:{payload.conversationId}").UserStoppedTyping(payload);
        }


        #region Call Signaling


        /// <summary>
        /// Called by the client to initiate a call in a conversation.
        /// Creates the call record and broadcasts IncomingCall to all recipients.
        /// </summary>
        public async Task InitiateCall(int conversationId, string callType, System.Collections.Generic.List<int> recipientUserIds, string preferredProviderId = null)
        {
            SecurityUser securityUser = await ResolveSecurityUserAsync();
            if (securityUser == null) return;

            try
            {
                CallService.CallSummary callSummary = await _callService.InitiateCallAsync(
                    securityUser, conversationId, callType, recipientUserIds, preferredProviderId);

                //
                // Build the offer payload
                //
                CallOfferPayload offerPayload = new CallOfferPayload
                {
                    callId = callSummary.id,
                    conversationId = conversationId,
                    callType = callSummary.callType,
                    initiatorUserId = callSummary.initiatorUserId,
                    initiatorDisplayName = callSummary.initiatorDisplayName,
                    providerId = callSummary.providerId,
                    providerCapabilities = callSummary.providerCapabilities
                };

                //
                // Send to all participants in the conversation group
                //
                await Clients.OthersInGroup($"conversation:{conversationId}").IncomingCall(offerPayload);

                _logger?.LogDebug($"MessagingHub: Call {callSummary.id} initiated in conversation:{conversationId} by user {callSummary.initiatorUserId}");
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"MessagingHub: Failed to initiate call: {ex.Message}");
            }
        }


        /// <summary>
        /// Called by the client to accept an incoming call.
        /// </summary>
        public async Task AcceptCall(int callId)
        {
            SecurityUser securityUser = await ResolveSecurityUserAsync();
            if (securityUser == null) return;

            try
            {
                CallService.CallSummary callSummary = await _callService.AcceptCallAsync(securityUser, callId);

                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                CallAnswerPayload answerPayload = new CallAnswerPayload
                {
                    callId = callId,
                    conversationId = callSummary.conversationId,
                    userId = user?.id ?? 0,
                    userDisplayName = user?.displayName ?? "Unknown"
                };

                await Clients.OthersInGroup($"conversation:{callSummary.conversationId}").CallAccepted(answerPayload);

                _logger?.LogDebug($"MessagingHub: Call {callId} accepted by user {answerPayload.userId}");
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"MessagingHub: Failed to accept call: {ex.Message}");
            }
        }


        /// <summary>
        /// Called by the client to decline an incoming call.
        /// </summary>
        public async Task DeclineCall(int callId)
        {
            SecurityUser securityUser = await ResolveSecurityUserAsync();
            if (securityUser == null) return;

            try
            {
                CallService.CallSummary callSummary = await _callService.DeclineCallAsync(securityUser, callId);

                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                CallDeclinePayload declinePayload = new CallDeclinePayload
                {
                    callId = callId,
                    conversationId = callSummary.conversationId,
                    userId = user?.id ?? 0,
                    userDisplayName = user?.displayName ?? "Unknown"
                };

                await Clients.OthersInGroup($"conversation:{callSummary.conversationId}").CallDeclined(declinePayload);

                //
                // If all recipients declined, end the call
                //
                if (callSummary.callStatus == "Declined")
                {
                    await Clients.Group($"conversation:{callSummary.conversationId}").CallEnded(new CallEndPayload
                    {
                        callId = callId,
                        conversationId = callSummary.conversationId,
                        endedByUserId = user?.id ?? 0,
                        endedByDisplayName = user?.displayName ?? "Unknown",
                        reason = "declined"
                    });
                }

                _logger?.LogDebug($"MessagingHub: Call {callId} declined by user {declinePayload.userId}");
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"MessagingHub: Failed to decline call via service: {ex.Message}. Broadcasting CallDeclined anyway.");

                //
                // Even if the service call fails (e.g. call already ended),
                // still broadcast so the caller knows and can clean up.
                //
                try
                {
                    MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                    var conversationId = await _callService.GetCallConversationIdAsync(securityUser, callId);

                    if (conversationId > 0)
                    {
                        await Clients.OthersInGroup($"conversation:{conversationId}").CallDeclined(new CallDeclinePayload
                        {
                            callId = callId,
                            conversationId = conversationId,
                            userId = user?.id ?? 0,
                            userDisplayName = user?.displayName ?? "Unknown"
                        });
                    }
                }
                catch (Exception broadcastEx)
                {
                    _logger?.LogDebug($"MessagingHub: Could not broadcast CallDeclined fallback: {broadcastEx.Message}");
                }
            }
        }


        /// <summary>
        /// Called by the client to end an active call.
        /// </summary>
        public async Task EndCall(int callId)
        {
            SecurityUser securityUser = await ResolveSecurityUserAsync();
            if (securityUser == null) return;

            try
            {
                CallService.CallSummary callSummary = await _callService.EndCallAsync(securityUser, callId);

                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                CallEndPayload endPayload = new CallEndPayload
                {
                    callId = callId,
                    conversationId = callSummary.conversationId,
                    endedByUserId = user?.id ?? 0,
                    endedByDisplayName = user?.displayName ?? "Unknown",
                    durationSeconds = callSummary.durationSeconds,
                    reason = "normal"
                };

                await Clients.Group($"conversation:{callSummary.conversationId}").CallEnded(endPayload);

                _logger?.LogDebug($"MessagingHub: Call {callId} ended by user {endPayload.endedByUserId}, duration: {callSummary.durationSeconds}s");
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"MessagingHub: Failed to end call via service: {ex.Message}. Broadcasting CallEnded anyway.");

                //
                // Even if the service call fails (e.g. call already ended),
                // we still need to broadcast CallEnded so the remote side cleans up.
                //
                try
                {
                    MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                    var conversationId = await _callService.GetCallConversationIdAsync(securityUser, callId);

                    if (conversationId > 0)
                    {
                        await Clients.Group($"conversation:{conversationId}").CallEnded(new CallEndPayload
                        {
                            callId = callId,
                            conversationId = conversationId,
                            endedByUserId = user?.id ?? 0,
                            endedByDisplayName = user?.displayName ?? "Unknown",
                            reason = "normal"
                        });
                    }
                }
                catch (Exception broadcastEx)
                {
                    _logger?.LogDebug($"MessagingHub: Could not broadcast CallEnded fallback: {broadcastEx.Message}");
                }
            }
        }


        /// <summary>
        /// Called by the client to relay an ICE candidate to another peer.
        /// This is WebRTC-specific but handled generically at the hub level.
        /// </summary>
        public async Task RelayIceCandidate(IceCandidatePayload payload)
        {
            SecurityUser securityUser = await ResolveSecurityUserAsync();
            if (securityUser == null) return;

            //
            // Resolve the target user's objectGuid — SignalR groups use user:{objectGuid}
            //
            string targetGroupKey = await ResolveUserGroupKeyAsync(payload.toUserId, securityUser.securityTenant.objectGuid);
            if (targetGroupKey == null) return;

            await Clients.Group(targetGroupKey).IceCandidate(payload);

            _logger?.LogDebug($"MessagingHub: ICE candidate relayed for call {payload.callId}: user {payload.fromUserId} -> {targetGroupKey}");
        }


        /// <summary>
        /// Called by the caller to relay their SDP offer to the answerer.
        /// </summary>
        public async Task RelaySdpOffer(SdpPayload payload)
        {
            SecurityUser securityUser = await ResolveSecurityUserAsync();
            if (securityUser == null) return;

            string targetGroupKey = await ResolveUserGroupKeyAsync(payload.toUserId, securityUser.securityTenant.objectGuid);
            if (targetGroupKey == null) return;

            await Clients.Group(targetGroupKey).SdpOffer(payload);

            _logger?.LogDebug($"MessagingHub: SDP offer relayed for call {payload.callId}: user {payload.fromUserId} -> {targetGroupKey}");
        }


        /// <summary>
        /// Called by the answerer to relay their SDP answer back to the caller.
        /// </summary>
        public async Task RelaySdpAnswer(SdpPayload payload)
        {
            SecurityUser securityUser = await ResolveSecurityUserAsync();
            if (securityUser == null) return;

            string targetGroupKey = await ResolveUserGroupKeyAsync(payload.toUserId, securityUser.securityTenant.objectGuid);
            if (targetGroupKey == null) return;

            await Clients.Group(targetGroupKey).SdpAnswer(payload);

            _logger?.LogDebug($"MessagingHub: SDP answer relayed for call {payload.callId}: user {payload.fromUserId} -> {targetGroupKey}");
        }


        /// <summary>
        /// Resolves a messaging userId (int) to the SignalR group key string "user:{objectGuid}".
        /// This is needed because the client joins groups using its auth objectGuid,
        /// but call payloads reference users by their messaging DB integer id.
        /// </summary>
        private async Task<string> ResolveUserGroupKeyAsync(int messagingUserId, Guid tenantGuid)
        {
            try
            {
                MessagingUser targetUser = await _userResolver.GetUserByIdAsync(messagingUserId, tenantGuid);
                if (targetUser != null)
                {
                    return $"user:{targetUser.objectGuid}";
                }

                _logger?.LogDebug($"MessagingHub: Could not resolve messaging user {messagingUserId} for relay routing");
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogException($"MessagingHub: Error resolving user group key for messaging user {messagingUserId}", ex);
                return null;
            }
        }


        #endregion
    }
}
