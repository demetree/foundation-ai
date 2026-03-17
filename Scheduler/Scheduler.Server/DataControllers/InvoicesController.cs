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
    /// This auto generated class provides the basic CRUD operations for the Invoice entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Invoice entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class InvoicesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object invoicePutSyncRoot = new object();
		static object invoiceDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<InvoicesController> _logger;

		public InvoicesController(SchedulerContext context, ILogger<InvoicesController> logger) : base("Scheduler", "Invoice")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Invoices filtered by the parameters provided.
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
		[Route("api/Invoices")]
		public async Task<IActionResult> GetInvoices(
			string invoiceNumber = null,
			int? clientId = null,
			int? contactId = null,
			int? scheduledEventId = null,
			int? financialOfficeId = null,
			int? invoiceStatusId = null,
			int? currencyId = null,
			int? taxCodeId = null,
			DateTime? invoiceDate = null,
			DateTime? dueDate = null,
			decimal? subtotal = null,
			decimal? taxAmount = null,
			decimal? totalAmount = null,
			decimal? amountPaid = null,
			DateTime? sentDate = null,
			DateTime? paidDate = null,
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
			if (invoiceDate.HasValue == true && invoiceDate.Value.Kind != DateTimeKind.Utc)
			{
				invoiceDate = invoiceDate.Value.ToUniversalTime();
			}

			if (dueDate.HasValue == true && dueDate.Value.Kind != DateTimeKind.Utc)
			{
				dueDate = dueDate.Value.ToUniversalTime();
			}

			if (sentDate.HasValue == true && sentDate.Value.Kind != DateTimeKind.Utc)
			{
				sentDate = sentDate.Value.ToUniversalTime();
			}

			if (paidDate.HasValue == true && paidDate.Value.Kind != DateTimeKind.Utc)
			{
				paidDate = paidDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Invoice> query = (from i in _context.Invoices select i);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(invoiceNumber) == false)
			{
				query = query.Where(i => i.invoiceNumber == invoiceNumber);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(i => i.clientId == clientId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(i => i.contactId == contactId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(i => i.scheduledEventId == scheduledEventId.Value);
			}
			if (financialOfficeId.HasValue == true)
			{
				query = query.Where(i => i.financialOfficeId == financialOfficeId.Value);
			}
			if (invoiceStatusId.HasValue == true)
			{
				query = query.Where(i => i.invoiceStatusId == invoiceStatusId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(i => i.currencyId == currencyId.Value);
			}
			if (taxCodeId.HasValue == true)
			{
				query = query.Where(i => i.taxCodeId == taxCodeId.Value);
			}
			if (invoiceDate.HasValue == true)
			{
				query = query.Where(i => i.invoiceDate == invoiceDate.Value);
			}
			if (dueDate.HasValue == true)
			{
				query = query.Where(i => i.dueDate == dueDate.Value);
			}
			if (subtotal.HasValue == true)
			{
				query = query.Where(i => i.subtotal == subtotal.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(i => i.taxAmount == taxAmount.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(i => i.totalAmount == totalAmount.Value);
			}
			if (amountPaid.HasValue == true)
			{
				query = query.Where(i => i.amountPaid == amountPaid.Value);
			}
			if (sentDate.HasValue == true)
			{
				query = query.Where(i => i.sentDate == sentDate.Value);
			}
			if (paidDate.HasValue == true)
			{
				query = query.Where(i => i.paidDate == paidDate.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(i => i.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(i => i.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(i => i.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(i => i.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(i => i.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(i => i.deleted == false);
				}
			}
			else
			{
				query = query.Where(i => i.active == true);
				query = query.Where(i => i.deleted == false);
			}

			query = query.OrderBy(i => i.invoiceNumber);


			//
			// Add the any string contains parameter to span all the string fields on the Invoice, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.invoiceNumber.Contains(anyStringContains)
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
			       || (includeRelations == true && x.financialOffice.name.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.description.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.code.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.contactName.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.contactEmail.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.exportFormat.Contains(anyStringContains))
			       || (includeRelations == true && x.financialOffice.color.Contains(anyStringContains))
			       || (includeRelations == true && x.invoiceStatus.name.Contains(anyStringContains))
			       || (includeRelations == true && x.invoiceStatus.description.Contains(anyStringContains))
			       || (includeRelations == true && x.invoiceStatus.color.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.description.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.location.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.bookingContactName.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.bookingContactEmail.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.bookingContactPhone.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.color.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduledEvent.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.taxCode.name.Contains(anyStringContains))
			       || (includeRelations == true && x.taxCode.description.Contains(anyStringContains))
			       || (includeRelations == true && x.taxCode.code.Contains(anyStringContains))
			       || (includeRelations == true && x.taxCode.externalTaxCodeId.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.client);
				query = query.Include(x => x.contact);
				query = query.Include(x => x.currency);
				query = query.Include(x => x.financialOffice);
				query = query.Include(x => x.invoiceStatus);
				query = query.Include(x => x.scheduledEvent);
				query = query.Include(x => x.taxCode);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.Invoice> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Invoice invoice in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(invoice, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Invoice Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Invoice Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of Invoices filtered by the parameters provided.  Its query is similar to the GetInvoices method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Invoices/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string invoiceNumber = null,
			int? clientId = null,
			int? contactId = null,
			int? scheduledEventId = null,
			int? financialOfficeId = null,
			int? invoiceStatusId = null,
			int? currencyId = null,
			int? taxCodeId = null,
			DateTime? invoiceDate = null,
			DateTime? dueDate = null,
			decimal? subtotal = null,
			decimal? taxAmount = null,
			decimal? totalAmount = null,
			decimal? amountPaid = null,
			DateTime? sentDate = null,
			DateTime? paidDate = null,
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
			if (invoiceDate.HasValue == true && invoiceDate.Value.Kind != DateTimeKind.Utc)
			{
				invoiceDate = invoiceDate.Value.ToUniversalTime();
			}

			if (dueDate.HasValue == true && dueDate.Value.Kind != DateTimeKind.Utc)
			{
				dueDate = dueDate.Value.ToUniversalTime();
			}

			if (sentDate.HasValue == true && sentDate.Value.Kind != DateTimeKind.Utc)
			{
				sentDate = sentDate.Value.ToUniversalTime();
			}

			if (paidDate.HasValue == true && paidDate.Value.Kind != DateTimeKind.Utc)
			{
				paidDate = paidDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Invoice> query = (from i in _context.Invoices select i);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (invoiceNumber != null)
			{
				query = query.Where(i => i.invoiceNumber == invoiceNumber);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(i => i.clientId == clientId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(i => i.contactId == contactId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(i => i.scheduledEventId == scheduledEventId.Value);
			}
			if (financialOfficeId.HasValue == true)
			{
				query = query.Where(i => i.financialOfficeId == financialOfficeId.Value);
			}
			if (invoiceStatusId.HasValue == true)
			{
				query = query.Where(i => i.invoiceStatusId == invoiceStatusId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(i => i.currencyId == currencyId.Value);
			}
			if (taxCodeId.HasValue == true)
			{
				query = query.Where(i => i.taxCodeId == taxCodeId.Value);
			}
			if (invoiceDate.HasValue == true)
			{
				query = query.Where(i => i.invoiceDate == invoiceDate.Value);
			}
			if (dueDate.HasValue == true)
			{
				query = query.Where(i => i.dueDate == dueDate.Value);
			}
			if (subtotal.HasValue == true)
			{
				query = query.Where(i => i.subtotal == subtotal.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(i => i.taxAmount == taxAmount.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(i => i.totalAmount == totalAmount.Value);
			}
			if (amountPaid.HasValue == true)
			{
				query = query.Where(i => i.amountPaid == amountPaid.Value);
			}
			if (sentDate.HasValue == true)
			{
				query = query.Where(i => i.sentDate == sentDate.Value);
			}
			if (paidDate.HasValue == true)
			{
				query = query.Where(i => i.paidDate == paidDate.Value);
			}
			if (notes != null)
			{
				query = query.Where(i => i.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(i => i.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(i => i.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(i => i.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(i => i.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(i => i.deleted == false);
				}
			}
			else
			{
				query = query.Where(i => i.active == true);
				query = query.Where(i => i.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Invoice, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.invoiceNumber.Contains(anyStringContains)
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
			       || x.financialOffice.name.Contains(anyStringContains)
			       || x.financialOffice.description.Contains(anyStringContains)
			       || x.financialOffice.code.Contains(anyStringContains)
			       || x.financialOffice.contactName.Contains(anyStringContains)
			       || x.financialOffice.contactEmail.Contains(anyStringContains)
			       || x.financialOffice.exportFormat.Contains(anyStringContains)
			       || x.financialOffice.color.Contains(anyStringContains)
			       || x.invoiceStatus.name.Contains(anyStringContains)
			       || x.invoiceStatus.description.Contains(anyStringContains)
			       || x.invoiceStatus.color.Contains(anyStringContains)
			       || x.scheduledEvent.name.Contains(anyStringContains)
			       || x.scheduledEvent.description.Contains(anyStringContains)
			       || x.scheduledEvent.location.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactName.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactEmail.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactPhone.Contains(anyStringContains)
			       || x.scheduledEvent.notes.Contains(anyStringContains)
			       || x.scheduledEvent.color.Contains(anyStringContains)
			       || x.scheduledEvent.externalId.Contains(anyStringContains)
			       || x.scheduledEvent.attributes.Contains(anyStringContains)
			       || x.taxCode.name.Contains(anyStringContains)
			       || x.taxCode.description.Contains(anyStringContains)
			       || x.taxCode.code.Contains(anyStringContains)
			       || x.taxCode.externalTaxCodeId.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Invoice by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Invoice/{id}")]
		public async Task<IActionResult> GetInvoice(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.Invoice> query = (from i in _context.Invoices where
							(i.id == id) &&
							(userIsAdmin == true || i.deleted == false) &&
							(userIsWriter == true || i.active == true)
					select i);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.client);
					query = query.Include(x => x.contact);
					query = query.Include(x => x.currency);
					query = query.Include(x => x.financialOffice);
					query = query.Include(x => x.invoiceStatus);
					query = query.Include(x => x.scheduledEvent);
					query = query.Include(x => x.taxCode);
					query = query.AsSplitQuery();
				}

				Database.Invoice materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Invoice Entity was read with Admin privilege." : "Scheduler.Invoice Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Invoice", materialized.id, materialized.invoiceNumber));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Invoice entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Invoice.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Invoice.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing Invoice record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/Invoice/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutInvoice(int id, [FromBody]Database.Invoice.InvoiceDTO invoiceDTO, CancellationToken cancellationToken = default)
		{
			if (invoiceDTO == null)
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



			if (id != invoiceDTO.id)
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


			IQueryable<Database.Invoice> query = (from x in _context.Invoices
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Invoice existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Invoice PUT", id.ToString(), new Exception("No Scheduler.Invoice entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (invoiceDTO.objectGuid == Guid.Empty)
            {
                invoiceDTO.objectGuid = existing.objectGuid;
            }
            else if (invoiceDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Invoice record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.Invoice cloneOfExisting = (Database.Invoice)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new Invoice object using the data from the existing record, updated with what is in the DTO.
			//
			Database.Invoice invoice = (Database.Invoice)_context.Entry(existing).GetDatabaseValues().ToObject();
			invoice.ApplyDTO(invoiceDTO);
			//
			// The tenant guid for any Invoice being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the Invoice because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				invoice.tenantGuid = existing.tenantGuid;
			}

			lock (invoicePutSyncRoot)
			{
				//
				// Validate the version number for the invoice being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != invoice.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Invoice save attempt was made but save request was with version " + invoice.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The Invoice you are trying to update has already changed.  Please try your save again after reloading the Invoice.");
				}
				else
				{
					// Same record.  Increase version.
					invoice.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (invoice.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Invoice record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (invoice.invoiceNumber != null && invoice.invoiceNumber.Length > 50)
				{
					invoice.invoiceNumber = invoice.invoiceNumber.Substring(0, 50);
				}

				if (invoice.invoiceDate.Kind != DateTimeKind.Utc)
				{
					invoice.invoiceDate = invoice.invoiceDate.ToUniversalTime();
				}

				if (invoice.dueDate.Kind != DateTimeKind.Utc)
				{
					invoice.dueDate = invoice.dueDate.ToUniversalTime();
				}

				if (invoice.sentDate.HasValue == true && invoice.sentDate.Value.Kind != DateTimeKind.Utc)
				{
					invoice.sentDate = invoice.sentDate.Value.ToUniversalTime();
				}

				if (invoice.paidDate.HasValue == true && invoice.paidDate.Value.Kind != DateTimeKind.Utc)
				{
					invoice.paidDate = invoice.paidDate.Value.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.Invoice> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(invoice);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        InvoiceChangeHistory invoiceChangeHistory = new InvoiceChangeHistory();
				        invoiceChangeHistory.invoiceId = invoice.id;
				        invoiceChangeHistory.versionNumber = invoice.versionNumber;
				        invoiceChangeHistory.timeStamp = DateTime.UtcNow;
				        invoiceChangeHistory.userId = securityUser.id;
				        invoiceChangeHistory.tenantGuid = userTenantGuid;
				        invoiceChangeHistory.data = JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice));
				        _context.InvoiceChangeHistories.Add(invoiceChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Invoice entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice)),
						null);

				return Ok(Database.Invoice.CreateAnonymous(invoice));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Invoice entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice)),
						ex);

					return Problem(ex.Message);
				}

			}
		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new Invoice record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Invoice", Name = "Invoice")]
		public async Task<IActionResult> PostInvoice([FromBody]Database.Invoice.InvoiceDTO invoiceDTO, CancellationToken cancellationToken = default)
		{
			if (invoiceDTO == null)
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
			// Create a new Invoice object using the data from the DTO
			//
			Database.Invoice invoice = Database.Invoice.FromDTO(invoiceDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				invoice.tenantGuid = userTenantGuid;

				if (invoice.invoiceNumber != null && invoice.invoiceNumber.Length > 50)
				{
					invoice.invoiceNumber = invoice.invoiceNumber.Substring(0, 50);
				}

				if (invoice.invoiceDate.Kind != DateTimeKind.Utc)
				{
					invoice.invoiceDate = invoice.invoiceDate.ToUniversalTime();
				}

				if (invoice.dueDate.Kind != DateTimeKind.Utc)
				{
					invoice.dueDate = invoice.dueDate.ToUniversalTime();
				}

				if (invoice.sentDate.HasValue == true && invoice.sentDate.Value.Kind != DateTimeKind.Utc)
				{
					invoice.sentDate = invoice.sentDate.Value.ToUniversalTime();
				}

				if (invoice.paidDate.HasValue == true && invoice.paidDate.Value.Kind != DateTimeKind.Utc)
				{
					invoice.paidDate = invoice.paidDate.Value.ToUniversalTime();
				}

				invoice.objectGuid = Guid.NewGuid();
				invoice.versionNumber = 1;

				_context.Invoices.Add(invoice);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the invoice object so that no further changes will be written to the database
				    //
				    _context.Entry(invoice).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					invoice.Documents = null;
					invoice.InvoiceChangeHistories = null;
					invoice.InvoiceLineItems = null;
					invoice.Receipts = null;
					invoice.client = null;
					invoice.contact = null;
					invoice.currency = null;
					invoice.financialOffice = null;
					invoice.invoiceStatus = null;
					invoice.scheduledEvent = null;
					invoice.taxCode = null;


				    InvoiceChangeHistory invoiceChangeHistory = new InvoiceChangeHistory();
				    invoiceChangeHistory.invoiceId = invoice.id;
				    invoiceChangeHistory.versionNumber = invoice.versionNumber;
				    invoiceChangeHistory.timeStamp = DateTime.UtcNow;
				    invoiceChangeHistory.userId = securityUser.id;
				    invoiceChangeHistory.tenantGuid = userTenantGuid;
				    invoiceChangeHistory.data = JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice));
				    _context.InvoiceChangeHistories.Add(invoiceChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.Invoice entity successfully created.",
						true,
						invoice. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Invoice entity creation failed.", false, invoice.id.ToString(), "", JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Invoice", invoice.id, invoice.invoiceNumber));

			return CreatedAtRoute("Invoice", new { id = invoice.id }, Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice));
		}

*/

/* This function is expected to be overridden in a custom file

        /// <summary>
        /// 
        /// This rolls a Invoice entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Invoice/Rollback/{id}")]
		[Route("api/Invoice/Rollback")]
		public async Task<IActionResult> RollbackToInvoiceVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.Invoice> query = (from x in _context.Invoices
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Invoice concurrently
			//
			lock (invoicePutSyncRoot)
			{
				
				Database.Invoice invoice = query.FirstOrDefault();
				
				if (invoice == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Invoice rollback", id.ToString(), new Exception("No Scheduler.Invoice entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Invoice current state so we can log it.
				//
				Database.Invoice cloneOfExisting = (Database.Invoice)_context.Entry(invoice).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.Documents = null;
				cloneOfExisting.InvoiceChangeHistories = null;
				cloneOfExisting.InvoiceLineItems = null;
				cloneOfExisting.Receipts = null;
				cloneOfExisting.client = null;
				cloneOfExisting.contact = null;
				cloneOfExisting.currency = null;
				cloneOfExisting.financialOffice = null;
				cloneOfExisting.invoiceStatus = null;
				cloneOfExisting.scheduledEvent = null;
				cloneOfExisting.taxCode = null;

				if (versionNumber >= invoice.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Invoice rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Invoice rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				InvoiceChangeHistory invoiceChangeHistory = (from x in _context.InvoiceChangeHistories
				                                               where
				                                               x.invoiceId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (invoiceChangeHistory != null)
				{
				    Database.Invoice oldInvoice = JsonSerializer.Deserialize<Database.Invoice>(invoiceChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    invoice.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    invoice.invoiceNumber = oldInvoice.invoiceNumber;
				    invoice.clientId = oldInvoice.clientId;
				    invoice.contactId = oldInvoice.contactId;
				    invoice.scheduledEventId = oldInvoice.scheduledEventId;
				    invoice.financialOfficeId = oldInvoice.financialOfficeId;
				    invoice.invoiceStatusId = oldInvoice.invoiceStatusId;
				    invoice.currencyId = oldInvoice.currencyId;
				    invoice.taxCodeId = oldInvoice.taxCodeId;
				    invoice.invoiceDate = oldInvoice.invoiceDate;
				    invoice.dueDate = oldInvoice.dueDate;
				    invoice.subtotal = oldInvoice.subtotal;
				    invoice.taxAmount = oldInvoice.taxAmount;
				    invoice.totalAmount = oldInvoice.totalAmount;
				    invoice.amountPaid = oldInvoice.amountPaid;
				    invoice.sentDate = oldInvoice.sentDate;
				    invoice.paidDate = oldInvoice.paidDate;
				    invoice.notes = oldInvoice.notes;
				    invoice.objectGuid = oldInvoice.objectGuid;
				    invoice.active = oldInvoice.active;
				    invoice.deleted = oldInvoice.deleted;

				    string serializedInvoice = JsonSerializer.Serialize(invoice);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        InvoiceChangeHistory newInvoiceChangeHistory = new InvoiceChangeHistory();
				        newInvoiceChangeHistory.invoiceId = invoice.id;
				        newInvoiceChangeHistory.versionNumber = invoice.versionNumber;
				        newInvoiceChangeHistory.timeStamp = DateTime.UtcNow;
				        newInvoiceChangeHistory.userId = securityUser.id;
				        newInvoiceChangeHistory.tenantGuid = userTenantGuid;
				        newInvoiceChangeHistory.data = JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice));
				        _context.InvoiceChangeHistories.Add(newInvoiceChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Invoice rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice)),
						null);


				    return Ok(Database.Invoice.CreateAnonymous(invoice));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Invoice rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Invoice rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}

*/



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a Invoice.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Invoice</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Invoice/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetInvoiceChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.Invoice invoice = await _context.Invoices.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (invoice == null)
			{
				return NotFound();
			}

			try
			{
				invoice.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Invoice> versionInfo = await invoice.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a Invoice.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Invoice</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Invoice/{id}/AuditHistory")]
		public async Task<IActionResult> GetInvoiceAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.Invoice invoice = await _context.Invoices.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (invoice == null)
			{
				return NotFound();
			}

			try
			{
				invoice.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.Invoice>> versions = await invoice.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a Invoice.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Invoice</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The Invoice object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Invoice/{id}/Version/{version}")]
		public async Task<IActionResult> GetInvoiceVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.Invoice invoice = await _context.Invoices.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (invoice == null)
			{
				return NotFound();
			}

			try
			{
				invoice.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Invoice> versionInfo = await invoice.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a Invoice at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the Invoice</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The Invoice object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Invoice/{id}/StateAtTime")]
		public async Task<IActionResult> GetInvoiceStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.Invoice invoice = await _context.Invoices.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (invoice == null)
			{
				return NotFound();
			}

			try
			{
				invoice.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.Invoice> versionInfo = await invoice.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a Invoice record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Invoice/{id}")]
		[Route("api/Invoice")]
		public async Task<IActionResult> DeleteInvoice(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.Invoice> query = (from x in _context.Invoices
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.Invoice invoice = await query.FirstOrDefaultAsync(cancellationToken);

			if (invoice == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Invoice DELETE", id.ToString(), new Exception("No Scheduler.Invoice entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Invoice cloneOfExisting = (Database.Invoice)_context.Entry(invoice).GetDatabaseValues().ToObject();


			lock (invoiceDeleteSyncRoot)
			{
			    try
			    {
			        invoice.deleted = true;
			        invoice.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        InvoiceChangeHistory invoiceChangeHistory = new InvoiceChangeHistory();
			        invoiceChangeHistory.invoiceId = invoice.id;
			        invoiceChangeHistory.versionNumber = invoice.versionNumber;
			        invoiceChangeHistory.timeStamp = DateTime.UtcNow;
			        invoiceChangeHistory.userId = securityUser.id;
			        invoiceChangeHistory.tenantGuid = userTenantGuid;
			        invoiceChangeHistory.data = JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice));
			        _context.InvoiceChangeHistories.Add(invoiceChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Invoice entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Invoice entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


*/
        /// <summary>
        /// 
        /// This gets a list of Invoice records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Invoices/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string invoiceNumber = null,
			int? clientId = null,
			int? contactId = null,
			int? scheduledEventId = null,
			int? financialOfficeId = null,
			int? invoiceStatusId = null,
			int? currencyId = null,
			int? taxCodeId = null,
			DateTime? invoiceDate = null,
			DateTime? dueDate = null,
			decimal? subtotal = null,
			decimal? taxAmount = null,
			decimal? totalAmount = null,
			decimal? amountPaid = null,
			DateTime? sentDate = null,
			DateTime? paidDate = null,
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
			if (invoiceDate.HasValue == true && invoiceDate.Value.Kind != DateTimeKind.Utc)
			{
				invoiceDate = invoiceDate.Value.ToUniversalTime();
			}

			if (dueDate.HasValue == true && dueDate.Value.Kind != DateTimeKind.Utc)
			{
				dueDate = dueDate.Value.ToUniversalTime();
			}

			if (sentDate.HasValue == true && sentDate.Value.Kind != DateTimeKind.Utc)
			{
				sentDate = sentDate.Value.ToUniversalTime();
			}

			if (paidDate.HasValue == true && paidDate.Value.Kind != DateTimeKind.Utc)
			{
				paidDate = paidDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Invoice> query = (from i in _context.Invoices select i);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(invoiceNumber) == false)
			{
				query = query.Where(i => i.invoiceNumber == invoiceNumber);
			}
			if (clientId.HasValue == true)
			{
				query = query.Where(i => i.clientId == clientId.Value);
			}
			if (contactId.HasValue == true)
			{
				query = query.Where(i => i.contactId == contactId.Value);
			}
			if (scheduledEventId.HasValue == true)
			{
				query = query.Where(i => i.scheduledEventId == scheduledEventId.Value);
			}
			if (financialOfficeId.HasValue == true)
			{
				query = query.Where(i => i.financialOfficeId == financialOfficeId.Value);
			}
			if (invoiceStatusId.HasValue == true)
			{
				query = query.Where(i => i.invoiceStatusId == invoiceStatusId.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(i => i.currencyId == currencyId.Value);
			}
			if (taxCodeId.HasValue == true)
			{
				query = query.Where(i => i.taxCodeId == taxCodeId.Value);
			}
			if (invoiceDate.HasValue == true)
			{
				query = query.Where(i => i.invoiceDate == invoiceDate.Value);
			}
			if (dueDate.HasValue == true)
			{
				query = query.Where(i => i.dueDate == dueDate.Value);
			}
			if (subtotal.HasValue == true)
			{
				query = query.Where(i => i.subtotal == subtotal.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(i => i.taxAmount == taxAmount.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(i => i.totalAmount == totalAmount.Value);
			}
			if (amountPaid.HasValue == true)
			{
				query = query.Where(i => i.amountPaid == amountPaid.Value);
			}
			if (sentDate.HasValue == true)
			{
				query = query.Where(i => i.sentDate == sentDate.Value);
			}
			if (paidDate.HasValue == true)
			{
				query = query.Where(i => i.paidDate == paidDate.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(i => i.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(i => i.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(i => i.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(i => i.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(i => i.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(i => i.deleted == false);
				}
			}
			else
			{
				query = query.Where(i => i.active == true);
				query = query.Where(i => i.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Invoice, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.invoiceNumber.Contains(anyStringContains)
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
			       || x.financialOffice.name.Contains(anyStringContains)
			       || x.financialOffice.description.Contains(anyStringContains)
			       || x.financialOffice.code.Contains(anyStringContains)
			       || x.financialOffice.contactName.Contains(anyStringContains)
			       || x.financialOffice.contactEmail.Contains(anyStringContains)
			       || x.financialOffice.exportFormat.Contains(anyStringContains)
			       || x.financialOffice.color.Contains(anyStringContains)
			       || x.invoiceStatus.name.Contains(anyStringContains)
			       || x.invoiceStatus.description.Contains(anyStringContains)
			       || x.invoiceStatus.color.Contains(anyStringContains)
			       || x.scheduledEvent.name.Contains(anyStringContains)
			       || x.scheduledEvent.description.Contains(anyStringContains)
			       || x.scheduledEvent.location.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactName.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactEmail.Contains(anyStringContains)
			       || x.scheduledEvent.bookingContactPhone.Contains(anyStringContains)
			       || x.scheduledEvent.notes.Contains(anyStringContains)
			       || x.scheduledEvent.color.Contains(anyStringContains)
			       || x.scheduledEvent.externalId.Contains(anyStringContains)
			       || x.scheduledEvent.attributes.Contains(anyStringContains)
			       || x.taxCode.name.Contains(anyStringContains)
			       || x.taxCode.description.Contains(anyStringContains)
			       || x.taxCode.code.Contains(anyStringContains)
			       || x.taxCode.externalTaxCodeId.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.invoiceNumber);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Invoice.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/Invoice/CreateAuditEvent")]
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
