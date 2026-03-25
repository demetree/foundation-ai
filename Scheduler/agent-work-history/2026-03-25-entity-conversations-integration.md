# Entity Conversations Integration

**Date:** 2026-03-25

## Summary

Integrated the `app-entity-conversations` component into the 6 primary custom entity detail components (Resource, Office, Client, Contact, Crew, and SchedulingTarget).

## Changes Made

- `client-custom-detail.component.ts` and `client-custom-detail.component.html`: Added FeatureConfigService injection and Conversations tab conditional rendering.
- `contact-custom-detail.component.ts` and `contact-custom-detail.component.html`: Added FeatureConfigService injection and Conversations tab conditional rendering.
- `crew-custom-detail.component.ts` and `crew-custom-detail.component.html`: Added FeatureConfigService injection and Conversations tab conditional rendering.
- `scheduling-target-custom-detail.component.ts` and `scheduling-target-custom-detail.component.html`: Added FeatureConfigService injection and Conversations tab conditional rendering.
- `office-custom-detail.component.ts` and `office-custom-detail.component.html`: Added FeatureConfigService injection and Conversations tab conditional rendering.
- `resource-custom-detail.component.ts` and `resource-custom-detail.component.html`: Added FeatureConfigService injection and Conversations tab conditional rendering.
- `entity-conversations.component.ts`: Updated `@Input() entityId` to accept `number | bigint` to accommodate types passed from entity detail components, and cast internal `getEntityConversations`/`createEntityConversation` calls to `Number()`.

## Key Decisions

- Updated the `app-entity-conversations` component input typing rather than casting all 6 HTML template input bounds via `$any()`, centralizing the type conversion logic.
- Managed the visual presentation of the tab via the `FeatureConfigService.isMessagingEnabled$` runtime feature flag to prevent unauthorized or unintended usage of the messaging subsystem when disabled.

## Testing / Verification

- Validated code correctness by running `npm run build --configuration production` within the `Scheduler.Client` project.
- Verified that all components compile successfully without type-check errors or binding failures.
