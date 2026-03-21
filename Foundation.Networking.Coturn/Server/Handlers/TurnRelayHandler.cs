// ============================================================================
//
// TurnRelayHandler.cs — Handles TURN data relay (Send/Data indications and
// ChannelData messages).
//
// Three relay paths:
//
//   1. Client → Server → Peer (Send Indication)
//      Client sends a STUN Send Indication with DATA + XOR-PEER-ADDRESS.
//      Server extracts data, checks permission, sends via relay socket.
//
//   2. Client → Server → Peer (ChannelData)
//      Client sends a 4-byte ChannelData header + payload.
//      Server looks up channel binding, sends via relay socket.
//
//   3. Peer → Server → Client (Data Indication / ChannelData)
//      Relay socket receives UDP from peer.  Server checks permission,
//      then sends either a STUN Data Indication or ChannelData back to client.
//
// ============================================================================

using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

using Foundation.Networking.Coturn.Protocol;

namespace Foundation.Networking.Coturn.Server.Handlers
{
    /// <summary>
    ///
    /// Handles TURN data relay — Send/Data indications and ChannelData messages.
    ///
    /// </summary>
    public class TurnRelayHandler
    {
        // ── Client → Peer via Send Indication ─────────────────────────────


        /// <summary>
        /// Handles a Send Indication from the client.
        ///
        /// Extracts DATA and XOR-PEER-ADDRESS, checks permission, and sends
        /// the data to the peer via the allocation's relay socket.
        ///
        /// Send Indications are not authenticated (they are indications, not requests),
        /// but the client must have an existing allocation and permission.
        /// </summary>
        public void HandleSendIndication(StunMessage message, TurnAllocation allocation)
        {
            if (allocation == null)
            {
                return;
            }

            //
            // Extract XOR-PEER-ADDRESS
            //
            StunAttribute peerAttr = message.FindAttribute(StunAttributeType.XOR_PEER_ADDRESS);

            if (peerAttr == null)
            {
                return;
            }

            IPEndPoint peerEndPoint = StunAttribute.ParseXorMappedAddress(peerAttr.Value, message.TransactionId);

            if (peerEndPoint == null)
            {
                return;
            }

            //
            // Check permission
            //
            if (allocation.HasPermission(peerEndPoint.Address) == false)
            {
                return;
            }

            //
            // Extract DATA attribute
            //
            StunAttribute dataAttr = message.FindAttribute(StunAttributeType.DATA);

            if (dataAttr == null || dataAttr.Value == null || dataAttr.Value.Length == 0)
            {
                return;
            }

            //
            // Send via the relay socket
            //
            try
            {
                allocation.RelaySocket.SendTo(dataAttr.Value, peerEndPoint);
            }
            catch
            {
                // Best-effort — peer may be unreachable
            }
        }


        // ── Client → Peer via ChannelData ─────────────────────────────────


        /// <summary>
        /// ChannelData header layout:
        ///   [0-1] Channel number (0x4000–0x7FFE)
        ///   [2-3] Data length
        ///   [4+]  Application data
        /// </summary>
        public const int CHANNEL_DATA_HEADER_SIZE = 4;


        /// <summary>
        /// Handles a ChannelData message from the client.
        ///
        /// Extracts the channel number and payload, looks up the channel binding,
        /// checks permission, and sends data to the peer.
        /// </summary>
        public void HandleChannelData(byte[] data, int dataLength, TurnAllocation allocation)
        {
            if (allocation == null || data == null || dataLength < CHANNEL_DATA_HEADER_SIZE)
            {
                return;
            }

            //
            // Parse the ChannelData header
            //
            ushort channelNumber = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(0, 2));
            ushort payloadLength = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(2, 2));

            if (dataLength < CHANNEL_DATA_HEADER_SIZE + payloadLength)
            {
                return;
            }

            //
            // Look up the channel binding
            //
            TurnChannel channel = allocation.FindChannel(channelNumber);

            if (channel == null)
            {
                return;
            }

            //
            // Check permission
            //
            if (allocation.HasPermission(channel.PeerEndPoint.Address) == false)
            {
                return;
            }

            //
            // Send the payload to the peer
            //
            try
            {
                allocation.RelaySocket.SendTo(data, CHANNEL_DATA_HEADER_SIZE, payloadLength, SocketFlags.None, channel.PeerEndPoint);
            }
            catch
            {
                // Best-effort
            }
        }


        // ── Peer → Client ─────────────────────────────────────────────────


        /// <summary>
        /// Handles data received from a peer on the relay socket.
        ///
        /// Checks permission, then sends a Data Indication or ChannelData
        /// message back to the client.
        ///
        /// Returns the bytes to send back to the client, or null if dropped.
        /// </summary>
        public byte[] HandlePeerData(byte[] peerData, int peerDataLength, IPEndPoint peerEndPoint, TurnAllocation allocation)
        {
            if (allocation == null || peerData == null || peerDataLength == 0)
            {
                return null;
            }

            //
            // Check permission
            //
            if (allocation.HasPermission(peerEndPoint.Address) == false)
            {
                return null;
            }

            //
            // Check if there's a channel binding for this peer
            //
            ushort channelNumber = allocation.FindChannelForPeer(peerEndPoint);

            if (channelNumber != 0)
            {
                //
                // Send as ChannelData (more compact)
                //
                return BuildChannelData(channelNumber, peerData, peerDataLength);
            }
            else
            {
                //
                // Send as Data Indication
                //
                return BuildDataIndication(peerData, peerDataLength, peerEndPoint, allocation);
            }
        }


        // ── Builders ──────────────────────────────────────────────────────


        /// <summary>
        /// Builds a ChannelData message to send to the client.
        /// </summary>
        public static byte[] BuildChannelData(ushort channelNumber, byte[] payload, int payloadLength)
        {
            //
            // ChannelData: 4-byte header + payload (padded to 4-byte boundary for UDP)
            //
            int paddedLength = (payloadLength + 3) & ~3;
            byte[] channelData = new byte[CHANNEL_DATA_HEADER_SIZE + paddedLength];

            BinaryPrimitives.WriteUInt16BigEndian(channelData.AsSpan(0), channelNumber);
            BinaryPrimitives.WriteUInt16BigEndian(channelData.AsSpan(2), (ushort)payloadLength);

            Array.Copy(payload, 0, channelData, CHANNEL_DATA_HEADER_SIZE, payloadLength);

            return channelData;
        }


        /// <summary>
        /// Builds a STUN Data Indication to send to the client.
        /// </summary>
        private byte[] BuildDataIndication(byte[] peerData, int peerDataLength, IPEndPoint peerEndPoint, TurnAllocation allocation)
        {
            StunMessage indication = new StunMessage();
            indication.MessageType = StunMessageType.DATA_INDICATION;
            indication.TransactionId = new byte[StunConstants.TRANSACTION_ID_SIZE];

            //
            // Generate a random transaction ID for the indication
            //
            using (System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(indication.TransactionId);
            }

            //
            // XOR-PEER-ADDRESS — the peer that sent the data
            //
            indication.AddAttribute(
                StunAttribute.CreateXorPeerAddress(peerEndPoint, indication.TransactionId));

            //
            // DATA — the payload from the peer
            //
            byte[] dataValue = new byte[peerDataLength];
            Array.Copy(peerData, dataValue, peerDataLength);

            indication.AddAttribute(new StunAttribute
            {
                Type = StunAttributeType.DATA,
                Value = dataValue
            });

            return indication.Encode();
        }


        // ── Helpers ───────────────────────────────────────────────────────


        /// <summary>
        /// Checks if a received datagram is a ChannelData message (first 2 bits are 01).
        /// STUN messages have first 2 bits = 00.
        /// </summary>
        public static bool IsChannelData(byte[] data, int length)
        {
            if (data == null || length < CHANNEL_DATA_HEADER_SIZE)
            {
                return false;
            }

            //
            // ChannelData: channel number starts with 0x40–0x7F (bits 01xxxxxx)
            // STUN messages: first 2 bits are 00
            //
            ushort firstTwoBytes = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(0, 2));

            return firstTwoBytes >= TurnChannel.MIN_CHANNEL_NUMBER && firstTwoBytes <= TurnChannel.MAX_CHANNEL_NUMBER;
        }
    }
}
