/*
   GENERATED FORM FOR THE SCHEDULEDEVENTQUALIFICATIONREQUIREMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventQualificationRequirement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-qualification-requirement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventQualificationRequirementService, ScheduledEventQualificationRequirementData, ScheduledEventQualificationRequirementSubmitData } from '../../../scheduler-data-services/scheduled-event-qualification-requirement.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { QualificationService } from '../../../scheduler-data-services/qualification.service';
import { ScheduledEventQualificationRequirementChangeHistoryService } from '../../../scheduler-data-services/scheduled-event-qualification-requirement-change-history.service';
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
interface ScheduledEventQualificationRequirementFormValues {
  scheduledEventId: number | bigint,       // For FK link number
  qualificationId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-scheduled-event-qualification-requirement-detail',
  templateUrl: './scheduled-event-qualification-requirement-detail.component.html',
  styleUrls: ['./scheduled-event-qualification-requirement-detail.component.scss']
})

export class ScheduledEventQualificationRequirementDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventQualificationRequirementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventQualificationRequirementForm: FormGroup = this.fb.group({
        scheduledEventId: [null, Validators.required],
        qualificationId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public scheduledEventQualificationRequirementId: string | null = null;
  public scheduledEventQualificationRequirementData: ScheduledEventQualificationRequirementData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  scheduledEventQualificationRequirements$ = this.scheduledEventQualificationRequirementService.GetScheduledEventQualificationRequirementList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public qualifications$ = this.qualificationService.GetQualificationList();
  public scheduledEventQualificationRequirementChangeHistories$ = this.scheduledEventQualificationRequirementChangeHistoryService.GetScheduledEventQualificationRequirementChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public scheduledEventQualificationRequirementService: ScheduledEventQualificationRequirementService,
    public scheduledEventService: ScheduledEventService,
    public qualificationService: QualificationService,
    public scheduledEventQualificationRequirementChangeHistoryService: ScheduledEventQualificationRequirementChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the scheduledEventQualificationRequirementId from the route parameters
    this.scheduledEventQualificationRequirementId = this.route.snapshot.paramMap.get('scheduledEventQualificationRequirementId');

    if (this.scheduledEventQualificationRequirementId === 'new' ||
        this.scheduledEventQualificationRequirementId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.scheduledEventQualificationRequirementData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduledEventQualificationRequirementForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventQualificationRequirementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduled Event Qualification Requirement';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduled Event Qualification Requirement';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.scheduledEventQualificationRequirementForm.dirty) {
      return confirm('You have unsaved Scheduled Event Qualification Requirement changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.scheduledEventQualificationRequirementId != null && this.scheduledEventQualificationRequirementId !== 'new') {

      const id = parseInt(this.scheduledEventQualificationRequirementId, 10);

      if (!isNaN(id)) {
        return { scheduledEventQualificationRequirementId: id };
      }
    }

    return null;
  }


/*
  * Loads the ScheduledEventQualificationRequirement data for the current scheduledEventQualificationRequirementId.
  *
  * Fully respects the ScheduledEventQualificationRequirementService caching strategy and error handling strategy.
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
    if (!this.scheduledEventQualificationRequirementService.userIsSchedulerScheduledEventQualificationRequirementReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ScheduledEventQualificationRequirements.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate scheduledEventQualificationRequirementId
    //
    if (!this.scheduledEventQualificationRequirementId) {

      this.alertService.showMessage('No ScheduledEventQualificationRequirement ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const scheduledEventQualificationRequirementId = Number(this.scheduledEventQualificationRequirementId);

    if (isNaN(scheduledEventQualificationRequirementId) || scheduledEventQualificationRequirementId <= 0) {

      this.alertService.showMessage(`Invalid Scheduled Event Qualification Requirement ID: "${this.scheduledEventQualificationRequirementId}"`,
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
      // This is the most targeted way: clear only this ScheduledEventQualificationRequirement + relations

      this.scheduledEventQualificationRequirementService.ClearRecordCache(scheduledEventQualificationRequirementId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.scheduledEventQualificationRequirementService.GetScheduledEventQualificationRequirement(scheduledEventQualificationRequirementId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (scheduledEventQualificationRequirementData) => {

        //
        // Success path — scheduledEventQualificationRequirementData can legitimately be null if 404'd but request succeeded
        //
        if (!scheduledEventQualificationRequirementData) {

          this.handleScheduledEventQualificationRequirementNotFound(scheduledEventQualificationRequirementId);

        } else {

          this.scheduledEventQualificationRequirementData = scheduledEventQualificationRequirementData;
          this.buildFormValues(this.scheduledEventQualificationRequirementData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ScheduledEventQualificationRequirement loaded successfully',
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
        this.handleScheduledEventQualificationRequirementLoadError(error, scheduledEventQualificationRequirementId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleScheduledEventQualificationRequirementNotFound(scheduledEventQualificationRequirementId: number): void {

    this.scheduledEventQualificationRequirementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ScheduledEventQualificationRequirement #${scheduledEventQualificationRequirementId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleScheduledEventQualificationRequirementLoadError(error: any, scheduledEventQualificationRequirementId: number): void {

    let message = 'Failed to load Scheduled Event Qualification Requirement.';
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
          message = 'You do not have permission to view this Scheduled Event Qualification Requirement.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduled Event Qualification Requirement #${scheduledEventQualificationRequirementId} was not found.`;
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

    console.error(`Scheduled Event Qualification Requirement load failed (ID: ${scheduledEventQualificationRequirementId})`, error);

    //
    // Reset UI to safe state
    //
    this.scheduledEventQualificationRequirementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(scheduledEventQualificationRequirementData: ScheduledEventQualificationRequirementData | null) {

    if (scheduledEventQualificationRequirementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventQualificationRequirementForm.reset({
        scheduledEventId: null,
        qualificationId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduledEventQualificationRequirementForm.reset({
        scheduledEventId: scheduledEventQualificationRequirementData.scheduledEventId,
        qualificationId: scheduledEventQualificationRequirementData.qualificationId,
        versionNumber: scheduledEventQualificationRequirementData.versionNumber?.toString() ?? '',
        active: scheduledEventQualificationRequirementData.active ?? true,
        deleted: scheduledEventQualificationRequirementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduledEventQualificationRequirementForm.markAsPristine();
    this.scheduledEventQualificationRequirementForm.markAsUntouched();
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

    if (this.scheduledEventQualificationRequirementService.userIsSchedulerScheduledEventQualificationRequirementWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduled Event Qualification Requirements", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.scheduledEventQualificationRequirementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventQualificationRequirementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventQualificationRequirementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventQualificationRequirementSubmitData: ScheduledEventQualificationRequirementSubmitData = {
        id: this.scheduledEventQualificationRequirementData?.id || 0,
        scheduledEventId: Number(formValue.scheduledEventId),
        qualificationId: Number(formValue.qualificationId),
        versionNumber: this.scheduledEventQualificationRequirementData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.scheduledEventQualificationRequirementService.PutScheduledEventQualificationRequirement(scheduledEventQualificationRequirementSubmitData.id, scheduledEventQualificationRequirementSubmitData)
      : this.scheduledEventQualificationRequirementService.PostScheduledEventQualificationRequirement(scheduledEventQualificationRequirementSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedScheduledEventQualificationRequirementData) => {

        this.scheduledEventQualificationRequirementService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduled Event Qualification Requirement's detail page
          //
          this.scheduledEventQualificationRequirementForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.scheduledEventQualificationRequirementForm.markAsUntouched();

          this.router.navigate(['/scheduledeventqualificationrequirements', savedScheduledEventQualificationRequirementData.id]);
          this.alertService.showMessage('Scheduled Event Qualification Requirement added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.scheduledEventQualificationRequirementData = savedScheduledEventQualificationRequirementData;
          this.buildFormValues(this.scheduledEventQualificationRequirementData);

          this.alertService.showMessage("Scheduled Event Qualification Requirement saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduled Event Qualification Requirement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Qualification Requirement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Qualification Requirement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerScheduledEventQualificationRequirementReader(): boolean {
    return this.scheduledEventQualificationRequirementService.userIsSchedulerScheduledEventQualificationRequirementReader();
  }

  public userIsSchedulerScheduledEventQualificationRequirementWriter(): boolean {
    return this.scheduledEventQualificationRequirementService.userIsSchedulerScheduledEventQualificationRequirementWriter();
  }
}
