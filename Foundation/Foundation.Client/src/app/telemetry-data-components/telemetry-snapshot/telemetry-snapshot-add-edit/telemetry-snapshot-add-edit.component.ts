/*
   GENERATED FORM FOR THE TELEMETRYSNAPSHOT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetrySnapshot table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-snapshot-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetrySnapshotService, TelemetrySnapshotData, TelemetrySnapshotSubmitData } from '../../../telemetry-data-services/telemetry-snapshot.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { TelemetryApplicationService } from '../../../telemetry-data-services/telemetry-application.service';
import { TelemetryCollectionRunService } from '../../../telemetry-data-services/telemetry-collection-run.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TelemetrySnapshotFormValues {
  telemetryApplicationId: number | bigint,       // For FK link number
  telemetryCollectionRunId: number | bigint,       // For FK link number
  collectedAt: string,
  isOnline: boolean,
  uptimeSeconds: string | null,     // Stored as string for form input, converted to number on submit.
  memoryWorkingSetMB: string | null,     // Stored as string for form input, converted to number on submit.
  memoryGcHeapMB: string | null,     // Stored as string for form input, converted to number on submit.
  memoryPercent: string | null,     // Stored as string for form input, converted to number on submit.
  systemMemoryPercent: string | null,     // Stored as string for form input, converted to number on submit.
  cpuPercent: string | null,     // Stored as string for form input, converted to number on submit.
  systemCpuPercent: string | null,     // Stored as string for form input, converted to number on submit.
  threadPoolWorkerThreads: string | null,     // Stored as string for form input, converted to number on submit.
  threadPoolCompletionPortThreads: string | null,     // Stored as string for form input, converted to number on submit.
  threadPoolPendingWorkItems: string | null,     // Stored as string for form input, converted to number on submit.
  machineName: string | null,
  dotNetVersion: string | null,
  statusJson: string | null,
};

@Component({
  selector: 'app-telemetry-snapshot-add-edit',
  templateUrl: './telemetry-snapshot-add-edit.component.html',
  styleUrls: ['./telemetry-snapshot-add-edit.component.scss']
})
export class TelemetrySnapshotAddEditComponent {
  @ViewChild('telemetrySnapshotModal') telemetrySnapshotModal!: TemplateRef<any>;
  @Output() telemetrySnapshotChanged = new Subject<TelemetrySnapshotData[]>();
  @Input() telemetrySnapshotSubmitData: TelemetrySnapshotSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetrySnapshotFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetrySnapshotForm: FormGroup = this.fb.group({
        telemetryApplicationId: [null, Validators.required],
        telemetryCollectionRunId: [null, Validators.required],
        collectedAt: ['', Validators.required],
        isOnline: [false],
        uptimeSeconds: [''],
        memoryWorkingSetMB: [''],
        memoryGcHeapMB: [''],
        memoryPercent: [''],
        systemMemoryPercent: [''],
        cpuPercent: [''],
        systemCpuPercent: [''],
        threadPoolWorkerThreads: [''],
        threadPoolCompletionPortThreads: [''],
        threadPoolPendingWorkItems: [''],
        machineName: [''],
        dotNetVersion: [''],
        statusJson: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();
  telemetryApplications$ = this.telemetryApplicationService.GetTelemetryApplicationList();
  telemetryCollectionRuns$ = this.telemetryCollectionRunService.GetTelemetryCollectionRunList();

  constructor(
    private modalService: NgbModal,
    private telemetrySnapshotService: TelemetrySnapshotService,
    private telemetryApplicationService: TelemetryApplicationService,
    private telemetryCollectionRunService: TelemetryCollectionRunService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(telemetrySnapshotData?: TelemetrySnapshotData) {

    if (telemetrySnapshotData != null) {

      if (!this.telemetrySnapshotService.userIsTelemetryTelemetrySnapshotReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Telemetry Snapshots`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.telemetrySnapshotSubmitData = this.telemetrySnapshotService.ConvertToTelemetrySnapshotSubmitData(telemetrySnapshotData);
      this.isEditMode = true;

      this.buildFormValues(telemetrySnapshotData);

    } else {

      if (!this.telemetrySnapshotService.userIsTelemetryTelemetrySnapshotWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Telemetry Snapshots`,
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
        this.telemetrySnapshotForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetrySnapshotForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.telemetrySnapshotModal, {
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

    if (this.telemetrySnapshotService.userIsTelemetryTelemetrySnapshotWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Telemetry Snapshots`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.telemetrySnapshotForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetrySnapshotForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetrySnapshotForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetrySnapshotSubmitData: TelemetrySnapshotSubmitData = {
        id: this.telemetrySnapshotSubmitData?.id || 0,
        telemetryApplicationId: Number(formValue.telemetryApplicationId),
        telemetryCollectionRunId: Number(formValue.telemetryCollectionRunId),
        collectedAt: dateTimeLocalToIsoUtc(formValue.collectedAt!.trim())!,
        isOnline: !!formValue.isOnline,
        uptimeSeconds: formValue.uptimeSeconds ? Number(formValue.uptimeSeconds) : null,
        memoryWorkingSetMB: formValue.memoryWorkingSetMB ? Number(formValue.memoryWorkingSetMB) : null,
        memoryGcHeapMB: formValue.memoryGcHeapMB ? Number(formValue.memoryGcHeapMB) : null,
        memoryPercent: formValue.memoryPercent ? Number(formValue.memoryPercent) : null,
        systemMemoryPercent: formValue.systemMemoryPercent ? Number(formValue.systemMemoryPercent) : null,
        cpuPercent: formValue.cpuPercent ? Number(formValue.cpuPercent) : null,
        systemCpuPercent: formValue.systemCpuPercent ? Number(formValue.systemCpuPercent) : null,
        threadPoolWorkerThreads: formValue.threadPoolWorkerThreads ? Number(formValue.threadPoolWorkerThreads) : null,
        threadPoolCompletionPortThreads: formValue.threadPoolCompletionPortThreads ? Number(formValue.threadPoolCompletionPortThreads) : null,
        threadPoolPendingWorkItems: formValue.threadPoolPendingWorkItems ? Number(formValue.threadPoolPendingWorkItems) : null,
        machineName: formValue.machineName?.trim() || null,
        dotNetVersion: formValue.dotNetVersion?.trim() || null,
        statusJson: formValue.statusJson?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateTelemetrySnapshot(telemetrySnapshotSubmitData);
      } else {
        this.addTelemetrySnapshot(telemetrySnapshotSubmitData);
      }
  }

  private addTelemetrySnapshot(telemetrySnapshotData: TelemetrySnapshotSubmitData) {
    this.telemetrySnapshotService.PostTelemetrySnapshot(telemetrySnapshotData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTelemetrySnapshot) => {

        this.telemetrySnapshotService.ClearAllCaches();

        this.telemetrySnapshotChanged.next([newTelemetrySnapshot]);

        this.alertService.showMessage("Telemetry Snapshot added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/telemetrysnapshot', newTelemetrySnapshot.id]);
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
                                   'You do not have permission to save this Telemetry Snapshot.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Snapshot.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Snapshot could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTelemetrySnapshot(telemetrySnapshotData: TelemetrySnapshotSubmitData) {
    this.telemetrySnapshotService.PutTelemetrySnapshot(telemetrySnapshotData.id, telemetrySnapshotData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTelemetrySnapshot) => {

        this.telemetrySnapshotService.ClearAllCaches();

        this.telemetrySnapshotChanged.next([updatedTelemetrySnapshot]);

        this.alertService.showMessage("Telemetry Snapshot updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Telemetry Snapshot.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Snapshot.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Snapshot could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(telemetrySnapshotData: TelemetrySnapshotData | null) {

    if (telemetrySnapshotData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetrySnapshotForm.reset({
        telemetryApplicationId: null,
        telemetryCollectionRunId: null,
        collectedAt: '',
        isOnline: false,
        uptimeSeconds: '',
        memoryWorkingSetMB: '',
        memoryGcHeapMB: '',
        memoryPercent: '',
        systemMemoryPercent: '',
        cpuPercent: '',
        systemCpuPercent: '',
        threadPoolWorkerThreads: '',
        threadPoolCompletionPortThreads: '',
        threadPoolPendingWorkItems: '',
        machineName: '',
        dotNetVersion: '',
        statusJson: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetrySnapshotForm.reset({
        telemetryApplicationId: telemetrySnapshotData.telemetryApplicationId,
        telemetryCollectionRunId: telemetrySnapshotData.telemetryCollectionRunId,
        collectedAt: isoUtcStringToDateTimeLocal(telemetrySnapshotData.collectedAt) ?? '',
        isOnline: telemetrySnapshotData.isOnline ?? false,
        uptimeSeconds: telemetrySnapshotData.uptimeSeconds?.toString() ?? '',
        memoryWorkingSetMB: telemetrySnapshotData.memoryWorkingSetMB?.toString() ?? '',
        memoryGcHeapMB: telemetrySnapshotData.memoryGcHeapMB?.toString() ?? '',
        memoryPercent: telemetrySnapshotData.memoryPercent?.toString() ?? '',
        systemMemoryPercent: telemetrySnapshotData.systemMemoryPercent?.toString() ?? '',
        cpuPercent: telemetrySnapshotData.cpuPercent?.toString() ?? '',
        systemCpuPercent: telemetrySnapshotData.systemCpuPercent?.toString() ?? '',
        threadPoolWorkerThreads: telemetrySnapshotData.threadPoolWorkerThreads?.toString() ?? '',
        threadPoolCompletionPortThreads: telemetrySnapshotData.threadPoolCompletionPortThreads?.toString() ?? '',
        threadPoolPendingWorkItems: telemetrySnapshotData.threadPoolPendingWorkItems?.toString() ?? '',
        machineName: telemetrySnapshotData.machineName ?? '',
        dotNetVersion: telemetrySnapshotData.dotNetVersion ?? '',
        statusJson: telemetrySnapshotData.statusJson ?? '',
      }, { emitEvent: false});
    }

    this.telemetrySnapshotForm.markAsPristine();
    this.telemetrySnapshotForm.markAsUntouched();
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


  public userIsTelemetryTelemetrySnapshotReader(): boolean {
    return this.telemetrySnapshotService.userIsTelemetryTelemetrySnapshotReader();
  }

  public userIsTelemetryTelemetrySnapshotWriter(): boolean {
    return this.telemetrySnapshotService.userIsTelemetryTelemetrySnapshotWriter();
  }
}
