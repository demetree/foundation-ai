/*
   GENERATED FORM FOR THE FINANCIALTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from FinancialTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to financial-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FinancialTransactionService, FinancialTransactionData, FinancialTransactionSubmitData } from '../../../scheduler-data-services/financial-transaction.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { TaxCodeService } from '../../../scheduler-data-services/tax-code.service';
import { FiscalPeriodService } from '../../../scheduler-data-services/fiscal-period.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { FinancialTransactionChangeHistoryService } from '../../../scheduler-data-services/financial-transaction-change-history.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
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
interface FinancialTransactionFormValues {
  financialCategoryId: number | bigint,       // For FK link number
  scheduledEventId: number | bigint | null,       // For FK link number
  contactId: number | bigint | null,       // For FK link number
  contactRole: string | null,
  taxCodeId: number | bigint | null,       // For FK link number
  fiscalPeriodId: number | bigint | null,       // For FK link number
  transactionDate: string,
  description: string,
  amount: string,     // Stored as string for form input, converted to number on submit.
  taxAmount: string,     // Stored as string for form input, converted to number on submit.
  totalAmount: string,     // Stored as string for form input, converted to number on submit.
  isRevenue: boolean,
  journalEntryType: string | null,
  paymentMethod: string | null,
  referenceNumber: string | null,
  notes: string | null,
  currencyId: number | bigint,       // For FK link number
  exportedDate: string | null,
  externalId: string | null,
  externalSystemName: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-financial-transaction-detail',
  templateUrl: './financial-transaction-detail.component.html',
  styleUrls: ['./financial-transaction-detail.component.scss']
})

export class FinancialTransactionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<FinancialTransactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public financialTransactionForm: FormGroup = this.fb.group({
        financialCategoryId: [null, Validators.required],
        scheduledEventId: [null],
        contactId: [null],
        contactRole: [''],
        taxCodeId: [null],
        fiscalPeriodId: [null],
        transactionDate: ['', Validators.required],
        description: ['', Validators.required],
        amount: ['', Validators.required],
        taxAmount: ['', Validators.required],
        totalAmount: ['', Validators.required],
        isRevenue: [false],
        journalEntryType: [''],
        paymentMethod: [''],
        referenceNumber: [''],
        notes: [''],
        currencyId: [null, Validators.required],
        exportedDate: [''],
        externalId: [''],
        externalSystemName: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public financialTransactionId: string | null = null;
  public financialTransactionData: FinancialTransactionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public contacts$ = this.contactService.GetContactList();
  public taxCodes$ = this.taxCodeService.GetTaxCodeList();
  public fiscalPeriods$ = this.fiscalPeriodService.GetFiscalPeriodList();
  public currencies$ = this.currencyService.GetCurrencyList();
  public financialTransactionChangeHistories$ = this.financialTransactionChangeHistoryService.GetFinancialTransactionChangeHistoryList();
  public documents$ = this.documentService.GetDocumentList();
  public paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();

  private destroy$ = new Subject<void>();

  constructor(
    public financialTransactionService: FinancialTransactionService,
    public financialCategoryService: FinancialCategoryService,
    public scheduledEventService: ScheduledEventService,
    public contactService: ContactService,
    public taxCodeService: TaxCodeService,
    public fiscalPeriodService: FiscalPeriodService,
    public currencyService: CurrencyService,
    public financialTransactionChangeHistoryService: FinancialTransactionChangeHistoryService,
    public documentService: DocumentService,
    public paymentTransactionService: PaymentTransactionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the financialTransactionId from the route parameters
    this.financialTransactionId = this.route.snapshot.paramMap.get('financialTransactionId');

    if (this.financialTransactionId === 'new' ||
        this.financialTransactionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.financialTransactionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.financialTransactionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.financialTransactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Financial Transaction';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Financial Transaction';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.financialTransactionForm.dirty) {
      return confirm('You have unsaved Financial Transaction changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.financialTransactionId != null && this.financialTransactionId !== 'new') {

      const id = parseInt(this.financialTransactionId, 10);

      if (!isNaN(id)) {
        return { financialTransactionId: id };
      }
    }

    return null;
  }


/*
  * Loads the FinancialTransaction data for the current financialTransactionId.
  *
  * Fully respects the FinancialTransactionService caching strategy and error handling strategy.
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
    if (!this.financialTransactionService.userIsSchedulerFinancialTransactionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read FinancialTransactions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate financialTransactionId
    //
    if (!this.financialTransactionId) {

      this.alertService.showMessage('No FinancialTransaction ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const financialTransactionId = Number(this.financialTransactionId);

    if (isNaN(financialTransactionId) || financialTransactionId <= 0) {

      this.alertService.showMessage(`Invalid Financial Transaction ID: "${this.financialTransactionId}"`,
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
      // This is the most targeted way: clear only this FinancialTransaction + relations

      this.financialTransactionService.ClearRecordCache(financialTransactionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.financialTransactionService.GetFinancialTransaction(financialTransactionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (financialTransactionData) => {

        //
        // Success path — financialTransactionData can legitimately be null if 404'd but request succeeded
        //
        if (!financialTransactionData) {

          this.handleFinancialTransactionNotFound(financialTransactionId);

        } else {

          this.financialTransactionData = financialTransactionData;
          this.buildFormValues(this.financialTransactionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'FinancialTransaction loaded successfully',
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
        this.handleFinancialTransactionLoadError(error, financialTransactionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleFinancialTransactionNotFound(financialTransactionId: number): void {

    this.financialTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `FinancialTransaction #${financialTransactionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleFinancialTransactionLoadError(error: any, financialTransactionId: number): void {

    let message = 'Failed to load Financial Transaction.';
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
          message = 'You do not have permission to view this Financial Transaction.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Financial Transaction #${financialTransactionId} was not found.`;
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

    console.error(`Financial Transaction load failed (ID: ${financialTransactionId})`, error);

    //
    // Reset UI to safe state
    //
    this.financialTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(financialTransactionData: FinancialTransactionData | null) {

    if (financialTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.financialTransactionForm.reset({
        financialCategoryId: null,
        scheduledEventId: null,
        contactId: null,
        contactRole: '',
        taxCodeId: null,
        fiscalPeriodId: null,
        transactionDate: '',
        description: '',
        amount: '',
        taxAmount: '',
        totalAmount: '',
        isRevenue: false,
        journalEntryType: '',
        paymentMethod: '',
        referenceNumber: '',
        notes: '',
        currencyId: null,
        exportedDate: '',
        externalId: '',
        externalSystemName: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.financialTransactionForm.reset({
        financialCategoryId: financialTransactionData.financialCategoryId,
        scheduledEventId: financialTransactionData.scheduledEventId,
        contactId: financialTransactionData.contactId,
        contactRole: financialTransactionData.contactRole ?? '',
        taxCodeId: financialTransactionData.taxCodeId,
        fiscalPeriodId: financialTransactionData.fiscalPeriodId,
        transactionDate: isoUtcStringToDateTimeLocal(financialTransactionData.transactionDate) ?? '',
        description: financialTransactionData.description ?? '',
        amount: financialTransactionData.amount?.toString() ?? '',
        taxAmount: financialTransactionData.taxAmount?.toString() ?? '',
        totalAmount: financialTransactionData.totalAmount?.toString() ?? '',
        isRevenue: financialTransactionData.isRevenue ?? false,
        journalEntryType: financialTransactionData.journalEntryType ?? '',
        paymentMethod: financialTransactionData.paymentMethod ?? '',
        referenceNumber: financialTransactionData.referenceNumber ?? '',
        notes: financialTransactionData.notes ?? '',
        currencyId: financialTransactionData.currencyId,
        exportedDate: isoUtcStringToDateTimeLocal(financialTransactionData.exportedDate) ?? '',
        externalId: financialTransactionData.externalId ?? '',
        externalSystemName: financialTransactionData.externalSystemName ?? '',
        versionNumber: financialTransactionData.versionNumber?.toString() ?? '',
        active: financialTransactionData.active ?? true,
        deleted: financialTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.financialTransactionForm.markAsPristine();
    this.financialTransactionForm.markAsUntouched();
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

    if (this.financialTransactionService.userIsSchedulerFinancialTransactionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Financial Transactions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.financialTransactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.financialTransactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.financialTransactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const financialTransactionSubmitData: FinancialTransactionSubmitData = {
        id: this.financialTransactionData?.id || 0,
        financialCategoryId: Number(formValue.financialCategoryId),
        scheduledEventId: formValue.scheduledEventId ? Number(formValue.scheduledEventId) : null,
        contactId: formValue.contactId ? Number(formValue.contactId) : null,
        contactRole: formValue.contactRole?.trim() || null,
        taxCodeId: formValue.taxCodeId ? Number(formValue.taxCodeId) : null,
        fiscalPeriodId: formValue.fiscalPeriodId ? Number(formValue.fiscalPeriodId) : null,
        transactionDate: dateTimeLocalToIsoUtc(formValue.transactionDate!.trim())!,
        description: formValue.description!.trim(),
        amount: Number(formValue.amount),
        taxAmount: Number(formValue.taxAmount),
        totalAmount: Number(formValue.totalAmount),
        isRevenue: !!formValue.isRevenue,
        journalEntryType: formValue.journalEntryType?.trim() || null,
        paymentMethod: formValue.paymentMethod?.trim() || null,
        referenceNumber: formValue.referenceNumber?.trim() || null,
        notes: formValue.notes?.trim() || null,
        currencyId: Number(formValue.currencyId),
        exportedDate: formValue.exportedDate ? dateTimeLocalToIsoUtc(formValue.exportedDate.trim()) : null,
        externalId: formValue.externalId?.trim() || null,
        externalSystemName: formValue.externalSystemName?.trim() || null,
        versionNumber: this.financialTransactionData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.financialTransactionService.PutFinancialTransaction(financialTransactionSubmitData.id, financialTransactionSubmitData)
      : this.financialTransactionService.PostFinancialTransaction(financialTransactionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedFinancialTransactionData) => {

        this.financialTransactionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Financial Transaction's detail page
          //
          this.financialTransactionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.financialTransactionForm.markAsUntouched();

          this.router.navigate(['/financialtransactions', savedFinancialTransactionData.id]);
          this.alertService.showMessage('Financial Transaction added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.financialTransactionData = savedFinancialTransactionData;
          this.buildFormValues(this.financialTransactionData);

          this.alertService.showMessage("Financial Transaction saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Financial Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Financial Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Financial Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerFinancialTransactionReader(): boolean {
    return this.financialTransactionService.userIsSchedulerFinancialTransactionReader();
  }

  public userIsSchedulerFinancialTransactionWriter(): boolean {
    return this.financialTransactionService.userIsSchedulerFinancialTransactionWriter();
  }
}
