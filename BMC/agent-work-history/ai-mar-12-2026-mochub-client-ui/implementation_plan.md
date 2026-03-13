# MOCHub — GitHub-Style Collaborative MOC Publishing Platform

A platform for publishing, versioning, forking, and collaborating on LEGO MOC designs. Styled after GitHub's repository model.

---

## What Already Exists

The BMC database schema already has extensive community infrastructure that MOCHub builds on top of:

| Existing Entity | MOCHub Role |
|----------------|-------------|
| `PublishedMoc` (with `allowForking` flag) | The "repository" listing |
| `Project` → `PlacedBrick` / `Submodel` | The "source code" |
| `ModelDocument` → `sourceFileData` | The raw file content (for diffing) |
| `MocLike` / `MocComment` / `MocFavourite` | Stars / Discussions / Bookmarks |
| `UserFollow` / `ActivityEvent` | Social graph / Activity feed |
| `UserProfile` / `UserProfileStat` | Builder profiles |
| `ProjectTag` / `ProjectTagAssignment` | Topics / Tags |
| `ContentReport` / `ModerationAction` | Content moderation |

> [!IMPORTANT]
> **What's missing for MOCHub**: Version control (commit snapshots), fork lineage (parent→child graph), and enhanced visibility control (public/private/unlisted).

---

## Proposed Changes

### Database Schema — New Tables

> [!NOTE]
> All new tables use the existing `BMC_COMMUNITY_WRITER_PERMISSION_LEVEL` and follow established patterns (multi-tenant, control fields, GUIDs).

---

#### [NEW] `MocVersion` — Version snapshots (commits)

Each row represents a saved snapshot of a MOC at a point in time. Stores the full MPD text content as a snapshot — MPD files are small (~50–200KB even for large models), so full snapshots are simple and reliable.

| Column | Type | Purpose |
|--------|------|---------|
| `publishedMocId` | FK → PublishedMoc | The MOC this version belongs to |
| `versionNumber` | int | Sequential version number (1, 2, 3...) |
| `commitMessage` | text | User-provided description of changes |
| `mpdSnapshot` | text | Full MPD text content at this version |
| `partCount` | int? | Part count at this version |
| `addedPartCount` | int? | Parts added since previous version |
| `removedPartCount` | int? | Parts removed since previous version |
| `modifiedPartCount` | int? | Parts moved/recoloured since previous version |
| `snapshotDate` | datetime | When this version was saved |

---

#### [NEW] `MocFork` — Fork lineage tracking

Tracks the fork graph: which MOC was forked from which, and at what version. Enables GitHub-style "forked from user/moc" headers and network graphs.

| Column | Type | Purpose |
|--------|------|---------|
| `forkedMocId` | FK → PublishedMoc | The fork (child) |
| `sourceMocId` | FK → PublishedMoc | The original (parent) |
| `sourceVersionId` | FK → MocVersion? | The specific version the fork was created from |
| `forkedDate` | datetime | When the fork was created |

---

#### [NEW] `MocCollaborator` — Shared access to MOCs

Allows MOC owners to grant other users read/write access to their MOCs, enabling collaborative building.

| Column | Type | Purpose |
|--------|------|---------|
| `publishedMocId` | FK → PublishedMoc | The MOC |
| `collaboratorTenantGuid` | guid | Tenant GUID of the collaborator |
| `accessLevel` | string(50) | `Read`, `Write`, `Admin` |
| `invitedDate` | datetime | When the collaborator was added |
| `acceptedDate` | datetime? | When the invitation was accepted |

---

#### [MODIFY] `PublishedMoc` — Add version control fields

| New Column | Type | Purpose |
|------------|------|---------|
| `visibility` | string(50) | `Public`, `Private`, `Unlisted` (replaces boolean `isPublished`) |
| `currentVersionId` | FK → MocVersion? | Points to the latest version |
| `forkCount` | int (default 0) | Cached fork count |
| `forkedFromMocId` | FK → PublishedMoc? | Direct FK to the source MOC (denormalized for fast display) |
| `licenseName` | string(100) | Licence type: `CC-BY`, `CC-BY-SA`, `CC-BY-NC`, `All Rights Reserved`, etc. |
| `readmeMarkdown` | text | GitHub-style README displayed on the MOC page |
| `defaultBranchName` | string(100) | Reserved for future branching support (default: "main") |

---

### Server — Controllers & Services

---

#### [NEW] `Controllers/MocHubController.cs`

Public + authenticated endpoints for the MOCHub experience:

**Public (anonymous):**
- `GET /api/mochub/explore` — Trending, recent, most-starred public MOCs
- `GET /api/mochub/explore/search` — Full-text search with filters (tags, part count, theme, author)
- `GET /api/mochub/{username}/{mocSlug}` — MOC detail page (README, stats, version list, parts list)
- `GET /api/mochub/{username}/{mocSlug}/versions` — Version history list
- `GET /api/mochub/{username}/{mocSlug}/versions/{versionNum}` — Specific version detail
- `GET /api/mochub/{username}/{mocSlug}/versions/{versionNum}/diff` — Diff between two versions
- `GET /api/mochub/{username}/{mocSlug}/forks` — Fork network
- `GET /api/mochub/{username}` — User's public MOC directory

**Authenticated:**
- `POST /api/mochub/publish` — Publish a project as a MOC (creates initial version snapshot)
- `POST /api/mochub/{id}/commit` — Create a new version snapshot with commit message
- `POST /api/mochub/{id}/fork` — Fork a MOC to the current user's account
- `PUT /api/mochub/{id}` — Update MOC metadata (README, visibility, license, tags)
- `PUT /api/mochub/{id}/visibility` — Change visibility (public/private/unlisted)
- `DELETE /api/mochub/{id}` — Unpublish / delete
- `POST /api/mochub/{id}/collaborators` — Add collaborator
- `DELETE /api/mochub/{id}/collaborators/{tenantGuid}` — Remove collaborator

---

#### [NEW] `Services/MocVersioningService.cs`

Core version control logic:

- `CreateSnapshotAsync(publishedMocId, commitMessage)` — Generates MPD via `ModelExportService`, stores as `MocVersion` row
- `ComputeDiffAsync(versionA, versionB)` — Parses both MPD snapshots, diffs part placements, returns structured change summary (added/removed/moved bricks with position and colour)
- `ForkMocAsync(sourceMocId, targetTenantGuid)` — Clones the project (PlacedBricks, Submodels, SubmodelInstances, ModelDocument), creates new PublishedMoc + MocFork record + initial MocVersion

---

### Client — Angular UI Components

Organised into 4 feature areas, all styled with GitHub's design language (clean white/dark backgrounds, monospace metadata, subtle borders, contribution-graph green).

---

#### Area 1: Explore & Discovery

| Component | Description |
|-----------|-------------|
| `mochub-explore` | Landing page: trending, recent, featured MOCs. Search bar with tag/theme filters. Responsive card grid with thumbnails, star count, fork count, author avatar. |
| `mochub-search-results` | Full search results page with sort (stars, recent, forks, parts), advanced filters. |

---

#### Area 2: MOC Repository Page

| Component | Description |
|-----------|-------------|
| `mochub-repo` | Main repository page: header (author/name, star/fork buttons, visibility badge), tabbed content (README, Files, Versions, Parts, Forks). |
| `mochub-readme` | Rendered markdown README panel with GitHub-style typography. |
| `mochub-version-list` | Commit history list — each entry shows version number, message, date, part count delta, author avatar. |
| `mochub-version-diff` | Visual diff between two versions — table of added/removed/moved parts with colour swatches. |
| `mochub-fork-network` | Fork graph visualisation showing lineage tree. |

---

#### Area 3: Publishing & Management

| Component | Description |
|-----------|-------------|
| `mochub-publish-modal` | Publish wizard: select project, set name/description/tags/visibility/license, optional README, commit message. |
| `mochub-commit-modal` | Quick commit dialog: commit message input + button. |
| `mochub-settings` | Repository settings page: rename, change visibility, manage collaborators, danger zone (delete/transfer). |

---

#### Area 4: User MOC Directory

| Component | Description |
|-----------|-------------|
| `mochub-user-repos` | User's MOC listing (public profile tab): pinned MOCs, sorted list, contribution activity graph. |

---

### Navigation & Routing

| Route | Component |
|-------|-----------|
| `/mochub` | `mochub-explore` |
| `/mochub/search` | `mochub-search-results` |
| `/mochub/:username` | `mochub-user-repos` |
| `/mochub/:username/:slug` | `mochub-repo` |
| `/mochub/:username/:slug/versions` | `mochub-version-list` (also tab in repo) |
| `/mochub/:username/:slug/settings` | `mochub-settings` |

- Add **MOCHub** to sidebar navigation (under COMMUNITY group)
- Add MOCHub pathway card to the welcome page

---

## Implementation Phases

### Phase 1 — Database Schema & Migrations
Add `MocVersion`, `MocFork`, `MocCollaborator` tables to `BmcDatabaseGenerator.cs`. Add new columns to `PublishedMoc`. Write idempotent SQL migration scripts. Rescaffold EF Core entities.

### Phase 2 — Server Services & Controller
Implement `MocVersioningService` (snapshot, diff, fork logic). Implement `MocHubController` with all endpoints. Wire DI registrations in `Program.cs`.

### Phase 3 — Client Core UI
Build `mochub-explore`, `mochub-repo`, `mochub-version-list`, `mochub-publish-modal` components. Add routing and navigation. Style with GitHub-inspired design.

### Phase 4 — Advanced Features & Polish
Build `mochub-version-diff`, `mochub-fork-network`, `mochub-settings`, `mochub-commit-modal`. Add collaboration features. Polish UX, add loading states, error handling.

---

## Verification Plan

### Build Verification
- `dotnet build BmcDatabaseGenerator/BmcDatabaseGenerator.csproj` — Schema generator compiles
- `dotnet build BMC/BMC.Server/BMC.Server.csproj` — Server compiles with new controller/service
- `ng build --configuration production` (in `BMC/BMC.Client/`) — Angular client compiles

### Manual Testing
After each phase, navigate the BMC app locally and verify:
1. **Phase 1**: New tables appear in database after migration
2. **Phase 2**: API endpoints respond correctly via browser dev tools / Swagger
3. **Phase 3**: MOCHub explore page loads, publish flow works end-to-end, MOC repo page renders
4. **Phase 4**: Version history displays, diff shows changes, fork creates a copy

> [!NOTE]
> No automated test framework exists in the BMC project currently. Verification is build-based + manual testing, consistent with all prior BMC development sessions.
