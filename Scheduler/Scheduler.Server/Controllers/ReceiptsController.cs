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
    /// Custom partial class extension of the auto-generated ReceiptsController.
    ///
    /// Provides business logic endpoints beyond basic CRUD:
    ///   - Create receipt from a payment transaction
    ///   - Create receipt when an invoice is paid
    ///   - Generate next receipt number
    ///   - PDF generation
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    ///
    /// </summary>
    public partial class ReceiptsController
    {
        /// <summary>
        ///
        /// Creates a receipt from a payment transaction.
        /// Populates receipt fields from the payment transaction details.
        ///
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Receipts/CreateFromPayment/{paymentTransactionId}")]
        public async Task<IActionResult> CreateFromPaymentAsync(
            int paymentTransactionId,
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
                    "Attempt to create receipt from payment by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            //
            // Load the payment transaction
            //
            var payment = await _context.PaymentTransactions
                .Include(pt => pt.paymentMethod)
                .Where(pt => pt.id == paymentTransactionId && pt.tenantGuid == tenantGuid && pt.active == true && pt.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (payment == null)
            {
                return NotFound("Payment transaction not found.");
            }

            //
            // Get tenant profile for receipt number mask
            //
            var tenantProfile = await _context.TenantProfiles
                .Where(tp => tp.tenantGuid == tenantGuid && tp.active == true && tp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            //
            // Generate receipt number
            //
            string receiptNumber = await GenerateNextReceiptNumberAsync(tenantGuid, tenantProfile, cancellationToken);

            //
            // Get default ReceiptType (first active one)
            //
            var receiptType = await _context.ReceiptTypes
                .Where(rt => rt.tenantGuid == tenantGuid && rt.active == true && rt.deleted == false)
                .OrderBy(rt => rt.sequence)
                .FirstOrDefaultAsync(cancellationToken);

            if (receiptType == null)
            {
                return BadRequest("No receipt types configured for this tenant.");
            }

            //
            // Create the receipt
            //
            var receipt = new Database.Receipt
            {
                tenantGuid = tenantGuid,
                receiptNumber = receiptNumber,
                receiptTypeId = receiptType.id,
                paymentTransactionId = paymentTransactionId,
                financialTransactionId = payment.financialTransactionId,
                currencyId = payment.currencyId,
                receiptDate = DateTime.UtcNow,
                amount = payment.amount,
                paymentMethod = payment.paymentMethod?.name,
                description = $"Payment received on {payment.transactionDate:MMM dd, yyyy}",
                notes = payment.notes,
                active = true,
                deleted = false,
                versionNumber = 1,
                objectGuid = Guid.NewGuid()
            };

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditType.CreateEntity,
                $"Receipt {receiptNumber} created from payment transaction {paymentTransactionId}. Amount: {payment.amount:C}");

            return Ok(new { receiptId = receipt.id, receiptNumber });
        }


        /// <summary>
        ///
        /// Creates a receipt when an invoice payment is recorded.
        ///
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Receipts/CreateFromInvoicePayment/{invoiceId}")]
        public async Task<IActionResult> CreateFromInvoicePaymentAsync(
            int invoiceId,
            [FromQuery] decimal amount,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);

            //
            // Load the invoice
            //
            var invoice = await _context.Invoices
                .Where(i => i.id == invoiceId && i.tenantGuid == tenantGuid && i.active == true && i.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (invoice == null)
            {
                return NotFound("Invoice not found.");
            }

            var tenantProfile = await _context.TenantProfiles
                .Where(tp => tp.tenantGuid == tenantGuid && tp.active == true && tp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            string receiptNumber = await GenerateNextReceiptNumberAsync(tenantGuid, tenantProfile, cancellationToken);

            var receiptType = await _context.ReceiptTypes
                .Where(rt => rt.tenantGuid == tenantGuid && rt.active == true && rt.deleted == false)
                .OrderBy(rt => rt.sequence)
                .FirstOrDefaultAsync(cancellationToken);

            if (receiptType == null)
            {
                return BadRequest("No receipt types configured for this tenant.");
            }

            var receipt = new Database.Receipt
            {
                tenantGuid = tenantGuid,
                receiptNumber = receiptNumber,
                receiptTypeId = receiptType.id,
                invoiceId = invoiceId,
                clientId = invoice.clientId,
                contactId = invoice.contactId,
                currencyId = invoice.currencyId,
                receiptDate = DateTime.UtcNow,
                amount = amount,
                description = $"Payment for Invoice {invoice.invoiceNumber}",
                active = true,
                deleted = false,
                versionNumber = 1,
                objectGuid = Guid.NewGuid()
            };

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditType.CreateEntity,
                $"Receipt {receiptNumber} created for invoice {invoice.invoiceNumber}. Amount: {amount:C}");

            return Ok(new { receiptId = receipt.id, receiptNumber });
        }


        /// <summary>
        ///
        /// Returns the next sequential receipt number for the tenant.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Receipts/NextReceiptNumber")]
        public async Task<IActionResult> GetNextReceiptNumberAsync(
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

            string nextNumber = await GenerateNextReceiptNumberAsync(tenantGuid, tenantProfile, cancellationToken);

            return Ok(new { receiptNumber = nextNumber });
        }


        /// <summary>
        ///
        /// Generates a PDF for the specified receipt and returns it as a file download.
        /// Also stores the generated PDF as a Document record linked to the receipt.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Receipts/GeneratePdf/{id}")]
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
            // Load receipt
            //
            var receipt = await _context.Receipts
                .Where(r => r.id == id && r.tenantGuid == tenantGuid && r.active == true && r.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (receipt == null)
            {
                return NotFound("Receipt not found.");
            }

            //
            // Load tenant profile for header
            //
            var tenantProfile = await _context.TenantProfiles
                .Where(tp => tp.tenantGuid == tenantGuid && tp.active == true && tp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            //
            // Load client and contact names
            //
            string clientName = null;
            if (receipt.clientId.HasValue)
            {
                var client = await _context.Clients
                    .Where(c => c.id == receipt.clientId.Value && c.tenantGuid == tenantGuid)
                    .FirstOrDefaultAsync(cancellationToken);
                clientName = client?.name;
            }

            string contactName = null;
            if (receipt.contactId.HasValue)
            {
                var contact = await _context.Contacts
                    .Where(c => c.id == receipt.contactId.Value && c.tenantGuid == tenantGuid)
                    .FirstOrDefaultAsync(cancellationToken);
                contactName = contact != null
                    ? $"{contact.firstName} {contact.lastName}".Trim()
                    : null;
            }

            //
            // Load invoice number for reference if linked
            //
            string invoiceNumber = null;
            if (receipt.invoiceId.HasValue)
            {
                var invoice = await _context.Invoices
                    .Where(i => i.id == receipt.invoiceId.Value && i.tenantGuid == tenantGuid)
                    .FirstOrDefaultAsync(cancellationToken);
                invoiceNumber = invoice?.invoiceNumber;
            }

            //
            // Build PDF data
            //
            var pdfData = new ReceiptPdfData
            {
                TenantName = tenantProfile?.name,
                TenantAddress1 = tenantProfile?.addressLine1,
                TenantCity = tenantProfile?.city,
                TenantPhone = tenantProfile?.phoneNumber,
                ReceiptNumber = receipt.receiptNumber,
                ReceiptDate = receipt.receiptDate,
                PaymentMethod = receipt.paymentMethod,
                ClientName = clientName,
                ContactName = contactName,
                Amount = receipt.amount,
                Description = receipt.description,
                InvoiceNumber = invoiceNumber,
                Notes = receipt.notes
            };

            //
            // Generate PDF
            //
            var pdfService = new ReceiptPdfService();
            byte[] pdfBytes = pdfService.GenerateReceiptPdf(pdfData);

            //
            // Store in Document table
            //
            var receiptDocType = await _context.DocumentTypes
                .Where(dt => dt.name == "Receipt" && dt.active == true && dt.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (receiptDocType != null)
            {
                var document = new Database.Document
                {
                    tenantGuid = tenantGuid,
                    documentTypeId = receiptDocType.id,
                    name = $"Receipt {receipt.receiptNumber}",
                    description = $"Generated PDF for receipt {receipt.receiptNumber}",
                    fileName = $"Receipt-{receipt.receiptNumber}.pdf",
                    mimeType = "application/pdf",
                    fileSizeBytes = pdfBytes.Length,
                    fileDataData = pdfBytes,
                    fileDataFileName = $"Receipt-{receipt.receiptNumber}.pdf",
                    fileDataSize = pdfBytes.Length,
                    fileDataMimeType = "application/pdf",
                    receiptId = receipt.id,
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
                $"Receipt PDF generated for {receipt.receiptNumber}");

            return File(pdfBytes, "application/pdf", $"Receipt-{receipt.receiptNumber}.pdf");
        }


        //
        // ── Private helpers ──
        //

        private async Task<string> GenerateNextReceiptNumberAsync(
            Guid tenantGuid,
            Database.TenantProfile tenantProfile,
            CancellationToken cancellationToken)
        {
            string mask = tenantProfile?.receiptNumberMask ?? "REC-{YYYY}-{NNNN}";
            int year = DateTime.UtcNow.Year;

            //
            // Find the highest existing receipt number for this tenant
            //
            int maxSequence = 0;
            var existingReceipts = await _context.Receipts
                .Where(r => r.tenantGuid == tenantGuid && r.active == true && r.deleted == false)
                .Select(r => r.receiptNumber)
                .ToListAsync(cancellationToken);

            foreach (string num in existingReceipts)
            {
                if (num != null)
                {
                    string digits = new string(num.Where(char.IsDigit).ToArray());
                    if (digits.Length > 0)
                    {
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
