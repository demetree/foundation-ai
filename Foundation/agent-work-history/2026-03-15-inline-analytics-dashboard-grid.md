# Inline Analytics Dashboard Grid for Login Attempts

**Date:** 2026-03-15

## Summary

Added inline Hourly Distribution and Login Origins analytics cards to the Login Attempts listing page, then restructured the entire content area into a 2×2 dashboard grid layout with integrated header filters, compact stats bar, and dedicated Geographic Origins panel.

## Changes Made

- **`login-attempt-custom-listing.component.ts`** — Added `hourlyChartData`, `hourlyChartOptions` (stacked bar config), and `computeHourlyDistribution()` method (24 hourly bucket success/failure breakdown). Called from existing `computeStats()`.
- **`login-attempt-custom-listing.component.html`** — Full layout rework:
  - Filters moved into an integrated bar inside the header card (replacing separate filter card)
  - Stats compressed from tall bordered cards into a compact horizontal pill strip
  - Content restructured into 2×2 grid: Row 1 = Hourly Distribution + Login Origins map; Row 2 = Login Attempt Detail table (col-lg-7) + Geographic Origins breakdown (col-lg-5)
  - Two instances of `app-login-geo-map`: map-only (top-right) and breakdown-only (bottom-right)
- **`login-attempt-custom-listing.component.scss`** — Complete rewrite: added `.filter-bar`, `.stats-bar` with `.stat-pill` variants, `.inline-analytics-card`, `.geo-breakdown-container` styles. Responsive breakpoints for tablet/mobile.
- **`login-geo-map.component.ts`** — Added `@Input() showMap: boolean = true` and `@Input() showBreakdown: boolean = true` for selective rendering.
- **`login-geo-map.component.html`** — Wrapped map section and breakdown section with `*ngIf` guards for the new inputs. Hid internal "Geographic Breakdown" title when `showMap` is false to avoid duplication with parent card header.

## Key Decisions

- **Two geo-map instances** rather than duplicating breakdown logic — keeps all geographic processing in one component, controlled by input flags
- **col-lg-7 / col-lg-5 split** for Row 2 — gives the table more room for columns while the breakdown table is naturally narrower
- **Integrated filter bar** inside the header gradient — reduces vertical space and keeps controls immediately accessible
- **Compact stat pills** instead of bordered stat cards — significant vertical space savings while keeping all KPIs visible

## Testing / Verification

- Angular build (`ng build --configuration development`) verified clean with no errors after each iteration
- Template type safety issues (nullable async pipe) caught and fixed during build verification
