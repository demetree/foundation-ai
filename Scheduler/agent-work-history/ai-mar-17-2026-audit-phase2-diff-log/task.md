# Phase 2 — Explicit Audit Diff Log

## Backend
- [x] Create structured audit entry writer (`BuildAuditJson`) in `FinancialManagementService`
- [x] Integrate with RecordExpense / RecordRevenue (action: Created)
- [x] Integrate with UpdateTransactionAsync (action: Updated, field diffs)
- [x] Integrate with VoidTransactionAsync (action: Voided, with reason)
- [x] Add `GetAuditLogAsync()` method with user name resolution
- [x] Add `GET /api/FinancialTransactions/AuditLog` endpoint

## Frontend
- [x] Create Audit Log Viewer component (TS + HTML + SCSS)
- [x] Wire up route `/finances/audit-log` and module registration

## Verification
- [x] `dotnet build` — 0 errors
- [x] `ng build` — no new errors
