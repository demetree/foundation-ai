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
    /// This auto generated class provides the basic CRUD operations for the EntityDataToken entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the EntityDataToken entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class EntityDataTokensController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SecurityContext _context;

		private ILogger<EntityDataTokensController> _logger;

		public EntityDataTokensController(SecurityContext context, ILogger<EntityDataTokensController> logger) : base("Security", "EntityDataToken")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of EntityDataTokens filtered by the parameters provided.
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
		[Route("api/EntityDataTokens")]
		public async Task<IActionResult> GetEntityDataTokens(
			int? securityUserId = null,
			int? moduleId = null,
			string entity = null,
			string sessionId = null,
			string authenticationToken = null,
			string token = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.EntityDataToken> query = (from edt in _context.EntityDataTokens select edt);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(edt => edt.securityUserId == securityUserId.Value);
			}
			if (moduleId.HasValue == true)
			{
				query = query.Where(edt => edt.moduleId == moduleId.Value);
			}
			if (string.IsNullOrEmpty(entity) == false)
			{
				query = query.Where(edt => edt.entity == entity);
			}
			if (string.IsNullOrEmpty(sessionId) == false)
			{
				query = query.Where(edt => edt.sessionId == sessionId);
			}
			if (string.IsNullOrEmpty(authenticationToken) == false)
			{
				query = query.Where(edt => edt.authenticationToken == authenticationToken);
			}
			if (string.IsNullOrEmpty(token) == false)
			{
				query = query.Where(edt => edt.token == token);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(edt => edt.timeStamp == timeStamp.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(edt => edt.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(edt => edt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(edt => edt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(edt => edt.deleted == false);
				}
			}
			else
			{
				query = query.Where(edt => edt.active == true);
				query = query.Where(edt => edt.deleted == false);
			}

			query = query.OrderByDescending(edt => edt.timeStamp);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.module);
				query = query.Include(x => x.securityUser);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Entity Data Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.entity.Contains(anyStringContains)
			       || x.sessionId.Contains(anyStringContains)
			       || x.authenticationToken.Contains(anyStringContains)
			       || x.token.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			       || (includeRelations == true && x.module.name.Contains(anyStringContains))
			       || (includeRelations == true && x.module.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.accountName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.password.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.firstName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.middleName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.lastName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.emailAddress.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.cellPhoneNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.phoneNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.phoneExtension.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.authenticationDomain.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.alternateIdentifier.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.settings.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.authenticationToken.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.twoFactorToken.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.EntityDataToken> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.EntityDataToken entityDataToken in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(entityDataToken, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.EntityDataToken Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.EntityDataToken Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of EntityDataTokens filtered by the parameters provided.  Its query is similar to the GetEntityDataTokens method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EntityDataTokens/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityUserId = null,
			int? moduleId = null,
			string entity = null,
			string sessionId = null,
			string authenticationToken = null,
			string token = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.EntityDataToken> query = (from edt in _context.EntityDataTokens select edt);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(edt => edt.securityUserId == securityUserId.Value);
			}
			if (moduleId.HasValue == true)
			{
				query = query.Where(edt => edt.moduleId == moduleId.Value);
			}
			if (entity != null)
			{
				query = query.Where(edt => edt.entity == entity);
			}
			if (sessionId != null)
			{
				query = query.Where(edt => edt.sessionId == sessionId);
			}
			if (authenticationToken != null)
			{
				query = query.Where(edt => edt.authenticationToken == authenticationToken);
			}
			if (token != null)
			{
				query = query.Where(edt => edt.token == token);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(edt => edt.timeStamp == timeStamp.Value);
			}
			if (comments != null)
			{
				query = query.Where(edt => edt.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(edt => edt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(edt => edt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(edt => edt.deleted == false);
				}
			}
			else
			{
				query = query.Where(edt => edt.active == true);
				query = query.Where(edt => edt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Entity Data Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.entity.Contains(anyStringContains)
			       || x.sessionId.Contains(anyStringContains)
			       || x.authenticationToken.Contains(anyStringContains)
			       || x.token.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			       || x.module.name.Contains(anyStringContains)
			       || x.module.description.Contains(anyStringContains)
			       || x.securityUser.accountName.Contains(anyStringContains)
			       || x.securityUser.password.Contains(anyStringContains)
			       || x.securityUser.firstName.Contains(anyStringContains)
			       || x.securityUser.middleName.Contains(anyStringContains)
			       || x.securityUser.lastName.Contains(anyStringContains)
			       || x.securityUser.emailAddress.Contains(anyStringContains)
			       || x.securityUser.cellPhoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneExtension.Contains(anyStringContains)
			       || x.securityUser.description.Contains(anyStringContains)
			       || x.securityUser.authenticationDomain.Contains(anyStringContains)
			       || x.securityUser.alternateIdentifier.Contains(anyStringContains)
			       || x.securityUser.settings.Contains(anyStringContains)
			       || x.securityUser.authenticationToken.Contains(anyStringContains)
			       || x.securityUser.twoFactorToken.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single EntityDataToken by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EntityDataToken/{id}")]
		public async Task<IActionResult> GetEntityDataToken(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.EntityDataToken> query = (from edt in _context.EntityDataTokens where
							(edt.id == id) &&
							(userIsAdmin == true || edt.deleted == false) &&
							(userIsWriter == true || edt.active == true)
					select edt);

				if (includeRelations == true)
				{
					query = query.Include(x => x.module);
					query = query.Include(x => x.securityUser);
					query = query.AsSplitQuery();
				}

				Database.EntityDataToken materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.EntityDataToken Entity was read with Admin privilege." : "Security.EntityDataToken Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EntityDataToken", materialized.id, materialized.token));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.EntityDataToken entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.EntityDataToken.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.EntityDataToken.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing EntityDataToken record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/EntityDataToken/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutEntityDataToken(int id, [FromBody]Database.EntityDataToken.EntityDataTokenDTO entityDataTokenDTO, CancellationToken cancellationToken = default)
		{
			if (entityDataTokenDTO == null)
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



			if (id != entityDataTokenDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.EntityDataToken> query = (from x in _context.EntityDataTokens
				where
				(x.id == id)
				select x);


			Database.EntityDataToken existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.EntityDataToken PUT", id.ToString(), new Exception("No Security.EntityDataToken entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.EntityDataToken cloneOfExisting = (Database.EntityDataToken)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new EntityDataToken object using the data from the existing record, updated with what is in the DTO.
			//
			Database.EntityDataToken entityDataToken = (Database.EntityDataToken)_context.Entry(existing).GetDatabaseValues().ToObject();
			entityDataToken.ApplyDTO(entityDataTokenDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (entityDataToken.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.EntityDataToken record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (entityDataToken.entity != null && entityDataToken.entity.Length > 250)
			{
				entityDataToken.entity = entityDataToken.entity.Substring(0, 250);
			}

			if (entityDataToken.sessionId != null && entityDataToken.sessionId.Length > 50)
			{
				entityDataToken.sessionId = entityDataToken.sessionId.Substring(0, 50);
			}

			if (entityDataToken.authenticationToken != null && entityDataToken.authenticationToken.Length > 50)
			{
				entityDataToken.authenticationToken = entityDataToken.authenticationToken.Substring(0, 50);
			}

			if (entityDataToken.token != null && entityDataToken.token.Length > 50)
			{
				entityDataToken.token = entityDataToken.token.Substring(0, 50);
			}

			if (entityDataToken.timeStamp.Kind != DateTimeKind.Utc)
			{
				entityDataToken.timeStamp = entityDataToken.timeStamp.ToUniversalTime();
			}

			if (entityDataToken.comments != null && entityDataToken.comments.Length > 1000)
			{
				entityDataToken.comments = entityDataToken.comments.Substring(0, 1000);
			}

			EntityEntry<Database.EntityDataToken> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(entityDataToken);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.EntityDataToken entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.EntityDataToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EntityDataToken.CreateAnonymousWithFirstLevelSubObjects(entityDataToken)),
					null);


				return Ok(Database.EntityDataToken.CreateAnonymous(entityDataToken));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.EntityDataToken entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.EntityDataToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EntityDataToken.CreateAnonymousWithFirstLevelSubObjects(entityDataToken)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new EntityDataToken record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EntityDataToken", Name = "EntityDataToken")]
		public async Task<IActionResult> PostEntityDataToken([FromBody]Database.EntityDataToken.EntityDataTokenDTO entityDataTokenDTO, CancellationToken cancellationToken = default)
		{
			if (entityDataTokenDTO == null)
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
			// Create a new EntityDataToken object using the data from the DTO
			//
			Database.EntityDataToken entityDataToken = Database.EntityDataToken.FromDTO(entityDataTokenDTO);

			try
			{
				if (entityDataToken.entity != null && entityDataToken.entity.Length > 250)
				{
					entityDataToken.entity = entityDataToken.entity.Substring(0, 250);
				}

				if (entityDataToken.sessionId != null && entityDataToken.sessionId.Length > 50)
				{
					entityDataToken.sessionId = entityDataToken.sessionId.Substring(0, 50);
				}

				if (entityDataToken.authenticationToken != null && entityDataToken.authenticationToken.Length > 50)
				{
					entityDataToken.authenticationToken = entityDataToken.authenticationToken.Substring(0, 50);
				}

				if (entityDataToken.token != null && entityDataToken.token.Length > 50)
				{
					entityDataToken.token = entityDataToken.token.Substring(0, 50);
				}

				if (entityDataToken.timeStamp.Kind != DateTimeKind.Utc)
				{
					entityDataToken.timeStamp = entityDataToken.timeStamp.ToUniversalTime();
				}

				if (entityDataToken.comments != null && entityDataToken.comments.Length > 1000)
				{
					entityDataToken.comments = entityDataToken.comments.Substring(0, 1000);
				}

				_context.EntityDataTokens.Add(entityDataToken);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.EntityDataToken entity successfully created.",
					true,
					entityDataToken.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.EntityDataToken.CreateAnonymousWithFirstLevelSubObjects(entityDataToken)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.EntityDataToken entity creation failed.", false, entityDataToken.id.ToString(), "", JsonSerializer.Serialize(entityDataToken), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "EntityDataToken", entityDataToken.id, entityDataToken.token));

			return CreatedAtRoute("EntityDataToken", new { id = entityDataToken.id }, Database.EntityDataToken.CreateAnonymousWithFirstLevelSubObjects(entityDataToken));
		}



        /// <summary>
        /// 
        /// This deletes a EntityDataToken record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/EntityDataToken/{id}")]
		[Route("api/EntityDataToken")]
		public async Task<IActionResult> DeleteEntityDataToken(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.EntityDataToken> query = (from x in _context.EntityDataTokens
				where
				(x.id == id)
				select x);


			Database.EntityDataToken entityDataToken = await query.FirstOrDefaultAsync(cancellationToken);

			if (entityDataToken == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.EntityDataToken DELETE", id.ToString(), new Exception("No Security.EntityDataToken entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.EntityDataToken cloneOfExisting = (Database.EntityDataToken)_context.Entry(entityDataToken).GetDatabaseValues().ToObject();


			try
			{
				entityDataToken.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.EntityDataToken entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.EntityDataToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EntityDataToken.CreateAnonymousWithFirstLevelSubObjects(entityDataToken)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.EntityDataToken entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.EntityDataToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.EntityDataToken.CreateAnonymousWithFirstLevelSubObjects(entityDataToken)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of EntityDataToken records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/EntityDataTokens/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityUserId = null,
			int? moduleId = null,
			string entity = null,
			string sessionId = null,
			string authenticationToken = null,
			string token = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.EntityDataToken> query = (from edt in _context.EntityDataTokens select edt);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(edt => edt.securityUserId == securityUserId.Value);
			}
			if (moduleId.HasValue == true)
			{
				query = query.Where(edt => edt.moduleId == moduleId.Value);
			}
			if (string.IsNullOrEmpty(entity) == false)
			{
				query = query.Where(edt => edt.entity == entity);
			}
			if (string.IsNullOrEmpty(sessionId) == false)
			{
				query = query.Where(edt => edt.sessionId == sessionId);
			}
			if (string.IsNullOrEmpty(authenticationToken) == false)
			{
				query = query.Where(edt => edt.authenticationToken == authenticationToken);
			}
			if (string.IsNullOrEmpty(token) == false)
			{
				query = query.Where(edt => edt.token == token);
			}
			if (timeStamp.HasValue == true)
			{
				query = query.Where(edt => edt.timeStamp == timeStamp.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(edt => edt.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(edt => edt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(edt => edt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(edt => edt.deleted == false);
				}
			}
			else
			{
				query = query.Where(edt => edt.active == true);
				query = query.Where(edt => edt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Entity Data Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.entity.Contains(anyStringContains)
			       || x.sessionId.Contains(anyStringContains)
			       || x.authenticationToken.Contains(anyStringContains)
			       || x.token.Contains(anyStringContains)
			       || x.comments.Contains(anyStringContains)
			       || x.module.name.Contains(anyStringContains)
			       || x.module.description.Contains(anyStringContains)
			       || x.securityUser.accountName.Contains(anyStringContains)
			       || x.securityUser.password.Contains(anyStringContains)
			       || x.securityUser.firstName.Contains(anyStringContains)
			       || x.securityUser.middleName.Contains(anyStringContains)
			       || x.securityUser.lastName.Contains(anyStringContains)
			       || x.securityUser.emailAddress.Contains(anyStringContains)
			       || x.securityUser.cellPhoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneExtension.Contains(anyStringContains)
			       || x.securityUser.description.Contains(anyStringContains)
			       || x.securityUser.authenticationDomain.Contains(anyStringContains)
			       || x.securityUser.alternateIdentifier.Contains(anyStringContains)
			       || x.securityUser.settings.Contains(anyStringContains)
			       || x.securityUser.authenticationToken.Contains(anyStringContains)
			       || x.securityUser.twoFactorToken.Contains(anyStringContains)
			   );
			}


			query = query.OrderByDescending (x => x.timeStamp);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.EntityDataToken.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/EntityDataToken/CreateAuditEvent")]
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
