//
// SqlDocumentStorageProvider.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Default IDocumentStorageProvider that stores binary content inline in the
// Document table's fileDataData column via SQL Server. This preserves the
// existing behavior with zero migration required.
//
// The storageKey for SQL storage is the document ID as a string (e.g., "42").
//
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Scheduler.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Scheduler.Server.Services
{
    /// <summary>
    /// Stores document binary content inline in SQL Server via the Document.fileDataData column.
    /// This is the default provider, preserving the original behavior.
    /// </summary>
    public class SqlDocumentStorageProvider : IDocumentStorageProvider
    {
        private readonly SchedulerContext _db;
        private readonly ILogger<SqlDocumentStorageProvider> _logger;

        public string ProviderName => "Sql";


        public SqlDocumentStorageProvider(SchedulerContext db, ILogger<SqlDocumentStorageProvider> logger)
        {
            _db = db;
            _logger = logger;
        }


        public async Task<byte[]> GetContentAsync(string storageKey, CancellationToken ct = default)
        {
            if (int.TryParse(storageKey, out int documentId) == false)
            {
                _logger.LogWarning("SqlDocumentStorageProvider: invalid storage key '{Key}' — expected document ID.", storageKey);
                return null;
            }

            byte[] content = await _db.Documents
                .Where(d => d.id == documentId)
                .Select(d => d.fileDataData)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            return content;
        }


        public Task<string> StoreContentAsync(string storageKey, byte[] data, string mimeType, CancellationToken ct = default)
        {
            //
            // For SQL storage, the binary is saved inline on the Document entity
            // by the calling code (SqlFileStorageService.UploadDocumentAsync).
            // This method is a no-op — the storageKey is the document ID.
            //
            // External providers (Local, DeepSpace) will actually write here.
            //
            return Task.FromResult(storageKey);
        }


        public Task DeleteContentAsync(string storageKey, CancellationToken ct = default)
        {
            //
            // For SQL storage, binary is deleted when the Document row is removed
            // by SqlFileStorageService.PermanentlyDeleteDocumentAsync.
            //
            return Task.CompletedTask;
        }


        public async Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
        {
            if (int.TryParse(storageKey, out int documentId) == false)
            {
                return false;
            }

            return await _db.Documents
                .AnyAsync(d => d.id == documentId && d.fileDataData != null, ct)
                .ConfigureAwait(false);
        }
    }
}
