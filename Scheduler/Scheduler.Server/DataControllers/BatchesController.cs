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
    /// This auto generated class provides the basic CRUD operations for the Batch entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Batch entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BatchesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object batchPutSyncRoot = new object();
		static object batchDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<BatchesController> _logger;

		public BatchesController(SchedulerContext context, ILogger<BatchesController> logger) : base("Scheduler", "Batch")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Batches filtered by the parameters provided.
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
		[Route("api/Batches")]
		public async Task<IActionResult> GetBatches(
			string batchNumber = null,
			string description = null,
			DateTime? dateOpened = null,
			DateTime? datePosted = null,
			int? batchStatusId = null,
			decimal? controlAmount = null,
			int? controlCount = null,
			int? defaultFundId = null,
			int? defaultCampaignId = null,
			int? defaultAppealId = null,
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
			if (dateOpened.HasValue == true && dateOpened.Value.Kind != DateTimeKind.Utc)
			{
				dateOpened = dateOpened.Value.ToUniversalTime();
			}

			if (datePosted.HasValue == true && datePosted.Value.Kind != DateTimeKind.Utc)
			{
				datePosted = datePosted.Value.ToUniversalTime();
			}

			IQueryable<Database.Batch> query = (from b in _context.Batches select b);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(batchNumber) == false)
			{
				query = query.Where(b => b.batchNumber == batchNumber);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(b => b.description == description);
			}
			if (dateOpened.HasValue == true)
			{
				query = query.Where(b => b.dateOpened == dateOpened.Value);
			}
			if (datePosted.HasValue == true)
			{
				query = query.Where(b => b.datePosted == datePosted.Value);
			}
			if (batchStatusId.HasValue == true)
			{
				query = query.Where(b => b.batchStatusId == batchStatusId.Value);
			}
			if (controlAmount.HasValue == true)
			{
				query = query.Where(b => b.controlAmount == controlAmount.Value);
			}
			if (controlCount.HasValue == true)
			{
				query = query.Where(b => b.controlCount == controlCount.Value);
			}
			if (defaultFundId.HasValue == true)
			{
				query = query.Where(b => b.defaultFundId == defaultFundId.Value);
			}
			if (defaultCampaignId.HasValue == true)
			{
				query = query.Where(b => b.defaultCampaignId == defaultCampaignId.Value);
			}
			if (defaultAppealId.HasValue == true)
			{
				query = query.Where(b => b.defaultAppealId == defaultAppealId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(b => b.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(b => b.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(b => b.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(b => b.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(b => b.deleted == false);
				}
			}
			else
			{
				query = query.Where(b => b.active == true);
				query = query.Where(b => b.deleted == false);
			}

			query = query.OrderBy(b => b.batchNumber).ThenBy(b => b.description);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.batchStatus);
				query = query.Include(x => x.defaultAppeal);
				query = query.Include(x => x.defaultCampaign);
				query = query.Include(x => x.defaultFund);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Batch, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.batchNumber.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.batchStatus.name.Contains(anyStringContains))
			       || (includeRelations == true && x.batchStatus.description.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultAppeal.name.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultAppeal.description.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultAppeal.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultAppeal.color.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultCampaign.name.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultCampaign.description.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultCampaign.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultCampaign.color.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultFund.name.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultFund.description.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultFund.glCode.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultFund.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultFund.color.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Batch> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Batch batch in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(batch, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Batch Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Batch Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Batches filtered by the parameters provided.  Its query is similar to the GetBatches method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Batches/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string batchNumber = null,
			string description = null,
			DateTime? dateOpened = null,
			DateTime? datePosted = null,
			int? batchStatusId = null,
			decimal? controlAmount = null,
			int? controlCount = null,
			int? defaultFundId = null,
			int? defaultCampaignId = null,
			int? defaultAppealId = null,
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
			if (dateOpened.HasValue == true && dateOpened.Value.Kind != DateTimeKind.Utc)
			{
				dateOpened = dateOpened.Value.ToUniversalTime();
			}

			if (datePosted.HasValue == true && datePosted.Value.Kind != DateTimeKind.Utc)
			{
				datePosted = datePosted.Value.ToUniversalTime();
			}

			IQueryable<Database.Batch> query = (from b in _context.Batches select b);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (batchNumber != null)
			{
				query = query.Where(b => b.batchNumber == batchNumber);
			}
			if (description != null)
			{
				query = query.Where(b => b.description == description);
			}
			if (dateOpened.HasValue == true)
			{
				query = query.Where(b => b.dateOpened == dateOpened.Value);
			}
			if (datePosted.HasValue == true)
			{
				query = query.Where(b => b.datePosted == datePosted.Value);
			}
			if (batchStatusId.HasValue == true)
			{
				query = query.Where(b => b.batchStatusId == batchStatusId.Value);
			}
			if (controlAmount.HasValue == true)
			{
				query = query.Where(b => b.controlAmount == controlAmount.Value);
			}
			if (controlCount.HasValue == true)
			{
				query = query.Where(b => b.controlCount == controlCount.Value);
			}
			if (defaultFundId.HasValue == true)
			{
				query = query.Where(b => b.defaultFundId == defaultFundId.Value);
			}
			if (defaultCampaignId.HasValue == true)
			{
				query = query.Where(b => b.defaultCampaignId == defaultCampaignId.Value);
			}
			if (defaultAppealId.HasValue == true)
			{
				query = query.Where(b => b.defaultAppealId == defaultAppealId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(b => b.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(b => b.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(b => b.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(b => b.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(b => b.deleted == false);
				}
			}
			else
			{
				query = query.Where(b => b.active == true);
				query = query.Where(b => b.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Batch, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.batchNumber.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.batchStatus.name.Contains(anyStringContains)
			       || x.batchStatus.description.Contains(anyStringContains)
			       || x.defaultAppeal.name.Contains(anyStringContains)
			       || x.defaultAppeal.description.Contains(anyStringContains)
			       || x.defaultAppeal.notes.Contains(anyStringContains)
			       || x.defaultAppeal.color.Contains(anyStringContains)
			       || x.defaultCampaign.name.Contains(anyStringContains)
			       || x.defaultCampaign.description.Contains(anyStringContains)
			       || x.defaultCampaign.notes.Contains(anyStringContains)
			       || x.defaultCampaign.color.Contains(anyStringContains)
			       || x.defaultFund.name.Contains(anyStringContains)
			       || x.defaultFund.description.Contains(anyStringContains)
			       || x.defaultFund.glCode.Contains(anyStringContains)
			       || x.defaultFund.notes.Contains(anyStringContains)
			       || x.defaultFund.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Batch by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Batch/{id}")]
		public async Task<IActionResult> GetBatch(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Batch> query = (from b in _context.Batches where
							(b.id == id) &&
							(userIsAdmin == true || b.deleted == false) &&
							(userIsWriter == true || b.active == true)
					select b);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.batchStatus);
					query = query.Include(x => x.defaultAppeal);
					query = query.Include(x => x.defaultCampaign);
					query = query.Include(x => x.defaultFund);
					query = query.AsSplitQuery();
				}

				Database.Batch materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Batch Entity was read with Admin privilege." : "Scheduler.Batch Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Batch", materialized.id, materialized.batchNumber));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Batch entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Batch.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Batch.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Batch record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Batch/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBatch(int id, [FromBody]Database.Batch.BatchDTO batchDTO, CancellationToken cancellationToken = default)
		{
			if (batchDTO == null)
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



			if (id != batchDTO.id)
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


			IQueryable<Database.Batch> query = (from x in _context.Batches
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Batch existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Batch PUT", id.ToString(), new Exception("No Scheduler.Batch entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (batchDTO.objectGuid == Guid.Empty)
            {
                batchDTO.objectGuid = existing.objectGuid;
            }
            else if (batchDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Batch record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Batch cloneOfExisting = (Database.Batch)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Batch object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Batch batch = (Database.Batch)_context.Entry(existing).GetDatabaseValues().ToObject();
			batch.ApplyDTO(batchDTO);
			//
			// The tenant guid for any Batch being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Batch because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				batch.tenantGuid = existing.tenantGuid;
			}

			lock (batchPutSyncRoot)
			{
				//
				// Validate the version number for the batch being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != batch.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Batch save attempt was made but save request was with version " + batch.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Batch you are trying to update has already changed.  Please try your save again after reloading the Batch.");
				}
				else
				{
					// Same record.  Increase version.
					batch.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (batch.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Batch record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (batch.batchNumber != null && batch.batchNumber.Length > 100)
				{
					batch.batchNumber = batch.batchNumber.Substring(0, 100);
				}

				if (batch.description != null && batch.description.Length > 500)
				{
					batch.description = batch.description.Substring(0, 500);
				}

				if (batch.dateOpened.Kind != DateTimeKind.Utc)
				{
					batch.dateOpened = batch.dateOpened.ToUniversalTime();
				}

				if (batch.datePosted.HasValue == true && batch.datePosted.Value.Kind != DateTimeKind.Utc)
				{
					batch.datePosted = batch.datePosted.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.Batch> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(batch);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BatchChangeHistory batchChangeHistory = new BatchChangeHistory();
				        batchChangeHistory.batchId = batch.id;
				        batchChangeHistory.versionNumber = batch.versionNumber;
				        batchChangeHistory.timeStamp = DateTime.UtcNow;
				        batchChangeHistory.userId = securityUser.id;
				        batchChangeHistory.tenantGuid = userTenantGuid;
				        batchChangeHistory.data = JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch));
				        _context.BatchChangeHistories.Add(batchChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Batch entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch)),
						null);

				return Ok(Database.Batch.CreateAnonymous(batch));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Batch entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Batch record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Batch", Name = "Batch")]
		public async Task<IActionResult> PostBatch([FromBody]Database.Batch.BatchDTO batchDTO, CancellationToken cancellationToken = default)
		{
			if (batchDTO == null)
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
			// Create a new Batch object using the data from the DTO
			//
			Database.Batch batch = Database.Batch.FromDTO(batchDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				batch.tenantGuid = userTenantGuid;

				if (batch.batchNumber != null && batch.batchNumber.Length > 100)
				{
					batch.batchNumber = batch.batchNumber.Substring(0, 100);
				}

				if (batch.description != null && batch.description.Length > 500)
				{
					batch.description = batch.description.Substring(0, 500);
				}

				if (batch.dateOpened.Kind != DateTimeKind.Utc)
				{
					batch.dateOpened = batch.dateOpened.ToUniversalTime();
				}

				if (batch.datePosted.HasValue == true && batch.datePosted.Value.Kind != DateTimeKind.Utc)
				{
					batch.datePosted = batch.datePosted.Value.ToUniversalTime();
				}

				batch.objectGuid = Guid.NewGuid();
				batch.versionNumber = 1;

				_context.Batches.Add(batch);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the batch object so that no further changes will be written to the database
				    //
				    _context.Entry(batch).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					batch.BatchChangeHistories = null;
					batch.Gifts = null;
					batch.batchStatus = null;
					batch.defaultAppeal = null;
					batch.defaultCampaign = null;
					batch.defaultFund = null;


				    BatchChangeHistory batchChangeHistory = new BatchChangeHistory();
				    batchChangeHistory.batchId = batch.id;
				    batchChangeHistory.versionNumber = batch.versionNumber;
				    batchChangeHistory.timeStamp = DateTime.UtcNow;
				    batchChangeHistory.userId = securityUser.id;
				    batchChangeHistory.tenantGuid = userTenantGuid;
				    batchChangeHistory.data = JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch));
				    _context.BatchChangeHistories.Add(batchChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Batch entity successfully created.",
						true,
						batch. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Batch entity creation failed.", false, batch.id.ToString(), "", JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Batch", batch.id, batch.batchNumber));

			return CreatedAtRoute("Batch", new { id = batch.id }, Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch));
		}



        /// <summary>
        /// 
        /// This rolls a Batch entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Batch/Rollback/{id}")]
		[Route("api/Batch/Rollback")]
		public async Task<IActionResult> RollbackToBatchVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Batch> query = (from x in _context.Batches
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Batch concurrently
			//
			lock (batchPutSyncRoot)
			{
				
				Database.Batch batch = query.FirstOrDefault();
				
				if (batch == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Batch rollback", id.ToString(), new Exception("No Scheduler.Batch entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Batch current state so we can log it.
				//
				Database.Batch cloneOfExisting = (Database.Batch)_context.Entry(batch).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.BatchChangeHistories = null;
				cloneOfExisting.Gifts = null;
				cloneOfExisting.batchStatus = null;
				cloneOfExisting.defaultAppeal = null;
				cloneOfExisting.defaultCampaign = null;
				cloneOfExisting.defaultFund = null;

				if (versionNumber >= batch.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Batch rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Batch rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				BatchChangeHistory batchChangeHistory = (from x in _context.BatchChangeHistories
				                                               where
				                                               x.batchId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (batchChangeHistory != null)
				{
				    Database.Batch oldBatch = JsonSerializer.Deserialize<Database.Batch>(batchChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    batch.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    batch.batchNumber = oldBatch.batchNumber;
				    batch.description = oldBatch.description;
				    batch.dateOpened = oldBatch.dateOpened;
				    batch.datePosted = oldBatch.datePosted;
				    batch.batchStatusId = oldBatch.batchStatusId;
				    batch.controlAmount = oldBatch.controlAmount;
				    batch.controlCount = oldBatch.controlCount;
				    batch.defaultFundId = oldBatch.defaultFundId;
				    batch.defaultCampaignId = oldBatch.defaultCampaignId;
				    batch.defaultAppealId = oldBatch.defaultAppealId;
				    batch.defaultDate = oldBatch.defaultDate;
				    batch.objectGuid = oldBatch.objectGuid;
				    batch.active = oldBatch.active;
				    batch.deleted = oldBatch.deleted;

				    string serializedBatch = JsonSerializer.Serialize(batch);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BatchChangeHistory newBatchChangeHistory = new BatchChangeHistory();
				        newBatchChangeHistory.batchId = batch.id;
				        newBatchChangeHistory.versionNumber = batch.versionNumber;
				        newBatchChangeHistory.timeStamp = DateTime.UtcNow;
				        newBatchChangeHistory.userId = securityUser.id;
				        newBatchChangeHistory.tenantGuid = userTenantGuid;
				        newBatchChangeHistory.data = JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch));
				        _context.BatchChangeHistories.Add(newBatchChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Batch rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch)),
						null);


				    return Ok(Database.Batch.CreateAnonymous(batch));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Batch rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Batch rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Batch.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Batch</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Batch/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetBatchChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Batch batch = await _context.Batches.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (batch == null)
			{
				return NotFound();
			}

			try
			{
				batch.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Batch> versionInfo = await batch.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Batch.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Batch</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Batch/{id}/AuditHistory")]
		public async Task<IActionResult> GetBatchAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Batch batch = await _context.Batches.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (batch == null)
			{
				return NotFound();
			}

			try
			{
				batch.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Batch>> versions = await batch.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Batch.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Batch</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Batch object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Batch/{id}/Version/{version}")]
		public async Task<IActionResult> GetBatchVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Batch batch = await _context.Batches.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (batch == null)
			{
				return NotFound();
			}

			try
			{
				batch.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Batch> versionInfo = await batch.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Batch at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Batch</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Batch object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Batch/{id}/StateAtTime")]
		public async Task<IActionResult> GetBatchStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Batch batch = await _context.Batches.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (batch == null)
			{
				return NotFound();
			}

			try
			{
				batch.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Batch> versionInfo = await batch.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Batch record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Batch/{id}")]
		[Route("api/Batch")]
		public async Task<IActionResult> DeleteBatch(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Batch> query = (from x in _context.Batches
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Batch batch = await query.FirstOrDefaultAsync(cancellationToken);

			if (batch == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Batch DELETE", id.ToString(), new Exception("No Scheduler.Batch entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Batch cloneOfExisting = (Database.Batch)_context.Entry(batch).GetDatabaseValues().ToObject();


			lock (batchDeleteSyncRoot)
			{
			    try
			    {
			        batch.deleted = true;
			        batch.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        BatchChangeHistory batchChangeHistory = new BatchChangeHistory();
			        batchChangeHistory.batchId = batch.id;
			        batchChangeHistory.versionNumber = batch.versionNumber;
			        batchChangeHistory.timeStamp = DateTime.UtcNow;
			        batchChangeHistory.userId = securityUser.id;
			        batchChangeHistory.tenantGuid = userTenantGuid;
			        batchChangeHistory.data = JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch));
			        _context.BatchChangeHistories.Add(batchChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Batch entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Batch entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Batch.CreateAnonymousWithFirstLevelSubObjects(batch)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Batch records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Batches/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string batchNumber = null,
			string description = null,
			DateTime? dateOpened = null,
			DateTime? datePosted = null,
			int? batchStatusId = null,
			decimal? controlAmount = null,
			int? controlCount = null,
			int? defaultFundId = null,
			int? defaultCampaignId = null,
			int? defaultAppealId = null,
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
			if (dateOpened.HasValue == true && dateOpened.Value.Kind != DateTimeKind.Utc)
			{
				dateOpened = dateOpened.Value.ToUniversalTime();
			}

			if (datePosted.HasValue == true && datePosted.Value.Kind != DateTimeKind.Utc)
			{
				datePosted = datePosted.Value.ToUniversalTime();
			}

			IQueryable<Database.Batch> query = (from b in _context.Batches select b);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(batchNumber) == false)
			{
				query = query.Where(b => b.batchNumber == batchNumber);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(b => b.description == description);
			}
			if (dateOpened.HasValue == true)
			{
				query = query.Where(b => b.dateOpened == dateOpened.Value);
			}
			if (datePosted.HasValue == true)
			{
				query = query.Where(b => b.datePosted == datePosted.Value);
			}
			if (batchStatusId.HasValue == true)
			{
				query = query.Where(b => b.batchStatusId == batchStatusId.Value);
			}
			if (controlAmount.HasValue == true)
			{
				query = query.Where(b => b.controlAmount == controlAmount.Value);
			}
			if (controlCount.HasValue == true)
			{
				query = query.Where(b => b.controlCount == controlCount.Value);
			}
			if (defaultFundId.HasValue == true)
			{
				query = query.Where(b => b.defaultFundId == defaultFundId.Value);
			}
			if (defaultCampaignId.HasValue == true)
			{
				query = query.Where(b => b.defaultCampaignId == defaultCampaignId.Value);
			}
			if (defaultAppealId.HasValue == true)
			{
				query = query.Where(b => b.defaultAppealId == defaultAppealId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(b => b.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(b => b.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(b => b.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(b => b.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(b => b.deleted == false);
				}
			}
			else
			{
				query = query.Where(b => b.active == true);
				query = query.Where(b => b.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Batch, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.batchNumber.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.batchStatus.name.Contains(anyStringContains)
			       || x.batchStatus.description.Contains(anyStringContains)
			       || x.defaultAppeal.name.Contains(anyStringContains)
			       || x.defaultAppeal.description.Contains(anyStringContains)
			       || x.defaultAppeal.notes.Contains(anyStringContains)
			       || x.defaultAppeal.color.Contains(anyStringContains)
			       || x.defaultCampaign.name.Contains(anyStringContains)
			       || x.defaultCampaign.description.Contains(anyStringContains)
			       || x.defaultCampaign.notes.Contains(anyStringContains)
			       || x.defaultCampaign.color.Contains(anyStringContains)
			       || x.defaultFund.name.Contains(anyStringContains)
			       || x.defaultFund.description.Contains(anyStringContains)
			       || x.defaultFund.glCode.Contains(anyStringContains)
			       || x.defaultFund.notes.Contains(anyStringContains)
			       || x.defaultFund.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.batchNumber).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Batch.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Batch/CreateAuditEvent")]
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
