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
    /// This auto generated class provides the basic CRUD operations for the PeriodStatus entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PeriodStatus entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PeriodStatusesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<PeriodStatusesController> _logger;

		public PeriodStatusesController(SchedulerContext context, ILogger<PeriodStatusesController> logger) : base("Scheduler", "PeriodStatus")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PeriodStatuses filtered by the parameters provided.
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
		[Route("api/PeriodStatuses")]
		public async Task<IActionResult> GetPeriodStatuses(
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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

			IQueryable<Database.PeriodStatus> query = (from ps in _context.PeriodStatuses select ps);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ps => ps.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ps => ps.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(ps => ps.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ps => ps.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ps => ps.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ps => ps.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ps => ps.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ps => ps.deleted == false);
				}
			}
			else
			{
				query = query.Where(ps => ps.active == true);
				query = query.Where(ps => ps.deleted == false);
			}

			query = query.OrderBy(ps => ps.sequence).ThenBy(ps => ps.name).ThenBy(ps => ps.description).ThenBy(ps => ps.color);


			//
			// Add the any string contains parameter to span all the string fields on the Period Status, or on an any of the string fields on its immediate relations
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
			
			List<Database.PeriodStatus> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PeriodStatus periodStatus in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(periodStatus, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.PeriodStatus Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.PeriodStatus Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PeriodStatuses filtered by the parameters provided.  Its query is similar to the GetPeriodStatuses method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PeriodStatuses/RowCount")]
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.PeriodStatus> query = (from ps in _context.PeriodStatuses select ps);
			if (name != null)
			{
				query = query.Where(ps => ps.name == name);
			}
			if (description != null)
			{
				query = query.Where(ps => ps.description == description);
			}
			if (color != null)
			{
				query = query.Where(ps => ps.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ps => ps.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ps => ps.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ps => ps.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ps => ps.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ps => ps.deleted == false);
				}
			}
			else
			{
				query = query.Where(ps => ps.active == true);
				query = query.Where(ps => ps.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Period Status, or on an any of the string fields on its immediate relations
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
        /// This gets a single PeriodStatus by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PeriodStatus/{id}")]
		public async Task<IActionResult> GetPeriodStatus(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			try
			{
				IQueryable<Database.PeriodStatus> query = (from ps in _context.PeriodStatuses where
							(ps.id == id) &&
							(userIsAdmin == true || ps.deleted == false) &&
							(userIsWriter == true || ps.active == true)
					select ps);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.PeriodStatus materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.PeriodStatus Entity was read with Admin privilege." : "Scheduler.PeriodStatus Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PeriodStatus", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.PeriodStatus entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.PeriodStatus.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.PeriodStatus.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PeriodStatus record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PeriodStatus/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPeriodStatus(int id, [FromBody]Database.PeriodStatus.PeriodStatusDTO periodStatusDTO, CancellationToken cancellationToken = default)
		{
			if (periodStatusDTO == null)
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



			if (id != periodStatusDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.PeriodStatus> query = (from x in _context.PeriodStatuses
				where
				(x.id == id)
				select x);


			Database.PeriodStatus existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PeriodStatus PUT", id.ToString(), new Exception("No Scheduler.PeriodStatus entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (periodStatusDTO.objectGuid == Guid.Empty)
            {
                periodStatusDTO.objectGuid = existing.objectGuid;
            }
            else if (periodStatusDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PeriodStatus record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PeriodStatus cloneOfExisting = (Database.PeriodStatus)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PeriodStatus object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PeriodStatus periodStatus = (Database.PeriodStatus)_context.Entry(existing).GetDatabaseValues().ToObject();
			periodStatus.ApplyDTO(periodStatusDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (periodStatus.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.PeriodStatus record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (periodStatus.name != null && periodStatus.name.Length > 100)
			{
				periodStatus.name = periodStatus.name.Substring(0, 100);
			}

			if (periodStatus.description != null && periodStatus.description.Length > 500)
			{
				periodStatus.description = periodStatus.description.Substring(0, 500);
			}

			if (periodStatus.color != null && periodStatus.color.Length > 10)
			{
				periodStatus.color = periodStatus.color.Substring(0, 10);
			}

			EntityEntry<Database.PeriodStatus> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(periodStatus);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.PeriodStatus entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PeriodStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PeriodStatus.CreateAnonymousWithFirstLevelSubObjects(periodStatus)),
					null);


				return Ok(Database.PeriodStatus.CreateAnonymous(periodStatus));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.PeriodStatus entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PeriodStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PeriodStatus.CreateAnonymousWithFirstLevelSubObjects(periodStatus)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new PeriodStatus record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PeriodStatus", Name = "PeriodStatus")]
		public async Task<IActionResult> PostPeriodStatus([FromBody]Database.PeriodStatus.PeriodStatusDTO periodStatusDTO, CancellationToken cancellationToken = default)
		{
			if (periodStatusDTO == null)
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
			// Create a new PeriodStatus object using the data from the DTO
			//
			Database.PeriodStatus periodStatus = Database.PeriodStatus.FromDTO(periodStatusDTO);

			try
			{
				if (periodStatus.name != null && periodStatus.name.Length > 100)
				{
					periodStatus.name = periodStatus.name.Substring(0, 100);
				}

				if (periodStatus.description != null && periodStatus.description.Length > 500)
				{
					periodStatus.description = periodStatus.description.Substring(0, 500);
				}

				if (periodStatus.color != null && periodStatus.color.Length > 10)
				{
					periodStatus.color = periodStatus.color.Substring(0, 10);
				}

				periodStatus.objectGuid = Guid.NewGuid();
				_context.PeriodStatuses.Add(periodStatus);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.PeriodStatus entity successfully created.",
					true,
					periodStatus.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PeriodStatus.CreateAnonymousWithFirstLevelSubObjects(periodStatus)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.PeriodStatus entity creation failed.", false, periodStatus.id.ToString(), "", JsonSerializer.Serialize(periodStatus), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PeriodStatus", periodStatus.id, periodStatus.name));

			return CreatedAtRoute("PeriodStatus", new { id = periodStatus.id }, Database.PeriodStatus.CreateAnonymousWithFirstLevelSubObjects(periodStatus));
		}



        /// <summary>
        /// 
        /// This deletes a PeriodStatus record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PeriodStatus/{id}")]
		[Route("api/PeriodStatus")]
		public async Task<IActionResult> DeletePeriodStatus(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.PeriodStatus> query = (from x in _context.PeriodStatuses
				where
				(x.id == id)
				select x);


			Database.PeriodStatus periodStatus = await query.FirstOrDefaultAsync(cancellationToken);

			if (periodStatus == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PeriodStatus DELETE", id.ToString(), new Exception("No Scheduler.PeriodStatus entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PeriodStatus cloneOfExisting = (Database.PeriodStatus)_context.Entry(periodStatus).GetDatabaseValues().ToObject();


			try
			{
				periodStatus.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.PeriodStatus entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PeriodStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PeriodStatus.CreateAnonymousWithFirstLevelSubObjects(periodStatus)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.PeriodStatus entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PeriodStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PeriodStatus.CreateAnonymousWithFirstLevelSubObjects(periodStatus)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of PeriodStatus records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PeriodStatuses/ListData")]
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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

			IQueryable<Database.PeriodStatus> query = (from ps in _context.PeriodStatuses select ps);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ps => ps.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ps => ps.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(ps => ps.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ps => ps.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ps => ps.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ps => ps.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ps => ps.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ps => ps.deleted == false);
				}
			}
			else
			{
				query = query.Where(ps => ps.active == true);
				query = query.Where(ps => ps.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Period Status, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.PeriodStatus.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PeriodStatus/CreateAuditEvent")]
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
