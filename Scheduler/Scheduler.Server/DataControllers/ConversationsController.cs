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
    /// This auto generated class provides the basic CRUD operations for the Conversation entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Conversation entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ConversationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<ConversationsController> _logger;

		public ConversationsController(SchedulerContext context, ILogger<ConversationsController> logger) : base("Scheduler", "Conversation")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Conversations filtered by the parameters provided.
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
		[Route("api/Conversations")]
		public async Task<IActionResult> GetConversations(
			int? createdByUserId = null,
			int? conversationTypeId = null,
			int? priority = null,
			DateTime? dateTimeCreated = null,
			string entity = null,
			int? entityId = null,
			string externalURL = null,
			string name = null,
			string description = null,
			bool? isPublic = null,
			int? userId = null,
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
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.Conversation> query = (from c in _context.Conversations select c);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (createdByUserId.HasValue == true)
			{
				query = query.Where(c => c.createdByUserId == createdByUserId.Value);
			}
			if (conversationTypeId.HasValue == true)
			{
				query = query.Where(c => c.conversationTypeId == conversationTypeId.Value);
			}
			if (priority.HasValue == true)
			{
				query = query.Where(c => c.priority == priority.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(c => c.dateTimeCreated == dateTimeCreated.Value);
			}
			if (string.IsNullOrEmpty(entity) == false)
			{
				query = query.Where(c => c.entity == entity);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(c => c.entityId == entityId.Value);
			}
			if (string.IsNullOrEmpty(externalURL) == false)
			{
				query = query.Where(c => c.externalURL == externalURL);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(c => c.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(c => c.description == description);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(c => c.isPublic == isPublic.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(c => c.userId == userId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}

			query = query.OrderBy(c => c.entity).ThenBy(c => c.externalURL).ThenBy(c => c.name);


			//
			// Add the any string contains parameter to span all the string fields on the Conversation, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.entity.Contains(anyStringContains)
			       || x.externalURL.Contains(anyStringContains)
			       || x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.conversationType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.conversationType.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.conversationType);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.Conversation> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Conversation conversation in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(conversation, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Conversation Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Conversation Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Conversations filtered by the parameters provided.  Its query is similar to the GetConversations method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Conversations/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? createdByUserId = null,
			int? conversationTypeId = null,
			int? priority = null,
			DateTime? dateTimeCreated = null,
			string entity = null,
			int? entityId = null,
			string externalURL = null,
			string name = null,
			string description = null,
			bool? isPublic = null,
			int? userId = null,
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
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.Conversation> query = (from c in _context.Conversations select c);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (createdByUserId.HasValue == true)
			{
				query = query.Where(c => c.createdByUserId == createdByUserId.Value);
			}
			if (conversationTypeId.HasValue == true)
			{
				query = query.Where(c => c.conversationTypeId == conversationTypeId.Value);
			}
			if (priority.HasValue == true)
			{
				query = query.Where(c => c.priority == priority.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(c => c.dateTimeCreated == dateTimeCreated.Value);
			}
			if (entity != null)
			{
				query = query.Where(c => c.entity == entity);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(c => c.entityId == entityId.Value);
			}
			if (externalURL != null)
			{
				query = query.Where(c => c.externalURL == externalURL);
			}
			if (name != null)
			{
				query = query.Where(c => c.name == name);
			}
			if (description != null)
			{
				query = query.Where(c => c.description == description);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(c => c.isPublic == isPublic.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(c => c.userId == userId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Conversation, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.entity.Contains(anyStringContains)
			       || x.externalURL.Contains(anyStringContains)
			       || x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.conversationType.name.Contains(anyStringContains)
			       || x.conversationType.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Conversation by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Conversation/{id}")]
		public async Task<IActionResult> GetConversation(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Conversation> query = (from c in _context.Conversations where
							(c.id == id) &&
							(userIsAdmin == true || c.deleted == false) &&
							(userIsWriter == true || c.active == true)
					select c);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.conversationType);
					query = query.AsSplitQuery();
				}

				Database.Conversation materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Conversation Entity was read with Admin privilege." : "Scheduler.Conversation Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Conversation", materialized.id, materialized.entity));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Conversation entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Conversation.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Conversation.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Conversation record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Conversation/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutConversation(int id, [FromBody]Database.Conversation.ConversationDTO conversationDTO, CancellationToken cancellationToken = default)
		{
			if (conversationDTO == null)
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



			if (id != conversationDTO.id)
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


			IQueryable<Database.Conversation> query = (from x in _context.Conversations
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Conversation existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Conversation PUT", id.ToString(), new Exception("No Scheduler.Conversation entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (conversationDTO.objectGuid == Guid.Empty)
            {
                conversationDTO.objectGuid = existing.objectGuid;
            }
            else if (conversationDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Conversation record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Conversation cloneOfExisting = (Database.Conversation)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Conversation object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Conversation conversation = (Database.Conversation)_context.Entry(existing).GetDatabaseValues().ToObject();
			conversation.ApplyDTO(conversationDTO);
			//
			// The tenant guid for any Conversation being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Conversation because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				conversation.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (conversation.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Conversation record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (conversation.dateTimeCreated.Kind != DateTimeKind.Utc)
			{
				conversation.dateTimeCreated = conversation.dateTimeCreated.ToUniversalTime();
			}

			if (conversation.entity != null && conversation.entity.Length > 250)
			{
				conversation.entity = conversation.entity.Substring(0, 250);
			}

			if (conversation.externalURL != null && conversation.externalURL.Length > 1000)
			{
				conversation.externalURL = conversation.externalURL.Substring(0, 1000);
			}

			if (conversation.name != null && conversation.name.Length > 250)
			{
				conversation.name = conversation.name.Substring(0, 250);
			}

			if (conversation.description != null && conversation.description.Length > 1000)
			{
				conversation.description = conversation.description.Substring(0, 1000);
			}

			EntityEntry<Database.Conversation> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(conversation);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.Conversation entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.Conversation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Conversation.CreateAnonymousWithFirstLevelSubObjects(conversation)),
					null);


				return Ok(Database.Conversation.CreateAnonymous(conversation));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.Conversation entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.Conversation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Conversation.CreateAnonymousWithFirstLevelSubObjects(conversation)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new Conversation record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Conversation", Name = "Conversation")]
		public async Task<IActionResult> PostConversation([FromBody]Database.Conversation.ConversationDTO conversationDTO, CancellationToken cancellationToken = default)
		{
			if (conversationDTO == null)
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
			// Create a new Conversation object using the data from the DTO
			//
			Database.Conversation conversation = Database.Conversation.FromDTO(conversationDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				conversation.tenantGuid = userTenantGuid;

				if (conversation.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					conversation.dateTimeCreated = conversation.dateTimeCreated.ToUniversalTime();
				}

				if (conversation.entity != null && conversation.entity.Length > 250)
				{
					conversation.entity = conversation.entity.Substring(0, 250);
				}

				if (conversation.externalURL != null && conversation.externalURL.Length > 1000)
				{
					conversation.externalURL = conversation.externalURL.Substring(0, 1000);
				}

				if (conversation.name != null && conversation.name.Length > 250)
				{
					conversation.name = conversation.name.Substring(0, 250);
				}

				if (conversation.description != null && conversation.description.Length > 1000)
				{
					conversation.description = conversation.description.Substring(0, 1000);
				}

				conversation.objectGuid = Guid.NewGuid();
				_context.Conversations.Add(conversation);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.Conversation entity successfully created.",
					true,
					conversation.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.Conversation.CreateAnonymousWithFirstLevelSubObjects(conversation)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Conversation entity creation failed.", false, conversation.id.ToString(), "", JsonSerializer.Serialize(conversation), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Conversation", conversation.id, conversation.entity));

			return CreatedAtRoute("Conversation", new { id = conversation.id }, Database.Conversation.CreateAnonymousWithFirstLevelSubObjects(conversation));
		}



        /// <summary>
        /// 
        /// This deletes a Conversation record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Conversation/{id}")]
		[Route("api/Conversation")]
		public async Task<IActionResult> DeleteConversation(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Conversation> query = (from x in _context.Conversations
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Conversation conversation = await query.FirstOrDefaultAsync(cancellationToken);

			if (conversation == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Conversation DELETE", id.ToString(), new Exception("No Scheduler.Conversation entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Conversation cloneOfExisting = (Database.Conversation)_context.Entry(conversation).GetDatabaseValues().ToObject();


			try
			{
				conversation.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.Conversation entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.Conversation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Conversation.CreateAnonymousWithFirstLevelSubObjects(conversation)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.Conversation entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.Conversation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Conversation.CreateAnonymousWithFirstLevelSubObjects(conversation)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of Conversation records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Conversations/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? createdByUserId = null,
			int? conversationTypeId = null,
			int? priority = null,
			DateTime? dateTimeCreated = null,
			string entity = null,
			int? entityId = null,
			string externalURL = null,
			string name = null,
			string description = null,
			bool? isPublic = null,
			int? userId = null,
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
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.Conversation> query = (from c in _context.Conversations select c);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (createdByUserId.HasValue == true)
			{
				query = query.Where(c => c.createdByUserId == createdByUserId.Value);
			}
			if (conversationTypeId.HasValue == true)
			{
				query = query.Where(c => c.conversationTypeId == conversationTypeId.Value);
			}
			if (priority.HasValue == true)
			{
				query = query.Where(c => c.priority == priority.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(c => c.dateTimeCreated == dateTimeCreated.Value);
			}
			if (string.IsNullOrEmpty(entity) == false)
			{
				query = query.Where(c => c.entity == entity);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(c => c.entityId == entityId.Value);
			}
			if (string.IsNullOrEmpty(externalURL) == false)
			{
				query = query.Where(c => c.externalURL == externalURL);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(c => c.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(c => c.description == description);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(c => c.isPublic == isPublic.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(c => c.userId == userId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Conversation, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.entity.Contains(anyStringContains)
			       || x.externalURL.Contains(anyStringContains)
			       || x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.conversationType.name.Contains(anyStringContains)
			       || x.conversationType.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.entity).ThenBy(x => x.externalURL).ThenBy(x => x.name);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Conversation.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Conversation/CreateAuditEvent")]
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
