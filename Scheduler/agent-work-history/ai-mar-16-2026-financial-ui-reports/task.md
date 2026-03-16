# Financial UI Enhancements

## Phase 1 — Invoice Action Bar
- [x] Add Void/Refund/Record Payment endpoints to InvoicesController
- [x] Add action dropdown to invoice-custom-detail HTML
- [x] Add voidInvoice(), issueRefund(), recordPayment() to TS
- [x] Add SCSS for action buttons
- [x] Add voidInvoice/issueRefund to InvoiceHelperService

## Phase 2 — A/R Aging Report
- [x] Create ar-aging-report Angular component (TS, HTML, SCSS)
- [x] Register route and module declaration
- [x] Add to dashboard Reports & Tools

## Phase 3 — Revenue by Client Report
- [x] Create revenue-by-client-report Angular component
- [x] Register route and module declaration
- [x] Add to dashboard Reports & Tools

## Phase 4 — Gift Entry Form
- [x] Create RecordGift API endpoint on GiftsController (routes through FinancialManagementService)
- [x] Create gift-entry Angular component (constituent picker, pledge matching)
- [x] Register route and module declaration
- [x] Add Record Gift button to dashboard

## Phase 5 — Pledge Dashboard
- [x] Create pledge-dashboard Angular component (progress bars, cards, filters)
- [x] Register route and module declaration
- [x] Add Pledges button to dashboard Reports & Tools

## Phase 6 — Dashboard & Wiring
- [x] Add all navigation methods to dashboard TS
- [x] Add all buttons to dashboard HTML
- [ ] Build verification (dotnet build + ng build)
