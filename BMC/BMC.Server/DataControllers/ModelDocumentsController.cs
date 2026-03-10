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
using Foundation.BMC.Database;
using Foundation.ChangeHistory;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the ModelDocument entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ModelDocument entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ModelDocumentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object modelDocumentPutSyncRoot = new object();
		static object modelDocumentDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<ModelDocumentsController> _logger;

		public ModelDocumentsController(BMCContext context, ILogger<ModelDocumentsController> logger) : base("BMC", "ModelDocument")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ModelDocuments filtered by the parameters provided.
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
		[Route("api/ModelDocuments")]
		public async Task<IActionResult> GetModelDocuments(
			int? projectId = null,
			string name = null,
			string description = null,
			string sourceFormat = null,
			string sourceFileName = null,
			string sourceFileFileName = null,
			long? sourceFileSize = null,
			string sourceFileMimeType = null,
			string author = null,
			int? totalPartCount = null,
			int? totalStepCount = null,
			string studioVersion = null,
			string instructionSettingsXml = null,
			string errorPartList = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

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

			IQueryable<Database.ModelDocument> query = (from md in _context.ModelDocuments select md);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(md => md.projectId == projectId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(md => md.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(md => md.description == description);
			}
			if (string.IsNullOrEmpty(sourceFormat) == false)
			{
				query = query.Where(md => md.sourceFormat == sourceFormat);
			}
			if (string.IsNullOrEmpty(sourceFileName) == false)
			{
				query = query.Where(md => md.sourceFileName == sourceFileName);
			}
			if (string.IsNullOrEmpty(sourceFileFileName) == false)
			{
				query = query.Where(md => md.sourceFileFileName == sourceFileFileName);
			}
			if (sourceFileSize.HasValue == true)
			{
				query = query.Where(md => md.sourceFileSize == sourceFileSize.Value);
			}
			if (string.IsNullOrEmpty(sourceFileMimeType) == false)
			{
				query = query.Where(md => md.sourceFileMimeType == sourceFileMimeType);
			}
			if (string.IsNullOrEmpty(author) == false)
			{
				query = query.Where(md => md.author == author);
			}
			if (totalPartCount.HasValue == true)
			{
				query = query.Where(md => md.totalPartCount == totalPartCount.Value);
			}
			if (totalStepCount.HasValue == true)
			{
				query = query.Where(md => md.totalStepCount == totalStepCount.Value);
			}
			if (string.IsNullOrEmpty(studioVersion) == false)
			{
				query = query.Where(md => md.studioVersion == studioVersion);
			}
			if (string.IsNullOrEmpty(instructionSettingsXml) == false)
			{
				query = query.Where(md => md.instructionSettingsXml == instructionSettingsXml);
			}
			if (string.IsNullOrEmpty(errorPartList) == false)
			{
				query = query.Where(md => md.errorPartList == errorPartList);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(md => md.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(md => md.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(md => md.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(md => md.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(md => md.deleted == false);
				}
			}
			else
			{
				query = query.Where(md => md.active == true);
				query = query.Where(md => md.deleted == false);
			}

			query = query.OrderBy(md => md.name).ThenBy(md => md.sourceFormat).ThenBy(md => md.sourceFileName);


			//
			// Add the any string contains parameter to span all the string fields on the Model Document, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.sourceFormat.Contains(anyStringContains)
			       || x.sourceFileName.Contains(anyStringContains)
			       || x.sourceFileFileName.Contains(anyStringContains)
			       || x.sourceFileMimeType.Contains(anyStringContains)
			       || x.author.Contains(anyStringContains)
			       || x.studioVersion.Contains(anyStringContains)
			       || x.instructionSettingsXml.Contains(anyStringContains)
			       || x.errorPartList.Contains(anyStringContains)
			       || (includeRelations == true && x.project.name.Contains(anyStringContains))
			       || (includeRelations == true && x.project.description.Contains(anyStringContains))
			       || (includeRelations == true && x.project.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.project.thumbnailImagePath.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.project);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ModelDocument> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ModelDocument modelDocument in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(modelDocument, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async modelDocument =>
				{

					if (modelDocument.sourceFileData == null &&
					    modelDocument.sourceFileSize.HasValue == true &&
					    modelDocument.sourceFileSize.Value > 0)
					{
					    modelDocument.sourceFileData = await LoadDataFromDiskAsync(modelDocument.objectGuid, modelDocument.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ModelDocument Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ModelDocument Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ModelDocuments filtered by the parameters provided.  Its query is similar to the GetModelDocuments method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelDocuments/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? projectId = null,
			string name = null,
			string description = null,
			string sourceFormat = null,
			string sourceFileName = null,
			string sourceFileFileName = null,
			long? sourceFileSize = null,
			string sourceFileMimeType = null,
			string author = null,
			int? totalPartCount = null,
			int? totalStepCount = null,
			string studioVersion = null,
			string instructionSettingsXml = null,
			string errorPartList = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			IQueryable<Database.ModelDocument> query = (from md in _context.ModelDocuments select md);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (projectId.HasValue == true)
			{
				query = query.Where(md => md.projectId == projectId.Value);
			}
			if (name != null)
			{
				query = query.Where(md => md.name == name);
			}
			if (description != null)
			{
				query = query.Where(md => md.description == description);
			}
			if (sourceFormat != null)
			{
				query = query.Where(md => md.sourceFormat == sourceFormat);
			}
			if (sourceFileName != null)
			{
				query = query.Where(md => md.sourceFileName == sourceFileName);
			}
			if (sourceFileFileName != null)
			{
				query = query.Where(md => md.sourceFileFileName == sourceFileFileName);
			}
			if (sourceFileSize.HasValue == true)
			{
				query = query.Where(md => md.sourceFileSize == sourceFileSize.Value);
			}
			if (sourceFileMimeType != null)
			{
				query = query.Where(md => md.sourceFileMimeType == sourceFileMimeType);
			}
			if (author != null)
			{
				query = query.Where(md => md.author == author);
			}
			if (totalPartCount.HasValue == true)
			{
				query = query.Where(md => md.totalPartCount == totalPartCount.Value);
			}
			if (totalStepCount.HasValue == true)
			{
				query = query.Where(md => md.totalStepCount == totalStepCount.Value);
			}
			if (studioVersion != null)
			{
				query = query.Where(md => md.studioVersion == studioVersion);
			}
			if (instructionSettingsXml != null)
			{
				query = query.Where(md => md.instructionSettingsXml == instructionSettingsXml);
			}
			if (errorPartList != null)
			{
				query = query.Where(md => md.errorPartList == errorPartList);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(md => md.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(md => md.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(md => md.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(md => md.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(md => md.deleted == false);
				}
			}
			else
			{
				query = query.Where(md => md.active == true);
				query = query.Where(md => md.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Model Document, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.sourceFormat.Contains(anyStringContains)
			       || x.sourceFileName.Contains(anyStringContains)
			       || x.sourceFileFileName.Contains(anyStringContains)
			       || x.sourceFileMimeType.Contains(anyStringContains)
			       || x.author.Contains(anyStringContains)
			       || x.studioVersion.Contains(anyStringContains)
			       || x.instructionSettingsXml.Contains(anyStringContains)
			       || x.errorPartList.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ModelDocument by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelDocument/{id}")]
		public async Task<IActionResult> GetModelDocument(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			try
			{
				IQueryable<Database.ModelDocument> query = (from md in _context.ModelDocuments where
							(md.id == id) &&
							(userIsAdmin == true || md.deleted == false) &&
							(userIsWriter == true || md.active == true)
					select md);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.project);
					query = query.AsSplitQuery();
				}

				Database.ModelDocument materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.sourceFileData == null &&
					    materialized.sourceFileSize.HasValue == true &&
					    materialized.sourceFileSize.Value > 0)
					{
					    materialized.sourceFileData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ModelDocument Entity was read with Admin privilege." : "BMC.ModelDocument Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModelDocument", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ModelDocument entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ModelDocument.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ModelDocument.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ModelDocument record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ModelDocument/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutModelDocument(int id, [FromBody]Database.ModelDocument.ModelDocumentDTO modelDocumentDTO, CancellationToken cancellationToken = default)
		{
			if (modelDocumentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != modelDocumentDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			IQueryable<Database.ModelDocument> query = (from x in _context.ModelDocuments
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ModelDocument existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModelDocument PUT", id.ToString(), new Exception("No BMC.ModelDocument entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (modelDocumentDTO.objectGuid == Guid.Empty)
            {
                modelDocumentDTO.objectGuid = existing.objectGuid;
            }
            else if (modelDocumentDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ModelDocument record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ModelDocument cloneOfExisting = (Database.ModelDocument)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ModelDocument object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ModelDocument modelDocument = (Database.ModelDocument)_context.Entry(existing).GetDatabaseValues().ToObject();
			modelDocument.ApplyDTO(modelDocumentDTO);
			//
			// The tenant guid for any ModelDocument being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ModelDocument because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				modelDocument.tenantGuid = existing.tenantGuid;
			}

			lock (modelDocumentPutSyncRoot)
			{
				//
				// Validate the version number for the modelDocument being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != modelDocument.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ModelDocument save attempt was made but save request was with version " + modelDocument.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ModelDocument you are trying to update has already changed.  Please try your save again after reloading the ModelDocument.");
				}
				else
				{
					// Same record.  Increase version.
					modelDocument.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (modelDocument.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ModelDocument record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (modelDocument.name != null && modelDocument.name.Length > 250)
				{
					modelDocument.name = modelDocument.name.Substring(0, 250);
				}

				if (modelDocument.sourceFormat != null && modelDocument.sourceFormat.Length > 50)
				{
					modelDocument.sourceFormat = modelDocument.sourceFormat.Substring(0, 50);
				}

				if (modelDocument.sourceFileName != null && modelDocument.sourceFileName.Length > 250)
				{
					modelDocument.sourceFileName = modelDocument.sourceFileName.Substring(0, 250);
				}

				if (modelDocument.sourceFileFileName != null && modelDocument.sourceFileFileName.Length > 250)
				{
					modelDocument.sourceFileFileName = modelDocument.sourceFileFileName.Substring(0, 250);
				}

				if (modelDocument.sourceFileMimeType != null && modelDocument.sourceFileMimeType.Length > 100)
				{
					modelDocument.sourceFileMimeType = modelDocument.sourceFileMimeType.Substring(0, 100);
				}

				if (modelDocument.author != null && modelDocument.author.Length > 100)
				{
					modelDocument.author = modelDocument.author.Substring(0, 100);
				}

				if (modelDocument.studioVersion != null && modelDocument.studioVersion.Length > 100)
				{
					modelDocument.studioVersion = modelDocument.studioVersion.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (modelDocument.sourceFileData != null && string.IsNullOrEmpty(modelDocument.sourceFileFileName))
				{
				    modelDocument.sourceFileFileName = modelDocument.objectGuid.ToString() + ".data";
				}

				if (modelDocument.sourceFileData != null && (modelDocument.sourceFileSize.HasValue == false || modelDocument.sourceFileSize != modelDocument.sourceFileData.Length))
				{
				    modelDocument.sourceFileSize = modelDocument.sourceFileData.Length;
				}

				if (modelDocument.sourceFileData != null && string.IsNullOrEmpty(modelDocument.sourceFileMimeType))
				{
				    modelDocument.sourceFileMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = modelDocument.sourceFileData;

					if (diskBasedBinaryStorageMode == true &&
					    modelDocument.sourceFileFileName != null &&
					    modelDocument.sourceFileData != null &&
					    modelDocument.sourceFileSize.HasValue == true &&
					    modelDocument.sourceFileSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(modelDocument.objectGuid, modelDocument.versionNumber, modelDocument.sourceFileData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    modelDocument.sourceFileData = null;

					}

				    EntityEntry<Database.ModelDocument> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(modelDocument);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ModelDocumentChangeHistory modelDocumentChangeHistory = new ModelDocumentChangeHistory();
				        modelDocumentChangeHistory.modelDocumentId = modelDocument.id;
				        modelDocumentChangeHistory.versionNumber = modelDocument.versionNumber;
				        modelDocumentChangeHistory.timeStamp = DateTime.UtcNow;
				        modelDocumentChangeHistory.userId = securityUser.id;
				        modelDocumentChangeHistory.tenantGuid = userTenantGuid;
				        modelDocumentChangeHistory.data = JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument));
				        _context.ModelDocumentChangeHistories.Add(modelDocumentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    modelDocument.sourceFileData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.ModelDocument entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument)),
						null);

				return Ok(Database.ModelDocument.CreateAnonymous(modelDocument));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.ModelDocument entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ModelDocument record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelDocument", Name = "ModelDocument")]
		public async Task<IActionResult> PostModelDocument([FromBody]Database.ModelDocument.ModelDocumentDTO modelDocumentDTO, CancellationToken cancellationToken = default)
		{
			if (modelDocumentDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			//
			// Create a new ModelDocument object using the data from the DTO
			//
			Database.ModelDocument modelDocument = Database.ModelDocument.FromDTO(modelDocumentDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				modelDocument.tenantGuid = userTenantGuid;

				if (modelDocument.name != null && modelDocument.name.Length > 250)
				{
					modelDocument.name = modelDocument.name.Substring(0, 250);
				}

				if (modelDocument.sourceFormat != null && modelDocument.sourceFormat.Length > 50)
				{
					modelDocument.sourceFormat = modelDocument.sourceFormat.Substring(0, 50);
				}

				if (modelDocument.sourceFileName != null && modelDocument.sourceFileName.Length > 250)
				{
					modelDocument.sourceFileName = modelDocument.sourceFileName.Substring(0, 250);
				}

				if (modelDocument.sourceFileFileName != null && modelDocument.sourceFileFileName.Length > 250)
				{
					modelDocument.sourceFileFileName = modelDocument.sourceFileFileName.Substring(0, 250);
				}

				if (modelDocument.sourceFileMimeType != null && modelDocument.sourceFileMimeType.Length > 100)
				{
					modelDocument.sourceFileMimeType = modelDocument.sourceFileMimeType.Substring(0, 100);
				}

				if (modelDocument.author != null && modelDocument.author.Length > 100)
				{
					modelDocument.author = modelDocument.author.Substring(0, 100);
				}

				if (modelDocument.studioVersion != null && modelDocument.studioVersion.Length > 100)
				{
					modelDocument.studioVersion = modelDocument.studioVersion.Substring(0, 100);
				}

				modelDocument.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (modelDocument.sourceFileData != null && string.IsNullOrEmpty(modelDocument.sourceFileFileName))
				{
				    modelDocument.sourceFileFileName = modelDocument.objectGuid.ToString() + ".data";
				}

				if (modelDocument.sourceFileData != null && (modelDocument.sourceFileSize.HasValue == false || modelDocument.sourceFileSize != modelDocument.sourceFileData.Length))
				{
				    modelDocument.sourceFileSize = modelDocument.sourceFileData.Length;
				}

				if (modelDocument.sourceFileData != null && string.IsNullOrEmpty(modelDocument.sourceFileMimeType))
				{
				    modelDocument.sourceFileMimeType = "application/octet-stream";
				}

				modelDocument.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = modelDocument.sourceFileData;

				if (diskBasedBinaryStorageMode == true &&
				    modelDocument.sourceFileData != null &&
				    modelDocument.sourceFileFileName != null &&
				    modelDocument.sourceFileSize.HasValue == true &&
				    modelDocument.sourceFileSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(modelDocument.objectGuid, modelDocument.versionNumber, modelDocument.sourceFileData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    modelDocument.sourceFileData = null;

				}

				_context.ModelDocuments.Add(modelDocument);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the modelDocument object so that no further changes will be written to the database
				    //
				    _context.Entry(modelDocument).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					modelDocument.sourceFileData = null;
					modelDocument.ModelDocumentChangeHistories = null;
					modelDocument.ModelSubFiles = null;
					modelDocument.project = null;


				    ModelDocumentChangeHistory modelDocumentChangeHistory = new ModelDocumentChangeHistory();
				    modelDocumentChangeHistory.modelDocumentId = modelDocument.id;
				    modelDocumentChangeHistory.versionNumber = modelDocument.versionNumber;
				    modelDocumentChangeHistory.timeStamp = DateTime.UtcNow;
				    modelDocumentChangeHistory.userId = securityUser.id;
				    modelDocumentChangeHistory.tenantGuid = userTenantGuid;
				    modelDocumentChangeHistory.data = JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument));
				    _context.ModelDocumentChangeHistories.Add(modelDocumentChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.ModelDocument entity successfully created.",
						true,
						modelDocument. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    modelDocument.sourceFileData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ModelDocument entity creation failed.", false, modelDocument.id.ToString(), "", JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModelDocument", modelDocument.id, modelDocument.name));

			return CreatedAtRoute("ModelDocument", new { id = modelDocument.id }, Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument));
		}



        /// <summary>
        /// 
        /// This rolls a ModelDocument entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelDocument/Rollback/{id}")]
		[Route("api/ModelDocument/Rollback")]
		public async Task<IActionResult> RollbackToModelDocumentVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
			//
			// Data rollback is an admin only function, like Deletes.
			//
			StartAuditEventClock();
			
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			
			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
			
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			

			
			IQueryable <Database.ModelDocument> query = (from x in _context.ModelDocuments
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this ModelDocument concurrently
			//
			lock (modelDocumentPutSyncRoot)
			{
				
				Database.ModelDocument modelDocument = query.FirstOrDefault();
				
				if (modelDocument == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModelDocument rollback", id.ToString(), new Exception("No BMC.ModelDocument entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ModelDocument current state so we can log it.
				//
				Database.ModelDocument cloneOfExisting = (Database.ModelDocument)_context.Entry(modelDocument).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.sourceFileData = null;
				cloneOfExisting.ModelDocumentChangeHistories = null;
				cloneOfExisting.ModelSubFiles = null;
				cloneOfExisting.project = null;

				if (versionNumber >= modelDocument.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.ModelDocument rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.ModelDocument rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ModelDocumentChangeHistory modelDocumentChangeHistory = (from x in _context.ModelDocumentChangeHistories
				                                               where
				                                               x.modelDocumentId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (modelDocumentChangeHistory != null)
				{
				    Database.ModelDocument oldModelDocument = JsonSerializer.Deserialize<Database.ModelDocument>(modelDocumentChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    modelDocument.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    modelDocument.projectId = oldModelDocument.projectId;
				    modelDocument.name = oldModelDocument.name;
				    modelDocument.description = oldModelDocument.description;
				    modelDocument.sourceFormat = oldModelDocument.sourceFormat;
				    modelDocument.sourceFileName = oldModelDocument.sourceFileName;
				    modelDocument.sourceFileFileName = oldModelDocument.sourceFileFileName;
				    modelDocument.sourceFileSize = oldModelDocument.sourceFileSize;
				    modelDocument.sourceFileData = oldModelDocument.sourceFileData;
				    modelDocument.sourceFileMimeType = oldModelDocument.sourceFileMimeType;
				    modelDocument.author = oldModelDocument.author;
				    modelDocument.totalPartCount = oldModelDocument.totalPartCount;
				    modelDocument.totalStepCount = oldModelDocument.totalStepCount;
				    modelDocument.studioVersion = oldModelDocument.studioVersion;
				    modelDocument.instructionSettingsXml = oldModelDocument.instructionSettingsXml;
				    modelDocument.errorPartList = oldModelDocument.errorPartList;
				    modelDocument.objectGuid = oldModelDocument.objectGuid;
				    modelDocument.active = oldModelDocument.active;
				    modelDocument.deleted = oldModelDocument.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldModelDocument.objectGuid, oldModelDocument.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(modelDocument.objectGuid, modelDocument.versionNumber, binaryData, "data");
				    }

				    string serializedModelDocument = JsonSerializer.Serialize(modelDocument);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ModelDocumentChangeHistory newModelDocumentChangeHistory = new ModelDocumentChangeHistory();
				        newModelDocumentChangeHistory.modelDocumentId = modelDocument.id;
				        newModelDocumentChangeHistory.versionNumber = modelDocument.versionNumber;
				        newModelDocumentChangeHistory.timeStamp = DateTime.UtcNow;
				        newModelDocumentChangeHistory.userId = securityUser.id;
				        newModelDocumentChangeHistory.tenantGuid = userTenantGuid;
				        newModelDocumentChangeHistory.data = JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument));
				        _context.ModelDocumentChangeHistories.Add(newModelDocumentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.ModelDocument rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument)),
						null);


				    return Ok(Database.ModelDocument.CreateAnonymous(modelDocument));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.ModelDocument rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.ModelDocument rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ModelDocument.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ModelDocument</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelDocument/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetModelDocumentChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.ModelDocument modelDocument = await _context.ModelDocuments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (modelDocument == null)
			{
				return NotFound();
			}

			try
			{
				modelDocument.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ModelDocument> versionInfo = await modelDocument.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null)
				{
					return NotFound($"Version {versionNumber} not found.");
				}

				return Ok(versionInfo);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the full audit history for a ModelDocument.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ModelDocument</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelDocument/{id}/AuditHistory")]
		public async Task<IActionResult> GetModelDocumentAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.ModelDocument modelDocument = await _context.ModelDocuments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (modelDocument == null)
			{
				return NotFound();
			}

			try
			{
				modelDocument.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ModelDocument>> versions = await modelDocument.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ModelDocument.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ModelDocument</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ModelDocument object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelDocument/{id}/Version/{version}")]
		public async Task<IActionResult> GetModelDocumentVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.ModelDocument modelDocument = await _context.ModelDocuments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (modelDocument == null)
			{
				return NotFound();
			}

			try
			{
				modelDocument.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ModelDocument> versionInfo = await modelDocument.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound();
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the state of a ModelDocument at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ModelDocument</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ModelDocument object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelDocument/{id}/StateAtTime")]
		public async Task<IActionResult> GetModelDocumentStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			Database.ModelDocument modelDocument = await _context.ModelDocuments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (modelDocument == null)
			{
				return NotFound();
			}

			try
			{
				modelDocument.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ModelDocument> versionInfo = await modelDocument.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound("No state found at specified time.");
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}

        /// <summary>
        /// 
        /// This deletes a ModelDocument record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelDocument/{id}")]
		[Route("api/ModelDocument")]
		public async Task<IActionResult> DeleteModelDocument(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			IQueryable<Database.ModelDocument> query = (from x in _context.ModelDocuments
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ModelDocument modelDocument = await query.FirstOrDefaultAsync(cancellationToken);

			if (modelDocument == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModelDocument DELETE", id.ToString(), new Exception("No BMC.ModelDocument entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ModelDocument cloneOfExisting = (Database.ModelDocument)_context.Entry(modelDocument).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (modelDocumentDeleteSyncRoot)
			{
			    try
			    {
			        modelDocument.deleted = true;
			        modelDocument.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(modelDocument.objectGuid, modelDocument.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(modelDocument.objectGuid, modelDocument.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        ModelDocumentChangeHistory modelDocumentChangeHistory = new ModelDocumentChangeHistory();
			        modelDocumentChangeHistory.modelDocumentId = modelDocument.id;
			        modelDocumentChangeHistory.versionNumber = modelDocument.versionNumber;
			        modelDocumentChangeHistory.timeStamp = DateTime.UtcNow;
			        modelDocumentChangeHistory.userId = securityUser.id;
			        modelDocumentChangeHistory.tenantGuid = userTenantGuid;
			        modelDocumentChangeHistory.data = JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument));
			        _context.ModelDocumentChangeHistories.Add(modelDocumentChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.ModelDocument entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.ModelDocument entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ModelDocument records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ModelDocuments/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? projectId = null,
			string name = null,
			string description = null,
			string sourceFormat = null,
			string sourceFileName = null,
			string sourceFileFileName = null,
			long? sourceFileSize = null,
			string sourceFileMimeType = null,
			string author = null,
			int? totalPartCount = null,
			int? totalStepCount = null,
			string studioVersion = null,
			string instructionSettingsXml = null,
			string errorPartList = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


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

			IQueryable<Database.ModelDocument> query = (from md in _context.ModelDocuments select md);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (projectId.HasValue == true)
			{
				query = query.Where(md => md.projectId == projectId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(md => md.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(md => md.description == description);
			}
			if (string.IsNullOrEmpty(sourceFormat) == false)
			{
				query = query.Where(md => md.sourceFormat == sourceFormat);
			}
			if (string.IsNullOrEmpty(sourceFileName) == false)
			{
				query = query.Where(md => md.sourceFileName == sourceFileName);
			}
			if (string.IsNullOrEmpty(sourceFileFileName) == false)
			{
				query = query.Where(md => md.sourceFileFileName == sourceFileFileName);
			}
			if (sourceFileSize.HasValue == true)
			{
				query = query.Where(md => md.sourceFileSize == sourceFileSize.Value);
			}
			if (string.IsNullOrEmpty(sourceFileMimeType) == false)
			{
				query = query.Where(md => md.sourceFileMimeType == sourceFileMimeType);
			}
			if (string.IsNullOrEmpty(author) == false)
			{
				query = query.Where(md => md.author == author);
			}
			if (totalPartCount.HasValue == true)
			{
				query = query.Where(md => md.totalPartCount == totalPartCount.Value);
			}
			if (totalStepCount.HasValue == true)
			{
				query = query.Where(md => md.totalStepCount == totalStepCount.Value);
			}
			if (string.IsNullOrEmpty(studioVersion) == false)
			{
				query = query.Where(md => md.studioVersion == studioVersion);
			}
			if (string.IsNullOrEmpty(instructionSettingsXml) == false)
			{
				query = query.Where(md => md.instructionSettingsXml == instructionSettingsXml);
			}
			if (string.IsNullOrEmpty(errorPartList) == false)
			{
				query = query.Where(md => md.errorPartList == errorPartList);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(md => md.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(md => md.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(md => md.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(md => md.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(md => md.deleted == false);
				}
			}
			else
			{
				query = query.Where(md => md.active == true);
				query = query.Where(md => md.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Model Document, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.sourceFormat.Contains(anyStringContains)
			       || x.sourceFileName.Contains(anyStringContains)
			       || x.sourceFileFileName.Contains(anyStringContains)
			       || x.sourceFileMimeType.Contains(anyStringContains)
			       || x.author.Contains(anyStringContains)
			       || x.studioVersion.Contains(anyStringContains)
			       || x.instructionSettingsXml.Contains(anyStringContains)
			       || x.errorPartList.Contains(anyStringContains)
			       || x.project.name.Contains(anyStringContains)
			       || x.project.description.Contains(anyStringContains)
			       || x.project.notes.Contains(anyStringContains)
			       || x.project.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.sourceFormat).ThenBy(x => x.sourceFileName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ModelDocument.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ModelDocument/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}




        [Route("api/ModelDocument/Data/{id:int}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> UploadData(int id, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			MediaTypeHeaderValue mediaTypeHeader; 

            if (!HttpContext.Request.HasFormContentType ||
				!MediaTypeHeaderValue.TryParse(HttpContext.Request.ContentType, out mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }


            Database.ModelDocument modelDocument = await (from x in _context.ModelDocuments where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (modelDocument == null)
            {
                return NotFound();
            }

            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


            // This will be used to signal whether we are saving data or clearing it.
            bool foundFileData = false;


            //
            // This will get the first file from the request and save it
            //
			try
			{
                MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, HttpContext.Request.Body);
                MultipartSection section = await reader.ReadNextSectionAsync();

                while (section != null)
				{
					ContentDispositionHeaderValue contentDisposition;

					bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);


					if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
						!string.IsNullOrEmpty(contentDisposition.FileName.Value))
					{

						foundFileData = true;
						string fileName = contentDisposition.FileName.ToString().Trim('"');

						// default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.
						MediaTypeHeaderValue mediaType;
						bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);

						string mimeType = "application/octet-stream";
						if (hasMediaTypeHeader && mediaTypeHeader.MediaType != null )
						{
							mimeType = mediaTypeHeader.MediaType.ToString();
						}

						lock (modelDocumentPutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									modelDocument.sourceFileFileName = fileName.Trim();
									modelDocument.sourceFileMimeType = mimeType;
									modelDocument.sourceFileSize = section.Body.Length;

									modelDocument.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 modelDocument.sourceFileFileName != null &&
										 modelDocument.sourceFileSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(modelDocument.objectGuid, modelDocument.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										modelDocument.sourceFileData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											modelDocument.sourceFileData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									ModelDocumentChangeHistory modelDocumentChangeHistory = new ModelDocumentChangeHistory();
									modelDocumentChangeHistory.modelDocumentId = modelDocument.id;
									modelDocumentChangeHistory.versionNumber = modelDocument.versionNumber;
									modelDocumentChangeHistory.timeStamp = DateTime.UtcNow;
									modelDocumentChangeHistory.userId = securityUser.id;
									modelDocumentChangeHistory.tenantGuid = modelDocument.tenantGuid;
									modelDocumentChangeHistory.data = JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument));
									_context.ModelDocumentChangeHistories.Add(modelDocumentChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ModelDocument Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "ModelDocument Data Upload Failed.", false, id.ToString(), "", "", ex);

								return Problem(ex.Message);
							}
						}


						//
						// Stop looking for more files.
						//
						break;
					}

					section = await reader.ReadNextSectionAsync();
				}
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Caught error in UploadData handler", id.ToString(), ex);

                return Problem(ex.Message);
            }

            //
            // Treat the situation where we have a valid ID but no file content as a request to clear the data
            //
            if (foundFileData == false)
            {
                lock (modelDocumentPutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(modelDocument.objectGuid, modelDocument.versionNumber, "data");
                            }

                            modelDocument.sourceFileFileName = null;
                            modelDocument.sourceFileMimeType = null;
                            modelDocument.sourceFileSize = 0;
                            modelDocument.sourceFileData = null;
                            modelDocument.versionNumber++;


                            //
                            // Now add the change history
                            //
                            ModelDocumentChangeHistory modelDocumentChangeHistory = new ModelDocumentChangeHistory();
                            modelDocumentChangeHistory.modelDocumentId = modelDocument.id;
                            modelDocumentChangeHistory.versionNumber = modelDocument.versionNumber;
                            modelDocumentChangeHistory.timeStamp = DateTime.UtcNow;
                            modelDocumentChangeHistory.userId = securityUser.id;
                                    modelDocumentChangeHistory.tenantGuid = modelDocument.tenantGuid;
                                    modelDocumentChangeHistory.data = JsonSerializer.Serialize(Database.ModelDocument.CreateAnonymousWithFirstLevelSubObjects(modelDocument));
                            _context.ModelDocumentChangeHistories.Add(modelDocumentChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ModelDocument data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "ModelDocument data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/ModelDocument/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id, CancellationToken cancellationToken = default)
        {

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			using (BMCContext context = new BMCContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.ModelDocument modelDocument = await (from d in context.ModelDocuments
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (modelDocument != null && modelDocument.sourceFileData != null)
                {
                   return File(modelDocument.sourceFileData.ToArray<byte>(), modelDocument.sourceFileMimeType, modelDocument.sourceFileFileName != null ? modelDocument.sourceFileFileName.Trim() : "ModelDocument_" + modelDocument.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
