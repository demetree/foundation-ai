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

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the PlatformAnnouncement entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PlatformAnnouncement entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PlatformAnnouncementsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		private BMCContext _context;

		private ILogger<PlatformAnnouncementsController> _logger;

		public PlatformAnnouncementsController(BMCContext context, ILogger<PlatformAnnouncementsController> logger) : base("BMC", "PlatformAnnouncement")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PlatformAnnouncements filtered by the parameters provided.
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
		[Route("api/PlatformAnnouncements")]
		public async Task<IActionResult> GetPlatformAnnouncements(
			string name = null,
			string body = null,
			string announcementType = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			bool? isActive = null,
			int? priority = null,
			bool? showOnLandingPage = null,
			bool? showOnDashboard = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startDate.HasValue == true && startDate.Value.Kind != DateTimeKind.Utc)
			{
				startDate = startDate.Value.ToUniversalTime();
			}

			if (endDate.HasValue == true && endDate.Value.Kind != DateTimeKind.Utc)
			{
				endDate = endDate.Value.ToUniversalTime();
			}

			IQueryable<Database.PlatformAnnouncement> query = (from pa in _context.PlatformAnnouncements select pa);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pa => pa.name == name);
			}
			if (string.IsNullOrEmpty(body) == false)
			{
				query = query.Where(pa => pa.body == body);
			}
			if (string.IsNullOrEmpty(announcementType) == false)
			{
				query = query.Where(pa => pa.announcementType == announcementType);
			}
			if (startDate.HasValue == true)
			{
				query = query.Where(pa => pa.startDate == startDate.Value);
			}
			if (endDate.HasValue == true)
			{
				query = query.Where(pa => pa.endDate == endDate.Value);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(pa => pa.isActive == isActive.Value);
			}
			if (priority.HasValue == true)
			{
				query = query.Where(pa => pa.priority == priority.Value);
			}
			if (showOnLandingPage.HasValue == true)
			{
				query = query.Where(pa => pa.showOnLandingPage == showOnLandingPage.Value);
			}
			if (showOnDashboard.HasValue == true)
			{
				query = query.Where(pa => pa.showOnDashboard == showOnDashboard.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pa => pa.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pa => pa.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pa => pa.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pa => pa.deleted == false);
				}
			}
			else
			{
				query = query.Where(pa => pa.active == true);
				query = query.Where(pa => pa.deleted == false);
			}

			query = query.OrderBy(pa => pa.name).ThenBy(pa => pa.announcementType);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Platform Announcement, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.body.Contains(anyStringContains)
			       || x.announcementType.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.PlatformAnnouncement> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PlatformAnnouncement platformAnnouncement in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(platformAnnouncement, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.PlatformAnnouncement Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.PlatformAnnouncement Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PlatformAnnouncements filtered by the parameters provided.  Its query is similar to the GetPlatformAnnouncements method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlatformAnnouncements/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string body = null,
			string announcementType = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			bool? isActive = null,
			int? priority = null,
			bool? showOnLandingPage = null,
			bool? showOnDashboard = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (startDate.HasValue == true && startDate.Value.Kind != DateTimeKind.Utc)
			{
				startDate = startDate.Value.ToUniversalTime();
			}

			if (endDate.HasValue == true && endDate.Value.Kind != DateTimeKind.Utc)
			{
				endDate = endDate.Value.ToUniversalTime();
			}

			IQueryable<Database.PlatformAnnouncement> query = (from pa in _context.PlatformAnnouncements select pa);
			if (name != null)
			{
				query = query.Where(pa => pa.name == name);
			}
			if (body != null)
			{
				query = query.Where(pa => pa.body == body);
			}
			if (announcementType != null)
			{
				query = query.Where(pa => pa.announcementType == announcementType);
			}
			if (startDate.HasValue == true)
			{
				query = query.Where(pa => pa.startDate == startDate.Value);
			}
			if (endDate.HasValue == true)
			{
				query = query.Where(pa => pa.endDate == endDate.Value);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(pa => pa.isActive == isActive.Value);
			}
			if (priority.HasValue == true)
			{
				query = query.Where(pa => pa.priority == priority.Value);
			}
			if (showOnLandingPage.HasValue == true)
			{
				query = query.Where(pa => pa.showOnLandingPage == showOnLandingPage.Value);
			}
			if (showOnDashboard.HasValue == true)
			{
				query = query.Where(pa => pa.showOnDashboard == showOnDashboard.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pa => pa.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pa => pa.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pa => pa.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pa => pa.deleted == false);
				}
			}
			else
			{
				query = query.Where(pa => pa.active == true);
				query = query.Where(pa => pa.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Platform Announcement, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.body.Contains(anyStringContains)
			       || x.announcementType.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PlatformAnnouncement by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlatformAnnouncement/{id}")]
		public async Task<IActionResult> GetPlatformAnnouncement(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.PlatformAnnouncement> query = (from pa in _context.PlatformAnnouncements where
							(pa.id == id) &&
							(userIsAdmin == true || pa.deleted == false) &&
							(userIsWriter == true || pa.active == true)
					select pa);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.PlatformAnnouncement materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.PlatformAnnouncement Entity was read with Admin privilege." : "BMC.PlatformAnnouncement Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PlatformAnnouncement", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.PlatformAnnouncement entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.PlatformAnnouncement.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.PlatformAnnouncement.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PlatformAnnouncement record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PlatformAnnouncement/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPlatformAnnouncement(int id, [FromBody]Database.PlatformAnnouncement.PlatformAnnouncementDTO platformAnnouncementDTO, CancellationToken cancellationToken = default)
		{
			if (platformAnnouncementDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Moderator role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Moderator", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != platformAnnouncementDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.PlatformAnnouncement> query = (from x in _context.PlatformAnnouncements
				where
				(x.id == id)
				select x);


			Database.PlatformAnnouncement existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PlatformAnnouncement PUT", id.ToString(), new Exception("No BMC.PlatformAnnouncement entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (platformAnnouncementDTO.objectGuid == Guid.Empty)
            {
                platformAnnouncementDTO.objectGuid = existing.objectGuid;
            }
            else if (platformAnnouncementDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PlatformAnnouncement record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PlatformAnnouncement cloneOfExisting = (Database.PlatformAnnouncement)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PlatformAnnouncement object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PlatformAnnouncement platformAnnouncement = (Database.PlatformAnnouncement)_context.Entry(existing).GetDatabaseValues().ToObject();
			platformAnnouncement.ApplyDTO(platformAnnouncementDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (platformAnnouncement.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.PlatformAnnouncement record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (platformAnnouncement.name != null && platformAnnouncement.name.Length > 100)
			{
				platformAnnouncement.name = platformAnnouncement.name.Substring(0, 100);
			}

			if (platformAnnouncement.announcementType != null && platformAnnouncement.announcementType.Length > 50)
			{
				platformAnnouncement.announcementType = platformAnnouncement.announcementType.Substring(0, 50);
			}

			if (platformAnnouncement.startDate.Kind != DateTimeKind.Utc)
			{
				platformAnnouncement.startDate = platformAnnouncement.startDate.ToUniversalTime();
			}

			if (platformAnnouncement.endDate.HasValue == true && platformAnnouncement.endDate.Value.Kind != DateTimeKind.Utc)
			{
				platformAnnouncement.endDate = platformAnnouncement.endDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.PlatformAnnouncement> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(platformAnnouncement);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.PlatformAnnouncement entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PlatformAnnouncement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PlatformAnnouncement.CreateAnonymousWithFirstLevelSubObjects(platformAnnouncement)),
					null);


				return Ok(Database.PlatformAnnouncement.CreateAnonymous(platformAnnouncement));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.PlatformAnnouncement entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PlatformAnnouncement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PlatformAnnouncement.CreateAnonymousWithFirstLevelSubObjects(platformAnnouncement)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new PlatformAnnouncement record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlatformAnnouncement", Name = "PlatformAnnouncement")]
		public async Task<IActionResult> PostPlatformAnnouncement([FromBody]Database.PlatformAnnouncement.PlatformAnnouncementDTO platformAnnouncementDTO, CancellationToken cancellationToken = default)
		{
			if (platformAnnouncementDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Moderator role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Moderator", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new PlatformAnnouncement object using the data from the DTO
			//
			Database.PlatformAnnouncement platformAnnouncement = Database.PlatformAnnouncement.FromDTO(platformAnnouncementDTO);

			try
			{
				if (platformAnnouncement.name != null && platformAnnouncement.name.Length > 100)
				{
					platformAnnouncement.name = platformAnnouncement.name.Substring(0, 100);
				}

				if (platformAnnouncement.announcementType != null && platformAnnouncement.announcementType.Length > 50)
				{
					platformAnnouncement.announcementType = platformAnnouncement.announcementType.Substring(0, 50);
				}

				if (platformAnnouncement.startDate.Kind != DateTimeKind.Utc)
				{
					platformAnnouncement.startDate = platformAnnouncement.startDate.ToUniversalTime();
				}

				if (platformAnnouncement.endDate.HasValue == true && platformAnnouncement.endDate.Value.Kind != DateTimeKind.Utc)
				{
					platformAnnouncement.endDate = platformAnnouncement.endDate.Value.ToUniversalTime();
				}

				platformAnnouncement.objectGuid = Guid.NewGuid();
				_context.PlatformAnnouncements.Add(platformAnnouncement);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.PlatformAnnouncement entity successfully created.",
					true,
					platformAnnouncement.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PlatformAnnouncement.CreateAnonymousWithFirstLevelSubObjects(platformAnnouncement)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.PlatformAnnouncement entity creation failed.", false, platformAnnouncement.id.ToString(), "", JsonSerializer.Serialize(platformAnnouncement), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PlatformAnnouncement", platformAnnouncement.id, platformAnnouncement.name));

			return CreatedAtRoute("PlatformAnnouncement", new { id = platformAnnouncement.id }, Database.PlatformAnnouncement.CreateAnonymousWithFirstLevelSubObjects(platformAnnouncement));
		}



        /// <summary>
        /// 
        /// This deletes a PlatformAnnouncement record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PlatformAnnouncement/{id}")]
		[Route("api/PlatformAnnouncement")]
		public async Task<IActionResult> DeletePlatformAnnouncement(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Moderator role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Moderator", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.PlatformAnnouncement> query = (from x in _context.PlatformAnnouncements
				where
				(x.id == id)
				select x);


			Database.PlatformAnnouncement platformAnnouncement = await query.FirstOrDefaultAsync(cancellationToken);

			if (platformAnnouncement == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PlatformAnnouncement DELETE", id.ToString(), new Exception("No BMC.PlatformAnnouncement entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PlatformAnnouncement cloneOfExisting = (Database.PlatformAnnouncement)_context.Entry(platformAnnouncement).GetDatabaseValues().ToObject();


			try
			{
				platformAnnouncement.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.PlatformAnnouncement entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PlatformAnnouncement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PlatformAnnouncement.CreateAnonymousWithFirstLevelSubObjects(platformAnnouncement)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.PlatformAnnouncement entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PlatformAnnouncement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PlatformAnnouncement.CreateAnonymousWithFirstLevelSubObjects(platformAnnouncement)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of PlatformAnnouncement records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PlatformAnnouncements/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string body = null,
			string announcementType = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			bool? isActive = null,
			int? priority = null,
			bool? showOnLandingPage = null,
			bool? showOnDashboard = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startDate.HasValue == true && startDate.Value.Kind != DateTimeKind.Utc)
			{
				startDate = startDate.Value.ToUniversalTime();
			}

			if (endDate.HasValue == true && endDate.Value.Kind != DateTimeKind.Utc)
			{
				endDate = endDate.Value.ToUniversalTime();
			}

			IQueryable<Database.PlatformAnnouncement> query = (from pa in _context.PlatformAnnouncements select pa);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pa => pa.name == name);
			}
			if (string.IsNullOrEmpty(body) == false)
			{
				query = query.Where(pa => pa.body == body);
			}
			if (string.IsNullOrEmpty(announcementType) == false)
			{
				query = query.Where(pa => pa.announcementType == announcementType);
			}
			if (startDate.HasValue == true)
			{
				query = query.Where(pa => pa.startDate == startDate.Value);
			}
			if (endDate.HasValue == true)
			{
				query = query.Where(pa => pa.endDate == endDate.Value);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(pa => pa.isActive == isActive.Value);
			}
			if (priority.HasValue == true)
			{
				query = query.Where(pa => pa.priority == priority.Value);
			}
			if (showOnLandingPage.HasValue == true)
			{
				query = query.Where(pa => pa.showOnLandingPage == showOnLandingPage.Value);
			}
			if (showOnDashboard.HasValue == true)
			{
				query = query.Where(pa => pa.showOnDashboard == showOnDashboard.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pa => pa.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pa => pa.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pa => pa.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pa => pa.deleted == false);
				}
			}
			else
			{
				query = query.Where(pa => pa.active == true);
				query = query.Where(pa => pa.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Platform Announcement, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.body.Contains(anyStringContains)
			       || x.announcementType.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.announcementType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PlatformAnnouncement.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PlatformAnnouncement/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Moderator role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Moderator", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
