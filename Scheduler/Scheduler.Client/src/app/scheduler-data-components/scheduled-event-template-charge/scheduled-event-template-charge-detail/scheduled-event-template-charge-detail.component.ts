/*
   GENERATED FORM FOR THE SCHEDULEDEVENTTEMPLATECHARGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventTemplateCharge table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-template-charge-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventTemplateChargeService, ScheduledEventTemplateChargeData, ScheduledEventTemplateChargeSubmitData } from '../../../scheduler-data-services/scheduled-event-template-charge.service';
import { ScheduledEventTemplateService } from '../../../scheduler-data-services/scheduled-event-template.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { ScheduledEventTemplateChargeChangeHistoryService } from '../../../scheduler-data-services/scheduled-event-template-charge-change-history.service';
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
interface ScheduledEventTemplateChargeFormValues {
  scheduledEventTemplateId: number | bigint,       // For FK link number
  chargeTypeId: number | bigint,       // For FK link number
  defaultAmount: string,     // Stored as string for form input, converted to number on submit.
  isRequired: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-scheduled-event-template-charge-detail',
  templateUrl: './scheduled-event-template-charge-detail.component.html',
  styleUrls: ['./scheduled-event-template-charge-detail.component.scss']
})

export class ScheduledEventTemplateChargeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventTemplateChargeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventTemplateChargeForm: FormGroup = this.fb.group({
        scheduledEventTemplateId: [null, Validators.required],
        chargeTypeId: [null, Validators.required],
        defaultAmount: ['', Validators.required],
        isRequired: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public scheduledEventTemplateChargeId: string | null = null;
  public scheduledEventTemplateChargeData: ScheduledEventTemplateChargeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  scheduledEventTemplateCharges$ = this.scheduledEventTemplateChargeService.GetScheduledEventTemplateChargeList();
  public scheduledEventTemplates$ = this.scheduledEventTemplateService.GetScheduledEventTemplateList();
  public chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  public scheduledEventTemplateChargeChangeHistories$ = this.scheduledEventTemplateChargeChangeHistoryService.GetScheduledEventTemplateChargeChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public scheduledEventTemplateChargeService: ScheduledEventTemplateChargeService,
    public scheduledEventTemplateService: ScheduledEventTemplateService,
    public chargeTypeService: ChargeTypeService,
    public scheduledEventTemplateChargeChangeHistoryService: ScheduledEventTemplateChargeChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the scheduledEventTemplateChargeId from the route parameters
    this.scheduledEventTemplateChargeId = this.route.snapshot.paramMap.get('scheduledEventTemplateChargeId');

    if (this.scheduledEventTemplateChargeId === 'new' ||
        this.scheduledEventTemplateChargeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.scheduledEventTemplateChargeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduledEventTemplateChargeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventTemplateChargeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduled Event Template Charge';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduled Event Template Charge';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.scheduledEventTemplateChargeForm.dirty) {
      return confirm('You have unsaved Scheduled Event Template Charge changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.scheduledEventTemplateChargeId != null && this.scheduledEventTemplateChargeId !== 'new') {

      const id = parseInt(this.scheduledEventTemplateChargeId, 10);

      if (!isNaN(id)) {
        return { scheduledEventTemplateChargeId: id };
      }
    }

    return null;
  }


/*
  * Loads the ScheduledEventTemplateCharge data for the current scheduledEventTemplateChargeId.
  *
  * Fully respects the ScheduledEventTemplateChargeService caching strategy and error handling strategy.
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
    if (!this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ScheduledEventTemplateCharges.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate scheduledEventTemplateChargeId
    //
    if (!this.scheduledEventTemplateChargeId) {

      this.alertService.showMessage('No ScheduledEventTemplateCharge ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const scheduledEventTemplateChargeId = Number(this.scheduledEventTemplateChargeId);

    if (isNaN(scheduledEventTemplateChargeId) || scheduledEventTemplateChargeId <= 0) {

      this.alertService.showMessage(`Invalid Scheduled Event Template Charge ID: "${this.scheduledEventTemplateChargeId}"`,
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
      // This is the most targeted way: clear only this ScheduledEventTemplateCharge + relations

      this.scheduledEventTemplateChargeService.ClearRecordCache(scheduledEventTemplateChargeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.scheduledEventTemplateChargeService.GetScheduledEventTemplateCharge(scheduledEventTemplateChargeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (scheduledEventTemplateChargeData) => {

        //
        // Success path — scheduledEventTemplateChargeData can legitimately be null if 404'd but request succeeded
        //
        if (!scheduledEventTemplateChargeData) {

          this.handleScheduledEventTemplateChargeNotFound(scheduledEventTemplateChargeId);

        } else {

          this.scheduledEventTemplateChargeData = scheduledEventTemplateChargeData;
          this.buildFormValues(this.scheduledEventTemplateChargeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ScheduledEventTemplateCharge loaded successfully',
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
        this.handleScheduledEventTemplateChargeLoadError(error, scheduledEventTemplateChargeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleScheduledEventTemplateChargeNotFound(scheduledEventTemplateChargeId: number): void {

    this.scheduledEventTemplateChargeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ScheduledEventTemplateCharge #${scheduledEventTemplateChargeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleScheduledEventTemplateChargeLoadError(error: any, scheduledEventTemplateChargeId: number): void {

    let message = 'Failed to load Scheduled Event Template Charge.';
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
          message = 'You do not have permission to view this Scheduled Event Template Charge.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduled Event Template Charge #${scheduledEventTemplateChargeId} was not found.`;
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

    console.error(`Scheduled Event Template Charge load failed (ID: ${scheduledEventTemplateChargeId})`, error);

    //
    // Reset UI to safe state
    //
    this.scheduledEventTemplateChargeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(scheduledEventTemplateChargeData: ScheduledEventTemplateChargeData | null) {

    if (scheduledEventTemplateChargeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventTemplateChargeForm.reset({
        scheduledEventTemplateId: null,
        chargeTypeId: null,
        defaultAmount: '',
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
        this.scheduledEventTemplateChargeForm.reset({
        scheduledEventTemplateId: scheduledEventTemplateChargeData.scheduledEventTemplateId,
        chargeTypeId: scheduledEventTemplateChargeData.chargeTypeId,
        defaultAmount: scheduledEventTemplateChargeData.defaultAmount?.toString() ?? '',
        isRequired: scheduledEventTemplateChargeData.isRequired ?? false,
        versionNumber: scheduledEventTemplateChargeData.versionNumber?.toString() ?? '',
        active: scheduledEventTemplateChargeData.active ?? true,
        deleted: scheduledEventTemplateChargeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduledEventTemplateChargeForm.markAsPristine();
    this.scheduledEventTemplateChargeForm.markAsUntouched();
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

    if (this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduled Event Template Charges", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.scheduledEventTemplateChargeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventTemplateChargeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventTemplateChargeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventTemplateChargeSubmitData: ScheduledEventTemplateChargeSubmitData = {
        id: this.scheduledEventTemplateChargeData?.id || 0,
        scheduledEventTemplateId: Number(formValue.scheduledEventTemplateId),
        chargeTypeId: Number(formValue.chargeTypeId),
        defaultAmount: Number(formValue.defaultAmount),
        isRequired: !!formValue.isRequired,
        versionNumber: this.scheduledEventTemplateChargeData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.scheduledEventTemplateChargeService.PutScheduledEventTemplateCharge(scheduledEventTemplateChargeSubmitData.id, scheduledEventTemplateChargeSubmitData)
      : this.scheduledEventTemplateChargeService.PostScheduledEventTemplateCharge(scheduledEventTemplateChargeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedScheduledEventTemplateChargeData) => {

        this.scheduledEventTemplateChargeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduled Event Template Charge's detail page
          //
          this.scheduledEventTemplateChargeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.scheduledEventTemplateChargeForm.markAsUntouched();

          this.router.navigate(['/scheduledeventtemplatecharges', savedScheduledEventTemplateChargeData.id]);
          this.alertService.showMessage('Scheduled Event Template Charge added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.scheduledEventTemplateChargeData = savedScheduledEventTemplateChargeData;
          this.buildFormValues(this.scheduledEventTemplateChargeData);

          this.alertService.showMessage("Scheduled Event Template Charge saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduled Event Template Charge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Template Charge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Template Charge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerScheduledEventTemplateChargeReader(): boolean {
    return this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeReader();
  }

  public userIsSchedulerScheduledEventTemplateChargeWriter(): boolean {
    return this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeWriter();
  }
}
