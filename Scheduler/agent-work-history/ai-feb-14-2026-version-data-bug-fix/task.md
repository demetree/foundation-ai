# Fix Audit History Data Bug

- [x] Fix history tab empty on direct URL navigation (`?tab=history`) — 7 components
- [x] Fix server-side `data: null` in audit history responses
  - [x] Fix code generator bug: `GetAllVersionsAsync` hardcodes version 1
  - [x] Bulk-patch ~50 generated entity extension files in `SchedulerDatabase/EntityExtensions/`
  - [x] Verify: `dotnet build` passes (0 errors), `ng build` passes (0 errors)
