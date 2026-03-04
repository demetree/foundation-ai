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

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the TimeZone entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TimeZone entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TimeZonesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<TimeZonesController> _logger;

		public TimeZonesController(SchedulerContext context, ILogger<TimeZonesController> logger) : base("Scheduler", "TimeZone")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TimeZones filtered by the parameters provided.
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
		[Route("api/TimeZones")]
		public async Task<IActionResult> GetTimeZones(
			string name = null,
			string description = null,
			string ianaTimeZone = null,
			string abbreviation = null,
			string abbreviationDaylightSavings = null,
			bool? supportsDaylightSavings = null,
			float? standardUTCOffsetHours = null,
			float? dstUTCOffsetHours = null,
			int? sequence = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.TimeZone> query = (from tz in _context.TimeZones select tz);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(tz => tz.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(tz => tz.description == description);
			}
			if (string.IsNullOrEmpty(ianaTimeZone) == false)
			{
				query = query.Where(tz => tz.ianaTimeZone == ianaTimeZone);
			}
			if (string.IsNullOrEmpty(abbreviation) == false)
			{
				query = query.Where(tz => tz.abbreviation == abbreviation);
			}
			if (string.IsNullOrEmpty(abbreviationDaylightSavings) == false)
			{
				query = query.Where(tz => tz.abbreviationDaylightSavings == abbreviationDaylightSavings);
			}
			if (supportsDaylightSavings.HasValue == true)
			{
				query = query.Where(tz => tz.supportsDaylightSavings == supportsDaylightSavings.Value);
			}
			if (standardUTCOffsetHours.HasValue == true)
			{
				query = query.Where(tz => tz.standardUTCOffsetHours == standardUTCOffsetHours.Value);
			}
			if (dstUTCOffsetHours.HasValue == true)
			{
				query = query.Where(tz => tz.dstUTCOffsetHours == dstUTCOffsetHours.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(tz => tz.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tz => tz.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tz => tz.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tz => tz.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tz => tz.deleted == false);
				}
			}
			else
			{
				query = query.Where(tz => tz.active == true);
				query = query.Where(tz => tz.deleted == false);
			}

			query = query.OrderBy(tz => tz.sequence).ThenBy(tz => tz.name).ThenBy(tz => tz.description).ThenBy(tz => tz.ianaTimeZone);


			//
			// Add the any string contains parameter to span all the string fields on the Time Zone, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.ianaTimeZone.Contains(anyStringContains)
			       || x.abbreviation.Contains(anyStringContains)
			       || x.abbreviationDaylightSavings.Contains(anyStringContains)
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
			
			List<Database.TimeZone> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TimeZone timeZone in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(timeZone, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.TimeZone Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.TimeZone Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TimeZones filtered by the parameters provided.  Its query is similar to the GetTimeZones method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TimeZones/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string ianaTimeZone = null,
			string abbreviation = null,
			string abbreviationDaylightSavings = null,
			bool? supportsDaylightSavings = null,
			float? standardUTCOffsetHours = null,
			float? dstUTCOffsetHours = null,
			int? sequence = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.TimeZone> query = (from tz in _context.TimeZones select tz);
			if (name != null)
			{
				query = query.Where(tz => tz.name == name);
			}
			if (description != null)
			{
				query = query.Where(tz => tz.description == description);
			}
			if (ianaTimeZone != null)
			{
				query = query.Where(tz => tz.ianaTimeZone == ianaTimeZone);
			}
			if (abbreviation != null)
			{
				query = query.Where(tz => tz.abbreviation == abbreviation);
			}
			if (abbreviationDaylightSavings != null)
			{
				query = query.Where(tz => tz.abbreviationDaylightSavings == abbreviationDaylightSavings);
			}
			if (supportsDaylightSavings.HasValue == true)
			{
				query = query.Where(tz => tz.supportsDaylightSavings == supportsDaylightSavings.Value);
			}
			if (standardUTCOffsetHours.HasValue == true)
			{
				query = query.Where(tz => tz.standardUTCOffsetHours == standardUTCOffsetHours.Value);
			}
			if (dstUTCOffsetHours.HasValue == true)
			{
				query = query.Where(tz => tz.dstUTCOffsetHours == dstUTCOffsetHours.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(tz => tz.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tz => tz.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tz => tz.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tz => tz.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tz => tz.deleted == false);
				}
			}
			else
			{
				query = query.Where(tz => tz.active == true);
				query = query.Where(tz => tz.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Time Zone, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.ianaTimeZone.Contains(anyStringContains)
			       || x.abbreviation.Contains(anyStringContains)
			       || x.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single TimeZone by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TimeZone/{id}")]
		public async Task<IActionResult> GetTimeZone(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.TimeZone> query = (from tz in _context.TimeZones where
							(tz.id == id) &&
							(userIsAdmin == true || tz.deleted == false) &&
							(userIsWriter == true || tz.active == true)
					select tz);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.TimeZone materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.TimeZone Entity was read with Admin privilege." : "Scheduler.TimeZone Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TimeZone", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.TimeZone entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.TimeZone.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.TimeZone.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TimeZone record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TimeZone/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTimeZone(int id, [FromBody]Database.TimeZone.TimeZoneDTO timeZoneDTO, CancellationToken cancellationToken = default)
		{
			if (timeZoneDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != timeZoneDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.TimeZone> query = (from x in _context.TimeZones
				where
				(x.id == id)
				select x);


			Database.TimeZone existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.TimeZone PUT", id.ToString(), new Exception("No Scheduler.TimeZone entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (timeZoneDTO.objectGuid == Guid.Empty)
            {
                timeZoneDTO.objectGuid = existing.objectGuid;
            }
            else if (timeZoneDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a TimeZone record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TimeZone cloneOfExisting = (Database.TimeZone)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TimeZone object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TimeZone timeZone = (Database.TimeZone)_context.Entry(existing).GetDatabaseValues().ToObject();
			timeZone.ApplyDTO(timeZoneDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (timeZone.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.TimeZone record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (timeZone.name != null && timeZone.name.Length > 100)
			{
				timeZone.name = timeZone.name.Substring(0, 100);
			}

			if (timeZone.description != null && timeZone.description.Length > 500)
			{
				timeZone.description = timeZone.description.Substring(0, 500);
			}

			if (timeZone.ianaTimeZone != null && timeZone.ianaTimeZone.Length > 50)
			{
				timeZone.ianaTimeZone = timeZone.ianaTimeZone.Substring(0, 50);
			}

			if (timeZone.abbreviation != null && timeZone.abbreviation.Length > 50)
			{
				timeZone.abbreviation = timeZone.abbreviation.Substring(0, 50);
			}

			if (timeZone.abbreviationDaylightSavings != null && timeZone.abbreviationDaylightSavings.Length > 50)
			{
				timeZone.abbreviationDaylightSavings = timeZone.abbreviationDaylightSavings.Substring(0, 50);
			}

			EntityEntry<Database.TimeZone> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(timeZone);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.TimeZone entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TimeZone.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TimeZone.CreateAnonymousWithFirstLevelSubObjects(timeZone)),
					null);


				return Ok(Database.TimeZone.CreateAnonymous(timeZone));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.TimeZone entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TimeZone.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TimeZone.CreateAnonymousWithFirstLevelSubObjects(timeZone)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TimeZone record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TimeZone", Name = "TimeZone")]
		public async Task<IActionResult> PostTimeZone([FromBody]Database.TimeZone.TimeZoneDTO timeZoneDTO, CancellationToken cancellationToken = default)
		{
			if (timeZoneDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new TimeZone object using the data from the DTO
			//
			Database.TimeZone timeZone = Database.TimeZone.FromDTO(timeZoneDTO);

			try
			{
				if (timeZone.name != null && timeZone.name.Length > 100)
				{
					timeZone.name = timeZone.name.Substring(0, 100);
				}

				if (timeZone.description != null && timeZone.description.Length > 500)
				{
					timeZone.description = timeZone.description.Substring(0, 500);
				}

				if (timeZone.ianaTimeZone != null && timeZone.ianaTimeZone.Length > 50)
				{
					timeZone.ianaTimeZone = timeZone.ianaTimeZone.Substring(0, 50);
				}

				if (timeZone.abbreviation != null && timeZone.abbreviation.Length > 50)
				{
					timeZone.abbreviation = timeZone.abbreviation.Substring(0, 50);
				}

				if (timeZone.abbreviationDaylightSavings != null && timeZone.abbreviationDaylightSavings.Length > 50)
				{
					timeZone.abbreviationDaylightSavings = timeZone.abbreviationDaylightSavings.Substring(0, 50);
				}

				timeZone.objectGuid = Guid.NewGuid();
				_context.TimeZones.Add(timeZone);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.TimeZone entity successfully created.",
					true,
					timeZone.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TimeZone.CreateAnonymousWithFirstLevelSubObjects(timeZone)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.TimeZone entity creation failed.", false, timeZone.id.ToString(), "", JsonSerializer.Serialize(timeZone), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TimeZone", timeZone.id, timeZone.name));

			return CreatedAtRoute("TimeZone", new { id = timeZone.id }, Database.TimeZone.CreateAnonymousWithFirstLevelSubObjects(timeZone));
		}



        /// <summary>
        /// 
        /// This deletes a TimeZone record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TimeZone/{id}")]
		[Route("api/TimeZone")]
		public async Task<IActionResult> DeleteTimeZone(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.TimeZone> query = (from x in _context.TimeZones
				where
				(x.id == id)
				select x);


			Database.TimeZone timeZone = await query.FirstOrDefaultAsync(cancellationToken);

			if (timeZone == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.TimeZone DELETE", id.ToString(), new Exception("No Scheduler.TimeZone entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TimeZone cloneOfExisting = (Database.TimeZone)_context.Entry(timeZone).GetDatabaseValues().ToObject();


			try
			{
				timeZone.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.TimeZone entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TimeZone.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TimeZone.CreateAnonymousWithFirstLevelSubObjects(timeZone)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.TimeZone entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TimeZone.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TimeZone.CreateAnonymousWithFirstLevelSubObjects(timeZone)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TimeZone records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TimeZones/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string ianaTimeZone = null,
			string abbreviation = null,
			string abbreviationDaylightSavings = null,
			bool? supportsDaylightSavings = null,
			float? standardUTCOffsetHours = null,
			float? dstUTCOffsetHours = null,
			int? sequence = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.TimeZone> query = (from tz in _context.TimeZones select tz);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(tz => tz.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(tz => tz.description == description);
			}
			if (string.IsNullOrEmpty(ianaTimeZone) == false)
			{
				query = query.Where(tz => tz.ianaTimeZone == ianaTimeZone);
			}
			if (string.IsNullOrEmpty(abbreviation) == false)
			{
				query = query.Where(tz => tz.abbreviation == abbreviation);
			}
			if (string.IsNullOrEmpty(abbreviationDaylightSavings) == false)
			{
				query = query.Where(tz => tz.abbreviationDaylightSavings == abbreviationDaylightSavings);
			}
			if (supportsDaylightSavings.HasValue == true)
			{
				query = query.Where(tz => tz.supportsDaylightSavings == supportsDaylightSavings.Value);
			}
			if (standardUTCOffsetHours.HasValue == true)
			{
				query = query.Where(tz => tz.standardUTCOffsetHours == standardUTCOffsetHours.Value);
			}
			if (dstUTCOffsetHours.HasValue == true)
			{
				query = query.Where(tz => tz.dstUTCOffsetHours == dstUTCOffsetHours.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(tz => tz.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tz => tz.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tz => tz.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tz => tz.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tz => tz.deleted == false);
				}
			}
			else
			{
				query = query.Where(tz => tz.active == true);
				query = query.Where(tz => tz.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Time Zone, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.ianaTimeZone.Contains(anyStringContains)
			       || x.abbreviation.Contains(anyStringContains)
			       || x.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.ianaTimeZone);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TimeZone.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TimeZone/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
