// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit, OnDestroy, ViewChild, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { ReceiptService, ReceiptData } from '../../../scheduler-data-services/receipt.service';
import { ReceiptHelperService } from '../../../services/receipt-helper.service';
import { InvoiceService, InvoiceData } from '../../../scheduler-data-services/invoice.service';
import { ReceiptCustomAddEditComponent } from '../receipt-custom-add-edit/receipt-custom-add-edit.component';


@Component({
    selector: 'app-receipt-custom-detail',
    templateUrl: './receipt-custom-detail.component.html',
    styleUrls: ['./receipt-custom-detail.component.scss']
})
export class ReceiptCustomDetailComponent implements OnInit, OnDestroy, AfterViewInit {

    @ViewChild(ReceiptCustomAddEditComponent) addEditComponent!: ReceiptCustomAddEditComponent;

    public receiptId: string | null = null;
    public receipt: ReceiptData | null = null;
    public isLoading = true;
    public isMobile = false;
    public isGeneratingPdf = false;

    // Record Payment flow (P0-4)
    public isNewReceiptMode = false;
    private pendingInvoiceId: number | null = null;

    private destroy$ = new Subject<void>();

    constructor(
        public receiptService: ReceiptService,
        private receiptHelper: ReceiptHelperService,
        private invoiceService: InvoiceService,
        private route: ActivatedRoute,
        public router: Router,
        private alertService: AlertService,
        private authService: AuthService,
        private breakpointObserver: BreakpointObserver
    ) { }


    ngOnInit(): void {
        this.breakpointObserver.observe([Breakpoints.Handset])
            .pipe(takeUntil(this.destroy$))
            .subscribe(result => {
                this.isMobile = result.matches;
            });

        this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
            this.receiptId = params.get('receiptId');
            if (this.receiptId) {
                this.loadData();
            }
        });

        // P0-4: Handle "Record Payment" flow from invoice detail
        this.route.queryParamMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
            const invoiceIdStr = params.get('invoiceId');
            if (invoiceIdStr && !this.receiptId) {
                this.isNewReceiptMode = true;
                this.isLoading = false;
                this.pendingInvoiceId = Number(invoiceIdStr);
                this.openPreSeededModal(this.pendingInvoiceId);
            }
        });
    }


    ngAfterViewInit(): void {
        // If we have a pending invoice from query params and the ViewChild is now available
        if (this.pendingInvoiceId && this.addEditComponent) {
            setTimeout(() => this.openPreSeededModal(this.pendingInvoiceId!), 200);
        }
    }


    /**
     * P0-4: Load the invoice and open add-edit modal pre-seeded with invoice data
     */
    private openPreSeededModal(invoiceId: number): void {
        this.invoiceService.GetInvoice(invoiceId, true).subscribe({
            next: (invoice: InvoiceData) => {
                if (this.addEditComponent && invoice) {
                    // Build a partial ReceiptData to pre-seed the form
                    const preSeed: Partial<ReceiptData> = {
                        invoiceId: invoice.id,
                        clientId: invoice.clientId,
                        amount: Number(invoice.totalAmount ?? 0) - Number(invoice.amountPaid ?? 0),
                        receiptDate: new Date().toISOString(),
                        description: `Payment for Invoice ${invoice.invoiceNumber}`,
                    } as any;

                    // Open the add-edit modal with pre-seeded data (as a new receipt)
                    this.addEditComponent.openModal(preSeed as ReceiptData);
                }
            },
            error: () => {
                this.alertService.showMessage('Could not load invoice for pre-seeding', '', MessageSeverity.warn);
                // Still open the modal, just without pre-seeded data
                if (this.addEditComponent) {
                    this.addEditComponent.openModal();
                }
            }
        });
    }



    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    loadData(): void {
        if (!this.receiptId) return;

        this.isLoading = true;
        const id = Number(this.receiptId);

        this.receiptService.GetReceipt(id, true).subscribe({
            next: (data) => {
                if (data) {
                    this.receipt = data;
                } else {
                    this.alertService.showMessage('Receipt not found', '', MessageSeverity.warn);
                    this.router.navigate(['/finances/receipts']);
                }
                this.isLoading = false;
            },
            error: (err) => {
                this.alertService.showMessage('Error loading receipt', JSON.stringify(err), MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    //
    // PDF
    //
    generatePdf(): void {
        if (!this.receipt || this.isGeneratingPdf) return;
        this.isGeneratingPdf = true;

        this.receiptHelper.generatePdf(Number(this.receipt.id)).subscribe({
            next: (blob) => {
                this.receiptHelper.downloadPdf(blob, this.receipt!.receiptNumber);
                this.isGeneratingPdf = false;
            },
            error: (err) => {
                this.alertService.showMessage('Failed to generate PDF', JSON.stringify(err), MessageSeverity.error);
                this.isGeneratingPdf = false;
            }
        });
    }


    //
    // Navigation
    //
    goBack(): void {
        this.router.navigate(['/finances/receipts']);
    }


    openEditModal(): void {
        if (this.addEditComponent && this.receipt) {
            this.addEditComponent.openModal(this.receipt);
        }
    }


    onReceiptChanged(): void {
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
