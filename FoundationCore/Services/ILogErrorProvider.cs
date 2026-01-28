//
// Log Error Provider Interface
//
// Enables applications to provide recent log errors for telemetry collection.
// The telemetry collector fetches errors since the last collection for historical analysis.
//
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    /// Interface for providers that supply recent log errors for telemetry collection.
    /// Implementations scan log files or other sources for errors since a given timestamp.
    /// </summary>
    public interface ILogErrorProvider
    {
        /// <summary>
        /// Gets log errors that occurred since the specified time.
        /// </summary>
        /// <param name="sinceUtc">Only return errors after this UTC timestamp</param>
        /// <returns>Collection of recent log error entries</returns>
        Task<IEnumerable<LogErrorEntry>> GetRecentErrorsAsync(DateTime sinceUtc);
    }


    /// <summary>
    /// Represents a single log error entry captured for telemetry
    /// </summary>
    public class LogErrorEntry
    {
        /// <summary>
        /// Timestamp when the error occurred (from log file)
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Log level (ERROR, FATAL, etc.)
        /// </summary>
        [JsonPropertyName("level")]
        public string Level { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Exception details if available
        /// </summary>
        [JsonPropertyName("exception")]
        public string Exception { get; set; }

        /// <summary>
        /// Name of the log file where this error was found
        /// </summary>
        [JsonPropertyName("logFileName")]
        public string LogFileName { get; set; }

        /// <summary>
        /// Optional logger name / source
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }


    /// <summary>
    /// Response model for recent errors endpoint
    /// </summary>
    public class RecentErrorsResponse
    {
        /// <summary>
        /// Collection of error entries
        /// </summary>
        [JsonPropertyName("errors")]
        public List<LogErrorEntry> Errors { get; set; } = new List<LogErrorEntry>();

        /// <summary>
        /// The timestamp from which errors were retrieved
        /// </summary>
        [JsonPropertyName("since")]
        public DateTime Since { get; set; }

        /// <summary>
        /// Count of errors returned
        /// </summary>
        [JsonPropertyName("count")]
        public int Count => Errors?.Count ?? 0;

        /// <summary>
        /// Whether the log directory was accessible
        /// </summary>
        [JsonPropertyName("logDirectoryAvailable")]
        public bool LogDirectoryAvailable { get; set; } = true;

        /// <summary>
        /// Error message if log parsing failed
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
