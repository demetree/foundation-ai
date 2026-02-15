import { Component, ViewChild, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { VolunteerGroupService, VolunteerGroupData, VolunteerGroupQueryParameters } from '../../../scheduler-data-services/volunteer-group.service';
import { VolunteerGroupCustomAddEditComponent } from '../volunteer-group-custom-add-edit/volunteer-group-custom-add-edit.component';
import { VolunteerGroupCustomTableComponent } from '../volunteer-group-custom-table/volunteer-group-custom-table.component';
import { AuthService } from '../../../services/auth.service';
import { Location } from '@angular/common';

@Component({
    selector: 'app-volunteer-group-custom-listing',
    templateUrl: './volunteer-group-custom-listing.component.html',
    styleUrls: ['./volunteer-group-custom-listing.component.scss']
})
export class VolunteerGroupCustomListingComponent implements OnInit, OnDestroy, AfterViewInit {
    @ViewChild(VolunteerGroupCustomAddEditComponent) addEditComponent!: VolunteerGroupCustomAddEditComponent;
    @ViewChild(VolunteerGroupCustomTableComponent) tableComponent!: VolunteerGroupCustomTableComponent;

    public filterText: string = '';
    public isSmallScreen: boolean = false;

    public totalGroupCount$!: Observable<bigint | number>;
    public filteredGroupCount$!: Observable<bigint | number>;

    public loadingTotalCount: boolean = true;
    public loadingFilteredCount: boolean = false;

    private filterChanged$ = new Subject<string>();
    private destroy$ = new Subject<void>();

    constructor(
        private volunteerGroupService: VolunteerGroupService,
        private authService: AuthService,
        private breakpointObserver: BreakpointObserver,
        private location: Location
    ) { }

    ngOnInit(): void {
        this.breakpointObserver.observe([Breakpoints.Handset])
            .pipe(takeUntil(this.destroy$))
            .subscribe(result => {
                this.isSmallScreen = result.matches;
            });

        this.loadTotalCount();

        this.filterChanged$.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(filterText => {
            this.loadFilteredCount(filterText);
        });
    }

    ngAfterViewInit(): void {
        if (this.addEditComponent) {
            this.addEditComponent.volunteerGroupChanged.pipe(
                takeUntil(this.destroy$)
            ).subscribe({
                next: () => {
                    if (this.tableComponent) {
                        this.tableComponent.loadData();
                    }
                    this.loadTotalCount();
                }
            });
        }
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    onFilterChange(): void {
        this.filterChanged$.next(this.filterText);
        if (this.filterText) {
            this.loadingFilteredCount = true;
        }
    }

    private loadTotalCount(): void {
        this.loadingTotalCount = true;
        this.totalGroupCount$ = this.volunteerGroupService.GetVolunteerGroupsRowCount();
        this.totalGroupCount$.pipe(takeUntil(this.destroy$)).subscribe({
            next: () => this.loadingTotalCount = false,
            error: () => this.loadingTotalCount = false
        });
    }

    private loadFilteredCount(filterText: string): void {
        if (!filterText) {
            this.filteredGroupCount$ = this.totalGroupCount$;
            this.loadingFilteredCount = false;
            return;
        }
        this.filteredGroupCount$ = this.volunteerGroupService.GetVolunteerGroupsRowCount({ anyStringContains: filterText });
        this.filteredGroupCount$.pipe(takeUntil(this.destroy$)).subscribe({
            next: () => this.loadingFilteredCount = false,
            error: () => this.loadingFilteredCount = false
        });
    }

    public goBack(): void {
        this.location.back();
    }

    public canGoBack(): boolean {
        return true;
    }

    public userIsGroupReader(): boolean {
        return this.volunteerGroupService.userIsSchedulerVolunteerGroupReader();
    }

    public userIsGroupWriter(): boolean {
        return this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter();
    }
}
