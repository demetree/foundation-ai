// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { InvoiceService, InvoiceData } from '../../../scheduler-data-services/invoice.service';


interface AgingBucket {
    label: string;
    min: number;        // inclusive
    max: number | null;  // exclusive, null = unbounded
}

interface AgingClientRow {
    clientName: string;
    current: number;     // 0 days
    days1to30: number;
    days31to60: number;
    days61to90: number;
    days90plus: number;
    total: number;
    invoices: AgingInvoiceRow[];
    expanded: boolean;
}

interface AgingInvoiceRow {
    invoiceNumber: string;
    invoiceId: number;
    dueDate: string;
    daysOverdue: number;
    totalAmount: number;
    amountPaid: number;
    balanceDue: number;
    bucket: string;
}


@Component({
    selector: 'app-ar-aging-report',
    templateUrl: './ar-aging-report.component.html',
    styleUrls: ['./ar-aging-report.component.scss']
})
export class ArAgingReportComponent implements OnInit {

    public isLoading = true;
    public clientRows: AgingClientRow[] = [];
    public reportDate = new Date().toLocaleDateString('en-CA', { year: 'numeric', month: 'long', day: 'numeric' });

    // Totals
    public totalCurrent = 0;
    public total1to30 = 0;
    public total31to60 = 0;
    public total61to90 = 0;
    public total90plus = 0;
    public grandTotal = 0;

    private readonly today = new Date();

    private readonly buckets: AgingBucket[] = [
        { label: 'Current', min: -999999, max: 1 },
        { label: '1–30', min: 1, max: 31 },
        { label: '31–60', min: 31, max: 61 },
        { label: '61–90', min: 61, max: 91 },
        { label: '90+', min: 91, max: null }
    ];

    constructor(
        private invoiceService: InvoiceService,
        private alertService: AlertService,
        private authService: AuthService,
        private router: Router
    ) { }


    ngOnInit(): void {
        this.loadReport();
    }


    private loadReport(): void {
        this.isLoading = true;

        // Load all invoices — filter to unpaid client-side
        this.invoiceService.GetInvoiceList({
            active: true,
            deleted: false,
            includeRelations: true,
            pageSize: 10000
        }).subscribe({
            next: (invoices) => {
                this.buildAgingReport(invoices ?? []);
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load invoices', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private buildAgingReport(invoices: InvoiceData[]): void {
        const clientMap = new Map<string, AgingClientRow>();

        for (const inv of invoices) {
            const statusName = (inv.invoiceStatus as any)?.name?.toLowerCase() || '';
            if (statusName === 'void' || statusName === 'cancelled') continue;

            const balanceDue = (inv.totalAmount || 0) - (inv.amountPaid || 0);
            if (balanceDue <= 0) continue;

            const clientName = (inv.client as any)?.name || 'No Client';
            const dueDate = inv.dueDate ? new Date(inv.dueDate) : this.today;
            const daysOverdue = Math.floor((this.today.getTime() - dueDate.getTime()) / (1000 * 60 * 60 * 24));

            const bucket = this.getBucketLabel(daysOverdue);

            const invoiceRow: AgingInvoiceRow = {
                invoiceNumber: inv.invoiceNumber || '—',
                invoiceId: Number(inv.id),
                dueDate: inv.dueDate || '',
                daysOverdue: Math.max(0, daysOverdue),
                totalAmount: inv.totalAmount || 0,
                amountPaid: inv.amountPaid || 0,
                balanceDue,
                bucket
            };

            if (!clientMap.has(clientName)) {
                clientMap.set(clientName, {
                    clientName,
                    current: 0,
                    days1to30: 0,
                    days31to60: 0,
                    days61to90: 0,
                    days90plus: 0,
                    total: 0,
                    invoices: [],
                    expanded: false
                });
            }

            const row = clientMap.get(clientName)!;
            row.invoices.push(invoiceRow);
            row.total += balanceDue;

            if (daysOverdue < 1) row.current += balanceDue;
            else if (daysOverdue < 31) row.days1to30 += balanceDue;
            else if (daysOverdue < 61) row.days31to60 += balanceDue;
            else if (daysOverdue < 91) row.days61to90 += balanceDue;
            else row.days90plus += balanceDue;
        }

        this.clientRows = Array.from(clientMap.values())
            .sort((a, b) => b.total - a.total);

        // Compute column totals
        this.totalCurrent = this.clientRows.reduce((s, r) => s + r.current, 0);
        this.total1to30 = this.clientRows.reduce((s, r) => s + r.days1to30, 0);
        this.total31to60 = this.clientRows.reduce((s, r) => s + r.days31to60, 0);
        this.total61to90 = this.clientRows.reduce((s, r) => s + r.days61to90, 0);
        this.total90plus = this.clientRows.reduce((s, r) => s + r.days90plus, 0);
        this.grandTotal = this.clientRows.reduce((s, r) => s + r.total, 0);
    }


    private getBucketLabel(daysOverdue: number): string {
        if (daysOverdue < 1) return 'Current';
        if (daysOverdue < 31) return '1–30';
        if (daysOverdue < 61) return '31–60';
        if (daysOverdue < 91) return '61–90';
        return '90+';
    }


    toggleClient(row: AgingClientRow): void {
        row.expanded = !row.expanded;
    }


    goToInvoice(invoiceId: number): void {
        this.router.navigate(['/finances/invoices', invoiceId]);
    }


    exportCsv(): void {
        const lines: string[] = ['Client,Current,1-30 Days,31-60 Days,61-90 Days,90+ Days,Total'];

        for (const row of this.clientRows) {
            lines.push([
                `"${row.clientName}"`,
                row.current.toFixed(2),
                row.days1to30.toFixed(2),
                row.days31to60.toFixed(2),
                row.days61to90.toFixed(2),
                row.days90plus.toFixed(2),
                row.total.toFixed(2)
            ].join(','));
        }

        lines.push('');
        lines.push([
            'TOTAL',
            this.totalCurrent.toFixed(2),
            this.total1to30.toFixed(2),
            this.total31to60.toFixed(2),
            this.total61to90.toFixed(2),
            this.total90plus.toFixed(2),
            this.grandTotal.toFixed(2)
        ].join(','));

        const blob = new Blob([lines.join('\n')], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `ar-aging-report-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        URL.revokeObjectURL(url);
    }


    printReport(): void {
        window.print();
    }


    formatCurrency(amount: number): string {
        return '$' + amount.toFixed(2);
    }


    formatDate(dateStr: string): string {
        if (!dateStr) return '—';
        try {
            return new Date(dateStr).toLocaleDateString('en-CA', {
                year: 'numeric', month: 'short', day: 'numeric'
            });
        } catch {
            return dateStr;
        }
    }


    goBack(): void {
        this.router.navigate(['/finances']);
    }
}
