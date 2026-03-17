// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Observable, forkJoin, map } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { BudgetService, BudgetData, BudgetSubmitData } from '../../../scheduler-data-services/budget.service';
import { FinancialTransactionService, FinancialTransactionData } from '../../../scheduler-data-services/financial-transaction.service';
import { FinancialCategoryService, FinancialCategoryData } from '../../../scheduler-data-services/financial-category.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { FiscalPeriodService, FiscalPeriodData } from '../../../scheduler-data-services/fiscal-period.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
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
    notes: string | null;
}


//
// Unbudgeted category for add-budget flow
//
export interface UnbudgetedCategory {
    category: FinancialCategoryData;
    isRevenue: boolean;
}


@Component({
    selector: 'app-financial-budget-manager',
    templateUrl: './financial-budget-manager.component.html',
    styleUrls: ['./financial-budget-manager.component.scss']
})
export class FinancialBudgetManagerComponent implements OnInit {

    public isLoading = true;
    public isSmallScreen = false;

    //
    // Filters
    //
    public offices: FinancialOfficeData[] = [];
    public fiscalPeriods: FiscalPeriodData[] = [];
    public selectedOfficeId: number | null = null;
    public selectedFiscalPeriodId: number | null = null;
    public searchText = '';

    //
    // Display data
    //
    public revenueRows: BudgetDisplayRow[] = [];
    public expenseRows: BudgetDisplayRow[] = [];
    public filteredRevenueRows: BudgetDisplayRow[] = [];
    public filteredExpenseRows: BudgetDisplayRow[] = [];

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
    // Budget health stats
    //
    public onTrackCount = 0;
    public warningCount = 0;
    public overBudgetCount = 0;

    //
    // Editing
    //
    public editingCell: { budgetId: number | bigint; field: 'budgetedAmount' | 'revisedAmount' | 'notes' } | null = null;
    public editValue: number = 0;
    public editNotes: string = '';

    //
    // Unbudgeted categories
    //
    public unbudgetedCategories: UnbudgetedCategory[] = [];
    public showUnbudgeted = false;

    //
    // Add budget inline
    //
    public addingBudget = false;
    public addBudgetCategoryId: number | null = null;
    public addBudgetAmount: number = 0;
    public isSavingNew = false;

    //
    // Default currency
    //
    private defaultCurrencyId: number = 1;

    private debounceTimeout: any;

    constructor(
        private budgetService: BudgetService,
        private transactionService: FinancialTransactionService,
        private categoryService: FinancialCategoryService,
        private officeService: FinancialOfficeService,
        private fiscalPeriodService: FiscalPeriodService,
        private currencyService: CurrencyService,
        private alertService: AlertService,
        private authService: AuthService,
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

        this.loadFilters();
        this.loadDefaultCurrency();
    }


    //
    // Load default currency
    //
    private loadDefaultCurrency(): void {
        this.currencyService.GetCurrencyList({ active: true, deleted: false, pageSize: 1 }).subscribe({
            next: (currencies) => {
                if (currencies && currencies.length > 0) {
                    this.defaultCurrencyId = Number(currencies[0].id);
                }
            }
        });
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
            transactions: this.transactionService.GetFinancialTransactionList(txParams),
            categories: this.categoryService.GetFinancialCategoryList({ active: true, deleted: false, includeRelations: true })
        }).subscribe({
            next: ({ budgets, transactions, categories }) => {
                this.buildDisplayRows(budgets ?? [], transactions ?? []);
                this.findUnbudgetedCategories(budgets ?? [], categories ?? []);
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
            actualByCat.set(catId, current + (tx.totalAmount || 0));
        }

        //
        // Filter budgets by fiscal period
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
                percentUsed: Math.round(pct),
                notes: budget.notes
            };
        });

        // Split into revenue and expense
        this.revenueRows = rows.filter(r => r.isRevenue).sort((a, b) => a.categoryCode.localeCompare(b.categoryCode));
        this.expenseRows = rows.filter(r => !r.isRevenue).sort((a, b) => a.categoryCode.localeCompare(b.categoryCode));

        // Compute totals
        this.computeTotals();

        // Compute health stats
        this.computeHealthStats(rows);

        // Apply search filter
        this.applySearchFilter();
    }


    private computeTotals(): void {
        this.totalRevenueBudget = this.revenueRows.reduce((sum, r) => sum + r.budgetedAmount, 0);
        this.totalRevenueBudgetRevised = this.revenueRows.reduce((sum, r) => sum + (r.revisedAmount ?? r.budgetedAmount), 0);
        this.totalRevenueActual = this.revenueRows.reduce((sum, r) => sum + r.actualAmount, 0);
        this.totalExpenseBudget = this.expenseRows.reduce((sum, r) => sum + r.budgetedAmount, 0);
        this.totalExpenseBudgetRevised = this.expenseRows.reduce((sum, r) => sum + (r.revisedAmount ?? r.budgetedAmount), 0);
        this.totalExpenseActual = this.expenseRows.reduce((sum, r) => sum + r.actualAmount, 0);
    }


    private computeHealthStats(rows: BudgetDisplayRow[]): void {
        this.onTrackCount = 0;
        this.warningCount = 0;
        this.overBudgetCount = 0;

        for (const row of rows) {
            if (row.percentUsed > 100) {
                this.overBudgetCount++;
            } else if (row.percentUsed >= 80) {
                this.warningCount++;
            } else {
                this.onTrackCount++;
            }
        }
    }


    //
    // Find categories without budgets for the selected period
    //
    private findUnbudgetedCategories(budgets: BudgetData[], categories: FinancialCategoryData[]): void {
        const budgetedCatIds = new Set(budgets.map(b => Number(b.financialCategoryId)));

        this.unbudgetedCategories = categories
            .filter(cat => {
                // Not already budgeted
                if (budgetedCatIds.has(Number(cat.id))) return false;
                // Filter by office if selected
                if (this.selectedOfficeId && cat.financialOfficeId) {
                    if (Number(cat.financialOfficeId) !== this.selectedOfficeId) return false;
                }
                return true;
            })
            .map(cat => ({
                category: cat,
                isRevenue: cat.accountType?.isRevenue ?? false
            }))
            .sort((a, b) => (a.category.code ?? '').localeCompare(b.category.code ?? ''));
    }


    //
    // ── Search Filter ──
    //

    public onSearchChange(): void {
        clearTimeout(this.debounceTimeout);
        this.debounceTimeout = setTimeout(() => {
            this.applySearchFilter();
        }, 100);
    }


    private applySearchFilter(): void {
        if (!this.searchText) {
            this.filteredRevenueRows = this.revenueRows;
            this.filteredExpenseRows = this.expenseRows;
        } else {
            const search = this.searchText.toLowerCase();
            this.filteredRevenueRows = this.revenueRows.filter(r =>
                r.categoryName.toLowerCase().includes(search) ||
                r.categoryCode.toLowerCase().includes(search)
            );
            this.filteredExpenseRows = this.expenseRows.filter(r =>
                r.categoryName.toLowerCase().includes(search) ||
                r.categoryCode.toLowerCase().includes(search)
            );
        }
    }


    //
    // Filter change
    //
    public onFilterChange(): void {
        this.loadBudgetData();
    }


    //
    // ── Inline Cell Editing ──
    //

    public startEdit(row: BudgetDisplayRow, field: 'budgetedAmount' | 'revisedAmount' | 'notes'): void {
        this.editingCell = { budgetId: row.budget.id, field };
        if (field === 'notes') {
            this.editNotes = row.notes ?? '';
        } else {
            this.editValue = field === 'budgetedAmount' ? row.budgetedAmount : (row.revisedAmount ?? row.budgetedAmount);
        }
    }


    public cancelEdit(): void {
        this.editingCell = null;
    }


    public saveEdit(row: BudgetDisplayRow): void {
        if (!this.editingCell) return;

        const submitData = row.budget.ConvertToSubmitData();
        if (this.editingCell.field === 'budgetedAmount') {
            submitData.budgetedAmount = this.editValue;
        } else if (this.editingCell.field === 'revisedAmount') {
            submitData.revisedAmount = this.editValue;
        } else if (this.editingCell.field === 'notes') {
            submitData.notes = this.editNotes?.trim() || null;
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
    // ── Add New Budget ──
    //

    public startAddBudget(): void {
        this.addingBudget = true;
        this.addBudgetCategoryId = null;
        this.addBudgetAmount = 0;
    }


    public cancelAddBudget(): void {
        this.addingBudget = false;
    }


    public quickAddBudget(cat: UnbudgetedCategory): void {
        this.addingBudget = true;
        this.addBudgetCategoryId = Number(cat.category.id);
        this.addBudgetAmount = 0;
    }


    public saveNewBudget(): void {
        if (!this.addBudgetCategoryId || !this.selectedFiscalPeriodId) {
            this.alertService.showMessage('Please select a category and fiscal period', '', MessageSeverity.warn);
            return;
        }

        this.isSavingNew = true;

        const submitData = new BudgetSubmitData();
        submitData.id = 0;
        submitData.financialCategoryId = this.addBudgetCategoryId;
        submitData.fiscalPeriodId = this.selectedFiscalPeriodId;
        submitData.financialOfficeId = this.selectedOfficeId;
        submitData.budgetedAmount = this.addBudgetAmount;
        submitData.revisedAmount = null;
        submitData.notes = null;
        submitData.currencyId = this.defaultCurrencyId;
        submitData.versionNumber = 0;
        submitData.active = true;
        submitData.deleted = false;

        this.budgetService.PostBudget(submitData).subscribe({
            next: () => {
                this.alertService.showMessage('Budget created', '', MessageSeverity.success);
                this.addingBudget = false;
                this.isSavingNew = false;
                this.loadBudgetData();
            },
            error: (err) => {
                const msg = err?.error?.message || err?.error?.detail || 'Failed to create budget';
                this.alertService.showMessage('Error', msg, MessageSeverity.error);
                this.isSavingNew = false;
            }
        });
    }


    //
    // ── Delete Budget ──
    //

    public deleteBudget(row: BudgetDisplayRow): void {
        if (!confirm(`Remove the budget for "${row.categoryName}"?`)) return;

        this.budgetService.DeleteBudget(row.budget.id).subscribe({
            next: () => {
                this.alertService.showMessage('Budget removed', '', MessageSeverity.success);
                this.loadBudgetData();
            },
            error: () => {
                this.alertService.showMessage('Failed to remove budget', '', MessageSeverity.error);
            }
        });
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
    // Health badge class
    //
    public getHealthLabel(): string {
        const total = this.revenueRows.length + this.expenseRows.length;
        if (total === 0) return '—';
        if (this.overBudgetCount === 0 && this.warningCount === 0) return 'Healthy';
        if (this.overBudgetCount === 0) return 'Caution';
        return 'At Risk';
    }

    public getHealthClass(): string {
        if (this.overBudgetCount > 0) return 'health-risk';
        if (this.warningCount > 0) return 'health-caution';
        return 'health-good';
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
