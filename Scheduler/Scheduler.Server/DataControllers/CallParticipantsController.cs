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
    /// This auto generated class provides the basic CRUD operations for the CallParticipant entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the CallParticipant entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class CallParticipantsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<CallParticipantsController> _logger;

		public CallParticipantsController(SchedulerContext context, ILogger<CallParticipantsController> logger) : base("Scheduler", "CallParticipant")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of CallParticipants filtered by the parameters provided.
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
		[Route("api/CallParticipants")]
		public async Task<IActionResult> GetCallParticipants(
			int? callId = null,
			int? userId = null,
			string role = null,
			string status = null,
			DateTime? joinedDateTime = null,
			DateTime? leftDateTime = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
			if (joinedDateTime.HasValue == true && joinedDateTime.Value.Kind != DateTimeKind.Utc)
			{
				joinedDateTime = joinedDateTime.Value.ToUniversalTime();
			}

			if (leftDateTime.HasValue == true && leftDateTime.Value.Kind != DateTimeKind.Utc)
			{
				leftDateTime = leftDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.CallParticipant> query = (from cp in _context.CallParticipants select cp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (callId.HasValue == true)
			{
				query = query.Where(cp => cp.callId == callId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cp => cp.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(role) == false)
			{
				query = query.Where(cp => cp.role == role);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(cp => cp.status == status);
			}
			if (joinedDateTime.HasValue == true)
			{
				query = query.Where(cp => cp.joinedDateTime == joinedDateTime.Value);
			}
			if (leftDateTime.HasValue == true)
			{
				query = query.Where(cp => cp.leftDateTime == leftDateTime.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cp => cp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cp => cp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cp => cp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cp => cp.deleted == false);
				}
			}
			else
			{
				query = query.Where(cp => cp.active == true);
				query = query.Where(cp => cp.deleted == false);
			}

			query = query.OrderBy(cp => cp.role).ThenBy(cp => cp.status);


			//
			// Add the any string contains parameter to span all the string fields on the Call Participant, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.role.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || (includeRelations == true && x.call.providerId.Contains(anyStringContains))
			       || (includeRelations == true && x.call.providerCallId.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.call);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.CallParticipant> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.CallParticipant callParticipant in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(callParticipant, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.CallParticipant Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.CallParticipant Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of CallParticipants filtered by the parameters provided.  Its query is similar to the GetCallParticipants method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CallParticipants/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? callId = null,
			int? userId = null,
			string role = null,
			string status = null,
			DateTime? joinedDateTime = null,
			DateTime? leftDateTime = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
			if (joinedDateTime.HasValue == true && joinedDateTime.Value.Kind != DateTimeKind.Utc)
			{
				joinedDateTime = joinedDateTime.Value.ToUniversalTime();
			}

			if (leftDateTime.HasValue == true && leftDateTime.Value.Kind != DateTimeKind.Utc)
			{
				leftDateTime = leftDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.CallParticipant> query = (from cp in _context.CallParticipants select cp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (callId.HasValue == true)
			{
				query = query.Where(cp => cp.callId == callId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cp => cp.userId == userId.Value);
			}
			if (role != null)
			{
				query = query.Where(cp => cp.role == role);
			}
			if (status != null)
			{
				query = query.Where(cp => cp.status == status);
			}
			if (joinedDateTime.HasValue == true)
			{
				query = query.Where(cp => cp.joinedDateTime == joinedDateTime.Value);
			}
			if (leftDateTime.HasValue == true)
			{
				query = query.Where(cp => cp.leftDateTime == leftDateTime.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cp => cp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cp => cp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cp => cp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cp => cp.deleted == false);
				}
			}
			else
			{
				query = query.Where(cp => cp.active == true);
				query = query.Where(cp => cp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Call Participant, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.role.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.call.providerId.Contains(anyStringContains)
			       || x.call.providerCallId.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single CallParticipant by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CallParticipant/{id}")]
		public async Task<IActionResult> GetCallParticipant(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
				IQueryable<Database.CallParticipant> query = (from cp in _context.CallParticipants where
							(cp.id == id) &&
							(userIsAdmin == true || cp.deleted == false) &&
							(userIsWriter == true || cp.active == true)
					select cp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.call);
					query = query.AsSplitQuery();
				}

				Database.CallParticipant materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.CallParticipant Entity was read with Admin privilege." : "Scheduler.CallParticipant Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "CallParticipant", materialized.id, materialized.role));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.CallParticipant entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.CallParticipant.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.CallParticipant.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing CallParticipant record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/CallParticipant/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutCallParticipant(int id, [FromBody]Database.CallParticipant.CallParticipantDTO callParticipantDTO, CancellationToken cancellationToken = default)
		{
			if (callParticipantDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != callParticipantDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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


			IQueryable<Database.CallParticipant> query = (from x in _context.CallParticipants
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.CallParticipant existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.CallParticipant PUT", id.ToString(), new Exception("No Scheduler.CallParticipant entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (callParticipantDTO.objectGuid == Guid.Empty)
            {
                callParticipantDTO.objectGuid = existing.objectGuid;
            }
            else if (callParticipantDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a CallParticipant record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.CallParticipant cloneOfExisting = (Database.CallParticipant)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new CallParticipant object using the data from the existing record, updated with what is in the DTO.
			//
			Database.CallParticipant callParticipant = (Database.CallParticipant)_context.Entry(existing).GetDatabaseValues().ToObject();
			callParticipant.ApplyDTO(callParticipantDTO);
			//
			// The tenant guid for any CallParticipant being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the CallParticipant because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				callParticipant.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (callParticipant.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.CallParticipant record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (callParticipant.role != null && callParticipant.role.Length > 50)
			{
				callParticipant.role = callParticipant.role.Substring(0, 50);
			}

			if (callParticipant.status != null && callParticipant.status.Length > 50)
			{
				callParticipant.status = callParticipant.status.Substring(0, 50);
			}

			if (callParticipant.joinedDateTime.HasValue == true && callParticipant.joinedDateTime.Value.Kind != DateTimeKind.Utc)
			{
				callParticipant.joinedDateTime = callParticipant.joinedDateTime.Value.ToUniversalTime();
			}

			if (callParticipant.leftDateTime.HasValue == true && callParticipant.leftDateTime.Value.Kind != DateTimeKind.Utc)
			{
				callParticipant.leftDateTime = callParticipant.leftDateTime.Value.ToUniversalTime();
			}

			EntityEntry<Database.CallParticipant> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(callParticipant);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.CallParticipant entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.CallParticipant.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CallParticipant.CreateAnonymousWithFirstLevelSubObjects(callParticipant)),
					null);


				return Ok(Database.CallParticipant.CreateAnonymous(callParticipant));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.CallParticipant entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.CallParticipant.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CallParticipant.CreateAnonymousWithFirstLevelSubObjects(callParticipant)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new CallParticipant record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CallParticipant", Name = "CallParticipant")]
		public async Task<IActionResult> PostCallParticipant([FromBody]Database.CallParticipant.CallParticipantDTO callParticipantDTO, CancellationToken cancellationToken = default)
		{
			if (callParticipantDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
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

			//
			// Create a new CallParticipant object using the data from the DTO
			//
			Database.CallParticipant callParticipant = Database.CallParticipant.FromDTO(callParticipantDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				callParticipant.tenantGuid = userTenantGuid;

				if (callParticipant.role != null && callParticipant.role.Length > 50)
				{
					callParticipant.role = callParticipant.role.Substring(0, 50);
				}

				if (callParticipant.status != null && callParticipant.status.Length > 50)
				{
					callParticipant.status = callParticipant.status.Substring(0, 50);
				}

				if (callParticipant.joinedDateTime.HasValue == true && callParticipant.joinedDateTime.Value.Kind != DateTimeKind.Utc)
				{
					callParticipant.joinedDateTime = callParticipant.joinedDateTime.Value.ToUniversalTime();
				}

				if (callParticipant.leftDateTime.HasValue == true && callParticipant.leftDateTime.Value.Kind != DateTimeKind.Utc)
				{
					callParticipant.leftDateTime = callParticipant.leftDateTime.Value.ToUniversalTime();
				}

				callParticipant.objectGuid = Guid.NewGuid();
				_context.CallParticipants.Add(callParticipant);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.CallParticipant entity successfully created.",
					true,
					callParticipant.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.CallParticipant.CreateAnonymousWithFirstLevelSubObjects(callParticipant)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.CallParticipant entity creation failed.", false, callParticipant.id.ToString(), "", JsonSerializer.Serialize(callParticipant), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "CallParticipant", callParticipant.id, callParticipant.role));

			return CreatedAtRoute("CallParticipant", new { id = callParticipant.id }, Database.CallParticipant.CreateAnonymousWithFirstLevelSubObjects(callParticipant));
		}



        /// <summary>
        /// 
        /// This deletes a CallParticipant record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CallParticipant/{id}")]
		[Route("api/CallParticipant")]
		public async Task<IActionResult> DeleteCallParticipant(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.CallParticipant> query = (from x in _context.CallParticipants
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.CallParticipant callParticipant = await query.FirstOrDefaultAsync(cancellationToken);

			if (callParticipant == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.CallParticipant DELETE", id.ToString(), new Exception("No Scheduler.CallParticipant entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.CallParticipant cloneOfExisting = (Database.CallParticipant)_context.Entry(callParticipant).GetDatabaseValues().ToObject();


			try
			{
				callParticipant.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.CallParticipant entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.CallParticipant.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CallParticipant.CreateAnonymousWithFirstLevelSubObjects(callParticipant)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.CallParticipant entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.CallParticipant.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CallParticipant.CreateAnonymousWithFirstLevelSubObjects(callParticipant)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of CallParticipant records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/CallParticipants/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? callId = null,
			int? userId = null,
			string role = null,
			string status = null,
			DateTime? joinedDateTime = null,
			DateTime? leftDateTime = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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
			if (joinedDateTime.HasValue == true && joinedDateTime.Value.Kind != DateTimeKind.Utc)
			{
				joinedDateTime = joinedDateTime.Value.ToUniversalTime();
			}

			if (leftDateTime.HasValue == true && leftDateTime.Value.Kind != DateTimeKind.Utc)
			{
				leftDateTime = leftDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.CallParticipant> query = (from cp in _context.CallParticipants select cp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (callId.HasValue == true)
			{
				query = query.Where(cp => cp.callId == callId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cp => cp.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(role) == false)
			{
				query = query.Where(cp => cp.role == role);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(cp => cp.status == status);
			}
			if (joinedDateTime.HasValue == true)
			{
				query = query.Where(cp => cp.joinedDateTime == joinedDateTime.Value);
			}
			if (leftDateTime.HasValue == true)
			{
				query = query.Where(cp => cp.leftDateTime == leftDateTime.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cp => cp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cp => cp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cp => cp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cp => cp.deleted == false);
				}
			}
			else
			{
				query = query.Where(cp => cp.active == true);
				query = query.Where(cp => cp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Call Participant, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.role.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.call.providerId.Contains(anyStringContains)
			       || x.call.providerCallId.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.role).ThenBy(x => x.status);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.CallParticipant.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/CallParticipant/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
