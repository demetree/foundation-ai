//
// TURN Server Dashboard Component
//
// Displays TURN server status, endpoint information, and active allocations.
// Auto-refreshes on a configurable interval.
//

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import {
    TurnServerService,
    TurnServerStatus,
    TurnAllocationsResponse,
    TurnAllocationSummary
} from '../../services/turn-server.service';


@Component({
    selector: 'app-turn-server-dashboard',
    templateUrl: './turn-server-dashboard.component.html',
    styleUrls: ['./turn-server-dashboard.component.scss']
})
export class TurnServerDashboardComponent implements OnInit, OnDestroy {

    //
    // Lifecycle management
    //
    private destroy$ = new Subject<void>();

    //
    // Loading state
    //
    loading = true;
    error: string | null = null;
    lastUpdated: Date | null = null;

    //
    // Server status
    //
    status: TurnServerStatus | null = null;

    //
    // Allocations
    //
    allocations: TurnAllocationSummary[] = [];
    totalAllocations = 0;

    //
    // Auto-refresh
    //
    autoRefreshEnabled = true;
    autoRefreshInterval = 10;
    autoRefreshCountdown = 10;
    private countdownInterval: ReturnType<typeof setInterval> | null = null;

    //
    // Allocation sort
    //
    sortColumn: 'username' | 'relayPort' | 'lifetimeSeconds' | 'expiresAtUtc' | 'permissionCount' | 'channelCount' = 'expiresAtUtc';
    sortDirection: 'asc' | 'desc' = 'asc';


    constructor(
        private turnService: TurnServerService,
        private location: Location
    ) { }


    ngOnInit(): void {
        this.loadAll();
        this.startAutoRefresh();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.stopAutoRefresh();
    }


    //
    // Data Loading
    //
    loadAll(): void {
        this.loadStatus();
        this.loadAllocations();
    }


    loadStatus(): void {
        this.turnService.getStatus()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (status: TurnServerStatus) => {
                    this.status = status;
                    this.lastUpdated = new Date();
                    this.loading = false;
                    this.error = null;
                },
                error: (err: Error) => {
                    console.error('Failed to load TURN server status:', err);
                    this.error = 'Failed to load TURN server status';
                    this.loading = false;
                }
            });
    }


    loadAllocations(): void {
        this.turnService.getAllocations()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response: TurnAllocationsResponse) => {
                    this.allocations = response.allocations;
                    this.totalAllocations = response.totalCount;
                },
                error: (err: Error) => {
                    console.error('Failed to load allocations:', err);
                }
            });
    }


    refresh(): void {
        this.autoRefreshCountdown = this.autoRefreshInterval;
        this.loadAll();
    }


    //
    // Auto-refresh controls
    //
    toggleAutoRefresh(): void {
        this.autoRefreshEnabled = !this.autoRefreshEnabled;

        if (this.autoRefreshEnabled) {
            this.startAutoRefresh();
        } else {
            this.stopAutoRefresh();
        }
    }


    private startAutoRefresh(): void {
        this.autoRefreshCountdown = this.autoRefreshInterval;

        this.countdownInterval = setInterval(() => {
            if (this.autoRefreshEnabled) {
                this.autoRefreshCountdown--;

                if (this.autoRefreshCountdown <= 0) {
                    this.loadAll();
                    this.autoRefreshCountdown = this.autoRefreshInterval;
                }
            }
        }, 1000);
    }


    private stopAutoRefresh(): void {
        if (this.countdownInterval) {
            clearInterval(this.countdownInterval);
            this.countdownInterval = null;
        }
    }


    //
    // Sorting
    //
    sort(column: typeof this.sortColumn): void {
        if (this.sortColumn === column) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = column;
            this.sortDirection = column === 'expiresAtUtc' ? 'asc' : 'desc';
        }
    }


    getSortedAllocations(): TurnAllocationSummary[] {
        return [...this.allocations].sort((a, b) => {
            let comparison = 0;

            switch (this.sortColumn) {
                case 'username':
                    comparison = (a.username || '').localeCompare(b.username || '');
                    break;
                case 'relayPort':
                    comparison = a.relayPort - b.relayPort;
                    break;
                case 'lifetimeSeconds':
                    comparison = a.lifetimeSeconds - b.lifetimeSeconds;
                    break;
                case 'expiresAtUtc':
                    comparison = new Date(a.expiresAtUtc).getTime() - new Date(b.expiresAtUtc).getTime();
                    break;
                case 'permissionCount':
                    comparison = a.permissionCount - b.permissionCount;
                    break;
                case 'channelCount':
                    comparison = a.channelCount - b.channelCount;
                    break;
            }

            return this.sortDirection === 'asc' ? comparison : -comparison;
        });
    }


    //
    // Helpers
    //
    getStatusClass(): string {
        if (!this.status) return 'status-unknown';
        return this.status.isRunning ? 'status-running' : 'status-stopped';
    }


    getStatusLabel(): string {
        if (!this.status) return 'Unknown';
        return this.status.isRunning ? 'Running' : 'Stopped';
    }


    getTimeRemaining(expiresAtUtc: Date): string {
        const now = new Date().getTime();
        const expires = new Date(expiresAtUtc).getTime();
        const diff = Math.max(0, Math.floor((expires - now) / 1000));

        if (diff <= 0) return 'Expired';

        const minutes = Math.floor(diff / 60);
        const seconds = diff % 60;

        if (minutes > 0) {
            return `${minutes}m ${seconds}s`;
        }

        return `${seconds}s`;
    }


    //
    // Back navigation
    //
    goBack(): void {
        this.location.back();
    }


    canGoBack(): boolean {
        return window.history.length > 1;
    }
}
