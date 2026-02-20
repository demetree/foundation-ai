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
    /// This auto generated class provides the basic CRUD operations for the BuildManualStep entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BuildManualStep entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BuildManualStepsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 20;

		private BMCContext _context;

		private ILogger<BuildManualStepsController> _logger;

		public BuildManualStepsController(BMCContext context, ILogger<BuildManualStepsController> logger) : base("BMC", "BuildManualStep")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BuildManualSteps filtered by the parameters provided.
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
		[Route("api/BuildManualSteps")]
		public async Task<IActionResult> GetBuildManualSteps(
			int? buildManualPageId = null,
			int? stepNumber = null,
			float? cameraPositionX = null,
			float? cameraPositionY = null,
			float? cameraPositionZ = null,
			float? cameraTargetX = null,
			float? cameraTargetY = null,
			float? cameraTargetZ = null,
			float? cameraZoom = null,
			bool? showExplodedView = null,
			float? explodedDistance = null,
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

			IQueryable<Database.BuildManualStep> query = (from bms in _context.BuildManualSteps select bms);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildManualPageId.HasValue == true)
			{
				query = query.Where(bms => bms.buildManualPageId == buildManualPageId.Value);
			}
			if (stepNumber.HasValue == true)
			{
				query = query.Where(bms => bms.stepNumber == stepNumber.Value);
			}
			if (cameraPositionX.HasValue == true)
			{
				query = query.Where(bms => bms.cameraPositionX == cameraPositionX.Value);
			}
			if (cameraPositionY.HasValue == true)
			{
				query = query.Where(bms => bms.cameraPositionY == cameraPositionY.Value);
			}
			if (cameraPositionZ.HasValue == true)
			{
				query = query.Where(bms => bms.cameraPositionZ == cameraPositionZ.Value);
			}
			if (cameraTargetX.HasValue == true)
			{
				query = query.Where(bms => bms.cameraTargetX == cameraTargetX.Value);
			}
			if (cameraTargetY.HasValue == true)
			{
				query = query.Where(bms => bms.cameraTargetY == cameraTargetY.Value);
			}
			if (cameraTargetZ.HasValue == true)
			{
				query = query.Where(bms => bms.cameraTargetZ == cameraTargetZ.Value);
			}
			if (cameraZoom.HasValue == true)
			{
				query = query.Where(bms => bms.cameraZoom == cameraZoom.Value);
			}
			if (showExplodedView.HasValue == true)
			{
				query = query.Where(bms => bms.showExplodedView == showExplodedView.Value);
			}
			if (explodedDistance.HasValue == true)
			{
				query = query.Where(bms => bms.explodedDistance == explodedDistance.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bms => bms.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bms => bms.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bms => bms.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bms => bms.deleted == false);
				}
			}
			else
			{
				query = query.Where(bms => bms.active == true);
				query = query.Where(bms => bms.deleted == false);
			}

			query = query.OrderBy(bms => bms.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.buildManualPage);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.BuildManualStep> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BuildManualStep buildManualStep in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(buildManualStep, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BuildManualStep Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BuildManualStep Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BuildManualSteps filtered by the parameters provided.  Its query is similar to the GetBuildManualSteps method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualSteps/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? buildManualPageId = null,
			int? stepNumber = null,
			float? cameraPositionX = null,
			float? cameraPositionY = null,
			float? cameraPositionZ = null,
			float? cameraTargetX = null,
			float? cameraTargetY = null,
			float? cameraTargetZ = null,
			float? cameraZoom = null,
			bool? showExplodedView = null,
			float? explodedDistance = null,
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


			IQueryable<Database.BuildManualStep> query = (from bms in _context.BuildManualSteps select bms);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (buildManualPageId.HasValue == true)
			{
				query = query.Where(bms => bms.buildManualPageId == buildManualPageId.Value);
			}
			if (stepNumber.HasValue == true)
			{
				query = query.Where(bms => bms.stepNumber == stepNumber.Value);
			}
			if (cameraPositionX.HasValue == true)
			{
				query = query.Where(bms => bms.cameraPositionX == cameraPositionX.Value);
			}
			if (cameraPositionY.HasValue == true)
			{
				query = query.Where(bms => bms.cameraPositionY == cameraPositionY.Value);
			}
			if (cameraPositionZ.HasValue == true)
			{
				query = query.Where(bms => bms.cameraPositionZ == cameraPositionZ.Value);
			}
			if (cameraTargetX.HasValue == true)
			{
				query = query.Where(bms => bms.cameraTargetX == cameraTargetX.Value);
			}
			if (cameraTargetY.HasValue == true)
			{
				query = query.Where(bms => bms.cameraTargetY == cameraTargetY.Value);
			}
			if (cameraTargetZ.HasValue == true)
			{
				query = query.Where(bms => bms.cameraTargetZ == cameraTargetZ.Value);
			}
			if (cameraZoom.HasValue == true)
			{
				query = query.Where(bms => bms.cameraZoom == cameraZoom.Value);
			}
			if (showExplodedView.HasValue == true)
			{
				query = query.Where(bms => bms.showExplodedView == showExplodedView.Value);
			}
			if (explodedDistance.HasValue == true)
			{
				query = query.Where(bms => bms.explodedDistance == explodedDistance.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bms => bms.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bms => bms.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bms => bms.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bms => bms.deleted == false);
				}
			}
			else
			{
				query = query.Where(bms => bms.active == true);
				query = query.Where(bms => bms.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BuildManualStep by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualStep/{id}")]
		public async Task<IActionResult> GetBuildManualStep(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BuildManualStep> query = (from bms in _context.BuildManualSteps where
							(bms.id == id) &&
							(userIsAdmin == true || bms.deleted == false) &&
							(userIsWriter == true || bms.active == true)
					select bms);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.buildManualPage);
					query = query.AsSplitQuery();
				}

				Database.BuildManualStep materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BuildManualStep Entity was read with Admin privilege." : "BMC.BuildManualStep Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildManualStep", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BuildManualStep entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BuildManualStep.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BuildManualStep.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BuildManualStep record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BuildManualStep/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBuildManualStep(int id, [FromBody]Database.BuildManualStep.BuildManualStepDTO buildManualStepDTO, CancellationToken cancellationToken = default)
		{
			if (buildManualStepDTO == null)
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



			if (id != buildManualStepDTO.id)
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


			IQueryable<Database.BuildManualStep> query = (from x in _context.BuildManualSteps
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BuildManualStep existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildManualStep PUT", id.ToString(), new Exception("No BMC.BuildManualStep entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (buildManualStepDTO.objectGuid == Guid.Empty)
            {
                buildManualStepDTO.objectGuid = existing.objectGuid;
            }
            else if (buildManualStepDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BuildManualStep record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BuildManualStep cloneOfExisting = (Database.BuildManualStep)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BuildManualStep object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BuildManualStep buildManualStep = (Database.BuildManualStep)_context.Entry(existing).GetDatabaseValues().ToObject();
			buildManualStep.ApplyDTO(buildManualStepDTO);
			//
			// The tenant guid for any BuildManualStep being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BuildManualStep because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				buildManualStep.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (buildManualStep.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BuildManualStep record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.BuildManualStep> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(buildManualStep);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BuildManualStep entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep)),
					null);


				return Ok(Database.BuildManualStep.CreateAnonymous(buildManualStep));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BuildManualStep entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BuildManualStep record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualStep", Name = "BuildManualStep")]
		public async Task<IActionResult> PostBuildManualStep([FromBody]Database.BuildManualStep.BuildManualStepDTO buildManualStepDTO, CancellationToken cancellationToken = default)
		{
			if (buildManualStepDTO == null)
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
			// Create a new BuildManualStep object using the data from the DTO
			//
			Database.BuildManualStep buildManualStep = Database.BuildManualStep.FromDTO(buildManualStepDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				buildManualStep.tenantGuid = userTenantGuid;

				buildManualStep.objectGuid = Guid.NewGuid();
				_context.BuildManualSteps.Add(buildManualStep);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BuildManualStep entity successfully created.",
					true,
					buildManualStep.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BuildManualStep entity creation failed.", false, buildManualStep.id.ToString(), "", JsonSerializer.Serialize(buildManualStep), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildManualStep", buildManualStep.id, buildManualStep.id.ToString()));

			return CreatedAtRoute("BuildManualStep", new { id = buildManualStep.id }, Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep));
		}



        /// <summary>
        /// 
        /// This deletes a BuildManualStep record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualStep/{id}")]
		[Route("api/BuildManualStep")]
		public async Task<IActionResult> DeleteBuildManualStep(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BuildManualStep> query = (from x in _context.BuildManualSteps
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BuildManualStep buildManualStep = await query.FirstOrDefaultAsync(cancellationToken);

			if (buildManualStep == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildManualStep DELETE", id.ToString(), new Exception("No BMC.BuildManualStep entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BuildManualStep cloneOfExisting = (Database.BuildManualStep)_context.Entry(buildManualStep).GetDatabaseValues().ToObject();


			try
			{
				buildManualStep.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BuildManualStep entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BuildManualStep entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BuildManualStep records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BuildManualSteps/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? buildManualPageId = null,
			int? stepNumber = null,
			float? cameraPositionX = null,
			float? cameraPositionY = null,
			float? cameraPositionZ = null,
			float? cameraTargetX = null,
			float? cameraTargetY = null,
			float? cameraTargetZ = null,
			float? cameraZoom = null,
			bool? showExplodedView = null,
			float? explodedDistance = null,
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

			IQueryable<Database.BuildManualStep> query = (from bms in _context.BuildManualSteps select bms);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildManualPageId.HasValue == true)
			{
				query = query.Where(bms => bms.buildManualPageId == buildManualPageId.Value);
			}
			if (stepNumber.HasValue == true)
			{
				query = query.Where(bms => bms.stepNumber == stepNumber.Value);
			}
			if (cameraPositionX.HasValue == true)
			{
				query = query.Where(bms => bms.cameraPositionX == cameraPositionX.Value);
			}
			if (cameraPositionY.HasValue == true)
			{
				query = query.Where(bms => bms.cameraPositionY == cameraPositionY.Value);
			}
			if (cameraPositionZ.HasValue == true)
			{
				query = query.Where(bms => bms.cameraPositionZ == cameraPositionZ.Value);
			}
			if (cameraTargetX.HasValue == true)
			{
				query = query.Where(bms => bms.cameraTargetX == cameraTargetX.Value);
			}
			if (cameraTargetY.HasValue == true)
			{
				query = query.Where(bms => bms.cameraTargetY == cameraTargetY.Value);
			}
			if (cameraTargetZ.HasValue == true)
			{
				query = query.Where(bms => bms.cameraTargetZ == cameraTargetZ.Value);
			}
			if (cameraZoom.HasValue == true)
			{
				query = query.Where(bms => bms.cameraZoom == cameraZoom.Value);
			}
			if (showExplodedView.HasValue == true)
			{
				query = query.Where(bms => bms.showExplodedView == showExplodedView.Value);
			}
			if (explodedDistance.HasValue == true)
			{
				query = query.Where(bms => bms.explodedDistance == explodedDistance.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bms => bms.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bms => bms.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bms => bms.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bms => bms.deleted == false);
				}
			}
			else
			{
				query = query.Where(bms => bms.active == true);
				query = query.Where(bms => bms.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BuildManualStep.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BuildManualStep/CreateAuditEvent")]
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
