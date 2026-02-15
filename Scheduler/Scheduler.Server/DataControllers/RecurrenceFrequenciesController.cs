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
    /// This auto generated class provides the basic CRUD operations for the RecurrenceFrequency entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the RecurrenceFrequency entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class RecurrenceFrequenciesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<RecurrenceFrequenciesController> _logger;

		public RecurrenceFrequenciesController(SchedulerContext context, ILogger<RecurrenceFrequenciesController> logger) : base("Scheduler", "RecurrenceFrequency")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of RecurrenceFrequencies filtered by the parameters provided.
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
		[Route("api/RecurrenceFrequencies")]
		public async Task<IActionResult> GetRecurrenceFrequencies(
			string name = null,
			string description = null,
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

			IQueryable<Database.RecurrenceFrequency> query = (from rf in _context.RecurrenceFrequencies select rf);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(rf => rf.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(rf => rf.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(rf => rf.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rf => rf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rf => rf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rf => rf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rf => rf.deleted == false);
				}
			}
			else
			{
				query = query.Where(rf => rf.active == true);
				query = query.Where(rf => rf.deleted == false);
			}

			query = query.OrderBy(rf => rf.sequence).ThenBy(rf => rf.name).ThenBy(rf => rf.description);

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
			// Add the any string contains parameter to span all the string fields on the Recurrence Frequency, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.RecurrenceFrequency> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.RecurrenceFrequency recurrenceFrequency in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(recurrenceFrequency, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.RecurrenceFrequency Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.RecurrenceFrequency Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of RecurrenceFrequencies filtered by the parameters provided.  Its query is similar to the GetRecurrenceFrequencies method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RecurrenceFrequencies/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
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

			IQueryable<Database.RecurrenceFrequency> query = (from rf in _context.RecurrenceFrequencies select rf);
			if (name != null)
			{
				query = query.Where(rf => rf.name == name);
			}
			if (description != null)
			{
				query = query.Where(rf => rf.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(rf => rf.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rf => rf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rf => rf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rf => rf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rf => rf.deleted == false);
				}
			}
			else
			{
				query = query.Where(rf => rf.active == true);
				query = query.Where(rf => rf.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Recurrence Frequency, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single RecurrenceFrequency by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RecurrenceFrequency/{id}")]
		public async Task<IActionResult> GetRecurrenceFrequency(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.RecurrenceFrequency> query = (from rf in _context.RecurrenceFrequencies where
							(rf.id == id) &&
							(userIsAdmin == true || rf.deleted == false) &&
							(userIsWriter == true || rf.active == true)
					select rf);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.RecurrenceFrequency materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.RecurrenceFrequency Entity was read with Admin privilege." : "Scheduler.RecurrenceFrequency Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RecurrenceFrequency", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.RecurrenceFrequency entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.RecurrenceFrequency.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.RecurrenceFrequency.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing RecurrenceFrequency record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/RecurrenceFrequency/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutRecurrenceFrequency(int id, [FromBody]Database.RecurrenceFrequency.RecurrenceFrequencyDTO recurrenceFrequencyDTO, CancellationToken cancellationToken = default)
		{
			if (recurrenceFrequencyDTO == null)
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



			if (id != recurrenceFrequencyDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.RecurrenceFrequency> query = (from x in _context.RecurrenceFrequencies
				where
				(x.id == id)
				select x);


			Database.RecurrenceFrequency existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.RecurrenceFrequency PUT", id.ToString(), new Exception("No Scheduler.RecurrenceFrequency entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (recurrenceFrequencyDTO.objectGuid == Guid.Empty)
            {
                recurrenceFrequencyDTO.objectGuid = existing.objectGuid;
            }
            else if (recurrenceFrequencyDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a RecurrenceFrequency record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.RecurrenceFrequency cloneOfExisting = (Database.RecurrenceFrequency)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new RecurrenceFrequency object using the data from the existing record, updated with what is in the DTO.
			//
			Database.RecurrenceFrequency recurrenceFrequency = (Database.RecurrenceFrequency)_context.Entry(existing).GetDatabaseValues().ToObject();
			recurrenceFrequency.ApplyDTO(recurrenceFrequencyDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (recurrenceFrequency.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.RecurrenceFrequency record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (recurrenceFrequency.name != null && recurrenceFrequency.name.Length > 100)
			{
				recurrenceFrequency.name = recurrenceFrequency.name.Substring(0, 100);
			}

			if (recurrenceFrequency.description != null && recurrenceFrequency.description.Length > 500)
			{
				recurrenceFrequency.description = recurrenceFrequency.description.Substring(0, 500);
			}

			EntityEntry<Database.RecurrenceFrequency> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(recurrenceFrequency);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.RecurrenceFrequency entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RecurrenceFrequency.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RecurrenceFrequency.CreateAnonymousWithFirstLevelSubObjects(recurrenceFrequency)),
					null);


				return Ok(Database.RecurrenceFrequency.CreateAnonymous(recurrenceFrequency));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.RecurrenceFrequency entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RecurrenceFrequency.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RecurrenceFrequency.CreateAnonymousWithFirstLevelSubObjects(recurrenceFrequency)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new RecurrenceFrequency record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RecurrenceFrequency", Name = "RecurrenceFrequency")]
		public async Task<IActionResult> PostRecurrenceFrequency([FromBody]Database.RecurrenceFrequency.RecurrenceFrequencyDTO recurrenceFrequencyDTO, CancellationToken cancellationToken = default)
		{
			if (recurrenceFrequencyDTO == null)
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
			// Create a new RecurrenceFrequency object using the data from the DTO
			//
			Database.RecurrenceFrequency recurrenceFrequency = Database.RecurrenceFrequency.FromDTO(recurrenceFrequencyDTO);

			try
			{
				if (recurrenceFrequency.name != null && recurrenceFrequency.name.Length > 100)
				{
					recurrenceFrequency.name = recurrenceFrequency.name.Substring(0, 100);
				}

				if (recurrenceFrequency.description != null && recurrenceFrequency.description.Length > 500)
				{
					recurrenceFrequency.description = recurrenceFrequency.description.Substring(0, 500);
				}

				recurrenceFrequency.objectGuid = Guid.NewGuid();
				_context.RecurrenceFrequencies.Add(recurrenceFrequency);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.RecurrenceFrequency entity successfully created.",
					true,
					recurrenceFrequency.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.RecurrenceFrequency.CreateAnonymousWithFirstLevelSubObjects(recurrenceFrequency)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.RecurrenceFrequency entity creation failed.", false, recurrenceFrequency.id.ToString(), "", JsonSerializer.Serialize(recurrenceFrequency), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RecurrenceFrequency", recurrenceFrequency.id, recurrenceFrequency.name));

			return CreatedAtRoute("RecurrenceFrequency", new { id = recurrenceFrequency.id }, Database.RecurrenceFrequency.CreateAnonymousWithFirstLevelSubObjects(recurrenceFrequency));
		}



        /// <summary>
        /// 
        /// This deletes a RecurrenceFrequency record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RecurrenceFrequency/{id}")]
		[Route("api/RecurrenceFrequency")]
		public async Task<IActionResult> DeleteRecurrenceFrequency(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.RecurrenceFrequency> query = (from x in _context.RecurrenceFrequencies
				where
				(x.id == id)
				select x);


			Database.RecurrenceFrequency recurrenceFrequency = await query.FirstOrDefaultAsync(cancellationToken);

			if (recurrenceFrequency == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.RecurrenceFrequency DELETE", id.ToString(), new Exception("No Scheduler.RecurrenceFrequency entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.RecurrenceFrequency cloneOfExisting = (Database.RecurrenceFrequency)_context.Entry(recurrenceFrequency).GetDatabaseValues().ToObject();


			try
			{
				recurrenceFrequency.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.RecurrenceFrequency entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.RecurrenceFrequency.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RecurrenceFrequency.CreateAnonymousWithFirstLevelSubObjects(recurrenceFrequency)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.RecurrenceFrequency entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.RecurrenceFrequency.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.RecurrenceFrequency.CreateAnonymousWithFirstLevelSubObjects(recurrenceFrequency)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of RecurrenceFrequency records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/RecurrenceFrequencies/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
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

			IQueryable<Database.RecurrenceFrequency> query = (from rf in _context.RecurrenceFrequencies select rf);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(rf => rf.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(rf => rf.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(rf => rf.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rf => rf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rf => rf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rf => rf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rf => rf.deleted == false);
				}
			}
			else
			{
				query = query.Where(rf => rf.active == true);
				query = query.Where(rf => rf.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Recurrence Frequency, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.RecurrenceFrequency.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/RecurrenceFrequency/CreateAuditEvent")]
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
