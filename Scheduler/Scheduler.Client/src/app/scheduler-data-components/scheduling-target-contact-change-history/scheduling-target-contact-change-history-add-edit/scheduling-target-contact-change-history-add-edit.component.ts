/*
   GENERATED FORM FOR THE SCHEDULINGTARGETCONTACTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetContactChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-contact-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetContactChangeHistoryService, SchedulingTargetContactChangeHistoryData, SchedulingTargetContactChangeHistorySubmitData } from '../../../scheduler-data-services/scheduling-target-contact-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SchedulingTargetContactService } from '../../../scheduler-data-services/scheduling-target-contact.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SchedulingTargetContactChangeHistoryFormValues {
  schedulingTargetContactId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-scheduling-target-contact-change-history-add-edit',
  templateUrl: './scheduling-target-contact-change-history-add-edit.component.html',
  styleUrls: ['./scheduling-target-contact-change-history-add-edit.component.scss']
})
export class SchedulingTargetContactChangeHistoryAddEditComponent {
  @ViewChild('schedulingTargetContactChangeHistoryModal') schedulingTargetContactChangeHistoryModal!: TemplateRef<any>;
  @Output() schedulingTargetContactChangeHistoryChanged = new Subject<SchedulingTargetContactChangeHistoryData[]>();
  @Input() schedulingTargetContactChangeHistorySubmitData: SchedulingTargetContactChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetContactChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetContactChangeHistoryForm: FormGroup = this.fb.group({
        schedulingTargetContactId: [null, Validators.required],
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

  schedulingTargetContactChangeHistories$ = this.schedulingTargetContactChangeHistoryService.GetSchedulingTargetContactChangeHistoryList();
  schedulingTargetContacts$ = this.schedulingTargetContactService.GetSchedulingTargetContactList();

  constructor(
    private modalService: NgbModal,
    private schedulingTargetContactChangeHistoryService: SchedulingTargetContactChangeHistoryService,
    private schedulingTargetContactService: SchedulingTargetContactService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(schedulingTargetContactChangeHistoryData?: SchedulingTargetContactChangeHistoryData) {

    if (schedulingTargetContactChangeHistoryData != null) {

      if (!this.schedulingTargetContactChangeHistoryService.userIsSchedulerSchedulingTargetContactChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduling Target Contact Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.schedulingTargetContactChangeHistorySubmitData = this.schedulingTargetContactChangeHistoryService.ConvertToSchedulingTargetContactChangeHistorySubmitData(schedulingTargetContactChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(schedulingTargetContactChangeHistoryData);

    } else {

      if (!this.schedulingTargetContactChangeHistoryService.userIsSchedulerSchedulingTargetContactChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduling Target Contact Change Histories`,
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
        this.schedulingTargetContactChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetContactChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.schedulingTargetContactChangeHistoryModal, {
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

    if (this.schedulingTargetContactChangeHistoryService.userIsSchedulerSchedulingTargetContactChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduling Target Contact Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.schedulingTargetContactChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetContactChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetContactChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetContactChangeHistorySubmitData: SchedulingTargetContactChangeHistorySubmitData = {
        id: this.schedulingTargetContactChangeHistorySubmitData?.id || 0,
        schedulingTargetContactId: Number(formValue.schedulingTargetContactId),
        versionNumber: this.schedulingTargetContactChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateSchedulingTargetContactChangeHistory(schedulingTargetContactChangeHistorySubmitData);
      } else {
        this.addSchedulingTargetContactChangeHistory(schedulingTargetContactChangeHistorySubmitData);
      }
  }

  private addSchedulingTargetContactChangeHistory(schedulingTargetContactChangeHistoryData: SchedulingTargetContactChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    schedulingTargetContactChangeHistoryData.versionNumber = 0;
    this.schedulingTargetContactChangeHistoryService.PostSchedulingTargetContactChangeHistory(schedulingTargetContactChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSchedulingTargetContactChangeHistory) => {

        this.schedulingTargetContactChangeHistoryService.ClearAllCaches();

        this.schedulingTargetContactChangeHistoryChanged.next([newSchedulingTargetContactChangeHistory]);

        this.alertService.showMessage("Scheduling Target Contact Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/schedulingtargetcontactchangehistory', newSchedulingTargetContactChangeHistory.id]);
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
                                   'You do not have permission to save this Scheduling Target Contact Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Contact Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Contact Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSchedulingTargetContactChangeHistory(schedulingTargetContactChangeHistoryData: SchedulingTargetContactChangeHistorySubmitData) {
    this.schedulingTargetContactChangeHistoryService.PutSchedulingTargetContactChangeHistory(schedulingTargetContactChangeHistoryData.id, schedulingTargetContactChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSchedulingTargetContactChangeHistory) => {

        this.schedulingTargetContactChangeHistoryService.ClearAllCaches();

        this.schedulingTargetContactChangeHistoryChanged.next([updatedSchedulingTargetContactChangeHistory]);

        this.alertService.showMessage("Scheduling Target Contact Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduling Target Contact Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Contact Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Contact Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(schedulingTargetContactChangeHistoryData: SchedulingTargetContactChangeHistoryData | null) {

    if (schedulingTargetContactChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetContactChangeHistoryForm.reset({
        schedulingTargetContactId: null,
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
        this.schedulingTargetContactChangeHistoryForm.reset({
        schedulingTargetContactId: schedulingTargetContactChangeHistoryData.schedulingTargetContactId,
        versionNumber: schedulingTargetContactChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(schedulingTargetContactChangeHistoryData.timeStamp) ?? '',
        userId: schedulingTargetContactChangeHistoryData.userId?.toString() ?? '',
        data: schedulingTargetContactChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.schedulingTargetContactChangeHistoryForm.markAsPristine();
    this.schedulingTargetContactChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerSchedulingTargetContactChangeHistoryReader(): boolean {
    return this.schedulingTargetContactChangeHistoryService.userIsSchedulerSchedulingTargetContactChangeHistoryReader();
  }

  public userIsSchedulerSchedulingTargetContactChangeHistoryWriter(): boolean {
    return this.schedulingTargetContactChangeHistoryService.userIsSchedulerSchedulingTargetContactChangeHistoryWriter();
  }
}
