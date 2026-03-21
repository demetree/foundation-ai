// ============================================================================
//
// ThreatLog.cs — Audit trail of blocked and flagged requests.
//
// Maintains a rolling in-memory log of all firewall actions for the
// admin dashboard to display.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Foundation.Networking.Skynet.Configuration;

namespace Foundation.Networking.Skynet.Logging
{
    /// <summary>
    /// A single threat log entry.
    /// </summary>
    public class ThreatLogEntry
    {
        /// <summary>
        /// Source IP address.
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Request path.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Country code (from GeoIP), if available.
        /// </summary>
        public string CountryCode { get; set; } = string.Empty;

        /// <summary>
        /// The action taken: "Blocked" or "RateLimited".
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// The rule name that triggered the action.
        /// </summary>
        public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// Reason for the action.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// When the event occurred.
        /// </summary>
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }


    /// <summary>
    /// Summary statistics for the threat log.
    /// </summary>
    public class ThreatLogSummary
    {
        /// <summary>
        /// Total entries in the log.
        /// </summary>
        public int TotalEntries { get; set; }

        /// <summary>
        /// Number of blocked requests.
        /// </summary>
        public int BlockedCount { get; set; }

        /// <summary>
        /// Number of rate-limited requests.
        /// </summary>
        public int RateLimitedCount { get; set; }

        /// <summary>
        /// Top offending IPs with their block counts.
        /// </summary>
        public List<IpBlockCount> TopOffenders { get; set; } = new List<IpBlockCount>();

        /// <summary>
        /// Most targeted paths.
        /// </summary>
        public List<PathBlockCount> TopTargetedPaths { get; set; } = new List<PathBlockCount>();
    }


    /// <summary>
    /// IP address with its block count.
    /// </summary>
    public class IpBlockCount
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Count { get; set; }
    }


    /// <summary>
    /// Path with its block count.
    /// </summary>
    public class PathBlockCount
    {
        public string Path { get; set; } = string.Empty;
        public int Count { get; set; }
    }


    /// <summary>
    ///
    /// Maintains a rolling in-memory threat log of blocked/flagged requests.
    ///
    /// </summary>
    public class ThreatLog
    {
        private readonly ConcurrentQueue<ThreatLogEntry> _entries;
        private readonly int _maxEntries;


        public ThreatLog(int maxEntries)
        {
            _entries = new ConcurrentQueue<ThreatLogEntry>();
            _maxEntries = maxEntries;
        }


        public ThreatLog(SkynetConfiguration config)
        {
            _entries = new ConcurrentQueue<ThreatLogEntry>();
            _maxEntries = config.ThreatLogMaxEntries;
        }


        /// <summary>
        /// Number of entries currently in the log.
        /// </summary>
        public int Count => _entries.Count;


        /// <summary>
        /// Records a blocked or flagged request.
        /// </summary>
        public void Record(string ipAddress, string path, string countryCode, string action, string ruleName, string reason)
        {
            ThreatLogEntry entry = new ThreatLogEntry
            {
                IpAddress = ipAddress,
                Path = path,
                CountryCode = countryCode ?? string.Empty,
                Action = action,
                RuleName = ruleName,
                Reason = reason,
                TimestampUtc = DateTime.UtcNow
            };

            _entries.Enqueue(entry);

            //
            // Trim to max size
            //
            while (_entries.Count > _maxEntries)
            {
                _entries.TryDequeue(out _);
            }
        }


        /// <summary>
        /// Gets the most recent entries.
        /// </summary>
        public List<ThreatLogEntry> GetRecent(int count = 100)
        {
            List<ThreatLogEntry> entries = _entries.ToList();
            int skip = Math.Max(0, entries.Count - count);

            return entries.Skip(skip).Reverse().ToList();
        }


        /// <summary>
        /// Gets a summary of the threat log.
        /// </summary>
        public ThreatLogSummary GetSummary(int topCount = 10)
        {
            List<ThreatLogEntry> entries = _entries.ToList();

            ThreatLogSummary summary = new ThreatLogSummary
            {
                TotalEntries = entries.Count,
                BlockedCount = entries.Count(e => e.Action == "Blocked"),
                RateLimitedCount = entries.Count(e => e.Action == "RateLimited")
            };

            //
            // Top offending IPs
            //
            summary.TopOffenders = entries
                .GroupBy(e => e.IpAddress)
                .Select(g => new IpBlockCount { IpAddress = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount)
                .ToList();

            //
            // Most targeted paths
            //
            summary.TopTargetedPaths = entries
                .GroupBy(e => e.Path)
                .Select(g => new PathBlockCount { Path = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount)
                .ToList();

            return summary;
        }


        /// <summary>
        /// Clears all log entries.
        /// </summary>
        public void Clear()
        {
            while (_entries.TryDequeue(out _)) { }
        }
    }
}
