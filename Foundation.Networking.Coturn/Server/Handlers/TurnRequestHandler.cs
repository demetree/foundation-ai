// ============================================================================
//
// TurnRequestHandler.cs — Handles authenticated TURN requests.
//
// Processes the four core TURN request types:
//   - Allocate (0x0003)      — creates a new relay allocation
//   - Refresh (0x0004)       — refreshes or deletes an allocation
//   - CreatePermission (0x0008) — installs peer permissions
//   - ChannelBind (0x0009)   — binds a channel number to a peer address
//
// All requests require authentication via the long-term credential mechanism.
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Net;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Protocol;

namespace Foundation.Networking.Coturn.Server.Handlers
{
    /// <summary>
    ///
    /// Handles authenticated TURN requests: Allocate, Refresh,
    /// CreatePermission, and ChannelBind.
    ///
    /// </summary>
    public class TurnRequestHandler
    {
        private readonly TurnServerConfiguration _config;
        private readonly AllocationManager _allocationManager;
        private readonly TurnAuthenticator _authenticator;


        public TurnRequestHandler(
            TurnServerConfiguration config,
            AllocationManager allocationManager,
            TurnAuthenticator authenticator)
        {
            _config = config;
            _allocationManager = allocationManager;
            _authenticator = authenticator;
        }


        /// <summary>
        /// Processes a TURN request and returns the response bytes.
        /// Returns null if the message type is not handled.
        /// </summary>
        public byte[] HandleRequest(StunMessage request, byte[] rawBytes, IPEndPoint clientEndPoint, IPEndPoint serverEndPoint)
        {
            ushort method = StunMessageType.GetMethod(request.MessageType);

            switch (method)
            {
                case StunMessageType.METHOD_ALLOCATE:
                    return HandleAllocateRequest(request, rawBytes, clientEndPoint, serverEndPoint);

                case StunMessageType.METHOD_REFRESH:
                    return HandleRefreshRequest(request, rawBytes, clientEndPoint, serverEndPoint);

                case StunMessageType.METHOD_CREATE_PERMISSION:
                    return HandleCreatePermissionRequest(request, rawBytes, clientEndPoint, serverEndPoint);

                case StunMessageType.METHOD_CHANNEL_BIND:
                    return HandleChannelBindRequest(request, rawBytes, clientEndPoint, serverEndPoint);

                default:
                    return null;
            }
        }


        // ── Allocate ──────────────────────────────────────────────────────


        private byte[] HandleAllocateRequest(StunMessage request, byte[] rawBytes, IPEndPoint clientEndPoint, IPEndPoint serverEndPoint)
        {
            //
            // Authenticate
            //
            TurnAuthenticator.AuthResult auth = _authenticator.TryAuthenticate(request, rawBytes);

            if (auth.IsAuthenticated == false)
            {
                return BuildErrorResponse(request, auth);
            }

            //
            // Check REQUESTED-TRANSPORT — we only support UDP (17)
            //
            StunAttribute transportAttr = request.FindAttribute(StunAttributeType.REQUESTED_TRANSPORT);

            if (transportAttr == null || transportAttr.Value == null || transportAttr.Value.Length < 1)
            {
                return BuildErrorResponse(request, StunErrorCode.BAD_REQUEST, "Missing REQUESTED-TRANSPORT", auth.IntegrityKey);
            }

            if (transportAttr.Value[0] != StunConstants.TRANSPORT_UDP)
            {
                return BuildErrorResponse(request, StunErrorCode.UNSUPPORTED_TRANSPORT_PROTOCOL, "Only UDP relay is supported", auth.IntegrityKey);
            }

            //
            // Get requested lifetime (or use default)
            //
            int requestedLifetime = _config.DefaultLifetime;
            StunAttribute lifetimeAttr = request.FindAttribute(StunAttributeType.LIFETIME);

            if (lifetimeAttr != null)
            {
                requestedLifetime = (int)lifetimeAttr.ReadUInt32();
            }

            //
            // Create the allocation
            //
            FiveTuple fiveTuple = new FiveTuple(clientEndPoint, serverEndPoint, StunConstants.TRANSPORT_UDP);
            int errorCode;

            TurnAllocation allocation = _allocationManager.TryCreateAllocation(
                fiveTuple, auth.Username, auth.Realm, auth.Nonce, auth.IntegrityKey, requestedLifetime, out errorCode);

            if (allocation == null)
            {
                string reason = "Allocation failed";

                if (errorCode == StunErrorCode.ALLOCATION_MISMATCH) reason = "Allocation mismatch";
                else if (errorCode == StunErrorCode.ALLOCATION_QUOTA_REACHED) reason = "Allocation quota reached";
                else if (errorCode == StunErrorCode.INSUFFICIENT_CAPACITY) reason = "Insufficient capacity";

                return BuildErrorResponse(request, errorCode, reason, auth.IntegrityKey);
            }

            //
            // Build the success response
            //
            IPEndPoint relayedAddress = new IPEndPoint(
                IPAddress.Parse(_config.ExternalAddress), allocation.RelayPort);

            StunMessage response = new StunMessage();
            response.MessageType = StunMessageType.ALLOCATE_SUCCESS_RESPONSE;
            response.TransactionId = new byte[StunConstants.TRANSACTION_ID_SIZE];
            Array.Copy(request.TransactionId, response.TransactionId, StunConstants.TRANSACTION_ID_SIZE);

            //
            // XOR-RELAYED-ADDRESS — the relay address peers will send data to
            //
            response.AddAttribute(
                StunAttribute.CreateXorRelayedAddress(relayedAddress, response.TransactionId));

            //
            // LIFETIME — the granted lifetime
            //
            response.AddAttribute(StunAttribute.CreateLifetime((uint)allocation.LifetimeSeconds));

            //
            // XOR-MAPPED-ADDRESS — the client's server-reflexive address
            //
            response.AddAttribute(
                StunAttribute.CreateXorMappedAddress(clientEndPoint, response.TransactionId));

            //
            // SOFTWARE
            //
            if (string.IsNullOrWhiteSpace(_config.Software) == false)
            {
                response.AddAttribute(StunAttribute.CreateSoftware(_config.Software));
            }

            return EncodeWithIntegrityAndFingerprint(response, auth.IntegrityKey);
        }


        // ── Refresh ───────────────────────────────────────────────────────


        private byte[] HandleRefreshRequest(StunMessage request, byte[] rawBytes, IPEndPoint clientEndPoint, IPEndPoint serverEndPoint)
        {
            //
            // Authenticate
            //
            TurnAuthenticator.AuthResult auth = _authenticator.TryAuthenticate(request, rawBytes);

            if (auth.IsAuthenticated == false)
            {
                return BuildErrorResponse(request, auth);
            }

            //
            // Find the existing allocation
            //
            FiveTuple fiveTuple = new FiveTuple(clientEndPoint, serverEndPoint, StunConstants.TRANSPORT_UDP);
            TurnAllocation allocation = _allocationManager.FindAllocation(fiveTuple);

            if (allocation == null)
            {
                return BuildErrorResponse(request, StunErrorCode.ALLOCATION_MISMATCH, "No allocation for this 5-tuple", auth.IntegrityKey);
            }

            //
            // Get the requested lifetime
            //
            int requestedLifetime = allocation.LifetimeSeconds;
            StunAttribute lifetimeAttr = request.FindAttribute(StunAttributeType.LIFETIME);

            if (lifetimeAttr != null)
            {
                requestedLifetime = (int)lifetimeAttr.ReadUInt32();
            }

            //
            // Lifetime 0 means delete the allocation
            //
            if (requestedLifetime == 0)
            {
                _allocationManager.RemoveAllocation(fiveTuple);
                requestedLifetime = 0;
            }
            else
            {
                //
                // Clamp and refresh
                //
                if (requestedLifetime > _config.MaxLifetime)
                {
                    requestedLifetime = _config.MaxLifetime;
                }

                allocation.Refresh(requestedLifetime);
            }

            //
            // Build success response
            //
            StunMessage response = new StunMessage();
            response.MessageType = StunMessageType.REFRESH_SUCCESS_RESPONSE;
            response.TransactionId = new byte[StunConstants.TRANSACTION_ID_SIZE];
            Array.Copy(request.TransactionId, response.TransactionId, StunConstants.TRANSACTION_ID_SIZE);

            response.AddAttribute(StunAttribute.CreateLifetime((uint)requestedLifetime));

            if (string.IsNullOrWhiteSpace(_config.Software) == false)
            {
                response.AddAttribute(StunAttribute.CreateSoftware(_config.Software));
            }

            return EncodeWithIntegrityAndFingerprint(response, auth.IntegrityKey);
        }


        // ── CreatePermission ──────────────────────────────────────────────


        private byte[] HandleCreatePermissionRequest(StunMessage request, byte[] rawBytes, IPEndPoint clientEndPoint, IPEndPoint serverEndPoint)
        {
            //
            // Authenticate
            //
            TurnAuthenticator.AuthResult auth = _authenticator.TryAuthenticate(request, rawBytes);

            if (auth.IsAuthenticated == false)
            {
                return BuildErrorResponse(request, auth);
            }

            //
            // Find the allocation
            //
            FiveTuple fiveTuple = new FiveTuple(clientEndPoint, serverEndPoint, StunConstants.TRANSPORT_UDP);
            TurnAllocation allocation = _allocationManager.FindAllocation(fiveTuple);

            if (allocation == null)
            {
                return BuildErrorResponse(request, StunErrorCode.ALLOCATION_MISMATCH, "No allocation for this 5-tuple", auth.IntegrityKey);
            }

            //
            // Extract all XOR-PEER-ADDRESS attributes and install permissions
            //
            List<StunAttribute> peerAddresses = request.FindAllAttributes(StunAttributeType.XOR_PEER_ADDRESS);

            if (peerAddresses.Count == 0)
            {
                return BuildErrorResponse(request, StunErrorCode.BAD_REQUEST, "Missing XOR-PEER-ADDRESS", auth.IntegrityKey);
            }

            foreach (StunAttribute peerAttr in peerAddresses)
            {
                IPEndPoint peerEndPoint = StunAttribute.ParseXorMappedAddress(peerAttr.Value, request.TransactionId);

                if (peerEndPoint != null)
                {
                    allocation.AddOrRefreshPermission(peerEndPoint.Address);
                }
            }

            //
            // Build success response (no body needed for CreatePermission)
            //
            StunMessage response = new StunMessage();
            response.MessageType = StunMessageType.CREATE_PERMISSION_SUCCESS_RESPONSE;
            response.TransactionId = new byte[StunConstants.TRANSACTION_ID_SIZE];
            Array.Copy(request.TransactionId, response.TransactionId, StunConstants.TRANSACTION_ID_SIZE);

            if (string.IsNullOrWhiteSpace(_config.Software) == false)
            {
                response.AddAttribute(StunAttribute.CreateSoftware(_config.Software));
            }

            return EncodeWithIntegrityAndFingerprint(response, auth.IntegrityKey);
        }


        // ── ChannelBind ───────────────────────────────────────────────────


        private byte[] HandleChannelBindRequest(StunMessage request, byte[] rawBytes, IPEndPoint clientEndPoint, IPEndPoint serverEndPoint)
        {
            //
            // Authenticate
            //
            TurnAuthenticator.AuthResult auth = _authenticator.TryAuthenticate(request, rawBytes);

            if (auth.IsAuthenticated == false)
            {
                return BuildErrorResponse(request, auth);
            }

            //
            // Find the allocation
            //
            FiveTuple fiveTuple = new FiveTuple(clientEndPoint, serverEndPoint, StunConstants.TRANSPORT_UDP);
            TurnAllocation allocation = _allocationManager.FindAllocation(fiveTuple);

            if (allocation == null)
            {
                return BuildErrorResponse(request, StunErrorCode.ALLOCATION_MISMATCH, "No allocation for this 5-tuple", auth.IntegrityKey);
            }

            //
            // Extract CHANNEL-NUMBER
            //
            StunAttribute channelAttr = request.FindAttribute(StunAttributeType.CHANNEL_NUMBER);

            if (channelAttr == null || channelAttr.Value == null || channelAttr.Value.Length < 2)
            {
                return BuildErrorResponse(request, StunErrorCode.BAD_REQUEST, "Missing CHANNEL-NUMBER", auth.IntegrityKey);
            }

            ushort channelNumber = System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(channelAttr.Value);

            if (TurnChannel.IsValidChannelNumber(channelNumber) == false)
            {
                return BuildErrorResponse(request, StunErrorCode.BAD_REQUEST, "Invalid channel number", auth.IntegrityKey);
            }

            //
            // Extract XOR-PEER-ADDRESS
            //
            StunAttribute peerAttr = request.FindAttribute(StunAttributeType.XOR_PEER_ADDRESS);

            if (peerAttr == null)
            {
                return BuildErrorResponse(request, StunErrorCode.BAD_REQUEST, "Missing XOR-PEER-ADDRESS", auth.IntegrityKey);
            }

            IPEndPoint peerEndPoint = StunAttribute.ParseXorMappedAddress(peerAttr.Value, request.TransactionId);

            if (peerEndPoint == null)
            {
                return BuildErrorResponse(request, StunErrorCode.BAD_REQUEST, "Invalid XOR-PEER-ADDRESS", auth.IntegrityKey);
            }

            //
            // Bind the channel
            //
            bool success = allocation.AddOrRefreshChannel(channelNumber, peerEndPoint);

            if (success == false)
            {
                return BuildErrorResponse(request, StunErrorCode.BAD_REQUEST, "Channel binding conflict", auth.IntegrityKey);
            }

            //
            // Build success response
            //
            StunMessage response = new StunMessage();
            response.MessageType = StunMessageType.CHANNEL_BIND_SUCCESS_RESPONSE;
            response.TransactionId = new byte[StunConstants.TRANSACTION_ID_SIZE];
            Array.Copy(request.TransactionId, response.TransactionId, StunConstants.TRANSACTION_ID_SIZE);

            if (string.IsNullOrWhiteSpace(_config.Software) == false)
            {
                response.AddAttribute(StunAttribute.CreateSoftware(_config.Software));
            }

            return EncodeWithIntegrityAndFingerprint(response, auth.IntegrityKey);
        }


        // ── Helpers ───────────────────────────────────────────────────────


        /// <summary>
        /// Encodes a response with MESSAGE-INTEGRITY and FINGERPRINT.
        /// </summary>
        private byte[] EncodeWithIntegrityAndFingerprint(StunMessage response, byte[] integrityKey)
        {
            byte[] encoded = response.Encode();
            encoded = MessageIntegrity.AppendIntegrity(encoded, integrityKey);
            encoded = MessageIntegrity.AppendFingerprint(encoded);

            return encoded;
        }


        /// <summary>
        /// Builds an error response from an AuthResult (includes REALM and NONCE for 401/438).
        /// </summary>
        private byte[] BuildErrorResponse(StunMessage request, TurnAuthenticator.AuthResult auth)
        {
            StunMessage response = StunMessage.CreateErrorResponse(request, auth.ErrorCode, auth.ErrorReason);

            //
            // Include REALM and NONCE on 401 and 438 errors
            //
            if (auth.ErrorCode == StunErrorCode.UNAUTHORIZED || auth.ErrorCode == StunErrorCode.STALE_NONCE)
            {
                if (string.IsNullOrEmpty(auth.Realm) == false)
                {
                    response.AddAttribute(StunAttribute.CreateRealm(auth.Realm));
                }

                if (string.IsNullOrEmpty(auth.Nonce) == false)
                {
                    response.AddAttribute(StunAttribute.CreateNonce(auth.Nonce));
                }
            }

            if (string.IsNullOrWhiteSpace(_config.Software) == false)
            {
                response.AddAttribute(StunAttribute.CreateSoftware(_config.Software));
            }

            byte[] encoded = response.Encode();
            encoded = MessageIntegrity.AppendFingerprint(encoded);

            return encoded;
        }


        /// <summary>
        /// Builds an error response with integrity (for authenticated errors after initial auth).
        /// </summary>
        private byte[] BuildErrorResponse(StunMessage request, int errorCode, string reason, byte[] integrityKey)
        {
            StunMessage response = StunMessage.CreateErrorResponse(request, errorCode, reason);

            if (string.IsNullOrWhiteSpace(_config.Software) == false)
            {
                response.AddAttribute(StunAttribute.CreateSoftware(_config.Software));
            }

            return EncodeWithIntegrityAndFingerprint(response, integrityKey);
        }
    }
}
