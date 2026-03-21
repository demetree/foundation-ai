// ============================================================================
//
// RateLimiterTests.cs — Unit tests for RateLimiter.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Threading.Tasks;
using Xunit;

using Foundation.Networking.Skynet.Firewall;

namespace Foundation.Networking.Skynet.Tests.Firewall
{
    public class RateLimiterTests
    {
        // ── Basic Rate Limiting ──────────────────────────────────────────


        [Fact]
        public void IsAllowed_WithinLimit_ReturnsTrue()
        {
            RateLimiter limiter = new RateLimiter(5, 60);

            Assert.True(limiter.IsAllowed("192.168.1.1"));
            Assert.True(limiter.IsAllowed("192.168.1.1"));
            Assert.True(limiter.IsAllowed("192.168.1.1"));
        }


        [Fact]
        public void IsAllowed_ExceedsLimit_ReturnsFalse()
        {
            RateLimiter limiter = new RateLimiter(3, 60);

            Assert.True(limiter.IsAllowed("192.168.1.1"));
            Assert.True(limiter.IsAllowed("192.168.1.1"));
            Assert.True(limiter.IsAllowed("192.168.1.1"));
            Assert.False(limiter.IsAllowed("192.168.1.1"));
        }


        [Fact]
        public void IsAllowed_DifferentIps_IndependentLimits()
        {
            RateLimiter limiter = new RateLimiter(2, 60);

            Assert.True(limiter.IsAllowed("10.0.0.1"));
            Assert.True(limiter.IsAllowed("10.0.0.1"));
            Assert.False(limiter.IsAllowed("10.0.0.1"));

            //
            // Different IP should still be allowed
            //
            Assert.True(limiter.IsAllowed("10.0.0.2"));
            Assert.True(limiter.IsAllowed("10.0.0.2"));
            Assert.False(limiter.IsAllowed("10.0.0.2"));
        }


        // ── Query Methods ────────────────────────────────────────────────


        [Fact]
        public void GetRequestCount_ReturnsCurrentCount()
        {
            RateLimiter limiter = new RateLimiter(10, 60);

            limiter.IsAllowed("192.168.1.1");
            limiter.IsAllowed("192.168.1.1");
            limiter.IsAllowed("192.168.1.1");

            Assert.Equal(3, limiter.GetRequestCount("192.168.1.1"));
        }


        [Fact]
        public void GetRequestCount_UnknownIp_ReturnsZero()
        {
            RateLimiter limiter = new RateLimiter(10, 60);

            Assert.Equal(0, limiter.GetRequestCount("192.168.1.1"));
        }


        [Fact]
        public void GetRemainingRequests_ReturnsCorrectValue()
        {
            RateLimiter limiter = new RateLimiter(5, 60);

            limiter.IsAllowed("192.168.1.1");
            limiter.IsAllowed("192.168.1.1");

            Assert.Equal(3, limiter.GetRemainingRequests("192.168.1.1"));
        }


        [Fact]
        public void GetRemainingRequests_AtLimit_ReturnsZero()
        {
            RateLimiter limiter = new RateLimiter(2, 60);

            limiter.IsAllowed("192.168.1.1");
            limiter.IsAllowed("192.168.1.1");

            Assert.Equal(0, limiter.GetRemainingRequests("192.168.1.1"));
        }


        // ── Reset ────────────────────────────────────────────────────────


        [Fact]
        public void Reset_ClearsCountForIp()
        {
            RateLimiter limiter = new RateLimiter(2, 60);

            limiter.IsAllowed("192.168.1.1");
            limiter.IsAllowed("192.168.1.1");
            Assert.False(limiter.IsAllowed("192.168.1.1"));

            limiter.Reset("192.168.1.1");

            Assert.True(limiter.IsAllowed("192.168.1.1"));
        }


        [Fact]
        public void ResetAll_ClearsAllCounters()
        {
            RateLimiter limiter = new RateLimiter(2, 60);

            limiter.IsAllowed("10.0.0.1");
            limiter.IsAllowed("10.0.0.2");

            limiter.ResetAll();

            Assert.Equal(0, limiter.TrackedIpCount);
        }


        // ── Edge Cases ───────────────────────────────────────────────────


        [Fact]
        public void IsAllowed_NullIp_ReturnsTrue()
        {
            RateLimiter limiter = new RateLimiter(2, 60);

            Assert.True(limiter.IsAllowed(null));
        }


        [Fact]
        public void IsAllowed_EmptyIp_ReturnsTrue()
        {
            RateLimiter limiter = new RateLimiter(2, 60);

            Assert.True(limiter.IsAllowed(""));
        }


        [Fact]
        public void TrackedIpCount_ReflectsUniqueIps()
        {
            RateLimiter limiter = new RateLimiter(10, 60);

            limiter.IsAllowed("10.0.0.1");
            limiter.IsAllowed("10.0.0.2");
            limiter.IsAllowed("10.0.0.3");

            Assert.Equal(3, limiter.TrackedIpCount);
        }
    }
}
