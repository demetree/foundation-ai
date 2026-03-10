# Phase 4 Walkthrough — Invoice & Receipt Client UI

## Summary

Created 20 new files implementing the full invoice and receipt client-side UI for the PHMC Scheduler.

---

## New Files

### Helper Services

| Service | File |
|---------|------|
| `InvoiceHelperService` | [invoice-helper.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/invoice-helper.service.ts) |
| `ReceiptHelperService` | [receipt-helper.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/receipt-helper.service.ts) |

Both extend `SecureEndpointBase` with `getAuthHeaders()` using `accessToken`.

---

### Invoice Components

| Component | TS | HTML | SCSS |
|-----------|----|----|------|
| Listing | [TS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-listing/invoice-custom-listing.component.ts) | [HTML](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-listing/invoice-custom-listing.component.html) | [SCSS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-listing/invoice-custom-listing.component.scss) |
| Detail | [TS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-detail/invoice-custom-detail.component.ts) | [HTML](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-detail/invoice-custom-detail.component.html) | [SCSS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-detail/invoice-custom-detail.component.scss) |
| Add/Edit | [TS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-add-edit/invoice-custom-add-edit.component.ts) | [HTML](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-add-edit/invoice-custom-add-edit.component.html) | [SCSS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-add-edit/invoice-custom-add-edit.component.scss) |

### Receipt Components

| Component | TS | HTML | SCSS |
|-----------|----|----|------|
| Listing | [TS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-listing/receipt-custom-listing.component.ts) | [HTML](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-listing/receipt-custom-listing.component.html) | [SCSS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-listing/receipt-custom-listing.component.scss) |
| Detail | [TS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-detail/receipt-custom-detail.component.ts) | [HTML](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-detail/receipt-custom-detail.component.html) | [SCSS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-detail/receipt-custom-detail.component.scss) |
| Add/Edit | [TS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-add-edit/receipt-custom-add-edit.component.ts) | [HTML](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-add-edit/receipt-custom-add-edit.component.html) | [SCSS](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-add-edit/receipt-custom-add-edit.component.scss) |

---

## Routes Added

| Path | Component | Title |
|------|-----------|-------|
| `/finances/invoices` | `InvoiceCustomListingComponent` | Invoices |
| `/finances/invoices/new` | `InvoiceCustomDetailComponent` | New Invoice |
| `/finances/invoices/:invoiceId` | `InvoiceCustomDetailComponent` | Invoice Detail |
| `/finances/receipts` | `ReceiptCustomListingComponent` | Receipts |
| `/finances/receipts/new` | `ReceiptCustomDetailComponent` | New Receipt |
| `/finances/receipts/:receiptId` | `ReceiptCustomDetailComponent` | Receipt Detail |

---

## Key Features

**Invoice Listing** — Blue gradient header, status filter pills, text search, sortable columns, status badges, PDF download, mobile card view

**Invoice Detail** — Summary cards (client, dates, balance due), tabbed view (line items table with subtotal/tax/total/paid/balance + info tab), edit modal, PDF download

**Invoice Add/Edit Modal** — Sectioned form (invoice info, dates, amounts with auto-computed total, notes), status/client/currency/tax code lookups

**Receipt Listing** — Emerald gradient header, text search, sortable columns, PDF download, mobile card view

**Receipt Detail** — Hero amount display, info grid with linked invoice navigation, edit modal, PDF download

**Receipt Add/Edit Modal** — Sectioned form (receipt info, date & amount, description & notes), receipt type/client/currency lookups

---

## Build Verification

✅ Zero errors from new files. Pre-existing errors in `VolunteerGroupOverviewTabComponent`, `SystemHealthComponent`, `ShiftPatternCustomDetailComponent` are unrelated.
