/*
   GENERATED FORM FOR THE SCHEDULEOVERRIDE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduleOverride table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to schedule-override-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduleOverrideService, ScheduleOverrideData, ScheduleOverrideSubmitData } from '../../../alerting-data-services/schedule-override.service';
import { OnCallScheduleService } from '../../../alerting-data-services/on-call-schedule.service';
import { ScheduleLayerService } from '../../../alerting-data-services/schedule-layer.service';
import { ScheduleOverrideTypeService } from '../../../alerting-data-services/schedule-override-type.service';
import { ScheduleOverrideChangeHistoryService } from '../../../alerting-data-services/schedule-override-change-history.service';
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
interface ScheduleOverrideFormValues {
  onCallScheduleId: number | bigint,       // For FK link number
  scheduleLayerId: number | bigint | null,       // For FK link number
  startDateTime: string,
  endDateTime: string,
  scheduleOverrideTypeId: number | bigint,       // For FK link number
  originalUserObjectGuid: string | null,
  replacementUserObjectGuid: string | null,
  reason: string | null,
  createdByUserObjectGuid: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-schedule-override-detail',
  templateUrl: './schedule-override-detail.component.html',
  styleUrls: ['./schedule-override-detail.component.scss']
})

export class ScheduleOverrideDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduleOverrideFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduleOverrideForm: FormGroup = this.fb.group({
        onCallScheduleId: [null, Validators.required],
        scheduleLayerId: [null],
        startDateTime: ['', Validators.required],
        endDateTime: ['', Validators.required],
        scheduleOverrideTypeId: [null, Validators.required],
        originalUserObjectGuid: [''],
        replacementUserObjectGuid: [''],
        reason: [''],
        createdByUserObjectGuid: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public scheduleOverrideId: string | null = null;
  public scheduleOverrideData: ScheduleOverrideData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  scheduleOverrides$ = this.scheduleOverrideService.GetScheduleOverrideList();
  public onCallSchedules$ = this.onCallScheduleService.GetOnCallScheduleList();
  public scheduleLayers$ = this.scheduleLayerService.GetScheduleLayerList();
  public scheduleOverrideTypes$ = this.scheduleOverrideTypeService.GetScheduleOverrideTypeList();
  public scheduleOverrideChangeHistories$ = this.scheduleOverrideChangeHistoryService.GetScheduleOverrideChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public scheduleOverrideService: ScheduleOverrideService,
    public onCallScheduleService: OnCallScheduleService,
    public scheduleLayerService: ScheduleLayerService,
    public scheduleOverrideTypeService: ScheduleOverrideTypeService,
    public scheduleOverrideChangeHistoryService: ScheduleOverrideChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the scheduleOverrideId from the route parameters
    this.scheduleOverrideId = this.route.snapshot.paramMap.get('scheduleOverrideId');

    if (this.scheduleOverrideId === 'new' ||
        this.scheduleOverrideId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.scheduleOverrideData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduleOverrideForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduleOverrideForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Schedule Override';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Schedule Override';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.scheduleOverrideForm.dirty) {
      return confirm('You have unsaved Schedule Override changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.scheduleOverrideId != null && this.scheduleOverrideId !== 'new') {

      const id = parseInt(this.scheduleOverrideId, 10);

      if (!isNaN(id)) {
        return { scheduleOverrideId: id };
      }
    }

    return null;
  }


/*
  * Loads the ScheduleOverride data for the current scheduleOverrideId.
  *
  * Fully respects the ScheduleOverrideService caching strategy and error handling strategy.
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
    if (!this.scheduleOverrideService.userIsAlertingScheduleOverrideReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ScheduleOverrides.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate scheduleOverrideId
    //
    if (!this.scheduleOverrideId) {

      this.alertService.showMessage('No ScheduleOverride ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const scheduleOverrideId = Number(this.scheduleOverrideId);

    if (isNaN(scheduleOverrideId) || scheduleOverrideId <= 0) {

      this.alertService.showMessage(`Invalid Schedule Override ID: "${this.scheduleOverrideId}"`,
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
      // This is the most targeted way: clear only this ScheduleOverride + relations

      this.scheduleOverrideService.ClearRecordCache(scheduleOverrideId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.scheduleOverrideService.GetScheduleOverride(scheduleOverrideId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (scheduleOverrideData) => {

        //
        // Success path — scheduleOverrideData can legitimately be null if 404'd but request succeeded
        //
        if (!scheduleOverrideData) {

          this.handleScheduleOverrideNotFound(scheduleOverrideId);

        } else {

          this.scheduleOverrideData = scheduleOverrideData;
          this.buildFormValues(this.scheduleOverrideData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ScheduleOverride loaded successfully',
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
        this.handleScheduleOverrideLoadError(error, scheduleOverrideId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleScheduleOverrideNotFound(scheduleOverrideId: number): void {

    this.scheduleOverrideData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ScheduleOverride #${scheduleOverrideId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleScheduleOverrideLoadError(error: any, scheduleOverrideId: number): void {

    let message = 'Failed to load Schedule Override.';
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
          message = 'You do not have permission to view this Schedule Override.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Schedule Override #${scheduleOverrideId} was not found.`;
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

    console.error(`Schedule Override load failed (ID: ${scheduleOverrideId})`, error);

    //
    // Reset UI to safe state
    //
    this.scheduleOverrideData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(scheduleOverrideData: ScheduleOverrideData | null) {

    if (scheduleOverrideData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduleOverrideForm.reset({
        onCallScheduleId: null,
        scheduleLayerId: null,
        startDateTime: '',
        endDateTime: '',
        scheduleOverrideTypeId: null,
        originalUserObjectGuid: '',
        replacementUserObjectGuid: '',
        reason: '',
        createdByUserObjectGuid: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduleOverrideForm.reset({
        onCallScheduleId: scheduleOverrideData.onCallScheduleId,
        scheduleLayerId: scheduleOverrideData.scheduleLayerId,
        startDateTime: isoUtcStringToDateTimeLocal(scheduleOverrideData.startDateTime) ?? '',
        endDateTime: isoUtcStringToDateTimeLocal(scheduleOverrideData.endDateTime) ?? '',
        scheduleOverrideTypeId: scheduleOverrideData.scheduleOverrideTypeId,
        originalUserObjectGuid: scheduleOverrideData.originalUserObjectGuid ?? '',
        replacementUserObjectGuid: scheduleOverrideData.replacementUserObjectGuid ?? '',
        reason: scheduleOverrideData.reason ?? '',
        createdByUserObjectGuid: scheduleOverrideData.createdByUserObjectGuid ?? '',
        versionNumber: scheduleOverrideData.versionNumber?.toString() ?? '',
        active: scheduleOverrideData.active ?? true,
        deleted: scheduleOverrideData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduleOverrideForm.markAsPristine();
    this.scheduleOverrideForm.markAsUntouched();
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

    if (this.scheduleOverrideService.userIsAlertingScheduleOverrideWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Schedule Overrides", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.scheduleOverrideForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduleOverrideForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduleOverrideForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduleOverrideSubmitData: ScheduleOverrideSubmitData = {
        id: this.scheduleOverrideData?.id || 0,
        onCallScheduleId: Number(formValue.onCallScheduleId),
        scheduleLayerId: formValue.scheduleLayerId ? Number(formValue.scheduleLayerId) : null,
        startDateTime: dateTimeLocalToIsoUtc(formValue.startDateTime!.trim())!,
        endDateTime: dateTimeLocalToIsoUtc(formValue.endDateTime!.trim())!,
        scheduleOverrideTypeId: Number(formValue.scheduleOverrideTypeId),
        originalUserObjectGuid: formValue.originalUserObjectGuid?.trim() || null,
        replacementUserObjectGuid: formValue.replacementUserObjectGuid?.trim() || null,
        reason: formValue.reason?.trim() || null,
        createdByUserObjectGuid: formValue.createdByUserObjectGuid!.trim(),
        versionNumber: this.scheduleOverrideData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.scheduleOverrideService.PutScheduleOverride(scheduleOverrideSubmitData.id, scheduleOverrideSubmitData)
      : this.scheduleOverrideService.PostScheduleOverride(scheduleOverrideSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedScheduleOverrideData) => {

        this.scheduleOverrideService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Schedule Override's detail page
          //
          this.scheduleOverrideForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.scheduleOverrideForm.markAsUntouched();

          this.router.navigate(['/scheduleoverrides', savedScheduleOverrideData.id]);
          this.alertService.showMessage('Schedule Override added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.scheduleOverrideData = savedScheduleOverrideData;
          this.buildFormValues(this.scheduleOverrideData);

          this.alertService.showMessage("Schedule Override saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Schedule Override.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Schedule Override.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Schedule Override could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingScheduleOverrideReader(): boolean {
    return this.scheduleOverrideService.userIsAlertingScheduleOverrideReader();
  }

  public userIsAlertingScheduleOverrideWriter(): boolean {
    return this.scheduleOverrideService.userIsAlertingScheduleOverrideWriter();
  }
}
