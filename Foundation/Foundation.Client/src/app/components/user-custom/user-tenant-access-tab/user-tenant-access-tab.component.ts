//
// User Tenant Access Tab Component
//
// Displays and manages tenant-level entitlements (isOwner, canRead, canWrite)
// from the SecurityTenantUser junction table.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';

import { SecurityUserData } from '../../../security-data-services/security-user.service';
import {
    SecurityTenantUserService,
    SecurityTenantUserQueryParameters,
    SecurityTenantUserData,
    SecurityTenantUserSubmitData
} from '../../../security-data-services/security-tenant-user.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-user-tenant-access-tab',
    templateUrl: './user-tenant-access-tab.component.html',
    styleUrls: ['./user-tenant-access-tab.component.scss']
})
export class UserTenantAccessTabComponent implements OnInit, OnChanges {

    @Input() user: SecurityUserData | null = null;

    //
    // Tenant user data
    //
    public tenantUser: SecurityTenantUserData | null = null;
    public loading: boolean = false;
    public saving: boolean = false;


    constructor(
        private securityTenantUserService: SecurityTenantUserService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        this.loadData();
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['user'] && !changes['user'].firstChange) {
            this.loadData();
        }
    }


    //
    // Data Loading
    //
    private loadData(): void {
        if (!this.user || !this.user.securityTenantId) {
            this.tenantUser = null;
            return;
        }

        this.loading = true;

        const params = new SecurityTenantUserQueryParameters();
        params.securityUserId = Number(this.user.id);
        params.securityTenantId = Number(this.user.securityTenantId);
        params.includeRelations = true;
        params.deleted = false;

        this.securityTenantUserService.GetSecurityTenantUserList(params).subscribe({
            next: (data) => {
                if (data && data.length > 0) {
                    this.tenantUser = data[0];
                } else {
                    this.tenantUser = null;
                }
                this.loading = false;
            },
            error: () => {
                this.tenantUser = null;
                this.loading = false;
                this.alertService.showMessage('Error', 'Failed to load tenant access data', MessageSeverity.error);
            }
        });
    }


    //
    // Toggle handlers — auto-save on change
    //
    public toggleIsOwner(): void {
        if (!this.tenantUser || !this.userCanManage()) return;
        this.tenantUser.isOwner = !this.tenantUser.isOwner;
        this.saveEntitlements();
    }


    public toggleCanRead(): void {
        if (!this.tenantUser || !this.userCanManage()) return;
        this.tenantUser.canRead = !this.tenantUser.canRead;
        this.saveEntitlements();
    }


    public toggleCanWrite(): void {
        if (!this.tenantUser || !this.userCanManage()) return;
        this.tenantUser.canWrite = !this.tenantUser.canWrite;
        this.saveEntitlements();
    }


    //
    // Save
    //
    private saveEntitlements(): void {
        if (!this.tenantUser) return;

        this.saving = true;

        const submitData = new SecurityTenantUserSubmitData();
        submitData.id = this.tenantUser.id;
        submitData.securityTenantId = this.tenantUser.securityTenantId;
        submitData.securityUserId = this.tenantUser.securityUserId;
        submitData.isOwner = this.tenantUser.isOwner;
        submitData.canRead = this.tenantUser.canRead;
        submitData.canWrite = this.tenantUser.canWrite;
        submitData.active = this.tenantUser.active;
        submitData.deleted = this.tenantUser.deleted;

        this.securityTenantUserService.PutSecurityTenantUser(this.tenantUser.id, submitData).subscribe({
            next: (updated) => {
                this.tenantUser = updated;
                this.saving = false;
                this.alertService.showMessage('Success', 'Tenant access updated', MessageSeverity.success);
            },
            error: () => {
                this.saving = false;
                this.alertService.showMessage('Error', 'Failed to update tenant access', MessageSeverity.error);

                //
                // Reload to revert optimistic UI change
                //
                this.loadData();
            }
        });
    }


    //
    // Helpers
    //
    public getTenantName(): string {
        if (this.tenantUser?.securityTenant) {
            return this.tenantUser.securityTenant.name ?? 'Unknown Tenant';
        }
        if (this.user?.securityTenant) {
            return (this.user.securityTenant as any).name ?? 'Unknown Tenant';
        }
        return 'Unknown Tenant';
    }


    //
    // Permissions
    //
    public userCanManage(): boolean {
        return this.securityTenantUserService.userIsSecuritySecurityTenantUserWriter();
    }
}
