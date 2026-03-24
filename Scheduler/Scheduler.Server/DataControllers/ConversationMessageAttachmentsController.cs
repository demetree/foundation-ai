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
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the ConversationMessageAttachment entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ConversationMessageAttachment entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ConversationMessageAttachmentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object conversationMessageAttachmentPutSyncRoot = new object();
		static object conversationMessageAttachmentDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ConversationMessageAttachmentsController> _logger;

		public ConversationMessageAttachmentsController(SchedulerContext context, ILogger<ConversationMessageAttachmentsController> logger) : base("Scheduler", "ConversationMessageAttachment")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ConversationMessageAttachments filtered by the parameters provided.
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
		[Route("api/ConversationMessageAttachments")]
		public async Task<IActionResult> GetConversationMessageAttachments(
			int? conversationMessageId = null,
			int? userId = null,
			DateTime? dateTimeCreated = null,
			string contentFileName = null,
			long? contentSize = null,
			string contentMimeType = null,
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

			IQueryable<Database.ConversationMessageAttachment> query = (from cma in _context.ConversationMessageAttachments select cma);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(cma => cma.conversationMessageId == conversationMessageId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cma => cma.userId == userId.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(cma => cma.dateTimeCreated == dateTimeCreated.Value);
			}
			if (string.IsNullOrEmpty(contentFileName) == false)
			{
				query = query.Where(cma => cma.contentFileName == contentFileName);
			}
			if (contentSize.HasValue == true)
			{
				query = query.Where(cma => cma.contentSize == contentSize.Value);
			}
			if (string.IsNullOrEmpty(contentMimeType) == false)
			{
				query = query.Where(cma => cma.contentMimeType == contentMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cma => cma.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cma => cma.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cma => cma.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cma => cma.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cma => cma.deleted == false);
				}
			}
			else
			{
				query = query.Where(cma => cma.active == true);
				query = query.Where(cma => cma.deleted == false);
			}

			query = query.OrderBy(cma => cma.contentFileName).ThenBy(cma => cma.contentMimeType);


			//
			// Add the any string contains parameter to span all the string fields on the Conversation Message Attachment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contentFileName.Contains(anyStringContains)
			       || x.contentMimeType.Contains(anyStringContains)
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
			
			List<Database.ConversationMessageAttachment> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ConversationMessageAttachment conversationMessageAttachment in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(conversationMessageAttachment, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async conversationMessageAttachment =>
				{

					if (conversationMessageAttachment.contentData == null &&
					    conversationMessageAttachment.contentSize > 0)
					{
					    conversationMessageAttachment.contentData = await LoadDataFromDiskAsync(conversationMessageAttachment.objectGuid, conversationMessageAttachment.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ConversationMessageAttachment Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ConversationMessageAttachment Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ConversationMessageAttachments filtered by the parameters provided.  Its query is similar to the GetConversationMessageAttachments method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageAttachments/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? conversationMessageId = null,
			int? userId = null,
			DateTime? dateTimeCreated = null,
			string contentFileName = null,
			long? contentSize = null,
			string contentMimeType = null,
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

			IQueryable<Database.ConversationMessageAttachment> query = (from cma in _context.ConversationMessageAttachments select cma);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(cma => cma.conversationMessageId == conversationMessageId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cma => cma.userId == userId.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(cma => cma.dateTimeCreated == dateTimeCreated.Value);
			}
			if (contentFileName != null)
			{
				query = query.Where(cma => cma.contentFileName == contentFileName);
			}
			if (contentSize.HasValue == true)
			{
				query = query.Where(cma => cma.contentSize == contentSize.Value);
			}
			if (contentMimeType != null)
			{
				query = query.Where(cma => cma.contentMimeType == contentMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cma => cma.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cma => cma.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cma => cma.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cma => cma.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cma => cma.deleted == false);
				}
			}
			else
			{
				query = query.Where(cma => cma.active == true);
				query = query.Where(cma => cma.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Conversation Message Attachment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contentFileName.Contains(anyStringContains)
			       || x.contentMimeType.Contains(anyStringContains)
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
        /// This gets a single ConversationMessageAttachment by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageAttachment/{id}")]
		public async Task<IActionResult> GetConversationMessageAttachment(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ConversationMessageAttachment> query = (from cma in _context.ConversationMessageAttachments where
							(cma.id == id) &&
							(userIsAdmin == true || cma.deleted == false) &&
							(userIsWriter == true || cma.active == true)
					select cma);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.conversationMessage);
					query = query.AsSplitQuery();
				}

				Database.ConversationMessageAttachment materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.contentData == null &&
					    materialized.contentSize > 0)
					{
					    materialized.contentData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ConversationMessageAttachment Entity was read with Admin privilege." : "Scheduler.ConversationMessageAttachment Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationMessageAttachment", materialized.id, materialized.contentFileName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ConversationMessageAttachment entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ConversationMessageAttachment.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ConversationMessageAttachment.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ConversationMessageAttachment record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ConversationMessageAttachment/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutConversationMessageAttachment(int id, [FromBody]Database.ConversationMessageAttachment.ConversationMessageAttachmentDTO conversationMessageAttachmentDTO, CancellationToken cancellationToken = default)
		{
			if (conversationMessageAttachmentDTO == null)
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



			if (id != conversationMessageAttachmentDTO.id)
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


			IQueryable<Database.ConversationMessageAttachment> query = (from x in _context.ConversationMessageAttachments
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationMessageAttachment existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationMessageAttachment PUT", id.ToString(), new Exception("No Scheduler.ConversationMessageAttachment entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (conversationMessageAttachmentDTO.objectGuid == Guid.Empty)
            {
                conversationMessageAttachmentDTO.objectGuid = existing.objectGuid;
            }
            else if (conversationMessageAttachmentDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ConversationMessageAttachment record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ConversationMessageAttachment cloneOfExisting = (Database.ConversationMessageAttachment)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ConversationMessageAttachment object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ConversationMessageAttachment conversationMessageAttachment = (Database.ConversationMessageAttachment)_context.Entry(existing).GetDatabaseValues().ToObject();
			conversationMessageAttachment.ApplyDTO(conversationMessageAttachmentDTO);
			//
			// The tenant guid for any ConversationMessageAttachment being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ConversationMessageAttachment because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				conversationMessageAttachment.tenantGuid = existing.tenantGuid;
			}

			lock (conversationMessageAttachmentPutSyncRoot)
			{
				//
				// Validate the version number for the conversationMessageAttachment being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != conversationMessageAttachment.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ConversationMessageAttachment save attempt was made but save request was with version " + conversationMessageAttachment.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ConversationMessageAttachment you are trying to update has already changed.  Please try your save again after reloading the ConversationMessageAttachment.");
				}
				else
				{
					// Same record.  Increase version.
					conversationMessageAttachment.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (conversationMessageAttachment.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ConversationMessageAttachment record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (conversationMessageAttachment.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					conversationMessageAttachment.dateTimeCreated = conversationMessageAttachment.dateTimeCreated.ToUniversalTime();
				}

				if (conversationMessageAttachment.contentFileName != null && conversationMessageAttachment.contentFileName.Length > 250)
				{
					conversationMessageAttachment.contentFileName = conversationMessageAttachment.contentFileName.Substring(0, 250);
				}

				if (conversationMessageAttachment.contentMimeType != null && conversationMessageAttachment.contentMimeType.Length > 100)
				{
					conversationMessageAttachment.contentMimeType = conversationMessageAttachment.contentMimeType.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (conversationMessageAttachment.contentData != null && string.IsNullOrEmpty(conversationMessageAttachment.contentFileName))
				{
				    conversationMessageAttachment.contentFileName = conversationMessageAttachment.objectGuid.ToString() + ".data";
				}

				if (conversationMessageAttachment.contentData != null && conversationMessageAttachment.contentSize != conversationMessageAttachment.contentData.Length)
				{
				    conversationMessageAttachment.contentSize = conversationMessageAttachment.contentData.Length;
				}

				if (conversationMessageAttachment.contentData != null && string.IsNullOrEmpty(conversationMessageAttachment.contentMimeType))
				{
				    conversationMessageAttachment.contentMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = conversationMessageAttachment.contentData;

					if (diskBasedBinaryStorageMode == true &&
					    conversationMessageAttachment.contentFileName != null &&
					    conversationMessageAttachment.contentData != null &&
					    conversationMessageAttachment.contentSize > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(conversationMessageAttachment.objectGuid, conversationMessageAttachment.versionNumber, conversationMessageAttachment.contentData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    conversationMessageAttachment.contentData = null;

					}

				    EntityEntry<Database.ConversationMessageAttachment> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(conversationMessageAttachment);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory = new ConversationMessageAttachmentChangeHistory();
				        conversationMessageAttachmentChangeHistory.conversationMessageAttachmentId = conversationMessageAttachment.id;
				        conversationMessageAttachmentChangeHistory.versionNumber = conversationMessageAttachment.versionNumber;
				        conversationMessageAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
				        conversationMessageAttachmentChangeHistory.userId = securityUser.id;
				        conversationMessageAttachmentChangeHistory.tenantGuid = userTenantGuid;
				        conversationMessageAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment));
				        _context.ConversationMessageAttachmentChangeHistories.Add(conversationMessageAttachmentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    conversationMessageAttachment.contentData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationMessageAttachment entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment)),
						null);

				return Ok(Database.ConversationMessageAttachment.CreateAnonymous(conversationMessageAttachment));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationMessageAttachment entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ConversationMessageAttachment record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageAttachment", Name = "ConversationMessageAttachment")]
		public async Task<IActionResult> PostConversationMessageAttachment([FromBody]Database.ConversationMessageAttachment.ConversationMessageAttachmentDTO conversationMessageAttachmentDTO, CancellationToken cancellationToken = default)
		{
			if (conversationMessageAttachmentDTO == null)
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


			if (conversationMessageAttachmentDTO.contentData == null)
			{
				return BadRequest("No data");
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
			// Create a new ConversationMessageAttachment object using the data from the DTO
			//
			Database.ConversationMessageAttachment conversationMessageAttachment = Database.ConversationMessageAttachment.FromDTO(conversationMessageAttachmentDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				conversationMessageAttachment.tenantGuid = userTenantGuid;

				if (conversationMessageAttachment.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					conversationMessageAttachment.dateTimeCreated = conversationMessageAttachment.dateTimeCreated.ToUniversalTime();
				}

				if (conversationMessageAttachment.contentFileName != null && conversationMessageAttachment.contentFileName.Length > 250)
				{
					conversationMessageAttachment.contentFileName = conversationMessageAttachment.contentFileName.Substring(0, 250);
				}

				if (conversationMessageAttachment.contentMimeType != null && conversationMessageAttachment.contentMimeType.Length > 100)
				{
					conversationMessageAttachment.contentMimeType = conversationMessageAttachment.contentMimeType.Substring(0, 100);
				}

				conversationMessageAttachment.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (conversationMessageAttachment.contentData != null && string.IsNullOrEmpty(conversationMessageAttachment.contentFileName))
				{
				    conversationMessageAttachment.contentFileName = conversationMessageAttachment.objectGuid.ToString() + ".data";
				}

				if (conversationMessageAttachment.contentData != null && conversationMessageAttachment.contentSize != conversationMessageAttachment.contentData.Length)
				{
				    conversationMessageAttachment.contentSize = conversationMessageAttachment.contentData.Length;
				}

				if (conversationMessageAttachment.contentData != null && string.IsNullOrEmpty(conversationMessageAttachment.contentMimeType))
				{
				    conversationMessageAttachment.contentMimeType = "application/octet-stream";
				}

				conversationMessageAttachment.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = conversationMessageAttachment.contentData;

				if (diskBasedBinaryStorageMode == true &&
				    conversationMessageAttachment.contentData != null &&
				    conversationMessageAttachment.contentFileName != null &&
				    conversationMessageAttachment.contentSize > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(conversationMessageAttachment.objectGuid, conversationMessageAttachment.versionNumber, conversationMessageAttachment.contentData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    conversationMessageAttachment.contentData = null;

				}

				_context.ConversationMessageAttachments.Add(conversationMessageAttachment);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the conversationMessageAttachment object so that no further changes will be written to the database
				    //
				    _context.Entry(conversationMessageAttachment).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					conversationMessageAttachment.contentData = null;
					conversationMessageAttachment.ConversationMessageAttachmentChangeHistories = null;
					conversationMessageAttachment.conversationMessage = null;


				    ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory = new ConversationMessageAttachmentChangeHistory();
				    conversationMessageAttachmentChangeHistory.conversationMessageAttachmentId = conversationMessageAttachment.id;
				    conversationMessageAttachmentChangeHistory.versionNumber = conversationMessageAttachment.versionNumber;
				    conversationMessageAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
				    conversationMessageAttachmentChangeHistory.userId = securityUser.id;
				    conversationMessageAttachmentChangeHistory.tenantGuid = userTenantGuid;
				    conversationMessageAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment));
				    _context.ConversationMessageAttachmentChangeHistories.Add(conversationMessageAttachmentChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ConversationMessageAttachment entity successfully created.",
						true,
						conversationMessageAttachment. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    conversationMessageAttachment.contentData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ConversationMessageAttachment entity creation failed.", false, conversationMessageAttachment.id.ToString(), "", JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ConversationMessageAttachment", conversationMessageAttachment.id, conversationMessageAttachment.contentFileName));

			return CreatedAtRoute("ConversationMessageAttachment", new { id = conversationMessageAttachment.id }, Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment));
		}



        /// <summary>
        /// 
        /// This rolls a ConversationMessageAttachment entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageAttachment/Rollback/{id}")]
		[Route("api/ConversationMessageAttachment/Rollback")]
		public async Task<IActionResult> RollbackToConversationMessageAttachmentVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ConversationMessageAttachment> query = (from x in _context.ConversationMessageAttachments
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this ConversationMessageAttachment concurrently
			//
			lock (conversationMessageAttachmentPutSyncRoot)
			{
				
				Database.ConversationMessageAttachment conversationMessageAttachment = query.FirstOrDefault();
				
				if (conversationMessageAttachment == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationMessageAttachment rollback", id.ToString(), new Exception("No Scheduler.ConversationMessageAttachment entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ConversationMessageAttachment current state so we can log it.
				//
				Database.ConversationMessageAttachment cloneOfExisting = (Database.ConversationMessageAttachment)_context.Entry(conversationMessageAttachment).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.contentData = null;
				cloneOfExisting.ConversationMessageAttachmentChangeHistories = null;
				cloneOfExisting.conversationMessage = null;

				if (versionNumber >= conversationMessageAttachment.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ConversationMessageAttachment rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ConversationMessageAttachment rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory = (from x in _context.ConversationMessageAttachmentChangeHistories
				                                               where
				                                               x.conversationMessageAttachmentId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (conversationMessageAttachmentChangeHistory != null)
				{
				    Database.ConversationMessageAttachment oldConversationMessageAttachment = JsonSerializer.Deserialize<Database.ConversationMessageAttachment>(conversationMessageAttachmentChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    conversationMessageAttachment.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    conversationMessageAttachment.conversationMessageId = oldConversationMessageAttachment.conversationMessageId;
				    conversationMessageAttachment.userId = oldConversationMessageAttachment.userId;
				    conversationMessageAttachment.dateTimeCreated = oldConversationMessageAttachment.dateTimeCreated;
				    conversationMessageAttachment.contentFileName = oldConversationMessageAttachment.contentFileName;
				    conversationMessageAttachment.contentSize = oldConversationMessageAttachment.contentSize;
				    conversationMessageAttachment.contentData = oldConversationMessageAttachment.contentData;
				    conversationMessageAttachment.contentMimeType = oldConversationMessageAttachment.contentMimeType;
				    conversationMessageAttachment.objectGuid = oldConversationMessageAttachment.objectGuid;
				    conversationMessageAttachment.active = oldConversationMessageAttachment.active;
				    conversationMessageAttachment.deleted = oldConversationMessageAttachment.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldConversationMessageAttachment.objectGuid, oldConversationMessageAttachment.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(conversationMessageAttachment.objectGuid, conversationMessageAttachment.versionNumber, binaryData, "data");
				    }

				    string serializedConversationMessageAttachment = JsonSerializer.Serialize(conversationMessageAttachment);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ConversationMessageAttachmentChangeHistory newConversationMessageAttachmentChangeHistory = new ConversationMessageAttachmentChangeHistory();
				        newConversationMessageAttachmentChangeHistory.conversationMessageAttachmentId = conversationMessageAttachment.id;
				        newConversationMessageAttachmentChangeHistory.versionNumber = conversationMessageAttachment.versionNumber;
				        newConversationMessageAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
				        newConversationMessageAttachmentChangeHistory.userId = securityUser.id;
				        newConversationMessageAttachmentChangeHistory.tenantGuid = userTenantGuid;
				        newConversationMessageAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment));
				        _context.ConversationMessageAttachmentChangeHistories.Add(newConversationMessageAttachmentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ConversationMessageAttachment rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment)),
						null);


				    return Ok(Database.ConversationMessageAttachment.CreateAnonymous(conversationMessageAttachment));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ConversationMessageAttachment rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ConversationMessageAttachment rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ConversationMessageAttachment.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessageAttachment</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageAttachment/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetConversationMessageAttachmentChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.ConversationMessageAttachment conversationMessageAttachment = await _context.ConversationMessageAttachments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessageAttachment == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessageAttachment.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationMessageAttachment> versionInfo = await conversationMessageAttachment.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ConversationMessageAttachment.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessageAttachment</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageAttachment/{id}/AuditHistory")]
		public async Task<IActionResult> GetConversationMessageAttachmentAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.ConversationMessageAttachment conversationMessageAttachment = await _context.ConversationMessageAttachments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessageAttachment == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessageAttachment.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ConversationMessageAttachment>> versions = await conversationMessageAttachment.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ConversationMessageAttachment.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessageAttachment</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ConversationMessageAttachment object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageAttachment/{id}/Version/{version}")]
		public async Task<IActionResult> GetConversationMessageAttachmentVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.ConversationMessageAttachment conversationMessageAttachment = await _context.ConversationMessageAttachments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessageAttachment == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessageAttachment.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationMessageAttachment> versionInfo = await conversationMessageAttachment.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ConversationMessageAttachment at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ConversationMessageAttachment</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ConversationMessageAttachment object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageAttachment/{id}/StateAtTime")]
		public async Task<IActionResult> GetConversationMessageAttachmentStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.ConversationMessageAttachment conversationMessageAttachment = await _context.ConversationMessageAttachments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (conversationMessageAttachment == null)
			{
				return NotFound();
			}

			try
			{
				conversationMessageAttachment.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ConversationMessageAttachment> versionInfo = await conversationMessageAttachment.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ConversationMessageAttachment record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ConversationMessageAttachment/{id}")]
		[Route("api/ConversationMessageAttachment")]
		public async Task<IActionResult> DeleteConversationMessageAttachment(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ConversationMessageAttachment> query = (from x in _context.ConversationMessageAttachments
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ConversationMessageAttachment conversationMessageAttachment = await query.FirstOrDefaultAsync(cancellationToken);

			if (conversationMessageAttachment == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ConversationMessageAttachment DELETE", id.ToString(), new Exception("No Scheduler.ConversationMessageAttachment entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ConversationMessageAttachment cloneOfExisting = (Database.ConversationMessageAttachment)_context.Entry(conversationMessageAttachment).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (conversationMessageAttachmentDeleteSyncRoot)
			{
			    try
			    {
			        conversationMessageAttachment.deleted = true;
			        conversationMessageAttachment.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(conversationMessageAttachment.objectGuid, conversationMessageAttachment.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(conversationMessageAttachment.objectGuid, conversationMessageAttachment.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory = new ConversationMessageAttachmentChangeHistory();
			        conversationMessageAttachmentChangeHistory.conversationMessageAttachmentId = conversationMessageAttachment.id;
			        conversationMessageAttachmentChangeHistory.versionNumber = conversationMessageAttachment.versionNumber;
			        conversationMessageAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
			        conversationMessageAttachmentChangeHistory.userId = securityUser.id;
			        conversationMessageAttachmentChangeHistory.tenantGuid = userTenantGuid;
			        conversationMessageAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment));
			        _context.ConversationMessageAttachmentChangeHistories.Add(conversationMessageAttachmentChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ConversationMessageAttachment entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ConversationMessageAttachment entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ConversationMessageAttachment records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ConversationMessageAttachments/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? conversationMessageId = null,
			int? userId = null,
			DateTime? dateTimeCreated = null,
			string contentFileName = null,
			long? contentSize = null,
			string contentMimeType = null,
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

			IQueryable<Database.ConversationMessageAttachment> query = (from cma in _context.ConversationMessageAttachments select cma);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (conversationMessageId.HasValue == true)
			{
				query = query.Where(cma => cma.conversationMessageId == conversationMessageId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(cma => cma.userId == userId.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(cma => cma.dateTimeCreated == dateTimeCreated.Value);
			}
			if (string.IsNullOrEmpty(contentFileName) == false)
			{
				query = query.Where(cma => cma.contentFileName == contentFileName);
			}
			if (contentSize.HasValue == true)
			{
				query = query.Where(cma => cma.contentSize == contentSize.Value);
			}
			if (string.IsNullOrEmpty(contentMimeType) == false)
			{
				query = query.Where(cma => cma.contentMimeType == contentMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cma => cma.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cma => cma.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cma => cma.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cma => cma.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cma => cma.deleted == false);
				}
			}
			else
			{
				query = query.Where(cma => cma.active == true);
				query = query.Where(cma => cma.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Conversation Message Attachment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contentFileName.Contains(anyStringContains)
			       || x.contentMimeType.Contains(anyStringContains)
			       || x.conversationMessage.message.Contains(anyStringContains)
			       || x.conversationMessage.messageType.Contains(anyStringContains)
			       || x.conversationMessage.entity.Contains(anyStringContains)
			       || x.conversationMessage.externalURL.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.contentFileName).ThenBy(x => x.contentMimeType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ConversationMessageAttachment.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ConversationMessageAttachment/CreateAuditEvent")]
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




        [Route("api/ConversationMessageAttachment/Data/{id:int}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> UploadData(int id, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			MediaTypeHeaderValue mediaTypeHeader; 

            if (!HttpContext.Request.HasFormContentType ||
				!MediaTypeHeaderValue.TryParse(HttpContext.Request.ContentType, out mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }


            Database.ConversationMessageAttachment conversationMessageAttachment = await (from x in _context.ConversationMessageAttachments where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (conversationMessageAttachment == null)
            {
                return NotFound();
            }

            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


            // This will be used to signal whether we are saving data or clearing it.
            bool foundFileData = false;


            //
            // This will get the first file from the request and save it
            //
			try
			{
                MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, HttpContext.Request.Body);
                MultipartSection section = await reader.ReadNextSectionAsync();

                while (section != null)
				{
					ContentDispositionHeaderValue contentDisposition;

					bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);


					if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
						!string.IsNullOrEmpty(contentDisposition.FileName.Value))
					{

						foundFileData = true;
						string fileName = contentDisposition.FileName.ToString().Trim('"');

						// default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.
						MediaTypeHeaderValue mediaType;
						bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);

						string mimeType = "application/octet-stream";
						if (hasMediaTypeHeader && mediaTypeHeader.MediaType != null )
						{
							mimeType = mediaTypeHeader.MediaType.ToString();
						}

						lock (conversationMessageAttachmentPutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									conversationMessageAttachment.contentFileName = fileName.Trim();
									conversationMessageAttachment.contentMimeType = mimeType;
									conversationMessageAttachment.contentSize = section.Body.Length;

									conversationMessageAttachment.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 conversationMessageAttachment.contentFileName != null &&
										 conversationMessageAttachment.contentSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(conversationMessageAttachment.objectGuid, conversationMessageAttachment.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										conversationMessageAttachment.contentData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											conversationMessageAttachment.contentData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory = new ConversationMessageAttachmentChangeHistory();
									conversationMessageAttachmentChangeHistory.conversationMessageAttachmentId = conversationMessageAttachment.id;
									conversationMessageAttachmentChangeHistory.versionNumber = conversationMessageAttachment.versionNumber;
									conversationMessageAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
									conversationMessageAttachmentChangeHistory.userId = securityUser.id;
									conversationMessageAttachmentChangeHistory.tenantGuid = conversationMessageAttachment.tenantGuid;
									conversationMessageAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment));
									_context.ConversationMessageAttachmentChangeHistories.Add(conversationMessageAttachmentChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ConversationMessageAttachment Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "ConversationMessageAttachment Data Upload Failed.", false, id.ToString(), "", "", ex);

								return Problem(ex.Message);
							}
						}


						//
						// Stop looking for more files.
						//
						break;
					}

					section = await reader.ReadNextSectionAsync();
				}
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Caught error in UploadData handler", id.ToString(), ex);

                return Problem(ex.Message);
            }

            //
            // Treat the situation where we have a valid ID but no file content as a request to clear the data
            //
            if (foundFileData == false)
            {
                lock (conversationMessageAttachmentPutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(conversationMessageAttachment.objectGuid, conversationMessageAttachment.versionNumber, "data");
                            }

                            conversationMessageAttachment.contentFileName = null;
                            conversationMessageAttachment.contentMimeType = null;
                            conversationMessageAttachment.contentSize = 0;
                            conversationMessageAttachment.contentData = null;
                            conversationMessageAttachment.versionNumber++;


                            //
                            // Now add the change history
                            //
                            ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory = new ConversationMessageAttachmentChangeHistory();
                            conversationMessageAttachmentChangeHistory.conversationMessageAttachmentId = conversationMessageAttachment.id;
                            conversationMessageAttachmentChangeHistory.versionNumber = conversationMessageAttachment.versionNumber;
                            conversationMessageAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
                            conversationMessageAttachmentChangeHistory.userId = securityUser.id;
                                    conversationMessageAttachmentChangeHistory.tenantGuid = conversationMessageAttachment.tenantGuid;
                                    conversationMessageAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.ConversationMessageAttachment.CreateAnonymousWithFirstLevelSubObjects(conversationMessageAttachment));
                            _context.ConversationMessageAttachmentChangeHistories.Add(conversationMessageAttachmentChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ConversationMessageAttachment data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "ConversationMessageAttachment data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/ConversationMessageAttachment/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id, CancellationToken cancellationToken = default)
        {

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			using (SchedulerContext context = new SchedulerContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.ConversationMessageAttachment conversationMessageAttachment = await (from d in context.ConversationMessageAttachments
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (conversationMessageAttachment != null && conversationMessageAttachment.contentData != null)
                {
                   return File(conversationMessageAttachment.contentData.ToArray<byte>(), conversationMessageAttachment.contentMimeType, conversationMessageAttachment.contentFileName != null ? conversationMessageAttachment.contentFileName.Trim() : "ConversationMessageAttachment_" + conversationMessageAttachment.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
