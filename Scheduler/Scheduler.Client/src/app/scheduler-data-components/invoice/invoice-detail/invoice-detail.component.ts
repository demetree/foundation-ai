/*
   GENERATED FORM FOR THE INVOICE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Invoice table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to invoice-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { InvoiceService, InvoiceData, InvoiceSubmitData } from '../../../scheduler-data-services/invoice.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { FinancialOfficeService } from '../../../scheduler-data-services/financial-office.service';
import { InvoiceStatusService } from '../../../scheduler-data-services/invoice-status.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { TaxCodeService } from '../../../scheduler-data-services/tax-code.service';
import { InvoiceChangeHistoryService } from '../../../scheduler-data-services/invoice-change-history.service';
import { InvoiceLineItemService } from '../../../scheduler-data-services/invoice-line-item.service';
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
interface InvoiceFormValues {
  invoiceNumber: string,
  clientId: number | bigint,       // For FK link number
  contactId: number | bigint | null,       // For FK link number
  scheduledEventId: number | bigint | null,       // For FK link number
  financialOfficeId: number | bigint | null,       // For FK link number
  invoiceStatusId: number | bigint,       // For FK link number
  currencyId: number | bigint,       // For FK link number
  taxCodeId: number | bigint | null,       // For FK link number
  invoiceDate: string,
  dueDate: string,
  subtotal: string,     // Stored as string for form input, converted to number on submit.
  taxAmount: string,     // Stored as string for form input, converted to number on submit.
  totalAmount: string,     // Stored as string for form input, converted to number on submit.
  amountPaid: string,     // Stored as string for form input, converted to number on submit.
  sentDate: string | null,
  paidDate: string | null,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-invoice-detail',
  templateUrl: './invoice-detail.component.html',
  styleUrls: ['./invoice-detail.component.scss']
})

export class InvoiceDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<InvoiceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public invoiceForm: FormGroup = this.fb.group({
        invoiceNumber: ['', Validators.required],
        clientId: [null, Validators.required],
        contactId: [null],
        scheduledEventId: [null],
        financialOfficeId: [null],
        invoiceStatusId: [null, Validators.required],
        currencyId: [null, Validators.required],
        taxCodeId: [null],
        invoiceDate: ['', Validators.required],
        dueDate: ['', Validators.required],
        subtotal: ['', Validators.required],
        taxAmount: ['', Validators.required],
        totalAmount: ['', Validators.required],
        amountPaid: ['', Validators.required],
        sentDate: [''],
        paidDate: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public invoiceId: string | null = null;
  public invoiceData: InvoiceData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  invoices$ = this.invoiceService.GetInvoiceList();
  public clients$ = this.clientService.GetClientList();
  public contacts$ = this.contactService.GetContactList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public financialOffices$ = this.financialOfficeService.GetFinancialOfficeList();
  public invoiceStatuses$ = this.invoiceStatusService.GetInvoiceStatusList();
  public currencies$ = this.currencyService.GetCurrencyList();
  public taxCodes$ = this.taxCodeService.GetTaxCodeList();
  public invoiceChangeHistories$ = this.invoiceChangeHistoryService.GetInvoiceChangeHistoryList();
  public invoiceLineItems$ = this.invoiceLineItemService.GetInvoiceLineItemList();
  public receipts$ = this.receiptService.GetReceiptList();
  public documents$ = this.documentService.GetDocumentList();

  private destroy$ = new Subject<void>();

  constructor(
    public invoiceService: InvoiceService,
    public clientService: ClientService,
    public contactService: ContactService,
    public scheduledEventService: ScheduledEventService,
    public financialOfficeService: FinancialOfficeService,
    public invoiceStatusService: InvoiceStatusService,
    public currencyService: CurrencyService,
    public taxCodeService: TaxCodeService,
    public invoiceChangeHistoryService: InvoiceChangeHistoryService,
    public invoiceLineItemService: InvoiceLineItemService,
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

    // Get the invoiceId from the route parameters
    this.invoiceId = this.route.snapshot.paramMap.get('invoiceId');

    if (this.invoiceId === 'new' ||
        this.invoiceId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.invoiceData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.invoiceForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.invoiceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Invoice';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Invoice';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.invoiceForm.dirty) {
      return confirm('You have unsaved Invoice changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.invoiceId != null && this.invoiceId !== 'new') {

      const id = parseInt(this.invoiceId, 10);

      if (!isNaN(id)) {
        return { invoiceId: id };
      }
    }

    return null;
  }


/*
  * Loads the Invoice data for the current invoiceId.
  *
  * Fully respects the InvoiceService caching strategy and error handling strategy.
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
    if (!this.invoiceService.userIsSchedulerInvoiceReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Invoices.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate invoiceId
    //
    if (!this.invoiceId) {

      this.alertService.showMessage('No Invoice ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const invoiceId = Number(this.invoiceId);

    if (isNaN(invoiceId) || invoiceId <= 0) {

      this.alertService.showMessage(`Invalid Invoice ID: "${this.invoiceId}"`,
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
      // This is the most targeted way: clear only this Invoice + relations

      this.invoiceService.ClearRecordCache(invoiceId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.invoiceService.GetInvoice(invoiceId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (invoiceData) => {

        //
        // Success path — invoiceData can legitimately be null if 404'd but request succeeded
        //
        if (!invoiceData) {

          this.handleInvoiceNotFound(invoiceId);

        } else {

          this.invoiceData = invoiceData;
          this.buildFormValues(this.invoiceData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Invoice loaded successfully',
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
        this.handleInvoiceLoadError(error, invoiceId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleInvoiceNotFound(invoiceId: number): void {

    this.invoiceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Invoice #${invoiceId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleInvoiceLoadError(error: any, invoiceId: number): void {

    let message = 'Failed to load Invoice.';
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
          message = 'You do not have permission to view this Invoice.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Invoice #${invoiceId} was not found.`;
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

    console.error(`Invoice load failed (ID: ${invoiceId})`, error);

    //
    // Reset UI to safe state
    //
    this.invoiceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(invoiceData: InvoiceData | null) {

    if (invoiceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.invoiceForm.reset({
        invoiceNumber: '',
        clientId: null,
        contactId: null,
        scheduledEventId: null,
        financialOfficeId: null,
        invoiceStatusId: null,
        currencyId: null,
        taxCodeId: null,
        invoiceDate: '',
        dueDate: '',
        subtotal: '',
        taxAmount: '',
        totalAmount: '',
        amountPaid: '',
        sentDate: '',
        paidDate: '',
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
        this.invoiceForm.reset({
        invoiceNumber: invoiceData.invoiceNumber ?? '',
        clientId: invoiceData.clientId,
        contactId: invoiceData.contactId,
        scheduledEventId: invoiceData.scheduledEventId,
        financialOfficeId: invoiceData.financialOfficeId,
        invoiceStatusId: invoiceData.invoiceStatusId,
        currencyId: invoiceData.currencyId,
        taxCodeId: invoiceData.taxCodeId,
        invoiceDate: isoUtcStringToDateTimeLocal(invoiceData.invoiceDate) ?? '',
        dueDate: isoUtcStringToDateTimeLocal(invoiceData.dueDate) ?? '',
        subtotal: invoiceData.subtotal?.toString() ?? '',
        taxAmount: invoiceData.taxAmount?.toString() ?? '',
        totalAmount: invoiceData.totalAmount?.toString() ?? '',
        amountPaid: invoiceData.amountPaid?.toString() ?? '',
        sentDate: isoUtcStringToDateTimeLocal(invoiceData.sentDate) ?? '',
        paidDate: isoUtcStringToDateTimeLocal(invoiceData.paidDate) ?? '',
        notes: invoiceData.notes ?? '',
        versionNumber: invoiceData.versionNumber?.toString() ?? '',
        active: invoiceData.active ?? true,
        deleted: invoiceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.invoiceForm.markAsPristine();
    this.invoiceForm.markAsUntouched();
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

    if (this.invoiceService.userIsSchedulerInvoiceWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Invoices", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.invoiceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.invoiceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.invoiceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const invoiceSubmitData: InvoiceSubmitData = {
        id: this.invoiceData?.id || 0,
        invoiceNumber: formValue.invoiceNumber!.trim(),
        clientId: Number(formValue.clientId),
        contactId: formValue.contactId ? Number(formValue.contactId) : null,
        scheduledEventId: formValue.scheduledEventId ? Number(formValue.scheduledEventId) : null,
        financialOfficeId: formValue.financialOfficeId ? Number(formValue.financialOfficeId) : null,
        invoiceStatusId: Number(formValue.invoiceStatusId),
        currencyId: Number(formValue.currencyId),
        taxCodeId: formValue.taxCodeId ? Number(formValue.taxCodeId) : null,
        invoiceDate: dateTimeLocalToIsoUtc(formValue.invoiceDate!.trim())!,
        dueDate: dateTimeLocalToIsoUtc(formValue.dueDate!.trim())!,
        subtotal: Number(formValue.subtotal),
        taxAmount: Number(formValue.taxAmount),
        totalAmount: Number(formValue.totalAmount),
        amountPaid: Number(formValue.amountPaid),
        sentDate: formValue.sentDate ? dateTimeLocalToIsoUtc(formValue.sentDate.trim()) : null,
        paidDate: formValue.paidDate ? dateTimeLocalToIsoUtc(formValue.paidDate.trim()) : null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.invoiceData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.invoiceService.PutInvoice(invoiceSubmitData.id, invoiceSubmitData)
      : this.invoiceService.PostInvoice(invoiceSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedInvoiceData) => {

        this.invoiceService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Invoice's detail page
          //
          this.invoiceForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.invoiceForm.markAsUntouched();

          this.router.navigate(['/invoices', savedInvoiceData.id]);
          this.alertService.showMessage('Invoice added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.invoiceData = savedInvoiceData;
          this.buildFormValues(this.invoiceData);

          this.alertService.showMessage("Invoice saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Invoice.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Invoice.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Invoice could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerInvoiceReader(): boolean {
    return this.invoiceService.userIsSchedulerInvoiceReader();
  }

  public userIsSchedulerInvoiceWriter(): boolean {
    return this.invoiceService.userIsSchedulerInvoiceWriter();
  }
}
