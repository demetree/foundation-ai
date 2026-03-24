using Foundation.Messaging.Services;
using Ganss.Xss;
using Foundation.Messaging;
using Foundation.Messaging.Database;
using Foundation.Controllers;
using Foundation.HubConfig;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.Auditor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Foundation.Messaging.Controllers
{
    /// <summary>
    /// 
    /// Foundation Messaging Controller Base - abstract REST API controller that modules can inherit
    /// to gain a complete messaging REST API with real-time SignalR events.
    /// 
    /// Modules inherit this class and pass in their module-specific constructor arguments.
    /// This base handles all conversation, messaging, read-tracking, reaction, pin, and presence endpoints.
    /// 
    /// Example usage in Catalyst:
    ///     public class MessagingController : MessagingControllerBase
    ///     {
    ///         public MessagingController(
    ///             ConversationService conversationService,
    ///             PresenceService presenceService,
    ///             IHubContext&lt;MessagingHub, IMessagingHub&gt; messagingHub
    ///         ) : base("Catalyst", "Messaging", conversationService, presenceService, messagingHub) { }
    ///     }
    /// 
    /// </summary>
    public abstract class MessagingControllerBase : SecureWebAPIController
    {
        protected readonly ConversationService _conversationService;
        protected readonly PresenceService _presenceService;
        protected readonly NotificationService _notificationService;
        protected readonly MessagingProfileService _profileService;
        protected readonly MessagingAdminService _adminService;
        protected readonly CallService _callService;
        protected readonly IAttachmentStorageProvider _attachmentStorageProvider;
        protected readonly IMessagingUserResolver _userResolver;
        protected readonly IHubContext<MessagingHub, IMessagingHub> _messagingHub;
        protected readonly LinkPreviewQueue _linkPreviewQueue;

        /// <summary>
        /// Maximum attachment file size in bytes.  Configured via appSettings key "Messaging:MaxAttachmentSizeMB".
        /// Defaults to 100 MB if not specified.
        /// </summary>
        protected readonly long _maxAttachmentSizeBytes;

        /// <summary>
        /// Set of file extensions (lowercase, with leading dot) that are blocked from upload.
        /// Configured via appSettings key "Messaging:BlockedAttachmentExtensions" (comma-separated).
        /// Defaults to a comprehensive list of executable and script extensions.
        /// </summary>
        protected readonly HashSet<string> _blockedExtensions;


        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;
        public const int DEFAULT_MAX_ATTACHMENT_SIZE_MB = 100;

        /// <summary>
        /// Per-user send-message rate limiter.  Static so it is shared across all controller instances
        /// within the process.  Enforces a maximum of SEND_RATE_LIMIT_MAX messages per second per user.
        /// </summary>
        private static readonly ConcurrentDictionary<int, Queue<DateTime>> _sendRateLimits = new ConcurrentDictionary<int, Queue<DateTime>>();
        private const int SEND_RATE_LIMIT_MAX = 5;
        private const int SEND_RATE_LIMIT_WINDOW_SECONDS = 1;

        /// <summary>
        /// Default blocked file extensions — executables, scripts, and system files.
        /// </summary>
        public static readonly string[] DEFAULT_BLOCKED_EXTENSIONS = new[]
        {
            ".exe", ".bat", ".cmd", ".scr", ".pif", ".msi", ".com",
            ".vbs", ".vbe", ".js", ".jse", ".wsf", ".wsh",
            ".ps1", ".psm1",
            ".reg", ".dll", ".sys", ".cpl", ".hta", ".inf", ".lnk"
        };


        /// <summary>
        /// Shared HTML sanitizer instance configured with a safe allowlist for Tiptap editor output.
        /// Static so it is created once and reused across all controller instances.
        /// </summary>
        private static readonly HtmlSanitizer _htmlSanitizer = CreateMessageSanitizer();

        private static HtmlSanitizer CreateMessageSanitizer()
        {
            HtmlSanitizer sanitizer = new HtmlSanitizer();

            //
            // Clear defaults and explicitly allow only safe tags produced by the Tiptap rich-text editor.
            //
            sanitizer.AllowedTags.Clear();
            foreach (string tag in new[] { "p", "br", "strong", "b", "em", "i", "u", "s", "strike",
                "a", "code", "pre", "blockquote",
                "ul", "ol", "li",
                "h1", "h2", "h3", "h4",
                "span", "div",
                "sub", "sup",
                "hr", "img" })
            {
                sanitizer.AllowedTags.Add(tag);
            }

            //
            // Allow only safe attributes (href for links, src/alt for images, class for styling)
            //
            sanitizer.AllowedAttributes.Clear();
            foreach (string attr in new[] { "href", "target", "rel", "class", "src", "alt", "title", "data-type", "data-mention" })
            {
                sanitizer.AllowedAttributes.Add(attr);
            }

            //
            // Allow safe CSS properties for basic inline styling from the editor
            //
            sanitizer.AllowedCssProperties.Clear();
            foreach (string prop in new[] { "color", "background-color", "font-weight", "font-style", "text-decoration" })
            {
                sanitizer.AllowedCssProperties.Add(prop);
            }

            //
            // Allow safe URI schemes (http, https, mailto) — blocks javascript: etc.
            //
            sanitizer.AllowedSchemes.Clear();
            sanitizer.AllowedSchemes.Add("http");
            sanitizer.AllowedSchemes.Add("https");
            sanitizer.AllowedSchemes.Add("mailto");

            return sanitizer;
        }


        /// <summary>
        /// Sanitizes user-submitted message HTML using the static allowlist-based sanitizer.
        /// Returns the sanitized HTML string, or null/empty if the input is null/empty.
        /// </summary>
        protected static string SanitizeMessageHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return html;
            }

            return _htmlSanitizer.Sanitize(html);
        }


        protected MessagingControllerBase(
            string moduleName,
            string entityName,
            ConversationService conversationService,
            PresenceService presenceService,
            NotificationService notificationService,
            MessagingProfileService profileService,
            MessagingAdminService adminService,
            CallService callService,
            IAttachmentStorageProvider attachmentStorageProvider,
            IMessagingUserResolver userResolver,
            IHubContext<MessagingHub, IMessagingHub> messagingHub,
            LinkPreviewQueue linkPreviewQueue = null,
            IConfiguration configuration = null
        ) : base(moduleName, entityName)
        {
            _conversationService = conversationService;
            _presenceService = presenceService;
            _notificationService = notificationService;
            _profileService = profileService;
            _adminService = adminService;
            _callService = callService;
            _attachmentStorageProvider = attachmentStorageProvider;
            _userResolver = userResolver;
            _messagingHub = messagingHub;
            _linkPreviewQueue = linkPreviewQueue;

            int maxMB = configuration?.GetValue<int?>("Messaging:MaxAttachmentSizeMB") ?? DEFAULT_MAX_ATTACHMENT_SIZE_MB;
            _maxAttachmentSizeBytes = (long)maxMB * 1024 * 1024;

            //
            // Build the blocked extensions set from configuration or defaults.
            //
            string blockedExtConfig = configuration?.GetValue<string>("Messaging:BlockedAttachmentExtensions");
            if (!string.IsNullOrWhiteSpace(blockedExtConfig))
            {
                _blockedExtensions = new HashSet<string>(
                    blockedExtConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(e => e.StartsWith(".") ? e.ToLowerInvariant() : "." + e.ToLowerInvariant()),
                    StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                _blockedExtensions = new HashSet<string>(DEFAULT_BLOCKED_EXTENSIONS, StringComparer.OrdinalIgnoreCase);
            }
        }


        #region Request DTOs

        public class CreateDirectMessageRequest
        {
            public List<string> recipientAccountNames { get; set; }
            public string initialMessage { get; set; }
        }

        public class CreateEntityConversationRequest
        {
            public string entityName { get; set; }
            public int entityId { get; set; }
            public List<string> participantAccountNames { get; set; }
            public string initialMessage { get; set; }
        }

        public class SendMessageRequest
        {
            public int conversationId { get; set; }
            public string message { get; set; }
            public int? parentMessageId { get; set; }
            public string entity { get; set; }
            public int? entityId { get; set; }
            public int? channelId { get; set; }
            public List<AttachmentMetadata> attachments { get; set; }
        }

        public class AttachmentMetadata
        {
            public Guid attachmentGuid { get; set; }
            public string fileName { get; set; }
            public string mimeType { get; set; }
            public long contentSize { get; set; }
        }

        public class EditMessageRequest
        {
            public int messageId { get; set; }
            public string message { get; set; }
        }

        public class ForwardMessageRequest
        {
            public int sourceMessageId { get; set; }
            public int targetConversationId { get; set; }
        }

        public class AddReactionRequest
        {
            public int messageId { get; set; }
            public string reaction { get; set; }
        }

        public class AddUserToConversationRequest
        {
            public int conversationId { get; set; }
            public string accountName { get; set; }
        }

        public class CreateNotificationRequest
        {
            public List<int> recipientUserIds { get; set; }
            public string message { get; set; }
            public string entity { get; set; }
            public int? entityId { get; set; }
            public string externalURL { get; set; }
            public string notificationType { get; set; }
            public int priority { get; set; } = 10;
            public bool distribute { get; set; } = true;
        }

        public class CreateChannelRequest
        {
            public int conversationId { get; set; }
            public string name { get; set; }
            public string topic { get; set; }
            public bool isPrivate { get; set; }
        }

        public class UpdateChannelRequest
        {
            public string name { get; set; }
            public string topic { get; set; }
            public bool? isPrivate { get; set; }
        }

        public class SetStatusRequest
        {
            public string status { get; set; }
            public string customStatusMessage { get; set; }
        }

        public class CreateChannelConversationRequest
        {
            public string name { get; set; }
            public string description { get; set; }
            public bool isPublic { get; set; }
        }

        public class UpdateNotificationProfileRequest
        {
            public string email { get; set; }
            public string phone { get; set; }
            public bool? emailEnabled { get; set; }
            public bool? smsEnabled { get; set; }
            public string emailPreference { get; set; }
            public string smsPreference { get; set; }
            public bool? quietHoursEnabled { get; set; }
            public string quietHoursStart { get; set; }
            public string quietHoursEnd { get; set; }
            public string timezone { get; set; }
            public List<int> pinnedConversations { get; set; }
            public Dictionary<int, string> notificationPrefs { get; set; }
        }

        public class CreateFlagRequest
        {
            public int conversationMessageId { get; set; }
            public string reason { get; set; }
            public string details { get; set; }
        }

        public class ResolveFlagRequest
        {
            public string resolutionStatus { get; set; }
            public string resolutionNotes { get; set; }
        }

        public class DismissLinkPreviewRequest
        {
            public int previewId { get; set; }
        }

        public class InitiateCallRequest
        {
            public int conversationId { get; set; }
            public string callType { get; set; }                // "Voice", "Video", "ScreenShare"
            public List<int> recipientUserIds { get; set; }
            public string preferredProviderId { get; set; }      // optional: "webrtc", "azure-acs"
        }

        #endregion


        #region Conversation Lifecycle


        [Route("api/Messaging/Conversation/DirectMessage")]
        [HttpPost]
        public virtual async Task<IActionResult> CreateDirectMessage([FromBody] CreateDirectMessageRequest request)
        {
            StartAuditEventClock();

            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                string sanitizedMessage = SanitizeMessageHtml(request.initialMessage);
                ConversationService.ConversationSummary result = await _conversationService.CreateDirectMessageAsync(securityUser, request.recipientAccountNames, sanitizedMessage);

                if (result != null && result.members != null)
                {
                    foreach (var member in result.members)
                    {
                        await _messagingHub.Clients.Group($"user:{member.accountName}").ReceiveNotification(new NotificationPayload
                        {
                            message = "New conversation started",
                            notificationType = "DirectMessage",
                            dateTimeCreated = result.dateTimeCreated,
                            priority = result.priority
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to create direct message conversation.", null, ex);
                return Problem(detail: ex.Message, title: "Create Direct Message Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversation/Entity")]
        [HttpPost]
        public virtual async Task<IActionResult> CreateEntityConversation([FromBody] CreateEntityConversationRequest request)
        {
            StartAuditEventClock();

            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                string sanitizedEntityMessage = SanitizeMessageHtml(request.initialMessage);
                ConversationService.ConversationSummary result = await _conversationService.CreateEntityConversationAsync(securityUser, request.entityName, request.entityId, request.participantAccountNames, sanitizedEntityMessage);

                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to create entity conversation.", null, ex);
                return Problem(detail: ex.Message, title: "Create Entity Conversation Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversation/Entity/{entityName}/{entityId}")]
        [HttpGet]
        public virtual async Task<IActionResult> GetEntityConversations(string entityName, int entityId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.GetConversationsForEntityAsync(securityUser, entityName, entityId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Entity Conversations Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversation/{conversationId}")]
        [HttpGet]
        public virtual async Task<IActionResult> GetConversation(int conversationId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.GetConversationSummaryAsync(securityUser, conversationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Conversation Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversations")]
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        public virtual async Task<IActionResult> GetConversations([FromQuery] bool includeArchived = false)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.GetConversationsForUserAsync(securityUser, includeArchived);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Conversations Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversation/{conversationId}/Archive")]
        [HttpPost]
        public virtual async Task<IActionResult> ArchiveConversation(int conversationId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.ArchiveConversationAsync(securityUser, conversationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to archive conversation.", null, ex);
                return Problem(detail: ex.Message, title: "Archive Conversation Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        public class RenameConversationRequest
        {
            public string name { get; set; }
        }

        [Route("api/Messaging/Conversation/{conversationId}/Rename")]
        [HttpPut]
        public virtual async Task<IActionResult> RenameConversation(int conversationId, [FromBody] RenameConversationRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.RenameConversationAsync(securityUser, conversationId, request.name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to rename conversation.", null, ex);
                return Problem(detail: ex.Message, title: "Rename Conversation Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Membership


        [Route("api/Messaging/Conversation/{conversationId}/Members")]
        [HttpGet]
        public virtual async Task<IActionResult> GetConversationMembers(int conversationId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.GetConversationMembersAsync(securityUser, conversationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Members Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversation/AddUser")]
        [HttpPost]
        public virtual async Task<IActionResult> AddUserToConversation([FromBody] AddUserToConversationRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.AddUserToConversationAsync(securityUser, request.conversationId, request.accountName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to add user to conversation.", null, ex);
                return Problem(detail: ex.Message, title: "Add User Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversation/{conversationId}/RemoveUser/{userId}")]
        [HttpPost]
        public virtual async Task<IActionResult> RemoveUserFromConversation(int conversationId, int userId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.RemoveUserFromConversationAsync(securityUser, conversationId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to remove user from conversation.", null, ex);
                return Problem(detail: ex.Message, title: "Remove User Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Messaging


        [Route("api/Messaging/Message")]
        [HttpPost]
        public virtual async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            StartAuditEventClock();

            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            //
            // Rate-limit check: prevent floods from buggy or malicious clients.
            //
            SecurityUser rateLimitUser = await GetSecurityUserAsync();
            if (rateLimitUser != null)
            {
                MessagingUser rateLimitMsgUser = await _userResolver.GetUserAsync(rateLimitUser);
                if (rateLimitMsgUser != null && IsUserRateLimited(rateLimitMsgUser.id))
                {
                    return StatusCode(429, new { error = "Rate limit exceeded. Please slow down." });
                }
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                string sanitizedSendMessage = SanitizeMessageHtml(request.message);
                ConversationService.MessageSummary result = await _conversationService.SendMessageAsync(securityUser, request.conversationId, sanitizedSendMessage, request.parentMessageId, request.entity, request.entityId, request.attachments?.ConvertAll(a => new ConversationService.AttachmentInput { attachmentGuid = a.attachmentGuid, fileName = a.fileName, mimeType = a.mimeType, contentSize = a.contentSize }), request.channelId);

                //
                // Broadcast new message to conversation participants via SignalR
                //
                if (result != null)
                {
                    await _messagingHub.Clients.Group($"conversation:{request.conversationId}").ReceiveMessage(new MessagePayload
                    {
                        conversationId = result.conversationId,
                        conversationChannelId = result.conversationChannelId,
                        messageId = result.id,
                        userId = result.userId,
                        userDisplayName = result.userDisplayName,
                        message = result.message,
                        parentConversationMessageId = result.parentConversationMessageId,
                        entity = result.entity,
                        entityId = result.entityId,
                        dateTimeCreated = result.dateTimeCreated,
                        hasAttachments = result.attachments != null && result.attachments.Count > 0
                    });
                }

                //
                // Process @mentions — scan for @[Display Name] patterns and create
                // server-side notifications so mentioned users are reliably alerted.
                //
                if (result != null && string.IsNullOrWhiteSpace(request.message) == false)
                {
                    await ProcessMentionNotificationsAsync(securityUser, request.conversationId, result, request.message);
                }

                //
                // Enqueue link preview processing for background service.
                // The LinkPreviewBackgroundService will fetch the previews and push them via SignalR.
                //
                if (result != null && string.IsNullOrWhiteSpace(request.message) == false && _linkPreviewQueue != null)
                {
                    _linkPreviewQueue.Enqueue(new LinkPreviewRequest
                    {
                        MessageId = result.id,
                        ConversationId = request.conversationId,
                        MessageHtml = request.message,
                        TenantGuid = securityUser.securityTenant.objectGuid
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to send message.", null, ex);
                return Problem(detail: ex.Message, title: "Send Message Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Message/Forward")]
        [HttpPost]
        public virtual async Task<IActionResult> ForwardMessage([FromBody] ForwardMessageRequest request)
        {
            StartAuditEventClock();

            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                ConversationService.MessageSummary result = await _conversationService.ForwardMessageAsync(securityUser, request.sourceMessageId, request.targetConversationId);

                //
                // Broadcast to the target conversation participants via SignalR
                //
                if (result != null)
                {
                    await _messagingHub.Clients.Group($"conversation:{request.targetConversationId}").ReceiveMessage(new MessagePayload
                    {
                        conversationId = result.conversationId,
                        messageId = result.id,
                        userId = result.userId,
                        userDisplayName = result.userDisplayName,
                        message = result.message,
                        dateTimeCreated = result.dateTimeCreated,
                        hasAttachments = false
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to forward message.", null, ex);
                return Problem(detail: ex.Message, title: "Forward Message Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        [Route("api/Messaging/Message/Edit")]
        [HttpPut]
        public virtual async Task<IActionResult> EditMessage([FromBody] EditMessageRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                string sanitizedEditMessage = SanitizeMessageHtml(request.message);
                var result = await _conversationService.EditMessageAsync(securityUser, request.messageId, sanitizedEditMessage);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to edit message.", null, ex);
                return Problem(detail: ex.Message, title: "Edit Message Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Message/{messageId}")]
        [HttpDelete]
        public virtual async Task<IActionResult> DeleteMessage(int messageId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                //
                // Resolve the message's conversationId and the user's messaging ID
                // before deleting, so we can broadcast the deletion via SignalR.
                //
                MessagingUser messagingUser = await _userResolver.GetUserAsync(securityUser);
                int conversationId = await _conversationService.GetConversationIdForMessageAsync(messageId);

                var result = await _conversationService.DeleteMessageAsync(securityUser, messageId);

                //
                // Broadcast deletion to conversation participants via SignalR
                //
                if (result && conversationId > 0 && messagingUser != null)
                {
                    await _messagingHub.Clients.Group($"conversation:{conversationId}").ReceiveMessageDelete(new MessageDeletePayload
                    {
                        conversationId = conversationId,
                        messageId = messageId,
                        userId = messagingUser.id
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to delete message.", null, ex);
                return Problem(detail: ex.Message, title: "Delete Message Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversation/{conversationId}/Messages")]
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        public virtual async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] DateTime? before = null, [FromQuery] int? channelId = null)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.GetMessagesAsync(securityUser, conversationId, page, pageSize, before, channelId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Messages Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Message/{parentMessageId}/Thread")]
        [HttpGet]
        public virtual async Task<IActionResult> GetThread(int parentMessageId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.GetThreadAsync(securityUser, parentMessageId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Thread Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Read Tracking


        [Route("api/Messaging/Message/{messageId}/MarkRead")]
        [HttpPost]
        public virtual async Task<IActionResult> MarkMessageRead(int messageId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.MarkMessageReadAsync(securityUser, messageId);

                //
                // Broadcast read receipt to conversation participants via SignalR
                //
                if (result)
                {
                    MessagingUser messagingUser = await _userResolver.GetUserAsync(securityUser);
                    int conversationId = await _conversationService.GetConversationIdForMessageAsync(messageId);

                    if (conversationId > 0 && messagingUser != null)
                    {
                        await _messagingHub.Clients.Group($"conversation:{conversationId}").MessageRead(new ReadReceiptPayload
                        {
                            conversationId = conversationId,
                            messageId = messageId,
                            userId = messagingUser.id
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Mark Read Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversation/{conversationId}/MarkRead")]
        [HttpPost]
        public virtual async Task<IActionResult> MarkConversationRead(int conversationId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.MarkConversationReadAsync(securityUser, conversationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Mark Conversation Read Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/UnreadCounts")]
        [HttpGet]
        public virtual async Task<IActionResult> GetUnreadCounts()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.GetUnreadCountsAsync(securityUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Unread Counts Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Search


        [Route("api/Messaging/Search")]
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        public virtual async Task<IActionResult> SearchMessages([FromQuery] string query, [FromQuery] int? conversationId = null, [FromQuery] string entityName = null, [FromQuery] int pageSize = 25)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.SearchMessagesAsync(securityUser, query, conversationId, entityName, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Search Messages Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Reactions


        [Route("api/Messaging/Reaction")]
        [HttpPost]
        public virtual async Task<IActionResult> AddReaction([FromBody] AddReactionRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.AddReactionAsync(securityUser, request.messageId, request.reaction);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to add reaction.", null, ex);
                return Problem(detail: ex.Message, title: "Add Reaction Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Reaction/{reactionId}")]
        [HttpDelete]
        public virtual async Task<IActionResult> RemoveReaction(int reactionId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.RemoveReactionAsync(securityUser, reactionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to remove reaction.", null, ex);
                return Problem(detail: ex.Message, title: "Remove Reaction Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Reaction/Toggle")]
        [HttpPost]
        public virtual async Task<IActionResult> ToggleReaction([FromBody] AddReactionRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.ToggleReactionAsync(securityUser, request.messageId, request.reaction);

                //
                // Broadcast the appropriate reaction event via SignalR
                //
                if (result != null && result.conversationId > 0)
                {
                    var payload = new ReactionPayload
                    {
                        conversationId = result.conversationId,
                        messageId = request.messageId,
                        reactionId = result.reactionId,
                        userId = result.userId,
                        userDisplayName = result.userDisplayName,
                        reaction = result.reaction
                    };

                    if (result.action == "added")
                    {
                        await _messagingHub.Clients.Group($"conversation:{result.conversationId}").ReactionAdded(payload);
                    }
                    else
                    {
                        await _messagingHub.Clients.Group($"conversation:{result.conversationId}").ReactionRemoved(payload);
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to toggle reaction.", null, ex);
                return Problem(detail: ex.Message, title: "Toggle Reaction Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/LinkPreview/Dismiss")]
        [HttpPost]
        public virtual async Task<IActionResult> DismissLinkPreview([FromBody] DismissLinkPreviewRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                LinkPreviewService linkPreviewService = new LinkPreviewService();
                await linkPreviewService.DismissPreviewAsync(request.previewId, securityUser.securityTenant.objectGuid);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Dismiss Link Preview Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Pins


        [Route("api/Messaging/Pin/{messageId}")]
        [HttpPost]
        public virtual async Task<IActionResult> PinMessage(int messageId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.PinMessageAsync(securityUser, messageId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to pin message.", null, ex);
                return Problem(detail: ex.Message, title: "Pin Message Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Unpin/{pinId}")]
        [HttpPost]
        public virtual async Task<IActionResult> UnpinMessage(int pinId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.UnpinMessageAsync(securityUser, pinId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to unpin message.", null, ex);
                return Problem(detail: ex.Message, title: "Unpin Message Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversation/{conversationId}/Pins")]
        [HttpGet]
        public virtual async Task<IActionResult> GetPinnedMessages(int conversationId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.GetPinnedMessagesAsync(securityUser, conversationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Pinned Messages Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Presence


        [Route("api/Messaging/Presence/Status")]
        [HttpPost]
        public virtual async Task<IActionResult> SetPresenceStatus([FromBody] SetStatusRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _presenceService.SetStatusAsync(securityUser, request.status, request.customStatusMessage);

                //
                // Broadcast presence change to all users in the tenant
                //
                if (result != null)
                {
                    string tenantGroup = $"tenant:{securityUser.securityTenant.objectGuid}";
                    await _messagingHub.Clients.Group(tenantGroup).PresenceChanged(new PresencePayload
                    {
                        userId = result.userId,
                        userDisplayName = result.displayName,
                        status = result.status,
                        customStatusMessage = result.customStatusMessage
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Set Presence Status Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Presence/{userId}")]
        [HttpGet]
        public virtual async Task<IActionResult> GetPresence(int userId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _presenceService.GetUserPresenceAsync(securityUser, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Presence Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Presence/Conversation/{conversationId}")]
        [HttpGet]
        public virtual async Task<IActionResult> GetConversationPresences(int conversationId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _presenceService.GetConversationPresencesAsync(securityUser, conversationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Conversation Presences Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }



        [Route("api/Messaging/Presence/Online")]
        [HttpGet]
        public virtual async Task<IActionResult> GetOnlineUsers()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _presenceService.GetOnlineUsersAsync(securityUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Online Users Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Presence/All")]
        [HttpGet]
        public virtual async Task<IActionResult> GetAllUserPresences()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _presenceService.GetAllUserPresencesAsync(securityUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get All User Presences Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Presence/Heartbeat")]
        [HttpPost]
        public virtual async Task<IActionResult> Heartbeat()
        {
            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                await _presenceService.RecordActivityAsync(securityUser);
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Heartbeat Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Notification Endpoints

        [Route("api/Messaging/Notifications")]
        [HttpGet]
        public virtual async Task<IActionResult> GetNotifications([FromQuery] bool unacknowledgedOnly = true, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _notificationService.GetNotificationsForUserAsync(securityUser, unacknowledgedOnly, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Notifications Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Notification")]
        [HttpPost]
        public virtual async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                int notificationId = await _notificationService.CreateNotificationAsync(
                    securityUser,
                    request.recipientUserIds,
                    request.message,
                    request.entity,
                    request.entityId,
                    request.externalURL,
                    request.notificationType,
                    request.priority,
                    request.distribute);

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Notification created", notificationId.ToString(), null);

                //
                // Push real-time notification to each recipient via SignalR
                //
                if (request.distribute)
                {
                    Guid tenantGuid = securityUser.securityTenant.objectGuid;

                    foreach (int recipientUserId in request.recipientUserIds)
                    {
                        MessagingUser recipientUser = await _userResolver.GetUserByIdAsync(recipientUserId, tenantGuid);

                        if (recipientUser != null)
                        {
                            await _messagingHub.Clients.Group($"user:{recipientUser.objectGuid}")
                                .ReceiveNotification(new NotificationPayload
                                {
                                    notificationId = notificationId,
                                    message = request.message,
                                    entity = request.entity,
                                    entityId = request.entityId,
                                    notificationType = request.notificationType,
                                    priority = request.priority,
                                    dateTimeCreated = DateTime.UtcNow
                                });
                        }
                    }
                }

                return Ok(new { notificationId = notificationId });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Create Notification Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Notification/{notificationDistributionId}/Acknowledge")]
        [HttpPost]
        public virtual async Task<IActionResult> AcknowledgeNotification(int notificationDistributionId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                await _notificationService.AcknowledgeNotificationAsync(securityUser, notificationDistributionId);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Notification acknowledged", notificationDistributionId.ToString(), null);

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Acknowledge Notification Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Notifications/AcknowledgeAll")]
        [HttpPost]
        public virtual async Task<IActionResult> AcknowledgeAllNotifications()
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                int count = await _notificationService.AcknowledgeAllNotificationsAsync(securityUser);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, $"All notifications acknowledged ({count})", null, null);

                return Ok(new { acknowledgedCount = count });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Acknowledge All Notifications Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/NotificationTypes")]
        [HttpGet]
        public virtual async Task<IActionResult> GetNotificationTypes()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _notificationService.GetNotificationTypesAsync(securityUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Notification Types Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Notifications/UnreadCount")]
        [HttpGet]
        public virtual async Task<IActionResult> GetNotificationUnreadCount()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                int count = await _notificationService.GetUnacknowledgedCountAsync(securityUser);
                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Notification Unread Count Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Channel Endpoints

        [Route("api/Messaging/Channel")]
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        public virtual async Task<IActionResult> CreateChannel([FromBody] CreateChannelRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.CreateChannelAsync(securityUser, request.conversationId, request.name, request.topic, request.isPrivate);

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Channel created", result.id.ToString(), null);

                //
                // Broadcast channel creation to conversation participants
                //
                await _messagingHub.Clients.Group($"conversation:{request.conversationId}").ChannelCreated(new ChannelPayload
                {
                    conversationId = result.conversationId,
                    channelId = result.id,
                    name = result.name,
                    topic = result.topic,
                    isPrivate = result.isPrivate,
                    isPinned = result.isPinned
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Create Channel Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Channel/{channelId}")]
        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        public virtual async Task<IActionResult> UpdateChannel(int channelId, [FromBody] UpdateChannelRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.UpdateChannelAsync(securityUser, channelId, request.name, request.topic, request.isPrivate);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Channel updated", channelId.ToString(), null);

                //
                // Broadcast channel update to conversation participants
                //
                await _messagingHub.Clients.Group($"conversation:{result.conversationId}").ChannelUpdated(new ChannelPayload
                {
                    conversationId = result.conversationId,
                    channelId = result.id,
                    name = result.name,
                    topic = result.topic,
                    isPrivate = result.isPrivate,
                    isPinned = result.isPinned
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Update Channel Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Channel/{channelId}")]
        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        public virtual async Task<IActionResult> DeleteChannel(int channelId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                int conversationId = await _conversationService.DeleteChannelAsync(securityUser, channelId);

                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity, "Channel deleted", channelId.ToString(), null);

                //
                // Broadcast channel deletion to conversation participants
                //
                if (conversationId > 0)
                {
                    await _messagingHub.Clients.Group($"conversation:{conversationId}").ChannelDeleted(new ChannelPayload
                    {
                        conversationId = conversationId,
                        channelId = channelId
                    });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Delete Channel Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Conversation/{conversationId}/Channels")]
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        public virtual async Task<IActionResult> GetChannels(int conversationId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.GetChannelsAsync(securityUser, conversationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Channels Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Attachment Endpoints

        [Route("api/Messaging/Attachment/Upload")]
        [HttpPost]
        public virtual async Task<IActionResult> UploadAttachment(IFormFile file)
        {
            try
            {
                if (_attachmentStorageProvider == null)
                {
                    return Problem(detail: "Attachment storage is not configured.", title: "Upload Failed", statusCode: 501, instance: HttpContext?.Request?.Path);
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file was uploaded.");
                }

                if (file.Length > _maxAttachmentSizeBytes)
                {
                    long maxMB = _maxAttachmentSizeBytes / (1024 * 1024);
                    return BadRequest($"File exceeds the maximum allowed size of {maxMB} MB.");
                }

                //
                // Validate file extension against blocklist.
                // Check all extensions in the filename to catch double-extension tricks (e.g. "report.pdf.exe").
                //
                string fileName = file.FileName ?? string.Empty;
                string remaining = fileName;
                while (!string.IsNullOrEmpty(remaining))
                {
                    string ext = Path.GetExtension(remaining);
                    if (string.IsNullOrEmpty(ext)) break;

                    if (_blockedExtensions.Contains(ext))
                    {
                        return BadRequest($"Files with the '{ext}' extension are not allowed.");
                    }

                    remaining = Path.GetFileNameWithoutExtension(remaining);
                }

                SecurityUser securityUser = await GetSecurityUserAsync();

                using (Stream stream = file.OpenReadStream())
                {
                    AttachmentStorageResult result = await _attachmentStorageProvider.StoreAsync(
                        securityUser.securityTenant.objectGuid,
                        file.FileName,
                        file.ContentType,
                        stream);

                    await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Attachment uploaded", result.storageGuid.ToString(), null);

                    return Ok(new
                    {
                        attachmentGuid = result.storageGuid,
                        contentSize = result.contentSize,
                        fileName = file.FileName,
                        mimeType = file.ContentType,
                    });
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Upload Attachment Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Attachment/{attachmentGuid}")]
        [HttpGet]
        public virtual async Task<IActionResult> DownloadAttachment(Guid attachmentGuid)
        {
            try
            {
                if (_attachmentStorageProvider == null)
                {
                    return Problem(detail: "Attachment storage is not configured.", title: "Download Failed", statusCode: 501, instance: HttpContext?.Request?.Path);
                }

                SecurityUser securityUser = await GetSecurityUserAsync();

                Stream contentStream = await _attachmentStorageProvider.RetrieveAsync(securityUser.securityTenant.objectGuid, attachmentGuid);

                if (contentStream == null)
                {
                    return NotFound();
                }

                //
                // Return the stream as a file download.  The caller can override Content-Type 
                // if they know the MIME type from the attachment metadata.
                //
                return File(contentStream, "application/octet-stream");
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Download Attachment Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        [Route("api/Messaging/Attachment/{attachmentGuid}")]
        [HttpDelete]
        public virtual async Task<IActionResult> DeleteAttachment(Guid attachmentGuid)
        {
            try
            {
                if (_attachmentStorageProvider == null)
                {
                    return Problem(detail: "Attachment storage is not configured.", title: "Delete Failed", statusCode: 501, instance: HttpContext?.Request?.Path);
                }

                SecurityUser securityUser = await GetSecurityUserAsync();

                bool deleted = await _attachmentStorageProvider.DeleteAsync(securityUser.securityTenant.objectGuid, attachmentGuid);

                if (deleted == false)
                {
                    return NotFound();
                }

                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity, "Attachment deleted", attachmentGuid.ToString(), null);

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Delete Attachment Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Mention Processing

        /// <summary>
        /// Scans the message text for @[Display Name] patterns and creates server-side
        /// notifications for each mentioned user.  Errors are logged but do not fail the request.
        /// </summary>
        private async Task ProcessMentionNotificationsAsync(SecurityUser securityUser, int conversationId, ConversationService.MessageSummary messageSummary, string messageText)
        {
            try
            {
                //
                // Match @[Display Name] patterns used by the client-side mention picker
                //
                MatchCollection matches = Regex.Matches(messageText, @"@\[([^\]]+)\]");

                if (matches.Count == 0)
                {
                    return;
                }

                //
                // Resolve conversation members so we can map display names → user IDs
                //
                List<ConversationService.ConversationMember> members = await _conversationService.GetConversationMembersAsync(securityUser, conversationId);

                if (members == null || members.Count == 0)
                {
                    return;
                }

                HashSet<int> mentionedUserIds = new HashSet<int>();

                foreach (Match match in matches)
                {
                    string displayName = match.Groups[1].Value;

                    ConversationService.ConversationMember member = members.FirstOrDefault(
                        m => string.Equals(m.displayName, displayName, StringComparison.OrdinalIgnoreCase)
                    );

                    if (member != null)
                    {
                        mentionedUserIds.Add(member.userId);
                    }
                }

                //
                // Don't notify the sender about their own @mention
                //
                mentionedUserIds.Remove(messageSummary.userId);

                if (mentionedUserIds.Count == 0)
                {
                    return;
                }

                //
                // Create a notification for the mentioned users
                //
                string preview = messageText.Length > 80 ? messageText.Substring(0, 80) + "…" : messageText;
                string notificationMessage = $"{messageSummary.userDisplayName} mentioned you: \"{preview}\"";

                await _notificationService.CreateNotificationAsync(
                    securityUser,
                    mentionedUserIds.ToList(),
                    notificationMessage,
                    "Conversation",
                    conversationId,
                    null,
                    "Mention",
                    20,
                    true);
            }
            catch (Exception ex)
            {
                //
                // Mention notifications are best-effort — log but don't fail the message send
                //
                System.Diagnostics.Debug.WriteLine($"Failed to process mention notifications: {ex.Message}");
            }
        }

        #endregion


        #region Top-Level Channels

        /// <summary>
        /// Creates a new top-level channel conversation.
        /// </summary>
        [Route("api/Messaging/Conversations/Channel")]
        [HttpPost]
        public virtual async Task<IActionResult> CreateChannelConversation([FromBody] CreateChannelConversationRequest request)
        {
            try
            {
                if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
                {
                    return Forbid();
                }

                SecurityUser securityUser = await GetSecurityUserAsync();


                var result = await _conversationService.CreateChannelConversationAsync(securityUser, request.name, request.description, request.isPublic);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Browse all public channels available to join.
        /// </summary>
        [Route("api/Messaging/Conversations/Channels/Browse")]
        [HttpGet]
        public virtual async Task<IActionResult> BrowseChannels()
        {
            try
            {
                if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
                {
                    return Forbid();
                }

                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.GetPublicChannelsAsync(securityUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Join a channel conversation.
        /// </summary>
        [Route("api/Messaging/Conversations/{conversationId}/Join")]
        [HttpPost]
        public virtual async Task<IActionResult> JoinChannel(int conversationId)
        {
            try
            {
                if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
                {
                    return Forbid();
                }

                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.JoinChannelAsync(securityUser, conversationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Operation Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Leave a channel conversation.
        /// </summary>
        [Route("api/Messaging/Conversations/{conversationId}/Leave")]
        [HttpPost]
        public virtual async Task<IActionResult> LeaveChannel(int conversationId)
        {
            try
            {
                if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
                {
                    return Forbid();
                }

                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _conversationService.LeaveChannelAsync(securityUser, conversationId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Operation Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Notification Profile Endpoints


        /// <summary>
        /// Gets the current user's notification profile (email, phone, delivery preferences, quiet hours).
        /// </summary>
        [Route("api/Messaging/Profile")]
        [HttpGet]
        public virtual async Task<IActionResult> GetNotificationProfile()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _profileService.GetNotificationProfileAsync(securityUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Notification Profile Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Updates the current user's notification profile.
        /// Only non-null fields in the request body are updated (partial update).
        /// </summary>
        [Route("api/Messaging/Profile")]
        [HttpPut]
        public virtual async Task<IActionResult> UpdateNotificationProfile([FromBody] UpdateNotificationProfileRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                var update = new MessagingProfileService.NotificationProfileUpdate
                {
                    email = request.email,
                    phone = request.phone,
                    emailEnabled = request.emailEnabled,
                    smsEnabled = request.smsEnabled,
                    emailPreference = request.emailPreference,
                    smsPreference = request.smsPreference,
                    quietHoursEnabled = request.quietHoursEnabled,
                    quietHoursStart = request.quietHoursStart,
                    quietHoursEnd = request.quietHoursEnd,
                    timezone = request.timezone,
                    pinnedConversations = request.pinnedConversations,
                    notificationPrefs = request.notificationPrefs
                };

                bool success = await _profileService.UpdateNotificationProfileAsync(securityUser, update);

                if (success)
                {
                    //
                    // Return the updated profile so the client can refresh its state
                    //
                    var updatedProfile = await _profileService.GetNotificationProfileAsync(securityUser);
                    return Ok(updatedProfile);
                }

                return Problem(detail: "Failed to update notification profile.", title: "Update Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Update Notification Profile Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets a specific user's notification profile (admin only).
        /// Requires the 'Catalyst Message Administrator' role.
        /// </summary>
        [Route("api/Messaging/Admin/Profile/{userId}")]
        [HttpGet]
        public virtual async Task<IActionResult> GetNotificationProfileAdmin(int userId)
        {
            //
            // Admin role check — requires elevated read permission
            //
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(3))
            {
                return Forbid();
            }

            try
            {
                //
                // Resolve the target user via the messaging user resolver
                //
                SecurityUser callerUser = await GetSecurityUserAsync();
                MessagingUser targetMsgUser = await _userResolver.GetUserByIdAsync(userId, callerUser.securityTenant.objectGuid);

                if (targetMsgUser == null)
                {
                    return NotFound("User not found.");
                }

                //
                // Create a minimal SecurityUser to read their settings
                //
                SecurityUser targetSecurityUser = await ResolveSecurityUserByAccountNameAsync(targetMsgUser.accountName);

                if (targetSecurityUser == null)
                {
                    return NotFound("Security user not found.");
                }

                var result = await _profileService.GetNotificationProfileAdminAsync(targetSecurityUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Admin Profile Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Updates a specific user's notification profile (admin only).
        /// Requires the 'Catalyst Message Administrator' role.
        /// </summary>
        [Route("api/Messaging/Admin/Profile/{userId}")]
        [HttpPut]
        public virtual async Task<IActionResult> UpdateNotificationProfileAdmin(int userId, [FromBody] UpdateNotificationProfileRequest request)
        {
            //
            // Admin role check — requires elevated write permission
            //
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(3))
            {
                return Forbid();
            }

            try
            {
                SecurityUser callerUser = await GetSecurityUserAsync();
                MessagingUser targetMsgUser = await _userResolver.GetUserByIdAsync(userId, callerUser.securityTenant.objectGuid);

                if (targetMsgUser == null)
                {
                    return NotFound("User not found.");
                }

                SecurityUser targetSecurityUser = await ResolveSecurityUserByAccountNameAsync(targetMsgUser.accountName);

                if (targetSecurityUser == null)
                {
                    return NotFound("Security user not found.");
                }

                var update = new MessagingProfileService.NotificationProfileUpdate
                {
                    email = request.email,
                    phone = request.phone,
                    emailEnabled = request.emailEnabled,
                    smsEnabled = request.smsEnabled,
                    emailPreference = request.emailPreference,
                    smsPreference = request.smsPreference,
                    quietHoursEnabled = request.quietHoursEnabled,
                    quietHoursStart = request.quietHoursStart,
                    quietHoursEnd = request.quietHoursEnd,
                    timezone = request.timezone,
                    pinnedConversations = request.pinnedConversations,
                    notificationPrefs = request.notificationPrefs
                };

                bool success = await _profileService.UpdateNotificationProfileAdminAsync(targetSecurityUser, update);

                if (success)
                {
                    var updatedProfile = await _profileService.GetNotificationProfileAdminAsync(targetSecurityUser);
                    return Ok(updatedProfile);
                }

                return Problem(detail: "Failed to update notification profile.", title: "Update Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Update Admin Profile Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Resolves a SecurityUser from an account name for admin operations.
        /// </summary>
        private async Task<SecurityUser> ResolveSecurityUserByAccountNameAsync(string accountName)
        {
            using (var db = new SecurityContext())
            {
                return await (from u in db.SecurityUsers
                              where u.accountName == accountName && u.deleted == false
                              select u)
                             .FirstOrDefaultAsync();
            }
        }

        #endregion


        #region Admin Endpoints


        /// <summary>
        /// Creates a flag/report on a message for admin review.
        /// </summary>
        [Route("api/Messaging/Flag")]
        [HttpPost]
        public virtual async Task<IActionResult> CreateFlag([FromBody] CreateFlagRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _adminService.CreateFlagAsync(securityUser, request.conversationMessageId, request.reason, request.details);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Create Flag Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets message flags (admin only). Optional filter by status (open, resolved, dismissed).
        /// </summary>
        [Route("api/Messaging/Admin/Flags")]
        [HttpGet]
        public virtual async Task<IActionResult> GetFlags([FromQuery] string status = null)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(3))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _adminService.GetFlagsAsync(securityUser, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Flags Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Resolves a message flag (admin only).
        /// </summary>
        [Route("api/Messaging/Admin/Flag/{flagId}/Resolve")]
        [HttpPost]
        public virtual async Task<IActionResult> ResolveFlag(int flagId, [FromBody] ResolveFlagRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(3))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _adminService.ResolveFlagAsync(securityUser, flagId, request.resolutionStatus, request.resolutionNotes);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Resolve Flag Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets messaging audit log entries (admin only).
        /// </summary>
        [Route("api/Messaging/Admin/AuditLog")]
        [HttpGet]
        public virtual async Task<IActionResult> GetAuditLog(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string action = null,
            [FromQuery] int maxResults = 100)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(3))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _adminService.GetAuditLogAsync(securityUser, startDate, endDate, action, maxResults);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Audit Log Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets push delivery logs (admin only).
        /// </summary>
        [Route("api/Messaging/Admin/DeliveryLogs")]
        [HttpGet]
        public virtual async Task<IActionResult> GetDeliveryLogs(
            [FromQuery] int? userId = null,
            [FromQuery] string providerId = null,
            [FromQuery] bool? successOnly = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] int maxResults = 100)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(3))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _adminService.GetDeliveryLogsAsync(securityUser, userId, providerId, successOnly, startDate, maxResults);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Delivery Logs Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Searches all messages across all conversations for the tenant (admin only).
        /// Supports text search, user filter, conversation filter, and date range.
        /// </summary>
        [Route("api/Messaging/Admin/Search")]
        [HttpGet]
        public virtual async Task<IActionResult> AdminSearchMessages(
            [FromQuery] string query = null,
            [FromQuery] int? conversationId = null,
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int maxResults = 100)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(3))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _adminService.AdminSearchMessagesAsync(securityUser, query, conversationId, userId, startDate, endDate, maxResults);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Admin Search Messages Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets high-level messaging usage metrics (admin only).
        /// </summary>
        [Route("api/Messaging/Admin/Metrics")]
        [HttpGet]
        public virtual async Task<IActionResult> GetMessagingMetrics()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(3))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _adminService.GetMetricsAsync(securityUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Metrics Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets analytics data for chart visualizations (admin only).
        /// </summary>
        [Route("api/Messaging/Admin/Analytics")]
        [HttpGet]
        public virtual async Task<IActionResult> GetMessagingAnalytics()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(3))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _adminService.GetAnalyticsAsync(securityUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Analytics Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Calling


        /// <summary>
        /// Initiates a voice, video, or screen share call in a conversation.
        /// </summary>
        [Route("api/Messaging/Call")]
        [HttpPost]
        public virtual async Task<IActionResult> InitiateCall([FromBody] InitiateCallRequest request)
        {
            StartAuditEventClock();

            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                CallService.CallSummary result = await _callService.InitiateCallAsync(
                    securityUser, request.conversationId, request.callType,
                    request.recipientUserIds, request.preferredProviderId);

                //
                // Broadcast the incoming call to conversation participants
                //
                if (result != null)
                {
                    await _messagingHub.Clients.Group($"conversation:{request.conversationId}").IncomingCall(new CallOfferPayload
                    {
                        callId = result.id,
                        conversationId = request.conversationId,
                        callType = result.callType,
                        initiatorUserId = result.initiatorUserId,
                        initiatorDisplayName = result.initiatorDisplayName,
                        providerId = result.providerId,
                        providerCapabilities = result.providerCapabilities
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Failed to initiate call.", null, ex);
                return Problem(detail: ex.Message, title: "Initiate Call Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Accepts an incoming call.
        /// </summary>
        [Route("api/Messaging/Call/{callId}/Accept")]
        [HttpPost]
        public virtual async Task<IActionResult> AcceptCall(int callId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                CallService.CallSummary result = await _callService.AcceptCallAsync(securityUser, callId);

                //
                // Broadcast the acceptance
                //
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                await _messagingHub.Clients.Group($"conversation:{result.conversationId}").CallAccepted(new CallAnswerPayload
                {
                    callId = callId,
                    conversationId = result.conversationId,
                    userId = user?.id ?? 0,
                    userDisplayName = user?.displayName ?? "Unknown"
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Accept Call Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Declines an incoming call.
        /// </summary>
        [Route("api/Messaging/Call/{callId}/Decline")]
        [HttpPost]
        public virtual async Task<IActionResult> DeclineCall(int callId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                CallService.CallSummary result = await _callService.DeclineCallAsync(securityUser, callId);

                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                await _messagingHub.Clients.Group($"conversation:{result.conversationId}").CallDeclined(new CallDeclinePayload
                {
                    callId = callId,
                    conversationId = result.conversationId,
                    userId = user?.id ?? 0,
                    userDisplayName = user?.displayName ?? "Unknown"
                });

                //
                // If all recipients declined, broadcast the call end
                //
                if (result.callStatus == "Declined")
                {
                    await _messagingHub.Clients.Group($"conversation:{result.conversationId}").CallEnded(new CallEndPayload
                    {
                        callId = callId,
                        conversationId = result.conversationId,
                        endedByUserId = user?.id ?? 0,
                        endedByDisplayName = user?.displayName ?? "Unknown",
                        reason = "declined"
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Decline Call Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Ends an active call.
        /// </summary>
        [Route("api/Messaging/Call/{callId}/End")]
        [HttpPost]
        public virtual async Task<IActionResult> EndCall(int callId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                CallService.CallSummary result = await _callService.EndCallAsync(securityUser, callId);

                MessagingUser user = await _userResolver.GetUserAsync(securityUser);
                await _messagingHub.Clients.Group($"conversation:{result.conversationId}").CallEnded(new CallEndPayload
                {
                    callId = callId,
                    conversationId = result.conversationId,
                    endedByUserId = user?.id ?? 0,
                    endedByDisplayName = user?.displayName ?? "Unknown",
                    durationSeconds = result.durationSeconds,
                    reason = "normal"
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "End Call Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets the details of a call.
        /// </summary>
        [Route("api/Messaging/Call/{callId}")]
        [HttpGet]
        public virtual async Task<IActionResult> GetCall(int callId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _callService.GetCallAsync(securityUser, callId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Call Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets provider-specific connection info for a call (ICE servers, tokens, etc.).
        /// Called after accepting a call to establish the media connection.
        /// </summary>
        [Route("api/Messaging/Call/{callId}/ConnectionInfo")]
        [HttpGet]
        public virtual async Task<IActionResult> GetCallConnectionInfo(int callId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _callService.GetConnectionInfoAsync(securityUser, callId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Connection Info Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets call history for a specific conversation.
        /// </summary>
        [Route("api/Messaging/Conversation/{conversationId}/Calls")]
        [HttpGet]
        public virtual async Task<IActionResult> GetConversationCalls(int conversationId, [FromQuery] int maxResults = 50)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _callService.GetConversationCallsAsync(securityUser, conversationId, maxResults);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Conversation Calls Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets the current user's recent call history.
        /// </summary>
        [Route("api/Messaging/Calls/Recent")]
        [HttpGet]
        public virtual async Task<IActionResult> GetRecentCalls([FromQuery] int maxResults = 50)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var result = await _callService.GetRecentCallsAsync(securityUser, maxResults);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Recent Calls Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets information about registered call providers and their capabilities.
        /// </summary>
        [Route("api/Messaging/Call/Providers")]
        [HttpGet]
        public virtual async Task<IActionResult> GetCallProviders()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                var result = _callService.GetRegisteredProviders();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Call Providers Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }

        #endregion


        #region Mark-as-Unread


        public class MarkMessageUnreadRequest
        {
            public int conversationMessageId { get; set; }
        }


        /// <summary>
        /// Marks a message as unread for the current user.
        /// Allows users to flag messages for deferred follow-up.
        /// </summary>
        [Route("api/Messaging/MarkMessageUnread")]
        [HttpPost]
        public virtual async Task<IActionResult> MarkMessageUnread([FromBody] MarkMessageUnreadRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                bool result = await _conversationService.MarkMessageUnreadAsync(securityUser, request.conversationMessageId);

                if (!result)
                {
                    return NotFound(new { error = "Message acknowledgment record not found." });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Mark Message Unread Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        #endregion


        #region Thread Unread Tracking


        public class UpdateThreadReadPositionRequest
        {
            public int conversationId { get; set; }
            public int parentMessageId { get; set; }
            public int lastReadMessageId { get; set; }
        }


        /// <summary>
        /// Updates the user's last-read position within a message thread.
        /// </summary>
        [Route("api/Messaging/UpdateThreadReadPosition")]
        [HttpPost]
        public virtual async Task<IActionResult> UpdateThreadReadPosition([FromBody] UpdateThreadReadPositionRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                await _conversationService.UpdateThreadReadPositionAsync(securityUser, request.conversationId, request.parentMessageId, request.lastReadMessageId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Update Thread Read Position Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets the unread count for a specific thread.
        /// </summary>
        [Route("api/Messaging/GetThreadUnreadCount")]
        [HttpGet]
        public virtual async Task<IActionResult> GetThreadUnreadCount([FromQuery] int conversationId, [FromQuery] int parentMessageId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                int count = await _conversationService.GetThreadUnreadCountAsync(securityUser, conversationId, parentMessageId);
                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Thread Unread Count Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        #endregion


        #region Channel Admin Roles


        public class UpdateUserRoleRequest
        {
            public int conversationId { get; set; }
            public int targetUserId { get; set; }
            public string newRole { get; set; }
        }


        /// <summary>
        /// Updates a user's role within a conversation (Owner, Admin, Member).
        /// </summary>
        [Route("api/Messaging/UpdateUserRole")]
        [HttpPost]
        public virtual async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                // Verify the caller has Owner or Admin role
                string callerRole = await _conversationService.GetUserRoleAsync(securityUser, request.conversationId);
                if (callerRole != "Owner" && callerRole != "Admin")
                {
                    return StatusCode(403, new { error = "Only Owners and Admins can change user roles." });
                }

                bool result = await _conversationService.UpdateUserRoleAsync(securityUser, request.conversationId, request.targetUserId, request.newRole);

                if (!result)
                {
                    return NotFound(new { error = "User membership not found." });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Update User Role Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        #endregion


        #region Message Bookmarks


        public class AddBookmarkRequest
        {
            public int conversationMessageId { get; set; }
            public string note { get; set; }
        }


        /// <summary>
        /// Adds a bookmark on a message for the current user.
        /// </summary>
        [Route("api/Messaging/AddBookmark")]
        [HttpPost]
        public virtual async Task<IActionResult> AddBookmark([FromBody] AddBookmarkRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var bookmark = await _conversationService.AddBookmarkAsync(securityUser, request.conversationMessageId, request.note);

                if (bookmark == null)
                {
                    return Conflict(new { error = "Message is already bookmarked." });
                }

                return Ok(new { success = true, bookmarkId = bookmark.id });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Add Bookmark Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Removes a bookmark by its ID.
        /// </summary>
        [Route("api/Messaging/RemoveBookmark")]
        [HttpPost]
        public virtual async Task<IActionResult> RemoveBookmark([FromBody] int bookmarkId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                bool result = await _conversationService.RemoveBookmarkAsync(securityUser, bookmarkId);

                if (!result)
                {
                    return NotFound(new { error = "Bookmark not found." });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Remove Bookmark Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        /// <summary>
        /// Gets all bookmarks for the current user.
        /// </summary>
        [Route("api/Messaging/GetBookmarks")]
        [HttpGet]
        public virtual async Task<IActionResult> GetBookmarks()
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                var bookmarks = await _conversationService.GetBookmarksAsync(securityUser);
                return Ok(bookmarks);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Get Bookmarks Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        #endregion


        #region Message Scheduling


        public class ScheduleMessageRequest
        {
            public int conversationId { get; set; }
            public string messageHtml { get; set; }
            public DateTime scheduledDateTime { get; set; }
            public int? parentConversationMessageId { get; set; }
        }


        /// <summary>
        /// Creates a scheduled message that will be released at the specified time.
        /// The message is stored with isScheduled=true and picked up by ScheduledMessageService.
        /// </summary>
        [Route("api/Messaging/ScheduleMessage")]
        [HttpPost]
        public virtual async Task<IActionResult> ScheduleMessage([FromBody] ScheduleMessageRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                MessagingUser user = await _userResolver.GetUserAsync(securityUser);

                if (user == null)
                {
                    return BadRequest(new { error = "Could not resolve user." });
                }

                if (request.scheduledDateTime <= DateTime.UtcNow)
                {
                    return BadRequest(new { error = "Scheduled time must be in the future." });
                }

                using (MessagingContext db = new MessagingContext())
                {
                    ConversationMessage scheduledMessage = new ConversationMessage
                    {
                        tenantGuid = securityUser.securityTenant.objectGuid,
                        conversationId = request.conversationId,
                        userId = user.id,
                        parentConversationMessageId = request.parentConversationMessageId,
                        message = request.messageHtml,
                        dateTimeCreated = DateTime.UtcNow,
                        isScheduled = true,
                        scheduledDateTime = request.scheduledDateTime,
                        versionNumber = 1,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    db.ConversationMessages.Add(scheduledMessage);
                    await db.SaveChangesAsync();

                    return Ok(new { success = true, messageId = scheduledMessage.id, scheduledDateTime = request.scheduledDateTime });
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Schedule Message Failed", statusCode: 500, instance: HttpContext?.Request?.Path);
            }
        }


        #endregion


        #region Rate Limiting


        /// <summary>
        /// Checks whether the specified user has exceeded the send-message rate limit.
        /// Uses a sliding window of SEND_RATE_LIMIT_WINDOW_SECONDS seconds and allows
        /// at most SEND_RATE_LIMIT_MAX messages within that window.
        /// </summary>
        private static bool IsUserRateLimited(int userId)
        {
            DateTime now = DateTime.UtcNow;
            DateTime windowStart = now.AddSeconds(-SEND_RATE_LIMIT_WINDOW_SECONDS);

            Queue<DateTime> timestamps = _sendRateLimits.GetOrAdd(userId, _ => new Queue<DateTime>());

            lock (timestamps)
            {
                //
                // Purge timestamps older than the sliding window
                //
                while (timestamps.Count > 0 && timestamps.Peek() < windowStart)
                {
                    timestamps.Dequeue();
                }

                if (timestamps.Count >= SEND_RATE_LIMIT_MAX)
                {
                    return true;
                }

                timestamps.Enqueue(now);
                return false;
            }
        }


        #endregion
    }
}
