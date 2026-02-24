import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { SetComparisonService } from '../../services/set-comparison.service';
import { SetExplorerItem } from '../../services/set-explorer-api.service';


@Component({
    selector: 'app-set-comparison',
    templateUrl: './set-comparison.component.html',
    styleUrl: './set-comparison.component.scss'
})
export class SetComparisonComponent implements OnInit, OnDestroy {

    sets: SetExplorerItem[] = [];

    /** Comparison rows — each row is a label + values for each set */
    rows: { label: string; icon: string; values: string[]; highlight?: 'max' | 'min' }[] = [];

    private destroy$ = new Subject<void>();

    constructor(
        private router: Router,
        public comparisonService: SetComparisonService,
    ) { }

    ngOnInit(): void {
        this.comparisonService.sets$.pipe(takeUntil(this.destroy$)).subscribe(sets => {
            this.sets = sets;
            this.buildRows();
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    removeSet(id: number): void {
        this.comparisonService.removeSet(id);
    }

    clearAll(): void {
        this.comparisonService.clearAll();
    }

    openSet(set: SetExplorerItem): void {
        this.router.navigate(['/lego/sets', set.id]);
    }

    goBack(): void {
        this.router.navigate(['/lego/sets']);
    }


    private buildRows(): void {
        if (this.sets.length === 0) {
            this.rows = [];
            return;
        }

        const years = this.sets.map(s => s.year);
        const parts = this.sets.map(s => s.partCount);
        const maxYear = Math.max(...years);
        const maxParts = Math.max(...parts);
        const minParts = Math.min(...parts);

        this.rows = [
            {
                label: 'Set Number',
                icon: 'fas fa-hashtag',
                values: this.sets.map(s => s.setNumber)
            },
            {
                label: 'Year',
                icon: 'fas fa-calendar',
                values: this.sets.map(s => String(s.year)),
                highlight: 'max'
            },
            {
                label: 'Parts',
                icon: 'fas fa-puzzle-piece',
                values: this.sets.map(s => s.partCount.toLocaleString()),
                highlight: 'max'
            },
            {
                label: 'Theme',
                icon: 'fas fa-layer-group',
                values: this.sets.map(s => s.themeName ?? '—')
            },
            {
                label: 'Parts / Year Ratio',
                icon: 'fas fa-chart-line',
                values: this.sets.map(s => {
                    const age = new Date().getFullYear() - s.year + 1;
                    return (s.partCount / Math.max(age, 1)).toFixed(0);
                }),
                highlight: 'max'
            },
            {
                label: 'Set Age',
                icon: 'fas fa-hourglass-half',
                values: this.sets.map(s => {
                    const age = new Date().getFullYear() - s.year;
                    return age === 0 ? 'Current year' : `${age} year${age !== 1 ? 's' : ''}`;
                })
            },
            {
                label: 'Has Image',
                icon: 'fas fa-image',
                values: this.sets.map(s => s.imageUrl ? '✓' : '✗')
            }
        ];
    }

    /** Returns true if this cell holds the max value in its row */
    isMax(row: { highlight?: string; values: string[] }, colIdx: number): boolean {
        if (row.highlight !== 'max') return false;
        const nums = row.values.map(v => parseInt(v.replace(/,/g, ''), 10));
        return nums[colIdx] === Math.max(...nums);
    }
}
