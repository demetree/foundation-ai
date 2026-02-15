/*
   GENERATED FORM FOR THE ACTIVITYEVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ActivityEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to activity-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ActivityEventService, ActivityEventData, ActivityEventSubmitData } from '../../../bmc-data-services/activity-event.service';
import { ActivityEventTypeService } from '../../../bmc-data-services/activity-event-type.service';
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
interface ActivityEventFormValues {
  activityEventTypeId: number | bigint,       // For FK link number
  title: string,
  description: string | null,
  relatedEntityType: string | null,
  relatedEntityId: string | null,     // Stored as string for form input, converted to number on submit.
  eventDate: string,
  isPublic: boolean,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-activity-event-detail',
  templateUrl: './activity-event-detail.component.html',
  styleUrls: ['./activity-event-detail.component.scss']
})

export class ActivityEventDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ActivityEventFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public activityEventForm: FormGroup = this.fb.group({
        activityEventTypeId: [null, Validators.required],
        title: ['', Validators.required],
        description: [''],
        relatedEntityType: [''],
        relatedEntityId: [''],
        eventDate: ['', Validators.required],
        isPublic: [false],
        active: [true],
        deleted: [false],
      });


  public activityEventId: string | null = null;
  public activityEventData: ActivityEventData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  activityEvents$ = this.activityEventService.GetActivityEventList();
  public activityEventTypes$ = this.activityEventTypeService.GetActivityEventTypeList();

  private destroy$ = new Subject<void>();

  constructor(
    public activityEventService: ActivityEventService,
    public activityEventTypeService: ActivityEventTypeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the activityEventId from the route parameters
    this.activityEventId = this.route.snapshot.paramMap.get('activityEventId');

    if (this.activityEventId === 'new' ||
        this.activityEventId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.activityEventData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.activityEventForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.activityEventForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Activity Event';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Activity Event';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.activityEventForm.dirty) {
      return confirm('You have unsaved Activity Event changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.activityEventId != null && this.activityEventId !== 'new') {

      const id = parseInt(this.activityEventId, 10);

      if (!isNaN(id)) {
        return { activityEventId: id };
      }
    }

    return null;
  }


/*
  * Loads the ActivityEvent data for the current activityEventId.
  *
  * Fully respects the ActivityEventService caching strategy and error handling strategy.
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
    if (!this.activityEventService.userIsBMCActivityEventReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ActivityEvents.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate activityEventId
    //
    if (!this.activityEventId) {

      this.alertService.showMessage('No ActivityEvent ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const activityEventId = Number(this.activityEventId);

    if (isNaN(activityEventId) || activityEventId <= 0) {

      this.alertService.showMessage(`Invalid Activity Event ID: "${this.activityEventId}"`,
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
      // This is the most targeted way: clear only this ActivityEvent + relations

      this.activityEventService.ClearRecordCache(activityEventId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.activityEventService.GetActivityEvent(activityEventId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (activityEventData) => {

        //
        // Success path — activityEventData can legitimately be null if 404'd but request succeeded
        //
        if (!activityEventData) {

          this.handleActivityEventNotFound(activityEventId);

        } else {

          this.activityEventData = activityEventData;
          this.buildFormValues(this.activityEventData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ActivityEvent loaded successfully',
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
        this.handleActivityEventLoadError(error, activityEventId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleActivityEventNotFound(activityEventId: number): void {

    this.activityEventData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ActivityEvent #${activityEventId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleActivityEventLoadError(error: any, activityEventId: number): void {

    let message = 'Failed to load Activity Event.';
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
          message = 'You do not have permission to view this Activity Event.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Activity Event #${activityEventId} was not found.`;
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

    console.error(`Activity Event load failed (ID: ${activityEventId})`, error);

    //
    // Reset UI to safe state
    //
    this.activityEventData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(activityEventData: ActivityEventData | null) {

    if (activityEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.activityEventForm.reset({
        activityEventTypeId: null,
        title: '',
        description: '',
        relatedEntityType: '',
        relatedEntityId: '',
        eventDate: '',
        isPublic: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.activityEventForm.reset({
        activityEventTypeId: activityEventData.activityEventTypeId,
        title: activityEventData.title ?? '',
        description: activityEventData.description ?? '',
        relatedEntityType: activityEventData.relatedEntityType ?? '',
        relatedEntityId: activityEventData.relatedEntityId?.toString() ?? '',
        eventDate: isoUtcStringToDateTimeLocal(activityEventData.eventDate) ?? '',
        isPublic: activityEventData.isPublic ?? false,
        active: activityEventData.active ?? true,
        deleted: activityEventData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.activityEventForm.markAsPristine();
    this.activityEventForm.markAsUntouched();
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

    if (this.activityEventService.userIsBMCActivityEventWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Activity Events", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.activityEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.activityEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.activityEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const activityEventSubmitData: ActivityEventSubmitData = {
        id: this.activityEventData?.id || 0,
        activityEventTypeId: Number(formValue.activityEventTypeId),
        title: formValue.title!.trim(),
        description: formValue.description?.trim() || null,
        relatedEntityType: formValue.relatedEntityType?.trim() || null,
        relatedEntityId: formValue.relatedEntityId ? Number(formValue.relatedEntityId) : null,
        eventDate: dateTimeLocalToIsoUtc(formValue.eventDate!.trim())!,
        isPublic: !!formValue.isPublic,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.activityEventService.PutActivityEvent(activityEventSubmitData.id, activityEventSubmitData)
      : this.activityEventService.PostActivityEvent(activityEventSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedActivityEventData) => {

        this.activityEventService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Activity Event's detail page
          //
          this.activityEventForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.activityEventForm.markAsUntouched();

          this.router.navigate(['/activityevents', savedActivityEventData.id]);
          this.alertService.showMessage('Activity Event added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.activityEventData = savedActivityEventData;
          this.buildFormValues(this.activityEventData);

          this.alertService.showMessage("Activity Event saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Activity Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Activity Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Activity Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCActivityEventReader(): boolean {
    return this.activityEventService.userIsBMCActivityEventReader();
  }

  public userIsBMCActivityEventWriter(): boolean {
    return this.activityEventService.userIsBMCActivityEventWriter();
  }
}
