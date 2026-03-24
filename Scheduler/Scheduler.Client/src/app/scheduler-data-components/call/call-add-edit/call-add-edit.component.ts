/*
   GENERATED FORM FOR THE CALL TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Call table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to call-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CallService, CallData, CallSubmitData } from '../../../scheduler-data-services/call.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { CallTypeService } from '../../../scheduler-data-services/call-type.service';
import { CallStatusService } from '../../../scheduler-data-services/call-status.service';
import { ConversationService } from '../../../scheduler-data-services/conversation.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface CallFormValues {
  callTypeId: number | bigint,       // For FK link number
  callStatusId: number | bigint,       // For FK link number
  providerId: string,
  providerCallId: string | null,
  conversationId: number | bigint,       // For FK link number
  initiatorUserId: string,     // Stored as string for form input, converted to number on submit.
  startDateTime: string,
  answerDateTime: string | null,
  endDateTime: string | null,
  durationSeconds: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-call-add-edit',
  templateUrl: './call-add-edit.component.html',
  styleUrls: ['./call-add-edit.component.scss']
})
export class CallAddEditComponent {
  @ViewChild('callModal') callModal!: TemplateRef<any>;
  @Output() callChanged = new Subject<CallData[]>();
  @Input() callSubmitData: CallSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CallFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public callForm: FormGroup = this.fb.group({
        callTypeId: [null, Validators.required],
        callStatusId: [null, Validators.required],
        providerId: ['', Validators.required],
        providerCallId: [''],
        conversationId: [null, Validators.required],
        initiatorUserId: ['', Validators.required],
        startDateTime: ['', Validators.required],
        answerDateTime: [''],
        endDateTime: [''],
        durationSeconds: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  calls$ = this.callService.GetCallList();
  callTypes$ = this.callTypeService.GetCallTypeList();
  callStatuses$ = this.callStatusService.GetCallStatusList();
  conversations$ = this.conversationService.GetConversationList();

  constructor(
    private modalService: NgbModal,
    private callService: CallService,
    private callTypeService: CallTypeService,
    private callStatusService: CallStatusService,
    private conversationService: ConversationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(callData?: CallData) {

    if (callData != null) {

      if (!this.callService.userIsSchedulerCallReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Calls`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.callSubmitData = this.callService.ConvertToCallSubmitData(callData);
      this.isEditMode = true;
      this.objectGuid = callData.objectGuid;

      this.buildFormValues(callData);

    } else {

      if (!this.callService.userIsSchedulerCallWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Calls`,
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
        this.callForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.callForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.callModal, {
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

    if (this.callService.userIsSchedulerCallWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Calls`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.callForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.callForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.callForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const callSubmitData: CallSubmitData = {
        id: this.callSubmitData?.id || 0,
        callTypeId: Number(formValue.callTypeId),
        callStatusId: Number(formValue.callStatusId),
        providerId: formValue.providerId!.trim(),
        providerCallId: formValue.providerCallId?.trim() || null,
        conversationId: Number(formValue.conversationId),
        initiatorUserId: Number(formValue.initiatorUserId),
        startDateTime: dateTimeLocalToIsoUtc(formValue.startDateTime!.trim())!,
        answerDateTime: formValue.answerDateTime ? dateTimeLocalToIsoUtc(formValue.answerDateTime.trim()) : null,
        endDateTime: formValue.endDateTime ? dateTimeLocalToIsoUtc(formValue.endDateTime.trim()) : null,
        durationSeconds: formValue.durationSeconds ? Number(formValue.durationSeconds) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateCall(callSubmitData);
      } else {
        this.addCall(callSubmitData);
      }
  }

  private addCall(callData: CallSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    callData.active = true;
    callData.deleted = false;
    this.callService.PostCall(callData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCall) => {

        this.callService.ClearAllCaches();

        this.callChanged.next([newCall]);

        this.alertService.showMessage("Call added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/call', newCall.id]);
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
                                   'You do not have permission to save this Call.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCall(callData: CallSubmitData) {
    this.callService.PutCall(callData.id, callData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCall) => {

        this.callService.ClearAllCaches();

        this.callChanged.next([updatedCall]);

        this.alertService.showMessage("Call updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Call.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(callData: CallData | null) {

    if (callData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.callForm.reset({
        callTypeId: null,
        callStatusId: null,
        providerId: '',
        providerCallId: '',
        conversationId: null,
        initiatorUserId: '',
        startDateTime: '',
        answerDateTime: '',
        endDateTime: '',
        durationSeconds: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.callForm.reset({
        callTypeId: callData.callTypeId,
        callStatusId: callData.callStatusId,
        providerId: callData.providerId ?? '',
        providerCallId: callData.providerCallId ?? '',
        conversationId: callData.conversationId,
        initiatorUserId: callData.initiatorUserId?.toString() ?? '',
        startDateTime: isoUtcStringToDateTimeLocal(callData.startDateTime) ?? '',
        answerDateTime: isoUtcStringToDateTimeLocal(callData.answerDateTime) ?? '',
        endDateTime: isoUtcStringToDateTimeLocal(callData.endDateTime) ?? '',
        durationSeconds: callData.durationSeconds?.toString() ?? '',
        active: callData.active ?? true,
        deleted: callData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.callForm.markAsPristine();
    this.callForm.markAsUntouched();
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


  public userIsSchedulerCallReader(): boolean {
    return this.callService.userIsSchedulerCallReader();
  }

  public userIsSchedulerCallWriter(): boolean {
    return this.callService.userIsSchedulerCallWriter();
  }
}
