/*
   GENERATED FORM FOR THE RECURRENCEFREQUENCY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RecurrenceFrequency table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to recurrence-frequency-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RecurrenceFrequencyService, RecurrenceFrequencyData, RecurrenceFrequencySubmitData } from '../../../scheduler-data-services/recurrence-frequency.service';
import { RecurrenceRuleService } from '../../../scheduler-data-services/recurrence-rule.service';
import { PledgeService } from '../../../scheduler-data-services/pledge.service';
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
interface RecurrenceFrequencyFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-recurrence-frequency-detail',
  templateUrl: './recurrence-frequency-detail.component.html',
  styleUrls: ['./recurrence-frequency-detail.component.scss']
})

export class RecurrenceFrequencyDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RecurrenceFrequencyFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public recurrenceFrequencyForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public recurrenceFrequencyId: string | null = null;
  public recurrenceFrequencyData: RecurrenceFrequencyData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  recurrenceFrequencies$ = this.recurrenceFrequencyService.GetRecurrenceFrequencyList();
  public recurrenceRules$ = this.recurrenceRuleService.GetRecurrenceRuleList();
  public pledges$ = this.pledgeService.GetPledgeList();

  private destroy$ = new Subject<void>();

  constructor(
    public recurrenceFrequencyService: RecurrenceFrequencyService,
    public recurrenceRuleService: RecurrenceRuleService,
    public pledgeService: PledgeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the recurrenceFrequencyId from the route parameters
    this.recurrenceFrequencyId = this.route.snapshot.paramMap.get('recurrenceFrequencyId');

    if (this.recurrenceFrequencyId === 'new' ||
        this.recurrenceFrequencyId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.recurrenceFrequencyData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.recurrenceFrequencyForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.recurrenceFrequencyForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Recurrence Frequency';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Recurrence Frequency';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.recurrenceFrequencyForm.dirty) {
      return confirm('You have unsaved Recurrence Frequency changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.recurrenceFrequencyId != null && this.recurrenceFrequencyId !== 'new') {

      const id = parseInt(this.recurrenceFrequencyId, 10);

      if (!isNaN(id)) {
        return { recurrenceFrequencyId: id };
      }
    }

    return null;
  }


/*
  * Loads the RecurrenceFrequency data for the current recurrenceFrequencyId.
  *
  * Fully respects the RecurrenceFrequencyService caching strategy and error handling strategy.
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
    if (!this.recurrenceFrequencyService.userIsSchedulerRecurrenceFrequencyReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read RecurrenceFrequencies.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate recurrenceFrequencyId
    //
    if (!this.recurrenceFrequencyId) {

      this.alertService.showMessage('No RecurrenceFrequency ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const recurrenceFrequencyId = Number(this.recurrenceFrequencyId);

    if (isNaN(recurrenceFrequencyId) || recurrenceFrequencyId <= 0) {

      this.alertService.showMessage(`Invalid Recurrence Frequency ID: "${this.recurrenceFrequencyId}"`,
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
      // This is the most targeted way: clear only this RecurrenceFrequency + relations

      this.recurrenceFrequencyService.ClearRecordCache(recurrenceFrequencyId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.recurrenceFrequencyService.GetRecurrenceFrequency(recurrenceFrequencyId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (recurrenceFrequencyData) => {

        //
        // Success path — recurrenceFrequencyData can legitimately be null if 404'd but request succeeded
        //
        if (!recurrenceFrequencyData) {

          this.handleRecurrenceFrequencyNotFound(recurrenceFrequencyId);

        } else {

          this.recurrenceFrequencyData = recurrenceFrequencyData;
          this.buildFormValues(this.recurrenceFrequencyData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'RecurrenceFrequency loaded successfully',
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
        this.handleRecurrenceFrequencyLoadError(error, recurrenceFrequencyId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleRecurrenceFrequencyNotFound(recurrenceFrequencyId: number): void {

    this.recurrenceFrequencyData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `RecurrenceFrequency #${recurrenceFrequencyId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleRecurrenceFrequencyLoadError(error: any, recurrenceFrequencyId: number): void {

    let message = 'Failed to load Recurrence Frequency.';
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
          message = 'You do not have permission to view this Recurrence Frequency.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Recurrence Frequency #${recurrenceFrequencyId} was not found.`;
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

    console.error(`Recurrence Frequency load failed (ID: ${recurrenceFrequencyId})`, error);

    //
    // Reset UI to safe state
    //
    this.recurrenceFrequencyData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(recurrenceFrequencyData: RecurrenceFrequencyData | null) {

    if (recurrenceFrequencyData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.recurrenceFrequencyForm.reset({
        name: '',
        description: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.recurrenceFrequencyForm.reset({
        name: recurrenceFrequencyData.name ?? '',
        description: recurrenceFrequencyData.description ?? '',
        sequence: recurrenceFrequencyData.sequence?.toString() ?? '',
        active: recurrenceFrequencyData.active ?? true,
        deleted: recurrenceFrequencyData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.recurrenceFrequencyForm.markAsPristine();
    this.recurrenceFrequencyForm.markAsUntouched();
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

    if (this.recurrenceFrequencyService.userIsSchedulerRecurrenceFrequencyWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Recurrence Frequencies", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.recurrenceFrequencyForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.recurrenceFrequencyForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.recurrenceFrequencyForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const recurrenceFrequencySubmitData: RecurrenceFrequencySubmitData = {
        id: this.recurrenceFrequencyData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.recurrenceFrequencyService.PutRecurrenceFrequency(recurrenceFrequencySubmitData.id, recurrenceFrequencySubmitData)
      : this.recurrenceFrequencyService.PostRecurrenceFrequency(recurrenceFrequencySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedRecurrenceFrequencyData) => {

        this.recurrenceFrequencyService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Recurrence Frequency's detail page
          //
          this.recurrenceFrequencyForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.recurrenceFrequencyForm.markAsUntouched();

          this.router.navigate(['/recurrencefrequencies', savedRecurrenceFrequencyData.id]);
          this.alertService.showMessage('Recurrence Frequency added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.recurrenceFrequencyData = savedRecurrenceFrequencyData;
          this.buildFormValues(this.recurrenceFrequencyData);

          this.alertService.showMessage("Recurrence Frequency saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Recurrence Frequency.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Recurrence Frequency.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Recurrence Frequency could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerRecurrenceFrequencyReader(): boolean {
    return this.recurrenceFrequencyService.userIsSchedulerRecurrenceFrequencyReader();
  }

  public userIsSchedulerRecurrenceFrequencyWriter(): boolean {
    return this.recurrenceFrequencyService.userIsSchedulerRecurrenceFrequencyWriter();
  }
}
