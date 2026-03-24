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
    /// This auto generated class provides the basic CRUD operations for the NotificationDistribution entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the NotificationDistribution entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class NotificationDistributionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<NotificationDistributionsController> _logger;

		public NotificationDistributionsController(SchedulerContext context, ILogger<NotificationDistributionsController> logger) : base("Scheduler", "NotificationDistribution")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of NotificationDistributions filtered by the parameters provided.
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
		[Route("api/NotificationDistributions")]
		public async Task<IActionResult> GetNotificationDistributions(
			int? notificationId = null,
			int? userId = null,
			bool? acknowledged = null,
			DateTime? dateTimeAcknowledged = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (dateTimeAcknowledged.HasValue == true && dateTimeAcknowledged.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeAcknowledged = dateTimeAcknowledged.Value.ToUniversalTime();
			}

			IQueryable<Database.NotificationDistribution> query = (from nd in _context.NotificationDistributions select nd);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (notificationId.HasValue == true)
			{
				query = query.Where(nd => nd.notificationId == notificationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(nd => nd.userId == userId.Value);
			}
			if (acknowledged.HasValue == true)
			{
				query = query.Where(nd => nd.acknowledged == acknowledged.Value);
			}
			if (dateTimeAcknowledged.HasValue == true)
			{
				query = query.Where(nd => nd.dateTimeAcknowledged == dateTimeAcknowledged.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(nd => nd.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(nd => nd.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(nd => nd.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(nd => nd.deleted == false);
				}
			}
			else
			{
				query = query.Where(nd => nd.active == true);
				query = query.Where(nd => nd.deleted == false);
			}

			query = query.OrderBy(nd => nd.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.notification);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.NotificationDistribution> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.NotificationDistribution notificationDistribution in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(notificationDistribution, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.NotificationDistribution Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.NotificationDistribution Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of NotificationDistributions filtered by the parameters provided.  Its query is similar to the GetNotificationDistributions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationDistributions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? notificationId = null,
			int? userId = null,
			bool? acknowledged = null,
			DateTime? dateTimeAcknowledged = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (dateTimeAcknowledged.HasValue == true && dateTimeAcknowledged.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeAcknowledged = dateTimeAcknowledged.Value.ToUniversalTime();
			}

			IQueryable<Database.NotificationDistribution> query = (from nd in _context.NotificationDistributions select nd);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (notificationId.HasValue == true)
			{
				query = query.Where(nd => nd.notificationId == notificationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(nd => nd.userId == userId.Value);
			}
			if (acknowledged.HasValue == true)
			{
				query = query.Where(nd => nd.acknowledged == acknowledged.Value);
			}
			if (dateTimeAcknowledged.HasValue == true)
			{
				query = query.Where(nd => nd.dateTimeAcknowledged == dateTimeAcknowledged.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(nd => nd.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(nd => nd.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(nd => nd.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(nd => nd.deleted == false);
				}
			}
			else
			{
				query = query.Where(nd => nd.active == true);
				query = query.Where(nd => nd.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single NotificationDistribution by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationDistribution/{id}")]
		public async Task<IActionResult> GetNotificationDistribution(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.NotificationDistribution> query = (from nd in _context.NotificationDistributions where
							(nd.id == id) &&
							(userIsAdmin == true || nd.deleted == false) &&
							(userIsWriter == true || nd.active == true)
					select nd);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.notification);
					query = query.AsSplitQuery();
				}

				Database.NotificationDistribution materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.NotificationDistribution Entity was read with Admin privilege." : "Scheduler.NotificationDistribution Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "NotificationDistribution", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.NotificationDistribution entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.NotificationDistribution.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.NotificationDistribution.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing NotificationDistribution record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/NotificationDistribution/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutNotificationDistribution(int id, [FromBody]Database.NotificationDistribution.NotificationDistributionDTO notificationDistributionDTO, CancellationToken cancellationToken = default)
		{
			if (notificationDistributionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != notificationDistributionDTO.id)
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


			IQueryable<Database.NotificationDistribution> query = (from x in _context.NotificationDistributions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.NotificationDistribution existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.NotificationDistribution PUT", id.ToString(), new Exception("No Scheduler.NotificationDistribution entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (notificationDistributionDTO.objectGuid == Guid.Empty)
            {
                notificationDistributionDTO.objectGuid = existing.objectGuid;
            }
            else if (notificationDistributionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a NotificationDistribution record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.NotificationDistribution cloneOfExisting = (Database.NotificationDistribution)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new NotificationDistribution object using the data from the existing record, updated with what is in the DTO.
			//
			Database.NotificationDistribution notificationDistribution = (Database.NotificationDistribution)_context.Entry(existing).GetDatabaseValues().ToObject();
			notificationDistribution.ApplyDTO(notificationDistributionDTO);
			//
			// The tenant guid for any NotificationDistribution being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the NotificationDistribution because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				notificationDistribution.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (notificationDistribution.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.NotificationDistribution record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (notificationDistribution.dateTimeAcknowledged.HasValue == true && notificationDistribution.dateTimeAcknowledged.Value.Kind != DateTimeKind.Utc)
			{
				notificationDistribution.dateTimeAcknowledged = notificationDistribution.dateTimeAcknowledged.Value.ToUniversalTime();
			}

			EntityEntry<Database.NotificationDistribution> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(notificationDistribution);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.NotificationDistribution entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationDistribution.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationDistribution.CreateAnonymousWithFirstLevelSubObjects(notificationDistribution)),
					null);


				return Ok(Database.NotificationDistribution.CreateAnonymous(notificationDistribution));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.NotificationDistribution entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationDistribution.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationDistribution.CreateAnonymousWithFirstLevelSubObjects(notificationDistribution)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new NotificationDistribution record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationDistribution", Name = "NotificationDistribution")]
		public async Task<IActionResult> PostNotificationDistribution([FromBody]Database.NotificationDistribution.NotificationDistributionDTO notificationDistributionDTO, CancellationToken cancellationToken = default)
		{
			if (notificationDistributionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
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

			//
			// Create a new NotificationDistribution object using the data from the DTO
			//
			Database.NotificationDistribution notificationDistribution = Database.NotificationDistribution.FromDTO(notificationDistributionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				notificationDistribution.tenantGuid = userTenantGuid;

				if (notificationDistribution.dateTimeAcknowledged.HasValue == true && notificationDistribution.dateTimeAcknowledged.Value.Kind != DateTimeKind.Utc)
				{
					notificationDistribution.dateTimeAcknowledged = notificationDistribution.dateTimeAcknowledged.Value.ToUniversalTime();
				}

				notificationDistribution.objectGuid = Guid.NewGuid();
				_context.NotificationDistributions.Add(notificationDistribution);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.NotificationDistribution entity successfully created.",
					true,
					notificationDistribution.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.NotificationDistribution.CreateAnonymousWithFirstLevelSubObjects(notificationDistribution)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.NotificationDistribution entity creation failed.", false, notificationDistribution.id.ToString(), "", JsonSerializer.Serialize(notificationDistribution), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "NotificationDistribution", notificationDistribution.id, notificationDistribution.id.ToString()));

			return CreatedAtRoute("NotificationDistribution", new { id = notificationDistribution.id }, Database.NotificationDistribution.CreateAnonymousWithFirstLevelSubObjects(notificationDistribution));
		}



        /// <summary>
        /// 
        /// This deletes a NotificationDistribution record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationDistribution/{id}")]
		[Route("api/NotificationDistribution")]
		public async Task<IActionResult> DeleteNotificationDistribution(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.NotificationDistribution> query = (from x in _context.NotificationDistributions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.NotificationDistribution notificationDistribution = await query.FirstOrDefaultAsync(cancellationToken);

			if (notificationDistribution == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.NotificationDistribution DELETE", id.ToString(), new Exception("No Scheduler.NotificationDistribution entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.NotificationDistribution cloneOfExisting = (Database.NotificationDistribution)_context.Entry(notificationDistribution).GetDatabaseValues().ToObject();


			try
			{
				notificationDistribution.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.NotificationDistribution entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationDistribution.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationDistribution.CreateAnonymousWithFirstLevelSubObjects(notificationDistribution)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.NotificationDistribution entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.NotificationDistribution.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.NotificationDistribution.CreateAnonymousWithFirstLevelSubObjects(notificationDistribution)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of NotificationDistribution records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/NotificationDistributions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? notificationId = null,
			int? userId = null,
			bool? acknowledged = null,
			DateTime? dateTimeAcknowledged = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (dateTimeAcknowledged.HasValue == true && dateTimeAcknowledged.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeAcknowledged = dateTimeAcknowledged.Value.ToUniversalTime();
			}

			IQueryable<Database.NotificationDistribution> query = (from nd in _context.NotificationDistributions select nd);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (notificationId.HasValue == true)
			{
				query = query.Where(nd => nd.notificationId == notificationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(nd => nd.userId == userId.Value);
			}
			if (acknowledged.HasValue == true)
			{
				query = query.Where(nd => nd.acknowledged == acknowledged.Value);
			}
			if (dateTimeAcknowledged.HasValue == true)
			{
				query = query.Where(nd => nd.dateTimeAcknowledged == dateTimeAcknowledged.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(nd => nd.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(nd => nd.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(nd => nd.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(nd => nd.deleted == false);
				}
			}
			else
			{
				query = query.Where(nd => nd.active == true);
				query = query.Where(nd => nd.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.NotificationDistribution.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/NotificationDistribution/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
