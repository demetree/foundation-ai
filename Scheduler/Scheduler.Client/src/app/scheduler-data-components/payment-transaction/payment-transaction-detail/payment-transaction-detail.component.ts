/*
   GENERATED FORM FOR THE PAYMENTTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PaymentTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to payment-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PaymentTransactionService, PaymentTransactionData, PaymentTransactionSubmitData } from '../../../scheduler-data-services/payment-transaction.service';
import { PaymentMethodService } from '../../../scheduler-data-services/payment-method.service';
import { PaymentProviderService } from '../../../scheduler-data-services/payment-provider.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { PaymentTransactionChangeHistoryService } from '../../../scheduler-data-services/payment-transaction-change-history.service';
import { ReceiptService } from '../../../scheduler-data-services/receipt.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
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
interface PaymentTransactionFormValues {
  paymentMethodId: number | bigint,       // For FK link number
  paymentProviderId: number | bigint | null,       // For FK link number
  scheduledEventId: number | bigint | null,       // For FK link number
  financialTransactionId: number | bigint | null,       // For FK link number
  eventChargeId: number | bigint | null,       // For FK link number
  transactionDate: string,
  amount: string,     // Stored as string for form input, converted to number on submit.
  processingFee: string,     // Stored as string for form input, converted to number on submit.
  netAmount: string,     // Stored as string for form input, converted to number on submit.
  currencyId: number | bigint,       // For FK link number
  status: string,
  providerTransactionId: string | null,
  providerResponse: string | null,
  payerName: string | null,
  payerEmail: string | null,
  payerPhone: string | null,
  receiptNumber: string | null,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-payment-transaction-detail',
  templateUrl: './payment-transaction-detail.component.html',
  styleUrls: ['./payment-transaction-detail.component.scss']
})

export class PaymentTransactionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PaymentTransactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public paymentTransactionForm: FormGroup = this.fb.group({
        paymentMethodId: [null, Validators.required],
        paymentProviderId: [null],
        scheduledEventId: [null],
        financialTransactionId: [null],
        eventChargeId: [null],
        transactionDate: ['', Validators.required],
        amount: ['', Validators.required],
        processingFee: ['', Validators.required],
        netAmount: ['', Validators.required],
        currencyId: [null, Validators.required],
        status: ['', Validators.required],
        providerTransactionId: [''],
        providerResponse: [''],
        payerName: [''],
        payerEmail: [''],
        payerPhone: [''],
        receiptNumber: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public paymentTransactionId: string | null = null;
  public paymentTransactionData: PaymentTransactionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();
  public paymentMethods$ = this.paymentMethodService.GetPaymentMethodList();
  public paymentProviders$ = this.paymentProviderService.GetPaymentProviderList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public eventCharges$ = this.eventChargeService.GetEventChargeList();
  public currencies$ = this.currencyService.GetCurrencyList();
  public paymentTransactionChangeHistories$ = this.paymentTransactionChangeHistoryService.GetPaymentTransactionChangeHistoryList();
  public receipts$ = this.receiptService.GetReceiptList();
  public documents$ = this.documentService.GetDocumentList();

  private destroy$ = new Subject<void>();

  constructor(
    public paymentTransactionService: PaymentTransactionService,
    public paymentMethodService: PaymentMethodService,
    public paymentProviderService: PaymentProviderService,
    public scheduledEventService: ScheduledEventService,
    public financialTransactionService: FinancialTransactionService,
    public eventChargeService: EventChargeService,
    public currencyService: CurrencyService,
    public paymentTransactionChangeHistoryService: PaymentTransactionChangeHistoryService,
    public receiptService: ReceiptService,
    public documentService: DocumentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the paymentTransactionId from the route parameters
    this.paymentTransactionId = this.route.snapshot.paramMap.get('paymentTransactionId');

    if (this.paymentTransactionId === 'new' ||
        this.paymentTransactionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.paymentTransactionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.paymentTransactionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.paymentTransactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Payment Transaction';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Payment Transaction';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.paymentTransactionForm.dirty) {
      return confirm('You have unsaved Payment Transaction changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.paymentTransactionId != null && this.paymentTransactionId !== 'new') {

      const id = parseInt(this.paymentTransactionId, 10);

      if (!isNaN(id)) {
        return { paymentTransactionId: id };
      }
    }

    return null;
  }


/*
  * Loads the PaymentTransaction data for the current paymentTransactionId.
  *
  * Fully respects the PaymentTransactionService caching strategy and error handling strategy.
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
    if (!this.paymentTransactionService.userIsSchedulerPaymentTransactionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PaymentTransactions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate paymentTransactionId
    //
    if (!this.paymentTransactionId) {

      this.alertService.showMessage('No PaymentTransaction ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const paymentTransactionId = Number(this.paymentTransactionId);

    if (isNaN(paymentTransactionId) || paymentTransactionId <= 0) {

      this.alertService.showMessage(`Invalid Payment Transaction ID: "${this.paymentTransactionId}"`,
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
      // This is the most targeted way: clear only this PaymentTransaction + relations

      this.paymentTransactionService.ClearRecordCache(paymentTransactionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.paymentTransactionService.GetPaymentTransaction(paymentTransactionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (paymentTransactionData) => {

        //
        // Success path — paymentTransactionData can legitimately be null if 404'd but request succeeded
        //
        if (!paymentTransactionData) {

          this.handlePaymentTransactionNotFound(paymentTransactionId);

        } else {

          this.paymentTransactionData = paymentTransactionData;
          this.buildFormValues(this.paymentTransactionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PaymentTransaction loaded successfully',
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
        this.handlePaymentTransactionLoadError(error, paymentTransactionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePaymentTransactionNotFound(paymentTransactionId: number): void {

    this.paymentTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PaymentTransaction #${paymentTransactionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePaymentTransactionLoadError(error: any, paymentTransactionId: number): void {

    let message = 'Failed to load Payment Transaction.';
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
          message = 'You do not have permission to view this Payment Transaction.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Payment Transaction #${paymentTransactionId} was not found.`;
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

    console.error(`Payment Transaction load failed (ID: ${paymentTransactionId})`, error);

    //
    // Reset UI to safe state
    //
    this.paymentTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(paymentTransactionData: PaymentTransactionData | null) {

    if (paymentTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.paymentTransactionForm.reset({
        paymentMethodId: null,
        paymentProviderId: null,
        scheduledEventId: null,
        financialTransactionId: null,
        eventChargeId: null,
        transactionDate: '',
        amount: '',
        processingFee: '',
        netAmount: '',
        currencyId: null,
        status: '',
        providerTransactionId: '',
        providerResponse: '',
        payerName: '',
        payerEmail: '',
        payerPhone: '',
        receiptNumber: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.paymentTransactionForm.reset({
        paymentMethodId: paymentTransactionData.paymentMethodId,
        paymentProviderId: paymentTransactionData.paymentProviderId,
        scheduledEventId: paymentTransactionData.scheduledEventId,
        financialTransactionId: paymentTransactionData.financialTransactionId,
        eventChargeId: paymentTransactionData.eventChargeId,
        transactionDate: isoUtcStringToDateTimeLocal(paymentTransactionData.transactionDate) ?? '',
        amount: paymentTransactionData.amount?.toString() ?? '',
        processingFee: paymentTransactionData.processingFee?.toString() ?? '',
        netAmount: paymentTransactionData.netAmount?.toString() ?? '',
        currencyId: paymentTransactionData.currencyId,
        status: paymentTransactionData.status ?? '',
        providerTransactionId: paymentTransactionData.providerTransactionId ?? '',
        providerResponse: paymentTransactionData.providerResponse ?? '',
        payerName: paymentTransactionData.payerName ?? '',
        payerEmail: paymentTransactionData.payerEmail ?? '',
        payerPhone: paymentTransactionData.payerPhone ?? '',
        receiptNumber: paymentTransactionData.receiptNumber ?? '',
        notes: paymentTransactionData.notes ?? '',
        versionNumber: paymentTransactionData.versionNumber?.toString() ?? '',
        active: paymentTransactionData.active ?? true,
        deleted: paymentTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.paymentTransactionForm.markAsPristine();
    this.paymentTransactionForm.markAsUntouched();
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

    if (this.paymentTransactionService.userIsSchedulerPaymentTransactionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Payment Transactions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.paymentTransactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.paymentTransactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.paymentTransactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const paymentTransactionSubmitData: PaymentTransactionSubmitData = {
        id: this.paymentTransactionData?.id || 0,
        paymentMethodId: Number(formValue.paymentMethodId),
        paymentProviderId: formValue.paymentProviderId ? Number(formValue.paymentProviderId) : null,
        scheduledEventId: formValue.scheduledEventId ? Number(formValue.scheduledEventId) : null,
        financialTransactionId: formValue.financialTransactionId ? Number(formValue.financialTransactionId) : null,
        eventChargeId: formValue.eventChargeId ? Number(formValue.eventChargeId) : null,
        transactionDate: dateTimeLocalToIsoUtc(formValue.transactionDate!.trim())!,
        amount: Number(formValue.amount),
        processingFee: Number(formValue.processingFee),
        netAmount: Number(formValue.netAmount),
        currencyId: Number(formValue.currencyId),
        status: formValue.status!.trim(),
        providerTransactionId: formValue.providerTransactionId?.trim() || null,
        providerResponse: formValue.providerResponse?.trim() || null,
        payerName: formValue.payerName?.trim() || null,
        payerEmail: formValue.payerEmail?.trim() || null,
        payerPhone: formValue.payerPhone?.trim() || null,
        receiptNumber: formValue.receiptNumber?.trim() || null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.paymentTransactionData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.paymentTransactionService.PutPaymentTransaction(paymentTransactionSubmitData.id, paymentTransactionSubmitData)
      : this.paymentTransactionService.PostPaymentTransaction(paymentTransactionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPaymentTransactionData) => {

        this.paymentTransactionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Payment Transaction's detail page
          //
          this.paymentTransactionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.paymentTransactionForm.markAsUntouched();

          this.router.navigate(['/paymenttransactions', savedPaymentTransactionData.id]);
          this.alertService.showMessage('Payment Transaction added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.paymentTransactionData = savedPaymentTransactionData;
          this.buildFormValues(this.paymentTransactionData);

          this.alertService.showMessage("Payment Transaction saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Payment Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerPaymentTransactionReader(): boolean {
    return this.paymentTransactionService.userIsSchedulerPaymentTransactionReader();
  }

  public userIsSchedulerPaymentTransactionWriter(): boolean {
    return this.paymentTransactionService.userIsSchedulerPaymentTransactionWriter();
  }
}
