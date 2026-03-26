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
    /// This auto generated class provides the basic CRUD operations for the VolunteerStatus entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the VolunteerStatus entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class VolunteerStatusesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 40;

		private SchedulerContext _context;

		private ILogger<VolunteerStatusesController> _logger;

		public VolunteerStatusesController(SchedulerContext context, ILogger<VolunteerStatusesController> logger) : base("Scheduler", "VolunteerStatus")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of VolunteerStatuses filtered by the parameters provided.
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
		[Route("api/VolunteerStatuses")]
		public async Task<IActionResult> GetVolunteerStatuses(
			string name = null,
			string description = null,
			int? sequence = null,
			string color = null,
			int? iconId = null,
			bool? isActive = null,
			bool? preventsScheduling = null,
			bool? requiresApproval = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 40, cancellationToken);
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

			IQueryable<Database.VolunteerStatus> query = (from vs in _context.VolunteerStatuses select vs);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(vs => vs.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(vs => vs.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(vs => vs.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(vs => vs.color == color);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(vs => vs.iconId == iconId.Value);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(vs => vs.isActive == isActive.Value);
			}
			if (preventsScheduling.HasValue == true)
			{
				query = query.Where(vs => vs.preventsScheduling == preventsScheduling.Value);
			}
			if (requiresApproval.HasValue == true)
			{
				query = query.Where(vs => vs.requiresApproval == requiresApproval.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vs => vs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vs => vs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vs => vs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vs => vs.deleted == false);
				}
			}
			else
			{
				query = query.Where(vs => vs.active == true);
				query = query.Where(vs => vs.deleted == false);
			}

			query = query.OrderBy(vs => vs.sequence).ThenBy(vs => vs.name).ThenBy(vs => vs.description).ThenBy(vs => vs.color);


			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Status, or on an any of the string fields on its immediate relations
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
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.icon);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.VolunteerStatus> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.VolunteerStatus volunteerStatus in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(volunteerStatus, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.VolunteerStatus Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.VolunteerStatus Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of VolunteerStatuses filtered by the parameters provided.  Its query is similar to the GetVolunteerStatuses method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerStatuses/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? sequence = null,
			string color = null,
			int? iconId = null,
			bool? isActive = null,
			bool? preventsScheduling = null,
			bool? requiresApproval = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 40, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.VolunteerStatus> query = (from vs in _context.VolunteerStatuses select vs);
			if (name != null)
			{
				query = query.Where(vs => vs.name == name);
			}
			if (description != null)
			{
				query = query.Where(vs => vs.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(vs => vs.sequence == sequence.Value);
			}
			if (color != null)
			{
				query = query.Where(vs => vs.color == color);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(vs => vs.iconId == iconId.Value);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(vs => vs.isActive == isActive.Value);
			}
			if (preventsScheduling.HasValue == true)
			{
				query = query.Where(vs => vs.preventsScheduling == preventsScheduling.Value);
			}
			if (requiresApproval.HasValue == true)
			{
				query = query.Where(vs => vs.requiresApproval == requiresApproval.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vs => vs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vs => vs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vs => vs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vs => vs.deleted == false);
				}
			}
			else
			{
				query = query.Where(vs => vs.active == true);
				query = query.Where(vs => vs.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Status, or on an any of the string fields on its immediate relations
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
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single VolunteerStatus by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerStatus/{id}")]
		public async Task<IActionResult> GetVolunteerStatus(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 40, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.VolunteerStatus> query = (from vs in _context.VolunteerStatuses where
							(vs.id == id) &&
							(userIsAdmin == true || vs.deleted == false) &&
							(userIsWriter == true || vs.active == true)
					select vs);

				if (includeRelations == true)
				{
					query = query.Include(x => x.icon);
					query = query.AsSplitQuery();
				}

				Database.VolunteerStatus materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.VolunteerStatus Entity was read with Admin privilege." : "Scheduler.VolunteerStatus Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "VolunteerStatus", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.VolunteerStatus entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.VolunteerStatus.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.VolunteerStatus.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing VolunteerStatus record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/VolunteerStatus/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutVolunteerStatus(int id, [FromBody]Database.VolunteerStatus.VolunteerStatusDTO volunteerStatusDTO, CancellationToken cancellationToken = default)
		{
			if (volunteerStatusDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Volunteer Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Volunteer Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != volunteerStatusDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 40, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.VolunteerStatus> query = (from x in _context.VolunteerStatuses
				where
				(x.id == id)
				select x);


			Database.VolunteerStatus existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerStatus PUT", id.ToString(), new Exception("No Scheduler.VolunteerStatus entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (volunteerStatusDTO.objectGuid == Guid.Empty)
            {
                volunteerStatusDTO.objectGuid = existing.objectGuid;
            }
            else if (volunteerStatusDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a VolunteerStatus record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.VolunteerStatus cloneOfExisting = (Database.VolunteerStatus)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new VolunteerStatus object using the data from the existing record, updated with what is in the DTO.
			//
			Database.VolunteerStatus volunteerStatus = (Database.VolunteerStatus)_context.Entry(existing).GetDatabaseValues().ToObject();
			volunteerStatus.ApplyDTO(volunteerStatusDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (volunteerStatus.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.VolunteerStatus record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (volunteerStatus.name != null && volunteerStatus.name.Length > 100)
			{
				volunteerStatus.name = volunteerStatus.name.Substring(0, 100);
			}

			if (volunteerStatus.description != null && volunteerStatus.description.Length > 500)
			{
				volunteerStatus.description = volunteerStatus.description.Substring(0, 500);
			}

			if (volunteerStatus.color != null && volunteerStatus.color.Length > 10)
			{
				volunteerStatus.color = volunteerStatus.color.Substring(0, 10);
			}

			EntityEntry<Database.VolunteerStatus> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(volunteerStatus);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.VolunteerStatus entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.VolunteerStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.VolunteerStatus.CreateAnonymousWithFirstLevelSubObjects(volunteerStatus)),
					null);


				return Ok(Database.VolunteerStatus.CreateAnonymous(volunteerStatus));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.VolunteerStatus entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.VolunteerStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.VolunteerStatus.CreateAnonymousWithFirstLevelSubObjects(volunteerStatus)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new VolunteerStatus record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerStatus", Name = "VolunteerStatus")]
		public async Task<IActionResult> PostVolunteerStatus([FromBody]Database.VolunteerStatus.VolunteerStatusDTO volunteerStatusDTO, CancellationToken cancellationToken = default)
		{
			if (volunteerStatusDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Volunteer Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Volunteer Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new VolunteerStatus object using the data from the DTO
			//
			Database.VolunteerStatus volunteerStatus = Database.VolunteerStatus.FromDTO(volunteerStatusDTO);

			try
			{
				if (volunteerStatus.name != null && volunteerStatus.name.Length > 100)
				{
					volunteerStatus.name = volunteerStatus.name.Substring(0, 100);
				}

				if (volunteerStatus.description != null && volunteerStatus.description.Length > 500)
				{
					volunteerStatus.description = volunteerStatus.description.Substring(0, 500);
				}

				if (volunteerStatus.color != null && volunteerStatus.color.Length > 10)
				{
					volunteerStatus.color = volunteerStatus.color.Substring(0, 10);
				}

				volunteerStatus.objectGuid = Guid.NewGuid();
				_context.VolunteerStatuses.Add(volunteerStatus);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.VolunteerStatus entity successfully created.",
					true,
					volunteerStatus.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.VolunteerStatus.CreateAnonymousWithFirstLevelSubObjects(volunteerStatus)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.VolunteerStatus entity creation failed.", false, volunteerStatus.id.ToString(), "", JsonSerializer.Serialize(volunteerStatus), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "VolunteerStatus", volunteerStatus.id, volunteerStatus.name));

			return CreatedAtRoute("VolunteerStatus", new { id = volunteerStatus.id }, Database.VolunteerStatus.CreateAnonymousWithFirstLevelSubObjects(volunteerStatus));
		}



        /// <summary>
        /// 
        /// This deletes a VolunteerStatus record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/VolunteerStatus/{id}")]
		[Route("api/VolunteerStatus")]
		public async Task<IActionResult> DeleteVolunteerStatus(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Volunteer Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Volunteer Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.VolunteerStatus> query = (from x in _context.VolunteerStatuses
				where
				(x.id == id)
				select x);


			Database.VolunteerStatus volunteerStatus = await query.FirstOrDefaultAsync(cancellationToken);

			if (volunteerStatus == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.VolunteerStatus DELETE", id.ToString(), new Exception("No Scheduler.VolunteerStatus entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.VolunteerStatus cloneOfExisting = (Database.VolunteerStatus)_context.Entry(volunteerStatus).GetDatabaseValues().ToObject();


			try
			{
				volunteerStatus.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.VolunteerStatus entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.VolunteerStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.VolunteerStatus.CreateAnonymousWithFirstLevelSubObjects(volunteerStatus)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.VolunteerStatus entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.VolunteerStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.VolunteerStatus.CreateAnonymousWithFirstLevelSubObjects(volunteerStatus)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of VolunteerStatus records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/VolunteerStatuses/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? sequence = null,
			string color = null,
			int? iconId = null,
			bool? isActive = null,
			bool? preventsScheduling = null,
			bool? requiresApproval = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 40, cancellationToken);


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

			IQueryable<Database.VolunteerStatus> query = (from vs in _context.VolunteerStatuses select vs);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(vs => vs.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(vs => vs.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(vs => vs.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(vs => vs.color == color);
			}
			if (iconId.HasValue == true)
			{
				query = query.Where(vs => vs.iconId == iconId.Value);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(vs => vs.isActive == isActive.Value);
			}
			if (preventsScheduling.HasValue == true)
			{
				query = query.Where(vs => vs.preventsScheduling == preventsScheduling.Value);
			}
			if (requiresApproval.HasValue == true)
			{
				query = query.Where(vs => vs.requiresApproval == requiresApproval.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(vs => vs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(vs => vs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(vs => vs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(vs => vs.deleted == false);
				}
			}
			else
			{
				query = query.Where(vs => vs.active == true);
				query = query.Where(vs => vs.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Volunteer Status, or on an any of the string fields on its immediate relations
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
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.VolunteerStatus.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/VolunteerStatus/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Volunteer Manager role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Volunteer Manager", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
