// AI-Developed: Scheduler Messaging Controller
// Thin wrapper around Foundation MessagingControllerBase following the Catalyst pattern

using Foundation.Messaging;
using Foundation.Messaging.Controllers;
using Foundation.Messaging.Services;
using Foundation.HubConfig;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;


namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// Scheduler Messaging Controller - thin module-specific wrapper around the Foundation MessagingControllerBase.
    /// 
    /// All REST API endpoints for conversations, messages, membership, read tracking, reactions,
    /// pins, presence, notifications, channels, and attachments are provided by the base class.  
    /// This controller just wires up the Scheduler-specific DI dependencies.
    /// 
    /// Override any base endpoint method here if Scheduler needs module-specific behavior.
    /// 
    /// </summary>
    public partial class MessagingController : MessagingControllerBase
    {
        public MessagingController(
            ConversationService conversationService,
            PresenceService presenceService,
            NotificationService notificationService,
            MessagingProfileService profileService,
            MessagingAdminService adminService,
            CallService callService,
            IAttachmentStorageProvider attachmentStorageProvider,
            IMessagingUserResolver userResolver,
            IHubContext<MessagingHub, IMessagingHub> messagingHub,
            LinkPreviewQueue linkPreviewQueue,
            IConfiguration configuration
        ) : base("Scheduler", "Messaging", conversationService, presenceService, notificationService, profileService, adminService, callService, attachmentStorageProvider, userResolver, messagingHub, linkPreviewQueue, configuration)
        {
        }
    }
}
