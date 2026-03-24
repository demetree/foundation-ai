using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// WebRTC Call Provider — signaling-only provider for peer-to-peer calls.
    /// 
    /// This provider does not route any media through the server.  All audio/video
    /// flows directly between browsers via WebRTC's RTCPeerConnection API.
    /// The server only relays signaling messages (SDP offers/answers and ICE candidates)
    /// via SignalR.
    /// 
    /// InitializeCallAsync and TerminateCallAsync are no-ops because there is no
    /// server-side media infrastructure to allocate/release.
    /// 
    /// GetConnectionInfoAsync delegates to ITurnServerProvider for ICE server
    /// configuration (STUN/TURN URLs and credentials).
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 4 (Calling), March 2026.
    /// 
    /// </summary>
    public class WebRtcCallProvider : ICallProvider
    {
        private readonly ITurnServerProvider _turnServerProvider;
        private readonly IConfiguration _configuration;


        public WebRtcCallProvider(IConfiguration configuration, ITurnServerProvider turnServerProvider = null)
        {
            _configuration = configuration;
            _turnServerProvider = turnServerProvider;
        }


        // ─── ICallProvider Identity ──────────────────────────────────────────

        public string ProviderId => "webrtc";

        public string DisplayName => "WebRTC (Peer-to-Peer)";

        public bool IsEnabled => true;

        public CallProviderCapabilities Capabilities => new CallProviderCapabilities
        {
            SupportsVoice = true,
            SupportsVideo = true,
            SupportsScreenShare = true,
            SupportsRecording = false,
            SupportsGroupCalls = true,
            MaxParticipants = 4,
            SupportsVoiceMessages = false,
            RequsClientSideWebRTC = true
        };


        // ─── Call Lifecycle ──────────────────────────────────────────────────

        /// <summary>
        /// No-op for WebRTC — there is no server-side media to allocate.
        /// The call record is managed by CallService; the provider just acknowledges.
        /// </summary>
        public Task<CallProviderResult> InitializeCallAsync(CallInitRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CallProviderResult
            {
                Success = true,
                ProviderCallId = $"webrtc-{request.CallId}"
            });
        }


        /// <summary>
        /// No-op for WebRTC — there is no server-side media to release.
        /// </summary>
        public Task<CallProviderResult> TerminateCallAsync(CallTerminateRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CallProviderResult
            {
                Success = true,
                ProviderCallId = request.ProviderCallId
            });
        }


        /// <summary>
        /// Returns ICE server configuration for the WebRTC peer connection.
        /// 
        /// If an ITurnServerProvider is registered, delegates to it for STUN/TURN
        /// servers and credentials.  Otherwise, falls back to Google's public STUN.
        /// </summary>
        public async Task<CallConnectionInfo> GetConnectionInfoAsync(CallConnectionRequest request, CancellationToken cancellationToken = default)
        {
            var iceServers = new List<IceServer>();

            if (_turnServerProvider != null && _turnServerProvider.IsEnabled)
            {
                //
                // Use the registered TURN provider for ICE server config
                //
                var providerServers = await _turnServerProvider.GetIceServersAsync(cancellationToken);

                if (providerServers != null)
                {
                    iceServers.AddRange(providerServers);
                }
            }
            else
            {
                //
                // Fallback: Google public STUN (no TURN — will fail behind symmetric NAT)
                //
                iceServers.Add(new IceServer
                {
                    Urls = new List<string>
                    {
                        "stun:stun.l.google.com:19302",
                        "stun:stun1.l.google.com:19302"
                    }
                });
            }

            return new CallConnectionInfo
            {
                Success = true,
                IceServers = iceServers,
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "provider", "webrtc" },
                    { "mode", "p2p" }
                }
            };
        }


        /// <summary>
        /// Validates that the provider is configured.
        /// WebRTC always works — it just needs a browser.  TURN config is optional.
        /// </summary>
        public bool ValidateConfiguration()
        {
            return true;
        }
    }
}
