/*
   GENERATED FORM FOR THE ONCALLSCHEDULECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from OnCallScheduleChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to on-call-schedule-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OnCallScheduleChangeHistoryService, OnCallScheduleChangeHistoryData, OnCallScheduleChangeHistorySubmitData } from '../../../alerting-data-services/on-call-schedule-change-history.service';
import { OnCallScheduleService } from '../../../alerting-data-services/on-call-schedule.service';
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
interface OnCallScheduleChangeHistoryFormValues {
  onCallScheduleId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-on-call-schedule-change-history-detail',
  templateUrl: './on-call-schedule-change-history-detail.component.html',
  styleUrls: ['./on-call-schedule-change-history-detail.component.scss']
})

export class OnCallScheduleChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<OnCallScheduleChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public onCallScheduleChangeHistoryForm: FormGroup = this.fb.group({
        onCallScheduleId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public onCallScheduleChangeHistoryId: string | null = null;
  public onCallScheduleChangeHistoryData: OnCallScheduleChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  onCallScheduleChangeHistories$ = this.onCallScheduleChangeHistoryService.GetOnCallScheduleChangeHistoryList();
  public onCallSchedules$ = this.onCallScheduleService.GetOnCallScheduleList();

  private destroy$ = new Subject<void>();

  constructor(
    public onCallScheduleChangeHistoryService: OnCallScheduleChangeHistoryService,
    public onCallScheduleService: OnCallScheduleService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the onCallScheduleChangeHistoryId from the route parameters
    this.onCallScheduleChangeHistoryId = this.route.snapshot.paramMap.get('onCallScheduleChangeHistoryId');

    if (this.onCallScheduleChangeHistoryId === 'new' ||
        this.onCallScheduleChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.onCallScheduleChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.onCallScheduleChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.onCallScheduleChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New On Call Schedule Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit On Call Schedule Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.onCallScheduleChangeHistoryForm.dirty) {
      return confirm('You have unsaved On Call Schedule Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.onCallScheduleChangeHistoryId != null && this.onCallScheduleChangeHistoryId !== 'new') {

      const id = parseInt(this.onCallScheduleChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { onCallScheduleChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the OnCallScheduleChangeHistory data for the current onCallScheduleChangeHistoryId.
  *
  * Fully respects the OnCallScheduleChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.onCallScheduleChangeHistoryService.userIsAlertingOnCallScheduleChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read OnCallScheduleChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate onCallScheduleChangeHistoryId
    //
    if (!this.onCallScheduleChangeHistoryId) {

      this.alertService.showMessage('No OnCallScheduleChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const onCallScheduleChangeHistoryId = Number(this.onCallScheduleChangeHistoryId);

    if (isNaN(onCallScheduleChangeHistoryId) || onCallScheduleChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid On Call Schedule Change History ID: "${this.onCallScheduleChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this OnCallScheduleChangeHistory + relations

      this.onCallScheduleChangeHistoryService.ClearRecordCache(onCallScheduleChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.onCallScheduleChangeHistoryService.GetOnCallScheduleChangeHistory(onCallScheduleChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (onCallScheduleChangeHistoryData) => {

        //
        // Success path — onCallScheduleChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!onCallScheduleChangeHistoryData) {

          this.handleOnCallScheduleChangeHistoryNotFound(onCallScheduleChangeHistoryId);

        } else {

          this.onCallScheduleChangeHistoryData = onCallScheduleChangeHistoryData;
          this.buildFormValues(this.onCallScheduleChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'OnCallScheduleChangeHistory loaded successfully',
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
        this.handleOnCallScheduleChangeHistoryLoadError(error, onCallScheduleChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleOnCallScheduleChangeHistoryNotFound(onCallScheduleChangeHistoryId: number): void {

    this.onCallScheduleChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `OnCallScheduleChangeHistory #${onCallScheduleChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleOnCallScheduleChangeHistoryLoadError(error: any, onCallScheduleChangeHistoryId: number): void {

    let message = 'Failed to load On Call Schedule Change History.';
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
          message = 'You do not have permission to view this On Call Schedule Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `On Call Schedule Change History #${onCallScheduleChangeHistoryId} was not found.`;
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

    console.error(`On Call Schedule Change History load failed (ID: ${onCallScheduleChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.onCallScheduleChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(onCallScheduleChangeHistoryData: OnCallScheduleChangeHistoryData | null) {

    if (onCallScheduleChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.onCallScheduleChangeHistoryForm.reset({
        onCallScheduleId: null,
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
        this.onCallScheduleChangeHistoryForm.reset({
        onCallScheduleId: onCallScheduleChangeHistoryData.onCallScheduleId,
        versionNumber: onCallScheduleChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(onCallScheduleChangeHistoryData.timeStamp) ?? '',
        userId: onCallScheduleChangeHistoryData.userId?.toString() ?? '',
        data: onCallScheduleChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.onCallScheduleChangeHistoryForm.markAsPristine();
    this.onCallScheduleChangeHistoryForm.markAsUntouched();
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

    if (this.onCallScheduleChangeHistoryService.userIsAlertingOnCallScheduleChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to On Call Schedule Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.onCallScheduleChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.onCallScheduleChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.onCallScheduleChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const onCallScheduleChangeHistorySubmitData: OnCallScheduleChangeHistorySubmitData = {
        id: this.onCallScheduleChangeHistoryData?.id || 0,
        onCallScheduleId: Number(formValue.onCallScheduleId),
        versionNumber: this.onCallScheduleChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.onCallScheduleChangeHistoryService.PutOnCallScheduleChangeHistory(onCallScheduleChangeHistorySubmitData.id, onCallScheduleChangeHistorySubmitData)
      : this.onCallScheduleChangeHistoryService.PostOnCallScheduleChangeHistory(onCallScheduleChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedOnCallScheduleChangeHistoryData) => {

        this.onCallScheduleChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created On Call Schedule Change History's detail page
          //
          this.onCallScheduleChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.onCallScheduleChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/oncallschedulechangehistories', savedOnCallScheduleChangeHistoryData.id]);
          this.alertService.showMessage('On Call Schedule Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.onCallScheduleChangeHistoryData = savedOnCallScheduleChangeHistoryData;
          this.buildFormValues(this.onCallScheduleChangeHistoryData);

          this.alertService.showMessage("On Call Schedule Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this On Call Schedule Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the On Call Schedule Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('On Call Schedule Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingOnCallScheduleChangeHistoryReader(): boolean {
    return this.onCallScheduleChangeHistoryService.userIsAlertingOnCallScheduleChangeHistoryReader();
  }

  public userIsAlertingOnCallScheduleChangeHistoryWriter(): boolean {
    return this.onCallScheduleChangeHistoryService.userIsAlertingOnCallScheduleChangeHistoryWriter();
  }
}
