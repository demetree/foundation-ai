// ============================================================================
//
// SessionStore.cs — In-process session store with sliding expiry.
//
// Provides session management with key-value data per session,
// sliding expiry, and session lifecycle management.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Foundation.Networking.Hivemind.Configuration;

namespace Foundation.Networking.Hivemind.Sessions
{
    /// <summary>
    /// A single session with key-value data.
    /// </summary>
    public class Session
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastAccessedUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresUtc { get; set; }
        public ConcurrentDictionary<string, string> Data { get; set; } = new ConcurrentDictionary<string, string>();
    }


    /// <summary>
    /// Session store statistics.
    /// </summary>
    public class SessionStoreStatistics
    {
        public int ActiveSessions { get; set; }
        public long TotalCreated { get; set; }
        public long TotalExpired { get; set; }
    }


    /// <summary>
    ///
    /// Thread-safe session store with sliding expiry.
    ///
    /// </summary>
    public class SessionStore : IDisposable
    {
        private readonly HivemindConfiguration _config;
        private readonly ConcurrentDictionary<string, Session> _sessions;

        private long _totalCreated;
        private long _totalExpired;
        private Timer _cleanupTimer;
        private bool _disposed = false;


        public SessionStore(HivemindConfiguration config)
        {
            _config = config;
            _sessions = new ConcurrentDictionary<string, Session>();

            _cleanupTimer = new Timer(
                CleanupCallback,
                null,
                TimeSpan.FromSeconds(config.CleanupIntervalSeconds),
                TimeSpan.FromSeconds(config.CleanupIntervalSeconds));
        }


        /// <summary>
        /// Number of sessions.
        /// </summary>
        public int Count => _sessions.Count;


        // ── Session Lifecycle ─────────────────────────────────────────────


        /// <summary>
        /// Creates a new session and returns its ID.
        /// </summary>
        public string CreateSession()
        {
            string sessionId = Guid.NewGuid().ToString("N");

            Session session = new Session
            {
                SessionId = sessionId,
                CreatedUtc = DateTime.UtcNow,
                LastAccessedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddSeconds(_config.SessionTtlSeconds)
            };

            _sessions[sessionId] = session;
            Interlocked.Increment(ref _totalCreated);

            return sessionId;
        }


        /// <summary>
        /// Gets a session by ID, extending its expiry if sliding expiry is enabled.
        /// Returns null if not found or expired.
        /// </summary>
        public Session GetSession(string sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out Session session))
            {
                if (DateTime.UtcNow > session.ExpiresUtc)
                {
                    _sessions.TryRemove(sessionId, out _);
                    Interlocked.Increment(ref _totalExpired);
                    return null;
                }

                session.LastAccessedUtc = DateTime.UtcNow;

                if (_config.SessionSlidingExpiry == true)
                {
                    session.ExpiresUtc = DateTime.UtcNow.AddSeconds(_config.SessionTtlSeconds);
                }

                return session;
            }

            return null;
        }


        /// <summary>
        /// Destroys a session.
        /// </summary>
        public bool DestroySession(string sessionId)
        {
            return _sessions.TryRemove(sessionId, out _);
        }


        /// <summary>
        /// Checks whether a session exists and is valid.
        /// </summary>
        public bool SessionExists(string sessionId)
        {
            return GetSession(sessionId) != null;
        }


        // ── Session Data ──────────────────────────────────────────────────


        /// <summary>
        /// Sets a value in a session.
        /// </summary>
        public bool SetValue(string sessionId, string key, string value)
        {
            Session session = GetSession(sessionId);

            if (session == null)
            {
                return false;
            }

            session.Data[key] = value;
            return true;
        }


        /// <summary>
        /// Gets a value from a session.
        /// </summary>
        public string GetValue(string sessionId, string key)
        {
            Session session = GetSession(sessionId);

            if (session == null)
            {
                return null;
            }

            session.Data.TryGetValue(key, out string value);
            return value;
        }


        /// <summary>
        /// Removes a value from a session.
        /// </summary>
        public bool RemoveValue(string sessionId, string key)
        {
            Session session = GetSession(sessionId);

            if (session == null)
            {
                return false;
            }

            return session.Data.TryRemove(key, out _);
        }


        /// <summary>
        /// Gets all keys in a session.
        /// </summary>
        public List<string> GetKeys(string sessionId)
        {
            Session session = GetSession(sessionId);

            if (session == null)
            {
                return new List<string>();
            }

            return session.Data.Keys.ToList();
        }


        // ── Statistics ────────────────────────────────────────────────────


        /// <summary>
        /// Gets session store statistics.
        /// </summary>
        public SessionStoreStatistics GetStatistics()
        {
            return new SessionStoreStatistics
            {
                ActiveSessions = _sessions.Count,
                TotalCreated = Interlocked.Read(ref _totalCreated),
                TotalExpired = Interlocked.Read(ref _totalExpired)
            };
        }


        // ── Cleanup ───────────────────────────────────────────────────────


        private void CleanupCallback(object state)
        {
            DateTime now = DateTime.UtcNow;
            List<string> expired = new List<string>();

            foreach (var kvp in _sessions)
            {
                if (now > kvp.Value.ExpiresUtc)
                {
                    expired.Add(kvp.Key);
                }
            }

            foreach (string id in expired)
            {
                if (_sessions.TryRemove(id, out _))
                {
                    Interlocked.Increment(ref _totalExpired);
                }
            }
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;

                if (_cleanupTimer != null)
                {
                    _cleanupTimer.Dispose();
                    _cleanupTimer = null;
                }
            }
        }
    }
}
