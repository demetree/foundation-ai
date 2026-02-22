# Set Explorer Improvements

## Server Side
- [x] Create `SetExplorerController.cs` — lightweight custom controller with a single GET endpoint
- [x] Create `SetExplorerService.cs` — precomputes and caches a lean DTO list from `BMCContext.LegoSets`
- [x] Register service + controller in `Program.cs`

## Client Side — New Service
- [x] Create `set-explorer-api.service.ts` — fetches from the new endpoint, caches in IndexedDB (24h TTL)

## Client Side — Component Rewrite
- [x] Rewrite `set-explorer.component.ts` — load all sets once, filter/sort client-side, CDK virtual scroll
- [x] Rewrite `set-explorer.component.html` — `cdk-virtual-scroll-viewport` + card rows
- [x] Rewrite `set-explorer.component.scss` — viewport styles, remove pagination styles

## Verification
- [x] Server build
- [x] Client build
- [ ] Manual testing
