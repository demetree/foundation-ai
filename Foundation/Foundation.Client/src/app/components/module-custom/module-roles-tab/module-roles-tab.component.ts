//
// Module Roles Tab Component
//
// Displays ModuleSecurityRole entries for the module with user counts.
//

import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, Observable, of, forkJoin } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { ModuleData } from '../../../security-data-services/module.service';
import { ModuleSecurityRoleData } from '../../../security-data-services/module-security-role.service';
import { SecurityUserSecurityRoleService, SecurityUserSecurityRoleData } from '../../../security-data-services/security-user-security-role.service';

// Extended role data with user count
interface RoleWithUsers extends ModuleSecurityRoleData {
    userCount?: number;
    users?: SecurityUserSecurityRoleData[];
    isExpanded?: boolean;
    isLoadingUsers?: boolean;
}

@Component({
    selector: 'app-module-roles-tab',
    templateUrl: './module-roles-tab.component.html',
    styleUrls: ['./module-roles-tab.component.scss']
})
export class ModuleRolesTabComponent implements OnInit, OnDestroy {

    @Input() module: ModuleData | null = null;

    private destroy$ = new Subject<void>();

    public roles: RoleWithUsers[] = [];
    public isLoading: boolean = true;
    public errorMessage: string | null = null;


    constructor(
        private router: Router,
        private securityUserSecurityRoleService: SecurityUserSecurityRoleService
    ) { }


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
                // Add extended properties directly to each role object
                this.roles = roles.map(r => {
                    const extended = r as RoleWithUsers;
                    extended.userCount = undefined;
                    extended.users = undefined;
                    extended.isExpanded = false;
                    extended.isLoadingUsers = false;
                    return extended;
                });
                this.isLoading = false;

                // Load user counts for each role
                this.loadUserCounts();
            })
            .catch(err => {
                console.error('Error loading module security roles:', err);
                this.errorMessage = 'Failed to load security roles.';
                this.isLoading = false;
            });
    }


    private loadUserCounts(): void {
        this.roles.forEach(role => {
            this.securityUserSecurityRoleService.GetSecurityUserSecurityRolesRowCount({
                securityRoleId: role.securityRoleId,
                active: true,
                deleted: false
            }).subscribe({
                next: count => {
                    role.userCount = Number(count);
                },
                error: () => {
                    role.userCount = 0;
                }
            });
        });
    }


    toggleUserList(role: RoleWithUsers): void {
        if (role.isExpanded) {
            role.isExpanded = false;
            return;
        }

        // Expand and load users if not already loaded
        role.isExpanded = true;

        if (!role.users) {
            role.isLoadingUsers = true;
            this.securityUserSecurityRoleService.GetSecurityUserSecurityRoleList({
                securityRoleId: role.securityRoleId,
                active: true,
                deleted: false,
                includeRelations: true
            }).subscribe({
                next: users => {
                    role.users = users;
                    role.isLoadingUsers = false;
                },
                error: () => {
                    role.users = [];
                    role.isLoadingUsers = false;
                }
            });
        }
    }


    navigateToUser(userId: number | bigint): void {
        this.router.navigate(['/user', userId]);
    }


    getInitials(user: SecurityUserSecurityRoleData): string {
        if (user.securityUser) {
            const firstName = user.securityUser.firstName || '';
            const lastName = user.securityUser.lastName || '';
            if (firstName || lastName) {
                return ((firstName[0] || '') + (lastName[0] || '')).toUpperCase() || '?';
            }
            const name = user.securityUser.accountName || '';
            return name.substring(0, 2).toUpperCase() || '?';
        }
        return '?';
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


    trackByUserId(index: number, user: SecurityUserSecurityRoleData): number | bigint {
        return user.id;
    }
}
