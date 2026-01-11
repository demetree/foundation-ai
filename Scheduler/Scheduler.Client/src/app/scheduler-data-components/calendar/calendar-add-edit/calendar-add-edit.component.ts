/*
   GENERATED FORM FOR THE CALENDAR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Calendar table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to calendar-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CalendarService, CalendarData, CalendarSubmitData } from '../../../scheduler-data-services/calendar.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface CalendarFormValues {
  name: string,
  description: string | null,
  officeId: number | bigint | null,       // For FK link number
  isDefault: boolean | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
};

@Component({
  selector: 'app-calendar-add-edit',
  templateUrl: './calendar-add-edit.component.html',
  styleUrls: ['./calendar-add-edit.component.scss']
})
export class CalendarAddEditComponent {
  @ViewChild('calendarModal') calendarModal!: TemplateRef<any>;
  @Output() calendarChanged = new Subject<CalendarData[]>();
  @Input() calendarSubmitData: CalendarSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CalendarFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public calendarForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        officeId: [null],
        isDefault: [false],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
        versionNumber: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  calendars$ = this.calendarService.GetCalendarList();
  offices$ = this.officeService.GetOfficeList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private calendarService: CalendarService,
    private officeService: OfficeService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(calendarData?: CalendarData) {

    if (calendarData != null) {

      if (!this.calendarService.userIsSchedulerCalendarReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Calendars`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.calendarSubmitData = this.calendarService.ConvertToCalendarSubmitData(calendarData);
      this.isEditMode = true;
      this.objectGuid = calendarData.objectGuid;

      this.buildFormValues(calendarData);

    } else {

      if (!this.calendarService.userIsSchedulerCalendarWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Calendars`,
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
        this.calendarForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.calendarForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.calendarModal, {
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

    if (this.calendarService.userIsSchedulerCalendarWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Calendars`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.calendarForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.calendarForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.calendarForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const calendarSubmitData: CalendarSubmitData = {
        id: this.calendarSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        isDefault: formValue.isDefault == true ? true : formValue.isDefault == false ? false : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
        versionNumber: this.calendarSubmitData?.versionNumber ?? 0,
   };

      if (this.isEditMode) {
        this.updateCalendar(calendarSubmitData);
      } else {
        this.addCalendar(calendarSubmitData);
      }
  }

  private addCalendar(calendarData: CalendarSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    calendarData.versionNumber = 0;
    calendarData.active = true;
    calendarData.deleted = false;
    this.calendarService.PostCalendar(calendarData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCalendar) => {

        this.calendarService.ClearAllCaches();

        this.calendarChanged.next([newCalendar]);

        this.alertService.showMessage("Calendar added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/calendar', newCalendar.id]);
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
                                   'You do not have permission to save this Calendar.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Calendar.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Calendar could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCalendar(calendarData: CalendarSubmitData) {
    this.calendarService.PutCalendar(calendarData.id, calendarData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCalendar) => {

        this.calendarService.ClearAllCaches();

        this.calendarChanged.next([updatedCalendar]);

        this.alertService.showMessage("Calendar updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Calendar.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Calendar.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Calendar could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(calendarData: CalendarData | null) {

    if (calendarData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.calendarForm.reset({
        name: '',
        description: '',
        officeId: null,
        isDefault: false,
        iconId: null,
        color: '',
        active: true,
        deleted: false,
        versionNumber: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.calendarForm.reset({
        name: calendarData.name ?? '',
        description: calendarData.description ?? '',
        officeId: calendarData.officeId,
        isDefault: calendarData.isDefault ?? false,
        iconId: calendarData.iconId,
        color: calendarData.color ?? '',
        active: calendarData.active ?? true,
        deleted: calendarData.deleted ?? false,
        versionNumber: calendarData.versionNumber?.toString() ?? '',
      }, { emitEvent: false});
    }

    this.calendarForm.markAsPristine();
    this.calendarForm.markAsUntouched();
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


  public userIsSchedulerCalendarReader(): boolean {
    return this.calendarService.userIsSchedulerCalendarReader();
  }

  public userIsSchedulerCalendarWriter(): boolean {
    return this.calendarService.userIsSchedulerCalendarWriter();
  }
}
