// ============================================================================
//
// LoadBalancer.cs — Core load balancing engine.
//
// Manages backend nodes, routing strategy, and provides the primary
// Select() method for choosing a backend for each request.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

using Foundation.Networking.Switchboard.Configuration;

namespace Foundation.Networking.Switchboard.Balancing
{
    /// <summary>
    ///
    /// Core load balancer that manages backend nodes and delegates
    /// selection to a pluggable routing strategy.
    ///
    /// </summary>
    public class LoadBalancer
    {
        private readonly SwitchboardConfiguration _config;
        private readonly IRoutingStrategy _strategy;
        private readonly List<BackendNode> _backends;
        private readonly object _lock = new object();


        public LoadBalancer(SwitchboardConfiguration config)
        {
            _config = config;
            _strategy = CreateStrategy(config.Strategy);
            _backends = new List<BackendNode>();

            //
            // Initialize backend nodes from configuration
            //
            foreach (SwitchboardBackend backend in config.Backends)
            {
                if (backend.Enabled == true)
                {
                    _backends.Add(new BackendNode
                    {
                        Id = backend.Id,
                        Label = backend.Label,
                        Url = backend.Url,
                        Weight = backend.Weight,
                        MaxConnections = backend.MaxConnections,
                        IsHealthy = true
                    });
                }
            }
        }


        /// <summary>
        /// The name of the active routing strategy.
        /// </summary>
        public string StrategyName => _strategy.Name;


        /// <summary>
        /// Number of total backends.
        /// </summary>
        public int TotalBackends
        {
            get
            {
                lock (_lock)
                {
                    return _backends.Count;
                }
            }
        }


        /// <summary>
        /// Number of healthy backends.
        /// </summary>
        public int HealthyBackends
        {
            get
            {
                lock (_lock)
                {
                    return _backends.Count(b => b.IsHealthy == true);
                }
            }
        }


        /// <summary>
        /// Gets all backend nodes (snapshot).
        /// </summary>
        public List<BackendNode> GetBackends()
        {
            lock (_lock)
            {
                return _backends.ToList();
            }
        }


        /// <summary>
        /// Selects the next backend using the configured strategy.
        /// Returns null if no backends are available.
        /// </summary>
        public BackendNode Select(string clientIp = null)
        {
            lock (_lock)
            {
                BackendNode selected = _strategy.Select(_backends.AsReadOnly(), clientIp);

                if (selected != null)
                {
                    selected.ActiveConnections++;
                    selected.TotalRequests++;
                }

                return selected;
            }
        }


        /// <summary>
        /// Releases a connection back to the backend.
        /// </summary>
        public void Release(BackendNode backend)
        {
            if (backend == null)
            {
                return;
            }

            lock (_lock)
            {
                if (backend.ActiveConnections > 0)
                {
                    backend.ActiveConnections--;
                }
            }
        }


        /// <summary>
        /// Records an error on a backend.
        /// </summary>
        public void RecordError(BackendNode backend)
        {
            if (backend == null)
            {
                return;
            }

            lock (_lock)
            {
                backend.TotalErrors++;
            }
        }


        /// <summary>
        /// Sets the health status of a backend by ID.
        /// </summary>
        public void SetHealth(string backendId, bool isHealthy)
        {
            lock (_lock)
            {
                BackendNode backend = _backends.FirstOrDefault(b => b.Id == backendId);

                if (backend != null)
                {
                    backend.IsHealthy = isHealthy;
                }
            }
        }


        /// <summary>
        /// Dynamically adds a new backend to the pool.
        /// </summary>
        public void AddBackend(BackendNode backend)
        {
            lock (_lock)
            {
                //
                // Don't add duplicates
                //
                if (_backends.Any(b => b.Id == backend.Id) == false)
                {
                    _backends.Add(backend);
                }
            }
        }


        /// <summary>
        /// Removes a backend from the pool by ID.
        /// </summary>
        public void RemoveBackend(string backendId)
        {
            lock (_lock)
            {
                _backends.RemoveAll(b => b.Id == backendId);
            }
        }


        // ── Internal ──────────────────────────────────────────────────────


        private static IRoutingStrategy CreateStrategy(string name)
        {
            switch (name?.ToLowerInvariant())
            {
                case "leastconnections":
                    return new LeastConnectionsStrategy();

                case "weighted":
                    return new WeightedStrategy();

                case "iphash":
                    return new IpHashStrategy();

                case "roundrobin":
                default:
                    return new RoundRobinStrategy();
            }
        }
    }
}
