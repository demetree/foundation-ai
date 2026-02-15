/**
 * VolunteerCalendarComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Calendar view of volunteer event assignments using FullCalendar.
 * Can show a single volunteer's assignments (detail tab) or all volunteer assignments (listing view).
 */

import { Component, OnInit, OnDestroy, Input, ViewChild } from '@angular/core';
import { FullCalendarComponent } from '@fullcalendar/angular';
import { CalendarOptions, EventInput } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import { Subject, lastValueFrom } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { EventResourceAssignmentService, EventResourceAssignmentData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-volunteer-calendar',
    templateUrl: './volunteer-calendar.component.html',
    styleUrls: ['./volunteer-calendar.component.scss']
})
export class VolunteerCalendarComponent implements OnInit, OnDestroy {

    @ViewChild('calendar') calendarComponent!: FullCalendarComponent;

    /**
     * When set, shows only assignments for this specific resource.
     * When null/undefined, shows all volunteer assignments.
     */
    @Input() resourceId: bigint | number | null = null;

    public currentView: string = 'dayGridMonth';
    public currentTitle: string = '';
    public isLoading: boolean = true;

    public calendarOptions: CalendarOptions = {
        plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
        initialView: 'dayGridMonth',
        headerToolbar: false, // custom header
        height: 'auto',
        editable: false,
        selectable: false,
        dayMaxEvents: 3,
        eventDisplay: 'block',
        events: [],
        datesSet: (info) => this.handleDatesSet(info),
        eventClick: (info) => this.handleEventClick(info)
    };

    private destroy$ = new Subject<void>();
    private currentStart: string = '';
    private currentEnd: string = '';

    constructor(
        private assignmentService: EventResourceAssignmentService,
        private router: Router
    ) { }

    ngOnInit(): void {
        // Calendar initializes via datesSet callback
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    // --- Header Navigation ---

    navigateCalendar(action: 'prev' | 'next' | 'today'): void {
        const api = this.calendarComponent?.getApi();
        if (!api) return;

        if (action === 'prev') api.prev();
        else if (action === 'next') api.next();
        else api.today();

        this.updateHeaderState(api);
    }

    changeView(viewName: string): void {
        const api = this.calendarComponent?.getApi();
        if (!api) return;

        api.changeView(viewName);
        this.currentView = viewName;
        this.updateHeaderState(api);
    }

    private updateHeaderState(api: any): void {
        this.currentTitle = api.view.title;
    }

    // --- Data Loading ---

    private handleDatesSet(info: any): void {
        this.currentTitle = info.view.title;
        this.currentStart = info.startStr;
        this.currentEnd = info.endStr;
        this.loadAssignments();
    }

    private async loadAssignments(): Promise<void> {
        this.isLoading = true;

        try {
            const query: any = {
                isVolunteer: true,
                includeRelations: true,
                active: true,
                deleted: false
            };

            if (this.resourceId) {
                query.resourceId = this.resourceId;
            }

            const assignments = await lastValueFrom(
                this.assignmentService.GetEventResourceAssignmentList(query)
            );

            const events: EventInput[] = assignments
                .filter(a => a.scheduledEvent)
                .map(a => this.mapAssignmentToEvent(a));

            this.calendarOptions = {
                ...this.calendarOptions,
                events
            };
        } catch (err) {
            console.error('Failed to load volunteer calendar assignments', err);
        } finally {
            this.isLoading = false;
        }
    }

    private mapAssignmentToEvent(assignment: EventResourceAssignmentData): EventInput {
        const event = assignment.scheduledEvent;
        const volunteerName = assignment.resource?.name || 'Unknown';
        const eventName = event?.name || 'Untitled';

        // Determine color based on status
        let color = '#0ea5e9'; // default blue
        let textColor = '#fff';

        if (assignment.approvedVolunteerHours != null && assignment.approvedVolunteerHours > 0) {
            color = '#10b981'; // green - approved
        } else if (assignment.reportedVolunteerHours != null && assignment.reportedVolunteerHours > 0) {
            color = '#f59e0b'; // amber - reported/pending
            textColor = '#1a1a2e';
        }

        return {
            id: String(assignment.id),
            title: this.resourceId
                ? eventName        // On detail page, show event name since volunteer is known
                : `${volunteerName} — ${eventName}`, // On listing, show both
            start: assignment.assignmentStartDateTime || event?.startDateTime || '',
            end: assignment.assignmentEndDateTime || event?.endDateTime || undefined,
            backgroundColor: color,
            borderColor: color,
            textColor,
            extendedProps: {
                assignmentId: assignment.id,
                volunteerId: assignment.resourceId,
                eventId: event?.id,
                volunteerName,
                eventName,
                hours: assignment.reportedVolunteerHours || assignment.approvedVolunteerHours || null,
                status: this.getAssignmentStatus(assignment)
            }
        };
    }

    private getAssignmentStatus(a: EventResourceAssignmentData): string {
        if (a.approvedVolunteerHours != null && a.approvedVolunteerHours > 0) return 'approved';
        if (a.reportedVolunteerHours != null && a.reportedVolunteerHours > 0) return 'reported';
        return 'assigned';
    }

    private handleEventClick(info: any): void {
        const props = info.event.extendedProps;
        if (props?.volunteerId && !this.resourceId) {
            // Navigate to volunteer detail when clicking from listing calendar
            this.router.navigate(['/volunteers', props.volunteerId]);
        }
    }

    // --- View Helpers ---

    getViewLabel(viewName: string): string {
        switch (viewName) {
            case 'dayGridMonth': return 'Month';
            case 'timeGridWeek': return 'Week';
            case 'timeGridDay': return 'Day';
            default: return viewName;
        }
    }
}
