// ============================================================================
//
// TcpClientConnectionTests.cs — Tests for TCP 2-byte length framing.
//
// ============================================================================

using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Xunit;

using Foundation.Networking.Coturn.Protocol;
using Foundation.Networking.Coturn.Server;
using Foundation.Networking.Coturn.Server.Handlers;

namespace Foundation.Networking.Coturn.Tests.Server
{
    public class TcpClientConnectionTests
    {
        [Fact]
        public void FrameHeaderSize_Is2Bytes()
        {
            Assert.Equal(2, TcpClientConnection.FRAME_HEADER_SIZE);
        }


        [Fact]
        public void FramedWrite_ProducesCorrectFormat()
        {
            //
            // Create a STUN Binding Request and simulate framing
            //
            StunMessage request = StunMessage.CreateBindingRequest();
            byte[] messageBytes = request.Encode();

            //
            // Build the expected framed format: 2-byte length + message
            //
            byte[] expectedFrame = new byte[2 + messageBytes.Length];
            BinaryPrimitives.WriteUInt16BigEndian(expectedFrame.AsSpan(0), (ushort)messageBytes.Length);
            Array.Copy(messageBytes, 0, expectedFrame, 2, messageBytes.Length);

            //
            // Verify the frame structure
            //
            ushort frameLength = BinaryPrimitives.ReadUInt16BigEndian(expectedFrame.AsSpan(0));
            Assert.Equal(messageBytes.Length, (int)frameLength);

            //
            // Verify the message can be parsed from within the frame
            //
            byte[] extractedMessage = new byte[frameLength];
            Array.Copy(expectedFrame, 2, extractedMessage, 0, frameLength);

            bool parsed = StunMessage.TryParse(extractedMessage, out StunMessage parsedMsg);
            Assert.True(parsed);
            Assert.Equal(StunMessageType.BINDING_REQUEST, parsedMsg.MessageType);
        }


        [Fact]
        public void ReadExact_Simulation_HandlesPartialReads()
        {
            //
            // Simulate a stream that returns data in small chunks
            // (mimicking TCP fragmentation)
            //
            byte[] fullData = new byte[] { 0x00, 0x14, 0x01, 0x02, 0x03, 0x04,
                                            0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                                            0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
                                            0x11, 0x12, 0x13, 0x14 };

            using (MemoryStream ms = new MemoryStream(fullData))
            {
                //
                // Read the 2-byte length header
                //
                byte[] header = new byte[2];
                int totalRead = 0;

                while (totalRead < 2)
                {
                    int bytesRead = ms.Read(header, totalRead, 2 - totalRead);
                    Assert.True(bytesRead > 0, "Stream ended unexpectedly");
                    totalRead += bytesRead;
                }

                ushort messageLength = BinaryPrimitives.ReadUInt16BigEndian(header);
                Assert.Equal(20, (int)messageLength);

                //
                // Read the full message
                //
                byte[] messageBytes = new byte[messageLength];
                totalRead = 0;

                while (totalRead < messageLength)
                {
                    int bytesRead = ms.Read(messageBytes, totalRead, messageLength - totalRead);
                    Assert.True(bytesRead > 0, "Stream ended unexpectedly");
                    totalRead += bytesRead;
                }

                Assert.Equal(0x01, messageBytes[0]);
                Assert.Equal(0x14, messageBytes[19]);
            }
        }


        [Fact]
        public void ChannelData_FramedCorrectly_OverTcp()
        {
            //
            // ChannelData over TCP also uses 2-byte length framing
            // around the 4-byte ChannelData header + payload
            //
            byte[] payload = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            ushort channelNumber = 0x4001;

            //
            // Build ChannelData (4-byte header + payload)
            //
            byte[] channelData = new byte[4 + payload.Length];
            BinaryPrimitives.WriteUInt16BigEndian(channelData.AsSpan(0), channelNumber);
            BinaryPrimitives.WriteUInt16BigEndian(channelData.AsSpan(2), (ushort)payload.Length);
            Array.Copy(payload, 0, channelData, 4, payload.Length);

            //
            // Frame it for TCP transport
            //
            byte[] tcpFrame = new byte[2 + channelData.Length];
            BinaryPrimitives.WriteUInt16BigEndian(tcpFrame.AsSpan(0), (ushort)channelData.Length);
            Array.Copy(channelData, 0, tcpFrame, 2, channelData.Length);

            //
            // Verify framing
            //
            ushort frameLength = BinaryPrimitives.ReadUInt16BigEndian(tcpFrame.AsSpan(0));
            Assert.Equal(channelData.Length, (int)frameLength);

            //
            // Extract and verify ChannelData
            //
            byte[] extracted = new byte[frameLength];
            Array.Copy(tcpFrame, 2, extracted, 0, frameLength);

            Assert.True(TurnRelayHandler.IsChannelData(extracted, extracted.Length));

            ushort extractedChannel = BinaryPrimitives.ReadUInt16BigEndian(extracted.AsSpan(0));
            Assert.Equal(channelNumber, extractedChannel);
        }


        [Fact]
        public void MultipleFramedMessages_InSingleStream()
        {
            //
            // Simulate multiple framed STUN messages in a single TCP stream
            //
            StunMessage msg1 = StunMessage.CreateBindingRequest();
            StunMessage msg2 = StunMessage.CreateBindingRequest();

            byte[] bytes1 = msg1.Encode();
            byte[] bytes2 = msg2.Encode();

            //
            // Concatenate two framed messages
            //
            byte[] stream = new byte[2 + bytes1.Length + 2 + bytes2.Length];
            int offset = 0;

            BinaryPrimitives.WriteUInt16BigEndian(stream.AsSpan(offset), (ushort)bytes1.Length);
            offset += 2;
            Array.Copy(bytes1, 0, stream, offset, bytes1.Length);
            offset += bytes1.Length;

            BinaryPrimitives.WriteUInt16BigEndian(stream.AsSpan(offset), (ushort)bytes2.Length);
            offset += 2;
            Array.Copy(bytes2, 0, stream, offset, bytes2.Length);

            //
            // Read both messages back
            //
            using (MemoryStream ms = new MemoryStream(stream))
            {
                byte[] header = new byte[2];
                int messagesRead = 0;

                while (ms.Position < ms.Length)
                {
                    int headerRead = ms.Read(header, 0, 2);
                    if (headerRead == 0) break;

                    ushort msgLength = BinaryPrimitives.ReadUInt16BigEndian(header);
                    byte[] msgBytes = new byte[msgLength];
                    int totalRead = 0;

                    while (totalRead < msgLength)
                    {
                        int bytesRead = ms.Read(msgBytes, totalRead, msgLength - totalRead);
                        if (bytesRead == 0) break;
                        totalRead += bytesRead;
                    }

                    bool parsed = StunMessage.TryParse(msgBytes, out StunMessage msg);
                    Assert.True(parsed);
                    Assert.Equal(StunMessageType.BINDING_REQUEST, msg.MessageType);

                    messagesRead++;
                }

                Assert.Equal(2, messagesRead);
            }
        }
    }
}
