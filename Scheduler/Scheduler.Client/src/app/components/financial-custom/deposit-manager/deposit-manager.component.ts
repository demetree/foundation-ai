// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { EventChargeService, EventChargeData, EventChargeSubmitData } from '../../../scheduler-data-services/event-charge.service';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Subject, takeUntil } from 'rxjs';


interface DepositRow {
    charge: EventChargeData;
    eventName: string;
    description: string;
    amount: number;
    isRefunded: boolean;
    refundedDate: string | null;
}


@Component({
    selector: 'app-deposit-manager',
    templateUrl: './deposit-manager.component.html',
    styleUrls: ['./deposit-manager.component.scss']
})
export class DepositManagerComponent implements OnInit {

    public isLoading = true;
    public isMobile = false;
    public deposits: DepositRow[] = [];
    public filteredDeposits: DepositRow[] = [];
    public isRefunding = false;

    //
    // Filters
    //
    public viewFilter: 'outstanding' | 'refunded' | 'all' = 'outstanding';
    public searchText = '';

    //
    // Totals
    //
    public totalOutstanding = 0;
    public totalRefunded = 0;
    public outstandingCount = 0;
    public refundedCount = 0;

    private destroy$ = new Subject<void>();

    constructor(
        private eventChargeService: EventChargeService,
        private alertService: AlertService,
        private authService: AuthService,
        private router: Router,
        private breakpointObserver: BreakpointObserver
    ) { }


    ngOnInit(): void {
        this.breakpointObserver.observe([Breakpoints.Handset])
            .pipe(takeUntil(this.destroy$))
            .subscribe(r => this.isMobile = r.matches);

        this.loadDeposits();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    loadDeposits(): void {
        this.isLoading = true;

        this.eventChargeService.GetEventChargeList({
            active: true,
            deleted: false,
            includeRelations: true,
            isDeposit: true
        } as any).subscribe({
            next: (data) => {
                this.deposits = (data ?? []).map(charge => ({
                    charge,
                    eventName: charge.scheduledEvent?.name ?? 'Unknown Event',
                    description: charge.description ?? 'Deposit',
                    amount: charge.totalAmount ?? 0,
                    isRefunded: !!charge.depositRefundedDate,
                    refundedDate: charge.depositRefundedDate
                }));

                this.outstandingCount = this.deposits.filter(d => !d.isRefunded).length;
                this.refundedCount = this.deposits.filter(d => d.isRefunded).length;
                this.totalOutstanding = this.deposits.filter(d => !d.isRefunded).reduce((s, d) => s + d.amount, 0);
                this.totalRefunded = this.deposits.filter(d => d.isRefunded).reduce((s, d) => s + d.amount, 0);

                this.applyFilters();
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load deposits', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    applyFilters(): void {
        let result = [...this.deposits];

        if (this.viewFilter === 'outstanding') {
            result = result.filter(d => !d.isRefunded);
        } else if (this.viewFilter === 'refunded') {
            result = result.filter(d => d.isRefunded);
        }

        if (this.searchText) {
            const q = this.searchText.toLowerCase();
            result = result.filter(d =>
                d.eventName.toLowerCase().includes(q) ||
                d.description.toLowerCase().includes(q)
            );
        }

        // Sort: outstanding first by oldest, refunded by refund date desc
        result.sort((a, b) => {
            if (!a.isRefunded && !b.isRefunded) {
                return Number(a.charge.id) - Number(b.charge.id);
            }
            return (a.isRefunded ? 1 : 0) - (b.isRefunded ? 1 : 0);
        });

        this.filteredDeposits = result;
    }


    markRefunded(deposit: DepositRow): void {
        if (this.isRefunding) return;
        this.isRefunding = true;

        const submitData = deposit.charge.ConvertToSubmitData();
        submitData.depositRefundedDate = new Date().toISOString();

        this.eventChargeService.PutEventCharge(deposit.charge.id, submitData).subscribe({
            next: () => {
                this.alertService.showMessage('Deposit marked as refunded', '', MessageSeverity.success);
                this.isRefunding = false;
                this.eventChargeService.ClearAllCaches();
                this.loadDeposits();
            },
            error: (err) => {
                this.alertService.showMessage('Failed to refund deposit', JSON.stringify(err), MessageSeverity.error);
                this.isRefunding = false;
            }
        });
    }


    goBack(): void {
        this.router.navigate(['/finances/dashboard']);
    }


    formatCurrency(amount: number | null | undefined): string {
        if (amount == null) return '$0.00';
        return '$' + amount.toFixed(2);
    }


    formatDate(dateStr: string | null | undefined): string {
        if (!dateStr) return '—';
        try {
            return new Date(dateStr).toLocaleDateString('en-CA', {
                year: 'numeric', month: 'short', day: 'numeric'
            });
        } catch {
            return dateStr;
        }
    }
}
