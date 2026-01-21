//
// Tenant Custom Listing Component
//
// Premium listing view for managing Security Tenants with search and count badge.
// Modeled after module-custom-listing pattern.
//

import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Subject, BehaviorSubject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '../../../services/auth.service';
import { SecurityTenantService, SecurityTenantQueryParameters } from '../../../security-data-services/security-tenant.service';

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

    navigateToAddTenant(): void {
        this.router.navigate(['/tenant/add']);
    }


    goBack(): void {
        this.location.back();
    }


    canGoBack(): boolean {
        return window.history.length > 1;
    }
}
