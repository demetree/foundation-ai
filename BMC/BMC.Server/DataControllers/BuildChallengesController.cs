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
using Foundation.ChangeHistory;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the BuildChallenge entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BuildChallenge entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BuildChallengesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		static object buildChallengePutSyncRoot = new object();
		static object buildChallengeDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<BuildChallengesController> _logger;

		public BuildChallengesController(BMCContext context, ILogger<BuildChallengesController> logger) : base("BMC", "BuildChallenge")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BuildChallenges filtered by the parameters provided.
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
		[Route("api/BuildChallenges")]
		public async Task<IActionResult> GetBuildChallenges(
			string name = null,
			string description = null,
			string rules = null,
			string thumbnailImagePath = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			DateTime? votingEndDate = null,
			bool? isActive = null,
			bool? isFeatured = null,
			int? entryCount = null,
			int? maxPartsLimit = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (startDate.HasValue == true && startDate.Value.Kind != DateTimeKind.Utc)
			{
				startDate = startDate.Value.ToUniversalTime();
			}

			if (endDate.HasValue == true && endDate.Value.Kind != DateTimeKind.Utc)
			{
				endDate = endDate.Value.ToUniversalTime();
			}

			if (votingEndDate.HasValue == true && votingEndDate.Value.Kind != DateTimeKind.Utc)
			{
				votingEndDate = votingEndDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BuildChallenge> query = (from bc in _context.BuildChallenges select bc);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(bc => bc.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(bc => bc.description == description);
			}
			if (string.IsNullOrEmpty(rules) == false)
			{
				query = query.Where(bc => bc.rules == rules);
			}
			if (string.IsNullOrEmpty(thumbnailImagePath) == false)
			{
				query = query.Where(bc => bc.thumbnailImagePath == thumbnailImagePath);
			}
			if (startDate.HasValue == true)
			{
				query = query.Where(bc => bc.startDate == startDate.Value);
			}
			if (endDate.HasValue == true)
			{
				query = query.Where(bc => bc.endDate == endDate.Value);
			}
			if (votingEndDate.HasValue == true)
			{
				query = query.Where(bc => bc.votingEndDate == votingEndDate.Value);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(bc => bc.isActive == isActive.Value);
			}
			if (isFeatured.HasValue == true)
			{
				query = query.Where(bc => bc.isFeatured == isFeatured.Value);
			}
			if (entryCount.HasValue == true)
			{
				query = query.Where(bc => bc.entryCount == entryCount.Value);
			}
			if (maxPartsLimit.HasValue == true)
			{
				query = query.Where(bc => bc.maxPartsLimit == maxPartsLimit.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bc => bc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bc => bc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bc => bc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bc => bc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bc => bc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bc => bc.active == true);
				query = query.Where(bc => bc.deleted == false);
			}

			query = query.OrderBy(bc => bc.name).ThenBy(bc => bc.thumbnailImagePath);


			//
			// Add the any string contains parameter to span all the string fields on the Build Challenge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.rules.Contains(anyStringContains)
			       || x.thumbnailImagePath.Contains(anyStringContains)
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
			
			List<Database.BuildChallenge> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BuildChallenge buildChallenge in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(buildChallenge, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BuildChallenge Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BuildChallenge Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BuildChallenges filtered by the parameters provided.  Its query is similar to the GetBuildChallenges method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallenges/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string rules = null,
			string thumbnailImagePath = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			DateTime? votingEndDate = null,
			bool? isActive = null,
			bool? isFeatured = null,
			int? entryCount = null,
			int? maxPartsLimit = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (startDate.HasValue == true && startDate.Value.Kind != DateTimeKind.Utc)
			{
				startDate = startDate.Value.ToUniversalTime();
			}

			if (endDate.HasValue == true && endDate.Value.Kind != DateTimeKind.Utc)
			{
				endDate = endDate.Value.ToUniversalTime();
			}

			if (votingEndDate.HasValue == true && votingEndDate.Value.Kind != DateTimeKind.Utc)
			{
				votingEndDate = votingEndDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BuildChallenge> query = (from bc in _context.BuildChallenges select bc);
			if (name != null)
			{
				query = query.Where(bc => bc.name == name);
			}
			if (description != null)
			{
				query = query.Where(bc => bc.description == description);
			}
			if (rules != null)
			{
				query = query.Where(bc => bc.rules == rules);
			}
			if (thumbnailImagePath != null)
			{
				query = query.Where(bc => bc.thumbnailImagePath == thumbnailImagePath);
			}
			if (startDate.HasValue == true)
			{
				query = query.Where(bc => bc.startDate == startDate.Value);
			}
			if (endDate.HasValue == true)
			{
				query = query.Where(bc => bc.endDate == endDate.Value);
			}
			if (votingEndDate.HasValue == true)
			{
				query = query.Where(bc => bc.votingEndDate == votingEndDate.Value);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(bc => bc.isActive == isActive.Value);
			}
			if (isFeatured.HasValue == true)
			{
				query = query.Where(bc => bc.isFeatured == isFeatured.Value);
			}
			if (entryCount.HasValue == true)
			{
				query = query.Where(bc => bc.entryCount == entryCount.Value);
			}
			if (maxPartsLimit.HasValue == true)
			{
				query = query.Where(bc => bc.maxPartsLimit == maxPartsLimit.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bc => bc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bc => bc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bc => bc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bc => bc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bc => bc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bc => bc.active == true);
				query = query.Where(bc => bc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Build Challenge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.rules.Contains(anyStringContains)
			       || x.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BuildChallenge by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallenge/{id}")]
		public async Task<IActionResult> GetBuildChallenge(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.BuildChallenge> query = (from bc in _context.BuildChallenges where
							(bc.id == id) &&
							(userIsAdmin == true || bc.deleted == false) &&
							(userIsWriter == true || bc.active == true)
					select bc);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.BuildChallenge materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BuildChallenge Entity was read with Admin privilege." : "BMC.BuildChallenge Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildChallenge", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BuildChallenge entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BuildChallenge.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BuildChallenge.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BuildChallenge record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BuildChallenge/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBuildChallenge(int id, [FromBody]Database.BuildChallenge.BuildChallengeDTO buildChallengeDTO, CancellationToken cancellationToken = default)
		{
			if (buildChallengeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Moderator role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Moderator", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != buildChallengeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.BuildChallenge> query = (from x in _context.BuildChallenges
				where
				(x.id == id)
				select x);


			Database.BuildChallenge existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildChallenge PUT", id.ToString(), new Exception("No BMC.BuildChallenge entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (buildChallengeDTO.objectGuid == Guid.Empty)
            {
                buildChallengeDTO.objectGuid = existing.objectGuid;
            }
            else if (buildChallengeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BuildChallenge record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BuildChallenge cloneOfExisting = (Database.BuildChallenge)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BuildChallenge object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BuildChallenge buildChallenge = (Database.BuildChallenge)_context.Entry(existing).GetDatabaseValues().ToObject();
			buildChallenge.ApplyDTO(buildChallengeDTO);
			lock (buildChallengePutSyncRoot)
			{
				//
				// Validate the version number for the buildChallenge being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != buildChallenge.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "BuildChallenge save attempt was made but save request was with version " + buildChallenge.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The BuildChallenge you are trying to update has already changed.  Please try your save again after reloading the BuildChallenge.");
				}
				else
				{
					// Same record.  Increase version.
					buildChallenge.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (buildChallenge.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BuildChallenge record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (buildChallenge.name != null && buildChallenge.name.Length > 100)
				{
					buildChallenge.name = buildChallenge.name.Substring(0, 100);
				}

				if (buildChallenge.thumbnailImagePath != null && buildChallenge.thumbnailImagePath.Length > 250)
				{
					buildChallenge.thumbnailImagePath = buildChallenge.thumbnailImagePath.Substring(0, 250);
				}

				if (buildChallenge.startDate.Kind != DateTimeKind.Utc)
				{
					buildChallenge.startDate = buildChallenge.startDate.ToUniversalTime();
				}

				if (buildChallenge.endDate.Kind != DateTimeKind.Utc)
				{
					buildChallenge.endDate = buildChallenge.endDate.ToUniversalTime();
				}

				if (buildChallenge.votingEndDate.HasValue == true && buildChallenge.votingEndDate.Value.Kind != DateTimeKind.Utc)
				{
					buildChallenge.votingEndDate = buildChallenge.votingEndDate.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.BuildChallenge> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(buildChallenge);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BuildChallengeChangeHistory buildChallengeChangeHistory = new BuildChallengeChangeHistory();
				        buildChallengeChangeHistory.buildChallengeId = buildChallenge.id;
				        buildChallengeChangeHistory.versionNumber = buildChallenge.versionNumber;
				        buildChallengeChangeHistory.timeStamp = DateTime.UtcNow;
				        buildChallengeChangeHistory.userId = securityUser.id;
				        buildChallengeChangeHistory.data = JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge));
				        _context.BuildChallengeChangeHistories.Add(buildChallengeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BuildChallenge entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge)),
						null);

				return Ok(Database.BuildChallenge.CreateAnonymous(buildChallenge));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BuildChallenge entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new BuildChallenge record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallenge", Name = "BuildChallenge")]
		public async Task<IActionResult> PostBuildChallenge([FromBody]Database.BuildChallenge.BuildChallengeDTO buildChallengeDTO, CancellationToken cancellationToken = default)
		{
			if (buildChallengeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Moderator role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Moderator", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new BuildChallenge object using the data from the DTO
			//
			Database.BuildChallenge buildChallenge = Database.BuildChallenge.FromDTO(buildChallengeDTO);

			try
			{
				if (buildChallenge.name != null && buildChallenge.name.Length > 100)
				{
					buildChallenge.name = buildChallenge.name.Substring(0, 100);
				}

				if (buildChallenge.thumbnailImagePath != null && buildChallenge.thumbnailImagePath.Length > 250)
				{
					buildChallenge.thumbnailImagePath = buildChallenge.thumbnailImagePath.Substring(0, 250);
				}

				if (buildChallenge.startDate.Kind != DateTimeKind.Utc)
				{
					buildChallenge.startDate = buildChallenge.startDate.ToUniversalTime();
				}

				if (buildChallenge.endDate.Kind != DateTimeKind.Utc)
				{
					buildChallenge.endDate = buildChallenge.endDate.ToUniversalTime();
				}

				if (buildChallenge.votingEndDate.HasValue == true && buildChallenge.votingEndDate.Value.Kind != DateTimeKind.Utc)
				{
					buildChallenge.votingEndDate = buildChallenge.votingEndDate.Value.ToUniversalTime();
				}

				buildChallenge.objectGuid = Guid.NewGuid();
				buildChallenge.versionNumber = 1;

				_context.BuildChallenges.Add(buildChallenge);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the buildChallenge object so that no further changes will be written to the database
				    //
				    _context.Entry(buildChallenge).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					buildChallenge.BuildChallengeChangeHistories = null;
					buildChallenge.BuildChallengeEntries = null;


				    BuildChallengeChangeHistory buildChallengeChangeHistory = new BuildChallengeChangeHistory();
				    buildChallengeChangeHistory.buildChallengeId = buildChallenge.id;
				    buildChallengeChangeHistory.versionNumber = buildChallenge.versionNumber;
				    buildChallengeChangeHistory.timeStamp = DateTime.UtcNow;
				    buildChallengeChangeHistory.userId = securityUser.id;
				    buildChallengeChangeHistory.data = JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge));
				    _context.BuildChallengeChangeHistories.Add(buildChallengeChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.BuildChallenge entity successfully created.",
						true,
						buildChallenge. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BuildChallenge entity creation failed.", false, buildChallenge.id.ToString(), "", JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildChallenge", buildChallenge.id, buildChallenge.name));

			return CreatedAtRoute("BuildChallenge", new { id = buildChallenge.id }, Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge));
		}



        /// <summary>
        /// 
        /// This rolls a BuildChallenge entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallenge/Rollback/{id}")]
		[Route("api/BuildChallenge/Rollback")]
		public async Task<IActionResult> RollbackToBuildChallengeVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
			//
			// Data rollback is an admin only function, like Deletes.
			//
			StartAuditEventClock();
			
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			
			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
			
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			

			
			IQueryable <Database.BuildChallenge> query = (from x in _context.BuildChallenges
			        where
			        (x.id == id)
			        select x);


			//
			// Make sure nobody else is editing this BuildChallenge concurrently
			//
			lock (buildChallengePutSyncRoot)
			{
				
				Database.BuildChallenge buildChallenge = query.FirstOrDefault();
				
				if (buildChallenge == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildChallenge rollback", id.ToString(), new Exception("No BMC.BuildChallenge entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the BuildChallenge current state so we can log it.
				//
				Database.BuildChallenge cloneOfExisting = (Database.BuildChallenge)_context.Entry(buildChallenge).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.BuildChallengeChangeHistories = null;
				cloneOfExisting.BuildChallengeEntries = null;

				if (versionNumber >= buildChallenge.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.BuildChallenge rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.BuildChallenge rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				BuildChallengeChangeHistory buildChallengeChangeHistory = (from x in _context.BuildChallengeChangeHistories
				                                               where
				                                               x.buildChallengeId == id &&
				                                               x.versionNumber == versionNumber
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (buildChallengeChangeHistory != null)
				{
				    Database.BuildChallenge oldBuildChallenge = JsonSerializer.Deserialize<Database.BuildChallenge>(buildChallengeChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    buildChallenge.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    buildChallenge.name = oldBuildChallenge.name;
				    buildChallenge.description = oldBuildChallenge.description;
				    buildChallenge.rules = oldBuildChallenge.rules;
				    buildChallenge.thumbnailImagePath = oldBuildChallenge.thumbnailImagePath;
				    buildChallenge.startDate = oldBuildChallenge.startDate;
				    buildChallenge.endDate = oldBuildChallenge.endDate;
				    buildChallenge.votingEndDate = oldBuildChallenge.votingEndDate;
				    buildChallenge.isActive = oldBuildChallenge.isActive;
				    buildChallenge.isFeatured = oldBuildChallenge.isFeatured;
				    buildChallenge.entryCount = oldBuildChallenge.entryCount;
				    buildChallenge.maxPartsLimit = oldBuildChallenge.maxPartsLimit;
				    buildChallenge.objectGuid = oldBuildChallenge.objectGuid;
				    buildChallenge.active = oldBuildChallenge.active;
				    buildChallenge.deleted = oldBuildChallenge.deleted;

				    string serializedBuildChallenge = JsonSerializer.Serialize(buildChallenge);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BuildChallengeChangeHistory newBuildChallengeChangeHistory = new BuildChallengeChangeHistory();
				        newBuildChallengeChangeHistory.buildChallengeId = buildChallenge.id;
				        newBuildChallengeChangeHistory.versionNumber = buildChallenge.versionNumber;
				        newBuildChallengeChangeHistory.timeStamp = DateTime.UtcNow;
				        newBuildChallengeChangeHistory.userId = securityUser.id;
				        newBuildChallengeChangeHistory.data = JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge));
				        _context.BuildChallengeChangeHistories.Add(newBuildChallengeChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BuildChallenge rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge)),
						null);


				    return Ok(Database.BuildChallenge.CreateAnonymous(buildChallenge));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.BuildChallenge rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.BuildChallenge rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a BuildChallenge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildChallenge</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallenge/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetBuildChallengeChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.BuildChallenge buildChallenge = await _context.BuildChallenges.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (buildChallenge == null)
			{
				return NotFound();
			}

			try
			{
				buildChallenge.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.BuildChallenge> versionInfo = await buildChallenge.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null)
				{
					return NotFound($"Version {versionNumber} not found.");
				}

				return Ok(versionInfo);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the full audit history for a BuildChallenge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildChallenge</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallenge/{id}/AuditHistory")]
		public async Task<IActionResult> GetBuildChallengeAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.BuildChallenge buildChallenge = await _context.BuildChallenges.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (buildChallenge == null)
			{
				return NotFound();
			}

			try
			{
				buildChallenge.SetupVersionInquiry(_context, Guid.Empty);

				List<VersionInformation<Database.BuildChallenge>> versions = await buildChallenge.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a BuildChallenge.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildChallenge</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The BuildChallenge object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallenge/{id}/Version/{version}")]
		public async Task<IActionResult> GetBuildChallengeVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.BuildChallenge buildChallenge = await _context.BuildChallenges.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (buildChallenge == null)
			{
				return NotFound();
			}

			try
			{
				buildChallenge.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.BuildChallenge> versionInfo = await buildChallenge.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound();
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the state of a BuildChallenge at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildChallenge</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The BuildChallenge object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallenge/{id}/StateAtTime")]
		public async Task<IActionResult> GetBuildChallengeStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.BuildChallenge buildChallenge = await _context.BuildChallenges.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (buildChallenge == null)
			{
				return NotFound();
			}

			try
			{
				buildChallenge.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.BuildChallenge> versionInfo = await buildChallenge.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound("No state found at specified time.");
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}

        /// <summary>
        /// 
        /// This deletes a BuildChallenge record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildChallenge/{id}")]
		[Route("api/BuildChallenge")]
		public async Task<IActionResult> DeleteBuildChallenge(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Moderator role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Moderator", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.BuildChallenge> query = (from x in _context.BuildChallenges
				where
				(x.id == id)
				select x);


			Database.BuildChallenge buildChallenge = await query.FirstOrDefaultAsync(cancellationToken);

			if (buildChallenge == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildChallenge DELETE", id.ToString(), new Exception("No BMC.BuildChallenge entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BuildChallenge cloneOfExisting = (Database.BuildChallenge)_context.Entry(buildChallenge).GetDatabaseValues().ToObject();


			lock (buildChallengeDeleteSyncRoot)
			{
			    try
			    {
			        buildChallenge.deleted = true;
			        buildChallenge.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        BuildChallengeChangeHistory buildChallengeChangeHistory = new BuildChallengeChangeHistory();
			        buildChallengeChangeHistory.buildChallengeId = buildChallenge.id;
			        buildChallengeChangeHistory.versionNumber = buildChallenge.versionNumber;
			        buildChallengeChangeHistory.timeStamp = DateTime.UtcNow;
			        buildChallengeChangeHistory.userId = securityUser.id;
			        buildChallengeChangeHistory.data = JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge));
			        _context.BuildChallengeChangeHistories.Add(buildChallengeChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.BuildChallenge entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.BuildChallenge entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildChallenge.CreateAnonymousWithFirstLevelSubObjects(buildChallenge)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of BuildChallenge records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BuildChallenges/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string rules = null,
			string thumbnailImagePath = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			DateTime? votingEndDate = null,
			bool? isActive = null,
			bool? isFeatured = null,
			int? entryCount = null,
			int? maxPartsLimit = null,
			int? versionNumber = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);


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
			if (startDate.HasValue == true && startDate.Value.Kind != DateTimeKind.Utc)
			{
				startDate = startDate.Value.ToUniversalTime();
			}

			if (endDate.HasValue == true && endDate.Value.Kind != DateTimeKind.Utc)
			{
				endDate = endDate.Value.ToUniversalTime();
			}

			if (votingEndDate.HasValue == true && votingEndDate.Value.Kind != DateTimeKind.Utc)
			{
				votingEndDate = votingEndDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BuildChallenge> query = (from bc in _context.BuildChallenges select bc);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(bc => bc.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(bc => bc.description == description);
			}
			if (string.IsNullOrEmpty(rules) == false)
			{
				query = query.Where(bc => bc.rules == rules);
			}
			if (string.IsNullOrEmpty(thumbnailImagePath) == false)
			{
				query = query.Where(bc => bc.thumbnailImagePath == thumbnailImagePath);
			}
			if (startDate.HasValue == true)
			{
				query = query.Where(bc => bc.startDate == startDate.Value);
			}
			if (endDate.HasValue == true)
			{
				query = query.Where(bc => bc.endDate == endDate.Value);
			}
			if (votingEndDate.HasValue == true)
			{
				query = query.Where(bc => bc.votingEndDate == votingEndDate.Value);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(bc => bc.isActive == isActive.Value);
			}
			if (isFeatured.HasValue == true)
			{
				query = query.Where(bc => bc.isFeatured == isFeatured.Value);
			}
			if (entryCount.HasValue == true)
			{
				query = query.Where(bc => bc.entryCount == entryCount.Value);
			}
			if (maxPartsLimit.HasValue == true)
			{
				query = query.Where(bc => bc.maxPartsLimit == maxPartsLimit.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bc => bc.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bc => bc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bc => bc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bc => bc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bc => bc.deleted == false);
				}
			}
			else
			{
				query = query.Where(bc => bc.active == true);
				query = query.Where(bc => bc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Build Challenge, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.rules.Contains(anyStringContains)
			       || x.thumbnailImagePath.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.name).ThenBy(x => x.thumbnailImagePath);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BuildChallenge.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BuildChallenge/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Moderator role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Moderator", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
