/*
   GENERATED FORM FOR THE CALLSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CallStatus table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to call-status-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CallStatusService, CallStatusData, CallStatusSubmitData } from '../../../scheduler-data-services/call-status.service';
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
interface CallStatusFormValues {
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-call-status-detail',
  templateUrl: './call-status-detail.component.html',
  styleUrls: ['./call-status-detail.component.scss']
})

export class CallStatusDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CallStatusFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public callStatusForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public callStatusId: string | null = null;
  public callStatusData: CallStatusData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  callStatuses$ = this.callStatusService.GetCallStatusList();
  public calls$ = this.callService.GetCallList();

  private destroy$ = new Subject<void>();

  constructor(
    public callStatusService: CallStatusService,
    public callService: CallService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the callStatusId from the route parameters
    this.callStatusId = this.route.snapshot.paramMap.get('callStatusId');

    if (this.callStatusId === 'new' ||
        this.callStatusId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.callStatusData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.callStatusForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.callStatusForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Call Status';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Call Status';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.callStatusForm.dirty) {
      return confirm('You have unsaved Call Status changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.callStatusId != null && this.callStatusId !== 'new') {

      const id = parseInt(this.callStatusId, 10);

      if (!isNaN(id)) {
        return { callStatusId: id };
      }
    }

    return null;
  }


/*
  * Loads the CallStatus data for the current callStatusId.
  *
  * Fully respects the CallStatusService caching strategy and error handling strategy.
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
    if (!this.callStatusService.userIsSchedulerCallStatusReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read CallStatuses.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate callStatusId
    //
    if (!this.callStatusId) {

      this.alertService.showMessage('No CallStatus ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const callStatusId = Number(this.callStatusId);

    if (isNaN(callStatusId) || callStatusId <= 0) {

      this.alertService.showMessage(`Invalid Call Status ID: "${this.callStatusId}"`,
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
      // This is the most targeted way: clear only this CallStatus + relations

      this.callStatusService.ClearRecordCache(callStatusId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.callStatusService.GetCallStatus(callStatusId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (callStatusData) => {

        //
        // Success path — callStatusData can legitimately be null if 404'd but request succeeded
        //
        if (!callStatusData) {

          this.handleCallStatusNotFound(callStatusId);

        } else {

          this.callStatusData = callStatusData;
          this.buildFormValues(this.callStatusData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'CallStatus loaded successfully',
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
        this.handleCallStatusLoadError(error, callStatusId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleCallStatusNotFound(callStatusId: number): void {

    this.callStatusData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `CallStatus #${callStatusId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleCallStatusLoadError(error: any, callStatusId: number): void {

    let message = 'Failed to load Call Status.';
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
          message = 'You do not have permission to view this Call Status.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Call Status #${callStatusId} was not found.`;
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

    console.error(`Call Status load failed (ID: ${callStatusId})`, error);

    //
    // Reset UI to safe state
    //
    this.callStatusData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(callStatusData: CallStatusData | null) {

    if (callStatusData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.callStatusForm.reset({
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.callStatusForm.reset({
        name: callStatusData.name ?? '',
        description: callStatusData.description ?? '',
        active: callStatusData.active ?? true,
        deleted: callStatusData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.callStatusForm.markAsPristine();
    this.callStatusForm.markAsUntouched();
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

    if (this.callStatusService.userIsSchedulerCallStatusWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Call Statuses", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.callStatusForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.callStatusForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.callStatusForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const callStatusSubmitData: CallStatusSubmitData = {
        id: this.callStatusData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.callStatusService.PutCallStatus(callStatusSubmitData.id, callStatusSubmitData)
      : this.callStatusService.PostCallStatus(callStatusSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedCallStatusData) => {

        this.callStatusService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Call Status's detail page
          //
          this.callStatusForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.callStatusForm.markAsUntouched();

          this.router.navigate(['/callstatuses', savedCallStatusData.id]);
          this.alertService.showMessage('Call Status added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.callStatusData = savedCallStatusData;
          this.buildFormValues(this.callStatusData);

          this.alertService.showMessage("Call Status saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Call Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerCallStatusReader(): boolean {
    return this.callStatusService.userIsSchedulerCallStatusReader();
  }

  public userIsSchedulerCallStatusWriter(): boolean {
    return this.callStatusService.userIsSchedulerCallStatusWriter();
  }
}
