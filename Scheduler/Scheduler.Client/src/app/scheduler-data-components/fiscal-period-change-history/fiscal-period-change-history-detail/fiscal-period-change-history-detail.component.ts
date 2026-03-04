/*
   GENERATED FORM FOR THE FISCALPERIODCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from FiscalPeriodChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to fiscal-period-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FiscalPeriodChangeHistoryService, FiscalPeriodChangeHistoryData, FiscalPeriodChangeHistorySubmitData } from '../../../scheduler-data-services/fiscal-period-change-history.service';
import { FiscalPeriodService } from '../../../scheduler-data-services/fiscal-period.service';
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
interface FiscalPeriodChangeHistoryFormValues {
  fiscalPeriodId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-fiscal-period-change-history-detail',
  templateUrl: './fiscal-period-change-history-detail.component.html',
  styleUrls: ['./fiscal-period-change-history-detail.component.scss']
})

export class FiscalPeriodChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<FiscalPeriodChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public fiscalPeriodChangeHistoryForm: FormGroup = this.fb.group({
        fiscalPeriodId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public fiscalPeriodChangeHistoryId: string | null = null;
  public fiscalPeriodChangeHistoryData: FiscalPeriodChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  fiscalPeriodChangeHistories$ = this.fiscalPeriodChangeHistoryService.GetFiscalPeriodChangeHistoryList();
  public fiscalPeriods$ = this.fiscalPeriodService.GetFiscalPeriodList();

  private destroy$ = new Subject<void>();

  constructor(
    public fiscalPeriodChangeHistoryService: FiscalPeriodChangeHistoryService,
    public fiscalPeriodService: FiscalPeriodService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the fiscalPeriodChangeHistoryId from the route parameters
    this.fiscalPeriodChangeHistoryId = this.route.snapshot.paramMap.get('fiscalPeriodChangeHistoryId');

    if (this.fiscalPeriodChangeHistoryId === 'new' ||
        this.fiscalPeriodChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.fiscalPeriodChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.fiscalPeriodChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.fiscalPeriodChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Fiscal Period Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Fiscal Period Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.fiscalPeriodChangeHistoryForm.dirty) {
      return confirm('You have unsaved Fiscal Period Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.fiscalPeriodChangeHistoryId != null && this.fiscalPeriodChangeHistoryId !== 'new') {

      const id = parseInt(this.fiscalPeriodChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { fiscalPeriodChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the FiscalPeriodChangeHistory data for the current fiscalPeriodChangeHistoryId.
  *
  * Fully respects the FiscalPeriodChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.fiscalPeriodChangeHistoryService.userIsSchedulerFiscalPeriodChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read FiscalPeriodChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate fiscalPeriodChangeHistoryId
    //
    if (!this.fiscalPeriodChangeHistoryId) {

      this.alertService.showMessage('No FiscalPeriodChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const fiscalPeriodChangeHistoryId = Number(this.fiscalPeriodChangeHistoryId);

    if (isNaN(fiscalPeriodChangeHistoryId) || fiscalPeriodChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Fiscal Period Change History ID: "${this.fiscalPeriodChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this FiscalPeriodChangeHistory + relations

      this.fiscalPeriodChangeHistoryService.ClearRecordCache(fiscalPeriodChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.fiscalPeriodChangeHistoryService.GetFiscalPeriodChangeHistory(fiscalPeriodChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (fiscalPeriodChangeHistoryData) => {

        //
        // Success path — fiscalPeriodChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!fiscalPeriodChangeHistoryData) {

          this.handleFiscalPeriodChangeHistoryNotFound(fiscalPeriodChangeHistoryId);

        } else {

          this.fiscalPeriodChangeHistoryData = fiscalPeriodChangeHistoryData;
          this.buildFormValues(this.fiscalPeriodChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'FiscalPeriodChangeHistory loaded successfully',
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
        this.handleFiscalPeriodChangeHistoryLoadError(error, fiscalPeriodChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleFiscalPeriodChangeHistoryNotFound(fiscalPeriodChangeHistoryId: number): void {

    this.fiscalPeriodChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `FiscalPeriodChangeHistory #${fiscalPeriodChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleFiscalPeriodChangeHistoryLoadError(error: any, fiscalPeriodChangeHistoryId: number): void {

    let message = 'Failed to load Fiscal Period Change History.';
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
          message = 'You do not have permission to view this Fiscal Period Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Fiscal Period Change History #${fiscalPeriodChangeHistoryId} was not found.`;
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

    console.error(`Fiscal Period Change History load failed (ID: ${fiscalPeriodChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.fiscalPeriodChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(fiscalPeriodChangeHistoryData: FiscalPeriodChangeHistoryData | null) {

    if (fiscalPeriodChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.fiscalPeriodChangeHistoryForm.reset({
        fiscalPeriodId: null,
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
        this.fiscalPeriodChangeHistoryForm.reset({
        fiscalPeriodId: fiscalPeriodChangeHistoryData.fiscalPeriodId,
        versionNumber: fiscalPeriodChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(fiscalPeriodChangeHistoryData.timeStamp) ?? '',
        userId: fiscalPeriodChangeHistoryData.userId?.toString() ?? '',
        data: fiscalPeriodChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.fiscalPeriodChangeHistoryForm.markAsPristine();
    this.fiscalPeriodChangeHistoryForm.markAsUntouched();
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

    if (this.fiscalPeriodChangeHistoryService.userIsSchedulerFiscalPeriodChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Fiscal Period Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.fiscalPeriodChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.fiscalPeriodChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.fiscalPeriodChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const fiscalPeriodChangeHistorySubmitData: FiscalPeriodChangeHistorySubmitData = {
        id: this.fiscalPeriodChangeHistoryData?.id || 0,
        fiscalPeriodId: Number(formValue.fiscalPeriodId),
        versionNumber: this.fiscalPeriodChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.fiscalPeriodChangeHistoryService.PutFiscalPeriodChangeHistory(fiscalPeriodChangeHistorySubmitData.id, fiscalPeriodChangeHistorySubmitData)
      : this.fiscalPeriodChangeHistoryService.PostFiscalPeriodChangeHistory(fiscalPeriodChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedFiscalPeriodChangeHistoryData) => {

        this.fiscalPeriodChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Fiscal Period Change History's detail page
          //
          this.fiscalPeriodChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.fiscalPeriodChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/fiscalperiodchangehistories', savedFiscalPeriodChangeHistoryData.id]);
          this.alertService.showMessage('Fiscal Period Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.fiscalPeriodChangeHistoryData = savedFiscalPeriodChangeHistoryData;
          this.buildFormValues(this.fiscalPeriodChangeHistoryData);

          this.alertService.showMessage("Fiscal Period Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Fiscal Period Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Fiscal Period Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Fiscal Period Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerFiscalPeriodChangeHistoryReader(): boolean {
    return this.fiscalPeriodChangeHistoryService.userIsSchedulerFiscalPeriodChangeHistoryReader();
  }

  public userIsSchedulerFiscalPeriodChangeHistoryWriter(): boolean {
    return this.fiscalPeriodChangeHistoryService.userIsSchedulerFiscalPeriodChangeHistoryWriter();
  }
}
