/*
   GENERATED FORM FOR THE TELEMETRYDISKHEALTH TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryDiskHealth table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-disk-health-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryDiskHealthService, TelemetryDiskHealthData, TelemetryDiskHealthSubmitData } from '../../../telemetry-data-services/telemetry-disk-health.service';
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
interface TelemetryDiskHealthFormValues {
  telemetrySnapshotId: number | bigint,       // For FK link number
  driveName: string,
  driveLabel: string | null,
  totalGB: string | null,     // Stored as string for form input, converted to number on submit.
  freeGB: string | null,     // Stored as string for form input, converted to number on submit.
  freePercent: string | null,     // Stored as string for form input, converted to number on submit.
  usedPercent: string | null,     // Stored as string for form input, converted to number on submit.
  status: string | null,
  isApplicationDrive: boolean,
};

@Component({
  selector: 'app-telemetry-disk-health-add-edit',
  templateUrl: './telemetry-disk-health-add-edit.component.html',
  styleUrls: ['./telemetry-disk-health-add-edit.component.scss']
})
export class TelemetryDiskHealthAddEditComponent {
  @ViewChild('telemetryDiskHealthModal') telemetryDiskHealthModal!: TemplateRef<any>;
  @Output() telemetryDiskHealthChanged = new Subject<TelemetryDiskHealthData[]>();
  @Input() telemetryDiskHealthSubmitData: TelemetryDiskHealthSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryDiskHealthFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryDiskHealthForm: FormGroup = this.fb.group({
        telemetrySnapshotId: [null, Validators.required],
        driveName: ['', Validators.required],
        driveLabel: [''],
        totalGB: [''],
        freeGB: [''],
        freePercent: [''],
        usedPercent: [''],
        status: [''],
        isApplicationDrive: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  telemetryDiskHealths$ = this.telemetryDiskHealthService.GetTelemetryDiskHealthList();
  telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  constructor(
    private modalService: NgbModal,
    private telemetryDiskHealthService: TelemetryDiskHealthService,
    private telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(telemetryDiskHealthData?: TelemetryDiskHealthData) {

    if (telemetryDiskHealthData != null) {

      if (!this.telemetryDiskHealthService.userIsTelemetryTelemetryDiskHealthReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Telemetry Disk Healths`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.telemetryDiskHealthSubmitData = this.telemetryDiskHealthService.ConvertToTelemetryDiskHealthSubmitData(telemetryDiskHealthData);
      this.isEditMode = true;

      this.buildFormValues(telemetryDiskHealthData);

    } else {

      if (!this.telemetryDiskHealthService.userIsTelemetryTelemetryDiskHealthWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Telemetry Disk Healths`,
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
        this.telemetryDiskHealthForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryDiskHealthForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.telemetryDiskHealthModal, {
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

    if (this.telemetryDiskHealthService.userIsTelemetryTelemetryDiskHealthWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Telemetry Disk Healths`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.telemetryDiskHealthForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryDiskHealthForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryDiskHealthForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryDiskHealthSubmitData: TelemetryDiskHealthSubmitData = {
        id: this.telemetryDiskHealthSubmitData?.id || 0,
        telemetrySnapshotId: Number(formValue.telemetrySnapshotId),
        driveName: formValue.driveName!.trim(),
        driveLabel: formValue.driveLabel?.trim() || null,
        totalGB: formValue.totalGB ? Number(formValue.totalGB) : null,
        freeGB: formValue.freeGB ? Number(formValue.freeGB) : null,
        freePercent: formValue.freePercent ? Number(formValue.freePercent) : null,
        usedPercent: formValue.usedPercent ? Number(formValue.usedPercent) : null,
        status: formValue.status?.trim() || null,
        isApplicationDrive: !!formValue.isApplicationDrive,
   };

      if (this.isEditMode) {
        this.updateTelemetryDiskHealth(telemetryDiskHealthSubmitData);
      } else {
        this.addTelemetryDiskHealth(telemetryDiskHealthSubmitData);
      }
  }

  private addTelemetryDiskHealth(telemetryDiskHealthData: TelemetryDiskHealthSubmitData) {
    this.telemetryDiskHealthService.PostTelemetryDiskHealth(telemetryDiskHealthData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTelemetryDiskHealth) => {

        this.telemetryDiskHealthService.ClearAllCaches();

        this.telemetryDiskHealthChanged.next([newTelemetryDiskHealth]);

        this.alertService.showMessage("Telemetry Disk Health added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/telemetrydiskhealth', newTelemetryDiskHealth.id]);
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
                                   'You do not have permission to save this Telemetry Disk Health.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Disk Health.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Disk Health could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTelemetryDiskHealth(telemetryDiskHealthData: TelemetryDiskHealthSubmitData) {
    this.telemetryDiskHealthService.PutTelemetryDiskHealth(telemetryDiskHealthData.id, telemetryDiskHealthData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTelemetryDiskHealth) => {

        this.telemetryDiskHealthService.ClearAllCaches();

        this.telemetryDiskHealthChanged.next([updatedTelemetryDiskHealth]);

        this.alertService.showMessage("Telemetry Disk Health updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Telemetry Disk Health.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Disk Health.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Disk Health could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(telemetryDiskHealthData: TelemetryDiskHealthData | null) {

    if (telemetryDiskHealthData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryDiskHealthForm.reset({
        telemetrySnapshotId: null,
        driveName: '',
        driveLabel: '',
        totalGB: '',
        freeGB: '',
        freePercent: '',
        usedPercent: '',
        status: '',
        isApplicationDrive: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryDiskHealthForm.reset({
        telemetrySnapshotId: telemetryDiskHealthData.telemetrySnapshotId,
        driveName: telemetryDiskHealthData.driveName ?? '',
        driveLabel: telemetryDiskHealthData.driveLabel ?? '',
        totalGB: telemetryDiskHealthData.totalGB?.toString() ?? '',
        freeGB: telemetryDiskHealthData.freeGB?.toString() ?? '',
        freePercent: telemetryDiskHealthData.freePercent?.toString() ?? '',
        usedPercent: telemetryDiskHealthData.usedPercent?.toString() ?? '',
        status: telemetryDiskHealthData.status ?? '',
        isApplicationDrive: telemetryDiskHealthData.isApplicationDrive ?? false,
      }, { emitEvent: false});
    }

    this.telemetryDiskHealthForm.markAsPristine();
    this.telemetryDiskHealthForm.markAsUntouched();
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


  public userIsTelemetryTelemetryDiskHealthReader(): boolean {
    return this.telemetryDiskHealthService.userIsTelemetryTelemetryDiskHealthReader();
  }

  public userIsTelemetryTelemetryDiskHealthWriter(): boolean {
    return this.telemetryDiskHealthService.userIsTelemetryTelemetryDiskHealthWriter();
  }
}
