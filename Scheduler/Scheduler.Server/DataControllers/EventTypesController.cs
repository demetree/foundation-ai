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
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the EventType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the EventType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class EventTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object eventTypePutSyncRoot = new object();
		static object eventTypeDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<EventTypesController> _logger;

		public EventTypesController(SchedulerContext context, ILogger<EventTypesController> logger) : base("Scheduler", "EventType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of EventTypes filtered by the parameters provided.
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
		[Route("api/EventTypes")]
		public async Task<IActionResult> GetEventTypes(
			string name = null,
			string description = null,
			string color = null,
			int? iconId = null,
			int? sequence = null,
			bool? requiresRentalAgreement = null,
			bool? requiresExternalContact = null,
			bool? requiresPayment = null,
			bool? requiresDeposit = null,
			bool? requiresBarService = null,
			bool? allowsTicketSales = null,
			bool? isInternalEvent = null,
			decimal? defaultPrice = null,
			int? chargeTypeId = null,
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

			IQueryable<Database.EventType> query = (from et in _context.EventTypes select et);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(et => et.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(et => et.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(et => et.color == color);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(et => et.iconId == iconId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(et => et.sequence == sequence.Value);
			}
			if (requiresRentalAgreement.HasValue == true)
			{
				query = query.Where(et => et.requiresRentalAgreement == requiresRentalAgreement.Value);
			}
			if (requiresExternalContact.HasValue == true)
			{
				query = query.Where(et => et.requiresExternalContact == requiresExternalContact.Value);
			}
			if (requiresPayment.HasValue == true)
			{
				query = query.Where(et => et.requiresPayment == requiresPayment.Value);
			}
			if (requiresDeposit.HasValue == true)
			{
				query = query.Where(et => et.requiresDeposit == requiresDeposit.Value);
			}
			if (requiresBarService.HasValue == true)
			{
				query = query.Where(et => et.requiresBarService == requiresBarService.Value);
			}
			if (allowsTicketSales.HasValue == true)
			{
				query = query.Where(et => et.allowsTicketSales == allowsTicketSales.Value);
			}
			if (isInternalEvent.HasValue == true)
			{
				query = query.Where(et => et.isInternalEvent == isInternalEvent.Value);
			}
			if (defaultPrice.HasValue == true)
			{
				query = query.Where(et => et.defaultPrice == defaultPrice.Value);
			}
			if (chargeTypeId.HasValue == true)
			{
				query = query.Where(et => et.chargeTypeId == chargeTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(et => et.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(et => et.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(et => et.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(et => et.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(et => et.deleted == false);
				}
			}
			else
			{
				query = query.Where(et => et.active == true);
				query = query.Where(et => et.deleted == false);
			}

			query = query.OrderBy(et => et.sequence).ThenBy(et => et.name).ThenBy(et => et.description).ThenBy(et => et.color);


			//
			// Add the any string contains parameter to span all the string fields on the Event Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || (includeRelations == true && x.chargeType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.defaultDescription.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.chargeType);
				query = query.Include(x => x.icon);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.EventType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.EventType eventType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(eventType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.EventType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.EventType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of EventTypes filtered by the parameters provided.  Its query is similar to the GetEventTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string color = null,
			int? iconId = null,
			int? sequence = null,
			bool? requiresRentalAgreement = null,
			bool? requiresExternalContact = null,
			bool? requiresPayment = null,
			bool? requiresDeposit = null,
			bool? requiresBarService = null,
			bool? allowsTicketSales = null,
			bool? isInternalEvent = null,
			decimal? defaultPrice = null,
			int? chargeTypeId = null,
			int? versionNumber = null,
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


			IQueryable<Database.EventType> query = (from et in _context.EventTypes select et);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(et => et.name == name);
			}
			if (description != null)
			{
				query = query.Where(et => et.description == description);
			}
			if (color != null)
			{
				query = query.Where(et => et.color == color);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(et => et.iconId == iconId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(et => et.sequence == sequence.Value);
			}
			if (requiresRentalAgreement.HasValue == true)
			{
				query = query.Where(et => et.requiresRentalAgreement == requiresRentalAgreement.Value);
			}
			if (requiresExternalContact.HasValue == true)
			{
				query = query.Where(et => et.requiresExternalContact == requiresExternalContact.Value);
			}
			if (requiresPayment.HasValue == true)
			{
				query = query.Where(et => et.requiresPayment == requiresPayment.Value);
			}
			if (requiresDeposit.HasValue == true)
			{
				query = query.Where(et => et.requiresDeposit == requiresDeposit.Value);
			}
			if (requiresBarService.HasValue == true)
			{
				query = query.Where(et => et.requiresBarService == requiresBarService.Value);
			}
			if (allowsTicketSales.HasValue == true)
			{
				query = query.Where(et => et.allowsTicketSales == allowsTicketSales.Value);
			}
			if (isInternalEvent.HasValue == true)
			{
				query = query.Where(et => et.isInternalEvent == isInternalEvent.Value);
			}
			if (defaultPrice.HasValue == true)
			{
				query = query.Where(et => et.defaultPrice == defaultPrice.Value);
			}
			if (chargeTypeId.HasValue == true)
			{
				query = query.Where(et => et.chargeTypeId == chargeTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(et => et.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(et => et.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(et => et.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(et => et.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(et => et.deleted == false);
				}
			}
			else
			{
				query = query.Where(et => et.active == true);
				query = query.Where(et => et.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Event Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.chargeType.name.Contains(anyStringContains)
			       || x.chargeType.description.Contains(anyStringContains)
			       || x.chargeType.externalId.Contains(anyStringContains)
			       || x.chargeType.defaultDescription.Contains(anyStringContains)
			       || x.chargeType.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single EventType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventType/{id}")]
		public async Task<IActionResult> GetEventType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.EventType> query = (from et in _context.EventTypes where
							(et.id == id) &&
							(userIsAdmin == true || et.deleted == false) &&
							(userIsWriter == true || et.active == true)
					select et);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.chargeType);
					query = query.Include(x => x.icon);
					query = query.AsSplitQuery();
				}

				Database.EventType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.EventType Entity was read with Admin privilege." : "Scheduler.EventType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.EventType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.EventType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.EventType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing EventType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/EventType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutEventType(int id, [FromBody]Database.EventType.EventTypeDTO eventTypeDTO, CancellationToken cancellationToken = default)
		{
			if (eventTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != eventTypeDTO.id)
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


			IQueryable<Database.EventType> query = (from x in _context.EventTypes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventType PUT", id.ToString(), new Exception("No Scheduler.EventType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (eventTypeDTO.objectGuid == Guid.Empty)
            {
                eventTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (eventTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a EventType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.EventType cloneOfExisting = (Database.EventType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new EventType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.EventType eventType = (Database.EventType)_context.Entry(existing).GetDatabaseValues().ToObject();
			eventType.ApplyDTO(eventTypeDTO);
			//
			// The tenant guid for any EventType being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the EventType because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				eventType.tenantGuid = existing.tenantGuid;
			}

			lock (eventTypePutSyncRoot)
			{
				//
				// Validate the version number for the eventType being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != eventType.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "EventType save attempt was made but save request was with version " + eventType.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The EventType you are trying to update has already changed.  Please try your save again after reloading the EventType.");
				}
				else
				{
					// Same record.  Increase version.
					eventType.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (eventType.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.EventType record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (eventType.name != null && eventType.name.Length > 100)
				{
					eventType.name = eventType.name.Substring(0, 100);
				}

				if (eventType.description != null && eventType.description.Length > 500)
				{
					eventType.description = eventType.description.Substring(0, 500);
				}

				if (eventType.color != null && eventType.color.Length > 10)
				{
					eventType.color = eventType.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.EventType> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(eventType);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        EventTypeChangeHistory eventTypeChangeHistory = new EventTypeChangeHistory();
				        eventTypeChangeHistory.eventTypeId = eventType.id;
				        eventTypeChangeHistory.versionNumber = eventType.versionNumber;
				        eventTypeChangeHistory.timeStamp = DateTime.UtcNow;
				        eventTypeChangeHistory.userId = securityUser.id;
				        eventTypeChangeHistory.tenantGuid = userTenantGuid;
				        eventTypeChangeHistory.data = JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType));
				        _context.EventTypeChangeHistories.Add(eventTypeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventType entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType)),
						null);

				return Ok(Database.EventType.CreateAnonymous(eventType));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventType entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new EventType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventType", Name = "EventType")]
		public async Task<IActionResult> PostEventType([FromBody]Database.EventType.EventTypeDTO eventTypeDTO, CancellationToken cancellationToken = default)
		{
			if (eventTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new EventType object using the data from the DTO
			//
			Database.EventType eventType = Database.EventType.FromDTO(eventTypeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				eventType.tenantGuid = userTenantGuid;

				if (eventType.name != null && eventType.name.Length > 100)
				{
					eventType.name = eventType.name.Substring(0, 100);
				}

				if (eventType.description != null && eventType.description.Length > 500)
				{
					eventType.description = eventType.description.Substring(0, 500);
				}

				if (eventType.color != null && eventType.color.Length > 10)
				{
					eventType.color = eventType.color.Substring(0, 10);
				}

				eventType.objectGuid = Guid.NewGuid();
				eventType.versionNumber = 1;

				_context.EventTypes.Add(eventType);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the eventType object so that no further changes will be written to the database
				    //
				    _context.Entry(eventType).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					eventType.EventTypeChangeHistories = null;
					eventType.ScheduledEvents = null;
					eventType.chargeType = null;
					eventType.icon = null;


				    EventTypeChangeHistory eventTypeChangeHistory = new EventTypeChangeHistory();
				    eventTypeChangeHistory.eventTypeId = eventType.id;
				    eventTypeChangeHistory.versionNumber = eventType.versionNumber;
				    eventTypeChangeHistory.timeStamp = DateTime.UtcNow;
				    eventTypeChangeHistory.userId = securityUser.id;
				    eventTypeChangeHistory.tenantGuid = userTenantGuid;
				    eventTypeChangeHistory.data = JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType));
				    _context.EventTypeChangeHistories.Add(eventTypeChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.EventType entity successfully created.",
						true,
						eventType. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.EventType entity creation failed.", false, eventType.id.ToString(), "", JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventType", eventType.id, eventType.name));

			return CreatedAtRoute("EventType", new { id = eventType.id }, Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType));
		}



        /// <summary>
        /// 
        /// This rolls a EventType entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventType/Rollback/{id}")]
		[Route("api/EventType/Rollback")]
		public async Task<IActionResult> RollbackToEventTypeVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.EventType> query = (from x in _context.EventTypes
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this EventType concurrently
			//
			lock (eventTypePutSyncRoot)
			{
				
				Database.EventType eventType = query.FirstOrDefault();
				
				if (eventType == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventType rollback", id.ToString(), new Exception("No Scheduler.EventType entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the EventType current state so we can log it.
				//
				Database.EventType cloneOfExisting = (Database.EventType)_context.Entry(eventType).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.EventTypeChangeHistories = null;
				cloneOfExisting.ScheduledEvents = null;
				cloneOfExisting.chargeType = null;
				cloneOfExisting.icon = null;

				if (versionNumber >= eventType.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.EventType rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.EventType rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				EventTypeChangeHistory eventTypeChangeHistory = (from x in _context.EventTypeChangeHistories
				                                               where
				                                               x.eventTypeId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (eventTypeChangeHistory != null)
				{
				    Database.EventType oldEventType = JsonSerializer.Deserialize<Database.EventType>(eventTypeChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    eventType.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    eventType.name = oldEventType.name;
				    eventType.description = oldEventType.description;
				    eventType.color = oldEventType.color;
				    eventType.iconId = oldEventType.iconId;
				    eventType.sequence = oldEventType.sequence;
				    eventType.requiresRentalAgreement = oldEventType.requiresRentalAgreement;
				    eventType.requiresExternalContact = oldEventType.requiresExternalContact;
				    eventType.requiresPayment = oldEventType.requiresPayment;
				    eventType.requiresDeposit = oldEventType.requiresDeposit;
				    eventType.requiresBarService = oldEventType.requiresBarService;
				    eventType.allowsTicketSales = oldEventType.allowsTicketSales;
				    eventType.isInternalEvent = oldEventType.isInternalEvent;
				    eventType.defaultPrice = oldEventType.defaultPrice;
				    eventType.chargeTypeId = oldEventType.chargeTypeId;
				    eventType.objectGuid = oldEventType.objectGuid;
				    eventType.active = oldEventType.active;
				    eventType.deleted = oldEventType.deleted;

				    string serializedEventType = JsonSerializer.Serialize(eventType);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        EventTypeChangeHistory newEventTypeChangeHistory = new EventTypeChangeHistory();
				        newEventTypeChangeHistory.eventTypeId = eventType.id;
				        newEventTypeChangeHistory.versionNumber = eventType.versionNumber;
				        newEventTypeChangeHistory.timeStamp = DateTime.UtcNow;
				        newEventTypeChangeHistory.userId = securityUser.id;
				        newEventTypeChangeHistory.tenantGuid = userTenantGuid;
				        newEventTypeChangeHistory.data = JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType));
				        _context.EventTypeChangeHistories.Add(newEventTypeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventType rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType)),
						null);


				    return Ok(Database.EventType.CreateAnonymous(eventType));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.EventType rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.EventType rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a EventType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventType</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventType/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetEventTypeChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.EventType eventType = await _context.EventTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventType == null)
			{
				return NotFound();
			}

			try
			{
				eventType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EventType> versionInfo = await eventType.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a EventType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventType</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventType/{id}/AuditHistory")]
		public async Task<IActionResult> GetEventTypeAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.EventType eventType = await _context.EventTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventType == null)
			{
				return NotFound();
			}

			try
			{
				eventType.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.EventType>> versions = await eventType.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a EventType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventType</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The EventType object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventType/{id}/Version/{version}")]
		public async Task<IActionResult> GetEventTypeVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.EventType eventType = await _context.EventTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventType == null)
			{
				return NotFound();
			}

			try
			{
				eventType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EventType> versionInfo = await eventType.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a EventType at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventType</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The EventType object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventType/{id}/StateAtTime")]
		public async Task<IActionResult> GetEventTypeStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.EventType eventType = await _context.EventTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventType == null)
			{
				return NotFound();
			}

			try
			{
				eventType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EventType> versionInfo = await eventType.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a EventType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventType/{id}")]
		[Route("api/EventType")]
		public async Task<IActionResult> DeleteEventType(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.EventType> query = (from x in _context.EventTypes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventType eventType = await query.FirstOrDefaultAsync(cancellationToken);

			if (eventType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventType DELETE", id.ToString(), new Exception("No Scheduler.EventType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.EventType cloneOfExisting = (Database.EventType)_context.Entry(eventType).GetDatabaseValues().ToObject();


			lock (eventTypeDeleteSyncRoot)
			{
			    try
			    {
			        eventType.deleted = true;
			        eventType.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        EventTypeChangeHistory eventTypeChangeHistory = new EventTypeChangeHistory();
			        eventTypeChangeHistory.eventTypeId = eventType.id;
			        eventTypeChangeHistory.versionNumber = eventType.versionNumber;
			        eventTypeChangeHistory.timeStamp = DateTime.UtcNow;
			        eventTypeChangeHistory.userId = securityUser.id;
			        eventTypeChangeHistory.tenantGuid = userTenantGuid;
			        eventTypeChangeHistory.data = JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType));
			        _context.EventTypeChangeHistories.Add(eventTypeChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.EventType entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.EventType entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventType.CreateAnonymousWithFirstLevelSubObjects(eventType)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of EventType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/EventTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string color = null,
			int? iconId = null,
			int? sequence = null,
			bool? requiresRentalAgreement = null,
			bool? requiresExternalContact = null,
			bool? requiresPayment = null,
			bool? requiresDeposit = null,
			bool? requiresBarService = null,
			bool? allowsTicketSales = null,
			bool? isInternalEvent = null,
			decimal? defaultPrice = null,
			int? chargeTypeId = null,
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

			IQueryable<Database.EventType> query = (from et in _context.EventTypes select et);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(et => et.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(et => et.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(et => et.color == color);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(et => et.iconId == iconId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(et => et.sequence == sequence.Value);
			}
			if (requiresRentalAgreement.HasValue == true)
			{
				query = query.Where(et => et.requiresRentalAgreement == requiresRentalAgreement.Value);
			}
			if (requiresExternalContact.HasValue == true)
			{
				query = query.Where(et => et.requiresExternalContact == requiresExternalContact.Value);
			}
			if (requiresPayment.HasValue == true)
			{
				query = query.Where(et => et.requiresPayment == requiresPayment.Value);
			}
			if (requiresDeposit.HasValue == true)
			{
				query = query.Where(et => et.requiresDeposit == requiresDeposit.Value);
			}
			if (requiresBarService.HasValue == true)
			{
				query = query.Where(et => et.requiresBarService == requiresBarService.Value);
			}
			if (allowsTicketSales.HasValue == true)
			{
				query = query.Where(et => et.allowsTicketSales == allowsTicketSales.Value);
			}
			if (isInternalEvent.HasValue == true)
			{
				query = query.Where(et => et.isInternalEvent == isInternalEvent.Value);
			}
			if (defaultPrice.HasValue == true)
			{
				query = query.Where(et => et.defaultPrice == defaultPrice.Value);
			}
			if (chargeTypeId.HasValue == true)
			{
				query = query.Where(et => et.chargeTypeId == chargeTypeId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(et => et.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(et => et.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(et => et.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(et => et.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(et => et.deleted == false);
				}
			}
			else
			{
				query = query.Where(et => et.active == true);
				query = query.Where(et => et.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Event Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.chargeType.name.Contains(anyStringContains)
			       || x.chargeType.description.Contains(anyStringContains)
			       || x.chargeType.externalId.Contains(anyStringContains)
			       || x.chargeType.defaultDescription.Contains(anyStringContains)
			       || x.chargeType.color.Contains(anyStringContains)
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
			return Ok(await (from queryData in query select Database.EventType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/EventType/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
