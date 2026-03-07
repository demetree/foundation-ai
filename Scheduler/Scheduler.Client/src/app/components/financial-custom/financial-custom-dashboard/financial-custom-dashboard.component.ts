// AI-Developed — This file was significantly developed with AI assistance.
//
// Financial Dashboard Component
//
// Provides a financial overview dashboard with summary cards, a fiscal-year-aware
// monthly breakdown chart, and recent transaction list.  The year picker is driven
// by FiscalPeriod records so that only defined fiscal years appear in the dropdown.
//

import { Component, OnInit } from '@angular/core';
import { NavigationService } from '../../../utility-services/navigation.service';
import { FinancialTransactionService, FinancialTransactionData } from '../../../scheduler-data-services/financial-transaction.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { FiscalPeriodService, FiscalPeriodData } from '../../../scheduler-data-services/fiscal-period.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { AuthService } from '../../../services/auth.service';
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
        private transactionService: FinancialTransactionService,
        private categoryService: FinancialCategoryService,
        private fiscalPeriodService: FiscalPeriodService,
        private financialOfficeService: FinancialOfficeService,
        private authService: AuthService,
        private navigationService: NavigationService,
        private router: Router
    ) { }


    ngOnInit(): void {
        this.loadDashboardData();
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
            pageSize: 5000
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

        for (const t of this.allTransactions) {
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
    }


    public onOfficeChange(officeId: number | null): void {
        this.selectedOfficeId = officeId;
        this.processTransactions(this.getFilteredTransactions());
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
}
