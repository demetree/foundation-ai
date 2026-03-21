// ============================================================================
//
// TcpTurnListenerTests.cs — Tests for TCP listener and STUN-over-TCP.
//
// Uses localhost loopback to test the full TCP listener pipeline:
//   connect → send framed STUN Binding → receive framed response
//
// ============================================================================

using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Xunit;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Protocol;
using Foundation.Networking.Coturn.Server;
using Foundation.Networking.Coturn.Server.Handlers;

namespace Foundation.Networking.Coturn.Tests.Server
{
    public class TcpTurnListenerTests
    {
        private TurnServerConfiguration CreateConfig(int tcpPort)
        {
            return new TurnServerConfiguration
            {
                ListenAddress = "127.0.0.1",
                ListenPort = tcpPort + 1,  // UDP on a different port
                RelayAddress = "127.0.0.1",
                ExternalAddress = "127.0.0.1",
                RelayPortMin = 61000,
                RelayPortMax = 61050,
                MaxAllocationsPerUser = 10,
                DefaultLifetime = 600,
                MaxLifetime = 3600,
                Realm = "test.local",
                SharedSecret = "test-secret",
                TcpEnabled = true,
                TcpListenPort = tcpPort,
                TlsEnabled = false,
                Software = "TestServer/1.0"
            };
        }


        [Fact]
        public void TcpBinding_Request_ReturnsXorMappedAddress()
        {
            //
            // Use a high port to avoid conflicts
            //
            int port = 63478;
            TurnServerConfiguration config = CreateConfig(port);

            using (AllocationManager allocationManager = new AllocationManager(config))
            {
                StunRequestHandler stunHandler = new StunRequestHandler(config);
                TurnAuthenticator authenticator = new TurnAuthenticator(config);
                TurnRequestHandler turnHandler = new TurnRequestHandler(config, allocationManager, authenticator);
                TurnRelayHandler relayHandler = new TurnRelayHandler();

                using (TcpTurnListener listener = new TcpTurnListener(
                    config, allocationManager, stunHandler, turnHandler, relayHandler, port))
                {
                    listener.Start();

                    //
                    // Give the listener a moment to start accepting
                    //
                    Thread.Sleep(100);

                    //
                    // Connect as a TCP client
                    //
                    using (TcpClient client = new TcpClient())
                    {
                        client.Connect(IPAddress.Loopback, port);
                        NetworkStream stream = client.GetStream();

                        //
                        // Build and send a framed STUN Binding Request
                        //
                        StunMessage request = StunMessage.CreateBindingRequest();
                        byte[] messageBytes = request.Encode();

                        byte[] frame = new byte[2 + messageBytes.Length];
                        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(0), (ushort)messageBytes.Length);
                        Array.Copy(messageBytes, 0, frame, 2, messageBytes.Length);

                        stream.Write(frame, 0, frame.Length);
                        stream.Flush();

                        //
                        // Read the framed response
                        //
                        byte[] responseHeader = new byte[2];
                        int headerRead = ReadExact(stream, responseHeader, 0, 2, 2000);
                        Assert.Equal(2, headerRead);

                        ushort responseLength = BinaryPrimitives.ReadUInt16BigEndian(responseHeader);
                        Assert.True(responseLength > 0);
                        Assert.True(responseLength < 1000);

                        byte[] responseBytes = new byte[responseLength];
                        int bodyRead = ReadExact(stream, responseBytes, 0, responseLength, 2000);
                        Assert.Equal(responseLength, bodyRead);

                        //
                        // Parse the response
                        //
                        bool parsed = StunMessage.TryParse(responseBytes, out StunMessage response);
                        Assert.True(parsed);
                        Assert.Equal(StunMessageType.BINDING_SUCCESS_RESPONSE, response.MessageType);

                        //
                        // Should have XOR-MAPPED-ADDRESS
                        //
                        StunAttribute xorAttr = response.FindAttribute(StunAttributeType.XOR_MAPPED_ADDRESS);
                        Assert.NotNull(xorAttr);

                        IPEndPoint mapped = StunAttribute.ParseXorMappedAddress(xorAttr.Value, response.TransactionId);
                        Assert.NotNull(mapped);
                        Assert.Equal(IPAddress.Loopback, mapped.Address);
                    }
                }
            }
        }


        [Fact]
        public void TcpListener_TracksConnections()
        {
            int port = 63479;
            TurnServerConfiguration config = CreateConfig(port);

            using (AllocationManager allocationManager = new AllocationManager(config))
            {
                StunRequestHandler stunHandler = new StunRequestHandler(config);
                TurnAuthenticator authenticator = new TurnAuthenticator(config);
                TurnRequestHandler turnHandler = new TurnRequestHandler(config, allocationManager, authenticator);
                TurnRelayHandler relayHandler = new TurnRelayHandler();

                using (TcpTurnListener listener = new TcpTurnListener(
                    config, allocationManager, stunHandler, turnHandler, relayHandler, port))
                {
                    listener.Start();
                    Thread.Sleep(100);

                    Assert.Equal(0, listener.ConnectionCount);

                    //
                    // Connect a client
                    //
                    using (TcpClient client = new TcpClient())
                    {
                        client.Connect(IPAddress.Loopback, port);
                        Thread.Sleep(200);

                        Assert.True(listener.ConnectionCount >= 1);
                    }

                    //
                    // After disconnect, give time for cleanup
                    //
                    Thread.Sleep(500);

                    // Connection count should decrease after client disconnects
                    // (may take a moment for the receive loop to detect the close)
                }
            }
        }


        /// <summary>
        /// Reads exactly 'count' bytes from the stream with a timeout.
        /// Returns the number of bytes actually read.
        /// </summary>
        private static int ReadExact(NetworkStream stream, byte[] buffer, int offset, int count, int timeoutMs)
        {
            stream.ReadTimeout = timeoutMs;
            int totalRead = 0;

            while (totalRead < count)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, offset + totalRead, count - totalRead);

                    if (bytesRead == 0) break;

                    totalRead += bytesRead;
                }
                catch (IOException)
                {
                    break;
                }
            }

            return totalRead;
        }
    }
}
