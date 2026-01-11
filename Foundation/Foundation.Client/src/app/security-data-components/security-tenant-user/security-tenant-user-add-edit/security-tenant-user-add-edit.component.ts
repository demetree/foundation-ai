import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityTenantUserService, SecurityTenantUserData, SecurityTenantUserSubmitData } from '../../../security-data-services/security-tenant-user.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityTenantService } from '../../../security-data-services/security-tenant.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-tenant-user-add-edit',
  templateUrl: './security-tenant-user-add-edit.component.html',
  styleUrls: ['./security-tenant-user-add-edit.component.scss']
})
export class SecurityTenantUserAddEditComponent {
  @ViewChild('securityTenantUserModal') securityTenantUserModal!: TemplateRef<any>;
  @Output() securityTenantUserChanged = new Subject<SecurityTenantUserData[]>();
  @Input() securityTenantUserSubmitData: SecurityTenantUserSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityTenantUserForm: FormGroup = this.fb.group({
        securityTenantId: [null, Validators.required],
        securityUserId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityTenantUsers$ = this.securityTenantUserService.GetSecurityTenantUserList();
  securityTenants$ = this.securityTenantService.GetSecurityTenantList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  constructor(
    private modalService: NgbModal,
    private securityTenantUserService: SecurityTenantUserService,
    private securityTenantService: SecurityTenantService,
    private securityUserService: SecurityUserService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityTenantUserData?: SecurityTenantUserData) {

    if (securityTenantUserData != null) {

      if (!this.securityTenantUserService.userIsSecuritySecurityTenantUserReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Tenant Users`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityTenantUserSubmitData = this.securityTenantUserService.ConvertToSecurityTenantUserSubmitData(securityTenantUserData);
      this.isEditMode = true;
      this.objectGuid = securityTenantUserData.objectGuid;

      this.buildFormValues(securityTenantUserData);

    } else {

      if (!this.securityTenantUserService.userIsSecuritySecurityTenantUserWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Tenant Users`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityTenantUserModal, {
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

    if (this.securityTenantUserService.userIsSecuritySecurityTenantUserWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Tenant Users`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityTenantUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityTenantUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityTenantUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityTenantUserSubmitData: SecurityTenantUserSubmitData = {
        id: this.securityTenantUserSubmitData?.id || 0,
        securityTenantId: Number(formValue.securityTenantId),
        securityUserId: Number(formValue.securityUserId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityTenantUser(securityTenantUserSubmitData);
      } else {
        this.addSecurityTenantUser(securityTenantUserSubmitData);
      }
  }

  private addSecurityTenantUser(securityTenantUserData: SecurityTenantUserSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityTenantUserData.active = true;
    securityTenantUserData.deleted = false;
    this.securityTenantUserService.PostSecurityTenantUser(securityTenantUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityTenantUser) => {

        this.securityTenantUserService.ClearAllCaches();

        this.securityTenantUserChanged.next([newSecurityTenantUser]);

        this.alertService.showMessage("Security Tenant User added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securitytenantuser', newSecurityTenantUser.id]);
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
                                   'You do not have permission to save this Security Tenant User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Tenant User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Tenant User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityTenantUser(securityTenantUserData: SecurityTenantUserSubmitData) {
    this.securityTenantUserService.PutSecurityTenantUser(securityTenantUserData.id, securityTenantUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityTenantUser) => {

        this.securityTenantUserService.ClearAllCaches();

        this.securityTenantUserChanged.next([updatedSecurityTenantUser]);

        this.alertService.showMessage("Security Tenant User updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security Tenant User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Tenant User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Tenant User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityTenantUserData: SecurityTenantUserData | null) {

    if (securityTenantUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityTenantUserForm.reset({
        securityTenantId: null,
        securityUserId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityTenantUserForm.reset({
        securityTenantId: securityTenantUserData.securityTenantId,
        securityUserId: securityTenantUserData.securityUserId,
        active: securityTenantUserData.active ?? true,
        deleted: securityTenantUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityTenantUserForm.markAsPristine();
    this.securityTenantUserForm.markAsUntouched();
  }

  public userIsSecuritySecurityTenantUserReader(): boolean {
    return this.securityTenantUserService.userIsSecuritySecurityTenantUserReader();
  }

  public userIsSecuritySecurityTenantUserWriter(): boolean {
    return this.securityTenantUserService.userIsSecuritySecurityTenantUserWriter();
  }
}
