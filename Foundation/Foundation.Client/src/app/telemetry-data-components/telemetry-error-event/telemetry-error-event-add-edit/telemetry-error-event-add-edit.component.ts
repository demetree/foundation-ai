/*
   GENERATED FORM FOR THE TELEMETRYERROREVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryErrorEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-error-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryErrorEventService, TelemetryErrorEventData, TelemetryErrorEventSubmitData } from '../../../telemetry-data-services/telemetry-error-event.service';
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
interface TelemetryErrorEventFormValues {
  telemetryApplicationId: number | bigint,       // For FK link number
  telemetrySnapshotId: number | bigint | null,       // For FK link number
  originalAuditEventId: string | null,     // Stored as string for form input, converted to number on submit.
  occurredAt: string,
  auditTypeName: string | null,
  moduleName: string | null,
  entityName: string | null,
  userName: string | null,
  message: string | null,
  errorMessage: string | null,
};

@Component({
  selector: 'app-telemetry-error-event-add-edit',
  templateUrl: './telemetry-error-event-add-edit.component.html',
  styleUrls: ['./telemetry-error-event-add-edit.component.scss']
})
export class TelemetryErrorEventAddEditComponent {
  @ViewChild('telemetryErrorEventModal') telemetryErrorEventModal!: TemplateRef<any>;
  @Output() telemetryErrorEventChanged = new Subject<TelemetryErrorEventData[]>();
  @Input() telemetryErrorEventSubmitData: TelemetryErrorEventSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryErrorEventFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryErrorEventForm: FormGroup = this.fb.group({
        telemetryApplicationId: [null, Validators.required],
        telemetrySnapshotId: [null],
        originalAuditEventId: [''],
        occurredAt: ['', Validators.required],
        auditTypeName: [''],
        moduleName: [''],
        entityName: [''],
        userName: [''],
        message: [''],
        errorMessage: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  telemetryErrorEvents$ = this.telemetryErrorEventService.GetTelemetryErrorEventList();
  telemetryApplications$ = this.telemetryApplicationService.GetTelemetryApplicationList();
  telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  constructor(
    private modalService: NgbModal,
    private telemetryErrorEventService: TelemetryErrorEventService,
    private telemetryApplicationService: TelemetryApplicationService,
    private telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(telemetryErrorEventData?: TelemetryErrorEventData) {

    if (telemetryErrorEventData != null) {

      if (!this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Telemetry Error Events`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.telemetryErrorEventSubmitData = this.telemetryErrorEventService.ConvertToTelemetryErrorEventSubmitData(telemetryErrorEventData);
      this.isEditMode = true;

      this.buildFormValues(telemetryErrorEventData);

    } else {

      if (!this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Telemetry Error Events`,
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
        this.telemetryErrorEventForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryErrorEventForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.telemetryErrorEventModal, {
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

    if (this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Telemetry Error Events`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.telemetryErrorEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryErrorEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryErrorEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryErrorEventSubmitData: TelemetryErrorEventSubmitData = {
        id: this.telemetryErrorEventSubmitData?.id || 0,
        telemetryApplicationId: Number(formValue.telemetryApplicationId),
        telemetrySnapshotId: formValue.telemetrySnapshotId ? Number(formValue.telemetrySnapshotId) : null,
        originalAuditEventId: formValue.originalAuditEventId ? Number(formValue.originalAuditEventId) : null,
        occurredAt: dateTimeLocalToIsoUtc(formValue.occurredAt!.trim())!,
        auditTypeName: formValue.auditTypeName?.trim() || null,
        moduleName: formValue.moduleName?.trim() || null,
        entityName: formValue.entityName?.trim() || null,
        userName: formValue.userName?.trim() || null,
        message: formValue.message?.trim() || null,
        errorMessage: formValue.errorMessage?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateTelemetryErrorEvent(telemetryErrorEventSubmitData);
      } else {
        this.addTelemetryErrorEvent(telemetryErrorEventSubmitData);
      }
  }

  private addTelemetryErrorEvent(telemetryErrorEventData: TelemetryErrorEventSubmitData) {
    this.telemetryErrorEventService.PostTelemetryErrorEvent(telemetryErrorEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTelemetryErrorEvent) => {

        this.telemetryErrorEventService.ClearAllCaches();

        this.telemetryErrorEventChanged.next([newTelemetryErrorEvent]);

        this.alertService.showMessage("Telemetry Error Event added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/telemetryerrorevent', newTelemetryErrorEvent.id]);
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
                                   'You do not have permission to save this Telemetry Error Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Error Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Error Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTelemetryErrorEvent(telemetryErrorEventData: TelemetryErrorEventSubmitData) {
    this.telemetryErrorEventService.PutTelemetryErrorEvent(telemetryErrorEventData.id, telemetryErrorEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTelemetryErrorEvent) => {

        this.telemetryErrorEventService.ClearAllCaches();

        this.telemetryErrorEventChanged.next([updatedTelemetryErrorEvent]);

        this.alertService.showMessage("Telemetry Error Event updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Telemetry Error Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Error Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Error Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(telemetryErrorEventData: TelemetryErrorEventData | null) {

    if (telemetryErrorEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryErrorEventForm.reset({
        telemetryApplicationId: null,
        telemetrySnapshotId: null,
        originalAuditEventId: '',
        occurredAt: '',
        auditTypeName: '',
        moduleName: '',
        entityName: '',
        userName: '',
        message: '',
        errorMessage: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryErrorEventForm.reset({
        telemetryApplicationId: telemetryErrorEventData.telemetryApplicationId,
        telemetrySnapshotId: telemetryErrorEventData.telemetrySnapshotId,
        originalAuditEventId: telemetryErrorEventData.originalAuditEventId?.toString() ?? '',
        occurredAt: isoUtcStringToDateTimeLocal(telemetryErrorEventData.occurredAt) ?? '',
        auditTypeName: telemetryErrorEventData.auditTypeName ?? '',
        moduleName: telemetryErrorEventData.moduleName ?? '',
        entityName: telemetryErrorEventData.entityName ?? '',
        userName: telemetryErrorEventData.userName ?? '',
        message: telemetryErrorEventData.message ?? '',
        errorMessage: telemetryErrorEventData.errorMessage ?? '',
      }, { emitEvent: false});
    }

    this.telemetryErrorEventForm.markAsPristine();
    this.telemetryErrorEventForm.markAsUntouched();
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


  public userIsTelemetryTelemetryErrorEventReader(): boolean {
    return this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventReader();
  }

  public userIsTelemetryTelemetryErrorEventWriter(): boolean {
    return this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventWriter();
  }
}
