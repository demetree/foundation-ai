# Event Routing Strategy

I've analyzed the codebase and discovered exactly how the "Quick Access" card manages to open the event modal without a dedicated route!

When you click an event in Quick Access, it routes to `/schedule?eventId=X`. The `SchedulerCalendarComponent` listens for this query parameter on initialization, finds the event, and automatically triggers the `modalService.open(EventAddEditModalComponent)` popup on top of the calendar. This is a very common and effective UI pattern for scheduling apps, because it allows users to edit an event while still seeing the schedule backdrop for context!

Because the modal is also heavily used inside the various assignment tabs and the calendar itself, **we should not rip out the modal** like we did for the financial transactions. 

## User Review Required

> [!IMPORTANT]
> **Decision Needed**
> 
> How would you like to proceed?
> 
> **Option A (Recommended): Reuse the Calendar Modal pattern**
> I can update the "Outstanding Deposits" card (and the "This Week's Events" card) to route to `/schedule?eventId=X`. This provides a polished, instant fix that keeps the user in the calendar context when viewing/editing events.
> 
> **Option B: Create a true standalone route**
> If you really want a dedicated full-page route (e.g., `/schedule/events/123`), I can create a new wrapper component (`EventCustomDetailComponent`) that embeds the modal's internal form logic into a full-page layout. However, this separates the user from the calendar view when editing.

## Proposed Changes (Option A)

### Scheduler.Client
#### [MODIFY] `overview-coordinator-tab.component.html`
- Update the `routerLink` for the **Outstanding Deposits** list items from `['/scheduledevents', deposit.eventId]` to `['/schedule']` and add `[queryParams]="{eventId: deposit.eventId}"`.
- Apply the same fix to **This Week's Events** which currently links to `['/scheduledevents', event.id]`.

## Verification Plan
1. Click on an outstanding deposit in the overview tab.
2. Ensure it navigates to the calendar.
3. Verify the Event Add/Edit Modal immediately pops open loaded with the correct event's details.
