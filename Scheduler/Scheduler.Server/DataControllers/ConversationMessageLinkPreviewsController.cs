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
    /// This auto generated class provides the basic CRUD operations for the ConversationMessageLinkPreview entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ConversationMessageLinkPreview entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ConversationMessageLinkPreviewsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object conversationMessageLinkPreviewPutSyncRoot = new object();
		static object conversationMessageLinkPreviewDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ConversationMessageLinkPreviewsController> _logger;

		public ConversationMessageLinkPreviewsController(SchedulerContext context, ILogger<ConversationMessageLinkPreviewsController> logger) : base("Scheduler", "ConversationMessageLinkPreview")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ConversationMessageLinkPreviews filtered by the parameters provided.
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
		[Route("api/ConversationMessageLinkPreviews")]
		public async Task<IActionResult> GetConversationMessageLinkPreviews(
			int? conversationMessageId = null,
			string url = null,
			string title = null,
			string description = null,
			string imageUrl = null,
			string siteName = null,
			DateTime? fetchedDateTime = null,
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
			if (fetchedDateTime.HasValue == true && fetchedDateTime.Value.Kind != DateTimeKind.Utc)
			{
				fetchedDateTime = fetchedDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationMessageLinkPreview> query = (from cmlp in _context.ConversationMessageLinkPreviews select cmlp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.conversationMessageId == conversationMessageId.Value);
			}
			if (string.IsNullOrEmpty(url) == false)
			{
				query = query.Where(cmlp => cmlp.url == url);
			}
			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(cmlp => cmlp.title == title);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cmlp => cmlp.description == description);
			}
			if (string.IsNullOrEmpty(imageUrl) == false)
			{
				query = query.Where(cmlp => cmlp.imageUrl == imageUrl);
			}
			if (string.IsNullOrEmpty(siteName) == false)
			{
				query = query.Where(cmlp => cmlp.siteName == siteName);
			}
			if (fetchedDateTime.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.fetchedDateTime == fetchedDateTime.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cmlp => cmlp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cmlp => cmlp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cmlp => cmlp.deleted == false);
				}
			}
			else
			{
				query = query.Where(cmlp => cmlp.active == true);
				query = query.Where(cmlp => cmlp.deleted == false);
			}

			query = query.OrderBy(cmlp => cmlp.url).ThenBy(cmlp => cmlp.title).ThenBy(cmlp => cmlp.description);


			//
			// Add the any string contains parameter to span all the string fields on the Conversation Message Link Preview, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.url.Contains(anyStringContains)
			       || x.title.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.imageUrl.Contains(anyStringContains)
			       || x.siteName.Contains(anyStringContains)
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
			
			List<Database.ConversationMessageLinkPreview> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ConversationMessageLinkPreview conversationMessageLinkPreview in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(conversationMessageLinkPreview, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ConversationMessageLinkPreview Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ConversationMessageLinkPreview Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ConversationMessageLinkPreviews filtered by the parameters provided.  Its query is similar to the GetConversationMessageLinkPreviews method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageLinkPreviews/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? conversationMessageId = null,
			string url = null,
			string title = null,
			string description = null,
			string imageUrl = null,
			string siteName = null,
			DateTime? fetchedDateTime = null,
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
			if (fetchedDateTime.HasValue == true && fetchedDateTime.Value.Kind != DateTimeKind.Utc)
			{
				fetchedDateTime = fetchedDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationMessageLinkPreview> query = (from cmlp in _context.ConversationMessageLinkPreviews select cmlp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.conversationMessageId == conversationMessageId.Value);
			}
			if (url != null)
			{
				query = query.Where(cmlp => cmlp.url == url);
			}
			if (title != null)
			{
				query = query.Where(cmlp => cmlp.title == title);
			}
			if (description != null)
			{
				query = query.Where(cmlp => cmlp.description == description);
			}
			if (imageUrl != null)
			{
				query = query.Where(cmlp => cmlp.imageUrl == imageUrl);
			}
			if (siteName != null)
			{
				query = query.Where(cmlp => cmlp.siteName == siteName);
			}
			if (fetchedDateTime.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.fetchedDateTime == fetchedDateTime.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cmlp => cmlp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cmlp => cmlp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cmlp => cmlp.deleted == false);
				}
			}
			else
			{
				query = query.Where(cmlp => cmlp.active == true);
				query = query.Where(cmlp => cmlp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Conversation Message Link Preview, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.url.Contains(anyStringContains)
			       || x.title.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.imageUrl.Contains(anyStringContains)
			       || x.siteName.Contains(anyStringContains)
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
        /// This gets a single ConversationMessageLinkPreview by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageLinkPreview/{id}")]
		public async Task<IActionResult> GetConversationMessageLinkPreview(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ConversationMessageLinkPreview> query = (from cmlp in _context.ConversationMessageLinkPreviews where
							(cmlp.id == id) &&
							(userIsAdmin == true || cmlp.deleted == false) &&
							(userIsWriter == true || cmlp.active == true)
					select cmlp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.conversationMessage);
					query = query.AsSplitQuery();
				}

				Database.ConversationMessageLinkPreview materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ConversationMessageLinkPreview Entity was read with Admin privilege." : "Scheduler.ConversationMessageLinkPreview Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationMessageLinkPreview", materialized.id, materialized.url));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ConversationMessageLinkPreview entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ConversationMessageLinkPreview.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ConversationMessageLinkPreview.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ConversationMessageLinkPreview record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ConversationMessageLinkPreview/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutConversationMessageLinkPreview(int id, [FromBody]Database.ConversationMessageLinkPreview.ConversationMessageLinkPreviewDTO conversationMessageLinkPreviewDTO, CancellationToken cancellationToken = default)
		{
			if (conversationMessageLinkPreviewDTO == null)
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



			if (id != conversationMessageLinkPreviewDTO.id)
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


			IQueryable<Database.ConversationMessageLinkPreview> query = (from x in _context.ConversationMessageLinkPreviews
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationMessageLinkPreview existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationMessageLinkPreview PUT", id.ToString(), new Exception("No Scheduler.ConversationMessageLinkPreview entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (conversationMessageLinkPreviewDTO.objectGuid == Guid.Empty)
            {
                conversationMessageLinkPreviewDTO.objectGuid = existing.objectGuid;
            }
            else if (conversationMessageLinkPreviewDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ConversationMessageLinkPreview record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ConversationMessageLinkPreview cloneOfExisting = (Database.ConversationMessageLinkPreview)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ConversationMessageLinkPreview object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ConversationMessageLinkPreview conversationMessageLinkPreview = (Database.ConversationMessageLinkPreview)_context.Entry(existing).GetDatabaseValues().ToObject();
			conversationMessageLinkPreview.ApplyDTO(conversationMessageLinkPreviewDTO);
			//
			// The tenant guid for any ConversationMessageLinkPreview being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ConversationMessageLinkPreview because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				conversationMessageLinkPreview.tenantGuid = existing.tenantGuid;
			}

			lock (conversationMessageLinkPreviewPutSyncRoot)
			{
				//
				// Validate the version number for the conversationMessageLinkPreview being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != conversationMessageLinkPreview.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ConversationMessageLinkPreview save attempt was made but save request was with version " + conversationMessageLinkPreview.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ConversationMessageLinkPreview you are trying to update has already changed.  Please try your save again after reloading the ConversationMessageLinkPreview.");
				}
				else
				{
					// Same record.  Increase version.
					conversationMessageLinkPreview.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (conversationMessageLinkPreview.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ConversationMessageLinkPreview record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (conversationMessageLinkPreview.url != null && conversationMessageLinkPreview.url.Length > 1000)
				{
					conversationMessageLinkPreview.url = conversationMessageLinkPreview.url.Substring(0, 1000);
				}

				if (conversationMessageLinkPreview.title != null && conversationMessageLinkPreview.title.Length > 500)
				{
					conversationMessageLinkPreview.title = conversationMessageLinkPreview.title.Substring(0, 500);
				}

				if (conversationMessageLinkPreview.description != null && conversationMessageLinkPreview.description.Length > 1000)
				{
					conversationMessageLinkPreview.description = conversationMessageLinkPreview.description.Substring(0, 1000);
				}

				if (conversationMessageLinkPreview.imageUrl != null && conversationMessageLinkPreview.imageUrl.Length > 1000)
				{
					conversationMessageLinkPreview.imageUrl = conversationMessageLinkPreview.imageUrl.Substring(0, 1000);
				}

				if (conversationMessageLinkPreview.siteName != null && conversationMessageLinkPreview.siteName.Length > 250)
				{
					conversationMessageLinkPreview.siteName = conversationMessageLinkPreview.siteName.Substring(0, 250);
				}

				if (conversationMessageLinkPreview.fetchedDateTime.Kind != DateTimeKind.Utc)
				{
					conversationMessageLinkPreview.fetchedDateTime = conversationMessageLinkPreview.fetchedDateTime.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.ConversationMessageLinkPreview> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(conversationMessageLinkPreview);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ConversationMessageLinkPreviewChangeHistory conversationMessageLinkPreviewChangeHistory = new ConversationMessageLinkPreviewChangeHistory();
				        conversationMessageLinkPreviewChangeHistory.conversationMessageLinkPreviewId = conversationMessageLinkPreview.id;
				        conversationMessageLinkPreviewChangeHistory.versionNumber = conversationMessageLinkPreview.versionNumber;
				        conversationMessageLinkPreviewChangeHistory.timeStamp = DateTime.UtcNow;
				        conversationMessageLinkPreviewChangeHistory.userId = securityUser.id;
				        conversationMessageLinkPreviewChangeHistory.tenantGuid = userTenantGuid;
				        conversationMessageLinkPreviewChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview));
				        _context.ConversationMessageLinkPreviewChangeHistories.Add(conversationMessageLinkPreviewChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationMessageLinkPreview entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview)),
						null);

				return Ok(Database.ConversationMessageLinkPreview.CreateAnonymous(conversationMessageLinkPreview));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationMessageLinkPreview entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ConversationMessageLinkPreview record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageLinkPreview", Name = "ConversationMessageLinkPreview")]
		public async Task<IActionResult> PostConversationMessageLinkPreview([FromBody]Database.ConversationMessageLinkPreview.ConversationMessageLinkPreviewDTO conversationMessageLinkPreviewDTO, CancellationToken cancellationToken = default)
		{
			if (conversationMessageLinkPreviewDTO == null)
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
			// Create a new ConversationMessageLinkPreview object using the data from the DTO
			//
			Database.ConversationMessageLinkPreview conversationMessageLinkPreview = Database.ConversationMessageLinkPreview.FromDTO(conversationMessageLinkPreviewDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				conversationMessageLinkPreview.tenantGuid = userTenantGuid;

				if (conversationMessageLinkPreview.url != null && conversationMessageLinkPreview.url.Length > 1000)
				{
					conversationMessageLinkPreview.url = conversationMessageLinkPreview.url.Substring(0, 1000);
				}

				if (conversationMessageLinkPreview.title != null && conversationMessageLinkPreview.title.Length > 500)
				{
					conversationMessageLinkPreview.title = conversationMessageLinkPreview.title.Substring(0, 500);
				}

				if (conversationMessageLinkPreview.description != null && conversationMessageLinkPreview.description.Length > 1000)
				{
					conversationMessageLinkPreview.description = conversationMessageLinkPreview.description.Substring(0, 1000);
				}

				if (conversationMessageLinkPreview.imageUrl != null && conversationMessageLinkPreview.imageUrl.Length > 1000)
				{
					conversationMessageLinkPreview.imageUrl = conversationMessageLinkPreview.imageUrl.Substring(0, 1000);
				}

				if (conversationMessageLinkPreview.siteName != null && conversationMessageLinkPreview.siteName.Length > 250)
				{
					conversationMessageLinkPreview.siteName = conversationMessageLinkPreview.siteName.Substring(0, 250);
				}

				if (conversationMessageLinkPreview.fetchedDateTime.Kind != DateTimeKind.Utc)
				{
					conversationMessageLinkPreview.fetchedDateTime = conversationMessageLinkPreview.fetchedDateTime.ToUniversalTime();
				}

				conversationMessageLinkPreview.objectGuid = Guid.NewGuid();
				conversationMessageLinkPreview.versionNumber = 1;

				_context.ConversationMessageLinkPreviews.Add(conversationMessageLinkPreview);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the conversationMessageLinkPreview object so that no further changes will be written to the database
				    //
				    _context.Entry(conversationMessageLinkPreview).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					conversationMessageLinkPreview.ConversationMessageLinkPreviewChangeHistories = null;
					conversationMessageLinkPreview.conversationMessage = null;


				    ConversationMessageLinkPreviewChangeHistory conversationMessageLinkPreviewChangeHistory = new ConversationMessageLinkPreviewChangeHistory();
				    conversationMessageLinkPreviewChangeHistory.conversationMessageLinkPreviewId = conversationMessageLinkPreview.id;
				    conversationMessageLinkPreviewChangeHistory.versionNumber = conversationMessageLinkPreview.versionNumber;
				    conversationMessageLinkPreviewChangeHistory.timeStamp = DateTime.UtcNow;
				    conversationMessageLinkPreviewChangeHistory.userId = securityUser.id;
				    conversationMessageLinkPreviewChangeHistory.tenantGuid = userTenantGuid;
				    conversationMessageLinkPreviewChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview));
				    _context.ConversationMessageLinkPreviewChangeHistories.Add(conversationMessageLinkPreviewChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ConversationMessageLinkPreview entity successfully created.",
						true,
						conversationMessageLinkPreview. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ConversationMessageLinkPreview entity creation failed.", false, conversationMessageLinkPreview.id.ToString(), "", JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationMessageLinkPreview", conversationMessageLinkPreview.id, conversationMessageLinkPreview.url));

			return CreatedAtRoute("ConversationMessageLinkPreview", new { id = conversationMessageLinkPreview.id }, Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview));
		}



        /// <summary>
        /// 
        /// This rolls a ConversationMessageLinkPreview entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageLinkPreview/Rollback/{id}")]
		[Route("api/ConversationMessageLinkPreview/Rollback")]
		public async Task<IActionResult> RollbackToConversationMessageLinkPreviewVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ConversationMessageLinkPreview> query = (from x in _context.ConversationMessageLinkPreviews
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ConversationMessageLinkPreview concurrently
			//
			lock (conversationMessageLinkPreviewPutSyncRoot)
			{
				
				Database.ConversationMessageLinkPreview conversationMessageLinkPreview = query.FirstOrDefault();
				
				if (conversationMessageLinkPreview == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationMessageLinkPreview rollback", id.ToString(), new Exception("No Scheduler.ConversationMessageLinkPreview entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ConversationMessageLinkPreview current state so we can log it.
				//
				Database.ConversationMessageLinkPreview cloneOfExisting = (Database.ConversationMessageLinkPreview)_context.Entry(conversationMessageLinkPreview).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ConversationMessageLinkPreviewChangeHistories = null;
				cloneOfExisting.conversationMessage = null;

				if (versionNumber >= conversationMessageLinkPreview.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ConversationMessageLinkPreview rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ConversationMessageLinkPreview rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ConversationMessageLinkPreviewChangeHistory conversationMessageLinkPreviewChangeHistory = (from x in _context.ConversationMessageLinkPreviewChangeHistories
				                                               where
				                                               x.conversationMessageLinkPreviewId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (conversationMessageLinkPreviewChangeHistory != null)
				{
				    Database.ConversationMessageLinkPreview oldConversationMessageLinkPreview = JsonSerializer.Deserialize<Database.ConversationMessageLinkPreview>(conversationMessageLinkPreviewChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    conversationMessageLinkPreview.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    conversationMessageLinkPreview.conversationMessageId = oldConversationMessageLinkPreview.conversationMessageId;
				    conversationMessageLinkPreview.url = oldConversationMessageLinkPreview.url;
				    conversationMessageLinkPreview.title = oldConversationMessageLinkPreview.title;
				    conversationMessageLinkPreview.description = oldConversationMessageLinkPreview.description;
				    conversationMessageLinkPreview.imageUrl = oldConversationMessageLinkPreview.imageUrl;
				    conversationMessageLinkPreview.siteName = oldConversationMessageLinkPreview.siteName;
				    conversationMessageLinkPreview.fetchedDateTime = oldConversationMessageLinkPreview.fetchedDateTime;
				    conversationMessageLinkPreview.objectGuid = oldConversationMessageLinkPreview.objectGuid;
				    conversationMessageLinkPreview.active = oldConversationMessageLinkPreview.active;
				    conversationMessageLinkPreview.deleted = oldConversationMessageLinkPreview.deleted;

				    string serializedConversationMessageLinkPreview = JsonSerializer.Serialize(conversationMessageLinkPreview);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ConversationMessageLinkPreviewChangeHistory newConversationMessageLinkPreviewChangeHistory = new ConversationMessageLinkPreviewChangeHistory();
				        newConversationMessageLinkPreviewChangeHistory.conversationMessageLinkPreviewId = conversationMessageLinkPreview.id;
				        newConversationMessageLinkPreviewChangeHistory.versionNumber = conversationMessageLinkPreview.versionNumber;
				        newConversationMessageLinkPreviewChangeHistory.timeStamp = DateTime.UtcNow;
				        newConversationMessageLinkPreviewChangeHistory.userId = securityUser.id;
				        newConversationMessageLinkPreviewChangeHistory.tenantGuid = userTenantGuid;
				        newConversationMessageLinkPreviewChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview));
				        _context.ConversationMessageLinkPreviewChangeHistories.Add(newConversationMessageLinkPreviewChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationMessageLinkPreview rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview)),
						null);


				    return Ok(Database.ConversationMessageLinkPreview.CreateAnonymous(conversationMessageLinkPreview));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ConversationMessageLinkPreview rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ConversationMessageLinkPreview rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ConversationMessageLinkPreview.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessageLinkPreview</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageLinkPreview/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetConversationMessageLinkPreviewChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ConversationMessageLinkPreview conversationMessageLinkPreview = await _context.ConversationMessageLinkPreviews.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessageLinkPreview == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessageLinkPreview.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationMessageLinkPreview> versionInfo = await conversationMessageLinkPreview.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ConversationMessageLinkPreview.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessageLinkPreview</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageLinkPreview/{id}/AuditHistory")]
		public async Task<IActionResult> GetConversationMessageLinkPreviewAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ConversationMessageLinkPreview conversationMessageLinkPreview = await _context.ConversationMessageLinkPreviews.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessageLinkPreview == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessageLinkPreview.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ConversationMessageLinkPreview>> versions = await conversationMessageLinkPreview.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ConversationMessageLinkPreview.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessageLinkPreview</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ConversationMessageLinkPreview object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageLinkPreview/{id}/Version/{version}")]
		public async Task<IActionResult> GetConversationMessageLinkPreviewVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ConversationMessageLinkPreview conversationMessageLinkPreview = await _context.ConversationMessageLinkPreviews.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessageLinkPreview == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessageLinkPreview.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationMessageLinkPreview> versionInfo = await conversationMessageLinkPreview.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ConversationMessageLinkPreview at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessageLinkPreview</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ConversationMessageLinkPreview object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageLinkPreview/{id}/StateAtTime")]
		public async Task<IActionResult> GetConversationMessageLinkPreviewStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ConversationMessageLinkPreview conversationMessageLinkPreview = await _context.ConversationMessageLinkPreviews.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessageLinkPreview == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessageLinkPreview.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationMessageLinkPreview> versionInfo = await conversationMessageLinkPreview.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ConversationMessageLinkPreview record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageLinkPreview/{id}")]
		[Route("api/ConversationMessageLinkPreview")]
		public async Task<IActionResult> DeleteConversationMessageLinkPreview(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ConversationMessageLinkPreview> query = (from x in _context.ConversationMessageLinkPreviews
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationMessageLinkPreview conversationMessageLinkPreview = await query.FirstOrDefaultAsync(cancellationToken);

			if (conversationMessageLinkPreview == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationMessageLinkPreview DELETE", id.ToString(), new Exception("No Scheduler.ConversationMessageLinkPreview entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ConversationMessageLinkPreview cloneOfExisting = (Database.ConversationMessageLinkPreview)_context.Entry(conversationMessageLinkPreview).GetDatabaseValues().ToObject();


			lock (conversationMessageLinkPreviewDeleteSyncRoot)
			{
			    try
			    {
			        conversationMessageLinkPreview.deleted = true;
			        conversationMessageLinkPreview.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ConversationMessageLinkPreviewChangeHistory conversationMessageLinkPreviewChangeHistory = new ConversationMessageLinkPreviewChangeHistory();
			        conversationMessageLinkPreviewChangeHistory.conversationMessageLinkPreviewId = conversationMessageLinkPreview.id;
			        conversationMessageLinkPreviewChangeHistory.versionNumber = conversationMessageLinkPreview.versionNumber;
			        conversationMessageLinkPreviewChangeHistory.timeStamp = DateTime.UtcNow;
			        conversationMessageLinkPreviewChangeHistory.userId = securityUser.id;
			        conversationMessageLinkPreviewChangeHistory.tenantGuid = userTenantGuid;
			        conversationMessageLinkPreviewChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview));
			        _context.ConversationMessageLinkPreviewChangeHistories.Add(conversationMessageLinkPreviewChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ConversationMessageLinkPreview entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ConversationMessageLinkPreview entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessageLinkPreview.CreateAnonymousWithFirstLevelSubObjects(conversationMessageLinkPreview)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ConversationMessageLinkPreview records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ConversationMessageLinkPreviews/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? conversationMessageId = null,
			string url = null,
			string title = null,
			string description = null,
			string imageUrl = null,
			string siteName = null,
			DateTime? fetchedDateTime = null,
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
			if (fetchedDateTime.HasValue == true && fetchedDateTime.Value.Kind != DateTimeKind.Utc)
			{
				fetchedDateTime = fetchedDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.ConversationMessageLinkPreview> query = (from cmlp in _context.ConversationMessageLinkPreviews select cmlp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.conversationMessageId == conversationMessageId.Value);
			}
			if (string.IsNullOrEmpty(url) == false)
			{
				query = query.Where(cmlp => cmlp.url == url);
			}
			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(cmlp => cmlp.title == title);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cmlp => cmlp.description == description);
			}
			if (string.IsNullOrEmpty(imageUrl) == false)
			{
				query = query.Where(cmlp => cmlp.imageUrl == imageUrl);
			}
			if (string.IsNullOrEmpty(siteName) == false)
			{
				query = query.Where(cmlp => cmlp.siteName == siteName);
			}
			if (fetchedDateTime.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.fetchedDateTime == fetchedDateTime.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cmlp => cmlp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cmlp => cmlp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cmlp => cmlp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cmlp => cmlp.deleted == false);
				}
			}
			else
			{
				query = query.Where(cmlp => cmlp.active == true);
				query = query.Where(cmlp => cmlp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Conversation Message Link Preview, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.url.Contains(anyStringContains)
			       || x.title.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.imageUrl.Contains(anyStringContains)
			       || x.siteName.Contains(anyStringContains)
			       || x.conversationMessage.message.Contains(anyStringContains)
			       || x.conversationMessage.messageType.Contains(anyStringContains)
			       || x.conversationMessage.entity.Contains(anyStringContains)
			       || x.conversationMessage.externalURL.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.url).ThenBy(x => x.title).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ConversationMessageLinkPreview.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ConversationMessageLinkPreview/CreateAuditEvent")]
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
