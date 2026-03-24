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
    /// This auto generated class provides the basic CRUD operations for the Call entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Call entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class CallsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<CallsController> _logger;

		public CallsController(SchedulerContext context, ILogger<CallsController> logger) : base("Scheduler", "Call")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Calls filtered by the parameters provided.
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
		[Route("api/Calls")]
		public async Task<IActionResult> GetCalls(
			int? callTypeId = null,
			int? callStatusId = null,
			string providerId = null,
			string providerCallId = null,
			int? conversationId = null,
			int? initiatorUserId = null,
			DateTime? startDateTime = null,
			DateTime? answerDateTime = null,
			DateTime? endDateTime = null,
			int? durationSeconds = null,
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
			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (answerDateTime.HasValue == true && answerDateTime.Value.Kind != DateTimeKind.Utc)
			{
				answerDateTime = answerDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.Call> query = (from c in _context.Calls select c);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (callTypeId.HasValue == true)
			{
				query = query.Where(c => c.callTypeId == callTypeId.Value);
			}
			if (callStatusId.HasValue == true)
			{
				query = query.Where(c => c.callStatusId == callStatusId.Value);
			}
			if (string.IsNullOrEmpty(providerId) == false)
			{
				query = query.Where(c => c.providerId == providerId);
			}
			if (string.IsNullOrEmpty(providerCallId) == false)
			{
				query = query.Where(c => c.providerCallId == providerCallId);
			}
			if (conversationId.HasValue == true)
			{
				query = query.Where(c => c.conversationId == conversationId.Value);
			}
			if (initiatorUserId.HasValue == true)
			{
				query = query.Where(c => c.initiatorUserId == initiatorUserId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(c => c.startDateTime == startDateTime.Value);
			}
			if (answerDateTime.HasValue == true)
			{
				query = query.Where(c => c.answerDateTime == answerDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(c => c.endDateTime == endDateTime.Value);
			}
			if (durationSeconds.HasValue == true)
			{
				query = query.Where(c => c.durationSeconds == durationSeconds.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}

			query = query.OrderBy(c => c.providerId).ThenBy(c => c.providerCallId);


			//
			// Add the any string contains parameter to span all the string fields on the Call, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerId.Contains(anyStringContains)
			       || x.providerCallId.Contains(anyStringContains)
			       || (includeRelations == true && x.callStatus.name.Contains(anyStringContains))
			       || (includeRelations == true && x.callStatus.description.Contains(anyStringContains))
			       || (includeRelations == true && x.callType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.callType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.conversation.entity.Contains(anyStringContains))
			       || (includeRelations == true && x.conversation.externalURL.Contains(anyStringContains))
			       || (includeRelations == true && x.conversation.name.Contains(anyStringContains))
			       || (includeRelations == true && x.conversation.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.callStatus);
				query = query.Include(x => x.callType);
				query = query.Include(x => x.conversation);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.Call> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Call call in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(call, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Call Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Call Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Calls filtered by the parameters provided.  Its query is similar to the GetCalls method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Calls/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? callTypeId = null,
			int? callStatusId = null,
			string providerId = null,
			string providerCallId = null,
			int? conversationId = null,
			int? initiatorUserId = null,
			DateTime? startDateTime = null,
			DateTime? answerDateTime = null,
			DateTime? endDateTime = null,
			int? durationSeconds = null,
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
			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (answerDateTime.HasValue == true && answerDateTime.Value.Kind != DateTimeKind.Utc)
			{
				answerDateTime = answerDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.Call> query = (from c in _context.Calls select c);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (callTypeId.HasValue == true)
			{
				query = query.Where(c => c.callTypeId == callTypeId.Value);
			}
			if (callStatusId.HasValue == true)
			{
				query = query.Where(c => c.callStatusId == callStatusId.Value);
			}
			if (providerId != null)
			{
				query = query.Where(c => c.providerId == providerId);
			}
			if (providerCallId != null)
			{
				query = query.Where(c => c.providerCallId == providerCallId);
			}
			if (conversationId.HasValue == true)
			{
				query = query.Where(c => c.conversationId == conversationId.Value);
			}
			if (initiatorUserId.HasValue == true)
			{
				query = query.Where(c => c.initiatorUserId == initiatorUserId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(c => c.startDateTime == startDateTime.Value);
			}
			if (answerDateTime.HasValue == true)
			{
				query = query.Where(c => c.answerDateTime == answerDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(c => c.endDateTime == endDateTime.Value);
			}
			if (durationSeconds.HasValue == true)
			{
				query = query.Where(c => c.durationSeconds == durationSeconds.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Call, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerId.Contains(anyStringContains)
			       || x.providerCallId.Contains(anyStringContains)
			       || x.callStatus.name.Contains(anyStringContains)
			       || x.callStatus.description.Contains(anyStringContains)
			       || x.callType.name.Contains(anyStringContains)
			       || x.callType.description.Contains(anyStringContains)
			       || x.conversation.entity.Contains(anyStringContains)
			       || x.conversation.externalURL.Contains(anyStringContains)
			       || x.conversation.name.Contains(anyStringContains)
			       || x.conversation.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Call by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Call/{id}")]
		public async Task<IActionResult> GetCall(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Call> query = (from c in _context.Calls where
							(c.id == id) &&
							(userIsAdmin == true || c.deleted == false) &&
							(userIsWriter == true || c.active == true)
					select c);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.callStatus);
					query = query.Include(x => x.callType);
					query = query.Include(x => x.conversation);
					query = query.AsSplitQuery();
				}

				Database.Call materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Call Entity was read with Admin privilege." : "Scheduler.Call Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Call", materialized.id, materialized.providerId));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Call entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Call.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Call.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Call record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Call/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutCall(int id, [FromBody]Database.Call.CallDTO callDTO, CancellationToken cancellationToken = default)
		{
			if (callDTO == null)
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



			if (id != callDTO.id)
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


			IQueryable<Database.Call> query = (from x in _context.Calls
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Call existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Call PUT", id.ToString(), new Exception("No Scheduler.Call entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (callDTO.objectGuid == Guid.Empty)
            {
                callDTO.objectGuid = existing.objectGuid;
            }
            else if (callDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Call record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Call cloneOfExisting = (Database.Call)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Call object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Call call = (Database.Call)_context.Entry(existing).GetDatabaseValues().ToObject();
			call.ApplyDTO(callDTO);
			//
			// The tenant guid for any Call being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Call because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				call.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (call.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Call record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (call.providerId != null && call.providerId.Length > 50)
			{
				call.providerId = call.providerId.Substring(0, 50);
			}

			if (call.providerCallId != null && call.providerCallId.Length > 250)
			{
				call.providerCallId = call.providerCallId.Substring(0, 250);
			}

			if (call.startDateTime.Kind != DateTimeKind.Utc)
			{
				call.startDateTime = call.startDateTime.ToUniversalTime();
			}

			if (call.answerDateTime.HasValue == true && call.answerDateTime.Value.Kind != DateTimeKind.Utc)
			{
				call.answerDateTime = call.answerDateTime.Value.ToUniversalTime();
			}

			if (call.endDateTime.HasValue == true && call.endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				call.endDateTime = call.endDateTime.Value.ToUniversalTime();
			}

			EntityEntry<Database.Call> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(call);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.Call entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.Call.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Call.CreateAnonymousWithFirstLevelSubObjects(call)),
					null);


				return Ok(Database.Call.CreateAnonymous(call));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.Call entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.Call.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Call.CreateAnonymousWithFirstLevelSubObjects(call)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new Call record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Call", Name = "Call")]
		public async Task<IActionResult> PostCall([FromBody]Database.Call.CallDTO callDTO, CancellationToken cancellationToken = default)
		{
			if (callDTO == null)
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
			// Create a new Call object using the data from the DTO
			//
			Database.Call call = Database.Call.FromDTO(callDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				call.tenantGuid = userTenantGuid;

				if (call.providerId != null && call.providerId.Length > 50)
				{
					call.providerId = call.providerId.Substring(0, 50);
				}

				if (call.providerCallId != null && call.providerCallId.Length > 250)
				{
					call.providerCallId = call.providerCallId.Substring(0, 250);
				}

				if (call.startDateTime.Kind != DateTimeKind.Utc)
				{
					call.startDateTime = call.startDateTime.ToUniversalTime();
				}

				if (call.answerDateTime.HasValue == true && call.answerDateTime.Value.Kind != DateTimeKind.Utc)
				{
					call.answerDateTime = call.answerDateTime.Value.ToUniversalTime();
				}

				if (call.endDateTime.HasValue == true && call.endDateTime.Value.Kind != DateTimeKind.Utc)
				{
					call.endDateTime = call.endDateTime.Value.ToUniversalTime();
				}

				call.objectGuid = Guid.NewGuid();
				_context.Calls.Add(call);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.Call entity successfully created.",
					true,
					call.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.Call.CreateAnonymousWithFirstLevelSubObjects(call)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Call entity creation failed.", false, call.id.ToString(), "", JsonSerializer.Serialize(call), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Call", call.id, call.providerId));

			return CreatedAtRoute("Call", new { id = call.id }, Database.Call.CreateAnonymousWithFirstLevelSubObjects(call));
		}



        /// <summary>
        /// 
        /// This deletes a Call record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Call/{id}")]
		[Route("api/Call")]
		public async Task<IActionResult> DeleteCall(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Call> query = (from x in _context.Calls
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Call call = await query.FirstOrDefaultAsync(cancellationToken);

			if (call == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Call DELETE", id.ToString(), new Exception("No Scheduler.Call entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Call cloneOfExisting = (Database.Call)_context.Entry(call).GetDatabaseValues().ToObject();


			try
			{
				call.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.Call entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.Call.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Call.CreateAnonymousWithFirstLevelSubObjects(call)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.Call entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.Call.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Call.CreateAnonymousWithFirstLevelSubObjects(call)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of Call records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Calls/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? callTypeId = null,
			int? callStatusId = null,
			string providerId = null,
			string providerCallId = null,
			int? conversationId = null,
			int? initiatorUserId = null,
			DateTime? startDateTime = null,
			DateTime? answerDateTime = null,
			DateTime? endDateTime = null,
			int? durationSeconds = null,
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
			if (startDateTime.HasValue == true && startDateTime.Value.Kind != DateTimeKind.Utc)
			{
				startDateTime = startDateTime.Value.ToUniversalTime();
			}

			if (answerDateTime.HasValue == true && answerDateTime.Value.Kind != DateTimeKind.Utc)
			{
				answerDateTime = answerDateTime.Value.ToUniversalTime();
			}

			if (endDateTime.HasValue == true && endDateTime.Value.Kind != DateTimeKind.Utc)
			{
				endDateTime = endDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.Call> query = (from c in _context.Calls select c);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (callTypeId.HasValue == true)
			{
				query = query.Where(c => c.callTypeId == callTypeId.Value);
			}
			if (callStatusId.HasValue == true)
			{
				query = query.Where(c => c.callStatusId == callStatusId.Value);
			}
			if (string.IsNullOrEmpty(providerId) == false)
			{
				query = query.Where(c => c.providerId == providerId);
			}
			if (string.IsNullOrEmpty(providerCallId) == false)
			{
				query = query.Where(c => c.providerCallId == providerCallId);
			}
			if (conversationId.HasValue == true)
			{
				query = query.Where(c => c.conversationId == conversationId.Value);
			}
			if (initiatorUserId.HasValue == true)
			{
				query = query.Where(c => c.initiatorUserId == initiatorUserId.Value);
			}
			if (startDateTime.HasValue == true)
			{
				query = query.Where(c => c.startDateTime == startDateTime.Value);
			}
			if (answerDateTime.HasValue == true)
			{
				query = query.Where(c => c.answerDateTime == answerDateTime.Value);
			}
			if (endDateTime.HasValue == true)
			{
				query = query.Where(c => c.endDateTime == endDateTime.Value);
			}
			if (durationSeconds.HasValue == true)
			{
				query = query.Where(c => c.durationSeconds == durationSeconds.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Call, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.providerId.Contains(anyStringContains)
			       || x.providerCallId.Contains(anyStringContains)
			       || x.callStatus.name.Contains(anyStringContains)
			       || x.callStatus.description.Contains(anyStringContains)
			       || x.callType.name.Contains(anyStringContains)
			       || x.callType.description.Contains(anyStringContains)
			       || x.conversation.entity.Contains(anyStringContains)
			       || x.conversation.externalURL.Contains(anyStringContains)
			       || x.conversation.name.Contains(anyStringContains)
			       || x.conversation.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.providerId).ThenBy(x => x.providerCallId);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Call.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Call/CreateAuditEvent")]
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
