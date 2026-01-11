import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityDepartmentUserService, SecurityDepartmentUserData, SecurityDepartmentUserSubmitData } from '../../../security-data-services/security-department-user.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityDepartmentService } from '../../../security-data-services/security-department.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-department-user-add-edit',
  templateUrl: './security-department-user-add-edit.component.html',
  styleUrls: ['./security-department-user-add-edit.component.scss']
})
export class SecurityDepartmentUserAddEditComponent {
  @ViewChild('securityDepartmentUserModal') securityDepartmentUserModal!: TemplateRef<any>;
  @Output() securityDepartmentUserChanged = new Subject<SecurityDepartmentUserData[]>();
  @Input() securityDepartmentUserSubmitData: SecurityDepartmentUserSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityDepartmentUserForm: FormGroup = this.fb.group({
        securityDepartmentId: [null, Validators.required],
        securityUserId: [null, Validators.required],
        canRead: [false],
        canWrite: [false],
        canChangeHierarchy: [false],
        canChangeOwner: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityDepartmentUsers$ = this.securityDepartmentUserService.GetSecurityDepartmentUserList();
  securityDepartments$ = this.securityDepartmentService.GetSecurityDepartmentList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  constructor(
    private modalService: NgbModal,
    private securityDepartmentUserService: SecurityDepartmentUserService,
    private securityDepartmentService: SecurityDepartmentService,
    private securityUserService: SecurityUserService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityDepartmentUserData?: SecurityDepartmentUserData) {

    if (securityDepartmentUserData != null) {

      if (!this.securityDepartmentUserService.userIsSecuritySecurityDepartmentUserReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Department Users`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityDepartmentUserSubmitData = this.securityDepartmentUserService.ConvertToSecurityDepartmentUserSubmitData(securityDepartmentUserData);
      this.isEditMode = true;
      this.objectGuid = securityDepartmentUserData.objectGuid;

      this.buildFormValues(securityDepartmentUserData);

    } else {

      if (!this.securityDepartmentUserService.userIsSecuritySecurityDepartmentUserWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Department Users`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityDepartmentUserModal, {
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

    if (this.securityDepartmentUserService.userIsSecuritySecurityDepartmentUserWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Department Users`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityDepartmentUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityDepartmentUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityDepartmentUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityDepartmentUserSubmitData: SecurityDepartmentUserSubmitData = {
        id: this.securityDepartmentUserSubmitData?.id || 0,
        securityDepartmentId: Number(formValue.securityDepartmentId),
        securityUserId: Number(formValue.securityUserId),
        canRead: !!formValue.canRead,
        canWrite: !!formValue.canWrite,
        canChangeHierarchy: !!formValue.canChangeHierarchy,
        canChangeOwner: !!formValue.canChangeOwner,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityDepartmentUser(securityDepartmentUserSubmitData);
      } else {
        this.addSecurityDepartmentUser(securityDepartmentUserSubmitData);
      }
  }

  private addSecurityDepartmentUser(securityDepartmentUserData: SecurityDepartmentUserSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityDepartmentUserData.active = true;
    securityDepartmentUserData.deleted = false;
    this.securityDepartmentUserService.PostSecurityDepartmentUser(securityDepartmentUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityDepartmentUser) => {

        this.securityDepartmentUserService.ClearAllCaches();

        this.securityDepartmentUserChanged.next([newSecurityDepartmentUser]);

        this.alertService.showMessage("Security Department User added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securitydepartmentuser', newSecurityDepartmentUser.id]);
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
                                   'You do not have permission to save this Security Department User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Department User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Department User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityDepartmentUser(securityDepartmentUserData: SecurityDepartmentUserSubmitData) {
    this.securityDepartmentUserService.PutSecurityDepartmentUser(securityDepartmentUserData.id, securityDepartmentUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityDepartmentUser) => {

        this.securityDepartmentUserService.ClearAllCaches();

        this.securityDepartmentUserChanged.next([updatedSecurityDepartmentUser]);

        this.alertService.showMessage("Security Department User updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security Department User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Department User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Department User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityDepartmentUserData: SecurityDepartmentUserData | null) {

    if (securityDepartmentUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityDepartmentUserForm.reset({
        securityDepartmentId: null,
        securityUserId: null,
        canRead: false,
        canWrite: false,
        canChangeHierarchy: false,
        canChangeOwner: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityDepartmentUserForm.reset({
        securityDepartmentId: securityDepartmentUserData.securityDepartmentId,
        securityUserId: securityDepartmentUserData.securityUserId,
        canRead: securityDepartmentUserData.canRead ?? false,
        canWrite: securityDepartmentUserData.canWrite ?? false,
        canChangeHierarchy: securityDepartmentUserData.canChangeHierarchy ?? false,
        canChangeOwner: securityDepartmentUserData.canChangeOwner ?? false,
        active: securityDepartmentUserData.active ?? true,
        deleted: securityDepartmentUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityDepartmentUserForm.markAsPristine();
    this.securityDepartmentUserForm.markAsUntouched();
  }

  public userIsSecuritySecurityDepartmentUserReader(): boolean {
    return this.securityDepartmentUserService.userIsSecuritySecurityDepartmentUserReader();
  }

  public userIsSecuritySecurityDepartmentUserWriter(): boolean {
    return this.securityDepartmentUserService.userIsSecuritySecurityDepartmentUserWriter();
  }
}
