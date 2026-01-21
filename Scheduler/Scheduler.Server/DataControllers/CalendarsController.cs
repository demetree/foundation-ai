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

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Calendar entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Calendar entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class CalendarsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object calendarPutSyncRoot = new object();
		static object calendarDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<CalendarsController> _logger;

		public CalendarsController(SchedulerContext context, ILogger<CalendarsController> logger) : base("Scheduler", "Calendar")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Calendars filtered by the parameters provided.
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
		[Route("api/Calendars")]
		public async Task<IActionResult> GetCalendars(
			string name = null,
			string description = null,
			int? officeId = null,
			bool? isDefault = null,
			int? iconId = null,
			string color = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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

			IQueryable<Database.Calendar> query = (from c in _context.Calendars select c);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(c => c.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(c => c.description == description);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(c => c.officeId == officeId.Value);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(c => c.isDefault == isDefault.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(c => c.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(c => c.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(c => c.versionNumber == versionNumber.Value);
			}

			query = query.OrderBy(c => c.name).ThenBy(c => c.description).ThenBy(c => c.color);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.icon);
				query = query.Include(x => x.office);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Calendar, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || (includeRelations == true && x.icon.name.Contains(anyStringContains))
			       || (includeRelations == true && x.icon.fontAwesomeCode.Contains(anyStringContains))
			       || (includeRelations == true && x.office.name.Contains(anyStringContains))
			       || (includeRelations == true && x.office.description.Contains(anyStringContains))
			       || (includeRelations == true && x.office.addressLine1.Contains(anyStringContains))
			       || (includeRelations == true && x.office.addressLine2.Contains(anyStringContains))
			       || (includeRelations == true && x.office.city.Contains(anyStringContains))
			       || (includeRelations == true && x.office.postalCode.Contains(anyStringContains))
			       || (includeRelations == true && x.office.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.office.email.Contains(anyStringContains))
			       || (includeRelations == true && x.office.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.office.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.office.color.Contains(anyStringContains))
			       || (includeRelations == true && x.office.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.office.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.office.avatarMimeType.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Calendar> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Calendar calendar in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(calendar, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Calendar Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Calendar Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Calendars filtered by the parameters provided.  Its query is similar to the GetCalendars method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Calendars/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? officeId = null,
			bool? isDefault = null,
			int? iconId = null,
			string color = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? versionNumber = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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


			IQueryable<Database.Calendar> query = (from c in _context.Calendars select c);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(c => c.name == name);
			}
			if (description != null)
			{
				query = query.Where(c => c.description == description);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(c => c.officeId == officeId.Value);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(c => c.isDefault == isDefault.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(c => c.iconId == iconId.Value);
			}
			if (color != null)
			{
				query = query.Where(c => c.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(c => c.versionNumber == versionNumber.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Calendar, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			       || x.office.name.Contains(anyStringContains)
			       || x.office.description.Contains(anyStringContains)
			       || x.office.addressLine1.Contains(anyStringContains)
			       || x.office.addressLine2.Contains(anyStringContains)
			       || x.office.city.Contains(anyStringContains)
			       || x.office.postalCode.Contains(anyStringContains)
			       || x.office.phone.Contains(anyStringContains)
			       || x.office.email.Contains(anyStringContains)
			       || x.office.notes.Contains(anyStringContains)
			       || x.office.externalId.Contains(anyStringContains)
			       || x.office.color.Contains(anyStringContains)
			       || x.office.attributes.Contains(anyStringContains)
			       || x.office.avatarFileName.Contains(anyStringContains)
			       || x.office.avatarMimeType.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Calendar by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Calendar/{id}")]
		public async Task<IActionResult> GetCalendar(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
				IQueryable<Database.Calendar> query = (from c in _context.Calendars where
							(c.id == id) &&
							(userIsAdmin == true || c.deleted == false) &&
							(userIsWriter == true || c.active == true)
					select c);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.icon);
					query = query.Include(x => x.office);
					query = query.AsSplitQuery();
				}

				Database.Calendar materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Calendar Entity was read with Admin privilege." : "Scheduler.Calendar Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Calendar", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Calendar entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Calendar.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Calendar.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Calendar record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Calendar/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutCalendar(int id, [FromBody]Database.Calendar.CalendarDTO calendarDTO, CancellationToken cancellationToken = default)
		{
			if (calendarDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != calendarDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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


			IQueryable<Database.Calendar> query = (from x in _context.Calendars
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Calendar existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Calendar PUT", id.ToString(), new Exception("No Scheduler.Calendar entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (calendarDTO.objectGuid == Guid.Empty)
            {
                calendarDTO.objectGuid = existing.objectGuid;
            }
            else if (calendarDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Calendar record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Calendar cloneOfExisting = (Database.Calendar)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Calendar object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Calendar calendar = (Database.Calendar)_context.Entry(existing).GetDatabaseValues().ToObject();
			calendar.ApplyDTO(calendarDTO);
			//
			// The tenant guid for any Calendar being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Calendar because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				calendar.tenantGuid = existing.tenantGuid;
			}

			lock (calendarPutSyncRoot)
			{
				//
				// Validate the version number for the calendar being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != calendar.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Calendar save attempt was made but save request was with version " + calendar.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Calendar you are trying to update has already changed.  Please try your save again after reloading the Calendar.");
				}
				else
				{
					// Same record.  Increase version.
					calendar.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (calendar.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Calendar record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (calendar.name != null && calendar.name.Length > 100)
				{
					calendar.name = calendar.name.Substring(0, 100);
				}

				if (calendar.description != null && calendar.description.Length > 500)
				{
					calendar.description = calendar.description.Substring(0, 500);
				}

				if (calendar.color != null && calendar.color.Length > 10)
				{
					calendar.color = calendar.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.Calendar> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(calendar);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        CalendarChangeHistory calendarChangeHistory = new CalendarChangeHistory();
				        calendarChangeHistory.calendarId = calendar.id;
				        calendarChangeHistory.versionNumber = calendar.versionNumber;
				        calendarChangeHistory.timeStamp = DateTime.UtcNow;
				        calendarChangeHistory.userId = securityUser.id;
				        calendarChangeHistory.tenantGuid = userTenantGuid;
				        calendarChangeHistory.data = JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar));
				        _context.CalendarChangeHistories.Add(calendarChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Calendar entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar)),
						null);

				return Ok(Database.Calendar.CreateAnonymous(calendar));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Calendar entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new Calendar record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Calendar", Name = "Calendar")]
		public async Task<IActionResult> PostCalendar([FromBody]Database.Calendar.CalendarDTO calendarDTO, CancellationToken cancellationToken = default)
		{
			if (calendarDTO == null)
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
			// Create a new Calendar object using the data from the DTO
			//
			Database.Calendar calendar = Database.Calendar.FromDTO(calendarDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				calendar.tenantGuid = userTenantGuid;

				if (calendar.name != null && calendar.name.Length > 100)
				{
					calendar.name = calendar.name.Substring(0, 100);
				}

				if (calendar.description != null && calendar.description.Length > 500)
				{
					calendar.description = calendar.description.Substring(0, 500);
				}

				if (calendar.color != null && calendar.color.Length > 10)
				{
					calendar.color = calendar.color.Substring(0, 10);
				}

				calendar.objectGuid = Guid.NewGuid();
				calendar.versionNumber = 1;

				_context.Calendars.Add(calendar);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the calendar object so that no further changes will be written to the database
				    //
				    _context.Entry(calendar).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					calendar.CalendarChangeHistories = null;
					calendar.Clients = null;
					calendar.EventCalendars = null;
					calendar.SchedulingTargets = null;
					calendar.icon = null;
					calendar.office = null;


				    CalendarChangeHistory calendarChangeHistory = new CalendarChangeHistory();
				    calendarChangeHistory.calendarId = calendar.id;
				    calendarChangeHistory.versionNumber = calendar.versionNumber;
				    calendarChangeHistory.timeStamp = DateTime.UtcNow;
				    calendarChangeHistory.userId = securityUser.id;
				    calendarChangeHistory.tenantGuid = userTenantGuid;
				    calendarChangeHistory.data = JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar));
				    _context.CalendarChangeHistories.Add(calendarChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Calendar entity successfully created.",
						true,
						calendar. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Calendar entity creation failed.", false, calendar.id.ToString(), "", JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Calendar", calendar.id, calendar.name));

			return CreatedAtRoute("Calendar", new { id = calendar.id }, Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar));
		}



        /// <summary>
        /// 
        /// This rolls a Calendar entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Calendar/Rollback/{id}")]
		[Route("api/Calendar/Rollback")]
		public async Task<IActionResult> RollbackToCalendarVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Calendar> query = (from x in _context.Calendars
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Calendar concurrently
			//
			lock (calendarPutSyncRoot)
			{
				
				Database.Calendar calendar = query.FirstOrDefault();
				
				if (calendar == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Calendar rollback", id.ToString(), new Exception("No Scheduler.Calendar entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Calendar current state so we can log it.
				//
				Database.Calendar cloneOfExisting = (Database.Calendar)_context.Entry(calendar).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.CalendarChangeHistories = null;
				cloneOfExisting.Clients = null;
				cloneOfExisting.EventCalendars = null;
				cloneOfExisting.SchedulingTargets = null;
				cloneOfExisting.icon = null;
				cloneOfExisting.office = null;

				if (versionNumber >= calendar.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Calendar rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Calendar rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				CalendarChangeHistory calendarChangeHistory = (from x in _context.CalendarChangeHistories
				                                               where
				                                               x.calendarId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (calendarChangeHistory != null)
				{
				    Database.Calendar oldCalendar = JsonSerializer.Deserialize<Database.Calendar>(calendarChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    calendar.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    calendar.name = oldCalendar.name;
				    calendar.description = oldCalendar.description;
				    calendar.officeId = oldCalendar.officeId;
				    calendar.isDefault = oldCalendar.isDefault;
				    calendar.iconId = oldCalendar.iconId;
				    calendar.color = oldCalendar.color;
				    calendar.objectGuid = oldCalendar.objectGuid;
				    calendar.active = oldCalendar.active;
				    calendar.deleted = oldCalendar.deleted;

				    string serializedCalendar = JsonSerializer.Serialize(calendar);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        CalendarChangeHistory newCalendarChangeHistory = new CalendarChangeHistory();
				        newCalendarChangeHistory.calendarId = calendar.id;
				        newCalendarChangeHistory.versionNumber = calendar.versionNumber;
				        newCalendarChangeHistory.timeStamp = DateTime.UtcNow;
				        newCalendarChangeHistory.userId = securityUser.id;
				        newCalendarChangeHistory.tenantGuid = userTenantGuid;
				        newCalendarChangeHistory.data = JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar));
				        _context.CalendarChangeHistories.Add(newCalendarChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Calendar rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar)),
						null);


				    return Ok(Database.Calendar.CreateAnonymous(calendar));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Calendar rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Calendar rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Calendar.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Calendar</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Calendar/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetCalendarChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Calendar calendar = await _context.Calendars.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (calendar == null)
			{
				return NotFound();
			}

			try
			{
				calendar.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Calendar> versionInfo = await calendar.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Calendar.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Calendar</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Calendar/{id}/AuditHistory")]
		public async Task<IActionResult> GetCalendarAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Calendar calendar = await _context.Calendars.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (calendar == null)
			{
				return NotFound();
			}

			try
			{
				calendar.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Calendar>> versions = await calendar.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Calendar.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Calendar</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Calendar object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Calendar/{id}/Version/{version}")]
		public async Task<IActionResult> GetCalendarVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Calendar calendar = await _context.Calendars.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (calendar == null)
			{
				return NotFound();
			}

			try
			{
				calendar.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Calendar> versionInfo = await calendar.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Calendar at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Calendar</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Calendar object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Calendar/{id}/StateAtTime")]
		public async Task<IActionResult> GetCalendarStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Calendar calendar = await _context.Calendars.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (calendar == null)
			{
				return NotFound();
			}

			try
			{
				calendar.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Calendar> versionInfo = await calendar.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Calendar record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Calendar/{id}")]
		[Route("api/Calendar")]
		public async Task<IActionResult> DeleteCalendar(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Calendar> query = (from x in _context.Calendars
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Calendar calendar = await query.FirstOrDefaultAsync(cancellationToken);

			if (calendar == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Calendar DELETE", id.ToString(), new Exception("No Scheduler.Calendar entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Calendar cloneOfExisting = (Database.Calendar)_context.Entry(calendar).GetDatabaseValues().ToObject();


			lock (calendarDeleteSyncRoot)
			{
			    try
			    {
			        calendar.deleted = true;
			        calendar.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        CalendarChangeHistory calendarChangeHistory = new CalendarChangeHistory();
			        calendarChangeHistory.calendarId = calendar.id;
			        calendarChangeHistory.versionNumber = calendar.versionNumber;
			        calendarChangeHistory.timeStamp = DateTime.UtcNow;
			        calendarChangeHistory.userId = securityUser.id;
			        calendarChangeHistory.tenantGuid = userTenantGuid;
			        calendarChangeHistory.data = JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar));
			        _context.CalendarChangeHistories.Add(calendarChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Calendar entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Calendar entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Calendar.CreateAnonymousWithFirstLevelSubObjects(calendar)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of Calendar records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Calendars/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? officeId = null,
			bool? isDefault = null,
			int? iconId = null,
			string color = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? versionNumber = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);

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

			IQueryable<Database.Calendar> query = (from c in _context.Calendars select c);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(c => c.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(c => c.description == description);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(c => c.officeId == officeId.Value);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(c => c.isDefault == isDefault.Value);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(c => c.iconId == iconId.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(c => c.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(c => c.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(c => c.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(c => c.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(c => c.deleted == false);
				}
			}
			else
			{
				query = query.Where(c => c.active == true);
				query = query.Where(c => c.deleted == false);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(c => c.versionNumber == versionNumber.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Calendar, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.icon.name.Contains(anyStringContains)
			       || x.icon.fontAwesomeCode.Contains(anyStringContains)
			       || x.office.name.Contains(anyStringContains)
			       || x.office.description.Contains(anyStringContains)
			       || x.office.addressLine1.Contains(anyStringContains)
			       || x.office.addressLine2.Contains(anyStringContains)
			       || x.office.city.Contains(anyStringContains)
			       || x.office.postalCode.Contains(anyStringContains)
			       || x.office.phone.Contains(anyStringContains)
			       || x.office.email.Contains(anyStringContains)
			       || x.office.notes.Contains(anyStringContains)
			       || x.office.externalId.Contains(anyStringContains)
			       || x.office.color.Contains(anyStringContains)
			       || x.office.attributes.Contains(anyStringContains)
			       || x.office.avatarFileName.Contains(anyStringContains)
			       || x.office.avatarMimeType.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Calendar.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Calendar/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


        /// <summary>
        /// 
        /// This makes a Calendar record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Calendar/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPut]
		public async Task<IActionResult> SetFavourite(int id, string description = null, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(cancellationToken);
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


			IQueryable<Database.Calendar> query = (from x in _context.Calendars
			                               where x.id == id
			                               select x);


			Database.Calendar calendar = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (calendar != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = calendar.name;
				}

				//
				// Add the user favourite Calendar
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "Calendar", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Calendar' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Calendar' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a Calendar record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/Calendar/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpDelete]
		public async Task<IActionResult> DeleteFavourite(int id, CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
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


			//
			// Delete the user favourite Calendar
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "Calendar", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'Calendar' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}


	}
}
