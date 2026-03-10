/*
   GENERATED FORM FOR THE INVOICELINEITEM TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from InvoiceLineItem table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to invoice-line-item-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { InvoiceLineItemService, InvoiceLineItemData, InvoiceLineItemSubmitData } from '../../../scheduler-data-services/invoice-line-item.service';
import { InvoiceService } from '../../../scheduler-data-services/invoice.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
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
interface InvoiceLineItemFormValues {
  invoiceId: number | bigint,       // For FK link number
  eventChargeId: number | bigint | null,       // For FK link number
  financialCategoryId: number | bigint | null,       // For FK link number
  description: string,
  quantity: string,     // Stored as string for form input, converted to number on submit.
  unitPrice: string,     // Stored as string for form input, converted to number on submit.
  amount: string,     // Stored as string for form input, converted to number on submit.
  taxAmount: string,     // Stored as string for form input, converted to number on submit.
  totalAmount: string,     // Stored as string for form input, converted to number on submit.
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-invoice-line-item-detail',
  templateUrl: './invoice-line-item-detail.component.html',
  styleUrls: ['./invoice-line-item-detail.component.scss']
})

export class InvoiceLineItemDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<InvoiceLineItemFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public invoiceLineItemForm: FormGroup = this.fb.group({
        invoiceId: [null, Validators.required],
        eventChargeId: [null],
        financialCategoryId: [null],
        description: ['', Validators.required],
        quantity: ['', Validators.required],
        unitPrice: ['', Validators.required],
        amount: ['', Validators.required],
        taxAmount: ['', Validators.required],
        totalAmount: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public invoiceLineItemId: string | null = null;
  public invoiceLineItemData: InvoiceLineItemData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  invoiceLineItems$ = this.invoiceLineItemService.GetInvoiceLineItemList();
  public invoices$ = this.invoiceService.GetInvoiceList();
  public eventCharges$ = this.eventChargeService.GetEventChargeList();
  public financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public invoiceLineItemService: InvoiceLineItemService,
    public invoiceService: InvoiceService,
    public eventChargeService: EventChargeService,
    public financialCategoryService: FinancialCategoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the invoiceLineItemId from the route parameters
    this.invoiceLineItemId = this.route.snapshot.paramMap.get('invoiceLineItemId');

    if (this.invoiceLineItemId === 'new' ||
        this.invoiceLineItemId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.invoiceLineItemData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.invoiceLineItemForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.invoiceLineItemForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Invoice Line Item';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Invoice Line Item';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.invoiceLineItemForm.dirty) {
      return confirm('You have unsaved Invoice Line Item changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.invoiceLineItemId != null && this.invoiceLineItemId !== 'new') {

      const id = parseInt(this.invoiceLineItemId, 10);

      if (!isNaN(id)) {
        return { invoiceLineItemId: id };
      }
    }

    return null;
  }


/*
  * Loads the InvoiceLineItem data for the current invoiceLineItemId.
  *
  * Fully respects the InvoiceLineItemService caching strategy and error handling strategy.
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
    if (!this.invoiceLineItemService.userIsSchedulerInvoiceLineItemReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read InvoiceLineItems.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate invoiceLineItemId
    //
    if (!this.invoiceLineItemId) {

      this.alertService.showMessage('No InvoiceLineItem ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const invoiceLineItemId = Number(this.invoiceLineItemId);

    if (isNaN(invoiceLineItemId) || invoiceLineItemId <= 0) {

      this.alertService.showMessage(`Invalid Invoice Line Item ID: "${this.invoiceLineItemId}"`,
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
      // This is the most targeted way: clear only this InvoiceLineItem + relations

      this.invoiceLineItemService.ClearRecordCache(invoiceLineItemId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.invoiceLineItemService.GetInvoiceLineItem(invoiceLineItemId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (invoiceLineItemData) => {

        //
        // Success path — invoiceLineItemData can legitimately be null if 404'd but request succeeded
        //
        if (!invoiceLineItemData) {

          this.handleInvoiceLineItemNotFound(invoiceLineItemId);

        } else {

          this.invoiceLineItemData = invoiceLineItemData;
          this.buildFormValues(this.invoiceLineItemData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'InvoiceLineItem loaded successfully',
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
        this.handleInvoiceLineItemLoadError(error, invoiceLineItemId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleInvoiceLineItemNotFound(invoiceLineItemId: number): void {

    this.invoiceLineItemData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `InvoiceLineItem #${invoiceLineItemId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleInvoiceLineItemLoadError(error: any, invoiceLineItemId: number): void {

    let message = 'Failed to load Invoice Line Item.';
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
          message = 'You do not have permission to view this Invoice Line Item.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Invoice Line Item #${invoiceLineItemId} was not found.`;
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

    console.error(`Invoice Line Item load failed (ID: ${invoiceLineItemId})`, error);

    //
    // Reset UI to safe state
    //
    this.invoiceLineItemData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(invoiceLineItemData: InvoiceLineItemData | null) {

    if (invoiceLineItemData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.invoiceLineItemForm.reset({
        invoiceId: null,
        eventChargeId: null,
        financialCategoryId: null,
        description: '',
        quantity: '',
        unitPrice: '',
        amount: '',
        taxAmount: '',
        totalAmount: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.invoiceLineItemForm.reset({
        invoiceId: invoiceLineItemData.invoiceId,
        eventChargeId: invoiceLineItemData.eventChargeId,
        financialCategoryId: invoiceLineItemData.financialCategoryId,
        description: invoiceLineItemData.description ?? '',
        quantity: invoiceLineItemData.quantity?.toString() ?? '',
        unitPrice: invoiceLineItemData.unitPrice?.toString() ?? '',
        amount: invoiceLineItemData.amount?.toString() ?? '',
        taxAmount: invoiceLineItemData.taxAmount?.toString() ?? '',
        totalAmount: invoiceLineItemData.totalAmount?.toString() ?? '',
        sequence: invoiceLineItemData.sequence?.toString() ?? '',
        active: invoiceLineItemData.active ?? true,
        deleted: invoiceLineItemData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.invoiceLineItemForm.markAsPristine();
    this.invoiceLineItemForm.markAsUntouched();
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

    if (this.invoiceLineItemService.userIsSchedulerInvoiceLineItemWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Invoice Line Items", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.invoiceLineItemForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.invoiceLineItemForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.invoiceLineItemForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const invoiceLineItemSubmitData: InvoiceLineItemSubmitData = {
        id: this.invoiceLineItemData?.id || 0,
        invoiceId: Number(formValue.invoiceId),
        eventChargeId: formValue.eventChargeId ? Number(formValue.eventChargeId) : null,
        financialCategoryId: formValue.financialCategoryId ? Number(formValue.financialCategoryId) : null,
        description: formValue.description!.trim(),
        quantity: Number(formValue.quantity),
        unitPrice: Number(formValue.unitPrice),
        amount: Number(formValue.amount),
        taxAmount: Number(formValue.taxAmount),
        totalAmount: Number(formValue.totalAmount),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.invoiceLineItemService.PutInvoiceLineItem(invoiceLineItemSubmitData.id, invoiceLineItemSubmitData)
      : this.invoiceLineItemService.PostInvoiceLineItem(invoiceLineItemSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedInvoiceLineItemData) => {

        this.invoiceLineItemService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Invoice Line Item's detail page
          //
          this.invoiceLineItemForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.invoiceLineItemForm.markAsUntouched();

          this.router.navigate(['/invoicelineitems', savedInvoiceLineItemData.id]);
          this.alertService.showMessage('Invoice Line Item added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.invoiceLineItemData = savedInvoiceLineItemData;
          this.buildFormValues(this.invoiceLineItemData);

          this.alertService.showMessage("Invoice Line Item saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Invoice Line Item.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Invoice Line Item.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Invoice Line Item could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerInvoiceLineItemReader(): boolean {
    return this.invoiceLineItemService.userIsSchedulerInvoiceLineItemReader();
  }

  public userIsSchedulerInvoiceLineItemWriter(): boolean {
    return this.invoiceLineItemService.userIsSchedulerInvoiceLineItemWriter();
  }
}
