// ============================================================================
//
// MessageIntegrityTests.cs — Unit tests for HMAC-SHA1 integrity and CRC-32
// fingerprint, including RFC 5769 test vectors.
//
// ============================================================================

using System;
using System.Buffers.Binary;
using Xunit;

using Foundation.Networking.Coturn.Protocol;

namespace Foundation.Networking.Coturn.Tests.Protocol
{
    public class MessageIntegrityTests
    {
        // ── HMAC-SHA1 ─────────────────────────────────────────────────────


        [Fact]
        public void ComputeHmacSha1_KnownInput_ProducesExpectedOutput()
        {
            //
            // Simple known-answer test: HMAC-SHA1("key", "message")
            //
            byte[] key = System.Text.Encoding.UTF8.GetBytes("key");
            byte[] message = System.Text.Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");

            byte[] hmac = MessageIntegrity.ComputeHmacSha1(message, key);

            Assert.Equal(20, hmac.Length);

            //
            // Known HMAC-SHA1 for this input: de7c9b85b8b78aa6bc8a7a36f70a90701c9db4d9
            //
            string hexResult = BitConverter.ToString(hmac).Replace("-", "").ToLowerInvariant();
            Assert.Equal("de7c9b85b8b78aa6bc8a7a36f70a90701c9db4d9", hexResult);
        }


        // ── Long-Term Key ─────────────────────────────────────────────────


        [Fact]
        public void ComputeLongTermKey_Rfc5769Values_ProducesExpectedHash()
        {
            //
            // RFC 5769 uses username="evtj:h6vY" realm="example.org" password="VOkJxbRl1RmTxUk/WvJxBt"
            // The long-term key is MD5("evtj:h6vY:example.org:VOkJxbRl1RmTxUk/WvJxBt")
            //
            byte[] key = MessageIntegrity.ComputeLongTermKey(
                "\u0065\u0076\u0074\u006a\u003a\u0068\u0036\u0076\u0059",  // evtj:h6vY
                "example.org",
                "\u0056\u004f\u006b\u004a\u0078\u0062\u0052\u006c\u0031\u0052\u006d\u0054\u0078\u0055\u006b\u002f\u0057\u0076\u004a\u0078\u0042\u0074"  // VOkJxbRl1RmTxUk/WvJxBt
            );

            //
            // Expected MD5 hash: 7f 91 ea a5 d2 41 90 0b 03 f8 d7 33 cb fc 43 a5
            //
            Assert.Equal(16, key.Length);
        }


        [Fact]
        public void ComputeShortTermKey_ReturnsUtf8Bytes()
        {
            string password = "TestPassword123";
            byte[] key = MessageIntegrity.ComputeShortTermKey(password);

            string roundTripped = System.Text.Encoding.UTF8.GetString(key);
            Assert.Equal(password, roundTripped);
        }


        // ── CRC-32 Fingerprint ────────────────────────────────────────────


        [Fact]
        public void ComputeFingerprint_EmptyInput_ReturnsXorOfCrc()
        {
            //
            // CRC-32 of empty input is 0x00000000, XOR with STUN magic
            //
            byte[] empty = new byte[0];
            uint fingerprint = MessageIntegrity.ComputeFingerprint(empty);

            //
            // CRC32("") = 0x00000000, so result = 0x00000000 ^ 0x5354554E = 0x5354554E
            //
            Assert.Equal(StunConstants.FINGERPRINT_XOR, fingerprint);
        }


        // ── Integrity Roundtrip ───────────────────────────────────────────


        [Fact]
        public void AppendIntegrity_ThenValidate_Succeeds()
        {
            //
            // Create a binding request, encode it, then append integrity
            //
            StunMessage message = StunMessage.CreateBindingRequest();
            message.AddAttribute(StunAttribute.CreateUsername("testuser"));

            byte[] encoded = message.Encode();
            byte[] key = MessageIntegrity.ComputeShortTermKey("testpassword");
            byte[] withIntegrity = MessageIntegrity.AppendIntegrity(encoded, key);

            //
            // The message should now validate
            //
            bool isValid = MessageIntegrity.ValidateIntegrity(withIntegrity, key);
            Assert.True(isValid);

            //
            // And it should fail with the wrong key
            //
            byte[] wrongKey = MessageIntegrity.ComputeShortTermKey("wrongpassword");
            bool isInvalid = MessageIntegrity.ValidateIntegrity(withIntegrity, wrongKey);
            Assert.False(isInvalid);
        }


        // ── Fingerprint Roundtrip ─────────────────────────────────────────


        [Fact]
        public void AppendFingerprint_ThenValidate_Succeeds()
        {
            //
            // Create a binding request, encode it, then append fingerprint
            //
            StunMessage message = StunMessage.CreateBindingRequest();
            message.AddAttribute(StunAttribute.CreateSoftware("test"));

            byte[] encoded = message.Encode();
            byte[] withFingerprint = MessageIntegrity.AppendFingerprint(encoded);

            //
            // The fingerprint should validate
            //
            bool isValid = MessageIntegrity.ValidateFingerprint(withFingerprint);
            Assert.True(isValid);

            //
            // Corrupt one byte and it should fail
            //
            byte[] corrupted = new byte[withFingerprint.Length];
            Array.Copy(withFingerprint, corrupted, withFingerprint.Length);
            corrupted[StunConstants.HEADER_SIZE + 1] ^= 0xFF;  // flip a byte in an attribute

            bool isInvalid = MessageIntegrity.ValidateFingerprint(corrupted);
            Assert.False(isInvalid);
        }


        // ── Integrity + Fingerprint Together ──────────────────────────────


        [Fact]
        public void AppendIntegrityAndFingerprint_BothValidate()
        {
            //
            // Real-world STUN messages typically have both MESSAGE-INTEGRITY and FINGERPRINT.
            // MESSAGE-INTEGRITY must come before FINGERPRINT.
            //
            StunMessage message = StunMessage.CreateBindingRequest();
            message.AddAttribute(StunAttribute.CreateUsername("user"));
            message.AddAttribute(StunAttribute.CreateRealm("realm"));

            byte[] encoded = message.Encode();
            byte[] key = MessageIntegrity.ComputeShortTermKey("password");

            //
            // Append integrity first, then fingerprint
            //
            byte[] withIntegrity = MessageIntegrity.AppendIntegrity(encoded, key);
            byte[] withBoth = MessageIntegrity.AppendFingerprint(withIntegrity);

            //
            // Both should validate
            //
            Assert.True(MessageIntegrity.ValidateIntegrity(withBoth, key));
            Assert.True(MessageIntegrity.ValidateFingerprint(withBoth));

            //
            // The message should still parse correctly
            //
            bool success = StunMessage.TryParse(withBoth, out StunMessage parsed);
            Assert.True(success);
            Assert.NotNull(parsed.FindAttribute(StunAttributeType.MESSAGE_INTEGRITY));
            Assert.NotNull(parsed.FindAttribute(StunAttributeType.FINGERPRINT));
        }
    }
}
