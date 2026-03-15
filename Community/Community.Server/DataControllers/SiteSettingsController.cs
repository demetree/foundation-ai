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
using Foundation.Community.Database;

namespace Foundation.Community.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the SiteSetting entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SiteSetting entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SiteSettingsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		private CommunityContext _context;

		private ILogger<SiteSettingsController> _logger;

		public SiteSettingsController(CommunityContext context, ILogger<SiteSettingsController> logger) : base("Community", "SiteSetting")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SiteSettings filtered by the parameters provided.
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
		[Route("api/SiteSettings")]
		public async Task<IActionResult> GetSiteSettings(
			int? Id = null,
			string SettingKey = null,
			string SettingValue = null,
			string Description = null,
			string SettingGroup = null,
			Guid? ObjectGuid = null,
			bool? Active = null,
			bool? Deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

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

			IQueryable<Database.SiteSetting> query = (from ss in _context.SiteSettings select ss);
			if (Id.HasValue == true)
			{
				query = query.Where(ss => ss.Id == Id.Value);
			}
			if (string.IsNullOrEmpty(SettingKey) == false)
			{
				query = query.Where(ss => ss.SettingKey == SettingKey);
			}
			if (string.IsNullOrEmpty(SettingValue) == false)
			{
				query = query.Where(ss => ss.SettingValue == SettingValue);
			}
			if (string.IsNullOrEmpty(Description) == false)
			{
				query = query.Where(ss => ss.Description == Description);
			}
			if (string.IsNullOrEmpty(SettingGroup) == false)
			{
				query = query.Where(ss => ss.SettingGroup == SettingGroup);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(ss => ss.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(ss => ss.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(ss => ss.Deleted == Deleted.Value);
			}

			query = query.OrderBy(ss => ss.settingKey).ThenBy(ss => ss.description).ThenBy(ss => ss.settingGroup);


			//
			// Add the any string contains parameter to span all the string fields on the Site Setting, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.SettingKey.Contains(anyStringContains)
			       || x.SettingValue.Contains(anyStringContains)
			       || x.Description.Contains(anyStringContains)
			       || x.SettingGroup.Contains(anyStringContains)
			   );
			}

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.SiteSetting> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SiteSetting siteSetting in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(siteSetting, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.SiteSetting Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.SiteSetting Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SiteSettings filtered by the parameters provided.  Its query is similar to the GetSiteSettings method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SiteSettings/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? Id = null,
			string SettingKey = null,
			string SettingValue = null,
			string Description = null,
			string SettingGroup = null,
			Guid? ObjectGuid = null,
			bool? Active = null,
			bool? Deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SiteSetting> query = (from ss in _context.SiteSettings select ss);
			if (Id.HasValue == true)
			{
				query = query.Where(ss => ss.Id == Id.Value);
			}
			if (SettingKey != null)
			{
				query = query.Where(ss => ss.SettingKey == SettingKey);
			}
			if (SettingValue != null)
			{
				query = query.Where(ss => ss.SettingValue == SettingValue);
			}
			if (Description != null)
			{
				query = query.Where(ss => ss.Description == Description);
			}
			if (SettingGroup != null)
			{
				query = query.Where(ss => ss.SettingGroup == SettingGroup);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(ss => ss.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(ss => ss.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(ss => ss.Deleted == Deleted.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Site Setting, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.SettingKey.Contains(anyStringContains)
			       || x.SettingValue.Contains(anyStringContains)
			       || x.Description.Contains(anyStringContains)
			       || x.SettingGroup.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SiteSetting by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SiteSetting/{id}")]
		public async Task<IActionResult> GetSiteSetting(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.SiteSetting> query = (from ss in _context.SiteSettings where
				(ss.id == id)
					select ss);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.SiteSetting materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.SiteSetting Entity was read with Admin privilege." : "Community.SiteSetting Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SiteSetting", materialized.id, materialized.settingKey));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.SiteSetting entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.SiteSetting.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.SiteSetting.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SiteSetting record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SiteSetting/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSiteSetting(int id, [FromBody]Database.SiteSetting.SiteSettingDTO siteSettingDTO, CancellationToken cancellationToken = default)
		{
			if (siteSettingDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Admin role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Admin", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != siteSettingDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SiteSetting> query = (from x in _context.SiteSettings
				where
				(x.id == id)
				select x);


			Database.SiteSetting existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.SiteSetting PUT", id.ToString(), new Exception("No Community.SiteSetting entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (siteSettingDTO.objectGuid == Guid.Empty)
            {
                siteSettingDTO.objectGuid = existing.objectGuid;
            }
            else if (siteSettingDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SiteSetting record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SiteSetting cloneOfExisting = (Database.SiteSetting)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SiteSetting object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SiteSetting siteSetting = (Database.SiteSetting)_context.Entry(existing).GetDatabaseValues().ToObject();
			siteSetting.ApplyDTO(siteSettingDTO);


			EntityEntry<Database.SiteSetting> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(siteSetting);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.SiteSetting entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SiteSetting.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SiteSetting.CreateAnonymousWithFirstLevelSubObjects(siteSetting)),
					null);


				return Ok(Database.SiteSetting.CreateAnonymous(siteSetting));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.SiteSetting entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SiteSetting.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SiteSetting.CreateAnonymousWithFirstLevelSubObjects(siteSetting)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SiteSetting record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SiteSetting", Name = "SiteSetting")]
		public async Task<IActionResult> PostSiteSetting([FromBody]Database.SiteSetting.SiteSettingDTO siteSettingDTO, CancellationToken cancellationToken = default)
		{
			if (siteSettingDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Admin role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Admin", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new SiteSetting object using the data from the DTO
			//
			Database.SiteSetting siteSetting = Database.SiteSetting.FromDTO(siteSettingDTO);

			try
			{
				_context.SiteSettings.Add(siteSetting);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.SiteSetting entity successfully created.",
					true,
					siteSetting.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SiteSetting.CreateAnonymousWithFirstLevelSubObjects(siteSetting)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.SiteSetting entity creation failed.", false, siteSetting.id.ToString(), "", JsonSerializer.Serialize(siteSetting), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SiteSetting", siteSetting.id, siteSetting.settingKey));

			return CreatedAtRoute("SiteSetting", new { id = siteSetting.id }, Database.SiteSetting.CreateAnonymousWithFirstLevelSubObjects(siteSetting));
		}



        /// <summary>
        /// 
        /// This deletes a SiteSetting record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SiteSetting/{id}")]
		[Route("api/SiteSetting")]
		public async Task<IActionResult> DeleteSiteSetting(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Admin role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Admin", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SiteSetting> query = (from x in _context.SiteSettings
				where
				(x.id == id)
				select x);


			Database.SiteSetting siteSetting = await query.FirstOrDefaultAsync(cancellationToken);

			if (siteSetting == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.SiteSetting DELETE", id.ToString(), new Exception("No Community.SiteSetting entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SiteSetting cloneOfExisting = (Database.SiteSetting)_context.Entry(siteSetting).GetDatabaseValues().ToObject();


			try
			{
				_context.SiteSettings.Remove(siteSetting);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.SiteSetting entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SiteSetting.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SiteSetting.CreateAnonymousWithFirstLevelSubObjects(siteSetting)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.SiteSetting entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SiteSetting.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SiteSetting.CreateAnonymousWithFirstLevelSubObjects(siteSetting)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SiteSetting records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SiteSettings/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? Id = null,
			string SettingKey = null,
			string SettingValue = null,
			string Description = null,
			string SettingGroup = null,
			Guid? ObjectGuid = null,
			bool? Active = null,
			bool? Deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);


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

			IQueryable<Database.SiteSetting> query = (from ss in _context.SiteSettings select ss);
			if (Id.HasValue == true)
			{
				query = query.Where(ss => ss.Id == Id.Value);
			}
			if (string.IsNullOrEmpty(SettingKey) == false)
			{
				query = query.Where(ss => ss.SettingKey == SettingKey);
			}
			if (string.IsNullOrEmpty(SettingValue) == false)
			{
				query = query.Where(ss => ss.SettingValue == SettingValue);
			}
			if (string.IsNullOrEmpty(Description) == false)
			{
				query = query.Where(ss => ss.Description == Description);
			}
			if (string.IsNullOrEmpty(SettingGroup) == false)
			{
				query = query.Where(ss => ss.SettingGroup == SettingGroup);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(ss => ss.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(ss => ss.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(ss => ss.Deleted == Deleted.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Site Setting, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.SettingKey.Contains(anyStringContains)
			       || x.SettingValue.Contains(anyStringContains)
			       || x.Description.Contains(anyStringContains)
			       || x.SettingGroup.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.settingKey).ThenBy(x => x.description).ThenBy(x => x.settingGroup);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SiteSetting.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SiteSetting/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Community Admin role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Admin", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
