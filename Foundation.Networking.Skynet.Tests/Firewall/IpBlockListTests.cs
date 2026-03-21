// ============================================================================
//
// IpBlockListTests.cs — Unit tests for IpBlockList.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Skynet.Firewall;

namespace Foundation.Networking.Skynet.Tests.Firewall
{
    public class IpBlockListTests
    {
        // ── Single IP ────────────────────────────────────────────────────


        [Fact]
        public void Contains_ExactIpMatch_ReturnsTrue()
        {
            IpBlockList list = new IpBlockList(new List<string> { "192.168.1.1" });

            Assert.True(list.Contains("192.168.1.1"));
        }


        [Fact]
        public void Contains_DifferentIp_ReturnsFalse()
        {
            IpBlockList list = new IpBlockList(new List<string> { "192.168.1.1" });

            Assert.False(list.Contains("192.168.1.2"));
        }


        [Fact]
        public void Contains_MultipleIps_MatchesAll()
        {
            IpBlockList list = new IpBlockList(new List<string> { "10.0.0.1", "10.0.0.2", "10.0.0.3" });

            Assert.True(list.Contains("10.0.0.1"));
            Assert.True(list.Contains("10.0.0.2"));
            Assert.True(list.Contains("10.0.0.3"));
            Assert.False(list.Contains("10.0.0.4"));
        }


        // ── CIDR Ranges ──────────────────────────────────────────────────


        [Fact]
        public void Contains_Cidr24_MatchesSubnet()
        {
            IpBlockList list = new IpBlockList(new List<string> { "192.168.1.0/24" });

            Assert.True(list.Contains("192.168.1.0"));
            Assert.True(list.Contains("192.168.1.1"));
            Assert.True(list.Contains("192.168.1.128"));
            Assert.True(list.Contains("192.168.1.255"));
            Assert.False(list.Contains("192.168.2.1"));
        }


        [Fact]
        public void Contains_Cidr16_MatchesLargeSubnet()
        {
            IpBlockList list = new IpBlockList(new List<string> { "10.0.0.0/16" });

            Assert.True(list.Contains("10.0.0.1"));
            Assert.True(list.Contains("10.0.255.255"));
            Assert.False(list.Contains("10.1.0.1"));
        }


        [Fact]
        public void Contains_Cidr8_MatchesClassA()
        {
            IpBlockList list = new IpBlockList(new List<string> { "10.0.0.0/8" });

            Assert.True(list.Contains("10.0.0.1"));
            Assert.True(list.Contains("10.255.255.255"));
            Assert.False(list.Contains("11.0.0.1"));
        }


        [Fact]
        public void Contains_Cidr32_MatchesSingleIp()
        {
            IpBlockList list = new IpBlockList(new List<string> { "192.168.1.100/32" });

            Assert.True(list.Contains("192.168.1.100"));
            Assert.False(list.Contains("192.168.1.101"));
        }


        // ── Edge Cases ───────────────────────────────────────────────────


        [Fact]
        public void Contains_EmptyList_ReturnsFalse()
        {
            IpBlockList list = new IpBlockList();

            Assert.False(list.Contains("192.168.1.1"));
        }


        [Fact]
        public void Contains_NullIp_ReturnsFalse()
        {
            IpBlockList list = new IpBlockList(new List<string> { "10.0.0.0/8" });

            Assert.False(list.Contains((string)null));
        }


        [Fact]
        public void Contains_EmptyIp_ReturnsFalse()
        {
            IpBlockList list = new IpBlockList(new List<string> { "10.0.0.0/8" });

            Assert.False(list.Contains(""));
        }


        [Fact]
        public void Contains_InvalidIp_ReturnsFalse()
        {
            IpBlockList list = new IpBlockList(new List<string> { "10.0.0.0/8" });

            Assert.False(list.Contains("not-an-ip"));
        }


        [Fact]
        public void AddRange_InvalidFormat_IgnoredGracefully()
        {
            IpBlockList list = new IpBlockList();
            list.AddRange("invalid");
            list.AddRange(null);
            list.AddRange("");

            Assert.Equal(0, list.Count);
        }


        [Fact]
        public void Count_ReflectsNumberOfRanges()
        {
            IpBlockList list = new IpBlockList(new List<string> { "10.0.0.1", "10.0.0.0/24" });

            Assert.Equal(2, list.Count);
        }


        [Fact]
        public void Clear_RemovesAllRanges()
        {
            IpBlockList list = new IpBlockList(new List<string> { "10.0.0.1", "10.0.0.2" });

            list.Clear();

            Assert.Equal(0, list.Count);
            Assert.False(list.Contains("10.0.0.1"));
        }


        // ── Loopback / Private ───────────────────────────────────────────


        [Fact]
        public void Contains_Loopback_Works()
        {
            IpBlockList list = new IpBlockList(new List<string> { "127.0.0.0/8" });

            Assert.True(list.Contains("127.0.0.1"));
            Assert.True(list.Contains("127.255.255.255"));
        }
    }
}
