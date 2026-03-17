# Generate Fiscal Year Periods

## Backend
- [x] Add `GenerateFiscalYearAsync()` to `FinancialManagementService`
- [x] Add `POST /api/FiscalPeriods/GenerateYear` endpoint to controller

## Frontend
- [x] Add generate year UI to `fiscal-period-close` component (TS, HTML, SCSS)

## Verification
- [x] `dotnet build` — compilation correct (file-lock errors only from running server)
- [x] `ng build` — no new errors
