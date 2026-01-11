/*
   GENERATED FORM FOR THE RECURRENCERULE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RecurrenceRule table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to recurrence-rule-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RecurrenceRuleService, RecurrenceRuleData, RecurrenceRuleSubmitData } from '../../../scheduler-data-services/recurrence-rule.service';
import { RecurrenceFrequencyService } from '../../../scheduler-data-services/recurrence-frequency.service';
import { RecurrenceRuleChangeHistoryService } from '../../../scheduler-data-services/recurrence-rule-change-history.service';
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
interface RecurrenceRuleFormValues {
  recurrenceFrequencyId: number | bigint,       // For FK link number
  interval: string,     // Stored as string for form input, converted to number on submit.
  untilDateTime: string | null,
  count: string | null,     // Stored as string for form input, converted to number on submit.
  dayOfWeekMask: string | null,     // Stored as string for form input, converted to number on submit.
  dayOfMonth: string | null,     // Stored as string for form input, converted to number on submit.
  dayOfWeekInMonth: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-recurrence-rule-detail',
  templateUrl: './recurrence-rule-detail.component.html',
  styleUrls: ['./recurrence-rule-detail.component.scss']
})

export class RecurrenceRuleDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RecurrenceRuleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public recurrenceRuleForm: FormGroup = this.fb.group({
        recurrenceFrequencyId: [null, Validators.required],
        interval: ['', Validators.required],
        untilDateTime: [''],
        count: [''],
        dayOfWeekMask: [''],
        dayOfMonth: [''],
        dayOfWeekInMonth: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public recurrenceRuleId: string | null = null;
  public recurrenceRuleData: RecurrenceRuleData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  recurrenceRules$ = this.recurrenceRuleService.GetRecurrenceRuleList();
  public recurrenceFrequencies$ = this.recurrenceFrequencyService.GetRecurrenceFrequencyList();
  public recurrenceRuleChangeHistories$ = this.recurrenceRuleChangeHistoryService.GetRecurrenceRuleChangeHistoryList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public recurrenceRuleService: RecurrenceRuleService,
    public recurrenceFrequencyService: RecurrenceFrequencyService,
    public recurrenceRuleChangeHistoryService: RecurrenceRuleChangeHistoryService,
    public scheduledEventService: ScheduledEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the recurrenceRuleId from the route parameters
    this.recurrenceRuleId = this.route.snapshot.paramMap.get('recurrenceRuleId');

    if (this.recurrenceRuleId === 'new' ||
        this.recurrenceRuleId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.recurrenceRuleData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.recurrenceRuleForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.recurrenceRuleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Recurrence Rule';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Recurrence Rule';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.recurrenceRuleForm.dirty) {
      return confirm('You have unsaved Recurrence Rule changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.recurrenceRuleId != null && this.recurrenceRuleId !== 'new') {

      const id = parseInt(this.recurrenceRuleId, 10);

      if (!isNaN(id)) {
        return { recurrenceRuleId: id };
      }
    }

    return null;
  }


/*
  * Loads the RecurrenceRule data for the current recurrenceRuleId.
  *
  * Fully respects the RecurrenceRuleService caching strategy and error handling strategy.
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
    if (!this.recurrenceRuleService.userIsSchedulerRecurrenceRuleReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read RecurrenceRules.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate recurrenceRuleId
    //
    if (!this.recurrenceRuleId) {

      this.alertService.showMessage('No RecurrenceRule ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const recurrenceRuleId = Number(this.recurrenceRuleId);

    if (isNaN(recurrenceRuleId) || recurrenceRuleId <= 0) {

      this.alertService.showMessage(`Invalid Recurrence Rule ID: "${this.recurrenceRuleId}"`,
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
      // This is the most targeted way: clear only this RecurrenceRule + relations

      this.recurrenceRuleService.ClearRecordCache(recurrenceRuleId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.recurrenceRuleService.GetRecurrenceRule(recurrenceRuleId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (recurrenceRuleData) => {

        //
        // Success path — recurrenceRuleData can legitimately be null if 404'd but request succeeded
        //
        if (!recurrenceRuleData) {

          this.handleRecurrenceRuleNotFound(recurrenceRuleId);

        } else {

          this.recurrenceRuleData = recurrenceRuleData;
          this.buildFormValues(this.recurrenceRuleData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'RecurrenceRule loaded successfully',
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
        this.handleRecurrenceRuleLoadError(error, recurrenceRuleId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleRecurrenceRuleNotFound(recurrenceRuleId: number): void {

    this.recurrenceRuleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `RecurrenceRule #${recurrenceRuleId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleRecurrenceRuleLoadError(error: any, recurrenceRuleId: number): void {

    let message = 'Failed to load Recurrence Rule.';
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
          message = 'You do not have permission to view this Recurrence Rule.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Recurrence Rule #${recurrenceRuleId} was not found.`;
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

    console.error(`Recurrence Rule load failed (ID: ${recurrenceRuleId})`, error);

    //
    // Reset UI to safe state
    //
    this.recurrenceRuleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(recurrenceRuleData: RecurrenceRuleData | null) {

    if (recurrenceRuleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.recurrenceRuleForm.reset({
        recurrenceFrequencyId: null,
        interval: '',
        untilDateTime: '',
        count: '',
        dayOfWeekMask: '',
        dayOfMonth: '',
        dayOfWeekInMonth: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.recurrenceRuleForm.reset({
        recurrenceFrequencyId: recurrenceRuleData.recurrenceFrequencyId,
        interval: recurrenceRuleData.interval?.toString() ?? '',
        untilDateTime: isoUtcStringToDateTimeLocal(recurrenceRuleData.untilDateTime) ?? '',
        count: recurrenceRuleData.count?.toString() ?? '',
        dayOfWeekMask: recurrenceRuleData.dayOfWeekMask?.toString() ?? '',
        dayOfMonth: recurrenceRuleData.dayOfMonth?.toString() ?? '',
        dayOfWeekInMonth: recurrenceRuleData.dayOfWeekInMonth?.toString() ?? '',
        versionNumber: recurrenceRuleData.versionNumber?.toString() ?? '',
        active: recurrenceRuleData.active ?? true,
        deleted: recurrenceRuleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.recurrenceRuleForm.markAsPristine();
    this.recurrenceRuleForm.markAsUntouched();
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

    if (this.recurrenceRuleService.userIsSchedulerRecurrenceRuleWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Recurrence Rules", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.recurrenceRuleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.recurrenceRuleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.recurrenceRuleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const recurrenceRuleSubmitData: RecurrenceRuleSubmitData = {
        id: this.recurrenceRuleData?.id || 0,
        recurrenceFrequencyId: Number(formValue.recurrenceFrequencyId),
        interval: Number(formValue.interval),
        untilDateTime: formValue.untilDateTime ? dateTimeLocalToIsoUtc(formValue.untilDateTime.trim()) : null,
        count: formValue.count ? Number(formValue.count) : null,
        dayOfWeekMask: formValue.dayOfWeekMask ? Number(formValue.dayOfWeekMask) : null,
        dayOfMonth: formValue.dayOfMonth ? Number(formValue.dayOfMonth) : null,
        dayOfWeekInMonth: formValue.dayOfWeekInMonth ? Number(formValue.dayOfWeekInMonth) : null,
        versionNumber: this.recurrenceRuleData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.recurrenceRuleService.PutRecurrenceRule(recurrenceRuleSubmitData.id, recurrenceRuleSubmitData)
      : this.recurrenceRuleService.PostRecurrenceRule(recurrenceRuleSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedRecurrenceRuleData) => {

        this.recurrenceRuleService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Recurrence Rule's detail page
          //
          this.recurrenceRuleForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.recurrenceRuleForm.markAsUntouched();

          this.router.navigate(['/recurrencerules', savedRecurrenceRuleData.id]);
          this.alertService.showMessage('Recurrence Rule added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.recurrenceRuleData = savedRecurrenceRuleData;
          this.buildFormValues(this.recurrenceRuleData);

          this.alertService.showMessage("Recurrence Rule saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Recurrence Rule.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Recurrence Rule.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Recurrence Rule could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerRecurrenceRuleReader(): boolean {
    return this.recurrenceRuleService.userIsSchedulerRecurrenceRuleReader();
  }

  public userIsSchedulerRecurrenceRuleWriter(): boolean {
    return this.recurrenceRuleService.userIsSchedulerRecurrenceRuleWriter();
  }
}
