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
    /// This auto generated class provides the basic CRUD operations for the StateProvince entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the StateProvince entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class StateProvincesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<StateProvincesController> _logger;

		public StateProvincesController(SchedulerContext context, ILogger<StateProvincesController> logger) : base("Scheduler", "StateProvince")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of StateProvinces filtered by the parameters provided.
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
		[Route("api/StateProvinces")]
		public async Task<IActionResult> GetStateProvinces(
			int? countryId = null,
			string name = null,
			string description = null,
			string abbreviation = null,
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

			IQueryable<Database.StateProvince> query = (from sp in _context.StateProvinces select sp);
			if (countryId.HasValue == true)
			{
				query = query.Where(sp => sp.countryId == countryId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sp => sp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sp => sp.description == description);
			}
			if (string.IsNullOrEmpty(abbreviation) == false)
			{
				query = query.Where(sp => sp.abbreviation == abbreviation);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(sp => sp.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sp => sp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sp => sp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sp => sp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sp => sp.deleted == false);
				}
			}
			else
			{
				query = query.Where(sp => sp.active == true);
				query = query.Where(sp => sp.deleted == false);
			}

			query = query.OrderBy(sp => sp.sequence).ThenBy(sp => sp.name).ThenBy(sp => sp.description).ThenBy(sp => sp.abbreviation);


			//
			// Add the any string contains parameter to span all the string fields on the State Province, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.abbreviation.Contains(anyStringContains)
			       || (includeRelations == true && x.country.name.Contains(anyStringContains))
			       || (includeRelations == true && x.country.description.Contains(anyStringContains))
			       || (includeRelations == true && x.country.abbreviation.Contains(anyStringContains))
			       || (includeRelations == true && x.country.postalCodeFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.country.postalCodeRegEx.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.country);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.StateProvince> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.StateProvince stateProvince in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(stateProvince, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.StateProvince Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.StateProvince Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of StateProvinces filtered by the parameters provided.  Its query is similar to the GetStateProvinces method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StateProvinces/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? countryId = null,
			string name = null,
			string description = null,
			string abbreviation = null,
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

			IQueryable<Database.StateProvince> query = (from sp in _context.StateProvinces select sp);
			if (countryId.HasValue == true)
			{
				query = query.Where(sp => sp.countryId == countryId.Value);
			}
			if (name != null)
			{
				query = query.Where(sp => sp.name == name);
			}
			if (description != null)
			{
				query = query.Where(sp => sp.description == description);
			}
			if (abbreviation != null)
			{
				query = query.Where(sp => sp.abbreviation == abbreviation);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(sp => sp.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sp => sp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sp => sp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sp => sp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sp => sp.deleted == false);
				}
			}
			else
			{
				query = query.Where(sp => sp.active == true);
				query = query.Where(sp => sp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the State Province, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.abbreviation.Contains(anyStringContains)
			       || x.country.name.Contains(anyStringContains)
			       || x.country.description.Contains(anyStringContains)
			       || x.country.abbreviation.Contains(anyStringContains)
			       || x.country.postalCodeFormat.Contains(anyStringContains)
			       || x.country.postalCodeRegEx.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single StateProvince by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StateProvince/{id}")]
		public async Task<IActionResult> GetStateProvince(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.StateProvince> query = (from sp in _context.StateProvinces where
							(sp.id == id) &&
							(userIsAdmin == true || sp.deleted == false) &&
							(userIsWriter == true || sp.active == true)
					select sp);

				if (includeRelations == true)
				{
					query = query.Include(x => x.country);
					query = query.AsSplitQuery();
				}

				Database.StateProvince materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.StateProvince Entity was read with Admin privilege." : "Scheduler.StateProvince Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "StateProvince", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.StateProvince entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.StateProvince.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.StateProvince.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing StateProvince record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/StateProvince/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutStateProvince(int id, [FromBody]Database.StateProvince.StateProvinceDTO stateProvinceDTO, CancellationToken cancellationToken = default)
		{
			if (stateProvinceDTO == null)
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



			if (id != stateProvinceDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.StateProvince> query = (from x in _context.StateProvinces
				where
				(x.id == id)
				select x);


			Database.StateProvince existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.StateProvince PUT", id.ToString(), new Exception("No Scheduler.StateProvince entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (stateProvinceDTO.objectGuid == Guid.Empty)
            {
                stateProvinceDTO.objectGuid = existing.objectGuid;
            }
            else if (stateProvinceDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a StateProvince record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.StateProvince cloneOfExisting = (Database.StateProvince)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new StateProvince object using the data from the existing record, updated with what is in the DTO.
			//
			Database.StateProvince stateProvince = (Database.StateProvince)_context.Entry(existing).GetDatabaseValues().ToObject();
			stateProvince.ApplyDTO(stateProvinceDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (stateProvince.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.StateProvince record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (stateProvince.name != null && stateProvince.name.Length > 100)
			{
				stateProvince.name = stateProvince.name.Substring(0, 100);
			}

			if (stateProvince.description != null && stateProvince.description.Length > 500)
			{
				stateProvince.description = stateProvince.description.Substring(0, 500);
			}

			if (stateProvince.abbreviation != null && stateProvince.abbreviation.Length > 10)
			{
				stateProvince.abbreviation = stateProvince.abbreviation.Substring(0, 10);
			}

			EntityEntry<Database.StateProvince> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(stateProvince);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.StateProvince entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.StateProvince.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.StateProvince.CreateAnonymousWithFirstLevelSubObjects(stateProvince)),
					null);


				return Ok(Database.StateProvince.CreateAnonymous(stateProvince));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.StateProvince entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.StateProvince.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.StateProvince.CreateAnonymousWithFirstLevelSubObjects(stateProvince)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new StateProvince record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StateProvince", Name = "StateProvince")]
		public async Task<IActionResult> PostStateProvince([FromBody]Database.StateProvince.StateProvinceDTO stateProvinceDTO, CancellationToken cancellationToken = default)
		{
			if (stateProvinceDTO == null)
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
			// Create a new StateProvince object using the data from the DTO
			//
			Database.StateProvince stateProvince = Database.StateProvince.FromDTO(stateProvinceDTO);

			try
			{
				if (stateProvince.name != null && stateProvince.name.Length > 100)
				{
					stateProvince.name = stateProvince.name.Substring(0, 100);
				}

				if (stateProvince.description != null && stateProvince.description.Length > 500)
				{
					stateProvince.description = stateProvince.description.Substring(0, 500);
				}

				if (stateProvince.abbreviation != null && stateProvince.abbreviation.Length > 10)
				{
					stateProvince.abbreviation = stateProvince.abbreviation.Substring(0, 10);
				}

				stateProvince.objectGuid = Guid.NewGuid();
				_context.StateProvinces.Add(stateProvince);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.StateProvince entity successfully created.",
					true,
					stateProvince.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.StateProvince.CreateAnonymousWithFirstLevelSubObjects(stateProvince)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.StateProvince entity creation failed.", false, stateProvince.id.ToString(), "", JsonSerializer.Serialize(stateProvince), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "StateProvince", stateProvince.id, stateProvince.name));

			return CreatedAtRoute("StateProvince", new { id = stateProvince.id }, Database.StateProvince.CreateAnonymousWithFirstLevelSubObjects(stateProvince));
		}



        /// <summary>
        /// 
        /// This deletes a StateProvince record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/StateProvince/{id}")]
		[Route("api/StateProvince")]
		public async Task<IActionResult> DeleteStateProvince(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.StateProvince> query = (from x in _context.StateProvinces
				where
				(x.id == id)
				select x);


			Database.StateProvince stateProvince = await query.FirstOrDefaultAsync(cancellationToken);

			if (stateProvince == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.StateProvince DELETE", id.ToString(), new Exception("No Scheduler.StateProvince entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.StateProvince cloneOfExisting = (Database.StateProvince)_context.Entry(stateProvince).GetDatabaseValues().ToObject();


			try
			{
				stateProvince.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.StateProvince entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.StateProvince.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.StateProvince.CreateAnonymousWithFirstLevelSubObjects(stateProvince)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.StateProvince entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.StateProvince.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.StateProvince.CreateAnonymousWithFirstLevelSubObjects(stateProvince)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of StateProvince records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/StateProvinces/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? countryId = null,
			string name = null,
			string description = null,
			string abbreviation = null,
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

			IQueryable<Database.StateProvince> query = (from sp in _context.StateProvinces select sp);
			if (countryId.HasValue == true)
			{
				query = query.Where(sp => sp.countryId == countryId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(sp => sp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(sp => sp.description == description);
			}
			if (string.IsNullOrEmpty(abbreviation) == false)
			{
				query = query.Where(sp => sp.abbreviation == abbreviation);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(sp => sp.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(sp => sp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(sp => sp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(sp => sp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(sp => sp.deleted == false);
				}
			}
			else
			{
				query = query.Where(sp => sp.active == true);
				query = query.Where(sp => sp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the State Province, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.abbreviation.Contains(anyStringContains)
			       || x.country.name.Contains(anyStringContains)
			       || x.country.description.Contains(anyStringContains)
			       || x.country.abbreviation.Contains(anyStringContains)
			       || x.country.postalCodeFormat.Contains(anyStringContains)
			       || x.country.postalCodeRegEx.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.abbreviation);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.StateProvince.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/StateProvince/CreateAuditEvent")]
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
