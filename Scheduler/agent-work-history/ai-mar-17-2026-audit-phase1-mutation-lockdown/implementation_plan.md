# Financial Audit Gap Remediation Plan

## Goal

Make the financial system trustworthy to an external auditor who trusts neither the software nor the organization. Five phases, each independently deployable.

---

## Phase 1 — Lock Down All Mutations

**Goal:** Every financial transaction create/edit/delete goes through `FinancialManagementService` with full validation.

### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

Add three new methods:

- **`UpdateTransactionAsync()`** — Validates:
  - Fiscal period not closed (both original and new period if changed)
  - Category exists and matches `isRevenue` flag
  - Amount change is recorded with reason
  - Writes `FinancialTransactionChangeHistory` with diff metadata
  
- **`VoidTransactionAsync(reason)`** — Replaces soft-delete:
  - Sets `deleted = true`, records void reason and voided-by user
  - Rejects if fiscal period is closed
  - Writes change history with void reason
  
- **`DeleteTransactionAsync()`** — Hard block:
  - Returns error. Financial transactions should never be truly deleted.

### [MODIFY] [FinancialTransactionsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)

- Add `PUT /api/FinancialTransactions/{id}/Update` → `UpdateTransactionAsync()`
- Add `POST /api/FinancialTransactions/{id}/Void` → `VoidTransactionAsync()`
- Consider: override generated PUT/DELETE to return 403 with migration message

### [MODIFY] [financial-transaction-custom-add-edit.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-add-edit/financial-transaction-custom-add-edit.component.ts)

- Route edits through `/Update` endpoint instead of standard PUT
- Route deletes through `/Void` endpoint with reason prompt

### [NEW] Void reason UI

- Modal dialog prompting for void reason before deletion
- Void reason stored in change history

---

## Phase 2 — Explicit Audit Diff Log

**Goal:** Every mutation produces a human-readable diff, not just a snapshot.

### [NEW] `FinancialAuditEntry` entity

```
Table: FinancialAuditEntries
- id, tenantGuid, transactionId
- action: 'Created' | 'Updated' | 'Voided'
- userId, userName, timestamp
- fieldChanges: JSON — [{ field, oldValue, newValue }]
- reason: string (for voids, corrections)
- ipAddress (optional, from HttpContext)
```

### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

- All create/update/void methods write a `FinancialAuditEntry`
- On create: fieldChanges = all fields with oldValue = null
- On update: fieldChanges = only changed fields with before/after
- On void: fieldChanges = [{field: "status", old: "active", new: "voided"}] + reason

### [NEW] Audit Log Viewer component

- Filterable by date range, user, action type, transaction
- Shows field-level diffs in a clean table
- Exportable to CSV/PDF for external auditors

---

## Phase 3 — Double-Entry General Ledger

**Goal:** Every financial event produces balanced debit/credit pairs.

### [NEW] `GeneralLedgerEntry` entity

```
Table: GeneralLedgerEntries
- id, tenantGuid, transactionDate
- journalEntryNumber (auto-increment per tenant)
- description, referenceNumber
- fiscalPeriodId, financialOfficeId
- postedBy (userId), postedDate
- reversalOfId (nullable — for correction entries)
```

### [NEW] `GeneralLedgerLine` entity

```
Table: GeneralLedgerLines
- id, generalLedgerEntryId
- financialCategoryId (account)
- debitAmount, creditAmount (one must be 0)
- description
```

**Constraint:** Sum of debits must equal sum of credits per `GeneralLedgerEntry`.

### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

Every financial operation now also creates GL entries:

| Operation | Debit Account | Credit Account |
|-----------|--------------|----------------|
| Record Expense | Expense category | Cash/Bank |
| Record Revenue | Cash/Bank | Revenue category |
| Invoice Created | Accounts Receivable | Revenue category |
| Payment Received | Cash/Bank | Accounts Receivable |
| Void/Refund | Reversal entry (opposite of original) | — |

### [MODIFY] Trial balance report

- Rewrite to read from `GeneralLedgerLines` instead of summing `FinancialTransaction.totalAmount`
- True double-entry balance check: sum(debits) == sum(credits) across ALL entries

### [NEW] Chart of Accounts enhancement

- Add account types: Asset, Liability, Equity, Revenue, Expense
- Add a default "Cash/Bank" account (asset type) per office
- Add "Accounts Receivable" account (asset type)

---

## Phase 4 — Bank Reconciliation

**Goal:** Match recorded transactions against external bank data.

### [NEW] `BankStatement` / `BankStatementLine` entities

```
BankStatements: id, tenantGuid, bankName, accountNumber, 
                statementDate, importedDate, importedBy
BankStatementLines: id, bankStatementId, transactionDate,
                    description, amount, referenceNumber,
                    matchedTransactionId (nullable), matchStatus
```

### [NEW] Bank reconciliation UI

- Import bank CSV (configurable column mapping)
- Auto-match by amount + date ± tolerance
- Manual match/unmatch interface
- Reconciliation report showing matched, unmatched bank items, unmatched system items
- Reconciliation sign-off (recorded with user + timestamp)

### [NEW] `POST /api/BankStatements/Import` endpoint

---

## Phase 5 — Approval Workflows

**Goal:** Material entries require a second person to approve before posting.

### [NEW] `ApprovalRule` entity

```
ApprovalRules: id, tenantGuid, name,
               thresholdAmount, appliesToRevenue, appliesToExpense,
               requiredApproverRoleId, active
```

### [NEW] `PendingApproval` entity

```
PendingApprovals: id, tenantGuid, 
                  transactionId, submittedBy, submittedDate,
                  approverUserId (nullable), approvalDate (nullable),
                  status: 'Pending' | 'Approved' | 'Rejected',
                  rejectionReason
```

### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

- On create/update: check if amount exceeds any `ApprovalRule` threshold
- If yes: create transaction in "pending" state + `PendingApproval` record
- Transaction doesn't appear in reports until approved

### [NEW] Approval Queue UI

- Dashboard showing pending items
- Approve/reject with comment
- Email notification to approvers (optional, via existing alerting infra)

---

## Implementation Order & Effort

| Phase | Effort | Audit Impact | Dependencies |
|-------|--------|-------------|-------------|
| **Phase 1** — Lock mutations | ~1 session | 🔴 Critical | None |
| **Phase 2** — Audit diffs | ~1 session | 🔴 Critical | Phase 1 |
| **Phase 3** — Double-entry GL | ~2-3 sessions | 🟡 High | Phase 1 |
| **Phase 4** — Bank reconciliation | ~2 sessions | 🟡 High | Phase 3 |
| **Phase 5** — Approval workflows | ~1-2 sessions | 🟢 Medium | Phase 1 |

> [!IMPORTANT]
> **Phase 1 is the foundation.** Everything else depends on all mutations flowing through `FinancialManagementService`. This should be done first.

> [!NOTE]
> **Phase 3 (Double-Entry GL) is the biggest architectural change.** It adds new database tables and fundamentally changes how reporting works. The existing `FinancialTransaction` table remains as the source document, but the GL becomes the authoritative ledger for reporting.
