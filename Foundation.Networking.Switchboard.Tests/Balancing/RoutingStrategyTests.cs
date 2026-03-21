// ============================================================================
//
// RoutingStrategyTests.cs — Unit tests for routing strategies.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Switchboard.Balancing;

namespace Foundation.Networking.Switchboard.Tests.Balancing
{
    public class RoutingStrategyTests
    {
        private List<BackendNode> CreateBackends(int count)
        {
            List<BackendNode> backends = new List<BackendNode>();

            for (int i = 0; i < count; i++)
            {
                backends.Add(new BackendNode
                {
                    Id = "backend-" + i,
                    Label = "Backend " + i,
                    Url = "http://localhost:" + (10100 + i),
                    Weight = 1,
                    IsHealthy = true,
                    ActiveConnections = 0
                });
            }

            return backends;
        }


        // ── RoundRobin ───────────────────────────────────────────────────


        [Fact]
        public void RoundRobin_DistributesEvenly()
        {
            RoundRobinStrategy strategy = new RoundRobinStrategy();
            List<BackendNode> backends = CreateBackends(3);
            Dictionary<string, int> counts = new Dictionary<string, int>();

            for (int i = 0; i < 9; i++)
            {
                BackendNode selected = strategy.Select(backends);
                if (counts.ContainsKey(selected.Id) == false)
                {
                    counts[selected.Id] = 0;
                }
                counts[selected.Id]++;
            }

            Assert.Equal(3, counts["backend-0"]);
            Assert.Equal(3, counts["backend-1"]);
            Assert.Equal(3, counts["backend-2"]);
        }


        [Fact]
        public void RoundRobin_SkipsUnhealthy()
        {
            RoundRobinStrategy strategy = new RoundRobinStrategy();
            List<BackendNode> backends = CreateBackends(3);
            backends[1].IsHealthy = false;

            for (int i = 0; i < 10; i++)
            {
                BackendNode selected = strategy.Select(backends);
                Assert.NotEqual("backend-1", selected.Id);
            }
        }


        [Fact]
        public void RoundRobin_NoHealthyBackends_ReturnsNull()
        {
            RoundRobinStrategy strategy = new RoundRobinStrategy();
            List<BackendNode> backends = CreateBackends(2);
            backends[0].IsHealthy = false;
            backends[1].IsHealthy = false;

            BackendNode selected = strategy.Select(backends);

            Assert.Null(selected);
        }


        [Fact]
        public void RoundRobin_Name_IsCorrect()
        {
            Assert.Equal("RoundRobin", new RoundRobinStrategy().Name);
        }


        // ── LeastConnections ─────────────────────────────────────────────


        [Fact]
        public void LeastConnections_SelectsLowestConnections()
        {
            LeastConnectionsStrategy strategy = new LeastConnectionsStrategy();
            List<BackendNode> backends = CreateBackends(3);
            backends[0].ActiveConnections = 10;
            backends[1].ActiveConnections = 2;
            backends[2].ActiveConnections = 5;

            BackendNode selected = strategy.Select(backends);

            Assert.Equal("backend-1", selected.Id);
        }


        [Fact]
        public void LeastConnections_SkipsUnhealthy()
        {
            LeastConnectionsStrategy strategy = new LeastConnectionsStrategy();
            List<BackendNode> backends = CreateBackends(3);
            backends[0].ActiveConnections = 10;
            backends[1].ActiveConnections = 0;
            backends[1].IsHealthy = false;
            backends[2].ActiveConnections = 5;

            BackendNode selected = strategy.Select(backends);

            Assert.Equal("backend-2", selected.Id);
        }


        [Fact]
        public void LeastConnections_Name_IsCorrect()
        {
            Assert.Equal("LeastConnections", new LeastConnectionsStrategy().Name);
        }


        // ── Weighted ─────────────────────────────────────────────────────


        [Fact]
        public void Weighted_HigherWeightGetsMoreTraffic()
        {
            WeightedStrategy strategy = new WeightedStrategy();
            List<BackendNode> backends = CreateBackends(2);
            backends[0].Weight = 3;
            backends[1].Weight = 1;

            Dictionary<string, int> counts = new Dictionary<string, int>();

            for (int i = 0; i < 40; i++)
            {
                BackendNode selected = strategy.Select(backends);
                if (counts.ContainsKey(selected.Id) == false)
                {
                    counts[selected.Id] = 0;
                }
                counts[selected.Id]++;
            }

            //
            // Backend 0 (weight 3) should get ~3x the traffic of backend 1 (weight 1)
            //
            Assert.True(counts["backend-0"] > counts["backend-1"]);
        }


        [Fact]
        public void Weighted_Name_IsCorrect()
        {
            Assert.Equal("Weighted", new WeightedStrategy().Name);
        }


        // ── IpHash ───────────────────────────────────────────────────────


        [Fact]
        public void IpHash_SameIp_SameBackend()
        {
            IpHashStrategy strategy = new IpHashStrategy();
            List<BackendNode> backends = CreateBackends(5);

            BackendNode first = strategy.Select(backends, "192.168.1.100");
            BackendNode second = strategy.Select(backends, "192.168.1.100");
            BackendNode third = strategy.Select(backends, "192.168.1.100");

            Assert.Equal(first.Id, second.Id);
            Assert.Equal(second.Id, third.Id);
        }


        [Fact]
        public void IpHash_DifferentIps_MayDiffer()
        {
            IpHashStrategy strategy = new IpHashStrategy();
            List<BackendNode> backends = CreateBackends(10);

            //
            // With enough IPs and backends, at least some should go to different backends
            //
            HashSet<string> selectedIds = new HashSet<string>();

            for (int i = 0; i < 20; i++)
            {
                BackendNode selected = strategy.Select(backends, "192.168.1." + i);
                selectedIds.Add(selected.Id);
            }

            Assert.True(selectedIds.Count > 1);
        }


        [Fact]
        public void IpHash_NullIp_SelectsFirst()
        {
            IpHashStrategy strategy = new IpHashStrategy();
            List<BackendNode> backends = CreateBackends(3);

            BackendNode selected = strategy.Select(backends, null);

            Assert.NotNull(selected);
        }


        [Fact]
        public void IpHash_Name_IsCorrect()
        {
            Assert.Equal("IpHash", new IpHashStrategy().Name);
        }
    }
}
