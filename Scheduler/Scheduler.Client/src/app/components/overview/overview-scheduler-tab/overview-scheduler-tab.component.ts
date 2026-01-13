import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
    selector: 'app-overview-scheduler-tab',
    templateUrl: './overview-scheduler-tab.component.html',
    styleUrls: ['./overview-scheduler-tab.component.scss']
})
export class OverviewSchedulerTabComponent implements OnInit {

    @Input() daySummaries: any[] = [];
    @Input() potentialConflicts: number = 0;

    constructor(private router: Router) { }

    ngOnInit(): void {
    }

    public getTotalEvents(): number {
        return this.daySummaries.reduce((sum, day) => sum + day.events, 0);
    }

    public navigateToCalendar(): void {
        this.router.navigate(['/calendars']); // Assuming this route exists or similar
    }
}
