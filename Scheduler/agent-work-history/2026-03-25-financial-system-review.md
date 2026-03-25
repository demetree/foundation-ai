# Scheduler Financial System — Critical Review

**Date:** 2026-03-25

## Summary

Conducted a comprehensive review of the scheduler's financial subsystem in preparation for the PHMC recreation committee coordinator. Analyzed all financial entity models, server controllers, client services, routing, and custom UI components to assess readiness and identify UX gaps.

## Changes Made

- No code changes in this session — review and analysis only
- Created review document at `brain/scheduler_financial_review.md`

## Key Findings

### What's Working Well
- Invoice detail view (full lifecycle: void, refund, record payment, PDF, line items)
- Receipt and payment detail views with linked entity navigation
- Invoice listing with status filtering, search, sort, and mobile card view
- Financial dashboard with revenue/expense summary and monthly chart
- PHMC-specific Excel financial report export (Summary, Income, Expenses sheets)
- Event financial timeline (chronological merge of charges + transactions)
- Deposit management with refund workflow
- Server-side routing through `FinancialManagementService` with fiscal validation & audit

### Critical UX Gaps Identified

1. **No charge creation from event editor** — Financials tab is read-only, no "Add Charge" capability
2. **"Generate Invoice" has no UI trigger** — `POST /api/Invoices/CreateFromEvent/{eventId}` and `InvoiceHelperService.createFromEvent()` exist but no button calls them
3. **Financials tab hidden behind Advanced mode** — coordinator won't find it in Simple mode
4. **Invoice form has no line-item editor** — header-level fields only
5. **"Record Payment" flow is broken** — invoice detail navigates to `/finances/receipts/new?invoiceId=X` but receipt component doesn't consume the query param
6. **Currency display inconsistency** — hardcoded CAD in invoice listing, USD in event financials, raw `$` in detail views

## Key Decisions

- No schema changes needed — the data model is solid with proper multi-tenancy, soft-delete, and audit support
- All event charge writes must route through `FinancialManagementService` (not direct CRUD) per existing architecture
- Improvements prioritized as P0 (launch-blocking), P1 (should have), P2 (nice to have)

## Entity Architecture Reviewed

- `EventCharge` → linked to `ScheduledEvent`, `ChargeType`, `ChargeStatus`, `Resource`, `Currency`, `TaxCode`
- `InvoiceLineItem` → bridges `EventCharge` → `Invoice`
- `Receipt` → linked to `Invoice`, `PaymentTransaction`, `Client`, `Contact`
- `PaymentTransaction` → linked to `EventCharge`, `PaymentMethod`, `PaymentProvider`, `ScheduledEvent`
- `FinancialTransaction` → general ledger entries with category, office, event links

## Files Reviewed

- `SchedulerDatabase/Database/EventCharge.cs`, `Invoice.cs`, `InvoiceLineItem.cs`, `Receipt.cs`, `PaymentTransaction.cs`, `ScheduledEvent.cs`
- `Scheduler.Server/Controllers/EventChargesControllerOverrides.cs`, `FinancialTransactionsController.cs`
- `Scheduler.Client/src/app/components/invoice-custom/` (detail, listing, add-edit)
- `Scheduler.Client/src/app/components/receipt-custom/` (detail, listing, add-edit)
- `Scheduler.Client/src/app/components/payment-custom/` (detail, listing, add-edit)
- `Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/`
- `Scheduler.Client/src/app/services/invoice-helper.service.ts`, `receipt-helper.service.ts`
- `Scheduler.Client/src/app/app-routing.module.ts` (23 finance routes)

## Testing / Verification

- Review-only session — no code changes to verify
- Verified API endpoints exist for all recommended features (createFromEvent, createFromPayment, createFromInvoicePayment)
- Confirmed data model supports all identified improvements without schema changes
