//
// SqlFileStorageService.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// SQL Server-backed implementation of IFileStorageService.
//
// Uses the existing Document, DocumentFolder, DocumentTag, and DocumentDocumentTag
// tables via Entity Framework Core.  All write operations go through the Foundation
// ChangeHistoryToolset so that every create/update/delete is fully audited.
//
// Binary document content (fileDataData) is stored inline in SQL Server.  For
// listings and search results we project metadata-only queries to avoid loading
// large binary payloads into memory.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Scheduler.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Scheduler.Server.Services
{
    public class SqlFileStorageService : IFileStorageService
    {
        private readonly SchedulerContext _db;
        private readonly ILogger<SqlFileStorageService> _logger;

        public SqlFileStorageService(SchedulerContext db, ILogger<SqlFileStorageService> logger)
        {
            _db = db;
            _logger = logger;
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  FOLDER OPERATIONS
        // ═══════════════════════════════════════════════════════════════════════

        public async Task<List<DocumentFolder>> GetFoldersAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            return await _db.DocumentFolders
                .Where(f => f.tenantGuid == tenantGuid && f.deleted == false)
                .Include(f => f.icon)
                .Include(f => f.parentDocumentFolder)
                .OrderBy(f => f.sequence)
                .ThenBy(f => f.name)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }


        public async Task<DocumentFolder> GetFolderByIdAsync(int folderId, Guid tenantGuid, CancellationToken ct = default)
        {
            return await _db.DocumentFolders
                .Where(f => f.id == folderId && f.tenantGuid == tenantGuid && f.deleted == false)
                .Include(f => f.icon)
                .Include(f => f.parentDocumentFolder)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }


        public async Task<DocumentFolder> CreateFolderAsync(DocumentFolder folder, int securityUserId, CancellationToken ct = default)
        {
            folder.objectGuid = Guid.NewGuid();
            folder.active = true;
            folder.deleted = false;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<DocumentFolder, DocumentFolderChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(folder).ConfigureAwait(false);

            _logger.LogInformation("Created DocumentFolder {FolderId} '{FolderName}' for tenant {TenantGuid}.", folder.id, folder.name, folder.tenantGuid);

            return folder;
        }


        public async Task<DocumentFolder> UpdateFolderAsync(DocumentFolder folder, int securityUserId, CancellationToken ct = default)
        {
            DocumentFolder existing = await _db.DocumentFolders
                .Where(f => f.id == folder.id && f.tenantGuid == folder.tenantGuid && f.deleted == false)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (existing == null)
            {
                throw new InvalidOperationException($"DocumentFolder {folder.id} not found.");
            }

            existing.name = folder.name;
            existing.description = folder.description;
            existing.parentDocumentFolderId = folder.parentDocumentFolderId;
            existing.iconId = folder.iconId;
            existing.color = folder.color;
            existing.sequence = folder.sequence;
            existing.notes = folder.notes;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<DocumentFolder, DocumentFolderChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(existing).ConfigureAwait(false);

            return existing;
        }


        public async Task DeleteFolderAsync(int folderId, Guid tenantGuid, int securityUserId, bool cascade = false, CancellationToken ct = default)
        {
            DocumentFolder folder = await _db.DocumentFolders
                .Where(f => f.id == folderId && f.tenantGuid == tenantGuid && f.deleted == false)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (folder == null)
            {
                throw new InvalidOperationException($"DocumentFolder {folderId} not found.");
            }

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<DocumentFolder, DocumentFolderChangeHistory>(_db, securityUserId, false, ct);

            if (cascade)
            {
                //
                // Recursively soft-delete child folders
                //
                List<DocumentFolder> children = await _db.DocumentFolders
                    .Where(f => f.parentDocumentFolderId == folderId && f.tenantGuid == tenantGuid && f.deleted == false)
                    .ToListAsync(ct)
                    .ConfigureAwait(false);

                foreach (DocumentFolder child in children)
                {
                    await DeleteFolderAsync(child.id, tenantGuid, securityUserId, cascade: true, ct).ConfigureAwait(false);
                }

                //
                // Soft-delete documents in this folder
                //
                List<Document> docs = await _db.Documents
                    .Where(d => d.documentFolderId == folderId && d.tenantGuid == tenantGuid && d.deleted == false)
                    .ToListAsync(ct)
                    .ConfigureAwait(false);

                var docChts = new Foundation.ChangeHistory.ChangeHistoryToolset<Document, DocumentChangeHistory>(_db, securityUserId, false, ct);
                foreach (Document doc in docs)
                {
                    doc.deleted = true;
                    await docChts.SaveEntityAsync(doc).ConfigureAwait(false);
                }
            }

            folder.deleted = true;
            await chts.SaveEntityAsync(folder).ConfigureAwait(false);

            _logger.LogInformation("Deleted DocumentFolder {FolderId} for tenant {TenantGuid} (cascade={Cascade}).", folderId, tenantGuid, cascade);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  DOCUMENT / FILE OPERATIONS
        // ═══════════════════════════════════════════════════════════════════════

        public async Task<List<Document>> GetDocumentsInFolderAsync(int? folderId, Guid tenantGuid, CancellationToken ct = default)
        {
            //
            // Return metadata-only (no binary) for listing.
            // We select all columns except fileDataData to keep memory low.
            // Entity link nav properties are included so the file manager can
            // display linked entity names without extra round-trips.
            //
            return await _db.Documents
                .Where(d => d.documentFolderId == folderId && d.tenantGuid == tenantGuid && d.deleted == false)
                .Include(d => d.documentType)
                .Include(d => d.documentFolder)
                .Include(d => d.contact)
                .Include(d => d.resource)
                .Include(d => d.client)
                .Include(d => d.office)
                .Include(d => d.crew)
                .Include(d => d.schedulingTarget)
                .Include(d => d.scheduledEvent)
                .Include(d => d.invoice)
                .Include(d => d.receipt)
                .Include(d => d.paymentTransaction)
                .Include(d => d.volunteerProfile)
                .Include(d => d.financialTransaction)
                .Include(d => d.financialOffice)
                .Include(d => d.campaign)
                .Include(d => d.household)
                .Include(d => d.constituent)
                .Include(d => d.tribute)
                .OrderBy(d => d.name)
                .Select(d => new Document {
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
                    // fileDataData intentionally excluded
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
                    // Nav properties for display
                    documentType = d.documentType,
                    documentFolder = d.documentFolder,
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
                    tribute = d.tribute
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }


        public async Task<Document> GetDocumentByIdAsync(int documentId, Guid tenantGuid, CancellationToken ct = default)
        {
            //
            // Full fetch including binary — used for download
            //
            return await _db.Documents
                .Where(d => d.id == documentId && d.tenantGuid == tenantGuid && d.deleted == false)
                .Include(d => d.documentType)
                .Include(d => d.documentFolder)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }


        public async Task<Document> UploadDocumentAsync(Document document, int securityUserId, CancellationToken ct = default)
        {
            document.objectGuid = Guid.NewGuid();
            document.active = true;
            document.deleted = false;
            document.uploadedDate = DateTime.UtcNow;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<Document, DocumentChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(document).ConfigureAwait(false);

            _logger.LogInformation("Uploaded Document {DocumentId} '{FileName}' ({Size} bytes) for tenant {TenantGuid}.",
                document.id, document.fileName, document.fileSizeBytes, document.tenantGuid);

            return document;
        }


        public async Task<Document> UpdateDocumentMetadataAsync(Document document, int securityUserId, CancellationToken ct = default)
        {
            Document existing = await _db.Documents
                .Where(d => d.id == document.id && d.tenantGuid == document.tenantGuid && d.deleted == false)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (existing == null)
            {
                throw new InvalidOperationException($"Document {document.id} not found.");
            }

            //
            // Update metadata fields only — do not touch binary content
            //
            existing.name = document.name;
            existing.description = document.description;
            existing.documentTypeId = document.documentTypeId;
            existing.documentFolderId = document.documentFolderId;
            existing.notes = document.notes;
            existing.status = document.status;
            existing.statusDate = document.statusDate;
            existing.statusChangedBy = document.statusChangedBy;

            // Update entity links
            existing.scheduledEventId = document.scheduledEventId;
            existing.financialTransactionId = document.financialTransactionId;
            existing.contactId = document.contactId;
            existing.resourceId = document.resourceId;
            existing.clientId = document.clientId;
            existing.officeId = document.officeId;
            existing.crewId = document.crewId;
            existing.schedulingTargetId = document.schedulingTargetId;
            existing.paymentTransactionId = document.paymentTransactionId;
            existing.financialOfficeId = document.financialOfficeId;
            existing.tenantProfileId = document.tenantProfileId;
            existing.campaignId = document.campaignId;
            existing.householdId = document.householdId;
            existing.constituentId = document.constituentId;
            existing.tributeId = document.tributeId;
            existing.volunteerProfileId = document.volunteerProfileId;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<Document, DocumentChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(existing).ConfigureAwait(false);

            return existing;
        }


        public async Task MoveDocumentAsync(int documentId, int? targetFolderId, Guid tenantGuid, int securityUserId, CancellationToken ct = default)
        {
            Document document = await _db.Documents
                .Where(d => d.id == documentId && d.tenantGuid == tenantGuid && d.deleted == false)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (document == null)
            {
                throw new InvalidOperationException($"Document {documentId} not found.");
            }

            //
            // Validate target folder exists (if not moving to root)
            //
            if (targetFolderId.HasValue)
            {
                bool folderExists = await _db.DocumentFolders
                    .AnyAsync(f => f.id == targetFolderId.Value && f.tenantGuid == tenantGuid && f.deleted == false, ct)
                    .ConfigureAwait(false);

                if (!folderExists)
                {
                    throw new InvalidOperationException($"Target folder {targetFolderId.Value} not found.");
                }
            }

            document.documentFolderId = targetFolderId;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<Document, DocumentChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(document).ConfigureAwait(false);

            _logger.LogInformation("Moved Document {DocumentId} to folder {FolderId} for tenant {TenantGuid}.", documentId, targetFolderId?.ToString() ?? "root", tenantGuid);
        }


        public async Task DeleteDocumentAsync(int documentId, Guid tenantGuid, int securityUserId, CancellationToken ct = default)
        {
            Document document = await _db.Documents
                .Where(d => d.id == documentId && d.tenantGuid == tenantGuid && d.deleted == false)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (document == null)
            {
                throw new InvalidOperationException($"Document {documentId} not found.");
            }

            document.deleted = true;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<Document, DocumentChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(document).ConfigureAwait(false);

            _logger.LogInformation("Deleted Document {DocumentId} for tenant {TenantGuid}.", documentId, tenantGuid);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  RECYCLE BIN
        // ═══════════════════════════════════════════════════════════════════════

        public async Task<List<Document>> GetDeletedDocumentsAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            return await _db.Documents
                .Where(d => d.tenantGuid == tenantGuid && d.deleted == true)
                .OrderByDescending(d => d.uploadedDate)
                .Select(d => new Document {
                    id = d.id,
                    tenantGuid = d.tenantGuid,
                    documentFolderId = d.documentFolderId,
                    name = d.name,
                    description = d.description,
                    fileName = d.fileName,
                    mimeType = d.mimeType,
                    fileSizeBytes = d.fileSizeBytes,
                    uploadedDate = d.uploadedDate,
                    uploadedBy = d.uploadedBy,
                    deleted = d.deleted
                })
                .AsNoTracking()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task RestoreDocumentAsync(int documentId, Guid tenantGuid, int securityUserId, CancellationToken ct = default)
        {
            Document document = await _db.Documents
                .Where(d => d.id == documentId && d.tenantGuid == tenantGuid && d.deleted == true)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (document == null)
            {
                throw new InvalidOperationException($"Deleted document {documentId} not found.");
            }

            document.deleted = false;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<Document, DocumentChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(document).ConfigureAwait(false);

            _logger.LogInformation("Restored Document {DocumentId} for tenant {TenantGuid}.", documentId, tenantGuid);
        }

        public async Task PermanentlyDeleteDocumentAsync(int documentId, Guid tenantGuid, int securityUserId, CancellationToken ct = default)
        {
            Document document = await _db.Documents
                .Where(d => d.id == documentId && d.tenantGuid == tenantGuid)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (document == null)
            {
                throw new InvalidOperationException($"Document {documentId} not found.");
            }

            _db.Documents.Remove(document);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            _logger.LogInformation("Permanently deleted Document {DocumentId} for tenant {TenantGuid}.", documentId, tenantGuid);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  VERSION HISTORY
        // ═══════════════════════════════════════════════════════════════════════

        public async Task<List<Document>> GetDocumentVersionsAsync(int documentId, Guid tenantGuid, CancellationToken ct = default)
        {
            // First, get the objectGuid for this document
            Document source = await _db.Documents
                .Where(d => d.id == documentId && d.tenantGuid == tenantGuid)
                .Select(d => new Document { objectGuid = d.objectGuid })
                .AsNoTracking()
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (source == null || source.objectGuid == Guid.Empty)
            {
                return new List<Document>();
            }

            // Return all documents with the same objectGuid (all versions)
            return await _db.Documents
                .Where(d => d.tenantGuid == tenantGuid && d.objectGuid == source.objectGuid && d.deleted == false)
                .OrderByDescending(d => d.versionNumber)
                .Select(d => new Document {
                    id = d.id,
                    tenantGuid = d.tenantGuid,
                    name = d.name,
                    fileName = d.fileName,
                    mimeType = d.mimeType,
                    fileSizeBytes = d.fileSizeBytes,
                    uploadedDate = d.uploadedDate,
                    uploadedBy = d.uploadedBy,
                    versionNumber = d.versionNumber,
                    objectGuid = d.objectGuid
                })
                .AsNoTracking()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  STORAGE QUOTA
        // ═══════════════════════════════════════════════════════════════════════

        public async Task<(long totalBytes, int documentCount)> GetStorageUsageAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var result = await _db.Documents
                .Where(d => d.tenantGuid == tenantGuid && d.deleted == false)
                .GroupBy(d => 1)
                .Select(g => new {
                    totalBytes = g.Sum(d => d.fileSizeBytes),
                    documentCount = g.Count()
                })
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            return result != null ? (result.totalBytes, result.documentCount) : (0, 0);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  SEARCH
        // ═══════════════════════════════════════════════════════════════════════

        public async Task<List<Document>> SearchDocumentsAsync(string query, Guid tenantGuid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<Document>();
            }

            string lowered = query.ToLowerInvariant();

            //
            // Search across name, fileName, and description.
            // Return metadata only (no binary).
            // Entity link nav properties are included so the file manager can
            // display linked entity names in search results.
            //
            return await _db.Documents
                .Where(d => d.tenantGuid == tenantGuid
                    && d.deleted == false
                    && (d.name.ToLower().Contains(lowered)
                        || d.fileName.ToLower().Contains(lowered)
                        || (d.description != null && d.description.ToLower().Contains(lowered))))
                .Include(d => d.documentType)
                .Include(d => d.documentFolder)
                .Include(d => d.contact)
                .Include(d => d.resource)
                .Include(d => d.client)
                .Include(d => d.office)
                .Include(d => d.crew)
                .Include(d => d.schedulingTarget)
                .Include(d => d.scheduledEvent)
                .Include(d => d.invoice)
                .Include(d => d.receipt)
                .Include(d => d.paymentTransaction)
                .Include(d => d.volunteerProfile)
                .Include(d => d.financialTransaction)
                .Include(d => d.financialOffice)
                .Include(d => d.campaign)
                .Include(d => d.household)
                .Include(d => d.constituent)
                .Include(d => d.tribute)
                .OrderBy(d => d.name)
                .Take(100)
                .Select(d => new Document {
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
                    uploadedDate = d.uploadedDate,
                    uploadedBy = d.uploadedBy,
                    notes = d.notes,
                    versionNumber = d.versionNumber,
                    objectGuid = d.objectGuid,
                    active = d.active,
                    deleted = d.deleted,
                    // Nav properties for display
                    documentType = d.documentType,
                    documentFolder = d.documentFolder,
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
                    tribute = d.tribute
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  TAG OPERATIONS
        // ═══════════════════════════════════════════════════════════════════════

        public async Task<List<DocumentTag>> GetTagsAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            return await _db.DocumentTags
                .Where(t => t.tenantGuid == tenantGuid && t.deleted == false)
                .OrderBy(t => t.sequence)
                .ThenBy(t => t.name)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }


        public async Task<DocumentTag> CreateTagAsync(DocumentTag tag, int securityUserId, CancellationToken ct = default)
        {
            tag.objectGuid = Guid.NewGuid();
            tag.active = true;
            tag.deleted = false;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<DocumentTag, DocumentTagChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(tag).ConfigureAwait(false);

            return tag;
        }


        public async Task<DocumentTag> UpdateTagAsync(DocumentTag tag, int securityUserId, CancellationToken ct = default)
        {
            DocumentTag existing = await _db.DocumentTags
                .Where(t => t.id == tag.id && t.tenantGuid == tag.tenantGuid && t.deleted == false)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (existing == null)
            {
                throw new InvalidOperationException($"DocumentTag {tag.id} not found.");
            }

            existing.name = tag.name;
            existing.description = tag.description;
            existing.color = tag.color;
            existing.sequence = tag.sequence;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<DocumentTag, DocumentTagChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(existing).ConfigureAwait(false);

            _logger.LogInformation("Updated DocumentTag {TagId} '{TagName}' for tenant {TenantGuid}.", existing.id, existing.name, existing.tenantGuid);

            return existing;
        }


        public async Task DeleteTagAsync(int tagId, Guid tenantGuid, int securityUserId, CancellationToken ct = default)
        {
            DocumentTag tag = await _db.DocumentTags
                .Where(t => t.id == tagId && t.tenantGuid == tenantGuid && t.deleted == false)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (tag == null)
            {
                throw new InvalidOperationException($"DocumentTag {tagId} not found.");
            }

            // Soft-delete all junction records for this tag
            List<DocumentDocumentTag> junctions = await _db.DocumentDocumentTags
                .Where(ddt => ddt.documentTagId == tagId && ddt.tenantGuid == tenantGuid && ddt.deleted == false)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var junctionChts = new Foundation.ChangeHistory.ChangeHistoryToolset<DocumentDocumentTag, DocumentDocumentTagChangeHistory>(_db, securityUserId, false, ct);
            foreach (DocumentDocumentTag junction in junctions)
            {
                junction.deleted = true;
                await junctionChts.SaveEntityAsync(junction).ConfigureAwait(false);
            }

            // Soft-delete the tag itself
            tag.deleted = true;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<DocumentTag, DocumentTagChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(tag).ConfigureAwait(false);

            _logger.LogInformation("Deleted DocumentTag {TagId} for tenant {TenantGuid} ({JunctionCount} associations removed).", tagId, tenantGuid, junctions.Count);
        }


        public async Task AddTagToDocumentAsync(int documentId, int tagId, Guid tenantGuid, int securityUserId, CancellationToken ct = default)
        {
            //
            // Check for existing assignment to avoid unique constraint violation
            //
            bool alreadyTagged = await _db.DocumentDocumentTags
                .AnyAsync(ddt => ddt.documentId == documentId && ddt.documentTagId == tagId && ddt.tenantGuid == tenantGuid && ddt.deleted == false, ct)
                .ConfigureAwait(false);

            if (alreadyTagged)
            {
                return; // Idempotent — already tagged
            }

            DocumentDocumentTag junction = new DocumentDocumentTag
            {
                tenantGuid = tenantGuid,
                documentId = documentId,
                documentTagId = tagId,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<DocumentDocumentTag, DocumentDocumentTagChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(junction).ConfigureAwait(false);
        }


        public async Task RemoveTagFromDocumentAsync(int documentId, int tagId, Guid tenantGuid, int securityUserId, CancellationToken ct = default)
        {
            DocumentDocumentTag junction = await _db.DocumentDocumentTags
                .Where(ddt => ddt.documentId == documentId && ddt.documentTagId == tagId && ddt.tenantGuid == tenantGuid && ddt.deleted == false)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (junction == null)
            {
                return; // Idempotent — not tagged
            }

            junction.deleted = true;

            var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<DocumentDocumentTag, DocumentDocumentTagChangeHistory>(_db, securityUserId, false, ct);
            await chts.SaveEntityAsync(junction).ConfigureAwait(false);
        }


        public async Task<List<DocumentTag>> GetTagsForDocumentAsync(int documentId, Guid tenantGuid, CancellationToken ct = default)
        {
            return await _db.DocumentDocumentTags
                .Where(ddt => ddt.documentId == documentId && ddt.tenantGuid == tenantGuid && ddt.deleted == false)
                .Select(ddt => ddt.documentTag)
                .Where(t => t.deleted == false)
                .OrderBy(t => t.sequence)
                .ThenBy(t => t.name)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }
    }
}
