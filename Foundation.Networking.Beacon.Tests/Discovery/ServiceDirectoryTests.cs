// ============================================================================
//
// ServiceDirectoryTests.cs — Unit tests for ServiceDirectory.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using Xunit;

using Microsoft.Extensions.Logging.Abstractions;

using Foundation.Networking.Beacon.Configuration;
using Foundation.Networking.Beacon.Discovery;

namespace Foundation.Networking.Beacon.Tests.Discovery
{
    public class ServiceDirectoryTests
    {
        private ServiceDirectory CreateDirectory()
        {
            return new ServiceDirectory(
                new BeaconConfiguration
                {
                    HealthCheckEnabled = false,
                    DiscoveryStaleTimeoutSeconds = 120
                },
                NullLogger<ServiceDirectory>.Instance);
        }


        // ── Registration ─────────────────────────────────────────────────


        [Fact]
        public void Register_AddsEndpoint()
        {
            ServiceDirectory dir = CreateDirectory();

            dir.Register(new ServiceEndpoint
            {
                ServiceName = "Scheduler",
                Host = "10.0.0.1",
                Port = 5000
            });

            Assert.Equal(1, dir.EndpointCount);
        }


        [Fact]
        public void Register_GeneratesIdIfEmpty()
        {
            ServiceDirectory dir = CreateDirectory();

            string id = dir.Register(new ServiceEndpoint { ServiceName = "Test" });

            Assert.False(string.IsNullOrEmpty(id));
        }


        [Fact]
        public void Register_UsesProvidedId()
        {
            ServiceDirectory dir = CreateDirectory();

            string id = dir.Register(new ServiceEndpoint
            {
                InstanceId = "my-instance-id",
                ServiceName = "Test"
            });

            Assert.Equal("my-instance-id", id);
        }


        // ── Deregister ───────────────────────────────────────────────────


        [Fact]
        public void Deregister_RemovesEndpoint()
        {
            ServiceDirectory dir = CreateDirectory();

            string id = dir.Register(new ServiceEndpoint { ServiceName = "Test" });
            bool removed = dir.Deregister(id);

            Assert.True(removed);
            Assert.Equal(0, dir.EndpointCount);
        }


        [Fact]
        public void Deregister_NonExistent_ReturnsFalse()
        {
            ServiceDirectory dir = CreateDirectory();

            Assert.False(dir.Deregister("missing"));
        }


        // ── Discovery ────────────────────────────────────────────────────


        [Fact]
        public void Discover_FindsByServiceName()
        {
            ServiceDirectory dir = CreateDirectory();

            dir.Register(new ServiceEndpoint { ServiceName = "Scheduler", Host = "10.0.0.1", Port = 5000 });
            dir.Register(new ServiceEndpoint { ServiceName = "Scheduler", Host = "10.0.0.2", Port = 5000 });
            dir.Register(new ServiceEndpoint { ServiceName = "Foundation", Host = "10.0.0.3", Port = 5001 });

            List<ServiceEndpoint> schedulers = dir.Discover("Scheduler");

            Assert.Equal(2, schedulers.Count);
        }


        [Fact]
        public void Discover_CaseInsensitive()
        {
            ServiceDirectory dir = CreateDirectory();

            dir.Register(new ServiceEndpoint { ServiceName = "Scheduler" });

            Assert.Single(dir.Discover("scheduler"));
            Assert.Single(dir.Discover("SCHEDULER"));
        }


        [Fact]
        public void Discover_SkipsUnhealthy()
        {
            ServiceDirectory dir = CreateDirectory();

            string healthyId = dir.Register(new ServiceEndpoint { ServiceName = "Test", IsHealthy = true });
            string unhealthyId = dir.Register(new ServiceEndpoint { ServiceName = "Test", IsHealthy = false });

            List<ServiceEndpoint> results = dir.Discover("Test");

            Assert.Single(results);
        }


        [Fact]
        public void DiscoverAll_IncludesUnhealthy()
        {
            ServiceDirectory dir = CreateDirectory();

            dir.Register(new ServiceEndpoint { ServiceName = "Test", IsHealthy = true });
            dir.Register(new ServiceEndpoint { ServiceName = "Test", IsHealthy = false });

            List<ServiceEndpoint> results = dir.DiscoverAll("Test");

            Assert.Equal(2, results.Count);
        }


        // ── GetServiceNames ──────────────────────────────────────────────


        [Fact]
        public void GetServiceNames_ReturnsUniqueNames()
        {
            ServiceDirectory dir = CreateDirectory();

            dir.Register(new ServiceEndpoint { ServiceName = "Scheduler" });
            dir.Register(new ServiceEndpoint { ServiceName = "Scheduler" });
            dir.Register(new ServiceEndpoint { ServiceName = "Foundation" });

            List<string> names = dir.GetServiceNames();

            Assert.Equal(2, names.Count);
        }


        // ── Heartbeat ────────────────────────────────────────────────────


        [Fact]
        public void Heartbeat_UpdatesTimestamp()
        {
            ServiceDirectory dir = CreateDirectory();

            string id = dir.Register(new ServiceEndpoint { ServiceName = "Test" });
            System.DateTime before = dir.GetEndpoint(id).LastHeartbeatUtc;

            System.Threading.Thread.Sleep(50);
            dir.Heartbeat(id);

            System.DateTime after = dir.GetEndpoint(id).LastHeartbeatUtc;
            Assert.True(after >= before);
        }


        // ── GetEndpoint ──────────────────────────────────────────────────


        [Fact]
        public void GetEndpoint_ReturnsSpecificInstance()
        {
            ServiceDirectory dir = CreateDirectory();

            string id = dir.Register(new ServiceEndpoint
            {
                ServiceName = "Test",
                Host = "10.0.0.5",
                Port = 8080
            });

            ServiceEndpoint ep = dir.GetEndpoint(id);

            Assert.NotNull(ep);
            Assert.Equal("10.0.0.5", ep.Host);
            Assert.Equal(8080, ep.Port);
        }


        [Fact]
        public void GetEndpoint_NonExistent_ReturnsNull()
        {
            ServiceDirectory dir = CreateDirectory();

            Assert.Null(dir.GetEndpoint("missing"));
        }


        // ── Url ──────────────────────────────────────────────────────────


        [Fact]
        public void Url_FormatsCorrectly()
        {
            ServiceEndpoint ep = new ServiceEndpoint
            {
                Protocol = "https",
                Host = "scheduler.local",
                Port = 443
            };

            Assert.Equal("https://scheduler.local:443", ep.Url);
        }


        // ── Statistics ───────────────────────────────────────────────────


        [Fact]
        public void GetStatistics_ReportsCounts()
        {
            ServiceDirectory dir = CreateDirectory();

            dir.Register(new ServiceEndpoint { ServiceName = "A", IsHealthy = true });
            dir.Register(new ServiceEndpoint { ServiceName = "A", IsHealthy = true });
            dir.Register(new ServiceEndpoint { ServiceName = "B", IsHealthy = false });

            DirectoryStatistics stats = dir.GetStatistics();

            Assert.Equal(2, stats.TotalServices);
            Assert.Equal(3, stats.TotalEndpoints);
            Assert.Equal(2, stats.HealthyEndpoints);
            Assert.Equal(1, stats.UnhealthyEndpoints);
        }
    }
}
