/*
   GENERATED FORM FOR THE CALLPARTICIPANT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CallParticipant table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to call-participant-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CallParticipantService, CallParticipantData, CallParticipantSubmitData } from '../../../scheduler-data-services/call-participant.service';
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
interface CallParticipantFormValues {
  callId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  role: string,
  status: string,
  joinedDateTime: string | null,
  leftDateTime: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-call-participant-detail',
  templateUrl: './call-participant-detail.component.html',
  styleUrls: ['./call-participant-detail.component.scss']
})

export class CallParticipantDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CallParticipantFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public callParticipantForm: FormGroup = this.fb.group({
        callId: [null, Validators.required],
        userId: ['', Validators.required],
        role: ['', Validators.required],
        status: ['', Validators.required],
        joinedDateTime: [''],
        leftDateTime: [''],
        active: [true],
        deleted: [false],
      });


  public callParticipantId: string | null = null;
  public callParticipantData: CallParticipantData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  callParticipants$ = this.callParticipantService.GetCallParticipantList();
  public calls$ = this.callService.GetCallList();

  private destroy$ = new Subject<void>();

  constructor(
    public callParticipantService: CallParticipantService,
    public callService: CallService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the callParticipantId from the route parameters
    this.callParticipantId = this.route.snapshot.paramMap.get('callParticipantId');

    if (this.callParticipantId === 'new' ||
        this.callParticipantId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.callParticipantData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.callParticipantForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.callParticipantForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Call Participant';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Call Participant';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.callParticipantForm.dirty) {
      return confirm('You have unsaved Call Participant changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.callParticipantId != null && this.callParticipantId !== 'new') {

      const id = parseInt(this.callParticipantId, 10);

      if (!isNaN(id)) {
        return { callParticipantId: id };
      }
    }

    return null;
  }


/*
  * Loads the CallParticipant data for the current callParticipantId.
  *
  * Fully respects the CallParticipantService caching strategy and error handling strategy.
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
    if (!this.callParticipantService.userIsSchedulerCallParticipantReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read CallParticipants.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate callParticipantId
    //
    if (!this.callParticipantId) {

      this.alertService.showMessage('No CallParticipant ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const callParticipantId = Number(this.callParticipantId);

    if (isNaN(callParticipantId) || callParticipantId <= 0) {

      this.alertService.showMessage(`Invalid Call Participant ID: "${this.callParticipantId}"`,
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
      // This is the most targeted way: clear only this CallParticipant + relations

      this.callParticipantService.ClearRecordCache(callParticipantId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.callParticipantService.GetCallParticipant(callParticipantId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (callParticipantData) => {

        //
        // Success path — callParticipantData can legitimately be null if 404'd but request succeeded
        //
        if (!callParticipantData) {

          this.handleCallParticipantNotFound(callParticipantId);

        } else {

          this.callParticipantData = callParticipantData;
          this.buildFormValues(this.callParticipantData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'CallParticipant loaded successfully',
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
        this.handleCallParticipantLoadError(error, callParticipantId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleCallParticipantNotFound(callParticipantId: number): void {

    this.callParticipantData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `CallParticipant #${callParticipantId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleCallParticipantLoadError(error: any, callParticipantId: number): void {

    let message = 'Failed to load Call Participant.';
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
          message = 'You do not have permission to view this Call Participant.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Call Participant #${callParticipantId} was not found.`;
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

    console.error(`Call Participant load failed (ID: ${callParticipantId})`, error);

    //
    // Reset UI to safe state
    //
    this.callParticipantData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(callParticipantData: CallParticipantData | null) {

    if (callParticipantData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.callParticipantForm.reset({
        callId: null,
        userId: '',
        role: '',
        status: '',
        joinedDateTime: '',
        leftDateTime: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.callParticipantForm.reset({
        callId: callParticipantData.callId,
        userId: callParticipantData.userId?.toString() ?? '',
        role: callParticipantData.role ?? '',
        status: callParticipantData.status ?? '',
        joinedDateTime: isoUtcStringToDateTimeLocal(callParticipantData.joinedDateTime) ?? '',
        leftDateTime: isoUtcStringToDateTimeLocal(callParticipantData.leftDateTime) ?? '',
        active: callParticipantData.active ?? true,
        deleted: callParticipantData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.callParticipantForm.markAsPristine();
    this.callParticipantForm.markAsUntouched();
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

    if (this.callParticipantService.userIsSchedulerCallParticipantWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Call Participants", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.callParticipantForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.callParticipantForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.callParticipantForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const callParticipantSubmitData: CallParticipantSubmitData = {
        id: this.callParticipantData?.id || 0,
        callId: Number(formValue.callId),
        userId: Number(formValue.userId),
        role: formValue.role!.trim(),
        status: formValue.status!.trim(),
        joinedDateTime: formValue.joinedDateTime ? dateTimeLocalToIsoUtc(formValue.joinedDateTime.trim()) : null,
        leftDateTime: formValue.leftDateTime ? dateTimeLocalToIsoUtc(formValue.leftDateTime.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.callParticipantService.PutCallParticipant(callParticipantSubmitData.id, callParticipantSubmitData)
      : this.callParticipantService.PostCallParticipant(callParticipantSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedCallParticipantData) => {

        this.callParticipantService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Call Participant's detail page
          //
          this.callParticipantForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.callParticipantForm.markAsUntouched();

          this.router.navigate(['/callparticipants', savedCallParticipantData.id]);
          this.alertService.showMessage('Call Participant added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.callParticipantData = savedCallParticipantData;
          this.buildFormValues(this.callParticipantData);

          this.alertService.showMessage("Call Participant saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Call Participant.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Call Participant.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Call Participant could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerCallParticipantReader(): boolean {
    return this.callParticipantService.userIsSchedulerCallParticipantReader();
  }

  public userIsSchedulerCallParticipantWriter(): boolean {
    return this.callParticipantService.userIsSchedulerCallParticipantWriter();
  }
}
