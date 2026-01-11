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
    /// This auto generated class provides the basic CRUD operations for the RecurrenceException entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the RecurrenceException entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class RecurrenceExceptionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object recurrenceExceptionPutSyncRoot = new object();
		static object recurrenceExceptionDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<RecurrenceExceptionsController> _logger;

		public RecurrenceExceptionsController(SchedulerContext context, ILogger<RecurrenceExceptionsController> logger) : base("Scheduler", "RecurrenceException")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of RecurrenceExceptions filtered by the parameters provided.
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
		[Route("api/RecurrenceExceptions")]
		public async Task<IActionResult> GetRecurrenceExceptions(
			int? scheduledEventId = null,
			DateTime? exceptionDateTime = null,
			DateTime? movedToDateTime = null,
			string reason = null,
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

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (exceptionDateTime.HasValue == true && exceptionDateTime.Value.Kind != DateTimeKind.Utc)
			{
				exceptionDateTime = exceptionDateTime.Value.ToUniversalTime();
			}

			if (movedToDateTime.HasValue == true && movedToDateTime.Value.Kind != DateTimeKind.Utc)
			{
				movedToDateTime = movedToDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.RecurrenceException> query = (from re in _context.RecurrenceExceptions select re);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(re => re.scheduledEventId == scheduledEventId.Value);
			}
			if (exceptionDateTime.HasValue == true)
			{
				query = query.Where(re => re.exceptionDateTime == exceptionDateTime.Value);
			}
			if (movedToDateTime.HasValue == true)
			{
				query = query.Where(re => re.movedToDateTime == movedToDateTime.Value);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(re => re.reason == reason);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(re => re.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(re => re.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(re => re.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(re => re.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(re => re.deleted == false);
				}
			}
			else
			{
				query = query.Where(re => re.active == true);
				query = query.Where(re => re.deleted == false);
			}

			query = query.OrderBy(re => re.reason);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.scheduledEvent);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Recurrence Exception, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || (includeRelations == true && x.scheduledEvent.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.description.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.location.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.color.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.attributes.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.RecurrenceException> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.RecurrenceException recurrenceException in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(recurrenceException, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.RecurrenceException Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.RecurrenceException Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of RecurrenceExceptions filtered by the parameters provided.  Its query is similar to the GetRecurrenceExceptions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RecurrenceExceptions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? scheduledEventId = null,
			DateTime? exceptionDateTime = null,
			DateTime? movedToDateTime = null,
			string reason = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			// Fix any non-UTC date parameters that come in.
			//
			if (exceptionDateTime.HasValue == true && exceptionDateTime.Value.Kind != DateTimeKind.Utc)
			{
				exceptionDateTime = exceptionDateTime.Value.ToUniversalTime();
			}

			if (movedToDateTime.HasValue == true && movedToDateTime.Value.Kind != DateTimeKind.Utc)
			{
				movedToDateTime = movedToDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.RecurrenceException> query = (from re in _context.RecurrenceExceptions select re);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(re => re.scheduledEventId == scheduledEventId.Value);
			}
			if (exceptionDateTime.HasValue == true)
			{
				query = query.Where(re => re.exceptionDateTime == exceptionDateTime.Value);
			}
			if (movedToDateTime.HasValue == true)
			{
				query = query.Where(re => re.movedToDateTime == movedToDateTime.Value);
			}
			if (reason != null)
			{
				query = query.Where(re => re.reason == reason);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(re => re.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(re => re.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(re => re.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(re => re.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(re => re.deleted == false);
				}
			}
			else
			{
				query = query.Where(re => re.active == true);
				query = query.Where(re => re.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Recurrence Exception, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.scheduledEvent.name.Contains(anyStringContains)
			       || x.scheduledEvent.description.Contains(anyStringContains)
			       || x.scheduledEvent.location.Contains(anyStringContains)
			       || x.scheduledEvent.notes.Contains(anyStringContains)
			       || x.scheduledEvent.color.Contains(anyStringContains)
			       || x.scheduledEvent.externalId.Contains(anyStringContains)
			       || x.scheduledEvent.attributes.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single RecurrenceException by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RecurrenceException/{id}")]
		public async Task<IActionResult> GetRecurrenceException(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.RecurrenceException> query = (from re in _context.RecurrenceExceptions where
							(re.id == id) &&
							(userIsAdmin == true || re.deleted == false) &&
							(userIsWriter == true || re.active == true)
					select re);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.scheduledEvent);
					query = query.AsSplitQuery();
				}

				Database.RecurrenceException materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.RecurrenceException Entity was read with Admin privilege." : "Scheduler.RecurrenceException Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RecurrenceException", materialized.id, materialized.reason));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.RecurrenceException entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.RecurrenceException.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.RecurrenceException.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing RecurrenceException record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/RecurrenceException/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutRecurrenceException(int id, [FromBody]Database.RecurrenceException.RecurrenceExceptionDTO recurrenceExceptionDTO, CancellationToken cancellationToken = default)
		{
			if (recurrenceExceptionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != recurrenceExceptionDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.RecurrenceException> query = (from x in _context.RecurrenceExceptions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.RecurrenceException existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.RecurrenceException PUT", id.ToString(), new Exception("No Scheduler.RecurrenceException entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (recurrenceExceptionDTO.objectGuid == Guid.Empty)
            {
                recurrenceExceptionDTO.objectGuid = existing.objectGuid;
            }
            else if (recurrenceExceptionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a RecurrenceException record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.RecurrenceException cloneOfExisting = (Database.RecurrenceException)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new RecurrenceException object using the data from the existing record, updated with what is in the DTO.
			//
			Database.RecurrenceException recurrenceException = (Database.RecurrenceException)_context.Entry(existing).GetDatabaseValues().ToObject();
			recurrenceException.ApplyDTO(recurrenceExceptionDTO);
			//
			// The tenant guid for any RecurrenceException being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the RecurrenceException because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				recurrenceException.tenantGuid = existing.tenantGuid;
			}

			lock (recurrenceExceptionPutSyncRoot)
			{
				//
				// Validate the version number for the recurrenceException being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != recurrenceException.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "RecurrenceException save attempt was made but save request was with version " + recurrenceException.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The RecurrenceException you are trying to update has already changed.  Please try your save again after reloading the RecurrenceException.");
				}
				else
				{
					// Same record.  Increase version.
					recurrenceException.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (recurrenceException.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.RecurrenceException record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (recurrenceException.exceptionDateTime.Kind != DateTimeKind.Utc)
				{
					recurrenceException.exceptionDateTime = recurrenceException.exceptionDateTime.ToUniversalTime();
				}

				if (recurrenceException.movedToDateTime.HasValue == true && recurrenceException.movedToDateTime.Value.Kind != DateTimeKind.Utc)
				{
					recurrenceException.movedToDateTime = recurrenceException.movedToDateTime.Value.ToUniversalTime();
				}

				if (recurrenceException.reason != null && recurrenceException.reason.Length > 250)
				{
					recurrenceException.reason = recurrenceException.reason.Substring(0, 250);
				}

				try
				{
				    EntityEntry<Database.RecurrenceException> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(recurrenceException);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        RecurrenceExceptionChangeHistory recurrenceExceptionChangeHistory = new RecurrenceExceptionChangeHistory();
				        recurrenceExceptionChangeHistory.recurrenceExceptionId = recurrenceException.id;
				        recurrenceExceptionChangeHistory.versionNumber = recurrenceException.versionNumber;
				        recurrenceExceptionChangeHistory.timeStamp = DateTime.UtcNow;
				        recurrenceExceptionChangeHistory.userId = securityUser.id;
				        recurrenceExceptionChangeHistory.tenantGuid = userTenantGuid;
				        recurrenceExceptionChangeHistory.data = JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException));
				        _context.RecurrenceExceptionChangeHistories.Add(recurrenceExceptionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.RecurrenceException entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException)),
						null);

				return Ok(Database.RecurrenceException.CreateAnonymous(recurrenceException));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.RecurrenceException entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new RecurrenceException record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RecurrenceException", Name = "RecurrenceException")]
		public async Task<IActionResult> PostRecurrenceException([FromBody]Database.RecurrenceException.RecurrenceExceptionDTO recurrenceExceptionDTO, CancellationToken cancellationToken = default)
		{
			if (recurrenceExceptionDTO == null)
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
			// Create a new RecurrenceException object using the data from the DTO
			//
			Database.RecurrenceException recurrenceException = Database.RecurrenceException.FromDTO(recurrenceExceptionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				recurrenceException.tenantGuid = userTenantGuid;

				if (recurrenceException.exceptionDateTime.Kind != DateTimeKind.Utc)
				{
					recurrenceException.exceptionDateTime = recurrenceException.exceptionDateTime.ToUniversalTime();
				}

				if (recurrenceException.movedToDateTime.HasValue == true && recurrenceException.movedToDateTime.Value.Kind != DateTimeKind.Utc)
				{
					recurrenceException.movedToDateTime = recurrenceException.movedToDateTime.Value.ToUniversalTime();
				}

				if (recurrenceException.reason != null && recurrenceException.reason.Length > 250)
				{
					recurrenceException.reason = recurrenceException.reason.Substring(0, 250);
				}

				recurrenceException.objectGuid = Guid.NewGuid();
				recurrenceException.versionNumber = 1;

				_context.RecurrenceExceptions.Add(recurrenceException);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the recurrenceException object so that no further changes will be written to the database
				    //
				    _context.Entry(recurrenceException).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					recurrenceException.RecurrenceExceptionChangeHistories = null;
					recurrenceException.scheduledEvent = null;


				    RecurrenceExceptionChangeHistory recurrenceExceptionChangeHistory = new RecurrenceExceptionChangeHistory();
				    recurrenceExceptionChangeHistory.recurrenceExceptionId = recurrenceException.id;
				    recurrenceExceptionChangeHistory.versionNumber = recurrenceException.versionNumber;
				    recurrenceExceptionChangeHistory.timeStamp = DateTime.UtcNow;
				    recurrenceExceptionChangeHistory.userId = securityUser.id;
				    recurrenceExceptionChangeHistory.tenantGuid = userTenantGuid;
				    recurrenceExceptionChangeHistory.data = JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException));
				    _context.RecurrenceExceptionChangeHistories.Add(recurrenceExceptionChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.RecurrenceException entity successfully created.",
						true,
						recurrenceException. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.RecurrenceException entity creation failed.", false, recurrenceException.id.ToString(), "", JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RecurrenceException", recurrenceException.id, recurrenceException.reason));

			return CreatedAtRoute("RecurrenceException", new { id = recurrenceException.id }, Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException));
		}



        /// <summary>
        /// 
        /// This rolls a RecurrenceException entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RecurrenceException/Rollback/{id}")]
		[Route("api/RecurrenceException/Rollback")]
		public async Task<IActionResult> RollbackToRecurrenceExceptionVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.RecurrenceException> query = (from x in _context.RecurrenceExceptions
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this RecurrenceException concurrently
			//
			lock (recurrenceExceptionPutSyncRoot)
			{
				
				Database.RecurrenceException recurrenceException = query.FirstOrDefault();
				
				if (recurrenceException == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.RecurrenceException rollback", id.ToString(), new Exception("No Scheduler.RecurrenceException entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the RecurrenceException current state so we can log it.
				//
				Database.RecurrenceException cloneOfExisting = (Database.RecurrenceException)_context.Entry(recurrenceException).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.RecurrenceExceptionChangeHistories = null;
				cloneOfExisting.scheduledEvent = null;

				if (versionNumber >= recurrenceException.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.RecurrenceException rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.RecurrenceException rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				RecurrenceExceptionChangeHistory recurrenceExceptionChangeHistory = (from x in _context.RecurrenceExceptionChangeHistories
				                                               where
				                                               x.recurrenceExceptionId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (recurrenceExceptionChangeHistory != null)
				{
				    Database.RecurrenceException oldRecurrenceException = JsonSerializer.Deserialize<Database.RecurrenceException>(recurrenceExceptionChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    recurrenceException.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    recurrenceException.scheduledEventId = oldRecurrenceException.scheduledEventId;
				    recurrenceException.exceptionDateTime = oldRecurrenceException.exceptionDateTime;
				    recurrenceException.movedToDateTime = oldRecurrenceException.movedToDateTime;
				    recurrenceException.reason = oldRecurrenceException.reason;
				    recurrenceException.objectGuid = oldRecurrenceException.objectGuid;
				    recurrenceException.active = oldRecurrenceException.active;
				    recurrenceException.deleted = oldRecurrenceException.deleted;

				    string serializedRecurrenceException = JsonSerializer.Serialize(recurrenceException);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        RecurrenceExceptionChangeHistory newRecurrenceExceptionChangeHistory = new RecurrenceExceptionChangeHistory();
				        newRecurrenceExceptionChangeHistory.recurrenceExceptionId = recurrenceException.id;
				        newRecurrenceExceptionChangeHistory.versionNumber = recurrenceException.versionNumber;
				        newRecurrenceExceptionChangeHistory.timeStamp = DateTime.UtcNow;
				        newRecurrenceExceptionChangeHistory.userId = securityUser.id;
				        newRecurrenceExceptionChangeHistory.tenantGuid = userTenantGuid;
				        newRecurrenceExceptionChangeHistory.data = JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException));
				        _context.RecurrenceExceptionChangeHistories.Add(newRecurrenceExceptionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.RecurrenceException rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException)),
						null);


				    return Ok(Database.RecurrenceException.CreateAnonymous(recurrenceException));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.RecurrenceException rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.RecurrenceException rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}


        /// <summary>
        /// 
        /// This deletes a RecurrenceException record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RecurrenceException/{id}")]
		[Route("api/RecurrenceException")]
		public async Task<IActionResult> DeleteRecurrenceException(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.RecurrenceException> query = (from x in _context.RecurrenceExceptions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.RecurrenceException recurrenceException = await query.FirstOrDefaultAsync(cancellationToken);

			if (recurrenceException == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.RecurrenceException DELETE", id.ToString(), new Exception("No Scheduler.RecurrenceException entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.RecurrenceException cloneOfExisting = (Database.RecurrenceException)_context.Entry(recurrenceException).GetDatabaseValues().ToObject();


			lock (recurrenceExceptionDeleteSyncRoot)
			{
			    try
			    {
			        recurrenceException.deleted = true;
			        recurrenceException.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        RecurrenceExceptionChangeHistory recurrenceExceptionChangeHistory = new RecurrenceExceptionChangeHistory();
			        recurrenceExceptionChangeHistory.recurrenceExceptionId = recurrenceException.id;
			        recurrenceExceptionChangeHistory.versionNumber = recurrenceException.versionNumber;
			        recurrenceExceptionChangeHistory.timeStamp = DateTime.UtcNow;
			        recurrenceExceptionChangeHistory.userId = securityUser.id;
			        recurrenceExceptionChangeHistory.tenantGuid = userTenantGuid;
			        recurrenceExceptionChangeHistory.data = JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException));
			        _context.RecurrenceExceptionChangeHistories.Add(recurrenceExceptionChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.RecurrenceException entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.RecurrenceException entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.RecurrenceException.CreateAnonymousWithFirstLevelSubObjects(recurrenceException)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of RecurrenceException records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/RecurrenceExceptions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? scheduledEventId = null,
			DateTime? exceptionDateTime = null,
			DateTime? movedToDateTime = null,
			string reason = null,
			int? versionNumber = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);

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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (exceptionDateTime.HasValue == true && exceptionDateTime.Value.Kind != DateTimeKind.Utc)
			{
				exceptionDateTime = exceptionDateTime.Value.ToUniversalTime();
			}

			if (movedToDateTime.HasValue == true && movedToDateTime.Value.Kind != DateTimeKind.Utc)
			{
				movedToDateTime = movedToDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.RecurrenceException> query = (from re in _context.RecurrenceExceptions select re);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(re => re.scheduledEventId == scheduledEventId.Value);
			}
			if (exceptionDateTime.HasValue == true)
			{
				query = query.Where(re => re.exceptionDateTime == exceptionDateTime.Value);
			}
			if (movedToDateTime.HasValue == true)
			{
				query = query.Where(re => re.movedToDateTime == movedToDateTime.Value);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(re => re.reason == reason);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(re => re.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(re => re.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(re => re.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(re => re.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(re => re.deleted == false);
				}
			}
			else
			{
				query = query.Where(re => re.active == true);
				query = query.Where(re => re.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Recurrence Exception, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.scheduledEvent.name.Contains(anyStringContains)
			       || x.scheduledEvent.description.Contains(anyStringContains)
			       || x.scheduledEvent.location.Contains(anyStringContains)
			       || x.scheduledEvent.notes.Contains(anyStringContains)
			       || x.scheduledEvent.color.Contains(anyStringContains)
			       || x.scheduledEvent.externalId.Contains(anyStringContains)
			       || x.scheduledEvent.attributes.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.reason);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.RecurrenceException.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/RecurrenceException/CreateAuditEvent")]
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
