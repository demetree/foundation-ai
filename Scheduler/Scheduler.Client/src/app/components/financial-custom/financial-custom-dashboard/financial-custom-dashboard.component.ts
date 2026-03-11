// AI-Developed — This file was significantly developed with AI assistance.
//
// Financial Dashboard Component
//
// Provides a financial overview dashboard with summary cards, a fiscal-year-aware
// monthly breakdown chart, and recent transaction list.  The year picker is driven
// by FiscalPeriod records so that only defined fiscal years appear in the dropdown.
//

import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NavigationService } from '../../../utility-services/navigation.service';
import { FinancialTransactionService, FinancialTransactionData } from '../../../scheduler-data-services/financial-transaction.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { FiscalPeriodService, FiscalPeriodData } from '../../../scheduler-data-services/fiscal-period.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { Router } from '@angular/router';


@Component({
    selector: 'app-financial-custom-dashboard',
    templateUrl: './financial-custom-dashboard.component.html',
    styleUrls: ['./financial-custom-dashboard.component.scss']
})
export class FinancialCustomDashboardComponent implements OnInit {

    public isLoading = true;

    //
    // Summary card values (all-time totals, unaffected by year picker)
    //
    public totalIncome = 0;
    public totalExpenses = 0;
    public netBalance = 0;
    public transactionCount = 0;
    public categoryCount = 0;
    public isExporting = false;

    //
    // Category breakdown for income-vs-expense summary table
    //
    public revenueCategories: CategoryBreakdownItem[] = [];
    public expenseCategories: CategoryBreakdownItem[] = [];
    public yearRevenue = 0;
    public yearExpenses = 0;
    public yearNet = 0;
    public loadingBreakdown = false;

    //
    // Outstanding deposits widget
    //
    public outstandingDeposits: OutstandingDepositItem[] = [];
    public outstandingDepositsTotal = 0;
    public loadingDeposits = false;

    //
    // Recent transactions
    //
    public recentTransactions: FinancialTransactionData[] = [];
    public loadingRecent = true;

    //
    // Fiscal year picker — driven by the FiscalPeriod table
    //
    public selectedYear: number = new Date().getFullYear();
    public currentYear: number = new Date().getFullYear();
    public availableYears: number[] = [];
    public fiscalPeriodList: FiscalPeriodData[] = [];

    //
    // Monthly breakdown for selected fiscal year
    //
    public monthlyIncome: number[] = new Array(12).fill(0);
    public monthlyExpenses: number[] = new Array(12).fill(0);
    public months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

    //
    // Cached transaction list for re-processing when the year or office changes
    //
    private allTransactions: FinancialTransactionData[] = [];

    //
    // Financial Office picker
    //
    public offices: FinancialOfficeData[] = [];
    public selectedOfficeId: number | null = null;  // null = "All Offices"

    constructor(
        private http: HttpClient,
        private transactionService: FinancialTransactionService,
        private categoryService: FinancialCategoryService,
        private fiscalPeriodService: FiscalPeriodService,
        private financialOfficeService: FinancialOfficeService,
        private authService: AuthService,
        private alertService: AlertService,
        private navigationService: NavigationService,
        private router: Router
    ) { }


    ngOnInit(): void {
        this.loadDashboardData();
        this.loadCategoryBreakdown();
        this.loadOutstandingDeposits();
    }


    private loadDashboardData(): void {
        this.isLoading = true;

        //
        // Load financial offices for the office picker
        //
        this.financialOfficeService.GetFinancialOfficeList({
            active: true,
            deleted: false,
            pageSize: 100
        }).subscribe({
            next: (offices) => {
                this.offices = offices ?? [];
            }
        });

        //
        // Load fiscal periods to populate the year picker
        //
        this.fiscalPeriodService.GetFiscalPeriodList({
            active: true,
            deleted: false,
            pageSize: 500
        }).subscribe({
            next: (periods) => {
                if (periods) {
                    this.fiscalPeriodList = periods;
                    this.buildAvailableYears(periods);
                }
            }
        });

        //
        // Load all transactions for the dashboard.
        // For a small community deployment, loading all transactions is fine.
        //
        this.transactionService.GetFinancialTransactionList({
            active: true,
            deleted: false,
            includeRelations: true,
            // TODO: Replace with server-side aggregation/pagination when transaction volume grows
            pageSize: 10000
        }).subscribe({
            next: (transactions) => {
                if (transactions) {
                    this.allTransactions = transactions;
                    this.processTransactions(this.getFilteredTransactions());
                }
                this.isLoading = false;
            },
            error: () => {
                this.isLoading = false;
            }
        });

        //
        // Load category count
        //
        this.categoryService.GetFinancialCategoriesRowCount({
            active: true,
            deleted: false
        }).subscribe({
            next: (count) => {
                this.categoryCount = Number(count ?? 0);
            }
        });
    }


    /**
     * Builds the list of available fiscal years from the FiscalPeriod records.
     * Defaults the selected year to the current calendar year if present,
     * otherwise falls back to the most recent fiscal year.
     */
    private buildAvailableYears(periods: FiscalPeriodData[]): void {

        //
        // Extract unique fiscal years and sort descending
        //
        const yearSet = new Set<number>();

        for (const period of periods) {
            if (period.fiscalYear != null) {
                yearSet.add(Number(period.fiscalYear));
            }
        }

        this.availableYears = Array.from(yearSet).sort((a, b) => b - a);

        //
        // Default to current calendar year if it exists in the list, otherwise use the most recent
        //
        const currentCalendarYear = new Date().getFullYear();

        if (this.availableYears.includes(currentCalendarYear)) {
            this.selectedYear = currentCalendarYear;
        } else if (this.availableYears.length > 0) {
            this.selectedYear = this.availableYears[0];
        }
    }


    private processTransactions(transactions: FinancialTransactionData[]): void {
        this.transactionCount = transactions.length;

        //
        // Calculate all-time totals (unaffected by year picker)
        //
        this.totalIncome = 0;
        this.totalExpenses = 0;

        for (const t of transactions) {
            const amount = Number(t.totalAmount ?? t.amount ?? 0);

            if (t.isRevenue) {
                this.totalIncome += amount;
            } else {
                this.totalExpenses += amount;
            }
        }

        this.netBalance = this.totalIncome - this.totalExpenses;

        //
        // Build the monthly breakdown for the selected year
        //
        this.rebuildMonthlyBreakdown();

        //
        // Recent transactions: sort by date descending, take last 10
        //
        this.recentTransactions = transactions
            .sort((a, b) => {
                const da = a.transactionDate ? new Date(a.transactionDate).getTime() : 0;
                const db = b.transactionDate ? new Date(b.transactionDate).getTime() : 0;
                return db - da;
            })
            .slice(0, 10);

        this.loadingRecent = false;
    }


    /**
     * Recalculates the monthly income/expense arrays for the currently selected year.
     * Called on init and whenever the year picker changes.
     */
    private rebuildMonthlyBreakdown(): void {

        this.monthlyIncome = new Array(12).fill(0);
        this.monthlyExpenses = new Array(12).fill(0);

        for (const t of this.getFilteredTransactions()) {
            const txDate = t.transactionDate ? new Date(t.transactionDate) : null;

            if (txDate && txDate.getFullYear() === this.selectedYear) {
                const month = txDate.getMonth();
                const amount = Number(t.totalAmount ?? t.amount ?? 0);

                if (t.isRevenue) {
                    this.monthlyIncome[month] += amount;
                } else {
                    this.monthlyExpenses[month] += amount;
                }
            }
        }
    }


    //
    // Year picker handlers
    //

    public onYearChange(year: number): void {
        this.selectedYear = year;
        this.rebuildMonthlyBreakdown();
        this.loadCategoryBreakdown();
    }


    public onOfficeChange(officeId: number | null): void {
        this.selectedOfficeId = officeId;
        this.processTransactions(this.getFilteredTransactions());
        this.loadCategoryBreakdown();
    }


    /**
     * Returns transactions filtered by the selected office.
     * When selectedOfficeId is null, returns all transactions.
     */
    private getFilteredTransactions(): FinancialTransactionData[] {
        if (this.selectedOfficeId === null) {
            return this.allTransactions;
        }
        return this.allTransactions.filter(t => Number(t.financialOfficeId) === this.selectedOfficeId);
    }


    public stepYear(direction: number): void {
        const currentIndex = this.availableYears.indexOf(this.selectedYear);
        const newIndex = currentIndex - direction;

        if (newIndex >= 0 && newIndex < this.availableYears.length) {
            this.selectedYear = this.availableYears[newIndex];
            this.rebuildMonthlyBreakdown();
        }
    }


    public canStepYear(direction: number): boolean {
        const currentIndex = this.availableYears.indexOf(this.selectedYear);
        const newIndex = currentIndex - direction;
        return newIndex >= 0 && newIndex < this.availableYears.length;
    }


    //
    // Navigation helpers
    //

    public goToTransactions(): void {
        this.router.navigate(['/finances/transactions']);
    }


    public goToCategories(): void {
        this.router.navigate(['/finances/categories']);
    }


    public goToBudgets(): void {
        this.router.navigate(['/finances/budgets']);
    }


    public goBack(): void {
        this.navigationService.goBack();
    }


    public canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }


    //
    // Visual helpers
    //

    public getMaxMonthlyAmount(): number {
        const maxIncome = Math.max(...this.monthlyIncome);
        const maxExpense = Math.max(...this.monthlyExpenses);
        return Math.max(maxIncome, maxExpense, 1); // Avoid divide by zero
    }


    public getBarHeight(amount: number): number {
        return (amount / this.getMaxMonthlyAmount()) * 100;
    }


    public getCurrentMonthIndex(): number {
        return new Date().getMonth();
    }


    /**
     * Downloads a formatted Excel financial report for the selected year and office.
     */
    public exportReport(): void {
        if (this.isExporting) return;

        this.isExporting = true;

        let url = `/api/FinancialTransactions/ExportFinancialReport?year=${this.selectedYear}`;
        if (this.selectedOfficeId !== null) {
            url += `&financialOfficeId=${this.selectedOfficeId}`;
        }

        this.http.get(url, { responseType: 'blob', observe: 'response' }).subscribe({
            next: (response) => {
                const blob = response.body;
                if (!blob) {
                    this.alertService.showMessage('Export failed', 'No data returned', MessageSeverity.error);
                    this.isExporting = false;
                    return;
                }

                //
                // Extract filename from Content-Disposition header or use a default
                //
                const contentDisposition = response.headers.get('Content-Disposition');
                let filename = `Financial_Report_${this.selectedYear}.xlsx`;
                if (contentDisposition) {
                    const match = contentDisposition.match(/filename="?([^"]+)"?/);
                    if (match && match[1]) {
                        filename = match[1];
                    }
                }

                //
                // Trigger browser download
                //
                const blobUrl = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = blobUrl;
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                window.URL.revokeObjectURL(blobUrl);

                this.alertService.showMessage('Report exported successfully', '', MessageSeverity.success);
                this.isExporting = false;
            },
            error: (err) => {
                this.alertService.showMessage('Export failed', err?.message || 'Unknown error', MessageSeverity.error);
                this.isExporting = false;
            }
        });
    }


    /**
     * Loads category-level breakdown from the server for the selected year and office.
     */
    private loadCategoryBreakdown(): void {
        this.loadingBreakdown = true;

        let url = `/api/FinancialTransactions/CategoryBreakdown?year=${this.selectedYear}`;
        if (this.selectedOfficeId !== null) {
            url += `&financialOfficeId=${this.selectedOfficeId}`;
        }

        this.http.get<CategoryBreakdownResponse>(url).subscribe({
            next: (response) => {
                const categories = response?.categories ?? [];
                this.revenueCategories = categories.filter((c: CategoryBreakdownItem) => c.isRevenue);
                this.expenseCategories = categories.filter((c: CategoryBreakdownItem) => !c.isRevenue);
                this.yearRevenue = this.revenueCategories.reduce((sum, c) => sum + c.total, 0);
                this.yearExpenses = this.expenseCategories.reduce((sum, c) => sum + c.total, 0);
                this.yearNet = this.yearRevenue - this.yearExpenses;
                this.loadingBreakdown = false;
            },
            error: () => {
                this.loadingBreakdown = false;
            }
        });
    }


    /**
     * Loads outstanding (unreturned) deposits across the tenant.
     */
    private loadOutstandingDeposits(): void {
        this.loadingDeposits = true;

        this.http.get<OutstandingDepositsResponse>('/api/FinancialTransactions/OutstandingDeposits').subscribe({
            next: (response) => {
                this.outstandingDeposits = response?.deposits ?? [];
                this.outstandingDepositsTotal = response?.totalAmount ?? 0;
                this.loadingDeposits = false;
            },
            error: () => {
                this.loadingDeposits = false;
            }
        });
    }
}


//
// Interfaces for Category Breakdown API response
//
interface CategoryBreakdownItem {
    categoryId: number;
    categoryName: string;
    code: string;
    isRevenue: boolean;
    total: number;
    count: number;
}

interface CategoryBreakdownResponse {
    year: number;
    categories: CategoryBreakdownItem[];
}

interface OutstandingDepositItem {
    chargeId: number;
    eventId: number;
    eventName: string;
    chargeType: string;
    amount: number;
    eventDate: string | null;
}

interface OutstandingDepositsResponse {
    count: number;
    totalAmount: number;
    deposits: OutstandingDepositItem[];
}
