# Fix NG8107 TypeScript Warnings — Walkthrough

## Problem
`ng build` produced **19 NG8107 warnings** — all about unnecessary `?.` optional chaining where the left-hand side is already non-nullable.

## Changes Made

### [system-health.component.html](file:///d:/source/repos/scheduler/Foundation/Foundation.Client/src/app/components/system-health/system-health.component.html)
- Lines 163–175: Replaced 8× `memory?.systemPercent` → `memory.systemPercent`
  - `memory` is non-optional in `ApplicationMetrics` interface
  - The `*ngIf` guard already confirms `systemPercent` is defined

render_diffs(file:///d:/source/repos/scheduler/Foundation/Foundation.Client/src/app/components/system-health/system-health.component.html)

---

### [systems-dashboard.component.html](file:///d:/source/repos/scheduler/Foundation/Foundation.Client/src/app/components/systems-dashboard/systems-dashboard.component.html)
- Line 220: `avgSystemMemoryPercent?.toFixed(1)` → `.toFixed(1)` (inside `*ngIf` guard)
- Line 274: `avgSystemCpuPercent?.toFixed(1)` → `.toFixed(1)` (inside `*ngIf` guard)
- Lines 610–622: 8× `memory?.systemPercent` → `memory.systemPercent` (same as system-health)
- Line 967: `utilizationPercent?.toFixed(1)` → `.toFixed(1)` (`utilizationPercent` is `number`, not optional)

render_diffs(file:///d:/source/repos/scheduler/Foundation/Foundation.Client/src/app/components/systems-dashboard/systems-dashboard.component.html)

## Verification

`ng build` now produces **zero NG8107 warnings**. Only pre-existing CSS warnings remain (budget exceeded on 2 SCSS files, 4 CSS selector errors — all unrelated).
