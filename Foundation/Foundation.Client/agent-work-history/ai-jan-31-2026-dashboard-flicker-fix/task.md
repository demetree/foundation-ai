# Reduce Systems Dashboard Flicker

## Objective
Minimize screen flickering during 1-second auto-refresh by updating DOM values in-place rather than rebuilding the entire view.

## Tasks

- [x] Planning - analyze cause of flicker
- [x] Implement `trackBy` functions for all `*ngFor` loops
- [x] Remove `loading = true` during refreshes (only use for initial load)
- [x] Apply to all sub-tabs (Overview, Real-Time, Historical)
- [x] Build and verify
