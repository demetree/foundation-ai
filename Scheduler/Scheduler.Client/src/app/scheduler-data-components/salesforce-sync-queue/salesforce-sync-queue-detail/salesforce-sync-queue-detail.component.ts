/*
   GENERATED FORM FOR THE SALESFORCESYNCQUEUE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SalesforceSyncQueue table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to salesforce-sync-queue-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SalesforceSyncQueueService, SalesforceSyncQueueData, SalesforceSyncQueueSubmitData } from '../../../scheduler-data-services/salesforce-sync-queue.service';
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
interface SalesforceSyncQueueFormValues {
  entityType: string,
  operationType: string,
  entityId: string,     // Stored as string for form input, converted to number on submit.
  payload: string | null,
  status: string,
  attemptCount: string,     // Stored as string for form input, converted to number on submit.
  maxAttempts: string,     // Stored as string for form input, converted to number on submit.
  lastAttemptDate: string | null,
  completedDate: string | null,
  createdDate: string | null,
  errorMessage: string | null,
  responseBody: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-salesforce-sync-queue-detail',
  templateUrl: './salesforce-sync-queue-detail.component.html',
  styleUrls: ['./salesforce-sync-queue-detail.component.scss']
})

export class SalesforceSyncQueueDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SalesforceSyncQueueFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public salesforceSyncQueueForm: FormGroup = this.fb.group({
        entityType: ['', Validators.required],
        operationType: ['', Validators.required],
        entityId: ['', Validators.required],
        payload: [''],
        status: ['', Validators.required],
        attemptCount: ['', Validators.required],
        maxAttempts: ['', Validators.required],
        lastAttemptDate: [''],
        completedDate: [''],
        createdDate: [''],
        errorMessage: [''],
        responseBody: [''],
        active: [true],
        deleted: [false],
      });


  public salesforceSyncQueueId: string | null = null;
  public salesforceSyncQueueData: SalesforceSyncQueueData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  salesforceSyncQueues$ = this.salesforceSyncQueueService.GetSalesforceSyncQueueList();

  private destroy$ = new Subject<void>();

  constructor(
    public salesforceSyncQueueService: SalesforceSyncQueueService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the salesforceSyncQueueId from the route parameters
    this.salesforceSyncQueueId = this.route.snapshot.paramMap.get('salesforceSyncQueueId');

    if (this.salesforceSyncQueueId === 'new' ||
        this.salesforceSyncQueueId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.salesforceSyncQueueData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.salesforceSyncQueueForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.salesforceSyncQueueForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Salesforce Sync Queue';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Salesforce Sync Queue';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.salesforceSyncQueueForm.dirty) {
      return confirm('You have unsaved Salesforce Sync Queue changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.salesforceSyncQueueId != null && this.salesforceSyncQueueId !== 'new') {

      const id = parseInt(this.salesforceSyncQueueId, 10);

      if (!isNaN(id)) {
        return { salesforceSyncQueueId: id };
      }
    }

    return null;
  }


/*
  * Loads the SalesforceSyncQueue data for the current salesforceSyncQueueId.
  *
  * Fully respects the SalesforceSyncQueueService caching strategy and error handling strategy.
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
    if (!this.salesforceSyncQueueService.userIsSchedulerSalesforceSyncQueueReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SalesforceSyncQueues.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate salesforceSyncQueueId
    //
    if (!this.salesforceSyncQueueId) {

      this.alertService.showMessage('No SalesforceSyncQueue ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const salesforceSyncQueueId = Number(this.salesforceSyncQueueId);

    if (isNaN(salesforceSyncQueueId) || salesforceSyncQueueId <= 0) {

      this.alertService.showMessage(`Invalid Salesforce Sync Queue ID: "${this.salesforceSyncQueueId}"`,
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
      // This is the most targeted way: clear only this SalesforceSyncQueue + relations

      this.salesforceSyncQueueService.ClearRecordCache(salesforceSyncQueueId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.salesforceSyncQueueService.GetSalesforceSyncQueue(salesforceSyncQueueId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (salesforceSyncQueueData) => {

        //
        // Success path — salesforceSyncQueueData can legitimately be null if 404'd but request succeeded
        //
        if (!salesforceSyncQueueData) {

          this.handleSalesforceSyncQueueNotFound(salesforceSyncQueueId);

        } else {

          this.salesforceSyncQueueData = salesforceSyncQueueData;
          this.buildFormValues(this.salesforceSyncQueueData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SalesforceSyncQueue loaded successfully',
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
        this.handleSalesforceSyncQueueLoadError(error, salesforceSyncQueueId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSalesforceSyncQueueNotFound(salesforceSyncQueueId: number): void {

    this.salesforceSyncQueueData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SalesforceSyncQueue #${salesforceSyncQueueId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSalesforceSyncQueueLoadError(error: any, salesforceSyncQueueId: number): void {

    let message = 'Failed to load Salesforce Sync Queue.';
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
          message = 'You do not have permission to view this Salesforce Sync Queue.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Salesforce Sync Queue #${salesforceSyncQueueId} was not found.`;
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

    console.error(`Salesforce Sync Queue load failed (ID: ${salesforceSyncQueueId})`, error);

    //
    // Reset UI to safe state
    //
    this.salesforceSyncQueueData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(salesforceSyncQueueData: SalesforceSyncQueueData | null) {

    if (salesforceSyncQueueData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.salesforceSyncQueueForm.reset({
        entityType: '',
        operationType: '',
        entityId: '',
        payload: '',
        status: '',
        attemptCount: '',
        maxAttempts: '',
        lastAttemptDate: '',
        completedDate: '',
        createdDate: '',
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
        this.salesforceSyncQueueForm.reset({
        entityType: salesforceSyncQueueData.entityType ?? '',
        operationType: salesforceSyncQueueData.operationType ?? '',
        entityId: salesforceSyncQueueData.entityId?.toString() ?? '',
        payload: salesforceSyncQueueData.payload ?? '',
        status: salesforceSyncQueueData.status ?? '',
        attemptCount: salesforceSyncQueueData.attemptCount?.toString() ?? '',
        maxAttempts: salesforceSyncQueueData.maxAttempts?.toString() ?? '',
        lastAttemptDate: isoUtcStringToDateTimeLocal(salesforceSyncQueueData.lastAttemptDate) ?? '',
        completedDate: isoUtcStringToDateTimeLocal(salesforceSyncQueueData.completedDate) ?? '',
        createdDate: isoUtcStringToDateTimeLocal(salesforceSyncQueueData.createdDate) ?? '',
        errorMessage: salesforceSyncQueueData.errorMessage ?? '',
        responseBody: salesforceSyncQueueData.responseBody ?? '',
        active: salesforceSyncQueueData.active ?? true,
        deleted: salesforceSyncQueueData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.salesforceSyncQueueForm.markAsPristine();
    this.salesforceSyncQueueForm.markAsUntouched();
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

    if (this.salesforceSyncQueueService.userIsSchedulerSalesforceSyncQueueWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Salesforce Sync Queues", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.salesforceSyncQueueForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.salesforceSyncQueueForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.salesforceSyncQueueForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const salesforceSyncQueueSubmitData: SalesforceSyncQueueSubmitData = {
        id: this.salesforceSyncQueueData?.id || 0,
        entityType: formValue.entityType!.trim(),
        operationType: formValue.operationType!.trim(),
        entityId: Number(formValue.entityId),
        payload: formValue.payload?.trim() || null,
        status: formValue.status!.trim(),
        attemptCount: Number(formValue.attemptCount),
        maxAttempts: Number(formValue.maxAttempts),
        lastAttemptDate: formValue.lastAttemptDate ? dateTimeLocalToIsoUtc(formValue.lastAttemptDate.trim()) : null,
        completedDate: formValue.completedDate ? dateTimeLocalToIsoUtc(formValue.completedDate.trim()) : null,
        createdDate: formValue.createdDate ? dateTimeLocalToIsoUtc(formValue.createdDate.trim()) : null,
        errorMessage: formValue.errorMessage?.trim() || null,
        responseBody: formValue.responseBody?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.salesforceSyncQueueService.PutSalesforceSyncQueue(salesforceSyncQueueSubmitData.id, salesforceSyncQueueSubmitData)
      : this.salesforceSyncQueueService.PostSalesforceSyncQueue(salesforceSyncQueueSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSalesforceSyncQueueData) => {

        this.salesforceSyncQueueService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Salesforce Sync Queue's detail page
          //
          this.salesforceSyncQueueForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.salesforceSyncQueueForm.markAsUntouched();

          this.router.navigate(['/salesforcesyncqueues', savedSalesforceSyncQueueData.id]);
          this.alertService.showMessage('Salesforce Sync Queue added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.salesforceSyncQueueData = savedSalesforceSyncQueueData;
          this.buildFormValues(this.salesforceSyncQueueData);

          this.alertService.showMessage("Salesforce Sync Queue saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Salesforce Sync Queue.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Salesforce Sync Queue.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Salesforce Sync Queue could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerSalesforceSyncQueueReader(): boolean {
    return this.salesforceSyncQueueService.userIsSchedulerSalesforceSyncQueueReader();
  }

  public userIsSchedulerSalesforceSyncQueueWriter(): boolean {
    return this.salesforceSyncQueueService.userIsSchedulerSalesforceSyncQueueWriter();
  }
}
