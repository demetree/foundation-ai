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
    /// This auto generated class provides the basic CRUD operations for the ReceiptType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ReceiptType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ReceiptTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object receiptTypePutSyncRoot = new object();
		static object receiptTypeDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ReceiptTypesController> _logger;

		public ReceiptTypesController(SchedulerContext context, ILogger<ReceiptTypesController> logger) : base("Scheduler", "ReceiptType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ReceiptTypes filtered by the parameters provided.
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
		[Route("api/ReceiptTypes")]
		public async Task<IActionResult> GetReceiptTypes(
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

			IQueryable<Database.ReceiptType> query = (from rt in _context.ReceiptTypes select rt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(rt => rt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(rt => rt.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(rt => rt.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(rt => rt.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rt => rt.versionNumber == versionNumber.Value);
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

			query = query.OrderBy(rt => rt.sequence).ThenBy(rt => rt.name).ThenBy(rt => rt.description).ThenBy(rt => rt.color);


			//
			// Add the any string contains parameter to span all the string fields on the Receipt Type, or on an any of the string fields on its immediate relations
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
			
			List<Database.ReceiptType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ReceiptType receiptType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(receiptType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ReceiptType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ReceiptType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ReceiptTypes filtered by the parameters provided.  Its query is similar to the GetReceiptTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReceiptTypes/RowCount")]
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


			IQueryable<Database.ReceiptType> query = (from rt in _context.ReceiptTypes select rt);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(rt => rt.name == name);
			}
			if (description != null)
			{
				query = query.Where(rt => rt.description == description);
			}
			if (color != null)
			{
				query = query.Where(rt => rt.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(rt => rt.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rt => rt.versionNumber == versionNumber.Value);
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
			// Add the any string contains parameter to span all the string fields on the Receipt Type, or on an any of the string fields on its immediate relations
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
        /// This gets a single ReceiptType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReceiptType/{id}")]
		public async Task<IActionResult> GetReceiptType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ReceiptType> query = (from rt in _context.ReceiptTypes where
							(rt.id == id) &&
							(userIsAdmin == true || rt.deleted == false) &&
							(userIsWriter == true || rt.active == true)
					select rt);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.ReceiptType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ReceiptType Entity was read with Admin privilege." : "Scheduler.ReceiptType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ReceiptType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ReceiptType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ReceiptType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ReceiptType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ReceiptType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ReceiptType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutReceiptType(int id, [FromBody]Database.ReceiptType.ReceiptTypeDTO receiptTypeDTO, CancellationToken cancellationToken = default)
		{
			if (receiptTypeDTO == null)
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



			if (id != receiptTypeDTO.id)
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


			IQueryable<Database.ReceiptType> query = (from x in _context.ReceiptTypes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ReceiptType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ReceiptType PUT", id.ToString(), new Exception("No Scheduler.ReceiptType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (receiptTypeDTO.objectGuid == Guid.Empty)
            {
                receiptTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (receiptTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ReceiptType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ReceiptType cloneOfExisting = (Database.ReceiptType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ReceiptType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ReceiptType receiptType = (Database.ReceiptType)_context.Entry(existing).GetDatabaseValues().ToObject();
			receiptType.ApplyDTO(receiptTypeDTO);
			//
			// The tenant guid for any ReceiptType being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ReceiptType because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				receiptType.tenantGuid = existing.tenantGuid;
			}

			lock (receiptTypePutSyncRoot)
			{
				//
				// Validate the version number for the receiptType being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != receiptType.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ReceiptType save attempt was made but save request was with version " + receiptType.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ReceiptType you are trying to update has already changed.  Please try your save again after reloading the ReceiptType.");
				}
				else
				{
					// Same record.  Increase version.
					receiptType.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (receiptType.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ReceiptType record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (receiptType.name != null && receiptType.name.Length > 100)
				{
					receiptType.name = receiptType.name.Substring(0, 100);
				}

				if (receiptType.description != null && receiptType.description.Length > 500)
				{
					receiptType.description = receiptType.description.Substring(0, 500);
				}

				if (receiptType.color != null && receiptType.color.Length > 10)
				{
					receiptType.color = receiptType.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.ReceiptType> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(receiptType);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ReceiptTypeChangeHistory receiptTypeChangeHistory = new ReceiptTypeChangeHistory();
				        receiptTypeChangeHistory.receiptTypeId = receiptType.id;
				        receiptTypeChangeHistory.versionNumber = receiptType.versionNumber;
				        receiptTypeChangeHistory.timeStamp = DateTime.UtcNow;
				        receiptTypeChangeHistory.userId = securityUser.id;
				        receiptTypeChangeHistory.tenantGuid = userTenantGuid;
				        receiptTypeChangeHistory.data = JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType));
				        _context.ReceiptTypeChangeHistories.Add(receiptTypeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ReceiptType entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType)),
						null);

				return Ok(Database.ReceiptType.CreateAnonymous(receiptType));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ReceiptType entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ReceiptType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReceiptType", Name = "ReceiptType")]
		public async Task<IActionResult> PostReceiptType([FromBody]Database.ReceiptType.ReceiptTypeDTO receiptTypeDTO, CancellationToken cancellationToken = default)
		{
			if (receiptTypeDTO == null)
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
			// Create a new ReceiptType object using the data from the DTO
			//
			Database.ReceiptType receiptType = Database.ReceiptType.FromDTO(receiptTypeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				receiptType.tenantGuid = userTenantGuid;

				if (receiptType.name != null && receiptType.name.Length > 100)
				{
					receiptType.name = receiptType.name.Substring(0, 100);
				}

				if (receiptType.description != null && receiptType.description.Length > 500)
				{
					receiptType.description = receiptType.description.Substring(0, 500);
				}

				if (receiptType.color != null && receiptType.color.Length > 10)
				{
					receiptType.color = receiptType.color.Substring(0, 10);
				}

				receiptType.objectGuid = Guid.NewGuid();
				receiptType.versionNumber = 1;

				_context.ReceiptTypes.Add(receiptType);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the receiptType object so that no further changes will be written to the database
				    //
				    _context.Entry(receiptType).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					receiptType.Gifts = null;
					receiptType.ReceiptTypeChangeHistories = null;


				    ReceiptTypeChangeHistory receiptTypeChangeHistory = new ReceiptTypeChangeHistory();
				    receiptTypeChangeHistory.receiptTypeId = receiptType.id;
				    receiptTypeChangeHistory.versionNumber = receiptType.versionNumber;
				    receiptTypeChangeHistory.timeStamp = DateTime.UtcNow;
				    receiptTypeChangeHistory.userId = securityUser.id;
				    receiptTypeChangeHistory.tenantGuid = userTenantGuid;
				    receiptTypeChangeHistory.data = JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType));
				    _context.ReceiptTypeChangeHistories.Add(receiptTypeChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ReceiptType entity successfully created.",
						true,
						receiptType. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ReceiptType entity creation failed.", false, receiptType.id.ToString(), "", JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ReceiptType", receiptType.id, receiptType.name));

			return CreatedAtRoute("ReceiptType", new { id = receiptType.id }, Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType));
		}



        /// <summary>
        /// 
        /// This rolls a ReceiptType entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReceiptType/Rollback/{id}")]
		[Route("api/ReceiptType/Rollback")]
		public async Task<IActionResult> RollbackToReceiptTypeVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ReceiptType> query = (from x in _context.ReceiptTypes
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ReceiptType concurrently
			//
			lock (receiptTypePutSyncRoot)
			{
				
				Database.ReceiptType receiptType = query.FirstOrDefault();
				
				if (receiptType == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ReceiptType rollback", id.ToString(), new Exception("No Scheduler.ReceiptType entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ReceiptType current state so we can log it.
				//
				Database.ReceiptType cloneOfExisting = (Database.ReceiptType)_context.Entry(receiptType).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Gifts = null;
				cloneOfExisting.ReceiptTypeChangeHistories = null;

				if (versionNumber >= receiptType.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ReceiptType rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ReceiptType rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ReceiptTypeChangeHistory receiptTypeChangeHistory = (from x in _context.ReceiptTypeChangeHistories
				                                               where
				                                               x.receiptTypeId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (receiptTypeChangeHistory != null)
				{
				    Database.ReceiptType oldReceiptType = JsonSerializer.Deserialize<Database.ReceiptType>(receiptTypeChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    receiptType.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    receiptType.name = oldReceiptType.name;
				    receiptType.description = oldReceiptType.description;
				    receiptType.color = oldReceiptType.color;
				    receiptType.sequence = oldReceiptType.sequence;
				    receiptType.objectGuid = oldReceiptType.objectGuid;
				    receiptType.active = oldReceiptType.active;
				    receiptType.deleted = oldReceiptType.deleted;

				    string serializedReceiptType = JsonSerializer.Serialize(receiptType);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ReceiptTypeChangeHistory newReceiptTypeChangeHistory = new ReceiptTypeChangeHistory();
				        newReceiptTypeChangeHistory.receiptTypeId = receiptType.id;
				        newReceiptTypeChangeHistory.versionNumber = receiptType.versionNumber;
				        newReceiptTypeChangeHistory.timeStamp = DateTime.UtcNow;
				        newReceiptTypeChangeHistory.userId = securityUser.id;
				        newReceiptTypeChangeHistory.tenantGuid = userTenantGuid;
				        newReceiptTypeChangeHistory.data = JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType));
				        _context.ReceiptTypeChangeHistories.Add(newReceiptTypeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ReceiptType rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType)),
						null);


				    return Ok(Database.ReceiptType.CreateAnonymous(receiptType));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ReceiptType rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ReceiptType rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ReceiptType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ReceiptType</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReceiptType/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetReceiptTypeChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ReceiptType receiptType = await _context.ReceiptTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (receiptType == null)
			{
				return NotFound();
			}

			try
			{
				receiptType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ReceiptType> versionInfo = await receiptType.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ReceiptType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ReceiptType</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReceiptType/{id}/AuditHistory")]
		public async Task<IActionResult> GetReceiptTypeAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ReceiptType receiptType = await _context.ReceiptTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (receiptType == null)
			{
				return NotFound();
			}

			try
			{
				receiptType.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ReceiptType>> versions = await receiptType.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ReceiptType.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ReceiptType</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ReceiptType object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReceiptType/{id}/Version/{version}")]
		public async Task<IActionResult> GetReceiptTypeVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ReceiptType receiptType = await _context.ReceiptTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (receiptType == null)
			{
				return NotFound();
			}

			try
			{
				receiptType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ReceiptType> versionInfo = await receiptType.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ReceiptType at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ReceiptType</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ReceiptType object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReceiptType/{id}/StateAtTime")]
		public async Task<IActionResult> GetReceiptTypeStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ReceiptType receiptType = await _context.ReceiptTypes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (receiptType == null)
			{
				return NotFound();
			}

			try
			{
				receiptType.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ReceiptType> versionInfo = await receiptType.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ReceiptType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReceiptType/{id}")]
		[Route("api/ReceiptType")]
		public async Task<IActionResult> DeleteReceiptType(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ReceiptType> query = (from x in _context.ReceiptTypes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ReceiptType receiptType = await query.FirstOrDefaultAsync(cancellationToken);

			if (receiptType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ReceiptType DELETE", id.ToString(), new Exception("No Scheduler.ReceiptType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ReceiptType cloneOfExisting = (Database.ReceiptType)_context.Entry(receiptType).GetDatabaseValues().ToObject();


			lock (receiptTypeDeleteSyncRoot)
			{
			    try
			    {
			        receiptType.deleted = true;
			        receiptType.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ReceiptTypeChangeHistory receiptTypeChangeHistory = new ReceiptTypeChangeHistory();
			        receiptTypeChangeHistory.receiptTypeId = receiptType.id;
			        receiptTypeChangeHistory.versionNumber = receiptType.versionNumber;
			        receiptTypeChangeHistory.timeStamp = DateTime.UtcNow;
			        receiptTypeChangeHistory.userId = securityUser.id;
			        receiptTypeChangeHistory.tenantGuid = userTenantGuid;
			        receiptTypeChangeHistory.data = JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType));
			        _context.ReceiptTypeChangeHistories.Add(receiptTypeChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ReceiptType entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ReceiptType entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ReceiptType.CreateAnonymousWithFirstLevelSubObjects(receiptType)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ReceiptType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ReceiptTypes/ListData")]
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

			IQueryable<Database.ReceiptType> query = (from rt in _context.ReceiptTypes select rt);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(rt => rt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(rt => rt.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(rt => rt.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(rt => rt.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rt => rt.versionNumber == versionNumber.Value);
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
			// Add the any string contains parameter to span all the string fields on the Receipt Type, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.ReceiptType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ReceiptType/CreateAuditEvent")]
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
