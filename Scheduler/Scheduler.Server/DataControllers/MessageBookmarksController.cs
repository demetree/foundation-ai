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
    /// This auto generated class provides the basic CRUD operations for the MessageBookmark entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MessageBookmark entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MessageBookmarksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<MessageBookmarksController> _logger;

		public MessageBookmarksController(SchedulerContext context, ILogger<MessageBookmarksController> logger) : base("Scheduler", "MessageBookmark")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MessageBookmarks filtered by the parameters provided.
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
		[Route("api/MessageBookmarks")]
		public async Task<IActionResult> GetMessageBookmarks(
			int? userId = null,
			int? conversationMessageId = null,
			string note = null,
			DateTime? dateTimeCreated = null,
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

			IQueryable<Database.MessageBookmark> query = (from mb in _context.MessageBookmarks select mb);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userId.HasValue == true)
			{
				query = query.Where(mb => mb.userId == userId.Value);
			}
			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(mb => mb.conversationMessageId == conversationMessageId.Value);
			}
			if (string.IsNullOrEmpty(note) == false)
			{
				query = query.Where(mb => mb.note == note);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(mb => mb.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mb => mb.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mb => mb.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mb => mb.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mb => mb.deleted == false);
				}
			}
			else
			{
				query = query.Where(mb => mb.active == true);
				query = query.Where(mb => mb.deleted == false);
			}

			query = query.OrderBy(mb => mb.note);


			//
			// Add the any string contains parameter to span all the string fields on the Message Bookmark, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.note.Contains(anyStringContains)
			       || (includeRelations == true && x.conversationMessage.message.Contains(anyStringContains))
			       || (includeRelations == true && x.conversationMessage.messageType.Contains(anyStringContains))
			       || (includeRelations == true && x.conversationMessage.entity.Contains(anyStringContains))
			       || (includeRelations == true && x.conversationMessage.externalURL.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.conversationMessage);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.MessageBookmark> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MessageBookmark messageBookmark in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(messageBookmark, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.MessageBookmark Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.MessageBookmark Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MessageBookmarks filtered by the parameters provided.  Its query is similar to the GetMessageBookmarks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessageBookmarks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userId = null,
			int? conversationMessageId = null,
			string note = null,
			DateTime? dateTimeCreated = null,
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

			IQueryable<Database.MessageBookmark> query = (from mb in _context.MessageBookmarks select mb);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userId.HasValue == true)
			{
				query = query.Where(mb => mb.userId == userId.Value);
			}
			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(mb => mb.conversationMessageId == conversationMessageId.Value);
			}
			if (note != null)
			{
				query = query.Where(mb => mb.note == note);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(mb => mb.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mb => mb.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mb => mb.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mb => mb.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mb => mb.deleted == false);
				}
			}
			else
			{
				query = query.Where(mb => mb.active == true);
				query = query.Where(mb => mb.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Message Bookmark, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.note.Contains(anyStringContains)
			       || x.conversationMessage.message.Contains(anyStringContains)
			       || x.conversationMessage.messageType.Contains(anyStringContains)
			       || x.conversationMessage.entity.Contains(anyStringContains)
			       || x.conversationMessage.externalURL.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MessageBookmark by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessageBookmark/{id}")]
		public async Task<IActionResult> GetMessageBookmark(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.MessageBookmark> query = (from mb in _context.MessageBookmarks where
							(mb.id == id) &&
							(userIsAdmin == true || mb.deleted == false) &&
							(userIsWriter == true || mb.active == true)
					select mb);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.conversationMessage);
					query = query.AsSplitQuery();
				}

				Database.MessageBookmark materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.MessageBookmark Entity was read with Admin privilege." : "Scheduler.MessageBookmark Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MessageBookmark", materialized.id, materialized.note));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.MessageBookmark entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.MessageBookmark.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.MessageBookmark.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MessageBookmark record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MessageBookmark/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMessageBookmark(int id, [FromBody]Database.MessageBookmark.MessageBookmarkDTO messageBookmarkDTO, CancellationToken cancellationToken = default)
		{
			if (messageBookmarkDTO == null)
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



			if (id != messageBookmarkDTO.id)
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


			IQueryable<Database.MessageBookmark> query = (from x in _context.MessageBookmarks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MessageBookmark existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.MessageBookmark PUT", id.ToString(), new Exception("No Scheduler.MessageBookmark entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (messageBookmarkDTO.objectGuid == Guid.Empty)
            {
                messageBookmarkDTO.objectGuid = existing.objectGuid;
            }
            else if (messageBookmarkDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MessageBookmark record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MessageBookmark cloneOfExisting = (Database.MessageBookmark)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MessageBookmark object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MessageBookmark messageBookmark = (Database.MessageBookmark)_context.Entry(existing).GetDatabaseValues().ToObject();
			messageBookmark.ApplyDTO(messageBookmarkDTO);
			//
			// The tenant guid for any MessageBookmark being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the MessageBookmark because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				messageBookmark.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (messageBookmark.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.MessageBookmark record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (messageBookmark.note != null && messageBookmark.note.Length > 500)
			{
				messageBookmark.note = messageBookmark.note.Substring(0, 500);
			}

			if (messageBookmark.dateTimeCreated.Kind != DateTimeKind.Utc)
			{
				messageBookmark.dateTimeCreated = messageBookmark.dateTimeCreated.ToUniversalTime();
			}

			EntityEntry<Database.MessageBookmark> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(messageBookmark);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.MessageBookmark entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessageBookmark.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessageBookmark.CreateAnonymousWithFirstLevelSubObjects(messageBookmark)),
					null);


				return Ok(Database.MessageBookmark.CreateAnonymous(messageBookmark));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.MessageBookmark entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessageBookmark.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessageBookmark.CreateAnonymousWithFirstLevelSubObjects(messageBookmark)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MessageBookmark record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessageBookmark", Name = "MessageBookmark")]
		public async Task<IActionResult> PostMessageBookmark([FromBody]Database.MessageBookmark.MessageBookmarkDTO messageBookmarkDTO, CancellationToken cancellationToken = default)
		{
			if (messageBookmarkDTO == null)
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
			// Create a new MessageBookmark object using the data from the DTO
			//
			Database.MessageBookmark messageBookmark = Database.MessageBookmark.FromDTO(messageBookmarkDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				messageBookmark.tenantGuid = userTenantGuid;

				if (messageBookmark.note != null && messageBookmark.note.Length > 500)
				{
					messageBookmark.note = messageBookmark.note.Substring(0, 500);
				}

				if (messageBookmark.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					messageBookmark.dateTimeCreated = messageBookmark.dateTimeCreated.ToUniversalTime();
				}

				messageBookmark.objectGuid = Guid.NewGuid();
				_context.MessageBookmarks.Add(messageBookmark);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.MessageBookmark entity successfully created.",
					true,
					messageBookmark.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MessageBookmark.CreateAnonymousWithFirstLevelSubObjects(messageBookmark)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.MessageBookmark entity creation failed.", false, messageBookmark.id.ToString(), "", JsonSerializer.Serialize(messageBookmark), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MessageBookmark", messageBookmark.id, messageBookmark.note));

			return CreatedAtRoute("MessageBookmark", new { id = messageBookmark.id }, Database.MessageBookmark.CreateAnonymousWithFirstLevelSubObjects(messageBookmark));
		}



        /// <summary>
        /// 
        /// This deletes a MessageBookmark record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessageBookmark/{id}")]
		[Route("api/MessageBookmark")]
		public async Task<IActionResult> DeleteMessageBookmark(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.MessageBookmark> query = (from x in _context.MessageBookmarks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MessageBookmark messageBookmark = await query.FirstOrDefaultAsync(cancellationToken);

			if (messageBookmark == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.MessageBookmark DELETE", id.ToString(), new Exception("No Scheduler.MessageBookmark entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MessageBookmark cloneOfExisting = (Database.MessageBookmark)_context.Entry(messageBookmark).GetDatabaseValues().ToObject();


			try
			{
				messageBookmark.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.MessageBookmark entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessageBookmark.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessageBookmark.CreateAnonymousWithFirstLevelSubObjects(messageBookmark)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.MessageBookmark entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessageBookmark.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessageBookmark.CreateAnonymousWithFirstLevelSubObjects(messageBookmark)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MessageBookmark records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MessageBookmarks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userId = null,
			int? conversationMessageId = null,
			string note = null,
			DateTime? dateTimeCreated = null,
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

			IQueryable<Database.MessageBookmark> query = (from mb in _context.MessageBookmarks select mb);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userId.HasValue == true)
			{
				query = query.Where(mb => mb.userId == userId.Value);
			}
			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(mb => mb.conversationMessageId == conversationMessageId.Value);
			}
			if (string.IsNullOrEmpty(note) == false)
			{
				query = query.Where(mb => mb.note == note);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(mb => mb.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mb => mb.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mb => mb.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mb => mb.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mb => mb.deleted == false);
				}
			}
			else
			{
				query = query.Where(mb => mb.active == true);
				query = query.Where(mb => mb.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Message Bookmark, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.note.Contains(anyStringContains)
			       || x.conversationMessage.message.Contains(anyStringContains)
			       || x.conversationMessage.messageType.Contains(anyStringContains)
			       || x.conversationMessage.entity.Contains(anyStringContains)
			       || x.conversationMessage.externalURL.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.note);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MessageBookmark.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MessageBookmark/CreateAuditEvent")]
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
