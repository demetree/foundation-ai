# System Settings Custom UI Implementation

## Overview

Create new premium System Settings custom components in `components/system-setting-custom/`, using route overrides to take precedence over auto-generated components.

## Approach

**Create new `-custom` components** in the `components/` folder, then add routes **before** the auto-generated routes so they take precedence. This protects customizations from code generation.

---

## Proposed Changes

### New Components

#### [NEW] `components/system-setting-custom/`

| File | Purpose |
|------|---------|
| `system-setting-custom-listing/` | Premium listing with hero header |
| `system-setting-custom-table/` | Table with edit button |
| `system-setting-custom-add-edit/` | Modal-based add/edit |

---

### Routing Override

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app-routing.module.ts)

Add custom routes in the "Custom component routes" section (before auto-generated routes):
```typescript
{ path: 'systemsettings', component: SystemSettingCustomListingComponent, ... },
```

---

### Module Registration

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app.module.ts)

Import and declare the new custom components.

---

## Verification Plan

### Build
```bash
cd g:\source\repos\Scheduler\Foundation\Foundation.Client
npm run build
```

### Manual Testing
- Navigate to `/systemsettings` → should load custom component
- Test add/edit modal flow
- Test search/filter
