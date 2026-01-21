# SecurityTenant Custom Components Implementation

Custom premium UI components for managing Security Tenants in the Foundation Admin application. Tenants are the primary user grouping mechanism, making this a critical administrative feature.

## Proposed Changes

### Component Structure

Following the established `user-custom` and `module-custom` patterns, we'll create:

```
components/tenant-custom/
├── tenant-custom-listing/      # Premium listing page (/tenants)
├── tenant-custom-table/        # Reusable table component
├── tenant-custom-detail/       # Detail page (/tenant/:id) with tabs
├── tenant-overview-tab/        # Overview with statistics
└── tenant-users-tab/           # Users assigned to this tenant
```

---

### [NEW] tenant-custom-listing
Premium listing component with gradient header, search, and tenant statistics.

**Features:**
- Premium gradient header with tenant count
- Search bar for filtering tenants
- Uses `tenant-custom-table` for display

---

### [NEW] tenant-custom-table
Reusable table component for displaying tenants.

**Columns:**
- Name
- Description
- Status (Active/Inactive badge)
- User Count (from `SecurityUsersCount$`)
- Organization Count (from `SecurityOrganizationsCount$`)

**Features:**
- Pagination
- Sorting
- Row click navigation to detail

---

### [NEW] tenant-custom-detail
Detail view with premium header and tabbed interface.

**Features:**
- Premium gradient header with tenant name
- Active/Inactive status badge
- Quick actions (Edit, Toggle Status)
- Tabs: Overview, Users

---

### [NEW] tenant-overview-tab
Overview tab displaying tenant information and statistics.

**Statistics Cards:**
- Total Users
- Total Organizations
- Status (Active/Inactive)

**Info Section:**
- Tenant Name
- Description
- Object GUID

---

### [NEW] tenant-users-tab
Users tab showing all users associated with this tenant.

**Features:**
- Table of users with: Name, Email, Status
- Clickable rows to navigate to user detail (`/user/:id`)
- User count badge in header

---

### Route Integration

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app-routing.module.ts)

Add routes:
- `/tenants` → `TenantCustomListingComponent`
- `/tenant/:id` → `TenantCustomDetailComponent`

---

### Sidebar Integration

#### [MODIFY] [sidebar.component.html](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/sidebar/sidebar.component.html)

Add "Tenants" link to sidebar navigation.

---

### Module Registration

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app.module.ts)

Register all new components in declarations and entryComponents.

---

## Verification Plan

### Build
```powershell
cd g:\source\repos\Scheduler\Foundation\Foundation.Client
npm run lint
ng build
```

### Manual Testing
1. Navigate to `/tenants` - verify premium listing displays
2. Search for a tenant - verify filtering works
3. Click a tenant row - verify navigation to `/tenant/:id`
4. On detail page - verify tabs work (Overview, Users)
5. Click a user in Users tab - verify navigation to `/user/:id`

---

## Data Flow Summary

| Component | Service | Key Data |
|-----------|---------|----------|
| TenantCustomListing | `SecurityTenantService` | `GetSecurityTenantList()`, `GetSecurityTenantsRowCount()` |
| TenantOverviewTab | `SecurityTenantData` | `SecurityUsersCount$`, `SecurityOrganizationsCount$` |
| TenantUsersTab | `SecurityTenantData` | `SecurityUsers$` |
