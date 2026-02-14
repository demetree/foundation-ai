# Attributes Field UI — Audit & Upgrade Plan

## Audit Findings

### Entities with `attributes` in the Schema

| Entity | Data Service | Add/Edit UI | Table/Display | Implementation Quality |
|--------|-------------|-------------|---------------|----------------------|
| **Contact** | ✅ | ✅ `DynamicFieldRendererComponent` | ✅ Raw display | 🟢 **Best** — JSON-parsed, typed fields via `AttributeDefinitionService` |
| **Resource** | ✅ | ⚠️ Raw `<textarea>` (admin only) | ✅ Column + mobile | 🟡 Works but no structured editing |
| **Office** | ✅ | ⚠️ Raw `<textarea>` (admin only) | ✅ Column + mobile | 🟡 Works but no structured editing |
| **Client** | ✅ | ⚠️ Raw `<textarea>` | ✅ Column + mobile | 🟡 Works but no structured editing |
| **SchedulingTarget** | ✅ | ⚠️ Raw form field | ❌ No display | 🟡 Partial |
| **ScheduledEvent** | ✅ | ❌ None | ❌ None | 🔴 **Gap** — field exists but completely unused |
| **Constituent** | ✅ | ❌ None | ❌ None | 🔴 **Gap** — no UI component exists at all |
| **VolunteerProfile** | ✅ | ❌ None | ❌ None | 🔴 **Gap** — no UI component exists at all |

### Key Observations

1. **Contact is the gold standard** — it uses `DynamicFieldRendererComponent`, which fetches `AttributeDefinitionData` by entity name, rendering typed fields (Text, LongText, Number, Boolean, Date, Select) with validation support.

2. **The `DynamicFieldRendererComponent` is already fully reusable** — it takes `[entityName]` and `[data]` inputs. Any entity can use it by:
   - Parsing `attributes` JSON → object on load
   - Embedding `<app-dynamic-field-renderer>` in the template
   - Serializing the object back to JSON on save

3. **Resource, Office, Client, SchedulingTarget** all use raw `<textarea>` controls that dump/load the JSON string directly. Users can't meaningfully interact with structured data this way.

4. **Constituent and VolunteerProfile** have no custom components at all — they exist only as data services.

---

## Proposed Changes

### Phase 1 — Upgrade Existing Textareas → Dynamic Renderer (4 entities)

For each entity below, the same 3-step pattern applies:

1. **Add `attributesParsed: any = {}`** property to the component
2. **Replace the `<textarea formControlName="attributes">` with `<app-dynamic-field-renderer>`** in the template  
3. **Update load/save logic**: parse JSON on load, `JSON.stringify()` on save (matching the Contact pattern)

> [!IMPORTANT]
> The raw textarea will be **kept** as a fallback for Foundation Administrators behind the existing `*ngIf="userIsFoundationAdministrator()"` guard. This provides a raw JSON escape hatch for power users. The dynamic renderer will be shown to all users above the advanced section.

---

#### [MODIFY] [resource-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/resource-custom/resource-custom-add-edit/resource-custom-add-edit.component.ts)
- Add `attributesParsed` property, `onDynamicAttributeChange()` handler
- Parse `attributes` JSON in `buildFormValues()`, stringify in `submitForm()`

#### [MODIFY] [resource-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/resource-custom/resource-custom-add-edit/resource-custom-add-edit.component.html)
- Add `<app-dynamic-field-renderer [entityName]="'Resource'" ...>` before the Advanced section

---

#### [MODIFY] [office-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-custom-add-edit/office-custom-add-edit.component.ts)
- Same pattern: `attributesParsed`, parse/stringify, handler

#### [MODIFY] [office-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-custom-add-edit/office-custom-add-edit.component.html)
- Add `<app-dynamic-field-renderer [entityName]="'Office'" ...>`

---

#### [MODIFY] [client-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-custom-add-edit/client-custom-add-edit.component.ts)
- Same pattern

#### [MODIFY] [client-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-custom-add-edit/client-custom-add-edit.component.html)
- Add `<app-dynamic-field-renderer [entityName]="'Client'" ...>`

---

#### [MODIFY] [scheduling-target-custom-detail.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/scheduling-target-custom/scheduling-target-custom-detail/scheduling-target-custom-detail.component.ts)
- Same pattern (this one already has `attributes` in its form, just needs the dynamic renderer)

#### [MODIFY] [scheduling-target-custom-detail.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/scheduling-target-custom/scheduling-target-custom-detail/scheduling-target-custom-detail.component.html)
- Add `<app-dynamic-field-renderer [entityName]="'SchedulingTarget'" ...>`

---

### Phase 2 — Add Attributes to ScheduledEvent Modal

#### [MODIFY] [event-add-edit-modal.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.ts)
- Add `attributesParsed` property and handler
- Parse `attributes` JSON when loading event, stringify on save (line ~758 already has `attributes: null`)

#### [MODIFY] [event-add-edit-modal.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.html)
- Add `<app-dynamic-field-renderer [entityName]="'ScheduledEvent'" ...>` in a suitable tab or at the bottom of the Details tab

---

### Out of Scope

- **Constituent** and **VolunteerProfile** have no existing custom UI components at all. Creating entire CRUD views is a separate initiative and not part of this upgrade.

---

## Verification Plan

### Automated
- `ng build --configuration=development` — must pass with exit code 0

### Manual
- For each upgraded entity, verify:
  1. Dynamic fields appear when `AttributeDefinition` records exist for that entity name
  2. Values save and reload correctly (round-trip JSON)
  3. Raw textarea still visible for Foundation Administrators in the Advanced section
  4. Forms without any `AttributeDefinition` records show no "Additional Information" heading (renderer self-hides)
