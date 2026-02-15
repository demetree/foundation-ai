/*
   GENERATED FORM FOR THE BUILDSTEPANNOTATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildStepAnnotation table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-step-annotation-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildStepAnnotationService, BuildStepAnnotationData, BuildStepAnnotationSubmitData } from '../../../bmc-data-services/build-step-annotation.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BuildManualStepService } from '../../../bmc-data-services/build-manual-step.service';
import { BuildStepAnnotationTypeService } from '../../../bmc-data-services/build-step-annotation-type.service';
import { PlacedBrickService } from '../../../bmc-data-services/placed-brick.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BuildStepAnnotationFormValues {
  buildManualStepId: number | bigint,       // For FK link number
  buildStepAnnotationTypeId: number | bigint,       // For FK link number
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  width: string | null,     // Stored as string for form input, converted to number on submit.
  height: string | null,     // Stored as string for form input, converted to number on submit.
  text: string | null,
  placedBrickId: number | bigint | null,       // For FK link number
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-build-step-annotation-add-edit',
  templateUrl: './build-step-annotation-add-edit.component.html',
  styleUrls: ['./build-step-annotation-add-edit.component.scss']
})
export class BuildStepAnnotationAddEditComponent {
  @ViewChild('buildStepAnnotationModal') buildStepAnnotationModal!: TemplateRef<any>;
  @Output() buildStepAnnotationChanged = new Subject<BuildStepAnnotationData[]>();
  @Input() buildStepAnnotationSubmitData: BuildStepAnnotationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildStepAnnotationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildStepAnnotationForm: FormGroup = this.fb.group({
        buildManualStepId: [null, Validators.required],
        buildStepAnnotationTypeId: [null, Validators.required],
        positionX: [''],
        positionY: [''],
        width: [''],
        height: [''],
        text: [''],
        placedBrickId: [null],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  buildStepAnnotations$ = this.buildStepAnnotationService.GetBuildStepAnnotationList();
  buildManualSteps$ = this.buildManualStepService.GetBuildManualStepList();
  buildStepAnnotationTypes$ = this.buildStepAnnotationTypeService.GetBuildStepAnnotationTypeList();
  placedBricks$ = this.placedBrickService.GetPlacedBrickList();

  constructor(
    private modalService: NgbModal,
    private buildStepAnnotationService: BuildStepAnnotationService,
    private buildManualStepService: BuildManualStepService,
    private buildStepAnnotationTypeService: BuildStepAnnotationTypeService,
    private placedBrickService: PlacedBrickService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(buildStepAnnotationData?: BuildStepAnnotationData) {

    if (buildStepAnnotationData != null) {

      if (!this.buildStepAnnotationService.userIsBMCBuildStepAnnotationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Build Step Annotations`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.buildStepAnnotationSubmitData = this.buildStepAnnotationService.ConvertToBuildStepAnnotationSubmitData(buildStepAnnotationData);
      this.isEditMode = true;
      this.objectGuid = buildStepAnnotationData.objectGuid;

      this.buildFormValues(buildStepAnnotationData);

    } else {

      if (!this.buildStepAnnotationService.userIsBMCBuildStepAnnotationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Build Step Annotations`,
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
        this.buildStepAnnotationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildStepAnnotationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.buildStepAnnotationModal, {
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

    if (this.buildStepAnnotationService.userIsBMCBuildStepAnnotationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Build Step Annotations`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.buildStepAnnotationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildStepAnnotationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildStepAnnotationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildStepAnnotationSubmitData: BuildStepAnnotationSubmitData = {
        id: this.buildStepAnnotationSubmitData?.id || 0,
        buildManualStepId: Number(formValue.buildManualStepId),
        buildStepAnnotationTypeId: Number(formValue.buildStepAnnotationTypeId),
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        width: formValue.width ? Number(formValue.width) : null,
        height: formValue.height ? Number(formValue.height) : null,
        text: formValue.text?.trim() || null,
        placedBrickId: formValue.placedBrickId ? Number(formValue.placedBrickId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBuildStepAnnotation(buildStepAnnotationSubmitData);
      } else {
        this.addBuildStepAnnotation(buildStepAnnotationSubmitData);
      }
  }

  private addBuildStepAnnotation(buildStepAnnotationData: BuildStepAnnotationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    buildStepAnnotationData.active = true;
    buildStepAnnotationData.deleted = false;
    this.buildStepAnnotationService.PostBuildStepAnnotation(buildStepAnnotationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBuildStepAnnotation) => {

        this.buildStepAnnotationService.ClearAllCaches();

        this.buildStepAnnotationChanged.next([newBuildStepAnnotation]);

        this.alertService.showMessage("Build Step Annotation added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/buildstepannotation', newBuildStepAnnotation.id]);
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
                                   'You do not have permission to save this Build Step Annotation.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Annotation.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Annotation could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBuildStepAnnotation(buildStepAnnotationData: BuildStepAnnotationSubmitData) {
    this.buildStepAnnotationService.PutBuildStepAnnotation(buildStepAnnotationData.id, buildStepAnnotationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBuildStepAnnotation) => {

        this.buildStepAnnotationService.ClearAllCaches();

        this.buildStepAnnotationChanged.next([updatedBuildStepAnnotation]);

        this.alertService.showMessage("Build Step Annotation updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Build Step Annotation.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Annotation.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Annotation could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(buildStepAnnotationData: BuildStepAnnotationData | null) {

    if (buildStepAnnotationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildStepAnnotationForm.reset({
        buildManualStepId: null,
        buildStepAnnotationTypeId: null,
        positionX: '',
        positionY: '',
        width: '',
        height: '',
        text: '',
        placedBrickId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildStepAnnotationForm.reset({
        buildManualStepId: buildStepAnnotationData.buildManualStepId,
        buildStepAnnotationTypeId: buildStepAnnotationData.buildStepAnnotationTypeId,
        positionX: buildStepAnnotationData.positionX?.toString() ?? '',
        positionY: buildStepAnnotationData.positionY?.toString() ?? '',
        width: buildStepAnnotationData.width?.toString() ?? '',
        height: buildStepAnnotationData.height?.toString() ?? '',
        text: buildStepAnnotationData.text ?? '',
        placedBrickId: buildStepAnnotationData.placedBrickId,
        active: buildStepAnnotationData.active ?? true,
        deleted: buildStepAnnotationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildStepAnnotationForm.markAsPristine();
    this.buildStepAnnotationForm.markAsUntouched();
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


  public userIsBMCBuildStepAnnotationReader(): boolean {
    return this.buildStepAnnotationService.userIsBMCBuildStepAnnotationReader();
  }

  public userIsBMCBuildStepAnnotationWriter(): boolean {
    return this.buildStepAnnotationService.userIsBMCBuildStepAnnotationWriter();
  }
}
