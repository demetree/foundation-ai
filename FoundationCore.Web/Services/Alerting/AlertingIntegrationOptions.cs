//
// Alerting Integration Options
//
// Configuration options for the Alerting integration client.
//
using System;

namespace Foundation.Web.Services.Alerting
{
    /// <summary>
    /// Configuration options for connecting to the Alerting service.
    /// </summary>
    public class AlertingIntegrationOptions
    {
        /// <summary>
        /// Configuration section name in appsettings.json.
        /// </summary>
        public const string SectionName = "Alerting";

        /// <summary>
        /// Base URL of the Alerting service (e.g., "https://alerting.mycompany.com").
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// API key for authenticated requests. Obtained via self-registration.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Name of this service when registering with Alerting.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// URL of this service (for Alerting to display in incident details).
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// URL where Alerting should send webhook callbacks.
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Path to file where API key should be stored after registration.
        /// If not set, the key will only be logged (not recommended for production).
        /// </summary>
        public string ApiKeyFilePath { get; set; }

        /// <summary>
        /// HTTP timeout for Alerting API calls in seconds. Default: 30.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Number of retry attempts for failed API calls. Default: 3.
        /// </summary>
        public int RetryCount { get; set; } = 3;
    }
}
