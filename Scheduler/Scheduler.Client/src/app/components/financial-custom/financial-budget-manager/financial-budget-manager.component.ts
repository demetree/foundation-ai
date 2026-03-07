import { Component, OnInit } from '@angular/core';
import { Observable, forkJoin, map } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { BudgetService, BudgetData } from '../../../scheduler-data-services/budget.service';
import { FinancialTransactionService, FinancialTransactionData } from '../../../scheduler-data-services/financial-transaction.service';
import { FinancialCategoryService, FinancialCategoryData } from '../../../scheduler-data-services/financial-category.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { FiscalPeriodService, FiscalPeriodData } from '../../../scheduler-data-services/fiscal-period.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';


//
// Display row combining budget data with actual transaction totals
//
export interface BudgetDisplayRow {
    budget: BudgetData;
    categoryName: string;
    categoryCode: string;
    categoryColor: string | null;
    accountTypeName: string;
    isRevenue: boolean;
    budgetedAmount: number;
    revisedAmount: number | null;
    actualAmount: number;
    variance: number;           // positive = under budget, negative = over
    percentUsed: number;        // 0-100+ (can exceed 100)
}


@Component({
    selector: 'app-financial-budget-manager',
    templateUrl: './financial-budget-manager.component.html',
    styleUrls: ['./financial-budget-manager.component.scss']
})
export class FinancialBudgetManagerComponent implements OnInit {

    public isLoading = true;
    public isSmallScreen$: Observable<boolean>;

    //
    // Filters
    //
    public offices: FinancialOfficeData[] = [];
    public fiscalPeriods: FiscalPeriodData[] = [];
    public selectedOfficeId: number | null = null;
    public selectedFiscalPeriodId: number | null = null;

    //
    // Display data
    //
    public revenueRows: BudgetDisplayRow[] = [];
    public expenseRows: BudgetDisplayRow[] = [];

    //
    // Totals
    //
    public totalRevenueBudget = 0;
    public totalRevenueBudgetRevised = 0;
    public totalRevenueActual = 0;
    public totalExpenseBudget = 0;
    public totalExpenseBudgetRevised = 0;
    public totalExpenseActual = 0;

    //
    // Editing
    //
    public editingCell: { budgetId: number | bigint; field: 'budgetedAmount' | 'revisedAmount' } | null = null;
    public editValue: number = 0;

    constructor(
        private budgetService: BudgetService,
        private transactionService: FinancialTransactionService,
        private categoryService: FinancialCategoryService,
        private officeService: FinancialOfficeService,
        private fiscalPeriodService: FiscalPeriodService,
        private alertService: AlertService,
        private authService: AuthService,
        private navigationService: NavigationService,
        private breakpointObserver: BreakpointObserver,
        private router: Router
    ) {
        this.isSmallScreen$ = this.breakpointObserver
            .observe(['(max-width: 768px)'])
            .pipe(map(state => state.matches));
    }


    ngOnInit(): void {
        this.loadFilters();
    }


    //
    // Load filter dropdowns, then load budget data
    //
    private loadFilters(): void {
        forkJoin({
            offices: this.officeService.GetFinancialOfficeList({ active: true, deleted: false }),
            periods: this.fiscalPeriodService.GetFiscalPeriodList({ active: true, deleted: false })
        }).subscribe({
            next: ({ offices, periods }) => {
                this.offices = offices ?? [];
                this.fiscalPeriods = (periods ?? []).sort((a, b) =>
                    (a.startDate || '').localeCompare(b.startDate || '')
                );

                // Auto-select first office if only one
                if (this.offices.length === 1) {
                    this.selectedOfficeId = Number(this.offices[0].id);
                }

                // Auto-select most recent fiscal period
                if (this.fiscalPeriods.length > 0) {
                    this.selectedFiscalPeriodId = Number(this.fiscalPeriods[this.fiscalPeriods.length - 1].id);
                }

                this.loadBudgetData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to load filters', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    //
    // Load budgets + transactions and compute display rows
    //
    public loadBudgetData(): void {
        this.isLoading = true;

        const budgetParams: any = {
            active: true,
            deleted: false,
            includeRelations: true
        };
        if (this.selectedOfficeId) {
            budgetParams.financialOfficeId = this.selectedOfficeId;
        }
        if (this.selectedFiscalPeriodId) {
            budgetParams.fiscalPeriodId = this.selectedFiscalPeriodId;
        }

        const txParams: any = {
            active: true,
            deleted: false,
            includeRelations: true
        };
        if (this.selectedOfficeId) {
            txParams.financialOfficeId = this.selectedOfficeId;
        }
        if (this.selectedFiscalPeriodId) {
            txParams.fiscalPeriodId = this.selectedFiscalPeriodId;
        }

        // Clear caches to get fresh data
        this.budgetService.ClearAllCaches();
        this.transactionService.ClearAllCaches();

        forkJoin({
            budgets: this.budgetService.GetBudgetList(budgetParams),
            transactions: this.transactionService.GetFinancialTransactionList(txParams)
        }).subscribe({
            next: ({ budgets, transactions }) => {
                this.buildDisplayRows(budgets ?? [], transactions ?? []);
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to load budget data', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    //
    // Build display rows by matching budgets with actual transaction totals
    //
    private buildDisplayRows(budgets: BudgetData[], transactions: FinancialTransactionData[]): void {

        //
        // Sum transactions by category
        //
        const actualByCat = new Map<number, number>();
        for (const tx of transactions) {
            const catId = Number(tx.financialCategoryId);
            const current = actualByCat.get(catId) || 0;
            // Use totalAmount for the actual, since that includes tax
            actualByCat.set(catId, current + (tx.totalAmount || 0));
        }

        //
        // Filter budgets by fiscal period (if transactions span multiple periods)
        //
        let filteredBudgets = budgets;
        if (this.selectedFiscalPeriodId) {
            filteredBudgets = budgets.filter(b =>
                Number(b.fiscalPeriodId) === this.selectedFiscalPeriodId
            );
        }

        //
        // Build rows
        //
        const rows: BudgetDisplayRow[] = filteredBudgets.map(budget => {
            const catId = Number(budget.financialCategoryId);
            const actual = actualByCat.get(catId) || 0;
            const effectiveBudget = budget.revisedAmount ?? budget.budgetedAmount;
            const variance = effectiveBudget - actual;
            const pct = effectiveBudget > 0 ? (actual / effectiveBudget) * 100 : (actual > 0 ? 100 : 0);

            return {
                budget,
                categoryName: budget.financialCategory?.name ?? 'Unknown',
                categoryCode: budget.financialCategory?.code ?? '',
                categoryColor: budget.financialCategory?.color ?? null,
                accountTypeName: budget.financialCategory?.accountType?.name ?? '',
                isRevenue: budget.financialCategory?.accountType?.isRevenue ?? false,
                budgetedAmount: budget.budgetedAmount,
                revisedAmount: budget.revisedAmount,
                actualAmount: actual,
                variance,
                percentUsed: Math.round(pct)
            };
        });

        // Split into revenue and expense
        this.revenueRows = rows.filter(r => r.isRevenue).sort((a, b) => a.categoryCode.localeCompare(b.categoryCode));
        this.expenseRows = rows.filter(r => !r.isRevenue).sort((a, b) => a.categoryCode.localeCompare(b.categoryCode));

        // Compute totals
        this.totalRevenueBudget = this.revenueRows.reduce((sum, r) => sum + r.budgetedAmount, 0);
        this.totalRevenueBudgetRevised = this.revenueRows.reduce((sum, r) => sum + (r.revisedAmount ?? r.budgetedAmount), 0);
        this.totalRevenueActual = this.revenueRows.reduce((sum, r) => sum + r.actualAmount, 0);
        this.totalExpenseBudget = this.expenseRows.reduce((sum, r) => sum + r.budgetedAmount, 0);
        this.totalExpenseBudgetRevised = this.expenseRows.reduce((sum, r) => sum + (r.revisedAmount ?? r.budgetedAmount), 0);
        this.totalExpenseActual = this.expenseRows.reduce((sum, r) => sum + r.actualAmount, 0);
    }


    //
    // Filter change
    //
    public onFilterChange(): void {
        this.loadBudgetData();
    }


    //
    // Inline cell editing
    //
    public startEdit(row: BudgetDisplayRow, field: 'budgetedAmount' | 'revisedAmount'): void {
        this.editingCell = { budgetId: row.budget.id, field };
        this.editValue = field === 'budgetedAmount' ? row.budgetedAmount : (row.revisedAmount ?? row.budgetedAmount);
    }


    public cancelEdit(): void {
        this.editingCell = null;
    }


    public saveEdit(row: BudgetDisplayRow): void {
        if (!this.editingCell) return;

        const submitData = row.budget.ConvertToSubmitData();
        if (this.editingCell.field === 'budgetedAmount') {
            submitData.budgetedAmount = this.editValue;
        } else {
            submitData.revisedAmount = this.editValue;
        }

        this.budgetService.PutBudget(row.budget.id, submitData).subscribe({
            next: () => {
                this.alertService.showMessage('Budget updated', '', MessageSeverity.success);
                this.editingCell = null;
                this.loadBudgetData();
            },
            error: () => {
                this.alertService.showMessage('Failed to update budget', '', MessageSeverity.error);
            }
        });
    }


    public isEditing(budgetId: number | bigint, field: string): boolean {
        return this.editingCell?.budgetId === budgetId && this.editingCell?.field === field;
    }


    //
    // Variance styling
    //
    public getVarianceClass(row: BudgetDisplayRow): string {
        if (row.percentUsed > 100) return 'variance-over';
        if (row.percentUsed >= 80) return 'variance-warning';
        return 'variance-under';
    }


    public getProgressBarClass(row: BudgetDisplayRow): string {
        if (row.percentUsed > 100) return 'bg-danger';
        if (row.percentUsed >= 80) return 'bg-warning';
        return 'bg-success';
    }


    //
    // Navigation
    //
    public goBack(): void {
        this.navigationService.goBack();
    }

    public canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }

    public goToDashboard(): void {
        this.router.navigate(['/finances']);
    }
}
