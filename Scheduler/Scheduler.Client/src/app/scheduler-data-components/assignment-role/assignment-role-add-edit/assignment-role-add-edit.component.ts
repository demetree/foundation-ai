/*
   GENERATED FORM FOR THE ASSIGNMENTROLE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AssignmentRole table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to assignment-role-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AssignmentRoleService, AssignmentRoleData, AssignmentRoleSubmitData } from '../../../scheduler-data-services/assignment-role.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AssignmentRoleFormValues {
  name: string,
  description: string | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-assignment-role-add-edit',
  templateUrl: './assignment-role-add-edit.component.html',
  styleUrls: ['./assignment-role-add-edit.component.scss']
})
export class AssignmentRoleAddEditComponent {
  @ViewChild('assignmentRoleModal') assignmentRoleModal!: TemplateRef<any>;
  @Output() assignmentRoleChanged = new Subject<AssignmentRoleData[]>();
  @Input() assignmentRoleSubmitData: AssignmentRoleSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AssignmentRoleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public assignmentRoleForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        iconId: [null],
        color: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private assignmentRoleService: AssignmentRoleService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(assignmentRoleData?: AssignmentRoleData) {

    if (assignmentRoleData != null) {

      if (!this.assignmentRoleService.userIsSchedulerAssignmentRoleReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Assignment Roles`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.assignmentRoleSubmitData = this.assignmentRoleService.ConvertToAssignmentRoleSubmitData(assignmentRoleData);
      this.isEditMode = true;
      this.objectGuid = assignmentRoleData.objectGuid;

      this.buildFormValues(assignmentRoleData);

    } else {

      if (!this.assignmentRoleService.userIsSchedulerAssignmentRoleWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Assignment Roles`,
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
        this.assignmentRoleForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.assignmentRoleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.assignmentRoleModal, {
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

    if (this.assignmentRoleService.userIsSchedulerAssignmentRoleWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Assignment Roles`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.assignmentRoleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.assignmentRoleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.assignmentRoleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const assignmentRoleSubmitData: AssignmentRoleSubmitData = {
        id: this.assignmentRoleSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateAssignmentRole(assignmentRoleSubmitData);
      } else {
        this.addAssignmentRole(assignmentRoleSubmitData);
      }
  }

  private addAssignmentRole(assignmentRoleData: AssignmentRoleSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    assignmentRoleData.active = true;
    assignmentRoleData.deleted = false;
    this.assignmentRoleService.PostAssignmentRole(assignmentRoleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAssignmentRole) => {

        this.assignmentRoleService.ClearAllCaches();

        this.assignmentRoleChanged.next([newAssignmentRole]);

        this.alertService.showMessage("Assignment Role added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/assignmentrole', newAssignmentRole.id]);
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
                                   'You do not have permission to save this Assignment Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Assignment Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Assignment Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAssignmentRole(assignmentRoleData: AssignmentRoleSubmitData) {
    this.assignmentRoleService.PutAssignmentRole(assignmentRoleData.id, assignmentRoleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAssignmentRole) => {

        this.assignmentRoleService.ClearAllCaches();

        this.assignmentRoleChanged.next([updatedAssignmentRole]);

        this.alertService.showMessage("Assignment Role updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Assignment Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Assignment Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Assignment Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(assignmentRoleData: AssignmentRoleData | null) {

    if (assignmentRoleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.assignmentRoleForm.reset({
        name: '',
        description: '',
        iconId: null,
        color: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.assignmentRoleForm.reset({
        name: assignmentRoleData.name ?? '',
        description: assignmentRoleData.description ?? '',
        iconId: assignmentRoleData.iconId,
        color: assignmentRoleData.color ?? '',
        sequence: assignmentRoleData.sequence?.toString() ?? '',
        active: assignmentRoleData.active ?? true,
        deleted: assignmentRoleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.assignmentRoleForm.markAsPristine();
    this.assignmentRoleForm.markAsUntouched();
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


  public userIsSchedulerAssignmentRoleReader(): boolean {
    return this.assignmentRoleService.userIsSchedulerAssignmentRoleReader();
  }

  public userIsSchedulerAssignmentRoleWriter(): boolean {
    return this.assignmentRoleService.userIsSchedulerAssignmentRoleWriter();
  }
}
