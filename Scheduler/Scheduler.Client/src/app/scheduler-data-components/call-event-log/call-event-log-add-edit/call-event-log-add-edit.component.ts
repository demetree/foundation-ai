/*
   GENERATED FORM FOR THE CALLEVENTLOG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CallEventLog table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to call-event-log-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CallEventLogService, CallEventLogData, CallEventLogSubmitData } from '../../../scheduler-data-services/call-event-log.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { CallService } from '../../../scheduler-data-services/call.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface CallEventLogFormValues {
  callId: number | bigint,       // For FK link number
  eventType: string,
  userId: string | null,     // Stored as string for form input, converted to number on submit.
  providerId: string | null,
  metadata: string | null,
  dateTimeCreated: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-call-event-log-add-edit',
  templateUrl: './call-event-log-add-edit.component.html',
  styleUrls: ['./call-event-log-add-edit.component.scss']
})
export class CallEventLogAddEditComponent {
  @ViewChild('callEventLogModal') callEventLogModal!: TemplateRef<any>;
  @Output() callEventLogChanged = new Subject<CallEventLogData[]>();
  @Input() callEventLogSubmitData: CallEventLogSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CallEventLogFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public callEventLogForm: FormGroup = this.fb.group({
        callId: [null, Validators.required],
        eventType: ['', Validators.required],
        userId: [''],
        providerId: [''],
        metadata: [''],
        dateTimeCreated: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  callEventLogs$ = this.callEventLogService.GetCallEventLogList();
  calls$ = this.callService.GetCallList();

  constructor(
    private modalService: NgbModal,
    private callEventLogService: CallEventLogService,
    private callService: CallService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(callEventLogData?: CallEventLogData) {

    if (callEventLogData != null) {

      if (!this.callEventLogService.userIsSchedulerCallEventLogReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Call Event Logs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.callEventLogSubmitData = this.callEventLogService.ConvertToCallEventLogSubmitData(callEventLogData);
      this.isEditMode = true;
      this.objectGuid = callEventLogData.objectGuid;

      this.buildFormValues(callEventLogData);

    } else {

      if (!this.callEventLogService.userIsSchedulerCallEventLogWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Call Event Logs`,
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
        this.callEventLogForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.callEventLogForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.callEventLogModal, {
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

    if (this.callEventLogService.userIsSchedulerCallEventLogWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Call Event Logs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.callEventLogForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.callEventLogForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.callEventLogForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const callEventLogSubmitData: CallEventLogSubmitData = {
        id: this.callEventLogSubmitData?.id || 0,
        callId: Number(formValue.callId),
        eventType: formValue.eventType!.trim(),
        userId: formValue.userId ? Number(formValue.userId) : null,
        providerId: formValue.providerId?.trim() || null,
        metadata: formValue.metadata?.trim() || null,
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateCallEventLog(callEventLogSubmitData);
      } else {
        this.addCallEventLog(callEventLogSubmitData);
      }
  }

  private addCallEventLog(callEventLogData: CallEventLogSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    callEventLogData.active = true;
    callEventLogData.deleted = false;
    this.callEventLogService.PostCallEventLog(callEventLogData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCallEventLog) => {

        this.callEventLogService.ClearAllCaches();

        this.callEventLogChanged.next([newCallEventLog]);

        this.alertService.showMessage("Call Event Log added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/calleventlog', newCallEventLog.id]);
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
                                   'You do not have permission to save this Call Event Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call Event Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call Event Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCallEventLog(callEventLogData: CallEventLogSubmitData) {
    this.callEventLogService.PutCallEventLog(callEventLogData.id, callEventLogData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCallEventLog) => {

        this.callEventLogService.ClearAllCaches();

        this.callEventLogChanged.next([updatedCallEventLog]);

        this.alertService.showMessage("Call Event Log updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Call Event Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call Event Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call Event Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(callEventLogData: CallEventLogData | null) {

    if (callEventLogData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.callEventLogForm.reset({
        callId: null,
        eventType: '',
        userId: '',
        providerId: '',
        metadata: '',
        dateTimeCreated: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.callEventLogForm.reset({
        callId: callEventLogData.callId,
        eventType: callEventLogData.eventType ?? '',
        userId: callEventLogData.userId?.toString() ?? '',
        providerId: callEventLogData.providerId ?? '',
        metadata: callEventLogData.metadata ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(callEventLogData.dateTimeCreated) ?? '',
        active: callEventLogData.active ?? true,
        deleted: callEventLogData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.callEventLogForm.markAsPristine();
    this.callEventLogForm.markAsUntouched();
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


  public userIsSchedulerCallEventLogReader(): boolean {
    return this.callEventLogService.userIsSchedulerCallEventLogReader();
  }

  public userIsSchedulerCallEventLogWriter(): boolean {
    return this.callEventLogService.userIsSchedulerCallEventLogWriter();
  }
}
