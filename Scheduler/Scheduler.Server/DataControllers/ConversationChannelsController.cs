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
    /// This auto generated class provides the basic CRUD operations for the ConversationChannel entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ConversationChannel entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ConversationChannelsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object conversationChannelPutSyncRoot = new object();
		static object conversationChannelDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ConversationChannelsController> _logger;

		public ConversationChannelsController(SchedulerContext context, ILogger<ConversationChannelsController> logger) : base("Scheduler", "ConversationChannel")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ConversationChannels filtered by the parameters provided.
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
		[Route("api/ConversationChannels")]
		public async Task<IActionResult> GetConversationChannels(
			int? conversationId = null,
			string name = null,
			string topic = null,
			bool? isPrivate = null,
			bool? isPinned = null,
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

			IQueryable<Database.ConversationChannel> query = (from cc in _context.ConversationChannels select cc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationId.HasValue == true)
			{
				query = query.Where(cc => cc.conversationId == conversationId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cc => cc.name == name);
			}
			if (string.IsNullOrEmpty(topic) == false)
			{
				query = query.Where(cc => cc.topic == topic);
			}
			if (isPrivate.HasValue == true)
			{
				query = query.Where(cc => cc.isPrivate == isPrivate.Value);
			}
			if (isPinned.HasValue == true)
			{
				query = query.Where(cc => cc.isPinned == isPinned.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cc => cc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cc => cc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cc => cc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cc => cc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cc => cc.deleted == false);
				}
			}
			else
			{
				query = query.Where(cc => cc.active == true);
				query = query.Where(cc => cc.deleted == false);
			}

			query = query.OrderBy(cc => cc.name).ThenBy(cc => cc.topic);


			//
			// Add the any string contains parameter to span all the string fields on the Conversation Channel, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.topic.Contains(anyStringContains)
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
			
			List<Database.ConversationChannel> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ConversationChannel conversationChannel in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(conversationChannel, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ConversationChannel Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ConversationChannel Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ConversationChannels filtered by the parameters provided.  Its query is similar to the GetConversationChannels method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationChannels/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? conversationId = null,
			string name = null,
			string topic = null,
			bool? isPrivate = null,
			bool? isPinned = null,
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


			IQueryable<Database.ConversationChannel> query = (from cc in _context.ConversationChannels select cc);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (conversationId.HasValue == true)
			{
				query = query.Where(cc => cc.conversationId == conversationId.Value);
			}
			if (name != null)
			{
				query = query.Where(cc => cc.name == name);
			}
			if (topic != null)
			{
				query = query.Where(cc => cc.topic == topic);
			}
			if (isPrivate.HasValue == true)
			{
				query = query.Where(cc => cc.isPrivate == isPrivate.Value);
			}
			if (isPinned.HasValue == true)
			{
				query = query.Where(cc => cc.isPinned == isPinned.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cc => cc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cc => cc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cc => cc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cc => cc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cc => cc.deleted == false);
				}
			}
			else
			{
				query = query.Where(cc => cc.active == true);
				query = query.Where(cc => cc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Conversation Channel, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.topic.Contains(anyStringContains)
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
        /// This gets a single ConversationChannel by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationChannel/{id}")]
		public async Task<IActionResult> GetConversationChannel(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ConversationChannel> query = (from cc in _context.ConversationChannels where
							(cc.id == id) &&
							(userIsAdmin == true || cc.deleted == false) &&
							(userIsWriter == true || cc.active == true)
					select cc);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.conversation);
					query = query.AsSplitQuery();
				}

				Database.ConversationChannel materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ConversationChannel Entity was read with Admin privilege." : "Scheduler.ConversationChannel Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationChannel", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ConversationChannel entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ConversationChannel.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ConversationChannel.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ConversationChannel record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ConversationChannel/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutConversationChannel(int id, [FromBody]Database.ConversationChannel.ConversationChannelDTO conversationChannelDTO, CancellationToken cancellationToken = default)
		{
			if (conversationChannelDTO == null)
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



			if (id != conversationChannelDTO.id)
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


			IQueryable<Database.ConversationChannel> query = (from x in _context.ConversationChannels
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationChannel existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationChannel PUT", id.ToString(), new Exception("No Scheduler.ConversationChannel entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (conversationChannelDTO.objectGuid == Guid.Empty)
            {
                conversationChannelDTO.objectGuid = existing.objectGuid;
            }
            else if (conversationChannelDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ConversationChannel record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ConversationChannel cloneOfExisting = (Database.ConversationChannel)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ConversationChannel object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ConversationChannel conversationChannel = (Database.ConversationChannel)_context.Entry(existing).GetDatabaseValues().ToObject();
			conversationChannel.ApplyDTO(conversationChannelDTO);
			//
			// The tenant guid for any ConversationChannel being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ConversationChannel because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				conversationChannel.tenantGuid = existing.tenantGuid;
			}

			lock (conversationChannelPutSyncRoot)
			{
				//
				// Validate the version number for the conversationChannel being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != conversationChannel.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ConversationChannel save attempt was made but save request was with version " + conversationChannel.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ConversationChannel you are trying to update has already changed.  Please try your save again after reloading the ConversationChannel.");
				}
				else
				{
					// Same record.  Increase version.
					conversationChannel.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (conversationChannel.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ConversationChannel record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (conversationChannel.name != null && conversationChannel.name.Length > 250)
				{
					conversationChannel.name = conversationChannel.name.Substring(0, 250);
				}

				if (conversationChannel.topic != null && conversationChannel.topic.Length > 1000)
				{
					conversationChannel.topic = conversationChannel.topic.Substring(0, 1000);
				}

				try
				{
				    EntityEntry<Database.ConversationChannel> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(conversationChannel);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ConversationChannelChangeHistory conversationChannelChangeHistory = new ConversationChannelChangeHistory();
				        conversationChannelChangeHistory.conversationChannelId = conversationChannel.id;
				        conversationChannelChangeHistory.versionNumber = conversationChannel.versionNumber;
				        conversationChannelChangeHistory.timeStamp = DateTime.UtcNow;
				        conversationChannelChangeHistory.userId = securityUser.id;
				        conversationChannelChangeHistory.tenantGuid = userTenantGuid;
				        conversationChannelChangeHistory.data = JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel));
				        _context.ConversationChannelChangeHistories.Add(conversationChannelChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationChannel entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel)),
						null);

				return Ok(Database.ConversationChannel.CreateAnonymous(conversationChannel));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationChannel entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ConversationChannel record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationChannel", Name = "ConversationChannel")]
		public async Task<IActionResult> PostConversationChannel([FromBody]Database.ConversationChannel.ConversationChannelDTO conversationChannelDTO, CancellationToken cancellationToken = default)
		{
			if (conversationChannelDTO == null)
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
			// Create a new ConversationChannel object using the data from the DTO
			//
			Database.ConversationChannel conversationChannel = Database.ConversationChannel.FromDTO(conversationChannelDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				conversationChannel.tenantGuid = userTenantGuid;

				if (conversationChannel.name != null && conversationChannel.name.Length > 250)
				{
					conversationChannel.name = conversationChannel.name.Substring(0, 250);
				}

				if (conversationChannel.topic != null && conversationChannel.topic.Length > 1000)
				{
					conversationChannel.topic = conversationChannel.topic.Substring(0, 1000);
				}

				conversationChannel.objectGuid = Guid.NewGuid();
				conversationChannel.versionNumber = 1;

				_context.ConversationChannels.Add(conversationChannel);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the conversationChannel object so that no further changes will be written to the database
				    //
				    _context.Entry(conversationChannel).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					conversationChannel.ConversationChannelChangeHistories = null;
					conversationChannel.ConversationMessages = null;
					conversationChannel.conversation = null;


				    ConversationChannelChangeHistory conversationChannelChangeHistory = new ConversationChannelChangeHistory();
				    conversationChannelChangeHistory.conversationChannelId = conversationChannel.id;
				    conversationChannelChangeHistory.versionNumber = conversationChannel.versionNumber;
				    conversationChannelChangeHistory.timeStamp = DateTime.UtcNow;
				    conversationChannelChangeHistory.userId = securityUser.id;
				    conversationChannelChangeHistory.tenantGuid = userTenantGuid;
				    conversationChannelChangeHistory.data = JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel));
				    _context.ConversationChannelChangeHistories.Add(conversationChannelChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ConversationChannel entity successfully created.",
						true,
						conversationChannel. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ConversationChannel entity creation failed.", false, conversationChannel.id.ToString(), "", JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationChannel", conversationChannel.id, conversationChannel.name));

			return CreatedAtRoute("ConversationChannel", new { id = conversationChannel.id }, Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel));
		}



        /// <summary>
        /// 
        /// This rolls a ConversationChannel entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationChannel/Rollback/{id}")]
		[Route("api/ConversationChannel/Rollback")]
		public async Task<IActionResult> RollbackToConversationChannelVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ConversationChannel> query = (from x in _context.ConversationChannels
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ConversationChannel concurrently
			//
			lock (conversationChannelPutSyncRoot)
			{
				
				Database.ConversationChannel conversationChannel = query.FirstOrDefault();
				
				if (conversationChannel == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationChannel rollback", id.ToString(), new Exception("No Scheduler.ConversationChannel entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ConversationChannel current state so we can log it.
				//
				Database.ConversationChannel cloneOfExisting = (Database.ConversationChannel)_context.Entry(conversationChannel).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ConversationChannelChangeHistories = null;
				cloneOfExisting.ConversationMessages = null;
				cloneOfExisting.conversation = null;

				if (versionNumber >= conversationChannel.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ConversationChannel rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ConversationChannel rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ConversationChannelChangeHistory conversationChannelChangeHistory = (from x in _context.ConversationChannelChangeHistories
				                                               where
				                                               x.conversationChannelId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (conversationChannelChangeHistory != null)
				{
				    Database.ConversationChannel oldConversationChannel = JsonSerializer.Deserialize<Database.ConversationChannel>(conversationChannelChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    conversationChannel.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    conversationChannel.conversationId = oldConversationChannel.conversationId;
				    conversationChannel.name = oldConversationChannel.name;
				    conversationChannel.topic = oldConversationChannel.topic;
				    conversationChannel.isPrivate = oldConversationChannel.isPrivate;
				    conversationChannel.isPinned = oldConversationChannel.isPinned;
				    conversationChannel.objectGuid = oldConversationChannel.objectGuid;
				    conversationChannel.active = oldConversationChannel.active;
				    conversationChannel.deleted = oldConversationChannel.deleted;

				    string serializedConversationChannel = JsonSerializer.Serialize(conversationChannel);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ConversationChannelChangeHistory newConversationChannelChangeHistory = new ConversationChannelChangeHistory();
				        newConversationChannelChangeHistory.conversationChannelId = conversationChannel.id;
				        newConversationChannelChangeHistory.versionNumber = conversationChannel.versionNumber;
				        newConversationChannelChangeHistory.timeStamp = DateTime.UtcNow;
				        newConversationChannelChangeHistory.userId = securityUser.id;
				        newConversationChannelChangeHistory.tenantGuid = userTenantGuid;
				        newConversationChannelChangeHistory.data = JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel));
				        _context.ConversationChannelChangeHistories.Add(newConversationChannelChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationChannel rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel)),
						null);


				    return Ok(Database.ConversationChannel.CreateAnonymous(conversationChannel));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ConversationChannel rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ConversationChannel rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ConversationChannel.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationChannel</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationChannel/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetConversationChannelChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ConversationChannel conversationChannel = await _context.ConversationChannels.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationChannel == null)
			{
				return NotFound();
			}

			try
			{
				conversationChannel.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationChannel> versionInfo = await conversationChannel.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ConversationChannel.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationChannel</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationChannel/{id}/AuditHistory")]
		public async Task<IActionResult> GetConversationChannelAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ConversationChannel conversationChannel = await _context.ConversationChannels.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationChannel == null)
			{
				return NotFound();
			}

			try
			{
				conversationChannel.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ConversationChannel>> versions = await conversationChannel.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ConversationChannel.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationChannel</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ConversationChannel object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationChannel/{id}/Version/{version}")]
		public async Task<IActionResult> GetConversationChannelVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ConversationChannel conversationChannel = await _context.ConversationChannels.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationChannel == null)
			{
				return NotFound();
			}

			try
			{
				conversationChannel.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationChannel> versionInfo = await conversationChannel.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ConversationChannel at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationChannel</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ConversationChannel object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationChannel/{id}/StateAtTime")]
		public async Task<IActionResult> GetConversationChannelStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ConversationChannel conversationChannel = await _context.ConversationChannels.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationChannel == null)
			{
				return NotFound();
			}

			try
			{
				conversationChannel.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationChannel> versionInfo = await conversationChannel.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ConversationChannel record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationChannel/{id}")]
		[Route("api/ConversationChannel")]
		public async Task<IActionResult> DeleteConversationChannel(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ConversationChannel> query = (from x in _context.ConversationChannels
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationChannel conversationChannel = await query.FirstOrDefaultAsync(cancellationToken);

			if (conversationChannel == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationChannel DELETE", id.ToString(), new Exception("No Scheduler.ConversationChannel entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ConversationChannel cloneOfExisting = (Database.ConversationChannel)_context.Entry(conversationChannel).GetDatabaseValues().ToObject();


			lock (conversationChannelDeleteSyncRoot)
			{
			    try
			    {
			        conversationChannel.deleted = true;
			        conversationChannel.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ConversationChannelChangeHistory conversationChannelChangeHistory = new ConversationChannelChangeHistory();
			        conversationChannelChangeHistory.conversationChannelId = conversationChannel.id;
			        conversationChannelChangeHistory.versionNumber = conversationChannel.versionNumber;
			        conversationChannelChangeHistory.timeStamp = DateTime.UtcNow;
			        conversationChannelChangeHistory.userId = securityUser.id;
			        conversationChannelChangeHistory.tenantGuid = userTenantGuid;
			        conversationChannelChangeHistory.data = JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel));
			        _context.ConversationChannelChangeHistories.Add(conversationChannelChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ConversationChannel entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ConversationChannel entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationChannel.CreateAnonymousWithFirstLevelSubObjects(conversationChannel)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ConversationChannel records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ConversationChannels/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? conversationId = null,
			string name = null,
			string topic = null,
			bool? isPrivate = null,
			bool? isPinned = null,
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

			IQueryable<Database.ConversationChannel> query = (from cc in _context.ConversationChannels select cc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationId.HasValue == true)
			{
				query = query.Where(cc => cc.conversationId == conversationId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cc => cc.name == name);
			}
			if (string.IsNullOrEmpty(topic) == false)
			{
				query = query.Where(cc => cc.topic == topic);
			}
			if (isPrivate.HasValue == true)
			{
				query = query.Where(cc => cc.isPrivate == isPrivate.Value);
			}
			if (isPinned.HasValue == true)
			{
				query = query.Where(cc => cc.isPinned == isPinned.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cc => cc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cc => cc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cc => cc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cc => cc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cc => cc.deleted == false);
				}
			}
			else
			{
				query = query.Where(cc => cc.active == true);
				query = query.Where(cc => cc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Conversation Channel, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.topic.Contains(anyStringContains)
			       || x.conversation.entity.Contains(anyStringContains)
			       || x.conversation.externalURL.Contains(anyStringContains)
			       || x.conversation.name.Contains(anyStringContains)
			       || x.conversation.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.topic);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ConversationChannel.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ConversationChannel/CreateAuditEvent")]
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
