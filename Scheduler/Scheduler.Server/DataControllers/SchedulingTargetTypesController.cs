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
using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the SchedulingTargetType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SchedulingTargetType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SchedulingTargetTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<SchedulingTargetTypesController> _logger;

		public SchedulingTargetTypesController(SchedulerContext context, ILogger<SchedulingTargetTypesController> logger) : base("Scheduler", "SchedulingTargetType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SchedulingTargetTypes filtered by the parameters provided.
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
		[Route("api/SchedulingTargetTypes")]
		public async Task<IActionResult> GetSchedulingTargetTypes(
			string name = null,
			string description = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
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

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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

			IQueryable<Database.SchedulingTargetType> query = (from stt in _context.SchedulingTargetTypes select stt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(stt => stt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(stt => stt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(stt => stt.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(stt => stt.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(stt => stt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stt => stt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stt => stt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stt => stt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stt => stt.deleted == false);
				}
			}
			else
			{
				query = query.Where(stt => stt.active == true);
				query = query.Where(stt => stt.deleted == false);
			}

			query = query.OrderBy(stt => stt.sequence).ThenBy(stt => stt.name).ThenBy(stt => stt.description).ThenBy(stt => stt.color);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.icon);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduling Target Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.SchedulingTargetType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SchedulingTargetType schedulingTargetType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(schedulingTargetType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.SchedulingTargetType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.SchedulingTargetType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SchedulingTargetTypes filtered by the parameters provided.  Its query is similar to the GetSchedulingTargetTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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


			IQueryable<Database.SchedulingTargetType> query = (from stt in _context.SchedulingTargetTypes select stt);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(stt => stt.name == name);
			}
			if (description != null)
			{
				query = query.Where(stt => stt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(stt => stt.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(stt => stt.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(stt => stt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stt => stt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stt => stt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stt => stt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stt => stt.deleted == false);
				}
			}
			else
			{
				query = query.Where(stt => stt.active == true);
				query = query.Where(stt => stt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Scheduling Target Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SchedulingTargetType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetType/{id}")]
		public async Task<IActionResult> GetSchedulingTargetType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			
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
				IQueryable<Database.SchedulingTargetType> query = (from stt in _context.SchedulingTargetTypes where
							(stt.id == id) &&
							(userIsAdmin == true || stt.deleted == false) &&
							(userIsWriter == true || stt.active == true)
					select stt);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.icon);
					query = query.AsSplitQuery();
				}

				Database.SchedulingTargetType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.SchedulingTargetType Entity was read with Admin privilege." : "Scheduler.SchedulingTargetType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SchedulingTargetType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.SchedulingTargetType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.SchedulingTargetType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.SchedulingTargetType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SchedulingTargetType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SchedulingTargetType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSchedulingTargetType(int id, [FromBody]Database.SchedulingTargetType.SchedulingTargetTypeDTO schedulingTargetTypeDTO, CancellationToken cancellationToken = default)
		{
			if (schedulingTargetTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != schedulingTargetTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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


			IQueryable<Database.SchedulingTargetType> query = (from x in _context.SchedulingTargetTypes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SchedulingTargetType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTargetType PUT", id.ToString(), new Exception("No Scheduler.SchedulingTargetType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (schedulingTargetTypeDTO.objectGuid == Guid.Empty)
            {
                schedulingTargetTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (schedulingTargetTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SchedulingTargetType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SchedulingTargetType cloneOfExisting = (Database.SchedulingTargetType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SchedulingTargetType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SchedulingTargetType schedulingTargetType = (Database.SchedulingTargetType)_context.Entry(existing).GetDatabaseValues().ToObject();
			schedulingTargetType.ApplyDTO(schedulingTargetTypeDTO);
			//
			// The tenant guid for any SchedulingTargetType being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the SchedulingTargetType because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				schedulingTargetType.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (schedulingTargetType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.SchedulingTargetType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (schedulingTargetType.name != null && schedulingTargetType.name.Length > 100)
			{
				schedulingTargetType.name = schedulingTargetType.name.Substring(0, 100);
			}

			if (schedulingTargetType.description != null && schedulingTargetType.description.Length > 500)
			{
				schedulingTargetType.description = schedulingTargetType.description.Substring(0, 500);
			}

			if (schedulingTargetType.color != null && schedulingTargetType.color.Length > 10)
			{
				schedulingTargetType.color = schedulingTargetType.color.Substring(0, 10);
			}

			EntityEntry<Database.SchedulingTargetType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(schedulingTargetType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.SchedulingTargetType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SchedulingTargetType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SchedulingTargetType.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetType)),
					null);


				return Ok(Database.SchedulingTargetType.CreateAnonymous(schedulingTargetType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.SchedulingTargetType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SchedulingTargetType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SchedulingTargetType.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SchedulingTargetType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetType", Name = "SchedulingTargetType")]
		public async Task<IActionResult> PostSchedulingTargetType([FromBody]Database.SchedulingTargetType.SchedulingTargetTypeDTO schedulingTargetTypeDTO, CancellationToken cancellationToken = default)
		{
			if (schedulingTargetTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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
			// Create a new SchedulingTargetType object using the data from the DTO
			//
			Database.SchedulingTargetType schedulingTargetType = Database.SchedulingTargetType.FromDTO(schedulingTargetTypeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				schedulingTargetType.tenantGuid = userTenantGuid;

				if (schedulingTargetType.name != null && schedulingTargetType.name.Length > 100)
				{
					schedulingTargetType.name = schedulingTargetType.name.Substring(0, 100);
				}

				if (schedulingTargetType.description != null && schedulingTargetType.description.Length > 500)
				{
					schedulingTargetType.description = schedulingTargetType.description.Substring(0, 500);
				}

				if (schedulingTargetType.color != null && schedulingTargetType.color.Length > 10)
				{
					schedulingTargetType.color = schedulingTargetType.color.Substring(0, 10);
				}

				schedulingTargetType.objectGuid = Guid.NewGuid();
				_context.SchedulingTargetTypes.Add(schedulingTargetType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.SchedulingTargetType entity successfully created.",
					true,
					schedulingTargetType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SchedulingTargetType.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.SchedulingTargetType entity creation failed.", false, schedulingTargetType.id.ToString(), "", JsonSerializer.Serialize(schedulingTargetType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SchedulingTargetType", schedulingTargetType.id, schedulingTargetType.name));

			return CreatedAtRoute("SchedulingTargetType", new { id = schedulingTargetType.id }, Database.SchedulingTargetType.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetType));
		}



        /// <summary>
        /// 
        /// This deletes a SchedulingTargetType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargetType/{id}")]
		[Route("api/SchedulingTargetType")]
		public async Task<IActionResult> DeleteSchedulingTargetType(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(cancellationToken);
			
			
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

			IQueryable<Database.SchedulingTargetType> query = (from x in _context.SchedulingTargetTypes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SchedulingTargetType schedulingTargetType = await query.FirstOrDefaultAsync(cancellationToken);

			if (schedulingTargetType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTargetType DELETE", id.ToString(), new Exception("No Scheduler.SchedulingTargetType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SchedulingTargetType cloneOfExisting = (Database.SchedulingTargetType)_context.Entry(schedulingTargetType).GetDatabaseValues().ToObject();


			try
			{
				schedulingTargetType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.SchedulingTargetType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SchedulingTargetType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SchedulingTargetType.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.SchedulingTargetType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SchedulingTargetType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SchedulingTargetType.CreateAnonymousWithFirstLevelSubObjects(schedulingTargetType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SchedulingTargetType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SchedulingTargetTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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

			IQueryable<Database.SchedulingTargetType> query = (from stt in _context.SchedulingTargetTypes select stt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(stt => stt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(stt => stt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(stt => stt.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(stt => stt.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(stt => stt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(stt => stt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(stt => stt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(stt => stt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(stt => stt.deleted == false);
				}
			}
			else
			{
				query = query.Where(stt => stt.active == true);
				query = query.Where(stt => stt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduling Target Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SchedulingTargetType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SchedulingTargetType/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
