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
    /// This auto generated class provides the basic CRUD operations for the ContentReport entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ContentReport entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ContentReportsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<ContentReportsController> _logger;

		public ContentReportsController(BMCContext context, ILogger<ContentReportsController> logger) : base("BMC", "ContentReport")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ContentReports filtered by the parameters provided.
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
		[Route("api/ContentReports")]
		public async Task<IActionResult> GetContentReports(
			int? contentReportReasonId = null,
			Guid? reporterTenantGuid = null,
			string reportedEntityType = null,
			long? reportedEntityId = null,
			string description = null,
			string status = null,
			DateTime? reportedDate = null,
			DateTime? reviewedDate = null,
			Guid? reviewerTenantGuid = null,
			string reviewNotes = null,
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
			if (reportedDate.HasValue == true && reportedDate.Value.Kind != DateTimeKind.Utc)
			{
				reportedDate = reportedDate.Value.ToUniversalTime();
			}

			if (reviewedDate.HasValue == true && reviewedDate.Value.Kind != DateTimeKind.Utc)
			{
				reviewedDate = reviewedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ContentReport> query = (from cr in _context.ContentReports select cr);
			if (contentReportReasonId.HasValue == true)
			{
				query = query.Where(cr => cr.contentReportReasonId == contentReportReasonId.Value);
			}
			if (reporterTenantGuid.HasValue == true)
			{
				query = query.Where(cr => cr.reporterTenantGuid == reporterTenantGuid);
			}
			if (string.IsNullOrEmpty(reportedEntityType) == false)
			{
				query = query.Where(cr => cr.reportedEntityType == reportedEntityType);
			}
			if (reportedEntityId.HasValue == true)
			{
				query = query.Where(cr => cr.reportedEntityId == reportedEntityId.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cr => cr.description == description);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(cr => cr.status == status);
			}
			if (reportedDate.HasValue == true)
			{
				query = query.Where(cr => cr.reportedDate == reportedDate.Value);
			}
			if (reviewedDate.HasValue == true)
			{
				query = query.Where(cr => cr.reviewedDate == reviewedDate.Value);
			}
			if (reviewerTenantGuid.HasValue == true)
			{
				query = query.Where(cr => cr.reviewerTenantGuid == reviewerTenantGuid);
			}
			if (string.IsNullOrEmpty(reviewNotes) == false)
			{
				query = query.Where(cr => cr.reviewNotes == reviewNotes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cr => cr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cr => cr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cr => cr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cr => cr.deleted == false);
				}
			}
			else
			{
				query = query.Where(cr => cr.active == true);
				query = query.Where(cr => cr.deleted == false);
			}

			query = query.OrderBy(cr => cr.reportedEntityType).ThenBy(cr => cr.status);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.contentReportReason);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Content Report, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reportedEntityType.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.reviewNotes.Contains(anyStringContains)
			       || (includeRelations == true && x.contentReportReason.name.Contains(anyStringContains))
			       || (includeRelations == true && x.contentReportReason.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ContentReport> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ContentReport contentReport in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(contentReport, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ContentReport Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ContentReport Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ContentReports filtered by the parameters provided.  Its query is similar to the GetContentReports method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContentReports/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? contentReportReasonId = null,
			Guid? reporterTenantGuid = null,
			string reportedEntityType = null,
			long? reportedEntityId = null,
			string description = null,
			string status = null,
			DateTime? reportedDate = null,
			DateTime? reviewedDate = null,
			Guid? reviewerTenantGuid = null,
			string reviewNotes = null,
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
			if (reportedDate.HasValue == true && reportedDate.Value.Kind != DateTimeKind.Utc)
			{
				reportedDate = reportedDate.Value.ToUniversalTime();
			}

			if (reviewedDate.HasValue == true && reviewedDate.Value.Kind != DateTimeKind.Utc)
			{
				reviewedDate = reviewedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ContentReport> query = (from cr in _context.ContentReports select cr);
			if (contentReportReasonId.HasValue == true)
			{
				query = query.Where(cr => cr.contentReportReasonId == contentReportReasonId.Value);
			}
			if (reporterTenantGuid.HasValue == true)
			{
				query = query.Where(cr => cr.reporterTenantGuid == reporterTenantGuid);
			}
			if (reportedEntityType != null)
			{
				query = query.Where(cr => cr.reportedEntityType == reportedEntityType);
			}
			if (reportedEntityId.HasValue == true)
			{
				query = query.Where(cr => cr.reportedEntityId == reportedEntityId.Value);
			}
			if (description != null)
			{
				query = query.Where(cr => cr.description == description);
			}
			if (status != null)
			{
				query = query.Where(cr => cr.status == status);
			}
			if (reportedDate.HasValue == true)
			{
				query = query.Where(cr => cr.reportedDate == reportedDate.Value);
			}
			if (reviewedDate.HasValue == true)
			{
				query = query.Where(cr => cr.reviewedDate == reviewedDate.Value);
			}
			if (reviewerTenantGuid.HasValue == true)
			{
				query = query.Where(cr => cr.reviewerTenantGuid == reviewerTenantGuid);
			}
			if (reviewNotes != null)
			{
				query = query.Where(cr => cr.reviewNotes == reviewNotes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cr => cr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cr => cr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cr => cr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cr => cr.deleted == false);
				}
			}
			else
			{
				query = query.Where(cr => cr.active == true);
				query = query.Where(cr => cr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Content Report, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reportedEntityType.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.reviewNotes.Contains(anyStringContains)
			       || x.contentReportReason.name.Contains(anyStringContains)
			       || x.contentReportReason.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ContentReport by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContentReport/{id}")]
		public async Task<IActionResult> GetContentReport(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ContentReport> query = (from cr in _context.ContentReports where
							(cr.id == id) &&
							(userIsAdmin == true || cr.deleted == false) &&
							(userIsWriter == true || cr.active == true)
					select cr);

				if (includeRelations == true)
				{
					query = query.Include(x => x.contentReportReason);
					query = query.AsSplitQuery();
				}

				Database.ContentReport materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ContentReport Entity was read with Admin privilege." : "BMC.ContentReport Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContentReport", materialized.id, materialized.reportedEntityType));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ContentReport entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ContentReport.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ContentReport.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ContentReport record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ContentReport/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutContentReport(int id, [FromBody]Database.ContentReport.ContentReportDTO contentReportDTO, CancellationToken cancellationToken = default)
		{
			if (contentReportDTO == null)
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



			if (id != contentReportDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ContentReport> query = (from x in _context.ContentReports
				where
				(x.id == id)
				select x);


			Database.ContentReport existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ContentReport PUT", id.ToString(), new Exception("No BMC.ContentReport entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (contentReportDTO.objectGuid == Guid.Empty)
            {
                contentReportDTO.objectGuid = existing.objectGuid;
            }
            else if (contentReportDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ContentReport record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ContentReport cloneOfExisting = (Database.ContentReport)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ContentReport object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ContentReport contentReport = (Database.ContentReport)_context.Entry(existing).GetDatabaseValues().ToObject();
			contentReport.ApplyDTO(contentReportDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (contentReport.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ContentReport record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (contentReport.reportedEntityType != null && contentReport.reportedEntityType.Length > 100)
			{
				contentReport.reportedEntityType = contentReport.reportedEntityType.Substring(0, 100);
			}

			if (contentReport.status != null && contentReport.status.Length > 50)
			{
				contentReport.status = contentReport.status.Substring(0, 50);
			}

			if (contentReport.reportedDate.Kind != DateTimeKind.Utc)
			{
				contentReport.reportedDate = contentReport.reportedDate.ToUniversalTime();
			}

			if (contentReport.reviewedDate.HasValue == true && contentReport.reviewedDate.Value.Kind != DateTimeKind.Utc)
			{
				contentReport.reviewedDate = contentReport.reviewedDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.ContentReport> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(contentReport);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ContentReport entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContentReport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContentReport.CreateAnonymousWithFirstLevelSubObjects(contentReport)),
					null);


				return Ok(Database.ContentReport.CreateAnonymous(contentReport));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ContentReport entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContentReport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContentReport.CreateAnonymousWithFirstLevelSubObjects(contentReport)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ContentReport record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContentReport", Name = "ContentReport")]
		public async Task<IActionResult> PostContentReport([FromBody]Database.ContentReport.ContentReportDTO contentReportDTO, CancellationToken cancellationToken = default)
		{
			if (contentReportDTO == null)
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
			// Create a new ContentReport object using the data from the DTO
			//
			Database.ContentReport contentReport = Database.ContentReport.FromDTO(contentReportDTO);

			try
			{
				if (contentReport.reportedEntityType != null && contentReport.reportedEntityType.Length > 100)
				{
					contentReport.reportedEntityType = contentReport.reportedEntityType.Substring(0, 100);
				}

				if (contentReport.status != null && contentReport.status.Length > 50)
				{
					contentReport.status = contentReport.status.Substring(0, 50);
				}

				if (contentReport.reportedDate.Kind != DateTimeKind.Utc)
				{
					contentReport.reportedDate = contentReport.reportedDate.ToUniversalTime();
				}

				if (contentReport.reviewedDate.HasValue == true && contentReport.reviewedDate.Value.Kind != DateTimeKind.Utc)
				{
					contentReport.reviewedDate = contentReport.reviewedDate.Value.ToUniversalTime();
				}

				contentReport.objectGuid = Guid.NewGuid();
				_context.ContentReports.Add(contentReport);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ContentReport entity successfully created.",
					true,
					contentReport.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ContentReport.CreateAnonymousWithFirstLevelSubObjects(contentReport)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ContentReport entity creation failed.", false, contentReport.id.ToString(), "", JsonSerializer.Serialize(contentReport), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ContentReport", contentReport.id, contentReport.reportedEntityType));

			return CreatedAtRoute("ContentReport", new { id = contentReport.id }, Database.ContentReport.CreateAnonymousWithFirstLevelSubObjects(contentReport));
		}



        /// <summary>
        /// 
        /// This deletes a ContentReport record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ContentReport/{id}")]
		[Route("api/ContentReport")]
		public async Task<IActionResult> DeleteContentReport(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ContentReport> query = (from x in _context.ContentReports
				where
				(x.id == id)
				select x);


			Database.ContentReport contentReport = await query.FirstOrDefaultAsync(cancellationToken);

			if (contentReport == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ContentReport DELETE", id.ToString(), new Exception("No BMC.ContentReport entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ContentReport cloneOfExisting = (Database.ContentReport)_context.Entry(contentReport).GetDatabaseValues().ToObject();


			try
			{
				contentReport.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ContentReport entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContentReport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContentReport.CreateAnonymousWithFirstLevelSubObjects(contentReport)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ContentReport entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ContentReport.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ContentReport.CreateAnonymousWithFirstLevelSubObjects(contentReport)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ContentReport records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ContentReports/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? contentReportReasonId = null,
			Guid? reporterTenantGuid = null,
			string reportedEntityType = null,
			long? reportedEntityId = null,
			string description = null,
			string status = null,
			DateTime? reportedDate = null,
			DateTime? reviewedDate = null,
			Guid? reviewerTenantGuid = null,
			string reviewNotes = null,
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
			if (reportedDate.HasValue == true && reportedDate.Value.Kind != DateTimeKind.Utc)
			{
				reportedDate = reportedDate.Value.ToUniversalTime();
			}

			if (reviewedDate.HasValue == true && reviewedDate.Value.Kind != DateTimeKind.Utc)
			{
				reviewedDate = reviewedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ContentReport> query = (from cr in _context.ContentReports select cr);
			if (contentReportReasonId.HasValue == true)
			{
				query = query.Where(cr => cr.contentReportReasonId == contentReportReasonId.Value);
			}
			if (reporterTenantGuid.HasValue == true)
			{
				query = query.Where(cr => cr.reporterTenantGuid == reporterTenantGuid);
			}
			if (string.IsNullOrEmpty(reportedEntityType) == false)
			{
				query = query.Where(cr => cr.reportedEntityType == reportedEntityType);
			}
			if (reportedEntityId.HasValue == true)
			{
				query = query.Where(cr => cr.reportedEntityId == reportedEntityId.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cr => cr.description == description);
			}
			if (string.IsNullOrEmpty(status) == false)
			{
				query = query.Where(cr => cr.status == status);
			}
			if (reportedDate.HasValue == true)
			{
				query = query.Where(cr => cr.reportedDate == reportedDate.Value);
			}
			if (reviewedDate.HasValue == true)
			{
				query = query.Where(cr => cr.reviewedDate == reviewedDate.Value);
			}
			if (reviewerTenantGuid.HasValue == true)
			{
				query = query.Where(cr => cr.reviewerTenantGuid == reviewerTenantGuid);
			}
			if (string.IsNullOrEmpty(reviewNotes) == false)
			{
				query = query.Where(cr => cr.reviewNotes == reviewNotes);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cr => cr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cr => cr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cr => cr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cr => cr.deleted == false);
				}
			}
			else
			{
				query = query.Where(cr => cr.active == true);
				query = query.Where(cr => cr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Content Report, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reportedEntityType.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.status.Contains(anyStringContains)
			       || x.reviewNotes.Contains(anyStringContains)
			       || x.contentReportReason.name.Contains(anyStringContains)
			       || x.contentReportReason.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.reportedEntityType).ThenBy(x => x.status);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ContentReport.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ContentReport/CreateAuditEvent")]
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
