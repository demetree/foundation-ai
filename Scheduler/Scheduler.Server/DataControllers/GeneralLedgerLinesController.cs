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
using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the GeneralLedgerLine entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the GeneralLedgerLine entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class GeneralLedgerLinesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<GeneralLedgerLinesController> _logger;

		public GeneralLedgerLinesController(SchedulerContext context, ILogger<GeneralLedgerLinesController> logger) : base("Scheduler", "GeneralLedgerLine")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of GeneralLedgerLines filtered by the parameters provided.
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
		[Route("api/GeneralLedgerLines")]
		public async Task<IActionResult> GetGeneralLedgerLines(
			int? generalLedgerEntryId = null,
			int? financialCategoryId = null,
			decimal? debitAmount = null,
			decimal? creditAmount = null,
			string description = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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

			IQueryable<Database.GeneralLedgerLine> query = (from gll in _context.GeneralLedgerLines select gll);
			if (generalLedgerEntryId.HasValue == true)
			{
				query = query.Where(gll => gll.generalLedgerEntryId == generalLedgerEntryId.Value);
			}
			if (financialCategoryId.HasValue == true)
			{
				query = query.Where(gll => gll.financialCategoryId == financialCategoryId.Value);
			}
			if (debitAmount.HasValue == true)
			{
				query = query.Where(gll => gll.debitAmount == debitAmount.Value);
			}
			if (creditAmount.HasValue == true)
			{
				query = query.Where(gll => gll.creditAmount == creditAmount.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(gll => gll.description == description);
			}

			query = query.OrderBy(gll => gll.description);


			//
			// Add the any string contains parameter to span all the string fields on the General Ledger Line, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.financialCategory.name.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.description.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.code.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.externalAccountId.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.color.Contains(anyStringContains))
			       || (includeRelations == true && x.generalLedgerEntry.description.Contains(anyStringContains))
			       || (includeRelations == true && x.generalLedgerEntry.referenceNumber.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.financialCategory);
				query = query.Include(x => x.generalLedgerEntry);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.GeneralLedgerLine> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.GeneralLedgerLine generalLedgerLine in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(generalLedgerLine, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.GeneralLedgerLine Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.GeneralLedgerLine Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of GeneralLedgerLines filtered by the parameters provided.  Its query is similar to the GetGeneralLedgerLines method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GeneralLedgerLines/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? generalLedgerEntryId = null,
			int? financialCategoryId = null,
			decimal? debitAmount = null,
			decimal? creditAmount = null,
			string description = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.GeneralLedgerLine> query = (from gll in _context.GeneralLedgerLines select gll);
			if (generalLedgerEntryId.HasValue == true)
			{
				query = query.Where(gll => gll.generalLedgerEntryId == generalLedgerEntryId.Value);
			}
			if (financialCategoryId.HasValue == true)
			{
				query = query.Where(gll => gll.financialCategoryId == financialCategoryId.Value);
			}
			if (debitAmount.HasValue == true)
			{
				query = query.Where(gll => gll.debitAmount == debitAmount.Value);
			}
			if (creditAmount.HasValue == true)
			{
				query = query.Where(gll => gll.creditAmount == creditAmount.Value);
			}
			if (description != null)
			{
				query = query.Where(gll => gll.description == description);
			}

			//
			// Add the any string contains parameter to span all the string fields on the General Ledger Line, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.description.Contains(anyStringContains)
			       || x.financialCategory.name.Contains(anyStringContains)
			       || x.financialCategory.description.Contains(anyStringContains)
			       || x.financialCategory.code.Contains(anyStringContains)
			       || x.financialCategory.externalAccountId.Contains(anyStringContains)
			       || x.financialCategory.color.Contains(anyStringContains)
			       || x.generalLedgerEntry.description.Contains(anyStringContains)
			       || x.generalLedgerEntry.referenceNumber.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single GeneralLedgerLine by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GeneralLedgerLine/{id}")]
		public async Task<IActionResult> GetGeneralLedgerLine(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.GeneralLedgerLine> query = (from gll in _context.GeneralLedgerLines where
				(gll.id == id)
					select gll);

				if (includeRelations == true)
				{
					query = query.Include(x => x.financialCategory);
					query = query.Include(x => x.generalLedgerEntry);
					query = query.AsSplitQuery();
				}

				Database.GeneralLedgerLine materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.GeneralLedgerLine Entity was read with Admin privilege." : "Scheduler.GeneralLedgerLine Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "GeneralLedgerLine", materialized.id, materialized.description));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.GeneralLedgerLine entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.GeneralLedgerLine.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.GeneralLedgerLine.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing GeneralLedgerLine record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/GeneralLedgerLine/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutGeneralLedgerLine(int id, [FromBody]Database.GeneralLedgerLine.GeneralLedgerLineDTO generalLedgerLineDTO, CancellationToken cancellationToken = default)
		{
			if (generalLedgerLineDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != generalLedgerLineDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.GeneralLedgerLine> query = (from x in _context.GeneralLedgerLines
				where
				(x.id == id)
				select x);


			Database.GeneralLedgerLine existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.GeneralLedgerLine PUT", id.ToString(), new Exception("No Scheduler.GeneralLedgerLine entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.GeneralLedgerLine cloneOfExisting = (Database.GeneralLedgerLine)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new GeneralLedgerLine object using the data from the existing record, updated with what is in the DTO.
			//
			Database.GeneralLedgerLine generalLedgerLine = (Database.GeneralLedgerLine)_context.Entry(existing).GetDatabaseValues().ToObject();
			generalLedgerLine.ApplyDTO(generalLedgerLineDTO);


			if (generalLedgerLine.description != null && generalLedgerLine.description.Length > 500)
			{
				generalLedgerLine.description = generalLedgerLine.description.Substring(0, 500);
			}

			EntityEntry<Database.GeneralLedgerLine> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(generalLedgerLine);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.GeneralLedgerLine entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.GeneralLedgerLine.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GeneralLedgerLine.CreateAnonymousWithFirstLevelSubObjects(generalLedgerLine)),
					null);


				return Ok(Database.GeneralLedgerLine.CreateAnonymous(generalLedgerLine));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.GeneralLedgerLine entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.GeneralLedgerLine.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GeneralLedgerLine.CreateAnonymousWithFirstLevelSubObjects(generalLedgerLine)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new GeneralLedgerLine record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GeneralLedgerLine", Name = "GeneralLedgerLine")]
		public async Task<IActionResult> PostGeneralLedgerLine([FromBody]Database.GeneralLedgerLine.GeneralLedgerLineDTO generalLedgerLineDTO, CancellationToken cancellationToken = default)
		{
			if (generalLedgerLineDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new GeneralLedgerLine object using the data from the DTO
			//
			Database.GeneralLedgerLine generalLedgerLine = Database.GeneralLedgerLine.FromDTO(generalLedgerLineDTO);

			try
			{
				if (generalLedgerLine.description != null && generalLedgerLine.description.Length > 500)
				{
					generalLedgerLine.description = generalLedgerLine.description.Substring(0, 500);
				}

				_context.GeneralLedgerLines.Add(generalLedgerLine);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.GeneralLedgerLine entity successfully created.",
					true,
					generalLedgerLine.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.GeneralLedgerLine.CreateAnonymousWithFirstLevelSubObjects(generalLedgerLine)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.GeneralLedgerLine entity creation failed.", false, generalLedgerLine.id.ToString(), "", JsonSerializer.Serialize(generalLedgerLine), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "GeneralLedgerLine", generalLedgerLine.id, generalLedgerLine.description));

			return CreatedAtRoute("GeneralLedgerLine", new { id = generalLedgerLine.id }, Database.GeneralLedgerLine.CreateAnonymousWithFirstLevelSubObjects(generalLedgerLine));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a GeneralLedgerLine record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GeneralLedgerLine/{id}")]
		[Route("api/GeneralLedgerLine")]
		public async Task<IActionResult> DeleteGeneralLedgerLine(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.GeneralLedgerLine> query = (from x in _context.GeneralLedgerLines
				where
				(x.id == id)
				select x);


			Database.GeneralLedgerLine generalLedgerLine = await query.FirstOrDefaultAsync(cancellationToken);

			if (generalLedgerLine == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.GeneralLedgerLine DELETE", id.ToString(), new Exception("No Scheduler.GeneralLedgerLine entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.GeneralLedgerLine cloneOfExisting = (Database.GeneralLedgerLine)_context.Entry(generalLedgerLine).GetDatabaseValues().ToObject();


			try
			{
				_context.GeneralLedgerLines.Remove(generalLedgerLine);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.GeneralLedgerLine entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.GeneralLedgerLine.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GeneralLedgerLine.CreateAnonymousWithFirstLevelSubObjects(generalLedgerLine)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.GeneralLedgerLine entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.GeneralLedgerLine.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GeneralLedgerLine.CreateAnonymousWithFirstLevelSubObjects(generalLedgerLine)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of GeneralLedgerLine records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/GeneralLedgerLines/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? generalLedgerEntryId = null,
			int? financialCategoryId = null,
			decimal? debitAmount = null,
			decimal? creditAmount = null,
			string description = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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

			IQueryable<Database.GeneralLedgerLine> query = (from gll in _context.GeneralLedgerLines select gll);
			if (generalLedgerEntryId.HasValue == true)
			{
				query = query.Where(gll => gll.generalLedgerEntryId == generalLedgerEntryId.Value);
			}
			if (financialCategoryId.HasValue == true)
			{
				query = query.Where(gll => gll.financialCategoryId == financialCategoryId.Value);
			}
			if (debitAmount.HasValue == true)
			{
				query = query.Where(gll => gll.debitAmount == debitAmount.Value);
			}
			if (creditAmount.HasValue == true)
			{
				query = query.Where(gll => gll.creditAmount == creditAmount.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(gll => gll.description == description);
			}


			//
			// Add the any string contains parameter to span all the string fields on the General Ledger Line, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.description.Contains(anyStringContains)
			       || x.financialCategory.name.Contains(anyStringContains)
			       || x.financialCategory.description.Contains(anyStringContains)
			       || x.financialCategory.code.Contains(anyStringContains)
			       || x.financialCategory.externalAccountId.Contains(anyStringContains)
			       || x.financialCategory.color.Contains(anyStringContains)
			       || x.generalLedgerEntry.description.Contains(anyStringContains)
			       || x.generalLedgerEntry.referenceNumber.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.GeneralLedgerLine.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/GeneralLedgerLine/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
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
