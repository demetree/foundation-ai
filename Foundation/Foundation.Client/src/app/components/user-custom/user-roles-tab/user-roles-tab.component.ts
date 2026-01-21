//
// User Roles Tab Component
//
// Displays and manages user's security roles and group memberships.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { SecurityUserData } from '../../../security-data-services/security-user.service';
import { SecurityUserSecurityRoleService, SecurityUserSecurityRoleQueryParameters, SecurityUserSecurityRoleData } from '../../../security-data-services/security-user-security-role.service';
import { SecurityUserSecurityGroupService, SecurityUserSecurityGroupQueryParameters, SecurityUserSecurityGroupData } from '../../../security-data-services/security-user-security-group.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-user-roles-tab',
    templateUrl: './user-roles-tab.component.html',
    styleUrls: ['./user-roles-tab.component.scss']
})
export class UserRolesTabComponent implements OnInit, OnChanges {

    @Input() user: SecurityUserData | null = null;

    //
    // Roles data
    //
    public roles: SecurityUserSecurityRoleData[] = [];
    public loadingRoles: boolean = false;

    //
    // Groups data
    //
    public groups: SecurityUserSecurityGroupData[] = [];
    public loadingGroups: boolean = false;


    constructor(
        private securityUserSecurityRoleService: SecurityUserSecurityRoleService,
        private securityUserSecurityGroupService: SecurityUserSecurityGroupService,
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


    private loadData(): void {
        if (!this.user) return;
        this.loadRoles();
        this.loadGroups();
    }


    private loadRoles(): void {
        if (!this.user) return;

        this.loadingRoles = true;

        const params = new SecurityUserSecurityRoleQueryParameters();
        params.securityUserId = Number(this.user.id);
        params.includeRelations = true;
        params.deleted = false;

        this.securityUserSecurityRoleService.GetSecurityUserSecurityRoleList(params).subscribe({
            next: (data) => {
                this.roles = data ?? [];
                this.loadingRoles = false;
            },
            error: () => {
                this.roles = [];
                this.loadingRoles = false;
                this.alertService.showMessage('Error', 'Failed to load roles', MessageSeverity.error);
            }
        });
    }


    private loadGroups(): void {
        if (!this.user) return;

        this.loadingGroups = true;

        const params = new SecurityUserSecurityGroupQueryParameters();
        params.securityUserId = Number(this.user.id);
        params.includeRelations = true;
        params.deleted = false;

        this.securityUserSecurityGroupService.GetSecurityUserSecurityGroupList(params).subscribe({
            next: (data) => {
                this.groups = data ?? [];
                this.loadingGroups = false;
            },
            error: () => {
                this.groups = [];
                this.loadingGroups = false;
                this.alertService.showMessage('Error', 'Failed to load groups', MessageSeverity.error);
            }
        });
    }


    //
    // Permissions
    //
    public userCanManageRoles(): boolean {
        return this.securityUserSecurityRoleService.userIsSecuritySecurityUserSecurityRoleWriter();
    }


    public userCanManageGroups(): boolean {
        return this.securityUserSecurityGroupService.userIsSecuritySecurityUserSecurityGroupWriter();
    }
}
