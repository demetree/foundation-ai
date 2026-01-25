/*
   GENERATED FORM FOR THE TELEMETRYDATABASEHEALTH TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryDatabaseHealth table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-database-health-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryDatabaseHealthService, TelemetryDatabaseHealthData, TelemetryDatabaseHealthSubmitData } from '../../../telemetry-data-services/telemetry-database-health.service';
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
interface TelemetryDatabaseHealthFormValues {
  telemetrySnapshotId: number | bigint,       // For FK link number
  databaseName: string,
  isConnected: boolean,
  status: string | null,
  server: string | null,
  provider: string | null,
  errorMessage: string | null,
};

@Component({
  selector: 'app-telemetry-database-health-add-edit',
  templateUrl: './telemetry-database-health-add-edit.component.html',
  styleUrls: ['./telemetry-database-health-add-edit.component.scss']
})
export class TelemetryDatabaseHealthAddEditComponent {
  @ViewChild('telemetryDatabaseHealthModal') telemetryDatabaseHealthModal!: TemplateRef<any>;
  @Output() telemetryDatabaseHealthChanged = new Subject<TelemetryDatabaseHealthData[]>();
  @Input() telemetryDatabaseHealthSubmitData: TelemetryDatabaseHealthSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryDatabaseHealthFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryDatabaseHealthForm: FormGroup = this.fb.group({
        telemetrySnapshotId: [null, Validators.required],
        databaseName: ['', Validators.required],
        isConnected: [false],
        status: [''],
        server: [''],
        provider: [''],
        errorMessage: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  telemetryDatabaseHealths$ = this.telemetryDatabaseHealthService.GetTelemetryDatabaseHealthList();
  telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  constructor(
    private modalService: NgbModal,
    private telemetryDatabaseHealthService: TelemetryDatabaseHealthService,
    private telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(telemetryDatabaseHealthData?: TelemetryDatabaseHealthData) {

    if (telemetryDatabaseHealthData != null) {

      if (!this.telemetryDatabaseHealthService.userIsTelemetryTelemetryDatabaseHealthReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Telemetry Database Healths`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.telemetryDatabaseHealthSubmitData = this.telemetryDatabaseHealthService.ConvertToTelemetryDatabaseHealthSubmitData(telemetryDatabaseHealthData);
      this.isEditMode = true;

      this.buildFormValues(telemetryDatabaseHealthData);

    } else {

      if (!this.telemetryDatabaseHealthService.userIsTelemetryTelemetryDatabaseHealthWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Telemetry Database Healths`,
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
        this.telemetryDatabaseHealthForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryDatabaseHealthForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.telemetryDatabaseHealthModal, {
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

    if (this.telemetryDatabaseHealthService.userIsTelemetryTelemetryDatabaseHealthWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Telemetry Database Healths`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.telemetryDatabaseHealthForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryDatabaseHealthForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryDatabaseHealthForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryDatabaseHealthSubmitData: TelemetryDatabaseHealthSubmitData = {
        id: this.telemetryDatabaseHealthSubmitData?.id || 0,
        telemetrySnapshotId: Number(formValue.telemetrySnapshotId),
        databaseName: formValue.databaseName!.trim(),
        isConnected: !!formValue.isConnected,
        status: formValue.status?.trim() || null,
        server: formValue.server?.trim() || null,
        provider: formValue.provider?.trim() || null,
        errorMessage: formValue.errorMessage?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateTelemetryDatabaseHealth(telemetryDatabaseHealthSubmitData);
      } else {
        this.addTelemetryDatabaseHealth(telemetryDatabaseHealthSubmitData);
      }
  }

  private addTelemetryDatabaseHealth(telemetryDatabaseHealthData: TelemetryDatabaseHealthSubmitData) {
    this.telemetryDatabaseHealthService.PostTelemetryDatabaseHealth(telemetryDatabaseHealthData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTelemetryDatabaseHealth) => {

        this.telemetryDatabaseHealthService.ClearAllCaches();

        this.telemetryDatabaseHealthChanged.next([newTelemetryDatabaseHealth]);

        this.alertService.showMessage("Telemetry Database Health added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/telemetrydatabasehealth', newTelemetryDatabaseHealth.id]);
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
                                   'You do not have permission to save this Telemetry Database Health.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Database Health.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Database Health could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTelemetryDatabaseHealth(telemetryDatabaseHealthData: TelemetryDatabaseHealthSubmitData) {
    this.telemetryDatabaseHealthService.PutTelemetryDatabaseHealth(telemetryDatabaseHealthData.id, telemetryDatabaseHealthData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTelemetryDatabaseHealth) => {

        this.telemetryDatabaseHealthService.ClearAllCaches();

        this.telemetryDatabaseHealthChanged.next([updatedTelemetryDatabaseHealth]);

        this.alertService.showMessage("Telemetry Database Health updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Telemetry Database Health.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Database Health.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Database Health could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(telemetryDatabaseHealthData: TelemetryDatabaseHealthData | null) {

    if (telemetryDatabaseHealthData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryDatabaseHealthForm.reset({
        telemetrySnapshotId: null,
        databaseName: '',
        isConnected: false,
        status: '',
        server: '',
        provider: '',
        errorMessage: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryDatabaseHealthForm.reset({
        telemetrySnapshotId: telemetryDatabaseHealthData.telemetrySnapshotId,
        databaseName: telemetryDatabaseHealthData.databaseName ?? '',
        isConnected: telemetryDatabaseHealthData.isConnected ?? false,
        status: telemetryDatabaseHealthData.status ?? '',
        server: telemetryDatabaseHealthData.server ?? '',
        provider: telemetryDatabaseHealthData.provider ?? '',
        errorMessage: telemetryDatabaseHealthData.errorMessage ?? '',
      }, { emitEvent: false});
    }

    this.telemetryDatabaseHealthForm.markAsPristine();
    this.telemetryDatabaseHealthForm.markAsUntouched();
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


  public userIsTelemetryTelemetryDatabaseHealthReader(): boolean {
    return this.telemetryDatabaseHealthService.userIsTelemetryTelemetryDatabaseHealthReader();
  }

  public userIsTelemetryTelemetryDatabaseHealthWriter(): boolean {
    return this.telemetryDatabaseHealthService.userIsTelemetryTelemetryDatabaseHealthWriter();
  }
}
