# Fix EventCharge 500 & Disable Financial Write Routes

## Problem

The simple booking wizard's rental flow hits `POST api/EventCharge` (the **code-generated** `EventChargesController` in `DataControllers/`), which returns a 500. This route bypasses the `FinancialManagementService` — it does direct DB writes without the financial safeguards (DB transactions, fiscal period validation, GL posting, audit trail).

The `EventCharge` table is missing `SetTableToBeReadonlyForControllerCreationPurposes()` in the generator, so the code generator creates full CRUD routes. Other financial tables (Invoice, Receipt, FinancialTransaction, GL entries) already have this flag.

## Proposed Changes

### 1. Database Generator — Add Readonly Flags

#### [MODIFY] [SchedulerDatabaseGenerator.cs](file:///g:/source/repos/Scheduler/SchedulerDatabaseGenerator/SchedulerDatabaseGenerator.cs)

Add `SetTableToBeReadonlyForControllerCreationPurposes()` to:
- `eventChargeTable` (line ~3069) — **primary fix**
- `paymentTransactionTable` — also financial, also missing

> [!NOTE]
> This only affects future rescaffolds. We still need to manually disable the existing deployed controllers.

---

### 2. Deployed Controller — Block Write Routes

#### [MODIFY] [EventChargesController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/DataControllers/EventChargesController.cs)

Override the `PutEventCharge`, `PostEventCharge` (create), and `DeleteEventCharge` methods in the existing code-generated controller to return `400 Bad Request` with a message directing callers to use `api/financial/charges` instead.

Since the controller is `partial`, we can add a separate partial class file to override the write methods without touching the generated file directly.

---

### 3. Financial Service — Add EventCharge CRUD

#### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

Add three new methods:
- `CreateEventChargeAsync(...)` — validates ChargeType, applies tax from TaxCode, sets default ChargeStatus, calculates extendedAmount/taxAmount/totalAmount
- `UpdateEventChargeAsync(...)` — validates status (can't edit invoiced charges), recalculates amounts
- `DeleteEventChargeAsync(...)` — validates status (can't delete invoiced charges), soft-deletes

#### [MODIFY] [FinancialTransactionsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)

Add new endpoints that delegate to the service:
- `POST api/financial/charges` — create charge
- `PUT api/financial/charges/{id}` — update charge
- `DELETE api/financial/charges/{id}` — delete charge

#### [MODIFY] [event-charge.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/scheduler-data-services/event-charge.service.ts)

Update the `createEventCharge()`, `updateEventCharge()`, and `deleteEventCharge()` methods to POST/PUT/DELETE to `api/financial/charges` instead of `api/EventCharge`.

> [!IMPORTANT]
> The GET routes (`api/EventCharges`, `api/EventCharge/{id}`) stay as-is on the code-generated controller — they're read-only and work fine.

## Verification Plan

### Automated Tests
- `dotnet build` — zero CS errors
- `ng build` — zero TS errors
- Test the simple booking wizard rental flow end-to-end
