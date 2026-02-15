//
// Tenant Users Tab Component
//
// Displays users associated with this tenant, including inline management
// of isOwner, canRead, canWrite entitlements from SecurityTenantUser.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import {
    SecurityTenantUserService,
    SecurityTenantUserQueryParameters,
    SecurityTenantUserData,
    SecurityTenantUserSubmitData
} from '../../../security-data-services/security-tenant-user.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-tenant-users-tab',
    templateUrl: './tenant-users-tab.component.html',
    styleUrls: ['./tenant-users-tab.component.scss']
})
export class TenantUsersTabComponent implements OnInit, OnChanges, OnDestroy {

    @Input() tenant: SecurityTenantData | null = null;

    //
    // Lifecycle
    //
    private destroy$ = new Subject<void>();

    //
    // Data
    //
    public tenantUsers: SecurityTenantUserData[] = [];
    public isLoading: boolean = true;
    public errorMessage: string | null = null;

    //
    // Track which row is currently saving
    //
    public savingIds: Set<number | bigint> = new Set();


    constructor(
        private router: Router,
        private securityTenantUserService: SecurityTenantUserService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        if (this.tenant) {
            this.loadTenantUsers();
        }
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['tenant'] && this.tenant) {
            this.loadTenantUsers();
        }
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    //
    // Data Loading
    //

    private loadTenantUsers(): void {
        if (!this.tenant) return;

        this.isLoading = true;
        this.errorMessage = null;

        const params = new SecurityTenantUserQueryParameters();
        params.securityTenantId = Number(this.tenant.id);
        params.includeRelations = true;
        params.deleted = false;
        params.active = true;

        this.securityTenantUserService.GetSecurityTenantUserList(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (data) => {
                this.tenantUsers = data || [];
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading tenant users:', err);
                this.errorMessage = 'Failed to load users. Please try again.';
                this.isLoading = false;
            }
        });
    }


    //
    // Entitlement Toggle Handlers
    //

    toggleIsOwner(tenantUser: SecurityTenantUserData, event: Event): void {
        event.stopPropagation();
        if (!this.userCanManage()) return;
        tenantUser.isOwner = !tenantUser.isOwner;
        this.saveEntitlement(tenantUser);
    }


    toggleCanRead(tenantUser: SecurityTenantUserData, event: Event): void {
        event.stopPropagation();
        if (!this.userCanManage()) return;
        tenantUser.canRead = !tenantUser.canRead;
        this.saveEntitlement(tenantUser);
    }


    toggleCanWrite(tenantUser: SecurityTenantUserData, event: Event): void {
        event.stopPropagation();
        if (!this.userCanManage()) return;
        tenantUser.canWrite = !tenantUser.canWrite;
        this.saveEntitlement(tenantUser);
    }


    private saveEntitlement(tenantUser: SecurityTenantUserData): void {
        this.savingIds.add(tenantUser.id);

        const submitData = tenantUser.ConvertToSubmitData();

        this.securityTenantUserService.PutSecurityTenantUser(tenantUser.id, submitData).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (updated) => {
                //
                // Merge the response back but preserve the navigation properties
                //
                tenantUser.isOwner = updated.isOwner;
                tenantUser.canRead = updated.canRead;
                tenantUser.canWrite = updated.canWrite;
                this.savingIds.delete(tenantUser.id);
                this.alertService.showMessage('Success', 'Entitlement updated', MessageSeverity.success);
            },
            error: () => {
                this.savingIds.delete(tenantUser.id);
                this.alertService.showMessage('Error', 'Failed to update entitlement', MessageSeverity.error);
                //
                // Reload to revert optimistic change
                //
                this.loadTenantUsers();
            }
        });
    }


    isSaving(tenantUser: SecurityTenantUserData): boolean {
        return this.savingIds.has(tenantUser.id);
    }


    //
    // Permissions
    //

    userCanManage(): boolean {
        return this.securityTenantUserService.userIsSecuritySecurityTenantUserWriter();
    }


    //
    // Navigation
    //

    navigateToUser(tenantUser: SecurityTenantUserData): void {
        if (tenantUser.securityUser) {
            this.router.navigate(['/user', tenantUser.securityUserId]);
        }
    }


    //
    // Display Helpers
    //

    getStatusBadgeClass(tenantUser: SecurityTenantUserData): string {
        const user = tenantUser.securityUser;
        if (!user) return 'badge-inactive';
        if (user.deleted) return 'badge-deleted';
        return user.active ? 'badge-active' : 'badge-inactive';
    }


    getStatusText(tenantUser: SecurityTenantUserData): string {
        const user = tenantUser.securityUser;
        if (!user) return 'Unknown';
        if (user.deleted) return 'Deleted';
        return user.active ? 'Active' : 'Inactive';
    }


    getInitials(tenantUser: SecurityTenantUserData): string {
        const user = tenantUser.securityUser;
        if (!user) return '?';

        const firstName = user.firstName || '';
        const lastName = user.lastName || '';

        if (firstName && lastName) {
            return (firstName.charAt(0) + lastName.charAt(0)).toUpperCase();
        }
        if (firstName) return firstName.charAt(0).toUpperCase();
        if (user.accountName) return user.accountName.charAt(0).toUpperCase();
        return '?';
    }


    getDisplayName(tenantUser: SecurityTenantUserData): string {
        const user = tenantUser.securityUser;
        if (!user) return 'Unknown';

        const firstName = user.firstName || '';
        const lastName = user.lastName || '';

        if (firstName && lastName) {
            return `${firstName} ${lastName}`;
        }
        return user.accountName || 'Unknown';
    }


    getEmail(tenantUser: SecurityTenantUserData): string {
        return tenantUser.securityUser?.emailAddress || '—';
    }


    getAccountName(tenantUser: SecurityTenantUserData): string {
        return tenantUser.securityUser?.accountName || '—';
    }


    trackByTenantUserId(index: number, tenantUser: SecurityTenantUserData): number | bigint {
        return tenantUser.id;
    }
}
