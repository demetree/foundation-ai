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
    /// This auto generated class provides the basic CRUD operations for the AnnouncementChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AnnouncementChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AnnouncementChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private CommunityContext _context;

		private ILogger<AnnouncementChangeHistoriesController> _logger;

		public AnnouncementChangeHistoriesController(CommunityContext context, ILogger<AnnouncementChangeHistoriesController> logger) : base("Community", "AnnouncementChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AnnouncementChangeHistories filtered by the parameters provided.
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
		[Route("api/AnnouncementChangeHistories")]
		public async Task<IActionResult> GetAnnouncementChangeHistories(
			int? Id = null,
			int? AnnouncementId = null,
			int? VersionNumber = null,
			DateTime? TimeStamp = null,
			int? UserId = null,
			string Data = null,
			int? pageSize = null,
			int? pageNumber = null,
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
			if (TimeStamp.HasValue == true && TimeStamp.Value.Kind != DateTimeKind.Utc)
			{
				TimeStamp = TimeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.AnnouncementChangeHistory> query = (from ach in _context.AnnouncementChangeHistories select ach);
			if (Id.HasValue == true)
			{
				query = query.Where(ach => ach.Id == Id.Value);
			}
			if (AnnouncementId.HasValue == true)
			{
				query = query.Where(ach => ach.AnnouncementId == AnnouncementId.Value);
			}
			if (VersionNumber.HasValue == true)
			{
				query = query.Where(ach => ach.VersionNumber == VersionNumber.Value);
			}
			if (TimeStamp.HasValue == true)
			{
				query = query.Where(ach => ach.TimeStamp == TimeStamp.Value);
			}
			if (UserId.HasValue == true)
			{
				query = query.Where(ach => ach.UserId == UserId.Value);
			}
			if (string.IsNullOrEmpty(Data) == false)
			{
				query = query.Where(ach => ach.Data == Data);
			}

			query = query.OrderByDescending(ach => ach.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.Announcement);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.AnnouncementChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AnnouncementChangeHistory announcementChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(announcementChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.AnnouncementChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.AnnouncementChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AnnouncementChangeHistories filtered by the parameters provided.  Its query is similar to the GetAnnouncementChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AnnouncementChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? Id = null,
			int? AnnouncementId = null,
			int? VersionNumber = null,
			DateTime? TimeStamp = null,
			int? UserId = null,
			string Data = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (TimeStamp.HasValue == true && TimeStamp.Value.Kind != DateTimeKind.Utc)
			{
				TimeStamp = TimeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.AnnouncementChangeHistory> query = (from ach in _context.AnnouncementChangeHistories select ach);
			if (Id.HasValue == true)
			{
				query = query.Where(ach => ach.Id == Id.Value);
			}
			if (AnnouncementId.HasValue == true)
			{
				query = query.Where(ach => ach.AnnouncementId == AnnouncementId.Value);
			}
			if (VersionNumber.HasValue == true)
			{
				query = query.Where(ach => ach.VersionNumber == VersionNumber.Value);
			}
			if (TimeStamp.HasValue == true)
			{
				query = query.Where(ach => ach.TimeStamp == TimeStamp.Value);
			}
			if (UserId.HasValue == true)
			{
				query = query.Where(ach => ach.UserId == UserId.Value);
			}
			if (Data != null)
			{
				query = query.Where(ach => ach.Data == Data);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AnnouncementChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AnnouncementChangeHistory/{id}")]
		public async Task<IActionResult> GetAnnouncementChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.AnnouncementChangeHistory> query = (from ach in _context.AnnouncementChangeHistories where
				(ach.id == id)
					select ach);

				if (includeRelations == true)
				{
					query = query.Include(x => x.Announcement);
					query = query.AsSplitQuery();
				}

				Database.AnnouncementChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.AnnouncementChangeHistory Entity was read with Admin privilege." : "Community.AnnouncementChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.AnnouncementChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.AnnouncementChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.AnnouncementChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing AnnouncementChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AnnouncementChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAnnouncementChangeHistory(int id, [FromBody]Database.AnnouncementChangeHistory.AnnouncementChangeHistoryDTO announcementChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (announcementChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != announcementChangeHistoryDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AnnouncementChangeHistory> query = (from x in _context.AnnouncementChangeHistories
				where
				(x.id == id)
				select x);


			Database.AnnouncementChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.AnnouncementChangeHistory PUT", id.ToString(), new Exception("No Community.AnnouncementChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AnnouncementChangeHistory cloneOfExisting = (Database.AnnouncementChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AnnouncementChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AnnouncementChangeHistory announcementChangeHistory = (Database.AnnouncementChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();
			announcementChangeHistory.ApplyDTO(announcementChangeHistoryDTO);


			if (announcementChangeHistory.TimeStamp.Kind != DateTimeKind.Utc)
			{
				announcementChangeHistory.TimeStamp = announcementChangeHistory.TimeStamp.ToUniversalTime();
			}

			EntityEntry<Database.AnnouncementChangeHistory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(announcementChangeHistory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.AnnouncementChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AnnouncementChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AnnouncementChangeHistory.CreateAnonymousWithFirstLevelSubObjects(announcementChangeHistory)),
					null);


				return Ok(Database.AnnouncementChangeHistory.CreateAnonymous(announcementChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.AnnouncementChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AnnouncementChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AnnouncementChangeHistory.CreateAnonymousWithFirstLevelSubObjects(announcementChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new AnnouncementChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AnnouncementChangeHistory", Name = "AnnouncementChangeHistory")]
		public async Task<IActionResult> PostAnnouncementChangeHistory([FromBody]Database.AnnouncementChangeHistory.AnnouncementChangeHistoryDTO announcementChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (announcementChangeHistoryDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new AnnouncementChangeHistory object using the data from the DTO
			//
			Database.AnnouncementChangeHistory announcementChangeHistory = Database.AnnouncementChangeHistory.FromDTO(announcementChangeHistoryDTO);

			try
			{
				if (announcementChangeHistory.TimeStamp.Kind != DateTimeKind.Utc)
				{
					announcementChangeHistory.TimeStamp = announcementChangeHistory.TimeStamp.ToUniversalTime();
				}

				_context.AnnouncementChangeHistories.Add(announcementChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.AnnouncementChangeHistory entity successfully created.",
					true,
					announcementChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AnnouncementChangeHistory.CreateAnonymousWithFirstLevelSubObjects(announcementChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.AnnouncementChangeHistory entity creation failed.", false, announcementChangeHistory.id.ToString(), "", JsonSerializer.Serialize(announcementChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("AnnouncementChangeHistory", new { id = announcementChangeHistory.id }, Database.AnnouncementChangeHistory.CreateAnonymousWithFirstLevelSubObjects(announcementChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a AnnouncementChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AnnouncementChangeHistory/{id}")]
		[Route("api/AnnouncementChangeHistory")]
		public async Task<IActionResult> DeleteAnnouncementChangeHistory(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AnnouncementChangeHistory> query = (from x in _context.AnnouncementChangeHistories
				where
				(x.id == id)
				select x);


			Database.AnnouncementChangeHistory announcementChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (announcementChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.AnnouncementChangeHistory DELETE", id.ToString(), new Exception("No Community.AnnouncementChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AnnouncementChangeHistory cloneOfExisting = (Database.AnnouncementChangeHistory)_context.Entry(announcementChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_context.AnnouncementChangeHistories.Remove(announcementChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.AnnouncementChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AnnouncementChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AnnouncementChangeHistory.CreateAnonymousWithFirstLevelSubObjects(announcementChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.AnnouncementChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AnnouncementChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AnnouncementChangeHistory.CreateAnonymousWithFirstLevelSubObjects(announcementChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of AnnouncementChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AnnouncementChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? Id = null,
			int? AnnouncementId = null,
			int? VersionNumber = null,
			DateTime? TimeStamp = null,
			int? UserId = null,
			string Data = null,
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
			if (TimeStamp.HasValue == true && TimeStamp.Value.Kind != DateTimeKind.Utc)
			{
				TimeStamp = TimeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.AnnouncementChangeHistory> query = (from ach in _context.AnnouncementChangeHistories select ach);
			if (Id.HasValue == true)
			{
				query = query.Where(ach => ach.Id == Id.Value);
			}
			if (AnnouncementId.HasValue == true)
			{
				query = query.Where(ach => ach.AnnouncementId == AnnouncementId.Value);
			}
			if (VersionNumber.HasValue == true)
			{
				query = query.Where(ach => ach.VersionNumber == VersionNumber.Value);
			}
			if (TimeStamp.HasValue == true)
			{
				query = query.Where(ach => ach.TimeStamp == TimeStamp.Value);
			}
			if (UserId.HasValue == true)
			{
				query = query.Where(ach => ach.UserId == UserId.Value);
			}
			if (string.IsNullOrEmpty(Data) == false)
			{
				query = query.Where(ach => ach.Data == Data);
			}


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AnnouncementChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AnnouncementChangeHistory/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Community Administrator role needed to write to this table.
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
