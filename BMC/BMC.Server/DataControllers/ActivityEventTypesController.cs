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
    /// This auto generated class provides the basic CRUD operations for the ActivityEventType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ActivityEventType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ActivityEventTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<ActivityEventTypesController> _logger;

		public ActivityEventTypesController(BMCContext context, ILogger<ActivityEventTypesController> logger) : base("BMC", "ActivityEventType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ActivityEventTypes filtered by the parameters provided.
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
		[Route("api/ActivityEventTypes")]
		public async Task<IActionResult> GetActivityEventTypes(
			string name = null,
			string description = null,
			string iconCssClass = null,
			string accentColor = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.ActivityEventType> query = (from aet in _context.ActivityEventTypes select aet);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(aet => aet.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(aet => aet.description == description);
			}
			if (string.IsNullOrEmpty(iconCssClass) == false)
			{
				query = query.Where(aet => aet.iconCssClass == iconCssClass);
			}
			if (string.IsNullOrEmpty(accentColor) == false)
			{
				query = query.Where(aet => aet.accentColor == accentColor);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(aet => aet.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(aet => aet.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(aet => aet.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(aet => aet.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(aet => aet.deleted == false);
				}
			}
			else
			{
				query = query.Where(aet => aet.active == true);
				query = query.Where(aet => aet.deleted == false);
			}

			query = query.OrderBy(aet => aet.sequence).ThenBy(aet => aet.name).ThenBy(aet => aet.description).ThenBy(aet => aet.iconCssClass);

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
			// Add the any string contains parameter to span all the string fields on the Activity Event Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.iconCssClass.Contains(anyStringContains)
			       || x.accentColor.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ActivityEventType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ActivityEventType activityEventType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(activityEventType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ActivityEventType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ActivityEventType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ActivityEventTypes filtered by the parameters provided.  Its query is similar to the GetActivityEventTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ActivityEventTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string iconCssClass = null,
			string accentColor = null,
			int? sequence = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.ActivityEventType> query = (from aet in _context.ActivityEventTypes select aet);
			if (name != null)
			{
				query = query.Where(aet => aet.name == name);
			}
			if (description != null)
			{
				query = query.Where(aet => aet.description == description);
			}
			if (iconCssClass != null)
			{
				query = query.Where(aet => aet.iconCssClass == iconCssClass);
			}
			if (accentColor != null)
			{
				query = query.Where(aet => aet.accentColor == accentColor);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(aet => aet.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(aet => aet.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(aet => aet.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(aet => aet.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(aet => aet.deleted == false);
				}
			}
			else
			{
				query = query.Where(aet => aet.active == true);
				query = query.Where(aet => aet.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Activity Event Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.iconCssClass.Contains(anyStringContains)
			       || x.accentColor.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ActivityEventType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ActivityEventType/{id}")]
		public async Task<IActionResult> GetActivityEventType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.ActivityEventType> query = (from aet in _context.ActivityEventTypes where
							(aet.id == id) &&
							(userIsAdmin == true || aet.deleted == false) &&
							(userIsWriter == true || aet.active == true)
					select aet);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.ActivityEventType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ActivityEventType Entity was read with Admin privilege." : "BMC.ActivityEventType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ActivityEventType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ActivityEventType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ActivityEventType.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ActivityEventType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ActivityEventType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ActivityEventType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutActivityEventType(int id, [FromBody]Database.ActivityEventType.ActivityEventTypeDTO activityEventTypeDTO, CancellationToken cancellationToken = default)
		{
			if (activityEventTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != activityEventTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ActivityEventType> query = (from x in _context.ActivityEventTypes
				where
				(x.id == id)
				select x);


			Database.ActivityEventType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ActivityEventType PUT", id.ToString(), new Exception("No BMC.ActivityEventType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (activityEventTypeDTO.objectGuid == Guid.Empty)
            {
                activityEventTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (activityEventTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ActivityEventType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ActivityEventType cloneOfExisting = (Database.ActivityEventType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ActivityEventType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ActivityEventType activityEventType = (Database.ActivityEventType)_context.Entry(existing).GetDatabaseValues().ToObject();
			activityEventType.ApplyDTO(activityEventTypeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (activityEventType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ActivityEventType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (activityEventType.name != null && activityEventType.name.Length > 100)
			{
				activityEventType.name = activityEventType.name.Substring(0, 100);
			}

			if (activityEventType.description != null && activityEventType.description.Length > 500)
			{
				activityEventType.description = activityEventType.description.Substring(0, 500);
			}

			if (activityEventType.iconCssClass != null && activityEventType.iconCssClass.Length > 100)
			{
				activityEventType.iconCssClass = activityEventType.iconCssClass.Substring(0, 100);
			}

			if (activityEventType.accentColor != null && activityEventType.accentColor.Length > 10)
			{
				activityEventType.accentColor = activityEventType.accentColor.Substring(0, 10);
			}

			EntityEntry<Database.ActivityEventType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(activityEventType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ActivityEventType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ActivityEventType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ActivityEventType.CreateAnonymousWithFirstLevelSubObjects(activityEventType)),
					null);


				return Ok(Database.ActivityEventType.CreateAnonymous(activityEventType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ActivityEventType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ActivityEventType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ActivityEventType.CreateAnonymousWithFirstLevelSubObjects(activityEventType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ActivityEventType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ActivityEventType", Name = "ActivityEventType")]
		public async Task<IActionResult> PostActivityEventType([FromBody]Database.ActivityEventType.ActivityEventTypeDTO activityEventTypeDTO, CancellationToken cancellationToken = default)
		{
			if (activityEventTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new ActivityEventType object using the data from the DTO
			//
			Database.ActivityEventType activityEventType = Database.ActivityEventType.FromDTO(activityEventTypeDTO);

			try
			{
				if (activityEventType.name != null && activityEventType.name.Length > 100)
				{
					activityEventType.name = activityEventType.name.Substring(0, 100);
				}

				if (activityEventType.description != null && activityEventType.description.Length > 500)
				{
					activityEventType.description = activityEventType.description.Substring(0, 500);
				}

				if (activityEventType.iconCssClass != null && activityEventType.iconCssClass.Length > 100)
				{
					activityEventType.iconCssClass = activityEventType.iconCssClass.Substring(0, 100);
				}

				if (activityEventType.accentColor != null && activityEventType.accentColor.Length > 10)
				{
					activityEventType.accentColor = activityEventType.accentColor.Substring(0, 10);
				}

				activityEventType.objectGuid = Guid.NewGuid();
				_context.ActivityEventTypes.Add(activityEventType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ActivityEventType entity successfully created.",
					true,
					activityEventType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ActivityEventType.CreateAnonymousWithFirstLevelSubObjects(activityEventType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ActivityEventType entity creation failed.", false, activityEventType.id.ToString(), "", JsonSerializer.Serialize(activityEventType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ActivityEventType", activityEventType.id, activityEventType.name));

			return CreatedAtRoute("ActivityEventType", new { id = activityEventType.id }, Database.ActivityEventType.CreateAnonymousWithFirstLevelSubObjects(activityEventType));
		}



        /// <summary>
        /// 
        /// This deletes a ActivityEventType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ActivityEventType/{id}")]
		[Route("api/ActivityEventType")]
		public async Task<IActionResult> DeleteActivityEventType(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.ActivityEventType> query = (from x in _context.ActivityEventTypes
				where
				(x.id == id)
				select x);


			Database.ActivityEventType activityEventType = await query.FirstOrDefaultAsync(cancellationToken);

			if (activityEventType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ActivityEventType DELETE", id.ToString(), new Exception("No BMC.ActivityEventType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ActivityEventType cloneOfExisting = (Database.ActivityEventType)_context.Entry(activityEventType).GetDatabaseValues().ToObject();


			try
			{
				activityEventType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ActivityEventType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ActivityEventType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ActivityEventType.CreateAnonymousWithFirstLevelSubObjects(activityEventType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ActivityEventType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ActivityEventType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ActivityEventType.CreateAnonymousWithFirstLevelSubObjects(activityEventType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ActivityEventType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ActivityEventTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string iconCssClass = null,
			string accentColor = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.ActivityEventType> query = (from aet in _context.ActivityEventTypes select aet);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(aet => aet.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(aet => aet.description == description);
			}
			if (string.IsNullOrEmpty(iconCssClass) == false)
			{
				query = query.Where(aet => aet.iconCssClass == iconCssClass);
			}
			if (string.IsNullOrEmpty(accentColor) == false)
			{
				query = query.Where(aet => aet.accentColor == accentColor);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(aet => aet.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(aet => aet.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(aet => aet.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(aet => aet.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(aet => aet.deleted == false);
				}
			}
			else
			{
				query = query.Where(aet => aet.active == true);
				query = query.Where(aet => aet.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Activity Event Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.iconCssClass.Contains(anyStringContains)
			       || x.accentColor.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.iconCssClass);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ActivityEventType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ActivityEventType/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
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
