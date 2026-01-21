/*
   GENERATED FORM FOR THE SECURITYUSERSECURITYROLE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityUserSecurityRole table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-user-security-role-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserSecurityRoleService, SecurityUserSecurityRoleData, SecurityUserSecurityRoleSubmitData } from '../../../security-data-services/security-user-security-role.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { SecurityRoleService } from '../../../security-data-services/security-role.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SecurityUserSecurityRoleFormValues {
  securityUserId: number | bigint,       // For FK link number
  securityRoleId: number | bigint,       // For FK link number
  comments: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-security-user-security-role-add-edit',
  templateUrl: './security-user-security-role-add-edit.component.html',
  styleUrls: ['./security-user-security-role-add-edit.component.scss']
})
export class SecurityUserSecurityRoleAddEditComponent {
  @ViewChild('securityUserSecurityRoleModal') securityUserSecurityRoleModal!: TemplateRef<any>;
  @Output() securityUserSecurityRoleChanged = new Subject<SecurityUserSecurityRoleData[]>();
  @Input() securityUserSecurityRoleSubmitData: SecurityUserSecurityRoleSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityUserSecurityRoleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityUserSecurityRoleForm: FormGroup = this.fb.group({
        securityUserId: [null, Validators.required],
        securityRoleId: [null, Validators.required],
        comments: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityUserSecurityRoles$ = this.securityUserSecurityRoleService.GetSecurityUserSecurityRoleList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();
  securityRoles$ = this.securityRoleService.GetSecurityRoleList();

  constructor(
    private modalService: NgbModal,
    private securityUserSecurityRoleService: SecurityUserSecurityRoleService,
    private securityUserService: SecurityUserService,
    private securityRoleService: SecurityRoleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityUserSecurityRoleData?: SecurityUserSecurityRoleData) {

    if (securityUserSecurityRoleData != null) {

      if (!this.securityUserSecurityRoleService.userIsSecuritySecurityUserSecurityRoleReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security User Security Roles`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityUserSecurityRoleSubmitData = this.securityUserSecurityRoleService.ConvertToSecurityUserSecurityRoleSubmitData(securityUserSecurityRoleData);
      this.isEditMode = true;

      this.buildFormValues(securityUserSecurityRoleData);

    } else {

      if (!this.securityUserSecurityRoleService.userIsSecuritySecurityUserSecurityRoleWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security User Security Roles`,
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
        this.securityUserSecurityRoleForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityUserSecurityRoleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.securityUserSecurityRoleModal, {
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

    if (this.securityUserSecurityRoleService.userIsSecuritySecurityUserSecurityRoleWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security User Security Roles`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityUserSecurityRoleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserSecurityRoleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserSecurityRoleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserSecurityRoleSubmitData: SecurityUserSecurityRoleSubmitData = {
        id: this.securityUserSecurityRoleSubmitData?.id || 0,
        securityUserId: Number(formValue.securityUserId),
        securityRoleId: Number(formValue.securityRoleId),
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityUserSecurityRole(securityUserSecurityRoleSubmitData);
      } else {
        this.addSecurityUserSecurityRole(securityUserSecurityRoleSubmitData);
      }
  }

  private addSecurityUserSecurityRole(securityUserSecurityRoleData: SecurityUserSecurityRoleSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityUserSecurityRoleData.active = true;
    securityUserSecurityRoleData.deleted = false;
    this.securityUserSecurityRoleService.PostSecurityUserSecurityRole(securityUserSecurityRoleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityUserSecurityRole) => {

        this.securityUserSecurityRoleService.ClearAllCaches();

        this.securityUserSecurityRoleChanged.next([newSecurityUserSecurityRole]);

        this.alertService.showMessage("Security User Security Role added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityusersecurityrole', newSecurityUserSecurityRole.id]);
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
                                   'You do not have permission to save this Security User Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityUserSecurityRole(securityUserSecurityRoleData: SecurityUserSecurityRoleSubmitData) {
    this.securityUserSecurityRoleService.PutSecurityUserSecurityRole(securityUserSecurityRoleData.id, securityUserSecurityRoleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityUserSecurityRole) => {

        this.securityUserSecurityRoleService.ClearAllCaches();

        this.securityUserSecurityRoleChanged.next([updatedSecurityUserSecurityRole]);

        this.alertService.showMessage("Security User Security Role updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security User Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityUserSecurityRoleData: SecurityUserSecurityRoleData | null) {

    if (securityUserSecurityRoleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserSecurityRoleForm.reset({
        securityUserId: null,
        securityRoleId: null,
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserSecurityRoleForm.reset({
        securityUserId: securityUserSecurityRoleData.securityUserId,
        securityRoleId: securityUserSecurityRoleData.securityRoleId,
        comments: securityUserSecurityRoleData.comments ?? '',
        active: securityUserSecurityRoleData.active ?? true,
        deleted: securityUserSecurityRoleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityUserSecurityRoleForm.markAsPristine();
    this.securityUserSecurityRoleForm.markAsUntouched();
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


  public userIsSecuritySecurityUserSecurityRoleReader(): boolean {
    return this.securityUserSecurityRoleService.userIsSecuritySecurityUserSecurityRoleReader();
  }

  public userIsSecuritySecurityUserSecurityRoleWriter(): boolean {
    return this.securityUserSecurityRoleService.userIsSecuritySecurityUserSecurityRoleWriter();
  }
}
