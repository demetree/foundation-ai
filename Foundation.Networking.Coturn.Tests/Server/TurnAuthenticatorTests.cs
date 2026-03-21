// ============================================================================
//
// TurnAuthenticatorTests.cs — Tests for the TURN authentication flow.
//
// ============================================================================

using System;
using Xunit;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Protocol;
using Foundation.Networking.Coturn.Server;

namespace Foundation.Networking.Coturn.Tests.Server
{
    public class TurnAuthenticatorTests
    {
        private TurnServerConfiguration CreateConfig()
        {
            return new TurnServerConfiguration
            {
                Realm = "test.local",
                SharedSecret = "test-secret-key",
                NonceLifetimeSeconds = 3600
            };
        }


        [Fact]
        public void GenerateNonce_ProducesUniqueValues()
        {
            TurnServerConfiguration config = CreateConfig();
            TurnAuthenticator auth = new TurnAuthenticator(config);

            string nonce1 = auth.GenerateNonce();
            string nonce2 = auth.GenerateNonce();

            Assert.False(string.IsNullOrEmpty(nonce1));
            Assert.False(string.IsNullOrEmpty(nonce2));
            Assert.NotEqual(nonce1, nonce2);
        }


        [Fact]
        public void IsNonceValid_ReturnsTrueForFreshNonce()
        {
            TurnServerConfiguration config = CreateConfig();
            TurnAuthenticator auth = new TurnAuthenticator(config);

            string nonce = auth.GenerateNonce();

            Assert.True(auth.IsNonceValid(nonce));
        }


        [Fact]
        public void IsNonceValid_ReturnsFalseForUnknownNonce()
        {
            TurnServerConfiguration config = CreateConfig();
            TurnAuthenticator auth = new TurnAuthenticator(config);

            Assert.False(auth.IsNonceValid("unknown-nonce-value"));
        }


        [Fact]
        public void TryAuthenticate_NoIntegrity_Returns401()
        {
            TurnServerConfiguration config = CreateConfig();
            TurnAuthenticator auth = new TurnAuthenticator(config);

            //
            // Create a message with no MESSAGE-INTEGRITY attribute
            //
            StunMessage message = StunMessage.CreateBindingRequest();
            byte[] rawBytes = message.Encode();

            TurnAuthenticator.AuthResult result = auth.TryAuthenticate(message, rawBytes);

            Assert.False(result.IsAuthenticated);
            Assert.Equal(StunErrorCode.UNAUTHORIZED, result.ErrorCode);
            Assert.Equal("test.local", result.Realm);
            Assert.False(string.IsNullOrEmpty(result.Nonce));
        }


        [Fact]
        public void TryAuthenticate_MissingCredentialAttributes_ReturnsBadRequest()
        {
            TurnServerConfiguration config = CreateConfig();
            TurnAuthenticator auth = new TurnAuthenticator(config);

            //
            // Create a message with MESSAGE-INTEGRITY but missing USERNAME/REALM/NONCE
            //
            StunMessage message = StunMessage.CreateBindingRequest();
            message.AddAttribute(new StunAttribute
            {
                Type = StunAttributeType.MESSAGE_INTEGRITY,
                Value = new byte[20]
            });

            byte[] rawBytes = message.Encode();

            TurnAuthenticator.AuthResult result = auth.TryAuthenticate(message, rawBytes);

            Assert.False(result.IsAuthenticated);
            Assert.Equal(StunErrorCode.BAD_REQUEST, result.ErrorCode);
        }


        [Fact]
        public void TryAuthenticate_StaleNonce_Returns438()
        {
            TurnServerConfiguration config = CreateConfig();
            config.NonceLifetimeSeconds = 0;  // Nonces expire immediately

            TurnAuthenticator auth = new TurnAuthenticator(config);
            string nonce = auth.GenerateNonce();

            //
            // Build a message with credentials using the (now expired) nonce
            //
            StunMessage message = StunMessage.CreateBindingRequest();
            message.AddAttribute(StunAttribute.CreateUsername("testuser"));
            message.AddAttribute(StunAttribute.CreateRealm("test.local"));
            message.AddAttribute(StunAttribute.CreateNonce(nonce));
            message.AddAttribute(new StunAttribute
            {
                Type = StunAttributeType.MESSAGE_INTEGRITY,
                Value = new byte[20]
            });

            byte[] rawBytes = message.Encode();

            //
            // Wait a moment for the nonce to actually expire
            //
            System.Threading.Thread.Sleep(50);

            TurnAuthenticator.AuthResult result = auth.TryAuthenticate(message, rawBytes);

            Assert.False(result.IsAuthenticated);
            Assert.Equal(StunErrorCode.STALE_NONCE, result.ErrorCode);
            Assert.True(result.IsStaleNonce);
        }


        [Fact]
        public void CleanupExpiredNonces_RemovesOldNonces()
        {
            TurnServerConfiguration config = CreateConfig();
            config.NonceLifetimeSeconds = 0;  // Expire immediately

            TurnAuthenticator auth = new TurnAuthenticator(config);
            string nonce = auth.GenerateNonce();

            //
            // Wait for expiry
            //
            System.Threading.Thread.Sleep(50);

            auth.CleanupExpiredNonces();

            Assert.False(auth.IsNonceValid(nonce));
        }
    }
}
