using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Coturn Server Provider — provides STUN/TURN server configuration for WebRTC.
    /// 
    /// Reads configuration from appsettings.json under "Calling:Coturn" and generates
    /// time-limited, HMAC-based ephemeral TURN credentials.
    /// 
    /// Coturn's ephemeral credential mechanism works like this:
    ///   - Username = "{expiry_timestamp}:{arbitrary_user_id}"
    ///   - Credential = Base64(HMAC-SHA1(shared_secret, username))
    ///   - The TURN server validates credentials using the same shared secret
    /// 
    /// This means credentials expire automatically and don't need to be revoked.
    /// 
    /// Configuration shape:
    /// 
    ///   "Calling": {
    ///     "Coturn": {
    ///       "StunUrls": ["stun:stun.l.google.com:19302"],
    ///       "TurnUrl": "turn:your-server.com:3478",
    ///       "TurnUrlTls": "turns:your-server.com:5349",
    ///       "SharedSecret": "your-shared-secret",
    ///       "CredentialTtlSeconds": 86400,
    ///       "Realm": "your-realm"
    ///     }
    ///   }
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 4 (Calling), March 2026.
    /// 
    /// </summary>
    public class CoturnServerProvider : ITurnServerProvider
    {
        private readonly IConfiguration _configuration;

        //
        // Config values cached from appsettings.json
        //
        private readonly string[] _stunUrls;
        private readonly string _turnUrl;
        private readonly string _turnUrlTls;
        private readonly string _sharedSecret;
        private readonly int _credentialTtlSeconds;
        private readonly string _realm;


        public CoturnServerProvider(IConfiguration configuration)
        {
            _configuration = configuration;

            var section = _configuration.GetSection("Calling:Coturn");

            _stunUrls = section.GetSection("StunUrls").Get<string[]>()
                ?? new[] { "stun:stun.l.google.com:19302" };

            _turnUrl = section.GetValue<string>("TurnUrl");
            _turnUrlTls = section.GetValue<string>("TurnUrlTls");
            _sharedSecret = section.GetValue<string>("SharedSecret");
            _credentialTtlSeconds = section.GetValue<int>("CredentialTtlSeconds", 86400);
            _realm = section.GetValue<string>("Realm", "");
        }


        // ─── ITurnServerProvider ─────────────────────────────────────────────

        public bool IsEnabled => true;

        public bool ValidateConfiguration()
        {
            //
            // STUN always works (public servers).
            // TURN requires a shared secret and URL.
            //
            if (!string.IsNullOrEmpty(_turnUrl) && string.IsNullOrEmpty(_sharedSecret))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns ICE server configuration with time-limited TURN credentials.
        /// 
        /// Always includes STUN servers (they don't need auth).
        /// Adds TURN servers only if a TurnUrl and SharedSecret are configured.
        /// </summary>
        public Task<List<IceServer>> GetIceServersAsync(CancellationToken cancellationToken = default)
        {
            var servers = new List<IceServer>();

            //
            // 1. STUN servers — always included, no credentials needed
            //
            if (_stunUrls != null && _stunUrls.Length > 0)
            {
                servers.Add(new IceServer
                {
                    Urls = new List<string>(_stunUrls)
                });
            }


            //
            // 2. TURN server — only if configured with shared secret
            //
            if (!string.IsNullOrEmpty(_turnUrl) && !string.IsNullOrEmpty(_sharedSecret))
            {
                var (username, credential) = GenerateEphemeralCredentials();

                var turnUrls = new List<string> { _turnUrl };

                //
                // Add TLS TURN URL if configured (for networks that block UDP)
                //
                if (!string.IsNullOrEmpty(_turnUrlTls))
                {
                    turnUrls.Add(_turnUrlTls);
                }

                servers.Add(new IceServer
                {
                    Urls = turnUrls,
                    Username = username,
                    Credential = credential,
                    CredentialType = "password"
                });
            }

            return Task.FromResult(servers);
        }


        // ─── Ephemeral Credential Generation ─────────────────────────────────

        /// <summary>
        /// Generates time-limited TURN credentials using the HMAC-SHA1 mechanism
        /// that Coturn supports natively.
        /// 
        /// Format:
        ///   username = "{unix_expiry}:{random_id}"
        ///   credential = Base64(HMAC-SHA1(shared_secret, username))
        /// 
        /// The TURN server must be configured with the same shared secret
        /// (--use-auth-secret --static-auth-secret=...).
        /// </summary>
        private (string username, string credential) GenerateEphemeralCredentials()
        {
            //
            // Expiry = current time + TTL
            //
            var unixExpiry = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + _credentialTtlSeconds;

            //
            // Username format expected by Coturn
            //
            var username = $"{unixExpiry}:foundation-caller";

            //
            // HMAC-SHA1 credential
            //
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_sharedSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(username));
                var credential = Convert.ToBase64String(hash);

                return (username, credential);
            }
        }
    }
}
