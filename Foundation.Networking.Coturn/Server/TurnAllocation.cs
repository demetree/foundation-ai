// ============================================================================
//
// TurnAllocation.cs — Represents a single TURN relay allocation.
//
// Per RFC 5766 §5, an allocation is the core data structure on the server.
// It contains the relay transport address, permissions, channel bindings,
// and the relay UDP socket.  Each allocation belongs to exactly one client,
// identified by its 5-tuple.
//
// The allocation owns the relay socket and is responsible for disposing it.
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Foundation.Networking.Coturn.Server
{
    /// <summary>
    ///
    /// Represents a single TURN allocation — the server-side data structure
    /// that holds the relay socket, permissions, and channel bindings for
    /// one client 5-tuple.
    ///
    /// </summary>
    public class TurnAllocation : IDisposable
    {
        //
        // The 5-tuple that identifies this allocation
        //
        public FiveTuple FiveTuple { get; set; }

        //
        // The relay transport address assigned to this allocation
        // (the address that peers send data to)
        //
        public IPEndPoint RelayEndPoint { get; set; }

        //
        // The UDP socket used for relaying data to/from peers
        //
        public Socket RelaySocket { get; set; }

        //
        // The relay port number (also stored in RelayEndPoint, but convenient)
        //
        public int RelayPort { get; set; }

        //
        // When this allocation expires (UTC)
        //
        public DateTime ExpiresAtUtc { get; set; }

        //
        // The lifetime in seconds (as granted to the client)
        //
        public int LifetimeSeconds { get; set; }

        //
        // Authentication credentials used to create this allocation
        //
        public string Username { get; set; }
        public string Realm { get; set; }
        public string Nonce { get; set; }

        //
        // The HMAC key used for MESSAGE-INTEGRITY on this allocation
        // (cached to avoid recomputing for every request)
        //
        public byte[] IntegrityKey { get; set; }

        //
        // Permissions: keyed by peer IP address (no port)
        //
        public Dictionary<string, TurnPermission> Permissions { get; set; }

        //
        // Channel bindings: keyed by channel number
        //
        public Dictionary<ushort, TurnChannel> Channels { get; set; }

        //
        // Reverse channel lookup: keyed by peer endpoint string → channel number
        //
        public Dictionary<string, ushort> ChannelsByPeer { get; set; }

        //
        // Lock object for thread-safe access to permissions and channels
        //
        private readonly object _lock = new object();

        //
        // Whether this allocation has been disposed
        //
        private bool _disposed = false;


        public TurnAllocation()
        {
            Permissions = new Dictionary<string, TurnPermission>();
            Channels = new Dictionary<ushort, TurnChannel>();
            ChannelsByPeer = new Dictionary<string, ushort>();
        }


        // ── Lifetime ──────────────────────────────────────────────────────


        /// <summary>
        /// Returns true if this allocation has expired.
        /// </summary>
        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpiresAtUtc;
        }


        /// <summary>
        /// Refreshes the allocation with a new lifetime.
        /// A lifetime of 0 means the allocation should be deleted immediately.
        /// </summary>
        public void Refresh(int lifetimeSeconds)
        {
            LifetimeSeconds = lifetimeSeconds;
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(lifetimeSeconds);
        }


        // ── Permissions ───────────────────────────────────────────────────


        /// <summary>
        /// Checks if a peer IP has an active (non-expired) permission.
        /// </summary>
        public bool HasPermission(IPAddress peerAddress)
        {
            string key = peerAddress.ToString();

            lock (_lock)
            {
                if (Permissions.TryGetValue(key, out TurnPermission permission))
                {
                    if (permission.IsExpired() == false)
                    {
                        return true;
                    }

                    //
                    // Expired — remove it
                    //
                    Permissions.Remove(key);
                }
            }

            return false;
        }


        /// <summary>
        /// Adds or refreshes a permission for the given peer IP.
        /// </summary>
        public void AddOrRefreshPermission(IPAddress peerAddress)
        {
            string key = peerAddress.ToString();

            lock (_lock)
            {
                if (Permissions.TryGetValue(key, out TurnPermission existing))
                {
                    existing.Refresh();
                }
                else
                {
                    Permissions[key] = new TurnPermission(peerAddress);
                }
            }
        }


        // ── Channel Bindings ──────────────────────────────────────────────


        /// <summary>
        /// Adds or refreshes a channel binding.
        /// </summary>
        public bool AddOrRefreshChannel(ushort channelNumber, IPEndPoint peerEndPoint)
        {
            if (TurnChannel.IsValidChannelNumber(channelNumber) == false)
            {
                return false;
            }

            string peerKey = peerEndPoint.ToString();

            lock (_lock)
            {
                //
                // Check if this channel number is already bound to a different peer
                //
                if (Channels.TryGetValue(channelNumber, out TurnChannel existingChannel))
                {
                    if (existingChannel.PeerEndPoint.Equals(peerEndPoint) == false)
                    {
                        //
                        // Channel number already bound to a different peer — reject
                        //
                        return false;
                    }

                    //
                    // Same peer — just refresh
                    //
                    existingChannel.Refresh();
                    return true;
                }

                //
                // Check if this peer is already bound to a different channel
                //
                if (ChannelsByPeer.TryGetValue(peerKey, out ushort existingChannelNumber))
                {
                    if (existingChannelNumber != channelNumber)
                    {
                        //
                        // Peer already bound to a different channel — reject
                        //
                        return false;
                    }
                }

                //
                // Create the new binding
                //
                TurnChannel channel = new TurnChannel(channelNumber, peerEndPoint);
                Channels[channelNumber] = channel;
                ChannelsByPeer[peerKey] = channelNumber;

                //
                // Channel binding also installs/refreshes a permission
                //
                AddOrRefreshPermission(peerEndPoint.Address);

                return true;
            }
        }


        /// <summary>
        /// Finds the channel binding for a given channel number, or null if not bound/expired.
        /// </summary>
        public TurnChannel FindChannel(ushort channelNumber)
        {
            lock (_lock)
            {
                if (Channels.TryGetValue(channelNumber, out TurnChannel channel))
                {
                    if (channel.IsExpired() == false)
                    {
                        return channel;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Finds the channel number bound to a specific peer, or 0 if none.
        /// </summary>
        public ushort FindChannelForPeer(IPEndPoint peerEndPoint)
        {
            string peerKey = peerEndPoint.ToString();

            lock (_lock)
            {
                if (ChannelsByPeer.TryGetValue(peerKey, out ushort channelNumber))
                {
                    if (Channels.TryGetValue(channelNumber, out TurnChannel channel))
                    {
                        if (channel.IsExpired() == false)
                        {
                            return channelNumber;
                        }
                    }
                }
            }

            return 0;
        }


        // ── Cleanup ───────────────────────────────────────────────────────


        /// <summary>
        /// Removes expired permissions and channel bindings.
        /// </summary>
        public void CleanupExpired()
        {
            lock (_lock)
            {
                //
                // Clean expired permissions
                //
                List<string> expiredPermissions = new List<string>();

                foreach (KeyValuePair<string, TurnPermission> kvp in Permissions)
                {
                    if (kvp.Value.IsExpired())
                    {
                        expiredPermissions.Add(kvp.Key);
                    }
                }

                foreach (string key in expiredPermissions)
                {
                    Permissions.Remove(key);
                }

                //
                // Clean expired channels
                //
                List<ushort> expiredChannels = new List<ushort>();

                foreach (KeyValuePair<ushort, TurnChannel> kvp in Channels)
                {
                    if (kvp.Value.IsExpired())
                    {
                        expiredChannels.Add(kvp.Key);
                    }
                }

                foreach (ushort channelNumber in expiredChannels)
                {
                    if (Channels.TryGetValue(channelNumber, out TurnChannel channel))
                    {
                        string peerKey = channel.PeerEndPoint.ToString();
                        ChannelsByPeer.Remove(peerKey);
                    }

                    Channels.Remove(channelNumber);
                }
            }
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;

                if (RelaySocket != null)
                {
                    try
                    {
                        RelaySocket.Close();
                        RelaySocket.Dispose();
                    }
                    catch
                    {
                        // Best-effort cleanup
                    }
                }
            }
        }
    }
}
