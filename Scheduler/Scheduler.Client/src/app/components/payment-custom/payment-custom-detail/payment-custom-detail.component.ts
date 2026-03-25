// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { PaymentTransactionService, PaymentTransactionData } from '../../../scheduler-data-services/payment-transaction.service';
import { ReceiptHelperService } from '../../../services/receipt-helper.service';
import { PaymentCustomAddEditComponent } from '../payment-custom-add-edit/payment-custom-add-edit.component';


@Component({
    selector: 'app-payment-custom-detail',
    templateUrl: './payment-custom-detail.component.html',
    styleUrls: ['./payment-custom-detail.component.scss']
})
export class PaymentCustomDetailComponent implements OnInit, OnDestroy {

    @ViewChild(PaymentCustomAddEditComponent) addEditComponent!: PaymentCustomAddEditComponent;

    public paymentId: string | null = null;
    public payment: PaymentTransactionData | null = null;
    public isLoading = true;

    private destroy$ = new Subject<void>();

    constructor(
        public paymentService: PaymentTransactionService,
        private route: ActivatedRoute,
        public router: Router,
        private alertService: AlertService,
        private authService: AuthService,
        private receiptHelper: ReceiptHelperService
    ) { }


    ngOnInit(): void {
        this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
            this.paymentId = params.get('paymentTransactionId');
            if (this.paymentId) {
                this.loadData();
            }
        });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    loadData(): void {
        if (!this.paymentId) return;
        this.isLoading = true;

        this.paymentService.GetPaymentTransaction(Number(this.paymentId), true).subscribe({
            next: (data) => {
                if (data) {
                    this.payment = data;
                } else {
                    this.alertService.showMessage('Payment not found', '', MessageSeverity.warn);
                    this.router.navigate(['/finances/payments']);
                }
                this.isLoading = false;
            },
            error: (err) => {
                this.alertService.showMessage('Error loading payment', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
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


    goBack(): void {
        this.router.navigate(['/finances/payments']);
    }


    // P1-6: Create Receipt from Payment
    public creatingReceipt = false;

    createReceipt(): void {
        if (!this.payment || this.creatingReceipt) return;
        this.creatingReceipt = true;

        this.receiptHelper.createFromPayment(Number(this.payment.id)).subscribe({
            next: (result) => {
                this.creatingReceipt = false;
                this.alertService.showMessage(
                    `Receipt ${result.receiptNumber} created`,
                    '', MessageSeverity.success
                );
                this.router.navigate(['/finances/receipts', result.receiptId]);
            },
            error: (err: any) => {
                this.creatingReceipt = false;
                this.alertService.showMessage(
                    'Failed to create receipt',
                    err?.error?.error || err?.message || '',
                    MessageSeverity.error
                );
            }
        });
    }


    openEditModal(): void {
        if (this.addEditComponent && this.payment) {
            this.addEditComponent.openModal(this.payment);
        }
    }


    onPaymentChanged(): void {
        this.paymentService.ClearAllCaches();
        this.loadData();
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
