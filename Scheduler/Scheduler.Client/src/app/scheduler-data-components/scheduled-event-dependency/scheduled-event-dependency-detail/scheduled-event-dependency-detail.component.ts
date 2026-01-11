/*
   GENERATED FORM FOR THE SCHEDULEDEVENTDEPENDENCY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventDependency table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-dependency-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventDependencyService, ScheduledEventDependencyData, ScheduledEventDependencySubmitData } from '../../../scheduler-data-services/scheduled-event-dependency.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { DependencyTypeService } from '../../../scheduler-data-services/dependency-type.service';
import { ScheduledEventDependencyChangeHistoryService } from '../../../scheduler-data-services/scheduled-event-dependency-change-history.service';
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
interface ScheduledEventDependencyFormValues {
  predecessorEventId: number | bigint,       // For FK link number
  successorEventId: number | bigint,       // For FK link number
  dependencyTypeId: number | bigint,       // For FK link number
  lagMinutes: string,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-scheduled-event-dependency-detail',
  templateUrl: './scheduled-event-dependency-detail.component.html',
  styleUrls: ['./scheduled-event-dependency-detail.component.scss']
})

export class ScheduledEventDependencyDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventDependencyFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventDependencyForm: FormGroup = this.fb.group({
        predecessorEventId: [null, Validators.required],
        successorEventId: [null, Validators.required],
        dependencyTypeId: [null, Validators.required],
        lagMinutes: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public scheduledEventDependencyId: string | null = null;
  public scheduledEventDependencyData: ScheduledEventDependencyData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  scheduledEventDependencies$ = this.scheduledEventDependencyService.GetScheduledEventDependencyList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public dependencyTypes$ = this.dependencyTypeService.GetDependencyTypeList();
  public scheduledEventDependencyChangeHistories$ = this.scheduledEventDependencyChangeHistoryService.GetScheduledEventDependencyChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public scheduledEventDependencyService: ScheduledEventDependencyService,
    public scheduledEventService: ScheduledEventService,
    public dependencyTypeService: DependencyTypeService,
    public scheduledEventDependencyChangeHistoryService: ScheduledEventDependencyChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the scheduledEventDependencyId from the route parameters
    this.scheduledEventDependencyId = this.route.snapshot.paramMap.get('scheduledEventDependencyId');

    if (this.scheduledEventDependencyId === 'new' ||
        this.scheduledEventDependencyId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.scheduledEventDependencyData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduledEventDependencyForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventDependencyForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduled Event Dependency';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduled Event Dependency';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.scheduledEventDependencyForm.dirty) {
      return confirm('You have unsaved Scheduled Event Dependency changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.scheduledEventDependencyId != null && this.scheduledEventDependencyId !== 'new') {

      const id = parseInt(this.scheduledEventDependencyId, 10);

      if (!isNaN(id)) {
        return { scheduledEventDependencyId: id };
      }
    }

    return null;
  }


/*
  * Loads the ScheduledEventDependency data for the current scheduledEventDependencyId.
  *
  * Fully respects the ScheduledEventDependencyService caching strategy and error handling strategy.
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
    if (!this.scheduledEventDependencyService.userIsSchedulerScheduledEventDependencyReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ScheduledEventDependencies.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate scheduledEventDependencyId
    //
    if (!this.scheduledEventDependencyId) {

      this.alertService.showMessage('No ScheduledEventDependency ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const scheduledEventDependencyId = Number(this.scheduledEventDependencyId);

    if (isNaN(scheduledEventDependencyId) || scheduledEventDependencyId <= 0) {

      this.alertService.showMessage(`Invalid Scheduled Event Dependency ID: "${this.scheduledEventDependencyId}"`,
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
      // This is the most targeted way: clear only this ScheduledEventDependency + relations

      this.scheduledEventDependencyService.ClearRecordCache(scheduledEventDependencyId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.scheduledEventDependencyService.GetScheduledEventDependency(scheduledEventDependencyId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (scheduledEventDependencyData) => {

        //
        // Success path — scheduledEventDependencyData can legitimately be null if 404'd but request succeeded
        //
        if (!scheduledEventDependencyData) {

          this.handleScheduledEventDependencyNotFound(scheduledEventDependencyId);

        } else {

          this.scheduledEventDependencyData = scheduledEventDependencyData;
          this.buildFormValues(this.scheduledEventDependencyData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ScheduledEventDependency loaded successfully',
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
        this.handleScheduledEventDependencyLoadError(error, scheduledEventDependencyId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleScheduledEventDependencyNotFound(scheduledEventDependencyId: number): void {

    this.scheduledEventDependencyData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ScheduledEventDependency #${scheduledEventDependencyId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleScheduledEventDependencyLoadError(error: any, scheduledEventDependencyId: number): void {

    let message = 'Failed to load Scheduled Event Dependency.';
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
          message = 'You do not have permission to view this Scheduled Event Dependency.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduled Event Dependency #${scheduledEventDependencyId} was not found.`;
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

    console.error(`Scheduled Event Dependency load failed (ID: ${scheduledEventDependencyId})`, error);

    //
    // Reset UI to safe state
    //
    this.scheduledEventDependencyData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(scheduledEventDependencyData: ScheduledEventDependencyData | null) {

    if (scheduledEventDependencyData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventDependencyForm.reset({
        predecessorEventId: null,
        successorEventId: null,
        dependencyTypeId: null,
        lagMinutes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduledEventDependencyForm.reset({
        predecessorEventId: scheduledEventDependencyData.predecessorEventId,
        successorEventId: scheduledEventDependencyData.successorEventId,
        dependencyTypeId: scheduledEventDependencyData.dependencyTypeId,
        lagMinutes: scheduledEventDependencyData.lagMinutes?.toString() ?? '',
        versionNumber: scheduledEventDependencyData.versionNumber?.toString() ?? '',
        active: scheduledEventDependencyData.active ?? true,
        deleted: scheduledEventDependencyData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduledEventDependencyForm.markAsPristine();
    this.scheduledEventDependencyForm.markAsUntouched();
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

    if (this.scheduledEventDependencyService.userIsSchedulerScheduledEventDependencyWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduled Event Dependencies", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.scheduledEventDependencyForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventDependencyForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventDependencyForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventDependencySubmitData: ScheduledEventDependencySubmitData = {
        id: this.scheduledEventDependencyData?.id || 0,
        predecessorEventId: Number(formValue.predecessorEventId),
        successorEventId: Number(formValue.successorEventId),
        dependencyTypeId: Number(formValue.dependencyTypeId),
        lagMinutes: Number(formValue.lagMinutes),
        versionNumber: this.scheduledEventDependencyData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.scheduledEventDependencyService.PutScheduledEventDependency(scheduledEventDependencySubmitData.id, scheduledEventDependencySubmitData)
      : this.scheduledEventDependencyService.PostScheduledEventDependency(scheduledEventDependencySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedScheduledEventDependencyData) => {

        this.scheduledEventDependencyService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduled Event Dependency's detail page
          //
          this.scheduledEventDependencyForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.scheduledEventDependencyForm.markAsUntouched();

          this.router.navigate(['/scheduledeventdependencies', savedScheduledEventDependencyData.id]);
          this.alertService.showMessage('Scheduled Event Dependency added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.scheduledEventDependencyData = savedScheduledEventDependencyData;
          this.buildFormValues(this.scheduledEventDependencyData);

          this.alertService.showMessage("Scheduled Event Dependency saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduled Event Dependency.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Dependency.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Dependency could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerScheduledEventDependencyReader(): boolean {
    return this.scheduledEventDependencyService.userIsSchedulerScheduledEventDependencyReader();
  }

  public userIsSchedulerScheduledEventDependencyWriter(): boolean {
    return this.scheduledEventDependencyService.userIsSchedulerScheduledEventDependencyWriter();
  }
}
