// AI-Developed — This file was significantly developed with AI assistance.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Foundation.Security;
using Foundation.Security.Database;
using Foundation.Controllers;
using Foundation.ChangeHistory;
using Foundation.Scheduler.Database;
using static Foundation.Auditor.AuditEngine;

using Scheduler.Server.Services;


namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    ///
    /// Custom partial class extension of the auto-generated InvoicesController.
    ///
    /// Provides business logic endpoints beyond basic CRUD:
    ///   - Create invoice from event charges
    ///   - Generate next invoice number
    ///   - Status transitions
    ///   - PDF generation
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    ///
    /// </summary>
    public partial class InvoicesController
    {
        /// <summary>
        ///
        /// Creates a draft invoice from event charges on a scheduled event.
        ///
        /// Delegates all financial logic to FinancialManagementService, which handles:
        ///   - Invoice + line item creation inside a DB transaction
        ///   - FinancialTransaction ledger entry (Accounts Receivable)
        ///   - EventCharge status cascade → "Invoiced"
        ///   - Invoice number generation with lock
        ///   - Fiscal period validation
        ///   - Change history
        ///
        /// This controller method is a thin wrapper: auth, tenant, audit, HTTP response.
        ///
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Invoices/CreateFromEvent/{eventId}")]
        public async Task<IActionResult> CreateFromEventAsync(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid tenantGuid;

            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "Attempt to create invoice from event by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            //
            // Resolve the FinancialManagementService from DI
            // (cannot constructor-inject because this is a partial class extension of auto-generated code)
            //
            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial Management Service is not available.");
            }

            var result = await financialService.CreateInvoiceFromEventAsync(
                tenantGuid, eventId, securityUser.id, cancellationToken);

            if (!result.Success)
            {
                await CreateAuditEventAsync(AuditType.Miscellaneous,
                    $"Invoice creation from event {eventId} failed: {result.ErrorMessage}");
                return BadRequest(result.ErrorMessage);
            }

            await CreateAuditEventAsync(AuditType.CreateEntity,
                $"Invoice {result.Data["invoiceNumber"]} created from event {eventId} via FinancialManagementService.");

            return Ok(new
            {
                invoiceId = result.Data["invoiceId"],
                invoiceNumber = result.Data["invoiceNumber"]
            });
        }


        /// <summary>
        ///
        /// Returns the next sequential invoice number for the tenant,
        /// using the invoiceNumberMask from TenantProfile.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Invoices/NextInvoiceNumber")]
        public async Task<IActionResult> GetNextInvoiceNumberAsync(
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);

            var tenantProfile = await _context.TenantProfiles
                .Where(tp => tp.tenantGuid == tenantGuid && tp.active == true && tp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            string nextNumber;
            lock (invoicePutSyncRoot)
            {
                nextNumber = GenerateNextInvoiceNumber(tenantGuid, tenantProfile);
            }

            return Ok(new { invoiceNumber = nextNumber });
        }


        /// <summary>
        ///
        /// Generates a PDF for the specified invoice and returns it as a file download.
        /// Also stores the generated PDF as a Document record linked to the invoice.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Invoices/GeneratePdf/{id}")]
        public async Task<IActionResult> GeneratePdfAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);

            //
            // Load invoice with related data
            //
            var invoice = await _context.Invoices
                .Include(i => i.invoiceStatus)
                .Where(i => i.id == id && i.tenantGuid == tenantGuid && i.active == true && i.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (invoice == null)
            {
                return NotFound("Invoice not found.");
            }

            var lineItems = await _context.InvoiceLineItems
                .Where(li => li.invoiceId == id && li.tenantGuid == tenantGuid && li.active == true && li.deleted == false)
                .OrderBy(li => li.sequence)
                .ToListAsync(cancellationToken);

            //
            // Load tenant profile for header
            //
            var tenantProfile = await _context.TenantProfiles
                .Where(tp => tp.tenantGuid == tenantGuid && tp.active == true && tp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            //
            // Load client name
            //
            string clientName = null;
            if (invoice.clientId > 0)
            {
                var client = await _context.Clients
                    .Where(c => c.id == invoice.clientId && c.tenantGuid == tenantGuid)
                    .FirstOrDefaultAsync(cancellationToken);
                clientName = client?.name;
            }

            //
            // Load contact name
            //
            string contactName = null;
            if (invoice.contactId.HasValue)
            {
                var contact = await _context.Contacts
                    .Where(c => c.id == invoice.contactId.Value && c.tenantGuid == tenantGuid)
                    .FirstOrDefaultAsync(cancellationToken);
                contactName = contact != null
                    ? $"{contact.firstName} {contact.lastName}".Trim()
                    : null;
            }

            //
            // Build PDF data
            //
            var pdfData = new InvoicePdfData
            {
                TenantName = tenantProfile?.name,
                TenantAddress1 = tenantProfile?.addressLine1,
                TenantCity = tenantProfile?.city,
                TenantPhone = tenantProfile?.phoneNumber,
                InvoiceNumber = invoice.invoiceNumber,
                InvoiceDate = invoice.invoiceDate,
                DueDate = invoice.dueDate,
                Status = invoice.invoiceStatus?.name,
                ClientName = clientName,
                ContactName = contactName,
                Subtotal = invoice.subtotal,
                TaxAmount = invoice.taxAmount,
                TotalAmount = invoice.totalAmount,
                AmountPaid = invoice.amountPaid,
                Notes = invoice.notes,
                LineItems = lineItems.Select(li => new InvoiceLineItemPdfData
                {
                    Description = li.description,
                    Quantity = li.quantity,
                    UnitPrice = li.unitPrice,
                    TaxAmount = li.taxAmount,
                    TotalAmount = li.totalAmount
                }).ToArray()
            };

            //
            // Generate PDF
            //
            var pdfService = new InvoicePdfService();
            byte[] pdfBytes = pdfService.GenerateInvoicePdf(pdfData);

            //
            // Store in Document table
            //
            var invoiceDocType = await _context.DocumentTypes
                .Where(dt => dt.name == "Invoice" && dt.active == true && dt.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (invoiceDocType != null)
            {
                var document = new Database.Document
                {
                    tenantGuid = tenantGuid,
                    documentTypeId = invoiceDocType.id,
                    name = $"Invoice {invoice.invoiceNumber}",
                    description = $"Generated PDF for invoice {invoice.invoiceNumber}",
                    fileName = $"Invoice-{invoice.invoiceNumber}.pdf",
                    mimeType = "application/pdf",
                    fileSizeBytes = pdfBytes.Length,
                    fileDataData = pdfBytes,
                    fileDataFileName = $"Invoice-{invoice.invoiceNumber}.pdf",
                    fileDataSize = pdfBytes.Length,
                    fileDataMimeType = "application/pdf",
                    invoiceId = invoice.id,
                    uploadedDate = DateTime.UtcNow,
                    uploadedBy = securityUser?.accountName ?? "System",
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                //
                // DocumentType "Invoice" is not configured — log a warning so the administrator knows
                // the PDF is not being persisted in the Document table.
                //
                await CreateAuditEventAsync(AuditType.Error,
                    $"Invoice PDF for {invoice.invoiceNumber} was generated but NOT stored in the Document table because no DocumentType named 'Invoice' exists. Please create this DocumentType to enable PDF archival.");
            }

            await CreateAuditEventAsync(AuditType.ReadEntity,
                $"Invoice PDF generated for {invoice.invoiceNumber}");

            return File(pdfBytes, "application/pdf", $"Invoice-{invoice.invoiceNumber}.pdf");
        }


        /// <summary>
        ///
        /// Voids an invoice, reversing its ledger entry and resetting event charge statuses.
        /// Delegates to FinancialManagementService.VoidInvoiceAsync.
        ///
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Invoices/{id}/Void")]
        public async Task<IActionResult> VoidInvoiceAsync(
            int id,
            [FromBody] VoidRefundRequest request = null,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid tenantGuid;

            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "Attempt to void invoice by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial Management Service is not available.");
            }

            var result = await financialService.VoidInvoiceAsync(
                tenantGuid, id, securityUser.id, request?.Reason, cancellationToken);

            if (!result.Success)
            {
                await CreateAuditEventAsync(AuditType.Miscellaneous,
                    $"Invoice void for {id} failed: {result.ErrorMessage}");
                return BadRequest(result.ErrorMessage);
            }

            await CreateAuditEventAsync(AuditType.UpdateEntity,
                $"Invoice {id} voided via FinancialManagementService. Reason: {request?.Reason ?? "N/A"}");

            return Ok(new { success = true, message = "Invoice voided successfully." });
        }


        /// <summary>
        ///
        /// Issues a partial or full refund against an invoice.
        /// Delegates to FinancialManagementService.IssueRefundAsync.
        ///
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Invoices/{id}/Refund")]
        public async Task<IActionResult> IssueRefundAsync(
            int id,
            [FromBody] RefundRequest request,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null || request.Amount <= 0)
            {
                return BadRequest("Refund amount is required and must be greater than zero.");
            }

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid tenantGuid;

            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "Attempt to refund invoice by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial Management Service is not available.");
            }

            var result = await financialService.IssueRefundAsync(
                tenantGuid, id, request.Amount, securityUser.id, request.Reason, cancellationToken);

            if (!result.Success)
            {
                await CreateAuditEventAsync(AuditType.Miscellaneous,
                    $"Invoice refund for {id} failed: {result.ErrorMessage}");
                return BadRequest(result.ErrorMessage);
            }

            await CreateAuditEventAsync(AuditType.UpdateEntity,
                $"Refund of {request.Amount:C} issued for invoice {id} via FinancialManagementService.");

            return Ok(new { success = true, message = $"Refund of {request.Amount:C} issued successfully." });
        }


        //
        // ── Request DTOs ──
        //

        public class VoidRefundRequest
        {
            public string Reason { get; set; }
        }

        public class RefundRequest
        {
            public decimal Amount { get; set; }
            public string Reason { get; set; }
        }


        //
        // ── Private helpers ──
        //

        /// <summary>
        /// Generates the next sequential invoice number for the given tenant.
        ///
        /// IMPORTANT: This method must be called inside the invoicePutSyncRoot lock
        /// to prevent concurrent requests from generating the same number.
        ///
        /// The method filters existing invoices by the year-specific prefix,
        /// extracts the trailing numeric sequence, and returns the next value
        /// formatted according to the tenant's invoiceNumberMask.
        /// </summary>
        private string GenerateNextInvoiceNumber(
            Guid tenantGuid,
            Database.TenantProfile tenantProfile)
        {
            string mask = tenantProfile?.invoiceNumberMask ?? "INV-{YYYY}-{NNNN}";
            int year = DateTime.UtcNow.Year;

            //
            // Build the prefix by applying year tokens but leaving the sequence placeholder.
            // This lets us filter invoices that match *this year's* prefix.
            //
            string prefixBeforeSequence = mask;
            prefixBeforeSequence = prefixBeforeSequence.Replace("{YYYY}", year.ToString(CultureInfo.InvariantCulture));
            prefixBeforeSequence = prefixBeforeSequence.Replace("{YY}", (year % 100).ToString("D2", CultureInfo.InvariantCulture));

            // Determine where the sequence placeholder starts
            int seqStart = prefixBeforeSequence.IndexOf("{N", StringComparison.Ordinal);
            string yearPrefix = seqStart >= 0 ? prefixBeforeSequence.Substring(0, seqStart) : prefixBeforeSequence;

            //
            // Query only invoices that match this year's prefix using a DB-level StartsWith filter
            //
            int maxSequence = 0;
            var matchingNumbers = _context.Invoices
                .Where(i => i.tenantGuid == tenantGuid && i.invoiceNumber.StartsWith(yearPrefix))
                .Select(i => i.invoiceNumber)
                .ToList();

            foreach (string num in matchingNumbers)
            {
                if (num != null && num.Length > yearPrefix.Length)
                {
                    //
                    // Extract the trailing portion after the prefix and parse as integer
                    //
                    string trailing = num.Substring(yearPrefix.Length);
                    string digits = new string(trailing.Where(char.IsDigit).ToArray());
                    if (digits.Length > 0 && int.TryParse(digits, out int seq))
                    {
                        if (seq > maxSequence)
                        {
                            maxSequence = seq;
                        }
                    }
                }
            }

            int nextSequence = maxSequence + 1;

            //
            // Apply the mask
            //
            string result = mask
                .Replace("{YYYY}", year.ToString(CultureInfo.InvariantCulture))
                .Replace("{YY}", (year % 100).ToString("D2", CultureInfo.InvariantCulture))
                .Replace("{NNNN}", nextSequence.ToString("D4", CultureInfo.InvariantCulture))
                .Replace("{NNN}", nextSequence.ToString("D3", CultureInfo.InvariantCulture))
                .Replace("{NN}", nextSequence.ToString("D2", CultureInfo.InvariantCulture));

            return result;
        }
    }
}
