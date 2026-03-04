/*
   GENERATED FORM FOR THE CURRENCY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Currency table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to currency-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CurrencyService, CurrencyData, CurrencySubmitData } from '../../../scheduler-data-services/currency.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { PaymentTransactionService } from '../../../scheduler-data-services/payment-transaction.service';
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
interface CurrencyFormValues {
  name: string,
  description: string,
  code: string,
  color: string | null,
  isDefault: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-currency-detail',
  templateUrl: './currency-detail.component.html',
  styleUrls: ['./currency-detail.component.scss']
})

export class CurrencyDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CurrencyFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public currencyForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        code: ['', Validators.required],
        color: [''],
        isDefault: [false],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public currencyId: string | null = null;
  public currencyData: CurrencyData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  currencies$ = this.currencyService.GetCurrencyList();
  public chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  public offices$ = this.officeService.GetOfficeList();
  public clients$ = this.clientService.GetClientList();
  public rateSheets$ = this.rateSheetService.GetRateSheetList();
  public eventCharges$ = this.eventChargeService.GetEventChargeList();
  public financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();

  private destroy$ = new Subject<void>();

  constructor(
    public currencyService: CurrencyService,
    public chargeTypeService: ChargeTypeService,
    public officeService: OfficeService,
    public clientService: ClientService,
    public rateSheetService: RateSheetService,
    public eventChargeService: EventChargeService,
    public financialTransactionService: FinancialTransactionService,
    public paymentTransactionService: PaymentTransactionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the currencyId from the route parameters
    this.currencyId = this.route.snapshot.paramMap.get('currencyId');

    if (this.currencyId === 'new' ||
        this.currencyId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.currencyData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.currencyForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.currencyForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Currency';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Currency';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.currencyForm.dirty) {
      return confirm('You have unsaved Currency changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.currencyId != null && this.currencyId !== 'new') {

      const id = parseInt(this.currencyId, 10);

      if (!isNaN(id)) {
        return { currencyId: id };
      }
    }

    return null;
  }


/*
  * Loads the Currency data for the current currencyId.
  *
  * Fully respects the CurrencyService caching strategy and error handling strategy.
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
    if (!this.currencyService.userIsSchedulerCurrencyReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Currencies.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate currencyId
    //
    if (!this.currencyId) {

      this.alertService.showMessage('No Currency ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const currencyId = Number(this.currencyId);

    if (isNaN(currencyId) || currencyId <= 0) {

      this.alertService.showMessage(`Invalid Currency ID: "${this.currencyId}"`,
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
      // This is the most targeted way: clear only this Currency + relations

      this.currencyService.ClearRecordCache(currencyId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.currencyService.GetCurrency(currencyId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (currencyData) => {

        //
        // Success path — currencyData can legitimately be null if 404'd but request succeeded
        //
        if (!currencyData) {

          this.handleCurrencyNotFound(currencyId);

        } else {

          this.currencyData = currencyData;
          this.buildFormValues(this.currencyData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Currency loaded successfully',
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
        this.handleCurrencyLoadError(error, currencyId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleCurrencyNotFound(currencyId: number): void {

    this.currencyData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Currency #${currencyId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleCurrencyLoadError(error: any, currencyId: number): void {

    let message = 'Failed to load Currency.';
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
          message = 'You do not have permission to view this Currency.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Currency #${currencyId} was not found.`;
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

    console.error(`Currency load failed (ID: ${currencyId})`, error);

    //
    // Reset UI to safe state
    //
    this.currencyData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(currencyData: CurrencyData | null) {

    if (currencyData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.currencyForm.reset({
        name: '',
        description: '',
        code: '',
        color: '',
        isDefault: false,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.currencyForm.reset({
        name: currencyData.name ?? '',
        description: currencyData.description ?? '',
        code: currencyData.code ?? '',
        color: currencyData.color ?? '',
        isDefault: currencyData.isDefault ?? false,
        sequence: currencyData.sequence?.toString() ?? '',
        active: currencyData.active ?? true,
        deleted: currencyData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.currencyForm.markAsPristine();
    this.currencyForm.markAsUntouched();
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

    if (this.currencyService.userIsSchedulerCurrencyWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Currencies", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.currencyForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.currencyForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.currencyForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const currencySubmitData: CurrencySubmitData = {
        id: this.currencyData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        code: formValue.code!.trim(),
        color: formValue.color?.trim() || null,
        isDefault: !!formValue.isDefault,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.currencyService.PutCurrency(currencySubmitData.id, currencySubmitData)
      : this.currencyService.PostCurrency(currencySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedCurrencyData) => {

        this.currencyService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Currency's detail page
          //
          this.currencyForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.currencyForm.markAsUntouched();

          this.router.navigate(['/currencies', savedCurrencyData.id]);
          this.alertService.showMessage('Currency added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.currencyData = savedCurrencyData;
          this.buildFormValues(this.currencyData);

          this.alertService.showMessage("Currency saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Currency.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Currency.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Currency could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerCurrencyReader(): boolean {
    return this.currencyService.userIsSchedulerCurrencyReader();
  }

  public userIsSchedulerCurrencyWriter(): boolean {
    return this.currencyService.userIsSchedulerCurrencyWriter();
  }
}
