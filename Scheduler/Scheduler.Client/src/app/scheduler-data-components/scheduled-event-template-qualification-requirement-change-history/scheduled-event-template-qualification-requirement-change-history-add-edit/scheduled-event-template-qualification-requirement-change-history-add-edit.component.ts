/*
   GENERATED FORM FOR THE SCHEDULEDEVENTTEMPLATEQUALIFICATIONREQUIREMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventTemplateQualificationRequirementChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-template-qualification-requirement-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventTemplateQualificationRequirementChangeHistoryService, ScheduledEventTemplateQualificationRequirementChangeHistoryData, ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData } from '../../../scheduler-data-services/scheduled-event-template-qualification-requirement-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduledEventTemplateQualificationRequirementService } from '../../../scheduler-data-services/scheduled-event-template-qualification-requirement.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ScheduledEventTemplateQualificationRequirementChangeHistoryFormValues {
  scheduledEventTemplateQualificationRequirementId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-scheduled-event-template-qualification-requirement-change-history-add-edit',
  templateUrl: './scheduled-event-template-qualification-requirement-change-history-add-edit.component.html',
  styleUrls: ['./scheduled-event-template-qualification-requirement-change-history-add-edit.component.scss']
})
export class ScheduledEventTemplateQualificationRequirementChangeHistoryAddEditComponent {
  @ViewChild('scheduledEventTemplateQualificationRequirementChangeHistoryModal') scheduledEventTemplateQualificationRequirementChangeHistoryModal!: TemplateRef<any>;
  @Output() scheduledEventTemplateQualificationRequirementChangeHistoryChanged = new Subject<ScheduledEventTemplateQualificationRequirementChangeHistoryData[]>();
  @Input() scheduledEventTemplateQualificationRequirementChangeHistorySubmitData: ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventTemplateQualificationRequirementChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventTemplateQualificationRequirementChangeHistoryForm: FormGroup = this.fb.group({
        scheduledEventTemplateQualificationRequirementId: [null, Validators.required],
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

  scheduledEventTemplateQualificationRequirementChangeHistories$ = this.scheduledEventTemplateQualificationRequirementChangeHistoryService.GetScheduledEventTemplateQualificationRequirementChangeHistoryList();
  scheduledEventTemplateQualificationRequirements$ = this.scheduledEventTemplateQualificationRequirementService.GetScheduledEventTemplateQualificationRequirementList();

  constructor(
    private modalService: NgbModal,
    private scheduledEventTemplateQualificationRequirementChangeHistoryService: ScheduledEventTemplateQualificationRequirementChangeHistoryService,
    private scheduledEventTemplateQualificationRequirementService: ScheduledEventTemplateQualificationRequirementService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(scheduledEventTemplateQualificationRequirementChangeHistoryData?: ScheduledEventTemplateQualificationRequirementChangeHistoryData) {

    if (scheduledEventTemplateQualificationRequirementChangeHistoryData != null) {

      if (!this.scheduledEventTemplateQualificationRequirementChangeHistoryService.userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduled Event Template Qualification Requirement Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.scheduledEventTemplateQualificationRequirementChangeHistorySubmitData = this.scheduledEventTemplateQualificationRequirementChangeHistoryService.ConvertToScheduledEventTemplateQualificationRequirementChangeHistorySubmitData(scheduledEventTemplateQualificationRequirementChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(scheduledEventTemplateQualificationRequirementChangeHistoryData);

    } else {

      if (!this.scheduledEventTemplateQualificationRequirementChangeHistoryService.userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Template Qualification Requirement Change Histories`,
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
        this.scheduledEventTemplateQualificationRequirementChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventTemplateQualificationRequirementChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.scheduledEventTemplateQualificationRequirementChangeHistoryModal, {
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

    if (this.scheduledEventTemplateQualificationRequirementChangeHistoryService.userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Template Qualification Requirement Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.scheduledEventTemplateQualificationRequirementChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventTemplateQualificationRequirementChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventTemplateQualificationRequirementChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventTemplateQualificationRequirementChangeHistorySubmitData: ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData = {
        id: this.scheduledEventTemplateQualificationRequirementChangeHistorySubmitData?.id || 0,
        scheduledEventTemplateQualificationRequirementId: Number(formValue.scheduledEventTemplateQualificationRequirementId),
        versionNumber: this.scheduledEventTemplateQualificationRequirementChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateScheduledEventTemplateQualificationRequirementChangeHistory(scheduledEventTemplateQualificationRequirementChangeHistorySubmitData);
      } else {
        this.addScheduledEventTemplateQualificationRequirementChangeHistory(scheduledEventTemplateQualificationRequirementChangeHistorySubmitData);
      }
  }

  private addScheduledEventTemplateQualificationRequirementChangeHistory(scheduledEventTemplateQualificationRequirementChangeHistoryData: ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    scheduledEventTemplateQualificationRequirementChangeHistoryData.versionNumber = 0;
    this.scheduledEventTemplateQualificationRequirementChangeHistoryService.PostScheduledEventTemplateQualificationRequirementChangeHistory(scheduledEventTemplateQualificationRequirementChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newScheduledEventTemplateQualificationRequirementChangeHistory) => {

        this.scheduledEventTemplateQualificationRequirementChangeHistoryService.ClearAllCaches();

        this.scheduledEventTemplateQualificationRequirementChangeHistoryChanged.next([newScheduledEventTemplateQualificationRequirementChangeHistory]);

        this.alertService.showMessage("Scheduled Event Template Qualification Requirement Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/scheduledeventtemplatequalificationrequirementchangehistory', newScheduledEventTemplateQualificationRequirementChangeHistory.id]);
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
                                   'You do not have permission to save this Scheduled Event Template Qualification Requirement Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Template Qualification Requirement Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Template Qualification Requirement Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateScheduledEventTemplateQualificationRequirementChangeHistory(scheduledEventTemplateQualificationRequirementChangeHistoryData: ScheduledEventTemplateQualificationRequirementChangeHistorySubmitData) {
    this.scheduledEventTemplateQualificationRequirementChangeHistoryService.PutScheduledEventTemplateQualificationRequirementChangeHistory(scheduledEventTemplateQualificationRequirementChangeHistoryData.id, scheduledEventTemplateQualificationRequirementChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedScheduledEventTemplateQualificationRequirementChangeHistory) => {

        this.scheduledEventTemplateQualificationRequirementChangeHistoryService.ClearAllCaches();

        this.scheduledEventTemplateQualificationRequirementChangeHistoryChanged.next([updatedScheduledEventTemplateQualificationRequirementChangeHistory]);

        this.alertService.showMessage("Scheduled Event Template Qualification Requirement Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduled Event Template Qualification Requirement Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Template Qualification Requirement Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Template Qualification Requirement Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(scheduledEventTemplateQualificationRequirementChangeHistoryData: ScheduledEventTemplateQualificationRequirementChangeHistoryData | null) {

    if (scheduledEventTemplateQualificationRequirementChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventTemplateQualificationRequirementChangeHistoryForm.reset({
        scheduledEventTemplateQualificationRequirementId: null,
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
        this.scheduledEventTemplateQualificationRequirementChangeHistoryForm.reset({
        scheduledEventTemplateQualificationRequirementId: scheduledEventTemplateQualificationRequirementChangeHistoryData.scheduledEventTemplateQualificationRequirementId,
        versionNumber: scheduledEventTemplateQualificationRequirementChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(scheduledEventTemplateQualificationRequirementChangeHistoryData.timeStamp) ?? '',
        userId: scheduledEventTemplateQualificationRequirementChangeHistoryData.userId?.toString() ?? '',
        data: scheduledEventTemplateQualificationRequirementChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.scheduledEventTemplateQualificationRequirementChangeHistoryForm.markAsPristine();
    this.scheduledEventTemplateQualificationRequirementChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryReader(): boolean {
    return this.scheduledEventTemplateQualificationRequirementChangeHistoryService.userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryReader();
  }

  public userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryWriter(): boolean {
    return this.scheduledEventTemplateQualificationRequirementChangeHistoryService.userIsSchedulerScheduledEventTemplateQualificationRequirementChangeHistoryWriter();
  }
}
