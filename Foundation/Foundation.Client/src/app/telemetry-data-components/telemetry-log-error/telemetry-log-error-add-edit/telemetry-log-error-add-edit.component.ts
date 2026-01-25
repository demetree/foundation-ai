/*
   GENERATED FORM FOR THE TELEMETRYLOGERROR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryLogError table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-log-error-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryLogErrorService, TelemetryLogErrorData, TelemetryLogErrorSubmitData } from '../../../telemetry-data-services/telemetry-log-error.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { TelemetryApplicationService } from '../../../telemetry-data-services/telemetry-application.service';
import { TelemetrySnapshotService } from '../../../telemetry-data-services/telemetry-snapshot.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TelemetryLogErrorFormValues {
  telemetryApplicationId: number | bigint,       // For FK link number
  telemetrySnapshotId: number | bigint | null,       // For FK link number
  capturedAt: string,
  logFileName: string | null,
  logTimestamp: string | null,
  level: string | null,
  message: string | null,
  exception: string | null,
};

@Component({
  selector: 'app-telemetry-log-error-add-edit',
  templateUrl: './telemetry-log-error-add-edit.component.html',
  styleUrls: ['./telemetry-log-error-add-edit.component.scss']
})
export class TelemetryLogErrorAddEditComponent {
  @ViewChild('telemetryLogErrorModal') telemetryLogErrorModal!: TemplateRef<any>;
  @Output() telemetryLogErrorChanged = new Subject<TelemetryLogErrorData[]>();
  @Input() telemetryLogErrorSubmitData: TelemetryLogErrorSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryLogErrorFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryLogErrorForm: FormGroup = this.fb.group({
        telemetryApplicationId: [null, Validators.required],
        telemetrySnapshotId: [null],
        capturedAt: ['', Validators.required],
        logFileName: [''],
        logTimestamp: [''],
        level: [''],
        message: [''],
        exception: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  telemetryLogErrors$ = this.telemetryLogErrorService.GetTelemetryLogErrorList();
  telemetryApplications$ = this.telemetryApplicationService.GetTelemetryApplicationList();
  telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  constructor(
    private modalService: NgbModal,
    private telemetryLogErrorService: TelemetryLogErrorService,
    private telemetryApplicationService: TelemetryApplicationService,
    private telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(telemetryLogErrorData?: TelemetryLogErrorData) {

    if (telemetryLogErrorData != null) {

      if (!this.telemetryLogErrorService.userIsTelemetryTelemetryLogErrorReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Telemetry Log Errors`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.telemetryLogErrorSubmitData = this.telemetryLogErrorService.ConvertToTelemetryLogErrorSubmitData(telemetryLogErrorData);
      this.isEditMode = true;

      this.buildFormValues(telemetryLogErrorData);

    } else {

      if (!this.telemetryLogErrorService.userIsTelemetryTelemetryLogErrorWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Telemetry Log Errors`,
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
        this.telemetryLogErrorForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryLogErrorForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.telemetryLogErrorModal, {
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

    if (this.telemetryLogErrorService.userIsTelemetryTelemetryLogErrorWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Telemetry Log Errors`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.telemetryLogErrorForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryLogErrorForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryLogErrorForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryLogErrorSubmitData: TelemetryLogErrorSubmitData = {
        id: this.telemetryLogErrorSubmitData?.id || 0,
        telemetryApplicationId: Number(formValue.telemetryApplicationId),
        telemetrySnapshotId: formValue.telemetrySnapshotId ? Number(formValue.telemetrySnapshotId) : null,
        capturedAt: dateTimeLocalToIsoUtc(formValue.capturedAt!.trim())!,
        logFileName: formValue.logFileName?.trim() || null,
        logTimestamp: formValue.logTimestamp ? dateTimeLocalToIsoUtc(formValue.logTimestamp.trim()) : null,
        level: formValue.level?.trim() || null,
        message: formValue.message?.trim() || null,
        exception: formValue.exception?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateTelemetryLogError(telemetryLogErrorSubmitData);
      } else {
        this.addTelemetryLogError(telemetryLogErrorSubmitData);
      }
  }

  private addTelemetryLogError(telemetryLogErrorData: TelemetryLogErrorSubmitData) {
    this.telemetryLogErrorService.PostTelemetryLogError(telemetryLogErrorData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTelemetryLogError) => {

        this.telemetryLogErrorService.ClearAllCaches();

        this.telemetryLogErrorChanged.next([newTelemetryLogError]);

        this.alertService.showMessage("Telemetry Log Error added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/telemetrylogerror', newTelemetryLogError.id]);
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
                                   'You do not have permission to save this Telemetry Log Error.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Log Error.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Log Error could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTelemetryLogError(telemetryLogErrorData: TelemetryLogErrorSubmitData) {
    this.telemetryLogErrorService.PutTelemetryLogError(telemetryLogErrorData.id, telemetryLogErrorData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTelemetryLogError) => {

        this.telemetryLogErrorService.ClearAllCaches();

        this.telemetryLogErrorChanged.next([updatedTelemetryLogError]);

        this.alertService.showMessage("Telemetry Log Error updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Telemetry Log Error.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Log Error.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Log Error could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(telemetryLogErrorData: TelemetryLogErrorData | null) {

    if (telemetryLogErrorData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryLogErrorForm.reset({
        telemetryApplicationId: null,
        telemetrySnapshotId: null,
        capturedAt: '',
        logFileName: '',
        logTimestamp: '',
        level: '',
        message: '',
        exception: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryLogErrorForm.reset({
        telemetryApplicationId: telemetryLogErrorData.telemetryApplicationId,
        telemetrySnapshotId: telemetryLogErrorData.telemetrySnapshotId,
        capturedAt: isoUtcStringToDateTimeLocal(telemetryLogErrorData.capturedAt) ?? '',
        logFileName: telemetryLogErrorData.logFileName ?? '',
        logTimestamp: isoUtcStringToDateTimeLocal(telemetryLogErrorData.logTimestamp) ?? '',
        level: telemetryLogErrorData.level ?? '',
        message: telemetryLogErrorData.message ?? '',
        exception: telemetryLogErrorData.exception ?? '',
      }, { emitEvent: false});
    }

    this.telemetryLogErrorForm.markAsPristine();
    this.telemetryLogErrorForm.markAsUntouched();
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


  public userIsTelemetryTelemetryLogErrorReader(): boolean {
    return this.telemetryLogErrorService.userIsTelemetryTelemetryLogErrorReader();
  }

  public userIsTelemetryTelemetryLogErrorWriter(): boolean {
    return this.telemetryLogErrorService.userIsTelemetryTelemetryLogErrorWriter();
  }
}
