/*
   GENERATED FORM FOR THE BATCH TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Batch table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to batch-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BatchService, BatchData, BatchSubmitData } from '../../../scheduler-data-services/batch.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BatchStatusService } from '../../../scheduler-data-services/batch-status.service';
import { FundService } from '../../../scheduler-data-services/fund.service';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { AppealService } from '../../../scheduler-data-services/appeal.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BatchFormValues {
  batchNumber: string,
  description: string | null,
  dateOpened: string,
  datePosted: string | null,
  batchStatusId: number | bigint,       // For FK link number
  controlAmount: string,     // Stored as string for form input, converted to number on submit.
  controlCount: string,     // Stored as string for form input, converted to number on submit.
  defaultFundId: number | bigint | null,       // For FK link number
  defaultCampaignId: number | bigint | null,       // For FK link number
  defaultAppealId: number | bigint | null,       // For FK link number
  defaultDate: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-batch-add-edit',
  templateUrl: './batch-add-edit.component.html',
  styleUrls: ['./batch-add-edit.component.scss']
})
export class BatchAddEditComponent {
  @ViewChild('batchModal') batchModal!: TemplateRef<any>;
  @Output() batchChanged = new Subject<BatchData[]>();
  @Input() batchSubmitData: BatchSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BatchFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public batchForm: FormGroup = this.fb.group({
        batchNumber: ['', Validators.required],
        description: [''],
        dateOpened: ['', Validators.required],
        datePosted: [''],
        batchStatusId: [null, Validators.required],
        controlAmount: ['', Validators.required],
        controlCount: ['', Validators.required],
        defaultFundId: [null],
        defaultCampaignId: [null],
        defaultAppealId: [null],
        defaultDate: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  batches$ = this.batchService.GetBatchList();
  batchStatuses$ = this.batchStatusService.GetBatchStatusList();
  funds$ = this.fundService.GetFundList();
  campaigns$ = this.campaignService.GetCampaignList();
  appeals$ = this.appealService.GetAppealList();

  constructor(
    private modalService: NgbModal,
    private batchService: BatchService,
    private batchStatusService: BatchStatusService,
    private fundService: FundService,
    private campaignService: CampaignService,
    private appealService: AppealService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(batchData?: BatchData) {

    if (batchData != null) {

      if (!this.batchService.userIsSchedulerBatchReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Batches`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.batchSubmitData = this.batchService.ConvertToBatchSubmitData(batchData);
      this.isEditMode = true;
      this.objectGuid = batchData.objectGuid;

      this.buildFormValues(batchData);

    } else {

      if (!this.batchService.userIsSchedulerBatchWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Batches`,
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
        this.batchForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.batchForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.batchModal, {
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

    if (this.batchService.userIsSchedulerBatchWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Batches`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.batchForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.batchForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.batchForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const batchSubmitData: BatchSubmitData = {
        id: this.batchSubmitData?.id || 0,
        batchNumber: formValue.batchNumber!.trim(),
        description: formValue.description?.trim() || null,
        dateOpened: dateTimeLocalToIsoUtc(formValue.dateOpened!.trim())!,
        datePosted: formValue.datePosted ? dateTimeLocalToIsoUtc(formValue.datePosted.trim()) : null,
        batchStatusId: Number(formValue.batchStatusId),
        controlAmount: Number(formValue.controlAmount),
        controlCount: Number(formValue.controlCount),
        defaultFundId: formValue.defaultFundId ? Number(formValue.defaultFundId) : null,
        defaultCampaignId: formValue.defaultCampaignId ? Number(formValue.defaultCampaignId) : null,
        defaultAppealId: formValue.defaultAppealId ? Number(formValue.defaultAppealId) : null,
        defaultDate: formValue.defaultDate?.trim() || null,
        versionNumber: this.batchSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBatch(batchSubmitData);
      } else {
        this.addBatch(batchSubmitData);
      }
  }

  private addBatch(batchData: BatchSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    batchData.versionNumber = 0;
    batchData.active = true;
    batchData.deleted = false;
    this.batchService.PostBatch(batchData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBatch) => {

        this.batchService.ClearAllCaches();

        this.batchChanged.next([newBatch]);

        this.alertService.showMessage("Batch added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/batch', newBatch.id]);
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
                                   'You do not have permission to save this Batch.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Batch.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Batch could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBatch(batchData: BatchSubmitData) {
    this.batchService.PutBatch(batchData.id, batchData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBatch) => {

        this.batchService.ClearAllCaches();

        this.batchChanged.next([updatedBatch]);

        this.alertService.showMessage("Batch updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Batch.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Batch.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Batch could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(batchData: BatchData | null) {

    if (batchData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.batchForm.reset({
        batchNumber: '',
        description: '',
        dateOpened: '',
        datePosted: '',
        batchStatusId: null,
        controlAmount: '',
        controlCount: '',
        defaultFundId: null,
        defaultCampaignId: null,
        defaultAppealId: null,
        defaultDate: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.batchForm.reset({
        batchNumber: batchData.batchNumber ?? '',
        description: batchData.description ?? '',
        dateOpened: isoUtcStringToDateTimeLocal(batchData.dateOpened) ?? '',
        datePosted: isoUtcStringToDateTimeLocal(batchData.datePosted) ?? '',
        batchStatusId: batchData.batchStatusId,
        controlAmount: batchData.controlAmount?.toString() ?? '',
        controlCount: batchData.controlCount?.toString() ?? '',
        defaultFundId: batchData.defaultFundId,
        defaultCampaignId: batchData.defaultCampaignId,
        defaultAppealId: batchData.defaultAppealId,
        defaultDate: batchData.defaultDate ?? '',
        versionNumber: batchData.versionNumber?.toString() ?? '',
        active: batchData.active ?? true,
        deleted: batchData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.batchForm.markAsPristine();
    this.batchForm.markAsUntouched();
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


  public userIsSchedulerBatchReader(): boolean {
    return this.batchService.userIsSchedulerBatchReader();
  }

  public userIsSchedulerBatchWriter(): boolean {
    return this.batchService.userIsSchedulerBatchWriter();
  }
}
