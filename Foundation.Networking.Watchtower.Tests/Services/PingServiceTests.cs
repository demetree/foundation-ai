// ============================================================================
//
// PingServiceTests.cs — Unit tests for PingService.
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
    public class PingServiceTests
    {
        private PingService CreateService(WatchtowerConfiguration config = null)
        {
            if (config == null)
            {
                config = new WatchtowerConfiguration
                {
                    PingTimeoutMs = 3000,
                    PingTtl = 128,
                    PingBufferSize = 32
                };
            }

            return new PingService(config);
        }


        // ── Single Ping ──────────────────────────────────────────────────


        [Fact]
        public async Task PingAsync_Localhost_ReturnsSuccess()
        {
            PingService service = CreateService();

            PingResult result = await service.PingAsync("127.0.0.1");

            Assert.True(result.Success);
            Assert.Equal("127.0.0.1", result.Host);
            Assert.True(result.RoundTripTimeMs >= 0);
        }


        [Fact]
        public async Task PingAsync_InvalidHost_ReturnsFailure()
        {
            PingService service = CreateService();

            PingResult result = await service.PingAsync("999.999.999.999");

            Assert.False(result.Success);
            Assert.False(string.IsNullOrEmpty(result.ErrorMessage));
        }


        [Fact]
        public async Task PingAsync_SetsTimestamp()
        {
            PingService service = CreateService();

            PingResult result = await service.PingAsync("127.0.0.1");

            Assert.True(result.TimestampUtc.Year > 2020);
        }


        // ── Statistics ───────────────────────────────────────────────────


        [Fact]
        public async Task PingWithStatisticsAsync_Localhost_ReturnsAggregatedStats()
        {
            PingService service = CreateService();

            PingStatistics stats = await service.PingWithStatisticsAsync("127.0.0.1", 4);

            Assert.Equal(4, stats.Sent);
            Assert.Equal(4, stats.Received);
            Assert.Equal(0, stats.Lost);
            Assert.Equal(0, stats.LossPercent);
            Assert.True(stats.MinRttMs >= 0);
            Assert.True(stats.MaxRttMs >= stats.MinRttMs);
            Assert.True(stats.AvgRttMs >= 0);
            Assert.Equal(4, stats.Results.Count);
        }


        [Fact]
        public async Task PingWithStatisticsAsync_SinglePing_ReturnsZeroJitter()
        {
            PingService service = CreateService();

            PingStatistics stats = await service.PingWithStatisticsAsync("127.0.0.1", 1);

            Assert.Equal(1, stats.Sent);
            Assert.Equal(0, stats.JitterMs);
        }


        [Fact]
        public async Task PingWithStatisticsAsync_InvalidHost_ReportsLoss()
        {
            PingService service = CreateService(new WatchtowerConfiguration { PingTimeoutMs = 500 });

            PingStatistics stats = await service.PingWithStatisticsAsync("999.999.999.999", 2);

            Assert.Equal(2, stats.Sent);
            Assert.Equal(2, stats.Lost);
            Assert.Equal(100, stats.LossPercent);
        }


        [Fact]
        public async Task PingWithStatisticsAsync_CancellationToken_StopsEarly()
        {
            PingService service = CreateService();
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            PingStatistics stats = await service.PingWithStatisticsAsync("127.0.0.1", 10, cts.Token);

            Assert.True(stats.Results.Count < 10);
        }


        // ── Custom Settings ──────────────────────────────────────────────


        [Fact]
        public async Task PingAsync_CustomTimeout_RespectsValue()
        {
            PingService service = CreateService();

            PingResult result = await service.PingAsync("127.0.0.1", 1000, 64, CancellationToken.None);

            Assert.True(result.Success);
        }


        [Fact]
        public async Task PingAsync_CustomBufferSize_Works()
        {
            WatchtowerConfiguration config = new WatchtowerConfiguration
            {
                PingBufferSize = 64
            };

            PingService service = CreateService(config);

            PingResult result = await service.PingAsync("127.0.0.1");

            Assert.True(result.Success);
        }
    }
}
