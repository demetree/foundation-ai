# Parts Catalog Server Endpoint

A new custom controller endpoint to replace loading all 50K+ BrickParts client-side. Filters to renderable parts (geometryFilePath IS NOT NULL), supports search, category/type filtering, and pagination — all server-side.

## Proposed Changes

### Server

#### [NEW] [PartsCatalogController.cs](file:///d:/source/repos/scheduler/BMC/BMC.Server/Controllers/PartsCatalogController.cs)

Follows the `CollectionController` pattern with `SecureWebAPIController` base.

**DTOs:**
- `CatalogPartDto` — lightweight, flat fields: id, name, ldrawPartId, ldrawTitle, ldrawCategory, categoryId, categoryName, partTypeId, partTypeName, geometryFilePath, keywords, author, widthLdu, heightLdu, depthLdu, massGrams, versionNumber
- `CatalogPageResult` — wraps the list with `totalCount`, `pageSize`, `pageNumber`, `totalPages`, and `items[]`

**Endpoints:**

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/parts-catalog` | Paginated, filtered list with totalCount |
| GET | `/api/parts-catalog/categories` | Categories with counts (only categories that have renderable parts) |

**Baked-in filter:** `WHERE geometryFilePath IS NOT NULL` — always applied.

**Query params:**
- `search` — cross-field text search (name, ldrawPartId, ldrawTitle, keywords, author)
- `categoryId` — filter by category
- `partTypeId` — filter by part type
- `pageSize` (default 48) — items per page
- `pageNumber` (default 1) — page number

---

### Client

#### [NEW] [parts-catalog.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/parts-catalog.service.ts)

Thin Angular service with:
- `getCatalogPage(params)` → `Observable<CatalogPageResult>`
- `getCategories()` → `Observable<CatalogCategoryDto[]>`
- Interfaces for `CatalogPartDto`, `CatalogPageResult`, `CatalogCategoryDto`

#### [MODIFY] [parts-catalog.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.ts)

Major refactor:
- Replace `BrickPartService`, `BrickCategoryService`, `PartTypeService` with `PartsCatalogService`
- Remove `allParts[]` and `filteredParts[]` arrays — no more loading all data
- `displayedParts` becomes `CatalogPartDto[]` from server response
- `applyFilters()` now calls the server endpoint with current filter state
- Page changes call server endpoint with `pageNumber`
- Search debounce calls server endpoint with `search`
- `totalCount`, `totalPages` come from server response
- `buildCategorySidebar()` uses the dedicated categories endpoint
- Remove `IndexedDBCacheService` import (no longer needed — server does the work)

#### [MODIFY] [parts-catalog.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.html)

- Update property references from `BrickPartData` fields to `CatalogPartDto` fields
- `part.brickCategory?.name` → `part.categoryName`
- `part.partType?.name` → `part.partTypeName`

> [!NOTE]
> The isometric SVG rendering functions (`getIsometricPoints`, `getPartDimensions`, etc.) will be adapted to work with `CatalogPartDto` instead of `BrickPartData`. The DTO has all the same fields they need.

## Verification Plan

### Automated Tests
- `dotnet build` passes for BMC.Server
- `ng build --configuration=development` passes for BMC.Client

### Browser Tests
- Parts catalog loads fast with paginated data
- Search filters server-side and returns correct results
- Category sidebar shows counts and filters correctly
- Page navigation works (next/prev/go-to)
- 3D thumbnails still render for visible parts
