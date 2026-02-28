/*
   GENERATED FORM FOR THE MODELSTEPPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ModelStepPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to model-step-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModelStepPartService, ModelStepPartData, ModelStepPartSubmitData } from '../../../bmc-data-services/model-step-part.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ModelBuildStepService } from '../../../bmc-data-services/model-build-step.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { BrickColourService } from '../../../bmc-data-services/brick-colour.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ModelStepPartFormValues {
  modelBuildStepId: number | bigint,       // For FK link number
  brickPartId: number | bigint | null,       // For FK link number
  brickColourId: number | bigint | null,       // For FK link number
  partFileName: string,
  colorCode: string,     // Stored as string for form input, converted to number on submit.
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  transformMatrix: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-model-step-part-add-edit',
  templateUrl: './model-step-part-add-edit.component.html',
  styleUrls: ['./model-step-part-add-edit.component.scss']
})
export class ModelStepPartAddEditComponent {
  @ViewChild('modelStepPartModal') modelStepPartModal!: TemplateRef<any>;
  @Output() modelStepPartChanged = new Subject<ModelStepPartData[]>();
  @Input() modelStepPartSubmitData: ModelStepPartSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ModelStepPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public modelStepPartForm: FormGroup = this.fb.group({
        modelBuildStepId: [null, Validators.required],
        brickPartId: [null],
        brickColourId: [null],
        partFileName: ['', Validators.required],
        colorCode: ['', Validators.required],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        transformMatrix: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  modelStepParts$ = this.modelStepPartService.GetModelStepPartList();
  modelBuildSteps$ = this.modelBuildStepService.GetModelBuildStepList();
  brickParts$ = this.brickPartService.GetBrickPartList();
  brickColours$ = this.brickColourService.GetBrickColourList();

  constructor(
    private modalService: NgbModal,
    private modelStepPartService: ModelStepPartService,
    private modelBuildStepService: ModelBuildStepService,
    private brickPartService: BrickPartService,
    private brickColourService: BrickColourService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(modelStepPartData?: ModelStepPartData) {

    if (modelStepPartData != null) {

      if (!this.modelStepPartService.userIsBMCModelStepPartReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Model Step Parts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.modelStepPartSubmitData = this.modelStepPartService.ConvertToModelStepPartSubmitData(modelStepPartData);
      this.isEditMode = true;
      this.objectGuid = modelStepPartData.objectGuid;

      this.buildFormValues(modelStepPartData);

    } else {

      if (!this.modelStepPartService.userIsBMCModelStepPartWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Model Step Parts`,
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
        this.modelStepPartForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.modelStepPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.modelStepPartModal, {
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

    if (this.modelStepPartService.userIsBMCModelStepPartWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Model Step Parts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.modelStepPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.modelStepPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.modelStepPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const modelStepPartSubmitData: ModelStepPartSubmitData = {
        id: this.modelStepPartSubmitData?.id || 0,
        modelBuildStepId: Number(formValue.modelBuildStepId),
        brickPartId: formValue.brickPartId ? Number(formValue.brickPartId) : null,
        brickColourId: formValue.brickColourId ? Number(formValue.brickColourId) : null,
        partFileName: formValue.partFileName!.trim(),
        colorCode: Number(formValue.colorCode),
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        transformMatrix: formValue.transformMatrix!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateModelStepPart(modelStepPartSubmitData);
      } else {
        this.addModelStepPart(modelStepPartSubmitData);
      }
  }

  private addModelStepPart(modelStepPartData: ModelStepPartSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    modelStepPartData.active = true;
    modelStepPartData.deleted = false;
    this.modelStepPartService.PostModelStepPart(modelStepPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newModelStepPart) => {

        this.modelStepPartService.ClearAllCaches();

        this.modelStepPartChanged.next([newModelStepPart]);

        this.alertService.showMessage("Model Step Part added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/modelsteppart', newModelStepPart.id]);
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
                                   'You do not have permission to save this Model Step Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Step Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Step Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateModelStepPart(modelStepPartData: ModelStepPartSubmitData) {
    this.modelStepPartService.PutModelStepPart(modelStepPartData.id, modelStepPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedModelStepPart) => {

        this.modelStepPartService.ClearAllCaches();

        this.modelStepPartChanged.next([updatedModelStepPart]);

        this.alertService.showMessage("Model Step Part updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Model Step Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Step Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Step Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(modelStepPartData: ModelStepPartData | null) {

    if (modelStepPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.modelStepPartForm.reset({
        modelBuildStepId: null,
        brickPartId: null,
        brickColourId: null,
        partFileName: '',
        colorCode: '',
        positionX: '',
        positionY: '',
        positionZ: '',
        transformMatrix: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.modelStepPartForm.reset({
        modelBuildStepId: modelStepPartData.modelBuildStepId,
        brickPartId: modelStepPartData.brickPartId,
        brickColourId: modelStepPartData.brickColourId,
        partFileName: modelStepPartData.partFileName ?? '',
        colorCode: modelStepPartData.colorCode?.toString() ?? '',
        positionX: modelStepPartData.positionX?.toString() ?? '',
        positionY: modelStepPartData.positionY?.toString() ?? '',
        positionZ: modelStepPartData.positionZ?.toString() ?? '',
        transformMatrix: modelStepPartData.transformMatrix ?? '',
        sequence: modelStepPartData.sequence?.toString() ?? '',
        active: modelStepPartData.active ?? true,
        deleted: modelStepPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.modelStepPartForm.markAsPristine();
    this.modelStepPartForm.markAsUntouched();
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


  public userIsBMCModelStepPartReader(): boolean {
    return this.modelStepPartService.userIsBMCModelStepPartReader();
  }

  public userIsBMCModelStepPartWriter(): boolean {
    return this.modelStepPartService.userIsBMCModelStepPartWriter();
  }
}
