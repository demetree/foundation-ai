//
// User Group Manager Component
//
// Modal component for managing user's security group memberships.
// Displays all available groups with checkboxes to toggle memberships.
//

import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { SecurityUserData } from '../../../security-data-services/security-user.service';
import { SecurityGroupService, SecurityGroupData, SecurityGroupQueryParameters } from '../../../security-data-services/security-group.service';
import { SecurityUserSecurityGroupService, SecurityUserSecurityGroupData, SecurityUserSecurityGroupQueryParameters, SecurityUserSecurityGroupSubmitData } from '../../../security-data-services/security-user-security-group.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

interface GroupSelectionItem {
    group: SecurityGroupData;
    currentMembership: SecurityUserSecurityGroupData | null;
    selected: boolean;
    originallySelected: boolean;
}

@Component({
    selector: 'app-user-group-manager',
    templateUrl: './user-group-manager.component.html',
    styleUrls: ['./user-group-manager.component.scss']
})
export class UserGroupManagerComponent implements OnInit {

    @Input() user: SecurityUserData | null = null;

    //
    // Group data
    //
    public allGroups: GroupSelectionItem[] = [];
    public filteredGroups: GroupSelectionItem[] = [];
    public searchText: string = '';

    //
    // Loading states
    //
    public loading: boolean = false;
    public saving: boolean = false;

    constructor(
        public activeModal: NgbActiveModal,
        private securityGroupService: SecurityGroupService,
        private securityUserSecurityGroupService: SecurityUserSecurityGroupService,
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
            // Load all available groups
            //
            const groupParams = new SecurityGroupQueryParameters();
            groupParams.active = true;
            groupParams.deleted = false;

            const allGroupsPromise = new Promise<SecurityGroupData[]>((resolve, reject) => {
                this.securityGroupService.GetSecurityGroupList(groupParams).subscribe({
                    next: (data) => resolve(data ?? []),
                    error: (err) => reject(err)
                });
            });

            //
            // Load user's current group memberships
            //
            const membershipParams = new SecurityUserSecurityGroupQueryParameters();
            membershipParams.securityUserId = Number(this.user.id);
            membershipParams.deleted = false;
            membershipParams.includeRelations = true;

            const userGroupsPromise = new Promise<SecurityUserSecurityGroupData[]>((resolve, reject) => {
                this.securityUserSecurityGroupService.GetSecurityUserSecurityGroupList(membershipParams).subscribe({
                    next: (data) => resolve(data ?? []),
                    error: (err) => reject(err)
                });
            });

            //
            // Wait for both to complete
            //
            const [groups, userMemberships] = await Promise.all([allGroupsPromise, userGroupsPromise]);

            //
            // Build selection items
            //
            this.allGroups = groups.map(group => {
                const membership = userMemberships.find(um => um.securityGroupId === group.id);
                const isSelected = membership != null && membership.active === true;
                return {
                    group: group,
                    currentMembership: membership ?? null,
                    selected: isSelected,
                    originallySelected: isSelected
                };
            });

            this.applyFilter();

        } catch (error) {
            console.error('Error loading groups:', error);
            this.alertService.showMessage('Error', 'Failed to load groups', MessageSeverity.error);
        } finally {
            this.loading = false;
        }
    }


    applyFilter(): void {
        if (!this.searchText.trim()) {
            this.filteredGroups = [...this.allGroups];
        } else {
            const search = this.searchText.toLowerCase();
            this.filteredGroups = this.allGroups.filter(item =>
                item.group.name?.toLowerCase().includes(search) ||
                item.group.description?.toLowerCase().includes(search)
            );
        }
    }


    hasChanges(): boolean {
        return this.allGroups.some(item => item.selected !== item.originallySelected);
    }


    getChangeCount(): number {
        return this.allGroups.filter(item => item.selected !== item.originallySelected).length;
    }


    getSelectedCount(): number {
        return this.allGroups.filter(item => item.selected).length;
    }


    async save(): Promise<void> {
        if (!this.user || !this.hasChanges()) {
            this.activeModal.close(false);
            return;
        }

        this.saving = true;

        try {
            const promises: Promise<any>[] = [];

            for (const item of this.allGroups) {
                if (item.selected !== item.originallySelected) {
                    if (item.selected && !item.currentMembership) {
                        //
                        // Create new membership
                        //
                        const newMembership = new SecurityUserSecurityGroupSubmitData();
                        newMembership.securityUserId = Number(this.user.id);
                        newMembership.securityGroupId = Number(item.group.id);
                        newMembership.active = true;
                        newMembership.deleted = false;

                        promises.push(new Promise((resolve, reject) => {
                            this.securityUserSecurityGroupService.PostSecurityUserSecurityGroup(newMembership).subscribe({
                                next: () => resolve(true),
                                error: (err) => reject(err)
                            });
                        }));

                    } else if (item.selected && item.currentMembership && item.currentMembership.active !== true) {
                        //
                        // Re-activate existing membership
                        //
                        const updateData = new SecurityUserSecurityGroupSubmitData();
                        updateData.id = Number(item.currentMembership.id);
                        updateData.securityUserId = Number(this.user.id);
                        updateData.securityGroupId = Number(item.group.id);
                        updateData.active = true;
                        updateData.deleted = false;

                        promises.push(new Promise((resolve, reject) => {
                            this.securityUserSecurityGroupService.PutSecurityUserSecurityGroup(updateData.id, updateData).subscribe({
                                next: () => resolve(true),
                                error: (err) => reject(err)
                            });
                        }));

                    } else if (!item.selected && item.currentMembership) {
                        //
                        // Deactivate existing membership
                        //
                        const updateData = new SecurityUserSecurityGroupSubmitData();
                        updateData.id = Number(item.currentMembership.id);
                        updateData.securityUserId = Number(this.user.id);
                        updateData.securityGroupId = Number(item.group.id);
                        updateData.active = false;
                        updateData.deleted = false;

                        promises.push(new Promise((resolve, reject) => {
                            this.securityUserSecurityGroupService.PutSecurityUserSecurityGroup(updateData.id, updateData).subscribe({
                                next: () => resolve(true),
                                error: (err) => reject(err)
                            });
                        }));
                    }
                }
            }

            await Promise.all(promises);

            this.alertService.showMessage('Success', 'Group memberships updated successfully', MessageSeverity.success);
            this.activeModal.close(true);

        } catch (error) {
            console.error('Error saving group memberships:', error);
            this.alertService.showMessage('Error', 'Failed to save group memberships', MessageSeverity.error);
        } finally {
            this.saving = false;
        }
    }


    cancel(): void {
        this.activeModal.dismiss();
    }
}
