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
using Foundation.ChangeHistory;

namespace Foundation.Community.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Announcement entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Announcement entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AnnouncementsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 10;

		static object announcementPutSyncRoot = new object();
		static object announcementDeleteSyncRoot = new object();

		private CommunityContext _context;

		private ILogger<AnnouncementsController> _logger;

		public AnnouncementsController(CommunityContext context, ILogger<AnnouncementsController> logger) : base("Community", "Announcement")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Announcements filtered by the parameters provided.
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
		[Route("api/Announcements")]
		public async Task<IActionResult> GetAnnouncements(
			int? Id = null,
			string Title = null,
			string Body = null,
			string Severity = null,
			DateTime? StartDate = null,
			DateTime? EndDate = null,
			bool? IsPinned = null,
			int? VersionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
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
			if (StartDate.HasValue == true && StartDate.Value.Kind != DateTimeKind.Utc)
			{
				StartDate = StartDate.Value.ToUniversalTime();
			}

			if (EndDate.HasValue == true && EndDate.Value.Kind != DateTimeKind.Utc)
			{
				EndDate = EndDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Announcement> query = (from a in _context.Announcements select a);
			if (Id.HasValue == true)
			{
				query = query.Where(a => a.Id == Id.Value);
			}
			if (string.IsNullOrEmpty(Title) == false)
			{
				query = query.Where(a => a.Title == Title);
			}
			if (string.IsNullOrEmpty(Body) == false)
			{
				query = query.Where(a => a.Body == Body);
			}
			if (string.IsNullOrEmpty(Severity) == false)
			{
				query = query.Where(a => a.Severity == Severity);
			}
			if (StartDate.HasValue == true)
			{
				query = query.Where(a => a.StartDate == StartDate.Value);
			}
			if (EndDate.HasValue == true)
			{
				query = query.Where(a => a.EndDate == EndDate.Value);
			}
			if (IsPinned.HasValue == true)
			{
				query = query.Where(a => a.IsPinned == IsPinned.Value);
			}
			if (VersionNumber.HasValue == true)
			{
				query = query.Where(a => a.VersionNumber == VersionNumber.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(a => a.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(a => a.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(a => a.Deleted == Deleted.Value);
			}

			query = query.OrderBy(a => a.title).ThenBy(a => a.severity);


			//
			// Add the any string contains parameter to span all the string fields on the Announcement, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.Title.Contains(anyStringContains)
			       || x.Body.Contains(anyStringContains)
			       || x.Severity.Contains(anyStringContains)
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
			
			List<Database.Announcement> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Announcement announcement in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(announcement, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.Announcement Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.Announcement Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Announcements filtered by the parameters provided.  Its query is similar to the GetAnnouncements method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Announcements/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? Id = null,
			string Title = null,
			string Body = null,
			string Severity = null,
			DateTime? StartDate = null,
			DateTime? EndDate = null,
			bool? IsPinned = null,
			int? VersionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (StartDate.HasValue == true && StartDate.Value.Kind != DateTimeKind.Utc)
			{
				StartDate = StartDate.Value.ToUniversalTime();
			}

			if (EndDate.HasValue == true && EndDate.Value.Kind != DateTimeKind.Utc)
			{
				EndDate = EndDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Announcement> query = (from a in _context.Announcements select a);
			if (Id.HasValue == true)
			{
				query = query.Where(a => a.Id == Id.Value);
			}
			if (Title != null)
			{
				query = query.Where(a => a.Title == Title);
			}
			if (Body != null)
			{
				query = query.Where(a => a.Body == Body);
			}
			if (Severity != null)
			{
				query = query.Where(a => a.Severity == Severity);
			}
			if (StartDate.HasValue == true)
			{
				query = query.Where(a => a.StartDate == StartDate.Value);
			}
			if (EndDate.HasValue == true)
			{
				query = query.Where(a => a.EndDate == EndDate.Value);
			}
			if (IsPinned.HasValue == true)
			{
				query = query.Where(a => a.IsPinned == IsPinned.Value);
			}
			if (VersionNumber.HasValue == true)
			{
				query = query.Where(a => a.VersionNumber == VersionNumber.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(a => a.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(a => a.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(a => a.Deleted == Deleted.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Announcement, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.Title.Contains(anyStringContains)
			       || x.Body.Contains(anyStringContains)
			       || x.Severity.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Announcement by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Announcement/{id}")]
		public async Task<IActionResult> GetAnnouncement(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.Announcement> query = (from a in _context.Announcements where
				(a.id == id)
					select a);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.Announcement materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.Announcement Entity was read with Admin privilege." : "Community.Announcement Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Announcement", materialized.id, materialized.title));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.Announcement entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.Announcement.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.Announcement.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Announcement record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Announcement/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAnnouncement(int id, [FromBody]Database.Announcement.AnnouncementDTO announcementDTO, CancellationToken cancellationToken = default)
		{
			if (announcementDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != announcementDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.Announcement> query = (from x in _context.Announcements
				where
				(x.id == id)
				select x);


			Database.Announcement existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.Announcement PUT", id.ToString(), new Exception("No Community.Announcement entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (announcementDTO.objectGuid == Guid.Empty)
            {
                announcementDTO.objectGuid = existing.objectGuid;
            }
            else if (announcementDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Announcement record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Announcement cloneOfExisting = (Database.Announcement)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Announcement object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Announcement announcement = (Database.Announcement)_context.Entry(existing).GetDatabaseValues().ToObject();
			announcement.ApplyDTO(announcementDTO);
			lock (announcementPutSyncRoot)
			{
				//
				// Validate the version number for the announcement being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != announcement.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Announcement save attempt was made but save request was with version " + announcement.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Announcement you are trying to update has already changed.  Please try your save again after reloading the Announcement.");
				}
				else
				{
					// Same record.  Increase version.
					announcement.versionNumber++;
				}



				if (announcement.StartDate.Kind != DateTimeKind.Utc)
				{
					announcement.StartDate = announcement.StartDate.ToUniversalTime();
				}

				if (announcement.EndDate.HasValue == true && announcement.EndDate.Value.Kind != DateTimeKind.Utc)
				{
					announcement.EndDate = announcement.EndDate.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.Announcement> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(announcement);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        AnnouncementChangeHistory announcementChangeHistory = new AnnouncementChangeHistory();
				        announcementChangeHistory.announcementId = announcement.id;
				        announcementChangeHistory.versionNumber = announcement.versionNumber;
				        announcementChangeHistory.timeStamp = DateTime.UtcNow;
				        announcementChangeHistory.userId = securityUser.id;
				        announcementChangeHistory.data = JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement));
				        _context.AnnouncementChangeHistories.Add(announcementChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Community.Announcement entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement)),
						null);

				return Ok(Database.Announcement.CreateAnonymous(announcement));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Community.Announcement entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Announcement record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Announcement", Name = "Announcement")]
		public async Task<IActionResult> PostAnnouncement([FromBody]Database.Announcement.AnnouncementDTO announcementDTO, CancellationToken cancellationToken = default)
		{
			if (announcementDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new Announcement object using the data from the DTO
			//
			Database.Announcement announcement = Database.Announcement.FromDTO(announcementDTO);

			try
			{
				if (announcement.StartDate.Kind != DateTimeKind.Utc)
				{
					announcement.StartDate = announcement.StartDate.ToUniversalTime();
				}

				if (announcement.EndDate.HasValue == true && announcement.EndDate.Value.Kind != DateTimeKind.Utc)
				{
					announcement.EndDate = announcement.EndDate.Value.ToUniversalTime();
				}

				announcement.versionNumber = 1;

				_context.Announcements.Add(announcement);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the announcement object so that no further changes will be written to the database
				    //
				    _context.Entry(announcement).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					announcement.AnnouncementChangeHistories = null;


				    AnnouncementChangeHistory announcementChangeHistory = new AnnouncementChangeHistory();
				    announcementChangeHistory.announcementId = announcement.id;
				    announcementChangeHistory.versionNumber = announcement.versionNumber;
				    announcementChangeHistory.timeStamp = DateTime.UtcNow;
				    announcementChangeHistory.userId = securityUser.id;
				    announcementChangeHistory.data = JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement));
				    _context.AnnouncementChangeHistories.Add(announcementChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Community.Announcement entity successfully created.",
						true,
						announcement. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.Announcement entity creation failed.", false, announcement.id.ToString(), "", JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Announcement", announcement.id, announcement.title));

			return CreatedAtRoute("Announcement", new { id = announcement.id }, Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement));
		}



        /// <summary>
        /// 
        /// This rolls a Announcement entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Announcement/Rollback/{id}")]
		[Route("api/Announcement/Rollback")]
		public async Task<IActionResult> RollbackToAnnouncementVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Announcement> query = (from x in _context.Announcements
			        where
			        (x.id == id)
			        select x);


			//
			// Make sure nobody else is editing this Announcement concurrently
			//
			lock (announcementPutSyncRoot)
			{
				
				Database.Announcement announcement = query.FirstOrDefault();
				
				if (announcement == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.Announcement rollback", id.ToString(), new Exception("No Community.Announcement entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Announcement current state so we can log it.
				//
				Database.Announcement cloneOfExisting = (Database.Announcement)_context.Entry(announcement).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.AnnouncementChangeHistories = null;

				if (versionNumber >= announcement.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Community.Announcement rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Community.Announcement rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				AnnouncementChangeHistory announcementChangeHistory = (from x in _context.AnnouncementChangeHistories
				                                               where
				                                               x.announcementId == id &&
				                                               x.versionNumber == versionNumber
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (announcementChangeHistory != null)
				{
				    Database.Announcement oldAnnouncement = JsonSerializer.Deserialize<Database.Announcement>(announcementChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    announcement.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    announcement.Id = oldAnnouncement.Id;
				    announcement.Title = oldAnnouncement.Title;
				    announcement.Body = oldAnnouncement.Body;
				    announcement.Severity = oldAnnouncement.Severity;
				    announcement.StartDate = oldAnnouncement.StartDate;
				    announcement.EndDate = oldAnnouncement.EndDate;
				    announcement.IsPinned = oldAnnouncement.IsPinned;
				    announcement.VersionNumber = oldAnnouncement.VersionNumber;
				    announcement.ObjectGuid = oldAnnouncement.ObjectGuid;
				    announcement.Active = oldAnnouncement.Active;
				    announcement.Deleted = oldAnnouncement.Deleted;

				    string serializedAnnouncement = JsonSerializer.Serialize(announcement);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        AnnouncementChangeHistory newAnnouncementChangeHistory = new AnnouncementChangeHistory();
				        newAnnouncementChangeHistory.announcementId = announcement.id;
				        newAnnouncementChangeHistory.versionNumber = announcement.versionNumber;
				        newAnnouncementChangeHistory.timeStamp = DateTime.UtcNow;
				        newAnnouncementChangeHistory.userId = securityUser.id;
				        newAnnouncementChangeHistory.data = JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement));
				        _context.AnnouncementChangeHistories.Add(newAnnouncementChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Community.Announcement rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement)),
						null);


				    return Ok(Database.Announcement.CreateAnonymous(announcement));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Community.Announcement rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Community.Announcement rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Announcement.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Announcement</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Announcement/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetAnnouncementChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.Announcement announcement = await _context.Announcements.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (announcement == null)
			{
				return NotFound();
			}

			try
			{
				announcement.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.Announcement> versionInfo = await announcement.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Announcement.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Announcement</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Announcement/{id}/AuditHistory")]
		public async Task<IActionResult> GetAnnouncementAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.Announcement announcement = await _context.Announcements.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (announcement == null)
			{
				return NotFound();
			}

			try
			{
				announcement.SetupVersionInquiry(_context, Guid.Empty);

				List<VersionInformation<Database.Announcement>> versions = await announcement.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Announcement.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Announcement</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Announcement object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Announcement/{id}/Version/{version}")]
		public async Task<IActionResult> GetAnnouncementVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.Announcement announcement = await _context.Announcements.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (announcement == null)
			{
				return NotFound();
			}

			try
			{
				announcement.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.Announcement> versionInfo = await announcement.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Announcement at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Announcement</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Announcement object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Announcement/{id}/StateAtTime")]
		public async Task<IActionResult> GetAnnouncementStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.Announcement announcement = await _context.Announcements.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (announcement == null)
			{
				return NotFound();
			}

			try
			{
				announcement.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.Announcement> versionInfo = await announcement.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Announcement record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Announcement/{id}")]
		[Route("api/Announcement")]
		public async Task<IActionResult> DeleteAnnouncement(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.Announcement> query = (from x in _context.Announcements
				where
				(x.id == id)
				select x);


			Database.Announcement announcement = await query.FirstOrDefaultAsync(cancellationToken);

			if (announcement == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.Announcement DELETE", id.ToString(), new Exception("No Community.Announcement entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Announcement cloneOfExisting = (Database.Announcement)_context.Entry(announcement).GetDatabaseValues().ToObject();


			lock (announcementDeleteSyncRoot)
			{
			    try
			    {
			        announcement.deleted = true;
			        announcement.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        AnnouncementChangeHistory announcementChangeHistory = new AnnouncementChangeHistory();
			        announcementChangeHistory.announcementId = announcement.id;
			        announcementChangeHistory.versionNumber = announcement.versionNumber;
			        announcementChangeHistory.timeStamp = DateTime.UtcNow;
			        announcementChangeHistory.userId = securityUser.id;
			        announcementChangeHistory.data = JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement));
			        _context.AnnouncementChangeHistories.Add(announcementChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Community.Announcement entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Community.Announcement entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Announcement.CreateAnonymousWithFirstLevelSubObjects(announcement)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Announcement records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Announcements/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? Id = null,
			string Title = null,
			string Body = null,
			string Severity = null,
			DateTime? StartDate = null,
			DateTime? EndDate = null,
			bool? IsPinned = null,
			int? VersionNumber = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);


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
			if (StartDate.HasValue == true && StartDate.Value.Kind != DateTimeKind.Utc)
			{
				StartDate = StartDate.Value.ToUniversalTime();
			}

			if (EndDate.HasValue == true && EndDate.Value.Kind != DateTimeKind.Utc)
			{
				EndDate = EndDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Announcement> query = (from a in _context.Announcements select a);
			if (Id.HasValue == true)
			{
				query = query.Where(a => a.Id == Id.Value);
			}
			if (string.IsNullOrEmpty(Title) == false)
			{
				query = query.Where(a => a.Title == Title);
			}
			if (string.IsNullOrEmpty(Body) == false)
			{
				query = query.Where(a => a.Body == Body);
			}
			if (string.IsNullOrEmpty(Severity) == false)
			{
				query = query.Where(a => a.Severity == Severity);
			}
			if (StartDate.HasValue == true)
			{
				query = query.Where(a => a.StartDate == StartDate.Value);
			}
			if (EndDate.HasValue == true)
			{
				query = query.Where(a => a.EndDate == EndDate.Value);
			}
			if (IsPinned.HasValue == true)
			{
				query = query.Where(a => a.IsPinned == IsPinned.Value);
			}
			if (VersionNumber.HasValue == true)
			{
				query = query.Where(a => a.VersionNumber == VersionNumber.Value);
			}
			if (ObjectGuid.HasValue == true)
			{
				query = query.Where(a => a.ObjectGuid == ObjectGuid);
			}
			if (Active.HasValue == true)
			{
				query = query.Where(a => a.Active == Active.Value);
			}
			if (Deleted.HasValue == true)
			{
				query = query.Where(a => a.Deleted == Deleted.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Announcement, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.Title.Contains(anyStringContains)
			       || x.Body.Contains(anyStringContains)
			       || x.Severity.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.title).ThenBy(x => x.severity);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Announcement.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Announcement/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Community Content Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Content Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
