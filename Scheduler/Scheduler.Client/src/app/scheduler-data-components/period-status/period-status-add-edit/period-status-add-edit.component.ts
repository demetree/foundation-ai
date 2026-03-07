/*
   GENERATED FORM FOR THE PERIODSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PeriodStatus table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to period-status-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PeriodStatusService, PeriodStatusData, PeriodStatusSubmitData } from '../../../scheduler-data-services/period-status.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PeriodStatusFormValues {
  name: string,
  description: string,
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-period-status-add-edit',
  templateUrl: './period-status-add-edit.component.html',
  styleUrls: ['./period-status-add-edit.component.scss']
})
export class PeriodStatusAddEditComponent {
  @ViewChild('periodStatusModal') periodStatusModal!: TemplateRef<any>;
  @Output() periodStatusChanged = new Subject<PeriodStatusData[]>();
  @Input() periodStatusSubmitData: PeriodStatusSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PeriodStatusFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public periodStatusForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        color: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  periodStatuses$ = this.periodStatusService.GetPeriodStatusList();

  constructor(
    private modalService: NgbModal,
    private periodStatusService: PeriodStatusService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(periodStatusData?: PeriodStatusData) {

    if (periodStatusData != null) {

      if (!this.periodStatusService.userIsSchedulerPeriodStatusReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Period Statuses`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.periodStatusSubmitData = this.periodStatusService.ConvertToPeriodStatusSubmitData(periodStatusData);
      this.isEditMode = true;
      this.objectGuid = periodStatusData.objectGuid;

      this.buildFormValues(periodStatusData);

    } else {

      if (!this.periodStatusService.userIsSchedulerPeriodStatusWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Period Statuses`,
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
        this.periodStatusForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.periodStatusForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.periodStatusModal, {
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

    if (this.periodStatusService.userIsSchedulerPeriodStatusWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Period Statuses`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.periodStatusForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.periodStatusForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.periodStatusForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const periodStatusSubmitData: PeriodStatusSubmitData = {
        id: this.periodStatusSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePeriodStatus(periodStatusSubmitData);
      } else {
        this.addPeriodStatus(periodStatusSubmitData);
      }
  }

  private addPeriodStatus(periodStatusData: PeriodStatusSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    periodStatusData.active = true;
    periodStatusData.deleted = false;
    this.periodStatusService.PostPeriodStatus(periodStatusData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPeriodStatus) => {

        this.periodStatusService.ClearAllCaches();

        this.periodStatusChanged.next([newPeriodStatus]);

        this.alertService.showMessage("Period Status added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/periodstatus', newPeriodStatus.id]);
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
                                   'You do not have permission to save this Period Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Period Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Period Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePeriodStatus(periodStatusData: PeriodStatusSubmitData) {
    this.periodStatusService.PutPeriodStatus(periodStatusData.id, periodStatusData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPeriodStatus) => {

        this.periodStatusService.ClearAllCaches();

        this.periodStatusChanged.next([updatedPeriodStatus]);

        this.alertService.showMessage("Period Status updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Period Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Period Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Period Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(periodStatusData: PeriodStatusData | null) {

    if (periodStatusData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.periodStatusForm.reset({
        name: '',
        description: '',
        color: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.periodStatusForm.reset({
        name: periodStatusData.name ?? '',
        description: periodStatusData.description ?? '',
        color: periodStatusData.color ?? '',
        sequence: periodStatusData.sequence?.toString() ?? '',
        active: periodStatusData.active ?? true,
        deleted: periodStatusData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.periodStatusForm.markAsPristine();
    this.periodStatusForm.markAsUntouched();
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


  public userIsSchedulerPeriodStatusReader(): boolean {
    return this.periodStatusService.userIsSchedulerPeriodStatusReader();
  }

  public userIsSchedulerPeriodStatusWriter(): boolean {
    return this.periodStatusService.userIsSchedulerPeriodStatusWriter();
  }
}
