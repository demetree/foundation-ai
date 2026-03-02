import { Component, EventEmitter, OnInit, OnDestroy, Output } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { RebrickableSignalrService, SyncActivityEvent } from '../../services/rebrickable-signalr.service';
import { RebrickableSyncService, SyncTransaction, SyncTransactionsPage, SyncStatus } from '../../services/rebrickable-sync.service';

@Component({
    selector: 'app-rebrickable-activity-panel',
    templateUrl: './rebrickable-activity-panel.component.html',
    styleUrls: ['./rebrickable-activity-panel.component.scss']
})
export class RebrickableActivityPanelComponent implements OnInit, OnDestroy {
    @Output() close = new EventEmitter<void>();
    private destroy$ = new Subject<void>();

    // Status
    status: SyncStatus | null = null;

    // Live events (SignalR, newest first)
    liveEvents: SyncActivityEvent[] = [];

    // Paginated transactions
    transactions: SyncTransaction[] = [];
    totalCount = 0;
    pageSize = 25;
    currentPage = 1;

    // Filters
    searchTerm = '';
    directionFilter: string | null = null;
    successFilter: boolean | null = null;

    // Stats
    totalCalls = 0;
    successRate = 0;
    last24hCount = 0;

    // Export
    exporting = false;

    constructor(
        private signalr: RebrickableSignalrService,
        private syncService: RebrickableSyncService,
        private router: Router
    ) { }


    ngOnInit(): void {
        this.loadStatus();
        this.loadTransactions();

        // Live feed from SignalR
        this.signalr.onSyncActivity$.pipe(takeUntil(this.destroy$)).subscribe(event => {
            this.liveEvents.unshift(event);
            if (this.liveEvents.length > 50) this.liveEvents.pop();
            // Refresh stats
            this.totalCalls++;
        });
    }


    loadStatus(): void {
        this.syncService.getStatus().pipe(takeUntil(this.destroy$)).subscribe({
            next: (s) => {
                this.status = s;
                this.totalCalls = s.totalTransactions;
                this.last24hCount = s.recentErrorCount ?? 0;
                this.successRate = this.totalCalls > 0
                    ? Math.round(((this.totalCalls - (s.recentErrorCount ?? 0)) / this.totalCalls) * 100)
                    : 100;
            }
        });
    }


    loadTransactions(): void {
        this.syncService.getTransactions(
            this.pageSize,
            this.currentPage,
            this.directionFilter ?? undefined,
            this.successFilter ?? undefined
        ).pipe(takeUntil(this.destroy$)).subscribe({
            next: (page: SyncTransactionsPage) => {
                this.transactions = page.results;
                this.totalCount = page.totalCount;
            }
        });
    }


    onSearch(): void {
        this.currentPage = 1;
        this.loadTransactions();
    }


    setDirectionFilter(direction: string | null): void {
        this.directionFilter = this.directionFilter === direction ? null : direction;
        this.currentPage = 1;
        this.loadTransactions();
    }


    setSuccessFilter(success: boolean | null): void {
        this.successFilter = this.successFilter === success ? null : success;
        this.currentPage = 1;
        this.loadTransactions();
    }


    nextPage(): void {
        if (this.currentPage * this.pageSize < this.totalCount) {
            this.currentPage++;
            this.loadTransactions();
        }
    }


    prevPage(): void {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.loadTransactions();
        }
    }


    get totalPages(): number {
        return Math.ceil(this.totalCount / this.pageSize);
    }


    exportCsv(): void {
        this.exporting = true;

        // Load all (up to 10,000) for export
        this.syncService.getTransactions(
            10000, 1,
            this.directionFilter ?? undefined,
            this.successFilter ?? undefined
        ).pipe(takeUntil(this.destroy$)).subscribe({
            next: (page) => {
                const header = 'Date,Direction,Method,Endpoint,Summary,Status,Success,Error,RecordCount\n';
                const rows = page.results.map(t =>
                    [
                        t.transactionDate,
                        t.direction,
                        t.httpMethod,
                        `"${(t.endpoint || '').replace(/"/g, '""')}"`,
                        `"${(t.requestSummary || '').replace(/"/g, '""')}"`,
                        t.responseStatusCode,
                        t.success,
                        `"${(t.errorMessage || '').replace(/"/g, '""')}"`,
                        t.recordCount ?? ''
                    ].join(',')
                ).join('\n');

                const blob = new Blob([header + rows], { type: 'text/csv' });
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `rebrickable-activity-${new Date().toISOString().slice(0, 10)}.csv`;
                a.click();
                URL.revokeObjectURL(url);
                this.exporting = false;
            },
            error: () => {
                this.exporting = false;
            }
        });
    }


    closePanel(): void {
        this.close.emit();
    }


    navigateToIntegrations(): void {
        this.close.emit();
        this.router.navigate(['/integrations']);
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }
}
