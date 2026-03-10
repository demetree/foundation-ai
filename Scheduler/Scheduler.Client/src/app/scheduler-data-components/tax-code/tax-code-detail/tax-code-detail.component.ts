/*
   GENERATED FORM FOR THE TAXCODE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TaxCode table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tax-code-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TaxCodeService, TaxCodeData, TaxCodeSubmitData } from '../../../scheduler-data-services/tax-code.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { InvoiceService } from '../../../scheduler-data-services/invoice.service';
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
interface TaxCodeFormValues {
  name: string,
  description: string,
  code: string,
  rate: string,     // Stored as string for form input, converted to number on submit.
  isDefault: boolean,
  isExempt: boolean,
  externalTaxCodeId: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-tax-code-detail',
  templateUrl: './tax-code-detail.component.html',
  styleUrls: ['./tax-code-detail.component.scss']
})

export class TaxCodeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TaxCodeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public taxCodeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        code: ['', Validators.required],
        rate: ['', Validators.required],
        isDefault: [false],
        isExempt: [false],
        externalTaxCodeId: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public taxCodeId: string | null = null;
  public taxCodeData: TaxCodeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  taxCodes$ = this.taxCodeService.GetTaxCodeList();
  public chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  public eventCharges$ = this.eventChargeService.GetEventChargeList();
  public financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public invoices$ = this.invoiceService.GetInvoiceList();

  private destroy$ = new Subject<void>();

  constructor(
    public taxCodeService: TaxCodeService,
    public chargeTypeService: ChargeTypeService,
    public eventChargeService: EventChargeService,
    public financialTransactionService: FinancialTransactionService,
    public invoiceService: InvoiceService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the taxCodeId from the route parameters
    this.taxCodeId = this.route.snapshot.paramMap.get('taxCodeId');

    if (this.taxCodeId === 'new' ||
        this.taxCodeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.taxCodeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.taxCodeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.taxCodeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Tax Code';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Tax Code';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.taxCodeForm.dirty) {
      return confirm('You have unsaved Tax Code changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.taxCodeId != null && this.taxCodeId !== 'new') {

      const id = parseInt(this.taxCodeId, 10);

      if (!isNaN(id)) {
        return { taxCodeId: id };
      }
    }

    return null;
  }


/*
  * Loads the TaxCode data for the current taxCodeId.
  *
  * Fully respects the TaxCodeService caching strategy and error handling strategy.
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
    if (!this.taxCodeService.userIsSchedulerTaxCodeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TaxCodes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate taxCodeId
    //
    if (!this.taxCodeId) {

      this.alertService.showMessage('No TaxCode ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const taxCodeId = Number(this.taxCodeId);

    if (isNaN(taxCodeId) || taxCodeId <= 0) {

      this.alertService.showMessage(`Invalid Tax Code ID: "${this.taxCodeId}"`,
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
      // This is the most targeted way: clear only this TaxCode + relations

      this.taxCodeService.ClearRecordCache(taxCodeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.taxCodeService.GetTaxCode(taxCodeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (taxCodeData) => {

        //
        // Success path — taxCodeData can legitimately be null if 404'd but request succeeded
        //
        if (!taxCodeData) {

          this.handleTaxCodeNotFound(taxCodeId);

        } else {

          this.taxCodeData = taxCodeData;
          this.buildFormValues(this.taxCodeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TaxCode loaded successfully',
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
        this.handleTaxCodeLoadError(error, taxCodeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTaxCodeNotFound(taxCodeId: number): void {

    this.taxCodeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TaxCode #${taxCodeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTaxCodeLoadError(error: any, taxCodeId: number): void {

    let message = 'Failed to load Tax Code.';
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
          message = 'You do not have permission to view this Tax Code.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Tax Code #${taxCodeId} was not found.`;
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

    console.error(`Tax Code load failed (ID: ${taxCodeId})`, error);

    //
    // Reset UI to safe state
    //
    this.taxCodeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(taxCodeData: TaxCodeData | null) {

    if (taxCodeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.taxCodeForm.reset({
        name: '',
        description: '',
        code: '',
        rate: '',
        isDefault: false,
        isExempt: false,
        externalTaxCodeId: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.taxCodeForm.reset({
        name: taxCodeData.name ?? '',
        description: taxCodeData.description ?? '',
        code: taxCodeData.code ?? '',
        rate: taxCodeData.rate?.toString() ?? '',
        isDefault: taxCodeData.isDefault ?? false,
        isExempt: taxCodeData.isExempt ?? false,
        externalTaxCodeId: taxCodeData.externalTaxCodeId ?? '',
        sequence: taxCodeData.sequence?.toString() ?? '',
        active: taxCodeData.active ?? true,
        deleted: taxCodeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.taxCodeForm.markAsPristine();
    this.taxCodeForm.markAsUntouched();
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

    if (this.taxCodeService.userIsSchedulerTaxCodeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Tax Codes", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.taxCodeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.taxCodeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.taxCodeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const taxCodeSubmitData: TaxCodeSubmitData = {
        id: this.taxCodeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        code: formValue.code!.trim(),
        rate: Number(formValue.rate),
        isDefault: !!formValue.isDefault,
        isExempt: !!formValue.isExempt,
        externalTaxCodeId: formValue.externalTaxCodeId?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.taxCodeService.PutTaxCode(taxCodeSubmitData.id, taxCodeSubmitData)
      : this.taxCodeService.PostTaxCode(taxCodeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTaxCodeData) => {

        this.taxCodeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Tax Code's detail page
          //
          this.taxCodeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.taxCodeForm.markAsUntouched();

          this.router.navigate(['/taxcodes', savedTaxCodeData.id]);
          this.alertService.showMessage('Tax Code added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.taxCodeData = savedTaxCodeData;
          this.buildFormValues(this.taxCodeData);

          this.alertService.showMessage("Tax Code saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Tax Code.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tax Code.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tax Code could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerTaxCodeReader(): boolean {
    return this.taxCodeService.userIsSchedulerTaxCodeReader();
  }

  public userIsSchedulerTaxCodeWriter(): boolean {
    return this.taxCodeService.userIsSchedulerTaxCodeWriter();
  }
}
