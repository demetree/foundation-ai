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
using Foundation.Community.Database;

namespace Foundation.Community.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the MenuItem entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MenuItem entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MenuItemsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private CommunityContext _context;

		private ILogger<MenuItemsController> _logger;

		public MenuItemsController(CommunityContext context, ILogger<MenuItemsController> logger) : base("Community", "MenuItem")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MenuItems filtered by the parameters provided.
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
		[Route("api/MenuItems")]
		public async Task<IActionResult> GetMenuItems(
			int? menuId = null,
			string label = null,
			string url = null,
			int? pageId = null,
			int? parentMenuItemId = null,
			string iconClass = null,
			bool? openInNewTab = null,
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
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

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

			IQueryable<Database.MenuItem> query = (from mi in _context.MenuItems select mi);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (menuId.HasValue == true)
			{
				query = query.Where(mi => mi.menuId == menuId.Value);
			}
			if (string.IsNullOrEmpty(label) == false)
			{
				query = query.Where(mi => mi.label == label);
			}
			if (string.IsNullOrEmpty(url) == false)
			{
				query = query.Where(mi => mi.url == url);
			}
			if (pageId.HasValue == true)
			{
				query = query.Where(mi => mi.pageId == pageId.Value);
			}
			if (parentMenuItemId.HasValue == true)
			{
				query = query.Where(mi => mi.parentMenuItemId == parentMenuItemId.Value);
			}
			if (string.IsNullOrEmpty(iconClass) == false)
			{
				query = query.Where(mi => mi.iconClass == iconClass);
			}
			if (openInNewTab.HasValue == true)
			{
				query = query.Where(mi => mi.openInNewTab == openInNewTab.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(mi => mi.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mi => mi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mi => mi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mi => mi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mi => mi.deleted == false);
				}
			}
			else
			{
				query = query.Where(mi => mi.active == true);
				query = query.Where(mi => mi.deleted == false);
			}

			query = query.OrderBy(mi => mi.sequence).ThenBy(mi => mi.label).ThenBy(mi => mi.url).ThenBy(mi => mi.iconClass);


			//
			// Add the any string contains parameter to span all the string fields on the Menu Item, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.label.Contains(anyStringContains)
			       || x.url.Contains(anyStringContains)
			       || x.iconClass.Contains(anyStringContains)
			       || (includeRelations == true && x.menu.name.Contains(anyStringContains))
			       || (includeRelations == true && x.menu.location.Contains(anyStringContains))
			       || (includeRelations == true && x.page.title.Contains(anyStringContains))
			       || (includeRelations == true && x.page.slug.Contains(anyStringContains))
			       || (includeRelations == true && x.page.body.Contains(anyStringContains))
			       || (includeRelations == true && x.page.metaDescription.Contains(anyStringContains))
			       || (includeRelations == true && x.page.featuredImageUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.parentMenuItem.label.Contains(anyStringContains))
			       || (includeRelations == true && x.parentMenuItem.url.Contains(anyStringContains))
			       || (includeRelations == true && x.parentMenuItem.iconClass.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.menu);
				query = query.Include(x => x.page);
				query = query.Include(x => x.parentMenuItem);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.MenuItem> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MenuItem menuItem in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(menuItem, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Community.MenuItem Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Community.MenuItem Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MenuItems filtered by the parameters provided.  Its query is similar to the GetMenuItems method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MenuItems/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? menuId = null,
			string label = null,
			string url = null,
			int? pageId = null,
			int? parentMenuItemId = null,
			string iconClass = null,
			bool? openInNewTab = null,
			int? sequence = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			IQueryable<Database.MenuItem> query = (from mi in _context.MenuItems select mi);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (menuId.HasValue == true)
			{
				query = query.Where(mi => mi.menuId == menuId.Value);
			}
			if (label != null)
			{
				query = query.Where(mi => mi.label == label);
			}
			if (url != null)
			{
				query = query.Where(mi => mi.url == url);
			}
			if (pageId.HasValue == true)
			{
				query = query.Where(mi => mi.pageId == pageId.Value);
			}
			if (parentMenuItemId.HasValue == true)
			{
				query = query.Where(mi => mi.parentMenuItemId == parentMenuItemId.Value);
			}
			if (iconClass != null)
			{
				query = query.Where(mi => mi.iconClass == iconClass);
			}
			if (openInNewTab.HasValue == true)
			{
				query = query.Where(mi => mi.openInNewTab == openInNewTab.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(mi => mi.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mi => mi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mi => mi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mi => mi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mi => mi.deleted == false);
				}
			}
			else
			{
				query = query.Where(mi => mi.active == true);
				query = query.Where(mi => mi.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Menu Item, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.label.Contains(anyStringContains)
			       || x.url.Contains(anyStringContains)
			       || x.iconClass.Contains(anyStringContains)
			       || x.menu.name.Contains(anyStringContains)
			       || x.menu.location.Contains(anyStringContains)
			       || x.page.title.Contains(anyStringContains)
			       || x.page.slug.Contains(anyStringContains)
			       || x.page.body.Contains(anyStringContains)
			       || x.page.metaDescription.Contains(anyStringContains)
			       || x.page.featuredImageUrl.Contains(anyStringContains)
			       || x.parentMenuItem.label.Contains(anyStringContains)
			       || x.parentMenuItem.url.Contains(anyStringContains)
			       || x.parentMenuItem.iconClass.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MenuItem by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MenuItem/{id}")]
		public async Task<IActionResult> GetMenuItem(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			try
			{
				IQueryable<Database.MenuItem> query = (from mi in _context.MenuItems where
							(mi.id == id) &&
							(userIsAdmin == true || mi.deleted == false) &&
							(userIsWriter == true || mi.active == true)
					select mi);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.menu);
					query = query.Include(x => x.page);
					query = query.Include(x => x.parentMenuItem);
					query = query.AsSplitQuery();
				}

				Database.MenuItem materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Community.MenuItem Entity was read with Admin privilege." : "Community.MenuItem Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MenuItem", materialized.id, materialized.label));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Community.MenuItem entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Community.MenuItem.   Entity was read with Admin privilege." : "Exception caught during entity read of Community.MenuItem.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MenuItem record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MenuItem/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMenuItem(int id, [FromBody]Database.MenuItem.MenuItemDTO menuItemDTO, CancellationToken cancellationToken = default)
		{
			if (menuItemDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Navigation Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Navigation Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != menuItemDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


			IQueryable<Database.MenuItem> query = (from x in _context.MenuItems
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MenuItem existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.MenuItem PUT", id.ToString(), new Exception("No Community.MenuItem entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (menuItemDTO.objectGuid == Guid.Empty)
            {
                menuItemDTO.objectGuid = existing.objectGuid;
            }
            else if (menuItemDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MenuItem record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MenuItem cloneOfExisting = (Database.MenuItem)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MenuItem object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MenuItem menuItem = (Database.MenuItem)_context.Entry(existing).GetDatabaseValues().ToObject();
			menuItem.ApplyDTO(menuItemDTO);
			//
			// The tenant guid for any MenuItem being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the MenuItem because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				menuItem.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (menuItem.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Community.MenuItem record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (menuItem.label != null && menuItem.label.Length > 250)
			{
				menuItem.label = menuItem.label.Substring(0, 250);
			}

			if (menuItem.url != null && menuItem.url.Length > 500)
			{
				menuItem.url = menuItem.url.Substring(0, 500);
			}

			if (menuItem.iconClass != null && menuItem.iconClass.Length > 50)
			{
				menuItem.iconClass = menuItem.iconClass.Substring(0, 50);
			}

			EntityEntry<Database.MenuItem> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(menuItem);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Community.MenuItem entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MenuItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MenuItem.CreateAnonymousWithFirstLevelSubObjects(menuItem)),
					null);


				return Ok(Database.MenuItem.CreateAnonymous(menuItem));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Community.MenuItem entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MenuItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MenuItem.CreateAnonymousWithFirstLevelSubObjects(menuItem)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MenuItem record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MenuItem", Name = "MenuItem")]
		public async Task<IActionResult> PostMenuItem([FromBody]Database.MenuItem.MenuItemDTO menuItemDTO, CancellationToken cancellationToken = default)
		{
			if (menuItemDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Community Navigation Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Navigation Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			//
			// Create a new MenuItem object using the data from the DTO
			//
			Database.MenuItem menuItem = Database.MenuItem.FromDTO(menuItemDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				menuItem.tenantGuid = userTenantGuid;

				if (menuItem.label != null && menuItem.label.Length > 250)
				{
					menuItem.label = menuItem.label.Substring(0, 250);
				}

				if (menuItem.url != null && menuItem.url.Length > 500)
				{
					menuItem.url = menuItem.url.Substring(0, 500);
				}

				if (menuItem.iconClass != null && menuItem.iconClass.Length > 50)
				{
					menuItem.iconClass = menuItem.iconClass.Substring(0, 50);
				}

				menuItem.objectGuid = Guid.NewGuid();
				_context.MenuItems.Add(menuItem);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Community.MenuItem entity successfully created.",
					true,
					menuItem.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MenuItem.CreateAnonymousWithFirstLevelSubObjects(menuItem)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Community.MenuItem entity creation failed.", false, menuItem.id.ToString(), "", JsonSerializer.Serialize(menuItem), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MenuItem", menuItem.id, menuItem.label));

			return CreatedAtRoute("MenuItem", new { id = menuItem.id }, Database.MenuItem.CreateAnonymousWithFirstLevelSubObjects(menuItem));
		}



        /// <summary>
        /// 
        /// This deletes a MenuItem record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MenuItem/{id}")]
		[Route("api/MenuItem")]
		public async Task<IActionResult> DeleteMenuItem(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Community Navigation Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Navigation Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			
			
			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}

			IQueryable<Database.MenuItem> query = (from x in _context.MenuItems
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.MenuItem menuItem = await query.FirstOrDefaultAsync(cancellationToken);

			if (menuItem == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Community.MenuItem DELETE", id.ToString(), new Exception("No Community.MenuItem entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MenuItem cloneOfExisting = (Database.MenuItem)_context.Entry(menuItem).GetDatabaseValues().ToObject();


			try
			{
				menuItem.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.MenuItem entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MenuItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MenuItem.CreateAnonymousWithFirstLevelSubObjects(menuItem)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Community.MenuItem entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MenuItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MenuItem.CreateAnonymousWithFirstLevelSubObjects(menuItem)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MenuItem records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MenuItems/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? menuId = null,
			string label = null,
			string url = null,
			int? pageId = null,
			int? parentMenuItemId = null,
			string iconClass = null,
			bool? openInNewTab = null,
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
			// Community Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


			Guid userTenantGuid;

			try
			{
			    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
			}
			catch (Exception ex)
			{
			    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
			    return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
			}


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

			IQueryable<Database.MenuItem> query = (from mi in _context.MenuItems select mi);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (menuId.HasValue == true)
			{
				query = query.Where(mi => mi.menuId == menuId.Value);
			}
			if (string.IsNullOrEmpty(label) == false)
			{
				query = query.Where(mi => mi.label == label);
			}
			if (string.IsNullOrEmpty(url) == false)
			{
				query = query.Where(mi => mi.url == url);
			}
			if (pageId.HasValue == true)
			{
				query = query.Where(mi => mi.pageId == pageId.Value);
			}
			if (parentMenuItemId.HasValue == true)
			{
				query = query.Where(mi => mi.parentMenuItemId == parentMenuItemId.Value);
			}
			if (string.IsNullOrEmpty(iconClass) == false)
			{
				query = query.Where(mi => mi.iconClass == iconClass);
			}
			if (openInNewTab.HasValue == true)
			{
				query = query.Where(mi => mi.openInNewTab == openInNewTab.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(mi => mi.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mi => mi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mi => mi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mi => mi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mi => mi.deleted == false);
				}
			}
			else
			{
				query = query.Where(mi => mi.active == true);
				query = query.Where(mi => mi.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Menu Item, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.label.Contains(anyStringContains)
			       || x.url.Contains(anyStringContains)
			       || x.iconClass.Contains(anyStringContains)
			       || x.menu.name.Contains(anyStringContains)
			       || x.menu.location.Contains(anyStringContains)
			       || x.page.title.Contains(anyStringContains)
			       || x.page.slug.Contains(anyStringContains)
			       || x.page.body.Contains(anyStringContains)
			       || x.page.metaDescription.Contains(anyStringContains)
			       || x.page.featuredImageUrl.Contains(anyStringContains)
			       || x.parentMenuItem.label.Contains(anyStringContains)
			       || x.parentMenuItem.url.Contains(anyStringContains)
			       || x.parentMenuItem.iconClass.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.label).ThenBy(x => x.url).ThenBy(x => x.iconClass);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MenuItem.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MenuItem/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Community Navigation Writer role needed to write to this table, or Community Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Community Navigation Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
