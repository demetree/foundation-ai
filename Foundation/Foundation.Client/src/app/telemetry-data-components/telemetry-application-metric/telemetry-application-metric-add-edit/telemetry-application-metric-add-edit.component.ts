/*
   GENERATED FORM FOR THE TELEMETRYAPPLICATIONMETRIC TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryApplicationMetric table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-application-metric-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryApplicationMetricService, TelemetryApplicationMetricData, TelemetryApplicationMetricSubmitData } from '../../../telemetry-data-services/telemetry-application-metric.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { TelemetrySnapshotService } from '../../../telemetry-data-services/telemetry-snapshot.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TelemetryApplicationMetricFormValues {
  telemetrySnapshotId: number | bigint,       // For FK link number
  metricName: string,
  metricValue: string | null,
  state: string | null,     // Stored as string for form input, converted to number on submit.
  dataType: string | null,     // Stored as string for form input, converted to number on submit.
  numericValue: string | null,     // Stored as string for form input, converted to number on submit.
  category: string | null,
};

@Component({
  selector: 'app-telemetry-application-metric-add-edit',
  templateUrl: './telemetry-application-metric-add-edit.component.html',
  styleUrls: ['./telemetry-application-metric-add-edit.component.scss']
})
export class TelemetryApplicationMetricAddEditComponent {
  @ViewChild('telemetryApplicationMetricModal') telemetryApplicationMetricModal!: TemplateRef<any>;
  @Output() telemetryApplicationMetricChanged = new Subject<TelemetryApplicationMetricData[]>();
  @Input() telemetryApplicationMetricSubmitData: TelemetryApplicationMetricSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryApplicationMetricFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryApplicationMetricForm: FormGroup = this.fb.group({
        telemetrySnapshotId: [null, Validators.required],
        metricName: ['', Validators.required],
        metricValue: [''],
        state: [''],
        dataType: [''],
        numericValue: [''],
        category: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  telemetryApplicationMetrics$ = this.telemetryApplicationMetricService.GetTelemetryApplicationMetricList();
  telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  constructor(
    private modalService: NgbModal,
    private telemetryApplicationMetricService: TelemetryApplicationMetricService,
    private telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(telemetryApplicationMetricData?: TelemetryApplicationMetricData) {

    if (telemetryApplicationMetricData != null) {

      if (!this.telemetryApplicationMetricService.userIsTelemetryTelemetryApplicationMetricReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Telemetry Application Metrics`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.telemetryApplicationMetricSubmitData = this.telemetryApplicationMetricService.ConvertToTelemetryApplicationMetricSubmitData(telemetryApplicationMetricData);
      this.isEditMode = true;

      this.buildFormValues(telemetryApplicationMetricData);

    } else {

      if (!this.telemetryApplicationMetricService.userIsTelemetryTelemetryApplicationMetricWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Telemetry Application Metrics`,
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
        this.telemetryApplicationMetricForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryApplicationMetricForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.telemetryApplicationMetricModal, {
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

    if (this.telemetryApplicationMetricService.userIsTelemetryTelemetryApplicationMetricWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Telemetry Application Metrics`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.telemetryApplicationMetricForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryApplicationMetricForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryApplicationMetricForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryApplicationMetricSubmitData: TelemetryApplicationMetricSubmitData = {
        id: this.telemetryApplicationMetricSubmitData?.id || 0,
        telemetrySnapshotId: Number(formValue.telemetrySnapshotId),
        metricName: formValue.metricName!.trim(),
        metricValue: formValue.metricValue?.trim() || null,
        state: formValue.state ? Number(formValue.state) : null,
        dataType: formValue.dataType ? Number(formValue.dataType) : null,
        numericValue: formValue.numericValue ? Number(formValue.numericValue) : null,
        category: formValue.category?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateTelemetryApplicationMetric(telemetryApplicationMetricSubmitData);
      } else {
        this.addTelemetryApplicationMetric(telemetryApplicationMetricSubmitData);
      }
  }

  private addTelemetryApplicationMetric(telemetryApplicationMetricData: TelemetryApplicationMetricSubmitData) {
    this.telemetryApplicationMetricService.PostTelemetryApplicationMetric(telemetryApplicationMetricData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTelemetryApplicationMetric) => {

        this.telemetryApplicationMetricService.ClearAllCaches();

        this.telemetryApplicationMetricChanged.next([newTelemetryApplicationMetric]);

        this.alertService.showMessage("Telemetry Application Metric added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/telemetryapplicationmetric', newTelemetryApplicationMetric.id]);
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
                                   'You do not have permission to save this Telemetry Application Metric.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Application Metric.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Application Metric could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTelemetryApplicationMetric(telemetryApplicationMetricData: TelemetryApplicationMetricSubmitData) {
    this.telemetryApplicationMetricService.PutTelemetryApplicationMetric(telemetryApplicationMetricData.id, telemetryApplicationMetricData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTelemetryApplicationMetric) => {

        this.telemetryApplicationMetricService.ClearAllCaches();

        this.telemetryApplicationMetricChanged.next([updatedTelemetryApplicationMetric]);

        this.alertService.showMessage("Telemetry Application Metric updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Telemetry Application Metric.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Application Metric.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Application Metric could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(telemetryApplicationMetricData: TelemetryApplicationMetricData | null) {

    if (telemetryApplicationMetricData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryApplicationMetricForm.reset({
        telemetrySnapshotId: null,
        metricName: '',
        metricValue: '',
        state: '',
        dataType: '',
        numericValue: '',
        category: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryApplicationMetricForm.reset({
        telemetrySnapshotId: telemetryApplicationMetricData.telemetrySnapshotId,
        metricName: telemetryApplicationMetricData.metricName ?? '',
        metricValue: telemetryApplicationMetricData.metricValue ?? '',
        state: telemetryApplicationMetricData.state?.toString() ?? '',
        dataType: telemetryApplicationMetricData.dataType?.toString() ?? '',
        numericValue: telemetryApplicationMetricData.numericValue?.toString() ?? '',
        category: telemetryApplicationMetricData.category ?? '',
      }, { emitEvent: false});
    }

    this.telemetryApplicationMetricForm.markAsPristine();
    this.telemetryApplicationMetricForm.markAsUntouched();
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


  public userIsTelemetryTelemetryApplicationMetricReader(): boolean {
    return this.telemetryApplicationMetricService.userIsTelemetryTelemetryApplicationMetricReader();
  }

  public userIsTelemetryTelemetryApplicationMetricWriter(): boolean {
    return this.telemetryApplicationMetricService.userIsTelemetryTelemetryApplicationMetricWriter();
  }
}
