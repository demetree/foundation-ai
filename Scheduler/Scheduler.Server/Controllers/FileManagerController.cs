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
using Microsoft.Extensions.Logging;
using Scheduler.Server.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        private readonly SchedulerContext _db;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<FileManagerController> _logger;

        public FileManagerController(
            SchedulerContext db,
            IFileStorageService fileStorage,
            ILogger<FileManagerController> logger) : base("Scheduler", "FileManager")
        {
            _db = db;
            _fileStorage = fileStorage;
            _logger = logger;
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
                // Use ToDTO() instead of ToOutputDTO() to keep listings lightweight
                // Binary data is already excluded by the service layer
                //
                return Ok(Document.ToDTOList(documents));
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

                List<Document.DocumentDTO> uploadedResults = new List<Document.DocumentDTO>();

                foreach (IFormFile file in files)
                {
                    byte[] fileBytes;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
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

                return Ok(uploadedResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading documents.");
                return StatusCode(500, "Error uploading documents.");
            }
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

                return File(document.fileDataData, document.mimeType ?? "application/octet-stream", document.fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document {DocumentId}.", documentId);
                return StatusCode(500, "Error downloading document.");
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

                return Ok(Document.ToDTOList(results));
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
