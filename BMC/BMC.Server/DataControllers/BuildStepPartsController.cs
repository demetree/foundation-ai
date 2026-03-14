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
    /// This auto generated class provides the basic CRUD operations for the BuildStepPart entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BuildStepPart entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BuildStepPartsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 20;

		static object buildStepPartPutSyncRoot = new object();
		static object buildStepPartDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<BuildStepPartsController> _logger;

		public BuildStepPartsController(BMCContext context, ILogger<BuildStepPartsController> logger) : base("BMC", "BuildStepPart")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BuildStepParts filtered by the parameters provided.
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
		[Route("api/BuildStepParts")]
		public async Task<IActionResult> GetBuildStepParts(
			int? buildManualStepId = null,
			int? placedBrickId = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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

			IQueryable<Database.BuildStepPart> query = (from bsp in _context.BuildStepParts select bsp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildManualStepId.HasValue == true)
			{
				query = query.Where(bsp => bsp.buildManualStepId == buildManualStepId.Value);
			}
			if (placedBrickId.HasValue == true)
			{
				query = query.Where(bsp => bsp.placedBrickId == placedBrickId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bsp => bsp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsp => bsp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsp => bsp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsp => bsp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsp => bsp.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsp => bsp.active == true);
				query = query.Where(bsp => bsp.deleted == false);
			}

			query = query.OrderBy(bsp => bsp.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.buildManualStep);
				query = query.Include(x => x.placedBrick);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.BuildStepPart> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BuildStepPart buildStepPart in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(buildStepPart, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BuildStepPart Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BuildStepPart Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BuildStepParts filtered by the parameters provided.  Its query is similar to the GetBuildStepParts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepParts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? buildManualStepId = null,
			int? placedBrickId = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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


			IQueryable<Database.BuildStepPart> query = (from bsp in _context.BuildStepParts select bsp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (buildManualStepId.HasValue == true)
			{
				query = query.Where(bsp => bsp.buildManualStepId == buildManualStepId.Value);
			}
			if (placedBrickId.HasValue == true)
			{
				query = query.Where(bsp => bsp.placedBrickId == placedBrickId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bsp => bsp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsp => bsp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsp => bsp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsp => bsp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsp => bsp.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsp => bsp.active == true);
				query = query.Where(bsp => bsp.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BuildStepPart by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepPart/{id}")]
		public async Task<IActionResult> GetBuildStepPart(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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
				IQueryable<Database.BuildStepPart> query = (from bsp in _context.BuildStepParts where
							(bsp.id == id) &&
							(userIsAdmin == true || bsp.deleted == false) &&
							(userIsWriter == true || bsp.active == true)
					select bsp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.buildManualStep);
					query = query.Include(x => x.placedBrick);
					query = query.AsSplitQuery();
				}

				Database.BuildStepPart materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BuildStepPart Entity was read with Admin privilege." : "BMC.BuildStepPart Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildStepPart", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BuildStepPart entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BuildStepPart.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BuildStepPart.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BuildStepPart record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BuildStepPart/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBuildStepPart(int id, [FromBody]Database.BuildStepPart.BuildStepPartDTO buildStepPartDTO, CancellationToken cancellationToken = default)
		{
			if (buildStepPartDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != buildStepPartDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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


			IQueryable<Database.BuildStepPart> query = (from x in _context.BuildStepParts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BuildStepPart existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildStepPart PUT", id.ToString(), new Exception("No BMC.BuildStepPart entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (buildStepPartDTO.objectGuid == Guid.Empty)
            {
                buildStepPartDTO.objectGuid = existing.objectGuid;
            }
            else if (buildStepPartDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BuildStepPart record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BuildStepPart cloneOfExisting = (Database.BuildStepPart)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BuildStepPart object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BuildStepPart buildStepPart = (Database.BuildStepPart)_context.Entry(existing).GetDatabaseValues().ToObject();
			buildStepPart.ApplyDTO(buildStepPartDTO);
			//
			// The tenant guid for any BuildStepPart being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BuildStepPart because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				buildStepPart.tenantGuid = existing.tenantGuid;
			}

			lock (buildStepPartPutSyncRoot)
			{
				//
				// Validate the version number for the buildStepPart being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != buildStepPart.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "BuildStepPart save attempt was made but save request was with version " + buildStepPart.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The BuildStepPart you are trying to update has already changed.  Please try your save again after reloading the BuildStepPart.");
				}
				else
				{
					// Same record.  Increase version.
					buildStepPart.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (buildStepPart.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BuildStepPart record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.BuildStepPart> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(buildStepPart);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BuildStepPartChangeHistory buildStepPartChangeHistory = new BuildStepPartChangeHistory();
				        buildStepPartChangeHistory.buildStepPartId = buildStepPart.id;
				        buildStepPartChangeHistory.versionNumber = buildStepPart.versionNumber;
				        buildStepPartChangeHistory.timeStamp = DateTime.UtcNow;
				        buildStepPartChangeHistory.userId = securityUser.id;
				        buildStepPartChangeHistory.tenantGuid = userTenantGuid;
				        buildStepPartChangeHistory.data = JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart));
				        _context.BuildStepPartChangeHistories.Add(buildStepPartChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BuildStepPart entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart)),
						null);

				return Ok(Database.BuildStepPart.CreateAnonymous(buildStepPart));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BuildStepPart entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new BuildStepPart record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepPart", Name = "BuildStepPart")]
		public async Task<IActionResult> PostBuildStepPart([FromBody]Database.BuildStepPart.BuildStepPartDTO buildStepPartDTO, CancellationToken cancellationToken = default)
		{
			if (buildStepPartDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new BuildStepPart object using the data from the DTO
			//
			Database.BuildStepPart buildStepPart = Database.BuildStepPart.FromDTO(buildStepPartDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				buildStepPart.tenantGuid = userTenantGuid;

				buildStepPart.objectGuid = Guid.NewGuid();
				buildStepPart.versionNumber = 1;

				_context.BuildStepParts.Add(buildStepPart);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the buildStepPart object so that no further changes will be written to the database
				    //
				    _context.Entry(buildStepPart).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					buildStepPart.BuildStepPartChangeHistories = null;
					buildStepPart.buildManualStep = null;
					buildStepPart.placedBrick = null;


				    BuildStepPartChangeHistory buildStepPartChangeHistory = new BuildStepPartChangeHistory();
				    buildStepPartChangeHistory.buildStepPartId = buildStepPart.id;
				    buildStepPartChangeHistory.versionNumber = buildStepPart.versionNumber;
				    buildStepPartChangeHistory.timeStamp = DateTime.UtcNow;
				    buildStepPartChangeHistory.userId = securityUser.id;
				    buildStepPartChangeHistory.tenantGuid = userTenantGuid;
				    buildStepPartChangeHistory.data = JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart));
				    _context.BuildStepPartChangeHistories.Add(buildStepPartChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.BuildStepPart entity successfully created.",
						true,
						buildStepPart. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BuildStepPart entity creation failed.", false, buildStepPart.id.ToString(), "", JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildStepPart", buildStepPart.id, buildStepPart.id.ToString()));

			return CreatedAtRoute("BuildStepPart", new { id = buildStepPart.id }, Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart));
		}



        /// <summary>
        /// 
        /// This rolls a BuildStepPart entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepPart/Rollback/{id}")]
		[Route("api/BuildStepPart/Rollback")]
		public async Task<IActionResult> RollbackToBuildStepPartVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.BuildStepPart> query = (from x in _context.BuildStepParts
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this BuildStepPart concurrently
			//
			lock (buildStepPartPutSyncRoot)
			{
				
				Database.BuildStepPart buildStepPart = query.FirstOrDefault();
				
				if (buildStepPart == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildStepPart rollback", id.ToString(), new Exception("No BMC.BuildStepPart entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the BuildStepPart current state so we can log it.
				//
				Database.BuildStepPart cloneOfExisting = (Database.BuildStepPart)_context.Entry(buildStepPart).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.BuildStepPartChangeHistories = null;
				cloneOfExisting.buildManualStep = null;
				cloneOfExisting.placedBrick = null;

				if (versionNumber >= buildStepPart.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.BuildStepPart rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.BuildStepPart rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				BuildStepPartChangeHistory buildStepPartChangeHistory = (from x in _context.BuildStepPartChangeHistories
				                                               where
				                                               x.buildStepPartId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (buildStepPartChangeHistory != null)
				{
				    Database.BuildStepPart oldBuildStepPart = JsonSerializer.Deserialize<Database.BuildStepPart>(buildStepPartChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    buildStepPart.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    buildStepPart.buildManualStepId = oldBuildStepPart.buildManualStepId;
				    buildStepPart.placedBrickId = oldBuildStepPart.placedBrickId;
				    buildStepPart.objectGuid = oldBuildStepPart.objectGuid;
				    buildStepPart.active = oldBuildStepPart.active;
				    buildStepPart.deleted = oldBuildStepPart.deleted;

				    string serializedBuildStepPart = JsonSerializer.Serialize(buildStepPart);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BuildStepPartChangeHistory newBuildStepPartChangeHistory = new BuildStepPartChangeHistory();
				        newBuildStepPartChangeHistory.buildStepPartId = buildStepPart.id;
				        newBuildStepPartChangeHistory.versionNumber = buildStepPart.versionNumber;
				        newBuildStepPartChangeHistory.timeStamp = DateTime.UtcNow;
				        newBuildStepPartChangeHistory.userId = securityUser.id;
				        newBuildStepPartChangeHistory.tenantGuid = userTenantGuid;
				        newBuildStepPartChangeHistory.data = JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart));
				        _context.BuildStepPartChangeHistories.Add(newBuildStepPartChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BuildStepPart rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart)),
						null);


				    return Ok(Database.BuildStepPart.CreateAnonymous(buildStepPart));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.BuildStepPart rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.BuildStepPart rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a BuildStepPart.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildStepPart</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepPart/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetBuildStepPartChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.BuildStepPart buildStepPart = await _context.BuildStepParts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildStepPart == null)
			{
				return NotFound();
			}

			try
			{
				buildStepPart.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.BuildStepPart> versionInfo = await buildStepPart.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a BuildStepPart.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildStepPart</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepPart/{id}/AuditHistory")]
		public async Task<IActionResult> GetBuildStepPartAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.BuildStepPart buildStepPart = await _context.BuildStepParts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildStepPart == null)
			{
				return NotFound();
			}

			try
			{
				buildStepPart.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.BuildStepPart>> versions = await buildStepPart.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a BuildStepPart.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildStepPart</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The BuildStepPart object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepPart/{id}/Version/{version}")]
		public async Task<IActionResult> GetBuildStepPartVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.BuildStepPart buildStepPart = await _context.BuildStepParts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildStepPart == null)
			{
				return NotFound();
			}

			try
			{
				buildStepPart.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.BuildStepPart> versionInfo = await buildStepPart.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a BuildStepPart at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildStepPart</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The BuildStepPart object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepPart/{id}/StateAtTime")]
		public async Task<IActionResult> GetBuildStepPartStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.BuildStepPart buildStepPart = await _context.BuildStepParts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildStepPart == null)
			{
				return NotFound();
			}

			try
			{
				buildStepPart.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.BuildStepPart> versionInfo = await buildStepPart.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a BuildStepPart record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepPart/{id}")]
		[Route("api/BuildStepPart")]
		public async Task<IActionResult> DeleteBuildStepPart(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.BuildStepPart> query = (from x in _context.BuildStepParts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BuildStepPart buildStepPart = await query.FirstOrDefaultAsync(cancellationToken);

			if (buildStepPart == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildStepPart DELETE", id.ToString(), new Exception("No BMC.BuildStepPart entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BuildStepPart cloneOfExisting = (Database.BuildStepPart)_context.Entry(buildStepPart).GetDatabaseValues().ToObject();


			lock (buildStepPartDeleteSyncRoot)
			{
			    try
			    {
			        buildStepPart.deleted = true;
			        buildStepPart.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        BuildStepPartChangeHistory buildStepPartChangeHistory = new BuildStepPartChangeHistory();
			        buildStepPartChangeHistory.buildStepPartId = buildStepPart.id;
			        buildStepPartChangeHistory.versionNumber = buildStepPart.versionNumber;
			        buildStepPartChangeHistory.timeStamp = DateTime.UtcNow;
			        buildStepPartChangeHistory.userId = securityUser.id;
			        buildStepPartChangeHistory.tenantGuid = userTenantGuid;
			        buildStepPartChangeHistory.data = JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart));
			        _context.BuildStepPartChangeHistories.Add(buildStepPartChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.BuildStepPart entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.BuildStepPart entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildStepPart.CreateAnonymousWithFirstLevelSubObjects(buildStepPart)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of BuildStepPart records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BuildStepParts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? buildManualStepId = null,
			int? placedBrickId = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);


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

			IQueryable<Database.BuildStepPart> query = (from bsp in _context.BuildStepParts select bsp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildManualStepId.HasValue == true)
			{
				query = query.Where(bsp => bsp.buildManualStepId == buildManualStepId.Value);
			}
			if (placedBrickId.HasValue == true)
			{
				query = query.Where(bsp => bsp.placedBrickId == placedBrickId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bsp => bsp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsp => bsp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsp => bsp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsp => bsp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsp => bsp.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsp => bsp.active == true);
				query = query.Where(bsp => bsp.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BuildStepPart.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BuildStepPart/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
