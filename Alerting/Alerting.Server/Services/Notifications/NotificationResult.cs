//
// Notification Result Model
//
// Represents the outcome of a notification delivery attempt.
//
using System;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Result of a notification delivery attempt.
    /// </summary>
    public class NotificationResult
    {
        /// <summary>
        /// Whether the notification was successfully sent.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// External message ID from the provider (e.g., SendGrid message ID, Twilio SID).
        /// </summary>
        public string ExternalMessageId { get; set; }

        /// <summary>
        /// Error message if the notification failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Timestamp when the notification was attempted.
        /// </summary>
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The raw response from the provider (for debugging).
        /// </summary>
        public string ProviderResponse { get; set; }

        // Content archival fields for forensic auditing
        
        /// <summary>
        /// The recipient address (email, phone number, or device token).
        /// </summary>
        public string RecipientAddress { get; set; }

        /// <summary>
        /// The subject line (for email notifications).
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The full message body content that was sent.
        /// </summary>
        public string BodyContent { get; set; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static NotificationResult Succeeded(string externalMessageId = null, string providerResponse = null)
        {
            return new NotificationResult
            {
                Success = true,
                ExternalMessageId = externalMessageId,
                ProviderResponse = providerResponse
            };
        }

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        public static NotificationResult Failed(string errorMessage, string providerResponse = null)
        {
            return new NotificationResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                ProviderResponse = providerResponse
            };
        }
    }
}
