# Financial UI Audit — Scheduler.Client

## What Exists Today

### Custom (Enhanced) Components — 14 total

| Component | Quality | Notes |
|---|---|---|
| **Financial Dashboard** | ✅ Good | Summary cards, monthly chart, category breakdown, deposits, year + office filters, Excel export |
| **P&L Report** | ✅ Good | Dedicated report component |
| **Budget Report** | ✅ Good | Compares budget vs actuals |
| **Accountant Reports** | ✅ Good | Export-oriented reporting |
| **Budget Manager** | ✅ Good | Budget CRUD |
| **Deposit Manager** | ✅ Good | Outstanding deposits tracker |
| **Fiscal Period Close** | ✅ Good | Period locking workflow |
| **Invoice Listing** | ✅ Good | Status filters, sort, search, PDF, status badges |
| **Invoice Detail** | ✅ Good | Line items, PDF generation, edit, balance due |
| **Invoice Add/Edit** | ✅ Good | Modal-based creation |
| **Receipt Listing/Detail/Add-Edit** | ✅ Good | PDF generation, edit modals |
| **Payment Listing/Detail/Add-Edit** | ✅ Good | Standard CRUD |
| **Transaction Listing/Add-Edit** | ✅ Good | Income/expense recording with category |
| **Category Listing/Add-Edit** | ✅ Good | Revenue/expense category management |
| **Contact Financials Tab** | ✅ Good | Per-contact financial view |

### Auto-Generated CRUD Components
Standard listing/detail/add-edit/table for: Gift, Pledge, Soft Credit, Tribute, Constituent, Fund, Campaign, Appeal, Batch, Account Type, Charge Type/Status, etc.

---

## What's Missing — QuickBooks-Inspired Gaps

### 🔴 Critical Gaps (No UI exists)

| Missing Component | QuickBooks Equivalent | Why It Matters |
|---|---|---|
| **Void Invoice UI** | Sales → Invoices → Void | Backend `VoidInvoiceAsync` exists but no button or workflow in the UI |
| **Issue Refund UI** | Sales → Refund Receipt | Backend `IssueRefundAsync` exists but no trigger in the UI |
| **Record Payment UI** | Receive Payment dialog | Currently buried in receipt creation — should be a prominent "Record Payment" action on invoice detail |
| **Gift Entry Form** | N/A (QuickBooks doesn't do this) | Auto-generated CRUD exists but no purpose-built donation entry UX |

### 🟡 Enhancement Opportunities

| Enhancement | QuickBooks Inspiration | Impact |
|---|---|---|
| **Invoice Detail → Action Bar** | QB's "More actions" menu: Send, Record Payment, Void, Print | Currently invoice detail only has Edit + PDF — needs Void, Record Payment, Send buttons |
| **Accounts Receivable Aging** | Reports → A/R Aging Summary | No aging report — who owes what and how overdue |
| **Revenue by Client Report** | Reports → Sales by Customer | No per-client revenue rollup |
| **Pledge Fulfillment Dashboard** | N/A | No visual progress toward pledge goals — `balanceAmount` exists but isn't surfaced well |

### 🟢 Nice-to-Haves

| Feature | Description |
|---|---|
| **Dashboard "Money In/Out" chart** | QB-style dual-axis chart showing cash flow trend |
| **Invoice status pipeline** | Visual kanban: Draft → Sent → Partially Paid → Paid |
| **Quick-create shortcuts** | Dashboard quick-buttons: "New Invoice", "Record Payment", "Record Expense" (partially done) |
| **Year-over-year comparison** | Compare current fiscal year vs previous on dashboard |
| **Client statement generation** | Generate PDF statement for a client showing all invoices/payments |

---

## Architecture Assessment

**Strengths:**
- Clean separation: auto-generated CRUD in `scheduler-data-components/`, custom enhanced UX in `components/`
- Dashboard uses server-side aggregation endpoints (not client-side computation) — scales well
- Responsive design with `BreakpointObserver`
- PDF generation for invoices and receipts
- Fiscal-year-aware filtering throughout

**Weaknesses:**
- The new `FinancialManagementService` backend operations (Void, Refund, RecordGift, ReconcilePledge) have **no corresponding UI triggers** — they're API-only right now
- Gift/Pledge/Constituent components are all auto-generated CRUD only — no purpose-built donation management UX
- No aging report or client-level financial summary

---

## Recommended Priority Order

1. **Invoice Action Bar** — Add Void, Record Payment, and Send to invoice detail (wires up existing backend)
2. **Refund workflow** — Add refund button on receipt detail or invoice detail
3. **A/R Aging Report** — Most-requested financial report for any municipal/business tenant
4. **Gift Entry Form** — Purpose-built donation entry that surfaces pledge matching and creates ledger entries via `RecordGiftAsync`
5. **Pledge Dashboard** — Visual progress toward pledge fulfillment goals
6. **Revenue by Client** — Per-client financial summary
