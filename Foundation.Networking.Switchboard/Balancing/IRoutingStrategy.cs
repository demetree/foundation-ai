// ============================================================================
//
// IRoutingStrategy.cs — Routing strategy interface and implementations.
//
// Four pluggable strategies: RoundRobin, LeastConnections, Weighted, IpHash.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Foundation.Networking.Switchboard.Balancing
{
    /// <summary>
    /// Runtime state of a backend for routing decisions.
    /// </summary>
    public class BackendNode
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Weight { get; set; } = 1;
        public bool IsHealthy { get; set; } = true;
        public int ActiveConnections { get; set; }
        public long TotalRequests { get; set; }
        public long TotalErrors { get; set; }
        public int MaxConnections { get; set; } = 100;
    }


    /// <summary>
    /// Interface for backend selection strategies.
    /// </summary>
    public interface IRoutingStrategy
    {
        /// <summary>
        /// Selects the next backend from the available pool.
        /// Returns null if no backends are available.
        /// </summary>
        BackendNode Select(IReadOnlyList<BackendNode> backends, string clientIp = null);

        /// <summary>
        /// Strategy name for logging.
        /// </summary>
        string Name { get; }
    }


    /// <summary>
    /// Round-robin: rotates through backends in sequence.
    /// </summary>
    public class RoundRobinStrategy : IRoutingStrategy
    {
        private int _index = -1;

        public string Name => "RoundRobin";


        public BackendNode Select(IReadOnlyList<BackendNode> backends, string clientIp = null)
        {
            List<BackendNode> healthy = backends.Where(b => b.IsHealthy == true).ToList();

            if (healthy.Count == 0)
            {
                return null;
            }

            int next = Interlocked.Increment(ref _index);
            int index = Math.Abs(next) % healthy.Count;

            return healthy[index];
        }
    }


    /// <summary>
    /// Least connections: selects the backend with the fewest active connections.
    /// </summary>
    public class LeastConnectionsStrategy : IRoutingStrategy
    {
        public string Name => "LeastConnections";


        public BackendNode Select(IReadOnlyList<BackendNode> backends, string clientIp = null)
        {
            BackendNode selected = null;
            int minConnections = int.MaxValue;

            foreach (BackendNode backend in backends)
            {
                if (backend.IsHealthy == true && backend.ActiveConnections < minConnections)
                {
                    minConnections = backend.ActiveConnections;
                    selected = backend;
                }
            }

            return selected;
        }
    }


    /// <summary>
    /// Weighted: distributes traffic proportionally to backend weights.
    /// Uses weighted round-robin.
    /// </summary>
    public class WeightedStrategy : IRoutingStrategy
    {
        private int _counter = -1;

        public string Name => "Weighted";


        public BackendNode Select(IReadOnlyList<BackendNode> backends, string clientIp = null)
        {
            List<BackendNode> healthy = backends.Where(b => b.IsHealthy == true).ToList();

            if (healthy.Count == 0)
            {
                return null;
            }

            //
            // Build a weighted list (repeat each backend by its weight)
            //
            List<BackendNode> weightedList = new List<BackendNode>();

            foreach (BackendNode backend in healthy)
            {
                int weight = Math.Max(1, backend.Weight);

                for (int i = 0; i < weight; i++)
                {
                    weightedList.Add(backend);
                }
            }

            if (weightedList.Count == 0)
            {
                return null;
            }

            int next = Interlocked.Increment(ref _counter);
            int index = Math.Abs(next) % weightedList.Count;

            return weightedList[index];
        }
    }


    /// <summary>
    /// IP hash: routes the same client IP to the same backend (session affinity).
    /// </summary>
    public class IpHashStrategy : IRoutingStrategy
    {
        public string Name => "IpHash";


        public BackendNode Select(IReadOnlyList<BackendNode> backends, string clientIp = null)
        {
            List<BackendNode> healthy = backends.Where(b => b.IsHealthy == true).ToList();

            if (healthy.Count == 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(clientIp))
            {
                return healthy[0];
            }

            //
            // Hash the client IP to a consistent index
            //
            int hash = GetStableHash(clientIp);
            int index = Math.Abs(hash) % healthy.Count;

            return healthy[index];
        }


        /// <summary>
        /// Produces a stable hash from a string (consistent across process restarts).
        /// </summary>
        private static int GetStableHash(string input)
        {
            //
            // Simple FNV-1a hash for deterministic hashing
            //
            unchecked
            {
                int hash = (int)2166136261;

                foreach (char c in input)
                {
                    hash ^= c;
                    hash *= 16777619;
                }

                return hash;
            }
        }
    }
}
