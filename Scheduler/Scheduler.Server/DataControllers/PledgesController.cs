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
    /// This auto generated class provides the basic CRUD operations for the Pledge entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Pledge entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PledgesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 60;

		static object pledgePutSyncRoot = new object();
		static object pledgeDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<PledgesController> _logger;

		public PledgesController(SchedulerContext context, ILogger<PledgesController> logger) : base("Scheduler", "Pledge")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Pledges filtered by the parameters provided.
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
		[Route("api/Pledges")]
		public async Task<IActionResult> GetPledges(
			int? constituentId = null,
			decimal? totalAmount = null,
			decimal? balanceAmount = null,
			DateOnly? pledgeDate = null,
			DateOnly? startDate = null,
			DateOnly? endDate = null,
			int? recurrenceFrequencyId = null,
			int? fundId = null,
			int? campaignId = null,
			int? appealId = null,
			decimal? writeOffAmount = null,
			bool? isWrittenOff = null,
			string notes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
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

			IQueryable<Database.Pledge> query = (from p in _context.Pledges select p);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (constituentId.HasValue == true)
			{
				query = query.Where(p => p.constituentId == constituentId.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(p => p.totalAmount == totalAmount.Value);
			}
			if (balanceAmount.HasValue == true)
			{
				query = query.Where(p => p.balanceAmount == balanceAmount.Value);
			}
			if (recurrenceFrequencyId.HasValue == true)
			{
				query = query.Where(p => p.recurrenceFrequencyId == recurrenceFrequencyId.Value);
			}
			if (fundId.HasValue == true)
			{
				query = query.Where(p => p.fundId == fundId.Value);
			}
			if (campaignId.HasValue == true)
			{
				query = query.Where(p => p.campaignId == campaignId.Value);
			}
			if (appealId.HasValue == true)
			{
				query = query.Where(p => p.appealId == appealId.Value);
			}
			if (writeOffAmount.HasValue == true)
			{
				query = query.Where(p => p.writeOffAmount == writeOffAmount.Value);
			}
			if (isWrittenOff.HasValue == true)
			{
				query = query.Where(p => p.isWrittenOff == isWrittenOff.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(p => p.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(p => p.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(p => p.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(p => p.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(p => p.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(p => p.deleted == false);
				}
			}
			else
			{
				query = query.Where(p => p.active == true);
				query = query.Where(p => p.deleted == false);
			}

			query = query.OrderBy(p => p.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.appeal);
				query = query.Include(x => x.campaign);
				query = query.Include(x => x.constituent);
				query = query.Include(x => x.fund);
				query = query.Include(x => x.recurrenceFrequency);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.Pledge> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Pledge pledge in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(pledge, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Pledge Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Pledge Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Pledges filtered by the parameters provided.  Its query is similar to the GetPledges method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Pledges/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? constituentId = null,
			decimal? totalAmount = null,
			decimal? balanceAmount = null,
			DateOnly? pledgeDate = null,
			DateOnly? startDate = null,
			DateOnly? endDate = null,
			int? recurrenceFrequencyId = null,
			int? fundId = null,
			int? campaignId = null,
			int? appealId = null,
			decimal? writeOffAmount = null,
			bool? isWrittenOff = null,
			string notes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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


			IQueryable<Database.Pledge> query = (from p in _context.Pledges select p);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (constituentId.HasValue == true)
			{
				query = query.Where(p => p.constituentId == constituentId.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(p => p.totalAmount == totalAmount.Value);
			}
			if (balanceAmount.HasValue == true)
			{
				query = query.Where(p => p.balanceAmount == balanceAmount.Value);
			}
			if (recurrenceFrequencyId.HasValue == true)
			{
				query = query.Where(p => p.recurrenceFrequencyId == recurrenceFrequencyId.Value);
			}
			if (fundId.HasValue == true)
			{
				query = query.Where(p => p.fundId == fundId.Value);
			}
			if (campaignId.HasValue == true)
			{
				query = query.Where(p => p.campaignId == campaignId.Value);
			}
			if (appealId.HasValue == true)
			{
				query = query.Where(p => p.appealId == appealId.Value);
			}
			if (writeOffAmount.HasValue == true)
			{
				query = query.Where(p => p.writeOffAmount == writeOffAmount.Value);
			}
			if (isWrittenOff.HasValue == true)
			{
				query = query.Where(p => p.isWrittenOff == isWrittenOff.Value);
			}
			if (notes != null)
			{
				query = query.Where(p => p.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(p => p.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(p => p.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(p => p.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(p => p.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(p => p.deleted == false);
				}
			}
			else
			{
				query = query.Where(p => p.active == true);
				query = query.Where(p => p.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Pledge by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Pledge/{id}")]
		public async Task<IActionResult> GetPledge(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Pledge> query = (from p in _context.Pledges where
							(p.id == id) &&
							(userIsAdmin == true || p.deleted == false) &&
							(userIsWriter == true || p.active == true)
					select p);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.appeal);
					query = query.Include(x => x.campaign);
					query = query.Include(x => x.constituent);
					query = query.Include(x => x.fund);
					query = query.Include(x => x.recurrenceFrequency);
					query = query.AsSplitQuery();
				}

				Database.Pledge materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Pledge Entity was read with Admin privilege." : "Scheduler.Pledge Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Pledge", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Pledge entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Pledge.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Pledge.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Pledge record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Pledge/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPledge(int id, [FromBody]Database.Pledge.PledgeDTO pledgeDTO, CancellationToken cancellationToken = default)
		{
			if (pledgeDTO == null)
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



			if (id != pledgeDTO.id)
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


			IQueryable<Database.Pledge> query = (from x in _context.Pledges
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Pledge existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Pledge PUT", id.ToString(), new Exception("No Scheduler.Pledge entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (pledgeDTO.objectGuid == Guid.Empty)
            {
                pledgeDTO.objectGuid = existing.objectGuid;
            }
            else if (pledgeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Pledge record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Pledge cloneOfExisting = (Database.Pledge)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Pledge object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Pledge pledge = (Database.Pledge)_context.Entry(existing).GetDatabaseValues().ToObject();
			pledge.ApplyDTO(pledgeDTO);
			//
			// The tenant guid for any Pledge being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Pledge because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				pledge.tenantGuid = existing.tenantGuid;
			}

			lock (pledgePutSyncRoot)
			{
				//
				// Validate the version number for the pledge being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != pledge.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Pledge save attempt was made but save request was with version " + pledge.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Pledge you are trying to update has already changed.  Please try your save again after reloading the Pledge.");
				}
				else
				{
					// Same record.  Increase version.
					pledge.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (pledge.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Pledge record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.Pledge> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(pledge);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PledgeChangeHistory pledgeChangeHistory = new PledgeChangeHistory();
				        pledgeChangeHistory.pledgeId = pledge.id;
				        pledgeChangeHistory.versionNumber = pledge.versionNumber;
				        pledgeChangeHistory.timeStamp = DateTime.UtcNow;
				        pledgeChangeHistory.userId = securityUser.id;
				        pledgeChangeHistory.tenantGuid = userTenantGuid;
				        pledgeChangeHistory.data = JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge));
				        _context.PledgeChangeHistories.Add(pledgeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Pledge entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge)),
						null);

				return Ok(Database.Pledge.CreateAnonymous(pledge));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Pledge entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Pledge record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Pledge", Name = "Pledge")]
		public async Task<IActionResult> PostPledge([FromBody]Database.Pledge.PledgeDTO pledgeDTO, CancellationToken cancellationToken = default)
		{
			if (pledgeDTO == null)
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
			// Create a new Pledge object using the data from the DTO
			//
			Database.Pledge pledge = Database.Pledge.FromDTO(pledgeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				pledge.tenantGuid = userTenantGuid;

				pledge.objectGuid = Guid.NewGuid();
				pledge.versionNumber = 1;

				_context.Pledges.Add(pledge);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the pledge object so that no further changes will be written to the database
				    //
				    _context.Entry(pledge).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					pledge.Gifts = null;
					pledge.PledgeChangeHistories = null;
					pledge.appeal = null;
					pledge.campaign = null;
					pledge.constituent = null;
					pledge.fund = null;
					pledge.recurrenceFrequency = null;


				    PledgeChangeHistory pledgeChangeHistory = new PledgeChangeHistory();
				    pledgeChangeHistory.pledgeId = pledge.id;
				    pledgeChangeHistory.versionNumber = pledge.versionNumber;
				    pledgeChangeHistory.timeStamp = DateTime.UtcNow;
				    pledgeChangeHistory.userId = securityUser.id;
				    pledgeChangeHistory.tenantGuid = userTenantGuid;
				    pledgeChangeHistory.data = JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge));
				    _context.PledgeChangeHistories.Add(pledgeChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Pledge entity successfully created.",
						true,
						pledge. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Pledge entity creation failed.", false, pledge.id.ToString(), "", JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Pledge", pledge.id, pledge.id.ToString()));

			return CreatedAtRoute("Pledge", new { id = pledge.id }, Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge));
		}



        /// <summary>
        /// 
        /// This rolls a Pledge entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Pledge/Rollback/{id}")]
		[Route("api/Pledge/Rollback")]
		public async Task<IActionResult> RollbackToPledgeVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Pledge> query = (from x in _context.Pledges
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Pledge concurrently
			//
			lock (pledgePutSyncRoot)
			{
				
				Database.Pledge pledge = query.FirstOrDefault();
				
				if (pledge == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Pledge rollback", id.ToString(), new Exception("No Scheduler.Pledge entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Pledge current state so we can log it.
				//
				Database.Pledge cloneOfExisting = (Database.Pledge)_context.Entry(pledge).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Gifts = null;
				cloneOfExisting.PledgeChangeHistories = null;
				cloneOfExisting.appeal = null;
				cloneOfExisting.campaign = null;
				cloneOfExisting.constituent = null;
				cloneOfExisting.fund = null;
				cloneOfExisting.recurrenceFrequency = null;

				if (versionNumber >= pledge.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Pledge rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Pledge rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				PledgeChangeHistory pledgeChangeHistory = (from x in _context.PledgeChangeHistories
				                                               where
				                                               x.pledgeId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (pledgeChangeHistory != null)
				{
				    Database.Pledge oldPledge = JsonSerializer.Deserialize<Database.Pledge>(pledgeChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    pledge.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    pledge.constituentId = oldPledge.constituentId;
				    pledge.totalAmount = oldPledge.totalAmount;
				    pledge.balanceAmount = oldPledge.balanceAmount;
				    pledge.pledgeDate = oldPledge.pledgeDate;
				    pledge.startDate = oldPledge.startDate;
				    pledge.endDate = oldPledge.endDate;
				    pledge.recurrenceFrequencyId = oldPledge.recurrenceFrequencyId;
				    pledge.fundId = oldPledge.fundId;
				    pledge.campaignId = oldPledge.campaignId;
				    pledge.appealId = oldPledge.appealId;
				    pledge.writeOffAmount = oldPledge.writeOffAmount;
				    pledge.isWrittenOff = oldPledge.isWrittenOff;
				    pledge.notes = oldPledge.notes;
				    pledge.objectGuid = oldPledge.objectGuid;
				    pledge.active = oldPledge.active;
				    pledge.deleted = oldPledge.deleted;

				    string serializedPledge = JsonSerializer.Serialize(pledge);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PledgeChangeHistory newPledgeChangeHistory = new PledgeChangeHistory();
				        newPledgeChangeHistory.pledgeId = pledge.id;
				        newPledgeChangeHistory.versionNumber = pledge.versionNumber;
				        newPledgeChangeHistory.timeStamp = DateTime.UtcNow;
				        newPledgeChangeHistory.userId = securityUser.id;
				        newPledgeChangeHistory.tenantGuid = userTenantGuid;
				        newPledgeChangeHistory.data = JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge));
				        _context.PledgeChangeHistories.Add(newPledgeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Pledge rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge)),
						null);


				    return Ok(Database.Pledge.CreateAnonymous(pledge));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Pledge rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Pledge rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Pledge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Pledge</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Pledge/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetPledgeChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Pledge pledge = await _context.Pledges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (pledge == null)
			{
				return NotFound();
			}

			try
			{
				pledge.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Pledge> versionInfo = await pledge.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Pledge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Pledge</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Pledge/{id}/AuditHistory")]
		public async Task<IActionResult> GetPledgeAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Pledge pledge = await _context.Pledges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (pledge == null)
			{
				return NotFound();
			}

			try
			{
				pledge.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Pledge>> versions = await pledge.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Pledge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Pledge</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Pledge object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Pledge/{id}/Version/{version}")]
		public async Task<IActionResult> GetPledgeVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Pledge pledge = await _context.Pledges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (pledge == null)
			{
				return NotFound();
			}

			try
			{
				pledge.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Pledge> versionInfo = await pledge.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Pledge at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Pledge</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Pledge object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Pledge/{id}/StateAtTime")]
		public async Task<IActionResult> GetPledgeStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Pledge pledge = await _context.Pledges.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (pledge == null)
			{
				return NotFound();
			}

			try
			{
				pledge.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Pledge> versionInfo = await pledge.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Pledge record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Pledge/{id}")]
		[Route("api/Pledge")]
		public async Task<IActionResult> DeletePledge(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Pledge> query = (from x in _context.Pledges
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Pledge pledge = await query.FirstOrDefaultAsync(cancellationToken);

			if (pledge == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Pledge DELETE", id.ToString(), new Exception("No Scheduler.Pledge entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Pledge cloneOfExisting = (Database.Pledge)_context.Entry(pledge).GetDatabaseValues().ToObject();


			lock (pledgeDeleteSyncRoot)
			{
			    try
			    {
			        pledge.deleted = true;
			        pledge.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        PledgeChangeHistory pledgeChangeHistory = new PledgeChangeHistory();
			        pledgeChangeHistory.pledgeId = pledge.id;
			        pledgeChangeHistory.versionNumber = pledge.versionNumber;
			        pledgeChangeHistory.timeStamp = DateTime.UtcNow;
			        pledgeChangeHistory.userId = securityUser.id;
			        pledgeChangeHistory.tenantGuid = userTenantGuid;
			        pledgeChangeHistory.data = JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge));
			        _context.PledgeChangeHistories.Add(pledgeChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Pledge entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Pledge entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Pledge.CreateAnonymousWithFirstLevelSubObjects(pledge)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Pledge records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Pledges/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? constituentId = null,
			decimal? totalAmount = null,
			decimal? balanceAmount = null,
			DateOnly? pledgeDate = null,
			DateOnly? startDate = null,
			DateOnly? endDate = null,
			int? recurrenceFrequencyId = null,
			int? fundId = null,
			int? campaignId = null,
			int? appealId = null,
			decimal? writeOffAmount = null,
			bool? isWrittenOff = null,
			string notes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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

			IQueryable<Database.Pledge> query = (from p in _context.Pledges select p);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (constituentId.HasValue == true)
			{
				query = query.Where(p => p.constituentId == constituentId.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(p => p.totalAmount == totalAmount.Value);
			}
			if (balanceAmount.HasValue == true)
			{
				query = query.Where(p => p.balanceAmount == balanceAmount.Value);
			}
			if (recurrenceFrequencyId.HasValue == true)
			{
				query = query.Where(p => p.recurrenceFrequencyId == recurrenceFrequencyId.Value);
			}
			if (fundId.HasValue == true)
			{
				query = query.Where(p => p.fundId == fundId.Value);
			}
			if (campaignId.HasValue == true)
			{
				query = query.Where(p => p.campaignId == campaignId.Value);
			}
			if (appealId.HasValue == true)
			{
				query = query.Where(p => p.appealId == appealId.Value);
			}
			if (writeOffAmount.HasValue == true)
			{
				query = query.Where(p => p.writeOffAmount == writeOffAmount.Value);
			}
			if (isWrittenOff.HasValue == true)
			{
				query = query.Where(p => p.isWrittenOff == isWrittenOff.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(p => p.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(p => p.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(p => p.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(p => p.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(p => p.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(p => p.deleted == false);
				}
			}
			else
			{
				query = query.Where(p => p.active == true);
				query = query.Where(p => p.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Pledge.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Pledge/CreateAuditEvent")]
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
        /// This makes a Pledge record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Pledge/Favourite/{id}")]
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


			IQueryable<Database.Pledge> query = (from x in _context.Pledges
			                               where x.id == id
			                               select x);


			Database.Pledge pledge = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (pledge != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = pledge.id.ToString();
				}

				//
				// Add the user favourite Pledge
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "Pledge", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Pledge' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Pledge' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a Pledge record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Pledge/Favourite/{id}")]
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
			// Delete the user favourite Pledge
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "Pledge", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Pledge' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}


	}
}
