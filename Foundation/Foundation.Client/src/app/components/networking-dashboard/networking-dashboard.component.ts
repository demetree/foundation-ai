//
// Networking Dashboard - Component
//
// Unified dashboard for monitoring all Foundation networking services.
// Displays a grid of service cards with click-to-expand detail panels.
//
// Header pattern matches TURN Server Dashboard and Login Attempts components.
//
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { NetworkingService, NetworkingOverview, ServiceStatusSummary } from '../../services/networking.service';


@Component({
    selector: 'app-networking-dashboard',
    templateUrl: './networking-dashboard.component.html',
    styleUrls: ['./networking-dashboard.component.scss']
})
export class NetworkingDashboardComponent implements OnInit, OnDestroy {

    overview: NetworkingOverview | null = null;
    loading = false;
    error: string | null = null;
    lastUpdated: Date | null = null;

    // Auto-refresh (matches TURN server pattern)
    autoRefreshEnabled = true;
    autoRefreshCountdown = 0;
    private autoRefreshInterval: any;
    private countdownInterval: any;
    private readonly REFRESH_SECONDS = 15;

    // Expanded detail
    expandedService: string | null = null;
    serviceDetail: any = null;
    detailLoading = false;


    constructor(
        private networkingService: NetworkingService,
        private router: Router,
        private location: Location
    ) { }


    ngOnInit(): void {
        this.refresh();
        this.startAutoRefresh();
    }


    ngOnDestroy(): void {
        this.stopAutoRefresh();
    }


    // ── Navigation (matches TURN Server pattern) ───────────────────────


    canGoBack(): boolean {
        return true;
    }


    goBack(): void {
        this.location.back();
    }


    // ── Data Loading ───────────────────────────────────────────────────


    refresh(): void {
        this.loading = true;
        this.error = null;

        this.networkingService.getOverview().subscribe({
            next: (data) => {
                this.overview = data;
                this.loading = false;
                this.lastUpdated = new Date();
                this.resetCountdown();
            },
            error: (err) => {
                this.error = 'Unable to retrieve networking status. The server may be unavailable.';
                this.loading = false;
                console.error('Networking overview error:', err);
            }
        });
    }


    // ── Auto-Refresh (matches TURN Server pattern) ─────────────────────


    toggleAutoRefresh(): void {
        this.autoRefreshEnabled = !this.autoRefreshEnabled;

        if (this.autoRefreshEnabled) {
            this.startAutoRefresh();
        } else {
            this.stopAutoRefresh();
        }
    }


    private startAutoRefresh(): void {
        this.stopAutoRefresh();

        if (!this.autoRefreshEnabled) return;

        this.resetCountdown();

        this.autoRefreshInterval = setInterval(() => {
            this.refresh();
        }, this.REFRESH_SECONDS * 1000);

        this.countdownInterval = setInterval(() => {
            if (this.autoRefreshCountdown > 0) {
                this.autoRefreshCountdown--;
            }
        }, 1000);
    }


    private stopAutoRefresh(): void {
        if (this.autoRefreshInterval) {
            clearInterval(this.autoRefreshInterval);
            this.autoRefreshInterval = null;
        }
        if (this.countdownInterval) {
            clearInterval(this.countdownInterval);
            this.countdownInterval = null;
        }
    }


    private resetCountdown(): void {
        this.autoRefreshCountdown = this.REFRESH_SECONDS;
    }


    // ── Detail Panel ───────────────────────────────────────────────────


    toggleExpand(service: ServiceStatusSummary): void {
        if (this.expandedService === service.name) {
            this.closeDetail();
            return;
        }

        this.expandedService = service.name;
        this.detailLoading = true;
        this.serviceDetail = null;

        const serviceKey = service.name.toLowerCase().replace(/\s+/g, '');

        this.networkingService.getServiceDetail(serviceKey).subscribe({
            next: (data) => {
                this.serviceDetail = data;
                this.detailLoading = false;
            },
            error: () => {
                this.serviceDetail = { error: 'Failed to load detail data' };
                this.detailLoading = false;
            }
        });
    }


    closeDetail(): void {
        this.expandedService = null;
        this.serviceDetail = null;
    }


    getExpandedServiceIcon(): string {
        if (!this.overview || !this.expandedService) return '';
        const svc = this.overview.services.find(s => s.name === this.expandedService);
        return svc ? svc.icon : '';
    }
}
