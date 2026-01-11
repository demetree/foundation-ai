/*
   GENERATED FORM FOR THE EVENTRESOURCEASSIGNMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventResourceAssignmentChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-resource-assignment-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventResourceAssignmentChangeHistoryService, EventResourceAssignmentChangeHistoryData, EventResourceAssignmentChangeHistorySubmitData } from '../../../scheduler-data-services/event-resource-assignment-change-history.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
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
interface EventResourceAssignmentChangeHistoryFormValues {
  eventResourceAssignmentId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-event-resource-assignment-change-history-detail',
  templateUrl: './event-resource-assignment-change-history-detail.component.html',
  styleUrls: ['./event-resource-assignment-change-history-detail.component.scss']
})

export class EventResourceAssignmentChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventResourceAssignmentChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventResourceAssignmentChangeHistoryForm: FormGroup = this.fb.group({
        eventResourceAssignmentId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public eventResourceAssignmentChangeHistoryId: string | null = null;
  public eventResourceAssignmentChangeHistoryData: EventResourceAssignmentChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  eventResourceAssignmentChangeHistories$ = this.eventResourceAssignmentChangeHistoryService.GetEventResourceAssignmentChangeHistoryList();
  public eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public eventResourceAssignmentChangeHistoryService: EventResourceAssignmentChangeHistoryService,
    public eventResourceAssignmentService: EventResourceAssignmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the eventResourceAssignmentChangeHistoryId from the route parameters
    this.eventResourceAssignmentChangeHistoryId = this.route.snapshot.paramMap.get('eventResourceAssignmentChangeHistoryId');

    if (this.eventResourceAssignmentChangeHistoryId === 'new' ||
        this.eventResourceAssignmentChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.eventResourceAssignmentChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.eventResourceAssignmentChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventResourceAssignmentChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Event Resource Assignment Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Event Resource Assignment Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.eventResourceAssignmentChangeHistoryForm.dirty) {
      return confirm('You have unsaved Event Resource Assignment Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.eventResourceAssignmentChangeHistoryId != null && this.eventResourceAssignmentChangeHistoryId !== 'new') {

      const id = parseInt(this.eventResourceAssignmentChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { eventResourceAssignmentChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the EventResourceAssignmentChangeHistory data for the current eventResourceAssignmentChangeHistoryId.
  *
  * Fully respects the EventResourceAssignmentChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.eventResourceAssignmentChangeHistoryService.userIsSchedulerEventResourceAssignmentChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EventResourceAssignmentChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate eventResourceAssignmentChangeHistoryId
    //
    if (!this.eventResourceAssignmentChangeHistoryId) {

      this.alertService.showMessage('No EventResourceAssignmentChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const eventResourceAssignmentChangeHistoryId = Number(this.eventResourceAssignmentChangeHistoryId);

    if (isNaN(eventResourceAssignmentChangeHistoryId) || eventResourceAssignmentChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Event Resource Assignment Change History ID: "${this.eventResourceAssignmentChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this EventResourceAssignmentChangeHistory + relations

      this.eventResourceAssignmentChangeHistoryService.ClearRecordCache(eventResourceAssignmentChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.eventResourceAssignmentChangeHistoryService.GetEventResourceAssignmentChangeHistory(eventResourceAssignmentChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (eventResourceAssignmentChangeHistoryData) => {

        //
        // Success path — eventResourceAssignmentChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!eventResourceAssignmentChangeHistoryData) {

          this.handleEventResourceAssignmentChangeHistoryNotFound(eventResourceAssignmentChangeHistoryId);

        } else {

          this.eventResourceAssignmentChangeHistoryData = eventResourceAssignmentChangeHistoryData;
          this.buildFormValues(this.eventResourceAssignmentChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EventResourceAssignmentChangeHistory loaded successfully',
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
        this.handleEventResourceAssignmentChangeHistoryLoadError(error, eventResourceAssignmentChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEventResourceAssignmentChangeHistoryNotFound(eventResourceAssignmentChangeHistoryId: number): void {

    this.eventResourceAssignmentChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EventResourceAssignmentChangeHistory #${eventResourceAssignmentChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEventResourceAssignmentChangeHistoryLoadError(error: any, eventResourceAssignmentChangeHistoryId: number): void {

    let message = 'Failed to load Event Resource Assignment Change History.';
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
          message = 'You do not have permission to view this Event Resource Assignment Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Event Resource Assignment Change History #${eventResourceAssignmentChangeHistoryId} was not found.`;
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

    console.error(`Event Resource Assignment Change History load failed (ID: ${eventResourceAssignmentChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.eventResourceAssignmentChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(eventResourceAssignmentChangeHistoryData: EventResourceAssignmentChangeHistoryData | null) {

    if (eventResourceAssignmentChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventResourceAssignmentChangeHistoryForm.reset({
        eventResourceAssignmentId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventResourceAssignmentChangeHistoryForm.reset({
        eventResourceAssignmentId: eventResourceAssignmentChangeHistoryData.eventResourceAssignmentId,
        versionNumber: eventResourceAssignmentChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(eventResourceAssignmentChangeHistoryData.timeStamp) ?? '',
        userId: eventResourceAssignmentChangeHistoryData.userId?.toString() ?? '',
        data: eventResourceAssignmentChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.eventResourceAssignmentChangeHistoryForm.markAsPristine();
    this.eventResourceAssignmentChangeHistoryForm.markAsUntouched();
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

    if (this.eventResourceAssignmentChangeHistoryService.userIsSchedulerEventResourceAssignmentChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Event Resource Assignment Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.eventResourceAssignmentChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventResourceAssignmentChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventResourceAssignmentChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventResourceAssignmentChangeHistorySubmitData: EventResourceAssignmentChangeHistorySubmitData = {
        id: this.eventResourceAssignmentChangeHistoryData?.id || 0,
        eventResourceAssignmentId: Number(formValue.eventResourceAssignmentId),
        versionNumber: this.eventResourceAssignmentChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.eventResourceAssignmentChangeHistoryService.PutEventResourceAssignmentChangeHistory(eventResourceAssignmentChangeHistorySubmitData.id, eventResourceAssignmentChangeHistorySubmitData)
      : this.eventResourceAssignmentChangeHistoryService.PostEventResourceAssignmentChangeHistory(eventResourceAssignmentChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEventResourceAssignmentChangeHistoryData) => {

        this.eventResourceAssignmentChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Event Resource Assignment Change History's detail page
          //
          this.eventResourceAssignmentChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.eventResourceAssignmentChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/eventresourceassignmentchangehistories', savedEventResourceAssignmentChangeHistoryData.id]);
          this.alertService.showMessage('Event Resource Assignment Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.eventResourceAssignmentChangeHistoryData = savedEventResourceAssignmentChangeHistoryData;
          this.buildFormValues(this.eventResourceAssignmentChangeHistoryData);

          this.alertService.showMessage("Event Resource Assignment Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Event Resource Assignment Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Resource Assignment Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Resource Assignment Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerEventResourceAssignmentChangeHistoryReader(): boolean {
    return this.eventResourceAssignmentChangeHistoryService.userIsSchedulerEventResourceAssignmentChangeHistoryReader();
  }

  public userIsSchedulerEventResourceAssignmentChangeHistoryWriter(): boolean {
    return this.eventResourceAssignmentChangeHistoryService.userIsSchedulerEventResourceAssignmentChangeHistoryWriter();
  }
}
