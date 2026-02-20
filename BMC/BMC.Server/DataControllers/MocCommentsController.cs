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
    /// This auto generated class provides the basic CRUD operations for the MocComment entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the MocComment entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class MocCommentsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<MocCommentsController> _logger;

		public MocCommentsController(BMCContext context, ILogger<MocCommentsController> logger) : base("BMC", "MocComment")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of MocComments filtered by the parameters provided.
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
		[Route("api/MocComments")]
		public async Task<IActionResult> GetMocComments(
			int? publishedMocId = null,
			Guid? commenterTenantGuid = null,
			string commentText = null,
			DateTime? postedDate = null,
			int? mocCommentId = null,
			bool? isEdited = null,
			bool? isHidden = null,
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
			if (postedDate.HasValue == true && postedDate.Value.Kind != DateTimeKind.Utc)
			{
				postedDate = postedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocComment> query = (from mc in _context.MocComments select mc);
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(mc => mc.publishedMocId == publishedMocId.Value);
			}
			if (commenterTenantGuid.HasValue == true)
			{
				query = query.Where(mc => mc.commenterTenantGuid == commenterTenantGuid);
			}
			if (string.IsNullOrEmpty(commentText) == false)
			{
				query = query.Where(mc => mc.commentText == commentText);
			}
			if (postedDate.HasValue == true)
			{
				query = query.Where(mc => mc.postedDate == postedDate.Value);
			}
			if (mocCommentId.HasValue == true)
			{
				query = query.Where(mc => mc.mocCommentId == mocCommentId.Value);
			}
			if (isEdited.HasValue == true)
			{
				query = query.Where(mc => mc.isEdited == isEdited.Value);
			}
			if (isHidden.HasValue == true)
			{
				query = query.Where(mc => mc.isHidden == isHidden.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mc => mc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mc => mc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mc => mc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mc => mc.deleted == false);
				}
			}
			else
			{
				query = query.Where(mc => mc.active == true);
				query = query.Where(mc => mc.deleted == false);
			}

			query = query.OrderBy(mc => mc.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.mocComment);
				query = query.Include(x => x.publishedMoc);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.MocComment> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.MocComment mocComment in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(mocComment, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.MocComment Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.MocComment Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of MocComments filtered by the parameters provided.  Its query is similar to the GetMocComments method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocComments/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? publishedMocId = null,
			Guid? commenterTenantGuid = null,
			string commentText = null,
			DateTime? postedDate = null,
			int? mocCommentId = null,
			bool? isEdited = null,
			bool? isHidden = null,
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

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (postedDate.HasValue == true && postedDate.Value.Kind != DateTimeKind.Utc)
			{
				postedDate = postedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocComment> query = (from mc in _context.MocComments select mc);
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(mc => mc.publishedMocId == publishedMocId.Value);
			}
			if (commenterTenantGuid.HasValue == true)
			{
				query = query.Where(mc => mc.commenterTenantGuid == commenterTenantGuid);
			}
			if (commentText != null)
			{
				query = query.Where(mc => mc.commentText == commentText);
			}
			if (postedDate.HasValue == true)
			{
				query = query.Where(mc => mc.postedDate == postedDate.Value);
			}
			if (mocCommentId.HasValue == true)
			{
				query = query.Where(mc => mc.mocCommentId == mocCommentId.Value);
			}
			if (isEdited.HasValue == true)
			{
				query = query.Where(mc => mc.isEdited == isEdited.Value);
			}
			if (isHidden.HasValue == true)
			{
				query = query.Where(mc => mc.isHidden == isHidden.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mc => mc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mc => mc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mc => mc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mc => mc.deleted == false);
				}
			}
			else
			{
				query = query.Where(mc => mc.active == true);
				query = query.Where(mc => mc.deleted == false);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single MocComment by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocComment/{id}")]
		public async Task<IActionResult> GetMocComment(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.MocComment> query = (from mc in _context.MocComments where
							(mc.id == id) &&
							(userIsAdmin == true || mc.deleted == false) &&
							(userIsWriter == true || mc.active == true)
					select mc);

				if (includeRelations == true)
				{
					query = query.Include(x => x.mocComment);
					query = query.Include(x => x.publishedMoc);
					query = query.AsSplitQuery();
				}

				Database.MocComment materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.MocComment Entity was read with Admin privilege." : "BMC.MocComment Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MocComment", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.MocComment entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.MocComment.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.MocComment.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing MocComment record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/MocComment/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutMocComment(int id, [FromBody]Database.MocComment.MocCommentDTO mocCommentDTO, CancellationToken cancellationToken = default)
		{
			if (mocCommentDTO == null)
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



			if (id != mocCommentDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.MocComment> query = (from x in _context.MocComments
				where
				(x.id == id)
				select x);


			Database.MocComment existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocComment PUT", id.ToString(), new Exception("No BMC.MocComment entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (mocCommentDTO.objectGuid == Guid.Empty)
            {
                mocCommentDTO.objectGuid = existing.objectGuid;
            }
            else if (mocCommentDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a MocComment record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.MocComment cloneOfExisting = (Database.MocComment)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new MocComment object using the data from the existing record, updated with what is in the DTO.
			//
			Database.MocComment mocComment = (Database.MocComment)_context.Entry(existing).GetDatabaseValues().ToObject();
			mocComment.ApplyDTO(mocCommentDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (mocComment.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.MocComment record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (mocComment.postedDate.Kind != DateTimeKind.Utc)
			{
				mocComment.postedDate = mocComment.postedDate.ToUniversalTime();
			}

			EntityEntry<Database.MocComment> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(mocComment);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.MocComment entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocComment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocComment.CreateAnonymousWithFirstLevelSubObjects(mocComment)),
					null);


				return Ok(Database.MocComment.CreateAnonymous(mocComment));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.MocComment entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocComment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocComment.CreateAnonymousWithFirstLevelSubObjects(mocComment)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new MocComment record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocComment", Name = "MocComment")]
		public async Task<IActionResult> PostMocComment([FromBody]Database.MocComment.MocCommentDTO mocCommentDTO, CancellationToken cancellationToken = default)
		{
			if (mocCommentDTO == null)
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
			// Create a new MocComment object using the data from the DTO
			//
			Database.MocComment mocComment = Database.MocComment.FromDTO(mocCommentDTO);

			try
			{
				if (mocComment.postedDate.Kind != DateTimeKind.Utc)
				{
					mocComment.postedDate = mocComment.postedDate.ToUniversalTime();
				}

				mocComment.objectGuid = Guid.NewGuid();
				_context.MocComments.Add(mocComment);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.MocComment entity successfully created.",
					true,
					mocComment.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.MocComment.CreateAnonymousWithFirstLevelSubObjects(mocComment)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.MocComment entity creation failed.", false, mocComment.id.ToString(), "", JsonSerializer.Serialize(mocComment), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "MocComment", mocComment.id, mocComment.id.ToString()));

			return CreatedAtRoute("MocComment", new { id = mocComment.id }, Database.MocComment.CreateAnonymousWithFirstLevelSubObjects(mocComment));
		}



        /// <summary>
        /// 
        /// This deletes a MocComment record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/MocComment/{id}")]
		[Route("api/MocComment")]
		public async Task<IActionResult> DeleteMocComment(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.MocComment> query = (from x in _context.MocComments
				where
				(x.id == id)
				select x);


			Database.MocComment mocComment = await query.FirstOrDefaultAsync(cancellationToken);

			if (mocComment == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.MocComment DELETE", id.ToString(), new Exception("No BMC.MocComment entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.MocComment cloneOfExisting = (Database.MocComment)_context.Entry(mocComment).GetDatabaseValues().ToObject();


			try
			{
				mocComment.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.MocComment entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocComment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocComment.CreateAnonymousWithFirstLevelSubObjects(mocComment)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.MocComment entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.MocComment.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.MocComment.CreateAnonymousWithFirstLevelSubObjects(mocComment)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of MocComment records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/MocComments/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? publishedMocId = null,
			Guid? commenterTenantGuid = null,
			string commentText = null,
			DateTime? postedDate = null,
			int? mocCommentId = null,
			bool? isEdited = null,
			bool? isHidden = null,
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
			if (postedDate.HasValue == true && postedDate.Value.Kind != DateTimeKind.Utc)
			{
				postedDate = postedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.MocComment> query = (from mc in _context.MocComments select mc);
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(mc => mc.publishedMocId == publishedMocId.Value);
			}
			if (commenterTenantGuid.HasValue == true)
			{
				query = query.Where(mc => mc.commenterTenantGuid == commenterTenantGuid);
			}
			if (string.IsNullOrEmpty(commentText) == false)
			{
				query = query.Where(mc => mc.commentText == commentText);
			}
			if (postedDate.HasValue == true)
			{
				query = query.Where(mc => mc.postedDate == postedDate.Value);
			}
			if (mocCommentId.HasValue == true)
			{
				query = query.Where(mc => mc.mocCommentId == mocCommentId.Value);
			}
			if (isEdited.HasValue == true)
			{
				query = query.Where(mc => mc.isEdited == isEdited.Value);
			}
			if (isHidden.HasValue == true)
			{
				query = query.Where(mc => mc.isHidden == isHidden.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(mc => mc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(mc => mc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(mc => mc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(mc => mc.deleted == false);
				}
			}
			else
			{
				query = query.Where(mc => mc.active == true);
				query = query.Where(mc => mc.deleted == false);
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.MocComment.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/MocComment/CreateAuditEvent")]
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
