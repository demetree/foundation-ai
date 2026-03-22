// ============================================================================
//
// ChannelManager.cs — WebSocket channel/room management.
//
// Manages channel subscriptions, message broadcasting, and channel lifecycle.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Foundation.Networking.Conduit.Configuration;
using Foundation.Networking.Conduit.Connections;

namespace Foundation.Networking.Conduit.Channels
{
    /// <summary>
    /// A channel message.
    /// </summary>
    public class ChannelMessage
    {
        public string MessageId { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string MessageType { get; set; } = "text";
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }


    /// <summary>
    /// Channel information.
    /// </summary>
    public class ChannelInfo
    {
        public string Name { get; set; } = string.Empty;
        public int SubscriberCount { get; set; }
        public long TotalMessages { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime LastActivityUtc { get; set; }
    }


    /// <summary>
    ///
    /// Manages channels for WebSocket message broadcasting.
    ///
    /// </summary>
    public class ChannelManager
    {
        private readonly ConduitConfiguration _config;
        private readonly ConnectionManager _connectionManager;
        private readonly ConcurrentDictionary<string, ChannelState> _channels;


        public ChannelManager(ConduitConfiguration config, ConnectionManager connectionManager)
        {
            _config = config;
            _connectionManager = connectionManager;
            _channels = new ConcurrentDictionary<string, ChannelState>(StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Number of active channels.
        /// </summary>
        public int ChannelCount => _channels.Count;


        // ── Subscribe / Unsubscribe ───────────────────────────────────────


        /// <summary>
        /// Subscribes a connection to a channel.
        /// </summary>
        public bool Subscribe(string connectionId, string channel)
        {
            ClientConnection conn = _connectionManager.Get(connectionId);

            if (conn == null)
            {
                return false;
            }

            if (conn.Channels.Count >= _config.MaxChannelsPerConnection)
            {
                return false;
            }

            ChannelState state = _channels.GetOrAdd(channel, _ => new ChannelState
            {
                CreatedUtc = DateTime.UtcNow,
                LastActivityUtc = DateTime.UtcNow
            });

            if (state.Subscribers.Count >= _config.MaxSubscribersPerChannel)
            {
                return false;
            }

            state.Subscribers.TryAdd(connectionId, true);
            conn.Channels.Add(channel);

            return true;
        }


        /// <summary>
        /// Unsubscribes a connection from a channel.
        /// </summary>
        public bool Unsubscribe(string connectionId, string channel)
        {
            ClientConnection conn = _connectionManager.Get(connectionId);

            if (conn != null)
            {
                conn.Channels.Remove(channel);
            }

            if (_channels.TryGetValue(channel, out ChannelState state))
            {
                bool removed = state.Subscribers.TryRemove(connectionId, out _);

                //
                // Clean up empty channels
                //
                if (state.Subscribers.IsEmpty)
                {
                    _channels.TryRemove(channel, out _);
                }

                return removed;
            }

            return false;
        }


        /// <summary>
        /// Removes a connection from all channels (on disconnect).
        /// </summary>
        public void UnsubscribeAll(string connectionId)
        {
            ClientConnection conn = _connectionManager.Get(connectionId);

            if (conn == null)
            {
                return;
            }

            foreach (string channel in conn.Channels.ToList())
            {
                Unsubscribe(connectionId, channel);
            }
        }


        // ── Broadcast ─────────────────────────────────────────────────────


        /// <summary>
        /// Broadcasts a message to all subscribers of a channel.
        /// Returns the list of connection IDs that should receive the message.
        /// </summary>
        public List<string> Broadcast(string channel, string senderId, string payload, string messageType = "text")
        {
            ChannelMessage message = new ChannelMessage
            {
                MessageId = Guid.NewGuid().ToString("N"),
                Channel = channel,
                SenderId = senderId,
                Payload = payload,
                MessageType = messageType,
                TimestampUtc = DateTime.UtcNow
            };

            ChannelState state = _channels.GetOrAdd(channel, _ => new ChannelState
            {
                CreatedUtc = DateTime.UtcNow,
                LastActivityUtc = DateTime.UtcNow
            });

            //
            // Add to history
            //
            state.History.Enqueue(message);

            while (state.History.Count > _config.ChannelHistoryDepth)
            {
                state.History.TryDequeue(out _);
            }

            state.TotalMessages++;
            state.LastActivityUtc = DateTime.UtcNow;

            //
            // Return list of subscribers (excluding sender)
            //
            return state.Subscribers.Keys
                .Where(id => id != senderId)
                .ToList();
        }


        /// <summary>
        /// Broadcasts a typed message (serialized to JSON).
        /// </summary>
        public List<string> Broadcast<T>(string channel, string senderId, T payload, string messageType = "json")
        {
            string json = JsonSerializer.Serialize(payload);
            return Broadcast(channel, senderId, json, messageType);
        }


        // ── Channel Info ──────────────────────────────────────────────────


        /// <summary>
        /// Gets information about a channel.
        /// </summary>
        public ChannelInfo GetChannelInfo(string channel)
        {
            if (_channels.TryGetValue(channel, out ChannelState state))
            {
                return new ChannelInfo
                {
                    Name = channel,
                    SubscriberCount = state.Subscribers.Count,
                    TotalMessages = state.TotalMessages,
                    CreatedUtc = state.CreatedUtc,
                    LastActivityUtc = state.LastActivityUtc
                };
            }

            return null;
        }


        /// <summary>
        /// Gets all active channel names.
        /// </summary>
        public List<string> GetChannelNames()
        {
            return _channels.Keys.ToList();
        }


        /// <summary>
        /// Gets message history for a channel.
        /// </summary>
        public List<ChannelMessage> GetHistory(string channel, int count = 50)
        {
            if (_channels.TryGetValue(channel, out ChannelState state))
            {
                List<ChannelMessage> history = state.History.ToList();
                int skip = Math.Max(0, history.Count - count);
                return history.Skip(skip).ToList();
            }

            return new List<ChannelMessage>();
        }


        /// <summary>
        /// Gets the subscriber count for a channel.
        /// </summary>
        public int GetSubscriberCount(string channel)
        {
            if (_channels.TryGetValue(channel, out ChannelState state))
            {
                return state.Subscribers.Count;
            }

            return 0;
        }


        // ── Internal ──────────────────────────────────────────────────────


        private class ChannelState
        {
            public ConcurrentDictionary<string, bool> Subscribers { get; } =
                new ConcurrentDictionary<string, bool>();

            public ConcurrentQueue<ChannelMessage> History { get; } =
                new ConcurrentQueue<ChannelMessage>();

            public long TotalMessages { get; set; }
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
            public DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;
        }
    }
}
