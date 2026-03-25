# Scheduler UX Remediation Phase 2

**Date:** 2026-03-25

## Summary

Completed Phase 5 and Phase 6 of the Scheduler UX Remediation plan to streamline the application for small, service-based businesses. Specifically, implemented a composite "Quick Add Job" modal that creates a Client, SchedulingTarget, and ScheduledEvent in a single flow, and created a "Navigation Sandbox" (Simple Mode) that aggressively redacts enterprise and accounting features from the sidebar.

## Changes Made

- `g:\source\repos\Scheduler\Scheduler\Scheduler.Client\src\app\components\scheduler\quick-add-job-modal\quick-add-job-modal.component.ts`: Implemented the composite controller logic to chain `ClientService`, `SchedulingTargetService`, and `ScheduledEventService` API calls.
- `g:\source\repos\Scheduler\Scheduler\Scheduler.Client\src\app\components\scheduler\quick-add-job-modal\quick-add-job-modal.component.html`: Designed the single unified form capturing Customer Info, Service Location, and Job Details.
- `g:\source\repos\Scheduler\Scheduler\Scheduler.Client\src\app\components\scheduler\daily-dispatch\daily-dispatch.component.ts`: Wired the "Quick Add Job" modal into the daily dispatch dashboard and added `NgbModal` injection.
- `g:\source\repos\Scheduler\Scheduler\Scheduler.Client\src\app\components\scheduler\daily-dispatch\daily-dispatch.component.html`: Added the primary "Quick Add Job" launcher button to the header actions.
- `g:\source\repos\Scheduler\Scheduler\Scheduler.Client\src\app\components\sidebar\sidebar.component.html`: Updated the `ngIf` bindings on complex enterprise features (Finances, Setup, Messaging Admin, Administration) to completely hide them when `isSimpleMode` is true.

## Key Decisions

- Selected a client-side orchestration approach for the "Quick Add Job" composite form. Synthesizing the Client, SchedulingTarget, and ScheduledEvent directly from the Angular client avoids requiring immediate database schema or backend composite API changes.
- To ensure small business operators aren't visually overwhelmed, decided to completely remove enterprise features from the DOM via Angular structural directives (`*ngIf="!isSimpleMode"`) rather than attempting a complex generic RBAC redaction which might break other parts of the enterprise platform. 

## Testing / Verification

- Validated that the `isSimpleMode` toggle seamlessly redraws the navigation tree, cleanly hiding non-essential sections.
- Verified that all new TypeScript changes compile successfully and the final Angular application bundle remains under the newly established output budgets (`npm run build` completed with Exit Code 0).
