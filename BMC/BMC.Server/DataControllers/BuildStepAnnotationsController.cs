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
    /// This auto generated class provides the basic CRUD operations for the BuildStepAnnotation entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BuildStepAnnotation entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BuildStepAnnotationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 20;

		static object buildStepAnnotationPutSyncRoot = new object();
		static object buildStepAnnotationDeleteSyncRoot = new object();

		private BMCContext _context;

		private ILogger<BuildStepAnnotationsController> _logger;

		public BuildStepAnnotationsController(BMCContext context, ILogger<BuildStepAnnotationsController> logger) : base("BMC", "BuildStepAnnotation")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BuildStepAnnotations filtered by the parameters provided.
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
		[Route("api/BuildStepAnnotations")]
		public async Task<IActionResult> GetBuildStepAnnotations(
			int? buildManualStepId = null,
			int? buildStepAnnotationTypeId = null,
			float? positionX = null,
			float? positionY = null,
			float? width = null,
			float? height = null,
			string text = null,
			int? placedBrickId = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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

			IQueryable<Database.BuildStepAnnotation> query = (from bsa in _context.BuildStepAnnotations select bsa);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildManualStepId.HasValue == true)
			{
				query = query.Where(bsa => bsa.buildManualStepId == buildManualStepId.Value);
			}
			if (buildStepAnnotationTypeId.HasValue == true)
			{
				query = query.Where(bsa => bsa.buildStepAnnotationTypeId == buildStepAnnotationTypeId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(bsa => bsa.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(bsa => bsa.positionY == positionY.Value);
			}
			if (width.HasValue == true)
			{
				query = query.Where(bsa => bsa.width == width.Value);
			}
			if (height.HasValue == true)
			{
				query = query.Where(bsa => bsa.height == height.Value);
			}
			if (string.IsNullOrEmpty(text) == false)
			{
				query = query.Where(bsa => bsa.text == text);
			}
			if (placedBrickId.HasValue == true)
			{
				query = query.Where(bsa => bsa.placedBrickId == placedBrickId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bsa => bsa.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsa => bsa.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsa => bsa.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsa => bsa.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsa => bsa.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsa => bsa.active == true);
				query = query.Where(bsa => bsa.deleted == false);
			}

			query = query.OrderBy(bsa => bsa.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.buildManualStep);
				query = query.Include(x => x.buildStepAnnotationType);
				query = query.Include(x => x.placedBrick);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.BuildStepAnnotation> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BuildStepAnnotation buildStepAnnotation in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(buildStepAnnotation, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BuildStepAnnotation Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BuildStepAnnotation Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BuildStepAnnotations filtered by the parameters provided.  Its query is similar to the GetBuildStepAnnotations method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotations/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? buildManualStepId = null,
			int? buildStepAnnotationTypeId = null,
			float? positionX = null,
			float? positionY = null,
			float? width = null,
			float? height = null,
			string text = null,
			int? placedBrickId = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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


			IQueryable<Database.BuildStepAnnotation> query = (from bsa in _context.BuildStepAnnotations select bsa);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (buildManualStepId.HasValue == true)
			{
				query = query.Where(bsa => bsa.buildManualStepId == buildManualStepId.Value);
			}
			if (buildStepAnnotationTypeId.HasValue == true)
			{
				query = query.Where(bsa => bsa.buildStepAnnotationTypeId == buildStepAnnotationTypeId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(bsa => bsa.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(bsa => bsa.positionY == positionY.Value);
			}
			if (width.HasValue == true)
			{
				query = query.Where(bsa => bsa.width == width.Value);
			}
			if (height.HasValue == true)
			{
				query = query.Where(bsa => bsa.height == height.Value);
			}
			if (text != null)
			{
				query = query.Where(bsa => bsa.text == text);
			}
			if (placedBrickId.HasValue == true)
			{
				query = query.Where(bsa => bsa.placedBrickId == placedBrickId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bsa => bsa.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsa => bsa.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsa => bsa.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsa => bsa.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsa => bsa.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsa => bsa.active == true);
				query = query.Where(bsa => bsa.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BuildStepAnnotation by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotation/{id}")]
		public async Task<IActionResult> GetBuildStepAnnotation(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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
				IQueryable<Database.BuildStepAnnotation> query = (from bsa in _context.BuildStepAnnotations where
							(bsa.id == id) &&
							(userIsAdmin == true || bsa.deleted == false) &&
							(userIsWriter == true || bsa.active == true)
					select bsa);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.buildManualStep);
					query = query.Include(x => x.buildStepAnnotationType);
					query = query.Include(x => x.placedBrick);
					query = query.AsSplitQuery();
				}

				Database.BuildStepAnnotation materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BuildStepAnnotation Entity was read with Admin privilege." : "BMC.BuildStepAnnotation Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildStepAnnotation", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BuildStepAnnotation entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BuildStepAnnotation.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BuildStepAnnotation.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BuildStepAnnotation record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BuildStepAnnotation/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBuildStepAnnotation(int id, [FromBody]Database.BuildStepAnnotation.BuildStepAnnotationDTO buildStepAnnotationDTO, CancellationToken cancellationToken = default)
		{
			if (buildStepAnnotationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != buildStepAnnotationDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);
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


			IQueryable<Database.BuildStepAnnotation> query = (from x in _context.BuildStepAnnotations
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BuildStepAnnotation existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildStepAnnotation PUT", id.ToString(), new Exception("No BMC.BuildStepAnnotation entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (buildStepAnnotationDTO.objectGuid == Guid.Empty)
            {
                buildStepAnnotationDTO.objectGuid = existing.objectGuid;
            }
            else if (buildStepAnnotationDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BuildStepAnnotation record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BuildStepAnnotation cloneOfExisting = (Database.BuildStepAnnotation)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BuildStepAnnotation object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BuildStepAnnotation buildStepAnnotation = (Database.BuildStepAnnotation)_context.Entry(existing).GetDatabaseValues().ToObject();
			buildStepAnnotation.ApplyDTO(buildStepAnnotationDTO);
			//
			// The tenant guid for any BuildStepAnnotation being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the BuildStepAnnotation because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				buildStepAnnotation.tenantGuid = existing.tenantGuid;
			}

			lock (buildStepAnnotationPutSyncRoot)
			{
				//
				// Validate the version number for the buildStepAnnotation being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != buildStepAnnotation.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "BuildStepAnnotation save attempt was made but save request was with version " + buildStepAnnotation.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The BuildStepAnnotation you are trying to update has already changed.  Please try your save again after reloading the BuildStepAnnotation.");
				}
				else
				{
					// Same record.  Increase version.
					buildStepAnnotation.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (buildStepAnnotation.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BuildStepAnnotation record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.BuildStepAnnotation> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(buildStepAnnotation);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BuildStepAnnotationChangeHistory buildStepAnnotationChangeHistory = new BuildStepAnnotationChangeHistory();
				        buildStepAnnotationChangeHistory.buildStepAnnotationId = buildStepAnnotation.id;
				        buildStepAnnotationChangeHistory.versionNumber = buildStepAnnotation.versionNumber;
				        buildStepAnnotationChangeHistory.timeStamp = DateTime.UtcNow;
				        buildStepAnnotationChangeHistory.userId = securityUser.id;
				        buildStepAnnotationChangeHistory.tenantGuid = userTenantGuid;
				        buildStepAnnotationChangeHistory.data = JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation));
				        _context.BuildStepAnnotationChangeHistories.Add(buildStepAnnotationChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BuildStepAnnotation entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation)),
						null);

				return Ok(Database.BuildStepAnnotation.CreateAnonymous(buildStepAnnotation));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BuildStepAnnotation entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new BuildStepAnnotation record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotation", Name = "BuildStepAnnotation")]
		public async Task<IActionResult> PostBuildStepAnnotation([FromBody]Database.BuildStepAnnotation.BuildStepAnnotationDTO buildStepAnnotationDTO, CancellationToken cancellationToken = default)
		{
			if (buildStepAnnotationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new BuildStepAnnotation object using the data from the DTO
			//
			Database.BuildStepAnnotation buildStepAnnotation = Database.BuildStepAnnotation.FromDTO(buildStepAnnotationDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				buildStepAnnotation.tenantGuid = userTenantGuid;

				buildStepAnnotation.objectGuid = Guid.NewGuid();
				buildStepAnnotation.versionNumber = 1;

				_context.BuildStepAnnotations.Add(buildStepAnnotation);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the buildStepAnnotation object so that no further changes will be written to the database
				    //
				    _context.Entry(buildStepAnnotation).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					buildStepAnnotation.BuildStepAnnotationChangeHistories = null;
					buildStepAnnotation.buildManualStep = null;
					buildStepAnnotation.buildStepAnnotationType = null;
					buildStepAnnotation.placedBrick = null;


				    BuildStepAnnotationChangeHistory buildStepAnnotationChangeHistory = new BuildStepAnnotationChangeHistory();
				    buildStepAnnotationChangeHistory.buildStepAnnotationId = buildStepAnnotation.id;
				    buildStepAnnotationChangeHistory.versionNumber = buildStepAnnotation.versionNumber;
				    buildStepAnnotationChangeHistory.timeStamp = DateTime.UtcNow;
				    buildStepAnnotationChangeHistory.userId = securityUser.id;
				    buildStepAnnotationChangeHistory.tenantGuid = userTenantGuid;
				    buildStepAnnotationChangeHistory.data = JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation));
				    _context.BuildStepAnnotationChangeHistories.Add(buildStepAnnotationChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"BMC.BuildStepAnnotation entity successfully created.",
						true,
						buildStepAnnotation. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BuildStepAnnotation entity creation failed.", false, buildStepAnnotation.id.ToString(), "", JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildStepAnnotation", buildStepAnnotation.id, buildStepAnnotation.id.ToString()));

			return CreatedAtRoute("BuildStepAnnotation", new { id = buildStepAnnotation.id }, Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation));
		}



        /// <summary>
        /// 
        /// This rolls a BuildStepAnnotation entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotation/Rollback/{id}")]
		[Route("api/BuildStepAnnotation/Rollback")]
		public async Task<IActionResult> RollbackToBuildStepAnnotationVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.BuildStepAnnotation> query = (from x in _context.BuildStepAnnotations
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this BuildStepAnnotation concurrently
			//
			lock (buildStepAnnotationPutSyncRoot)
			{
				
				Database.BuildStepAnnotation buildStepAnnotation = query.FirstOrDefault();
				
				if (buildStepAnnotation == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildStepAnnotation rollback", id.ToString(), new Exception("No BMC.BuildStepAnnotation entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the BuildStepAnnotation current state so we can log it.
				//
				Database.BuildStepAnnotation cloneOfExisting = (Database.BuildStepAnnotation)_context.Entry(buildStepAnnotation).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.BuildStepAnnotationChangeHistories = null;
				cloneOfExisting.buildManualStep = null;
				cloneOfExisting.buildStepAnnotationType = null;
				cloneOfExisting.placedBrick = null;

				if (versionNumber >= buildStepAnnotation.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for BMC.BuildStepAnnotation rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for BMC.BuildStepAnnotation rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				BuildStepAnnotationChangeHistory buildStepAnnotationChangeHistory = (from x in _context.BuildStepAnnotationChangeHistories
				                                               where
				                                               x.buildStepAnnotationId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (buildStepAnnotationChangeHistory != null)
				{
				    Database.BuildStepAnnotation oldBuildStepAnnotation = JsonSerializer.Deserialize<Database.BuildStepAnnotation>(buildStepAnnotationChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    buildStepAnnotation.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    buildStepAnnotation.buildManualStepId = oldBuildStepAnnotation.buildManualStepId;
				    buildStepAnnotation.buildStepAnnotationTypeId = oldBuildStepAnnotation.buildStepAnnotationTypeId;
				    buildStepAnnotation.positionX = oldBuildStepAnnotation.positionX;
				    buildStepAnnotation.positionY = oldBuildStepAnnotation.positionY;
				    buildStepAnnotation.width = oldBuildStepAnnotation.width;
				    buildStepAnnotation.height = oldBuildStepAnnotation.height;
				    buildStepAnnotation.text = oldBuildStepAnnotation.text;
				    buildStepAnnotation.placedBrickId = oldBuildStepAnnotation.placedBrickId;
				    buildStepAnnotation.objectGuid = oldBuildStepAnnotation.objectGuid;
				    buildStepAnnotation.active = oldBuildStepAnnotation.active;
				    buildStepAnnotation.deleted = oldBuildStepAnnotation.deleted;

				    string serializedBuildStepAnnotation = JsonSerializer.Serialize(buildStepAnnotation);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        BuildStepAnnotationChangeHistory newBuildStepAnnotationChangeHistory = new BuildStepAnnotationChangeHistory();
				        newBuildStepAnnotationChangeHistory.buildStepAnnotationId = buildStepAnnotation.id;
				        newBuildStepAnnotationChangeHistory.versionNumber = buildStepAnnotation.versionNumber;
				        newBuildStepAnnotationChangeHistory.timeStamp = DateTime.UtcNow;
				        newBuildStepAnnotationChangeHistory.userId = securityUser.id;
				        newBuildStepAnnotationChangeHistory.tenantGuid = userTenantGuid;
				        newBuildStepAnnotationChangeHistory.data = JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation));
				        _context.BuildStepAnnotationChangeHistories.Add(newBuildStepAnnotationChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"BMC.BuildStepAnnotation rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation)),
						null);


				    return Ok(Database.BuildStepAnnotation.CreateAnonymous(buildStepAnnotation));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for BMC.BuildStepAnnotation rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for BMC.BuildStepAnnotation rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a BuildStepAnnotation.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildStepAnnotation</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotation/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetBuildStepAnnotationChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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


			Database.BuildStepAnnotation buildStepAnnotation = await _context.BuildStepAnnotations.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildStepAnnotation == null)
			{
				return NotFound();
			}

			try
			{
				buildStepAnnotation.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.BuildStepAnnotation> versionInfo = await buildStepAnnotation.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a BuildStepAnnotation.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildStepAnnotation</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotation/{id}/AuditHistory")]
		public async Task<IActionResult> GetBuildStepAnnotationAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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


			Database.BuildStepAnnotation buildStepAnnotation = await _context.BuildStepAnnotations.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildStepAnnotation == null)
			{
				return NotFound();
			}

			try
			{
				buildStepAnnotation.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.BuildStepAnnotation>> versions = await buildStepAnnotation.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a BuildStepAnnotation.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildStepAnnotation</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The BuildStepAnnotation object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotation/{id}/Version/{version}")]
		public async Task<IActionResult> GetBuildStepAnnotationVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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


			Database.BuildStepAnnotation buildStepAnnotation = await _context.BuildStepAnnotations.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildStepAnnotation == null)
			{
				return NotFound();
			}

			try
			{
				buildStepAnnotation.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.BuildStepAnnotation> versionInfo = await buildStepAnnotation.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a BuildStepAnnotation at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the BuildStepAnnotation</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The BuildStepAnnotation object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotation/{id}/StateAtTime")]
		public async Task<IActionResult> GetBuildStepAnnotationStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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


			Database.BuildStepAnnotation buildStepAnnotation = await _context.BuildStepAnnotations.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (buildStepAnnotation == null)
			{
				return NotFound();
			}

			try
			{
				buildStepAnnotation.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.BuildStepAnnotation> versionInfo = await buildStepAnnotation.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a BuildStepAnnotation record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotation/{id}")]
		[Route("api/BuildStepAnnotation")]
		public async Task<IActionResult> DeleteBuildStepAnnotation(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.BuildStepAnnotation> query = (from x in _context.BuildStepAnnotations
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.BuildStepAnnotation buildStepAnnotation = await query.FirstOrDefaultAsync(cancellationToken);

			if (buildStepAnnotation == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildStepAnnotation DELETE", id.ToString(), new Exception("No BMC.BuildStepAnnotation entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BuildStepAnnotation cloneOfExisting = (Database.BuildStepAnnotation)_context.Entry(buildStepAnnotation).GetDatabaseValues().ToObject();


			lock (buildStepAnnotationDeleteSyncRoot)
			{
			    try
			    {
			        buildStepAnnotation.deleted = true;
			        buildStepAnnotation.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        BuildStepAnnotationChangeHistory buildStepAnnotationChangeHistory = new BuildStepAnnotationChangeHistory();
			        buildStepAnnotationChangeHistory.buildStepAnnotationId = buildStepAnnotation.id;
			        buildStepAnnotationChangeHistory.versionNumber = buildStepAnnotation.versionNumber;
			        buildStepAnnotationChangeHistory.timeStamp = DateTime.UtcNow;
			        buildStepAnnotationChangeHistory.userId = securityUser.id;
			        buildStepAnnotationChangeHistory.tenantGuid = userTenantGuid;
			        buildStepAnnotationChangeHistory.data = JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation));
			        _context.BuildStepAnnotationChangeHistories.Add(buildStepAnnotationChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.BuildStepAnnotation entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"BMC.BuildStepAnnotation entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.BuildStepAnnotation.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotation)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of BuildStepAnnotation records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BuildStepAnnotations/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? buildManualStepId = null,
			int? buildStepAnnotationTypeId = null,
			float? positionX = null,
			float? positionY = null,
			float? width = null,
			float? height = null,
			string text = null,
			int? placedBrickId = null,
			int? versionNumber = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 20, cancellationToken);


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

			IQueryable<Database.BuildStepAnnotation> query = (from bsa in _context.BuildStepAnnotations select bsa);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (buildManualStepId.HasValue == true)
			{
				query = query.Where(bsa => bsa.buildManualStepId == buildManualStepId.Value);
			}
			if (buildStepAnnotationTypeId.HasValue == true)
			{
				query = query.Where(bsa => bsa.buildStepAnnotationTypeId == buildStepAnnotationTypeId.Value);
			}
			if (positionX.HasValue == true)
			{
				query = query.Where(bsa => bsa.positionX == positionX.Value);
			}
			if (positionY.HasValue == true)
			{
				query = query.Where(bsa => bsa.positionY == positionY.Value);
			}
			if (width.HasValue == true)
			{
				query = query.Where(bsa => bsa.width == width.Value);
			}
			if (height.HasValue == true)
			{
				query = query.Where(bsa => bsa.height == height.Value);
			}
			if (string.IsNullOrEmpty(text) == false)
			{
				query = query.Where(bsa => bsa.text == text);
			}
			if (placedBrickId.HasValue == true)
			{
				query = query.Where(bsa => bsa.placedBrickId == placedBrickId.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(bsa => bsa.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsa => bsa.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsa => bsa.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsa => bsa.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsa => bsa.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsa => bsa.active == true);
				query = query.Where(bsa => bsa.deleted == false);
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BuildStepAnnotation.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BuildStepAnnotation/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Instruction Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Instruction Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
