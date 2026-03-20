# File Manager — Component Decomposition Refactoring

The `file-manager` component is currently a monolith: **2,079 lines TS**, **972 lines HTML**, and **1,986 lines SCSS** — all in a single component. This refactoring extracts self-contained UI regions into child components, keeping the parent as an orchestrator. No new features or behavioral changes.

## Proposed Changes

We'll extract **5 child components**. Each gets its own folder under `file-manager/`, following the project's existing sub-component pattern (similar to `resource-custom/resource-overview-tab/`).

> [!IMPORTANT]
> All child components use `@Input()` / `@Output()` bindings to communicate with the parent. The parent retains all state and service dependencies — children are purely presentational or lightly interactive. This keeps the refactoring **safe** (no logic changes, just template extraction).

---

### 1. `fm-sidebar`

**What it owns:** Folder tree, favorites accordion, tags accordion, entity links accordion, quick-nav toolbar (trash/activity icons), storage usage bar, "New Folder" button.

#### [NEW] [fm-sidebar.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-sidebar/fm-sidebar.component.ts)
#### [NEW] [fm-sidebar.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-sidebar/fm-sidebar.component.html)
#### [NEW] [fm-sidebar.component.scss](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-sidebar/fm-sidebar.component.scss)

**Inputs:** `folderTree`, `allFolders`, `currentFolderId`, `sidebarCollapsed`, `favoriteDocuments`, `allTags`, `tagCounts`, `activeTagFilters`, `activeEntityLinkTypes`, `activeEntityLinkFilter`, `trashCount`, `showTrash`, `showActivity`, `storageUsage`, `fileManagerService` (for formatting helpers), accordion state flags.

**Outputs:** `navigateToFolder`, `toggleSidebarCollapse`, `toggleTagFilter`, `clearTagFilters`, `toggleEntityLinkFilter`, `clearEntityLinkFilter`, `toggleTrash`, `toggleActivity`, `openTagManager`, `showNewFolderDialog`, accordion toggle events.

**HTML moved:** Lines 63–183 (the entire `fm-sidebar` div) plus the recursive folder template (lines 951–971).

**SCSS moved:** `.fm-sidebar`, `.fm-sidebar-header`, `.fm-sidebar-title`, `.fm-sidebar-toggle`, `.fm-folder-tree`, `.fm-folder-node`, `.fm-tree-arrow`, `.fm-folder-icon`, `.fm-folder-name`, `.fm-sidebar-footer`, `.fm-sidebar-accordion`, `.fm-accordion-*`, `.fm-quick-nav`, `.fm-storage-*`, `.fm-tag-sidebar-*`, `.fm-tag-dot` classes.

---

### 2. `fm-detail-panel`

**What it owns:** Document preview (image/PDF/text), tags section with add/remove, entity links section with the two-step add picker, document properties table, version history, download/delete action buttons.

#### [NEW] [fm-detail-panel.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-detail-panel/fm-detail-panel.component.ts)
#### [NEW] [fm-detail-panel.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-detail-panel/fm-detail-panel.component.html)
#### [NEW] [fm-detail-panel.component.scss](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-detail-panel/fm-detail-panel.component.scss)

**Inputs:** `selectedDocument`, `previewBlobUrl`, `safePreviewBlobUrl`, `textPreviewContent`, `selectedDocumentTags`, `unassignedTags`, `entityLinks`, `linkableEntityTypes`, `showTagDropdown`, `showAddEntityLinkDropdown`, `addLinkSelectedType`, `addLinkSearchQuery`, `addLinkSearchResults`, `addLinkSearching`, `documentVersions`, `showVersionHistory`, `fileManagerService` (for formatting/icon helpers), `isFavorite`.

**Outputs:** `close`, `download`, `delete`, `toggleFavorite`, `addTag`, `removeTag`, `toggleTagDropdown`, `toggleAddEntityLink`, `selectEntityType`, `searchEntities`, `assignEntityLink`, `cancelAddEntityLink`, `removeEntityLink`, `toggleVersionHistory`, `uploadNewVersion`.

**HTML moved:** Lines 556–778 (the `fm-detail-panel` div and all its contents).

**SCSS moved:** `.fm-detail-panel`, `.fm-detail-header`, `.fm-detail-body`, `.fm-detail-preview`, `.fm-preview-*`, `.fm-detail-props`, `.fm-prop-*`, `.fm-detail-actions`, `.fm-detail-tags*`, `.fm-tag-chips`, `.fm-tag-chip`, `.fm-tag-dropdown*`, `.fm-entity-link-*`, `.fm-entity-add-*`, `.fm-entity-search-*`, `.fm-detail-versions`, `.fm-version-*` classes.

---

### 3. `fm-trash-view`

**What it owns:** Trash header (back/empty buttons), trash list with restore/delete actions, empty-state message.

#### [NEW] [fm-trash-view.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-trash-view/fm-trash-view.component.ts)
#### [NEW] [fm-trash-view.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-trash-view/fm-trash-view.component.html)
#### [NEW] [fm-trash-view.component.scss](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-trash-view/fm-trash-view.component.scss)

**Inputs:** `trashDocuments`, `fileManagerService` (for formatting/icon helpers).

**Outputs:** `close`, `restore`, `permanentDelete`, `emptyTrash`.

**HTML moved:** Lines 353–392 (the `fm-trash-view` div).

**SCSS moved:** `.fm-trash-*` classes.

---

### 4. `fm-activity-panel`

**What it owns:** Activity header, timeline of activity items, empty state.

#### [NEW] [fm-activity-panel.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-activity-panel/fm-activity-panel.component.ts)
#### [NEW] [fm-activity-panel.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-activity-panel/fm-activity-panel.component.html)
#### [NEW] [fm-activity-panel.component.scss](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-activity-panel/fm-activity-panel.component.scss)

**Inputs:** `activityItems`.

**Outputs:** `close`.

**HTML moved:** Lines 782–809 (the `fm-activity-panel` div).

**SCSS moved:** `.fm-activity-*` classes.

---

### 5. `fm-tag-manager-modal`

**What it owns:** Tag list with inline edit, color picker, create-new-tag form.

#### [NEW] [fm-tag-manager-modal.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-tag-manager-modal/fm-tag-manager-modal.component.ts)
#### [NEW] [fm-tag-manager-modal.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-tag-manager-modal/fm-tag-manager-modal.component.html)
#### [NEW] [fm-tag-manager-modal.component.scss](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-tag-manager-modal/fm-tag-manager-modal.component.scss)

**Inputs:** `visible`, `allTags`, `tagCounts`, `tagColorOptions`.

**Outputs:** `close`, `createTag`, `updateTag`, `deleteTag`.

**HTML moved:** Lines 882–941 (the tag manager modal).

**SCSS moved:** `.fm-tag-mgr-*`, `.fm-tag-color-*` classes.

---

### Parent Component Changes

#### [MODIFY] [file-manager.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/file-manager.component.ts)

- Remove template-only helpers that move entirely into children (e.g., `toggleFolderExpand`)
- Keep all state, services, and business logic
- Connect child components via `@Input()`/`@Output()` bindings
- Expected reduction: **~800+ lines** removed from TS, template reduced by ~60%

#### [MODIFY] [file-manager.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/file-manager.component.html)

- Replace extracted HTML blocks with child component tags
- Keep: header, content area (grid/list/breadcrumbs/bulk bar), context menus, folder dialog, rename dialog, upload input

#### [MODIFY] [file-manager.component.scss](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/file-manager.component.scss)

- Move relevant style blocks to each child's SCSS file
- Keep: `.page-container`, `.premium-header`, `.fm-main`, `.fm-content`, `.fm-action-bar`, `.fm-breadcrumbs`, `.fm-subfolder-*`, `.fm-file-grid`, `.fm-file-card`, `.fm-file-list`, `.fm-drop-overlay`, `.fm-loading`, `.fm-import-*`, `.fm-context-menu`, `.fm-modal*`, `.fm-bulk-*`, `.fm-search-*`, `.fm-tag-filter-bar`, `.fm-tag-pill*`, `.fm-empty`

---

### Module Registration

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)

- Import and declare all 5 new child components

---

## Verification Plan

### Automated Tests

No existing spec files cover `FileManagerComponent` (only `new-user.component.spec.ts` was found in `components/`). This is a pure refactoring — no logic changes — so we rely on the build and manual verification.

**Build verification:**
```powershell
cd g:\source\repos\Scheduler\Scheduler\Scheduler.Client
npx ng build --configuration=development 2>&1 | Select-Object -Last 20
```

### Manual Verification

After the build succeeds, the **user should verify** the following in the browser at `/filemanager`:

1. **Sidebar** — Folder tree renders, clicking folders navigates, accordion sections (Favorites, Tags, Entity Links) expand/collapse, trash/activity icons work, storage bar appears
2. **Detail panel** — Clicking a document opens the detail panel with preview, tags, entity links, version history, and download/delete buttons
3. **Trash** — Trash view shows deleted documents, restore and permanent delete work
4. **Activity** — Activity panel slides in with the timeline
5. **Tag manager** — Modal opens via gear icon, tags can be created/edited/deleted
6. **Context menus** — Right-click on documents and folders still works
7. **Drag & drop** — File upload via drag-and-drop still works
