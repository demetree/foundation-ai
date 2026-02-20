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
    /// This auto generated class provides the basic CRUD operations for the ExportFormat entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ExportFormat entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ExportFormatsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<ExportFormatsController> _logger;

		public ExportFormatsController(BMCContext context, ILogger<ExportFormatsController> logger) : base("BMC", "ExportFormat")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ExportFormats filtered by the parameters provided.
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
		[Route("api/ExportFormats")]
		public async Task<IActionResult> GetExportFormats(
			string name = null,
			string description = null,
			string fileExtension = null,
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

			IQueryable<Database.ExportFormat> query = (from ef in _context.ExportFormats select ef);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ef => ef.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ef => ef.description == description);
			}
			if (string.IsNullOrEmpty(fileExtension) == false)
			{
				query = query.Where(ef => ef.fileExtension == fileExtension);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ef => ef.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ef => ef.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ef => ef.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ef => ef.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ef => ef.deleted == false);
				}
			}
			else
			{
				query = query.Where(ef => ef.active == true);
				query = query.Where(ef => ef.deleted == false);
			}

			query = query.OrderBy(ef => ef.sequence).ThenBy(ef => ef.name).ThenBy(ef => ef.description).ThenBy(ef => ef.fileExtension);


			//
			// Add the any string contains parameter to span all the string fields on the Export Format, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.fileExtension.Contains(anyStringContains)
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
			
			List<Database.ExportFormat> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ExportFormat exportFormat in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(exportFormat, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ExportFormat Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ExportFormat Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ExportFormats filtered by the parameters provided.  Its query is similar to the GetExportFormats method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExportFormats/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string fileExtension = null,
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

			IQueryable<Database.ExportFormat> query = (from ef in _context.ExportFormats select ef);
			if (name != null)
			{
				query = query.Where(ef => ef.name == name);
			}
			if (description != null)
			{
				query = query.Where(ef => ef.description == description);
			}
			if (fileExtension != null)
			{
				query = query.Where(ef => ef.fileExtension == fileExtension);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ef => ef.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ef => ef.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ef => ef.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ef => ef.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ef => ef.deleted == false);
				}
			}
			else
			{
				query = query.Where(ef => ef.active == true);
				query = query.Where(ef => ef.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Export Format, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.fileExtension.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ExportFormat by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExportFormat/{id}")]
		public async Task<IActionResult> GetExportFormat(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ExportFormat> query = (from ef in _context.ExportFormats where
							(ef.id == id) &&
							(userIsAdmin == true || ef.deleted == false) &&
							(userIsWriter == true || ef.active == true)
					select ef);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.ExportFormat materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ExportFormat Entity was read with Admin privilege." : "BMC.ExportFormat Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ExportFormat", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ExportFormat entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ExportFormat.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ExportFormat.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ExportFormat record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ExportFormat/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutExportFormat(int id, [FromBody]Database.ExportFormat.ExportFormatDTO exportFormatDTO, CancellationToken cancellationToken = default)
		{
			if (exportFormatDTO == null)
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



			if (id != exportFormatDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ExportFormat> query = (from x in _context.ExportFormats
				where
				(x.id == id)
				select x);


			Database.ExportFormat existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ExportFormat PUT", id.ToString(), new Exception("No BMC.ExportFormat entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (exportFormatDTO.objectGuid == Guid.Empty)
            {
                exportFormatDTO.objectGuid = existing.objectGuid;
            }
            else if (exportFormatDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ExportFormat record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ExportFormat cloneOfExisting = (Database.ExportFormat)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ExportFormat object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ExportFormat exportFormat = (Database.ExportFormat)_context.Entry(existing).GetDatabaseValues().ToObject();
			exportFormat.ApplyDTO(exportFormatDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (exportFormat.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ExportFormat record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (exportFormat.name != null && exportFormat.name.Length > 100)
			{
				exportFormat.name = exportFormat.name.Substring(0, 100);
			}

			if (exportFormat.description != null && exportFormat.description.Length > 500)
			{
				exportFormat.description = exportFormat.description.Substring(0, 500);
			}

			if (exportFormat.fileExtension != null && exportFormat.fileExtension.Length > 50)
			{
				exportFormat.fileExtension = exportFormat.fileExtension.Substring(0, 50);
			}

			EntityEntry<Database.ExportFormat> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(exportFormat);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ExportFormat entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExportFormat.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExportFormat.CreateAnonymousWithFirstLevelSubObjects(exportFormat)),
					null);


				return Ok(Database.ExportFormat.CreateAnonymous(exportFormat));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ExportFormat entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExportFormat.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExportFormat.CreateAnonymousWithFirstLevelSubObjects(exportFormat)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ExportFormat record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExportFormat", Name = "ExportFormat")]
		public async Task<IActionResult> PostExportFormat([FromBody]Database.ExportFormat.ExportFormatDTO exportFormatDTO, CancellationToken cancellationToken = default)
		{
			if (exportFormatDTO == null)
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
			// Create a new ExportFormat object using the data from the DTO
			//
			Database.ExportFormat exportFormat = Database.ExportFormat.FromDTO(exportFormatDTO);

			try
			{
				if (exportFormat.name != null && exportFormat.name.Length > 100)
				{
					exportFormat.name = exportFormat.name.Substring(0, 100);
				}

				if (exportFormat.description != null && exportFormat.description.Length > 500)
				{
					exportFormat.description = exportFormat.description.Substring(0, 500);
				}

				if (exportFormat.fileExtension != null && exportFormat.fileExtension.Length > 50)
				{
					exportFormat.fileExtension = exportFormat.fileExtension.Substring(0, 50);
				}

				exportFormat.objectGuid = Guid.NewGuid();
				_context.ExportFormats.Add(exportFormat);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ExportFormat entity successfully created.",
					true,
					exportFormat.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ExportFormat.CreateAnonymousWithFirstLevelSubObjects(exportFormat)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ExportFormat entity creation failed.", false, exportFormat.id.ToString(), "", JsonSerializer.Serialize(exportFormat), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ExportFormat", exportFormat.id, exportFormat.name));

			return CreatedAtRoute("ExportFormat", new { id = exportFormat.id }, Database.ExportFormat.CreateAnonymousWithFirstLevelSubObjects(exportFormat));
		}



        /// <summary>
        /// 
        /// This deletes a ExportFormat record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExportFormat/{id}")]
		[Route("api/ExportFormat")]
		public async Task<IActionResult> DeleteExportFormat(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ExportFormat> query = (from x in _context.ExportFormats
				where
				(x.id == id)
				select x);


			Database.ExportFormat exportFormat = await query.FirstOrDefaultAsync(cancellationToken);

			if (exportFormat == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ExportFormat DELETE", id.ToString(), new Exception("No BMC.ExportFormat entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ExportFormat cloneOfExisting = (Database.ExportFormat)_context.Entry(exportFormat).GetDatabaseValues().ToObject();


			try
			{
				exportFormat.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ExportFormat entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExportFormat.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExportFormat.CreateAnonymousWithFirstLevelSubObjects(exportFormat)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ExportFormat entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExportFormat.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExportFormat.CreateAnonymousWithFirstLevelSubObjects(exportFormat)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ExportFormat records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ExportFormats/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string fileExtension = null,
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

			IQueryable<Database.ExportFormat> query = (from ef in _context.ExportFormats select ef);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(ef => ef.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ef => ef.description == description);
			}
			if (string.IsNullOrEmpty(fileExtension) == false)
			{
				query = query.Where(ef => ef.fileExtension == fileExtension);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ef => ef.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ef => ef.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ef => ef.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ef => ef.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ef => ef.deleted == false);
				}
			}
			else
			{
				query = query.Where(ef => ef.active == true);
				query = query.Where(ef => ef.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Export Format, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.fileExtension.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.fileExtension);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ExportFormat.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ExportFormat/CreateAuditEvent")]
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
