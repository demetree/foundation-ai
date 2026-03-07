/*
   GENERATED FORM FOR THE CHARGESTATUSCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ChargeStatusChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to charge-status-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ChargeStatusChangeHistoryService, ChargeStatusChangeHistoryData, ChargeStatusChangeHistorySubmitData } from '../../../scheduler-data-services/charge-status-change-history.service';
import { ChargeStatusService } from '../../../scheduler-data-services/charge-status.service';
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
interface ChargeStatusChangeHistoryFormValues {
  chargeStatusId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-charge-status-change-history-detail',
  templateUrl: './charge-status-change-history-detail.component.html',
  styleUrls: ['./charge-status-change-history-detail.component.scss']
})

export class ChargeStatusChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ChargeStatusChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public chargeStatusChangeHistoryForm: FormGroup = this.fb.group({
        chargeStatusId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public chargeStatusChangeHistoryId: string | null = null;
  public chargeStatusChangeHistoryData: ChargeStatusChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  chargeStatusChangeHistories$ = this.chargeStatusChangeHistoryService.GetChargeStatusChangeHistoryList();
  public chargeStatuses$ = this.chargeStatusService.GetChargeStatusList();

  private destroy$ = new Subject<void>();

  constructor(
    public chargeStatusChangeHistoryService: ChargeStatusChangeHistoryService,
    public chargeStatusService: ChargeStatusService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the chargeStatusChangeHistoryId from the route parameters
    this.chargeStatusChangeHistoryId = this.route.snapshot.paramMap.get('chargeStatusChangeHistoryId');

    if (this.chargeStatusChangeHistoryId === 'new' ||
        this.chargeStatusChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.chargeStatusChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.chargeStatusChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.chargeStatusChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Charge Status Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Charge Status Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.chargeStatusChangeHistoryForm.dirty) {
      return confirm('You have unsaved Charge Status Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.chargeStatusChangeHistoryId != null && this.chargeStatusChangeHistoryId !== 'new') {

      const id = parseInt(this.chargeStatusChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { chargeStatusChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the ChargeStatusChangeHistory data for the current chargeStatusChangeHistoryId.
  *
  * Fully respects the ChargeStatusChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.chargeStatusChangeHistoryService.userIsSchedulerChargeStatusChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ChargeStatusChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate chargeStatusChangeHistoryId
    //
    if (!this.chargeStatusChangeHistoryId) {

      this.alertService.showMessage('No ChargeStatusChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const chargeStatusChangeHistoryId = Number(this.chargeStatusChangeHistoryId);

    if (isNaN(chargeStatusChangeHistoryId) || chargeStatusChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Charge Status Change History ID: "${this.chargeStatusChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this ChargeStatusChangeHistory + relations

      this.chargeStatusChangeHistoryService.ClearRecordCache(chargeStatusChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.chargeStatusChangeHistoryService.GetChargeStatusChangeHistory(chargeStatusChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (chargeStatusChangeHistoryData) => {

        //
        // Success path — chargeStatusChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!chargeStatusChangeHistoryData) {

          this.handleChargeStatusChangeHistoryNotFound(chargeStatusChangeHistoryId);

        } else {

          this.chargeStatusChangeHistoryData = chargeStatusChangeHistoryData;
          this.buildFormValues(this.chargeStatusChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ChargeStatusChangeHistory loaded successfully',
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
        this.handleChargeStatusChangeHistoryLoadError(error, chargeStatusChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleChargeStatusChangeHistoryNotFound(chargeStatusChangeHistoryId: number): void {

    this.chargeStatusChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ChargeStatusChangeHistory #${chargeStatusChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleChargeStatusChangeHistoryLoadError(error: any, chargeStatusChangeHistoryId: number): void {

    let message = 'Failed to load Charge Status Change History.';
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
          message = 'You do not have permission to view this Charge Status Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Charge Status Change History #${chargeStatusChangeHistoryId} was not found.`;
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

    console.error(`Charge Status Change History load failed (ID: ${chargeStatusChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.chargeStatusChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(chargeStatusChangeHistoryData: ChargeStatusChangeHistoryData | null) {

    if (chargeStatusChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.chargeStatusChangeHistoryForm.reset({
        chargeStatusId: null,
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
        this.chargeStatusChangeHistoryForm.reset({
        chargeStatusId: chargeStatusChangeHistoryData.chargeStatusId,
        versionNumber: chargeStatusChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(chargeStatusChangeHistoryData.timeStamp) ?? '',
        userId: chargeStatusChangeHistoryData.userId?.toString() ?? '',
        data: chargeStatusChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.chargeStatusChangeHistoryForm.markAsPristine();
    this.chargeStatusChangeHistoryForm.markAsUntouched();
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

    if (this.chargeStatusChangeHistoryService.userIsSchedulerChargeStatusChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Charge Status Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.chargeStatusChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.chargeStatusChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.chargeStatusChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const chargeStatusChangeHistorySubmitData: ChargeStatusChangeHistorySubmitData = {
        id: this.chargeStatusChangeHistoryData?.id || 0,
        chargeStatusId: Number(formValue.chargeStatusId),
        versionNumber: this.chargeStatusChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.chargeStatusChangeHistoryService.PutChargeStatusChangeHistory(chargeStatusChangeHistorySubmitData.id, chargeStatusChangeHistorySubmitData)
      : this.chargeStatusChangeHistoryService.PostChargeStatusChangeHistory(chargeStatusChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedChargeStatusChangeHistoryData) => {

        this.chargeStatusChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Charge Status Change History's detail page
          //
          this.chargeStatusChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.chargeStatusChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/chargestatuschangehistories', savedChargeStatusChangeHistoryData.id]);
          this.alertService.showMessage('Charge Status Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.chargeStatusChangeHistoryData = savedChargeStatusChangeHistoryData;
          this.buildFormValues(this.chargeStatusChangeHistoryData);

          this.alertService.showMessage("Charge Status Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Charge Status Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Charge Status Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Charge Status Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerChargeStatusChangeHistoryReader(): boolean {
    return this.chargeStatusChangeHistoryService.userIsSchedulerChargeStatusChangeHistoryReader();
  }

  public userIsSchedulerChargeStatusChangeHistoryWriter(): boolean {
    return this.chargeStatusChangeHistoryService.userIsSchedulerChargeStatusChangeHistoryWriter();
  }
}
