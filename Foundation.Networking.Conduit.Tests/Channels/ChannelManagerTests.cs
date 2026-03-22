// ============================================================================
//
// ChannelManagerTests.cs — Unit tests for ChannelManager.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Conduit.Channels;
using Foundation.Networking.Conduit.Configuration;
using Foundation.Networking.Conduit.Connections;

namespace Foundation.Networking.Conduit.Tests.Channels
{
    public class ChannelManagerTests
    {
        private ConduitConfiguration _config;
        private ConnectionManager _connMgr;
        private ChannelManager _channelMgr;


        private void Setup(int maxChannelsPerConn = 50, int maxSubsPerChannel = 1000)
        {
            _config = new ConduitConfiguration
            {
                MaxConnections = 100,
                MaxChannelsPerConnection = maxChannelsPerConn,
                MaxSubscribersPerChannel = maxSubsPerChannel,
                ChannelHistoryDepth = 10
            };

            _connMgr = new ConnectionManager(_config);
            _channelMgr = new ChannelManager(_config, _connMgr);
        }


        // ── Subscribe ────────────────────────────────────────────────────


        [Fact]
        public void Subscribe_AddsToChannel()
        {
            Setup();
            ClientConnection conn = _connMgr.Connect("c1");

            bool result = _channelMgr.Subscribe("c1", "general");

            Assert.True(result);
            Assert.Equal(1, _channelMgr.GetSubscriberCount("general"));
            Assert.Contains("general", conn.Channels);
        }


        [Fact]
        public void Subscribe_MultipleConnections()
        {
            Setup();
            _connMgr.Connect("c1");
            _connMgr.Connect("c2");
            _connMgr.Connect("c3");

            _channelMgr.Subscribe("c1", "chat");
            _channelMgr.Subscribe("c2", "chat");
            _channelMgr.Subscribe("c3", "chat");

            Assert.Equal(3, _channelMgr.GetSubscriberCount("chat"));
        }


        [Fact]
        public void Subscribe_InvalidConnection_ReturnsFalse()
        {
            Setup();

            Assert.False(_channelMgr.Subscribe("nonexistent", "chat"));
        }


        [Fact]
        public void Subscribe_ExceedsMaxChannels_ReturnsFalse()
        {
            Setup(maxChannelsPerConn: 2);
            _connMgr.Connect("c1");

            _channelMgr.Subscribe("c1", "ch1");
            _channelMgr.Subscribe("c1", "ch2");
            bool third = _channelMgr.Subscribe("c1", "ch3");

            Assert.False(third);
        }


        [Fact]
        public void Subscribe_ExceedsMaxSubs_ReturnsFalse()
        {
            Setup(maxSubsPerChannel: 2);
            _connMgr.Connect("c1");
            _connMgr.Connect("c2");
            _connMgr.Connect("c3");

            _channelMgr.Subscribe("c1", "full");
            _channelMgr.Subscribe("c2", "full");
            bool third = _channelMgr.Subscribe("c3", "full");

            Assert.False(third);
        }


        // ── Unsubscribe ──────────────────────────────────────────────────


        [Fact]
        public void Unsubscribe_RemovesFromChannel()
        {
            Setup();
            _connMgr.Connect("c1");

            _channelMgr.Subscribe("c1", "chat");
            bool result = _channelMgr.Unsubscribe("c1", "chat");

            Assert.True(result);
            Assert.Equal(0, _channelMgr.GetSubscriberCount("chat"));
        }


        [Fact]
        public void Unsubscribe_EmptyChannel_Removed()
        {
            Setup();
            _connMgr.Connect("c1");

            _channelMgr.Subscribe("c1", "temp");
            _channelMgr.Unsubscribe("c1", "temp");

            Assert.Equal(0, _channelMgr.ChannelCount);
        }


        [Fact]
        public void UnsubscribeAll_RemovesFromEverything()
        {
            Setup();
            _connMgr.Connect("c1");

            _channelMgr.Subscribe("c1", "ch1");
            _channelMgr.Subscribe("c1", "ch2");
            _channelMgr.Subscribe("c1", "ch3");

            _channelMgr.UnsubscribeAll("c1");

            Assert.Equal(0, _channelMgr.ChannelCount);
        }


        // ── Broadcast ────────────────────────────────────────────────────


        [Fact]
        public void Broadcast_ReturnsRecipients_ExcludingSender()
        {
            Setup();
            _connMgr.Connect("sender");
            _connMgr.Connect("r1");
            _connMgr.Connect("r2");

            _channelMgr.Subscribe("sender", "chat");
            _channelMgr.Subscribe("r1", "chat");
            _channelMgr.Subscribe("r2", "chat");

            List<string> recipients = _channelMgr.Broadcast("chat", "sender", "hello");

            Assert.Equal(2, recipients.Count);
            Assert.DoesNotContain("sender", recipients);
            Assert.Contains("r1", recipients);
            Assert.Contains("r2", recipients);
        }


        [Fact]
        public void Broadcast_AddsToHistory()
        {
            Setup();
            _connMgr.Connect("c1");
            _channelMgr.Subscribe("c1", "chat");

            _channelMgr.Broadcast("chat", "c1", "msg1");
            _channelMgr.Broadcast("chat", "c1", "msg2");

            List<ChannelMessage> history = _channelMgr.GetHistory("chat");

            Assert.Equal(2, history.Count);
        }


        [Fact]
        public void Broadcast_TrimsHistoryToMax()
        {
            Setup();
            _connMgr.Connect("c1");
            _channelMgr.Subscribe("c1", "chat");

            for (int i = 0; i < 20; i++)
            {
                _channelMgr.Broadcast("chat", "c1", "msg" + i);
            }

            //
            // Config sets ChannelHistoryDepth = 10
            //
            Assert.Equal(10, _channelMgr.GetHistory("chat", 100).Count);
        }


        [Fact]
        public void Broadcast_TypedMessage_Works()
        {
            Setup();
            _connMgr.Connect("c1");
            _channelMgr.Subscribe("c1", "data");

            _channelMgr.Broadcast("data", "c1", new { Type = "update", Value = 42 });

            List<ChannelMessage> history = _channelMgr.GetHistory("data");
            Assert.Single(history);
            Assert.Contains("42", history[0].Payload);
        }


        // ── Channel Info ─────────────────────────────────────────────────


        [Fact]
        public void GetChannelInfo_ReturnsDetails()
        {
            Setup();
            _connMgr.Connect("c1");
            _connMgr.Connect("c2");

            _channelMgr.Subscribe("c1", "info-test");
            _channelMgr.Subscribe("c2", "info-test");

            _channelMgr.Broadcast("info-test", "c1", "hello");

            ChannelInfo info = _channelMgr.GetChannelInfo("info-test");

            Assert.NotNull(info);
            Assert.Equal("info-test", info.Name);
            Assert.Equal(2, info.SubscriberCount);
            Assert.Equal(1, info.TotalMessages);
        }


        [Fact]
        public void GetChannelInfo_NonExistent_ReturnsNull()
        {
            Setup();

            Assert.Null(_channelMgr.GetChannelInfo("missing"));
        }


        [Fact]
        public void GetChannelNames_ReturnsAllChannels()
        {
            Setup();
            _connMgr.Connect("c1");

            _channelMgr.Subscribe("c1", "alpha");
            _channelMgr.Subscribe("c1", "beta");

            List<string> names = _channelMgr.GetChannelNames();

            Assert.Equal(2, names.Count);
            Assert.Contains("alpha", names);
            Assert.Contains("beta", names);
        }


        // ── History ──────────────────────────────────────────────────────


        [Fact]
        public void GetHistory_LimitsCount()
        {
            Setup();
            _connMgr.Connect("c1");
            _channelMgr.Subscribe("c1", "h");

            for (int i = 0; i < 8; i++)
            {
                _channelMgr.Broadcast("h", "c1", "m" + i);
            }

            Assert.Equal(3, _channelMgr.GetHistory("h", 3).Count);
        }


        [Fact]
        public void GetHistory_EmptyChannel_ReturnsEmpty()
        {
            Setup();

            Assert.Empty(_channelMgr.GetHistory("missing"));
        }


        // ── GetSubscriberCount ───────────────────────────────────────────


        [Fact]
        public void GetSubscriberCount_NonExistent_ReturnsZero()
        {
            Setup();

            Assert.Equal(0, _channelMgr.GetSubscriberCount("missing"));
        }
    }
}
