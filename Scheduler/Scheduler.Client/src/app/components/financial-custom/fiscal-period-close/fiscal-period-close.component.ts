// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { FiscalPeriodService, FiscalPeriodData, FiscalPeriodSubmitData } from '../../../scheduler-data-services/fiscal-period.service';


interface PeriodRow {
    period: FiscalPeriodData;
    isClosed: boolean;
    dateRange: string;
}


@Component({
    selector: 'app-fiscal-period-close',
    templateUrl: './fiscal-period-close.component.html',
    styleUrls: ['./fiscal-period-close.component.scss']
})
export class FiscalPeriodCloseComponent implements OnInit {

    public isLoading = true;
    public isSaving = false;
    public allRows: PeriodRow[] = [];
    public fiscalYears: number[] = [];
    public selectedYear: number | null = null;

    // Confirmation state
    public confirmAction: 'close' | 'reopen' | null = null;
    public confirmPeriod: PeriodRow | null = null;

    public openCount = 0;
    public closedCount = 0;

    constructor(
        private fiscalPeriodService: FiscalPeriodService,
        private alertService: AlertService,
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute
    ) { }


    ngOnInit(): void {
        this.loadPeriods();
    }


    private loadPeriods(): void {
        this.isLoading = true;
        this.fiscalPeriodService.GetFiscalPeriodList({ active: true, deleted: false, includeRelations: true }).subscribe({
            next: (periods) => {
                this.allRows = (periods ?? [])
                    .sort((a, b) => {
                        const yearDiff = Number(b.fiscalYear) - Number(a.fiscalYear);
                        if (yearDiff !== 0) return yearDiff;
                        return Number(a.periodNumber) - Number(b.periodNumber);
                    })
                    .map(p => ({
                        period: p,
                        isClosed: !!p.closedDate,
                        dateRange: this.formatDateRange(p.startDate, p.endDate),
                    }));

                // Extract unique years
                const yearSet = new Set(this.allRows.map(r => Number(r.period.fiscalYear)));
                this.fiscalYears = Array.from(yearSet).sort((a, b) => b - a);

                this.updateCounts();

                //
                // Pre-select year from dashboard query param
                //
                const qp = this.route.snapshot.queryParams;
                if (qp['year']) {
                    const year = Number(qp['year']);
                    if (this.fiscalYears.includes(year)) {
                        this.selectedYear = year;
                        this.updateCounts();
                    }
                }

                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load fiscal periods', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private updateCounts(): void {
        const filtered = this.filteredRows;
        this.openCount = filtered.filter(r => !r.isClosed).length;
        this.closedCount = filtered.filter(r => r.isClosed).length;
    }


    get filteredRows(): PeriodRow[] {
        if (!this.selectedYear) return this.allRows;
        return this.allRows.filter(r => Number(r.period.fiscalYear) === this.selectedYear);
    }


    onYearChange(): void {
        this.updateCounts();
    }


    requestClose(row: PeriodRow): void {
        this.confirmAction = 'close';
        this.confirmPeriod = row;
    }


    requestReopen(row: PeriodRow): void {
        this.confirmAction = 'reopen';
        this.confirmPeriod = row;
    }


    cancelConfirm(): void {
        this.confirmAction = null;
        this.confirmPeriod = null;
    }


    executeAction(): void {
        if (!this.confirmPeriod || !this.confirmAction) return;

        this.isSaving = true;
        const period = this.confirmPeriod.period;

        const submitData: FiscalPeriodSubmitData = period.ConvertToSubmitData();

        if (this.confirmAction === 'close') {
            submitData.closedDate = new Date().toISOString();
            submitData.closedBy = this.authService.currentUser?.userName ?? 'system';
        } else {
            submitData.closedDate = null;
            submitData.closedBy = null;
        }

        this.fiscalPeriodService.PutFiscalPeriod(period.id, submitData).subscribe({
            next: () => {
                const verb = this.confirmAction === 'close' ? 'closed' : 'reopened';
                this.alertService.showMessage(`Period ${verb}`, period.name, MessageSeverity.success);
                this.cancelConfirm();
                this.isSaving = false;
                this.fiscalPeriodService.ClearAllCaches();
                this.loadPeriods();
            },
            error: () => {
                this.alertService.showMessage('Failed to update period', '', MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }


    private formatDateRange(start: string, end: string): string {
        try {
            const s = new Date(start).toLocaleDateString('en-CA', { year: 'numeric', month: 'short', day: 'numeric' });
            const e = new Date(end).toLocaleDateString('en-CA', { year: 'numeric', month: 'short', day: 'numeric' });
            return `${s} — ${e}`;
        } catch { return '—'; }
    }


    formatDate(dateStr: string | null): string {
        if (!dateStr) return '—';
        try {
            return new Date(dateStr).toLocaleDateString('en-CA', { year: 'numeric', month: 'short', day: 'numeric' });
        } catch { return dateStr; }
    }

    goBack(): void {
        const params: any = {};
        if (this.selectedYear) params.year = this.selectedYear;
        this.router.navigate(['/finances'], { queryParams: params });
    }
}
