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
using Foundation.BMC.Database;
using Foundation.ChangeHistory;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the MocVersion entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MocVersion entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MocVersionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object mocVersionPutSyncRoot = new object();
		static object mocVersionDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<MocVersionsController> _logger;

		public MocVersionsController(BMCContext context, ILogger<MocVersionsController> logger) : base("BMC", "MocVersion")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MocVersions filtered by the parameters provided.
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
		[Route("api/MocVersions")]
		public async Task<IActionResult> GetMocVersions(
			int? publishedMocId = null,
			int? versionNumber = null,
			string commitMessage = null,
			string mpdSnapshot = null,
			int? partCount = null,
			int? addedPartCount = null,
			int? removedPartCount = null,
			int? modifiedPartCount = null,
			DateTime? snapshotDate = null,
			Guid? authorTenantGuid = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (snapshotDate.HasValue == true && snapshotDate.Value.Kind != DateTimeKind.Utc)
			{
				snapshotDate = snapshotDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocVersion> query = (from mv in _context.MocVersions select mv);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (publishedMocId.HasValue == true)
			{
				query = query.Where(mv => mv.publishedMocId == publishedMocId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(mv => mv.versionNumber == versionNumber.Value);
			}
			if (string.IsNullOrEmpty(commitMessage) == false)
			{
				query = query.Where(mv => mv.commitMessage == commitMessage);
			}
			if (string.IsNullOrEmpty(mpdSnapshot) == false)
			{
				query = query.Where(mv => mv.mpdSnapshot == mpdSnapshot);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(mv => mv.partCount == partCount.Value);
			}
			if (addedPartCount.HasValue == true)
			{
				query = query.Where(mv => mv.addedPartCount == addedPartCount.Value);
			}
			if (removedPartCount.HasValue == true)
			{
				query = query.Where(mv => mv.removedPartCount == removedPartCount.Value);
			}
			if (modifiedPartCount.HasValue == true)
			{
				query = query.Where(mv => mv.modifiedPartCount == modifiedPartCount.Value);
			}
			if (snapshotDate.HasValue == true)
			{
				query = query.Where(mv => mv.snapshotDate == snapshotDate.Value);
			}
			if (authorTenantGuid.HasValue == true)
			{
				query = query.Where(mv => mv.authorTenantGuid == authorTenantGuid);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mv => mv.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mv => mv.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mv => mv.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mv => mv.deleted == false);
				}
			}
			else
			{
				query = query.Where(mv => mv.active == true);
				query = query.Where(mv => mv.deleted == false);
			}

			query = query.OrderBy(mv => mv.commitMessage);


			//
			// Add the any string contains parameter to span all the string fields on the Moc Version, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.commitMessage.Contains(anyStringContains)
			       || x.mpdSnapshot.Contains(anyStringContains)
			       || (includeRelations == true && x.publishedMoc.name.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.description.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.thumbnailImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.tags.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.visibility.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.licenseName.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.readmeMarkdown.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.slug.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.defaultBranchName.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.publishedMoc);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.MocVersion> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MocVersion mocVersion in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(mocVersion, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.MocVersion Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.MocVersion Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MocVersions filtered by the parameters provided.  Its query is similar to the GetMocVersions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocVersions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? publishedMocId = null,
			int? versionNumber = null,
			string commitMessage = null,
			string mpdSnapshot = null,
			int? partCount = null,
			int? addedPartCount = null,
			int? removedPartCount = null,
			int? modifiedPartCount = null,
			DateTime? snapshotDate = null,
			Guid? authorTenantGuid = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (snapshotDate.HasValue == true && snapshotDate.Value.Kind != DateTimeKind.Utc)
			{
				snapshotDate = snapshotDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocVersion> query = (from mv in _context.MocVersions select mv);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(mv => mv.publishedMocId == publishedMocId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(mv => mv.versionNumber == versionNumber.Value);
			}
			if (commitMessage != null)
			{
				query = query.Where(mv => mv.commitMessage == commitMessage);
			}
			if (mpdSnapshot != null)
			{
				query = query.Where(mv => mv.mpdSnapshot == mpdSnapshot);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(mv => mv.partCount == partCount.Value);
			}
			if (addedPartCount.HasValue == true)
			{
				query = query.Where(mv => mv.addedPartCount == addedPartCount.Value);
			}
			if (removedPartCount.HasValue == true)
			{
				query = query.Where(mv => mv.removedPartCount == removedPartCount.Value);
			}
			if (modifiedPartCount.HasValue == true)
			{
				query = query.Where(mv => mv.modifiedPartCount == modifiedPartCount.Value);
			}
			if (snapshotDate.HasValue == true)
			{
				query = query.Where(mv => mv.snapshotDate == snapshotDate.Value);
			}
			if (authorTenantGuid.HasValue == true)
			{
				query = query.Where(mv => mv.authorTenantGuid == authorTenantGuid);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mv => mv.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mv => mv.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mv => mv.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mv => mv.deleted == false);
				}
			}
			else
			{
				query = query.Where(mv => mv.active == true);
				query = query.Where(mv => mv.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Moc Version, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.commitMessage.Contains(anyStringContains)
			       || x.mpdSnapshot.Contains(anyStringContains)
			       || x.publishedMoc.name.Contains(anyStringContains)
			       || x.publishedMoc.description.Contains(anyStringContains)
			       || x.publishedMoc.thumbnailImagePath.Contains(anyStringContains)
			       || x.publishedMoc.tags.Contains(anyStringContains)
			       || x.publishedMoc.visibility.Contains(anyStringContains)
			       || x.publishedMoc.licenseName.Contains(anyStringContains)
			       || x.publishedMoc.readmeMarkdown.Contains(anyStringContains)
			       || x.publishedMoc.slug.Contains(anyStringContains)
			       || x.publishedMoc.defaultBranchName.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MocVersion by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocVersion/{id}")]
		public async Task<IActionResult> GetMocVersion(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.MocVersion> query = (from mv in _context.MocVersions where
							(mv.id == id) &&
							(userIsAdmin == true || mv.deleted == false) &&
							(userIsWriter == true || mv.active == true)
					select mv);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.publishedMoc);
					query = query.AsSplitQuery();
				}

				Database.MocVersion materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.MocVersion Entity was read with Admin privilege." : "BMC.MocVersion Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MocVersion", materialized.id, materialized.commitMessage));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.MocVersion entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.MocVersion.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.MocVersion.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MocVersion record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MocVersion/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMocVersion(int id, [FromBody]Database.MocVersion.MocVersionDTO mocVersionDTO, CancellationToken cancellationToken = default)
		{
			if (mocVersionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != mocVersionDTO.id)
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


			IQueryable<Database.MocVersion> query = (from x in _context.MocVersions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MocVersion existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocVersion PUT", id.ToString(), new Exception("No BMC.MocVersion entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (mocVersionDTO.objectGuid == Guid.Empty)
            {
                mocVersionDTO.objectGuid = existing.objectGuid;
            }
            else if (mocVersionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MocVersion record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MocVersion cloneOfExisting = (Database.MocVersion)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MocVersion object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MocVersion mocVersion = (Database.MocVersion)_context.Entry(existing).GetDatabaseValues().ToObject();
			mocVersion.ApplyDTO(mocVersionDTO);
			//
			// The tenant guid for any MocVersion being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the MocVersion because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				mocVersion.tenantGuid = existing.tenantGuid;
			}

			lock (mocVersionPutSyncRoot)
			{
				//
				// Validate the version number for the mocVersion being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != mocVersion.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "MocVersion save attempt was made but save request was with version " + mocVersion.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The MocVersion you are trying to update has already changed.  Please try your save again after reloading the MocVersion.");
				}
				else
				{
					// Same record.  Increase version.
					mocVersion.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (mocVersion.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.MocVersion record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (mocVersion.commitMessage != null && mocVersion.commitMessage.Length > 500)
				{
					mocVersion.commitMessage = mocVersion.commitMessage.Substring(0, 500);
				}

				if (mocVersion.snapshotDate.Kind != DateTimeKind.Utc)
				{
					mocVersion.snapshotDate = mocVersion.snapshotDate.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.MocVersion> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(mocVersion);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        MocVersionChangeHistory mocVersionChangeHistory = new MocVersionChangeHistory();
				        mocVersionChangeHistory.mocVersionId = mocVersion.id;
				        mocVersionChangeHistory.versionNumber = mocVersion.versionNumber;
				        mocVersionChangeHistory.timeStamp = DateTime.UtcNow;
				        mocVersionChangeHistory.userId = securityUser.id;
				        mocVersionChangeHistory.tenantGuid = userTenantGuid;
				        mocVersionChangeHistory.data = JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion));
				        _context.MocVersionChangeHistories.Add(mocVersionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.MocVersion entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion)),
						null);

				return Ok(Database.MocVersion.CreateAnonymous(mocVersion));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.MocVersion entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new MocVersion record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocVersion", Name = "MocVersion")]
		public async Task<IActionResult> PostMocVersion([FromBody]Database.MocVersion.MocVersionDTO mocVersionDTO, CancellationToken cancellationToken = default)
		{
			if (mocVersionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new MocVersion object using the data from the DTO
			//
			Database.MocVersion mocVersion = Database.MocVersion.FromDTO(mocVersionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				mocVersion.tenantGuid = userTenantGuid;

				if (mocVersion.commitMessage != null && mocVersion.commitMessage.Length > 500)
				{
					mocVersion.commitMessage = mocVersion.commitMessage.Substring(0, 500);
				}

				if (mocVersion.snapshotDate.Kind != DateTimeKind.Utc)
				{
					mocVersion.snapshotDate = mocVersion.snapshotDate.ToUniversalTime();
				}

				mocVersion.objectGuid = Guid.NewGuid();
				mocVersion.versionNumber = 1;

				_context.MocVersions.Add(mocVersion);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the mocVersion object so that no further changes will be written to the database
				    //
				    _context.Entry(mocVersion).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					mocVersion.MocForks = null;
					mocVersion.publishedMoc = null;


				    MocVersionChangeHistory mocVersionChangeHistory = new MocVersionChangeHistory();
				    mocVersionChangeHistory.mocVersionId = mocVersion.id;
				    mocVersionChangeHistory.versionNumber = mocVersion.versionNumber;
				    mocVersionChangeHistory.timeStamp = DateTime.UtcNow;
				    mocVersionChangeHistory.userId = securityUser.id;
				    mocVersionChangeHistory.tenantGuid = userTenantGuid;
				    mocVersionChangeHistory.data = JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion));
				    _context.MocVersionChangeHistories.Add(mocVersionChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.MocVersion entity successfully created.",
						true,
						mocVersion. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.MocVersion entity creation failed.", false, mocVersion.id.ToString(), "", JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MocVersion", mocVersion.id, mocVersion.commitMessage));

			return CreatedAtRoute("MocVersion", new { id = mocVersion.id }, Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion));
		}



        /// <summary>
        /// 
        /// This rolls a MocVersion entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocVersion/Rollback/{id}")]
		[Route("api/MocVersion/Rollback")]
		public async Task<IActionResult> RollbackToMocVersionVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.MocVersion> query = (from x in _context.MocVersions
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this MocVersion concurrently
			//
			lock (mocVersionPutSyncRoot)
			{
				
				Database.MocVersion mocVersion = query.FirstOrDefault();
				
				if (mocVersion == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocVersion rollback", id.ToString(), new Exception("No BMC.MocVersion entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the MocVersion current state so we can log it.
				//
				Database.MocVersion cloneOfExisting = (Database.MocVersion)_context.Entry(mocVersion).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.MocForks = null;
				cloneOfExisting.publishedMoc = null;

				if (versionNumber >= mocVersion.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.MocVersion rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.MocVersion rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				MocVersionChangeHistory mocVersionChangeHistory = (from x in _context.MocVersionChangeHistories
				                                               where
				                                               x.mocVersionId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (mocVersionChangeHistory != null)
				{
				    Database.MocVersion oldMocVersion = JsonSerializer.Deserialize<Database.MocVersion>(mocVersionChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    mocVersion.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    mocVersion.publishedMocId = oldMocVersion.publishedMocId;
				    mocVersion.commitMessage = oldMocVersion.commitMessage;
				    mocVersion.mpdSnapshot = oldMocVersion.mpdSnapshot;
				    mocVersion.partCount = oldMocVersion.partCount;
				    mocVersion.addedPartCount = oldMocVersion.addedPartCount;
				    mocVersion.removedPartCount = oldMocVersion.removedPartCount;
				    mocVersion.modifiedPartCount = oldMocVersion.modifiedPartCount;
				    mocVersion.snapshotDate = oldMocVersion.snapshotDate;
				    mocVersion.authorTenantGuid = oldMocVersion.authorTenantGuid;
				    mocVersion.objectGuid = oldMocVersion.objectGuid;
				    mocVersion.active = oldMocVersion.active;
				    mocVersion.deleted = oldMocVersion.deleted;

				    string serializedMocVersion = JsonSerializer.Serialize(mocVersion);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        MocVersionChangeHistory newMocVersionChangeHistory = new MocVersionChangeHistory();
				        newMocVersionChangeHistory.mocVersionId = mocVersion.id;
				        newMocVersionChangeHistory.versionNumber = mocVersion.versionNumber;
				        newMocVersionChangeHistory.timeStamp = DateTime.UtcNow;
				        newMocVersionChangeHistory.userId = securityUser.id;
				        newMocVersionChangeHistory.tenantGuid = userTenantGuid;
				        newMocVersionChangeHistory.data = JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion));
				        _context.MocVersionChangeHistories.Add(newMocVersionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.MocVersion rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion)),
						null);


				    return Ok(Database.MocVersion.CreateAnonymous(mocVersion));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.MocVersion rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.MocVersion rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a MocVersion.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the MocVersion</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocVersion/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetMocVersionChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.MocVersion mocVersion = await _context.MocVersions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (mocVersion == null)
			{
				return NotFound();
			}

			try
			{
				mocVersion.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.MocVersion> versionInfo = await mocVersion.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a MocVersion.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the MocVersion</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocVersion/{id}/AuditHistory")]
		public async Task<IActionResult> GetMocVersionAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.MocVersion mocVersion = await _context.MocVersions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (mocVersion == null)
			{
				return NotFound();
			}

			try
			{
				mocVersion.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.MocVersion>> versions = await mocVersion.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a MocVersion.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the MocVersion</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The MocVersion object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocVersion/{id}/Version/{version}")]
		public async Task<IActionResult> GetMocVersionVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.MocVersion mocVersion = await _context.MocVersions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (mocVersion == null)
			{
				return NotFound();
			}

			try
			{
				mocVersion.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.MocVersion> versionInfo = await mocVersion.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a MocVersion at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the MocVersion</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The MocVersion object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocVersion/{id}/StateAtTime")]
		public async Task<IActionResult> GetMocVersionStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.MocVersion mocVersion = await _context.MocVersions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (mocVersion == null)
			{
				return NotFound();
			}

			try
			{
				mocVersion.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.MocVersion> versionInfo = await mocVersion.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a MocVersion record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocVersion/{id}")]
		[Route("api/MocVersion")]
		public async Task<IActionResult> DeleteMocVersion(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.MocVersion> query = (from x in _context.MocVersions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MocVersion mocVersion = await query.FirstOrDefaultAsync(cancellationToken);

			if (mocVersion == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocVersion DELETE", id.ToString(), new Exception("No BMC.MocVersion entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MocVersion cloneOfExisting = (Database.MocVersion)_context.Entry(mocVersion).GetDatabaseValues().ToObject();


			lock (mocVersionDeleteSyncRoot)
			{
			    try
			    {
			        mocVersion.deleted = true;
			        mocVersion.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        MocVersionChangeHistory mocVersionChangeHistory = new MocVersionChangeHistory();
			        mocVersionChangeHistory.mocVersionId = mocVersion.id;
			        mocVersionChangeHistory.versionNumber = mocVersion.versionNumber;
			        mocVersionChangeHistory.timeStamp = DateTime.UtcNow;
			        mocVersionChangeHistory.userId = securityUser.id;
			        mocVersionChangeHistory.tenantGuid = userTenantGuid;
			        mocVersionChangeHistory.data = JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion));
			        _context.MocVersionChangeHistories.Add(mocVersionChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.MocVersion entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.MocVersion entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.MocVersion.CreateAnonymousWithFirstLevelSubObjects(mocVersion)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of MocVersion records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MocVersions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? publishedMocId = null,
			int? versionNumber = null,
			string commitMessage = null,
			string mpdSnapshot = null,
			int? partCount = null,
			int? addedPartCount = null,
			int? removedPartCount = null,
			int? modifiedPartCount = null,
			DateTime? snapshotDate = null,
			Guid? authorTenantGuid = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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
			if (snapshotDate.HasValue == true && snapshotDate.Value.Kind != DateTimeKind.Utc)
			{
				snapshotDate = snapshotDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocVersion> query = (from mv in _context.MocVersions select mv);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (publishedMocId.HasValue == true)
			{
				query = query.Where(mv => mv.publishedMocId == publishedMocId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(mv => mv.versionNumber == versionNumber.Value);
			}
			if (string.IsNullOrEmpty(commitMessage) == false)
			{
				query = query.Where(mv => mv.commitMessage == commitMessage);
			}
			if (string.IsNullOrEmpty(mpdSnapshot) == false)
			{
				query = query.Where(mv => mv.mpdSnapshot == mpdSnapshot);
			}
			if (partCount.HasValue == true)
			{
				query = query.Where(mv => mv.partCount == partCount.Value);
			}
			if (addedPartCount.HasValue == true)
			{
				query = query.Where(mv => mv.addedPartCount == addedPartCount.Value);
			}
			if (removedPartCount.HasValue == true)
			{
				query = query.Where(mv => mv.removedPartCount == removedPartCount.Value);
			}
			if (modifiedPartCount.HasValue == true)
			{
				query = query.Where(mv => mv.modifiedPartCount == modifiedPartCount.Value);
			}
			if (snapshotDate.HasValue == true)
			{
				query = query.Where(mv => mv.snapshotDate == snapshotDate.Value);
			}
			if (authorTenantGuid.HasValue == true)
			{
				query = query.Where(mv => mv.authorTenantGuid == authorTenantGuid);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mv => mv.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mv => mv.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mv => mv.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mv => mv.deleted == false);
				}
			}
			else
			{
				query = query.Where(mv => mv.active == true);
				query = query.Where(mv => mv.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Moc Version, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.commitMessage.Contains(anyStringContains)
			       || x.mpdSnapshot.Contains(anyStringContains)
			       || x.publishedMoc.name.Contains(anyStringContains)
			       || x.publishedMoc.description.Contains(anyStringContains)
			       || x.publishedMoc.thumbnailImagePath.Contains(anyStringContains)
			       || x.publishedMoc.tags.Contains(anyStringContains)
			       || x.publishedMoc.visibility.Contains(anyStringContains)
			       || x.publishedMoc.licenseName.Contains(anyStringContains)
			       || x.publishedMoc.readmeMarkdown.Contains(anyStringContains)
			       || x.publishedMoc.slug.Contains(anyStringContains)
			       || x.publishedMoc.defaultBranchName.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.commitMessage);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MocVersion.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MocVersion/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
