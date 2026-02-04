//
// Foundation.Logging — Modern, Safe, Scalable Logger
//
// Refactored by Demetree with Grok/Chat GPT/Gemini input — Nov 2025
//
// Interface unchanged, but implementation optimized and reorganized for efficiency.  
//
// - Replaced previous timer based log flushing with better Channel based solution
// - Added retention policy of 30 days
// - Refactored rate limiting into it's own class
// - Separated log writing into it's own service
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Foundation.Concurrent;

namespace Foundation
{
    /// <summary>
    /// 
    /// The Core Logger
    /// 
    /// </summary>
    public sealed class Logger : ILoggerProvider, ILogger, IDisposable
    {
        /// <summary>
        /// 
        /// Consumer interface to be implemented by objects that want a log feed.
        /// 
        /// </summary>
        public interface ILogConsumer
        {
            void Log(DateTime timestamp, Logger.LogLevels level, string message, string threadName);
        }


        internal record LogEntry(DateTime Timestamp,
                                 Logger.LogLevels Level,
                                 string ThreadName,
                                 string Message,
                                 Logger SourceLogger);


        public enum LogLevels
        {
            /// <summary>
            /// 
            /// Use this to disable logging output entirely by setting the logger's level to this.
            /// 
            /// Messages cannot actually be logged at this level.
            /// 
            /// </summary>
            None = 0,

            /// <summary>
            /// 
            /// Most significant Log Level that represents an unexpected condition in the system
            ///
            /// - It may or may not be gracefully handled, and potentially records the stopping of the system entirely.
            /// 
            ///      - The whole system should be wrapped in a try-catch that will catch unexpected .Net Exceptions, and create a log message at this level prior to terminating
            ///          ** These conditions should be trapped in try-catch, and then logged at the level that is most appropriate for their role in the system, as it is unlikely **
            ///
            ///      - Try-catch blocks should be used liberally throughout the business logic be able to gracefully handle System.Exception messages raised by the platform, as most libraries use 'System.Exception' derived objects to trap normal conditions, such as network connection timeouts.
            ///          A network timeout is likely not worth reporting as an Exception in our systems.  It could be logged as an error if truly unexpected, or even lower if network availability isn't guaranteed in the workflow.
            ///
            ///      - System.Exception events thrown by the platform may be unanticipated by the business logic, and therefore represent a flaw in the business logic.  For example 'array index out of bounds' as a low level logic error.
            ///          ** These should be logged at the Exception level **
            ///          ** Problems like these should be addressed by finding the root cause, and addressing it with a code fix. **
            ///          
            /// ** Developers of the business logic should create log events at this level with the mindset that when an even occurs at this level, there are no other options that the business logic has, and that its basically finished.
            ///          
            /// </summary>
            Exception = 1,


            /// <summary>
            /// 
            /// 2nd most significant Log Level that represents a problem during processing was encountered that deviates from the expected outcome and that has a material impact on the system.  
            /// 
            /// It may require notification or intervention, but doesn't immediately stop the system as whole.
            /// 
            /// For example - Disk out of space or some other resource that stops the system from doing what needs to be done.
            ///             - Unable to communicate with network, and it stops the system from doing what needs to be done.
            ///             - Denied access to some expected resource that can't be gracefully handled.
            /// 
            /// ** Developers of the business logic should create log events at this level with the mindset that the logged information will be immediately useful when investigating probems or reviewing operations .
            /// 
            /// </summary>
            Error = 2,

            /// <summary>
            /// 
            /// 3rd most significant log level.  It represents that an event occurred that is noteworthy to system functioning at a high level that should be noted, but perhaps not immediately communicated.
            /// 
            /// For example - System is starting
            ///             - Subsystem is starting
            ///             - Subsystem is stopping
            ///             - System is stopping
            ///             
            /// ** Developers of the business logic should create log events at this level with the mindset that the logged information will be useful when recording the system fundamentals.
            /// 
            /// </summary>
            System = 3,


            /// <summary>
            /// 
            /// 4th most significant log level. It represents that an event occurred that is unexpected or perhaps not on the happy path to system functioning level that should be noted, and may or may not need to be immediately communicated.
            /// 
            /// ** THIS LOG LEVEL IS THE MOST LIKELY LEVEL TO RUN A DEFAULT SYSTEM AT **
            /// 
            /// For example - Time to do some work is greater than expected
            ///             - The limit to some finite resource is being reached
            ///             
            /// ** Developers of the business logic should create log events at this level with the mindset that the logged information will be useful when an initial look at the operation of the system is going on **
            ///             
            /// </summary>
            Warning = 4,


            /// <summary>
            /// 
            /// 5th most significant log level.  It represents that an event occurred that is expected during system operation, and is worth filing when more detailed information is desired to retain.
            /// 
            /// For example - Opening resource to do something with
            ///             - sending X to Y
            ///             
            /// ** Developers of the business logic should create log events at this level with the mindset that the logged information will be useful when a closer look at the operation of the system is going on **
            ///             
            /// </summary>
            Information = 5,


            /// <summary>
            /// 
            /// 6th most significant log level.  It represents the recording of a detailed step in an event occurred that is expected during system operation, and is worth filing when EVEN MORE detailed information is desired to retain.
            ///  
            /// ** This level of detail is likely only useful to developers or QA/Testers as a resource to find the root cause of a problem by trying to understand the processing that occurred. **
            /// 
            /// ** Developers of the business logic should create log events at this level with the mindset that the logged information will be useful to solve problems later by having the program explain itself and the decisions that it is making **
            /// 
            /// For example - Starting processing loop to perform function X with parameters Y and context is Z
            /// 
            /// </summary>
            Debug = 6,


            /// <summary>
            /// 
            /// 7th most significant log level.  It represents the recording of a VERY detailed step in an event occurred that is expected during system operation, and is worth filing when THE MOST detailed information is desired to retain
            ///  
            /// ** This level of detail is likely only useful to developers or QA/Testers as a resource to find the root cause of a problem by trying to understand the processing that occurred, in a forensic reconstruction type scenario. **
            /// 
            /// ** Developers of the business logic should create log events at this level with the mindset that the logged information will be useful to solve problems later by having the program fully document its state, with all significant data 
            ///    and the decisions that it is making at an ABSURDLY  high level of detail that is only generally useful when all other options are exhausted.
            /// 
            ///    Log at this level only when the level of detail is technically valid for extreme case reconstruction of scenarios.
            ///    
            ///    When logging at this level, it is OK to do it in with tons of data, and/or within complex loops because the data is to be used for full state reconstruction in areas that may benefit from it
            ///    
            ///    ** Most scenarios don't need this level of details **
            ///    
            /// For example - Read content of 01234567890DESFRGHJKL from serial port 3 for sensor ABCDEFG   ( and log this at the fully frequency of data collection)
            ///             - Sent data of 'WU8RWU7R482784802794R8729' to network end point of ABCDEFG
            ///             - Send of data took 8493284 microseconds
            /// 
            /// </summary>
            Trace = 7
        }

        /// <summary>
        /// 
        /// Simple log file details class used by the functions that report on the existing log files.
        /// 
        /// </summary>
        public class LogFileDetails
        {
            public string fileName { get; set; }
            public long fileSize { get; set; }
            public DateTime lastModified { get; set; }
        }


        private static Logger _commonLogger;

        // To hold a a reference to all loggers created.
        private static ConcurrentList<Logger> _allLoggers = new ConcurrentList<Logger>();

        // Global consumers that receive log entries from ALL loggers (for system-wide notification)
        private static readonly ConcurrentList<ILogConsumer> _globalConsumers = new ConcurrentList<ILogConsumer>();

        internal static List<Logger> Loggers
        {
            get { return _allLoggers.ToList(); }
        }

        private static readonly object _commonLoggerLock = new();



        /// <summary>
        /// 
        /// These are basic state for a logger instance
        /// 
        /// </summary>
        private string _fileName = "logfile";
        private string _directory = string.Empty;
        private LogLevels _logLevel = LogLevels.Information;
        private LogLevels _consoleWriteLevel = LogLevels.Information;
        private bool _disposed;


        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        public string Directory
        {
            get
            {
                return _directory;
            }
            set
            {
                try
                {
                    //
                    // Make sure that the directory exists.
                    //
                    System.IO.Directory.CreateDirectory(value);

                    _directory = value;
                }
                catch
                {
                    //
                    // Fall back to the application current directory as a last resort.  Write permission here is assumed.
                    //
                    _directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                }
            }
        }


        public LogLevels Level
        {
            get
            {
                return _logLevel;
            }
            set
            {
                _logLevel = value;
            }
        }



        /// <summary>
        /// 
        /// The minimum level of log message that will be written to the console, when running the console output option
        /// 
        /// </summary>
        public LogLevels ConsoleWriteLevel
        {
            get
            {
                return _consoleWriteLevel;
            }
            set
            {
                _consoleWriteLevel = value;
            }
        }

        // Rate limiting per level
        private readonly Dictionary<LogLevels, RateLimiter> _limiters = new();

        // Real-time consumers
        private readonly ConcurrentList<ILogConsumer> _consumers = new();


        public Logger()
        {
            //
            // Default log directory is the a Log folder under the current assembly path
            //
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";

            _directory = Path.GetFullPath(System.IO.Path.Combine(currentPath, "Log"));

            // Make sure that it exists.
            System.IO.Directory.CreateDirectory(_directory);

            // Default rate limits - same as your original, but now configurable per instance
            foreach (LogLevels lvl in Enum.GetValues(typeof(LogLevels)))
            {
                int max = lvl switch
                {
                    LogLevels.Exception or LogLevels.Error or LogLevels.System => -1,
                    LogLevels.Warning or LogLevels.Information => 50,
                    LogLevels.Debug => 100,
                    LogLevels.Trace => 200,
                    _ => 0
                };

                _limiters[lvl] = new RateLimiter(max);
            }

            // Start the global drain service and retention service on first logger creation
            if (_commonLogger == null)
            {
                _ = LogDrainService.Writer; // Touch static to force initialization
                _ = LogRetentionService.Instance;  // Start retention
            }


            // Put this logger in the list of all loggers
            _allLoggers.Add(this);
        }


        public static void SetCommonLogger(Logger logger)
        {
            lock (_commonLoggerLock)
            {
                if (logger == null)
                {
                    throw new ArgumentNullException(nameof(logger));
                }

                _commonLogger = logger;
            }
        }


        public static Logger GetCommonLogger()
        {
            if (_commonLogger != null)
            {
                return _commonLogger;
            }

            lock (_commonLoggerLock)
            {
                if (_commonLogger == null)
                {
                    _commonLogger = new Logger();
                }

                return _commonLogger;
            }
        }


        public void SetMaxMessagesPerSecond(LogLevels level, int max)
        {
            // Anything less than 0 becomes full throttle
            if (max < 0)
            {
                max = -1;
            }

            _limiters[level] = new RateLimiter(max);
        }


        public void DisableThrottling()
        {
            foreach (Logger.LogLevels logLevel in _limiters.Keys.ToArray())
            {
                _limiters[logLevel].Disable();
            }
        }


        public void AddConsumer(ILogConsumer consumer)
        {
            _consumers.Add(consumer);
        }


        public void RemoveConsumer(ILogConsumer consumer)
        {
            _consumers.Remove(consumer);
        }


        /// <summary>
        /// 
        /// Adds a global consumer that receives log entries from ALL loggers.
        /// Use this for system-wide notification services.
        /// 
        /// </summary>
        public static void AddGlobalConsumer(ILogConsumer consumer)
        {
            _globalConsumers.Add(consumer);
        }


        /// <summary>
        /// 
        /// Removes a global consumer.
        /// 
        /// </summary>
        public static void RemoveGlobalConsumer(ILogConsumer consumer)
        {
            _globalConsumers.Remove(consumer);
        }


        internal void DistributeToConsumer(LogEntry entry)
        {
            // Fire-and-forget — never block the writer
            // First, notify instance-specific consumers
            foreach (ILogConsumer consumer in _consumers)
            {
                try
                {
                    consumer.Log(entry.Timestamp, entry.Level, entry.Message, entry.ThreadName);
                }
                catch
                {
                    /* consumer threw — ignore */
                }
            }

            // Then, notify global consumers (for system-wide notification services)
            foreach (ILogConsumer consumer in _globalConsumers)
            {
                try
                {
                    consumer.Log(entry.Timestamp, entry.Level, entry.Message, entry.ThreadName);
                }
                catch
                {
                    /* consumer threw — ignore */
                }
            }
        }


        //
        // Public strongly-typed logging methods
        //
        public void LogException(string message, Exception ex = null)
        {
            Log(message, LogLevels.Exception);

            if (ex != null)
            {
                Log(ex.ToString(), LogLevels.Exception);
            }

            //
            // For exceptions only, always put them onto the console
            //
            Console.WriteLine(message);

            if (ex != null)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        //
        // Log Level specific helper functions
        //
        public void LogError(string message) => Log(message, LogLevels.Error);
        public void LogSystem(string message) => Log(message, LogLevels.System);
        public void LogWarning(string message) => Log(message, LogLevels.Warning);
        public void LogInformation(string message) => Log(message, LogLevels.Information);
        public void LogDebug(string message) => Log(message, LogLevels.Debug);
        public void LogTrace(string message) => Log(message, LogLevels.Trace);


        private void Log(string message, LogLevels logLevelForMessage)
        {
            //
            // Exit if the log message level is none, or if the message is at a higher numeric level that what we are configured to log at.
            //
            if (logLevelForMessage == LogLevels.None || logLevelForMessage > _logLevel)
            {
                return;
            }

            //
            // Note - not using UTC here for local log time writing so that it's easier for the log reader.
            //
            DateTime now = DateTime.Now;

            // Rate limiting
            if (_limiters[logLevelForMessage].AllowAndMaybeReport(now, logLevelForMessage, out string report))
            {
                if (report != null)
                {
                    Enqueue(report, logLevelForMessage); // Report suppression at same level
                }

                Enqueue(message, logLevelForMessage);
            }

            //
            // Console output if level for log message is within the console write level range
            //
            if (logLevelForMessage <= _consoleWriteLevel)
            {
                Console.WriteLine($"{now:HH:mm:ss.ffffff}-{logLevelForMessage.ToStringFast()}-{Thread.CurrentThread.Name ?? "Thread"}-{message}");
            }
        }


        private void Enqueue(string message, LogLevels level)
        {
            LogEntry entry = new LogEntry(DateTime.Now,
                                          level,
                                          Thread.CurrentThread.Name ?? "Unnamed",
                                          message,
                                          this);

            LogDrainService.Writer.TryWrite(entry);
        }


        public bool IsEnabled(LogLevels levelToCheck)
        {
            if (_logLevel >= levelToCheck)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 
        /// Converts a log level string to a log level with validation
        /// 
        /// </summary>
        /// <param name="logLevelString"></param>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public static bool LogLevelFromString(string logLevelString, out LogLevels logLevel)
        {
            logLevel = LogLevels.None;

            if (logLevelString == null)
            {
                return false;
            }

            if (LogLevelExtensions.TryParse(logLevelString, out logLevel) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        #region Microsoft.Extensions.Logging integration

        ILogger ILoggerProvider.CreateLogger(string categoryName)
        {
            return this;
        }


        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }


        bool ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            LogLevels mappedLogLevel = Map(logLevel);

            if (Level >= mappedLogLevel)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        void ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel,
                                 EventId eventId,
                                 TState state,
                                 Exception exception,
                                 Func<TState, Exception,
                                 string> formatter)
        {
            LogLevels mappedLogLevel = Map(logLevel);

            if (Level < mappedLogLevel)
            {
                return;
            }

            string msg = string.Empty;

            if (formatter != null)
            {
                msg = formatter.Invoke(state, exception);

                if (msg == null)
                {
                    if (state != null)
                    {
                        msg = state.ToString();
                    }
                    else
                    {
                        msg = string.Empty;
                    }
                }
            }
            else
            {
                if (state != null)
                {
                    msg = state.ToString();
                }
            }

            if (string.IsNullOrEmpty(msg) == false)
            {
                Log(msg, mappedLogLevel);
            }

            if (exception != null)
            {
                Log(exception.ToString(), LogLevels.Exception);
            }
        }


        private static LogLevels Map(Microsoft.Extensions.Logging.LogLevel lvl) => lvl switch
        {
            Microsoft.Extensions.Logging.LogLevel.Critical => LogLevels.System,
            Microsoft.Extensions.Logging.LogLevel.Error => LogLevels.Error,
            Microsoft.Extensions.Logging.LogLevel.Warning => LogLevels.Warning,
            Microsoft.Extensions.Logging.LogLevel.Information => LogLevels.Information,
            Microsoft.Extensions.Logging.LogLevel.Debug => LogLevels.Debug,
            Microsoft.Extensions.Logging.LogLevel.Trace => LogLevels.Trace,
            _ => LogLevels.Debug
        };


        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance = new();
            public void Dispose() { }
        }

        #endregion

        public string GetDirectory()
        {
            return _directory;
        }


        /// <summary>
        /// 
        /// This returns a list of object representing the sub files in log folder. 
        /// 
        /// Only the file names are returned.  Add the path back in to access the file using the GetDirectory method.
        /// 
        /// </summary>
        /// <returns></returns>
        public List<LogFileDetails> GetLogFileDetails()
        {
            try
            {
                EnumerationOptions eo = new EnumerationOptions();

                string[] logFiles = System.IO.Directory.GetFiles(GetDirectory(), "*.log");


                List<LogFileDetails> output = new List<LogFileDetails>();

                foreach (string logFile in logFiles)
                {
                    LogFileDetails fileDetails = new LogFileDetails();


                    fileDetails.fileName = System.IO.Path.GetFileName(logFile);
                    fileDetails.fileSize = new System.IO.FileInfo(logFile).Length;
                    fileDetails.lastModified = new FileInfo(logFile).LastWriteTimeUtc;

                    output.Add(fileDetails);
                }

                return output;
            }
            catch (Exception)
            {
                //
                // Fail with an empty list so user doesn't need to null check.
                //
                return new List<LogFileDetails>();
            }
        }


        //
        // Can be removed later - keeping in for now to not break interface
        //
        public void SetDirectory(string directory)
        {
            Directory = directory;
        }

        //
        // Can be removed later - keeping in for now to not break interface
        //
        public void SetFileName(string fileName)
        {
            _fileName = fileName;
        }


        /// <summary>
        /// 
        /// *** Be very sure that you want to call this, because this will stop all logging ***
        /// 
        /// </summary>
        public static void TerminateApplicationLogging()
        {
            LogDrainService.Shutdown();
        }


        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // Only the common logger owns the pipeline shutdown
            if (ReferenceEquals(this, _commonLogger))
            {
                TerminateApplicationLogging();
            }
        }
    }


    internal sealed class LogDrainService
    {
        private const int DRAIN_DICTIONARY_CAPACITY = 8;
        private const int LOGGER_SHUTDOWN_TIMEOUT_SECONDS = 3;

        private static DateTime _lastCleanup = DateTime.MinValue;
        private const int LOG_RETENTION_DAYS = 30;

        private static readonly Channel<Logger.LogEntry> _channel = Channel.CreateUnbounded<Logger.LogEntry>(new UnboundedChannelOptions { SingleWriter = false, SingleReader = true });
        private static readonly Task _drainTask;
        private static readonly CancellationTokenSource _shutdownCts = new CancellationTokenSource();

        public static ChannelWriter<Logger.LogEntry> Writer
        {
            get
            {
                return _channel.Writer;
            }
        }


        /// <summary>
        ///
        /// Starts the drain loop as the time of service construction
        ///
        /// </summary>
        static LogDrainService()
        {
            _drainTask = Task.Run(() => DrainLoopAsync(_shutdownCts.Token));
        }


        /// <summary>
        /// 
        /// This watches the log entry channel and writes the data to file in batcehs.
        /// 
        /// </summary>
        /// <returns></returns>
        private static async Task DrainLoopAsync(CancellationToken cancellationToken)
        {
            ChannelReader<Logger.LogEntry> reader = _channel.Reader;
            Dictionary<Logger, List<Logger.LogEntry>> batches = new Dictionary<Logger, List<Logger.LogEntry>>(DRAIN_DICTIONARY_CAPACITY);

            try
            {
                while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (reader.TryRead(out Logger.LogEntry entry))
                    {
                        if (batches.TryGetValue(entry.SourceLogger, out List<Logger.LogEntry> list) == false)
                        {
                            list = new List<Logger.LogEntry>(256);
                            batches[entry.SourceLogger] = list;
                        }

                        list.Add(entry);
                    }

                    foreach (KeyValuePair<Logger, List<Logger.LogEntry>> kvp in batches)
                    {
                        await WriteBatchAsync(kvp.Key, kvp.Value, cancellationToken);
                    }

                    batches.Clear();
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown - try to flush whatever is left in the channel
                while (reader.TryRead(out Logger.LogEntry entry))
                {
                    if (batches.TryGetValue(entry.SourceLogger, out List<Logger.LogEntry> list) == false)
                    {
                        list = new List<Logger.LogEntry>(256);
                        batches[entry.SourceLogger] = list;
                    }

                    list.Add(entry);
                }

                // Best-effort flush without cancellation
                foreach (KeyValuePair<Logger, List<Logger.LogEntry>> kvp in batches)
                {
                    try
                    {
                        await WriteBatchAsync(kvp.Key, kvp.Value, CancellationToken.None)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Logger.GetCommonLogger().LogWarning(
                            $"[DrainLoopAsync] Failed to flush batch during shutdown: {ex.Message}. Details: {ex.InnerException}");
                    }
                }
            }
            catch (Exception ex)
            {
                // This is a real error, not shutdown-related. Do not swallow it silently.
                Logger.GetCommonLogger().LogError($"[Logger] Unexpected error in DrainLoopAsync: {ex.Message}. Details: {ex.InnerException}");
            }


            Console.WriteLine("[LogDrainService] Channel completed - shutting down.");
        }


        private static async Task WriteBatchAsync(Logger logger, List<Logger.LogEntry> entries, CancellationToken cancelationToken)
        {
            if (entries.Count == 0)
            {
                return;
            }

            string directory = logger.Directory;
            string fileName = $"{logger.FileName}_{DateTime.Now:yyyy-MM-dd}.log";
            string fullPath = Path.Combine(directory, fileName);

            try
            {
                Directory.CreateDirectory(directory);

                await using FileStream stream = new FileStream(fullPath,
                                                               FileMode.Append,
                                                               FileAccess.Write,
                                                               FileShare.ReadWrite,
                                                               bufferSize: 81920,
                                                               useAsync: true);

                await using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, 81920);

                foreach (Logger.LogEntry entry in entries)
                {
                    // Format: HH:mm:ss.ffffff-LEVEL-ThreadName-Message
                    string line = $"{entry.Timestamp:HH:mm:ss.ffffff}-{entry.Level.ToStringFast()}-{entry.ThreadName}-{entry.Message}";

                    await writer.WriteLineAsync(line.AsMemory(), cancelationToken).ConfigureAwait(false);

                    // Forward to real-time consumers (fire-and-forget)
                    logger.DistributeToConsumer(entry);
                }

                await writer.FlushAsync(cancelationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when ((ex is OperationCanceledException) == false)
            {
                //
                // Disk full, permission denied, network gone — we lose messages but stay alive
                //
                Console.WriteLine($"[Logger] Failed to write to {fullPath}: {ex.Message}");
            }
        }


        /// <summary>
        /// 
        /// A way to shut down cleanly if needed
        /// 
        /// </summary>
        internal static async Task ShutdownAsync(CancellationToken cancellationToken)
        {
            // Stop accepting new messages
            _channel.Writer.TryComplete();

            // Ask the drain loop to stop as soon as it finishes current work
            _shutdownCts.Cancel();

            using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdownCts.Token);

            try
            {
                await _drainTask.WaitAsync(linked.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Best-effort shutdown
            }
        }

        internal static void Shutdown()
        {
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(LOGGER_SHUTDOWN_TIMEOUT_SECONDS));

            ShutdownAsync(cts.Token).GetAwaiter().GetResult();
        }
    }


    internal sealed class LogRetentionService
    {
        //
        // This is a good start point, as today we have nothing, but we can add configuration later if it merits it.
        //
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(3);       // Run every three hours.  This could be longer, but I think this is reasonable.
        private static readonly TimeSpan DefaultRetention = TimeSpan.FromDays(30);      // Keep 30 days
        private static readonly int MaxFilesToDeletePerRun = 100;                       // Safety limit

        private static readonly Task _retentionTask;

        //
        // Stub that does nothing, just to start task with.
        //
        public static object Instance { get; internal set; }


        static LogRetentionService()
        {
            _retentionTask = Task.Run(RetentionLoopAsync);
        }


        private static async Task RetentionLoopAsync()
        {
            //
            // Main retention loop.  Execute on startup after 1 minute to let all logs get registered, and then delay for the regular interval.
            //
            await Task.Delay(TimeSpan.FromMinutes(1));      // initial delay to let any loggers come online.

            while (true)
            {
                try
                {
                    await CleanupOldLogsAsync();

                    await Task.Delay(CleanupInterval);
                }
                catch (Exception ex)
                {
                    // This MUST never crash — it's fire-and-forget
                    Console.WriteLine($"[LogRetentionService] Error during cleanup: {ex}");
                }
            }
        }


        private static async Task CleanupOldLogsAsync()
        {
            DateTime cutoff = DateTime.Now - DefaultRetention;
            int deletedCount = 0;

            //
            // Go through each logger and delete old files.
            //
            foreach (Logger logger in Logger.Loggers)
            {
                if (deletedCount >= MaxFilesToDeletePerRun)
                {
                    break;
                }

                string directory = logger.Directory;

                if (System.IO.Directory.Exists(directory) == false)
                {
                    continue;
                }

                IEnumerable<FileInfo> files = System.IO.Directory.GetFiles(directory, $"{logger.FileName}*.log")
                                                                 .Select(f => new FileInfo(f))
                                                                 .Where(f => f.CreationTime < cutoff || f.LastWriteTime < cutoff)
                                                                 .OrderBy(f => f.CreationTime)
                                                                 .Take(MaxFilesToDeletePerRun - deletedCount);

                foreach (FileInfo file in files)
                {
                    try
                    {
                        file.Delete();

                        deletedCount++;

                        //
                        // Write deleteion message to common and specific logger.
                        //
                        logger.LogSystem($"Deleted old log file: {file.Name}");

                        if (logger != Logger.GetCommonLogger())
                        {
                            Logger.GetCommonLogger().LogSystem($"Deleted old log file: {file.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.GetCommonLogger().LogWarning($"Failed to delete log file {file.Name}: {ex.Message}");
                    }
                }
            }

            if (deletedCount > 0)
            {
                Logger.GetCommonLogger().LogInformation($"Log cleanup completed. Deleted {deletedCount} old log files.");
            }
        }
    }


    /// <summary>
    /// 
    /// Rate Limiter helper class - separated from core logging function
    /// 
    /// </summary>
    internal sealed class RateLimiter
    {
        private readonly object _lock = new();

        private int _maxPerSecond; // -1 = unlimited
        private readonly double _fullThrottleSeconds;
        private readonly double _resetAfterInactivitySeconds;

        private int _countThisSecond;
        private int _suppressedThisSecond;
        private long _totalAttempted;

        private DateTime _firstMessage = DateTime.MinValue;
        private DateTime _lastMessage = DateTime.MinValue;
        private DateTime _lastReset = DateTime.MinValue;
        private bool _fullThrottleActive = true;


        public RateLimiter(int maxPerSecond = -1, double fullThrottleSeconds = 10.0, double resetInactivitySeconds = 30.0)
        {
            if (maxPerSecond < 0)
            {
                _maxPerSecond = -1;
            }
            else
            {
                _maxPerSecond = maxPerSecond;
            }


            if (fullThrottleSeconds > 0)
            {
                _fullThrottleSeconds = fullThrottleSeconds;
            }
            else
            {
                _fullThrottleSeconds = 10;
            }

            if (resetInactivitySeconds > 0)
            {
                _resetAfterInactivitySeconds = resetInactivitySeconds;
            }
            else
            {
                _resetAfterInactivitySeconds = 30.0;
            }
        }


        public bool AllowAndMaybeReport(DateTime now, Logger.LogLevels level, out string? reportMessage)
        {
            lock (_lock)
            {
                reportMessage = null;

                if (_maxPerSecond == -1)
                {
                    _totalAttempted++;
                    _lastMessage = now;
                    return true;
                }

                // First message ever?
                if (_firstMessage == DateTime.MinValue)
                {
                    _firstMessage = _lastMessage = _lastReset = now;
                    _countThisSecond = 1;
                    _totalAttempted = 1;
                    return true;
                }

                // Inactivity reset?
                if ((now - _lastMessage).TotalSeconds >= _resetAfterInactivitySeconds &&
                    _fullThrottleActive == false)
                {
                    ResetFullThrottle(now);
                    reportMessage = $"Rate limiting reset for {level} due to {(now - _lastMessage).TotalSeconds:F1}s inactivity";
                    _countThisSecond = 1;
                    _totalAttempted++;
                    _lastMessage = now;
                    return true;
                }

                // Second boundary?
                if ((now - _lastReset).TotalSeconds >= 1.0)
                {
                    if (_suppressedThisSecond > 0)
                    {
                        double currentRate = (_countThisSecond + _suppressedThisSecond) / (now - _lastReset).TotalSeconds;
                        reportMessage = $"Rate limiting suppressed {_suppressedThisSecond} {level} messages " +
                                       $"(current {currentRate:F1}/sec, max {_maxPerSecond})";
                    }

                    _countThisSecond = 0;
                    _suppressedThisSecond = 0;
                    _lastReset = now;

                    // End of full-throttle burst?
                    if (_fullThrottleActive && (now - _firstMessage).TotalSeconds > _fullThrottleSeconds)
                    {
                        _fullThrottleActive = false;
                    }
                }

                // Full throttle burst still active?
                if (_fullThrottleActive && (now - _firstMessage).TotalSeconds <= _fullThrottleSeconds)
                {
                    _countThisSecond++;
                    _totalAttempted++;
                    _lastMessage = now;
                    return true;
                }

                // Normal rate limiting
                if (_countThisSecond < _maxPerSecond)
                {
                    _countThisSecond++;
                    _totalAttempted++;
                    _lastMessage = now;
                    return true;
                }

                // Suppressed
                _suppressedThisSecond++;
                _totalAttempted++;
                _lastMessage = now;

                return false;
            }
        }


        private void ResetFullThrottle(DateTime now)
        {
            _fullThrottleActive = true;
            _firstMessage = now;
            _countThisSecond = 0;
            _suppressedThisSecond = 0;
            _totalAttempted = 0;
        }


        public void Disable()
        {
            lock (_lock)
            {
                _maxPerSecond = -1;
            }
        }
    }


    public static class LogLevelExtensions
    {
        private static readonly string[] NAMES = { "None", "Exception", "Error", "System", "Warning", "Information", "Debug", "Trace" };

        public static string ToStringFast(this Logger.LogLevels level)
        {
            return NAMES[(int)level];
        }

        public static bool TryParse(string s, out Logger.LogLevels level)
        {
            for (int i = 0; i < NAMES.Length; i++)
            {
                if (string.Equals(NAMES[i], s, StringComparison.OrdinalIgnoreCase))
                {
                    level = (Logger.LogLevels)i;
                    return true;
                }
            }

            level = Logger.LogLevels.Information;

            return false;
        }
    }
}