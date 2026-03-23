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
using Foundation.DeepSpace.Database;

namespace Foundation.DeepSpace.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the AccessLog entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AccessLog entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AccessLogsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private DeepSpaceContext _context;

		private ILogger<AccessLogsController> _logger;

		public AccessLogsController(DeepSpaceContext context, ILogger<AccessLogsController> logger) : base("DeepSpace", "AccessLog")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AccessLogs filtered by the parameters provided.
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
		[Route("api/AccessLogs")]
		public async Task<IActionResult> GetAccessLogs(
			int? storageObjectId = null,
			int? accessTypeId = null,
			Guid? accessedByUserGuid = null,
			DateTime? accessedUtc = null,
			string ipAddress = null,
			int? bytesTransferred = null,
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
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (accessedUtc.HasValue == true && accessedUtc.Value.Kind != DateTimeKind.Utc)
			{
				accessedUtc = accessedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.AccessLog> query = (from al in _context.AccessLogs select al);
			if (storageObjectId.HasValue == true)
			{
				query = query.Where(al => al.storageObjectId == storageObjectId.Value);
			}
			if (accessTypeId.HasValue == true)
			{
				query = query.Where(al => al.accessTypeId == accessTypeId.Value);
			}
			if (accessedByUserGuid.HasValue == true)
			{
				query = query.Where(al => al.accessedByUserGuid == accessedByUserGuid);
			}
			if (accessedUtc.HasValue == true)
			{
				query = query.Where(al => al.accessedUtc == accessedUtc.Value);
			}
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(al => al.ipAddress == ipAddress);
			}
			if (bytesTransferred.HasValue == true)
			{
				query = query.Where(al => al.bytesTransferred == bytesTransferred.Value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(al => al.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(al => al.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(al => al.deleted == false);
				}
			}
			else
			{
				query = query.Where(al => al.active == true);
				query = query.Where(al => al.deleted == false);
			}

			query = query.OrderByDescending(al => al.accessedUtc);


			//
			// Add the any string contains parameter to span all the string fields on the Access Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.ipAddress.Contains(anyStringContains)
			       || (includeRelations == true && x.accessType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.accessType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.storageObject.key.Contains(anyStringContains))
			       || (includeRelations == true && x.storageObject.contentType.Contains(anyStringContains))
			       || (includeRelations == true && x.storageObject.md5Hash.Contains(anyStringContains))
			       || (includeRelations == true && x.storageObject.sha256Hash.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.accessType);
				query = query.Include(x => x.storageObject);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.AccessLog> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AccessLog accessLog in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(accessLog, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "DeepSpace.AccessLog Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "DeepSpace.AccessLog Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AccessLogs filtered by the parameters provided.  Its query is similar to the GetAccessLogs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AccessLogs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? storageObjectId = null,
			int? accessTypeId = null,
			Guid? accessedByUserGuid = null,
			DateTime? accessedUtc = null,
			string ipAddress = null,
			int? bytesTransferred = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (accessedUtc.HasValue == true && accessedUtc.Value.Kind != DateTimeKind.Utc)
			{
				accessedUtc = accessedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.AccessLog> query = (from al in _context.AccessLogs select al);
			if (storageObjectId.HasValue == true)
			{
				query = query.Where(al => al.storageObjectId == storageObjectId.Value);
			}
			if (accessTypeId.HasValue == true)
			{
				query = query.Where(al => al.accessTypeId == accessTypeId.Value);
			}
			if (accessedByUserGuid.HasValue == true)
			{
				query = query.Where(al => al.accessedByUserGuid == accessedByUserGuid);
			}
			if (accessedUtc.HasValue == true)
			{
				query = query.Where(al => al.accessedUtc == accessedUtc.Value);
			}
			if (ipAddress != null)
			{
				query = query.Where(al => al.ipAddress == ipAddress);
			}
			if (bytesTransferred.HasValue == true)
			{
				query = query.Where(al => al.bytesTransferred == bytesTransferred.Value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(al => al.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(al => al.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(al => al.deleted == false);
				}
			}
			else
			{
				query = query.Where(al => al.active == true);
				query = query.Where(al => al.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Access Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.ipAddress.Contains(anyStringContains)
			       || x.accessType.name.Contains(anyStringContains)
			       || x.accessType.description.Contains(anyStringContains)
			       || x.storageObject.key.Contains(anyStringContains)
			       || x.storageObject.contentType.Contains(anyStringContains)
			       || x.storageObject.md5Hash.Contains(anyStringContains)
			       || x.storageObject.sha256Hash.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AccessLog by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AccessLog/{id}")]
		public async Task<IActionResult> GetAccessLog(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.AccessLog> query = (from al in _context.AccessLogs where
							(al.id == id) &&
							(userIsAdmin == true || al.deleted == false) &&
							(userIsWriter == true || al.active == true)
					select al);

				if (includeRelations == true)
				{
					query = query.Include(x => x.accessType);
					query = query.Include(x => x.storageObject);
					query = query.AsSplitQuery();
				}

				Database.AccessLog materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "DeepSpace.AccessLog Entity was read with Admin privilege." : "DeepSpace.AccessLog Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AccessLog", materialized.id, materialized.ipAddress));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a DeepSpace.AccessLog entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of DeepSpace.AccessLog.   Entity was read with Admin privilege." : "Exception caught during entity read of DeepSpace.AccessLog.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AccessLog record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AccessLog/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAccessLog(int id, [FromBody]Database.AccessLog.AccessLogDTO accessLogDTO, CancellationToken cancellationToken = default)
		{
			if (accessLogDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != accessLogDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AccessLog> query = (from x in _context.AccessLogs
				where
				(x.id == id)
				select x);


			Database.AccessLog existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.AccessLog PUT", id.ToString(), new Exception("No DeepSpace.AccessLog entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AccessLog cloneOfExisting = (Database.AccessLog)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AccessLog object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AccessLog accessLog = (Database.AccessLog)_context.Entry(existing).GetDatabaseValues().ToObject();
			accessLog.ApplyDTO(accessLogDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (accessLog.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted DeepSpace.AccessLog record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (accessLog.accessedUtc.Kind != DateTimeKind.Utc)
			{
				accessLog.accessedUtc = accessLog.accessedUtc.ToUniversalTime();
			}

			if (accessLog.ipAddress != null && accessLog.ipAddress.Length > 50)
			{
				accessLog.ipAddress = accessLog.ipAddress.Substring(0, 50);
			}

			EntityEntry<Database.AccessLog> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(accessLog);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"DeepSpace.AccessLog entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AccessLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AccessLog.CreateAnonymousWithFirstLevelSubObjects(accessLog)),
					null);


				return Ok(Database.AccessLog.CreateAnonymous(accessLog));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"DeepSpace.AccessLog entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AccessLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AccessLog.CreateAnonymousWithFirstLevelSubObjects(accessLog)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AccessLog record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AccessLog", Name = "AccessLog")]
		public async Task<IActionResult> PostAccessLog([FromBody]Database.AccessLog.AccessLogDTO accessLogDTO, CancellationToken cancellationToken = default)
		{
			if (accessLogDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new AccessLog object using the data from the DTO
			//
			Database.AccessLog accessLog = Database.AccessLog.FromDTO(accessLogDTO);

			try
			{
				if (accessLog.accessedUtc.Kind != DateTimeKind.Utc)
				{
					accessLog.accessedUtc = accessLog.accessedUtc.ToUniversalTime();
				}

				if (accessLog.ipAddress != null && accessLog.ipAddress.Length > 50)
				{
					accessLog.ipAddress = accessLog.ipAddress.Substring(0, 50);
				}

				_context.AccessLogs.Add(accessLog);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"DeepSpace.AccessLog entity successfully created.",
					true,
					accessLog.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AccessLog.CreateAnonymousWithFirstLevelSubObjects(accessLog)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "DeepSpace.AccessLog entity creation failed.", false, accessLog.id.ToString(), "", JsonSerializer.Serialize(accessLog), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AccessLog", accessLog.id, accessLog.ipAddress));

			return CreatedAtRoute("AccessLog", new { id = accessLog.id }, Database.AccessLog.CreateAnonymousWithFirstLevelSubObjects(accessLog));
		}



        /// <summary>
        /// 
        /// This deletes a AccessLog record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AccessLog/{id}")]
		[Route("api/AccessLog")]
		public async Task<IActionResult> DeleteAccessLog(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AccessLog> query = (from x in _context.AccessLogs
				where
				(x.id == id)
				select x);


			Database.AccessLog accessLog = await query.FirstOrDefaultAsync(cancellationToken);

			if (accessLog == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.AccessLog DELETE", id.ToString(), new Exception("No DeepSpace.AccessLog entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AccessLog cloneOfExisting = (Database.AccessLog)_context.Entry(accessLog).GetDatabaseValues().ToObject();


			try
			{
				accessLog.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"DeepSpace.AccessLog entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AccessLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AccessLog.CreateAnonymousWithFirstLevelSubObjects(accessLog)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"DeepSpace.AccessLog entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AccessLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AccessLog.CreateAnonymousWithFirstLevelSubObjects(accessLog)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AccessLog records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AccessLogs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? storageObjectId = null,
			int? accessTypeId = null,
			Guid? accessedByUserGuid = null,
			DateTime? accessedUtc = null,
			string ipAddress = null,
			int? bytesTransferred = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// DeepSpace Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (accessedUtc.HasValue == true && accessedUtc.Value.Kind != DateTimeKind.Utc)
			{
				accessedUtc = accessedUtc.Value.ToUniversalTime();
			}

			IQueryable<Database.AccessLog> query = (from al in _context.AccessLogs select al);
			if (storageObjectId.HasValue == true)
			{
				query = query.Where(al => al.storageObjectId == storageObjectId.Value);
			}
			if (accessTypeId.HasValue == true)
			{
				query = query.Where(al => al.accessTypeId == accessTypeId.Value);
			}
			if (accessedByUserGuid.HasValue == true)
			{
				query = query.Where(al => al.accessedByUserGuid == accessedByUserGuid);
			}
			if (accessedUtc.HasValue == true)
			{
				query = query.Where(al => al.accessedUtc == accessedUtc.Value);
			}
			if (string.IsNullOrEmpty(ipAddress) == false)
			{
				query = query.Where(al => al.ipAddress == ipAddress);
			}
			if (bytesTransferred.HasValue == true)
			{
				query = query.Where(al => al.bytesTransferred == bytesTransferred.Value);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(al => al.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(al => al.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(al => al.deleted == false);
				}
			}
			else
			{
				query = query.Where(al => al.active == true);
				query = query.Where(al => al.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Access Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.ipAddress.Contains(anyStringContains)
			       || x.accessType.name.Contains(anyStringContains)
			       || x.accessType.description.Contains(anyStringContains)
			       || x.storageObject.key.Contains(anyStringContains)
			       || x.storageObject.contentType.Contains(anyStringContains)
			       || x.storageObject.md5Hash.Contains(anyStringContains)
			       || x.storageObject.sha256Hash.Contains(anyStringContains)
			   );
			}


			query = query.OrderByDescending (x => x.accessedUtc);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AccessLog.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AccessLog/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Writer role needed to write to this table, as well as the minimum write permission level.
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
