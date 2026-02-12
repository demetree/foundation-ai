# Fix NG8107 TypeScript Warnings in Foundation.Client

The Angular compiler reports 14+ NG8107 warnings during `ng build` — all saying the `?.` optional chain operator is unnecessary because the left-hand side is already non-nullable. These are harmless but noisy.

## Proposed Changes

### System Health Component

#### [MODIFY] [system-health.component.html](file:///d:/source/repos/scheduler/Foundation/Foundation.Client/src/app/components/system-health/system-health.component.html)

Lines 159–175: Inside a `*ngIf="healthStatus.application.memory?.systemPercent !== undefined"` block, `memory` is typed as non-optional in `ApplicationMetrics`, so `memory?.systemPercent` → `memory.systemPercent` (8 occurrences on lines 163, 164, 165, 167, 172, 173, 174, 175).

---

### Systems Dashboard Component

#### [MODIFY] [systems-dashboard.component.html](file:///d:/source/repos/scheduler/Foundation/Foundation.Client/src/app/components/systems-dashboard/systems-dashboard.component.html)

- **Line 220**: `avgSystemMemoryPercent?.toFixed(1)` → `.toFixed(1)` — already inside a `*ngIf` guard that confirms it's defined.  
- **Line 274**: `avgSystemCpuPercent?.toFixed(1)` → `.toFixed(1)` — same pattern.  
- **Lines 610–622**: Same `memory?.systemPercent` → `memory.systemPercent` pattern as system-health (8 occurrences).  
- **Line 967**: `nic.utilizationPercent?.toFixed(1)` → `.toFixed(1)` — `utilizationPercent` is `number` (non-optional) in `NetworkInterfaceInfo`.  

## Verification Plan

### Automated Tests

Run `ng build` and confirm zero NG8107 warnings:

```bash
cd d:\source\repos\scheduler\Foundation\Foundation.Client
npx ng build 2>&1 | Select-String "NG8107"
```

Expected: no output (no NG8107 warnings remain).
