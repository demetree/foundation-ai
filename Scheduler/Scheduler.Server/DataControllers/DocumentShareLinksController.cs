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
    /// This auto generated class provides the basic CRUD operations for the DocumentShareLink entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the DocumentShareLink entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class DocumentShareLinksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object documentShareLinkPutSyncRoot = new object();
		static object documentShareLinkDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<DocumentShareLinksController> _logger;

		public DocumentShareLinksController(SchedulerContext context, ILogger<DocumentShareLinksController> logger) : base("Scheduler", "DocumentShareLink")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of DocumentShareLinks filtered by the parameters provided.
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
		[Route("api/DocumentShareLinks")]
		public async Task<IActionResult> GetDocumentShareLinks(
			int? documentId = null,
			Guid? token = null,
			string passwordHash = null,
			DateTime? expiresAt = null,
			int? maxDownloads = null,
			int? downloadCount = null,
			string createdBy = null,
			DateTime? createdDate = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (expiresAt.HasValue == true && expiresAt.Value.Kind != DateTimeKind.Utc)
			{
				expiresAt = expiresAt.Value.ToUniversalTime();
			}

			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			IQueryable<Database.DocumentShareLink> query = (from dsl in _context.DocumentShareLinks select dsl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (documentId.HasValue == true)
			{
				query = query.Where(dsl => dsl.documentId == documentId.Value);
			}
			if (token.HasValue == true)
			{
				query = query.Where(dsl => dsl.token == token);
			}
			if (string.IsNullOrEmpty(passwordHash) == false)
			{
				query = query.Where(dsl => dsl.passwordHash == passwordHash);
			}
			if (expiresAt.HasValue == true)
			{
				query = query.Where(dsl => dsl.expiresAt == expiresAt.Value);
			}
			if (maxDownloads.HasValue == true)
			{
				query = query.Where(dsl => dsl.maxDownloads == maxDownloads.Value);
			}
			if (downloadCount.HasValue == true)
			{
				query = query.Where(dsl => dsl.downloadCount == downloadCount.Value);
			}
			if (string.IsNullOrEmpty(createdBy) == false)
			{
				query = query.Where(dsl => dsl.createdBy == createdBy);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(dsl => dsl.createdDate == createdDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(dsl => dsl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(dsl => dsl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(dsl => dsl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(dsl => dsl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(dsl => dsl.deleted == false);
				}
			}
			else
			{
				query = query.Where(dsl => dsl.active == true);
				query = query.Where(dsl => dsl.deleted == false);
			}

			query = query.OrderBy(dsl => dsl.passwordHash).ThenBy(dsl => dsl.createdBy);


			//
			// Add the any string contains parameter to span all the string fields on the Document Share Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.passwordHash.Contains(anyStringContains)
			       || x.createdBy.Contains(anyStringContains)
			       || (includeRelations == true && x.document.name.Contains(anyStringContains))
			       || (includeRelations == true && x.document.description.Contains(anyStringContains))
			       || (includeRelations == true && x.document.fileName.Contains(anyStringContains))
			       || (includeRelations == true && x.document.mimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.document.fileDataFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.document.fileDataMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.document.status.Contains(anyStringContains))
			       || (includeRelations == true && x.document.statusChangedBy.Contains(anyStringContains))
			       || (includeRelations == true && x.document.uploadedBy.Contains(anyStringContains))
			       || (includeRelations == true && x.document.notes.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.document);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.DocumentShareLink> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.DocumentShareLink documentShareLink in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(documentShareLink, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.DocumentShareLink Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.DocumentShareLink Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of DocumentShareLinks filtered by the parameters provided.  Its query is similar to the GetDocumentShareLinks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentShareLinks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? documentId = null,
			Guid? token = null,
			string passwordHash = null,
			DateTime? expiresAt = null,
			int? maxDownloads = null,
			int? downloadCount = null,
			string createdBy = null,
			DateTime? createdDate = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (expiresAt.HasValue == true && expiresAt.Value.Kind != DateTimeKind.Utc)
			{
				expiresAt = expiresAt.Value.ToUniversalTime();
			}

			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			IQueryable<Database.DocumentShareLink> query = (from dsl in _context.DocumentShareLinks select dsl);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (documentId.HasValue == true)
			{
				query = query.Where(dsl => dsl.documentId == documentId.Value);
			}
			if (token.HasValue == true)
			{
				query = query.Where(dsl => dsl.token == token);
			}
			if (passwordHash != null)
			{
				query = query.Where(dsl => dsl.passwordHash == passwordHash);
			}
			if (expiresAt.HasValue == true)
			{
				query = query.Where(dsl => dsl.expiresAt == expiresAt.Value);
			}
			if (maxDownloads.HasValue == true)
			{
				query = query.Where(dsl => dsl.maxDownloads == maxDownloads.Value);
			}
			if (downloadCount.HasValue == true)
			{
				query = query.Where(dsl => dsl.downloadCount == downloadCount.Value);
			}
			if (createdBy != null)
			{
				query = query.Where(dsl => dsl.createdBy == createdBy);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(dsl => dsl.createdDate == createdDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(dsl => dsl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(dsl => dsl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(dsl => dsl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(dsl => dsl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(dsl => dsl.deleted == false);
				}
			}
			else
			{
				query = query.Where(dsl => dsl.active == true);
				query = query.Where(dsl => dsl.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Document Share Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.passwordHash.Contains(anyStringContains)
			       || x.createdBy.Contains(anyStringContains)
			       || x.document.name.Contains(anyStringContains)
			       || x.document.description.Contains(anyStringContains)
			       || x.document.fileName.Contains(anyStringContains)
			       || x.document.mimeType.Contains(anyStringContains)
			       || x.document.fileDataFileName.Contains(anyStringContains)
			       || x.document.fileDataMimeType.Contains(anyStringContains)
			       || x.document.status.Contains(anyStringContains)
			       || x.document.statusChangedBy.Contains(anyStringContains)
			       || x.document.uploadedBy.Contains(anyStringContains)
			       || x.document.notes.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single DocumentShareLink by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentShareLink/{id}")]
		public async Task<IActionResult> GetDocumentShareLink(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.DocumentShareLink> query = (from dsl in _context.DocumentShareLinks where
							(dsl.id == id) &&
							(userIsAdmin == true || dsl.deleted == false) &&
							(userIsWriter == true || dsl.active == true)
					select dsl);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.document);
					query = query.AsSplitQuery();
				}

				Database.DocumentShareLink materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.DocumentShareLink Entity was read with Admin privilege." : "Scheduler.DocumentShareLink Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "DocumentShareLink", materialized.id, materialized.passwordHash));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.DocumentShareLink entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.DocumentShareLink.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.DocumentShareLink.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing DocumentShareLink record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/DocumentShareLink/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutDocumentShareLink(int id, [FromBody]Database.DocumentShareLink.DocumentShareLinkDTO documentShareLinkDTO, CancellationToken cancellationToken = default)
		{
			if (documentShareLinkDTO == null)
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



			if (id != documentShareLinkDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.DocumentShareLink> query = (from x in _context.DocumentShareLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.DocumentShareLink existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.DocumentShareLink PUT", id.ToString(), new Exception("No Scheduler.DocumentShareLink entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (documentShareLinkDTO.objectGuid == Guid.Empty)
            {
                documentShareLinkDTO.objectGuid = existing.objectGuid;
            }
            else if (documentShareLinkDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a DocumentShareLink record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.DocumentShareLink cloneOfExisting = (Database.DocumentShareLink)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new DocumentShareLink object using the data from the existing record, updated with what is in the DTO.
			//
			Database.DocumentShareLink documentShareLink = (Database.DocumentShareLink)_context.Entry(existing).GetDatabaseValues().ToObject();
			documentShareLink.ApplyDTO(documentShareLinkDTO);
			//
			// The tenant guid for any DocumentShareLink being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the DocumentShareLink because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				documentShareLink.tenantGuid = existing.tenantGuid;
			}

			lock (documentShareLinkPutSyncRoot)
			{
				//
				// Validate the version number for the documentShareLink being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != documentShareLink.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "DocumentShareLink save attempt was made but save request was with version " + documentShareLink.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The DocumentShareLink you are trying to update has already changed.  Please try your save again after reloading the DocumentShareLink.");
				}
				else
				{
					// Same record.  Increase version.
					documentShareLink.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (documentShareLink.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.DocumentShareLink record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (documentShareLink.passwordHash != null && documentShareLink.passwordHash.Length > 250)
				{
					documentShareLink.passwordHash = documentShareLink.passwordHash.Substring(0, 250);
				}

				if (documentShareLink.expiresAt.HasValue == true && documentShareLink.expiresAt.Value.Kind != DateTimeKind.Utc)
				{
					documentShareLink.expiresAt = documentShareLink.expiresAt.Value.ToUniversalTime();
				}

				if (documentShareLink.createdBy != null && documentShareLink.createdBy.Length > 250)
				{
					documentShareLink.createdBy = documentShareLink.createdBy.Substring(0, 250);
				}

				if (documentShareLink.createdDate.Kind != DateTimeKind.Utc)
				{
					documentShareLink.createdDate = documentShareLink.createdDate.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.DocumentShareLink> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(documentShareLink);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        DocumentShareLinkChangeHistory documentShareLinkChangeHistory = new DocumentShareLinkChangeHistory();
				        documentShareLinkChangeHistory.documentShareLinkId = documentShareLink.id;
				        documentShareLinkChangeHistory.versionNumber = documentShareLink.versionNumber;
				        documentShareLinkChangeHistory.timeStamp = DateTime.UtcNow;
				        documentShareLinkChangeHistory.userId = securityUser.id;
				        documentShareLinkChangeHistory.tenantGuid = userTenantGuid;
				        documentShareLinkChangeHistory.data = JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink));
				        _context.DocumentShareLinkChangeHistories.Add(documentShareLinkChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.DocumentShareLink entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink)),
						null);

				return Ok(Database.DocumentShareLink.CreateAnonymous(documentShareLink));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.DocumentShareLink entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new DocumentShareLink record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentShareLink", Name = "DocumentShareLink")]
		public async Task<IActionResult> PostDocumentShareLink([FromBody]Database.DocumentShareLink.DocumentShareLinkDTO documentShareLinkDTO, CancellationToken cancellationToken = default)
		{
			if (documentShareLinkDTO == null)
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
			// Create a new DocumentShareLink object using the data from the DTO
			//
			Database.DocumentShareLink documentShareLink = Database.DocumentShareLink.FromDTO(documentShareLinkDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				documentShareLink.tenantGuid = userTenantGuid;

				if (documentShareLink.passwordHash != null && documentShareLink.passwordHash.Length > 250)
				{
					documentShareLink.passwordHash = documentShareLink.passwordHash.Substring(0, 250);
				}

				if (documentShareLink.expiresAt.HasValue == true && documentShareLink.expiresAt.Value.Kind != DateTimeKind.Utc)
				{
					documentShareLink.expiresAt = documentShareLink.expiresAt.Value.ToUniversalTime();
				}

				if (documentShareLink.createdBy != null && documentShareLink.createdBy.Length > 250)
				{
					documentShareLink.createdBy = documentShareLink.createdBy.Substring(0, 250);
				}

				if (documentShareLink.createdDate.Kind != DateTimeKind.Utc)
				{
					documentShareLink.createdDate = documentShareLink.createdDate.ToUniversalTime();
				}

				documentShareLink.objectGuid = Guid.NewGuid();
				documentShareLink.versionNumber = 1;

				_context.DocumentShareLinks.Add(documentShareLink);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the documentShareLink object so that no further changes will be written to the database
				    //
				    _context.Entry(documentShareLink).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					documentShareLink.DocumentShareLinkChangeHistories = null;
					documentShareLink.document = null;


				    DocumentShareLinkChangeHistory documentShareLinkChangeHistory = new DocumentShareLinkChangeHistory();
				    documentShareLinkChangeHistory.documentShareLinkId = documentShareLink.id;
				    documentShareLinkChangeHistory.versionNumber = documentShareLink.versionNumber;
				    documentShareLinkChangeHistory.timeStamp = DateTime.UtcNow;
				    documentShareLinkChangeHistory.userId = securityUser.id;
				    documentShareLinkChangeHistory.tenantGuid = userTenantGuid;
				    documentShareLinkChangeHistory.data = JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink));
				    _context.DocumentShareLinkChangeHistories.Add(documentShareLinkChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.DocumentShareLink entity successfully created.",
						true,
						documentShareLink. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.DocumentShareLink entity creation failed.", false, documentShareLink.id.ToString(), "", JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "DocumentShareLink", documentShareLink.id, documentShareLink.passwordHash));

			return CreatedAtRoute("DocumentShareLink", new { id = documentShareLink.id }, Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink));
		}



        /// <summary>
        /// 
        /// This rolls a DocumentShareLink entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentShareLink/Rollback/{id}")]
		[Route("api/DocumentShareLink/Rollback")]
		public async Task<IActionResult> RollbackToDocumentShareLinkVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.DocumentShareLink> query = (from x in _context.DocumentShareLinks
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this DocumentShareLink concurrently
			//
			lock (documentShareLinkPutSyncRoot)
			{
				
				Database.DocumentShareLink documentShareLink = query.FirstOrDefault();
				
				if (documentShareLink == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.DocumentShareLink rollback", id.ToString(), new Exception("No Scheduler.DocumentShareLink entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the DocumentShareLink current state so we can log it.
				//
				Database.DocumentShareLink cloneOfExisting = (Database.DocumentShareLink)_context.Entry(documentShareLink).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.DocumentShareLinkChangeHistories = null;
				cloneOfExisting.document = null;

				if (versionNumber >= documentShareLink.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.DocumentShareLink rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.DocumentShareLink rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				DocumentShareLinkChangeHistory documentShareLinkChangeHistory = (from x in _context.DocumentShareLinkChangeHistories
				                                               where
				                                               x.documentShareLinkId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (documentShareLinkChangeHistory != null)
				{
				    Database.DocumentShareLink oldDocumentShareLink = JsonSerializer.Deserialize<Database.DocumentShareLink>(documentShareLinkChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    documentShareLink.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    documentShareLink.documentId = oldDocumentShareLink.documentId;
				    documentShareLink.token = oldDocumentShareLink.token;
				    documentShareLink.passwordHash = oldDocumentShareLink.passwordHash;
				    documentShareLink.expiresAt = oldDocumentShareLink.expiresAt;
				    documentShareLink.maxDownloads = oldDocumentShareLink.maxDownloads;
				    documentShareLink.downloadCount = oldDocumentShareLink.downloadCount;
				    documentShareLink.createdBy = oldDocumentShareLink.createdBy;
				    documentShareLink.createdDate = oldDocumentShareLink.createdDate;
				    documentShareLink.objectGuid = oldDocumentShareLink.objectGuid;
				    documentShareLink.active = oldDocumentShareLink.active;
				    documentShareLink.deleted = oldDocumentShareLink.deleted;

				    string serializedDocumentShareLink = JsonSerializer.Serialize(documentShareLink);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        DocumentShareLinkChangeHistory newDocumentShareLinkChangeHistory = new DocumentShareLinkChangeHistory();
				        newDocumentShareLinkChangeHistory.documentShareLinkId = documentShareLink.id;
				        newDocumentShareLinkChangeHistory.versionNumber = documentShareLink.versionNumber;
				        newDocumentShareLinkChangeHistory.timeStamp = DateTime.UtcNow;
				        newDocumentShareLinkChangeHistory.userId = securityUser.id;
				        newDocumentShareLinkChangeHistory.tenantGuid = userTenantGuid;
				        newDocumentShareLinkChangeHistory.data = JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink));
				        _context.DocumentShareLinkChangeHistories.Add(newDocumentShareLinkChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.DocumentShareLink rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink)),
						null);


				    return Ok(Database.DocumentShareLink.CreateAnonymous(documentShareLink));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.DocumentShareLink rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.DocumentShareLink rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a DocumentShareLink.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the DocumentShareLink</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentShareLink/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetDocumentShareLinkChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.DocumentShareLink documentShareLink = await _context.DocumentShareLinks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (documentShareLink == null)
			{
				return NotFound();
			}

			try
			{
				documentShareLink.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.DocumentShareLink> versionInfo = await documentShareLink.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a DocumentShareLink.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the DocumentShareLink</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentShareLink/{id}/AuditHistory")]
		public async Task<IActionResult> GetDocumentShareLinkAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.DocumentShareLink documentShareLink = await _context.DocumentShareLinks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (documentShareLink == null)
			{
				return NotFound();
			}

			try
			{
				documentShareLink.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.DocumentShareLink>> versions = await documentShareLink.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a DocumentShareLink.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the DocumentShareLink</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The DocumentShareLink object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentShareLink/{id}/Version/{version}")]
		public async Task<IActionResult> GetDocumentShareLinkVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.DocumentShareLink documentShareLink = await _context.DocumentShareLinks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (documentShareLink == null)
			{
				return NotFound();
			}

			try
			{
				documentShareLink.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.DocumentShareLink> versionInfo = await documentShareLink.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a DocumentShareLink at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the DocumentShareLink</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The DocumentShareLink object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentShareLink/{id}/StateAtTime")]
		public async Task<IActionResult> GetDocumentShareLinkStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.DocumentShareLink documentShareLink = await _context.DocumentShareLinks.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (documentShareLink == null)
			{
				return NotFound();
			}

			try
			{
				documentShareLink.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.DocumentShareLink> versionInfo = await documentShareLink.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a DocumentShareLink record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DocumentShareLink/{id}")]
		[Route("api/DocumentShareLink")]
		public async Task<IActionResult> DeleteDocumentShareLink(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.DocumentShareLink> query = (from x in _context.DocumentShareLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.DocumentShareLink documentShareLink = await query.FirstOrDefaultAsync(cancellationToken);

			if (documentShareLink == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.DocumentShareLink DELETE", id.ToString(), new Exception("No Scheduler.DocumentShareLink entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.DocumentShareLink cloneOfExisting = (Database.DocumentShareLink)_context.Entry(documentShareLink).GetDatabaseValues().ToObject();


			lock (documentShareLinkDeleteSyncRoot)
			{
			    try
			    {
			        documentShareLink.deleted = true;
			        documentShareLink.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        DocumentShareLinkChangeHistory documentShareLinkChangeHistory = new DocumentShareLinkChangeHistory();
			        documentShareLinkChangeHistory.documentShareLinkId = documentShareLink.id;
			        documentShareLinkChangeHistory.versionNumber = documentShareLink.versionNumber;
			        documentShareLinkChangeHistory.timeStamp = DateTime.UtcNow;
			        documentShareLinkChangeHistory.userId = securityUser.id;
			        documentShareLinkChangeHistory.tenantGuid = userTenantGuid;
			        documentShareLinkChangeHistory.data = JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink));
			        _context.DocumentShareLinkChangeHistories.Add(documentShareLinkChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.DocumentShareLink entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.DocumentShareLink entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.DocumentShareLink.CreateAnonymousWithFirstLevelSubObjects(documentShareLink)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of DocumentShareLink records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/DocumentShareLinks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? documentId = null,
			Guid? token = null,
			string passwordHash = null,
			DateTime? expiresAt = null,
			int? maxDownloads = null,
			int? downloadCount = null,
			string createdBy = null,
			DateTime? createdDate = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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
			if (expiresAt.HasValue == true && expiresAt.Value.Kind != DateTimeKind.Utc)
			{
				expiresAt = expiresAt.Value.ToUniversalTime();
			}

			if (createdDate.HasValue == true && createdDate.Value.Kind != DateTimeKind.Utc)
			{
				createdDate = createdDate.Value.ToUniversalTime();
			}

			IQueryable<Database.DocumentShareLink> query = (from dsl in _context.DocumentShareLinks select dsl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (documentId.HasValue == true)
			{
				query = query.Where(dsl => dsl.documentId == documentId.Value);
			}
			if (token.HasValue == true)
			{
				query = query.Where(dsl => dsl.token == token);
			}
			if (string.IsNullOrEmpty(passwordHash) == false)
			{
				query = query.Where(dsl => dsl.passwordHash == passwordHash);
			}
			if (expiresAt.HasValue == true)
			{
				query = query.Where(dsl => dsl.expiresAt == expiresAt.Value);
			}
			if (maxDownloads.HasValue == true)
			{
				query = query.Where(dsl => dsl.maxDownloads == maxDownloads.Value);
			}
			if (downloadCount.HasValue == true)
			{
				query = query.Where(dsl => dsl.downloadCount == downloadCount.Value);
			}
			if (string.IsNullOrEmpty(createdBy) == false)
			{
				query = query.Where(dsl => dsl.createdBy == createdBy);
			}
			if (createdDate.HasValue == true)
			{
				query = query.Where(dsl => dsl.createdDate == createdDate.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(dsl => dsl.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(dsl => dsl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(dsl => dsl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(dsl => dsl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(dsl => dsl.deleted == false);
				}
			}
			else
			{
				query = query.Where(dsl => dsl.active == true);
				query = query.Where(dsl => dsl.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Document Share Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.passwordHash.Contains(anyStringContains)
			       || x.createdBy.Contains(anyStringContains)
			       || x.document.name.Contains(anyStringContains)
			       || x.document.description.Contains(anyStringContains)
			       || x.document.fileName.Contains(anyStringContains)
			       || x.document.mimeType.Contains(anyStringContains)
			       || x.document.fileDataFileName.Contains(anyStringContains)
			       || x.document.fileDataMimeType.Contains(anyStringContains)
			       || x.document.status.Contains(anyStringContains)
			       || x.document.statusChangedBy.Contains(anyStringContains)
			       || x.document.uploadedBy.Contains(anyStringContains)
			       || x.document.notes.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.passwordHash).ThenBy(x => x.createdBy);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.DocumentShareLink.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/DocumentShareLink/CreateAuditEvent")]
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
