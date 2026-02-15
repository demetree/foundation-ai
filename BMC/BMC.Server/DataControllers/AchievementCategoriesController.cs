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
    /// This auto generated class provides the basic CRUD operations for the AchievementCategory entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AchievementCategory entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AchievementCategoriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<AchievementCategoriesController> _logger;

		public AchievementCategoriesController(BMCContext context, ILogger<AchievementCategoriesController> logger) : base("BMC", "AchievementCategory")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AchievementCategories filtered by the parameters provided.
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
		[Route("api/AchievementCategories")]
		public async Task<IActionResult> GetAchievementCategories(
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

			IQueryable<Database.AchievementCategory> query = (from ac in _context.AchievementCategories select ac);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ac => ac.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ac => ac.description == description);
			}
			if (string.IsNullOrEmpty(iconCssClass) == false)
			{
				query = query.Where(ac => ac.iconCssClass == iconCssClass);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ac => ac.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ac => ac.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ac => ac.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ac => ac.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ac => ac.deleted == false);
				}
			}
			else
			{
				query = query.Where(ac => ac.active == true);
				query = query.Where(ac => ac.deleted == false);
			}

			query = query.OrderBy(ac => ac.sequence).ThenBy(ac => ac.name).ThenBy(ac => ac.description).ThenBy(ac => ac.iconCssClass);

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
			// Add the any string contains parameter to span all the string fields on the Achievement Category, or on an any of the string fields on its immediate relations
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
			
			List<Database.AchievementCategory> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AchievementCategory achievementCategory in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(achievementCategory, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.AchievementCategory Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.AchievementCategory Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AchievementCategories filtered by the parameters provided.  Its query is similar to the GetAchievementCategories method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AchievementCategories/RowCount")]
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

			IQueryable<Database.AchievementCategory> query = (from ac in _context.AchievementCategories select ac);
			if (name != null)
			{
				query = query.Where(ac => ac.name == name);
			}
			if (description != null)
			{
				query = query.Where(ac => ac.description == description);
			}
			if (iconCssClass != null)
			{
				query = query.Where(ac => ac.iconCssClass == iconCssClass);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ac => ac.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ac => ac.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ac => ac.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ac => ac.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ac => ac.deleted == false);
				}
			}
			else
			{
				query = query.Where(ac => ac.active == true);
				query = query.Where(ac => ac.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Achievement Category, or on an any of the string fields on its immediate relations
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
        /// This gets a single AchievementCategory by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AchievementCategory/{id}")]
		public async Task<IActionResult> GetAchievementCategory(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.AchievementCategory> query = (from ac in _context.AchievementCategories where
							(ac.id == id) &&
							(userIsAdmin == true || ac.deleted == false) &&
							(userIsWriter == true || ac.active == true)
					select ac);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.AchievementCategory materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.AchievementCategory Entity was read with Admin privilege." : "BMC.AchievementCategory Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AchievementCategory", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.AchievementCategory entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.AchievementCategory.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.AchievementCategory.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AchievementCategory record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AchievementCategory/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAchievementCategory(int id, [FromBody]Database.AchievementCategory.AchievementCategoryDTO achievementCategoryDTO, CancellationToken cancellationToken = default)
		{
			if (achievementCategoryDTO == null)
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



			if (id != achievementCategoryDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AchievementCategory> query = (from x in _context.AchievementCategories
				where
				(x.id == id)
				select x);


			Database.AchievementCategory existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.AchievementCategory PUT", id.ToString(), new Exception("No BMC.AchievementCategory entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (achievementCategoryDTO.objectGuid == Guid.Empty)
            {
                achievementCategoryDTO.objectGuid = existing.objectGuid;
            }
            else if (achievementCategoryDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a AchievementCategory record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AchievementCategory cloneOfExisting = (Database.AchievementCategory)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AchievementCategory object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AchievementCategory achievementCategory = (Database.AchievementCategory)_context.Entry(existing).GetDatabaseValues().ToObject();
			achievementCategory.ApplyDTO(achievementCategoryDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (achievementCategory.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.AchievementCategory record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (achievementCategory.name != null && achievementCategory.name.Length > 100)
			{
				achievementCategory.name = achievementCategory.name.Substring(0, 100);
			}

			if (achievementCategory.description != null && achievementCategory.description.Length > 500)
			{
				achievementCategory.description = achievementCategory.description.Substring(0, 500);
			}

			if (achievementCategory.iconCssClass != null && achievementCategory.iconCssClass.Length > 100)
			{
				achievementCategory.iconCssClass = achievementCategory.iconCssClass.Substring(0, 100);
			}

			EntityEntry<Database.AchievementCategory> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(achievementCategory);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.AchievementCategory entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AchievementCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AchievementCategory.CreateAnonymousWithFirstLevelSubObjects(achievementCategory)),
					null);


				return Ok(Database.AchievementCategory.CreateAnonymous(achievementCategory));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.AchievementCategory entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AchievementCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AchievementCategory.CreateAnonymousWithFirstLevelSubObjects(achievementCategory)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AchievementCategory record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AchievementCategory", Name = "AchievementCategory")]
		public async Task<IActionResult> PostAchievementCategory([FromBody]Database.AchievementCategory.AchievementCategoryDTO achievementCategoryDTO, CancellationToken cancellationToken = default)
		{
			if (achievementCategoryDTO == null)
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
			// Create a new AchievementCategory object using the data from the DTO
			//
			Database.AchievementCategory achievementCategory = Database.AchievementCategory.FromDTO(achievementCategoryDTO);

			try
			{
				if (achievementCategory.name != null && achievementCategory.name.Length > 100)
				{
					achievementCategory.name = achievementCategory.name.Substring(0, 100);
				}

				if (achievementCategory.description != null && achievementCategory.description.Length > 500)
				{
					achievementCategory.description = achievementCategory.description.Substring(0, 500);
				}

				if (achievementCategory.iconCssClass != null && achievementCategory.iconCssClass.Length > 100)
				{
					achievementCategory.iconCssClass = achievementCategory.iconCssClass.Substring(0, 100);
				}

				achievementCategory.objectGuid = Guid.NewGuid();
				_context.AchievementCategories.Add(achievementCategory);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.AchievementCategory entity successfully created.",
					true,
					achievementCategory.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AchievementCategory.CreateAnonymousWithFirstLevelSubObjects(achievementCategory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.AchievementCategory entity creation failed.", false, achievementCategory.id.ToString(), "", JsonSerializer.Serialize(achievementCategory), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AchievementCategory", achievementCategory.id, achievementCategory.name));

			return CreatedAtRoute("AchievementCategory", new { id = achievementCategory.id }, Database.AchievementCategory.CreateAnonymousWithFirstLevelSubObjects(achievementCategory));
		}



        /// <summary>
        /// 
        /// This deletes a AchievementCategory record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AchievementCategory/{id}")]
		[Route("api/AchievementCategory")]
		public async Task<IActionResult> DeleteAchievementCategory(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.AchievementCategory> query = (from x in _context.AchievementCategories
				where
				(x.id == id)
				select x);


			Database.AchievementCategory achievementCategory = await query.FirstOrDefaultAsync(cancellationToken);

			if (achievementCategory == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.AchievementCategory DELETE", id.ToString(), new Exception("No BMC.AchievementCategory entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AchievementCategory cloneOfExisting = (Database.AchievementCategory)_context.Entry(achievementCategory).GetDatabaseValues().ToObject();


			try
			{
				achievementCategory.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.AchievementCategory entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AchievementCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AchievementCategory.CreateAnonymousWithFirstLevelSubObjects(achievementCategory)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.AchievementCategory entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AchievementCategory.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AchievementCategory.CreateAnonymousWithFirstLevelSubObjects(achievementCategory)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AchievementCategory records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AchievementCategories/ListData")]
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

			IQueryable<Database.AchievementCategory> query = (from ac in _context.AchievementCategories select ac);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ac => ac.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ac => ac.description == description);
			}
			if (string.IsNullOrEmpty(iconCssClass) == false)
			{
				query = query.Where(ac => ac.iconCssClass == iconCssClass);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ac => ac.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ac => ac.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ac => ac.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ac => ac.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ac => ac.deleted == false);
				}
			}
			else
			{
				query = query.Where(ac => ac.active == true);
				query = query.Where(ac => ac.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Achievement Category, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.AchievementCategory.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AchievementCategory/CreateAuditEvent")]
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
