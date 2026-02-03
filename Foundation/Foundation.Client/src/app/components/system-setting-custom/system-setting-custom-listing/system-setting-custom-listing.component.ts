//
// System Setting Custom Listing Component
//
// Premium listing view for managing System Settings with glassmorphic styling,
// search, and modal-based add/edit. Following the user-custom pattern.
//

import { Component, OnInit, OnDestroy, HostListener, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Subject, BehaviorSubject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '../../../services/auth.service';
import { SystemSettingService, SystemSettingQueryParameters, SystemSettingData } from '../../../security-data-services/system-setting.service';
import { SystemSettingCustomAddEditComponent } from '../system-setting-custom-add-edit/system-setting-custom-add-edit.component';
import { SystemSettingCustomTableComponent } from '../system-setting-custom-table/system-setting-custom-table.component';

@Component({
    selector: 'app-system-setting-custom-listing',
    templateUrl: './system-setting-custom-listing.component.html',
    styleUrls: ['./system-setting-custom-listing.component.scss']
})
export class SystemSettingCustomListingComponent implements OnInit, OnDestroy {

    //
    // Lifecycle management
    //
    private destroy$ = new Subject<void>();

    //
    // Loading and counts
    //
    public loadingTotalCount: boolean = true;
    public loadingFilteredCount: boolean = false;
    public totalSettingCount$: BehaviorSubject<number | null> = new BehaviorSubject<number | null>(null);
    public filteredSettingCount$: BehaviorSubject<number | null> = new BehaviorSubject<number | null>(null);

    //
    // Filter state
    //
    public filterText: string = '';
    private filterTextSubject = new Subject<string>();

    //
    // Responsive state
    //
    public isSmallScreen: boolean = false;
    private readonly SMALL_SCREEN_BREAKPOINT = 768;

    //
    // Add/Edit component reference
    //
    @ViewChild('settingAddEdit') settingAddEdit!: SystemSettingCustomAddEditComponent;
    @ViewChild(SystemSettingCustomTableComponent) settingTable!: SystemSettingCustomTableComponent;


    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private location: Location,
        private authService: AuthService,
        private systemSettingService: SystemSettingService
    ) { }


    ngOnInit(): void {
        this.checkScreenSize();
        this.loadTotalCount();

        //
        // Setup debounced filter
        //
        this.filterTextSubject.pipe(
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


    //
    // Screen size handling
    //
    @HostListener('window:resize')
    onResize(): void {
        this.checkScreenSize();
    }


    private checkScreenSize(): void {
        this.isSmallScreen = window.innerWidth < this.SMALL_SCREEN_BREAKPOINT;
    }


    //
    // Count loading
    //
    private loadTotalCount(): void {
        this.loadingTotalCount = true;

        const params = new SystemSettingQueryParameters();
        params.deleted = false;
        params.active = true;

        this.systemSettingService.GetSystemSettingsRowCount(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (count) => {
                this.totalSettingCount$.next(Number(count));
                this.loadingTotalCount = false;
            },
            error: () => {
                this.totalSettingCount$.next(0);
                this.loadingTotalCount = false;
            }
        });
    }


    private loadFilteredCount(filterText: string): void {
        if (filterText == null || filterText.trim() === '') {
            this.filteredSettingCount$.next(null);
            this.loadingFilteredCount = false;
            return;
        }

        this.loadingFilteredCount = true;

        const params = new SystemSettingQueryParameters();
        params.deleted = false;
        params.active = true;
        params.anyStringContains = filterText;

        this.systemSettingService.GetSystemSettingsRowCount(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (count) => {
                this.filteredSettingCount$.next(Number(count));
                this.loadingFilteredCount = false;
            },
            error: () => {
                this.filteredSettingCount$.next(0);
                this.loadingFilteredCount = false;
            }
        });
    }


    //
    // Filter handling
    //
    public onFilterChange(): void {
        this.filterTextSubject.next(this.filterText);
    }


    //
    // Navigation
    //
    public goBack(): void {
        this.location.back();
    }


    public canGoBack(): boolean {
        return window.history.length > 1;
    }


    //
    // Permissions
    //
    public userIsSystemSettingReader(): boolean {
        return this.systemSettingService.userIsSecuritySystemSettingReader();
    }


    public userIsSystemSettingWriter(): boolean {
        return this.systemSettingService.userIsSecuritySystemSettingWriter();
    }


    //
    // Add Setting
    //
    public addSetting(): void {
        if (this.settingAddEdit) {
            this.settingAddEdit.openModal();
        }
    }


    public onSettingChanged(changedSetting: SystemSettingData): void {
        // Refresh the count and table after setting is added/edited
        this.loadTotalCount();
        if (this.filterText) {
            this.loadFilteredCount(this.filterText);
        }
        // Refresh the table to show updated data
        if (this.settingTable) {
            this.settingTable.refreshData();
        }
    }


    //
    // Edit Setting (called from table component)
    //
    public editSetting(setting: SystemSettingData): void {
        if (this.settingAddEdit) {
            this.settingAddEdit.openModal(setting);
        }
    }
}
