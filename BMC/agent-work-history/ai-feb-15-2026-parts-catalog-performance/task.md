# Parts Catalog Performance Fix

## Server Endpoint
- [x] Create `PartsCatalogController.cs` with DTOs, paginated list, and categories endpoint
- [x] Verify server builds (`dotnet build`)

## Client Integration
- [x] Create `parts-catalog.service.ts` with interfaces and service methods
- [x] Refactor `parts-catalog.component.ts` to use server-side pagination
- [x] Update `parts-catalog.component.html` for new DTO field names
- [x] Verify client builds (`ng build`)

## Verification
- [ ] Browser test: catalog loads, paginates, searches, filters
