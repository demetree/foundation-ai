/*
   GENERATED FORM FOR THE SCHEDULEDEVENTTEMPLATECHARGECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventTemplateChargeChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-template-charge-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventTemplateChargeChangeHistoryService, ScheduledEventTemplateChargeChangeHistoryData, ScheduledEventTemplateChargeChangeHistorySubmitData } from '../../../scheduler-data-services/scheduled-event-template-charge-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduledEventTemplateChargeService } from '../../../scheduler-data-services/scheduled-event-template-charge.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ScheduledEventTemplateChargeChangeHistoryFormValues {
  scheduledEventTemplateChargeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-scheduled-event-template-charge-change-history-add-edit',
  templateUrl: './scheduled-event-template-charge-change-history-add-edit.component.html',
  styleUrls: ['./scheduled-event-template-charge-change-history-add-edit.component.scss']
})
export class ScheduledEventTemplateChargeChangeHistoryAddEditComponent {
  @ViewChild('scheduledEventTemplateChargeChangeHistoryModal') scheduledEventTemplateChargeChangeHistoryModal!: TemplateRef<any>;
  @Output() scheduledEventTemplateChargeChangeHistoryChanged = new Subject<ScheduledEventTemplateChargeChangeHistoryData[]>();
  @Input() scheduledEventTemplateChargeChangeHistorySubmitData: ScheduledEventTemplateChargeChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventTemplateChargeChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventTemplateChargeChangeHistoryForm: FormGroup = this.fb.group({
        scheduledEventTemplateChargeId: [null, Validators.required],
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

  scheduledEventTemplateChargeChangeHistories$ = this.scheduledEventTemplateChargeChangeHistoryService.GetScheduledEventTemplateChargeChangeHistoryList();
  scheduledEventTemplateCharges$ = this.scheduledEventTemplateChargeService.GetScheduledEventTemplateChargeList();

  constructor(
    private modalService: NgbModal,
    private scheduledEventTemplateChargeChangeHistoryService: ScheduledEventTemplateChargeChangeHistoryService,
    private scheduledEventTemplateChargeService: ScheduledEventTemplateChargeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(scheduledEventTemplateChargeChangeHistoryData?: ScheduledEventTemplateChargeChangeHistoryData) {

    if (scheduledEventTemplateChargeChangeHistoryData != null) {

      if (!this.scheduledEventTemplateChargeChangeHistoryService.userIsSchedulerScheduledEventTemplateChargeChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduled Event Template Charge Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.scheduledEventTemplateChargeChangeHistorySubmitData = this.scheduledEventTemplateChargeChangeHistoryService.ConvertToScheduledEventTemplateChargeChangeHistorySubmitData(scheduledEventTemplateChargeChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(scheduledEventTemplateChargeChangeHistoryData);

    } else {

      if (!this.scheduledEventTemplateChargeChangeHistoryService.userIsSchedulerScheduledEventTemplateChargeChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Template Charge Change Histories`,
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
        this.scheduledEventTemplateChargeChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventTemplateChargeChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.scheduledEventTemplateChargeChangeHistoryModal, {
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

    if (this.scheduledEventTemplateChargeChangeHistoryService.userIsSchedulerScheduledEventTemplateChargeChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Template Charge Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.scheduledEventTemplateChargeChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventTemplateChargeChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventTemplateChargeChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventTemplateChargeChangeHistorySubmitData: ScheduledEventTemplateChargeChangeHistorySubmitData = {
        id: this.scheduledEventTemplateChargeChangeHistorySubmitData?.id || 0,
        scheduledEventTemplateChargeId: Number(formValue.scheduledEventTemplateChargeId),
        versionNumber: this.scheduledEventTemplateChargeChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateScheduledEventTemplateChargeChangeHistory(scheduledEventTemplateChargeChangeHistorySubmitData);
      } else {
        this.addScheduledEventTemplateChargeChangeHistory(scheduledEventTemplateChargeChangeHistorySubmitData);
      }
  }

  private addScheduledEventTemplateChargeChangeHistory(scheduledEventTemplateChargeChangeHistoryData: ScheduledEventTemplateChargeChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    scheduledEventTemplateChargeChangeHistoryData.versionNumber = 0;
    this.scheduledEventTemplateChargeChangeHistoryService.PostScheduledEventTemplateChargeChangeHistory(scheduledEventTemplateChargeChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newScheduledEventTemplateChargeChangeHistory) => {

        this.scheduledEventTemplateChargeChangeHistoryService.ClearAllCaches();

        this.scheduledEventTemplateChargeChangeHistoryChanged.next([newScheduledEventTemplateChargeChangeHistory]);

        this.alertService.showMessage("Scheduled Event Template Charge Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/scheduledeventtemplatechargechangehistory', newScheduledEventTemplateChargeChangeHistory.id]);
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
                                   'You do not have permission to save this Scheduled Event Template Charge Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Template Charge Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Template Charge Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateScheduledEventTemplateChargeChangeHistory(scheduledEventTemplateChargeChangeHistoryData: ScheduledEventTemplateChargeChangeHistorySubmitData) {
    this.scheduledEventTemplateChargeChangeHistoryService.PutScheduledEventTemplateChargeChangeHistory(scheduledEventTemplateChargeChangeHistoryData.id, scheduledEventTemplateChargeChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedScheduledEventTemplateChargeChangeHistory) => {

        this.scheduledEventTemplateChargeChangeHistoryService.ClearAllCaches();

        this.scheduledEventTemplateChargeChangeHistoryChanged.next([updatedScheduledEventTemplateChargeChangeHistory]);

        this.alertService.showMessage("Scheduled Event Template Charge Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduled Event Template Charge Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Template Charge Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Template Charge Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(scheduledEventTemplateChargeChangeHistoryData: ScheduledEventTemplateChargeChangeHistoryData | null) {

    if (scheduledEventTemplateChargeChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventTemplateChargeChangeHistoryForm.reset({
        scheduledEventTemplateChargeId: null,
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
        this.scheduledEventTemplateChargeChangeHistoryForm.reset({
        scheduledEventTemplateChargeId: scheduledEventTemplateChargeChangeHistoryData.scheduledEventTemplateChargeId,
        versionNumber: scheduledEventTemplateChargeChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(scheduledEventTemplateChargeChangeHistoryData.timeStamp) ?? '',
        userId: scheduledEventTemplateChargeChangeHistoryData.userId?.toString() ?? '',
        data: scheduledEventTemplateChargeChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.scheduledEventTemplateChargeChangeHistoryForm.markAsPristine();
    this.scheduledEventTemplateChargeChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerScheduledEventTemplateChargeChangeHistoryReader(): boolean {
    return this.scheduledEventTemplateChargeChangeHistoryService.userIsSchedulerScheduledEventTemplateChargeChangeHistoryReader();
  }

  public userIsSchedulerScheduledEventTemplateChargeChangeHistoryWriter(): boolean {
    return this.scheduledEventTemplateChargeChangeHistoryService.userIsSchedulerScheduledEventTemplateChargeChangeHistoryWriter();
  }
}
