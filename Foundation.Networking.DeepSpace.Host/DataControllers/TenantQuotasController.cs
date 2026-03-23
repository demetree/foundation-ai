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
using Foundation.DeepSpace.Database;
using Foundation.ChangeHistory;

namespace Foundation.DeepSpace.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the TenantQuota entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TenantQuota entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TenantQuotasController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		static object tenantQuotaPutSyncRoot = new object();
		static object tenantQuotaDeleteSyncRoot = new object();

		private DeepSpaceContext _context;

		private ILogger<TenantQuotasController> _logger;

		public TenantQuotasController(DeepSpaceContext context, ILogger<TenantQuotasController> logger) : base("DeepSpace", "TenantQuota")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TenantQuotas filtered by the parameters provided.
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
		[Route("api/TenantQuotas")]
		public async Task<IActionResult> GetTenantQuotas(
			int? maxStorageBytes = null,
			int? currentUsageBytes = null,
			int? maxObjectCount = null,
			int? currentObjectCount = null,
			int? alertThresholdPercent = null,
			bool? isEnforced = null,
			DateTime? lastRecalculatedUtc = null,
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
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
			if (lastRecalculatedUtc.HasValue == true && lastRecalculatedUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastRecalculatedUtc = lastRecalculatedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.TenantQuota> query = (from tq in _context.TenantQuotas select tq);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (maxStorageBytes.HasValue == true)
			{
				query = query.Where(tq => tq.maxStorageBytes == maxStorageBytes.Value);
			}
			if (currentUsageBytes.HasValue == true)
			{
				query = query.Where(tq => tq.currentUsageBytes == currentUsageBytes.Value);
			}
			if (maxObjectCount.HasValue == true)
			{
				query = query.Where(tq => tq.maxObjectCount == maxObjectCount.Value);
			}
			if (currentObjectCount.HasValue == true)
			{
				query = query.Where(tq => tq.currentObjectCount == currentObjectCount.Value);
			}
			if (alertThresholdPercent.HasValue == true)
			{
				query = query.Where(tq => tq.alertThresholdPercent == alertThresholdPercent.Value);
			}
			if (isEnforced.HasValue == true)
			{
				query = query.Where(tq => tq.isEnforced == isEnforced.Value);
			}
			if (lastRecalculatedUtc.HasValue == true)
			{
				query = query.Where(tq => tq.lastRecalculatedUtc == lastRecalculatedUtc.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(tq => tq.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tq => tq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tq => tq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tq => tq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tq => tq.deleted == false);
				}
			}
			else
			{
				query = query.Where(tq => tq.active == true);
				query = query.Where(tq => tq.deleted == false);
			}

			query = query.OrderBy(tq => tq.id);

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.TenantQuota> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TenantQuota tenantQuota in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(tenantQuota, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "DeepSpace.TenantQuota Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "DeepSpace.TenantQuota Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TenantQuotas filtered by the parameters provided.  Its query is similar to the GetTenantQuotas method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantQuotas/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? maxStorageBytes = null,
			int? currentUsageBytes = null,
			int? maxObjectCount = null,
			int? currentObjectCount = null,
			int? alertThresholdPercent = null,
			bool? isEnforced = null,
			DateTime? lastRecalculatedUtc = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			CancellationToken cancellationToken = default)
		{
			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
			if (lastRecalculatedUtc.HasValue == true && lastRecalculatedUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastRecalculatedUtc = lastRecalculatedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.TenantQuota> query = (from tq in _context.TenantQuotas select tq);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (maxStorageBytes.HasValue == true)
			{
				query = query.Where(tq => tq.maxStorageBytes == maxStorageBytes.Value);
			}
			if (currentUsageBytes.HasValue == true)
			{
				query = query.Where(tq => tq.currentUsageBytes == currentUsageBytes.Value);
			}
			if (maxObjectCount.HasValue == true)
			{
				query = query.Where(tq => tq.maxObjectCount == maxObjectCount.Value);
			}
			if (currentObjectCount.HasValue == true)
			{
				query = query.Where(tq => tq.currentObjectCount == currentObjectCount.Value);
			}
			if (alertThresholdPercent.HasValue == true)
			{
				query = query.Where(tq => tq.alertThresholdPercent == alertThresholdPercent.Value);
			}
			if (isEnforced.HasValue == true)
			{
				query = query.Where(tq => tq.isEnforced == isEnforced.Value);
			}
			if (lastRecalculatedUtc.HasValue == true)
			{
				query = query.Where(tq => tq.lastRecalculatedUtc == lastRecalculatedUtc.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(tq => tq.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tq => tq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tq => tq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tq => tq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tq => tq.deleted == false);
				}
			}
			else
			{
				query = query.Where(tq => tq.active == true);
				query = query.Where(tq => tq.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single TenantQuota by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantQuota/{id}")]
		public async Task<IActionResult> GetTenantQuota(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
				IQueryable<Database.TenantQuota> query = (from tq in _context.TenantQuotas where
							(tq.id == id) &&
							(userIsAdmin == true || tq.deleted == false) &&
							(userIsWriter == true || tq.active == true)
					select tq);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.TenantQuota materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "DeepSpace.TenantQuota Entity was read with Admin privilege." : "DeepSpace.TenantQuota Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TenantQuota", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a DeepSpace.TenantQuota entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of DeepSpace.TenantQuota.   Entity was read with Admin privilege." : "Exception caught during entity read of DeepSpace.TenantQuota.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TenantQuota record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TenantQuota/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTenantQuota(int id, [FromBody]Database.TenantQuota.TenantQuotaDTO tenantQuotaDTO, CancellationToken cancellationToken = default)
		{
			if (tenantQuotaDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != tenantQuotaDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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


			IQueryable<Database.TenantQuota> query = (from x in _context.TenantQuotas
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.TenantQuota existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.TenantQuota PUT", id.ToString(), new Exception("No DeepSpace.TenantQuota entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (tenantQuotaDTO.objectGuid == Guid.Empty)
            {
                tenantQuotaDTO.objectGuid = existing.objectGuid;
            }
            else if (tenantQuotaDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a TenantQuota record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TenantQuota cloneOfExisting = (Database.TenantQuota)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TenantQuota object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TenantQuota tenantQuota = (Database.TenantQuota)_context.Entry(existing).GetDatabaseValues().ToObject();
			tenantQuota.ApplyDTO(tenantQuotaDTO);
			//
			// The tenant guid for any TenantQuota being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the TenantQuota because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				tenantQuota.tenantGuid = existing.tenantGuid;
			}

			lock (tenantQuotaPutSyncRoot)
			{
				//
				// Validate the version number for the tenantQuota being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != tenantQuota.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "TenantQuota save attempt was made but save request was with version " + tenantQuota.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The TenantQuota you are trying to update has already changed.  Please try your save again after reloading the TenantQuota.");
				}
				else
				{
					// Same record.  Increase version.
					tenantQuota.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (tenantQuota.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted DeepSpace.TenantQuota record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (tenantQuota.lastRecalculatedUtc.HasValue == true && tenantQuota.lastRecalculatedUtc.Value.Kind != DateTimeKind.Utc)
				{
					tenantQuota.lastRecalculatedUtc = tenantQuota.lastRecalculatedUtc.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.TenantQuota> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(tenantQuota);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        TenantQuotaChangeHistory tenantQuotaChangeHistory = new TenantQuotaChangeHistory();
				        tenantQuotaChangeHistory.tenantQuotaId = tenantQuota.id;
				        tenantQuotaChangeHistory.versionNumber = tenantQuota.versionNumber;
				        tenantQuotaChangeHistory.timeStamp = DateTime.UtcNow;
				        tenantQuotaChangeHistory.userId = securityUser.id;
				        tenantQuotaChangeHistory.tenantGuid = userTenantGuid;
				        tenantQuotaChangeHistory.data = JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota));
				        _context.TenantQuotaChangeHistories.Add(tenantQuotaChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.TenantQuota entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota)),
						null);

				return Ok(Database.TenantQuota.CreateAnonymous(tenantQuota));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.TenantQuota entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new TenantQuota record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantQuota", Name = "TenantQuota")]
		public async Task<IActionResult> PostTenantQuota([FromBody]Database.TenantQuota.TenantQuotaDTO tenantQuotaDTO, CancellationToken cancellationToken = default)
		{
			if (tenantQuotaDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
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
			// Create a new TenantQuota object using the data from the DTO
			//
			Database.TenantQuota tenantQuota = Database.TenantQuota.FromDTO(tenantQuotaDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				tenantQuota.tenantGuid = userTenantGuid;

				if (tenantQuota.lastRecalculatedUtc.HasValue == true && tenantQuota.lastRecalculatedUtc.Value.Kind != DateTimeKind.Utc)
				{
					tenantQuota.lastRecalculatedUtc = tenantQuota.lastRecalculatedUtc.Value.ToUniversalTime();
				}

				tenantQuota.objectGuid = Guid.NewGuid();
				tenantQuota.versionNumber = 1;

				_context.TenantQuotas.Add(tenantQuota);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the tenantQuota object so that no further changes will be written to the database
				    //
				    _context.Entry(tenantQuota).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					tenantQuota.TenantQuotaChangeHistories = null;


				    TenantQuotaChangeHistory tenantQuotaChangeHistory = new TenantQuotaChangeHistory();
				    tenantQuotaChangeHistory.tenantQuotaId = tenantQuota.id;
				    tenantQuotaChangeHistory.versionNumber = tenantQuota.versionNumber;
				    tenantQuotaChangeHistory.timeStamp = DateTime.UtcNow;
				    tenantQuotaChangeHistory.userId = securityUser.id;
				    tenantQuotaChangeHistory.tenantGuid = userTenantGuid;
				    tenantQuotaChangeHistory.data = JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota));
				    _context.TenantQuotaChangeHistories.Add(tenantQuotaChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"DeepSpace.TenantQuota entity successfully created.",
						true,
						tenantQuota. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "DeepSpace.TenantQuota entity creation failed.", false, tenantQuota.id.ToString(), "", JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TenantQuota", tenantQuota.id, tenantQuota.id.ToString()));

			return CreatedAtRoute("TenantQuota", new { id = tenantQuota.id }, Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota));
		}



        /// <summary>
        /// 
        /// This rolls a TenantQuota entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantQuota/Rollback/{id}")]
		[Route("api/TenantQuota/Rollback")]
		public async Task<IActionResult> RollbackToTenantQuotaVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.TenantQuota> query = (from x in _context.TenantQuotas
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this TenantQuota concurrently
			//
			lock (tenantQuotaPutSyncRoot)
			{
				
				Database.TenantQuota tenantQuota = query.FirstOrDefault();
				
				if (tenantQuota == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.TenantQuota rollback", id.ToString(), new Exception("No DeepSpace.TenantQuota entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the TenantQuota current state so we can log it.
				//
				Database.TenantQuota cloneOfExisting = (Database.TenantQuota)_context.Entry(tenantQuota).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.TenantQuotaChangeHistories = null;

				if (versionNumber >= tenantQuota.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for DeepSpace.TenantQuota rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for DeepSpace.TenantQuota rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				TenantQuotaChangeHistory tenantQuotaChangeHistory = (from x in _context.TenantQuotaChangeHistories
				                                               where
				                                               x.tenantQuotaId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (tenantQuotaChangeHistory != null)
				{
				    Database.TenantQuota oldTenantQuota = JsonSerializer.Deserialize<Database.TenantQuota>(tenantQuotaChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    tenantQuota.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    tenantQuota.maxStorageBytes = oldTenantQuota.maxStorageBytes;
				    tenantQuota.currentUsageBytes = oldTenantQuota.currentUsageBytes;
				    tenantQuota.maxObjectCount = oldTenantQuota.maxObjectCount;
				    tenantQuota.currentObjectCount = oldTenantQuota.currentObjectCount;
				    tenantQuota.alertThresholdPercent = oldTenantQuota.alertThresholdPercent;
				    tenantQuota.isEnforced = oldTenantQuota.isEnforced;
				    tenantQuota.lastRecalculatedUtc = oldTenantQuota.lastRecalculatedUtc;
				    tenantQuota.objectGuid = oldTenantQuota.objectGuid;
				    tenantQuota.active = oldTenantQuota.active;
				    tenantQuota.deleted = oldTenantQuota.deleted;

				    string serializedTenantQuota = JsonSerializer.Serialize(tenantQuota);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        TenantQuotaChangeHistory newTenantQuotaChangeHistory = new TenantQuotaChangeHistory();
				        newTenantQuotaChangeHistory.tenantQuotaId = tenantQuota.id;
				        newTenantQuotaChangeHistory.versionNumber = tenantQuota.versionNumber;
				        newTenantQuotaChangeHistory.timeStamp = DateTime.UtcNow;
				        newTenantQuotaChangeHistory.userId = securityUser.id;
				        newTenantQuotaChangeHistory.tenantGuid = userTenantGuid;
				        newTenantQuotaChangeHistory.data = JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota));
				        _context.TenantQuotaChangeHistories.Add(newTenantQuotaChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.TenantQuota rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota)),
						null);


				    return Ok(Database.TenantQuota.CreateAnonymous(tenantQuota));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for DeepSpace.TenantQuota rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for DeepSpace.TenantQuota rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a TenantQuota.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the TenantQuota</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantQuota/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetTenantQuotaChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.TenantQuota tenantQuota = await _context.TenantQuotas.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tenantQuota == null)
			{
				return NotFound();
			}

			try
			{
				tenantQuota.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.TenantQuota> versionInfo = await tenantQuota.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a TenantQuota.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the TenantQuota</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantQuota/{id}/AuditHistory")]
		public async Task<IActionResult> GetTenantQuotaAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.TenantQuota tenantQuota = await _context.TenantQuotas.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tenantQuota == null)
			{
				return NotFound();
			}

			try
			{
				tenantQuota.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.TenantQuota>> versions = await tenantQuota.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a TenantQuota.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the TenantQuota</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The TenantQuota object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantQuota/{id}/Version/{version}")]
		public async Task<IActionResult> GetTenantQuotaVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.TenantQuota tenantQuota = await _context.TenantQuotas.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tenantQuota == null)
			{
				return NotFound();
			}

			try
			{
				tenantQuota.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.TenantQuota> versionInfo = await tenantQuota.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a TenantQuota at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the TenantQuota</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The TenantQuota object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantQuota/{id}/StateAtTime")]
		public async Task<IActionResult> GetTenantQuotaStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.TenantQuota tenantQuota = await _context.TenantQuotas.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tenantQuota == null)
			{
				return NotFound();
			}

			try
			{
				tenantQuota.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.TenantQuota> versionInfo = await tenantQuota.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a TenantQuota record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TenantQuota/{id}")]
		[Route("api/TenantQuota")]
		public async Task<IActionResult> DeleteTenantQuota(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
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

			IQueryable<Database.TenantQuota> query = (from x in _context.TenantQuotas
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.TenantQuota tenantQuota = await query.FirstOrDefaultAsync(cancellationToken);

			if (tenantQuota == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.TenantQuota DELETE", id.ToString(), new Exception("No DeepSpace.TenantQuota entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TenantQuota cloneOfExisting = (Database.TenantQuota)_context.Entry(tenantQuota).GetDatabaseValues().ToObject();


			lock (tenantQuotaDeleteSyncRoot)
			{
			    try
			    {
			        tenantQuota.deleted = true;
			        tenantQuota.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        TenantQuotaChangeHistory tenantQuotaChangeHistory = new TenantQuotaChangeHistory();
			        tenantQuotaChangeHistory.tenantQuotaId = tenantQuota.id;
			        tenantQuotaChangeHistory.versionNumber = tenantQuota.versionNumber;
			        tenantQuotaChangeHistory.timeStamp = DateTime.UtcNow;
			        tenantQuotaChangeHistory.userId = securityUser.id;
			        tenantQuotaChangeHistory.tenantGuid = userTenantGuid;
			        tenantQuotaChangeHistory.data = JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota));
			        _context.TenantQuotaChangeHistories.Add(tenantQuotaChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"DeepSpace.TenantQuota entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"DeepSpace.TenantQuota entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.TenantQuota.CreateAnonymousWithFirstLevelSubObjects(tenantQuota)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of TenantQuota records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TenantQuotas/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? maxStorageBytes = null,
			int? currentUsageBytes = null,
			int? maxObjectCount = null,
			int? currentObjectCount = null,
			int? alertThresholdPercent = null,
			bool? isEnforced = null,
			DateTime? lastRecalculatedUtc = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);


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
			if (lastRecalculatedUtc.HasValue == true && lastRecalculatedUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastRecalculatedUtc = lastRecalculatedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.TenantQuota> query = (from tq in _context.TenantQuotas select tq);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (maxStorageBytes.HasValue == true)
			{
				query = query.Where(tq => tq.maxStorageBytes == maxStorageBytes.Value);
			}
			if (currentUsageBytes.HasValue == true)
			{
				query = query.Where(tq => tq.currentUsageBytes == currentUsageBytes.Value);
			}
			if (maxObjectCount.HasValue == true)
			{
				query = query.Where(tq => tq.maxObjectCount == maxObjectCount.Value);
			}
			if (currentObjectCount.HasValue == true)
			{
				query = query.Where(tq => tq.currentObjectCount == currentObjectCount.Value);
			}
			if (alertThresholdPercent.HasValue == true)
			{
				query = query.Where(tq => tq.alertThresholdPercent == alertThresholdPercent.Value);
			}
			if (isEnforced.HasValue == true)
			{
				query = query.Where(tq => tq.isEnforced == isEnforced.Value);
			}
			if (lastRecalculatedUtc.HasValue == true)
			{
				query = query.Where(tq => tq.lastRecalculatedUtc == lastRecalculatedUtc.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(tq => tq.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tq => tq.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tq => tq.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tq => tq.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tq => tq.deleted == false);
				}
			}
			else
			{
				query = query.Where(tq => tq.active == true);
				query = query.Where(tq => tq.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TenantQuota.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TenantQuota/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
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
