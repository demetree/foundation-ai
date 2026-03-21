// ============================================================================
//
// TurnRelayHandlerTests.cs — Tests for data relay handling and ChannelData.
//
// ============================================================================

using System;
using System.Buffers.Binary;
using System.Net;
using Xunit;

using Foundation.Networking.Coturn.Protocol;
using Foundation.Networking.Coturn.Server;
using Foundation.Networking.Coturn.Server.Handlers;

namespace Foundation.Networking.Coturn.Tests.Server
{
    public class TurnRelayHandlerTests
    {
        [Fact]
        public void IsChannelData_ValidChannelNumber_ReturnsTrue()
        {
            byte[] data = new byte[8];
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0), 0x4001);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(2), 4);
            data[4] = 0x01;
            data[5] = 0x02;
            data[6] = 0x03;
            data[7] = 0x04;

            Assert.True(TurnRelayHandler.IsChannelData(data, data.Length));
        }


        [Fact]
        public void IsChannelData_StunMessage_ReturnsFalse()
        {
            //
            // STUN messages have first 2 bits = 00
            //
            byte[] data = new byte[20];
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0), StunMessageType.BINDING_REQUEST);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(2), 0);
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(4), StunConstants.MAGIC_COOKIE);

            Assert.False(TurnRelayHandler.IsChannelData(data, data.Length));
        }


        [Fact]
        public void IsChannelData_TooShort_ReturnsFalse()
        {
            byte[] data = new byte[2];
            Assert.False(TurnRelayHandler.IsChannelData(data, data.Length));
        }


        [Fact]
        public void BuildChannelData_ProducesCorrectFormat()
        {
            byte[] payload = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x01 };

            byte[] channelData = TurnRelayHandler.BuildChannelData(0x4001, payload, payload.Length);

            //
            // Channel number
            //
            ushort channelNumber = BinaryPrimitives.ReadUInt16BigEndian(channelData.AsSpan(0));
            Assert.Equal((ushort)0x4001, channelNumber);

            //
            // Data length
            //
            ushort dataLength = BinaryPrimitives.ReadUInt16BigEndian(channelData.AsSpan(2));
            Assert.Equal(5, (int)dataLength);

            //
            // Payload
            //
            Assert.Equal(0xDE, channelData[4]);
            Assert.Equal(0xAD, channelData[5]);
            Assert.Equal(0xBE, channelData[6]);
            Assert.Equal(0xEF, channelData[7]);
            Assert.Equal(0x01, channelData[8]);

            //
            // Total length should be padded to 4-byte boundary: 4 header + 8 (5 data + 3 padding)
            //
            Assert.Equal(12, channelData.Length);
        }


        [Fact]
        public void HandlePeerData_NoPermission_ReturnsNull()
        {
            TurnRelayHandler handler = new TurnRelayHandler();

            TurnAllocation allocation = new TurnAllocation
            {
                LifetimeSeconds = 600,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
            };

            //
            // Peer sends data but has no permission
            //
            IPEndPoint peerEp = new IPEndPoint(IPAddress.Parse("203.0.113.50"), 9000);
            byte[] peerData = new byte[] { 0x01, 0x02, 0x03 };

            byte[] result = handler.HandlePeerData(peerData, peerData.Length, peerEp, allocation);

            Assert.Null(result);

            allocation.Dispose();
        }


        [Fact]
        public void HandlePeerData_WithPermission_ReturnsDataIndication()
        {
            TurnRelayHandler handler = new TurnRelayHandler();

            TurnAllocation allocation = new TurnAllocation
            {
                LifetimeSeconds = 600,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
            };

            //
            // Install a permission for the peer
            //
            IPAddress peerIp = IPAddress.Parse("203.0.113.50");
            allocation.AddOrRefreshPermission(peerIp);

            IPEndPoint peerEp = new IPEndPoint(peerIp, 9000);
            byte[] peerData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

            byte[] result = handler.HandlePeerData(peerData, peerData.Length, peerEp, allocation);

            Assert.NotNull(result);

            //
            // Should be a parseable STUN Data Indication
            //
            bool parsed = StunMessage.TryParse(result, out StunMessage indication);
            Assert.True(parsed);
            Assert.Equal(StunMessageType.DATA_INDICATION, indication.MessageType);

            //
            // Should have XOR-PEER-ADDRESS
            //
            StunAttribute peerAttr = indication.FindAttribute(StunAttributeType.XOR_PEER_ADDRESS);
            Assert.NotNull(peerAttr);

            //
            // Should have DATA attribute
            //
            StunAttribute dataAttr = indication.FindAttribute(StunAttributeType.DATA);
            Assert.NotNull(dataAttr);
            Assert.Equal(4, dataAttr.Value.Length);
            Assert.Equal(0x01, dataAttr.Value[0]);
            Assert.Equal(0x04, dataAttr.Value[3]);

            allocation.Dispose();
        }


        [Fact]
        public void HandlePeerData_WithChannel_ReturnsChannelData()
        {
            TurnRelayHandler handler = new TurnRelayHandler();

            TurnAllocation allocation = new TurnAllocation
            {
                LifetimeSeconds = 600,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
            };

            //
            // Install a permission and bind a channel
            //
            IPEndPoint peerEp = new IPEndPoint(IPAddress.Parse("203.0.113.50"), 9000);
            allocation.AddOrRefreshChannel(0x4001, peerEp);

            byte[] peerData = new byte[] { 0xAA, 0xBB, 0xCC };

            byte[] result = handler.HandlePeerData(peerData, peerData.Length, peerEp, allocation);

            Assert.NotNull(result);

            //
            // Should be ChannelData format (first 2 bytes = channel number)
            //
            ushort channelNumber = BinaryPrimitives.ReadUInt16BigEndian(result.AsSpan(0));
            Assert.Equal((ushort)0x4001, channelNumber);

            ushort dataLength = BinaryPrimitives.ReadUInt16BigEndian(result.AsSpan(2));
            Assert.Equal(3, (int)dataLength);

            Assert.Equal(0xAA, result[4]);
            Assert.Equal(0xBB, result[5]);
            Assert.Equal(0xCC, result[6]);

            allocation.Dispose();
        }
    }
}
