/*
   GENERATED FORM FOR THE SCHEDULEOVERRIDE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduleOverride table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to schedule-override-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduleOverrideService, ScheduleOverrideData, ScheduleOverrideSubmitData } from '../../../alerting-data-services/schedule-override.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OnCallScheduleService } from '../../../alerting-data-services/on-call-schedule.service';
import { ScheduleLayerService } from '../../../alerting-data-services/schedule-layer.service';
import { ScheduleOverrideTypeService } from '../../../alerting-data-services/schedule-override-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ScheduleOverrideFormValues {
  onCallScheduleId: number | bigint,       // For FK link number
  scheduleLayerId: number | bigint | null,       // For FK link number
  startDateTime: string,
  endDateTime: string,
  scheduleOverrideTypeId: number | bigint,       // For FK link number
  originalUserObjectGuid: string | null,
  replacementUserObjectGuid: string | null,
  reason: string | null,
  createdByUserObjectGuid: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-schedule-override-add-edit',
  templateUrl: './schedule-override-add-edit.component.html',
  styleUrls: ['./schedule-override-add-edit.component.scss']
})
export class ScheduleOverrideAddEditComponent {
  @ViewChild('scheduleOverrideModal') scheduleOverrideModal!: TemplateRef<any>;
  @Output() scheduleOverrideChanged = new Subject<ScheduleOverrideData[]>();
  @Input() scheduleOverrideSubmitData: ScheduleOverrideSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduleOverrideFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduleOverrideForm: FormGroup = this.fb.group({
        onCallScheduleId: [null, Validators.required],
        scheduleLayerId: [null],
        startDateTime: ['', Validators.required],
        endDateTime: ['', Validators.required],
        scheduleOverrideTypeId: [null, Validators.required],
        originalUserObjectGuid: [''],
        replacementUserObjectGuid: [''],
        reason: [''],
        createdByUserObjectGuid: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  scheduleOverrides$ = this.scheduleOverrideService.GetScheduleOverrideList();
  onCallSchedules$ = this.onCallScheduleService.GetOnCallScheduleList();
  scheduleLayers$ = this.scheduleLayerService.GetScheduleLayerList();
  scheduleOverrideTypes$ = this.scheduleOverrideTypeService.GetScheduleOverrideTypeList();

  constructor(
    private modalService: NgbModal,
    private scheduleOverrideService: ScheduleOverrideService,
    private onCallScheduleService: OnCallScheduleService,
    private scheduleLayerService: ScheduleLayerService,
    private scheduleOverrideTypeService: ScheduleOverrideTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(scheduleOverrideData?: ScheduleOverrideData) {

    if (scheduleOverrideData != null) {

      if (!this.scheduleOverrideService.userIsAlertingScheduleOverrideReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Schedule Overrides`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.scheduleOverrideSubmitData = this.scheduleOverrideService.ConvertToScheduleOverrideSubmitData(scheduleOverrideData);
      this.isEditMode = true;
      this.objectGuid = scheduleOverrideData.objectGuid;

      this.buildFormValues(scheduleOverrideData);

    } else {

      if (!this.scheduleOverrideService.userIsAlertingScheduleOverrideWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Schedule Overrides`,
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
        this.scheduleOverrideForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduleOverrideForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.scheduleOverrideModal, {
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

    if (this.scheduleOverrideService.userIsAlertingScheduleOverrideWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Schedule Overrides`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.scheduleOverrideForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduleOverrideForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduleOverrideForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduleOverrideSubmitData: ScheduleOverrideSubmitData = {
        id: this.scheduleOverrideSubmitData?.id || 0,
        onCallScheduleId: Number(formValue.onCallScheduleId),
        scheduleLayerId: formValue.scheduleLayerId ? Number(formValue.scheduleLayerId) : null,
        startDateTime: dateTimeLocalToIsoUtc(formValue.startDateTime!.trim())!,
        endDateTime: dateTimeLocalToIsoUtc(formValue.endDateTime!.trim())!,
        scheduleOverrideTypeId: Number(formValue.scheduleOverrideTypeId),
        originalUserObjectGuid: formValue.originalUserObjectGuid?.trim() || null,
        replacementUserObjectGuid: formValue.replacementUserObjectGuid?.trim() || null,
        reason: formValue.reason?.trim() || null,
        createdByUserObjectGuid: formValue.createdByUserObjectGuid!.trim(),
        versionNumber: this.scheduleOverrideSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateScheduleOverride(scheduleOverrideSubmitData);
      } else {
        this.addScheduleOverride(scheduleOverrideSubmitData);
      }
  }

  private addScheduleOverride(scheduleOverrideData: ScheduleOverrideSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    scheduleOverrideData.versionNumber = 0;
    scheduleOverrideData.active = true;
    scheduleOverrideData.deleted = false;
    this.scheduleOverrideService.PostScheduleOverride(scheduleOverrideData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newScheduleOverride) => {

        this.scheduleOverrideService.ClearAllCaches();

        this.scheduleOverrideChanged.next([newScheduleOverride]);

        this.alertService.showMessage("Schedule Override added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/scheduleoverride', newScheduleOverride.id]);
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
                                   'You do not have permission to save this Schedule Override.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Schedule Override.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Schedule Override could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateScheduleOverride(scheduleOverrideData: ScheduleOverrideSubmitData) {
    this.scheduleOverrideService.PutScheduleOverride(scheduleOverrideData.id, scheduleOverrideData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedScheduleOverride) => {

        this.scheduleOverrideService.ClearAllCaches();

        this.scheduleOverrideChanged.next([updatedScheduleOverride]);

        this.alertService.showMessage("Schedule Override updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Schedule Override.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Schedule Override.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Schedule Override could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(scheduleOverrideData: ScheduleOverrideData | null) {

    if (scheduleOverrideData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduleOverrideForm.reset({
        onCallScheduleId: null,
        scheduleLayerId: null,
        startDateTime: '',
        endDateTime: '',
        scheduleOverrideTypeId: null,
        originalUserObjectGuid: '',
        replacementUserObjectGuid: '',
        reason: '',
        createdByUserObjectGuid: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduleOverrideForm.reset({
        onCallScheduleId: scheduleOverrideData.onCallScheduleId,
        scheduleLayerId: scheduleOverrideData.scheduleLayerId,
        startDateTime: isoUtcStringToDateTimeLocal(scheduleOverrideData.startDateTime) ?? '',
        endDateTime: isoUtcStringToDateTimeLocal(scheduleOverrideData.endDateTime) ?? '',
        scheduleOverrideTypeId: scheduleOverrideData.scheduleOverrideTypeId,
        originalUserObjectGuid: scheduleOverrideData.originalUserObjectGuid ?? '',
        replacementUserObjectGuid: scheduleOverrideData.replacementUserObjectGuid ?? '',
        reason: scheduleOverrideData.reason ?? '',
        createdByUserObjectGuid: scheduleOverrideData.createdByUserObjectGuid ?? '',
        versionNumber: scheduleOverrideData.versionNumber?.toString() ?? '',
        active: scheduleOverrideData.active ?? true,
        deleted: scheduleOverrideData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduleOverrideForm.markAsPristine();
    this.scheduleOverrideForm.markAsUntouched();
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


  public userIsAlertingScheduleOverrideReader(): boolean {
    return this.scheduleOverrideService.userIsAlertingScheduleOverrideReader();
  }

  public userIsAlertingScheduleOverrideWriter(): boolean {
    return this.scheduleOverrideService.userIsAlertingScheduleOverrideWriter();
  }
}
