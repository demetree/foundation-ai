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
    /// This auto generated class provides the basic CRUD operations for the SecurityUserTitle entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityUserTitle entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityUserTitlesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 150;

		private SecurityContext _context;

		private ILogger<SecurityUserTitlesController> _logger;

		public SecurityUserTitlesController(SecurityContext context, ILogger<SecurityUserTitlesController> logger) : base("Security", "SecurityUserTitle")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityUserTitles filtered by the parameters provided.
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
		[Route("api/SecurityUserTitles")]
		public async Task<IActionResult> GetSecurityUserTitles(
			string name = null,
			string description = null,
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

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
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

			IQueryable<Database.SecurityUserTitle> query = (from sut in _context.SecurityUserTitles select sut);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sut => sut.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sut => sut.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sut => sut.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sut => sut.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sut => sut.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sut => sut.deleted == false);
				}
			}
			else
			{
				query = query.Where(sut => sut.active == true);
				query = query.Where(sut => sut.deleted == false);
			}

			query = query.OrderBy(sut => sut.name).ThenBy(sut => sut.description);

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
			// Add the any string contains parameter to span all the string fields on the Security User Title, or on an any of the string fields on its immediate relations
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
			
			List<Database.SecurityUserTitle> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityUserTitle securityUserTitle in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityUserTitle, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityUserTitle Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityUserTitle Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityUserTitles filtered by the parameters provided.  Its query is similar to the GetSecurityUserTitles method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserTitles/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SecurityUserTitle> query = (from sut in _context.SecurityUserTitles select sut);
			if (name != null)
			{
				query = query.Where(sut => sut.name == name);
			}
			if (description != null)
			{
				query = query.Where(sut => sut.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sut => sut.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sut => sut.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sut => sut.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sut => sut.deleted == false);
				}
			}
			else
			{
				query = query.Where(sut => sut.active == true);
				query = query.Where(sut => sut.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security User Title, or on an any of the string fields on its immediate relations
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
        /// This gets a single SecurityUserTitle by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserTitle/{id}")]
		public async Task<IActionResult> GetSecurityUserTitle(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.SecurityUserTitle> query = (from sut in _context.SecurityUserTitles where
							(sut.id == id) &&
							(userIsAdmin == true || sut.deleted == false) &&
							(userIsWriter == true || sut.active == true)
					select sut);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.SecurityUserTitle materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityUserTitle Entity was read with Admin privilege." : "Security.SecurityUserTitle Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUserTitle", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityUserTitle entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityUserTitle.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityUserTitle.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityUserTitle record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityUserTitle/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityUserTitle(int id, [FromBody]Database.SecurityUserTitle.SecurityUserTitleDTO securityUserTitleDTO, CancellationToken cancellationToken = default)
		{
			if (securityUserTitleDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityUserTitleDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityUserTitle> query = (from x in _context.SecurityUserTitles
				where
				(x.id == id)
				select x);


			Database.SecurityUserTitle existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUserTitle PUT", id.ToString(), new Exception("No Security.SecurityUserTitle entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityUserTitleDTO.objectGuid == Guid.Empty)
            {
                securityUserTitleDTO.objectGuid = existing.objectGuid;
            }
            else if (securityUserTitleDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityUserTitle record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityUserTitle cloneOfExisting = (Database.SecurityUserTitle)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityUserTitle object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityUserTitle securityUserTitle = (Database.SecurityUserTitle)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityUserTitle.ApplyDTO(securityUserTitleDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityUserTitle.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityUserTitle record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityUserTitle.name != null && securityUserTitle.name.Length > 100)
			{
				securityUserTitle.name = securityUserTitle.name.Substring(0, 100);
			}

			if (securityUserTitle.description != null && securityUserTitle.description.Length > 500)
			{
				securityUserTitle.description = securityUserTitle.description.Substring(0, 500);
			}

			EntityEntry<Database.SecurityUserTitle> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityUserTitle);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityUserTitle entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserTitle.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserTitle.CreateAnonymousWithFirstLevelSubObjects(securityUserTitle)),
					null);


				return Ok(Database.SecurityUserTitle.CreateAnonymous(securityUserTitle));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityUserTitle entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserTitle.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserTitle.CreateAnonymousWithFirstLevelSubObjects(securityUserTitle)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityUserTitle record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserTitle", Name = "SecurityUserTitle")]
		public async Task<IActionResult> PostSecurityUserTitle([FromBody]Database.SecurityUserTitle.SecurityUserTitleDTO securityUserTitleDTO, CancellationToken cancellationToken = default)
		{
			if (securityUserTitleDTO == null)
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
			// Create a new SecurityUserTitle object using the data from the DTO
			//
			Database.SecurityUserTitle securityUserTitle = Database.SecurityUserTitle.FromDTO(securityUserTitleDTO);

			try
			{
				if (securityUserTitle.name != null && securityUserTitle.name.Length > 100)
				{
					securityUserTitle.name = securityUserTitle.name.Substring(0, 100);
				}

				if (securityUserTitle.description != null && securityUserTitle.description.Length > 500)
				{
					securityUserTitle.description = securityUserTitle.description.Substring(0, 500);
				}

				securityUserTitle.objectGuid = Guid.NewGuid();
				_context.SecurityUserTitles.Add(securityUserTitle);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityUserTitle entity successfully created.",
					true,
					securityUserTitle.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityUserTitle.CreateAnonymousWithFirstLevelSubObjects(securityUserTitle)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityUserTitle entity creation failed.", false, securityUserTitle.id.ToString(), "", JsonSerializer.Serialize(securityUserTitle), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUserTitle", securityUserTitle.id, securityUserTitle.name));

			return CreatedAtRoute("SecurityUserTitle", new { id = securityUserTitle.id }, Database.SecurityUserTitle.CreateAnonymousWithFirstLevelSubObjects(securityUserTitle));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityUserTitle record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserTitle/{id}")]
		[Route("api/SecurityUserTitle")]
		public async Task<IActionResult> DeleteSecurityUserTitle(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityUserTitle> query = (from x in _context.SecurityUserTitles
				where
				(x.id == id)
				select x);


			Database.SecurityUserTitle securityUserTitle = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityUserTitle == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUserTitle DELETE", id.ToString(), new Exception("No Security.SecurityUserTitle entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityUserTitle cloneOfExisting = (Database.SecurityUserTitle)_context.Entry(securityUserTitle).GetDatabaseValues().ToObject();


			try
			{
				securityUserTitle.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityUserTitle entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserTitle.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserTitle.CreateAnonymousWithFirstLevelSubObjects(securityUserTitle)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityUserTitle entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserTitle.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserTitle.CreateAnonymousWithFirstLevelSubObjects(securityUserTitle)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SecurityUserTitle records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityUserTitles/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 150, cancellationToken);

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

			IQueryable<Database.SecurityUserTitle> query = (from sut in _context.SecurityUserTitles select sut);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sut => sut.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sut => sut.description == description);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sut => sut.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sut => sut.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sut => sut.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sut => sut.deleted == false);
				}
			}
			else
			{
				query = query.Where(sut => sut.active == true);
				query = query.Where(sut => sut.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security User Title, or on an any of the string fields on its immediate relations
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


			query = query.OrderBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SecurityUserTitle.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityUserTitle/CreateAuditEvent")]
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
