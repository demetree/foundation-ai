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
    /// This auto generated class provides the basic CRUD operations for the BuildStepAnnotationType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BuildStepAnnotationType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BuildStepAnnotationTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<BuildStepAnnotationTypesController> _logger;

		public BuildStepAnnotationTypesController(BMCContext context, ILogger<BuildStepAnnotationTypesController> logger) : base("BMC", "BuildStepAnnotationType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BuildStepAnnotationTypes filtered by the parameters provided.
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
		[Route("api/BuildStepAnnotationTypes")]
		public async Task<IActionResult> GetBuildStepAnnotationTypes(
			string name = null,
			string description = null,
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

			IQueryable<Database.BuildStepAnnotationType> query = (from bsat in _context.BuildStepAnnotationTypes select bsat);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(bsat => bsat.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(bsat => bsat.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bsat => bsat.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsat => bsat.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsat => bsat.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsat => bsat.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsat => bsat.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsat => bsat.active == true);
				query = query.Where(bsat => bsat.deleted == false);
			}

			query = query.OrderBy(bsat => bsat.sequence).ThenBy(bsat => bsat.name).ThenBy(bsat => bsat.description);


			//
			// Add the any string contains parameter to span all the string fields on the Build Step Annotation Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
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
			
			List<Database.BuildStepAnnotationType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BuildStepAnnotationType buildStepAnnotationType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(buildStepAnnotationType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BuildStepAnnotationType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BuildStepAnnotationType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BuildStepAnnotationTypes filtered by the parameters provided.  Its query is similar to the GetBuildStepAnnotationTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotationTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
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

			IQueryable<Database.BuildStepAnnotationType> query = (from bsat in _context.BuildStepAnnotationTypes select bsat);
			if (name != null)
			{
				query = query.Where(bsat => bsat.name == name);
			}
			if (description != null)
			{
				query = query.Where(bsat => bsat.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bsat => bsat.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsat => bsat.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsat => bsat.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsat => bsat.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsat => bsat.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsat => bsat.active == true);
				query = query.Where(bsat => bsat.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Build Step Annotation Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BuildStepAnnotationType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotationType/{id}")]
		public async Task<IActionResult> GetBuildStepAnnotationType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BuildStepAnnotationType> query = (from bsat in _context.BuildStepAnnotationTypes where
							(bsat.id == id) &&
							(userIsAdmin == true || bsat.deleted == false) &&
							(userIsWriter == true || bsat.active == true)
					select bsat);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.BuildStepAnnotationType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BuildStepAnnotationType Entity was read with Admin privilege." : "BMC.BuildStepAnnotationType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildStepAnnotationType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BuildStepAnnotationType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BuildStepAnnotationType.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BuildStepAnnotationType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BuildStepAnnotationType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BuildStepAnnotationType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBuildStepAnnotationType(int id, [FromBody]Database.BuildStepAnnotationType.BuildStepAnnotationTypeDTO buildStepAnnotationTypeDTO, CancellationToken cancellationToken = default)
		{
			if (buildStepAnnotationTypeDTO == null)
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



			if (id != buildStepAnnotationTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.BuildStepAnnotationType> query = (from x in _context.BuildStepAnnotationTypes
				where
				(x.id == id)
				select x);


			Database.BuildStepAnnotationType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildStepAnnotationType PUT", id.ToString(), new Exception("No BMC.BuildStepAnnotationType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (buildStepAnnotationTypeDTO.objectGuid == Guid.Empty)
            {
                buildStepAnnotationTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (buildStepAnnotationTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BuildStepAnnotationType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BuildStepAnnotationType cloneOfExisting = (Database.BuildStepAnnotationType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BuildStepAnnotationType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BuildStepAnnotationType buildStepAnnotationType = (Database.BuildStepAnnotationType)_context.Entry(existing).GetDatabaseValues().ToObject();
			buildStepAnnotationType.ApplyDTO(buildStepAnnotationTypeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (buildStepAnnotationType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BuildStepAnnotationType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (buildStepAnnotationType.name != null && buildStepAnnotationType.name.Length > 100)
			{
				buildStepAnnotationType.name = buildStepAnnotationType.name.Substring(0, 100);
			}

			if (buildStepAnnotationType.description != null && buildStepAnnotationType.description.Length > 500)
			{
				buildStepAnnotationType.description = buildStepAnnotationType.description.Substring(0, 500);
			}

			EntityEntry<Database.BuildStepAnnotationType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(buildStepAnnotationType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BuildStepAnnotationType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildStepAnnotationType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildStepAnnotationType.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotationType)),
					null);


				return Ok(Database.BuildStepAnnotationType.CreateAnonymous(buildStepAnnotationType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BuildStepAnnotationType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildStepAnnotationType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildStepAnnotationType.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotationType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BuildStepAnnotationType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotationType", Name = "BuildStepAnnotationType")]
		public async Task<IActionResult> PostBuildStepAnnotationType([FromBody]Database.BuildStepAnnotationType.BuildStepAnnotationTypeDTO buildStepAnnotationTypeDTO, CancellationToken cancellationToken = default)
		{
			if (buildStepAnnotationTypeDTO == null)
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
			// Create a new BuildStepAnnotationType object using the data from the DTO
			//
			Database.BuildStepAnnotationType buildStepAnnotationType = Database.BuildStepAnnotationType.FromDTO(buildStepAnnotationTypeDTO);

			try
			{
				if (buildStepAnnotationType.name != null && buildStepAnnotationType.name.Length > 100)
				{
					buildStepAnnotationType.name = buildStepAnnotationType.name.Substring(0, 100);
				}

				if (buildStepAnnotationType.description != null && buildStepAnnotationType.description.Length > 500)
				{
					buildStepAnnotationType.description = buildStepAnnotationType.description.Substring(0, 500);
				}

				buildStepAnnotationType.objectGuid = Guid.NewGuid();
				_context.BuildStepAnnotationTypes.Add(buildStepAnnotationType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BuildStepAnnotationType entity successfully created.",
					true,
					buildStepAnnotationType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BuildStepAnnotationType.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotationType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BuildStepAnnotationType entity creation failed.", false, buildStepAnnotationType.id.ToString(), "", JsonSerializer.Serialize(buildStepAnnotationType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BuildStepAnnotationType", buildStepAnnotationType.id, buildStepAnnotationType.name));

			return CreatedAtRoute("BuildStepAnnotationType", new { id = buildStepAnnotationType.id }, Database.BuildStepAnnotationType.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotationType));
		}



        /// <summary>
        /// 
        /// This deletes a BuildStepAnnotationType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BuildStepAnnotationType/{id}")]
		[Route("api/BuildStepAnnotationType")]
		public async Task<IActionResult> DeleteBuildStepAnnotationType(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BuildStepAnnotationType> query = (from x in _context.BuildStepAnnotationTypes
				where
				(x.id == id)
				select x);


			Database.BuildStepAnnotationType buildStepAnnotationType = await query.FirstOrDefaultAsync(cancellationToken);

			if (buildStepAnnotationType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BuildStepAnnotationType DELETE", id.ToString(), new Exception("No BMC.BuildStepAnnotationType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BuildStepAnnotationType cloneOfExisting = (Database.BuildStepAnnotationType)_context.Entry(buildStepAnnotationType).GetDatabaseValues().ToObject();


			try
			{
				buildStepAnnotationType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BuildStepAnnotationType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildStepAnnotationType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildStepAnnotationType.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotationType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BuildStepAnnotationType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BuildStepAnnotationType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BuildStepAnnotationType.CreateAnonymousWithFirstLevelSubObjects(buildStepAnnotationType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BuildStepAnnotationType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BuildStepAnnotationTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
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

			IQueryable<Database.BuildStepAnnotationType> query = (from bsat in _context.BuildStepAnnotationTypes select bsat);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(bsat => bsat.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(bsat => bsat.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bsat => bsat.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bsat => bsat.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bsat => bsat.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bsat => bsat.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bsat => bsat.deleted == false);
				}
			}
			else
			{
				query = query.Where(bsat => bsat.active == true);
				query = query.Where(bsat => bsat.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Build Step Annotation Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BuildStepAnnotationType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BuildStepAnnotationType/CreateAuditEvent")]
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
