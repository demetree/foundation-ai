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
    /// This auto generated class provides the basic CRUD operations for the MessageFlag entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MessageFlag entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MessageFlagsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<MessageFlagsController> _logger;

		public MessageFlagsController(SchedulerContext context, ILogger<MessageFlagsController> logger) : base("Scheduler", "MessageFlag")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MessageFlags filtered by the parameters provided.
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
		[Route("api/MessageFlags")]
		public async Task<IActionResult> GetMessageFlags(
			int? conversationMessageId = null,
			int? flaggedByUserId = null,
			string reason = null,
			string details = null,
			string status = null,
			int? reviewedByUserId = null,
			DateTime? dateTimeReviewed = null,
			string resolutionNotes = null,
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
			if (dateTimeReviewed.HasValue == true && dateTimeReviewed.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeReviewed = dateTimeReviewed.Value.ToUniversalTime();
			}

			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.MessageFlag> query = (from mf in _context.MessageFlags select mf);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(mf => mf.conversationMessageId == conversationMessageId.Value);
			}
			if (flaggedByUserId.HasValue == true)
			{
				query = query.Where(mf => mf.flaggedByUserId == flaggedByUserId.Value);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(mf => mf.reason == reason);
			}
			if (string.IsNullOrEmpty(details) == false)
			{
				query = query.Where(mf => mf.details == details);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(mf => mf.status == status);
			}
			if (reviewedByUserId.HasValue == true)
			{
				query = query.Where(mf => mf.reviewedByUserId == reviewedByUserId.Value);
			}
			if (dateTimeReviewed.HasValue == true)
			{
				query = query.Where(mf => mf.dateTimeReviewed == dateTimeReviewed.Value);
			}
			if (string.IsNullOrEmpty(resolutionNotes) == false)
			{
				query = query.Where(mf => mf.resolutionNotes == resolutionNotes);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(mf => mf.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mf => mf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mf => mf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mf => mf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mf => mf.deleted == false);
				}
			}
			else
			{
				query = query.Where(mf => mf.active == true);
				query = query.Where(mf => mf.deleted == false);
			}

			query = query.OrderBy(mf => mf.reason).ThenBy(mf => mf.details).ThenBy(mf => mf.status);


			//
			// Add the any string contains parameter to span all the string fields on the Message Flag, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.details.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.resolutionNotes.Contains(anyStringContains)
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
			
			List<Database.MessageFlag> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MessageFlag messageFlag in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(messageFlag, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.MessageFlag Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.MessageFlag Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MessageFlags filtered by the parameters provided.  Its query is similar to the GetMessageFlags method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessageFlags/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? conversationMessageId = null,
			int? flaggedByUserId = null,
			string reason = null,
			string details = null,
			string status = null,
			int? reviewedByUserId = null,
			DateTime? dateTimeReviewed = null,
			string resolutionNotes = null,
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
			if (dateTimeReviewed.HasValue == true && dateTimeReviewed.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeReviewed = dateTimeReviewed.Value.ToUniversalTime();
			}

			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.MessageFlag> query = (from mf in _context.MessageFlags select mf);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(mf => mf.conversationMessageId == conversationMessageId.Value);
			}
			if (flaggedByUserId.HasValue == true)
			{
				query = query.Where(mf => mf.flaggedByUserId == flaggedByUserId.Value);
			}
			if (reason != null)
			{
				query = query.Where(mf => mf.reason == reason);
			}
			if (details != null)
			{
				query = query.Where(mf => mf.details == details);
			}
			if (status != null)
			{
				query = query.Where(mf => mf.status == status);
			}
			if (reviewedByUserId.HasValue == true)
			{
				query = query.Where(mf => mf.reviewedByUserId == reviewedByUserId.Value);
			}
			if (dateTimeReviewed.HasValue == true)
			{
				query = query.Where(mf => mf.dateTimeReviewed == dateTimeReviewed.Value);
			}
			if (resolutionNotes != null)
			{
				query = query.Where(mf => mf.resolutionNotes == resolutionNotes);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(mf => mf.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mf => mf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mf => mf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mf => mf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mf => mf.deleted == false);
				}
			}
			else
			{
				query = query.Where(mf => mf.active == true);
				query = query.Where(mf => mf.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Message Flag, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.details.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.resolutionNotes.Contains(anyStringContains)
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
        /// This gets a single MessageFlag by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessageFlag/{id}")]
		public async Task<IActionResult> GetMessageFlag(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.MessageFlag> query = (from mf in _context.MessageFlags where
							(mf.id == id) &&
							(userIsAdmin == true || mf.deleted == false) &&
							(userIsWriter == true || mf.active == true)
					select mf);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.conversationMessage);
					query = query.AsSplitQuery();
				}

				Database.MessageFlag materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.MessageFlag Entity was read with Admin privilege." : "Scheduler.MessageFlag Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MessageFlag", materialized.id, materialized.reason));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.MessageFlag entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.MessageFlag.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.MessageFlag.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MessageFlag record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MessageFlag/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMessageFlag(int id, [FromBody]Database.MessageFlag.MessageFlagDTO messageFlagDTO, CancellationToken cancellationToken = default)
		{
			if (messageFlagDTO == null)
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



			if (id != messageFlagDTO.id)
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


			IQueryable<Database.MessageFlag> query = (from x in _context.MessageFlags
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MessageFlag existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.MessageFlag PUT", id.ToString(), new Exception("No Scheduler.MessageFlag entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (messageFlagDTO.objectGuid == Guid.Empty)
            {
                messageFlagDTO.objectGuid = existing.objectGuid;
            }
            else if (messageFlagDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MessageFlag record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MessageFlag cloneOfExisting = (Database.MessageFlag)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MessageFlag object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MessageFlag messageFlag = (Database.MessageFlag)_context.Entry(existing).GetDatabaseValues().ToObject();
			messageFlag.ApplyDTO(messageFlagDTO);
			//
			// The tenant guid for any MessageFlag being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the MessageFlag because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				messageFlag.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (messageFlag.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.MessageFlag record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (messageFlag.reason != null && messageFlag.reason.Length > 100)
			{
				messageFlag.reason = messageFlag.reason.Substring(0, 100);
			}

			if (messageFlag.details != null && messageFlag.details.Length > 1000)
			{
				messageFlag.details = messageFlag.details.Substring(0, 1000);
			}

			if (messageFlag.status != null && messageFlag.status.Length > 50)
			{
				messageFlag.status = messageFlag.status.Substring(0, 50);
			}

			if (messageFlag.dateTimeReviewed.HasValue == true && messageFlag.dateTimeReviewed.Value.Kind != DateTimeKind.Utc)
			{
				messageFlag.dateTimeReviewed = messageFlag.dateTimeReviewed.Value.ToUniversalTime();
			}

			if (messageFlag.resolutionNotes != null && messageFlag.resolutionNotes.Length > 1000)
			{
				messageFlag.resolutionNotes = messageFlag.resolutionNotes.Substring(0, 1000);
			}

			if (messageFlag.dateTimeCreated.Kind != DateTimeKind.Utc)
			{
				messageFlag.dateTimeCreated = messageFlag.dateTimeCreated.ToUniversalTime();
			}

			EntityEntry<Database.MessageFlag> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(messageFlag);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.MessageFlag entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessageFlag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessageFlag.CreateAnonymousWithFirstLevelSubObjects(messageFlag)),
					null);


				return Ok(Database.MessageFlag.CreateAnonymous(messageFlag));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.MessageFlag entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessageFlag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessageFlag.CreateAnonymousWithFirstLevelSubObjects(messageFlag)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MessageFlag record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessageFlag", Name = "MessageFlag")]
		public async Task<IActionResult> PostMessageFlag([FromBody]Database.MessageFlag.MessageFlagDTO messageFlagDTO, CancellationToken cancellationToken = default)
		{
			if (messageFlagDTO == null)
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
			// Create a new MessageFlag object using the data from the DTO
			//
			Database.MessageFlag messageFlag = Database.MessageFlag.FromDTO(messageFlagDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				messageFlag.tenantGuid = userTenantGuid;

				if (messageFlag.reason != null && messageFlag.reason.Length > 100)
				{
					messageFlag.reason = messageFlag.reason.Substring(0, 100);
				}

				if (messageFlag.details != null && messageFlag.details.Length > 1000)
				{
					messageFlag.details = messageFlag.details.Substring(0, 1000);
				}

				if (messageFlag.status != null && messageFlag.status.Length > 50)
				{
					messageFlag.status = messageFlag.status.Substring(0, 50);
				}

				if (messageFlag.dateTimeReviewed.HasValue == true && messageFlag.dateTimeReviewed.Value.Kind != DateTimeKind.Utc)
				{
					messageFlag.dateTimeReviewed = messageFlag.dateTimeReviewed.Value.ToUniversalTime();
				}

				if (messageFlag.resolutionNotes != null && messageFlag.resolutionNotes.Length > 1000)
				{
					messageFlag.resolutionNotes = messageFlag.resolutionNotes.Substring(0, 1000);
				}

				if (messageFlag.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					messageFlag.dateTimeCreated = messageFlag.dateTimeCreated.ToUniversalTime();
				}

				messageFlag.objectGuid = Guid.NewGuid();
				_context.MessageFlags.Add(messageFlag);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.MessageFlag entity successfully created.",
					true,
					messageFlag.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MessageFlag.CreateAnonymousWithFirstLevelSubObjects(messageFlag)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.MessageFlag entity creation failed.", false, messageFlag.id.ToString(), "", JsonSerializer.Serialize(messageFlag), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MessageFlag", messageFlag.id, messageFlag.reason));

			return CreatedAtRoute("MessageFlag", new { id = messageFlag.id }, Database.MessageFlag.CreateAnonymousWithFirstLevelSubObjects(messageFlag));
		}



        /// <summary>
        /// 
        /// This deletes a MessageFlag record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MessageFlag/{id}")]
		[Route("api/MessageFlag")]
		public async Task<IActionResult> DeleteMessageFlag(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.MessageFlag> query = (from x in _context.MessageFlags
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MessageFlag messageFlag = await query.FirstOrDefaultAsync(cancellationToken);

			if (messageFlag == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.MessageFlag DELETE", id.ToString(), new Exception("No Scheduler.MessageFlag entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MessageFlag cloneOfExisting = (Database.MessageFlag)_context.Entry(messageFlag).GetDatabaseValues().ToObject();


			try
			{
				messageFlag.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.MessageFlag entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessageFlag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessageFlag.CreateAnonymousWithFirstLevelSubObjects(messageFlag)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.MessageFlag entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MessageFlag.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MessageFlag.CreateAnonymousWithFirstLevelSubObjects(messageFlag)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MessageFlag records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MessageFlags/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? conversationMessageId = null,
			int? flaggedByUserId = null,
			string reason = null,
			string details = null,
			string status = null,
			int? reviewedByUserId = null,
			DateTime? dateTimeReviewed = null,
			string resolutionNotes = null,
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
			if (dateTimeReviewed.HasValue == true && dateTimeReviewed.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeReviewed = dateTimeReviewed.Value.ToUniversalTime();
			}

			if (dateTimeCreated.HasValue == true && dateTimeCreated.Value.Kind != DateTimeKind.Utc)
			{
				dateTimeCreated = dateTimeCreated.Value.ToUniversalTime();
			}

			IQueryable<Database.MessageFlag> query = (from mf in _context.MessageFlags select mf);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(mf => mf.conversationMessageId == conversationMessageId.Value);
			}
			if (flaggedByUserId.HasValue == true)
			{
				query = query.Where(mf => mf.flaggedByUserId == flaggedByUserId.Value);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(mf => mf.reason == reason);
			}
			if (string.IsNullOrEmpty(details) == false)
			{
				query = query.Where(mf => mf.details == details);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(mf => mf.status == status);
			}
			if (reviewedByUserId.HasValue == true)
			{
				query = query.Where(mf => mf.reviewedByUserId == reviewedByUserId.Value);
			}
			if (dateTimeReviewed.HasValue == true)
			{
				query = query.Where(mf => mf.dateTimeReviewed == dateTimeReviewed.Value);
			}
			if (string.IsNullOrEmpty(resolutionNotes) == false)
			{
				query = query.Where(mf => mf.resolutionNotes == resolutionNotes);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(mf => mf.dateTimeCreated == dateTimeCreated.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mf => mf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mf => mf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mf => mf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mf => mf.deleted == false);
				}
			}
			else
			{
				query = query.Where(mf => mf.active == true);
				query = query.Where(mf => mf.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Message Flag, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reason.Contains(anyStringContains)
			       || x.details.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.resolutionNotes.Contains(anyStringContains)
			       || x.conversationMessage.message.Contains(anyStringContains)
			       || x.conversationMessage.messageType.Contains(anyStringContains)
			       || x.conversationMessage.entity.Contains(anyStringContains)
			       || x.conversationMessage.externalURL.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.reason).ThenBy(x => x.details).ThenBy(x => x.status);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MessageFlag.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MessageFlag/CreateAuditEvent")]
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
