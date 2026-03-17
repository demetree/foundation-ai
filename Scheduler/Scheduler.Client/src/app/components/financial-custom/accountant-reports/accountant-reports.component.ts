// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { FinancialTransactionService, FinancialTransactionData } from '../../../scheduler-data-services/financial-transaction.service';
import { FinancialCategoryService, FinancialCategoryData } from '../../../scheduler-data-services/financial-category.service';
import { FiscalPeriodService, FiscalPeriodData } from '../../../scheduler-data-services/fiscal-period.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import jsPDF from 'jspdf';


//
// Trial Balance row
//
interface TrialBalanceRow {
    code: string;
    name: string;
    accountType: string;
    debit: number;
    credit: number;
}


//
// Chart of Accounts row
//
interface CoaRow {
    code: string;
    name: string;
    accountType: string;
    externalId: string;
    level: number;         // Indent depth (0 = top)
    hasChildren: boolean;
}


//
// Journal row
//
interface JournalRow {
    date: string;
    code: string;
    category: string;
    description: string;
    amount: number;
    type: string;  // Revenue / Expense
}


@Component({
    selector: 'app-accountant-reports',
    templateUrl: './accountant-reports.component.html',
    styleUrls: ['./accountant-reports.component.scss']
})
export class AccountantReportsComponent implements OnInit {

    public isLoading = true;
    public activeTab: 'trial-balance' | 'coa' | 'journal' = 'trial-balance';

    public offices: FinancialOfficeData[] = [];
    public fiscalPeriods: FiscalPeriodData[] = [];
    public selectedOfficeId: number | null = null;
    public selectedFiscalPeriodId: number | null = null;
    public selectedOfficeName = 'All Offices';
    public selectedPeriodName = 'All Periods';
    public reportDate = new Date().toLocaleDateString('en-CA', { year: 'numeric', month: 'long', day: 'numeric' });

    // Data
    private allCategories: FinancialCategoryData[] = [];
    private allTransactions: FinancialTransactionData[] = [];

    // Trial Balance
    public trialBalanceRows: TrialBalanceRow[] = [];
    public totalDebits = 0;
    public totalCredits = 0;
    public netAmount = 0;          // abs(revenue - expenses)
    public netLabel = 'Net Income'; // 'Net Income' or 'Net Loss'
    public isBalanced = true;

    // Chart of Accounts
    public coaRows: CoaRow[] = [];

    // Journal
    public journalRows: JournalRow[] = [];
    public journalSortCol: 'date' | 'code' | 'amount' = 'date';
    public journalSortDir: 'asc' | 'desc' = 'desc';
    public journalSearch = '';


    constructor(
        private transactionService: FinancialTransactionService,
        private categoryService: FinancialCategoryService,
        private fiscalPeriodService: FiscalPeriodService,
        private officeService: FinancialOfficeService,
        private alertService: AlertService,
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute
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

                //
                // Pre-select filters from dashboard query params
                //
                const qp = this.route.snapshot.queryParams;
                if (qp['officeId']) {
                    this.selectedOfficeId = Number(qp['officeId']);
                    this.selectedOfficeName = this.offices.find(o => Number(o.id) === this.selectedOfficeId)?.name ?? 'All Offices';
                }
                if (qp['year']) {
                    const year = Number(qp['year']);
                    const matchingPeriod = this.fiscalPeriods.find(p => Number(p.fiscalYear) === year);
                    if (matchingPeriod) {
                        this.selectedFiscalPeriodId = Number(matchingPeriod.id);
                        this.selectedPeriodName = matchingPeriod.name ?? 'Unknown';
                    }
                }

                this.loadData();
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
        this.loadData();
    }


    private loadData(): void {
        this.isLoading = true;

        const catParams: any = { active: true, deleted: false, includeRelations: true };
        const txParams: any = { active: true, deleted: false, includeRelations: true };
        if (this.selectedOfficeId) {
            catParams.financialOfficeId = this.selectedOfficeId;
            txParams.financialOfficeId = this.selectedOfficeId;
        }

        forkJoin({
            categories: this.categoryService.GetFinancialCategoryList(catParams),
            transactions: this.transactionService.GetFinancialTransactionList(txParams)
        }).subscribe({
            next: ({ categories, transactions }) => {
                this.allCategories = categories ?? [];
                let filteredTx = transactions ?? [];

                //
                // Filter transactions by fiscal period date range when a period is selected
                //
                if (this.selectedFiscalPeriodId) {
                    const period = this.fiscalPeriods.find(p => Number(p.id) === this.selectedFiscalPeriodId);
                    if (period?.startDate && period?.endDate) {
                        const periodStart = new Date(period.startDate).getTime();
                        const periodEnd = new Date(period.endDate).getTime();
                        filteredTx = filteredTx.filter(tx => {
                            if (!tx.transactionDate) return false;
                            const txTime = new Date(tx.transactionDate).getTime();
                            return txTime >= periodStart && txTime <= periodEnd;
                        });
                    }
                }

                this.allTransactions = filteredTx;
                this.buildTrialBalance();
                this.buildChartOfAccounts();
                this.buildJournal();
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load data', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    // ═══════════════════════════════════════
    // Trial Balance
    // ═══════════════════════════════════════
    private buildTrialBalance(): void {
        const amountByCat = new Map<number, number>();
        for (const tx of this.allTransactions) {
            const catId = Number(tx.financialCategoryId);
            amountByCat.set(catId, (amountByCat.get(catId) || 0) + (tx.totalAmount || 0));
        }

        this.trialBalanceRows = this.allCategories.map(cat => {
            const amount = amountByCat.get(Number(cat.id)) || 0;
            const isRevenue = cat.accountType?.isRevenue ?? false;
            return {
                code: cat.code ?? '',
                name: cat.name ?? '',
                accountType: cat.accountType?.name ?? 'Unknown',
                debit: isRevenue ? 0 : amount,
                credit: isRevenue ? amount : 0,
            };
        }).filter(r => r.debit !== 0 || r.credit !== 0)
            .sort((a, b) => a.code.localeCompare(b.code));

        // Compute raw totals before the balancing row
        const rawDebits = this.trialBalanceRows.reduce((s, r) => s + r.debit, 0);
        const rawCredits = this.trialBalanceRows.reduce((s, r) => s + r.credit, 0);
        const difference = rawCredits - rawDebits;

        // Add a Net Income / Net Loss balancing row
        // Revenue > Expenses → Net Income placed on debit side to balance
        // Expenses > Revenue → Net Loss placed on credit side to balance
        this.netAmount = Math.abs(difference);
        if (Math.abs(difference) >= 0.01) {
            if (difference > 0) {
                this.netLabel = 'Net Income';
                this.trialBalanceRows.push({
                    code: '',
                    name: 'Net Income',
                    accountType: 'Equity',
                    debit: this.netAmount,
                    credit: 0,
                });
            } else {
                this.netLabel = 'Net Loss';
                this.trialBalanceRows.push({
                    code: '',
                    name: 'Net Loss',
                    accountType: 'Equity',
                    debit: 0,
                    credit: this.netAmount,
                });
            }
        } else {
            this.netLabel = 'Balanced';
        }

        this.totalDebits = this.trialBalanceRows.reduce((s, r) => s + r.debit, 0);
        this.totalCredits = this.trialBalanceRows.reduce((s, r) => s + r.credit, 0);
        this.isBalanced = Math.abs(this.totalDebits - this.totalCredits) < 0.01;
    }


    // ═══════════════════════════════════════
    // Chart of Accounts
    // ═══════════════════════════════════════
    private buildChartOfAccounts(): void {
        const catMap = new Map<number, FinancialCategoryData>();
        for (const cat of this.allCategories) {
            catMap.set(Number(cat.id), cat);
        }

        // Build tree, flattened with indent
        const topLevel = this.allCategories.filter(c => !c.parentFinancialCategoryId);
        this.coaRows = [];
        const addLevel = (cats: FinancialCategoryData[], level: number) => {
            for (const cat of cats.sort((a, b) => (a.code ?? '').localeCompare(b.code ?? ''))) {
                const children = this.allCategories.filter(c => Number(c.parentFinancialCategoryId) === Number(cat.id));
                this.coaRows.push({
                    code: cat.code ?? '',
                    name: cat.name ?? '',
                    accountType: cat.accountType?.name ?? '',
                    externalId: cat.externalAccountId ?? '',
                    level,
                    hasChildren: children.length > 0,
                });
                if (children.length > 0) {
                    addLevel(children, level + 1);
                }
            }
        };
        addLevel(topLevel, 0);
    }


    // ═══════════════════════════════════════
    // Transaction Journal
    // ═══════════════════════════════════════
    private buildJournal(): void {
        this.journalRows = this.allTransactions.map(tx => ({
            date: tx.transactionDate ?? '',
            code: tx.financialCategory?.code ?? '',
            category: tx.financialCategory?.name ?? 'Uncategorized',
            description: tx.description ?? '',
            amount: tx.totalAmount ?? 0,
            type: tx.isRevenue ? 'Revenue' : 'Expense',
        }));
        this.sortJournal();
    }


    get filteredJournal(): JournalRow[] {
        if (!this.journalSearch) return this.journalRows;
        const q = this.journalSearch.toLowerCase();
        return this.journalRows.filter(r =>
            r.category.toLowerCase().includes(q) ||
            r.description.toLowerCase().includes(q) ||
            r.code.toLowerCase().includes(q)
        );
    }


    sortJournal(col?: 'date' | 'code' | 'amount'): void {
        if (col) {
            if (this.journalSortCol === col) {
                this.journalSortDir = this.journalSortDir === 'asc' ? 'desc' : 'asc';
            } else {
                this.journalSortCol = col;
                this.journalSortDir = col === 'date' ? 'desc' : 'asc';
            }
        }

        const dir = this.journalSortDir === 'asc' ? 1 : -1;
        this.journalRows.sort((a, b) => {
            switch (this.journalSortCol) {
                case 'date': return a.date.localeCompare(b.date) * dir;
                case 'code': return a.code.localeCompare(b.code) * dir;
                case 'amount': return (a.amount - b.amount) * dir;
                default: return 0;
            }
        });
    }


    sortIcon(col: string): string {
        if (this.journalSortCol !== col) return 'fa-sort';
        return this.journalSortDir === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    }


    // ═══════════════════════════════════════
    // Export
    // ═══════════════════════════════════════
    printReport(): void { window.print(); }


    exportCsv(): void {
        let lines: string[] = [];

        if (this.activeTab === 'trial-balance') {
            lines.push('Code,Account,Account Type,Debit,Credit');
            for (const r of this.trialBalanceRows) {
                lines.push(`${r.code},"${r.name}","${r.accountType}",${r.debit.toFixed(2)},${r.credit.toFixed(2)}`);
            }
            lines.push(`Total,,,${this.totalDebits.toFixed(2)},${this.totalCredits.toFixed(2)}`);
        } else if (this.activeTab === 'coa') {
            lines.push('Code,Account,Type,External ID');
            for (const r of this.coaRows) {
                const indent = '  '.repeat(r.level);
                lines.push(`${r.code},"${indent}${r.name}","${r.accountType}","${r.externalId}"`);
            }
        } else {
            lines.push('Date,Code,Category,Description,Amount,Type');
            for (const r of this.filteredJournal) {
                lines.push(`${r.date},${r.code},"${r.category}","${r.description}",${r.amount.toFixed(2)},${r.type}`);
            }
        }

        const blob = new Blob([lines.join('\n')], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.activeTab}-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        URL.revokeObjectURL(url);
    }


    exportPdf(): void {
        const doc = new jsPDF();
        const pw = doc.internal.pageSize.getWidth();
        let y = 20;

        // Title
        const titleMap: Record<string, string> = {
            'trial-balance': 'Trial Balance',
            'coa': 'Chart of Accounts',
            'journal': 'Transaction Journal'
        };
        doc.setFontSize(16);
        doc.text(titleMap[this.activeTab], pw / 2, y, { align: 'center' });
        y += 7;
        doc.setFontSize(9);
        doc.text(`${this.selectedOfficeName}  •  ${this.selectedPeriodName}  •  ${this.reportDate}`, pw / 2, y, { align: 'center' });
        y += 10;

        doc.setFontSize(8);
        doc.setFont('helvetica', 'normal');

        if (this.activeTab === 'trial-balance') {
            // Header
            doc.setFont('helvetica', 'bold');
            doc.text('Code', 14, y); doc.text('Account', 40, y);
            doc.text('Debit', pw - 50, y, { align: 'right' }); doc.text('Credit', pw - 14, y, { align: 'right' });
            y += 2; doc.line(14, y, pw - 14, y); y += 5;
            doc.setFont('helvetica', 'normal');

            for (const r of this.trialBalanceRows) {
                doc.text(r.code, 14, y);
                doc.text(r.name.substring(0, 40), 40, y);
                if (r.debit) doc.text(`$${r.debit.toFixed(2)}`, pw - 50, y, { align: 'right' });
                if (r.credit) doc.text(`$${r.credit.toFixed(2)}`, pw - 14, y, { align: 'right' });
                y += 4.5;
                if (y > 275) { doc.addPage(); y = 20; }
            }

            y += 2; doc.line(14, y, pw - 14, y); y += 5;
            doc.setFont('helvetica', 'bold');
            doc.text('TOTAL', 14, y);
            doc.text(`$${this.totalDebits.toFixed(2)}`, pw - 50, y, { align: 'right' });
            doc.text(`$${this.totalCredits.toFixed(2)}`, pw - 14, y, { align: 'right' });

        } else if (this.activeTab === 'coa') {
            for (const r of this.coaRows) {
                const indent = r.level * 8;
                doc.setFont('helvetica', r.hasChildren ? 'bold' : 'normal');
                doc.text(r.code, 14, y);
                doc.text(r.name, 40 + indent, y);
                doc.text(r.accountType, pw - 50, y, { align: 'right' });
                y += 4.5;
                if (y > 275) { doc.addPage(); y = 20; }
            }

        } else {
            doc.setFont('helvetica', 'bold');
            doc.text('Date', 14, y); doc.text('Category', 45, y);
            doc.text('Amount', pw - 14, y, { align: 'right' });
            y += 2; doc.line(14, y, pw - 14, y); y += 5;
            doc.setFont('helvetica', 'normal');

            for (const r of this.filteredJournal) {
                const dateStr = r.date ? new Date(r.date).toLocaleDateString('en-CA') : '';
                doc.text(dateStr, 14, y);
                doc.text(r.category.substring(0, 35), 45, y);
                doc.text(`$${r.amount.toFixed(2)}`, pw - 14, y, { align: 'right' });
                y += 4.5;
                if (y > 275) { doc.addPage(); y = 20; }
            }
        }

        doc.save(`${this.activeTab}-${new Date().toISOString().slice(0, 10)}.pdf`);
    }


    formatDate(dateStr: string): string {
        if (!dateStr) return '—';
        try {
            return new Date(dateStr).toLocaleDateString('en-CA', { year: 'numeric', month: 'short', day: 'numeric' });
        } catch { return dateStr; }
    }

    goBack(): void {
        const params: any = {};
        if (this.selectedFiscalPeriodId) {
            const period = this.fiscalPeriods.find(p => Number(p.id) === this.selectedFiscalPeriodId);
            if (period?.fiscalYear) params.year = Number(period.fiscalYear);
        }
        if (this.selectedOfficeId) params.officeId = this.selectedOfficeId;
        this.router.navigate(['/finances'], { queryParams: params });
    }
}
