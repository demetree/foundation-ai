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
    /// This auto generated class provides the basic CRUD operations for the ColourFinish entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ColourFinish entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ColourFinishesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<ColourFinishesController> _logger;

		public ColourFinishesController(BMCContext context, ILogger<ColourFinishesController> logger) : base("BMC", "ColourFinish")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ColourFinishes filtered by the parameters provided.
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
		[Route("api/ColourFinishes")]
		public async Task<IActionResult> GetColourFinishes(
			string name = null,
			string description = null,
			bool? requiresEnvironmentMap = null,
			bool? isMatte = null,
			int? defaultAlpha = null,
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

			IQueryable<Database.ColourFinish> query = (from cf in _context.ColourFinishes select cf);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cf => cf.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cf => cf.description == description);
			}
			if (requiresEnvironmentMap.HasValue == true)
			{
				query = query.Where(cf => cf.requiresEnvironmentMap == requiresEnvironmentMap.Value);
			}
			if (isMatte.HasValue == true)
			{
				query = query.Where(cf => cf.isMatte == isMatte.Value);
			}
			if (defaultAlpha.HasValue == true)
			{
				query = query.Where(cf => cf.defaultAlpha == defaultAlpha.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cf => cf.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cf => cf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cf => cf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cf => cf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cf => cf.deleted == false);
				}
			}
			else
			{
				query = query.Where(cf => cf.active == true);
				query = query.Where(cf => cf.deleted == false);
			}

			query = query.OrderBy(cf => cf.sequence).ThenBy(cf => cf.name).ThenBy(cf => cf.description);

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
			// Add the any string contains parameter to span all the string fields on the Colour Finish, or on an any of the string fields on its immediate relations
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
			
			List<Database.ColourFinish> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ColourFinish colourFinish in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(colourFinish, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ColourFinish Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ColourFinish Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ColourFinishes filtered by the parameters provided.  Its query is similar to the GetColourFinishes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ColourFinishes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			bool? requiresEnvironmentMap = null,
			bool? isMatte = null,
			int? defaultAlpha = null,
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

			IQueryable<Database.ColourFinish> query = (from cf in _context.ColourFinishes select cf);
			if (name != null)
			{
				query = query.Where(cf => cf.name == name);
			}
			if (description != null)
			{
				query = query.Where(cf => cf.description == description);
			}
			if (requiresEnvironmentMap.HasValue == true)
			{
				query = query.Where(cf => cf.requiresEnvironmentMap == requiresEnvironmentMap.Value);
			}
			if (isMatte.HasValue == true)
			{
				query = query.Where(cf => cf.isMatte == isMatte.Value);
			}
			if (defaultAlpha.HasValue == true)
			{
				query = query.Where(cf => cf.defaultAlpha == defaultAlpha.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cf => cf.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cf => cf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cf => cf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cf => cf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cf => cf.deleted == false);
				}
			}
			else
			{
				query = query.Where(cf => cf.active == true);
				query = query.Where(cf => cf.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Colour Finish, or on an any of the string fields on its immediate relations
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
        /// This gets a single ColourFinish by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ColourFinish/{id}")]
		public async Task<IActionResult> GetColourFinish(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ColourFinish> query = (from cf in _context.ColourFinishes where
							(cf.id == id) &&
							(userIsAdmin == true || cf.deleted == false) &&
							(userIsWriter == true || cf.active == true)
					select cf);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.ColourFinish materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ColourFinish Entity was read with Admin privilege." : "BMC.ColourFinish Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ColourFinish", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ColourFinish entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ColourFinish.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ColourFinish.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ColourFinish record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ColourFinish/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutColourFinish(int id, [FromBody]Database.ColourFinish.ColourFinishDTO colourFinishDTO, CancellationToken cancellationToken = default)
		{
			if (colourFinishDTO == null)
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



			if (id != colourFinishDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ColourFinish> query = (from x in _context.ColourFinishes
				where
				(x.id == id)
				select x);


			Database.ColourFinish existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ColourFinish PUT", id.ToString(), new Exception("No BMC.ColourFinish entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (colourFinishDTO.objectGuid == Guid.Empty)
            {
                colourFinishDTO.objectGuid = existing.objectGuid;
            }
            else if (colourFinishDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ColourFinish record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ColourFinish cloneOfExisting = (Database.ColourFinish)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ColourFinish object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ColourFinish colourFinish = (Database.ColourFinish)_context.Entry(existing).GetDatabaseValues().ToObject();
			colourFinish.ApplyDTO(colourFinishDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (colourFinish.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ColourFinish record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (colourFinish.name != null && colourFinish.name.Length > 100)
			{
				colourFinish.name = colourFinish.name.Substring(0, 100);
			}

			if (colourFinish.description != null && colourFinish.description.Length > 500)
			{
				colourFinish.description = colourFinish.description.Substring(0, 500);
			}

			EntityEntry<Database.ColourFinish> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(colourFinish);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ColourFinish entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ColourFinish.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ColourFinish.CreateAnonymousWithFirstLevelSubObjects(colourFinish)),
					null);


				return Ok(Database.ColourFinish.CreateAnonymous(colourFinish));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ColourFinish entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ColourFinish.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ColourFinish.CreateAnonymousWithFirstLevelSubObjects(colourFinish)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ColourFinish record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ColourFinish", Name = "ColourFinish")]
		public async Task<IActionResult> PostColourFinish([FromBody]Database.ColourFinish.ColourFinishDTO colourFinishDTO, CancellationToken cancellationToken = default)
		{
			if (colourFinishDTO == null)
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
			// Create a new ColourFinish object using the data from the DTO
			//
			Database.ColourFinish colourFinish = Database.ColourFinish.FromDTO(colourFinishDTO);

			try
			{
				if (colourFinish.name != null && colourFinish.name.Length > 100)
				{
					colourFinish.name = colourFinish.name.Substring(0, 100);
				}

				if (colourFinish.description != null && colourFinish.description.Length > 500)
				{
					colourFinish.description = colourFinish.description.Substring(0, 500);
				}

				colourFinish.objectGuid = Guid.NewGuid();
				_context.ColourFinishes.Add(colourFinish);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ColourFinish entity successfully created.",
					true,
					colourFinish.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ColourFinish.CreateAnonymousWithFirstLevelSubObjects(colourFinish)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ColourFinish entity creation failed.", false, colourFinish.id.ToString(), "", JsonSerializer.Serialize(colourFinish), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ColourFinish", colourFinish.id, colourFinish.name));

			return CreatedAtRoute("ColourFinish", new { id = colourFinish.id }, Database.ColourFinish.CreateAnonymousWithFirstLevelSubObjects(colourFinish));
		}



        /// <summary>
        /// 
        /// This deletes a ColourFinish record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ColourFinish/{id}")]
		[Route("api/ColourFinish")]
		public async Task<IActionResult> DeleteColourFinish(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ColourFinish> query = (from x in _context.ColourFinishes
				where
				(x.id == id)
				select x);


			Database.ColourFinish colourFinish = await query.FirstOrDefaultAsync(cancellationToken);

			if (colourFinish == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ColourFinish DELETE", id.ToString(), new Exception("No BMC.ColourFinish entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ColourFinish cloneOfExisting = (Database.ColourFinish)_context.Entry(colourFinish).GetDatabaseValues().ToObject();


			try
			{
				colourFinish.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ColourFinish entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ColourFinish.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ColourFinish.CreateAnonymousWithFirstLevelSubObjects(colourFinish)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ColourFinish entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ColourFinish.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ColourFinish.CreateAnonymousWithFirstLevelSubObjects(colourFinish)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ColourFinish records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ColourFinishes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			bool? requiresEnvironmentMap = null,
			bool? isMatte = null,
			int? defaultAlpha = null,
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

			IQueryable<Database.ColourFinish> query = (from cf in _context.ColourFinishes select cf);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cf => cf.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cf => cf.description == description);
			}
			if (requiresEnvironmentMap.HasValue == true)
			{
				query = query.Where(cf => cf.requiresEnvironmentMap == requiresEnvironmentMap.Value);
			}
			if (isMatte.HasValue == true)
			{
				query = query.Where(cf => cf.isMatte == isMatte.Value);
			}
			if (defaultAlpha.HasValue == true)
			{
				query = query.Where(cf => cf.defaultAlpha == defaultAlpha.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cf => cf.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cf => cf.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cf => cf.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cf => cf.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cf => cf.deleted == false);
				}
			}
			else
			{
				query = query.Where(cf => cf.active == true);
				query = query.Where(cf => cf.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Colour Finish, or on an any of the string fields on its immediate relations
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


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ColourFinish.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ColourFinish/CreateAuditEvent")]
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
