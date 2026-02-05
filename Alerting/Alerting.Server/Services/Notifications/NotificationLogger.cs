//
// Notification Logger
//
// Provides a dedicated log file for notification engine operations.
// Pattern matches SendGridEmailService logging approach.
//
using System;
using System.IO;
using System.Reflection;
using Foundation;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Static logger for the notification engine.
    /// Creates a dedicated log file 'NotificationEngine*.log' for all notification processing.
    /// </summary>
    public static class NotificationLogger
    {
        private const string LOG_DIRECTORY = "Log";
        private const string LOG_FILENAME = "NotificationEngine";

        private static readonly Logger _logger = new Logger();
        private static bool _initialized = false;
        private static readonly object _initLock = new object();

        /// <summary>
        /// Gets the shared logger instance for notification operations.
        /// </summary>
        public static Logger Logger
        {
            get
            {
                EnsureInitialized();
                return _logger;
            }
        }

        /// <summary>
        /// Initializes the logger with the appropriate directory and settings.
        /// Thread-safe and idempotent.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_initialized) return;

            lock (_initLock)
            {
                if (_initialized) return;

                try
                {
                    string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
                    _logger.SetDirectory(Path.Combine(currentPath, LOG_DIRECTORY));
                    _logger.SetFileName(LOG_FILENAME);

                    // Set to Debug level for verbose notification logging
                    // This allows debug messages to be logged when enabled
                    _logger.Level = Logger.LogLevels.Debug;

                    // Disable throttling for this logger to ensure all notification events are captured
                    _logger.DisableThrottling();

                    _initialized = true;

                    // Log startup at System level
                    _logger.LogSystem("NotificationEngine logger initialized");
                }
                catch (Exception ex)
                {
                    // Fallback - log to console if logger initialization fails
                    Console.WriteLine($"Failed to initialize NotificationLogger: {ex.Message}");
                }
            }
        }

        #region Convenience Methods

        /// <summary>
        /// Logs a debug message (verbose operational details).
        /// </summary>
        public static void Debug(string message)
        {
            Logger.LogDebug(message);
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        public static void Info(string message)
        {
            Logger.LogInformation(message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public static void Warning(string message)
        {
            Logger.LogWarning(message);
        }

        /// <summary>
        /// Logs a system-level message (startup, shutdown, significant events).
        /// </summary>
        public static void System(string message)
        {
            Logger.LogSystem(message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        public static void Error(string message)
        {
            Logger.LogError(message);
        }

        /// <summary>
        /// Logs an exception with full stack trace.
        /// </summary>
        public static void Exception(string message, Exception ex = null)
        {
            Logger.LogException(message, ex);
        }

        #endregion
    }
}
