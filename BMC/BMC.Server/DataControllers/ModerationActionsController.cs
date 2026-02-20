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
    /// This auto generated class provides the basic CRUD operations for the ModerationAction entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ModerationAction entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ModerationActionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 100;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		private BMCContext _context;

		private ILogger<ModerationActionsController> _logger;

		public ModerationActionsController(BMCContext context, ILogger<ModerationActionsController> logger) : base("BMC", "ModerationAction")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ModerationActions filtered by the parameters provided.
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
		[Route("api/ModerationActions")]
		public async Task<IActionResult> GetModerationActions(
			Guid? moderatorTenantGuid = null,
			string actionType = null,
			Guid? targetTenantGuid = null,
			string targetEntityType = null,
			long? targetEntityId = null,
			string reason = null,
			DateTime? actionDate = null,
			int? contentReportId = null,
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
			if (actionDate.HasValue == true && actionDate.Value.Kind != DateTimeKind.Utc)
			{
				actionDate = actionDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ModerationAction> query = (from ma in _context.ModerationActions select ma);
			if (moderatorTenantGuid.HasValue == true)
			{
				query = query.Where(ma => ma.moderatorTenantGuid == moderatorTenantGuid);
			}
			if (string.IsNullOrEmpty(actionType) == false)
			{
				query = query.Where(ma => ma.actionType == actionType);
			}
			if (targetTenantGuid.HasValue == true)
			{
				query = query.Where(ma => ma.targetTenantGuid == targetTenantGuid);
			}
			if (string.IsNullOrEmpty(targetEntityType) == false)
			{
				query = query.Where(ma => ma.targetEntityType == targetEntityType);
			}
			if (targetEntityId.HasValue == true)
			{
				query = query.Where(ma => ma.targetEntityId == targetEntityId.Value);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(ma => ma.reason == reason);
			}
			if (actionDate.HasValue == true)
			{
				query = query.Where(ma => ma.actionDate == actionDate.Value);
			}
			if (contentReportId.HasValue == true)
			{
				query = query.Where(ma => ma.contentReportId == contentReportId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ma => ma.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ma => ma.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ma => ma.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ma => ma.deleted == false);
				}
			}
			else
			{
				query = query.Where(ma => ma.active == true);
				query = query.Where(ma => ma.deleted == false);
			}

			query = query.OrderBy(ma => ma.actionType).ThenBy(ma => ma.targetEntityType);


			//
			// Add the any string contains parameter to span all the string fields on the Moderation Action, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.actionType.Contains(anyStringContains)
			       || x.targetEntityType.Contains(anyStringContains)
			       || x.reason.Contains(anyStringContains)
			       || (includeRelations == true && x.contentReport.reportedEntityType.Contains(anyStringContains))
			       || (includeRelations == true && x.contentReport.description.Contains(anyStringContains))
			       || (includeRelations == true && x.contentReport.status.Contains(anyStringContains))
			       || (includeRelations == true && x.contentReport.reviewNotes.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.contentReport);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.ModerationAction> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ModerationAction moderationAction in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(moderationAction, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ModerationAction Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ModerationAction Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ModerationActions filtered by the parameters provided.  Its query is similar to the GetModerationActions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModerationActions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			Guid? moderatorTenantGuid = null,
			string actionType = null,
			Guid? targetTenantGuid = null,
			string targetEntityType = null,
			long? targetEntityId = null,
			string reason = null,
			DateTime? actionDate = null,
			int? contentReportId = null,
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
			if (actionDate.HasValue == true && actionDate.Value.Kind != DateTimeKind.Utc)
			{
				actionDate = actionDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ModerationAction> query = (from ma in _context.ModerationActions select ma);
			if (moderatorTenantGuid.HasValue == true)
			{
				query = query.Where(ma => ma.moderatorTenantGuid == moderatorTenantGuid);
			}
			if (actionType != null)
			{
				query = query.Where(ma => ma.actionType == actionType);
			}
			if (targetTenantGuid.HasValue == true)
			{
				query = query.Where(ma => ma.targetTenantGuid == targetTenantGuid);
			}
			if (targetEntityType != null)
			{
				query = query.Where(ma => ma.targetEntityType == targetEntityType);
			}
			if (targetEntityId.HasValue == true)
			{
				query = query.Where(ma => ma.targetEntityId == targetEntityId.Value);
			}
			if (reason != null)
			{
				query = query.Where(ma => ma.reason == reason);
			}
			if (actionDate.HasValue == true)
			{
				query = query.Where(ma => ma.actionDate == actionDate.Value);
			}
			if (contentReportId.HasValue == true)
			{
				query = query.Where(ma => ma.contentReportId == contentReportId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ma => ma.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ma => ma.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ma => ma.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ma => ma.deleted == false);
				}
			}
			else
			{
				query = query.Where(ma => ma.active == true);
				query = query.Where(ma => ma.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Moderation Action, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.actionType.Contains(anyStringContains)
			       || x.targetEntityType.Contains(anyStringContains)
			       || x.reason.Contains(anyStringContains)
			       || x.contentReport.reportedEntityType.Contains(anyStringContains)
			       || x.contentReport.description.Contains(anyStringContains)
			       || x.contentReport.status.Contains(anyStringContains)
			       || x.contentReport.reviewNotes.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ModerationAction by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModerationAction/{id}")]
		public async Task<IActionResult> GetModerationAction(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ModerationAction> query = (from ma in _context.ModerationActions where
							(ma.id == id) &&
							(userIsAdmin == true || ma.deleted == false) &&
							(userIsWriter == true || ma.active == true)
					select ma);

				if (includeRelations == true)
				{
					query = query.Include(x => x.contentReport);
					query = query.AsSplitQuery();
				}

				Database.ModerationAction materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ModerationAction Entity was read with Admin privilege." : "BMC.ModerationAction Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModerationAction", materialized.id, materialized.actionType));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ModerationAction entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ModerationAction.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ModerationAction.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ModerationAction record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ModerationAction/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutModerationAction(int id, [FromBody]Database.ModerationAction.ModerationActionDTO moderationActionDTO, CancellationToken cancellationToken = default)
		{
			if (moderationActionDTO == null)
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



			if (id != moderationActionDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ModerationAction> query = (from x in _context.ModerationActions
				where
				(x.id == id)
				select x);


			Database.ModerationAction existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModerationAction PUT", id.ToString(), new Exception("No BMC.ModerationAction entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (moderationActionDTO.objectGuid == Guid.Empty)
            {
                moderationActionDTO.objectGuid = existing.objectGuid;
            }
            else if (moderationActionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ModerationAction record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ModerationAction cloneOfExisting = (Database.ModerationAction)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ModerationAction object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ModerationAction moderationAction = (Database.ModerationAction)_context.Entry(existing).GetDatabaseValues().ToObject();
			moderationAction.ApplyDTO(moderationActionDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (moderationAction.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ModerationAction record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (moderationAction.actionType != null && moderationAction.actionType.Length > 100)
			{
				moderationAction.actionType = moderationAction.actionType.Substring(0, 100);
			}

			if (moderationAction.targetEntityType != null && moderationAction.targetEntityType.Length > 100)
			{
				moderationAction.targetEntityType = moderationAction.targetEntityType.Substring(0, 100);
			}

			if (moderationAction.actionDate.Kind != DateTimeKind.Utc)
			{
				moderationAction.actionDate = moderationAction.actionDate.ToUniversalTime();
			}

			EntityEntry<Database.ModerationAction> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(moderationAction);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ModerationAction entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModerationAction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModerationAction.CreateAnonymousWithFirstLevelSubObjects(moderationAction)),
					null);


				return Ok(Database.ModerationAction.CreateAnonymous(moderationAction));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ModerationAction entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModerationAction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModerationAction.CreateAnonymousWithFirstLevelSubObjects(moderationAction)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ModerationAction record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModerationAction", Name = "ModerationAction")]
		public async Task<IActionResult> PostModerationAction([FromBody]Database.ModerationAction.ModerationActionDTO moderationActionDTO, CancellationToken cancellationToken = default)
		{
			if (moderationActionDTO == null)
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
			// Create a new ModerationAction object using the data from the DTO
			//
			Database.ModerationAction moderationAction = Database.ModerationAction.FromDTO(moderationActionDTO);

			try
			{
				if (moderationAction.actionType != null && moderationAction.actionType.Length > 100)
				{
					moderationAction.actionType = moderationAction.actionType.Substring(0, 100);
				}

				if (moderationAction.targetEntityType != null && moderationAction.targetEntityType.Length > 100)
				{
					moderationAction.targetEntityType = moderationAction.targetEntityType.Substring(0, 100);
				}

				if (moderationAction.actionDate.Kind != DateTimeKind.Utc)
				{
					moderationAction.actionDate = moderationAction.actionDate.ToUniversalTime();
				}

				moderationAction.objectGuid = Guid.NewGuid();
				_context.ModerationActions.Add(moderationAction);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ModerationAction entity successfully created.",
					true,
					moderationAction.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ModerationAction.CreateAnonymousWithFirstLevelSubObjects(moderationAction)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ModerationAction entity creation failed.", false, moderationAction.id.ToString(), "", JsonSerializer.Serialize(moderationAction), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ModerationAction", moderationAction.id, moderationAction.actionType));

			return CreatedAtRoute("ModerationAction", new { id = moderationAction.id }, Database.ModerationAction.CreateAnonymousWithFirstLevelSubObjects(moderationAction));
		}



        /// <summary>
        /// 
        /// This deletes a ModerationAction record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ModerationAction/{id}")]
		[Route("api/ModerationAction")]
		public async Task<IActionResult> DeleteModerationAction(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ModerationAction> query = (from x in _context.ModerationActions
				where
				(x.id == id)
				select x);


			Database.ModerationAction moderationAction = await query.FirstOrDefaultAsync(cancellationToken);

			if (moderationAction == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ModerationAction DELETE", id.ToString(), new Exception("No BMC.ModerationAction entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ModerationAction cloneOfExisting = (Database.ModerationAction)_context.Entry(moderationAction).GetDatabaseValues().ToObject();


			try
			{
				moderationAction.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ModerationAction entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModerationAction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModerationAction.CreateAnonymousWithFirstLevelSubObjects(moderationAction)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ModerationAction entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ModerationAction.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ModerationAction.CreateAnonymousWithFirstLevelSubObjects(moderationAction)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ModerationAction records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ModerationActions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			Guid? moderatorTenantGuid = null,
			string actionType = null,
			Guid? targetTenantGuid = null,
			string targetEntityType = null,
			long? targetEntityId = null,
			string reason = null,
			DateTime? actionDate = null,
			int? contentReportId = null,
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
			if (actionDate.HasValue == true && actionDate.Value.Kind != DateTimeKind.Utc)
			{
				actionDate = actionDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ModerationAction> query = (from ma in _context.ModerationActions select ma);
			if (moderatorTenantGuid.HasValue == true)
			{
				query = query.Where(ma => ma.moderatorTenantGuid == moderatorTenantGuid);
			}
			if (string.IsNullOrEmpty(actionType) == false)
			{
				query = query.Where(ma => ma.actionType == actionType);
			}
			if (targetTenantGuid.HasValue == true)
			{
				query = query.Where(ma => ma.targetTenantGuid == targetTenantGuid);
			}
			if (string.IsNullOrEmpty(targetEntityType) == false)
			{
				query = query.Where(ma => ma.targetEntityType == targetEntityType);
			}
			if (targetEntityId.HasValue == true)
			{
				query = query.Where(ma => ma.targetEntityId == targetEntityId.Value);
			}
			if (string.IsNullOrEmpty(reason) == false)
			{
				query = query.Where(ma => ma.reason == reason);
			}
			if (actionDate.HasValue == true)
			{
				query = query.Where(ma => ma.actionDate == actionDate.Value);
			}
			if (contentReportId.HasValue == true)
			{
				query = query.Where(ma => ma.contentReportId == contentReportId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ma => ma.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ma => ma.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ma => ma.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ma => ma.deleted == false);
				}
			}
			else
			{
				query = query.Where(ma => ma.active == true);
				query = query.Where(ma => ma.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Moderation Action, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.actionType.Contains(anyStringContains)
			       || x.targetEntityType.Contains(anyStringContains)
			       || x.reason.Contains(anyStringContains)
			       || x.contentReport.reportedEntityType.Contains(anyStringContains)
			       || x.contentReport.description.Contains(anyStringContains)
			       || x.contentReport.status.Contains(anyStringContains)
			       || x.contentReport.reviewNotes.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.actionType).ThenBy(x => x.targetEntityType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ModerationAction.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ModerationAction/CreateAuditEvent")]
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
