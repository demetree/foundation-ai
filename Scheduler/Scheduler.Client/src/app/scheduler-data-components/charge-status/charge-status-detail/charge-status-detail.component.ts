/*
   GENERATED FORM FOR THE CHARGESTATUS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ChargeStatus table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to charge-status-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ChargeStatusService, ChargeStatusData, ChargeStatusSubmitData } from '../../../scheduler-data-services/charge-status.service';
import { ChargeStatusChangeHistoryService } from '../../../scheduler-data-services/charge-status-change-history.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
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
interface ChargeStatusFormValues {
  name: string,
  description: string,
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-charge-status-detail',
  templateUrl: './charge-status-detail.component.html',
  styleUrls: ['./charge-status-detail.component.scss']
})

export class ChargeStatusDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ChargeStatusFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public chargeStatusForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        color: [''],
        sequence: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public chargeStatusId: string | null = null;
  public chargeStatusData: ChargeStatusData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  chargeStatuses$ = this.chargeStatusService.GetChargeStatusList();
  public chargeStatusChangeHistories$ = this.chargeStatusChangeHistoryService.GetChargeStatusChangeHistoryList();
  public eventCharges$ = this.eventChargeService.GetEventChargeList();

  private destroy$ = new Subject<void>();

  constructor(
    public chargeStatusService: ChargeStatusService,
    public chargeStatusChangeHistoryService: ChargeStatusChangeHistoryService,
    public eventChargeService: EventChargeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the chargeStatusId from the route parameters
    this.chargeStatusId = this.route.snapshot.paramMap.get('chargeStatusId');

    if (this.chargeStatusId === 'new' ||
        this.chargeStatusId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.chargeStatusData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.chargeStatusForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.chargeStatusForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Charge Status';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Charge Status';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.chargeStatusForm.dirty) {
      return confirm('You have unsaved Charge Status changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.chargeStatusId != null && this.chargeStatusId !== 'new') {

      const id = parseInt(this.chargeStatusId, 10);

      if (!isNaN(id)) {
        return { chargeStatusId: id };
      }
    }

    return null;
  }


/*
  * Loads the ChargeStatus data for the current chargeStatusId.
  *
  * Fully respects the ChargeStatusService caching strategy and error handling strategy.
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
    if (!this.chargeStatusService.userIsSchedulerChargeStatusReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ChargeStatuses.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate chargeStatusId
    //
    if (!this.chargeStatusId) {

      this.alertService.showMessage('No ChargeStatus ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const chargeStatusId = Number(this.chargeStatusId);

    if (isNaN(chargeStatusId) || chargeStatusId <= 0) {

      this.alertService.showMessage(`Invalid Charge Status ID: "${this.chargeStatusId}"`,
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
      // This is the most targeted way: clear only this ChargeStatus + relations

      this.chargeStatusService.ClearRecordCache(chargeStatusId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.chargeStatusService.GetChargeStatus(chargeStatusId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (chargeStatusData) => {

        //
        // Success path — chargeStatusData can legitimately be null if 404'd but request succeeded
        //
        if (!chargeStatusData) {

          this.handleChargeStatusNotFound(chargeStatusId);

        } else {

          this.chargeStatusData = chargeStatusData;
          this.buildFormValues(this.chargeStatusData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ChargeStatus loaded successfully',
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
        this.handleChargeStatusLoadError(error, chargeStatusId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleChargeStatusNotFound(chargeStatusId: number): void {

    this.chargeStatusData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ChargeStatus #${chargeStatusId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleChargeStatusLoadError(error: any, chargeStatusId: number): void {

    let message = 'Failed to load Charge Status.';
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
          message = 'You do not have permission to view this Charge Status.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Charge Status #${chargeStatusId} was not found.`;
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

    console.error(`Charge Status load failed (ID: ${chargeStatusId})`, error);

    //
    // Reset UI to safe state
    //
    this.chargeStatusData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(chargeStatusData: ChargeStatusData | null) {

    if (chargeStatusData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.chargeStatusForm.reset({
        name: '',
        description: '',
        color: '',
        sequence: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.chargeStatusForm.reset({
        name: chargeStatusData.name ?? '',
        description: chargeStatusData.description ?? '',
        color: chargeStatusData.color ?? '',
        sequence: chargeStatusData.sequence?.toString() ?? '',
        versionNumber: chargeStatusData.versionNumber?.toString() ?? '',
        active: chargeStatusData.active ?? true,
        deleted: chargeStatusData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.chargeStatusForm.markAsPristine();
    this.chargeStatusForm.markAsUntouched();
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

    if (this.chargeStatusService.userIsSchedulerChargeStatusWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Charge Statuses", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.chargeStatusForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.chargeStatusForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.chargeStatusForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const chargeStatusSubmitData: ChargeStatusSubmitData = {
        id: this.chargeStatusData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        versionNumber: this.chargeStatusData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.chargeStatusService.PutChargeStatus(chargeStatusSubmitData.id, chargeStatusSubmitData)
      : this.chargeStatusService.PostChargeStatus(chargeStatusSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedChargeStatusData) => {

        this.chargeStatusService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Charge Status's detail page
          //
          this.chargeStatusForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.chargeStatusForm.markAsUntouched();

          this.router.navigate(['/chargestatuses', savedChargeStatusData.id]);
          this.alertService.showMessage('Charge Status added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.chargeStatusData = savedChargeStatusData;
          this.buildFormValues(this.chargeStatusData);

          this.alertService.showMessage("Charge Status saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Charge Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Charge Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Charge Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerChargeStatusReader(): boolean {
    return this.chargeStatusService.userIsSchedulerChargeStatusReader();
  }

  public userIsSchedulerChargeStatusWriter(): boolean {
    return this.chargeStatusService.userIsSchedulerChargeStatusWriter();
  }
}
