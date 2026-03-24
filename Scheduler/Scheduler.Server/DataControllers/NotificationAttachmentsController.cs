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
    /// This auto generated class provides the basic CRUD operations for the NotificationAttachment entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the NotificationAttachment entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class NotificationAttachmentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 50;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object notificationAttachmentPutSyncRoot = new object();
		static object notificationAttachmentDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<NotificationAttachmentsController> _logger;

		public NotificationAttachmentsController(SchedulerContext context, ILogger<NotificationAttachmentsController> logger) : base("Scheduler", "NotificationAttachment")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of NotificationAttachments filtered by the parameters provided.
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
		[Route("api/NotificationAttachments")]
		public async Task<IActionResult> GetNotificationAttachments(
			int? notificationId = null,
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

			IQueryable<Database.NotificationAttachment> query = (from na in _context.NotificationAttachments select na);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (notificationId.HasValue == true)
			{
				query = query.Where(na => na.notificationId == notificationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(na => na.userId == userId.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(na => na.dateTimeCreated == dateTimeCreated.Value);
			}
			if (string.IsNullOrEmpty(contentFileName) == false)
			{
				query = query.Where(na => na.contentFileName == contentFileName);
			}
			if (contentSize.HasValue == true)
			{
				query = query.Where(na => na.contentSize == contentSize.Value);
			}
			if (string.IsNullOrEmpty(contentMimeType) == false)
			{
				query = query.Where(na => na.contentMimeType == contentMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(na => na.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(na => na.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(na => na.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(na => na.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(na => na.deleted == false);
				}
			}
			else
			{
				query = query.Where(na => na.active == true);
				query = query.Where(na => na.deleted == false);
			}

			query = query.OrderBy(na => na.contentFileName).ThenBy(na => na.contentMimeType);


			//
			// Add the any string contains parameter to span all the string fields on the Notification Attachment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contentFileName.Contains(anyStringContains)
			       || x.contentMimeType.Contains(anyStringContains)
			       || (includeRelations == true && x.notification.message.Contains(anyStringContains))
			       || (includeRelations == true && x.notification.entity.Contains(anyStringContains))
			       || (includeRelations == true && x.notification.externalURL.Contains(anyStringContains))
			   );
			}

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
			
			List<Database.NotificationAttachment> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.NotificationAttachment notificationAttachment in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(notificationAttachment, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async notificationAttachment =>
				{

					if (notificationAttachment.contentData == null &&
					    notificationAttachment.contentSize > 0)
					{
					    notificationAttachment.contentData = await LoadDataFromDiskAsync(notificationAttachment.objectGuid, notificationAttachment.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.NotificationAttachment Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.NotificationAttachment Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of NotificationAttachments filtered by the parameters provided.  Its query is similar to the GetNotificationAttachments method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationAttachments/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? notificationId = null,
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

			IQueryable<Database.NotificationAttachment> query = (from na in _context.NotificationAttachments select na);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (notificationId.HasValue == true)
			{
				query = query.Where(na => na.notificationId == notificationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(na => na.userId == userId.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(na => na.dateTimeCreated == dateTimeCreated.Value);
			}
			if (contentFileName != null)
			{
				query = query.Where(na => na.contentFileName == contentFileName);
			}
			if (contentSize.HasValue == true)
			{
				query = query.Where(na => na.contentSize == contentSize.Value);
			}
			if (contentMimeType != null)
			{
				query = query.Where(na => na.contentMimeType == contentMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(na => na.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(na => na.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(na => na.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(na => na.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(na => na.deleted == false);
				}
			}
			else
			{
				query = query.Where(na => na.active == true);
				query = query.Where(na => na.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Notification Attachment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contentFileName.Contains(anyStringContains)
			       || x.contentMimeType.Contains(anyStringContains)
			       || x.notification.message.Contains(anyStringContains)
			       || x.notification.entity.Contains(anyStringContains)
			       || x.notification.externalURL.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single NotificationAttachment by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationAttachment/{id}")]
		public async Task<IActionResult> GetNotificationAttachment(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.NotificationAttachment> query = (from na in _context.NotificationAttachments where
							(na.id == id) &&
							(userIsAdmin == true || na.deleted == false) &&
							(userIsWriter == true || na.active == true)
					select na);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.notification);
					query = query.AsSplitQuery();
				}

				Database.NotificationAttachment materialized = await query.FirstOrDefaultAsync(cancellationToken);

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

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.NotificationAttachment Entity was read with Admin privilege." : "Scheduler.NotificationAttachment Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "NotificationAttachment", materialized.id, materialized.contentFileName));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.NotificationAttachment entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.NotificationAttachment.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.NotificationAttachment.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing NotificationAttachment record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/NotificationAttachment/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutNotificationAttachment(int id, [FromBody]Database.NotificationAttachment.NotificationAttachmentDTO notificationAttachmentDTO, CancellationToken cancellationToken = default)
		{
			if (notificationAttachmentDTO == null)
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



			if (id != notificationAttachmentDTO.id)
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


			IQueryable<Database.NotificationAttachment> query = (from x in _context.NotificationAttachments
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.NotificationAttachment existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.NotificationAttachment PUT", id.ToString(), new Exception("No Scheduler.NotificationAttachment entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (notificationAttachmentDTO.objectGuid == Guid.Empty)
            {
                notificationAttachmentDTO.objectGuid = existing.objectGuid;
            }
            else if (notificationAttachmentDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a NotificationAttachment record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.NotificationAttachment cloneOfExisting = (Database.NotificationAttachment)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new NotificationAttachment object using the data from the existing record, updated with what is in the DTO.
			//
			Database.NotificationAttachment notificationAttachment = (Database.NotificationAttachment)_context.Entry(existing).GetDatabaseValues().ToObject();
			notificationAttachment.ApplyDTO(notificationAttachmentDTO);
			//
			// The tenant guid for any NotificationAttachment being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the NotificationAttachment because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				notificationAttachment.tenantGuid = existing.tenantGuid;
			}

			lock (notificationAttachmentPutSyncRoot)
			{
				//
				// Validate the version number for the notificationAttachment being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != notificationAttachment.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "NotificationAttachment save attempt was made but save request was with version " + notificationAttachment.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The NotificationAttachment you are trying to update has already changed.  Please try your save again after reloading the NotificationAttachment.");
				}
				else
				{
					// Same record.  Increase version.
					notificationAttachment.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (notificationAttachment.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.NotificationAttachment record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (notificationAttachment.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					notificationAttachment.dateTimeCreated = notificationAttachment.dateTimeCreated.ToUniversalTime();
				}

				if (notificationAttachment.contentFileName != null && notificationAttachment.contentFileName.Length > 250)
				{
					notificationAttachment.contentFileName = notificationAttachment.contentFileName.Substring(0, 250);
				}

				if (notificationAttachment.contentMimeType != null && notificationAttachment.contentMimeType.Length > 100)
				{
					notificationAttachment.contentMimeType = notificationAttachment.contentMimeType.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (notificationAttachment.contentData != null && string.IsNullOrEmpty(notificationAttachment.contentFileName))
				{
				    notificationAttachment.contentFileName = notificationAttachment.objectGuid.ToString() + ".data";
				}

				if (notificationAttachment.contentData != null && notificationAttachment.contentSize != notificationAttachment.contentData.Length)
				{
				    notificationAttachment.contentSize = notificationAttachment.contentData.Length;
				}

				if (notificationAttachment.contentData != null && string.IsNullOrEmpty(notificationAttachment.contentMimeType))
				{
				    notificationAttachment.contentMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = notificationAttachment.contentData;

					if (diskBasedBinaryStorageMode == true &&
					    notificationAttachment.contentFileName != null &&
					    notificationAttachment.contentData != null &&
					    notificationAttachment.contentSize > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(notificationAttachment.objectGuid, notificationAttachment.versionNumber, notificationAttachment.contentData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    notificationAttachment.contentData = null;

					}

				    EntityEntry<Database.NotificationAttachment> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(notificationAttachment);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        NotificationAttachmentChangeHistory notificationAttachmentChangeHistory = new NotificationAttachmentChangeHistory();
				        notificationAttachmentChangeHistory.notificationAttachmentId = notificationAttachment.id;
				        notificationAttachmentChangeHistory.versionNumber = notificationAttachment.versionNumber;
				        notificationAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
				        notificationAttachmentChangeHistory.userId = securityUser.id;
				        notificationAttachmentChangeHistory.tenantGuid = userTenantGuid;
				        notificationAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment));
				        _context.NotificationAttachmentChangeHistories.Add(notificationAttachmentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    notificationAttachment.contentData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.NotificationAttachment entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment)),
						null);

				return Ok(Database.NotificationAttachment.CreateAnonymous(notificationAttachment));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.NotificationAttachment entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new NotificationAttachment record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationAttachment", Name = "NotificationAttachment")]
		public async Task<IActionResult> PostNotificationAttachment([FromBody]Database.NotificationAttachment.NotificationAttachmentDTO notificationAttachmentDTO, CancellationToken cancellationToken = default)
		{
			if (notificationAttachmentDTO == null)
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


			if (notificationAttachmentDTO.contentData == null)
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
			// Create a new NotificationAttachment object using the data from the DTO
			//
			Database.NotificationAttachment notificationAttachment = Database.NotificationAttachment.FromDTO(notificationAttachmentDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				notificationAttachment.tenantGuid = userTenantGuid;

				if (notificationAttachment.dateTimeCreated.Kind != DateTimeKind.Utc)
				{
					notificationAttachment.dateTimeCreated = notificationAttachment.dateTimeCreated.ToUniversalTime();
				}

				if (notificationAttachment.contentFileName != null && notificationAttachment.contentFileName.Length > 250)
				{
					notificationAttachment.contentFileName = notificationAttachment.contentFileName.Substring(0, 250);
				}

				if (notificationAttachment.contentMimeType != null && notificationAttachment.contentMimeType.Length > 100)
				{
					notificationAttachment.contentMimeType = notificationAttachment.contentMimeType.Substring(0, 100);
				}

				notificationAttachment.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (notificationAttachment.contentData != null && string.IsNullOrEmpty(notificationAttachment.contentFileName))
				{
				    notificationAttachment.contentFileName = notificationAttachment.objectGuid.ToString() + ".data";
				}

				if (notificationAttachment.contentData != null && notificationAttachment.contentSize != notificationAttachment.contentData.Length)
				{
				    notificationAttachment.contentSize = notificationAttachment.contentData.Length;
				}

				if (notificationAttachment.contentData != null && string.IsNullOrEmpty(notificationAttachment.contentMimeType))
				{
				    notificationAttachment.contentMimeType = "application/octet-stream";
				}

				notificationAttachment.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = notificationAttachment.contentData;

				if (diskBasedBinaryStorageMode == true &&
				    notificationAttachment.contentData != null &&
				    notificationAttachment.contentFileName != null &&
				    notificationAttachment.contentSize > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(notificationAttachment.objectGuid, notificationAttachment.versionNumber, notificationAttachment.contentData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    notificationAttachment.contentData = null;

				}

				_context.NotificationAttachments.Add(notificationAttachment);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the notificationAttachment object so that no further changes will be written to the database
				    //
				    _context.Entry(notificationAttachment).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					notificationAttachment.contentData = null;
					notificationAttachment.NotificationAttachmentChangeHistories = null;
					notificationAttachment.notification = null;


				    NotificationAttachmentChangeHistory notificationAttachmentChangeHistory = new NotificationAttachmentChangeHistory();
				    notificationAttachmentChangeHistory.notificationAttachmentId = notificationAttachment.id;
				    notificationAttachmentChangeHistory.versionNumber = notificationAttachment.versionNumber;
				    notificationAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
				    notificationAttachmentChangeHistory.userId = securityUser.id;
				    notificationAttachmentChangeHistory.tenantGuid = userTenantGuid;
				    notificationAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment));
				    _context.NotificationAttachmentChangeHistories.Add(notificationAttachmentChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.NotificationAttachment entity successfully created.",
						true,
						notificationAttachment. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    notificationAttachment.contentData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.NotificationAttachment entity creation failed.", false, notificationAttachment.id.ToString(), "", JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "NotificationAttachment", notificationAttachment.id, notificationAttachment.contentFileName));

			return CreatedAtRoute("NotificationAttachment", new { id = notificationAttachment.id }, Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment));
		}



        /// <summary>
        /// 
        /// This rolls a NotificationAttachment entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationAttachment/Rollback/{id}")]
		[Route("api/NotificationAttachment/Rollback")]
		public async Task<IActionResult> RollbackToNotificationAttachmentVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.NotificationAttachment> query = (from x in _context.NotificationAttachments
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this NotificationAttachment concurrently
			//
			lock (notificationAttachmentPutSyncRoot)
			{
				
				Database.NotificationAttachment notificationAttachment = query.FirstOrDefault();
				
				if (notificationAttachment == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.NotificationAttachment rollback", id.ToString(), new Exception("No Scheduler.NotificationAttachment entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the NotificationAttachment current state so we can log it.
				//
				Database.NotificationAttachment cloneOfExisting = (Database.NotificationAttachment)_context.Entry(notificationAttachment).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.contentData = null;
				cloneOfExisting.NotificationAttachmentChangeHistories = null;
				cloneOfExisting.notification = null;

				if (versionNumber >= notificationAttachment.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.NotificationAttachment rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.NotificationAttachment rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				NotificationAttachmentChangeHistory notificationAttachmentChangeHistory = (from x in _context.NotificationAttachmentChangeHistories
				                                               where
				                                               x.notificationAttachmentId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (notificationAttachmentChangeHistory != null)
				{
				    Database.NotificationAttachment oldNotificationAttachment = JsonSerializer.Deserialize<Database.NotificationAttachment>(notificationAttachmentChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    notificationAttachment.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    notificationAttachment.notificationId = oldNotificationAttachment.notificationId;
				    notificationAttachment.userId = oldNotificationAttachment.userId;
				    notificationAttachment.dateTimeCreated = oldNotificationAttachment.dateTimeCreated;
				    notificationAttachment.contentFileName = oldNotificationAttachment.contentFileName;
				    notificationAttachment.contentSize = oldNotificationAttachment.contentSize;
				    notificationAttachment.contentData = oldNotificationAttachment.contentData;
				    notificationAttachment.contentMimeType = oldNotificationAttachment.contentMimeType;
				    notificationAttachment.objectGuid = oldNotificationAttachment.objectGuid;
				    notificationAttachment.active = oldNotificationAttachment.active;
				    notificationAttachment.deleted = oldNotificationAttachment.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldNotificationAttachment.objectGuid, oldNotificationAttachment.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(notificationAttachment.objectGuid, notificationAttachment.versionNumber, binaryData, "data");
				    }

				    string serializedNotificationAttachment = JsonSerializer.Serialize(notificationAttachment);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        NotificationAttachmentChangeHistory newNotificationAttachmentChangeHistory = new NotificationAttachmentChangeHistory();
				        newNotificationAttachmentChangeHistory.notificationAttachmentId = notificationAttachment.id;
				        newNotificationAttachmentChangeHistory.versionNumber = notificationAttachment.versionNumber;
				        newNotificationAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
				        newNotificationAttachmentChangeHistory.userId = securityUser.id;
				        newNotificationAttachmentChangeHistory.tenantGuid = userTenantGuid;
				        newNotificationAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment));
				        _context.NotificationAttachmentChangeHistories.Add(newNotificationAttachmentChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.NotificationAttachment rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment)),
						null);


				    return Ok(Database.NotificationAttachment.CreateAnonymous(notificationAttachment));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.NotificationAttachment rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.NotificationAttachment rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a NotificationAttachment.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the NotificationAttachment</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationAttachment/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetNotificationAttachmentChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.NotificationAttachment notificationAttachment = await _context.NotificationAttachments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (notificationAttachment == null)
			{
				return NotFound();
			}

			try
			{
				notificationAttachment.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.NotificationAttachment> versionInfo = await notificationAttachment.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a NotificationAttachment.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the NotificationAttachment</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationAttachment/{id}/AuditHistory")]
		public async Task<IActionResult> GetNotificationAttachmentAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.NotificationAttachment notificationAttachment = await _context.NotificationAttachments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (notificationAttachment == null)
			{
				return NotFound();
			}

			try
			{
				notificationAttachment.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.NotificationAttachment>> versions = await notificationAttachment.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a NotificationAttachment.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the NotificationAttachment</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The NotificationAttachment object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationAttachment/{id}/Version/{version}")]
		public async Task<IActionResult> GetNotificationAttachmentVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.NotificationAttachment notificationAttachment = await _context.NotificationAttachments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (notificationAttachment == null)
			{
				return NotFound();
			}

			try
			{
				notificationAttachment.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.NotificationAttachment> versionInfo = await notificationAttachment.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a NotificationAttachment at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the NotificationAttachment</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The NotificationAttachment object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationAttachment/{id}/StateAtTime")]
		public async Task<IActionResult> GetNotificationAttachmentStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.NotificationAttachment notificationAttachment = await _context.NotificationAttachments.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (notificationAttachment == null)
			{
				return NotFound();
			}

			try
			{
				notificationAttachment.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.NotificationAttachment> versionInfo = await notificationAttachment.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a NotificationAttachment record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/NotificationAttachment/{id}")]
		[Route("api/NotificationAttachment")]
		public async Task<IActionResult> DeleteNotificationAttachment(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.NotificationAttachment> query = (from x in _context.NotificationAttachments
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.NotificationAttachment notificationAttachment = await query.FirstOrDefaultAsync(cancellationToken);

			if (notificationAttachment == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.NotificationAttachment DELETE", id.ToString(), new Exception("No Scheduler.NotificationAttachment entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.NotificationAttachment cloneOfExisting = (Database.NotificationAttachment)_context.Entry(notificationAttachment).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (notificationAttachmentDeleteSyncRoot)
			{
			    try
			    {
			        notificationAttachment.deleted = true;
			        notificationAttachment.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(notificationAttachment.objectGuid, notificationAttachment.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(notificationAttachment.objectGuid, notificationAttachment.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        NotificationAttachmentChangeHistory notificationAttachmentChangeHistory = new NotificationAttachmentChangeHistory();
			        notificationAttachmentChangeHistory.notificationAttachmentId = notificationAttachment.id;
			        notificationAttachmentChangeHistory.versionNumber = notificationAttachment.versionNumber;
			        notificationAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
			        notificationAttachmentChangeHistory.userId = securityUser.id;
			        notificationAttachmentChangeHistory.tenantGuid = userTenantGuid;
			        notificationAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment));
			        _context.NotificationAttachmentChangeHistories.Add(notificationAttachmentChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.NotificationAttachment entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.NotificationAttachment entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of NotificationAttachment records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/NotificationAttachments/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? notificationId = null,
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

			IQueryable<Database.NotificationAttachment> query = (from na in _context.NotificationAttachments select na);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (notificationId.HasValue == true)
			{
				query = query.Where(na => na.notificationId == notificationId.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(na => na.userId == userId.Value);
			}
			if (dateTimeCreated.HasValue == true)
			{
				query = query.Where(na => na.dateTimeCreated == dateTimeCreated.Value);
			}
			if (string.IsNullOrEmpty(contentFileName) == false)
			{
				query = query.Where(na => na.contentFileName == contentFileName);
			}
			if (contentSize.HasValue == true)
			{
				query = query.Where(na => na.contentSize == contentSize.Value);
			}
			if (string.IsNullOrEmpty(contentMimeType) == false)
			{
				query = query.Where(na => na.contentMimeType == contentMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(na => na.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(na => na.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(na => na.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(na => na.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(na => na.deleted == false);
				}
			}
			else
			{
				query = query.Where(na => na.active == true);
				query = query.Where(na => na.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Notification Attachment, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.contentFileName.Contains(anyStringContains)
			       || x.contentMimeType.Contains(anyStringContains)
			       || x.notification.message.Contains(anyStringContains)
			       || x.notification.entity.Contains(anyStringContains)
			       || x.notification.externalURL.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.contentFileName).ThenBy(x => x.contentMimeType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.NotificationAttachment.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/NotificationAttachment/CreateAuditEvent")]
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




        [Route("api/NotificationAttachment/Data/{id:int}")]
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


            Database.NotificationAttachment notificationAttachment = await (from x in _context.NotificationAttachments where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (notificationAttachment == null)
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

						lock (notificationAttachmentPutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									notificationAttachment.contentFileName = fileName.Trim();
									notificationAttachment.contentMimeType = mimeType;
									notificationAttachment.contentSize = section.Body.Length;

									notificationAttachment.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 notificationAttachment.contentFileName != null &&
										 notificationAttachment.contentSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(notificationAttachment.objectGuid, notificationAttachment.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										notificationAttachment.contentData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											notificationAttachment.contentData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									NotificationAttachmentChangeHistory notificationAttachmentChangeHistory = new NotificationAttachmentChangeHistory();
									notificationAttachmentChangeHistory.notificationAttachmentId = notificationAttachment.id;
									notificationAttachmentChangeHistory.versionNumber = notificationAttachment.versionNumber;
									notificationAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
									notificationAttachmentChangeHistory.userId = securityUser.id;
									notificationAttachmentChangeHistory.tenantGuid = notificationAttachment.tenantGuid;
									notificationAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment));
									_context.NotificationAttachmentChangeHistories.Add(notificationAttachmentChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "NotificationAttachment Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "NotificationAttachment Data Upload Failed.", false, id.ToString(), "", "", ex);

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
                lock (notificationAttachmentPutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(notificationAttachment.objectGuid, notificationAttachment.versionNumber, "data");
                            }

                            notificationAttachment.contentFileName = null;
                            notificationAttachment.contentMimeType = null;
                            notificationAttachment.contentSize = 0;
                            notificationAttachment.contentData = null;
                            notificationAttachment.versionNumber++;


                            //
                            // Now add the change history
                            //
                            NotificationAttachmentChangeHistory notificationAttachmentChangeHistory = new NotificationAttachmentChangeHistory();
                            notificationAttachmentChangeHistory.notificationAttachmentId = notificationAttachment.id;
                            notificationAttachmentChangeHistory.versionNumber = notificationAttachment.versionNumber;
                            notificationAttachmentChangeHistory.timeStamp = DateTime.UtcNow;
                            notificationAttachmentChangeHistory.userId = securityUser.id;
                                    notificationAttachmentChangeHistory.tenantGuid = notificationAttachment.tenantGuid;
                                    notificationAttachmentChangeHistory.data = JsonSerializer.Serialize(Database.NotificationAttachment.CreateAnonymousWithFirstLevelSubObjects(notificationAttachment));
                            _context.NotificationAttachmentChangeHistories.Add(notificationAttachmentChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "NotificationAttachment data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "NotificationAttachment data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/NotificationAttachment/Data/{id:int}")]
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
                Database.NotificationAttachment notificationAttachment = await (from d in context.NotificationAttachments
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (notificationAttachment != null && notificationAttachment.contentData != null)
                {
                   return File(notificationAttachment.contentData.ToArray<byte>(), notificationAttachment.contentMimeType, notificationAttachment.contentFileName != null ? notificationAttachment.contentFileName.Trim() : "NotificationAttachment_" + notificationAttachment.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
