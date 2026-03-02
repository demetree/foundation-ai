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
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RebrickableSyncQueueService, RebrickableSyncQueueData, RebrickableSyncQueueSubmitData } from '../../../bmc-data-services/rebrickable-sync-queue.service';
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
  selector: 'app-rebrickable-sync-queue-detail',
  templateUrl: './rebrickable-sync-queue-detail.component.html',
  styleUrls: ['./rebrickable-sync-queue-detail.component.scss']
})

export class RebrickableSyncQueueDetailComponent implements OnInit, CanComponentDeactivate {


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


  public rebrickableSyncQueueId: string | null = null;
  public rebrickableSyncQueueData: RebrickableSyncQueueData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  rebrickableSyncQueues$ = this.rebrickableSyncQueueService.GetRebrickableSyncQueueList();

  private destroy$ = new Subject<void>();

  constructor(
    public rebrickableSyncQueueService: RebrickableSyncQueueService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the rebrickableSyncQueueId from the route parameters
    this.rebrickableSyncQueueId = this.route.snapshot.paramMap.get('rebrickableSyncQueueId');

    if (this.rebrickableSyncQueueId === 'new' ||
        this.rebrickableSyncQueueId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.rebrickableSyncQueueData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.rebrickableSyncQueueForm.patchValue(this.preSeededData);
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


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Rebrickable Sync Queue';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Rebrickable Sync Queue';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.rebrickableSyncQueueForm.dirty) {
      return confirm('You have unsaved Rebrickable Sync Queue changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.rebrickableSyncQueueId != null && this.rebrickableSyncQueueId !== 'new') {

      const id = parseInt(this.rebrickableSyncQueueId, 10);

      if (!isNaN(id)) {
        return { rebrickableSyncQueueId: id };
      }
    }

    return null;
  }


/*
  * Loads the RebrickableSyncQueue data for the current rebrickableSyncQueueId.
  *
  * Fully respects the RebrickableSyncQueueService caching strategy and error handling strategy.
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
    if (!this.rebrickableSyncQueueService.userIsBMCRebrickableSyncQueueReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read RebrickableSyncQueues.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate rebrickableSyncQueueId
    //
    if (!this.rebrickableSyncQueueId) {

      this.alertService.showMessage('No RebrickableSyncQueue ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const rebrickableSyncQueueId = Number(this.rebrickableSyncQueueId);

    if (isNaN(rebrickableSyncQueueId) || rebrickableSyncQueueId <= 0) {

      this.alertService.showMessage(`Invalid Rebrickable Sync Queue ID: "${this.rebrickableSyncQueueId}"`,
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
      // This is the most targeted way: clear only this RebrickableSyncQueue + relations

      this.rebrickableSyncQueueService.ClearRecordCache(rebrickableSyncQueueId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.rebrickableSyncQueueService.GetRebrickableSyncQueue(rebrickableSyncQueueId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (rebrickableSyncQueueData) => {

        //
        // Success path — rebrickableSyncQueueData can legitimately be null if 404'd but request succeeded
        //
        if (!rebrickableSyncQueueData) {

          this.handleRebrickableSyncQueueNotFound(rebrickableSyncQueueId);

        } else {

          this.rebrickableSyncQueueData = rebrickableSyncQueueData;
          this.buildFormValues(this.rebrickableSyncQueueData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'RebrickableSyncQueue loaded successfully',
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
        this.handleRebrickableSyncQueueLoadError(error, rebrickableSyncQueueId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleRebrickableSyncQueueNotFound(rebrickableSyncQueueId: number): void {

    this.rebrickableSyncQueueData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `RebrickableSyncQueue #${rebrickableSyncQueueId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleRebrickableSyncQueueLoadError(error: any, rebrickableSyncQueueId: number): void {

    let message = 'Failed to load Rebrickable Sync Queue.';
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
          message = 'You do not have permission to view this Rebrickable Sync Queue.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Rebrickable Sync Queue #${rebrickableSyncQueueId} was not found.`;
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

    console.error(`Rebrickable Sync Queue load failed (ID: ${rebrickableSyncQueueId})`, error);

    //
    // Reset UI to safe state
    //
    this.rebrickableSyncQueueData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
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

    if (this.rebrickableSyncQueueService.userIsBMCRebrickableSyncQueueWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Rebrickable Sync Queues", 'Access Denied', MessageSeverity.info);
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
        id: this.rebrickableSyncQueueData?.id || 0,
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


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.rebrickableSyncQueueService.PutRebrickableSyncQueue(rebrickableSyncQueueSubmitData.id, rebrickableSyncQueueSubmitData)
      : this.rebrickableSyncQueueService.PostRebrickableSyncQueue(rebrickableSyncQueueSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedRebrickableSyncQueueData) => {

        this.rebrickableSyncQueueService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Rebrickable Sync Queue's detail page
          //
          this.rebrickableSyncQueueForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.rebrickableSyncQueueForm.markAsUntouched();

          this.router.navigate(['/rebrickablesyncqueues', savedRebrickableSyncQueueData.id]);
          this.alertService.showMessage('Rebrickable Sync Queue added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.rebrickableSyncQueueData = savedRebrickableSyncQueueData;
          this.buildFormValues(this.rebrickableSyncQueueData);

          this.alertService.showMessage("Rebrickable Sync Queue saved successfully", '', MessageSeverity.success);
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

  public userIsBMCRebrickableSyncQueueReader(): boolean {
    return this.rebrickableSyncQueueService.userIsBMCRebrickableSyncQueueReader();
  }

  public userIsBMCRebrickableSyncQueueWriter(): boolean {
    return this.rebrickableSyncQueueService.userIsBMCRebrickableSyncQueueWriter();
  }
}
