// ============================================================================
//
// CertificateInspectorTests.cs — Unit tests for CertificateInspector.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Threading;
using System.Threading.Tasks;
using Xunit;

using Foundation.Networking.Locksmith.Configuration;
using Foundation.Networking.Locksmith.Services;

namespace Foundation.Networking.Locksmith.Tests.Services
{
    public class CertificateInspectorTests
    {
        private CertificateInspector CreateInspector(LocksmithConfiguration config = null)
        {
            if (config == null)
            {
                config = new LocksmithConfiguration
                {
                    InspectTimeoutMs = 10000
                };
            }

            return new CertificateInspector(config);
        }


        // ── Live Inspection (requires network) ───────────────────────────


        [Fact]
        public async Task InspectAsync_GoogleCom_ReturnsValidCertificate()
        {
            CertificateInspector inspector = CreateInspector();

            CertificateInfo info = await inspector.InspectAsync("google.com", 443);

            Assert.True(info.Success);
            Assert.Equal("google.com", info.Host);
            Assert.Equal(443, info.Port);
            Assert.False(string.IsNullOrEmpty(info.Subject));
            Assert.False(string.IsNullOrEmpty(info.Issuer));
            Assert.False(string.IsNullOrEmpty(info.Thumbprint));
            Assert.False(string.IsNullOrEmpty(info.SerialNumber));
            Assert.True(info.DaysUntilExpiry > 0);
            Assert.False(info.IsExpired);
        }


        [Fact]
        public async Task InspectAsync_GoogleCom_HasValidDates()
        {
            CertificateInspector inspector = CreateInspector();

            CertificateInfo info = await inspector.InspectAsync("google.com", 443);

            Assert.True(info.Success);
            Assert.True(info.NotBeforeUtc.Year >= 2024);
            Assert.True(info.NotAfterUtc > info.NotBeforeUtc);
            Assert.True(info.NotAfterUtc > System.DateTime.UtcNow);
        }


        [Fact]
        public async Task InspectAsync_GoogleCom_ExtractsCommonName()
        {
            CertificateInspector inspector = CreateInspector();

            CertificateInfo info = await inspector.InspectAsync("google.com", 443);

            Assert.True(info.Success);
            Assert.False(string.IsNullOrEmpty(info.CommonName));
        }


        [Fact]
        public async Task InspectAsync_GoogleCom_HasSans()
        {
            CertificateInspector inspector = CreateInspector();

            CertificateInfo info = await inspector.InspectAsync("google.com", 443);

            Assert.True(info.Success);
            Assert.True(info.SubjectAlternativeNames.Count > 0);
        }


        [Fact]
        public async Task InspectAsync_GoogleCom_ReportsTlsVersion()
        {
            CertificateInspector inspector = CreateInspector();

            CertificateInfo info = await inspector.InspectAsync("google.com", 443);

            Assert.True(info.Success);
            Assert.False(string.IsNullOrEmpty(info.TlsVersion));
        }


        [Fact]
        public async Task InspectAsync_GoogleCom_HasChainInfo()
        {
            CertificateInspector inspector = CreateInspector();

            CertificateInfo info = await inspector.InspectAsync("google.com", 443);

            Assert.True(info.Success);
            Assert.True(info.ChainLength > 0);
        }


        [Fact]
        public async Task InspectAsync_GoogleCom_SetsTimestamp()
        {
            CertificateInspector inspector = CreateInspector();

            CertificateInfo info = await inspector.InspectAsync("google.com", 443);

            Assert.True(info.InspectedAtUtc.Year > 2020);
        }


        // ── Error Handling ───────────────────────────────────────────────


        [Fact]
        public async Task InspectAsync_InvalidHost_ReturnsFailure()
        {
            CertificateInspector inspector = CreateInspector(new LocksmithConfiguration
            {
                InspectTimeoutMs = 3000
            });

            CertificateInfo info = await inspector.InspectAsync("this.host.does.not.exist.invalid", 443);

            Assert.False(info.Success);
            Assert.False(string.IsNullOrEmpty(info.ErrorMessage));
        }


        [Fact]
        public async Task InspectAsync_ClosedPort_ReturnsFailure()
        {
            CertificateInspector inspector = CreateInspector(new LocksmithConfiguration
            {
                InspectTimeoutMs = 3000
            });

            //
            // Port 19999 is almost certainly not serving TLS
            //
            CertificateInfo info = await inspector.InspectAsync("127.0.0.1", 19999);

            Assert.False(info.Success);
            Assert.False(string.IsNullOrEmpty(info.ErrorMessage));
        }


        [Fact]
        public async Task InspectAsync_Timeout_ReturnsFailure()
        {
            CertificateInspector inspector = CreateInspector(new LocksmithConfiguration
            {
                InspectTimeoutMs = 100
            });

            //
            // Use an IP that should time out (non-routable)
            //
            CertificateInfo info = await inspector.InspectAsync("192.0.2.1", 443);

            Assert.False(info.Success);
        }


        // ── Result Properties ────────────────────────────────────────────


        [Fact]
        public async Task InspectAsync_SetsHostAndPort()
        {
            CertificateInspector inspector = CreateInspector(new LocksmithConfiguration
            {
                InspectTimeoutMs = 3000
            });

            CertificateInfo info = await inspector.InspectAsync("127.0.0.1", 19999);

            Assert.Equal("127.0.0.1", info.Host);
            Assert.Equal(19999, info.Port);
        }


        [Fact]
        public async Task InspectAsync_DefaultPort443_Works()
        {
            CertificateInspector inspector = CreateInspector();

            CertificateInfo info = await inspector.InspectAsync("google.com");

            Assert.True(info.Success);
            Assert.Equal(443, info.Port);
        }
    }
}
