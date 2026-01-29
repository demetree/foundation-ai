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
    /// This auto generated class provides the basic CRUD operations for the Tribute entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Tribute entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TributesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		static object tributePutSyncRoot = new object();
		static object tributeDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<TributesController> _logger;

		public TributesController(SchedulerContext context, ILogger<TributesController> logger) : base("Scheduler", "Tribute")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Tributes filtered by the parameters provided.
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
		[Route("api/Tributes")]
		public async Task<IActionResult> GetTributes(
			string name = null,
			string description = null,
			int? tributeTypeId = null,
			int? defaultAcknowledgeeId = null,
			int? iconId = null,
			string color = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
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

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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

			IQueryable<Database.Tribute> query = (from t in _context.Tributes select t);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(t => t.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(t => t.description == description);
			}
			if (tributeTypeId.HasValue == true)
			{
				query = query.Where(t => t.tributeTypeId == tributeTypeId.Value);
			}
			if (defaultAcknowledgeeId.HasValue == true)
			{
				query = query.Where(t => t.defaultAcknowledgeeId == defaultAcknowledgeeId.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(t => t.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(t => t.color == color);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(t => t.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(t => t.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(t => t.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(t => t.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(t => t.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(t => t.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(t => t.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(t => t.deleted == false);
				}
			}
			else
			{
				query = query.Where(t => t.active == true);
				query = query.Where(t => t.deleted == false);
			}

			query = query.OrderBy(t => t.name).ThenBy(t => t.description).ThenBy(t => t.color);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.defaultAcknowledgee);
				query = query.Include(x => x.icon);
				query = query.Include(x => x.tributeType);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Tribute, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || (includeRelations == true && x.defaultAcknowledgee.constituentNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultAcknowledgee.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultAcknowledgee.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultAcknowledgee.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultAcknowledgee.color.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultAcknowledgee.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.defaultAcknowledgee.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
			       || (includeRelations == true && x.tributeType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.tributeType.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Tribute> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Tribute tribute in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(tribute, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async tribute =>
				{

					if (tribute.avatarData == null &&
					    tribute.avatarSize.HasValue == true &&
					    tribute.avatarSize.Value > 0)
					{
					    tribute.avatarData = await LoadDataFromDiskAsync(tribute.objectGuid, tribute.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Tribute Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Tribute Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Tributes filtered by the parameters provided.  Its query is similar to the GetTributes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tributes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? tributeTypeId = null,
			int? defaultAcknowledgeeId = null,
			int? iconId = null,
			string color = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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


			IQueryable<Database.Tribute> query = (from t in _context.Tributes select t);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(t => t.name == name);
			}
			if (description != null)
			{
				query = query.Where(t => t.description == description);
			}
			if (tributeTypeId.HasValue == true)
			{
				query = query.Where(t => t.tributeTypeId == tributeTypeId.Value);
			}
			if (defaultAcknowledgeeId.HasValue == true)
			{
				query = query.Where(t => t.defaultAcknowledgeeId == defaultAcknowledgeeId.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(t => t.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(t => t.color == color);
			}
			if (avatarFileName != null)
			{
				query = query.Where(t => t.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(t => t.avatarSize == avatarSize.Value);
			}
			if (avatarMimeType != null)
			{
				query = query.Where(t => t.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(t => t.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(t => t.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(t => t.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(t => t.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(t => t.deleted == false);
				}
			}
			else
			{
				query = query.Where(t => t.active == true);
				query = query.Where(t => t.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Tribute, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.defaultAcknowledgee.constituentNumber.Contains(anyStringContains)
			       || x.defaultAcknowledgee.externalId.Contains(anyStringContains)
			       || x.defaultAcknowledgee.notes.Contains(anyStringContains)
			       || x.defaultAcknowledgee.attributes.Contains(anyStringContains)
			       || x.defaultAcknowledgee.color.Contains(anyStringContains)
			       || x.defaultAcknowledgee.avatarFileName.Contains(anyStringContains)
			       || x.defaultAcknowledgee.avatarMimeType.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			       || x.tributeType.name.Contains(anyStringContains)
			       || x.tributeType.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Tribute by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tribute/{id}")]
		public async Task<IActionResult> GetTribute(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			
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
				IQueryable<Database.Tribute> query = (from t in _context.Tributes where
							(t.id == id) &&
							(userIsAdmin == true || t.deleted == false) &&
							(userIsWriter == true || t.active == true)
					select t);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.defaultAcknowledgee);
					query = query.Include(x => x.icon);
					query = query.Include(x => x.tributeType);
					query = query.AsSplitQuery();
				}

				Database.Tribute materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.avatarData == null &&
					    materialized.avatarSize.HasValue == true &&
					    materialized.avatarSize.Value > 0)
					{
					    materialized.avatarData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Tribute Entity was read with Admin privilege." : "Scheduler.Tribute Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Tribute", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Tribute entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Tribute.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Tribute.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Tribute record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Tribute/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PutTribute(int id, [FromBody]Database.Tribute.TributeDTO tributeDTO, CancellationToken cancellationToken = default)
		{
			if (tributeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != tributeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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


			IQueryable<Database.Tribute> query = (from x in _context.Tributes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Tribute existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Tribute PUT", id.ToString(), new Exception("No Scheduler.Tribute entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (tributeDTO.objectGuid == Guid.Empty)
            {
                tributeDTO.objectGuid = existing.objectGuid;
            }
            else if (tributeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Tribute record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Tribute cloneOfExisting = (Database.Tribute)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Tribute object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Tribute tribute = (Database.Tribute)_context.Entry(existing).GetDatabaseValues().ToObject();
			tribute.ApplyDTO(tributeDTO);
			//
			// The tenant guid for any Tribute being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Tribute because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				tribute.tenantGuid = existing.tenantGuid;
			}

			lock (tributePutSyncRoot)
			{
				//
				// Validate the version number for the tribute being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != tribute.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Tribute save attempt was made but save request was with version " + tribute.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Tribute you are trying to update has already changed.  Please try your save again after reloading the Tribute.");
				}
				else
				{
					// Same record.  Increase version.
					tribute.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (tribute.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Tribute record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (tribute.name != null && tribute.name.Length > 100)
				{
					tribute.name = tribute.name.Substring(0, 100);
				}

				if (tribute.description != null && tribute.description.Length > 500)
				{
					tribute.description = tribute.description.Substring(0, 500);
				}

				if (tribute.color != null && tribute.color.Length > 10)
				{
					tribute.color = tribute.color.Substring(0, 10);
				}

				if (tribute.avatarFileName != null && tribute.avatarFileName.Length > 250)
				{
					tribute.avatarFileName = tribute.avatarFileName.Substring(0, 250);
				}

				if (tribute.avatarMimeType != null && tribute.avatarMimeType.Length > 100)
				{
					tribute.avatarMimeType = tribute.avatarMimeType.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (tribute.avatarData != null && string.IsNullOrEmpty(tribute.avatarFileName))
				{
				    tribute.avatarFileName = tribute.objectGuid.ToString() + ".data";
				}

				if (tribute.avatarData != null && (tribute.avatarSize.HasValue == false || tribute.avatarSize != tribute.avatarData.Length))
				{
				    tribute.avatarSize = tribute.avatarData.Length;
				}

				if (tribute.avatarData != null && string.IsNullOrEmpty(tribute.avatarMimeType))
				{
				    tribute.avatarMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = tribute.avatarData;

					if (diskBasedBinaryStorageMode == true &&
					    tribute.avatarFileName != null &&
					    tribute.avatarData != null &&
					    tribute.avatarSize.HasValue == true &&
					    tribute.avatarSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(tribute.objectGuid, tribute.versionNumber, tribute.avatarData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    tribute.avatarData = null;

					}

				    EntityEntry<Database.Tribute> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(tribute);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        TributeChangeHistory tributeChangeHistory = new TributeChangeHistory();
				        tributeChangeHistory.tributeId = tribute.id;
				        tributeChangeHistory.versionNumber = tribute.versionNumber;
				        tributeChangeHistory.timeStamp = DateTime.UtcNow;
				        tributeChangeHistory.userId = securityUser.id;
				        tributeChangeHistory.tenantGuid = userTenantGuid;
				        tributeChangeHistory.data = JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute));
				        _context.TributeChangeHistories.Add(tributeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    tribute.avatarData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Tribute entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute)),
						null);

				return Ok(Database.Tribute.CreateAnonymous(tribute));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Tribute entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Tribute record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tribute", Name = "Tribute")]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PostTribute([FromBody]Database.Tribute.TributeDTO tributeDTO, CancellationToken cancellationToken = default)
		{
			if (tributeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
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
			// Create a new Tribute object using the data from the DTO
			//
			Database.Tribute tribute = Database.Tribute.FromDTO(tributeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				tribute.tenantGuid = userTenantGuid;

				if (tribute.name != null && tribute.name.Length > 100)
				{
					tribute.name = tribute.name.Substring(0, 100);
				}

				if (tribute.description != null && tribute.description.Length > 500)
				{
					tribute.description = tribute.description.Substring(0, 500);
				}

				if (tribute.color != null && tribute.color.Length > 10)
				{
					tribute.color = tribute.color.Substring(0, 10);
				}

				if (tribute.avatarFileName != null && tribute.avatarFileName.Length > 250)
				{
					tribute.avatarFileName = tribute.avatarFileName.Substring(0, 250);
				}

				if (tribute.avatarMimeType != null && tribute.avatarMimeType.Length > 100)
				{
					tribute.avatarMimeType = tribute.avatarMimeType.Substring(0, 100);
				}

				tribute.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (tribute.avatarData != null && string.IsNullOrEmpty(tribute.avatarFileName))
				{
				    tribute.avatarFileName = tribute.objectGuid.ToString() + ".data";
				}

				if (tribute.avatarData != null && (tribute.avatarSize.HasValue == false || tribute.avatarSize != tribute.avatarData.Length))
				{
				    tribute.avatarSize = tribute.avatarData.Length;
				}

				if (tribute.avatarData != null && string.IsNullOrEmpty(tribute.avatarMimeType))
				{
				    tribute.avatarMimeType = "application/octet-stream";
				}

				tribute.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = tribute.avatarData;

				if (diskBasedBinaryStorageMode == true &&
				    tribute.avatarData != null &&
				    tribute.avatarFileName != null &&
				    tribute.avatarSize.HasValue == true &&
				    tribute.avatarSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(tribute.objectGuid, tribute.versionNumber, tribute.avatarData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    tribute.avatarData = null;

				}

				_context.Tributes.Add(tribute);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the tribute object so that no further changes will be written to the database
				    //
				    _context.Entry(tribute).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					tribute.avatarData = null;
					tribute.Gifts = null;
					tribute.TributeChangeHistories = null;
					tribute.defaultAcknowledgee = null;
					tribute.icon = null;
					tribute.tributeType = null;


				    TributeChangeHistory tributeChangeHistory = new TributeChangeHistory();
				    tributeChangeHistory.tributeId = tribute.id;
				    tributeChangeHistory.versionNumber = tribute.versionNumber;
				    tributeChangeHistory.timeStamp = DateTime.UtcNow;
				    tributeChangeHistory.userId = securityUser.id;
				    tributeChangeHistory.tenantGuid = userTenantGuid;
				    tributeChangeHistory.data = JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute));
				    _context.TributeChangeHistories.Add(tributeChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Tribute entity successfully created.",
						true,
						tribute. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    tribute.avatarData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Tribute entity creation failed.", false, tribute.id.ToString(), "", JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Tribute", tribute.id, tribute.name));

			return CreatedAtRoute("Tribute", new { id = tribute.id }, Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute));
		}



        /// <summary>
        /// 
        /// This rolls a Tribute entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tribute/Rollback/{id}")]
		[Route("api/Tribute/Rollback")]
		public async Task<IActionResult> RollbackToTributeVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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

			

			
			IQueryable <Database.Tribute> query = (from x in _context.Tributes
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this Tribute concurrently
			//
			lock (tributePutSyncRoot)
			{
				
				Database.Tribute tribute = query.FirstOrDefault();
				
				if (tribute == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Tribute rollback", id.ToString(), new Exception("No Scheduler.Tribute entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Tribute current state so we can log it.
				//
				Database.Tribute cloneOfExisting = (Database.Tribute)_context.Entry(tribute).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.avatarData = null;
				cloneOfExisting.Gifts = null;
				cloneOfExisting.TributeChangeHistories = null;
				cloneOfExisting.defaultAcknowledgee = null;
				cloneOfExisting.icon = null;
				cloneOfExisting.tributeType = null;

				if (versionNumber >= tribute.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Tribute rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Tribute rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				TributeChangeHistory tributeChangeHistory = (from x in _context.TributeChangeHistories
				                                               where
				                                               x.tributeId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (tributeChangeHistory != null)
				{
				    Database.Tribute oldTribute = JsonSerializer.Deserialize<Database.Tribute>(tributeChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    tribute.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    tribute.name = oldTribute.name;
				    tribute.description = oldTribute.description;
				    tribute.tributeTypeId = oldTribute.tributeTypeId;
				    tribute.defaultAcknowledgeeId = oldTribute.defaultAcknowledgeeId;
				    tribute.startDate = oldTribute.startDate;
				    tribute.endDate = oldTribute.endDate;
				    tribute.iconId = oldTribute.iconId;
				    tribute.color = oldTribute.color;
				    tribute.avatarFileName = oldTribute.avatarFileName;
				    tribute.avatarSize = oldTribute.avatarSize;
				    tribute.avatarData = oldTribute.avatarData;
				    tribute.avatarMimeType = oldTribute.avatarMimeType;
				    tribute.objectGuid = oldTribute.objectGuid;
				    tribute.active = oldTribute.active;
				    tribute.deleted = oldTribute.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldTribute.objectGuid, oldTribute.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(tribute.objectGuid, tribute.versionNumber, binaryData, "data");
				    }

				    string serializedTribute = JsonSerializer.Serialize(tribute);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        TributeChangeHistory newTributeChangeHistory = new TributeChangeHistory();
				        newTributeChangeHistory.tributeId = tribute.id;
				        newTributeChangeHistory.versionNumber = tribute.versionNumber;
				        newTributeChangeHistory.timeStamp = DateTime.UtcNow;
				        newTributeChangeHistory.userId = securityUser.id;
				        newTributeChangeHistory.tenantGuid = userTenantGuid;
				        newTributeChangeHistory.data = JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute));
				        _context.TributeChangeHistories.Add(newTributeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Tribute rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute)),
						null);


				    return Ok(Database.Tribute.CreateAnonymous(tribute));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Tribute rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Tribute rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Tribute.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Tribute</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tribute/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetTributeChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
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


			Database.Tribute tribute = await _context.Tributes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tribute == null)
			{
				return NotFound();
			}

			try
			{
				tribute.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Tribute> versionInfo = await tribute.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Tribute.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Tribute</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tribute/{id}/AuditHistory")]
		public async Task<IActionResult> GetTributeAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{
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


			Database.Tribute tribute = await _context.Tributes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tribute == null)
			{
				return NotFound();
			}

			try
			{
				tribute.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Tribute>> versions = await tribute.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Tribute.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Tribute</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Tribute object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tribute/{id}/Version/{version}")]
		public async Task<IActionResult> GetTributeVersion(int id, int version, CancellationToken cancellationToken = default)
		{
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


			Database.Tribute tribute = await _context.Tributes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tribute == null)
			{
				return NotFound();
			}

			try
			{
				tribute.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Tribute> versionInfo = await tribute.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Tribute at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Tribute</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Tribute object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tribute/{id}/StateAtTime")]
		public async Task<IActionResult> GetTributeStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{
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


			Database.Tribute tribute = await _context.Tributes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (tribute == null)
			{
				return NotFound();
			}

			try
			{
				tribute.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Tribute> versionInfo = await tribute.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Tribute record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Tribute/{id}")]
		[Route("api/Tribute")]
		public async Task<IActionResult> DeleteTribute(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(cancellationToken);
			
			
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

			IQueryable<Database.Tribute> query = (from x in _context.Tributes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Tribute tribute = await query.FirstOrDefaultAsync(cancellationToken);

			if (tribute == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Tribute DELETE", id.ToString(), new Exception("No Scheduler.Tribute entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Tribute cloneOfExisting = (Database.Tribute)_context.Entry(tribute).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (tributeDeleteSyncRoot)
			{
			    try
			    {
			        tribute.deleted = true;
			        tribute.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(tribute.objectGuid, tribute.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(tribute.objectGuid, tribute.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        TributeChangeHistory tributeChangeHistory = new TributeChangeHistory();
			        tributeChangeHistory.tributeId = tribute.id;
			        tributeChangeHistory.versionNumber = tribute.versionNumber;
			        tributeChangeHistory.timeStamp = DateTime.UtcNow;
			        tributeChangeHistory.userId = securityUser.id;
			        tributeChangeHistory.tenantGuid = userTenantGuid;
			        tributeChangeHistory.data = JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute));
			        _context.TributeChangeHistories.Add(tributeChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Tribute entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Tribute entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Tribute records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Tributes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? tributeTypeId = null,
			int? defaultAcknowledgeeId = null,
			int? iconId = null,
			string color = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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

			IQueryable<Database.Tribute> query = (from t in _context.Tributes select t);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(t => t.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(t => t.description == description);
			}
			if (tributeTypeId.HasValue == true)
			{
				query = query.Where(t => t.tributeTypeId == tributeTypeId.Value);
			}
			if (defaultAcknowledgeeId.HasValue == true)
			{
				query = query.Where(t => t.defaultAcknowledgeeId == defaultAcknowledgeeId.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(t => t.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(t => t.color == color);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(t => t.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(t => t.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(t => t.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(t => t.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(t => t.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(t => t.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(t => t.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(t => t.deleted == false);
				}
			}
			else
			{
				query = query.Where(t => t.active == true);
				query = query.Where(t => t.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Tribute, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.defaultAcknowledgee.constituentNumber.Contains(anyStringContains)
			       || x.defaultAcknowledgee.externalId.Contains(anyStringContains)
			       || x.defaultAcknowledgee.notes.Contains(anyStringContains)
			       || x.defaultAcknowledgee.attributes.Contains(anyStringContains)
			       || x.defaultAcknowledgee.color.Contains(anyStringContains)
			       || x.defaultAcknowledgee.avatarFileName.Contains(anyStringContains)
			       || x.defaultAcknowledgee.avatarMimeType.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			       || x.tributeType.name.Contains(anyStringContains)
			       || x.tributeType.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Tribute.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Tribute/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}




        [Route("api/Tribute/Data/{id:int}")]
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


            Database.Tribute tribute = await (from x in _context.Tributes where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (tribute == null)
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

						lock (tributePutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									tribute.avatarFileName = fileName.Trim();
									tribute.avatarMimeType = mimeType;
									tribute.avatarSize = section.Body.Length;

									tribute.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 tribute.avatarFileName != null &&
										 tribute.avatarSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(tribute.objectGuid, tribute.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										tribute.avatarData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											tribute.avatarData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									TributeChangeHistory tributeChangeHistory = new TributeChangeHistory();
									tributeChangeHistory.tributeId = tribute.id;
									tributeChangeHistory.versionNumber = tribute.versionNumber;
									tributeChangeHistory.timeStamp = DateTime.UtcNow;
									tributeChangeHistory.userId = securityUser.id;
									tributeChangeHistory.tenantGuid = tribute.tenantGuid;
									tributeChangeHistory.data = JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute));
									_context.TributeChangeHistories.Add(tributeChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Tribute Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Tribute Data Upload Failed.", false, id.ToString(), "", "", ex);

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
                lock (tributePutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(tribute.objectGuid, tribute.versionNumber, "data");
                            }

                            tribute.avatarFileName = null;
                            tribute.avatarMimeType = null;
                            tribute.avatarSize = 0;
                            tribute.avatarData = null;
                            tribute.versionNumber++;


                            //
                            // Now add the change history
                            //
                            TributeChangeHistory tributeChangeHistory = new TributeChangeHistory();
                            tributeChangeHistory.tributeId = tribute.id;
                            tributeChangeHistory.versionNumber = tribute.versionNumber;
                            tributeChangeHistory.timeStamp = DateTime.UtcNow;
                            tributeChangeHistory.userId = securityUser.id;
                                    tributeChangeHistory.tenantGuid = tribute.tenantGuid;
                                    tributeChangeHistory.data = JsonSerializer.Serialize(Database.Tribute.CreateAnonymousWithFirstLevelSubObjects(tribute));
                            _context.TributeChangeHistories.Add(tributeChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Tribute data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Tribute data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Tribute/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id)
        {
             if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
             {
                 return Forbid();
             }


			using (SchedulerContext context = new SchedulerContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.Tribute tribute = await (from d in context.Tributes
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (tribute != null && tribute.avatarData != null)
                {
                   return File(tribute.avatarData.ToArray<byte>(), tribute.avatarMimeType, tribute.avatarFileName != null ? tribute.avatarFileName.Trim() : "Tribute_" + tribute.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
