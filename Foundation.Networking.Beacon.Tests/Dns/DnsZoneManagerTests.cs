// ============================================================================
//
// DnsZoneManagerTests.cs — Unit tests for DnsZoneManager.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Beacon.Configuration;
using Foundation.Networking.Beacon.Dns;

namespace Foundation.Networking.Beacon.Tests.Dns
{
    public class DnsZoneManagerTests
    {
        // ── Add / Query ──────────────────────────────────────────────────


        [Fact]
        public void AddRecord_And_Query_ReturnsRecord()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord
            {
                Hostname = "scheduler.foundation.local",
                RecordType = "A",
                Value = "10.0.0.1"
            });

            List<DnsRecord> results = zone.Query("scheduler.foundation.local");

            Assert.Single(results);
            Assert.Equal("10.0.0.1", results[0].Value);
        }


        [Fact]
        public void Query_CaseInsensitive()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord
            {
                Hostname = "Scheduler.Foundation.LOCAL",
                Value = "10.0.0.1"
            });

            Assert.Single(zone.Query("scheduler.foundation.local"));
            Assert.Single(zone.Query("SCHEDULER.FOUNDATION.LOCAL"));
        }


        [Fact]
        public void Query_ByRecordType_Filters()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord { Hostname = "app.local", RecordType = "A", Value = "10.0.0.1" });
            zone.AddRecord(new DnsRecord { Hostname = "app.local", RecordType = "AAAA", Value = "::1" });
            zone.AddRecord(new DnsRecord { Hostname = "app.local", RecordType = "TXT", Value = "v=spf1" });

            Assert.Single(zone.Query("app.local", "A"));
            Assert.Single(zone.Query("app.local", "AAAA"));
            Assert.Single(zone.Query("app.local", "TXT"));
            Assert.Equal(3, zone.Query("app.local").Count);
        }


        [Fact]
        public void Query_NonExistent_ReturnsEmpty()
        {
            DnsZoneManager zone = new DnsZoneManager();

            Assert.Empty(zone.Query("missing.local"));
        }


        // ── Multiple Records ─────────────────────────────────────────────


        [Fact]
        public void AddRecord_MultipleForSameHost()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord { Hostname = "lb.local", RecordType = "A", Value = "10.0.0.1" });
            zone.AddRecord(new DnsRecord { Hostname = "lb.local", RecordType = "A", Value = "10.0.0.2" });

            Assert.Equal(2, zone.Query("lb.local", "A").Count);
        }


        // ── Remove ───────────────────────────────────────────────────────


        [Fact]
        public void RemoveHost_DeletesAllRecords()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord { Hostname = "app.local", RecordType = "A", Value = "10.0.0.1" });
            zone.AddRecord(new DnsRecord { Hostname = "app.local", RecordType = "TXT", Value = "test" });

            bool removed = zone.RemoveHost("app.local");

            Assert.True(removed);
            Assert.Empty(zone.Query("app.local"));
        }


        [Fact]
        public void RemoveHost_NonExistent_ReturnsFalse()
        {
            DnsZoneManager zone = new DnsZoneManager();

            Assert.False(zone.RemoveHost("missing.local"));
        }


        [Fact]
        public void RemoveRecords_ByType_RemovesOnlyThatType()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord { Hostname = "app.local", RecordType = "A", Value = "10.0.0.1" });
            zone.AddRecord(new DnsRecord { Hostname = "app.local", RecordType = "TXT", Value = "test" });

            int removed = zone.RemoveRecords("app.local", "A");

            Assert.Equal(1, removed);
            Assert.Empty(zone.Query("app.local", "A"));
            Assert.Single(zone.Query("app.local", "TXT"));
        }


        // ── Update ───────────────────────────────────────────────────────


        [Fact]
        public void UpdateRecords_ChangesValue()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord { Hostname = "app.local", RecordType = "A", Value = "10.0.0.1" });

            int updated = zone.UpdateRecords("app.local", "A", "10.0.0.99");

            Assert.Equal(1, updated);
            Assert.Equal("10.0.0.99", zone.Query("app.local", "A")[0].Value);
        }


        [Fact]
        public void UpdateRecords_NonExistent_ReturnsZero()
        {
            DnsZoneManager zone = new DnsZoneManager();

            Assert.Equal(0, zone.UpdateRecords("missing.local", "A", "10.0.0.1"));
        }


        // ── Counts ───────────────────────────────────────────────────────


        [Fact]
        public void RecordCount_ReflectsTotal()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord { Hostname = "a.local", Value = "1" });
            zone.AddRecord(new DnsRecord { Hostname = "a.local", Value = "2" });
            zone.AddRecord(new DnsRecord { Hostname = "b.local", Value = "3" });

            Assert.Equal(3, zone.RecordCount);
            Assert.Equal(2, zone.HostnameCount);
        }


        // ── Config Loading ───────────────────────────────────────────────


        [Fact]
        public void Constructor_LoadsFromConfig()
        {
            BeaconConfiguration config = new BeaconConfiguration
            {
                ZoneEntries = new List<DnsZoneEntry>
                {
                    new DnsZoneEntry { Hostname = "svc.local", RecordType = "A", Value = "10.0.0.1" },
                    new DnsZoneEntry { Hostname = "mail.local", RecordType = "MX", Value = "mail.example.com", Priority = 5 }
                }
            };

            DnsZoneManager zone = new DnsZoneManager(config);

            Assert.Equal(2, zone.RecordCount);
            Assert.Single(zone.Query("svc.local", "A"));
            Assert.Single(zone.Query("mail.local", "MX"));
        }


        // ── GetAll / GetHostnames ────────────────────────────────────────


        [Fact]
        public void GetAllRecords_ReturnsEverything()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord { Hostname = "a.local", Value = "1" });
            zone.AddRecord(new DnsRecord { Hostname = "b.local", Value = "2" });

            Assert.Equal(2, zone.GetAllRecords().Count);
        }


        [Fact]
        public void GetHostnames_ReturnsUniqueHosts()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord { Hostname = "a.local", Value = "1" });
            zone.AddRecord(new DnsRecord { Hostname = "a.local", Value = "2" });
            zone.AddRecord(new DnsRecord { Hostname = "b.local", Value = "3" });

            List<string> hosts = zone.GetHostnames();

            Assert.Equal(2, hosts.Count);
        }


        // ── Clear ────────────────────────────────────────────────────────


        [Fact]
        public void Clear_RemovesAll()
        {
            DnsZoneManager zone = new DnsZoneManager();

            zone.AddRecord(new DnsRecord { Hostname = "a.local", Value = "1" });
            zone.AddRecord(new DnsRecord { Hostname = "b.local", Value = "2" });

            zone.Clear();

            Assert.Equal(0, zone.RecordCount);
        }
    }
}
