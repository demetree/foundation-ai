# Add Tenant Creation to tenant-custom-listing

Add the ability to create new tenants from the tenant listing page, following the established `organization-add-edit` and `user-custom-listing` modal patterns.

## Proposed Changes

### tenant-custom Component Suite

#### [NEW] [tenant-add-edit.component.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/tenant-custom/tenant-add-edit/tenant-add-edit.component.ts)
#### [NEW] [tenant-add-edit.component.html](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/tenant-custom/tenant-add-edit/tenant-add-edit.component.html)
#### [NEW] [tenant-add-edit.component.scss](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/tenant-custom/tenant-add-edit/tenant-add-edit.component.scss)

New modal component following the `organization-add-edit` pattern:
- NgbModal with `@ViewChild('modalTemplate')` template-driven modal
- `FormGroup` with fields: `name` (required), `description` (optional), `active` (toggle)
- `openForCreate()` and `openForEdit(tenant)` public methods
- Uses `SecurityTenantService.PostSecurityTenant()` / `PutSecurityTenant()`
- `@Output() saved` emits the saved `SecurityTenantData`
- Permission gating via `userIsSecuritySecurityTenantWriter()`
- Styled with the blue gradient header matching the tenant listing theme

---

#### [MODIFY] [tenant-custom-listing.component.html](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/tenant-custom/tenant-custom-listing/tenant-custom-listing.component.html)

- Add an "Add Tenant" glass button in `header-right`, gated by `userIsTenantWriter()`
- Embed `<app-tenant-add-edit>` with `#tenantAddEdit` template ref and `(saved)` event binding

---

#### [MODIFY] [tenant-custom-listing.component.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/tenant-custom/tenant-custom-listing/tenant-custom-listing.component.ts)

- Add `@ViewChild` references for `TenantAddEditComponent` and `TenantCustomTableComponent`
- Replace `navigateToAddTenant()` with `addTenant()` that opens the modal
- Add `onTenantSaved()` handler to refresh counts and table
- Add `userIsTenantWriter()` permission check method via `SecurityTenantService`

---

#### [MODIFY] [tenant-custom-table.component.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/tenant-custom/tenant-custom-table/tenant-custom-table.component.ts)

- Make `loadTenants()` public (it already is, just need to confirm) so the listing can trigger refresh after save

---

### App Module

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app.module.ts)

- Import and declare `TenantAddEditComponent`

## Verification Plan

### Automated Tests
- No existing spec files for custom components in this project
- Run `ng build` from `Foundation.Client` to verify compilation:
  ```
  cd g:\source\repos\Scheduler\Foundation\Foundation.Client
  npx ng build
  ```

### Manual Verification
- The user can verify by running the app, navigating to the Tenants listing, and confirming the "Add Tenant" button appears and opens a modal with Name, Description, and Active fields
