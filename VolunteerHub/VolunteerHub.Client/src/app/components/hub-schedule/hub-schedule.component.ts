import { Component, OnInit } from '@angular/core';
import { HubApiService } from '../../services/hub-api.service';

@Component({
    selector: 'app-hub-schedule',
    templateUrl: './hub-schedule.component.html',
    styleUrls: ['./hub-schedule.component.scss']
})
export class HubScheduleComponent implements OnInit {

    assignments: any[] = [];
    filteredAssignments: any[] = [];
    isLoading = true;
    filter: 'upcoming' | 'past' | 'all' = 'upcoming';
    respondingId: number | null = null;

    constructor(private api: HubApiService) { }

    ngOnInit(): void {
        this.loadAssignments();
    }

    private loadAssignments(): void {
        this.isLoading = true;
        this.api.getMyAssignments().subscribe({
            next: (assigns) => {
                this.assignments = assigns;
                this.applyFilter();
                this.isLoading = false;
            },
            error: () => this.isLoading = false
        });
    }

    setFilter(filter: 'upcoming' | 'past' | 'all'): void {
        this.filter = filter;
        this.applyFilter();
    }

    private applyFilter(): void {
        const now = new Date();
        if (this.filter === 'upcoming') {
            this.filteredAssignments = this.assignments.filter(a => new Date(a.startDateTime) >= now);
        } else if (this.filter === 'past') {
            this.filteredAssignments = this.assignments.filter(a => new Date(a.startDateTime) < now);
        } else {
            this.filteredAssignments = [...this.assignments];
        }
    }

    respond(assignmentId: number, accepted: boolean): void {
        this.respondingId = assignmentId;
        this.api.respondToAssignment(assignmentId, accepted).subscribe({
            next: (result) => {
                const assignment = this.assignments.find(a => a.id === assignmentId);
                if (assignment) {
                    assignment.status = result.status;
                }
                this.respondingId = null;
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
