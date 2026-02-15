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
    /// This auto generated class provides the basic CRUD operations for the EventCharge entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the EventCharge entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class EventChargesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object eventChargePutSyncRoot = new object();
		static object eventChargeDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<EventChargesController> _logger;

		public EventChargesController(SchedulerContext context, ILogger<EventChargesController> logger) : base("Scheduler", "EventCharge")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of EventCharges filtered by the parameters provided.
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
		[Route("api/EventCharges")]
		public async Task<IActionResult> GetEventCharges(
			int? scheduledEventId = null,
			int? resourceId = null,
			int? chargeTypeId = null,
			int? chargeStatusId = null,
			decimal? quantity = null,
			decimal? unitPrice = null,
			decimal? extendedAmount = null,
			decimal? taxAmount = null,
			int? currencyId = null,
			int? rateTypeId = null,
			string notes = null,
			bool? isAutomatic = null,
			DateTime? exportedDate = null,
			string externalId = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (exportedDate.HasValue == true && exportedDate.Value.Kind != DateTimeKind.Utc)
			{
				exportedDate = exportedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.EventCharge> query = (from ec in _context.EventCharges select ec);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ec => ec.scheduledEventId == scheduledEventId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(ec => ec.resourceId == resourceId.Value);
			}
			if (chargeTypeId.HasValue == true)
			{
				query = query.Where(ec => ec.chargeTypeId == chargeTypeId.Value);
			}
			if (chargeStatusId.HasValue == true)
			{
				query = query.Where(ec => ec.chargeStatusId == chargeStatusId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(ec => ec.quantity == quantity.Value);
			}
			if (unitPrice.HasValue == true)
			{
				query = query.Where(ec => ec.unitPrice == unitPrice.Value);
			}
			if (extendedAmount.HasValue == true)
			{
				query = query.Where(ec => ec.extendedAmount == extendedAmount.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(ec => ec.taxAmount == taxAmount.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(ec => ec.currencyId == currencyId.Value);
			}
			if (rateTypeId.HasValue == true)
			{
				query = query.Where(ec => ec.rateTypeId == rateTypeId.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(ec => ec.notes == notes);
			}
			if (isAutomatic.HasValue == true)
			{
				query = query.Where(ec => ec.isAutomatic == isAutomatic.Value);
			}
			if (exportedDate.HasValue == true)
			{
				query = query.Where(ec => ec.exportedDate == exportedDate.Value);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(ec => ec.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ec => ec.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ec => ec.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ec => ec.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ec => ec.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ec => ec.deleted == false);
				}
			}
			else
			{
				query = query.Where(ec => ec.active == true);
				query = query.Where(ec => ec.deleted == false);
			}

			query = query.OrderBy(ec => ec.externalId);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.chargeStatus);
				query = query.Include(x => x.chargeType);
				query = query.Include(x => x.currency);
				query = query.Include(x => x.rateType);
				query = query.Include(x => x.resource);
				query = query.Include(x => x.scheduledEvent);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Event Charge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || (includeRelations == true && x.chargeStatus.name.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeStatus.description.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeStatus.color.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.defaultDescription.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.name.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.description.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.code.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.color.Contains(anyStringContains))
			       || (includeRelations == true && x.rateType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.rateType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.rateType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarMimeType.Contains(anyStringContains))
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
			
			List<Database.EventCharge> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.EventCharge eventCharge in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(eventCharge, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.EventCharge Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.EventCharge Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of EventCharges filtered by the parameters provided.  Its query is similar to the GetEventCharges method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCharges/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? scheduledEventId = null,
			int? resourceId = null,
			int? chargeTypeId = null,
			int? chargeStatusId = null,
			decimal? quantity = null,
			decimal? unitPrice = null,
			decimal? extendedAmount = null,
			decimal? taxAmount = null,
			int? currencyId = null,
			int? rateTypeId = null,
			string notes = null,
			bool? isAutomatic = null,
			DateTime? exportedDate = null,
			string externalId = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (exportedDate.HasValue == true && exportedDate.Value.Kind != DateTimeKind.Utc)
			{
				exportedDate = exportedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.EventCharge> query = (from ec in _context.EventCharges select ec);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ec => ec.scheduledEventId == scheduledEventId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(ec => ec.resourceId == resourceId.Value);
			}
			if (chargeTypeId.HasValue == true)
			{
				query = query.Where(ec => ec.chargeTypeId == chargeTypeId.Value);
			}
			if (chargeStatusId.HasValue == true)
			{
				query = query.Where(ec => ec.chargeStatusId == chargeStatusId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(ec => ec.quantity == quantity.Value);
			}
			if (unitPrice.HasValue == true)
			{
				query = query.Where(ec => ec.unitPrice == unitPrice.Value);
			}
			if (extendedAmount.HasValue == true)
			{
				query = query.Where(ec => ec.extendedAmount == extendedAmount.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(ec => ec.taxAmount == taxAmount.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(ec => ec.currencyId == currencyId.Value);
			}
			if (rateTypeId.HasValue == true)
			{
				query = query.Where(ec => ec.rateTypeId == rateTypeId.Value);
			}
			if (notes != null)
			{
				query = query.Where(ec => ec.notes == notes);
			}
			if (isAutomatic.HasValue == true)
			{
				query = query.Where(ec => ec.isAutomatic == isAutomatic.Value);
			}
			if (exportedDate.HasValue == true)
			{
				query = query.Where(ec => ec.exportedDate == exportedDate.Value);
			}
			if (externalId != null)
			{
				query = query.Where(ec => ec.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ec => ec.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ec => ec.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ec => ec.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ec => ec.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ec => ec.deleted == false);
				}
			}
			else
			{
				query = query.Where(ec => ec.active == true);
				query = query.Where(ec => ec.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Event Charge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.chargeStatus.name.Contains(anyStringContains)
			       || x.chargeStatus.description.Contains(anyStringContains)
			       || x.chargeStatus.color.Contains(anyStringContains)
			       || x.chargeType.name.Contains(anyStringContains)
			       || x.chargeType.description.Contains(anyStringContains)
			       || x.chargeType.externalId.Contains(anyStringContains)
			       || x.chargeType.defaultDescription.Contains(anyStringContains)
			       || x.chargeType.color.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.rateType.name.Contains(anyStringContains)
			       || x.rateType.description.Contains(anyStringContains)
			       || x.rateType.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
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
        /// This gets a single EventCharge by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCharge/{id}")]
		public async Task<IActionResult> GetEventCharge(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.EventCharge> query = (from ec in _context.EventCharges where
							(ec.id == id) &&
							(userIsAdmin == true || ec.deleted == false) &&
							(userIsWriter == true || ec.active == true)
					select ec);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.chargeStatus);
					query = query.Include(x => x.chargeType);
					query = query.Include(x => x.currency);
					query = query.Include(x => x.rateType);
					query = query.Include(x => x.resource);
					query = query.Include(x => x.scheduledEvent);
					query = query.AsSplitQuery();
				}

				Database.EventCharge materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.EventCharge Entity was read with Admin privilege." : "Scheduler.EventCharge Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventCharge", materialized.id, materialized.externalId));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.EventCharge entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.EventCharge.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.EventCharge.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing EventCharge record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/EventCharge/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutEventCharge(int id, [FromBody]Database.EventCharge.EventChargeDTO eventChargeDTO, CancellationToken cancellationToken = default)
		{
			if (eventChargeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != eventChargeDTO.id)
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


			IQueryable<Database.EventCharge> query = (from x in _context.EventCharges
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventCharge existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventCharge PUT", id.ToString(), new Exception("No Scheduler.EventCharge entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (eventChargeDTO.objectGuid == Guid.Empty)
            {
                eventChargeDTO.objectGuid = existing.objectGuid;
            }
            else if (eventChargeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a EventCharge record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.EventCharge cloneOfExisting = (Database.EventCharge)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new EventCharge object using the data from the existing record, updated with what is in the DTO.
			//
			Database.EventCharge eventCharge = (Database.EventCharge)_context.Entry(existing).GetDatabaseValues().ToObject();
			eventCharge.ApplyDTO(eventChargeDTO);
			//
			// The tenant guid for any EventCharge being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the EventCharge because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				eventCharge.tenantGuid = existing.tenantGuid;
			}

			lock (eventChargePutSyncRoot)
			{
				//
				// Validate the version number for the eventCharge being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != eventCharge.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "EventCharge save attempt was made but save request was with version " + eventCharge.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The EventCharge you are trying to update has already changed.  Please try your save again after reloading the EventCharge.");
				}
				else
				{
					// Same record.  Increase version.
					eventCharge.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (eventCharge.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.EventCharge record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (eventCharge.exportedDate.HasValue == true && eventCharge.exportedDate.Value.Kind != DateTimeKind.Utc)
				{
					eventCharge.exportedDate = eventCharge.exportedDate.Value.ToUniversalTime();
				}

				if (eventCharge.externalId != null && eventCharge.externalId.Length > 100)
				{
					eventCharge.externalId = eventCharge.externalId.Substring(0, 100);
				}

				try
				{
				    EntityEntry<Database.EventCharge> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(eventCharge);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        EventChargeChangeHistory eventChargeChangeHistory = new EventChargeChangeHistory();
				        eventChargeChangeHistory.eventChargeId = eventCharge.id;
				        eventChargeChangeHistory.versionNumber = eventCharge.versionNumber;
				        eventChargeChangeHistory.timeStamp = DateTime.UtcNow;
				        eventChargeChangeHistory.userId = securityUser.id;
				        eventChargeChangeHistory.tenantGuid = userTenantGuid;
				        eventChargeChangeHistory.data = JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge));
				        _context.EventChargeChangeHistories.Add(eventChargeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventCharge entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge)),
						null);

				return Ok(Database.EventCharge.CreateAnonymous(eventCharge));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventCharge entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new EventCharge record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCharge", Name = "EventCharge")]
		public async Task<IActionResult> PostEventCharge([FromBody]Database.EventCharge.EventChargeDTO eventChargeDTO, CancellationToken cancellationToken = default)
		{
			if (eventChargeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
			// Create a new EventCharge object using the data from the DTO
			//
			Database.EventCharge eventCharge = Database.EventCharge.FromDTO(eventChargeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				eventCharge.tenantGuid = userTenantGuid;

				if (eventCharge.exportedDate.HasValue == true && eventCharge.exportedDate.Value.Kind != DateTimeKind.Utc)
				{
					eventCharge.exportedDate = eventCharge.exportedDate.Value.ToUniversalTime();
				}

				if (eventCharge.externalId != null && eventCharge.externalId.Length > 100)
				{
					eventCharge.externalId = eventCharge.externalId.Substring(0, 100);
				}

				eventCharge.objectGuid = Guid.NewGuid();
				eventCharge.versionNumber = 1;

				_context.EventCharges.Add(eventCharge);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the eventCharge object so that no further changes will be written to the database
				    //
				    _context.Entry(eventCharge).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					eventCharge.EventChargeChangeHistories = null;
					eventCharge.chargeStatus = null;
					eventCharge.chargeType = null;
					eventCharge.currency = null;
					eventCharge.rateType = null;
					eventCharge.resource = null;
					eventCharge.scheduledEvent = null;


				    EventChargeChangeHistory eventChargeChangeHistory = new EventChargeChangeHistory();
				    eventChargeChangeHistory.eventChargeId = eventCharge.id;
				    eventChargeChangeHistory.versionNumber = eventCharge.versionNumber;
				    eventChargeChangeHistory.timeStamp = DateTime.UtcNow;
				    eventChargeChangeHistory.userId = securityUser.id;
				    eventChargeChangeHistory.tenantGuid = userTenantGuid;
				    eventChargeChangeHistory.data = JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge));
				    _context.EventChargeChangeHistories.Add(eventChargeChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.EventCharge entity successfully created.",
						true,
						eventCharge. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.EventCharge entity creation failed.", false, eventCharge.id.ToString(), "", JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EventCharge", eventCharge.id, eventCharge.externalId));

			return CreatedAtRoute("EventCharge", new { id = eventCharge.id }, Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge));
		}



        /// <summary>
        /// 
        /// This rolls a EventCharge entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCharge/Rollback/{id}")]
		[Route("api/EventCharge/Rollback")]
		public async Task<IActionResult> RollbackToEventChargeVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.EventCharge> query = (from x in _context.EventCharges
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this EventCharge concurrently
			//
			lock (eventChargePutSyncRoot)
			{
				
				Database.EventCharge eventCharge = query.FirstOrDefault();
				
				if (eventCharge == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventCharge rollback", id.ToString(), new Exception("No Scheduler.EventCharge entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the EventCharge current state so we can log it.
				//
				Database.EventCharge cloneOfExisting = (Database.EventCharge)_context.Entry(eventCharge).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.EventChargeChangeHistories = null;
				cloneOfExisting.chargeStatus = null;
				cloneOfExisting.chargeType = null;
				cloneOfExisting.currency = null;
				cloneOfExisting.rateType = null;
				cloneOfExisting.resource = null;
				cloneOfExisting.scheduledEvent = null;

				if (versionNumber >= eventCharge.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.EventCharge rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.EventCharge rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				EventChargeChangeHistory eventChargeChangeHistory = (from x in _context.EventChargeChangeHistories
				                                               where
				                                               x.eventChargeId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (eventChargeChangeHistory != null)
				{
				    Database.EventCharge oldEventCharge = JsonSerializer.Deserialize<Database.EventCharge>(eventChargeChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    eventCharge.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    eventCharge.scheduledEventId = oldEventCharge.scheduledEventId;
				    eventCharge.resourceId = oldEventCharge.resourceId;
				    eventCharge.chargeTypeId = oldEventCharge.chargeTypeId;
				    eventCharge.chargeStatusId = oldEventCharge.chargeStatusId;
				    eventCharge.quantity = oldEventCharge.quantity;
				    eventCharge.unitPrice = oldEventCharge.unitPrice;
				    eventCharge.extendedAmount = oldEventCharge.extendedAmount;
				    eventCharge.taxAmount = oldEventCharge.taxAmount;
				    eventCharge.currencyId = oldEventCharge.currencyId;
				    eventCharge.rateTypeId = oldEventCharge.rateTypeId;
				    eventCharge.notes = oldEventCharge.notes;
				    eventCharge.isAutomatic = oldEventCharge.isAutomatic;
				    eventCharge.exportedDate = oldEventCharge.exportedDate;
				    eventCharge.externalId = oldEventCharge.externalId;
				    eventCharge.objectGuid = oldEventCharge.objectGuid;
				    eventCharge.active = oldEventCharge.active;
				    eventCharge.deleted = oldEventCharge.deleted;

				    string serializedEventCharge = JsonSerializer.Serialize(eventCharge);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        EventChargeChangeHistory newEventChargeChangeHistory = new EventChargeChangeHistory();
				        newEventChargeChangeHistory.eventChargeId = eventCharge.id;
				        newEventChargeChangeHistory.versionNumber = eventCharge.versionNumber;
				        newEventChargeChangeHistory.timeStamp = DateTime.UtcNow;
				        newEventChargeChangeHistory.userId = securityUser.id;
				        newEventChargeChangeHistory.tenantGuid = userTenantGuid;
				        newEventChargeChangeHistory.data = JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge));
				        _context.EventChargeChangeHistories.Add(newEventChargeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.EventCharge rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge)),
						null);


				    return Ok(Database.EventCharge.CreateAnonymous(eventCharge));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.EventCharge rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.EventCharge rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a EventCharge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventCharge</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCharge/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetEventChargeChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.EventCharge eventCharge = await _context.EventCharges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventCharge == null)
			{
				return NotFound();
			}

			try
			{
				eventCharge.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EventCharge> versionInfo = await eventCharge.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a EventCharge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventCharge</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCharge/{id}/AuditHistory")]
		public async Task<IActionResult> GetEventChargeAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.EventCharge eventCharge = await _context.EventCharges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventCharge == null)
			{
				return NotFound();
			}

			try
			{
				eventCharge.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.EventCharge>> versions = await eventCharge.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a EventCharge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventCharge</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The EventCharge object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCharge/{id}/Version/{version}")]
		public async Task<IActionResult> GetEventChargeVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.EventCharge eventCharge = await _context.EventCharges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventCharge == null)
			{
				return NotFound();
			}

			try
			{
				eventCharge.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EventCharge> versionInfo = await eventCharge.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a EventCharge at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the EventCharge</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The EventCharge object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCharge/{id}/StateAtTime")]
		public async Task<IActionResult> GetEventChargeStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.EventCharge eventCharge = await _context.EventCharges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (eventCharge == null)
			{
				return NotFound();
			}

			try
			{
				eventCharge.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.EventCharge> versionInfo = await eventCharge.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a EventCharge record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EventCharge/{id}")]
		[Route("api/EventCharge")]
		public async Task<IActionResult> DeleteEventCharge(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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

			IQueryable<Database.EventCharge> query = (from x in _context.EventCharges
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.EventCharge eventCharge = await query.FirstOrDefaultAsync(cancellationToken);

			if (eventCharge == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.EventCharge DELETE", id.ToString(), new Exception("No Scheduler.EventCharge entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.EventCharge cloneOfExisting = (Database.EventCharge)_context.Entry(eventCharge).GetDatabaseValues().ToObject();


			lock (eventChargeDeleteSyncRoot)
			{
			    try
			    {
			        eventCharge.deleted = true;
			        eventCharge.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        EventChargeChangeHistory eventChargeChangeHistory = new EventChargeChangeHistory();
			        eventChargeChangeHistory.eventChargeId = eventCharge.id;
			        eventChargeChangeHistory.versionNumber = eventCharge.versionNumber;
			        eventChargeChangeHistory.timeStamp = DateTime.UtcNow;
			        eventChargeChangeHistory.userId = securityUser.id;
			        eventChargeChangeHistory.tenantGuid = userTenantGuid;
			        eventChargeChangeHistory.data = JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge));
			        _context.EventChargeChangeHistories.Add(eventChargeChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.EventCharge entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.EventCharge entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.EventCharge.CreateAnonymousWithFirstLevelSubObjects(eventCharge)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of EventCharge records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/EventCharges/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? scheduledEventId = null,
			int? resourceId = null,
			int? chargeTypeId = null,
			int? chargeStatusId = null,
			decimal? quantity = null,
			decimal? unitPrice = null,
			decimal? extendedAmount = null,
			decimal? taxAmount = null,
			int? currencyId = null,
			int? rateTypeId = null,
			string notes = null,
			bool? isAutomatic = null,
			DateTime? exportedDate = null,
			string externalId = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (exportedDate.HasValue == true && exportedDate.Value.Kind != DateTimeKind.Utc)
			{
				exportedDate = exportedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.EventCharge> query = (from ec in _context.EventCharges select ec);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(ec => ec.scheduledEventId == scheduledEventId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(ec => ec.resourceId == resourceId.Value);
			}
			if (chargeTypeId.HasValue == true)
			{
				query = query.Where(ec => ec.chargeTypeId == chargeTypeId.Value);
			}
			if (chargeStatusId.HasValue == true)
			{
				query = query.Where(ec => ec.chargeStatusId == chargeStatusId.Value);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(ec => ec.quantity == quantity.Value);
			}
			if (unitPrice.HasValue == true)
			{
				query = query.Where(ec => ec.unitPrice == unitPrice.Value);
			}
			if (extendedAmount.HasValue == true)
			{
				query = query.Where(ec => ec.extendedAmount == extendedAmount.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(ec => ec.taxAmount == taxAmount.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(ec => ec.currencyId == currencyId.Value);
			}
			if (rateTypeId.HasValue == true)
			{
				query = query.Where(ec => ec.rateTypeId == rateTypeId.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(ec => ec.notes == notes);
			}
			if (isAutomatic.HasValue == true)
			{
				query = query.Where(ec => ec.isAutomatic == isAutomatic.Value);
			}
			if (exportedDate.HasValue == true)
			{
				query = query.Where(ec => ec.exportedDate == exportedDate.Value);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(ec => ec.externalId == externalId);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ec => ec.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ec => ec.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ec => ec.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ec => ec.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ec => ec.deleted == false);
				}
			}
			else
			{
				query = query.Where(ec => ec.active == true);
				query = query.Where(ec => ec.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Event Charge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.chargeStatus.name.Contains(anyStringContains)
			       || x.chargeStatus.description.Contains(anyStringContains)
			       || x.chargeStatus.color.Contains(anyStringContains)
			       || x.chargeType.name.Contains(anyStringContains)
			       || x.chargeType.description.Contains(anyStringContains)
			       || x.chargeType.externalId.Contains(anyStringContains)
			       || x.chargeType.defaultDescription.Contains(anyStringContains)
			       || x.chargeType.color.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.rateType.name.Contains(anyStringContains)
			       || x.rateType.description.Contains(anyStringContains)
			       || x.rateType.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
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


			query = query.OrderBy(x => x.externalId);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.EventCharge.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/EventCharge/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
