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
    /// This auto generated class provides the basic CRUD operations for the ActivityEvent entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ActivityEvent entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ActivityEventsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<ActivityEventsController> _logger;

		public ActivityEventsController(BMCContext context, ILogger<ActivityEventsController> logger) : base("BMC", "ActivityEvent")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ActivityEvents filtered by the parameters provided.
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
		[Route("api/ActivityEvents")]
		public async Task<IActionResult> GetActivityEvents(
			int? activityEventTypeId = null,
			string title = null,
			string description = null,
			string relatedEntityType = null,
			long? relatedEntityId = null,
			DateTime? eventDate = null,
			bool? isPublic = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (eventDate.HasValue == true && eventDate.Value.Kind != DateTimeKind.Utc)
			{
				eventDate = eventDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ActivityEvent> query = (from ae in _context.ActivityEvents select ae);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (activityEventTypeId.HasValue == true)
			{
				query = query.Where(ae => ae.activityEventTypeId == activityEventTypeId.Value);
			}
			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(ae => ae.title == title);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ae => ae.description == description);
			}
			if (string.IsNullOrEmpty(relatedEntityType) == false)
			{
				query = query.Where(ae => ae.relatedEntityType == relatedEntityType);
			}
			if (relatedEntityId.HasValue == true)
			{
				query = query.Where(ae => ae.relatedEntityId == relatedEntityId.Value);
			}
			if (eventDate.HasValue == true)
			{
				query = query.Where(ae => ae.eventDate == eventDate.Value);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(ae => ae.isPublic == isPublic.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ae => ae.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ae => ae.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ae => ae.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ae => ae.deleted == false);
				}
			}
			else
			{
				query = query.Where(ae => ae.active == true);
				query = query.Where(ae => ae.deleted == false);
			}

			query = query.OrderBy(ae => ae.title).ThenBy(ae => ae.relatedEntityType);


			//
			// Add the any string contains parameter to span all the string fields on the Activity Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.relatedEntityType.Contains(anyStringContains)
			       || (includeRelations == true && x.activityEventType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.activityEventType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.activityEventType.iconCssClass.Contains(anyStringContains))
			       || (includeRelations == true && x.activityEventType.accentColor.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.activityEventType);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ActivityEvent> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ActivityEvent activityEvent in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(activityEvent, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ActivityEvent Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ActivityEvent Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ActivityEvents filtered by the parameters provided.  Its query is similar to the GetActivityEvents method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ActivityEvents/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? activityEventTypeId = null,
			string title = null,
			string description = null,
			string relatedEntityType = null,
			long? relatedEntityId = null,
			DateTime? eventDate = null,
			bool? isPublic = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			// Fix any non-UTC date parameters that come in.
			//
			if (eventDate.HasValue == true && eventDate.Value.Kind != DateTimeKind.Utc)
			{
				eventDate = eventDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ActivityEvent> query = (from ae in _context.ActivityEvents select ae);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (activityEventTypeId.HasValue == true)
			{
				query = query.Where(ae => ae.activityEventTypeId == activityEventTypeId.Value);
			}
			if (title != null)
			{
				query = query.Where(ae => ae.title == title);
			}
			if (description != null)
			{
				query = query.Where(ae => ae.description == description);
			}
			if (relatedEntityType != null)
			{
				query = query.Where(ae => ae.relatedEntityType == relatedEntityType);
			}
			if (relatedEntityId.HasValue == true)
			{
				query = query.Where(ae => ae.relatedEntityId == relatedEntityId.Value);
			}
			if (eventDate.HasValue == true)
			{
				query = query.Where(ae => ae.eventDate == eventDate.Value);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(ae => ae.isPublic == isPublic.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ae => ae.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ae => ae.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ae => ae.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ae => ae.deleted == false);
				}
			}
			else
			{
				query = query.Where(ae => ae.active == true);
				query = query.Where(ae => ae.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Activity Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.relatedEntityType.Contains(anyStringContains)
			       || x.activityEventType.name.Contains(anyStringContains)
			       || x.activityEventType.description.Contains(anyStringContains)
			       || x.activityEventType.iconCssClass.Contains(anyStringContains)
			       || x.activityEventType.accentColor.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ActivityEvent by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ActivityEvent/{id}")]
		public async Task<IActionResult> GetActivityEvent(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.ActivityEvent> query = (from ae in _context.ActivityEvents where
							(ae.id == id) &&
							(userIsAdmin == true || ae.deleted == false) &&
							(userIsWriter == true || ae.active == true)
					select ae);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.activityEventType);
					query = query.AsSplitQuery();
				}

				Database.ActivityEvent materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ActivityEvent Entity was read with Admin privilege." : "BMC.ActivityEvent Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ActivityEvent", materialized.id, materialized.title));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ActivityEvent entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ActivityEvent.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ActivityEvent.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ActivityEvent record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ActivityEvent/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutActivityEvent(int id, [FromBody]Database.ActivityEvent.ActivityEventDTO activityEventDTO, CancellationToken cancellationToken = default)
		{
			if (activityEventDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != activityEventDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.ActivityEvent> query = (from x in _context.ActivityEvents
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ActivityEvent existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ActivityEvent PUT", id.ToString(), new Exception("No BMC.ActivityEvent entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (activityEventDTO.objectGuid == Guid.Empty)
            {
                activityEventDTO.objectGuid = existing.objectGuid;
            }
            else if (activityEventDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ActivityEvent record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ActivityEvent cloneOfExisting = (Database.ActivityEvent)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ActivityEvent object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ActivityEvent activityEvent = (Database.ActivityEvent)_context.Entry(existing).GetDatabaseValues().ToObject();
			activityEvent.ApplyDTO(activityEventDTO);
			//
			// The tenant guid for any ActivityEvent being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ActivityEvent because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				activityEvent.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (activityEvent.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ActivityEvent record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (activityEvent.title != null && activityEvent.title.Length > 250)
			{
				activityEvent.title = activityEvent.title.Substring(0, 250);
			}

			if (activityEvent.relatedEntityType != null && activityEvent.relatedEntityType.Length > 100)
			{
				activityEvent.relatedEntityType = activityEvent.relatedEntityType.Substring(0, 100);
			}

			if (activityEvent.eventDate.Kind != DateTimeKind.Utc)
			{
				activityEvent.eventDate = activityEvent.eventDate.ToUniversalTime();
			}

			EntityEntry<Database.ActivityEvent> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(activityEvent);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ActivityEvent entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ActivityEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ActivityEvent.CreateAnonymousWithFirstLevelSubObjects(activityEvent)),
					null);


				return Ok(Database.ActivityEvent.CreateAnonymous(activityEvent));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ActivityEvent entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ActivityEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ActivityEvent.CreateAnonymousWithFirstLevelSubObjects(activityEvent)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ActivityEvent record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ActivityEvent", Name = "ActivityEvent")]
		public async Task<IActionResult> PostActivityEvent([FromBody]Database.ActivityEvent.ActivityEventDTO activityEventDTO, CancellationToken cancellationToken = default)
		{
			if (activityEventDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new ActivityEvent object using the data from the DTO
			//
			Database.ActivityEvent activityEvent = Database.ActivityEvent.FromDTO(activityEventDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				activityEvent.tenantGuid = userTenantGuid;

				if (activityEvent.title != null && activityEvent.title.Length > 250)
				{
					activityEvent.title = activityEvent.title.Substring(0, 250);
				}

				if (activityEvent.relatedEntityType != null && activityEvent.relatedEntityType.Length > 100)
				{
					activityEvent.relatedEntityType = activityEvent.relatedEntityType.Substring(0, 100);
				}

				if (activityEvent.eventDate.Kind != DateTimeKind.Utc)
				{
					activityEvent.eventDate = activityEvent.eventDate.ToUniversalTime();
				}

				activityEvent.objectGuid = Guid.NewGuid();
				_context.ActivityEvents.Add(activityEvent);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ActivityEvent entity successfully created.",
					true,
					activityEvent.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ActivityEvent.CreateAnonymousWithFirstLevelSubObjects(activityEvent)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ActivityEvent entity creation failed.", false, activityEvent.id.ToString(), "", JsonSerializer.Serialize(activityEvent), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ActivityEvent", activityEvent.id, activityEvent.title));

			return CreatedAtRoute("ActivityEvent", new { id = activityEvent.id }, Database.ActivityEvent.CreateAnonymousWithFirstLevelSubObjects(activityEvent));
		}



        /// <summary>
        /// 
        /// This deletes a ActivityEvent record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ActivityEvent/{id}")]
		[Route("api/ActivityEvent")]
		public async Task<IActionResult> DeleteActivityEvent(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.ActivityEvent> query = (from x in _context.ActivityEvents
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ActivityEvent activityEvent = await query.FirstOrDefaultAsync(cancellationToken);

			if (activityEvent == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ActivityEvent DELETE", id.ToString(), new Exception("No BMC.ActivityEvent entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ActivityEvent cloneOfExisting = (Database.ActivityEvent)_context.Entry(activityEvent).GetDatabaseValues().ToObject();


			try
			{
				activityEvent.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ActivityEvent entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ActivityEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ActivityEvent.CreateAnonymousWithFirstLevelSubObjects(activityEvent)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ActivityEvent entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ActivityEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ActivityEvent.CreateAnonymousWithFirstLevelSubObjects(activityEvent)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ActivityEvent records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ActivityEvents/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? activityEventTypeId = null,
			string title = null,
			string description = null,
			string relatedEntityType = null,
			long? relatedEntityId = null,
			DateTime? eventDate = null,
			bool? isPublic = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (eventDate.HasValue == true && eventDate.Value.Kind != DateTimeKind.Utc)
			{
				eventDate = eventDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ActivityEvent> query = (from ae in _context.ActivityEvents select ae);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (activityEventTypeId.HasValue == true)
			{
				query = query.Where(ae => ae.activityEventTypeId == activityEventTypeId.Value);
			}
			if (string.IsNullOrEmpty(title) == false)
			{
				query = query.Where(ae => ae.title == title);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ae => ae.description == description);
			}
			if (string.IsNullOrEmpty(relatedEntityType) == false)
			{
				query = query.Where(ae => ae.relatedEntityType == relatedEntityType);
			}
			if (relatedEntityId.HasValue == true)
			{
				query = query.Where(ae => ae.relatedEntityId == relatedEntityId.Value);
			}
			if (eventDate.HasValue == true)
			{
				query = query.Where(ae => ae.eventDate == eventDate.Value);
			}
			if (isPublic.HasValue == true)
			{
				query = query.Where(ae => ae.isPublic == isPublic.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ae => ae.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ae => ae.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ae => ae.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ae => ae.deleted == false);
				}
			}
			else
			{
				query = query.Where(ae => ae.active == true);
				query = query.Where(ae => ae.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Activity Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.title.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.relatedEntityType.Contains(anyStringContains)
			       || x.activityEventType.name.Contains(anyStringContains)
			       || x.activityEventType.description.Contains(anyStringContains)
			       || x.activityEventType.iconCssClass.Contains(anyStringContains)
			       || x.activityEventType.accentColor.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.title).ThenBy(x => x.relatedEntityType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ActivityEvent.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ActivityEvent/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
