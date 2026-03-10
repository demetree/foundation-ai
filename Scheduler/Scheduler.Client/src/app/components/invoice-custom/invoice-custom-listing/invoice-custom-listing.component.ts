// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { InvoiceService, InvoiceData } from '../../../scheduler-data-services/invoice.service';
import { InvoiceStatusService, InvoiceStatusData } from '../../../scheduler-data-services/invoice-status.service';
import { InvoiceHelperService } from '../../../services/invoice-helper.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NavigationService } from '../../../utility-services/navigation.service';


@Component({
    selector: 'app-invoice-custom-listing',
    templateUrl: './invoice-custom-listing.component.html',
    styleUrls: ['./invoice-custom-listing.component.scss']
})
export class InvoiceCustomListingComponent implements OnInit {

    public invoices: InvoiceData[] = [];
    public filteredInvoices: InvoiceData[] = [];
    public statuses: InvoiceStatusData[] = [];

    public isLoading = true;
    public isSmallScreen = false;

    // Filters
    public filterText = '';
    public filterStatusId: number | null = null;

    // Sort
    public sortColumn = 'invoiceDate';
    public sortDirection: 'asc' | 'desc' = 'desc';

    // Counts
    public totalCount = 0;
    public draftCount = 0;
    public sentCount = 0;
    public paidCount = 0;
    public overdueCount = 0;

    private debounceTimeout: any;


    constructor(
        private invoiceService: InvoiceService,
        private invoiceStatusService: InvoiceStatusService,
        private invoiceHelperService: InvoiceHelperService,
        private alertService: AlertService,
        private navigationService: NavigationService,
        private breakpointObserver: BreakpointObserver,
        private router: Router
    ) { }


    ngOnInit(): void {
        this.breakpointObserver
            .observe(['(max-width: 768px)'])
            .subscribe((result) => {
                this.isSmallScreen = result.matches;
            });

        this.loadStatuses();
        this.loadData();
    }


    public loadData(): void {
        this.isLoading = true;

        this.invoiceService.GetInvoiceList({
            active: true,
            deleted: false,
            includeRelations: true,
            pageSize: 10000
        }).subscribe({
            next: (data: InvoiceData[] | null) => {
                this.invoices = data ?? [];
                this.totalCount = this.invoices.length;
                this.updateStatusCounts();
                this.applyFiltersAndSort();
                this.isLoading = false;
            },
            error: (err: any) => {
                this.alertService.showMessage('Error', 'Failed to load invoices', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private loadStatuses(): void {
        this.invoiceStatusService.GetInvoiceStatusList({
            active: true,
            deleted: false,
            pageSize: 100
        }).subscribe({
            next: (data: InvoiceStatusData[] | null) => {
                this.statuses = data ?? [];
            }
        });
    }


    private updateStatusCounts(): void {
        this.draftCount = this.invoices.filter(i => i.invoiceStatus?.name?.toLowerCase() === 'draft').length;
        this.sentCount = this.invoices.filter(i => i.invoiceStatus?.name?.toLowerCase() === 'sent').length;
        this.paidCount = this.invoices.filter(i => i.invoiceStatus?.name?.toLowerCase() === 'paid').length;
        this.overdueCount = this.invoices.filter(i => i.invoiceStatus?.name?.toLowerCase() === 'overdue').length;
    }


    public applyFiltersAndSort(): void {
        let result = [...this.invoices];

        // Status filter
        if (this.filterStatusId !== null) {
            result = result.filter(i => Number(i.invoiceStatusId) === this.filterStatusId);
        }

        // Text filter
        if (this.filterText) {
            const search = this.filterText.toLowerCase();
            result = result.filter(i =>
                (i.invoiceNumber && i.invoiceNumber.toLowerCase().includes(search)) ||
                (i.client?.name && i.client.name.toLowerCase().includes(search)) ||
                (i.notes && i.notes.toLowerCase().includes(search))
            );
        }

        // Sort
        result.sort((a, b) => {
            let valA: any, valB: any;

            switch (this.sortColumn) {
                case 'invoiceDate':
                    valA = a.invoiceDate ? new Date(a.invoiceDate).getTime() : 0;
                    valB = b.invoiceDate ? new Date(b.invoiceDate).getTime() : 0;
                    break;
                case 'dueDate':
                    valA = a.dueDate ? new Date(a.dueDate).getTime() : 0;
                    valB = b.dueDate ? new Date(b.dueDate).getTime() : 0;
                    break;
                case 'totalAmount':
                    valA = Number(a.totalAmount ?? 0);
                    valB = Number(b.totalAmount ?? 0);
                    break;
                case 'invoiceNumber':
                    valA = (a.invoiceNumber ?? '').toLowerCase();
                    valB = (b.invoiceNumber ?? '').toLowerCase();
                    break;
                case 'client':
                    valA = (a.client?.name ?? '').toLowerCase();
                    valB = (b.client?.name ?? '').toLowerCase();
                    break;
                default:
                    valA = 0;
                    valB = 0;
            }

            if (valA < valB) return this.sortDirection === 'asc' ? -1 : 1;
            if (valA > valB) return this.sortDirection === 'asc' ? 1 : -1;
            return 0;
        });

        this.filteredInvoices = result;
    }


    public onFilterChange(): void {
        clearTimeout(this.debounceTimeout);
        this.debounceTimeout = setTimeout(() => {
            this.applyFiltersAndSort();
        }, 100);
    }


    public sortBy(column: string): void {
        if (this.sortColumn === column) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = column;
            this.sortDirection = column === 'invoiceDate' ? 'desc' : 'asc';
        }
        this.applyFiltersAndSort();
    }


    public getSortIcon(column: string): string {
        if (this.sortColumn !== column) return 'fa-sort';
        return this.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    }


    public getStatusBadgeClass(statusName: string | null | undefined): string {
        if (!statusName) return 'status-default';
        switch (statusName.toLowerCase()) {
            case 'draft': return 'status-draft';
            case 'sent': return 'status-sent';
            case 'partially paid': return 'status-partial';
            case 'paid': return 'status-paid';
            case 'overdue': return 'status-overdue';
            case 'cancelled': return 'status-cancelled';
            case 'void': return 'status-void';
            default: return 'status-default';
        }
    }


    public onGeneratePdf(invoice: InvoiceData, event: MouseEvent): void {
        event.stopPropagation();

        this.invoiceHelperService.generatePdf(Number(invoice.id)).subscribe({
            next: (blob: Blob) => {
                this.invoiceHelperService.downloadPdf(blob, invoice.invoiceNumber);
                this.alertService.showMessage('PDF Generated', `Invoice ${invoice.invoiceNumber} PDF downloaded`, MessageSeverity.success);
            },
            error: (err: any) => {
                this.alertService.showMessage('Error', 'Failed to generate invoice PDF', MessageSeverity.error);
            }
        });
    }


    public viewInvoice(invoice: InvoiceData): void {
        this.router.navigate(['/invoices', invoice.id]);
    }


    public goBack(): void {
        this.navigationService.goBack();
    }


    public canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }


    public goToDashboard(): void {
        this.router.navigate(['/finances']);
    }


    public trackById(index: number, item: InvoiceData): number {
        return Number(item.id);
    }


    public getAmountRemaining(invoice: InvoiceData): number {
        return Number(invoice.totalAmount ?? 0) - Number(invoice.amountPaid ?? 0);
    }
}
