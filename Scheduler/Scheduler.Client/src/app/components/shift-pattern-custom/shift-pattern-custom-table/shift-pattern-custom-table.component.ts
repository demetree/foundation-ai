/**
 * ShiftPatternCustomTableComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Displays ShiftPattern records in a sortable table with mobile card fallback.
 * Follows the same pattern as ShiftCustomTableComponent.
 */
import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { ShiftPatternService, ShiftPatternData } from '../../../scheduler-data-services/shift-pattern.service';
import { ShiftPatternCustomAddEditComponent } from '../shift-pattern-custom-add-edit/shift-pattern-custom-add-edit.component';
import { ConfirmationService } from '../../../services/confirmation-service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

type SortColumn = 'name' | 'description' | 'color' | 'active';

@Component({
    selector: 'app-shift-pattern-custom-table',
    templateUrl: './shift-pattern-custom-table.component.html',
    styleUrls: ['./shift-pattern-custom-table.component.scss']
})
export class ShiftPatternCustomTableComponent implements OnInit, OnDestroy {

    @ViewChild('addEditComponent') addEditComponent!: ShiftPatternCustomAddEditComponent;

    @Input() filterText = '';

    patterns: ShiftPatternData[] = [];
    filteredPatterns: ShiftPatternData[] = [];
    isLoading = true;

    sortColumn: SortColumn = 'name';
    sortDirection: 'asc' | 'desc' = 'asc';

    isMobile = false;

    private destroy$ = new Subject<void>();

    constructor(
        private patternService: ShiftPatternService,
        private confirmationService: ConfirmationService,
        private alertService: AlertService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.checkMobile();
        window.addEventListener('resize', this.checkMobile.bind(this));
        this.loadPatterns();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        window.removeEventListener('resize', this.checkMobile.bind(this));
    }

    // ── Data Loading ──

    loadPatterns(): void {
        this.isLoading = true;
        this.patternService.GetShiftPatternList({ includeRelations: true })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (data) => {
                    this.patterns = data;
                    this.applyFilter();
                    this.isLoading = false;
                },
                error: () => {
                    this.isLoading = false;
                }
            });
    }

    // ── Filtering ──

    applyFilter(): void {
        const q = (this.filterText || '').toLowerCase().trim();
        if (!q) {
            this.filteredPatterns = [...this.patterns];
        } else {
            this.filteredPatterns = this.patterns.filter(p =>
                p.name?.toLowerCase().includes(q) ||
                p.description?.toLowerCase().includes(q)
            );
        }
        this.sortData();
    }

    // ── Sorting ──

    sort(column: SortColumn): void {
        if (this.sortColumn === column) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = column;
            this.sortDirection = 'asc';
        }
        this.sortData();
    }

    sortIcon(column: SortColumn): string {
        if (this.sortColumn !== column) return 'fa-sort';
        return this.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    }

    private sortData(): void {
        this.filteredPatterns.sort((a, b) => {
            let valA: any, valB: any;
            switch (this.sortColumn) {
                case 'name': valA = a.name?.toLowerCase() || ''; valB = b.name?.toLowerCase() || ''; break;
                case 'description': valA = a.description?.toLowerCase() || ''; valB = b.description?.toLowerCase() || ''; break;
                case 'color': valA = a.color || ''; valB = b.color || ''; break;
                case 'active': valA = a.active ? 1 : 0; valB = b.active ? 1 : 0; break;
            }
            const cmp = valA < valB ? -1 : valA > valB ? 1 : 0;
            return this.sortDirection === 'asc' ? cmp : -cmp;
        });
    }

    // ── Actions ──

    navigateToDetail(p: ShiftPatternData): void {
        this.router.navigate(['/shiftpattern', p.id]);
    }

    handleEdit(p: ShiftPatternData): void {
        this.addEditComponent?.openModal(p);
    }

    handleDelete(p: ShiftPatternData): void {
        this.confirmationService.confirm(
            `Are you sure you want to delete shift pattern "${p.name}"?`,
            'Delete Shift Pattern'
        ).then(confirmed => {
            if (confirmed) {
                this.patternService.DeleteShiftPattern(p.id)
                    .pipe(takeUntil(this.destroy$))
                    .subscribe({
                        next: () => {
                            this.patternService.ClearAllCaches();
                            this.alertService.showMessage(`Shift Pattern "${p.name}" deleted`, '', MessageSeverity.success);
                            this.loadPatterns();
                        },
                        error: (err) => {
                            this.alertService.showMessage('Error deleting Shift Pattern', err?.message || '', MessageSeverity.error);
                        }
                    });
            }
        });
    }

    onPatternChanged(): void {
        this.loadPatterns();
    }

    // ── Helpers ──

    private checkMobile(): void {
        this.isMobile = window.innerWidth < 768;
    }

    public userIsWriter(): boolean {
        return this.patternService.userIsSchedulerShiftPatternWriter();
    }

    getDayLabel(dayOfWeek: number): string {
        const days = ['', 'Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
        return days[dayOfWeek] || '?';
    }
}
