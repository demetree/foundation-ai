// ============================================================================
//
// AllocationManagerTests.cs — Tests for allocation lifecycle and port pool.
//
// ============================================================================

using System.Net;
using Xunit;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Protocol;
using Foundation.Networking.Coturn.Server;

namespace Foundation.Networking.Coturn.Tests.Server
{
    public class AllocationManagerTests
    {
        private TurnServerConfiguration CreateConfig(int portMin = 60000, int portMax = 60010)
        {
            return new TurnServerConfiguration
            {
                RelayAddress = "127.0.0.1",
                RelayPortMin = portMin,
                RelayPortMax = portMax,
                MaxAllocationsPerUser = 5,
                DefaultLifetime = 600,
                MaxLifetime = 3600,
                Realm = "test.local",
                SharedSecret = "test-secret"
            };
        }


        private FiveTuple CreateFiveTuple(int clientPort)
        {
            return new FiveTuple(
                new IPEndPoint(IPAddress.Loopback, clientPort),
                new IPEndPoint(IPAddress.Loopback, 3478),
                StunConstants.TRANSPORT_UDP);
        }


        [Fact]
        public void TryCreateAllocation_Success()
        {
            TurnServerConfiguration config = CreateConfig();

            using (AllocationManager manager = new AllocationManager(config))
            {
                FiveTuple ft = CreateFiveTuple(5000);
                int errorCode;

                TurnAllocation allocation = manager.TryCreateAllocation(
                    ft, "testuser", "test.local", "nonce1", new byte[16], 600, out errorCode);

                Assert.NotNull(allocation);
                Assert.Equal(0, errorCode);
                Assert.Equal(1, manager.AllocationCount);
                Assert.Equal("testuser", allocation.Username);
                Assert.True(allocation.RelayPort >= 60000 && allocation.RelayPort <= 60010);

                //
                // Should be findable
                //
                TurnAllocation found = manager.FindAllocation(ft);
                Assert.NotNull(found);
                Assert.Equal(allocation.RelayPort, found.RelayPort);
            }
        }


        [Fact]
        public void TryCreateAllocation_Duplicate_ReturnsAllocationMismatch()
        {
            TurnServerConfiguration config = CreateConfig();

            using (AllocationManager manager = new AllocationManager(config))
            {
                FiveTuple ft = CreateFiveTuple(5000);
                int errorCode;

                TurnAllocation first = manager.TryCreateAllocation(
                    ft, "testuser", "test.local", "nonce1", new byte[16], 600, out errorCode);

                Assert.NotNull(first);

                //
                // Second allocation with same 5-tuple should fail
                //
                TurnAllocation second = manager.TryCreateAllocation(
                    ft, "testuser", "test.local", "nonce2", new byte[16], 600, out errorCode);

                Assert.Null(second);
                Assert.Equal(StunErrorCode.ALLOCATION_MISMATCH, errorCode);
            }
        }


        [Fact]
        public void TryCreateAllocation_QuotaExceeded_ReturnsQuotaReached()
        {
            TurnServerConfiguration config = CreateConfig();
            config.MaxAllocationsPerUser = 2;

            using (AllocationManager manager = new AllocationManager(config))
            {
                int errorCode;

                //
                // Create 2 allocations (max quota)
                //
                manager.TryCreateAllocation(
                    CreateFiveTuple(5000), "testuser", "test.local", "n1", new byte[16], 600, out errorCode);

                manager.TryCreateAllocation(
                    CreateFiveTuple(5001), "testuser", "test.local", "n2", new byte[16], 600, out errorCode);

                //
                // Third should fail
                //
                TurnAllocation third = manager.TryCreateAllocation(
                    CreateFiveTuple(5002), "testuser", "test.local", "n3", new byte[16], 600, out errorCode);

                Assert.Null(third);
                Assert.Equal(StunErrorCode.ALLOCATION_QUOTA_REACHED, errorCode);
            }
        }


        [Fact]
        public void TryCreateAllocation_PortPoolExhausted_ReturnsInsufficientCapacity()
        {
            //
            // Port range of only 2 ports
            //
            TurnServerConfiguration config = CreateConfig(60000, 60001);
            config.MaxAllocationsPerUser = 10;

            using (AllocationManager manager = new AllocationManager(config))
            {
                int errorCode;

                manager.TryCreateAllocation(
                    CreateFiveTuple(5000), "user1", "test.local", "n1", new byte[16], 600, out errorCode);

                manager.TryCreateAllocation(
                    CreateFiveTuple(5001), "user1", "test.local", "n2", new byte[16], 600, out errorCode);

                //
                // Third should fail — no ports left
                //
                TurnAllocation third = manager.TryCreateAllocation(
                    CreateFiveTuple(5002), "user1", "test.local", "n3", new byte[16], 600, out errorCode);

                Assert.Null(third);
                Assert.Equal(StunErrorCode.INSUFFICIENT_CAPACITY, errorCode);
            }
        }


        [Fact]
        public void RemoveAllocation_FreesPort()
        {
            TurnServerConfiguration config = CreateConfig(60000, 60000);  // 1 port only

            using (AllocationManager manager = new AllocationManager(config))
            {
                FiveTuple ft = CreateFiveTuple(5000);
                int errorCode;

                TurnAllocation first = manager.TryCreateAllocation(
                    ft, "user1", "test.local", "n1", new byte[16], 600, out errorCode);

                Assert.NotNull(first);

                //
                // Remove it
                //
                manager.RemoveAllocation(ft);
                Assert.Equal(0, manager.AllocationCount);

                //
                // Port should be free for a new allocation
                //
                FiveTuple ft2 = CreateFiveTuple(5001);

                TurnAllocation second = manager.TryCreateAllocation(
                    ft2, "user1", "test.local", "n2", new byte[16], 600, out errorCode);

                Assert.NotNull(second);
            }
        }


        [Fact]
        public void CleanupExpired_RemovesExpiredAllocations()
        {
            TurnServerConfiguration config = CreateConfig();

            using (AllocationManager manager = new AllocationManager(config))
            {
                FiveTuple ft = CreateFiveTuple(5000);
                int errorCode;

                TurnAllocation allocation = manager.TryCreateAllocation(
                    ft, "user1", "test.local", "n1", new byte[16], 600, out errorCode);

                Assert.NotNull(allocation);

                //
                // Force the allocation to expire
                //
                allocation.ExpiresAtUtc = System.DateTime.UtcNow.AddMinutes(-1);

                int removed = manager.CleanupExpired();

                Assert.Equal(1, removed);
                Assert.Equal(0, manager.AllocationCount);
            }
        }


        [Fact]
        public void Lifetime_IsClamped_ToMaxLifetime()
        {
            TurnServerConfiguration config = CreateConfig();
            config.MaxLifetime = 1800;

            using (AllocationManager manager = new AllocationManager(config))
            {
                FiveTuple ft = CreateFiveTuple(5000);
                int errorCode;

                TurnAllocation allocation = manager.TryCreateAllocation(
                    ft, "user1", "test.local", "n1", new byte[16], 99999, out errorCode);

                Assert.NotNull(allocation);
                Assert.True(allocation.LifetimeSeconds <= 1800);
            }
        }
    }
}
