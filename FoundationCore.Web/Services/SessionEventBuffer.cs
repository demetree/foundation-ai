//
// Session Event Buffer Service
//
// Provides a local IndexedDB-backed cache for session validity data.
// Eliminates SQL Server round-trips for per-request session validation
// by caching session validity state locally in a SQLite store.
//
// Located in Foundation.Web because Foundation.IndexedDB already references
// FoundationCore — placing this in FoundationCore would create a circular dependency.
//
// AI-assisted development - February 2026
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Foundation.IndexedDB;
using Foundation.IndexedDB.Dexter;
using Foundation.Services;
using Microsoft.Extensions.Logging;

namespace Foundation.Web.Services
{
    /// <summary>
    /// Interface for the local session event buffer.
    /// </summary>
    public interface ISessionEventBuffer
    {
        /// <summary>
        /// Caches a new session record locally after SQL Server insert.
        /// </summary>
        Task CacheSessionAsync(SessionInfo info, int sessionId);

        /// <summary>
        /// Fast local check: is this session still valid?
        /// Returns null if not cached (caller should fall through to SQL Server).
        /// </summary>
        Task<bool?> IsSessionValidLocallyAsync(int sessionId);

        /// <summary>
        /// Fast local check by token ID.
        /// Returns null if not cached.
        /// </summary>
        Task<bool?> IsSessionValidByTokenLocallyAsync(string tokenId);

        /// <summary>
        /// Immediately marks a session as revoked in the local cache.
        /// </summary>
        Task RevokeLocalSessionAsync(int sessionId);

        /// <summary>
        /// Immediately marks all sessions for a user as revoked locally.
        /// </summary>
        Task RevokeAllUserSessionsLocallyAsync(int securityUserId);

        /// <summary>
        /// Updates the cached validity of a session after a SQL Server check.
        /// </summary>
        Task UpdateCachedValidityAsync(int sessionId, bool isValid);

        /// <summary>
        /// Updates the cached validity of a session by token after a SQL Server check.
        /// </summary>
        Task UpdateCachedValidityByTokenAsync(string tokenId, bool isValid);

        /// <summary>
        /// Removes expired sessions from the local cache.
        /// </summary>
        Task CleanupExpiredSessionsAsync();
    }


    /// <summary>
    /// Dexie-style database definition for the session event local store.
    /// </summary>
    public class SessionEventDb : DexterDatabase
    {
        public DexterTable<LocalSessionRecord, int> Sessions { get; }

        public SessionEventDb(IDBDatabase db) : base(db)
        {
            //
            // Schema definition:
            //   __SessionId     -> non-auto-increment primary key (matches SQL Server ID)
            //   &TokenId        -> unique index for token-based lookups
            //   SecurityUserId  -> indexed for user-scoped queries (revoke all)
            //   ExpiresAt       -> indexed for expiry cleanup
            //   IsRevoked       -> indexed for revocation queries
            //
            Version(1).DefineStores(new Dictionary<string, string>
            {
                ["sessions"] = "__SessionId, &TokenId, SecurityUserId, ExpiresAt, IsRevoked"
            }).Wait();

            Sessions = Table<LocalSessionRecord, int>("sessions");
        }
    }


    /// <summary>
    /// Local session event buffer using Foundation.IndexedDB.
    /// Provides fast local session validity checks without SQL Server round-trips.
    /// Thread-safe via SQLite WAL mode.
    /// </summary>
    public class SessionEventBuffer : ISessionEventBuffer, IDisposable
    {
        private readonly ILogger<SessionEventBuffer> _logger;
        private readonly IDBFactory _factory;
        private SessionEventDb _db;
        private bool _initialized;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private const int MaxInitRetries = 3;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

        //
        // Operation-level lock to serialize all DbContext access.
        // EF Core DbContext is not thread-safe, and this buffer is a singleton,
        // so concurrent callers must be serialized.
        //
        private readonly SemaphoreSlim _opLock = new SemaphoreSlim(1, 1);

        public SessionEventBuffer(ILogger<SessionEventBuffer> logger)
        {
            _logger = logger;

            string basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "";
            _factory = new IDBFactory(basePath);

            _logger.LogInformation("SessionEventBuffer initialized with base path: {BasePath}", basePath);
        }


        private async Task<SessionEventDb> GetDatabaseAsync()
        {
            if (_initialized && _db != null)
                return _db;

            await _initLock.WaitAsync().ConfigureAwait(false);
            try
            {
                // Double-check after acquiring lock
                if (_initialized && _db != null)
                    return _db;

                for (int attempt = 1; attempt <= MaxInitRetries; attempt++)
                {
                    try
                    {
                        var request = await _factory.OpenAsync("SessionEvents", version: 1).ConfigureAwait(false);
                        _db = new SessionEventDb(request.Result);
                        _initialized = true;

                        _logger.LogInformation("SessionEventBuffer database opened successfully.");
                        return _db;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "SessionEventBuffer database open attempt {Attempt}/{Max} failed.",
                            attempt, MaxInitRetries);

                        if (attempt < MaxInitRetries)
                        {
                            await Task.Delay(RetryDelay).ConfigureAwait(false);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                throw new InvalidOperationException("Failed to open SessionEventBuffer database after retries.");
            }
            finally
            {
                _initLock.Release();
            }
        }


        /// <inheritdoc/>
        public async Task CacheSessionAsync(SessionInfo info, int sessionId)
        {
            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);

                var record = new LocalSessionRecord
                {
                    SessionId = sessionId,
                    SecurityUserId = info.SecurityUserId,
                    ObjectGuid = info.ObjectGuid.ToString(),
                    TokenId = info.TokenId,
                    SessionStart = info.SessionStart,
                    ExpiresAt = info.ExpiresAt,
                    IsRevoked = false,
                    IsValid = true,
                    LastVerifiedAt = DateTime.UtcNow
                };

                await db.Sessions.PutAsync(record).ConfigureAwait(false);

                _logger.LogDebug("Cached session {SessionId} for user {UserId}", sessionId, info.SecurityUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache session {SessionId} locally.", sessionId);
            }
            finally
            {
                _opLock.Release();
            }
        }


        /// <inheritdoc/>
        public async Task<bool?> IsSessionValidLocallyAsync(int sessionId)
        {
            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);
                var record = await db.Sessions.GetAsync(sessionId).ConfigureAwait(false);

                if (record == null)
                    return null; // Cache miss — caller should query SQL Server

                // Check if expired
                if (record.ExpiresAt <= DateTime.UtcNow)
                {
                    record.IsValid = false;
                    await db.Sessions.PutAsync(record).ConfigureAwait(false);
                    return false;
                }

                // Check if revoked
                if (record.IsRevoked)
                    return false;

                return record.IsValid;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Local session validity check failed for session {SessionId}. Falling through to SQL.",
                    sessionId);
                return null; // Fall through to SQL Server on error
            }
            finally
            {
                _opLock.Release();
            }
        }


        /// <inheritdoc/>
        public async Task<bool?> IsSessionValidByTokenLocallyAsync(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
                return null;

            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);

                // Use the TokenId unique index
                var result = await db.Sessions
                    .Where<string>(r => r.TokenId)
                    .Equals(tokenId)
                    .First()
                    .ConfigureAwait(false);

                if (result == null)
                    return null; // Cache miss

                if (result.ExpiresAt <= DateTime.UtcNow)
                {
                    result.IsValid = false;
                    await db.Sessions.PutAsync(result).ConfigureAwait(false);
                    return false;
                }

                if (result.IsRevoked)
                    return false;

                return result.IsValid;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Local session validity check by token failed. Falling through to SQL.");
                return null;
            }
            finally
            {
                _opLock.Release();
            }
        }


        /// <inheritdoc/>
        public async Task RevokeLocalSessionAsync(int sessionId)
        {
            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);
                var record = await db.Sessions.GetAsync(sessionId).ConfigureAwait(false);

                if (record != null)
                {
                    record.IsRevoked = true;
                    record.IsValid = false;
                    record.LastVerifiedAt = DateTime.UtcNow;
                    await db.Sessions.PutAsync(record).ConfigureAwait(false);

                    _logger.LogDebug("Locally revoked session {SessionId}", sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locally revoke session {SessionId}.", sessionId);
            }
            finally
            {
                _opLock.Release();
            }
        }


        /// <inheritdoc/>
        public async Task RevokeAllUserSessionsLocallyAsync(int securityUserId)
        {
            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);

                var userSessions = await db.Sessions
                    .Where<int>(r => r.SecurityUserId)
                    .Equals(securityUserId)
                    .ToArray()
                    .ConfigureAwait(false);

                foreach (var record in userSessions)
                {
                    record.IsRevoked = true;
                    record.IsValid = false;
                    record.LastVerifiedAt = DateTime.UtcNow;
                    await db.Sessions.PutAsync(record).ConfigureAwait(false);
                }

                _logger.LogDebug("Locally revoked {Count} sessions for user {UserId}", userSessions.Count, securityUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locally revoke sessions for user {UserId}.", securityUserId);
            }
            finally
            {
                _opLock.Release();
            }
        }


        /// <inheritdoc/>
        public async Task UpdateCachedValidityAsync(int sessionId, bool isValid)
        {
            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);
                var record = await db.Sessions.GetAsync(sessionId).ConfigureAwait(false);

                if (record != null)
                {
                    record.IsValid = isValid;
                    record.LastVerifiedAt = DateTime.UtcNow;
                    if (!isValid) record.IsRevoked = true;
                    await db.Sessions.PutAsync(record).ConfigureAwait(false);
                }
                else
                {
                    // Session not in cache — create a minimal record so we don't miss again
                    var newRecord = new LocalSessionRecord
                    {
                        SessionId = sessionId,
                        IsValid = isValid,
                        IsRevoked = !isValid,
                        LastVerifiedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddHours(1) // Approximate; sync worker will correct
                    };
                    await db.Sessions.PutAsync(newRecord).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update cached validity for session {SessionId}.", sessionId);
            }
            finally
            {
                _opLock.Release();
            }
        }


        /// <inheritdoc/>
        public async Task UpdateCachedValidityByTokenAsync(string tokenId, bool isValid)
        {
            if (string.IsNullOrEmpty(tokenId))
                return;

            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);

                var record = await db.Sessions
                    .Where<string>(r => r.TokenId)
                    .Equals(tokenId)
                    .First()
                    .ConfigureAwait(false);

                if (record != null)
                {
                    record.IsValid = isValid;
                    record.LastVerifiedAt = DateTime.UtcNow;
                    if (!isValid) record.IsRevoked = true;
                    await db.Sessions.PutAsync(record).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update cached validity by token.");
            }
            finally
            {
                _opLock.Release();
            }
        }


        /// <inheritdoc/>
        public async Task CleanupExpiredSessionsAsync()
        {
            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);
                var allSessions = await db.Sessions.ToListAsync().ConfigureAwait(false);
                var now = DateTime.UtcNow;
                int cleaned = 0;

                foreach (var session in allSessions)
                {
                    // Remove sessions expired more than 1 hour ago (grace period)
                    if (session.ExpiresAt < now.AddHours(-1))
                    {
                        await db.Sessions.DeleteAsync(session.SessionId).ConfigureAwait(false);
                        cleaned++;
                    }
                }

                if (cleaned > 0)
                {
                    _logger.LogDebug("Cleaned up {Count} expired sessions from local cache.", cleaned);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up expired sessions from local cache.");
            }
            finally
            {
                _opLock.Release();
            }
        }


        public void Dispose()
        {
            _opLock?.Dispose();
            _db?.Dispose();
        }
    }
}
