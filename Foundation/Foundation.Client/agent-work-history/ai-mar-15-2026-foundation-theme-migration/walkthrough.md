# Scheduler Theming — Phase 3 Walkthrough

## Objective
Migrate all remaining Scheduler component SCSS files from hardcoded colors to `--sch-*` theme tokens, completing the full theming system.

## Components Migrated

### Custom Tables (10 files — batch migrated)
All share a common pattern: virtual table container, sticky header, hover effects, scrollbar styling.
- client, resource, crew, contact, calendar, office, volunteer, volunteer-group, shift-pattern*, scheduling-target*, rate-sheet

> [!NOTE]
> `*` = had unique structures requiring individual migration rather than template copy.

### Heavy Components (6 files)
| Component | Lines | Key Changes |
|---|---|---|
| `scheduler-calendar` | 990 | Calendar grid, popovers, conflict panel, sidebar |
| `system-health` | 333 | Metric rows, drive cards, app tabs, modals |
| `financial-dashboard` | 421 | Summary cards, chart bars, transactions, breakdown |
| `change-history-viewer` | 363 | Timeline, diff tables, snapshot viewer |
| `administration` | 106 | Nav tabs, card styling |
| `template-manager` | 310 | Card grid, empty state, meta items |

### Medium Components (7 files)
| Component | Key Changes |
|---|---|
| `invoice-custom-detail` | Summary cards, tabs, line items table, info grid |
| `shift-pattern-custom-detail` | Info cards, weekly preview, timeline |
| `payment-custom-listing` | Filter pills, listing table, mobile cards |
| `accountant-reports` | Tab buttons, accounting table |
| `pnl-report` | PnL sections, net income bar |
| `budget-report` | Report table, summary cards |
| `deposit-manager` | Filter pills, listing table, refund buttons |

### Shared Components (1 file)
- `intelligence-modal` — Section cards, stat grid, social links, hook list

## Design Principle
- **Structural colors** → `--sch-*` tokens (backgrounds, text, borders, shadows)
- **Entity identity gradients** → Preserved hardcoded (scheduler purple, finance teal, invoice indigo, payment amber, etc.)
- **Functional/status colors** → Preserved hardcoded (conflict red, warning amber, success green, diff added/removed, badge colors)

## Build Verification
```
Exit: 0
Errors: 0
```
All ~26 migrated SCSS files compile cleanly with zero TypeScript or SCSS errors.
