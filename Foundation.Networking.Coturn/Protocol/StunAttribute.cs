// ============================================================================
//
// StunAttribute.cs — STUN message attribute (TLV: Type-Length-Value).
//
// Every STUN attribute is a TLV block that follows the 20-byte message header.
// The attribute header is 4 bytes (2-byte type + 2-byte length), followed by
// the value, padded to a 4-byte boundary.
//
// This class provides the in-memory representation and factory methods for
// common attribute types, plus address parsing for MAPPED-ADDRESS and
// XOR-MAPPED-ADDRESS.
//
// ============================================================================

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Foundation.Networking.Coturn.Protocol
{
    /// <summary>
    ///
    /// Represents a single STUN message attribute in TLV format.
    ///
    /// Layout on the wire:
    ///   [Type: 2 bytes] [Length: 2 bytes] [Value: variable] [Padding: 0-3 bytes]
    ///
    /// The Length field contains the value length before padding.
    ///
    /// </summary>
    public class StunAttribute
    {
        //
        // The attribute type code (e.g., StunAttributeType.USERNAME)
        //
        public ushort Type { get; set; }

        //
        // The raw attribute value bytes (no padding)
        //
        public byte[] Value { get; set; }


        /// <summary>
        /// Returns the total encoded size on the wire: 4-byte header + value + padding.
        /// </summary>
        public int EncodedLength
        {
            get
            {
                int valueLength = (Value != null) ? Value.Length : 0;
                int paddedLength = (valueLength + 3) & ~3;

                return StunConstants.ATTRIBUTE_HEADER_SIZE + paddedLength;
            }
        }


        // ── Factory Methods for Common Attributes ─────────────────────────


        /// <summary>
        /// Creates a USERNAME attribute from the given string.
        /// </summary>
        public static StunAttribute CreateUsername(string username)
        {
            return new StunAttribute
            {
                Type = StunAttributeType.USERNAME,
                Value = Encoding.UTF8.GetBytes(username)
            };
        }


        /// <summary>
        /// Creates a REALM attribute from the given string.
        /// </summary>
        public static StunAttribute CreateRealm(string realm)
        {
            return new StunAttribute
            {
                Type = StunAttributeType.REALM,
                Value = Encoding.UTF8.GetBytes(realm)
            };
        }


        /// <summary>
        /// Creates a NONCE attribute from the given string.
        /// </summary>
        public static StunAttribute CreateNonce(string nonce)
        {
            return new StunAttribute
            {
                Type = StunAttributeType.NONCE,
                Value = Encoding.UTF8.GetBytes(nonce)
            };
        }


        /// <summary>
        /// Creates a SOFTWARE attribute with the given software description.
        /// </summary>
        public static StunAttribute CreateSoftware(string software)
        {
            return new StunAttribute
            {
                Type = StunAttributeType.SOFTWARE,
                Value = Encoding.UTF8.GetBytes(software)
            };
        }


        /// <summary>
        /// Creates a LIFETIME attribute with the given seconds value.
        /// </summary>
        public static StunAttribute CreateLifetime(uint seconds)
        {
            byte[] value = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(value, seconds);

            return new StunAttribute
            {
                Type = StunAttributeType.LIFETIME,
                Value = value
            };
        }


        /// <summary>
        /// Creates a REQUESTED-TRANSPORT attribute for the given protocol number.
        /// </summary>
        public static StunAttribute CreateRequestedTransport(byte protocol)
        {
            byte[] value = new byte[4];
            value[0] = protocol;

            // bytes 1-3 are reserved (zero)

            return new StunAttribute
            {
                Type = StunAttributeType.REQUESTED_TRANSPORT,
                Value = value
            };
        }


        /// <summary>
        /// Creates a CHANNEL-NUMBER attribute.
        /// </summary>
        public static StunAttribute CreateChannelNumber(ushort channelNumber)
        {
            byte[] value = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(value, channelNumber);

            // bytes 2-3 are reserved (zero)

            return new StunAttribute
            {
                Type = StunAttributeType.CHANNEL_NUMBER,
                Value = value
            };
        }


        /// <summary>
        /// Creates a XOR-MAPPED-ADDRESS attribute for the given endpoint.
        /// </summary>
        public static StunAttribute CreateXorMappedAddress(IPEndPoint endPoint, byte[] transactionId)
        {
            byte[] value = EncodeXorAddress(endPoint, transactionId);

            return new StunAttribute
            {
                Type = StunAttributeType.XOR_MAPPED_ADDRESS,
                Value = value
            };
        }


        /// <summary>
        /// Creates a XOR-PEER-ADDRESS attribute for the given endpoint.
        /// </summary>
        public static StunAttribute CreateXorPeerAddress(IPEndPoint endPoint, byte[] transactionId)
        {
            byte[] value = EncodeXorAddress(endPoint, transactionId);

            return new StunAttribute
            {
                Type = StunAttributeType.XOR_PEER_ADDRESS,
                Value = value
            };
        }


        /// <summary>
        /// Creates a XOR-RELAYED-ADDRESS attribute for the given endpoint.
        /// </summary>
        public static StunAttribute CreateXorRelayedAddress(IPEndPoint endPoint, byte[] transactionId)
        {
            byte[] value = EncodeXorAddress(endPoint, transactionId);

            return new StunAttribute
            {
                Type = StunAttributeType.XOR_RELAYED_ADDRESS,
                Value = value
            };
        }


        // ── Address Parsing ───────────────────────────────────────────────


        //
        // Address family constants
        //
        private const byte ADDRESS_FAMILY_IPV4 = 0x01;
        private const byte ADDRESS_FAMILY_IPV6 = 0x02;


        /// <summary>
        /// Parses a MAPPED-ADDRESS attribute value into an IPEndPoint.
        /// </summary>
        public static IPEndPoint ParseMappedAddress(byte[] value)
        {
            if (value == null || value.Length < 8)
            {
                return null;
            }

            //
            // Layout: [0]=reserved, [1]=family, [2-3]=port, [4+]=address
            //
            byte family = value[1];
            ushort port = BinaryPrimitives.ReadUInt16BigEndian(value.AsSpan(2, 2));

            if (family == ADDRESS_FAMILY_IPV4 && value.Length >= 8)
            {
                byte[] addressBytes = new byte[4];
                Array.Copy(value, 4, addressBytes, 0, 4);

                return new IPEndPoint(new IPAddress(addressBytes), port);
            }
            else if (family == ADDRESS_FAMILY_IPV6 && value.Length >= 20)
            {
                byte[] addressBytes = new byte[16];
                Array.Copy(value, 4, addressBytes, 0, 16);

                return new IPEndPoint(new IPAddress(addressBytes), port);
            }

            return null;
        }


        /// <summary>
        /// Parses a XOR-MAPPED-ADDRESS (or XOR-PEER-ADDRESS / XOR-RELAYED-ADDRESS) 
        /// attribute value into an IPEndPoint.
        ///
        /// The address and port are XOR'd with the magic cookie and transaction ID.
        /// </summary>
        public static IPEndPoint ParseXorMappedAddress(byte[] value, byte[] transactionId)
        {
            if (value == null || value.Length < 8 || transactionId == null || transactionId.Length < 12)
            {
                return null;
            }

            //
            // Layout: [0]=reserved, [1]=family, [2-3]=xor-port, [4+]=xor-address
            //
            byte family = value[1];

            //
            // Port is XOR'd with the most significant 16 bits of the magic cookie
            //
            ushort xorPort = BinaryPrimitives.ReadUInt16BigEndian(value.AsSpan(2, 2));
            ushort port = (ushort)(xorPort ^ (ushort)(StunConstants.MAGIC_COOKIE >> 16));

            if (family == ADDRESS_FAMILY_IPV4 && value.Length >= 8)
            {
                //
                // IPv4: 4-byte address XOR'd with the magic cookie
                //
                uint xorAddress = BinaryPrimitives.ReadUInt32BigEndian(value.AsSpan(4, 4));
                uint address = xorAddress ^ StunConstants.MAGIC_COOKIE;

                byte[] addressBytes = new byte[4];
                BinaryPrimitives.WriteUInt32BigEndian(addressBytes, address);

                return new IPEndPoint(new IPAddress(addressBytes), port);
            }
            else if (family == ADDRESS_FAMILY_IPV6 && value.Length >= 20)
            {
                //
                // IPv6: 16-byte address XOR'd with magic cookie (4 bytes) + transaction ID (12 bytes)
                //
                byte[] addressBytes = new byte[16];

                byte[] magicCookieBytes = new byte[4];
                BinaryPrimitives.WriteUInt32BigEndian(magicCookieBytes, StunConstants.MAGIC_COOKIE);

                for (int byteIndex = 0; byteIndex < 4; byteIndex++)
                {
                    addressBytes[byteIndex] = (byte)(value[4 + byteIndex] ^ magicCookieBytes[byteIndex]);
                }

                for (int byteIndex = 0; byteIndex < 12; byteIndex++)
                {
                    addressBytes[4 + byteIndex] = (byte)(value[8 + byteIndex] ^ transactionId[byteIndex]);
                }

                return new IPEndPoint(new IPAddress(addressBytes), port);
            }

            return null;
        }


        /// <summary>
        /// Reads a string value from the attribute's raw bytes using UTF-8.
        /// </summary>
        public string ReadString()
        {
            if (Value == null || Value.Length == 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(Value);
        }


        /// <summary>
        /// Reads a uint32 value from the attribute's raw bytes (big-endian).
        /// </summary>
        public uint ReadUInt32()
        {
            if (Value == null || Value.Length < 4)
            {
                return 0;
            }

            return BinaryPrimitives.ReadUInt32BigEndian(Value);
        }


        // ── Private Helpers ───────────────────────────────────────────────


        /// <summary>
        /// Encodes an IPEndPoint as a XOR'd address value (used by XOR-MAPPED-ADDRESS,
        /// XOR-PEER-ADDRESS, and XOR-RELAYED-ADDRESS).
        /// </summary>
        private static byte[] EncodeXorAddress(IPEndPoint endPoint, byte[] transactionId)
        {
            byte[] addressBytes = endPoint.Address.GetAddressBytes();
            bool isIPv6 = (addressBytes.Length == 16);

            //
            // Value layout: [0]=reserved, [1]=family, [2-3]=xor-port, [4+]=xor-address
            //
            int valueLength = isIPv6 ? 20 : 8;
            byte[] value = new byte[valueLength];

            value[0] = 0;
            value[1] = isIPv6 ? ADDRESS_FAMILY_IPV6 : ADDRESS_FAMILY_IPV4;

            //
            // XOR the port with the most significant 16 bits of the magic cookie
            //
            ushort xorPort = (ushort)(endPoint.Port ^ (ushort)(StunConstants.MAGIC_COOKIE >> 16));
            BinaryPrimitives.WriteUInt16BigEndian(value.AsSpan(2), xorPort);

            if (isIPv6 == false)
            {
                //
                // IPv4: XOR with magic cookie
                //
                uint addressInt = BinaryPrimitives.ReadUInt32BigEndian(addressBytes);
                uint xorAddress = addressInt ^ StunConstants.MAGIC_COOKIE;
                BinaryPrimitives.WriteUInt32BigEndian(value.AsSpan(4), xorAddress);
            }
            else
            {
                //
                // IPv6: XOR with magic cookie (4 bytes) + transaction ID (12 bytes)
                //
                byte[] magicCookieBytes = new byte[4];
                BinaryPrimitives.WriteUInt32BigEndian(magicCookieBytes, StunConstants.MAGIC_COOKIE);

                for (int byteIndex = 0; byteIndex < 4; byteIndex++)
                {
                    value[4 + byteIndex] = (byte)(addressBytes[byteIndex] ^ magicCookieBytes[byteIndex]);
                }

                for (int byteIndex = 0; byteIndex < 12; byteIndex++)
                {
                    value[8 + byteIndex] = (byte)(addressBytes[4 + byteIndex] ^ transactionId[byteIndex]);
                }
            }

            return value;
        }
    }
}
