/*
   GENERATED FORM FOR THE TELEMETRYCOLLECTIONRUN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryCollectionRun table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-collection-run-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryCollectionRunService, TelemetryCollectionRunData, TelemetryCollectionRunSubmitData } from '../../../telemetry-data-services/telemetry-collection-run.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TelemetryCollectionRunFormValues {
  startTime: string,
  endTime: string | null,
  applicationsPolled: string | null,     // Stored as string for form input, converted to number on submit.
  applicationsSucceeded: string | null,     // Stored as string for form input, converted to number on submit.
  errorMessage: string | null,
};

@Component({
  selector: 'app-telemetry-collection-run-add-edit',
  templateUrl: './telemetry-collection-run-add-edit.component.html',
  styleUrls: ['./telemetry-collection-run-add-edit.component.scss']
})
export class TelemetryCollectionRunAddEditComponent {
  @ViewChild('telemetryCollectionRunModal') telemetryCollectionRunModal!: TemplateRef<any>;
  @Output() telemetryCollectionRunChanged = new Subject<TelemetryCollectionRunData[]>();
  @Input() telemetryCollectionRunSubmitData: TelemetryCollectionRunSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryCollectionRunFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryCollectionRunForm: FormGroup = this.fb.group({
        startTime: ['', Validators.required],
        endTime: [''],
        applicationsPolled: [''],
        applicationsSucceeded: [''],
        errorMessage: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  telemetryCollectionRuns$ = this.telemetryCollectionRunService.GetTelemetryCollectionRunList();

  constructor(
    private modalService: NgbModal,
    private telemetryCollectionRunService: TelemetryCollectionRunService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(telemetryCollectionRunData?: TelemetryCollectionRunData) {

    if (telemetryCollectionRunData != null) {

      if (!this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Telemetry Collection Runs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.telemetryCollectionRunSubmitData = this.telemetryCollectionRunService.ConvertToTelemetryCollectionRunSubmitData(telemetryCollectionRunData);
      this.isEditMode = true;

      this.buildFormValues(telemetryCollectionRunData);

    } else {

      if (!this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Telemetry Collection Runs`,
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
        this.telemetryCollectionRunForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryCollectionRunForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.telemetryCollectionRunModal, {
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

    if (this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Telemetry Collection Runs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.telemetryCollectionRunForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryCollectionRunForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryCollectionRunForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryCollectionRunSubmitData: TelemetryCollectionRunSubmitData = {
        id: this.telemetryCollectionRunSubmitData?.id || 0,
        startTime: dateTimeLocalToIsoUtc(formValue.startTime!.trim())!,
        endTime: formValue.endTime ? dateTimeLocalToIsoUtc(formValue.endTime.trim()) : null,
        applicationsPolled: formValue.applicationsPolled ? Number(formValue.applicationsPolled) : null,
        applicationsSucceeded: formValue.applicationsSucceeded ? Number(formValue.applicationsSucceeded) : null,
        errorMessage: formValue.errorMessage?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateTelemetryCollectionRun(telemetryCollectionRunSubmitData);
      } else {
        this.addTelemetryCollectionRun(telemetryCollectionRunSubmitData);
      }
  }

  private addTelemetryCollectionRun(telemetryCollectionRunData: TelemetryCollectionRunSubmitData) {
    this.telemetryCollectionRunService.PostTelemetryCollectionRun(telemetryCollectionRunData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTelemetryCollectionRun) => {

        this.telemetryCollectionRunService.ClearAllCaches();

        this.telemetryCollectionRunChanged.next([newTelemetryCollectionRun]);

        this.alertService.showMessage("Telemetry Collection Run added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/telemetrycollectionrun', newTelemetryCollectionRun.id]);
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
                                   'You do not have permission to save this Telemetry Collection Run.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Collection Run.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Collection Run could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTelemetryCollectionRun(telemetryCollectionRunData: TelemetryCollectionRunSubmitData) {
    this.telemetryCollectionRunService.PutTelemetryCollectionRun(telemetryCollectionRunData.id, telemetryCollectionRunData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTelemetryCollectionRun) => {

        this.telemetryCollectionRunService.ClearAllCaches();

        this.telemetryCollectionRunChanged.next([updatedTelemetryCollectionRun]);

        this.alertService.showMessage("Telemetry Collection Run updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Telemetry Collection Run.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Collection Run.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Collection Run could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(telemetryCollectionRunData: TelemetryCollectionRunData | null) {

    if (telemetryCollectionRunData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryCollectionRunForm.reset({
        startTime: '',
        endTime: '',
        applicationsPolled: '',
        applicationsSucceeded: '',
        errorMessage: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryCollectionRunForm.reset({
        startTime: isoUtcStringToDateTimeLocal(telemetryCollectionRunData.startTime) ?? '',
        endTime: isoUtcStringToDateTimeLocal(telemetryCollectionRunData.endTime) ?? '',
        applicationsPolled: telemetryCollectionRunData.applicationsPolled?.toString() ?? '',
        applicationsSucceeded: telemetryCollectionRunData.applicationsSucceeded?.toString() ?? '',
        errorMessage: telemetryCollectionRunData.errorMessage ?? '',
      }, { emitEvent: false});
    }

    this.telemetryCollectionRunForm.markAsPristine();
    this.telemetryCollectionRunForm.markAsUntouched();
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


  public userIsTelemetryTelemetryCollectionRunReader(): boolean {
    return this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunReader();
  }

  public userIsTelemetryTelemetryCollectionRunWriter(): boolean {
    return this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunWriter();
  }
}
