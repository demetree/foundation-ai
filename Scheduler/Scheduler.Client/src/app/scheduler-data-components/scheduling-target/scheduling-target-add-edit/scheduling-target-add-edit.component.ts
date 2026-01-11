/*
   GENERATED FORM FOR THE SCHEDULINGTARGET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTarget table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetService, SchedulingTargetData, SchedulingTargetSubmitData } from '../../../scheduler-data-services/scheduling-target.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { SchedulingTargetTypeService } from '../../../scheduler-data-services/scheduling-target-type.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SchedulingTargetFormValues {
  name: string,
  description: string | null,
  officeId: number | bigint | null,       // For FK link number
  clientId: number | bigint,       // For FK link number
  schedulingTargetTypeId: number | bigint,       // For FK link number
  timeZoneId: number | bigint,       // For FK link number
  calendarId: number | bigint | null,       // For FK link number
  notes: string | null,
  externalId: string | null,
  color: string | null,
  attributes: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-scheduling-target-add-edit',
  templateUrl: './scheduling-target-add-edit.component.html',
  styleUrls: ['./scheduling-target-add-edit.component.scss']
})
export class SchedulingTargetAddEditComponent {
  @ViewChild('schedulingTargetModal') schedulingTargetModal!: TemplateRef<any>;
  @Output() schedulingTargetChanged = new Subject<SchedulingTargetData[]>();
  @Input() schedulingTargetSubmitData: SchedulingTargetSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        officeId: [null],
        clientId: [null, Validators.required],
        schedulingTargetTypeId: [null, Validators.required],
        timeZoneId: [null, Validators.required],
        calendarId: [null],
        notes: [''],
        externalId: [''],
        color: [''],
        attributes: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  offices$ = this.officeService.GetOfficeList();
  clients$ = this.clientService.GetClientList();
  schedulingTargetTypes$ = this.schedulingTargetTypeService.GetSchedulingTargetTypeList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();
  calendars$ = this.calendarService.GetCalendarList();

  constructor(
    private modalService: NgbModal,
    private schedulingTargetService: SchedulingTargetService,
    private officeService: OfficeService,
    private clientService: ClientService,
    private schedulingTargetTypeService: SchedulingTargetTypeService,
    private timeZoneService: TimeZoneService,
    private calendarService: CalendarService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(schedulingTargetData?: SchedulingTargetData) {

    if (schedulingTargetData != null) {

      if (!this.schedulingTargetService.userIsSchedulerSchedulingTargetReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduling Targets`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.schedulingTargetSubmitData = this.schedulingTargetService.ConvertToSchedulingTargetSubmitData(schedulingTargetData);
      this.isEditMode = true;
      this.objectGuid = schedulingTargetData.objectGuid;

      this.buildFormValues(schedulingTargetData);

    } else {

      if (!this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduling Targets`,
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
        this.schedulingTargetForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.schedulingTargetModal, {
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

    if (this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduling Targets`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.schedulingTargetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetSubmitData: SchedulingTargetSubmitData = {
        id: this.schedulingTargetSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        clientId: Number(formValue.clientId),
        schedulingTargetTypeId: Number(formValue.schedulingTargetTypeId),
        timeZoneId: Number(formValue.timeZoneId),
        calendarId: formValue.calendarId ? Number(formValue.calendarId) : null,
        notes: formValue.notes?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        color: formValue.color?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.schedulingTargetSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSchedulingTarget(schedulingTargetSubmitData);
      } else {
        this.addSchedulingTarget(schedulingTargetSubmitData);
      }
  }

  private addSchedulingTarget(schedulingTargetData: SchedulingTargetSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    schedulingTargetData.versionNumber = 0;
    schedulingTargetData.active = true;
    schedulingTargetData.deleted = false;
    this.schedulingTargetService.PostSchedulingTarget(schedulingTargetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSchedulingTarget) => {

        this.schedulingTargetService.ClearAllCaches();

        this.schedulingTargetChanged.next([newSchedulingTarget]);

        this.alertService.showMessage("Scheduling Target added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/schedulingtarget', newSchedulingTarget.id]);
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
                                   'You do not have permission to save this Scheduling Target.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSchedulingTarget(schedulingTargetData: SchedulingTargetSubmitData) {
    this.schedulingTargetService.PutSchedulingTarget(schedulingTargetData.id, schedulingTargetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSchedulingTarget) => {

        this.schedulingTargetService.ClearAllCaches();

        this.schedulingTargetChanged.next([updatedSchedulingTarget]);

        this.alertService.showMessage("Scheduling Target updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduling Target.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(schedulingTargetData: SchedulingTargetData | null) {

    if (schedulingTargetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetForm.reset({
        name: '',
        description: '',
        officeId: null,
        clientId: null,
        schedulingTargetTypeId: null,
        timeZoneId: null,
        calendarId: null,
        notes: '',
        externalId: '',
        color: '',
        attributes: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.schedulingTargetForm.reset({
        name: schedulingTargetData.name ?? '',
        description: schedulingTargetData.description ?? '',
        officeId: schedulingTargetData.officeId,
        clientId: schedulingTargetData.clientId,
        schedulingTargetTypeId: schedulingTargetData.schedulingTargetTypeId,
        timeZoneId: schedulingTargetData.timeZoneId,
        calendarId: schedulingTargetData.calendarId,
        notes: schedulingTargetData.notes ?? '',
        externalId: schedulingTargetData.externalId ?? '',
        color: schedulingTargetData.color ?? '',
        attributes: schedulingTargetData.attributes ?? '',
        avatarFileName: schedulingTargetData.avatarFileName ?? '',
        avatarSize: schedulingTargetData.avatarSize?.toString() ?? '',
        avatarData: schedulingTargetData.avatarData ?? '',
        avatarMimeType: schedulingTargetData.avatarMimeType ?? '',
        versionNumber: schedulingTargetData.versionNumber?.toString() ?? '',
        active: schedulingTargetData.active ?? true,
        deleted: schedulingTargetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.schedulingTargetForm.markAsPristine();
    this.schedulingTargetForm.markAsUntouched();
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


  public userIsSchedulerSchedulingTargetReader(): boolean {
    return this.schedulingTargetService.userIsSchedulerSchedulingTargetReader();
  }

  public userIsSchedulerSchedulingTargetWriter(): boolean {
    return this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter();
  }
}
