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
    /// This auto generated class provides the basic CRUD operations for the ContentReportReason entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ContentReportReason entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ContentReportReasonsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<ContentReportReasonsController> _logger;

		public ContentReportReasonsController(BMCContext context, ILogger<ContentReportReasonsController> logger) : base("BMC", "ContentReportReason")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ContentReportReasons filtered by the parameters provided.
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
		[Route("api/ContentReportReasons")]
		public async Task<IActionResult> GetContentReportReasons(
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

			IQueryable<Database.ContentReportReason> query = (from crr in _context.ContentReportReasons select crr);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(crr => crr.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(crr => crr.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(crr => crr.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(crr => crr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(crr => crr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(crr => crr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(crr => crr.deleted == false);
				}
			}
			else
			{
				query = query.Where(crr => crr.active == true);
				query = query.Where(crr => crr.deleted == false);
			}

			query = query.OrderBy(crr => crr.sequence).ThenBy(crr => crr.name).ThenBy(crr => crr.description);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Content Report Reason, or on an any of the string fields on its immediate relations
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

			query = query.AsNoTracking();
			
			List<Database.ContentReportReason> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ContentReportReason contentReportReason in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(contentReportReason, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ContentReportReason Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ContentReportReason Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ContentReportReasons filtered by the parameters provided.  Its query is similar to the GetContentReportReasons method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContentReportReasons/RowCount")]
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

			IQueryable<Database.ContentReportReason> query = (from crr in _context.ContentReportReasons select crr);
			if (name != null)
			{
				query = query.Where(crr => crr.name == name);
			}
			if (description != null)
			{
				query = query.Where(crr => crr.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(crr => crr.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(crr => crr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(crr => crr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(crr => crr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(crr => crr.deleted == false);
				}
			}
			else
			{
				query = query.Where(crr => crr.active == true);
				query = query.Where(crr => crr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Content Report Reason, or on an any of the string fields on its immediate relations
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
        /// This gets a single ContentReportReason by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContentReportReason/{id}")]
		public async Task<IActionResult> GetContentReportReason(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ContentReportReason> query = (from crr in _context.ContentReportReasons where
							(crr.id == id) &&
							(userIsAdmin == true || crr.deleted == false) &&
							(userIsWriter == true || crr.active == true)
					select crr);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.ContentReportReason materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ContentReportReason Entity was read with Admin privilege." : "BMC.ContentReportReason Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContentReportReason", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ContentReportReason entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ContentReportReason.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ContentReportReason.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ContentReportReason record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ContentReportReason/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutContentReportReason(int id, [FromBody]Database.ContentReportReason.ContentReportReasonDTO contentReportReasonDTO, CancellationToken cancellationToken = default)
		{
			if (contentReportReasonDTO == null)
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



			if (id != contentReportReasonDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ContentReportReason> query = (from x in _context.ContentReportReasons
				where
				(x.id == id)
				select x);


			Database.ContentReportReason existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ContentReportReason PUT", id.ToString(), new Exception("No BMC.ContentReportReason entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (contentReportReasonDTO.objectGuid == Guid.Empty)
            {
                contentReportReasonDTO.objectGuid = existing.objectGuid;
            }
            else if (contentReportReasonDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ContentReportReason record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ContentReportReason cloneOfExisting = (Database.ContentReportReason)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ContentReportReason object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ContentReportReason contentReportReason = (Database.ContentReportReason)_context.Entry(existing).GetDatabaseValues().ToObject();
			contentReportReason.ApplyDTO(contentReportReasonDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (contentReportReason.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ContentReportReason record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (contentReportReason.name != null && contentReportReason.name.Length > 100)
			{
				contentReportReason.name = contentReportReason.name.Substring(0, 100);
			}

			if (contentReportReason.description != null && contentReportReason.description.Length > 500)
			{
				contentReportReason.description = contentReportReason.description.Substring(0, 500);
			}

			EntityEntry<Database.ContentReportReason> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(contentReportReason);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ContentReportReason entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContentReportReason.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContentReportReason.CreateAnonymousWithFirstLevelSubObjects(contentReportReason)),
					null);


				return Ok(Database.ContentReportReason.CreateAnonymous(contentReportReason));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ContentReportReason entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContentReportReason.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContentReportReason.CreateAnonymousWithFirstLevelSubObjects(contentReportReason)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ContentReportReason record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContentReportReason", Name = "ContentReportReason")]
		public async Task<IActionResult> PostContentReportReason([FromBody]Database.ContentReportReason.ContentReportReasonDTO contentReportReasonDTO, CancellationToken cancellationToken = default)
		{
			if (contentReportReasonDTO == null)
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
			// Create a new ContentReportReason object using the data from the DTO
			//
			Database.ContentReportReason contentReportReason = Database.ContentReportReason.FromDTO(contentReportReasonDTO);

			try
			{
				if (contentReportReason.name != null && contentReportReason.name.Length > 100)
				{
					contentReportReason.name = contentReportReason.name.Substring(0, 100);
				}

				if (contentReportReason.description != null && contentReportReason.description.Length > 500)
				{
					contentReportReason.description = contentReportReason.description.Substring(0, 500);
				}

				contentReportReason.objectGuid = Guid.NewGuid();
				_context.ContentReportReasons.Add(contentReportReason);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ContentReportReason entity successfully created.",
					true,
					contentReportReason.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ContentReportReason.CreateAnonymousWithFirstLevelSubObjects(contentReportReason)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ContentReportReason entity creation failed.", false, contentReportReason.id.ToString(), "", JsonSerializer.Serialize(contentReportReason), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContentReportReason", contentReportReason.id, contentReportReason.name));

			return CreatedAtRoute("ContentReportReason", new { id = contentReportReason.id }, Database.ContentReportReason.CreateAnonymousWithFirstLevelSubObjects(contentReportReason));
		}



        /// <summary>
        /// 
        /// This deletes a ContentReportReason record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContentReportReason/{id}")]
		[Route("api/ContentReportReason")]
		public async Task<IActionResult> DeleteContentReportReason(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ContentReportReason> query = (from x in _context.ContentReportReasons
				where
				(x.id == id)
				select x);


			Database.ContentReportReason contentReportReason = await query.FirstOrDefaultAsync(cancellationToken);

			if (contentReportReason == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ContentReportReason DELETE", id.ToString(), new Exception("No BMC.ContentReportReason entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ContentReportReason cloneOfExisting = (Database.ContentReportReason)_context.Entry(contentReportReason).GetDatabaseValues().ToObject();


			try
			{
				contentReportReason.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ContentReportReason entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContentReportReason.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContentReportReason.CreateAnonymousWithFirstLevelSubObjects(contentReportReason)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ContentReportReason entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContentReportReason.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContentReportReason.CreateAnonymousWithFirstLevelSubObjects(contentReportReason)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ContentReportReason records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ContentReportReasons/ListData")]
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

			IQueryable<Database.ContentReportReason> query = (from crr in _context.ContentReportReasons select crr);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(crr => crr.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(crr => crr.description == description);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(crr => crr.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(crr => crr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(crr => crr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(crr => crr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(crr => crr.deleted == false);
				}
			}
			else
			{
				query = query.Where(crr => crr.active == true);
				query = query.Where(crr => crr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Content Report Reason, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.ContentReportReason.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ContentReportReason/CreateAuditEvent")]
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
