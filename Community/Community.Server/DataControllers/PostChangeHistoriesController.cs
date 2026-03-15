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
    /// This auto generated class provides the basic CRUD operations for the PostChangeHistory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PostChangeHistory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PostChangeHistoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 10;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private CommunityContext _context;

		private ILogger<PostChangeHistoriesController> _logger;

		public PostChangeHistoriesController(CommunityContext context, ILogger<PostChangeHistoriesController> logger) : base("Community", "PostChangeHistory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PostChangeHistories filtered by the parameters provided.
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
		[Route("api/PostChangeHistories")]
		public async Task<IActionResult> GetPostChangeHistories(
			int? Id = null,
			int? PostId = null,
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

			IQueryable<Database.PostChangeHistory> query = (from pch in _context.PostChangeHistories select pch);
			if (Id.HasValue == true)
			{
				query = query.Where(pch => pch.Id == Id.Value);
			}
			if (PostId.HasValue == true)
			{
				query = query.Where(pch => pch.PostId == PostId.Value);
			}
			if (VersionNumber.HasValue == true)
			{
				query = query.Where(pch => pch.VersionNumber == VersionNumber.Value);
			}
			if (TimeStamp.HasValue == true)
			{
				query = query.Where(pch => pch.TimeStamp == TimeStamp.Value);
			}
			if (UserId.HasValue == true)
			{
				query = query.Where(pch => pch.UserId == UserId.Value);
			}
			if (string.IsNullOrEmpty(Data) == false)
			{
				query = query.Where(pch => pch.Data == Data);
			}

			query = query.OrderByDescending(pch => pch.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.Post);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.PostChangeHistory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PostChangeHistory postChangeHistory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(postChangeHistory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.PostChangeHistory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.PostChangeHistory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PostChangeHistories filtered by the parameters provided.  Its query is similar to the GetPostChangeHistories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostChangeHistories/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? Id = null,
			int? PostId = null,
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

			IQueryable<Database.PostChangeHistory> query = (from pch in _context.PostChangeHistories select pch);
			if (Id.HasValue == true)
			{
				query = query.Where(pch => pch.Id == Id.Value);
			}
			if (PostId.HasValue == true)
			{
				query = query.Where(pch => pch.PostId == PostId.Value);
			}
			if (VersionNumber.HasValue == true)
			{
				query = query.Where(pch => pch.VersionNumber == VersionNumber.Value);
			}
			if (TimeStamp.HasValue == true)
			{
				query = query.Where(pch => pch.TimeStamp == TimeStamp.Value);
			}
			if (UserId.HasValue == true)
			{
				query = query.Where(pch => pch.UserId == UserId.Value);
			}
			if (Data != null)
			{
				query = query.Where(pch => pch.Data == Data);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PostChangeHistory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostChangeHistory/{id}")]
		public async Task<IActionResult> GetPostChangeHistory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.PostChangeHistory> query = (from pch in _context.PostChangeHistories where
				(pch.id == id)
					select pch);

				if (includeRelations == true)
				{
					query = query.Include(x => x.Post);
					query = query.AsSplitQuery();
				}

				Database.PostChangeHistory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.PostChangeHistory Entity was read with Admin privilege." : "Community.PostChangeHistory Entity was read.");

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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.PostChangeHistory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.PostChangeHistory.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.PostChangeHistory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing PostChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PostChangeHistory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPostChangeHistory(int id, [FromBody]Database.PostChangeHistory.PostChangeHistoryDTO postChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (postChangeHistoryDTO == null)
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



			if (id != postChangeHistoryDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.PostChangeHistory> query = (from x in _context.PostChangeHistories
				where
				(x.id == id)
				select x);


			Database.PostChangeHistory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.PostChangeHistory PUT", id.ToString(), new Exception("No Community.PostChangeHistory entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PostChangeHistory cloneOfExisting = (Database.PostChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PostChangeHistory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PostChangeHistory postChangeHistory = (Database.PostChangeHistory)_context.Entry(existing).GetDatabaseValues().ToObject();
			postChangeHistory.ApplyDTO(postChangeHistoryDTO);


			if (postChangeHistory.TimeStamp.Kind != DateTimeKind.Utc)
			{
				postChangeHistory.TimeStamp = postChangeHistory.TimeStamp.ToUniversalTime();
			}

			EntityEntry<Database.PostChangeHistory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(postChangeHistory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.PostChangeHistory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostChangeHistory.CreateAnonymousWithFirstLevelSubObjects(postChangeHistory)),
					null);


				return Ok(Database.PostChangeHistory.CreateAnonymous(postChangeHistory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.PostChangeHistory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostChangeHistory.CreateAnonymousWithFirstLevelSubObjects(postChangeHistory)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new PostChangeHistory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostChangeHistory", Name = "PostChangeHistory")]
		public async Task<IActionResult> PostPostChangeHistory([FromBody]Database.PostChangeHistory.PostChangeHistoryDTO postChangeHistoryDTO, CancellationToken cancellationToken = default)
		{
			if (postChangeHistoryDTO == null)
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
			// Create a new PostChangeHistory object using the data from the DTO
			//
			Database.PostChangeHistory postChangeHistory = Database.PostChangeHistory.FromDTO(postChangeHistoryDTO);

			try
			{
				if (postChangeHistory.TimeStamp.Kind != DateTimeKind.Utc)
				{
					postChangeHistory.TimeStamp = postChangeHistory.TimeStamp.ToUniversalTime();
				}

				_context.PostChangeHistories.Add(postChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.PostChangeHistory entity successfully created.",
					true,
					postChangeHistory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PostChangeHistory.CreateAnonymousWithFirstLevelSubObjects(postChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.PostChangeHistory entity creation failed.", false, postChangeHistory.id.ToString(), "", JsonSerializer.Serialize(postChangeHistory), ex);

				return Problem(ex.Message);
			}


			return CreatedAtRoute("PostChangeHistory", new { id = postChangeHistory.id }, Database.PostChangeHistory.CreateAnonymousWithFirstLevelSubObjects(postChangeHistory));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a PostChangeHistory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PostChangeHistory/{id}")]
		[Route("api/PostChangeHistory")]
		public async Task<IActionResult> DeletePostChangeHistory(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.PostChangeHistory> query = (from x in _context.PostChangeHistories
				where
				(x.id == id)
				select x);


			Database.PostChangeHistory postChangeHistory = await query.FirstOrDefaultAsync(cancellationToken);

			if (postChangeHistory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.PostChangeHistory DELETE", id.ToString(), new Exception("No Community.PostChangeHistory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PostChangeHistory cloneOfExisting = (Database.PostChangeHistory)_context.Entry(postChangeHistory).GetDatabaseValues().ToObject();


			try
			{
				_context.PostChangeHistories.Remove(postChangeHistory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.PostChangeHistory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostChangeHistory.CreateAnonymousWithFirstLevelSubObjects(postChangeHistory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.PostChangeHistory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PostChangeHistory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PostChangeHistory.CreateAnonymousWithFirstLevelSubObjects(postChangeHistory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of PostChangeHistory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PostChangeHistories/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? Id = null,
			int? PostId = null,
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

			IQueryable<Database.PostChangeHistory> query = (from pch in _context.PostChangeHistories select pch);
			if (Id.HasValue == true)
			{
				query = query.Where(pch => pch.Id == Id.Value);
			}
			if (PostId.HasValue == true)
			{
				query = query.Where(pch => pch.PostId == PostId.Value);
			}
			if (VersionNumber.HasValue == true)
			{
				query = query.Where(pch => pch.VersionNumber == VersionNumber.Value);
			}
			if (TimeStamp.HasValue == true)
			{
				query = query.Where(pch => pch.TimeStamp == TimeStamp.Value);
			}
			if (UserId.HasValue == true)
			{
				query = query.Where(pch => pch.UserId == UserId.Value);
			}
			if (string.IsNullOrEmpty(Data) == false)
			{
				query = query.Where(pch => pch.Data == Data);
			}


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PostChangeHistory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PostChangeHistory/CreateAuditEvent")]
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
