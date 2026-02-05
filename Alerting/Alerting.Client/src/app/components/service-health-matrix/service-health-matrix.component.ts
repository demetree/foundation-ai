//
// Service Health Matrix Component
//
// Executive dashboard showing all services with health status,
// active incidents, and on-call responders at a glance.
// AI-assisted development - February 2026
//
import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject, timer, takeUntil, switchMap, forkJoin } from 'rxjs';

import { AuthService } from '../../services/auth.service';
import { ConfigurationService } from '../../services/configuration.service';
import { NavigationService } from '../../utility-services/navigation.service';
import { AlertingUserService, AlertingUser } from '../../services/alerting-user.service';

export interface ServiceHealth {
    serviceId: number;
    serviceObjectGuid: string;
    serviceName: string;
    status: 'Healthy' | 'Degraded' | 'Critical';
    activeIncidentCount: number;
    criticalCount: number;
    highCount: number;
    mediumCount: number;
    escalationPolicyName: string | null;
    onCallUserGuids: string[];
}

@Component({
    selector: 'app-service-health-matrix',
    standalone: false,
    templateUrl: './service-health-matrix.component.html',
    styleUrl: './service-health-matrix.component.scss'
})
export class ServiceHealthMatrixComponent implements OnInit, OnDestroy {
    services: ServiceHealth[] = [];
    isLoading = true;
    errorMessage: string | null = null;
    lastUpdated: Date | null = null;

    // Summary counts
    healthyCount = 0;
    degradedCount = 0;
    criticalCount = 0;

    // User lookup map for resolving GUIDs to display names
    private userMap = new Map<string, AlertingUser>();

    private destroy$ = new Subject<void>();
    private refreshInterval = 30000; // 30 seconds

    constructor(
        private http: HttpClient,
        private config: ConfigurationService,
        private authService: AuthService,
        private navigationService: NavigationService,
        private alertingUserService: AlertingUserService
    ) { }

    ngOnInit(): void {
        this.loadData();
        this.startAutoRefresh();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadData(): void {
        this.isLoading = true;
        this.errorMessage = null;

        const headers = this.authService.GetAuthenticationHeaders();

        // Load both users and service health in parallel
        forkJoin({
            users: this.alertingUserService.getUsers(),
            health: this.http.get<ServiceHealth[]>(
                `${this.config.baseUrl}/api/Dashboard/service-health`,
                { headers }
            )
        }).pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (data) => {
                    // Build user lookup map
                    this.userMap.clear();
                    data.users.forEach(u => this.userMap.set(u.objectGuid, u));

                    this.services = data.health;
                    this.lastUpdated = new Date();
                    this.calculateSummary();
                    this.isLoading = false;
                },
                error: (err) => {
                    console.error('Error loading service health:', err);
                    this.errorMessage = 'Failed to load service health data.';
                    this.isLoading = false;
                }
            });
    }

    private startAutoRefresh(): void {
        timer(this.refreshInterval, this.refreshInterval)
            .pipe(
                takeUntil(this.destroy$),
                switchMap(() => {
                    const headers = this.authService.GetAuthenticationHeaders();
                    return forkJoin({
                        users: this.alertingUserService.getUsers(),
                        health: this.http.get<ServiceHealth[]>(
                            `${this.config.baseUrl}/api/Dashboard/service-health`,
                            { headers }
                        )
                    });
                })
            )
            .subscribe({
                next: (data) => {
                    this.userMap.clear();
                    data.users.forEach(u => this.userMap.set(u.objectGuid, u));

                    this.services = data.health;
                    this.lastUpdated = new Date();
                    this.calculateSummary();
                },
                error: (err) => {
                    console.error('Auto-refresh error:', err);
                }
            });
    }

    private calculateSummary(): void {
        this.healthyCount = this.services.filter(s => s.status === 'Healthy').length;
        this.degradedCount = this.services.filter(s => s.status === 'Degraded').length;
        this.criticalCount = this.services.filter(s => s.status === 'Critical').length;
    }

    refresh(): void {
        this.loadData();
    }

    /**
     * Resolve on-call user GUIDs to display names.
     */
    getOnCallDisplay(service: ServiceHealth): string {
        if (!service.onCallUserGuids || service.onCallUserGuids.length === 0) {
            return service.escalationPolicyName ? 'No one' : 'No policy';
        }

        const names = service.onCallUserGuids
            .map(guid => {
                const user = this.userMap.get(guid);
                return user ? user.displayName : null;
            })
            .filter(n => n != null) as string[];

        if (names.length === 0) {
            return `${service.onCallUserGuids.length} responder(s)`;
        } else if (names.length === 1) {
            return names[0];
        } else if (names.length === 2) {
            return `${names[0]}, ${names[1]}`;
        } else {
            return `${names[0]} + ${names.length - 1} more`;
        }
    }

    // Status helpers
    getStatusClass(status: string): string {
        switch (status) {
            case 'Critical': return 'status-critical';
            case 'Degraded': return 'status-degraded';
            case 'Healthy':
            default: return 'status-healthy';
        }
    }

    getStatusIcon(status: string): string {
        switch (status) {
            case 'Critical': return 'fa-circle-xmark';
            case 'Degraded': return 'fa-triangle-exclamation';
            case 'Healthy':
            default: return 'fa-circle-check';
        }
    }

    getIncidentBreakdown(service: ServiceHealth): string {
        const parts: string[] = [];
        if (service.criticalCount > 0) parts.push(`${service.criticalCount} Crit`);
        if (service.highCount > 0) parts.push(`${service.highCount} High`);
        if (service.mediumCount > 0) parts.push(`${service.mediumCount} Med`);
        return parts.length > 0 ? `(${parts.join(', ')})` : '';
    }

    // Navigation
    goBack(): void {
        this.navigationService.goBack();
    }

    canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }

    trackByServiceId(index: number, service: ServiceHealth): number {
        return service.serviceId;
    }
}
