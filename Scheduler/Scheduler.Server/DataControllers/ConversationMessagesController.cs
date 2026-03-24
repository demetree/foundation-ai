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
    /// This auto generated class provides the basic CRUD operations for the ConversationMessage entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ConversationMessage entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ConversationMessagesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object conversationMessagePutSyncRoot = new object();
		static object conversationMessageDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ConversationMessagesController> _logger;

		public ConversationMessagesController(SchedulerContext context, ILogger<ConversationMessagesController> logger) : base("Scheduler", "ConversationMessage")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ConversationMessages filtered by the parameters provided.
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
		[Route("api/ConversationMessages")]
		public async Task<IActionResult> GetConversationMessages(
			int? conversationId = null,
			int? userId = null,
			int? parentConversationMessageId = null,
			int? conversationChannelId = null,
			DateTime? dateTimeCreated = null,
			string message = null,
			string messageType = null,
			string entity = null,
			int? entityId = null,
			string externalURL = null,
			int? forwardedFromMessageId = null,
			int? forwardedFromUserId = null,
			bool? isScheduled = null,
			DateTime? scheduledDateTime = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			if (scheduledDateTime.HasValue == true && scheduledDateTime.Value.Kind != DateTimeKind.Utc)
			{
				scheduledDateTime = scheduledDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationMessage> query = (from cm in _context.ConversationMessages select cm);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationId.HasValue == true)
			{
				query = query.Where(cm => cm.conversationId == conversationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cm => cm.userId == userId.Value);
			}
			if (parentConversationMessageId.HasValue == true)
			{
				query = query.Where(cm => cm.parentConversationMessageId == parentConversationMessageId.Value);
			}
			if (conversationChannelId.HasValue == true)
			{
				query = query.Where(cm => cm.conversationChannelId == conversationChannelId.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(cm => cm.dateTimeCreated == dateTimeCreated.Value);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(cm => cm.message == message);
			}
			if (string.IsNullOrEmpty(messageType) == false)
			{
				query = query.Where(cm => cm.messageType == messageType);
			}
			if (string.IsNullOrEmpty(entity) == false)
			{
				query = query.Where(cm => cm.entity == entity);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(cm => cm.entityId == entityId.Value);
			}
			if (string.IsNullOrEmpty(externalURL) == false)
			{
				query = query.Where(cm => cm.externalURL == externalURL);
			}
			if (forwardedFromMessageId.HasValue == true)
			{
				query = query.Where(cm => cm.forwardedFromMessageId == forwardedFromMessageId.Value);
			}
			if (forwardedFromUserId.HasValue == true)
			{
				query = query.Where(cm => cm.forwardedFromUserId == forwardedFromUserId.Value);
			}
			if (isScheduled.HasValue == true)
			{
				query = query.Where(cm => cm.isScheduled == isScheduled.Value);
			}
			if (scheduledDateTime.HasValue == true)
			{
				query = query.Where(cm => cm.scheduledDateTime == scheduledDateTime.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cm => cm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cm => cm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cm => cm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cm => cm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cm => cm.deleted == false);
				}
			}
			else
			{
				query = query.Where(cm => cm.active == true);
				query = query.Where(cm => cm.deleted == false);
			}

			query = query.OrderBy(cm => cm.messageType).ThenBy(cm => cm.entity).ThenBy(cm => cm.externalURL);


			//
			// Add the any string contains parameter to span all the string fields on the Conversation Message, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.message.Contains(anyStringContains)
			       || x.messageType.Contains(anyStringContains)
			       || x.entity.Contains(anyStringContains)
			       || x.externalURL.Contains(anyStringContains)
			       || (includeRelations == true && x.conversation.entity.Contains(anyStringContains))
			       || (includeRelations == true && x.conversation.externalURL.Contains(anyStringContains))
			       || (includeRelations == true && x.conversation.name.Contains(anyStringContains))
			       || (includeRelations == true && x.conversation.description.Contains(anyStringContains))
			       || (includeRelations == true && x.conversationChannel.name.Contains(anyStringContains))
			       || (includeRelations == true && x.conversationChannel.topic.Contains(anyStringContains))
			       || (includeRelations == true && x.parentConversationMessage.message.Contains(anyStringContains))
			       || (includeRelations == true && x.parentConversationMessage.messageType.Contains(anyStringContains))
			       || (includeRelations == true && x.parentConversationMessage.entity.Contains(anyStringContains))
			       || (includeRelations == true && x.parentConversationMessage.externalURL.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.conversation);
				query = query.Include(x => x.conversationChannel);
				query = query.Include(x => x.parentConversationMessage);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ConversationMessage> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ConversationMessage conversationMessage in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(conversationMessage, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ConversationMessage Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ConversationMessage Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ConversationMessages filtered by the parameters provided.  Its query is similar to the GetConversationMessages method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessages/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? conversationId = null,
			int? userId = null,
			int? parentConversationMessageId = null,
			int? conversationChannelId = null,
			DateTime? dateTimeCreated = null,
			string message = null,
			string messageType = null,
			string entity = null,
			int? entityId = null,
			string externalURL = null,
			int? forwardedFromMessageId = null,
			int? forwardedFromUserId = null,
			bool? isScheduled = null,
			DateTime? scheduledDateTime = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			if (scheduledDateTime.HasValue == true && scheduledDateTime.Value.Kind != DateTimeKind.Utc)
			{
				scheduledDateTime = scheduledDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationMessage> query = (from cm in _context.ConversationMessages select cm);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (conversationId.HasValue == true)
			{
				query = query.Where(cm => cm.conversationId == conversationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cm => cm.userId == userId.Value);
			}
			if (parentConversationMessageId.HasValue == true)
			{
				query = query.Where(cm => cm.parentConversationMessageId == parentConversationMessageId.Value);
			}
			if (conversationChannelId.HasValue == true)
			{
				query = query.Where(cm => cm.conversationChannelId == conversationChannelId.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(cm => cm.dateTimeCreated == dateTimeCreated.Value);
			}
			if (message != null)
			{
				query = query.Where(cm => cm.message == message);
			}
			if (messageType != null)
			{
				query = query.Where(cm => cm.messageType == messageType);
			}
			if (entity != null)
			{
				query = query.Where(cm => cm.entity == entity);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(cm => cm.entityId == entityId.Value);
			}
			if (externalURL != null)
			{
				query = query.Where(cm => cm.externalURL == externalURL);
			}
			if (forwardedFromMessageId.HasValue == true)
			{
				query = query.Where(cm => cm.forwardedFromMessageId == forwardedFromMessageId.Value);
			}
			if (forwardedFromUserId.HasValue == true)
			{
				query = query.Where(cm => cm.forwardedFromUserId == forwardedFromUserId.Value);
			}
			if (isScheduled.HasValue == true)
			{
				query = query.Where(cm => cm.isScheduled == isScheduled.Value);
			}
			if (scheduledDateTime.HasValue == true)
			{
				query = query.Where(cm => cm.scheduledDateTime == scheduledDateTime.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cm => cm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cm => cm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cm => cm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cm => cm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cm => cm.deleted == false);
				}
			}
			else
			{
				query = query.Where(cm => cm.active == true);
				query = query.Where(cm => cm.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Conversation Message, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.message.Contains(anyStringContains)
			       || x.messageType.Contains(anyStringContains)
			       || x.entity.Contains(anyStringContains)
			       || x.externalURL.Contains(anyStringContains)
			       || x.conversation.entity.Contains(anyStringContains)
			       || x.conversation.externalURL.Contains(anyStringContains)
			       || x.conversation.name.Contains(anyStringContains)
			       || x.conversation.description.Contains(anyStringContains)
			       || x.conversationChannel.name.Contains(anyStringContains)
			       || x.conversationChannel.topic.Contains(anyStringContains)
			       || x.parentConversationMessage.message.Contains(anyStringContains)
			       || x.parentConversationMessage.messageType.Contains(anyStringContains)
			       || x.parentConversationMessage.entity.Contains(anyStringContains)
			       || x.parentConversationMessage.externalURL.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ConversationMessage by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessage/{id}")]
		public async Task<IActionResult> GetConversationMessage(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ConversationMessage> query = (from cm in _context.ConversationMessages where
							(cm.id == id) &&
							(userIsAdmin == true || cm.deleted == false) &&
							(userIsWriter == true || cm.active == true)
					select cm);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.conversation);
					query = query.Include(x => x.conversationChannel);
					query = query.Include(x => x.parentConversationMessage);
					query = query.AsSplitQuery();
				}

				Database.ConversationMessage materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ConversationMessage Entity was read with Admin privilege." : "Scheduler.ConversationMessage Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationMessage", materialized.id, materialized.messageType));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ConversationMessage entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ConversationMessage.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ConversationMessage.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ConversationMessage record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ConversationMessage/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutConversationMessage(int id, [FromBody]Database.ConversationMessage.ConversationMessageDTO conversationMessageDTO, CancellationToken cancellationToken = default)
		{
			if (conversationMessageDTO == null)
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



			if (id != conversationMessageDTO.id)
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


			IQueryable<Database.ConversationMessage> query = (from x in _context.ConversationMessages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationMessage existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationMessage PUT", id.ToString(), new Exception("No Scheduler.ConversationMessage entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (conversationMessageDTO.objectGuid == Guid.Empty)
            {
                conversationMessageDTO.objectGuid = existing.objectGuid;
            }
            else if (conversationMessageDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ConversationMessage record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ConversationMessage cloneOfExisting = (Database.ConversationMessage)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ConversationMessage object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ConversationMessage conversationMessage = (Database.ConversationMessage)_context.Entry(existing).GetDatabaseValues().ToObject();
			conversationMessage.ApplyDTO(conversationMessageDTO);
			//
			// The tenant guid for any ConversationMessage being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ConversationMessage because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				conversationMessage.tenantGuid = existing.tenantGuid;
			}

			lock (conversationMessagePutSyncRoot)
			{
				//
				// Validate the version number for the conversationMessage being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != conversationMessage.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ConversationMessage save attempt was made but save request was with version " + conversationMessage.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ConversationMessage you are trying to update has already changed.  Please try your save again after reloading the ConversationMessage.");
				}
				else
				{
					// Same record.  Increase version.
					conversationMessage.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (conversationMessage.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ConversationMessage record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (conversationMessage.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					conversationMessage.dateTimeCreated = conversationMessage.dateTimeCreated.ToUniversalTime();
				}

				if (conversationMessage.messageType != null && conversationMessage.messageType.Length > 50)
				{
					conversationMessage.messageType = conversationMessage.messageType.Substring(0, 50);
				}

				if (conversationMessage.entity != null && conversationMessage.entity.Length > 250)
				{
					conversationMessage.entity = conversationMessage.entity.Substring(0, 250);
				}

				if (conversationMessage.externalURL != null && conversationMessage.externalURL.Length > 1000)
				{
					conversationMessage.externalURL = conversationMessage.externalURL.Substring(0, 1000);
				}

				if (conversationMessage.scheduledDateTime.HasValue == true && conversationMessage.scheduledDateTime.Value.Kind != DateTimeKind.Utc)
				{
					conversationMessage.scheduledDateTime = conversationMessage.scheduledDateTime.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.ConversationMessage> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(conversationMessage);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ConversationMessageChangeHistory conversationMessageChangeHistory = new ConversationMessageChangeHistory();
				        conversationMessageChangeHistory.conversationMessageId = conversationMessage.id;
				        conversationMessageChangeHistory.versionNumber = conversationMessage.versionNumber;
				        conversationMessageChangeHistory.timeStamp = DateTime.UtcNow;
				        conversationMessageChangeHistory.userId = securityUser.id;
				        conversationMessageChangeHistory.tenantGuid = userTenantGuid;
				        conversationMessageChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage));
				        _context.ConversationMessageChangeHistories.Add(conversationMessageChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationMessage entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage)),
						null);

				return Ok(Database.ConversationMessage.CreateAnonymous(conversationMessage));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationMessage entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ConversationMessage record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessage", Name = "ConversationMessage")]
		public async Task<IActionResult> PostConversationMessage([FromBody]Database.ConversationMessage.ConversationMessageDTO conversationMessageDTO, CancellationToken cancellationToken = default)
		{
			if (conversationMessageDTO == null)
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
			// Create a new ConversationMessage object using the data from the DTO
			//
			Database.ConversationMessage conversationMessage = Database.ConversationMessage.FromDTO(conversationMessageDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				conversationMessage.tenantGuid = userTenantGuid;

				if (conversationMessage.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					conversationMessage.dateTimeCreated = conversationMessage.dateTimeCreated.ToUniversalTime();
				}

				if (conversationMessage.messageType != null && conversationMessage.messageType.Length > 50)
				{
					conversationMessage.messageType = conversationMessage.messageType.Substring(0, 50);
				}

				if (conversationMessage.entity != null && conversationMessage.entity.Length > 250)
				{
					conversationMessage.entity = conversationMessage.entity.Substring(0, 250);
				}

				if (conversationMessage.externalURL != null && conversationMessage.externalURL.Length > 1000)
				{
					conversationMessage.externalURL = conversationMessage.externalURL.Substring(0, 1000);
				}

				if (conversationMessage.scheduledDateTime.HasValue == true && conversationMessage.scheduledDateTime.Value.Kind != DateTimeKind.Utc)
				{
					conversationMessage.scheduledDateTime = conversationMessage.scheduledDateTime.Value.ToUniversalTime();
				}

				conversationMessage.objectGuid = Guid.NewGuid();
				conversationMessage.versionNumber = 1;

				_context.ConversationMessages.Add(conversationMessage);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the conversationMessage object so that no further changes will be written to the database
				    //
				    _context.Entry(conversationMessage).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					conversationMessage.ConversationMessageAttachments = null;
					conversationMessage.ConversationMessageChangeHistories = null;
					conversationMessage.ConversationMessageLinkPreviews = null;
					conversationMessage.ConversationMessageReactions = null;
					conversationMessage.ConversationMessageUsers = null;
					conversationMessage.ConversationPins = null;
					conversationMessage.ConversationThreadUsers = null;
					conversationMessage.InverseparentConversationMessage = null;
					conversationMessage.MessageBookmarks = null;
					conversationMessage.MessageFlags = null;
					conversationMessage.conversation = null;
					conversationMessage.conversationChannel = null;
					conversationMessage.parentConversationMessage = null;


				    ConversationMessageChangeHistory conversationMessageChangeHistory = new ConversationMessageChangeHistory();
				    conversationMessageChangeHistory.conversationMessageId = conversationMessage.id;
				    conversationMessageChangeHistory.versionNumber = conversationMessage.versionNumber;
				    conversationMessageChangeHistory.timeStamp = DateTime.UtcNow;
				    conversationMessageChangeHistory.userId = securityUser.id;
				    conversationMessageChangeHistory.tenantGuid = userTenantGuid;
				    conversationMessageChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage));
				    _context.ConversationMessageChangeHistories.Add(conversationMessageChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ConversationMessage entity successfully created.",
						true,
						conversationMessage. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ConversationMessage entity creation failed.", false, conversationMessage.id.ToString(), "", JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationMessage", conversationMessage.id, conversationMessage.messageType));

			return CreatedAtRoute("ConversationMessage", new { id = conversationMessage.id }, Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage));
		}



        /// <summary>
        /// 
        /// This rolls a ConversationMessage entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessage/Rollback/{id}")]
		[Route("api/ConversationMessage/Rollback")]
		public async Task<IActionResult> RollbackToConversationMessageVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ConversationMessage> query = (from x in _context.ConversationMessages
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ConversationMessage concurrently
			//
			lock (conversationMessagePutSyncRoot)
			{
				
				Database.ConversationMessage conversationMessage = query.FirstOrDefault();
				
				if (conversationMessage == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationMessage rollback", id.ToString(), new Exception("No Scheduler.ConversationMessage entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ConversationMessage current state so we can log it.
				//
				Database.ConversationMessage cloneOfExisting = (Database.ConversationMessage)_context.Entry(conversationMessage).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ConversationMessageAttachments = null;
				cloneOfExisting.ConversationMessageChangeHistories = null;
				cloneOfExisting.ConversationMessageLinkPreviews = null;
				cloneOfExisting.ConversationMessageReactions = null;
				cloneOfExisting.ConversationMessageUsers = null;
				cloneOfExisting.ConversationPins = null;
				cloneOfExisting.ConversationThreadUsers = null;
				cloneOfExisting.InverseparentConversationMessage = null;
				cloneOfExisting.MessageBookmarks = null;
				cloneOfExisting.MessageFlags = null;
				cloneOfExisting.conversation = null;
				cloneOfExisting.conversationChannel = null;
				cloneOfExisting.parentConversationMessage = null;

				if (versionNumber >= conversationMessage.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ConversationMessage rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ConversationMessage rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ConversationMessageChangeHistory conversationMessageChangeHistory = (from x in _context.ConversationMessageChangeHistories
				                                               where
				                                               x.conversationMessageId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (conversationMessageChangeHistory != null)
				{
				    Database.ConversationMessage oldConversationMessage = JsonSerializer.Deserialize<Database.ConversationMessage>(conversationMessageChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    conversationMessage.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    conversationMessage.conversationId = oldConversationMessage.conversationId;
				    conversationMessage.userId = oldConversationMessage.userId;
				    conversationMessage.parentConversationMessageId = oldConversationMessage.parentConversationMessageId;
				    conversationMessage.conversationChannelId = oldConversationMessage.conversationChannelId;
				    conversationMessage.dateTimeCreated = oldConversationMessage.dateTimeCreated;
				    conversationMessage.message = oldConversationMessage.message;
				    conversationMessage.messageType = oldConversationMessage.messageType;
				    conversationMessage.entity = oldConversationMessage.entity;
				    conversationMessage.entityId = oldConversationMessage.entityId;
				    conversationMessage.externalURL = oldConversationMessage.externalURL;
				    conversationMessage.forwardedFromMessageId = oldConversationMessage.forwardedFromMessageId;
				    conversationMessage.forwardedFromUserId = oldConversationMessage.forwardedFromUserId;
				    conversationMessage.isScheduled = oldConversationMessage.isScheduled;
				    conversationMessage.scheduledDateTime = oldConversationMessage.scheduledDateTime;
				    conversationMessage.objectGuid = oldConversationMessage.objectGuid;
				    conversationMessage.active = oldConversationMessage.active;
				    conversationMessage.deleted = oldConversationMessage.deleted;

				    string serializedConversationMessage = JsonSerializer.Serialize(conversationMessage);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ConversationMessageChangeHistory newConversationMessageChangeHistory = new ConversationMessageChangeHistory();
				        newConversationMessageChangeHistory.conversationMessageId = conversationMessage.id;
				        newConversationMessageChangeHistory.versionNumber = conversationMessage.versionNumber;
				        newConversationMessageChangeHistory.timeStamp = DateTime.UtcNow;
				        newConversationMessageChangeHistory.userId = securityUser.id;
				        newConversationMessageChangeHistory.tenantGuid = userTenantGuid;
				        newConversationMessageChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage));
				        _context.ConversationMessageChangeHistories.Add(newConversationMessageChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationMessage rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage)),
						null);


				    return Ok(Database.ConversationMessage.CreateAnonymous(conversationMessage));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ConversationMessage rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ConversationMessage rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ConversationMessage.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessage</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessage/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetConversationMessageChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ConversationMessage conversationMessage = await _context.ConversationMessages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessage == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessage.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationMessage> versionInfo = await conversationMessage.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ConversationMessage.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessage</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessage/{id}/AuditHistory")]
		public async Task<IActionResult> GetConversationMessageAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ConversationMessage conversationMessage = await _context.ConversationMessages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessage == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessage.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ConversationMessage>> versions = await conversationMessage.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ConversationMessage.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessage</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ConversationMessage object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessage/{id}/Version/{version}")]
		public async Task<IActionResult> GetConversationMessageVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ConversationMessage conversationMessage = await _context.ConversationMessages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessage == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessage.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationMessage> versionInfo = await conversationMessage.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ConversationMessage at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessage</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ConversationMessage object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessage/{id}/StateAtTime")]
		public async Task<IActionResult> GetConversationMessageStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ConversationMessage conversationMessage = await _context.ConversationMessages.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessage == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessage.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationMessage> versionInfo = await conversationMessage.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ConversationMessage record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessage/{id}")]
		[Route("api/ConversationMessage")]
		public async Task<IActionResult> DeleteConversationMessage(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ConversationMessage> query = (from x in _context.ConversationMessages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationMessage conversationMessage = await query.FirstOrDefaultAsync(cancellationToken);

			if (conversationMessage == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationMessage DELETE", id.ToString(), new Exception("No Scheduler.ConversationMessage entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ConversationMessage cloneOfExisting = (Database.ConversationMessage)_context.Entry(conversationMessage).GetDatabaseValues().ToObject();


			lock (conversationMessageDeleteSyncRoot)
			{
			    try
			    {
			        conversationMessage.deleted = true;
			        conversationMessage.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ConversationMessageChangeHistory conversationMessageChangeHistory = new ConversationMessageChangeHistory();
			        conversationMessageChangeHistory.conversationMessageId = conversationMessage.id;
			        conversationMessageChangeHistory.versionNumber = conversationMessage.versionNumber;
			        conversationMessageChangeHistory.timeStamp = DateTime.UtcNow;
			        conversationMessageChangeHistory.userId = securityUser.id;
			        conversationMessageChangeHistory.tenantGuid = userTenantGuid;
			        conversationMessageChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage));
			        _context.ConversationMessageChangeHistories.Add(conversationMessageChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ConversationMessage entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ConversationMessage entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessage.CreateAnonymousWithFirstLevelSubObjects(conversationMessage)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ConversationMessage records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ConversationMessages/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? conversationId = null,
			int? userId = null,
			int? parentConversationMessageId = null,
			int? conversationChannelId = null,
			DateTime? dateTimeCreated = null,
			string message = null,
			string messageType = null,
			string entity = null,
			int? entityId = null,
			string externalURL = null,
			int? forwardedFromMessageId = null,
			int? forwardedFromUserId = null,
			bool? isScheduled = null,
			DateTime? scheduledDateTime = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			if (scheduledDateTime.HasValue == true && scheduledDateTime.Value.Kind != DateTimeKind.Utc)
			{
				scheduledDateTime = scheduledDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationMessage> query = (from cm in _context.ConversationMessages select cm);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationId.HasValue == true)
			{
				query = query.Where(cm => cm.conversationId == conversationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cm => cm.userId == userId.Value);
			}
			if (parentConversationMessageId.HasValue == true)
			{
				query = query.Where(cm => cm.parentConversationMessageId == parentConversationMessageId.Value);
			}
			if (conversationChannelId.HasValue == true)
			{
				query = query.Where(cm => cm.conversationChannelId == conversationChannelId.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(cm => cm.dateTimeCreated == dateTimeCreated.Value);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(cm => cm.message == message);
			}
			if (string.IsNullOrEmpty(messageType) == false)
			{
				query = query.Where(cm => cm.messageType == messageType);
			}
			if (string.IsNullOrEmpty(entity) == false)
			{
				query = query.Where(cm => cm.entity == entity);
			}
			if (entityId.HasValue == true)
			{
				query = query.Where(cm => cm.entityId == entityId.Value);
			}
			if (string.IsNullOrEmpty(externalURL) == false)
			{
				query = query.Where(cm => cm.externalURL == externalURL);
			}
			if (forwardedFromMessageId.HasValue == true)
			{
				query = query.Where(cm => cm.forwardedFromMessageId == forwardedFromMessageId.Value);
			}
			if (forwardedFromUserId.HasValue == true)
			{
				query = query.Where(cm => cm.forwardedFromUserId == forwardedFromUserId.Value);
			}
			if (isScheduled.HasValue == true)
			{
				query = query.Where(cm => cm.isScheduled == isScheduled.Value);
			}
			if (scheduledDateTime.HasValue == true)
			{
				query = query.Where(cm => cm.scheduledDateTime == scheduledDateTime.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cm => cm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cm => cm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cm => cm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cm => cm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cm => cm.deleted == false);
				}
			}
			else
			{
				query = query.Where(cm => cm.active == true);
				query = query.Where(cm => cm.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Conversation Message, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.message.Contains(anyStringContains)
			       || x.messageType.Contains(anyStringContains)
			       || x.entity.Contains(anyStringContains)
			       || x.externalURL.Contains(anyStringContains)
			       || x.conversation.entity.Contains(anyStringContains)
			       || x.conversation.externalURL.Contains(anyStringContains)
			       || x.conversation.name.Contains(anyStringContains)
			       || x.conversation.description.Contains(anyStringContains)
			       || x.conversationChannel.name.Contains(anyStringContains)
			       || x.conversationChannel.topic.Contains(anyStringContains)
			       || x.parentConversationMessage.message.Contains(anyStringContains)
			       || x.parentConversationMessage.messageType.Contains(anyStringContains)
			       || x.parentConversationMessage.entity.Contains(anyStringContains)
			       || x.parentConversationMessage.externalURL.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.messageType).ThenBy(x => x.entity).ThenBy(x => x.externalURL);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ConversationMessage.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ConversationMessage/CreateAuditEvent")]
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
