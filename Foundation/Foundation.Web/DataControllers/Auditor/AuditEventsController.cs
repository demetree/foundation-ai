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
using Foundation.Auditor.Database;

namespace Foundation.Auditor.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the AuditEvent entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AuditEvent entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AuditEventsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<AuditEventsController> _logger;

		public AuditEventsController(AuditorContext context, ILogger<AuditEventsController> logger) : base("Auditor", "AuditEvent")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This gets a list of AuditEvents filtered by the parameters provided.
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
		[Route("api/AuditEvents")]
		public async Task<IActionResult> GetAuditEvents(
			DateTime? startTime = null,
			DateTime? stopTime = null,
			bool? completedSuccessfully = null,
			int? auditUserId = null,
			int? auditSessionId = null,
			int? auditTypeId = null,
			int? auditAccessTypeId = null,
			int? auditSourceId = null,
			int? auditUserAgentId = null,
			int? auditModuleId = null,
			int? auditModuleEntityId = null,
			int? auditResourceId = null,
			int? auditHostSystemId = null,
			string primaryKey = null,
			int? threadId = null,
			string message = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (stopTime.HasValue == true && stopTime.Value.Kind != DateTimeKind.Utc)
			{
				stopTime = stopTime.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditEvent> query = (from ae in _context.AuditEvents select ae);
			if (startTime.HasValue == true)
			{
				query = query.Where(ae => ae.startTime == startTime.Value);
			}
			if (stopTime.HasValue == true)
			{
				query = query.Where(ae => ae.stopTime == stopTime.Value);
			}
			if (completedSuccessfully.HasValue == true)
			{
				query = query.Where(ae => ae.completedSuccessfully == completedSuccessfully.Value);
			}
			if (auditUserId.HasValue == true)
			{
				query = query.Where(ae => ae.auditUserId == auditUserId.Value);
			}
			if (auditSessionId.HasValue == true)
			{
				query = query.Where(ae => ae.auditSessionId == auditSessionId.Value);
			}
			if (auditTypeId.HasValue == true)
			{
				query = query.Where(ae => ae.auditTypeId == auditTypeId.Value);
			}
			if (auditAccessTypeId.HasValue == true)
			{
				query = query.Where(ae => ae.auditAccessTypeId == auditAccessTypeId.Value);
			}
			if (auditSourceId.HasValue == true)
			{
				query = query.Where(ae => ae.auditSourceId == auditSourceId.Value);
			}
			if (auditUserAgentId.HasValue == true)
			{
				query = query.Where(ae => ae.auditUserAgentId == auditUserAgentId.Value);
			}
			if (auditModuleId.HasValue == true)
			{
				query = query.Where(ae => ae.auditModuleId == auditModuleId.Value);
			}
			if (auditModuleEntityId.HasValue == true)
			{
				query = query.Where(ae => ae.auditModuleEntityId == auditModuleEntityId.Value);
			}
			if (auditResourceId.HasValue == true)
			{
				query = query.Where(ae => ae.auditResourceId == auditResourceId.Value);
			}
			if (auditHostSystemId.HasValue == true)
			{
				query = query.Where(ae => ae.auditHostSystemId == auditHostSystemId.Value);
			}
			if (string.IsNullOrEmpty(primaryKey) == false)
			{
				query = query.Where(ae => ae.primaryKey == primaryKey);
			}
			if (threadId.HasValue == true)
			{
				query = query.Where(ae => ae.threadId == threadId.Value);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(ae => ae.message == message);
			}

			query = query.OrderByDescending(ae => ae.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.auditAccessType);
				query = query.Include(x => x.auditHostSystem);
				query = query.Include(x => x.auditModule);
				query = query.Include(x => x.auditModuleEntity);
				query = query.Include(x => x.auditResource);
				query = query.Include(x => x.auditSession);
				query = query.Include(x => x.auditSource);
				query = query.Include(x => x.auditType);
				query = query.Include(x => x.auditUser);
				query = query.Include(x => x.auditUserAgent);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.primaryKey.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || (includeRelations == true && x.auditAccessType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditAccessType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.auditHostSystem.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditHostSystem.comments.Contains(anyStringContains))
			       || (includeRelations == true && x.auditModule.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditModule.comments.Contains(anyStringContains))
			       || (includeRelations == true && x.auditModuleEntity.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditModuleEntity.comments.Contains(anyStringContains))
			       || (includeRelations == true && x.auditResource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditResource.comments.Contains(anyStringContains))
			       || (includeRelations == true && x.auditSession.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditSession.comments.Contains(anyStringContains))
			       || (includeRelations == true && x.auditSource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditSource.comments.Contains(anyStringContains))
			       || (includeRelations == true && x.auditType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.auditUser.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditUser.comments.Contains(anyStringContains))
			       || (includeRelations == true && x.auditUserAgent.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditUserAgent.comments.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.AuditEvent> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AuditEvent auditEvent in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditEvent, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditEvent Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditEvent Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
		
*/
		
        /// <summary>
        /// 
        /// This returns a row count of AuditEvents filtered by the parameters provided.  Its query is similar to the GetAuditEvents method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEvents/RowCount")]
		public async Task<IActionResult> GetRowCount(
			DateTime? startTime = null,
			DateTime? stopTime = null,
			bool? completedSuccessfully = null,
			int? auditUserId = null,
			int? auditSessionId = null,
			int? auditTypeId = null,
			int? auditAccessTypeId = null,
			int? auditSourceId = null,
			int? auditUserAgentId = null,
			int? auditModuleId = null,
			int? auditModuleEntityId = null,
			int? auditResourceId = null,
			int? auditHostSystemId = null,
			string primaryKey = null,
			int? threadId = null,
			string message = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (stopTime.HasValue == true && stopTime.Value.Kind != DateTimeKind.Utc)
			{
				stopTime = stopTime.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditEvent> query = (from ae in _context.AuditEvents select ae);
			if (startTime.HasValue == true)
			{
				query = query.Where(ae => ae.startTime == startTime.Value);
			}
			if (stopTime.HasValue == true)
			{
				query = query.Where(ae => ae.stopTime == stopTime.Value);
			}
			if (completedSuccessfully.HasValue == true)
			{
				query = query.Where(ae => ae.completedSuccessfully == completedSuccessfully.Value);
			}
			if (auditUserId.HasValue == true)
			{
				query = query.Where(ae => ae.auditUserId == auditUserId.Value);
			}
			if (auditSessionId.HasValue == true)
			{
				query = query.Where(ae => ae.auditSessionId == auditSessionId.Value);
			}
			if (auditTypeId.HasValue == true)
			{
				query = query.Where(ae => ae.auditTypeId == auditTypeId.Value);
			}
			if (auditAccessTypeId.HasValue == true)
			{
				query = query.Where(ae => ae.auditAccessTypeId == auditAccessTypeId.Value);
			}
			if (auditSourceId.HasValue == true)
			{
				query = query.Where(ae => ae.auditSourceId == auditSourceId.Value);
			}
			if (auditUserAgentId.HasValue == true)
			{
				query = query.Where(ae => ae.auditUserAgentId == auditUserAgentId.Value);
			}
			if (auditModuleId.HasValue == true)
			{
				query = query.Where(ae => ae.auditModuleId == auditModuleId.Value);
			}
			if (auditModuleEntityId.HasValue == true)
			{
				query = query.Where(ae => ae.auditModuleEntityId == auditModuleEntityId.Value);
			}
			if (auditResourceId.HasValue == true)
			{
				query = query.Where(ae => ae.auditResourceId == auditResourceId.Value);
			}
			if (auditHostSystemId.HasValue == true)
			{
				query = query.Where(ae => ae.auditHostSystemId == auditHostSystemId.Value);
			}
			if (primaryKey != null)
			{
				query = query.Where(ae => ae.primaryKey == primaryKey);
			}
			if (threadId.HasValue == true)
			{
				query = query.Where(ae => ae.threadId == threadId.Value);
			}
			if (message != null)
			{
				query = query.Where(ae => ae.message == message);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Audit Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.primaryKey.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.auditAccessType.name.Contains(anyStringContains)
			       || x.auditAccessType.description.Contains(anyStringContains)
			       || x.auditHostSystem.name.Contains(anyStringContains)
			       || x.auditHostSystem.comments.Contains(anyStringContains)
			       || x.auditModule.name.Contains(anyStringContains)
			       || x.auditModule.comments.Contains(anyStringContains)
			       || x.auditModuleEntity.name.Contains(anyStringContains)
			       || x.auditModuleEntity.comments.Contains(anyStringContains)
			       || x.auditResource.name.Contains(anyStringContains)
			       || x.auditResource.comments.Contains(anyStringContains)
			       || x.auditSession.name.Contains(anyStringContains)
			       || x.auditSession.comments.Contains(anyStringContains)
			       || x.auditSource.name.Contains(anyStringContains)
			       || x.auditSource.comments.Contains(anyStringContains)
			       || x.auditType.name.Contains(anyStringContains)
			       || x.auditType.description.Contains(anyStringContains)
			       || x.auditUser.name.Contains(anyStringContains)
			       || x.auditUser.comments.Contains(anyStringContains)
			       || x.auditUserAgent.name.Contains(anyStringContains)
			       || x.auditUserAgent.comments.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AuditEvent by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEvent/{id}")]
		public async Task<IActionResult> GetAuditEvent(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.AuditEvent> query = (from ae in _context.AuditEvents where
				(ae.id == id)
					select ae);

				if (includeRelations == true)
				{
					query = query.Include(x => x.auditAccessType);
					query = query.Include(x => x.auditHostSystem);
					query = query.Include(x => x.auditModule);
					query = query.Include(x => x.auditModuleEntity);
					query = query.Include(x => x.auditResource);
					query = query.Include(x => x.auditSession);
					query = query.Include(x => x.auditSource);
					query = query.Include(x => x.auditType);
					query = query.Include(x => x.auditUser);
					query = query.Include(x => x.auditUserAgent);
					query = query.AsSplitQuery();
				}

				Database.AuditEvent materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.AuditEvent Entity was read with Admin privilege." : "Auditor.AuditEvent Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditEvent", materialized.id, materialized.message));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.AuditEvent entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.AuditEvent.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.AuditEvent.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AuditEvent record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AuditEvent/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAuditEvent(int id, [FromBody]Database.AuditEvent.AuditEventDTO auditEventDTO, CancellationToken cancellationToken = default)
		{
			if (auditEventDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != auditEventDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AuditEvent> query = (from x in _context.AuditEvents
				where
				(x.id == id)
				select x);


			Database.AuditEvent existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditEvent PUT", id.ToString(), new Exception("No Auditor.AuditEvent entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AuditEvent cloneOfExisting = (Database.AuditEvent)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AuditEvent object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AuditEvent auditEvent = (Database.AuditEvent)_context.Entry(existing).GetDatabaseValues().ToObject();
			auditEvent.ApplyDTO(auditEventDTO);


			if (auditEvent.startTime.Kind != DateTimeKind.Utc)
			{
				auditEvent.startTime = auditEvent.startTime.ToUniversalTime();
			}

			if (auditEvent.stopTime.Kind != DateTimeKind.Utc)
			{
				auditEvent.stopTime = auditEvent.stopTime.ToUniversalTime();
			}

			if (auditEvent.primaryKey != null && auditEvent.primaryKey.Length > 250)
			{
				auditEvent.primaryKey = auditEvent.primaryKey.Substring(0, 250);
			}

			EntityEntry<Database.AuditEvent> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(auditEvent);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditEvent entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(auditEvent)),
					null);


				return Ok(Database.AuditEvent.CreateAnonymous(auditEvent));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditEvent entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(auditEvent)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AuditEvent record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEvent", Name = "AuditEvent")]
		public async Task<IActionResult> PostAuditEvent([FromBody]Database.AuditEvent.AuditEventDTO auditEventDTO, CancellationToken cancellationToken = default)
		{
			if (auditEventDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new AuditEvent object using the data from the DTO
			//
			Database.AuditEvent auditEvent = Database.AuditEvent.FromDTO(auditEventDTO);

			try
			{
				if (auditEvent.startTime.Kind != DateTimeKind.Utc)
				{
					auditEvent.startTime = auditEvent.startTime.ToUniversalTime();
				}

				if (auditEvent.stopTime.Kind != DateTimeKind.Utc)
				{
					auditEvent.stopTime = auditEvent.stopTime.ToUniversalTime();
				}

				if (auditEvent.primaryKey != null && auditEvent.primaryKey.Length > 250)
				{
					auditEvent.primaryKey = auditEvent.primaryKey.Substring(0, 250);
				}

				_context.AuditEvents.Add(auditEvent);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.AuditEvent entity successfully created.",
					true,
					auditEvent.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(auditEvent)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.AuditEvent entity creation failed.", false, auditEvent.id.ToString(), "", JsonSerializer.Serialize(auditEvent), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditEvent", auditEvent.id, auditEvent.message));

			return CreatedAtRoute("AuditEvent", new { id = auditEvent.id }, Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(auditEvent));
		}



        /// <summary>
        /// 
        /// This deletes a AuditEvent record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEvent/{id}")]
		[Route("api/AuditEvent")]
		public async Task<IActionResult> DeleteAuditEvent(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AuditEvent> query = (from x in _context.AuditEvents
				where
				(x.id == id)
				select x);


			Database.AuditEvent auditEvent = await query.FirstOrDefaultAsync(cancellationToken);

			if (auditEvent == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditEvent DELETE", id.ToString(), new Exception("No Auditor.AuditEvent entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AuditEvent cloneOfExisting = (Database.AuditEvent)_context.Entry(auditEvent).GetDatabaseValues().ToObject();


			try
			{
				_context.AuditEvents.Remove(auditEvent);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditEvent entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(auditEvent)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditEvent entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(auditEvent)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AuditEvent records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AuditEvents/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			DateTime? startTime = null,
			DateTime? stopTime = null,
			bool? completedSuccessfully = null,
			int? auditUserId = null,
			int? auditSessionId = null,
			int? auditTypeId = null,
			int? auditAccessTypeId = null,
			int? auditSourceId = null,
			int? auditUserAgentId = null,
			int? auditModuleId = null,
			int? auditModuleEntityId = null,
			int? auditResourceId = null,
			int? auditHostSystemId = null,
			string primaryKey = null,
			int? threadId = null,
			string message = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (stopTime.HasValue == true && stopTime.Value.Kind != DateTimeKind.Utc)
			{
				stopTime = stopTime.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditEvent> query = (from ae in _context.AuditEvents select ae);
			if (startTime.HasValue == true)
			{
				query = query.Where(ae => ae.startTime == startTime.Value);
			}
			if (stopTime.HasValue == true)
			{
				query = query.Where(ae => ae.stopTime == stopTime.Value);
			}
			if (completedSuccessfully.HasValue == true)
			{
				query = query.Where(ae => ae.completedSuccessfully == completedSuccessfully.Value);
			}
			if (auditUserId.HasValue == true)
			{
				query = query.Where(ae => ae.auditUserId == auditUserId.Value);
			}
			if (auditSessionId.HasValue == true)
			{
				query = query.Where(ae => ae.auditSessionId == auditSessionId.Value);
			}
			if (auditTypeId.HasValue == true)
			{
				query = query.Where(ae => ae.auditTypeId == auditTypeId.Value);
			}
			if (auditAccessTypeId.HasValue == true)
			{
				query = query.Where(ae => ae.auditAccessTypeId == auditAccessTypeId.Value);
			}
			if (auditSourceId.HasValue == true)
			{
				query = query.Where(ae => ae.auditSourceId == auditSourceId.Value);
			}
			if (auditUserAgentId.HasValue == true)
			{
				query = query.Where(ae => ae.auditUserAgentId == auditUserAgentId.Value);
			}
			if (auditModuleId.HasValue == true)
			{
				query = query.Where(ae => ae.auditModuleId == auditModuleId.Value);
			}
			if (auditModuleEntityId.HasValue == true)
			{
				query = query.Where(ae => ae.auditModuleEntityId == auditModuleEntityId.Value);
			}
			if (auditResourceId.HasValue == true)
			{
				query = query.Where(ae => ae.auditResourceId == auditResourceId.Value);
			}
			if (auditHostSystemId.HasValue == true)
			{
				query = query.Where(ae => ae.auditHostSystemId == auditHostSystemId.Value);
			}
			if (string.IsNullOrEmpty(primaryKey) == false)
			{
				query = query.Where(ae => ae.primaryKey == primaryKey);
			}
			if (threadId.HasValue == true)
			{
				query = query.Where(ae => ae.threadId == threadId.Value);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(ae => ae.message == message);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.primaryKey.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.auditAccessType.name.Contains(anyStringContains)
			       || x.auditAccessType.description.Contains(anyStringContains)
			       || x.auditHostSystem.name.Contains(anyStringContains)
			       || x.auditHostSystem.comments.Contains(anyStringContains)
			       || x.auditModule.name.Contains(anyStringContains)
			       || x.auditModule.comments.Contains(anyStringContains)
			       || x.auditModuleEntity.name.Contains(anyStringContains)
			       || x.auditModuleEntity.comments.Contains(anyStringContains)
			       || x.auditResource.name.Contains(anyStringContains)
			       || x.auditResource.comments.Contains(anyStringContains)
			       || x.auditSession.name.Contains(anyStringContains)
			       || x.auditSession.comments.Contains(anyStringContains)
			       || x.auditSource.name.Contains(anyStringContains)
			       || x.auditSource.comments.Contains(anyStringContains)
			       || x.auditType.name.Contains(anyStringContains)
			       || x.auditType.description.Contains(anyStringContains)
			       || x.auditUser.name.Contains(anyStringContains)
			       || x.auditUser.comments.Contains(anyStringContains)
			       || x.auditUserAgent.name.Contains(anyStringContains)
			       || x.auditUserAgent.comments.Contains(anyStringContains)
			   );
			}


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AuditEvent.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AuditEvent/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
