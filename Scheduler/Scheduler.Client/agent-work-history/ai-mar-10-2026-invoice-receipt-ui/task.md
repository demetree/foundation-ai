# PHMC P0 — Invoice & Receipt Generation

## Planning
- [x] Research code generation workflow and entity patterns
- [x] Research Foundation SimplePdf capabilities  
- [x] Verify existing financial entities in generator (all 16+ confirmed present)
- [x] Write implementation plan (v4)
- [x] Get user approval on implementation plan

## Phase 1 — New Entity Definitions in Code Generator
- [x] Add InvoiceStatus, Invoice, InvoiceLineItem, Receipt tables
- [x] Add invoiceId/receiptId FKs to Document table
- [x] Add invoiceNumberMask + receiptNumberMask to TenantProfile
- [x] Build verification (SchedulerDatabaseGenerator + SchedulerTools)

## Phase 2 — Code Generation Pipeline ✅ (completed by user)
- [x] Run SchedulerTools, apply SQL, EF Core Power Tools, regenerate code
- [x] Document table moved after Invoice/Receipt for inline FK definitions

## Phase 3 — Custom Server Controllers & PDF Services
- [x] Create InvoicePdfService + ReceiptPdfService (SimplePdf)
- [x] Create InvoicesController + ReceiptsController (partial class extensions)
- [x] Add Foundation.Imaging project reference
- [x] Fix QuickBooks Invoice namespace clash (QbInvoice alias)
- [x] Fix AuditType enum values (CreateEntity, ReadEntity)
- [x] Build verification — Scheduler.Server builds successfully ✅

## Phase 4 — Client UI Components ✅
- [x] Create InvoiceHelperService / ReceiptHelperService
- [x] Build invoice-custom-listing (TS/HTML/SCSS)
- [x] Build receipt-custom-listing (TS/HTML/SCSS)
- [x] Build invoice-custom-detail (TS/HTML/SCSS)
- [x] Build invoice-custom-add-edit (TS/HTML/SCSS)
- [x] Build receipt-custom-detail (TS/HTML/SCSS)
- [x] Build receipt-custom-add-edit (TS/HTML/SCSS)
- [x] Register all components in app.module.ts
- [x] Add routes in app-routing.module.ts

## Verification ✅
- [x] Client build — zero errors from new files
- [x] Walkthrough documentation
