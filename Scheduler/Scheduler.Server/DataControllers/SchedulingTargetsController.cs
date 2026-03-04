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
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the SchedulingTarget entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SchedulingTarget entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SchedulingTargetsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object schedulingTargetPutSyncRoot = new object();
		static object schedulingTargetDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<SchedulingTargetsController> _logger;

		public SchedulingTargetsController(SchedulerContext context, ILogger<SchedulingTargetsController> logger) : base("Scheduler", "SchedulingTarget")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SchedulingTargets filtered by the parameters provided.
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
		[Route("api/SchedulingTargets")]
		public async Task<IActionResult> GetSchedulingTargets(
			string name = null,
			string description = null,
			int? officeId = null,
			int? clientId = null,
			int? schedulingTargetTypeId = null,
			int? timeZoneId = null,
			int? calendarId = null,
			string notes = null,
			string externalId = null,
			string color = null,
			string attributes = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

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

			IQueryable<Database.SchedulingTarget> query = (from st in _context.SchedulingTargets select st);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(st => st.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(st => st.description == description);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(st => st.officeId == officeId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(st => st.clientId == clientId.Value);
			}
			if (schedulingTargetTypeId.HasValue == true)
			{
				query = query.Where(st => st.schedulingTargetTypeId == schedulingTargetTypeId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(st => st.timeZoneId == timeZoneId.Value);
			}
			if (calendarId.HasValue == true)
			{
				query = query.Where(st => st.calendarId == calendarId.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(st => st.notes == notes);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(st => st.externalId == externalId);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(st => st.color == color);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(st => st.attributes == attributes);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(st => st.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(st => st.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(st => st.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(st => st.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(st => st.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(st => st.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(st => st.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(st => st.deleted == false);
				}
			}
			else
			{
				query = query.Where(st => st.active == true);
				query = query.Where(st => st.deleted == false);
			}

			query = query.OrderBy(st => st.name);


			//
			// Add the any string contains parameter to span all the string fields on the Scheduling Target, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || (includeRelations == true && x.calendar.name.Contains(anyStringContains))
			       || (includeRelations == true && x.calendar.description.Contains(anyStringContains))
			       || (includeRelations == true && x.calendar.color.Contains(anyStringContains))
			       || (includeRelations == true && x.client.name.Contains(anyStringContains))
			       || (includeRelations == true && x.client.description.Contains(anyStringContains))
			       || (includeRelations == true && x.client.addressLine1.Contains(anyStringContains))
			       || (includeRelations == true && x.client.addressLine2.Contains(anyStringContains))
			       || (includeRelations == true && x.client.city.Contains(anyStringContains))
			       || (includeRelations == true && x.client.postalCode.Contains(anyStringContains))
			       || (includeRelations == true && x.client.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.client.email.Contains(anyStringContains))
			       || (includeRelations == true && x.client.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.client.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.client.color.Contains(anyStringContains))
			       || (includeRelations == true && x.client.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.client.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.client.avatarMimeType.Contains(anyStringContains))
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
			       || (includeRelations == true && x.schedulingTargetType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTargetType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTargetType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.name.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.description.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.ianaTimeZone.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.calendar);
				query = query.Include(x => x.client);
				query = query.Include(x => x.office);
				query = query.Include(x => x.schedulingTargetType);
				query = query.Include(x => x.timeZone);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.SchedulingTarget> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SchedulingTarget schedulingTarget in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(schedulingTarget, databaseStoresDateWithTimeZone);
			}


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			if (diskBasedBinaryStorageMode == true)
			{
				var tasks = materialized.Select(async schedulingTarget =>
				{

					if (schedulingTarget.avatarData == null &&
					    schedulingTarget.avatarSize.HasValue == true &&
					    schedulingTarget.avatarSize.Value > 0)
					{
					    schedulingTarget.avatarData = await LoadDataFromDiskAsync(schedulingTarget.objectGuid, schedulingTarget.versionNumber, "data");
					}

				}).ToList();

				// Run tasks concurrently and await their completion
				await Task.WhenAll(tasks);
			}

			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.SchedulingTarget Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.SchedulingTarget Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SchedulingTargets filtered by the parameters provided.  Its query is similar to the GetSchedulingTargets method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTargets/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? officeId = null,
			int? clientId = null,
			int? schedulingTargetTypeId = null,
			int? timeZoneId = null,
			int? calendarId = null,
			string notes = null,
			string externalId = null,
			string color = null,
			string attributes = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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


			IQueryable<Database.SchedulingTarget> query = (from st in _context.SchedulingTargets select st);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(st => st.name == name);
			}
			if (description != null)
			{
				query = query.Where(st => st.description == description);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(st => st.officeId == officeId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(st => st.clientId == clientId.Value);
			}
			if (schedulingTargetTypeId.HasValue == true)
			{
				query = query.Where(st => st.schedulingTargetTypeId == schedulingTargetTypeId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(st => st.timeZoneId == timeZoneId.Value);
			}
			if (calendarId.HasValue == true)
			{
				query = query.Where(st => st.calendarId == calendarId.Value);
			}
			if (notes != null)
			{
				query = query.Where(st => st.notes == notes);
			}
			if (externalId != null)
			{
				query = query.Where(st => st.externalId == externalId);
			}
			if (color != null)
			{
				query = query.Where(st => st.color == color);
			}
			if (attributes != null)
			{
				query = query.Where(st => st.attributes == attributes);
			}
			if (avatarFileName != null)
			{
				query = query.Where(st => st.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(st => st.avatarSize == avatarSize.Value);
			}
			if (avatarMimeType != null)
			{
				query = query.Where(st => st.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(st => st.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(st => st.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(st => st.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(st => st.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(st => st.deleted == false);
				}
			}
			else
			{
				query = query.Where(st => st.active == true);
				query = query.Where(st => st.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Scheduling Target, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.calendar.name.Contains(anyStringContains)
			       || x.calendar.description.Contains(anyStringContains)
			       || x.calendar.color.Contains(anyStringContains)
			       || x.client.name.Contains(anyStringContains)
			       || x.client.description.Contains(anyStringContains)
			       || x.client.addressLine1.Contains(anyStringContains)
			       || x.client.addressLine2.Contains(anyStringContains)
			       || x.client.city.Contains(anyStringContains)
			       || x.client.postalCode.Contains(anyStringContains)
			       || x.client.phone.Contains(anyStringContains)
			       || x.client.email.Contains(anyStringContains)
			       || x.client.notes.Contains(anyStringContains)
			       || x.client.externalId.Contains(anyStringContains)
			       || x.client.color.Contains(anyStringContains)
			       || x.client.attributes.Contains(anyStringContains)
			       || x.client.avatarFileName.Contains(anyStringContains)
			       || x.client.avatarMimeType.Contains(anyStringContains)
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
			       || x.schedulingTargetType.name.Contains(anyStringContains)
			       || x.schedulingTargetType.description.Contains(anyStringContains)
			       || x.schedulingTargetType.color.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SchedulingTarget by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTarget/{id}")]
		public async Task<IActionResult> GetSchedulingTarget(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			
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
				IQueryable<Database.SchedulingTarget> query = (from st in _context.SchedulingTargets where
							(st.id == id) &&
							(userIsAdmin == true || st.deleted == false) &&
							(userIsWriter == true || st.active == true)
					select st);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.calendar);
					query = query.Include(x => x.client);
					query = query.Include(x => x.office);
					query = query.Include(x => x.schedulingTargetType);
					query = query.Include(x => x.timeZone);
					query = query.AsSplitQuery();
				}

				Database.SchedulingTarget materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

					if (diskBasedBinaryStorageMode == true &&
					    materialized.avatarData == null &&
					    materialized.avatarSize.HasValue == true &&
					    materialized.avatarSize.Value > 0)
					{
					    materialized.avatarData = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, "data", cancellationToken);
					}

					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.SchedulingTarget Entity was read with Admin privilege." : "Scheduler.SchedulingTarget Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SchedulingTarget", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.SchedulingTarget entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.SchedulingTarget.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.SchedulingTarget.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SchedulingTarget record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SchedulingTarget/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PutSchedulingTarget(int id, [FromBody]Database.SchedulingTarget.SchedulingTargetDTO schedulingTargetDTO, CancellationToken cancellationToken = default)
		{
			if (schedulingTargetDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != schedulingTargetDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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


			IQueryable<Database.SchedulingTarget> query = (from x in _context.SchedulingTargets
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SchedulingTarget existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTarget PUT", id.ToString(), new Exception("No Scheduler.SchedulingTarget entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (schedulingTargetDTO.objectGuid == Guid.Empty)
            {
                schedulingTargetDTO.objectGuid = existing.objectGuid;
            }
            else if (schedulingTargetDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SchedulingTarget record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SchedulingTarget cloneOfExisting = (Database.SchedulingTarget)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SchedulingTarget object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SchedulingTarget schedulingTarget = (Database.SchedulingTarget)_context.Entry(existing).GetDatabaseValues().ToObject();
			schedulingTarget.ApplyDTO(schedulingTargetDTO);
			//
			// The tenant guid for any SchedulingTarget being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the SchedulingTarget because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				schedulingTarget.tenantGuid = existing.tenantGuid;
			}

			lock (schedulingTargetPutSyncRoot)
			{
				//
				// Validate the version number for the schedulingTarget being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != schedulingTarget.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "SchedulingTarget save attempt was made but save request was with version " + schedulingTarget.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The SchedulingTarget you are trying to update has already changed.  Please try your save again after reloading the SchedulingTarget.");
				}
				else
				{
					// Same record.  Increase version.
					schedulingTarget.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (schedulingTarget.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.SchedulingTarget record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (schedulingTarget.name != null && schedulingTarget.name.Length > 100)
				{
					schedulingTarget.name = schedulingTarget.name.Substring(0, 100);
				}

				if (schedulingTarget.description != null && schedulingTarget.description.Length > 500)
				{
					schedulingTarget.description = schedulingTarget.description.Substring(0, 500);
				}

				if (schedulingTarget.externalId != null && schedulingTarget.externalId.Length > 100)
				{
					schedulingTarget.externalId = schedulingTarget.externalId.Substring(0, 100);
				}

				if (schedulingTarget.color != null && schedulingTarget.color.Length > 10)
				{
					schedulingTarget.color = schedulingTarget.color.Substring(0, 10);
				}

				if (schedulingTarget.avatarFileName != null && schedulingTarget.avatarFileName.Length > 250)
				{
					schedulingTarget.avatarFileName = schedulingTarget.avatarFileName.Substring(0, 250);
				}

				if (schedulingTarget.avatarMimeType != null && schedulingTarget.avatarMimeType.Length > 100)
				{
					schedulingTarget.avatarMimeType = schedulingTarget.avatarMimeType.Substring(0, 100);
				}


				//
				// Add default values for any missing data attribute fields.
				//
				if (schedulingTarget.avatarData != null && string.IsNullOrEmpty(schedulingTarget.avatarFileName))
				{
				    schedulingTarget.avatarFileName = schedulingTarget.objectGuid.ToString() + ".data";
				}

				if (schedulingTarget.avatarData != null && (schedulingTarget.avatarSize.HasValue == false || schedulingTarget.avatarSize != schedulingTarget.avatarData.Length))
				{
				    schedulingTarget.avatarSize = schedulingTarget.avatarData.Length;
				}

				if (schedulingTarget.avatarData != null && string.IsNullOrEmpty(schedulingTarget.avatarMimeType))
				{
				    schedulingTarget.avatarMimeType = "application/octet-stream";
				}

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				try
				{
					byte[] dataReferenceBeforeClearing = schedulingTarget.avatarData;

					if (diskBasedBinaryStorageMode == true &&
					    schedulingTarget.avatarFileName != null &&
					    schedulingTarget.avatarData != null &&
					    schedulingTarget.avatarSize.HasValue == true &&
					    schedulingTarget.avatarSize.Value > 0)
					{
					    //
					    // write the bytes to disk
					    //
					    WriteDataToDisk(schedulingTarget.objectGuid, schedulingTarget.versionNumber, schedulingTarget.avatarData, "data");

					    //
					    // Clear the data from the object before we put it into the db
					    //
					    schedulingTarget.avatarData = null;

					}

				    EntityEntry<Database.SchedulingTarget> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(schedulingTarget);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SchedulingTargetChangeHistory schedulingTargetChangeHistory = new SchedulingTargetChangeHistory();
				        schedulingTargetChangeHistory.schedulingTargetId = schedulingTarget.id;
				        schedulingTargetChangeHistory.versionNumber = schedulingTarget.versionNumber;
				        schedulingTargetChangeHistory.timeStamp = DateTime.UtcNow;
				        schedulingTargetChangeHistory.userId = securityUser.id;
				        schedulingTargetChangeHistory.tenantGuid = userTenantGuid;
				        schedulingTargetChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget));
				        _context.SchedulingTargetChangeHistories.Add(schedulingTargetChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    schedulingTarget.avatarData = dataReferenceBeforeClearing;
					}

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SchedulingTarget entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget)),
						null);

				return Ok(Database.SchedulingTarget.CreateAnonymous(schedulingTarget));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SchedulingTarget entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new SchedulingTarget record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTarget", Name = "SchedulingTarget")]
		[RequestSizeLimit(5000000)]
		public async Task<IActionResult> PostSchedulingTarget([FromBody]Database.SchedulingTarget.SchedulingTargetDTO schedulingTargetDTO, CancellationToken cancellationToken = default)
		{
			if (schedulingTargetDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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
			// Create a new SchedulingTarget object using the data from the DTO
			//
			Database.SchedulingTarget schedulingTarget = Database.SchedulingTarget.FromDTO(schedulingTargetDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				schedulingTarget.tenantGuid = userTenantGuid;

				if (schedulingTarget.name != null && schedulingTarget.name.Length > 100)
				{
					schedulingTarget.name = schedulingTarget.name.Substring(0, 100);
				}

				if (schedulingTarget.description != null && schedulingTarget.description.Length > 500)
				{
					schedulingTarget.description = schedulingTarget.description.Substring(0, 500);
				}

				if (schedulingTarget.externalId != null && schedulingTarget.externalId.Length > 100)
				{
					schedulingTarget.externalId = schedulingTarget.externalId.Substring(0, 100);
				}

				if (schedulingTarget.color != null && schedulingTarget.color.Length > 10)
				{
					schedulingTarget.color = schedulingTarget.color.Substring(0, 10);
				}

				if (schedulingTarget.avatarFileName != null && schedulingTarget.avatarFileName.Length > 250)
				{
					schedulingTarget.avatarFileName = schedulingTarget.avatarFileName.Substring(0, 250);
				}

				if (schedulingTarget.avatarMimeType != null && schedulingTarget.avatarMimeType.Length > 100)
				{
					schedulingTarget.avatarMimeType = schedulingTarget.avatarMimeType.Substring(0, 100);
				}

				schedulingTarget.objectGuid = Guid.NewGuid();

				//
				// Add default values for any missing data attribute fields.
				//
				if (schedulingTarget.avatarData != null && string.IsNullOrEmpty(schedulingTarget.avatarFileName))
				{
				    schedulingTarget.avatarFileName = schedulingTarget.objectGuid.ToString() + ".data";
				}

				if (schedulingTarget.avatarData != null && (schedulingTarget.avatarSize.HasValue == false || schedulingTarget.avatarSize != schedulingTarget.avatarData.Length))
				{
				    schedulingTarget.avatarSize = schedulingTarget.avatarData.Length;
				}

				if (schedulingTarget.avatarData != null && string.IsNullOrEmpty(schedulingTarget.avatarMimeType))
				{
				    schedulingTarget.avatarMimeType = "application/octet-stream";
				}

				schedulingTarget.versionNumber = 1;

				bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

				byte[] dataReferenceBeforeClearing = schedulingTarget.avatarData;

				if (diskBasedBinaryStorageMode == true &&
				    schedulingTarget.avatarData != null &&
				    schedulingTarget.avatarFileName != null &&
				    schedulingTarget.avatarSize.HasValue == true &&
				    schedulingTarget.avatarSize.Value > 0)
				{
				    //
				    // write the bytes to disk
				    //
				    await WriteDataToDiskAsync(schedulingTarget.objectGuid, schedulingTarget.versionNumber, schedulingTarget.avatarData, "data", cancellationToken);

				    //
				    // Clear the data from the object before we put it into the db
				    //
				    schedulingTarget.avatarData = null;

				}

				_context.SchedulingTargets.Add(schedulingTarget);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the schedulingTarget object so that no further changes will be written to the database
				    //
				    _context.Entry(schedulingTarget).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					schedulingTarget.avatarData = null;
					schedulingTarget.Households = null;
					schedulingTarget.RateSheets = null;
					schedulingTarget.ScheduledEvents = null;
					schedulingTarget.SchedulingTargetAddresses = null;
					schedulingTarget.SchedulingTargetChangeHistories = null;
					schedulingTarget.SchedulingTargetContacts = null;
					schedulingTarget.SchedulingTargetQualificationRequirements = null;
					schedulingTarget.calendar = null;
					schedulingTarget.client = null;
					schedulingTarget.office = null;
					schedulingTarget.schedulingTargetType = null;
					schedulingTarget.timeZone = null;


				    SchedulingTargetChangeHistory schedulingTargetChangeHistory = new SchedulingTargetChangeHistory();
				    schedulingTargetChangeHistory.schedulingTargetId = schedulingTarget.id;
				    schedulingTargetChangeHistory.versionNumber = schedulingTarget.versionNumber;
				    schedulingTargetChangeHistory.timeStamp = DateTime.UtcNow;
				    schedulingTargetChangeHistory.userId = securityUser.id;
				    schedulingTargetChangeHistory.tenantGuid = userTenantGuid;
				    schedulingTargetChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget));
				    _context.SchedulingTargetChangeHistories.Add(schedulingTargetChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.SchedulingTarget entity successfully created.",
						true,
						schedulingTarget. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget)),
						null);



					if (diskBasedBinaryStorageMode == true)
					{
					    //
					    // Put the data bytes back into the object that will be returned.
					    //
					    schedulingTarget.avatarData = dataReferenceBeforeClearing;
					}

				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.SchedulingTarget entity creation failed.", false, schedulingTarget.id.ToString(), "", JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SchedulingTarget", schedulingTarget.id, schedulingTarget.name));

			return CreatedAtRoute("SchedulingTarget", new { id = schedulingTarget.id }, Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget));
		}



        /// <summary>
        /// 
        /// This rolls a SchedulingTarget entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTarget/Rollback/{id}")]
		[Route("api/SchedulingTarget/Rollback")]
		public async Task<IActionResult> RollbackToSchedulingTargetVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.SchedulingTarget> query = (from x in _context.SchedulingTargets
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


			//
			// Make sure nobody else is editing this SchedulingTarget concurrently
			//
			lock (schedulingTargetPutSyncRoot)
			{
				
				Database.SchedulingTarget schedulingTarget = query.FirstOrDefault();
				
				if (schedulingTarget == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTarget rollback", id.ToString(), new Exception("No Scheduler.SchedulingTarget entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the SchedulingTarget current state so we can log it.
				//
				Database.SchedulingTarget cloneOfExisting = (Database.SchedulingTarget)_context.Entry(schedulingTarget).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.avatarData = null;
				cloneOfExisting.Households = null;
				cloneOfExisting.RateSheets = null;
				cloneOfExisting.ScheduledEvents = null;
				cloneOfExisting.SchedulingTargetAddresses = null;
				cloneOfExisting.SchedulingTargetChangeHistories = null;
				cloneOfExisting.SchedulingTargetContacts = null;
				cloneOfExisting.SchedulingTargetQualificationRequirements = null;
				cloneOfExisting.calendar = null;
				cloneOfExisting.client = null;
				cloneOfExisting.office = null;
				cloneOfExisting.schedulingTargetType = null;
				cloneOfExisting.timeZone = null;

				if (versionNumber >= schedulingTarget.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.SchedulingTarget rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.SchedulingTarget rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				SchedulingTargetChangeHistory schedulingTargetChangeHistory = (from x in _context.SchedulingTargetChangeHistories
				                                               where
				                                               x.schedulingTargetId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (schedulingTargetChangeHistory != null)
				{
				    Database.SchedulingTarget oldSchedulingTarget = JsonSerializer.Deserialize<Database.SchedulingTarget>(schedulingTargetChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    schedulingTarget.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    schedulingTarget.name = oldSchedulingTarget.name;
				    schedulingTarget.description = oldSchedulingTarget.description;
				    schedulingTarget.officeId = oldSchedulingTarget.officeId;
				    schedulingTarget.clientId = oldSchedulingTarget.clientId;
				    schedulingTarget.schedulingTargetTypeId = oldSchedulingTarget.schedulingTargetTypeId;
				    schedulingTarget.timeZoneId = oldSchedulingTarget.timeZoneId;
				    schedulingTarget.calendarId = oldSchedulingTarget.calendarId;
				    schedulingTarget.notes = oldSchedulingTarget.notes;
				    schedulingTarget.externalId = oldSchedulingTarget.externalId;
				    schedulingTarget.color = oldSchedulingTarget.color;
				    schedulingTarget.attributes = oldSchedulingTarget.attributes;
				    schedulingTarget.avatarFileName = oldSchedulingTarget.avatarFileName;
				    schedulingTarget.avatarSize = oldSchedulingTarget.avatarSize;
				    schedulingTarget.avatarData = oldSchedulingTarget.avatarData;
				    schedulingTarget.avatarMimeType = oldSchedulingTarget.avatarMimeType;
				    schedulingTarget.objectGuid = oldSchedulingTarget.objectGuid;
				    schedulingTarget.active = oldSchedulingTarget.active;
				    schedulingTarget.deleted = oldSchedulingTarget.deleted;
				    //
				    // If disk based binary mode is on, then we need to copy the old data file over as well.
				    //
				    if (diskBasedBinaryStorageMode == true)
				    {
				    	Byte[] binaryData = LoadDataFromDisk(oldSchedulingTarget.objectGuid, oldSchedulingTarget.versionNumber, "data");

				    	//
				    	// Write out the data as the new version
				    	//
				    	WriteDataToDisk(schedulingTarget.objectGuid, schedulingTarget.versionNumber, binaryData, "data");
				    }

				    string serializedSchedulingTarget = JsonSerializer.Serialize(schedulingTarget);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        SchedulingTargetChangeHistory newSchedulingTargetChangeHistory = new SchedulingTargetChangeHistory();
				        newSchedulingTargetChangeHistory.schedulingTargetId = schedulingTarget.id;
				        newSchedulingTargetChangeHistory.versionNumber = schedulingTarget.versionNumber;
				        newSchedulingTargetChangeHistory.timeStamp = DateTime.UtcNow;
				        newSchedulingTargetChangeHistory.userId = securityUser.id;
				        newSchedulingTargetChangeHistory.tenantGuid = userTenantGuid;
				        newSchedulingTargetChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget));
				        _context.SchedulingTargetChangeHistories.Add(newSchedulingTargetChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.SchedulingTarget rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget)),
						null);


				    return Ok(Database.SchedulingTarget.CreateAnonymous(schedulingTarget));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.SchedulingTarget rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.SchedulingTarget rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a SchedulingTarget.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTarget</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTarget/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetSchedulingTargetChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.SchedulingTarget schedulingTarget = await _context.SchedulingTargets.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTarget == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTarget.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SchedulingTarget> versionInfo = await schedulingTarget.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a SchedulingTarget.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTarget</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTarget/{id}/AuditHistory")]
		public async Task<IActionResult> GetSchedulingTargetAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.SchedulingTarget schedulingTarget = await _context.SchedulingTargets.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTarget == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTarget.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.SchedulingTarget>> versions = await schedulingTarget.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a SchedulingTarget.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTarget</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The SchedulingTarget object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTarget/{id}/Version/{version}")]
		public async Task<IActionResult> GetSchedulingTargetVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.SchedulingTarget schedulingTarget = await _context.SchedulingTargets.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTarget == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTarget.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SchedulingTarget> versionInfo = await schedulingTarget.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a SchedulingTarget at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the SchedulingTarget</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The SchedulingTarget object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTarget/{id}/StateAtTime")]
		public async Task<IActionResult> GetSchedulingTargetStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			Database.SchedulingTarget schedulingTarget = await _context.SchedulingTargets.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (schedulingTarget == null)
			{
				return NotFound();
			}

			try
			{
				schedulingTarget.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.SchedulingTarget> versionInfo = await schedulingTarget.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a SchedulingTarget record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SchedulingTarget/{id}")]
		[Route("api/SchedulingTarget")]
		public async Task<IActionResult> DeleteSchedulingTarget(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.SchedulingTarget> query = (from x in _context.SchedulingTargets
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.SchedulingTarget schedulingTarget = await query.FirstOrDefaultAsync(cancellationToken);

			if (schedulingTarget == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.SchedulingTarget DELETE", id.ToString(), new Exception("No Scheduler.SchedulingTarget entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SchedulingTarget cloneOfExisting = (Database.SchedulingTarget)_context.Entry(schedulingTarget).GetDatabaseValues().ToObject();


			bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();

			lock (schedulingTargetDeleteSyncRoot)
			{
			    try
			    {
			        schedulingTarget.deleted = true;
			        schedulingTarget.versionNumber++;

			        _context.SaveChanges();

			        //
			        // If in disk based storage mode, create a copy of the disk data file for the new version.
			        //
			        if (diskBasedBinaryStorageMode == true)
			        {
			        	Byte[] binaryData = LoadDataFromDisk(schedulingTarget.objectGuid, schedulingTarget.versionNumber -1, "data");

			        	//
			        	// Write out the same data
			        	//
			        	WriteDataToDisk(schedulingTarget.objectGuid, schedulingTarget.versionNumber, binaryData, "data");
			        }

			        //
			        // Now add the change history
			        //
			        SchedulingTargetChangeHistory schedulingTargetChangeHistory = new SchedulingTargetChangeHistory();
			        schedulingTargetChangeHistory.schedulingTargetId = schedulingTarget.id;
			        schedulingTargetChangeHistory.versionNumber = schedulingTarget.versionNumber;
			        schedulingTargetChangeHistory.timeStamp = DateTime.UtcNow;
			        schedulingTargetChangeHistory.userId = securityUser.id;
			        schedulingTargetChangeHistory.tenantGuid = userTenantGuid;
			        schedulingTargetChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget));
			        _context.SchedulingTargetChangeHistories.Add(schedulingTargetChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.SchedulingTarget entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.SchedulingTarget entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of SchedulingTarget records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SchedulingTargets/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? officeId = null,
			int? clientId = null,
			int? schedulingTargetTypeId = null,
			int? timeZoneId = null,
			int? calendarId = null,
			string notes = null,
			string externalId = null,
			string color = null,
			string attributes = null,
			string avatarFileName = null,
			long? avatarSize = null,
			string avatarMimeType = null,
			int? versionNumber = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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

			IQueryable<Database.SchedulingTarget> query = (from st in _context.SchedulingTargets select st);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(st => st.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(st => st.description == description);
			}
			if (officeId.HasValue == true)
			{
				query = query.Where(st => st.officeId == officeId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(st => st.clientId == clientId.Value);
			}
			if (schedulingTargetTypeId.HasValue == true)
			{
				query = query.Where(st => st.schedulingTargetTypeId == schedulingTargetTypeId.Value);
			}
			if (timeZoneId.HasValue == true)
			{
				query = query.Where(st => st.timeZoneId == timeZoneId.Value);
			}
			if (calendarId.HasValue == true)
			{
				query = query.Where(st => st.calendarId == calendarId.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(st => st.notes == notes);
			}
			if (string.IsNullOrEmpty(externalId) == false)
			{
				query = query.Where(st => st.externalId == externalId);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(st => st.color == color);
			}
			if (string.IsNullOrEmpty(attributes) == false)
			{
				query = query.Where(st => st.attributes == attributes);
			}
			if (string.IsNullOrEmpty(avatarFileName) == false)
			{
				query = query.Where(st => st.avatarFileName == avatarFileName);
			}
			if (avatarSize.HasValue == true)
			{
				query = query.Where(st => st.avatarSize == avatarSize.Value);
			}
			if (string.IsNullOrEmpty(avatarMimeType) == false)
			{
				query = query.Where(st => st.avatarMimeType == avatarMimeType);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(st => st.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(st => st.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(st => st.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(st => st.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(st => st.deleted == false);
				}
			}
			else
			{
				query = query.Where(st => st.active == true);
				query = query.Where(st => st.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Scheduling Target, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.externalId.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			       || x.attributes.Contains(anyStringContains)
			       || x.avatarFileName.Contains(anyStringContains)
			       || x.avatarMimeType.Contains(anyStringContains)
			       || x.calendar.name.Contains(anyStringContains)
			       || x.calendar.description.Contains(anyStringContains)
			       || x.calendar.color.Contains(anyStringContains)
			       || x.client.name.Contains(anyStringContains)
			       || x.client.description.Contains(anyStringContains)
			       || x.client.addressLine1.Contains(anyStringContains)
			       || x.client.addressLine2.Contains(anyStringContains)
			       || x.client.city.Contains(anyStringContains)
			       || x.client.postalCode.Contains(anyStringContains)
			       || x.client.phone.Contains(anyStringContains)
			       || x.client.email.Contains(anyStringContains)
			       || x.client.notes.Contains(anyStringContains)
			       || x.client.externalId.Contains(anyStringContains)
			       || x.client.color.Contains(anyStringContains)
			       || x.client.attributes.Contains(anyStringContains)
			       || x.client.avatarFileName.Contains(anyStringContains)
			       || x.client.avatarMimeType.Contains(anyStringContains)
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
			       || x.schedulingTargetType.name.Contains(anyStringContains)
			       || x.schedulingTargetType.description.Contains(anyStringContains)
			       || x.schedulingTargetType.color.Contains(anyStringContains)
			       || x.timeZone.name.Contains(anyStringContains)
			       || x.timeZone.description.Contains(anyStringContains)
			       || x.timeZone.ianaTimeZone.Contains(anyStringContains)
			       || x.timeZone.abbreviation.Contains(anyStringContains)
			       || x.timeZone.abbreviationDaylightSavings.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SchedulingTarget.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SchedulingTarget/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


        /// <summary>
        /// 
        /// This makes a SchedulingTarget record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/SchedulingTarget/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPut]
		public async Task<IActionResult> SetFavourite(int id, string description = null, CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(cancellationToken);

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


			IQueryable<Database.SchedulingTarget> query = (from x in _context.SchedulingTargets
			                               where x.id == id
			                               select x);


			Database.SchedulingTarget schedulingTarget = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

			if (schedulingTarget != null)
			{
				if (string.IsNullOrEmpty(description) == true)
				{
					description = schedulingTarget.name;
				}

				//
				// Add the user favourite SchedulingTarget
				//
				await SecurityLogic.AddToUserFavouritesAsync(securityUser, "SchedulingTarget", id, description, cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'SchedulingTarget' was added for record with id of " + id + " for user " + securityUser.accountName, true);

				//
				// Return the complete list of user favourites after the addition
				//
				return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
			}
			else
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'SchedulingTarget' add request was made with an invalid id value of " + id, false);
				return BadRequest();
			}
		}


        /// <summary>
        /// 
        /// This removes a SchedulingTarget record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>
		[Route("api/SchedulingTarget/Favourite/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpDelete]
		public async Task<IActionResult> DeleteFavourite(int id, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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


			//
			// Delete the user favourite SchedulingTarget
			//
			await SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, "SchedulingTarget", id, cancellationToken);

			await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, "Favourite 'SchedulingTarget' was removed for record with id of " + id + " for user " + securityUser.accountName, true);

			//
			// Return the complete list of user favourites after the deletion
			//
			return Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));
		}




        [Route("api/SchedulingTarget/Data/{id:int}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> UploadData(int id, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			MediaTypeHeaderValue mediaTypeHeader; 

            if (!HttpContext.Request.HasFormContentType ||
				!MediaTypeHeaderValue.TryParse(HttpContext.Request.ContentType, out mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }


            Database.SchedulingTarget schedulingTarget = await (from x in _context.SchedulingTargets where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();
            if (schedulingTarget == null)
            {
                return NotFound();
            }

            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();


            // This will be used to signal whether we are saving data or clearing it.
            bool foundFileData = false;


            //
            // This will get the first file from the request and save it
            //
			try
			{
                MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, HttpContext.Request.Body);
                MultipartSection section = await reader.ReadNextSectionAsync();

                while (section != null)
				{
					ContentDispositionHeaderValue contentDisposition;

					bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);


					if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
						!string.IsNullOrEmpty(contentDisposition.FileName.Value))
					{

						foundFileData = true;
						string fileName = contentDisposition.FileName.ToString().Trim('"');

						// default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.
						MediaTypeHeaderValue mediaType;
						bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);

						string mimeType = "application/octet-stream";
						if (hasMediaTypeHeader && mediaTypeHeader.MediaType != null )
						{
							mimeType = mediaTypeHeader.MediaType.ToString();
						}

						lock (schedulingTargetPutSyncRoot)
						{
							try
							{
								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
								{
									schedulingTarget.avatarFileName = fileName.Trim();
									schedulingTarget.avatarMimeType = mimeType;
									schedulingTarget.avatarSize = section.Body.Length;

									schedulingTarget.versionNumber++;

									if (diskBasedBinaryStorageMode == true &&
										 schedulingTarget.avatarFileName != null &&
										 schedulingTarget.avatarSize > 0)
									{
										//
										// write the bytes to disk
										//
										WriteDataToDisk(schedulingTarget.objectGuid, schedulingTarget.versionNumber, section.Body, "data");
										//
										// Clear the data from the object before we put it into the db
										//
										schedulingTarget.avatarData = null;
									}
									else
									{
										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))
										{
											section.Body.CopyTo(memoryStream);
											schedulingTarget.avatarData = memoryStream.ToArray();
										}
									}
									//
									// Now add the change history
									//
									SchedulingTargetChangeHistory schedulingTargetChangeHistory = new SchedulingTargetChangeHistory();
									schedulingTargetChangeHistory.schedulingTargetId = schedulingTarget.id;
									schedulingTargetChangeHistory.versionNumber = schedulingTarget.versionNumber;
									schedulingTargetChangeHistory.timeStamp = DateTime.UtcNow;
									schedulingTargetChangeHistory.userId = securityUser.id;
									schedulingTargetChangeHistory.tenantGuid = schedulingTarget.tenantGuid;
									schedulingTargetChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget));
									_context.SchedulingTargetChangeHistories.Add(schedulingTargetChangeHistory);

									_context.SaveChanges();

									transaction.Commit();

									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "SchedulingTarget Data Uploaded with filename of " + fileName + " and with size of " + section.Body.Length, id.ToString());
								}
							}
							catch (Exception ex)
							{
								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "SchedulingTarget Data Upload Failed.", false, id.ToString(), "", "", ex);

								return Problem(ex.Message);
							}
						}


						//
						// Stop looking for more files.
						//
						break;
					}

					section = await reader.ReadNextSectionAsync();
				}
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Caught error in UploadData handler", id.ToString(), ex);

                return Problem(ex.Message);
            }

            //
            // Treat the situation where we have a valid ID but no file content as a request to clear the data
            //
            if (foundFileData == false)
            {
                lock (schedulingTargetPutSyncRoot)
                {
                    try
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            if (diskBasedBinaryStorageMode == true)
                            {
								DeleteDataFromDisk(schedulingTarget.objectGuid, schedulingTarget.versionNumber, "data");
                            }

                            schedulingTarget.avatarFileName = null;
                            schedulingTarget.avatarMimeType = null;
                            schedulingTarget.avatarSize = 0;
                            schedulingTarget.avatarData = null;
                            schedulingTarget.versionNumber++;


                            //
                            // Now add the change history
                            //
                            SchedulingTargetChangeHistory schedulingTargetChangeHistory = new SchedulingTargetChangeHistory();
                            schedulingTargetChangeHistory.schedulingTargetId = schedulingTarget.id;
                            schedulingTargetChangeHistory.versionNumber = schedulingTarget.versionNumber;
                            schedulingTargetChangeHistory.timeStamp = DateTime.UtcNow;
                            schedulingTargetChangeHistory.userId = securityUser.id;
                                    schedulingTargetChangeHistory.tenantGuid = schedulingTarget.tenantGuid;
                                    schedulingTargetChangeHistory.data = JsonSerializer.Serialize(Database.SchedulingTarget.CreateAnonymousWithFirstLevelSubObjects(schedulingTarget));
                            _context.SchedulingTargetChangeHistories.Add(schedulingTargetChangeHistory);

                            _context.SaveChanges();

                            transaction.Commit();

                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "SchedulingTarget data cleared.", id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "SchedulingTarget data clear failed.", false, id.ToString(), "", "", ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/SchedulingTarget/Data/{id:int}")]
        public async Task<IActionResult> DownloadDataAsync(int id, CancellationToken cancellationToken = default)
        {

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			using (SchedulerContext context = new SchedulerContext())
            {
                //
                // Return the data to the user as though it was a file.
                //
                Database.SchedulingTarget schedulingTarget = await (from d in context.SchedulingTargets
                                                where d.id == id &&
                                                d.active == true &&
                                                d.deleted == false
                                                select d).FirstOrDefaultAsync();

                if (schedulingTarget != null && schedulingTarget.avatarData != null)
                {
                   return File(schedulingTarget.avatarData.ToArray<byte>(), schedulingTarget.avatarMimeType, schedulingTarget.avatarFileName != null ? schedulingTarget.avatarFileName.Trim() : "SchedulingTarget_" + schedulingTarget.id.ToString(), true);
                }
                else
                {
                    return BadRequest();
                }
            }
        }
	}
}
