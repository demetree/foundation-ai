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
    /// This auto generated class provides the basic CRUD operations for the LifecycleRule entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the LifecycleRule entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class LifecycleRulesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object lifecycleRulePutSyncRoot = new object();
		static object lifecycleRuleDeleteSyncRoot = new object();

		private DeepSpaceContext _context;

		private ILogger<LifecycleRulesController> _logger;

		public LifecycleRulesController(DeepSpaceContext context, ILogger<LifecycleRulesController> logger) : base("DeepSpace", "LifecycleRule")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of LifecycleRules filtered by the parameters provided.
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
		[Route("api/LifecycleRules")]
		public async Task<IActionResult> GetLifecycleRules(
			string name = null,
			string description = null,
			string keyPrefixPattern = null,
			int? minAgeDays = null,
			int? sourceStorageTierId = null,
			int? targetStorageTierId = null,
			int? minAccessCount = null,
			bool? isEnabled = null,
			int? priority = null,
			DateTime? lastEvaluatedUtc = null,
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
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (lastEvaluatedUtc.HasValue == true && lastEvaluatedUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastEvaluatedUtc = lastEvaluatedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.LifecycleRule> query = (from lr in _context.LifecycleRules select lr);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(lr => lr.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(lr => lr.description == description);
			}
			if (string.IsNullOrEmpty(keyPrefixPattern) == false)
			{
				query = query.Where(lr => lr.keyPrefixPattern == keyPrefixPattern);
			}
			if (minAgeDays.HasValue == true)
			{
				query = query.Where(lr => lr.minAgeDays == minAgeDays.Value);
			}
			if (sourceStorageTierId.HasValue == true)
			{
				query = query.Where(lr => lr.sourceStorageTierId == sourceStorageTierId.Value);
			}
			if (targetStorageTierId.HasValue == true)
			{
				query = query.Where(lr => lr.targetStorageTierId == targetStorageTierId.Value);
			}
			if (minAccessCount.HasValue == true)
			{
				query = query.Where(lr => lr.minAccessCount == minAccessCount.Value);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(lr => lr.isEnabled == isEnabled.Value);
			}
			if (priority.HasValue == true)
			{
				query = query.Where(lr => lr.priority == priority.Value);
			}
			if (lastEvaluatedUtc.HasValue == true)
			{
				query = query.Where(lr => lr.lastEvaluatedUtc == lastEvaluatedUtc.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(lr => lr.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lr => lr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lr => lr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lr => lr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lr => lr.deleted == false);
				}
			}
			else
			{
				query = query.Where(lr => lr.active == true);
				query = query.Where(lr => lr.deleted == false);
			}

			query = query.OrderBy(lr => lr.name).ThenBy(lr => lr.description).ThenBy(lr => lr.keyPrefixPattern);


			//
			// Add the any string contains parameter to span all the string fields on the Lifecycle Rule, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.keyPrefixPattern.Contains(anyStringContains)
			       || (includeRelations == true && x.sourceStorageTier.name.Contains(anyStringContains))
			       || (includeRelations == true && x.sourceStorageTier.description.Contains(anyStringContains))
			       || (includeRelations == true && x.targetStorageTier.name.Contains(anyStringContains))
			       || (includeRelations == true && x.targetStorageTier.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.sourceStorageTier);
				query = query.Include(x => x.targetStorageTier);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.LifecycleRule> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.LifecycleRule lifecycleRule in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(lifecycleRule, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "DeepSpace.LifecycleRule Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "DeepSpace.LifecycleRule Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of LifecycleRules filtered by the parameters provided.  Its query is similar to the GetLifecycleRules method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRules/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string keyPrefixPattern = null,
			int? minAgeDays = null,
			int? sourceStorageTierId = null,
			int? targetStorageTierId = null,
			int? minAccessCount = null,
			bool? isEnabled = null,
			int? priority = null,
			DateTime? lastEvaluatedUtc = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (lastEvaluatedUtc.HasValue == true && lastEvaluatedUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastEvaluatedUtc = lastEvaluatedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.LifecycleRule> query = (from lr in _context.LifecycleRules select lr);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(lr => lr.name == name);
			}
			if (description != null)
			{
				query = query.Where(lr => lr.description == description);
			}
			if (keyPrefixPattern != null)
			{
				query = query.Where(lr => lr.keyPrefixPattern == keyPrefixPattern);
			}
			if (minAgeDays.HasValue == true)
			{
				query = query.Where(lr => lr.minAgeDays == minAgeDays.Value);
			}
			if (sourceStorageTierId.HasValue == true)
			{
				query = query.Where(lr => lr.sourceStorageTierId == sourceStorageTierId.Value);
			}
			if (targetStorageTierId.HasValue == true)
			{
				query = query.Where(lr => lr.targetStorageTierId == targetStorageTierId.Value);
			}
			if (minAccessCount.HasValue == true)
			{
				query = query.Where(lr => lr.minAccessCount == minAccessCount.Value);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(lr => lr.isEnabled == isEnabled.Value);
			}
			if (priority.HasValue == true)
			{
				query = query.Where(lr => lr.priority == priority.Value);
			}
			if (lastEvaluatedUtc.HasValue == true)
			{
				query = query.Where(lr => lr.lastEvaluatedUtc == lastEvaluatedUtc.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(lr => lr.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lr => lr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lr => lr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lr => lr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lr => lr.deleted == false);
				}
			}
			else
			{
				query = query.Where(lr => lr.active == true);
				query = query.Where(lr => lr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Lifecycle Rule, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.keyPrefixPattern.Contains(anyStringContains)
			       || x.sourceStorageTier.name.Contains(anyStringContains)
			       || x.sourceStorageTier.description.Contains(anyStringContains)
			       || x.targetStorageTier.name.Contains(anyStringContains)
			       || x.targetStorageTier.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single LifecycleRule by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRule/{id}")]
		public async Task<IActionResult> GetLifecycleRule(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.LifecycleRule> query = (from lr in _context.LifecycleRules where
							(lr.id == id) &&
							(userIsAdmin == true || lr.deleted == false) &&
							(userIsWriter == true || lr.active == true)
					select lr);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.sourceStorageTier);
					query = query.Include(x => x.targetStorageTier);
					query = query.AsSplitQuery();
				}

				Database.LifecycleRule materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "DeepSpace.LifecycleRule Entity was read with Admin privilege." : "DeepSpace.LifecycleRule Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LifecycleRule", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a DeepSpace.LifecycleRule entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of DeepSpace.LifecycleRule.   Entity was read with Admin privilege." : "Exception caught during entity read of DeepSpace.LifecycleRule.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing LifecycleRule record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/LifecycleRule/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutLifecycleRule(int id, [FromBody]Database.LifecycleRule.LifecycleRuleDTO lifecycleRuleDTO, CancellationToken cancellationToken = default)
		{
			if (lifecycleRuleDTO == null)
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



			if (id != lifecycleRuleDTO.id)
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


			IQueryable<Database.LifecycleRule> query = (from x in _context.LifecycleRules
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.LifecycleRule existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.LifecycleRule PUT", id.ToString(), new Exception("No DeepSpace.LifecycleRule entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (lifecycleRuleDTO.objectGuid == Guid.Empty)
            {
                lifecycleRuleDTO.objectGuid = existing.objectGuid;
            }
            else if (lifecycleRuleDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a LifecycleRule record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.LifecycleRule cloneOfExisting = (Database.LifecycleRule)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new LifecycleRule object using the data from the existing record, updated with what is in the DTO.
			//
			Database.LifecycleRule lifecycleRule = (Database.LifecycleRule)_context.Entry(existing).GetDatabaseValues().ToObject();
			lifecycleRule.ApplyDTO(lifecycleRuleDTO);
			//
			// The tenant guid for any LifecycleRule being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the LifecycleRule because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				lifecycleRule.tenantGuid = existing.tenantGuid;
			}

			lock (lifecycleRulePutSyncRoot)
			{
				//
				// Validate the version number for the lifecycleRule being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != lifecycleRule.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "LifecycleRule save attempt was made but save request was with version " + lifecycleRule.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The LifecycleRule you are trying to update has already changed.  Please try your save again after reloading the LifecycleRule.");
				}
				else
				{
					// Same record.  Increase version.
					lifecycleRule.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (lifecycleRule.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted DeepSpace.LifecycleRule record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (lifecycleRule.name != null && lifecycleRule.name.Length > 100)
				{
					lifecycleRule.name = lifecycleRule.name.Substring(0, 100);
				}

				if (lifecycleRule.description != null && lifecycleRule.description.Length > 500)
				{
					lifecycleRule.description = lifecycleRule.description.Substring(0, 500);
				}

				if (lifecycleRule.keyPrefixPattern != null && lifecycleRule.keyPrefixPattern.Length > 500)
				{
					lifecycleRule.keyPrefixPattern = lifecycleRule.keyPrefixPattern.Substring(0, 500);
				}

				if (lifecycleRule.lastEvaluatedUtc.HasValue == true && lifecycleRule.lastEvaluatedUtc.Value.Kind != DateTimeKind.Utc)
				{
					lifecycleRule.lastEvaluatedUtc = lifecycleRule.lastEvaluatedUtc.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.LifecycleRule> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(lifecycleRule);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        LifecycleRuleChangeHistory lifecycleRuleChangeHistory = new LifecycleRuleChangeHistory();
				        lifecycleRuleChangeHistory.lifecycleRuleId = lifecycleRule.id;
				        lifecycleRuleChangeHistory.versionNumber = lifecycleRule.versionNumber;
				        lifecycleRuleChangeHistory.timeStamp = DateTime.UtcNow;
				        lifecycleRuleChangeHistory.userId = securityUser.id;
				        lifecycleRuleChangeHistory.tenantGuid = userTenantGuid;
				        lifecycleRuleChangeHistory.data = JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule));
				        _context.LifecycleRuleChangeHistories.Add(lifecycleRuleChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.LifecycleRule entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule)),
						null);

				return Ok(Database.LifecycleRule.CreateAnonymous(lifecycleRule));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.LifecycleRule entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new LifecycleRule record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRule", Name = "LifecycleRule")]
		public async Task<IActionResult> PostLifecycleRule([FromBody]Database.LifecycleRule.LifecycleRuleDTO lifecycleRuleDTO, CancellationToken cancellationToken = default)
		{
			if (lifecycleRuleDTO == null)
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
			// Create a new LifecycleRule object using the data from the DTO
			//
			Database.LifecycleRule lifecycleRule = Database.LifecycleRule.FromDTO(lifecycleRuleDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				lifecycleRule.tenantGuid = userTenantGuid;

				if (lifecycleRule.name != null && lifecycleRule.name.Length > 100)
				{
					lifecycleRule.name = lifecycleRule.name.Substring(0, 100);
				}

				if (lifecycleRule.description != null && lifecycleRule.description.Length > 500)
				{
					lifecycleRule.description = lifecycleRule.description.Substring(0, 500);
				}

				if (lifecycleRule.keyPrefixPattern != null && lifecycleRule.keyPrefixPattern.Length > 500)
				{
					lifecycleRule.keyPrefixPattern = lifecycleRule.keyPrefixPattern.Substring(0, 500);
				}

				if (lifecycleRule.lastEvaluatedUtc.HasValue == true && lifecycleRule.lastEvaluatedUtc.Value.Kind != DateTimeKind.Utc)
				{
					lifecycleRule.lastEvaluatedUtc = lifecycleRule.lastEvaluatedUtc.Value.ToUniversalTime();
				}

				lifecycleRule.objectGuid = Guid.NewGuid();
				lifecycleRule.versionNumber = 1;

				_context.LifecycleRules.Add(lifecycleRule);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the lifecycleRule object so that no further changes will be written to the database
				    //
				    _context.Entry(lifecycleRule).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					lifecycleRule.LifecycleRuleChangeHistories = null;
					lifecycleRule.MigrationJobs = null;
					lifecycleRule.sourceStorageTier = null;
					lifecycleRule.targetStorageTier = null;


				    LifecycleRuleChangeHistory lifecycleRuleChangeHistory = new LifecycleRuleChangeHistory();
				    lifecycleRuleChangeHistory.lifecycleRuleId = lifecycleRule.id;
				    lifecycleRuleChangeHistory.versionNumber = lifecycleRule.versionNumber;
				    lifecycleRuleChangeHistory.timeStamp = DateTime.UtcNow;
				    lifecycleRuleChangeHistory.userId = securityUser.id;
				    lifecycleRuleChangeHistory.tenantGuid = userTenantGuid;
				    lifecycleRuleChangeHistory.data = JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule));
				    _context.LifecycleRuleChangeHistories.Add(lifecycleRuleChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"DeepSpace.LifecycleRule entity successfully created.",
						true,
						lifecycleRule. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "DeepSpace.LifecycleRule entity creation failed.", false, lifecycleRule.id.ToString(), "", JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LifecycleRule", lifecycleRule.id, lifecycleRule.name));

			return CreatedAtRoute("LifecycleRule", new { id = lifecycleRule.id }, Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule));
		}



        /// <summary>
        /// 
        /// This rolls a LifecycleRule entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRule/Rollback/{id}")]
		[Route("api/LifecycleRule/Rollback")]
		public async Task<IActionResult> RollbackToLifecycleRuleVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.LifecycleRule> query = (from x in _context.LifecycleRules
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this LifecycleRule concurrently
			//
			lock (lifecycleRulePutSyncRoot)
			{
				
				Database.LifecycleRule lifecycleRule = query.FirstOrDefault();
				
				if (lifecycleRule == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.LifecycleRule rollback", id.ToString(), new Exception("No DeepSpace.LifecycleRule entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the LifecycleRule current state so we can log it.
				//
				Database.LifecycleRule cloneOfExisting = (Database.LifecycleRule)_context.Entry(lifecycleRule).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.LifecycleRuleChangeHistories = null;
				cloneOfExisting.MigrationJobs = null;
				cloneOfExisting.sourceStorageTier = null;
				cloneOfExisting.targetStorageTier = null;

				if (versionNumber >= lifecycleRule.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for DeepSpace.LifecycleRule rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for DeepSpace.LifecycleRule rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				LifecycleRuleChangeHistory lifecycleRuleChangeHistory = (from x in _context.LifecycleRuleChangeHistories
				                                               where
				                                               x.lifecycleRuleId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (lifecycleRuleChangeHistory != null)
				{
				    Database.LifecycleRule oldLifecycleRule = JsonSerializer.Deserialize<Database.LifecycleRule>(lifecycleRuleChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    lifecycleRule.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    lifecycleRule.name = oldLifecycleRule.name;
				    lifecycleRule.description = oldLifecycleRule.description;
				    lifecycleRule.keyPrefixPattern = oldLifecycleRule.keyPrefixPattern;
				    lifecycleRule.minAgeDays = oldLifecycleRule.minAgeDays;
				    lifecycleRule.sourceStorageTierId = oldLifecycleRule.sourceStorageTierId;
				    lifecycleRule.targetStorageTierId = oldLifecycleRule.targetStorageTierId;
				    lifecycleRule.minAccessCount = oldLifecycleRule.minAccessCount;
				    lifecycleRule.isEnabled = oldLifecycleRule.isEnabled;
				    lifecycleRule.priority = oldLifecycleRule.priority;
				    lifecycleRule.lastEvaluatedUtc = oldLifecycleRule.lastEvaluatedUtc;
				    lifecycleRule.objectGuid = oldLifecycleRule.objectGuid;
				    lifecycleRule.active = oldLifecycleRule.active;
				    lifecycleRule.deleted = oldLifecycleRule.deleted;

				    string serializedLifecycleRule = JsonSerializer.Serialize(lifecycleRule);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        LifecycleRuleChangeHistory newLifecycleRuleChangeHistory = new LifecycleRuleChangeHistory();
				        newLifecycleRuleChangeHistory.lifecycleRuleId = lifecycleRule.id;
				        newLifecycleRuleChangeHistory.versionNumber = lifecycleRule.versionNumber;
				        newLifecycleRuleChangeHistory.timeStamp = DateTime.UtcNow;
				        newLifecycleRuleChangeHistory.userId = securityUser.id;
				        newLifecycleRuleChangeHistory.tenantGuid = userTenantGuid;
				        newLifecycleRuleChangeHistory.data = JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule));
				        _context.LifecycleRuleChangeHistories.Add(newLifecycleRuleChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"DeepSpace.LifecycleRule rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule)),
						null);


				    return Ok(Database.LifecycleRule.CreateAnonymous(lifecycleRule));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for DeepSpace.LifecycleRule rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for DeepSpace.LifecycleRule rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a LifecycleRule.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the LifecycleRule</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRule/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetLifecycleRuleChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.LifecycleRule lifecycleRule = await _context.LifecycleRules.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (lifecycleRule == null)
			{
				return NotFound();
			}

			try
			{
				lifecycleRule.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.LifecycleRule> versionInfo = await lifecycleRule.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a LifecycleRule.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the LifecycleRule</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRule/{id}/AuditHistory")]
		public async Task<IActionResult> GetLifecycleRuleAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.LifecycleRule lifecycleRule = await _context.LifecycleRules.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (lifecycleRule == null)
			{
				return NotFound();
			}

			try
			{
				lifecycleRule.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.LifecycleRule>> versions = await lifecycleRule.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a LifecycleRule.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the LifecycleRule</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The LifecycleRule object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRule/{id}/Version/{version}")]
		public async Task<IActionResult> GetLifecycleRuleVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.LifecycleRule lifecycleRule = await _context.LifecycleRules.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (lifecycleRule == null)
			{
				return NotFound();
			}

			try
			{
				lifecycleRule.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.LifecycleRule> versionInfo = await lifecycleRule.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a LifecycleRule at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the LifecycleRule</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The LifecycleRule object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRule/{id}/StateAtTime")]
		public async Task<IActionResult> GetLifecycleRuleStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.LifecycleRule lifecycleRule = await _context.LifecycleRules.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (lifecycleRule == null)
			{
				return NotFound();
			}

			try
			{
				lifecycleRule.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.LifecycleRule> versionInfo = await lifecycleRule.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a LifecycleRule record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LifecycleRule/{id}")]
		[Route("api/LifecycleRule")]
		public async Task<IActionResult> DeleteLifecycleRule(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.LifecycleRule> query = (from x in _context.LifecycleRules
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.LifecycleRule lifecycleRule = await query.FirstOrDefaultAsync(cancellationToken);

			if (lifecycleRule == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.LifecycleRule DELETE", id.ToString(), new Exception("No DeepSpace.LifecycleRule entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.LifecycleRule cloneOfExisting = (Database.LifecycleRule)_context.Entry(lifecycleRule).GetDatabaseValues().ToObject();


			lock (lifecycleRuleDeleteSyncRoot)
			{
			    try
			    {
			        lifecycleRule.deleted = true;
			        lifecycleRule.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        LifecycleRuleChangeHistory lifecycleRuleChangeHistory = new LifecycleRuleChangeHistory();
			        lifecycleRuleChangeHistory.lifecycleRuleId = lifecycleRule.id;
			        lifecycleRuleChangeHistory.versionNumber = lifecycleRule.versionNumber;
			        lifecycleRuleChangeHistory.timeStamp = DateTime.UtcNow;
			        lifecycleRuleChangeHistory.userId = securityUser.id;
			        lifecycleRuleChangeHistory.tenantGuid = userTenantGuid;
			        lifecycleRuleChangeHistory.data = JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule));
			        _context.LifecycleRuleChangeHistories.Add(lifecycleRuleChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"DeepSpace.LifecycleRule entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"DeepSpace.LifecycleRule entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.LifecycleRule.CreateAnonymousWithFirstLevelSubObjects(lifecycleRule)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of LifecycleRule records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/LifecycleRules/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string keyPrefixPattern = null,
			int? minAgeDays = null,
			int? sourceStorageTierId = null,
			int? targetStorageTierId = null,
			int? minAccessCount = null,
			bool? isEnabled = null,
			int? priority = null,
			DateTime? lastEvaluatedUtc = null,
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
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (lastEvaluatedUtc.HasValue == true && lastEvaluatedUtc.Value.Kind != DateTimeKind.Utc)
			{
				lastEvaluatedUtc = lastEvaluatedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.LifecycleRule> query = (from lr in _context.LifecycleRules select lr);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(lr => lr.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(lr => lr.description == description);
			}
			if (string.IsNullOrEmpty(keyPrefixPattern) == false)
			{
				query = query.Where(lr => lr.keyPrefixPattern == keyPrefixPattern);
			}
			if (minAgeDays.HasValue == true)
			{
				query = query.Where(lr => lr.minAgeDays == minAgeDays.Value);
			}
			if (sourceStorageTierId.HasValue == true)
			{
				query = query.Where(lr => lr.sourceStorageTierId == sourceStorageTierId.Value);
			}
			if (targetStorageTierId.HasValue == true)
			{
				query = query.Where(lr => lr.targetStorageTierId == targetStorageTierId.Value);
			}
			if (minAccessCount.HasValue == true)
			{
				query = query.Where(lr => lr.minAccessCount == minAccessCount.Value);
			}
			if (isEnabled.HasValue == true)
			{
				query = query.Where(lr => lr.isEnabled == isEnabled.Value);
			}
			if (priority.HasValue == true)
			{
				query = query.Where(lr => lr.priority == priority.Value);
			}
			if (lastEvaluatedUtc.HasValue == true)
			{
				query = query.Where(lr => lr.lastEvaluatedUtc == lastEvaluatedUtc.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(lr => lr.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lr => lr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lr => lr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lr => lr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lr => lr.deleted == false);
				}
			}
			else
			{
				query = query.Where(lr => lr.active == true);
				query = query.Where(lr => lr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Lifecycle Rule, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.keyPrefixPattern.Contains(anyStringContains)
			       || x.sourceStorageTier.name.Contains(anyStringContains)
			       || x.sourceStorageTier.description.Contains(anyStringContains)
			       || x.targetStorageTier.name.Contains(anyStringContains)
			       || x.targetStorageTier.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.keyPrefixPattern);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.LifecycleRule.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/LifecycleRule/CreateAuditEvent")]
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
