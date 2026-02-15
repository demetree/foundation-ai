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
    /// This auto generated class provides the basic CRUD operations for the UserBadge entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserBadge entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserBadgesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<UserBadgesController> _logger;

		public UserBadgesController(BMCContext context, ILogger<UserBadgesController> logger) : base("BMC", "UserBadge")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserBadges filtered by the parameters provided.
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
		[Route("api/UserBadges")]
		public async Task<IActionResult> GetUserBadges(
			string name = null,
			string description = null,
			string iconCssClass = null,
			string iconImagePath = null,
			string badgeColor = null,
			bool? isAutomatic = null,
			string automaticCriteriaCode = null,
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

			IQueryable<Database.UserBadge> query = (from ub in _context.UserBadges select ub);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ub => ub.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ub => ub.description == description);
			}
			if (string.IsNullOrEmpty(iconCssClass) == false)
			{
				query = query.Where(ub => ub.iconCssClass == iconCssClass);
			}
			if (string.IsNullOrEmpty(iconImagePath) == false)
			{
				query = query.Where(ub => ub.iconImagePath == iconImagePath);
			}
			if (string.IsNullOrEmpty(badgeColor) == false)
			{
				query = query.Where(ub => ub.badgeColor == badgeColor);
			}
			if (isAutomatic.HasValue == true)
			{
				query = query.Where(ub => ub.isAutomatic == isAutomatic.Value);
			}
			if (string.IsNullOrEmpty(automaticCriteriaCode) == false)
			{
				query = query.Where(ub => ub.automaticCriteriaCode == automaticCriteriaCode);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ub => ub.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ub => ub.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ub => ub.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ub => ub.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ub => ub.deleted == false);
				}
			}
			else
			{
				query = query.Where(ub => ub.active == true);
				query = query.Where(ub => ub.deleted == false);
			}

			query = query.OrderBy(ub => ub.sequence).ThenBy(ub => ub.name).ThenBy(ub => ub.description).ThenBy(ub => ub.iconCssClass);

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
			// Add the any string contains parameter to span all the string fields on the User Badge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.iconCssClass.Contains(anyStringContains)
			       || x.iconImagePath.Contains(anyStringContains)
			       || x.badgeColor.Contains(anyStringContains)
			       || x.automaticCriteriaCode.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserBadge> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserBadge userBadge in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userBadge, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserBadge Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserBadge Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserBadges filtered by the parameters provided.  Its query is similar to the GetUserBadges method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserBadges/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string iconCssClass = null,
			string iconImagePath = null,
			string badgeColor = null,
			bool? isAutomatic = null,
			string automaticCriteriaCode = null,
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

			IQueryable<Database.UserBadge> query = (from ub in _context.UserBadges select ub);
			if (name != null)
			{
				query = query.Where(ub => ub.name == name);
			}
			if (description != null)
			{
				query = query.Where(ub => ub.description == description);
			}
			if (iconCssClass != null)
			{
				query = query.Where(ub => ub.iconCssClass == iconCssClass);
			}
			if (iconImagePath != null)
			{
				query = query.Where(ub => ub.iconImagePath == iconImagePath);
			}
			if (badgeColor != null)
			{
				query = query.Where(ub => ub.badgeColor == badgeColor);
			}
			if (isAutomatic.HasValue == true)
			{
				query = query.Where(ub => ub.isAutomatic == isAutomatic.Value);
			}
			if (automaticCriteriaCode != null)
			{
				query = query.Where(ub => ub.automaticCriteriaCode == automaticCriteriaCode);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ub => ub.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ub => ub.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ub => ub.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ub => ub.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ub => ub.deleted == false);
				}
			}
			else
			{
				query = query.Where(ub => ub.active == true);
				query = query.Where(ub => ub.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Badge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.iconCssClass.Contains(anyStringContains)
			       || x.iconImagePath.Contains(anyStringContains)
			       || x.badgeColor.Contains(anyStringContains)
			       || x.automaticCriteriaCode.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserBadge by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserBadge/{id}")]
		public async Task<IActionResult> GetUserBadge(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.UserBadge> query = (from ub in _context.UserBadges where
							(ub.id == id) &&
							(userIsAdmin == true || ub.deleted == false) &&
							(userIsWriter == true || ub.active == true)
					select ub);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.UserBadge materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserBadge Entity was read with Admin privilege." : "BMC.UserBadge Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserBadge", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserBadge entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserBadge.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserBadge.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserBadge record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserBadge/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserBadge(int id, [FromBody]Database.UserBadge.UserBadgeDTO userBadgeDTO, CancellationToken cancellationToken = default)
		{
			if (userBadgeDTO == null)
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



			if (id != userBadgeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.UserBadge> query = (from x in _context.UserBadges
				where
				(x.id == id)
				select x);


			Database.UserBadge existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserBadge PUT", id.ToString(), new Exception("No BMC.UserBadge entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userBadgeDTO.objectGuid == Guid.Empty)
            {
                userBadgeDTO.objectGuid = existing.objectGuid;
            }
            else if (userBadgeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserBadge record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserBadge cloneOfExisting = (Database.UserBadge)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserBadge object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserBadge userBadge = (Database.UserBadge)_context.Entry(existing).GetDatabaseValues().ToObject();
			userBadge.ApplyDTO(userBadgeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userBadge.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserBadge record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (userBadge.name != null && userBadge.name.Length > 100)
			{
				userBadge.name = userBadge.name.Substring(0, 100);
			}

			if (userBadge.description != null && userBadge.description.Length > 500)
			{
				userBadge.description = userBadge.description.Substring(0, 500);
			}

			if (userBadge.iconCssClass != null && userBadge.iconCssClass.Length > 100)
			{
				userBadge.iconCssClass = userBadge.iconCssClass.Substring(0, 100);
			}

			if (userBadge.iconImagePath != null && userBadge.iconImagePath.Length > 250)
			{
				userBadge.iconImagePath = userBadge.iconImagePath.Substring(0, 250);
			}

			if (userBadge.badgeColor != null && userBadge.badgeColor.Length > 10)
			{
				userBadge.badgeColor = userBadge.badgeColor.Substring(0, 10);
			}

			if (userBadge.automaticCriteriaCode != null && userBadge.automaticCriteriaCode.Length > 250)
			{
				userBadge.automaticCriteriaCode = userBadge.automaticCriteriaCode.Substring(0, 250);
			}

			EntityEntry<Database.UserBadge> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userBadge);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserBadge entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserBadge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserBadge.CreateAnonymousWithFirstLevelSubObjects(userBadge)),
					null);


				return Ok(Database.UserBadge.CreateAnonymous(userBadge));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserBadge entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserBadge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserBadge.CreateAnonymousWithFirstLevelSubObjects(userBadge)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserBadge record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserBadge", Name = "UserBadge")]
		public async Task<IActionResult> PostUserBadge([FromBody]Database.UserBadge.UserBadgeDTO userBadgeDTO, CancellationToken cancellationToken = default)
		{
			if (userBadgeDTO == null)
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
			// Create a new UserBadge object using the data from the DTO
			//
			Database.UserBadge userBadge = Database.UserBadge.FromDTO(userBadgeDTO);

			try
			{
				if (userBadge.name != null && userBadge.name.Length > 100)
				{
					userBadge.name = userBadge.name.Substring(0, 100);
				}

				if (userBadge.description != null && userBadge.description.Length > 500)
				{
					userBadge.description = userBadge.description.Substring(0, 500);
				}

				if (userBadge.iconCssClass != null && userBadge.iconCssClass.Length > 100)
				{
					userBadge.iconCssClass = userBadge.iconCssClass.Substring(0, 100);
				}

				if (userBadge.iconImagePath != null && userBadge.iconImagePath.Length > 250)
				{
					userBadge.iconImagePath = userBadge.iconImagePath.Substring(0, 250);
				}

				if (userBadge.badgeColor != null && userBadge.badgeColor.Length > 10)
				{
					userBadge.badgeColor = userBadge.badgeColor.Substring(0, 10);
				}

				if (userBadge.automaticCriteriaCode != null && userBadge.automaticCriteriaCode.Length > 250)
				{
					userBadge.automaticCriteriaCode = userBadge.automaticCriteriaCode.Substring(0, 250);
				}

				userBadge.objectGuid = Guid.NewGuid();
				_context.UserBadges.Add(userBadge);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserBadge entity successfully created.",
					true,
					userBadge.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserBadge.CreateAnonymousWithFirstLevelSubObjects(userBadge)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserBadge entity creation failed.", false, userBadge.id.ToString(), "", JsonSerializer.Serialize(userBadge), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserBadge", userBadge.id, userBadge.name));

			return CreatedAtRoute("UserBadge", new { id = userBadge.id }, Database.UserBadge.CreateAnonymousWithFirstLevelSubObjects(userBadge));
		}



        /// <summary>
        /// 
        /// This deletes a UserBadge record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserBadge/{id}")]
		[Route("api/UserBadge")]
		public async Task<IActionResult> DeleteUserBadge(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.UserBadge> query = (from x in _context.UserBadges
				where
				(x.id == id)
				select x);


			Database.UserBadge userBadge = await query.FirstOrDefaultAsync(cancellationToken);

			if (userBadge == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserBadge DELETE", id.ToString(), new Exception("No BMC.UserBadge entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserBadge cloneOfExisting = (Database.UserBadge)_context.Entry(userBadge).GetDatabaseValues().ToObject();


			try
			{
				userBadge.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserBadge entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserBadge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserBadge.CreateAnonymousWithFirstLevelSubObjects(userBadge)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserBadge entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserBadge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserBadge.CreateAnonymousWithFirstLevelSubObjects(userBadge)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserBadge records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserBadges/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string iconCssClass = null,
			string iconImagePath = null,
			string badgeColor = null,
			bool? isAutomatic = null,
			string automaticCriteriaCode = null,
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

			IQueryable<Database.UserBadge> query = (from ub in _context.UserBadges select ub);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ub => ub.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ub => ub.description == description);
			}
			if (string.IsNullOrEmpty(iconCssClass) == false)
			{
				query = query.Where(ub => ub.iconCssClass == iconCssClass);
			}
			if (string.IsNullOrEmpty(iconImagePath) == false)
			{
				query = query.Where(ub => ub.iconImagePath == iconImagePath);
			}
			if (string.IsNullOrEmpty(badgeColor) == false)
			{
				query = query.Where(ub => ub.badgeColor == badgeColor);
			}
			if (isAutomatic.HasValue == true)
			{
				query = query.Where(ub => ub.isAutomatic == isAutomatic.Value);
			}
			if (string.IsNullOrEmpty(automaticCriteriaCode) == false)
			{
				query = query.Where(ub => ub.automaticCriteriaCode == automaticCriteriaCode);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ub => ub.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ub => ub.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ub => ub.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ub => ub.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ub => ub.deleted == false);
				}
			}
			else
			{
				query = query.Where(ub => ub.active == true);
				query = query.Where(ub => ub.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Badge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.iconCssClass.Contains(anyStringContains)
			       || x.iconImagePath.Contains(anyStringContains)
			       || x.badgeColor.Contains(anyStringContains)
			       || x.automaticCriteriaCode.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.iconCssClass);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserBadge.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserBadge/CreateAuditEvent")]
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
