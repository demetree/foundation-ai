# Financial Module Fixes

## Bug Fixes
- [x] Fix dashboard monthly chart to respect office filter (use `getFilteredTransactions()` instead of `allTransactions`)
- [x] Fix budget manager to filter transaction actuals by fiscal period

## Enhancements
- [x] Bump `pageSize` from 5000 to 10000 across all financial components and add TODO comments
- [x] Add click handler to mobile transaction cards for editing
- [x] Implement `hiddenFields` logic in transaction add-edit HTML template
- [x] Create custom category add-edit modal component
- [x] Create server-side financial aggregation controller (easy wins)

## Verification
- [x] Build client application successfully
- [x] Build server application successfully
