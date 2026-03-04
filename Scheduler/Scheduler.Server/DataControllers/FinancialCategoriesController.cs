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
    /// This auto generated class provides the basic CRUD operations for the FinancialCategory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the FinancialCategory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class FinancialCategoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object financialCategoryPutSyncRoot = new object();
		static object financialCategoryDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<FinancialCategoriesController> _logger;

		public FinancialCategoriesController(SchedulerContext context, ILogger<FinancialCategoriesController> logger) : base("Scheduler", "FinancialCategory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of FinancialCategories filtered by the parameters provided.
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
		[Route("api/FinancialCategories")]
		public async Task<IActionResult> GetFinancialCategories(
			string name = null,
			string description = null,
			string code = null,
			bool? isRevenue = null,
			int? parentFinancialCategoryId = null,
			bool? isTaxApplicable = null,
			decimal? defaultAmount = null,
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

			IQueryable<Database.FinancialCategory> query = (from fc in _context.FinancialCategories select fc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(fc => fc.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(fc => fc.description == description);
			}
			if (string.IsNullOrEmpty(code) == false)
			{
				query = query.Where(fc => fc.code == code);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(fc => fc.isRevenue == isRevenue.Value);
			}
			if (parentFinancialCategoryId.HasValue == true)
			{
				query = query.Where(fc => fc.parentFinancialCategoryId == parentFinancialCategoryId.Value);
			}
			if (isTaxApplicable.HasValue == true)
			{
				query = query.Where(fc => fc.isTaxApplicable == isTaxApplicable.Value);
			}
			if (defaultAmount.HasValue == true)
			{
				query = query.Where(fc => fc.defaultAmount == defaultAmount.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(fc => fc.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(fc => fc.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(fc => fc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(fc => fc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(fc => fc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(fc => fc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(fc => fc.deleted == false);
				}
			}
			else
			{
				query = query.Where(fc => fc.active == true);
				query = query.Where(fc => fc.deleted == false);
			}

			query = query.OrderBy(fc => fc.sequence).ThenBy(fc => fc.name).ThenBy(fc => fc.description).ThenBy(fc => fc.code);


			//
			// Add the any string contains parameter to span all the string fields on the Financial Category, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.code.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || (includeRelations == true && x.parentFinancialCategory.name.Contains(anyStringContains))
			       || (includeRelations == true && x.parentFinancialCategory.description.Contains(anyStringContains))
			       || (includeRelations == true && x.parentFinancialCategory.code.Contains(anyStringContains))
			       || (includeRelations == true && x.parentFinancialCategory.color.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.parentFinancialCategory);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.FinancialCategory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.FinancialCategory financialCategory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(financialCategory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.FinancialCategory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.FinancialCategory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of FinancialCategories filtered by the parameters provided.  Its query is similar to the GetFinancialCategories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialCategories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string code = null,
			bool? isRevenue = null,
			int? parentFinancialCategoryId = null,
			bool? isTaxApplicable = null,
			decimal? defaultAmount = null,
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


			IQueryable<Database.FinancialCategory> query = (from fc in _context.FinancialCategories select fc);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(fc => fc.name == name);
			}
			if (description != null)
			{
				query = query.Where(fc => fc.description == description);
			}
			if (code != null)
			{
				query = query.Where(fc => fc.code == code);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(fc => fc.isRevenue == isRevenue.Value);
			}
			if (parentFinancialCategoryId.HasValue == true)
			{
				query = query.Where(fc => fc.parentFinancialCategoryId == parentFinancialCategoryId.Value);
			}
			if (isTaxApplicable.HasValue == true)
			{
				query = query.Where(fc => fc.isTaxApplicable == isTaxApplicable.Value);
			}
			if (defaultAmount.HasValue == true)
			{
				query = query.Where(fc => fc.defaultAmount == defaultAmount.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(fc => fc.sequence == sequence.Value);
			}
			if (color != null)
			{
				query = query.Where(fc => fc.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(fc => fc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(fc => fc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(fc => fc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(fc => fc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(fc => fc.deleted == false);
				}
			}
			else
			{
				query = query.Where(fc => fc.active == true);
				query = query.Where(fc => fc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Financial Category, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.code.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.parentFinancialCategory.name.Contains(anyStringContains)
			       || x.parentFinancialCategory.description.Contains(anyStringContains)
			       || x.parentFinancialCategory.code.Contains(anyStringContains)
			       || x.parentFinancialCategory.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single FinancialCategory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialCategory/{id}")]
		public async Task<IActionResult> GetFinancialCategory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.FinancialCategory> query = (from fc in _context.FinancialCategories where
							(fc.id == id) &&
							(userIsAdmin == true || fc.deleted == false) &&
							(userIsWriter == true || fc.active == true)
					select fc);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.parentFinancialCategory);
					query = query.AsSplitQuery();
				}

				Database.FinancialCategory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.FinancialCategory Entity was read with Admin privilege." : "Scheduler.FinancialCategory Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "FinancialCategory", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.FinancialCategory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.FinancialCategory.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.FinancialCategory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing FinancialCategory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/FinancialCategory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutFinancialCategory(int id, [FromBody]Database.FinancialCategory.FinancialCategoryDTO financialCategoryDTO, CancellationToken cancellationToken = default)
		{
			if (financialCategoryDTO == null)
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



			if (id != financialCategoryDTO.id)
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


			IQueryable<Database.FinancialCategory> query = (from x in _context.FinancialCategories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.FinancialCategory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.FinancialCategory PUT", id.ToString(), new Exception("No Scheduler.FinancialCategory entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (financialCategoryDTO.objectGuid == Guid.Empty)
            {
                financialCategoryDTO.objectGuid = existing.objectGuid;
            }
            else if (financialCategoryDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a FinancialCategory record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.FinancialCategory cloneOfExisting = (Database.FinancialCategory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new FinancialCategory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.FinancialCategory financialCategory = (Database.FinancialCategory)_context.Entry(existing).GetDatabaseValues().ToObject();
			financialCategory.ApplyDTO(financialCategoryDTO);
			//
			// The tenant guid for any FinancialCategory being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the FinancialCategory because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				financialCategory.tenantGuid = existing.tenantGuid;
			}

			lock (financialCategoryPutSyncRoot)
			{
				//
				// Validate the version number for the financialCategory being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != financialCategory.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "FinancialCategory save attempt was made but save request was with version " + financialCategory.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The FinancialCategory you are trying to update has already changed.  Please try your save again after reloading the FinancialCategory.");
				}
				else
				{
					// Same record.  Increase version.
					financialCategory.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (financialCategory.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.FinancialCategory record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (financialCategory.name != null && financialCategory.name.Length > 100)
				{
					financialCategory.name = financialCategory.name.Substring(0, 100);
				}

				if (financialCategory.description != null && financialCategory.description.Length > 500)
				{
					financialCategory.description = financialCategory.description.Substring(0, 500);
				}

				if (financialCategory.code != null && financialCategory.code.Length > 50)
				{
					financialCategory.code = financialCategory.code.Substring(0, 50);
				}

				if (financialCategory.color != null && financialCategory.color.Length > 10)
				{
					financialCategory.color = financialCategory.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.FinancialCategory> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(financialCategory);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        FinancialCategoryChangeHistory financialCategoryChangeHistory = new FinancialCategoryChangeHistory();
				        financialCategoryChangeHistory.financialCategoryId = financialCategory.id;
				        financialCategoryChangeHistory.versionNumber = financialCategory.versionNumber;
				        financialCategoryChangeHistory.timeStamp = DateTime.UtcNow;
				        financialCategoryChangeHistory.userId = securityUser.id;
				        financialCategoryChangeHistory.tenantGuid = userTenantGuid;
				        financialCategoryChangeHistory.data = JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory));
				        _context.FinancialCategoryChangeHistories.Add(financialCategoryChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.FinancialCategory entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory)),
						null);

				return Ok(Database.FinancialCategory.CreateAnonymous(financialCategory));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.FinancialCategory entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new FinancialCategory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialCategory", Name = "FinancialCategory")]
		public async Task<IActionResult> PostFinancialCategory([FromBody]Database.FinancialCategory.FinancialCategoryDTO financialCategoryDTO, CancellationToken cancellationToken = default)
		{
			if (financialCategoryDTO == null)
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
			// Create a new FinancialCategory object using the data from the DTO
			//
			Database.FinancialCategory financialCategory = Database.FinancialCategory.FromDTO(financialCategoryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				financialCategory.tenantGuid = userTenantGuid;

				if (financialCategory.name != null && financialCategory.name.Length > 100)
				{
					financialCategory.name = financialCategory.name.Substring(0, 100);
				}

				if (financialCategory.description != null && financialCategory.description.Length > 500)
				{
					financialCategory.description = financialCategory.description.Substring(0, 500);
				}

				if (financialCategory.code != null && financialCategory.code.Length > 50)
				{
					financialCategory.code = financialCategory.code.Substring(0, 50);
				}

				if (financialCategory.color != null && financialCategory.color.Length > 10)
				{
					financialCategory.color = financialCategory.color.Substring(0, 10);
				}

				financialCategory.objectGuid = Guid.NewGuid();
				financialCategory.versionNumber = 1;

				_context.FinancialCategories.Add(financialCategory);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the financialCategory object so that no further changes will be written to the database
				    //
				    _context.Entry(financialCategory).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					financialCategory.FinancialCategoryChangeHistories = null;
					financialCategory.FinancialTransactions = null;
					financialCategory.InverseparentFinancialCategory = null;
					financialCategory.parentFinancialCategory = null;


				    FinancialCategoryChangeHistory financialCategoryChangeHistory = new FinancialCategoryChangeHistory();
				    financialCategoryChangeHistory.financialCategoryId = financialCategory.id;
				    financialCategoryChangeHistory.versionNumber = financialCategory.versionNumber;
				    financialCategoryChangeHistory.timeStamp = DateTime.UtcNow;
				    financialCategoryChangeHistory.userId = securityUser.id;
				    financialCategoryChangeHistory.tenantGuid = userTenantGuid;
				    financialCategoryChangeHistory.data = JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory));
				    _context.FinancialCategoryChangeHistories.Add(financialCategoryChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.FinancialCategory entity successfully created.",
						true,
						financialCategory. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.FinancialCategory entity creation failed.", false, financialCategory.id.ToString(), "", JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "FinancialCategory", financialCategory.id, financialCategory.name));

			return CreatedAtRoute("FinancialCategory", new { id = financialCategory.id }, Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory));
		}



        /// <summary>
        /// 
        /// This rolls a FinancialCategory entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialCategory/Rollback/{id}")]
		[Route("api/FinancialCategory/Rollback")]
		public async Task<IActionResult> RollbackToFinancialCategoryVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.FinancialCategory> query = (from x in _context.FinancialCategories
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this FinancialCategory concurrently
			//
			lock (financialCategoryPutSyncRoot)
			{
				
				Database.FinancialCategory financialCategory = query.FirstOrDefault();
				
				if (financialCategory == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.FinancialCategory rollback", id.ToString(), new Exception("No Scheduler.FinancialCategory entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the FinancialCategory current state so we can log it.
				//
				Database.FinancialCategory cloneOfExisting = (Database.FinancialCategory)_context.Entry(financialCategory).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.FinancialCategoryChangeHistories = null;
				cloneOfExisting.FinancialTransactions = null;
				cloneOfExisting.InverseparentFinancialCategory = null;
				cloneOfExisting.parentFinancialCategory = null;

				if (versionNumber >= financialCategory.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.FinancialCategory rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.FinancialCategory rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				FinancialCategoryChangeHistory financialCategoryChangeHistory = (from x in _context.FinancialCategoryChangeHistories
				                                               where
				                                               x.financialCategoryId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (financialCategoryChangeHistory != null)
				{
				    Database.FinancialCategory oldFinancialCategory = JsonSerializer.Deserialize<Database.FinancialCategory>(financialCategoryChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    financialCategory.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    financialCategory.name = oldFinancialCategory.name;
				    financialCategory.description = oldFinancialCategory.description;
				    financialCategory.code = oldFinancialCategory.code;
				    financialCategory.isRevenue = oldFinancialCategory.isRevenue;
				    financialCategory.parentFinancialCategoryId = oldFinancialCategory.parentFinancialCategoryId;
				    financialCategory.isTaxApplicable = oldFinancialCategory.isTaxApplicable;
				    financialCategory.defaultAmount = oldFinancialCategory.defaultAmount;
				    financialCategory.sequence = oldFinancialCategory.sequence;
				    financialCategory.color = oldFinancialCategory.color;
				    financialCategory.objectGuid = oldFinancialCategory.objectGuid;
				    financialCategory.active = oldFinancialCategory.active;
				    financialCategory.deleted = oldFinancialCategory.deleted;

				    string serializedFinancialCategory = JsonSerializer.Serialize(financialCategory);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        FinancialCategoryChangeHistory newFinancialCategoryChangeHistory = new FinancialCategoryChangeHistory();
				        newFinancialCategoryChangeHistory.financialCategoryId = financialCategory.id;
				        newFinancialCategoryChangeHistory.versionNumber = financialCategory.versionNumber;
				        newFinancialCategoryChangeHistory.timeStamp = DateTime.UtcNow;
				        newFinancialCategoryChangeHistory.userId = securityUser.id;
				        newFinancialCategoryChangeHistory.tenantGuid = userTenantGuid;
				        newFinancialCategoryChangeHistory.data = JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory));
				        _context.FinancialCategoryChangeHistories.Add(newFinancialCategoryChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.FinancialCategory rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory)),
						null);


				    return Ok(Database.FinancialCategory.CreateAnonymous(financialCategory));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.FinancialCategory rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.FinancialCategory rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a FinancialCategory.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FinancialCategory</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialCategory/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetFinancialCategoryChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.FinancialCategory financialCategory = await _context.FinancialCategories.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (financialCategory == null)
			{
				return NotFound();
			}

			try
			{
				financialCategory.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.FinancialCategory> versionInfo = await financialCategory.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a FinancialCategory.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FinancialCategory</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialCategory/{id}/AuditHistory")]
		public async Task<IActionResult> GetFinancialCategoryAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.FinancialCategory financialCategory = await _context.FinancialCategories.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (financialCategory == null)
			{
				return NotFound();
			}

			try
			{
				financialCategory.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.FinancialCategory>> versions = await financialCategory.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a FinancialCategory.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FinancialCategory</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The FinancialCategory object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialCategory/{id}/Version/{version}")]
		public async Task<IActionResult> GetFinancialCategoryVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.FinancialCategory financialCategory = await _context.FinancialCategories.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (financialCategory == null)
			{
				return NotFound();
			}

			try
			{
				financialCategory.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.FinancialCategory> versionInfo = await financialCategory.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a FinancialCategory at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the FinancialCategory</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The FinancialCategory object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialCategory/{id}/StateAtTime")]
		public async Task<IActionResult> GetFinancialCategoryStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.FinancialCategory financialCategory = await _context.FinancialCategories.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (financialCategory == null)
			{
				return NotFound();
			}

			try
			{
				financialCategory.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.FinancialCategory> versionInfo = await financialCategory.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a FinancialCategory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/FinancialCategory/{id}")]
		[Route("api/FinancialCategory")]
		public async Task<IActionResult> DeleteFinancialCategory(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.FinancialCategory> query = (from x in _context.FinancialCategories
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.FinancialCategory financialCategory = await query.FirstOrDefaultAsync(cancellationToken);

			if (financialCategory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.FinancialCategory DELETE", id.ToString(), new Exception("No Scheduler.FinancialCategory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.FinancialCategory cloneOfExisting = (Database.FinancialCategory)_context.Entry(financialCategory).GetDatabaseValues().ToObject();


			lock (financialCategoryDeleteSyncRoot)
			{
			    try
			    {
			        financialCategory.deleted = true;
			        financialCategory.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        FinancialCategoryChangeHistory financialCategoryChangeHistory = new FinancialCategoryChangeHistory();
			        financialCategoryChangeHistory.financialCategoryId = financialCategory.id;
			        financialCategoryChangeHistory.versionNumber = financialCategory.versionNumber;
			        financialCategoryChangeHistory.timeStamp = DateTime.UtcNow;
			        financialCategoryChangeHistory.userId = securityUser.id;
			        financialCategoryChangeHistory.tenantGuid = userTenantGuid;
			        financialCategoryChangeHistory.data = JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory));
			        _context.FinancialCategoryChangeHistories.Add(financialCategoryChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.FinancialCategory entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.FinancialCategory entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.FinancialCategory.CreateAnonymousWithFirstLevelSubObjects(financialCategory)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of FinancialCategory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/FinancialCategories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string code = null,
			bool? isRevenue = null,
			int? parentFinancialCategoryId = null,
			bool? isTaxApplicable = null,
			decimal? defaultAmount = null,
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

			IQueryable<Database.FinancialCategory> query = (from fc in _context.FinancialCategories select fc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(fc => fc.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(fc => fc.description == description);
			}
			if (string.IsNullOrEmpty(code) == false)
			{
				query = query.Where(fc => fc.code == code);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(fc => fc.isRevenue == isRevenue.Value);
			}
			if (parentFinancialCategoryId.HasValue == true)
			{
				query = query.Where(fc => fc.parentFinancialCategoryId == parentFinancialCategoryId.Value);
			}
			if (isTaxApplicable.HasValue == true)
			{
				query = query.Where(fc => fc.isTaxApplicable == isTaxApplicable.Value);
			}
			if (defaultAmount.HasValue == true)
			{
				query = query.Where(fc => fc.defaultAmount == defaultAmount.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(fc => fc.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(fc => fc.color == color);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(fc => fc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(fc => fc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(fc => fc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(fc => fc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(fc => fc.deleted == false);
				}
			}
			else
			{
				query = query.Where(fc => fc.active == true);
				query = query.Where(fc => fc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Financial Category, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.code.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.parentFinancialCategory.name.Contains(anyStringContains)
			       || x.parentFinancialCategory.description.Contains(anyStringContains)
			       || x.parentFinancialCategory.code.Contains(anyStringContains)
			       || x.parentFinancialCategory.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.code);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.FinancialCategory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/FinancialCategory/CreateAuditEvent")]
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
