// ============================================================================
//
// StunMessageTests.cs — Unit tests for STUN message parsing and encoding.
//
// Tests the core StunMessage class — header parsing, TLV attribute iteration,
// roundtrip encode/parse, factory methods, and validation of malformed input.
//
// ============================================================================

using System;
using System.Buffers.Binary;
using System.Net;
using Xunit;

using Foundation.Networking.Coturn.Protocol;

namespace Foundation.Networking.Coturn.Tests.Protocol
{
    public class StunMessageTests
    {
        // ── Header Parsing ────────────────────────────────────────────────


        [Fact]
        public void TryParse_ValidBindingRequest_ParsesHeader()
        {
            //
            // Build a minimal STUN Binding Request with no attributes
            //
            byte[] data = new byte[20];
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0), StunMessageType.BINDING_REQUEST);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(2), 0);  // no attributes
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(4), StunConstants.MAGIC_COOKIE);

            //
            // Write a known transaction ID
            //
            for (int index = 0; index < 12; index++)
            {
                data[8 + index] = (byte)(index + 1);
            }

            bool success = StunMessage.TryParse(data, out StunMessage message);

            Assert.True(success);
            Assert.Equal(StunMessageType.BINDING_REQUEST, message.MessageType);
            Assert.Equal(0, message.Attributes.Count);

            for (int index = 0; index < 12; index++)
            {
                Assert.Equal((byte)(index + 1), message.TransactionId[index]);
            }
        }


        [Fact]
        public void TryParse_WrongMagicCookie_ReturnsFalse()
        {
            byte[] data = new byte[20];
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0), StunMessageType.BINDING_REQUEST);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(2), 0);
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(4), 0xDEADBEEF);  // wrong magic cookie

            bool success = StunMessage.TryParse(data, out StunMessage message);

            Assert.False(success);
            Assert.Null(message);
        }


        [Fact]
        public void TryParse_TooShortBuffer_ReturnsFalse()
        {
            byte[] data = new byte[10];  // less than 20 byte header

            bool success = StunMessage.TryParse(data, out StunMessage message);

            Assert.False(success);
            Assert.Null(message);
        }


        [Fact]
        public void TryParse_OddMessageLength_ReturnsFalse()
        {
            byte[] data = new byte[20];
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0), StunMessageType.BINDING_REQUEST);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(2), 3);  // not a multiple of 4
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(4), StunConstants.MAGIC_COOKIE);

            bool success = StunMessage.TryParse(data, out StunMessage message);

            Assert.False(success);
        }


        [Fact]
        public void TryParse_NullData_ReturnsFalse()
        {
            bool success = StunMessage.TryParse((byte[])null, out StunMessage message);

            Assert.False(success);
            Assert.Null(message);
        }


        // ── Attribute TLV ─────────────────────────────────────────────────


        [Fact]
        public void TryParse_MessageWithSoftwareAttribute_ParsesCorrectly()
        {
            //
            // Build a binding request with a SOFTWARE attribute ("test")
            //
            string software = "test";
            byte[] softwareBytes = System.Text.Encoding.UTF8.GetBytes(software);

            //
            // Attribute: type(2) + length(2) + value(4 bytes, padded to 4)
            //
            int attrLength = 4 + softwareBytes.Length;
            int paddedAttrLength = (attrLength + 3) & ~3;

            byte[] data = new byte[20 + paddedAttrLength];
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0), StunMessageType.BINDING_REQUEST);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(2), (ushort)paddedAttrLength);
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(4), StunConstants.MAGIC_COOKIE);

            //
            // Write attribute TLV
            //
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(20), StunAttributeType.SOFTWARE);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(22), (ushort)softwareBytes.Length);
            Array.Copy(softwareBytes, 0, data, 24, softwareBytes.Length);

            bool success = StunMessage.TryParse(data, out StunMessage message);

            Assert.True(success);
            Assert.Equal(1, message.Attributes.Count);
            Assert.Equal(StunAttributeType.SOFTWARE, message.Attributes[0].Type);

            string parsedSoftware = message.Attributes[0].ReadString();
            Assert.Equal("test", parsedSoftware);
        }


        // ── Roundtrip Encode/Parse ────────────────────────────────────────


        [Fact]
        public void EncodeAndParse_BindingRequest_Roundtrips()
        {
            //
            // Create a binding request with a SOFTWARE attribute
            //
            StunMessage original = StunMessage.CreateBindingRequest();
            original.AddAttribute(StunAttribute.CreateSoftware("Foundation.Networking.Coturn/1.0"));

            //
            // Encode to bytes, then parse back
            //
            byte[] encoded = original.Encode();
            bool success = StunMessage.TryParse(encoded, out StunMessage parsed);

            Assert.True(success);
            Assert.Equal(original.MessageType, parsed.MessageType);
            Assert.Equal(original.TransactionId, parsed.TransactionId);
            Assert.Equal(1, parsed.Attributes.Count);
            Assert.Equal(StunAttributeType.SOFTWARE, parsed.Attributes[0].Type);
            Assert.Equal("Foundation.Networking.Coturn/1.0", parsed.Attributes[0].ReadString());
        }


        [Fact]
        public void EncodeAndParse_MultipleAttributes_Roundtrips()
        {
            //
            // Create a message with multiple attributes
            //
            StunMessage original = StunMessage.CreateBindingRequest();
            original.AddAttribute(StunAttribute.CreateUsername("testuser"));
            original.AddAttribute(StunAttribute.CreateRealm("example.com"));
            original.AddAttribute(StunAttribute.CreateNonce("nonce-value-123"));
            original.AddAttribute(StunAttribute.CreateSoftware("test/1.0"));

            byte[] encoded = original.Encode();
            bool success = StunMessage.TryParse(encoded, out StunMessage parsed);

            Assert.True(success);
            Assert.Equal(4, parsed.Attributes.Count);
            Assert.Equal("testuser", parsed.FindAttribute(StunAttributeType.USERNAME).ReadString());
            Assert.Equal("example.com", parsed.FindAttribute(StunAttributeType.REALM).ReadString());
            Assert.Equal("nonce-value-123", parsed.FindAttribute(StunAttributeType.NONCE).ReadString());
            Assert.Equal("test/1.0", parsed.FindAttribute(StunAttributeType.SOFTWARE).ReadString());
        }


        // ── Message Type Encoding ─────────────────────────────────────────


        [Fact]
        public void MessageType_Compose_BindingRequest()
        {
            ushort composed = StunMessageType.Compose(StunMessageType.METHOD_BINDING, StunMessageType.CLASS_REQUEST);
            Assert.Equal(StunMessageType.BINDING_REQUEST, composed);
        }


        [Fact]
        public void MessageType_Compose_BindingSuccessResponse()
        {
            ushort composed = StunMessageType.Compose(StunMessageType.METHOD_BINDING, StunMessageType.CLASS_SUCCESS_RESPONSE);
            Assert.Equal(StunMessageType.BINDING_SUCCESS_RESPONSE, composed);
        }


        [Fact]
        public void MessageType_GetMethod_ReturnsCorrectMethod()
        {
            ushort method = StunMessageType.GetMethod(StunMessageType.ALLOCATE_ERROR_RESPONSE);
            Assert.Equal(StunMessageType.METHOD_ALLOCATE, method);
        }


        [Fact]
        public void MessageType_GetClass_ReturnsCorrectClass()
        {
            ushort messageClass = StunMessageType.GetClass(StunMessageType.BINDING_ERROR_RESPONSE);
            Assert.Equal(StunMessageType.CLASS_ERROR_RESPONSE, messageClass);
        }


        [Fact]
        public void MessageType_IsRequest_ReturnsTrue()
        {
            Assert.True(StunMessageType.IsRequest(StunMessageType.BINDING_REQUEST));
            Assert.False(StunMessageType.IsRequest(StunMessageType.BINDING_SUCCESS_RESPONSE));
        }


        // ── XOR-MAPPED-ADDRESS ────────────────────────────────────────────


        [Fact]
        public void XorMappedAddress_IPv4_Roundtrips()
        {
            byte[] transactionId = new byte[12];
            for (int index = 0; index < 12; index++)
            {
                transactionId[index] = (byte)(index + 0xA0);
            }

            IPEndPoint original = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 12345);
            StunAttribute attr = StunAttribute.CreateXorMappedAddress(original, transactionId);

            IPEndPoint parsed = StunAttribute.ParseXorMappedAddress(attr.Value, transactionId);

            Assert.NotNull(parsed);
            Assert.Equal(original.Address, parsed.Address);
            Assert.Equal(original.Port, parsed.Port);
        }


        [Fact]
        public void XorMappedAddress_IPv6_Roundtrips()
        {
            byte[] transactionId = new byte[12];
            for (int index = 0; index < 12; index++)
            {
                transactionId[index] = (byte)(index + 0xB0);
            }

            IPEndPoint original = new IPEndPoint(IPAddress.Parse("2001:db8::1"), 54321);
            StunAttribute attr = StunAttribute.CreateXorMappedAddress(original, transactionId);

            IPEndPoint parsed = StunAttribute.ParseXorMappedAddress(attr.Value, transactionId);

            Assert.NotNull(parsed);
            Assert.Equal(original.Address, parsed.Address);
            Assert.Equal(original.Port, parsed.Port);
        }


        // ── Error Responses ───────────────────────────────────────────────


        [Fact]
        public void CreateErrorResponse_SetsCorrectType()
        {
            StunMessage request = StunMessage.CreateBindingRequest();
            StunMessage errorResponse = StunMessage.CreateErrorResponse(request, StunErrorCode.UNAUTHORIZED, "Unauthorized");

            Assert.True(StunMessageType.IsErrorResponse(errorResponse.MessageType));
            Assert.Equal(StunMessageType.METHOD_BINDING, StunMessageType.GetMethod(errorResponse.MessageType));

            //
            // Verify ERROR-CODE attribute
            //
            StunAttribute errorAttr = errorResponse.FindAttribute(StunAttributeType.ERROR_CODE);
            Assert.NotNull(errorAttr);
            Assert.Equal(4, (int)errorAttr.Value[2]);  // class = 4 (401 / 100)
            Assert.Equal(1, (int)errorAttr.Value[3]);   // number = 1 (401 % 100)
        }
    }
}
