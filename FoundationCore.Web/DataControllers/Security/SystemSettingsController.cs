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
using static Foundation.Auditor.AuditEngine;
using Foundation.Security.Database;

namespace Foundation.Security.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the SystemSetting entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SystemSetting entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SystemSettingsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SecurityContext _context;

		private ILogger<SystemSettingsController> _logger;

		public SystemSettingsController(SecurityContext context, ILogger<SystemSettingsController> logger) : base("Security", "SystemSetting")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SystemSettings filtered by the parameters provided.
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
		[Route("api/SystemSettings")]
		public async Task<IActionResult> GetSystemSettings(
			string name = null,
			string description = null,
			string value = null,
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
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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

			IQueryable<Database.SystemSetting> query = (from ss in _context.SystemSettings select ss);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ss => ss.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ss => ss.description == description);
			}
			if (string.IsNullOrEmpty(value) == false)
			{
				query = query.Where(ss => ss.value == value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ss => ss.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ss => ss.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ss => ss.deleted == false);
				}
			}
			else
			{
				query = query.Where(ss => ss.active == true);
				query = query.Where(ss => ss.deleted == false);
			}

			query = query.OrderBy(ss => ss.name).ThenBy(ss => ss.description);


			//
			// Add the any string contains parameter to span all the string fields on the System Setting, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.value.Contains(anyStringContains)
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
			
			List<Database.SystemSetting> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SystemSetting systemSetting in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(systemSetting, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SystemSetting Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SystemSetting Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SystemSettings filtered by the parameters provided.  Its query is similar to the GetSystemSettings method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SystemSettings/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string value = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SystemSetting> query = (from ss in _context.SystemSettings select ss);
			if (name != null)
			{
				query = query.Where(ss => ss.name == name);
			}
			if (description != null)
			{
				query = query.Where(ss => ss.description == description);
			}
			if (value != null)
			{
				query = query.Where(ss => ss.value == value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ss => ss.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ss => ss.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ss => ss.deleted == false);
				}
			}
			else
			{
				query = query.Where(ss => ss.active == true);
				query = query.Where(ss => ss.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the System Setting, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.value.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SystemSetting by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SystemSetting/{id}")]
		public async Task<IActionResult> GetSystemSetting(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.SystemSetting> query = (from ss in _context.SystemSettings where
							(ss.id == id) &&
							(userIsAdmin == true || ss.deleted == false) &&
							(userIsWriter == true || ss.active == true)
					select ss);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.SystemSetting materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SystemSetting Entity was read with Admin privilege." : "Security.SystemSetting Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SystemSetting", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SystemSetting entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SystemSetting.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SystemSetting.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SystemSetting record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SystemSetting/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSystemSetting(int id, [FromBody]Database.SystemSetting.SystemSettingDTO systemSettingDTO, CancellationToken cancellationToken = default)
		{
			if (systemSettingDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != systemSettingDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SystemSetting> query = (from x in _context.SystemSettings
				where
				(x.id == id)
				select x);


			Database.SystemSetting existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SystemSetting PUT", id.ToString(), new Exception("No Security.SystemSetting entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SystemSetting cloneOfExisting = (Database.SystemSetting)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SystemSetting object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SystemSetting systemSetting = (Database.SystemSetting)_context.Entry(existing).GetDatabaseValues().ToObject();
			systemSetting.ApplyDTO(systemSettingDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (systemSetting.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SystemSetting record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (systemSetting.name != null && systemSetting.name.Length > 100)
			{
				systemSetting.name = systemSetting.name.Substring(0, 100);
			}

			if (systemSetting.description != null && systemSetting.description.Length > 500)
			{
				systemSetting.description = systemSetting.description.Substring(0, 500);
			}

			EntityEntry<Database.SystemSetting> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(systemSetting);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SystemSetting entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SystemSetting.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SystemSetting.CreateAnonymousWithFirstLevelSubObjects(systemSetting)),
					null);


				return Ok(Database.SystemSetting.CreateAnonymous(systemSetting));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SystemSetting entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SystemSetting.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SystemSetting.CreateAnonymousWithFirstLevelSubObjects(systemSetting)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SystemSetting record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SystemSetting", Name = "SystemSetting")]
		public async Task<IActionResult> PostSystemSetting([FromBody]Database.SystemSetting.SystemSettingDTO systemSettingDTO, CancellationToken cancellationToken = default)
		{
			if (systemSettingDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new SystemSetting object using the data from the DTO
			//
			Database.SystemSetting systemSetting = Database.SystemSetting.FromDTO(systemSettingDTO);

			try
			{
				if (systemSetting.name != null && systemSetting.name.Length > 100)
				{
					systemSetting.name = systemSetting.name.Substring(0, 100);
				}

				if (systemSetting.description != null && systemSetting.description.Length > 500)
				{
					systemSetting.description = systemSetting.description.Substring(0, 500);
				}

				_context.SystemSettings.Add(systemSetting);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SystemSetting entity successfully created.",
					true,
					systemSetting.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SystemSetting.CreateAnonymousWithFirstLevelSubObjects(systemSetting)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SystemSetting entity creation failed.", false, systemSetting.id.ToString(), "", JsonSerializer.Serialize(systemSetting), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SystemSetting", systemSetting.id, systemSetting.name));

			return CreatedAtRoute("SystemSetting", new { id = systemSetting.id }, Database.SystemSetting.CreateAnonymousWithFirstLevelSubObjects(systemSetting));
		}



        /// <summary>
        /// 
        /// This deletes a SystemSetting record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SystemSetting/{id}")]
		[Route("api/SystemSetting")]
		public async Task<IActionResult> DeleteSystemSetting(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SystemSetting> query = (from x in _context.SystemSettings
				where
				(x.id == id)
				select x);


			Database.SystemSetting systemSetting = await query.FirstOrDefaultAsync(cancellationToken);

			if (systemSetting == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SystemSetting DELETE", id.ToString(), new Exception("No Security.SystemSetting entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SystemSetting cloneOfExisting = (Database.SystemSetting)_context.Entry(systemSetting).GetDatabaseValues().ToObject();


			try
			{
				systemSetting.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SystemSetting entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SystemSetting.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SystemSetting.CreateAnonymousWithFirstLevelSubObjects(systemSetting)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SystemSetting entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SystemSetting.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SystemSetting.CreateAnonymousWithFirstLevelSubObjects(systemSetting)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SystemSetting records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SystemSettings/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string value = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);


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

			IQueryable<Database.SystemSetting> query = (from ss in _context.SystemSettings select ss);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ss => ss.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ss => ss.description == description);
			}
			if (string.IsNullOrEmpty(value) == false)
			{
				query = query.Where(ss => ss.value == value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ss => ss.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ss => ss.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ss => ss.deleted == false);
				}
			}
			else
			{
				query = query.Where(ss => ss.active == true);
				query = query.Where(ss => ss.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the System Setting, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.value.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SystemSetting.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SystemSetting/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
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
