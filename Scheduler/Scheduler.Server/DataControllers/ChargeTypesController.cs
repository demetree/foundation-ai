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
    /// This auto generated class provides the basic CRUD operations for the ChargeType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ChargeType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ChargeTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object chargeTypePutSyncRoot = new object();
		static object chargeTypeDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ChargeTypesController> _logger;

		public ChargeTypesController(SchedulerContext context, ILogger<ChargeTypesController> logger) : base("Scheduler", "ChargeType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ChargeTypes filtered by the parameters provided.
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
		[Route("api/ChargeTypes")]
		public async Task<IActionResult> GetChargeTypes(
			string name = null,
			string description = null,
			string externalId = null,
			bool? isRevenue = null,
			bool? isTaxable = null,
			decimal? defaultAmount = null,
			string defaultDescription = null,
			int? rateTypeId = null,
			int? currencyId = null,
			int? sequence = null,
			string color = null,
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

			IQueryable<Database.ChargeType> query = (from ct in _context.ChargeTypes select ct);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ct => ct.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ct => ct.description == description);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(ct => ct.externalId == externalId);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(ct => ct.isRevenue == isRevenue.Value);
			}
			if (isTaxable.HasValue == true)
			{
				query = query.Where(ct => ct.isTaxable == isTaxable.Value);
			}
			if (defaultAmount.HasValue == true)
			{
				query = query.Where(ct => ct.defaultAmount == defaultAmount.Value);
			}
			if (string.IsNullOrEmpty(defaultDescription) == false)
			{
				query = query.Where(ct => ct.defaultDescription == defaultDescription);
			}
			if (rateTypeId.HasValue == true)
			{
				query = query.Where(ct => ct.rateTypeId == rateTypeId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(ct => ct.currencyId == currencyId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ct => ct.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(ct => ct.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ct => ct.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ct => ct.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ct => ct.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ct => ct.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ct => ct.deleted == false);
				}
			}
			else
			{
				query = query.Where(ct => ct.active == true);
				query = query.Where(ct => ct.deleted == false);
			}

			query = query.OrderBy(ct => ct.sequence).ThenBy(ct => ct.name).ThenBy(ct => ct.description).ThenBy(ct => ct.externalId);


			//
			// Add the any string contains parameter to span all the string fields on the Charge Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.defaultDescription.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || (includeRelations == true && x.currency.name.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.description.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.code.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.color.Contains(anyStringContains))
			       || (includeRelations == true && x.rateType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.rateType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.rateType.color.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.currency);
				query = query.Include(x => x.rateType);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ChargeType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ChargeType chargeType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(chargeType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ChargeType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ChargeType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ChargeTypes filtered by the parameters provided.  Its query is similar to the GetChargeTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string externalId = null,
			bool? isRevenue = null,
			bool? isTaxable = null,
			decimal? defaultAmount = null,
			string defaultDescription = null,
			int? rateTypeId = null,
			int? currencyId = null,
			int? sequence = null,
			string color = null,
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


			IQueryable<Database.ChargeType> query = (from ct in _context.ChargeTypes select ct);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(ct => ct.name == name);
			}
			if (description != null)
			{
				query = query.Where(ct => ct.description == description);
			}
			if (externalId != null)
			{
				query = query.Where(ct => ct.externalId == externalId);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(ct => ct.isRevenue == isRevenue.Value);
			}
			if (isTaxable.HasValue == true)
			{
				query = query.Where(ct => ct.isTaxable == isTaxable.Value);
			}
			if (defaultAmount.HasValue == true)
			{
				query = query.Where(ct => ct.defaultAmount == defaultAmount.Value);
			}
			if (defaultDescription != null)
			{
				query = query.Where(ct => ct.defaultDescription == defaultDescription);
			}
			if (rateTypeId.HasValue == true)
			{
				query = query.Where(ct => ct.rateTypeId == rateTypeId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(ct => ct.currencyId == currencyId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ct => ct.sequence == sequence.Value);
			}
			if (color != null)
			{
				query = query.Where(ct => ct.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ct => ct.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ct => ct.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ct => ct.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ct => ct.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ct => ct.deleted == false);
				}
			}
			else
			{
				query = query.Where(ct => ct.active == true);
				query = query.Where(ct => ct.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Charge Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.defaultDescription.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.rateType.name.Contains(anyStringContains)
			       || x.rateType.description.Contains(anyStringContains)
			       || x.rateType.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ChargeType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeType/{id}")]
		public async Task<IActionResult> GetChargeType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ChargeType> query = (from ct in _context.ChargeTypes where
							(ct.id == id) &&
							(userIsAdmin == true || ct.deleted == false) &&
							(userIsWriter == true || ct.active == true)
					select ct);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.currency);
					query = query.Include(x => x.rateType);
					query = query.AsSplitQuery();
				}

				Database.ChargeType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ChargeType Entity was read with Admin privilege." : "Scheduler.ChargeType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ChargeType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ChargeType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ChargeType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ChargeType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ChargeType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ChargeType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutChargeType(int id, [FromBody]Database.ChargeType.ChargeTypeDTO chargeTypeDTO, CancellationToken cancellationToken = default)
		{
			if (chargeTypeDTO == null)
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



			if (id != chargeTypeDTO.id)
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


			IQueryable<Database.ChargeType> query = (from x in _context.ChargeTypes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ChargeType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ChargeType PUT", id.ToString(), new Exception("No Scheduler.ChargeType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (chargeTypeDTO.objectGuid == Guid.Empty)
            {
                chargeTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (chargeTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ChargeType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ChargeType cloneOfExisting = (Database.ChargeType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ChargeType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ChargeType chargeType = (Database.ChargeType)_context.Entry(existing).GetDatabaseValues().ToObject();
			chargeType.ApplyDTO(chargeTypeDTO);
			//
			// The tenant guid for any ChargeType being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ChargeType because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				chargeType.tenantGuid = existing.tenantGuid;
			}

			lock (chargeTypePutSyncRoot)
			{
				//
				// Validate the version number for the chargeType being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != chargeType.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ChargeType save attempt was made but save request was with version " + chargeType.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ChargeType you are trying to update has already changed.  Please try your save again after reloading the ChargeType.");
				}
				else
				{
					// Same record.  Increase version.
					chargeType.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (chargeType.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ChargeType record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (chargeType.name != null && chargeType.name.Length > 100)
				{
					chargeType.name = chargeType.name.Substring(0, 100);
				}

				if (chargeType.description != null && chargeType.description.Length > 500)
				{
					chargeType.description = chargeType.description.Substring(0, 500);
				}

				if (chargeType.externalId != null && chargeType.externalId.Length > 100)
				{
					chargeType.externalId = chargeType.externalId.Substring(0, 100);
				}

				if (chargeType.defaultDescription != null && chargeType.defaultDescription.Length > 500)
				{
					chargeType.defaultDescription = chargeType.defaultDescription.Substring(0, 500);
				}

				if (chargeType.color != null && chargeType.color.Length > 10)
				{
					chargeType.color = chargeType.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.ChargeType> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(chargeType);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ChargeTypeChangeHistory chargeTypeChangeHistory = new ChargeTypeChangeHistory();
				        chargeTypeChangeHistory.chargeTypeId = chargeType.id;
				        chargeTypeChangeHistory.versionNumber = chargeType.versionNumber;
				        chargeTypeChangeHistory.timeStamp = DateTime.UtcNow;
				        chargeTypeChangeHistory.userId = securityUser.id;
				        chargeTypeChangeHistory.tenantGuid = userTenantGuid;
				        chargeTypeChangeHistory.data = JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType));
				        _context.ChargeTypeChangeHistories.Add(chargeTypeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ChargeType entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType)),
						null);

				return Ok(Database.ChargeType.CreateAnonymous(chargeType));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ChargeType entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ChargeType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeType", Name = "ChargeType")]
		public async Task<IActionResult> PostChargeType([FromBody]Database.ChargeType.ChargeTypeDTO chargeTypeDTO, CancellationToken cancellationToken = default)
		{
			if (chargeTypeDTO == null)
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
			// Create a new ChargeType object using the data from the DTO
			//
			Database.ChargeType chargeType = Database.ChargeType.FromDTO(chargeTypeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				chargeType.tenantGuid = userTenantGuid;

				if (chargeType.name != null && chargeType.name.Length > 100)
				{
					chargeType.name = chargeType.name.Substring(0, 100);
				}

				if (chargeType.description != null && chargeType.description.Length > 500)
				{
					chargeType.description = chargeType.description.Substring(0, 500);
				}

				if (chargeType.externalId != null && chargeType.externalId.Length > 100)
				{
					chargeType.externalId = chargeType.externalId.Substring(0, 100);
				}

				if (chargeType.defaultDescription != null && chargeType.defaultDescription.Length > 500)
				{
					chargeType.defaultDescription = chargeType.defaultDescription.Substring(0, 500);
				}

				if (chargeType.color != null && chargeType.color.Length > 10)
				{
					chargeType.color = chargeType.color.Substring(0, 10);
				}

				chargeType.objectGuid = Guid.NewGuid();
				chargeType.versionNumber = 1;

				_context.ChargeTypes.Add(chargeType);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the chargeType object so that no further changes will be written to the database
				    //
				    _context.Entry(chargeType).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					chargeType.ChargeTypeChangeHistories = null;
					chargeType.EventCharges = null;
					chargeType.EventResourceAssignments = null;
					chargeType.ScheduledEventTemplateCharges = null;
					chargeType.currency = null;
					chargeType.rateType = null;


				    ChargeTypeChangeHistory chargeTypeChangeHistory = new ChargeTypeChangeHistory();
				    chargeTypeChangeHistory.chargeTypeId = chargeType.id;
				    chargeTypeChangeHistory.versionNumber = chargeType.versionNumber;
				    chargeTypeChangeHistory.timeStamp = DateTime.UtcNow;
				    chargeTypeChangeHistory.userId = securityUser.id;
				    chargeTypeChangeHistory.tenantGuid = userTenantGuid;
				    chargeTypeChangeHistory.data = JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType));
				    _context.ChargeTypeChangeHistories.Add(chargeTypeChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ChargeType entity successfully created.",
						true,
						chargeType. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ChargeType entity creation failed.", false, chargeType.id.ToString(), "", JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ChargeType", chargeType.id, chargeType.name));

			return CreatedAtRoute("ChargeType", new { id = chargeType.id }, Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType));
		}



        /// <summary>
        /// 
        /// This rolls a ChargeType entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeType/Rollback/{id}")]
		[Route("api/ChargeType/Rollback")]
		public async Task<IActionResult> RollbackToChargeTypeVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ChargeType> query = (from x in _context.ChargeTypes
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ChargeType concurrently
			//
			lock (chargeTypePutSyncRoot)
			{
				
				Database.ChargeType chargeType = query.FirstOrDefault();
				
				if (chargeType == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ChargeType rollback", id.ToString(), new Exception("No Scheduler.ChargeType entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ChargeType current state so we can log it.
				//
				Database.ChargeType cloneOfExisting = (Database.ChargeType)_context.Entry(chargeType).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ChargeTypeChangeHistories = null;
				cloneOfExisting.EventCharges = null;
				cloneOfExisting.EventResourceAssignments = null;
				cloneOfExisting.ScheduledEventTemplateCharges = null;
				cloneOfExisting.currency = null;
				cloneOfExisting.rateType = null;

				if (versionNumber >= chargeType.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ChargeType rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ChargeType rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ChargeTypeChangeHistory chargeTypeChangeHistory = (from x in _context.ChargeTypeChangeHistories
				                                               where
				                                               x.chargeTypeId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (chargeTypeChangeHistory != null)
				{
				    Database.ChargeType oldChargeType = JsonSerializer.Deserialize<Database.ChargeType>(chargeTypeChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    chargeType.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    chargeType.name = oldChargeType.name;
				    chargeType.description = oldChargeType.description;
				    chargeType.externalId = oldChargeType.externalId;
				    chargeType.isRevenue = oldChargeType.isRevenue;
				    chargeType.isTaxable = oldChargeType.isTaxable;
				    chargeType.defaultAmount = oldChargeType.defaultAmount;
				    chargeType.defaultDescription = oldChargeType.defaultDescription;
				    chargeType.rateTypeId = oldChargeType.rateTypeId;
				    chargeType.currencyId = oldChargeType.currencyId;
				    chargeType.sequence = oldChargeType.sequence;
				    chargeType.color = oldChargeType.color;
				    chargeType.objectGuid = oldChargeType.objectGuid;
				    chargeType.active = oldChargeType.active;
				    chargeType.deleted = oldChargeType.deleted;

				    string serializedChargeType = JsonSerializer.Serialize(chargeType);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ChargeTypeChangeHistory newChargeTypeChangeHistory = new ChargeTypeChangeHistory();
				        newChargeTypeChangeHistory.chargeTypeId = chargeType.id;
				        newChargeTypeChangeHistory.versionNumber = chargeType.versionNumber;
				        newChargeTypeChangeHistory.timeStamp = DateTime.UtcNow;
				        newChargeTypeChangeHistory.userId = securityUser.id;
				        newChargeTypeChangeHistory.tenantGuid = userTenantGuid;
				        newChargeTypeChangeHistory.data = JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType));
				        _context.ChargeTypeChangeHistories.Add(newChargeTypeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ChargeType rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType)),
						null);


				    return Ok(Database.ChargeType.CreateAnonymous(chargeType));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ChargeType rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ChargeType rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ChargeType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ChargeType</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeType/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetChargeTypeChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ChargeType chargeType = await _context.ChargeTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (chargeType == null)
			{
				return NotFound();
			}

			try
			{
				chargeType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ChargeType> versionInfo = await chargeType.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ChargeType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ChargeType</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeType/{id}/AuditHistory")]
		public async Task<IActionResult> GetChargeTypeAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ChargeType chargeType = await _context.ChargeTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (chargeType == null)
			{
				return NotFound();
			}

			try
			{
				chargeType.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ChargeType>> versions = await chargeType.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ChargeType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ChargeType</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ChargeType object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeType/{id}/Version/{version}")]
		public async Task<IActionResult> GetChargeTypeVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ChargeType chargeType = await _context.ChargeTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (chargeType == null)
			{
				return NotFound();
			}

			try
			{
				chargeType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ChargeType> versionInfo = await chargeType.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ChargeType at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ChargeType</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ChargeType object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeType/{id}/StateAtTime")]
		public async Task<IActionResult> GetChargeTypeStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ChargeType chargeType = await _context.ChargeTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (chargeType == null)
			{
				return NotFound();
			}

			try
			{
				chargeType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ChargeType> versionInfo = await chargeType.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ChargeType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeType/{id}")]
		[Route("api/ChargeType")]
		public async Task<IActionResult> DeleteChargeType(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ChargeType> query = (from x in _context.ChargeTypes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ChargeType chargeType = await query.FirstOrDefaultAsync(cancellationToken);

			if (chargeType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ChargeType DELETE", id.ToString(), new Exception("No Scheduler.ChargeType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ChargeType cloneOfExisting = (Database.ChargeType)_context.Entry(chargeType).GetDatabaseValues().ToObject();


			lock (chargeTypeDeleteSyncRoot)
			{
			    try
			    {
			        chargeType.deleted = true;
			        chargeType.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ChargeTypeChangeHistory chargeTypeChangeHistory = new ChargeTypeChangeHistory();
			        chargeTypeChangeHistory.chargeTypeId = chargeType.id;
			        chargeTypeChangeHistory.versionNumber = chargeType.versionNumber;
			        chargeTypeChangeHistory.timeStamp = DateTime.UtcNow;
			        chargeTypeChangeHistory.userId = securityUser.id;
			        chargeTypeChangeHistory.tenantGuid = userTenantGuid;
			        chargeTypeChangeHistory.data = JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType));
			        _context.ChargeTypeChangeHistories.Add(chargeTypeChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ChargeType entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ChargeType entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ChargeType.CreateAnonymousWithFirstLevelSubObjects(chargeType)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ChargeType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ChargeTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string externalId = null,
			bool? isRevenue = null,
			bool? isTaxable = null,
			decimal? defaultAmount = null,
			string defaultDescription = null,
			int? rateTypeId = null,
			int? currencyId = null,
			int? sequence = null,
			string color = null,
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

			IQueryable<Database.ChargeType> query = (from ct in _context.ChargeTypes select ct);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ct => ct.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ct => ct.description == description);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(ct => ct.externalId == externalId);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(ct => ct.isRevenue == isRevenue.Value);
			}
			if (isTaxable.HasValue == true)
			{
				query = query.Where(ct => ct.isTaxable == isTaxable.Value);
			}
			if (defaultAmount.HasValue == true)
			{
				query = query.Where(ct => ct.defaultAmount == defaultAmount.Value);
			}
			if (string.IsNullOrEmpty(defaultDescription) == false)
			{
				query = query.Where(ct => ct.defaultDescription == defaultDescription);
			}
			if (rateTypeId.HasValue == true)
			{
				query = query.Where(ct => ct.rateTypeId == rateTypeId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(ct => ct.currencyId == currencyId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ct => ct.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(ct => ct.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ct => ct.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ct => ct.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ct => ct.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ct => ct.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ct => ct.deleted == false);
				}
			}
			else
			{
				query = query.Where(ct => ct.active == true);
				query = query.Where(ct => ct.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Charge Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.defaultDescription.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.rateType.name.Contains(anyStringContains)
			       || x.rateType.description.Contains(anyStringContains)
			       || x.rateType.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.externalId);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ChargeType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ChargeType/CreateAuditEvent")]
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
