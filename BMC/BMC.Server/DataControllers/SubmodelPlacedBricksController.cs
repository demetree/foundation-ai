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
    /// This auto generated class provides the basic CRUD operations for the SubmodelPlacedBrick entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SubmodelPlacedBrick entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SubmodelPlacedBricksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<SubmodelPlacedBricksController> _logger;

		public SubmodelPlacedBricksController(BMCContext context, ILogger<SubmodelPlacedBricksController> logger) : base("BMC", "SubmodelPlacedBrick")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SubmodelPlacedBricks filtered by the parameters provided.
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
		[Route("api/SubmodelPlacedBricks")]
		public async Task<IActionResult> GetSubmodelPlacedBricks(
			int? submodelId = null,
			int? placedBrickId = null,
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

			IQueryable<Database.SubmodelPlacedBrick> query = (from spb in _context.SubmodelPlacedBricks select spb);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (submodelId.HasValue == true)
			{
				query = query.Where(spb => spb.submodelId == submodelId.Value);
			}
			if (placedBrickId.HasValue == true)
			{
				query = query.Where(spb => spb.placedBrickId == placedBrickId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(spb => spb.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(spb => spb.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(spb => spb.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(spb => spb.deleted == false);
				}
			}
			else
			{
				query = query.Where(spb => spb.active == true);
				query = query.Where(spb => spb.deleted == false);
			}

			query = query.OrderBy(spb => spb.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.placedBrick);
				query = query.Include(x => x.submodel);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.SubmodelPlacedBrick> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SubmodelPlacedBrick submodelPlacedBrick in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(submodelPlacedBrick, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.SubmodelPlacedBrick Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.SubmodelPlacedBrick Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SubmodelPlacedBricks filtered by the parameters provided.  Its query is similar to the GetSubmodelPlacedBricks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SubmodelPlacedBricks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? submodelId = null,
			int? placedBrickId = null,
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


			IQueryable<Database.SubmodelPlacedBrick> query = (from spb in _context.SubmodelPlacedBricks select spb);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (submodelId.HasValue == true)
			{
				query = query.Where(spb => spb.submodelId == submodelId.Value);
			}
			if (placedBrickId.HasValue == true)
			{
				query = query.Where(spb => spb.placedBrickId == placedBrickId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(spb => spb.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(spb => spb.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(spb => spb.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(spb => spb.deleted == false);
				}
			}
			else
			{
				query = query.Where(spb => spb.active == true);
				query = query.Where(spb => spb.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SubmodelPlacedBrick by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SubmodelPlacedBrick/{id}")]
		public async Task<IActionResult> GetSubmodelPlacedBrick(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.SubmodelPlacedBrick> query = (from spb in _context.SubmodelPlacedBricks where
							(spb.id == id) &&
							(userIsAdmin == true || spb.deleted == false) &&
							(userIsWriter == true || spb.active == true)
					select spb);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.placedBrick);
					query = query.Include(x => x.submodel);
					query = query.AsSplitQuery();
				}

				Database.SubmodelPlacedBrick materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.SubmodelPlacedBrick Entity was read with Admin privilege." : "BMC.SubmodelPlacedBrick Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SubmodelPlacedBrick", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.SubmodelPlacedBrick entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.SubmodelPlacedBrick.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.SubmodelPlacedBrick.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SubmodelPlacedBrick record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SubmodelPlacedBrick/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSubmodelPlacedBrick(int id, [FromBody]Database.SubmodelPlacedBrick.SubmodelPlacedBrickDTO submodelPlacedBrickDTO, CancellationToken cancellationToken = default)
		{
			if (submodelPlacedBrickDTO == null)
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



			if (id != submodelPlacedBrickDTO.id)
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


			IQueryable<Database.SubmodelPlacedBrick> query = (from x in _context.SubmodelPlacedBricks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SubmodelPlacedBrick existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.SubmodelPlacedBrick PUT", id.ToString(), new Exception("No BMC.SubmodelPlacedBrick entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (submodelPlacedBrickDTO.objectGuid == Guid.Empty)
            {
                submodelPlacedBrickDTO.objectGuid = existing.objectGuid;
            }
            else if (submodelPlacedBrickDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SubmodelPlacedBrick record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SubmodelPlacedBrick cloneOfExisting = (Database.SubmodelPlacedBrick)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SubmodelPlacedBrick object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SubmodelPlacedBrick submodelPlacedBrick = (Database.SubmodelPlacedBrick)_context.Entry(existing).GetDatabaseValues().ToObject();
			submodelPlacedBrick.ApplyDTO(submodelPlacedBrickDTO);
			//
			// The tenant guid for any SubmodelPlacedBrick being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the SubmodelPlacedBrick because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				submodelPlacedBrick.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (submodelPlacedBrick.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.SubmodelPlacedBrick record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			EntityEntry<Database.SubmodelPlacedBrick> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(submodelPlacedBrick);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.SubmodelPlacedBrick entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SubmodelPlacedBrick.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SubmodelPlacedBrick.CreateAnonymousWithFirstLevelSubObjects(submodelPlacedBrick)),
					null);


				return Ok(Database.SubmodelPlacedBrick.CreateAnonymous(submodelPlacedBrick));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.SubmodelPlacedBrick entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SubmodelPlacedBrick.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SubmodelPlacedBrick.CreateAnonymousWithFirstLevelSubObjects(submodelPlacedBrick)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SubmodelPlacedBrick record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SubmodelPlacedBrick", Name = "SubmodelPlacedBrick")]
		public async Task<IActionResult> PostSubmodelPlacedBrick([FromBody]Database.SubmodelPlacedBrick.SubmodelPlacedBrickDTO submodelPlacedBrickDTO, CancellationToken cancellationToken = default)
		{
			if (submodelPlacedBrickDTO == null)
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
			// Create a new SubmodelPlacedBrick object using the data from the DTO
			//
			Database.SubmodelPlacedBrick submodelPlacedBrick = Database.SubmodelPlacedBrick.FromDTO(submodelPlacedBrickDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				submodelPlacedBrick.tenantGuid = userTenantGuid;

				submodelPlacedBrick.objectGuid = Guid.NewGuid();
				_context.SubmodelPlacedBricks.Add(submodelPlacedBrick);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.SubmodelPlacedBrick entity successfully created.",
					true,
					submodelPlacedBrick.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SubmodelPlacedBrick.CreateAnonymousWithFirstLevelSubObjects(submodelPlacedBrick)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.SubmodelPlacedBrick entity creation failed.", false, submodelPlacedBrick.id.ToString(), "", JsonSerializer.Serialize(submodelPlacedBrick), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SubmodelPlacedBrick", submodelPlacedBrick.id, submodelPlacedBrick.id.ToString()));

			return CreatedAtRoute("SubmodelPlacedBrick", new { id = submodelPlacedBrick.id }, Database.SubmodelPlacedBrick.CreateAnonymousWithFirstLevelSubObjects(submodelPlacedBrick));
		}



        /// <summary>
        /// 
        /// This deletes a SubmodelPlacedBrick record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SubmodelPlacedBrick/{id}")]
		[Route("api/SubmodelPlacedBrick")]
		public async Task<IActionResult> DeleteSubmodelPlacedBrick(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.SubmodelPlacedBrick> query = (from x in _context.SubmodelPlacedBricks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SubmodelPlacedBrick submodelPlacedBrick = await query.FirstOrDefaultAsync(cancellationToken);

			if (submodelPlacedBrick == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.SubmodelPlacedBrick DELETE", id.ToString(), new Exception("No BMC.SubmodelPlacedBrick entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SubmodelPlacedBrick cloneOfExisting = (Database.SubmodelPlacedBrick)_context.Entry(submodelPlacedBrick).GetDatabaseValues().ToObject();


			try
			{
				submodelPlacedBrick.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.SubmodelPlacedBrick entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SubmodelPlacedBrick.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SubmodelPlacedBrick.CreateAnonymousWithFirstLevelSubObjects(submodelPlacedBrick)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.SubmodelPlacedBrick entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SubmodelPlacedBrick.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SubmodelPlacedBrick.CreateAnonymousWithFirstLevelSubObjects(submodelPlacedBrick)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SubmodelPlacedBrick records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SubmodelPlacedBricks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? submodelId = null,
			int? placedBrickId = null,
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

			IQueryable<Database.SubmodelPlacedBrick> query = (from spb in _context.SubmodelPlacedBricks select spb);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (submodelId.HasValue == true)
			{
				query = query.Where(spb => spb.submodelId == submodelId.Value);
			}
			if (placedBrickId.HasValue == true)
			{
				query = query.Where(spb => spb.placedBrickId == placedBrickId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(spb => spb.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(spb => spb.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(spb => spb.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(spb => spb.deleted == false);
				}
			}
			else
			{
				query = query.Where(spb => spb.active == true);
				query = query.Where(spb => spb.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SubmodelPlacedBrick.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SubmodelPlacedBrick/CreateAuditEvent")]
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
