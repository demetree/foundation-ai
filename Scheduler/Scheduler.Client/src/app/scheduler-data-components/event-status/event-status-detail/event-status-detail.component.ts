/*
   GENERATED FORM FOR THE EVENTSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventStatus table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-status-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventStatusService, EventStatusData, EventStatusSubmitData } from '../../../scheduler-data-services/event-status.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
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
interface EventStatusFormValues {
  name: string,
  description: string,
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-event-status-detail',
  templateUrl: './event-status-detail.component.html',
  styleUrls: ['./event-status-detail.component.scss']
})

export class EventStatusDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventStatusFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventStatusForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        color: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public eventStatusId: string | null = null;
  public eventStatusData: EventStatusData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  eventStatuses$ = this.eventStatusService.GetEventStatusList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public eventStatusService: EventStatusService,
    public scheduledEventService: ScheduledEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the eventStatusId from the route parameters
    this.eventStatusId = this.route.snapshot.paramMap.get('eventStatusId');

    if (this.eventStatusId === 'new' ||
        this.eventStatusId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.eventStatusData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.eventStatusForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventStatusForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Event Status';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Event Status';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.eventStatusForm.dirty) {
      return confirm('You have unsaved Event Status changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.eventStatusId != null && this.eventStatusId !== 'new') {

      const id = parseInt(this.eventStatusId, 10);

      if (!isNaN(id)) {
        return { eventStatusId: id };
      }
    }

    return null;
  }


/*
  * Loads the EventStatus data for the current eventStatusId.
  *
  * Fully respects the EventStatusService caching strategy and error handling strategy.
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
    if (!this.eventStatusService.userIsSchedulerEventStatusReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EventStatuses.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate eventStatusId
    //
    if (!this.eventStatusId) {

      this.alertService.showMessage('No EventStatus ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const eventStatusId = Number(this.eventStatusId);

    if (isNaN(eventStatusId) || eventStatusId <= 0) {

      this.alertService.showMessage(`Invalid Event Status ID: "${this.eventStatusId}"`,
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
      // This is the most targeted way: clear only this EventStatus + relations

      this.eventStatusService.ClearRecordCache(eventStatusId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.eventStatusService.GetEventStatus(eventStatusId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (eventStatusData) => {

        //
        // Success path — eventStatusData can legitimately be null if 404'd but request succeeded
        //
        if (!eventStatusData) {

          this.handleEventStatusNotFound(eventStatusId);

        } else {

          this.eventStatusData = eventStatusData;
          this.buildFormValues(this.eventStatusData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EventStatus loaded successfully',
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
        this.handleEventStatusLoadError(error, eventStatusId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEventStatusNotFound(eventStatusId: number): void {

    this.eventStatusData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EventStatus #${eventStatusId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEventStatusLoadError(error: any, eventStatusId: number): void {

    let message = 'Failed to load Event Status.';
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
          message = 'You do not have permission to view this Event Status.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Event Status #${eventStatusId} was not found.`;
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

    console.error(`Event Status load failed (ID: ${eventStatusId})`, error);

    //
    // Reset UI to safe state
    //
    this.eventStatusData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(eventStatusData: EventStatusData | null) {

    if (eventStatusData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventStatusForm.reset({
        name: '',
        description: '',
        color: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventStatusForm.reset({
        name: eventStatusData.name ?? '',
        description: eventStatusData.description ?? '',
        color: eventStatusData.color ?? '',
        sequence: eventStatusData.sequence?.toString() ?? '',
        active: eventStatusData.active ?? true,
        deleted: eventStatusData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.eventStatusForm.markAsPristine();
    this.eventStatusForm.markAsUntouched();
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

    if (this.eventStatusService.userIsSchedulerEventStatusWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Event Statuses", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.eventStatusForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventStatusForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventStatusForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventStatusSubmitData: EventStatusSubmitData = {
        id: this.eventStatusData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.eventStatusService.PutEventStatus(eventStatusSubmitData.id, eventStatusSubmitData)
      : this.eventStatusService.PostEventStatus(eventStatusSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEventStatusData) => {

        this.eventStatusService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Event Status's detail page
          //
          this.eventStatusForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.eventStatusForm.markAsUntouched();

          this.router.navigate(['/eventstatuses', savedEventStatusData.id]);
          this.alertService.showMessage('Event Status added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.eventStatusData = savedEventStatusData;
          this.buildFormValues(this.eventStatusData);

          this.alertService.showMessage("Event Status saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Event Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerEventStatusReader(): boolean {
    return this.eventStatusService.userIsSchedulerEventStatusReader();
  }

  public userIsSchedulerEventStatusWriter(): boolean {
    return this.eventStatusService.userIsSchedulerEventStatusWriter();
  }
}
