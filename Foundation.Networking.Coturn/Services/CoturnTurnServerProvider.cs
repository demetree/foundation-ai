// ============================================================================
//
// CoturnTurnServerProvider.cs — ITurnServerProvider for self-hosted coturn.
//
// Generates time-limited TURN REST API credentials using the shared secret
// mechanism described in the coturn documentation and the TURN REST API draft.
//
// Credential generation:
//   username = "<expiry_timestamp>:<optional_user_id>"
//   credential = Base64(HMAC-SHA1(shared_secret, username))
//
// This is the standard TURN REST API credential generation scheme supported
// by coturn (--use-auth-secret), Twilio, and other TURN providers.
//
// When the Catalyst messaging code merges into this repository, this class
// registers via DI and WebRtcCallProvider.GetConnectionInfoAsync() will
// return these credentials instead of falling back to Google public STUN.
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using Foundation.Networking.Coturn.Configuration;

namespace Foundation.Networking.Coturn.Services
{
    /// <summary>
    ///
    /// TURN server provider for self-hosted coturn instances.
    ///
    /// Implements the TURN REST API credential generation pattern where
    /// time-limited credentials are generated using HMAC-SHA1 with a shared
    /// secret that is also configured on the coturn server.
    ///
    /// </summary>
    public class CoturnTurnServerProvider
    {
        private readonly TurnServerOptions _options;


        public CoturnTurnServerProvider(TurnServerOptions options)
        {
            _options = options;
        }


        //
        // Whether this provider is enabled and configured
        //
        public bool IsEnabled
        {
            get
            {
                return _options != null && _options.Enabled == true;
            }
        }


        /// <summary>
        /// Gets the list of ICE servers (STUN and TURN) with time-limited credentials.
        ///
        /// Returns both STUN servers (no credentials needed) and TURN servers
        /// (with generated username/credential).
        /// </summary>
        public List<IceServerInfo> GetIceServers()
        {
            List<IceServerInfo> iceServerList = new List<IceServerInfo>();

            //
            // Add STUN servers (no credentials needed)
            //
            if (_options.StunUrls != null && _options.StunUrls.Count > 0)
            {
                IceServerInfo stunServer = new IceServerInfo
                {
                    Urls = new List<string>(_options.StunUrls)
                };

                iceServerList.Add(stunServer);
            }

            //
            // Add TURN servers with time-limited credentials
            //
            if (_options.TurnUrls != null && _options.TurnUrls.Count > 0)
            {
                string username = string.Empty;
                string credential = string.Empty;

                //
                // Generate the time-limited credential if we have a shared secret
                //
                if (string.IsNullOrWhiteSpace(_options.SharedSecret) == false)
                {
                    GenerateTimeLimitedCredential(_options.SharedSecret, _options.CredentialTtlSeconds, out username, out credential);
                }

                IceServerInfo turnServer = new IceServerInfo
                {
                    Urls = new List<string>(_options.TurnUrls),
                    Username = username,
                    Credential = credential,
                    CredentialType = "password"
                };

                iceServerList.Add(turnServer);
            }

            return iceServerList;
        }


        /// <summary>
        /// Validates that the provider has all required configuration values.
        /// </summary>
        public bool ValidateConfiguration()
        {
            if (_options == null)
            {
                return false;
            }

            //
            // Must have at least one TURN URL and a shared secret
            //
            if (_options.TurnUrls == null || _options.TurnUrls.Count == 0)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(_options.SharedSecret))
            {
                return false;
            }

            return true;
        }


        // ── Credential Generation ─────────────────────────────────────────


        /// <summary>
        /// Generates a time-limited TURN REST API credential.
        ///
        /// The username encodes the expiry timestamp.  The credential is the
        /// HMAC-SHA1 of the username using the shared secret, Base64-encoded.
        ///
        /// This is the standard mechanism described in the TURN REST API draft
        /// and supported by coturn's --use-auth-secret mode.
        /// </summary>
        public static void GenerateTimeLimitedCredential(string sharedSecret, int ttlSeconds, out string username, out string credential)
        {
            //
            // Calculate the credential expiry as a Unix timestamp
            //
            long expiryTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ttlSeconds;

            //
            // The username is the expiry timestamp.  Some implementations append
            // a user identifier after a colon (e.g., "timestamp:userId"), but
            // the bare timestamp format works with coturn's --use-auth-secret.
            //
            username = expiryTimestamp.ToString();

            //
            // The credential is HMAC-SHA1(shared_secret, username), Base64-encoded
            //
            byte[] keyBytes = Encoding.UTF8.GetBytes(sharedSecret);
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);

            using (HMACSHA1 hmac = new HMACSHA1(keyBytes))
            {
                byte[] hash = hmac.ComputeHash(usernameBytes);
                credential = Convert.ToBase64String(hash);
            }
        }


        /// <summary>
        /// Overload that includes a user identifier in the username.
        /// Format: "timestamp:userId"
        /// </summary>
        public static void GenerateTimeLimitedCredential(string sharedSecret, int ttlSeconds, string userId, out string username, out string credential)
        {
            long expiryTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ttlSeconds;

            //
            // Include the user ID in the username for logging/debugging
            //
            username = expiryTimestamp.ToString() + ":" + userId;

            byte[] keyBytes = Encoding.UTF8.GetBytes(sharedSecret);
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);

            using (HMACSHA1 hmac = new HMACSHA1(keyBytes))
            {
                byte[] hash = hmac.ComputeHash(usernameBytes);
                credential = Convert.ToBase64String(hash);
            }
        }
    }


    /// <summary>
    ///
    /// ICE server information for WebRTC connections.
    ///
    /// This mirrors the IceServer class from Foundation.Messaging.Services.
    /// When the Catalyst code merges, this class will be replaced by a direct
    /// reference to that interface's IceServer type.
    ///
    /// </summary>
    public class IceServerInfo
    {
        /// <summary>
        /// Server URLs (e.g., "stun:stun.example.com:3478", "turn:turn.example.com:3478").
        /// </summary>
        public List<string> Urls { get; set; }

        /// <summary>
        /// Username for TURN authentication (not needed for STUN).
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Credential for TURN authentication.
        /// </summary>
        public string Credential { get; set; }

        /// <summary>
        /// Credential type — typically "password".
        /// </summary>
        public string CredentialType { get; set; } = "password";
    }
}
