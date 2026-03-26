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
    /// This auto generated class provides the basic CRUD operations for the ConstituentJourneyStage entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ConstituentJourneyStage entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ConstituentJourneyStagesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 60;

		static object constituentJourneyStagePutSyncRoot = new object();
		static object constituentJourneyStageDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ConstituentJourneyStagesController> _logger;

		public ConstituentJourneyStagesController(SchedulerContext context, ILogger<ConstituentJourneyStagesController> logger) : base("Scheduler", "ConstituentJourneyStage")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ConstituentJourneyStages filtered by the parameters provided.
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
		[Route("api/ConstituentJourneyStages")]
		public async Task<IActionResult> GetConstituentJourneyStages(
			string name = null,
			string description = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
			decimal? minLifetimeGiving = null,
			decimal? maxLifetimeGiving = null,
			decimal? minSingleGiftAmount = null,
			bool? isDefault = null,
			decimal? minAnnualGiving = null,
			int? maxDaysSinceLastGift = null,
			int? minGiftCount = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 60, cancellationToken);
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

			IQueryable<Database.ConstituentJourneyStage> query = (from cjs in _context.ConstituentJourneyStages select cjs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cjs => cjs.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cjs => cjs.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cjs => cjs.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(cjs => cjs.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(cjs => cjs.color == color);
			}
			if (minLifetimeGiving.HasValue == true)
			{
				query = query.Where(cjs => cjs.minLifetimeGiving == minLifetimeGiving.Value);
			}
			if (maxLifetimeGiving.HasValue == true)
			{
				query = query.Where(cjs => cjs.maxLifetimeGiving == maxLifetimeGiving.Value);
			}
			if (minSingleGiftAmount.HasValue == true)
			{
				query = query.Where(cjs => cjs.minSingleGiftAmount == minSingleGiftAmount.Value);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(cjs => cjs.isDefault == isDefault.Value);
			}
			if (minAnnualGiving.HasValue == true)
			{
				query = query.Where(cjs => cjs.minAnnualGiving == minAnnualGiving.Value);
			}
			if (maxDaysSinceLastGift.HasValue == true)
			{
				query = query.Where(cjs => cjs.maxDaysSinceLastGift == maxDaysSinceLastGift.Value);
			}
			if (minGiftCount.HasValue == true)
			{
				query = query.Where(cjs => cjs.minGiftCount == minGiftCount.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cjs => cjs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cjs => cjs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cjs => cjs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cjs => cjs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cjs => cjs.deleted == false);
				}
			}
			else
			{
				query = query.Where(cjs => cjs.active == true);
				query = query.Where(cjs => cjs.deleted == false);
			}

			query = query.OrderBy(cjs => cjs.sequence).ThenBy(cjs => cjs.name).ThenBy(cjs => cjs.description).ThenBy(cjs => cjs.color);


			//
			// Add the any string contains parameter to span all the string fields on the Constituent Journey Stage, or on an any of the string fields on its immediate relations
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

			if (includeRelations == true)
			{
				query = query.Include(x => x.icon);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ConstituentJourneyStage> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ConstituentJourneyStage constituentJourneyStage in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(constituentJourneyStage, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ConstituentJourneyStage Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ConstituentJourneyStage Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ConstituentJourneyStages filtered by the parameters provided.  Its query is similar to the GetConstituentJourneyStages method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConstituentJourneyStages/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
			decimal? minLifetimeGiving = null,
			decimal? maxLifetimeGiving = null,
			decimal? minSingleGiftAmount = null,
			bool? isDefault = null,
			decimal? minAnnualGiving = null,
			int? maxDaysSinceLastGift = null,
			int? minGiftCount = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 60, cancellationToken);
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


			IQueryable<Database.ConstituentJourneyStage> query = (from cjs in _context.ConstituentJourneyStages select cjs);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(cjs => cjs.name == name);
			}
			if (description != null)
			{
				query = query.Where(cjs => cjs.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cjs => cjs.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(cjs => cjs.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(cjs => cjs.color == color);
			}
			if (minLifetimeGiving.HasValue == true)
			{
				query = query.Where(cjs => cjs.minLifetimeGiving == minLifetimeGiving.Value);
			}
			if (maxLifetimeGiving.HasValue == true)
			{
				query = query.Where(cjs => cjs.maxLifetimeGiving == maxLifetimeGiving.Value);
			}
			if (minSingleGiftAmount.HasValue == true)
			{
				query = query.Where(cjs => cjs.minSingleGiftAmount == minSingleGiftAmount.Value);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(cjs => cjs.isDefault == isDefault.Value);
			}
			if (minAnnualGiving.HasValue == true)
			{
				query = query.Where(cjs => cjs.minAnnualGiving == minAnnualGiving.Value);
			}
			if (maxDaysSinceLastGift.HasValue == true)
			{
				query = query.Where(cjs => cjs.maxDaysSinceLastGift == maxDaysSinceLastGift.Value);
			}
			if (minGiftCount.HasValue == true)
			{
				query = query.Where(cjs => cjs.minGiftCount == minGiftCount.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cjs => cjs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cjs => cjs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cjs => cjs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cjs => cjs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cjs => cjs.deleted == false);
				}
			}
			else
			{
				query = query.Where(cjs => cjs.active == true);
				query = query.Where(cjs => cjs.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Constituent Journey Stage, or on an any of the string fields on its immediate relations
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
        /// This gets a single ConstituentJourneyStage by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConstituentJourneyStage/{id}")]
		public async Task<IActionResult> GetConstituentJourneyStage(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 60, cancellationToken);
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
				IQueryable<Database.ConstituentJourneyStage> query = (from cjs in _context.ConstituentJourneyStages where
							(cjs.id == id) &&
							(userIsAdmin == true || cjs.deleted == false) &&
							(userIsWriter == true || cjs.active == true)
					select cjs);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.icon);
					query = query.AsSplitQuery();
				}

				Database.ConstituentJourneyStage materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ConstituentJourneyStage Entity was read with Admin privilege." : "Scheduler.ConstituentJourneyStage Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConstituentJourneyStage", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ConstituentJourneyStage entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ConstituentJourneyStage.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ConstituentJourneyStage.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ConstituentJourneyStage record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ConstituentJourneyStage/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutConstituentJourneyStage(int id, [FromBody]Database.ConstituentJourneyStage.ConstituentJourneyStageDTO constituentJourneyStageDTO, CancellationToken cancellationToken = default)
		{
			if (constituentJourneyStageDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Fundraising Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Fundraising Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != constituentJourneyStageDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 60, cancellationToken);
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


			IQueryable<Database.ConstituentJourneyStage> query = (from x in _context.ConstituentJourneyStages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConstituentJourneyStage existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConstituentJourneyStage PUT", id.ToString(), new Exception("No Scheduler.ConstituentJourneyStage entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (constituentJourneyStageDTO.objectGuid == Guid.Empty)
            {
                constituentJourneyStageDTO.objectGuid = existing.objectGuid;
            }
            else if (constituentJourneyStageDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ConstituentJourneyStage record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ConstituentJourneyStage cloneOfExisting = (Database.ConstituentJourneyStage)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ConstituentJourneyStage object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ConstituentJourneyStage constituentJourneyStage = (Database.ConstituentJourneyStage)_context.Entry(existing).GetDatabaseValues().ToObject();
			constituentJourneyStage.ApplyDTO(constituentJourneyStageDTO);
			//
			// The tenant guid for any ConstituentJourneyStage being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ConstituentJourneyStage because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				constituentJourneyStage.tenantGuid = existing.tenantGuid;
			}

			lock (constituentJourneyStagePutSyncRoot)
			{
				//
				// Validate the version number for the constituentJourneyStage being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != constituentJourneyStage.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ConstituentJourneyStage save attempt was made but save request was with version " + constituentJourneyStage.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ConstituentJourneyStage you are trying to update has already changed.  Please try your save again after reloading the ConstituentJourneyStage.");
				}
				else
				{
					// Same record.  Increase version.
					constituentJourneyStage.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (constituentJourneyStage.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ConstituentJourneyStage record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (constituentJourneyStage.name != null && constituentJourneyStage.name.Length > 100)
				{
					constituentJourneyStage.name = constituentJourneyStage.name.Substring(0, 100);
				}

				if (constituentJourneyStage.description != null && constituentJourneyStage.description.Length > 500)
				{
					constituentJourneyStage.description = constituentJourneyStage.description.Substring(0, 500);
				}

				if (constituentJourneyStage.color != null && constituentJourneyStage.color.Length > 10)
				{
					constituentJourneyStage.color = constituentJourneyStage.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.ConstituentJourneyStage> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(constituentJourneyStage);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ConstituentJourneyStageChangeHistory constituentJourneyStageChangeHistory = new ConstituentJourneyStageChangeHistory();
				        constituentJourneyStageChangeHistory.constituentJourneyStageId = constituentJourneyStage.id;
				        constituentJourneyStageChangeHistory.versionNumber = constituentJourneyStage.versionNumber;
				        constituentJourneyStageChangeHistory.timeStamp = DateTime.UtcNow;
				        constituentJourneyStageChangeHistory.userId = securityUser.id;
				        constituentJourneyStageChangeHistory.tenantGuid = userTenantGuid;
				        constituentJourneyStageChangeHistory.data = JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage));
				        _context.ConstituentJourneyStageChangeHistories.Add(constituentJourneyStageChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConstituentJourneyStage entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage)),
						null);

				return Ok(Database.ConstituentJourneyStage.CreateAnonymous(constituentJourneyStage));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConstituentJourneyStage entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ConstituentJourneyStage record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConstituentJourneyStage", Name = "ConstituentJourneyStage")]
		public async Task<IActionResult> PostConstituentJourneyStage([FromBody]Database.ConstituentJourneyStage.ConstituentJourneyStageDTO constituentJourneyStageDTO, CancellationToken cancellationToken = default)
		{
			if (constituentJourneyStageDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Fundraising Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Fundraising Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new ConstituentJourneyStage object using the data from the DTO
			//
			Database.ConstituentJourneyStage constituentJourneyStage = Database.ConstituentJourneyStage.FromDTO(constituentJourneyStageDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				constituentJourneyStage.tenantGuid = userTenantGuid;

				if (constituentJourneyStage.name != null && constituentJourneyStage.name.Length > 100)
				{
					constituentJourneyStage.name = constituentJourneyStage.name.Substring(0, 100);
				}

				if (constituentJourneyStage.description != null && constituentJourneyStage.description.Length > 500)
				{
					constituentJourneyStage.description = constituentJourneyStage.description.Substring(0, 500);
				}

				if (constituentJourneyStage.color != null && constituentJourneyStage.color.Length > 10)
				{
					constituentJourneyStage.color = constituentJourneyStage.color.Substring(0, 10);
				}

				constituentJourneyStage.objectGuid = Guid.NewGuid();
				constituentJourneyStage.versionNumber = 1;

				_context.ConstituentJourneyStages.Add(constituentJourneyStage);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the constituentJourneyStage object so that no further changes will be written to the database
				    //
				    _context.Entry(constituentJourneyStage).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					constituentJourneyStage.ConstituentJourneyStageChangeHistories = null;
					constituentJourneyStage.Constituents = null;
					constituentJourneyStage.icon = null;


				    ConstituentJourneyStageChangeHistory constituentJourneyStageChangeHistory = new ConstituentJourneyStageChangeHistory();
				    constituentJourneyStageChangeHistory.constituentJourneyStageId = constituentJourneyStage.id;
				    constituentJourneyStageChangeHistory.versionNumber = constituentJourneyStage.versionNumber;
				    constituentJourneyStageChangeHistory.timeStamp = DateTime.UtcNow;
				    constituentJourneyStageChangeHistory.userId = securityUser.id;
				    constituentJourneyStageChangeHistory.tenantGuid = userTenantGuid;
				    constituentJourneyStageChangeHistory.data = JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage));
				    _context.ConstituentJourneyStageChangeHistories.Add(constituentJourneyStageChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ConstituentJourneyStage entity successfully created.",
						true,
						constituentJourneyStage. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ConstituentJourneyStage entity creation failed.", false, constituentJourneyStage.id.ToString(), "", JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConstituentJourneyStage", constituentJourneyStage.id, constituentJourneyStage.name));

			return CreatedAtRoute("ConstituentJourneyStage", new { id = constituentJourneyStage.id }, Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage));
		}



        /// <summary>
        /// 
        /// This rolls a ConstituentJourneyStage entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConstituentJourneyStage/Rollback/{id}")]
		[Route("api/ConstituentJourneyStage/Rollback")]
		public async Task<IActionResult> RollbackToConstituentJourneyStageVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ConstituentJourneyStage> query = (from x in _context.ConstituentJourneyStages
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ConstituentJourneyStage concurrently
			//
			lock (constituentJourneyStagePutSyncRoot)
			{
				
				Database.ConstituentJourneyStage constituentJourneyStage = query.FirstOrDefault();
				
				if (constituentJourneyStage == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConstituentJourneyStage rollback", id.ToString(), new Exception("No Scheduler.ConstituentJourneyStage entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ConstituentJourneyStage current state so we can log it.
				//
				Database.ConstituentJourneyStage cloneOfExisting = (Database.ConstituentJourneyStage)_context.Entry(constituentJourneyStage).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ConstituentJourneyStageChangeHistories = null;
				cloneOfExisting.Constituents = null;
				cloneOfExisting.icon = null;

				if (versionNumber >= constituentJourneyStage.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ConstituentJourneyStage rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ConstituentJourneyStage rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ConstituentJourneyStageChangeHistory constituentJourneyStageChangeHistory = (from x in _context.ConstituentJourneyStageChangeHistories
				                                               where
				                                               x.constituentJourneyStageId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (constituentJourneyStageChangeHistory != null)
				{
				    Database.ConstituentJourneyStage oldConstituentJourneyStage = JsonSerializer.Deserialize<Database.ConstituentJourneyStage>(constituentJourneyStageChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    constituentJourneyStage.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    constituentJourneyStage.name = oldConstituentJourneyStage.name;
				    constituentJourneyStage.description = oldConstituentJourneyStage.description;
				    constituentJourneyStage.sequence = oldConstituentJourneyStage.sequence;
				    constituentJourneyStage.iconId = oldConstituentJourneyStage.iconId;
				    constituentJourneyStage.color = oldConstituentJourneyStage.color;
				    constituentJourneyStage.minLifetimeGiving = oldConstituentJourneyStage.minLifetimeGiving;
				    constituentJourneyStage.maxLifetimeGiving = oldConstituentJourneyStage.maxLifetimeGiving;
				    constituentJourneyStage.minSingleGiftAmount = oldConstituentJourneyStage.minSingleGiftAmount;
				    constituentJourneyStage.isDefault = oldConstituentJourneyStage.isDefault;
				    constituentJourneyStage.minAnnualGiving = oldConstituentJourneyStage.minAnnualGiving;
				    constituentJourneyStage.maxDaysSinceLastGift = oldConstituentJourneyStage.maxDaysSinceLastGift;
				    constituentJourneyStage.minGiftCount = oldConstituentJourneyStage.minGiftCount;
				    constituentJourneyStage.objectGuid = oldConstituentJourneyStage.objectGuid;
				    constituentJourneyStage.active = oldConstituentJourneyStage.active;
				    constituentJourneyStage.deleted = oldConstituentJourneyStage.deleted;

				    string serializedConstituentJourneyStage = JsonSerializer.Serialize(constituentJourneyStage);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ConstituentJourneyStageChangeHistory newConstituentJourneyStageChangeHistory = new ConstituentJourneyStageChangeHistory();
				        newConstituentJourneyStageChangeHistory.constituentJourneyStageId = constituentJourneyStage.id;
				        newConstituentJourneyStageChangeHistory.versionNumber = constituentJourneyStage.versionNumber;
				        newConstituentJourneyStageChangeHistory.timeStamp = DateTime.UtcNow;
				        newConstituentJourneyStageChangeHistory.userId = securityUser.id;
				        newConstituentJourneyStageChangeHistory.tenantGuid = userTenantGuid;
				        newConstituentJourneyStageChangeHistory.data = JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage));
				        _context.ConstituentJourneyStageChangeHistories.Add(newConstituentJourneyStageChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConstituentJourneyStage rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage)),
						null);


				    return Ok(Database.ConstituentJourneyStage.CreateAnonymous(constituentJourneyStage));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ConstituentJourneyStage rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ConstituentJourneyStage rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ConstituentJourneyStage.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConstituentJourneyStage</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConstituentJourneyStage/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetConstituentJourneyStageChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ConstituentJourneyStage constituentJourneyStage = await _context.ConstituentJourneyStages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (constituentJourneyStage == null)
			{
				return NotFound();
			}

			try
			{
				constituentJourneyStage.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConstituentJourneyStage> versionInfo = await constituentJourneyStage.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ConstituentJourneyStage.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConstituentJourneyStage</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConstituentJourneyStage/{id}/AuditHistory")]
		public async Task<IActionResult> GetConstituentJourneyStageAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ConstituentJourneyStage constituentJourneyStage = await _context.ConstituentJourneyStages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (constituentJourneyStage == null)
			{
				return NotFound();
			}

			try
			{
				constituentJourneyStage.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ConstituentJourneyStage>> versions = await constituentJourneyStage.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ConstituentJourneyStage.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConstituentJourneyStage</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ConstituentJourneyStage object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConstituentJourneyStage/{id}/Version/{version}")]
		public async Task<IActionResult> GetConstituentJourneyStageVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ConstituentJourneyStage constituentJourneyStage = await _context.ConstituentJourneyStages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (constituentJourneyStage == null)
			{
				return NotFound();
			}

			try
			{
				constituentJourneyStage.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConstituentJourneyStage> versionInfo = await constituentJourneyStage.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ConstituentJourneyStage at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConstituentJourneyStage</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ConstituentJourneyStage object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConstituentJourneyStage/{id}/StateAtTime")]
		public async Task<IActionResult> GetConstituentJourneyStageStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ConstituentJourneyStage constituentJourneyStage = await _context.ConstituentJourneyStages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (constituentJourneyStage == null)
			{
				return NotFound();
			}

			try
			{
				constituentJourneyStage.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConstituentJourneyStage> versionInfo = await constituentJourneyStage.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ConstituentJourneyStage record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConstituentJourneyStage/{id}")]
		[Route("api/ConstituentJourneyStage")]
		public async Task<IActionResult> DeleteConstituentJourneyStage(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Fundraising Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Fundraising Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.ConstituentJourneyStage> query = (from x in _context.ConstituentJourneyStages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConstituentJourneyStage constituentJourneyStage = await query.FirstOrDefaultAsync(cancellationToken);

			if (constituentJourneyStage == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConstituentJourneyStage DELETE", id.ToString(), new Exception("No Scheduler.ConstituentJourneyStage entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ConstituentJourneyStage cloneOfExisting = (Database.ConstituentJourneyStage)_context.Entry(constituentJourneyStage).GetDatabaseValues().ToObject();


			lock (constituentJourneyStageDeleteSyncRoot)
			{
			    try
			    {
			        constituentJourneyStage.deleted = true;
			        constituentJourneyStage.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ConstituentJourneyStageChangeHistory constituentJourneyStageChangeHistory = new ConstituentJourneyStageChangeHistory();
			        constituentJourneyStageChangeHistory.constituentJourneyStageId = constituentJourneyStage.id;
			        constituentJourneyStageChangeHistory.versionNumber = constituentJourneyStage.versionNumber;
			        constituentJourneyStageChangeHistory.timeStamp = DateTime.UtcNow;
			        constituentJourneyStageChangeHistory.userId = securityUser.id;
			        constituentJourneyStageChangeHistory.tenantGuid = userTenantGuid;
			        constituentJourneyStageChangeHistory.data = JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage));
			        _context.ConstituentJourneyStageChangeHistories.Add(constituentJourneyStageChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ConstituentJourneyStage entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ConstituentJourneyStage entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConstituentJourneyStage.CreateAnonymousWithFirstLevelSubObjects(constituentJourneyStage)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ConstituentJourneyStage records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ConstituentJourneyStages/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? sequence = null,
			int? iconId = null,
			string color = null,
			decimal? minLifetimeGiving = null,
			decimal? maxLifetimeGiving = null,
			decimal? minSingleGiftAmount = null,
			bool? isDefault = null,
			decimal? minAnnualGiving = null,
			int? maxDaysSinceLastGift = null,
			int? minGiftCount = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 60, cancellationToken);


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

			IQueryable<Database.ConstituentJourneyStage> query = (from cjs in _context.ConstituentJourneyStages select cjs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cjs => cjs.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cjs => cjs.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cjs => cjs.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(cjs => cjs.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(cjs => cjs.color == color);
			}
			if (minLifetimeGiving.HasValue == true)
			{
				query = query.Where(cjs => cjs.minLifetimeGiving == minLifetimeGiving.Value);
			}
			if (maxLifetimeGiving.HasValue == true)
			{
				query = query.Where(cjs => cjs.maxLifetimeGiving == maxLifetimeGiving.Value);
			}
			if (minSingleGiftAmount.HasValue == true)
			{
				query = query.Where(cjs => cjs.minSingleGiftAmount == minSingleGiftAmount.Value);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(cjs => cjs.isDefault == isDefault.Value);
			}
			if (minAnnualGiving.HasValue == true)
			{
				query = query.Where(cjs => cjs.minAnnualGiving == minAnnualGiving.Value);
			}
			if (maxDaysSinceLastGift.HasValue == true)
			{
				query = query.Where(cjs => cjs.maxDaysSinceLastGift == maxDaysSinceLastGift.Value);
			}
			if (minGiftCount.HasValue == true)
			{
				query = query.Where(cjs => cjs.minGiftCount == minGiftCount.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cjs => cjs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cjs => cjs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cjs => cjs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cjs => cjs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cjs => cjs.deleted == false);
				}
			}
			else
			{
				query = query.Where(cjs => cjs.active == true);
				query = query.Where(cjs => cjs.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Constituent Journey Stage, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.ConstituentJourneyStage.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ConstituentJourneyStage/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Fundraising Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Fundraising Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


        /// <summary>
        /// 
        /// This makes a ConstituentJourneyStage record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/ConstituentJourneyStage/Favourite/{id}")]
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


			IQueryable<Database.ConstituentJourneyStage> query = (from x in _context.ConstituentJourneyStages
			                               where x.id == id
			                               select x);


			Database.ConstituentJourneyStage constituentJourneyStage = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (constituentJourneyStage != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = constituentJourneyStage.name;
				}

				//
				// Add the user favourite ConstituentJourneyStage
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "ConstituentJourneyStage", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ConstituentJourneyStage' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ConstituentJourneyStage' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a ConstituentJourneyStage record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/ConstituentJourneyStage/Favourite/{id}")]
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
			// Delete the user favourite ConstituentJourneyStage
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "ConstituentJourneyStage", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'ConstituentJourneyStage' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}


	}
}
