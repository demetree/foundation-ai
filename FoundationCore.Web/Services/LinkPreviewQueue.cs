using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Singleton queue for link preview processing requests.
    /// 
    /// Messages are enqueued by the controller after a message is sent,
    /// and dequeued by LinkPreviewBackgroundService for asynchronous processing.
    /// 
    /// Uses System.Threading.Channels for lock-free, high-performance producer/consumer.
    /// 
    /// </summary>
    public class LinkPreviewQueue
    {
        private readonly Channel<LinkPreviewRequest> _channel;


        public LinkPreviewQueue()
        {
            //
            // Unbounded channel — we don't want to block the controller if the processor falls behind.
            // In practice, link preview fetches are fast and the queue depth stays near zero.
            //
            _channel = Channel.CreateUnbounded<LinkPreviewRequest>(new UnboundedChannelOptions
            {
                SingleReader = true
            });
        }


        /// <summary>
        /// Enqueues a link preview request for background processing.
        /// Called from the controller after a message with URLs is sent.
        /// </summary>
        public bool Enqueue(LinkPreviewRequest request)
        {
            return _channel.Writer.TryWrite(request);
        }


        /// <summary>
        /// Dequeues the next link preview request.  Blocks asynchronously until a request is available
        /// or the cancellation token is triggered (graceful shutdown).
        /// </summary>
        public async ValueTask<LinkPreviewRequest> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }


        /// <summary>
        /// Checks if there are any pending requests without blocking.
        /// </summary>
        public bool HasPending => _channel.Reader.Count > 0;
    }


    /// <summary>
    /// Represents a request to fetch link previews for a message.
    /// </summary>
    public class LinkPreviewRequest
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public string MessageHtml { get; set; }
        public Guid TenantGuid { get; set; }
    }
}
