// ============================================================================
//
// TurnAuthenticator.cs — TURN server-side authentication.
//
// Implements the long-term credential mechanism (RFC 5389 §10.2) and TURN
// REST API timestamp-based authentication.
//
// Authentication flow:
//   1. Client sends request without credentials
//   2. Server responds 401 with REALM + NONCE
//   3. Client resends with USERNAME, REALM, NONCE, MESSAGE-INTEGRITY
//   4. Server validates MESSAGE-INTEGRITY using the appropriate key
//
// For TURN REST API auth (coturn --use-auth-secret):
//   - Username is "timestamp[:userId]"
//   - HMAC key = HMAC-SHA1(shared_secret, username)
//   - Server validates that timestamp hasn't expired
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Protocol;

namespace Foundation.Networking.Coturn.Server
{
    /// <summary>
    ///
    /// Server-side TURN authentication — handles the 401 challenge flow
    /// and validates credentials using either long-term credentials or
    /// TURN REST API shared secret mode.
    ///
    /// </summary>
    public class TurnAuthenticator
    {
        private readonly TurnServerConfiguration _config;

        //
        // Active nonces: maps nonce string → expiry time
        //
        private readonly ConcurrentDictionary<string, DateTime> _nonces;


        public TurnAuthenticator(TurnServerConfiguration config)
        {
            _config = config;
            _nonces = new ConcurrentDictionary<string, DateTime>();
        }


        // ── Nonce Management ──────────────────────────────────────────────


        /// <summary>
        /// Generates a new cryptographic nonce and tracks it.
        /// </summary>
        public string GenerateNonce()
        {
            byte[] nonceBytes = new byte[16];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonceBytes);
            }

            string nonce = BitConverter.ToString(nonceBytes).Replace("-", "").ToLowerInvariant();

            DateTime expiresAt = DateTime.UtcNow.AddSeconds(_config.NonceLifetimeSeconds);
            _nonces[nonce] = expiresAt;

            return nonce;
        }


        /// <summary>
        /// Checks if a nonce is valid (exists and hasn't expired).
        /// </summary>
        public bool IsNonceValid(string nonce)
        {
            if (string.IsNullOrEmpty(nonce))
            {
                return false;
            }

            if (_nonces.TryGetValue(nonce, out DateTime expiresAt))
            {
                if (DateTime.UtcNow < expiresAt)
                {
                    return true;
                }

                //
                // Expired — remove it
                //
                _nonces.TryRemove(nonce, out _);
            }

            return false;
        }


        /// <summary>
        /// Removes expired nonces from the tracking dictionary.
        /// </summary>
        public void CleanupExpiredNonces()
        {
            DateTime now = DateTime.UtcNow;

            foreach (var kvp in _nonces)
            {
                if (now >= kvp.Value)
                {
                    _nonces.TryRemove(kvp.Key, out _);
                }
            }
        }


        // ── Authentication ────────────────────────────────────────────────


        /// <summary>
        /// Authentication result returned by TryAuthenticate.
        /// </summary>
        public class AuthResult
        {
            public bool IsAuthenticated { get; set; }
            public int ErrorCode { get; set; }
            public string ErrorReason { get; set; }
            public string Username { get; set; }
            public string Realm { get; set; }
            public string Nonce { get; set; }
            public byte[] IntegrityKey { get; set; }
            public bool IsStaleNonce { get; set; }
        }


        /// <summary>
        /// Attempts to authenticate a STUN/TURN request.
        ///
        /// Returns an AuthResult indicating success or the appropriate error code.
        /// </summary>
        public AuthResult TryAuthenticate(StunMessage message, byte[] rawBytes)
        {
            //
            // Extract credentials from the message
            //
            StunAttribute usernameAttr = message.FindAttribute(StunAttributeType.USERNAME);
            StunAttribute realmAttr = message.FindAttribute(StunAttributeType.REALM);
            StunAttribute nonceAttr = message.FindAttribute(StunAttributeType.NONCE);
            StunAttribute integrityAttr = message.FindAttribute(StunAttributeType.MESSAGE_INTEGRITY);

            //
            // If no MESSAGE-INTEGRITY → 401 challenge
            //
            if (integrityAttr == null)
            {
                string nonce = GenerateNonce();

                return new AuthResult
                {
                    IsAuthenticated = false,
                    ErrorCode = StunErrorCode.UNAUTHORIZED,
                    ErrorReason = "Unauthorized",
                    Nonce = nonce,
                    Realm = _config.Realm
                };
            }

            //
            // All four must be present if MESSAGE-INTEGRITY is included
            //
            if (usernameAttr == null || realmAttr == null || nonceAttr == null)
            {
                return new AuthResult
                {
                    IsAuthenticated = false,
                    ErrorCode = StunErrorCode.BAD_REQUEST,
                    ErrorReason = "Missing required authentication attributes"
                };
            }

            string username = usernameAttr.ReadString();
            string realm = realmAttr.ReadString();
            string nonce2 = nonceAttr.ReadString();

            //
            // Validate the nonce
            //
            if (IsNonceValid(nonce2) == false)
            {
                string freshNonce = GenerateNonce();

                return new AuthResult
                {
                    IsAuthenticated = false,
                    ErrorCode = StunErrorCode.STALE_NONCE,
                    ErrorReason = "Stale nonce",
                    IsStaleNonce = true,
                    Nonce = freshNonce,
                    Realm = _config.Realm
                };
            }

            //
            // Validate the realm
            //
            if (realm != _config.Realm)
            {
                return new AuthResult
                {
                    IsAuthenticated = false,
                    ErrorCode = StunErrorCode.UNAUTHORIZED,
                    ErrorReason = "Wrong realm",
                    Nonce = GenerateNonce(),
                    Realm = _config.Realm
                };
            }

            //
            // Compute the HMAC key based on the authentication mode
            //
            byte[] hmacKey = ComputeHmacKey(username);

            if (hmacKey == null)
            {
                return new AuthResult
                {
                    IsAuthenticated = false,
                    ErrorCode = StunErrorCode.UNAUTHORIZED,
                    ErrorReason = "Invalid credentials",
                    Nonce = GenerateNonce(),
                    Realm = _config.Realm
                };
            }

            //
            // Validate MESSAGE-INTEGRITY
            //
            if (MessageIntegrity.ValidateIntegrity(rawBytes, hmacKey) == false)
            {
                return new AuthResult
                {
                    IsAuthenticated = false,
                    ErrorCode = StunErrorCode.UNAUTHORIZED,
                    ErrorReason = "Invalid MESSAGE-INTEGRITY",
                    Nonce = GenerateNonce(),
                    Realm = _config.Realm
                };
            }

            //
            // Success!
            //
            return new AuthResult
            {
                IsAuthenticated = true,
                Username = username,
                Realm = realm,
                Nonce = nonce2,
                IntegrityKey = hmacKey
            };
        }


        // ── Key Computation ───────────────────────────────────────────────


        /// <summary>
        /// Computes the HMAC key for the given username.
        ///
        /// For TURN REST API mode (shared secret configured):
        ///   - Username is "timestamp[:userId]"
        ///   - Validate that the timestamp hasn't expired
        ///   - Key = MD5( username : realm : HMAC-SHA1(secret, username) )
        ///
        /// Actually, for TURN REST API the key derivation is:
        ///   - password = Base64(HMAC-SHA1(sharedSecret, username))
        ///   - key = MD5(username:realm:password)
        /// </summary>
        private byte[] ComputeHmacKey(string username)
        {
            if (string.IsNullOrWhiteSpace(_config.SharedSecret) == false)
            {
                //
                // TURN REST API mode
                //
                return ComputeRestApiKey(username);
            }

            //
            // No shared secret configured — cannot authenticate
            //
            return null;
        }


        /// <summary>
        /// Computes the HMAC key for TURN REST API authentication.
        ///
        /// The username contains a Unix timestamp (and optionally a user ID).
        /// The password is HMAC-SHA1(sharedSecret, username), Base64-encoded.
        /// The HMAC key for MESSAGE-INTEGRITY is MD5(username:realm:password).
        /// </summary>
        private byte[] ComputeRestApiKey(string username)
        {
            //
            // Extract and validate the timestamp from the username
            //
            string timestampStr = username;
            int colonIndex = username.IndexOf(':');

            if (colonIndex > 0)
            {
                timestampStr = username.Substring(0, colonIndex);
            }

            if (long.TryParse(timestampStr, out long timestamp) == false)
            {
                return null;
            }

            //
            // Check that the credential hasn't expired
            //
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (timestamp < now)
            {
                return null;
            }

            //
            // Compute the password: Base64(HMAC-SHA1(sharedSecret, username))
            //
            byte[] secretBytes = Encoding.UTF8.GetBytes(_config.SharedSecret);
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);

            byte[] hmacHash;

            using (HMACSHA1 hmac = new HMACSHA1(secretBytes))
            {
                hmacHash = hmac.ComputeHash(usernameBytes);
            }

            string password = Convert.ToBase64String(hmacHash);

            //
            // The HMAC key for MESSAGE-INTEGRITY is MD5(username:realm:password)
            //
            return MessageIntegrity.ComputeLongTermKey(username, _config.Realm, password);
        }
    }
}
