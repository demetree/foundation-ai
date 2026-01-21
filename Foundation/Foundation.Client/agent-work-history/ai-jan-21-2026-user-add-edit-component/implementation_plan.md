# Custom User Management & Organization Hierarchy

## Overview
Enhance Foundation Admin UI with premium user add/edit capabilities and full organization hierarchy management under tenants.

## Phase 1: Custom User Add/Edit Component

### Goal
Create a premium add/edit experience for SecurityUser with:
- Sectioned form layout (Personal, Contact, Organization, Security)
- Cascading organization dropdowns (Tenant → Org → Dept → Team)
- Optional password set/reset section
- Premium styling matching existing custom components

### Proposed Changes

#### [NEW] user-custom-add-edit/
Create 3 files: `.ts`, `.html`, `.scss`

**Features:**
- **Personal Section**: Name fields, Date of Birth, Title, Reports To
- **Contact Section**: Email, phones (with Tel: links)
- **Organization Section**: Cascading Tenant → Organization → Department → Team
- **Security Section**: Permissions, login flags, 2FA options
- **Status Section**: Active/Deleted (edit mode only)
- Green gradient header matching user-custom theme
- Modal or full-page (configurable)

#### [MODIFY] user-custom-detail.component
- Add "Edit" button in header quick actions
- Wire up to new add/edit component

#### [MODIFY] user-custom-listing.component  
- Add "Add User" button in header
- Wire up to new add/edit component

---

## Phase 2: Organization Hierarchy Management

### Goal
Add Organizations, Departments, and Teams management as tabs within Tenant detail, supporting the full hierarchy.

### Schema Structure
```
SecurityTenant
  └── SecurityOrganization (via securityTenantId)
        └── SecurityDepartment (via securityOrganizationId)
              └── SecurityTeam (via securityDepartmentId)
```

### Proposed Changes

#### [NEW] tenant-organizations-tab/
Shows organizations for the tenant with:
- Table: Name, Description, Departments count, Users count, Status
- Click to open Organization detail (inline or drill-down)
- Add Organization button

#### [NEW] organization-detail-panel/
Reusable panel/modal showing:
- Organization info
- Departments table within this organization
- Users table within this organization
- Add Department button

#### [NEW] department-detail-panel/
Reusable panel/modal showing:
- Department info
- Teams table within this department
- Users table within this department
- Add Team button

#### [MODIFY] tenant-custom-detail.component
- Add "Organizations" tab alongside Overview and Users

---

## Verification Plan

### Phase 1 Testing
1. Open user detail → click Edit → verify form opens with user data
2. Modify fields → save → verify changes persist
3. Open users listing → click Add → verify empty form opens
4. Create new user → verify appears in listing
5. Test cascading dropdowns (select Tenant → Orgs filter)

### Phase 2 Testing
1. Open tenant detail → verify Organizations tab appears
2. Click Organizations tab → verify orgs list loads
3. Click organization → verify departments/teams hierarchy
4. Add organization → verify creation
5. Navigate hierarchy: Tenant → Org → Dept → Team

---

## Implementation Order
1. **user-custom-add-edit** component (Phase 1)
2. Integration with listing/detail (Phase 1)
3. **tenant-organizations-tab** (Phase 2)
4. **organization-detail-panel** (Phase 2)
5. **department-detail-panel** (Phase 2)
