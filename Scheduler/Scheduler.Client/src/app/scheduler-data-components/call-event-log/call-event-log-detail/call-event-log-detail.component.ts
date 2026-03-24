/*
   GENERATED FORM FOR THE CALLEVENTLOG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CallEventLog table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to call-event-log-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CallEventLogService, CallEventLogData, CallEventLogSubmitData } from '../../../scheduler-data-services/call-event-log.service';
import { CallService } from '../../../scheduler-data-services/call.service';
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
interface CallEventLogFormValues {
  callId: number | bigint,       // For FK link number
  eventType: string,
  userId: string | null,     // Stored as string for form input, converted to number on submit.
  providerId: string | null,
  metadata: string | null,
  dateTimeCreated: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-call-event-log-detail',
  templateUrl: './call-event-log-detail.component.html',
  styleUrls: ['./call-event-log-detail.component.scss']
})

export class CallEventLogDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CallEventLogFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public callEventLogForm: FormGroup = this.fb.group({
        callId: [null, Validators.required],
        eventType: ['', Validators.required],
        userId: [''],
        providerId: [''],
        metadata: [''],
        dateTimeCreated: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public callEventLogId: string | null = null;
  public callEventLogData: CallEventLogData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  callEventLogs$ = this.callEventLogService.GetCallEventLogList();
  public calls$ = this.callService.GetCallList();

  private destroy$ = new Subject<void>();

  constructor(
    public callEventLogService: CallEventLogService,
    public callService: CallService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the callEventLogId from the route parameters
    this.callEventLogId = this.route.snapshot.paramMap.get('callEventLogId');

    if (this.callEventLogId === 'new' ||
        this.callEventLogId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.callEventLogData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.callEventLogForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.callEventLogForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Call Event Log';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Call Event Log';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.callEventLogForm.dirty) {
      return confirm('You have unsaved Call Event Log changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.callEventLogId != null && this.callEventLogId !== 'new') {

      const id = parseInt(this.callEventLogId, 10);

      if (!isNaN(id)) {
        return { callEventLogId: id };
      }
    }

    return null;
  }


/*
  * Loads the CallEventLog data for the current callEventLogId.
  *
  * Fully respects the CallEventLogService caching strategy and error handling strategy.
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
    if (!this.callEventLogService.userIsSchedulerCallEventLogReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read CallEventLogs.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate callEventLogId
    //
    if (!this.callEventLogId) {

      this.alertService.showMessage('No CallEventLog ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const callEventLogId = Number(this.callEventLogId);

    if (isNaN(callEventLogId) || callEventLogId <= 0) {

      this.alertService.showMessage(`Invalid Call Event Log ID: "${this.callEventLogId}"`,
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
      // This is the most targeted way: clear only this CallEventLog + relations

      this.callEventLogService.ClearRecordCache(callEventLogId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.callEventLogService.GetCallEventLog(callEventLogId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (callEventLogData) => {

        //
        // Success path — callEventLogData can legitimately be null if 404'd but request succeeded
        //
        if (!callEventLogData) {

          this.handleCallEventLogNotFound(callEventLogId);

        } else {

          this.callEventLogData = callEventLogData;
          this.buildFormValues(this.callEventLogData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'CallEventLog loaded successfully',
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
        this.handleCallEventLogLoadError(error, callEventLogId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleCallEventLogNotFound(callEventLogId: number): void {

    this.callEventLogData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `CallEventLog #${callEventLogId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleCallEventLogLoadError(error: any, callEventLogId: number): void {

    let message = 'Failed to load Call Event Log.';
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
          message = 'You do not have permission to view this Call Event Log.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Call Event Log #${callEventLogId} was not found.`;
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

    console.error(`Call Event Log load failed (ID: ${callEventLogId})`, error);

    //
    // Reset UI to safe state
    //
    this.callEventLogData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(callEventLogData: CallEventLogData | null) {

    if (callEventLogData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.callEventLogForm.reset({
        callId: null,
        eventType: '',
        userId: '',
        providerId: '',
        metadata: '',
        dateTimeCreated: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.callEventLogForm.reset({
        callId: callEventLogData.callId,
        eventType: callEventLogData.eventType ?? '',
        userId: callEventLogData.userId?.toString() ?? '',
        providerId: callEventLogData.providerId ?? '',
        metadata: callEventLogData.metadata ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(callEventLogData.dateTimeCreated) ?? '',
        active: callEventLogData.active ?? true,
        deleted: callEventLogData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.callEventLogForm.markAsPristine();
    this.callEventLogForm.markAsUntouched();
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

    if (this.callEventLogService.userIsSchedulerCallEventLogWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Call Event Logs", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.callEventLogForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.callEventLogForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.callEventLogForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const callEventLogSubmitData: CallEventLogSubmitData = {
        id: this.callEventLogData?.id || 0,
        callId: Number(formValue.callId),
        eventType: formValue.eventType!.trim(),
        userId: formValue.userId ? Number(formValue.userId) : null,
        providerId: formValue.providerId?.trim() || null,
        metadata: formValue.metadata?.trim() || null,
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.callEventLogService.PutCallEventLog(callEventLogSubmitData.id, callEventLogSubmitData)
      : this.callEventLogService.PostCallEventLog(callEventLogSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedCallEventLogData) => {

        this.callEventLogService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Call Event Log's detail page
          //
          this.callEventLogForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.callEventLogForm.markAsUntouched();

          this.router.navigate(['/calleventlogs', savedCallEventLogData.id]);
          this.alertService.showMessage('Call Event Log added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.callEventLogData = savedCallEventLogData;
          this.buildFormValues(this.callEventLogData);

          this.alertService.showMessage("Call Event Log saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Call Event Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call Event Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call Event Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerCallEventLogReader(): boolean {
    return this.callEventLogService.userIsSchedulerCallEventLogReader();
  }

  public userIsSchedulerCallEventLogWriter(): boolean {
    return this.callEventLogService.userIsSchedulerCallEventLogWriter();
  }
}
