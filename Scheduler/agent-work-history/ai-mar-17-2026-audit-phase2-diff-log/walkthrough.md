# Phase 2 — Explicit Audit Diff Log

## Summary

Every financial mutation now produces structured, human-readable field-level diffs stored in `FinancialTransactionChangeHistory.data`. A new Audit Log Viewer component displays these diffs with expandable rows.

## What Changed

### Backend Service — [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

**`BuildAuditJson(action, before, after, reason)`** — Private helper that:
- Compares before/after transaction states field-by-field
- Generates JSON: `{ action, fieldChanges: [{field, oldValue, newValue}], reason, transactionId, timestamp }`
- Handles create (all fields new), update (changed fields only), and void (deleted flag + reason)

**Integration into mutation methods:**

| Method | Action | Diff Type |
|--------|--------|-----------|
| `RecordExpenseAsync` | Created | All fields as new values |
| `RecordDirectRevenueAsync` | Created | All fields as new values |
| `UpdateTransactionAsync` | Updated | Before vs after clone (only changed fields) |
| `VoidTransactionAsync` | Voided | Before vs after + reason string |

**`GetAuditLogAsync()`** — Reads change history entries, parses structured JSON, resolves user names via `ChangeHistoryMultiTenant.GetChangeHistoryUserAsync()`, and returns clean audit objects. Handles legacy snapshot entries gracefully.

### Backend Controller — [FinancialTransactionsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)

| Endpoint | Description |
|----------|-------------|
| `GET /api/FinancialTransactions/AuditLog` | Returns structured audit entries. Supports `?transactionId=`, `?fromDate=`, `?toDate=`, `?maxResults=` |

### Frontend — [AuditLogViewerComponent](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/audit-log-viewer/audit-log-viewer.component.ts)

| Feature | Detail |
|---------|--------|
| Route | `/finances/audit-log` |
| Filter bar | Transaction ID, Action (Created/Updated/Voided), User name, Date range |
| Expandable rows | Click to expand field-level before/after diff table |
| Action badges | Color-coded: green (Created), blue (Updated), red (Voided) |
| Value formatting | Dates, currencies, boolean types all human-readable |
| Header | Deep blue/slate gradient, consistent with financial dashboard |

### Module Wiring

| File | Change |
|------|--------|
| [app.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts) | Import + declaration |
| [app-routing.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app-routing.module.ts) | Import + route |

## Verification

| Check | Result |
|-------|--------|
| `dotnet build` | ✅ 0 errors |
| `ng build` | ✅ No new errors |
