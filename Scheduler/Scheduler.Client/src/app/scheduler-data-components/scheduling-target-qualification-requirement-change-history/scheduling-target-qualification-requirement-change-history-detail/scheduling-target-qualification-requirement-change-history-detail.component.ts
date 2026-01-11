/*
   GENERATED FORM FOR THE SCHEDULINGTARGETQUALIFICATIONREQUIREMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetQualificationRequirementChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-qualification-requirement-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetQualificationRequirementChangeHistoryService, SchedulingTargetQualificationRequirementChangeHistoryData, SchedulingTargetQualificationRequirementChangeHistorySubmitData } from '../../../scheduler-data-services/scheduling-target-qualification-requirement-change-history.service';
import { SchedulingTargetQualificationRequirementService } from '../../../scheduler-data-services/scheduling-target-qualification-requirement.service';
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
interface SchedulingTargetQualificationRequirementChangeHistoryFormValues {
  schedulingTargetQualificationRequirementId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-scheduling-target-qualification-requirement-change-history-detail',
  templateUrl: './scheduling-target-qualification-requirement-change-history-detail.component.html',
  styleUrls: ['./scheduling-target-qualification-requirement-change-history-detail.component.scss']
})

export class SchedulingTargetQualificationRequirementChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetQualificationRequirementChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetQualificationRequirementChangeHistoryForm: FormGroup = this.fb.group({
        schedulingTargetQualificationRequirementId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public schedulingTargetQualificationRequirementChangeHistoryId: string | null = null;
  public schedulingTargetQualificationRequirementChangeHistoryData: SchedulingTargetQualificationRequirementChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  schedulingTargetQualificationRequirementChangeHistories$ = this.schedulingTargetQualificationRequirementChangeHistoryService.GetSchedulingTargetQualificationRequirementChangeHistoryList();
  public schedulingTargetQualificationRequirements$ = this.schedulingTargetQualificationRequirementService.GetSchedulingTargetQualificationRequirementList();

  private destroy$ = new Subject<void>();

  constructor(
    public schedulingTargetQualificationRequirementChangeHistoryService: SchedulingTargetQualificationRequirementChangeHistoryService,
    public schedulingTargetQualificationRequirementService: SchedulingTargetQualificationRequirementService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the schedulingTargetQualificationRequirementChangeHistoryId from the route parameters
    this.schedulingTargetQualificationRequirementChangeHistoryId = this.route.snapshot.paramMap.get('schedulingTargetQualificationRequirementChangeHistoryId');

    if (this.schedulingTargetQualificationRequirementChangeHistoryId === 'new' ||
        this.schedulingTargetQualificationRequirementChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.schedulingTargetQualificationRequirementChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.schedulingTargetQualificationRequirementChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetQualificationRequirementChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduling Target Qualification Requirement Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduling Target Qualification Requirement Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.schedulingTargetQualificationRequirementChangeHistoryForm.dirty) {
      return confirm('You have unsaved Scheduling Target Qualification Requirement Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.schedulingTargetQualificationRequirementChangeHistoryId != null && this.schedulingTargetQualificationRequirementChangeHistoryId !== 'new') {

      const id = parseInt(this.schedulingTargetQualificationRequirementChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { schedulingTargetQualificationRequirementChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the SchedulingTargetQualificationRequirementChangeHistory data for the current schedulingTargetQualificationRequirementChangeHistoryId.
  *
  * Fully respects the SchedulingTargetQualificationRequirementChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.schedulingTargetQualificationRequirementChangeHistoryService.userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SchedulingTargetQualificationRequirementChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate schedulingTargetQualificationRequirementChangeHistoryId
    //
    if (!this.schedulingTargetQualificationRequirementChangeHistoryId) {

      this.alertService.showMessage('No SchedulingTargetQualificationRequirementChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const schedulingTargetQualificationRequirementChangeHistoryId = Number(this.schedulingTargetQualificationRequirementChangeHistoryId);

    if (isNaN(schedulingTargetQualificationRequirementChangeHistoryId) || schedulingTargetQualificationRequirementChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Scheduling Target Qualification Requirement Change History ID: "${this.schedulingTargetQualificationRequirementChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this SchedulingTargetQualificationRequirementChangeHistory + relations

      this.schedulingTargetQualificationRequirementChangeHistoryService.ClearRecordCache(schedulingTargetQualificationRequirementChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.schedulingTargetQualificationRequirementChangeHistoryService.GetSchedulingTargetQualificationRequirementChangeHistory(schedulingTargetQualificationRequirementChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (schedulingTargetQualificationRequirementChangeHistoryData) => {

        //
        // Success path — schedulingTargetQualificationRequirementChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!schedulingTargetQualificationRequirementChangeHistoryData) {

          this.handleSchedulingTargetQualificationRequirementChangeHistoryNotFound(schedulingTargetQualificationRequirementChangeHistoryId);

        } else {

          this.schedulingTargetQualificationRequirementChangeHistoryData = schedulingTargetQualificationRequirementChangeHistoryData;
          this.buildFormValues(this.schedulingTargetQualificationRequirementChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SchedulingTargetQualificationRequirementChangeHistory loaded successfully',
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
        this.handleSchedulingTargetQualificationRequirementChangeHistoryLoadError(error, schedulingTargetQualificationRequirementChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSchedulingTargetQualificationRequirementChangeHistoryNotFound(schedulingTargetQualificationRequirementChangeHistoryId: number): void {

    this.schedulingTargetQualificationRequirementChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SchedulingTargetQualificationRequirementChangeHistory #${schedulingTargetQualificationRequirementChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSchedulingTargetQualificationRequirementChangeHistoryLoadError(error: any, schedulingTargetQualificationRequirementChangeHistoryId: number): void {

    let message = 'Failed to load Scheduling Target Qualification Requirement Change History.';
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
          message = 'You do not have permission to view this Scheduling Target Qualification Requirement Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduling Target Qualification Requirement Change History #${schedulingTargetQualificationRequirementChangeHistoryId} was not found.`;
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

    console.error(`Scheduling Target Qualification Requirement Change History load failed (ID: ${schedulingTargetQualificationRequirementChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.schedulingTargetQualificationRequirementChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(schedulingTargetQualificationRequirementChangeHistoryData: SchedulingTargetQualificationRequirementChangeHistoryData | null) {

    if (schedulingTargetQualificationRequirementChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetQualificationRequirementChangeHistoryForm.reset({
        schedulingTargetQualificationRequirementId: null,
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
        this.schedulingTargetQualificationRequirementChangeHistoryForm.reset({
        schedulingTargetQualificationRequirementId: schedulingTargetQualificationRequirementChangeHistoryData.schedulingTargetQualificationRequirementId,
        versionNumber: schedulingTargetQualificationRequirementChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(schedulingTargetQualificationRequirementChangeHistoryData.timeStamp) ?? '',
        userId: schedulingTargetQualificationRequirementChangeHistoryData.userId?.toString() ?? '',
        data: schedulingTargetQualificationRequirementChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.schedulingTargetQualificationRequirementChangeHistoryForm.markAsPristine();
    this.schedulingTargetQualificationRequirementChangeHistoryForm.markAsUntouched();
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

    if (this.schedulingTargetQualificationRequirementChangeHistoryService.userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduling Target Qualification Requirement Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.schedulingTargetQualificationRequirementChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetQualificationRequirementChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetQualificationRequirementChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetQualificationRequirementChangeHistorySubmitData: SchedulingTargetQualificationRequirementChangeHistorySubmitData = {
        id: this.schedulingTargetQualificationRequirementChangeHistoryData?.id || 0,
        schedulingTargetQualificationRequirementId: Number(formValue.schedulingTargetQualificationRequirementId),
        versionNumber: this.schedulingTargetQualificationRequirementChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.schedulingTargetQualificationRequirementChangeHistoryService.PutSchedulingTargetQualificationRequirementChangeHistory(schedulingTargetQualificationRequirementChangeHistorySubmitData.id, schedulingTargetQualificationRequirementChangeHistorySubmitData)
      : this.schedulingTargetQualificationRequirementChangeHistoryService.PostSchedulingTargetQualificationRequirementChangeHistory(schedulingTargetQualificationRequirementChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSchedulingTargetQualificationRequirementChangeHistoryData) => {

        this.schedulingTargetQualificationRequirementChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduling Target Qualification Requirement Change History's detail page
          //
          this.schedulingTargetQualificationRequirementChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.schedulingTargetQualificationRequirementChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/schedulingtargetqualificationrequirementchangehistories', savedSchedulingTargetQualificationRequirementChangeHistoryData.id]);
          this.alertService.showMessage('Scheduling Target Qualification Requirement Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.schedulingTargetQualificationRequirementChangeHistoryData = savedSchedulingTargetQualificationRequirementChangeHistoryData;
          this.buildFormValues(this.schedulingTargetQualificationRequirementChangeHistoryData);

          this.alertService.showMessage("Scheduling Target Qualification Requirement Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduling Target Qualification Requirement Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Qualification Requirement Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Qualification Requirement Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader(): boolean {
    return this.schedulingTargetQualificationRequirementChangeHistoryService.userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader();
  }

  public userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter(): boolean {
    return this.schedulingTargetQualificationRequirementChangeHistoryService.userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter();
  }
}
