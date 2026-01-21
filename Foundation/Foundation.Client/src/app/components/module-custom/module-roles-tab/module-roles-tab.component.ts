//
// Module Roles Tab Component
//
// Displays ModuleSecurityRole entries for the module.
//

import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ModuleData } from '../../../security-data-services/module.service';
import { ModuleSecurityRoleData } from '../../../security-data-services/module-security-role.service';

@Component({
    selector: 'app-module-roles-tab',
    templateUrl: './module-roles-tab.component.html',
    styleUrls: ['./module-roles-tab.component.scss']
})
export class ModuleRolesTabComponent implements OnInit, OnDestroy {

    @Input() module: ModuleData | null = null;

    private destroy$ = new Subject<void>();

    public roles: ModuleSecurityRoleData[] = [];
    public isLoading: boolean = true;
    public errorMessage: string | null = null;


    ngOnInit(): void {
        this.loadRoles();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    private loadRoles(): void {
        if (!this.module) {
            this.isLoading = false;
            return;
        }

        this.isLoading = true;
        this.errorMessage = null;

        this.module.ModuleSecurityRoles
            .then(roles => {
                this.roles = roles;
                this.isLoading = false;
            })
            .catch(err => {
                console.error('Error loading module security roles:', err);
                this.errorMessage = 'Failed to load security roles.';
                this.isLoading = false;
            });
    }


    getStatusBadgeClass(role: ModuleSecurityRoleData): string {
        if (role.deleted) return 'badge-deleted';
        return role.active ? 'badge-active' : 'badge-inactive';
    }


    getStatusText(role: ModuleSecurityRoleData): string {
        if (role.deleted) return 'Deleted';
        return role.active ? 'Active' : 'Inactive';
    }


    trackByRoleId(index: number, role: ModuleSecurityRoleData): number | bigint {
        return role.id;
    }
}
