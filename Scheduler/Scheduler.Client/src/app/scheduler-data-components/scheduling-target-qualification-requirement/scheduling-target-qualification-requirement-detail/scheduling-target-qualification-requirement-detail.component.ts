/*
   GENERATED FORM FOR THE SCHEDULINGTARGETQUALIFICATIONREQUIREMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetQualificationRequirement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-qualification-requirement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetQualificationRequirementService, SchedulingTargetQualificationRequirementData, SchedulingTargetQualificationRequirementSubmitData } from '../../../scheduler-data-services/scheduling-target-qualification-requirement.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { QualificationService } from '../../../scheduler-data-services/qualification.service';
import { SchedulingTargetQualificationRequirementChangeHistoryService } from '../../../scheduler-data-services/scheduling-target-qualification-requirement-change-history.service';
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
interface SchedulingTargetQualificationRequirementFormValues {
  schedulingTargetId: number | bigint,       // For FK link number
  qualificationId: number | bigint,       // For FK link number
  isRequired: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-scheduling-target-qualification-requirement-detail',
  templateUrl: './scheduling-target-qualification-requirement-detail.component.html',
  styleUrls: ['./scheduling-target-qualification-requirement-detail.component.scss']
})

export class SchedulingTargetQualificationRequirementDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetQualificationRequirementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetQualificationRequirementForm: FormGroup = this.fb.group({
        schedulingTargetId: [null, Validators.required],
        qualificationId: [null, Validators.required],
        isRequired: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public schedulingTargetQualificationRequirementId: string | null = null;
  public schedulingTargetQualificationRequirementData: SchedulingTargetQualificationRequirementData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  schedulingTargetQualificationRequirements$ = this.schedulingTargetQualificationRequirementService.GetSchedulingTargetQualificationRequirementList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public qualifications$ = this.qualificationService.GetQualificationList();
  public schedulingTargetQualificationRequirementChangeHistories$ = this.schedulingTargetQualificationRequirementChangeHistoryService.GetSchedulingTargetQualificationRequirementChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public schedulingTargetQualificationRequirementService: SchedulingTargetQualificationRequirementService,
    public schedulingTargetService: SchedulingTargetService,
    public qualificationService: QualificationService,
    public schedulingTargetQualificationRequirementChangeHistoryService: SchedulingTargetQualificationRequirementChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the schedulingTargetQualificationRequirementId from the route parameters
    this.schedulingTargetQualificationRequirementId = this.route.snapshot.paramMap.get('schedulingTargetQualificationRequirementId');

    if (this.schedulingTargetQualificationRequirementId === 'new' ||
        this.schedulingTargetQualificationRequirementId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.schedulingTargetQualificationRequirementData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.schedulingTargetQualificationRequirementForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetQualificationRequirementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduling Target Qualification Requirement';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduling Target Qualification Requirement';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.schedulingTargetQualificationRequirementForm.dirty) {
      return confirm('You have unsaved Scheduling Target Qualification Requirement changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.schedulingTargetQualificationRequirementId != null && this.schedulingTargetQualificationRequirementId !== 'new') {

      const id = parseInt(this.schedulingTargetQualificationRequirementId, 10);

      if (!isNaN(id)) {
        return { schedulingTargetQualificationRequirementId: id };
      }
    }

    return null;
  }


/*
  * Loads the SchedulingTargetQualificationRequirement data for the current schedulingTargetQualificationRequirementId.
  *
  * Fully respects the SchedulingTargetQualificationRequirementService caching strategy and error handling strategy.
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
    if (!this.schedulingTargetQualificationRequirementService.userIsSchedulerSchedulingTargetQualificationRequirementReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SchedulingTargetQualificationRequirements.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate schedulingTargetQualificationRequirementId
    //
    if (!this.schedulingTargetQualificationRequirementId) {

      this.alertService.showMessage('No SchedulingTargetQualificationRequirement ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const schedulingTargetQualificationRequirementId = Number(this.schedulingTargetQualificationRequirementId);

    if (isNaN(schedulingTargetQualificationRequirementId) || schedulingTargetQualificationRequirementId <= 0) {

      this.alertService.showMessage(`Invalid Scheduling Target Qualification Requirement ID: "${this.schedulingTargetQualificationRequirementId}"`,
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
      // This is the most targeted way: clear only this SchedulingTargetQualificationRequirement + relations

      this.schedulingTargetQualificationRequirementService.ClearRecordCache(schedulingTargetQualificationRequirementId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.schedulingTargetQualificationRequirementService.GetSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirementId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (schedulingTargetQualificationRequirementData) => {

        //
        // Success path — schedulingTargetQualificationRequirementData can legitimately be null if 404'd but request succeeded
        //
        if (!schedulingTargetQualificationRequirementData) {

          this.handleSchedulingTargetQualificationRequirementNotFound(schedulingTargetQualificationRequirementId);

        } else {

          this.schedulingTargetQualificationRequirementData = schedulingTargetQualificationRequirementData;
          this.buildFormValues(this.schedulingTargetQualificationRequirementData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SchedulingTargetQualificationRequirement loaded successfully',
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
        this.handleSchedulingTargetQualificationRequirementLoadError(error, schedulingTargetQualificationRequirementId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSchedulingTargetQualificationRequirementNotFound(schedulingTargetQualificationRequirementId: number): void {

    this.schedulingTargetQualificationRequirementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SchedulingTargetQualificationRequirement #${schedulingTargetQualificationRequirementId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSchedulingTargetQualificationRequirementLoadError(error: any, schedulingTargetQualificationRequirementId: number): void {

    let message = 'Failed to load Scheduling Target Qualification Requirement.';
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
          message = 'You do not have permission to view this Scheduling Target Qualification Requirement.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduling Target Qualification Requirement #${schedulingTargetQualificationRequirementId} was not found.`;
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

    console.error(`Scheduling Target Qualification Requirement load failed (ID: ${schedulingTargetQualificationRequirementId})`, error);

    //
    // Reset UI to safe state
    //
    this.schedulingTargetQualificationRequirementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(schedulingTargetQualificationRequirementData: SchedulingTargetQualificationRequirementData | null) {

    if (schedulingTargetQualificationRequirementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetQualificationRequirementForm.reset({
        schedulingTargetId: null,
        qualificationId: null,
        isRequired: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.schedulingTargetQualificationRequirementForm.reset({
        schedulingTargetId: schedulingTargetQualificationRequirementData.schedulingTargetId,
        qualificationId: schedulingTargetQualificationRequirementData.qualificationId,
        isRequired: schedulingTargetQualificationRequirementData.isRequired ?? false,
        versionNumber: schedulingTargetQualificationRequirementData.versionNumber?.toString() ?? '',
        active: schedulingTargetQualificationRequirementData.active ?? true,
        deleted: schedulingTargetQualificationRequirementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.schedulingTargetQualificationRequirementForm.markAsPristine();
    this.schedulingTargetQualificationRequirementForm.markAsUntouched();
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

    if (this.schedulingTargetQualificationRequirementService.userIsSchedulerSchedulingTargetQualificationRequirementWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduling Target Qualification Requirements", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.schedulingTargetQualificationRequirementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetQualificationRequirementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetQualificationRequirementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetQualificationRequirementSubmitData: SchedulingTargetQualificationRequirementSubmitData = {
        id: this.schedulingTargetQualificationRequirementData?.id || 0,
        schedulingTargetId: Number(formValue.schedulingTargetId),
        qualificationId: Number(formValue.qualificationId),
        isRequired: !!formValue.isRequired,
        versionNumber: this.schedulingTargetQualificationRequirementData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.schedulingTargetQualificationRequirementService.PutSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirementSubmitData.id, schedulingTargetQualificationRequirementSubmitData)
      : this.schedulingTargetQualificationRequirementService.PostSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirementSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSchedulingTargetQualificationRequirementData) => {

        this.schedulingTargetQualificationRequirementService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduling Target Qualification Requirement's detail page
          //
          this.schedulingTargetQualificationRequirementForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.schedulingTargetQualificationRequirementForm.markAsUntouched();

          this.router.navigate(['/schedulingtargetqualificationrequirements', savedSchedulingTargetQualificationRequirementData.id]);
          this.alertService.showMessage('Scheduling Target Qualification Requirement added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.schedulingTargetQualificationRequirementData = savedSchedulingTargetQualificationRequirementData;
          this.buildFormValues(this.schedulingTargetQualificationRequirementData);

          this.alertService.showMessage("Scheduling Target Qualification Requirement saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduling Target Qualification Requirement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Qualification Requirement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Qualification Requirement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerSchedulingTargetQualificationRequirementReader(): boolean {
    return this.schedulingTargetQualificationRequirementService.userIsSchedulerSchedulingTargetQualificationRequirementReader();
  }

  public userIsSchedulerSchedulingTargetQualificationRequirementWriter(): boolean {
    return this.schedulingTargetQualificationRequirementService.userIsSchedulerSchedulingTargetQualificationRequirementWriter();
  }
}
