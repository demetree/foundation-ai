// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { ReceiptService, ReceiptData } from '../../../scheduler-data-services/receipt.service';
import { ReceiptHelperService } from '../../../services/receipt-helper.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NavigationService } from '../../../utility-services/navigation.service';


@Component({
    selector: 'app-receipt-custom-listing',
    templateUrl: './receipt-custom-listing.component.html',
    styleUrls: ['./receipt-custom-listing.component.scss']
})
export class ReceiptCustomListingComponent implements OnInit {

    public receipts: ReceiptData[] = [];
    public filteredReceipts: ReceiptData[] = [];

    public isLoading = true;
    public isSmallScreen = false;

    // Filters
    public filterText = '';

    // Sort
    public sortColumn = 'receiptDate';
    public sortDirection: 'asc' | 'desc' = 'desc';

    // Counts
    public totalCount = 0;

    private debounceTimeout: any;


    constructor(
        private receiptService: ReceiptService,
        private receiptHelperService: ReceiptHelperService,
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

        this.loadData();
    }


    public loadData(): void {
        this.isLoading = true;

        this.receiptService.GetReceiptList({
            active: true,
            deleted: false,
            includeRelations: true
        }).subscribe({
            next: (data: ReceiptData[] | null) => {
                this.receipts = data ?? [];
                this.totalCount = this.receipts.length;
                this.applyFiltersAndSort();
                this.isLoading = false;
            },
            error: (err: any) => {
                this.alertService.showMessage('Error', 'Failed to load receipts', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    public applyFiltersAndSort(): void {
        let result = [...this.receipts];

        // Text filter
        if (this.filterText) {
            const search = this.filterText.toLowerCase();
            result = result.filter(r =>
                (r.receiptNumber && r.receiptNumber.toLowerCase().includes(search)) ||
                (r.description && r.description.toLowerCase().includes(search)) ||
                (r.paymentMethod && r.paymentMethod.toLowerCase().includes(search)) ||
                (r.notes && r.notes.toLowerCase().includes(search))
            );
        }

        // Sort
        result.sort((a, b) => {
            let valA: any, valB: any;

            switch (this.sortColumn) {
                case 'receiptDate':
                    valA = a.receiptDate ? new Date(a.receiptDate).getTime() : 0;
                    valB = b.receiptDate ? new Date(b.receiptDate).getTime() : 0;
                    break;
                case 'amount':
                    valA = Number(a.amount ?? 0);
                    valB = Number(b.amount ?? 0);
                    break;
                case 'receiptNumber':
                    valA = (a.receiptNumber ?? '').toLowerCase();
                    valB = (b.receiptNumber ?? '').toLowerCase();
                    break;
                default:
                    valA = 0;
                    valB = 0;
            }

            if (valA < valB) return this.sortDirection === 'asc' ? -1 : 1;
            if (valA > valB) return this.sortDirection === 'asc' ? 1 : -1;
            return 0;
        });

        this.filteredReceipts = result;
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
            this.sortDirection = column === 'receiptDate' ? 'desc' : 'asc';
        }
        this.applyFiltersAndSort();
    }


    public getSortIcon(column: string): string {
        if (this.sortColumn !== column) return 'fa-sort';
        return this.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    }


    public onGeneratePdf(receipt: ReceiptData, event: MouseEvent): void {
        event.stopPropagation();

        this.receiptHelperService.generatePdf(Number(receipt.id)).subscribe({
            next: (blob: Blob) => {
                this.receiptHelperService.downloadPdf(blob, receipt.receiptNumber);
                this.alertService.showMessage('PDF Generated', `Receipt ${receipt.receiptNumber} PDF downloaded`, MessageSeverity.success);
            },
            error: (err: any) => {
                this.alertService.showMessage('Error', 'Failed to generate receipt PDF', MessageSeverity.error);
            }
        });
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


    public trackById(index: number, item: ReceiptData): number {
        return Number(item.id);
    }
}
