# VolunteerHub Audit Fixes

**Date:** 2026-03-10

## Summary

Implemented 8 fixes from the volunteer signup UX audit, addressing critical bugs, UX gaps, and code quality issues across the VolunteerHub client (Angular 17) and Scheduler.Server (VolunteerHubController).

## Changes Made

### Fix 1: Post-Registration Flow
- `hub-register.component.html` — Replaced misleading "Go to Login" with pending-approval steps, admin contact, "Return to Home" button
- `hub-register.component.ts` — Renamed `goToLogin()` → `goHome()`, navigating to `/welcome`
- `hub-register.component.scss` — Styles for pending-steps section and secondary button

### Fix 2: Schedule Bug
- `hub-schedule.component.ts` — Added `this.loadAssignments()` in `ngOnInit()` to fix empty calendar on load

### Fix 3: Login Interstitial for Pending Users
- `VolunteerHubController.cs` — `RequestCode` now checks for pending `VolunteerProfile` by email in `attributes` JSON; returns `status: "pending"`
- `hub-login.component.ts` — Added `'pending'` step type, checks response status
- `hub-login.component.html` — Pending interstitial with ⏳ icon and explanation
- `hub-login.component.scss` — Pending interstitial and secondary button styles

### Fix 4: Opportunities Search/Filter
- `VolunteerHubController.cs` — `GetOpportunities` accepts optional `search`, `fromDate`, `toDate` params; filters by name/description/location and date range
- `hub-api.service.ts` — Updated `getOpportunities()` with filter params
- `hub-opportunities.component.ts` — Debounced search (300ms), date filter state, clear-filters method; added `FormsModule` import
- `hub-opportunities.component.html` — Filter bar with search input, date pickers, clear button
- `hub-opportunities.component.scss` — Filter bar styles

### Fix 5: Landing Page + Org Branding
- `VolunteerHubController.cs` — `GET public/branding` (returns name, colors, contact info, logo URL from `TenantProfile`) and `GET public/branding/logo` (serves logo bytes)
- **[NEW]** `hub-landing/` — 3 files (`.ts`, `.html`, `.scss`): standalone public landing page with hero layout, org branding, register/login CTAs
- `app-routing.module.ts` — Added `/welcome` route; wildcard redirect to `/welcome`
- `hub-shell.component.ts` — Loads branding via `getBranding()`, binds org name + logo
- `hub-shell.component.html` — Dynamic org name and optional logo in header
- `hub-shell.component.scss` — Logo image styles
- `hub-api.service.ts` — Added `getBranding()` method

### Fix 6: Hours CSV Export
- `hub-hours.component.ts` — `exportCsv()` method builds CSV from assignments via `Blob` + `createObjectURL`
- `hub-hours.component.html` — "Download CSV" button in section header

### Fix 7: Calendar Legend
- `hub-schedule.component.html` — Color legend: Assigned (blue `#0ea5e9`), Hours Pending (amber `#f59e0b`), Approved (green `#10b981`)
- `hub-schedule.component.scss` — Legend layout styles

### Fix 8: TypeScript Interfaces
- **[NEW]** `models/hub-models.ts` — Interfaces: `VolunteerProfile`, `VolunteerAssignment`, `Opportunity`, `BrandingInfo`, `OtpRequestResult`, `ProfileUpdateRequest`
- `hub-api.service.ts` — All `Observable<any>` replaced with typed generics
- 5 hub components updated: `hub-dashboard`, `hub-profile`, `hub-opportunities`, `hub-hours`, `hub-schedule`

## Key Decisions

- **Tenant identification for branding**: Uses per-deployment `environment.ts` config rather than DNS-based tenant resolution. The existing `GetDefaultTenantGuidAsync()` works for single-tenant deployments. Multi-tenant would require one hub instance per tenant.
- **Pending detection in login flow**: Server checks `VolunteerProfile.attributes` JSON for the email (since pending volunteers don't have a `SecurityUser` record yet). This avoids creating a separate status-check endpoint.
- **Client-side CSV export**: Chose client-side CSV generation via `Blob` rather than a server endpoint, since the data is already loaded in the component.
- **Landing page as standalone component**: `HubLandingComponent` is standalone (not declared in NgModule) to match the pattern used by `HubOpportunitiesComponent`.

## Testing / Verification

- `dotnet build` on `Scheduler.Server` — **0 errors**, warnings only (pre-existing nullable reference type warnings)
- `ng build` on `VolunteerHub.Client` — **0 errors**, 1 pre-existing CSS warning about `.form-floating>~label` selector
