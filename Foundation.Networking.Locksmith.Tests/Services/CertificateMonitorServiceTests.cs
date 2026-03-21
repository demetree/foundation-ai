// ============================================================================
//
// CertificateMonitorServiceTests.cs — Unit tests for CertificateMonitorService.
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

using Foundation.Networking.Locksmith.Configuration;
using Foundation.Networking.Locksmith.Services;

namespace Foundation.Networking.Locksmith.Tests.Services
{
    public class CertificateMonitorServiceTests
    {
        private LocksmithConfiguration CreateConfig(bool enabled = true)
        {
            return new LocksmithConfiguration
            {
                InspectTimeoutMs = 10000,
                Monitor = new CertificateMonitorConfiguration
                {
                    Enabled = enabled,
                    IntervalHours = 6,
                    WarningDays = 30,
                    CriticalDays = 7,
                    Endpoints = new List<CertificateEndpoint>
                    {
                        new CertificateEndpoint
                        {
                            Host = "google.com",
                            Port = 443,
                            Label = "Google"
                        }
                    }
                }
            };
        }


        private CertificateMonitorService CreateService(LocksmithConfiguration config = null)
        {
            if (config == null)
            {
                config = CreateConfig();
            }

            CertificateInspector inspector = new CertificateInspector(config);
            ILogger<CertificateMonitorService> logger = NullLogger<CertificateMonitorService>.Instance;

            return new CertificateMonitorService(config, inspector, logger);
        }


        // ── Lifecycle ────────────────────────────────────────────────────


        [Fact]
        public async Task StartAsync_WhenEnabled_SetsIsRunning()
        {
            CertificateMonitorService service = CreateService();

            await service.StartAsync(CancellationToken.None);

            Assert.True(service.IsRunning);

            await service.StopAsync(CancellationToken.None);
            service.Dispose();
        }


        [Fact]
        public async Task StartAsync_WhenDisabled_DoesNotStart()
        {
            LocksmithConfiguration config = CreateConfig(enabled: false);
            CertificateMonitorService service = CreateService(config);

            await service.StartAsync(CancellationToken.None);

            Assert.False(service.IsRunning);

            service.Dispose();
        }


        [Fact]
        public async Task StopAsync_SetsIsRunningToFalse()
        {
            CertificateMonitorService service = CreateService();

            await service.StartAsync(CancellationToken.None);

            Assert.True(service.IsRunning);

            await service.StopAsync(CancellationToken.None);

            Assert.False(service.IsRunning);

            service.Dispose();
        }


        [Fact]
        public async Task StartAsync_NoEndpoints_DoesNotStart()
        {
            LocksmithConfiguration config = new LocksmithConfiguration
            {
                Monitor = new CertificateMonitorConfiguration
                {
                    Enabled = true,
                    Endpoints = new List<CertificateEndpoint>()
                }
            };

            CertificateMonitorService service = CreateService(config);

            await service.StartAsync(CancellationToken.None);

            Assert.False(service.IsRunning);

            service.Dispose();
        }


        // ── Statuses ─────────────────────────────────────────────────────


        [Fact]
        public void GetAllStatuses_BeforeCheck_ReturnsUnknown()
        {
            CertificateMonitorService service = CreateService();

            List<CertificateEndpointStatus> statuses = service.GetAllStatuses();

            Assert.Single(statuses);
            Assert.Equal("Google", statuses[0].Label);
            Assert.Equal(CertificateHealthStatus.Unknown, statuses[0].Status);

            service.Dispose();
        }


        [Fact]
        public void GetStatus_UnknownEndpoint_ReturnsUnknown()
        {
            CertificateMonitorService service = CreateService();

            CertificateEndpointStatus status = service.GetStatus("unknown.host", 443);

            Assert.Equal(CertificateHealthStatus.Unknown, status.Status);

            service.Dispose();
        }


        [Fact]
        public void GetAlerts_BeforeCheck_ReturnsEmpty()
        {
            CertificateMonitorService service = CreateService();

            List<CertificateEndpointStatus> alerts = service.GetAlerts();

            Assert.Empty(alerts);

            service.Dispose();
        }


        // ── Force Check ──────────────────────────────────────────────────


        [Fact]
        public async Task ForceCheckAsync_GoogleCom_SetsHealthyStatus()
        {
            CertificateMonitorService service = CreateService();

            await service.ForceCheckAsync();

            List<CertificateEndpointStatus> statuses = service.GetAllStatuses();

            Assert.Single(statuses);
            Assert.Equal("Google", statuses[0].Label);

            //
            // Google's cert should be healthy (not warning/critical/expired)
            //
            Assert.Equal(CertificateHealthStatus.Healthy, statuses[0].Status);
            Assert.True(statuses[0].DaysUntilExpiry > 0);
            Assert.False(string.IsNullOrEmpty(statuses[0].Subject));

            service.Dispose();
        }


        [Fact]
        public async Task ForceCheckAsync_UnreachableEndpoint_SetsUnreachable()
        {
            LocksmithConfiguration config = new LocksmithConfiguration
            {
                InspectTimeoutMs = 3000,
                Monitor = new CertificateMonitorConfiguration
                {
                    Enabled = true,
                    Endpoints = new List<CertificateEndpoint>
                    {
                        new CertificateEndpoint
                        {
                            Host = "127.0.0.1",
                            Port = 19999,
                            Label = "Unreachable"
                        }
                    }
                }
            };

            CertificateMonitorService service = CreateService(config);

            await service.ForceCheckAsync();

            CertificateEndpointStatus status = service.GetStatus("127.0.0.1", 19999);

            Assert.Equal(CertificateHealthStatus.Unreachable, status.Status);
            Assert.False(string.IsNullOrEmpty(status.ErrorMessage));

            service.Dispose();
        }


        // ── Dispose ──────────────────────────────────────────────────────


        [Fact]
        public async Task Dispose_AfterStart_CleansUp()
        {
            CertificateMonitorService service = CreateService();

            await service.StartAsync(CancellationToken.None);

            service.Dispose();

            // Should not throw
        }


        [Fact]
        public void Dispose_BeforeStart_DoesNotThrow()
        {
            CertificateMonitorService service = CreateService();

            service.Dispose();

            // Should not throw
        }
    }
}
