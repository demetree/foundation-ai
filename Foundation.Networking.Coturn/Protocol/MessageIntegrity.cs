// ============================================================================
//
// MessageIntegrity.cs — STUN MESSAGE-INTEGRITY and FINGERPRINT computation.
//
// MESSAGE-INTEGRITY uses HMAC-SHA1 to authenticate STUN messages (RFC 5389
// §15.4).  The HMAC is computed over the entire STUN message up to and
// including the MESSAGE-INTEGRITY attribute, with the message length field
// adjusted to point to the end of MESSAGE-INTEGRITY.
//
// FINGERPRINT uses CRC-32 XOR'd with 0x5354554E ("STUN") to detect
// STUN messages in a stream of mixed traffic (RFC 5389 §15.5).
//
// For long-term credentials, the HMAC key is MD5(username:realm:password).
//
// ============================================================================

using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Foundation.Networking.Coturn.Protocol
{
    /// <summary>
    ///
    /// STUN message integrity and fingerprint computation per RFC 5389 §15.4-§15.5.
    ///
    /// </summary>
    public static class MessageIntegrity
    {
        //
        // CRC-32 lookup table (same polynomial as used by coturn: ISO 3309 / ITU-T V.42)
        //
        private static readonly uint[] _crc32Table = GenerateCrc32Table();


        // ── HMAC-SHA1 Message Integrity ───────────────────────────────────


        /// <summary>
        /// Computes the HMAC-SHA1 message integrity value for the given message bytes.
        ///
        /// The message bytes should include the STUN header and all attributes up to
        /// (but NOT including) the MESSAGE-INTEGRITY attribute itself.  The length
        /// field in the header must be pre-adjusted to account for the 24 bytes that
        /// MESSAGE-INTEGRITY will occupy (4-byte attr header + 20-byte HMAC).
        /// </summary>
        public static byte[] ComputeHmacSha1(ReadOnlySpan<byte> messageBytes, byte[] key)
        {
            using (HMACSHA1 hmac = new HMACSHA1(key))
            {
                return hmac.ComputeHash(messageBytes.ToArray());
            }
        }


        /// <summary>
        /// Computes the HMAC-SHA1 message integrity value for the given message bytes.
        /// Convenience overload for byte arrays.
        /// </summary>
        public static byte[] ComputeHmacSha1(byte[] messageBytes, byte[] key)
        {
            return ComputeHmacSha1(new ReadOnlySpan<byte>(messageBytes), key);
        }


        /// <summary>
        /// Computes the long-term credential HMAC key: MD5(username:realm:password).
        ///
        /// Per RFC 5389 §15.4, when long-term credentials are used, the key for
        /// MESSAGE-INTEGRITY is the MD5 hash of "username:realm:password".
        /// </summary>
        public static byte[] ComputeLongTermKey(string username, string realm, string password)
        {
            string input = username + ":" + realm + ":" + password;
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(inputBytes);
            }
        }


        /// <summary>
        /// Computes the short-term credential HMAC key: UTF-8(password).
        ///
        /// For short-term credentials, the key is just the SASLprep'd password.
        /// </summary>
        public static byte[] ComputeShortTermKey(string password)
        {
            return Encoding.UTF8.GetBytes(password);
        }


        /// <summary>
        /// Validates the MESSAGE-INTEGRITY attribute in a parsed STUN message.
        ///
        /// Takes the original raw bytes and the HMAC key.  Returns true if the
        /// MESSAGE-INTEGRITY attribute value matches the computed HMAC.
        /// </summary>
        public static bool ValidateIntegrity(byte[] rawMessage, byte[] key)
        {
            if (rawMessage == null || rawMessage.Length < StunConstants.HEADER_SIZE)
            {
                return false;
            }

            //
            // Find the MESSAGE-INTEGRITY attribute position by scanning the attributes
            //
            int integrityOffset = FindAttributeOffset(rawMessage, StunAttributeType.MESSAGE_INTEGRITY);

            if (integrityOffset < 0)
            {
                return false;
            }

            //
            // The message integrity covers everything up to and including the 
            // MESSAGE-INTEGRITY attribute header, but with the message length adjusted
            //
            int messageUpToIntegrity = integrityOffset + StunConstants.ATTRIBUTE_HEADER_SIZE + StunConstants.HMAC_SHA1_LENGTH;

            //
            // Create a copy of the message and adjust the length field
            //
            byte[] adjustedMessage = new byte[integrityOffset];
            Array.Copy(rawMessage, adjustedMessage, integrityOffset);

            //
            // Set the message length to point to the end of MESSAGE-INTEGRITY attribute
            //
            ushort adjustedLength = (ushort)(messageUpToIntegrity - StunConstants.HEADER_SIZE);
            BinaryPrimitives.WriteUInt16BigEndian(adjustedMessage.AsSpan(2), adjustedLength);

            //
            // Compute the expected HMAC
            //
            byte[] expectedHmac = ComputeHmacSha1(adjustedMessage, key);

            //
            // Read the actual HMAC from the attribute value
            //
            byte[] actualHmac = new byte[StunConstants.HMAC_SHA1_LENGTH];
            Array.Copy(rawMessage, integrityOffset + StunConstants.ATTRIBUTE_HEADER_SIZE, actualHmac, 0, StunConstants.HMAC_SHA1_LENGTH);

            //
            // Constant-time comparison to prevent timing attacks
            //
            return CryptographicOperations.FixedTimeEquals(expectedHmac, actualHmac);
        }


        /// <summary>
        /// Appends a MESSAGE-INTEGRITY attribute to the encoded message bytes.
        ///
        /// Returns a new byte array with the MESSAGE-INTEGRITY attribute added.
        /// The message length field is adjusted accordingly.
        /// </summary>
        public static byte[] AppendIntegrity(byte[] encodedMessage, byte[] key)
        {
            //
            // The new length includes the MESSAGE-INTEGRITY attribute (24 bytes: 4 header + 20 HMAC)
            //
            int integrityAttrSize = StunConstants.ATTRIBUTE_HEADER_SIZE + StunConstants.HMAC_SHA1_LENGTH;
            int newTotalLength = encodedMessage.Length + integrityAttrSize;

            //
            // Make a copy with the adjusted length field for HMAC computation
            //
            byte[] forHmac = new byte[encodedMessage.Length];
            Array.Copy(encodedMessage, forHmac, encodedMessage.Length);

            ushort newMessageLength = (ushort)(newTotalLength - StunConstants.HEADER_SIZE);
            BinaryPrimitives.WriteUInt16BigEndian(forHmac.AsSpan(2), newMessageLength);

            //
            // Compute the HMAC over the message with the adjusted length
            //
            byte[] hmac = ComputeHmacSha1(forHmac, key);

            //
            // Build the final message: original + MESSAGE-INTEGRITY attribute
            //
            byte[] result = new byte[newTotalLength];
            Array.Copy(encodedMessage, result, encodedMessage.Length);

            //
            // Write the attribute header and HMAC value
            //
            int offset = encodedMessage.Length;
            BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(offset), StunAttributeType.MESSAGE_INTEGRITY);
            BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(offset + 2), StunConstants.HMAC_SHA1_LENGTH);
            Array.Copy(hmac, 0, result, offset + StunConstants.ATTRIBUTE_HEADER_SIZE, StunConstants.HMAC_SHA1_LENGTH);

            //
            // Update the message length field in the header
            //
            BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(2), newMessageLength);

            return result;
        }


        // ── CRC-32 Fingerprint ────────────────────────────────────────────


        /// <summary>
        /// Computes the CRC-32 fingerprint for the given message bytes, XOR'd with
        /// the STUN magic value 0x5354554E.
        ///
        /// The message bytes should include everything up to (but NOT including)
        /// the FINGERPRINT attribute itself.
        /// </summary>
        public static uint ComputeFingerprint(ReadOnlySpan<byte> messageBytes)
        {
            uint crc = 0xFFFFFFFF;

            for (int index = 0; index < messageBytes.Length; index++)
            {
                byte tableIndex = (byte)(crc ^ messageBytes[index]);
                crc = _crc32Table[tableIndex] ^ (crc >> 8);
            }

            crc = crc ^ 0xFFFFFFFF;
            crc = crc ^ StunConstants.FINGERPRINT_XOR;

            return crc;
        }


        /// <summary>
        /// Computes the CRC-32 fingerprint for the given message bytes.
        /// Convenience overload for byte arrays.
        /// </summary>
        public static uint ComputeFingerprint(byte[] messageBytes)
        {
            return ComputeFingerprint(new ReadOnlySpan<byte>(messageBytes));
        }


        /// <summary>
        /// Validates the FINGERPRINT attribute in a raw STUN message.
        /// </summary>
        public static bool ValidateFingerprint(byte[] rawMessage)
        {
            if (rawMessage == null || rawMessage.Length < StunConstants.HEADER_SIZE + 8)
            {
                return false;
            }

            //
            // Find the FINGERPRINT attribute — it must be the last attribute
            //
            int fingerprintOffset = FindAttributeOffset(rawMessage, StunAttributeType.FINGERPRINT);

            if (fingerprintOffset < 0)
            {
                return false;
            }

            //
            // Compute the fingerprint over everything before the FINGERPRINT attribute,
            // with the length field adjusted to include the FINGERPRINT attribute
            //
            byte[] forCrc = new byte[fingerprintOffset];
            Array.Copy(rawMessage, forCrc, fingerprintOffset);

            int fingerprintAttrSize = StunConstants.ATTRIBUTE_HEADER_SIZE + StunConstants.FINGERPRINT_LENGTH;
            ushort adjustedLength = (ushort)(fingerprintOffset + fingerprintAttrSize - StunConstants.HEADER_SIZE);
            BinaryPrimitives.WriteUInt16BigEndian(forCrc.AsSpan(2), adjustedLength);

            uint expectedFingerprint = ComputeFingerprint(forCrc);

            //
            // Read the actual fingerprint from the attribute value
            //
            uint actualFingerprint = BinaryPrimitives.ReadUInt32BigEndian(
                rawMessage.AsSpan(fingerprintOffset + StunConstants.ATTRIBUTE_HEADER_SIZE, 4));

            return expectedFingerprint == actualFingerprint;
        }


        /// <summary>
        /// Appends a FINGERPRINT attribute to the encoded message bytes.
        ///
        /// Returns a new byte array with the FINGERPRINT attribute added.
        /// </summary>
        public static byte[] AppendFingerprint(byte[] encodedMessage)
        {
            int fingerprintAttrSize = StunConstants.ATTRIBUTE_HEADER_SIZE + StunConstants.FINGERPRINT_LENGTH;
            int newTotalLength = encodedMessage.Length + fingerprintAttrSize;

            //
            // Adjust the message length to include the fingerprint
            //
            byte[] forCrc = new byte[encodedMessage.Length];
            Array.Copy(encodedMessage, forCrc, encodedMessage.Length);

            ushort newMessageLength = (ushort)(newTotalLength - StunConstants.HEADER_SIZE);
            BinaryPrimitives.WriteUInt16BigEndian(forCrc.AsSpan(2), newMessageLength);

            //
            // Compute the CRC-32 over the adjusted message
            //
            uint fingerprint = ComputeFingerprint(forCrc);

            //
            // Build the final message: original + FINGERPRINT attribute
            //
            byte[] result = new byte[newTotalLength];
            Array.Copy(encodedMessage, result, encodedMessage.Length);

            int offset = encodedMessage.Length;
            BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(offset), StunAttributeType.FINGERPRINT);
            BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(offset + 2), StunConstants.FINGERPRINT_LENGTH);
            BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(offset + StunConstants.ATTRIBUTE_HEADER_SIZE), fingerprint);

            //
            // Update the message length field in the header
            //
            BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(2), newMessageLength);

            return result;
        }


        // ── Private Helpers ───────────────────────────────────────────────


        /// <summary>
        /// Scans raw STUN message bytes for the byte offset of a given attribute type.
        /// Returns -1 if not found.
        /// </summary>
        private static int FindAttributeOffset(byte[] rawMessage, ushort targetAttributeType)
        {
            int offset = StunConstants.HEADER_SIZE;
            ushort messageLength = BinaryPrimitives.ReadUInt16BigEndian(rawMessage.AsSpan(2, 2));
            int endOffset = StunConstants.HEADER_SIZE + messageLength;

            if (endOffset > rawMessage.Length)
            {
                endOffset = rawMessage.Length;
            }

            while (offset + StunConstants.ATTRIBUTE_HEADER_SIZE <= endOffset)
            {
                ushort attrType = BinaryPrimitives.ReadUInt16BigEndian(rawMessage.AsSpan(offset, 2));
                ushort attrLength = BinaryPrimitives.ReadUInt16BigEndian(rawMessage.AsSpan(offset + 2, 2));

                if (attrType == targetAttributeType)
                {
                    return offset;
                }

                int paddedLength = (attrLength + 3) & ~3;
                offset += StunConstants.ATTRIBUTE_HEADER_SIZE + paddedLength;
            }

            return -1;
        }


        /// <summary>
        /// Generates the CRC-32 lookup table using the standard polynomial 0xEDB88320.
        /// This is the same polynomial used by coturn, zlib, and ISO 3309.
        /// </summary>
        private static uint[] GenerateCrc32Table()
        {
            uint[] table = new uint[256];

            for (uint index = 0; index < 256; index++)
            {
                uint crc = index;

                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 1) == 1)
                    {
                        crc = (crc >> 1) ^ 0xEDB88320;
                    }
                    else
                    {
                        crc = crc >> 1;
                    }
                }

                table[index] = crc;
            }

            return table;
        }
    }
}
