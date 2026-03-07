/*
   GENERATED FORM FOR THE ACCOUNTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AccountType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to account-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AccountTypeService, AccountTypeData, AccountTypeSubmitData } from '../../../scheduler-data-services/account-type.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
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
interface AccountTypeFormValues {
  name: string,
  description: string,
  isRevenue: boolean,
  externalMapping: string | null,
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-account-type-detail',
  templateUrl: './account-type-detail.component.html',
  styleUrls: ['./account-type-detail.component.scss']
})

export class AccountTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AccountTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public accountTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        isRevenue: [false],
        externalMapping: [''],
        color: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public accountTypeId: string | null = null;
  public accountTypeData: AccountTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  accountTypes$ = this.accountTypeService.GetAccountTypeList();
  public financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public accountTypeService: AccountTypeService,
    public financialCategoryService: FinancialCategoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the accountTypeId from the route parameters
    this.accountTypeId = this.route.snapshot.paramMap.get('accountTypeId');

    if (this.accountTypeId === 'new' ||
        this.accountTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.accountTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.accountTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.accountTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Account Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Account Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.accountTypeForm.dirty) {
      return confirm('You have unsaved Account Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.accountTypeId != null && this.accountTypeId !== 'new') {

      const id = parseInt(this.accountTypeId, 10);

      if (!isNaN(id)) {
        return { accountTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the AccountType data for the current accountTypeId.
  *
  * Fully respects the AccountTypeService caching strategy and error handling strategy.
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
    if (!this.accountTypeService.userIsSchedulerAccountTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AccountTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate accountTypeId
    //
    if (!this.accountTypeId) {

      this.alertService.showMessage('No AccountType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const accountTypeId = Number(this.accountTypeId);

    if (isNaN(accountTypeId) || accountTypeId <= 0) {

      this.alertService.showMessage(`Invalid Account Type ID: "${this.accountTypeId}"`,
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
      // This is the most targeted way: clear only this AccountType + relations

      this.accountTypeService.ClearRecordCache(accountTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.accountTypeService.GetAccountType(accountTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (accountTypeData) => {

        //
        // Success path — accountTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!accountTypeData) {

          this.handleAccountTypeNotFound(accountTypeId);

        } else {

          this.accountTypeData = accountTypeData;
          this.buildFormValues(this.accountTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AccountType loaded successfully',
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
        this.handleAccountTypeLoadError(error, accountTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAccountTypeNotFound(accountTypeId: number): void {

    this.accountTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AccountType #${accountTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAccountTypeLoadError(error: any, accountTypeId: number): void {

    let message = 'Failed to load Account Type.';
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
          message = 'You do not have permission to view this Account Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Account Type #${accountTypeId} was not found.`;
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

    console.error(`Account Type load failed (ID: ${accountTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.accountTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(accountTypeData: AccountTypeData | null) {

    if (accountTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.accountTypeForm.reset({
        name: '',
        description: '',
        isRevenue: false,
        externalMapping: '',
        color: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.accountTypeForm.reset({
        name: accountTypeData.name ?? '',
        description: accountTypeData.description ?? '',
        isRevenue: accountTypeData.isRevenue ?? false,
        externalMapping: accountTypeData.externalMapping ?? '',
        color: accountTypeData.color ?? '',
        sequence: accountTypeData.sequence?.toString() ?? '',
        active: accountTypeData.active ?? true,
        deleted: accountTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.accountTypeForm.markAsPristine();
    this.accountTypeForm.markAsUntouched();
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

    if (this.accountTypeService.userIsSchedulerAccountTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Account Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.accountTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.accountTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.accountTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const accountTypeSubmitData: AccountTypeSubmitData = {
        id: this.accountTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        isRevenue: !!formValue.isRevenue,
        externalMapping: formValue.externalMapping?.trim() || null,
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.accountTypeService.PutAccountType(accountTypeSubmitData.id, accountTypeSubmitData)
      : this.accountTypeService.PostAccountType(accountTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAccountTypeData) => {

        this.accountTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Account Type's detail page
          //
          this.accountTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.accountTypeForm.markAsUntouched();

          this.router.navigate(['/accounttypes', savedAccountTypeData.id]);
          this.alertService.showMessage('Account Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.accountTypeData = savedAccountTypeData;
          this.buildFormValues(this.accountTypeData);

          this.alertService.showMessage("Account Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Account Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Account Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Account Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerAccountTypeReader(): boolean {
    return this.accountTypeService.userIsSchedulerAccountTypeReader();
  }

  public userIsSchedulerAccountTypeWriter(): boolean {
    return this.accountTypeService.userIsSchedulerAccountTypeWriter();
  }
}
