# Generate Fiscal Year Periods

## Problem
No UI or API exists to create fiscal periods. They're only created by the data loader tool. When the coordinator starts a new year, she has no way to set up fiscal periods.

## Approach
Add a backend endpoint that generates 12 monthly fiscal periods for a given year (matching the data loader pattern), and add a "Generate Year" button to the existing `fiscal-period-close` component.

---

## Proposed Changes

### Backend

#### [MODIFY] [FiscalPeriodsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FiscalPeriodsController.cs)

Add `POST /api/FiscalPeriods/GenerateYear?year=2026`:
- Creates 12 monthly periods (Jan–Dec) with status "Open"
- Idempotent — skips if periods already exist for the year
- Validates year range (current year ± 5)
- Delegates to `FinancialManagementService.GenerateFiscalYearAsync()`

### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

Add `GenerateFiscalYearAsync(tenantGuid, year)`:
- Creates 12 monthly `FiscalPeriod` records
- Resolves "Open" `PeriodStatus` from seed data
- Idempotent check — returns existing count if periods exist
- Runs in a transaction

### Frontend

#### [MODIFY] [fiscal-period-close.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/fiscal-period-close/fiscal-period-close.component.ts)

Add:
- `generateYear` input field and `onGenerateYear()` method
- Calls `POST /api/FiscalPeriods/GenerateYear?year=XXXX`
- On success, reloads the period list
- Defaults to next year (current year + 1)

#### [MODIFY] [fiscal-period-close.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/fiscal-period-close/fiscal-period-close.component.html)

- Add "Generate Year" button + year input in the header area

---

## Verification
- `dotnet build` passes
- `ng build` passes
- Generate 2027 → creates 12 periods, shows in list
- Generate 2027 again → no duplicates
