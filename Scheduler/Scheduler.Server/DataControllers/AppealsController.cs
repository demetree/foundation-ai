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
    /// This auto generated class provides the basic CRUD operations for the Appeal entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Appeal entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AppealsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object appealPutSyncRoot = new object();
		static object appealDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<AppealsController> _logger;

		public AppealsController(SchedulerContext context, ILogger<AppealsController> logger) : base("Scheduler", "Appeal")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Appeals filtered by the parameters provided.
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
		[Route("api/Appeals")]
		public async Task<IActionResult> GetAppeals(
			int? campaignId = null,
			string name = null,
			string description = null,
			decimal? costPerUnit = null,
			string notes = null,
			int? iconId = null,
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

			IQueryable<Database.Appeal> query = (from a in _context.Appeals select a);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (campaignId.HasValue == true)
			{
				query = query.Where(a => a.campaignId == campaignId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(a => a.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(a => a.description == description);
			}
			if (costPerUnit.HasValue == true)
			{
				query = query.Where(a => a.costPerUnit == costPerUnit.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(a => a.notes == notes);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(a => a.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(a => a.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(a => a.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(a => a.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(a => a.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(a => a.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(a => a.deleted == false);
				}
			}
			else
			{
				query = query.Where(a => a.active == true);
				query = query.Where(a => a.deleted == false);
			}

			query = query.OrderBy(a => a.name).ThenBy(a => a.description).ThenBy(a => a.color);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.campaign);
				query = query.Include(x => x.icon);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Appeal, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || (includeRelations == true && x.campaign.name.Contains(anyStringContains))
			       || (includeRelations == true && x.campaign.description.Contains(anyStringContains))
			       || (includeRelations == true && x.campaign.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.campaign.color.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Appeal> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Appeal appeal in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(appeal, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Appeal Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Appeal Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Appeals filtered by the parameters provided.  Its query is similar to the GetAppeals method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Appeals/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? campaignId = null,
			string name = null,
			string description = null,
			decimal? costPerUnit = null,
			string notes = null,
			int? iconId = null,
			string color = null,
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


			IQueryable<Database.Appeal> query = (from a in _context.Appeals select a);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (campaignId.HasValue == true)
			{
				query = query.Where(a => a.campaignId == campaignId.Value);
			}
			if (name != null)
			{
				query = query.Where(a => a.name == name);
			}
			if (description != null)
			{
				query = query.Where(a => a.description == description);
			}
			if (costPerUnit.HasValue == true)
			{
				query = query.Where(a => a.costPerUnit == costPerUnit.Value);
			}
			if (notes != null)
			{
				query = query.Where(a => a.notes == notes);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(a => a.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(a => a.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(a => a.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(a => a.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(a => a.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(a => a.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(a => a.deleted == false);
				}
			}
			else
			{
				query = query.Where(a => a.active == true);
				query = query.Where(a => a.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Appeal, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.campaign.name.Contains(anyStringContains)
			       || x.campaign.description.Contains(anyStringContains)
			       || x.campaign.notes.Contains(anyStringContains)
			       || x.campaign.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Appeal by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Appeal/{id}")]
		public async Task<IActionResult> GetAppeal(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Appeal> query = (from a in _context.Appeals where
							(a.id == id) &&
							(userIsAdmin == true || a.deleted == false) &&
							(userIsWriter == true || a.active == true)
					select a);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.campaign);
					query = query.Include(x => x.icon);
					query = query.AsSplitQuery();
				}

				Database.Appeal materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Appeal Entity was read with Admin privilege." : "Scheduler.Appeal Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Appeal", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Appeal entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Appeal.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Appeal.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Appeal record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Appeal/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAppeal(int id, [FromBody]Database.Appeal.AppealDTO appealDTO, CancellationToken cancellationToken = default)
		{
			if (appealDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != appealDTO.id)
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


			IQueryable<Database.Appeal> query = (from x in _context.Appeals
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Appeal existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Appeal PUT", id.ToString(), new Exception("No Scheduler.Appeal entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (appealDTO.objectGuid == Guid.Empty)
            {
                appealDTO.objectGuid = existing.objectGuid;
            }
            else if (appealDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Appeal record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Appeal cloneOfExisting = (Database.Appeal)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Appeal object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Appeal appeal = (Database.Appeal)_context.Entry(existing).GetDatabaseValues().ToObject();
			appeal.ApplyDTO(appealDTO);
			//
			// The tenant guid for any Appeal being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Appeal because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				appeal.tenantGuid = existing.tenantGuid;
			}

			lock (appealPutSyncRoot)
			{
				//
				// Validate the version number for the appeal being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != appeal.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Appeal save attempt was made but save request was with version " + appeal.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Appeal you are trying to update has already changed.  Please try your save again after reloading the Appeal.");
				}
				else
				{
					// Same record.  Increase version.
					appeal.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (appeal.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Appeal record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (appeal.name != null && appeal.name.Length > 100)
				{
					appeal.name = appeal.name.Substring(0, 100);
				}

				if (appeal.description != null && appeal.description.Length > 500)
				{
					appeal.description = appeal.description.Substring(0, 500);
				}

				if (appeal.color != null && appeal.color.Length > 10)
				{
					appeal.color = appeal.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.Appeal> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(appeal);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        AppealChangeHistory appealChangeHistory = new AppealChangeHistory();
				        appealChangeHistory.appealId = appeal.id;
				        appealChangeHistory.versionNumber = appeal.versionNumber;
				        appealChangeHistory.timeStamp = DateTime.UtcNow;
				        appealChangeHistory.userId = securityUser.id;
				        appealChangeHistory.tenantGuid = userTenantGuid;
				        appealChangeHistory.data = JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal));
				        _context.AppealChangeHistories.Add(appealChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Appeal entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal)),
						null);

				return Ok(Database.Appeal.CreateAnonymous(appeal));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Appeal entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Appeal record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Appeal", Name = "Appeal")]
		public async Task<IActionResult> PostAppeal([FromBody]Database.Appeal.AppealDTO appealDTO, CancellationToken cancellationToken = default)
		{
			if (appealDTO == null)
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
			// Create a new Appeal object using the data from the DTO
			//
			Database.Appeal appeal = Database.Appeal.FromDTO(appealDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				appeal.tenantGuid = userTenantGuid;

				if (appeal.name != null && appeal.name.Length > 100)
				{
					appeal.name = appeal.name.Substring(0, 100);
				}

				if (appeal.description != null && appeal.description.Length > 500)
				{
					appeal.description = appeal.description.Substring(0, 500);
				}

				if (appeal.color != null && appeal.color.Length > 10)
				{
					appeal.color = appeal.color.Substring(0, 10);
				}

				appeal.objectGuid = Guid.NewGuid();
				appeal.versionNumber = 1;

				_context.Appeals.Add(appeal);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the appeal object so that no further changes will be written to the database
				    //
				    _context.Entry(appeal).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					appeal.AppealChangeHistories = null;
					appeal.Batches = null;
					appeal.Gifts = null;
					appeal.Pledges = null;
					appeal.campaign = null;
					appeal.icon = null;


				    AppealChangeHistory appealChangeHistory = new AppealChangeHistory();
				    appealChangeHistory.appealId = appeal.id;
				    appealChangeHistory.versionNumber = appeal.versionNumber;
				    appealChangeHistory.timeStamp = DateTime.UtcNow;
				    appealChangeHistory.userId = securityUser.id;
				    appealChangeHistory.tenantGuid = userTenantGuid;
				    appealChangeHistory.data = JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal));
				    _context.AppealChangeHistories.Add(appealChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Appeal entity successfully created.",
						true,
						appeal. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Appeal entity creation failed.", false, appeal.id.ToString(), "", JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Appeal", appeal.id, appeal.name));

			return CreatedAtRoute("Appeal", new { id = appeal.id }, Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal));
		}



        /// <summary>
        /// 
        /// This rolls a Appeal entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Appeal/Rollback/{id}")]
		[Route("api/Appeal/Rollback")]
		public async Task<IActionResult> RollbackToAppealVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Appeal> query = (from x in _context.Appeals
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Appeal concurrently
			//
			lock (appealPutSyncRoot)
			{
				
				Database.Appeal appeal = query.FirstOrDefault();
				
				if (appeal == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Appeal rollback", id.ToString(), new Exception("No Scheduler.Appeal entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Appeal current state so we can log it.
				//
				Database.Appeal cloneOfExisting = (Database.Appeal)_context.Entry(appeal).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.AppealChangeHistories = null;
				cloneOfExisting.Batches = null;
				cloneOfExisting.Gifts = null;
				cloneOfExisting.Pledges = null;
				cloneOfExisting.campaign = null;
				cloneOfExisting.icon = null;

				if (versionNumber >= appeal.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Appeal rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Appeal rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				AppealChangeHistory appealChangeHistory = (from x in _context.AppealChangeHistories
				                                               where
				                                               x.appealId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (appealChangeHistory != null)
				{
				    Database.Appeal oldAppeal = JsonSerializer.Deserialize<Database.Appeal>(appealChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    appeal.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    appeal.campaignId = oldAppeal.campaignId;
				    appeal.name = oldAppeal.name;
				    appeal.description = oldAppeal.description;
				    appeal.costPerUnit = oldAppeal.costPerUnit;
				    appeal.notes = oldAppeal.notes;
				    appeal.iconId = oldAppeal.iconId;
				    appeal.color = oldAppeal.color;
				    appeal.objectGuid = oldAppeal.objectGuid;
				    appeal.active = oldAppeal.active;
				    appeal.deleted = oldAppeal.deleted;

				    string serializedAppeal = JsonSerializer.Serialize(appeal);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        AppealChangeHistory newAppealChangeHistory = new AppealChangeHistory();
				        newAppealChangeHistory.appealId = appeal.id;
				        newAppealChangeHistory.versionNumber = appeal.versionNumber;
				        newAppealChangeHistory.timeStamp = DateTime.UtcNow;
				        newAppealChangeHistory.userId = securityUser.id;
				        newAppealChangeHistory.tenantGuid = userTenantGuid;
				        newAppealChangeHistory.data = JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal));
				        _context.AppealChangeHistories.Add(newAppealChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Appeal rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal)),
						null);


				    return Ok(Database.Appeal.CreateAnonymous(appeal));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Appeal rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Appeal rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}


        /// <summary>
        /// 
        /// This deletes a Appeal record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Appeal/{id}")]
		[Route("api/Appeal")]
		public async Task<IActionResult> DeleteAppeal(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Appeal> query = (from x in _context.Appeals
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Appeal appeal = await query.FirstOrDefaultAsync(cancellationToken);

			if (appeal == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Appeal DELETE", id.ToString(), new Exception("No Scheduler.Appeal entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Appeal cloneOfExisting = (Database.Appeal)_context.Entry(appeal).GetDatabaseValues().ToObject();


			lock (appealDeleteSyncRoot)
			{
			    try
			    {
			        appeal.deleted = true;
			        appeal.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        AppealChangeHistory appealChangeHistory = new AppealChangeHistory();
			        appealChangeHistory.appealId = appeal.id;
			        appealChangeHistory.versionNumber = appeal.versionNumber;
			        appealChangeHistory.timeStamp = DateTime.UtcNow;
			        appealChangeHistory.userId = securityUser.id;
			        appealChangeHistory.tenantGuid = userTenantGuid;
			        appealChangeHistory.data = JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal));
			        _context.AppealChangeHistories.Add(appealChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Appeal entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Appeal entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Appeal.CreateAnonymousWithFirstLevelSubObjects(appeal)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Appeal records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Appeals/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? campaignId = null,
			string name = null,
			string description = null,
			decimal? costPerUnit = null,
			string notes = null,
			int? iconId = null,
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

			IQueryable<Database.Appeal> query = (from a in _context.Appeals select a);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (campaignId.HasValue == true)
			{
				query = query.Where(a => a.campaignId == campaignId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(a => a.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(a => a.description == description);
			}
			if (costPerUnit.HasValue == true)
			{
				query = query.Where(a => a.costPerUnit == costPerUnit.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(a => a.notes == notes);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(a => a.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(a => a.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(a => a.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(a => a.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(a => a.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(a => a.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(a => a.deleted == false);
				}
			}
			else
			{
				query = query.Where(a => a.active == true);
				query = query.Where(a => a.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Appeal, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.campaign.name.Contains(anyStringContains)
			       || x.campaign.description.Contains(anyStringContains)
			       || x.campaign.notes.Contains(anyStringContains)
			       || x.campaign.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Appeal.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Appeal/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


        /// <summary>
        /// 
        /// This makes a Appeal record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Appeal/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPut]
		public async Task<IActionResult> SetFavourite(int id, string description = null, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(cancellationToken);
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


			IQueryable<Database.Appeal> query = (from x in _context.Appeals
			                               where x.id == id
			                               select x);


			Database.Appeal appeal = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (appeal != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = appeal.name;
				}

				//
				// Add the user favourite Appeal
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "Appeal", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Appeal' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Appeal' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a Appeal record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Appeal/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpDelete]
		public async Task<IActionResult> DeleteFavourite(int id, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
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
			// Delete the user favourite Appeal
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "Appeal", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Appeal' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}


	}
}
