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
    /// This auto generated class provides the basic CRUD operations for the UserProfileLinkType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserProfileLinkType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserProfileLinkTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<UserProfileLinkTypesController> _logger;

		public UserProfileLinkTypesController(BMCContext context, ILogger<UserProfileLinkTypesController> logger) : base("BMC", "UserProfileLinkType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserProfileLinkTypes filtered by the parameters provided.
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
		[Route("api/UserProfileLinkTypes")]
		public async Task<IActionResult> GetUserProfileLinkTypes(
			string name = null,
			string description = null,
			string iconCssClass = null,
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

			IQueryable<Database.UserProfileLinkType> query = (from uplt in _context.UserProfileLinkTypes select uplt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(uplt => uplt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(uplt => uplt.description == description);
			}
			if (string.IsNullOrEmpty(iconCssClass) == false)
			{
				query = query.Where(uplt => uplt.iconCssClass == iconCssClass);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(uplt => uplt.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uplt => uplt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uplt => uplt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uplt => uplt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uplt => uplt.deleted == false);
				}
			}
			else
			{
				query = query.Where(uplt => uplt.active == true);
				query = query.Where(uplt => uplt.deleted == false);
			}

			query = query.OrderBy(uplt => uplt.sequence).ThenBy(uplt => uplt.name).ThenBy(uplt => uplt.description).ThenBy(uplt => uplt.iconCssClass);

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
			// Add the any string contains parameter to span all the string fields on the User Profile Link Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.iconCssClass.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserProfileLinkType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserProfileLinkType userProfileLinkType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userProfileLinkType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserProfileLinkType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserProfileLinkType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserProfileLinkTypes filtered by the parameters provided.  Its query is similar to the GetUserProfileLinkTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileLinkTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string iconCssClass = null,
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

			IQueryable<Database.UserProfileLinkType> query = (from uplt in _context.UserProfileLinkTypes select uplt);
			if (name != null)
			{
				query = query.Where(uplt => uplt.name == name);
			}
			if (description != null)
			{
				query = query.Where(uplt => uplt.description == description);
			}
			if (iconCssClass != null)
			{
				query = query.Where(uplt => uplt.iconCssClass == iconCssClass);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(uplt => uplt.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uplt => uplt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uplt => uplt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uplt => uplt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uplt => uplt.deleted == false);
				}
			}
			else
			{
				query = query.Where(uplt => uplt.active == true);
				query = query.Where(uplt => uplt.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Profile Link Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.iconCssClass.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserProfileLinkType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileLinkType/{id}")]
		public async Task<IActionResult> GetUserProfileLinkType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserProfileLinkType> query = (from uplt in _context.UserProfileLinkTypes where
							(uplt.id == id) &&
							(userIsAdmin == true || uplt.deleted == false) &&
							(userIsWriter == true || uplt.active == true)
					select uplt);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.UserProfileLinkType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserProfileLinkType Entity was read with Admin privilege." : "BMC.UserProfileLinkType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserProfileLinkType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserProfileLinkType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserProfileLinkType.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserProfileLinkType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserProfileLinkType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserProfileLinkType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserProfileLinkType(int id, [FromBody]Database.UserProfileLinkType.UserProfileLinkTypeDTO userProfileLinkTypeDTO, CancellationToken cancellationToken = default)
		{
			if (userProfileLinkTypeDTO == null)
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



			if (id != userProfileLinkTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.UserProfileLinkType> query = (from x in _context.UserProfileLinkTypes
				where
				(x.id == id)
				select x);


			Database.UserProfileLinkType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfileLinkType PUT", id.ToString(), new Exception("No BMC.UserProfileLinkType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userProfileLinkTypeDTO.objectGuid == Guid.Empty)
            {
                userProfileLinkTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (userProfileLinkTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserProfileLinkType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserProfileLinkType cloneOfExisting = (Database.UserProfileLinkType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserProfileLinkType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserProfileLinkType userProfileLinkType = (Database.UserProfileLinkType)_context.Entry(existing).GetDatabaseValues().ToObject();
			userProfileLinkType.ApplyDTO(userProfileLinkTypeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userProfileLinkType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserProfileLinkType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (userProfileLinkType.name != null && userProfileLinkType.name.Length > 100)
			{
				userProfileLinkType.name = userProfileLinkType.name.Substring(0, 100);
			}

			if (userProfileLinkType.description != null && userProfileLinkType.description.Length > 500)
			{
				userProfileLinkType.description = userProfileLinkType.description.Substring(0, 500);
			}

			if (userProfileLinkType.iconCssClass != null && userProfileLinkType.iconCssClass.Length > 100)
			{
				userProfileLinkType.iconCssClass = userProfileLinkType.iconCssClass.Substring(0, 100);
			}

			EntityEntry<Database.UserProfileLinkType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userProfileLinkType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserProfileLinkType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileLinkType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileLinkType.CreateAnonymousWithFirstLevelSubObjects(userProfileLinkType)),
					null);


				return Ok(Database.UserProfileLinkType.CreateAnonymous(userProfileLinkType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserProfileLinkType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileLinkType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileLinkType.CreateAnonymousWithFirstLevelSubObjects(userProfileLinkType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserProfileLinkType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileLinkType", Name = "UserProfileLinkType")]
		public async Task<IActionResult> PostUserProfileLinkType([FromBody]Database.UserProfileLinkType.UserProfileLinkTypeDTO userProfileLinkTypeDTO, CancellationToken cancellationToken = default)
		{
			if (userProfileLinkTypeDTO == null)
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
			// Create a new UserProfileLinkType object using the data from the DTO
			//
			Database.UserProfileLinkType userProfileLinkType = Database.UserProfileLinkType.FromDTO(userProfileLinkTypeDTO);

			try
			{
				if (userProfileLinkType.name != null && userProfileLinkType.name.Length > 100)
				{
					userProfileLinkType.name = userProfileLinkType.name.Substring(0, 100);
				}

				if (userProfileLinkType.description != null && userProfileLinkType.description.Length > 500)
				{
					userProfileLinkType.description = userProfileLinkType.description.Substring(0, 500);
				}

				if (userProfileLinkType.iconCssClass != null && userProfileLinkType.iconCssClass.Length > 100)
				{
					userProfileLinkType.iconCssClass = userProfileLinkType.iconCssClass.Substring(0, 100);
				}

				userProfileLinkType.objectGuid = Guid.NewGuid();
				_context.UserProfileLinkTypes.Add(userProfileLinkType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserProfileLinkType entity successfully created.",
					true,
					userProfileLinkType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserProfileLinkType.CreateAnonymousWithFirstLevelSubObjects(userProfileLinkType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserProfileLinkType entity creation failed.", false, userProfileLinkType.id.ToString(), "", JsonSerializer.Serialize(userProfileLinkType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserProfileLinkType", userProfileLinkType.id, userProfileLinkType.name));

			return CreatedAtRoute("UserProfileLinkType", new { id = userProfileLinkType.id }, Database.UserProfileLinkType.CreateAnonymousWithFirstLevelSubObjects(userProfileLinkType));
		}



        /// <summary>
        /// 
        /// This deletes a UserProfileLinkType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileLinkType/{id}")]
		[Route("api/UserProfileLinkType")]
		public async Task<IActionResult> DeleteUserProfileLinkType(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserProfileLinkType> query = (from x in _context.UserProfileLinkTypes
				where
				(x.id == id)
				select x);


			Database.UserProfileLinkType userProfileLinkType = await query.FirstOrDefaultAsync(cancellationToken);

			if (userProfileLinkType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfileLinkType DELETE", id.ToString(), new Exception("No BMC.UserProfileLinkType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserProfileLinkType cloneOfExisting = (Database.UserProfileLinkType)_context.Entry(userProfileLinkType).GetDatabaseValues().ToObject();


			try
			{
				userProfileLinkType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserProfileLinkType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileLinkType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileLinkType.CreateAnonymousWithFirstLevelSubObjects(userProfileLinkType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserProfileLinkType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileLinkType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileLinkType.CreateAnonymousWithFirstLevelSubObjects(userProfileLinkType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserProfileLinkType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserProfileLinkTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string iconCssClass = null,
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

			IQueryable<Database.UserProfileLinkType> query = (from uplt in _context.UserProfileLinkTypes select uplt);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(uplt => uplt.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(uplt => uplt.description == description);
			}
			if (string.IsNullOrEmpty(iconCssClass) == false)
			{
				query = query.Where(uplt => uplt.iconCssClass == iconCssClass);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(uplt => uplt.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(uplt => uplt.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(uplt => uplt.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(uplt => uplt.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(uplt => uplt.deleted == false);
				}
			}
			else
			{
				query = query.Where(uplt => uplt.active == true);
				query = query.Where(uplt => uplt.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Profile Link Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.iconCssClass.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.iconCssClass);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserProfileLinkType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserProfileLinkType/CreateAuditEvent")]
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
