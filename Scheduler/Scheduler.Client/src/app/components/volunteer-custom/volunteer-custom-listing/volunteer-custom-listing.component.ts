import { Component, ViewChild, OnInit, OnDestroy } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { VolunteerProfileService, VolunteerProfileData, VolunteerProfileQueryParameters } from '../../../scheduler-data-services/volunteer-profile.service';
import { VolunteerCustomAddEditComponent } from '../volunteer-custom-add-edit/volunteer-custom-add-edit.component';
import { VolunteerCustomTableComponent } from '../volunteer-custom-table/volunteer-custom-table.component';
import { AuthService } from '../../../services/auth.service';
import { Location } from '@angular/common';

@Component({
    selector: 'app-volunteer-custom-listing',
    templateUrl: './volunteer-custom-listing.component.html',
    styleUrls: ['./volunteer-custom-listing.component.scss']
})
export class VolunteerCustomListingComponent implements OnInit, OnDestroy {
    @ViewChild(VolunteerCustomAddEditComponent) addEditComponent!: VolunteerCustomAddEditComponent;
    @ViewChild(VolunteerCustomTableComponent) tableComponent!: VolunteerCustomTableComponent;

    public filterText: string = '';
    public isSmallScreen: boolean = false;

    // Row count observables
    public totalVolunteerCount$!: Observable<bigint | number>;
    public filteredVolunteerCount$!: Observable<bigint | number>;

    public loadingTotalCount: boolean = true;
    public loadingFilteredCount: boolean = false;

    private filterChanged$ = new Subject<string>();
    private destroy$ = new Subject<void>();

    constructor(
        private volunteerProfileService: VolunteerProfileService,
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
        this.totalVolunteerCount$ = this.volunteerProfileService.GetVolunteerProfilesRowCount();
        this.totalVolunteerCount$.pipe(takeUntil(this.destroy$)).subscribe({
            next: () => this.loadingTotalCount = false,
            error: () => this.loadingTotalCount = false
        });
    }

    private loadFilteredCount(filterText: string): void {
        if (!filterText) {
            this.filteredVolunteerCount$ = this.totalVolunteerCount$;
            this.loadingFilteredCount = false;
            return;
        }
        this.filteredVolunteerCount$ = this.volunteerProfileService.GetVolunteerProfilesRowCount({ anyStringContains: filterText });
        this.filteredVolunteerCount$.pipe(takeUntil(this.destroy$)).subscribe({
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

    public userIsVolunteerReader(): boolean {
        return this.volunteerProfileService.userIsSchedulerVolunteerProfileReader();
    }

    public userIsVolunteerWriter(): boolean {
        return this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter();
    }
}
