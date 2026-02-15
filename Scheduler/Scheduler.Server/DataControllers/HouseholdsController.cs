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
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Household entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Household entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class HouseholdsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object householdPutSyncRoot = new object();
		static object householdDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<HouseholdsController> _logger;

		public HouseholdsController(SchedulerContext context, ILogger<HouseholdsController> logger) : base("Scheduler", "Household")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Households filtered by the parameters provided.
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
		[Route("api/Households")]
		public async Task<IActionResult> GetHouseholds(
			string name = null,
			string description = null,
			int? schedulingTargetId = null,
			string formalSalutation = null,
			string informalSalutation = null,
			string addressee = null,
			decimal? totalHouseholdGiving = null,
			string notes = null,
			int? iconId = null,
			string color = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
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

			IQueryable<Database.Household> query = (from h in _context.Households select h);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(h => h.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(h => h.description == description);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(h => h.schedulingTargetId == schedulingTargetId.Value);
			}
			if (string.IsNullOrEmpty(formalSalutation) == false)
			{
				query = query.Where(h => h.formalSalutation == formalSalutation);
			}
			if (string.IsNullOrEmpty(informalSalutation) == false)
			{
				query = query.Where(h => h.informalSalutation == informalSalutation);
			}
			if (string.IsNullOrEmpty(addressee) == false)
			{
				query = query.Where(h => h.addressee == addressee);
			}
			if (totalHouseholdGiving.HasValue == true)
			{
				query = query.Where(h => h.totalHouseholdGiving == totalHouseholdGiving.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(h => h.notes == notes);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(h => h.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(h => h.color == color);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(h => h.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(h => h.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(h => h.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(h => h.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(h => h.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(h => h.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(h => h.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(h => h.deleted == false);
				}
			}
			else
			{
				query = query.Where(h => h.active == true);
				query = query.Where(h => h.deleted == false);
			}

			query = query.OrderBy(h => h.name).ThenBy(h => h.description).ThenBy(h => h.formalSalutation);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.icon);
				query = query.Include(x => x.schedulingTarget);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Household, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.formalSalutation.Contains(anyStringContains)
			       || x.informalSalutation.Contains(anyStringContains)
			       || x.addressee.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.name.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.description.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.color.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.avatarMimeType.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Household> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Household household in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(household, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async household =>
				{

					if (household.avatarData == null &&
					    household.avatarSize.HasValue == true &&
					    household.avatarSize.Value > 0)
					{
					    household.avatarData = await LoadDataFromDiskAsync(household.objectGuid, household.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Household Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Household Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Households filtered by the parameters provided.  Its query is similar to the GetHouseholds method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Households/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? schedulingTargetId = null,
			string formalSalutation = null,
			string informalSalutation = null,
			string addressee = null,
			decimal? totalHouseholdGiving = null,
			string notes = null,
			int? iconId = null,
			string color = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
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


			IQueryable<Database.Household> query = (from h in _context.Households select h);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(h => h.name == name);
			}
			if (description != null)
			{
				query = query.Where(h => h.description == description);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(h => h.schedulingTargetId == schedulingTargetId.Value);
			}
			if (formalSalutation != null)
			{
				query = query.Where(h => h.formalSalutation == formalSalutation);
			}
			if (informalSalutation != null)
			{
				query = query.Where(h => h.informalSalutation == informalSalutation);
			}
			if (addressee != null)
			{
				query = query.Where(h => h.addressee == addressee);
			}
			if (totalHouseholdGiving.HasValue == true)
			{
				query = query.Where(h => h.totalHouseholdGiving == totalHouseholdGiving.Value);
			}
			if (notes != null)
			{
				query = query.Where(h => h.notes == notes);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(h => h.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(h => h.color == color);
			}
			if (avatarFileName != null)
			{
				query = query.Where(h => h.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(h => h.avatarSize == avatarSize.Value);
			}
			if (avatarMimeType != null)
			{
				query = query.Where(h => h.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(h => h.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(h => h.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(h => h.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(h => h.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(h => h.deleted == false);
				}
			}
			else
			{
				query = query.Where(h => h.active == true);
				query = query.Where(h => h.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Household, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.formalSalutation.Contains(anyStringContains)
			       || x.informalSalutation.Contains(anyStringContains)
			       || x.addressee.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			       || x.schedulingTarget.name.Contains(anyStringContains)
			       || x.schedulingTarget.description.Contains(anyStringContains)
			       || x.schedulingTarget.notes.Contains(anyStringContains)
			       || x.schedulingTarget.externalId.Contains(anyStringContains)
			       || x.schedulingTarget.color.Contains(anyStringContains)
			       || x.schedulingTarget.attributes.Contains(anyStringContains)
			       || x.schedulingTarget.avatarFileName.Contains(anyStringContains)
			       || x.schedulingTarget.avatarMimeType.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Household by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Household/{id}")]
		public async Task<IActionResult> GetHousehold(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Household> query = (from h in _context.Households where
							(h.id == id) &&
							(userIsAdmin == true || h.deleted == false) &&
							(userIsWriter == true || h.active == true)
					select h);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.icon);
					query = query.Include(x => x.schedulingTarget);
					query = query.AsSplitQuery();
				}

				Database.Household materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.avatarData == null &&
					    materialized.avatarSize.HasValue == true &&
					    materialized.avatarSize.Value > 0)
					{
					    materialized.avatarData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Household Entity was read with Admin privilege." : "Scheduler.Household Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Household", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Household entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Household.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Household.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Household record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Household/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PutHousehold(int id, [FromBody]Database.Household.HouseholdDTO householdDTO, CancellationToken cancellationToken = default)
		{
			if (householdDTO == null)
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



			if (id != householdDTO.id)
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


			IQueryable<Database.Household> query = (from x in _context.Households
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Household existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Household PUT", id.ToString(), new Exception("No Scheduler.Household entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (householdDTO.objectGuid == Guid.Empty)
            {
                householdDTO.objectGuid = existing.objectGuid;
            }
            else if (householdDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Household record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Household cloneOfExisting = (Database.Household)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Household object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Household household = (Database.Household)_context.Entry(existing).GetDatabaseValues().ToObject();
			household.ApplyDTO(householdDTO);
			//
			// The tenant guid for any Household being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Household because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				household.tenantGuid = existing.tenantGuid;
			}

			lock (householdPutSyncRoot)
			{
				//
				// Validate the version number for the household being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != household.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Household save attempt was made but save request was with version " + household.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Household you are trying to update has already changed.  Please try your save again after reloading the Household.");
				}
				else
				{
					// Same record.  Increase version.
					household.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (household.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Household record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (household.name != null && household.name.Length > 100)
				{
					household.name = household.name.Substring(0, 100);
				}

				if (household.description != null && household.description.Length > 500)
				{
					household.description = household.description.Substring(0, 500);
				}

				if (household.formalSalutation != null && household.formalSalutation.Length > 250)
				{
					household.formalSalutation = household.formalSalutation.Substring(0, 250);
				}

				if (household.informalSalutation != null && household.informalSalutation.Length > 250)
				{
					household.informalSalutation = household.informalSalutation.Substring(0, 250);
				}

				if (household.addressee != null && household.addressee.Length > 250)
				{
					household.addressee = household.addressee.Substring(0, 250);
				}

				if (household.color != null && household.color.Length > 10)
				{
					household.color = household.color.Substring(0, 10);
				}

				if (household.avatarFileName != null && household.avatarFileName.Length > 250)
				{
					household.avatarFileName = household.avatarFileName.Substring(0, 250);
				}

				if (household.avatarMimeType != null && household.avatarMimeType.Length > 100)
				{
					household.avatarMimeType = household.avatarMimeType.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (household.avatarData != null && string.IsNullOrEmpty(household.avatarFileName))
				{
				    household.avatarFileName = household.objectGuid.ToString() + ".data";
				}

				if (household.avatarData != null && (household.avatarSize.HasValue == false || household.avatarSize != household.avatarData.Length))
				{
				    household.avatarSize = household.avatarData.Length;
				}

				if (household.avatarData != null && string.IsNullOrEmpty(household.avatarMimeType))
				{
				    household.avatarMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = household.avatarData;

					if (diskBasedBinaryStorageMode == true &&
					    household.avatarFileName != null &&
					    household.avatarData != null &&
					    household.avatarSize.HasValue == true &&
					    household.avatarSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(household.objectGuid, household.versionNumber, household.avatarData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    household.avatarData = null;

					}

				    EntityEntry<Database.Household> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(household);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        HouseholdChangeHistory householdChangeHistory = new HouseholdChangeHistory();
				        householdChangeHistory.householdId = household.id;
				        householdChangeHistory.versionNumber = household.versionNumber;
				        householdChangeHistory.timeStamp = DateTime.UtcNow;
				        householdChangeHistory.userId = securityUser.id;
				        householdChangeHistory.tenantGuid = userTenantGuid;
				        householdChangeHistory.data = JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household));
				        _context.HouseholdChangeHistories.Add(householdChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    household.avatarData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Household entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household)),
						null);

				return Ok(Database.Household.CreateAnonymous(household));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Household entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Household record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Household", Name = "Household")]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PostHousehold([FromBody]Database.Household.HouseholdDTO householdDTO, CancellationToken cancellationToken = default)
		{
			if (householdDTO == null)
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
			// Create a new Household object using the data from the DTO
			//
			Database.Household household = Database.Household.FromDTO(householdDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				household.tenantGuid = userTenantGuid;

				if (household.name != null && household.name.Length > 100)
				{
					household.name = household.name.Substring(0, 100);
				}

				if (household.description != null && household.description.Length > 500)
				{
					household.description = household.description.Substring(0, 500);
				}

				if (household.formalSalutation != null && household.formalSalutation.Length > 250)
				{
					household.formalSalutation = household.formalSalutation.Substring(0, 250);
				}

				if (household.informalSalutation != null && household.informalSalutation.Length > 250)
				{
					household.informalSalutation = household.informalSalutation.Substring(0, 250);
				}

				if (household.addressee != null && household.addressee.Length > 250)
				{
					household.addressee = household.addressee.Substring(0, 250);
				}

				if (household.color != null && household.color.Length > 10)
				{
					household.color = household.color.Substring(0, 10);
				}

				if (household.avatarFileName != null && household.avatarFileName.Length > 250)
				{
					household.avatarFileName = household.avatarFileName.Substring(0, 250);
				}

				if (household.avatarMimeType != null && household.avatarMimeType.Length > 100)
				{
					household.avatarMimeType = household.avatarMimeType.Substring(0, 100);
				}

				household.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (household.avatarData != null && string.IsNullOrEmpty(household.avatarFileName))
				{
				    household.avatarFileName = household.objectGuid.ToString() + ".data";
				}

				if (household.avatarData != null && (household.avatarSize.HasValue == false || household.avatarSize != household.avatarData.Length))
				{
				    household.avatarSize = household.avatarData.Length;
				}

				if (household.avatarData != null && string.IsNullOrEmpty(household.avatarMimeType))
				{
				    household.avatarMimeType = "application/octet-stream";
				}

				household.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = household.avatarData;

				if (diskBasedBinaryStorageMode == true &&
				    household.avatarData != null &&
				    household.avatarFileName != null &&
				    household.avatarSize.HasValue == true &&
				    household.avatarSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(household.objectGuid, household.versionNumber, household.avatarData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    household.avatarData = null;

				}

				_context.Households.Add(household);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the household object so that no further changes will be written to the database
				    //
				    _context.Entry(household).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					household.avatarData = null;
					household.Constituents = null;
					household.HouseholdChangeHistories = null;
					household.icon = null;
					household.schedulingTarget = null;


				    HouseholdChangeHistory householdChangeHistory = new HouseholdChangeHistory();
				    householdChangeHistory.householdId = household.id;
				    householdChangeHistory.versionNumber = household.versionNumber;
				    householdChangeHistory.timeStamp = DateTime.UtcNow;
				    householdChangeHistory.userId = securityUser.id;
				    householdChangeHistory.tenantGuid = userTenantGuid;
				    householdChangeHistory.data = JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household));
				    _context.HouseholdChangeHistories.Add(householdChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Household entity successfully created.",
						true,
						household. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    household.avatarData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Household entity creation failed.", false, household.id.ToString(), "", JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Household", household.id, household.name));

			return CreatedAtRoute("Household", new { id = household.id }, Database.Household.CreateAnonymousWithFirstLevelSubObjects(household));
		}



        /// <summary>
        /// 
        /// This rolls a Household entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Household/Rollback/{id}")]
		[Route("api/Household/Rollback")]
		public async Task<IActionResult> RollbackToHouseholdVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Household> query = (from x in _context.Households
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this Household concurrently
			//
			lock (householdPutSyncRoot)
			{
				
				Database.Household household = query.FirstOrDefault();
				
				if (household == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Household rollback", id.ToString(), new Exception("No Scheduler.Household entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Household current state so we can log it.
				//
				Database.Household cloneOfExisting = (Database.Household)_context.Entry(household).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.avatarData = null;
				cloneOfExisting.Constituents = null;
				cloneOfExisting.HouseholdChangeHistories = null;
				cloneOfExisting.icon = null;
				cloneOfExisting.schedulingTarget = null;

				if (versionNumber >= household.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Household rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Household rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				HouseholdChangeHistory householdChangeHistory = (from x in _context.HouseholdChangeHistories
				                                               where
				                                               x.householdId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (householdChangeHistory != null)
				{
				    Database.Household oldHousehold = JsonSerializer.Deserialize<Database.Household>(householdChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    household.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    household.name = oldHousehold.name;
				    household.description = oldHousehold.description;
				    household.schedulingTargetId = oldHousehold.schedulingTargetId;
				    household.formalSalutation = oldHousehold.formalSalutation;
				    household.informalSalutation = oldHousehold.informalSalutation;
				    household.addressee = oldHousehold.addressee;
				    household.totalHouseholdGiving = oldHousehold.totalHouseholdGiving;
				    household.lastHouseholdGiftDate = oldHousehold.lastHouseholdGiftDate;
				    household.notes = oldHousehold.notes;
				    household.iconId = oldHousehold.iconId;
				    household.color = oldHousehold.color;
				    household.avatarFileName = oldHousehold.avatarFileName;
				    household.avatarSize = oldHousehold.avatarSize;
				    household.avatarData = oldHousehold.avatarData;
				    household.avatarMimeType = oldHousehold.avatarMimeType;
				    household.objectGuid = oldHousehold.objectGuid;
				    household.active = oldHousehold.active;
				    household.deleted = oldHousehold.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldHousehold.objectGuid, oldHousehold.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(household.objectGuid, household.versionNumber, binaryData, "data");
				    }

				    string serializedHousehold = JsonSerializer.Serialize(household);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        HouseholdChangeHistory newHouseholdChangeHistory = new HouseholdChangeHistory();
				        newHouseholdChangeHistory.householdId = household.id;
				        newHouseholdChangeHistory.versionNumber = household.versionNumber;
				        newHouseholdChangeHistory.timeStamp = DateTime.UtcNow;
				        newHouseholdChangeHistory.userId = securityUser.id;
				        newHouseholdChangeHistory.tenantGuid = userTenantGuid;
				        newHouseholdChangeHistory.data = JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household));
				        _context.HouseholdChangeHistories.Add(newHouseholdChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Household rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household)),
						null);


				    return Ok(Database.Household.CreateAnonymous(household));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Household rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Household rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Household.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Household</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Household/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetHouseholdChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Household household = await _context.Households.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (household == null)
			{
				return NotFound();
			}

			try
			{
				household.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Household> versionInfo = await household.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Household.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Household</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Household/{id}/AuditHistory")]
		public async Task<IActionResult> GetHouseholdAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Household household = await _context.Households.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (household == null)
			{
				return NotFound();
			}

			try
			{
				household.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Household>> versions = await household.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Household.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Household</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Household object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Household/{id}/Version/{version}")]
		public async Task<IActionResult> GetHouseholdVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Household household = await _context.Households.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (household == null)
			{
				return NotFound();
			}

			try
			{
				household.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Household> versionInfo = await household.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Household at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Household</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Household object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Household/{id}/StateAtTime")]
		public async Task<IActionResult> GetHouseholdStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Household household = await _context.Households.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (household == null)
			{
				return NotFound();
			}

			try
			{
				household.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Household> versionInfo = await household.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Household record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Household/{id}")]
		[Route("api/Household")]
		public async Task<IActionResult> DeleteHousehold(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Household> query = (from x in _context.Households
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Household household = await query.FirstOrDefaultAsync(cancellationToken);

			if (household == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Household DELETE", id.ToString(), new Exception("No Scheduler.Household entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Household cloneOfExisting = (Database.Household)_context.Entry(household).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (householdDeleteSyncRoot)
			{
			    try
			    {
			        household.deleted = true;
			        household.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(household.objectGuid, household.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(household.objectGuid, household.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        HouseholdChangeHistory householdChangeHistory = new HouseholdChangeHistory();
			        householdChangeHistory.householdId = household.id;
			        householdChangeHistory.versionNumber = household.versionNumber;
			        householdChangeHistory.timeStamp = DateTime.UtcNow;
			        householdChangeHistory.userId = securityUser.id;
			        householdChangeHistory.tenantGuid = userTenantGuid;
			        householdChangeHistory.data = JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household));
			        _context.HouseholdChangeHistories.Add(householdChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Household entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Household entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Household records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Households/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? schedulingTargetId = null,
			string formalSalutation = null,
			string informalSalutation = null,
			string addressee = null,
			decimal? totalHouseholdGiving = null,
			string notes = null,
			int? iconId = null,
			string color = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
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

			IQueryable<Database.Household> query = (from h in _context.Households select h);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(h => h.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(h => h.description == description);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(h => h.schedulingTargetId == schedulingTargetId.Value);
			}
			if (string.IsNullOrEmpty(formalSalutation) == false)
			{
				query = query.Where(h => h.formalSalutation == formalSalutation);
			}
			if (string.IsNullOrEmpty(informalSalutation) == false)
			{
				query = query.Where(h => h.informalSalutation == informalSalutation);
			}
			if (string.IsNullOrEmpty(addressee) == false)
			{
				query = query.Where(h => h.addressee == addressee);
			}
			if (totalHouseholdGiving.HasValue == true)
			{
				query = query.Where(h => h.totalHouseholdGiving == totalHouseholdGiving.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(h => h.notes == notes);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(h => h.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(h => h.color == color);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(h => h.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(h => h.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(h => h.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(h => h.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(h => h.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(h => h.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(h => h.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(h => h.deleted == false);
				}
			}
			else
			{
				query = query.Where(h => h.active == true);
				query = query.Where(h => h.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Household, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.formalSalutation.Contains(anyStringContains)
			       || x.informalSalutation.Contains(anyStringContains)
			       || x.addressee.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			       || x.schedulingTarget.name.Contains(anyStringContains)
			       || x.schedulingTarget.description.Contains(anyStringContains)
			       || x.schedulingTarget.notes.Contains(anyStringContains)
			       || x.schedulingTarget.externalId.Contains(anyStringContains)
			       || x.schedulingTarget.color.Contains(anyStringContains)
			       || x.schedulingTarget.attributes.Contains(anyStringContains)
			       || x.schedulingTarget.avatarFileName.Contains(anyStringContains)
			       || x.schedulingTarget.avatarMimeType.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.formalSalutation);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Household.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Household/CreateAuditEvent")]
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


        /// <summary>
        /// 
        /// This makes a Household record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Household/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPut]
		public async Task<IActionResult> SetFavourite(int id, string description = null, CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(cancellationToken);

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


			IQueryable<Database.Household> query = (from x in _context.Households
			                               where x.id == id
			                               select x);


			Database.Household household = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (household != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = household.name;
				}

				//
				// Add the user favourite Household
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "Household", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Household' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Household' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a Household record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Household/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpDelete]
		public async Task<IActionResult> DeleteFavourite(int id, CancellationToken cancellationToken = default)
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


			//
			// Delete the user favourite Household
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "Household", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Household' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}




        [Route("api/Household/Data/{id:int}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> UploadData(int id, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			MediaTypeHeaderValue mediaTypeHeader; 

            if (!HttpContext.Request.HasFormContentType ||
				!MediaTypeHeaderValue.TryParse(HttpContext.Request.ContentType, out mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }


            Database.Household household = await (from x in _context.Households where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (household == null)
            {
                return NotFound();
            }

            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


            // This will be used to signal whether we are saving data or clearing it.
            bool foundFileData = false;


            //
            // This will get the first file from the request and save it
            //
			try
			{
                MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, HttpContext.Request.Body);
                MultipartSection section = await reader.ReadNextSectionAsync();

                while (section != null)
				{
					ContentDispositionHeaderValue contentDisposition;

					bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);


					if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
						!string.IsNullOrEmpty(contentDisposition.FileName.Value))
					{

						foundFileData = true;
						string fileName = contentDisposition.FileName.ToString().Trim('"');

						// default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.
						MediaTypeHeaderValue mediaType;
						bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);

						string mimeType = "application/octet-stream";
						if (hasMediaTypeHeader && mediaTypeHeader.MediaType != null )
						{
							mimeType = mediaTypeHeader.MediaType.ToString();
						}

						lock (householdPutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									household.avatarFileName = fileName.Trim();
									household.avatarMimeType = mimeType;
									household.avatarSize = section.Body.Length;

									household.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 household.avatarFileName != null &&
										 household.avatarSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(household.objectGuid, household.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										household.avatarData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											household.avatarData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									HouseholdChangeHistory householdChangeHistory = new HouseholdChangeHistory();
									householdChangeHistory.householdId = household.id;
									householdChangeHistory.versionNumber = household.versionNumber;
									householdChangeHistory.timeStamp = DateTime.UtcNow;
									householdChangeHistory.userId = securityUser.id;
									householdChangeHistory.tenantGuid = household.tenantGuid;
									householdChangeHistory.data = JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household));
									_context.HouseholdChangeHistories.Add(householdChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Household Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Household Data Upload Failed.", false, id.ToString(), "", "", ex);

								return Problem(ex.Message);
							}
						}


						//
						// Stop looking for more files.
						//
						break;
					}

					section = await reader.ReadNextSectionAsync();
				}
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Caught error in UploadData handler", id.ToString(), ex);

                return Problem(ex.Message);
            }

            //
            // Treat the situation where we have a valid ID but no file content as a request to clear the data
            //
            if (foundFileData == false)
            {
                lock (householdPutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(household.objectGuid, household.versionNumber, "data");
                            }

                            household.avatarFileName = null;
                            household.avatarMimeType = null;
                            household.avatarSize = 0;
                            household.avatarData = null;
                            household.versionNumber++;


                            //
                            // Now add the change history
                            //
                            HouseholdChangeHistory householdChangeHistory = new HouseholdChangeHistory();
                            householdChangeHistory.householdId = household.id;
                            householdChangeHistory.versionNumber = household.versionNumber;
                            householdChangeHistory.timeStamp = DateTime.UtcNow;
                            householdChangeHistory.userId = securityUser.id;
                                    householdChangeHistory.tenantGuid = household.tenantGuid;
                                    householdChangeHistory.data = JsonSerializer.Serialize(Database.Household.CreateAnonymousWithFirstLevelSubObjects(household));
                            _context.HouseholdChangeHistories.Add(householdChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Household data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Household data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Household/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id, CancellationToken cancellationToken = default)
        {

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			using (SchedulerContext context = new SchedulerContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.Household household = await (from d in context.Households
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (household != null && household.avatarData != null)
                {
                   return File(household.avatarData.ToArray<byte>(), household.avatarMimeType, household.avatarFileName != null ? household.avatarFileName.Trim() : "Household_" + household.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
