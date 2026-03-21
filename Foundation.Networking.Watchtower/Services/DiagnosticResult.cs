// ============================================================================
//
// DiagnosticResult.cs — Result models for Watchtower diagnostic operations.
//
// Contains PingResult, PingStatistics, TraceHop, PortScanResult, and
// LatencyRecord — the return types for all four core services.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Foundation.Networking.Watchtower.Services
{
    // ── Ping ──────────────────────────────────────────────────────────────


    /// <summary>
    /// Result of a single ICMP ping to a host.
    /// </summary>
    public class PingResult
    {
        /// <summary>
        /// The target host that was pinged.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// The resolved IP address of the host.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Whether the ping was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The ICMP status code.
        /// </summary>
        public IPStatus Status { get; set; }

        /// <summary>
        /// Round-trip time in milliseconds.
        /// </summary>
        public long RoundTripTimeMs { get; set; }

        /// <summary>
        /// TTL of the reply.
        /// </summary>
        public int Ttl { get; set; }

        /// <summary>
        /// Reply buffer size in bytes.
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// Error message if the ping failed.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of when the ping was performed.
        /// </summary>
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }


    /// <summary>
    /// Aggregated statistics from multiple ping requests to a host.
    /// </summary>
    public class PingStatistics
    {
        /// <summary>
        /// The target host that was pinged.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Number of ping requests sent.
        /// </summary>
        public int Sent { get; set; }

        /// <summary>
        /// Number of successful replies received.
        /// </summary>
        public int Received { get; set; }

        /// <summary>
        /// Number of lost replies.
        /// </summary>
        public int Lost { get; set; }

        /// <summary>
        /// Packet loss percentage (0-100).
        /// </summary>
        public double LossPercent { get; set; }

        /// <summary>
        /// Minimum round-trip time in milliseconds.
        /// </summary>
        public long MinRttMs { get; set; }

        /// <summary>
        /// Maximum round-trip time in milliseconds.
        /// </summary>
        public long MaxRttMs { get; set; }

        /// <summary>
        /// Average round-trip time in milliseconds.
        /// </summary>
        public double AvgRttMs { get; set; }

        /// <summary>
        /// Jitter (standard deviation of RTT) in milliseconds.
        /// </summary>
        public double JitterMs { get; set; }

        /// <summary>
        /// Individual ping results.
        /// </summary>
        public List<PingResult> Results { get; set; } = new List<PingResult>();
    }


    // ── Traceroute ────────────────────────────────────────────────────────


    /// <summary>
    /// A single hop in a traceroute path.
    /// </summary>
    public class TraceHop
    {
        /// <summary>
        /// The hop number (1-based, distance from source).
        /// </summary>
        public int HopNumber { get; set; }

        /// <summary>
        /// The IP address of the node at this hop.  Empty if the hop timed out.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// The hostname of the node (reverse DNS), if available.
        /// </summary>
        public string Hostname { get; set; } = string.Empty;

        /// <summary>
        /// Round-trip time in milliseconds.  -1 if the hop timed out.
        /// </summary>
        public long RoundTripTimeMs { get; set; } = -1;

        /// <summary>
        /// Whether this hop responded.
        /// </summary>
        public bool Responded { get; set; }

        /// <summary>
        /// Whether this is the final destination hop.
        /// </summary>
        public bool IsDestination { get; set; }
    }


    /// <summary>
    /// Complete traceroute result to a host.
    /// </summary>
    public class TracerouteResult
    {
        /// <summary>
        /// The target host.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// The resolved IP address of the destination.
        /// </summary>
        public string DestinationAddress { get; set; } = string.Empty;

        /// <summary>
        /// Whether the traceroute reached the destination.
        /// </summary>
        public bool ReachedDestination { get; set; }

        /// <summary>
        /// The hops in the path.
        /// </summary>
        public List<TraceHop> Hops { get; set; } = new List<TraceHop>();

        /// <summary>
        /// Total number of hops.
        /// </summary>
        public int TotalHops { get; set; }

        /// <summary>
        /// Timestamp of when the traceroute was performed.
        /// </summary>
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }


    // ── Port Scanner ──────────────────────────────────────────────────────


    /// <summary>
    /// Result of scanning a single port on a host.
    /// </summary>
    public class PortScanResult
    {
        /// <summary>
        /// The target host.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// The port number that was scanned.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Whether the port is open.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Service name, if identified (e.g., "HTTP", "HTTPS", "SSH").
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Service banner, if one was received.
        /// </summary>
        public string Banner { get; set; } = string.Empty;

        /// <summary>
        /// Connection time in milliseconds.  -1 if timed out.
        /// </summary>
        public long ConnectionTimeMs { get; set; } = -1;
    }


    /// <summary>
    /// Complete port scan result for a host.
    /// </summary>
    public class PortScanReport
    {
        /// <summary>
        /// The target host.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// The resolved IP address.
        /// </summary>
        public string ResolvedAddress { get; set; } = string.Empty;

        /// <summary>
        /// Individual port results.
        /// </summary>
        public List<PortScanResult> Results { get; set; } = new List<PortScanResult>();

        /// <summary>
        /// Number of open ports found.
        /// </summary>
        public int OpenPortCount { get; set; }

        /// <summary>
        /// Number of closed/filtered ports.
        /// </summary>
        public int ClosedPortCount { get; set; }

        /// <summary>
        /// Total scan duration in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Timestamp of when the scan was performed.
        /// </summary>
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }


    // ── Latency Monitor ───────────────────────────────────────────────────


    /// <summary>
    /// A single latency measurement record.
    /// </summary>
    public class LatencyRecord
    {
        /// <summary>
        /// The endpoint label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The host that was pinged.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Round-trip time in milliseconds.  -1 if unreachable.
        /// </summary>
        public long RoundTripTimeMs { get; set; } = -1;

        /// <summary>
        /// Whether the endpoint was reachable.
        /// </summary>
        public bool IsReachable { get; set; }

        /// <summary>
        /// Whether the latency exceeds the alert threshold.
        /// </summary>
        public bool ExceedsThreshold { get; set; }

        /// <summary>
        /// Timestamp of the measurement.
        /// </summary>
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }


    /// <summary>
    /// Summary of latency history for a monitored endpoint.
    /// </summary>
    public class LatencyEndpointSummary
    {
        /// <summary>
        /// The endpoint label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint host.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Current status — whether the endpoint is reachable right now.
        /// </summary>
        public bool IsReachable { get; set; }

        /// <summary>
        /// Current round-trip time in milliseconds.
        /// </summary>
        public long CurrentRttMs { get; set; }

        /// <summary>
        /// Average RTT over the history window.
        /// </summary>
        public double AverageRttMs { get; set; }

        /// <summary>
        /// Minimum RTT in the history window.
        /// </summary>
        public long MinRttMs { get; set; }

        /// <summary>
        /// Maximum RTT in the history window.
        /// </summary>
        public long MaxRttMs { get; set; }

        /// <summary>
        /// Jitter (standard deviation) in the history window.
        /// </summary>
        public double JitterMs { get; set; }

        /// <summary>
        /// Uptime percentage over the history window (0-100).
        /// </summary>
        public double UptimePercent { get; set; }

        /// <summary>
        /// Number of records in the history.
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// Recent latency history records.
        /// </summary>
        public List<LatencyRecord> History { get; set; } = new List<LatencyRecord>();
    }
}
