/*
   GENERATED FORM FOR THE RECURRENCEEXCEPTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RecurrenceException table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to recurrence-exception-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RecurrenceExceptionService, RecurrenceExceptionData, RecurrenceExceptionSubmitData } from '../../../scheduler-data-services/recurrence-exception.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface RecurrenceExceptionFormValues {
  scheduledEventId: number | bigint,       // For FK link number
  exceptionDateTime: string,
  movedToDateTime: string | null,
  reason: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-recurrence-exception-add-edit',
  templateUrl: './recurrence-exception-add-edit.component.html',
  styleUrls: ['./recurrence-exception-add-edit.component.scss']
})
export class RecurrenceExceptionAddEditComponent {
  @ViewChild('recurrenceExceptionModal') recurrenceExceptionModal!: TemplateRef<any>;
  @Output() recurrenceExceptionChanged = new Subject<RecurrenceExceptionData[]>();
  @Input() recurrenceExceptionSubmitData: RecurrenceExceptionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RecurrenceExceptionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public recurrenceExceptionForm: FormGroup = this.fb.group({
        scheduledEventId: [null, Validators.required],
        exceptionDateTime: ['', Validators.required],
        movedToDateTime: [''],
        reason: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  recurrenceExceptions$ = this.recurrenceExceptionService.GetRecurrenceExceptionList();
  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();

  constructor(
    private modalService: NgbModal,
    private recurrenceExceptionService: RecurrenceExceptionService,
    private scheduledEventService: ScheduledEventService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(recurrenceExceptionData?: RecurrenceExceptionData) {

    if (recurrenceExceptionData != null) {

      if (!this.recurrenceExceptionService.userIsSchedulerRecurrenceExceptionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Recurrence Exceptions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.recurrenceExceptionSubmitData = this.recurrenceExceptionService.ConvertToRecurrenceExceptionSubmitData(recurrenceExceptionData);
      this.isEditMode = true;
      this.objectGuid = recurrenceExceptionData.objectGuid;

      this.buildFormValues(recurrenceExceptionData);

    } else {

      if (!this.recurrenceExceptionService.userIsSchedulerRecurrenceExceptionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Recurrence Exceptions`,
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
        this.recurrenceExceptionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.recurrenceExceptionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.recurrenceExceptionModal, {
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

    if (this.recurrenceExceptionService.userIsSchedulerRecurrenceExceptionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Recurrence Exceptions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.recurrenceExceptionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.recurrenceExceptionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.recurrenceExceptionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const recurrenceExceptionSubmitData: RecurrenceExceptionSubmitData = {
        id: this.recurrenceExceptionSubmitData?.id || 0,
        scheduledEventId: Number(formValue.scheduledEventId),
        exceptionDateTime: dateTimeLocalToIsoUtc(formValue.exceptionDateTime!.trim())!,
        movedToDateTime: formValue.movedToDateTime ? dateTimeLocalToIsoUtc(formValue.movedToDateTime.trim()) : null,
        reason: formValue.reason?.trim() || null,
        versionNumber: this.recurrenceExceptionSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateRecurrenceException(recurrenceExceptionSubmitData);
      } else {
        this.addRecurrenceException(recurrenceExceptionSubmitData);
      }
  }

  private addRecurrenceException(recurrenceExceptionData: RecurrenceExceptionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    recurrenceExceptionData.versionNumber = 0;
    recurrenceExceptionData.active = true;
    recurrenceExceptionData.deleted = false;
    this.recurrenceExceptionService.PostRecurrenceException(recurrenceExceptionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newRecurrenceException) => {

        this.recurrenceExceptionService.ClearAllCaches();

        this.recurrenceExceptionChanged.next([newRecurrenceException]);

        this.alertService.showMessage("Recurrence Exception added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/recurrenceexception', newRecurrenceException.id]);
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
                                   'You do not have permission to save this Recurrence Exception.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Recurrence Exception.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Recurrence Exception could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateRecurrenceException(recurrenceExceptionData: RecurrenceExceptionSubmitData) {
    this.recurrenceExceptionService.PutRecurrenceException(recurrenceExceptionData.id, recurrenceExceptionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedRecurrenceException) => {

        this.recurrenceExceptionService.ClearAllCaches();

        this.recurrenceExceptionChanged.next([updatedRecurrenceException]);

        this.alertService.showMessage("Recurrence Exception updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Recurrence Exception.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Recurrence Exception.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Recurrence Exception could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(recurrenceExceptionData: RecurrenceExceptionData | null) {

    if (recurrenceExceptionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.recurrenceExceptionForm.reset({
        scheduledEventId: null,
        exceptionDateTime: '',
        movedToDateTime: '',
        reason: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.recurrenceExceptionForm.reset({
        scheduledEventId: recurrenceExceptionData.scheduledEventId,
        exceptionDateTime: isoUtcStringToDateTimeLocal(recurrenceExceptionData.exceptionDateTime) ?? '',
        movedToDateTime: isoUtcStringToDateTimeLocal(recurrenceExceptionData.movedToDateTime) ?? '',
        reason: recurrenceExceptionData.reason ?? '',
        versionNumber: recurrenceExceptionData.versionNumber?.toString() ?? '',
        active: recurrenceExceptionData.active ?? true,
        deleted: recurrenceExceptionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.recurrenceExceptionForm.markAsPristine();
    this.recurrenceExceptionForm.markAsUntouched();
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


  public userIsSchedulerRecurrenceExceptionReader(): boolean {
    return this.recurrenceExceptionService.userIsSchedulerRecurrenceExceptionReader();
  }

  public userIsSchedulerRecurrenceExceptionWriter(): boolean {
    return this.recurrenceExceptionService.userIsSchedulerRecurrenceExceptionWriter();
  }
}
