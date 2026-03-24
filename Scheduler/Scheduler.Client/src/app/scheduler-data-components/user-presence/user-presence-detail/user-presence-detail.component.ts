/*
   GENERATED FORM FOR THE USERPRESENCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserPresence table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-presence-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserPresenceService, UserPresenceData, UserPresenceSubmitData } from '../../../scheduler-data-services/user-presence.service';
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
interface UserPresenceFormValues {
  userId: string,     // Stored as string for form input, converted to number on submit.
  status: string,
  customStatusMessage: string | null,
  lastSeenDateTime: string,
  lastActivityDateTime: string,
  connectionCount: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-presence-detail',
  templateUrl: './user-presence-detail.component.html',
  styleUrls: ['./user-presence-detail.component.scss']
})

export class UserPresenceDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserPresenceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userPresenceForm: FormGroup = this.fb.group({
        userId: ['', Validators.required],
        status: ['', Validators.required],
        customStatusMessage: [''],
        lastSeenDateTime: ['', Validators.required],
        lastActivityDateTime: ['', Validators.required],
        connectionCount: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public userPresenceId: string | null = null;
  public userPresenceData: UserPresenceData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userPresences$ = this.userPresenceService.GetUserPresenceList();

  private destroy$ = new Subject<void>();

  constructor(
    public userPresenceService: UserPresenceService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userPresenceId from the route parameters
    this.userPresenceId = this.route.snapshot.paramMap.get('userPresenceId');

    if (this.userPresenceId === 'new' ||
        this.userPresenceId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userPresenceData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userPresenceForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userPresenceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Presence';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Presence';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userPresenceForm.dirty) {
      return confirm('You have unsaved User Presence changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userPresenceId != null && this.userPresenceId !== 'new') {

      const id = parseInt(this.userPresenceId, 10);

      if (!isNaN(id)) {
        return { userPresenceId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserPresence data for the current userPresenceId.
  *
  * Fully respects the UserPresenceService caching strategy and error handling strategy.
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
    if (!this.userPresenceService.userIsSchedulerUserPresenceReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserPresences.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userPresenceId
    //
    if (!this.userPresenceId) {

      this.alertService.showMessage('No UserPresence ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userPresenceId = Number(this.userPresenceId);

    if (isNaN(userPresenceId) || userPresenceId <= 0) {

      this.alertService.showMessage(`Invalid User Presence ID: "${this.userPresenceId}"`,
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
      // This is the most targeted way: clear only this UserPresence + relations

      this.userPresenceService.ClearRecordCache(userPresenceId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userPresenceService.GetUserPresence(userPresenceId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userPresenceData) => {

        //
        // Success path — userPresenceData can legitimately be null if 404'd but request succeeded
        //
        if (!userPresenceData) {

          this.handleUserPresenceNotFound(userPresenceId);

        } else {

          this.userPresenceData = userPresenceData;
          this.buildFormValues(this.userPresenceData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserPresence loaded successfully',
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
        this.handleUserPresenceLoadError(error, userPresenceId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserPresenceNotFound(userPresenceId: number): void {

    this.userPresenceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserPresence #${userPresenceId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserPresenceLoadError(error: any, userPresenceId: number): void {

    let message = 'Failed to load User Presence.';
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
          message = 'You do not have permission to view this User Presence.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Presence #${userPresenceId} was not found.`;
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

    console.error(`User Presence load failed (ID: ${userPresenceId})`, error);

    //
    // Reset UI to safe state
    //
    this.userPresenceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userPresenceData: UserPresenceData | null) {

    if (userPresenceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userPresenceForm.reset({
        userId: '',
        status: '',
        customStatusMessage: '',
        lastSeenDateTime: '',
        lastActivityDateTime: '',
        connectionCount: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userPresenceForm.reset({
        userId: userPresenceData.userId?.toString() ?? '',
        status: userPresenceData.status ?? '',
        customStatusMessage: userPresenceData.customStatusMessage ?? '',
        lastSeenDateTime: isoUtcStringToDateTimeLocal(userPresenceData.lastSeenDateTime) ?? '',
        lastActivityDateTime: isoUtcStringToDateTimeLocal(userPresenceData.lastActivityDateTime) ?? '',
        connectionCount: userPresenceData.connectionCount?.toString() ?? '',
        active: userPresenceData.active ?? true,
        deleted: userPresenceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userPresenceForm.markAsPristine();
    this.userPresenceForm.markAsUntouched();
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

    if (this.userPresenceService.userIsSchedulerUserPresenceWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Presences", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userPresenceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userPresenceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userPresenceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userPresenceSubmitData: UserPresenceSubmitData = {
        id: this.userPresenceData?.id || 0,
        userId: Number(formValue.userId),
        status: formValue.status!.trim(),
        customStatusMessage: formValue.customStatusMessage?.trim() || null,
        lastSeenDateTime: dateTimeLocalToIsoUtc(formValue.lastSeenDateTime!.trim())!,
        lastActivityDateTime: dateTimeLocalToIsoUtc(formValue.lastActivityDateTime!.trim())!,
        connectionCount: Number(formValue.connectionCount),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userPresenceService.PutUserPresence(userPresenceSubmitData.id, userPresenceSubmitData)
      : this.userPresenceService.PostUserPresence(userPresenceSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserPresenceData) => {

        this.userPresenceService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Presence's detail page
          //
          this.userPresenceForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userPresenceForm.markAsUntouched();

          this.router.navigate(['/userpresences', savedUserPresenceData.id]);
          this.alertService.showMessage('User Presence added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userPresenceData = savedUserPresenceData;
          this.buildFormValues(this.userPresenceData);

          this.alertService.showMessage("User Presence saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Presence.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Presence.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Presence could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerUserPresenceReader(): boolean {
    return this.userPresenceService.userIsSchedulerUserPresenceReader();
  }

  public userIsSchedulerUserPresenceWriter(): boolean {
    return this.userPresenceService.userIsSchedulerUserPresenceWriter();
  }
}
