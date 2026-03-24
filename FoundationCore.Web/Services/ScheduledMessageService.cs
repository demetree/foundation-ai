using Foundation.Messaging.Database;
using Foundation.Messaging.Services;
using Foundation.HubConfig;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Messaging.Hosting
{
    /// <summary>
    /// 
    /// Background service that polls for due scheduled messages and releases them.
    /// 
    /// Every polling interval (default 30 seconds), this service queries for
    /// ConversationMessage rows where isScheduled = true and scheduledDateTime
    /// has passed.  It flips isScheduled to false and broadcasts the message
    /// via SignalR as if it were just sent.
    /// 
    /// Each module (Catalyst, Basecamp) registers this as a hosted service in their startup.
    /// 
    /// </summary>
    public class ScheduledMessageService : BackgroundService
    {
        private readonly IHubContext<MessagingHub, IMessagingHub> _hubContext;
        private readonly ILogger<ScheduledMessageService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);


        public ScheduledMessageService(
            IHubContext<MessagingHub, IMessagingHub> hubContext,
            ILogger<ScheduledMessageService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ScheduledMessageService started. Polling every {Interval}s.", _pollingInterval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessDueMessagesAsync();
                }
                catch (Exception ex)
                {
                    //
                    // Non-critical — log and continue polling on the next interval.
                    //
                    _logger.LogWarning(ex, "ScheduledMessageService: Error processing scheduled messages.");
                }

                try
                {
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("ScheduledMessageService stopped.");
        }


        private async Task ProcessDueMessagesAsync()
        {
            using (MessagingContext db = new MessagingContext())
            {
                DateTime now = DateTime.UtcNow;

                //
                // Find all scheduled messages whose scheduled time has passed
                //
                List<ConversationMessage> dueMessages = await (from m in db.ConversationMessages
                                                                where
                                                                m.isScheduled == true &&
                                                                m.scheduledDateTime != null &&
                                                                m.scheduledDateTime <= now &&
                                                                m.active == true &&
                                                                m.deleted == false
                                                                select m)
                                                               .ToListAsync();

                if (dueMessages.Count == 0) return;

                _logger.LogInformation("ScheduledMessageService: Releasing {Count} scheduled message(s).", dueMessages.Count);

                foreach (ConversationMessage message in dueMessages)
                {
                    //
                    // Release the message by clearing the scheduled flag
                    //
                    message.isScheduled = false;
                    message.scheduledDateTime = null;
                }

                await db.SaveChangesAsync();

                //
                // Broadcast each released message to its conversation group via SignalR
                //
                foreach (ConversationMessage message in dueMessages)
                {
                    try
                    {
                        await _hubContext.Clients.Group($"conversation:{message.conversationId}").ScheduledMessageReleased(new ScheduledMessageReleasedPayload
                        {
                            conversationId = message.conversationId,
                            messageId = message.id,
                            userId = message.userId,
                            messageHtml = message.message,
                            dateTimeCreated = message.dateTimeCreated
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "ScheduledMessageService: Failed to broadcast released message {MessageId}.", message.id);
                    }
                }
            }
        }
    }
}
