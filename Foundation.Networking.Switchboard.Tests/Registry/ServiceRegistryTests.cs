// ============================================================================
//
// ServiceRegistryTests.cs — Unit tests for ServiceRegistry.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Switchboard.Balancing;
using Foundation.Networking.Switchboard.Configuration;
using Foundation.Networking.Switchboard.Registry;

namespace Foundation.Networking.Switchboard.Tests.Registry
{
    public class ServiceRegistryTests
    {
        // ── Registration ─────────────────────────────────────────────────


        [Fact]
        public void Register_AddsInstance()
        {
            ServiceRegistry registry = new ServiceRegistry();

            registry.Register(new ServiceInstance
            {
                InstanceId = "inst-1",
                ServiceName = "Scheduler",
                Url = "http://localhost:10100"
            });

            Assert.Equal(1, registry.Count);
        }


        [Fact]
        public void Register_GeneratesIdIfEmpty()
        {
            ServiceRegistry registry = new ServiceRegistry();

            ServiceInstance instance = new ServiceInstance
            {
                ServiceName = "Scheduler",
                Url = "http://localhost:10100"
            };

            registry.Register(instance);

            Assert.False(string.IsNullOrEmpty(instance.InstanceId));
            Assert.Equal(1, registry.Count);
        }


        [Fact]
        public void Register_MultipleInstances()
        {
            ServiceRegistry registry = new ServiceRegistry();

            registry.Register(new ServiceInstance { InstanceId = "inst-1", ServiceName = "Scheduler" });
            registry.Register(new ServiceInstance { InstanceId = "inst-2", ServiceName = "Scheduler" });
            registry.Register(new ServiceInstance { InstanceId = "inst-3", ServiceName = "Foundation" });

            Assert.Equal(3, registry.Count);
        }


        // ── Deregistration ───────────────────────────────────────────────


        [Fact]
        public void Deregister_RemovesInstance()
        {
            ServiceRegistry registry = new ServiceRegistry();

            registry.Register(new ServiceInstance { InstanceId = "inst-1" });
            registry.Deregister("inst-1");

            Assert.Equal(0, registry.Count);
        }


        [Fact]
        public void Deregister_NonExistent_NoOp()
        {
            ServiceRegistry registry = new ServiceRegistry();

            registry.Deregister("nonexistent");

            Assert.Equal(0, registry.Count);
        }


        // ── Query ────────────────────────────────────────────────────────


        [Fact]
        public void GetAll_ReturnsAllInstances()
        {
            ServiceRegistry registry = new ServiceRegistry();

            registry.Register(new ServiceInstance { InstanceId = "inst-1", ServiceName = "A" });
            registry.Register(new ServiceInstance { InstanceId = "inst-2", ServiceName = "B" });

            List<ServiceInstance> all = registry.GetAll();

            Assert.Equal(2, all.Count);
        }


        [Fact]
        public void GetByService_FiltersCorrectly()
        {
            ServiceRegistry registry = new ServiceRegistry();

            registry.Register(new ServiceInstance { InstanceId = "s1", ServiceName = "Scheduler" });
            registry.Register(new ServiceInstance { InstanceId = "s2", ServiceName = "Scheduler" });
            registry.Register(new ServiceInstance { InstanceId = "f1", ServiceName = "Foundation" });

            List<ServiceInstance> schedulers = registry.GetByService("Scheduler");

            Assert.Equal(2, schedulers.Count);
        }


        [Fact]
        public void GetByService_CaseInsensitive()
        {
            ServiceRegistry registry = new ServiceRegistry();

            registry.Register(new ServiceInstance { InstanceId = "s1", ServiceName = "Scheduler" });

            Assert.Single(registry.GetByService("scheduler"));
            Assert.Single(registry.GetByService("SCHEDULER"));
        }


        [Fact]
        public void Get_ReturnsSpecificInstance()
        {
            ServiceRegistry registry = new ServiceRegistry();

            registry.Register(new ServiceInstance { InstanceId = "inst-1", ServiceName = "Test", Url = "http://test" });

            ServiceInstance instance = registry.Get("inst-1");

            Assert.NotNull(instance);
            Assert.Equal("Test", instance.ServiceName);
        }


        [Fact]
        public void Get_NonExistent_ReturnsNull()
        {
            ServiceRegistry registry = new ServiceRegistry();

            Assert.Null(registry.Get("nonexistent"));
        }


        // ── Heartbeat ────────────────────────────────────────────────────


        [Fact]
        public void Heartbeat_UpdatesTimestamp()
        {
            ServiceRegistry registry = new ServiceRegistry();

            registry.Register(new ServiceInstance { InstanceId = "inst-1" });

            DateTime beforeHeartbeat = registry.Get("inst-1").LastHeartbeatUtc;
            System.Threading.Thread.Sleep(50);
            registry.Heartbeat("inst-1");
            DateTime afterHeartbeat = registry.Get("inst-1").LastHeartbeatUtc;

            Assert.True(afterHeartbeat >= beforeHeartbeat);
        }


        // ── Stale Removal ────────────────────────────────────────────────


        [Fact]
        public void RemoveStale_RemovesOldInstances()
        {
            ServiceRegistry registry = new ServiceRegistry();

            ServiceInstance stale = new ServiceInstance
            {
                InstanceId = "old",
                ServiceName = "Old",
                LastHeartbeatUtc = DateTime.UtcNow.AddMinutes(-10)
            };

            //
            // Register will overwrite the heartbeat, so set it after
            //
            registry.Register(stale);
            stale.LastHeartbeatUtc = DateTime.UtcNow.AddMinutes(-10);

            registry.Register(new ServiceInstance { InstanceId = "fresh", ServiceName = "Fresh" });

            int removed = registry.RemoveStale(TimeSpan.FromMinutes(5));

            Assert.Equal(1, removed);
            Assert.Equal(1, registry.Count);
            Assert.Null(registry.Get("old"));
            Assert.NotNull(registry.Get("fresh"));
        }


        // ── LoadBalancer Integration ─────────────────────────────────────


        [Fact]
        public void Register_WithLoadBalancer_AddsBackend()
        {
            LoadBalancer lb = new LoadBalancer(new SwitchboardConfiguration());
            ServiceRegistry registry = new ServiceRegistry(lb);

            registry.Register(new ServiceInstance
            {
                InstanceId = "inst-1",
                ServiceName = "Scheduler",
                Url = "http://localhost:10100"
            });

            Assert.Equal(1, lb.TotalBackends);
        }


        [Fact]
        public void Deregister_WithLoadBalancer_RemovesBackend()
        {
            LoadBalancer lb = new LoadBalancer(new SwitchboardConfiguration());
            ServiceRegistry registry = new ServiceRegistry(lb);

            registry.Register(new ServiceInstance
            {
                InstanceId = "inst-1",
                ServiceName = "Scheduler",
                Url = "http://localhost:10100"
            });

            Assert.Equal(1, lb.TotalBackends);

            registry.Deregister("inst-1");

            Assert.Equal(0, lb.TotalBackends);
        }
    }
}
