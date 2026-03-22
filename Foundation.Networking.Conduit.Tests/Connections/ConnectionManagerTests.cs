// ============================================================================
//
// ConnectionManagerTests.cs — Unit tests for ConnectionManager.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Conduit.Configuration;
using Foundation.Networking.Conduit.Connections;

namespace Foundation.Networking.Conduit.Tests.Connections
{
    public class ConnectionManagerTests
    {
        private ConduitConfiguration CreateConfig(int maxConnections = 100)
        {
            return new ConduitConfiguration
            {
                MaxConnections = maxConnections,
                IdleTimeoutSeconds = 300
            };
        }


        // ── Connect ──────────────────────────────────────────────────────


        [Fact]
        public void Connect_ReturnsConnection()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            ClientConnection conn = mgr.Connect();

            Assert.NotNull(conn);
            Assert.False(string.IsNullOrEmpty(conn.ConnectionId));
        }


        [Fact]
        public void Connect_WithId_UsesProvidedId()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            ClientConnection conn = mgr.Connect("my-id", "10.0.0.1", "user1");

            Assert.Equal("my-id", conn.ConnectionId);
            Assert.Equal("10.0.0.1", conn.ClientIp);
            Assert.Equal("user1", conn.UserId);
        }


        [Fact]
        public void Connect_IncrementsCount()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            mgr.Connect();
            mgr.Connect();

            Assert.Equal(2, mgr.ActiveCount);
        }


        [Fact]
        public void Connect_AtLimit_ReturnsNull()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig(maxConnections: 2));

            mgr.Connect();
            mgr.Connect();
            ClientConnection third = mgr.Connect();

            Assert.Null(third);
            Assert.Equal(2, mgr.ActiveCount);
        }


        [Fact]
        public void CanAccept_WhenBelowLimit_ReturnsTrue()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig(maxConnections: 10));

            Assert.True(mgr.CanAccept);
        }


        [Fact]
        public void CanAccept_WhenAtLimit_ReturnsFalse()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig(maxConnections: 1));
            mgr.Connect();

            Assert.False(mgr.CanAccept);
        }


        // ── Disconnect ───────────────────────────────────────────────────


        [Fact]
        public void Disconnect_RemovesConnection()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            ClientConnection conn = mgr.Connect();
            bool removed = mgr.Disconnect(conn.ConnectionId);

            Assert.True(removed);
            Assert.Equal(0, mgr.ActiveCount);
        }


        [Fact]
        public void Disconnect_NonExistent_ReturnsFalse()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            Assert.False(mgr.Disconnect("missing"));
        }


        // ── Get ──────────────────────────────────────────────────────────


        [Fact]
        public void Get_ReturnsConnection()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            ClientConnection conn = mgr.Connect("test-id");

            Assert.NotNull(mgr.Get("test-id"));
        }


        [Fact]
        public void Get_NonExistent_ReturnsNull()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            Assert.Null(mgr.Get("missing"));
        }


        // ── Message Tracking ─────────────────────────────────────────────


        [Fact]
        public void RecordSent_IncrementsCounters()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            ClientConnection conn = mgr.Connect("test");
            mgr.RecordSent("test", 100);
            mgr.RecordSent("test", 200);

            Assert.Equal(2, conn.MessagesSent);
            Assert.Equal(300, conn.BytesSent);
        }


        [Fact]
        public void RecordReceived_IncrementsCounters()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            ClientConnection conn = mgr.Connect("test");
            mgr.RecordReceived("test", 150);

            Assert.Equal(1, conn.MessagesReceived);
            Assert.Equal(150, conn.BytesReceived);
        }


        // ── Touch ────────────────────────────────────────────────────────


        [Fact]
        public void Touch_UpdatesLastActivity()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            ClientConnection conn = mgr.Connect("test");
            System.DateTime before = conn.LastActivityUtc;

            System.Threading.Thread.Sleep(50);
            mgr.Touch("test");

            Assert.True(conn.LastActivityUtc >= before);
        }


        // ── Query ────────────────────────────────────────────────────────


        [Fact]
        public void GetAll_ReturnsAllConnections()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            mgr.Connect();
            mgr.Connect();
            mgr.Connect();

            Assert.Equal(3, mgr.GetAll().Count);
        }


        [Fact]
        public void GetByUser_FiltersByUserId()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            mgr.Connect(null, "", "user1");
            mgr.Connect(null, "", "user1");
            mgr.Connect(null, "", "user2");

            Assert.Equal(2, mgr.GetByUser("user1").Count);
        }


        [Fact]
        public void GetByChannel_FiltersCorrectly()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            ClientConnection c1 = mgr.Connect("c1");
            ClientConnection c2 = mgr.Connect("c2");
            ClientConnection c3 = mgr.Connect("c3");

            c1.Channels.Add("general");
            c2.Channels.Add("general");
            c3.Channels.Add("vip");

            Assert.Equal(2, mgr.GetByChannel("general").Count);
        }


        // ── Idle Cleanup ─────────────────────────────────────────────────


        [Fact]
        public void RemoveIdle_RemovesStaleConnections()
        {
            ConduitConfiguration config = CreateConfig();
            config.IdleTimeoutSeconds = 0;

            ConnectionManager mgr = new ConnectionManager(config);
            mgr.Connect();
            mgr.Connect();

            System.Threading.Thread.Sleep(50);

            int removed = mgr.RemoveIdle();

            Assert.Equal(2, removed);
            Assert.Equal(0, mgr.ActiveCount);
        }


        // ── Statistics ───────────────────────────────────────────────────


        [Fact]
        public void GetStatistics_TracksAll()
        {
            ConnectionManager mgr = new ConnectionManager(CreateConfig());

            ClientConnection c1 = mgr.Connect();
            mgr.Connect();
            mgr.Disconnect(c1.ConnectionId);

            mgr.RecordSent(mgr.GetAll()[0].ConnectionId, 100);

            ConnectionManagerStatistics stats = mgr.GetStatistics();

            Assert.Equal(1, stats.ActiveConnections);
            Assert.Equal(2, stats.TotalConnections);
            Assert.Equal(1, stats.TotalDisconnections);
            Assert.Equal(1, stats.TotalMessagesSent);
        }
    }
}
