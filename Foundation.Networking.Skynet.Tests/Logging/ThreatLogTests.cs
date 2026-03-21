// ============================================================================
//
// ThreatLogTests.cs — Unit tests for ThreatLog.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using System.Linq;
using Xunit;

using Foundation.Networking.Skynet.Logging;

namespace Foundation.Networking.Skynet.Tests.Logging
{
    public class ThreatLogTests
    {
        // ── Recording ────────────────────────────────────────────────────


        [Fact]
        public void Record_AddsEntry()
        {
            ThreatLog log = new ThreatLog(100);

            log.Record("10.0.0.1", "/malicious", "CN", "Blocked", "Rule1", "Bad IP");

            Assert.Equal(1, log.Count);
        }


        [Fact]
        public void Record_MaxEntries_TrimsOldest()
        {
            ThreatLog log = new ThreatLog(5);

            for (int i = 0; i < 10; i++)
            {
                log.Record("10.0.0." + i, "/path", "", "Blocked", "Rule1", "Test");
            }

            Assert.Equal(5, log.Count);
        }


        // ── GetRecent ────────────────────────────────────────────────────


        [Fact]
        public void GetRecent_ReturnsNewestFirst()
        {
            ThreatLog log = new ThreatLog(100);

            log.Record("10.0.0.1", "/first", "", "Blocked", "Rule1", "First");
            log.Record("10.0.0.2", "/second", "", "Blocked", "Rule1", "Second");
            log.Record("10.0.0.3", "/third", "", "Blocked", "Rule1", "Third");

            List<ThreatLogEntry> recent = log.GetRecent(10);

            Assert.Equal(3, recent.Count);
            Assert.Equal("10.0.0.3", recent[0].IpAddress);
            Assert.Equal("10.0.0.1", recent[2].IpAddress);
        }


        [Fact]
        public void GetRecent_LimitsCount()
        {
            ThreatLog log = new ThreatLog(100);

            for (int i = 0; i < 50; i++)
            {
                log.Record("10.0.0.1", "/path", "", "Blocked", "Rule1", "Test");
            }

            List<ThreatLogEntry> recent = log.GetRecent(10);

            Assert.Equal(10, recent.Count);
        }


        // ── Summary ──────────────────────────────────────────────────────


        [Fact]
        public void GetSummary_CorrectCounts()
        {
            ThreatLog log = new ThreatLog(100);

            log.Record("10.0.0.1", "/a", "", "Blocked", "Rule1", "Test");
            log.Record("10.0.0.1", "/b", "", "Blocked", "Rule1", "Test");
            log.Record("10.0.0.2", "/a", "", "RateLimited", "RateLimit", "Too fast");

            ThreatLogSummary summary = log.GetSummary();

            Assert.Equal(3, summary.TotalEntries);
            Assert.Equal(2, summary.BlockedCount);
            Assert.Equal(1, summary.RateLimitedCount);
        }


        [Fact]
        public void GetSummary_TopOffenders_Sorted()
        {
            ThreatLog log = new ThreatLog(100);

            log.Record("10.0.0.1", "/a", "", "Blocked", "Rule1", "Test");
            log.Record("10.0.0.1", "/b", "", "Blocked", "Rule1", "Test");
            log.Record("10.0.0.1", "/c", "", "Blocked", "Rule1", "Test");
            log.Record("10.0.0.2", "/a", "", "Blocked", "Rule1", "Test");

            ThreatLogSummary summary = log.GetSummary();

            Assert.True(summary.TopOffenders.Count > 0);
            Assert.Equal("10.0.0.1", summary.TopOffenders[0].IpAddress);
            Assert.Equal(3, summary.TopOffenders[0].Count);
        }


        [Fact]
        public void GetSummary_TopTargetedPaths_Sorted()
        {
            ThreatLog log = new ThreatLog(100);

            log.Record("10.0.0.1", "/admin", "", "Blocked", "Rule1", "Test");
            log.Record("10.0.0.2", "/admin", "", "Blocked", "Rule1", "Test");
            log.Record("10.0.0.3", "/login", "", "Blocked", "Rule1", "Test");

            ThreatLogSummary summary = log.GetSummary();

            Assert.True(summary.TopTargetedPaths.Count > 0);
            Assert.Equal("/admin", summary.TopTargetedPaths[0].Path);
            Assert.Equal(2, summary.TopTargetedPaths[0].Count);
        }


        // ── Clear ────────────────────────────────────────────────────────


        [Fact]
        public void Clear_RemovesAllEntries()
        {
            ThreatLog log = new ThreatLog(100);

            log.Record("10.0.0.1", "/a", "", "Blocked", "Rule1", "Test");
            log.Record("10.0.0.2", "/b", "", "Blocked", "Rule1", "Test");

            log.Clear();

            Assert.Equal(0, log.Count);
        }


        // ── Empty Log ────────────────────────────────────────────────────


        [Fact]
        public void GetRecent_EmptyLog_ReturnsEmpty()
        {
            ThreatLog log = new ThreatLog(100);

            List<ThreatLogEntry> recent = log.GetRecent();

            Assert.Empty(recent);
        }


        [Fact]
        public void GetSummary_EmptyLog_ReturnsZeros()
        {
            ThreatLog log = new ThreatLog(100);

            ThreatLogSummary summary = log.GetSummary();

            Assert.Equal(0, summary.TotalEntries);
            Assert.Equal(0, summary.BlockedCount);
            Assert.Equal(0, summary.RateLimitedCount);
        }
    }
}
