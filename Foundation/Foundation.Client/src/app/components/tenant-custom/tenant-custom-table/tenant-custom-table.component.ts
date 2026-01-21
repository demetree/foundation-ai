//
// Tenant Custom Table Component
//
// Premium table for displaying security tenants with user/org counts and status badges.
// Modeled after module-custom-table pattern.
//

import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { SecurityTenantService, SecurityTenantData, SecurityTenantQueryParameters } from '../../../security-data-services/security-tenant.service';

@Component({
    selector: 'app-tenant-custom-table',
    templateUrl: './tenant-custom-table.component.html',
    styleUrls: ['./tenant-custom-table.component.scss']
})
export class TenantCustomTableComponent implements OnInit, OnDestroy, OnChanges {

    //
    // Inputs
    //
    @Input() filterText: string = '';

    //
    // Outputs
    //
    @Output() countChange = new EventEmitter<number>();

    //
    // Lifecycle
    //
    private destroy$ = new Subject<void>();
    private filterSubject = new Subject<string>();

    //
    // Data
    //
    public tenants: SecurityTenantData[] = [];
    public isLoading: boolean = true;
    public errorMessage: string | null = null;

    //
    // Pagination
    //
    public currentPage: number = 1;
    public pageSize: number = 20;
    public totalCount: number = 0;

    // Math reference for template
    public Math = Math;


    constructor(
        private router: Router,
        private securityTenantService: SecurityTenantService
    ) { }


    ngOnInit(): void {
        this.setupFilterDebounce();
        this.loadTenants();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['filterText'] && !changes['filterText'].firstChange) {
            this.filterSubject.next(this.filterText);
        }
    }


    private setupFilterDebounce(): void {
        this.filterSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(() => {
            this.currentPage = 1;
            this.loadTenants();
        });
    }


    //
    // Data Loading
    //

    loadTenants(): void {
        this.isLoading = true;
        this.errorMessage = null;

        const params: any = {
            active: true,
            deleted: false,
            pageSize: this.pageSize,
            pageNumber: this.currentPage,
            includeRelations: true
        };

        if (this.filterText && this.filterText.trim()) {
            params.anyStringContains = this.filterText.trim();
        }

        this.securityTenantService.GetSecurityTenantList(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (tenants) => {
                    this.tenants = tenants;
                    this.isLoading = false;
                    this.loadTotalCount();
                },
                error: (err) => {
                    console.error('Error loading tenants:', err);
                    this.errorMessage = 'Failed to load tenants. Please try again.';
                    this.isLoading = false;
                }
            });
    }


    private loadTotalCount(): void {
        const params: any = {
            active: true,
            deleted: false
        };

        if (this.filterText && this.filterText.trim()) {
            params.anyStringContains = this.filterText.trim();
        }

        this.securityTenantService.GetSecurityTenantsRowCount(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (count) => {
                    this.totalCount = Number(count);
                    this.countChange.emit(this.totalCount);
                },
                error: (err) => {
                    console.error('Error loading tenant count:', err);
                }
            });
    }


    //
    // Pagination
    //

    onPageChange(page: number): void {
        this.currentPage = page;
        this.loadTenants();
    }


    get totalPages(): number {
        return Math.ceil(this.totalCount / this.pageSize);
    }


    //
    // Navigation
    //

    navigateToDetail(tenant: SecurityTenantData): void {
        this.router.navigate(['/tenant', tenant.id]);
    }


    //
    // Display helpers
    //

    getStatusBadgeClass(tenant: SecurityTenantData): string {
        if (tenant.deleted) {
            return 'badge-deleted';
        }
        return tenant.active ? 'badge-active' : 'badge-inactive';
    }


    getStatusText(tenant: SecurityTenantData): string {
        if (tenant.deleted) {
            return 'Deleted';
        }
        return tenant.active ? 'Active' : 'Inactive';
    }


    trackByTenantId(index: number, tenant: SecurityTenantData): number | bigint {
        return tenant.id;
    }
}
