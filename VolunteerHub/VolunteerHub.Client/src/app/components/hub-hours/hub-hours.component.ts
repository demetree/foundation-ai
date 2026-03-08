import { Component, OnInit } from '@angular/core';
import { HubApiService } from '../../services/hub-api.service';

@Component({
    selector: 'app-hub-hours',
    templateUrl: './hub-hours.component.html',
    styleUrls: ['./hub-hours.component.scss']
})
export class HubHoursComponent implements OnInit {

    assignments: any[] = [];
    isLoading = true;
    totalReported = 0;
    totalApproved = 0;
    pendingCount = 0;

    // Inline report form state
    reportingId: number | null = null;
    reportHours: number | null = null;
    reportNotes = '';
    reportError = '';
    reportSuccess = '';

    constructor(private api: HubApiService) { }

    ngOnInit(): void {
        this.loadHours();
    }

    private loadHours(): void {
        this.isLoading = true;

        // Load past assignments (last 365 days)
        const to = new Date();
        const from = new Date();
        from.setFullYear(from.getFullYear() - 1);

        this.api.getMyAssignments(from, to).subscribe({
            next: (assigns) => {
                this.assignments = assigns.filter(a => new Date(a.startDateTime) < new Date());
                this.calculateTotals();
                this.isLoading = false;
            },
            error: () => this.isLoading = false
        });
    }

    private calculateTotals(): void {
        this.totalReported = this.assignments.reduce((sum, a) => sum + (a.reportedHours || 0), 0);
        this.totalApproved = this.assignments.reduce((sum, a) => sum + (a.approvedHours || 0), 0);
        this.pendingCount = this.assignments.filter(a => a.reportedHours && !a.approvedHours).length;
    }

    startReporting(assignmentId: number): void {
        this.reportingId = assignmentId;
        this.reportHours = null;
        this.reportNotes = '';
        this.reportError = '';
        this.reportSuccess = '';
    }

    cancelReporting(): void {
        this.reportingId = null;
    }

    submitHours(assignmentId: number): void {
        if (!this.reportHours || this.reportHours <= 0) {
            this.reportError = 'Please enter a valid number of hours.';
            return;
        }

        this.reportError = '';

        this.api.reportHours(assignmentId, this.reportHours, this.reportNotes || undefined).subscribe({
            next: () => {
                const a = this.assignments.find(x => x.id === assignmentId);
                if (a) {
                    a.reportedHours = this.reportHours;
                    a.notes = this.reportNotes;
                }
                this.calculateTotals();
                this.reportSuccess = 'Hours reported!';
                setTimeout(() => {
                    this.reportingId = null;
                    this.reportSuccess = '';
                }, 1500);
            },
            error: () => {
                this.reportError = 'Failed to report hours. Please try again.';
            }
        });
    }
}
