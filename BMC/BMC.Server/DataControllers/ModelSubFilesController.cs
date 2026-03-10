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

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the ModelSubFile entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ModelSubFile entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ModelSubFilesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<ModelSubFilesController> _logger;

		public ModelSubFilesController(BMCContext context, ILogger<ModelSubFilesController> logger) : base("BMC", "ModelSubFile")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ModelSubFiles filtered by the parameters provided.
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
		[Route("api/ModelSubFiles")]
		public async Task<IActionResult> GetModelSubFiles(
			int? modelDocumentId = null,
			string fileName = null,
			bool? isMainModel = null,
			int? parentModelSubFileId = null,
			int? sequence = null,
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

			IQueryable<Database.ModelSubFile> query = (from msf in _context.ModelSubFiles select msf);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (modelDocumentId.HasValue == true)
			{
				query = query.Where(msf => msf.modelDocumentId == modelDocumentId.Value);
			}
			if (string.IsNullOrEmpty(fileName) == false)
			{
				query = query.Where(msf => msf.fileName == fileName);
			}
			if (isMainModel.HasValue == true)
			{
				query = query.Where(msf => msf.isMainModel == isMainModel.Value);
			}
			if (parentModelSubFileId.HasValue == true)
			{
				query = query.Where(msf => msf.parentModelSubFileId == parentModelSubFileId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(msf => msf.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(msf => msf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(msf => msf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(msf => msf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(msf => msf.deleted == false);
				}
			}
			else
			{
				query = query.Where(msf => msf.active == true);
				query = query.Where(msf => msf.deleted == false);
			}

			query = query.OrderBy(msf => msf.sequence).ThenBy(msf => msf.fileName);


			//
			// Add the any string contains parameter to span all the string fields on the Model Sub File, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.fileName.Contains(anyStringContains)
			       || (includeRelations == true && x.modelDocument.name.Contains(anyStringContains))
			       || (includeRelations == true && x.modelDocument.description.Contains(anyStringContains))
			       || (includeRelations == true && x.modelDocument.sourceFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.modelDocument.sourceFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.modelDocument.sourceFileFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.modelDocument.sourceFileMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.modelDocument.author.Contains(anyStringContains))
			       || (includeRelations == true && x.modelDocument.studioVersion.Contains(anyStringContains))
			       || (includeRelations == true && x.modelDocument.instructionSettingsXml.Contains(anyStringContains))
			       || (includeRelations == true && x.modelDocument.errorPartList.Contains(anyStringContains))
			       || (includeRelations == true && x.parentModelSubFile.fileName.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.modelDocument);
				query = query.Include(x => x.parentModelSubFile);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ModelSubFile> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ModelSubFile modelSubFile in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(modelSubFile, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ModelSubFile Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ModelSubFile Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ModelSubFiles filtered by the parameters provided.  Its query is similar to the GetModelSubFiles method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelSubFiles/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? modelDocumentId = null,
			string fileName = null,
			bool? isMainModel = null,
			int? parentModelSubFileId = null,
			int? sequence = null,
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


			IQueryable<Database.ModelSubFile> query = (from msf in _context.ModelSubFiles select msf);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (modelDocumentId.HasValue == true)
			{
				query = query.Where(msf => msf.modelDocumentId == modelDocumentId.Value);
			}
			if (fileName != null)
			{
				query = query.Where(msf => msf.fileName == fileName);
			}
			if (isMainModel.HasValue == true)
			{
				query = query.Where(msf => msf.isMainModel == isMainModel.Value);
			}
			if (parentModelSubFileId.HasValue == true)
			{
				query = query.Where(msf => msf.parentModelSubFileId == parentModelSubFileId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(msf => msf.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(msf => msf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(msf => msf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(msf => msf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(msf => msf.deleted == false);
				}
			}
			else
			{
				query = query.Where(msf => msf.active == true);
				query = query.Where(msf => msf.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Model Sub File, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.fileName.Contains(anyStringContains)
			       || x.modelDocument.name.Contains(anyStringContains)
			       || x.modelDocument.description.Contains(anyStringContains)
			       || x.modelDocument.sourceFormat.Contains(anyStringContains)
			       || x.modelDocument.sourceFileName.Contains(anyStringContains)
			       || x.modelDocument.sourceFileFileName.Contains(anyStringContains)
			       || x.modelDocument.sourceFileMimeType.Contains(anyStringContains)
			       || x.modelDocument.author.Contains(anyStringContains)
			       || x.modelDocument.studioVersion.Contains(anyStringContains)
			       || x.modelDocument.instructionSettingsXml.Contains(anyStringContains)
			       || x.modelDocument.errorPartList.Contains(anyStringContains)
			       || x.parentModelSubFile.fileName.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ModelSubFile by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelSubFile/{id}")]
		public async Task<IActionResult> GetModelSubFile(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ModelSubFile> query = (from msf in _context.ModelSubFiles where
							(msf.id == id) &&
							(userIsAdmin == true || msf.deleted == false) &&
							(userIsWriter == true || msf.active == true)
					select msf);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.modelDocument);
					query = query.Include(x => x.parentModelSubFile);
					query = query.AsSplitQuery();
				}

				Database.ModelSubFile materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ModelSubFile Entity was read with Admin privilege." : "BMC.ModelSubFile Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModelSubFile", materialized.id, materialized.fileName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ModelSubFile entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ModelSubFile.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ModelSubFile.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ModelSubFile record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ModelSubFile/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutModelSubFile(int id, [FromBody]Database.ModelSubFile.ModelSubFileDTO modelSubFileDTO, CancellationToken cancellationToken = default)
		{
			if (modelSubFileDTO == null)
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



			if (id != modelSubFileDTO.id)
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


			IQueryable<Database.ModelSubFile> query = (from x in _context.ModelSubFiles
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ModelSubFile existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModelSubFile PUT", id.ToString(), new Exception("No BMC.ModelSubFile entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (modelSubFileDTO.objectGuid == Guid.Empty)
            {
                modelSubFileDTO.objectGuid = existing.objectGuid;
            }
            else if (modelSubFileDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ModelSubFile record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ModelSubFile cloneOfExisting = (Database.ModelSubFile)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ModelSubFile object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ModelSubFile modelSubFile = (Database.ModelSubFile)_context.Entry(existing).GetDatabaseValues().ToObject();
			modelSubFile.ApplyDTO(modelSubFileDTO);
			//
			// The tenant guid for any ModelSubFile being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ModelSubFile because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				modelSubFile.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (modelSubFile.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ModelSubFile record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (modelSubFile.fileName != null && modelSubFile.fileName.Length > 250)
			{
				modelSubFile.fileName = modelSubFile.fileName.Substring(0, 250);
			}

			EntityEntry<Database.ModelSubFile> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(modelSubFile);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ModelSubFile entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelSubFile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelSubFile.CreateAnonymousWithFirstLevelSubObjects(modelSubFile)),
					null);


				return Ok(Database.ModelSubFile.CreateAnonymous(modelSubFile));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ModelSubFile entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelSubFile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelSubFile.CreateAnonymousWithFirstLevelSubObjects(modelSubFile)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ModelSubFile record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelSubFile", Name = "ModelSubFile")]
		public async Task<IActionResult> PostModelSubFile([FromBody]Database.ModelSubFile.ModelSubFileDTO modelSubFileDTO, CancellationToken cancellationToken = default)
		{
			if (modelSubFileDTO == null)
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
			// Create a new ModelSubFile object using the data from the DTO
			//
			Database.ModelSubFile modelSubFile = Database.ModelSubFile.FromDTO(modelSubFileDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				modelSubFile.tenantGuid = userTenantGuid;

				if (modelSubFile.fileName != null && modelSubFile.fileName.Length > 250)
				{
					modelSubFile.fileName = modelSubFile.fileName.Substring(0, 250);
				}

				modelSubFile.objectGuid = Guid.NewGuid();
				_context.ModelSubFiles.Add(modelSubFile);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ModelSubFile entity successfully created.",
					true,
					modelSubFile.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ModelSubFile.CreateAnonymousWithFirstLevelSubObjects(modelSubFile)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ModelSubFile entity creation failed.", false, modelSubFile.id.ToString(), "", JsonSerializer.Serialize(modelSubFile), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModelSubFile", modelSubFile.id, modelSubFile.fileName));

			return CreatedAtRoute("ModelSubFile", new { id = modelSubFile.id }, Database.ModelSubFile.CreateAnonymousWithFirstLevelSubObjects(modelSubFile));
		}



        /// <summary>
        /// 
        /// This deletes a ModelSubFile record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelSubFile/{id}")]
		[Route("api/ModelSubFile")]
		public async Task<IActionResult> DeleteModelSubFile(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ModelSubFile> query = (from x in _context.ModelSubFiles
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ModelSubFile modelSubFile = await query.FirstOrDefaultAsync(cancellationToken);

			if (modelSubFile == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModelSubFile DELETE", id.ToString(), new Exception("No BMC.ModelSubFile entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ModelSubFile cloneOfExisting = (Database.ModelSubFile)_context.Entry(modelSubFile).GetDatabaseValues().ToObject();


			try
			{
				modelSubFile.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ModelSubFile entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelSubFile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelSubFile.CreateAnonymousWithFirstLevelSubObjects(modelSubFile)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ModelSubFile entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelSubFile.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelSubFile.CreateAnonymousWithFirstLevelSubObjects(modelSubFile)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ModelSubFile records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ModelSubFiles/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? modelDocumentId = null,
			string fileName = null,
			bool? isMainModel = null,
			int? parentModelSubFileId = null,
			int? sequence = null,
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

			IQueryable<Database.ModelSubFile> query = (from msf in _context.ModelSubFiles select msf);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (modelDocumentId.HasValue == true)
			{
				query = query.Where(msf => msf.modelDocumentId == modelDocumentId.Value);
			}
			if (string.IsNullOrEmpty(fileName) == false)
			{
				query = query.Where(msf => msf.fileName == fileName);
			}
			if (isMainModel.HasValue == true)
			{
				query = query.Where(msf => msf.isMainModel == isMainModel.Value);
			}
			if (parentModelSubFileId.HasValue == true)
			{
				query = query.Where(msf => msf.parentModelSubFileId == parentModelSubFileId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(msf => msf.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(msf => msf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(msf => msf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(msf => msf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(msf => msf.deleted == false);
				}
			}
			else
			{
				query = query.Where(msf => msf.active == true);
				query = query.Where(msf => msf.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Model Sub File, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.fileName.Contains(anyStringContains)
			       || x.modelDocument.name.Contains(anyStringContains)
			       || x.modelDocument.description.Contains(anyStringContains)
			       || x.modelDocument.sourceFormat.Contains(anyStringContains)
			       || x.modelDocument.sourceFileName.Contains(anyStringContains)
			       || x.modelDocument.sourceFileFileName.Contains(anyStringContains)
			       || x.modelDocument.sourceFileMimeType.Contains(anyStringContains)
			       || x.modelDocument.author.Contains(anyStringContains)
			       || x.modelDocument.studioVersion.Contains(anyStringContains)
			       || x.modelDocument.instructionSettingsXml.Contains(anyStringContains)
			       || x.modelDocument.errorPartList.Contains(anyStringContains)
			       || x.parentModelSubFile.fileName.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.fileName);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ModelSubFile.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ModelSubFile/CreateAuditEvent")]
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


	}
}
