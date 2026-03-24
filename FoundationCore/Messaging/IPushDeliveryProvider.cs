using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Messaging
{
    /// <summary>
    /// 
    /// Standard interface for external push delivery providers.
    /// 
    /// Implementors handle delivery to a specific external channel (email, SMS, webhook, etc.).
    /// The PushDeliveryService discovers all registered providers and dispatches requests
    /// to the appropriate one based on the ProviderId.
    /// 
    /// Built-in implementations:
    ///   - SmtpPushDeliveryProvider   : Email via SMTP
    ///   - SmsPushDeliveryProvider    : SMS via console stub (swap for Twilio, AWS SNS, etc.)
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 3B, March 2026.
    /// 
    /// </summary>
    public interface IPushDeliveryProvider
    {
        /// <summary>
        /// Unique identifier for this provider (e.g. "email", "sms", "teams").
        /// </summary>
        string ProviderId { get; }

        /// <summary>
        /// Human-readable display name.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Whether this provider is currently enabled and configured.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Delivers a notification to the specified destination.
        /// </summary>
        Task<PushDeliveryResult> DeliverAsync(PushDeliveryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates whether the destination string is a valid format for this provider.
        /// For example, email provider validates email format, SMS validates phone format.
        /// </summary>
        bool ValidateDestination(string destination);
    }


    /// <summary>
    /// Request payload for push delivery.
    /// </summary>
    public class PushDeliveryRequest
    {
        /// <summary>
        /// Destination address: email address, phone number, webhook URL, etc.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Notification subject line (for email).
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Plain-text body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// HTML body (optional — for email providers that support rich formatting).
        /// </summary>
        public string HtmlBody { get; set; }

        /// <summary>
        /// Display name of the sender.
        /// </summary>
        public string SenderDisplayName { get; set; }

        /// <summary>
        /// Name of the conversation (if the notification is related to a conversation).
        /// </summary>
        public string ConversationName { get; set; }

        /// <summary>
        /// ID of the source conversation (nullable).
        /// </summary>
        public int? ConversationId { get; set; }

        /// <summary>
        /// ID of the source message (nullable).
        /// </summary>
        public int? MessageId { get; set; }

        /// <summary>
        /// Tenant GUID for multi-tenant isolation.
        /// </summary>
        public Guid TenantGuid { get; set; }

        /// <summary>
        /// Recipient user ID.
        /// </summary>
        public int RecipientUserId { get; set; }

        /// <summary>
        /// Additional provider-specific metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }


    /// <summary>
    /// Result of a push delivery attempt.
    /// </summary>
    public class PushDeliveryResult
    {
        /// <summary>
        /// Whether the delivery was accepted by the external provider.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// External message ID assigned by the provider (for tracking/correlation).
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Error message if delivery failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The provider that handled this delivery.
        /// </summary>
        public string ProviderId { get; set; }


        /// <summary>
        /// Creates a success result.
        /// </summary>
        public static PushDeliveryResult Succeeded(string providerId, string externalId = null)
        {
            return new PushDeliveryResult
            {
                Success = true,
                ProviderId = providerId,
                ExternalId = externalId
            };
        }


        /// <summary>
        /// Creates a failure result.
        /// </summary>
        public static PushDeliveryResult Failed(string providerId, string errorMessage)
        {
            return new PushDeliveryResult
            {
                Success = false,
                ProviderId = providerId,
                ErrorMessage = errorMessage
            };
        }
    }
}
