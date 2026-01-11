/*
   GENERATED FORM FOR THE SHIFTPATTERNDAY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ShiftPatternDay table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to shift-pattern-day-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ShiftPatternDayService, ShiftPatternDayData, ShiftPatternDaySubmitData } from '../../../scheduler-data-services/shift-pattern-day.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ShiftPatternService } from '../../../scheduler-data-services/shift-pattern.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ShiftPatternDayFormValues {
  shiftPatternId: number | bigint,       // For FK link number
  dayOfWeek: string,     // Stored as string for form input, converted to number on submit.
  startTime: string,
  hours: string,     // Stored as string for form input, converted to number on submit.
  label: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-shift-pattern-day-add-edit',
  templateUrl: './shift-pattern-day-add-edit.component.html',
  styleUrls: ['./shift-pattern-day-add-edit.component.scss']
})
export class ShiftPatternDayAddEditComponent {
  @ViewChild('shiftPatternDayModal') shiftPatternDayModal!: TemplateRef<any>;
  @Output() shiftPatternDayChanged = new Subject<ShiftPatternDayData[]>();
  @Input() shiftPatternDaySubmitData: ShiftPatternDaySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ShiftPatternDayFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public shiftPatternDayForm: FormGroup = this.fb.group({
        shiftPatternId: [null, Validators.required],
        dayOfWeek: ['', Validators.required],
        startTime: ['', Validators.required],
        hours: ['', Validators.required],
        label: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  shiftPatternDays$ = this.shiftPatternDayService.GetShiftPatternDayList();
  shiftPatterns$ = this.shiftPatternService.GetShiftPatternList();

  constructor(
    private modalService: NgbModal,
    private shiftPatternDayService: ShiftPatternDayService,
    private shiftPatternService: ShiftPatternService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(shiftPatternDayData?: ShiftPatternDayData) {

    if (shiftPatternDayData != null) {

      if (!this.shiftPatternDayService.userIsSchedulerShiftPatternDayReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Shift Pattern Days`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.shiftPatternDaySubmitData = this.shiftPatternDayService.ConvertToShiftPatternDaySubmitData(shiftPatternDayData);
      this.isEditMode = true;
      this.objectGuid = shiftPatternDayData.objectGuid;

      this.buildFormValues(shiftPatternDayData);

    } else {

      if (!this.shiftPatternDayService.userIsSchedulerShiftPatternDayWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Shift Pattern Days`,
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
        this.shiftPatternDayForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.shiftPatternDayForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.shiftPatternDayModal, {
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

    if (this.shiftPatternDayService.userIsSchedulerShiftPatternDayWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Shift Pattern Days`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.shiftPatternDayForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.shiftPatternDayForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.shiftPatternDayForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const shiftPatternDaySubmitData: ShiftPatternDaySubmitData = {
        id: this.shiftPatternDaySubmitData?.id || 0,
        shiftPatternId: Number(formValue.shiftPatternId),
        dayOfWeek: Number(formValue.dayOfWeek),
        startTime: formValue.startTime!.trim(),
        hours: Number(formValue.hours),
        label: formValue.label?.trim() || null,
        versionNumber: this.shiftPatternDaySubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateShiftPatternDay(shiftPatternDaySubmitData);
      } else {
        this.addShiftPatternDay(shiftPatternDaySubmitData);
      }
  }

  private addShiftPatternDay(shiftPatternDayData: ShiftPatternDaySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    shiftPatternDayData.versionNumber = 0;
    shiftPatternDayData.active = true;
    shiftPatternDayData.deleted = false;
    this.shiftPatternDayService.PostShiftPatternDay(shiftPatternDayData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newShiftPatternDay) => {

        this.shiftPatternDayService.ClearAllCaches();

        this.shiftPatternDayChanged.next([newShiftPatternDay]);

        this.alertService.showMessage("Shift Pattern Day added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/shiftpatternday', newShiftPatternDay.id]);
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
                                   'You do not have permission to save this Shift Pattern Day.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Shift Pattern Day.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Shift Pattern Day could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateShiftPatternDay(shiftPatternDayData: ShiftPatternDaySubmitData) {
    this.shiftPatternDayService.PutShiftPatternDay(shiftPatternDayData.id, shiftPatternDayData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedShiftPatternDay) => {

        this.shiftPatternDayService.ClearAllCaches();

        this.shiftPatternDayChanged.next([updatedShiftPatternDay]);

        this.alertService.showMessage("Shift Pattern Day updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Shift Pattern Day.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Shift Pattern Day.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Shift Pattern Day could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(shiftPatternDayData: ShiftPatternDayData | null) {

    if (shiftPatternDayData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.shiftPatternDayForm.reset({
        shiftPatternId: null,
        dayOfWeek: '',
        startTime: '',
        hours: '',
        label: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.shiftPatternDayForm.reset({
        shiftPatternId: shiftPatternDayData.shiftPatternId,
        dayOfWeek: shiftPatternDayData.dayOfWeek?.toString() ?? '',
        startTime: shiftPatternDayData.startTime ?? '',
        hours: shiftPatternDayData.hours?.toString() ?? '',
        label: shiftPatternDayData.label ?? '',
        versionNumber: shiftPatternDayData.versionNumber?.toString() ?? '',
        active: shiftPatternDayData.active ?? true,
        deleted: shiftPatternDayData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.shiftPatternDayForm.markAsPristine();
    this.shiftPatternDayForm.markAsUntouched();
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


  public userIsSchedulerShiftPatternDayReader(): boolean {
    return this.shiftPatternDayService.userIsSchedulerShiftPatternDayReader();
  }

  public userIsSchedulerShiftPatternDayWriter(): boolean {
    return this.shiftPatternDayService.userIsSchedulerShiftPatternDayWriter();
  }
}
