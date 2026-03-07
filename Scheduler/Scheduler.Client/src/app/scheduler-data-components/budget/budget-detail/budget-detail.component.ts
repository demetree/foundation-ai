/*
   GENERATED FORM FOR THE BUDGET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Budget table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to budget-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BudgetService, BudgetData, BudgetSubmitData } from '../../../scheduler-data-services/budget.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { FiscalPeriodService } from '../../../scheduler-data-services/fiscal-period.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { BudgetChangeHistoryService } from '../../../scheduler-data-services/budget-change-history.service';
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
interface BudgetFormValues {
  financialCategoryId: number | bigint,       // For FK link number
  fiscalPeriodId: number | bigint,       // For FK link number
  budgetedAmount: string,     // Stored as string for form input, converted to number on submit.
  revisedAmount: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  currencyId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-budget-detail',
  templateUrl: './budget-detail.component.html',
  styleUrls: ['./budget-detail.component.scss']
})

export class BudgetDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BudgetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public budgetForm: FormGroup = this.fb.group({
        financialCategoryId: [null, Validators.required],
        fiscalPeriodId: [null, Validators.required],
        budgetedAmount: ['', Validators.required],
        revisedAmount: [''],
        notes: [''],
        currencyId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public budgetId: string | null = null;
  public budgetData: BudgetData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  budgets$ = this.budgetService.GetBudgetList();
  public financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();
  public fiscalPeriods$ = this.fiscalPeriodService.GetFiscalPeriodList();
  public currencies$ = this.currencyService.GetCurrencyList();
  public budgetChangeHistories$ = this.budgetChangeHistoryService.GetBudgetChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public budgetService: BudgetService,
    public financialCategoryService: FinancialCategoryService,
    public fiscalPeriodService: FiscalPeriodService,
    public currencyService: CurrencyService,
    public budgetChangeHistoryService: BudgetChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the budgetId from the route parameters
    this.budgetId = this.route.snapshot.paramMap.get('budgetId');

    if (this.budgetId === 'new' ||
        this.budgetId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.budgetData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.budgetForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.budgetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Budget';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Budget';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.budgetForm.dirty) {
      return confirm('You have unsaved Budget changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.budgetId != null && this.budgetId !== 'new') {

      const id = parseInt(this.budgetId, 10);

      if (!isNaN(id)) {
        return { budgetId: id };
      }
    }

    return null;
  }


/*
  * Loads the Budget data for the current budgetId.
  *
  * Fully respects the BudgetService caching strategy and error handling strategy.
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
    if (!this.budgetService.userIsSchedulerBudgetReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Budgets.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate budgetId
    //
    if (!this.budgetId) {

      this.alertService.showMessage('No Budget ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const budgetId = Number(this.budgetId);

    if (isNaN(budgetId) || budgetId <= 0) {

      this.alertService.showMessage(`Invalid Budget ID: "${this.budgetId}"`,
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
      // This is the most targeted way: clear only this Budget + relations

      this.budgetService.ClearRecordCache(budgetId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.budgetService.GetBudget(budgetId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (budgetData) => {

        //
        // Success path — budgetData can legitimately be null if 404'd but request succeeded
        //
        if (!budgetData) {

          this.handleBudgetNotFound(budgetId);

        } else {

          this.budgetData = budgetData;
          this.buildFormValues(this.budgetData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Budget loaded successfully',
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
        this.handleBudgetLoadError(error, budgetId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBudgetNotFound(budgetId: number): void {

    this.budgetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Budget #${budgetId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBudgetLoadError(error: any, budgetId: number): void {

    let message = 'Failed to load Budget.';
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
          message = 'You do not have permission to view this Budget.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Budget #${budgetId} was not found.`;
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

    console.error(`Budget load failed (ID: ${budgetId})`, error);

    //
    // Reset UI to safe state
    //
    this.budgetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(budgetData: BudgetData | null) {

    if (budgetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.budgetForm.reset({
        financialCategoryId: null,
        fiscalPeriodId: null,
        budgetedAmount: '',
        revisedAmount: '',
        notes: '',
        currencyId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.budgetForm.reset({
        financialCategoryId: budgetData.financialCategoryId,
        fiscalPeriodId: budgetData.fiscalPeriodId,
        budgetedAmount: budgetData.budgetedAmount?.toString() ?? '',
        revisedAmount: budgetData.revisedAmount?.toString() ?? '',
        notes: budgetData.notes ?? '',
        currencyId: budgetData.currencyId,
        versionNumber: budgetData.versionNumber?.toString() ?? '',
        active: budgetData.active ?? true,
        deleted: budgetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.budgetForm.markAsPristine();
    this.budgetForm.markAsUntouched();
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

    if (this.budgetService.userIsSchedulerBudgetWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Budgets", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.budgetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.budgetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.budgetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const budgetSubmitData: BudgetSubmitData = {
        id: this.budgetData?.id || 0,
        financialCategoryId: Number(formValue.financialCategoryId),
        fiscalPeriodId: Number(formValue.fiscalPeriodId),
        budgetedAmount: Number(formValue.budgetedAmount),
        revisedAmount: formValue.revisedAmount ? Number(formValue.revisedAmount) : null,
        notes: formValue.notes?.trim() || null,
        currencyId: Number(formValue.currencyId),
        versionNumber: this.budgetData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.budgetService.PutBudget(budgetSubmitData.id, budgetSubmitData)
      : this.budgetService.PostBudget(budgetSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBudgetData) => {

        this.budgetService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Budget's detail page
          //
          this.budgetForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.budgetForm.markAsUntouched();

          this.router.navigate(['/budgets', savedBudgetData.id]);
          this.alertService.showMessage('Budget added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.budgetData = savedBudgetData;
          this.buildFormValues(this.budgetData);

          this.alertService.showMessage("Budget saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Budget.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Budget.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Budget could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerBudgetReader(): boolean {
    return this.budgetService.userIsSchedulerBudgetReader();
  }

  public userIsSchedulerBudgetWriter(): boolean {
    return this.budgetService.userIsSchedulerBudgetWriter();
  }
}
