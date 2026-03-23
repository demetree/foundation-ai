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
    /// This auto generated class provides the basic CRUD operations for the ReplicationTargetChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ReplicationTargetChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ReplicationTargetChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private DeepSpaceContext _context;

		private ILogger<ReplicationTargetChangeHistoriesController> _logger;

		public ReplicationTargetChangeHistoriesController(DeepSpaceContext context, ILogger<ReplicationTargetChangeHistoriesController> logger) : base("DeepSpace", "ReplicationTargetChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ReplicationTargetChangeHistories filtered by the parameters provided.
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
		[Route("api/ReplicationTargetChangeHistories")]
		public async Task<IActionResult> GetReplicationTargetChangeHistories(
			int? replicationTargetId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
			int? pageSize = null,
			int? pageNumber = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.ReplicationTargetChangeHistory> query = (from rtch in _context.ReplicationTargetChangeHistories select rtch);
			if (replicationTargetId.HasValue == true)
			{
				query = query.Where(rtch => rtch.replicationTargetId == replicationTargetId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rtch => rtch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(rtch => rtch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(rtch => rtch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(rtch => rtch.data == data);
			}

			query = query.OrderByDescending(rtch => rtch.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.replicationTarget);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ReplicationTargetChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ReplicationTargetChangeHistory replicationTargetChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(replicationTargetChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "DeepSpace.ReplicationTargetChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "DeepSpace.ReplicationTargetChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ReplicationTargetChangeHistories filtered by the parameters provided.  Its query is similar to the GetReplicationTargetChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTargetChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? replicationTargetId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.ReplicationTargetChangeHistory> query = (from rtch in _context.ReplicationTargetChangeHistories select rtch);
			if (replicationTargetId.HasValue == true)
			{
				query = query.Where(rtch => rtch.replicationTargetId == replicationTargetId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rtch => rtch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(rtch => rtch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(rtch => rtch.userId == userId.Value);
			}
			if (data != null)
			{
				query = query.Where(rtch => rtch.data == data);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ReplicationTargetChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTargetChangeHistory/{id}")]
		public async Task<IActionResult> GetReplicationTargetChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ReplicationTargetChangeHistory> query = (from rtch in _context.ReplicationTargetChangeHistories where
				(rtch.id == id)
					select rtch);

				if (includeRelations == true)
				{
					query = query.Include(x => x.replicationTarget);
					query = query.AsSplitQuery();
				}

				Database.ReplicationTargetChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "DeepSpace.ReplicationTargetChangeHistory Entity was read with Admin privilege." : "DeepSpace.ReplicationTargetChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a DeepSpace.ReplicationTargetChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of DeepSpace.ReplicationTargetChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of DeepSpace.ReplicationTargetChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing ReplicationTargetChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ReplicationTargetChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutReplicationTargetChangeHistory(int id, [FromBody]Database.ReplicationTargetChangeHistory.ReplicationTargetChangeHistoryDTO replicationTargetChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (replicationTargetChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// DeepSpace Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != replicationTargetChangeHistoryDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ReplicationTargetChangeHistory> query = (from x in _context.ReplicationTargetChangeHistories
				where
				(x.id == id)
				select x);


			Database.ReplicationTargetChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.ReplicationTargetChangeHistory PUT", id.ToString(), new Exception("No DeepSpace.ReplicationTargetChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ReplicationTargetChangeHistory cloneOfExisting = (Database.ReplicationTargetChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ReplicationTargetChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ReplicationTargetChangeHistory replicationTargetChangeHistory = (Database.ReplicationTargetChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();
			replicationTargetChangeHistory.ApplyDTO(replicationTargetChangeHistoryDTO);


			if (replicationTargetChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
			{
				replicationTargetChangeHistory.timeStamp = replicationTargetChangeHistory.timeStamp.ToUniversalTime();
			}

			EntityEntry<Database.ReplicationTargetChangeHistory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(replicationTargetChangeHistory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"DeepSpace.ReplicationTargetChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ReplicationTargetChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ReplicationTargetChangeHistory.CreateAnonymousWithFirstLevelSubObjects(replicationTargetChangeHistory)),
					null);


				return Ok(Database.ReplicationTargetChangeHistory.CreateAnonymous(replicationTargetChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"DeepSpace.ReplicationTargetChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ReplicationTargetChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ReplicationTargetChangeHistory.CreateAnonymousWithFirstLevelSubObjects(replicationTargetChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new ReplicationTargetChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTargetChangeHistory", Name = "ReplicationTargetChangeHistory")]
		public async Task<IActionResult> PostReplicationTargetChangeHistory([FromBody]Database.ReplicationTargetChangeHistory.ReplicationTargetChangeHistoryDTO replicationTargetChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (replicationTargetChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// DeepSpace Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new ReplicationTargetChangeHistory object using the data from the DTO
			//
			Database.ReplicationTargetChangeHistory replicationTargetChangeHistory = Database.ReplicationTargetChangeHistory.FromDTO(replicationTargetChangeHistoryDTO);

			try
			{
				if (replicationTargetChangeHistory.timeStamp.Kind != DateTimeKind.Utc)
				{
					replicationTargetChangeHistory.timeStamp = replicationTargetChangeHistory.timeStamp.ToUniversalTime();
				}

				_context.ReplicationTargetChangeHistories.Add(replicationTargetChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"DeepSpace.ReplicationTargetChangeHistory entity successfully created.",
					true,
					replicationTargetChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ReplicationTargetChangeHistory.CreateAnonymousWithFirstLevelSubObjects(replicationTargetChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "DeepSpace.ReplicationTargetChangeHistory entity creation failed.", false, replicationTargetChangeHistory.id.ToString(), "", JsonSerializer.Serialize(replicationTargetChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("ReplicationTargetChangeHistory", new { id = replicationTargetChangeHistory.id }, Database.ReplicationTargetChangeHistory.CreateAnonymousWithFirstLevelSubObjects(replicationTargetChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a ReplicationTargetChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ReplicationTargetChangeHistory/{id}")]
		[Route("api/ReplicationTargetChangeHistory")]
		public async Task<IActionResult> DeleteReplicationTargetChangeHistory(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// DeepSpace Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.ReplicationTargetChangeHistory> query = (from x in _context.ReplicationTargetChangeHistories
				where
				(x.id == id)
				select x);


			Database.ReplicationTargetChangeHistory replicationTargetChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (replicationTargetChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for DeepSpace.ReplicationTargetChangeHistory DELETE", id.ToString(), new Exception("No DeepSpace.ReplicationTargetChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ReplicationTargetChangeHistory cloneOfExisting = (Database.ReplicationTargetChangeHistory)_context.Entry(replicationTargetChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_context.ReplicationTargetChangeHistories.Remove(replicationTargetChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"DeepSpace.ReplicationTargetChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ReplicationTargetChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ReplicationTargetChangeHistory.CreateAnonymousWithFirstLevelSubObjects(replicationTargetChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"DeepSpace.ReplicationTargetChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ReplicationTargetChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ReplicationTargetChangeHistory.CreateAnonymousWithFirstLevelSubObjects(replicationTargetChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of ReplicationTargetChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ReplicationTargetChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? replicationTargetId = null,
			int? versionNumber = null,
			DateTime? timeStamp = null,
			int? userId = null,
			string data = null,
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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.ReplicationTargetChangeHistory> query = (from rtch in _context.ReplicationTargetChangeHistories select rtch);
			if (replicationTargetId.HasValue == true)
			{
				query = query.Where(rtch => rtch.replicationTargetId == replicationTargetId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rtch => rtch.versionNumber == versionNumber.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(rtch => rtch.timeStamp == timeStamp.Value);
			}
			if (userId.HasValue == true)
			{
				query = query.Where(rtch => rtch.userId == userId.Value);
			}
			if (string.IsNullOrEmpty(data) == false)
			{
				query = query.Where(rtch => rtch.data == data);
			}


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ReplicationTargetChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ReplicationTargetChangeHistory/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// DeepSpace Administrator role needed to write to this table.
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
