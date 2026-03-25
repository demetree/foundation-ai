# Financial System UX Improvements for PHMC Readiness

**Date:** 2026-03-25

## Summary

Implemented 8 of 10 planned financial UX improvements across P0/P1/P2 tiers to ensure the Scheduler system is ready for the PHMC recreation committee coordinator. The work was preceded by a holistic financial system review that identified UX gaps in event charges, invoices, receipts, and payment workflows.

## Changes Made

### event-add-edit-modal.component.ts
- Added imports: `ChargeTypeService`, `ChargeStatusService`, `CurrencyService`, `InvoiceHelperService`, `InvoiceService`, `Router`
- Added state properties for: inline Add Charge form, Generate Invoice, linked invoices, charge lookups
- Added methods: `openAddCharge()`, `cancelAddCharge()`, `resetNewCharge()`, `submitNewCharge()` — POSTs to `api/financial/charges`
- Added methods: `generateInvoice()` — uses `InvoiceHelperService.createFromEvent()`
- Added methods: `viewInvoice()`, `updateChargeStatus()`, `deleteCharge()`
- Modified `loadEventFinancials()` to also load invoices and charge form lookups (lazy, first-visit only)

### event-add-edit-modal.component.html
- **P0-1**: Inline "Add Charge" form with fields for type, description, qty, unit price, status, notes, deposit toggle
- **P0-2**: "Generate Invoice" button with success feedback and link to view generated invoice
- **P0-3**: Removed `!isSimpleMode` condition on Financials tab (always visible for existing events)
- **P1-7**: Changed 6 instances of hardcoded `'USD'` to `'CAD'` in currency pipes
- **P2-8**: Added Linked Invoices section showing invoice number, date, status badge, and total with navigation
- **P2-9**: Replaced static charge status display with inline dropdown; added per-row delete button
- Added empty-state UX when no charges exist ("Add First Charge" CTA)
- Added charge count badge to section header

### receipt-custom-detail.component.ts
- **P0-4**: Added `InvoiceService` import and `AfterViewInit` lifecycle hook
- Added `isNewReceiptMode`, `pendingInvoiceId` state
- Added `openPreSeededModal()` — loads invoice via `InvoiceService.GetInvoice()`, calculates outstanding balance (`totalAmount - amountPaid`), and opens add-edit modal pre-seeded with invoice client, amount, description
- Added `queryParamMap` subscription in `ngOnInit()` to handle `invoiceId` query parameter from Record Payment flow

### payment-custom-detail.component.ts
- **P1-6**: Added `ReceiptHelperService` import and `createReceipt()` method using `createFromPayment()` API
- Navigates to receipt detail on success

### payment-custom-detail.component.html
- **P1-6**: Added "Create Receipt" button (visible for completed payments) with spinner state

## Key Decisions

- **All charge writes routed through `api/financial/charges`** (FinancialManagementService) — not the code-generated EventCharge controller — for audit logging and fiscal validation
- **Charge lookups lazily loaded** on first Financials tab visit and cached — avoids unnecessary API calls for events where the user doesn't visit the Financials tab
- **Outstanding balance calculated client-side** from `totalAmount - amountPaid` for receipt pre-seeding
- **Deferred P1-5** (invoice line-item editor) — larger feature requiring significant form refactoring
- **Deferred P2-10** (batch invoice generation) — not critical for initial PHMC launch

## Testing / Verification

- Built successfully with `ng build --configuration=development` (0 TS errors, 62.9 seconds)
- Fixed `Number()` template error (not available in Angular templates) → used `bigint | number` parameter type with `Number()` conversion in method
- Fixed `paidAmount` → `amountPaid` property name to match `InvoiceData` schema
