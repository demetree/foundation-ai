import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserService, SecurityUserData, SecurityUserSubmitData } from '../../../security-data-services/security-user.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityUserTitleService } from '../../../security-data-services/security-user-title.service';
import { SecurityTenantService } from '../../../security-data-services/security-tenant.service';
import { SecurityOrganizationService } from '../../../security-data-services/security-organization.service';
import { SecurityDepartmentService } from '../../../security-data-services/security-department.service';
import { SecurityTeamService } from '../../../security-data-services/security-team.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-user-add-edit',
  templateUrl: './security-user-add-edit.component.html',
  styleUrls: ['./security-user-add-edit.component.scss']
})
export class SecurityUserAddEditComponent {
  @ViewChild('securityUserModal') securityUserModal!: TemplateRef<any>;
  @Output() securityUserChanged = new Subject<SecurityUserData[]>();
  @Input() securityUserSubmitData: SecurityUserSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityUserForm: FormGroup = this.fb.group({
        accountName: ['', Validators.required],
        activeDirectoryAccount: [false],
        canLogin: [false],
        mustChangePassword: [false],
        firstName: [''],
        middleName: [''],
        lastName: [''],
        dateOfBirth: [''],
        emailAddress: [''],
        cellPhoneNumber: [''],
        phoneNumber: [''],
        phoneExtension: [''],
        description: [''],
        securityUserTitleId: [null],
        reportsToSecurityUserId: [null],
        authenticationDomain: [''],
        failedLoginCount: [''],
        lastLoginAttempt: [''],
        mostRecentActivity: [''],
        alternateIdentifier: [''],
        image: [''],
        settings: [''],
        securityTenantId: [null],
        readPermissionLevel: ['', Validators.required],
        writePermissionLevel: ['', Validators.required],
        securityOrganizationId: [null],
        securityDepartmentId: [null],
        securityTeamId: [null],
        authenticationToken: [''],
        authenticationTokenExpiry: [''],
        twoFactorToken: [''],
        twoFactorTokenExpiry: [''],
        twoFactorSendByEmail: [false],
        twoFactorSendBySMS: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityUsers$ = this.securityUserService.GetSecurityUserList();
  securityUserTitles$ = this.securityUserTitleService.GetSecurityUserTitleList();
  securityTenants$ = this.securityTenantService.GetSecurityTenantList();
  securityOrganizations$ = this.securityOrganizationService.GetSecurityOrganizationList();
  securityDepartments$ = this.securityDepartmentService.GetSecurityDepartmentList();
  securityTeams$ = this.securityTeamService.GetSecurityTeamList();

  constructor(
    private modalService: NgbModal,
    private securityUserService: SecurityUserService,
    private securityUserTitleService: SecurityUserTitleService,
    private securityTenantService: SecurityTenantService,
    private securityOrganizationService: SecurityOrganizationService,
    private securityDepartmentService: SecurityDepartmentService,
    private securityTeamService: SecurityTeamService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityUserData?: SecurityUserData) {

    if (securityUserData != null) {

      if (!this.securityUserService.userIsSecuritySecurityUserReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Users`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityUserSubmitData = this.securityUserService.ConvertToSecurityUserSubmitData(securityUserData);
      this.isEditMode = true;
      this.objectGuid = securityUserData.objectGuid;

      this.buildFormValues(securityUserData);

    } else {

      if (!this.securityUserService.userIsSecuritySecurityUserWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Users`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityUserModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.securityUserService.userIsSecuritySecurityUserWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Users`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserSubmitData: SecurityUserSubmitData = {
        id: this.securityUserSubmitData?.id || 0,
        accountName: formValue.accountName!.trim(),
        activeDirectoryAccount: !!formValue.activeDirectoryAccount,
        canLogin: !!formValue.canLogin,
        mustChangePassword: !!formValue.mustChangePassword,
        firstName: formValue.firstName?.trim() || null,
        middleName: formValue.middleName?.trim() || null,
        lastName: formValue.lastName?.trim() || null,
        dateOfBirth: formValue.dateOfBirth ? dateTimeLocalToIsoUtc(formValue.dateOfBirth.trim()) : null,
        emailAddress: formValue.emailAddress?.trim() || null,
        cellPhoneNumber: formValue.cellPhoneNumber?.trim() || null,
        phoneNumber: formValue.phoneNumber?.trim() || null,
        phoneExtension: formValue.phoneExtension?.trim() || null,
        description: formValue.description?.trim() || null,
        securityUserTitleId: formValue.securityUserTitleId ? Number(formValue.securityUserTitleId) : null,
        reportsToSecurityUserId: formValue.reportsToSecurityUserId ? Number(formValue.reportsToSecurityUserId) : null,
        authenticationDomain: formValue.authenticationDomain?.trim() || null,
        failedLoginCount: formValue.failedLoginCount ? Number(formValue.failedLoginCount) : null,
        lastLoginAttempt: formValue.lastLoginAttempt ? dateTimeLocalToIsoUtc(formValue.lastLoginAttempt.trim()) : null,
        mostRecentActivity: formValue.mostRecentActivity ? dateTimeLocalToIsoUtc(formValue.mostRecentActivity.trim()) : null,
        alternateIdentifier: formValue.alternateIdentifier?.trim() || null,
        image: formValue.image?.trim() || null,
        settings: formValue.settings?.trim() || null,
        securityTenantId: formValue.securityTenantId ? Number(formValue.securityTenantId) : null,
        readPermissionLevel: Number(formValue.readPermissionLevel),
        writePermissionLevel: Number(formValue.writePermissionLevel),
        securityOrganizationId: formValue.securityOrganizationId ? Number(formValue.securityOrganizationId) : null,
        securityDepartmentId: formValue.securityDepartmentId ? Number(formValue.securityDepartmentId) : null,
        securityTeamId: formValue.securityTeamId ? Number(formValue.securityTeamId) : null,
        authenticationToken: formValue.authenticationToken?.trim() || null,
        authenticationTokenExpiry: formValue.authenticationTokenExpiry ? dateTimeLocalToIsoUtc(formValue.authenticationTokenExpiry.trim()) : null,
        twoFactorToken: formValue.twoFactorToken?.trim() || null,
        twoFactorTokenExpiry: formValue.twoFactorTokenExpiry ? dateTimeLocalToIsoUtc(formValue.twoFactorTokenExpiry.trim()) : null,
        twoFactorSendByEmail: formValue.twoFactorSendByEmail == true ? true : formValue.twoFactorSendByEmail == false ? false : null,
        twoFactorSendBySMS: formValue.twoFactorSendBySMS == true ? true : formValue.twoFactorSendBySMS == false ? false : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityUser(securityUserSubmitData);
      } else {
        this.addSecurityUser(securityUserSubmitData);
      }
  }

  private addSecurityUser(securityUserData: SecurityUserSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityUserData.active = true;
    securityUserData.deleted = false;
    this.securityUserService.PostSecurityUser(securityUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityUser) => {

        this.securityUserService.ClearAllCaches();

        this.securityUserChanged.next([newSecurityUser]);

        this.alertService.showMessage("Security User added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityuser', newSecurityUser.id]);
        }
      },
      error: (err) => {
            let errorMessage: string;

            // Check if err is an Error object (e.g., new Error('message'))
            if (err instanceof Error) {
                errorMessage = err.message || 'An unexpected error occurred.';
            }
            // Check if err is a ServerError object with status and error properties
            else if (err.status && err.error)
            {
                if (err.status === 403)
                {
                    errorMessage = err.error?.message ||
                                   'You do not have permission to save this Security User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityUser(securityUserData: SecurityUserSubmitData) {
    this.securityUserService.PutSecurityUser(securityUserData.id, securityUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityUser) => {

        this.securityUserService.ClearAllCaches();

        this.securityUserChanged.next([updatedSecurityUser]);

        this.alertService.showMessage("Security User updated successfully", '', MessageSeverity.success);

        this.closeModal();
      },
      error: (err) => {
            let errorMessage: string;

            // Check if err is an Error object (e.g., new Error('message'))
            if (err instanceof Error) {
                errorMessage = err.message || 'An unexpected error occurred.';
            }
            // Check if err is a ServerError object with status and error properties
            else if (err.status && err.error)
            {
                if (err.status === 403)
                {
                    errorMessage = err.error?.message ||
                                   'You do not have permission to save this Security User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityUserData: SecurityUserData | null) {

    if (securityUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserForm.reset({
        accountName: '',
        activeDirectoryAccount: false,
        canLogin: false,
        mustChangePassword: false,
        firstName: '',
        middleName: '',
        lastName: '',
        dateOfBirth: '',
        emailAddress: '',
        cellPhoneNumber: '',
        phoneNumber: '',
        phoneExtension: '',
        description: '',
        securityUserTitleId: null,
        reportsToSecurityUserId: null,
        authenticationDomain: '',
        failedLoginCount: '',
        lastLoginAttempt: '',
        mostRecentActivity: '',
        alternateIdentifier: '',
        image: '',
        settings: '',
        securityTenantId: null,
        readPermissionLevel: '',
        writePermissionLevel: '',
        securityOrganizationId: null,
        securityDepartmentId: null,
        securityTeamId: null,
        authenticationToken: '',
        authenticationTokenExpiry: '',
        twoFactorToken: '',
        twoFactorTokenExpiry: '',
        twoFactorSendByEmail: false,
        twoFactorSendBySMS: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserForm.reset({
        accountName: securityUserData.accountName ?? '',
        activeDirectoryAccount: securityUserData.activeDirectoryAccount ?? false,
        canLogin: securityUserData.canLogin ?? false,
        mustChangePassword: securityUserData.mustChangePassword ?? false,
        firstName: securityUserData.firstName ?? '',
        middleName: securityUserData.middleName ?? '',
        lastName: securityUserData.lastName ?? '',
        dateOfBirth: isoUtcStringToDateTimeLocal(securityUserData.dateOfBirth) ?? '',
        emailAddress: securityUserData.emailAddress ?? '',
        cellPhoneNumber: securityUserData.cellPhoneNumber ?? '',
        phoneNumber: securityUserData.phoneNumber ?? '',
        phoneExtension: securityUserData.phoneExtension ?? '',
        description: securityUserData.description ?? '',
        securityUserTitleId: securityUserData.securityUserTitleId,
        reportsToSecurityUserId: securityUserData.reportsToSecurityUserId,
        authenticationDomain: securityUserData.authenticationDomain ?? '',
        failedLoginCount: securityUserData.failedLoginCount?.toString() ?? '',
        lastLoginAttempt: isoUtcStringToDateTimeLocal(securityUserData.lastLoginAttempt) ?? '',
        mostRecentActivity: isoUtcStringToDateTimeLocal(securityUserData.mostRecentActivity) ?? '',
        alternateIdentifier: securityUserData.alternateIdentifier ?? '',
        image: securityUserData.image ?? '',
        settings: securityUserData.settings ?? '',
        securityTenantId: securityUserData.securityTenantId,
        readPermissionLevel: securityUserData.readPermissionLevel?.toString() ?? '',
        writePermissionLevel: securityUserData.writePermissionLevel?.toString() ?? '',
        securityOrganizationId: securityUserData.securityOrganizationId,
        securityDepartmentId: securityUserData.securityDepartmentId,
        securityTeamId: securityUserData.securityTeamId,
        authenticationToken: securityUserData.authenticationToken ?? '',
        authenticationTokenExpiry: isoUtcStringToDateTimeLocal(securityUserData.authenticationTokenExpiry) ?? '',
        twoFactorToken: securityUserData.twoFactorToken ?? '',
        twoFactorTokenExpiry: isoUtcStringToDateTimeLocal(securityUserData.twoFactorTokenExpiry) ?? '',
        twoFactorSendByEmail: securityUserData.twoFactorSendByEmail ?? false,
        twoFactorSendBySMS: securityUserData.twoFactorSendBySMS ?? false,
        active: securityUserData.active ?? true,
        deleted: securityUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityUserForm.markAsPristine();
    this.securityUserForm.markAsUntouched();
  }

  public userIsSecuritySecurityUserReader(): boolean {
    return this.securityUserService.userIsSecuritySecurityUserReader();
  }

  public userIsSecuritySecurityUserWriter(): boolean {
    return this.securityUserService.userIsSecuritySecurityUserWriter();
  }
}
