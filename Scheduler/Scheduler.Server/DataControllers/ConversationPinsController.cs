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
    /// This auto generated class provides the basic CRUD operations for the ConversationPin entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ConversationPin entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ConversationPinsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<ConversationPinsController> _logger;

		public ConversationPinsController(SchedulerContext context, ILogger<ConversationPinsController> logger) : base("Scheduler", "ConversationPin")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ConversationPins filtered by the parameters provided.
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
		[Route("api/ConversationPins")]
		public async Task<IActionResult> GetConversationPins(
			int? conversationId = null,
			int? conversationMessageId = null,
			int? pinnedByUserId = null,
			DateTime? dateTimePinned = null,
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
			if (dateTimePinned.HasValue == true && dateTimePinned.Value.Kind != DateTimeKind.Utc)
			{
				dateTimePinned = dateTimePinned.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationPin> query = (from cp in _context.ConversationPins select cp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationId.HasValue == true)
			{
				query = query.Where(cp => cp.conversationId == conversationId.Value);
			}
			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(cp => cp.conversationMessageId == conversationMessageId.Value);
			}
			if (pinnedByUserId.HasValue == true)
			{
				query = query.Where(cp => cp.pinnedByUserId == pinnedByUserId.Value);
			}
			if (dateTimePinned.HasValue == true)
			{
				query = query.Where(cp => cp.dateTimePinned == dateTimePinned.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cp => cp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cp => cp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cp => cp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cp => cp.deleted == false);
				}
			}
			else
			{
				query = query.Where(cp => cp.active == true);
				query = query.Where(cp => cp.deleted == false);
			}

			query = query.OrderBy(cp => cp.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.conversation);
				query = query.Include(x => x.conversationMessage);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ConversationPin> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ConversationPin conversationPin in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(conversationPin, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ConversationPin Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ConversationPin Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ConversationPins filtered by the parameters provided.  Its query is similar to the GetConversationPins method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationPins/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? conversationId = null,
			int? conversationMessageId = null,
			int? pinnedByUserId = null,
			DateTime? dateTimePinned = null,
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
			if (dateTimePinned.HasValue == true && dateTimePinned.Value.Kind != DateTimeKind.Utc)
			{
				dateTimePinned = dateTimePinned.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationPin> query = (from cp in _context.ConversationPins select cp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (conversationId.HasValue == true)
			{
				query = query.Where(cp => cp.conversationId == conversationId.Value);
			}
			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(cp => cp.conversationMessageId == conversationMessageId.Value);
			}
			if (pinnedByUserId.HasValue == true)
			{
				query = query.Where(cp => cp.pinnedByUserId == pinnedByUserId.Value);
			}
			if (dateTimePinned.HasValue == true)
			{
				query = query.Where(cp => cp.dateTimePinned == dateTimePinned.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cp => cp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cp => cp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cp => cp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cp => cp.deleted == false);
				}
			}
			else
			{
				query = query.Where(cp => cp.active == true);
				query = query.Where(cp => cp.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ConversationPin by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationPin/{id}")]
		public async Task<IActionResult> GetConversationPin(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ConversationPin> query = (from cp in _context.ConversationPins where
							(cp.id == id) &&
							(userIsAdmin == true || cp.deleted == false) &&
							(userIsWriter == true || cp.active == true)
					select cp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.conversation);
					query = query.Include(x => x.conversationMessage);
					query = query.AsSplitQuery();
				}

				Database.ConversationPin materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ConversationPin Entity was read with Admin privilege." : "Scheduler.ConversationPin Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationPin", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ConversationPin entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ConversationPin.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ConversationPin.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ConversationPin record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ConversationPin/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutConversationPin(int id, [FromBody]Database.ConversationPin.ConversationPinDTO conversationPinDTO, CancellationToken cancellationToken = default)
		{
			if (conversationPinDTO == null)
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



			if (id != conversationPinDTO.id)
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


			IQueryable<Database.ConversationPin> query = (from x in _context.ConversationPins
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationPin existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationPin PUT", id.ToString(), new Exception("No Scheduler.ConversationPin entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (conversationPinDTO.objectGuid == Guid.Empty)
            {
                conversationPinDTO.objectGuid = existing.objectGuid;
            }
            else if (conversationPinDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ConversationPin record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ConversationPin cloneOfExisting = (Database.ConversationPin)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ConversationPin object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ConversationPin conversationPin = (Database.ConversationPin)_context.Entry(existing).GetDatabaseValues().ToObject();
			conversationPin.ApplyDTO(conversationPinDTO);
			//
			// The tenant guid for any ConversationPin being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ConversationPin because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				conversationPin.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (conversationPin.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ConversationPin record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (conversationPin.dateTimePinned.Kind != DateTimeKind.Utc)
			{
				conversationPin.dateTimePinned = conversationPin.dateTimePinned.ToUniversalTime();
			}

			EntityEntry<Database.ConversationPin> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(conversationPin);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.ConversationPin entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConversationPin.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConversationPin.CreateAnonymousWithFirstLevelSubObjects(conversationPin)),
					null);


				return Ok(Database.ConversationPin.CreateAnonymous(conversationPin));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.ConversationPin entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConversationPin.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConversationPin.CreateAnonymousWithFirstLevelSubObjects(conversationPin)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ConversationPin record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationPin", Name = "ConversationPin")]
		public async Task<IActionResult> PostConversationPin([FromBody]Database.ConversationPin.ConversationPinDTO conversationPinDTO, CancellationToken cancellationToken = default)
		{
			if (conversationPinDTO == null)
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
			// Create a new ConversationPin object using the data from the DTO
			//
			Database.ConversationPin conversationPin = Database.ConversationPin.FromDTO(conversationPinDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				conversationPin.tenantGuid = userTenantGuid;

				if (conversationPin.dateTimePinned.Kind != DateTimeKind.Utc)
				{
					conversationPin.dateTimePinned = conversationPin.dateTimePinned.ToUniversalTime();
				}

				conversationPin.objectGuid = Guid.NewGuid();
				_context.ConversationPins.Add(conversationPin);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.ConversationPin entity successfully created.",
					true,
					conversationPin.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ConversationPin.CreateAnonymousWithFirstLevelSubObjects(conversationPin)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ConversationPin entity creation failed.", false, conversationPin.id.ToString(), "", JsonSerializer.Serialize(conversationPin), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationPin", conversationPin.id, conversationPin.id.ToString()));

			return CreatedAtRoute("ConversationPin", new { id = conversationPin.id }, Database.ConversationPin.CreateAnonymousWithFirstLevelSubObjects(conversationPin));
		}



        /// <summary>
        /// 
        /// This deletes a ConversationPin record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationPin/{id}")]
		[Route("api/ConversationPin")]
		public async Task<IActionResult> DeleteConversationPin(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ConversationPin> query = (from x in _context.ConversationPins
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationPin conversationPin = await query.FirstOrDefaultAsync(cancellationToken);

			if (conversationPin == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationPin DELETE", id.ToString(), new Exception("No Scheduler.ConversationPin entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ConversationPin cloneOfExisting = (Database.ConversationPin)_context.Entry(conversationPin).GetDatabaseValues().ToObject();


			try
			{
				conversationPin.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.ConversationPin entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConversationPin.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConversationPin.CreateAnonymousWithFirstLevelSubObjects(conversationPin)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.ConversationPin entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConversationPin.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConversationPin.CreateAnonymousWithFirstLevelSubObjects(conversationPin)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ConversationPin records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ConversationPins/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? conversationId = null,
			int? conversationMessageId = null,
			int? pinnedByUserId = null,
			DateTime? dateTimePinned = null,
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
			if (dateTimePinned.HasValue == true && dateTimePinned.Value.Kind != DateTimeKind.Utc)
			{
				dateTimePinned = dateTimePinned.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationPin> query = (from cp in _context.ConversationPins select cp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationId.HasValue == true)
			{
				query = query.Where(cp => cp.conversationId == conversationId.Value);
			}
			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(cp => cp.conversationMessageId == conversationMessageId.Value);
			}
			if (pinnedByUserId.HasValue == true)
			{
				query = query.Where(cp => cp.pinnedByUserId == pinnedByUserId.Value);
			}
			if (dateTimePinned.HasValue == true)
			{
				query = query.Where(cp => cp.dateTimePinned == dateTimePinned.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cp => cp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cp => cp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cp => cp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cp => cp.deleted == false);
				}
			}
			else
			{
				query = query.Where(cp => cp.active == true);
				query = query.Where(cp => cp.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ConversationPin.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ConversationPin/CreateAuditEvent")]
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
