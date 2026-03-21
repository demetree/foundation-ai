// ============================================================================
//
// LoadBalancerTests.cs — Unit tests for LoadBalancer.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Switchboard.Balancing;
using Foundation.Networking.Switchboard.Configuration;

namespace Foundation.Networking.Switchboard.Tests.Balancing
{
    public class LoadBalancerTests
    {
        private SwitchboardConfiguration CreateConfig(string strategy = "RoundRobin", int backendCount = 3)
        {
            SwitchboardConfiguration config = new SwitchboardConfiguration
            {
                Strategy = strategy,
                Backends = new List<SwitchboardBackend>()
            };

            for (int i = 0; i < backendCount; i++)
            {
                config.Backends.Add(new SwitchboardBackend
                {
                    Id = "backend-" + i,
                    Label = "Backend " + i,
                    Url = "http://localhost:" + (10100 + i),
                    Weight = 1,
                    Enabled = true
                });
            }

            return config;
        }


        // ── Initialization ───────────────────────────────────────────────


        [Fact]
        public void Constructor_LoadsBackends()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig());

            Assert.Equal(3, lb.TotalBackends);
            Assert.Equal(3, lb.HealthyBackends);
        }


        [Fact]
        public void Constructor_SkipsDisabledBackends()
        {
            SwitchboardConfiguration config = CreateConfig();
            config.Backends[1].Enabled = false;

            LoadBalancer lb = new LoadBalancer(config);

            Assert.Equal(2, lb.TotalBackends);
        }


        [Fact]
        public void Constructor_SetsStrategyName()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig("LeastConnections"));

            Assert.Equal("LeastConnections", lb.StrategyName);
        }


        // ── Select ───────────────────────────────────────────────────────


        [Fact]
        public void Select_ReturnsBackend()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig());

            BackendNode backend = lb.Select();

            Assert.NotNull(backend);
            Assert.True(backend.TotalRequests > 0);
        }


        [Fact]
        public void Select_IncrementsConnections()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig(backendCount: 1));

            BackendNode backend = lb.Select();

            Assert.Equal(1, backend.ActiveConnections);
            Assert.Equal(1, backend.TotalRequests);
        }


        [Fact]
        public void Select_NoBackends_ReturnsNull()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig(backendCount: 0));

            BackendNode backend = lb.Select();

            Assert.Null(backend);
        }


        [Fact]
        public void Select_AllUnhealthy_ReturnsNull()
        {
            SwitchboardConfiguration config = CreateConfig(backendCount: 2);
            LoadBalancer lb = new LoadBalancer(config);

            lb.SetHealth("backend-0", false);
            lb.SetHealth("backend-1", false);

            BackendNode backend = lb.Select();

            Assert.Null(backend);
        }


        // ── Release ──────────────────────────────────────────────────────


        [Fact]
        public void Release_DecrementsConnections()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig(backendCount: 1));

            BackendNode backend = lb.Select();
            Assert.Equal(1, backend.ActiveConnections);

            lb.Release(backend);
            Assert.Equal(0, backend.ActiveConnections);
        }


        [Fact]
        public void Release_Null_DoesNotThrow()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig());

            lb.Release(null);
        }


        // ── Health ───────────────────────────────────────────────────────


        [Fact]
        public void SetHealth_MarksBackendUnhealthy()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig());

            lb.SetHealth("backend-0", false);

            Assert.Equal(2, lb.HealthyBackends);
        }


        [Fact]
        public void SetHealth_MarksBackendHealthyAgain()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig());

            lb.SetHealth("backend-0", false);
            Assert.Equal(2, lb.HealthyBackends);

            lb.SetHealth("backend-0", true);
            Assert.Equal(3, lb.HealthyBackends);
        }


        // ── Dynamic Backends ─────────────────────────────────────────────


        [Fact]
        public void AddBackend_IncreasesCount()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig(backendCount: 1));

            lb.AddBackend(new BackendNode
            {
                Id = "new-backend",
                Label = "New Backend",
                Url = "http://localhost:10200",
                IsHealthy = true
            });

            Assert.Equal(2, lb.TotalBackends);
        }


        [Fact]
        public void AddBackend_DuplicateId_Ignored()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig(backendCount: 1));

            lb.AddBackend(new BackendNode { Id = "backend-0" });

            Assert.Equal(1, lb.TotalBackends);
        }


        [Fact]
        public void RemoveBackend_DecreasesCount()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig());

            lb.RemoveBackend("backend-1");

            Assert.Equal(2, lb.TotalBackends);
        }


        [Fact]
        public void RemoveBackend_NonExistent_NoOp()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig());

            lb.RemoveBackend("nonexistent");

            Assert.Equal(3, lb.TotalBackends);
        }


        // ── RecordError ──────────────────────────────────────────────────


        [Fact]
        public void RecordError_IncrementsErrorCount()
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig(backendCount: 1));

            BackendNode backend = lb.Select();
            lb.RecordError(backend);

            Assert.Equal(1, backend.TotalErrors);
        }


        // ── Strategy Selection ───────────────────────────────────────────


        [Theory]
        [InlineData("RoundRobin", "RoundRobin")]
        [InlineData("LeastConnections", "LeastConnections")]
        [InlineData("Weighted", "Weighted")]
        [InlineData("IpHash", "IpHash")]
        [InlineData("unknown", "RoundRobin")]
        [InlineData(null, "RoundRobin")]
        public void Constructor_SelectsCorrectStrategy(string inputStrategy, string expectedName)
        {
            LoadBalancer lb = new LoadBalancer(CreateConfig(inputStrategy));

            Assert.Equal(expectedName, lb.StrategyName);
        }
    }
}
