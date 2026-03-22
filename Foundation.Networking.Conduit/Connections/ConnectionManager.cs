// ============================================================================
//
// ConnectionManager.cs — WebSocket connection tracking and management.
//
// Manages connected clients, tracks metadata, and enforces connection limits.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Foundation.Networking.Conduit.Configuration;

namespace Foundation.Networking.Conduit.Connections
{
    /// <summary>
    /// Represents a connected client.
    /// </summary>
    public class ClientConnection
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string ClientIp { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime ConnectedUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;
        public long MessagesSent { get; set; }
        public long MessagesReceived { get; set; }
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public HashSet<string> Channels { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }


    /// <summary>
    /// Connection manager statistics.
    /// </summary>
    public class ConnectionManagerStatistics
    {
        public int ActiveConnections { get; set; }
        public int MaxConnections { get; set; }
        public long TotalConnections { get; set; }
        public long TotalDisconnections { get; set; }
        public long TotalMessagesSent { get; set; }
        public long TotalMessagesReceived { get; set; }
    }


    /// <summary>
    ///
    /// Manages WebSocket connections with metadata tracking and lifecycle management.
    ///
    /// </summary>
    public class ConnectionManager
    {
        private readonly ConduitConfiguration _config;
        private readonly ConcurrentDictionary<string, ClientConnection> _connections;

        private long _totalConnections;
        private long _totalDisconnections;
        private long _totalMessagesSent;
        private long _totalMessagesReceived;


        public ConnectionManager(ConduitConfiguration config)
        {
            _config = config;
            _connections = new ConcurrentDictionary<string, ClientConnection>();
        }


        /// <summary>
        /// Number of active connections.
        /// </summary>
        public int ActiveCount => _connections.Count;


        /// <summary>
        /// Whether the manager can accept new connections.
        /// </summary>
        public bool CanAccept => _connections.Count < _config.MaxConnections;


        // ── Connection Lifecycle ──────────────────────────────────────────


        /// <summary>
        /// Registers a new connection. Returns the connection or null if limit reached.
        /// </summary>
        public ClientConnection Connect(string connectionId = null, string clientIp = "", string userId = "")
        {
            if (CanAccept == false)
            {
                return null;
            }

            string id = connectionId ?? Guid.NewGuid().ToString("N");

            ClientConnection conn = new ClientConnection
            {
                ConnectionId = id,
                ClientIp = clientIp,
                UserId = userId,
                ConnectedUtc = DateTime.UtcNow,
                LastActivityUtc = DateTime.UtcNow
            };

            if (_connections.TryAdd(id, conn))
            {
                Interlocked.Increment(ref _totalConnections);
                return conn;
            }

            return null;
        }


        /// <summary>
        /// Disconnects a client.
        /// </summary>
        public bool Disconnect(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out _))
            {
                Interlocked.Increment(ref _totalDisconnections);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Gets a connection by ID.
        /// </summary>
        public ClientConnection Get(string connectionId)
        {
            _connections.TryGetValue(connectionId, out ClientConnection conn);
            return conn;
        }


        /// <summary>
        /// Updates the last activity timestamp.
        /// </summary>
        public void Touch(string connectionId)
        {
            if (_connections.TryGetValue(connectionId, out ClientConnection conn))
            {
                conn.LastActivityUtc = DateTime.UtcNow;
            }
        }


        // ── Message Tracking ──────────────────────────────────────────────


        /// <summary>
        /// Records a sent message.
        /// </summary>
        public void RecordSent(string connectionId, long bytes)
        {
            if (_connections.TryGetValue(connectionId, out ClientConnection conn))
            {
                conn.MessagesSent++;
                conn.BytesSent += bytes;
                conn.LastActivityUtc = DateTime.UtcNow;
                Interlocked.Increment(ref _totalMessagesSent);
            }
        }


        /// <summary>
        /// Records a received message.
        /// </summary>
        public void RecordReceived(string connectionId, long bytes)
        {
            if (_connections.TryGetValue(connectionId, out ClientConnection conn))
            {
                conn.MessagesReceived++;
                conn.BytesReceived += bytes;
                conn.LastActivityUtc = DateTime.UtcNow;
                Interlocked.Increment(ref _totalMessagesReceived);
            }
        }


        // ── Query ─────────────────────────────────────────────────────────


        /// <summary>
        /// Gets all active connections.
        /// </summary>
        public List<ClientConnection> GetAll()
        {
            return _connections.Values.ToList();
        }


        /// <summary>
        /// Finds connections by user ID.
        /// </summary>
        public List<ClientConnection> GetByUser(string userId)
        {
            return _connections.Values
                .Where(c => string.Equals(c.UserId, userId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }


        /// <summary>
        /// Finds connections subscribed to a specific channel.
        /// </summary>
        public List<ClientConnection> GetByChannel(string channel)
        {
            return _connections.Values
                .Where(c => c.Channels.Contains(channel))
                .ToList();
        }


        // ── Idle Cleanup ──────────────────────────────────────────────────


        /// <summary>
        /// Removes connections that have been idle beyond the configured timeout.
        /// Returns the number of connections removed.
        /// </summary>
        public int RemoveIdle()
        {
            DateTime cutoff = DateTime.UtcNow.AddSeconds(-_config.IdleTimeoutSeconds);
            List<string> idle = new List<string>();

            foreach (var kvp in _connections)
            {
                if (kvp.Value.LastActivityUtc < cutoff)
                {
                    idle.Add(kvp.Key);
                }
            }

            foreach (string id in idle)
            {
                Disconnect(id);
            }

            return idle.Count;
        }


        // ── Statistics ────────────────────────────────────────────────────


        public ConnectionManagerStatistics GetStatistics()
        {
            return new ConnectionManagerStatistics
            {
                ActiveConnections = _connections.Count,
                MaxConnections = _config.MaxConnections,
                TotalConnections = Interlocked.Read(ref _totalConnections),
                TotalDisconnections = Interlocked.Read(ref _totalDisconnections),
                TotalMessagesSent = Interlocked.Read(ref _totalMessagesSent),
                TotalMessagesReceived = Interlocked.Read(ref _totalMessagesReceived)
            };
        }
    }
}
