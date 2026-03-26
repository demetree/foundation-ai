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
        /// Wrapped in a DB transaction with change history and full audit state.
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
            // Use the static lock to safely generate the receipt number and create the receipt
            //
            Database.Receipt receipt;

            lock (receiptPutSyncRoot)
            {
                string receiptNumber = GenerateNextReceiptNumber(tenantGuid, tenantProfile);

                receipt = new Database.Receipt
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

                using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _context.SaveChanges();

                        //
                        // Write change history record (following the auto-generated pattern)
                        //
                        _context.Entry(receipt).State = EntityState.Detached;
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
                        receiptChangeHistory.tenantGuid = tenantGuid;
                        receiptChangeHistory.data = JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt));
                        _context.ReceiptChangeHistories.Add(receiptChangeHistory);

                        _context.SaveChanges();

                        transaction.Commit();

                        //
                        // Audit event with full after-state
                        //
                        CreateAuditEvent(AuditType.CreateEntity,
                            $"Receipt {receipt.receiptNumber} created from payment transaction {paymentTransactionId}. Amount: {payment.amount:C}",
                            true,
                            receipt.id.ToString(),
                            "",
                            JsonSerializer.Serialize(Database.Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt)),
                            null);
                    }
                    catch (Exception ex)
                    {
                        CreateAuditEvent(AuditType.CreateEntity,
                            $"Receipt creation from payment {paymentTransactionId} failed.",
                            false,
                            receipt.id.ToString(),
                            "",
                            "",
                            ex);

                        return Problem(ex.Message);
                    }
                }
            }

            return Ok(new { receiptId = receipt.id, receiptNumber = receipt.receiptNumber });
        }


        /// <summary>
        ///
        /// Creates a receipt when an invoice payment is recorded.
        ///
        /// Validates that the payment amount is positive and does not exceed
        /// the remaining balance on the invoice.
        ///
        /// Wrapped in a DB transaction with change history and full audit state.
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
            Guid tenantGuid;

            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "Attempt to create receipt from invoice payment by user with no tenant. User: " + securityUser?.accountName,
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

            var result = await financialService.RecordInvoicePaymentAsync(
                tenantGuid, invoiceId, amount, securityUser.id,
                cancellationToken: cancellationToken);

            if (!result.Success)
            {
                await CreateAuditEventAsync(AuditType.Miscellaneous,
                    $"Invoice payment for invoice {invoiceId} failed: {result.ErrorMessage}");
                return BadRequest(result.ErrorMessage);
            }

            await CreateAuditEventAsync(AuditType.CreateEntity,
                $"Receipt {result.Data["receiptNumber"]} created for invoice {invoiceId}. " +
                $"Amount: {amount:C}. Balance: {result.Data["invoiceBalance"]:C}. " +
                $"Via FinancialManagementService.");

            return Ok(new
            {
                receiptId = result.Data["receiptId"],
                receiptNumber = result.Data["receiptNumber"],
                invoiceAmountPaid = result.Data["invoiceAmountPaid"],
                invoiceBalance = result.Data["invoiceBalance"]
            });
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

            string nextNumber;
            lock (receiptPutSyncRoot)
            {
                nextNumber = GenerateNextReceiptNumber(tenantGuid, tenantProfile);
            }

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

                // Invalidate the file manager cache so the document appears immediately
                var fileCache = HttpContext.RequestServices.GetService(typeof(FileManagerCacheService)) as FileManagerCacheService;
                fileCache?.InvalidateDocuments(tenantGuid);
            }
            else
            {
                //
                // DocumentType "Receipt" is not configured — log a warning so the administrator knows
                // the PDF is not being persisted in the Document table.
                //
                await CreateAuditEventAsync(AuditType.Error,
                    $"Receipt PDF for {receipt.receiptNumber} was generated but NOT stored in the Document table because no DocumentType named 'Receipt' exists. Please create this DocumentType to enable PDF archival.");
            }

            await CreateAuditEventAsync(AuditType.ReadEntity,
                $"Receipt PDF generated for {receipt.receiptNumber}");

            return File(pdfBytes, "application/pdf", $"Receipt-{receipt.receiptNumber}.pdf");
        }


        //
        // ── Private helpers ──
        //

        /// <summary>
        /// Generates the next sequential receipt number for the given tenant.
        ///
        /// IMPORTANT: This method must be called inside the receiptPutSyncRoot lock
        /// to prevent concurrent requests from generating the same number.
        ///
        /// The method filters existing receipts by the year-specific prefix,
        /// extracts the trailing numeric sequence, and returns the next value
        /// formatted according to the tenant's receiptNumberMask.
        /// </summary>
        private string GenerateNextReceiptNumber(
            Guid tenantGuid,
            Database.TenantProfile tenantProfile)
        {
            string mask = tenantProfile?.receiptNumberMask ?? "REC-{YYYY}-{NNNN}";
            int year = DateTime.UtcNow.Year;

            //
            // Build the prefix by applying year tokens but leaving the sequence placeholder.
            //
            string prefixBeforeSequence = mask;
            prefixBeforeSequence = prefixBeforeSequence.Replace("{YYYY}", year.ToString(CultureInfo.InvariantCulture));
            prefixBeforeSequence = prefixBeforeSequence.Replace("{YY}", (year % 100).ToString("D2", CultureInfo.InvariantCulture));

            // Determine where the sequence placeholder starts
            int seqStart = prefixBeforeSequence.IndexOf("{N", StringComparison.Ordinal);
            string yearPrefix = seqStart >= 0 ? prefixBeforeSequence.Substring(0, seqStart) : prefixBeforeSequence;

            //
            // Query only receipts that match this year's prefix using a DB-level StartsWith filter
            //
            int maxSequence = 0;
            var matchingNumbers = _context.Receipts
                .Where(r => r.tenantGuid == tenantGuid && r.receiptNumber.StartsWith(yearPrefix))
                .Select(r => r.receiptNumber)
                .ToList();

            foreach (string num in matchingNumbers)
            {
                if (num != null && num.Length > yearPrefix.Length)
                {
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
