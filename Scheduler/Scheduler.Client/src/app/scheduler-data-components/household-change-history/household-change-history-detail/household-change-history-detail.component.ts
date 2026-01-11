/*
   GENERATED FORM FOR THE HOUSEHOLDCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from HouseholdChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to household-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { HouseholdChangeHistoryService, HouseholdChangeHistoryData, HouseholdChangeHistorySubmitData } from '../../../scheduler-data-services/household-change-history.service';
import { HouseholdService } from '../../../scheduler-data-services/household.service';
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
interface HouseholdChangeHistoryFormValues {
  householdId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-household-change-history-detail',
  templateUrl: './household-change-history-detail.component.html',
  styleUrls: ['./household-change-history-detail.component.scss']
})

export class HouseholdChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<HouseholdChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public householdChangeHistoryForm: FormGroup = this.fb.group({
        householdId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public householdChangeHistoryId: string | null = null;
  public householdChangeHistoryData: HouseholdChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  householdChangeHistories$ = this.householdChangeHistoryService.GetHouseholdChangeHistoryList();
  public households$ = this.householdService.GetHouseholdList();

  private destroy$ = new Subject<void>();

  constructor(
    public householdChangeHistoryService: HouseholdChangeHistoryService,
    public householdService: HouseholdService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the householdChangeHistoryId from the route parameters
    this.householdChangeHistoryId = this.route.snapshot.paramMap.get('householdChangeHistoryId');

    if (this.householdChangeHistoryId === 'new' ||
        this.householdChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.householdChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.householdChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.householdChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Household Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Household Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.householdChangeHistoryForm.dirty) {
      return confirm('You have unsaved Household Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.householdChangeHistoryId != null && this.householdChangeHistoryId !== 'new') {

      const id = parseInt(this.householdChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { householdChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the HouseholdChangeHistory data for the current householdChangeHistoryId.
  *
  * Fully respects the HouseholdChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.householdChangeHistoryService.userIsSchedulerHouseholdChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read HouseholdChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate householdChangeHistoryId
    //
    if (!this.householdChangeHistoryId) {

      this.alertService.showMessage('No HouseholdChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const householdChangeHistoryId = Number(this.householdChangeHistoryId);

    if (isNaN(householdChangeHistoryId) || householdChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Household Change History ID: "${this.householdChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this HouseholdChangeHistory + relations

      this.householdChangeHistoryService.ClearRecordCache(householdChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.householdChangeHistoryService.GetHouseholdChangeHistory(householdChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (householdChangeHistoryData) => {

        //
        // Success path — householdChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!householdChangeHistoryData) {

          this.handleHouseholdChangeHistoryNotFound(householdChangeHistoryId);

        } else {

          this.householdChangeHistoryData = householdChangeHistoryData;
          this.buildFormValues(this.householdChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'HouseholdChangeHistory loaded successfully',
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
        this.handleHouseholdChangeHistoryLoadError(error, householdChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleHouseholdChangeHistoryNotFound(householdChangeHistoryId: number): void {

    this.householdChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `HouseholdChangeHistory #${householdChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleHouseholdChangeHistoryLoadError(error: any, householdChangeHistoryId: number): void {

    let message = 'Failed to load Household Change History.';
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
          message = 'You do not have permission to view this Household Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Household Change History #${householdChangeHistoryId} was not found.`;
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

    console.error(`Household Change History load failed (ID: ${householdChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.householdChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(householdChangeHistoryData: HouseholdChangeHistoryData | null) {

    if (householdChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.householdChangeHistoryForm.reset({
        householdId: null,
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
        this.householdChangeHistoryForm.reset({
        householdId: householdChangeHistoryData.householdId,
        versionNumber: householdChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(householdChangeHistoryData.timeStamp) ?? '',
        userId: householdChangeHistoryData.userId?.toString() ?? '',
        data: householdChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.householdChangeHistoryForm.markAsPristine();
    this.householdChangeHistoryForm.markAsUntouched();
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

    if (this.householdChangeHistoryService.userIsSchedulerHouseholdChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Household Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.householdChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.householdChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.householdChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const householdChangeHistorySubmitData: HouseholdChangeHistorySubmitData = {
        id: this.householdChangeHistoryData?.id || 0,
        householdId: Number(formValue.householdId),
        versionNumber: this.householdChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.householdChangeHistoryService.PutHouseholdChangeHistory(householdChangeHistorySubmitData.id, householdChangeHistorySubmitData)
      : this.householdChangeHistoryService.PostHouseholdChangeHistory(householdChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedHouseholdChangeHistoryData) => {

        this.householdChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Household Change History's detail page
          //
          this.householdChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.householdChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/householdchangehistories', savedHouseholdChangeHistoryData.id]);
          this.alertService.showMessage('Household Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.householdChangeHistoryData = savedHouseholdChangeHistoryData;
          this.buildFormValues(this.householdChangeHistoryData);

          this.alertService.showMessage("Household Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Household Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Household Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Household Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerHouseholdChangeHistoryReader(): boolean {
    return this.householdChangeHistoryService.userIsSchedulerHouseholdChangeHistoryReader();
  }

  public userIsSchedulerHouseholdChangeHistoryWriter(): boolean {
    return this.householdChangeHistoryService.userIsSchedulerHouseholdChangeHistoryWriter();
  }
}
