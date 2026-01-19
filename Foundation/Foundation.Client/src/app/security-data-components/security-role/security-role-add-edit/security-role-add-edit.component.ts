/*
   GENERATED FORM FOR THE SECURITYROLE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityRole table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-role-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityRoleService, SecurityRoleData, SecurityRoleSubmitData } from '../../../security-data-services/security-role.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PrivilegeService } from '../../../security-data-services/privilege.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SecurityRoleFormValues {
  privilegeId: number | bigint,       // For FK link number
  name: string,
  description: string | null,
  comments: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-security-role-add-edit',
  templateUrl: './security-role-add-edit.component.html',
  styleUrls: ['./security-role-add-edit.component.scss']
})
export class SecurityRoleAddEditComponent {
  @ViewChild('securityRoleModal') securityRoleModal!: TemplateRef<any>;
  @Output() securityRoleChanged = new Subject<SecurityRoleData[]>();
  @Input() securityRoleSubmitData: SecurityRoleSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityRoleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityRoleForm: FormGroup = this.fb.group({
        privilegeId: [null, Validators.required],
        name: ['', Validators.required],
        description: [''],
        comments: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityRoles$ = this.securityRoleService.GetSecurityRoleList();
  privileges$ = this.privilegeService.GetPrivilegeList();

  constructor(
    private modalService: NgbModal,
    private securityRoleService: SecurityRoleService,
    private privilegeService: PrivilegeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityRoleData?: SecurityRoleData) {

    if (securityRoleData != null) {

      if (!this.securityRoleService.userIsSecuritySecurityRoleReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Roles`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityRoleSubmitData = this.securityRoleService.ConvertToSecurityRoleSubmitData(securityRoleData);
      this.isEditMode = true;

      this.buildFormValues(securityRoleData);

    } else {

      if (!this.securityRoleService.userIsSecuritySecurityRoleWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Roles`,
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
        this.securityRoleForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityRoleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.securityRoleModal, {
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

    if (this.securityRoleService.userIsSecuritySecurityRoleWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Roles`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityRoleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityRoleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityRoleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityRoleSubmitData: SecurityRoleSubmitData = {
        id: this.securityRoleSubmitData?.id || 0,
        privilegeId: Number(formValue.privilegeId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityRole(securityRoleSubmitData);
      } else {
        this.addSecurityRole(securityRoleSubmitData);
      }
  }

  private addSecurityRole(securityRoleData: SecurityRoleSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityRoleData.active = true;
    securityRoleData.deleted = false;
    this.securityRoleService.PostSecurityRole(securityRoleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityRole) => {

        this.securityRoleService.ClearAllCaches();

        this.securityRoleChanged.next([newSecurityRole]);

        this.alertService.showMessage("Security Role added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityrole', newSecurityRole.id]);
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
                                   'You do not have permission to save this Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityRole(securityRoleData: SecurityRoleSubmitData) {
    this.securityRoleService.PutSecurityRole(securityRoleData.id, securityRoleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityRole) => {

        this.securityRoleService.ClearAllCaches();

        this.securityRoleChanged.next([updatedSecurityRole]);

        this.alertService.showMessage("Security Role updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityRoleData: SecurityRoleData | null) {

    if (securityRoleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityRoleForm.reset({
        privilegeId: null,
        name: '',
        description: '',
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityRoleForm.reset({
        privilegeId: securityRoleData.privilegeId,
        name: securityRoleData.name ?? '',
        description: securityRoleData.description ?? '',
        comments: securityRoleData.comments ?? '',
        active: securityRoleData.active ?? true,
        deleted: securityRoleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityRoleForm.markAsPristine();
    this.securityRoleForm.markAsUntouched();
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


  public userIsSecuritySecurityRoleReader(): boolean {
    return this.securityRoleService.userIsSecuritySecurityRoleReader();
  }

  public userIsSecuritySecurityRoleWriter(): boolean {
    return this.securityRoleService.userIsSecuritySecurityRoleWriter();
  }
}
