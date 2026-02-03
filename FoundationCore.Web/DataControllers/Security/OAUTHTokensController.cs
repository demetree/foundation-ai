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
    /// This auto generated class provides the basic CRUD operations for the OAUTHToken entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the OAUTHToken entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class OAUTHTokensController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SecurityContext _context;

		private ILogger<OAUTHTokensController> _logger;

		public OAUTHTokensController(SecurityContext context, ILogger<OAUTHTokensController> logger) : base("Security", "OAUTHToken")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of OAUTHTokens filtered by the parameters provided.
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
		[Route("api/OAUTHTokens")]
		public async Task<IActionResult> GetOAUTHTokens(
			string token = null,
			DateTime? expiryDateTime = null,
			string userData = null,
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
			if (expiryDateTime.HasValue == true && expiryDateTime.Value.Kind != DateTimeKind.Utc)
			{
				expiryDateTime = expiryDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.OAUTHToken> query = (from oautht in _context.OAUTHTokens select oautht);
			if (string.IsNullOrEmpty(token) == false)
			{
				query = query.Where(oautht => oautht.token == token);
			}
			if (expiryDateTime.HasValue == true)
			{
				query = query.Where(oautht => oautht.expiryDateTime == expiryDateTime.Value);
			}
			if (string.IsNullOrEmpty(userData) == false)
			{
				query = query.Where(oautht => oautht.userData == userData);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(oautht => oautht.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(oautht => oautht.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(oautht => oautht.deleted == false);
				}
			}
			else
			{
				query = query.Where(oautht => oautht.active == true);
				query = query.Where(oautht => oautht.deleted == false);
			}

			query = query.OrderBy(oautht => oautht.token).ThenBy(oautht => oautht.userData);

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
			// Add the any string contains parameter to span all the string fields on the O A U T H Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.token.Contains(anyStringContains)
			       || x.userData.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.OAUTHToken> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.OAUTHToken oAUTHToken in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(oAUTHToken, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.OAUTHToken Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.OAUTHToken Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of OAUTHTokens filtered by the parameters provided.  Its query is similar to the GetOAUTHTokens method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OAUTHTokens/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string token = null,
			DateTime? expiryDateTime = null,
			string userData = null,
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
			if (expiryDateTime.HasValue == true && expiryDateTime.Value.Kind != DateTimeKind.Utc)
			{
				expiryDateTime = expiryDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.OAUTHToken> query = (from oautht in _context.OAUTHTokens select oautht);
			if (token != null)
			{
				query = query.Where(oautht => oautht.token == token);
			}
			if (expiryDateTime.HasValue == true)
			{
				query = query.Where(oautht => oautht.expiryDateTime == expiryDateTime.Value);
			}
			if (userData != null)
			{
				query = query.Where(oautht => oautht.userData == userData);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(oautht => oautht.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(oautht => oautht.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(oautht => oautht.deleted == false);
				}
			}
			else
			{
				query = query.Where(oautht => oautht.active == true);
				query = query.Where(oautht => oautht.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the O A U T H Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.token.Contains(anyStringContains)
			       || x.userData.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single OAUTHToken by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OAUTHToken/{id}")]
		public async Task<IActionResult> GetOAUTHToken(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.OAUTHToken> query = (from oautht in _context.OAUTHTokens where
							(oautht.id == id) &&
							(userIsAdmin == true || oautht.deleted == false) &&
							(userIsWriter == true || oautht.active == true)
					select oautht);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.OAUTHToken materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.OAUTHToken Entity was read with Admin privilege." : "Security.OAUTHToken Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "OAUTHToken", materialized.id, materialized.token));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.OAUTHToken entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.OAUTHToken.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.OAUTHToken.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing OAUTHToken record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/OAUTHToken/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutOAUTHToken(int id, [FromBody]Database.OAUTHToken.OAUTHTokenDTO oAUTHTokenDTO, CancellationToken cancellationToken = default)
		{
			if (oAUTHTokenDTO == null)
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



			if (id != oAUTHTokenDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.OAUTHToken> query = (from x in _context.OAUTHTokens
				where
				(x.id == id)
				select x);


			Database.OAUTHToken existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.OAUTHToken PUT", id.ToString(), new Exception("No Security.OAUTHToken entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.OAUTHToken cloneOfExisting = (Database.OAUTHToken)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new OAUTHToken object using the data from the existing record, updated with what is in the DTO.
			//
			Database.OAUTHToken oAUTHToken = (Database.OAUTHToken)_context.Entry(existing).GetDatabaseValues().ToObject();
			oAUTHToken.ApplyDTO(oAUTHTokenDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (oAUTHToken.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.OAUTHToken record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (oAUTHToken.token != null && oAUTHToken.token.Length > 250)
			{
				oAUTHToken.token = oAUTHToken.token.Substring(0, 250);
			}

			if (oAUTHToken.expiryDateTime.Kind != DateTimeKind.Utc)
			{
				oAUTHToken.expiryDateTime = oAUTHToken.expiryDateTime.ToUniversalTime();
			}

			if (oAUTHToken.userData != null && oAUTHToken.userData.Length > 1000)
			{
				oAUTHToken.userData = oAUTHToken.userData.Substring(0, 1000);
			}

			EntityEntry<Database.OAUTHToken> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(oAUTHToken);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.OAUTHToken entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.OAUTHToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.OAUTHToken.CreateAnonymousWithFirstLevelSubObjects(oAUTHToken)),
					null);


				return Ok(Database.OAUTHToken.CreateAnonymous(oAUTHToken));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.OAUTHToken entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.OAUTHToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.OAUTHToken.CreateAnonymousWithFirstLevelSubObjects(oAUTHToken)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new OAUTHToken record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OAUTHToken", Name = "OAUTHToken")]
		public async Task<IActionResult> PostOAUTHToken([FromBody]Database.OAUTHToken.OAUTHTokenDTO oAUTHTokenDTO, CancellationToken cancellationToken = default)
		{
			if (oAUTHTokenDTO == null)
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
			// Create a new OAUTHToken object using the data from the DTO
			//
			Database.OAUTHToken oAUTHToken = Database.OAUTHToken.FromDTO(oAUTHTokenDTO);

			try
			{
				if (oAUTHToken.token != null && oAUTHToken.token.Length > 250)
				{
					oAUTHToken.token = oAUTHToken.token.Substring(0, 250);
				}

				if (oAUTHToken.expiryDateTime.Kind != DateTimeKind.Utc)
				{
					oAUTHToken.expiryDateTime = oAUTHToken.expiryDateTime.ToUniversalTime();
				}

				if (oAUTHToken.userData != null && oAUTHToken.userData.Length > 1000)
				{
					oAUTHToken.userData = oAUTHToken.userData.Substring(0, 1000);
				}

				_context.OAUTHTokens.Add(oAUTHToken);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.OAUTHToken entity successfully created.",
					true,
					oAUTHToken.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.OAUTHToken.CreateAnonymousWithFirstLevelSubObjects(oAUTHToken)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.OAUTHToken entity creation failed.", false, oAUTHToken.id.ToString(), "", JsonSerializer.Serialize(oAUTHToken), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "OAUTHToken", oAUTHToken.id, oAUTHToken.token));

			return CreatedAtRoute("OAUTHToken", new { id = oAUTHToken.id }, Database.OAUTHToken.CreateAnonymousWithFirstLevelSubObjects(oAUTHToken));
		}



        /// <summary>
        /// 
        /// This deletes a OAUTHToken record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/OAUTHToken/{id}")]
		[Route("api/OAUTHToken")]
		public async Task<IActionResult> DeleteOAUTHToken(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.OAUTHToken> query = (from x in _context.OAUTHTokens
				where
				(x.id == id)
				select x);


			Database.OAUTHToken oAUTHToken = await query.FirstOrDefaultAsync(cancellationToken);

			if (oAUTHToken == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.OAUTHToken DELETE", id.ToString(), new Exception("No Security.OAUTHToken entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.OAUTHToken cloneOfExisting = (Database.OAUTHToken)_context.Entry(oAUTHToken).GetDatabaseValues().ToObject();


			try
			{
				oAUTHToken.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.OAUTHToken entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.OAUTHToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.OAUTHToken.CreateAnonymousWithFirstLevelSubObjects(oAUTHToken)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.OAUTHToken entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.OAUTHToken.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.OAUTHToken.CreateAnonymousWithFirstLevelSubObjects(oAUTHToken)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of OAUTHToken records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/OAUTHTokens/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string token = null,
			DateTime? expiryDateTime = null,
			string userData = null,
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
			if (expiryDateTime.HasValue == true && expiryDateTime.Value.Kind != DateTimeKind.Utc)
			{
				expiryDateTime = expiryDateTime.Value.ToUniversalTime();
			}

			IQueryable<Database.OAUTHToken> query = (from oautht in _context.OAUTHTokens select oautht);
			if (string.IsNullOrEmpty(token) == false)
			{
				query = query.Where(oautht => oautht.token == token);
			}
			if (expiryDateTime.HasValue == true)
			{
				query = query.Where(oautht => oautht.expiryDateTime == expiryDateTime.Value);
			}
			if (string.IsNullOrEmpty(userData) == false)
			{
				query = query.Where(oautht => oautht.userData == userData);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(oautht => oautht.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(oautht => oautht.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(oautht => oautht.deleted == false);
				}
			}
			else
			{
				query = query.Where(oautht => oautht.active == true);
				query = query.Where(oautht => oautht.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the O A U T H Token, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.token.Contains(anyStringContains)
			       || x.userData.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.token).ThenBy(x => x.userData);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.OAUTHToken.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/OAUTHToken/CreateAuditEvent")]
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
