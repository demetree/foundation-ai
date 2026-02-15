/*
   GENERATED FORM FOR THE BUILDMANUALSTEP TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildManualStep table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-manual-step-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildManualStepService, BuildManualStepData, BuildManualStepSubmitData } from '../../../bmc-data-services/build-manual-step.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BuildManualPageService } from '../../../bmc-data-services/build-manual-page.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BuildManualStepFormValues {
  buildManualPageId: number | bigint,       // For FK link number
  stepNumber: string | null,     // Stored as string for form input, converted to number on submit.
  cameraPositionX: string | null,     // Stored as string for form input, converted to number on submit.
  cameraPositionY: string | null,     // Stored as string for form input, converted to number on submit.
  cameraPositionZ: string | null,     // Stored as string for form input, converted to number on submit.
  cameraTargetX: string | null,     // Stored as string for form input, converted to number on submit.
  cameraTargetY: string | null,     // Stored as string for form input, converted to number on submit.
  cameraTargetZ: string | null,     // Stored as string for form input, converted to number on submit.
  cameraZoom: string | null,     // Stored as string for form input, converted to number on submit.
  showExplodedView: boolean,
  explodedDistance: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-build-manual-step-add-edit',
  templateUrl: './build-manual-step-add-edit.component.html',
  styleUrls: ['./build-manual-step-add-edit.component.scss']
})
export class BuildManualStepAddEditComponent {
  @ViewChild('buildManualStepModal') buildManualStepModal!: TemplateRef<any>;
  @Output() buildManualStepChanged = new Subject<BuildManualStepData[]>();
  @Input() buildManualStepSubmitData: BuildManualStepSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildManualStepFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildManualStepForm: FormGroup = this.fb.group({
        buildManualPageId: [null, Validators.required],
        stepNumber: [''],
        cameraPositionX: [''],
        cameraPositionY: [''],
        cameraPositionZ: [''],
        cameraTargetX: [''],
        cameraTargetY: [''],
        cameraTargetZ: [''],
        cameraZoom: [''],
        showExplodedView: [false],
        explodedDistance: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  buildManualSteps$ = this.buildManualStepService.GetBuildManualStepList();
  buildManualPages$ = this.buildManualPageService.GetBuildManualPageList();

  constructor(
    private modalService: NgbModal,
    private buildManualStepService: BuildManualStepService,
    private buildManualPageService: BuildManualPageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(buildManualStepData?: BuildManualStepData) {

    if (buildManualStepData != null) {

      if (!this.buildManualStepService.userIsBMCBuildManualStepReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Build Manual Steps`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.buildManualStepSubmitData = this.buildManualStepService.ConvertToBuildManualStepSubmitData(buildManualStepData);
      this.isEditMode = true;
      this.objectGuid = buildManualStepData.objectGuid;

      this.buildFormValues(buildManualStepData);

    } else {

      if (!this.buildManualStepService.userIsBMCBuildManualStepWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Build Manual Steps`,
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
        this.buildManualStepForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildManualStepForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.buildManualStepModal, {
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

    if (this.buildManualStepService.userIsBMCBuildManualStepWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Build Manual Steps`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.buildManualStepForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildManualStepForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildManualStepForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildManualStepSubmitData: BuildManualStepSubmitData = {
        id: this.buildManualStepSubmitData?.id || 0,
        buildManualPageId: Number(formValue.buildManualPageId),
        stepNumber: formValue.stepNumber ? Number(formValue.stepNumber) : null,
        cameraPositionX: formValue.cameraPositionX ? Number(formValue.cameraPositionX) : null,
        cameraPositionY: formValue.cameraPositionY ? Number(formValue.cameraPositionY) : null,
        cameraPositionZ: formValue.cameraPositionZ ? Number(formValue.cameraPositionZ) : null,
        cameraTargetX: formValue.cameraTargetX ? Number(formValue.cameraTargetX) : null,
        cameraTargetY: formValue.cameraTargetY ? Number(formValue.cameraTargetY) : null,
        cameraTargetZ: formValue.cameraTargetZ ? Number(formValue.cameraTargetZ) : null,
        cameraZoom: formValue.cameraZoom ? Number(formValue.cameraZoom) : null,
        showExplodedView: !!formValue.showExplodedView,
        explodedDistance: formValue.explodedDistance ? Number(formValue.explodedDistance) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBuildManualStep(buildManualStepSubmitData);
      } else {
        this.addBuildManualStep(buildManualStepSubmitData);
      }
  }

  private addBuildManualStep(buildManualStepData: BuildManualStepSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    buildManualStepData.active = true;
    buildManualStepData.deleted = false;
    this.buildManualStepService.PostBuildManualStep(buildManualStepData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBuildManualStep) => {

        this.buildManualStepService.ClearAllCaches();

        this.buildManualStepChanged.next([newBuildManualStep]);

        this.alertService.showMessage("Build Manual Step added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/buildmanualstep', newBuildManualStep.id]);
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
                                   'You do not have permission to save this Build Manual Step.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual Step.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual Step could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBuildManualStep(buildManualStepData: BuildManualStepSubmitData) {
    this.buildManualStepService.PutBuildManualStep(buildManualStepData.id, buildManualStepData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBuildManualStep) => {

        this.buildManualStepService.ClearAllCaches();

        this.buildManualStepChanged.next([updatedBuildManualStep]);

        this.alertService.showMessage("Build Manual Step updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Build Manual Step.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual Step.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual Step could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(buildManualStepData: BuildManualStepData | null) {

    if (buildManualStepData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildManualStepForm.reset({
        buildManualPageId: null,
        stepNumber: '',
        cameraPositionX: '',
        cameraPositionY: '',
        cameraPositionZ: '',
        cameraTargetX: '',
        cameraTargetY: '',
        cameraTargetZ: '',
        cameraZoom: '',
        showExplodedView: false,
        explodedDistance: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildManualStepForm.reset({
        buildManualPageId: buildManualStepData.buildManualPageId,
        stepNumber: buildManualStepData.stepNumber?.toString() ?? '',
        cameraPositionX: buildManualStepData.cameraPositionX?.toString() ?? '',
        cameraPositionY: buildManualStepData.cameraPositionY?.toString() ?? '',
        cameraPositionZ: buildManualStepData.cameraPositionZ?.toString() ?? '',
        cameraTargetX: buildManualStepData.cameraTargetX?.toString() ?? '',
        cameraTargetY: buildManualStepData.cameraTargetY?.toString() ?? '',
        cameraTargetZ: buildManualStepData.cameraTargetZ?.toString() ?? '',
        cameraZoom: buildManualStepData.cameraZoom?.toString() ?? '',
        showExplodedView: buildManualStepData.showExplodedView ?? false,
        explodedDistance: buildManualStepData.explodedDistance?.toString() ?? '',
        active: buildManualStepData.active ?? true,
        deleted: buildManualStepData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildManualStepForm.markAsPristine();
    this.buildManualStepForm.markAsUntouched();
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


  public userIsBMCBuildManualStepReader(): boolean {
    return this.buildManualStepService.userIsBMCBuildManualStepReader();
  }

  public userIsBMCBuildManualStepWriter(): boolean {
    return this.buildManualStepService.userIsBMCBuildManualStepWriter();
  }
}
