# Attributes Field UI Upgrade — Walkthrough

## What Changed

Upgraded 5 entities from raw JSON textarea / no UI to structured editing via `DynamicFieldRendererComponent`:

| Entity | Component | Change |
|---|---|---|
| **Resource** | [resource-custom-add-edit](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/resource-custom/resource-custom-add-edit/resource-custom-add-edit.component.ts) | Textarea → Dynamic Renderer |
| **Office** | [office-custom-add-edit](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-custom-add-edit/office-custom-add-edit.component.ts) | Textarea → Dynamic Renderer |
| **Client** | [client-custom-add-edit](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-custom-add-edit/client-custom-add-edit.component.ts) | Textarea → Dynamic Renderer |
| **SchedulingTarget** | [scheduling-target-custom-detail](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/scheduling-target-custom/scheduling-target-custom-detail/scheduling-target-custom-detail.component.ts) | Textarea → Dynamic Renderer |
| **ScheduledEvent** | [event-add-edit-modal](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.ts) | No UI → Dynamic Renderer |

## Pattern Applied (per entity)

Each component received the same 4-part change matching the existing **Contact** pattern:

1. **`attributesParsed: any = {}`** — property to hold parsed JSON
2. **`onDynamicAttributeChange(data)`** — handler that updates parsed data and marks form dirty
3. **`JSON.parse()` on load** — in `buildFormValues()` / `populateForm()`, with try/catch fallback to `{}`
4. **`JSON.stringify()` on save** — in `submitForm()` / `save()`, only when keys exist

The `<app-dynamic-field-renderer>` component was added to each HTML template with `[entityName]` and `[data]` bindings.

> [!NOTE]
> The raw textarea was **retained** for entities that already had one (Resource, Office, Client), gated behind Foundation Administrator visibility. This serves as a JSON escape hatch.

## Verification

- **`ng build --configuration=development`** — ✅ exit code 0, no new errors (only pre-existing NG8107 optional chain warnings)
