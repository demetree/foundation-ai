# BMC Collection, Export & Round-Trip — Phased Plan

Three features broken into phases, each independently shippable.

---

## Phase 1: My Collection UI

> [!IMPORTANT]
> This is a premium, hand-crafted UI component — not the auto-generated CRUD views. It should match the quality level of the Parts Catalog and Colour Library.

### Server

#### [NEW] [CollectionController.cs](file:///d:/source/repos/scheduler/BMC/BMC.Server/Controllers/CollectionController.cs)

Custom API controller with composite endpoints:

- `GET /api/collection/mine` — list current user's collections with summary stats (part count, total bricks, value)
- `GET /api/collection/{id}/parts` — paginated parts list with part name, colour swatch, quantity, thumbnail
- `POST /api/collection/{id}/import-set/{legoSetId}` — import a LEGO set's full parts list into a collection, auto-creating `UserCollectionPart` entries and a `UserCollectionSetImport` record
- `POST /api/collection/{id}/add-part` — add/increment a single part+colour to collection
- `DELETE /api/collection/{id}/remove-part/{partId}` — remove a part from collection

---

### Client

#### [NEW] `components/my-collection/` — MyCollectionComponent

Full-page component with:

- **Collection selector** — dropdown/tabs for switching between user's collections
- **Collection summary** — total unique parts, total brick count, imported sets list
- **Parts grid** — reuse card style from Parts Catalog, showing colour swatch + quantity badge + thumbnail
- **Quick actions** — "Add Set" modal (search/select from LegoSet), "Add Part" modal (search from BrickPart, pick colour)
- **Wishlist tab** — display `UserWishlistItem` entries with "any colour" indicator

#### Routing & Sidebar

- Route: `/collection` → `MyCollectionComponent`
- Add "My Collection" to sidebar navigation (icon: `fas fa-boxes-stacked`)

---

## Phase 2: BOM / Wanted List Export

### Server

#### [NEW] [BomExportController.cs](file:///d:/source/repos/scheduler/BMC/BMC.Server/Controllers/BomExportController.cs)

- `GET /api/bom/project/{projectId}/bricklink-xml` — generates BrickLink Wanted List XML from all `PlacedBrick` entries in a project, grouped by part+colour with quantities
- `GET /api/bom/project/{projectId}/rebrickable-csv` — generates Rebrickable-compatible CSV
- `GET /api/bom/project/{projectId}/summary` — JSON summary: total unique parts, total bricks, grouped by category, missing-from-collection count (if collection ID provided)
- `GET /api/bom/collection/{collectionId}/bricklink-xml` — export a collection as a BrickLink wanted list (for ordering missing parts)

### Client

#### [NEW] `components/bom-export/` — BomExportComponent

- Modal or panel accessible from project detail pages
- Format selector (BrickLink XML / Rebrickable CSV)
- Live preview of parts list
- "Download" button for each format
- "Compare to My Collection" toggle — highlights missing parts and generates a diff BOM

---

## Phase 3: LDraw Round-Trip

### BMC.LDraw library

#### [NEW] [Writers/LdrWriter.cs](file:///d:/source/repos/scheduler/BMC.LDraw/Writers/LdrWriter.cs)

Write a project's model back to LDraw `.ldr` format:
- Header comments (title, author, BMC metadata)
- One `1` (subfile reference) line per `PlacedBrick` with position/rotation matrix and LDraw part ID + colour code
- Submodel support via `0 FILE` / `0 NOFILE` markers for `.mpd` output

#### [NEW] [Writers/MpdWriter.cs](file:///d:/source/repos/scheduler/BMC.LDraw/Writers/MpdWriter.cs)

Multi-Part Document writer — wraps `LdrWriter` to emit `.mpd` with submodels as embedded subfiles.

### Server

#### Extend [LDrawController.cs](file:///d:/source/repos/scheduler/BMC/BMC.Server/Controllers/LDrawController.cs)

- `GET /api/ldraw/export/{projectId}/ldr` — export project as `.ldr` file download
- `GET /api/ldraw/export/{projectId}/mpd` — export project as `.mpd` file download  
- `POST /api/ldraw/import` — import an `.ldr`/`.mpd` file as a new project (parts resolved via `ldrawPartId` lookup)

### Client

- "Export LDR/MPD" button on Project detail page
- "Import LDR File" option on Dashboard or Projects list

---

## Phasing Strategy

| Phase | Depends On | Estimated Scope |
|-------|-----------|-----------------|
| **Phase 1** | Schema (done) | ~1 controller + 1 component + routing/sidebar |
| **Phase 2** | Phase 1 (collection comparison) | ~1 controller + 1 component/modal |
| **Phase 3** | BMC.LDraw parsers (exist) | ~2 writers + controller endpoints + UI buttons |

Each phase is independently functional — no phase blocks another.

---

## User Review Required

> [!IMPORTANT]
> **Phase 1 question:** Should the "Import Set" flow auto-create a default collection if the user has none, or require manual collection creation first?

> [!IMPORTANT]
> **Phase 2 question:** For BrickLink XML, should we use `<WANTEDLIST>` format (for ordering) or `<INVENTORY>` format (for uploading)?

> [!IMPORTANT]
> **Phase 3 question:** For LDraw import, should we create `PlacedBrick` records for every line-type-1 reference, or only for parts that exist in the BrickPart catalog? (Unknown parts could be flagged for review.)

---

## Verification Plan

### Automated Tests
- `dotnet build` on `BMC.Server` and `BMC.LDraw` after each phase
- `ng build` on `BMC.Client` after each phase

### Browser Testing
- Navigate to My Collection, verify CRUD operations work
- Test BOM export download produces valid XML/CSV
- Round-trip test: export a project as LDR, re-import, verify parts match

### Manual
- Verify BrickLink XML validates against BrickLink's upload format
