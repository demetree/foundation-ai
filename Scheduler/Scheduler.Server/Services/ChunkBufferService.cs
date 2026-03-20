//
// ChunkBufferService.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Server-side chunk buffer using Foundation.IndexedDB (Dexter layer).
// Incoming file chunks are buffered in a per-session SQLite store
// (WAL mode, durable across restarts) and assembled into the final
// document on upload completion.
//
// Session lifecycle:
//   1. InitSession()  — creates the database + "chunks" store via Dexter API
//   2. AddChunk()     — buffers a verified chunk
//   3. Assemble()     — reads all chunks in order, concatenates bytes
//   4. Cleanup()      — drops the database file
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Foundation.IndexedDB;
using Foundation.IndexedDB.Dexter;

namespace Scheduler.Server.Services
{
    /// <summary>
    /// Model for a single upload chunk stored in IndexedDB.
    /// </summary>
    public class UploadChunk
    {
        public int Id { get; set; }
        public string SessionId { get; set; }
        public int ChunkIndex { get; set; }
        public byte[] Data { get; set; }
        public string Hash { get; set; }
        public DateTime Received { get; set; }
    }


    /// <summary>
    /// Dexter-style typed database for chunk buffering.
    /// </summary>
    public class ChunkDatabase : DexterDatabase
    {
        public DexterTable<UploadChunk, int> Chunks { get; private set; }

        public ChunkDatabase(IDBDatabase indexedDB) : base(indexedDB)
        {
            Chunks = Table<UploadChunk, int>("chunks");
        }

        public static async Task<ChunkDatabase> OpenAsync(string sessionPath)
        {
            IDBFactory factory = new IDBFactory(sessionPath);
            IDBOpenDBRequest request = await factory.OpenAsync("chunks", version: 1,
                upgradeNeededHandler: async (db, oldVer, newVer) =>
                {
                    await db.CreateObjectStoreAsync("chunks",
                        new ObjectStoreOptions { KeyPath = "Id", AutoIncrement = true });
                });

            ChunkDatabase chunkDb = new ChunkDatabase(request.Result);
            await chunkDb.Version(1).DefineStores(new Dictionary<string, string>
            {
                { "chunks", "++Id, ChunkIndex" }
            });

            return chunkDb;
        }
    }


    /// <summary>
    /// Manages chunked upload sessions using Foundation.IndexedDB for durable buffering.
    /// </summary>
    public class ChunkBufferService : IDisposable
    {
        /// <summary>
        /// Default chunk size: 4 MB.
        /// </summary>
        public const int DEFAULT_CHUNK_SIZE = 4 * 1024 * 1024;

        private readonly string _dataPath;
        private readonly Dictionary<string, ChunkDatabase> _sessions = new();
        private readonly object _lock = new();


        /// <summary>
        /// Creates the chunk buffer service.
        /// </summary>
        /// <param name="dataPath">Directory for SQLite chunk databases.</param>
        public ChunkBufferService(string dataPath)
        {
            _dataPath = dataPath;
            Directory.CreateDirectory(_dataPath);

            // Clean up any orphaned sessions from previous crashes
            CleanupOrphanedSessions();
        }


        /// <summary>
        /// Cleans up orphaned session directories that were left behind by
        /// previous server crashes or restarts.
        /// </summary>
        private void CleanupOrphanedSessions()
        {
            try
            {
                foreach (string sessionDir in Directory.GetDirectories(_dataPath))
                {
                    try
                    {
                        // Clear SQLite connection pools for any .sqlite files in the directory
                        foreach (string sqliteFile in Directory.GetFiles(sessionDir, "*.sqlite"))
                        {
                            string connStr = $"Data Source={sqliteFile}";
                            using var tempConn = new Microsoft.Data.Sqlite.SqliteConnection(connStr);
                            Microsoft.Data.Sqlite.SqliteConnection.ClearPool(tempConn);
                        }

                        Directory.Delete(sessionDir, recursive: true);
                    }
                    catch
                    {
                        // Best-effort — files may still be locked briefly
                    }
                }
            }
            catch
            {
                // Best-effort — data path may not exist yet
            }
        }


        /// <summary>
        /// Initialises a new upload session, returns the session ID.
        /// </summary>
        public async Task<string> InitSessionAsync()
        {
            string sessionId = Guid.NewGuid().ToString("N");
            string sessionPath = Path.Combine(_dataPath, sessionId);
            Directory.CreateDirectory(sessionPath);

            ChunkDatabase chunkDb = await ChunkDatabase.OpenAsync(sessionPath);

            lock (_lock)
            {
                _sessions[sessionId] = chunkDb;
            }

            return sessionId;
        }


        /// <summary>
        /// Buffers a single chunk. Verifies the SHA-256 hash before storing.
        /// Returns true if the chunk was accepted.
        /// </summary>
        public async Task<bool> AddChunkAsync(string sessionId, int chunkIndex, byte[] chunkData, string expectedHash)
        {
            ChunkDatabase db = GetSession(sessionId);

            // Verify chunk integrity
            string actualHash = ComputeSha256(chunkData);
            if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            await db.Chunks.AddAsync(new UploadChunk
            {
                SessionId = sessionId,
                ChunkIndex = chunkIndex,
                Data = chunkData,
                Hash = actualHash,
                Received = DateTime.UtcNow
            });

            return true;
        }


        /// <summary>
        /// Returns the number of chunks buffered for a session.
        /// </summary>
        public async Task<long> GetChunkCountAsync(string sessionId)
        {
            ChunkDatabase db = GetSession(sessionId);
            return await db.Chunks.CountAsync();
        }


        /// <summary>
        /// Assembles all buffered chunks into a single byte array, ordered by ChunkIndex.
        /// Optionally verifies the total SHA-256 hash.
        /// </summary>
        public async Task<(byte[] Data, bool Verified)> AssembleAsync(string sessionId, string expectedTotalHash = null)
        {
            ChunkDatabase db = GetSession(sessionId);

            // Read all chunks, ordered by chunk index
            List<UploadChunk> chunks = await db.Chunks.ToListAsync();
            chunks = chunks.OrderBy(c => c.ChunkIndex).ToList();

            // Pre-allocate a single output buffer
            int totalSize = chunks.Sum(c => c.Data.Length);
            byte[] assembled = new byte[totalSize];
            int offset = 0;

            // Copy one chunk at a time, then null out the chunk data
            // to allow GC to reclaim the chunk bytes as we go
            for (int i = 0; i < chunks.Count; i++)
            {
                Buffer.BlockCopy(chunks[i].Data, 0, assembled, offset, chunks[i].Data.Length);
                offset += chunks[i].Data.Length;
                chunks[i].Data = null; // Release chunk data for GC
            }
            chunks = null; // Release the list itself

            // Verify total hash if provided
            bool verified = true;
            if (!string.IsNullOrEmpty(expectedTotalHash))
            {
                string actualTotal = ComputeSha256(assembled);
                verified = string.Equals(actualTotal, expectedTotalHash, StringComparison.OrdinalIgnoreCase);
            }

            return (assembled, verified);
        }


        /// <summary>
        /// Cleans up a completed or abandoned session.
        /// Uses Close() instead of Dispose() to clear the SQLite connection pool,
        /// ensuring file handles are fully released before directory deletion.
        /// </summary>
        public void CleanupSession(string sessionId)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(sessionId, out ChunkDatabase db))
                {
                    // Close() clears the SQLite connection pool so files can be deleted
                    db.IndexedDB.Close();
                    _sessions.Remove(sessionId);
                }
            }

            string sessionPath = Path.Combine(_dataPath, sessionId);
            try
            {
                if (Directory.Exists(sessionPath))
                {
                    Directory.Delete(sessionPath, recursive: true);
                }
            }
            catch
            {
                // Best-effort cleanup — files may be locked briefly
            }
        }


        /// <summary>
        /// Returns true if the given session ID is active.
        /// </summary>
        public bool HasSession(string sessionId)
        {
            lock (_lock)
            {
                return _sessions.ContainsKey(sessionId);
            }
        }


        /// <summary>
        /// Computes SHA-256 hash of the given data.
        /// </summary>
        public static string ComputeSha256(byte[] data)
        {
            byte[] hash = SHA256.HashData(data);
            return Convert.ToHexStringLower(hash);
        }


        private ChunkDatabase GetSession(string sessionId)
        {
            lock (_lock)
            {
                if (!_sessions.TryGetValue(sessionId, out ChunkDatabase db))
                {
                    throw new InvalidOperationException($"Upload session '{sessionId}' not found.");
                }
                return db;
            }
        }


        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var db in _sessions.Values)
                {
                    db.IndexedDB.Close();
                }
                _sessions.Clear();
            }
        }
    }
}
