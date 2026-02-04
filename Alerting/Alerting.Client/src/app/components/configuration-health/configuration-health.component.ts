//
// Configuration Health Checker Component
//
// Dedicated screen for viewing detailed configuration health status.
// Displays the full chain validation with quick-fix navigation.
// AI-assisted development - February 2026
//
import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ConfigurationService } from '../../services/configuration.service';
import { NavigationService } from '../../utility-services/navigation.service';
import { AlertService } from '../../services/alert.service';

//
// DTO Interfaces - matches backend ConfigurationHealthDto
//
interface ConfigurationIssue {
    entityType: string;
    entityId: number;
    entityName: string;
    severity: string;
    description: string;
    quickFixRoute: string;
}

interface ConfigurationHealth {
    overallStatus: string;
    fullyConfiguredCount: number;
    partiallyConfiguredCount: number;
    unconfiguredCount: number;
    issues: ConfigurationIssue[];
}

interface DashboardSummary {
    configurationHealth: ConfigurationHealth;
}

@Component({
    selector: 'app-configuration-health',
    standalone: false,
    templateUrl: './configuration-health.component.html',
    styleUrl: './configuration-health.component.scss'
})
export class ConfigurationHealthComponent implements OnInit, OnDestroy {

    //
    // Data
    //
    health: ConfigurationHealth | null = null;
    loading = true;
    error: string | null = null;

    //
    // Filter
    //
    selectedSeverity: string | null = null;
    selectedEntityType: string | null = null;

    //
    // Subscriptions
    //
    private loadSubscription?: Subscription;

    constructor(
        private http: HttpClient,
        private config: ConfigurationService,
        private authService: AuthService,
        private navigationService: NavigationService,
        private alertService: AlertService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.loadData();
    }

    ngOnDestroy(): void {
        this.loadSubscription?.unsubscribe();
    }

    //
    // Data Loading
    //
    loadData(): void {
        this.loading = true;
        const headers = this.authService.GetAuthenticationHeaders();

        this.loadSubscription = this.http.get<DashboardSummary>(
            `${this.config.baseUrl}/api/Dashboard/summary`,
            { headers }
        ).subscribe({
            next: (data) => {
                this.health = data.configurationHealth;
                this.loading = false;
                this.error = null;
            },
            error: (err) => {
                this.loading = false;
                this.error = 'Failed to load configuration health data.';
                console.error('Configuration health error:', err);
            }
        });
    }

    refresh(): void {
        this.loadData();
    }

    //
    // Navigation
    //
    canGoBack(): boolean {
        return this.navigationService?.canGoBack?.() ?? false;
    }

    goBack(): void {
        this.navigationService?.goBack?.();
    }

    navigateToQuickFix(issue: ConfigurationIssue): void {
        if (issue.quickFixRoute) {
            this.router.navigateByUrl(issue.quickFixRoute);
        }
    }

    //
    // Filtering
    //
    get filteredIssues(): ConfigurationIssue[] {
        if (!this.health?.issues) return [];

        return this.health.issues.filter(issue => {
            if (this.selectedSeverity && issue.severity !== this.selectedSeverity) {
                return false;
            }
            if (this.selectedEntityType && issue.entityType !== this.selectedEntityType) {
                return false;
            }
            return true;
        });
    }

    get uniqueEntityTypes(): string[] {
        if (!this.health?.issues) return [];
        return [...new Set(this.health.issues.map(i => i.entityType))];
    }

    get errorCount(): number {
        return this.health?.issues?.filter(i => i.severity === 'Error').length ?? 0;
    }

    get warningCount(): number {
        return this.health?.issues?.filter(i => i.severity === 'Warning').length ?? 0;
    }

    clearFilters(): void {
        this.selectedSeverity = null;
        this.selectedEntityType = null;
    }

    //
    // Formatting Helpers
    //
    getStatusClass(status: string): string {
        switch (status?.toLowerCase()) {
            case 'healthy': return 'status-healthy';
            case 'warning': return 'status-warning';
            case 'error': return 'status-error';
            default: return '';
        }
    }

    getSeverityClass(severity: string): string {
        switch (severity?.toLowerCase()) {
            case 'error': return 'severity-error';
            case 'warning': return 'severity-warning';
            default: return '';
        }
    }

    getEntityIcon(entityType: string): string {
        switch (entityType?.toLowerCase()) {
            case 'integration': return 'bi bi-plug';
            case 'service': return 'bi bi-server';
            case 'escalationpolicy': return 'bi bi-diagram-3';
            case 'schedule': return 'bi bi-calendar-week';
            default: return 'bi bi-gear';
        }
    }
}
