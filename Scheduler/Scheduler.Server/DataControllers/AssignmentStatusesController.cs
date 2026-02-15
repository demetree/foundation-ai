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
    /// This auto generated class provides the basic CRUD operations for the AssignmentStatus entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AssignmentStatus entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AssignmentStatusesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<AssignmentStatusesController> _logger;

		public AssignmentStatusesController(SchedulerContext context, ILogger<AssignmentStatusesController> logger) : base("Scheduler", "AssignmentStatus")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AssignmentStatuses filtered by the parameters provided.
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
		[Route("api/AssignmentStatuses")]
		public async Task<IActionResult> GetAssignmentStatuses(
			string name = null,
			string description = null,
			string color = null,
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

			IQueryable<Database.AssignmentStatus> query = (from _as in _context.AssignmentStatuses select _as);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(_as => _as.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(_as => _as.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(_as => _as.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(_as => _as.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(_as => _as.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(_as => _as.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(_as => _as.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(_as => _as.deleted == false);
				}
			}
			else
			{
				query = query.Where(_as => _as.active == true);
				query = query.Where(_as => _as.deleted == false);
			}

			query = query.OrderBy(_as => _as.sequence).ThenBy(_as => _as.name).ThenBy(_as => _as.description).ThenBy(_as => _as.color);

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
			// Add the any string contains parameter to span all the string fields on the Assignment Status, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.AssignmentStatus> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AssignmentStatus assignmentStatus in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(assignmentStatus, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.AssignmentStatus Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.AssignmentStatus Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AssignmentStatuses filtered by the parameters provided.  Its query is similar to the GetAssignmentStatuses method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AssignmentStatuses/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string color = null,
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

			IQueryable<Database.AssignmentStatus> query = (from _as in _context.AssignmentStatuses select _as);
			if (name != null)
			{
				query = query.Where(_as => _as.name == name);
			}
			if (description != null)
			{
				query = query.Where(_as => _as.description == description);
			}
			if (color != null)
			{
				query = query.Where(_as => _as.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(_as => _as.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(_as => _as.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(_as => _as.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(_as => _as.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(_as => _as.deleted == false);
				}
			}
			else
			{
				query = query.Where(_as => _as.active == true);
				query = query.Where(_as => _as.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Assignment Status, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AssignmentStatus by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AssignmentStatus/{id}")]
		public async Task<IActionResult> GetAssignmentStatus(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.AssignmentStatus> query = (from _as in _context.AssignmentStatuses where
							(_as.id == id) &&
							(userIsAdmin == true || _as.deleted == false) &&
							(userIsWriter == true || _as.active == true)
					select _as);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.AssignmentStatus materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.AssignmentStatus Entity was read with Admin privilege." : "Scheduler.AssignmentStatus Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AssignmentStatus", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.AssignmentStatus entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.AssignmentStatus.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.AssignmentStatus.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AssignmentStatus record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AssignmentStatus/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAssignmentStatus(int id, [FromBody]Database.AssignmentStatus.AssignmentStatusDTO assignmentStatusDTO, CancellationToken cancellationToken = default)
		{
			if (assignmentStatusDTO == null)
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



			if (id != assignmentStatusDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AssignmentStatus> query = (from x in _context.AssignmentStatuses
				where
				(x.id == id)
				select x);


			Database.AssignmentStatus existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AssignmentStatus PUT", id.ToString(), new Exception("No Scheduler.AssignmentStatus entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (assignmentStatusDTO.objectGuid == Guid.Empty)
            {
                assignmentStatusDTO.objectGuid = existing.objectGuid;
            }
            else if (assignmentStatusDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a AssignmentStatus record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AssignmentStatus cloneOfExisting = (Database.AssignmentStatus)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AssignmentStatus object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AssignmentStatus assignmentStatus = (Database.AssignmentStatus)_context.Entry(existing).GetDatabaseValues().ToObject();
			assignmentStatus.ApplyDTO(assignmentStatusDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (assignmentStatus.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.AssignmentStatus record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (assignmentStatus.name != null && assignmentStatus.name.Length > 100)
			{
				assignmentStatus.name = assignmentStatus.name.Substring(0, 100);
			}

			if (assignmentStatus.description != null && assignmentStatus.description.Length > 500)
			{
				assignmentStatus.description = assignmentStatus.description.Substring(0, 500);
			}

			if (assignmentStatus.color != null && assignmentStatus.color.Length > 10)
			{
				assignmentStatus.color = assignmentStatus.color.Substring(0, 10);
			}

			EntityEntry<Database.AssignmentStatus> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(assignmentStatus);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.AssignmentStatus entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AssignmentStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AssignmentStatus.CreateAnonymousWithFirstLevelSubObjects(assignmentStatus)),
					null);


				return Ok(Database.AssignmentStatus.CreateAnonymous(assignmentStatus));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.AssignmentStatus entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AssignmentStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AssignmentStatus.CreateAnonymousWithFirstLevelSubObjects(assignmentStatus)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AssignmentStatus record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AssignmentStatus", Name = "AssignmentStatus")]
		public async Task<IActionResult> PostAssignmentStatus([FromBody]Database.AssignmentStatus.AssignmentStatusDTO assignmentStatusDTO, CancellationToken cancellationToken = default)
		{
			if (assignmentStatusDTO == null)
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
			// Create a new AssignmentStatus object using the data from the DTO
			//
			Database.AssignmentStatus assignmentStatus = Database.AssignmentStatus.FromDTO(assignmentStatusDTO);

			try
			{
				if (assignmentStatus.name != null && assignmentStatus.name.Length > 100)
				{
					assignmentStatus.name = assignmentStatus.name.Substring(0, 100);
				}

				if (assignmentStatus.description != null && assignmentStatus.description.Length > 500)
				{
					assignmentStatus.description = assignmentStatus.description.Substring(0, 500);
				}

				if (assignmentStatus.color != null && assignmentStatus.color.Length > 10)
				{
					assignmentStatus.color = assignmentStatus.color.Substring(0, 10);
				}

				assignmentStatus.objectGuid = Guid.NewGuid();
				_context.AssignmentStatuses.Add(assignmentStatus);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.AssignmentStatus entity successfully created.",
					true,
					assignmentStatus.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AssignmentStatus.CreateAnonymousWithFirstLevelSubObjects(assignmentStatus)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.AssignmentStatus entity creation failed.", false, assignmentStatus.id.ToString(), "", JsonSerializer.Serialize(assignmentStatus), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AssignmentStatus", assignmentStatus.id, assignmentStatus.name));

			return CreatedAtRoute("AssignmentStatus", new { id = assignmentStatus.id }, Database.AssignmentStatus.CreateAnonymousWithFirstLevelSubObjects(assignmentStatus));
		}



        /// <summary>
        /// 
        /// This deletes a AssignmentStatus record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AssignmentStatus/{id}")]
		[Route("api/AssignmentStatus")]
		public async Task<IActionResult> DeleteAssignmentStatus(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.AssignmentStatus> query = (from x in _context.AssignmentStatuses
				where
				(x.id == id)
				select x);


			Database.AssignmentStatus assignmentStatus = await query.FirstOrDefaultAsync(cancellationToken);

			if (assignmentStatus == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AssignmentStatus DELETE", id.ToString(), new Exception("No Scheduler.AssignmentStatus entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AssignmentStatus cloneOfExisting = (Database.AssignmentStatus)_context.Entry(assignmentStatus).GetDatabaseValues().ToObject();


			try
			{
				assignmentStatus.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.AssignmentStatus entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AssignmentStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AssignmentStatus.CreateAnonymousWithFirstLevelSubObjects(assignmentStatus)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.AssignmentStatus entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AssignmentStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AssignmentStatus.CreateAnonymousWithFirstLevelSubObjects(assignmentStatus)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AssignmentStatus records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AssignmentStatuses/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string color = null,
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

			IQueryable<Database.AssignmentStatus> query = (from _as in _context.AssignmentStatuses select _as);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(_as => _as.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(_as => _as.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(_as => _as.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(_as => _as.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(_as => _as.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(_as => _as.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(_as => _as.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(_as => _as.deleted == false);
				}
			}
			else
			{
				query = query.Where(_as => _as.active == true);
				query = query.Where(_as => _as.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Assignment Status, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AssignmentStatus.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AssignmentStatus/CreateAuditEvent")]
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
