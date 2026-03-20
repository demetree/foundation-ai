# Document Exchange Features — Task Checklist

## Planning
- [x] Research existing codebase patterns (controller, DB model, email, roles)
- [x] Draft implementation plan for all three features
- [x] Get user approval on implementation plan

## Feature 1: Shareable Links
- [x] Create `DocumentShareLink` entity class + extension (DTO, ToDTO, FromDTO, etc.)
- [x] Register in `SchedulerContext` (DbSet + OnModelCreating)
- [x] Add table to `SchedulerDatabaseGenerator.cs`
- [x] Add authenticated endpoints to `FileManagerController` (create, list, revoke)
- [x] Add unauthenticated public download endpoints (metadata, download, verify password)
- [x] Add client service methods to `file-manager.service.ts`
- [x] Build verification — 0 errors, 0 warnings
- [x] Add share link UI to `fm-detail-panel` (create, list, copy, revoke)

## Feature 2: Email a File
- [x] Add email endpoint to `FileManagerController`
- [x] Add client service method
- [x] Add email UI to `fm-detail-panel` (inline form with to/subject/message/sendAsLink)
- [x] Angular build verification — 0 TS errors

## Feature 3: Upload Portal — DEFERRED
- [ ] _(Deferred to follow-up session per user request)_
