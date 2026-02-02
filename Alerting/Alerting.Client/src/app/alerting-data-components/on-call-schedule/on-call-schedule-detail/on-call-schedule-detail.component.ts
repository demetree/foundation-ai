/*
   GENERATED FORM FOR THE ONCALLSCHEDULE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from OnCallSchedule table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to on-call-schedule-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OnCallScheduleService, OnCallScheduleData, OnCallScheduleSubmitData } from '../../../alerting-data-services/on-call-schedule.service';
import { OnCallScheduleChangeHistoryService } from '../../../alerting-data-services/on-call-schedule-change-history.service';
import { ScheduleLayerService } from '../../../alerting-data-services/schedule-layer.service';
import { ScheduleOverrideService } from '../../../alerting-data-services/schedule-override.service';
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
interface OnCallScheduleFormValues {
  name: string,
  description: string | null,
  timeZoneId: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-on-call-schedule-detail',
  templateUrl: './on-call-schedule-detail.component.html',
  styleUrls: ['./on-call-schedule-detail.component.scss']
})

export class OnCallScheduleDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<OnCallScheduleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public onCallScheduleForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        timeZoneId: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public onCallScheduleId: string | null = null;
  public onCallScheduleData: OnCallScheduleData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  onCallSchedules$ = this.onCallScheduleService.GetOnCallScheduleList();
  public onCallScheduleChangeHistories$ = this.onCallScheduleChangeHistoryService.GetOnCallScheduleChangeHistoryList();
  public scheduleLayers$ = this.scheduleLayerService.GetScheduleLayerList();
  public scheduleOverrides$ = this.scheduleOverrideService.GetScheduleOverrideList();

  private destroy$ = new Subject<void>();

  constructor(
    public onCallScheduleService: OnCallScheduleService,
    public onCallScheduleChangeHistoryService: OnCallScheduleChangeHistoryService,
    public scheduleLayerService: ScheduleLayerService,
    public scheduleOverrideService: ScheduleOverrideService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the onCallScheduleId from the route parameters
    this.onCallScheduleId = this.route.snapshot.paramMap.get('onCallScheduleId');

    if (this.onCallScheduleId === 'new' ||
        this.onCallScheduleId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.onCallScheduleData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.onCallScheduleForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.onCallScheduleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New On Call Schedule';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit On Call Schedule';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.onCallScheduleForm.dirty) {
      return confirm('You have unsaved On Call Schedule changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.onCallScheduleId != null && this.onCallScheduleId !== 'new') {

      const id = parseInt(this.onCallScheduleId, 10);

      if (!isNaN(id)) {
        return { onCallScheduleId: id };
      }
    }

    return null;
  }


/*
  * Loads the OnCallSchedule data for the current onCallScheduleId.
  *
  * Fully respects the OnCallScheduleService caching strategy and error handling strategy.
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
    if (!this.onCallScheduleService.userIsAlertingOnCallScheduleReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read OnCallSchedules.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate onCallScheduleId
    //
    if (!this.onCallScheduleId) {

      this.alertService.showMessage('No OnCallSchedule ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const onCallScheduleId = Number(this.onCallScheduleId);

    if (isNaN(onCallScheduleId) || onCallScheduleId <= 0) {

      this.alertService.showMessage(`Invalid On Call Schedule ID: "${this.onCallScheduleId}"`,
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
      // This is the most targeted way: clear only this OnCallSchedule + relations

      this.onCallScheduleService.ClearRecordCache(onCallScheduleId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.onCallScheduleService.GetOnCallSchedule(onCallScheduleId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (onCallScheduleData) => {

        //
        // Success path — onCallScheduleData can legitimately be null if 404'd but request succeeded
        //
        if (!onCallScheduleData) {

          this.handleOnCallScheduleNotFound(onCallScheduleId);

        } else {

          this.onCallScheduleData = onCallScheduleData;
          this.buildFormValues(this.onCallScheduleData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'OnCallSchedule loaded successfully',
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
        this.handleOnCallScheduleLoadError(error, onCallScheduleId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleOnCallScheduleNotFound(onCallScheduleId: number): void {

    this.onCallScheduleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `OnCallSchedule #${onCallScheduleId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleOnCallScheduleLoadError(error: any, onCallScheduleId: number): void {

    let message = 'Failed to load On Call Schedule.';
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
          message = 'You do not have permission to view this On Call Schedule.';
          title = 'Forbidden';
          break;
        case 404:
          message = `On Call Schedule #${onCallScheduleId} was not found.`;
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

    console.error(`On Call Schedule load failed (ID: ${onCallScheduleId})`, error);

    //
    // Reset UI to safe state
    //
    this.onCallScheduleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(onCallScheduleData: OnCallScheduleData | null) {

    if (onCallScheduleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.onCallScheduleForm.reset({
        name: '',
        description: '',
        timeZoneId: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.onCallScheduleForm.reset({
        name: onCallScheduleData.name ?? '',
        description: onCallScheduleData.description ?? '',
        timeZoneId: onCallScheduleData.timeZoneId ?? '',
        versionNumber: onCallScheduleData.versionNumber?.toString() ?? '',
        active: onCallScheduleData.active ?? true,
        deleted: onCallScheduleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.onCallScheduleForm.markAsPristine();
    this.onCallScheduleForm.markAsUntouched();
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

    if (this.onCallScheduleService.userIsAlertingOnCallScheduleWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to On Call Schedules", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.onCallScheduleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.onCallScheduleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.onCallScheduleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const onCallScheduleSubmitData: OnCallScheduleSubmitData = {
        id: this.onCallScheduleData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        timeZoneId: formValue.timeZoneId!.trim(),
        versionNumber: this.onCallScheduleData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.onCallScheduleService.PutOnCallSchedule(onCallScheduleSubmitData.id, onCallScheduleSubmitData)
      : this.onCallScheduleService.PostOnCallSchedule(onCallScheduleSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedOnCallScheduleData) => {

        this.onCallScheduleService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created On Call Schedule's detail page
          //
          this.onCallScheduleForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.onCallScheduleForm.markAsUntouched();

          this.router.navigate(['/oncallschedules', savedOnCallScheduleData.id]);
          this.alertService.showMessage('On Call Schedule added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.onCallScheduleData = savedOnCallScheduleData;
          this.buildFormValues(this.onCallScheduleData);

          this.alertService.showMessage("On Call Schedule saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this On Call Schedule.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the On Call Schedule.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('On Call Schedule could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingOnCallScheduleReader(): boolean {
    return this.onCallScheduleService.userIsAlertingOnCallScheduleReader();
  }

  public userIsAlertingOnCallScheduleWriter(): boolean {
    return this.onCallScheduleService.userIsAlertingOnCallScheduleWriter();
  }
}
