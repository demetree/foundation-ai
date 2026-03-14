# MOCHub Pagination and MOC Viewer Fixes

**Date:** 2026-03-14

## Summary

Implemented progressive loading and pagination for the MOCHub Version History tab to ensure all versions (even >20) are accessible. Addressed regressions in the MOC 3D viewer caused by the recent GLB pre-compilation pipeline: restored missing build step playback controls and fixed mirrored rendering in the Photo Booth.

## Changes Made

### MOCHub Version History Pagination
- **`BMC.Server/Controllers/MocHubController.cs`**: Increased the maximum allowed `pageSize` in `GetVersionHistory` endpoint to 500 (from 50).
- **`BMC.Client/src/app/components/mochub-repo/mochub-repo.component.ts`**: Re-implemented `loadVersions()` to support progressive loading (`pageNumber`, `pageSize=50`), tracking `versionsTotalCount` and exposing `hasMoreVersions()`. Added `loadMoreVersions()` logic.
- **`BMC.Client/src/app/components/mochub-repo/mochub-repo.component.html` & `.scss`**: Added a themed "Load More Versions" button with spinner states and a remaining versions counter. Updated the 'Versions' tab badge to reflect total server-side count instead of client-loaded count.

### MOC Viewer Bug Fixes
- **`BMC.Client/src/app/components/moc-viewer/moc-viewer.component.ts`**:
    - **Step Discovery**: Fixed Strategy 1 step discovery for GLB models. Three.js `GLTFLoader` flattens glTF `extras` directly into `userData` (i.e. `userData.stepIndex` instead of `userData.extras.stepIndex`). Relaxed the check to `userData.stepIndex` with fallbacks, restoring the build step playback UI.
    - **Image Rendering Mirrored**: Corrected the `getCameraAngles` azimuth calculation for GLB-loaded models. The GLB exporter pre-flips the Y-axis without rotating the scene (`rotation.x = Math.PI`), which flips the left-right orientation on Z relative to the LDraw load path. Conditionally negated the X component in calculating the azimuth (`atan2(-x, z)`) when `modelLoadedViaGlb` is true, ensuring the server-side render matches the 3D viewer's pose.

## Key Decisions

- **Client-Side Pagination vs Infinite Scroll**: Opted for a "Load More" button approach in the Version History rather than complex infinite scrolling to maintain a simpler, predictable DOM hierarchy for potentially hundreds of lightweight timeline entries.
- **Coordinate Un-Flipping**: Instead of changing the native `RenderService` coordinate system behavior, we correct the client-side camera angle extraction. The server natively expects LDraw's original orientation, so adjusting the azimuth calculation for GLB models cleanly resolves the mirror issue.
- **Robustness in GLB parsing**: Added step parsing from node names (`step_N`) as a fallback in case metadata mapping varies in future Three.js updates.

## Testing / Verification

- Validated pagination handles 150+ version history entries efficiently in blocks of 50.
- Re-tested 3D viewer step playback with large GLB exported model; playback transport controls now display and function properly as steps are discovered.
- Tested Photo Booth rendering from multiple camera angles; image orientation now precisely matches the interactive WebGL view. Angular production build completed with 0 errors.
