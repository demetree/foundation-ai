import { Component, OnInit, ViewChild } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { FinancialCategoryService, FinancialCategoryData } from '../../../scheduler-data-services/financial-category.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { FinancialCategoryAddEditComponent } from '../../../scheduler-data-components/financial-category/financial-category-add-edit/financial-category-add-edit.component';
import { Router } from '@angular/router';


@Component({
    selector: 'app-financial-category-custom-listing',
    templateUrl: './financial-category-custom-listing.component.html',
    styleUrls: ['./financial-category-custom-listing.component.scss']
})
export class FinancialCategoryCustomListingComponent implements OnInit {

    public categories: FinancialCategoryData[] = [];
    public filteredCategories: FinancialCategoryData[] = [];
    public isLoading = true;
    public isSmallScreen = false;

    // Filter state
    public filterText = '';
    public filterType: 'all' | 'income' | 'expense' = 'all';

    // Sort state
    public sortColumn = 'code';
    public sortDirection: 'asc' | 'desc' = 'asc';

    // Counts
    public totalCount = 0;
    public incomeCount = 0;
    public expenseCount = 0;

    private debounceTimeout: any;

    // Account type colors and labels
    public accountTypeConfig: { [key: string]: { bg: string; color: string; label: string } } = {
        'Income': { bg: 'rgba(16, 185, 129, 0.1)', color: '#059669', label: 'Income' },
        'Expense': { bg: 'rgba(239, 68, 68, 0.1)', color: '#dc2626', label: 'Expense' },
        'COGS': { bg: 'rgba(245, 158, 11, 0.1)', color: '#d97706', label: 'COGS' },
        'Asset': { bg: 'rgba(59, 130, 246, 0.1)', color: '#2563eb', label: 'Asset' },
        'Liability': { bg: 'rgba(139, 92, 246, 0.1)', color: '#7c3aed', label: 'Liability' },
        'Equity': { bg: 'rgba(236, 72, 153, 0.1)', color: '#db2777', label: 'Equity' }
    };

    // Office filter
    public offices: FinancialOfficeData[] = [];
    public selectedOfficeId: number | null = null;


    constructor(
        private categoryService: FinancialCategoryService,
        private financialOfficeService: FinancialOfficeService,
        private alertService: AlertService,
        private authService: AuthService,
        private navigationService: NavigationService,
        private breakpointObserver: BreakpointObserver,
        private router: Router
    ) { }

    @ViewChild('catAddEdit') catAddEdit!: FinancialCategoryAddEditComponent;


    ngOnInit(): void {
        this.breakpointObserver
            .observe(['(max-width: 768px)'])
            .subscribe((result) => {
                this.isSmallScreen = result.matches;
            });

        this.loadData();
        this.loadOffices();
    }


    public loadData(): void {
        this.isLoading = true;

        this.categoryService.GetFinancialCategoryList({
            active: true,
            deleted: false,
            includeRelations: true,
            pageSize: 500
        }).subscribe({
            next: (data: FinancialCategoryData[] | null) => {
                this.categories = data ?? [];
                this.totalCount = this.categories.length;
                this.incomeCount = this.categories.filter(c => c.accountType?.isRevenue).length;
                this.expenseCount = this.categories.filter(c => !c.accountType?.isRevenue).length;
                this.applyFiltersAndSort();
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to load categories', MessageSeverity.error);
                this.isLoading = false;
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
        let result = [...this.categories];

        // Office filter
        if (this.selectedOfficeId !== null) {
            result = result.filter(c => Number(c.financialOfficeId) === this.selectedOfficeId);
        }

        // Type filter
        if (this.filterType === 'income') {
            result = result.filter(c => c.accountType?.isRevenue);
        } else if (this.filterType === 'expense') {
            result = result.filter(c => !c.accountType?.isRevenue);
        }

        // Text filter
        if (this.filterText) {
            const search = this.filterText.toLowerCase();
            result = result.filter(c =>
                (c.name && c.name.toLowerCase().includes(search)) ||
                (c.code && c.code.toLowerCase().includes(search)) ||
                (c.description && c.description.toLowerCase().includes(search))
            );
        }

        // Sort
        result.sort((a, b) => {
            let valA: any, valB: any;

            switch (this.sortColumn) {
                case 'code':
                    valA = (a.code ?? '').toLowerCase();
                    valB = (b.code ?? '').toLowerCase();
                    break;
                case 'name':
                    valA = (a.name ?? '').toLowerCase();
                    valB = (b.name ?? '').toLowerCase();
                    break;
                case 'accountType':
                    valA = (a.accountType?.name ?? '').toLowerCase();
                    valB = (b.accountType?.name ?? '').toLowerCase();
                    break;
                default:
                    valA = 0;
                    valB = 0;
            }

            if (valA < valB) return this.sortDirection === 'asc' ? -1 : 1;
            if (valA > valB) return this.sortDirection === 'asc' ? 1 : -1;
            return 0;
        });

        this.filteredCategories = result;
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
            this.sortDirection = 'asc';
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


    public getAccountTypeStyle(type: string): { [key: string]: string } {
        const config = this.accountTypeConfig[type] || { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' };
        return {
            'background': config.bg,
            'color': config.color
        };
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

    public trackById(index: number, item: FinancialCategoryData): number {
        return Number(item.id);
    }


    public addCategory(): void {
        if (this.catAddEdit) {
            this.catAddEdit.preSeededData = {
                financialOfficeId: this.selectedOfficeId
            };
            this.catAddEdit.openModal();
        }
    }


    public editCategory(cat: FinancialCategoryData): void {
        if (this.catAddEdit) {
            this.catAddEdit.openModal(cat);
        }
    }


    public onCategoryChanged(): void {
        this.loadData();
    }
}
