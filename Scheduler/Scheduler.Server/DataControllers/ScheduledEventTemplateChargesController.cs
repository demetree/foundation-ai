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
    /// This auto generated class provides the basic CRUD operations for the ScheduledEventTemplateCharge entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ScheduledEventTemplateCharge entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ScheduledEventTemplateChargesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object scheduledEventTemplateChargePutSyncRoot = new object();
		static object scheduledEventTemplateChargeDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ScheduledEventTemplateChargesController> _logger;

		public ScheduledEventTemplateChargesController(SchedulerContext context, ILogger<ScheduledEventTemplateChargesController> logger) : base("Scheduler", "ScheduledEventTemplateCharge")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ScheduledEventTemplateCharges filtered by the parameters provided.
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
		[Route("api/ScheduledEventTemplateCharges")]
		public async Task<IActionResult> GetScheduledEventTemplateCharges(
			int? scheduledEventTemplateId = null,
			int? chargeTypeId = null,
			decimal? defaultAmount = null,
			bool? isRequired = null,
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

			IQueryable<Database.ScheduledEventTemplateCharge> query = (from setc in _context.ScheduledEventTemplateCharges select setc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventTemplateId.HasValue == true)
			{
				query = query.Where(setc => setc.scheduledEventTemplateId == scheduledEventTemplateId.Value);
			}
			if (chargeTypeId.HasValue == true)
			{
				query = query.Where(setc => setc.chargeTypeId == chargeTypeId.Value);
			}
			if (defaultAmount.HasValue == true)
			{
				query = query.Where(setc => setc.defaultAmount == defaultAmount.Value);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(setc => setc.isRequired == isRequired.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(setc => setc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(setc => setc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(setc => setc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(setc => setc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(setc => setc.deleted == false);
				}
			}
			else
			{
				query = query.Where(setc => setc.active == true);
				query = query.Where(setc => setc.deleted == false);
			}

			query = query.OrderBy(setc => setc.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.chargeType);
				query = query.Include(x => x.scheduledEventTemplate);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Template Charge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.chargeType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.defaultDescription.Contains(anyStringContains))
			       || (includeRelations == true && x.chargeType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEventTemplate.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEventTemplate.description.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEventTemplate.defaultLocationPattern.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ScheduledEventTemplateCharge> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ScheduledEventTemplateCharge scheduledEventTemplateCharge in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(scheduledEventTemplateCharge, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ScheduledEventTemplateCharge Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ScheduledEventTemplateCharge Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ScheduledEventTemplateCharges filtered by the parameters provided.  Its query is similar to the GetScheduledEventTemplateCharges method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateCharges/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? scheduledEventTemplateId = null,
			int? chargeTypeId = null,
			decimal? defaultAmount = null,
			bool? isRequired = null,
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


			IQueryable<Database.ScheduledEventTemplateCharge> query = (from setc in _context.ScheduledEventTemplateCharges select setc);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (scheduledEventTemplateId.HasValue == true)
			{
				query = query.Where(setc => setc.scheduledEventTemplateId == scheduledEventTemplateId.Value);
			}
			if (chargeTypeId.HasValue == true)
			{
				query = query.Where(setc => setc.chargeTypeId == chargeTypeId.Value);
			}
			if (defaultAmount.HasValue == true)
			{
				query = query.Where(setc => setc.defaultAmount == defaultAmount.Value);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(setc => setc.isRequired == isRequired.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(setc => setc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(setc => setc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(setc => setc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(setc => setc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(setc => setc.deleted == false);
				}
			}
			else
			{
				query = query.Where(setc => setc.active == true);
				query = query.Where(setc => setc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Template Charge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.chargeType.name.Contains(anyStringContains)
			       || x.chargeType.description.Contains(anyStringContains)
			       || x.chargeType.externalId.Contains(anyStringContains)
			       || x.chargeType.defaultDescription.Contains(anyStringContains)
			       || x.chargeType.color.Contains(anyStringContains)
			       || x.scheduledEventTemplate.name.Contains(anyStringContains)
			       || x.scheduledEventTemplate.description.Contains(anyStringContains)
			       || x.scheduledEventTemplate.defaultLocationPattern.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ScheduledEventTemplateCharge by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateCharge/{id}")]
		public async Task<IActionResult> GetScheduledEventTemplateCharge(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ScheduledEventTemplateCharge> query = (from setc in _context.ScheduledEventTemplateCharges where
							(setc.id == id) &&
							(userIsAdmin == true || setc.deleted == false) &&
							(userIsWriter == true || setc.active == true)
					select setc);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.chargeType);
					query = query.Include(x => x.scheduledEventTemplate);
					query = query.AsSplitQuery();
				}

				Database.ScheduledEventTemplateCharge materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ScheduledEventTemplateCharge Entity was read with Admin privilege." : "Scheduler.ScheduledEventTemplateCharge Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduledEventTemplateCharge", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ScheduledEventTemplateCharge entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ScheduledEventTemplateCharge.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ScheduledEventTemplateCharge.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ScheduledEventTemplateCharge record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ScheduledEventTemplateCharge/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutScheduledEventTemplateCharge(int id, [FromBody]Database.ScheduledEventTemplateCharge.ScheduledEventTemplateChargeDTO scheduledEventTemplateChargeDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventTemplateChargeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != scheduledEventTemplateChargeDTO.id)
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


			IQueryable<Database.ScheduledEventTemplateCharge> query = (from x in _context.ScheduledEventTemplateCharges
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEventTemplateCharge existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventTemplateCharge PUT", id.ToString(), new Exception("No Scheduler.ScheduledEventTemplateCharge entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (scheduledEventTemplateChargeDTO.objectGuid == Guid.Empty)
            {
                scheduledEventTemplateChargeDTO.objectGuid = existing.objectGuid;
            }
            else if (scheduledEventTemplateChargeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ScheduledEventTemplateCharge record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ScheduledEventTemplateCharge cloneOfExisting = (Database.ScheduledEventTemplateCharge)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ScheduledEventTemplateCharge object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ScheduledEventTemplateCharge scheduledEventTemplateCharge = (Database.ScheduledEventTemplateCharge)_context.Entry(existing).GetDatabaseValues().ToObject();
			scheduledEventTemplateCharge.ApplyDTO(scheduledEventTemplateChargeDTO);
			//
			// The tenant guid for any ScheduledEventTemplateCharge being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ScheduledEventTemplateCharge because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				scheduledEventTemplateCharge.tenantGuid = existing.tenantGuid;
			}

			lock (scheduledEventTemplateChargePutSyncRoot)
			{
				//
				// Validate the version number for the scheduledEventTemplateCharge being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != scheduledEventTemplateCharge.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ScheduledEventTemplateCharge save attempt was made but save request was with version " + scheduledEventTemplateCharge.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ScheduledEventTemplateCharge you are trying to update has already changed.  Please try your save again after reloading the ScheduledEventTemplateCharge.");
				}
				else
				{
					// Same record.  Increase version.
					scheduledEventTemplateCharge.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (scheduledEventTemplateCharge.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ScheduledEventTemplateCharge record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.ScheduledEventTemplateCharge> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(scheduledEventTemplateCharge);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduledEventTemplateChargeChangeHistory scheduledEventTemplateChargeChangeHistory = new ScheduledEventTemplateChargeChangeHistory();
				        scheduledEventTemplateChargeChangeHistory.scheduledEventTemplateChargeId = scheduledEventTemplateCharge.id;
				        scheduledEventTemplateChargeChangeHistory.versionNumber = scheduledEventTemplateCharge.versionNumber;
				        scheduledEventTemplateChargeChangeHistory.timeStamp = DateTime.UtcNow;
				        scheduledEventTemplateChargeChangeHistory.userId = securityUser.id;
				        scheduledEventTemplateChargeChangeHistory.tenantGuid = userTenantGuid;
				        scheduledEventTemplateChargeChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge));
				        _context.ScheduledEventTemplateChargeChangeHistories.Add(scheduledEventTemplateChargeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventTemplateCharge entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge)),
						null);

				return Ok(Database.ScheduledEventTemplateCharge.CreateAnonymous(scheduledEventTemplateCharge));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventTemplateCharge entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ScheduledEventTemplateCharge record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateCharge", Name = "ScheduledEventTemplateCharge")]
		public async Task<IActionResult> PostScheduledEventTemplateCharge([FromBody]Database.ScheduledEventTemplateCharge.ScheduledEventTemplateChargeDTO scheduledEventTemplateChargeDTO, CancellationToken cancellationToken = default)
		{
			if (scheduledEventTemplateChargeDTO == null)
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
			// Create a new ScheduledEventTemplateCharge object using the data from the DTO
			//
			Database.ScheduledEventTemplateCharge scheduledEventTemplateCharge = Database.ScheduledEventTemplateCharge.FromDTO(scheduledEventTemplateChargeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				scheduledEventTemplateCharge.tenantGuid = userTenantGuid;

				scheduledEventTemplateCharge.objectGuid = Guid.NewGuid();
				scheduledEventTemplateCharge.versionNumber = 1;

				_context.ScheduledEventTemplateCharges.Add(scheduledEventTemplateCharge);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the scheduledEventTemplateCharge object so that no further changes will be written to the database
				    //
				    _context.Entry(scheduledEventTemplateCharge).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					scheduledEventTemplateCharge.ScheduledEventTemplateChargeChangeHistories = null;
					scheduledEventTemplateCharge.chargeType = null;
					scheduledEventTemplateCharge.scheduledEventTemplate = null;


				    ScheduledEventTemplateChargeChangeHistory scheduledEventTemplateChargeChangeHistory = new ScheduledEventTemplateChargeChangeHistory();
				    scheduledEventTemplateChargeChangeHistory.scheduledEventTemplateChargeId = scheduledEventTemplateCharge.id;
				    scheduledEventTemplateChargeChangeHistory.versionNumber = scheduledEventTemplateCharge.versionNumber;
				    scheduledEventTemplateChargeChangeHistory.timeStamp = DateTime.UtcNow;
				    scheduledEventTemplateChargeChangeHistory.userId = securityUser.id;
				    scheduledEventTemplateChargeChangeHistory.tenantGuid = userTenantGuid;
				    scheduledEventTemplateChargeChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge));
				    _context.ScheduledEventTemplateChargeChangeHistories.Add(scheduledEventTemplateChargeChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ScheduledEventTemplateCharge entity successfully created.",
						true,
						scheduledEventTemplateCharge. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ScheduledEventTemplateCharge entity creation failed.", false, scheduledEventTemplateCharge.id.ToString(), "", JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduledEventTemplateCharge", scheduledEventTemplateCharge.id, scheduledEventTemplateCharge.id.ToString()));

			return CreatedAtRoute("ScheduledEventTemplateCharge", new { id = scheduledEventTemplateCharge.id }, Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge));
		}



        /// <summary>
        /// 
        /// This rolls a ScheduledEventTemplateCharge entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateCharge/Rollback/{id}")]
		[Route("api/ScheduledEventTemplateCharge/Rollback")]
		public async Task<IActionResult> RollbackToScheduledEventTemplateChargeVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ScheduledEventTemplateCharge> query = (from x in _context.ScheduledEventTemplateCharges
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ScheduledEventTemplateCharge concurrently
			//
			lock (scheduledEventTemplateChargePutSyncRoot)
			{
				
				Database.ScheduledEventTemplateCharge scheduledEventTemplateCharge = query.FirstOrDefault();
				
				if (scheduledEventTemplateCharge == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventTemplateCharge rollback", id.ToString(), new Exception("No Scheduler.ScheduledEventTemplateCharge entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ScheduledEventTemplateCharge current state so we can log it.
				//
				Database.ScheduledEventTemplateCharge cloneOfExisting = (Database.ScheduledEventTemplateCharge)_context.Entry(scheduledEventTemplateCharge).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ScheduledEventTemplateChargeChangeHistories = null;
				cloneOfExisting.chargeType = null;
				cloneOfExisting.scheduledEventTemplate = null;

				if (versionNumber >= scheduledEventTemplateCharge.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ScheduledEventTemplateCharge rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ScheduledEventTemplateCharge rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ScheduledEventTemplateChargeChangeHistory scheduledEventTemplateChargeChangeHistory = (from x in _context.ScheduledEventTemplateChargeChangeHistories
				                                               where
				                                               x.scheduledEventTemplateChargeId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (scheduledEventTemplateChargeChangeHistory != null)
				{
				    Database.ScheduledEventTemplateCharge oldScheduledEventTemplateCharge = JsonSerializer.Deserialize<Database.ScheduledEventTemplateCharge>(scheduledEventTemplateChargeChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    scheduledEventTemplateCharge.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    scheduledEventTemplateCharge.scheduledEventTemplateId = oldScheduledEventTemplateCharge.scheduledEventTemplateId;
				    scheduledEventTemplateCharge.chargeTypeId = oldScheduledEventTemplateCharge.chargeTypeId;
				    scheduledEventTemplateCharge.defaultAmount = oldScheduledEventTemplateCharge.defaultAmount;
				    scheduledEventTemplateCharge.isRequired = oldScheduledEventTemplateCharge.isRequired;
				    scheduledEventTemplateCharge.objectGuid = oldScheduledEventTemplateCharge.objectGuid;
				    scheduledEventTemplateCharge.active = oldScheduledEventTemplateCharge.active;
				    scheduledEventTemplateCharge.deleted = oldScheduledEventTemplateCharge.deleted;

				    string serializedScheduledEventTemplateCharge = JsonSerializer.Serialize(scheduledEventTemplateCharge);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduledEventTemplateChargeChangeHistory newScheduledEventTemplateChargeChangeHistory = new ScheduledEventTemplateChargeChangeHistory();
				        newScheduledEventTemplateChargeChangeHistory.scheduledEventTemplateChargeId = scheduledEventTemplateCharge.id;
				        newScheduledEventTemplateChargeChangeHistory.versionNumber = scheduledEventTemplateCharge.versionNumber;
				        newScheduledEventTemplateChargeChangeHistory.timeStamp = DateTime.UtcNow;
				        newScheduledEventTemplateChargeChangeHistory.userId = securityUser.id;
				        newScheduledEventTemplateChargeChangeHistory.tenantGuid = userTenantGuid;
				        newScheduledEventTemplateChargeChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge));
				        _context.ScheduledEventTemplateChargeChangeHistories.Add(newScheduledEventTemplateChargeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ScheduledEventTemplateCharge rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge)),
						null);


				    return Ok(Database.ScheduledEventTemplateCharge.CreateAnonymous(scheduledEventTemplateCharge));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ScheduledEventTemplateCharge rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ScheduledEventTemplateCharge rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ScheduledEventTemplateCharge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplateCharge</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateCharge/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetScheduledEventTemplateChargeChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
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


			Database.ScheduledEventTemplateCharge scheduledEventTemplateCharge = await _context.ScheduledEventTemplateCharges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplateCharge == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplateCharge.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEventTemplateCharge> versionInfo = await scheduledEventTemplateCharge.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ScheduledEventTemplateCharge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplateCharge</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateCharge/{id}/AuditHistory")]
		public async Task<IActionResult> GetScheduledEventTemplateChargeAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{
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


			Database.ScheduledEventTemplateCharge scheduledEventTemplateCharge = await _context.ScheduledEventTemplateCharges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplateCharge == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplateCharge.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ScheduledEventTemplateCharge>> versions = await scheduledEventTemplateCharge.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ScheduledEventTemplateCharge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplateCharge</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ScheduledEventTemplateCharge object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateCharge/{id}/Version/{version}")]
		public async Task<IActionResult> GetScheduledEventTemplateChargeVersion(int id, int version, CancellationToken cancellationToken = default)
		{
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


			Database.ScheduledEventTemplateCharge scheduledEventTemplateCharge = await _context.ScheduledEventTemplateCharges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplateCharge == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplateCharge.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEventTemplateCharge> versionInfo = await scheduledEventTemplateCharge.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ScheduledEventTemplateCharge at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduledEventTemplateCharge</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ScheduledEventTemplateCharge object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateCharge/{id}/StateAtTime")]
		public async Task<IActionResult> GetScheduledEventTemplateChargeStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{
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


			Database.ScheduledEventTemplateCharge scheduledEventTemplateCharge = await _context.ScheduledEventTemplateCharges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplateCharge == null)
			{
				return NotFound();
			}

			try
			{
				scheduledEventTemplateCharge.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduledEventTemplateCharge> versionInfo = await scheduledEventTemplateCharge.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ScheduledEventTemplateCharge record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduledEventTemplateCharge/{id}")]
		[Route("api/ScheduledEventTemplateCharge")]
		public async Task<IActionResult> DeleteScheduledEventTemplateCharge(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ScheduledEventTemplateCharge> query = (from x in _context.ScheduledEventTemplateCharges
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduledEventTemplateCharge scheduledEventTemplateCharge = await query.FirstOrDefaultAsync(cancellationToken);

			if (scheduledEventTemplateCharge == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ScheduledEventTemplateCharge DELETE", id.ToString(), new Exception("No Scheduler.ScheduledEventTemplateCharge entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ScheduledEventTemplateCharge cloneOfExisting = (Database.ScheduledEventTemplateCharge)_context.Entry(scheduledEventTemplateCharge).GetDatabaseValues().ToObject();


			lock (scheduledEventTemplateChargeDeleteSyncRoot)
			{
			    try
			    {
			        scheduledEventTemplateCharge.deleted = true;
			        scheduledEventTemplateCharge.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ScheduledEventTemplateChargeChangeHistory scheduledEventTemplateChargeChangeHistory = new ScheduledEventTemplateChargeChangeHistory();
			        scheduledEventTemplateChargeChangeHistory.scheduledEventTemplateChargeId = scheduledEventTemplateCharge.id;
			        scheduledEventTemplateChargeChangeHistory.versionNumber = scheduledEventTemplateCharge.versionNumber;
			        scheduledEventTemplateChargeChangeHistory.timeStamp = DateTime.UtcNow;
			        scheduledEventTemplateChargeChangeHistory.userId = securityUser.id;
			        scheduledEventTemplateChargeChangeHistory.tenantGuid = userTenantGuid;
			        scheduledEventTemplateChargeChangeHistory.data = JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge));
			        _context.ScheduledEventTemplateChargeChangeHistories.Add(scheduledEventTemplateChargeChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ScheduledEventTemplateCharge entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ScheduledEventTemplateCharge entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduledEventTemplateCharge.CreateAnonymousWithFirstLevelSubObjects(scheduledEventTemplateCharge)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ScheduledEventTemplateCharge records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ScheduledEventTemplateCharges/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? scheduledEventTemplateId = null,
			int? chargeTypeId = null,
			decimal? defaultAmount = null,
			bool? isRequired = null,
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

			IQueryable<Database.ScheduledEventTemplateCharge> query = (from setc in _context.ScheduledEventTemplateCharges select setc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduledEventTemplateId.HasValue == true)
			{
				query = query.Where(setc => setc.scheduledEventTemplateId == scheduledEventTemplateId.Value);
			}
			if (chargeTypeId.HasValue == true)
			{
				query = query.Where(setc => setc.chargeTypeId == chargeTypeId.Value);
			}
			if (defaultAmount.HasValue == true)
			{
				query = query.Where(setc => setc.defaultAmount == defaultAmount.Value);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(setc => setc.isRequired == isRequired.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(setc => setc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(setc => setc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(setc => setc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(setc => setc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(setc => setc.deleted == false);
				}
			}
			else
			{
				query = query.Where(setc => setc.active == true);
				query = query.Where(setc => setc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduled Event Template Charge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.chargeType.name.Contains(anyStringContains)
			       || x.chargeType.description.Contains(anyStringContains)
			       || x.chargeType.externalId.Contains(anyStringContains)
			       || x.chargeType.defaultDescription.Contains(anyStringContains)
			       || x.chargeType.color.Contains(anyStringContains)
			       || x.scheduledEventTemplate.name.Contains(anyStringContains)
			       || x.scheduledEventTemplate.description.Contains(anyStringContains)
			       || x.scheduledEventTemplate.defaultLocationPattern.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ScheduledEventTemplateCharge.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ScheduledEventTemplateCharge/CreateAuditEvent")]
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
