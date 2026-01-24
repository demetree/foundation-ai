//
// User Custom Detail Component
//
// Premium detail view for SecurityUser with tabbed layout (Overview, Roles, Activity).
// Modeled after Scheduler.Client contact-custom-detail pattern.
//

import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { Subject, BehaviorSubject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { SecurityUserService, SecurityUserData, SecurityUserQueryParameters, SecurityUserSubmitData } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NavigationService } from '../../../utility-services/navigation.service';
import { UserCustomAddEditComponent } from '../user-custom-add-edit/user-custom-add-edit.component';
import { UserImageUploadComponent } from '../user-image-upload/user-image-upload.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

export type UserStatusType = 'active' | 'cooling-down' | 'locked' | 'inactive' | 'no-login';

@Component({
    selector: 'app-user-custom-detail',
    templateUrl: './user-custom-detail.component.html',
    styleUrls: ['./user-custom-detail.component.scss']
})
export class UserCustomDetailComponent implements OnInit, OnDestroy {

    //
    // Lifecycle
    //
    private destroy$ = new Subject<void>();

    //
    // Data state
    //
    public userId: string | null = null;
    public user: SecurityUserData | null = null;
    public isLoading$ = new BehaviorSubject<boolean>(true);
    public isNotFound: boolean = false;

    //
    // Active tab
    //
    public activeTabId: string = 'overview';

    //
    // Action state
    //
    public actionInProgress: boolean = false;

    //
    // Add/Edit component reference
    //
    @ViewChild('userAddEdit') userAddEdit!: UserCustomAddEditComponent;


    constructor(
        public securityUserService: SecurityUserService,
        private authService: AuthService,
        private route: ActivatedRoute,
        private router: Router,
        private location: Location,
        private alertService: AlertService,
        private navigationService: NavigationService,
        private modalService: NgbModal
    ) { }


    ngOnInit(): void {
        this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
            this.userId = params.get('id');
            if (this.userId) {
                this.loadData();
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
    private loadData(): void {
        if (!this.userId) return;

        this.isLoading$.next(true);
        this.isNotFound = false;

        const params = new SecurityUserQueryParameters();
        params.includeRelations = true;

        this.securityUserService.GetSecurityUser(Number(this.userId), true).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (userData) => {
                if (userData) {
                    this.user = userData;
                    this.isLoading$.next(false);
                } else {
                    this.handleNotFound();
                }
            },
            error: (err) => {
                console.error('Failed to load user', err);
                this.handleNotFound();
            }
        });
    }


    private handleNotFound(): void {
        this.isNotFound = true;
        this.isLoading$.next(false);
        this.alertService.showMessage('Not Found', 'User not found', MessageSeverity.error);
    }


    //
    // Tab handling
    //
    public onTabChange(tabId: string): void {
        this.activeTabId = tabId;
    }


    //
    // User display helpers
    //
    public getUserDisplayName(): string {
        if (!this.user) return '';
        const parts = [this.user.firstName, this.user.lastName].filter(p => p);
        return parts.length > 0 ? parts.join(' ') : this.user.accountName;
    }


    public getUserInitials(): string {
        if (!this.user) return '?';
        const first = this.user.firstName?.charAt(0) ?? '';
        const last = this.user.lastName?.charAt(0) ?? '';
        if (first || last) {
            return (first + last).toUpperCase();
        }
        return this.user.accountName?.charAt(0)?.toUpperCase() ?? '?';
    }


    //
    // Status helpers
    //
    public getUserStatus(): UserStatusType {
        if (!this.user) return 'inactive';

        if (this.user.canLogin !== true) {
            return 'no-login';
        }
        if (this.user.active !== true) {
            return 'inactive';
        }

        const failedCount = this.user.failedLoginCount ?? 0;
        if (failedCount >= 10) return 'locked';
        if (failedCount >= 4) return 'cooling-down';

        return 'active';
    }


    public getStatusLabel(): string {
        const status = this.getUserStatus();
        switch (status) {
            case 'active': return 'Active';
            case 'cooling-down': return 'Cooling Down';
            case 'locked': return 'Locked';
            case 'inactive': return 'Inactive';
            case 'no-login': return 'Cannot Login';
            default: return 'Unknown';
        }
    }


    public getStatusClass(): string {
        const status = this.getUserStatus();
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
    // 2FA status
    //
    public has2FA(): boolean {
        if (!this.user) return false;
        return this.user.twoFactorSendByEmail === true || this.user.twoFactorSendBySMS === true;
    }


    public get2FAMethods(): string {
        if (!this.user) return 'None';
        const methods: string[] = [];
        if (this.user.twoFactorSendByEmail === true) methods.push('Email');
        if (this.user.twoFactorSendBySMS === true) methods.push('SMS');
        return methods.length > 0 ? methods.join(' & ') : 'None';
    }


    //
    // Quick Actions
    //
    public toggleActive(): void {
        if (!this.user || this.actionInProgress) return;

        this.actionInProgress = true;

        const submitData = new SecurityUserSubmitData();
        submitData.id = this.user.id;
        submitData.accountName = this.user.accountName;
        submitData.activeDirectoryAccount = this.user.activeDirectoryAccount;
        submitData.canLogin = this.user.canLogin;
        submitData.mustChangePassword = this.user.mustChangePassword;
        submitData.firstName = this.user.firstName;
        submitData.lastName = this.user.lastName;
        submitData.emailAddress = this.user.emailAddress;
        submitData.readPermissionLevel = this.user.readPermissionLevel;
        submitData.writePermissionLevel = this.user.writePermissionLevel;
        submitData.active = !this.user.active;
        submitData.deleted = this.user.deleted;

        this.securityUserService.PutSecurityUser(this.user.id, submitData).subscribe({
            next: () => {
                if (this.user) {
                    this.user.active = !this.user.active;
                }
                this.actionInProgress = false;
                this.alertService.showMessage('Success', `User ${this.user?.active ? 'activated' : 'deactivated'}`, MessageSeverity.success);
            },
            error: () => {
                this.actionInProgress = false;
                this.alertService.showMessage('Error', 'Failed to update user', MessageSeverity.error);
            }
        });
    }


    public unlockUser(): void {
        if (!this.user || this.actionInProgress) return;

        this.actionInProgress = true;

        const submitData = new SecurityUserSubmitData();
        submitData.id = this.user.id;
        submitData.accountName = this.user.accountName;
        submitData.activeDirectoryAccount = this.user.activeDirectoryAccount;
        submitData.canLogin = this.user.canLogin;
        submitData.mustChangePassword = this.user.mustChangePassword;
        submitData.firstName = this.user.firstName;
        submitData.lastName = this.user.lastName;
        submitData.emailAddress = this.user.emailAddress;
        submitData.readPermissionLevel = this.user.readPermissionLevel;
        submitData.writePermissionLevel = this.user.writePermissionLevel;
        submitData.active = true;
        submitData.deleted = this.user.deleted;
        submitData.failedLoginCount = 0;

        this.securityUserService.PutSecurityUser(this.user.id, submitData).subscribe({
            next: () => {
                if (this.user) {
                    this.user.failedLoginCount = 0;
                    this.user.active = true;
                }
                this.actionInProgress = false;
                this.alertService.showMessage('Success', 'User unlocked successfully', MessageSeverity.success);
            },
            error: () => {
                this.actionInProgress = false;
                this.alertService.showMessage('Error', 'Failed to unlock user', MessageSeverity.error);
            }
        });
    }


    public canUnlock(): boolean {
        if (!this.user) return false;
        const failedCount = this.user.failedLoginCount ?? 0;
        return failedCount >= 4 || this.user.active !== true;
    }


    //
    // Navigation
    //
    public goBack(): void {
        this.location.back();
    }


    public canGoBack(): boolean {
        return window.history.length > 1;
    }


    //
    // Permissions
    //
    public userIsSecurityUserReader(): boolean {
        return this.securityUserService.userIsSecuritySecurityUserReader();
    }


    public userIsSecurityUserWriter(): boolean {
        return this.securityUserService.userIsSecuritySecurityUserWriter();
    }


    //
    // Edit User
    //
    public editUser(): void {
        if (this.user && this.userAddEdit) {
            this.userAddEdit.openModal(this.user);
        }
    }


    public onUserChanged(updatedUser: SecurityUserData): void {
        this.user = updatedUser;
        this.alertService.showMessage('Success', 'User updated successfully', MessageSeverity.success);
    }


    //
    // Image helpers
    //
    public hasUserImage(): boolean {
        return this.user?.image != null && this.user.image.length > 0;
    }


    public getUserImageUrl(): string {
        if (this.user?.image) {
            return 'data:image/png;base64,' + this.user.image;
        }
        return '';
    }


    public openImageUpload(): void {
        if (!this.user || !this.userIsSecurityUserWriter()) return;

        const modalRef = this.modalService.open(UserImageUploadComponent, {
            size: 'md',
            centered: true
        });
        modalRef.componentInstance.user = this.user;

        modalRef.result.then(
            (result) => {
                if (result === true) {
                    this.loadData();
                }
            },
            () => { }
        );
    }
}
