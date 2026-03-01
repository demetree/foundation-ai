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
    /// This auto generated class provides the basic CRUD operations for the RebrickableTransaction entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the RebrickableTransaction entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class RebrickableTransactionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		private BMCContext _context;

		private ILogger<RebrickableTransactionsController> _logger;

		public RebrickableTransactionsController(BMCContext context, ILogger<RebrickableTransactionsController> logger) : base("BMC", "RebrickableTransaction")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of RebrickableTransactions filtered by the parameters provided.
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
		[Route("api/RebrickableTransactions")]
		public async Task<IActionResult> GetRebrickableTransactions(
			DateTime? transactionDate = null,
			string direction = null,
			string httpMethod = null,
			string endpoint = null,
			string requestSummary = null,
			int? responseStatusCode = null,
			string responseBody = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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

			IQueryable<Database.RebrickableTransaction> query = (from rt in _context.RebrickableTransactions select rt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (transactionDate.HasValue == true)
			{
				query = query.Where(rt => rt.transactionDate == transactionDate.Value);
			}
			if (string.IsNullOrEmpty(direction) == false)
			{
				query = query.Where(rt => rt.direction == direction);
			}
			if (string.IsNullOrEmpty(httpMethod) == false)
			{
				query = query.Where(rt => rt.httpMethod == httpMethod);
			}
			if (string.IsNullOrEmpty(endpoint) == false)
			{
				query = query.Where(rt => rt.endpoint == endpoint);
			}
			if (string.IsNullOrEmpty(requestSummary) == false)
			{
				query = query.Where(rt => rt.requestSummary == requestSummary);
			}
			if (responseStatusCode.HasValue == true)
			{
				query = query.Where(rt => rt.responseStatusCode == responseStatusCode.Value);
			}
			if (string.IsNullOrEmpty(responseBody) == false)
			{
				query = query.Where(rt => rt.responseBody == responseBody);
			}
			if (success.HasValue == true)
			{
				query = query.Where(rt => rt.success == success.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(rt => rt.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(triggeredBy) == false)
			{
				query = query.Where(rt => rt.triggeredBy == triggeredBy);
			}
			if (recordCount.HasValue == true)
			{
				query = query.Where(rt => rt.recordCount == recordCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rt => rt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rt => rt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rt => rt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rt => rt.deleted == false);
				}
			}
			else
			{
				query = query.Where(rt => rt.active == true);
				query = query.Where(rt => rt.deleted == false);
			}

			query = query.OrderBy(rt => rt.direction).ThenBy(rt => rt.httpMethod).ThenBy(rt => rt.endpoint);


			//
			// Add the any string contains parameter to span all the string fields on the Rebrickable Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.direction.Contains(anyStringContains)
			       || x.httpMethod.Contains(anyStringContains)
			       || x.endpoint.Contains(anyStringContains)
			       || x.requestSummary.Contains(anyStringContains)
			       || x.responseBody.Contains(anyStringContains)
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
			
			List<Database.RebrickableTransaction> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.RebrickableTransaction rebrickableTransaction in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(rebrickableTransaction, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.RebrickableTransaction Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.RebrickableTransaction Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of RebrickableTransactions filtered by the parameters provided.  Its query is similar to the GetRebrickableTransactions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableTransactions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			DateTime? transactionDate = null,
			string direction = null,
			string httpMethod = null,
			string endpoint = null,
			string requestSummary = null,
			int? responseStatusCode = null,
			string responseBody = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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

			IQueryable<Database.RebrickableTransaction> query = (from rt in _context.RebrickableTransactions select rt);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (transactionDate.HasValue == true)
			{
				query = query.Where(rt => rt.transactionDate == transactionDate.Value);
			}
			if (direction != null)
			{
				query = query.Where(rt => rt.direction == direction);
			}
			if (httpMethod != null)
			{
				query = query.Where(rt => rt.httpMethod == httpMethod);
			}
			if (endpoint != null)
			{
				query = query.Where(rt => rt.endpoint == endpoint);
			}
			if (requestSummary != null)
			{
				query = query.Where(rt => rt.requestSummary == requestSummary);
			}
			if (responseStatusCode.HasValue == true)
			{
				query = query.Where(rt => rt.responseStatusCode == responseStatusCode.Value);
			}
			if (responseBody != null)
			{
				query = query.Where(rt => rt.responseBody == responseBody);
			}
			if (success.HasValue == true)
			{
				query = query.Where(rt => rt.success == success.Value);
			}
			if (errorMessage != null)
			{
				query = query.Where(rt => rt.errorMessage == errorMessage);
			}
			if (triggeredBy != null)
			{
				query = query.Where(rt => rt.triggeredBy == triggeredBy);
			}
			if (recordCount.HasValue == true)
			{
				query = query.Where(rt => rt.recordCount == recordCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rt => rt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rt => rt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rt => rt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rt => rt.deleted == false);
				}
			}
			else
			{
				query = query.Where(rt => rt.active == true);
				query = query.Where(rt => rt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Rebrickable Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.direction.Contains(anyStringContains)
			       || x.httpMethod.Contains(anyStringContains)
			       || x.endpoint.Contains(anyStringContains)
			       || x.requestSummary.Contains(anyStringContains)
			       || x.responseBody.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.triggeredBy.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single RebrickableTransaction by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableTransaction/{id}")]
		public async Task<IActionResult> GetRebrickableTransaction(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
				IQueryable<Database.RebrickableTransaction> query = (from rt in _context.RebrickableTransactions where
							(rt.id == id) &&
							(userIsAdmin == true || rt.deleted == false) &&
							(userIsWriter == true || rt.active == true)
					select rt);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.RebrickableTransaction materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.RebrickableTransaction Entity was read with Admin privilege." : "BMC.RebrickableTransaction Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RebrickableTransaction", materialized.id, materialized.direction));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.RebrickableTransaction entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.RebrickableTransaction.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.RebrickableTransaction.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing RebrickableTransaction record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/RebrickableTransaction/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutRebrickableTransaction(int id, [FromBody]Database.RebrickableTransaction.RebrickableTransactionDTO rebrickableTransactionDTO, CancellationToken cancellationToken = default)
		{
			if (rebrickableTransactionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != rebrickableTransactionDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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


			IQueryable<Database.RebrickableTransaction> query = (from x in _context.RebrickableTransactions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.RebrickableTransaction existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.RebrickableTransaction PUT", id.ToString(), new Exception("No BMC.RebrickableTransaction entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (rebrickableTransactionDTO.objectGuid == Guid.Empty)
            {
                rebrickableTransactionDTO.objectGuid = existing.objectGuid;
            }
            else if (rebrickableTransactionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a RebrickableTransaction record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.RebrickableTransaction cloneOfExisting = (Database.RebrickableTransaction)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new RebrickableTransaction object using the data from the existing record, updated with what is in the DTO.
			//
			Database.RebrickableTransaction rebrickableTransaction = (Database.RebrickableTransaction)_context.Entry(existing).GetDatabaseValues().ToObject();
			rebrickableTransaction.ApplyDTO(rebrickableTransactionDTO);
			//
			// The tenant guid for any RebrickableTransaction being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the RebrickableTransaction because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				rebrickableTransaction.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (rebrickableTransaction.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.RebrickableTransaction record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (rebrickableTransaction.transactionDate.HasValue == true && rebrickableTransaction.transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				rebrickableTransaction.transactionDate = rebrickableTransaction.transactionDate.Value.ToUniversalTime();
			}

			if (rebrickableTransaction.direction != null && rebrickableTransaction.direction.Length > 50)
			{
				rebrickableTransaction.direction = rebrickableTransaction.direction.Substring(0, 50);
			}

			if (rebrickableTransaction.httpMethod != null && rebrickableTransaction.httpMethod.Length > 50)
			{
				rebrickableTransaction.httpMethod = rebrickableTransaction.httpMethod.Substring(0, 50);
			}

			if (rebrickableTransaction.endpoint != null && rebrickableTransaction.endpoint.Length > 500)
			{
				rebrickableTransaction.endpoint = rebrickableTransaction.endpoint.Substring(0, 500);
			}

			if (rebrickableTransaction.triggeredBy != null && rebrickableTransaction.triggeredBy.Length > 100)
			{
				rebrickableTransaction.triggeredBy = rebrickableTransaction.triggeredBy.Substring(0, 100);
			}

			EntityEntry<Database.RebrickableTransaction> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(rebrickableTransaction);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.RebrickableTransaction entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableTransaction.CreateAnonymousWithFirstLevelSubObjects(rebrickableTransaction)),
					null);


				return Ok(Database.RebrickableTransaction.CreateAnonymous(rebrickableTransaction));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.RebrickableTransaction entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableTransaction.CreateAnonymousWithFirstLevelSubObjects(rebrickableTransaction)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new RebrickableTransaction record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableTransaction", Name = "RebrickableTransaction")]
		public async Task<IActionResult> PostRebrickableTransaction([FromBody]Database.RebrickableTransaction.RebrickableTransactionDTO rebrickableTransactionDTO, CancellationToken cancellationToken = default)
		{
			if (rebrickableTransactionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new RebrickableTransaction object using the data from the DTO
			//
			Database.RebrickableTransaction rebrickableTransaction = Database.RebrickableTransaction.FromDTO(rebrickableTransactionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				rebrickableTransaction.tenantGuid = userTenantGuid;

				if (rebrickableTransaction.transactionDate.HasValue == true && rebrickableTransaction.transactionDate.Value.Kind != DateTimeKind.Utc)
				{
					rebrickableTransaction.transactionDate = rebrickableTransaction.transactionDate.Value.ToUniversalTime();
				}

				if (rebrickableTransaction.direction != null && rebrickableTransaction.direction.Length > 50)
				{
					rebrickableTransaction.direction = rebrickableTransaction.direction.Substring(0, 50);
				}

				if (rebrickableTransaction.httpMethod != null && rebrickableTransaction.httpMethod.Length > 50)
				{
					rebrickableTransaction.httpMethod = rebrickableTransaction.httpMethod.Substring(0, 50);
				}

				if (rebrickableTransaction.endpoint != null && rebrickableTransaction.endpoint.Length > 500)
				{
					rebrickableTransaction.endpoint = rebrickableTransaction.endpoint.Substring(0, 500);
				}

				if (rebrickableTransaction.triggeredBy != null && rebrickableTransaction.triggeredBy.Length > 100)
				{
					rebrickableTransaction.triggeredBy = rebrickableTransaction.triggeredBy.Substring(0, 100);
				}

				rebrickableTransaction.objectGuid = Guid.NewGuid();
				_context.RebrickableTransactions.Add(rebrickableTransaction);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.RebrickableTransaction entity successfully created.",
					true,
					rebrickableTransaction.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.RebrickableTransaction.CreateAnonymousWithFirstLevelSubObjects(rebrickableTransaction)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.RebrickableTransaction entity creation failed.", false, rebrickableTransaction.id.ToString(), "", JsonSerializer.Serialize(rebrickableTransaction), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RebrickableTransaction", rebrickableTransaction.id, rebrickableTransaction.direction));

			return CreatedAtRoute("RebrickableTransaction", new { id = rebrickableTransaction.id }, Database.RebrickableTransaction.CreateAnonymousWithFirstLevelSubObjects(rebrickableTransaction));
		}



        /// <summary>
        /// 
        /// This deletes a RebrickableTransaction record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RebrickableTransaction/{id}")]
		[Route("api/RebrickableTransaction")]
		public async Task<IActionResult> DeleteRebrickableTransaction(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.RebrickableTransaction> query = (from x in _context.RebrickableTransactions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.RebrickableTransaction rebrickableTransaction = await query.FirstOrDefaultAsync(cancellationToken);

			if (rebrickableTransaction == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.RebrickableTransaction DELETE", id.ToString(), new Exception("No BMC.RebrickableTransaction entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.RebrickableTransaction cloneOfExisting = (Database.RebrickableTransaction)_context.Entry(rebrickableTransaction).GetDatabaseValues().ToObject();


			try
			{
				rebrickableTransaction.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.RebrickableTransaction entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableTransaction.CreateAnonymousWithFirstLevelSubObjects(rebrickableTransaction)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.RebrickableTransaction entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RebrickableTransaction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RebrickableTransaction.CreateAnonymousWithFirstLevelSubObjects(rebrickableTransaction)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of RebrickableTransaction records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/RebrickableTransactions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			DateTime? transactionDate = null,
			string direction = null,
			string httpMethod = null,
			string endpoint = null,
			string requestSummary = null,
			int? responseStatusCode = null,
			string responseBody = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);


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

			IQueryable<Database.RebrickableTransaction> query = (from rt in _context.RebrickableTransactions select rt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (transactionDate.HasValue == true)
			{
				query = query.Where(rt => rt.transactionDate == transactionDate.Value);
			}
			if (string.IsNullOrEmpty(direction) == false)
			{
				query = query.Where(rt => rt.direction == direction);
			}
			if (string.IsNullOrEmpty(httpMethod) == false)
			{
				query = query.Where(rt => rt.httpMethod == httpMethod);
			}
			if (string.IsNullOrEmpty(endpoint) == false)
			{
				query = query.Where(rt => rt.endpoint == endpoint);
			}
			if (string.IsNullOrEmpty(requestSummary) == false)
			{
				query = query.Where(rt => rt.requestSummary == requestSummary);
			}
			if (responseStatusCode.HasValue == true)
			{
				query = query.Where(rt => rt.responseStatusCode == responseStatusCode.Value);
			}
			if (string.IsNullOrEmpty(responseBody) == false)
			{
				query = query.Where(rt => rt.responseBody == responseBody);
			}
			if (success.HasValue == true)
			{
				query = query.Where(rt => rt.success == success.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(rt => rt.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(triggeredBy) == false)
			{
				query = query.Where(rt => rt.triggeredBy == triggeredBy);
			}
			if (recordCount.HasValue == true)
			{
				query = query.Where(rt => rt.recordCount == recordCount.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rt => rt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rt => rt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rt => rt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rt => rt.deleted == false);
				}
			}
			else
			{
				query = query.Where(rt => rt.active == true);
				query = query.Where(rt => rt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Rebrickable Transaction, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.direction.Contains(anyStringContains)
			       || x.httpMethod.Contains(anyStringContains)
			       || x.endpoint.Contains(anyStringContains)
			       || x.requestSummary.Contains(anyStringContains)
			       || x.responseBody.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.triggeredBy.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.direction).ThenBy(x => x.httpMethod).ThenBy(x => x.endpoint);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.RebrickableTransaction.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/RebrickableTransaction/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Collection Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
