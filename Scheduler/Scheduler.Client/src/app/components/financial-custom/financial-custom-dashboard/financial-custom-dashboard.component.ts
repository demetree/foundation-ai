import { Component, OnInit } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay, forkJoin, of } from 'rxjs';
import { NavigationService } from '../../../utility-services/navigation.service';
import { FinancialTransactionService, FinancialTransactionData } from '../../../scheduler-data-services/financial-transaction.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';


@Component({
    selector: 'app-financial-custom-dashboard',
    templateUrl: './financial-custom-dashboard.component.html',
    styleUrls: ['./financial-custom-dashboard.component.scss']
})
export class FinancialCustomDashboardComponent implements OnInit {

    public isLoading = true;

    // Summary card values
    public totalIncome = 0;
    public totalExpenses = 0;
    public netBalance = 0;
    public transactionCount = 0;
    public categoryCount = 0;

    // Recent transactions
    public recentTransactions: FinancialTransactionData[] = [];
    public loadingRecent = true;

    // Monthly breakdown for current year
    public monthlyIncome: number[] = new Array(12).fill(0);
    public monthlyExpenses: number[] = new Array(12).fill(0);
    public currentYear = new Date().getFullYear();
    public months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

    constructor(
        private transactionService: FinancialTransactionService,
        private categoryService: FinancialCategoryService,
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
        // Load all transactions for the current year to build the dashboard.
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
                    this.processTransactions(transactions);
                }
                this.isLoading = false;
            },
            error: () => {
                this.isLoading = false;
            }
        });

        // Load category count
        this.categoryService.GetFinancialCategoriesRowCount({
            active: true,
            deleted: false
        }).subscribe({
            next: (count) => {
                this.categoryCount = Number(count ?? 0);
            }
        });
    }


    private processTransactions(transactions: FinancialTransactionData[]): void {
        this.transactionCount = transactions.length;

        // Calculate totals
        for (const t of transactions) {
            const amount = Number(t.totalAmount ?? t.amount ?? 0);

            if (t.isRevenue) {
                this.totalIncome += amount;
            } else {
                this.totalExpenses += amount;
            }

            // Monthly breakdown — check if transaction is in current year
            const txDate = t.transactionDate ? new Date(t.transactionDate) : null;

            if (txDate && txDate.getFullYear() === this.currentYear) {
                const month = txDate.getMonth();

                if (t.isRevenue) {
                    this.monthlyIncome[month] += amount;
                } else {
                    this.monthlyExpenses[month] += amount;
                }
            }
        }

        this.netBalance = this.totalIncome - this.totalExpenses;

        // Recent transactions: sort by date descending, take last 10
        this.recentTransactions = transactions
            .sort((a, b) => {
                const da = a.transactionDate ? new Date(a.transactionDate).getTime() : 0;
                const db = b.transactionDate ? new Date(b.transactionDate).getTime() : 0;
                return db - da;
            })
            .slice(0, 10);

        this.loadingRecent = false;
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
