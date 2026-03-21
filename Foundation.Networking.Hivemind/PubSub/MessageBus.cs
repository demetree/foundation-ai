// ============================================================================
//
// MessageBus.cs — In-process pub/sub message bus.
//
// Provides channel-based publish/subscribe with typed messages,
// subscriber management, and message history.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Foundation.Networking.Hivemind.Configuration;

namespace Foundation.Networking.Hivemind.PubSub
{
    /// <summary>
    /// A published message.
    /// </summary>
    public class BusMessage
    {
        public string MessageId { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string SenderTag { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }


    /// <summary>
    /// Message bus statistics.
    /// </summary>
    public class MessageBusStatistics
    {
        public int ChannelCount { get; set; }
        public int TotalSubscribers { get; set; }
        public long TotalPublished { get; set; }
        public long TotalDelivered { get; set; }
    }


    /// <summary>
    ///
    /// In-process pub/sub message bus with channel-based subscription.
    ///
    /// </summary>
    public class MessageBus
    {
        private readonly HivemindConfiguration _config;
        private readonly ConcurrentDictionary<string, ChannelState> _channels;

        private long _totalPublished;
        private long _totalDelivered;


        public MessageBus(HivemindConfiguration config)
        {
            _config = config;
            _channels = new ConcurrentDictionary<string, ChannelState>();
        }


        /// <summary>
        /// Number of active channels.
        /// </summary>
        public int ChannelCount => _channels.Count;


        // ── Subscribe ─────────────────────────────────────────────────────


        /// <summary>
        /// Subscribes to a channel.  Returns a subscription ID.
        /// </summary>
        public string Subscribe(string channel, Action<BusMessage> handler)
        {
            ChannelState state = _channels.GetOrAdd(channel, _ => new ChannelState());
            string subscriptionId = Guid.NewGuid().ToString("N");

            state.Subscribers[subscriptionId] = handler;

            return subscriptionId;
        }


        /// <summary>
        /// Unsubscribes from a channel.
        /// </summary>
        public bool Unsubscribe(string channel, string subscriptionId)
        {
            if (_channels.TryGetValue(channel, out ChannelState state))
            {
                return state.Subscribers.TryRemove(subscriptionId, out _);
            }

            return false;
        }


        /// <summary>
        /// Gets the number of subscribers on a channel.
        /// </summary>
        public int GetSubscriberCount(string channel)
        {
            if (_channels.TryGetValue(channel, out ChannelState state))
            {
                return state.Subscribers.Count;
            }

            return 0;
        }


        // ── Publish ───────────────────────────────────────────────────────


        /// <summary>
        /// Publishes a string message to a channel.
        /// Returns the number of subscribers that received it.
        /// </summary>
        public int Publish(string channel, string payload, string senderTag = "")
        {
            BusMessage message = new BusMessage
            {
                MessageId = Guid.NewGuid().ToString("N"),
                Channel = channel,
                Payload = payload,
                SenderTag = senderTag,
                TimestampUtc = DateTime.UtcNow
            };

            return DeliverMessage(channel, message);
        }


        /// <summary>
        /// Publishes a typed message (serialized to JSON) to a channel.
        /// </summary>
        public int Publish<T>(string channel, T payload, string senderTag = "")
        {
            string json = JsonSerializer.Serialize(payload);
            return Publish(channel, json, senderTag);
        }


        // ── Channel Management ────────────────────────────────────────────


        /// <summary>
        /// Gets all active channel names.
        /// </summary>
        public List<string> GetChannels()
        {
            return _channels.Keys.ToList();
        }


        /// <summary>
        /// Gets the message history for a channel.
        /// </summary>
        public List<BusMessage> GetHistory(string channel, int count = 50)
        {
            if (_channels.TryGetValue(channel, out ChannelState state))
            {
                List<BusMessage> messages = state.History.ToList();
                int skip = Math.Max(0, messages.Count - count);

                return messages.Skip(skip).ToList();
            }

            return new List<BusMessage>();
        }


        // ── Statistics ────────────────────────────────────────────────────


        /// <summary>
        /// Gets message bus statistics.
        /// </summary>
        public MessageBusStatistics GetStatistics()
        {
            return new MessageBusStatistics
            {
                ChannelCount = _channels.Count,
                TotalSubscribers = _channels.Values.Sum(c => c.Subscribers.Count),
                TotalPublished = Interlocked.Read(ref _totalPublished),
                TotalDelivered = Interlocked.Read(ref _totalDelivered)
            };
        }


        // ── Internal ──────────────────────────────────────────────────────


        private int DeliverMessage(string channel, BusMessage message)
        {
            Interlocked.Increment(ref _totalPublished);

            ChannelState state = _channels.GetOrAdd(channel, _ => new ChannelState());

            //
            // Add to history
            //
            state.History.Enqueue(message);

            while (state.History.Count > _config.PubSubMaxQueueDepth)
            {
                state.History.TryDequeue(out _);
            }

            //
            // Deliver to subscribers
            //
            int delivered = 0;

            foreach (var subscriber in state.Subscribers)
            {
                try
                {
                    subscriber.Value(message);
                    delivered++;
                    Interlocked.Increment(ref _totalDelivered);
                }
                catch
                {
                    //
                    // Don't let a bad subscriber take down the bus
                    //
                }
            }

            return delivered;
        }


        /// <summary>
        /// State for a single channel.
        /// </summary>
        private class ChannelState
        {
            public ConcurrentDictionary<string, Action<BusMessage>> Subscribers { get; } =
                new ConcurrentDictionary<string, Action<BusMessage>>();

            public ConcurrentQueue<BusMessage> History { get; } =
                new ConcurrentQueue<BusMessage>();
        }
    }
}
