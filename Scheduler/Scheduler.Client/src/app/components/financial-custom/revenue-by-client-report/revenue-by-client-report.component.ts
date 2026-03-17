// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { InvoiceService, InvoiceData } from '../../../scheduler-data-services/invoice.service';
import { FiscalPeriodService, FiscalPeriodData } from '../../../scheduler-data-services/fiscal-period.service';


interface ClientRevenueRow {
    clientName: string;
    clientId: number;
    totalInvoiced: number;
    totalPaid: number;
    balanceOutstanding: number;
    invoiceCount: number;
}


@Component({
    selector: 'app-revenue-by-client-report',
    templateUrl: './revenue-by-client-report.component.html',
    styleUrls: ['./revenue-by-client-report.component.scss']
})
export class RevenueByClientReportComponent implements OnInit {

    public isLoading = true;
    public clientRows: ClientRevenueRow[] = [];
    public fiscalPeriods: FiscalPeriodData[] = [];
    public selectedFiscalPeriodId: number | null = null;
    public sortColumn = 'totalInvoiced';
    public sortAsc = false;

    public totalInvoiced = 0;
    public totalPaid = 0;
    public totalOutstanding = 0;
    public totalInvoiceCount = 0;

    public reportDate = new Date().toLocaleDateString('en-CA', { year: 'numeric', month: 'long', day: 'numeric' });

    constructor(
        private invoiceService: InvoiceService,
        private fiscalPeriodService: FiscalPeriodService,
        private alertService: AlertService,
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute
    ) { }


    ngOnInit(): void {
        this.loadFilters();
    }


    private loadFilters(): void {
        this.fiscalPeriodService.GetFiscalPeriodList({ active: true, deleted: false }).subscribe({
            next: (periods) => {
                this.fiscalPeriods = (periods ?? []).sort((a, b) => (a.startDate ?? '').localeCompare(b.startDate ?? ''));

                const qp = this.route.snapshot.queryParams;
                if (qp['year']) {
                    const year = Number(qp['year']);
                    const match = this.fiscalPeriods.find(p => Number(p.fiscalYear) === year);
                    if (match) this.selectedFiscalPeriodId = Number(match.id);
                }

                this.loadReport();
            },
            error: () => {
                this.alertService.showMessage('Failed to load periods', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    onFilterChange(): void {
        this.loadReport();
    }


    private loadReport(): void {
        this.isLoading = true;

        this.invoiceService.GetInvoiceList({
            active: true, deleted: false, includeRelations: true
        }).subscribe({
            next: (invoices) => {
                this.buildReport(invoices ?? []);
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load invoices', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private buildReport(invoices: InvoiceData[]): void {
        let filtered = invoices.filter(inv => {
            const status = (inv.invoiceStatus as any)?.name?.toLowerCase() || '';
            return status !== 'void' && status !== 'cancelled';
        });

        // Filter by fiscal period date range
        if (this.selectedFiscalPeriodId) {
            const period = this.fiscalPeriods.find(p => Number(p.id) === this.selectedFiscalPeriodId);
            if (period?.startDate && period?.endDate) {
                const start = new Date(period.startDate).getTime();
                const end = new Date(period.endDate).getTime();
                filtered = filtered.filter(inv => {
                    if (!inv.invoiceDate) return false;
                    const t = new Date(inv.invoiceDate).getTime();
                    return t >= start && t <= end;
                });
            }
        }

        const clientMap = new Map<string, ClientRevenueRow>();

        for (const inv of filtered) {
            const clientName = (inv.client as any)?.name || 'No Client';
            const clientId = (inv.client as any)?.id || 0;

            if (!clientMap.has(clientName)) {
                clientMap.set(clientName, {
                    clientName, clientId: Number(clientId),
                    totalInvoiced: 0, totalPaid: 0, balanceOutstanding: 0, invoiceCount: 0
                });
            }

            const row = clientMap.get(clientName)!;
            row.totalInvoiced += inv.totalAmount || 0;
            row.totalPaid += inv.amountPaid || 0;
            row.balanceOutstanding += (inv.totalAmount || 0) - (inv.amountPaid || 0);
            row.invoiceCount++;
        }

        this.clientRows = Array.from(clientMap.values());
        this.applySorting();

        this.totalInvoiced = this.clientRows.reduce((s, r) => s + r.totalInvoiced, 0);
        this.totalPaid = this.clientRows.reduce((s, r) => s + r.totalPaid, 0);
        this.totalOutstanding = this.clientRows.reduce((s, r) => s + r.balanceOutstanding, 0);
        this.totalInvoiceCount = this.clientRows.reduce((s, r) => s + r.invoiceCount, 0);
    }


    sortBy(column: string): void {
        if (this.sortColumn === column) {
            this.sortAsc = !this.sortAsc;
        } else {
            this.sortColumn = column;
            this.sortAsc = false;
        }
        this.applySorting();
    }


    private applySorting(): void {
        const dir = this.sortAsc ? 1 : -1;
        this.clientRows.sort((a, b) => {
            const av = (a as any)[this.sortColumn];
            const bv = (b as any)[this.sortColumn];
            if (typeof av === 'string') return av.localeCompare(bv) * dir;
            return ((av || 0) - (bv || 0)) * dir;
        });
    }


    getSortIcon(column: string): string {
        if (this.sortColumn !== column) return 'fa-sort';
        return this.sortAsc ? 'fa-sort-up' : 'fa-sort-down';
    }


    exportCsv(): void {
        const lines: string[] = ['Client,Invoices,Total Invoiced,Total Paid,Balance Outstanding'];
        for (const row of this.clientRows) {
            lines.push([
                `"${row.clientName}"`, row.invoiceCount,
                row.totalInvoiced.toFixed(2), row.totalPaid.toFixed(2), row.balanceOutstanding.toFixed(2)
            ].join(','));
        }
        lines.push('');
        lines.push(['TOTAL', this.totalInvoiceCount,
            this.totalInvoiced.toFixed(2), this.totalPaid.toFixed(2), this.totalOutstanding.toFixed(2)
        ].join(','));

        const blob = new Blob([lines.join('\n')], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `revenue-by-client-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        URL.revokeObjectURL(url);
    }


    printReport(): void { window.print(); }

    formatCurrency(amount: number): string { return '$' + amount.toFixed(2); }

    goBack(): void { this.router.navigate(['/finances']); }
}
