/*
   GENERATED FORM FOR THE BUILDSTEPANNOTATIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildStepAnnotationType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-step-annotation-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildStepAnnotationTypeService, BuildStepAnnotationTypeData, BuildStepAnnotationTypeSubmitData } from '../../../bmc-data-services/build-step-annotation-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BuildStepAnnotationTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-build-step-annotation-type-add-edit',
  templateUrl: './build-step-annotation-type-add-edit.component.html',
  styleUrls: ['./build-step-annotation-type-add-edit.component.scss']
})
export class BuildStepAnnotationTypeAddEditComponent {
  @ViewChild('buildStepAnnotationTypeModal') buildStepAnnotationTypeModal!: TemplateRef<any>;
  @Output() buildStepAnnotationTypeChanged = new Subject<BuildStepAnnotationTypeData[]>();
  @Input() buildStepAnnotationTypeSubmitData: BuildStepAnnotationTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildStepAnnotationTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildStepAnnotationTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  buildStepAnnotationTypes$ = this.buildStepAnnotationTypeService.GetBuildStepAnnotationTypeList();

  constructor(
    private modalService: NgbModal,
    private buildStepAnnotationTypeService: BuildStepAnnotationTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(buildStepAnnotationTypeData?: BuildStepAnnotationTypeData) {

    if (buildStepAnnotationTypeData != null) {

      if (!this.buildStepAnnotationTypeService.userIsBMCBuildStepAnnotationTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Build Step Annotation Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.buildStepAnnotationTypeSubmitData = this.buildStepAnnotationTypeService.ConvertToBuildStepAnnotationTypeSubmitData(buildStepAnnotationTypeData);
      this.isEditMode = true;
      this.objectGuid = buildStepAnnotationTypeData.objectGuid;

      this.buildFormValues(buildStepAnnotationTypeData);

    } else {

      if (!this.buildStepAnnotationTypeService.userIsBMCBuildStepAnnotationTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Build Step Annotation Types`,
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
        this.buildStepAnnotationTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildStepAnnotationTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.buildStepAnnotationTypeModal, {
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

    if (this.buildStepAnnotationTypeService.userIsBMCBuildStepAnnotationTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Build Step Annotation Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.buildStepAnnotationTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildStepAnnotationTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildStepAnnotationTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildStepAnnotationTypeSubmitData: BuildStepAnnotationTypeSubmitData = {
        id: this.buildStepAnnotationTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBuildStepAnnotationType(buildStepAnnotationTypeSubmitData);
      } else {
        this.addBuildStepAnnotationType(buildStepAnnotationTypeSubmitData);
      }
  }

  private addBuildStepAnnotationType(buildStepAnnotationTypeData: BuildStepAnnotationTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    buildStepAnnotationTypeData.active = true;
    buildStepAnnotationTypeData.deleted = false;
    this.buildStepAnnotationTypeService.PostBuildStepAnnotationType(buildStepAnnotationTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBuildStepAnnotationType) => {

        this.buildStepAnnotationTypeService.ClearAllCaches();

        this.buildStepAnnotationTypeChanged.next([newBuildStepAnnotationType]);

        this.alertService.showMessage("Build Step Annotation Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/buildstepannotationtype', newBuildStepAnnotationType.id]);
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
                                   'You do not have permission to save this Build Step Annotation Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Annotation Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Annotation Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBuildStepAnnotationType(buildStepAnnotationTypeData: BuildStepAnnotationTypeSubmitData) {
    this.buildStepAnnotationTypeService.PutBuildStepAnnotationType(buildStepAnnotationTypeData.id, buildStepAnnotationTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBuildStepAnnotationType) => {

        this.buildStepAnnotationTypeService.ClearAllCaches();

        this.buildStepAnnotationTypeChanged.next([updatedBuildStepAnnotationType]);

        this.alertService.showMessage("Build Step Annotation Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Build Step Annotation Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Annotation Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Annotation Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(buildStepAnnotationTypeData: BuildStepAnnotationTypeData | null) {

    if (buildStepAnnotationTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildStepAnnotationTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildStepAnnotationTypeForm.reset({
        name: buildStepAnnotationTypeData.name ?? '',
        description: buildStepAnnotationTypeData.description ?? '',
        sequence: buildStepAnnotationTypeData.sequence?.toString() ?? '',
        active: buildStepAnnotationTypeData.active ?? true,
        deleted: buildStepAnnotationTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildStepAnnotationTypeForm.markAsPristine();
    this.buildStepAnnotationTypeForm.markAsUntouched();
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


  public userIsBMCBuildStepAnnotationTypeReader(): boolean {
    return this.buildStepAnnotationTypeService.userIsBMCBuildStepAnnotationTypeReader();
  }

  public userIsBMCBuildStepAnnotationTypeWriter(): boolean {
    return this.buildStepAnnotationTypeService.userIsBMCBuildStepAnnotationTypeWriter();
  }
}
