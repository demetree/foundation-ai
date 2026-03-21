// ============================================================================
//
// StunRequestHandler.cs — Handles STUN Binding requests.
//
// STUN Binding is the simplest operation: the server receives a Binding
// Request and responds with the client's server-reflexive transport address
// (the address as seen by the server) in a XOR-MAPPED-ADDRESS attribute.
//
// No authentication is required for STUN Binding.
//
// ============================================================================

using System.Net;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Protocol;

namespace Foundation.Networking.Coturn.Server.Handlers
{
    /// <summary>
    ///
    /// Handles STUN Binding requests — returns the client's server-reflexive
    /// transport address.
    ///
    /// </summary>
    public class StunRequestHandler
    {
        private readonly TurnServerConfiguration _config;


        public StunRequestHandler(TurnServerConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Processes a STUN Binding Request and returns the response bytes.
        /// </summary>
        public byte[] HandleBindingRequest(StunMessage request, IPEndPoint clientEndPoint)
        {
            //
            // Create the Binding Success Response
            //
            StunMessage response = StunMessage.CreateBindingSuccessResponse(request);

            //
            // Add XOR-MAPPED-ADDRESS with the client's address as seen by the server
            //
            response.AddAttribute(
                StunAttribute.CreateXorMappedAddress(clientEndPoint, response.TransactionId));

            //
            // Add SOFTWARE attribute
            //
            if (string.IsNullOrWhiteSpace(_config.Software) == false)
            {
                response.AddAttribute(StunAttribute.CreateSoftware(_config.Software));
            }

            //
            // Encode and add fingerprint
            //
            byte[] encoded = response.Encode();
            encoded = MessageIntegrity.AppendFingerprint(encoded);

            return encoded;
        }
    }
}
