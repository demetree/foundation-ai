/*
   GENERATED FORM FOR THE CALLPARTICIPANT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CallParticipant table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to call-participant-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CallParticipantService, CallParticipantData, CallParticipantSubmitData } from '../../../scheduler-data-services/call-participant.service';
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
interface CallParticipantFormValues {
  callId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  role: string,
  status: string,
  joinedDateTime: string | null,
  leftDateTime: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-call-participant-add-edit',
  templateUrl: './call-participant-add-edit.component.html',
  styleUrls: ['./call-participant-add-edit.component.scss']
})
export class CallParticipantAddEditComponent {
  @ViewChild('callParticipantModal') callParticipantModal!: TemplateRef<any>;
  @Output() callParticipantChanged = new Subject<CallParticipantData[]>();
  @Input() callParticipantSubmitData: CallParticipantSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CallParticipantFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public callParticipantForm: FormGroup = this.fb.group({
        callId: [null, Validators.required],
        userId: ['', Validators.required],
        role: ['', Validators.required],
        status: ['', Validators.required],
        joinedDateTime: [''],
        leftDateTime: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  callParticipants$ = this.callParticipantService.GetCallParticipantList();
  calls$ = this.callService.GetCallList();

  constructor(
    private modalService: NgbModal,
    private callParticipantService: CallParticipantService,
    private callService: CallService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(callParticipantData?: CallParticipantData) {

    if (callParticipantData != null) {

      if (!this.callParticipantService.userIsSchedulerCallParticipantReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Call Participants`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.callParticipantSubmitData = this.callParticipantService.ConvertToCallParticipantSubmitData(callParticipantData);
      this.isEditMode = true;
      this.objectGuid = callParticipantData.objectGuid;

      this.buildFormValues(callParticipantData);

    } else {

      if (!this.callParticipantService.userIsSchedulerCallParticipantWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Call Participants`,
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
        this.callParticipantForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.callParticipantForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.callParticipantModal, {
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

    if (this.callParticipantService.userIsSchedulerCallParticipantWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Call Participants`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.callParticipantForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.callParticipantForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.callParticipantForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const callParticipantSubmitData: CallParticipantSubmitData = {
        id: this.callParticipantSubmitData?.id || 0,
        callId: Number(formValue.callId),
        userId: Number(formValue.userId),
        role: formValue.role!.trim(),
        status: formValue.status!.trim(),
        joinedDateTime: formValue.joinedDateTime ? dateTimeLocalToIsoUtc(formValue.joinedDateTime.trim()) : null,
        leftDateTime: formValue.leftDateTime ? dateTimeLocalToIsoUtc(formValue.leftDateTime.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateCallParticipant(callParticipantSubmitData);
      } else {
        this.addCallParticipant(callParticipantSubmitData);
      }
  }

  private addCallParticipant(callParticipantData: CallParticipantSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    callParticipantData.active = true;
    callParticipantData.deleted = false;
    this.callParticipantService.PostCallParticipant(callParticipantData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCallParticipant) => {

        this.callParticipantService.ClearAllCaches();

        this.callParticipantChanged.next([newCallParticipant]);

        this.alertService.showMessage("Call Participant added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/callparticipant', newCallParticipant.id]);
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
                                   'You do not have permission to save this Call Participant.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call Participant.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call Participant could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCallParticipant(callParticipantData: CallParticipantSubmitData) {
    this.callParticipantService.PutCallParticipant(callParticipantData.id, callParticipantData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCallParticipant) => {

        this.callParticipantService.ClearAllCaches();

        this.callParticipantChanged.next([updatedCallParticipant]);

        this.alertService.showMessage("Call Participant updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Call Participant.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call Participant.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call Participant could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(callParticipantData: CallParticipantData | null) {

    if (callParticipantData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.callParticipantForm.reset({
        callId: null,
        userId: '',
        role: '',
        status: '',
        joinedDateTime: '',
        leftDateTime: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.callParticipantForm.reset({
        callId: callParticipantData.callId,
        userId: callParticipantData.userId?.toString() ?? '',
        role: callParticipantData.role ?? '',
        status: callParticipantData.status ?? '',
        joinedDateTime: isoUtcStringToDateTimeLocal(callParticipantData.joinedDateTime) ?? '',
        leftDateTime: isoUtcStringToDateTimeLocal(callParticipantData.leftDateTime) ?? '',
        active: callParticipantData.active ?? true,
        deleted: callParticipantData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.callParticipantForm.markAsPristine();
    this.callParticipantForm.markAsUntouched();
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


  public userIsSchedulerCallParticipantReader(): boolean {
    return this.callParticipantService.userIsSchedulerCallParticipantReader();
  }

  public userIsSchedulerCallParticipantWriter(): boolean {
    return this.callParticipantService.userIsSchedulerCallParticipantWriter();
  }
}
