/*
   GENERATED FORM FOR THE SCHEDULINGTARGETCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetChangeHistoryService, SchedulingTargetChangeHistoryData, SchedulingTargetChangeHistorySubmitData } from '../../../scheduler-data-services/scheduling-target-change-history.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
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
interface SchedulingTargetChangeHistoryFormValues {
  schedulingTargetId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-scheduling-target-change-history-detail',
  templateUrl: './scheduling-target-change-history-detail.component.html',
  styleUrls: ['./scheduling-target-change-history-detail.component.scss']
})

export class SchedulingTargetChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetChangeHistoryForm: FormGroup = this.fb.group({
        schedulingTargetId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public schedulingTargetChangeHistoryId: string | null = null;
  public schedulingTargetChangeHistoryData: SchedulingTargetChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  schedulingTargetChangeHistories$ = this.schedulingTargetChangeHistoryService.GetSchedulingTargetChangeHistoryList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();

  private destroy$ = new Subject<void>();

  constructor(
    public schedulingTargetChangeHistoryService: SchedulingTargetChangeHistoryService,
    public schedulingTargetService: SchedulingTargetService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the schedulingTargetChangeHistoryId from the route parameters
    this.schedulingTargetChangeHistoryId = this.route.snapshot.paramMap.get('schedulingTargetChangeHistoryId');

    if (this.schedulingTargetChangeHistoryId === 'new' ||
        this.schedulingTargetChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.schedulingTargetChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.schedulingTargetChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduling Target Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduling Target Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.schedulingTargetChangeHistoryForm.dirty) {
      return confirm('You have unsaved Scheduling Target Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.schedulingTargetChangeHistoryId != null && this.schedulingTargetChangeHistoryId !== 'new') {

      const id = parseInt(this.schedulingTargetChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { schedulingTargetChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the SchedulingTargetChangeHistory data for the current schedulingTargetChangeHistoryId.
  *
  * Fully respects the SchedulingTargetChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.schedulingTargetChangeHistoryService.userIsSchedulerSchedulingTargetChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SchedulingTargetChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate schedulingTargetChangeHistoryId
    //
    if (!this.schedulingTargetChangeHistoryId) {

      this.alertService.showMessage('No SchedulingTargetChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const schedulingTargetChangeHistoryId = Number(this.schedulingTargetChangeHistoryId);

    if (isNaN(schedulingTargetChangeHistoryId) || schedulingTargetChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Scheduling Target Change History ID: "${this.schedulingTargetChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this SchedulingTargetChangeHistory + relations

      this.schedulingTargetChangeHistoryService.ClearRecordCache(schedulingTargetChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.schedulingTargetChangeHistoryService.GetSchedulingTargetChangeHistory(schedulingTargetChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (schedulingTargetChangeHistoryData) => {

        //
        // Success path — schedulingTargetChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!schedulingTargetChangeHistoryData) {

          this.handleSchedulingTargetChangeHistoryNotFound(schedulingTargetChangeHistoryId);

        } else {

          this.schedulingTargetChangeHistoryData = schedulingTargetChangeHistoryData;
          this.buildFormValues(this.schedulingTargetChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SchedulingTargetChangeHistory loaded successfully',
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
        this.handleSchedulingTargetChangeHistoryLoadError(error, schedulingTargetChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSchedulingTargetChangeHistoryNotFound(schedulingTargetChangeHistoryId: number): void {

    this.schedulingTargetChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SchedulingTargetChangeHistory #${schedulingTargetChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSchedulingTargetChangeHistoryLoadError(error: any, schedulingTargetChangeHistoryId: number): void {

    let message = 'Failed to load Scheduling Target Change History.';
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
          message = 'You do not have permission to view this Scheduling Target Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduling Target Change History #${schedulingTargetChangeHistoryId} was not found.`;
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

    console.error(`Scheduling Target Change History load failed (ID: ${schedulingTargetChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.schedulingTargetChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(schedulingTargetChangeHistoryData: SchedulingTargetChangeHistoryData | null) {

    if (schedulingTargetChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetChangeHistoryForm.reset({
        schedulingTargetId: null,
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
        this.schedulingTargetChangeHistoryForm.reset({
        schedulingTargetId: schedulingTargetChangeHistoryData.schedulingTargetId,
        versionNumber: schedulingTargetChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(schedulingTargetChangeHistoryData.timeStamp) ?? '',
        userId: schedulingTargetChangeHistoryData.userId?.toString() ?? '',
        data: schedulingTargetChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.schedulingTargetChangeHistoryForm.markAsPristine();
    this.schedulingTargetChangeHistoryForm.markAsUntouched();
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

    if (this.schedulingTargetChangeHistoryService.userIsSchedulerSchedulingTargetChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduling Target Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.schedulingTargetChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetChangeHistorySubmitData: SchedulingTargetChangeHistorySubmitData = {
        id: this.schedulingTargetChangeHistoryData?.id || 0,
        schedulingTargetId: Number(formValue.schedulingTargetId),
        versionNumber: this.schedulingTargetChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.schedulingTargetChangeHistoryService.PutSchedulingTargetChangeHistory(schedulingTargetChangeHistorySubmitData.id, schedulingTargetChangeHistorySubmitData)
      : this.schedulingTargetChangeHistoryService.PostSchedulingTargetChangeHistory(schedulingTargetChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSchedulingTargetChangeHistoryData) => {

        this.schedulingTargetChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduling Target Change History's detail page
          //
          this.schedulingTargetChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.schedulingTargetChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/schedulingtargetchangehistories', savedSchedulingTargetChangeHistoryData.id]);
          this.alertService.showMessage('Scheduling Target Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.schedulingTargetChangeHistoryData = savedSchedulingTargetChangeHistoryData;
          this.buildFormValues(this.schedulingTargetChangeHistoryData);

          this.alertService.showMessage("Scheduling Target Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduling Target Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerSchedulingTargetChangeHistoryReader(): boolean {
    return this.schedulingTargetChangeHistoryService.userIsSchedulerSchedulingTargetChangeHistoryReader();
  }

  public userIsSchedulerSchedulingTargetChangeHistoryWriter(): boolean {
    return this.schedulingTargetChangeHistoryService.userIsSchedulerSchedulingTargetChangeHistoryWriter();
  }
}
