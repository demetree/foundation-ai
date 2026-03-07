/*
   GENERATED FORM FOR THE FINANCIALCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from FinancialCategory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to financial-category-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FinancialCategoryService, FinancialCategoryData, FinancialCategorySubmitData } from '../../../scheduler-data-services/financial-category.service';
import { AccountTypeService } from '../../../scheduler-data-services/account-type.service';
import { FinancialCategoryChangeHistoryService } from '../../../scheduler-data-services/financial-category-change-history.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
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
interface FinancialCategoryFormValues {
  name: string,
  description: string,
  code: string,
  accountTypeId: number | bigint,       // For FK link number
  parentFinancialCategoryId: number | bigint | null,       // For FK link number
  isTaxApplicable: boolean,
  defaultAmount: string | null,     // Stored as string for form input, converted to number on submit.
  externalAccountId: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-financial-category-detail',
  templateUrl: './financial-category-detail.component.html',
  styleUrls: ['./financial-category-detail.component.scss']
})

export class FinancialCategoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<FinancialCategoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public financialCategoryForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        code: ['', Validators.required],
        accountTypeId: [null, Validators.required],
        parentFinancialCategoryId: [null],
        isTaxApplicable: [false],
        defaultAmount: [''],
        externalAccountId: [''],
        sequence: [''],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public financialCategoryId: string | null = null;
  public financialCategoryData: FinancialCategoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();
  public accountTypes$ = this.accountTypeService.GetAccountTypeList();
  public financialCategoryChangeHistories$ = this.financialCategoryChangeHistoryService.GetFinancialCategoryChangeHistoryList();
  public chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  public financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public budgets$ = this.budgetService.GetBudgetList();

  private destroy$ = new Subject<void>();

  constructor(
    public financialCategoryService: FinancialCategoryService,
    public accountTypeService: AccountTypeService,
    public financialCategoryChangeHistoryService: FinancialCategoryChangeHistoryService,
    public chargeTypeService: ChargeTypeService,
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

    // Get the financialCategoryId from the route parameters
    this.financialCategoryId = this.route.snapshot.paramMap.get('financialCategoryId');

    if (this.financialCategoryId === 'new' ||
        this.financialCategoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.financialCategoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.financialCategoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.financialCategoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Financial Category';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Financial Category';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.financialCategoryForm.dirty) {
      return confirm('You have unsaved Financial Category changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.financialCategoryId != null && this.financialCategoryId !== 'new') {

      const id = parseInt(this.financialCategoryId, 10);

      if (!isNaN(id)) {
        return { financialCategoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the FinancialCategory data for the current financialCategoryId.
  *
  * Fully respects the FinancialCategoryService caching strategy and error handling strategy.
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
    if (!this.financialCategoryService.userIsSchedulerFinancialCategoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read FinancialCategories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate financialCategoryId
    //
    if (!this.financialCategoryId) {

      this.alertService.showMessage('No FinancialCategory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const financialCategoryId = Number(this.financialCategoryId);

    if (isNaN(financialCategoryId) || financialCategoryId <= 0) {

      this.alertService.showMessage(`Invalid Financial Category ID: "${this.financialCategoryId}"`,
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
      // This is the most targeted way: clear only this FinancialCategory + relations

      this.financialCategoryService.ClearRecordCache(financialCategoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.financialCategoryService.GetFinancialCategory(financialCategoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (financialCategoryData) => {

        //
        // Success path — financialCategoryData can legitimately be null if 404'd but request succeeded
        //
        if (!financialCategoryData) {

          this.handleFinancialCategoryNotFound(financialCategoryId);

        } else {

          this.financialCategoryData = financialCategoryData;
          this.buildFormValues(this.financialCategoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'FinancialCategory loaded successfully',
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
        this.handleFinancialCategoryLoadError(error, financialCategoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleFinancialCategoryNotFound(financialCategoryId: number): void {

    this.financialCategoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `FinancialCategory #${financialCategoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleFinancialCategoryLoadError(error: any, financialCategoryId: number): void {

    let message = 'Failed to load Financial Category.';
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
          message = 'You do not have permission to view this Financial Category.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Financial Category #${financialCategoryId} was not found.`;
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

    console.error(`Financial Category load failed (ID: ${financialCategoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.financialCategoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(financialCategoryData: FinancialCategoryData | null) {

    if (financialCategoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.financialCategoryForm.reset({
        name: '',
        description: '',
        code: '',
        accountTypeId: null,
        parentFinancialCategoryId: null,
        isTaxApplicable: false,
        defaultAmount: '',
        externalAccountId: '',
        sequence: '',
        color: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.financialCategoryForm.reset({
        name: financialCategoryData.name ?? '',
        description: financialCategoryData.description ?? '',
        code: financialCategoryData.code ?? '',
        accountTypeId: financialCategoryData.accountTypeId,
        parentFinancialCategoryId: financialCategoryData.parentFinancialCategoryId,
        isTaxApplicable: financialCategoryData.isTaxApplicable ?? false,
        defaultAmount: financialCategoryData.defaultAmount?.toString() ?? '',
        externalAccountId: financialCategoryData.externalAccountId ?? '',
        sequence: financialCategoryData.sequence?.toString() ?? '',
        color: financialCategoryData.color ?? '',
        versionNumber: financialCategoryData.versionNumber?.toString() ?? '',
        active: financialCategoryData.active ?? true,
        deleted: financialCategoryData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.financialCategoryForm.markAsPristine();
    this.financialCategoryForm.markAsUntouched();
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

    if (this.financialCategoryService.userIsSchedulerFinancialCategoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Financial Categories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.financialCategoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.financialCategoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.financialCategoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const financialCategorySubmitData: FinancialCategorySubmitData = {
        id: this.financialCategoryData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        code: formValue.code!.trim(),
        accountTypeId: Number(formValue.accountTypeId),
        parentFinancialCategoryId: formValue.parentFinancialCategoryId ? Number(formValue.parentFinancialCategoryId) : null,
        isTaxApplicable: !!formValue.isTaxApplicable,
        defaultAmount: formValue.defaultAmount ? Number(formValue.defaultAmount) : null,
        externalAccountId: formValue.externalAccountId?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.financialCategoryData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.financialCategoryService.PutFinancialCategory(financialCategorySubmitData.id, financialCategorySubmitData)
      : this.financialCategoryService.PostFinancialCategory(financialCategorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedFinancialCategoryData) => {

        this.financialCategoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Financial Category's detail page
          //
          this.financialCategoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.financialCategoryForm.markAsUntouched();

          this.router.navigate(['/financialcategories', savedFinancialCategoryData.id]);
          this.alertService.showMessage('Financial Category added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.financialCategoryData = savedFinancialCategoryData;
          this.buildFormValues(this.financialCategoryData);

          this.alertService.showMessage("Financial Category saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Financial Category.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Financial Category.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Financial Category could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerFinancialCategoryReader(): boolean {
    return this.financialCategoryService.userIsSchedulerFinancialCategoryReader();
  }

  public userIsSchedulerFinancialCategoryWriter(): boolean {
    return this.financialCategoryService.userIsSchedulerFinancialCategoryWriter();
  }
}
