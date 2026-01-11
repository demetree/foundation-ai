/*
   GENERATED FORM FOR THE RECURRENCERULE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RecurrenceRule table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to recurrence-rule-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RecurrenceRuleService, RecurrenceRuleData, RecurrenceRuleSubmitData } from '../../../scheduler-data-services/recurrence-rule.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { RecurrenceFrequencyService } from '../../../scheduler-data-services/recurrence-frequency.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface RecurrenceRuleFormValues {
  recurrenceFrequencyId: number | bigint,       // For FK link number
  interval: string,     // Stored as string for form input, converted to number on submit.
  untilDateTime: string | null,
  count: string | null,     // Stored as string for form input, converted to number on submit.
  dayOfWeekMask: string | null,     // Stored as string for form input, converted to number on submit.
  dayOfMonth: string | null,     // Stored as string for form input, converted to number on submit.
  dayOfWeekInMonth: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-recurrence-rule-add-edit',
  templateUrl: './recurrence-rule-add-edit.component.html',
  styleUrls: ['./recurrence-rule-add-edit.component.scss']
})
export class RecurrenceRuleAddEditComponent {
  @ViewChild('recurrenceRuleModal') recurrenceRuleModal!: TemplateRef<any>;
  @Output() recurrenceRuleChanged = new Subject<RecurrenceRuleData[]>();
  @Input() recurrenceRuleSubmitData: RecurrenceRuleSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RecurrenceRuleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public recurrenceRuleForm: FormGroup = this.fb.group({
        recurrenceFrequencyId: [null, Validators.required],
        interval: ['', Validators.required],
        untilDateTime: [''],
        count: [''],
        dayOfWeekMask: [''],
        dayOfMonth: [''],
        dayOfWeekInMonth: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  recurrenceRules$ = this.recurrenceRuleService.GetRecurrenceRuleList();
  recurrenceFrequencies$ = this.recurrenceFrequencyService.GetRecurrenceFrequencyList();

  constructor(
    private modalService: NgbModal,
    private recurrenceRuleService: RecurrenceRuleService,
    private recurrenceFrequencyService: RecurrenceFrequencyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(recurrenceRuleData?: RecurrenceRuleData) {

    if (recurrenceRuleData != null) {

      if (!this.recurrenceRuleService.userIsSchedulerRecurrenceRuleReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Recurrence Rules`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.recurrenceRuleSubmitData = this.recurrenceRuleService.ConvertToRecurrenceRuleSubmitData(recurrenceRuleData);
      this.isEditMode = true;
      this.objectGuid = recurrenceRuleData.objectGuid;

      this.buildFormValues(recurrenceRuleData);

    } else {

      if (!this.recurrenceRuleService.userIsSchedulerRecurrenceRuleWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Recurrence Rules`,
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
        this.recurrenceRuleForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.recurrenceRuleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.recurrenceRuleModal, {
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

    if (this.recurrenceRuleService.userIsSchedulerRecurrenceRuleWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Recurrence Rules`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.recurrenceRuleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.recurrenceRuleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.recurrenceRuleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const recurrenceRuleSubmitData: RecurrenceRuleSubmitData = {
        id: this.recurrenceRuleSubmitData?.id || 0,
        recurrenceFrequencyId: Number(formValue.recurrenceFrequencyId),
        interval: Number(formValue.interval),
        untilDateTime: formValue.untilDateTime ? dateTimeLocalToIsoUtc(formValue.untilDateTime.trim()) : null,
        count: formValue.count ? Number(formValue.count) : null,
        dayOfWeekMask: formValue.dayOfWeekMask ? Number(formValue.dayOfWeekMask) : null,
        dayOfMonth: formValue.dayOfMonth ? Number(formValue.dayOfMonth) : null,
        dayOfWeekInMonth: formValue.dayOfWeekInMonth ? Number(formValue.dayOfWeekInMonth) : null,
        versionNumber: this.recurrenceRuleSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateRecurrenceRule(recurrenceRuleSubmitData);
      } else {
        this.addRecurrenceRule(recurrenceRuleSubmitData);
      }
  }

  private addRecurrenceRule(recurrenceRuleData: RecurrenceRuleSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    recurrenceRuleData.versionNumber = 0;
    recurrenceRuleData.active = true;
    recurrenceRuleData.deleted = false;
    this.recurrenceRuleService.PostRecurrenceRule(recurrenceRuleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newRecurrenceRule) => {

        this.recurrenceRuleService.ClearAllCaches();

        this.recurrenceRuleChanged.next([newRecurrenceRule]);

        this.alertService.showMessage("Recurrence Rule added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/recurrencerule', newRecurrenceRule.id]);
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
                                   'You do not have permission to save this Recurrence Rule.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Recurrence Rule.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Recurrence Rule could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateRecurrenceRule(recurrenceRuleData: RecurrenceRuleSubmitData) {
    this.recurrenceRuleService.PutRecurrenceRule(recurrenceRuleData.id, recurrenceRuleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedRecurrenceRule) => {

        this.recurrenceRuleService.ClearAllCaches();

        this.recurrenceRuleChanged.next([updatedRecurrenceRule]);

        this.alertService.showMessage("Recurrence Rule updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Recurrence Rule.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Recurrence Rule.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Recurrence Rule could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(recurrenceRuleData: RecurrenceRuleData | null) {

    if (recurrenceRuleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.recurrenceRuleForm.reset({
        recurrenceFrequencyId: null,
        interval: '',
        untilDateTime: '',
        count: '',
        dayOfWeekMask: '',
        dayOfMonth: '',
        dayOfWeekInMonth: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.recurrenceRuleForm.reset({
        recurrenceFrequencyId: recurrenceRuleData.recurrenceFrequencyId,
        interval: recurrenceRuleData.interval?.toString() ?? '',
        untilDateTime: isoUtcStringToDateTimeLocal(recurrenceRuleData.untilDateTime) ?? '',
        count: recurrenceRuleData.count?.toString() ?? '',
        dayOfWeekMask: recurrenceRuleData.dayOfWeekMask?.toString() ?? '',
        dayOfMonth: recurrenceRuleData.dayOfMonth?.toString() ?? '',
        dayOfWeekInMonth: recurrenceRuleData.dayOfWeekInMonth?.toString() ?? '',
        versionNumber: recurrenceRuleData.versionNumber?.toString() ?? '',
        active: recurrenceRuleData.active ?? true,
        deleted: recurrenceRuleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.recurrenceRuleForm.markAsPristine();
    this.recurrenceRuleForm.markAsUntouched();
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


  public userIsSchedulerRecurrenceRuleReader(): boolean {
    return this.recurrenceRuleService.userIsSchedulerRecurrenceRuleReader();
  }

  public userIsSchedulerRecurrenceRuleWriter(): boolean {
    return this.recurrenceRuleService.userIsSchedulerRecurrenceRuleWriter();
  }
}
