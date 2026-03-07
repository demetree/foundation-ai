/*
   GENERATED FORM FOR THE FISCALPERIOD TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from FiscalPeriod table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to fiscal-period-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FiscalPeriodService, FiscalPeriodData, FiscalPeriodSubmitData } from '../../../scheduler-data-services/fiscal-period.service';
import { PeriodStatusService } from '../../../scheduler-data-services/period-status.service';
import { FiscalPeriodChangeHistoryService } from '../../../scheduler-data-services/fiscal-period-change-history.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { BudgetService } from '../../../scheduler-data-services/budget.service';
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
interface FiscalPeriodFormValues {
  name: string,
  description: string,
  startDate: string,
  endDate: string,
  periodType: string,
  fiscalYear: string,     // Stored as string for form input, converted to number on submit.
  periodNumber: string,     // Stored as string for form input, converted to number on submit.
  periodStatusId: number | bigint,       // For FK link number
  closedDate: string | null,
  closedBy: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-fiscal-period-detail',
  templateUrl: './fiscal-period-detail.component.html',
  styleUrls: ['./fiscal-period-detail.component.scss']
})

export class FiscalPeriodDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<FiscalPeriodFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public fiscalPeriodForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        startDate: ['', Validators.required],
        endDate: ['', Validators.required],
        periodType: ['', Validators.required],
        fiscalYear: ['', Validators.required],
        periodNumber: ['', Validators.required],
        periodStatusId: [null, Validators.required],
        closedDate: [''],
        closedBy: [''],
        sequence: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public fiscalPeriodId: string | null = null;
  public fiscalPeriodData: FiscalPeriodData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  fiscalPeriods$ = this.fiscalPeriodService.GetFiscalPeriodList();
  public periodStatuses$ = this.periodStatusService.GetPeriodStatusList();
  public fiscalPeriodChangeHistories$ = this.fiscalPeriodChangeHistoryService.GetFiscalPeriodChangeHistoryList();
  public financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public budgets$ = this.budgetService.GetBudgetList();

  private destroy$ = new Subject<void>();

  constructor(
    public fiscalPeriodService: FiscalPeriodService,
    public periodStatusService: PeriodStatusService,
    public fiscalPeriodChangeHistoryService: FiscalPeriodChangeHistoryService,
    public financialTransactionService: FinancialTransactionService,
    public budgetService: BudgetService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the fiscalPeriodId from the route parameters
    this.fiscalPeriodId = this.route.snapshot.paramMap.get('fiscalPeriodId');

    if (this.fiscalPeriodId === 'new' ||
        this.fiscalPeriodId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.fiscalPeriodData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.fiscalPeriodForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.fiscalPeriodForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Fiscal Period';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Fiscal Period';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.fiscalPeriodForm.dirty) {
      return confirm('You have unsaved Fiscal Period changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.fiscalPeriodId != null && this.fiscalPeriodId !== 'new') {

      const id = parseInt(this.fiscalPeriodId, 10);

      if (!isNaN(id)) {
        return { fiscalPeriodId: id };
      }
    }

    return null;
  }


/*
  * Loads the FiscalPeriod data for the current fiscalPeriodId.
  *
  * Fully respects the FiscalPeriodService caching strategy and error handling strategy.
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
    if (!this.fiscalPeriodService.userIsSchedulerFiscalPeriodReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read FiscalPeriods.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate fiscalPeriodId
    //
    if (!this.fiscalPeriodId) {

      this.alertService.showMessage('No FiscalPeriod ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const fiscalPeriodId = Number(this.fiscalPeriodId);

    if (isNaN(fiscalPeriodId) || fiscalPeriodId <= 0) {

      this.alertService.showMessage(`Invalid Fiscal Period ID: "${this.fiscalPeriodId}"`,
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
      // This is the most targeted way: clear only this FiscalPeriod + relations

      this.fiscalPeriodService.ClearRecordCache(fiscalPeriodId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.fiscalPeriodService.GetFiscalPeriod(fiscalPeriodId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (fiscalPeriodData) => {

        //
        // Success path — fiscalPeriodData can legitimately be null if 404'd but request succeeded
        //
        if (!fiscalPeriodData) {

          this.handleFiscalPeriodNotFound(fiscalPeriodId);

        } else {

          this.fiscalPeriodData = fiscalPeriodData;
          this.buildFormValues(this.fiscalPeriodData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'FiscalPeriod loaded successfully',
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
        this.handleFiscalPeriodLoadError(error, fiscalPeriodId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleFiscalPeriodNotFound(fiscalPeriodId: number): void {

    this.fiscalPeriodData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `FiscalPeriod #${fiscalPeriodId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleFiscalPeriodLoadError(error: any, fiscalPeriodId: number): void {

    let message = 'Failed to load Fiscal Period.';
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
          message = 'You do not have permission to view this Fiscal Period.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Fiscal Period #${fiscalPeriodId} was not found.`;
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

    console.error(`Fiscal Period load failed (ID: ${fiscalPeriodId})`, error);

    //
    // Reset UI to safe state
    //
    this.fiscalPeriodData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(fiscalPeriodData: FiscalPeriodData | null) {

    if (fiscalPeriodData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.fiscalPeriodForm.reset({
        name: '',
        description: '',
        startDate: '',
        endDate: '',
        periodType: '',
        fiscalYear: '',
        periodNumber: '',
        periodStatusId: null,
        closedDate: '',
        closedBy: '',
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
        this.fiscalPeriodForm.reset({
        name: fiscalPeriodData.name ?? '',
        description: fiscalPeriodData.description ?? '',
        startDate: isoUtcStringToDateTimeLocal(fiscalPeriodData.startDate) ?? '',
        endDate: isoUtcStringToDateTimeLocal(fiscalPeriodData.endDate) ?? '',
        periodType: fiscalPeriodData.periodType ?? '',
        fiscalYear: fiscalPeriodData.fiscalYear?.toString() ?? '',
        periodNumber: fiscalPeriodData.periodNumber?.toString() ?? '',
        periodStatusId: fiscalPeriodData.periodStatusId,
        closedDate: isoUtcStringToDateTimeLocal(fiscalPeriodData.closedDate) ?? '',
        closedBy: fiscalPeriodData.closedBy ?? '',
        sequence: fiscalPeriodData.sequence?.toString() ?? '',
        versionNumber: fiscalPeriodData.versionNumber?.toString() ?? '',
        active: fiscalPeriodData.active ?? true,
        deleted: fiscalPeriodData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.fiscalPeriodForm.markAsPristine();
    this.fiscalPeriodForm.markAsUntouched();
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

    if (this.fiscalPeriodService.userIsSchedulerFiscalPeriodWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Fiscal Periods", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.fiscalPeriodForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.fiscalPeriodForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.fiscalPeriodForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const fiscalPeriodSubmitData: FiscalPeriodSubmitData = {
        id: this.fiscalPeriodData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        startDate: dateTimeLocalToIsoUtc(formValue.startDate!.trim())!,
        endDate: dateTimeLocalToIsoUtc(formValue.endDate!.trim())!,
        periodType: formValue.periodType!.trim(),
        fiscalYear: Number(formValue.fiscalYear),
        periodNumber: Number(formValue.periodNumber),
        periodStatusId: Number(formValue.periodStatusId),
        closedDate: formValue.closedDate ? dateTimeLocalToIsoUtc(formValue.closedDate.trim()) : null,
        closedBy: formValue.closedBy?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        versionNumber: this.fiscalPeriodData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.fiscalPeriodService.PutFiscalPeriod(fiscalPeriodSubmitData.id, fiscalPeriodSubmitData)
      : this.fiscalPeriodService.PostFiscalPeriod(fiscalPeriodSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedFiscalPeriodData) => {

        this.fiscalPeriodService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Fiscal Period's detail page
          //
          this.fiscalPeriodForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.fiscalPeriodForm.markAsUntouched();

          this.router.navigate(['/fiscalperiods', savedFiscalPeriodData.id]);
          this.alertService.showMessage('Fiscal Period added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.fiscalPeriodData = savedFiscalPeriodData;
          this.buildFormValues(this.fiscalPeriodData);

          this.alertService.showMessage("Fiscal Period saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Fiscal Period.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Fiscal Period.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Fiscal Period could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerFiscalPeriodReader(): boolean {
    return this.fiscalPeriodService.userIsSchedulerFiscalPeriodReader();
  }

  public userIsSchedulerFiscalPeriodWriter(): boolean {
    return this.fiscalPeriodService.userIsSchedulerFiscalPeriodWriter();
  }
}
