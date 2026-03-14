/*
   GENERATED FORM FOR THE BUILDSTEPANNOTATIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildStepAnnotationChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-step-annotation-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildStepAnnotationChangeHistoryService, BuildStepAnnotationChangeHistoryData, BuildStepAnnotationChangeHistorySubmitData } from '../../../bmc-data-services/build-step-annotation-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BuildStepAnnotationService } from '../../../bmc-data-services/build-step-annotation.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BuildStepAnnotationChangeHistoryFormValues {
  buildStepAnnotationId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-build-step-annotation-change-history-add-edit',
  templateUrl: './build-step-annotation-change-history-add-edit.component.html',
  styleUrls: ['./build-step-annotation-change-history-add-edit.component.scss']
})
export class BuildStepAnnotationChangeHistoryAddEditComponent {
  @ViewChild('buildStepAnnotationChangeHistoryModal') buildStepAnnotationChangeHistoryModal!: TemplateRef<any>;
  @Output() buildStepAnnotationChangeHistoryChanged = new Subject<BuildStepAnnotationChangeHistoryData[]>();
  @Input() buildStepAnnotationChangeHistorySubmitData: BuildStepAnnotationChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildStepAnnotationChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildStepAnnotationChangeHistoryForm: FormGroup = this.fb.group({
        buildStepAnnotationId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  buildStepAnnotationChangeHistories$ = this.buildStepAnnotationChangeHistoryService.GetBuildStepAnnotationChangeHistoryList();
  buildStepAnnotations$ = this.buildStepAnnotationService.GetBuildStepAnnotationList();

  constructor(
    private modalService: NgbModal,
    private buildStepAnnotationChangeHistoryService: BuildStepAnnotationChangeHistoryService,
    private buildStepAnnotationService: BuildStepAnnotationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(buildStepAnnotationChangeHistoryData?: BuildStepAnnotationChangeHistoryData) {

    if (buildStepAnnotationChangeHistoryData != null) {

      if (!this.buildStepAnnotationChangeHistoryService.userIsBMCBuildStepAnnotationChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Build Step Annotation Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.buildStepAnnotationChangeHistorySubmitData = this.buildStepAnnotationChangeHistoryService.ConvertToBuildStepAnnotationChangeHistorySubmitData(buildStepAnnotationChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(buildStepAnnotationChangeHistoryData);

    } else {

      if (!this.buildStepAnnotationChangeHistoryService.userIsBMCBuildStepAnnotationChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Build Step Annotation Change Histories`,
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
        this.buildStepAnnotationChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildStepAnnotationChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.buildStepAnnotationChangeHistoryModal, {
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

    if (this.buildStepAnnotationChangeHistoryService.userIsBMCBuildStepAnnotationChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Build Step Annotation Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.buildStepAnnotationChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildStepAnnotationChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildStepAnnotationChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildStepAnnotationChangeHistorySubmitData: BuildStepAnnotationChangeHistorySubmitData = {
        id: this.buildStepAnnotationChangeHistorySubmitData?.id || 0,
        buildStepAnnotationId: Number(formValue.buildStepAnnotationId),
        versionNumber: this.buildStepAnnotationChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateBuildStepAnnotationChangeHistory(buildStepAnnotationChangeHistorySubmitData);
      } else {
        this.addBuildStepAnnotationChangeHistory(buildStepAnnotationChangeHistorySubmitData);
      }
  }

  private addBuildStepAnnotationChangeHistory(buildStepAnnotationChangeHistoryData: BuildStepAnnotationChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    buildStepAnnotationChangeHistoryData.versionNumber = 0;
    this.buildStepAnnotationChangeHistoryService.PostBuildStepAnnotationChangeHistory(buildStepAnnotationChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBuildStepAnnotationChangeHistory) => {

        this.buildStepAnnotationChangeHistoryService.ClearAllCaches();

        this.buildStepAnnotationChangeHistoryChanged.next([newBuildStepAnnotationChangeHistory]);

        this.alertService.showMessage("Build Step Annotation Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/buildstepannotationchangehistory', newBuildStepAnnotationChangeHistory.id]);
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
                                   'You do not have permission to save this Build Step Annotation Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Annotation Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Annotation Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBuildStepAnnotationChangeHistory(buildStepAnnotationChangeHistoryData: BuildStepAnnotationChangeHistorySubmitData) {
    this.buildStepAnnotationChangeHistoryService.PutBuildStepAnnotationChangeHistory(buildStepAnnotationChangeHistoryData.id, buildStepAnnotationChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBuildStepAnnotationChangeHistory) => {

        this.buildStepAnnotationChangeHistoryService.ClearAllCaches();

        this.buildStepAnnotationChangeHistoryChanged.next([updatedBuildStepAnnotationChangeHistory]);

        this.alertService.showMessage("Build Step Annotation Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Build Step Annotation Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Annotation Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Annotation Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(buildStepAnnotationChangeHistoryData: BuildStepAnnotationChangeHistoryData | null) {

    if (buildStepAnnotationChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildStepAnnotationChangeHistoryForm.reset({
        buildStepAnnotationId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildStepAnnotationChangeHistoryForm.reset({
        buildStepAnnotationId: buildStepAnnotationChangeHistoryData.buildStepAnnotationId,
        versionNumber: buildStepAnnotationChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(buildStepAnnotationChangeHistoryData.timeStamp) ?? '',
        userId: buildStepAnnotationChangeHistoryData.userId?.toString() ?? '',
        data: buildStepAnnotationChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.buildStepAnnotationChangeHistoryForm.markAsPristine();
    this.buildStepAnnotationChangeHistoryForm.markAsUntouched();
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


  public userIsBMCBuildStepAnnotationChangeHistoryReader(): boolean {
    return this.buildStepAnnotationChangeHistoryService.userIsBMCBuildStepAnnotationChangeHistoryReader();
  }

  public userIsBMCBuildStepAnnotationChangeHistoryWriter(): boolean {
    return this.buildStepAnnotationChangeHistoryService.userIsBMCBuildStepAnnotationChangeHistoryWriter();
  }
}
