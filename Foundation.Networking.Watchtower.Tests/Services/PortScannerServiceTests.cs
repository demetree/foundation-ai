// ============================================================================
//
// PortScannerServiceTests.cs — Unit tests for PortScannerService.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using Foundation.Networking.Watchtower.Configuration;
using Foundation.Networking.Watchtower.Services;

namespace Foundation.Networking.Watchtower.Tests.Services
{
    public class PortScannerServiceTests
    {
        private PortScannerService CreateService(WatchtowerConfiguration config = null)
        {
            if (config == null)
            {
                config = new WatchtowerConfiguration
                {
                    PortScanTimeoutMs = 2000,
                    PortScanMaxConcurrency = 50
                };
            }

            return new PortScannerService(config);
        }


        // ── Specific Ports ───────────────────────────────────────────────


        [Fact]
        public async Task ScanPortsAsync_ClosedPort_ReportsNotOpen()
        {
            PortScannerService service = CreateService(new WatchtowerConfiguration
            {
                PortScanTimeoutMs = 500,
                PortScanMaxConcurrency = 10
            });

            //
            // Port 19999 is almost certainly not in use
            //
            List<int> ports = new List<int> { 19999 };
            PortScanReport report = await service.ScanPortsAsync("127.0.0.1", ports);

            Assert.Equal(1, report.Results.Count);
            Assert.False(report.Results[0].IsOpen);
            Assert.Equal(0, report.OpenPortCount);
            Assert.Equal(1, report.ClosedPortCount);
        }


        [Fact]
        public async Task ScanPortsAsync_SetsReportMetadata()
        {
            PortScannerService service = CreateService(new WatchtowerConfiguration
            {
                PortScanTimeoutMs = 500,
                PortScanMaxConcurrency = 10
            });

            List<int> ports = new List<int> { 19999 };
            PortScanReport report = await service.ScanPortsAsync("127.0.0.1", ports);

            Assert.Equal("127.0.0.1", report.Host);
            Assert.True(report.DurationMs >= 0);
            Assert.True(report.TimestampUtc.Year > 2020);
        }


        [Fact]
        public async Task ScanPortsAsync_MultiplePorts_ReturnsSortedByPort()
        {
            PortScannerService service = CreateService(new WatchtowerConfiguration
            {
                PortScanTimeoutMs = 500,
                PortScanMaxConcurrency = 10
            });

            List<int> ports = new List<int> { 19003, 19001, 19002 };
            PortScanReport report = await service.ScanPortsAsync("127.0.0.1", ports);

            Assert.Equal(3, report.Results.Count);
            Assert.Equal(19001, report.Results[0].Port);
            Assert.Equal(19002, report.Results[1].Port);
            Assert.Equal(19003, report.Results[2].Port);
        }


        [Fact]
        public async Task ScanPortsAsync_SetsHostOnEachResult()
        {
            PortScannerService service = CreateService(new WatchtowerConfiguration
            {
                PortScanTimeoutMs = 500,
                PortScanMaxConcurrency = 10
            });

            List<int> ports = new List<int> { 19998, 19999 };
            PortScanReport report = await service.ScanPortsAsync("127.0.0.1", ports);

            foreach (PortScanResult result in report.Results)
            {
                Assert.Equal("127.0.0.1", result.Host);
            }
        }


        // ── Port Range ───────────────────────────────────────────────────


        [Fact]
        public async Task ScanRangeAsync_SmallRange_ScansAllPorts()
        {
            PortScannerService service = CreateService(new WatchtowerConfiguration
            {
                PortScanTimeoutMs = 500,
                PortScanMaxConcurrency = 10
            });

            PortScanReport report = await service.ScanRangeAsync("127.0.0.1", 19990, 19995);

            Assert.Equal(6, report.Results.Count);
        }


        // ── Common Ports ─────────────────────────────────────────────────


        [Fact]
        public async Task ScanCommonPortsAsync_ReturnsResults()
        {
            PortScannerService service = CreateService(new WatchtowerConfiguration
            {
                PortScanTimeoutMs = 200,
                PortScanMaxConcurrency = 20
            });

            PortScanReport report = await service.ScanCommonPortsAsync("127.0.0.1");

            Assert.True(report.Results.Count > 0);
            Assert.True(report.OpenPortCount + report.ClosedPortCount == report.Results.Count);
        }


        // ── Service Identification ───────────────────────────────────────


        [Fact]
        public async Task ScanPortsAsync_WellKnownPort_IdentifiesService()
        {
            PortScannerService service = CreateService(new WatchtowerConfiguration
            {
                PortScanTimeoutMs = 500,
                PortScanMaxConcurrency = 10
            });

            //
            // Port 80 may or may not be open, but if it IS open,
            // it should be identified as HTTP
            //
            List<int> ports = new List<int> { 80 };
            PortScanReport report = await service.ScanPortsAsync("127.0.0.1", ports);

            if (report.Results[0].IsOpen == true)
            {
                Assert.Equal("HTTP", report.Results[0].ServiceName);
            }
        }


        // ── Cancellation ─────────────────────────────────────────────────


        [Fact]
        public async Task ScanPortsAsync_CancellationToken_StopsEarly()
        {
            PortScannerService service = CreateService(new WatchtowerConfiguration
            {
                PortScanTimeoutMs = 5000,
                PortScanMaxConcurrency = 2
            });

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            List<int> ports = new List<int>();
            for (int i = 19000; i < 19100; i++)
            {
                ports.Add(i);
            }

            PortScanReport report = await service.ScanPortsAsync("127.0.0.1", ports, cts.Token);

            Assert.True(report.Results.Count < 100);
        }
    }
}
