/*
   GENERATED FORM FOR THE EVENTCALENDAR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventCalendar table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-calendar-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventCalendarService, EventCalendarData, EventCalendarSubmitData } from '../../../scheduler-data-services/event-calendar.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
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
interface EventCalendarFormValues {
  scheduledEventId: number | bigint,       // For FK link number
  calendarId: number | bigint,       // For FK link number
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-event-calendar-detail',
  templateUrl: './event-calendar-detail.component.html',
  styleUrls: ['./event-calendar-detail.component.scss']
})

export class EventCalendarDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventCalendarFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventCalendarForm: FormGroup = this.fb.group({
        scheduledEventId: [null, Validators.required],
        calendarId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });


  public eventCalendarId: string | null = null;
  public eventCalendarData: EventCalendarData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  eventCalendars$ = this.eventCalendarService.GetEventCalendarList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public calendars$ = this.calendarService.GetCalendarList();

  private destroy$ = new Subject<void>();

  constructor(
    public eventCalendarService: EventCalendarService,
    public scheduledEventService: ScheduledEventService,
    public calendarService: CalendarService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the eventCalendarId from the route parameters
    this.eventCalendarId = this.route.snapshot.paramMap.get('eventCalendarId');

    if (this.eventCalendarId === 'new' ||
        this.eventCalendarId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.eventCalendarData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.eventCalendarForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventCalendarForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Event Calendar';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Event Calendar';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.eventCalendarForm.dirty) {
      return confirm('You have unsaved Event Calendar changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.eventCalendarId != null && this.eventCalendarId !== 'new') {

      const id = parseInt(this.eventCalendarId, 10);

      if (!isNaN(id)) {
        return { eventCalendarId: id };
      }
    }

    return null;
  }


/*
  * Loads the EventCalendar data for the current eventCalendarId.
  *
  * Fully respects the EventCalendarService caching strategy and error handling strategy.
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
    if (!this.eventCalendarService.userIsSchedulerEventCalendarReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EventCalendars.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate eventCalendarId
    //
    if (!this.eventCalendarId) {

      this.alertService.showMessage('No EventCalendar ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const eventCalendarId = Number(this.eventCalendarId);

    if (isNaN(eventCalendarId) || eventCalendarId <= 0) {

      this.alertService.showMessage(`Invalid Event Calendar ID: "${this.eventCalendarId}"`,
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
      // This is the most targeted way: clear only this EventCalendar + relations

      this.eventCalendarService.ClearRecordCache(eventCalendarId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.eventCalendarService.GetEventCalendar(eventCalendarId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (eventCalendarData) => {

        //
        // Success path — eventCalendarData can legitimately be null if 404'd but request succeeded
        //
        if (!eventCalendarData) {

          this.handleEventCalendarNotFound(eventCalendarId);

        } else {

          this.eventCalendarData = eventCalendarData;
          this.buildFormValues(this.eventCalendarData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EventCalendar loaded successfully',
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
        this.handleEventCalendarLoadError(error, eventCalendarId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEventCalendarNotFound(eventCalendarId: number): void {

    this.eventCalendarData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EventCalendar #${eventCalendarId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEventCalendarLoadError(error: any, eventCalendarId: number): void {

    let message = 'Failed to load Event Calendar.';
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
          message = 'You do not have permission to view this Event Calendar.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Event Calendar #${eventCalendarId} was not found.`;
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

    console.error(`Event Calendar load failed (ID: ${eventCalendarId})`, error);

    //
    // Reset UI to safe state
    //
    this.eventCalendarData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(eventCalendarData: EventCalendarData | null) {

    if (eventCalendarData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventCalendarForm.reset({
        scheduledEventId: null,
        calendarId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventCalendarForm.reset({
        scheduledEventId: eventCalendarData.scheduledEventId,
        calendarId: eventCalendarData.calendarId,
        active: eventCalendarData.active ?? true,
        deleted: eventCalendarData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.eventCalendarForm.markAsPristine();
    this.eventCalendarForm.markAsUntouched();
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

    if (this.eventCalendarService.userIsSchedulerEventCalendarWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Event Calendars", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.eventCalendarForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventCalendarForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventCalendarForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventCalendarSubmitData: EventCalendarSubmitData = {
        id: this.eventCalendarData?.id || 0,
        scheduledEventId: Number(formValue.scheduledEventId),
        calendarId: Number(formValue.calendarId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.eventCalendarService.PutEventCalendar(eventCalendarSubmitData.id, eventCalendarSubmitData)
      : this.eventCalendarService.PostEventCalendar(eventCalendarSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEventCalendarData) => {

        this.eventCalendarService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Event Calendar's detail page
          //
          this.eventCalendarForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.eventCalendarForm.markAsUntouched();

          this.router.navigate(['/eventcalendars', savedEventCalendarData.id]);
          this.alertService.showMessage('Event Calendar added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.eventCalendarData = savedEventCalendarData;
          this.buildFormValues(this.eventCalendarData);

          this.alertService.showMessage("Event Calendar saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Event Calendar.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Calendar.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Calendar could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerEventCalendarReader(): boolean {
    return this.eventCalendarService.userIsSchedulerEventCalendarReader();
  }

  public userIsSchedulerEventCalendarWriter(): boolean {
    return this.eventCalendarService.userIsSchedulerEventCalendarWriter();
  }
}
