import { Component, OnInit, ViewChild, Inject } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { FinancialTransactionService, FinancialTransactionData } from '../../../scheduler-data-services/financial-transaction.service';
import { FinancialCategoryService, FinancialCategoryData } from '../../../scheduler-data-services/financial-category.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { FinancialTransactionCustomAddEditComponent } from '../financial-transaction-custom-add-edit/financial-transaction-custom-add-edit.component';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';


@Component({
    selector: 'app-financial-transaction-custom-listing',
    templateUrl: './financial-transaction-custom-listing.component.html',
    styleUrls: ['./financial-transaction-custom-listing.component.scss']
})
export class FinancialTransactionCustomListingComponent implements OnInit {

    public transactions: FinancialTransactionData[] = [];
    public filteredTransactions: FinancialTransactionData[] = [];
    public categories: FinancialCategoryData[] = [];

    public isLoading = true;
    public isSmallScreen = false;

    // Filter state
    public filterText = '';
    public filterType: 'all' | 'income' | 'expense' = 'all';
    public filterCategoryId: number | null = null;

    // Sort state
    public sortColumn = 'transactionDate';
    public sortDirection: 'asc' | 'desc' = 'desc';

    // Counts
    public totalCount = 0;
    public incomeCount = 0;
    public expenseCount = 0;

    private debounceTimeout: any;

    // Office filter
    public offices: FinancialOfficeData[] = [];
    public selectedOfficeId: number | null = null;

    // Void state
    public voidTarget: FinancialTransactionData | null = null;
    public voidReason = '';
    public isVoiding = false;

    constructor(
        private transactionService: FinancialTransactionService,
        private categoryService: FinancialCategoryService,
        private financialOfficeService: FinancialOfficeService,
        private alertService: AlertService,
        private authService: AuthService,
        private navigationService: NavigationService,
        private breakpointObserver: BreakpointObserver,
        private router: Router,
        private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string
    ) { }

    @ViewChild('txAddEdit') txAddEdit!: FinancialTransactionCustomAddEditComponent;


    ngOnInit(): void {
        this.breakpointObserver
            .observe(['(max-width: 768px)'])
            .subscribe((result) => {
                this.isSmallScreen = result.matches;
            });

        this.loadData();
        this.loadCategories();
        this.loadOffices();
    }


    public loadData(): void {
        this.isLoading = true;

        this.transactionService.GetFinancialTransactionList({
            active: true,
            deleted: false,
            includeRelations: true,
            // TODO: Replace with server-side pagination when transaction volume grows
            pageSize: 10000
        }).subscribe({
            next: (data: FinancialTransactionData[] | null) => {
                this.transactions = data ?? [];
                this.totalCount = this.transactions.length;
                this.incomeCount = this.transactions.filter(t => t.isRevenue).length;
                this.expenseCount = this.transactions.filter(t => !t.isRevenue).length;
                this.applyFiltersAndSort();
                this.isLoading = false;
            },
            error: (err: any) => {
                this.alertService.showMessage('Error', 'Failed to load transactions', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private loadCategories(): void {
        this.categoryService.GetFinancialCategoryList({
            active: true,
            deleted: false,
            pageSize: 500
        }).subscribe({
            next: (data: FinancialCategoryData[] | null) => {
                this.categories = data ?? [];
            }
        });
    }


    private loadOffices(): void {
        this.financialOfficeService.GetFinancialOfficeList({
            active: true,
            deleted: false,
            pageSize: 100
        }).subscribe({
            next: (data: FinancialOfficeData[] | null) => {
                this.offices = data ?? [];
            }
        });
    }


    public onOfficeChange(): void {
        this.applyFiltersAndSort();
    }


    public applyFiltersAndSort(): void {
        let result = [...this.transactions];

        // Office filter
        if (this.selectedOfficeId !== null) {
            result = result.filter(t => Number(t.financialOfficeId) === this.selectedOfficeId);
        }

        // Type filter
        if (this.filterType === 'income') {
            result = result.filter(t => t.isRevenue);
        } else if (this.filterType === 'expense') {
            result = result.filter(t => !t.isRevenue);
        }

        // Category filter
        if (this.filterCategoryId) {
            result = result.filter(t => Number(t.financialCategoryId) === this.filterCategoryId);
        }

        // Text filter
        if (this.filterText) {
            const search = this.filterText.toLowerCase();
            result = result.filter(t =>
                (t.description && t.description.toLowerCase().includes(search)) ||
                (t.referenceNumber && t.referenceNumber.toLowerCase().includes(search)) ||
                (t.paymentType && t.paymentType.name && t.paymentType.name.toLowerCase().includes(search)) ||
                (t.notes && t.notes.toLowerCase().includes(search)) ||
                (t.financialCategory && t.financialCategory.name && t.financialCategory.name.toLowerCase().includes(search))
            );
        }

        // Sort
        result.sort((a, b) => {
            let valA: any, valB: any;

            switch (this.sortColumn) {
                case 'transactionDate':
                    valA = a.transactionDate ? new Date(a.transactionDate).getTime() : 0;
                    valB = b.transactionDate ? new Date(b.transactionDate).getTime() : 0;
                    break;
                case 'totalAmount':
                    valA = Number(a.totalAmount ?? 0);
                    valB = Number(b.totalAmount ?? 0);
                    break;
                case 'description':
                    valA = (a.description ?? '').toLowerCase();
                    valB = (b.description ?? '').toLowerCase();
                    break;
                case 'category':
                    valA = (a.financialCategory?.name ?? '').toLowerCase();
                    valB = (b.financialCategory?.name ?? '').toLowerCase();
                    break;
                default:
                    valA = 0;
                    valB = 0;
            }

            if (valA < valB) return this.sortDirection === 'asc' ? -1 : 1;
            if (valA > valB) return this.sortDirection === 'asc' ? 1 : -1;
            return 0;
        });

        this.filteredTransactions = result;
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
            this.sortDirection = column === 'transactionDate' ? 'desc' : 'asc';
        }
        this.applyFiltersAndSort();
    }


    public getSortIcon(column: string): string {
        if (this.sortColumn !== column) return 'fa-sort';
        return this.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    }


    public setTypeFilter(type: 'all' | 'income' | 'expense'): void {
        this.filterType = type;
        this.applyFiltersAndSort();
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

    public trackById(index: number, item: FinancialTransactionData): number {
        return Number(item.id);
    }


    public addTransaction(): void {
        if (this.txAddEdit) {
            this.txAddEdit.preSeededData = {
                financialOfficeId: this.selectedOfficeId
            };
            this.txAddEdit.openModal();
        }
    }


    public editTransaction(tx: FinancialTransactionData): void {
        if (this.txAddEdit) {
            this.txAddEdit.openModal(tx);
        }
    }


    public onTransactionChanged(): void {
        this.loadData();
    }


    public requestVoid(tx: FinancialTransactionData, event: Event): void {
        event.stopPropagation();
        this.voidTarget = tx;
        this.voidReason = '';
    }

    public cancelVoid(): void {
        this.voidTarget = null;
        this.voidReason = '';
    }

    public executeVoid(): void {
        if (!this.voidTarget || !this.voidReason.trim() || this.isVoiding) return;
        this.isVoiding = true;

        const headers = this.authService.GetAuthenticationHeaders()
            .set('Content-Type', 'application/json');
        this.http.post<any>(
            `${this.baseUrl}api/FinancialTransactions/${this.voidTarget.id}/Void`,
            { reason: this.voidReason.trim() },
            { headers }
        ).subscribe({
            next: () => {
                this.alertService.showMessage(
                    `Transaction voided`, this.voidTarget?.description ?? '', MessageSeverity.success);
                this.cancelVoid();
                this.isVoiding = false;
                this.transactionService.ClearAllCaches();
                this.loadData();
            },
            error: (err: any) => {
                const msg = err?.error?.error || 'Failed to void transaction';
                this.alertService.showMessage('Void failed', msg, MessageSeverity.error);
                this.isVoiding = false;
            }
        });
    }
}
