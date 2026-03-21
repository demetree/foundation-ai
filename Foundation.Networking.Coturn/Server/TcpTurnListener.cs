// ============================================================================
//
// TcpTurnListener.cs — TCP accept loop for STUN/TURN-over-TCP.
//
// Binds a TCP socket to the configured address/port, accepts connections,
// and creates a TcpClientConnection for each.  Each connection runs its
// own receive loop on a ThreadPool thread.
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Protocol;
using Foundation.Networking.Coturn.Server.Handlers;

namespace Foundation.Networking.Coturn.Server
{
    /// <summary>
    ///
    /// TCP listener for STUN/TURN messages.
    ///
    /// Accepts TCP connections and creates a TcpClientConnection for each,
    /// which handles the 2-byte length framing and message dispatch.
    ///
    /// </summary>
    public class TcpTurnListener : IDisposable
    {
        private readonly TurnServerConfiguration _config;
        private readonly AllocationManager _allocationManager;
        private readonly StunRequestHandler _stunHandler;
        private readonly TurnRequestHandler _turnHandler;
        private readonly TurnRelayHandler _relayHandler;
        private readonly int _listenPort;

        //
        // Transport byte for FiveTuple — TCP = 6
        //
        protected readonly byte _transport;

        private Socket _listenSocket;
        private IPEndPoint _listenEndPoint;
        private volatile bool _running;
        private bool _disposed;

        //
        // Active connections tracked by connection ID
        //
        private readonly ConcurrentDictionary<string, TcpClientConnection> _connections;


        public TcpTurnListener(
            TurnServerConfiguration config,
            AllocationManager allocationManager,
            StunRequestHandler stunHandler,
            TurnRequestHandler turnHandler,
            TurnRelayHandler relayHandler,
            int listenPort,
            byte transport = StunConstants.TRANSPORT_TCP)
        {
            _config = config;
            _allocationManager = allocationManager;
            _stunHandler = stunHandler;
            _turnHandler = turnHandler;
            _relayHandler = relayHandler;
            _listenPort = listenPort;
            _transport = transport;
            _connections = new ConcurrentDictionary<string, TcpClientConnection>();
        }


        /// <summary>
        /// The local endpoint this listener is bound to.
        /// </summary>
        public IPEndPoint ListenEndPoint
        {
            get { return _listenEndPoint; }
        }


        /// <summary>
        /// Number of active connections.
        /// </summary>
        public int ConnectionCount
        {
            get { return _connections.Count; }
        }


        /// <summary>
        /// Starts the TCP listener.
        /// </summary>
        public virtual void Start()
        {
            _listenEndPoint = new IPEndPoint(IPAddress.Parse(_config.ListenAddress), _listenPort);

            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenSocket.Bind(_listenEndPoint);
            _listenSocket.Listen(128);

            _running = true;

            //
            // Start the accept loop on a background thread
            //
            ThreadPool.QueueUserWorkItem(_ => AcceptLoop());
        }


        /// <summary>
        /// Stops the listener and closes all connections.
        /// </summary>
        public void Stop()
        {
            _running = false;

            //
            // Close the listen socket to unblock Accept
            //
            if (_listenSocket != null)
            {
                try { _listenSocket.Close(); } catch { }
            }

            //
            // Close all active connections
            //
            foreach (var kvp in _connections)
            {
                try { kvp.Value.Dispose(); } catch { }
            }

            _connections.Clear();
        }


        // ── Accept Loop ───────────────────────────────────────────────────


        private void AcceptLoop()
        {
            while (_running)
            {
                try
                {
                    Socket clientSocket = _listenSocket.Accept();

                    //
                    // Set TCP keepalive and no-delay
                    //
                    clientSocket.NoDelay = true;
                    clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                    //
                    // Create the connection — subclasses can override to wrap with TLS
                    //
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        HandleNewConnection(clientSocket);
                    });
                }
                catch (ObjectDisposedException)
                {
                    // Listen socket was closed — stop
                    break;
                }
                catch (SocketException)
                {
                    // Accept failed — may be shutting down
                    if (_running == false) break;
                }
                catch
                {
                    // Unexpected — continue accepting
                    if (_running == false) break;
                }
            }
        }


        /// <summary>
        /// Handles a newly accepted connection.
        /// Virtual so TlsTurnListener can override to perform TLS handshake.
        /// </summary>
        protected virtual void HandleNewConnection(Socket clientSocket)
        {
            try
            {
                NetworkStream stream = new NetworkStream(clientSocket, ownsSocket: false);

                TcpClientConnection connection = CreateConnection(clientSocket, stream);

                if (_connections.TryAdd(connection.ConnectionId, connection))
                {
                    connection.OnDisconnected = OnConnectionDisconnected;
                    connection.RunReceiveLoop();
                }
                else
                {
                    connection.Dispose();
                }
            }
            catch
            {
                try { clientSocket.Close(); clientSocket.Dispose(); } catch { }
            }
        }


        /// <summary>
        /// Creates a TcpClientConnection for the given socket and stream.
        /// </summary>
        protected TcpClientConnection CreateConnection(Socket clientSocket, System.IO.Stream stream)
        {
            return new TcpClientConnection(
                clientSocket,
                stream,
                _listenEndPoint,
                _transport,
                _config,
                _allocationManager,
                _stunHandler,
                _turnHandler,
                _relayHandler);
        }


        // ── Connection Lifecycle ──────────────────────────────────────────


        private void OnConnectionDisconnected(TcpClientConnection connection)
        {
            _connections.TryRemove(connection.ConnectionId, out _);

            try { connection.Dispose(); } catch { }
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;
                Stop();

                if (_listenSocket != null)
                {
                    try { _listenSocket.Dispose(); } catch { }
                    _listenSocket = null;
                }
            }
        }
    }
}
