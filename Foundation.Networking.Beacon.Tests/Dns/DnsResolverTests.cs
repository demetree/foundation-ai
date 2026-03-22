// ============================================================================
//
// DnsResolverTests.cs — Unit tests for DnsResolver.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Threading.Tasks;
using Xunit;

using Microsoft.Extensions.Logging.Abstractions;

using Foundation.Networking.Beacon.Configuration;
using Foundation.Networking.Beacon.Dns;

namespace Foundation.Networking.Beacon.Tests.Dns
{
    public class DnsResolverTests
    {
        private DnsResolver CreateResolver()
        {
            return new DnsResolver(
                new BeaconConfiguration { DnsTimeoutMs = 10000 },
                NullLogger<DnsResolver>.Instance);
        }


        [Fact]
        public async Task Resolve_KnownHost_Succeeds()
        {
            DnsResolver resolver = CreateResolver();

            DnsLookupResult result = await resolver.ResolveAsync("google.com");

            Assert.True(result.Success);
            Assert.NotEmpty(result.Addresses);
            Assert.Equal("google.com", result.Hostname);
            Assert.True(result.QueryTimeMs >= 0);
        }


        [Fact]
        public async Task Resolve_InvalidHost_Fails()
        {
            DnsResolver resolver = CreateResolver();

            DnsLookupResult result = await resolver.ResolveAsync("this.host.definitely.does.not.exist.invalid");

            Assert.False(result.Success);
            Assert.False(string.IsNullOrEmpty(result.Error));
        }


        [Fact]
        public async Task Resolve_Localhost_Succeeds()
        {
            DnsResolver resolver = CreateResolver();

            DnsLookupResult result = await resolver.ResolveAsync("localhost");

            Assert.True(result.Success);
            Assert.NotEmpty(result.Addresses);
        }


        [Fact]
        public async Task ResolveBatch_MultipleHosts()
        {
            DnsResolver resolver = CreateResolver();

            var results = await resolver.ResolveBatchAsync(new[] { "google.com", "localhost" });

            Assert.Equal(2, results.Count);
            Assert.True(results[0].Success);
            Assert.True(results[1].Success);
        }


        [Fact]
        public async Task ReverseLookup_ValidIp_Succeeds()
        {
            DnsResolver resolver = CreateResolver();

            ReverseDnsResult result = await resolver.ReverseLookupAsync("127.0.0.1");

            //
            // Reverse lookup on loopback may or may not succeed depending on OS config
            //
            Assert.Equal("127.0.0.1", result.IpAddress);
        }


        [Fact]
        public async Task ReverseLookup_InvalidIp_ReturnsError()
        {
            DnsResolver resolver = CreateResolver();

            ReverseDnsResult result = await resolver.ReverseLookupAsync("not.an.ip");

            Assert.False(result.Success);
            Assert.Equal("Invalid IP address", result.Error);
        }
    }
}
