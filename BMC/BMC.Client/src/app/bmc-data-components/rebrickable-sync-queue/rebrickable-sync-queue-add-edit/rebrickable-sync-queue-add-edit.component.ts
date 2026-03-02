/*
   GENERATED FORM FOR THE REBRICKABLESYNCQUEUE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RebrickableSyncQueue table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to rebrickable-sync-queue-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RebrickableSyncQueueService, RebrickableSyncQueueData, RebrickableSyncQueueSubmitData } from '../../../bmc-data-services/rebrickable-sync-queue.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface RebrickableSyncQueueFormValues {
  operationType: string,
  entityType: string,
  entityId: string,     // Stored as string for form input, converted to number on submit.
  payload: string | null,
  status: string,
  createdDate: string | null,
  lastAttemptDate: string | null,
  completedDate: string | null,
  attemptCount: string,     // Stored as string for form input, converted to number on submit.
  maxAttempts: string,     // Stored as string for form input, converted to number on submit.
  errorMessage: string | null,
  responseBody: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-rebrickable-sync-queue-add-edit',
  templateUrl: './rebrickable-sync-queue-add-edit.component.html',
  styleUrls: ['./rebrickable-sync-queue-add-edit.component.scss']
})
export class RebrickableSyncQueueAddEditComponent {
  @ViewChild('rebrickableSyncQueueModal') rebrickableSyncQueueModal!: TemplateRef<any>;
  @Output() rebrickableSyncQueueChanged = new Subject<RebrickableSyncQueueData[]>();
  @Input() rebrickableSyncQueueSubmitData: RebrickableSyncQueueSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RebrickableSyncQueueFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public rebrickableSyncQueueForm: FormGroup = this.fb.group({
        operationType: ['', Validators.required],
        entityType: ['', Validators.required],
        entityId: ['', Validators.required],
        payload: [''],
        status: ['', Validators.required],
        createdDate: [''],
        lastAttemptDate: [''],
        completedDate: [''],
        attemptCount: ['', Validators.required],
        maxAttempts: ['', Validators.required],
        errorMessage: [''],
        responseBody: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  rebrickableSyncQueues$ = this.rebrickableSyncQueueService.GetRebrickableSyncQueueList();

  constructor(
    private modalService: NgbModal,
    private rebrickableSyncQueueService: RebrickableSyncQueueService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(rebrickableSyncQueueData?: RebrickableSyncQueueData) {

    if (rebrickableSyncQueueData != null) {

      if (!this.rebrickableSyncQueueService.userIsBMCRebrickableSyncQueueReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Rebrickable Sync Queues`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.rebrickableSyncQueueSubmitData = this.rebrickableSyncQueueService.ConvertToRebrickableSyncQueueSubmitData(rebrickableSyncQueueData);
      this.isEditMode = true;
      this.objectGuid = rebrickableSyncQueueData.objectGuid;

      this.buildFormValues(rebrickableSyncQueueData);

    } else {

      if (!this.rebrickableSyncQueueService.userIsBMCRebrickableSyncQueueWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Rebrickable Sync Queues`,
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
        this.rebrickableSyncQueueForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.rebrickableSyncQueueForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.rebrickableSyncQueueModal, {
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

    if (this.rebrickableSyncQueueService.userIsBMCRebrickableSyncQueueWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Rebrickable Sync Queues`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.rebrickableSyncQueueForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.rebrickableSyncQueueForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.rebrickableSyncQueueForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const rebrickableSyncQueueSubmitData: RebrickableSyncQueueSubmitData = {
        id: this.rebrickableSyncQueueSubmitData?.id || 0,
        operationType: formValue.operationType!.trim(),
        entityType: formValue.entityType!.trim(),
        entityId: Number(formValue.entityId),
        payload: formValue.payload?.trim() || null,
        status: formValue.status!.trim(),
        createdDate: formValue.createdDate ? dateTimeLocalToIsoUtc(formValue.createdDate.trim()) : null,
        lastAttemptDate: formValue.lastAttemptDate ? dateTimeLocalToIsoUtc(formValue.lastAttemptDate.trim()) : null,
        completedDate: formValue.completedDate ? dateTimeLocalToIsoUtc(formValue.completedDate.trim()) : null,
        attemptCount: Number(formValue.attemptCount),
        maxAttempts: Number(formValue.maxAttempts),
        errorMessage: formValue.errorMessage?.trim() || null,
        responseBody: formValue.responseBody?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateRebrickableSyncQueue(rebrickableSyncQueueSubmitData);
      } else {
        this.addRebrickableSyncQueue(rebrickableSyncQueueSubmitData);
      }
  }

  private addRebrickableSyncQueue(rebrickableSyncQueueData: RebrickableSyncQueueSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    rebrickableSyncQueueData.active = true;
    rebrickableSyncQueueData.deleted = false;
    this.rebrickableSyncQueueService.PostRebrickableSyncQueue(rebrickableSyncQueueData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newRebrickableSyncQueue) => {

        this.rebrickableSyncQueueService.ClearAllCaches();

        this.rebrickableSyncQueueChanged.next([newRebrickableSyncQueue]);

        this.alertService.showMessage("Rebrickable Sync Queue added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/rebrickablesyncqueue', newRebrickableSyncQueue.id]);
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
                                   'You do not have permission to save this Rebrickable Sync Queue.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rebrickable Sync Queue.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rebrickable Sync Queue could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateRebrickableSyncQueue(rebrickableSyncQueueData: RebrickableSyncQueueSubmitData) {
    this.rebrickableSyncQueueService.PutRebrickableSyncQueue(rebrickableSyncQueueData.id, rebrickableSyncQueueData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedRebrickableSyncQueue) => {

        this.rebrickableSyncQueueService.ClearAllCaches();

        this.rebrickableSyncQueueChanged.next([updatedRebrickableSyncQueue]);

        this.alertService.showMessage("Rebrickable Sync Queue updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Rebrickable Sync Queue.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rebrickable Sync Queue.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rebrickable Sync Queue could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(rebrickableSyncQueueData: RebrickableSyncQueueData | null) {

    if (rebrickableSyncQueueData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.rebrickableSyncQueueForm.reset({
        operationType: '',
        entityType: '',
        entityId: '',
        payload: '',
        status: '',
        createdDate: '',
        lastAttemptDate: '',
        completedDate: '',
        attemptCount: '',
        maxAttempts: '',
        errorMessage: '',
        responseBody: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.rebrickableSyncQueueForm.reset({
        operationType: rebrickableSyncQueueData.operationType ?? '',
        entityType: rebrickableSyncQueueData.entityType ?? '',
        entityId: rebrickableSyncQueueData.entityId?.toString() ?? '',
        payload: rebrickableSyncQueueData.payload ?? '',
        status: rebrickableSyncQueueData.status ?? '',
        createdDate: isoUtcStringToDateTimeLocal(rebrickableSyncQueueData.createdDate) ?? '',
        lastAttemptDate: isoUtcStringToDateTimeLocal(rebrickableSyncQueueData.lastAttemptDate) ?? '',
        completedDate: isoUtcStringToDateTimeLocal(rebrickableSyncQueueData.completedDate) ?? '',
        attemptCount: rebrickableSyncQueueData.attemptCount?.toString() ?? '',
        maxAttempts: rebrickableSyncQueueData.maxAttempts?.toString() ?? '',
        errorMessage: rebrickableSyncQueueData.errorMessage ?? '',
        responseBody: rebrickableSyncQueueData.responseBody ?? '',
        active: rebrickableSyncQueueData.active ?? true,
        deleted: rebrickableSyncQueueData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.rebrickableSyncQueueForm.markAsPristine();
    this.rebrickableSyncQueueForm.markAsUntouched();
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


  public userIsBMCRebrickableSyncQueueReader(): boolean {
    return this.rebrickableSyncQueueService.userIsBMCRebrickableSyncQueueReader();
  }

  public userIsBMCRebrickableSyncQueueWriter(): boolean {
    return this.rebrickableSyncQueueService.userIsBMCRebrickableSyncQueueWriter();
  }
}
