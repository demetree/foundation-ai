//
// User Role Manager Component
//
// Modal component for managing user's security role assignments.
// Displays all available roles with checkboxes to toggle assignments.
//

import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { SecurityUserData } from '../../../security-data-services/security-user.service';
import { SecurityRoleService, SecurityRoleData, SecurityRoleQueryParameters } from '../../../security-data-services/security-role.service';
import { SecurityUserSecurityRoleService, SecurityUserSecurityRoleData, SecurityUserSecurityRoleQueryParameters, SecurityUserSecurityRoleSubmitData } from '../../../security-data-services/security-user-security-role.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

interface RoleSelectionItem {
    role: SecurityRoleData;
    currentAssignment: SecurityUserSecurityRoleData | null;
    selected: boolean;
    originallySelected: boolean;
}

@Component({
    selector: 'app-user-role-manager',
    templateUrl: './user-role-manager.component.html',
    styleUrls: ['./user-role-manager.component.scss']
})
export class UserRoleManagerComponent implements OnInit {

    @Input() user: SecurityUserData | null = null;

    //
    // Role data
    //
    public allRoles: RoleSelectionItem[] = [];
    public filteredRoles: RoleSelectionItem[] = [];
    public searchText: string = '';

    //
    // Loading states
    //
    public loading: boolean = false;
    public saving: boolean = false;

    constructor(
        public activeModal: NgbActiveModal,
        private securityRoleService: SecurityRoleService,
        private securityUserSecurityRoleService: SecurityUserSecurityRoleService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        this.loadData();
    }


    private async loadData(): Promise<void> {
        if (!this.user) return;

        this.loading = true;

        try {
            //
            // Load all available roles
            //
            const roleParams = new SecurityRoleQueryParameters();
            roleParams.active = true;
            roleParams.deleted = false;

            const allRolesPromise = new Promise<SecurityRoleData[]>((resolve, reject) => {
                this.securityRoleService.GetSecurityRoleList(roleParams).subscribe({
                    next: (data) => resolve(data ?? []),
                    error: (err) => reject(err)
                });
            });

            //
            // Load user's current role assignments
            //
            const assignmentParams = new SecurityUserSecurityRoleQueryParameters();
            assignmentParams.securityUserId = Number(this.user.id);
            assignmentParams.deleted = false;
            assignmentParams.includeRelations = true;

            const userRolesPromise = new Promise<SecurityUserSecurityRoleData[]>((resolve, reject) => {
                this.securityUserSecurityRoleService.GetSecurityUserSecurityRoleList(assignmentParams).subscribe({
                    next: (data) => resolve(data ?? []),
                    error: (err) => reject(err)
                });
            });

            //
            // Wait for both to complete
            //
            const [roles, userAssignments] = await Promise.all([allRolesPromise, userRolesPromise]);

            //
            // Build selection items
            //
            this.allRoles = roles.map(role => {
                const assignment = userAssignments.find(ua => ua.securityRoleId === role.id);
                const isSelected = assignment != null && assignment.active === true;
                return {
                    role: role,
                    currentAssignment: assignment ?? null,
                    selected: isSelected,
                    originallySelected: isSelected
                };
            });

            this.applyFilter();

        } catch (error) {
            console.error('Error loading roles:', error);
            this.alertService.showMessage('Error', 'Failed to load roles', MessageSeverity.error);
        } finally {
            this.loading = false;
        }
    }


    applyFilter(): void {
        if (!this.searchText.trim()) {
            this.filteredRoles = [...this.allRoles];
        } else {
            const search = this.searchText.toLowerCase();
            this.filteredRoles = this.allRoles.filter(item =>
                item.role.name?.toLowerCase().includes(search) ||
                item.role.description?.toLowerCase().includes(search)
            );
        }
    }


    hasChanges(): boolean {
        return this.allRoles.some(item => item.selected !== item.originallySelected);
    }


    getChangeCount(): number {
        return this.allRoles.filter(item => item.selected !== item.originallySelected).length;
    }


    getSelectedCount(): number {
        return this.allRoles.filter(item => item.selected).length;
    }


    async save(): Promise<void> {
        if (!this.user || !this.hasChanges()) {
            this.activeModal.close(false);
            return;
        }

        this.saving = true;

        try {
            const promises: Promise<any>[] = [];

            for (const item of this.allRoles) {
                if (item.selected !== item.originallySelected) {
                    if (item.selected && !item.currentAssignment) {
                        //
                        // Create new assignment
                        //
                        const newAssignment = new SecurityUserSecurityRoleSubmitData();
                        newAssignment.securityUserId = Number(this.user.id);
                        newAssignment.securityRoleId = Number(item.role.id);
                        newAssignment.active = true;
                        newAssignment.deleted = false;

                        promises.push(new Promise((resolve, reject) => {
                            this.securityUserSecurityRoleService.PostSecurityUserSecurityRole(newAssignment).subscribe({
                                next: () => resolve(true),
                                error: (err) => reject(err)
                            });
                        }));

                    } else if (item.selected && item.currentAssignment && item.currentAssignment.active !== true) {
                        //
                        // Re-activate existing assignment
                        //
                        const updateData = new SecurityUserSecurityRoleSubmitData();
                        updateData.id = Number(item.currentAssignment.id);
                        updateData.securityUserId = Number(this.user.id);
                        updateData.securityRoleId = Number(item.role.id);
                        updateData.active = true;
                        updateData.deleted = false;

                        promises.push(new Promise((resolve, reject) => {
                            this.securityUserSecurityRoleService.PutSecurityUserSecurityRole(updateData.id, updateData).subscribe({
                                next: () => resolve(true),
                                error: (err) => reject(err)
                            });
                        }));

                    } else if (!item.selected && item.currentAssignment) {
                        //
                        // Deactivate existing assignment
                        //
                        const updateData = new SecurityUserSecurityRoleSubmitData();
                        updateData.id = Number(item.currentAssignment.id);
                        updateData.securityUserId = Number(this.user.id);
                        updateData.securityRoleId = Number(item.role.id);
                        updateData.active = false;
                        updateData.deleted = false;

                        promises.push(new Promise((resolve, reject) => {
                            this.securityUserSecurityRoleService.PutSecurityUserSecurityRole(updateData.id, updateData).subscribe({
                                next: () => resolve(true),
                                error: (err) => reject(err)
                            });
                        }));
                    }
                }
            }

            await Promise.all(promises);

            this.alertService.showMessage('Success', 'Role assignments updated successfully', MessageSeverity.success);
            this.activeModal.close(true);

        } catch (error) {
            console.error('Error saving role assignments:', error);
            this.alertService.showMessage('Error', 'Failed to save role assignments', MessageSeverity.error);
        } finally {
            this.saving = false;
        }
    }


    cancel(): void {
        this.activeModal.dismiss();
    }
}
