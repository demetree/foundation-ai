/*
   GENERATED FORM FOR THE SECURITYORGANIZATIONUSER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityOrganizationUser table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-organization-user-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityOrganizationUserService, SecurityOrganizationUserData, SecurityOrganizationUserSubmitData } from '../../../security-data-services/security-organization-user.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityOrganizationService } from '../../../security-data-services/security-organization.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SecurityOrganizationUserFormValues {
  securityOrganizationId: number | bigint,       // For FK link number
  securityUserId: number | bigint,       // For FK link number
  canRead: boolean,
  canWrite: boolean,
  canChangeHierarchy: boolean,
  canChangeOwner: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-security-organization-user-add-edit',
  templateUrl: './security-organization-user-add-edit.component.html',
  styleUrls: ['./security-organization-user-add-edit.component.scss']
})
export class SecurityOrganizationUserAddEditComponent {
  @ViewChild('securityOrganizationUserModal') securityOrganizationUserModal!: TemplateRef<any>;
  @Output() securityOrganizationUserChanged = new Subject<SecurityOrganizationUserData[]>();
  @Input() securityOrganizationUserSubmitData: SecurityOrganizationUserSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityOrganizationUserFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityOrganizationUserForm: FormGroup = this.fb.group({
        securityOrganizationId: [null, Validators.required],
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

  securityOrganizationUsers$ = this.securityOrganizationUserService.GetSecurityOrganizationUserList();
  securityOrganizations$ = this.securityOrganizationService.GetSecurityOrganizationList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  constructor(
    private modalService: NgbModal,
    private securityOrganizationUserService: SecurityOrganizationUserService,
    private securityOrganizationService: SecurityOrganizationService,
    private securityUserService: SecurityUserService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityOrganizationUserData?: SecurityOrganizationUserData) {

    if (securityOrganizationUserData != null) {

      if (!this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Organization Users`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityOrganizationUserSubmitData = this.securityOrganizationUserService.ConvertToSecurityOrganizationUserSubmitData(securityOrganizationUserData);
      this.isEditMode = true;
      this.objectGuid = securityOrganizationUserData.objectGuid;

      this.buildFormValues(securityOrganizationUserData);

    } else {

      if (!this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Organization Users`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityOrganizationUserForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityOrganizationUserForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.securityOrganizationUserModal, {
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

    if (this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Organization Users`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityOrganizationUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityOrganizationUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityOrganizationUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityOrganizationUserSubmitData: SecurityOrganizationUserSubmitData = {
        id: this.securityOrganizationUserSubmitData?.id || 0,
        securityOrganizationId: Number(formValue.securityOrganizationId),
        securityUserId: Number(formValue.securityUserId),
        canRead: !!formValue.canRead,
        canWrite: !!formValue.canWrite,
        canChangeHierarchy: !!formValue.canChangeHierarchy,
        canChangeOwner: !!formValue.canChangeOwner,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityOrganizationUser(securityOrganizationUserSubmitData);
      } else {
        this.addSecurityOrganizationUser(securityOrganizationUserSubmitData);
      }
  }

  private addSecurityOrganizationUser(securityOrganizationUserData: SecurityOrganizationUserSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityOrganizationUserData.active = true;
    securityOrganizationUserData.deleted = false;
    this.securityOrganizationUserService.PostSecurityOrganizationUser(securityOrganizationUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityOrganizationUser) => {

        this.securityOrganizationUserService.ClearAllCaches();

        this.securityOrganizationUserChanged.next([newSecurityOrganizationUser]);

        this.alertService.showMessage("Security Organization User added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityorganizationuser', newSecurityOrganizationUser.id]);
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
                                   'You do not have permission to save this Security Organization User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Organization User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Organization User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityOrganizationUser(securityOrganizationUserData: SecurityOrganizationUserSubmitData) {
    this.securityOrganizationUserService.PutSecurityOrganizationUser(securityOrganizationUserData.id, securityOrganizationUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityOrganizationUser) => {

        this.securityOrganizationUserService.ClearAllCaches();

        this.securityOrganizationUserChanged.next([updatedSecurityOrganizationUser]);

        this.alertService.showMessage("Security Organization User updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security Organization User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Organization User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Organization User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityOrganizationUserData: SecurityOrganizationUserData | null) {

    if (securityOrganizationUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityOrganizationUserForm.reset({
        securityOrganizationId: null,
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
        this.securityOrganizationUserForm.reset({
        securityOrganizationId: securityOrganizationUserData.securityOrganizationId,
        securityUserId: securityOrganizationUserData.securityUserId,
        canRead: securityOrganizationUserData.canRead ?? false,
        canWrite: securityOrganizationUserData.canWrite ?? false,
        canChangeHierarchy: securityOrganizationUserData.canChangeHierarchy ?? false,
        canChangeOwner: securityOrganizationUserData.canChangeOwner ?? false,
        active: securityOrganizationUserData.active ?? true,
        deleted: securityOrganizationUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityOrganizationUserForm.markAsPristine();
    this.securityOrganizationUserForm.markAsUntouched();
  }

  //
  // Helper method to determine if a field should be hidden based on the hiddenFields input.
  // Returns true if the field is in the array, false otherwise.
  //
  public isFieldHidden(fieldName: string): boolean {
    // Explicit check for array existence to avoid runtime errors.
    if (this.hiddenFields === null || this.hiddenFields === undefined) {
      return false;
    }
    // Use traditional includes method for clarity.
    return this.hiddenFields.includes(fieldName);
  }


  public userIsSecuritySecurityOrganizationUserReader(): boolean {
    return this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserReader();
  }

  public userIsSecuritySecurityOrganizationUserWriter(): boolean {
    return this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserWriter();
  }
}
