/*
   GENERATED FORM FOR THE SHIFTPATTERN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ShiftPattern table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to shift-pattern-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ShiftPatternService, ShiftPatternData, ShiftPatternSubmitData } from '../../../scheduler-data-services/shift-pattern.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ShiftPatternFormValues {
  name: string,
  description: string | null,
  timeZoneId: number | bigint | null,       // For FK link number
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-shift-pattern-add-edit',
  templateUrl: './shift-pattern-add-edit.component.html',
  styleUrls: ['./shift-pattern-add-edit.component.scss']
})
export class ShiftPatternAddEditComponent {
  @ViewChild('shiftPatternModal') shiftPatternModal!: TemplateRef<any>;
  @Output() shiftPatternChanged = new Subject<ShiftPatternData[]>();
  @Input() shiftPatternSubmitData: ShiftPatternSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ShiftPatternFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public shiftPatternForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        timeZoneId: [null],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  shiftPatterns$ = this.shiftPatternService.GetShiftPatternList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();

  constructor(
    private modalService: NgbModal,
    private shiftPatternService: ShiftPatternService,
    private timeZoneService: TimeZoneService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(shiftPatternData?: ShiftPatternData) {

    if (shiftPatternData != null) {

      if (!this.shiftPatternService.userIsSchedulerShiftPatternReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Shift Patterns`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.shiftPatternSubmitData = this.shiftPatternService.ConvertToShiftPatternSubmitData(shiftPatternData);
      this.isEditMode = true;
      this.objectGuid = shiftPatternData.objectGuid;

      this.buildFormValues(shiftPatternData);

    } else {

      if (!this.shiftPatternService.userIsSchedulerShiftPatternWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Shift Patterns`,
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
        this.shiftPatternForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.shiftPatternForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.shiftPatternModal, {
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

    if (this.shiftPatternService.userIsSchedulerShiftPatternWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Shift Patterns`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.shiftPatternForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.shiftPatternForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.shiftPatternForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const shiftPatternSubmitData: ShiftPatternSubmitData = {
        id: this.shiftPatternSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        timeZoneId: formValue.timeZoneId ? Number(formValue.timeZoneId) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.shiftPatternSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateShiftPattern(shiftPatternSubmitData);
      } else {
        this.addShiftPattern(shiftPatternSubmitData);
      }
  }

  private addShiftPattern(shiftPatternData: ShiftPatternSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    shiftPatternData.versionNumber = 0;
    shiftPatternData.active = true;
    shiftPatternData.deleted = false;
    this.shiftPatternService.PostShiftPattern(shiftPatternData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newShiftPattern) => {

        this.shiftPatternService.ClearAllCaches();

        this.shiftPatternChanged.next([newShiftPattern]);

        this.alertService.showMessage("Shift Pattern added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/shiftpattern', newShiftPattern.id]);
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
                                   'You do not have permission to save this Shift Pattern.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Shift Pattern.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Shift Pattern could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateShiftPattern(shiftPatternData: ShiftPatternSubmitData) {
    this.shiftPatternService.PutShiftPattern(shiftPatternData.id, shiftPatternData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedShiftPattern) => {

        this.shiftPatternService.ClearAllCaches();

        this.shiftPatternChanged.next([updatedShiftPattern]);

        this.alertService.showMessage("Shift Pattern updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Shift Pattern.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Shift Pattern.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Shift Pattern could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(shiftPatternData: ShiftPatternData | null) {

    if (shiftPatternData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.shiftPatternForm.reset({
        name: '',
        description: '',
        timeZoneId: null,
        color: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.shiftPatternForm.reset({
        name: shiftPatternData.name ?? '',
        description: shiftPatternData.description ?? '',
        timeZoneId: shiftPatternData.timeZoneId,
        color: shiftPatternData.color ?? '',
        versionNumber: shiftPatternData.versionNumber?.toString() ?? '',
        active: shiftPatternData.active ?? true,
        deleted: shiftPatternData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.shiftPatternForm.markAsPristine();
    this.shiftPatternForm.markAsUntouched();
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


  public userIsSchedulerShiftPatternReader(): boolean {
    return this.shiftPatternService.userIsSchedulerShiftPatternReader();
  }

  public userIsSchedulerShiftPatternWriter(): boolean {
    return this.shiftPatternService.userIsSchedulerShiftPatternWriter();
  }
}
