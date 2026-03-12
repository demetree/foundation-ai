//
// Tenant Custom Listing Component
//
// Premium listing view for managing Security Tenants with search, count badge,
// and add-tenant modal integration.
// Modeled after user-custom-listing and module-custom-listing patterns.
//
// AI-Developed: Added tenant add-edit modal integration with permission gating.
//

import { Component, OnInit, OnDestroy, HostListener, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Subject, BehaviorSubject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '../../../services/auth.service';
import { SecurityTenantService, SecurityTenantQueryParameters, SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { TenantAddEditComponent } from '../tenant-add-edit/tenant-add-edit.component';
import { TenantCustomTableComponent } from '../tenant-custom-table/tenant-custom-table.component';

@Component({
    selector: 'app-tenant-custom-listing',
    templateUrl: './tenant-custom-listing.component.html',
    styleUrls: ['./tenant-custom-listing.component.scss']
})
export class TenantCustomListingComponent implements OnInit, OnDestroy {

    //
    // Lifecycle management
    //
    private destroy$ = new Subject<void>();

    //
    // Loading and counts
    //
    public loadingTotalCount: boolean = true;
    public loadingFilteredCount: boolean = false;
    public totalTenantCount$: BehaviorSubject<number | null> = new BehaviorSubject<number | null>(null);
    public filteredTenantCount$: BehaviorSubject<number | null> = new BehaviorSubject<number | null>(null);

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
    // Child component references
    //
    @ViewChild('tenantAddEdit') tenantAddEdit!: TenantAddEditComponent;
    @ViewChild(TenantCustomTableComponent) tenantTable!: TenantCustomTableComponent;


    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private location: Location,
        private authService: AuthService,
        private securityTenantService: SecurityTenantService
    ) { }


    ngOnInit(): void {
        this.checkScreenSize();
        this.loadTotalCount();
        this.setupFilterDebounce();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    @HostListener('window:resize')
    onResize(): void {
        this.checkScreenSize();
    }


    private checkScreenSize(): void {
        this.isSmallScreen = window.innerWidth < this.SMALL_SCREEN_BREAKPOINT;
    }


    //
    // Data Loading
    //

    private loadTotalCount(): void {
        this.loadingTotalCount = true;

        this.securityTenantService.GetSecurityTenantsRowCount({ active: true, deleted: false })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (count) => {
                    this.totalTenantCount$.next(Number(count));
                    this.filteredTenantCount$.next(Number(count));
                    this.loadingTotalCount = false;
                },
                error: (err) => {
                    console.error('Error loading tenant count:', err);
                    this.loadingTotalCount = false;
                }
            });
    }


    private setupFilterDebounce(): void {
        this.filterTextSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(filterText => {
            this.loadFilteredCount(filterText);
        });
    }


    private loadFilteredCount(filterText: string): void {
        if (!filterText || filterText.trim() === '') {
            // Reset to total count
            const totalCount = this.totalTenantCount$.getValue();
            this.filteredTenantCount$.next(totalCount);
            return;
        }

        this.loadingFilteredCount = true;

        const params: any = {
            active: true,
            deleted: false,
            anyStringContains: filterText.trim()
        };

        this.securityTenantService.GetSecurityTenantsRowCount(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (count) => {
                    this.filteredTenantCount$.next(Number(count));
                    this.loadingFilteredCount = false;
                },
                error: (err) => {
                    console.error('Error loading filtered count:', err);
                    this.loadingFilteredCount = false;
                }
            });
    }


    //
    // Filter handling
    //

    onFilterChange(filterText: string): void {
        this.filterText = filterText;
        this.filterTextSubject.next(filterText);
    }


    clearFilter(): void {
        this.filterText = '';
        this.filterTextSubject.next('');
    }


    //
    // Navigation
    //

    goBack(): void {
        this.location.back();
    }


    canGoBack(): boolean {
        return window.history.length > 1;
    }


    //
    // Add Tenant — opens the add-edit modal for creating a new tenant
    //

    addTenant(): void {
        if (this.tenantAddEdit) {
            this.tenantAddEdit.openForCreate();
        }
    }


    //
    // Called when a tenant is saved (created or updated) from the add-edit modal.
    // Refreshes the count and table data.
    //

    onTenantSaved(savedTenant: SecurityTenantData): void {

        //
        // Refresh the total count
        //
        this.loadTotalCount();

        //
        // Refresh the filtered count if a filter is active
        //
        if (this.filterText) {
            this.filterTextSubject.next(this.filterText);
        }

        //
        // Refresh the table to show the new/updated tenant
        //
        if (this.tenantTable) {
            this.tenantTable.loadTenants();
        }
    }


    //
    // Permissions
    //

    userIsTenantWriter(): boolean {
        return this.securityTenantService.userIsSecuritySecurityTenantWriter();
    }
}
