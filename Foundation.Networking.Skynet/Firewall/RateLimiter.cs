// ============================================================================
//
// RateLimiter.cs — Sliding window rate limiter per IP address.
//
// Tracks request counts per IP within a configurable time window and
// returns whether a request should be allowed or denied.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Foundation.Networking.Skynet.Firewall
{
    /// <summary>
    ///
    /// Sliding window rate limiter that tracks requests per IP address.
    ///
    /// </summary>
    public class RateLimiter
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _window;
        private readonly ConcurrentDictionary<string, RequestTracker> _trackers;
        private Timer _cleanupTimer;


        /// <summary>
        /// Creates a rate limiter with the specified limit and window.
        /// </summary>
        public RateLimiter(int maxRequests, int windowSeconds)
        {
            _maxRequests = maxRequests;
            _window = TimeSpan.FromSeconds(windowSeconds);
            _trackers = new ConcurrentDictionary<string, RequestTracker>();

            //
            // Periodic cleanup of expired trackers
            //
            _cleanupTimer = new Timer(
                CleanupCallback,
                null,
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(1));
        }


        /// <summary>
        /// Maximum requests allowed per window.
        /// </summary>
        public int MaxRequests => _maxRequests;


        /// <summary>
        /// Window size.
        /// </summary>
        public TimeSpan Window => _window;


        /// <summary>
        /// Number of tracked IP addresses.
        /// </summary>
        public int TrackedIpCount => _trackers.Count;


        /// <summary>
        /// Checks whether a request from the given IP should be allowed.
        /// Returns true if allowed, false if rate limited.
        /// </summary>
        public bool IsAllowed(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return true;
            }

            RequestTracker tracker = _trackers.GetOrAdd(ipAddress, _ => new RequestTracker());

            return tracker.TryRecord(_maxRequests, _window);
        }


        /// <summary>
        /// Gets the current request count for an IP within the window.
        /// </summary>
        public int GetRequestCount(string ipAddress)
        {
            if (_trackers.TryGetValue(ipAddress, out RequestTracker tracker))
            {
                return tracker.GetCount(_window);
            }

            return 0;
        }


        /// <summary>
        /// Gets the number of remaining requests for an IP.
        /// </summary>
        public int GetRemainingRequests(string ipAddress)
        {
            int count = GetRequestCount(ipAddress);
            int remaining = _maxRequests - count;

            return remaining > 0 ? remaining : 0;
        }


        /// <summary>
        /// Resets the rate limit counter for a specific IP.
        /// </summary>
        public void Reset(string ipAddress)
        {
            _trackers.TryRemove(ipAddress, out _);
        }


        /// <summary>
        /// Resets all rate limit counters.
        /// </summary>
        public void ResetAll()
        {
            _trackers.Clear();
        }


        // ── Internal ──────────────────────────────────────────────────────


        private void CleanupCallback(object state)
        {
            DateTime cutoff = DateTime.UtcNow;
            List<string> expiredKeys = new List<string>();

            foreach (var kvp in _trackers)
            {
                if (kvp.Value.IsExpired(_window, cutoff) == true)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (string key in expiredKeys)
            {
                _trackers.TryRemove(key, out _);
            }
        }


        /// <summary>
        /// Tracks requests within a sliding window for a single IP.
        /// </summary>
        private class RequestTracker
        {
            private readonly object _lock = new object();
            private readonly Queue<DateTime> _timestamps;


            public RequestTracker()
            {
                _timestamps = new Queue<DateTime>();
            }


            /// <summary>
            /// Tries to record a request.  Returns true if within the limit.
            /// </summary>
            public bool TryRecord(int maxRequests, TimeSpan window)
            {
                lock (_lock)
                {
                    DateTime now = DateTime.UtcNow;
                    DateTime cutoff = now - window;

                    //
                    // Remove expired entries
                    //
                    while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff)
                    {
                        _timestamps.Dequeue();
                    }

                    //
                    // Check the limit
                    //
                    if (_timestamps.Count >= maxRequests)
                    {
                        return false;
                    }

                    //
                    // Record this request
                    //
                    _timestamps.Enqueue(now);
                    return true;
                }
            }


            /// <summary>
            /// Gets the current request count within the window.
            /// </summary>
            public int GetCount(TimeSpan window)
            {
                lock (_lock)
                {
                    DateTime cutoff = DateTime.UtcNow - window;

                    while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff)
                    {
                        _timestamps.Dequeue();
                    }

                    return _timestamps.Count;
                }
            }


            /// <summary>
            /// Whether this tracker has no recent requests and can be cleaned up.
            /// </summary>
            public bool IsExpired(TimeSpan window, DateTime now)
            {
                lock (_lock)
                {
                    if (_timestamps.Count == 0)
                    {
                        return true;
                    }

                    //
                    // Check if all entries are older than the window
                    //
                    DateTime cutoff = now - window;

                    while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff)
                    {
                        _timestamps.Dequeue();
                    }

                    return _timestamps.Count == 0;
                }
            }
        }
    }
}
