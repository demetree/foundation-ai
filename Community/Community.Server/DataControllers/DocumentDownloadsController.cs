using System;
using System.Threading;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Foundation.Security;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security.Database;
using static Foundation.Auditor.AuditEngine;
using Foundation.Community.Database;

namespace Foundation.Community.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the DocumentDownload entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the DocumentDownload entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class DocumentDownloadsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private CommunityContext _context;

		private ILogger<DocumentDownloadsController> _logger;

		public DocumentDownloadsController(CommunityContext context, ILogger<DocumentDownloadsController> logger) : base("Community", "DocumentDownload")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of DocumentDownloads filtered by the parameters provided.
		/// 
		/// There is a filter parameter for every field, and an 'anyStringContains' parameter for cross field string partial searches.
		/// 
		/// Note also the pagination control in the pageSize and pageNumber parameters.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentDownloads")]
		public async Task<IActionResult> GetDocumentDownloads(
			int? Id = null,
			string Title = null,
			string Description = null,
			string FilePath = null,
			string FileName = null,
			string MimeType = null,
			long? FileSizeBytes = null,
			string CategoryName = null,
			DateTime? DocumentDate = null,
			bool? IsPublished = null,
			int? Sequence = null,
			Guid? ObjectGuid = null,
			bool? Active = null,
			bool? Deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			if (pageNumber.HasValue == true &&
			    pageNumber < 1)
			{
			    pageNumber = null;
			}

			if (pageSize.HasValue == true &&
			    pageSize <= 0)
			{
			    pageSize = null;
			}

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (DocumentDate.HasValue == true && DocumentDate.Value.Kind != DateTimeKind.Utc)
			{
				DocumentDate = DocumentDate.Value.ToUniversalTime();
			}

			IQueryable<Database.DocumentDownload> query = (from dd in _context.DocumentDownloads select dd);
			if (Id.HasValue == true)
			{
				query = query.Where(dd => dd.Id == Id.Value);
			}
			if (string.IsNullOrEmpty(Title) == false)
			{
				query = query.Where(dd => dd.Title == Title);
			}
			if (string.IsNullOrEmpty(Description) == false)
			{
				query = query.Where(dd => dd.Description == Description);
			}
			if (string.IsNullOrEmpty(FilePath) == false)
			{
				query = query.Where(dd => dd.FilePath == FilePath);
			}
			if (string.IsNullOrEmpty(FileName) == false)
			{
				query = query.Where(dd => dd.FileName == FileName);
			}
			if (string.IsNullOrEmpty(MimeType) == false)
			{
				query = query.Where(dd => dd.MimeType == MimeType);
			}
			if (FileSizeBytes.HasValue == true)
			{
				query = query.Where(dd => dd.FileSizeBytes == FileSizeBytes.Value);
			}
			if (string.IsNullOrEmpty(CategoryName) == false)
			{
				query = query.Where(dd => dd.CategoryName == CategoryName);
			}
			if (DocumentDate.HasValue == true)
			{
				query = query.Where(dd => dd.DocumentDate == DocumentDate.Value);
			}
			if (IsPublished.HasValue == true)
			{
				query = query.Where(dd => dd.IsPublished == IsPublished.Value);
			}
			if (Sequence.HasValue == true)
			{
				query = query.Where(dd => dd.Sequence == Sequence.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(dd => dd.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(dd => dd.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(dd => dd.Deleted == Deleted.Value);
			}

			query = query.OrderBy(dd => dd.sequence).ThenBy(dd => dd.title).ThenBy(dd => dd.filePath).ThenBy(dd => dd.fileName);


			//
			// Add the any string contains parameter to span all the string fields on the Document Download, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.Title.Contains(anyStringContains)
			       || x.Description.Contains(anyStringContains)
			       || x.FilePath.Contains(anyStringContains)
			       || x.FileName.Contains(anyStringContains)
			       || x.MimeType.Contains(anyStringContains)
			       || x.CategoryName.Contains(anyStringContains)
			   );
			}

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.DocumentDownload> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.DocumentDownload documentDownload in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(documentDownload, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.DocumentDownload Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.DocumentDownload Entity list was read.  Returning " + materialized.Count + " rows of data.");

			// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.
			if (includeRelations == true)
			{
				// Return a DTO with nav properties.
				return Ok((from materializedData in materialized select materializedData.ToOutputDTO()).ToList());
			}
			else
			{
				// Return a DTO without nav properties.
				return Ok((from materializedData in materialized select materializedData.ToDTO()).ToList());
			}
		}
		
		
        /// <summary>
        /// 
        /// This returns a row count of DocumentDownloads filtered by the parameters provided.  Its query is similar to the GetDocumentDownloads method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentDownloads/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? Id = null,
			string Title = null,
			string Description = null,
			string FilePath = null,
			string FileName = null,
			string MimeType = null,
			long? FileSizeBytes = null,
			string CategoryName = null,
			DateTime? DocumentDate = null,
			bool? IsPublished = null,
			int? Sequence = null,
			Guid? ObjectGuid = null,
			bool? Active = null,
			bool? Deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (DocumentDate.HasValue == true && DocumentDate.Value.Kind != DateTimeKind.Utc)
			{
				DocumentDate = DocumentDate.Value.ToUniversalTime();
			}

			IQueryable<Database.DocumentDownload> query = (from dd in _context.DocumentDownloads select dd);
			if (Id.HasValue == true)
			{
				query = query.Where(dd => dd.Id == Id.Value);
			}
			if (Title != null)
			{
				query = query.Where(dd => dd.Title == Title);
			}
			if (Description != null)
			{
				query = query.Where(dd => dd.Description == Description);
			}
			if (FilePath != null)
			{
				query = query.Where(dd => dd.FilePath == FilePath);
			}
			if (FileName != null)
			{
				query = query.Where(dd => dd.FileName == FileName);
			}
			if (MimeType != null)
			{
				query = query.Where(dd => dd.MimeType == MimeType);
			}
			if (FileSizeBytes.HasValue == true)
			{
				query = query.Where(dd => dd.FileSizeBytes == FileSizeBytes.Value);
			}
			if (CategoryName != null)
			{
				query = query.Where(dd => dd.CategoryName == CategoryName);
			}
			if (DocumentDate.HasValue == true)
			{
				query = query.Where(dd => dd.DocumentDate == DocumentDate.Value);
			}
			if (IsPublished.HasValue == true)
			{
				query = query.Where(dd => dd.IsPublished == IsPublished.Value);
			}
			if (Sequence.HasValue == true)
			{
				query = query.Where(dd => dd.Sequence == Sequence.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(dd => dd.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(dd => dd.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(dd => dd.Deleted == Deleted.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Document Download, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.Title.Contains(anyStringContains)
			       || x.Description.Contains(anyStringContains)
			       || x.FilePath.Contains(anyStringContains)
			       || x.FileName.Contains(anyStringContains)
			       || x.MimeType.Contains(anyStringContains)
			       || x.CategoryName.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single DocumentDownload by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentDownload/{id}")]
		public async Task<IActionResult> GetDocumentDownload(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.DocumentDownload> query = (from dd in _context.DocumentDownloads where
				(dd.id == id)
					select dd);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.DocumentDownload materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.DocumentDownload Entity was read with Admin privilege." : "Community.DocumentDownload Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "DocumentDownload", materialized.id, materialized.title));


					// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.
					if (includeRelations == true)
					{
						return Ok(materialized.ToOutputDTO());             // DTO with nav properties
					}
					else
					{
						return Ok(materialized.ToDTO());                   // DTO without nav properties
					}
				}
				else
				{
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.DocumentDownload entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.DocumentDownload.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.DocumentDownload.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing DocumentDownload record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/DocumentDownload/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutDocumentDownload(int id, [FromBody]Database.DocumentDownload.DocumentDownloadDTO documentDownloadDTO, CancellationToken cancellationToken = default)
		{
			if (documentDownloadDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != documentDownloadDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.DocumentDownload> query = (from x in _context.DocumentDownloads
				where
				(x.id == id)
				select x);


			Database.DocumentDownload existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.DocumentDownload PUT", id.ToString(), new Exception("No Community.DocumentDownload entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (documentDownloadDTO.objectGuid == Guid.Empty)
            {
                documentDownloadDTO.objectGuid = existing.objectGuid;
            }
            else if (documentDownloadDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a DocumentDownload record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.DocumentDownload cloneOfExisting = (Database.DocumentDownload)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new DocumentDownload object using the data from the existing record, updated with what is in the DTO.
			//
			Database.DocumentDownload documentDownload = (Database.DocumentDownload)_context.Entry(existing).GetDatabaseValues().ToObject();
			documentDownload.ApplyDTO(documentDownloadDTO);


			if (documentDownload.DocumentDate.HasValue == true && documentDownload.DocumentDate.Value.Kind != DateTimeKind.Utc)
			{
				documentDownload.DocumentDate = documentDownload.DocumentDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.DocumentDownload> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(documentDownload);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.DocumentDownload entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.DocumentDownload.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.DocumentDownload.CreateAnonymousWithFirstLevelSubObjects(documentDownload)),
					null);


				return Ok(Database.DocumentDownload.CreateAnonymous(documentDownload));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.DocumentDownload entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.DocumentDownload.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.DocumentDownload.CreateAnonymousWithFirstLevelSubObjects(documentDownload)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new DocumentDownload record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentDownload", Name = "DocumentDownload")]
		public async Task<IActionResult> PostDocumentDownload([FromBody]Database.DocumentDownload.DocumentDownloadDTO documentDownloadDTO, CancellationToken cancellationToken = default)
		{
			if (documentDownloadDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new DocumentDownload object using the data from the DTO
			//
			Database.DocumentDownload documentDownload = Database.DocumentDownload.FromDTO(documentDownloadDTO);

			try
			{
				if (documentDownload.DocumentDate.HasValue == true && documentDownload.DocumentDate.Value.Kind != DateTimeKind.Utc)
				{
					documentDownload.DocumentDate = documentDownload.DocumentDate.Value.ToUniversalTime();
				}

				_context.DocumentDownloads.Add(documentDownload);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.DocumentDownload entity successfully created.",
					true,
					documentDownload.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.DocumentDownload.CreateAnonymousWithFirstLevelSubObjects(documentDownload)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.DocumentDownload entity creation failed.", false, documentDownload.id.ToString(), "", JsonSerializer.Serialize(documentDownload), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "DocumentDownload", documentDownload.id, documentDownload.title));

			return CreatedAtRoute("DocumentDownload", new { id = documentDownload.id }, Database.DocumentDownload.CreateAnonymousWithFirstLevelSubObjects(documentDownload));
		}



        /// <summary>
        /// 
        /// This deletes a DocumentDownload record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentDownload/{id}")]
		[Route("api/DocumentDownload")]
		public async Task<IActionResult> DeleteDocumentDownload(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.DocumentDownload> query = (from x in _context.DocumentDownloads
				where
				(x.id == id)
				select x);


			Database.DocumentDownload documentDownload = await query.FirstOrDefaultAsync(cancellationToken);

			if (documentDownload == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.DocumentDownload DELETE", id.ToString(), new Exception("No Community.DocumentDownload entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.DocumentDownload cloneOfExisting = (Database.DocumentDownload)_context.Entry(documentDownload).GetDatabaseValues().ToObject();


			try
			{
				_context.DocumentDownloads.Remove(documentDownload);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.DocumentDownload entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.DocumentDownload.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.DocumentDownload.CreateAnonymousWithFirstLevelSubObjects(documentDownload)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.DocumentDownload entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.DocumentDownload.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.DocumentDownload.CreateAnonymousWithFirstLevelSubObjects(documentDownload)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of DocumentDownload records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/DocumentDownloads/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? Id = null,
			string Title = null,
			string Description = null,
			string FilePath = null,
			string FileName = null,
			string MimeType = null,
			long? FileSizeBytes = null,
			string CategoryName = null,
			DateTime? DocumentDate = null,
			bool? IsPublished = null,
			int? Sequence = null,
			Guid? ObjectGuid = null,
			bool? Active = null,
			bool? Deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);


			if (pageNumber.HasValue == true &&
			    pageNumber < 1)
			{
			    pageNumber = null;
			}

			if (pageSize.HasValue == true &&
			    pageSize <= 0)
			{
			    pageSize = null;
			}

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (DocumentDate.HasValue == true && DocumentDate.Value.Kind != DateTimeKind.Utc)
			{
				DocumentDate = DocumentDate.Value.ToUniversalTime();
			}

			IQueryable<Database.DocumentDownload> query = (from dd in _context.DocumentDownloads select dd);
			if (Id.HasValue == true)
			{
				query = query.Where(dd => dd.Id == Id.Value);
			}
			if (string.IsNullOrEmpty(Title) == false)
			{
				query = query.Where(dd => dd.Title == Title);
			}
			if (string.IsNullOrEmpty(Description) == false)
			{
				query = query.Where(dd => dd.Description == Description);
			}
			if (string.IsNullOrEmpty(FilePath) == false)
			{
				query = query.Where(dd => dd.FilePath == FilePath);
			}
			if (string.IsNullOrEmpty(FileName) == false)
			{
				query = query.Where(dd => dd.FileName == FileName);
			}
			if (string.IsNullOrEmpty(MimeType) == false)
			{
				query = query.Where(dd => dd.MimeType == MimeType);
			}
			if (FileSizeBytes.HasValue == true)
			{
				query = query.Where(dd => dd.FileSizeBytes == FileSizeBytes.Value);
			}
			if (string.IsNullOrEmpty(CategoryName) == false)
			{
				query = query.Where(dd => dd.CategoryName == CategoryName);
			}
			if (DocumentDate.HasValue == true)
			{
				query = query.Where(dd => dd.DocumentDate == DocumentDate.Value);
			}
			if (IsPublished.HasValue == true)
			{
				query = query.Where(dd => dd.IsPublished == IsPublished.Value);
			}
			if (Sequence.HasValue == true)
			{
				query = query.Where(dd => dd.Sequence == Sequence.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(dd => dd.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(dd => dd.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(dd => dd.Deleted == Deleted.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Document Download, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.Title.Contains(anyStringContains)
			       || x.Description.Contains(anyStringContains)
			       || x.FilePath.Contains(anyStringContains)
			       || x.FileName.Contains(anyStringContains)
			       || x.MimeType.Contains(anyStringContains)
			       || x.CategoryName.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.title).ThenBy(x => x.filePath).ThenBy(x => x.fileName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.DocumentDownload.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
		}


        /// <summary>
        /// 
        /// This method creates an audit event from within the controller.  It is intended for use by custom logic in client applications that needs to create audit events.
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentDownload/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
