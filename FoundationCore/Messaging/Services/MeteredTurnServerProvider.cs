using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Metered.ca TURN Server Provider — fetches STUN/TURN credentials from
    /// the Metered.ca REST API for WebRTC ICE negotiation.
    /// 
    /// Metered provides globally distributed TURN servers with a simple API:
    ///   GET https://{appName}.metered.live/api/v1/turn/credentials?apiKey={apiKey}
    /// 
    /// Returns an array of ICE server objects with temporary credentials.
    /// Credentials are cached briefly to avoid excessive API calls.
    /// 
    /// Configuration shape:
    /// 
    ///   "Calling": {
    ///     "Metered": {
    ///       "AppName": "your-app-name",
    ///       "ApiKey": "your-api-key"
    ///     }
    ///   }
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 4 (Calling), March 2026.
    /// 
    /// </summary>
    public class MeteredTurnServerProvider : ITurnServerProvider
    {
        private readonly ILogger<MeteredTurnServerProvider> _logger;
        private readonly HttpClient _httpClient;

        //
        // Config from appsettings.json
        //
        private readonly string _appName;
        private readonly string _apiKey;

        //
        // Credential cache — Metered credentials are valid for a while,
        // so we cache them to avoid hammering the API on every call.
        //
        private List<IceServer> _cachedServers;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);
        private readonly object _cacheLock = new object();


        public MeteredTurnServerProvider(
            IConfiguration configuration,
            ILogger<MeteredTurnServerProvider> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("MeteredTurn");

            var section = configuration.GetSection("Calling:Metered");

            _appName = section.GetValue<string>("AppName") ?? "";
            _apiKey = section.GetValue<string>("ApiKey") ?? "";
        }


        // ─── ITurnServerProvider ─────────────────────────────────────────────

        public bool IsEnabled => !string.IsNullOrEmpty(_appName) && !string.IsNullOrEmpty(_apiKey);


        public bool ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_appName))
            {
                _logger.LogWarning("MeteredTurnServerProvider: AppName is not configured");
                return false;
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("MeteredTurnServerProvider: ApiKey is not configured");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Performs a live health check against the Metered.ca API at startup.
        /// Logs prominent warnings if anything is wrong so misconfiguration
        /// is caught immediately rather than silently falling back to STUN.
        /// </summary>
        public async Task<bool> ValidateCredentialsAsync()
        {
            if (!ValidateConfiguration())
                return false;

            try
            {
                var url = $"https://{_appName}.metered.live/api/v1/turn/credentials?apiKey={_apiKey}";

                _logger.LogInformation("MeteredTurnServerProvider: Validating credentials against https://{AppName}.metered.live ...", _appName);

                var response = await _httpClient.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("╔══════════════════════════════════════════════════════════════╗");
                    _logger.LogError("║  METERED TURN SERVER: API KEY IS INVALID (HTTP 401)         ║");
                    _logger.LogError("║  Calls between different networks will NOT work!             ║");
                    _logger.LogError("║  Check Calling:Metered:ApiKey in appsettings.json            ║");
                    _logger.LogError("╚══════════════════════════════════════════════════════════════╝");
                    return false;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError("╔══════════════════════════════════════════════════════════════╗");
                    _logger.LogError("║  METERED TURN SERVER: APP NAME NOT FOUND (HTTP 404)         ║");
                    _logger.LogError("║  AppName '{AppName}' does not exist on Metered.ca            ║", _appName);
                    _logger.LogError("║  Check Calling:Metered:AppName in appsettings.json           ║");
                    _logger.LogError("╚══════════════════════════════════════════════════════════════╝");
                    return false;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var servers = System.Text.Json.JsonSerializer.Deserialize<List<MeteredIceServer>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                int turnCount = servers?.FindAll(s => s.Urls != null && s.Urls.StartsWith("turn")).Count ?? 0;

                _logger.LogInformation("MeteredTurnServerProvider: ✓ Credentials valid — {Count} ICE servers ({TurnCount} TURN relays)", servers?.Count ?? 0, turnCount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MeteredTurnServerProvider: Failed to validate credentials. TURN relay will not be available.");
                return false;
            }
        }


        /// <summary>
        /// Fetches ICE server credentials from the Metered.ca API.
        /// Results are cached for 30 minutes to reduce API calls.
        /// Falls back to public STUN if the API call fails.
        /// </summary>
        public async Task<List<IceServer>> GetIceServersAsync(CancellationToken cancellationToken = default)
        {
            //
            // Check cache first
            //
            lock (_cacheLock)
            {
                if (_cachedServers != null && DateTime.UtcNow < _cacheExpiry)
                {
                    return _cachedServers;
                }
            }


            //
            // Fetch fresh credentials from Metered API
            //
            try
            {
                var url = $"https://{_appName}.metered.live/api/v1/turn/credentials?apiKey={_apiKey}";

                _logger.LogInformation("MeteredTurnServerProvider: Fetching ICE servers from: https://{AppName}.metered.live/api/v1/turn/credentials", _appName);

                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var meteredServers = JsonSerializer.Deserialize<List<MeteredIceServer>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                //
                // Map Metered's response format to our IceServer model
                //
                var servers = new List<IceServer>();

                if (meteredServers != null)
                {
                    foreach (var ms in meteredServers)
                    {
                        servers.Add(new IceServer
                        {
                            Urls = new List<string> { ms.Urls },
                            Username = ms.Username,
                            Credential = ms.Credential,
                            CredentialType = !string.IsNullOrEmpty(ms.Credential) ? "password" : null
                        });
                    }
                }


                //
                // Cache the results
                //
                lock (_cacheLock)
                {
                    _cachedServers = servers;
                    _cacheExpiry = DateTime.UtcNow.Add(_cacheDuration);
                }

                _logger.LogInformation("MeteredTurnServerProvider: Fetched {Count} ICE servers from Metered.ca", servers.Count);

                return servers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MeteredTurnServerProvider: FAILED to fetch Metered credentials (AppName={AppName}). Falling back to STUN-only. Calls between different networks will NOT work!", _appName);

                //
                // Fallback: return public STUN so calls can still work peer-to-peer
                // (TURN relay won't be available but direct/STUN connections may succeed)
                //
                return new List<IceServer>
                {
                    new IceServer
                    {
                        Urls = new List<string> { "stun:stun.l.google.com:19302" }
                    }
                };
            }
        }


        // ─── Metered API Response DTO ────────────────────────────────────────

        /// <summary>
        /// Maps the JSON shape returned by Metered.ca's credential API.
        /// Each object has: urls (string), username (string?), credential (string?).
        /// </summary>
        private class MeteredIceServer
        {
            public string Urls { get; set; }
            public string Username { get; set; }
            public string Credential { get; set; }
        }
    }
}
