// AI-Developed — This file was significantly developed with AI assistance.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Foundation.Security;
using Foundation.Security.Database;
using Foundation.Controllers;
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
        /// Copies all active, non-deleted EventCharge records on the specified event
        /// into InvoiceLineItem records on a new Invoice.
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
            // Load the event and its charges
            //
            var scheduledEvent = await _context.ScheduledEvents
                .Where(e => e.id == eventId && e.tenantGuid == tenantGuid && e.active == true && e.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (scheduledEvent == null)
            {
                return NotFound("Scheduled event not found.");
            }

            var eventCharges = await _context.EventCharges
                .Where(ec => ec.scheduledEventId == eventId && ec.tenantGuid == tenantGuid && ec.active == true && ec.deleted == false)
                .ToListAsync(cancellationToken);

            if (eventCharges.Count == 0)
            {
                return BadRequest("No charges found on this event.");
            }

            //
            // Get tenant profile for currency defaults and invoice number mask
            //
            var tenantProfile = await _context.TenantProfiles
                .Where(tp => tp.tenantGuid == tenantGuid && tp.active == true && tp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            //
            // Generate next invoice number
            //
            string invoiceNumber = await GenerateNextInvoiceNumberAsync(tenantGuid, tenantProfile, cancellationToken);

            //
            // Determine default currency (use first charge's currency or fallback)
            //
            int currencyId = eventCharges.First().currencyId;

            //
            // Get default InvoiceStatus (Draft = sequence 1)
            //
            var draftStatus = await _context.InvoiceStatuses
                .Where(s => s.active == true && s.deleted == false)
                .OrderBy(s => s.sequence)
                .FirstAsync(cancellationToken);

            //
            // Create the invoice
            //
            decimal subtotal = eventCharges.Sum(ec => ec.extendedAmount);
            decimal taxAmount = eventCharges.Sum(ec => ec.taxAmount);
            decimal totalAmount = eventCharges.Sum(ec => ec.totalAmount);

            var invoice = new Database.Invoice
            {
                tenantGuid = tenantGuid,
                invoiceNumber = invoiceNumber,
                scheduledEventId = eventId,
                invoiceStatusId = draftStatus.id,
                currencyId = currencyId,
                invoiceDate = DateTime.UtcNow,
                dueDate = DateTime.UtcNow.AddDays(30),
                subtotal = subtotal,
                taxAmount = taxAmount,
                totalAmount = totalAmount,
                amountPaid = 0,
                active = true,
                deleted = false,
                versionNumber = 1,
                objectGuid = Guid.NewGuid()
            };

            //
            // Copy client from event if available
            //
            if (scheduledEvent.clientId.HasValue)
            {
                invoice.clientId = scheduledEvent.clientId.Value;
            }

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(cancellationToken);

            //
            // Create line items from charges
            //
            int sequence = 1;
            foreach (var charge in eventCharges)
            {
                var lineItem = new Database.InvoiceLineItem
                {
                    tenantGuid = tenantGuid,
                    invoiceId = invoice.id,
                    eventChargeId = charge.id,
                    description = charge.description ?? "Event Charge",
                    quantity = charge.quantity ?? 1,
                    unitPrice = charge.unitPrice ?? charge.extendedAmount,
                    amount = charge.extendedAmount,
                    taxAmount = charge.taxAmount,
                    totalAmount = charge.totalAmount,
                    sequence = sequence++,
                    active = true,
                    deleted = false,
                    objectGuid = Guid.NewGuid()
                };

                _context.InvoiceLineItems.Add(lineItem);
            }

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditType.CreateEntity,
                $"Invoice {invoiceNumber} created from event {eventId} with {eventCharges.Count} line items. Total: {totalAmount:C}");

            return Ok(new { invoiceId = invoice.id, invoiceNumber });
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

            string nextNumber = await GenerateNextInvoiceNumberAsync(tenantGuid, tenantProfile, cancellationToken);

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

            await CreateAuditEventAsync(AuditType.ReadEntity,
                $"Invoice PDF generated for {invoice.invoiceNumber}");

            return File(pdfBytes, "application/pdf", $"Invoice-{invoice.invoiceNumber}.pdf");
        }


        //
        // ── Private helpers ──
        //

        private async Task<string> GenerateNextInvoiceNumberAsync(
            Guid tenantGuid,
            Database.TenantProfile tenantProfile,
            CancellationToken cancellationToken)
        {
            string mask = tenantProfile?.invoiceNumberMask ?? "INV-{YYYY}-{NNNN}";
            int year = DateTime.UtcNow.Year;

            //
            // Find the highest existing invoice number for this tenant and year
            //
            string yearPrefix = $"INV-{year}-";

            int maxSequence = 0;
            var existingInvoices = await _context.Invoices
                .Where(i => i.tenantGuid == tenantGuid && i.active == true && i.deleted == false)
                .Select(i => i.invoiceNumber)
                .ToListAsync(cancellationToken);

            foreach (string num in existingInvoices)
            {
                //
                // Try to extract the numeric portion from the existing invoice number
                //
                if (num != null)
                {
                    string digits = new string(num.Where(char.IsDigit).ToArray());
                    if (digits.Length > 0)
                    {
                        // Take the last 4+ digits as the sequence
                        string lastDigits = digits.Length > 4 ? digits.Substring(digits.Length - 4) : digits;
                        if (int.TryParse(lastDigits, out int seq))
                        {
                            if (seq > maxSequence)
                            {
                                maxSequence = seq;
                            }
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
