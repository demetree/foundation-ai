//
// Log Error Notification Options
//
// Configuration options for the LogErrorNotificationConsumer.
// This class is placed in FoundationCore so it can be used by non-web applications.
//
// AI Generated - Feb 2026
//
using System.Collections.Generic;

namespace Foundation
{
    /// <summary>
    /// 
    /// Configuration options for the LogErrorNotificationConsumer.
    /// 
    /// </summary>
    public class LogErrorNotificationOptions
    {
        //
        /// <summary>
        /// 
        /// Overall enabled flag.  Defaults to false.
        /// 
        /// </summary>
        public bool Enabled { get; set; } = false;



        //
        // System Identification
        //

        /// <summary>
        /// Name of the system for identification in notifications.
        /// </summary>
        public string SystemName { get; set; } = "Unknown System";

        /// <summary>
        /// Environment name (e.g., Production, Staging, Development).
        /// </summary>
        public string Environment { get; set; } = "Production";


        //
        // Email Configuration
        //

        /// <summary>
        /// Whether to send email notifications.
        /// </summary>
        public bool EnableEmail { get; set; } = true;

        /// <summary>
        /// List of email addresses to notify.
        /// </summary>
        public List<string> NotificationEmails { get; set; } = new List<string>();

        /// <summary>
        /// Email from address. Falls back to SendGrid configuration if not set.
        /// </summary>
        public string EmailFromAddress { get; set; }

        /// <summary>
        /// Display name for the email sender.
        /// </summary>
        public string EmailFromName { get; set; } = "System Monitor";


        //
        // Alerting Integration Configuration
        //

        /// <summary>
        /// Whether to raise incidents in the Alerting system.
        /// </summary>
        public bool EnableAlerting { get; set; } = false;

        /// <summary>
        /// Severity level for alerts: "Critical", "High", "Medium", "Low", "Info".
        /// </summary>
        public string AlertingSeverity { get; set; } = "High";


        //
        // Batching Configuration
        //

        /// <summary>
        /// Number of minutes to suppress notifications after the first error.
        /// During this window, errors are accumulated and sent in a batch when the window expires.
        /// Default is 10 minutes.
        /// </summary>
        public int BatchWindowMinutes { get; set; } = 10;

        /// <summary>
        /// Maximum number of errors to include in a single batch notification.
        /// Older errors are dropped if this limit is exceeded.
        /// </summary>
        public int MaxErrorsPerBatch { get; set; } = 100;


        //
        // Log Level Filter
        //

        /// <summary>
        /// Minimum log level to capture. Default is Error (captures Exception and Error).
        /// </summary>
        public Logger.LogLevels MinimumLevel { get; set; } = Logger.LogLevels.Error;
    }
}
