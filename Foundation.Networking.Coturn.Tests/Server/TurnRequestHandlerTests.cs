// ============================================================================
//
// TurnRequestHandlerTests.cs — Tests for TURN request handling.
//
// ============================================================================

using System;
using System.Net;
using Xunit;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Protocol;
using Foundation.Networking.Coturn.Server;
using Foundation.Networking.Coturn.Server.Handlers;

namespace Foundation.Networking.Coturn.Tests.Server
{
    public class TurnRequestHandlerTests
    {
        private TurnServerConfiguration CreateConfig()
        {
            return new TurnServerConfiguration
            {
                ListenAddress = "127.0.0.1",
                ListenPort = 3478,
                RelayAddress = "127.0.0.1",
                ExternalAddress = "127.0.0.1",
                RelayPortMin = 60000,
                RelayPortMax = 60050,
                MaxAllocationsPerUser = 10,
                DefaultLifetime = 600,
                MaxLifetime = 3600,
                Realm = "test.local",
                SharedSecret = "test-secret-key",
                NonceLifetimeSeconds = 3600,
                Software = "TestServer/1.0"
            };
        }


        [Fact]
        public void AllocateRequest_NoAuth_Returns401WithRealmAndNonce()
        {
            TurnServerConfiguration config = CreateConfig();

            using (AllocationManager allocationManager = new AllocationManager(config))
            {
                TurnAuthenticator authenticator = new TurnAuthenticator(config);
                TurnRequestHandler handler = new TurnRequestHandler(config, allocationManager, authenticator);

                //
                // Create an Allocate Request with no credentials
                //
                StunMessage request = new StunMessage();
                request.MessageType = StunMessageType.ALLOCATE_REQUEST;
                request.TransactionId = new byte[12];
                Array.Fill<byte>(request.TransactionId, 0xAB);

                request.AddAttribute(StunAttribute.CreateRequestedTransport(StunConstants.TRANSPORT_UDP));

                byte[] rawBytes = request.Encode();

                IPEndPoint clientEp = new IPEndPoint(IPAddress.Loopback, 5000);
                IPEndPoint serverEp = new IPEndPoint(IPAddress.Loopback, 3478);

                byte[] responseBytes = handler.HandleRequest(request, rawBytes, clientEp, serverEp);

                Assert.NotNull(responseBytes);

                //
                // Parse the error response
                //
                bool parsed = StunMessage.TryParse(responseBytes, out StunMessage response);
                Assert.True(parsed);

                //
                // Should be an error response
                //
                Assert.True(StunMessageType.IsErrorResponse(response.MessageType));

                //
                // Should have ERROR-CODE 401
                //
                StunAttribute errorAttr = response.FindAttribute(StunAttributeType.ERROR_CODE);
                Assert.NotNull(errorAttr);
                Assert.Equal(4, (int)errorAttr.Value[2]);  // class = 4
                Assert.Equal(1, (int)errorAttr.Value[3]);  // number = 1

                //
                // Should have REALM and NONCE for the client to retry
                //
                StunAttribute realmAttr = response.FindAttribute(StunAttributeType.REALM);
                Assert.NotNull(realmAttr);
                Assert.Equal("test.local", realmAttr.ReadString());

                StunAttribute nonceAttr = response.FindAttribute(StunAttributeType.NONCE);
                Assert.NotNull(nonceAttr);
                Assert.False(string.IsNullOrEmpty(nonceAttr.ReadString()));
            }
        }


        [Fact]
        public void TurnAllocation_PermissionsAndChannels_WorkCorrectly()
        {
            //
            // Test the allocation's permission and channel management directly
            //
            TurnAllocation allocation = new TurnAllocation
            {
                LifetimeSeconds = 600,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
            };

            //
            // Initially no permissions
            //
            IPAddress peerIp = IPAddress.Parse("203.0.113.50");
            Assert.False(allocation.HasPermission(peerIp));

            //
            // Add a permission
            //
            allocation.AddOrRefreshPermission(peerIp);
            Assert.True(allocation.HasPermission(peerIp));

            //
            // Bind a channel
            //
            IPEndPoint peerEp = new IPEndPoint(peerIp, 9000);
            bool channelBound = allocation.AddOrRefreshChannel(0x4001, peerEp);
            Assert.True(channelBound);

            //
            // Find the channel
            //
            TurnChannel channel = allocation.FindChannel(0x4001);
            Assert.NotNull(channel);
            Assert.Equal(peerEp.ToString(), channel.PeerEndPoint.ToString());

            //
            // Reverse lookup
            //
            ushort foundChannel = allocation.FindChannelForPeer(peerEp);
            Assert.Equal((ushort)0x4001, foundChannel);

            //
            // Channel number conflict — same number, different peer
            //
            IPEndPoint otherPeer = new IPEndPoint(IPAddress.Parse("203.0.113.60"), 9001);
            bool conflictResult = allocation.AddOrRefreshChannel(0x4001, otherPeer);
            Assert.False(conflictResult);

            allocation.Dispose();
        }


        [Fact]
        public void TurnAllocation_CleanupExpired_RemovesOldPermissionsAndChannels()
        {
            TurnAllocation allocation = new TurnAllocation
            {
                LifetimeSeconds = 600,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
            };

            //
            // Add a permission and channel, then force them to expire
            //
            IPAddress peerIp = IPAddress.Parse("203.0.113.50");
            allocation.AddOrRefreshPermission(peerIp);

            IPEndPoint peerEp = new IPEndPoint(peerIp, 9000);
            allocation.AddOrRefreshChannel(0x4001, peerEp);

            //
            // Force expiry
            //
            allocation.Permissions[peerIp.ToString()].ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
            allocation.Channels[0x4001].ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);

            allocation.CleanupExpired();

            Assert.False(allocation.HasPermission(peerIp));
            Assert.Null(allocation.FindChannel(0x4001));

            allocation.Dispose();
        }


        [Fact]
        public void TurnChannel_IsValidChannelNumber_ValidatesRange()
        {
            Assert.True(TurnChannel.IsValidChannelNumber(0x4000));
            Assert.True(TurnChannel.IsValidChannelNumber(0x7FFE));
            Assert.True(TurnChannel.IsValidChannelNumber(0x5000));

            Assert.False(TurnChannel.IsValidChannelNumber(0x3FFF));
            Assert.False(TurnChannel.IsValidChannelNumber(0x7FFF));
            Assert.False(TurnChannel.IsValidChannelNumber(0x0000));
            Assert.False(TurnChannel.IsValidChannelNumber(0x8000));
        }
    }
}
