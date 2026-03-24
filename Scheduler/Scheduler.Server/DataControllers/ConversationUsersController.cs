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
    /// This auto generated class provides the basic CRUD operations for the ConversationUser entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ConversationUser entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ConversationUsersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<ConversationUsersController> _logger;

		public ConversationUsersController(SchedulerContext context, ILogger<ConversationUsersController> logger) : base("Scheduler", "ConversationUser")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ConversationUsers filtered by the parameters provided.
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
		[Route("api/ConversationUsers")]
		public async Task<IActionResult> GetConversationUsers(
			int? conversationId = null,
			int? userId = null,
			string role = null,
			DateTime? dateTimeAdded = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (dateTimeAdded.HasValue == true && dateTimeAdded.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeAdded = dateTimeAdded.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationUser> query = (from cu in _context.ConversationUsers select cu);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationId.HasValue == true)
			{
				query = query.Where(cu => cu.conversationId == conversationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cu => cu.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(role) == false)
			{
				query = query.Where(cu => cu.role == role);
			}
			if (dateTimeAdded.HasValue == true)
			{
				query = query.Where(cu => cu.dateTimeAdded == dateTimeAdded.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cu => cu.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cu => cu.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cu => cu.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cu => cu.deleted == false);
				}
			}
			else
			{
				query = query.Where(cu => cu.active == true);
				query = query.Where(cu => cu.deleted == false);
			}

			query = query.OrderBy(cu => cu.role);


			//
			// Add the any string contains parameter to span all the string fields on the Conversation User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.role.Contains(anyStringContains)
			       || (includeRelations == true && x.conversation.entity.Contains(anyStringContains))
			       || (includeRelations == true && x.conversation.externalURL.Contains(anyStringContains))
			       || (includeRelations == true && x.conversation.name.Contains(anyStringContains))
			       || (includeRelations == true && x.conversation.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.conversation);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ConversationUser> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ConversationUser conversationUser in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(conversationUser, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ConversationUser Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ConversationUser Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ConversationUsers filtered by the parameters provided.  Its query is similar to the GetConversationUsers method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationUsers/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? conversationId = null,
			int? userId = null,
			string role = null,
			DateTime? dateTimeAdded = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (dateTimeAdded.HasValue == true && dateTimeAdded.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeAdded = dateTimeAdded.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationUser> query = (from cu in _context.ConversationUsers select cu);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (conversationId.HasValue == true)
			{
				query = query.Where(cu => cu.conversationId == conversationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cu => cu.userId == userId.Value);
			}
			if (role != null)
			{
				query = query.Where(cu => cu.role == role);
			}
			if (dateTimeAdded.HasValue == true)
			{
				query = query.Where(cu => cu.dateTimeAdded == dateTimeAdded.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cu => cu.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cu => cu.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cu => cu.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cu => cu.deleted == false);
				}
			}
			else
			{
				query = query.Where(cu => cu.active == true);
				query = query.Where(cu => cu.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Conversation User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.role.Contains(anyStringContains)
			       || x.conversation.entity.Contains(anyStringContains)
			       || x.conversation.externalURL.Contains(anyStringContains)
			       || x.conversation.name.Contains(anyStringContains)
			       || x.conversation.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ConversationUser by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationUser/{id}")]
		public async Task<IActionResult> GetConversationUser(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ConversationUser> query = (from cu in _context.ConversationUsers where
							(cu.id == id) &&
							(userIsAdmin == true || cu.deleted == false) &&
							(userIsWriter == true || cu.active == true)
					select cu);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.conversation);
					query = query.AsSplitQuery();
				}

				Database.ConversationUser materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ConversationUser Entity was read with Admin privilege." : "Scheduler.ConversationUser Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationUser", materialized.id, materialized.role));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ConversationUser entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ConversationUser.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ConversationUser.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ConversationUser record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ConversationUser/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutConversationUser(int id, [FromBody]Database.ConversationUser.ConversationUserDTO conversationUserDTO, CancellationToken cancellationToken = default)
		{
			if (conversationUserDTO == null)
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



			if (id != conversationUserDTO.id)
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


			IQueryable<Database.ConversationUser> query = (from x in _context.ConversationUsers
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationUser existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationUser PUT", id.ToString(), new Exception("No Scheduler.ConversationUser entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (conversationUserDTO.objectGuid == Guid.Empty)
            {
                conversationUserDTO.objectGuid = existing.objectGuid;
            }
            else if (conversationUserDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ConversationUser record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ConversationUser cloneOfExisting = (Database.ConversationUser)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ConversationUser object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ConversationUser conversationUser = (Database.ConversationUser)_context.Entry(existing).GetDatabaseValues().ToObject();
			conversationUser.ApplyDTO(conversationUserDTO);
			//
			// The tenant guid for any ConversationUser being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ConversationUser because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				conversationUser.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (conversationUser.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ConversationUser record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (conversationUser.role != null && conversationUser.role.Length > 50)
			{
				conversationUser.role = conversationUser.role.Substring(0, 50);
			}

			if (conversationUser.dateTimeAdded.Kind != DateTimeKind.Utc)
			{
				conversationUser.dateTimeAdded = conversationUser.dateTimeAdded.ToUniversalTime();
			}

			EntityEntry<Database.ConversationUser> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(conversationUser);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.ConversationUser entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConversationUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConversationUser.CreateAnonymousWithFirstLevelSubObjects(conversationUser)),
					null);


				return Ok(Database.ConversationUser.CreateAnonymous(conversationUser));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.ConversationUser entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConversationUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConversationUser.CreateAnonymousWithFirstLevelSubObjects(conversationUser)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ConversationUser record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationUser", Name = "ConversationUser")]
		public async Task<IActionResult> PostConversationUser([FromBody]Database.ConversationUser.ConversationUserDTO conversationUserDTO, CancellationToken cancellationToken = default)
		{
			if (conversationUserDTO == null)
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
			// Create a new ConversationUser object using the data from the DTO
			//
			Database.ConversationUser conversationUser = Database.ConversationUser.FromDTO(conversationUserDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				conversationUser.tenantGuid = userTenantGuid;

				if (conversationUser.role != null && conversationUser.role.Length > 50)
				{
					conversationUser.role = conversationUser.role.Substring(0, 50);
				}

				if (conversationUser.dateTimeAdded.Kind != DateTimeKind.Utc)
				{
					conversationUser.dateTimeAdded = conversationUser.dateTimeAdded.ToUniversalTime();
				}

				conversationUser.objectGuid = Guid.NewGuid();
				_context.ConversationUsers.Add(conversationUser);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.ConversationUser entity successfully created.",
					true,
					conversationUser.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ConversationUser.CreateAnonymousWithFirstLevelSubObjects(conversationUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ConversationUser entity creation failed.", false, conversationUser.id.ToString(), "", JsonSerializer.Serialize(conversationUser), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationUser", conversationUser.id, conversationUser.role));

			return CreatedAtRoute("ConversationUser", new { id = conversationUser.id }, Database.ConversationUser.CreateAnonymousWithFirstLevelSubObjects(conversationUser));
		}



        /// <summary>
        /// 
        /// This deletes a ConversationUser record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationUser/{id}")]
		[Route("api/ConversationUser")]
		public async Task<IActionResult> DeleteConversationUser(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ConversationUser> query = (from x in _context.ConversationUsers
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationUser conversationUser = await query.FirstOrDefaultAsync(cancellationToken);

			if (conversationUser == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationUser DELETE", id.ToString(), new Exception("No Scheduler.ConversationUser entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ConversationUser cloneOfExisting = (Database.ConversationUser)_context.Entry(conversationUser).GetDatabaseValues().ToObject();


			try
			{
				conversationUser.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.ConversationUser entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConversationUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConversationUser.CreateAnonymousWithFirstLevelSubObjects(conversationUser)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.ConversationUser entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ConversationUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ConversationUser.CreateAnonymousWithFirstLevelSubObjects(conversationUser)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ConversationUser records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ConversationUsers/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? conversationId = null,
			int? userId = null,
			string role = null,
			DateTime? dateTimeAdded = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (dateTimeAdded.HasValue == true && dateTimeAdded.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeAdded = dateTimeAdded.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationUser> query = (from cu in _context.ConversationUsers select cu);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationId.HasValue == true)
			{
				query = query.Where(cu => cu.conversationId == conversationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cu => cu.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(role) == false)
			{
				query = query.Where(cu => cu.role == role);
			}
			if (dateTimeAdded.HasValue == true)
			{
				query = query.Where(cu => cu.dateTimeAdded == dateTimeAdded.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cu => cu.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cu => cu.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cu => cu.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cu => cu.deleted == false);
				}
			}
			else
			{
				query = query.Where(cu => cu.active == true);
				query = query.Where(cu => cu.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Conversation User, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.role.Contains(anyStringContains)
			       || x.conversation.entity.Contains(anyStringContains)
			       || x.conversation.externalURL.Contains(anyStringContains)
			       || x.conversation.name.Contains(anyStringContains)
			       || x.conversation.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.role);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ConversationUser.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ConversationUser/CreateAuditEvent")]
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
