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
    /// This auto generated class provides the basic CRUD operations for the PaymentType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PaymentType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PaymentTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object paymentTypePutSyncRoot = new object();
		static object paymentTypeDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<PaymentTypesController> _logger;

		public PaymentTypesController(SchedulerContext context, ILogger<PaymentTypesController> logger) : base("Scheduler", "PaymentType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PaymentTypes filtered by the parameters provided.
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
		[Route("api/PaymentTypes")]
		public async Task<IActionResult> GetPaymentTypes(
			string name = null,
			string description = null,
			string color = null,
			int? sequence = null,
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

			IQueryable<Database.PaymentType> query = (from pt in _context.PaymentTypes select pt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pt => pt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(pt => pt.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(pt => pt.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pt => pt.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pt => pt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pt => pt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pt => pt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pt => pt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pt => pt.deleted == false);
				}
			}
			else
			{
				query = query.Where(pt => pt.active == true);
				query = query.Where(pt => pt.deleted == false);
			}

			query = query.OrderBy(pt => pt.sequence).ThenBy(pt => pt.name).ThenBy(pt => pt.description).ThenBy(pt => pt.color);


			//
			// Add the any string contains parameter to span all the string fields on the Payment Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
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
			
			List<Database.PaymentType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PaymentType paymentType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(paymentType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.PaymentType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.PaymentType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PaymentTypes filtered by the parameters provided.  Its query is similar to the GetPaymentTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string color = null,
			int? sequence = null,
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


			IQueryable<Database.PaymentType> query = (from pt in _context.PaymentTypes select pt);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(pt => pt.name == name);
			}
			if (description != null)
			{
				query = query.Where(pt => pt.description == description);
			}
			if (color != null)
			{
				query = query.Where(pt => pt.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pt => pt.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pt => pt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pt => pt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pt => pt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pt => pt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pt => pt.deleted == false);
				}
			}
			else
			{
				query = query.Where(pt => pt.active == true);
				query = query.Where(pt => pt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Payment Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PaymentType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentType/{id}")]
		public async Task<IActionResult> GetPaymentType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.PaymentType> query = (from pt in _context.PaymentTypes where
							(pt.id == id) &&
							(userIsAdmin == true || pt.deleted == false) &&
							(userIsWriter == true || pt.active == true)
					select pt);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.PaymentType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.PaymentType Entity was read with Admin privilege." : "Scheduler.PaymentType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PaymentType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.PaymentType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.PaymentType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.PaymentType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PaymentType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PaymentType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPaymentType(int id, [FromBody]Database.PaymentType.PaymentTypeDTO paymentTypeDTO, CancellationToken cancellationToken = default)
		{
			if (paymentTypeDTO == null)
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



			if (id != paymentTypeDTO.id)
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


			IQueryable<Database.PaymentType> query = (from x in _context.PaymentTypes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PaymentType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentType PUT", id.ToString(), new Exception("No Scheduler.PaymentType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (paymentTypeDTO.objectGuid == Guid.Empty)
            {
                paymentTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (paymentTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PaymentType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PaymentType cloneOfExisting = (Database.PaymentType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PaymentType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PaymentType paymentType = (Database.PaymentType)_context.Entry(existing).GetDatabaseValues().ToObject();
			paymentType.ApplyDTO(paymentTypeDTO);
			//
			// The tenant guid for any PaymentType being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the PaymentType because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				paymentType.tenantGuid = existing.tenantGuid;
			}

			lock (paymentTypePutSyncRoot)
			{
				//
				// Validate the version number for the paymentType being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != paymentType.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "PaymentType save attempt was made but save request was with version " + paymentType.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The PaymentType you are trying to update has already changed.  Please try your save again after reloading the PaymentType.");
				}
				else
				{
					// Same record.  Increase version.
					paymentType.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (paymentType.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.PaymentType record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (paymentType.name != null && paymentType.name.Length > 100)
				{
					paymentType.name = paymentType.name.Substring(0, 100);
				}

				if (paymentType.description != null && paymentType.description.Length > 500)
				{
					paymentType.description = paymentType.description.Substring(0, 500);
				}

				if (paymentType.color != null && paymentType.color.Length > 10)
				{
					paymentType.color = paymentType.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.PaymentType> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(paymentType);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PaymentTypeChangeHistory paymentTypeChangeHistory = new PaymentTypeChangeHistory();
				        paymentTypeChangeHistory.paymentTypeId = paymentType.id;
				        paymentTypeChangeHistory.versionNumber = paymentType.versionNumber;
				        paymentTypeChangeHistory.timeStamp = DateTime.UtcNow;
				        paymentTypeChangeHistory.userId = securityUser.id;
				        paymentTypeChangeHistory.tenantGuid = userTenantGuid;
				        paymentTypeChangeHistory.data = JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType));
				        _context.PaymentTypeChangeHistories.Add(paymentTypeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.PaymentType entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType)),
						null);

				return Ok(Database.PaymentType.CreateAnonymous(paymentType));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.PaymentType entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new PaymentType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentType", Name = "PaymentType")]
		public async Task<IActionResult> PostPaymentType([FromBody]Database.PaymentType.PaymentTypeDTO paymentTypeDTO, CancellationToken cancellationToken = default)
		{
			if (paymentTypeDTO == null)
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
			// Create a new PaymentType object using the data from the DTO
			//
			Database.PaymentType paymentType = Database.PaymentType.FromDTO(paymentTypeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				paymentType.tenantGuid = userTenantGuid;

				if (paymentType.name != null && paymentType.name.Length > 100)
				{
					paymentType.name = paymentType.name.Substring(0, 100);
				}

				if (paymentType.description != null && paymentType.description.Length > 500)
				{
					paymentType.description = paymentType.description.Substring(0, 500);
				}

				if (paymentType.color != null && paymentType.color.Length > 10)
				{
					paymentType.color = paymentType.color.Substring(0, 10);
				}

				paymentType.objectGuid = Guid.NewGuid();
				paymentType.versionNumber = 1;

				_context.PaymentTypes.Add(paymentType);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the paymentType object so that no further changes will be written to the database
				    //
				    _context.Entry(paymentType).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					paymentType.FinancialTransactions = null;
					paymentType.Gifts = null;
					paymentType.PaymentTypeChangeHistories = null;


				    PaymentTypeChangeHistory paymentTypeChangeHistory = new PaymentTypeChangeHistory();
				    paymentTypeChangeHistory.paymentTypeId = paymentType.id;
				    paymentTypeChangeHistory.versionNumber = paymentType.versionNumber;
				    paymentTypeChangeHistory.timeStamp = DateTime.UtcNow;
				    paymentTypeChangeHistory.userId = securityUser.id;
				    paymentTypeChangeHistory.tenantGuid = userTenantGuid;
				    paymentTypeChangeHistory.data = JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType));
				    _context.PaymentTypeChangeHistories.Add(paymentTypeChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.PaymentType entity successfully created.",
						true,
						paymentType. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.PaymentType entity creation failed.", false, paymentType.id.ToString(), "", JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PaymentType", paymentType.id, paymentType.name));

			return CreatedAtRoute("PaymentType", new { id = paymentType.id }, Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType));
		}



        /// <summary>
        /// 
        /// This rolls a PaymentType entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentType/Rollback/{id}")]
		[Route("api/PaymentType/Rollback")]
		public async Task<IActionResult> RollbackToPaymentTypeVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.PaymentType> query = (from x in _context.PaymentTypes
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this PaymentType concurrently
			//
			lock (paymentTypePutSyncRoot)
			{
				
				Database.PaymentType paymentType = query.FirstOrDefault();
				
				if (paymentType == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentType rollback", id.ToString(), new Exception("No Scheduler.PaymentType entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the PaymentType current state so we can log it.
				//
				Database.PaymentType cloneOfExisting = (Database.PaymentType)_context.Entry(paymentType).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.FinancialTransactions = null;
				cloneOfExisting.Gifts = null;
				cloneOfExisting.PaymentTypeChangeHistories = null;

				if (versionNumber >= paymentType.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.PaymentType rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.PaymentType rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				PaymentTypeChangeHistory paymentTypeChangeHistory = (from x in _context.PaymentTypeChangeHistories
				                                               where
				                                               x.paymentTypeId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (paymentTypeChangeHistory != null)
				{
				    Database.PaymentType oldPaymentType = JsonSerializer.Deserialize<Database.PaymentType>(paymentTypeChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    paymentType.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    paymentType.name = oldPaymentType.name;
				    paymentType.description = oldPaymentType.description;
				    paymentType.color = oldPaymentType.color;
				    paymentType.sequence = oldPaymentType.sequence;
				    paymentType.objectGuid = oldPaymentType.objectGuid;
				    paymentType.active = oldPaymentType.active;
				    paymentType.deleted = oldPaymentType.deleted;

				    string serializedPaymentType = JsonSerializer.Serialize(paymentType);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PaymentTypeChangeHistory newPaymentTypeChangeHistory = new PaymentTypeChangeHistory();
				        newPaymentTypeChangeHistory.paymentTypeId = paymentType.id;
				        newPaymentTypeChangeHistory.versionNumber = paymentType.versionNumber;
				        newPaymentTypeChangeHistory.timeStamp = DateTime.UtcNow;
				        newPaymentTypeChangeHistory.userId = securityUser.id;
				        newPaymentTypeChangeHistory.tenantGuid = userTenantGuid;
				        newPaymentTypeChangeHistory.data = JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType));
				        _context.PaymentTypeChangeHistories.Add(newPaymentTypeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.PaymentType rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType)),
						null);


				    return Ok(Database.PaymentType.CreateAnonymous(paymentType));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.PaymentType rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.PaymentType rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a PaymentType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentType</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentType/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetPaymentTypeChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.PaymentType paymentType = await _context.PaymentTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentType == null)
			{
				return NotFound();
			}

			try
			{
				paymentType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PaymentType> versionInfo = await paymentType.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a PaymentType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentType</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentType/{id}/AuditHistory")]
		public async Task<IActionResult> GetPaymentTypeAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.PaymentType paymentType = await _context.PaymentTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentType == null)
			{
				return NotFound();
			}

			try
			{
				paymentType.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.PaymentType>> versions = await paymentType.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a PaymentType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentType</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The PaymentType object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentType/{id}/Version/{version}")]
		public async Task<IActionResult> GetPaymentTypeVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.PaymentType paymentType = await _context.PaymentTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentType == null)
			{
				return NotFound();
			}

			try
			{
				paymentType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PaymentType> versionInfo = await paymentType.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a PaymentType at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentType</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The PaymentType object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentType/{id}/StateAtTime")]
		public async Task<IActionResult> GetPaymentTypeStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.PaymentType paymentType = await _context.PaymentTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentType == null)
			{
				return NotFound();
			}

			try
			{
				paymentType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PaymentType> versionInfo = await paymentType.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a PaymentType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentType/{id}")]
		[Route("api/PaymentType")]
		public async Task<IActionResult> DeletePaymentType(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.PaymentType> query = (from x in _context.PaymentTypes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PaymentType paymentType = await query.FirstOrDefaultAsync(cancellationToken);

			if (paymentType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentType DELETE", id.ToString(), new Exception("No Scheduler.PaymentType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PaymentType cloneOfExisting = (Database.PaymentType)_context.Entry(paymentType).GetDatabaseValues().ToObject();


			lock (paymentTypeDeleteSyncRoot)
			{
			    try
			    {
			        paymentType.deleted = true;
			        paymentType.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        PaymentTypeChangeHistory paymentTypeChangeHistory = new PaymentTypeChangeHistory();
			        paymentTypeChangeHistory.paymentTypeId = paymentType.id;
			        paymentTypeChangeHistory.versionNumber = paymentType.versionNumber;
			        paymentTypeChangeHistory.timeStamp = DateTime.UtcNow;
			        paymentTypeChangeHistory.userId = securityUser.id;
			        paymentTypeChangeHistory.tenantGuid = userTenantGuid;
			        paymentTypeChangeHistory.data = JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType));
			        _context.PaymentTypeChangeHistories.Add(paymentTypeChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.PaymentType entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.PaymentType entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentType.CreateAnonymousWithFirstLevelSubObjects(paymentType)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of PaymentType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PaymentTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string color = null,
			int? sequence = null,
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

			IQueryable<Database.PaymentType> query = (from pt in _context.PaymentTypes select pt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pt => pt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(pt => pt.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(pt => pt.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pt => pt.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pt => pt.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pt => pt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pt => pt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pt => pt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pt => pt.deleted == false);
				}
			}
			else
			{
				query = query.Where(pt => pt.active == true);
				query = query.Where(pt => pt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Payment Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PaymentType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PaymentType/CreateAuditEvent")]
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
