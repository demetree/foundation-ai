/*
   GENERATED FORM FOR THE SCHEDULEDEVENTTEMPLATECHARGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventTemplateCharge table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-template-charge-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventTemplateChargeService, ScheduledEventTemplateChargeData, ScheduledEventTemplateChargeSubmitData } from '../../../scheduler-data-services/scheduled-event-template-charge.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduledEventTemplateService } from '../../../scheduler-data-services/scheduled-event-template.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ScheduledEventTemplateChargeFormValues {
  scheduledEventTemplateId: number | bigint,       // For FK link number
  chargeTypeId: number | bigint,       // For FK link number
  defaultAmount: string,     // Stored as string for form input, converted to number on submit.
  isRequired: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-scheduled-event-template-charge-add-edit',
  templateUrl: './scheduled-event-template-charge-add-edit.component.html',
  styleUrls: ['./scheduled-event-template-charge-add-edit.component.scss']
})
export class ScheduledEventTemplateChargeAddEditComponent {
  @ViewChild('scheduledEventTemplateChargeModal') scheduledEventTemplateChargeModal!: TemplateRef<any>;
  @Output() scheduledEventTemplateChargeChanged = new Subject<ScheduledEventTemplateChargeData[]>();
  @Input() scheduledEventTemplateChargeSubmitData: ScheduledEventTemplateChargeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventTemplateChargeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventTemplateChargeForm: FormGroup = this.fb.group({
        scheduledEventTemplateId: [null, Validators.required],
        chargeTypeId: [null, Validators.required],
        defaultAmount: ['', Validators.required],
        isRequired: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  scheduledEventTemplateCharges$ = this.scheduledEventTemplateChargeService.GetScheduledEventTemplateChargeList();
  scheduledEventTemplates$ = this.scheduledEventTemplateService.GetScheduledEventTemplateList();
  chargeTypes$ = this.chargeTypeService.GetChargeTypeList();

  constructor(
    private modalService: NgbModal,
    private scheduledEventTemplateChargeService: ScheduledEventTemplateChargeService,
    private scheduledEventTemplateService: ScheduledEventTemplateService,
    private chargeTypeService: ChargeTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(scheduledEventTemplateChargeData?: ScheduledEventTemplateChargeData) {

    if (scheduledEventTemplateChargeData != null) {

      if (!this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduled Event Template Charges`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.scheduledEventTemplateChargeSubmitData = this.scheduledEventTemplateChargeService.ConvertToScheduledEventTemplateChargeSubmitData(scheduledEventTemplateChargeData);
      this.isEditMode = true;
      this.objectGuid = scheduledEventTemplateChargeData.objectGuid;

      this.buildFormValues(scheduledEventTemplateChargeData);

    } else {

      if (!this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Template Charges`,
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
        this.scheduledEventTemplateChargeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventTemplateChargeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.scheduledEventTemplateChargeModal, {
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

    if (this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Template Charges`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.scheduledEventTemplateChargeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventTemplateChargeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventTemplateChargeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventTemplateChargeSubmitData: ScheduledEventTemplateChargeSubmitData = {
        id: this.scheduledEventTemplateChargeSubmitData?.id || 0,
        scheduledEventTemplateId: Number(formValue.scheduledEventTemplateId),
        chargeTypeId: Number(formValue.chargeTypeId),
        defaultAmount: Number(formValue.defaultAmount),
        isRequired: !!formValue.isRequired,
        versionNumber: this.scheduledEventTemplateChargeSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateScheduledEventTemplateCharge(scheduledEventTemplateChargeSubmitData);
      } else {
        this.addScheduledEventTemplateCharge(scheduledEventTemplateChargeSubmitData);
      }
  }

  private addScheduledEventTemplateCharge(scheduledEventTemplateChargeData: ScheduledEventTemplateChargeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    scheduledEventTemplateChargeData.versionNumber = 0;
    scheduledEventTemplateChargeData.active = true;
    scheduledEventTemplateChargeData.deleted = false;
    this.scheduledEventTemplateChargeService.PostScheduledEventTemplateCharge(scheduledEventTemplateChargeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newScheduledEventTemplateCharge) => {

        this.scheduledEventTemplateChargeService.ClearAllCaches();

        this.scheduledEventTemplateChargeChanged.next([newScheduledEventTemplateCharge]);

        this.alertService.showMessage("Scheduled Event Template Charge added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/scheduledeventtemplatecharge', newScheduledEventTemplateCharge.id]);
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
                                   'You do not have permission to save this Scheduled Event Template Charge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Template Charge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Template Charge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateScheduledEventTemplateCharge(scheduledEventTemplateChargeData: ScheduledEventTemplateChargeSubmitData) {
    this.scheduledEventTemplateChargeService.PutScheduledEventTemplateCharge(scheduledEventTemplateChargeData.id, scheduledEventTemplateChargeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedScheduledEventTemplateCharge) => {

        this.scheduledEventTemplateChargeService.ClearAllCaches();

        this.scheduledEventTemplateChargeChanged.next([updatedScheduledEventTemplateCharge]);

        this.alertService.showMessage("Scheduled Event Template Charge updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduled Event Template Charge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Template Charge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Template Charge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(scheduledEventTemplateChargeData: ScheduledEventTemplateChargeData | null) {

    if (scheduledEventTemplateChargeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventTemplateChargeForm.reset({
        scheduledEventTemplateId: null,
        chargeTypeId: null,
        defaultAmount: '',
        isRequired: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduledEventTemplateChargeForm.reset({
        scheduledEventTemplateId: scheduledEventTemplateChargeData.scheduledEventTemplateId,
        chargeTypeId: scheduledEventTemplateChargeData.chargeTypeId,
        defaultAmount: scheduledEventTemplateChargeData.defaultAmount?.toString() ?? '',
        isRequired: scheduledEventTemplateChargeData.isRequired ?? false,
        versionNumber: scheduledEventTemplateChargeData.versionNumber?.toString() ?? '',
        active: scheduledEventTemplateChargeData.active ?? true,
        deleted: scheduledEventTemplateChargeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduledEventTemplateChargeForm.markAsPristine();
    this.scheduledEventTemplateChargeForm.markAsUntouched();
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


  public userIsSchedulerScheduledEventTemplateChargeReader(): boolean {
    return this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeReader();
  }

  public userIsSchedulerScheduledEventTemplateChargeWriter(): boolean {
    return this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeWriter();
  }
}
