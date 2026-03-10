// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { InvoiceService, InvoiceData } from '../../../scheduler-data-services/invoice.service';
import { InvoiceLineItemData } from '../../../scheduler-data-services/invoice-line-item.service';
import { InvoiceHelperService } from '../../../services/invoice-helper.service';
import { InvoiceCustomAddEditComponent } from '../invoice-custom-add-edit/invoice-custom-add-edit.component';


@Component({
    selector: 'app-invoice-custom-detail',
    templateUrl: './invoice-custom-detail.component.html',
    styleUrls: ['./invoice-custom-detail.component.scss']
})
export class InvoiceCustomDetailComponent implements OnInit, OnDestroy {

    @ViewChild(InvoiceCustomAddEditComponent) addEditComponent!: InvoiceCustomAddEditComponent;

    public invoiceId: string | null = null;
    public invoice: InvoiceData | null = null;
    public lineItems: InvoiceLineItemData[] = [];
    public isLoading = true;
    public isMobile = false;
    public activeTab = 'details';
    public isGeneratingPdf = false;

    private destroy$ = new Subject<void>();

    constructor(
        public invoiceService: InvoiceService,
        private invoiceHelper: InvoiceHelperService,
        private route: ActivatedRoute,
        private router: Router,
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
            this.invoiceId = params.get('invoiceId');
            if (this.invoiceId) {
                this.loadData();
            }
        });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    loadData(): void {
        if (!this.invoiceId) return;

        this.isLoading = true;
        const id = Number(this.invoiceId);

        this.invoiceService.GetInvoice(id, true).subscribe({
            next: (data) => {
                if (data) {
                    this.invoice = data;
                    this.loadLineItems();
                } else {
                    this.alertService.showMessage('Invoice not found', '', MessageSeverity.warn);
                    this.router.navigate(['/finances/invoices']);
                }
                this.isLoading = false;
            },
            error: (err) => {
                this.alertService.showMessage('Error loading invoice', JSON.stringify(err), MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private async loadLineItems(): Promise<void> {
        if (!this.invoice) return;
        try {
            this.lineItems = await this.invoice.InvoiceLineItems;
        } catch {
            this.lineItems = [];
        }
    }


    //
    // Status badge
    //
    getStatusBadgeClass(statusName: string | null | undefined): string {
        switch (statusName?.toLowerCase()) {
            case 'draft': return 'badge-draft';
            case 'sent': return 'badge-sent';
            case 'paid': return 'badge-paid';
            case 'partially paid': return 'badge-partial';
            case 'overdue': return 'badge-overdue';
            case 'cancelled': return 'badge-cancelled';
            case 'void': return 'badge-void';
            default: return 'badge-draft';
        }
    }


    //
    // PDF
    //
    generatePdf(): void {
        if (!this.invoice || this.isGeneratingPdf) return;
        this.isGeneratingPdf = true;

        this.invoiceHelper.generatePdf(Number(this.invoice.id)).subscribe({
            next: (blob) => {
                this.invoiceHelper.downloadPdf(blob, this.invoice!.invoiceNumber);
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
        this.router.navigate(['/finances/invoices']);
    }


    openEditModal(): void {
        if (this.addEditComponent && this.invoice) {
            this.addEditComponent.openModal(this.invoice);
        }
    }


    onInvoiceChanged(): void {
        this.loadData();
    }


    //
    // Computed
    //
    get balanceDue(): number {
        if (!this.invoice) return 0;
        return (this.invoice.totalAmount || 0) - (this.invoice.amountPaid || 0);
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
