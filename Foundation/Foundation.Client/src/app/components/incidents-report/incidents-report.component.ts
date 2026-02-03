//
// Incidents Report Component
//
// Displays incidents from the Alerting system for this Foundation instance.
//
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, interval } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { IncidentsService, IncidentSummary, IncidentsResponse } from '../../services/incidents.service';

@Component({
    selector: 'app-incidents-report',
    templateUrl: './incidents-report.component.html',
    styleUrls: ['./incidents-report.component.scss']
})
export class IncidentsReportComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Data
    incidents: IncidentSummary[] = [];
    isConfigured = true;
    message: string | null = null;

    // Loading state
    loading = true;
    error: string | null = null;

    // Filters
    selectedHours = 168;  // Default 7 days
    hourOptions = [24, 48, 72, 168, 720];  // 1d, 2d, 3d, 7d, 30d
    selectedStatus = '';
    statusOptions = ['', 'Triggered', 'Acknowledged', 'Resolved'];
    selectedSeverity = '';
    severityOptions = ['', 'Critical', 'High', 'Medium', 'Low', 'Info'];

    // Auto-refresh
    autoRefreshEnabled = false;
    autoRefreshInterval = 30;
    autoRefreshCountdown = 30;

    // Stats
    get triggeredCount(): number {
        return this.incidents.filter(i => i.status === 'Triggered').length;
    }

    get acknowledgedCount(): number {
        return this.incidents.filter(i => i.status === 'Acknowledged').length;
    }

    get resolvedCount(): number {
        return this.incidents.filter(i => i.status === 'Resolved').length;
    }

    get criticalCount(): number {
        return this.incidents.filter(i => i.severity === 'Critical').length;
    }

    constructor(private incidentsService: IncidentsService) { }

    ngOnInit(): void {
        this.loadIncidents();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadIncidents(): void {
        this.loading = true;
        this.error = null;

        const since = new Date();
        since.setHours(since.getHours() - this.selectedHours);

        this.incidentsService.getIncidents({
            since,
            status: this.selectedStatus || undefined,
            severity: this.selectedSeverity || undefined,
            limit: 100
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response: IncidentsResponse) => {
                    this.incidents = response.incidents;
                    this.isConfigured = response.isConfigured;
                    this.message = response.message || null;
                    this.loading = false;
                },
                error: (err: Error) => {
                    this.error = 'Failed to load incidents';
                    this.loading = false;
                    console.error('Incidents load error:', err);
                }
            });
    }

    onFilterChange(): void {
        this.loadIncidents();
    }

    toggleAutoRefresh(): void {
        this.autoRefreshEnabled = !this.autoRefreshEnabled;
        if (this.autoRefreshEnabled) {
            this.startAutoRefresh();
        }
    }

    private startAutoRefresh(): void {
        this.autoRefreshCountdown = this.autoRefreshInterval;
        interval(1000)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
                if (!this.autoRefreshEnabled) return;
                this.autoRefreshCountdown--;
                if (this.autoRefreshCountdown <= 0) {
                    this.loadIncidents();
                    this.autoRefreshCountdown = this.autoRefreshInterval;
                }
            });
    }

    getSeverityClass(severity: string): string {
        switch (severity?.toLowerCase()) {
            case 'critical': return 'severity-critical';
            case 'high': return 'severity-high';
            case 'medium': return 'severity-medium';
            case 'low': return 'severity-low';
            case 'info': return 'severity-info';
            default: return 'severity-medium';
        }
    }

    getStatusClass(status: string): string {
        switch (status?.toLowerCase()) {
            case 'triggered': return 'status-triggered';
            case 'acknowledged': return 'status-acknowledged';
            case 'resolved': return 'status-resolved';
            default: return 'status-triggered';
        }
    }

    getTimeAgo(date: Date): string {
        const now = new Date();
        const past = new Date(date);
        const diffMs = now.getTime() - past.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMins / 60);
        const diffDays = Math.floor(diffHours / 24);

        if (diffDays > 0) return `${diffDays}d ago`;
        if (diffHours > 0) return `${diffHours}h ago`;
        if (diffMins > 0) return `${diffMins}m ago`;
        return 'Just now';
    }

    getHourLabel(hours: number): string {
        if (hours < 24) return `${hours}h`;
        return `${hours / 24}d`;
    }

    // Test Integration
    testingIntegration = false;
    testResult: { success: boolean; message: string } | null = null;

    testIntegration(): void {
        this.testingIntegration = true;
        this.testResult = null;

        this.incidentsService.testIntegration()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    this.testingIntegration = false;
                    this.testResult = {
                        success: response.success,
                        message: response.message || (response.success ? 'Test successful' : 'Test failed')
                    };
                    // Refresh incidents list to show the test incident
                    if (response.success) {
                        setTimeout(() => this.loadIncidents(), 1000);
                    }
                    // Clear result after 5 seconds
                    setTimeout(() => this.testResult = null, 5000);
                },
                error: (err: Error) => {
                    this.testingIntegration = false;
                    this.testResult = {
                        success: false,
                        message: 'Failed to test integration: ' + err.message
                    };
                    setTimeout(() => this.testResult = null, 5000);
                }
            });
    }
}
