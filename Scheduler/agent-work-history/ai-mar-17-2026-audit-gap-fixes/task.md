# Audit Gap Fixes

- [x] Fix 1: Lock down financial CRUD endpoints (7 tables in generator)
- [x] Fix 2: GL balance validation in PostToGeneralLedgerAsync + PostReversalToGLAsync
- [x] Fix 3: GL reconciliation endpoint (ReconcileGLAsync + API)
- [x] Fix 4: Fix userId=0 in RecordExpense/Revenue change history
- [x] Fix 5: Remove pageSize:10000 from 9 client components
- [/] Verify builds
