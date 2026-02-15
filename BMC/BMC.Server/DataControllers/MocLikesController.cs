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
    /// This auto generated class provides the basic CRUD operations for the MocLike entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MocLike entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MocLikesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<MocLikesController> _logger;

		public MocLikesController(BMCContext context, ILogger<MocLikesController> logger) : base("BMC", "MocLike")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MocLikes filtered by the parameters provided.
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
		[Route("api/MocLikes")]
		public async Task<IActionResult> GetMocLikes(
			int? publishedMocId = null,
			Guid? likerTenantGuid = null,
			DateTime? likedDate = null,
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
			if (likedDate.HasValue == true && likedDate.Value.Kind != DateTimeKind.Utc)
			{
				likedDate = likedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocLike> query = (from ml in _context.MocLikes select ml);
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(ml => ml.publishedMocId == publishedMocId.Value);
			}
			if (likerTenantGuid.HasValue == true)
			{
				query = query.Where(ml => ml.likerTenantGuid == likerTenantGuid);
			}
			if (likedDate.HasValue == true)
			{
				query = query.Where(ml => ml.likedDate == likedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ml => ml.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ml => ml.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ml => ml.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ml => ml.deleted == false);
				}
			}
			else
			{
				query = query.Where(ml => ml.active == true);
				query = query.Where(ml => ml.deleted == false);
			}

			query = query.OrderBy(ml => ml.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.publishedMoc);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Moc Like, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.publishedMoc.name.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.description.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.thumbnailImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.tags.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.MocLike> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MocLike mocLike in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(mocLike, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.MocLike Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.MocLike Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MocLikes filtered by the parameters provided.  Its query is similar to the GetMocLikes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocLikes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? publishedMocId = null,
			Guid? likerTenantGuid = null,
			DateTime? likedDate = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (likedDate.HasValue == true && likedDate.Value.Kind != DateTimeKind.Utc)
			{
				likedDate = likedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocLike> query = (from ml in _context.MocLikes select ml);
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(ml => ml.publishedMocId == publishedMocId.Value);
			}
			if (likerTenantGuid.HasValue == true)
			{
				query = query.Where(ml => ml.likerTenantGuid == likerTenantGuid);
			}
			if (likedDate.HasValue == true)
			{
				query = query.Where(ml => ml.likedDate == likedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ml => ml.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ml => ml.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ml => ml.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ml => ml.deleted == false);
				}
			}
			else
			{
				query = query.Where(ml => ml.active == true);
				query = query.Where(ml => ml.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Moc Like, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.publishedMoc.name.Contains(anyStringContains)
			       || x.publishedMoc.description.Contains(anyStringContains)
			       || x.publishedMoc.thumbnailImagePath.Contains(anyStringContains)
			       || x.publishedMoc.tags.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MocLike by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocLike/{id}")]
		public async Task<IActionResult> GetMocLike(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			try
			{
				IQueryable<Database.MocLike> query = (from ml in _context.MocLikes where
							(ml.id == id) &&
							(userIsAdmin == true || ml.deleted == false) &&
							(userIsWriter == true || ml.active == true)
					select ml);

				if (includeRelations == true)
				{
					query = query.Include(x => x.publishedMoc);
					query = query.AsSplitQuery();
				}

				Database.MocLike materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.MocLike Entity was read with Admin privilege." : "BMC.MocLike Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MocLike", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.MocLike entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.MocLike.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.MocLike.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MocLike record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MocLike/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMocLike(int id, [FromBody]Database.MocLike.MocLikeDTO mocLikeDTO, CancellationToken cancellationToken = default)
		{
			if (mocLikeDTO == null)
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



			if (id != mocLikeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.MocLike> query = (from x in _context.MocLikes
				where
				(x.id == id)
				select x);


			Database.MocLike existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocLike PUT", id.ToString(), new Exception("No BMC.MocLike entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (mocLikeDTO.objectGuid == Guid.Empty)
            {
                mocLikeDTO.objectGuid = existing.objectGuid;
            }
            else if (mocLikeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MocLike record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MocLike cloneOfExisting = (Database.MocLike)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MocLike object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MocLike mocLike = (Database.MocLike)_context.Entry(existing).GetDatabaseValues().ToObject();
			mocLike.ApplyDTO(mocLikeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (mocLike.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.MocLike record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (mocLike.likedDate.Kind != DateTimeKind.Utc)
			{
				mocLike.likedDate = mocLike.likedDate.ToUniversalTime();
			}

			EntityEntry<Database.MocLike> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(mocLike);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.MocLike entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocLike.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocLike.CreateAnonymousWithFirstLevelSubObjects(mocLike)),
					null);


				return Ok(Database.MocLike.CreateAnonymous(mocLike));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.MocLike entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocLike.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocLike.CreateAnonymousWithFirstLevelSubObjects(mocLike)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MocLike record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocLike", Name = "MocLike")]
		public async Task<IActionResult> PostMocLike([FromBody]Database.MocLike.MocLikeDTO mocLikeDTO, CancellationToken cancellationToken = default)
		{
			if (mocLikeDTO == null)
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

			//
			// Create a new MocLike object using the data from the DTO
			//
			Database.MocLike mocLike = Database.MocLike.FromDTO(mocLikeDTO);

			try
			{
				if (mocLike.likedDate.Kind != DateTimeKind.Utc)
				{
					mocLike.likedDate = mocLike.likedDate.ToUniversalTime();
				}

				mocLike.objectGuid = Guid.NewGuid();
				_context.MocLikes.Add(mocLike);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.MocLike entity successfully created.",
					true,
					mocLike.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MocLike.CreateAnonymousWithFirstLevelSubObjects(mocLike)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.MocLike entity creation failed.", false, mocLike.id.ToString(), "", JsonSerializer.Serialize(mocLike), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MocLike", mocLike.id, mocLike.id.ToString()));

			return CreatedAtRoute("MocLike", new { id = mocLike.id }, Database.MocLike.CreateAnonymousWithFirstLevelSubObjects(mocLike));
		}



        /// <summary>
        /// 
        /// This deletes a MocLike record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocLike/{id}")]
		[Route("api/MocLike")]
		public async Task<IActionResult> DeleteMocLike(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.MocLike> query = (from x in _context.MocLikes
				where
				(x.id == id)
				select x);


			Database.MocLike mocLike = await query.FirstOrDefaultAsync(cancellationToken);

			if (mocLike == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocLike DELETE", id.ToString(), new Exception("No BMC.MocLike entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MocLike cloneOfExisting = (Database.MocLike)_context.Entry(mocLike).GetDatabaseValues().ToObject();


			try
			{
				mocLike.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.MocLike entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocLike.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocLike.CreateAnonymousWithFirstLevelSubObjects(mocLike)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.MocLike entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocLike.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocLike.CreateAnonymousWithFirstLevelSubObjects(mocLike)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MocLike records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MocLikes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? publishedMocId = null,
			Guid? likerTenantGuid = null,
			DateTime? likedDate = null,
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
			if (likedDate.HasValue == true && likedDate.Value.Kind != DateTimeKind.Utc)
			{
				likedDate = likedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocLike> query = (from ml in _context.MocLikes select ml);
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(ml => ml.publishedMocId == publishedMocId.Value);
			}
			if (likerTenantGuid.HasValue == true)
			{
				query = query.Where(ml => ml.likerTenantGuid == likerTenantGuid);
			}
			if (likedDate.HasValue == true)
			{
				query = query.Where(ml => ml.likedDate == likedDate.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ml => ml.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ml => ml.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ml => ml.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ml => ml.deleted == false);
				}
			}
			else
			{
				query = query.Where(ml => ml.active == true);
				query = query.Where(ml => ml.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Moc Like, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.publishedMoc.name.Contains(anyStringContains)
			       || x.publishedMoc.description.Contains(anyStringContains)
			       || x.publishedMoc.thumbnailImagePath.Contains(anyStringContains)
			       || x.publishedMoc.tags.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MocLike.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MocLike/CreateAuditEvent")]
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
