# LDraw File Cache — Task Checklist

- [x] Create `ldraw-file-cache.service.ts` with THREE.Cache + IndexedDB integration
- [x] Integrate cache into `catalog-part-detail.component.ts`
- [x] Integrate cache into `ldraw-thumbnail.service.ts`
- [x] Register service in `app.module.ts` (not needed — uses `providedIn: 'root'`)
- [x] Verify with `ng build --configuration production`
