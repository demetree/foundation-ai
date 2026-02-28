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
    /// This auto generated class provides the basic CRUD operations for the ModelBuildStep entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ModelBuildStep entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ModelBuildStepsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<ModelBuildStepsController> _logger;

		public ModelBuildStepsController(BMCContext context, ILogger<ModelBuildStepsController> logger) : base("BMC", "ModelBuildStep")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ModelBuildSteps filtered by the parameters provided.
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
		[Route("api/ModelBuildSteps")]
		public async Task<IActionResult> GetModelBuildSteps(
			int? modelSubFileId = null,
			int? stepNumber = null,
			string rotationType = null,
			float? rotationX = null,
			float? rotationY = null,
			float? rotationZ = null,
			string description = null,
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

			IQueryable<Database.ModelBuildStep> query = (from mbs in _context.ModelBuildSteps select mbs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (modelSubFileId.HasValue == true)
			{
				query = query.Where(mbs => mbs.modelSubFileId == modelSubFileId.Value);
			}
			if (stepNumber.HasValue == true)
			{
				query = query.Where(mbs => mbs.stepNumber == stepNumber.Value);
			}
			if (string.IsNullOrEmpty(rotationType) == false)
			{
				query = query.Where(mbs => mbs.rotationType == rotationType);
			}
			if (rotationX.HasValue == true)
			{
				query = query.Where(mbs => mbs.rotationX == rotationX.Value);
			}
			if (rotationY.HasValue == true)
			{
				query = query.Where(mbs => mbs.rotationY == rotationY.Value);
			}
			if (rotationZ.HasValue == true)
			{
				query = query.Where(mbs => mbs.rotationZ == rotationZ.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(mbs => mbs.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mbs => mbs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mbs => mbs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mbs => mbs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mbs => mbs.deleted == false);
				}
			}
			else
			{
				query = query.Where(mbs => mbs.active == true);
				query = query.Where(mbs => mbs.deleted == false);
			}

			query = query.OrderBy(mbs => mbs.rotationType);


			//
			// Add the any string contains parameter to span all the string fields on the Model Build Step, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.rotationType.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.modelSubFile.fileName.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.modelSubFile);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ModelBuildStep> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ModelBuildStep modelBuildStep in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(modelBuildStep, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ModelBuildStep Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ModelBuildStep Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ModelBuildSteps filtered by the parameters provided.  Its query is similar to the GetModelBuildSteps method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelBuildSteps/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? modelSubFileId = null,
			int? stepNumber = null,
			string rotationType = null,
			float? rotationX = null,
			float? rotationY = null,
			float? rotationZ = null,
			string description = null,
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


			IQueryable<Database.ModelBuildStep> query = (from mbs in _context.ModelBuildSteps select mbs);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (modelSubFileId.HasValue == true)
			{
				query = query.Where(mbs => mbs.modelSubFileId == modelSubFileId.Value);
			}
			if (stepNumber.HasValue == true)
			{
				query = query.Where(mbs => mbs.stepNumber == stepNumber.Value);
			}
			if (rotationType != null)
			{
				query = query.Where(mbs => mbs.rotationType == rotationType);
			}
			if (rotationX.HasValue == true)
			{
				query = query.Where(mbs => mbs.rotationX == rotationX.Value);
			}
			if (rotationY.HasValue == true)
			{
				query = query.Where(mbs => mbs.rotationY == rotationY.Value);
			}
			if (rotationZ.HasValue == true)
			{
				query = query.Where(mbs => mbs.rotationZ == rotationZ.Value);
			}
			if (description != null)
			{
				query = query.Where(mbs => mbs.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mbs => mbs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mbs => mbs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mbs => mbs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mbs => mbs.deleted == false);
				}
			}
			else
			{
				query = query.Where(mbs => mbs.active == true);
				query = query.Where(mbs => mbs.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Model Build Step, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.rotationType.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.modelSubFile.fileName.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ModelBuildStep by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelBuildStep/{id}")]
		public async Task<IActionResult> GetModelBuildStep(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ModelBuildStep> query = (from mbs in _context.ModelBuildSteps where
							(mbs.id == id) &&
							(userIsAdmin == true || mbs.deleted == false) &&
							(userIsWriter == true || mbs.active == true)
					select mbs);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.modelSubFile);
					query = query.AsSplitQuery();
				}

				Database.ModelBuildStep materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ModelBuildStep Entity was read with Admin privilege." : "BMC.ModelBuildStep Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModelBuildStep", materialized.id, materialized.rotationType));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ModelBuildStep entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ModelBuildStep.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ModelBuildStep.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ModelBuildStep record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ModelBuildStep/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutModelBuildStep(int id, [FromBody]Database.ModelBuildStep.ModelBuildStepDTO modelBuildStepDTO, CancellationToken cancellationToken = default)
		{
			if (modelBuildStepDTO == null)
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



			if (id != modelBuildStepDTO.id)
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


			IQueryable<Database.ModelBuildStep> query = (from x in _context.ModelBuildSteps
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ModelBuildStep existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModelBuildStep PUT", id.ToString(), new Exception("No BMC.ModelBuildStep entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (modelBuildStepDTO.objectGuid == Guid.Empty)
            {
                modelBuildStepDTO.objectGuid = existing.objectGuid;
            }
            else if (modelBuildStepDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ModelBuildStep record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ModelBuildStep cloneOfExisting = (Database.ModelBuildStep)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ModelBuildStep object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ModelBuildStep modelBuildStep = (Database.ModelBuildStep)_context.Entry(existing).GetDatabaseValues().ToObject();
			modelBuildStep.ApplyDTO(modelBuildStepDTO);
			//
			// The tenant guid for any ModelBuildStep being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ModelBuildStep because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				modelBuildStep.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (modelBuildStep.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ModelBuildStep record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (modelBuildStep.rotationType != null && modelBuildStep.rotationType.Length > 10)
			{
				modelBuildStep.rotationType = modelBuildStep.rotationType.Substring(0, 10);
			}

			EntityEntry<Database.ModelBuildStep> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(modelBuildStep);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ModelBuildStep entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelBuildStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelBuildStep.CreateAnonymousWithFirstLevelSubObjects(modelBuildStep)),
					null);


				return Ok(Database.ModelBuildStep.CreateAnonymous(modelBuildStep));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ModelBuildStep entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelBuildStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelBuildStep.CreateAnonymousWithFirstLevelSubObjects(modelBuildStep)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ModelBuildStep record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelBuildStep", Name = "ModelBuildStep")]
		public async Task<IActionResult> PostModelBuildStep([FromBody]Database.ModelBuildStep.ModelBuildStepDTO modelBuildStepDTO, CancellationToken cancellationToken = default)
		{
			if (modelBuildStepDTO == null)
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
			// Create a new ModelBuildStep object using the data from the DTO
			//
			Database.ModelBuildStep modelBuildStep = Database.ModelBuildStep.FromDTO(modelBuildStepDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				modelBuildStep.tenantGuid = userTenantGuid;

				if (modelBuildStep.rotationType != null && modelBuildStep.rotationType.Length > 10)
				{
					modelBuildStep.rotationType = modelBuildStep.rotationType.Substring(0, 10);
				}

				modelBuildStep.objectGuid = Guid.NewGuid();
				_context.ModelBuildSteps.Add(modelBuildStep);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ModelBuildStep entity successfully created.",
					true,
					modelBuildStep.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ModelBuildStep.CreateAnonymousWithFirstLevelSubObjects(modelBuildStep)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ModelBuildStep entity creation failed.", false, modelBuildStep.id.ToString(), "", JsonSerializer.Serialize(modelBuildStep), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModelBuildStep", modelBuildStep.id, modelBuildStep.rotationType));

			return CreatedAtRoute("ModelBuildStep", new { id = modelBuildStep.id }, Database.ModelBuildStep.CreateAnonymousWithFirstLevelSubObjects(modelBuildStep));
		}



        /// <summary>
        /// 
        /// This deletes a ModelBuildStep record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModelBuildStep/{id}")]
		[Route("api/ModelBuildStep")]
		public async Task<IActionResult> DeleteModelBuildStep(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ModelBuildStep> query = (from x in _context.ModelBuildSteps
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ModelBuildStep modelBuildStep = await query.FirstOrDefaultAsync(cancellationToken);

			if (modelBuildStep == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModelBuildStep DELETE", id.ToString(), new Exception("No BMC.ModelBuildStep entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ModelBuildStep cloneOfExisting = (Database.ModelBuildStep)_context.Entry(modelBuildStep).GetDatabaseValues().ToObject();


			try
			{
				modelBuildStep.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ModelBuildStep entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelBuildStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelBuildStep.CreateAnonymousWithFirstLevelSubObjects(modelBuildStep)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ModelBuildStep entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModelBuildStep.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModelBuildStep.CreateAnonymousWithFirstLevelSubObjects(modelBuildStep)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ModelBuildStep records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ModelBuildSteps/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? modelSubFileId = null,
			int? stepNumber = null,
			string rotationType = null,
			float? rotationX = null,
			float? rotationY = null,
			float? rotationZ = null,
			string description = null,
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

			IQueryable<Database.ModelBuildStep> query = (from mbs in _context.ModelBuildSteps select mbs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (modelSubFileId.HasValue == true)
			{
				query = query.Where(mbs => mbs.modelSubFileId == modelSubFileId.Value);
			}
			if (stepNumber.HasValue == true)
			{
				query = query.Where(mbs => mbs.stepNumber == stepNumber.Value);
			}
			if (string.IsNullOrEmpty(rotationType) == false)
			{
				query = query.Where(mbs => mbs.rotationType == rotationType);
			}
			if (rotationX.HasValue == true)
			{
				query = query.Where(mbs => mbs.rotationX == rotationX.Value);
			}
			if (rotationY.HasValue == true)
			{
				query = query.Where(mbs => mbs.rotationY == rotationY.Value);
			}
			if (rotationZ.HasValue == true)
			{
				query = query.Where(mbs => mbs.rotationZ == rotationZ.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(mbs => mbs.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mbs => mbs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mbs => mbs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mbs => mbs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mbs => mbs.deleted == false);
				}
			}
			else
			{
				query = query.Where(mbs => mbs.active == true);
				query = query.Where(mbs => mbs.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Model Build Step, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.rotationType.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.modelSubFile.fileName.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.rotationType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ModelBuildStep.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ModelBuildStep/CreateAuditEvent")]
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
