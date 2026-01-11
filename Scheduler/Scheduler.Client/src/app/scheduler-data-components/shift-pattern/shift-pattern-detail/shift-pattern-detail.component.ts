/*
   GENERATED FORM FOR THE SHIFTPATTERN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ShiftPattern table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to shift-pattern-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ShiftPatternService, ShiftPatternData, ShiftPatternSubmitData } from '../../../scheduler-data-services/shift-pattern.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { ShiftPatternChangeHistoryService } from '../../../scheduler-data-services/shift-pattern-change-history.service';
import { ShiftPatternDayService } from '../../../scheduler-data-services/shift-pattern-day.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
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
interface ShiftPatternFormValues {
  name: string,
  description: string | null,
  timeZoneId: number | bigint | null,       // For FK link number
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-shift-pattern-detail',
  templateUrl: './shift-pattern-detail.component.html',
  styleUrls: ['./shift-pattern-detail.component.scss']
})

export class ShiftPatternDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ShiftPatternFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public shiftPatternForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        timeZoneId: [null],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public shiftPatternId: string | null = null;
  public shiftPatternData: ShiftPatternData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  shiftPatterns$ = this.shiftPatternService.GetShiftPatternList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();
  public shiftPatternChangeHistories$ = this.shiftPatternChangeHistoryService.GetShiftPatternChangeHistoryList();
  public shiftPatternDays$ = this.shiftPatternDayService.GetShiftPatternDayList();
  public resources$ = this.resourceService.GetResourceList();

  private destroy$ = new Subject<void>();

  constructor(
    public shiftPatternService: ShiftPatternService,
    public timeZoneService: TimeZoneService,
    public shiftPatternChangeHistoryService: ShiftPatternChangeHistoryService,
    public shiftPatternDayService: ShiftPatternDayService,
    public resourceService: ResourceService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the shiftPatternId from the route parameters
    this.shiftPatternId = this.route.snapshot.paramMap.get('shiftPatternId');

    if (this.shiftPatternId === 'new' ||
        this.shiftPatternId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.shiftPatternData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.shiftPatternForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.shiftPatternForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Shift Pattern';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Shift Pattern';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.shiftPatternForm.dirty) {
      return confirm('You have unsaved Shift Pattern changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.shiftPatternId != null && this.shiftPatternId !== 'new') {

      const id = parseInt(this.shiftPatternId, 10);

      if (!isNaN(id)) {
        return { shiftPatternId: id };
      }
    }

    return null;
  }


/*
  * Loads the ShiftPattern data for the current shiftPatternId.
  *
  * Fully respects the ShiftPatternService caching strategy and error handling strategy.
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
    if (!this.shiftPatternService.userIsSchedulerShiftPatternReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ShiftPatterns.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate shiftPatternId
    //
    if (!this.shiftPatternId) {

      this.alertService.showMessage('No ShiftPattern ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const shiftPatternId = Number(this.shiftPatternId);

    if (isNaN(shiftPatternId) || shiftPatternId <= 0) {

      this.alertService.showMessage(`Invalid Shift Pattern ID: "${this.shiftPatternId}"`,
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
      // This is the most targeted way: clear only this ShiftPattern + relations

      this.shiftPatternService.ClearRecordCache(shiftPatternId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.shiftPatternService.GetShiftPattern(shiftPatternId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (shiftPatternData) => {

        //
        // Success path — shiftPatternData can legitimately be null if 404'd but request succeeded
        //
        if (!shiftPatternData) {

          this.handleShiftPatternNotFound(shiftPatternId);

        } else {

          this.shiftPatternData = shiftPatternData;
          this.buildFormValues(this.shiftPatternData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ShiftPattern loaded successfully',
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
        this.handleShiftPatternLoadError(error, shiftPatternId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleShiftPatternNotFound(shiftPatternId: number): void {

    this.shiftPatternData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ShiftPattern #${shiftPatternId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleShiftPatternLoadError(error: any, shiftPatternId: number): void {

    let message = 'Failed to load Shift Pattern.';
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
          message = 'You do not have permission to view this Shift Pattern.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Shift Pattern #${shiftPatternId} was not found.`;
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

    console.error(`Shift Pattern load failed (ID: ${shiftPatternId})`, error);

    //
    // Reset UI to safe state
    //
    this.shiftPatternData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(shiftPatternData: ShiftPatternData | null) {

    if (shiftPatternData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.shiftPatternForm.reset({
        name: '',
        description: '',
        timeZoneId: null,
        color: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.shiftPatternForm.reset({
        name: shiftPatternData.name ?? '',
        description: shiftPatternData.description ?? '',
        timeZoneId: shiftPatternData.timeZoneId,
        color: shiftPatternData.color ?? '',
        versionNumber: shiftPatternData.versionNumber?.toString() ?? '',
        active: shiftPatternData.active ?? true,
        deleted: shiftPatternData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.shiftPatternForm.markAsPristine();
    this.shiftPatternForm.markAsUntouched();
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

    if (this.shiftPatternService.userIsSchedulerShiftPatternWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Shift Patterns", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.shiftPatternForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.shiftPatternForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.shiftPatternForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const shiftPatternSubmitData: ShiftPatternSubmitData = {
        id: this.shiftPatternData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        timeZoneId: formValue.timeZoneId ? Number(formValue.timeZoneId) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.shiftPatternData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.shiftPatternService.PutShiftPattern(shiftPatternSubmitData.id, shiftPatternSubmitData)
      : this.shiftPatternService.PostShiftPattern(shiftPatternSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedShiftPatternData) => {

        this.shiftPatternService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Shift Pattern's detail page
          //
          this.shiftPatternForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.shiftPatternForm.markAsUntouched();

          this.router.navigate(['/shiftpatterns', savedShiftPatternData.id]);
          this.alertService.showMessage('Shift Pattern added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.shiftPatternData = savedShiftPatternData;
          this.buildFormValues(this.shiftPatternData);

          this.alertService.showMessage("Shift Pattern saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Shift Pattern.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Shift Pattern.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Shift Pattern could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerShiftPatternReader(): boolean {
    return this.shiftPatternService.userIsSchedulerShiftPatternReader();
  }

  public userIsSchedulerShiftPatternWriter(): boolean {
    return this.shiftPatternService.userIsSchedulerShiftPatternWriter();
  }
}
