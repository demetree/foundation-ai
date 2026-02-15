# Parts Catalog Server-Side Pagination

## Problem
The Parts Catalog was loading all 50K+ BrickPart records client-side, causing OOM crashes and slow load times. Most of these were Rebrickable-only parts without LDraw geometry files.

## Solution
Created a dedicated server endpoint that filters to renderable parts and handles pagination, search, and category filtering entirely server-side.

## Changes Made

### Server — New Custom Controller

#### [PartsCatalogController.cs](file:///d:/source/repos/scheduler/BMC/BMC.Server/Controllers/PartsCatalogController.cs)

Three endpoints with `WHERE geometryFilePath IS NOT NULL` baked in:

| Endpoint | Purpose |
|----------|---------|
| `GET /api/parts-catalog` | Paginated parts list with search + category/type filters |
| `GET /api/parts-catalog/categories` | Categories with renderable part counts |
| `GET /api/parts-catalog/part-types` | Part types with renderable part counts |

Returns lightweight `CatalogPartDto` objects — flat fields only, no navigation properties.

---

### Client — New Service + Component Refactor

#### [parts-catalog.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/parts-catalog.service.ts)
New Angular service with typed DTOs (`CatalogPart`, `CatalogPageResult`, `CatalogCategory`, `CatalogPartType`).

#### [parts-catalog.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.ts)
Complete rewrite:
- Removed `BrickPartService`, `BrickCategoryService`, `PartTypeService`, `IndexedDBCacheService`
- Removed `allParts[]`, `filteredParts[]` — no more loading all data
- Every search/filter/page change calls `PartsCatalogService.getCatalogPage()`
- Server returns `totalCount`, `totalPages` — no client-side counting

#### [parts-catalog.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.html)
Updated field references: `part.brickCategory?.name` → `part.categoryName`, `part.partType?.name` → `part.partTypeName`, `filteredParts.length` → `totalCount`.

## Verification
- ✅ Server builds clean (`dotnet build`, exit 0)
- ✅ Client builds clean (`ng build`, exit 0)
- ⏳ Browser testing needed (user)
