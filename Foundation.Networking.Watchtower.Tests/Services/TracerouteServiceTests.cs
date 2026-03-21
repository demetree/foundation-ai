// ============================================================================
//
// TracerouteServiceTests.cs — Unit tests for TracerouteService.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Threading;
using System.Threading.Tasks;
using Xunit;

using Foundation.Networking.Watchtower.Configuration;
using Foundation.Networking.Watchtower.Services;

namespace Foundation.Networking.Watchtower.Tests.Services
{
    public class TracerouteServiceTests
    {
        private TracerouteService CreateService(WatchtowerConfiguration config = null)
        {
            if (config == null)
            {
                config = new WatchtowerConfiguration
                {
                    TracerouteMaxHops = 30,
                    TracerouteTimeoutMs = 3000,
                    PingBufferSize = 32
                };
            }

            return new TracerouteService(config);
        }


        // ── Basic Trace ──────────────────────────────────────────────────


        [Fact]
        public async Task TraceAsync_Localhost_ReachesDestination()
        {
            TracerouteService service = CreateService();

            TracerouteResult result = await service.TraceAsync("127.0.0.1");

            Assert.True(result.ReachedDestination);
            Assert.Equal("127.0.0.1", result.Host);
            Assert.True(result.Hops.Count > 0);
            Assert.True(result.Hops[result.Hops.Count - 1].IsDestination);
        }


        [Fact]
        public async Task TraceAsync_InvalidHost_DoesNotReachDestination()
        {
            TracerouteService service = CreateService(new WatchtowerConfiguration
            {
                TracerouteMaxHops = 3,
                TracerouteTimeoutMs = 500,
                PingBufferSize = 32
            });

            TracerouteResult result = await service.TraceAsync("999.999.999.999");

            Assert.False(result.ReachedDestination);
        }


        [Fact]
        public async Task TraceAsync_SetsTimestamp()
        {
            TracerouteService service = CreateService();

            TracerouteResult result = await service.TraceAsync("127.0.0.1");

            Assert.True(result.TimestampUtc.Year > 2020);
        }


        [Fact]
        public async Task TraceAsync_SetsTotalHops()
        {
            TracerouteService service = CreateService();

            TracerouteResult result = await service.TraceAsync("127.0.0.1");

            Assert.Equal(result.Hops.Count, result.TotalHops);
        }


        // ── Custom Settings ──────────────────────────────────────────────


        [Fact]
        public async Task TraceAsync_MaxHops_LimitsHopCount()
        {
            TracerouteService service = CreateService();

            TracerouteResult result = await service.TraceAsync("127.0.0.1", 2, 3000, CancellationToken.None);

            Assert.True(result.Hops.Count <= 2);
        }


        [Fact]
        public async Task TraceAsync_CancellationToken_StopsEarly()
        {
            TracerouteService service = CreateService();
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            TracerouteResult result = await service.TraceAsync("127.0.0.1", 30, 3000, cts.Token);

            Assert.True(result.Hops.Count < 30);
        }


        // ── Hop Properties ───────────────────────────────────────────────


        [Fact]
        public async Task TraceAsync_Localhost_FirstHopHasCorrectNumber()
        {
            TracerouteService service = CreateService();

            TracerouteResult result = await service.TraceAsync("127.0.0.1");

            Assert.True(result.Hops.Count > 0);
            Assert.Equal(1, result.Hops[0].HopNumber);
        }


        [Fact]
        public async Task TraceAsync_Localhost_DestinationHopResponded()
        {
            TracerouteService service = CreateService();

            TracerouteResult result = await service.TraceAsync("127.0.0.1");

            if (result.ReachedDestination == true)
            {
                TraceHop lastHop = result.Hops[result.Hops.Count - 1];
                Assert.True(lastHop.Responded);
                Assert.True(lastHop.IsDestination);
            }
        }
    }
}
