// ============================================================================
//
// StunRequestHandlerTests.cs — Tests for STUN Binding request handling.
//
// ============================================================================

using System.Net;
using Xunit;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Protocol;
using Foundation.Networking.Coturn.Server.Handlers;

namespace Foundation.Networking.Coturn.Tests.Server
{
    public class StunRequestHandlerTests
    {
        [Fact]
        public void HandleBindingRequest_ReturnsXorMappedAddress()
        {
            TurnServerConfiguration config = new TurnServerConfiguration
            {
                Software = "TestServer/1.0"
            };

            StunRequestHandler handler = new StunRequestHandler(config);

            //
            // Create a binding request
            //
            StunMessage request = StunMessage.CreateBindingRequest();
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 12345);

            //
            // Handle it
            //
            byte[] responseBytes = handler.HandleBindingRequest(request, clientEndPoint);

            Assert.NotNull(responseBytes);

            //
            // Parse the response
            //
            bool parsed = StunMessage.TryParse(responseBytes, out StunMessage response);
            Assert.True(parsed);

            //
            // Should be a Binding Success Response
            //
            Assert.Equal(StunMessageType.BINDING_SUCCESS_RESPONSE, response.MessageType);

            //
            // Should have XOR-MAPPED-ADDRESS
            //
            StunAttribute xorAttr = response.FindAttribute(StunAttributeType.XOR_MAPPED_ADDRESS);
            Assert.NotNull(xorAttr);

            IPEndPoint mappedAddress = StunAttribute.ParseXorMappedAddress(xorAttr.Value, response.TransactionId);
            Assert.Equal(clientEndPoint.Address, mappedAddress.Address);
            Assert.Equal(clientEndPoint.Port, mappedAddress.Port);

            //
            // Should have SOFTWARE attribute
            //
            StunAttribute softwareAttr = response.FindAttribute(StunAttributeType.SOFTWARE);
            Assert.NotNull(softwareAttr);
            Assert.Equal("TestServer/1.0", softwareAttr.ReadString());

            //
            // Should have FINGERPRINT
            //
            Assert.True(MessageIntegrity.ValidateFingerprint(responseBytes));
        }
    }
}
