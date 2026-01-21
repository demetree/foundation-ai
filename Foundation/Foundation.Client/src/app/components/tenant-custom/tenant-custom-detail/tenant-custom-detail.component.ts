//
// Tenant Custom Detail Component
//
// Premium detail view for managing a single Security Tenant with tabbed interface.
// Modeled after module-custom-detail pattern.
//

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Subject, BehaviorSubject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { SecurityTenantService, SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { AlertService } from '../../../services/alert.service';

@Component({
    selector: 'app-tenant-custom-detail',
    templateUrl: './tenant-custom-detail.component.html',
    styleUrls: ['./tenant-custom-detail.component.scss']
})
export class TenantCustomDetailComponent implements OnInit, OnDestroy {

    //
    // Lifecycle
    //
    private destroy$ = new Subject<void>();

    //
    // Data
    //
    public tenantData: SecurityTenantData | null = null;
    public isLoading$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);
    public notFound: boolean = false;

    //
    // Tab state
    //
    public activeTab: number = 1;

    //
    // Quick action states
    //
    public isToggling: boolean = false;


    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private location: Location,
        private securityTenantService: SecurityTenantService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        this.route.params
            .pipe(takeUntil(this.destroy$))
            .subscribe(params => {
                const id = params['id'];
                if (id) {
                    this.loadTenant(Number(id));
                }
            });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    //
    // Data Loading
    //

    private loadTenant(id: number): void {
        this.isLoading$.next(true);
        this.notFound = false;

        this.securityTenantService.GetSecurityTenant(id, true)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (tenant) => {
                    this.tenantData = tenant;
                    this.isLoading$.next(false);
                },
                error: (err) => {
                    console.error('Error loading tenant:', err);
                    this.notFound = true;
                    this.isLoading$.next(false);
                }
            });
    }


    //
    // Display Helpers
    //

    getStatusBadgeClass(): string {
        if (!this.tenantData) return '';
        if (this.tenantData.deleted) return 'badge-deleted';
        return this.tenantData.active ? 'badge-active' : 'badge-inactive';
    }


    getStatusText(): string {
        if (!this.tenantData) return '';
        if (this.tenantData.deleted) return 'Deleted';
        return this.tenantData.active ? 'Active' : 'Inactive';
    }


    //
    // Quick Actions
    //

    toggleActive(): void {
        if (!this.tenantData || this.isToggling) return;

        this.isToggling = true;
        const newActiveState = !this.tenantData.active;

        const submitData = this.tenantData.ConvertToSubmitData();
        submitData.active = newActiveState;

        this.securityTenantService.PutSecurityTenant(Number(this.tenantData.id), submitData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (updated) => {
                    this.tenantData = updated;
                    this.isToggling = false;
                    this.alertService.showSuccessMessage(
                        'Tenant Updated',
                        `Tenant is now ${newActiveState ? 'active' : 'inactive'}.`
                    );
                },
                error: (err) => {
                    console.error('Error toggling tenant active state:', err);
                    this.isToggling = false;
                    this.alertService.showErrorMessage(
                        'Update Failed',
                        'Failed to update tenant status. Please try again.'
                    );
                }
            });
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


    navigateToTenants(): void {
        this.router.navigate(['/tenants']);
    }


    //
    // Permission Helpers
    //

    canEdit(): boolean {
        return this.securityTenantService.userIsSecuritySecurityTenantWriter();
    }
}
