# Phase 1 — Lock Down All Mutations

## Backend Service
- [x] Add `UpdateTransactionAsync()` to `FinancialManagementService`
- [x] Add `VoidTransactionAsync()` to `FinancialManagementService`

## Backend Controller
- [x] Add `PUT /api/FinancialTransactions/{id}/Update` endpoint
- [x] Add `POST /api/FinancialTransactions/{id}/Void` endpoint

## Frontend
- [x] Route edits through `/Update` endpoint via `putViaService()`
- [x] Add void confirmation dialog with reason prompt
- [x] Route voids through `/Void` endpoint

## Verification
- [x] `dotnet build` — 0 errors
- [x] `ng build` — no new errors
