using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Foundation Call Provider Interface — defines the contract for all call providers.
    /// 
    /// Each provider (WebRTC, Azure ACS, etc.) implements this interface to handle
    /// call-specific operations while the CallService handles common orchestration.
    /// 
    /// This follows the same pluggable pattern as IPushDeliveryProvider.
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 4 (Calling), March 2026.
    /// 
    /// </summary>
    public interface ICallProvider
    {
        /// <summary>
        /// Unique provider identifier (e.g., "webrtc", "azure-acs").
        /// </summary>
        string ProviderId { get; }

        /// <summary>
        /// Human-readable display name.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Whether this provider is currently enabled and can accept calls.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Provider capabilities — the UI queries this to adapt the experience.
        /// </summary>
        CallProviderCapabilities Capabilities { get; }

        /// <summary>
        /// Initialize a call on the provider side (e.g., create a room, allocate resources).
        /// Called by CallService after creating the Call record.
        /// </summary>
        Task<CallProviderResult> InitializeCallAsync(CallInitRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Terminate a call on the provider side (e.g., destroy a room, release resources).
        /// Called by CallService when a call ends.
        /// </summary>
        Task<CallProviderResult> TerminateCallAsync(CallTerminateRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get provider-specific connection information for a participant.
        /// Returns ICE servers, access tokens, room URLs, etc.
        /// </summary>
        Task<CallConnectionInfo> GetConnectionInfoAsync(CallConnectionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates that the provider's configuration is correct (e.g., API keys are set).
        /// </summary>
        bool ValidateConfiguration();
    }


    #region Supporting Types


    /// <summary>
    /// Advertises what a provider can do — the UI uses this to show/hide features.
    /// </summary>
    public class CallProviderCapabilities
    {
        public bool SupportsVoice { get; set; } = true;
        public bool SupportsVideo { get; set; } = true;
        public bool SupportsScreenShare { get; set; } = false;
        public bool SupportsRecording { get; set; } = false;
        public bool SupportsGroupCalls { get; set; } = false;
        public int MaxParticipants { get; set; } = 2;
        public bool SupportsVoiceMessages { get; set; } = false;
        public bool RequsClientSideWebRTC { get; set; } = true;
    }


    /// <summary>
    /// Request to initialize a call on the provider.
    /// </summary>
    public class CallInitRequest
    {
        public int CallId { get; set; }
        public int ConversationId { get; set; }
        public int InitiatorUserId { get; set; }
        public List<int> ParticipantUserIds { get; set; }
        public string CallType { get; set; }
        public Guid TenantGuid { get; set; }
    }


    /// <summary>
    /// Request to terminate a call on the provider.
    /// </summary>
    public class CallTerminateRequest
    {
        public int CallId { get; set; }
        public string ProviderCallId { get; set; }
        public Guid TenantGuid { get; set; }
    }


    /// <summary>
    /// Result of a provider operation.
    /// </summary>
    public class CallProviderResult
    {
        public bool Success { get; set; }
        public string ProviderCallId { get; set; }
        public string ErrorMessage { get; set; }
    }


    /// <summary>
    /// Request for connection info for a specific user in a call.
    /// </summary>
    public class CallConnectionRequest
    {
        public int CallId { get; set; }
        public int UserId { get; set; }
        public Guid TenantGuid { get; set; }
        public string CallType { get; set; }
    }


    /// <summary>
    /// Connection info returned by the provider — everything the client needs to connect.
    /// </summary>
    public class CallConnectionInfo
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        /// <summary>
        /// ICE servers for WebRTC-based providers (STUN/TURN).
        /// Uses the IceServer class from ITurnServerProvider.
        /// </summary>
        public List<IceServer> IceServers { get; set; }

        /// <summary>
        /// Provider-specific access token (e.g., ACS token).
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Provider-specific room/session URL.
        /// </summary>
        public string RoomUrl { get; set; }

        /// <summary>
        /// Any additional provider-specific metadata the client might need.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }
    }


    #endregion
}
