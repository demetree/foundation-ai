/*
   GENERATED FORM FOR THE RECURRENCEFREQUENCY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RecurrenceFrequency table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to recurrence-frequency-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RecurrenceFrequencyService, RecurrenceFrequencyData, RecurrenceFrequencySubmitData } from '../../../scheduler-data-services/recurrence-frequency.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface RecurrenceFrequencyFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-recurrence-frequency-add-edit',
  templateUrl: './recurrence-frequency-add-edit.component.html',
  styleUrls: ['./recurrence-frequency-add-edit.component.scss']
})
export class RecurrenceFrequencyAddEditComponent {
  @ViewChild('recurrenceFrequencyModal') recurrenceFrequencyModal!: TemplateRef<any>;
  @Output() recurrenceFrequencyChanged = new Subject<RecurrenceFrequencyData[]>();
  @Input() recurrenceFrequencySubmitData: RecurrenceFrequencySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RecurrenceFrequencyFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public recurrenceFrequencyForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  recurrenceFrequencies$ = this.recurrenceFrequencyService.GetRecurrenceFrequencyList();

  constructor(
    private modalService: NgbModal,
    private recurrenceFrequencyService: RecurrenceFrequencyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(recurrenceFrequencyData?: RecurrenceFrequencyData) {

    if (recurrenceFrequencyData != null) {

      if (!this.recurrenceFrequencyService.userIsSchedulerRecurrenceFrequencyReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Recurrence Frequencies`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.recurrenceFrequencySubmitData = this.recurrenceFrequencyService.ConvertToRecurrenceFrequencySubmitData(recurrenceFrequencyData);
      this.isEditMode = true;
      this.objectGuid = recurrenceFrequencyData.objectGuid;

      this.buildFormValues(recurrenceFrequencyData);

    } else {

      if (!this.recurrenceFrequencyService.userIsSchedulerRecurrenceFrequencyWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Recurrence Frequencies`,
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
        this.recurrenceFrequencyForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.recurrenceFrequencyForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.recurrenceFrequencyModal, {
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

    if (this.recurrenceFrequencyService.userIsSchedulerRecurrenceFrequencyWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Recurrence Frequencies`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.recurrenceFrequencyForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.recurrenceFrequencyForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.recurrenceFrequencyForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const recurrenceFrequencySubmitData: RecurrenceFrequencySubmitData = {
        id: this.recurrenceFrequencySubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateRecurrenceFrequency(recurrenceFrequencySubmitData);
      } else {
        this.addRecurrenceFrequency(recurrenceFrequencySubmitData);
      }
  }

  private addRecurrenceFrequency(recurrenceFrequencyData: RecurrenceFrequencySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    recurrenceFrequencyData.active = true;
    recurrenceFrequencyData.deleted = false;
    this.recurrenceFrequencyService.PostRecurrenceFrequency(recurrenceFrequencyData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newRecurrenceFrequency) => {

        this.recurrenceFrequencyService.ClearAllCaches();

        this.recurrenceFrequencyChanged.next([newRecurrenceFrequency]);

        this.alertService.showMessage("Recurrence Frequency added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/recurrencefrequency', newRecurrenceFrequency.id]);
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
                                   'You do not have permission to save this Recurrence Frequency.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Recurrence Frequency.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Recurrence Frequency could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateRecurrenceFrequency(recurrenceFrequencyData: RecurrenceFrequencySubmitData) {
    this.recurrenceFrequencyService.PutRecurrenceFrequency(recurrenceFrequencyData.id, recurrenceFrequencyData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedRecurrenceFrequency) => {

        this.recurrenceFrequencyService.ClearAllCaches();

        this.recurrenceFrequencyChanged.next([updatedRecurrenceFrequency]);

        this.alertService.showMessage("Recurrence Frequency updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Recurrence Frequency.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Recurrence Frequency.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Recurrence Frequency could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(recurrenceFrequencyData: RecurrenceFrequencyData | null) {

    if (recurrenceFrequencyData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.recurrenceFrequencyForm.reset({
        name: '',
        description: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.recurrenceFrequencyForm.reset({
        name: recurrenceFrequencyData.name ?? '',
        description: recurrenceFrequencyData.description ?? '',
        sequence: recurrenceFrequencyData.sequence?.toString() ?? '',
        active: recurrenceFrequencyData.active ?? true,
        deleted: recurrenceFrequencyData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.recurrenceFrequencyForm.markAsPristine();
    this.recurrenceFrequencyForm.markAsUntouched();
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


  public userIsSchedulerRecurrenceFrequencyReader(): boolean {
    return this.recurrenceFrequencyService.userIsSchedulerRecurrenceFrequencyReader();
  }

  public userIsSchedulerRecurrenceFrequencyWriter(): boolean {
    return this.recurrenceFrequencyService.userIsSchedulerRecurrenceFrequencyWriter();
  }
}
