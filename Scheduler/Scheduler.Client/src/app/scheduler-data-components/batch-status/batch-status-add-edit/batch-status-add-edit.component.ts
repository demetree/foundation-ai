/*
   GENERATED FORM FOR THE BATCHSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BatchStatus table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to batch-status-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BatchStatusService, BatchStatusData, BatchStatusSubmitData } from '../../../scheduler-data-services/batch-status.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BatchStatusFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-batch-status-add-edit',
  templateUrl: './batch-status-add-edit.component.html',
  styleUrls: ['./batch-status-add-edit.component.scss']
})
export class BatchStatusAddEditComponent {
  @ViewChild('batchStatusModal') batchStatusModal!: TemplateRef<any>;
  @Output() batchStatusChanged = new Subject<BatchStatusData[]>();
  @Input() batchStatusSubmitData: BatchStatusSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BatchStatusFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public batchStatusForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  batchStatuses$ = this.batchStatusService.GetBatchStatusList();

  constructor(
    private modalService: NgbModal,
    private batchStatusService: BatchStatusService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(batchStatusData?: BatchStatusData) {

    if (batchStatusData != null) {

      if (!this.batchStatusService.userIsSchedulerBatchStatusReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Batch Statuses`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.batchStatusSubmitData = this.batchStatusService.ConvertToBatchStatusSubmitData(batchStatusData);
      this.isEditMode = true;
      this.objectGuid = batchStatusData.objectGuid;

      this.buildFormValues(batchStatusData);

    } else {

      if (!this.batchStatusService.userIsSchedulerBatchStatusWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Batch Statuses`,
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
        this.batchStatusForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.batchStatusForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.batchStatusModal, {
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

    if (this.batchStatusService.userIsSchedulerBatchStatusWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Batch Statuses`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.batchStatusForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.batchStatusForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.batchStatusForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const batchStatusSubmitData: BatchStatusSubmitData = {
        id: this.batchStatusSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBatchStatus(batchStatusSubmitData);
      } else {
        this.addBatchStatus(batchStatusSubmitData);
      }
  }

  private addBatchStatus(batchStatusData: BatchStatusSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    batchStatusData.active = true;
    batchStatusData.deleted = false;
    this.batchStatusService.PostBatchStatus(batchStatusData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBatchStatus) => {

        this.batchStatusService.ClearAllCaches();

        this.batchStatusChanged.next([newBatchStatus]);

        this.alertService.showMessage("Batch Status added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/batchstatus', newBatchStatus.id]);
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
                                   'You do not have permission to save this Batch Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Batch Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Batch Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBatchStatus(batchStatusData: BatchStatusSubmitData) {
    this.batchStatusService.PutBatchStatus(batchStatusData.id, batchStatusData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBatchStatus) => {

        this.batchStatusService.ClearAllCaches();

        this.batchStatusChanged.next([updatedBatchStatus]);

        this.alertService.showMessage("Batch Status updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Batch Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Batch Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Batch Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(batchStatusData: BatchStatusData | null) {

    if (batchStatusData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.batchStatusForm.reset({
        name: '',
        description: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.batchStatusForm.reset({
        name: batchStatusData.name ?? '',
        description: batchStatusData.description ?? '',
        sequence: batchStatusData.sequence?.toString() ?? '',
        active: batchStatusData.active ?? true,
        deleted: batchStatusData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.batchStatusForm.markAsPristine();
    this.batchStatusForm.markAsUntouched();
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


  public userIsSchedulerBatchStatusReader(): boolean {
    return this.batchStatusService.userIsSchedulerBatchStatusReader();
  }

  public userIsSchedulerBatchStatusWriter(): boolean {
    return this.batchStatusService.userIsSchedulerBatchStatusWriter();
  }
}
