// ============================================================================
//
// TlsTurnListener.cs — TLS-wrapped TCP listener for STUN/TURN-over-TLS.
//
// Extends TcpTurnListener to perform a TLS handshake on accepted connections
// using SslStream.  Listens on port 5349 by default per RFC 5766.
//
// The TLS certificate is loaded from a PFX/PKCS#12 file specified in the
// TurnServerConfiguration.
//
// ============================================================================

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Protocol;
using Foundation.Networking.Coturn.Server.Handlers;

namespace Foundation.Networking.Coturn.Server
{
    /// <summary>
    ///
    /// TLS listener for STUN/TURN-over-TLS.
    ///
    /// Inherits the TCP accept loop from TcpTurnListener and wraps
    /// accepted connections in SslStream for TLS encryption.
    ///
    /// </summary>
    public class TlsTurnListener : TcpTurnListener
    {
        private readonly X509Certificate2 _certificate;


        public TlsTurnListener(
            TurnServerConfiguration config,
            AllocationManager allocationManager,
            StunRequestHandler stunHandler,
            TurnRequestHandler turnHandler,
            TurnRelayHandler relayHandler)
            : base(
                config,
                allocationManager,
                stunHandler,
                turnHandler,
                relayHandler,
                config.TlsListenPort,
                StunConstants.TRANSPORT_TCP)
        {
            //
            // Load the TLS certificate
            //
            if (string.IsNullOrWhiteSpace(config.TlsCertificatePath))
            {
                throw new InvalidOperationException("TLS is enabled but TlsCertificatePath is not configured.");
            }

            if (File.Exists(config.TlsCertificatePath) == false)
            {
                throw new FileNotFoundException(
                    $"TLS certificate not found: {config.TlsCertificatePath}",
                    config.TlsCertificatePath);
            }

            if (string.IsNullOrEmpty(config.TlsCertificatePassword))
            {
                _certificate = new X509Certificate2(config.TlsCertificatePath);
            }
            else
            {
                _certificate = new X509Certificate2(config.TlsCertificatePath, config.TlsCertificatePassword);
            }
        }


        /// <summary>
        /// Override: wraps the accepted socket in an SslStream and performs
        /// the TLS handshake before starting the receive loop.
        /// </summary>
        protected override void HandleNewConnection(Socket clientSocket)
        {
            try
            {
                NetworkStream networkStream = new NetworkStream(clientSocket, ownsSocket: false);
                SslStream sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false);

                //
                // Perform the TLS handshake (synchronous for simplicity)
                //
                sslStream.AuthenticateAsServer(_certificate);

                //
                // Create the connection with the SslStream
                //
                TcpClientConnection connection = CreateConnection(clientSocket, sslStream);

                //
                // Note: we can't use _connections directly since it's private in the base.
                // The connection's OnDisconnected callback handles cleanup.
                //
                connection.OnDisconnected = (conn) =>
                {
                    try { conn.Dispose(); } catch { }
                };

                connection.RunReceiveLoop();
            }
            catch
            {
                //
                // TLS handshake failed — close the socket
                //
                try { clientSocket.Close(); clientSocket.Dispose(); } catch { }
            }
        }
    }
}
