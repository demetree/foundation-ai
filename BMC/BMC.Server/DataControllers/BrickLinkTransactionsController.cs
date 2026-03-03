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
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the BrickLinkTransaction entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickLinkTransaction entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickLinkTransactionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<BrickLinkTransactionsController> _logger;

		public BrickLinkTransactionsController(BMCContext context, ILogger<BrickLinkTransactionsController> logger) : base("BMC", "BrickLinkTransaction")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickLinkTransactions filtered by the parameters provided.
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
		[Route("api/BrickLinkTransactions")]
		public async Task<IActionResult> GetBrickLinkTransactions(
			DateTime? transactionDate = null,
			string direction = null,
			string methodName = null,
			string requestSummary = null,
			bool? success = null,
			string errorMessage = null,
			string triggeredBy = null,
			int? recordCount = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
			if (transactionDate.HasValue == true && transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				transactionDate = transactionDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickLinkTransaction> query = (from blt in _context.BrickLinkTransactions select blt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (transactionDate.HasValue == true)
			{
				query = query.Where(blt => blt.transactionDate == transactionDate.Value);
			}
			if (string.IsNullOrEmpty(direction) == false)
			{
				query = query.Where(blt => blt.direction == direction);
			}
			if (string.IsNullOrEmpty(methodName) == false)
			{
				query = query.Where(blt => blt.methodName == methodName);
			}
			if (string.IsNullOrEmpty(requestSummary) == false)
			{
				query = query.Where(blt => blt.requestSummary == requestSummary);
			}
			if (success.HasValue == true)
			{
				query = query.Where(blt => blt.success == success.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(blt => blt.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(triggeredBy) == false)
			{
				query = query.Where(blt => blt.triggeredBy == triggeredBy);
			}
			if (recordCount.HasValue == true)
			{
				query = query.Where(blt => blt.recordCount == recordCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(blt => blt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(blt => blt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(blt => blt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(blt => blt.deleted == false);
				}
			}
			else
			{
				query = query.Where(blt => blt.active == true);
				query = query.Where(blt => blt.deleted == false);
			}

			query = query.OrderBy(blt => blt.direction).ThenBy(blt => blt.methodName).ThenBy(blt => blt.triggeredBy);


			//
			// Add the any string contains parameter to span all the string fields on the Brick Link Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.direction.Contains(anyStringContains)
			       || x.methodName.Contains(anyStringContains)
			       || x.requestSummary.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.triggeredBy.Contains(anyStringContains)
			   );
			}

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
			
			List<Database.BrickLinkTransaction> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickLinkTransaction brickLinkTransaction in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickLinkTransaction, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickLinkTransaction Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickLinkTransaction Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickLinkTransactions filtered by the parameters provided.  Its query is similar to the GetBrickLinkTransactions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickLinkTransactions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			DateTime? transactionDate = null,
			string direction = null,
			string methodName = null,
			string requestSummary = null,
			bool? success = null,
			string errorMessage = null,
			string triggeredBy = null,
			int? recordCount = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
			if (transactionDate.HasValue == true && transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				transactionDate = transactionDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickLinkTransaction> query = (from blt in _context.BrickLinkTransactions select blt);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (transactionDate.HasValue == true)
			{
				query = query.Where(blt => blt.transactionDate == transactionDate.Value);
			}
			if (direction != null)
			{
				query = query.Where(blt => blt.direction == direction);
			}
			if (methodName != null)
			{
				query = query.Where(blt => blt.methodName == methodName);
			}
			if (requestSummary != null)
			{
				query = query.Where(blt => blt.requestSummary == requestSummary);
			}
			if (success.HasValue == true)
			{
				query = query.Where(blt => blt.success == success.Value);
			}
			if (errorMessage != null)
			{
				query = query.Where(blt => blt.errorMessage == errorMessage);
			}
			if (triggeredBy != null)
			{
				query = query.Where(blt => blt.triggeredBy == triggeredBy);
			}
			if (recordCount.HasValue == true)
			{
				query = query.Where(blt => blt.recordCount == recordCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(blt => blt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(blt => blt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(blt => blt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(blt => blt.deleted == false);
				}
			}
			else
			{
				query = query.Where(blt => blt.active == true);
				query = query.Where(blt => blt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Link Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.direction.Contains(anyStringContains)
			       || x.methodName.Contains(anyStringContains)
			       || x.requestSummary.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.triggeredBy.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickLinkTransaction by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickLinkTransaction/{id}")]
		public async Task<IActionResult> GetBrickLinkTransaction(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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
				IQueryable<Database.BrickLinkTransaction> query = (from blt in _context.BrickLinkTransactions where
							(blt.id == id) &&
							(userIsAdmin == true || blt.deleted == false) &&
							(userIsWriter == true || blt.active == true)
					select blt);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.BrickLinkTransaction materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickLinkTransaction Entity was read with Admin privilege." : "BMC.BrickLinkTransaction Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickLinkTransaction", materialized.id, materialized.direction));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickLinkTransaction entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickLinkTransaction.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickLinkTransaction.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing BrickLinkTransaction record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickLinkTransaction/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickLinkTransaction(int id, [FromBody]Database.BrickLinkTransaction.BrickLinkTransactionDTO brickLinkTransactionDTO, CancellationToken cancellationToken = default)
		{
			if (brickLinkTransactionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != brickLinkTransactionDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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


			IQueryable<Database.BrickLinkTransaction> query = (from x in _context.BrickLinkTransactions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickLinkTransaction existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickLinkTransaction PUT", id.ToString(), new Exception("No BMC.BrickLinkTransaction entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickLinkTransactionDTO.objectGuid == Guid.Empty)
            {
                brickLinkTransactionDTO.objectGuid = existing.objectGuid;
            }
            else if (brickLinkTransactionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickLinkTransaction record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickLinkTransaction cloneOfExisting = (Database.BrickLinkTransaction)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickLinkTransaction object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickLinkTransaction brickLinkTransaction = (Database.BrickLinkTransaction)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickLinkTransaction.ApplyDTO(brickLinkTransactionDTO);
			//
			// The tenant guid for any BrickLinkTransaction being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BrickLinkTransaction because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				brickLinkTransaction.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickLinkTransaction.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickLinkTransaction record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (brickLinkTransaction.transactionDate.HasValue == true && brickLinkTransaction.transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				brickLinkTransaction.transactionDate = brickLinkTransaction.transactionDate.Value.ToUniversalTime();
			}

			if (brickLinkTransaction.direction != null && brickLinkTransaction.direction.Length > 50)
			{
				brickLinkTransaction.direction = brickLinkTransaction.direction.Substring(0, 50);
			}

			if (brickLinkTransaction.methodName != null && brickLinkTransaction.methodName.Length > 100)
			{
				brickLinkTransaction.methodName = brickLinkTransaction.methodName.Substring(0, 100);
			}

			if (brickLinkTransaction.triggeredBy != null && brickLinkTransaction.triggeredBy.Length > 100)
			{
				brickLinkTransaction.triggeredBy = brickLinkTransaction.triggeredBy.Substring(0, 100);
			}

			EntityEntry<Database.BrickLinkTransaction> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickLinkTransaction);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickLinkTransaction entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickLinkTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickLinkTransaction.CreateAnonymousWithFirstLevelSubObjects(brickLinkTransaction)),
					null);


				return Ok(Database.BrickLinkTransaction.CreateAnonymous(brickLinkTransaction));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickLinkTransaction entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickLinkTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickLinkTransaction.CreateAnonymousWithFirstLevelSubObjects(brickLinkTransaction)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new BrickLinkTransaction record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickLinkTransaction", Name = "BrickLinkTransaction")]
		public async Task<IActionResult> PostBrickLinkTransaction([FromBody]Database.BrickLinkTransaction.BrickLinkTransactionDTO brickLinkTransactionDTO, CancellationToken cancellationToken = default)
		{
			if (brickLinkTransactionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
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
			// Create a new BrickLinkTransaction object using the data from the DTO
			//
			Database.BrickLinkTransaction brickLinkTransaction = Database.BrickLinkTransaction.FromDTO(brickLinkTransactionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				brickLinkTransaction.tenantGuid = userTenantGuid;

				if (brickLinkTransaction.transactionDate.HasValue == true && brickLinkTransaction.transactionDate.Value.Kind != DateTimeKind.Utc)
				{
					brickLinkTransaction.transactionDate = brickLinkTransaction.transactionDate.Value.ToUniversalTime();
				}

				if (brickLinkTransaction.direction != null && brickLinkTransaction.direction.Length > 50)
				{
					brickLinkTransaction.direction = brickLinkTransaction.direction.Substring(0, 50);
				}

				if (brickLinkTransaction.methodName != null && brickLinkTransaction.methodName.Length > 100)
				{
					brickLinkTransaction.methodName = brickLinkTransaction.methodName.Substring(0, 100);
				}

				if (brickLinkTransaction.triggeredBy != null && brickLinkTransaction.triggeredBy.Length > 100)
				{
					brickLinkTransaction.triggeredBy = brickLinkTransaction.triggeredBy.Substring(0, 100);
				}

				brickLinkTransaction.objectGuid = Guid.NewGuid();
				_context.BrickLinkTransactions.Add(brickLinkTransaction);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickLinkTransaction entity successfully created.",
					true,
					brickLinkTransaction.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickLinkTransaction.CreateAnonymousWithFirstLevelSubObjects(brickLinkTransaction)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickLinkTransaction entity creation failed.", false, brickLinkTransaction.id.ToString(), "", JsonSerializer.Serialize(brickLinkTransaction), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickLinkTransaction", brickLinkTransaction.id, brickLinkTransaction.direction));

			return CreatedAtRoute("BrickLinkTransaction", new { id = brickLinkTransaction.id }, Database.BrickLinkTransaction.CreateAnonymousWithFirstLevelSubObjects(brickLinkTransaction));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a BrickLinkTransaction record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickLinkTransaction/{id}")]
		[Route("api/BrickLinkTransaction")]
		public async Task<IActionResult> DeleteBrickLinkTransaction(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
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

			IQueryable<Database.BrickLinkTransaction> query = (from x in _context.BrickLinkTransactions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickLinkTransaction brickLinkTransaction = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickLinkTransaction == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickLinkTransaction DELETE", id.ToString(), new Exception("No BMC.BrickLinkTransaction entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickLinkTransaction cloneOfExisting = (Database.BrickLinkTransaction)_context.Entry(brickLinkTransaction).GetDatabaseValues().ToObject();


			try
			{
				brickLinkTransaction.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickLinkTransaction entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickLinkTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickLinkTransaction.CreateAnonymousWithFirstLevelSubObjects(brickLinkTransaction)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickLinkTransaction entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickLinkTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickLinkTransaction.CreateAnonymousWithFirstLevelSubObjects(brickLinkTransaction)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of BrickLinkTransaction records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickLinkTransactions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			DateTime? transactionDate = null,
			string direction = null,
			string methodName = null,
			string requestSummary = null,
			bool? success = null,
			string errorMessage = null,
			string triggeredBy = null,
			int? recordCount = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);


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
			if (transactionDate.HasValue == true && transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				transactionDate = transactionDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickLinkTransaction> query = (from blt in _context.BrickLinkTransactions select blt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (transactionDate.HasValue == true)
			{
				query = query.Where(blt => blt.transactionDate == transactionDate.Value);
			}
			if (string.IsNullOrEmpty(direction) == false)
			{
				query = query.Where(blt => blt.direction == direction);
			}
			if (string.IsNullOrEmpty(methodName) == false)
			{
				query = query.Where(blt => blt.methodName == methodName);
			}
			if (string.IsNullOrEmpty(requestSummary) == false)
			{
				query = query.Where(blt => blt.requestSummary == requestSummary);
			}
			if (success.HasValue == true)
			{
				query = query.Where(blt => blt.success == success.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(blt => blt.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(triggeredBy) == false)
			{
				query = query.Where(blt => blt.triggeredBy == triggeredBy);
			}
			if (recordCount.HasValue == true)
			{
				query = query.Where(blt => blt.recordCount == recordCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(blt => blt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(blt => blt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(blt => blt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(blt => blt.deleted == false);
				}
			}
			else
			{
				query = query.Where(blt => blt.active == true);
				query = query.Where(blt => blt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Link Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.direction.Contains(anyStringContains)
			       || x.methodName.Contains(anyStringContains)
			       || x.requestSummary.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.triggeredBy.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.direction).ThenBy(x => x.methodName).ThenBy(x => x.triggeredBy);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickLinkTransaction.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickLinkTransaction/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
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
