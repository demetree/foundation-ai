import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';
import { ResourceData } from '../../../scheduler-data-services/resource.service';

interface DispatchSummary {
    unassignedCount: number;
    upcomingShiftsCount: number;
    activeResourcesCount: number;
    conflictsCount: number;
}

@Component({
    selector: 'app-overview-dispatcher-tab',
    templateUrl: './overview-dispatcher-tab.component.html',
    styleUrls: ['./overview-dispatcher-tab.component.scss']
})
export class OverviewDispatcherTabComponent implements OnInit {

    @Input() events: ScheduledEventData[] = [];
    @Input() resources: ResourceData[] = []; // Parent needs to pass this

    public summary: DispatchSummary = {
        unassignedCount: 0,
        upcomingShiftsCount: 0,
        activeResourcesCount: 0,
        conflictsCount: 0
    };

    public unassignedEvents: ScheduledEventData[] = [];
    public upcomingShifts: ScheduledEventData[] = [];

    constructor(private router: Router) { }

    ngOnInit(): void {
        this.processData();
    }

    private processData(): void {
        const now = new Date();

        // Filter for unassigned events (mock logic: no resources assigned - complex to check without includeRelations details sometimes, but checking eventResourceAssignments count if available or assume input is rich)
        // For now, let's assume if it has no assignments it's unassigned.
        // Ideally we need EventResourceAssignments loaded.

        this.unassignedEvents = this.events.filter(e => {
            // Mock check - in reality need to check relations
            return false; // Placeholder until we have assignment data
        }).slice(0, 5);

        // Upcoming shifts (next 24 hours)
        this.upcomingShifts = this.events
            .filter(e => new Date(e.startDateTime) > now && new Date(e.startDateTime).getTime() < now.getTime() + 86400000)
            .slice(0, 5);

        this.summary = {
            unassignedCount: Math.floor(Math.random() * 5), // Mock until logic implemented
            upcomingShiftsCount: this.upcomingShifts.length,
            activeResourcesCount: this.resources.filter(r => r.active).length,
            conflictsCount: 0 // Would need conflict detection service
        };
    }

    public navigateToEvent(id: number): void {
        this.router.navigate(['/scheduledevents', id]);
    }
}
