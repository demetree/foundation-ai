# Fix NG8107 TypeScript Warnings in Foundation.Client

## Tasks
- [x] Identify all warning sources via `ng build`
- [x] Analyze TypeScript interfaces to understand root causes
- [x] Write implementation plan
- [x] Fix `system-health.component.html` — replace `memory?.systemPercent` with `memory.systemPercent` (8 instances)
- [x] Fix `systems-dashboard.component.html` — replace `?.toFixed` with `.toFixed` and `memory?.systemPercent` with `memory.systemPercent` (11 instances)
- [x] Verify build is warning-free
