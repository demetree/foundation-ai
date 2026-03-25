# EventCharge Route Fix & Financial Write Safeguards

## Problem
The simple booking wizard's rental flow hit `POST api/EventCharge` (code-generated controller), which bypassed the `FinancialManagementService`. The code-generated controller did direct DB writes without transaction wrapping, fiscal period validation, or audit trails.

## Changes Made

### 1. Generator — Readonly Flags
#### [SchedulerDatabaseGenerator.cs](file:///g:/source/repos/Scheduler/SchedulerDatabaseGenerator/SchedulerDatabaseGenerator.cs)
Added `SetTableToBeReadonlyForControllerCreationPurposes()` to:
- `eventChargeTable` (line ~3070)
- `paymentTransactionTable` (line ~3425)

Future rescaffolds will produce read-only controllers for these financial entities.

---

### 2. Deployed Controller — Write Blocking
#### [NEW] [EventChargesControllerOverrides.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/EventChargesControllerOverrides.cs)
Partial class that shadows `PostEventCharge`, `PutEventCharge`, `DeleteEventCharge` with `400 Bad Request` responses directing callers to `api/financial/charges`.

---

### 3. Financial Service — EventCharge CRUD
#### [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)
Added three new methods:
- `CreateEventChargeAsync` — validates ChargeType/ChargeStatus, calculates tax from TaxCode, computes amounts (qty × unitPrice), writes change history
- `UpdateEventChargeAsync` — guards against editing invoiced/voided charges, recalculates amounts
- `DeleteEventChargeAsync` — guards against deleting invoiced charges, soft-deletes

Helper: `WriteEventChargeChangeHistory` — structured JSON audit trail

---

### 4. Controller Endpoints
#### [FinancialTransactionsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)
New endpoints:
- `POST api/financial/charges` — create EventCharge
- `PUT api/financial/charges/{id}` — update EventCharge
- `DELETE api/financial/charges/{id}` — soft-delete EventCharge

All delegate to `FinancialManagementService` via the same DI pattern as existing `VoidTransaction`.

---

### 5. Client Route Updates
#### [event-charge.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/scheduler-data-services/event-charge.service.ts)
Updated `PostEventCharge`, `PutEventCharge`, `DeleteEventCharge` to call `api/financial/charges` instead of `api/EventCharge`.

---

### 6. Private Rental Booking Fix
#### [private-rental-booking.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/private-rental-booking/private-rental-booking.component.ts)
Fixed charge creation payload — the old code sent `{amount, isPaid}` which doesn't match the `EventChargeRequest` DTO. Now correctly sends `{quantity, unitPrice, chargeStatusId, currencyId, chargeTypeId, ...}`. Added `ChargeStatusService` and `CurrencyService` imports/DI/lookups.

### 7. Event-Add-Edit-Modal Review
#### [event-add-edit-modal.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.ts)
Reviewed — **no changes needed**. This component only reads charges (via `GetEventChargeList` for the financials tab) and calls `PutEventCharge` in `refundDeposit()`, which already routes through the service's updated URL.

## Verification
- **Server build**: ✅ 0 errors
- **Client build**: ✅ 0 TypeScript errors (exit code 1 is a pre-existing bundle size budget warning)
