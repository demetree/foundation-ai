# Feature Toggles & File Manager Folder View Improvements

**Date:** 2026-03-21

## Summary

Two major improvements in this session:
1. **Consolidated Feature Toggle System** — Added system-level toggles and role-based access for Fundraising, Financial/Billing, and Crew Management modules, consolidating the earlier per-feature Volunteer approach into a unified endpoint.
2. **File Manager Compact Folder View** — Added a badge/list toggle for subfolder display with auto-switching, collapsibility, and scroll safety to handle hundreds of folders.

## Changes Made

### Feature Toggle System (Consolidated)

- **[NEW] `Scheduler.Server/Controllers/FeatureConfigController.cs`** — Unified `GET /api/FeatureConfig` endpoint returning all 4 feature toggle states
- **[DELETE] `Scheduler.Server/Controllers/VolunteerManagementConfigController.cs`** — Replaced by unified controller
- **[MODIFY] `Scheduler.Server/appsettings.json`** — Added `FundraisingEnabled` (false), `FinancialManagementEnabled` (false), `CrewManagementEnabled` (true) toggles
- **[MODIFY] `Scheduler.Server/Program.cs`** — Replaced `VolunteerManagementConfigController` registration with `FeatureConfigController`
- **[NEW] `Scheduler.Client/src/app/services/feature-config.service.ts`** — Unified client service caching all feature flags with individual observables
- **[DELETE] `Scheduler.Client/src/app/services/volunteer-config.service.ts`** — Replaced by unified service
- **[MODIFY] `Scheduler.Client/src/app/services/auth.service.ts`** — Added `isFundraisingManager` and `isFinancialManager` role getters
- **[MODIFY] `Scheduler.Client/src/app/components/sidebar/sidebar.component.ts`** — Wired all feature observables and role checks
- **[MODIFY] `Scheduler.Client/src/app/components/sidebar/sidebar.component.html`** — Gated: Finances (fundraising+role), Crews/Shifts/Shift Patterns (crew toggle), Rate Sheets (financial+role)

### File Manager Folder View

- **[MODIFY] `Scheduler.Client/src/app/components/file-manager/file-manager.component.ts`** — Added `folderViewMode` (badges/list), `foldersCollapsed`, auto-switch threshold (8), preference persistence
- **[MODIFY] `Scheduler.Client/src/app/components/file-manager/file-manager.component.html`** — Folder section header with toggle/collapse buttons, collapsed summary bar, compact list template
- **[MODIFY] `Scheduler.Client/src/app/components/file-manager/file-manager.component.scss`** — Section header, collapsed bar, compact list row styles, max-height scroll safety

## Key Decisions

- **Unified endpoint over per-feature controllers** — Single `FeatureConfigController` returns all flags in one HTTP call, reducing boilerplate and network requests
- **Two-layer access control** — System toggle (deployment-wide) + JWT role check (per-user) for modules needing it; Crew Management uses toggle-only (no dedicated role)
- **Auto-switch threshold of 8 folders** — Balances visual appeal of badges with density needs; manually overridable and persisted
- **Default toggle values for Petty Harbour launch** — Fundraising/Financial off (not needed for rec committee), Crew/Volunteer on

## Testing / Verification

- **Server build**: 0 errors, 142 pre-existing warnings
- **Client build**: TypeScript compilation clean; pre-existing bundle budget exceeded warning (21.47 MB vs 21 MB limit) unrelated to changes
- Folder view modes, toggle persistence, and drag-and-drop targets verified structurally
