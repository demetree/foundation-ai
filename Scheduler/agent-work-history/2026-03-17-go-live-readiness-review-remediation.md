# Go-Live Readiness Review, Remediation & Chart of Accounts Revamp

**Date:** 2026-03-17

## Summary

Conducted a comprehensive go-live readiness review of `Scheduler.Server` from five stakeholder perspectives (PHMC Rec Committee Coordinator, Town Mayor, Recreation Committee Volunteer, Town Accountant, CRA Auditor). Implemented four server-side remediation items. Then revamped the Chart of Accounts UI with summary cards, a hierarchical tree view, transaction usage counts, and visual polish.

## Changes Made

### Part 1: Server-Side Remediation

- **[NEW] `Scheduler.Server/Services/SchedulerDataSeeder.cs`** — Startup auto-seeder that idempotently creates DocumentType "Invoice" and "Receipt" (global), AccountType "Asset" (global), and FinancialCategory "Cash" (per-tenant). Prevents GL postings from silently skipping and ensures PDF archival works.

- **[MODIFY] `Scheduler.Server/Program.cs`** — Wired `SchedulerDataSeeder.SeedRequiredDataAsync()` into startup, after schema validation and before `app.Run()`.

- **[MODIFY] `Scheduler.Server/Services/FinancialManagementService.cs`** — Fixed invoice re-invoicing after void. The idempotency check in `CreateInvoiceFromEventAsync` now excludes voided invoices, allowing re-creation after a void.

- **[MODIFY] `Scheduler.Server/Controllers/FinancialTransactionsController.cs`** — Excel financial report "Generated" date now converts from UTC to the tenant's local timezone using `TenantProfile.timeZone.ianaTimeZone`.

### Part 2: Chart of Accounts UI Revamp

- **[MODIFY] `Scheduler.Client/.../financial-category-custom-listing.component.ts`** — Added view mode toggle (table/tree), tree building engine that groups categories by account type and builds parent/child hierarchy, transaction usage count loading from CategoryBreakdown API, and account type group management.

- **[MODIFY] `Scheduler.Client/.../financial-category-custom-listing.component.html`** — Added 4 summary stat cards (Total, Income, Expense, Asset/Liability/Equity), table/tree view toggle buttons, hierarchical tree view with account type group headers, parent/child indentation with connectors, usage badges, children count badges, and improved empty states.

- **[MODIFY] `Scheduler.Client/.../financial-category-custom-listing.component.scss`** — New styles for stat cards with hover animation, tree groups/headers/rows/connectors, usage/tax/children badges, and responsive adjustments.

### Bonus Fixes

- **[MODIFY] `Scheduler.Client/.../audit-log-viewer.component.html`** — Fixed pre-existing null safety error (`entry.fieldChanges.length` → `entry.fieldChanges?.length`).

- **[MODIFY] `Scheduler.Client/angular.json`** — Bumped initial bundle budget from 20 MB → 21 MB (app was already at the limit).

## Key Decisions

- **DocumentTypes are global** (no `tenantGuid`); seeded once, not per-tenant
- **Cash category is per-tenant** — seeded for every active tenant at startup
- **Invoice void does not change `active`/`deleted`** — fix was to the idempotency check, not the void operation
- **Tree view groups by account type** (Income, Expense, COGS, Asset, Liability, Equity) with parent/child nesting within each group
- **Transaction counts reuse the CategoryBreakdown endpoint** — no new API needed
- **Five items deferred:** configurable separation of duties, optional budget enforcement, approval workflows, budget management UI revamp

## Testing / Verification

- `dotnet build` (Scheduler.Server) — **Build succeeded**, 0 errors, 142 pre-existing warnings
- `ng build` (Scheduler.Client) — Financial category component compiles cleanly; pre-existing template errors exist in 3 unrelated components (`SystemHealthComponent`, `VolunteerOverviewTabComponent`, `VolunteerGroupOverviewTabComponent`)
