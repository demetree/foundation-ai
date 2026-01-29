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
using static Foundation.Auditor.AuditEngine;
using Foundation.Security.Database;

namespace Foundation.Security.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the SecurityUserSecurityGroup entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the SecurityUserSecurityGroup entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class SecurityUserSecurityGroupsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SecurityContext _context;

		private ILogger<SecurityUserSecurityGroupsController> _logger;

		public SecurityUserSecurityGroupsController(SecurityContext context, ILogger<SecurityUserSecurityGroupsController> logger) : base("Security", "SecurityUserSecurityGroup")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of SecurityUserSecurityGroups filtered by the parameters provided.
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
		[Route("api/SecurityUserSecurityGroups")]
		public async Task<IActionResult> GetSecurityUserSecurityGroups(
			int? securityUserId = null,
			int? securityGroupId = null,
			string comments = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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

			IQueryable<Database.SecurityUserSecurityGroup> query = (from susg in _context.SecurityUserSecurityGroups select susg);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(susg => susg.securityUserId == securityUserId.Value);
			}
			if (securityGroupId.HasValue == true)
			{
				query = query.Where(susg => susg.securityGroupId == securityGroupId.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(susg => susg.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(susg => susg.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(susg => susg.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(susg => susg.deleted == false);
				}
			}
			else
			{
				query = query.Where(susg => susg.active == true);
				query = query.Where(susg => susg.deleted == false);
			}

			query = query.OrderBy(susg => susg.comments);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.securityGroup);
				query = query.Include(x => x.securityUser);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security User Security Group, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || (includeRelations == true && x.securityGroup.name.Contains(anyStringContains))
			       || (includeRelations == true && x.securityGroup.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.accountName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.password.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.firstName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.middleName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.lastName.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.emailAddress.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.cellPhoneNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.phoneNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.phoneExtension.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.description.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.authenticationDomain.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.alternateIdentifier.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.settings.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.authenticationToken.Contains(anyStringContains))
			       || (includeRelations == true && x.securityUser.twoFactorToken.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.SecurityUserSecurityGroup> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.SecurityUserSecurityGroup securityUserSecurityGroup in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityUserSecurityGroup, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.SecurityUserSecurityGroup Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.SecurityUserSecurityGroup Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of SecurityUserSecurityGroups filtered by the parameters provided.  Its query is similar to the GetSecurityUserSecurityGroups method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserSecurityGroups/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? securityUserId = null,
			int? securityGroupId = null,
			string comments = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.SecurityUserSecurityGroup> query = (from susg in _context.SecurityUserSecurityGroups select susg);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(susg => susg.securityUserId == securityUserId.Value);
			}
			if (securityGroupId.HasValue == true)
			{
				query = query.Where(susg => susg.securityGroupId == securityGroupId.Value);
			}
			if (comments != null)
			{
				query = query.Where(susg => susg.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(susg => susg.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(susg => susg.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(susg => susg.deleted == false);
				}
			}
			else
			{
				query = query.Where(susg => susg.active == true);
				query = query.Where(susg => susg.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Security User Security Group, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || x.securityGroup.name.Contains(anyStringContains)
			       || x.securityGroup.description.Contains(anyStringContains)
			       || x.securityUser.accountName.Contains(anyStringContains)
			       || x.securityUser.password.Contains(anyStringContains)
			       || x.securityUser.firstName.Contains(anyStringContains)
			       || x.securityUser.middleName.Contains(anyStringContains)
			       || x.securityUser.lastName.Contains(anyStringContains)
			       || x.securityUser.emailAddress.Contains(anyStringContains)
			       || x.securityUser.cellPhoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneExtension.Contains(anyStringContains)
			       || x.securityUser.description.Contains(anyStringContains)
			       || x.securityUser.authenticationDomain.Contains(anyStringContains)
			       || x.securityUser.alternateIdentifier.Contains(anyStringContains)
			       || x.securityUser.settings.Contains(anyStringContains)
			       || x.securityUser.authenticationToken.Contains(anyStringContains)
			       || x.securityUser.twoFactorToken.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single SecurityUserSecurityGroup by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserSecurityGroup/{id}")]
		public async Task<IActionResult> GetSecurityUserSecurityGroup(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.SecurityUserSecurityGroup> query = (from susg in _context.SecurityUserSecurityGroups where
							(susg.id == id) &&
							(userIsAdmin == true || susg.deleted == false) &&
							(userIsWriter == true || susg.active == true)
					select susg);

				if (includeRelations == true)
				{
					query = query.Include(x => x.securityGroup);
					query = query.Include(x => x.securityUser);
					query = query.AsSplitQuery();
				}

				Database.SecurityUserSecurityGroup materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.SecurityUserSecurityGroup Entity was read with Admin privilege." : "Security.SecurityUserSecurityGroup Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUserSecurityGroup", materialized.id, materialized.comments));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Security.SecurityUserSecurityGroup entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.SecurityUserSecurityGroup.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.SecurityUserSecurityGroup.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing SecurityUserSecurityGroup record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/SecurityUserSecurityGroup/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutSecurityUserSecurityGroup(int id, [FromBody]Database.SecurityUserSecurityGroup.SecurityUserSecurityGroupDTO securityUserSecurityGroupDTO, CancellationToken cancellationToken = default)
		{
			if (securityUserSecurityGroupDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != securityUserSecurityGroupDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.SecurityUserSecurityGroup> query = (from x in _context.SecurityUserSecurityGroups
				where
				(x.id == id)
				select x);


			Database.SecurityUserSecurityGroup existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUserSecurityGroup PUT", id.ToString(), new Exception("No Security.SecurityUserSecurityGroup entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.SecurityUserSecurityGroup cloneOfExisting = (Database.SecurityUserSecurityGroup)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new SecurityUserSecurityGroup object using the data from the existing record, updated with what is in the DTO.
			//
			Database.SecurityUserSecurityGroup securityUserSecurityGroup = (Database.SecurityUserSecurityGroup)_context.Entry(existing).GetDatabaseValues().ToObject();
			securityUserSecurityGroup.ApplyDTO(securityUserSecurityGroupDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (securityUserSecurityGroup.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityUserSecurityGroup record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (securityUserSecurityGroup.comments != null && securityUserSecurityGroup.comments.Length > 1000)
			{
				securityUserSecurityGroup.comments = securityUserSecurityGroup.comments.Substring(0, 1000);
			}

			EntityEntry<Database.SecurityUserSecurityGroup> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(securityUserSecurityGroup);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityUserSecurityGroup entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserSecurityGroup.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserSecurityGroup.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityGroup)),
					null);


				return Ok(Database.SecurityUserSecurityGroup.CreateAnonymous(securityUserSecurityGroup));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Security.SecurityUserSecurityGroup entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserSecurityGroup.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserSecurityGroup.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityGroup)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new SecurityUserSecurityGroup record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserSecurityGroup", Name = "SecurityUserSecurityGroup")]
		public async Task<IActionResult> PostSecurityUserSecurityGroup([FromBody]Database.SecurityUserSecurityGroup.SecurityUserSecurityGroupDTO securityUserSecurityGroupDTO, CancellationToken cancellationToken = default)
		{
			if (securityUserSecurityGroupDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new SecurityUserSecurityGroup object using the data from the DTO
			//
			Database.SecurityUserSecurityGroup securityUserSecurityGroup = Database.SecurityUserSecurityGroup.FromDTO(securityUserSecurityGroupDTO);

			try
			{
				if (securityUserSecurityGroup.comments != null && securityUserSecurityGroup.comments.Length > 1000)
				{
					securityUserSecurityGroup.comments = securityUserSecurityGroup.comments.Substring(0, 1000);
				}

				_context.SecurityUserSecurityGroups.Add(securityUserSecurityGroup);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Security.SecurityUserSecurityGroup entity successfully created.",
					true,
					securityUserSecurityGroup.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.SecurityUserSecurityGroup.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityGroup)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityUserSecurityGroup entity creation failed.", false, securityUserSecurityGroup.id.ToString(), "", JsonSerializer.Serialize(securityUserSecurityGroup), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUserSecurityGroup", securityUserSecurityGroup.id, securityUserSecurityGroup.comments));

			return CreatedAtRoute("SecurityUserSecurityGroup", new { id = securityUserSecurityGroup.id }, Database.SecurityUserSecurityGroup.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityGroup));
		}



        /// <summary>
        /// 
        /// This deletes a SecurityUserSecurityGroup record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/SecurityUserSecurityGroup/{id}")]
		[Route("api/SecurityUserSecurityGroup")]
		public async Task<IActionResult> DeleteSecurityUserSecurityGroup(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.SecurityUserSecurityGroup> query = (from x in _context.SecurityUserSecurityGroups
				where
				(x.id == id)
				select x);


			Database.SecurityUserSecurityGroup securityUserSecurityGroup = await query.FirstOrDefaultAsync(cancellationToken);

			if (securityUserSecurityGroup == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUserSecurityGroup DELETE", id.ToString(), new Exception("No Security.SecurityUserSecurityGroup entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.SecurityUserSecurityGroup cloneOfExisting = (Database.SecurityUserSecurityGroup)_context.Entry(securityUserSecurityGroup).GetDatabaseValues().ToObject();


			try
			{
				securityUserSecurityGroup.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityUserSecurityGroup entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserSecurityGroup.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserSecurityGroup.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityGroup)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Security.SecurityUserSecurityGroup entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.SecurityUserSecurityGroup.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.SecurityUserSecurityGroup.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityGroup)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of SecurityUserSecurityGroup records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/SecurityUserSecurityGroups/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? securityUserId = null,
			int? securityGroupId = null,
			string comments = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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

			IQueryable<Database.SecurityUserSecurityGroup> query = (from susg in _context.SecurityUserSecurityGroups select susg);
			if (securityUserId.HasValue == true)
			{
				query = query.Where(susg => susg.securityUserId == securityUserId.Value);
			}
			if (securityGroupId.HasValue == true)
			{
				query = query.Where(susg => susg.securityGroupId == securityGroupId.Value);
			}
			if (string.IsNullOrEmpty(comments) == false)
			{
				query = query.Where(susg => susg.comments == comments);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(susg => susg.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(susg => susg.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(susg => susg.deleted == false);
				}
			}
			else
			{
				query = query.Where(susg => susg.active == true);
				query = query.Where(susg => susg.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Security User Security Group, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.comments.Contains(anyStringContains)
			       || x.securityGroup.name.Contains(anyStringContains)
			       || x.securityGroup.description.Contains(anyStringContains)
			       || x.securityUser.accountName.Contains(anyStringContains)
			       || x.securityUser.password.Contains(anyStringContains)
			       || x.securityUser.firstName.Contains(anyStringContains)
			       || x.securityUser.middleName.Contains(anyStringContains)
			       || x.securityUser.lastName.Contains(anyStringContains)
			       || x.securityUser.emailAddress.Contains(anyStringContains)
			       || x.securityUser.cellPhoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneNumber.Contains(anyStringContains)
			       || x.securityUser.phoneExtension.Contains(anyStringContains)
			       || x.securityUser.description.Contains(anyStringContains)
			       || x.securityUser.authenticationDomain.Contains(anyStringContains)
			       || x.securityUser.alternateIdentifier.Contains(anyStringContains)
			       || x.securityUser.settings.Contains(anyStringContains)
			       || x.securityUser.authenticationToken.Contains(anyStringContains)
			       || x.securityUser.twoFactorToken.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.comments);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.SecurityUserSecurityGroup.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/SecurityUserSecurityGroup/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
