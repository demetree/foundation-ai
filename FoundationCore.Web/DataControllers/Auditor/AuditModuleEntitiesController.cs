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
    /// This auto generated class provides the basic CRUD operations for the AuditModuleEntity entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AuditModuleEntity entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AuditModuleEntitiesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<AuditModuleEntitiesController> _logger;

		public AuditModuleEntitiesController(AuditorContext context, ILogger<AuditModuleEntitiesController> logger) : base("Auditor", "AuditModuleEntity")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AuditModuleEntities filtered by the parameters provided.
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
		[Route("api/AuditModuleEntities")]
		public async Task<IActionResult> GetAuditModuleEntities(
			int? auditModuleId = null,
			string name = null,
			string comments = null,
			DateTime? firstAccess = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (firstAccess.HasValue == true && firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				firstAccess = firstAccess.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditModuleEntity> query = (from ame in _context.AuditModuleEntities select ame);
			if (auditModuleId.HasValue == true)
			{
				query = query.Where(ame => ame.auditModuleId == auditModuleId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ame => ame.name == name);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(ame => ame.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(ame => ame.firstAccess == firstAccess.Value);
			}

			query = query.OrderBy(ame => ame.name).ThenBy(ame => ame.comments);


			//
			// Add the any string contains parameter to span all the string fields on the Audit Module Entity, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			       || (includeRelations == true && x.auditModule.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditModule.comments.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.auditModule);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.AuditModuleEntity> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AuditModuleEntity auditModuleEntity in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditModuleEntity, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditModuleEntity Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditModuleEntity Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AuditModuleEntities filtered by the parameters provided.  Its query is similar to the GetAuditModuleEntities method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditModuleEntities/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? auditModuleId = null,
			string name = null,
			string comments = null,
			DateTime? firstAccess = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (firstAccess.HasValue == true && firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				firstAccess = firstAccess.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditModuleEntity> query = (from ame in _context.AuditModuleEntities select ame);
			if (auditModuleId.HasValue == true)
			{
				query = query.Where(ame => ame.auditModuleId == auditModuleId.Value);
			}
			if (name != null)
			{
				query = query.Where(ame => ame.name == name);
			}
			if (comments != null)
			{
				query = query.Where(ame => ame.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(ame => ame.firstAccess == firstAccess.Value);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Audit Module Entity, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			       || x.auditModule.name.Contains(anyStringContains)
			       || x.auditModule.comments.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AuditModuleEntity by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditModuleEntity/{id}")]
		public async Task<IActionResult> GetAuditModuleEntity(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.AuditModuleEntity> query = (from ame in _context.AuditModuleEntities where
				(ame.id == id)
					select ame);

				if (includeRelations == true)
				{
					query = query.Include(x => x.auditModule);
					query = query.AsSplitQuery();
				}

				Database.AuditModuleEntity materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.AuditModuleEntity Entity was read with Admin privilege." : "Auditor.AuditModuleEntity Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditModuleEntity", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.AuditModuleEntity entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.AuditModuleEntity.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.AuditModuleEntity.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AuditModuleEntity record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AuditModuleEntity/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAuditModuleEntity(int id, [FromBody]Database.AuditModuleEntity.AuditModuleEntityDTO auditModuleEntityDTO, CancellationToken cancellationToken = default)
		{
			if (auditModuleEntityDTO == null)
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



			if (id != auditModuleEntityDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AuditModuleEntity> query = (from x in _context.AuditModuleEntities
				where
				(x.id == id)
				select x);


			Database.AuditModuleEntity existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditModuleEntity PUT", id.ToString(), new Exception("No Auditor.AuditModuleEntity entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AuditModuleEntity cloneOfExisting = (Database.AuditModuleEntity)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AuditModuleEntity object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AuditModuleEntity auditModuleEntity = (Database.AuditModuleEntity)_context.Entry(existing).GetDatabaseValues().ToObject();
			auditModuleEntity.ApplyDTO(auditModuleEntityDTO);


			if (auditModuleEntity.name != null && auditModuleEntity.name.Length > 500)
			{
				auditModuleEntity.name = auditModuleEntity.name.Substring(0, 500);
			}

			if (auditModuleEntity.comments != null && auditModuleEntity.comments.Length > 1000)
			{
				auditModuleEntity.comments = auditModuleEntity.comments.Substring(0, 1000);
			}

			if (auditModuleEntity.firstAccess.HasValue == true && auditModuleEntity.firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				auditModuleEntity.firstAccess = auditModuleEntity.firstAccess.Value.ToUniversalTime();
			}

			EntityEntry<Database.AuditModuleEntity> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(auditModuleEntity);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditModuleEntity entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditModuleEntity.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditModuleEntity.CreateAnonymousWithFirstLevelSubObjects(auditModuleEntity)),
					null);


				return Ok(Database.AuditModuleEntity.CreateAnonymous(auditModuleEntity));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditModuleEntity entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditModuleEntity.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditModuleEntity.CreateAnonymousWithFirstLevelSubObjects(auditModuleEntity)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AuditModuleEntity record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditModuleEntity", Name = "AuditModuleEntity")]
		public async Task<IActionResult> PostAuditModuleEntity([FromBody]Database.AuditModuleEntity.AuditModuleEntityDTO auditModuleEntityDTO, CancellationToken cancellationToken = default)
		{
			if (auditModuleEntityDTO == null)
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
			// Create a new AuditModuleEntity object using the data from the DTO
			//
			Database.AuditModuleEntity auditModuleEntity = Database.AuditModuleEntity.FromDTO(auditModuleEntityDTO);

			try
			{
				if (auditModuleEntity.name != null && auditModuleEntity.name.Length > 500)
				{
					auditModuleEntity.name = auditModuleEntity.name.Substring(0, 500);
				}

				if (auditModuleEntity.comments != null && auditModuleEntity.comments.Length > 1000)
				{
					auditModuleEntity.comments = auditModuleEntity.comments.Substring(0, 1000);
				}

				if (auditModuleEntity.firstAccess.HasValue == true && auditModuleEntity.firstAccess.Value.Kind != DateTimeKind.Utc)
				{
					auditModuleEntity.firstAccess = auditModuleEntity.firstAccess.Value.ToUniversalTime();
				}

				_context.AuditModuleEntities.Add(auditModuleEntity);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.AuditModuleEntity entity successfully created.",
					true,
					auditModuleEntity.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AuditModuleEntity.CreateAnonymousWithFirstLevelSubObjects(auditModuleEntity)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.AuditModuleEntity entity creation failed.", false, auditModuleEntity.id.ToString(), "", JsonSerializer.Serialize(auditModuleEntity), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditModuleEntity", auditModuleEntity.id, auditModuleEntity.name));

			return CreatedAtRoute("AuditModuleEntity", new { id = auditModuleEntity.id }, Database.AuditModuleEntity.CreateAnonymousWithFirstLevelSubObjects(auditModuleEntity));
		}



        /// <summary>
        /// 
        /// This deletes a AuditModuleEntity record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditModuleEntity/{id}")]
		[Route("api/AuditModuleEntity")]
		public async Task<IActionResult> DeleteAuditModuleEntity(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.AuditModuleEntity> query = (from x in _context.AuditModuleEntities
				where
				(x.id == id)
				select x);


			Database.AuditModuleEntity auditModuleEntity = await query.FirstOrDefaultAsync(cancellationToken);

			if (auditModuleEntity == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditModuleEntity DELETE", id.ToString(), new Exception("No Auditor.AuditModuleEntity entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AuditModuleEntity cloneOfExisting = (Database.AuditModuleEntity)_context.Entry(auditModuleEntity).GetDatabaseValues().ToObject();


			try
			{
				_context.AuditModuleEntities.Remove(auditModuleEntity);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditModuleEntity entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditModuleEntity.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditModuleEntity.CreateAnonymousWithFirstLevelSubObjects(auditModuleEntity)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditModuleEntity entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditModuleEntity.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditModuleEntity.CreateAnonymousWithFirstLevelSubObjects(auditModuleEntity)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AuditModuleEntity records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AuditModuleEntities/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? auditModuleId = null,
			string name = null,
			string comments = null,
			DateTime? firstAccess = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (firstAccess.HasValue == true && firstAccess.Value.Kind != DateTimeKind.Utc)
			{
				firstAccess = firstAccess.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditModuleEntity> query = (from ame in _context.AuditModuleEntities select ame);
			if (auditModuleId.HasValue == true)
			{
				query = query.Where(ame => ame.auditModuleId == auditModuleId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ame => ame.name == name);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(ame => ame.comments == comments);
			}
			if (firstAccess.HasValue == true)
			{
				query = query.Where(ame => ame.firstAccess == firstAccess.Value);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit Module Entity, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			       || x.auditModule.name.Contains(anyStringContains)
			       || x.auditModule.comments.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.comments);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AuditModuleEntity.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AuditModuleEntity/CreateAuditEvent")]
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
