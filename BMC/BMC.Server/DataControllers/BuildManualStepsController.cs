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

		static object buildManualStepPutSyncRoot = new object();
		static object buildManualStepDeleteSyncRoot = new object();

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
			string renderImagePath = null,
			string pliImagePath = null,
			bool? fadeStepEnabled = null,
			bool? isCallout = null,
			string calloutModelName = null,
			bool? showPartsListImage = null,
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
			if (string.IsNullOrEmpty(renderImagePath) == false)
			{
				query = query.Where(bms => bms.renderImagePath == renderImagePath);
			}
			if (string.IsNullOrEmpty(pliImagePath) == false)
			{
				query = query.Where(bms => bms.pliImagePath == pliImagePath);
			}
			if (fadeStepEnabled.HasValue == true)
			{
				query = query.Where(bms => bms.fadeStepEnabled == fadeStepEnabled.Value);
			}
			if (isCallout.HasValue == true)
			{
				query = query.Where(bms => bms.isCallout == isCallout.Value);
			}
			if (string.IsNullOrEmpty(calloutModelName) == false)
			{
				query = query.Where(bms => bms.calloutModelName == calloutModelName);
			}
			if (showPartsListImage.HasValue == true)
			{
				query = query.Where(bms => bms.showPartsListImage == showPartsListImage.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bms => bms.versionNumber == versionNumber.Value);
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

			query = query.OrderBy(bms => bms.calloutModelName);


			//
			// Add the any string contains parameter to span all the string fields on the Build Manual Step, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.renderImagePath.Contains(anyStringContains)
			       || x.pliImagePath.Contains(anyStringContains)
			       || x.calloutModelName.Contains(anyStringContains)
			       || (includeRelations == true && x.buildManualPage.title.Contains(anyStringContains))
			       || (includeRelations == true && x.buildManualPage.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.buildManualPage.backgroundTheme.Contains(anyStringContains))
			       || (includeRelations == true && x.buildManualPage.layoutPreset.Contains(anyStringContains))
			       || (includeRelations == true && x.buildManualPage.backgroundColorHex.Contains(anyStringContains))
			   );
			}

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
			string renderImagePath = null,
			string pliImagePath = null,
			bool? fadeStepEnabled = null,
			bool? isCallout = null,
			string calloutModelName = null,
			bool? showPartsListImage = null,
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
			if (renderImagePath != null)
			{
				query = query.Where(bms => bms.renderImagePath == renderImagePath);
			}
			if (pliImagePath != null)
			{
				query = query.Where(bms => bms.pliImagePath == pliImagePath);
			}
			if (fadeStepEnabled.HasValue == true)
			{
				query = query.Where(bms => bms.fadeStepEnabled == fadeStepEnabled.Value);
			}
			if (isCallout.HasValue == true)
			{
				query = query.Where(bms => bms.isCallout == isCallout.Value);
			}
			if (calloutModelName != null)
			{
				query = query.Where(bms => bms.calloutModelName == calloutModelName);
			}
			if (showPartsListImage.HasValue == true)
			{
				query = query.Where(bms => bms.showPartsListImage == showPartsListImage.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bms => bms.versionNumber == versionNumber.Value);
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

			//
			// Add the any string contains parameter to span all the string fields on the Build Manual Step, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.renderImagePath.Contains(anyStringContains)
			       || x.pliImagePath.Contains(anyStringContains)
			       || x.calloutModelName.Contains(anyStringContains)
			       || x.buildManualPage.title.Contains(anyStringContains)
			       || x.buildManualPage.notes.Contains(anyStringContains)
			       || x.buildManualPage.backgroundTheme.Contains(anyStringContains)
			       || x.buildManualPage.layoutPreset.Contains(anyStringContains)
			       || x.buildManualPage.backgroundColorHex.Contains(anyStringContains)
			   );
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

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildManualStep", materialized.id, materialized.calloutModelName));


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

			lock (buildManualStepPutSyncRoot)
			{
				//
				// Validate the version number for the buildManualStep being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != buildManualStep.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "BuildManualStep save attempt was made but save request was with version " + buildManualStep.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The BuildManualStep you are trying to update has already changed.  Please try your save again after reloading the BuildManualStep.");
				}
				else
				{
					// Same record.  Increase version.
					buildManualStep.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (buildManualStep.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BuildManualStep record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (buildManualStep.calloutModelName != null && buildManualStep.calloutModelName.Length > 250)
				{
					buildManualStep.calloutModelName = buildManualStep.calloutModelName.Substring(0, 250);
				}

				try
				{
				    EntityEntry<Database.BuildManualStep> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(buildManualStep);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BuildManualStepChangeHistory buildManualStepChangeHistory = new BuildManualStepChangeHistory();
				        buildManualStepChangeHistory.buildManualStepId = buildManualStep.id;
				        buildManualStepChangeHistory.versionNumber = buildManualStep.versionNumber;
				        buildManualStepChangeHistory.timeStamp = DateTime.UtcNow;
				        buildManualStepChangeHistory.userId = securityUser.id;
				        buildManualStepChangeHistory.tenantGuid = userTenantGuid;
				        buildManualStepChangeHistory.data = JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep));
				        _context.BuildManualStepChangeHistories.Add(buildManualStepChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
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

				if (buildManualStep.calloutModelName != null && buildManualStep.calloutModelName.Length > 250)
				{
					buildManualStep.calloutModelName = buildManualStep.calloutModelName.Substring(0, 250);
				}

				buildManualStep.objectGuid = Guid.NewGuid();
				buildManualStep.versionNumber = 1;

				_context.BuildManualSteps.Add(buildManualStep);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the buildManualStep object so that no further changes will be written to the database
				    //
				    _context.Entry(buildManualStep).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					buildManualStep.BuildManualStepChangeHistories = null;
					buildManualStep.BuildStepAnnotations = null;
					buildManualStep.BuildStepParts = null;
					buildManualStep.buildManualPage = null;


				    BuildManualStepChangeHistory buildManualStepChangeHistory = new BuildManualStepChangeHistory();
				    buildManualStepChangeHistory.buildManualStepId = buildManualStep.id;
				    buildManualStepChangeHistory.versionNumber = buildManualStep.versionNumber;
				    buildManualStepChangeHistory.timeStamp = DateTime.UtcNow;
				    buildManualStepChangeHistory.userId = securityUser.id;
				    buildManualStepChangeHistory.tenantGuid = userTenantGuid;
				    buildManualStepChangeHistory.data = JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep));
				    _context.BuildManualStepChangeHistories.Add(buildManualStepChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.BuildManualStep entity successfully created.",
						true,
						buildManualStep. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BuildManualStep entity creation failed.", false, buildManualStep.id.ToString(), "", JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildManualStep", buildManualStep.id, buildManualStep.calloutModelName));

			return CreatedAtRoute("BuildManualStep", new { id = buildManualStep.id }, Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep));
		}



        /// <summary>
        /// 
        /// This rolls a BuildManualStep entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualStep/Rollback/{id}")]
		[Route("api/BuildManualStep/Rollback")]
		public async Task<IActionResult> RollbackToBuildManualStepVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.BuildManualStep> query = (from x in _context.BuildManualSteps
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this BuildManualStep concurrently
			//
			lock (buildManualStepPutSyncRoot)
			{
				
				Database.BuildManualStep buildManualStep = query.FirstOrDefault();
				
				if (buildManualStep == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildManualStep rollback", id.ToString(), new Exception("No BMC.BuildManualStep entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the BuildManualStep current state so we can log it.
				//
				Database.BuildManualStep cloneOfExisting = (Database.BuildManualStep)_context.Entry(buildManualStep).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.BuildManualStepChangeHistories = null;
				cloneOfExisting.BuildStepAnnotations = null;
				cloneOfExisting.BuildStepParts = null;
				cloneOfExisting.buildManualPage = null;

				if (versionNumber >= buildManualStep.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.BuildManualStep rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.BuildManualStep rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				BuildManualStepChangeHistory buildManualStepChangeHistory = (from x in _context.BuildManualStepChangeHistories
				                                               where
				                                               x.buildManualStepId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (buildManualStepChangeHistory != null)
				{
				    Database.BuildManualStep oldBuildManualStep = JsonSerializer.Deserialize<Database.BuildManualStep>(buildManualStepChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    buildManualStep.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    buildManualStep.buildManualPageId = oldBuildManualStep.buildManualPageId;
				    buildManualStep.stepNumber = oldBuildManualStep.stepNumber;
				    buildManualStep.cameraPositionX = oldBuildManualStep.cameraPositionX;
				    buildManualStep.cameraPositionY = oldBuildManualStep.cameraPositionY;
				    buildManualStep.cameraPositionZ = oldBuildManualStep.cameraPositionZ;
				    buildManualStep.cameraTargetX = oldBuildManualStep.cameraTargetX;
				    buildManualStep.cameraTargetY = oldBuildManualStep.cameraTargetY;
				    buildManualStep.cameraTargetZ = oldBuildManualStep.cameraTargetZ;
				    buildManualStep.cameraZoom = oldBuildManualStep.cameraZoom;
				    buildManualStep.showExplodedView = oldBuildManualStep.showExplodedView;
				    buildManualStep.explodedDistance = oldBuildManualStep.explodedDistance;
				    buildManualStep.renderImagePath = oldBuildManualStep.renderImagePath;
				    buildManualStep.pliImagePath = oldBuildManualStep.pliImagePath;
				    buildManualStep.fadeStepEnabled = oldBuildManualStep.fadeStepEnabled;
				    buildManualStep.isCallout = oldBuildManualStep.isCallout;
				    buildManualStep.calloutModelName = oldBuildManualStep.calloutModelName;
				    buildManualStep.showPartsListImage = oldBuildManualStep.showPartsListImage;
				    buildManualStep.objectGuid = oldBuildManualStep.objectGuid;
				    buildManualStep.active = oldBuildManualStep.active;
				    buildManualStep.deleted = oldBuildManualStep.deleted;

				    string serializedBuildManualStep = JsonSerializer.Serialize(buildManualStep);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BuildManualStepChangeHistory newBuildManualStepChangeHistory = new BuildManualStepChangeHistory();
				        newBuildManualStepChangeHistory.buildManualStepId = buildManualStep.id;
				        newBuildManualStepChangeHistory.versionNumber = buildManualStep.versionNumber;
				        newBuildManualStepChangeHistory.timeStamp = DateTime.UtcNow;
				        newBuildManualStepChangeHistory.userId = securityUser.id;
				        newBuildManualStepChangeHistory.tenantGuid = userTenantGuid;
				        newBuildManualStepChangeHistory.data = JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep));
				        _context.BuildManualStepChangeHistories.Add(newBuildManualStepChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BuildManualStep rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep)),
						null);


				    return Ok(Database.BuildManualStep.CreateAnonymous(buildManualStep));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.BuildManualStep rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.BuildManualStep rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a BuildManualStep.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildManualStep</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualStep/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetBuildManualStepChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.BuildManualStep buildManualStep = await _context.BuildManualSteps.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildManualStep == null)
			{
				return NotFound();
			}

			try
			{
				buildManualStep.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.BuildManualStep> versionInfo = await buildManualStep.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a BuildManualStep.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildManualStep</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualStep/{id}/AuditHistory")]
		public async Task<IActionResult> GetBuildManualStepAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.BuildManualStep buildManualStep = await _context.BuildManualSteps.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildManualStep == null)
			{
				return NotFound();
			}

			try
			{
				buildManualStep.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.BuildManualStep>> versions = await buildManualStep.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a BuildManualStep.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildManualStep</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The BuildManualStep object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualStep/{id}/Version/{version}")]
		public async Task<IActionResult> GetBuildManualStepVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.BuildManualStep buildManualStep = await _context.BuildManualSteps.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildManualStep == null)
			{
				return NotFound();
			}

			try
			{
				buildManualStep.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.BuildManualStep> versionInfo = await buildManualStep.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a BuildManualStep at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildManualStep</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The BuildManualStep object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildManualStep/{id}/StateAtTime")]
		public async Task<IActionResult> GetBuildManualStepStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.BuildManualStep buildManualStep = await _context.BuildManualSteps.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildManualStep == null)
			{
				return NotFound();
			}

			try
			{
				buildManualStep.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.BuildManualStep> versionInfo = await buildManualStep.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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


			lock (buildManualStepDeleteSyncRoot)
			{
			    try
			    {
			        buildManualStep.deleted = true;
			        buildManualStep.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        BuildManualStepChangeHistory buildManualStepChangeHistory = new BuildManualStepChangeHistory();
			        buildManualStepChangeHistory.buildManualStepId = buildManualStep.id;
			        buildManualStepChangeHistory.versionNumber = buildManualStep.versionNumber;
			        buildManualStepChangeHistory.timeStamp = DateTime.UtcNow;
			        buildManualStepChangeHistory.userId = securityUser.id;
			        buildManualStepChangeHistory.tenantGuid = userTenantGuid;
			        buildManualStepChangeHistory.data = JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep));
			        _context.BuildManualStepChangeHistories.Add(buildManualStepChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.BuildManualStep entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.BuildManualStep entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildManualStep.CreateAnonymousWithFirstLevelSubObjects(buildManualStep)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
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
			string renderImagePath = null,
			string pliImagePath = null,
			bool? fadeStepEnabled = null,
			bool? isCallout = null,
			string calloutModelName = null,
			bool? showPartsListImage = null,
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
			if (string.IsNullOrEmpty(renderImagePath) == false)
			{
				query = query.Where(bms => bms.renderImagePath == renderImagePath);
			}
			if (string.IsNullOrEmpty(pliImagePath) == false)
			{
				query = query.Where(bms => bms.pliImagePath == pliImagePath);
			}
			if (fadeStepEnabled.HasValue == true)
			{
				query = query.Where(bms => bms.fadeStepEnabled == fadeStepEnabled.Value);
			}
			if (isCallout.HasValue == true)
			{
				query = query.Where(bms => bms.isCallout == isCallout.Value);
			}
			if (string.IsNullOrEmpty(calloutModelName) == false)
			{
				query = query.Where(bms => bms.calloutModelName == calloutModelName);
			}
			if (showPartsListImage.HasValue == true)
			{
				query = query.Where(bms => bms.showPartsListImage == showPartsListImage.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bms => bms.versionNumber == versionNumber.Value);
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


			//
			// Add the any string contains parameter to span all the string fields on the Build Manual Step, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.renderImagePath.Contains(anyStringContains)
			       || x.pliImagePath.Contains(anyStringContains)
			       || x.calloutModelName.Contains(anyStringContains)
			       || x.buildManualPage.title.Contains(anyStringContains)
			       || x.buildManualPage.notes.Contains(anyStringContains)
			       || x.buildManualPage.backgroundTheme.Contains(anyStringContains)
			       || x.buildManualPage.layoutPreset.Contains(anyStringContains)
			       || x.buildManualPage.backgroundColorHex.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.calloutModelName);
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
