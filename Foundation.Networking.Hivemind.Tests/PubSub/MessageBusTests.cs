// ============================================================================
//
// MessageBusTests.cs — Unit tests for MessageBus.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Hivemind.Configuration;
using Foundation.Networking.Hivemind.PubSub;

namespace Foundation.Networking.Hivemind.Tests.PubSub
{
    public class MessageBusTests
    {
        private HivemindConfiguration CreateConfig()
        {
            return new HivemindConfiguration
            {
                PubSubMaxQueueDepth = 100
            };
        }


        // ── Subscribe / Publish ──────────────────────────────────────────


        [Fact]
        public void Publish_DeliversToSubscriber()
        {
            MessageBus bus = new MessageBus(CreateConfig());
            BusMessage received = null;

            bus.Subscribe("events", msg => received = msg);
            bus.Publish("events", "hello");

            Assert.NotNull(received);
            Assert.Equal("hello", received.Payload);
            Assert.Equal("events", received.Channel);
        }


        [Fact]
        public void Publish_DeliversToAllSubscribers()
        {
            MessageBus bus = new MessageBus(CreateConfig());
            int count = 0;

            bus.Subscribe("events", msg => count++);
            bus.Subscribe("events", msg => count++);
            bus.Subscribe("events", msg => count++);

            int delivered = bus.Publish("events", "hello");

            Assert.Equal(3, delivered);
            Assert.Equal(3, count);
        }


        [Fact]
        public void Publish_NoSubscribers_ReturnsZero()
        {
            MessageBus bus = new MessageBus(CreateConfig());

            int delivered = bus.Publish("events", "hello");

            Assert.Equal(0, delivered);
        }


        [Fact]
        public void Publish_DifferentChannels_Independent()
        {
            MessageBus bus = new MessageBus(CreateConfig());
            int channel1Count = 0;
            int channel2Count = 0;

            bus.Subscribe("ch1", msg => channel1Count++);
            bus.Subscribe("ch2", msg => channel2Count++);

            bus.Publish("ch1", "msg1");
            bus.Publish("ch1", "msg2");
            bus.Publish("ch2", "msg3");

            Assert.Equal(2, channel1Count);
            Assert.Equal(1, channel2Count);
        }


        // ── Unsubscribe ──────────────────────────────────────────────────


        [Fact]
        public void Unsubscribe_StopsDelivery()
        {
            MessageBus bus = new MessageBus(CreateConfig());
            int count = 0;

            string subId = bus.Subscribe("events", msg => count++);

            bus.Publish("events", "first");
            Assert.Equal(1, count);

            bus.Unsubscribe("events", subId);

            bus.Publish("events", "second");
            Assert.Equal(1, count);
        }


        [Fact]
        public void Unsubscribe_NonExistent_ReturnsFalse()
        {
            MessageBus bus = new MessageBus(CreateConfig());

            Assert.False(bus.Unsubscribe("events", "missing"));
        }


        // ── Channel Management ───────────────────────────────────────────


        [Fact]
        public void GetChannels_ReturnsActiveChannels()
        {
            MessageBus bus = new MessageBus(CreateConfig());

            bus.Subscribe("ch1", msg => { });
            bus.Subscribe("ch2", msg => { });

            List<string> channels = bus.GetChannels();

            Assert.Equal(2, channels.Count);
            Assert.Contains("ch1", channels);
            Assert.Contains("ch2", channels);
        }


        [Fact]
        public void GetSubscriberCount_ReturnsCorrectCount()
        {
            MessageBus bus = new MessageBus(CreateConfig());

            bus.Subscribe("events", msg => { });
            bus.Subscribe("events", msg => { });

            Assert.Equal(2, bus.GetSubscriberCount("events"));
        }


        [Fact]
        public void GetSubscriberCount_NoChannel_ReturnsZero()
        {
            MessageBus bus = new MessageBus(CreateConfig());

            Assert.Equal(0, bus.GetSubscriberCount("missing"));
        }


        // ── History ──────────────────────────────────────────────────────


        [Fact]
        public void GetHistory_ReturnsPublishedMessages()
        {
            MessageBus bus = new MessageBus(CreateConfig());

            bus.Publish("events", "msg1");
            bus.Publish("events", "msg2");
            bus.Publish("events", "msg3");

            List<BusMessage> history = bus.GetHistory("events");

            Assert.Equal(3, history.Count);
        }


        [Fact]
        public void GetHistory_LimitsCount()
        {
            MessageBus bus = new MessageBus(CreateConfig());

            for (int i = 0; i < 20; i++)
            {
                bus.Publish("events", "msg" + i);
            }

            List<BusMessage> history = bus.GetHistory("events", 5);

            Assert.Equal(5, history.Count);
        }


        [Fact]
        public void GetHistory_EmptyChannel_ReturnsEmpty()
        {
            MessageBus bus = new MessageBus(CreateConfig());

            Assert.Empty(bus.GetHistory("missing"));
        }


        [Fact]
        public void History_TrimmedToMaxDepth()
        {
            HivemindConfiguration config = CreateConfig();
            config.PubSubMaxQueueDepth = 5;

            MessageBus bus = new MessageBus(config);

            for (int i = 0; i < 10; i++)
            {
                bus.Publish("events", "msg" + i);
            }

            List<BusMessage> history = bus.GetHistory("events", 100);

            Assert.Equal(5, history.Count);
        }


        // ── Typed Messages ───────────────────────────────────────────────


        [Fact]
        public void Publish_TypedMessage_SerializesPayload()
        {
            MessageBus bus = new MessageBus(CreateConfig());
            BusMessage received = null;

            bus.Subscribe("events", msg => received = msg);
            bus.Publish("events", new { Name = "Test", Value = 42 });

            Assert.NotNull(received);
            Assert.Contains("Test", received.Payload);
            Assert.Contains("42", received.Payload);
        }


        // ── Fault Tolerance ──────────────────────────────────────────────


        [Fact]
        public void Publish_BadSubscriber_DoesNotAffectOthers()
        {
            MessageBus bus = new MessageBus(CreateConfig());
            int goodCount = 0;

            bus.Subscribe("events", msg => throw new System.Exception("bad"));
            bus.Subscribe("events", msg => goodCount++);

            bus.Publish("events", "hello");

            Assert.Equal(1, goodCount);
        }


        // ── Statistics ───────────────────────────────────────────────────


        [Fact]
        public void GetStatistics_TracksPublishedAndDelivered()
        {
            MessageBus bus = new MessageBus(CreateConfig());

            bus.Subscribe("events", msg => { });
            bus.Subscribe("events", msg => { });

            bus.Publish("events", "msg1");
            bus.Publish("events", "msg2");

            MessageBusStatistics stats = bus.GetStatistics();

            Assert.Equal(2, stats.TotalPublished);
            Assert.Equal(4, stats.TotalDelivered);
            Assert.Equal(1, stats.ChannelCount);
            Assert.Equal(2, stats.TotalSubscribers);
        }
    }
}
