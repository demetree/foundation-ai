import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';
import { ResourceData } from '../../../scheduler-data-services/resource.service';
import { EventResourceAssignmentService, EventResourceAssignmentData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { AuthService } from '../../../services/auth.service';

interface DispatchSummary {
    unassignedCount: number;
    upcomingShiftsCount: number;
    activeResourcesCount: number;
}

@Component({
    selector: 'app-overview-dispatcher-tab',
    templateUrl: './overview-dispatcher-tab.component.html',
    styleUrls: ['./overview-dispatcher-tab.component.scss']
})
export class OverviewDispatcherTabComponent implements OnInit, OnChanges {

    @Input() events: ScheduledEventData[] = [];
    @Input() resources: ResourceData[] = [];

    public summary: DispatchSummary = {
        unassignedCount: 0,
        upcomingShiftsCount: 0,
        activeResourcesCount: 0
    };

    public unassignedEvents: ScheduledEventData[] = [];
    public upcomingShifts: ScheduledEventData[] = [];
    public isLoading = true;

    constructor(
        private router: Router,
        private assignmentService: EventResourceAssignmentService,
        public authService: AuthService
    ) { }

    ngOnInit(): void {
        this.loadAssignmentsAndProcess();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if ((changes['events'] || changes['resources']) && !changes['events']?.firstChange) {
            this.loadAssignmentsAndProcess();
        }
    }

    private loadAssignmentsAndProcess(): void {
        this.isLoading = true;

        // Fetch assignments to determine which events are unassigned
        this.assignmentService.GetEventResourceAssignmentList({
            active: true,
            deleted: false
        }).subscribe({
            next: (assignments: EventResourceAssignmentData[]) => {
                this.processData(assignments);
                this.isLoading = false;
            },
            error: () => {
                // On error, still process with empty assignments so the view renders
                this.processData([]);
                this.isLoading = false;
            }
        });
    }

    private processData(assignments: EventResourceAssignmentData[]): void {
        const now = new Date();

        // Build set of event IDs that have at least one assignment
        const assignedEventIds = new Set(
            assignments
                .filter(a => a.scheduledEventId)
                .map(a => Number(a.scheduledEventId))
        );

        // Filter for events today that have no assignments — these are unassigned
        const todayStart = new Date();
        todayStart.setHours(0, 0, 0, 0);
        const todayEnd = new Date();
        todayEnd.setHours(23, 59, 59, 999);

        this.unassignedEvents = this.events
            .filter(e => {
                const start = new Date(e.startDateTime);
                return start >= todayStart && start <= todayEnd && !assignedEventIds.has(Number(e.id));
            })
            .slice(0, 5);

        // Upcoming shifts (next 24 hours)
        this.upcomingShifts = this.events
            .filter(e => {
                const start = new Date(e.startDateTime);
                return start > now && start.getTime() < now.getTime() + 86400000;
            })
            .slice(0, 5);

        this.summary = {
            unassignedCount: this.unassignedEvents.length,
            upcomingShiftsCount: this.upcomingShifts.length,
            activeResourcesCount: this.resources.filter(r => r.active).length
        };
    }

    public navigateToEvent(id: number | bigint): void {
        this.router.navigate(['/scheduledevents', Number(id)]);
    }

    public navigateToDispatch(): void {
        this.router.navigate(['/dailydispatch']);
    }

    public get canWrite(): boolean {
        return this.authService.isSchedulerReaderWriter;
    }
}
