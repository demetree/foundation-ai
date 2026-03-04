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
    /// This auto generated class provides the basic CRUD operations for the DependencyType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the DependencyType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class DependencyTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<DependencyTypesController> _logger;

		public DependencyTypesController(SchedulerContext context, ILogger<DependencyTypesController> logger) : base("Scheduler", "DependencyType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of DependencyTypes filtered by the parameters provided.
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
		[Route("api/DependencyTypes")]
		public async Task<IActionResult> GetDependencyTypes(
			string name = null,
			string description = null,
			int? sequence = null,
			string color = null,
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

			IQueryable<Database.DependencyType> query = (from dt in _context.DependencyTypes select dt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(dt => dt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(dt => dt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(dt => dt.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(dt => dt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(dt => dt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(dt => dt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(dt => dt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(dt => dt.deleted == false);
				}
			}
			else
			{
				query = query.Where(dt => dt.active == true);
				query = query.Where(dt => dt.deleted == false);
			}

			query = query.OrderBy(dt => dt.sequence).ThenBy(dt => dt.name).ThenBy(dt => dt.description).ThenBy(dt => dt.color);


			//
			// Add the any string contains parameter to span all the string fields on the Dependency Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.DependencyType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.DependencyType dependencyType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(dependencyType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.DependencyType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.DependencyType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of DependencyTypes filtered by the parameters provided.  Its query is similar to the GetDependencyTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DependencyTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			int? sequence = null,
			string color = null,
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

			IQueryable<Database.DependencyType> query = (from dt in _context.DependencyTypes select dt);
			if (name != null)
			{
				query = query.Where(dt => dt.name == name);
			}
			if (description != null)
			{
				query = query.Where(dt => dt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(dt => dt.sequence == sequence.Value);
			}
			if (color != null)
			{
				query = query.Where(dt => dt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(dt => dt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(dt => dt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(dt => dt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(dt => dt.deleted == false);
				}
			}
			else
			{
				query = query.Where(dt => dt.active == true);
				query = query.Where(dt => dt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Dependency Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single DependencyType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DependencyType/{id}")]
		public async Task<IActionResult> GetDependencyType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.DependencyType> query = (from dt in _context.DependencyTypes where
							(dt.id == id) &&
							(userIsAdmin == true || dt.deleted == false) &&
							(userIsWriter == true || dt.active == true)
					select dt);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.DependencyType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.DependencyType Entity was read with Admin privilege." : "Scheduler.DependencyType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "DependencyType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.DependencyType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.DependencyType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.DependencyType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing DependencyType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/DependencyType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutDependencyType(int id, [FromBody]Database.DependencyType.DependencyTypeDTO dependencyTypeDTO, CancellationToken cancellationToken = default)
		{
			if (dependencyTypeDTO == null)
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



			if (id != dependencyTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.DependencyType> query = (from x in _context.DependencyTypes
				where
				(x.id == id)
				select x);


			Database.DependencyType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.DependencyType PUT", id.ToString(), new Exception("No Scheduler.DependencyType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (dependencyTypeDTO.objectGuid == Guid.Empty)
            {
                dependencyTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (dependencyTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a DependencyType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.DependencyType cloneOfExisting = (Database.DependencyType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new DependencyType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.DependencyType dependencyType = (Database.DependencyType)_context.Entry(existing).GetDatabaseValues().ToObject();
			dependencyType.ApplyDTO(dependencyTypeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (dependencyType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.DependencyType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (dependencyType.name != null && dependencyType.name.Length > 100)
			{
				dependencyType.name = dependencyType.name.Substring(0, 100);
			}

			if (dependencyType.description != null && dependencyType.description.Length > 500)
			{
				dependencyType.description = dependencyType.description.Substring(0, 500);
			}

			if (dependencyType.color != null && dependencyType.color.Length > 10)
			{
				dependencyType.color = dependencyType.color.Substring(0, 10);
			}

			EntityEntry<Database.DependencyType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(dependencyType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.DependencyType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.DependencyType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.DependencyType.CreateAnonymousWithFirstLevelSubObjects(dependencyType)),
					null);


				return Ok(Database.DependencyType.CreateAnonymous(dependencyType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.DependencyType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.DependencyType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.DependencyType.CreateAnonymousWithFirstLevelSubObjects(dependencyType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new DependencyType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DependencyType", Name = "DependencyType")]
		public async Task<IActionResult> PostDependencyType([FromBody]Database.DependencyType.DependencyTypeDTO dependencyTypeDTO, CancellationToken cancellationToken = default)
		{
			if (dependencyTypeDTO == null)
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
			// Create a new DependencyType object using the data from the DTO
			//
			Database.DependencyType dependencyType = Database.DependencyType.FromDTO(dependencyTypeDTO);

			try
			{
				if (dependencyType.name != null && dependencyType.name.Length > 100)
				{
					dependencyType.name = dependencyType.name.Substring(0, 100);
				}

				if (dependencyType.description != null && dependencyType.description.Length > 500)
				{
					dependencyType.description = dependencyType.description.Substring(0, 500);
				}

				if (dependencyType.color != null && dependencyType.color.Length > 10)
				{
					dependencyType.color = dependencyType.color.Substring(0, 10);
				}

				dependencyType.objectGuid = Guid.NewGuid();
				_context.DependencyTypes.Add(dependencyType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.DependencyType entity successfully created.",
					true,
					dependencyType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.DependencyType.CreateAnonymousWithFirstLevelSubObjects(dependencyType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.DependencyType entity creation failed.", false, dependencyType.id.ToString(), "", JsonSerializer.Serialize(dependencyType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "DependencyType", dependencyType.id, dependencyType.name));

			return CreatedAtRoute("DependencyType", new { id = dependencyType.id }, Database.DependencyType.CreateAnonymousWithFirstLevelSubObjects(dependencyType));
		}



        /// <summary>
        /// 
        /// This deletes a DependencyType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/DependencyType/{id}")]
		[Route("api/DependencyType")]
		public async Task<IActionResult> DeleteDependencyType(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.DependencyType> query = (from x in _context.DependencyTypes
				where
				(x.id == id)
				select x);


			Database.DependencyType dependencyType = await query.FirstOrDefaultAsync(cancellationToken);

			if (dependencyType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.DependencyType DELETE", id.ToString(), new Exception("No Scheduler.DependencyType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.DependencyType cloneOfExisting = (Database.DependencyType)_context.Entry(dependencyType).GetDatabaseValues().ToObject();


			try
			{
				dependencyType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.DependencyType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.DependencyType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.DependencyType.CreateAnonymousWithFirstLevelSubObjects(dependencyType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.DependencyType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.DependencyType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.DependencyType.CreateAnonymousWithFirstLevelSubObjects(dependencyType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of DependencyType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/DependencyTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			int? sequence = null,
			string color = null,
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

			IQueryable<Database.DependencyType> query = (from dt in _context.DependencyTypes select dt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(dt => dt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(dt => dt.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(dt => dt.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(dt => dt.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(dt => dt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(dt => dt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(dt => dt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(dt => dt.deleted == false);
				}
			}
			else
			{
				query = query.Where(dt => dt.active == true);
				query = query.Where(dt => dt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Dependency Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.DependencyType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/DependencyType/CreateAuditEvent")]
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
