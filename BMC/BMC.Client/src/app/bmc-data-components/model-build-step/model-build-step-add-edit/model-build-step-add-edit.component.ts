/*
   GENERATED FORM FOR THE MODELBUILDSTEP TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ModelBuildStep table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to model-build-step-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModelBuildStepService, ModelBuildStepData, ModelBuildStepSubmitData } from '../../../bmc-data-services/model-build-step.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ModelSubFileService } from '../../../bmc-data-services/model-sub-file.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ModelBuildStepFormValues {
  modelSubFileId: number | bigint,       // For FK link number
  stepNumber: string,     // Stored as string for form input, converted to number on submit.
  rotationType: string | null,
  rotationX: string | null,     // Stored as string for form input, converted to number on submit.
  rotationY: string | null,     // Stored as string for form input, converted to number on submit.
  rotationZ: string | null,     // Stored as string for form input, converted to number on submit.
  description: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-model-build-step-add-edit',
  templateUrl: './model-build-step-add-edit.component.html',
  styleUrls: ['./model-build-step-add-edit.component.scss']
})
export class ModelBuildStepAddEditComponent {
  @ViewChild('modelBuildStepModal') modelBuildStepModal!: TemplateRef<any>;
  @Output() modelBuildStepChanged = new Subject<ModelBuildStepData[]>();
  @Input() modelBuildStepSubmitData: ModelBuildStepSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ModelBuildStepFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public modelBuildStepForm: FormGroup = this.fb.group({
        modelSubFileId: [null, Validators.required],
        stepNumber: ['', Validators.required],
        rotationType: [''],
        rotationX: [''],
        rotationY: [''],
        rotationZ: [''],
        description: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  modelBuildSteps$ = this.modelBuildStepService.GetModelBuildStepList();
  modelSubFiles$ = this.modelSubFileService.GetModelSubFileList();

  constructor(
    private modalService: NgbModal,
    private modelBuildStepService: ModelBuildStepService,
    private modelSubFileService: ModelSubFileService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(modelBuildStepData?: ModelBuildStepData) {

    if (modelBuildStepData != null) {

      if (!this.modelBuildStepService.userIsBMCModelBuildStepReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Model Build Steps`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.modelBuildStepSubmitData = this.modelBuildStepService.ConvertToModelBuildStepSubmitData(modelBuildStepData);
      this.isEditMode = true;
      this.objectGuid = modelBuildStepData.objectGuid;

      this.buildFormValues(modelBuildStepData);

    } else {

      if (!this.modelBuildStepService.userIsBMCModelBuildStepWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Model Build Steps`,
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
        this.modelBuildStepForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.modelBuildStepForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.modelBuildStepModal, {
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

    if (this.modelBuildStepService.userIsBMCModelBuildStepWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Model Build Steps`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.modelBuildStepForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.modelBuildStepForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.modelBuildStepForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const modelBuildStepSubmitData: ModelBuildStepSubmitData = {
        id: this.modelBuildStepSubmitData?.id || 0,
        modelSubFileId: Number(formValue.modelSubFileId),
        stepNumber: Number(formValue.stepNumber),
        rotationType: formValue.rotationType?.trim() || null,
        rotationX: formValue.rotationX ? Number(formValue.rotationX) : null,
        rotationY: formValue.rotationY ? Number(formValue.rotationY) : null,
        rotationZ: formValue.rotationZ ? Number(formValue.rotationZ) : null,
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateModelBuildStep(modelBuildStepSubmitData);
      } else {
        this.addModelBuildStep(modelBuildStepSubmitData);
      }
  }

  private addModelBuildStep(modelBuildStepData: ModelBuildStepSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    modelBuildStepData.active = true;
    modelBuildStepData.deleted = false;
    this.modelBuildStepService.PostModelBuildStep(modelBuildStepData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newModelBuildStep) => {

        this.modelBuildStepService.ClearAllCaches();

        this.modelBuildStepChanged.next([newModelBuildStep]);

        this.alertService.showMessage("Model Build Step added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/modelbuildstep', newModelBuildStep.id]);
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
                                   'You do not have permission to save this Model Build Step.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Build Step.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Build Step could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateModelBuildStep(modelBuildStepData: ModelBuildStepSubmitData) {
    this.modelBuildStepService.PutModelBuildStep(modelBuildStepData.id, modelBuildStepData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedModelBuildStep) => {

        this.modelBuildStepService.ClearAllCaches();

        this.modelBuildStepChanged.next([updatedModelBuildStep]);

        this.alertService.showMessage("Model Build Step updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Model Build Step.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Build Step.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Build Step could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(modelBuildStepData: ModelBuildStepData | null) {

    if (modelBuildStepData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.modelBuildStepForm.reset({
        modelSubFileId: null,
        stepNumber: '',
        rotationType: '',
        rotationX: '',
        rotationY: '',
        rotationZ: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.modelBuildStepForm.reset({
        modelSubFileId: modelBuildStepData.modelSubFileId,
        stepNumber: modelBuildStepData.stepNumber?.toString() ?? '',
        rotationType: modelBuildStepData.rotationType ?? '',
        rotationX: modelBuildStepData.rotationX?.toString() ?? '',
        rotationY: modelBuildStepData.rotationY?.toString() ?? '',
        rotationZ: modelBuildStepData.rotationZ?.toString() ?? '',
        description: modelBuildStepData.description ?? '',
        active: modelBuildStepData.active ?? true,
        deleted: modelBuildStepData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.modelBuildStepForm.markAsPristine();
    this.modelBuildStepForm.markAsUntouched();
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


  public userIsBMCModelBuildStepReader(): boolean {
    return this.modelBuildStepService.userIsBMCModelBuildStepReader();
  }

  public userIsBMCModelBuildStepWriter(): boolean {
    return this.modelBuildStepService.userIsBMCModelBuildStepWriter();
  }
}
