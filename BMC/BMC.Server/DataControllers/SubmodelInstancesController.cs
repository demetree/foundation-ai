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
    /// This auto generated class provides the basic CRUD operations for the SubmodelInstance entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SubmodelInstance entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SubmodelInstancesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<SubmodelInstancesController> _logger;

		public SubmodelInstancesController(BMCContext context, ILogger<SubmodelInstancesController> logger) : base("BMC", "SubmodelInstance")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SubmodelInstances filtered by the parameters provided.
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
		[Route("api/SubmodelInstances")]
		public async Task<IActionResult> GetSubmodelInstances(
			int? submodelId = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? rotationX = null,
			float? rotationY = null,
			float? rotationZ = null,
			float? rotationW = null,
			int? colourCode = null,
			int? buildStepNumber = null,
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

			IQueryable<Database.SubmodelInstance> query = (from si in _context.SubmodelInstances select si);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (submodelId.HasValue == true)
			{
				query = query.Where(si => si.submodelId == submodelId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(si => si.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(si => si.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(si => si.positionZ == positionZ.Value);
			}
			if (rotationX.HasValue == true)
			{
				query = query.Where(si => si.rotationX == rotationX.Value);
			}
			if (rotationY.HasValue == true)
			{
				query = query.Where(si => si.rotationY == rotationY.Value);
			}
			if (rotationZ.HasValue == true)
			{
				query = query.Where(si => si.rotationZ == rotationZ.Value);
			}
			if (rotationW.HasValue == true)
			{
				query = query.Where(si => si.rotationW == rotationW.Value);
			}
			if (colourCode.HasValue == true)
			{
				query = query.Where(si => si.colourCode == colourCode.Value);
			}
			if (buildStepNumber.HasValue == true)
			{
				query = query.Where(si => si.buildStepNumber == buildStepNumber.Value);
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

			query = query.OrderBy(si => si.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.submodel);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.SubmodelInstance> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SubmodelInstance submodelInstance in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(submodelInstance, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.SubmodelInstance Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.SubmodelInstance Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SubmodelInstances filtered by the parameters provided.  Its query is similar to the GetSubmodelInstances method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SubmodelInstances/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? submodelId = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? rotationX = null,
			float? rotationY = null,
			float? rotationZ = null,
			float? rotationW = null,
			int? colourCode = null,
			int? buildStepNumber = null,
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


			IQueryable<Database.SubmodelInstance> query = (from si in _context.SubmodelInstances select si);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (submodelId.HasValue == true)
			{
				query = query.Where(si => si.submodelId == submodelId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(si => si.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(si => si.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(si => si.positionZ == positionZ.Value);
			}
			if (rotationX.HasValue == true)
			{
				query = query.Where(si => si.rotationX == rotationX.Value);
			}
			if (rotationY.HasValue == true)
			{
				query = query.Where(si => si.rotationY == rotationY.Value);
			}
			if (rotationZ.HasValue == true)
			{
				query = query.Where(si => si.rotationZ == rotationZ.Value);
			}
			if (rotationW.HasValue == true)
			{
				query = query.Where(si => si.rotationW == rotationW.Value);
			}
			if (colourCode.HasValue == true)
			{
				query = query.Where(si => si.colourCode == colourCode.Value);
			}
			if (buildStepNumber.HasValue == true)
			{
				query = query.Where(si => si.buildStepNumber == buildStepNumber.Value);
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

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SubmodelInstance by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SubmodelInstance/{id}")]
		public async Task<IActionResult> GetSubmodelInstance(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SubmodelInstance> query = (from si in _context.SubmodelInstances where
							(si.id == id) &&
							(userIsAdmin == true || si.deleted == false) &&
							(userIsWriter == true || si.active == true)
					select si);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.submodel);
					query = query.AsSplitQuery();
				}

				Database.SubmodelInstance materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.SubmodelInstance Entity was read with Admin privilege." : "BMC.SubmodelInstance Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SubmodelInstance", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.SubmodelInstance entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.SubmodelInstance.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.SubmodelInstance.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SubmodelInstance record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SubmodelInstance/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSubmodelInstance(int id, [FromBody]Database.SubmodelInstance.SubmodelInstanceDTO submodelInstanceDTO, CancellationToken cancellationToken = default)
		{
			if (submodelInstanceDTO == null)
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



			if (id != submodelInstanceDTO.id)
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


			IQueryable<Database.SubmodelInstance> query = (from x in _context.SubmodelInstances
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SubmodelInstance existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.SubmodelInstance PUT", id.ToString(), new Exception("No BMC.SubmodelInstance entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (submodelInstanceDTO.objectGuid == Guid.Empty)
            {
                submodelInstanceDTO.objectGuid = existing.objectGuid;
            }
            else if (submodelInstanceDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SubmodelInstance record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SubmodelInstance cloneOfExisting = (Database.SubmodelInstance)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SubmodelInstance object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SubmodelInstance submodelInstance = (Database.SubmodelInstance)_context.Entry(existing).GetDatabaseValues().ToObject();
			submodelInstance.ApplyDTO(submodelInstanceDTO);
			//
			// The tenant guid for any SubmodelInstance being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the SubmodelInstance because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				submodelInstance.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (submodelInstance.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.SubmodelInstance record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.SubmodelInstance> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(submodelInstance);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.SubmodelInstance entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SubmodelInstance.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SubmodelInstance.CreateAnonymousWithFirstLevelSubObjects(submodelInstance)),
					null);


				return Ok(Database.SubmodelInstance.CreateAnonymous(submodelInstance));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.SubmodelInstance entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SubmodelInstance.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SubmodelInstance.CreateAnonymousWithFirstLevelSubObjects(submodelInstance)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SubmodelInstance record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SubmodelInstance", Name = "SubmodelInstance")]
		public async Task<IActionResult> PostSubmodelInstance([FromBody]Database.SubmodelInstance.SubmodelInstanceDTO submodelInstanceDTO, CancellationToken cancellationToken = default)
		{
			if (submodelInstanceDTO == null)
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
			// Create a new SubmodelInstance object using the data from the DTO
			//
			Database.SubmodelInstance submodelInstance = Database.SubmodelInstance.FromDTO(submodelInstanceDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				submodelInstance.tenantGuid = userTenantGuid;

				submodelInstance.objectGuid = Guid.NewGuid();
				_context.SubmodelInstances.Add(submodelInstance);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.SubmodelInstance entity successfully created.",
					true,
					submodelInstance.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SubmodelInstance.CreateAnonymousWithFirstLevelSubObjects(submodelInstance)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.SubmodelInstance entity creation failed.", false, submodelInstance.id.ToString(), "", JsonSerializer.Serialize(submodelInstance), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SubmodelInstance", submodelInstance.id, submodelInstance.id.ToString()));

			return CreatedAtRoute("SubmodelInstance", new { id = submodelInstance.id }, Database.SubmodelInstance.CreateAnonymousWithFirstLevelSubObjects(submodelInstance));
		}



        /// <summary>
        /// 
        /// This deletes a SubmodelInstance record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SubmodelInstance/{id}")]
		[Route("api/SubmodelInstance")]
		public async Task<IActionResult> DeleteSubmodelInstance(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.SubmodelInstance> query = (from x in _context.SubmodelInstances
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SubmodelInstance submodelInstance = await query.FirstOrDefaultAsync(cancellationToken);

			if (submodelInstance == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.SubmodelInstance DELETE", id.ToString(), new Exception("No BMC.SubmodelInstance entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SubmodelInstance cloneOfExisting = (Database.SubmodelInstance)_context.Entry(submodelInstance).GetDatabaseValues().ToObject();


			try
			{
				submodelInstance.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.SubmodelInstance entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SubmodelInstance.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SubmodelInstance.CreateAnonymousWithFirstLevelSubObjects(submodelInstance)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.SubmodelInstance entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SubmodelInstance.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SubmodelInstance.CreateAnonymousWithFirstLevelSubObjects(submodelInstance)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SubmodelInstance records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SubmodelInstances/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? submodelId = null,
			float? positionX = null,
			float? positionY = null,
			float? positionZ = null,
			float? rotationX = null,
			float? rotationY = null,
			float? rotationZ = null,
			float? rotationW = null,
			int? colourCode = null,
			int? buildStepNumber = null,
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

			IQueryable<Database.SubmodelInstance> query = (from si in _context.SubmodelInstances select si);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (submodelId.HasValue == true)
			{
				query = query.Where(si => si.submodelId == submodelId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(si => si.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(si => si.positionY == positionY.Value);
			}
			if (positionZ.HasValue == true)
			{
				query = query.Where(si => si.positionZ == positionZ.Value);
			}
			if (rotationX.HasValue == true)
			{
				query = query.Where(si => si.rotationX == rotationX.Value);
			}
			if (rotationY.HasValue == true)
			{
				query = query.Where(si => si.rotationY == rotationY.Value);
			}
			if (rotationZ.HasValue == true)
			{
				query = query.Where(si => si.rotationZ == rotationZ.Value);
			}
			if (rotationW.HasValue == true)
			{
				query = query.Where(si => si.rotationW == rotationW.Value);
			}
			if (colourCode.HasValue == true)
			{
				query = query.Where(si => si.colourCode == colourCode.Value);
			}
			if (buildStepNumber.HasValue == true)
			{
				query = query.Where(si => si.buildStepNumber == buildStepNumber.Value);
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


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SubmodelInstance.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SubmodelInstance/CreateAuditEvent")]
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
