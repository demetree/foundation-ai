/*
   GENERATED FORM FOR THE SCHEDULEDEVENTQUALIFICATIONREQUIREMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventQualificationRequirementChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-qualification-requirement-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventQualificationRequirementChangeHistoryService, ScheduledEventQualificationRequirementChangeHistoryData, ScheduledEventQualificationRequirementChangeHistorySubmitData } from '../../../scheduler-data-services/scheduled-event-qualification-requirement-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduledEventQualificationRequirementService } from '../../../scheduler-data-services/scheduled-event-qualification-requirement.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ScheduledEventQualificationRequirementChangeHistoryFormValues {
  scheduledEventQualificationRequirementId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-scheduled-event-qualification-requirement-change-history-add-edit',
  templateUrl: './scheduled-event-qualification-requirement-change-history-add-edit.component.html',
  styleUrls: ['./scheduled-event-qualification-requirement-change-history-add-edit.component.scss']
})
export class ScheduledEventQualificationRequirementChangeHistoryAddEditComponent {
  @ViewChild('scheduledEventQualificationRequirementChangeHistoryModal') scheduledEventQualificationRequirementChangeHistoryModal!: TemplateRef<any>;
  @Output() scheduledEventQualificationRequirementChangeHistoryChanged = new Subject<ScheduledEventQualificationRequirementChangeHistoryData[]>();
  @Input() scheduledEventQualificationRequirementChangeHistorySubmitData: ScheduledEventQualificationRequirementChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventQualificationRequirementChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventQualificationRequirementChangeHistoryForm: FormGroup = this.fb.group({
        scheduledEventQualificationRequirementId: [null, Validators.required],
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

  scheduledEventQualificationRequirementChangeHistories$ = this.scheduledEventQualificationRequirementChangeHistoryService.GetScheduledEventQualificationRequirementChangeHistoryList();
  scheduledEventQualificationRequirements$ = this.scheduledEventQualificationRequirementService.GetScheduledEventQualificationRequirementList();

  constructor(
    private modalService: NgbModal,
    private scheduledEventQualificationRequirementChangeHistoryService: ScheduledEventQualificationRequirementChangeHistoryService,
    private scheduledEventQualificationRequirementService: ScheduledEventQualificationRequirementService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(scheduledEventQualificationRequirementChangeHistoryData?: ScheduledEventQualificationRequirementChangeHistoryData) {

    if (scheduledEventQualificationRequirementChangeHistoryData != null) {

      if (!this.scheduledEventQualificationRequirementChangeHistoryService.userIsSchedulerScheduledEventQualificationRequirementChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduled Event Qualification Requirement Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.scheduledEventQualificationRequirementChangeHistorySubmitData = this.scheduledEventQualificationRequirementChangeHistoryService.ConvertToScheduledEventQualificationRequirementChangeHistorySubmitData(scheduledEventQualificationRequirementChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(scheduledEventQualificationRequirementChangeHistoryData);

    } else {

      if (!this.scheduledEventQualificationRequirementChangeHistoryService.userIsSchedulerScheduledEventQualificationRequirementChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Qualification Requirement Change Histories`,
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
        this.scheduledEventQualificationRequirementChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventQualificationRequirementChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.scheduledEventQualificationRequirementChangeHistoryModal, {
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

    if (this.scheduledEventQualificationRequirementChangeHistoryService.userIsSchedulerScheduledEventQualificationRequirementChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Qualification Requirement Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.scheduledEventQualificationRequirementChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventQualificationRequirementChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventQualificationRequirementChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventQualificationRequirementChangeHistorySubmitData: ScheduledEventQualificationRequirementChangeHistorySubmitData = {
        id: this.scheduledEventQualificationRequirementChangeHistorySubmitData?.id || 0,
        scheduledEventQualificationRequirementId: Number(formValue.scheduledEventQualificationRequirementId),
        versionNumber: this.scheduledEventQualificationRequirementChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateScheduledEventQualificationRequirementChangeHistory(scheduledEventQualificationRequirementChangeHistorySubmitData);
      } else {
        this.addScheduledEventQualificationRequirementChangeHistory(scheduledEventQualificationRequirementChangeHistorySubmitData);
      }
  }

  private addScheduledEventQualificationRequirementChangeHistory(scheduledEventQualificationRequirementChangeHistoryData: ScheduledEventQualificationRequirementChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    scheduledEventQualificationRequirementChangeHistoryData.versionNumber = 0;
    this.scheduledEventQualificationRequirementChangeHistoryService.PostScheduledEventQualificationRequirementChangeHistory(scheduledEventQualificationRequirementChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newScheduledEventQualificationRequirementChangeHistory) => {

        this.scheduledEventQualificationRequirementChangeHistoryService.ClearAllCaches();

        this.scheduledEventQualificationRequirementChangeHistoryChanged.next([newScheduledEventQualificationRequirementChangeHistory]);

        this.alertService.showMessage("Scheduled Event Qualification Requirement Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/scheduledeventqualificationrequirementchangehistory', newScheduledEventQualificationRequirementChangeHistory.id]);
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
                                   'You do not have permission to save this Scheduled Event Qualification Requirement Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Qualification Requirement Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Qualification Requirement Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateScheduledEventQualificationRequirementChangeHistory(scheduledEventQualificationRequirementChangeHistoryData: ScheduledEventQualificationRequirementChangeHistorySubmitData) {
    this.scheduledEventQualificationRequirementChangeHistoryService.PutScheduledEventQualificationRequirementChangeHistory(scheduledEventQualificationRequirementChangeHistoryData.id, scheduledEventQualificationRequirementChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedScheduledEventQualificationRequirementChangeHistory) => {

        this.scheduledEventQualificationRequirementChangeHistoryService.ClearAllCaches();

        this.scheduledEventQualificationRequirementChangeHistoryChanged.next([updatedScheduledEventQualificationRequirementChangeHistory]);

        this.alertService.showMessage("Scheduled Event Qualification Requirement Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduled Event Qualification Requirement Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Qualification Requirement Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Qualification Requirement Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(scheduledEventQualificationRequirementChangeHistoryData: ScheduledEventQualificationRequirementChangeHistoryData | null) {

    if (scheduledEventQualificationRequirementChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventQualificationRequirementChangeHistoryForm.reset({
        scheduledEventQualificationRequirementId: null,
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
        this.scheduledEventQualificationRequirementChangeHistoryForm.reset({
        scheduledEventQualificationRequirementId: scheduledEventQualificationRequirementChangeHistoryData.scheduledEventQualificationRequirementId,
        versionNumber: scheduledEventQualificationRequirementChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(scheduledEventQualificationRequirementChangeHistoryData.timeStamp) ?? '',
        userId: scheduledEventQualificationRequirementChangeHistoryData.userId?.toString() ?? '',
        data: scheduledEventQualificationRequirementChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.scheduledEventQualificationRequirementChangeHistoryForm.markAsPristine();
    this.scheduledEventQualificationRequirementChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerScheduledEventQualificationRequirementChangeHistoryReader(): boolean {
    return this.scheduledEventQualificationRequirementChangeHistoryService.userIsSchedulerScheduledEventQualificationRequirementChangeHistoryReader();
  }

  public userIsSchedulerScheduledEventQualificationRequirementChangeHistoryWriter(): boolean {
    return this.scheduledEventQualificationRequirementChangeHistoryService.userIsSchedulerScheduledEventQualificationRequirementChangeHistoryWriter();
  }
}
