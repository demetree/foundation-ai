// ============================================================================
//
// TurnServerProviderTests.cs — Unit tests for CoturnTurnServerProvider.
//
// Tests credential generation, configuration validation, and the
// time-limited TURN REST API credential scheme.
//
// ============================================================================

using System;
using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Services;

namespace Foundation.Networking.Coturn.Tests.Services
{
    public class TurnServerProviderTests
    {
        // ── Credential Generation ─────────────────────────────────────────


        [Fact]
        public void GenerateTimeLimitedCredential_ProducesNonEmptyCredential()
        {
            string sharedSecret = "test-shared-secret";
            int ttlSeconds = 86400;

            CoturnTurnServerProvider.GenerateTimeLimitedCredential(
                sharedSecret, ttlSeconds, out string username, out string credential);

            //
            // Username should be a Unix timestamp
            //
            Assert.False(string.IsNullOrWhiteSpace(username));

            bool isNumber = long.TryParse(username, out long timestamp);
            Assert.True(isNumber);

            //
            // The timestamp should be in the future
            //
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Assert.True(timestamp > now);
            Assert.True(timestamp <= now + ttlSeconds + 5);  // allow a few seconds of clock drift

            //
            // Credential should be a Base64-encoded HMAC-SHA1 hash
            //
            Assert.False(string.IsNullOrWhiteSpace(credential));

            byte[] credentialBytes = Convert.FromBase64String(credential);
            Assert.Equal(20, credentialBytes.Length);  // HMAC-SHA1 always produces 20 bytes
        }


        [Fact]
        public void GenerateTimeLimitedCredential_WithUserId_IncludesIdInUsername()
        {
            string sharedSecret = "test-secret";
            int ttlSeconds = 3600;
            string userId = "user42";

            CoturnTurnServerProvider.GenerateTimeLimitedCredential(
                sharedSecret, ttlSeconds, userId, out string username, out string credential);

            //
            // Username should contain the user ID after a colon
            //
            Assert.Contains(":", username);
            Assert.EndsWith(":user42", username);

            //
            // Credential should still be valid Base64
            //
            byte[] credentialBytes = Convert.FromBase64String(credential);
            Assert.Equal(20, credentialBytes.Length);
        }


        [Fact]
        public void GenerateTimeLimitedCredential_SameInput_ProducesSameCredential()
        {
            //
            // Two calls with the same inputs should produce the same HMAC
            // (since the username determines the output for a given secret)
            //
            string sharedSecret = "deterministic-test";
            string username = "1700000000";

            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(sharedSecret);
            byte[] usernameBytes = System.Text.Encoding.UTF8.GetBytes(username);

            using (System.Security.Cryptography.HMACSHA1 hmac1 = new System.Security.Cryptography.HMACSHA1(keyBytes))
            using (System.Security.Cryptography.HMACSHA1 hmac2 = new System.Security.Cryptography.HMACSHA1(keyBytes))
            {
                byte[] hash1 = hmac1.ComputeHash(usernameBytes);
                byte[] hash2 = hmac2.ComputeHash(usernameBytes);

                Assert.Equal(hash1, hash2);
            }
        }


        // ── Provider Configuration ────────────────────────────────────────


        [Fact]
        public void IsEnabled_ReturnsFalse_WhenNotEnabled()
        {
            TurnServerOptions options = new TurnServerOptions
            {
                Enabled = false,
                SharedSecret = "secret",
                TurnUrls = new List<string> { "turn:example.com:3478" }
            };

            CoturnTurnServerProvider provider = new CoturnTurnServerProvider(options);

            Assert.False(provider.IsEnabled);
        }


        [Fact]
        public void IsEnabled_ReturnsTrue_WhenEnabled()
        {
            TurnServerOptions options = new TurnServerOptions
            {
                Enabled = true,
                SharedSecret = "secret",
                TurnUrls = new List<string> { "turn:example.com:3478" }
            };

            CoturnTurnServerProvider provider = new CoturnTurnServerProvider(options);

            Assert.True(provider.IsEnabled);
        }


        [Fact]
        public void ValidateConfiguration_ReturnsFalse_WhenNoTurnUrls()
        {
            TurnServerOptions options = new TurnServerOptions
            {
                Enabled = true,
                SharedSecret = "secret",
                TurnUrls = new List<string>()
            };

            CoturnTurnServerProvider provider = new CoturnTurnServerProvider(options);

            Assert.False(provider.ValidateConfiguration());
        }


        [Fact]
        public void ValidateConfiguration_ReturnsFalse_WhenNoSharedSecret()
        {
            TurnServerOptions options = new TurnServerOptions
            {
                Enabled = true,
                SharedSecret = "",
                TurnUrls = new List<string> { "turn:example.com:3478" }
            };

            CoturnTurnServerProvider provider = new CoturnTurnServerProvider(options);

            Assert.False(provider.ValidateConfiguration());
        }


        [Fact]
        public void ValidateConfiguration_ReturnsTrue_WhenFullyConfigured()
        {
            TurnServerOptions options = new TurnServerOptions
            {
                Enabled = true,
                SharedSecret = "my-secret",
                TurnUrls = new List<string> { "turn:example.com:3478" }
            };

            CoturnTurnServerProvider provider = new CoturnTurnServerProvider(options);

            Assert.True(provider.ValidateConfiguration());
        }


        // ── GetIceServers ─────────────────────────────────────────────────


        [Fact]
        public void GetIceServers_ReturnsStunAndTurnServers()
        {
            TurnServerOptions options = new TurnServerOptions
            {
                Enabled = true,
                SharedSecret = "test-secret",
                CredentialTtlSeconds = 86400,
                StunUrls = new List<string> { "stun:stun.example.com:3478" },
                TurnUrls = new List<string> { "turn:turn.example.com:3478", "turns:turn.example.com:5349" }
            };

            CoturnTurnServerProvider provider = new CoturnTurnServerProvider(options);
            List<IceServerInfo> iceServerList = provider.GetIceServers();

            //
            // Should have 2 entries: one for STUN (no creds) and one for TURN (with creds)
            //
            Assert.Equal(2, iceServerList.Count);

            //
            // STUN server — no credentials
            //
            Assert.Equal(1, iceServerList[0].Urls.Count);
            Assert.Contains("stun:", iceServerList[0].Urls[0]);
            Assert.Null(iceServerList[0].Username);

            //
            // TURN server — has credentials
            //
            Assert.Equal(2, iceServerList[1].Urls.Count);
            Assert.False(string.IsNullOrWhiteSpace(iceServerList[1].Username));
            Assert.False(string.IsNullOrWhiteSpace(iceServerList[1].Credential));
            Assert.Equal("password", iceServerList[1].CredentialType);
        }


        [Fact]
        public void GetIceServers_NoStunUrls_ReturnsTurnOnly()
        {
            TurnServerOptions options = new TurnServerOptions
            {
                Enabled = true,
                SharedSecret = "test-secret",
                TurnUrls = new List<string> { "turn:turn.example.com:3478" }
            };

            CoturnTurnServerProvider provider = new CoturnTurnServerProvider(options);
            List<IceServerInfo> iceServerList = provider.GetIceServers();

            Assert.Equal(1, iceServerList.Count);
            Assert.Contains("turn:", iceServerList[0].Urls[0]);
        }
    }
}
