/*
   GENERATED FORM FOR THE CALLSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CallStatus table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to call-status-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CallStatusService, CallStatusData, CallStatusSubmitData } from '../../../scheduler-data-services/call-status.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface CallStatusFormValues {
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-call-status-add-edit',
  templateUrl: './call-status-add-edit.component.html',
  styleUrls: ['./call-status-add-edit.component.scss']
})
export class CallStatusAddEditComponent {
  @ViewChild('callStatusModal') callStatusModal!: TemplateRef<any>;
  @Output() callStatusChanged = new Subject<CallStatusData[]>();
  @Input() callStatusSubmitData: CallStatusSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CallStatusFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public callStatusForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  callStatuses$ = this.callStatusService.GetCallStatusList();

  constructor(
    private modalService: NgbModal,
    private callStatusService: CallStatusService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(callStatusData?: CallStatusData) {

    if (callStatusData != null) {

      if (!this.callStatusService.userIsSchedulerCallStatusReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Call Statuses`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.callStatusSubmitData = this.callStatusService.ConvertToCallStatusSubmitData(callStatusData);
      this.isEditMode = true;
      this.objectGuid = callStatusData.objectGuid;

      this.buildFormValues(callStatusData);

    } else {

      if (!this.callStatusService.userIsSchedulerCallStatusWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Call Statuses`,
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
        this.callStatusForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.callStatusForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.callStatusModal, {
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

    if (this.callStatusService.userIsSchedulerCallStatusWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Call Statuses`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.callStatusForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.callStatusForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.callStatusForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const callStatusSubmitData: CallStatusSubmitData = {
        id: this.callStatusSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateCallStatus(callStatusSubmitData);
      } else {
        this.addCallStatus(callStatusSubmitData);
      }
  }

  private addCallStatus(callStatusData: CallStatusSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    callStatusData.active = true;
    callStatusData.deleted = false;
    this.callStatusService.PostCallStatus(callStatusData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCallStatus) => {

        this.callStatusService.ClearAllCaches();

        this.callStatusChanged.next([newCallStatus]);

        this.alertService.showMessage("Call Status added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/callstatus', newCallStatus.id]);
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
                                   'You do not have permission to save this Call Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCallStatus(callStatusData: CallStatusSubmitData) {
    this.callStatusService.PutCallStatus(callStatusData.id, callStatusData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCallStatus) => {

        this.callStatusService.ClearAllCaches();

        this.callStatusChanged.next([updatedCallStatus]);

        this.alertService.showMessage("Call Status updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Call Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(callStatusData: CallStatusData | null) {

    if (callStatusData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.callStatusForm.reset({
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.callStatusForm.reset({
        name: callStatusData.name ?? '',
        description: callStatusData.description ?? '',
        active: callStatusData.active ?? true,
        deleted: callStatusData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.callStatusForm.markAsPristine();
    this.callStatusForm.markAsUntouched();
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


  public userIsSchedulerCallStatusReader(): boolean {
    return this.callStatusService.userIsSchedulerCallStatusReader();
  }

  public userIsSchedulerCallStatusWriter(): boolean {
    return this.callStatusService.userIsSchedulerCallStatusWriter();
  }
}
