/*
   GENERATED FORM FOR THE ONCALLSCHEDULECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from OnCallScheduleChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to on-call-schedule-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OnCallScheduleChangeHistoryService, OnCallScheduleChangeHistoryData, OnCallScheduleChangeHistorySubmitData } from '../../../alerting-data-services/on-call-schedule-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OnCallScheduleService } from '../../../alerting-data-services/on-call-schedule.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface OnCallScheduleChangeHistoryFormValues {
  onCallScheduleId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-on-call-schedule-change-history-add-edit',
  templateUrl: './on-call-schedule-change-history-add-edit.component.html',
  styleUrls: ['./on-call-schedule-change-history-add-edit.component.scss']
})
export class OnCallScheduleChangeHistoryAddEditComponent {
  @ViewChild('onCallScheduleChangeHistoryModal') onCallScheduleChangeHistoryModal!: TemplateRef<any>;
  @Output() onCallScheduleChangeHistoryChanged = new Subject<OnCallScheduleChangeHistoryData[]>();
  @Input() onCallScheduleChangeHistorySubmitData: OnCallScheduleChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<OnCallScheduleChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public onCallScheduleChangeHistoryForm: FormGroup = this.fb.group({
        onCallScheduleId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  onCallScheduleChangeHistories$ = this.onCallScheduleChangeHistoryService.GetOnCallScheduleChangeHistoryList();
  onCallSchedules$ = this.onCallScheduleService.GetOnCallScheduleList();

  constructor(
    private modalService: NgbModal,
    private onCallScheduleChangeHistoryService: OnCallScheduleChangeHistoryService,
    private onCallScheduleService: OnCallScheduleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(onCallScheduleChangeHistoryData?: OnCallScheduleChangeHistoryData) {

    if (onCallScheduleChangeHistoryData != null) {

      if (!this.onCallScheduleChangeHistoryService.userIsAlertingOnCallScheduleChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read On Call Schedule Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.onCallScheduleChangeHistorySubmitData = this.onCallScheduleChangeHistoryService.ConvertToOnCallScheduleChangeHistorySubmitData(onCallScheduleChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(onCallScheduleChangeHistoryData);

    } else {

      if (!this.onCallScheduleChangeHistoryService.userIsAlertingOnCallScheduleChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write On Call Schedule Change Histories`,
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
        this.onCallScheduleChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.onCallScheduleChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.onCallScheduleChangeHistoryModal, {
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

    if (this.onCallScheduleChangeHistoryService.userIsAlertingOnCallScheduleChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write On Call Schedule Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.onCallScheduleChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.onCallScheduleChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.onCallScheduleChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const onCallScheduleChangeHistorySubmitData: OnCallScheduleChangeHistorySubmitData = {
        id: this.onCallScheduleChangeHistorySubmitData?.id || 0,
        onCallScheduleId: Number(formValue.onCallScheduleId),
        versionNumber: this.onCallScheduleChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateOnCallScheduleChangeHistory(onCallScheduleChangeHistorySubmitData);
      } else {
        this.addOnCallScheduleChangeHistory(onCallScheduleChangeHistorySubmitData);
      }
  }

  private addOnCallScheduleChangeHistory(onCallScheduleChangeHistoryData: OnCallScheduleChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    onCallScheduleChangeHistoryData.versionNumber = 0;
    this.onCallScheduleChangeHistoryService.PostOnCallScheduleChangeHistory(onCallScheduleChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newOnCallScheduleChangeHistory) => {

        this.onCallScheduleChangeHistoryService.ClearAllCaches();

        this.onCallScheduleChangeHistoryChanged.next([newOnCallScheduleChangeHistory]);

        this.alertService.showMessage("On Call Schedule Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/oncallschedulechangehistory', newOnCallScheduleChangeHistory.id]);
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
                                   'You do not have permission to save this On Call Schedule Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the On Call Schedule Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('On Call Schedule Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateOnCallScheduleChangeHistory(onCallScheduleChangeHistoryData: OnCallScheduleChangeHistorySubmitData) {
    this.onCallScheduleChangeHistoryService.PutOnCallScheduleChangeHistory(onCallScheduleChangeHistoryData.id, onCallScheduleChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedOnCallScheduleChangeHistory) => {

        this.onCallScheduleChangeHistoryService.ClearAllCaches();

        this.onCallScheduleChangeHistoryChanged.next([updatedOnCallScheduleChangeHistory]);

        this.alertService.showMessage("On Call Schedule Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this On Call Schedule Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the On Call Schedule Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('On Call Schedule Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(onCallScheduleChangeHistoryData: OnCallScheduleChangeHistoryData | null) {

    if (onCallScheduleChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.onCallScheduleChangeHistoryForm.reset({
        onCallScheduleId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.onCallScheduleChangeHistoryForm.reset({
        onCallScheduleId: onCallScheduleChangeHistoryData.onCallScheduleId,
        versionNumber: onCallScheduleChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(onCallScheduleChangeHistoryData.timeStamp) ?? '',
        userId: onCallScheduleChangeHistoryData.userId?.toString() ?? '',
        data: onCallScheduleChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.onCallScheduleChangeHistoryForm.markAsPristine();
    this.onCallScheduleChangeHistoryForm.markAsUntouched();
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


  public userIsAlertingOnCallScheduleChangeHistoryReader(): boolean {
    return this.onCallScheduleChangeHistoryService.userIsAlertingOnCallScheduleChangeHistoryReader();
  }

  public userIsAlertingOnCallScheduleChangeHistoryWriter(): boolean {
    return this.onCallScheduleChangeHistoryService.userIsAlertingOnCallScheduleChangeHistoryWriter();
  }
}
