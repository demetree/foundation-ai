/*
   GENERATED FORM FOR THE RECEIPT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Receipt table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to receipt-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ReceiptService, ReceiptData, ReceiptSubmitData } from '../../../scheduler-data-services/receipt.service';
import { ReceiptTypeService } from '../../../scheduler-data-services/receipt-type.service';
import { InvoiceService } from '../../../scheduler-data-services/invoice.service';
import { PaymentTransactionService } from '../../../scheduler-data-services/payment-transaction.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { ReceiptChangeHistoryService } from '../../../scheduler-data-services/receipt-change-history.service';
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
interface ReceiptFormValues {
  receiptNumber: string,
  receiptTypeId: number | bigint,       // For FK link number
  invoiceId: number | bigint | null,       // For FK link number
  paymentTransactionId: number | bigint | null,       // For FK link number
  financialTransactionId: number | bigint | null,       // For FK link number
  clientId: number | bigint | null,       // For FK link number
  contactId: number | bigint | null,       // For FK link number
  currencyId: number | bigint,       // For FK link number
  receiptDate: string,
  amount: string,     // Stored as string for form input, converted to number on submit.
  paymentMethod: string | null,
  description: string | null,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-receipt-detail',
  templateUrl: './receipt-detail.component.html',
  styleUrls: ['./receipt-detail.component.scss']
})

export class ReceiptDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ReceiptFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public receiptForm: FormGroup = this.fb.group({
        receiptNumber: ['', Validators.required],
        receiptTypeId: [null, Validators.required],
        invoiceId: [null],
        paymentTransactionId: [null],
        financialTransactionId: [null],
        clientId: [null],
        contactId: [null],
        currencyId: [null, Validators.required],
        receiptDate: ['', Validators.required],
        amount: ['', Validators.required],
        paymentMethod: [''],
        description: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public receiptId: string | null = null;
  public receiptData: ReceiptData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  receipts$ = this.receiptService.GetReceiptList();
  public receiptTypes$ = this.receiptTypeService.GetReceiptTypeList();
  public invoices$ = this.invoiceService.GetInvoiceList();
  public paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();
  public financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public clients$ = this.clientService.GetClientList();
  public contacts$ = this.contactService.GetContactList();
  public currencies$ = this.currencyService.GetCurrencyList();
  public receiptChangeHistories$ = this.receiptChangeHistoryService.GetReceiptChangeHistoryList();
  public documents$ = this.documentService.GetDocumentList();

  private destroy$ = new Subject<void>();

  constructor(
    public receiptService: ReceiptService,
    public receiptTypeService: ReceiptTypeService,
    public invoiceService: InvoiceService,
    public paymentTransactionService: PaymentTransactionService,
    public financialTransactionService: FinancialTransactionService,
    public clientService: ClientService,
    public contactService: ContactService,
    public currencyService: CurrencyService,
    public receiptChangeHistoryService: ReceiptChangeHistoryService,
    public documentService: DocumentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the receiptId from the route parameters
    this.receiptId = this.route.snapshot.paramMap.get('receiptId');

    if (this.receiptId === 'new' ||
        this.receiptId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.receiptData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.receiptForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.receiptForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Receipt';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Receipt';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.receiptForm.dirty) {
      return confirm('You have unsaved Receipt changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.receiptId != null && this.receiptId !== 'new') {

      const id = parseInt(this.receiptId, 10);

      if (!isNaN(id)) {
        return { receiptId: id };
      }
    }

    return null;
  }


/*
  * Loads the Receipt data for the current receiptId.
  *
  * Fully respects the ReceiptService caching strategy and error handling strategy.
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
    if (!this.receiptService.userIsSchedulerReceiptReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Receipts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate receiptId
    //
    if (!this.receiptId) {

      this.alertService.showMessage('No Receipt ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const receiptId = Number(this.receiptId);

    if (isNaN(receiptId) || receiptId <= 0) {

      this.alertService.showMessage(`Invalid Receipt ID: "${this.receiptId}"`,
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
      // This is the most targeted way: clear only this Receipt + relations

      this.receiptService.ClearRecordCache(receiptId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.receiptService.GetReceipt(receiptId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (receiptData) => {

        //
        // Success path — receiptData can legitimately be null if 404'd but request succeeded
        //
        if (!receiptData) {

          this.handleReceiptNotFound(receiptId);

        } else {

          this.receiptData = receiptData;
          this.buildFormValues(this.receiptData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Receipt loaded successfully',
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
        this.handleReceiptLoadError(error, receiptId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleReceiptNotFound(receiptId: number): void {

    this.receiptData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Receipt #${receiptId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleReceiptLoadError(error: any, receiptId: number): void {

    let message = 'Failed to load Receipt.';
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
          message = 'You do not have permission to view this Receipt.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Receipt #${receiptId} was not found.`;
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

    console.error(`Receipt load failed (ID: ${receiptId})`, error);

    //
    // Reset UI to safe state
    //
    this.receiptData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(receiptData: ReceiptData | null) {

    if (receiptData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.receiptForm.reset({
        receiptNumber: '',
        receiptTypeId: null,
        invoiceId: null,
        paymentTransactionId: null,
        financialTransactionId: null,
        clientId: null,
        contactId: null,
        currencyId: null,
        receiptDate: '',
        amount: '',
        paymentMethod: '',
        description: '',
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
        this.receiptForm.reset({
        receiptNumber: receiptData.receiptNumber ?? '',
        receiptTypeId: receiptData.receiptTypeId,
        invoiceId: receiptData.invoiceId,
        paymentTransactionId: receiptData.paymentTransactionId,
        financialTransactionId: receiptData.financialTransactionId,
        clientId: receiptData.clientId,
        contactId: receiptData.contactId,
        currencyId: receiptData.currencyId,
        receiptDate: isoUtcStringToDateTimeLocal(receiptData.receiptDate) ?? '',
        amount: receiptData.amount?.toString() ?? '',
        paymentMethod: receiptData.paymentMethod ?? '',
        description: receiptData.description ?? '',
        notes: receiptData.notes ?? '',
        versionNumber: receiptData.versionNumber?.toString() ?? '',
        active: receiptData.active ?? true,
        deleted: receiptData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.receiptForm.markAsPristine();
    this.receiptForm.markAsUntouched();
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

    if (this.receiptService.userIsSchedulerReceiptWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Receipts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.receiptForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.receiptForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.receiptForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const receiptSubmitData: ReceiptSubmitData = {
        id: this.receiptData?.id || 0,
        receiptNumber: formValue.receiptNumber!.trim(),
        receiptTypeId: Number(formValue.receiptTypeId),
        invoiceId: formValue.invoiceId ? Number(formValue.invoiceId) : null,
        paymentTransactionId: formValue.paymentTransactionId ? Number(formValue.paymentTransactionId) : null,
        financialTransactionId: formValue.financialTransactionId ? Number(formValue.financialTransactionId) : null,
        clientId: formValue.clientId ? Number(formValue.clientId) : null,
        contactId: formValue.contactId ? Number(formValue.contactId) : null,
        currencyId: Number(formValue.currencyId),
        receiptDate: dateTimeLocalToIsoUtc(formValue.receiptDate!.trim())!,
        amount: Number(formValue.amount),
        paymentMethod: formValue.paymentMethod?.trim() || null,
        description: formValue.description?.trim() || null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.receiptData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.receiptService.PutReceipt(receiptSubmitData.id, receiptSubmitData)
      : this.receiptService.PostReceipt(receiptSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedReceiptData) => {

        this.receiptService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Receipt's detail page
          //
          this.receiptForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.receiptForm.markAsUntouched();

          this.router.navigate(['/receipts', savedReceiptData.id]);
          this.alertService.showMessage('Receipt added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.receiptData = savedReceiptData;
          this.buildFormValues(this.receiptData);

          this.alertService.showMessage("Receipt saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Receipt.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Receipt.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Receipt could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerReceiptReader(): boolean {
    return this.receiptService.userIsSchedulerReceiptReader();
  }

  public userIsSchedulerReceiptWriter(): boolean {
    return this.receiptService.userIsSchedulerReceiptWriter();
  }
}
