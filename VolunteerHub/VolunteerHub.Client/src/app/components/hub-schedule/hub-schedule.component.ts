import { Component, OnInit, ViewChild } from '@angular/core';
import { FullCalendarComponent } from '@fullcalendar/angular';
import { CalendarOptions, EventInput } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import { HubApiService } from '../../services/hub-api.service';

@Component({
    selector: 'app-hub-schedule',
    templateUrl: './hub-schedule.component.html',
    styleUrls: ['./hub-schedule.component.scss']
})
export class HubScheduleComponent implements OnInit {

    @ViewChild('calendar') calendarComponent!: FullCalendarComponent;

    assignments: any[] = [];
    isLoading = true;
    currentView: string = 'dayGridMonth';
    currentTitle: string = '';
    respondingId: number | null = null;

    calendarOptions: CalendarOptions = {
        plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
        initialView: 'dayGridMonth',
        headerToolbar: false,
        height: 'auto',
        editable: false,
        selectable: false,
        dayMaxEvents: 3,
        eventDisplay: 'block',
        events: [],
        datesSet: (info) => this.handleDatesSet(info),
        eventClick: (info) => this.handleEventClick(info)
    };

    constructor(private api: HubApiService) { }

    ngOnInit(): void {
        // Calendar initializes via datesSet callback
    }

    private handleDatesSet(info: any): void {
        this.currentTitle = info.view.title;
        this.currentView = info.view.type;
    }

    private loadAssignments(): void {
        this.isLoading = true;
        this.api.getMyAssignments().subscribe({
            next: (assigns) => {
                this.assignments = assigns;
                this.updateCalendarEvents();
                this.isLoading = false;
            },
            error: () => this.isLoading = false
        });
    }

    private updateCalendarEvents(): void {
        const events: EventInput[] = this.assignments
            .filter(a => a.startDateTime)
            .map(a => this.mapAssignmentToEvent(a));

        this.calendarOptions = {
            ...this.calendarOptions,
            events
        };
    }

    private mapAssignmentToEvent(assignment: any): EventInput {
        let color = '#0ea5e9';
        let textColor = '#fff';

        if (assignment.approvedHours != null && assignment.approvedHours > 0) {
            color = '#10b981';
        } else if (assignment.reportedHours != null && assignment.reportedHours > 0) {
            color = '#f59e0b';
            textColor = '#1a1a2e';
        }

        return {
            id: String(assignment.id),
            title: assignment.eventName || 'Event',
            start: assignment.startDateTime,
            end: assignment.endDateTime,
            backgroundColor: color,
            borderColor: color,
            textColor,
            extendedProps: {
                assignmentId: assignment.id,
                eventName: assignment.eventName,
                eventDescription: assignment.eventDescription,
                role: assignment.role,
                status: assignment.status,
                reportedHours: assignment.reportedHours,
                approvedHours: assignment.approvedHours,
                notes: assignment.notes,
                startDateTime: assignment.startDateTime,
                endDateTime: assignment.endDateTime
            }
        };
    }

    private handleEventClick(info: any): void {
        const props = info.event.extendedProps;
        const assignment = this.assignments.find(a => a.id === props.assignmentId);
        if (assignment) {
            this.openAssignmentModal(assignment);
        }
    }

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

    getViewLabel(viewName: string): string {
        switch (viewName) {
            case 'dayGridMonth': return 'Month';
            case 'timeGridWeek': return 'Week';
            case 'timeGridDay': return 'Day';
            default: return viewName;
        }
    }

    // Modal state
    selectedAssignment: any = null;
    showModal = false;

    openAssignmentModal(assignment: any): void {
        this.selectedAssignment = assignment;
        this.showModal = true;
    }

    closeModal(): void {
        this.showModal = false;
        this.selectedAssignment = null;
    }

    respond(assignmentId: number, accepted: boolean): void {
        this.respondingId = assignmentId;
        this.api.respondToAssignment(assignmentId, accepted).subscribe({
            next: (result) => {
                const assignment = this.assignments.find(a => a.id === assignmentId);
                if (assignment) {
                    assignment.status = result.status;
                }
                if (this.selectedAssignment && this.selectedAssignment.id === assignmentId) {
                    this.selectedAssignment.status = result.status;
                }
                this.respondingId = null;
                this.updateCalendarEvents();
            },
            error: () => this.respondingId = null
        });
    }

    getStatusClass(status: string): string {
        switch (status) {
            case 'Confirmed': return 'badge-success';
            case 'Declined': return 'badge-danger';
            case 'Planned': return 'badge-warning';
            default: return 'badge-info';
        }
    }
}
