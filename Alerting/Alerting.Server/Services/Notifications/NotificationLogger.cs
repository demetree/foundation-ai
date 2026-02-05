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
        
        // Configuration (set via Configure before first use)
        private static Logger.LogLevels _logLevel = Logger.LogLevels.Debug;
        private static bool _disableThrottling = true;

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
        /// Configures the notification logger with the specified options.
        /// Call this early in startup before any logging occurs.
        /// </summary>
        /// <param name="options">The notification engine options.</param>
        public static void Configure(NotificationEngineOptions options)
        {
            if (options == null) return;

            lock (_initLock)
            {
                // Parse log level from string
                _logLevel = options.LogLevel?.ToLowerInvariant() switch
                {
                    "debug" => Logger.LogLevels.Debug,
                    "information" => Logger.LogLevels.Information,
                    "warning" => Logger.LogLevels.Warning,
                    "error" => Logger.LogLevels.Error,
                    _ => Logger.LogLevels.Debug
                };

                _disableThrottling = options.DisableLogThrottling;

                // If already initialized, update the logger settings
                if (_initialized)
                {
                    _logger.Level = _logLevel;
                    if (_disableThrottling)
                    {
                        _logger.DisableThrottling();
                    }
                }
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

                    // Apply configured log level (or default)
                    _logger.Level = _logLevel;

                    // Apply throttling setting
                    if (_disableThrottling)
                    {
                        _logger.DisableThrottling();
                    }

                    _initialized = true;

                    // Log startup at System level
                    _logger.LogSystem($"NotificationEngine logger initialized (Level: {_logLevel})");
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

