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
    /// This auto generated class provides the basic CRUD operations for the UserProfileLink entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the UserProfileLink entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class UserProfileLinksController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<UserProfileLinksController> _logger;

		public UserProfileLinksController(BMCContext context, ILogger<UserProfileLinksController> logger) : base("BMC", "UserProfileLink")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of UserProfileLinks filtered by the parameters provided.
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
		[Route("api/UserProfileLinks")]
		public async Task<IActionResult> GetUserProfileLinks(
			int? userProfileId = null,
			int? userProfileLinkTypeId = null,
			string url = null,
			string displayLabel = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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

			IQueryable<Database.UserProfileLink> query = (from upl in _context.UserProfileLinks select upl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userProfileId.HasValue == true)
			{
				query = query.Where(upl => upl.userProfileId == userProfileId.Value);
			}
			if (userProfileLinkTypeId.HasValue == true)
			{
				query = query.Where(upl => upl.userProfileLinkTypeId == userProfileLinkTypeId.Value);
			}
			if (string.IsNullOrEmpty(url) == false)
			{
				query = query.Where(upl => upl.url == url);
			}
			if (string.IsNullOrEmpty(displayLabel) == false)
			{
				query = query.Where(upl => upl.displayLabel == displayLabel);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(upl => upl.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(upl => upl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(upl => upl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(upl => upl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(upl => upl.deleted == false);
				}
			}
			else
			{
				query = query.Where(upl => upl.active == true);
				query = query.Where(upl => upl.deleted == false);
			}

			query = query.OrderBy(upl => upl.sequence).ThenBy(upl => upl.url).ThenBy(upl => upl.displayLabel);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.userProfile);
				query = query.Include(x => x.userProfileLinkType);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Profile Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.url.Contains(anyStringContains)
			       || x.displayLabel.Contains(anyStringContains)
			       || (includeRelations == true && x.userProfile.displayName.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.bio.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.location.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.avatarImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.profileBannerImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfile.websiteUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfileLinkType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfileLinkType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.userProfileLinkType.iconCssClass.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.UserProfileLink> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.UserProfileLink userProfileLink in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(userProfileLink, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.UserProfileLink Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.UserProfileLink Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of UserProfileLinks filtered by the parameters provided.  Its query is similar to the GetUserProfileLinks method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileLinks/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? userProfileId = null,
			int? userProfileLinkTypeId = null,
			string url = null,
			string displayLabel = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.UserProfileLink> query = (from upl in _context.UserProfileLinks select upl);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (userProfileId.HasValue == true)
			{
				query = query.Where(upl => upl.userProfileId == userProfileId.Value);
			}
			if (userProfileLinkTypeId.HasValue == true)
			{
				query = query.Where(upl => upl.userProfileLinkTypeId == userProfileLinkTypeId.Value);
			}
			if (url != null)
			{
				query = query.Where(upl => upl.url == url);
			}
			if (displayLabel != null)
			{
				query = query.Where(upl => upl.displayLabel == displayLabel);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(upl => upl.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(upl => upl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(upl => upl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(upl => upl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(upl => upl.deleted == false);
				}
			}
			else
			{
				query = query.Where(upl => upl.active == true);
				query = query.Where(upl => upl.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the User Profile Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.url.Contains(anyStringContains)
			       || x.displayLabel.Contains(anyStringContains)
			       || x.userProfile.displayName.Contains(anyStringContains)
			       || x.userProfile.bio.Contains(anyStringContains)
			       || x.userProfile.location.Contains(anyStringContains)
			       || x.userProfile.avatarImagePath.Contains(anyStringContains)
			       || x.userProfile.profileBannerImagePath.Contains(anyStringContains)
			       || x.userProfile.websiteUrl.Contains(anyStringContains)
			       || x.userProfileLinkType.name.Contains(anyStringContains)
			       || x.userProfileLinkType.description.Contains(anyStringContains)
			       || x.userProfileLinkType.iconCssClass.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single UserProfileLink by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileLink/{id}")]
		public async Task<IActionResult> GetUserProfileLink(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.UserProfileLink> query = (from upl in _context.UserProfileLinks where
							(upl.id == id) &&
							(userIsAdmin == true || upl.deleted == false) &&
							(userIsWriter == true || upl.active == true)
					select upl);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.userProfile);
					query = query.Include(x => x.userProfileLinkType);
					query = query.AsSplitQuery();
				}

				Database.UserProfileLink materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.UserProfileLink Entity was read with Admin privilege." : "BMC.UserProfileLink Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserProfileLink", materialized.id, materialized.url));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.UserProfileLink entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.UserProfileLink.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.UserProfileLink.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing UserProfileLink record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/UserProfileLink/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutUserProfileLink(int id, [FromBody]Database.UserProfileLink.UserProfileLinkDTO userProfileLinkDTO, CancellationToken cancellationToken = default)
		{
			if (userProfileLinkDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != userProfileLinkDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.UserProfileLink> query = (from x in _context.UserProfileLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserProfileLink existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfileLink PUT", id.ToString(), new Exception("No BMC.UserProfileLink entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (userProfileLinkDTO.objectGuid == Guid.Empty)
            {
                userProfileLinkDTO.objectGuid = existing.objectGuid;
            }
            else if (userProfileLinkDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserProfileLink record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.UserProfileLink cloneOfExisting = (Database.UserProfileLink)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new UserProfileLink object using the data from the existing record, updated with what is in the DTO.
			//
			Database.UserProfileLink userProfileLink = (Database.UserProfileLink)_context.Entry(existing).GetDatabaseValues().ToObject();
			userProfileLink.ApplyDTO(userProfileLinkDTO);
			//
			// The tenant guid for any UserProfileLink being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the UserProfileLink because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				userProfileLink.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (userProfileLink.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserProfileLink record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (userProfileLink.url != null && userProfileLink.url.Length > 500)
			{
				userProfileLink.url = userProfileLink.url.Substring(0, 500);
			}

			if (userProfileLink.displayLabel != null && userProfileLink.displayLabel.Length > 100)
			{
				userProfileLink.displayLabel = userProfileLink.displayLabel.Substring(0, 100);
			}

			EntityEntry<Database.UserProfileLink> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(userProfileLink);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserProfileLink entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileLink.CreateAnonymousWithFirstLevelSubObjects(userProfileLink)),
					null);


				return Ok(Database.UserProfileLink.CreateAnonymous(userProfileLink));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.UserProfileLink entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileLink.CreateAnonymousWithFirstLevelSubObjects(userProfileLink)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new UserProfileLink record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileLink", Name = "UserProfileLink")]
		public async Task<IActionResult> PostUserProfileLink([FromBody]Database.UserProfileLink.UserProfileLinkDTO userProfileLinkDTO, CancellationToken cancellationToken = default)
		{
			if (userProfileLinkDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new UserProfileLink object using the data from the DTO
			//
			Database.UserProfileLink userProfileLink = Database.UserProfileLink.FromDTO(userProfileLinkDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				userProfileLink.tenantGuid = userTenantGuid;

				if (userProfileLink.url != null && userProfileLink.url.Length > 500)
				{
					userProfileLink.url = userProfileLink.url.Substring(0, 500);
				}

				if (userProfileLink.displayLabel != null && userProfileLink.displayLabel.Length > 100)
				{
					userProfileLink.displayLabel = userProfileLink.displayLabel.Substring(0, 100);
				}

				userProfileLink.objectGuid = Guid.NewGuid();
				_context.UserProfileLinks.Add(userProfileLink);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.UserProfileLink entity successfully created.",
					true,
					userProfileLink.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.UserProfileLink.CreateAnonymousWithFirstLevelSubObjects(userProfileLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserProfileLink entity creation failed.", false, userProfileLink.id.ToString(), "", JsonSerializer.Serialize(userProfileLink), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserProfileLink", userProfileLink.id, userProfileLink.url));

			return CreatedAtRoute("UserProfileLink", new { id = userProfileLink.id }, Database.UserProfileLink.CreateAnonymousWithFirstLevelSubObjects(userProfileLink));
		}



        /// <summary>
        /// 
        /// This deletes a UserProfileLink record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/UserProfileLink/{id}")]
		[Route("api/UserProfileLink")]
		public async Task<IActionResult> DeleteUserProfileLink(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.UserProfileLink> query = (from x in _context.UserProfileLinks
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.UserProfileLink userProfileLink = await query.FirstOrDefaultAsync(cancellationToken);

			if (userProfileLink == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserProfileLink DELETE", id.ToString(), new Exception("No BMC.UserProfileLink entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.UserProfileLink cloneOfExisting = (Database.UserProfileLink)_context.Entry(userProfileLink).GetDatabaseValues().ToObject();


			try
			{
				userProfileLink.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserProfileLink entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileLink.CreateAnonymousWithFirstLevelSubObjects(userProfileLink)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.UserProfileLink entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.UserProfileLink.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.UserProfileLink.CreateAnonymousWithFirstLevelSubObjects(userProfileLink)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of UserProfileLink records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/UserProfileLinks/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? userProfileId = null,
			int? userProfileLinkTypeId = null,
			string url = null,
			string displayLabel = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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

			IQueryable<Database.UserProfileLink> query = (from upl in _context.UserProfileLinks select upl);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (userProfileId.HasValue == true)
			{
				query = query.Where(upl => upl.userProfileId == userProfileId.Value);
			}
			if (userProfileLinkTypeId.HasValue == true)
			{
				query = query.Where(upl => upl.userProfileLinkTypeId == userProfileLinkTypeId.Value);
			}
			if (string.IsNullOrEmpty(url) == false)
			{
				query = query.Where(upl => upl.url == url);
			}
			if (string.IsNullOrEmpty(displayLabel) == false)
			{
				query = query.Where(upl => upl.displayLabel == displayLabel);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(upl => upl.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(upl => upl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(upl => upl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(upl => upl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(upl => upl.deleted == false);
				}
			}
			else
			{
				query = query.Where(upl => upl.active == true);
				query = query.Where(upl => upl.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the User Profile Link, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.url.Contains(anyStringContains)
			       || x.displayLabel.Contains(anyStringContains)
			       || x.userProfile.displayName.Contains(anyStringContains)
			       || x.userProfile.bio.Contains(anyStringContains)
			       || x.userProfile.location.Contains(anyStringContains)
			       || x.userProfile.avatarImagePath.Contains(anyStringContains)
			       || x.userProfile.profileBannerImagePath.Contains(anyStringContains)
			       || x.userProfile.websiteUrl.Contains(anyStringContains)
			       || x.userProfileLinkType.name.Contains(anyStringContains)
			       || x.userProfileLinkType.description.Contains(anyStringContains)
			       || x.userProfileLinkType.iconCssClass.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.url).ThenBy(x => x.displayLabel);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.UserProfileLink.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/UserProfileLink/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
