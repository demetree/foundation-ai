# Session Information

- **Conversation ID:** dd8e97d4-c80c-4ec4-9092-cd380083a0fe
- **Date:** 2026-02-22
- **Time:** 12:59 NST (UTC-3:30)
- **Duration:** ~25 minutes

## Summary

Refactored the minifig-gallery component from server-paginated `*ngFor` grid to precomputed server endpoint + IndexedDB caching + CDK virtual scroll, following the same pattern as the set-explorer refactor. Year is derived from `LegoSetMinifig` → `LegoSet` join. Default sort: year desc, name asc.

## Files Modified

### Server (New)
- `BMC.Server/Services/MinifigGalleryService.cs` — BackgroundService with derived year
- `BMC.Server/Controllers/MinifigGalleryController.cs` — GET /api/minifig-gallery

### Server (Modified)
- `BMC.Server/Program.cs` — registered MinifigGalleryService

### Client (New)
- `BMC.Client/src/app/services/minifig-gallery-api.service.ts` — API + IndexedDB cache (24h)

### Client (Rewritten)
- `BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.ts`
- `BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.html`
- `BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.scss`

## Related Sessions

- This session continues the IndexedDB caching + CDK virtual scroll work started in the set-explorer refactor (same conversation).
- Prior work in this conversation: parts-universe caching, lego-universe caching.
