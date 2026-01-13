import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { SchedulingTargetService, SchedulingTargetData } from '../../../scheduler-data-services/scheduling-target.service';
import { ScheduledEventService, ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';
import { ScheduledEventQueryParameters } from '../../../scheduler-data-services/scheduled-event.service';


interface ProjectSummary {
    id: number;
    name: string;
    target: SchedulingTargetData;
    eventCount: number;
    nextEventDate: Date | null;
    clientName: string;
    typeName: string;
    status: 'On Track' | 'At Risk' | 'Needs Attention'; // Mocked for now
    progress: number; // Mocked for now
}

@Component({
    selector: 'app-overview-manager-tab',
    templateUrl: './overview-manager-tab.component.html',
    styleUrls: ['./overview-manager-tab.component.scss']
})
export class OverviewManagerTabComponent implements OnInit {

    @Input() activeTargets: any[] = []; // Passed from parent to avoid re-fetching

    public projects: ProjectSummary[] = [];
    public loading = true;

    // KPIs
    public totalProjects = 0;
    public projectsAtRisk = 0;
    public totalEvents = 0;

    constructor(
        private router: Router,
        private scheduledEventService: ScheduledEventService
    ) { }

    ngOnInit(): void {
        this.processProjects();
    }

    private processProjects(): void {
        this.loading = true;

        //
        // Transform the raw targets into rich summaries
        //
        this.projects = this.activeTargets.map(t => {
            // Mock logic for status and progress until backend supports it
            const randomStatus = Math.random();
            let status: 'On Track' | 'At Risk' | 'Needs Attention' = 'On Track';
            if (randomStatus > 0.9) status = 'At Risk';
            else if (randomStatus > 0.7) status = 'Needs Attention';

            return {
                target: t, // This might be the summary object from parent, need to match interface
                id: t.id,
                name: t.name,
                eventCount: t.eventCount,
                nextEventDate: null, // Would need to fetch PER project or pass in from parent
                clientName: t.client?.name || 'Unknown Client',
                typeName: t.type || 'Project',
                status: status,
                progress: Math.floor(Math.random() * 100)
            };
        });

        // Update KPIs
        this.totalProjects = this.projects.length;
        this.projectsAtRisk = this.projects.filter(p => p.status !== 'On Track').length;
        this.totalEvents = this.projects.reduce((sum, p) => sum + p.eventCount, 0);

        this.loading = false;
    }

    public navigateToProject(id: number): void {
        this.router.navigate(['/schedulingtargets', id]);
    }

    public getStatusClass(status: string): string {
        switch (status) {
            case 'On Track': return 'bg-success';
            case 'At Risk': return 'bg-danger';
            case 'Needs Attention': return 'bg-warning';
            default: return 'bg-secondary';
        }
    }

    public getStatusTextClass(status: string): string {
        switch (status) {
            case 'On Track': return 'text-success';
            case 'At Risk': return 'text-danger';
            case 'Needs Attention': return 'text-warning';
            default: return 'text-muted';
        }
    }
}
