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

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the SharedInstruction entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SharedInstruction entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SharedInstructionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object sharedInstructionPutSyncRoot = new object();
		static object sharedInstructionDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<SharedInstructionsController> _logger;

		public SharedInstructionsController(BMCContext context, ILogger<SharedInstructionsController> logger) : base("BMC", "SharedInstruction")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SharedInstructions filtered by the parameters provided.
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
		[Route("api/SharedInstructions")]
		public async Task<IActionResult> GetSharedInstructions(
			int? buildManualId = null,
			int? publishedMocId = null,
			string name = null,
			string description = null,
			string formatType = null,
			string filePath = null,
			bool? isPublished = null,
			DateTime? publishedDate = null,
			int? downloadCount = null,
			int? pageCount = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (publishedDate.HasValue == true && publishedDate.Value.Kind != DateTimeKind.Utc)
			{
				publishedDate = publishedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.SharedInstruction> query = (from si in _context.SharedInstructions select si);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildManualId.HasValue == true)
			{
				query = query.Where(si => si.buildManualId == buildManualId.Value);
			}
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(si => si.publishedMocId == publishedMocId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(si => si.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(si => si.description == description);
			}
			if (string.IsNullOrEmpty(formatType) == false)
			{
				query = query.Where(si => si.formatType == formatType);
			}
			if (string.IsNullOrEmpty(filePath) == false)
			{
				query = query.Where(si => si.filePath == filePath);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(si => si.isPublished == isPublished.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(si => si.publishedDate == publishedDate.Value);
			}
			if (downloadCount.HasValue == true)
			{
				query = query.Where(si => si.downloadCount == downloadCount.Value);
			}
			if (pageCount.HasValue == true)
			{
				query = query.Where(si => si.pageCount == pageCount.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(si => si.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(si => si.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(si => si.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(si => si.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(si => si.deleted == false);
				}
			}
			else
			{
				query = query.Where(si => si.active == true);
				query = query.Where(si => si.deleted == false);
			}

			query = query.OrderBy(si => si.name).ThenBy(si => si.formatType).ThenBy(si => si.filePath);


			//
			// Add the any string contains parameter to span all the string fields on the Shared Instruction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.formatType.Contains(anyStringContains)
			       || x.filePath.Contains(anyStringContains)
			       || (includeRelations == true && x.buildManual.name.Contains(anyStringContains))
			       || (includeRelations == true && x.buildManual.description.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.name.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.description.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.thumbnailImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.tags.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.visibility.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.licenseName.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.readmeMarkdown.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.slug.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.defaultBranchName.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.buildManual);
				query = query.Include(x => x.publishedMoc);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.SharedInstruction> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SharedInstruction sharedInstruction in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(sharedInstruction, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.SharedInstruction Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.SharedInstruction Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SharedInstructions filtered by the parameters provided.  Its query is similar to the GetSharedInstructions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SharedInstructions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? buildManualId = null,
			int? publishedMocId = null,
			string name = null,
			string description = null,
			string formatType = null,
			string filePath = null,
			bool? isPublished = null,
			DateTime? publishedDate = null,
			int? downloadCount = null,
			int? pageCount = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (publishedDate.HasValue == true && publishedDate.Value.Kind != DateTimeKind.Utc)
			{
				publishedDate = publishedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.SharedInstruction> query = (from si in _context.SharedInstructions select si);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (buildManualId.HasValue == true)
			{
				query = query.Where(si => si.buildManualId == buildManualId.Value);
			}
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(si => si.publishedMocId == publishedMocId.Value);
			}
			if (name != null)
			{
				query = query.Where(si => si.name == name);
			}
			if (description != null)
			{
				query = query.Where(si => si.description == description);
			}
			if (formatType != null)
			{
				query = query.Where(si => si.formatType == formatType);
			}
			if (filePath != null)
			{
				query = query.Where(si => si.filePath == filePath);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(si => si.isPublished == isPublished.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(si => si.publishedDate == publishedDate.Value);
			}
			if (downloadCount.HasValue == true)
			{
				query = query.Where(si => si.downloadCount == downloadCount.Value);
			}
			if (pageCount.HasValue == true)
			{
				query = query.Where(si => si.pageCount == pageCount.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(si => si.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(si => si.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(si => si.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(si => si.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(si => si.deleted == false);
				}
			}
			else
			{
				query = query.Where(si => si.active == true);
				query = query.Where(si => si.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Shared Instruction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.formatType.Contains(anyStringContains)
			       || x.filePath.Contains(anyStringContains)
			       || x.buildManual.name.Contains(anyStringContains)
			       || x.buildManual.description.Contains(anyStringContains)
			       || x.publishedMoc.name.Contains(anyStringContains)
			       || x.publishedMoc.description.Contains(anyStringContains)
			       || x.publishedMoc.thumbnailImagePath.Contains(anyStringContains)
			       || x.publishedMoc.tags.Contains(anyStringContains)
			       || x.publishedMoc.visibility.Contains(anyStringContains)
			       || x.publishedMoc.licenseName.Contains(anyStringContains)
			       || x.publishedMoc.readmeMarkdown.Contains(anyStringContains)
			       || x.publishedMoc.slug.Contains(anyStringContains)
			       || x.publishedMoc.defaultBranchName.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SharedInstruction by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SharedInstruction/{id}")]
		public async Task<IActionResult> GetSharedInstruction(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SharedInstruction> query = (from si in _context.SharedInstructions where
							(si.id == id) &&
							(userIsAdmin == true || si.deleted == false) &&
							(userIsWriter == true || si.active == true)
					select si);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.buildManual);
					query = query.Include(x => x.publishedMoc);
					query = query.AsSplitQuery();
				}

				Database.SharedInstruction materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.SharedInstruction Entity was read with Admin privilege." : "BMC.SharedInstruction Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SharedInstruction", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.SharedInstruction entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.SharedInstruction.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.SharedInstruction.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SharedInstruction record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SharedInstruction/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSharedInstruction(int id, [FromBody]Database.SharedInstruction.SharedInstructionDTO sharedInstructionDTO, CancellationToken cancellationToken = default)
		{
			if (sharedInstructionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != sharedInstructionDTO.id)
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


			IQueryable<Database.SharedInstruction> query = (from x in _context.SharedInstructions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SharedInstruction existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.SharedInstruction PUT", id.ToString(), new Exception("No BMC.SharedInstruction entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (sharedInstructionDTO.objectGuid == Guid.Empty)
            {
                sharedInstructionDTO.objectGuid = existing.objectGuid;
            }
            else if (sharedInstructionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SharedInstruction record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SharedInstruction cloneOfExisting = (Database.SharedInstruction)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SharedInstruction object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SharedInstruction sharedInstruction = (Database.SharedInstruction)_context.Entry(existing).GetDatabaseValues().ToObject();
			sharedInstruction.ApplyDTO(sharedInstructionDTO);
			//
			// The tenant guid for any SharedInstruction being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the SharedInstruction because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				sharedInstruction.tenantGuid = existing.tenantGuid;
			}

			lock (sharedInstructionPutSyncRoot)
			{
				//
				// Validate the version number for the sharedInstruction being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != sharedInstruction.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "SharedInstruction save attempt was made but save request was with version " + sharedInstruction.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The SharedInstruction you are trying to update has already changed.  Please try your save again after reloading the SharedInstruction.");
				}
				else
				{
					// Same record.  Increase version.
					sharedInstruction.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (sharedInstruction.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.SharedInstruction record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (sharedInstruction.name != null && sharedInstruction.name.Length > 100)
				{
					sharedInstruction.name = sharedInstruction.name.Substring(0, 100);
				}

				if (sharedInstruction.formatType != null && sharedInstruction.formatType.Length > 50)
				{
					sharedInstruction.formatType = sharedInstruction.formatType.Substring(0, 50);
				}

				if (sharedInstruction.filePath != null && sharedInstruction.filePath.Length > 250)
				{
					sharedInstruction.filePath = sharedInstruction.filePath.Substring(0, 250);
				}

				if (sharedInstruction.publishedDate.HasValue == true && sharedInstruction.publishedDate.Value.Kind != DateTimeKind.Utc)
				{
					sharedInstruction.publishedDate = sharedInstruction.publishedDate.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.SharedInstruction> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(sharedInstruction);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SharedInstructionChangeHistory sharedInstructionChangeHistory = new SharedInstructionChangeHistory();
				        sharedInstructionChangeHistory.sharedInstructionId = sharedInstruction.id;
				        sharedInstructionChangeHistory.versionNumber = sharedInstruction.versionNumber;
				        sharedInstructionChangeHistory.timeStamp = DateTime.UtcNow;
				        sharedInstructionChangeHistory.userId = securityUser.id;
				        sharedInstructionChangeHistory.tenantGuid = userTenantGuid;
				        sharedInstructionChangeHistory.data = JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction));
				        _context.SharedInstructionChangeHistories.Add(sharedInstructionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.SharedInstruction entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction)),
						null);

				return Ok(Database.SharedInstruction.CreateAnonymous(sharedInstruction));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.SharedInstruction entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new SharedInstruction record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SharedInstruction", Name = "SharedInstruction")]
		public async Task<IActionResult> PostSharedInstruction([FromBody]Database.SharedInstruction.SharedInstructionDTO sharedInstructionDTO, CancellationToken cancellationToken = default)
		{
			if (sharedInstructionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new SharedInstruction object using the data from the DTO
			//
			Database.SharedInstruction sharedInstruction = Database.SharedInstruction.FromDTO(sharedInstructionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				sharedInstruction.tenantGuid = userTenantGuid;

				if (sharedInstruction.name != null && sharedInstruction.name.Length > 100)
				{
					sharedInstruction.name = sharedInstruction.name.Substring(0, 100);
				}

				if (sharedInstruction.formatType != null && sharedInstruction.formatType.Length > 50)
				{
					sharedInstruction.formatType = sharedInstruction.formatType.Substring(0, 50);
				}

				if (sharedInstruction.filePath != null && sharedInstruction.filePath.Length > 250)
				{
					sharedInstruction.filePath = sharedInstruction.filePath.Substring(0, 250);
				}

				if (sharedInstruction.publishedDate.HasValue == true && sharedInstruction.publishedDate.Value.Kind != DateTimeKind.Utc)
				{
					sharedInstruction.publishedDate = sharedInstruction.publishedDate.Value.ToUniversalTime();
				}

				sharedInstruction.objectGuid = Guid.NewGuid();
				sharedInstruction.versionNumber = 1;

				_context.SharedInstructions.Add(sharedInstruction);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the sharedInstruction object so that no further changes will be written to the database
				    //
				    _context.Entry(sharedInstruction).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					sharedInstruction.SharedInstructionChangeHistories = null;
					sharedInstruction.buildManual = null;
					sharedInstruction.publishedMoc = null;


				    SharedInstructionChangeHistory sharedInstructionChangeHistory = new SharedInstructionChangeHistory();
				    sharedInstructionChangeHistory.sharedInstructionId = sharedInstruction.id;
				    sharedInstructionChangeHistory.versionNumber = sharedInstruction.versionNumber;
				    sharedInstructionChangeHistory.timeStamp = DateTime.UtcNow;
				    sharedInstructionChangeHistory.userId = securityUser.id;
				    sharedInstructionChangeHistory.tenantGuid = userTenantGuid;
				    sharedInstructionChangeHistory.data = JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction));
				    _context.SharedInstructionChangeHistories.Add(sharedInstructionChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.SharedInstruction entity successfully created.",
						true,
						sharedInstruction. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.SharedInstruction entity creation failed.", false, sharedInstruction.id.ToString(), "", JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SharedInstruction", sharedInstruction.id, sharedInstruction.name));

			return CreatedAtRoute("SharedInstruction", new { id = sharedInstruction.id }, Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction));
		}



        /// <summary>
        /// 
        /// This rolls a SharedInstruction entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SharedInstruction/Rollback/{id}")]
		[Route("api/SharedInstruction/Rollback")]
		public async Task<IActionResult> RollbackToSharedInstructionVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.SharedInstruction> query = (from x in _context.SharedInstructions
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this SharedInstruction concurrently
			//
			lock (sharedInstructionPutSyncRoot)
			{
				
				Database.SharedInstruction sharedInstruction = query.FirstOrDefault();
				
				if (sharedInstruction == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.SharedInstruction rollback", id.ToString(), new Exception("No BMC.SharedInstruction entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the SharedInstruction current state so we can log it.
				//
				Database.SharedInstruction cloneOfExisting = (Database.SharedInstruction)_context.Entry(sharedInstruction).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.SharedInstructionChangeHistories = null;
				cloneOfExisting.buildManual = null;
				cloneOfExisting.publishedMoc = null;

				if (versionNumber >= sharedInstruction.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.SharedInstruction rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.SharedInstruction rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				SharedInstructionChangeHistory sharedInstructionChangeHistory = (from x in _context.SharedInstructionChangeHistories
				                                               where
				                                               x.sharedInstructionId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (sharedInstructionChangeHistory != null)
				{
				    Database.SharedInstruction oldSharedInstruction = JsonSerializer.Deserialize<Database.SharedInstruction>(sharedInstructionChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    sharedInstruction.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    sharedInstruction.buildManualId = oldSharedInstruction.buildManualId;
				    sharedInstruction.publishedMocId = oldSharedInstruction.publishedMocId;
				    sharedInstruction.name = oldSharedInstruction.name;
				    sharedInstruction.description = oldSharedInstruction.description;
				    sharedInstruction.formatType = oldSharedInstruction.formatType;
				    sharedInstruction.filePath = oldSharedInstruction.filePath;
				    sharedInstruction.isPublished = oldSharedInstruction.isPublished;
				    sharedInstruction.publishedDate = oldSharedInstruction.publishedDate;
				    sharedInstruction.downloadCount = oldSharedInstruction.downloadCount;
				    sharedInstruction.pageCount = oldSharedInstruction.pageCount;
				    sharedInstruction.objectGuid = oldSharedInstruction.objectGuid;
				    sharedInstruction.active = oldSharedInstruction.active;
				    sharedInstruction.deleted = oldSharedInstruction.deleted;

				    string serializedSharedInstruction = JsonSerializer.Serialize(sharedInstruction);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SharedInstructionChangeHistory newSharedInstructionChangeHistory = new SharedInstructionChangeHistory();
				        newSharedInstructionChangeHistory.sharedInstructionId = sharedInstruction.id;
				        newSharedInstructionChangeHistory.versionNumber = sharedInstruction.versionNumber;
				        newSharedInstructionChangeHistory.timeStamp = DateTime.UtcNow;
				        newSharedInstructionChangeHistory.userId = securityUser.id;
				        newSharedInstructionChangeHistory.tenantGuid = userTenantGuid;
				        newSharedInstructionChangeHistory.data = JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction));
				        _context.SharedInstructionChangeHistories.Add(newSharedInstructionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.SharedInstruction rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction)),
						null);


				    return Ok(Database.SharedInstruction.CreateAnonymous(sharedInstruction));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.SharedInstruction rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.SharedInstruction rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a SharedInstruction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SharedInstruction</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SharedInstruction/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetSharedInstructionChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.SharedInstruction sharedInstruction = await _context.SharedInstructions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (sharedInstruction == null)
			{
				return NotFound();
			}

			try
			{
				sharedInstruction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SharedInstruction> versionInfo = await sharedInstruction.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a SharedInstruction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SharedInstruction</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SharedInstruction/{id}/AuditHistory")]
		public async Task<IActionResult> GetSharedInstructionAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.SharedInstruction sharedInstruction = await _context.SharedInstructions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (sharedInstruction == null)
			{
				return NotFound();
			}

			try
			{
				sharedInstruction.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.SharedInstruction>> versions = await sharedInstruction.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a SharedInstruction.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SharedInstruction</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The SharedInstruction object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SharedInstruction/{id}/Version/{version}")]
		public async Task<IActionResult> GetSharedInstructionVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.SharedInstruction sharedInstruction = await _context.SharedInstructions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (sharedInstruction == null)
			{
				return NotFound();
			}

			try
			{
				sharedInstruction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SharedInstruction> versionInfo = await sharedInstruction.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a SharedInstruction at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SharedInstruction</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The SharedInstruction object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SharedInstruction/{id}/StateAtTime")]
		public async Task<IActionResult> GetSharedInstructionStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.SharedInstruction sharedInstruction = await _context.SharedInstructions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (sharedInstruction == null)
			{
				return NotFound();
			}

			try
			{
				sharedInstruction.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SharedInstruction> versionInfo = await sharedInstruction.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a SharedInstruction record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SharedInstruction/{id}")]
		[Route("api/SharedInstruction")]
		public async Task<IActionResult> DeleteSharedInstruction(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.SharedInstruction> query = (from x in _context.SharedInstructions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SharedInstruction sharedInstruction = await query.FirstOrDefaultAsync(cancellationToken);

			if (sharedInstruction == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.SharedInstruction DELETE", id.ToString(), new Exception("No BMC.SharedInstruction entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SharedInstruction cloneOfExisting = (Database.SharedInstruction)_context.Entry(sharedInstruction).GetDatabaseValues().ToObject();


			lock (sharedInstructionDeleteSyncRoot)
			{
			    try
			    {
			        sharedInstruction.deleted = true;
			        sharedInstruction.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        SharedInstructionChangeHistory sharedInstructionChangeHistory = new SharedInstructionChangeHistory();
			        sharedInstructionChangeHistory.sharedInstructionId = sharedInstruction.id;
			        sharedInstructionChangeHistory.versionNumber = sharedInstruction.versionNumber;
			        sharedInstructionChangeHistory.timeStamp = DateTime.UtcNow;
			        sharedInstructionChangeHistory.userId = securityUser.id;
			        sharedInstructionChangeHistory.tenantGuid = userTenantGuid;
			        sharedInstructionChangeHistory.data = JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction));
			        _context.SharedInstructionChangeHistories.Add(sharedInstructionChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.SharedInstruction entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.SharedInstruction entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SharedInstruction.CreateAnonymousWithFirstLevelSubObjects(sharedInstruction)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of SharedInstruction records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SharedInstructions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? buildManualId = null,
			int? publishedMocId = null,
			string name = null,
			string description = null,
			string formatType = null,
			string filePath = null,
			bool? isPublished = null,
			DateTime? publishedDate = null,
			int? downloadCount = null,
			int? pageCount = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (publishedDate.HasValue == true && publishedDate.Value.Kind != DateTimeKind.Utc)
			{
				publishedDate = publishedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.SharedInstruction> query = (from si in _context.SharedInstructions select si);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildManualId.HasValue == true)
			{
				query = query.Where(si => si.buildManualId == buildManualId.Value);
			}
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(si => si.publishedMocId == publishedMocId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(si => si.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(si => si.description == description);
			}
			if (string.IsNullOrEmpty(formatType) == false)
			{
				query = query.Where(si => si.formatType == formatType);
			}
			if (string.IsNullOrEmpty(filePath) == false)
			{
				query = query.Where(si => si.filePath == filePath);
			}
			if (isPublished.HasValue == true)
			{
				query = query.Where(si => si.isPublished == isPublished.Value);
			}
			if (publishedDate.HasValue == true)
			{
				query = query.Where(si => si.publishedDate == publishedDate.Value);
			}
			if (downloadCount.HasValue == true)
			{
				query = query.Where(si => si.downloadCount == downloadCount.Value);
			}
			if (pageCount.HasValue == true)
			{
				query = query.Where(si => si.pageCount == pageCount.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(si => si.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(si => si.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(si => si.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(si => si.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(si => si.deleted == false);
				}
			}
			else
			{
				query = query.Where(si => si.active == true);
				query = query.Where(si => si.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Shared Instruction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.formatType.Contains(anyStringContains)
			       || x.filePath.Contains(anyStringContains)
			       || x.buildManual.name.Contains(anyStringContains)
			       || x.buildManual.description.Contains(anyStringContains)
			       || x.publishedMoc.name.Contains(anyStringContains)
			       || x.publishedMoc.description.Contains(anyStringContains)
			       || x.publishedMoc.thumbnailImagePath.Contains(anyStringContains)
			       || x.publishedMoc.tags.Contains(anyStringContains)
			       || x.publishedMoc.visibility.Contains(anyStringContains)
			       || x.publishedMoc.licenseName.Contains(anyStringContains)
			       || x.publishedMoc.readmeMarkdown.Contains(anyStringContains)
			       || x.publishedMoc.slug.Contains(anyStringContains)
			       || x.publishedMoc.defaultBranchName.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.formatType).ThenBy(x => x.filePath);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SharedInstruction.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SharedInstruction/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
