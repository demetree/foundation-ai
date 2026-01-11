/*
   GENERATED FORM FOR THE SCHEDULEDEVENTDEPENDENCYCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventDependencyChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-dependency-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventDependencyChangeHistoryService, ScheduledEventDependencyChangeHistoryData, ScheduledEventDependencyChangeHistorySubmitData } from '../../../scheduler-data-services/scheduled-event-dependency-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduledEventDependencyService } from '../../../scheduler-data-services/scheduled-event-dependency.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ScheduledEventDependencyChangeHistoryFormValues {
  scheduledEventDependencyId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-scheduled-event-dependency-change-history-add-edit',
  templateUrl: './scheduled-event-dependency-change-history-add-edit.component.html',
  styleUrls: ['./scheduled-event-dependency-change-history-add-edit.component.scss']
})
export class ScheduledEventDependencyChangeHistoryAddEditComponent {
  @ViewChild('scheduledEventDependencyChangeHistoryModal') scheduledEventDependencyChangeHistoryModal!: TemplateRef<any>;
  @Output() scheduledEventDependencyChangeHistoryChanged = new Subject<ScheduledEventDependencyChangeHistoryData[]>();
  @Input() scheduledEventDependencyChangeHistorySubmitData: ScheduledEventDependencyChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventDependencyChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventDependencyChangeHistoryForm: FormGroup = this.fb.group({
        scheduledEventDependencyId: [null, Validators.required],
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

  scheduledEventDependencyChangeHistories$ = this.scheduledEventDependencyChangeHistoryService.GetScheduledEventDependencyChangeHistoryList();
  scheduledEventDependencies$ = this.scheduledEventDependencyService.GetScheduledEventDependencyList();

  constructor(
    private modalService: NgbModal,
    private scheduledEventDependencyChangeHistoryService: ScheduledEventDependencyChangeHistoryService,
    private scheduledEventDependencyService: ScheduledEventDependencyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(scheduledEventDependencyChangeHistoryData?: ScheduledEventDependencyChangeHistoryData) {

    if (scheduledEventDependencyChangeHistoryData != null) {

      if (!this.scheduledEventDependencyChangeHistoryService.userIsSchedulerScheduledEventDependencyChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduled Event Dependency Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.scheduledEventDependencyChangeHistorySubmitData = this.scheduledEventDependencyChangeHistoryService.ConvertToScheduledEventDependencyChangeHistorySubmitData(scheduledEventDependencyChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(scheduledEventDependencyChangeHistoryData);

    } else {

      if (!this.scheduledEventDependencyChangeHistoryService.userIsSchedulerScheduledEventDependencyChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Dependency Change Histories`,
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
        this.scheduledEventDependencyChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventDependencyChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.scheduledEventDependencyChangeHistoryModal, {
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

    if (this.scheduledEventDependencyChangeHistoryService.userIsSchedulerScheduledEventDependencyChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Dependency Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.scheduledEventDependencyChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventDependencyChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventDependencyChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventDependencyChangeHistorySubmitData: ScheduledEventDependencyChangeHistorySubmitData = {
        id: this.scheduledEventDependencyChangeHistorySubmitData?.id || 0,
        scheduledEventDependencyId: Number(formValue.scheduledEventDependencyId),
        versionNumber: this.scheduledEventDependencyChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateScheduledEventDependencyChangeHistory(scheduledEventDependencyChangeHistorySubmitData);
      } else {
        this.addScheduledEventDependencyChangeHistory(scheduledEventDependencyChangeHistorySubmitData);
      }
  }

  private addScheduledEventDependencyChangeHistory(scheduledEventDependencyChangeHistoryData: ScheduledEventDependencyChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    scheduledEventDependencyChangeHistoryData.versionNumber = 0;
    this.scheduledEventDependencyChangeHistoryService.PostScheduledEventDependencyChangeHistory(scheduledEventDependencyChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newScheduledEventDependencyChangeHistory) => {

        this.scheduledEventDependencyChangeHistoryService.ClearAllCaches();

        this.scheduledEventDependencyChangeHistoryChanged.next([newScheduledEventDependencyChangeHistory]);

        this.alertService.showMessage("Scheduled Event Dependency Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/scheduledeventdependencychangehistory', newScheduledEventDependencyChangeHistory.id]);
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
                                   'You do not have permission to save this Scheduled Event Dependency Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Dependency Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Dependency Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateScheduledEventDependencyChangeHistory(scheduledEventDependencyChangeHistoryData: ScheduledEventDependencyChangeHistorySubmitData) {
    this.scheduledEventDependencyChangeHistoryService.PutScheduledEventDependencyChangeHistory(scheduledEventDependencyChangeHistoryData.id, scheduledEventDependencyChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedScheduledEventDependencyChangeHistory) => {

        this.scheduledEventDependencyChangeHistoryService.ClearAllCaches();

        this.scheduledEventDependencyChangeHistoryChanged.next([updatedScheduledEventDependencyChangeHistory]);

        this.alertService.showMessage("Scheduled Event Dependency Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduled Event Dependency Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Dependency Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Dependency Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(scheduledEventDependencyChangeHistoryData: ScheduledEventDependencyChangeHistoryData | null) {

    if (scheduledEventDependencyChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventDependencyChangeHistoryForm.reset({
        scheduledEventDependencyId: null,
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
        this.scheduledEventDependencyChangeHistoryForm.reset({
        scheduledEventDependencyId: scheduledEventDependencyChangeHistoryData.scheduledEventDependencyId,
        versionNumber: scheduledEventDependencyChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(scheduledEventDependencyChangeHistoryData.timeStamp) ?? '',
        userId: scheduledEventDependencyChangeHistoryData.userId?.toString() ?? '',
        data: scheduledEventDependencyChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.scheduledEventDependencyChangeHistoryForm.markAsPristine();
    this.scheduledEventDependencyChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerScheduledEventDependencyChangeHistoryReader(): boolean {
    return this.scheduledEventDependencyChangeHistoryService.userIsSchedulerScheduledEventDependencyChangeHistoryReader();
  }

  public userIsSchedulerScheduledEventDependencyChangeHistoryWriter(): boolean {
    return this.scheduledEventDependencyChangeHistoryService.userIsSchedulerScheduledEventDependencyChangeHistoryWriter();
  }
}
