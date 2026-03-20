//
// WebDavLockDatabase.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// DexterDatabase subclass that provides a strongly-typed lock store
// backed by Foundation.IndexedDB (SQLite).
//
// Usage:
//   var factory = new IDBFactory("./Data");
//   var request = await factory.OpenAsync("WebDavLocks", 1, ...);
//   var lockDb = new WebDavLockDatabase(request.Result);
//   await lockDb.SetupSchemaAsync();
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.IndexedDB;
using Foundation.IndexedDB.Dexter;

namespace Scheduler.WebDAV.Services
{
    /// <summary>
    /// SQLite-backed lock storage for WebDAV LOCK/UNLOCK operations (DAV:2).
    /// Uses Foundation.IndexedDB's Dexter layer for a Dexie.js-style fluent API.
    /// </summary>
    public class WebDavLockDatabase : DexterDatabase
    {
        /// <summary>The "locks" object store.</summary>
        public DexterTable<WebDavLock, long> Locks { get; private set; }

        /// <summary>Default lock timeout in seconds (5 minutes).</summary>
        public const int DEFAULT_TIMEOUT_SECONDS = 300;

        /// <summary>Maximum lock timeout in seconds (30 minutes).</summary>
        public const int MAX_TIMEOUT_SECONDS = 1800;


        public WebDavLockDatabase(IDBDatabase indexedDB) : base(indexedDB)
        {
        }

        /// <summary>
        /// Defines the schema for the lock store. Must be called after construction.
        /// </summary>
        public async Task SetupSchemaAsync()
        {
            await Version(1).DefineStores(new Dictionary<string, string>
            {
                { "locks", "++Id, &LockToken, DocumentId, TenantGuid" }
            });

            Locks = Table<WebDavLock, long>("locks");
        }


        // ═══════════════════════════════════════════════════════════════
        //  LOCK OPERATIONS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates a new lock on a document. Returns the created lock.
        /// Throws InvalidOperationException if an exclusive lock already exists from another owner.
        /// </summary>
        public async Task<WebDavLock> AcquireLockAsync(
            int documentId,
            Guid tenantGuid,
            string owner,
            string scope = "exclusive",
            int depth = 0,
            int timeoutSeconds = DEFAULT_TIMEOUT_SECONDS)
        {
            // Clamp timeout
            if (timeoutSeconds <= 0) timeoutSeconds = DEFAULT_TIMEOUT_SECONDS;
            if (timeoutSeconds > MAX_TIMEOUT_SECONDS) timeoutSeconds = MAX_TIMEOUT_SECONDS;

            // Check for conflicting locks
            List<WebDavLock> existing = await GetLocksForDocumentAsync(documentId, tenantGuid);

            foreach (WebDavLock existingLock in existing)
            {
                if (existingLock.LockScope == "exclusive")
                {
                    if (!string.Equals(existingLock.Owner, owner, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Document {documentId} is exclusively locked by '{existingLock.Owner}'.");
                    }

                    // Same owner re-locking — refresh the existing lock
                    existingLock.ExpiresAt = DateTime.UtcNow.AddSeconds(timeoutSeconds);
                    await Locks.PutAsync(existingLock);
                    return existingLock;
                }
            }

            // Create new lock
            WebDavLock newLock = new WebDavLock
            {
                LockToken = $"urn:uuid:{Guid.NewGuid()}",
                DocumentId = documentId,
                TenantGuid = tenantGuid,
                Owner = owner,
                Depth = depth,
                LockScope = scope,
                LockType = "write",
                ExpiresAt = DateTime.UtcNow.AddSeconds(timeoutSeconds),
                CreatedAt = DateTime.UtcNow
            };

            await Locks.AddAsync(newLock);

            return newLock;
        }


        /// <summary>
        /// Refreshes an existing lock's timeout. Returns the updated lock, or null if not found.
        /// </summary>
        public async Task<WebDavLock> RefreshLockAsync(string lockToken, string owner, int timeoutSeconds = DEFAULT_TIMEOUT_SECONDS)
        {
            if (timeoutSeconds <= 0) timeoutSeconds = DEFAULT_TIMEOUT_SECONDS;
            if (timeoutSeconds > MAX_TIMEOUT_SECONDS) timeoutSeconds = MAX_TIMEOUT_SECONDS;

            WebDavLock existing = await GetLockByTokenAsync(lockToken);

            if (existing == null) return null;

            if (!string.Equals(existing.Owner, owner, StringComparison.OrdinalIgnoreCase))
                return null;

            existing.ExpiresAt = DateTime.UtcNow.AddSeconds(timeoutSeconds);
            await Locks.PutAsync(existing);
            return existing;
        }


        /// <summary>
        /// Releases a lock by its token. Returns true if the lock was found and removed.
        /// </summary>
        public async Task<bool> ReleaseLockAsync(string lockToken, string owner)
        {
            WebDavLock existing = await GetLockByTokenAsync(lockToken);

            if (existing == null) return false;

            if (!string.Equals(existing.Owner, owner, StringComparison.OrdinalIgnoreCase))
                return false;

            await Locks.DeleteAsync(existing.Id);
            return true;
        }


        /// <summary>
        /// Returns all active (non-expired) locks for a document.
        /// </summary>
        public async Task<List<WebDavLock>> GetLocksForDocumentAsync(int documentId, Guid tenantGuid)
        {
            List<WebDavLock> allLocks = await Locks
                .Where(l => l.DocumentId)
                .Equals(documentId)
                .ToArray();

            DateTime now = DateTime.UtcNow;

            return allLocks
                .Where(l => l.TenantGuid == tenantGuid && l.ExpiresAt > now)
                .ToList();
        }


        /// <summary>
        /// Returns a lock by its token, or null if not found or expired.
        /// </summary>
        public async Task<WebDavLock> GetLockByTokenAsync(string lockToken)
        {
            WebDavLock lockRecord = await Locks
                .Where(l => l.LockToken)
                .Equals(lockToken)
                .First();

            if (lockRecord != null && lockRecord.ExpiresAt <= DateTime.UtcNow)
            {
                // Expired — clean it up
                await Locks.DeleteAsync(lockRecord.Id);
                return null;
            }

            return lockRecord;
        }


        /// <summary>
        /// Validates that a request's If header contains the correct lock token
        /// for a locked resource. Returns true if the resource is not locked or
        /// the correct token is provided.
        /// </summary>
        public async Task<bool> ValidateLockTokenAsync(
            int documentId,
            Guid tenantGuid,
            string ifHeader)
        {
            List<WebDavLock> locks = await GetLocksForDocumentAsync(documentId, tenantGuid);

            if (locks.Count == 0)
                return true; // Not locked — always valid

            if (string.IsNullOrEmpty(ifHeader))
                return false; // Locked but no token provided

            // Parse lock tokens from If header
            // Formats: (<locktoken>) or (<locktoken> [etag])
            foreach (WebDavLock activeLock in locks)
            {
                if (ifHeader.Contains(activeLock.LockToken, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Removes all expired locks from the store.
        /// Called periodically by the background cleanup timer.
        /// </summary>
        public async Task CleanupExpiredLocksAsync()
        {
            List<WebDavLock> allLocks = await Locks.ToListAsync();

            DateTime now = DateTime.UtcNow;
            int cleaned = 0;

            foreach (WebDavLock lockRecord in allLocks)
            {
                if (lockRecord.ExpiresAt <= now)
                {
                    await Locks.DeleteAsync(lockRecord.Id);
                    cleaned++;
                }
            }
        }
    }
}
