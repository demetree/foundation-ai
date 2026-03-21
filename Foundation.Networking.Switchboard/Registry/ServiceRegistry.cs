// ============================================================================
//
// ServiceRegistry.cs — Dynamic service registration and discovery.
//
// Allows Foundation applications to register themselves at startup and
// be discovered by the load balancer.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Foundation.Networking.Switchboard.Balancing;

namespace Foundation.Networking.Switchboard.Registry
{
    /// <summary>
    /// A registered service instance.
    /// </summary>
    public class ServiceInstance
    {
        /// <summary>
        /// Unique instance ID.
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Service name (e.g., "Scheduler", "Foundation").
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Service URL.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Health check path.
        /// </summary>
        public string HealthCheckPath { get; set; } = "/api/health";

        /// <summary>
        /// Weight for load balancing.
        /// </summary>
        public int Weight { get; set; } = 1;

        /// <summary>
        /// Custom metadata tags.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// When this instance was registered.
        /// </summary>
        public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last heartbeat from this instance.
        /// </summary>
        public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;
    }


    /// <summary>
    ///
    /// Service registry for dynamic backend registration and discovery.
    ///
    /// Services register on startup and can be discovered by the load balancer
    /// or other services.  Includes heartbeat tracking for stale removal.
    ///
    /// </summary>
    public class ServiceRegistry
    {
        private readonly ConcurrentDictionary<string, ServiceInstance> _instances;
        private readonly LoadBalancer _loadBalancer;


        /// <summary>
        /// Creates a ServiceRegistry optionally linked to a LoadBalancer
        /// for automatic backend management.
        /// </summary>
        public ServiceRegistry(LoadBalancer loadBalancer = null)
        {
            _instances = new ConcurrentDictionary<string, ServiceInstance>();
            _loadBalancer = loadBalancer;
        }


        /// <summary>
        /// Number of registered instances.
        /// </summary>
        public int Count => _instances.Count;


        /// <summary>
        /// Registers a service instance.
        /// </summary>
        public void Register(ServiceInstance instance)
        {
            if (string.IsNullOrWhiteSpace(instance.InstanceId))
            {
                instance.InstanceId = Guid.NewGuid().ToString("N");
            }

            instance.RegisteredAtUtc = DateTime.UtcNow;
            instance.LastHeartbeatUtc = DateTime.UtcNow;

            _instances[instance.InstanceId] = instance;

            //
            // Auto-add to load balancer if linked
            //
            if (_loadBalancer != null)
            {
                _loadBalancer.AddBackend(new BackendNode
                {
                    Id = instance.InstanceId,
                    Label = instance.ServiceName + " (" + instance.InstanceId.Substring(0, Math.Min(8, instance.InstanceId.Length)) + ")",
                    Url = instance.Url,
                    Weight = instance.Weight,
                    IsHealthy = true
                });
            }
        }


        /// <summary>
        /// Deregisters a service instance.
        /// </summary>
        public void Deregister(string instanceId)
        {
            _instances.TryRemove(instanceId, out _);

            if (_loadBalancer != null)
            {
                _loadBalancer.RemoveBackend(instanceId);
            }
        }


        /// <summary>
        /// Updates the heartbeat for an instance.
        /// </summary>
        public void Heartbeat(string instanceId)
        {
            if (_instances.TryGetValue(instanceId, out ServiceInstance instance))
            {
                instance.LastHeartbeatUtc = DateTime.UtcNow;
            }
        }


        /// <summary>
        /// Gets all registered instances.
        /// </summary>
        public List<ServiceInstance> GetAll()
        {
            return _instances.Values.ToList();
        }


        /// <summary>
        /// Gets all instances for a specific service name.
        /// </summary>
        public List<ServiceInstance> GetByService(string serviceName)
        {
            return _instances.Values
                .Where(i => string.Equals(i.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }


        /// <summary>
        /// Gets a specific instance by ID.
        /// </summary>
        public ServiceInstance Get(string instanceId)
        {
            _instances.TryGetValue(instanceId, out ServiceInstance instance);
            return instance;
        }


        /// <summary>
        /// Removes instances that haven't sent a heartbeat within the specified timeout.
        /// </summary>
        public int RemoveStale(TimeSpan timeout)
        {
            DateTime cutoff = DateTime.UtcNow - timeout;
            List<string> staleIds = new List<string>();

            foreach (var kvp in _instances)
            {
                if (kvp.Value.LastHeartbeatUtc < cutoff)
                {
                    staleIds.Add(kvp.Key);
                }
            }

            foreach (string id in staleIds)
            {
                Deregister(id);
            }

            return staleIds.Count;
        }
    }
}
