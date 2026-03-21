// ============================================================================
//
// StunMessage.cs — STUN message parser and encoder.
//
// A STUN message consists of a 20-byte header followed by zero or more TLV
// attributes.  The header layout is:
//
//   [0-1]   Message Type (method + class, 14 bits)
//   [2-3]   Message Length (bytes after header, excludes the 20-byte header)
//   [4-7]   Magic Cookie (0x2112A442)
//   [8-19]  Transaction ID (96 bits)
//
// This class handles both parsing raw bytes into a StunMessage object and
// encoding a StunMessage back to raw bytes.
//
// ============================================================================

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Foundation.Networking.Coturn.Protocol
{
    /// <summary>
    ///
    /// Represents a complete STUN message — header plus attributes.
    ///
    /// Use TryParse() to read from raw bytes, and Encode() to write to bytes.
    ///
    /// </summary>
    public class StunMessage
    {
        //
        // The 14-bit message type (method + class combined)
        //
        public ushort MessageType { get; set; }

        //
        // The 96-bit transaction ID (12 bytes)
        //
        public byte[] TransactionId { get; set; }

        //
        // Ordered list of attributes in this message
        //
        public List<StunAttribute> Attributes { get; set; }


        //
        // Constructor
        //
        public StunMessage()
        {
            Attributes = new List<StunAttribute>();
            TransactionId = new byte[StunConstants.TRANSACTION_ID_SIZE];
        }


        // ── Factory Methods ───────────────────────────────────────────────


        /// <summary>
        /// Creates a new STUN Binding Request with a random transaction ID.
        /// </summary>
        public static StunMessage CreateBindingRequest()
        {
            StunMessage message = new StunMessage();
            message.MessageType = StunMessageType.BINDING_REQUEST;
            message.TransactionId = GenerateTransactionId();

            return message;
        }


        /// <summary>
        /// Creates a new STUN Binding Success Response for the given request.
        /// </summary>
        public static StunMessage CreateBindingSuccessResponse(StunMessage request)
        {
            StunMessage message = new StunMessage();
            message.MessageType = StunMessageType.BINDING_SUCCESS_RESPONSE;

            //
            // Copy the transaction ID from the request
            //
            message.TransactionId = new byte[StunConstants.TRANSACTION_ID_SIZE];
            Array.Copy(request.TransactionId, message.TransactionId, StunConstants.TRANSACTION_ID_SIZE);

            return message;
        }


        /// <summary>
        /// Creates a new error response for the given request with the specified error code.
        /// </summary>
        public static StunMessage CreateErrorResponse(StunMessage request, int errorCode, string reason)
        {
            ushort method = StunMessageType.GetMethod(request.MessageType);
            ushort errorType = StunMessageType.Compose(method, StunMessageType.CLASS_ERROR_RESPONSE);

            StunMessage message = new StunMessage();
            message.MessageType = errorType;

            //
            // Copy the transaction ID from the request
            //
            message.TransactionId = new byte[StunConstants.TRANSACTION_ID_SIZE];
            Array.Copy(request.TransactionId, message.TransactionId, StunConstants.TRANSACTION_ID_SIZE);

            //
            // Add the ERROR-CODE attribute
            //
            message.AddErrorCode(errorCode, reason);

            return message;
        }


        // ── Attribute Helpers ─────────────────────────────────────────────


        /// <summary>
        /// Adds an attribute to the message.
        /// </summary>
        public void AddAttribute(StunAttribute attribute)
        {
            Attributes.Add(attribute);
        }


        /// <summary>
        /// Finds the first attribute with the given type, or null if not present.
        /// </summary>
        public StunAttribute FindAttribute(ushort attributeType)
        {
            for (int index = 0; index < Attributes.Count; index++)
            {
                if (Attributes[index].Type == attributeType)
                {
                    return Attributes[index];
                }
            }

            return null;
        }


        /// <summary>
        /// Finds all attributes with the given type.
        /// </summary>
        public List<StunAttribute> FindAllAttributes(ushort attributeType)
        {
            List<StunAttribute> resultList = new List<StunAttribute>();

            for (int index = 0; index < Attributes.Count; index++)
            {
                if (Attributes[index].Type == attributeType)
                {
                    resultList.Add(Attributes[index]);
                }
            }

            return resultList;
        }


        /// <summary>
        /// Adds an ERROR-CODE attribute per RFC 5389 §15.6.
        ///
        /// Layout: [0-1]=reserved, [2]=class (hundreds digit), [3]=number (0-99), [4+]=reason phrase
        /// </summary>
        public void AddErrorCode(int errorCode, string reason)
        {
            int errorClass = errorCode / 100;
            int errorNumber = errorCode % 100;

            byte[] reasonBytes = System.Text.Encoding.UTF8.GetBytes(reason ?? string.Empty);
            byte[] value = new byte[4 + reasonBytes.Length];

            value[0] = 0;
            value[1] = 0;
            value[2] = (byte)(errorClass & 0x07);
            value[3] = (byte)(errorNumber & 0xFF);

            if (reasonBytes.Length > 0)
            {
                Array.Copy(reasonBytes, 0, value, 4, reasonBytes.Length);
            }

            StunAttribute attribute = new StunAttribute
            {
                Type = StunAttributeType.ERROR_CODE,
                Value = value
            };

            Attributes.Add(attribute);
        }


        // ── Parsing ───────────────────────────────────────────────────────


        /// <summary>
        /// Attempts to parse a STUN message from raw bytes.
        ///
        /// Returns true if the message was successfully parsed, false otherwise.
        /// On failure, the out parameter is set to null.
        /// </summary>
        public static bool TryParse(ReadOnlySpan<byte> data, out StunMessage message)
        {
            message = null;

            //
            // Minimum size check: need at least the 20-byte header
            //
            if (data.Length < StunConstants.HEADER_SIZE)
            {
                return false;
            }

            //
            // Read the header fields
            //
            ushort messageType = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
            ushort messageLength = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
            uint magicCookie = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(4, 4));

            //
            // Validate the magic cookie
            //
            if (magicCookie != StunConstants.MAGIC_COOKIE)
            {
                return false;
            }

            //
            // The first two bits of the message type must be zero (RFC 5389 §6)
            //
            if ((messageType & 0xC000) != 0)
            {
                return false;
            }

            //
            // The message length must be a multiple of 4
            //
            if ((messageLength % 4) != 0)
            {
                return false;
            }

            //
            // Make sure we have enough data for the declared message length
            //
            if (data.Length < StunConstants.HEADER_SIZE + messageLength)
            {
                return false;
            }

            //
            // Read the transaction ID (12 bytes)
            //
            byte[] transactionId = new byte[StunConstants.TRANSACTION_ID_SIZE];
            data.Slice(8, StunConstants.TRANSACTION_ID_SIZE).CopyTo(transactionId);

            //
            // Parse attributes from the body
            //
            List<StunAttribute> attributeList = new List<StunAttribute>();
            int offset = StunConstants.HEADER_SIZE;
            int endOffset = StunConstants.HEADER_SIZE + messageLength;

            while (offset + StunConstants.ATTRIBUTE_HEADER_SIZE <= endOffset)
            {
                //
                // Read attribute type and length
                //
                ushort attrType = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset, 2));
                ushort attrLength = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset + 2, 2));

                //
                // Make sure the attribute value fits within the message
                //
                if (offset + StunConstants.ATTRIBUTE_HEADER_SIZE + attrLength > endOffset)
                {
                    return false;
                }

                //
                // Copy the attribute value
                //
                byte[] attrValue = new byte[attrLength];
                data.Slice(offset + StunConstants.ATTRIBUTE_HEADER_SIZE, attrLength).CopyTo(attrValue);

                attributeList.Add(new StunAttribute
                {
                    Type = attrType,
                    Value = attrValue
                });

                //
                // Advance past this attribute, including padding to 4-byte boundary
                //
                int paddedLength = (attrLength + 3) & ~3;
                offset += StunConstants.ATTRIBUTE_HEADER_SIZE + paddedLength;
            }

            //
            // Build the message
            //
            message = new StunMessage
            {
                MessageType = messageType,
                TransactionId = transactionId,
                Attributes = attributeList
            };

            return true;
        }


        /// <summary>
        /// Overload that accepts a byte array for convenience.
        /// </summary>
        public static bool TryParse(byte[] data, out StunMessage message)
        {
            if (data == null)
            {
                message = null;
                return false;
            }

            return TryParse(new ReadOnlySpan<byte>(data), out message);
        }


        // ── Encoding ──────────────────────────────────────────────────────


        /// <summary>
        /// Encodes this message to raw bytes suitable for wire transmission.
        ///
        /// The MESSAGE-INTEGRITY and FINGERPRINT attributes are NOT automatically
        /// added — use MessageIntegrity.AppendIntegrity() and
        /// MessageIntegrity.AppendFingerprint() for that.
        /// </summary>
        public byte[] Encode()
        {
            //
            // Calculate total attribute payload size
            //
            int attributePayloadSize = 0;

            for (int index = 0; index < Attributes.Count; index++)
            {
                attributePayloadSize += Attributes[index].EncodedLength;
            }

            //
            // Allocate the output buffer: header + attributes
            //
            byte[] buffer = new byte[StunConstants.HEADER_SIZE + attributePayloadSize];

            //
            // Write the 20-byte header
            //
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(0), MessageType);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(2), (ushort)attributePayloadSize);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(4), StunConstants.MAGIC_COOKIE);

            if (TransactionId != null && TransactionId.Length >= StunConstants.TRANSACTION_ID_SIZE)
            {
                Array.Copy(TransactionId, 0, buffer, 8, StunConstants.TRANSACTION_ID_SIZE);
            }

            //
            // Write each attribute: type (2) + length (2) + value + padding
            //
            int offset = StunConstants.HEADER_SIZE;

            for (int index = 0; index < Attributes.Count; index++)
            {
                StunAttribute attr = Attributes[index];
                int valueLength = (attr.Value != null) ? attr.Value.Length : 0;

                //
                // Attribute header
                //
                BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset), attr.Type);
                BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset + 2), (ushort)valueLength);
                offset += StunConstants.ATTRIBUTE_HEADER_SIZE;

                //
                // Attribute value
                //
                if (valueLength > 0)
                {
                    Array.Copy(attr.Value, 0, buffer, offset, valueLength);
                }

                //
                // Advance past value + padding
                //
                int paddedLength = (valueLength + 3) & ~3;
                offset += paddedLength;
            }

            return buffer;
        }


        // ── Private Helpers ───────────────────────────────────────────────


        /// <summary>
        /// Generates a cryptographically random 96-bit transaction ID.
        /// </summary>
        private static byte[] GenerateTransactionId()
        {
            byte[] transactionId = new byte[StunConstants.TRANSACTION_ID_SIZE];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(transactionId);
            }

            return transactionId;
        }
    }
}
