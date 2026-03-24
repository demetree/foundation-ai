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
    /// This auto generated class provides the basic CRUD operations for the CallEventLog entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the CallEventLog entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class CallEventLogsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		private SchedulerContext _context;

		private ILogger<CallEventLogsController> _logger;

		public CallEventLogsController(SchedulerContext context, ILogger<CallEventLogsController> logger) : base("Scheduler", "CallEventLog")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of CallEventLogs filtered by the parameters provided.
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
		[Route("api/CallEventLogs")]
		public async Task<IActionResult> GetCallEventLogs(
			int? callId = null,
			string eventType = null,
			int? userId = null,
			string providerId = null,
			string metadata = null,
			DateTime? dateTimeCreated = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.CallEventLog> query = (from cel in _context.CallEventLogs select cel);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (callId.HasValue == true)
			{
				query = query.Where(cel => cel.callId == callId.Value);
			}
			if (string.IsNullOrEmpty(eventType) == false)
			{
				query = query.Where(cel => cel.eventType == eventType);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cel => cel.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(providerId) == false)
			{
				query = query.Where(cel => cel.providerId == providerId);
			}
			if (string.IsNullOrEmpty(metadata) == false)
			{
				query = query.Where(cel => cel.metadata == metadata);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(cel => cel.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cel => cel.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cel => cel.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cel => cel.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cel => cel.deleted == false);
				}
			}
			else
			{
				query = query.Where(cel => cel.active == true);
				query = query.Where(cel => cel.deleted == false);
			}

			query = query.OrderBy(cel => cel.eventType).ThenBy(cel => cel.providerId);


			//
			// Add the any string contains parameter to span all the string fields on the Call Event Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.eventType.Contains(anyStringContains)
			       || x.providerId.Contains(anyStringContains)
			       || x.metadata.Contains(anyStringContains)
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
			
			List<Database.CallEventLog> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.CallEventLog callEventLog in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(callEventLog, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.CallEventLog Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.CallEventLog Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of CallEventLogs filtered by the parameters provided.  Its query is similar to the GetCallEventLogs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CallEventLogs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? callId = null,
			string eventType = null,
			int? userId = null,
			string providerId = null,
			string metadata = null,
			DateTime? dateTimeCreated = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.CallEventLog> query = (from cel in _context.CallEventLogs select cel);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (callId.HasValue == true)
			{
				query = query.Where(cel => cel.callId == callId.Value);
			}
			if (eventType != null)
			{
				query = query.Where(cel => cel.eventType == eventType);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cel => cel.userId == userId.Value);
			}
			if (providerId != null)
			{
				query = query.Where(cel => cel.providerId == providerId);
			}
			if (metadata != null)
			{
				query = query.Where(cel => cel.metadata == metadata);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(cel => cel.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cel => cel.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cel => cel.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cel => cel.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cel => cel.deleted == false);
				}
			}
			else
			{
				query = query.Where(cel => cel.active == true);
				query = query.Where(cel => cel.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Call Event Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.eventType.Contains(anyStringContains)
			       || x.providerId.Contains(anyStringContains)
			       || x.metadata.Contains(anyStringContains)
			       || x.call.providerId.Contains(anyStringContains)
			       || x.call.providerCallId.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single CallEventLog by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CallEventLog/{id}")]
		public async Task<IActionResult> GetCallEventLog(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
				IQueryable<Database.CallEventLog> query = (from cel in _context.CallEventLogs where
							(cel.id == id) &&
							(userIsAdmin == true || cel.deleted == false) &&
							(userIsWriter == true || cel.active == true)
					select cel);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.call);
					query = query.AsSplitQuery();
				}

				Database.CallEventLog materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.CallEventLog Entity was read with Admin privilege." : "Scheduler.CallEventLog Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "CallEventLog", materialized.id, materialized.eventType));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.CallEventLog entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.CallEventLog.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.CallEventLog.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing CallEventLog record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/CallEventLog/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutCallEventLog(int id, [FromBody]Database.CallEventLog.CallEventLogDTO callEventLogDTO, CancellationToken cancellationToken = default)
		{
			if (callEventLogDTO == null)
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



			if (id != callEventLogDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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


			IQueryable<Database.CallEventLog> query = (from x in _context.CallEventLogs
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.CallEventLog existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.CallEventLog PUT", id.ToString(), new Exception("No Scheduler.CallEventLog entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (callEventLogDTO.objectGuid == Guid.Empty)
            {
                callEventLogDTO.objectGuid = existing.objectGuid;
            }
            else if (callEventLogDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a CallEventLog record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.CallEventLog cloneOfExisting = (Database.CallEventLog)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new CallEventLog object using the data from the existing record, updated with what is in the DTO.
			//
			Database.CallEventLog callEventLog = (Database.CallEventLog)_context.Entry(existing).GetDatabaseValues().ToObject();
			callEventLog.ApplyDTO(callEventLogDTO);
			//
			// The tenant guid for any CallEventLog being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the CallEventLog because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				callEventLog.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (callEventLog.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.CallEventLog record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (callEventLog.eventType != null && callEventLog.eventType.Length > 100)
			{
				callEventLog.eventType = callEventLog.eventType.Substring(0, 100);
			}

			if (callEventLog.providerId != null && callEventLog.providerId.Length > 50)
			{
				callEventLog.providerId = callEventLog.providerId.Substring(0, 50);
			}

			if (callEventLog.dateTimeCreated.Kind != DateTimeKind.Utc)
			{
				callEventLog.dateTimeCreated = callEventLog.dateTimeCreated.ToUniversalTime();
			}

			EntityEntry<Database.CallEventLog> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(callEventLog);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.CallEventLog entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.CallEventLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CallEventLog.CreateAnonymousWithFirstLevelSubObjects(callEventLog)),
					null);


				return Ok(Database.CallEventLog.CreateAnonymous(callEventLog));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.CallEventLog entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.CallEventLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CallEventLog.CreateAnonymousWithFirstLevelSubObjects(callEventLog)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new CallEventLog record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CallEventLog", Name = "CallEventLog")]
		public async Task<IActionResult> PostCallEventLog([FromBody]Database.CallEventLog.CallEventLogDTO callEventLogDTO, CancellationToken cancellationToken = default)
		{
			if (callEventLogDTO == null)
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
			// Create a new CallEventLog object using the data from the DTO
			//
			Database.CallEventLog callEventLog = Database.CallEventLog.FromDTO(callEventLogDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				callEventLog.tenantGuid = userTenantGuid;

				if (callEventLog.eventType != null && callEventLog.eventType.Length > 100)
				{
					callEventLog.eventType = callEventLog.eventType.Substring(0, 100);
				}

				if (callEventLog.providerId != null && callEventLog.providerId.Length > 50)
				{
					callEventLog.providerId = callEventLog.providerId.Substring(0, 50);
				}

				if (callEventLog.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					callEventLog.dateTimeCreated = callEventLog.dateTimeCreated.ToUniversalTime();
				}

				callEventLog.objectGuid = Guid.NewGuid();
				_context.CallEventLogs.Add(callEventLog);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.CallEventLog entity successfully created.",
					true,
					callEventLog.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.CallEventLog.CreateAnonymousWithFirstLevelSubObjects(callEventLog)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.CallEventLog entity creation failed.", false, callEventLog.id.ToString(), "", JsonSerializer.Serialize(callEventLog), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "CallEventLog", callEventLog.id, callEventLog.eventType));

			return CreatedAtRoute("CallEventLog", new { id = callEventLog.id }, Database.CallEventLog.CreateAnonymousWithFirstLevelSubObjects(callEventLog));
		}



        /// <summary>
        /// 
        /// This deletes a CallEventLog record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CallEventLog/{id}")]
		[Route("api/CallEventLog")]
		public async Task<IActionResult> DeleteCallEventLog(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.CallEventLog> query = (from x in _context.CallEventLogs
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.CallEventLog callEventLog = await query.FirstOrDefaultAsync(cancellationToken);

			if (callEventLog == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.CallEventLog DELETE", id.ToString(), new Exception("No Scheduler.CallEventLog entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.CallEventLog cloneOfExisting = (Database.CallEventLog)_context.Entry(callEventLog).GetDatabaseValues().ToObject();


			try
			{
				callEventLog.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.CallEventLog entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.CallEventLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CallEventLog.CreateAnonymousWithFirstLevelSubObjects(callEventLog)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.CallEventLog entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.CallEventLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.CallEventLog.CreateAnonymousWithFirstLevelSubObjects(callEventLog)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of CallEventLog records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/CallEventLogs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? callId = null,
			string eventType = null,
			int? userId = null,
			string providerId = null,
			string metadata = null,
			DateTime? dateTimeCreated = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);


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
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.CallEventLog> query = (from cel in _context.CallEventLogs select cel);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (callId.HasValue == true)
			{
				query = query.Where(cel => cel.callId == callId.Value);
			}
			if (string.IsNullOrEmpty(eventType) == false)
			{
				query = query.Where(cel => cel.eventType == eventType);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cel => cel.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(providerId) == false)
			{
				query = query.Where(cel => cel.providerId == providerId);
			}
			if (string.IsNullOrEmpty(metadata) == false)
			{
				query = query.Where(cel => cel.metadata == metadata);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(cel => cel.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cel => cel.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cel => cel.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cel => cel.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cel => cel.deleted == false);
				}
			}
			else
			{
				query = query.Where(cel => cel.active == true);
				query = query.Where(cel => cel.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Call Event Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.eventType.Contains(anyStringContains)
			       || x.providerId.Contains(anyStringContains)
			       || x.metadata.Contains(anyStringContains)
			       || x.call.providerId.Contains(anyStringContains)
			       || x.call.providerCallId.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.eventType).ThenBy(x => x.providerId);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.CallEventLog.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/CallEventLog/CreateAuditEvent")]
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
