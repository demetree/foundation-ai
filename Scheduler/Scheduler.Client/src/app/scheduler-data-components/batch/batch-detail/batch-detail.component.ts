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
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BatchService, BatchData, BatchSubmitData } from '../../../scheduler-data-services/batch.service';
import { BatchStatusService } from '../../../scheduler-data-services/batch-status.service';
import { FundService } from '../../../scheduler-data-services/fund.service';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { AppealService } from '../../../scheduler-data-services/appeal.service';
import { BatchChangeHistoryService } from '../../../scheduler-data-services/batch-change-history.service';
import { GiftService } from '../../../scheduler-data-services/gift.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
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
  selector: 'app-batch-detail',
  templateUrl: './batch-detail.component.html',
  styleUrls: ['./batch-detail.component.scss']
})

export class BatchDetailComponent implements OnInit, CanComponentDeactivate {


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


  public batchId: string | null = null;
  public batchData: BatchData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  batches$ = this.batchService.GetBatchList();
  public batchStatuses$ = this.batchStatusService.GetBatchStatusList();
  public funds$ = this.fundService.GetFundList();
  public campaigns$ = this.campaignService.GetCampaignList();
  public appeals$ = this.appealService.GetAppealList();
  public batchChangeHistories$ = this.batchChangeHistoryService.GetBatchChangeHistoryList();
  public gifts$ = this.giftService.GetGiftList();

  private destroy$ = new Subject<void>();

  constructor(
    public batchService: BatchService,
    public batchStatusService: BatchStatusService,
    public fundService: FundService,
    public campaignService: CampaignService,
    public appealService: AppealService,
    public batchChangeHistoryService: BatchChangeHistoryService,
    public giftService: GiftService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the batchId from the route parameters
    this.batchId = this.route.snapshot.paramMap.get('batchId');

    if (this.batchId === 'new' ||
        this.batchId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.batchData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.batchForm.patchValue(this.preSeededData);
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


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Batch';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Batch';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.batchForm.dirty) {
      return confirm('You have unsaved Batch changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.batchId != null && this.batchId !== 'new') {

      const id = parseInt(this.batchId, 10);

      if (!isNaN(id)) {
        return { batchId: id };
      }
    }

    return null;
  }


/*
  * Loads the Batch data for the current batchId.
  *
  * Fully respects the BatchService caching strategy and error handling strategy.
  *
  * @param forceLoadAndDisplaySuccessAlert
  *   - true  will bypass cache entirely and show success alert message
  *   - false/null will use cache if available, no alert message
  */
  public loadData(forceLoadAndDisplaySuccessAlert: boolean | null = null): void {

    //
    // Start loading indicator immediately
    //
    this.isLoadingSubject.next(true);


    //
    // Permission Check
    //
    if (!this.batchService.userIsSchedulerBatchReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Batches.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate batchId
    //
    if (!this.batchId) {

      this.alertService.showMessage('No Batch ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const batchId = Number(this.batchId);

    if (isNaN(batchId) || batchId <= 0) {

      this.alertService.showMessage(`Invalid Batch ID: "${this.batchId}"`,
                                    'Invalid ID',
                                    MessageSeverity.error
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this Batch + relations

      this.batchService.ClearRecordCache(batchId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.batchService.GetBatch(batchId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (batchData) => {

        //
        // Success path — batchData can legitimately be null if 404'd but request succeeded
        //
        if (!batchData) {

          this.handleBatchNotFound(batchId);

        } else {

          this.batchData = batchData;
          this.buildFormValues(this.batchData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Batch loaded successfully',
              '',
              MessageSeverity.success
            );
          }
        }

        this.isLoadingSubject.next(false);
      },

      error: (error: any) => {
        //
        // All HTTP/network/parsing errors flow here
        // The service already stripped sensitive info and re-threw cleanly
        //
        this.handleBatchLoadError(error, batchId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBatchNotFound(batchId: number): void {

    this.batchData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Batch #${batchId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBatchLoadError(error: any, batchId: number): void {

    let message = 'Failed to load Batch.';
    let title = 'Load Error';
    let severity = MessageSeverity.error;

    //
    // Leverage HTTP status if available
    //
    if (error?.status) {
      switch (error.status) {
        case 401:
          message = 'Your session has expired. Please log in again.';
          title = 'Unauthorized';
          break;
        case 403:
          message = 'You do not have permission to view this Batch.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Batch #${batchId} was not found.`;
          title = 'Not Found';
          severity = MessageSeverity.warn;
          break;
        case 500:
          message = 'Server error. Please try again or contact support.';
          title = 'Server Error';
          break;
        case 0:
          message = 'Cannot reach server. Check your internet connection.';
          title = 'Offline';
          break;
        default:
          message = `Server error ${error.status || 'unknown'}: ${error.statusText || 'Request failed'}`;
      }
    } else {
      message = error?.message || message;
    }

    console.error(`Batch load failed (ID: ${batchId})`, error);

    //
    // Reset UI to safe state
    //
    this.batchData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
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

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
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


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.batchService.userIsSchedulerBatchWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Batches", 'Access Denied', MessageSeverity.info);
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
        id: this.batchData?.id || 0,
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
        defaultDate: formValue.defaultDate ? formValue.defaultDate.trim() : null,
        versionNumber: this.batchData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.batchService.PutBatch(batchSubmitData.id, batchSubmitData)
      : this.batchService.PostBatch(batchSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBatchData) => {

        this.batchService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Batch's detail page
          //
          this.batchForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.batchForm.markAsUntouched();

          this.router.navigate(['/batches', savedBatchData.id]);
          this.alertService.showMessage('Batch added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.batchData = savedBatchData;
          this.buildFormValues(this.batchData);

          this.alertService.showMessage("Batch saved successfully", '', MessageSeverity.success);
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

  public userIsSchedulerBatchReader(): boolean {
    return this.batchService.userIsSchedulerBatchReader();
  }

  public userIsSchedulerBatchWriter(): boolean {
    return this.batchService.userIsSchedulerBatchWriter();
  }
}
