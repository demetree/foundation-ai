/*
   GENERATED FORM FOR THE SCHEDULEDEVENTTEMPLATEQUALIFICATIONREQUIREMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventTemplateQualificationRequirement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-template-qualification-requirement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventTemplateQualificationRequirementService, ScheduledEventTemplateQualificationRequirementData, ScheduledEventTemplateQualificationRequirementSubmitData } from '../../../scheduler-data-services/scheduled-event-template-qualification-requirement.service';
import { ScheduledEventTemplateService } from '../../../scheduler-data-services/scheduled-event-template.service';
import { QualificationService } from '../../../scheduler-data-services/qualification.service';
import { ScheduledEventTemplateQualificationRequirementChangeHistoryService } from '../../../scheduler-data-services/scheduled-event-template-qualification-requirement-change-history.service';
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
interface ScheduledEventTemplateQualificationRequirementFormValues {
  scheduledEventTemplateId: number | bigint,       // For FK link number
  qualificationId: number | bigint,       // For FK link number
  isRequired: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-scheduled-event-template-qualification-requirement-detail',
  templateUrl: './scheduled-event-template-qualification-requirement-detail.component.html',
  styleUrls: ['./scheduled-event-template-qualification-requirement-detail.component.scss']
})

export class ScheduledEventTemplateQualificationRequirementDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventTemplateQualificationRequirementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventTemplateQualificationRequirementForm: FormGroup = this.fb.group({
        scheduledEventTemplateId: [null, Validators.required],
        qualificationId: [null, Validators.required],
        isRequired: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public scheduledEventTemplateQualificationRequirementId: string | null = null;
  public scheduledEventTemplateQualificationRequirementData: ScheduledEventTemplateQualificationRequirementData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  scheduledEventTemplateQualificationRequirements$ = this.scheduledEventTemplateQualificationRequirementService.GetScheduledEventTemplateQualificationRequirementList();
  public scheduledEventTemplates$ = this.scheduledEventTemplateService.GetScheduledEventTemplateList();
  public qualifications$ = this.qualificationService.GetQualificationList();
  public scheduledEventTemplateQualificationRequirementChangeHistories$ = this.scheduledEventTemplateQualificationRequirementChangeHistoryService.GetScheduledEventTemplateQualificationRequirementChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public scheduledEventTemplateQualificationRequirementService: ScheduledEventTemplateQualificationRequirementService,
    public scheduledEventTemplateService: ScheduledEventTemplateService,
    public qualificationService: QualificationService,
    public scheduledEventTemplateQualificationRequirementChangeHistoryService: ScheduledEventTemplateQualificationRequirementChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the scheduledEventTemplateQualificationRequirementId from the route parameters
    this.scheduledEventTemplateQualificationRequirementId = this.route.snapshot.paramMap.get('scheduledEventTemplateQualificationRequirementId');

    if (this.scheduledEventTemplateQualificationRequirementId === 'new' ||
        this.scheduledEventTemplateQualificationRequirementId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.scheduledEventTemplateQualificationRequirementData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduledEventTemplateQualificationRequirementForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventTemplateQualificationRequirementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduled Event Template Qualification Requirement';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduled Event Template Qualification Requirement';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.scheduledEventTemplateQualificationRequirementForm.dirty) {
      return confirm('You have unsaved Scheduled Event Template Qualification Requirement changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.scheduledEventTemplateQualificationRequirementId != null && this.scheduledEventTemplateQualificationRequirementId !== 'new') {

      const id = parseInt(this.scheduledEventTemplateQualificationRequirementId, 10);

      if (!isNaN(id)) {
        return { scheduledEventTemplateQualificationRequirementId: id };
      }
    }

    return null;
  }


/*
  * Loads the ScheduledEventTemplateQualificationRequirement data for the current scheduledEventTemplateQualificationRequirementId.
  *
  * Fully respects the ScheduledEventTemplateQualificationRequirementService caching strategy and error handling strategy.
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
    if (!this.scheduledEventTemplateQualificationRequirementService.userIsSchedulerScheduledEventTemplateQualificationRequirementReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ScheduledEventTemplateQualificationRequirements.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate scheduledEventTemplateQualificationRequirementId
    //
    if (!this.scheduledEventTemplateQualificationRequirementId) {

      this.alertService.showMessage('No ScheduledEventTemplateQualificationRequirement ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const scheduledEventTemplateQualificationRequirementId = Number(this.scheduledEventTemplateQualificationRequirementId);

    if (isNaN(scheduledEventTemplateQualificationRequirementId) || scheduledEventTemplateQualificationRequirementId <= 0) {

      this.alertService.showMessage(`Invalid Scheduled Event Template Qualification Requirement ID: "${this.scheduledEventTemplateQualificationRequirementId}"`,
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
      // This is the most targeted way: clear only this ScheduledEventTemplateQualificationRequirement + relations

      this.scheduledEventTemplateQualificationRequirementService.ClearRecordCache(scheduledEventTemplateQualificationRequirementId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.scheduledEventTemplateQualificationRequirementService.GetScheduledEventTemplateQualificationRequirement(scheduledEventTemplateQualificationRequirementId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (scheduledEventTemplateQualificationRequirementData) => {

        //
        // Success path — scheduledEventTemplateQualificationRequirementData can legitimately be null if 404'd but request succeeded
        //
        if (!scheduledEventTemplateQualificationRequirementData) {

          this.handleScheduledEventTemplateQualificationRequirementNotFound(scheduledEventTemplateQualificationRequirementId);

        } else {

          this.scheduledEventTemplateQualificationRequirementData = scheduledEventTemplateQualificationRequirementData;
          this.buildFormValues(this.scheduledEventTemplateQualificationRequirementData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ScheduledEventTemplateQualificationRequirement loaded successfully',
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
        this.handleScheduledEventTemplateQualificationRequirementLoadError(error, scheduledEventTemplateQualificationRequirementId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleScheduledEventTemplateQualificationRequirementNotFound(scheduledEventTemplateQualificationRequirementId: number): void {

    this.scheduledEventTemplateQualificationRequirementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ScheduledEventTemplateQualificationRequirement #${scheduledEventTemplateQualificationRequirementId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleScheduledEventTemplateQualificationRequirementLoadError(error: any, scheduledEventTemplateQualificationRequirementId: number): void {

    let message = 'Failed to load Scheduled Event Template Qualification Requirement.';
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
          message = 'You do not have permission to view this Scheduled Event Template Qualification Requirement.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduled Event Template Qualification Requirement #${scheduledEventTemplateQualificationRequirementId} was not found.`;
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

    console.error(`Scheduled Event Template Qualification Requirement load failed (ID: ${scheduledEventTemplateQualificationRequirementId})`, error);

    //
    // Reset UI to safe state
    //
    this.scheduledEventTemplateQualificationRequirementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(scheduledEventTemplateQualificationRequirementData: ScheduledEventTemplateQualificationRequirementData | null) {

    if (scheduledEventTemplateQualificationRequirementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventTemplateQualificationRequirementForm.reset({
        scheduledEventTemplateId: null,
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
        this.scheduledEventTemplateQualificationRequirementForm.reset({
        scheduledEventTemplateId: scheduledEventTemplateQualificationRequirementData.scheduledEventTemplateId,
        qualificationId: scheduledEventTemplateQualificationRequirementData.qualificationId,
        isRequired: scheduledEventTemplateQualificationRequirementData.isRequired ?? false,
        versionNumber: scheduledEventTemplateQualificationRequirementData.versionNumber?.toString() ?? '',
        active: scheduledEventTemplateQualificationRequirementData.active ?? true,
        deleted: scheduledEventTemplateQualificationRequirementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduledEventTemplateQualificationRequirementForm.markAsPristine();
    this.scheduledEventTemplateQualificationRequirementForm.markAsUntouched();
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

    if (this.scheduledEventTemplateQualificationRequirementService.userIsSchedulerScheduledEventTemplateQualificationRequirementWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduled Event Template Qualification Requirements", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.scheduledEventTemplateQualificationRequirementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventTemplateQualificationRequirementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventTemplateQualificationRequirementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventTemplateQualificationRequirementSubmitData: ScheduledEventTemplateQualificationRequirementSubmitData = {
        id: this.scheduledEventTemplateQualificationRequirementData?.id || 0,
        scheduledEventTemplateId: Number(formValue.scheduledEventTemplateId),
        qualificationId: Number(formValue.qualificationId),
        isRequired: !!formValue.isRequired,
        versionNumber: this.scheduledEventTemplateQualificationRequirementData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.scheduledEventTemplateQualificationRequirementService.PutScheduledEventTemplateQualificationRequirement(scheduledEventTemplateQualificationRequirementSubmitData.id, scheduledEventTemplateQualificationRequirementSubmitData)
      : this.scheduledEventTemplateQualificationRequirementService.PostScheduledEventTemplateQualificationRequirement(scheduledEventTemplateQualificationRequirementSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedScheduledEventTemplateQualificationRequirementData) => {

        this.scheduledEventTemplateQualificationRequirementService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduled Event Template Qualification Requirement's detail page
          //
          this.scheduledEventTemplateQualificationRequirementForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.scheduledEventTemplateQualificationRequirementForm.markAsUntouched();

          this.router.navigate(['/scheduledeventtemplatequalificationrequirements', savedScheduledEventTemplateQualificationRequirementData.id]);
          this.alertService.showMessage('Scheduled Event Template Qualification Requirement added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.scheduledEventTemplateQualificationRequirementData = savedScheduledEventTemplateQualificationRequirementData;
          this.buildFormValues(this.scheduledEventTemplateQualificationRequirementData);

          this.alertService.showMessage("Scheduled Event Template Qualification Requirement saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduled Event Template Qualification Requirement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Template Qualification Requirement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Template Qualification Requirement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerScheduledEventTemplateQualificationRequirementReader(): boolean {
    return this.scheduledEventTemplateQualificationRequirementService.userIsSchedulerScheduledEventTemplateQualificationRequirementReader();
  }

  public userIsSchedulerScheduledEventTemplateQualificationRequirementWriter(): boolean {
    return this.scheduledEventTemplateQualificationRequirementService.userIsSchedulerScheduledEventTemplateQualificationRequirementWriter();
  }
}
