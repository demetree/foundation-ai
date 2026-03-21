// ============================================================================
//
// AllocationManager.cs — Thread-safe TURN allocation store with port pool.
//
// Manages the lifecycle of all active allocations on the server.  Uses a
// ConcurrentDictionary keyed by FiveTuple for O(1) lookups.  The port pool
// tracks available relay ports from a configurable range.
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using Foundation.Networking.Coturn.Configuration;

namespace Foundation.Networking.Coturn.Server
{
    /// <summary>
    ///
    /// Thread-safe store for all active TURN allocations.
    ///
    /// Manages relay port allocation from a configurable port pool and
    /// provides expiry sweeps via CleanupExpired().
    ///
    /// </summary>
    public class AllocationManager : IDisposable
    {
        //
        // All active allocations keyed by 5-tuple
        //
        private readonly ConcurrentDictionary<FiveTuple, TurnAllocation> _allocations;

        //
        // Port pool: tracks which relay ports are currently in use
        //
        private readonly ConcurrentDictionary<int, bool> _usedPorts;

        //
        // Configuration
        //
        private readonly TurnServerConfiguration _config;

        //
        // Lock for port allocation (to prevent races)
        //
        private readonly object _portLock = new object();

        //
        // RNG for port selection
        //
        private readonly Random _random = new Random();

        //
        // Whether disposed
        //
        private bool _disposed = false;


        public AllocationManager(TurnServerConfiguration config)
        {
            _config = config;
            _allocations = new ConcurrentDictionary<FiveTuple, TurnAllocation>();
            _usedPorts = new ConcurrentDictionary<int, bool>();
        }


        /// <summary>
        /// Gets the total number of active allocations.
        /// </summary>
        public int AllocationCount
        {
            get { return _allocations.Count; }
        }


        /// <summary>
        /// Returns all active allocations for admin/monitoring purposes.
        /// </summary>
        public IEnumerable<KeyValuePair<FiveTuple, TurnAllocation>> GetAllAllocations()
        {
            return _allocations;
        }


        // ── Create / Find / Remove ────────────────────────────────────────


        /// <summary>
        /// Attempts to create a new allocation for the given 5-tuple.
        ///
        /// Returns the allocation on success, or null if:
        ///   - An allocation already exists for this 5-tuple (AllocationMismatch)
        ///   - The user has exceeded their allocation quota
        ///   - No relay ports are available (InsufficientCapacity)
        /// </summary>
        public TurnAllocation TryCreateAllocation(
            FiveTuple fiveTuple,
            string username,
            string realm,
            string nonce,
            byte[] integrityKey,
            int lifetimeSeconds,
            out int errorCode)
        {
            errorCode = 0;

            //
            // Check if an allocation already exists for this 5-tuple
            //
            if (_allocations.ContainsKey(fiveTuple))
            {
                errorCode = Protocol.StunErrorCode.ALLOCATION_MISMATCH;
                return null;
            }

            //
            // Check per-user quota
            //
            if (_config.MaxAllocationsPerUser > 0)
            {
                int userAllocationCount = 0;

                foreach (KeyValuePair<FiveTuple, TurnAllocation> kvp in _allocations)
                {
                    if (kvp.Value.Username == username)
                    {
                        userAllocationCount++;
                    }
                }

                if (userAllocationCount >= _config.MaxAllocationsPerUser)
                {
                    errorCode = Protocol.StunErrorCode.ALLOCATION_QUOTA_REACHED;
                    return null;
                }
            }

            //
            // Allocate a relay port
            //
            int relayPort = AllocateRelayPort();

            if (relayPort < 0)
            {
                errorCode = Protocol.StunErrorCode.INSUFFICIENT_CAPACITY;
                return null;
            }

            //
            // Clamp the lifetime
            //
            if (lifetimeSeconds <= 0)
            {
                lifetimeSeconds = _config.DefaultLifetime;
            }

            if (lifetimeSeconds > _config.MaxLifetime)
            {
                lifetimeSeconds = _config.MaxLifetime;
            }

            //
            // Create the relay socket
            //
            Socket relaySocket = null;
            IPEndPoint relayEndPoint = null;

            try
            {
                relaySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                relayEndPoint = new IPEndPoint(IPAddress.Parse(_config.RelayAddress), relayPort);
                relaySocket.Bind(relayEndPoint);
            }
            catch
            {
                //
                // Failed to bind — release the port and return error
                //
                ReleaseRelayPort(relayPort);

                if (relaySocket != null)
                {
                    try { relaySocket.Close(); relaySocket.Dispose(); } catch { }
                }

                errorCode = Protocol.StunErrorCode.INSUFFICIENT_CAPACITY;
                return null;
            }

            //
            // Build the allocation
            //
            TurnAllocation allocation = new TurnAllocation
            {
                FiveTuple = fiveTuple,
                RelayEndPoint = relayEndPoint,
                RelaySocket = relaySocket,
                RelayPort = relayPort,
                LifetimeSeconds = lifetimeSeconds,
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(lifetimeSeconds),
                Username = username,
                Realm = realm,
                Nonce = nonce,
                IntegrityKey = integrityKey
            };

            //
            // Try to add atomically
            //
            if (_allocations.TryAdd(fiveTuple, allocation) == false)
            {
                //
                // Race: someone else added an allocation for this 5-tuple
                //
                allocation.Dispose();
                ReleaseRelayPort(relayPort);
                errorCode = Protocol.StunErrorCode.ALLOCATION_MISMATCH;
                return null;
            }

            return allocation;
        }


        /// <summary>
        /// Finds an existing allocation by 5-tuple.
        /// Returns null if not found or expired.
        /// </summary>
        public TurnAllocation FindAllocation(FiveTuple fiveTuple)
        {
            if (_allocations.TryGetValue(fiveTuple, out TurnAllocation allocation))
            {
                if (allocation.IsExpired() == false)
                {
                    return allocation;
                }
            }

            return null;
        }


        /// <summary>
        /// Removes and disposes an allocation.
        /// </summary>
        public void RemoveAllocation(FiveTuple fiveTuple)
        {
            if (_allocations.TryRemove(fiveTuple, out TurnAllocation allocation))
            {
                ReleaseRelayPort(allocation.RelayPort);
                allocation.Dispose();
            }
        }


        // ── Cleanup ───────────────────────────────────────────────────────


        /// <summary>
        /// Sweeps all allocations and removes expired ones.
        /// Also cleans up expired permissions and channels within live allocations.
        /// </summary>
        public int CleanupExpired()
        {
            int removedCount = 0;
            List<FiveTuple> expiredKeys = new List<FiveTuple>();

            foreach (KeyValuePair<FiveTuple, TurnAllocation> kvp in _allocations)
            {
                if (kvp.Value.IsExpired())
                {
                    expiredKeys.Add(kvp.Key);
                }
                else
                {
                    //
                    // Clean up expired permissions/channels within live allocations
                    //
                    kvp.Value.CleanupExpired();
                }
            }

            foreach (FiveTuple key in expiredKeys)
            {
                RemoveAllocation(key);
                removedCount++;
            }

            return removedCount;
        }


        // ── Port Pool ─────────────────────────────────────────────────────


        /// <summary>
        /// Allocates a relay port from the pool.
        /// Returns -1 if no ports are available.
        /// </summary>
        private int AllocateRelayPort()
        {
            lock (_portLock)
            {
                int poolSize = _config.RelayPortMax - _config.RelayPortMin + 1;

                if (_usedPorts.Count >= poolSize)
                {
                    return -1;
                }

                //
                // Try random ports first for better distribution
                //
                for (int attempt = 0; attempt < 100; attempt++)
                {
                    int port = _config.RelayPortMin + _random.Next(poolSize);

                    if (_usedPorts.TryAdd(port, true))
                    {
                        return port;
                    }
                }

                //
                // Fallback: linear scan
                //
                for (int port = _config.RelayPortMin; port <= _config.RelayPortMax; port++)
                {
                    if (_usedPorts.TryAdd(port, true))
                    {
                        return port;
                    }
                }

                return -1;
            }
        }


        /// <summary>
        /// Returns a relay port to the pool.
        /// </summary>
        private void ReleaseRelayPort(int port)
        {
            _usedPorts.TryRemove(port, out _);
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;

                foreach (KeyValuePair<FiveTuple, TurnAllocation> kvp in _allocations)
                {
                    kvp.Value.Dispose();
                }

                _allocations.Clear();
                _usedPorts.Clear();
            }
        }
    }
}
