/*
   GENERATED FORM FOR THE DEPENDENCYTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DependencyType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to dependency-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DependencyTypeService, DependencyTypeData, DependencyTypeSubmitData } from '../../../scheduler-data-services/dependency-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface DependencyTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-dependency-type-add-edit',
  templateUrl: './dependency-type-add-edit.component.html',
  styleUrls: ['./dependency-type-add-edit.component.scss']
})
export class DependencyTypeAddEditComponent {
  @ViewChild('dependencyTypeModal') dependencyTypeModal!: TemplateRef<any>;
  @Output() dependencyTypeChanged = new Subject<DependencyTypeData[]>();
  @Input() dependencyTypeSubmitData: DependencyTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DependencyTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public dependencyTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        color: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  dependencyTypes$ = this.dependencyTypeService.GetDependencyTypeList();

  constructor(
    private modalService: NgbModal,
    private dependencyTypeService: DependencyTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(dependencyTypeData?: DependencyTypeData) {

    if (dependencyTypeData != null) {

      if (!this.dependencyTypeService.userIsSchedulerDependencyTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Dependency Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.dependencyTypeSubmitData = this.dependencyTypeService.ConvertToDependencyTypeSubmitData(dependencyTypeData);
      this.isEditMode = true;
      this.objectGuid = dependencyTypeData.objectGuid;

      this.buildFormValues(dependencyTypeData);

    } else {

      if (!this.dependencyTypeService.userIsSchedulerDependencyTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Dependency Types`,
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
        this.dependencyTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.dependencyTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.dependencyTypeModal, {
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

    if (this.dependencyTypeService.userIsSchedulerDependencyTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Dependency Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.dependencyTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.dependencyTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.dependencyTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const dependencyTypeSubmitData: DependencyTypeSubmitData = {
        id: this.dependencyTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateDependencyType(dependencyTypeSubmitData);
      } else {
        this.addDependencyType(dependencyTypeSubmitData);
      }
  }

  private addDependencyType(dependencyTypeData: DependencyTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    dependencyTypeData.active = true;
    dependencyTypeData.deleted = false;
    this.dependencyTypeService.PostDependencyType(dependencyTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newDependencyType) => {

        this.dependencyTypeService.ClearAllCaches();

        this.dependencyTypeChanged.next([newDependencyType]);

        this.alertService.showMessage("Dependency Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/dependencytype', newDependencyType.id]);
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
                                   'You do not have permission to save this Dependency Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Dependency Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Dependency Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateDependencyType(dependencyTypeData: DependencyTypeSubmitData) {
    this.dependencyTypeService.PutDependencyType(dependencyTypeData.id, dependencyTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedDependencyType) => {

        this.dependencyTypeService.ClearAllCaches();

        this.dependencyTypeChanged.next([updatedDependencyType]);

        this.alertService.showMessage("Dependency Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Dependency Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Dependency Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Dependency Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(dependencyTypeData: DependencyTypeData | null) {

    if (dependencyTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.dependencyTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.dependencyTypeForm.reset({
        name: dependencyTypeData.name ?? '',
        description: dependencyTypeData.description ?? '',
        sequence: dependencyTypeData.sequence?.toString() ?? '',
        color: dependencyTypeData.color ?? '',
        active: dependencyTypeData.active ?? true,
        deleted: dependencyTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.dependencyTypeForm.markAsPristine();
    this.dependencyTypeForm.markAsUntouched();
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


  public userIsSchedulerDependencyTypeReader(): boolean {
    return this.dependencyTypeService.userIsSchedulerDependencyTypeReader();
  }

  public userIsSchedulerDependencyTypeWriter(): boolean {
    return this.dependencyTypeService.userIsSchedulerDependencyTypeWriter();
  }
}
