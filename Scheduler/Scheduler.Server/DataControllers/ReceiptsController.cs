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
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Receipt entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Receipt entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ReceiptsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object receiptPutSyncRoot = new object();
		static object receiptDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ReceiptsController> _logger;

		public ReceiptsController(SchedulerContext context, ILogger<ReceiptsController> logger) : base("Scheduler", "Receipt")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Receipts filtered by the parameters provided.
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
		[Route("api/Receipts")]
		public async Task<IActionResult> GetReceipts(
			string receiptNumber = null,
			int? receiptTypeId = null,
			int? invoiceId = null,
			int? paymentTransactionId = null,
			int? financialTransactionId = null,
			int? clientId = null,
			int? contactId = null,
			int? currencyId = null,
			DateTime? receiptDate = null,
			decimal? amount = null,
			string paymentMethod = null,
			string description = null,
			string notes = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (receiptDate.HasValue == true && receiptDate.Value.Kind != DateTimeKind.Utc)
			{
				receiptDate = receiptDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Receipt> query = (from r in _context.Receipts select r);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(receiptNumber) == false)
			{
				query = query.Where(r => r.receiptNumber == receiptNumber);
			}
			if (receiptTypeId.HasValue == true)
			{
				query = query.Where(r => r.receiptTypeId == receiptTypeId.Value);
			}
			if (invoiceId.HasValue == true)
			{
				query = query.Where(r => r.invoiceId == invoiceId.Value);
			}
			if (paymentTransactionId.HasValue == true)
			{
				query = query.Where(r => r.paymentTransactionId == paymentTransactionId.Value);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(r => r.financialTransactionId == financialTransactionId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(r => r.clientId == clientId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(r => r.contactId == contactId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(r => r.currencyId == currencyId.Value);
			}
			if (receiptDate.HasValue == true)
			{
				query = query.Where(r => r.receiptDate == receiptDate.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(r => r.amount == amount.Value);
			}
			if (string.IsNullOrEmpty(paymentMethod) == false)
			{
				query = query.Where(r => r.paymentMethod == paymentMethod);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(r => r.description == description);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(r => r.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(r => r.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(r => r.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(r => r.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(r => r.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(r => r.deleted == false);
				}
			}
			else
			{
				query = query.Where(r => r.active == true);
				query = query.Where(r => r.deleted == false);
			}

			query = query.OrderBy(r => r.receiptNumber);


			//
			// Add the any string contains parameter to span all the string fields on the Receipt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.receiptNumber.Contains(anyStringContains)
			       || x.paymentMethod.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || (includeRelations == true && x.client.name.Contains(anyStringContains))
			       || (includeRelations == true && x.client.description.Contains(anyStringContains))
			       || (includeRelations == true && x.client.addressLine1.Contains(anyStringContains))
			       || (includeRelations == true && x.client.addressLine2.Contains(anyStringContains))
			       || (includeRelations == true && x.client.city.Contains(anyStringContains))
			       || (includeRelations == true && x.client.postalCode.Contains(anyStringContains))
			       || (includeRelations == true && x.client.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.client.email.Contains(anyStringContains))
			       || (includeRelations == true && x.client.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.client.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.client.color.Contains(anyStringContains))
			       || (includeRelations == true && x.client.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.client.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.client.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.firstName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.middleName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.lastName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.title.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.company.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.email.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.mobile.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.position.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.webSite.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.color.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.contact.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.name.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.description.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.code.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.color.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.contactRole.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.description.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.journalEntryType.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.referenceNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.financialTransaction.externalSystemName.Contains(anyStringContains))
			       || (includeRelations == true && x.invoice.invoiceNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.invoice.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentTransaction.status.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentTransaction.providerTransactionId.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentTransaction.providerResponse.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentTransaction.payerName.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentTransaction.payerEmail.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentTransaction.payerPhone.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentTransaction.receiptNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentTransaction.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.receiptType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.receiptType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.receiptType.color.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.client);
				query = query.Include(x => x.contact);
				query = query.Include(x => x.currency);
				query = query.Include(x => x.financialTransaction);
				query = query.Include(x => x.invoice);
				query = query.Include(x => x.paymentTransaction);
				query = query.Include(x => x.receiptType);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.Receipt> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Receipt receipt in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(receipt, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Receipt Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Receipt Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Receipts filtered by the parameters provided.  Its query is similar to the GetReceipts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Receipts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string receiptNumber = null,
			int? receiptTypeId = null,
			int? invoiceId = null,
			int? paymentTransactionId = null,
			int? financialTransactionId = null,
			int? clientId = null,
			int? contactId = null,
			int? currencyId = null,
			DateTime? receiptDate = null,
			decimal? amount = null,
			string paymentMethod = null,
			string description = null,
			string notes = null,
			int? versionNumber = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (receiptDate.HasValue == true && receiptDate.Value.Kind != DateTimeKind.Utc)
			{
				receiptDate = receiptDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Receipt> query = (from r in _context.Receipts select r);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (receiptNumber != null)
			{
				query = query.Where(r => r.receiptNumber == receiptNumber);
			}
			if (receiptTypeId.HasValue == true)
			{
				query = query.Where(r => r.receiptTypeId == receiptTypeId.Value);
			}
			if (invoiceId.HasValue == true)
			{
				query = query.Where(r => r.invoiceId == invoiceId.Value);
			}
			if (paymentTransactionId.HasValue == true)
			{
				query = query.Where(r => r.paymentTransactionId == paymentTransactionId.Value);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(r => r.financialTransactionId == financialTransactionId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(r => r.clientId == clientId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(r => r.contactId == contactId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(r => r.currencyId == currencyId.Value);
			}
			if (receiptDate.HasValue == true)
			{
				query = query.Where(r => r.receiptDate == receiptDate.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(r => r.amount == amount.Value);
			}
			if (paymentMethod != null)
			{
				query = query.Where(r => r.paymentMethod == paymentMethod);
			}
			if (description != null)
			{
				query = query.Where(r => r.description == description);
			}
			if (notes != null)
			{
				query = query.Where(r => r.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(r => r.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(r => r.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(r => r.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(r => r.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(r => r.deleted == false);
				}
			}
			else
			{
				query = query.Where(r => r.active == true);
				query = query.Where(r => r.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Receipt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.receiptNumber.Contains(anyStringContains)
			       || x.paymentMethod.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.client.name.Contains(anyStringContains)
			       || x.client.description.Contains(anyStringContains)
			       || x.client.addressLine1.Contains(anyStringContains)
			       || x.client.addressLine2.Contains(anyStringContains)
			       || x.client.city.Contains(anyStringContains)
			       || x.client.postalCode.Contains(anyStringContains)
			       || x.client.phone.Contains(anyStringContains)
			       || x.client.email.Contains(anyStringContains)
			       || x.client.notes.Contains(anyStringContains)
			       || x.client.externalId.Contains(anyStringContains)
			       || x.client.color.Contains(anyStringContains)
			       || x.client.attributes.Contains(anyStringContains)
			       || x.client.avatarFileName.Contains(anyStringContains)
			       || x.client.avatarMimeType.Contains(anyStringContains)
			       || x.contact.firstName.Contains(anyStringContains)
			       || x.contact.middleName.Contains(anyStringContains)
			       || x.contact.lastName.Contains(anyStringContains)
			       || x.contact.title.Contains(anyStringContains)
			       || x.contact.company.Contains(anyStringContains)
			       || x.contact.email.Contains(anyStringContains)
			       || x.contact.phone.Contains(anyStringContains)
			       || x.contact.mobile.Contains(anyStringContains)
			       || x.contact.position.Contains(anyStringContains)
			       || x.contact.webSite.Contains(anyStringContains)
			       || x.contact.notes.Contains(anyStringContains)
			       || x.contact.attributes.Contains(anyStringContains)
			       || x.contact.color.Contains(anyStringContains)
			       || x.contact.avatarFileName.Contains(anyStringContains)
			       || x.contact.avatarMimeType.Contains(anyStringContains)
			       || x.contact.externalId.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.financialTransaction.contactRole.Contains(anyStringContains)
			       || x.financialTransaction.description.Contains(anyStringContains)
			       || x.financialTransaction.journalEntryType.Contains(anyStringContains)
			       || x.financialTransaction.referenceNumber.Contains(anyStringContains)
			       || x.financialTransaction.notes.Contains(anyStringContains)
			       || x.financialTransaction.externalId.Contains(anyStringContains)
			       || x.financialTransaction.externalSystemName.Contains(anyStringContains)
			       || x.invoice.invoiceNumber.Contains(anyStringContains)
			       || x.invoice.notes.Contains(anyStringContains)
			       || x.paymentTransaction.status.Contains(anyStringContains)
			       || x.paymentTransaction.providerTransactionId.Contains(anyStringContains)
			       || x.paymentTransaction.providerResponse.Contains(anyStringContains)
			       || x.paymentTransaction.payerName.Contains(anyStringContains)
			       || x.paymentTransaction.payerEmail.Contains(anyStringContains)
			       || x.paymentTransaction.payerPhone.Contains(anyStringContains)
			       || x.paymentTransaction.receiptNumber.Contains(anyStringContains)
			       || x.paymentTransaction.notes.Contains(anyStringContains)
			       || x.receiptType.name.Contains(anyStringContains)
			       || x.receiptType.description.Contains(anyStringContains)
			       || x.receiptType.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Receipt by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Receipt/{id}")]
		public async Task<IActionResult> GetReceipt(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.Receipt> query = (from r in _context.Receipts where
							(r.id == id) &&
							(userIsAdmin == true || r.deleted == false) &&
							(userIsWriter == true || r.active == true)
					select r);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.client);
					query = query.Include(x => x.contact);
					query = query.Include(x => x.currency);
					query = query.Include(x => x.financialTransaction);
					query = query.Include(x => x.invoice);
					query = query.Include(x => x.paymentTransaction);
					query = query.Include(x => x.receiptType);
					query = query.AsSplitQuery();
				}

				Database.Receipt materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Receipt Entity was read with Admin privilege." : "Scheduler.Receipt Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Receipt", materialized.id, materialized.receiptNumber));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Receipt entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Receipt.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Receipt.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing Receipt record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Receipt/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutReceipt(int id, [FromBody]Database.Receipt.ReceiptDTO receiptDTO, CancellationToken cancellationToken = default)
		{
			if (receiptDTO == null)
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



			if (id != receiptDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.Receipt> query = (from x in _context.Receipts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Receipt existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Receipt PUT", id.ToString(), new Exception("No Scheduler.Receipt entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (receiptDTO.objectGuid == Guid.Empty)
            {
                receiptDTO.objectGuid = existing.objectGuid;
            }
            else if (receiptDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Receipt record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Receipt cloneOfExisting = (Database.Receipt)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Receipt object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Receipt receipt = (Database.Receipt)_context.Entry(existing).GetDatabaseValues().ToObject();
			receipt.ApplyDTO(receiptDTO);
			//
			// The tenant guid for any Receipt being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Receipt because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				receipt.tenantGuid = existing.tenantGuid;
			}

			lock (receiptPutSyncRoot)
			{
				//
				// Validate the version number for the receipt being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != receipt.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Receipt save attempt was made but save request was with version " + receipt.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Receipt you are trying to update has already changed.  Please try your save again after reloading the Receipt.");
				}
				else
				{
					// Same record.  Increase version.
					receipt.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (receipt.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Receipt record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (receipt.receiptNumber != null && receipt.receiptNumber.Length > 50)
				{
					receipt.receiptNumber = receipt.receiptNumber.Substring(0, 50);
				}

				if (receipt.receiptDate.Kind != DateTimeKind.Utc)
				{
					receipt.receiptDate = receipt.receiptDate.ToUniversalTime();
				}

				if (receipt.paymentMethod != null && receipt.paymentMethod.Length > 100)
				{
					receipt.paymentMethod = receipt.paymentMethod.Substring(0, 100);
				}

				if (receipt.description != null && receipt.description.Length > 500)
				{
					receipt.description = receipt.description.Substring(0, 500);
				}

				try
				{
				    EntityEntry<Database.Receipt> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(receipt);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ReceiptChangeHistory receiptChangeHistory = new ReceiptChangeHistory();
				        receiptChangeHistory.receiptId = receipt.id;
				        receiptChangeHistory.versionNumber = receipt.versionNumber;
				        receiptChangeHistory.timeStamp = DateTime.UtcNow;
				        receiptChangeHistory.userId = securityUser.id;
				        receiptChangeHistory.tenantGuid = userTenantGuid;
				        receiptChangeHistory.data = JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt));
				        _context.ReceiptChangeHistories.Add(receiptChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Receipt entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt)),
						null);

				return Ok(Database.Receipt.CreateAnonymous(receipt));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Receipt entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt)),
						ex);

					return Problem(ex.Message);
				}

			}
		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new Receipt record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Receipt", Name = "Receipt")]
		public async Task<IActionResult> PostReceipt([FromBody]Database.Receipt.ReceiptDTO receiptDTO, CancellationToken cancellationToken = default)
		{
			if (receiptDTO == null)
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
			// Create a new Receipt object using the data from the DTO
			//
			Database.Receipt receipt = Database.Receipt.FromDTO(receiptDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				receipt.tenantGuid = userTenantGuid;

				if (receipt.receiptNumber != null && receipt.receiptNumber.Length > 50)
				{
					receipt.receiptNumber = receipt.receiptNumber.Substring(0, 50);
				}

				if (receipt.receiptDate.Kind != DateTimeKind.Utc)
				{
					receipt.receiptDate = receipt.receiptDate.ToUniversalTime();
				}

				if (receipt.paymentMethod != null && receipt.paymentMethod.Length > 100)
				{
					receipt.paymentMethod = receipt.paymentMethod.Substring(0, 100);
				}

				if (receipt.description != null && receipt.description.Length > 500)
				{
					receipt.description = receipt.description.Substring(0, 500);
				}

				receipt.objectGuid = Guid.NewGuid();
				receipt.versionNumber = 1;

				_context.Receipts.Add(receipt);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the receipt object so that no further changes will be written to the database
				    //
				    _context.Entry(receipt).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					receipt.Documents = null;
					receipt.ReceiptChangeHistories = null;
					receipt.client = null;
					receipt.contact = null;
					receipt.currency = null;
					receipt.financialTransaction = null;
					receipt.invoice = null;
					receipt.paymentTransaction = null;
					receipt.receiptType = null;


				    ReceiptChangeHistory receiptChangeHistory = new ReceiptChangeHistory();
				    receiptChangeHistory.receiptId = receipt.id;
				    receiptChangeHistory.versionNumber = receipt.versionNumber;
				    receiptChangeHistory.timeStamp = DateTime.UtcNow;
				    receiptChangeHistory.userId = securityUser.id;
				    receiptChangeHistory.tenantGuid = userTenantGuid;
				    receiptChangeHistory.data = JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt));
				    _context.ReceiptChangeHistories.Add(receiptChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Receipt entity successfully created.",
						true,
						receipt. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Receipt entity creation failed.", false, receipt.id.ToString(), "", JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Receipt", receipt.id, receipt.receiptNumber));

			return CreatedAtRoute("Receipt", new { id = receipt.id }, Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt));
		}

*/

/* This function is expected to be overridden in a custom file

        /// <summary>
        /// 
        /// This rolls a Receipt entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Receipt/Rollback/{id}")]
		[Route("api/Receipt/Rollback")]
		public async Task<IActionResult> RollbackToReceiptVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Receipt> query = (from x in _context.Receipts
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Receipt concurrently
			//
			lock (receiptPutSyncRoot)
			{
				
				Database.Receipt receipt = query.FirstOrDefault();
				
				if (receipt == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Receipt rollback", id.ToString(), new Exception("No Scheduler.Receipt entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Receipt current state so we can log it.
				//
				Database.Receipt cloneOfExisting = (Database.Receipt)_context.Entry(receipt).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Documents = null;
				cloneOfExisting.ReceiptChangeHistories = null;
				cloneOfExisting.client = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.currency = null;
				cloneOfExisting.financialTransaction = null;
				cloneOfExisting.invoice = null;
				cloneOfExisting.paymentTransaction = null;
				cloneOfExisting.receiptType = null;

				if (versionNumber >= receipt.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Receipt rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Receipt rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ReceiptChangeHistory receiptChangeHistory = (from x in _context.ReceiptChangeHistories
				                                               where
				                                               x.receiptId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (receiptChangeHistory != null)
				{
				    Database.Receipt oldReceipt = JsonSerializer.Deserialize<Database.Receipt>(receiptChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    receipt.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    receipt.receiptNumber = oldReceipt.receiptNumber;
				    receipt.receiptTypeId = oldReceipt.receiptTypeId;
				    receipt.invoiceId = oldReceipt.invoiceId;
				    receipt.paymentTransactionId = oldReceipt.paymentTransactionId;
				    receipt.financialTransactionId = oldReceipt.financialTransactionId;
				    receipt.clientId = oldReceipt.clientId;
				    receipt.contactId = oldReceipt.contactId;
				    receipt.currencyId = oldReceipt.currencyId;
				    receipt.receiptDate = oldReceipt.receiptDate;
				    receipt.amount = oldReceipt.amount;
				    receipt.paymentMethod = oldReceipt.paymentMethod;
				    receipt.description = oldReceipt.description;
				    receipt.notes = oldReceipt.notes;
				    receipt.objectGuid = oldReceipt.objectGuid;
				    receipt.active = oldReceipt.active;
				    receipt.deleted = oldReceipt.deleted;

				    string serializedReceipt = JsonSerializer.Serialize(receipt);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ReceiptChangeHistory newReceiptChangeHistory = new ReceiptChangeHistory();
				        newReceiptChangeHistory.receiptId = receipt.id;
				        newReceiptChangeHistory.versionNumber = receipt.versionNumber;
				        newReceiptChangeHistory.timeStamp = DateTime.UtcNow;
				        newReceiptChangeHistory.userId = securityUser.id;
				        newReceiptChangeHistory.tenantGuid = userTenantGuid;
				        newReceiptChangeHistory.data = JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt));
				        _context.ReceiptChangeHistories.Add(newReceiptChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Receipt rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt)),
						null);


				    return Ok(Database.Receipt.CreateAnonymous(receipt));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Receipt rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Receipt rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}

*/



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Receipt.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Receipt</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Receipt/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetReceiptChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Receipt receipt = await _context.Receipts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (receipt == null)
			{
				return NotFound();
			}

			try
			{
				receipt.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Receipt> versionInfo = await receipt.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Receipt.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Receipt</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Receipt/{id}/AuditHistory")]
		public async Task<IActionResult> GetReceiptAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Receipt receipt = await _context.Receipts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (receipt == null)
			{
				return NotFound();
			}

			try
			{
				receipt.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Receipt>> versions = await receipt.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Receipt.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Receipt</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Receipt object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Receipt/{id}/Version/{version}")]
		public async Task<IActionResult> GetReceiptVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Receipt receipt = await _context.Receipts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (receipt == null)
			{
				return NotFound();
			}

			try
			{
				receipt.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Receipt> versionInfo = await receipt.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Receipt at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Receipt</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Receipt object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Receipt/{id}/StateAtTime")]
		public async Task<IActionResult> GetReceiptStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.Receipt receipt = await _context.Receipts.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (receipt == null)
			{
				return NotFound();
			}

			try
			{
				receipt.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Receipt> versionInfo = await receipt.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a Receipt record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Receipt/{id}")]
		[Route("api/Receipt")]
		public async Task<IActionResult> DeleteReceipt(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Receipt> query = (from x in _context.Receipts
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Receipt receipt = await query.FirstOrDefaultAsync(cancellationToken);

			if (receipt == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Receipt DELETE", id.ToString(), new Exception("No Scheduler.Receipt entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Receipt cloneOfExisting = (Database.Receipt)_context.Entry(receipt).GetDatabaseValues().ToObject();


			lock (receiptDeleteSyncRoot)
			{
			    try
			    {
			        receipt.deleted = true;
			        receipt.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ReceiptChangeHistory receiptChangeHistory = new ReceiptChangeHistory();
			        receiptChangeHistory.receiptId = receipt.id;
			        receiptChangeHistory.versionNumber = receipt.versionNumber;
			        receiptChangeHistory.timeStamp = DateTime.UtcNow;
			        receiptChangeHistory.userId = securityUser.id;
			        receiptChangeHistory.tenantGuid = userTenantGuid;
			        receiptChangeHistory.data = JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt));
			        _context.ReceiptChangeHistories.Add(receiptChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Receipt entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Receipt entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


*/
        /// <summary>
        /// 
        /// This gets a list of Receipt records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Receipts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string receiptNumber = null,
			int? receiptTypeId = null,
			int? invoiceId = null,
			int? paymentTransactionId = null,
			int? financialTransactionId = null,
			int? clientId = null,
			int? contactId = null,
			int? currencyId = null,
			DateTime? receiptDate = null,
			decimal? amount = null,
			string paymentMethod = null,
			string description = null,
			string notes = null,
			int? versionNumber = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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
			if (receiptDate.HasValue == true && receiptDate.Value.Kind != DateTimeKind.Utc)
			{
				receiptDate = receiptDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Receipt> query = (from r in _context.Receipts select r);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(receiptNumber) == false)
			{
				query = query.Where(r => r.receiptNumber == receiptNumber);
			}
			if (receiptTypeId.HasValue == true)
			{
				query = query.Where(r => r.receiptTypeId == receiptTypeId.Value);
			}
			if (invoiceId.HasValue == true)
			{
				query = query.Where(r => r.invoiceId == invoiceId.Value);
			}
			if (paymentTransactionId.HasValue == true)
			{
				query = query.Where(r => r.paymentTransactionId == paymentTransactionId.Value);
			}
			if (financialTransactionId.HasValue == true)
			{
				query = query.Where(r => r.financialTransactionId == financialTransactionId.Value);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(r => r.clientId == clientId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(r => r.contactId == contactId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(r => r.currencyId == currencyId.Value);
			}
			if (receiptDate.HasValue == true)
			{
				query = query.Where(r => r.receiptDate == receiptDate.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(r => r.amount == amount.Value);
			}
			if (string.IsNullOrEmpty(paymentMethod) == false)
			{
				query = query.Where(r => r.paymentMethod == paymentMethod);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(r => r.description == description);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(r => r.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(r => r.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(r => r.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(r => r.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(r => r.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(r => r.deleted == false);
				}
			}
			else
			{
				query = query.Where(r => r.active == true);
				query = query.Where(r => r.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Receipt, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.receiptNumber.Contains(anyStringContains)
			       || x.paymentMethod.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.client.name.Contains(anyStringContains)
			       || x.client.description.Contains(anyStringContains)
			       || x.client.addressLine1.Contains(anyStringContains)
			       || x.client.addressLine2.Contains(anyStringContains)
			       || x.client.city.Contains(anyStringContains)
			       || x.client.postalCode.Contains(anyStringContains)
			       || x.client.phone.Contains(anyStringContains)
			       || x.client.email.Contains(anyStringContains)
			       || x.client.notes.Contains(anyStringContains)
			       || x.client.externalId.Contains(anyStringContains)
			       || x.client.color.Contains(anyStringContains)
			       || x.client.attributes.Contains(anyStringContains)
			       || x.client.avatarFileName.Contains(anyStringContains)
			       || x.client.avatarMimeType.Contains(anyStringContains)
			       || x.contact.firstName.Contains(anyStringContains)
			       || x.contact.middleName.Contains(anyStringContains)
			       || x.contact.lastName.Contains(anyStringContains)
			       || x.contact.title.Contains(anyStringContains)
			       || x.contact.company.Contains(anyStringContains)
			       || x.contact.email.Contains(anyStringContains)
			       || x.contact.phone.Contains(anyStringContains)
			       || x.contact.mobile.Contains(anyStringContains)
			       || x.contact.position.Contains(anyStringContains)
			       || x.contact.webSite.Contains(anyStringContains)
			       || x.contact.notes.Contains(anyStringContains)
			       || x.contact.attributes.Contains(anyStringContains)
			       || x.contact.color.Contains(anyStringContains)
			       || x.contact.avatarFileName.Contains(anyStringContains)
			       || x.contact.avatarMimeType.Contains(anyStringContains)
			       || x.contact.externalId.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
			       || x.financialTransaction.contactRole.Contains(anyStringContains)
			       || x.financialTransaction.description.Contains(anyStringContains)
			       || x.financialTransaction.journalEntryType.Contains(anyStringContains)
			       || x.financialTransaction.referenceNumber.Contains(anyStringContains)
			       || x.financialTransaction.notes.Contains(anyStringContains)
			       || x.financialTransaction.externalId.Contains(anyStringContains)
			       || x.financialTransaction.externalSystemName.Contains(anyStringContains)
			       || x.invoice.invoiceNumber.Contains(anyStringContains)
			       || x.invoice.notes.Contains(anyStringContains)
			       || x.paymentTransaction.status.Contains(anyStringContains)
			       || x.paymentTransaction.providerTransactionId.Contains(anyStringContains)
			       || x.paymentTransaction.providerResponse.Contains(anyStringContains)
			       || x.paymentTransaction.payerName.Contains(anyStringContains)
			       || x.paymentTransaction.payerEmail.Contains(anyStringContains)
			       || x.paymentTransaction.payerPhone.Contains(anyStringContains)
			       || x.paymentTransaction.receiptNumber.Contains(anyStringContains)
			       || x.paymentTransaction.notes.Contains(anyStringContains)
			       || x.receiptType.name.Contains(anyStringContains)
			       || x.receiptType.description.Contains(anyStringContains)
			       || x.receiptType.color.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.receiptNumber);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Receipt.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Receipt/CreateAuditEvent")]
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
