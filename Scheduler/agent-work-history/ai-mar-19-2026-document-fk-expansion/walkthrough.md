# Document Surface Area Expansion — Walkthrough

## Summary

Added document management integration across 8 detail components and improved the document listing with a flattened "Linked To" column.

## Part 1: Document Tabs on Detail Components

### Tabbed Components (5)

Each got a full Documents tab with badge count, inserted before the History tab:

| Component | Owner Field | Files Changed |
|-----------|------------|---------------|
| [client-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/client-custom/client-custom-detail/client-custom-detail.component.html) | `clientId` | HTML |
| [office-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/office-custom/office-custom-detail/office-custom-detail.component.html) | `officeId` | HTML |
| [crew-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/crew-custom/crew-custom-detail/crew-custom-detail.component.html) | `crewId` | HTML |
| [scheduling-target-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduling-target-custom/scheduling-target-custom-detail/scheduling-target-custom-detail.component.html) | `schedulingTargetId` | HTML |
| [volunteer-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-detail/volunteer-custom-detail.component.html) | `volunteerProfileId` | HTML |

### Non-Tabbed Components (3)

| Component | Approach |
|-----------|---------|
| [invoice-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-detail/invoice-custom-detail.component.html) | New "Documents" tab in custom tab nav |
| [receipt-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/receipt-custom/receipt-custom-detail/receipt-custom-detail.component.html) | Documents section below info grid |
| [payment-custom-detail](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/payment-custom/payment-custom-detail/payment-custom-detail.component.html) | Documents section below info grid |

## Part 2: Listing Column Improvement

Replaced 3 separate owner columns (Contact, Resource, Event) with a single **"Linked To"** column:

- [document-custom-table.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/document-custom/document-custom-table/document-custom-table.component.ts) — `getLinkedTo()` resolves first populated FK into `{label, name, route}`, checks 11 entity types
- [document-custom-table.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/document-custom/document-custom-table/document-custom-table.component.html) — `linkedTo` template in desktop + mobile views: badge label + clickable name
- [document-custom-listing.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/document-custom/document-custom-listing/document-custom-listing.component.ts) — Updated custom columns
- [foundation.utility.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/utility/foundation.utility.ts) — Extended `TableColumn.template` type union

## Verification

✅ `ng build --configuration production` — zero compilation errors (bundle size budget warning is pre-existing)
