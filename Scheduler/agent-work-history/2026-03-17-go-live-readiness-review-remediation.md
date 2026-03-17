# Go-Live Readiness Review & Remediation

**Date:** 2026-03-17

## Summary

Conducted a comprehensive go-live readiness review of `Scheduler.Server` from five stakeholder perspectives (PHMC Rec Committee Coordinator, Town Mayor, Recreation Committee Volunteer, Town Accountant, CRA Auditor). Identified strengths, gaps, and risks. Implemented four immediate remediation items based on user feedback.

## Changes Made

- **[NEW] `Scheduler.Server/Services/SchedulerDataSeeder.cs`** — Startup auto-seeder that idempotently creates DocumentType "Invoice" and "Receipt" records (global), AccountType "Asset" (global), and FinancialCategory "Cash" (per-tenant). Prevents GL postings from silently skipping and ensures PDF archival works.

- **[MODIFY] `Scheduler.Server/Program.cs`** — Wired `SchedulerDataSeeder.SeedRequiredDataAsync()` into startup, after schema validation and before `app.Run()`.

- **[MODIFY] `Scheduler.Server/Services/FinancialManagementService.cs`** — Fixed invoice re-invoicing after void. The idempotency check in `CreateInvoiceFromEventAsync` now excludes voided invoices, allowing re-creation after a void.

- **[MODIFY] `Scheduler.Server/Controllers/FinancialTransactionsController.cs`** — Excel financial report "Generated" date now converts from UTC to the tenant's local timezone using `TenantProfile.timeZone.ianaTimeZone`.

## Key Decisions

- **DocumentTypes are global** (no `tenantGuid`), so "Invoice" and "Receipt" are seeded once, not per-tenant
- **Cash category is per-tenant** — seeded for every active tenant at startup
- **Invoice void does not change `active`/`deleted`** — this is by design (void-not-delete). The fix was to the idempotency check query, not the void operation itself.
- **Timezone conversion uses IANA IDs** with `TimeZoneInfo.FindSystemTimeZoneById`, falling back to UTC gracefully
- **Five items deferred to future sessions:** configurable separation of duties, optional budget enforcement, configurable approval workflow, chart of accounts UI revamp, budget management UI revamp

## Testing / Verification

- `dotnet build` — **Build succeeded** with 0 errors and 142 pre-existing warnings
- All changes are server-side C# only — no client changes needed
