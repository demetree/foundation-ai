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
    /// This auto generated class provides the basic CRUD operations for the AuditEventEntityState entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AuditEventEntityState entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AuditEventEntityStatesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<AuditEventEntityStatesController> _logger;

		public AuditEventEntityStatesController(AuditorContext context, ILogger<AuditEventEntityStatesController> logger) : base("Auditor", "AuditEventEntityState")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This gets a list of AuditEventEntityStates filtered by the parameters provided.
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
		[Route("api/AuditEventEntityStates")]
		public async Task<IActionResult> GetAuditEventEntityStates(
			int? auditEventId = null,
			string beforeState = null,
			string afterState = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
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

			IQueryable<Database.AuditEventEntityState> query = (from aees in _context.AuditEventEntityStates select aees);
			if (auditEventId.HasValue == true)
			{
				query = query.Where(aees => aees.auditEventId == auditEventId.Value);
			}
			if (string.IsNullOrEmpty(beforeState) == false)
			{
				query = query.Where(aees => aees.beforeState == beforeState);
			}
			if (string.IsNullOrEmpty(afterState) == false)
			{
				query = query.Where(aees => aees.afterState == afterState);
			}

			query = query.OrderBy(aees => aees.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.auditEvent);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit Event Entity State, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.beforeState.Contains(anyStringContains)
			       || x.afterState.Contains(anyStringContains)
			       || (includeRelations == true && x.auditEvent.primaryKey.Contains(anyStringContains))
			       || (includeRelations == true && x.auditEvent.message.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.AuditEventEntityState> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AuditEventEntityState auditEventEntityState in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditEventEntityState, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditEventEntityState Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditEventEntityState Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AuditEventEntityStates filtered by the parameters provided.  Its query is similar to the GetAuditEventEntityStates method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEventEntityStates/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? auditEventId = null,
			string beforeState = null,
			string afterState = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.AuditEventEntityState> query = (from aees in _context.AuditEventEntityStates select aees);
			if (auditEventId.HasValue == true)
			{
				query = query.Where(aees => aees.auditEventId == auditEventId.Value);
			}
			if (beforeState != null)
			{
				query = query.Where(aees => aees.beforeState == beforeState);
			}
			if (afterState != null)
			{
				query = query.Where(aees => aees.afterState == afterState);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Audit Event Entity State, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.beforeState.Contains(anyStringContains)
			       || x.afterState.Contains(anyStringContains)
			       || x.auditEvent.primaryKey.Contains(anyStringContains)
			       || x.auditEvent.message.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AuditEventEntityState by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEventEntityState/{id}")]
		public async Task<IActionResult> GetAuditEventEntityState(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.AuditEventEntityState> query = (from aees in _context.AuditEventEntityStates where
				(aees.id == id)
					select aees);

				if (includeRelations == true)
				{
					query = query.Include(x => x.auditEvent);
					query = query.AsSplitQuery();
				}

				Database.AuditEventEntityState materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.AuditEventEntityState Entity was read with Admin privilege." : "Auditor.AuditEventEntityState Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditEventEntityState", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.AuditEventEntityState entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.AuditEventEntityState.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.AuditEventEntityState.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AuditEventEntityState record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AuditEventEntityState/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAuditEventEntityState(int id, [FromBody]Database.AuditEventEntityState.AuditEventEntityStateDTO auditEventEntityStateDTO, CancellationToken cancellationToken = default)
		{
			if (auditEventEntityStateDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != auditEventEntityStateDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AuditEventEntityState> query = (from x in _context.AuditEventEntityStates
				where
				(x.id == id)
				select x);


			Database.AuditEventEntityState existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditEventEntityState PUT", id.ToString(), new Exception("No Auditor.AuditEventEntityState entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AuditEventEntityState cloneOfExisting = (Database.AuditEventEntityState)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AuditEventEntityState object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AuditEventEntityState auditEventEntityState = (Database.AuditEventEntityState)_context.Entry(existing).GetDatabaseValues().ToObject();
			auditEventEntityState.ApplyDTO(auditEventEntityStateDTO);


			EntityEntry<Database.AuditEventEntityState> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(auditEventEntityState);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditEventEntityState entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(auditEventEntityState)),
					null);


				return Ok(Database.AuditEventEntityState.CreateAnonymous(auditEventEntityState));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditEventEntityState entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(auditEventEntityState)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AuditEventEntityState record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEventEntityState", Name = "AuditEventEntityState")]
		public async Task<IActionResult> PostAuditEventEntityState([FromBody]Database.AuditEventEntityState.AuditEventEntityStateDTO auditEventEntityStateDTO, CancellationToken cancellationToken = default)
		{
			if (auditEventEntityStateDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new AuditEventEntityState object using the data from the DTO
			//
			Database.AuditEventEntityState auditEventEntityState = Database.AuditEventEntityState.FromDTO(auditEventEntityStateDTO);

			try
			{
				_context.AuditEventEntityStates.Add(auditEventEntityState);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.AuditEventEntityState entity successfully created.",
					true,
					auditEventEntityState.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(auditEventEntityState)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.AuditEventEntityState entity creation failed.", false, auditEventEntityState.id.ToString(), "", JsonSerializer.Serialize(auditEventEntityState), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditEventEntityState", auditEventEntityState.id, auditEventEntityState.id.ToString()));

			return CreatedAtRoute("AuditEventEntityState", new { id = auditEventEntityState.id }, Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(auditEventEntityState));
		}



        /// <summary>
        /// 
        /// This deletes a AuditEventEntityState record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEventEntityState/{id}")]
		[Route("api/AuditEventEntityState")]
		public async Task<IActionResult> DeleteAuditEventEntityState(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AuditEventEntityState> query = (from x in _context.AuditEventEntityStates
				where
				(x.id == id)
				select x);


			Database.AuditEventEntityState auditEventEntityState = await query.FirstOrDefaultAsync(cancellationToken);

			if (auditEventEntityState == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditEventEntityState DELETE", id.ToString(), new Exception("No Auditor.AuditEventEntityState entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AuditEventEntityState cloneOfExisting = (Database.AuditEventEntityState)_context.Entry(auditEventEntityState).GetDatabaseValues().ToObject();


			try
			{
				_context.AuditEventEntityStates.Remove(auditEventEntityState);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditEventEntityState entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(auditEventEntityState)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditEventEntityState entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(auditEventEntityState)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AuditEventEntityState records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AuditEventEntityStates/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? auditEventId = null,
			string beforeState = null,
			string afterState = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);


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

			IQueryable<Database.AuditEventEntityState> query = (from aees in _context.AuditEventEntityStates select aees);
			if (auditEventId.HasValue == true)
			{
				query = query.Where(aees => aees.auditEventId == auditEventId.Value);
			}
			if (string.IsNullOrEmpty(beforeState) == false)
			{
				query = query.Where(aees => aees.beforeState == beforeState);
			}
			if (string.IsNullOrEmpty(afterState) == false)
			{
				query = query.Where(aees => aees.afterState == afterState);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit Event Entity State, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.beforeState.Contains(anyStringContains)
			       || x.afterState.Contains(anyStringContains)
			       || x.auditEvent.primaryKey.Contains(anyStringContains)
			       || x.auditEvent.message.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AuditEventEntityState.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AuditEventEntityState/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
