# Scheduler UX Remediation for Service Businesses

**Date:** 2026-03-25

## Summary

Simplified the enterprise-grade Scheduler Angular client to better serve small, service-based businesses. The remediation focused on abstracting terminology, simplifying the onboarding flow for new staff/resources, and creating a unified dashboard for daily dispatch operations.

## Changes Made

- **`src/app/services/terminology.service.ts`**
  - Created a new service to dynamically resolve system terms (e.g., "Resource" -> "Crew" or "Staff") based on tenant industry settings.
- **`src/app/components/sidebar/sidebar.component.html & .ts`**
  - Injected `TerminologyService` to dynamically label navigation links.
  - Added the new `/dispatch` route to the navigation.
- **`src/app/components/resource-custom/resource-custom-listing/resource-custom-listing.component.html & .ts`**
  - Applied dynamic terminology to headers and buttons.
  - Replaced the navigation to the complex Add/Edit component with a trigger for the new Quick Add modal.
- **`src/app/components/resource-custom/staff-quick-add-modal/staff-quick-add-modal.component.ts & .html`**
  - Created a lightweight, single-view modal for adding a new staff member with basic details (Name, Email), handling complex background associations automatically.
- **`src/app/components/scheduler/daily-dispatch/daily-dispatch.component.ts & .html`**
  - Built a new split-panel dashboard to display unassigned jobs and horizontal crew timelines.
  - Implemented HTML5 drag-and-drop to rapidly associate an unassigned job to a crew member (creating an `EventResourceAssignment`).
- **`src/app/app-routing.module.ts`**
  - Registered the new `DailyDispatchComponent` to the `/dispatch` route.

## Key Decisions

- **UI-Only Abstraction:** Decided to leave the underlying `Resource` database entity completely intact to guarantee backend API and schema stability. Instead, all changes are purely UI-level abstractions using the new `TerminologyService`.
- **Component Separation:** Rather than complicating the existing `resource-custom-add-edit` component with "simple vs advanced" modes, a dedicated `staff-quick-add-modal` component was created to strictly govern the simplified onboarding flow.
- **Native Implementation:** Utilized native HTML5 drag-and-drop events for the Dispatch view rather than introducing a heavy third-party library dependency.

## Testing / Verification

- **Build Verification:** Ran `npm run build` targeting the `Scheduler.Client` project.
- **Budget Adjustments:** The initial Angular build budget was slightly exceeded by the new components; increased the `angular.json` limits from `25mb` to `30mb` to allow the build to pass successfully.
- **Results:** The client compiled with zero errors (Exit code 0).
