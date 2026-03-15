# Foundation Theme & Layout Fixes

## Completed
- [x] Global Bootstrap overrides in `styles.scss` (card, table, form, bg-white, list-group, table-light)
- [x] Audit-event-custom-listing HTML fixes (card bg, pagination footer)
- [x] Incidents-report SCSS: `--bs-*` → `--fnd-*` migration (13 replacements)
- [x] User-activity-insights SCSS: `--bs-*` → `--fnd-*` migration (25 replacements)
- [x] User-custom-table SCSS: hardcoded borders/hovers fixed
- [x] Sidebar icon brightness adjusted (`#64748b` slate-500)

## In Progress
- [/] Standardize layout margins across all sidebar-accessible components
  - [ ] Audit current padding/margin values per component
  - [ ] Pick a consistent standard
  - [ ] Apply to all components
  - [ ] Build verify
