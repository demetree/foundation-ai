// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subject, BehaviorSubject, takeUntil } from 'rxjs';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { PaymentTransactionService, PaymentTransactionData, PaymentTransactionQueryParameters } from '../../../scheduler-data-services/payment-transaction.service';
import { PaymentCustomAddEditComponent } from '../payment-custom-add-edit/payment-custom-add-edit.component';


@Component({
    selector: 'app-payment-custom-listing',
    templateUrl: './payment-custom-listing.component.html',
    styleUrls: ['./payment-custom-listing.component.scss']
})
export class PaymentCustomListingComponent implements OnInit, OnDestroy {

    @ViewChild(PaymentCustomAddEditComponent) addEditComponent!: PaymentCustomAddEditComponent;

    public payments: PaymentTransactionData[] = [];
    public filteredPayments: PaymentTransactionData[] = [];
    public isLoading = true;
    public isMobile = false;

    //
    // Filters
    //
    public searchText = '';
    public statusFilter = '';

    //
    // Sorting
    //
    public sortColumn = 'transactionDate';
    public sortDirection: 'asc' | 'desc' = 'desc';

    private destroy$ = new Subject<void>();

    constructor(
        private paymentService: PaymentTransactionService,
        private router: Router,
        private alertService: AlertService,
        private authService: AuthService,
        private breakpointObserver: BreakpointObserver
    ) { }


    ngOnInit(): void {
        this.breakpointObserver.observe([Breakpoints.Handset])
            .pipe(takeUntil(this.destroy$))
            .subscribe(result => this.isMobile = result.matches);

        this.loadPayments();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    loadPayments(): void {
        this.isLoading = true;
        this.paymentService.GetPaymentTransactionList({
            active: true,
            deleted: false,
            includeRelations: true
        }).subscribe({
            next: (data) => {
                this.payments = data ?? [];
                this.applyFilters();
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load payments', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    applyFilters(): void {
        let result = [...this.payments];

        if (this.searchText) {
            const q = this.searchText.toLowerCase();
            result = result.filter(p =>
                p.payerName?.toLowerCase().includes(q) ||
                p.payerEmail?.toLowerCase().includes(q) ||
                p.status?.toLowerCase().includes(q) ||
                p.receiptNumber?.toLowerCase().includes(q) ||
                p.notes?.toLowerCase().includes(q) ||
                p.paymentMethod?.name?.toLowerCase().includes(q)
            );
        }

        if (this.statusFilter) {
            result = result.filter(p => p.status?.toLowerCase() === this.statusFilter.toLowerCase());
        }

        // Sort
        result.sort((a, b) => {
            let va: any, vb: any;
            switch (this.sortColumn) {
                case 'transactionDate': va = a.transactionDate; vb = b.transactionDate; break;
                case 'amount': va = a.amount; vb = b.amount; break;
                case 'status': va = a.status; vb = b.status; break;
                case 'payerName': va = a.payerName; vb = b.payerName; break;
                default: va = a.transactionDate; vb = b.transactionDate;
            }
            if (va == null) return 1;
            if (vb == null) return -1;
            const cmp = va < vb ? -1 : va > vb ? 1 : 0;
            return this.sortDirection === 'asc' ? cmp : -cmp;
        });

        this.filteredPayments = result;
    }


    toggleSort(col: string): void {
        if (this.sortColumn === col) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = col;
            this.sortDirection = col === 'transactionDate' ? 'desc' : 'asc';
        }
        this.applyFilters();
    }


    getSortIcon(col: string): string {
        if (this.sortColumn !== col) return 'fa-sort';
        return this.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    }


    getStatusBadgeClass(status: string | null): string {
        switch (status?.toLowerCase()) {
            case 'completed': return 'badge-completed';
            case 'pending': return 'badge-pending';
            case 'failed': return 'badge-failed';
            case 'refunded': return 'badge-refunded';
            default: return 'badge-pending';
        }
    }


    viewDetail(payment: PaymentTransactionData): void {
        this.router.navigate(['/finances/payments', payment.id]);
    }


    openNewPayment(): void {
        if (this.addEditComponent) {
            this.addEditComponent.openModal();
        }
    }


    onPaymentChanged(): void {
        this.paymentService.ClearAllCaches();
        this.loadPayments();
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
