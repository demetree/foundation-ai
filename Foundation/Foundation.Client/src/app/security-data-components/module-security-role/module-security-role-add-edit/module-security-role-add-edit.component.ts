/*
   GENERATED FORM FOR THE MODULESECURITYROLE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ModuleSecurityRole table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to module-security-role-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModuleSecurityRoleService, ModuleSecurityRoleData, ModuleSecurityRoleSubmitData } from '../../../security-data-services/module-security-role.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ModuleService } from '../../../security-data-services/module.service';
import { SecurityRoleService } from '../../../security-data-services/security-role.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ModuleSecurityRoleFormValues {
  moduleId: number | bigint,       // For FK link number
  securityRoleId: number | bigint,       // For FK link number
  comments: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-module-security-role-add-edit',
  templateUrl: './module-security-role-add-edit.component.html',
  styleUrls: ['./module-security-role-add-edit.component.scss']
})
export class ModuleSecurityRoleAddEditComponent {
  @ViewChild('moduleSecurityRoleModal') moduleSecurityRoleModal!: TemplateRef<any>;
  @Output() moduleSecurityRoleChanged = new Subject<ModuleSecurityRoleData[]>();
  @Input() moduleSecurityRoleSubmitData: ModuleSecurityRoleSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ModuleSecurityRoleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public moduleSecurityRoleForm: FormGroup = this.fb.group({
        moduleId: [null, Validators.required],
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

  moduleSecurityRoles$ = this.moduleSecurityRoleService.GetModuleSecurityRoleList();
  modules$ = this.moduleService.GetModuleList();
  securityRoles$ = this.securityRoleService.GetSecurityRoleList();

  constructor(
    private modalService: NgbModal,
    private moduleSecurityRoleService: ModuleSecurityRoleService,
    private moduleService: ModuleService,
    private securityRoleService: SecurityRoleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(moduleSecurityRoleData?: ModuleSecurityRoleData) {

    if (moduleSecurityRoleData != null) {

      if (!this.moduleSecurityRoleService.userIsSecurityModuleSecurityRoleReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Module Security Roles`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.moduleSecurityRoleSubmitData = this.moduleSecurityRoleService.ConvertToModuleSecurityRoleSubmitData(moduleSecurityRoleData);
      this.isEditMode = true;

      this.buildFormValues(moduleSecurityRoleData);

    } else {

      if (!this.moduleSecurityRoleService.userIsSecurityModuleSecurityRoleWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Module Security Roles`,
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
        this.moduleSecurityRoleForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.moduleSecurityRoleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.moduleSecurityRoleModal, {
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

    if (this.moduleSecurityRoleService.userIsSecurityModuleSecurityRoleWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Module Security Roles`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.moduleSecurityRoleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.moduleSecurityRoleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.moduleSecurityRoleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const moduleSecurityRoleSubmitData: ModuleSecurityRoleSubmitData = {
        id: this.moduleSecurityRoleSubmitData?.id || 0,
        moduleId: Number(formValue.moduleId),
        securityRoleId: Number(formValue.securityRoleId),
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateModuleSecurityRole(moduleSecurityRoleSubmitData);
      } else {
        this.addModuleSecurityRole(moduleSecurityRoleSubmitData);
      }
  }

  private addModuleSecurityRole(moduleSecurityRoleData: ModuleSecurityRoleSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    moduleSecurityRoleData.active = true;
    moduleSecurityRoleData.deleted = false;
    this.moduleSecurityRoleService.PostModuleSecurityRole(moduleSecurityRoleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newModuleSecurityRole) => {

        this.moduleSecurityRoleService.ClearAllCaches();

        this.moduleSecurityRoleChanged.next([newModuleSecurityRole]);

        this.alertService.showMessage("Module Security Role added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/modulesecurityrole', newModuleSecurityRole.id]);
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
                                   'You do not have permission to save this Module Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Module Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Module Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateModuleSecurityRole(moduleSecurityRoleData: ModuleSecurityRoleSubmitData) {
    this.moduleSecurityRoleService.PutModuleSecurityRole(moduleSecurityRoleData.id, moduleSecurityRoleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedModuleSecurityRole) => {

        this.moduleSecurityRoleService.ClearAllCaches();

        this.moduleSecurityRoleChanged.next([updatedModuleSecurityRole]);

        this.alertService.showMessage("Module Security Role updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Module Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Module Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Module Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(moduleSecurityRoleData: ModuleSecurityRoleData | null) {

    if (moduleSecurityRoleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.moduleSecurityRoleForm.reset({
        moduleId: null,
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
        this.moduleSecurityRoleForm.reset({
        moduleId: moduleSecurityRoleData.moduleId,
        securityRoleId: moduleSecurityRoleData.securityRoleId,
        comments: moduleSecurityRoleData.comments ?? '',
        active: moduleSecurityRoleData.active ?? true,
        deleted: moduleSecurityRoleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.moduleSecurityRoleForm.markAsPristine();
    this.moduleSecurityRoleForm.markAsUntouched();
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


  public userIsSecurityModuleSecurityRoleReader(): boolean {
    return this.moduleSecurityRoleService.userIsSecurityModuleSecurityRoleReader();
  }

  public userIsSecurityModuleSecurityRoleWriter(): boolean {
    return this.moduleSecurityRoleService.userIsSecurityModuleSecurityRoleWriter();
  }
}
