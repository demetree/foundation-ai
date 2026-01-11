/*
   GENERATED FORM FOR THE RECURRENCEEXCEPTIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RecurrenceExceptionChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to recurrence-exception-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RecurrenceExceptionChangeHistoryService, RecurrenceExceptionChangeHistoryData, RecurrenceExceptionChangeHistorySubmitData } from '../../../scheduler-data-services/recurrence-exception-change-history.service';
import { RecurrenceExceptionService } from '../../../scheduler-data-services/recurrence-exception.service';
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
interface RecurrenceExceptionChangeHistoryFormValues {
  recurrenceExceptionId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-recurrence-exception-change-history-detail',
  templateUrl: './recurrence-exception-change-history-detail.component.html',
  styleUrls: ['./recurrence-exception-change-history-detail.component.scss']
})

export class RecurrenceExceptionChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RecurrenceExceptionChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public recurrenceExceptionChangeHistoryForm: FormGroup = this.fb.group({
        recurrenceExceptionId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public recurrenceExceptionChangeHistoryId: string | null = null;
  public recurrenceExceptionChangeHistoryData: RecurrenceExceptionChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  recurrenceExceptionChangeHistories$ = this.recurrenceExceptionChangeHistoryService.GetRecurrenceExceptionChangeHistoryList();
  public recurrenceExceptions$ = this.recurrenceExceptionService.GetRecurrenceExceptionList();

  private destroy$ = new Subject<void>();

  constructor(
    public recurrenceExceptionChangeHistoryService: RecurrenceExceptionChangeHistoryService,
    public recurrenceExceptionService: RecurrenceExceptionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the recurrenceExceptionChangeHistoryId from the route parameters
    this.recurrenceExceptionChangeHistoryId = this.route.snapshot.paramMap.get('recurrenceExceptionChangeHistoryId');

    if (this.recurrenceExceptionChangeHistoryId === 'new' ||
        this.recurrenceExceptionChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.recurrenceExceptionChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.recurrenceExceptionChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.recurrenceExceptionChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Recurrence Exception Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Recurrence Exception Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.recurrenceExceptionChangeHistoryForm.dirty) {
      return confirm('You have unsaved Recurrence Exception Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.recurrenceExceptionChangeHistoryId != null && this.recurrenceExceptionChangeHistoryId !== 'new') {

      const id = parseInt(this.recurrenceExceptionChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { recurrenceExceptionChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the RecurrenceExceptionChangeHistory data for the current recurrenceExceptionChangeHistoryId.
  *
  * Fully respects the RecurrenceExceptionChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.recurrenceExceptionChangeHistoryService.userIsSchedulerRecurrenceExceptionChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read RecurrenceExceptionChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate recurrenceExceptionChangeHistoryId
    //
    if (!this.recurrenceExceptionChangeHistoryId) {

      this.alertService.showMessage('No RecurrenceExceptionChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const recurrenceExceptionChangeHistoryId = Number(this.recurrenceExceptionChangeHistoryId);

    if (isNaN(recurrenceExceptionChangeHistoryId) || recurrenceExceptionChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Recurrence Exception Change History ID: "${this.recurrenceExceptionChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this RecurrenceExceptionChangeHistory + relations

      this.recurrenceExceptionChangeHistoryService.ClearRecordCache(recurrenceExceptionChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.recurrenceExceptionChangeHistoryService.GetRecurrenceExceptionChangeHistory(recurrenceExceptionChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (recurrenceExceptionChangeHistoryData) => {

        //
        // Success path — recurrenceExceptionChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!recurrenceExceptionChangeHistoryData) {

          this.handleRecurrenceExceptionChangeHistoryNotFound(recurrenceExceptionChangeHistoryId);

        } else {

          this.recurrenceExceptionChangeHistoryData = recurrenceExceptionChangeHistoryData;
          this.buildFormValues(this.recurrenceExceptionChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'RecurrenceExceptionChangeHistory loaded successfully',
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
        this.handleRecurrenceExceptionChangeHistoryLoadError(error, recurrenceExceptionChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleRecurrenceExceptionChangeHistoryNotFound(recurrenceExceptionChangeHistoryId: number): void {

    this.recurrenceExceptionChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `RecurrenceExceptionChangeHistory #${recurrenceExceptionChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleRecurrenceExceptionChangeHistoryLoadError(error: any, recurrenceExceptionChangeHistoryId: number): void {

    let message = 'Failed to load Recurrence Exception Change History.';
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
          message = 'You do not have permission to view this Recurrence Exception Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Recurrence Exception Change History #${recurrenceExceptionChangeHistoryId} was not found.`;
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

    console.error(`Recurrence Exception Change History load failed (ID: ${recurrenceExceptionChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.recurrenceExceptionChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(recurrenceExceptionChangeHistoryData: RecurrenceExceptionChangeHistoryData | null) {

    if (recurrenceExceptionChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.recurrenceExceptionChangeHistoryForm.reset({
        recurrenceExceptionId: null,
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
        this.recurrenceExceptionChangeHistoryForm.reset({
        recurrenceExceptionId: recurrenceExceptionChangeHistoryData.recurrenceExceptionId,
        versionNumber: recurrenceExceptionChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(recurrenceExceptionChangeHistoryData.timeStamp) ?? '',
        userId: recurrenceExceptionChangeHistoryData.userId?.toString() ?? '',
        data: recurrenceExceptionChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.recurrenceExceptionChangeHistoryForm.markAsPristine();
    this.recurrenceExceptionChangeHistoryForm.markAsUntouched();
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

    if (this.recurrenceExceptionChangeHistoryService.userIsSchedulerRecurrenceExceptionChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Recurrence Exception Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.recurrenceExceptionChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.recurrenceExceptionChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.recurrenceExceptionChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const recurrenceExceptionChangeHistorySubmitData: RecurrenceExceptionChangeHistorySubmitData = {
        id: this.recurrenceExceptionChangeHistoryData?.id || 0,
        recurrenceExceptionId: Number(formValue.recurrenceExceptionId),
        versionNumber: this.recurrenceExceptionChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.recurrenceExceptionChangeHistoryService.PutRecurrenceExceptionChangeHistory(recurrenceExceptionChangeHistorySubmitData.id, recurrenceExceptionChangeHistorySubmitData)
      : this.recurrenceExceptionChangeHistoryService.PostRecurrenceExceptionChangeHistory(recurrenceExceptionChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedRecurrenceExceptionChangeHistoryData) => {

        this.recurrenceExceptionChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Recurrence Exception Change History's detail page
          //
          this.recurrenceExceptionChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.recurrenceExceptionChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/recurrenceexceptionchangehistories', savedRecurrenceExceptionChangeHistoryData.id]);
          this.alertService.showMessage('Recurrence Exception Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.recurrenceExceptionChangeHistoryData = savedRecurrenceExceptionChangeHistoryData;
          this.buildFormValues(this.recurrenceExceptionChangeHistoryData);

          this.alertService.showMessage("Recurrence Exception Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Recurrence Exception Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Recurrence Exception Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Recurrence Exception Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerRecurrenceExceptionChangeHistoryReader(): boolean {
    return this.recurrenceExceptionChangeHistoryService.userIsSchedulerRecurrenceExceptionChangeHistoryReader();
  }

  public userIsSchedulerRecurrenceExceptionChangeHistoryWriter(): boolean {
    return this.recurrenceExceptionChangeHistoryService.userIsSchedulerRecurrenceExceptionChangeHistoryWriter();
  }
}
