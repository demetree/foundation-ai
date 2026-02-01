/*
   GENERATED FORM FOR THE SCHEDULELAYER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduleLayer table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to schedule-layer-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduleLayerService, ScheduleLayerData, ScheduleLayerSubmitData } from '../../../alerting-data-services/schedule-layer.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OnCallScheduleService } from '../../../alerting-data-services/on-call-schedule.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ScheduleLayerFormValues {
  onCallScheduleId: number | bigint,       // For FK link number
  name: string,
  description: string | null,
  layerLevel: string,     // Stored as string for form input, converted to number on submit.
  rotationStart: string,
  rotationDays: string,     // Stored as string for form input, converted to number on submit.
  handoffTime: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-schedule-layer-add-edit',
  templateUrl: './schedule-layer-add-edit.component.html',
  styleUrls: ['./schedule-layer-add-edit.component.scss']
})
export class ScheduleLayerAddEditComponent {
  @ViewChild('scheduleLayerModal') scheduleLayerModal!: TemplateRef<any>;
  @Output() scheduleLayerChanged = new Subject<ScheduleLayerData[]>();
  @Input() scheduleLayerSubmitData: ScheduleLayerSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduleLayerFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduleLayerForm: FormGroup = this.fb.group({
        onCallScheduleId: [null, Validators.required],
        name: ['', Validators.required],
        description: [''],
        layerLevel: ['', Validators.required],
        rotationStart: ['', Validators.required],
        rotationDays: ['', Validators.required],
        handoffTime: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  scheduleLayers$ = this.scheduleLayerService.GetScheduleLayerList();
  onCallSchedules$ = this.onCallScheduleService.GetOnCallScheduleList();

  constructor(
    private modalService: NgbModal,
    private scheduleLayerService: ScheduleLayerService,
    private onCallScheduleService: OnCallScheduleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(scheduleLayerData?: ScheduleLayerData) {

    if (scheduleLayerData != null) {

      if (!this.scheduleLayerService.userIsAlertingScheduleLayerReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Schedule Layers`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.scheduleLayerSubmitData = this.scheduleLayerService.ConvertToScheduleLayerSubmitData(scheduleLayerData);
      this.isEditMode = true;
      this.objectGuid = scheduleLayerData.objectGuid;

      this.buildFormValues(scheduleLayerData);

    } else {

      if (!this.scheduleLayerService.userIsAlertingScheduleLayerWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Schedule Layers`,
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
        this.scheduleLayerForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduleLayerForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.scheduleLayerModal, {
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

    if (this.scheduleLayerService.userIsAlertingScheduleLayerWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Schedule Layers`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.scheduleLayerForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduleLayerForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduleLayerForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduleLayerSubmitData: ScheduleLayerSubmitData = {
        id: this.scheduleLayerSubmitData?.id || 0,
        onCallScheduleId: Number(formValue.onCallScheduleId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        layerLevel: Number(formValue.layerLevel),
        rotationStart: dateTimeLocalToIsoUtc(formValue.rotationStart!.trim())!,
        rotationDays: Number(formValue.rotationDays),
        handoffTime: formValue.handoffTime!.trim(),
        versionNumber: this.scheduleLayerSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateScheduleLayer(scheduleLayerSubmitData);
      } else {
        this.addScheduleLayer(scheduleLayerSubmitData);
      }
  }

  private addScheduleLayer(scheduleLayerData: ScheduleLayerSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    scheduleLayerData.versionNumber = 0;
    scheduleLayerData.active = true;
    scheduleLayerData.deleted = false;
    this.scheduleLayerService.PostScheduleLayer(scheduleLayerData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newScheduleLayer) => {

        this.scheduleLayerService.ClearAllCaches();

        this.scheduleLayerChanged.next([newScheduleLayer]);

        this.alertService.showMessage("Schedule Layer added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/schedulelayer', newScheduleLayer.id]);
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
                                   'You do not have permission to save this Schedule Layer.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Schedule Layer.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Schedule Layer could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateScheduleLayer(scheduleLayerData: ScheduleLayerSubmitData) {
    this.scheduleLayerService.PutScheduleLayer(scheduleLayerData.id, scheduleLayerData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedScheduleLayer) => {

        this.scheduleLayerService.ClearAllCaches();

        this.scheduleLayerChanged.next([updatedScheduleLayer]);

        this.alertService.showMessage("Schedule Layer updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Schedule Layer.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Schedule Layer.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Schedule Layer could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(scheduleLayerData: ScheduleLayerData | null) {

    if (scheduleLayerData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduleLayerForm.reset({
        onCallScheduleId: null,
        name: '',
        description: '',
        layerLevel: '',
        rotationStart: '',
        rotationDays: '',
        handoffTime: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduleLayerForm.reset({
        onCallScheduleId: scheduleLayerData.onCallScheduleId,
        name: scheduleLayerData.name ?? '',
        description: scheduleLayerData.description ?? '',
        layerLevel: scheduleLayerData.layerLevel?.toString() ?? '',
        rotationStart: isoUtcStringToDateTimeLocal(scheduleLayerData.rotationStart) ?? '',
        rotationDays: scheduleLayerData.rotationDays?.toString() ?? '',
        handoffTime: scheduleLayerData.handoffTime ?? '',
        versionNumber: scheduleLayerData.versionNumber?.toString() ?? '',
        active: scheduleLayerData.active ?? true,
        deleted: scheduleLayerData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduleLayerForm.markAsPristine();
    this.scheduleLayerForm.markAsUntouched();
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


  public userIsAlertingScheduleLayerReader(): boolean {
    return this.scheduleLayerService.userIsAlertingScheduleLayerReader();
  }

  public userIsAlertingScheduleLayerWriter(): boolean {
    return this.scheduleLayerService.userIsAlertingScheduleLayerWriter();
  }
}
