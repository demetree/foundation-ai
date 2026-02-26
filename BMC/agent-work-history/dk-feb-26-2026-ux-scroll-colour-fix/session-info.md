# Session Information

- **Conversation ID:** 2fe6da06-b2eb-4b5f-951d-b1f9d982eac5
- **Date:** 2026-02-26
- **Time:** 14:19 NST (UTC-03:30)
- **Duration:** ~15 minutes

## Summary

Applied two UX improvements to BMC.Client: (1) global scroll-to-top on navigation to fix the stale scroll position when drilling into components, and (2) fixed the colour flicker bug in catalog-part-detail where the 3D model would briefly render in default colours before the correct colour was applied.

## Files Modified

- `BMC.Client/src/app/app-routing.module.ts` — Added `scrollPositionRestoration: 'top'` and `anchorScrolling: 'enabled'` to RouterModule.forRoot options
- `BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.ts` — Added `pendingColourReady` flag to defer 3D model visibility until colour data is loaded when a `colourId` query param is present
- `BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.html` — Applied CSS visibility gate on canvas and contextual loading overlay during colour resolution

## Related Sessions

- `dk-feb-26-2026-controller-auditing` — Earlier session in same conversation covering server-side auditing
