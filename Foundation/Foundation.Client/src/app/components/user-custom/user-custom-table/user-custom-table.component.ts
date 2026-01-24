//
// User Custom Table Component
//
// Smart table for displaying Security Users with status badges, 2FA indicators,
// and quick actions (toggle active, unlock). Based on Foundation table patterns.
//

import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';

import { SecurityUserService, SecurityUserData, SecurityUserQueryParameters, SecurityUserSubmitData } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { InputDialogService } from '../../../services/input-dialog.service';
import { AdminUserActionsService } from '../admin-user-actions.service';

//
// Status badge types for visual display
//
export type UserStatusType = 'active' | 'cooling-down' | 'locked' | 'inactive' | 'no-login';

@Component({
    selector: 'app-user-custom-table',
    templateUrl: './user-custom-table.component.html',
    styleUrls: ['./user-custom-table.component.scss']
})
export class UserCustomTableComponent implements OnInit, AfterViewInit, OnChanges {

    //
    // Inputs
    //
    @Input() filterText: string = '';
    @Input() isSmallScreen: boolean = false;

    //
    // Outputs
    //
    @Output() editRequested = new EventEmitter<SecurityUserData>();

    //
    // Data state
    //
    public users: SecurityUserData[] = [];
    public filteredUsers: SecurityUserData[] = [];
    public loading: boolean = true;
    public errorState: boolean = false;

    //
    // Sorting
    //
    public sortColumn: string = 'lastName';
    public sortDirection: 'asc' | 'desc' = 'asc';

    //
    // Action tracking
    //
    public actionInProgress: { [key: number]: boolean } = {};


    constructor(
        private router: Router,
        private securityUserService: SecurityUserService,
        private authService: AuthService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService,
        private inputDialogService: InputDialogService,
        private adminUserActionsService: AdminUserActionsService
    ) { }


    ngOnInit(): void {
        // Initial load happens in ngAfterViewInit
    }


    ngAfterViewInit(): void {
        this.loadData();
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['filterText'] && !changes['filterText'].firstChange) {
            this.applyFiltersAndSort();
        }
    }


    //
    // Data Loading
    //
    private loadData(): void {
        this.loading = true;
        this.errorState = false;

        const params = new SecurityUserQueryParameters();
        params.deleted = false;
        params.includeRelations = true;

        this.securityUserService.GetSecurityUserList(params).subscribe({
            next: (users) => {
                this.users = users ?? [];
                this.applyFiltersAndSort();
                this.loading = false;
            },
            error: (err) => {
                console.error('Failed to load users', err);
                this.users = [];
                this.filteredUsers = [];
                this.loading = false;
                this.errorState = true;
                this.alertService.showMessage('Error', 'Failed to load users', MessageSeverity.error);
            }
        });
    }


    //
    // Filtering and Sorting
    //
    private applyFiltersAndSort(): void {
        let result = [...this.users];

        //
        // Apply text filter
        //
        if (this.filterText && this.filterText.trim() !== '') {
            const searchLower = this.filterText.toLowerCase();
            result = result.filter(user => {
                const searchableFields = [
                    user.firstName,
                    user.lastName,
                    user.accountName,
                    user.emailAddress,
                    user.description
                ];
                return searchableFields.some(field =>
                    field && field.toLowerCase().includes(searchLower)
                );
            });
        }

        //
        // Apply sort
        //
        result.sort((a, b) => {
            let aVal = this.getNestedValue(a, this.sortColumn);
            let bVal = this.getNestedValue(b, this.sortColumn);

            if (aVal == null) aVal = '';
            if (bVal == null) bVal = '';

            if (typeof aVal === 'string') aVal = aVal.toLowerCase();
            if (typeof bVal === 'string') bVal = bVal.toLowerCase();

            let comparison = 0;
            if (aVal < bVal) comparison = -1;
            if (aVal > bVal) comparison = 1;

            return this.sortDirection === 'asc' ? comparison : -comparison;
        });

        this.filteredUsers = result;
    }


    private getNestedValue(obj: any, path: string): any {
        return path.split('.').reduce((o, p) => o && o[p], obj);
    }


    public sortBy(column: string): void {
        if (this.sortColumn === column) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = column;
            this.sortDirection = 'asc';
        }
        this.applyFiltersAndSort();
    }


    //
    // Status Calculation
    //
    public getUserStatus(user: SecurityUserData): UserStatusType {
        //
        // Check if user can't login at all
        //
        if (user.canLogin !== true) {
            return 'no-login';
        }

        //
        // Check if user is inactive
        //
        if (user.active !== true) {
            return 'inactive';
        }

        //
        // Check lockout based on failed login count
        //
        const failedCount = user.failedLoginCount ?? 0;

        if (failedCount >= 10) {
            return 'locked';
        }

        if (failedCount >= 4) {
            return 'cooling-down';
        }

        return 'active';
    }


    public getStatusLabel(status: UserStatusType): string {
        switch (status) {
            case 'active': return 'Active';
            case 'cooling-down': return 'Cooling Down';
            case 'locked': return 'Locked';
            case 'inactive': return 'Inactive';
            case 'no-login': return 'No Login';
            default: return 'Unknown';
        }
    }


    public getStatusClass(status: UserStatusType): string {
        switch (status) {
            case 'active': return 'bg-success';
            case 'cooling-down': return 'bg-warning text-dark';
            case 'locked': return 'bg-danger';
            case 'inactive': return 'bg-secondary';
            case 'no-login': return 'bg-dark';
            default: return 'bg-secondary';
        }
    }


    //
    // 2FA Status
    //
    public has2FA(user: SecurityUserData): boolean {
        return user.twoFactorSendByEmail === true || user.twoFactorSendBySMS === true;
    }


    public get2FATooltip(user: SecurityUserData): string {
        const methods: string[] = [];
        if (user.twoFactorSendByEmail === true) methods.push('Email');
        if (user.twoFactorSendBySMS === true) methods.push('SMS');

        if (methods.length === 0) {
            return 'No 2FA configured';
        }
        return '2FA via ' + methods.join(' & ');
    }


    //
    // Display helpers
    //
    public getUserDisplayName(user: SecurityUserData): string {
        const parts = [user.firstName, user.lastName].filter(p => p);
        return parts.length > 0 ? parts.join(' ') : user.accountName;
    }


    public getUserInitials(user: SecurityUserData): string {
        const first = user.firstName?.charAt(0) ?? '';
        const last = user.lastName?.charAt(0) ?? '';

        if (first || last) {
            return (first + last).toUpperCase();
        }
        return user.accountName?.charAt(0)?.toUpperCase() ?? '?';
    }


    public formatRelativeTime(dateString: string | null): string {
        if (dateString == null) {
            return 'Never';
        }

        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;

        return date.toLocaleDateString();
    }


    //
    // Quick Actions
    //
    public toggleActive(user: SecurityUserData, event: Event): void {
        event.stopPropagation();

        const userId = Number(user.id);
        if (this.actionInProgress[userId]) return;

        this.actionInProgress[userId] = true;

        const submitData = new SecurityUserSubmitData();
        submitData.id = user.id;
        submitData.accountName = user.accountName;
        submitData.activeDirectoryAccount = user.activeDirectoryAccount;
        submitData.canLogin = user.canLogin;
        submitData.mustChangePassword = user.mustChangePassword;
        submitData.firstName = user.firstName;
        submitData.lastName = user.lastName;
        submitData.emailAddress = user.emailAddress;
        submitData.readPermissionLevel = user.readPermissionLevel;
        submitData.writePermissionLevel = user.writePermissionLevel;
        submitData.active = !user.active;  // Toggle
        submitData.deleted = user.deleted;

        this.securityUserService.PutSecurityUser(user.id, submitData).subscribe({
            next: () => {
                user.active = !user.active;
                this.actionInProgress[userId] = false;
                this.alertService.showMessage('Success', `User ${user.active ? 'activated' : 'deactivated'}`, MessageSeverity.success);
            },
            error: (err) => {
                this.actionInProgress[userId] = false;
                this.alertService.showMessage('Error', 'Failed to update user', MessageSeverity.error);
            }
        });
    }


    public unlockUser(user: SecurityUserData, event: Event): void {
        event.stopPropagation();

        const userId = Number(user.id);
        if (this.actionInProgress[userId]) return;

        this.actionInProgress[userId] = true;

        const submitData = new SecurityUserSubmitData();
        submitData.id = user.id;
        submitData.accountName = user.accountName;
        submitData.activeDirectoryAccount = user.activeDirectoryAccount;
        submitData.canLogin = user.canLogin;
        submitData.mustChangePassword = user.mustChangePassword;
        submitData.firstName = user.firstName;
        submitData.lastName = user.lastName;
        submitData.emailAddress = user.emailAddress;
        submitData.readPermissionLevel = user.readPermissionLevel;
        submitData.writePermissionLevel = user.writePermissionLevel;
        submitData.active = true;  // Reactivate if needed
        submitData.deleted = user.deleted;
        submitData.failedLoginCount = 0;  // Reset lockout

        this.securityUserService.PutSecurityUser(user.id, submitData).subscribe({
            next: () => {
                user.failedLoginCount = 0;
                user.active = true;
                this.actionInProgress[userId] = false;
                this.alertService.showMessage('Success', 'User unlocked successfully', MessageSeverity.success);
            },
            error: (err) => {
                this.actionInProgress[userId] = false;
                this.alertService.showMessage('Error', 'Failed to unlock user', MessageSeverity.error);
            }
        });
    }


    public canUnlock(user: SecurityUserData): boolean {
        const failedCount = user.failedLoginCount ?? 0;
        return failedCount >= 4 || user.active !== true;
    }


    public isActionInProgress(user: SecurityUserData): boolean {
        return this.actionInProgress[Number(user.id)] === true;
    }


    //
    // Navigation
    //
    public navigateToDetail(user: SecurityUserData): void {
        this.router.navigate(['/user', user.id]);
    }

    //
    // Edit request (emits to parent for modal handling)
    //
    public editUser(user: SecurityUserData, event: Event): void {
        event.stopPropagation();
        this.editRequested.emit(user);
    }

    //
    // Public refresh method for parent to call after edits
    //
    public refreshData(): void {
        this.loadData();
    }


    //
    // AI-Generated: Admin Actions
    //

    public async sendPasswordReset(user: SecurityUserData, event: Event): Promise<void> {
        event.stopPropagation();

        const confirmed = await this.confirmationService.confirm(
            'Send Password Reset',
            `Send a password reset email to ${user.emailAddress ?? user.accountName}?`
        );

        if (confirmed !== true) {
            return;
        }

        const userId = Number(user.id);

        if (this.actionInProgress[userId] === true) {
            return;
        }

        this.actionInProgress[userId] = true;

        this.adminUserActionsService.sendPasswordReset(user.id).subscribe({
            next: () => {
                this.actionInProgress[userId] = false;
                this.alertService.showMessage('Success', 'Password reset email sent', MessageSeverity.success);
            },
            error: (err) => {
                this.actionInProgress[userId] = false;
                this.alertService.showMessage('Error', 'Failed to send password reset email', MessageSeverity.error);
            }
        });
    }


    public async setTemporaryPassword(user: SecurityUserData, event: Event): Promise<void> {
        event.stopPropagation();

        //
        // Prompt for new password using styled dialog
        //
        const password = await this.inputDialogService.promptPassword(
            'Set Temporary Password',
            `Enter a temporary password for ${user.accountName}. The user will be required to change it on next login.`
        );

        if (password == null || password.trim() === '') {
            return;
        }

        const userId = Number(user.id);

        if (this.actionInProgress[userId] === true) {
            return;
        }

        this.actionInProgress[userId] = true;

        this.adminUserActionsService.setTemporaryPassword(user.id, password).subscribe({
            next: () => {
                this.actionInProgress[userId] = false;
                this.alertService.showMessage('Success', 'Temporary password set. User must change on next login.', MessageSeverity.success);
            },
            error: (err) => {
                this.actionInProgress[userId] = false;

                let errorMsg = 'Failed to set password';

                if (err.error && typeof err.error === 'string') {
                    errorMsg = err.error;
                }

                this.alertService.showMessage('Error', errorMsg, MessageSeverity.error);
            }
        });
    }


    public async lockAccount(user: SecurityUserData, event: Event): Promise<void> {
        event.stopPropagation();

        const confirmed = await this.confirmationService.confirm(
            'Lock Account',
            `Are you sure you want to lock the account for ${user.accountName}? They will not be able to log in.`
        );

        if (confirmed !== true) {
            return;
        }

        const userId = Number(user.id);

        if (this.actionInProgress[userId] === true) {
            return;
        }

        this.actionInProgress[userId] = true;

        this.adminUserActionsService.lockAccount(user.id).subscribe({
            next: () => {
                user.active = false;
                this.actionInProgress[userId] = false;
                this.alertService.showMessage('Success', 'Account locked', MessageSeverity.success);
            },
            error: (err) => {
                this.actionInProgress[userId] = false;
                this.alertService.showMessage('Error', 'Failed to lock account', MessageSeverity.error);
            }
        });
    }


    //
    // Permissions
    //
    public userIsSecurityUserWriter(): boolean {
        return this.securityUserService.userIsSecuritySecurityUserWriter();
    }


    //
    // TrackBy
    //
    public trackByUserId(index: number, user: SecurityUserData): number {
        return Number(user.id);
    }
}
