// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { BudgetService, BudgetData } from '../../../scheduler-data-services/budget.service';
import { FinancialTransactionService, FinancialTransactionData } from '../../../scheduler-data-services/financial-transaction.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { FiscalPeriodService, FiscalPeriodData } from '../../../scheduler-data-services/fiscal-period.service';
import { Router } from '@angular/router';


interface ReportRow {
    categoryName: string;
    categoryCode: string;
    budgetedAmount: number;
    revisedAmount: number | null;
    actualAmount: number;
    variance: number;
    percentUsed: number;
}


@Component({
    selector: 'app-budget-report',
    templateUrl: './budget-report.component.html',
    styleUrls: ['./budget-report.component.scss']
})
export class BudgetReportComponent implements OnInit {

    public isLoading = true;
    public offices: FinancialOfficeData[] = [];
    public fiscalPeriods: FiscalPeriodData[] = [];
    public selectedOfficeId: number | null = null;
    public selectedFiscalPeriodId: number | null = null;

    public revenueRows: ReportRow[] = [];
    public expenseRows: ReportRow[] = [];

    public totalRevenueBudget = 0;
    public totalRevenueActual = 0;
    public totalExpenseBudget = 0;
    public totalExpenseActual = 0;

    public reportDate = new Date().toLocaleDateString('en-CA', { year: 'numeric', month: 'long', day: 'numeric' });
    public selectedOfficeName = 'All Offices';
    public selectedPeriodName = 'All Periods';

    constructor(
        private budgetService: BudgetService,
        private transactionService: FinancialTransactionService,
        private officeService: FinancialOfficeService,
        private fiscalPeriodService: FiscalPeriodService,
        private alertService: AlertService,
        private authService: AuthService,
        private router: Router
    ) { }


    ngOnInit(): void {
        this.loadFilters();
    }


    private loadFilters(): void {
        forkJoin({
            offices: this.officeService.GetFinancialOfficeList(),
            periods: this.fiscalPeriodService.GetFiscalPeriodList({ active: true, deleted: false })
        }).subscribe({
            next: ({ offices, periods }) => {
                this.offices = offices ?? [];
                this.fiscalPeriods = (periods ?? []).sort((a, b) => (a.startDate ?? '').localeCompare(b.startDate ?? ''));
                this.loadReport();
            },
            error: () => {
                this.alertService.showMessage('Failed to load filters', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    onFilterChange(): void {
        this.selectedOfficeName = this.selectedOfficeId
            ? this.offices.find(o => Number(o.id) === this.selectedOfficeId)?.name ?? 'Unknown'
            : 'All Offices';
        this.selectedPeriodName = this.selectedFiscalPeriodId
            ? this.fiscalPeriods.find(p => Number(p.id) === this.selectedFiscalPeriodId)?.name ?? 'Unknown'
            : 'All Periods';
        this.loadReport();
    }


    private loadReport(): void {
        this.isLoading = true;

        const budgetParams: any = { active: true, deleted: false, includeRelations: true };
        if (this.selectedOfficeId) budgetParams.financialOfficeId = this.selectedOfficeId;
        if (this.selectedFiscalPeriodId) budgetParams.fiscalPeriodId = this.selectedFiscalPeriodId;

        const txParams: any = { active: true, deleted: false };
        if (this.selectedOfficeId) txParams.financialOfficeId = this.selectedOfficeId;

        forkJoin({
            budgets: this.budgetService.GetBudgetList(budgetParams),
            transactions: this.transactionService.GetFinancialTransactionList(txParams)
        }).subscribe({
            next: ({ budgets, transactions }) => {
                this.buildRows(budgets ?? [], transactions ?? []);
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load report data', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private buildRows(budgets: BudgetData[], transactions: FinancialTransactionData[]): void {
        // Sum actuals by category
        const actualByCat = new Map<number, number>();
        for (const tx of transactions) {
            const catId = Number(tx.financialCategoryId);
            actualByCat.set(catId, (actualByCat.get(catId) || 0) + (tx.totalAmount || 0));
        }

        // Filter by period if selected
        let filtered = budgets;
        if (this.selectedFiscalPeriodId) {
            filtered = budgets.filter(b => Number(b.fiscalPeriodId) === this.selectedFiscalPeriodId);
        }

        const rows: ReportRow[] = filtered.map(b => {
            const catId = Number(b.financialCategoryId);
            const actual = actualByCat.get(catId) || 0;
            const effective = b.revisedAmount ?? b.budgetedAmount;
            const variance = effective - actual;
            const pct = effective > 0 ? (actual / effective) * 100 : (actual > 0 ? 100 : 0);
            return {
                categoryName: b.financialCategory?.name ?? 'Unknown',
                categoryCode: b.financialCategory?.code ?? '',
                budgetedAmount: b.budgetedAmount,
                revisedAmount: b.revisedAmount,
                actualAmount: actual,
                variance,
                percentUsed: Math.round(pct),
            };
        });

        const isRevenue = (r: ReportRow) => {
            const b = filtered.find(bb => (bb.financialCategory?.code ?? '') === r.categoryCode);
            return b?.financialCategory?.accountType?.isRevenue ?? false;
        };

        this.revenueRows = rows.filter(isRevenue).sort((a, b) => a.categoryCode.localeCompare(b.categoryCode));
        this.expenseRows = rows.filter(r => !isRevenue(r)).sort((a, b) => a.categoryCode.localeCompare(b.categoryCode));

        this.totalRevenueBudget = this.revenueRows.reduce((s, r) => s + (r.revisedAmount ?? r.budgetedAmount), 0);
        this.totalRevenueActual = this.revenueRows.reduce((s, r) => s + r.actualAmount, 0);
        this.totalExpenseBudget = this.expenseRows.reduce((s, r) => s + (r.revisedAmount ?? r.budgetedAmount), 0);
        this.totalExpenseActual = this.expenseRows.reduce((s, r) => s + r.actualAmount, 0);
    }


    get netBudgeted(): number { return this.totalRevenueBudget - this.totalExpenseBudget; }
    get netActual(): number { return this.totalRevenueActual - this.totalExpenseActual; }


    printReport(): void {
        window.print();
    }


    exportCsv(): void {
        const lines: string[] = ['Section,Code,Category,Budgeted,Revised,Actual,Variance,%Used'];

        for (const r of this.revenueRows) {
            lines.push(`Revenue,${r.categoryCode},"${r.categoryName}",${r.budgetedAmount},${r.revisedAmount ?? ''},${r.actualAmount},${r.variance},${r.percentUsed}%`);
        }
        lines.push(`Revenue Total,,,${this.totalRevenueBudget},,${this.totalRevenueActual},${this.totalRevenueBudget - this.totalRevenueActual},`);

        for (const r of this.expenseRows) {
            lines.push(`Expense,${r.categoryCode},"${r.categoryName}",${r.budgetedAmount},${r.revisedAmount ?? ''},${r.actualAmount},${r.variance},${r.percentUsed}%`);
        }
        lines.push(`Expense Total,,,${this.totalExpenseBudget},,${this.totalExpenseActual},${this.totalExpenseBudget - this.totalExpenseActual},`);
        lines.push(`Net Income,,,${this.netBudgeted},,${this.netActual},${this.netBudgeted - this.netActual},`);

        const blob = new Blob([lines.join('\n')], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `budget-vs-actual-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        URL.revokeObjectURL(url);
    }


    goBack(): void {
        this.router.navigate(['/finances/dashboard']);
    }


    getVarianceClass(variance: number): string {
        if (variance > 0) return 'text-success';
        if (variance < 0) return 'text-danger';
        return '';
    }
}
