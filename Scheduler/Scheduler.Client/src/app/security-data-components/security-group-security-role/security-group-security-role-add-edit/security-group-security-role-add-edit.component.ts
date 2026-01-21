/*
   GENERATED FORM FOR THE SECURITYGROUPSECURITYROLE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityGroupSecurityRole table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-group-security-role-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityGroupSecurityRoleService, SecurityGroupSecurityRoleData, SecurityGroupSecurityRoleSubmitData } from '../../../security-data-services/security-group-security-role.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityGroupService } from '../../../security-data-services/security-group.service';
import { SecurityRoleService } from '../../../security-data-services/security-role.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SecurityGroupSecurityRoleFormValues {
  securityGroupId: number | bigint,       // For FK link number
  securityRoleId: number | bigint,       // For FK link number
  comments: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-security-group-security-role-add-edit',
  templateUrl: './security-group-security-role-add-edit.component.html',
  styleUrls: ['./security-group-security-role-add-edit.component.scss']
})
export class SecurityGroupSecurityRoleAddEditComponent {
  @ViewChild('securityGroupSecurityRoleModal') securityGroupSecurityRoleModal!: TemplateRef<any>;
  @Output() securityGroupSecurityRoleChanged = new Subject<SecurityGroupSecurityRoleData[]>();
  @Input() securityGroupSecurityRoleSubmitData: SecurityGroupSecurityRoleSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityGroupSecurityRoleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityGroupSecurityRoleForm: FormGroup = this.fb.group({
        securityGroupId: [null, Validators.required],
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

  securityGroupSecurityRoles$ = this.securityGroupSecurityRoleService.GetSecurityGroupSecurityRoleList();
  securityGroups$ = this.securityGroupService.GetSecurityGroupList();
  securityRoles$ = this.securityRoleService.GetSecurityRoleList();

  constructor(
    private modalService: NgbModal,
    private securityGroupSecurityRoleService: SecurityGroupSecurityRoleService,
    private securityGroupService: SecurityGroupService,
    private securityRoleService: SecurityRoleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityGroupSecurityRoleData?: SecurityGroupSecurityRoleData) {

    if (securityGroupSecurityRoleData != null) {

      if (!this.securityGroupSecurityRoleService.userIsSecuritySecurityGroupSecurityRoleReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Group Security Roles`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityGroupSecurityRoleSubmitData = this.securityGroupSecurityRoleService.ConvertToSecurityGroupSecurityRoleSubmitData(securityGroupSecurityRoleData);
      this.isEditMode = true;

      this.buildFormValues(securityGroupSecurityRoleData);

    } else {

      if (!this.securityGroupSecurityRoleService.userIsSecuritySecurityGroupSecurityRoleWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Group Security Roles`,
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
        this.securityGroupSecurityRoleForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityGroupSecurityRoleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.securityGroupSecurityRoleModal, {
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

    if (this.securityGroupSecurityRoleService.userIsSecuritySecurityGroupSecurityRoleWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Group Security Roles`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityGroupSecurityRoleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityGroupSecurityRoleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityGroupSecurityRoleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityGroupSecurityRoleSubmitData: SecurityGroupSecurityRoleSubmitData = {
        id: this.securityGroupSecurityRoleSubmitData?.id || 0,
        securityGroupId: Number(formValue.securityGroupId),
        securityRoleId: Number(formValue.securityRoleId),
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityGroupSecurityRole(securityGroupSecurityRoleSubmitData);
      } else {
        this.addSecurityGroupSecurityRole(securityGroupSecurityRoleSubmitData);
      }
  }

  private addSecurityGroupSecurityRole(securityGroupSecurityRoleData: SecurityGroupSecurityRoleSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityGroupSecurityRoleData.active = true;
    securityGroupSecurityRoleData.deleted = false;
    this.securityGroupSecurityRoleService.PostSecurityGroupSecurityRole(securityGroupSecurityRoleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityGroupSecurityRole) => {

        this.securityGroupSecurityRoleService.ClearAllCaches();

        this.securityGroupSecurityRoleChanged.next([newSecurityGroupSecurityRole]);

        this.alertService.showMessage("Security Group Security Role added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securitygroupsecurityrole', newSecurityGroupSecurityRole.id]);
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
                                   'You do not have permission to save this Security Group Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Group Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Group Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityGroupSecurityRole(securityGroupSecurityRoleData: SecurityGroupSecurityRoleSubmitData) {
    this.securityGroupSecurityRoleService.PutSecurityGroupSecurityRole(securityGroupSecurityRoleData.id, securityGroupSecurityRoleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityGroupSecurityRole) => {

        this.securityGroupSecurityRoleService.ClearAllCaches();

        this.securityGroupSecurityRoleChanged.next([updatedSecurityGroupSecurityRole]);

        this.alertService.showMessage("Security Group Security Role updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security Group Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Group Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Group Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityGroupSecurityRoleData: SecurityGroupSecurityRoleData | null) {

    if (securityGroupSecurityRoleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityGroupSecurityRoleForm.reset({
        securityGroupId: null,
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
        this.securityGroupSecurityRoleForm.reset({
        securityGroupId: securityGroupSecurityRoleData.securityGroupId,
        securityRoleId: securityGroupSecurityRoleData.securityRoleId,
        comments: securityGroupSecurityRoleData.comments ?? '',
        active: securityGroupSecurityRoleData.active ?? true,
        deleted: securityGroupSecurityRoleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityGroupSecurityRoleForm.markAsPristine();
    this.securityGroupSecurityRoleForm.markAsUntouched();
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


  public userIsSecuritySecurityGroupSecurityRoleReader(): boolean {
    return this.securityGroupSecurityRoleService.userIsSecuritySecurityGroupSecurityRoleReader();
  }

  public userIsSecuritySecurityGroupSecurityRoleWriter(): boolean {
    return this.securityGroupSecurityRoleService.userIsSecuritySecurityGroupSecurityRoleWriter();
  }
}
