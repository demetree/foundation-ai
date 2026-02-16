//
// Cached Session Tracking Service
//
// Decorator around SessionTrackingService that checks the local IndexedDB
// cache before falling through to SQL Server. This eliminates per-request
// SQL round-trips for session validation in the middleware.
//
// Located in Foundation.Web due to Foundation.IndexedDB dependency chain.
//
// AI-assisted development - February 2026
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation.Services;
using Microsoft.Extensions.Logging;

namespace Foundation.Web.Services
{
    /// <summary>
    /// Decorator that wraps <see cref="SessionTrackingService"/> with local cache lookups.
    /// Session validation checks the local cache first — only queries SQL Server on cache miss.
    /// Session creation and revocation events are written to both SQL Server and local cache.
    /// </summary>
    public class CachedSessionTrackingService : ISessionTrackingService
    {
        private readonly SessionTrackingService _inner;
        private readonly ISessionEventBuffer _buffer;
        private readonly ILogger<CachedSessionTrackingService> _logger;

        public CachedSessionTrackingService(
            SessionTrackingService inner,
            ISessionEventBuffer buffer,
            ILogger<CachedSessionTrackingService> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _logger = logger;
        }


        /// <summary>
        /// Records a session in SQL Server, then caches it locally.
        /// </summary>
        public async Task<int> RecordSessionAsync(SessionInfo sessionInfo)
        {
            //
            // 1. Write to SQL Server first (source of truth)
            //
            int sessionId = await _inner.RecordSessionAsync(sessionInfo).ConfigureAwait(false);

            //
            // 2. Cache locally (fire-and-forget-safe — SQL write already succeeded)
            //
            if (sessionId > 0)
            {
                try
                {
                    sessionInfo.Id = sessionId;
                    await _buffer.CacheSessionAsync(sessionInfo, sessionId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cache session {SessionId} locally after SQL insert.", sessionId);
                }
            }

            return sessionId;
        }


        /// <summary>
        /// Checks session validity — local cache first, SQL fallback on miss.
        /// This is the HOT PATH: called on every authenticated request by SessionValidationMiddleware.
        /// </summary>
        public async Task<bool> IsSessionValidAsync(int sessionId)
        {
            //
            // 1. Try local cache first (sub-millisecond)
            //
            bool? localResult = await _buffer.IsSessionValidLocallyAsync(sessionId).ConfigureAwait(false);

            if (localResult.HasValue)
            {
                return localResult.Value;
            }

            //
            // 2. Cache miss — fall through to SQL Server
            //
            bool isValid = await _inner.IsSessionValidAsync(sessionId).ConfigureAwait(false);

            //
            // 3. Cache the result for next time
            //
            try
            {
                await _buffer.UpdateCachedValidityAsync(sessionId, isValid).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache session validity result for session {SessionId}.", sessionId);
            }

            return isValid;
        }


        /// <summary>
        /// Checks session validity by token — local cache first, SQL fallback on miss.
        /// </summary>
        public async Task<bool> IsSessionValidByTokenAsync(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
                return false;

            //
            // 1. Try local cache first
            //
            bool? localResult = await _buffer.IsSessionValidByTokenLocallyAsync(tokenId).ConfigureAwait(false);

            if (localResult.HasValue)
            {
                return localResult.Value;
            }

            //
            // 2. Cache miss — fall through to SQL Server
            //
            bool isValid = await _inner.IsSessionValidByTokenAsync(tokenId).ConfigureAwait(false);

            //
            // 3. Cache the result for next time
            //
            try
            {
                await _buffer.UpdateCachedValidityByTokenAsync(tokenId, isValid).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache session validity by token.");
            }

            return isValid;
        }


        /// <summary>
        /// Revokes a session in SQL Server, then immediately marks it revoked locally.
        /// </summary>
        public async Task<bool> RevokeSessionAsync(int sessionId, string revokedBy, string reason)
        {
            bool result = await _inner.RevokeSessionAsync(sessionId, revokedBy, reason).ConfigureAwait(false);

            if (result)
            {
                try
                {
                    await _buffer.RevokeLocalSessionAsync(sessionId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to locally revoke session {SessionId}.", sessionId);
                }
            }

            return result;
        }


        /// <summary>
        /// Revokes all user sessions in SQL Server, then immediately marks them revoked locally.
        /// </summary>
        public async Task<int> RevokeAllUserSessionsAsync(int securityUserId, string revokedBy, string reason)
        {
            int count = await _inner.RevokeAllUserSessionsAsync(securityUserId, revokedBy, reason).ConfigureAwait(false);

            if (count > 0)
            {
                try
                {
                    await _buffer.RevokeAllUserSessionsLocallyAsync(securityUserId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to locally revoke sessions for user {UserId}.", securityUserId);
                }
            }

            return count;
        }


        /// <summary>
        /// Pass-through to inner service — admin read queries don't need caching.
        /// </summary>
        public Task<List<SessionInfo>> GetActiveSessionsAsync()
        {
            return _inner.GetActiveSessionsAsync();
        }


        /// <summary>
        /// Pass-through to inner service — admin read queries don't need caching.
        /// </summary>
        public Task<List<SessionInfo>> GetUserSessionsAsync(int securityUserId)
        {
            return _inner.GetUserSessionsAsync(securityUserId);
        }
    }
}
