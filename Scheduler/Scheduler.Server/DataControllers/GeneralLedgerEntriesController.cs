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
    /// This auto generated class provides the basic CRUD operations for the GeneralLedgerEntry entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the GeneralLedgerEntry entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class GeneralLedgerEntriesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<GeneralLedgerEntriesController> _logger;

		public GeneralLedgerEntriesController(SchedulerContext context, ILogger<GeneralLedgerEntriesController> logger) : base("Scheduler", "GeneralLedgerEntry")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of GeneralLedgerEntries filtered by the parameters provided.
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
		[Route("api/GeneralLedgerEntries")]
		public async Task<IActionResult> GetGeneralLedgerEntries(
			int? journalEntryNumber = null,
			DateTime? transactionDate = null,
			string description = null,
			string referenceNumber = null,
			int? financialTransactionId = null,
			int? fiscalPeriodId = null,
			int? financialOfficeId = null,
			int? postedBy = null,
			DateTime? postedDate = null,
			int? reversalOfId = null,
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
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (transactionDate.HasValue == true && transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				transactionDate = transactionDate.Value.ToUniversalTime();
			}

			if (postedDate.HasValue == true && postedDate.Value.Kind != DateTimeKind.Utc)
			{
				postedDate = postedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.GeneralLedgerEntry> query = (from gle in _context.GeneralLedgerEntries select gle);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (journalEntryNumber.HasValue == true)
			{
				query = query.Where(gle => gle.journalEntryNumber == journalEntryNumber.Value);
			}
			if (transactionDate.HasValue == true)
			{
				query = query.Where(gle => gle.transactionDate == transactionDate.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(gle => gle.description == description);
			}
			if (string.IsNullOrEmpty(referenceNumber) == false)
			{
				query = query.Where(gle => gle.referenceNumber == referenceNumber);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(gle => gle.financialTransactionId == financialTransactionId.Value);
			}
			if (fiscalPeriodId.HasValue == true)
			{
				query = query.Where(gle => gle.fiscalPeriodId == fiscalPeriodId.Value);
			}
			if (financialOfficeId.HasValue == true)
			{
				query = query.Where(gle => gle.financialOfficeId == financialOfficeId.Value);
			}
			if (postedBy.HasValue == true)
			{
				query = query.Where(gle => gle.postedBy == postedBy.Value);
			}
			if (postedDate.HasValue == true)
			{
				query = query.Where(gle => gle.postedDate == postedDate.Value);
			}
			if (reversalOfId.HasValue == true)
			{
				query = query.Where(gle => gle.reversalOfId == reversalOfId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(gle => gle.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(gle => gle.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(gle => gle.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(gle => gle.deleted == false);
				}
			}
			else
			{
				query = query.Where(gle => gle.active == true);
				query = query.Where(gle => gle.deleted == false);
			}

			query = query.OrderBy(gle => gle.description).ThenBy(gle => gle.referenceNumber);


			//
			// Add the any string contains parameter to span all the string fields on the General Ledger Entry, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.description.Contains(anyStringContains)
			       || x.referenceNumber.Contains(anyStringContains)
			       || (includeRelations == true && x.financialOffice.name.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.description.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.code.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.contactName.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.contactEmail.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.exportFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.color.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.contactRole.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.description.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.journalEntryType.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.referenceNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.externalSystemName.Contains(anyStringContains))
			       || (includeRelations == true && x.fiscalPeriod.name.Contains(anyStringContains))
			       || (includeRelations == true && x.fiscalPeriod.description.Contains(anyStringContains))
			       || (includeRelations == true && x.fiscalPeriod.periodType.Contains(anyStringContains))
			       || (includeRelations == true && x.fiscalPeriod.closedBy.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.financialOffice);
				query = query.Include(x => x.financialTransaction);
				query = query.Include(x => x.fiscalPeriod);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.GeneralLedgerEntry> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.GeneralLedgerEntry generalLedgerEntry in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(generalLedgerEntry, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.GeneralLedgerEntry Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.GeneralLedgerEntry Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of GeneralLedgerEntries filtered by the parameters provided.  Its query is similar to the GetGeneralLedgerEntries method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GeneralLedgerEntries/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? journalEntryNumber = null,
			DateTime? transactionDate = null,
			string description = null,
			string referenceNumber = null,
			int? financialTransactionId = null,
			int? fiscalPeriodId = null,
			int? financialOfficeId = null,
			int? postedBy = null,
			DateTime? postedDate = null,
			int? reversalOfId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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
			// Fix any non-UTC date parameters that come in.
			//
			if (transactionDate.HasValue == true && transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				transactionDate = transactionDate.Value.ToUniversalTime();
			}

			if (postedDate.HasValue == true && postedDate.Value.Kind != DateTimeKind.Utc)
			{
				postedDate = postedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.GeneralLedgerEntry> query = (from gle in _context.GeneralLedgerEntries select gle);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (journalEntryNumber.HasValue == true)
			{
				query = query.Where(gle => gle.journalEntryNumber == journalEntryNumber.Value);
			}
			if (transactionDate.HasValue == true)
			{
				query = query.Where(gle => gle.transactionDate == transactionDate.Value);
			}
			if (description != null)
			{
				query = query.Where(gle => gle.description == description);
			}
			if (referenceNumber != null)
			{
				query = query.Where(gle => gle.referenceNumber == referenceNumber);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(gle => gle.financialTransactionId == financialTransactionId.Value);
			}
			if (fiscalPeriodId.HasValue == true)
			{
				query = query.Where(gle => gle.fiscalPeriodId == fiscalPeriodId.Value);
			}
			if (financialOfficeId.HasValue == true)
			{
				query = query.Where(gle => gle.financialOfficeId == financialOfficeId.Value);
			}
			if (postedBy.HasValue == true)
			{
				query = query.Where(gle => gle.postedBy == postedBy.Value);
			}
			if (postedDate.HasValue == true)
			{
				query = query.Where(gle => gle.postedDate == postedDate.Value);
			}
			if (reversalOfId.HasValue == true)
			{
				query = query.Where(gle => gle.reversalOfId == reversalOfId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(gle => gle.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(gle => gle.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(gle => gle.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(gle => gle.deleted == false);
				}
			}
			else
			{
				query = query.Where(gle => gle.active == true);
				query = query.Where(gle => gle.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the General Ledger Entry, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.description.Contains(anyStringContains)
			       || x.referenceNumber.Contains(anyStringContains)
			       || x.financialOffice.name.Contains(anyStringContains)
			       || x.financialOffice.description.Contains(anyStringContains)
			       || x.financialOffice.code.Contains(anyStringContains)
			       || x.financialOffice.contactName.Contains(anyStringContains)
			       || x.financialOffice.contactEmail.Contains(anyStringContains)
			       || x.financialOffice.exportFormat.Contains(anyStringContains)
			       || x.financialOffice.color.Contains(anyStringContains)
			       || x.financialTransaction.contactRole.Contains(anyStringContains)
			       || x.financialTransaction.description.Contains(anyStringContains)
			       || x.financialTransaction.journalEntryType.Contains(anyStringContains)
			       || x.financialTransaction.referenceNumber.Contains(anyStringContains)
			       || x.financialTransaction.notes.Contains(anyStringContains)
			       || x.financialTransaction.externalId.Contains(anyStringContains)
			       || x.financialTransaction.externalSystemName.Contains(anyStringContains)
			       || x.fiscalPeriod.name.Contains(anyStringContains)
			       || x.fiscalPeriod.description.Contains(anyStringContains)
			       || x.fiscalPeriod.periodType.Contains(anyStringContains)
			       || x.fiscalPeriod.closedBy.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single GeneralLedgerEntry by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GeneralLedgerEntry/{id}")]
		public async Task<IActionResult> GetGeneralLedgerEntry(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.GeneralLedgerEntry> query = (from gle in _context.GeneralLedgerEntries where
							(gle.id == id) &&
							(userIsAdmin == true || gle.deleted == false) &&
							(userIsWriter == true || gle.active == true)
					select gle);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.financialOffice);
					query = query.Include(x => x.financialTransaction);
					query = query.Include(x => x.fiscalPeriod);
					query = query.AsSplitQuery();
				}

				Database.GeneralLedgerEntry materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.GeneralLedgerEntry Entity was read with Admin privilege." : "Scheduler.GeneralLedgerEntry Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "GeneralLedgerEntry", materialized.id, materialized.description));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.GeneralLedgerEntry entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.GeneralLedgerEntry.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.GeneralLedgerEntry.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing GeneralLedgerEntry record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/GeneralLedgerEntry/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutGeneralLedgerEntry(int id, [FromBody]Database.GeneralLedgerEntry.GeneralLedgerEntryDTO generalLedgerEntryDTO, CancellationToken cancellationToken = default)
		{
			if (generalLedgerEntryDTO == null)
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



			if (id != generalLedgerEntryDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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


			IQueryable<Database.GeneralLedgerEntry> query = (from x in _context.GeneralLedgerEntries
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.GeneralLedgerEntry existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.GeneralLedgerEntry PUT", id.ToString(), new Exception("No Scheduler.GeneralLedgerEntry entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (generalLedgerEntryDTO.objectGuid == Guid.Empty)
            {
                generalLedgerEntryDTO.objectGuid = existing.objectGuid;
            }
            else if (generalLedgerEntryDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a GeneralLedgerEntry record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.GeneralLedgerEntry cloneOfExisting = (Database.GeneralLedgerEntry)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new GeneralLedgerEntry object using the data from the existing record, updated with what is in the DTO.
			//
			Database.GeneralLedgerEntry generalLedgerEntry = (Database.GeneralLedgerEntry)_context.Entry(existing).GetDatabaseValues().ToObject();
			generalLedgerEntry.ApplyDTO(generalLedgerEntryDTO);
			//
			// The tenant guid for any GeneralLedgerEntry being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the GeneralLedgerEntry because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				generalLedgerEntry.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (generalLedgerEntry.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.GeneralLedgerEntry record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (generalLedgerEntry.transactionDate.Kind != DateTimeKind.Utc)
			{
				generalLedgerEntry.transactionDate = generalLedgerEntry.transactionDate.ToUniversalTime();
			}

			if (generalLedgerEntry.description != null && generalLedgerEntry.description.Length > 500)
			{
				generalLedgerEntry.description = generalLedgerEntry.description.Substring(0, 500);
			}

			if (generalLedgerEntry.referenceNumber != null && generalLedgerEntry.referenceNumber.Length > 100)
			{
				generalLedgerEntry.referenceNumber = generalLedgerEntry.referenceNumber.Substring(0, 100);
			}

			if (generalLedgerEntry.postedDate.Kind != DateTimeKind.Utc)
			{
				generalLedgerEntry.postedDate = generalLedgerEntry.postedDate.ToUniversalTime();
			}

			EntityEntry<Database.GeneralLedgerEntry> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(generalLedgerEntry);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.GeneralLedgerEntry entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.GeneralLedgerEntry.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GeneralLedgerEntry.CreateAnonymousWithFirstLevelSubObjects(generalLedgerEntry)),
					null);


				return Ok(Database.GeneralLedgerEntry.CreateAnonymous(generalLedgerEntry));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.GeneralLedgerEntry entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.GeneralLedgerEntry.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GeneralLedgerEntry.CreateAnonymousWithFirstLevelSubObjects(generalLedgerEntry)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new GeneralLedgerEntry record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GeneralLedgerEntry", Name = "GeneralLedgerEntry")]
		public async Task<IActionResult> PostGeneralLedgerEntry([FromBody]Database.GeneralLedgerEntry.GeneralLedgerEntryDTO generalLedgerEntryDTO, CancellationToken cancellationToken = default)
		{
			if (generalLedgerEntryDTO == null)
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
			// Create a new GeneralLedgerEntry object using the data from the DTO
			//
			Database.GeneralLedgerEntry generalLedgerEntry = Database.GeneralLedgerEntry.FromDTO(generalLedgerEntryDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				generalLedgerEntry.tenantGuid = userTenantGuid;

				if (generalLedgerEntry.transactionDate.Kind != DateTimeKind.Utc)
				{
					generalLedgerEntry.transactionDate = generalLedgerEntry.transactionDate.ToUniversalTime();
				}

				if (generalLedgerEntry.description != null && generalLedgerEntry.description.Length > 500)
				{
					generalLedgerEntry.description = generalLedgerEntry.description.Substring(0, 500);
				}

				if (generalLedgerEntry.referenceNumber != null && generalLedgerEntry.referenceNumber.Length > 100)
				{
					generalLedgerEntry.referenceNumber = generalLedgerEntry.referenceNumber.Substring(0, 100);
				}

				if (generalLedgerEntry.postedDate.Kind != DateTimeKind.Utc)
				{
					generalLedgerEntry.postedDate = generalLedgerEntry.postedDate.ToUniversalTime();
				}

				generalLedgerEntry.objectGuid = Guid.NewGuid();
				_context.GeneralLedgerEntries.Add(generalLedgerEntry);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.GeneralLedgerEntry entity successfully created.",
					true,
					generalLedgerEntry.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.GeneralLedgerEntry.CreateAnonymousWithFirstLevelSubObjects(generalLedgerEntry)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.GeneralLedgerEntry entity creation failed.", false, generalLedgerEntry.id.ToString(), "", JsonSerializer.Serialize(generalLedgerEntry), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "GeneralLedgerEntry", generalLedgerEntry.id, generalLedgerEntry.description));

			return CreatedAtRoute("GeneralLedgerEntry", new { id = generalLedgerEntry.id }, Database.GeneralLedgerEntry.CreateAnonymousWithFirstLevelSubObjects(generalLedgerEntry));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a GeneralLedgerEntry record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/GeneralLedgerEntry/{id}")]
		[Route("api/GeneralLedgerEntry")]
		public async Task<IActionResult> DeleteGeneralLedgerEntry(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.GeneralLedgerEntry> query = (from x in _context.GeneralLedgerEntries
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.GeneralLedgerEntry generalLedgerEntry = await query.FirstOrDefaultAsync(cancellationToken);

			if (generalLedgerEntry == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.GeneralLedgerEntry DELETE", id.ToString(), new Exception("No Scheduler.GeneralLedgerEntry entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.GeneralLedgerEntry cloneOfExisting = (Database.GeneralLedgerEntry)_context.Entry(generalLedgerEntry).GetDatabaseValues().ToObject();


			try
			{
				generalLedgerEntry.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.GeneralLedgerEntry entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.GeneralLedgerEntry.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GeneralLedgerEntry.CreateAnonymousWithFirstLevelSubObjects(generalLedgerEntry)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.GeneralLedgerEntry entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.GeneralLedgerEntry.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.GeneralLedgerEntry.CreateAnonymousWithFirstLevelSubObjects(generalLedgerEntry)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of GeneralLedgerEntry records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/GeneralLedgerEntries/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? journalEntryNumber = null,
			DateTime? transactionDate = null,
			string description = null,
			string referenceNumber = null,
			int? financialTransactionId = null,
			int? fiscalPeriodId = null,
			int? financialOfficeId = null,
			int? postedBy = null,
			DateTime? postedDate = null,
			int? reversalOfId = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (transactionDate.HasValue == true && transactionDate.Value.Kind != DateTimeKind.Utc)
			{
				transactionDate = transactionDate.Value.ToUniversalTime();
			}

			if (postedDate.HasValue == true && postedDate.Value.Kind != DateTimeKind.Utc)
			{
				postedDate = postedDate.Value.ToUniversalTime();
			}

			IQueryable<Database.GeneralLedgerEntry> query = (from gle in _context.GeneralLedgerEntries select gle);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (journalEntryNumber.HasValue == true)
			{
				query = query.Where(gle => gle.journalEntryNumber == journalEntryNumber.Value);
			}
			if (transactionDate.HasValue == true)
			{
				query = query.Where(gle => gle.transactionDate == transactionDate.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(gle => gle.description == description);
			}
			if (string.IsNullOrEmpty(referenceNumber) == false)
			{
				query = query.Where(gle => gle.referenceNumber == referenceNumber);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(gle => gle.financialTransactionId == financialTransactionId.Value);
			}
			if (fiscalPeriodId.HasValue == true)
			{
				query = query.Where(gle => gle.fiscalPeriodId == fiscalPeriodId.Value);
			}
			if (financialOfficeId.HasValue == true)
			{
				query = query.Where(gle => gle.financialOfficeId == financialOfficeId.Value);
			}
			if (postedBy.HasValue == true)
			{
				query = query.Where(gle => gle.postedBy == postedBy.Value);
			}
			if (postedDate.HasValue == true)
			{
				query = query.Where(gle => gle.postedDate == postedDate.Value);
			}
			if (reversalOfId.HasValue == true)
			{
				query = query.Where(gle => gle.reversalOfId == reversalOfId.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(gle => gle.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(gle => gle.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(gle => gle.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(gle => gle.deleted == false);
				}
			}
			else
			{
				query = query.Where(gle => gle.active == true);
				query = query.Where(gle => gle.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the General Ledger Entry, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.description.Contains(anyStringContains)
			       || x.referenceNumber.Contains(anyStringContains)
			       || x.financialOffice.name.Contains(anyStringContains)
			       || x.financialOffice.description.Contains(anyStringContains)
			       || x.financialOffice.code.Contains(anyStringContains)
			       || x.financialOffice.contactName.Contains(anyStringContains)
			       || x.financialOffice.contactEmail.Contains(anyStringContains)
			       || x.financialOffice.exportFormat.Contains(anyStringContains)
			       || x.financialOffice.color.Contains(anyStringContains)
			       || x.financialTransaction.contactRole.Contains(anyStringContains)
			       || x.financialTransaction.description.Contains(anyStringContains)
			       || x.financialTransaction.journalEntryType.Contains(anyStringContains)
			       || x.financialTransaction.referenceNumber.Contains(anyStringContains)
			       || x.financialTransaction.notes.Contains(anyStringContains)
			       || x.financialTransaction.externalId.Contains(anyStringContains)
			       || x.financialTransaction.externalSystemName.Contains(anyStringContains)
			       || x.fiscalPeriod.name.Contains(anyStringContains)
			       || x.fiscalPeriod.description.Contains(anyStringContains)
			       || x.fiscalPeriod.periodType.Contains(anyStringContains)
			       || x.fiscalPeriod.closedBy.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.description).ThenBy(x => x.referenceNumber);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.GeneralLedgerEntry.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/GeneralLedgerEntry/CreateAuditEvent")]
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
