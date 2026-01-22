# Phase 3: Organization Hierarchy CRUD

## Overview
Add CRUD capabilities for Organization, Department, and Team entities using modal-based add/edit components following the user-custom-add-edit pattern.

## Proposed Changes

### 1. Organization Add/Edit
**[NEW] `organization-add-edit/`**
- Modal form: Name, Description, Active toggle
- Hidden tenant context (passed from parent)
- Uses `SecurityOrganizationService.PostSecurityOrganization` / `PutSecurityOrganization`

---

### 2. Department Add/Edit  
**[NEW] `department-add-edit/`**
- Modal form: Name, Description, Active toggle
- Hidden organization context
- Uses `SecurityDepartmentService`

---

### 3. Team Add/Edit
**[NEW] `team-add-edit/`**
- Modal form: Name, Description, Active toggle
- Hidden department context
- Uses `SecurityTeamService`

---

### Integration Points
| Location | Action |
|----------|--------|
| tenant-organizations-tab | Add "Add Organization" button |
| organization-detail-panel | Add "Add Dept" + row Edit buttons |
| department-detail-panel | Add "Add Team" + row Edit buttons |
| team-detail-panel | Add Edit button in header |

## Form Pattern
Each modal follows:
- Purple/Orange/Teal header matching hierarchy colors
- Simple form: Name (required), Description, Active toggle
- Save/Cancel footer with spinner on save
