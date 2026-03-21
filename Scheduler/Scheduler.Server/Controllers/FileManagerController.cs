//
// FileManagerController.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Custom controller for the Document Manager / File Manager feature.
//
// Provides a unified REST API for the Angular File Manager UI to perform all
// file and folder operations.  Uses IFileStorageService as its storage backend,
// allowing the controller to remain unchanged when switching from SQL storage to
// cloud storage in the future.
//
// All endpoints are tenant-scoped and require authentication via the Foundation
// security model (SecureWebAPIController).
//
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Scheduler.Database;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scheduler.Server.Services;
using Foundation.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// Controller for the Document Manager / File Manager feature.
    /// Exposes folder, document, and tag operations via REST endpoints.
    /// </summary>
    public class FileManagerController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

        /// <summary>
        /// Default storage quota per tenant: 5 GB.
        /// </summary>
        public const long DEFAULT_QUOTA_BYTES = 5L * 1024 * 1024 * 1024;

        private readonly SchedulerContext _db;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<FileManagerController> _logger;
        private readonly IHubContext<FileManagerHub, IFileManagerHubClient> _hub;
        private readonly ChunkBufferService _chunkBuffer;
        private readonly FileManagerCacheService _cache;

        public FileManagerController(
            SchedulerContext db,
            IFileStorageService fileStorage,
            ILogger<FileManagerController> logger,
            IHubContext<FileManagerHub, IFileManagerHubClient> hub,
            ChunkBufferService chunkBuffer,
            FileManagerCacheService cache) : base("Scheduler", "FileManager")
        {
            _db = db;
            _fileStorage = fileStorage;
            _logger = logger;
            _hub = hub;
            _chunkBuffer = chunkBuffer;
            _cache = cache;
        }

        /// <summary>
        /// Broadcasts a signal to all clients in the same tenant group.
        /// </summary>
        private async Task BroadcastDocumentChangedAsync(Guid tenantGuid, object payload)
        {
            await _hub.Clients.Group($"filemanager_{tenantGuid}").DocumentChanged(payload);
        }

        private async Task BroadcastDocumentDeletedAsync(Guid tenantGuid, object payload)
        {
            await _hub.Clients.Group($"filemanager_{tenantGuid}").DocumentDeleted(payload);
        }

        private async Task BroadcastFolderChangedAsync(Guid tenantGuid, object payload)
        {
            await _hub.Clients.Group($"filemanager_{tenantGuid}").FolderChanged(payload);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  FOLDER ENDPOINTS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns all folders for the current tenant.
        /// </summary>
        [Route("api/FileManager/Folders")]
        [HttpGet]
        public async Task<IActionResult> GetFolders()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                List<DocumentFolder> folders = await _cache.GetFoldersAsync(tenantGuid);

                return Ok(DocumentFolder.ToOutputDTOList(folders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching folders.");
                return StatusCode(500, "Error fetching folders.");
            }
        }


        /// <summary>
        /// Returns a single folder by ID.
        /// </summary>
        [Route("api/FileManager/Folders/{folderId}")]
        [HttpGet]
        public async Task<IActionResult> GetFolder(int folderId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                DocumentFolder folder = await _cache.GetFolderByIdAsync(folderId, tenantGuid);

                if (folder == null)
                {
                    return NotFound();
                }

                return Ok(folder.ToOutputDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching folder {FolderId}.", folderId);
                return StatusCode(500, "Error fetching folder.");
            }
        }


        /// <summary>
        /// Creates a new folder.
        /// </summary>
        [Route("api/FileManager/Folders")]
        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] DocumentFolder.DocumentFolderDTO dto)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                DocumentFolder folder = DocumentFolder.FromDTO(dto);
                folder.tenantGuid = tenantGuid;

                DocumentFolder created = await _fileStorage.CreateFolderAsync(folder, securityUser.id);
                _cache.InvalidateFolders(tenantGuid);

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, $"Created folder '{created.name}'", securityUser.accountName);
                await BroadcastFolderChangedAsync(tenantGuid, new { action = "created", folderId = created.id });

                return Ok(created.ToOutputDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder.");
                return StatusCode(500, "Error creating folder.");
            }
        }


        /// <summary>
        /// Updates an existing folder.
        /// </summary>
        [Route("api/FileManager/Folders")]
        [HttpPut]
        public async Task<IActionResult> UpdateFolder([FromBody] DocumentFolder.DocumentFolderDTO dto)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                DocumentFolder folder = DocumentFolder.FromDTO(dto);
                folder.tenantGuid = tenantGuid;

                DocumentFolder updated = await _fileStorage.UpdateFolderAsync(folder, securityUser.id);
                _cache.InvalidateFolders(tenantGuid);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, $"Updated folder '{updated.name}'", securityUser.accountName);
                await BroadcastFolderChangedAsync(tenantGuid, new { action = "updated", folderId = updated.id });

                return Ok(updated.ToOutputDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating folder.");
                return StatusCode(500, "Error updating folder.");
            }
        }


        /// <summary>
        /// Deletes a folder (soft delete).  Pass cascade=true to also delete child folders/documents.
        /// </summary>
        [Route("api/FileManager/Folders/{folderId}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteFolder(int folderId, [FromQuery] bool cascade = false)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                await _fileStorage.DeleteFolderAsync(folderId, tenantGuid, securityUser.id, cascade);
                _cache.InvalidateFolders(tenantGuid);
                if (cascade) _cache.InvalidateDocuments(tenantGuid);

                await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, $"Deleted folder {folderId} (cascade={cascade})", securityUser.accountName);
                await BroadcastFolderChangedAsync(tenantGuid, new { action = "deleted", folderId });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting folder {FolderId}.", folderId);
                return StatusCode(500, "Error deleting folder.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  DOCUMENT / FILE ENDPOINTS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns documents in a specific folder (or root level if folderId is not provided).
        /// Metadata only — no binary data returned in listings.
        /// </summary>
        [Route("api/FileManager/Documents")]
        [HttpGet]
        public async Task<IActionResult> GetDocumentsInFolder([FromQuery] int? folderId = null)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                List<Document> documents = await _cache.GetDocumentsInFolderAsync(folderId, tenantGuid);

                return Ok(Document.ToOutputDTOList(documents));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching documents in folder {FolderId}.", folderId);
                return StatusCode(500, "Error fetching documents.");
            }
        }


        /// <summary>
        /// Returns ALL documents across all folders for the current tenant (metadata only).
        /// Used by the file manager's "flat mode" to display every document in one list.
        /// </summary>
        [Route("api/FileManager/Documents/All")]
        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                List<Document> documents = await _cache.GetAllDocumentsAsync(tenantGuid);

                return Ok(Document.ToOutputDTOList(documents));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all documents.");
                return StatusCode(500, "Error fetching all documents.");
            }
        }


        /// <summary>
        /// Uploads one or more documents.  Accepts multipart/form-data with one or more files.
        /// Optional query parameters: folderId, documentTypeId, and entity link IDs.
        /// </summary>
        [Route("api/FileManager/Documents/Upload")]
        [HttpPost]
        [RequestSizeLimit(Constants.ONE_GIGABYTE_IN_BYTES)]
        [RequestFormLimits(MultipartBodyLengthLimit = Constants.ONE_GIGABYTE_IN_BYTES)]
        public async Task<IActionResult> UploadDocuments(
            [FromQuery] int? folderId = null,
            [FromQuery] int? documentTypeId = null,
            [FromQuery] int? scheduledEventId = null,
            [FromQuery] int? contactId = null,
            [FromQuery] int? clientId = null,
            [FromQuery] int? resourceId = null,
            [FromQuery] int? crewId = null,
            [FromQuery] int? schedulingTargetId = null,
            [FromQuery] int? financialTransactionId = null,
            [FromQuery] int? officeId = null,
            [FromQuery] int? campaignId = null,
            [FromQuery] int? householdId = null,
            [FromQuery] int? constituentId = null,
            [FromQuery] int? tributeId = null,
            [FromQuery] int? volunteerProfileId = null)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                IFormFileCollection files = Request.Form.Files;

                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files provided.");
                }

                // Quota check — reject if upload would exceed limit
                var (usedBytes, _) = await _fileStorage.GetStorageUsageAsync(tenantGuid);
                long incomingBytes = files.Sum(f => f.Length);
                if (usedBytes + incomingBytes > DEFAULT_QUOTA_BYTES)
                {
                    string usedMb = (usedBytes / (1024.0 * 1024)).ToString("F1");
                    string quotaMb = (DEFAULT_QUOTA_BYTES / (1024.0 * 1024)).ToString("F0");
                    return BadRequest($"Storage quota exceeded. Used: {usedMb} MB / {quotaMb} MB. Upload size: {(incomingBytes / (1024.0 * 1024)):F1} MB.");
                }

                List<Document.DocumentDTO> uploadedResults = new List<Document.DocumentDTO>();

                foreach (IFormFile file in files)
                {
                    byte[] fileBytes = new byte[file.Length];
                    using (var stream = file.OpenReadStream())
                    {
                        await stream.ReadExactlyAsync(fileBytes, 0, (int)file.Length);
                    }

                    Document document = new Document
                    {
                        tenantGuid = tenantGuid,
                        name = Path.GetFileNameWithoutExtension(file.FileName),
                        fileName = file.FileName,
                        mimeType = file.ContentType ?? "application/octet-stream",
                        fileSizeBytes = file.Length,
                        fileDataFileName = file.FileName,
                        fileDataSize = file.Length,
                        fileDataData = fileBytes,
                        fileDataMimeType = file.ContentType ?? "application/octet-stream",
                        documentFolderId = folderId,
                        documentTypeId = documentTypeId,
                        uploadedBy = securityUser.accountName,

                        // Entity links
                        scheduledEventId = scheduledEventId,
                        contactId = contactId,
                        clientId = clientId,
                        resourceId = resourceId,
                        crewId = crewId,
                        schedulingTargetId = schedulingTargetId,
                        financialTransactionId = financialTransactionId,
                        officeId = officeId,
                        campaignId = campaignId,
                        householdId = householdId,
                        constituentId = constituentId,
                        tributeId = tributeId,
                        volunteerProfileId = volunteerProfileId
                    };

                    Document saved = await _fileStorage.UploadDocumentAsync(document, securityUser.id);
                    uploadedResults.Add(saved.ToDTO());
                }

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                    $"Uploaded {uploadedResults.Count} document(s) to folder {folderId?.ToString() ?? "root"}",
                    securityUser.accountName);

                // Broadcast real-time update + invalidate cache
                _cache.InvalidateDocuments(tenantGuid);
                await BroadcastDocumentChangedAsync(tenantGuid, new { action = "uploaded", count = uploadedResults.Count, folderId });

                return Ok(uploadedResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading documents.");
                return StatusCode(500, "Error uploading documents.");
            }
        }


        // ═══════════════════════════════════════════════════════════════
        // CHUNKED UPLOAD ENDPOINTS
        // ═══════════════════════════════════════════════════════════════


        /// <summary>
        /// Initiates a chunked upload session.
        /// Returns sessionId and recommended chunkSize.
        /// </summary>
        [Route("api/FileManager/Upload/Init")]
        [HttpPost]
        public async Task<IActionResult> InitChunkedUpload(
            [FromQuery] long fileSizeBytes,
            [FromQuery] string fileName)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                // Quota pre-check
                var (usedBytes, _) = await _fileStorage.GetStorageUsageAsync(tenantGuid);
                if (usedBytes + fileSizeBytes > DEFAULT_QUOTA_BYTES)
                {
                    return BadRequest("Storage quota would be exceeded by this upload.");
                }

                string sessionId = await _chunkBuffer.InitSessionAsync();
                int totalChunks = (int)Math.Ceiling((double)fileSizeBytes / ChunkBufferService.DEFAULT_CHUNK_SIZE);

                _logger.LogInformation("Chunked upload session {SessionId} initiated for '{FileName}' ({Size} bytes, {Chunks} chunks).",
                    sessionId, fileName, fileSizeBytes, totalChunks);

                return Ok(new
                {
                    sessionId,
                    chunkSize = ChunkBufferService.DEFAULT_CHUNK_SIZE,
                    totalChunks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating chunked upload.");
                return StatusCode(500, "Error initiating upload session.");
            }
        }


        /// <summary>
        /// Receives a single chunk for a chunked upload session.
        /// </summary>
        [Route("api/FileManager/Upload/Chunk")]
        [HttpPost]
        [RequestSizeLimit(8 * 1024 * 1024)] // Allow up to 8MB per chunk request
        public async Task<IActionResult> UploadChunk(
            [FromQuery] string sessionId,
            [FromQuery] int chunkIndex,
            [FromQuery] string chunkHash)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                if (!_chunkBuffer.HasSession(sessionId))
                {
                    return BadRequest("Invalid or expired upload session.");
                }

                IFormFileCollection files = Request.Form.Files;
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No chunk data provided.");
                }

                byte[] chunkData;
                using (MemoryStream ms = new MemoryStream())
                {
                    await files[0].CopyToAsync(ms);
                    chunkData = ms.ToArray();
                }

                bool accepted = await _chunkBuffer.AddChunkAsync(sessionId, chunkIndex, chunkData, chunkHash);

                if (!accepted)
                {
                    return BadRequest($"Chunk {chunkIndex} integrity check failed. Expected hash: {chunkHash}.");
                }

                long receivedCount = await _chunkBuffer.GetChunkCountAsync(sessionId);

                return Ok(new { chunkIndex, received = receivedCount, verified = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving chunk {ChunkIndex} for session {SessionId}.", chunkIndex, sessionId);
                return StatusCode(500, "Error receiving chunk.");
            }
        }


        /// <summary>
        /// Completes a chunked upload — assembles chunks, verifies total hash, saves document.
        /// </summary>
        [Route("api/FileManager/Upload/Complete")]
        [HttpPost]
        public async Task<IActionResult> CompleteChunkedUpload(
            [FromQuery] string sessionId,
            [FromQuery] string totalHash,
            [FromQuery] int? folderId = null,
            [FromQuery] int? documentTypeId = null,
            [FromQuery] int? scheduledEventId = null,
            [FromQuery] int? contactId = null,
            [FromQuery] int? clientId = null,
            [FromQuery] int? resourceId = null,
            [FromQuery] int? crewId = null,
            [FromQuery] int? schedulingTargetId = null,
            [FromQuery] int? financialTransactionId = null,
            [FromQuery] int? officeId = null,
            [FromQuery] int? campaignId = null,
            [FromQuery] int? householdId = null,
            [FromQuery] int? constituentId = null,
            [FromQuery] int? tributeId = null,
            [FromQuery] int? volunteerProfileId = null,
            [FromQuery] string fileName = null,
            [FromQuery] string mimeType = null)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                if (!_chunkBuffer.HasSession(sessionId))
                {
                    return BadRequest("Invalid or expired upload session.");
                }

                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                // Assemble all chunks and verify total hash
                var (fileBytes, verified) = await _chunkBuffer.AssembleAsync(sessionId, totalHash);

                if (!verified)
                {
                    _chunkBuffer.CleanupSession(sessionId);
                    return BadRequest("Total file integrity check failed. The file may have been corrupted during transmission.");
                }

                // Quota re-check (in case other uploads completed while this one was chunking)
                var (usedBytes, _) = await _fileStorage.GetStorageUsageAsync(tenantGuid);
                if (usedBytes + fileBytes.Length > DEFAULT_QUOTA_BYTES)
                {
                    _chunkBuffer.CleanupSession(sessionId);
                    return BadRequest("Storage quota exceeded.");
                }

                // Save document
                Document document = new Document
                {
                    tenantGuid = tenantGuid,
                    name = Path.GetFileNameWithoutExtension(fileName ?? "upload"),
                    fileName = fileName ?? "upload",
                    mimeType = mimeType ?? "application/octet-stream",
                    fileSizeBytes = fileBytes.Length,
                    fileDataFileName = fileName ?? "upload",
                    fileDataSize = fileBytes.Length,
                    fileDataData = fileBytes,
                    fileDataMimeType = mimeType ?? "application/octet-stream",
                    documentFolderId = folderId,
                    documentTypeId = documentTypeId,
                    uploadedBy = securityUser.accountName,
                    scheduledEventId = scheduledEventId,
                    contactId = contactId,
                    clientId = clientId,
                    resourceId = resourceId,
                    crewId = crewId,
                    schedulingTargetId = schedulingTargetId,
                    financialTransactionId = financialTransactionId,
                    officeId = officeId,
                    campaignId = campaignId,
                    householdId = householdId,
                    constituentId = constituentId,
                    tributeId = tributeId,
                    volunteerProfileId = volunteerProfileId
                };

                Document saved = await _fileStorage.UploadDocumentAsync(document, securityUser.id);

                // Release the large byte array as early as possible
                document.fileDataData = null;
                fileBytes = null;

                _chunkBuffer.CleanupSession(sessionId);

                // For large uploads, hint GC to reclaim the ~500MB+ buffer
                // before the next upload starts
                if (saved.fileSizeBytes > 50 * 1024 * 1024) // > 50MB
                {
                    GC.Collect(2, GCCollectionMode.Optimized, blocking: false);
                }

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                    $"Uploaded document '{fileName}' via chunked upload ({saved.fileSizeBytes} bytes)",
                    securityUser.accountName);

                _cache.InvalidateDocuments(tenantGuid);
                await BroadcastDocumentChangedAsync(tenantGuid, new { action = "uploaded", count = 1, folderId });

                _logger.LogInformation("Chunked upload session {SessionId} completed. Document {DocumentId} saved.",
                    sessionId, saved.id);

                return Ok(saved.ToDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing chunked upload for session {SessionId}.", sessionId);
                _chunkBuffer.CleanupSession(sessionId);
                return StatusCode(500, "Error completing upload.");
            }
        }


        /// <summary>
        /// Cancels / cleans up an upload session.
        /// </summary>
        [Route("api/FileManager/Upload/{sessionId}")]
        [HttpDelete]
        public async Task<IActionResult> CancelChunkedUpload(string sessionId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            _chunkBuffer.CleanupSession(sessionId);
            _logger.LogInformation("Chunked upload session {SessionId} cancelled.", sessionId);
            return Ok(new { cancelled = true, sessionId });
        }


        /// <summary>
        /// Downloads a document's binary content.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/Download")]
        [HttpGet]
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                Document document = await _fileStorage.GetDocumentByIdAsync(documentId, tenantGuid);

                if (document == null)
                {
                    return NotFound();
                }

                if (document.fileDataData == null || document.fileDataData.Length == 0)
                {
                    return NotFound("Document has no file content.");
                }

                var stream = new MemoryStream(document.fileDataData);
                return File(stream, document.mimeType ?? "application/octet-stream", document.fileName, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document {DocumentId}.", documentId);
                return StatusCode(500, "Error downloading document.");
            }
        }


        /// <summary>
        /// Returns an 80×80 PNG thumbnail for a document (if it's a supported image format).
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/Thumbnail")]
        [HttpGet]
        public async Task<IActionResult> GetThumbnail(int documentId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                // Cache-first: return cached thumbnail if available
                byte[] cached = _cache.GetThumbnail(documentId, tenantGuid);
                if (cached != null)
                {
                    return File(cached, "image/png");
                }

                // Cache miss: load from DB, generate, and cache
                Document document = await _fileStorage.GetDocumentByIdAsync(documentId, tenantGuid);

                if (document == null || document.fileDataData == null)
                {
                    return NotFound();
                }

                if (!ThumbnailGenerator.IsSupportedFormat(document.mimeType))
                {
                    return NotFound("Thumbnail not supported for this file type.");
                }

                byte[] thumbnail = ThumbnailGenerator.GenerateThumbnail(document.fileDataData, document.mimeType, 80);

                if (thumbnail == null)
                {
                    return NotFound("Could not generate thumbnail.");
                }

                // Store in cache for future requests
                _cache.StoreThumbnail(documentId, tenantGuid, thumbnail);

                return File(thumbnail, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnail for document {DocumentId}.", documentId);
                return StatusCode(500, "Error generating thumbnail.");
            }
        }


        /// <summary>
        /// Downloads an entire folder (and subfolders) as a zip archive.
        /// Streams directly to the response body — only one document blob
        /// is in memory at a time.
        /// </summary>
        [Route("api/FileManager/Folders/{folderId}/Download")]
        [HttpGet]
        public async Task<IActionResult> DownloadFolderAsZip(int folderId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                DocumentFolder folder = await _fileStorage.GetFolderByIdAsync(folderId, tenantGuid);
                if (folder == null)
                {
                    return NotFound("Folder not found.");
                }

                List<DocumentFolder> allFolders = await _fileStorage.GetFoldersAsync(tenantGuid);

                // Collect document metadata + relative paths (no blobs yet)
                var docRefs = new List<(int docId, string fileName, string relativePath)>();
                await CollectDocumentRefsRecursively(folderId, "", allFolders, tenantGuid, docRefs);

                if (docRefs.Count == 0)
                {
                    return NotFound("Folder contains no files.");
                }

                // Set response headers before streaming
                Response.ContentType = "application/zip";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{folder.name}.zip\"";

                // Stream the zip directly to the response body
                using (var archive = new ZipArchive(Response.Body, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var (docId, fileName, relativePath) in docRefs)
                    {
                        // Fetch one blob at a time
                        Document doc = await _fileStorage.GetDocumentByIdAsync(docId, tenantGuid);
                        if (doc?.fileDataData == null || doc.fileDataData.Length == 0) continue;

                        string entryPath = string.IsNullOrEmpty(relativePath)
                            ? fileName
                            : $"{relativePath}/{fileName}";

                        var entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal);
                        using var entryStream = entry.Open();
                        await entryStream.WriteAsync(doc.fileDataData);
                    }
                }

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading folder {FolderId} as zip.", folderId);
                return StatusCode(500, "Error creating zip archive.");
            }
        }


        /// <summary>
        /// Downloads multiple documents as a zip archive.
        /// Streams directly to the response body.
        /// </summary>
        [Route("api/FileManager/Documents/DownloadZip")]
        [HttpPost]
        public async Task<IActionResult> DownloadDocumentsAsZip([FromBody] List<int> documentIds)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            if (documentIds == null || documentIds.Count == 0)
            {
                return BadRequest("No document IDs provided.");
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                // Set response headers before streaming
                Response.ContentType = "application/zip";
                Response.Headers["Content-Disposition"] = "attachment; filename=\"Documents.zip\"";

                using (var archive = new ZipArchive(Response.Body, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (int docId in documentIds)
                    {
                        Document document = await _fileStorage.GetDocumentByIdAsync(docId, tenantGuid);
                        if (document?.fileDataData == null || document.fileDataData.Length == 0) continue;

                        // Deduplicate file names
                        string name = document.fileName;
                        if (usedNames.Contains(name))
                        {
                            string baseName = Path.GetFileNameWithoutExtension(name);
                            string ext = Path.GetExtension(name);
                            int count = 2;
                            while (usedNames.Contains($"{baseName} ({count}){ext}")) count++;
                            name = $"{baseName} ({count}){ext}";
                        }
                        usedNames.Add(name);

                        var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
                        using var entryStream = entry.Open();
                        await entryStream.WriteAsync(document.fileDataData);
                    }
                }

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading documents as zip.");
                return StatusCode(500, "Error creating zip archive.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  RECYCLE BIN / TRASH
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns all soft-deleted documents for the current tenant.
        /// </summary>
        [Route("api/FileManager/Trash")]
        [HttpGet]
        public async Task<IActionResult> GetTrash()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);
                List<Document> deleted = await _fileStorage.GetDeletedDocumentsAsync(tenantGuid);

                return Ok(deleted.Select(d => new {
                    d.id, d.name, d.fileName, d.mimeType, d.fileSizeBytes,
                    d.uploadedDate, d.uploadedBy, d.documentFolderId
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trash.");
                return StatusCode(500, "Error retrieving trash.");
            }
        }


        /// <summary>
        /// Restores a soft-deleted document.
        /// </summary>
        [Route("api/FileManager/Trash/{documentId}/Restore")]
        [HttpPost]
        public async Task<IActionResult> RestoreFromTrash(int documentId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);
                await _fileStorage.RestoreDocumentAsync(documentId, tenantGuid, securityUser.id);
                _cache.InvalidateDocuments(tenantGuid);
                await BroadcastDocumentChangedAsync(tenantGuid, new { action = "restored", documentId });

                return Ok(new { message = "Document restored." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring document {DocumentId}.", documentId);
                return StatusCode(500, "Error restoring document.");
            }
        }


        /// <summary>
        /// Permanently deletes a document from the database.
        /// </summary>
        [Route("api/FileManager/Trash/{documentId}")]
        [HttpDelete]
        public async Task<IActionResult> PermanentlyDelete(int documentId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);
                await _fileStorage.PermanentlyDeleteDocumentAsync(documentId, tenantGuid, securityUser.id);

                return Ok(new { message = "Document permanently deleted." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting document {DocumentId}.", documentId);
                return StatusCode(500, "Error permanently deleting document.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  VERSION HISTORY
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns all versions of a document, sorted descending by version number.
        /// The current state comes from the Document row; previous versions come from
        /// DocumentChangeHistory (snapshots created by the ChangeHistoryToolset).
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/Versions")]
        [HttpGet]
        public async Task<IActionResult> GetDocumentVersions(int documentId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                // Load the current document (the "latest" version)
                var current = await _db.Documents
                    .Where(d => d.id == documentId && d.tenantGuid == tenantGuid)
                    .Select(d => new {
                        d.id, d.name, d.fileName, d.mimeType, d.fileSizeBytes,
                        d.uploadedDate, d.uploadedBy, d.versionNumber, d.objectGuid
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (current == null)
                    return NotFound("Document not found.");

                // Load previous versions from change history
                // Exclude any entry whose versionNumber matches the current document's —
                // the current row already represents the latest version.
                var history = await _db.Set<DocumentChangeHistory>()
                    .Where(h => h.documentId == documentId && h.tenantGuid == tenantGuid
                                && h.versionNumber != current.versionNumber)
                    .OrderByDescending(h => h.versionNumber)
                    .Select(h => new {
                        h.id,
                        h.versionNumber,
                        h.timeStamp,
                        h.userId,
                        h.data
                    })
                    .AsNoTracking()
                    .ToListAsync();

                // Build unified version list: current + history entries
                var results = new List<object>();

                // Add the current (latest) version
                results.Add(new {
                    current.id,
                    current.name,
                    current.fileName,
                    current.mimeType,
                    current.fileSizeBytes,
                    current.uploadedDate,
                    current.uploadedBy,
                    current.versionNumber,
                    current.objectGuid,
                    isCurrent = true
                });

                // Add each change history snapshot as a previous version
                foreach (var h in history)
                {
                    // The data field contains a JSON snapshot; parse key fields
                    string uploadedBy = null;
                    DateTime? uploadedDate = null;
                    string fileName = null;
                    long? fileSizeBytes = null;

                    if (!string.IsNullOrEmpty(h.data))
                    {
                        try
                        {
                            var json = System.Text.Json.JsonDocument.Parse(h.data);
                            if (json.RootElement.TryGetProperty("uploadedBy", out var ub))
                                uploadedBy = ub.GetString();
                            if (json.RootElement.TryGetProperty("uploadedDate", out var ud))
                                uploadedDate = ud.GetDateTime();
                            if (json.RootElement.TryGetProperty("fileName", out var fn))
                                fileName = fn.GetString();
                            if (json.RootElement.TryGetProperty("fileSizeBytes", out var fs))
                                fileSizeBytes = fs.GetInt64();
                        }
                        catch { /* If JSON parse fails, leave fields null */ }
                    }

                    results.Add(new {
                        id = h.id,
                        name = fileName ?? current.name,
                        fileName = fileName ?? current.fileName,
                        mimeType = current.mimeType,
                        fileSizeBytes = fileSizeBytes ?? 0L,
                        uploadedDate = uploadedDate ?? h.timeStamp,
                        uploadedBy = uploadedBy ?? "System",
                        versionNumber = h.versionNumber,
                        objectGuid = current.objectGuid,
                        isCurrent = false
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving versions for document {DocumentId}.", documentId);
                return StatusCode(500, "Error retrieving version history.");
            }
        }


        /// <summary>
        /// Uploads a new version of an existing document, preserving the same objectGuid.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/NewVersion")]
        [HttpPost]
        public async Task<IActionResult> UploadNewVersion(int documentId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                //
                // Load the existing document from the DB context so we can update it in-place.
                //
                Document existing = await _db.Documents
                    .Where(d => d.id == documentId && d.tenantGuid == tenantGuid && d.deleted == false)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    return NotFound("Document not found.");
                }

                IFormFileCollection files = Request.Form.Files;
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No file provided.");
                }

                IFormFile file = files[0];
                byte[] fileBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }

                //
                // Update the existing document's file content in-place.
                // The ChangeHistoryToolset will snapshot the old state for version history.
                //
                existing.name = Path.GetFileNameWithoutExtension(file.FileName);
                existing.fileName = file.FileName;
                existing.mimeType = file.ContentType ?? "application/octet-stream";
                existing.fileSizeBytes = file.Length;
                existing.fileDataFileName = file.FileName;
                existing.fileDataSize = file.Length;
                existing.fileDataData = fileBytes;
                existing.fileDataMimeType = file.ContentType ?? "application/octet-stream";
                existing.uploadedBy = securityUser.accountName;
                existing.uploadedDate = DateTime.UtcNow;

                var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<Document, DocumentChangeHistory>(
                    _db, securityUser.id, false);
                await chts.SaveEntityAsync(existing).ConfigureAwait(false);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                    $"Uploaded new version (v{existing.versionNumber}) for document {documentId}",
                    securityUser.accountName);
                _cache.InvalidateDocuments(tenantGuid);
                _cache.InvalidateThumbnail(documentId, tenantGuid);
                await BroadcastDocumentChangedAsync(tenantGuid, new { action = "newVersion", documentId, versionNumber = existing.versionNumber });

                return Ok(existing.ToDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading new version for document {DocumentId}.", documentId);
                return StatusCode(500, "Error uploading new version.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  TEXT CONTENT (for in-app Markdown editor)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Helper: MIME types that the text editor is allowed to open.
        /// </summary>
        private static readonly HashSet<string> EditableMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "text/plain", "text/markdown", "text/html", "text/css", "text/csv",
            "text/xml", "text/javascript", "application/json", "application/xml",
            "application/javascript"
        };

        /// <summary>
        /// Returns a document's content as a UTF-8 string.
        /// Only works for text-based MIME types.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/Content")]
        [HttpGet]
        public async Task<IActionResult> GetDocumentContent(int documentId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                Document document = await _fileStorage.GetDocumentByIdAsync(documentId, tenantGuid);
                if (document == null)
                {
                    return NotFound("Document not found.");
                }

                // Only serve text-based files
                string mime = document.mimeType ?? "";
                if (!EditableMimeTypes.Contains(mime) && !mime.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Document is not a text-based file and cannot be edited.");
                }

                string content = document.fileDataData != null
                    ? System.Text.Encoding.UTF8.GetString(document.fileDataData)
                    : "";

                return Ok(new { content, document.versionNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading content for document {DocumentId}.", documentId);
                return StatusCode(500, "Error reading document content.");
            }
        }


        /// <summary>
        /// Saves text content as a new version of the document.
        /// Creates a version entry in history, preserving the same objectGuid.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/Content")]
        [HttpPut]
        public async Task<IActionResult> SaveDocumentContent(int documentId, [FromBody] SaveContentRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                //
                // Load the existing document from the DB context so we can update it in-place.
                // This is important — we must use the tracked entity for ChangeHistoryToolset
                // to properly snapshot the old state before saving.
                //
                Document existing = await _db.Documents
                    .Where(d => d.id == documentId && d.tenantGuid == tenantGuid && d.deleted == false)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    return NotFound("Document not found.");
                }

                string mime = existing.mimeType ?? "";
                if (!EditableMimeTypes.Contains(mime) && !mime.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Document is not a text-based file and cannot be edited.");
                }

                byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(request.Content ?? "");

                //
                // Update the existing document's content fields in-place.
                // The ChangeHistoryToolset will snapshot the old state automatically.
                //
                existing.fileDataData = contentBytes;
                existing.fileDataSize = contentBytes.Length;
                existing.fileSizeBytes = contentBytes.Length;
                existing.uploadedBy = securityUser.accountName;
                existing.uploadedDate = DateTime.UtcNow;

                //
                // Use ChangeHistoryToolset to save — this creates a DocumentChangeHistory
                // record with the previous state, providing proper version control.
                //
                var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<Document, DocumentChangeHistory>(
                    _db, securityUser.id, false);
                await chts.SaveEntityAsync(existing).ConfigureAwait(false);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                    $"Saved text content (v{existing.versionNumber}) for document {documentId}",
                    securityUser.accountName);
                _cache.InvalidateDocuments(tenantGuid);

                await BroadcastDocumentChangedAsync(tenantGuid,
                    new { action = "contentSaved", documentId, versionNumber = existing.versionNumber });

                return Ok(existing.ToOutputDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving content for document {DocumentId}.", documentId);
                return StatusCode(500, "Error saving document content.");
            }
        }

        /// <summary>
        /// Request body for the SaveDocumentContent endpoint.
        /// </summary>
        public class SaveContentRequest
        {
            public string Content { get; set; } = "";
        }


        /// <summary>
        /// Creates a new blank text/markdown document in the specified folder (or root).
        /// Returns the created document so the client can immediately open it in the editor.
        /// </summary>
        [Route("api/FileManager/Documents/CreateTextFile")]
        [HttpPost]
        public async Task<IActionResult> CreateTextDocument([FromBody] CreateTextFileRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                string docName = (request.Name ?? "Untitled").Trim();
                if (string.IsNullOrWhiteSpace(docName)) docName = "Untitled";

                // Ensure .md extension
                string fileName = docName;
                if (!fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase) &&
                    !fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".md";
                }

                byte[] content = System.Text.Encoding.UTF8.GetBytes($"# {docName}\n\n");

                Document document = new Document
                {
                    tenantGuid = tenantGuid,
                    name = docName,
                    fileName = fileName,
                    mimeType = fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                        ? "text/plain" : "text/markdown",
                    fileSizeBytes = content.Length,
                    fileDataFileName = fileName,
                    fileDataSize = content.Length,
                    fileDataData = content,
                    fileDataMimeType = fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                        ? "text/plain" : "text/markdown",
                    documentFolderId = request.FolderId,
                    uploadedBy = securityUser.accountName,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false,
                    uploadedDate = DateTime.UtcNow
                };

                var chts = new Foundation.ChangeHistory.ChangeHistoryToolset<Document, DocumentChangeHistory>(
                    _db, securityUser.id, false);
                await chts.SaveEntityAsync(document).ConfigureAwait(false);

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                    $"Created text document '{fileName}'",
                    securityUser.accountName);
                _cache.InvalidateDocuments(tenantGuid);

                await BroadcastDocumentChangedAsync(tenantGuid,
                    new { action = "created", documentId = document.id });

                return Ok(document.ToOutputDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating text document.");
                return StatusCode(500, "Error creating text document.");
            }
        }

        public class CreateTextFileRequest
        {
            public string Name { get; set; } = "Untitled";
            public int? FolderId { get; set; }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  SCRATCHPAD (Entity Notes)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Allowed entity types for scratchpad operations.
        /// Maps entity type name → (FK property name, display term).
        /// </summary>
        private static readonly Dictionary<string, (string fkProperty, string display)> ScratchpadEntityMap
            = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Client"]           = ("clientId",                "Client"),
            ["Contact"]          = ("contactId",               "Contact"),
            ["Resource"]         = ("resourceId",              "Resource"),
            ["Office"]           = ("officeId",                "Office"),
            ["Crew"]             = ("crewId",                  "Crew"),
            ["SchedulingTarget"] = ("schedulingTargetId",      "Target"),
            ["VolunteerProfile"] = ("volunteerProfileId",      "Volunteer"),
            ["Invoice"]          = ("invoiceId",               "Invoice"),
            ["Receipt"]          = ("receiptId",               "Receipt"),
            ["PaymentTransaction"]=("paymentTransactionId",    "Payment"),
            ["ScheduledEvent"]   = ("scheduledEventId",        "Event"),
        };

        /// <summary>
        /// Returns the active scratchpad document for a given entity, or 404 if none exists.
        /// A scratchpad is a text/markdown document in the _Notes folder linked to the entity.
        /// </summary>
        [Route("api/FileManager/Scratchpad/{entityType}/{entityId}")]
        [HttpGet]
        public async Task<IActionResult> GetScratchpad(string entityType, int entityId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
                return Forbid();

            if (!ScratchpadEntityMap.ContainsKey(entityType))
                return BadRequest($"Unknown entity type: {entityType}");

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                var (fkProp, _) = ScratchpadEntityMap[entityType];
                var param = System.Linq.Expressions.Expression.Parameter(typeof(Document), "d");
                var prop = System.Linq.Expressions.Expression.Property(param, fkProp);
                var val = System.Linq.Expressions.Expression.Constant(entityId, typeof(int?));
                var eq = System.Linq.Expressions.Expression.Equal(prop, val);
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<Document, bool>>(eq, param);

                // Find the most recent text/markdown document linked to this entity
                Document doc = await _db.Documents
                    .Where(d => d.tenantGuid == tenantGuid && d.active == true && d.deleted != true)
                    .Where(d => d.mimeType == "text/markdown")
                    .Where(lambda)
                    .OrderByDescending(d => d.id)
                    .FirstOrDefaultAsync();

                if (doc == null)
                    return NotFound();

                return Ok(doc.ToDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scratchpad for {EntityType}/{EntityId}.", entityType, entityId);
                return StatusCode(500, "Error retrieving scratchpad.");
            }
        }


        /// <summary>
        /// Creates a new scratchpad document for an entity.
        /// Creates a _Notes system folder if it doesn't exist.
        /// </summary>
        [Route("api/FileManager/Scratchpad/{entityType}/{entityId}")]
        [HttpPost]
        public async Task<IActionResult> CreateScratchpad(string entityType, int entityId, [FromQuery] string entityName = "")
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
                return Forbid();

            if (!ScratchpadEntityMap.ContainsKey(entityType))
                return BadRequest($"Unknown entity type: {entityType}");

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                // Ensure _Notes folder exists
                DocumentFolder notesFolder = await _db.DocumentFolders
                    .FirstOrDefaultAsync(f => f.tenantGuid == tenantGuid && f.name == "_Notes" && f.active == true);

                if (notesFolder == null)
                {
                    notesFolder = new DocumentFolder
                    {
                        tenantGuid = tenantGuid,
                        name = "_Notes",
                        description = "System-managed folder for entity scratchpad notes",
                        objectGuid = Guid.NewGuid(),
                        versionNumber = 1,
                        active = true,
                        deleted = false
                    };
                    _db.DocumentFolders.Add(notesFolder);
                    await _db.SaveChangesAsync();
                    _cache.InvalidateFolders(tenantGuid);
                }

                var (fkProp, displayName) = ScratchpadEntityMap[entityType];
                string docName = !string.IsNullOrWhiteSpace(entityName)
                    ? $"{displayName} - {entityName} Notes"
                    : $"{displayName} Notes";
                string fileName = docName + ".md";

                byte[] emptyContent = System.Text.Encoding.UTF8.GetBytes($"# {docName}\n\n");

                Document newDoc = new Document
                {
                    tenantGuid = tenantGuid,
                    name = docName,
                    fileName = fileName,
                    mimeType = "text/markdown",
                    fileSizeBytes = emptyContent.Length,
                    fileDataFileName = fileName,
                    fileDataSize = emptyContent.Length,
                    fileDataData = emptyContent,
                    fileDataMimeType = "text/markdown",
                    documentFolderId = notesFolder.id,
                    uploadedBy = securityUser.accountName,
                    objectGuid = Guid.NewGuid(),
                    versionNumber = 1,
                    active = true,
                    deleted = false
                };

                // Set the entity FK using reflection
                typeof(Document).GetProperty(fkProp)?.SetValue(newDoc, entityId);

                Document saved = await _fileStorage.UploadDocumentAsync(newDoc, securityUser.id);

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                    $"Created scratchpad note for {entityType} {entityId}: {docName}",
                    securityUser.accountName);
                _cache.InvalidateDocuments(tenantGuid);

                await BroadcastDocumentChangedAsync(tenantGuid,
                    new { action = "scratchpadCreated", documentId = saved.id, entityType, entityId });

                return Ok(saved.ToDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating scratchpad for {EntityType}/{EntityId}.", entityType, entityId);
                return StatusCode(500, "Error creating scratchpad.");
            }
        }


        /// <summary>
        /// Archives the current scratchpad for an entity (renames with timestamp)
        /// and creates a fresh empty one.
        /// </summary>
        [Route("api/FileManager/Scratchpad/{entityType}/{entityId}/Archive")]
        [HttpPost]
        public async Task<IActionResult> ArchiveScratchpad(string entityType, int entityId, [FromQuery] string entityName = "")
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
                return Forbid();

            if (!ScratchpadEntityMap.ContainsKey(entityType))
                return BadRequest($"Unknown entity type: {entityType}");

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                var (fkProp, _) = ScratchpadEntityMap[entityType];
                var param = System.Linq.Expressions.Expression.Parameter(typeof(Document), "d");
                var prop = System.Linq.Expressions.Expression.Property(param, fkProp);
                var val = System.Linq.Expressions.Expression.Constant(entityId, typeof(int?));
                var eq = System.Linq.Expressions.Expression.Equal(prop, val);
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<Document, bool>>(eq, param);

                Document existing = await _db.Documents
                    .Where(d => d.tenantGuid == tenantGuid && d.active == true && d.deleted != true)
                    .Where(d => d.mimeType == "text/markdown")
                    .Where(lambda)
                    .OrderByDescending(d => d.id)
                    .FirstOrDefaultAsync();

                if (existing == null)
                    return NotFound("No active scratchpad found to archive.");

                // Rename the existing document with a timestamp suffix
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd");
                existing.name = $"{existing.name} ({timestamp})";
                existing.fileName = $"{existing.name}.md";
                await _db.SaveChangesAsync();

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                    $"Archived scratchpad note: {existing.name}",
                    securityUser.accountName);

                _cache.InvalidateDocuments(tenantGuid);

                // Create a fresh scratchpad (delegate to the Create endpoint logic)
                return await CreateScratchpad(entityType, entityId, entityName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving scratchpad for {EntityType}/{EntityId}.", entityType, entityId);
                return StatusCode(500, "Error archiving scratchpad.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  STORAGE QUOTA
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns storage usage stats for the current tenant.
        /// </summary>
        [Route("api/FileManager/Storage")]
        [HttpGet]
        public async Task<IActionResult> GetStorageUsage()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);
                var (totalBytes, documentCount) = await _fileStorage.GetStorageUsageAsync(tenantGuid);

                return Ok(new { totalBytes, documentCount, quotaBytes = DEFAULT_QUOTA_BYTES });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving storage usage.");
                return StatusCode(500, "Error retrieving storage usage.");
            }
        }


        /// <summary>
        /// Returns recent document activity for the current tenant.
        /// </summary>
        [Route("api/FileManager/Activity")]
        [HttpGet]
        public async Task<IActionResult> GetRecentActivity([FromQuery] int count = 50)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                //
                // Project only lightweight columns — skip h.data (large JSON change snapshots)
                // and avoid materializing the full Document entity for the name.
                //
                var activity = await _db.DocumentChangeHistories
                    .Where(h => h.tenantGuid == tenantGuid)
                    .Join(
                        _db.Documents,
                        h => h.documentId,
                        d => d.id,
                        (h, d) => new
                        {
                            h.id,
                            h.documentId,
                            documentName = d.name,
                            documentFileName = d.fileName,
                            h.versionNumber,
                            h.timeStamp,
                            h.userId
                        })
                    .OrderByDescending(x => x.timeStamp)
                    .Take(Math.Min(count, 100))
                    .AsNoTracking()
                    .ToListAsync();

                //
                // Batch-resolve user display names (avoids N+1 per userId)
                //
                var userIds = activity.Select(a => a.userId).Distinct().ToList();
                var userNameMap = new Dictionary<int, string>();

                // Resolve all at once — GetChangeHistoryUserAsync has its own internal cache
                foreach (var uid in userIds)
                {
                    var changeHistoryUser = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(uid, tenantGuid);
                    if (changeHistoryUser != null)
                    {
                        string displayName = !string.IsNullOrWhiteSpace(changeHistoryUser.firstName) || !string.IsNullOrWhiteSpace(changeHistoryUser.lastName)
                            ? $"{changeHistoryUser.firstName} {changeHistoryUser.lastName}".Trim()
                            : changeHistoryUser.userName ?? $"User #{uid}";
                        userNameMap[uid] = displayName;
                    }
                }

                var enriched = activity.Select(a => new
                {
                    a.id,
                    a.documentId,
                    documentName = a.documentFileName ?? a.documentName,
                    a.versionNumber,
                    a.timeStamp,
                    a.userId,
                    userName = userNameMap.GetValueOrDefault(a.userId, $"User #{a.userId}")
                });

                return Ok(enriched);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activity.");
                return StatusCode(500, "Error retrieving activity.");
            }
        }


        /// <summary>
        /// Recursively collects document metadata (id, fileName, relative path)
        /// without loading blob data — blobs are fetched one-at-a-time during zip streaming.
        /// </summary>
        private async Task CollectDocumentRefsRecursively(
            int folderId,
            string currentPath,
            List<DocumentFolder> allFolders,
            Guid tenantGuid,
            List<(int docId, string fileName, string relativePath)> results)
        {
            List<Document> metaDocs = await _fileStorage.GetDocumentsInFolderAsync(folderId, tenantGuid);
            foreach (var meta in metaDocs.Where(d => !d.deleted))
            {
                results.Add((meta.id, meta.fileName, currentPath));
            }

            // Recurse into subfolders
            var children = allFolders.Where(f => f.parentDocumentFolderId == folderId && !f.deleted).ToList();
            foreach (var child in children)
            {
                string childPath = string.IsNullOrEmpty(currentPath)
                    ? child.name
                    : $"{currentPath}/{child.name}";
                await CollectDocumentRefsRecursively(child.id, childPath, allFolders, tenantGuid, results);
            }
        }


        /// <summary>
        /// Returns document metadata (for the properties panel / edit modal).
        /// Binary data is NOT included.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}")]
        [HttpGet]
        public async Task<IActionResult> GetDocument(int documentId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                Document document = await _fileStorage.GetDocumentByIdAsync(documentId, tenantGuid);

                if (document == null)
                {
                    return NotFound();
                }

                //
                // Return OutputDTO (with nav properties) but null out the binary data to save bandwidth
                //
                Document.DocumentOutputDTO dto = document.ToOutputDTO();
                dto.fileDataData = null;

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching document {DocumentId}.", documentId);
                return StatusCode(500, "Error fetching document.");
            }
        }


        /// <summary>
        /// Updates document metadata (name, description, folder, type, entity links, etc.).
        /// Does not touch binary content.
        /// </summary>
        [Route("api/FileManager/Documents")]
        [HttpPut]
        public async Task<IActionResult> UpdateDocumentMetadata([FromBody] Document.DocumentDTO dto)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                Document document = Document.FromDTO(dto);
                document.tenantGuid = tenantGuid;

                Document updated = await _fileStorage.UpdateDocumentMetadataAsync(document, securityUser.id);

                _cache.InvalidateDocuments(tenantGuid);
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, $"Updated document '{updated.name}'", securityUser.accountName);
                await BroadcastDocumentChangedAsync(tenantGuid, new { action = "updated", documentId = updated.id });

                return Ok(updated.ToDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document metadata.");
                return StatusCode(500, "Error updating document metadata.");
            }
        }


        /// <summary>
        /// Moves a document to a different folder (or to root when targetFolderId is null).
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/Move")]
        [HttpPut]
        public async Task<IActionResult> MoveDocument(int documentId, [FromQuery] int? targetFolderId = null)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                await _fileStorage.MoveDocumentAsync(documentId, targetFolderId, tenantGuid, securityUser.id);
                _cache.InvalidateDocuments(tenantGuid);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                    $"Moved document {documentId} to folder {targetFolderId?.ToString() ?? "root"}",
                    securityUser.accountName);
                await BroadcastDocumentChangedAsync(tenantGuid, new { action = "moved", documentId, targetFolderId });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving document {DocumentId}.", documentId);
                return StatusCode(500, "Error moving document.");
            }
        }


        /// <summary>
        /// Soft-deletes a document.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                await _fileStorage.DeleteDocumentAsync(documentId, tenantGuid, securityUser.id);
                _cache.InvalidateDocuments(tenantGuid);
                _cache.InvalidateThumbnail(documentId, tenantGuid);

                await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, $"Deleted document {documentId}", securityUser.accountName);
                await BroadcastDocumentDeletedAsync(tenantGuid, new { action = "deleted", documentId });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}.", documentId);
                return StatusCode(500, "Error deleting document.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  SEARCH
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Searches documents across all folders by name, fileName, or description.
        /// Returns metadata only — no binary data.
        /// </summary>
        [Route("api/FileManager/Documents/Search")]
        [HttpGet]
        public async Task<IActionResult> SearchDocuments([FromQuery] string q)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                List<Document> results = await _fileStorage.SearchDocumentsAsync(q, tenantGuid);

                return Ok(Document.ToOutputDTOList(results));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents.");
                return StatusCode(500, "Error searching documents.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  TAG ENDPOINTS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns all tags for the current tenant.
        /// </summary>
        [Route("api/FileManager/Tags")]
        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                List<DocumentTag> tags = await _cache.GetTagsAsync(tenantGuid);

                return Ok(DocumentTag.ToDTOList(tags));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tags.");
                return StatusCode(500, "Error fetching tags.");
            }
        }


        /// <summary>
        /// Creates a new tag.
        /// </summary>
        [Route("api/FileManager/Tags")]
        [HttpPost]
        public async Task<IActionResult> CreateTag([FromBody] DocumentTag.DocumentTagDTO dto)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                DocumentTag tag = DocumentTag.FromDTO(dto);
                tag.tenantGuid = tenantGuid;

                DocumentTag created = await _fileStorage.CreateTagAsync(tag, securityUser.id);
                _cache.InvalidateTagMappings(tenantGuid);

                return Ok(created.ToDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag.");
                return StatusCode(500, "Error creating tag.");
            }
        }


        /// <summary>
        /// Updates an existing tag (name, color, description, etc.).
        /// </summary>
        [Route("api/FileManager/Tags")]
        [HttpPut]
        public async Task<IActionResult> UpdateTag([FromBody] DocumentTag.DocumentTagDTO dto)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                DocumentTag tag = DocumentTag.FromDTO(dto);
                tag.tenantGuid = tenantGuid;

                DocumentTag updated = await _fileStorage.UpdateTagAsync(tag, securityUser.id);
                _cache.InvalidateTagMappings(tenantGuid);

                return Ok(updated.ToDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag {TagId}.", dto?.id);
                return StatusCode(500, "Error updating tag.");
            }
        }


        /// <summary>
        /// Deletes a tag and removes all document-tag associations.
        /// </summary>
        [Route("api/FileManager/Tags/{tagId}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteTag(int tagId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                await _fileStorage.DeleteTagAsync(tagId, tenantGuid, securityUser.id);
                _cache.InvalidateTagMappings(tenantGuid);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag {TagId}.", tagId);
                return StatusCode(500, "Error deleting tag.");
            }
        }


        /// <summary>
        /// Adds a tag to a document.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/Tags/{tagId}")]
        [HttpPost]
        public async Task<IActionResult> AddTagToDocument(int documentId, int tagId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                await _fileStorage.AddTagToDocumentAsync(documentId, tagId, tenantGuid, securityUser.id);
                _cache.InvalidateTagMappings(tenantGuid);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tag {TagId} to document {DocumentId}.", tagId, documentId);
                return StatusCode(500, "Error adding tag.");
            }
        }


        /// <summary>
        /// Removes a tag from a document.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/Tags/{tagId}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveTagFromDocument(int documentId, int tagId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                await _fileStorage.RemoveTagFromDocumentAsync(documentId, tagId, tenantGuid, securityUser.id);
                _cache.InvalidateTagMappings(tenantGuid);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tag {TagId} from document {DocumentId}.", tagId, documentId);
                return StatusCode(500, "Error removing tag.");
            }
        }


        /// <summary>
        /// Returns all tags for a specific document.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/Tags")]
        [HttpGet]
        public async Task<IActionResult> GetTagsForDocument(int documentId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                List<DocumentTag> tags = await _fileStorage.GetTagsForDocumentAsync(documentId, tenantGuid);

                return Ok(DocumentTag.ToDTOList(tags));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tags for document {DocumentId}.", documentId);
                return StatusCode(500, "Error fetching tags.");
            }
        }


        /// <summary>
        /// Returns tag mappings for multiple documents in a single request.
        /// Eliminates the N+1 pattern where the client fetches tags per-document.
        /// </summary>
        [Route("api/FileManager/Documents/Tags/Batch")]
        [HttpGet]
        public async Task<IActionResult> GetTagsForDocumentsBatch([FromQuery] string documentIds)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                if (string.IsNullOrWhiteSpace(documentIds))
                {
                    return Ok(new Dictionary<string, object>());
                }

                int[] ids = documentIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out int val) ? val : -1)
                    .Where(id => id > 0)
                    .ToArray();

                Dictionary<int, List<DocumentTag>> tagMap = await _cache.GetTagsForDocumentsAsync(ids, tenantGuid);

                // Convert to serializable format
                var result = tagMap.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => DocumentTag.ToDTOList(kvp.Value) as object);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching batch tags.");
                return StatusCode(500, "Error fetching batch tags.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  SHARE LINK ENDPOINTS (Authenticated — for managing share links)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates a shareable download link for a document.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/ShareLink")]
        [HttpPost]
        public async Task<IActionResult> CreateShareLink(int documentId, [FromBody] CreateShareLinkRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                // Verify the document exists and belongs to this tenant
                Document document = await _fileStorage.GetDocumentByIdAsync(documentId, tenantGuid);
                if (document == null)
                {
                    return NotFound("Document not found.");
                }

                string passwordHash = null;
                if (!string.IsNullOrWhiteSpace(request?.Password))
                {
                    passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }

                var shareLink = new DocumentShareLink
                {
                    tenantGuid = tenantGuid,
                    documentId = documentId,
                    token = Guid.NewGuid(),
                    passwordHash = passwordHash,
                    expiresAt = request?.ExpiresAt,
                    maxDownloads = request?.MaxDownloads,
                    downloadCount = 0,
                    createdBy = securityUser.emailAddress ?? securityUser.accountName,
                    createdDate = DateTime.UtcNow,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _db.DocumentShareLinks.Add(shareLink);
                await _db.SaveChangesAsync();

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, $"Created share link for document '{document.name}' (ID: {documentId})");

                return Ok(shareLink.ToDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating share link for document {DocumentId}.", documentId);
                return StatusCode(500, "Error creating share link.");
            }
        }


        /// <summary>
        /// Lists all share links for a specific document.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/ShareLinks")]
        [HttpGet]
        public async Task<IActionResult> GetShareLinks(int documentId)
        {
            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                List<DocumentShareLink> links = await _db.DocumentShareLinks
                    .Where(l => l.tenantGuid == tenantGuid && l.documentId == documentId && !l.deleted)
                    .OrderByDescending(l => l.createdDate)
                    .ToListAsync();

                return Ok(DocumentShareLink.ToDTOList(links));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching share links for document {DocumentId}.", documentId);
                return StatusCode(500, "Error fetching share links.");
            }
        }


        /// <summary>
        /// Revokes (soft-deletes) a share link.
        /// </summary>
        [Route("api/FileManager/ShareLinks/{linkId}")]
        [HttpDelete]
        public async Task<IActionResult> RevokeShareLink(int linkId)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                DocumentShareLink link = await _db.DocumentShareLinks
                    .FirstOrDefaultAsync(l => l.id == linkId && l.tenantGuid == tenantGuid);

                if (link == null)
                {
                    return NotFound("Share link not found.");
                }

                link.deleted = true;
                link.active = false;
                await _db.SaveChangesAsync();

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, $"Revoked share link (ID: {linkId}) for document ID {link.documentId}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking share link {LinkId}.", linkId);
                return StatusCode(500, "Error revoking share link.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  SHARE LINK ENDPOINTS (Public — unauthenticated access)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns metadata about a share link (document name, whether password is required, etc.).
        /// This endpoint is unauthenticated — external users call it to see what they're downloading.
        /// </summary>
        [Route("api/Share/{token}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetShareLinkInfo(Guid token)
        {
            try
            {
                DocumentShareLink link = await _db.DocumentShareLinks
                    .Include(l => l.document)
                    .FirstOrDefaultAsync(l => l.token == token && l.active && !l.deleted);

                if (link == null)
                {
                    return NotFound("Share link not found or has been revoked.");
                }

                // Check expiry
                if (link.expiresAt.HasValue && link.expiresAt.Value < DateTime.UtcNow)
                {
                    return BadRequest("This share link has expired.");
                }

                // Check download limit
                if (link.maxDownloads.HasValue && link.downloadCount >= link.maxDownloads.Value)
                {
                    return BadRequest("This share link has reached its download limit.");
                }

                return Ok(new
                {
                    fileName = link.document?.fileName,
                    name = link.document?.name,
                    mimeType = link.document?.mimeType,
                    fileSizeBytes = link.document?.fileSizeBytes,
                    requiresPassword = !string.IsNullOrEmpty(link.passwordHash),
                    expiresAt = link.expiresAt,
                    maxDownloads = link.maxDownloads,
                    downloadCount = link.downloadCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting share link info for token {Token}.", token);
                return StatusCode(500, "Error retrieving share link info.");
            }
        }


        /// <summary>
        /// Verifies the password for a password-protected share link.
        /// Returns a short-lived verification token if successful.
        /// </summary>
        [Route("api/Share/{token}/Verify")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyShareLinkPassword(Guid token, [FromBody] VerifyPasswordRequest request)
        {
            try
            {
                DocumentShareLink link = await _db.DocumentShareLinks
                    .FirstOrDefaultAsync(l => l.token == token && l.active && !l.deleted);

                if (link == null)
                {
                    return NotFound("Share link not found.");
                }

                if (string.IsNullOrEmpty(link.passwordHash))
                {
                    // No password required — already verified
                    return Ok(new { verified = true });
                }

                if (string.IsNullOrWhiteSpace(request?.Password))
                {
                    return BadRequest("Password is required.");
                }

                bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, link.passwordHash);

                if (!isValid)
                {
                    return Unauthorized("Invalid password.");
                }

                return Ok(new { verified = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying share link password for token {Token}.", token);
                return StatusCode(500, "Error verifying password.");
            }
        }


        /// <summary>
        /// Downloads the file associated with a share link.
        /// This endpoint is unauthenticated — validates via token + optional password.
        /// </summary>
        [Route("api/Share/{token}/Download")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadSharedFile(Guid token, [FromQuery] string password = null)
        {
            try
            {
                DocumentShareLink link = await _db.DocumentShareLinks
                    .Include(l => l.document)
                    .FirstOrDefaultAsync(l => l.token == token && l.active && !l.deleted);

                if (link == null)
                {
                    return NotFound("Share link not found or has been revoked.");
                }

                // Check expiry
                if (link.expiresAt.HasValue && link.expiresAt.Value < DateTime.UtcNow)
                {
                    return BadRequest("This share link has expired.");
                }

                // Check download limit
                if (link.maxDownloads.HasValue && link.downloadCount >= link.maxDownloads.Value)
                {
                    return BadRequest("This share link has reached its download limit.");
                }

                // Check password
                if (!string.IsNullOrEmpty(link.passwordHash))
                {
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        return Unauthorized("Password is required.");
                    }

                    if (!BCrypt.Net.BCrypt.Verify(password, link.passwordHash))
                    {
                        return Unauthorized("Invalid password.");
                    }
                }

                // Retrieve file data
                Document document = await _fileStorage.GetDocumentByIdAsync(link.documentId, link.tenantGuid);

                if (document == null || document.fileDataData == null || document.fileDataData.Length == 0)
                {
                    return NotFound("Document content not available.");
                }

                // Increment download count
                link.downloadCount++;
                await _db.SaveChangesAsync();

                var stream = new MemoryStream(document.fileDataData);
                return File(stream, document.mimeType ?? "application/octet-stream", document.fileName, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading shared file for token {Token}.", token);
                return StatusCode(500, "Error downloading shared file.");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  EMAIL ENDPOINTS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Emails a document to a recipient.
        /// Small files (≤10MB) are sent as attachments; large files get a share link instead.
        /// </summary>
        [Route("api/FileManager/Documents/{documentId}/Email")]
        [HttpPost]
        public async Task<IActionResult> EmailDocument(int documentId, [FromBody] EmailDocumentRequest request)
        {
            if (!await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();
                Guid tenantGuid = await UserTenantGuidAsync(securityUser);

                if (string.IsNullOrWhiteSpace(request?.ToEmail))
                {
                    return BadRequest("Recipient email is required.");
                }

                Document document = await _fileStorage.GetDocumentByIdAsync(documentId, tenantGuid);
                if (document == null)
                {
                    return NotFound("Document not found.");
                }

                string senderEmail = securityUser.emailAddress;
                string senderName = $"{securityUser.firstName} {securityUser.lastName}".Trim();
                if (string.IsNullOrEmpty(senderName)) senderName = securityUser.accountName;

                string subject = request.Subject ?? $"Document: {document.name}";
                string userMessage = request.Message ?? "";

                const long MAX_ATTACHMENT_BYTES = 10L * 1024 * 1024; // 10MB

                bool sendAsLink = request.SendAsLink == true
                    || (document.fileDataData != null && document.fileDataData.Length > MAX_ATTACHMENT_BYTES);

                bool success;

                if (sendAsLink)
                {
                    // Auto-create a share link (7-day expiry) and embed in email
                    var shareLink = new DocumentShareLink
                    {
                        tenantGuid = tenantGuid,
                        documentId = documentId,
                        token = Guid.NewGuid(),
                        expiresAt = DateTime.UtcNow.AddDays(7),
                        downloadCount = 0,
                        createdBy = senderEmail ?? senderName,
                        createdDate = DateTime.UtcNow,
                        versionNumber = 1,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    _db.DocumentShareLinks.Add(shareLink);
                    await _db.SaveChangesAsync();

                    // Build download URL (relative — the email template will need the full host)
                    string baseUrl = $"{Request.Scheme}://{Request.Host}";
                    string downloadUrl = $"{baseUrl}/api/Share/{shareLink.token}/Download";

                    string htmlBody = BuildEmailHtml(document.name, userMessage, downloadUrl, senderName);

                    success = await Foundation.Services.SendGridEmailService.SendEmailAsync(
                        senderEmail, senderName, request.ToEmail, subject, htmlBody,
                        bodyIsHtml: true, includeSignature: false);
                }
                else
                {
                    // Send as attachment
                    string base64Data = document.fileDataData != null
                        ? Convert.ToBase64String(document.fileDataData)
                        : "";

                    string htmlBody = BuildEmailHtml(document.name, userMessage, null, senderName);

                    success = await Foundation.Services.SendGridEmailService.SendEmailWithAttachmentAsync(
                        senderEmail, senderName, request.ToEmail, subject, htmlBody,
                        document.fileName, base64Data, document.mimeType ?? "application/octet-stream",
                        bodyIsHtml: true, includeSignature: false);
                }

                if (!success)
                {
                    return StatusCode(500, "Failed to send email.");
                }

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, $"Emailed document '{document.name}' to {request.ToEmail}");

                return Ok(new { sent = true, sentAsLink = sendAsLink });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error emailing document {DocumentId}.", documentId);
                return StatusCode(500, "Error sending email.");
            }
        }


        /// <summary>
        /// Builds a clean HTML email body for document sharing.
        /// </summary>
        private static string BuildEmailHtml(string documentName, string userMessage, string downloadUrl, string senderName)
        {
            string messageSection = string.IsNullOrWhiteSpace(userMessage)
                ? ""
                : $"<p style='font-family: Arial, sans-serif; font-size: 14px;'>{System.Net.WebUtility.HtmlEncode(userMessage)}</p>";

            string linkSection = string.IsNullOrEmpty(downloadUrl)
                ? "<p style='font-family: Arial, sans-serif; font-size: 14px;'>The document is attached to this email.</p>"
                : $@"<p style='font-family: Arial, sans-serif; font-size: 14px;'>
                    You can download the document using the link below:
                  </p>
                  <p style='margin: 16px 0;'>
                    <a href='{downloadUrl}' style='background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; font-family: Arial, sans-serif; font-size: 14px;'>
                      Download {System.Net.WebUtility.HtmlEncode(documentName)}
                    </a>
                  </p>
                  <p style='font-family: Arial, sans-serif; font-size: 12px; color: #666;'>
                    This link expires in 7 days.
                  </p>";

            return $@"<div style='font-family: Arial, sans-serif; font-size: 14px;'>
                <p><strong>{System.Net.WebUtility.HtmlEncode(senderName)}</strong> shared a document with you:</p>
                <p style='font-size: 16px; font-weight: bold;'>{System.Net.WebUtility.HtmlEncode(documentName)}</p>
                {messageSection}
                {linkSection}
            </div>";
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  REQUEST / RESPONSE DTOs
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>DTO for creating a share link.</summary>
        public class CreateShareLinkRequest
        {
            public string Password { get; set; }
            public DateTime? ExpiresAt { get; set; }
            public int? MaxDownloads { get; set; }
        }

        /// <summary>DTO for verifying a share link password.</summary>
        public class VerifyPasswordRequest
        {
            public string Password { get; set; }
        }

        /// <summary>DTO for emailing a document.</summary>
        public class EmailDocumentRequest
        {
            public string ToEmail { get; set; }
            public string Subject { get; set; }
            public string Message { get; set; }
            public bool? SendAsLink { get; set; }
        }
    }
}
