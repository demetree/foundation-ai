// ============================================================================
//
// TcpClientConnection.cs — Per-connection handler for STUN/TURN over TCP.
//
// STUN over TCP uses a 2-byte big-endian length prefix before each message
// (RFC 5389 §7.2.2).  This framing is necessary because TCP is a stream
// protocol — unlike UDP, there are no datagram boundaries.
//
// Wire format:
//   [2 bytes] Message length (big-endian)
//   [N bytes] STUN/TURN message or ChannelData
//
// Each TcpClientConnection wraps a NetworkStream (or SslStream for TLS)
// and runs a receive loop that reads framed messages and dispatches them
// to the same handlers used by the UDP listener.
//
// ============================================================================

using System;
using System.Buffers.Binary;
using System.IO;
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
    /// Manages a single TCP client connection with 2-byte length framing.
    ///
    /// </summary>
    public class TcpClientConnection : IDisposable
    {
        //
        // Max STUN message size over TCP (same as UDP max)
        //
        private const int MAX_MESSAGE_SIZE = 65535;

        //
        // 2-byte length prefix
        //
        public const int FRAME_HEADER_SIZE = 2;

        private readonly Socket _socket;
        private readonly Stream _stream;
        private readonly IPEndPoint _clientEndPoint;
        private readonly IPEndPoint _serverEndPoint;
        private readonly byte _transport;
        private readonly TurnServerConfiguration _config;
        private readonly AllocationManager _allocationManager;
        private readonly StunRequestHandler _stunHandler;
        private readonly TurnRequestHandler _turnHandler;
        private readonly TurnRelayHandler _relayHandler;

        //
        // Unique connection ID for tracking
        //
        public string ConnectionId { get; set; }

        //
        // Whether this connection is still active
        //
        private volatile bool _running;
        private bool _disposed;

        //
        // Callback when this connection closes (for cleanup by the listener)
        //
        public Action<TcpClientConnection> OnDisconnected { get; set; }


        public TcpClientConnection(
            Socket socket,
            Stream stream,
            IPEndPoint serverEndPoint,
            byte transport,
            TurnServerConfiguration config,
            AllocationManager allocationManager,
            StunRequestHandler stunHandler,
            TurnRequestHandler turnHandler,
            TurnRelayHandler relayHandler)
        {
            _socket = socket;
            _stream = stream;
            _clientEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            _serverEndPoint = serverEndPoint;
            _transport = transport;
            _config = config;
            _allocationManager = allocationManager;
            _stunHandler = stunHandler;
            _turnHandler = turnHandler;
            _relayHandler = relayHandler;

            ConnectionId = Guid.NewGuid().ToString("N").Substring(0, 8);
            _running = true;
        }


        /// <summary>
        /// The client's remote endpoint.
        /// </summary>
        public IPEndPoint ClientEndPoint
        {
            get { return _clientEndPoint; }
        }


        /// <summary>
        /// The 5-tuple for this connection.
        /// </summary>
        public FiveTuple FiveTuple
        {
            get { return new FiveTuple(_clientEndPoint, _serverEndPoint, _transport); }
        }


        // ── Receive Loop ──────────────────────────────────────────────────


        /// <summary>
        /// Starts the receive loop on the current thread.
        /// Call this from a ThreadPool work item.
        /// </summary>
        public void RunReceiveLoop()
        {
            try
            {
                byte[] headerBuffer = new byte[FRAME_HEADER_SIZE];

                while (_running)
                {
                    //
                    // Read the 2-byte length prefix
                    //
                    if (ReadExact(headerBuffer, 0, FRAME_HEADER_SIZE) == false)
                    {
                        break;
                    }

                    ushort messageLength = BinaryPrimitives.ReadUInt16BigEndian(headerBuffer);

                    if (messageLength == 0 || messageLength > MAX_MESSAGE_SIZE)
                    {
                        break;
                    }

                    //
                    // Read the full message
                    //
                    byte[] messageBuffer = new byte[messageLength];

                    if (ReadExact(messageBuffer, 0, messageLength) == false)
                    {
                        break;
                    }

                    //
                    // Dispatch the message
                    //
                    ProcessMessage(messageBuffer, messageLength);
                }
            }
            catch (IOException)
            {
                // Connection closed or broken
            }
            catch (ObjectDisposedException)
            {
                // Socket was disposed
            }
            catch
            {
                // Unexpected error — close connection
            }
            finally
            {
                //
                // Connection is done — clean up allocations for this 5-tuple
                //
                CleanupOnDisconnect();

                _running = false;
                OnDisconnected?.Invoke(this);
            }
        }


        // ── Send ──────────────────────────────────────────────────────────


        /// <summary>
        /// Sends a framed message (2-byte length prefix + message bytes).
        /// Thread-safe via lock on the stream.
        /// </summary>
        public void Send(byte[] messageBytes)
        {
            if (_running == false || messageBytes == null || messageBytes.Length == 0)
            {
                return;
            }

            try
            {
                byte[] frame = new byte[FRAME_HEADER_SIZE + messageBytes.Length];

                BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(0), (ushort)messageBytes.Length);
                Array.Copy(messageBytes, 0, frame, FRAME_HEADER_SIZE, messageBytes.Length);

                lock (_stream)
                {
                    _stream.Write(frame, 0, frame.Length);
                    _stream.Flush();
                }
            }
            catch
            {
                // Connection may have closed — best effort
                _running = false;
            }
        }


        // ── Message Dispatch ──────────────────────────────────────────────


        private void ProcessMessage(byte[] data, int length)
        {
            try
            {
                //
                // Check if it's a ChannelData message (first 2 bits = 01)
                //
                if (TurnRelayHandler.IsChannelData(data, length))
                {
                    FiveTuple ft = FiveTuple;
                    TurnAllocation allocation = _allocationManager.FindAllocation(ft);
                    _relayHandler.HandleChannelData(data, length, allocation);
                    return;
                }

                //
                // Try to parse as STUN/TURN message
                //
                if (StunMessage.TryParse(data, out StunMessage message) == false)
                {
                    return;
                }

                ushort method = StunMessageType.GetMethod(message.MessageType);
                ushort messageClass = StunMessageType.GetClass(message.MessageType);

                //
                // Ignore responses
                //
                if (messageClass == StunMessageType.CLASS_SUCCESS_RESPONSE ||
                    messageClass == StunMessageType.CLASS_ERROR_RESPONSE)
                {
                    return;
                }

                byte[] response = null;

                if (method == StunMessageType.METHOD_BINDING)
                {
                    response = _stunHandler.HandleBindingRequest(message, _clientEndPoint);
                }
                else if (method == StunMessageType.METHOD_SEND)
                {
                    FiveTuple ft = FiveTuple;
                    TurnAllocation allocation = _allocationManager.FindAllocation(ft);
                    _relayHandler.HandleSendIndication(message, allocation);
                    return;
                }
                else
                {
                    response = _turnHandler.HandleRequest(message, data, _clientEndPoint, _serverEndPoint);
                }

                if (response != null)
                {
                    Send(response);
                }
            }
            catch
            {
                // Drop malformed messages
            }
        }


        // ── Helpers ───────────────────────────────────────────────────────


        /// <summary>
        /// Reads exactly 'count' bytes from the stream, handling partial reads.
        /// Returns false if the connection closed before all bytes were read.
        /// </summary>
        private bool ReadExact(byte[] buffer, int offset, int count)
        {
            int totalRead = 0;

            while (totalRead < count)
            {
                int bytesRead = _stream.Read(buffer, offset + totalRead, count - totalRead);

                if (bytesRead == 0)
                {
                    //
                    // Connection closed
                    //
                    return false;
                }

                totalRead += bytesRead;
            }

            return true;
        }


        /// <summary>
        /// Cleans up any allocations associated with this TCP connection.
        /// When a TCP connection drops, all allocations on that 5-tuple are invalid.
        /// </summary>
        private void CleanupOnDisconnect()
        {
            try
            {
                FiveTuple ft = FiveTuple;
                TurnAllocation allocation = _allocationManager.FindAllocation(ft);

                if (allocation != null)
                {
                    _allocationManager.RemoveAllocation(ft);
                }
            }
            catch
            {
                // Best effort
            }
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;
                _running = false;

                try { _stream.Dispose(); } catch { }
                try { _socket.Close(); _socket.Dispose(); } catch { }
            }
        }
    }
}
