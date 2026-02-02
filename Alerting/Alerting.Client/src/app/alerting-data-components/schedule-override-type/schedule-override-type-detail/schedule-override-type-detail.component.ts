/*
   GENERATED FORM FOR THE SCHEDULEOVERRIDETYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduleOverrideType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to schedule-override-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduleOverrideTypeService, ScheduleOverrideTypeData, ScheduleOverrideTypeSubmitData } from '../../../alerting-data-services/schedule-override-type.service';
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
interface ScheduleOverrideTypeFormValues {
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-schedule-override-type-detail',
  templateUrl: './schedule-override-type-detail.component.html',
  styleUrls: ['./schedule-override-type-detail.component.scss']
})

export class ScheduleOverrideTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduleOverrideTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduleOverrideTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public scheduleOverrideTypeId: string | null = null;
  public scheduleOverrideTypeData: ScheduleOverrideTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  scheduleOverrideTypes$ = this.scheduleOverrideTypeService.GetScheduleOverrideTypeList();
  public scheduleOverrides$ = this.scheduleOverrideService.GetScheduleOverrideList();

  private destroy$ = new Subject<void>();

  constructor(
    public scheduleOverrideTypeService: ScheduleOverrideTypeService,
    public scheduleOverrideService: ScheduleOverrideService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the scheduleOverrideTypeId from the route parameters
    this.scheduleOverrideTypeId = this.route.snapshot.paramMap.get('scheduleOverrideTypeId');

    if (this.scheduleOverrideTypeId === 'new' ||
        this.scheduleOverrideTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.scheduleOverrideTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduleOverrideTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduleOverrideTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Schedule Override Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Schedule Override Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.scheduleOverrideTypeForm.dirty) {
      return confirm('You have unsaved Schedule Override Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.scheduleOverrideTypeId != null && this.scheduleOverrideTypeId !== 'new') {

      const id = parseInt(this.scheduleOverrideTypeId, 10);

      if (!isNaN(id)) {
        return { scheduleOverrideTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the ScheduleOverrideType data for the current scheduleOverrideTypeId.
  *
  * Fully respects the ScheduleOverrideTypeService caching strategy and error handling strategy.
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
    if (!this.scheduleOverrideTypeService.userIsAlertingScheduleOverrideTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ScheduleOverrideTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate scheduleOverrideTypeId
    //
    if (!this.scheduleOverrideTypeId) {

      this.alertService.showMessage('No ScheduleOverrideType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const scheduleOverrideTypeId = Number(this.scheduleOverrideTypeId);

    if (isNaN(scheduleOverrideTypeId) || scheduleOverrideTypeId <= 0) {

      this.alertService.showMessage(`Invalid Schedule Override Type ID: "${this.scheduleOverrideTypeId}"`,
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
      // This is the most targeted way: clear only this ScheduleOverrideType + relations

      this.scheduleOverrideTypeService.ClearRecordCache(scheduleOverrideTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.scheduleOverrideTypeService.GetScheduleOverrideType(scheduleOverrideTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (scheduleOverrideTypeData) => {

        //
        // Success path — scheduleOverrideTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!scheduleOverrideTypeData) {

          this.handleScheduleOverrideTypeNotFound(scheduleOverrideTypeId);

        } else {

          this.scheduleOverrideTypeData = scheduleOverrideTypeData;
          this.buildFormValues(this.scheduleOverrideTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ScheduleOverrideType loaded successfully',
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
        this.handleScheduleOverrideTypeLoadError(error, scheduleOverrideTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleScheduleOverrideTypeNotFound(scheduleOverrideTypeId: number): void {

    this.scheduleOverrideTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ScheduleOverrideType #${scheduleOverrideTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleScheduleOverrideTypeLoadError(error: any, scheduleOverrideTypeId: number): void {

    let message = 'Failed to load Schedule Override Type.';
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
          message = 'You do not have permission to view this Schedule Override Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Schedule Override Type #${scheduleOverrideTypeId} was not found.`;
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

    console.error(`Schedule Override Type load failed (ID: ${scheduleOverrideTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.scheduleOverrideTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(scheduleOverrideTypeData: ScheduleOverrideTypeData | null) {

    if (scheduleOverrideTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduleOverrideTypeForm.reset({
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduleOverrideTypeForm.reset({
        name: scheduleOverrideTypeData.name ?? '',
        description: scheduleOverrideTypeData.description ?? '',
        active: scheduleOverrideTypeData.active ?? true,
        deleted: scheduleOverrideTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduleOverrideTypeForm.markAsPristine();
    this.scheduleOverrideTypeForm.markAsUntouched();
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

    if (this.scheduleOverrideTypeService.userIsAlertingScheduleOverrideTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Schedule Override Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.scheduleOverrideTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduleOverrideTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduleOverrideTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduleOverrideTypeSubmitData: ScheduleOverrideTypeSubmitData = {
        id: this.scheduleOverrideTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.scheduleOverrideTypeService.PutScheduleOverrideType(scheduleOverrideTypeSubmitData.id, scheduleOverrideTypeSubmitData)
      : this.scheduleOverrideTypeService.PostScheduleOverrideType(scheduleOverrideTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedScheduleOverrideTypeData) => {

        this.scheduleOverrideTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Schedule Override Type's detail page
          //
          this.scheduleOverrideTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.scheduleOverrideTypeForm.markAsUntouched();

          this.router.navigate(['/scheduleoverridetypes', savedScheduleOverrideTypeData.id]);
          this.alertService.showMessage('Schedule Override Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.scheduleOverrideTypeData = savedScheduleOverrideTypeData;
          this.buildFormValues(this.scheduleOverrideTypeData);

          this.alertService.showMessage("Schedule Override Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Schedule Override Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Schedule Override Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Schedule Override Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingScheduleOverrideTypeReader(): boolean {
    return this.scheduleOverrideTypeService.userIsAlertingScheduleOverrideTypeReader();
  }

  public userIsAlertingScheduleOverrideTypeWriter(): boolean {
    return this.scheduleOverrideTypeService.userIsAlertingScheduleOverrideTypeWriter();
  }
}
