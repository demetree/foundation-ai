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
using static Foundation.Auditor.AuditEngine;
using Foundation.Security.Database;

namespace Foundation.Security.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the EntityDataTokenEvent entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the EntityDataTokenEvent entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class EntityDataTokenEventsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SecurityContext _context;

		private ILogger<EntityDataTokenEventsController> _logger;

		public EntityDataTokenEventsController(SecurityContext context, ILogger<EntityDataTokenEventsController> logger) : base("Security", "EntityDataTokenEvent")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of EntityDataTokenEvents filtered by the parameters provided.
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
		[Route("api/EntityDataTokenEvents")]
		public async Task<IActionResult> GetEntityDataTokenEvents(
			int? entityDataTokenId = null,
			int? entityDataTokenEventTypeId = null,
			DateTime? timeStamp = null,
			string comments = null,
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
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.EntityDataTokenEvent> query = (from edte in _context.EntityDataTokenEvents select edte);
			if (entityDataTokenId.HasValue == true)
			{
				query = query.Where(edte => edte.entityDataTokenId == entityDataTokenId.Value);
			}
			if (entityDataTokenEventTypeId.HasValue == true)
			{
				query = query.Where(edte => edte.entityDataTokenEventTypeId == entityDataTokenEventTypeId.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(edte => edte.timeStamp == timeStamp.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(edte => edte.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(edte => edte.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(edte => edte.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(edte => edte.deleted == false);
				}
			}
			else
			{
				query = query.Where(edte => edte.active == true);
				query = query.Where(edte => edte.deleted == false);
			}

			query = query.OrderByDescending(edte => edte.timeStamp);


			//
			// Add the any string contains parameter to span all the string fields on the Entity Data Token Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || (includeRelations == true && x.entityDataToken.entity.Contains(anyStringContains))
			       || (includeRelations == true && x.entityDataToken.sessionId.Contains(anyStringContains))
			       || (includeRelations == true && x.entityDataToken.authenticationToken.Contains(anyStringContains))
			       || (includeRelations == true && x.entityDataToken.token.Contains(anyStringContains))
			       || (includeRelations == true && x.entityDataToken.comments.Contains(anyStringContains))
			       || (includeRelations == true && x.entityDataTokenEventType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.entityDataTokenEventType.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.entityDataToken);
				query = query.Include(x => x.entityDataTokenEventType);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.EntityDataTokenEvent> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.EntityDataTokenEvent entityDataTokenEvent in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(entityDataTokenEvent, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.EntityDataTokenEvent Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.EntityDataTokenEvent Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of EntityDataTokenEvents filtered by the parameters provided.  Its query is similar to the GetEntityDataTokenEvents method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EntityDataTokenEvents/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? entityDataTokenId = null,
			int? entityDataTokenEventTypeId = null,
			DateTime? timeStamp = null,
			string comments = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.EntityDataTokenEvent> query = (from edte in _context.EntityDataTokenEvents select edte);
			if (entityDataTokenId.HasValue == true)
			{
				query = query.Where(edte => edte.entityDataTokenId == entityDataTokenId.Value);
			}
			if (entityDataTokenEventTypeId.HasValue == true)
			{
				query = query.Where(edte => edte.entityDataTokenEventTypeId == entityDataTokenEventTypeId.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(edte => edte.timeStamp == timeStamp.Value);
			}
			if (comments != null)
			{
				query = query.Where(edte => edte.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(edte => edte.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(edte => edte.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(edte => edte.deleted == false);
				}
			}
			else
			{
				query = query.Where(edte => edte.active == true);
				query = query.Where(edte => edte.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Entity Data Token Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || x.entityDataToken.entity.Contains(anyStringContains)
			       || x.entityDataToken.sessionId.Contains(anyStringContains)
			       || x.entityDataToken.authenticationToken.Contains(anyStringContains)
			       || x.entityDataToken.token.Contains(anyStringContains)
			       || x.entityDataToken.comments.Contains(anyStringContains)
			       || x.entityDataTokenEventType.name.Contains(anyStringContains)
			       || x.entityDataTokenEventType.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single EntityDataTokenEvent by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EntityDataTokenEvent/{id}")]
		public async Task<IActionResult> GetEntityDataTokenEvent(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.EntityDataTokenEvent> query = (from edte in _context.EntityDataTokenEvents where
							(edte.id == id) &&
							(userIsAdmin == true || edte.deleted == false) &&
							(userIsWriter == true || edte.active == true)
					select edte);

				if (includeRelations == true)
				{
					query = query.Include(x => x.entityDataToken);
					query = query.Include(x => x.entityDataTokenEventType);
					query = query.AsSplitQuery();
				}

				Database.EntityDataTokenEvent materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.EntityDataTokenEvent Entity was read with Admin privilege." : "Security.EntityDataTokenEvent Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EntityDataTokenEvent", materialized.id, materialized.comments));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.EntityDataTokenEvent entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.EntityDataTokenEvent.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.EntityDataTokenEvent.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing EntityDataTokenEvent record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/EntityDataTokenEvent/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutEntityDataTokenEvent(int id, [FromBody]Database.EntityDataTokenEvent.EntityDataTokenEventDTO entityDataTokenEventDTO, CancellationToken cancellationToken = default)
		{
			if (entityDataTokenEventDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != entityDataTokenEventDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.EntityDataTokenEvent> query = (from x in _context.EntityDataTokenEvents
				where
				(x.id == id)
				select x);


			Database.EntityDataTokenEvent existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.EntityDataTokenEvent PUT", id.ToString(), new Exception("No Security.EntityDataTokenEvent entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.EntityDataTokenEvent cloneOfExisting = (Database.EntityDataTokenEvent)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new EntityDataTokenEvent object using the data from the existing record, updated with what is in the DTO.
			//
			Database.EntityDataTokenEvent entityDataTokenEvent = (Database.EntityDataTokenEvent)_context.Entry(existing).GetDatabaseValues().ToObject();
			entityDataTokenEvent.ApplyDTO(entityDataTokenEventDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (entityDataTokenEvent.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.EntityDataTokenEvent record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (entityDataTokenEvent.timeStamp.Kind != DateTimeKind.Utc)
			{
				entityDataTokenEvent.timeStamp = entityDataTokenEvent.timeStamp.ToUniversalTime();
			}

			if (entityDataTokenEvent.comments != null && entityDataTokenEvent.comments.Length > 1000)
			{
				entityDataTokenEvent.comments = entityDataTokenEvent.comments.Substring(0, 1000);
			}

			EntityEntry<Database.EntityDataTokenEvent> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(entityDataTokenEvent);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.EntityDataTokenEvent entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.EntityDataTokenEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EntityDataTokenEvent.CreateAnonymousWithFirstLevelSubObjects(entityDataTokenEvent)),
					null);


				return Ok(Database.EntityDataTokenEvent.CreateAnonymous(entityDataTokenEvent));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.EntityDataTokenEvent entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.EntityDataTokenEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EntityDataTokenEvent.CreateAnonymousWithFirstLevelSubObjects(entityDataTokenEvent)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new EntityDataTokenEvent record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EntityDataTokenEvent", Name = "EntityDataTokenEvent")]
		public async Task<IActionResult> PostEntityDataTokenEvent([FromBody]Database.EntityDataTokenEvent.EntityDataTokenEventDTO entityDataTokenEventDTO, CancellationToken cancellationToken = default)
		{
			if (entityDataTokenEventDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new EntityDataTokenEvent object using the data from the DTO
			//
			Database.EntityDataTokenEvent entityDataTokenEvent = Database.EntityDataTokenEvent.FromDTO(entityDataTokenEventDTO);

			try
			{
				if (entityDataTokenEvent.timeStamp.Kind != DateTimeKind.Utc)
				{
					entityDataTokenEvent.timeStamp = entityDataTokenEvent.timeStamp.ToUniversalTime();
				}

				if (entityDataTokenEvent.comments != null && entityDataTokenEvent.comments.Length > 1000)
				{
					entityDataTokenEvent.comments = entityDataTokenEvent.comments.Substring(0, 1000);
				}

				_context.EntityDataTokenEvents.Add(entityDataTokenEvent);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.EntityDataTokenEvent entity successfully created.",
					true,
					entityDataTokenEvent.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.EntityDataTokenEvent.CreateAnonymousWithFirstLevelSubObjects(entityDataTokenEvent)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.EntityDataTokenEvent entity creation failed.", false, entityDataTokenEvent.id.ToString(), "", JsonSerializer.Serialize(entityDataTokenEvent), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EntityDataTokenEvent", entityDataTokenEvent.id, entityDataTokenEvent.comments));

			return CreatedAtRoute("EntityDataTokenEvent", new { id = entityDataTokenEvent.id }, Database.EntityDataTokenEvent.CreateAnonymousWithFirstLevelSubObjects(entityDataTokenEvent));
		}



        /// <summary>
        /// 
        /// This deletes a EntityDataTokenEvent record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EntityDataTokenEvent/{id}")]
		[Route("api/EntityDataTokenEvent")]
		public async Task<IActionResult> DeleteEntityDataTokenEvent(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.EntityDataTokenEvent> query = (from x in _context.EntityDataTokenEvents
				where
				(x.id == id)
				select x);


			Database.EntityDataTokenEvent entityDataTokenEvent = await query.FirstOrDefaultAsync(cancellationToken);

			if (entityDataTokenEvent == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.EntityDataTokenEvent DELETE", id.ToString(), new Exception("No Security.EntityDataTokenEvent entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.EntityDataTokenEvent cloneOfExisting = (Database.EntityDataTokenEvent)_context.Entry(entityDataTokenEvent).GetDatabaseValues().ToObject();


			try
			{
				entityDataTokenEvent.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.EntityDataTokenEvent entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.EntityDataTokenEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EntityDataTokenEvent.CreateAnonymousWithFirstLevelSubObjects(entityDataTokenEvent)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.EntityDataTokenEvent entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.EntityDataTokenEvent.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EntityDataTokenEvent.CreateAnonymousWithFirstLevelSubObjects(entityDataTokenEvent)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of EntityDataTokenEvent records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/EntityDataTokenEvents/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? entityDataTokenId = null,
			int? entityDataTokenEventTypeId = null,
			DateTime? timeStamp = null,
			string comments = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Security Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.EntityDataTokenEvent> query = (from edte in _context.EntityDataTokenEvents select edte);
			if (entityDataTokenId.HasValue == true)
			{
				query = query.Where(edte => edte.entityDataTokenId == entityDataTokenId.Value);
			}
			if (entityDataTokenEventTypeId.HasValue == true)
			{
				query = query.Where(edte => edte.entityDataTokenEventTypeId == entityDataTokenEventTypeId.Value);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(edte => edte.timeStamp == timeStamp.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(edte => edte.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(edte => edte.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(edte => edte.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(edte => edte.deleted == false);
				}
			}
			else
			{
				query = query.Where(edte => edte.active == true);
				query = query.Where(edte => edte.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Entity Data Token Event, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || x.entityDataToken.entity.Contains(anyStringContains)
			       || x.entityDataToken.sessionId.Contains(anyStringContains)
			       || x.entityDataToken.authenticationToken.Contains(anyStringContains)
			       || x.entityDataToken.token.Contains(anyStringContains)
			       || x.entityDataToken.comments.Contains(anyStringContains)
			       || x.entityDataTokenEventType.name.Contains(anyStringContains)
			       || x.entityDataTokenEventType.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderByDescending (x => x.timeStamp);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.EntityDataTokenEvent.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/EntityDataTokenEvent/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Security Writer role needed to write to this table, as well as the minimum write permission level.
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
