using Foundation.IndexedDB;
using Foundation.IndexedDB.Dexter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    ///
    /// IndexedDB/SQLite-based implementation of IAttachmentStorageProvider.
    ///
    /// Uses Foundation.IndexedDB's Dexter wrapper to store attachment content and metadata 
    /// in a single SQLite database file.  Each attachment is stored as a JSON record with 
    /// the binary content encoded as a Base64 string.
    ///
    /// Best suited for:
    ///   - Smaller attachments (less than 10 MB) where the Base64 encoding overhead is acceptable
    ///   - Scenarios where a single-file database is preferred over a directory hierarchy
    ///   - Testing and development environments
    ///
    /// For large attachments in production, use FileSystemAttachmentStorageProvider instead.
    ///
    /// The database is opened on first use and reused for all subsequent operations.
    /// Thread safety is handled by Foundation.IndexedDB's internal SemaphoreSlim locking.
    ///
    /// AI-developed as part of Foundation.Messaging refactoring, March 2026.
    ///
    /// </summary>
    public class IndexedDBAttachmentStorageProvider : IAttachmentStorageProvider, IDisposable
    {
        private readonly string _storagePath;

        private AttachmentDatabase _database;
        private bool _initialized = false;

        private const string DATABASE_NAME = "MessagingAttachments";
        private const string STORE_NAME = "attachments";


        /// <summary>
        /// Internal record class representing the stored attachment data.
        /// Serialized as JSON into the IndexedDB object store.
        /// </summary>
        public class AttachmentRecord
        {
            public string key { get; set; }
            public string tenantGuid { get; set; }
            public string attachmentGuid { get; set; }
            public string fileName { get; set; }
            public string mimeType { get; set; }
            public long contentSize { get; set; }
            public string contentBase64 { get; set; }
            public DateTime dateTimeCreated { get; set; }
        }


        /// <summary>
        /// Dexter database subclass for the attachment store.
        /// Defines a single "attachments" object store keyed by a composite string key.
        /// </summary>
        private class AttachmentDatabase : DexterDatabase
        {
            public DexterTable<AttachmentRecord, string> Attachments { get; private set; }


            public AttachmentDatabase(IDBDatabase indexedDB) : base(indexedDB)
            {
            }


            /// <summary>
            /// Initializes the database schema and table reference.
            /// Must be called after the IDBDatabase has been opened and stores created.
            /// </summary>
            public void InitializeTable()
            {
                Attachments = Table<AttachmentRecord, string>(STORE_NAME);
            }
        }


        /// <summary>
        /// Creates a new IndexedDB attachment storage provider.
        /// </summary>
        /// <param name="storagePath">
        /// The directory where the SQLite database file will be created.
        /// Defaults to the application's base directory if null.
        /// </param>
        public IndexedDBAttachmentStorageProvider(string storagePath = null)
        {
            _storagePath = storagePath;
        }


        /// <summary>
        /// Ensures the IndexedDB database is opened and the attachments store exists.
        /// Called lazily on first operation.
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (_initialized == true)
            {
                return;
            }

            IDBFactory factory = new IDBFactory(_storagePath);

            IDBOpenDBRequest request = await factory.OpenAsync(DATABASE_NAME, 1, (db, oldVersion, newVersion) =>
            {
                //
                // Create the attachments store with a non-auto-incrementing string key.
                // The key format is "{tenantGuid}:{attachmentGuid}".
                //
                db.CreateObjectStoreAsync(STORE_NAME, new ObjectStoreOptions
                {
                    KeyPath = "key",
                    AutoIncrement = false
                }).GetAwaiter().GetResult();
            });

            if (request.ErrorException != null)
            {
                throw new Exception($"Failed to open IndexedDB attachment database: {request.ErrorException.Message}", request.ErrorException);
            }

            _database = new AttachmentDatabase(request.Result);
            _database.InitializeTable();
            _initialized = true;
        }


        /// <summary>
        /// Stores attachment content by encoding it as Base64 and saving it to the IndexedDB store.
        /// </summary>
        public async Task<AttachmentStorageResult> StoreAsync(Guid tenantGuid, string fileName, string mimeType, Stream content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            await EnsureInitializedAsync();

            Guid attachmentGuid = Guid.NewGuid();

            //
            // Read the stream into memory and convert to Base64.
            //
            byte[] contentBytes;

            using (MemoryStream ms = new MemoryStream())
            {
                await content.CopyToAsync(ms);
                contentBytes = ms.ToArray();
            }


            string key = $"{tenantGuid}:{attachmentGuid}";

            AttachmentRecord record = new AttachmentRecord
            {
                key = key,
                tenantGuid = tenantGuid.ToString(),
                attachmentGuid = attachmentGuid.ToString(),
                fileName = fileName,
                mimeType = mimeType,
                contentSize = contentBytes.Length,
                contentBase64 = Convert.ToBase64String(contentBytes),
                dateTimeCreated = DateTime.UtcNow,
            };


            await _database.Attachments.PutAsync(record);


            return new AttachmentStorageResult
            {
                storageGuid = attachmentGuid,
                contentSize = contentBytes.Length,
                storagePath = $"indexeddb://{DATABASE_NAME}/{STORE_NAME}/{key}",
            };
        }


        /// <summary>
        /// Retrieves attachment content by loading the Base64 record from IndexedDB and decoding it.
        /// </summary>
        public async Task<Stream> RetrieveAsync(Guid tenantGuid, Guid attachmentGuid)
        {
            await EnsureInitializedAsync();

            string key = $"{tenantGuid}:{attachmentGuid}";

            AttachmentRecord record = await _database.Attachments.GetAsync(key);

            if (record == null)
            {
                return null;
            }

            byte[] contentBytes = Convert.FromBase64String(record.contentBase64);

            return new MemoryStream(contentBytes, writable: false);
        }


        /// <summary>
        /// Deletes an attachment record from the IndexedDB store.
        /// </summary>
        public async Task<bool> DeleteAsync(Guid tenantGuid, Guid attachmentGuid)
        {
            await EnsureInitializedAsync();

            string key = $"{tenantGuid}:{attachmentGuid}";

            //
            // Check if the record exists before attempting to delete.
            //
            AttachmentRecord existing = await _database.Attachments.GetAsync(key);

            if (existing == null)
            {
                return false;
            }

            await _database.Attachments.DeleteAsync(key);

            return true;
        }


        /// <summary>
        /// Checks whether an attachment exists in the IndexedDB store.
        /// </summary>
        public async Task<bool> ExistsAsync(Guid tenantGuid, Guid attachmentGuid)
        {
            await EnsureInitializedAsync();

            string key = $"{tenantGuid}:{attachmentGuid}";

            AttachmentRecord record = await _database.Attachments.GetAsync(key);

            return record != null;
        }


        public void Dispose()
        {
            if (_database != null)
            {
                _database.Dispose();
                _database = null;
            }

            _initialized = false;
        }
    }
}
