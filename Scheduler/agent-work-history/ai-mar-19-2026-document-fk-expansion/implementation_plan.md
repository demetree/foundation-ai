# Document Tab Expansion & Listing Improvements

## Part 1 — Add Documents Tab to Custom Detail Components

### Tabbed components (insert before `history` tab)

Each follows the same pattern as [contact-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/contact-custom/contact-custom-detail/contact-custom-detail.component.html#L247-L268):

| Component | Entity var | `ownerField` | `ownerId` | Insert before |
|-----------|-----------|-------------|-----------|--------------|
| [client-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-custom-detail/client-custom-detail.component.html) | `client` | `'clientId'` | `client!.id` | L170 (history) |
| [office-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-custom-detail/office-custom-detail.component.html) | `office` | `'officeId'` | `office!.id` | L239 (history) |
| [crew-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/crew-custom/crew-custom-detail/crew-custom-detail.component.html) | `crew` | `'crewId'` | `crew!.id` | L148 (history) |
| [scheduling-target-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduling-target-custom/scheduling-target-custom-detail/scheduling-target-custom-detail.component.html) | `schedulingTargetData` | `'schedulingTargetId'` | `schedulingTargetData!.id` | L308 (history) |
| [volunteer-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-detail/volunteer-custom-detail.component.html) | `volunteer` | `'volunteerProfileId'` | `volunteer!.id` | L87 (history) |

### Non-tabbed components (add simple section)

These 3 don't use a tabbed layout, so we'll add a collapsible Documents section at the end:

| Component | Entity var | `ownerField` |
|-----------|-----------|-------------|
| [invoice-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-detail/invoice-custom-detail.component.html) | TBD | `'invoiceId'` |
| [receipt-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-detail/receipt-custom-detail.component.html) | TBD | `'receiptId'` |
| [payment-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/payment-custom/payment-custom-detail/payment-custom-detail.component.html) | TBD | `'paymentTransactionId'` |

> [!NOTE]
> `EventDocumentPanelComponent` must be imported/declared in `app.module.ts` for all of these to work — it already is from the earlier session.

---

## Part 2 — Improve Document Listing

### [MODIFY] [document-custom-listing.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/document-custom/document-custom-listing/document-custom-listing.component.ts)

Replace the 3 separate owner-link columns (Contact, Resource, Event) with a **single "Linked To" column** that computes a display value from whichever FK is populated:

```typescript
{ key: '_linkedTo', label: 'Linked To', width: undefined, template: 'computed' }
```

Add a computed getter on each `DocumentData` row that returns e.g. `"Contact: John Smith"`, `"Client: Acme Corp"`, `"Event: Gala 2026"` — whichever is populated.

### [MODIFY] [document-custom-table.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/document-custom/document-custom-table/document-custom-table.component.ts)

- Add a `getLinkedTo(doc)` method that checks nav properties in priority order and returns `{label, name, route}`
- Add a `'computed'` template case in the HTML for clickable linked-to display

### [MODIFY] [document-custom-table.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/document-custom/document-custom-table/document-custom-table.component.html)

- Add `computed` template rendering in both desktop table and mobile card views

## Verification

- Production build (`ng build --configuration production`)
- Visual check of the Documents tab on each component via `ng serve`
