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
    /// This auto generated class provides the basic CRUD operations for the Achievement entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Achievement entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AchievementsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<AchievementsController> _logger;

		public AchievementsController(BMCContext context, ILogger<AchievementsController> logger) : base("BMC", "Achievement")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Achievements filtered by the parameters provided.
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
		[Route("api/Achievements")]
		public async Task<IActionResult> GetAchievements(
			int? achievementCategoryId = null,
			string name = null,
			string description = null,
			string iconCssClass = null,
			string iconImagePath = null,
			string criteria = null,
			string criteriaCode = null,
			int? pointValue = null,
			string rarity = null,
			bool? isActive = null,
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

			IQueryable<Database.Achievement> query = (from a in _context.Achievements select a);
			if (achievementCategoryId.HasValue == true)
			{
				query = query.Where(a => a.achievementCategoryId == achievementCategoryId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(a => a.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(a => a.description == description);
			}
			if (string.IsNullOrEmpty(iconCssClass) == false)
			{
				query = query.Where(a => a.iconCssClass == iconCssClass);
			}
			if (string.IsNullOrEmpty(iconImagePath) == false)
			{
				query = query.Where(a => a.iconImagePath == iconImagePath);
			}
			if (string.IsNullOrEmpty(criteria) == false)
			{
				query = query.Where(a => a.criteria == criteria);
			}
			if (string.IsNullOrEmpty(criteriaCode) == false)
			{
				query = query.Where(a => a.criteriaCode == criteriaCode);
			}
			if (pointValue.HasValue == true)
			{
				query = query.Where(a => a.pointValue == pointValue.Value);
			}
			if (string.IsNullOrEmpty(rarity) == false)
			{
				query = query.Where(a => a.rarity == rarity);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(a => a.isActive == isActive.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(a => a.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(a => a.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(a => a.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(a => a.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(a => a.deleted == false);
				}
			}
			else
			{
				query = query.Where(a => a.active == true);
				query = query.Where(a => a.deleted == false);
			}

			query = query.OrderBy(a => a.sequence).ThenBy(a => a.name).ThenBy(a => a.description).ThenBy(a => a.iconCssClass);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.achievementCategory);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Achievement, or on an any of the string fields on its immediate relations
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
			       || x.criteria.Contains(anyStringContains)
			       || x.criteriaCode.Contains(anyStringContains)
			       || x.rarity.Contains(anyStringContains)
			       || (includeRelations == true && x.achievementCategory.name.Contains(anyStringContains))
			       || (includeRelations == true && x.achievementCategory.description.Contains(anyStringContains))
			       || (includeRelations == true && x.achievementCategory.iconCssClass.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Achievement> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Achievement achievement in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(achievement, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.Achievement Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.Achievement Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Achievements filtered by the parameters provided.  Its query is similar to the GetAchievements method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Achievements/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? achievementCategoryId = null,
			string name = null,
			string description = null,
			string iconCssClass = null,
			string iconImagePath = null,
			string criteria = null,
			string criteriaCode = null,
			int? pointValue = null,
			string rarity = null,
			bool? isActive = null,
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

			IQueryable<Database.Achievement> query = (from a in _context.Achievements select a);
			if (achievementCategoryId.HasValue == true)
			{
				query = query.Where(a => a.achievementCategoryId == achievementCategoryId.Value);
			}
			if (name != null)
			{
				query = query.Where(a => a.name == name);
			}
			if (description != null)
			{
				query = query.Where(a => a.description == description);
			}
			if (iconCssClass != null)
			{
				query = query.Where(a => a.iconCssClass == iconCssClass);
			}
			if (iconImagePath != null)
			{
				query = query.Where(a => a.iconImagePath == iconImagePath);
			}
			if (criteria != null)
			{
				query = query.Where(a => a.criteria == criteria);
			}
			if (criteriaCode != null)
			{
				query = query.Where(a => a.criteriaCode == criteriaCode);
			}
			if (pointValue.HasValue == true)
			{
				query = query.Where(a => a.pointValue == pointValue.Value);
			}
			if (rarity != null)
			{
				query = query.Where(a => a.rarity == rarity);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(a => a.isActive == isActive.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(a => a.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(a => a.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(a => a.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(a => a.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(a => a.deleted == false);
				}
			}
			else
			{
				query = query.Where(a => a.active == true);
				query = query.Where(a => a.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Achievement, or on an any of the string fields on its immediate relations
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
			       || x.criteria.Contains(anyStringContains)
			       || x.criteriaCode.Contains(anyStringContains)
			       || x.rarity.Contains(anyStringContains)
			       || x.achievementCategory.name.Contains(anyStringContains)
			       || x.achievementCategory.description.Contains(anyStringContains)
			       || x.achievementCategory.iconCssClass.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Achievement by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Achievement/{id}")]
		public async Task<IActionResult> GetAchievement(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Achievement> query = (from a in _context.Achievements where
							(a.id == id) &&
							(userIsAdmin == true || a.deleted == false) &&
							(userIsWriter == true || a.active == true)
					select a);

				if (includeRelations == true)
				{
					query = query.Include(x => x.achievementCategory);
					query = query.AsSplitQuery();
				}

				Database.Achievement materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.Achievement Entity was read with Admin privilege." : "BMC.Achievement Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Achievement", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.Achievement entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.Achievement.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.Achievement.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing Achievement record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Achievement/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAchievement(int id, [FromBody]Database.Achievement.AchievementDTO achievementDTO, CancellationToken cancellationToken = default)
		{
			if (achievementDTO == null)
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



			if (id != achievementDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.Achievement> query = (from x in _context.Achievements
				where
				(x.id == id)
				select x);


			Database.Achievement existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.Achievement PUT", id.ToString(), new Exception("No BMC.Achievement entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (achievementDTO.objectGuid == Guid.Empty)
            {
                achievementDTO.objectGuid = existing.objectGuid;
            }
            else if (achievementDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Achievement record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Achievement cloneOfExisting = (Database.Achievement)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Achievement object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Achievement achievement = (Database.Achievement)_context.Entry(existing).GetDatabaseValues().ToObject();
			achievement.ApplyDTO(achievementDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (achievement.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.Achievement record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (achievement.name != null && achievement.name.Length > 100)
			{
				achievement.name = achievement.name.Substring(0, 100);
			}

			if (achievement.description != null && achievement.description.Length > 500)
			{
				achievement.description = achievement.description.Substring(0, 500);
			}

			if (achievement.iconCssClass != null && achievement.iconCssClass.Length > 100)
			{
				achievement.iconCssClass = achievement.iconCssClass.Substring(0, 100);
			}

			if (achievement.iconImagePath != null && achievement.iconImagePath.Length > 250)
			{
				achievement.iconImagePath = achievement.iconImagePath.Substring(0, 250);
			}

			if (achievement.criteriaCode != null && achievement.criteriaCode.Length > 250)
			{
				achievement.criteriaCode = achievement.criteriaCode.Substring(0, 250);
			}

			if (achievement.rarity != null && achievement.rarity.Length > 50)
			{
				achievement.rarity = achievement.rarity.Substring(0, 50);
			}

			EntityEntry<Database.Achievement> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(achievement);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.Achievement entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.Achievement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Achievement.CreateAnonymousWithFirstLevelSubObjects(achievement)),
					null);


				return Ok(Database.Achievement.CreateAnonymous(achievement));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.Achievement entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.Achievement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Achievement.CreateAnonymousWithFirstLevelSubObjects(achievement)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new Achievement record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Achievement", Name = "Achievement")]
		public async Task<IActionResult> PostAchievement([FromBody]Database.Achievement.AchievementDTO achievementDTO, CancellationToken cancellationToken = default)
		{
			if (achievementDTO == null)
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
			// Create a new Achievement object using the data from the DTO
			//
			Database.Achievement achievement = Database.Achievement.FromDTO(achievementDTO);

			try
			{
				if (achievement.name != null && achievement.name.Length > 100)
				{
					achievement.name = achievement.name.Substring(0, 100);
				}

				if (achievement.description != null && achievement.description.Length > 500)
				{
					achievement.description = achievement.description.Substring(0, 500);
				}

				if (achievement.iconCssClass != null && achievement.iconCssClass.Length > 100)
				{
					achievement.iconCssClass = achievement.iconCssClass.Substring(0, 100);
				}

				if (achievement.iconImagePath != null && achievement.iconImagePath.Length > 250)
				{
					achievement.iconImagePath = achievement.iconImagePath.Substring(0, 250);
				}

				if (achievement.criteriaCode != null && achievement.criteriaCode.Length > 250)
				{
					achievement.criteriaCode = achievement.criteriaCode.Substring(0, 250);
				}

				if (achievement.rarity != null && achievement.rarity.Length > 50)
				{
					achievement.rarity = achievement.rarity.Substring(0, 50);
				}

				achievement.objectGuid = Guid.NewGuid();
				_context.Achievements.Add(achievement);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.Achievement entity successfully created.",
					true,
					achievement.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.Achievement.CreateAnonymousWithFirstLevelSubObjects(achievement)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.Achievement entity creation failed.", false, achievement.id.ToString(), "", JsonSerializer.Serialize(achievement), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Achievement", achievement.id, achievement.name));

			return CreatedAtRoute("Achievement", new { id = achievement.id }, Database.Achievement.CreateAnonymousWithFirstLevelSubObjects(achievement));
		}



        /// <summary>
        /// 
        /// This deletes a Achievement record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Achievement/{id}")]
		[Route("api/Achievement")]
		public async Task<IActionResult> DeleteAchievement(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Achievement> query = (from x in _context.Achievements
				where
				(x.id == id)
				select x);


			Database.Achievement achievement = await query.FirstOrDefaultAsync(cancellationToken);

			if (achievement == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.Achievement DELETE", id.ToString(), new Exception("No BMC.Achievement entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Achievement cloneOfExisting = (Database.Achievement)_context.Entry(achievement).GetDatabaseValues().ToObject();


			try
			{
				achievement.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.Achievement entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.Achievement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Achievement.CreateAnonymousWithFirstLevelSubObjects(achievement)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.Achievement entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.Achievement.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.Achievement.CreateAnonymousWithFirstLevelSubObjects(achievement)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of Achievement records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Achievements/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? achievementCategoryId = null,
			string name = null,
			string description = null,
			string iconCssClass = null,
			string iconImagePath = null,
			string criteria = null,
			string criteriaCode = null,
			int? pointValue = null,
			string rarity = null,
			bool? isActive = null,
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

			IQueryable<Database.Achievement> query = (from a in _context.Achievements select a);
			if (achievementCategoryId.HasValue == true)
			{
				query = query.Where(a => a.achievementCategoryId == achievementCategoryId.Value);
			}
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(a => a.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(a => a.description == description);
			}
			if (string.IsNullOrEmpty(iconCssClass) == false)
			{
				query = query.Where(a => a.iconCssClass == iconCssClass);
			}
			if (string.IsNullOrEmpty(iconImagePath) == false)
			{
				query = query.Where(a => a.iconImagePath == iconImagePath);
			}
			if (string.IsNullOrEmpty(criteria) == false)
			{
				query = query.Where(a => a.criteria == criteria);
			}
			if (string.IsNullOrEmpty(criteriaCode) == false)
			{
				query = query.Where(a => a.criteriaCode == criteriaCode);
			}
			if (pointValue.HasValue == true)
			{
				query = query.Where(a => a.pointValue == pointValue.Value);
			}
			if (string.IsNullOrEmpty(rarity) == false)
			{
				query = query.Where(a => a.rarity == rarity);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(a => a.isActive == isActive.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(a => a.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(a => a.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(a => a.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(a => a.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(a => a.deleted == false);
				}
			}
			else
			{
				query = query.Where(a => a.active == true);
				query = query.Where(a => a.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Achievement, or on an any of the string fields on its immediate relations
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
			       || x.criteria.Contains(anyStringContains)
			       || x.criteriaCode.Contains(anyStringContains)
			       || x.rarity.Contains(anyStringContains)
			       || x.achievementCategory.name.Contains(anyStringContains)
			       || x.achievementCategory.description.Contains(anyStringContains)
			       || x.achievementCategory.iconCssClass.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.iconCssClass);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Achievement.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Achievement/CreateAuditEvent")]
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
