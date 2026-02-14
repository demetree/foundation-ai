# Change History Viewer — Session Walkthrough

## 1. Cache Invalidation Fix
Added `this.auditHistory = null` in each component's reload path so history re-fetches after edits.

## 2. Version Detail Modal
Added a click-to-open modal with **Changes** (untruncated diffs) and **Snapshot** (all field values) tabs to `ChangeHistoryViewerComponent`. Self-contained — no parent/module changes.

## 3. Direct URL Navigation Fix
History tab was empty when navigating directly to `?tab=history`. Added `if (activeTab === 'history') loadHistory()` after data load success in all 7 entity detail components.

## 4. Server-Side Version Data Bug Fix

### Root Cause
[EntityExtensionGenerator.cs:660](file:///d:/source/repos/scheduler/CodeGenerationCore/EntityExtensionGenerator.cs#L660) generated `GetAllVersionsAsync` with a hardcoded version `1`:

```diff
- version.data = await chts.GetVersionAsync(this, 1).ConfigureAwait(false);
+ version.data = await chts.GetVersionAsync(this, versionAudit.versionNumber).ConfigureAwait(false);
```

Every version snapshot incorrectly returned v1's data, making all diffs empty.

### Changes
- **Code generator**: Fixed template in `EntityExtensionGenerator.cs`
- **Generated files**: Bulk-patched all ~50 entity extension files in `SchedulerDatabase/EntityExtensions/`

### Verification
- `dotnet build` — 0 errors ✓
- `ng build` — 0 errors ✓
