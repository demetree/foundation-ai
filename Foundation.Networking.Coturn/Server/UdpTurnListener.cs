// ============================================================================
//
// UdpTurnListener.cs — High-performance UDP listener for STUN/TURN traffic.
//
// Binds a UDP socket to the configured address/port and dispatches incoming
// datagrams to the appropriate handler based on the message type:
//
//   First 2 bits 00 → STUN/TURN message → StunRequestHandler or TurnRequestHandler
//   First 2 bits 01 → ChannelData       → TurnRelayHandler
//
// Uses SocketAsyncEventArgs for efficient async IO on the receive path.
//
// ============================================================================

using System;
using System.Buffers.Binary;
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
    /// UDP listener for STUN/TURN messages.
    ///
    /// Receives datagrams and dispatches to the correct handler based on
    /// whether the datagram is a STUN/TURN message or ChannelData.
    ///
    /// </summary>
    public class UdpTurnListener : IDisposable
    {
        //
        // Max UDP datagram size.  STUN messages are typically < 1500 bytes,
        // but relay data can be up to MTU.
        //
        private const int MAX_DATAGRAM_SIZE = 65535;

        private readonly TurnServerConfiguration _config;
        private readonly AllocationManager _allocationManager;
        private readonly StunRequestHandler _stunHandler;
        private readonly TurnRequestHandler _turnHandler;
        private readonly TurnRelayHandler _relayHandler;

        private Socket _socket;
        private IPEndPoint _listenEndPoint;
        private bool _running;
        private bool _disposed;

        //
        // Callback for relay data received from peers → to be sent back to clients.
        // This is set by TurnServer to wire up the peer relay receive loop.
        //
        public Action<TurnAllocation> OnRelaySocketReady { get; set; }


        public UdpTurnListener(
            TurnServerConfiguration config,
            AllocationManager allocationManager,
            StunRequestHandler stunHandler,
            TurnRequestHandler turnHandler,
            TurnRelayHandler relayHandler)
        {
            _config = config;
            _allocationManager = allocationManager;
            _stunHandler = stunHandler;
            _turnHandler = turnHandler;
            _relayHandler = relayHandler;
        }


        /// <summary>
        /// The local endpoint this listener is bound to.
        /// </summary>
        public IPEndPoint ListenEndPoint
        {
            get { return _listenEndPoint; }
        }


        /// <summary>
        /// Starts listening for UDP datagrams.
        /// </summary>
        public void Start()
        {
            _listenEndPoint = new IPEndPoint(IPAddress.Parse(_config.ListenAddress), _config.ListenPort);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(_listenEndPoint);

            _running = true;

            //
            // Start the async receive loop
            //
            StartReceive();
        }


        /// <summary>
        /// Stops the listener and closes the socket.
        /// </summary>
        public void Stop()
        {
            _running = false;

            if (_socket != null)
            {
                try
                {
                    _socket.Close();
                }
                catch
                {
                    // Best-effort
                }
            }
        }


        /// <summary>
        /// Sends a response datagram to the specified endpoint.
        /// </summary>
        public void SendTo(byte[] data, IPEndPoint endPoint)
        {
            if (_socket != null && _running && data != null)
            {
                try
                {
                    _socket.SendTo(data, endPoint);
                }
                catch
                {
                    // Best-effort
                }
            }
        }


        // ── Async Receive Loop ────────────────────────────────────────────


        private void StartReceive()
        {
            if (_running == false) return;

            SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
            receiveArgs.SetBuffer(new byte[MAX_DATAGRAM_SIZE], 0, MAX_DATAGRAM_SIZE);
            receiveArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            receiveArgs.Completed += OnReceiveCompleted;

            try
            {
                bool isPending = _socket.ReceiveFromAsync(receiveArgs);

                if (isPending == false)
                {
                    //
                    // Completed synchronously — process inline
                    //
                    ProcessReceive(receiveArgs);
                }
            }
            catch (ObjectDisposedException)
            {
                // Socket was closed — stop
            }
            catch
            {
                // Retry on next iteration
                if (_running)
                {
                    StartReceive();
                }
            }
        }


        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }


        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                IPEndPoint clientEndPoint = (IPEndPoint)e.RemoteEndPoint;
                byte[] datagram = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, 0, datagram, 0, e.BytesTransferred);

                //
                // Process on a ThreadPool thread to avoid blocking the receive loop
                //
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    ProcessDatagram(datagram, e.BytesTransferred, clientEndPoint);
                });
            }

            //
            // Restart the receive loop
            //
            e.Dispose();

            if (_running)
            {
                StartReceive();
            }
        }


        // ── Message Dispatch ──────────────────────────────────────────────


        private void ProcessDatagram(byte[] data, int length, IPEndPoint clientEndPoint)
        {
            try
            {
                //
                // Check if it's a ChannelData message (first 2 bits = 01)
                //
                if (TurnRelayHandler.IsChannelData(data, length))
                {
                    ProcessChannelData(data, length, clientEndPoint);
                    return;
                }

                //
                // Try to parse as a STUN/TURN message (first 2 bits = 00)
                //
                if (StunMessage.TryParse(data, out StunMessage message) == false)
                {
                    return;
                }

                //
                // Dispatch based on message type
                //
                ushort method = StunMessageType.GetMethod(message.MessageType);
                ushort messageClass = StunMessageType.GetClass(message.MessageType);

                //
                // Only process requests and indications, not responses
                //
                if (messageClass == StunMessageType.CLASS_SUCCESS_RESPONSE ||
                    messageClass == StunMessageType.CLASS_ERROR_RESPONSE)
                {
                    return;
                }

                byte[] response = null;

                if (method == StunMessageType.METHOD_BINDING)
                {
                    //
                    // STUN Binding Request
                    //
                    response = _stunHandler.HandleBindingRequest(message, clientEndPoint);
                }
                else if (method == StunMessageType.METHOD_SEND)
                {
                    //
                    // Send Indication (no response — it's an indication)
                    //
                    FiveTuple fiveTuple = new FiveTuple(clientEndPoint, _listenEndPoint, StunConstants.TRANSPORT_UDP);
                    TurnAllocation allocation = _allocationManager.FindAllocation(fiveTuple);
                    _relayHandler.HandleSendIndication(message, allocation);
                    return;
                }
                else
                {
                    //
                    // TURN request (Allocate, Refresh, CreatePermission, ChannelBind)
                    //
                    response = _turnHandler.HandleRequest(message, data, clientEndPoint, _listenEndPoint);
                }

                //
                // Send the response back to the client
                //
                if (response != null)
                {
                    SendTo(response, clientEndPoint);
                }
            }
            catch
            {
                // Log in production — silently drop for now
            }
        }


        private void ProcessChannelData(byte[] data, int length, IPEndPoint clientEndPoint)
        {
            FiveTuple fiveTuple = new FiveTuple(clientEndPoint, _listenEndPoint, StunConstants.TRANSPORT_UDP);
            TurnAllocation allocation = _allocationManager.FindAllocation(fiveTuple);

            _relayHandler.HandleChannelData(data, length, allocation);
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;
                Stop();

                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }
    }
}
