// ============================================================================
//
// FiveTupleTests.cs — Tests for 5-tuple equality and hash code.
//
// ============================================================================

using System.Net;
using Xunit;

using Foundation.Networking.Coturn.Server;

namespace Foundation.Networking.Coturn.Tests.Server
{
    public class FiveTupleTests
    {
        [Fact]
        public void Equal_FiveTuples_AreEqual()
        {
            FiveTuple a = new FiveTuple(
                new IPEndPoint(IPAddress.Parse("192.168.1.10"), 5000),
                new IPEndPoint(IPAddress.Parse("10.0.0.1"), 3478),
                17);

            FiveTuple b = new FiveTuple(
                new IPEndPoint(IPAddress.Parse("192.168.1.10"), 5000),
                new IPEndPoint(IPAddress.Parse("10.0.0.1"), 3478),
                17);

            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }


        [Fact]
        public void Different_ClientPort_NotEqual()
        {
            FiveTuple a = new FiveTuple(
                new IPEndPoint(IPAddress.Parse("192.168.1.10"), 5000),
                new IPEndPoint(IPAddress.Parse("10.0.0.1"), 3478),
                17);

            FiveTuple b = new FiveTuple(
                new IPEndPoint(IPAddress.Parse("192.168.1.10"), 5001),
                new IPEndPoint(IPAddress.Parse("10.0.0.1"), 3478),
                17);

            Assert.False(a.Equals(b));
            Assert.True(a != b);
        }


        [Fact]
        public void Different_Transport_NotEqual()
        {
            FiveTuple a = new FiveTuple(
                new IPEndPoint(IPAddress.Parse("192.168.1.10"), 5000),
                new IPEndPoint(IPAddress.Parse("10.0.0.1"), 3478),
                17);  // UDP

            FiveTuple b = new FiveTuple(
                new IPEndPoint(IPAddress.Parse("192.168.1.10"), 5000),
                new IPEndPoint(IPAddress.Parse("10.0.0.1"), 3478),
                6);   // TCP

            Assert.False(a.Equals(b));
        }


        [Fact]
        public void ToString_ContainsTransportAndAddresses()
        {
            FiveTuple ft = new FiveTuple(
                new IPEndPoint(IPAddress.Parse("192.168.1.10"), 5000),
                new IPEndPoint(IPAddress.Parse("10.0.0.1"), 3478),
                17);

            string str = ft.ToString();

            Assert.Contains("UDP", str);
            Assert.Contains("192.168.1.10", str);
            Assert.Contains("10.0.0.1", str);
        }
    }
}
