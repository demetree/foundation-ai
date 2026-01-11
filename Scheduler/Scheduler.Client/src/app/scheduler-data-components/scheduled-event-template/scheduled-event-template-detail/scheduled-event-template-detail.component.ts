/*
   GENERATED FORM FOR THE SCHEDULEDEVENTTEMPLATE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventTemplate table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-template-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventTemplateService, ScheduledEventTemplateData, ScheduledEventTemplateSubmitData } from '../../../scheduler-data-services/scheduled-event-template.service';
import { SchedulingTargetTypeService } from '../../../scheduler-data-services/scheduling-target-type.service';
import { PriorityService } from '../../../scheduler-data-services/priority.service';
import { ScheduledEventTemplateChangeHistoryService } from '../../../scheduler-data-services/scheduled-event-template-change-history.service';
import { ScheduledEventTemplateChargeService } from '../../../scheduler-data-services/scheduled-event-template-charge.service';
import { ScheduledEventTemplateQualificationRequirementService } from '../../../scheduler-data-services/scheduled-event-template-qualification-requirement.service';
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
interface ScheduledEventTemplateFormValues {
  name: string,
  description: string | null,
  defaultAllDay: boolean,
  defaultDurationMinutes: string,     // Stored as string for form input, converted to number on submit.
  schedulingTargetTypeId: number | bigint | null,       // For FK link number
  priorityId: number | bigint | null,       // For FK link number
  defaultLocationPattern: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-scheduled-event-template-detail',
  templateUrl: './scheduled-event-template-detail.component.html',
  styleUrls: ['./scheduled-event-template-detail.component.scss']
})

export class ScheduledEventTemplateDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventTemplateFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventTemplateForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        defaultAllDay: [false],
        defaultDurationMinutes: ['', Validators.required],
        schedulingTargetTypeId: [null],
        priorityId: [null],
        defaultLocationPattern: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public scheduledEventTemplateId: string | null = null;
  public scheduledEventTemplateData: ScheduledEventTemplateData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  scheduledEventTemplates$ = this.scheduledEventTemplateService.GetScheduledEventTemplateList();
  public schedulingTargetTypes$ = this.schedulingTargetTypeService.GetSchedulingTargetTypeList();
  public priorities$ = this.priorityService.GetPriorityList();
  public scheduledEventTemplateChangeHistories$ = this.scheduledEventTemplateChangeHistoryService.GetScheduledEventTemplateChangeHistoryList();
  public scheduledEventTemplateCharges$ = this.scheduledEventTemplateChargeService.GetScheduledEventTemplateChargeList();
  public scheduledEventTemplateQualificationRequirements$ = this.scheduledEventTemplateQualificationRequirementService.GetScheduledEventTemplateQualificationRequirementList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public scheduledEventTemplateService: ScheduledEventTemplateService,
    public schedulingTargetTypeService: SchedulingTargetTypeService,
    public priorityService: PriorityService,
    public scheduledEventTemplateChangeHistoryService: ScheduledEventTemplateChangeHistoryService,
    public scheduledEventTemplateChargeService: ScheduledEventTemplateChargeService,
    public scheduledEventTemplateQualificationRequirementService: ScheduledEventTemplateQualificationRequirementService,
    public scheduledEventService: ScheduledEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the scheduledEventTemplateId from the route parameters
    this.scheduledEventTemplateId = this.route.snapshot.paramMap.get('scheduledEventTemplateId');

    if (this.scheduledEventTemplateId === 'new' ||
        this.scheduledEventTemplateId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.scheduledEventTemplateData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduledEventTemplateForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventTemplateForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduled Event Template';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduled Event Template';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.scheduledEventTemplateForm.dirty) {
      return confirm('You have unsaved Scheduled Event Template changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.scheduledEventTemplateId != null && this.scheduledEventTemplateId !== 'new') {

      const id = parseInt(this.scheduledEventTemplateId, 10);

      if (!isNaN(id)) {
        return { scheduledEventTemplateId: id };
      }
    }

    return null;
  }


/*
  * Loads the ScheduledEventTemplate data for the current scheduledEventTemplateId.
  *
  * Fully respects the ScheduledEventTemplateService caching strategy and error handling strategy.
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
    if (!this.scheduledEventTemplateService.userIsSchedulerScheduledEventTemplateReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ScheduledEventTemplates.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate scheduledEventTemplateId
    //
    if (!this.scheduledEventTemplateId) {

      this.alertService.showMessage('No ScheduledEventTemplate ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const scheduledEventTemplateId = Number(this.scheduledEventTemplateId);

    if (isNaN(scheduledEventTemplateId) || scheduledEventTemplateId <= 0) {

      this.alertService.showMessage(`Invalid Scheduled Event Template ID: "${this.scheduledEventTemplateId}"`,
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
      // This is the most targeted way: clear only this ScheduledEventTemplate + relations

      this.scheduledEventTemplateService.ClearRecordCache(scheduledEventTemplateId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.scheduledEventTemplateService.GetScheduledEventTemplate(scheduledEventTemplateId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (scheduledEventTemplateData) => {

        //
        // Success path — scheduledEventTemplateData can legitimately be null if 404'd but request succeeded
        //
        if (!scheduledEventTemplateData) {

          this.handleScheduledEventTemplateNotFound(scheduledEventTemplateId);

        } else {

          this.scheduledEventTemplateData = scheduledEventTemplateData;
          this.buildFormValues(this.scheduledEventTemplateData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ScheduledEventTemplate loaded successfully',
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
        this.handleScheduledEventTemplateLoadError(error, scheduledEventTemplateId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleScheduledEventTemplateNotFound(scheduledEventTemplateId: number): void {

    this.scheduledEventTemplateData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ScheduledEventTemplate #${scheduledEventTemplateId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleScheduledEventTemplateLoadError(error: any, scheduledEventTemplateId: number): void {

    let message = 'Failed to load Scheduled Event Template.';
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
          message = 'You do not have permission to view this Scheduled Event Template.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduled Event Template #${scheduledEventTemplateId} was not found.`;
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

    console.error(`Scheduled Event Template load failed (ID: ${scheduledEventTemplateId})`, error);

    //
    // Reset UI to safe state
    //
    this.scheduledEventTemplateData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(scheduledEventTemplateData: ScheduledEventTemplateData | null) {

    if (scheduledEventTemplateData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventTemplateForm.reset({
        name: '',
        description: '',
        defaultAllDay: false,
        defaultDurationMinutes: '',
        schedulingTargetTypeId: null,
        priorityId: null,
        defaultLocationPattern: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduledEventTemplateForm.reset({
        name: scheduledEventTemplateData.name ?? '',
        description: scheduledEventTemplateData.description ?? '',
        defaultAllDay: scheduledEventTemplateData.defaultAllDay ?? false,
        defaultDurationMinutes: scheduledEventTemplateData.defaultDurationMinutes?.toString() ?? '',
        schedulingTargetTypeId: scheduledEventTemplateData.schedulingTargetTypeId,
        priorityId: scheduledEventTemplateData.priorityId,
        defaultLocationPattern: scheduledEventTemplateData.defaultLocationPattern ?? '',
        versionNumber: scheduledEventTemplateData.versionNumber?.toString() ?? '',
        active: scheduledEventTemplateData.active ?? true,
        deleted: scheduledEventTemplateData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduledEventTemplateForm.markAsPristine();
    this.scheduledEventTemplateForm.markAsUntouched();
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

    if (this.scheduledEventTemplateService.userIsSchedulerScheduledEventTemplateWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduled Event Templates", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.scheduledEventTemplateForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventTemplateForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventTemplateForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventTemplateSubmitData: ScheduledEventTemplateSubmitData = {
        id: this.scheduledEventTemplateData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        defaultAllDay: !!formValue.defaultAllDay,
        defaultDurationMinutes: Number(formValue.defaultDurationMinutes),
        schedulingTargetTypeId: formValue.schedulingTargetTypeId ? Number(formValue.schedulingTargetTypeId) : null,
        priorityId: formValue.priorityId ? Number(formValue.priorityId) : null,
        defaultLocationPattern: formValue.defaultLocationPattern?.trim() || null,
        versionNumber: this.scheduledEventTemplateData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.scheduledEventTemplateService.PutScheduledEventTemplate(scheduledEventTemplateSubmitData.id, scheduledEventTemplateSubmitData)
      : this.scheduledEventTemplateService.PostScheduledEventTemplate(scheduledEventTemplateSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedScheduledEventTemplateData) => {

        this.scheduledEventTemplateService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduled Event Template's detail page
          //
          this.scheduledEventTemplateForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.scheduledEventTemplateForm.markAsUntouched();

          this.router.navigate(['/scheduledeventtemplates', savedScheduledEventTemplateData.id]);
          this.alertService.showMessage('Scheduled Event Template added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.scheduledEventTemplateData = savedScheduledEventTemplateData;
          this.buildFormValues(this.scheduledEventTemplateData);

          this.alertService.showMessage("Scheduled Event Template saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduled Event Template.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Template.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Template could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerScheduledEventTemplateReader(): boolean {
    return this.scheduledEventTemplateService.userIsSchedulerScheduledEventTemplateReader();
  }

  public userIsSchedulerScheduledEventTemplateWriter(): boolean {
    return this.scheduledEventTemplateService.userIsSchedulerScheduledEventTemplateWriter();
  }
}
