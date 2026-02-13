/**
 * ShiftPatternCustomListingComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Top-level listing page for ShiftPattern records.
 * Features a premium header with glassmorphic search and count badges.
 */
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { ShiftPatternService } from '../../../scheduler-data-services/shift-pattern.service';
import { ShiftPatternCustomTableComponent } from '../shift-pattern-custom-table/shift-pattern-custom-table.component';

@Component({
    selector: 'app-shift-pattern-custom-listing',
    templateUrl: './shift-pattern-custom-listing.component.html',
    styleUrls: ['./shift-pattern-custom-listing.component.scss']
})
export class ShiftPatternCustomListingComponent implements OnInit, OnDestroy {

    @ViewChild('tableComponent') tableComponent!: ShiftPatternCustomTableComponent;

    searchText = '';
    totalCount = 0;
    activeCount = 0;

    private destroy$ = new Subject<void>();

    constructor(private patternService: ShiftPatternService) { }

    ngOnInit(): void {
        this.loadCounts();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadCounts(): void {
        this.patternService.GetShiftPatternList()
            .pipe(takeUntil(this.destroy$))
            .subscribe(patterns => {
                this.totalCount = patterns.length;
                this.activeCount = patterns.filter(p => p.active).length;
            });
    }

    onSearchChange(): void {
        if (this.tableComponent) {
            this.tableComponent.filterText = this.searchText;
            this.tableComponent.applyFilter();
        }
    }

    onPatternChanged(): void {
        this.loadCounts();
        if (this.tableComponent) {
            this.tableComponent.loadPatterns();
        }
    }
}
