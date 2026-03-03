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
    /// This auto generated class provides the basic CRUD operations for the BrickOwlTransaction entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickOwlTransaction entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickOwlTransactionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<BrickOwlTransactionsController> _logger;

		public BrickOwlTransactionsController(BMCContext context, ILogger<BrickOwlTransactionsController> logger) : base("BMC", "BrickOwlTransaction")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickOwlTransactions filtered by the parameters provided.
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
		[Route("api/BrickOwlTransactions")]
		public async Task<IActionResult> GetBrickOwlTransactions(
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

			IQueryable<Database.BrickOwlTransaction> query = (from bot in _context.BrickOwlTransactions select bot);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (transactionDate.HasValue == true)
			{
				query = query.Where(bot => bot.transactionDate == transactionDate.Value);
			}
			if (string.IsNullOrEmpty(direction) == false)
			{
				query = query.Where(bot => bot.direction == direction);
			}
			if (string.IsNullOrEmpty(methodName) == false)
			{
				query = query.Where(bot => bot.methodName == methodName);
			}
			if (string.IsNullOrEmpty(requestSummary) == false)
			{
				query = query.Where(bot => bot.requestSummary == requestSummary);
			}
			if (success.HasValue == true)
			{
				query = query.Where(bot => bot.success == success.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(bot => bot.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(triggeredBy) == false)
			{
				query = query.Where(bot => bot.triggeredBy == triggeredBy);
			}
			if (recordCount.HasValue == true)
			{
				query = query.Where(bot => bot.recordCount == recordCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bot => bot.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bot => bot.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bot => bot.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bot => bot.deleted == false);
				}
			}
			else
			{
				query = query.Where(bot => bot.active == true);
				query = query.Where(bot => bot.deleted == false);
			}

			query = query.OrderBy(bot => bot.direction).ThenBy(bot => bot.methodName).ThenBy(bot => bot.triggeredBy);


			//
			// Add the any string contains parameter to span all the string fields on the Brick Owl Transaction, or on an any of the string fields on its immediate relations
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
			
			List<Database.BrickOwlTransaction> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickOwlTransaction brickOwlTransaction in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickOwlTransaction, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickOwlTransaction Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickOwlTransaction Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickOwlTransactions filtered by the parameters provided.  Its query is similar to the GetBrickOwlTransactions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickOwlTransactions/RowCount")]
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

			IQueryable<Database.BrickOwlTransaction> query = (from bot in _context.BrickOwlTransactions select bot);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (transactionDate.HasValue == true)
			{
				query = query.Where(bot => bot.transactionDate == transactionDate.Value);
			}
			if (direction != null)
			{
				query = query.Where(bot => bot.direction == direction);
			}
			if (methodName != null)
			{
				query = query.Where(bot => bot.methodName == methodName);
			}
			if (requestSummary != null)
			{
				query = query.Where(bot => bot.requestSummary == requestSummary);
			}
			if (success.HasValue == true)
			{
				query = query.Where(bot => bot.success == success.Value);
			}
			if (errorMessage != null)
			{
				query = query.Where(bot => bot.errorMessage == errorMessage);
			}
			if (triggeredBy != null)
			{
				query = query.Where(bot => bot.triggeredBy == triggeredBy);
			}
			if (recordCount.HasValue == true)
			{
				query = query.Where(bot => bot.recordCount == recordCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bot => bot.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bot => bot.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bot => bot.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bot => bot.deleted == false);
				}
			}
			else
			{
				query = query.Where(bot => bot.active == true);
				query = query.Where(bot => bot.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Owl Transaction, or on an any of the string fields on its immediate relations
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
        /// This gets a single BrickOwlTransaction by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickOwlTransaction/{id}")]
		public async Task<IActionResult> GetBrickOwlTransaction(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BrickOwlTransaction> query = (from bot in _context.BrickOwlTransactions where
							(bot.id == id) &&
							(userIsAdmin == true || bot.deleted == false) &&
							(userIsWriter == true || bot.active == true)
					select bot);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.BrickOwlTransaction materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickOwlTransaction Entity was read with Admin privilege." : "BMC.BrickOwlTransaction Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickOwlTransaction", materialized.id, materialized.direction));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickOwlTransaction entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickOwlTransaction.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickOwlTransaction.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing BrickOwlTransaction record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickOwlTransaction/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickOwlTransaction(int id, [FromBody]Database.BrickOwlTransaction.BrickOwlTransactionDTO brickOwlTransactionDTO, CancellationToken cancellationToken = default)
		{
			if (brickOwlTransactionDTO == null)
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



			if (id != brickOwlTransactionDTO.id)
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


			IQueryable<Database.BrickOwlTransaction> query = (from x in _context.BrickOwlTransactions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickOwlTransaction existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickOwlTransaction PUT", id.ToString(), new Exception("No BMC.BrickOwlTransaction entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickOwlTransactionDTO.objectGuid == Guid.Empty)
            {
                brickOwlTransactionDTO.objectGuid = existing.objectGuid;
            }
            else if (brickOwlTransactionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickOwlTransaction record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickOwlTransaction cloneOfExisting = (Database.BrickOwlTransaction)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickOwlTransaction object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickOwlTransaction brickOwlTransaction = (Database.BrickOwlTransaction)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickOwlTransaction.ApplyDTO(brickOwlTransactionDTO);
			//
			// The tenant guid for any BrickOwlTransaction being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BrickOwlTransaction because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				brickOwlTransaction.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickOwlTransaction.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickOwlTransaction record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (brickOwlTransaction.transactionDate.HasValue == true && brickOwlTransaction.transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				brickOwlTransaction.transactionDate = brickOwlTransaction.transactionDate.Value.ToUniversalTime();
			}

			if (brickOwlTransaction.direction != null && brickOwlTransaction.direction.Length > 50)
			{
				brickOwlTransaction.direction = brickOwlTransaction.direction.Substring(0, 50);
			}

			if (brickOwlTransaction.methodName != null && brickOwlTransaction.methodName.Length > 100)
			{
				brickOwlTransaction.methodName = brickOwlTransaction.methodName.Substring(0, 100);
			}

			if (brickOwlTransaction.triggeredBy != null && brickOwlTransaction.triggeredBy.Length > 100)
			{
				brickOwlTransaction.triggeredBy = brickOwlTransaction.triggeredBy.Substring(0, 100);
			}

			EntityEntry<Database.BrickOwlTransaction> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickOwlTransaction);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickOwlTransaction entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickOwlTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickOwlTransaction.CreateAnonymousWithFirstLevelSubObjects(brickOwlTransaction)),
					null);


				return Ok(Database.BrickOwlTransaction.CreateAnonymous(brickOwlTransaction));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickOwlTransaction entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickOwlTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickOwlTransaction.CreateAnonymousWithFirstLevelSubObjects(brickOwlTransaction)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new BrickOwlTransaction record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickOwlTransaction", Name = "BrickOwlTransaction")]
		public async Task<IActionResult> PostBrickOwlTransaction([FromBody]Database.BrickOwlTransaction.BrickOwlTransactionDTO brickOwlTransactionDTO, CancellationToken cancellationToken = default)
		{
			if (brickOwlTransactionDTO == null)
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
			// Create a new BrickOwlTransaction object using the data from the DTO
			//
			Database.BrickOwlTransaction brickOwlTransaction = Database.BrickOwlTransaction.FromDTO(brickOwlTransactionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				brickOwlTransaction.tenantGuid = userTenantGuid;

				if (brickOwlTransaction.transactionDate.HasValue == true && brickOwlTransaction.transactionDate.Value.Kind != DateTimeKind.Utc)
				{
					brickOwlTransaction.transactionDate = brickOwlTransaction.transactionDate.Value.ToUniversalTime();
				}

				if (brickOwlTransaction.direction != null && brickOwlTransaction.direction.Length > 50)
				{
					brickOwlTransaction.direction = brickOwlTransaction.direction.Substring(0, 50);
				}

				if (brickOwlTransaction.methodName != null && brickOwlTransaction.methodName.Length > 100)
				{
					brickOwlTransaction.methodName = brickOwlTransaction.methodName.Substring(0, 100);
				}

				if (brickOwlTransaction.triggeredBy != null && brickOwlTransaction.triggeredBy.Length > 100)
				{
					brickOwlTransaction.triggeredBy = brickOwlTransaction.triggeredBy.Substring(0, 100);
				}

				brickOwlTransaction.objectGuid = Guid.NewGuid();
				_context.BrickOwlTransactions.Add(brickOwlTransaction);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickOwlTransaction entity successfully created.",
					true,
					brickOwlTransaction.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickOwlTransaction.CreateAnonymousWithFirstLevelSubObjects(brickOwlTransaction)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickOwlTransaction entity creation failed.", false, brickOwlTransaction.id.ToString(), "", JsonSerializer.Serialize(brickOwlTransaction), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickOwlTransaction", brickOwlTransaction.id, brickOwlTransaction.direction));

			return CreatedAtRoute("BrickOwlTransaction", new { id = brickOwlTransaction.id }, Database.BrickOwlTransaction.CreateAnonymousWithFirstLevelSubObjects(brickOwlTransaction));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a BrickOwlTransaction record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickOwlTransaction/{id}")]
		[Route("api/BrickOwlTransaction")]
		public async Task<IActionResult> DeleteBrickOwlTransaction(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BrickOwlTransaction> query = (from x in _context.BrickOwlTransactions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BrickOwlTransaction brickOwlTransaction = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickOwlTransaction == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickOwlTransaction DELETE", id.ToString(), new Exception("No BMC.BrickOwlTransaction entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickOwlTransaction cloneOfExisting = (Database.BrickOwlTransaction)_context.Entry(brickOwlTransaction).GetDatabaseValues().ToObject();


			try
			{
				brickOwlTransaction.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickOwlTransaction entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickOwlTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickOwlTransaction.CreateAnonymousWithFirstLevelSubObjects(brickOwlTransaction)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickOwlTransaction entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickOwlTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickOwlTransaction.CreateAnonymousWithFirstLevelSubObjects(brickOwlTransaction)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of BrickOwlTransaction records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickOwlTransactions/ListData")]
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

			IQueryable<Database.BrickOwlTransaction> query = (from bot in _context.BrickOwlTransactions select bot);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (transactionDate.HasValue == true)
			{
				query = query.Where(bot => bot.transactionDate == transactionDate.Value);
			}
			if (string.IsNullOrEmpty(direction) == false)
			{
				query = query.Where(bot => bot.direction == direction);
			}
			if (string.IsNullOrEmpty(methodName) == false)
			{
				query = query.Where(bot => bot.methodName == methodName);
			}
			if (string.IsNullOrEmpty(requestSummary) == false)
			{
				query = query.Where(bot => bot.requestSummary == requestSummary);
			}
			if (success.HasValue == true)
			{
				query = query.Where(bot => bot.success == success.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(bot => bot.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(triggeredBy) == false)
			{
				query = query.Where(bot => bot.triggeredBy == triggeredBy);
			}
			if (recordCount.HasValue == true)
			{
				query = query.Where(bot => bot.recordCount == recordCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bot => bot.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bot => bot.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bot => bot.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bot => bot.deleted == false);
				}
			}
			else
			{
				query = query.Where(bot => bot.active == true);
				query = query.Where(bot => bot.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Owl Transaction, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.BrickOwlTransaction.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickOwlTransaction/CreateAuditEvent")]
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
