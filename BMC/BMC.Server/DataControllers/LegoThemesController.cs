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
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the LegoTheme entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the LegoTheme entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class LegoThemesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<LegoThemesController> _logger;

		public LegoThemesController(BMCContext context, ILogger<LegoThemesController> logger) : base("BMC", "LegoTheme")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of LegoThemes filtered by the parameters provided.
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
		[Route("api/LegoThemes")]
		public async Task<IActionResult> GetLegoThemes(
			string name = null,
			string description = null,
			int? legoThemeId = null,
			int? rebrickableThemeId = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.LegoTheme> query = (from lt in _context.LegoThemes select lt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(lt => lt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(lt => lt.description == description);
			}
			if (legoThemeId.HasValue == true)
			{
				query = query.Where(lt => lt.legoThemeId == legoThemeId.Value);
			}
			if (rebrickableThemeId.HasValue == true)
			{
				query = query.Where(lt => lt.rebrickableThemeId == rebrickableThemeId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(lt => lt.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lt => lt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lt => lt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lt => lt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lt => lt.deleted == false);
				}
			}
			else
			{
				query = query.Where(lt => lt.active == true);
				query = query.Where(lt => lt.deleted == false);
			}

			query = query.OrderBy(lt => lt.sequence).ThenBy(lt => lt.name).ThenBy(lt => lt.description);


			//
			// Add the any string contains parameter to span all the string fields on the Lego Theme, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.legoTheme.name.Contains(anyStringContains))
			       || (includeRelations == true && x.legoTheme.description.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.legoTheme);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.LegoTheme> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.LegoTheme legoTheme in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(legoTheme, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.LegoTheme Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.LegoTheme Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of LegoThemes filtered by the parameters provided.  Its query is similar to the GetLegoThemes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoThemes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? legoThemeId = null,
			int? rebrickableThemeId = null,
			int? sequence = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.LegoTheme> query = (from lt in _context.LegoThemes select lt);
			if (name != null)
			{
				query = query.Where(lt => lt.name == name);
			}
			if (description != null)
			{
				query = query.Where(lt => lt.description == description);
			}
			if (legoThemeId.HasValue == true)
			{
				query = query.Where(lt => lt.legoThemeId == legoThemeId.Value);
			}
			if (rebrickableThemeId.HasValue == true)
			{
				query = query.Where(lt => lt.rebrickableThemeId == rebrickableThemeId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(lt => lt.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lt => lt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lt => lt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lt => lt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lt => lt.deleted == false);
				}
			}
			else
			{
				query = query.Where(lt => lt.active == true);
				query = query.Where(lt => lt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Lego Theme, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.legoTheme.name.Contains(anyStringContains)
			       || x.legoTheme.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single LegoTheme by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoTheme/{id}")]
		public async Task<IActionResult> GetLegoTheme(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.LegoTheme> query = (from lt in _context.LegoThemes where
							(lt.id == id) &&
							(userIsAdmin == true || lt.deleted == false) &&
							(userIsWriter == true || lt.active == true)
					select lt);

				if (includeRelations == true)
				{
					query = query.Include(x => x.legoTheme);
					query = query.AsSplitQuery();
				}

				Database.LegoTheme materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.LegoTheme Entity was read with Admin privilege." : "BMC.LegoTheme Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoTheme", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.LegoTheme entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.LegoTheme.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.LegoTheme.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing LegoTheme record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/LegoTheme/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutLegoTheme(int id, [FromBody]Database.LegoTheme.LegoThemeDTO legoThemeDTO, CancellationToken cancellationToken = default)
		{
			if (legoThemeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != legoThemeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.LegoTheme> query = (from x in _context.LegoThemes
				where
				(x.id == id)
				select x);


			Database.LegoTheme existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoTheme PUT", id.ToString(), new Exception("No BMC.LegoTheme entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (legoThemeDTO.objectGuid == Guid.Empty)
            {
                legoThemeDTO.objectGuid = existing.objectGuid;
            }
            else if (legoThemeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a LegoTheme record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.LegoTheme cloneOfExisting = (Database.LegoTheme)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new LegoTheme object using the data from the existing record, updated with what is in the DTO.
			//
			Database.LegoTheme legoTheme = (Database.LegoTheme)_context.Entry(existing).GetDatabaseValues().ToObject();
			legoTheme.ApplyDTO(legoThemeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (legoTheme.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.LegoTheme record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (legoTheme.name != null && legoTheme.name.Length > 100)
			{
				legoTheme.name = legoTheme.name.Substring(0, 100);
			}

			if (legoTheme.description != null && legoTheme.description.Length > 500)
			{
				legoTheme.description = legoTheme.description.Substring(0, 500);
			}

			EntityEntry<Database.LegoTheme> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(legoTheme);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoTheme entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoTheme.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoTheme.CreateAnonymousWithFirstLevelSubObjects(legoTheme)),
					null);


				return Ok(Database.LegoTheme.CreateAnonymous(legoTheme));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.LegoTheme entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoTheme.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoTheme.CreateAnonymousWithFirstLevelSubObjects(legoTheme)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new LegoTheme record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoTheme", Name = "LegoTheme")]
		public async Task<IActionResult> PostLegoTheme([FromBody]Database.LegoTheme.LegoThemeDTO legoThemeDTO, CancellationToken cancellationToken = default)
		{
			if (legoThemeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new LegoTheme object using the data from the DTO
			//
			Database.LegoTheme legoTheme = Database.LegoTheme.FromDTO(legoThemeDTO);

			try
			{
				if (legoTheme.name != null && legoTheme.name.Length > 100)
				{
					legoTheme.name = legoTheme.name.Substring(0, 100);
				}

				if (legoTheme.description != null && legoTheme.description.Length > 500)
				{
					legoTheme.description = legoTheme.description.Substring(0, 500);
				}

				legoTheme.objectGuid = Guid.NewGuid();
				_context.LegoThemes.Add(legoTheme);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.LegoTheme entity successfully created.",
					true,
					legoTheme.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.LegoTheme.CreateAnonymousWithFirstLevelSubObjects(legoTheme)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.LegoTheme entity creation failed.", false, legoTheme.id.ToString(), "", JsonSerializer.Serialize(legoTheme), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "LegoTheme", legoTheme.id, legoTheme.name));

			return CreatedAtRoute("LegoTheme", new { id = legoTheme.id }, Database.LegoTheme.CreateAnonymousWithFirstLevelSubObjects(legoTheme));
		}



        /// <summary>
        /// 
        /// This deletes a LegoTheme record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/LegoTheme/{id}")]
		[Route("api/LegoTheme")]
		public async Task<IActionResult> DeleteLegoTheme(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.LegoTheme> query = (from x in _context.LegoThemes
				where
				(x.id == id)
				select x);


			Database.LegoTheme legoTheme = await query.FirstOrDefaultAsync(cancellationToken);

			if (legoTheme == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.LegoTheme DELETE", id.ToString(), new Exception("No BMC.LegoTheme entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.LegoTheme cloneOfExisting = (Database.LegoTheme)_context.Entry(legoTheme).GetDatabaseValues().ToObject();


			try
			{
				legoTheme.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoTheme entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoTheme.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoTheme.CreateAnonymousWithFirstLevelSubObjects(legoTheme)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.LegoTheme entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.LegoTheme.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.LegoTheme.CreateAnonymousWithFirstLevelSubObjects(legoTheme)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of LegoTheme records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/LegoThemes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? legoThemeId = null,
			int? rebrickableThemeId = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.LegoTheme> query = (from lt in _context.LegoThemes select lt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(lt => lt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(lt => lt.description == description);
			}
			if (legoThemeId.HasValue == true)
			{
				query = query.Where(lt => lt.legoThemeId == legoThemeId.Value);
			}
			if (rebrickableThemeId.HasValue == true)
			{
				query = query.Where(lt => lt.rebrickableThemeId == rebrickableThemeId.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(lt => lt.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(lt => lt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(lt => lt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(lt => lt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(lt => lt.deleted == false);
				}
			}
			else
			{
				query = query.Where(lt => lt.active == true);
				query = query.Where(lt => lt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Lego Theme, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.legoTheme.name.Contains(anyStringContains)
			       || x.legoTheme.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.LegoTheme.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/LegoTheme/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
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
