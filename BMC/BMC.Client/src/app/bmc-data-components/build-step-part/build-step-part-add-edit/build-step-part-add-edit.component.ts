/*
   GENERATED FORM FOR THE BUILDSTEPPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildStepPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-step-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildStepPartService, BuildStepPartData, BuildStepPartSubmitData } from '../../../bmc-data-services/build-step-part.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BuildManualStepService } from '../../../bmc-data-services/build-manual-step.service';
import { PlacedBrickService } from '../../../bmc-data-services/placed-brick.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BuildStepPartFormValues {
  buildManualStepId: number | bigint,       // For FK link number
  placedBrickId: number | bigint,       // For FK link number
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-build-step-part-add-edit',
  templateUrl: './build-step-part-add-edit.component.html',
  styleUrls: ['./build-step-part-add-edit.component.scss']
})
export class BuildStepPartAddEditComponent {
  @ViewChild('buildStepPartModal') buildStepPartModal!: TemplateRef<any>;
  @Output() buildStepPartChanged = new Subject<BuildStepPartData[]>();
  @Input() buildStepPartSubmitData: BuildStepPartSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildStepPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildStepPartForm: FormGroup = this.fb.group({
        buildManualStepId: [null, Validators.required],
        placedBrickId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  buildStepParts$ = this.buildStepPartService.GetBuildStepPartList();
  buildManualSteps$ = this.buildManualStepService.GetBuildManualStepList();
  placedBricks$ = this.placedBrickService.GetPlacedBrickList();

  constructor(
    private modalService: NgbModal,
    private buildStepPartService: BuildStepPartService,
    private buildManualStepService: BuildManualStepService,
    private placedBrickService: PlacedBrickService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(buildStepPartData?: BuildStepPartData) {

    if (buildStepPartData != null) {

      if (!this.buildStepPartService.userIsBMCBuildStepPartReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Build Step Parts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.buildStepPartSubmitData = this.buildStepPartService.ConvertToBuildStepPartSubmitData(buildStepPartData);
      this.isEditMode = true;
      this.objectGuid = buildStepPartData.objectGuid;

      this.buildFormValues(buildStepPartData);

    } else {

      if (!this.buildStepPartService.userIsBMCBuildStepPartWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Build Step Parts`,
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
        this.buildStepPartForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildStepPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.buildStepPartModal, {
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

    if (this.buildStepPartService.userIsBMCBuildStepPartWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Build Step Parts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.buildStepPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildStepPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildStepPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildStepPartSubmitData: BuildStepPartSubmitData = {
        id: this.buildStepPartSubmitData?.id || 0,
        buildManualStepId: Number(formValue.buildManualStepId),
        placedBrickId: Number(formValue.placedBrickId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBuildStepPart(buildStepPartSubmitData);
      } else {
        this.addBuildStepPart(buildStepPartSubmitData);
      }
  }

  private addBuildStepPart(buildStepPartData: BuildStepPartSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    buildStepPartData.active = true;
    buildStepPartData.deleted = false;
    this.buildStepPartService.PostBuildStepPart(buildStepPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBuildStepPart) => {

        this.buildStepPartService.ClearAllCaches();

        this.buildStepPartChanged.next([newBuildStepPart]);

        this.alertService.showMessage("Build Step Part added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/buildsteppart', newBuildStepPart.id]);
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
                                   'You do not have permission to save this Build Step Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBuildStepPart(buildStepPartData: BuildStepPartSubmitData) {
    this.buildStepPartService.PutBuildStepPart(buildStepPartData.id, buildStepPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBuildStepPart) => {

        this.buildStepPartService.ClearAllCaches();

        this.buildStepPartChanged.next([updatedBuildStepPart]);

        this.alertService.showMessage("Build Step Part updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Build Step Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(buildStepPartData: BuildStepPartData | null) {

    if (buildStepPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildStepPartForm.reset({
        buildManualStepId: null,
        placedBrickId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildStepPartForm.reset({
        buildManualStepId: buildStepPartData.buildManualStepId,
        placedBrickId: buildStepPartData.placedBrickId,
        active: buildStepPartData.active ?? true,
        deleted: buildStepPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildStepPartForm.markAsPristine();
    this.buildStepPartForm.markAsUntouched();
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


  public userIsBMCBuildStepPartReader(): boolean {
    return this.buildStepPartService.userIsBMCBuildStepPartReader();
  }

  public userIsBMCBuildStepPartWriter(): boolean {
    return this.buildStepPartService.userIsBMCBuildStepPartWriter();
  }
}
