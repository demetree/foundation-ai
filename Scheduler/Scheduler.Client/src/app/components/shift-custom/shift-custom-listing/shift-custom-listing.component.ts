/**
 * ShiftCustomListingComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Top-level listing page for Resource Shifts (/resourceshifts).
 * Premium header with indigo/violet gradient, glass search, count badges,
 * and embeds the ShiftCustomTableComponent.
 */
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ResourceShiftService, ResourceShiftData } from '../../../scheduler-data-services/resource-shift.service';
import { ShiftCustomAddEditComponent } from '../shift-custom-add-edit/shift-custom-add-edit.component';
import { ShiftCustomTableComponent } from '../shift-custom-table/shift-custom-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-shift-custom-listing',
    templateUrl: './shift-custom-listing.component.html',
    styleUrls: ['./shift-custom-listing.component.scss']
})
export class ShiftCustomListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
    @ViewChild(ShiftCustomAddEditComponent) addEditShiftComponent!: ShiftCustomAddEditComponent;
    @ViewChild(ShiftCustomTableComponent) shiftTableComponent!: ShiftCustomTableComponent;

    public ResourceShifts: ResourceShiftData[] | null = null;
    public isSmallScreen = false;

    public filterText: string | null = null;

    public totalShiftCount$: Observable<number> | null = null;
    public filteredShiftCount$: Observable<number> | null = null;
    public loadingTotalCount = false;
    public loadingFilteredCount = false;

    private debounceTimeout: any;

    constructor(
        private shiftService: ResourceShiftService,
        private alertService: AlertService,
        private navigationService: NavigationService,
        private breakpointObserver: BreakpointObserver
    ) { }

    ngOnInit(): void {
        this.breakpointObserver
            .observe(['(max-width: 1100px)'])
            .subscribe((result) => {
                this.isSmallScreen = result.matches;
            });

        this.loadCounts();
    }

    ngAfterViewInit(): void {
        this.addEditShiftComponent.resourceShiftChanged.subscribe({
            next: () => {
                this.shiftTableComponent.loadData();
                this.loadCounts();
            },
            error: (err: any) => {
                this.alertService.showMessage('Error during Shift changed notification', JSON.stringify(err), MessageSeverity.error);
            }
        });
    }

    canDeactivate(): boolean {
        if (this.addEditShiftComponent?.modalIsDisplayed == true) {
            return false;
        }
        return true;
    }

    private loadCounts(): void {
        this.loadingTotalCount = true;
        this.loadingFilteredCount = true;

        this.totalShiftCount$ = this.shiftService.GetResourceShiftsRowCount({
            active: true,
            deleted: false
        }).pipe(
            map(c => Number(c ?? 0)),
            startWith(0),
            finalize(() => this.loadingTotalCount = false),
            shareReplay(1)
        );

        if (this.filterText) {
            this.filteredShiftCount$ = this.shiftService.GetResourceShiftsRowCount({
                active: true,
                deleted: false,
                anyStringContains: this.filterText || undefined
            }).pipe(
                map(c => Number(c ?? 0)),
                startWith(0),
                finalize(() => this.loadingFilteredCount = false),
                shareReplay(1)
            );
        } else {
            this.filteredShiftCount$ = this.totalShiftCount$;
            this.loadingFilteredCount = false;
        }

        this.totalShiftCount$.subscribe();
        if (this.filteredShiftCount$ !== this.totalShiftCount$) {
            this.filteredShiftCount$.subscribe();
        }
    }

    public reload(): void {
        this.shiftTableComponent.loadData();
    }

    public goBack(): void {
        this.navigationService.goBack();
    }

    public canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }

    public clearFilter(): void {
        this.filterText = '';
    }

    public onFilterChange(): void {
        clearTimeout(this.debounceTimeout);
        this.debounceTimeout = setTimeout(() => {
            this.shiftTableComponent.loadData();
            this.loadCounts();
        }, 500);
    }

    public userIsSchedulerResourceShiftReader(): boolean {
        return this.shiftService.userIsSchedulerResourceShiftReader();
    }

    public userIsSchedulerResourceShiftWriter(): boolean {
        return this.shiftService.userIsSchedulerResourceShiftWriter();
    }
}
