/*
   GENERATED FORM FOR THE TELEMETRYNETWORKHEALTH TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryNetworkHealth table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-network-health-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryNetworkHealthService, TelemetryNetworkHealthData, TelemetryNetworkHealthSubmitData } from '../../../telemetry-data-services/telemetry-network-health.service';
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
interface TelemetryNetworkHealthFormValues {
  telemetrySnapshotId: number | bigint,       // For FK link number
  interfaceName: string,
  interfaceDescription: string | null,
  linkSpeedMbps: string | null,     // Stored as string for form input, converted to number on submit.
  bytesSentTotal: string | null,     // Stored as string for form input, converted to number on submit.
  bytesReceivedTotal: string | null,     // Stored as string for form input, converted to number on submit.
  bytesSentPerSecond: string | null,     // Stored as string for form input, converted to number on submit.
  bytesReceivedPerSecond: string | null,     // Stored as string for form input, converted to number on submit.
  utilizationPercent: string | null,     // Stored as string for form input, converted to number on submit.
  status: string | null,
  isActive: boolean,
};

@Component({
  selector: 'app-telemetry-network-health-add-edit',
  templateUrl: './telemetry-network-health-add-edit.component.html',
  styleUrls: ['./telemetry-network-health-add-edit.component.scss']
})
export class TelemetryNetworkHealthAddEditComponent {
  @ViewChild('telemetryNetworkHealthModal') telemetryNetworkHealthModal!: TemplateRef<any>;
  @Output() telemetryNetworkHealthChanged = new Subject<TelemetryNetworkHealthData[]>();
  @Input() telemetryNetworkHealthSubmitData: TelemetryNetworkHealthSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryNetworkHealthFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryNetworkHealthForm: FormGroup = this.fb.group({
        telemetrySnapshotId: [null, Validators.required],
        interfaceName: ['', Validators.required],
        interfaceDescription: [''],
        linkSpeedMbps: [''],
        bytesSentTotal: [''],
        bytesReceivedTotal: [''],
        bytesSentPerSecond: [''],
        bytesReceivedPerSecond: [''],
        utilizationPercent: [''],
        status: [''],
        isActive: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  telemetryNetworkHealths$ = this.telemetryNetworkHealthService.GetTelemetryNetworkHealthList();
  telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  constructor(
    private modalService: NgbModal,
    private telemetryNetworkHealthService: TelemetryNetworkHealthService,
    private telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(telemetryNetworkHealthData?: TelemetryNetworkHealthData) {

    if (telemetryNetworkHealthData != null) {

      if (!this.telemetryNetworkHealthService.userIsTelemetryTelemetryNetworkHealthReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Telemetry Network Healths`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.telemetryNetworkHealthSubmitData = this.telemetryNetworkHealthService.ConvertToTelemetryNetworkHealthSubmitData(telemetryNetworkHealthData);
      this.isEditMode = true;

      this.buildFormValues(telemetryNetworkHealthData);

    } else {

      if (!this.telemetryNetworkHealthService.userIsTelemetryTelemetryNetworkHealthWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Telemetry Network Healths`,
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
        this.telemetryNetworkHealthForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryNetworkHealthForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.telemetryNetworkHealthModal, {
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

    if (this.telemetryNetworkHealthService.userIsTelemetryTelemetryNetworkHealthWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Telemetry Network Healths`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.telemetryNetworkHealthForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryNetworkHealthForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryNetworkHealthForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryNetworkHealthSubmitData: TelemetryNetworkHealthSubmitData = {
        id: this.telemetryNetworkHealthSubmitData?.id || 0,
        telemetrySnapshotId: Number(formValue.telemetrySnapshotId),
        interfaceName: formValue.interfaceName!.trim(),
        interfaceDescription: formValue.interfaceDescription?.trim() || null,
        linkSpeedMbps: formValue.linkSpeedMbps ? Number(formValue.linkSpeedMbps) : null,
        bytesSentTotal: formValue.bytesSentTotal ? Number(formValue.bytesSentTotal) : null,
        bytesReceivedTotal: formValue.bytesReceivedTotal ? Number(formValue.bytesReceivedTotal) : null,
        bytesSentPerSecond: formValue.bytesSentPerSecond ? Number(formValue.bytesSentPerSecond) : null,
        bytesReceivedPerSecond: formValue.bytesReceivedPerSecond ? Number(formValue.bytesReceivedPerSecond) : null,
        utilizationPercent: formValue.utilizationPercent ? Number(formValue.utilizationPercent) : null,
        status: formValue.status?.trim() || null,
        isActive: !!formValue.isActive,
   };

      if (this.isEditMode) {
        this.updateTelemetryNetworkHealth(telemetryNetworkHealthSubmitData);
      } else {
        this.addTelemetryNetworkHealth(telemetryNetworkHealthSubmitData);
      }
  }

  private addTelemetryNetworkHealth(telemetryNetworkHealthData: TelemetryNetworkHealthSubmitData) {
    this.telemetryNetworkHealthService.PostTelemetryNetworkHealth(telemetryNetworkHealthData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTelemetryNetworkHealth) => {

        this.telemetryNetworkHealthService.ClearAllCaches();

        this.telemetryNetworkHealthChanged.next([newTelemetryNetworkHealth]);

        this.alertService.showMessage("Telemetry Network Health added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/telemetrynetworkhealth', newTelemetryNetworkHealth.id]);
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
                                   'You do not have permission to save this Telemetry Network Health.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Network Health.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Network Health could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTelemetryNetworkHealth(telemetryNetworkHealthData: TelemetryNetworkHealthSubmitData) {
    this.telemetryNetworkHealthService.PutTelemetryNetworkHealth(telemetryNetworkHealthData.id, telemetryNetworkHealthData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTelemetryNetworkHealth) => {

        this.telemetryNetworkHealthService.ClearAllCaches();

        this.telemetryNetworkHealthChanged.next([updatedTelemetryNetworkHealth]);

        this.alertService.showMessage("Telemetry Network Health updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Telemetry Network Health.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Network Health.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Network Health could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(telemetryNetworkHealthData: TelemetryNetworkHealthData | null) {

    if (telemetryNetworkHealthData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryNetworkHealthForm.reset({
        telemetrySnapshotId: null,
        interfaceName: '',
        interfaceDescription: '',
        linkSpeedMbps: '',
        bytesSentTotal: '',
        bytesReceivedTotal: '',
        bytesSentPerSecond: '',
        bytesReceivedPerSecond: '',
        utilizationPercent: '',
        status: '',
        isActive: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryNetworkHealthForm.reset({
        telemetrySnapshotId: telemetryNetworkHealthData.telemetrySnapshotId,
        interfaceName: telemetryNetworkHealthData.interfaceName ?? '',
        interfaceDescription: telemetryNetworkHealthData.interfaceDescription ?? '',
        linkSpeedMbps: telemetryNetworkHealthData.linkSpeedMbps?.toString() ?? '',
        bytesSentTotal: telemetryNetworkHealthData.bytesSentTotal?.toString() ?? '',
        bytesReceivedTotal: telemetryNetworkHealthData.bytesReceivedTotal?.toString() ?? '',
        bytesSentPerSecond: telemetryNetworkHealthData.bytesSentPerSecond?.toString() ?? '',
        bytesReceivedPerSecond: telemetryNetworkHealthData.bytesReceivedPerSecond?.toString() ?? '',
        utilizationPercent: telemetryNetworkHealthData.utilizationPercent?.toString() ?? '',
        status: telemetryNetworkHealthData.status ?? '',
        isActive: telemetryNetworkHealthData.isActive ?? false,
      }, { emitEvent: false});
    }

    this.telemetryNetworkHealthForm.markAsPristine();
    this.telemetryNetworkHealthForm.markAsUntouched();
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


  public userIsTelemetryTelemetryNetworkHealthReader(): boolean {
    return this.telemetryNetworkHealthService.userIsTelemetryTelemetryNetworkHealthReader();
  }

  public userIsTelemetryTelemetryNetworkHealthWriter(): boolean {
    return this.telemetryNetworkHealthService.userIsTelemetryTelemetryNetworkHealthWriter();
  }
}
