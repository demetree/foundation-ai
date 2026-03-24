using Foundation.Messaging.Services;
using Foundation.HubConfig;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Messaging.Hosting
{
    /// <summary>
    /// 
    /// Background service that processes link preview requests from the LinkPreviewQueue.
    /// 
    /// Replaces the previous Task.Run() fire-and-forget pattern in the messaging controller
    /// with a proper ASP.NET hosted service that respects graceful shutdown and provides
    /// structured exception logging.
    /// 
    /// Each module (Catalyst, Basecamp) registers this as a hosted service in their startup.
    /// 
    /// </summary>
    public class LinkPreviewBackgroundService : BackgroundService
    {
        private readonly LinkPreviewQueue _queue;
        private readonly IHubContext<MessagingHub, IMessagingHub> _hubContext;
        private readonly ILogger<LinkPreviewBackgroundService> _logger;


        public LinkPreviewBackgroundService(
            LinkPreviewQueue queue,
            IHubContext<MessagingHub, IMessagingHub> hubContext,
            ILogger<LinkPreviewBackgroundService> logger)
        {
            _queue = queue;
            _hubContext = hubContext;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LinkPreviewBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                LinkPreviewRequest request;

                try
                {
                    request = await _queue.DequeueAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    await ProcessLinkPreviewAsync(request);
                }
                catch (Exception ex)
                {
                    //
                    // Link previews are non-critical — log and continue processing the queue.
                    //
                    _logger.LogWarning(ex, "LinkPreviewBackgroundService: Error processing link preview for message {MessageId}.", request.MessageId);
                }
            }

            _logger.LogInformation("LinkPreviewBackgroundService stopped.");
        }


        private async Task ProcessLinkPreviewAsync(LinkPreviewRequest request)
        {
            LinkPreviewService linkPreviewService = new LinkPreviewService();

            var previews = await linkPreviewService.CreatePreviewsForMessageAsync(
                request.MessageId, request.TenantGuid, request.MessageHtml);

            if (previews != null && previews.Count > 0)
            {
                await _hubContext.Clients.Group($"conversation:{request.ConversationId}").LinkPreviewReady(new LinkPreviewReadyPayload
                {
                    conversationId = request.ConversationId,
                    messageId = request.MessageId,
                    previews = previews
                });
            }
        }
    }
}
