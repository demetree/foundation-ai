/*
   GENERATED FORM FOR THE TIMEZONE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TimeZone table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to time-zone-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TimeZoneService, TimeZoneData, TimeZoneSubmitData } from '../../../scheduler-data-services/time-zone.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TimeZoneFormValues {
  name: string,
  description: string,
  ianaTimeZone: string,
  abbreviation: string,
  abbreviationDaylightSavings: string,
  supportsDaylightSavings: boolean,
  standardUTCOffsetHours: string,     // Stored as string for form input, converted to number on submit.
  dstUTCOffsetHours: string,     // Stored as string for form input, converted to number on submit.
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-time-zone-add-edit',
  templateUrl: './time-zone-add-edit.component.html',
  styleUrls: ['./time-zone-add-edit.component.scss']
})
export class TimeZoneAddEditComponent {
  @ViewChild('timeZoneModal') timeZoneModal!: TemplateRef<any>;
  @Output() timeZoneChanged = new Subject<TimeZoneData[]>();
  @Input() timeZoneSubmitData: TimeZoneSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TimeZoneFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public timeZoneForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        ianaTimeZone: ['', Validators.required],
        abbreviation: ['', Validators.required],
        abbreviationDaylightSavings: ['', Validators.required],
        supportsDaylightSavings: [false],
        standardUTCOffsetHours: ['', Validators.required],
        dstUTCOffsetHours: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  timeZones$ = this.timeZoneService.GetTimeZoneList();

  constructor(
    private modalService: NgbModal,
    private timeZoneService: TimeZoneService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(timeZoneData?: TimeZoneData) {

    if (timeZoneData != null) {

      if (!this.timeZoneService.userIsSchedulerTimeZoneReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Time Zones`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.timeZoneSubmitData = this.timeZoneService.ConvertToTimeZoneSubmitData(timeZoneData);
      this.isEditMode = true;
      this.objectGuid = timeZoneData.objectGuid;

      this.buildFormValues(timeZoneData);

    } else {

      if (!this.timeZoneService.userIsSchedulerTimeZoneWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Time Zones`,
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
        this.timeZoneForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.timeZoneForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.timeZoneModal, {
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

    if (this.timeZoneService.userIsSchedulerTimeZoneWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Time Zones`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.timeZoneForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.timeZoneForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.timeZoneForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const timeZoneSubmitData: TimeZoneSubmitData = {
        id: this.timeZoneSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        ianaTimeZone: formValue.ianaTimeZone!.trim(),
        abbreviation: formValue.abbreviation!.trim(),
        abbreviationDaylightSavings: formValue.abbreviationDaylightSavings!.trim(),
        supportsDaylightSavings: !!formValue.supportsDaylightSavings,
        standardUTCOffsetHours: Number(formValue.standardUTCOffsetHours),
        dstUTCOffsetHours: Number(formValue.dstUTCOffsetHours),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateTimeZone(timeZoneSubmitData);
      } else {
        this.addTimeZone(timeZoneSubmitData);
      }
  }

  private addTimeZone(timeZoneData: TimeZoneSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    timeZoneData.active = true;
    timeZoneData.deleted = false;
    this.timeZoneService.PostTimeZone(timeZoneData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTimeZone) => {

        this.timeZoneService.ClearAllCaches();

        this.timeZoneChanged.next([newTimeZone]);

        this.alertService.showMessage("Time Zone added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/timezone', newTimeZone.id]);
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
                                   'You do not have permission to save this Time Zone.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Time Zone.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Time Zone could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTimeZone(timeZoneData: TimeZoneSubmitData) {
    this.timeZoneService.PutTimeZone(timeZoneData.id, timeZoneData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTimeZone) => {

        this.timeZoneService.ClearAllCaches();

        this.timeZoneChanged.next([updatedTimeZone]);

        this.alertService.showMessage("Time Zone updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Time Zone.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Time Zone.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Time Zone could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(timeZoneData: TimeZoneData | null) {

    if (timeZoneData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.timeZoneForm.reset({
        name: '',
        description: '',
        ianaTimeZone: '',
        abbreviation: '',
        abbreviationDaylightSavings: '',
        supportsDaylightSavings: false,
        standardUTCOffsetHours: '',
        dstUTCOffsetHours: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.timeZoneForm.reset({
        name: timeZoneData.name ?? '',
        description: timeZoneData.description ?? '',
        ianaTimeZone: timeZoneData.ianaTimeZone ?? '',
        abbreviation: timeZoneData.abbreviation ?? '',
        abbreviationDaylightSavings: timeZoneData.abbreviationDaylightSavings ?? '',
        supportsDaylightSavings: timeZoneData.supportsDaylightSavings ?? false,
        standardUTCOffsetHours: timeZoneData.standardUTCOffsetHours?.toString() ?? '',
        dstUTCOffsetHours: timeZoneData.dstUTCOffsetHours?.toString() ?? '',
        sequence: timeZoneData.sequence?.toString() ?? '',
        active: timeZoneData.active ?? true,
        deleted: timeZoneData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.timeZoneForm.markAsPristine();
    this.timeZoneForm.markAsUntouched();
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


  public userIsSchedulerTimeZoneReader(): boolean {
    return this.timeZoneService.userIsSchedulerTimeZoneReader();
  }

  public userIsSchedulerTimeZoneWriter(): boolean {
    return this.timeZoneService.userIsSchedulerTimeZoneWriter();
  }
}
