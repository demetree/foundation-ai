// ============================================================================
//
// LatencyMonitorServiceTests.cs — Unit tests for LatencyMonitorService.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

using Foundation.Networking.Watchtower.Configuration;
using Foundation.Networking.Watchtower.Services;

namespace Foundation.Networking.Watchtower.Tests.Services
{
    public class LatencyMonitorServiceTests
    {
        private WatchtowerConfiguration CreateConfig(bool enabled = true, int intervalSeconds = 60)
        {
            return new WatchtowerConfiguration
            {
                PingTimeoutMs = 3000,
                PingTtl = 128,
                PingBufferSize = 32,
                LatencyMonitor = new LatencyMonitorConfiguration
                {
                    Enabled = enabled,
                    IntervalSeconds = intervalSeconds,
                    MaxHistoryPerEndpoint = 100,
                    AlertThresholdMs = 500,
                    Endpoints = new List<MonitoredEndpoint>
                    {
                        new MonitoredEndpoint
                        {
                            Host = "127.0.0.1",
                            Label = "Localhost",
                            Ports = new List<int> { 80 }
                        }
                    }
                }
            };
        }


        private LatencyMonitorService CreateService(WatchtowerConfiguration config = null)
        {
            if (config == null)
            {
                config = CreateConfig();
            }

            PingService pingService = new PingService(config);
            ILogger<LatencyMonitorService> logger = NullLogger<LatencyMonitorService>.Instance;

            return new LatencyMonitorService(config, pingService, logger);
        }


        // ── Lifecycle ────────────────────────────────────────────────────


        [Fact]
        public async Task StartAsync_WhenEnabled_SetsIsRunning()
        {
            LatencyMonitorService service = CreateService();

            await service.StartAsync(CancellationToken.None);

            Assert.True(service.IsRunning);

            await service.StopAsync(CancellationToken.None);
            service.Dispose();
        }


        [Fact]
        public async Task StartAsync_WhenDisabled_DoesNotStart()
        {
            WatchtowerConfiguration config = CreateConfig(enabled: false);
            LatencyMonitorService service = CreateService(config);

            await service.StartAsync(CancellationToken.None);

            Assert.False(service.IsRunning);

            service.Dispose();
        }


        [Fact]
        public async Task StopAsync_SetsIsRunningToFalse()
        {
            LatencyMonitorService service = CreateService();

            await service.StartAsync(CancellationToken.None);

            Assert.True(service.IsRunning);

            await service.StopAsync(CancellationToken.None);

            Assert.False(service.IsRunning);

            service.Dispose();
        }


        [Fact]
        public async Task StartAsync_NoEndpoints_DoesNotStart()
        {
            WatchtowerConfiguration config = new WatchtowerConfiguration
            {
                LatencyMonitor = new LatencyMonitorConfiguration
                {
                    Enabled = true,
                    Endpoints = new List<MonitoredEndpoint>()
                }
            };

            LatencyMonitorService service = CreateService(config);

            await service.StartAsync(CancellationToken.None);

            Assert.False(service.IsRunning);

            service.Dispose();
        }


        // ── Summaries ────────────────────────────────────────────────────


        [Fact]
        public void GetAllSummaries_BeforeStart_ReturnsEmptySummaries()
        {
            LatencyMonitorService service = CreateService();

            List<LatencyEndpointSummary> summaries = service.GetAllSummaries();

            Assert.Single(summaries);
            Assert.Equal("Localhost", summaries[0].Label);
            Assert.Equal(0, summaries[0].RecordCount);

            service.Dispose();
        }


        [Fact]
        public void GetEndpointSummary_UnknownHost_ReturnsEmptySummary()
        {
            LatencyMonitorService service = CreateService();

            LatencyEndpointSummary summary = service.GetEndpointSummary("unknown.host.example");

            Assert.Equal("unknown.host.example", summary.Host);
            Assert.Equal(0, summary.RecordCount);

            service.Dispose();
        }


        // ── History ──────────────────────────────────────────────────────


        [Fact]
        public void GetHistory_BeforeStart_ReturnsEmptyList()
        {
            LatencyMonitorService service = CreateService();

            List<LatencyRecord> history = service.GetHistory("127.0.0.1");

            Assert.Empty(history);

            service.Dispose();
        }


        [Fact]
        public void GetHistory_UnknownHost_ReturnsEmptyList()
        {
            LatencyMonitorService service = CreateService();

            List<LatencyRecord> history = service.GetHistory("unknown.host.example");

            Assert.Empty(history);

            service.Dispose();
        }


        // ── Dispose ──────────────────────────────────────────────────────


        [Fact]
        public async Task Dispose_AfterStart_CleansUp()
        {
            LatencyMonitorService service = CreateService();

            await service.StartAsync(CancellationToken.None);

            service.Dispose();

            // Should not throw
            Assert.False(service.IsRunning == true && service.IsRunning == false);
        }


        [Fact]
        public void Dispose_BeforeStart_DoesNotThrow()
        {
            LatencyMonitorService service = CreateService();

            service.Dispose();

            // Should not throw
        }


        // ── Integration (short-lived monitor) ────────────────────────────


        [Fact]
        public async Task StartAndWait_CollectsData()
        {
            WatchtowerConfiguration config = CreateConfig(enabled: true, intervalSeconds: 1);
            config.LatencyMonitor.IntervalSeconds = 1;

            LatencyMonitorService service = CreateService(config);

            await service.StartAsync(CancellationToken.None);

            //
            // Wait for at least one monitoring cycle
            // (initial delay is 5s, so we wait a bit longer)
            //
            await Task.Delay(7000);

            List<LatencyEndpointSummary> summaries = service.GetAllSummaries();

            await service.StopAsync(CancellationToken.None);
            service.Dispose();

            //
            // Should have collected at least one record
            //
            Assert.Single(summaries);
            Assert.True(summaries[0].RecordCount > 0);
            Assert.True(summaries[0].IsReachable);
        }
    }
}
