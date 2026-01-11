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
    /// This auto generated class provides the basic CRUD operations for the Fund entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Fund entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class FundsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object fundPutSyncRoot = new object();
		static object fundDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<FundsController> _logger;

		public FundsController(SchedulerContext context, ILogger<FundsController> logger) : base("Scheduler", "Fund")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Funds filtered by the parameters provided.
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
		[Route("api/Funds")]
		public async Task<IActionResult> GetFunds(
			string name = null,
			string description = null,
			string glCode = null,
			bool? isRestricted = null,
			decimal? goalAmount = null,
			string notes = null,
			int? sequence = null,
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

			IQueryable<Database.Fund> query = (from f in _context.Funds select f);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(f => f.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(f => f.description == description);
			}
			if (string.IsNullOrEmpty(glCode) == false)
			{
				query = query.Where(f => f.glCode == glCode);
			}
			if (isRestricted.HasValue == true)
			{
				query = query.Where(f => f.isRestricted == isRestricted.Value);
			}
			if (goalAmount.HasValue == true)
			{
				query = query.Where(f => f.goalAmount == goalAmount.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(f => f.notes == notes);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(f => f.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(f => f.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(f => f.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(f => f.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(f => f.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(f => f.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(f => f.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(f => f.deleted == false);
				}
			}
			else
			{
				query = query.Where(f => f.active == true);
				query = query.Where(f => f.deleted == false);
			}

			query = query.OrderBy(f => f.sequence).ThenBy(f => f.name).ThenBy(f => f.description).ThenBy(f => f.glCode);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.icon);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Fund, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.glCode.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Fund> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Fund fund in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(fund, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Fund Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Fund Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Funds filtered by the parameters provided.  Its query is similar to the GetFunds method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Funds/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string glCode = null,
			bool? isRestricted = null,
			decimal? goalAmount = null,
			string notes = null,
			int? sequence = null,
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


			IQueryable<Database.Fund> query = (from f in _context.Funds select f);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(f => f.name == name);
			}
			if (description != null)
			{
				query = query.Where(f => f.description == description);
			}
			if (glCode != null)
			{
				query = query.Where(f => f.glCode == glCode);
			}
			if (isRestricted.HasValue == true)
			{
				query = query.Where(f => f.isRestricted == isRestricted.Value);
			}
			if (goalAmount.HasValue == true)
			{
				query = query.Where(f => f.goalAmount == goalAmount.Value);
			}
			if (notes != null)
			{
				query = query.Where(f => f.notes == notes);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(f => f.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(f => f.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(f => f.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(f => f.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(f => f.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(f => f.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(f => f.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(f => f.deleted == false);
				}
			}
			else
			{
				query = query.Where(f => f.active == true);
				query = query.Where(f => f.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Fund, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.glCode.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
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
        /// This gets a single Fund by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Fund/{id}")]
		public async Task<IActionResult> GetFund(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Fund> query = (from f in _context.Funds where
							(f.id == id) &&
							(userIsAdmin == true || f.deleted == false) &&
							(userIsWriter == true || f.active == true)
					select f);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.icon);
					query = query.AsSplitQuery();
				}

				Database.Fund materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Fund Entity was read with Admin privilege." : "Scheduler.Fund Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Fund", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Fund entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Fund.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Fund.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Fund record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Fund/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutFund(int id, [FromBody]Database.Fund.FundDTO fundDTO, CancellationToken cancellationToken = default)
		{
			if (fundDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != fundDTO.id)
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


			IQueryable<Database.Fund> query = (from x in _context.Funds
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Fund existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Fund PUT", id.ToString(), new Exception("No Scheduler.Fund entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (fundDTO.objectGuid == Guid.Empty)
            {
                fundDTO.objectGuid = existing.objectGuid;
            }
            else if (fundDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Fund record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Fund cloneOfExisting = (Database.Fund)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Fund object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Fund fund = (Database.Fund)_context.Entry(existing).GetDatabaseValues().ToObject();
			fund.ApplyDTO(fundDTO);
			//
			// The tenant guid for any Fund being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Fund because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				fund.tenantGuid = existing.tenantGuid;
			}

			lock (fundPutSyncRoot)
			{
				//
				// Validate the version number for the fund being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != fund.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Fund save attempt was made but save request was with version " + fund.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Fund you are trying to update has already changed.  Please try your save again after reloading the Fund.");
				}
				else
				{
					// Same record.  Increase version.
					fund.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (fund.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Fund record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (fund.name != null && fund.name.Length > 100)
				{
					fund.name = fund.name.Substring(0, 100);
				}

				if (fund.description != null && fund.description.Length > 500)
				{
					fund.description = fund.description.Substring(0, 500);
				}

				if (fund.glCode != null && fund.glCode.Length > 100)
				{
					fund.glCode = fund.glCode.Substring(0, 100);
				}

				if (fund.color != null && fund.color.Length > 10)
				{
					fund.color = fund.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.Fund> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(fund);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        FundChangeHistory fundChangeHistory = new FundChangeHistory();
				        fundChangeHistory.fundId = fund.id;
				        fundChangeHistory.versionNumber = fund.versionNumber;
				        fundChangeHistory.timeStamp = DateTime.UtcNow;
				        fundChangeHistory.userId = securityUser.id;
				        fundChangeHistory.tenantGuid = userTenantGuid;
				        fundChangeHistory.data = JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund));
				        _context.FundChangeHistories.Add(fundChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Fund entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund)),
						null);

				return Ok(Database.Fund.CreateAnonymous(fund));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Fund entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Fund record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Fund", Name = "Fund")]
		public async Task<IActionResult> PostFund([FromBody]Database.Fund.FundDTO fundDTO, CancellationToken cancellationToken = default)
		{
			if (fundDTO == null)
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
			// Create a new Fund object using the data from the DTO
			//
			Database.Fund fund = Database.Fund.FromDTO(fundDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				fund.tenantGuid = userTenantGuid;

				if (fund.name != null && fund.name.Length > 100)
				{
					fund.name = fund.name.Substring(0, 100);
				}

				if (fund.description != null && fund.description.Length > 500)
				{
					fund.description = fund.description.Substring(0, 500);
				}

				if (fund.glCode != null && fund.glCode.Length > 100)
				{
					fund.glCode = fund.glCode.Substring(0, 100);
				}

				if (fund.color != null && fund.color.Length > 10)
				{
					fund.color = fund.color.Substring(0, 10);
				}

				fund.objectGuid = Guid.NewGuid();
				fund.versionNumber = 1;

				_context.Funds.Add(fund);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the fund object so that no further changes will be written to the database
				    //
				    _context.Entry(fund).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					fund.Batches = null;
					fund.FundChangeHistories = null;
					fund.Gifts = null;
					fund.Pledges = null;
					fund.icon = null;


				    FundChangeHistory fundChangeHistory = new FundChangeHistory();
				    fundChangeHistory.fundId = fund.id;
				    fundChangeHistory.versionNumber = fund.versionNumber;
				    fundChangeHistory.timeStamp = DateTime.UtcNow;
				    fundChangeHistory.userId = securityUser.id;
				    fundChangeHistory.tenantGuid = userTenantGuid;
				    fundChangeHistory.data = JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund));
				    _context.FundChangeHistories.Add(fundChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Fund entity successfully created.",
						true,
						fund. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Fund entity creation failed.", false, fund.id.ToString(), "", JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Fund", fund.id, fund.name));

			return CreatedAtRoute("Fund", new { id = fund.id }, Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund));
		}



        /// <summary>
        /// 
        /// This rolls a Fund entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Fund/Rollback/{id}")]
		[Route("api/Fund/Rollback")]
		public async Task<IActionResult> RollbackToFundVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Fund> query = (from x in _context.Funds
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Fund concurrently
			//
			lock (fundPutSyncRoot)
			{
				
				Database.Fund fund = query.FirstOrDefault();
				
				if (fund == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Fund rollback", id.ToString(), new Exception("No Scheduler.Fund entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Fund current state so we can log it.
				//
				Database.Fund cloneOfExisting = (Database.Fund)_context.Entry(fund).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Batches = null;
				cloneOfExisting.FundChangeHistories = null;
				cloneOfExisting.Gifts = null;
				cloneOfExisting.Pledges = null;
				cloneOfExisting.icon = null;

				if (versionNumber >= fund.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Fund rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Fund rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				FundChangeHistory fundChangeHistory = (from x in _context.FundChangeHistories
				                                               where
				                                               x.fundId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (fundChangeHistory != null)
				{
				    Database.Fund oldFund = JsonSerializer.Deserialize<Database.Fund>(fundChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    fund.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    fund.name = oldFund.name;
				    fund.description = oldFund.description;
				    fund.glCode = oldFund.glCode;
				    fund.isRestricted = oldFund.isRestricted;
				    fund.goalAmount = oldFund.goalAmount;
				    fund.notes = oldFund.notes;
				    fund.sequence = oldFund.sequence;
				    fund.iconId = oldFund.iconId;
				    fund.color = oldFund.color;
				    fund.objectGuid = oldFund.objectGuid;
				    fund.active = oldFund.active;
				    fund.deleted = oldFund.deleted;

				    string serializedFund = JsonSerializer.Serialize(fund);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        FundChangeHistory newFundChangeHistory = new FundChangeHistory();
				        newFundChangeHistory.fundId = fund.id;
				        newFundChangeHistory.versionNumber = fund.versionNumber;
				        newFundChangeHistory.timeStamp = DateTime.UtcNow;
				        newFundChangeHistory.userId = securityUser.id;
				        newFundChangeHistory.tenantGuid = userTenantGuid;
				        newFundChangeHistory.data = JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund));
				        _context.FundChangeHistories.Add(newFundChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Fund rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund)),
						null);


				    return Ok(Database.Fund.CreateAnonymous(fund));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Fund rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Fund rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}


        /// <summary>
        /// 
        /// This deletes a Fund record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Fund/{id}")]
		[Route("api/Fund")]
		public async Task<IActionResult> DeleteFund(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Fund> query = (from x in _context.Funds
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Fund fund = await query.FirstOrDefaultAsync(cancellationToken);

			if (fund == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Fund DELETE", id.ToString(), new Exception("No Scheduler.Fund entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Fund cloneOfExisting = (Database.Fund)_context.Entry(fund).GetDatabaseValues().ToObject();


			lock (fundDeleteSyncRoot)
			{
			    try
			    {
			        fund.deleted = true;
			        fund.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        FundChangeHistory fundChangeHistory = new FundChangeHistory();
			        fundChangeHistory.fundId = fund.id;
			        fundChangeHistory.versionNumber = fund.versionNumber;
			        fundChangeHistory.timeStamp = DateTime.UtcNow;
			        fundChangeHistory.userId = securityUser.id;
			        fundChangeHistory.tenantGuid = userTenantGuid;
			        fundChangeHistory.data = JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund));
			        _context.FundChangeHistories.Add(fundChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Fund entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Fund entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Fund.CreateAnonymousWithFirstLevelSubObjects(fund)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Fund records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Funds/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string glCode = null,
			bool? isRestricted = null,
			decimal? goalAmount = null,
			string notes = null,
			int? sequence = null,
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

			IQueryable<Database.Fund> query = (from f in _context.Funds select f);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(f => f.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(f => f.description == description);
			}
			if (string.IsNullOrEmpty(glCode) == false)
			{
				query = query.Where(f => f.glCode == glCode);
			}
			if (isRestricted.HasValue == true)
			{
				query = query.Where(f => f.isRestricted == isRestricted.Value);
			}
			if (goalAmount.HasValue == true)
			{
				query = query.Where(f => f.goalAmount == goalAmount.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(f => f.notes == notes);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(f => f.sequence == sequence.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(f => f.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(f => f.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(f => f.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(f => f.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(f => f.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(f => f.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(f => f.deleted == false);
				}
			}
			else
			{
				query = query.Where(f => f.active == true);
				query = query.Where(f => f.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Fund, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.glCode.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.glCode);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Fund.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Fund/CreateAuditEvent")]
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
