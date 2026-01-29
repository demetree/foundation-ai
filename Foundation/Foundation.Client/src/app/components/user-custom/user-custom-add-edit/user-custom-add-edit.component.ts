import { Component, ViewChild, TemplateRef, Output, Input, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Subject, Observable, BehaviorSubject, of } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserService, SecurityUserData, SecurityUserSubmitData } from '../../../security-data-services/security-user.service';
import { SecurityUserCustomService } from '../../../security-data-services/security-user-custom.service';
import { SecurityUserTitleService, SecurityUserTitleData } from '../../../security-data-services/security-user-title.service';
import { SecurityTenantService, SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { SecurityOrganizationService, SecurityOrganizationData } from '../../../security-data-services/security-organization.service';
import { SecurityDepartmentService, SecurityDepartmentData } from '../../../security-data-services/security-department.service';
import { SecurityTeamService, SecurityTeamData } from '../../../security-data-services/security-team.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AdminCreateUserRequest } from '../../../models/create-user-model';
import { UserImageUploadComponent } from '../user-image-upload/user-image-upload.component';

@Component({
    selector: 'app-user-custom-add-edit',
    templateUrl: './user-custom-add-edit.component.html',
    styleUrls: ['./user-custom-add-edit.component.scss']
})
export class UserCustomAddEditComponent implements OnInit, OnDestroy {
    @ViewChild('userModal') userModal!: TemplateRef<any>;
    @Output() userChanged = new Subject<SecurityUserData>();
    @Input() showAddButton: boolean = false;

    private destroy$ = new Subject<void>();
    private modalRef: NgbModalRef | null = null;

    userForm!: FormGroup;
    isEditMode: boolean = false;
    isSaving: boolean = false;
    currentUser: SecurityUserData | null = null;
    objectGuid: string = '';

    // Dropdown data
    securityUserTitles$: Observable<SecurityUserTitleData[]>;
    securityUsers$: Observable<SecurityUserData[]>;
    securityTenants$: Observable<SecurityTenantData[]>;

    // Cascading dropdown data
    filteredOrganizations$ = new BehaviorSubject<SecurityOrganizationData[]>([]);
    filteredDepartments$ = new BehaviorSubject<SecurityDepartmentData[]>([]);
    filteredTeams$ = new BehaviorSubject<SecurityTeamData[]>([]);

    // Full lists for filtering
    private allOrganizations: SecurityOrganizationData[] = [];
    private allDepartments: SecurityDepartmentData[] = [];
    private allTeams: SecurityTeamData[] = [];

    // Active sections for accordion
    activeSection: string = 'personal';

    constructor(
        private modalService: NgbModal,
        private securityUserService: SecurityUserService,
        private securityUserCustomService: SecurityUserCustomService,
        private securityUserTitleService: SecurityUserTitleService,
        private securityTenantService: SecurityTenantService,
        private securityOrganizationService: SecurityOrganizationService,
        private securityDepartmentService: SecurityDepartmentService,
        private securityTeamService: SecurityTeamService,
        private authService: AuthService,
        private alertService: AlertService,
        private router: Router,
        private fb: FormBuilder
    ) {
        this.securityUserTitles$ = this.securityUserTitleService.GetSecurityUserTitleList();
        this.securityUsers$ = this.securityUserService.GetSecurityUserList();
        this.securityTenants$ = this.securityTenantService.GetSecurityTenantList();

        this.initForm();
    }

    ngOnInit(): void {
        this.loadCascadingData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private initForm(): void {
        this.userForm = this.fb.group({
            // Personal Information
            firstName: [''],
            middleName: [''],
            lastName: [''],
            dateOfBirth: [''],
            securityUserTitleId: [null],
            reportsToSecurityUserId: [null],
            description: [''],

            // Account Information
            accountName: ['', Validators.required],
            emailAddress: ['', [Validators.email]],
            password: [''],
            confirmPassword: [''],

            // Contact Information
            cellPhoneNumber: [''],
            phoneNumber: [''],
            phoneExtension: [''],

            // Organization
            securityTenantId: [null],
            securityOrganizationId: [null],
            securityDepartmentId: [null],
            securityTeamId: [null],

            // Security Settings
            activeDirectoryAccount: [false],
            canLogin: [true],
            mustChangePassword: [false],
            readPermissionLevel: [0, Validators.required],
            writePermissionLevel: [0, Validators.required],

            // Two Factor Authentication
            twoFactorSendByEmail: [false],
            twoFactorSendBySMS: [false],

            // Status (edit mode only)
            active: [true],
            deleted: [false],

            // Hidden fields (managed internally)
            authenticationDomain: [''],
            alternateIdentifier: [''],
            image: [''],
            settings: ['']
        });

        // Set up cascading dropdown listeners
        this.userForm.get('securityTenantId')?.valueChanges
            .pipe(takeUntil(this.destroy$))
            .subscribe(tenantId => this.onTenantChange(tenantId));

        this.userForm.get('securityOrganizationId')?.valueChanges
            .pipe(takeUntil(this.destroy$))
            .subscribe(orgId => this.onOrganizationChange(orgId));

        this.userForm.get('securityDepartmentId')?.valueChanges
            .pipe(takeUntil(this.destroy$))
            .subscribe(deptId => this.onDepartmentChange(deptId));
    }

    private loadCascadingData(): void {
        this.securityOrganizationService.GetSecurityOrganizationList()
            .pipe(takeUntil(this.destroy$))
            .subscribe((orgs: SecurityOrganizationData[]) => {
                this.allOrganizations = orgs;
                this.filterOrganizations(this.userForm.get('securityTenantId')?.value);
            });

        this.securityDepartmentService.GetSecurityDepartmentList()
            .pipe(takeUntil(this.destroy$))
            .subscribe((depts: SecurityDepartmentData[]) => {
                this.allDepartments = depts;
                this.filterDepartments(this.userForm.get('securityOrganizationId')?.value);
            });

        this.securityTeamService.GetSecurityTeamList()
            .pipe(takeUntil(this.destroy$))
            .subscribe((teams: SecurityTeamData[]) => {
                this.allTeams = teams;
                this.filterTeams(this.userForm.get('securityDepartmentId')?.value);
            });
    }

    private onTenantChange(tenantId: number | null): void {
        this.filterOrganizations(tenantId);
        // Reset dependent fields
        this.userForm.patchValue({
            securityOrganizationId: null,
            securityDepartmentId: null,
            securityTeamId: null
        }, { emitEvent: false });
        this.filteredDepartments$.next([]);
        this.filteredTeams$.next([]);
    }

    private onOrganizationChange(orgId: number | null): void {
        this.filterDepartments(orgId);
        // Reset dependent fields
        this.userForm.patchValue({
            securityDepartmentId: null,
            securityTeamId: null
        }, { emitEvent: false });
        this.filteredTeams$.next([]);
    }

    private onDepartmentChange(deptId: number | null): void {
        this.filterTeams(deptId);
        // Reset dependent field
        this.userForm.patchValue({
            securityTeamId: null
        }, { emitEvent: false });
    }

    private filterOrganizations(tenantId: number | null): void {
        if (!tenantId) {
            this.filteredOrganizations$.next([]);
        } else {
            const filtered = this.allOrganizations.filter(o => o.securityTenantId === tenantId);
            this.filteredOrganizations$.next(filtered);
        }
    }

    private filterDepartments(orgId: number | null): void {
        if (!orgId) {
            this.filteredDepartments$.next([]);
        } else {
            const filtered = this.allDepartments.filter(d => d.securityOrganizationId === orgId);
            this.filteredDepartments$.next(filtered);
        }
    }

    private filterTeams(deptId: number | null): void {
        if (!deptId) {
            this.filteredTeams$.next([]);
        } else {
            const filtered = this.allTeams.filter(t => t.securityDepartmentId === deptId);
            this.filteredTeams$.next(filtered);
        }
    }

    openModal(userData?: SecurityUserData): void {
        this.isEditMode = !!userData;
        this.currentUser = userData || null;

        if (userData) {
            this.objectGuid = userData.objectGuid || '';
            this.populateForm(userData);
        } else {
            this.userForm.reset({
                canLogin: true,
                activeDirectoryAccount: false,
                mustChangePassword: false,
                readPermissionLevel: 0,
                writePermissionLevel: 0,
                active: true,
                deleted: false,
                twoFactorSendByEmail: false,
                twoFactorSendBySMS: false
            });
        }

        this.modalRef = this.modalService.open(this.userModal, {
            size: 'lg',
            backdrop: 'static',
            centered: true,
            scrollable: true
        });
    }

    closeModal(): void {
        if (this.modalRef) {
            this.modalRef.close();
            this.modalRef = null;
        }
    }

    private populateForm(user: SecurityUserData): void {
        this.userForm.patchValue({
            firstName: user.firstName,
            middleName: user.middleName,
            lastName: user.lastName,
            dateOfBirth: user.dateOfBirth ? isoUtcStringToDateTimeLocal(user.dateOfBirth) : null,
            securityUserTitleId: user.securityUserTitleId,
            reportsToSecurityUserId: user.reportsToSecurityUserId,
            description: user.description,
            accountName: user.accountName,
            emailAddress: user.emailAddress,
            cellPhoneNumber: user.cellPhoneNumber,
            phoneNumber: user.phoneNumber,
            phoneExtension: user.phoneExtension,
            securityTenantId: user.securityTenantId,
            securityOrganizationId: user.securityOrganizationId,
            securityDepartmentId: user.securityDepartmentId,
            securityTeamId: user.securityTeamId,
            activeDirectoryAccount: user.activeDirectoryAccount,
            canLogin: user.canLogin,
            mustChangePassword: user.mustChangePassword,
            readPermissionLevel: user.readPermissionLevel,
            writePermissionLevel: user.writePermissionLevel,
            twoFactorSendByEmail: user.twoFactorSendByEmail,
            twoFactorSendBySMS: user.twoFactorSendBySMS,
            active: user.active,
            deleted: user.deleted,
            authenticationDomain: user.authenticationDomain,
            alternateIdentifier: user.alternateIdentifier,
            image: user.image,
            settings: user.settings
        });

        // Trigger cascading filters with current values
        this.filterOrganizations(user.securityTenantId as number);
        setTimeout(() => {
            this.filterDepartments(user.securityOrganizationId as number);
            setTimeout(() => {
                this.filterTeams(user.securityDepartmentId as number);
            }, 50);
        }, 50);
    }

    submitForm(): void {
        if (this.userForm.invalid || this.isSaving) {
            return;
        }

        // For new users, validate password
        const formValues = this.userForm.value;
        if (!this.isEditMode) {
            if (!formValues.password || formValues.password.length < 6) {
                this.alertService.showMessage('Validation Error', 'Password is required and must be at least 6 characters', MessageSeverity.error);
                return;
            }
            if (formValues.password !== formValues.confirmPassword) {
                this.alertService.showMessage('Validation Error', 'Passwords do not match', MessageSeverity.error);
                return;
            }
        }

        this.isSaving = true;

        const submitData: any = {
            id: this.currentUser?.id || 0,
            accountName: formValues.accountName,
            activeDirectoryAccount: formValues.activeDirectoryAccount,
            canLogin: formValues.canLogin,
            mustChangePassword: formValues.mustChangePassword,
            firstName: formValues.firstName || null,
            middleName: formValues.middleName || null,
            lastName: formValues.lastName || null,
            dateOfBirth: formValues.dateOfBirth ? dateTimeLocalToIsoUtc(formValues.dateOfBirth) : null,
            emailAddress: formValues.emailAddress || null,
            cellPhoneNumber: formValues.cellPhoneNumber || null,
            phoneNumber: formValues.phoneNumber || null,
            phoneExtension: formValues.phoneExtension || null,
            description: formValues.description || null,
            securityUserTitleId: formValues.securityUserTitleId,
            reportsToSecurityUserId: formValues.reportsToSecurityUserId,
            authenticationDomain: formValues.authenticationDomain || null,
            failedLoginCount: this.currentUser?.failedLoginCount || null,
            lastLoginAttempt: this.currentUser?.lastLoginAttempt || null,
            mostRecentActivity: this.currentUser?.mostRecentActivity || null,
            alternateIdentifier: formValues.alternateIdentifier || null,
            image: formValues.image || null,
            settings: formValues.settings || null,
            securityTenantId: formValues.securityTenantId,
            readPermissionLevel: formValues.readPermissionLevel,
            writePermissionLevel: formValues.writePermissionLevel,
            securityOrganizationId: formValues.securityOrganizationId,
            securityDepartmentId: formValues.securityDepartmentId,
            securityTeamId: formValues.securityTeamId,
            authenticationToken: this.currentUser?.authenticationToken || null,
            authenticationTokenExpiry: this.currentUser?.authenticationTokenExpiry || null,
            twoFactorToken: this.currentUser?.twoFactorToken || null,
            twoFactorTokenExpiry: this.currentUser?.twoFactorTokenExpiry || null,
            twoFactorSendByEmail: formValues.twoFactorSendByEmail,
            twoFactorSendBySMS: formValues.twoFactorSendBySMS,
            active: formValues.active,
            deleted: formValues.deleted
        };

        if (this.isEditMode) {
            this.updateUser(submitData);
        } else {
            // Use AdminCreateUser endpoint for new users - handles password securely
            const createRequest: AdminCreateUserRequest = {
                accountName: formValues.accountName,
                password: formValues.password,
                firstName: formValues.firstName || null,
                middleName: formValues.middleName || null,
                lastName: formValues.lastName || null,
                emailAddress: formValues.emailAddress || null,
                cellPhoneNumber: formValues.cellPhoneNumber || null,
                phoneNumber: formValues.phoneNumber || null,
                phoneExtension: formValues.phoneExtension || null,
                description: formValues.description || null,
                securityUserTitleId: formValues.securityUserTitleId ?? null,
                reportsToSecurityUserId: formValues.reportsToSecurityUserId ?? null,
                securityTenantId: formValues.securityTenantId ?? null,
                securityOrganizationId: formValues.securityOrganizationId ?? null,
                securityDepartmentId: formValues.securityDepartmentId ?? null,
                securityTeamId: formValues.securityTeamId ?? null,
                readPermissionLevel: formValues.readPermissionLevel ?? 0,
                writePermissionLevel: formValues.writePermissionLevel ?? 0,
                mustChangePassword: formValues.mustChangePassword,
                twoFactorSendByEmail: formValues.twoFactorSendByEmail ?? false,
                twoFactorSendBySMS: formValues.twoFactorSendBySMS ?? false
            };
            this.addUser(createRequest);
        }
    }

    private addUser(request: AdminCreateUserRequest): void {
        this.securityUserCustomService.AdminCreateUser(request).subscribe({
            next: (newUser: SecurityUserData) => {
                this.alertService.showMessage('Success', 'User created successfully', MessageSeverity.success);
                this.userChanged.next(newUser);
                this.isSaving = false;
                this.closeModal();
            },
            error: (err: any) => {
                this.alertService.showStickyMessage('Error', 'Failed to create user: ' + err.message, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }

    private updateUser(submitData: SecurityUserSubmitData): void {
        this.securityUserService.PutSecurityUser(submitData.id, submitData).subscribe({
            next: (updatedUser: SecurityUserData) => {
                this.alertService.showMessage('Success', 'User updated successfully', MessageSeverity.success);
                this.userChanged.next(updatedUser);
                this.isSaving = false;
                this.closeModal();
            },
            error: (err: any) => {
                this.alertService.showStickyMessage('Error', 'Failed to update user: ' + err.message, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }

    setActiveSection(section: string): void {
        this.activeSection = section;
    }

    userIsWriter(): boolean {
        return true; // Simplified - actual permission check should use authService
    }

    getDisplayName(): string {
        if (this.currentUser) {
            const parts = [this.currentUser.firstName, this.currentUser.lastName].filter(p => p);
            return parts.length > 0 ? parts.join(' ') : this.currentUser.accountName;
        }
        return '';
    }


    //
    // Image helpers - AI-assisted development January 2026
    //
    hasUserImage(): boolean {
        return this.currentUser?.image != null && this.currentUser.image.length > 0;
    }


    getUserImageUrl(): string {
        if (this.currentUser?.image) {
            return 'data:image/png;base64,' + this.currentUser.image;
        }
        return '';
    }


    getUserInitials(): string {
        if (!this.currentUser) {
            return '?';
        }

        const first = this.currentUser.firstName?.charAt(0) ?? '';
        const last = this.currentUser.lastName?.charAt(0) ?? '';

        if (first || last) {
            return (first + last).toUpperCase();
        }

        return this.currentUser.accountName?.charAt(0)?.toUpperCase() ?? '?';
    }


    openImageUpload(): void {
        if (!this.currentUser) {
            return;
        }

        const modalRef = this.modalService.open(UserImageUploadComponent, {
            size: 'md',
            centered: true
        });

        modalRef.componentInstance.user = this.currentUser;

        modalRef.result.then(
            (result) => {
                //
                // On success, reload the user data to get the updated image
                //
                if (result === true && this.currentUser) {
                    this.securityUserService.GetSecurityUser(this.currentUser.id, true).subscribe({
                        next: (updatedUser) => {
                            if (updatedUser) {
                                this.currentUser = updatedUser;
                                this.userForm.patchValue({ image: updatedUser.image });
                            }
                        }
                    });
                }
            },
            () => { }
        );
    }
}
