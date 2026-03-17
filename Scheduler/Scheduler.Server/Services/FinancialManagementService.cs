// AI-Developed — This file was significantly developed with AI assistance.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

using Foundation.ChangeHistory;
using Foundation.Scheduler.Database;


namespace Foundation.Scheduler.Services
{
    /// <summary>
    ///
    /// Centralized financial operations service.
    ///
    /// Every financial operation that touches more than one entity MUST flow through
    /// this service. Controllers are thin wrappers that call these methods.
    ///
    /// Design Principles:
    ///   1. Every operation that moves money runs inside a DB transaction.
    ///   2. Computed fields (Invoice.amountPaid, Pledge.balanceAmount, EventCharge.chargeStatusId)
    ///      are always recalculated from source-of-truth records, never manually set.
    ///   3. Every financial write creates a FinancialTransaction record in the general ledger.
    ///   4. Fiscal period validation prevents writes to closed periods.
    ///   5. Change history and audit events are written for every operation.
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    ///
    /// </summary>
    public class FinancialManagementService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<FinancialManagementService> _logger;

        //
        // Static locks for sequential number generation — shared with the controller
        // partial class definitions that also hold these lock objects.
        //
        private static readonly object _invoiceNumberLock = new object();
        private static readonly object _receiptNumberLock = new object();


        public FinancialManagementService(
            SchedulerContext context,
            ILogger<FinancialManagementService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        // ────────────────────────────────────────────────────────────────────────
        //  Result types
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Result returned from all financial operations.
        /// Controllers inspect Success/ErrorMessage and map to HTTP responses.
        /// </summary>
        public class FinancialOperationResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }

            /// <summary>Created/affected entity IDs for the caller to return.</summary>
            public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

            public static FinancialOperationResult Ok(Dictionary<string, object> data = null)
                => new FinancialOperationResult { Success = true, Data = data ?? new Dictionary<string, object>() };

            public static FinancialOperationResult Fail(string message)
                => new FinancialOperationResult { Success = false, ErrorMessage = message };
        }


        // ────────────────────────────────────────────────────────────────────────
        //  Structured Audit Helpers
        //
        //  Phase 2: Every mutation writes structured JSON into the existing
        //  FinancialTransactionChangeHistory.data column, containing:
        //    - action: Created | Updated | Voided
        //    - fieldChanges: [{field, oldValue, newValue}]
        //    - reason: string (for voids/corrections)
        //    - previousState: full snapshot (for backward compat)
        //
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a structured audit JSON string comparing old vs new transaction state.
        /// </summary>
        private string BuildAuditJson(string action, FinancialTransaction before, FinancialTransaction after, string reason = null)
        {
            var fieldChanges = new List<object>();

            void Diff(string field, object oldVal, object newVal)
            {
                var o = oldVal?.ToString() ?? "";
                var n = newVal?.ToString() ?? "";
                if (o != n) fieldChanges.Add(new { field, oldValue = o, newValue = n });
            }

            if (before != null && after != null)
            {
                // Field-level diff for updates
                Diff("financialCategoryId", before.financialCategoryId, after.financialCategoryId);
                Diff("transactionDate", before.transactionDate.ToString("o"), after.transactionDate.ToString("o"));
                Diff("amount", before.amount, after.amount);
                Diff("taxAmount", before.taxAmount, after.taxAmount);
                Diff("totalAmount", before.totalAmount, after.totalAmount);
                Diff("description", before.description, after.description);
                Diff("isRevenue", before.isRevenue, after.isRevenue);
                Diff("journalEntryType", before.journalEntryType, after.journalEntryType);
                Diff("currencyId", before.currencyId, after.currencyId);
                Diff("financialOfficeId", before.financialOfficeId, after.financialOfficeId);
                Diff("scheduledEventId", before.scheduledEventId, after.scheduledEventId);
                Diff("contactId", before.contactId, after.contactId);
                Diff("clientId", before.clientId, after.clientId);
                Diff("referenceNumber", before.referenceNumber, after.referenceNumber);
                Diff("notes", before.notes, after.notes);
                Diff("fiscalPeriodId", before.fiscalPeriodId, after.fiscalPeriodId);
                Diff("deleted", before.deleted, after.deleted);
            }
            else if (after != null)
            {
                // Created — all fields are new
                Diff("financialCategoryId", null, after.financialCategoryId);
                Diff("transactionDate", null, after.transactionDate.ToString("o"));
                Diff("amount", null, after.amount);
                Diff("taxAmount", null, after.taxAmount);
                Diff("totalAmount", null, after.totalAmount);
                Diff("description", null, after.description);
                Diff("isRevenue", null, after.isRevenue);
                Diff("journalEntryType", null, after.journalEntryType);
                Diff("currencyId", null, after.currencyId);
                Diff("referenceNumber", null, after.referenceNumber);
            }

            var audit = new
            {
                action,
                fieldChanges,
                reason = reason ?? (string)null,
                transactionId = after?.id ?? before?.id ?? 0,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            return JsonSerializer.Serialize(audit);
        }


        /// <summary>
        /// Reads the audit trail for financial transactions.
        /// Returns structured audit entries with user names resolved.
        /// </summary>
        public async Task<List<object>> GetAuditLogAsync(
            Guid tenantGuid,
            int? transactionId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int maxResults = 200,
            CancellationToken cancellationToken = default)
        {
            var query = _context.FinancialTransactionChangeHistories
                .Where(ch => ch.tenantGuid == tenantGuid);

            if (transactionId.HasValue)
                query = query.Where(ch => ch.financialTransactionId == transactionId.Value);

            if (fromDate.HasValue)
                query = query.Where(ch => ch.timeStamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(ch => ch.timeStamp <= toDate.Value);

            var entries = await query
                .OrderByDescending(ch => ch.timeStamp)
                .Take(maxResults)
                .Select(ch => new
                {
                    ch.id,
                    ch.financialTransactionId,
                    ch.versionNumber,
                    ch.timeStamp,
                    ch.userId,
                    ch.data
                })
                .ToListAsync(cancellationToken);

            // Resolve user names
            var userIds = entries.Select(e => e.userId).Distinct().ToList();
            var userMap = new Dictionary<int, string>();
            foreach (var uid in userIds)
            {
                try
                {
                    var u = await Foundation.Security.ChangeHistoryMultiTenant
                        .GetChangeHistoryUserAsync(uid, tenantGuid, cancellationToken);
                    if (u != null)
                        userMap[uid] = $"{u.firstName} {u.lastName}".Trim();
                }
                catch { /* user not found — will default to "Unknown" */ }
            }

            var result = new List<object>();
            foreach (var entry in entries)
            {
                // Try to parse structured audit JSON
                string action = "Unknown";
                object fieldChanges = null;
                string reason = null;

                if (!string.IsNullOrEmpty(entry.data))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(entry.data);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("action", out var actionProp))
                            action = actionProp.GetString();
                        if (root.TryGetProperty("fieldChanges", out var changesProp))
                            fieldChanges = JsonSerializer.Deserialize<object>(changesProp.GetRawText());
                        if (root.TryGetProperty("reason", out var reasonProp) && reasonProp.ValueKind != JsonValueKind.Null)
                            reason = reasonProp.GetString();
                    }
                    catch
                    {
                        // Legacy snapshot format — treat as Unknown action
                        action = entry.versionNumber == 1 ? "Created" : "Updated";
                    }
                }

                result.Add(new
                {
                    entry.id,
                    entry.financialTransactionId,
                    entry.versionNumber,
                    timestamp = entry.timeStamp,
                    entry.userId,
                    userName = userMap.GetValueOrDefault(entry.userId, "Unknown"),
                    action,
                    fieldChanges,
                    reason
                });
            }

            return result;
        }



        // ────────────────────────────────────────────────────────────────────────
        //  Double-Entry GL Posting (Phase 3)
        //
        //  Every financial operation posts balanced journal entries to the GL.
        //  The GL becomes the authoritative ledger for reporting.
        //
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Finds the Cash/Bank contra-account for the tenant (by name convention).
        /// Returns null if no cash account exists yet — GL posting is skipped gracefully.
        /// </summary>
        private async Task<int?> GetCashAccountIdAsync(Guid tenantGuid, CancellationToken cancellationToken)
        {
            var cashAccount = await _context.FinancialCategories
                .Where(c => c.tenantGuid == tenantGuid && c.active == true && c.deleted == false
                         && (c.name.Contains("Cash") || c.name.Contains("Bank")))
                .OrderBy(c => c.id)
                .Select(c => (int?)c.id)
                .FirstOrDefaultAsync(cancellationToken);

            return cashAccount;
        }
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Posts a balanced journal entry to the General Ledger.
        /// debitCategoryId receives the debit; creditCategoryId receives the credit.
        /// </summary>
        public async Task PostToGeneralLedgerAsync(
            Guid tenantGuid,
            int debitCategoryId,
            int creditCategoryId,
            decimal amount,
            DateTime transactionDate,
            string description,
            int postedByUserId,
            int? financialTransactionId = null,
            int? fiscalPeriodId = null,
            int? financialOfficeId = null,
            string referenceNumber = null,
            CancellationToken cancellationToken = default)
        {
            if (amount <= 0) return; // Nothing to post

            // Auto-increment journal entry number per tenant
            int nextJournalNumber = 1;
            var lastEntry = await _context.GeneralLedgerEntries
                .Where(e => e.tenantGuid == tenantGuid)
                .OrderByDescending(e => e.journalEntryNumber)
                .Select(e => e.journalEntryNumber)
                .FirstOrDefaultAsync(cancellationToken);
            if (lastEntry > 0) nextJournalNumber = lastEntry + 1;

            var glEntry = new GeneralLedgerEntry
            {
                tenantGuid = tenantGuid,
                journalEntryNumber = nextJournalNumber,
                transactionDate = transactionDate,
                description = description,
                referenceNumber = referenceNumber,
                financialTransactionId = financialTransactionId,
                fiscalPeriodId = fiscalPeriodId,
                financialOfficeId = financialOfficeId,
                postedBy = postedByUserId,
                postedDate = DateTime.UtcNow,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            glEntry.GeneralLedgerLines.Add(new GeneralLedgerLine
            {
                financialCategoryId = debitCategoryId,
                debitAmount = amount,
                creditAmount = 0,
                description = $"DR: {description}"
            });

            glEntry.GeneralLedgerLines.Add(new GeneralLedgerLine
            {
                financialCategoryId = creditCategoryId,
                debitAmount = 0,
                creditAmount = amount,
                description = $"CR: {description}"
            });

            // Balance validation — ensure debits equal credits before persisting
            decimal totalDebits = glEntry.GeneralLedgerLines.Sum(l => l.debitAmount);
            decimal totalCredits = glEntry.GeneralLedgerLines.Sum(l => l.creditAmount);
            if (totalDebits != totalCredits)
            {
                throw new InvalidOperationException(
                    $"GL entry is unbalanced: debits={totalDebits}, credits={totalCredits}. " +
                    $"Journal entry #{glEntry.journalEntryNumber} for tenant {tenantGuid}.");
            }

            _context.GeneralLedgerEntries.Add(glEntry);
            // SaveChanges is handled by the calling method
        }


        /// <summary>
        /// Posts a reversal entry to the GL (swaps debits and credits of an original entry).
        /// Used when voiding a transaction.
        /// </summary>
        public async Task PostReversalToGLAsync(
            Guid tenantGuid,
            int originalTransactionId,
            int postedByUserId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            // Find the original GL entry for this transaction
            var originalEntry = await _context.GeneralLedgerEntries
                .Include(e => e.GeneralLedgerLines)
                .Where(e => e.tenantGuid == tenantGuid
                         && e.financialTransactionId == originalTransactionId
                         && e.deleted == false
                         && e.reversalOfId == null)
                .FirstOrDefaultAsync(cancellationToken);

            if (originalEntry == null) return; // No GL entry to reverse

            int nextJournalNumber = 1;
            var lastEntry = await _context.GeneralLedgerEntries
                .Where(e => e.tenantGuid == tenantGuid)
                .OrderByDescending(e => e.journalEntryNumber)
                .Select(e => e.journalEntryNumber)
                .FirstOrDefaultAsync(cancellationToken);
            if (lastEntry > 0) nextJournalNumber = lastEntry + 1;

            var reversalEntry = new GeneralLedgerEntry
            {
                tenantGuid = tenantGuid,
                journalEntryNumber = nextJournalNumber,
                transactionDate = DateTime.UtcNow,
                description = $"REVERSAL: {reason}",
                referenceNumber = originalEntry.referenceNumber,
                financialTransactionId = originalTransactionId,
                fiscalPeriodId = originalEntry.fiscalPeriodId,
                financialOfficeId = originalEntry.financialOfficeId,
                postedBy = postedByUserId,
                postedDate = DateTime.UtcNow,
                reversalOfId = originalEntry.id,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            // Swap debits and credits
            foreach (var line in originalEntry.GeneralLedgerLines)
            {
                reversalEntry.GeneralLedgerLines.Add(new GeneralLedgerLine
                {
                    financialCategoryId = line.financialCategoryId,
                    debitAmount = line.creditAmount,   // Swap
                    creditAmount = line.debitAmount,    // Swap
                    description = $"REVERSAL: {line.description}"
                });
            }

            // Balance validation — ensure reversal debits equal credits
            decimal totalDebits = reversalEntry.GeneralLedgerLines.Sum(l => l.debitAmount);
            decimal totalCredits = reversalEntry.GeneralLedgerLines.Sum(l => l.creditAmount);
            if (totalDebits != totalCredits)
            {
                throw new InvalidOperationException(
                    $"GL reversal entry is unbalanced: debits={totalDebits}, credits={totalCredits}. " +
                    $"Journal entry #{reversalEntry.journalEntryNumber} for tenant {tenantGuid}.");
            }

            _context.GeneralLedgerEntries.Add(reversalEntry);
            // SaveChanges is handled by the calling method
        }


        /// <summary>
        /// Gets a GL-based trial balance: totals debits and credits per category.
        /// </summary>
        public async Task<List<object>> GetTrialBalanceFromGLAsync(
            Guid tenantGuid,
            int? fiscalPeriodId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var entryQuery = _context.GeneralLedgerEntries
                .Where(e => e.tenantGuid == tenantGuid && e.active == true && e.deleted == false);

            if (fiscalPeriodId.HasValue)
                entryQuery = entryQuery.Where(e => e.fiscalPeriodId == fiscalPeriodId.Value);
            if (fromDate.HasValue)
                entryQuery = entryQuery.Where(e => e.transactionDate >= fromDate.Value);
            if (toDate.HasValue)
                entryQuery = entryQuery.Where(e => e.transactionDate <= toDate.Value);

            var entryIds = entryQuery.Select(e => e.id);

            var lines = await _context.GeneralLedgerLines
                .Where(l => entryIds.Contains(l.generalLedgerEntryId))
                .GroupBy(l => l.financialCategoryId)
                .Select(g => new
                {
                    financialCategoryId = g.Key,
                    totalDebits = g.Sum(l => l.debitAmount),
                    totalCredits = g.Sum(l => l.creditAmount)
                })
                .ToListAsync(cancellationToken);

            // Resolve category names
            var categoryIds = lines.Select(l => l.financialCategoryId).Distinct().ToList();
            var categories = await _context.FinancialCategories
                .Where(c => categoryIds.Contains(c.id))
                .Select(c => new { c.id, c.name, c.code, c.accountTypeId })
                .ToListAsync(cancellationToken);

            var categoryMap = categories.ToDictionary(c => c.id);

            // Resolve account type names
            var accountTypeIds = categories.Select(c => c.accountTypeId).Distinct().ToList();
            var accountTypes = await _context.AccountTypes
                .Where(at => accountTypeIds.Contains(at.id))
                .Select(at => new { at.id, at.name, at.isRevenue })
                .ToListAsync(cancellationToken);

            var atMap = accountTypes.ToDictionary(at => at.id);

            var totalDebits = lines.Sum(l => l.totalDebits);
            var totalCredits = lines.Sum(l => l.totalCredits);

            var result = lines.Select(l =>
            {
                var cat = categoryMap.GetValueOrDefault(l.financialCategoryId);
                var at = cat != null ? atMap.GetValueOrDefault(cat.accountTypeId) : null;
                return (object)new
                {
                    l.financialCategoryId,
                    categoryName = cat?.name ?? "Unknown",
                    categoryCode = cat?.code,
                    accountType = at?.name ?? "Unknown",
                    isRevenue = at?.isRevenue ?? false,
                    l.totalDebits,
                    l.totalCredits,
                    netBalance = l.totalDebits - l.totalCredits
                };
            }).ToList();

            // Add summary row
            result.Add(new
            {
                financialCategoryId = 0,
                categoryName = "TOTALS",
                categoryCode = (string)null,
                accountType = "",
                isRevenue = false,
                totalDebits,
                totalCredits,
                netBalance = totalDebits - totalCredits
            });

            return result;
        }


        /// <summary>
        /// Reconciles FinancialTransaction ↔ GeneralLedgerEntry.
        /// Returns discrepancies: missing GL entries, orphaned GL entries, and amount mismatches.
        /// </summary>
        public async Task<object> ReconcileGLAsync(
            Guid tenantGuid,
            int? fiscalPeriodId = null,
            CancellationToken cancellationToken = default)
        {
            // Get all non-voided financial transactions
            var txQuery = _context.FinancialTransactions
                .Where(ft => ft.tenantGuid == tenantGuid && ft.active == true && ft.deleted == false);

            if (fiscalPeriodId.HasValue)
                txQuery = txQuery.Where(ft => ft.fiscalPeriodId == fiscalPeriodId.Value);

            var transactions = await txQuery
                .Select(ft => new { ft.id, ft.totalAmount, ft.description, ft.transactionDate, ft.isRevenue })
                .ToListAsync(cancellationToken);

            // Get all non-deleted, non-reversal GL entries
            var glQuery = _context.GeneralLedgerEntries
                .Where(e => e.tenantGuid == tenantGuid && e.active == true && e.deleted == false && e.reversalOfId == null);

            if (fiscalPeriodId.HasValue)
                glQuery = glQuery.Where(e => e.fiscalPeriodId == fiscalPeriodId.Value);

            var glEntries = await glQuery
                .Select(e => new
                {
                    e.id,
                    e.financialTransactionId,
                    e.description,
                    e.transactionDate,
                    totalDebits = e.GeneralLedgerLines.Sum(l => l.debitAmount),
                    totalCredits = e.GeneralLedgerLines.Sum(l => l.creditAmount)
                })
                .ToListAsync(cancellationToken);

            var glByTxId = glEntries
                .Where(e => e.financialTransactionId.HasValue)
                .GroupBy(e => e.financialTransactionId.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var txIds = new HashSet<int>(transactions.Select(t => t.id));

            // 1. Transactions missing GL entries
            var missingGLEntries = transactions
                .Where(t => !glByTxId.ContainsKey(t.id))
                .Select(t => new
                {
                    financialTransactionId = t.id,
                    t.totalAmount,
                    t.description,
                    transactionDate = t.transactionDate.ToString("o"),
                    t.isRevenue,
                    issue = "No GL entry found for this transaction"
                })
                .ToList();

            // 2. Orphaned GL entries (reference a missing/voided transaction)
            var orphanedGLEntries = glEntries
                .Where(e => e.financialTransactionId.HasValue && !txIds.Contains(e.financialTransactionId.Value))
                .Select(e => new
                {
                    generalLedgerEntryId = e.id,
                    e.financialTransactionId,
                    e.description,
                    transactionDate = e.transactionDate.ToString("o"),
                    e.totalDebits,
                    e.totalCredits,
                    issue = "GL entry references a transaction that does not exist or is voided/deleted"
                })
                .ToList();

            // 3. Amount mismatches
            var amountMismatches = new List<object>();
            foreach (var tx in transactions)
            {
                if (glByTxId.TryGetValue(tx.id, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        if (entry.totalDebits != tx.totalAmount || entry.totalCredits != tx.totalAmount)
                        {
                            amountMismatches.Add(new
                            {
                                financialTransactionId = tx.id,
                                generalLedgerEntryId = entry.id,
                                transactionAmount = tx.totalAmount,
                                glDebits = entry.totalDebits,
                                glCredits = entry.totalCredits,
                                issue = "GL amounts do not match transaction amount"
                            });
                        }
                    }
                }
            }

            // 4. Unbalanced GL entries
            var unbalancedEntries = glEntries
                .Where(e => e.totalDebits != e.totalCredits)
                .Select(e => new
                {
                    generalLedgerEntryId = e.id,
                    e.financialTransactionId,
                    e.description,
                    e.totalDebits,
                    e.totalCredits,
                    difference = e.totalDebits - e.totalCredits,
                    issue = "GL entry debits do not equal credits"
                })
                .ToList();

            return new
            {
                summary = new
                {
                    totalTransactions = transactions.Count,
                    totalGLEntries = glEntries.Count,
                    transactionsMissingGL = missingGLEntries.Count,
                    orphanedGLEntries = orphanedGLEntries.Count,
                    amountMismatches = amountMismatches.Count,
                    unbalancedEntries = unbalancedEntries.Count,
                    isClean = missingGLEntries.Count == 0
                              && orphanedGLEntries.Count == 0
                              && amountMismatches.Count == 0
                              && unbalancedEntries.Count == 0
                },
                missingGLEntries,
                orphanedGLEntries,
                amountMismatches,
                unbalancedEntries
            };
        }


        // ────────────────────────────────────────────────────────────────────────
        //  CreateInvoiceFromEvent
        //
        //  Atomically creates:
        //    1. Invoice record
        //    2. InvoiceLineItem records (one per EventCharge)
        //    3. FinancialTransaction (Accounts Receivable ledger entry)
        //    4. InvoiceChangeHistory record
        //    5. Cascades EventCharge.chargeStatusId → "Invoiced"
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> CreateInvoiceFromEventAsync(
            Guid tenantGuid,
            int eventId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            //
            // Validate: event exists
            //
            var scheduledEvent = await _context.ScheduledEvents
                .Where(e => e.id == eventId && e.tenantGuid == tenantGuid && e.active == true && e.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (scheduledEvent == null)
            {
                return FinancialOperationResult.Fail("Scheduled event not found.");
            }

            //
            // Idempotency check — prevent duplicate invoices for the same event
            //
            var existingInvoice = await _context.Invoices
                .Where(i => i.scheduledEventId == eventId && i.tenantGuid == tenantGuid && i.active == true && i.deleted == false)
                .Select(i => new { i.id, i.invoiceNumber })
                .FirstOrDefaultAsync(cancellationToken);

            if (existingInvoice != null)
            {
                return FinancialOperationResult.Fail($"An invoice ({existingInvoice.invoiceNumber}) already exists for this event.");
            }

            //
            // Load event charges
            //
            var eventCharges = await _context.EventCharges
                .Where(ec => ec.scheduledEventId == eventId && ec.tenantGuid == tenantGuid && ec.active == true && ec.deleted == false)
                .ToListAsync(cancellationToken);

            if (eventCharges.Count == 0)
            {
                return FinancialOperationResult.Fail("No charges found on this event.");
            }

            //
            // Resolve reference data
            //
            var tenantProfile = await _context.TenantProfiles
                .Where(tp => tp.tenantGuid == tenantGuid && tp.active == true && tp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            int currencyId = eventCharges.First().currencyId;

            var draftStatus = await _context.InvoiceStatuses
                .Where(s => s.active == true && s.deleted == false)
                .OrderBy(s => s.sequence)
                .FirstAsync(cancellationToken);

            //
            // Find the "Invoiced" charge status for cascading
            //
            var invoicedChargeStatus = await _context.ChargeStatuses
                .Where(cs => cs.name == "Invoiced" && cs.active == true && cs.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            //
            // Validate amounts
            //
            decimal subtotal = eventCharges.Sum(ec => ec.extendedAmount);
            decimal taxAmount = eventCharges.Sum(ec => ec.taxAmount);
            decimal totalAmount = eventCharges.Sum(ec => ec.totalAmount);

            if (subtotal < 0 || taxAmount < 0 || totalAmount < 0)
            {
                return FinancialOperationResult.Fail("Invoice totals cannot be negative.");
            }

            //
            // Validate fiscal period is open for the invoice date
            //
            DateTime invoiceDate = DateTime.UtcNow;
            var periodCheck = await ValidateFiscalPeriodOpenAsync(tenantGuid, invoiceDate, cancellationToken);
            if (!periodCheck.Success)
            {
                return periodCheck;
            }
            int? fiscalPeriodId = (int?)periodCheck.Data.GetValueOrDefault("fiscalPeriodId");

            //
            // Pre-resolve async dependencies before entering the lock
            //
            var revenueCategory = await GetOrCreateFinancialCategoryAsync(
                tenantGuid, "Booking Revenue", true, cancellationToken);

            //
            // Generate invoice number + create everything inside a lock + transaction
            //
            Invoice invoice;
            FinancialTransaction ledgerEntry = null;

            lock (_invoiceNumberLock)
            {
                string invoiceNumber = GenerateNextNumber(
                    tenantGuid,
                    tenantProfile?.invoiceNumberMask ?? "INV-{YYYY}-{NNNN}",
                    _context.Invoices.Where(i => i.tenantGuid == tenantGuid).Select(i => i.invoiceNumber));

                invoice = new Invoice
                {
                    tenantGuid = tenantGuid,
                    invoiceNumber = invoiceNumber,
                    scheduledEventId = eventId,
                    invoiceStatusId = draftStatus.id,
                    currencyId = currencyId,
                    invoiceDate = invoiceDate,
                    dueDate = invoiceDate.AddDays(30),
                    subtotal = subtotal,
                    taxAmount = taxAmount,
                    totalAmount = totalAmount,
                    amountPaid = 0,
                    active = true,
                    deleted = false,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid()
                };

                if (scheduledEvent.clientId.HasValue)
                {
                    invoice.clientId = scheduledEvent.clientId.Value;
                }

                _context.Invoices.Add(invoice);

                using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _context.SaveChanges();

                        //
                        // Create line items from charges
                        //
                        int seq = 1;
                        foreach (var charge in eventCharges)
                        {
                            _context.InvoiceLineItems.Add(new InvoiceLineItem
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
                                sequence = seq++,
                                active = true,
                                deleted = false,
                                objectGuid = Guid.NewGuid()
                            });

                            //
                            // Cascade EventCharge status → "Invoiced"
                            //
                            if (invoicedChargeStatus != null)
                            {
                                charge.chargeStatusId = invoicedChargeStatus.id;
                                _context.Entry(charge).State = EntityState.Modified;
                            }
                        }

                        _context.SaveChanges();

                        //
                        // Create FinancialTransaction ledger entry (Accounts Receivable)
                        // (revenueCategory was pre-resolved before the lock)
                        //

                        if (revenueCategory != null)
                        {
                            ledgerEntry = new FinancialTransaction
                            {
                                tenantGuid = tenantGuid,
                                financialCategoryId = revenueCategory.id,
                                financialOfficeId = invoice.financialOfficeId,
                                scheduledEventId = eventId,
                                clientId = invoice.clientId,
                                contactId = invoice.contactId,
                                fiscalPeriodId = fiscalPeriodId,
                                transactionDate = invoiceDate,
                                description = $"Invoice {invoice.invoiceNumber} — {scheduledEvent.name}",
                                amount = subtotal,
                                taxAmount = taxAmount,
                                totalAmount = totalAmount,
                                isRevenue = true,
                                journalEntryType = "Credit",
                                referenceNumber = invoice.invoiceNumber,
                                currencyId = currencyId,
                                versionNumber = 1,
                                objectGuid = Guid.NewGuid(),
                                active = true,
                                deleted = false
                            };

                            _context.FinancialTransactions.Add(ledgerEntry);
                            _context.SaveChanges();
                        }

                        //
                        // Write change history
                        //
                        WriteInvoiceChangeHistory(invoice, userId, tenantGuid);
                        _context.SaveChanges();

                        transaction.Commit();

                        _logger.LogInformation(
                            "Invoice {InvoiceNumber} created from event {EventId} with {LineCount} line items. " +
                            "Total: {Total}. FinancialTransaction {LedgerId} created.",
                            invoice.invoiceNumber, eventId, eventCharges.Count, totalAmount,
                            ledgerEntry?.id.ToString() ?? "N/A");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create invoice from event {EventId}", eventId);
                        return FinancialOperationResult.Fail($"Invoice creation failed: {ex.Message}");
                    }
                }
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["invoiceId"] = invoice.id,
                ["invoiceNumber"] = invoice.invoiceNumber,
                ["financialTransactionId"] = ledgerEntry?.id
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  RecordInvoicePayment
        //
        //  Atomically:
        //    1. Creates Receipt
        //    2. Updates Invoice.amountPaid (computed from all receipts)
        //    3. Cascades Invoice status (Draft → Partially Paid → Paid)
        //    4. Creates FinancialTransaction (Revenue recognition)
        //    5. Updates EventCharge.chargeStatusId → "Paid" if fully paid
        //    6. Validates fiscal period is open
        //    7. Writes ReceiptChangeHistory
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> RecordInvoicePaymentAsync(
            Guid tenantGuid,
            int invoiceId,
            decimal amount,
            int userId,
            string paymentMethod = null,
            string notes = null,
            CancellationToken cancellationToken = default)
        {
            //
            // Validate amount
            //
            if (amount <= 0)
            {
                return FinancialOperationResult.Fail("Payment amount must be greater than zero.");
            }

            //
            // Load the invoice
            //
            var invoice = await _context.Invoices
                .Where(i => i.id == invoiceId && i.tenantGuid == tenantGuid && i.active == true && i.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (invoice == null)
            {
                return FinancialOperationResult.Fail("Invoice not found.");
            }

            //
            // Compute the actual remaining balance from source-of-truth records
            //
            decimal totalPaid = await _context.Receipts
                .Where(r => r.invoiceId == invoiceId && r.tenantGuid == tenantGuid && r.active == true && r.deleted == false)
                .SumAsync(r => r.amount, cancellationToken);

            decimal remainingBalance = invoice.totalAmount - totalPaid;

            if (amount > remainingBalance + 0.01m)  // 1 cent tolerance for rounding
            {
                return FinancialOperationResult.Fail(
                    $"Payment amount ({amount:C}) exceeds the remaining balance ({remainingBalance:C}) on this invoice.");
            }

            //
            // Validate fiscal period
            //
            DateTime paymentDate = DateTime.UtcNow;
            var periodCheck = await ValidateFiscalPeriodOpenAsync(tenantGuid, paymentDate, cancellationToken);
            if (!periodCheck.Success)
            {
                return periodCheck;
            }
            int? fiscalPeriodId = (int?)periodCheck.Data.GetValueOrDefault("fiscalPeriodId");

            //
            // Resolve reference data
            //
            var tenantProfile = await _context.TenantProfiles
                .Where(tp => tp.tenantGuid == tenantGuid && tp.active == true && tp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            var receiptType = await _context.ReceiptTypes
                .Where(rt => rt.tenantGuid == tenantGuid && rt.active == true && rt.deleted == false)
                .OrderBy(rt => rt.sequence)
                .FirstOrDefaultAsync(cancellationToken);

            if (receiptType == null)
            {
                return FinancialOperationResult.Fail("No receipt types configured for this tenant.");
            }

            //
            // Pre-resolve async dependencies before entering the lock
            //
            var revenueCategory = await GetOrCreateFinancialCategoryAsync(
                tenantGuid, "Booking Revenue", true, cancellationToken);

            // Resolve target invoice status names for cascading
            var paidInvoiceStatus = await _context.InvoiceStatuses
                .Where(s => s.name == "Paid" && s.active == true && s.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);
            var partiallyPaidInvoiceStatus = await _context.InvoiceStatuses
                .Where(s => s.name == "Partially Paid" && s.active == true && s.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);
            var draftInvoiceStatus = await _context.InvoiceStatuses
                .Where(s => s.name == "Draft" && s.active == true && s.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            // Resolve charge status for "Paid" cascade
            var paidChargeStatus = await _context.ChargeStatuses
                .Where(cs => cs.name == "Paid" && cs.active == true && cs.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            //
            // Generate receipt number + create everything inside lock + transaction
            //
            Receipt receipt;
            FinancialTransaction ledgerEntry = null;

            lock (_receiptNumberLock)
            {
                string receiptNumber = GenerateNextNumber(
                    tenantGuid,
                    tenantProfile?.receiptNumberMask ?? "REC-{YYYY}-{NNNN}",
                    _context.Receipts.Where(r => r.tenantGuid == tenantGuid).Select(r => r.receiptNumber));

                receipt = new Receipt
                {
                    tenantGuid = tenantGuid,
                    receiptNumber = receiptNumber,
                    receiptTypeId = receiptType.id,
                    invoiceId = invoiceId,
                    clientId = invoice.clientId,
                    contactId = invoice.contactId,
                    currencyId = invoice.currencyId,
                    receiptDate = paymentDate,
                    amount = amount,
                    paymentMethod = paymentMethod,
                    description = $"Payment for Invoice {invoice.invoiceNumber}",
                    notes = notes,
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
                        // Recalculate Invoice.amountPaid from ALL receipts (not just this one)
                        //
                        decimal newTotalPaid = totalPaid + amount;
                        invoice.amountPaid = newTotalPaid;

                        //
                        // Cascade invoice status (using pre-resolved statuses)
                        //
                        CascadeInvoiceStatusSync(invoice, newTotalPaid,
                            draftInvoiceStatus, partiallyPaidInvoiceStatus, paidInvoiceStatus);
                        _context.Entry(invoice).State = EntityState.Modified;
                        _context.SaveChanges();

                        //
                        // If fully paid, cascade EventCharge status → "Paid"
                        //
                        if (newTotalPaid >= invoice.totalAmount && invoice.scheduledEventId.HasValue
                            && paidChargeStatus != null)
                        {
                            var charges = _context.EventCharges
                                .Where(ec => ec.scheduledEventId == invoice.scheduledEventId.Value
                                          && ec.tenantGuid == tenantGuid
                                          && ec.active == true
                                          && ec.deleted == false)
                                .ToList();

                            foreach (var charge in charges)
                            {
                                charge.chargeStatusId = paidChargeStatus.id;
                                _context.Entry(charge).State = EntityState.Modified;
                            }
                            _context.SaveChanges();
                        }

                        //
                        // Create FinancialTransaction (revenue recognition)
                        // (revenueCategory was pre-resolved before the lock)
                        //

                        if (revenueCategory != null)
                        {
                            ledgerEntry = new FinancialTransaction
                            {
                                tenantGuid = tenantGuid,
                                financialCategoryId = revenueCategory.id,
                                financialOfficeId = invoice.financialOfficeId,
                                scheduledEventId = invoice.scheduledEventId,
                                clientId = invoice.clientId,
                                contactId = invoice.contactId,
                                fiscalPeriodId = fiscalPeriodId,
                                transactionDate = paymentDate,
                                description = $"Payment received — Invoice {invoice.invoiceNumber}",
                                amount = amount,
                                taxAmount = 0,
                                totalAmount = amount,
                                isRevenue = true,
                                journalEntryType = "Debit",
                                referenceNumber = receipt.receiptNumber,
                                currencyId = invoice.currencyId,
                                versionNumber = 1,
                                objectGuid = Guid.NewGuid(),
                                active = true,
                                deleted = false
                            };

                            _context.FinancialTransactions.Add(ledgerEntry);
                            _context.SaveChanges();

                            //
                            // Link the receipt to the FinancialTransaction
                            //
                            receipt.financialTransactionId = ledgerEntry.id;
                            _context.Entry(receipt).State = EntityState.Modified;
                            _context.SaveChanges();
                        }

                        //
                        // Write change history
                        //
                        WriteReceiptChangeHistory(receipt, userId, tenantGuid);
                        _context.SaveChanges();

                        transaction.Commit();

                        _logger.LogInformation(
                            "Payment of {Amount:C} recorded for Invoice {InvoiceNumber}. " +
                            "Receipt {ReceiptNumber}. New balance: {Balance:C}. FinancialTransaction {LedgerId}.",
                            amount, invoice.invoiceNumber, receipt.receiptNumber,
                            invoice.totalAmount - newTotalPaid, ledgerEntry?.id.ToString() ?? "N/A");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to record payment for invoice {InvoiceId}", invoiceId);
                        return FinancialOperationResult.Fail($"Payment recording failed: {ex.Message}");
                    }
                }
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["receiptId"] = receipt.id,
                ["receiptNumber"] = receipt.receiptNumber,
                ["invoiceAmountPaid"] = invoice.amountPaid,
                ["invoiceBalance"] = invoice.totalAmount - invoice.amountPaid,
                ["financialTransactionId"] = ledgerEntry?.id
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  RecordExpense
        //
        //  Creates a FinancialTransaction for an expense.
        //  Validates fiscal period and category.
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> RecordExpenseAsync(
            Guid tenantGuid,
            int financialCategoryId,
            DateTime transactionDate,
            decimal amount,
            decimal taxAmount,
            string description,
            int currencyId,
            int? financialOfficeId = null,
            int? scheduledEventId = null,
            int? contactId = null,
            int? clientId = null,
            string referenceNumber = null,
            string notes = null,
            int userId = 0,
            CancellationToken cancellationToken = default)
        {
            if (amount < 0 || taxAmount < 0)
            {
                return FinancialOperationResult.Fail("Amount and tax amount cannot be negative.");
            }

            //
            // Validate category exists and is an expense category
            //
            var category = await _context.FinancialCategories
                .Where(fc => fc.id == financialCategoryId && fc.tenantGuid == tenantGuid && fc.active == true && fc.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (category == null)
            {
                return FinancialOperationResult.Fail("Financial category not found.");
            }

            //
            // Validate fiscal period
            //
            var periodCheck = await ValidateFiscalPeriodOpenAsync(tenantGuid, transactionDate, cancellationToken);
            if (!periodCheck.Success)
            {
                return periodCheck;
            }
            int? fiscalPeriodId = (int?)periodCheck.Data.GetValueOrDefault("fiscalPeriodId");

            decimal totalAmount = amount + taxAmount;

            var ft = new FinancialTransaction
            {
                tenantGuid = tenantGuid,
                financialCategoryId = financialCategoryId,
                financialOfficeId = financialOfficeId,
                scheduledEventId = scheduledEventId,
                contactId = contactId,
                clientId = clientId,
                fiscalPeriodId = fiscalPeriodId,
                transactionDate = transactionDate,
                description = description?.Length > 500 ? description.Substring(0, 500) : description,
                amount = amount,
                taxAmount = taxAmount,
                totalAmount = totalAmount,
                isRevenue = false,
                journalEntryType = "Debit",
                referenceNumber = referenceNumber,
                notes = notes,
                currencyId = currencyId,
                versionNumber = 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.FinancialTransactions.Add(ft);

            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);

                    // Write structured audit entry
                    _context.FinancialTransactionChangeHistories.Add(new FinancialTransactionChangeHistory
                    {
                        financialTransactionId = ft.id,
                        versionNumber = 1,
                        timeStamp = DateTime.UtcNow,
                        userId = userId,
                        tenantGuid = tenantGuid,
                        data = BuildAuditJson("Created", null, ft)
                    });
                    await _context.SaveChangesAsync(cancellationToken);

                    // Post to General Ledger: DR Expense category, CR Cash/Bank
                    var cashId = await GetCashAccountIdAsync(tenantGuid, cancellationToken);
                    if (cashId.HasValue)
                    {
                        await PostToGeneralLedgerAsync(tenantGuid, financialCategoryId, cashId.Value, totalAmount,
                            transactionDate, $"Expense: {ft.description}", 0, ft.id, ft.fiscalPeriodId, financialOfficeId, referenceNumber, cancellationToken);
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("GL posting skipped for transaction {Id}: no FinancialCategory with 'Cash' or 'Bank' in its name found for tenant {Tenant}. Create one to enable double-entry GL.", ft.id, tenantGuid);
                    }

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Expense recorded: {Amount:C} in category {Category}. FinancialTransaction {Id}.",
                        totalAmount, category.name, ft.id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to record expense");
                    return FinancialOperationResult.Fail($"Expense recording failed: {ex.Message}");
                }
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["financialTransactionId"] = ft.id
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  RecordDirectRevenue
        //
        //  Creates a FinancialTransaction for ad-hoc revenue not tied to invoices.
        //  Example: permit fees, walk-in sales, fundraiser proceeds.
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> RecordDirectRevenueAsync(
            Guid tenantGuid,
            int financialCategoryId,
            DateTime transactionDate,
            decimal amount,
            decimal taxAmount,
            string description,
            int currencyId,
            int? financialOfficeId = null,
            int? scheduledEventId = null,
            int? contactId = null,
            int? clientId = null,
            string referenceNumber = null,
            string notes = null,
            int userId = 0,
            CancellationToken cancellationToken = default)
        {
            if (amount <= 0)
            {
                return FinancialOperationResult.Fail("Revenue amount must be greater than zero.");
            }

            var category = await _context.FinancialCategories
                .Where(fc => fc.id == financialCategoryId && fc.tenantGuid == tenantGuid && fc.active == true && fc.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (category == null)
            {
                return FinancialOperationResult.Fail("Financial category not found.");
            }

            var periodCheck = await ValidateFiscalPeriodOpenAsync(tenantGuid, transactionDate, cancellationToken);
            if (!periodCheck.Success)
            {
                return periodCheck;
            }
            int? fiscalPeriodId = (int?)periodCheck.Data.GetValueOrDefault("fiscalPeriodId");

            decimal totalAmount = amount + taxAmount;

            var ft = new FinancialTransaction
            {
                tenantGuid = tenantGuid,
                financialCategoryId = financialCategoryId,
                financialOfficeId = financialOfficeId,
                scheduledEventId = scheduledEventId,
                contactId = contactId,
                clientId = clientId,
                fiscalPeriodId = fiscalPeriodId,
                transactionDate = transactionDate,
                description = description?.Length > 500 ? description.Substring(0, 500) : description,
                amount = amount,
                taxAmount = taxAmount,
                totalAmount = totalAmount,
                isRevenue = true,
                journalEntryType = "Credit",
                referenceNumber = referenceNumber,
                notes = notes,
                currencyId = currencyId,
                versionNumber = 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.FinancialTransactions.Add(ft);

            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);

                    // Write structured audit entry
                    _context.FinancialTransactionChangeHistories.Add(new FinancialTransactionChangeHistory
                    {
                        financialTransactionId = ft.id,
                        versionNumber = 1,
                        timeStamp = DateTime.UtcNow,
                        userId = userId,
                        tenantGuid = tenantGuid,
                        data = BuildAuditJson("Created", null, ft)
                    });
                    await _context.SaveChangesAsync(cancellationToken);

                    // Post to General Ledger: DR Cash/Bank, CR Revenue category
                    var cashId = await GetCashAccountIdAsync(tenantGuid, cancellationToken);
                    if (cashId.HasValue)
                    {
                        await PostToGeneralLedgerAsync(tenantGuid, cashId.Value, financialCategoryId, totalAmount,
                            transactionDate, $"Revenue: {ft.description}", 0, ft.id, ft.fiscalPeriodId, financialOfficeId, referenceNumber, cancellationToken);
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("GL posting skipped for transaction {Id}: no FinancialCategory with 'Cash' or 'Bank' in its name found for tenant {Tenant}. Create one to enable double-entry GL.", ft.id, tenantGuid);
                    }

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Direct revenue recorded: {Amount:C} in category {Category}. FinancialTransaction {Id}.",
                        totalAmount, category.name, ft.id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to record direct revenue");
                    return FinancialOperationResult.Fail($"Revenue recording failed: {ex.Message}");
                }
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["financialTransactionId"] = ft.id
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  ReconcileInvoiceBalance
        //
        //  Recalculates Invoice.amountPaid from source-of-truth Receipt records.
        //  Cascades the invoice status accordingly.
        //  Use after manual corrections or data imports.
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> ReconcileInvoiceBalanceAsync(
            Guid tenantGuid,
            int invoiceId,
            CancellationToken cancellationToken = default)
        {
            var invoice = await _context.Invoices
                .Where(i => i.id == invoiceId && i.tenantGuid == tenantGuid && i.active == true && i.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (invoice == null)
            {
                return FinancialOperationResult.Fail("Invoice not found.");
            }

            decimal computedPaid = await _context.Receipts
                .Where(r => r.invoiceId == invoiceId && r.tenantGuid == tenantGuid && r.active == true && r.deleted == false)
                .SumAsync(r => r.amount, cancellationToken);

            decimal previousPaid = invoice.amountPaid;
            invoice.amountPaid = computedPaid;

            await CascadeInvoiceStatusAsync(invoice, computedPaid, cancellationToken);

            _context.Entry(invoice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);

                if (previousPaid != computedPaid)
                {
                    _logger.LogWarning(
                        "Invoice {InvoiceNumber} balance reconciled: was {Previous:C}, now {Current:C}",
                        invoice.invoiceNumber, previousPaid, computedPaid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconcile invoice {InvoiceId}", invoiceId);
                return FinancialOperationResult.Fail($"Reconciliation failed: {ex.Message}");
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["previousAmountPaid"] = previousPaid,
                ["currentAmountPaid"] = computedPaid,
                ["invoiceBalance"] = invoice.totalAmount - computedPaid,
                ["adjusted"] = previousPaid != computedPaid
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  RecordGift
        //
        //  Atomically:
        //    1. Creates Gift record
        //    2. Recalculates Pledge.balanceAmount (if pledgeId is present)
        //    3. Creates FinancialTransaction (Donation Revenue)
        //    4. Validates fiscal period is open
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> RecordGiftAsync(
            Guid tenantGuid,
            int constituentId,
            decimal amount,
            int fundId,
            int paymentTypeId,
            DateTime receivedDate,
            int? pledgeId = null,
            int? campaignId = null,
            int? appealId = null,
            int? officeId = null,
            int? batchId = null,
            string referenceNumber = null,
            string notes = null,
            CancellationToken cancellationToken = default)
        {
            if (amount <= 0)
            {
                return FinancialOperationResult.Fail("Gift amount must be greater than zero.");
            }

            //
            // Validate constituent exists
            //
            var constituent = await _context.Constituents
                .Where(c => c.id == constituentId && c.tenantGuid == tenantGuid && c.active == true && c.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (constituent == null)
            {
                return FinancialOperationResult.Fail("Constituent not found.");
            }

            //
            // Validate pledge if specified
            //
            Pledge pledge = null;
            if (pledgeId.HasValue)
            {
                pledge = await _context.Pledges
                    .Where(p => p.id == pledgeId.Value && p.tenantGuid == tenantGuid && p.active == true && p.deleted == false)
                    .FirstOrDefaultAsync(cancellationToken);

                if (pledge == null)
                {
                    return FinancialOperationResult.Fail("Pledge not found.");
                }
            }

            //
            // Validate fiscal period
            //
            var periodCheck = await ValidateFiscalPeriodOpenAsync(tenantGuid, receivedDate, cancellationToken);
            if (!periodCheck.Success)
            {
                return periodCheck;
            }
            int? fiscalPeriodId = (int?)periodCheck.Data.GetValueOrDefault("fiscalPeriodId");

            //
            // Pre-resolve async dependencies
            //
            var donationCategory = await GetOrCreateFinancialCategoryAsync(
                tenantGuid, "Donation Revenue", true, cancellationToken);

            //
            // Create gift + ledger entry inside a transaction
            //
            var gift = new Gift
            {
                tenantGuid = tenantGuid,
                constituentId = constituentId,
                amount = amount,
                receivedDate = receivedDate,
                fundId = fundId,
                paymentTypeId = paymentTypeId,
                pledgeId = pledgeId,
                campaignId = campaignId,
                appealId = appealId,
                officeId = officeId,
                batchId = batchId,
                referenceNumber = referenceNumber,
                notes = notes,
                versionNumber = 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            FinancialTransaction ledgerEntry = null;

            using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    _context.Gifts.Add(gift);
                    await _context.SaveChangesAsync(cancellationToken);

                    //
                    // Recalculate Pledge.balanceAmount from ALL gifts (not just this one)
                    //
                    if (pledge != null)
                    {
                        decimal totalGifts = await _context.Gifts
                            .Where(g => g.pledgeId == pledgeId.Value && g.tenantGuid == tenantGuid
                                     && g.active == true && g.deleted == false)
                            .SumAsync(g => g.amount, cancellationToken);

                        pledge.balanceAmount = pledge.totalAmount - totalGifts;
                        if (pledge.balanceAmount < 0) pledge.balanceAmount = 0;
                        _context.Entry(pledge).State = EntityState.Modified;
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    //
                    // Create FinancialTransaction
                    //
                    if (donationCategory != null)
                    {
                        ledgerEntry = new FinancialTransaction
                        {
                            tenantGuid = tenantGuid,
                            financialCategoryId = donationCategory.id,
                            contactId = null,
                            fiscalPeriodId = fiscalPeriodId,
                            transactionDate = receivedDate,
                            description = $"Gift from constituent #{constituent.constituentNumber}",
                            amount = amount,
                            taxAmount = 0,
                            totalAmount = amount,
                            isRevenue = true,
                            journalEntryType = "Credit",
                            referenceNumber = referenceNumber,
                            versionNumber = 1,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        };

                        _context.FinancialTransactions.Add(ledgerEntry);
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Gift of {Amount:C} recorded from constituent {ConstituentId}. Pledge balance: {Balance:C}. FinancialTransaction {LedgerId}.",
                        amount, constituentId, pledge?.balanceAmount, ledgerEntry?.id.ToString() ?? "N/A");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to record gift from constituent {ConstituentId}", constituentId);
                    return FinancialOperationResult.Fail($"Gift recording failed: {ex.Message}");
                }
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["giftId"] = gift.id,
                ["pledgeBalance"] = pledge?.balanceAmount,
                ["financialTransactionId"] = ledgerEntry?.id
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  VoidInvoice
        //
        //  Atomically:
        //    1. Sets Invoice status → "Voided"
        //    2. Creates reversing FinancialTransaction
        //    3. Resets EventCharge statuses → "Pending"
        //    4. Writes change history
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> VoidInvoiceAsync(
            Guid tenantGuid,
            int invoiceId,
            int userId,
            string reason = null,
            CancellationToken cancellationToken = default)
        {
            var invoice = await _context.Invoices
                .Where(i => i.id == invoiceId && i.tenantGuid == tenantGuid && i.active == true && i.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (invoice == null)
            {
                return FinancialOperationResult.Fail("Invoice not found.");
            }

            //
            // Cannot void a paid invoice — must refund first
            //
            if (invoice.amountPaid > 0)
            {
                return FinancialOperationResult.Fail(
                    $"Cannot void invoice {invoice.invoiceNumber} — it has payments of {invoice.amountPaid:C}. Issue refunds first.");
            }

            //
            // Validate fiscal period
            //
            var periodCheck = await ValidateFiscalPeriodOpenAsync(tenantGuid, DateTime.UtcNow, cancellationToken);
            if (!periodCheck.Success)
            {
                return periodCheck;
            }

            //
            // Pre-resolve status entities
            //
            var voidedStatus = await _context.InvoiceStatuses
                .Where(s => s.name == "Voided" && s.active == true && s.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            var pendingChargeStatus = await _context.ChargeStatuses
                .Where(cs => cs.name == "Pending" && cs.active == true && cs.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            var revenueCategory = await GetOrCreateFinancialCategoryAsync(
                tenantGuid, "Booking Revenue", true, cancellationToken);

            using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    //
                    // Set invoice status to Voided
                    //
                    if (voidedStatus != null)
                    {
                        invoice.invoiceStatusId = voidedStatus.id;
                    }
                    invoice.notes = string.IsNullOrEmpty(invoice.notes)
                        ? $"VOIDED: {reason ?? "No reason given"}"
                        : $"{invoice.notes}\nVOIDED: {reason ?? "No reason given"}";
                    _context.Entry(invoice).State = EntityState.Modified;

                    //
                    // Create reversing FinancialTransaction
                    //
                    if (revenueCategory != null && invoice.totalAmount > 0)
                    {
                        var reversingEntry = new FinancialTransaction
                        {
                            tenantGuid = tenantGuid,
                            financialCategoryId = revenueCategory.id,
                            financialOfficeId = invoice.financialOfficeId,
                            scheduledEventId = invoice.scheduledEventId,
                            clientId = invoice.clientId,
                            contactId = invoice.contactId,
                            transactionDate = DateTime.UtcNow,
                            description = $"VOID — Invoice {invoice.invoiceNumber}: {reason ?? "voided"}",
                            amount = -invoice.subtotal,
                            taxAmount = -invoice.taxAmount,
                            totalAmount = -invoice.totalAmount,
                            isRevenue = true,
                            journalEntryType = "Debit",
                            referenceNumber = invoice.invoiceNumber,
                            versionNumber = 1,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        };

                        _context.FinancialTransactions.Add(reversingEntry);
                    }

                    //
                    // Reset EventCharge statuses back to "Pending"
                    //
                    if (invoice.scheduledEventId.HasValue && pendingChargeStatus != null)
                    {
                        var charges = await _context.EventCharges
                            .Where(ec => ec.scheduledEventId == invoice.scheduledEventId.Value
                                      && ec.tenantGuid == tenantGuid
                                      && ec.active == true && ec.deleted == false)
                            .ToListAsync(cancellationToken);

                        foreach (var charge in charges)
                        {
                            charge.chargeStatusId = pendingChargeStatus.id;
                            _context.Entry(charge).State = EntityState.Modified;
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    //
                    // Write change history
                    //
                    WriteInvoiceChangeHistory(invoice, userId, tenantGuid);
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Invoice {InvoiceNumber} voided. Reason: {Reason}",
                        invoice.invoiceNumber, reason ?? "none given");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to void invoice {InvoiceId}", invoiceId);
                    return FinancialOperationResult.Fail($"Invoice void failed: {ex.Message}");
                }
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["invoiceId"] = invoice.id,
                ["invoiceNumber"] = invoice.invoiceNumber
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  IssueRefund
        //
        //  Atomically:
        //    1. Creates a negative Receipt (refund receipt)
        //    2. Recalculates Invoice.amountPaid from all receipts
        //    3. Cascades invoice status
        //    4. Creates reversing FinancialTransaction
        //    5. Validates fiscal period
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> IssueRefundAsync(
            Guid tenantGuid,
            int invoiceId,
            decimal refundAmount,
            int userId,
            string reason = null,
            CancellationToken cancellationToken = default)
        {
            if (refundAmount <= 0)
            {
                return FinancialOperationResult.Fail("Refund amount must be greater than zero.");
            }

            var invoice = await _context.Invoices
                .Where(i => i.id == invoiceId && i.tenantGuid == tenantGuid && i.active == true && i.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (invoice == null)
            {
                return FinancialOperationResult.Fail("Invoice not found.");
            }

            //
            // Compute actual paid from receipts
            //
            decimal totalPaid = await _context.Receipts
                .Where(r => r.invoiceId == invoiceId && r.tenantGuid == tenantGuid && r.active == true && r.deleted == false)
                .SumAsync(r => r.amount, cancellationToken);

            if (refundAmount > totalPaid + 0.01m)
            {
                return FinancialOperationResult.Fail(
                    $"Refund amount ({refundAmount:C}) exceeds total payments ({totalPaid:C}) on this invoice.");
            }

            //
            // Validate fiscal period
            //
            DateTime refundDate = DateTime.UtcNow;
            var periodCheck = await ValidateFiscalPeriodOpenAsync(tenantGuid, refundDate, cancellationToken);
            if (!periodCheck.Success)
            {
                return periodCheck;
            }
            int? fiscalPeriodId = (int?)periodCheck.Data.GetValueOrDefault("fiscalPeriodId");

            //
            // Pre-resolve async dependencies
            //
            var tenantProfile = await _context.TenantProfiles
                .Where(tp => tp.tenantGuid == tenantGuid && tp.active == true && tp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            var receiptType = await _context.ReceiptTypes
                .Where(rt => rt.tenantGuid == tenantGuid && rt.active == true && rt.deleted == false)
                .OrderBy(rt => rt.sequence)
                .FirstOrDefaultAsync(cancellationToken);

            if (receiptType == null)
            {
                return FinancialOperationResult.Fail("No receipt types configured for this tenant.");
            }

            var revenueCategory = await GetOrCreateFinancialCategoryAsync(
                tenantGuid, "Booking Revenue", true, cancellationToken);

            // Pre-resolve invoice statuses for lock block
            var paidInvoiceStatus = await _context.InvoiceStatuses
                .Where(s => s.name == "Paid" && s.active == true && s.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);
            var partiallyPaidInvoiceStatus = await _context.InvoiceStatuses
                .Where(s => s.name == "Partially Paid" && s.active == true && s.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);
            var draftInvoiceStatus = await _context.InvoiceStatuses
                .Where(s => s.name == "Draft" && s.active == true && s.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            Receipt refundReceipt;
            FinancialTransaction ledgerEntry = null;

            lock (_receiptNumberLock)
            {
                string refundReceiptNumber = GenerateNextNumber(
                    tenantGuid,
                    tenantProfile?.receiptNumberMask ?? "REC-{YYYY}-{NNNN}",
                    _context.Receipts.Where(r => r.tenantGuid == tenantGuid).Select(r => r.receiptNumber));

                refundReceipt = new Receipt
                {
                    tenantGuid = tenantGuid,
                    receiptNumber = refundReceiptNumber,
                    receiptTypeId = receiptType.id,
                    invoiceId = invoiceId,
                    clientId = invoice.clientId,
                    contactId = invoice.contactId,
                    currencyId = invoice.currencyId,
                    receiptDate = refundDate,
                    amount = -refundAmount,  // Negative amount = refund
                    description = $"REFUND for Invoice {invoice.invoiceNumber}: {reason ?? "refund issued"}",
                    active = true,
                    deleted = false,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid()
                };

                _context.Receipts.Add(refundReceipt);

                using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _context.SaveChanges();

                        //
                        // Recalculate Invoice.amountPaid from ALL receipts (including the refund)
                        //
                        decimal newTotalPaid = totalPaid - refundAmount;
                        if (newTotalPaid < 0) newTotalPaid = 0;
                        invoice.amountPaid = newTotalPaid;

                        //
                        // Cascade invoice status
                        //
                        CascadeInvoiceStatusSync(invoice, newTotalPaid,
                            draftInvoiceStatus, partiallyPaidInvoiceStatus, paidInvoiceStatus);
                        _context.Entry(invoice).State = EntityState.Modified;
                        _context.SaveChanges();

                        //
                        // Create reversing FinancialTransaction
                        //
                        if (revenueCategory != null)
                        {
                            ledgerEntry = new FinancialTransaction
                            {
                                tenantGuid = tenantGuid,
                                financialCategoryId = revenueCategory.id,
                                financialOfficeId = invoice.financialOfficeId,
                                scheduledEventId = invoice.scheduledEventId,
                                clientId = invoice.clientId,
                                contactId = invoice.contactId,
                                fiscalPeriodId = fiscalPeriodId,
                                transactionDate = refundDate,
                                description = $"REFUND — Invoice {invoice.invoiceNumber}: {reason ?? "refund"}",
                                amount = -refundAmount,
                                taxAmount = 0,
                                totalAmount = -refundAmount,
                                isRevenue = true,
                                journalEntryType = "Debit",
                                referenceNumber = refundReceipt.receiptNumber,
                                currencyId = invoice.currencyId,
                                versionNumber = 1,
                                objectGuid = Guid.NewGuid(),
                                active = true,
                                deleted = false
                            };

                            _context.FinancialTransactions.Add(ledgerEntry);
                            _context.SaveChanges();

                            refundReceipt.financialTransactionId = ledgerEntry.id;
                            _context.Entry(refundReceipt).State = EntityState.Modified;
                            _context.SaveChanges();
                        }

                        //
                        // Write change history
                        //
                        WriteReceiptChangeHistory(refundReceipt, userId, tenantGuid);
                        _context.SaveChanges();

                        transaction.Commit();

                        _logger.LogInformation(
                            "Refund of {Amount:C} issued for Invoice {InvoiceNumber}. New balance: {Balance:C}.",
                            refundAmount, invoice.invoiceNumber, invoice.totalAmount - newTotalPaid);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to issue refund for invoice {InvoiceId}", invoiceId);
                        return FinancialOperationResult.Fail($"Refund failed: {ex.Message}");
                    }
                }
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["receiptId"] = refundReceipt.id,
                ["receiptNumber"] = refundReceipt.receiptNumber,
                ["invoiceAmountPaid"] = invoice.amountPaid,
                ["invoiceBalance"] = invoice.totalAmount - invoice.amountPaid,
                ["financialTransactionId"] = ledgerEntry?.id
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  ReconcilePledgeBalance
        //
        //  Recalculates Pledge.balanceAmount from source-of-truth Gift records.
        //  Use after manual corrections or data imports.
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> ReconcilePledgeBalanceAsync(
            Guid tenantGuid,
            int pledgeId,
            CancellationToken cancellationToken = default)
        {
            var pledge = await _context.Pledges
                .Where(p => p.id == pledgeId && p.tenantGuid == tenantGuid && p.active == true && p.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (pledge == null)
            {
                return FinancialOperationResult.Fail("Pledge not found.");
            }

            decimal totalGifts = await _context.Gifts
                .Where(g => g.pledgeId == pledgeId && g.tenantGuid == tenantGuid
                         && g.active == true && g.deleted == false)
                .SumAsync(g => g.amount, cancellationToken);

            decimal previousBalance = pledge.balanceAmount;
            pledge.balanceAmount = pledge.totalAmount - totalGifts;
            if (pledge.balanceAmount < 0) pledge.balanceAmount = 0;

            _context.Entry(pledge).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);

                if (previousBalance != pledge.balanceAmount)
                {
                    _logger.LogWarning(
                        "Pledge {PledgeId} balance reconciled: was {Previous:C}, now {Current:C}",
                        pledgeId, previousBalance, pledge.balanceAmount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconcile pledge {PledgeId}", pledgeId);
                return FinancialOperationResult.Fail($"Pledge reconciliation failed: {ex.Message}");
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["previousBalance"] = previousBalance,
                ["currentBalance"] = pledge.balanceAmount,
                ["totalGifts"] = totalGifts,
                ["adjusted"] = previousBalance != pledge.balanceAmount
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  CloseFiscalPeriod
        //
        //  Atomically:
        //    1. Validates no unresolved invoices exist in the period
        //    2. Sets period status → "Closed"
        //    3. Records closedDate and closedBy
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> CloseFiscalPeriodAsync(
            Guid tenantGuid,
            int fiscalPeriodId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var fiscalPeriod = await _context.FiscalPeriods
                .Include(fp => fp.periodStatus)
                .Where(fp => fp.id == fiscalPeriodId && fp.tenantGuid == tenantGuid
                          && fp.active == true && fp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (fiscalPeriod == null)
            {
                return FinancialOperationResult.Fail("Fiscal period not found.");
            }

            if (fiscalPeriod.periodStatus != null &&
                fiscalPeriod.periodStatus.name.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                return FinancialOperationResult.Fail(
                    $"Fiscal period '{fiscalPeriod.name}' is already closed.");
            }

            //
            // Check for unpaid invoices in this period
            //
            var unpaidInvoices = await _context.Invoices
                .Include(i => i.invoiceStatus)
                .Where(i => i.tenantGuid == tenantGuid
                         && i.invoiceDate >= fiscalPeriod.startDate
                         && i.invoiceDate <= fiscalPeriod.endDate
                         && i.active == true && i.deleted == false
                         && i.amountPaid < i.totalAmount
                         && (i.invoiceStatus == null || i.invoiceStatus.name != "Voided"))
                .CountAsync(cancellationToken);

            if (unpaidInvoices > 0)
            {
                return FinancialOperationResult.Fail(
                    $"Cannot close fiscal period '{fiscalPeriod.name}' — {unpaidInvoices} unpaid invoice(s) remain.");
            }

            //
            // Close the period
            //
            var closedStatus = await _context.PeriodStatuses
                .Where(ps => ps.name == "Closed" && ps.active == true && ps.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (closedStatus == null)
            {
                return FinancialOperationResult.Fail("Period status 'Closed' not found. Please configure period statuses.");
            }

            fiscalPeriod.periodStatusId = closedStatus.id;
            fiscalPeriod.closedDate = DateTime.UtcNow;
            fiscalPeriod.closedBy = userId.ToString();
            _context.Entry(fiscalPeriod).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Fiscal period '{Name}' ({Start:MMM yyyy} – {End:MMM yyyy}) closed by user {UserId}.",
                    fiscalPeriod.name, fiscalPeriod.startDate, fiscalPeriod.endDate, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to close fiscal period {PeriodId}", fiscalPeriodId);
                return FinancialOperationResult.Fail($"Fiscal period close failed: {ex.Message}");
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["fiscalPeriodId"] = fiscalPeriod.id,
                ["periodName"] = fiscalPeriod.name,
                ["closedDate"] = fiscalPeriod.closedDate
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  Private Helpers
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Validates that the fiscal period covering the given date is open.
        /// Returns the matching period's ID in result.Data["fiscalPeriodId"].
        /// If no period covers the date, the result is still successful (period is optional).
        /// If a period exists but is closed, returns failure.
        /// </summary>
        private async Task<FinancialOperationResult> ValidateFiscalPeriodOpenAsync(
            Guid tenantGuid,
            DateTime transactionDate,
            CancellationToken cancellationToken)
        {
            var fiscalPeriod = await _context.FiscalPeriods
                .Include(fp => fp.periodStatus)
                .Where(fp => fp.tenantGuid == tenantGuid
                          && fp.startDate <= transactionDate
                          && fp.endDate >= transactionDate
                          && fp.active == true
                          && fp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (fiscalPeriod == null)
            {
                // No period configured for this date — allow the operation
                return FinancialOperationResult.Ok(new Dictionary<string, object>
                {
                    ["fiscalPeriodId"] = null
                });
            }

            //
            // Check if the period is closed
            //
            if (fiscalPeriod.periodStatus != null &&
                fiscalPeriod.periodStatus.name.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                return FinancialOperationResult.Fail(
                    $"Fiscal period '{fiscalPeriod.name}' ({fiscalPeriod.startDate:MMM yyyy}) is closed. " +
                    $"Financial transactions cannot be recorded in a closed period.");
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["fiscalPeriodId"] = fiscalPeriod.id
            });
        }


        /// <summary>
        /// Cascades the invoice status based on amount paid vs total.
        /// Looks for statuses named "Draft", "Partially Paid", "Paid".
        /// </summary>
        private async Task CascadeInvoiceStatusAsync(
            Invoice invoice,
            decimal totalPaid,
            CancellationToken cancellationToken)
        {
            string targetStatusName;

            if (totalPaid <= 0)
            {
                targetStatusName = "Draft";
            }
            else if (totalPaid >= invoice.totalAmount)
            {
                targetStatusName = "Paid";
            }
            else
            {
                targetStatusName = "Partially Paid";
            }

            var targetStatus = await _context.InvoiceStatuses
                .Where(s => s.name == targetStatusName && s.active == true && s.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (targetStatus != null)
            {
                invoice.invoiceStatusId = targetStatus.id;

                if (targetStatusName == "Paid")
                {
                    invoice.paidDate = DateTime.UtcNow;
                }
            }
        }


        /// <summary>
        /// Synchronous version of CascadeInvoiceStatusAsync for use inside lock blocks.
        /// Takes pre-resolved InvoiceStatus objects to avoid async DB calls.
        /// </summary>
        private void CascadeInvoiceStatusSync(
            Invoice invoice,
            decimal totalPaid,
            InvoiceStatus draftStatus,
            InvoiceStatus partiallyPaidStatus,
            InvoiceStatus paidStatus)
        {
            InvoiceStatus targetStatus;

            if (totalPaid <= 0)
            {
                targetStatus = draftStatus;
            }
            else if (totalPaid >= invoice.totalAmount)
            {
                targetStatus = paidStatus;
                invoice.paidDate = DateTime.UtcNow;
            }
            else
            {
                targetStatus = partiallyPaidStatus;
            }

            if (targetStatus != null)
            {
                invoice.invoiceStatusId = targetStatus.id;
            }
        }


        /// <summary>
        /// Cascades all EventCharges on a scheduled event to the given charge status name.
        /// </summary>
        private async Task CascadeChargeStatusAsync(
            Guid tenantGuid,
            int scheduledEventId,
            string statusName,
            CancellationToken cancellationToken)
        {
            var chargeStatus = await _context.ChargeStatuses
                .Where(cs => cs.name == statusName && cs.active == true && cs.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (chargeStatus == null) return;

            var charges = await _context.EventCharges
                .Where(ec => ec.scheduledEventId == scheduledEventId
                          && ec.tenantGuid == tenantGuid
                          && ec.active == true
                          && ec.deleted == false)
                .ToListAsync(cancellationToken);

            foreach (var charge in charges)
            {
                charge.chargeStatusId = chargeStatus.id;
                _context.Entry(charge).State = EntityState.Modified;
            }
        }


        /// <summary>
        /// Finds or creates a FinancialCategory by name for the given tenant.
        /// Used to ensure ledger entries have a valid category.
        /// </summary>
        private async Task<FinancialCategory> GetOrCreateFinancialCategoryAsync(
            Guid tenantGuid,
            string categoryName,
            bool isRevenue,
            CancellationToken cancellationToken)
        {
            var category = await _context.FinancialCategories
                .Where(fc => fc.name == categoryName && fc.tenantGuid == tenantGuid && fc.active == true && fc.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (category != null) return category;

            //
            // Find the appropriate account type
            //
            var accountType = await _context.AccountTypes
                .Where(at => at.name == (isRevenue ? "Income" : "Expense") && at.active == true && at.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (accountType == null)
            {
                _logger.LogWarning("Cannot create FinancialCategory '{Name}' — AccountType '{Type}' not found.",
                    categoryName, isRevenue ? "Income" : "Expense");
                return null;
            }

            category = new FinancialCategory
            {
                tenantGuid = tenantGuid,
                name = categoryName,
                description = $"Auto-created: {categoryName}",
                code = categoryName.Length > 10 ? categoryName.Substring(0, 10) : categoryName,
                accountTypeId = accountType.id,
                isTaxApplicable = false,
                sequence = 999,
                versionNumber = 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.FinancialCategories.Add(category);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auto-created FinancialCategory '{Name}' (id={Id})", categoryName, category.id);

            return category;
        }


        /// <summary>
        /// Generates the next sequential number based on a mask pattern like "INV-{YYYY}-{NNNN}".
        ///
        /// MUST be called inside the appropriate static lock to prevent duplicates.
        /// </summary>
        private string GenerateNextNumber(
            Guid tenantGuid,
            string mask,
            IQueryable<string> existingNumbers)
        {
            int year = DateTime.UtcNow.Year;

            string prefixBeforeSequence = mask
                .Replace("{YYYY}", year.ToString(CultureInfo.InvariantCulture))
                .Replace("{YY}", (year % 100).ToString("D2", CultureInfo.InvariantCulture));

            int seqStart = prefixBeforeSequence.IndexOf("{N", StringComparison.Ordinal);
            string yearPrefix = seqStart >= 0 ? prefixBeforeSequence.Substring(0, seqStart) : prefixBeforeSequence;

            int maxSequence = 0;
            var matchingNumbers = existingNumbers
                .Where(n => n.StartsWith(yearPrefix))
                .ToList();

            foreach (string num in matchingNumbers)
            {
                if (num != null && num.Length > yearPrefix.Length)
                {
                    string trailing = num.Substring(yearPrefix.Length);
                    string digits = new string(trailing.Where(char.IsDigit).ToArray());
                    if (digits.Length > 0 && int.TryParse(digits, out int seq))
                    {
                        if (seq > maxSequence) maxSequence = seq;
                    }
                }
            }

            int nextSequence = maxSequence + 1;

            return mask
                .Replace("{YYYY}", year.ToString(CultureInfo.InvariantCulture))
                .Replace("{YY}", (year % 100).ToString("D2", CultureInfo.InvariantCulture))
                .Replace("{NNNN}", nextSequence.ToString("D4", CultureInfo.InvariantCulture))
                .Replace("{NNN}", nextSequence.ToString("D3", CultureInfo.InvariantCulture))
                .Replace("{NN}", nextSequence.ToString("D2", CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Writes an InvoiceChangeHistory record following the auto-generated pattern.
        /// </summary>
        private void WriteInvoiceChangeHistory(Invoice invoice, int userId, Guid tenantGuid)
        {
            _context.Entry(invoice).State = EntityState.Detached;
            invoice.InvoiceChangeHistories = null;
            invoice.InvoiceLineItems = null;
            invoice.Documents = null;
            invoice.Receipts = null;
            invoice.client = null;
            invoice.contact = null;
            invoice.currency = null;
            invoice.financialOffice = null;
            invoice.invoiceStatus = null;
            invoice.scheduledEvent = null;
            invoice.taxCode = null;

            _context.InvoiceChangeHistories.Add(new InvoiceChangeHistory
            {
                invoiceId = invoice.id,
                versionNumber = invoice.versionNumber,
                timeStamp = DateTime.UtcNow,
                userId = userId,
                tenantGuid = tenantGuid,
                data = JsonSerializer.Serialize(Invoice.CreateAnonymousWithFirstLevelSubObjects(invoice))
            });
        }


        /// <summary>
        /// Writes a ReceiptChangeHistory record following the auto-generated pattern.
        /// </summary>
        private void WriteReceiptChangeHistory(Receipt receipt, int userId, Guid tenantGuid)
        {
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

            _context.ReceiptChangeHistories.Add(new ReceiptChangeHistory
            {
                receiptId = receipt.id,
                versionNumber = receipt.versionNumber,
                timeStamp = DateTime.UtcNow,
                userId = userId,
                tenantGuid = tenantGuid,
                data = JsonSerializer.Serialize(Receipt.CreateAnonymousWithFirstLevelSubObjects(receipt))
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  GenerateFiscalYear
        //
        //  Creates 12 monthly fiscal periods for a given year.
        //  Idempotent — returns existing count if periods already exist.
        //  Validates year range (current year ± 5).
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> GenerateFiscalYearAsync(
            Guid tenantGuid,
            int year,
            CancellationToken cancellationToken = default)
        {
            //
            // Validate year range
            //
            int currentYear = DateTime.UtcNow.Year;
            if (year < currentYear - 5 || year > currentYear + 5)
            {
                return FinancialOperationResult.Fail(
                    $"Year must be between {currentYear - 5} and {currentYear + 5}.");
            }

            //
            // Idempotency: check if periods already exist for this year
            //
            int existingCount = await _context.FiscalPeriods
                .Where(fp => fp.tenantGuid == tenantGuid && fp.fiscalYear == year
                          && fp.active == true && fp.deleted == false)
                .CountAsync(cancellationToken);

            if (existingCount > 0)
            {
                return FinancialOperationResult.Ok(new Dictionary<string, object>
                {
                    ["created"] = 0,
                    ["existing"] = existingCount,
                    ["message"] = $"Fiscal periods for {year} already exist ({existingCount} found)."
                });
            }

            //
            // Resolve the "Open" PeriodStatus from seed data
            //
            var openStatus = await _context.PeriodStatuses
                .Where(ps => ps.name == "Open" && ps.active == true && ps.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (openStatus == null)
            {
                return FinancialOperationResult.Fail(
                    "Period status 'Open' not found. Ensure database seed data has been run.");
            }

            //
            // Create 12 monthly periods
            //
            string[] monthNames = new string[]
            {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            };

            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    for (int month = 1; month <= 12; month++)
                    {
                        DateTime periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                        DateTime periodEnd = periodStart.AddMonths(1).AddSeconds(-1);

                        var fp = new FiscalPeriod
                        {
                            tenantGuid = tenantGuid,
                            name = $"{monthNames[month - 1]} {year}",
                            description = $"{monthNames[month - 1]} {year} fiscal period",
                            startDate = periodStart,
                            endDate = periodEnd,
                            periodType = "Month",
                            fiscalYear = year,
                            periodNumber = month,
                            periodStatusId = openStatus.id,
                            sequence = (year * 100) + month,
                            versionNumber = 0,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        };

                        _context.FiscalPeriods.Add(fp);
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Generated 12 fiscal periods for year {Year}, tenant {TenantGuid}.",
                        year, tenantGuid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate fiscal periods for year {Year}", year);
                    return FinancialOperationResult.Fail($"Failed to generate fiscal periods: {ex.Message}");
                }
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["created"] = 12,
                ["existing"] = 0,
                ["year"] = year
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  UpdateTransaction
        //
        //  Validates:
        //    - Transaction exists and belongs to tenant
        //    - Fiscal period not closed (both original and new if date changed)
        //    - Category exists
        //    - Writes FinancialTransactionChangeHistory with snapshot
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> UpdateTransactionAsync(
            Guid tenantGuid,
            int transactionId,
            int userId,
            int financialCategoryId,
            DateTime transactionDate,
            decimal amount,
            decimal taxAmount,
            string description,
            int currencyId,
            int? financialOfficeId = null,
            int? scheduledEventId = null,
            int? contactId = null,
            int? clientId = null,
            string referenceNumber = null,
            string notes = null,
            CancellationToken cancellationToken = default)
        {
            //
            // 1. Load existing transaction
            //
            var ft = await _context.FinancialTransactions
                .Where(t => t.id == transactionId && t.tenantGuid == tenantGuid
                         && t.active == true && t.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (ft == null)
            {
                return FinancialOperationResult.Fail("Transaction not found.");
            }

            //
            // 2. Validate fiscal period for the ORIGINAL transaction date (must be open)
            //
            var originalPeriodCheck = await ValidateFiscalPeriodOpenAsync(tenantGuid, ft.transactionDate, cancellationToken);
            if (!originalPeriodCheck.Success)
            {
                return FinancialOperationResult.Fail(
                    $"Cannot modify — the original fiscal period is closed. {originalPeriodCheck.ErrorMessage}");
            }

            //
            // 3. If date changed, also validate the NEW fiscal period
            //
            int? newFiscalPeriodId = null;
            if (transactionDate.Date != ft.transactionDate.Date)
            {
                var newPeriodCheck = await ValidateFiscalPeriodOpenAsync(tenantGuid, transactionDate, cancellationToken);
                if (!newPeriodCheck.Success)
                {
                    return FinancialOperationResult.Fail(
                        $"Cannot move transaction to a closed fiscal period. {newPeriodCheck.ErrorMessage}");
                }
                newFiscalPeriodId = (int?)newPeriodCheck.Data.GetValueOrDefault("fiscalPeriodId");
            }

            //
            // 4. Validate category
            //
            var category = await _context.FinancialCategories
                .Where(fc => fc.id == financialCategoryId && fc.tenantGuid == tenantGuid
                          && fc.active == true && fc.deleted == false)
                .Include(fc => fc.accountType)
                .FirstOrDefaultAsync(cancellationToken);

            if (category == null)
            {
                return FinancialOperationResult.Fail("Financial category not found.");
            }

            //
            // 5. Clone BEFORE state for audit diff
            //
            var beforeClone = ft.Clone();

            //
            // 6. Apply updates
            //
            ft.financialCategoryId = financialCategoryId;
            ft.transactionDate = transactionDate;
            ft.amount = amount;
            ft.taxAmount = taxAmount;
            ft.totalAmount = amount + taxAmount;
            ft.description = description?.Length > 500 ? description.Substring(0, 500) : description;
            ft.currencyId = currencyId;
            ft.financialOfficeId = financialOfficeId;
            ft.scheduledEventId = scheduledEventId;
            ft.contactId = contactId;
            ft.clientId = clientId;
            ft.referenceNumber = referenceNumber;
            ft.notes = notes;
            ft.isRevenue = category.accountType?.isRevenue ?? ft.isRevenue;
            ft.journalEntryType = (category.accountType?.isRevenue ?? false) ? "Credit" : "Debit";

            if (newFiscalPeriodId.HasValue)
            {
                ft.fiscalPeriodId = newFiscalPeriodId.Value;
            }

            ft.versionNumber++;

            // Null out nav properties to prevent EF tracking conflicts
            ft.financialCategory = null;
            ft.financialOffice = null;
            ft.scheduledEvent = null;
            ft.contact = null;
            ft.client = null;
            ft.currency = null;
            ft.taxCode = null;
            ft.fiscalPeriod = null;
            ft.paymentType = null;

            //
            // 7. Write structured audit change history with field-level diffs
            //
            _context.FinancialTransactionChangeHistories.Add(new FinancialTransactionChangeHistory
            {
                financialTransactionId = ft.id,
                versionNumber = ft.versionNumber,
                timeStamp = DateTime.UtcNow,
                userId = userId,
                tenantGuid = tenantGuid,
                data = BuildAuditJson("Updated", beforeClone, ft)
            });

            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Transaction {Id} updated by user {UserId}. Version: {Version}.",
                        ft.id, userId, ft.versionNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update transaction {Id}", transactionId);
                    return FinancialOperationResult.Fail($"Update failed: {ex.Message}");
                }
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["financialTransactionId"] = ft.id,
                ["versionNumber"] = ft.versionNumber
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  VoidTransaction
        //
        //  Replaces soft-delete with an auditable void-with-reason.
        //    - Rejects if fiscal period is closed
        //    - Records void reason in change history
        //    - Sets deleted = true (existing soft-delete pattern)
        //    - Financial transactions should never be truly deleted
        //
        // ────────────────────────────────────────────────────────────────────────

        public async Task<FinancialOperationResult> VoidTransactionAsync(
            Guid tenantGuid,
            int transactionId,
            int userId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return FinancialOperationResult.Fail("A reason is required to void a transaction.");
            }

            //
            // 1. Load existing transaction
            //
            var ft = await _context.FinancialTransactions
                .Where(t => t.id == transactionId && t.tenantGuid == tenantGuid
                         && t.active == true && t.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (ft == null)
            {
                return FinancialOperationResult.Fail("Transaction not found or already voided.");
            }

            //
            // 2. Validate fiscal period is open
            //
            var periodCheck = await ValidateFiscalPeriodOpenAsync(tenantGuid, ft.transactionDate, cancellationToken);
            if (!periodCheck.Success)
            {
                return FinancialOperationResult.Fail(
                    $"Cannot void — the fiscal period is closed. {periodCheck.ErrorMessage}");
            }

            //
            // 3. Clone BEFORE state for audit diff
            //
            var beforeClone = ft.Clone();

            //
            // 4. Mark as voided
            //
            ft.deleted = true;
            ft.notes = string.IsNullOrWhiteSpace(ft.notes)
                ? $"VOIDED: {reason}"
                : $"{ft.notes}\n\nVOIDED: {reason}";
            ft.versionNumber++;

            // Null out nav properties
            ft.financialCategory = null;
            ft.financialOffice = null;
            ft.scheduledEvent = null;
            ft.contact = null;
            ft.client = null;
            ft.currency = null;
            ft.taxCode = null;
            ft.fiscalPeriod = null;
            ft.paymentType = null;

            //
            // 5. Write structured audit change history with void reason
            //
            _context.FinancialTransactionChangeHistories.Add(new FinancialTransactionChangeHistory
            {
                financialTransactionId = ft.id,
                versionNumber = ft.versionNumber,
                timeStamp = DateTime.UtcNow,
                userId = userId,
                tenantGuid = tenantGuid,
                data = BuildAuditJson("Voided", beforeClone, ft, reason)
            });

            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);

                    // Post reversal to General Ledger
                    await PostReversalToGLAsync(tenantGuid, ft.id, userId, reason, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Transaction {Id} voided by user {UserId}. Reason: {Reason}",
                        ft.id, userId, reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to void transaction {Id}", transactionId);
                    return FinancialOperationResult.Fail($"Void failed: {ex.Message}");
                }
            }

            return FinancialOperationResult.Ok(new Dictionary<string, object>
            {
                ["financialTransactionId"] = ft.id,
                ["voided"] = true
            });
        }
    }
}
