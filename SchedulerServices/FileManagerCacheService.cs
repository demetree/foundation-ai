//
// FileManagerCacheService.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Per-tenant in-memory cache for the File Manager feature.
//
// Caches: folder trees, document metadata (NO binary), tag definitions,
// document-tag mappings, and generated thumbnails.  Write operations
// invalidate affected cache entries so subsequent reads are fresh.
//
// Registered as a Singleton so the cache persists across requests.
// A background timer performs periodic full refreshes to detect external
// changes (e.g., documents uploaded from other modules).
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Scheduler.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Scheduler.Server.Services
{
    /// <summary>
    /// Holds the in-memory file system representation for a single tenant.
    /// All collections are metadata-only — binary document data is never cached.
    /// </summary>
    public class TenantFileSystemCache
    {
        /// <summary>All non-deleted folders for this tenant, keyed by folder ID.</summary>
        public ConcurrentDictionary<int, DocumentFolder> Folders { get; set; }

        /// <summary>Documents grouped by folder ID. null key = root folder.</summary>
        public ConcurrentDictionary<int?, List<Document>> DocumentsByFolder { get; set; }

        /// <summary>All non-deleted tags for this tenant, keyed by tag ID.</summary>
        public ConcurrentDictionary<int, DocumentTag> Tags { get; set; }

        /// <summary>Document-tag mappings: documentId → list of tagIds.</summary>
        public ConcurrentDictionary<int, List<int>> DocumentTagMappings { get; set; }

        /// <summary>Cached generated thumbnail bytes, keyed by document ID.</summary>
        public ConcurrentDictionary<int, byte[]> Thumbnails { get; set; }

        /// <summary>When the folders cache was last fully loaded.</summary>
        public DateTime FoldersLoadedAt { get; set; }

        /// <summary>When the documents cache was last fully loaded.</summary>
        public DateTime DocumentsLoadedAt { get; set; }

        /// <summary>When the tag mappings cache was last fully loaded.</summary>
        public DateTime TagMappingsLoadedAt { get; set; }

        /// <summary>Synchronization lock for folder loading.</summary>
        public SemaphoreSlim FolderLock { get; } = new SemaphoreSlim(1, 1);

        /// <summary>Synchronization lock for document loading.</summary>
        public SemaphoreSlim DocumentLock { get; } = new SemaphoreSlim(1, 1);

        /// <summary>Synchronization lock for tag mapping loading.</summary>
        public SemaphoreSlim TagLock { get; } = new SemaphoreSlim(1, 1);
    }


    /// <summary>
    /// Per-tenant in-memory cache for the File Manager.
    /// Provides fast reads for folders, document metadata, tags, and thumbnails
    /// without hitting the database on every request.
    /// </summary>
    public class FileManagerCacheService : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, TenantFileSystemCache> _tenantCaches = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FileManagerCacheService> _logger;
        private Timer _refreshTimer;

        /// <summary>How long cached data is considered fresh before a background refresh.</summary>
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        /// <summary>Background refresh interval.</summary>
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);


        public FileManagerCacheService(IServiceScopeFactory scopeFactory, ILogger<FileManagerCacheService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            // Start the periodic background refresh timer
            _refreshTimer = new Timer(OnRefreshTimer, null, RefreshInterval, RefreshInterval);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  FOLDER CACHE
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns all non-deleted folders for the tenant from cache, loading from DB on first access or expiry.
        /// </summary>
        public async Task<List<DocumentFolder>> GetFoldersAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var cache = GetOrCreateTenantCache(tenantGuid);

            if (cache.Folders != null && (DateTime.UtcNow - cache.FoldersLoadedAt) < CacheTtl)
            {
                return cache.Folders.Values.OrderBy(f => f.sequence).ThenBy(f => f.name).ToList();
            }

            await cache.FolderLock.WaitAsync(ct);
            try
            {
                // Double-check after acquiring lock
                if (cache.Folders != null && (DateTime.UtcNow - cache.FoldersLoadedAt) < CacheTtl)
                {
                    return cache.Folders.Values.OrderBy(f => f.sequence).ThenBy(f => f.name).ToList();
                }

                await LoadFoldersFromDbAsync(tenantGuid, cache, ct);
                return cache.Folders.Values.OrderBy(f => f.sequence).ThenBy(f => f.name).ToList();
            }
            finally
            {
                cache.FolderLock.Release();
            }
        }


        /// <summary>
        /// Returns a single folder from cache by ID, or null if not found.
        /// </summary>
        public async Task<DocumentFolder> GetFolderByIdAsync(int folderId, Guid tenantGuid, CancellationToken ct = default)
        {
            var folders = await GetFoldersAsync(tenantGuid, ct);
            return folders.FirstOrDefault(f => f.id == folderId);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  DOCUMENT METADATA CACHE
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns document metadata (no binary) for a specific folder from cache.
        /// </summary>
        public async Task<List<Document>> GetDocumentsInFolderAsync(int? folderId, Guid tenantGuid, CancellationToken ct = default)
        {
            var cache = GetOrCreateTenantCache(tenantGuid);

            if (cache.DocumentsByFolder != null && (DateTime.UtcNow - cache.DocumentsLoadedAt) < CacheTtl)
            {
                if (cache.DocumentsByFolder.TryGetValue(folderId, out var cached))
                {
                    return cached;
                }
                return new List<Document>();
            }

            await cache.DocumentLock.WaitAsync(ct);
            try
            {
                if (cache.DocumentsByFolder != null && (DateTime.UtcNow - cache.DocumentsLoadedAt) < CacheTtl)
                {
                    if (cache.DocumentsByFolder.TryGetValue(folderId, out var cached))
                    {
                        return cached;
                    }
                    return new List<Document>();
                }

                await LoadDocumentsFromDbAsync(tenantGuid, cache, ct);

                if (cache.DocumentsByFolder.TryGetValue(folderId, out var docs))
                {
                    return docs;
                }
                return new List<Document>();
            }
            finally
            {
                cache.DocumentLock.Release();
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  TAG MAPPING CACHE
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns all document-tag mappings for the tenant from cache.
        /// Key = documentId, Value = list of tagIds.
        /// </summary>
        public async Task<Dictionary<int, List<int>>> GetTagMappingsAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var cache = GetOrCreateTenantCache(tenantGuid);

            if (cache.DocumentTagMappings != null && (DateTime.UtcNow - cache.TagMappingsLoadedAt) < CacheTtl)
            {
                return new Dictionary<int, List<int>>(cache.DocumentTagMappings);
            }

            await cache.TagLock.WaitAsync(ct);
            try
            {
                if (cache.DocumentTagMappings != null && (DateTime.UtcNow - cache.TagMappingsLoadedAt) < CacheTtl)
                {
                    return new Dictionary<int, List<int>>(cache.DocumentTagMappings);
                }

                await LoadTagMappingsFromDbAsync(tenantGuid, cache, ct);
                return new Dictionary<int, List<int>>(cache.DocumentTagMappings);
            }
            finally
            {
                cache.TagLock.Release();
            }
        }

        /// <summary>
        /// Returns tags for a specific set of document IDs from cache.
        /// More efficient than per-document lookups.
        /// </summary>
        public async Task<Dictionary<int, List<DocumentTag>>> GetTagsForDocumentsAsync(IEnumerable<int> documentIds, Guid tenantGuid, CancellationToken ct = default)
        {
            var mappings = await GetTagMappingsAsync(tenantGuid, ct);
            var allTags = await GetTagsAsync(tenantGuid, ct);
            var tagLookup = allTags.ToDictionary(t => t.id);

            var result = new Dictionary<int, List<DocumentTag>>();
            foreach (int docId in documentIds)
            {
                if (mappings.TryGetValue(docId, out var tagIds))
                {
                    result[docId] = tagIds
                        .Where(tid => tagLookup.ContainsKey(tid))
                        .Select(tid => tagLookup[tid])
                        .OrderBy(t => t.sequence)
                        .ThenBy(t => t.name)
                        .ToList();
                }
                else
                {
                    result[docId] = new List<DocumentTag>();
                }
            }

            return result;
        }


        /// <summary>
        /// Returns all tags for the tenant from cache.
        /// </summary>
        public async Task<List<DocumentTag>> GetTagsAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var cache = GetOrCreateTenantCache(tenantGuid);

            // Tags are loaded as part of the tag mappings load
            if (cache.Tags != null && (DateTime.UtcNow - cache.TagMappingsLoadedAt) < CacheTtl)
            {
                return cache.Tags.Values.OrderBy(t => t.sequence).ThenBy(t => t.name).ToList();
            }

            // Force a tag mappings load (which also loads tags)
            await GetTagMappingsAsync(tenantGuid, ct);
            return cache.Tags?.Values.OrderBy(t => t.sequence).ThenBy(t => t.name).ToList() ?? new List<DocumentTag>();
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  THUMBNAIL CACHE
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns a cached thumbnail for a document, or null if not cached.
        /// Does NOT generate thumbnails — that is left to the controller to do once
        /// and then store via <see cref="StoreThumbnail"/>.
        /// </summary>
        public byte[] GetThumbnail(int documentId, Guid tenantGuid)
        {
            var cache = GetOrCreateTenantCache(tenantGuid);
            if (cache.Thumbnails != null && cache.Thumbnails.TryGetValue(documentId, out var thumb))
            {
                return thumb;
            }
            return null;
        }

        /// <summary>
        /// Stores a generated thumbnail in the cache for future requests.
        /// </summary>
        public void StoreThumbnail(int documentId, Guid tenantGuid, byte[] thumbnailBytes)
        {
            var cache = GetOrCreateTenantCache(tenantGuid);
            cache.Thumbnails ??= new ConcurrentDictionary<int, byte[]>();
            cache.Thumbnails[documentId] = thumbnailBytes;
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  CACHE INVALIDATION
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Invalidates folder cache so the next read re-fetches from DB.
        /// </summary>
        public void InvalidateFolders(Guid tenantGuid)
        {
            if (_tenantCaches.TryGetValue(tenantGuid, out var cache))
            {
                cache.Folders = null;
                cache.FoldersLoadedAt = DateTime.MinValue;
                _logger.LogDebug("Invalidated folder cache for tenant {TenantGuid}.", tenantGuid);
            }
        }

        /// <summary>
        /// Invalidates the document metadata cache for a tenant.
        /// </summary>
        public void InvalidateDocuments(Guid tenantGuid)
        {
            if (_tenantCaches.TryGetValue(tenantGuid, out var cache))
            {
                cache.DocumentsByFolder = null;
                cache.DocumentsLoadedAt = DateTime.MinValue;
                _logger.LogDebug("Invalidated document cache for tenant {TenantGuid}.", tenantGuid);
            }
        }

        /// <summary>
        /// Invalidates tag definitions and document-tag mappings cache.
        /// </summary>
        public void InvalidateTagMappings(Guid tenantGuid)
        {
            if (_tenantCaches.TryGetValue(tenantGuid, out var cache))
            {
                cache.Tags = null;
                cache.DocumentTagMappings = null;
                cache.TagMappingsLoadedAt = DateTime.MinValue;
                _logger.LogDebug("Invalidated tag mappings cache for tenant {TenantGuid}.", tenantGuid);
            }
        }

        /// <summary>
        /// Invalidates the thumbnail cache for a specific document (e.g., on re-upload).
        /// </summary>
        public void InvalidateThumbnail(int documentId, Guid tenantGuid)
        {
            if (_tenantCaches.TryGetValue(tenantGuid, out var cache))
            {
                cache.Thumbnails?.TryRemove(documentId, out _);
            }
        }

        /// <summary>
        /// Invalidates ALL caches for a tenant.
        /// </summary>
        public void InvalidateAll(Guid tenantGuid)
        {
            _tenantCaches.TryRemove(tenantGuid, out _);
            _logger.LogDebug("Invalidated all caches for tenant {TenantGuid}.", tenantGuid);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  PRIVATE: DB LOADING
        // ═══════════════════════════════════════════════════════════════════════

        private TenantFileSystemCache GetOrCreateTenantCache(Guid tenantGuid)
        {
            return _tenantCaches.GetOrAdd(tenantGuid, _ => new TenantFileSystemCache());
        }

        private async Task LoadFoldersFromDbAsync(Guid tenantGuid, TenantFileSystemCache cache, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var folders = await db.DocumentFolders
                .Where(f => f.tenantGuid == tenantGuid && f.deleted == false)
                .Include(f => f.icon)
                .AsNoTracking()
                .ToListAsync(ct);

            cache.Folders = new ConcurrentDictionary<int, DocumentFolder>(
                folders.ToDictionary(f => f.id));
            cache.FoldersLoadedAt = DateTime.UtcNow;

            _logger.LogDebug("Loaded {Count} folders into cache for tenant {TenantGuid}.", folders.Count, tenantGuid);
        }

        private async Task LoadDocumentsFromDbAsync(Guid tenantGuid, TenantFileSystemCache cache, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            //
            // Load ALL metadata for the tenant in a single query.
            // Project to exclude fileDataData (binary).
            // We use .Select() to pick just the columns we need, including
            // entity link foreign keys and their names (avoiding 17 JOINs by
            // using conditional sub-selects for names).
            //
            var allDocs = await db.Documents
                .Where(d => d.tenantGuid == tenantGuid && d.deleted == false)
                .Select(d => new Document
                {
                    id = d.id,
                    tenantGuid = d.tenantGuid,
                    documentTypeId = d.documentTypeId,
                    documentFolderId = d.documentFolderId,
                    name = d.name,
                    description = d.description,
                    fileName = d.fileName,
                    mimeType = d.mimeType,
                    fileSizeBytes = d.fileSizeBytes,
                    fileDataFileName = d.fileDataFileName,
                    fileDataSize = d.fileDataSize,
                    fileDataMimeType = d.fileDataMimeType,
                    // fileDataData intentionally excluded — never cached
                    invoiceId = d.invoiceId,
                    receiptId = d.receiptId,
                    scheduledEventId = d.scheduledEventId,
                    financialTransactionId = d.financialTransactionId,
                    contactId = d.contactId,
                    resourceId = d.resourceId,
                    clientId = d.clientId,
                    officeId = d.officeId,
                    crewId = d.crewId,
                    schedulingTargetId = d.schedulingTargetId,
                    paymentTransactionId = d.paymentTransactionId,
                    financialOfficeId = d.financialOfficeId,
                    tenantProfileId = d.tenantProfileId,
                    campaignId = d.campaignId,
                    householdId = d.householdId,
                    constituentId = d.constituentId,
                    tributeId = d.tributeId,
                    volunteerProfileId = d.volunteerProfileId,
                    status = d.status,
                    statusDate = d.statusDate,
                    statusChangedBy = d.statusChangedBy,
                    uploadedDate = d.uploadedDate,
                    uploadedBy = d.uploadedBy,
                    notes = d.notes,
                    versionNumber = d.versionNumber,
                    objectGuid = d.objectGuid,
                    active = d.active,
                    deleted = d.deleted,
                    // Entity link nav properties — include only for name resolution
                    contact = d.contact,
                    resource = d.resource,
                    client = d.client,
                    office = d.office,
                    crew = d.crew,
                    schedulingTarget = d.schedulingTarget,
                    scheduledEvent = d.scheduledEvent,
                    invoice = d.invoice,
                    receipt = d.receipt,
                    paymentTransaction = d.paymentTransaction,
                    volunteerProfile = d.volunteerProfile,
                    financialTransaction = d.financialTransaction,
                    financialOffice = d.financialOffice,
                    campaign = d.campaign,
                    household = d.household,
                    constituent = d.constituent,
                    tribute = d.tribute,
                    documentType = d.documentType,
                    documentFolder = d.documentFolder
                })
                .AsNoTracking()
                .ToListAsync(ct);

            // Group by folder
            var byFolder = new ConcurrentDictionary<int?, List<Document>>();
            foreach (var group in allDocs.GroupBy(d => d.documentFolderId))
            {
                byFolder[group.Key] = group.OrderBy(d => d.name).ToList();
            }

            cache.DocumentsByFolder = byFolder;
            cache.DocumentsLoadedAt = DateTime.UtcNow;

            _logger.LogDebug("Loaded {Count} documents into cache for tenant {TenantGuid}.", allDocs.Count, tenantGuid);
        }

        private async Task LoadTagMappingsFromDbAsync(Guid tenantGuid, TenantFileSystemCache cache, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            // Load all tags
            var tags = await db.DocumentTags
                .Where(t => t.tenantGuid == tenantGuid && t.deleted == false)
                .AsNoTracking()
                .ToListAsync(ct);

            cache.Tags = new ConcurrentDictionary<int, DocumentTag>(
                tags.ToDictionary(t => t.id));

            // Load ALL document-tag mappings in a single query
            var mappings = await db.DocumentDocumentTags
                .Where(ddt => ddt.tenantGuid == tenantGuid && ddt.deleted == false)
                .Select(ddt => new { ddt.documentId, ddt.documentTagId })
                .AsNoTracking()
                .ToListAsync(ct);

            var tagMap = new ConcurrentDictionary<int, List<int>>();
            foreach (var group in mappings.GroupBy(m => m.documentId))
            {
                tagMap[group.Key] = group.Select(m => m.documentTagId).ToList();
            }

            cache.DocumentTagMappings = tagMap;
            cache.TagMappingsLoadedAt = DateTime.UtcNow;

            _logger.LogDebug("Loaded {TagCount} tags and {MappingCount} document-tag mappings into cache for tenant {TenantGuid}.",
                tags.Count, mappings.Count, tenantGuid);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  BACKGROUND REFRESH
        // ═══════════════════════════════════════════════════════════════════════

        private async void OnRefreshTimer(object state)
        {
            try
            {
                // Refresh all tenant caches that exist
                foreach (var kvp in _tenantCaches)
                {
                    Guid tenantGuid = kvp.Key;
                    var cache = kvp.Value;

                    // Only refresh if stale; skip if recently loaded
                    try
                    {
                        if (cache.Folders != null && (DateTime.UtcNow - cache.FoldersLoadedAt) >= CacheTtl)
                        {
                            await cache.FolderLock.WaitAsync(CancellationToken.None);
                            try
                            {
                                await LoadFoldersFromDbAsync(tenantGuid, cache, CancellationToken.None);
                            }
                            finally
                            {
                                cache.FolderLock.Release();
                            }
                        }

                        if (cache.DocumentsByFolder != null && (DateTime.UtcNow - cache.DocumentsLoadedAt) >= CacheTtl)
                        {
                            await cache.DocumentLock.WaitAsync(CancellationToken.None);
                            try
                            {
                                await LoadDocumentsFromDbAsync(tenantGuid, cache, CancellationToken.None);
                            }
                            finally
                            {
                                cache.DocumentLock.Release();
                            }
                        }

                        if (cache.DocumentTagMappings != null && (DateTime.UtcNow - cache.TagMappingsLoadedAt) >= CacheTtl)
                        {
                            await cache.TagLock.WaitAsync(CancellationToken.None);
                            try
                            {
                                await LoadTagMappingsFromDbAsync(tenantGuid, cache, CancellationToken.None);
                            }
                            finally
                            {
                                cache.TagLock.Release();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Background refresh failed for tenant {TenantGuid}.", tenantGuid);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in FileManagerCacheService background refresh.");
            }
        }


        public void Dispose()
        {
            _refreshTimer?.Dispose();
            _refreshTimer = null;
        }
    }
}
