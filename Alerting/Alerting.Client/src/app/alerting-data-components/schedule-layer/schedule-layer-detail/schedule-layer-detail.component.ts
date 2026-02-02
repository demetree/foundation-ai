/*
   GENERATED FORM FOR THE SCHEDULELAYER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduleLayer table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to schedule-layer-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduleLayerService, ScheduleLayerData, ScheduleLayerSubmitData } from '../../../alerting-data-services/schedule-layer.service';
import { OnCallScheduleService } from '../../../alerting-data-services/on-call-schedule.service';
import { ScheduleLayerChangeHistoryService } from '../../../alerting-data-services/schedule-layer-change-history.service';
import { ScheduleLayerMemberService } from '../../../alerting-data-services/schedule-layer-member.service';
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
interface ScheduleLayerFormValues {
  onCallScheduleId: number | bigint,       // For FK link number
  name: string,
  description: string | null,
  layerLevel: string,     // Stored as string for form input, converted to number on submit.
  rotationStart: string,
  rotationDays: string,     // Stored as string for form input, converted to number on submit.
  handoffTime: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-schedule-layer-detail',
  templateUrl: './schedule-layer-detail.component.html',
  styleUrls: ['./schedule-layer-detail.component.scss']
})

export class ScheduleLayerDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduleLayerFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduleLayerForm: FormGroup = this.fb.group({
        onCallScheduleId: [null, Validators.required],
        name: ['', Validators.required],
        description: [''],
        layerLevel: ['', Validators.required],
        rotationStart: ['', Validators.required],
        rotationDays: ['', Validators.required],
        handoffTime: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public scheduleLayerId: string | null = null;
  public scheduleLayerData: ScheduleLayerData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  scheduleLayers$ = this.scheduleLayerService.GetScheduleLayerList();
  public onCallSchedules$ = this.onCallScheduleService.GetOnCallScheduleList();
  public scheduleLayerChangeHistories$ = this.scheduleLayerChangeHistoryService.GetScheduleLayerChangeHistoryList();
  public scheduleLayerMembers$ = this.scheduleLayerMemberService.GetScheduleLayerMemberList();
  public scheduleOverrides$ = this.scheduleOverrideService.GetScheduleOverrideList();

  private destroy$ = new Subject<void>();

  constructor(
    public scheduleLayerService: ScheduleLayerService,
    public onCallScheduleService: OnCallScheduleService,
    public scheduleLayerChangeHistoryService: ScheduleLayerChangeHistoryService,
    public scheduleLayerMemberService: ScheduleLayerMemberService,
    public scheduleOverrideService: ScheduleOverrideService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the scheduleLayerId from the route parameters
    this.scheduleLayerId = this.route.snapshot.paramMap.get('scheduleLayerId');

    if (this.scheduleLayerId === 'new' ||
        this.scheduleLayerId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.scheduleLayerData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduleLayerForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduleLayerForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Schedule Layer';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Schedule Layer';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.scheduleLayerForm.dirty) {
      return confirm('You have unsaved Schedule Layer changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.scheduleLayerId != null && this.scheduleLayerId !== 'new') {

      const id = parseInt(this.scheduleLayerId, 10);

      if (!isNaN(id)) {
        return { scheduleLayerId: id };
      }
    }

    return null;
  }


/*
  * Loads the ScheduleLayer data for the current scheduleLayerId.
  *
  * Fully respects the ScheduleLayerService caching strategy and error handling strategy.
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
    if (!this.scheduleLayerService.userIsAlertingScheduleLayerReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ScheduleLayers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate scheduleLayerId
    //
    if (!this.scheduleLayerId) {

      this.alertService.showMessage('No ScheduleLayer ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const scheduleLayerId = Number(this.scheduleLayerId);

    if (isNaN(scheduleLayerId) || scheduleLayerId <= 0) {

      this.alertService.showMessage(`Invalid Schedule Layer ID: "${this.scheduleLayerId}"`,
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
      // This is the most targeted way: clear only this ScheduleLayer + relations

      this.scheduleLayerService.ClearRecordCache(scheduleLayerId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.scheduleLayerService.GetScheduleLayer(scheduleLayerId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (scheduleLayerData) => {

        //
        // Success path — scheduleLayerData can legitimately be null if 404'd but request succeeded
        //
        if (!scheduleLayerData) {

          this.handleScheduleLayerNotFound(scheduleLayerId);

        } else {

          this.scheduleLayerData = scheduleLayerData;
          this.buildFormValues(this.scheduleLayerData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ScheduleLayer loaded successfully',
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
        this.handleScheduleLayerLoadError(error, scheduleLayerId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleScheduleLayerNotFound(scheduleLayerId: number): void {

    this.scheduleLayerData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ScheduleLayer #${scheduleLayerId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleScheduleLayerLoadError(error: any, scheduleLayerId: number): void {

    let message = 'Failed to load Schedule Layer.';
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
          message = 'You do not have permission to view this Schedule Layer.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Schedule Layer #${scheduleLayerId} was not found.`;
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

    console.error(`Schedule Layer load failed (ID: ${scheduleLayerId})`, error);

    //
    // Reset UI to safe state
    //
    this.scheduleLayerData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(scheduleLayerData: ScheduleLayerData | null) {

    if (scheduleLayerData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduleLayerForm.reset({
        onCallScheduleId: null,
        name: '',
        description: '',
        layerLevel: '',
        rotationStart: '',
        rotationDays: '',
        handoffTime: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduleLayerForm.reset({
        onCallScheduleId: scheduleLayerData.onCallScheduleId,
        name: scheduleLayerData.name ?? '',
        description: scheduleLayerData.description ?? '',
        layerLevel: scheduleLayerData.layerLevel?.toString() ?? '',
        rotationStart: isoUtcStringToDateTimeLocal(scheduleLayerData.rotationStart) ?? '',
        rotationDays: scheduleLayerData.rotationDays?.toString() ?? '',
        handoffTime: scheduleLayerData.handoffTime ?? '',
        versionNumber: scheduleLayerData.versionNumber?.toString() ?? '',
        active: scheduleLayerData.active ?? true,
        deleted: scheduleLayerData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduleLayerForm.markAsPristine();
    this.scheduleLayerForm.markAsUntouched();
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

    if (this.scheduleLayerService.userIsAlertingScheduleLayerWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Schedule Layers", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.scheduleLayerForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduleLayerForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduleLayerForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduleLayerSubmitData: ScheduleLayerSubmitData = {
        id: this.scheduleLayerData?.id || 0,
        onCallScheduleId: Number(formValue.onCallScheduleId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        layerLevel: Number(formValue.layerLevel),
        rotationStart: dateTimeLocalToIsoUtc(formValue.rotationStart!.trim())!,
        rotationDays: Number(formValue.rotationDays),
        handoffTime: formValue.handoffTime!.trim(),
        versionNumber: this.scheduleLayerData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.scheduleLayerService.PutScheduleLayer(scheduleLayerSubmitData.id, scheduleLayerSubmitData)
      : this.scheduleLayerService.PostScheduleLayer(scheduleLayerSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedScheduleLayerData) => {

        this.scheduleLayerService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Schedule Layer's detail page
          //
          this.scheduleLayerForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.scheduleLayerForm.markAsUntouched();

          this.router.navigate(['/schedulelayers', savedScheduleLayerData.id]);
          this.alertService.showMessage('Schedule Layer added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.scheduleLayerData = savedScheduleLayerData;
          this.buildFormValues(this.scheduleLayerData);

          this.alertService.showMessage("Schedule Layer saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Schedule Layer.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Schedule Layer.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Schedule Layer could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingScheduleLayerReader(): boolean {
    return this.scheduleLayerService.userIsAlertingScheduleLayerReader();
  }

  public userIsAlertingScheduleLayerWriter(): boolean {
    return this.scheduleLayerService.userIsAlertingScheduleLayerWriter();
  }
}
