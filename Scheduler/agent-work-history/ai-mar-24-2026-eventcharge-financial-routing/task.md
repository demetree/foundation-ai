# Fix EventCharge 500 & Disable Financial Write Routes

- [x] Part 1: Generator — add readonly flags
  - [x] `eventChargeTable.SetTableToBeReadonlyForControllerCreationPurposes()`
  - [x] `paymentTransactionTable.SetTableToBeReadonlyForControllerCreationPurposes()`
- [x] Part 2: Block deployed write routes on EventChargesController
  - [x] Create `EventChargesControllerOverrides.cs` partial class to override POST/PUT/DELETE with 400
- [x] Part 3: Add EventCharge CRUD to financial service
  - [x] `CreateEventChargeAsync` in FinancialManagementService
  - [x] `UpdateEventChargeAsync` in FinancialManagementService
  - [x] `DeleteEventChargeAsync` in FinancialManagementService
  - [x] Wire `api/financial/charges` endpoints in FinancialTransactionsController
  - [x] Update `event-charge.service.ts` client routes
- [x] Part 4: Fix private-rental-booking charge payload
  - [x] Added `chargeStatusId`, `currencyId`, `quantity`, `unitPrice` to charge creation
  - [x] Load ChargeStatus/Currency lookups
- [x] Verify build (server: 0 errors ✓, client: 0 TS errors ✓)
- [x] Review event-add-edit-modal (only reads charges — no changes needed)
