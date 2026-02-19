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
    /// This auto generated class provides the basic CRUD operations for the BuildChallengeEntry entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BuildChallengeEntry entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BuildChallengeEntriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<BuildChallengeEntriesController> _logger;

		public BuildChallengeEntriesController(BMCContext context, ILogger<BuildChallengeEntriesController> logger) : base("BMC", "BuildChallengeEntry")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BuildChallengeEntries filtered by the parameters provided.
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
		[Route("api/BuildChallengeEntries")]
		public async Task<IActionResult> GetBuildChallengeEntries(
			int? buildChallengeId = null,
			int? publishedMocId = null,
			DateTime? submittedDate = null,
			string entryNotes = null,
			int? voteCount = null,
			bool? isWinner = null,
			bool? isDisqualified = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (submittedDate.HasValue == true && submittedDate.Value.Kind != DateTimeKind.Utc)
			{
				submittedDate = submittedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BuildChallengeEntry> query = (from bce in _context.BuildChallengeEntries select bce);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildChallengeId.HasValue == true)
			{
				query = query.Where(bce => bce.buildChallengeId == buildChallengeId.Value);
			}
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(bce => bce.publishedMocId == publishedMocId.Value);
			}
			if (submittedDate.HasValue == true)
			{
				query = query.Where(bce => bce.submittedDate == submittedDate.Value);
			}
			if (string.IsNullOrEmpty(entryNotes) == false)
			{
				query = query.Where(bce => bce.entryNotes == entryNotes);
			}
			if (voteCount.HasValue == true)
			{
				query = query.Where(bce => bce.voteCount == voteCount.Value);
			}
			if (isWinner.HasValue == true)
			{
				query = query.Where(bce => bce.isWinner == isWinner.Value);
			}
			if (isDisqualified.HasValue == true)
			{
				query = query.Where(bce => bce.isDisqualified == isDisqualified.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bce => bce.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bce => bce.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bce => bce.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bce => bce.deleted == false);
				}
			}
			else
			{
				query = query.Where(bce => bce.active == true);
				query = query.Where(bce => bce.deleted == false);
			}

			query = query.OrderBy(bce => bce.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.buildChallenge);
				query = query.Include(x => x.publishedMoc);
				query = query.AsSplitQuery();
			}

			query = query.AsNoTracking();
			
			List<Database.BuildChallengeEntry> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BuildChallengeEntry buildChallengeEntry in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(buildChallengeEntry, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BuildChallengeEntry Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BuildChallengeEntry Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BuildChallengeEntries filtered by the parameters provided.  Its query is similar to the GetBuildChallengeEntries method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallengeEntries/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? buildChallengeId = null,
			int? publishedMocId = null,
			DateTime? submittedDate = null,
			string entryNotes = null,
			int? voteCount = null,
			bool? isWinner = null,
			bool? isDisqualified = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (submittedDate.HasValue == true && submittedDate.Value.Kind != DateTimeKind.Utc)
			{
				submittedDate = submittedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BuildChallengeEntry> query = (from bce in _context.BuildChallengeEntries select bce);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (buildChallengeId.HasValue == true)
			{
				query = query.Where(bce => bce.buildChallengeId == buildChallengeId.Value);
			}
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(bce => bce.publishedMocId == publishedMocId.Value);
			}
			if (submittedDate.HasValue == true)
			{
				query = query.Where(bce => bce.submittedDate == submittedDate.Value);
			}
			if (entryNotes != null)
			{
				query = query.Where(bce => bce.entryNotes == entryNotes);
			}
			if (voteCount.HasValue == true)
			{
				query = query.Where(bce => bce.voteCount == voteCount.Value);
			}
			if (isWinner.HasValue == true)
			{
				query = query.Where(bce => bce.isWinner == isWinner.Value);
			}
			if (isDisqualified.HasValue == true)
			{
				query = query.Where(bce => bce.isDisqualified == isDisqualified.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bce => bce.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bce => bce.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bce => bce.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bce => bce.deleted == false);
				}
			}
			else
			{
				query = query.Where(bce => bce.active == true);
				query = query.Where(bce => bce.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BuildChallengeEntry by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallengeEntry/{id}")]
		public async Task<IActionResult> GetBuildChallengeEntry(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BuildChallengeEntry> query = (from bce in _context.BuildChallengeEntries where
							(bce.id == id) &&
							(userIsAdmin == true || bce.deleted == false) &&
							(userIsWriter == true || bce.active == true)
					select bce);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.buildChallenge);
					query = query.Include(x => x.publishedMoc);
					query = query.AsSplitQuery();
				}

				Database.BuildChallengeEntry materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BuildChallengeEntry Entity was read with Admin privilege." : "BMC.BuildChallengeEntry Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildChallengeEntry", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BuildChallengeEntry entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BuildChallengeEntry.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BuildChallengeEntry.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BuildChallengeEntry record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BuildChallengeEntry/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBuildChallengeEntry(int id, [FromBody]Database.BuildChallengeEntry.BuildChallengeEntryDTO buildChallengeEntryDTO, CancellationToken cancellationToken = default)
		{
			if (buildChallengeEntryDTO == null)
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



			if (id != buildChallengeEntryDTO.id)
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


			IQueryable<Database.BuildChallengeEntry> query = (from x in _context.BuildChallengeEntries
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BuildChallengeEntry existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildChallengeEntry PUT", id.ToString(), new Exception("No BMC.BuildChallengeEntry entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (buildChallengeEntryDTO.objectGuid == Guid.Empty)
            {
                buildChallengeEntryDTO.objectGuid = existing.objectGuid;
            }
            else if (buildChallengeEntryDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BuildChallengeEntry record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BuildChallengeEntry cloneOfExisting = (Database.BuildChallengeEntry)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BuildChallengeEntry object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BuildChallengeEntry buildChallengeEntry = (Database.BuildChallengeEntry)_context.Entry(existing).GetDatabaseValues().ToObject();
			buildChallengeEntry.ApplyDTO(buildChallengeEntryDTO);
			//
			// The tenant guid for any BuildChallengeEntry being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BuildChallengeEntry because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				buildChallengeEntry.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (buildChallengeEntry.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BuildChallengeEntry record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (buildChallengeEntry.submittedDate.Kind != DateTimeKind.Utc)
			{
				buildChallengeEntry.submittedDate = buildChallengeEntry.submittedDate.ToUniversalTime();
			}

			EntityEntry<Database.BuildChallengeEntry> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(buildChallengeEntry);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BuildChallengeEntry entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildChallengeEntry.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildChallengeEntry.CreateAnonymousWithFirstLevelSubObjects(buildChallengeEntry)),
					null);


				return Ok(Database.BuildChallengeEntry.CreateAnonymous(buildChallengeEntry));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BuildChallengeEntry entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildChallengeEntry.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildChallengeEntry.CreateAnonymousWithFirstLevelSubObjects(buildChallengeEntry)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BuildChallengeEntry record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallengeEntry", Name = "BuildChallengeEntry")]
		public async Task<IActionResult> PostBuildChallengeEntry([FromBody]Database.BuildChallengeEntry.BuildChallengeEntryDTO buildChallengeEntryDTO, CancellationToken cancellationToken = default)
		{
			if (buildChallengeEntryDTO == null)
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
			// Create a new BuildChallengeEntry object using the data from the DTO
			//
			Database.BuildChallengeEntry buildChallengeEntry = Database.BuildChallengeEntry.FromDTO(buildChallengeEntryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				buildChallengeEntry.tenantGuid = userTenantGuid;

				if (buildChallengeEntry.submittedDate.Kind != DateTimeKind.Utc)
				{
					buildChallengeEntry.submittedDate = buildChallengeEntry.submittedDate.ToUniversalTime();
				}

				buildChallengeEntry.objectGuid = Guid.NewGuid();
				_context.BuildChallengeEntries.Add(buildChallengeEntry);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BuildChallengeEntry entity successfully created.",
					true,
					buildChallengeEntry.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BuildChallengeEntry.CreateAnonymousWithFirstLevelSubObjects(buildChallengeEntry)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BuildChallengeEntry entity creation failed.", false, buildChallengeEntry.id.ToString(), "", JsonSerializer.Serialize(buildChallengeEntry), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildChallengeEntry", buildChallengeEntry.id, buildChallengeEntry.id.ToString()));

			return CreatedAtRoute("BuildChallengeEntry", new { id = buildChallengeEntry.id }, Database.BuildChallengeEntry.CreateAnonymousWithFirstLevelSubObjects(buildChallengeEntry));
		}



        /// <summary>
        /// 
        /// This deletes a BuildChallengeEntry record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallengeEntry/{id}")]
		[Route("api/BuildChallengeEntry")]
		public async Task<IActionResult> DeleteBuildChallengeEntry(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BuildChallengeEntry> query = (from x in _context.BuildChallengeEntries
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BuildChallengeEntry buildChallengeEntry = await query.FirstOrDefaultAsync(cancellationToken);

			if (buildChallengeEntry == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildChallengeEntry DELETE", id.ToString(), new Exception("No BMC.BuildChallengeEntry entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BuildChallengeEntry cloneOfExisting = (Database.BuildChallengeEntry)_context.Entry(buildChallengeEntry).GetDatabaseValues().ToObject();


			try
			{
				buildChallengeEntry.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BuildChallengeEntry entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildChallengeEntry.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildChallengeEntry.CreateAnonymousWithFirstLevelSubObjects(buildChallengeEntry)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BuildChallengeEntry entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildChallengeEntry.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildChallengeEntry.CreateAnonymousWithFirstLevelSubObjects(buildChallengeEntry)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BuildChallengeEntry records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BuildChallengeEntries/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? buildChallengeId = null,
			int? publishedMocId = null,
			DateTime? submittedDate = null,
			string entryNotes = null,
			int? voteCount = null,
			bool? isWinner = null,
			bool? isDisqualified = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (submittedDate.HasValue == true && submittedDate.Value.Kind != DateTimeKind.Utc)
			{
				submittedDate = submittedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BuildChallengeEntry> query = (from bce in _context.BuildChallengeEntries select bce);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildChallengeId.HasValue == true)
			{
				query = query.Where(bce => bce.buildChallengeId == buildChallengeId.Value);
			}
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(bce => bce.publishedMocId == publishedMocId.Value);
			}
			if (submittedDate.HasValue == true)
			{
				query = query.Where(bce => bce.submittedDate == submittedDate.Value);
			}
			if (string.IsNullOrEmpty(entryNotes) == false)
			{
				query = query.Where(bce => bce.entryNotes == entryNotes);
			}
			if (voteCount.HasValue == true)
			{
				query = query.Where(bce => bce.voteCount == voteCount.Value);
			}
			if (isWinner.HasValue == true)
			{
				query = query.Where(bce => bce.isWinner == isWinner.Value);
			}
			if (isDisqualified.HasValue == true)
			{
				query = query.Where(bce => bce.isDisqualified == isDisqualified.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bce => bce.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bce => bce.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bce => bce.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bce => bce.deleted == false);
				}
			}
			else
			{
				query = query.Where(bce => bce.active == true);
				query = query.Where(bce => bce.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BuildChallengeEntry.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BuildChallengeEntry/CreateAuditEvent")]
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
