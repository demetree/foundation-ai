// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { FinancialTransactionService, FinancialTransactionData } from '../../../scheduler-data-services/financial-transaction.service';
import { FinancialCategoryService, FinancialCategoryData } from '../../../scheduler-data-services/financial-category.service';
import { FiscalPeriodService, FiscalPeriodData } from '../../../scheduler-data-services/fiscal-period.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import jsPDF from 'jspdf';


interface PnlLineItem {
    categoryCode: string;
    categoryName: string;
    amount: number;
}


@Component({
    selector: 'app-pnl-report',
    templateUrl: './pnl-report.component.html',
    styleUrls: ['./pnl-report.component.scss']
})
export class PnlReportComponent implements OnInit {

    public isLoading = true;
    public offices: FinancialOfficeData[] = [];
    public fiscalPeriods: FiscalPeriodData[] = [];
    public selectedOfficeId: number | null = null;
    public selectedFiscalPeriodId: number | null = null;

    public revenueItems: PnlLineItem[] = [];
    public expenseItems: PnlLineItem[] = [];

    public totalRevenue = 0;
    public totalExpenses = 0;
    public netIncome = 0;

    public selectedOfficeName = 'All Offices';
    public selectedPeriodName = 'Current Period';
    public reportDate = new Date().toLocaleDateString('en-CA', { year: 'numeric', month: 'long', day: 'numeric' });

    constructor(
        private transactionService: FinancialTransactionService,
        private categoryService: FinancialCategoryService,
        private fiscalPeriodService: FiscalPeriodService,
        private officeService: FinancialOfficeService,
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

        const txParams: any = { active: true, deleted: false, includeRelations: true };
        if (this.selectedOfficeId) txParams.financialOfficeId = this.selectedOfficeId;

        this.transactionService.GetFinancialTransactionList(txParams).subscribe({
            next: (transactions) => {
                this.buildPnl(transactions ?? []);
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load transactions', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private buildPnl(transactions: FinancialTransactionData[]): void {
        // Group by category, split revenue vs expense
        const revMap = new Map<string, PnlLineItem>();
        const expMap = new Map<string, PnlLineItem>();

        for (const tx of transactions) {
            const catName = tx.financialCategory?.name ?? 'Uncategorized';
            const catCode = tx.financialCategory?.code ?? '';
            const key = catCode || catName;
            const amount = tx.totalAmount ?? 0;

            if (tx.isRevenue) {
                const existing = revMap.get(key);
                if (existing) {
                    existing.amount += amount;
                } else {
                    revMap.set(key, { categoryCode: catCode, categoryName: catName, amount });
                }
            } else {
                const existing = expMap.get(key);
                if (existing) {
                    existing.amount += amount;
                } else {
                    expMap.set(key, { categoryCode: catCode, categoryName: catName, amount });
                }
            }
        }

        this.revenueItems = Array.from(revMap.values()).sort((a, b) => a.categoryCode.localeCompare(b.categoryCode));
        this.expenseItems = Array.from(expMap.values()).sort((a, b) => a.categoryCode.localeCompare(b.categoryCode));

        this.totalRevenue = this.revenueItems.reduce((s, r) => s + r.amount, 0);
        this.totalExpenses = this.expenseItems.reduce((s, r) => s + r.amount, 0);
        this.netIncome = this.totalRevenue - this.totalExpenses;
    }


    printReport(): void {
        window.print();
    }


    exportCsv(): void {
        const lines: string[] = ['Section,Code,Category,Amount'];

        for (const r of this.revenueItems) {
            lines.push(`Revenue,${r.categoryCode},"${r.categoryName}",${r.amount.toFixed(2)}`);
        }
        lines.push(`Revenue Total,,,${this.totalRevenue.toFixed(2)}`);
        lines.push('');

        for (const r of this.expenseItems) {
            lines.push(`Expense,${r.categoryCode},"${r.categoryName}",${r.amount.toFixed(2)}`);
        }
        lines.push(`Expense Total,,,${this.totalExpenses.toFixed(2)}`);
        lines.push('');
        lines.push(`Net Income,,,${this.netIncome.toFixed(2)}`);

        const blob = new Blob([lines.join('\n')], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `pnl-report-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        URL.revokeObjectURL(url);
    }


    exportPdf(): void {
        const doc = new jsPDF();
        const pageWidth = doc.internal.pageSize.getWidth();
        let y = 20;

        // Header
        doc.setFontSize(18);
        doc.text('Income Statement (P&L)', pageWidth / 2, y, { align: 'center' });
        y += 8;
        doc.setFontSize(10);
        doc.text(`${this.selectedOfficeName}  •  ${this.selectedPeriodName}  •  ${this.reportDate}`, pageWidth / 2, y, { align: 'center' });
        y += 12;

        // Revenue section
        doc.setFontSize(12);
        doc.setFont('helvetica', 'bold');
        doc.text('REVENUE', 14, y);
        y += 7;

        doc.setFontSize(9);
        doc.setFont('helvetica', 'normal');
        for (const item of this.revenueItems) {
            doc.text(`${item.categoryCode}  ${item.categoryName}`, 18, y);
            doc.text(`$${item.amount.toFixed(2)}`, pageWidth - 14, y, { align: 'right' });
            y += 5;
            if (y > 270) { doc.addPage(); y = 20; }
        }

        y += 2;
        doc.setFont('helvetica', 'bold');
        doc.text('Total Revenue', 18, y);
        doc.text(`$${this.totalRevenue.toFixed(2)}`, pageWidth - 14, y, { align: 'right' });
        y += 3;
        doc.line(14, y, pageWidth - 14, y);
        y += 10;

        // Expense section
        doc.setFontSize(12);
        doc.text('EXPENSES', 14, y);
        y += 7;

        doc.setFontSize(9);
        doc.setFont('helvetica', 'normal');
        for (const item of this.expenseItems) {
            doc.text(`${item.categoryCode}  ${item.categoryName}`, 18, y);
            doc.text(`$${item.amount.toFixed(2)}`, pageWidth - 14, y, { align: 'right' });
            y += 5;
            if (y > 270) { doc.addPage(); y = 20; }
        }

        y += 2;
        doc.setFont('helvetica', 'bold');
        doc.text('Total Expenses', 18, y);
        doc.text(`$${this.totalExpenses.toFixed(2)}`, pageWidth - 14, y, { align: 'right' });
        y += 3;
        doc.line(14, y, pageWidth - 14, y);
        y += 12;

        // Net Income
        doc.setFontSize(14);
        doc.text('NET INCOME', 14, y);
        doc.text(`$${this.netIncome.toFixed(2)}`, pageWidth - 14, y, { align: 'right' });
        y += 3;
        doc.setLineWidth(0.5);
        doc.line(14, y, pageWidth - 14, y);
        y += 1;
        doc.line(14, y, pageWidth - 14, y); // Double underline

        doc.save(`pnl-report-${new Date().toISOString().slice(0, 10)}.pdf`);
    }


    goBack(): void {
        this.router.navigate(['/finances']);
    }
}
