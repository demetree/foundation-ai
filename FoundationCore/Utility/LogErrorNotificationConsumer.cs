//
// Log Error Notification Consumer
//
// Implements Logger.ILogConsumer to capture Exception/Error level log entries
// and send batched notifications via email and/or the Alerting system.
//
// Behavior:
//   1. On first error, sends an immediate notification
//   2. Suppresses further notifications for BatchWindowMinutes (default 10)
//   3. Accumulates errors during suppression window
//   4. Sends batched summary when window expires
//   5. Repeats
//
// AI Generated - Feb 2026
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation
{
    /// <summary>
    /// 
    /// A log consumer that monitors for Exception/Error level log entries
    /// and sends batched notifications via email and/or the Alerting system.
    /// 
    /// </summary>
    public class LogErrorNotificationConsumer : Logger.ILogConsumer, IDisposable
    {
        //
        // Constants
        //
        private const int TIMER_CHECK_INTERVAL_SECONDS = 60;
        private const int MAX_MESSAGE_LENGTH_FOR_EMAIL = 2000;


        //
        // Configuration
        //
        private readonly LogErrorNotificationOptions _options;


        //
        // Notification channels (injected delegates for flexibility)
        //
        private readonly Func<string, string, Task<bool>> _sendEmailAsync;
        private readonly Func<string, string, Task> _raiseAlertAsync;


        //
        // State
        //
        private readonly ConcurrentQueue<ErrorEntry> _pendingErrors = new ConcurrentQueue<ErrorEntry>();
        private DateTime _lastNotificationTime = DateTime.MinValue;
        private DateTime? _firstErrorInBatch = null;
        private readonly Timer _batchTimer;
        private bool _disposed = false;
        private readonly object _flushLock = new object();
        private bool _isFlushingInProgress = false;


        //
        // Internal record for captured errors
        //
        private record ErrorEntry(DateTime Timestamp,
                                  Logger.LogLevels Level,
                                  string Message,
                                  string ThreadName);


        /// <summary>
        /// 
        /// Creates a new LogErrorNotificationConsumer with the specified options and notification channels.
        /// 
        /// </summary>
        /// <param name="options">Configuration options.</param>
        /// <param name="sendEmailAsync">Optional email sender delegate: (subject, body) => success.</param>
        /// <param name="raiseAlertAsync">Optional alerting delegate: (title, description) => void.</param>
        public LogErrorNotificationConsumer(LogErrorNotificationOptions options,
                                            Func<string, string, Task<bool>> sendEmailAsync = null,
                                            Func<string, string, Task> raiseAlertAsync = null)
        {
            _options = options ?? new LogErrorNotificationOptions();
            _sendEmailAsync = sendEmailAsync;
            _raiseAlertAsync = raiseAlertAsync;

            //
            // Start background timer to check for batch window expiry
            //
            _batchTimer = new Timer(CheckBatchWindowExpiry,
                                    null,
                                    TimeSpan.FromSeconds(TIMER_CHECK_INTERVAL_SECONDS),
                                    TimeSpan.FromSeconds(TIMER_CHECK_INTERVAL_SECONDS));
        }


        //
        // Static Initialization Support (for use before DI or in console apps)
        //

        private static LogErrorNotificationConsumer _instance;
        private static readonly object _initLock = new object();


        /// <summary>
        /// 
        /// Initializes log error notification with programmatic options.
        /// Call this early in startup (after Logger is created) for notifications before DI builds.
        /// 
        /// </summary>
        /// <param name="options">Configuration options.</param>
        /// <param name="sendEmailAsync">Optional email sender delegate: (subject, body) => success.</param>
        /// <param name="raiseAlertAsync">Optional alerting delegate: (title, description) => void.</param>
        /// <returns>The initialized consumer instance.</returns>
        public static LogErrorNotificationConsumer Initialize(LogErrorNotificationOptions options,
                                                               Func<string, string, Task<bool>> sendEmailAsync = null,
                                                               Func<string, string, Task> raiseAlertAsync = null)
        {
            lock (_initLock)
            {
                if (_instance != null)
                {
                    Console.WriteLine("[LogErrorNotification] Already initialized. Ignoring duplicate initialization.");
                    return _instance;
                }

                _instance = new LogErrorNotificationConsumer(options, sendEmailAsync, raiseAlertAsync);

                //
                // Register as a global consumer to receive log entries from ALL loggers
                //
                Logger.AddGlobalConsumer(_instance);

                Console.WriteLine($"[LogErrorNotification] Initialized for {options.SystemName}. " +
                                  $"Email: {options.EnableEmail}, " +
                                  $"BatchWindow: {options.BatchWindowMinutes}min");

                return _instance;
            }
        }


        /// <summary>
        /// 
        /// Initializes log error notification with email-only support (most common use case).
        /// Uses SendGridEmailService for email delivery.
        /// 
        /// </summary>
        /// <param name="systemName">Name of the system (e.g., "Scheduler").</param>
        /// <param name="environment">Environment name (e.g., "Production").</param>
        /// <param name="notificationEmails">Email addresses to notify.</param>
        /// <param name="emailFromAddress">Sender email address.</param>
        /// <param name="emailFromName">Sender display name.</param>
        /// <returns>The initialized consumer instance.</returns>
        public static LogErrorNotificationConsumer InitializeWithEmail(string systemName,
                                                                        string environment,
                                                                        string[] notificationEmails,
                                                                        string emailFromAddress = null,
                                                                        string emailFromName = null)
        {
            LogErrorNotificationOptions options = new LogErrorNotificationOptions
            {
                SystemName = systemName ?? "Application",
                Environment = environment ?? "Unknown",
                EnableEmail = true,
                NotificationEmails = notificationEmails?.ToList() ?? new List<string>(),
                EmailFromAddress = emailFromAddress ?? string.Empty,
                EmailFromName = emailFromName ?? $"{systemName} Error Monitor"
            };

            Func<string, string, Task<bool>> emailSender = null;

            if (notificationEmails?.Length > 0)
            {
                emailSender = (subject, body) =>
                    Services.SendGridEmailService.SendEmailToMultipleRecipientsAsync(senderEmail: options.EmailFromAddress,
                                                                                      senderName: options.EmailFromName,
                                                                                      toEmails: options.NotificationEmails,
                                                                                      subject: subject,
                                                                                      body: body,
                                                                                      includeSignature: false,
                                                                                      bodyIsHtml: false);
            }

            return Initialize(options, emailSender, null);
        }


        /// <summary>
        /// 
        /// Gets the current singleton instance, or null if not initialized.
        /// 
        /// </summary>
        public static LogErrorNotificationConsumer Instance => _instance;


        /// <summary>
        /// 
        /// Shuts down the log error notification system.
        /// 
        /// </summary>
        public static void Shutdown()
        {
            lock (_initLock)
            {
                if (_instance != null)
                {
                    Logger.RemoveGlobalConsumer(_instance);
                    _instance.Dispose();
                    _instance = null;

                    Console.WriteLine("[LogErrorNotification] Shutdown complete.");
                }
            }
        }


        /// <summary>
        /// 
        /// Implementation of Logger.ILogConsumer.Log.
        /// Called by the Logger for each log entry - must be fast and non-blocking.
        /// 
        /// </summary>
        public void Log(DateTime timestamp, Logger.LogLevels level, string message, string threadName)
        {
            //
            // Only capture errors at or above the configured minimum level
            // Lower numeric value = higher severity (Exception=1, Error=2)
            //
            if (level > _options.MinimumLevel)
            {
                return;
            }

            //
            // Queue the error for batching
            //
            _pendingErrors.Enqueue(new ErrorEntry(timestamp, level, message, threadName));

            //
            // If this is the first error in a new window, trigger immediate notification
            //
            if (_firstErrorInBatch == null)
            {
                _firstErrorInBatch = DateTime.UtcNow;

                //
                // Fire-and-forget the notification - don't block the logger
                //
                Task.Run(() => FlushPendingErrorsAsync("Immediate"));
            }
        }


        /// <summary>
        /// 
        /// Timer callback to check if the batch window has expired.
        /// 
        /// </summary>
        private void CheckBatchWindowExpiry(object state)
        {
            if (_disposed == true)
            {
                return;
            }

            //
            // Check if we have pending errors and the window has expired
            //
            if (_firstErrorInBatch.HasValue == true && _pendingErrors.IsEmpty == false)
            {
                TimeSpan elapsed = DateTime.UtcNow - _firstErrorInBatch.Value;

                if (elapsed.TotalMinutes >= _options.BatchWindowMinutes)
                {
                    Task.Run(() => FlushPendingErrorsAsync("BatchWindow"));
                }
            }
        }


        /// <summary>
        /// 
        /// Flushes all pending errors and sends notifications.
        /// 
        /// </summary>
        /// <param name="trigger">Description of what triggered the flush (for logging).</param>
        private async Task FlushPendingErrorsAsync(string trigger)
        {
            //
            // Ensure only one flush is in progress at a time
            //
            lock (_flushLock)
            {
                if (_isFlushingInProgress == true)
                {
                    return;
                }

                _isFlushingInProgress = true;
            }

            try
            {
                //
                // Drain the queue
                //
                List<ErrorEntry> errors = new List<ErrorEntry>();

                while (_pendingErrors.TryDequeue(out ErrorEntry entry) == true && errors.Count < _options.MaxErrorsPerBatch)
                {
                    errors.Add(entry);
                }

                if (errors.Count == 0)
                {
                    return;
                }

                //
                // Build the notification content
                //
                string subject = BuildSubject(errors);
                string body = BuildBody(errors, trigger);
                string alertDescription = BuildAlertDescription(errors);

                //
                // Send email notification if configured
                //
                if (_options.EnableEmail == true && _sendEmailAsync != null)
                {
                    try
                    {
                        await _sendEmailAsync(subject, body).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        //
                        // Log to console - can't use the logger here to avoid recursion
                        //
                        Console.WriteLine($"[LogErrorNotificationConsumer] Failed to send email: {ex.Message}");
                    }
                }

                //
                // Raise alert if configured
                //
                if (_options.EnableAlerting == true && _raiseAlertAsync != null)
                {
                    try
                    {
                        await _raiseAlertAsync(subject, alertDescription).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LogErrorNotificationConsumer] Failed to raise alert: {ex.Message}");
                    }
                }

                //
                // Update state
                //
                _lastNotificationTime = DateTime.UtcNow;
                _firstErrorInBatch = null;
            }
            finally
            {
                lock (_flushLock)
                {
                    _isFlushingInProgress = false;
                }
            }
        }


        /// <summary>
        /// 
        /// Builds the email/alert subject line.
        /// 
        /// </summary>
        private string BuildSubject(List<ErrorEntry> errors)
        {
            DateTime oldest = errors.Min(e => e.Timestamp);

            int exceptionCount = errors.Count(e => e.Level == Logger.LogLevels.Exception);
            int errorCount = errors.Count(e => e.Level == Logger.LogLevels.Error);

            string levelSummary = string.Empty;

            if (exceptionCount > 0 && errorCount > 0)
            {
                levelSummary = $"{exceptionCount} exceptions, {errorCount} errors";
            }
            else if (exceptionCount > 0)
            {
                levelSummary = $"{exceptionCount} exception(s)";
            }
            else
            {
                levelSummary = $"{errorCount} error(s)";
            }

            return $"🚨 [{_options.SystemName}] Log Errors Detected - {levelSummary} since {oldest:h:mm tt}";
        }


        /// <summary>
        /// 
        /// Builds the email body with full error details.
        /// 
        /// </summary>
        private string BuildBody(List<ErrorEntry> errors, string trigger)
        {
            StringBuilder sb = new StringBuilder();

            //
            // Header
            //
            sb.AppendLine($"⚠️ Log Error Summary for {_options.SystemName}");
            sb.AppendLine();
            sb.AppendLine($"Environment: {_options.Environment}");

            DateTime oldest = errors.Min(e => e.Timestamp);
            DateTime newest = errors.Max(e => e.Timestamp);

            sb.AppendLine($"Time Window: {oldest:MMM d, yyyy h:mm:ss tt} - {newest:h:mm:ss tt}");
            sb.AppendLine($"Total Errors: {errors.Count}");
            sb.AppendLine($"Trigger: {trigger}");
            sb.AppendLine();
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine("ERROR DETAILS");
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine();

            //
            // Error details
            //
            foreach (ErrorEntry error in errors)
            {
                string levelTag = error.Level.ToString().ToUpperInvariant();
                string truncatedMessage = error.Message;

                //
                // Truncate very long messages for email readability
                //
                if (truncatedMessage.Length > MAX_MESSAGE_LENGTH_FOR_EMAIL)
                {
                    truncatedMessage = truncatedMessage.Substring(0, MAX_MESSAGE_LENGTH_FOR_EMAIL) + "... [truncated]";
                }

                sb.AppendLine($"[{error.Timestamp:HH:mm:ss.ffffff}] {levelTag} - {error.ThreadName}");
                sb.AppendLine(truncatedMessage);
                sb.AppendLine();
            }

            //
            // Footer
            //
            sb.AppendLine("───────────────────────────────────────────────────────────────");

            DateTime nextWindowEnd = DateTime.Now.AddMinutes(_options.BatchWindowMinutes);

            sb.AppendLine($"This is an automated notification. Next notification suppressed until {nextWindowEnd:h:mm tt}.");

            return sb.ToString();
        }


        /// <summary>
        /// 
        /// Builds a shorter description suitable for Alerting incidents.
        /// 
        /// </summary>
        private string BuildAlertDescription(List<ErrorEntry> errors)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Environment: {_options.Environment}");
            sb.AppendLine($"Total Errors: {errors.Count}");
            sb.AppendLine();

            //
            // Include first few errors only for alerting
            //
            int maxForAlert = Math.Min(5, errors.Count);

            for (int i = 0; i < maxForAlert; i++)
            {
                ErrorEntry error = errors[i];
                string shortMessage = error.Message.Length > 200 ? error.Message.Substring(0, 200) + "..." : error.Message;

                sb.AppendLine($"[{error.Timestamp:HH:mm:ss}] {error.Level}: {shortMessage}");
            }

            if (errors.Count > maxForAlert)
            {
                sb.AppendLine($"... and {errors.Count - maxForAlert} more errors");
            }

            return sb.ToString();
        }


        /// <summary>
        /// 
        /// Disposes the consumer and stops the background timer.
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_disposed == true)
            {
                return;
            }

            _disposed = true;
            _batchTimer?.Dispose();
        }
    }
}
