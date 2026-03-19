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

        public FileManagerController(
            SchedulerContext db,
            IFileStorageService fileStorage,
            ILogger<FileManagerController> logger,
            IHubContext<FileManagerHub, IFileManagerHubClient> hub,
            ChunkBufferService chunkBuffer) : base("Scheduler", "FileManager")
        {
            _db = db;
            _fileStorage = fileStorage;
            _logger = logger;
            _hub = hub;
            _chunkBuffer = chunkBuffer;
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

                List<DocumentFolder> folders = await _fileStorage.GetFoldersAsync(tenantGuid);

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

                DocumentFolder folder = await _fileStorage.GetFolderByIdAsync(folderId, tenantGuid);

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

                List<Document> documents = await _fileStorage.GetDocumentsInFolderAsync(folderId, tenantGuid);

                //
                // Use ToOutputDTOList() to include entity link nav properties
                // so the file manager can display linked entity names.
                // Binary data is already excluded by the service layer.
                //
                return Ok(Document.ToOutputDTOList(documents));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching documents in folder {FolderId}.", folderId);
                return StatusCode(500, "Error fetching documents.");
            }
        }


        /// <summary>
        /// Uploads one or more documents.  Accepts multipart/form-data with one or more files.
        /// Optional query parameters: folderId, documentTypeId, and entity link IDs.
        /// </summary>
        [Route("api/FileManager/Documents/Upload")]
        [HttpPost]
        [RequestSizeLimit(Constants.ONE_GIGABYTE_IN_BYTES)]
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

                // Broadcast real-time update
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

                _chunkBuffer.CleanupSession(sessionId);

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                    $"Uploaded document '{fileName}' via chunked upload ({fileBytes.Length} bytes)",
                    securityUser.accountName);

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
                List<Document> versions = await _fileStorage.GetDocumentVersionsAsync(documentId, tenantGuid);

                return Ok(versions.Select(d => new {
                    d.id, d.name, d.fileName, d.mimeType, d.fileSizeBytes,
                    d.uploadedDate, d.uploadedBy, d.versionNumber, d.objectGuid
                }));
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

                // Get existing document to copy objectGuid and versionNumber
                Document existing = await _fileStorage.GetDocumentByIdAsync(documentId, tenantGuid);
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

                Document newVersion = new Document
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
                    documentFolderId = existing.documentFolderId,
                    documentTypeId = existing.documentTypeId,
                    uploadedBy = securityUser.accountName,
                    objectGuid = existing.objectGuid != Guid.Empty ? existing.objectGuid : Guid.NewGuid(),
                    versionNumber = existing.versionNumber + 1,

                    // Preserve entity links
                    scheduledEventId = existing.scheduledEventId,
                    contactId = existing.contactId,
                    clientId = existing.clientId,
                    resourceId = existing.resourceId,
                    crewId = existing.crewId,
                    schedulingTargetId = existing.schedulingTargetId,
                    financialTransactionId = existing.financialTransactionId,
                    officeId = existing.officeId,
                    campaignId = existing.campaignId,
                    householdId = existing.householdId,
                    constituentId = existing.constituentId,
                    tributeId = existing.tributeId,
                    volunteerProfileId = existing.volunteerProfileId
                };

                Document saved = await _fileStorage.UploadDocumentAsync(newVersion, securityUser.id);

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                    $"Uploaded new version (v{saved.versionNumber}) for document {documentId}",
                    securityUser.accountName);
                await BroadcastDocumentChangedAsync(tenantGuid, new { action = "newVersion", documentId, versionNumber = saved.versionNumber });

                return Ok(saved.ToDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading new version for document {DocumentId}.", documentId);
                return StatusCode(500, "Error uploading new version.");
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

                var activity = await _db.DocumentChangeHistories
                    .Where(h => h.tenantGuid == tenantGuid)
                    .OrderByDescending(h => h.timeStamp)
                    .Take(Math.Min(count, 100))
                    .Select(h => new
                    {
                        h.id,
                        h.documentId,
                        documentName = h.document != null ? h.document.name : null,
                        h.versionNumber,
                        h.timeStamp,
                        h.userId,
                        h.data
                    })
                    .ToListAsync();

                // Resolve user IDs to display names via Foundation's ChangeHistory multi-tenant user resolution.
                // This uses the same pattern as DocumentsController.GetDocumentAuditHistory — the Foundation
                // ChangeHistoryMultiTenant class joins SecurityTenantUsers → SecurityUser with a built-in cache.
                var userIds = activity.Select(a => a.userId).Distinct().ToList();
                var userNameMap = new Dictionary<int, string>();
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
                    a.documentName,
                    a.versionNumber,
                    a.timeStamp,
                    a.userId,
                    userName = userNameMap.GetValueOrDefault(a.userId, $"User #{a.userId}"),
                    a.data
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

                List<DocumentTag> tags = await _fileStorage.GetTagsAsync(tenantGuid);

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
    }
}
