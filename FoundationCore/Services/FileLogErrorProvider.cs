//
// File Log Error Provider
//
// Scans log files in the configured log directory for ERROR/FATAL level entries
// since a given timestamp. Used by telemetry collection to capture recent errors.
//
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    /// Scans log files in a directory for error-level entries.
    /// Expects log files with standard ISO timestamp format.
    /// </summary>
    public class FileLogErrorProvider : ILogErrorProvider
    {
        private readonly ILogger<FileLogErrorProvider> _logger;
        private readonly string _logDirectory;
        private readonly int _maxErrors;

        // Pattern matches common log formats:
        // [2026-01-27 12:34:56.789] [ERROR] Message...
        // 2026-01-27T12:34:56.789Z ERROR Message...
        private static readonly Regex LogLinePattern = new Regex(
            @"^(?:\[)?(?<timestamp>\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?)\]?\s*(?:\[)?(?<level>ERROR|FATAL|CRITICAL|ERR|FTL)\]?\s*(?<message>.*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);


        /// <summary>
        /// Create a file log error provider
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="logDirectory">Directory to scan for log files</param>
        /// <param name="maxErrors">Maximum number of errors to return (default 50)</param>
        public FileLogErrorProvider(
            ILogger<FileLogErrorProvider> logger,
            string logDirectory,
            int maxErrors = 50)
        {
            _logger = logger;
            _logDirectory = logDirectory;
            _maxErrors = maxErrors;
        }


        /// <summary>
        /// Get errors from log files that occurred since the specified time.
        /// </summary>
        public async Task<IEnumerable<LogErrorEntry>> GetRecentErrorsAsync(DateTime sinceUtc)
        {
            var errors = new List<LogErrorEntry>();

            if (string.IsNullOrEmpty(_logDirectory) || !Directory.Exists(_logDirectory))
            {
                _logger.LogDebug("Log directory not configured or does not exist: {Directory}", _logDirectory);
                return errors;
            }

            try
            {
                // Get log files modified since the target time
                var logFiles = Directory.GetFiles(_logDirectory, "*.log")
                    .Select(f => new FileInfo(f))
                    .Where(f => f.LastWriteTimeUtc >= sinceUtc.AddMinutes(-5)) // Small buffer for file write timing
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .ToList();

                foreach (var logFile in logFiles)
                {
                    if (errors.Count >= _maxErrors) break;

                    var fileErrors = await ScanFileForErrorsAsync(logFile.FullName, sinceUtc)
                        .ConfigureAwait(false);
                    
                    errors.AddRange(fileErrors.Take(_maxErrors - errors.Count));
                }

                return errors.OrderByDescending(e => e.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error scanning log files in {Directory}", _logDirectory);
                return errors;
            }
        }


        /// <summary>
        /// Scan a single log file for error entries since the given time.
        /// </summary>
        private async Task<List<LogErrorEntry>> ScanFileForErrorsAsync(string filePath, DateTime sinceUtc)
        {
            var errors = new List<LogErrorEntry>();
            var fileName = Path.GetFileName(filePath);

            try
            {
                // Read file with shared access (log file may be in use)
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);

                string line;
                LogErrorEntry currentError = null;
                var exceptionLines = new List<string>();

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    var match = LogLinePattern.Match(line);
                    if (match.Success)
                    {
                        // Save previous error if we have one
                        if (currentError != null)
                        {
                            currentError.Exception = exceptionLines.Any() 
                                ? string.Join(Environment.NewLine, exceptionLines) 
                                : null;
                            errors.Add(currentError);
                            currentError = null;
                            exceptionLines.Clear();
                        }

                        // Parse timestamp
                        if (TryParseTimestamp(match.Groups["timestamp"].Value, out var timestamp))
                        {
                            // Only include if after the since time
                            if (timestamp >= sinceUtc)
                            {
                                currentError = new LogErrorEntry
                                {
                                    Timestamp = timestamp,
                                    Level = match.Groups["level"].Value.ToUpperInvariant(),
                                    Message = match.Groups["message"].Value.Trim(),
                                    LogFileName = fileName
                                };
                            }
                        }
                    }
                    else if (currentError != null && !string.IsNullOrWhiteSpace(line))
                    {
                        // Continuation line (likely exception stacktrace)
                        exceptionLines.Add(line.TrimEnd());
                    }
                }

                // Don't forget the last error
                if (currentError != null)
                {
                    currentError.Exception = exceptionLines.Any() 
                        ? string.Join(Environment.NewLine, exceptionLines) 
                        : null;
                    errors.Add(currentError);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error reading log file {File}", filePath);
            }

            return errors;
        }


        /// <summary>
        /// Try to parse various timestamp formats from log files.
        /// </summary>
        private static bool TryParseTimestamp(string value, out DateTime result)
        {
            // Try ISO 8601 formats
            string[] formats = {
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss.fff",
                "yyyy-MM-ddTHH:mm:ss"
            };

            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, 
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result))
            {
                return true;
            }

            // Fallback to general parsing
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, 
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result))
            {
                return true;
            }

            result = default;
            return false;
        }
    }
}
