// ============================================================================
//
// TurnServer.cs — Top-level STUN/TURN server entry point.
//
// Wires together the UDP, TCP, and TLS listeners, allocation manager,
// authenticator, and request handlers.  Provides Start/Stop lifecycle
// methods and a background timer for cleaning up expired allocations.
//
// Usage:
//
//   var config = new TurnServerConfiguration { ... };
//   var server = new TurnServer(config);
//   server.Start();
//   // ...
//   server.Stop();
//   server.Dispose();
//
// ============================================================================

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Server.Handlers;

namespace Foundation.Networking.Coturn.Server
{
    /// <summary>
    ///
    /// Top-level STUN/TURN server.
    ///
    /// Orchestrates the UDP, TCP, and TLS listeners, allocation manager,
    /// authenticator, and handlers.  Start() binds the sockets, Stop()
    /// tears them down.
    ///
    /// </summary>
    public class TurnServer : IDisposable
    {
        private readonly TurnServerConfiguration _config;
        private readonly AllocationManager _allocationManager;
        private readonly TurnAuthenticator _authenticator;
        private readonly StunRequestHandler _stunHandler;
        private readonly TurnRequestHandler _turnHandler;
        private readonly TurnRelayHandler _relayHandler;

        //
        // Listeners
        //
        private readonly UdpTurnListener _udpListener;
        private TcpTurnListener _tcpListener;
        private TlsTurnListener _tlsListener;

        //
        // Background cleanup timer
        //
        private Timer _cleanupTimer;

        //
        // Whether the server is running
        //
        private bool _running = false;
        private bool _disposed = false;


        public TurnServer(TurnServerConfiguration config)
        {
            _config = config;

            //
            // Create the core components
            //
            _allocationManager = new AllocationManager(config);
            _authenticator = new TurnAuthenticator(config);
            _stunHandler = new StunRequestHandler(config);
            _turnHandler = new TurnRequestHandler(config, _allocationManager, _authenticator);
            _relayHandler = new TurnRelayHandler();

            //
            // Always create the UDP listener
            //
            _udpListener = new UdpTurnListener(config, _allocationManager, _stunHandler, _turnHandler, _relayHandler);
        }


        //
        // Public properties for testing and monitoring
        //

        /// <summary>
        /// The allocation manager for this server.
        /// </summary>
        public AllocationManager Allocations
        {
            get { return _allocationManager; }
        }


        /// <summary>
        /// The authenticator for this server.
        /// </summary>
        public TurnAuthenticator Authenticator
        {
            get { return _authenticator; }
        }


        /// <summary>
        /// Whether the server is currently running.
        /// </summary>
        public bool IsRunning
        {
            get { return _running; }
        }


        /// <summary>
        /// The UDP local endpoint the server is listening on.
        /// </summary>
        public IPEndPoint ListenEndPoint
        {
            get { return _udpListener.ListenEndPoint; }
        }


        /// <summary>
        /// The TCP listener, if enabled.
        /// </summary>
        public TcpTurnListener TcpListener
        {
            get { return _tcpListener; }
        }


        /// <summary>
        /// The TLS listener, if enabled.
        /// </summary>
        public TlsTurnListener TlsListener
        {
            get { return _tlsListener; }
        }


        // ── Lifecycle ─────────────────────────────────────────────────────


        /// <summary>
        /// Starts the TURN server — binds UDP (always), TCP (if enabled),
        /// and TLS (if enabled) sockets and starts the cleanup timer.
        /// </summary>
        public void Start()
        {
            if (_running)
            {
                return;
            }

            //
            // Start the UDP listener (always)
            //
            _udpListener.Start();

            //
            // Start the TCP listener if enabled
            //
            if (_config.TcpEnabled)
            {
                _tcpListener = new TcpTurnListener(
                    _config, _allocationManager, _stunHandler, _turnHandler, _relayHandler,
                    _config.TcpListenPort);

                _tcpListener.Start();
            }

            //
            // Start the TLS listener if enabled
            //
            if (_config.TlsEnabled)
            {
                _tlsListener = new TlsTurnListener(
                    _config, _allocationManager, _stunHandler, _turnHandler, _relayHandler);

                _tlsListener.Start();
            }

            //
            // Start the background cleanup timer
            //
            int cleanupInterval = _config.CleanupIntervalSeconds * 1000;

            _cleanupTimer = new Timer(
                CleanupCallback,
                null,
                cleanupInterval,
                cleanupInterval);

            _running = true;
        }


        /// <summary>
        /// Stops the TURN server — closes all sockets and stops the timer.
        /// </summary>
        public void Stop()
        {
            if (_running == false)
            {
                return;
            }

            _running = false;

            //
            // Stop the cleanup timer
            //
            if (_cleanupTimer != null)
            {
                _cleanupTimer.Dispose();
                _cleanupTimer = null;
            }

            //
            // Stop all listeners
            //
            _udpListener.Stop();

            if (_tcpListener != null)
            {
                _tcpListener.Stop();
            }

            if (_tlsListener != null)
            {
                _tlsListener.Stop();
            }
        }


        /// <summary>
        /// Sends data to a specific endpoint through the UDP listener's socket.
        /// Used for sending relay data back to clients.
        /// </summary>
        public void SendToClient(byte[] data, IPEndPoint clientEndPoint)
        {
            _udpListener.SendTo(data, clientEndPoint);
        }


        // ── Cleanup ───────────────────────────────────────────────────────


        private void CleanupCallback(object state)
        {
            try
            {
                _allocationManager.CleanupExpired();
                _authenticator.CleanupExpiredNonces();
            }
            catch
            {
                // Log in production — don't crash the timer
            }
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;

                Stop();

                _udpListener.Dispose();

                if (_tcpListener != null)
                {
                    _tcpListener.Dispose();
                    _tcpListener = null;
                }

                if (_tlsListener != null)
                {
                    _tlsListener.Dispose();
                    _tlsListener = null;
                }

                _allocationManager.Dispose();
            }
        }
    }
}
