/*
   GENERATED FORM FOR THE SHIFTPATTERNDAY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ShiftPatternDay table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to shift-pattern-day-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ShiftPatternDayService, ShiftPatternDayData, ShiftPatternDaySubmitData } from '../../../scheduler-data-services/shift-pattern-day.service';
import { ShiftPatternService } from '../../../scheduler-data-services/shift-pattern.service';
import { ShiftPatternDayChangeHistoryService } from '../../../scheduler-data-services/shift-pattern-day-change-history.service';
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
interface ShiftPatternDayFormValues {
  shiftPatternId: number | bigint,       // For FK link number
  dayOfWeek: string,     // Stored as string for form input, converted to number on submit.
  startTime: string,
  hours: string,     // Stored as string for form input, converted to number on submit.
  label: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-shift-pattern-day-detail',
  templateUrl: './shift-pattern-day-detail.component.html',
  styleUrls: ['./shift-pattern-day-detail.component.scss']
})

export class ShiftPatternDayDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ShiftPatternDayFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public shiftPatternDayForm: FormGroup = this.fb.group({
        shiftPatternId: [null, Validators.required],
        dayOfWeek: ['', Validators.required],
        startTime: ['', Validators.required],
        hours: ['', Validators.required],
        label: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public shiftPatternDayId: string | null = null;
  public shiftPatternDayData: ShiftPatternDayData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  shiftPatternDays$ = this.shiftPatternDayService.GetShiftPatternDayList();
  public shiftPatterns$ = this.shiftPatternService.GetShiftPatternList();
  public shiftPatternDayChangeHistories$ = this.shiftPatternDayChangeHistoryService.GetShiftPatternDayChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public shiftPatternDayService: ShiftPatternDayService,
    public shiftPatternService: ShiftPatternService,
    public shiftPatternDayChangeHistoryService: ShiftPatternDayChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the shiftPatternDayId from the route parameters
    this.shiftPatternDayId = this.route.snapshot.paramMap.get('shiftPatternDayId');

    if (this.shiftPatternDayId === 'new' ||
        this.shiftPatternDayId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.shiftPatternDayData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.shiftPatternDayForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.shiftPatternDayForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Shift Pattern Day';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Shift Pattern Day';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.shiftPatternDayForm.dirty) {
      return confirm('You have unsaved Shift Pattern Day changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.shiftPatternDayId != null && this.shiftPatternDayId !== 'new') {

      const id = parseInt(this.shiftPatternDayId, 10);

      if (!isNaN(id)) {
        return { shiftPatternDayId: id };
      }
    }

    return null;
  }


/*
  * Loads the ShiftPatternDay data for the current shiftPatternDayId.
  *
  * Fully respects the ShiftPatternDayService caching strategy and error handling strategy.
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
    if (!this.shiftPatternDayService.userIsSchedulerShiftPatternDayReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ShiftPatternDays.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate shiftPatternDayId
    //
    if (!this.shiftPatternDayId) {

      this.alertService.showMessage('No ShiftPatternDay ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const shiftPatternDayId = Number(this.shiftPatternDayId);

    if (isNaN(shiftPatternDayId) || shiftPatternDayId <= 0) {

      this.alertService.showMessage(`Invalid Shift Pattern Day ID: "${this.shiftPatternDayId}"`,
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
      // This is the most targeted way: clear only this ShiftPatternDay + relations

      this.shiftPatternDayService.ClearRecordCache(shiftPatternDayId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.shiftPatternDayService.GetShiftPatternDay(shiftPatternDayId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (shiftPatternDayData) => {

        //
        // Success path — shiftPatternDayData can legitimately be null if 404'd but request succeeded
        //
        if (!shiftPatternDayData) {

          this.handleShiftPatternDayNotFound(shiftPatternDayId);

        } else {

          this.shiftPatternDayData = shiftPatternDayData;
          this.buildFormValues(this.shiftPatternDayData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ShiftPatternDay loaded successfully',
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
        this.handleShiftPatternDayLoadError(error, shiftPatternDayId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleShiftPatternDayNotFound(shiftPatternDayId: number): void {

    this.shiftPatternDayData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ShiftPatternDay #${shiftPatternDayId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleShiftPatternDayLoadError(error: any, shiftPatternDayId: number): void {

    let message = 'Failed to load Shift Pattern Day.';
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
          message = 'You do not have permission to view this Shift Pattern Day.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Shift Pattern Day #${shiftPatternDayId} was not found.`;
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

    console.error(`Shift Pattern Day load failed (ID: ${shiftPatternDayId})`, error);

    //
    // Reset UI to safe state
    //
    this.shiftPatternDayData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(shiftPatternDayData: ShiftPatternDayData | null) {

    if (shiftPatternDayData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.shiftPatternDayForm.reset({
        shiftPatternId: null,
        dayOfWeek: '',
        startTime: '',
        hours: '',
        label: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.shiftPatternDayForm.reset({
        shiftPatternId: shiftPatternDayData.shiftPatternId,
        dayOfWeek: shiftPatternDayData.dayOfWeek?.toString() ?? '',
        startTime: shiftPatternDayData.startTime ?? '',
        hours: shiftPatternDayData.hours?.toString() ?? '',
        label: shiftPatternDayData.label ?? '',
        versionNumber: shiftPatternDayData.versionNumber?.toString() ?? '',
        active: shiftPatternDayData.active ?? true,
        deleted: shiftPatternDayData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.shiftPatternDayForm.markAsPristine();
    this.shiftPatternDayForm.markAsUntouched();
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

    if (this.shiftPatternDayService.userIsSchedulerShiftPatternDayWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Shift Pattern Days", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.shiftPatternDayForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.shiftPatternDayForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.shiftPatternDayForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const shiftPatternDaySubmitData: ShiftPatternDaySubmitData = {
        id: this.shiftPatternDayData?.id || 0,
        shiftPatternId: Number(formValue.shiftPatternId),
        dayOfWeek: Number(formValue.dayOfWeek),
        startTime: formValue.startTime!.trim(),
        hours: Number(formValue.hours),
        label: formValue.label?.trim() || null,
        versionNumber: this.shiftPatternDayData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.shiftPatternDayService.PutShiftPatternDay(shiftPatternDaySubmitData.id, shiftPatternDaySubmitData)
      : this.shiftPatternDayService.PostShiftPatternDay(shiftPatternDaySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedShiftPatternDayData) => {

        this.shiftPatternDayService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Shift Pattern Day's detail page
          //
          this.shiftPatternDayForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.shiftPatternDayForm.markAsUntouched();

          this.router.navigate(['/shiftpatterndays', savedShiftPatternDayData.id]);
          this.alertService.showMessage('Shift Pattern Day added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.shiftPatternDayData = savedShiftPatternDayData;
          this.buildFormValues(this.shiftPatternDayData);

          this.alertService.showMessage("Shift Pattern Day saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Shift Pattern Day.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Shift Pattern Day.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Shift Pattern Day could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerShiftPatternDayReader(): boolean {
    return this.shiftPatternDayService.userIsSchedulerShiftPatternDayReader();
  }

  public userIsSchedulerShiftPatternDayWriter(): boolean {
    return this.shiftPatternDayService.userIsSchedulerShiftPatternDayWriter();
  }
}
