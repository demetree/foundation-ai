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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ReceiptService, ReceiptData, ReceiptSubmitData } from '../../../scheduler-data-services/receipt.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ReceiptTypeService } from '../../../scheduler-data-services/receipt-type.service';
import { InvoiceService } from '../../../scheduler-data-services/invoice.service';
import { PaymentTransactionService } from '../../../scheduler-data-services/payment-transaction.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { AuthService } from '../../../services/auth.service';

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
  selector: 'app-receipt-add-edit',
  templateUrl: './receipt-add-edit.component.html',
  styleUrls: ['./receipt-add-edit.component.scss']
})
export class ReceiptAddEditComponent {
  @ViewChild('receiptModal') receiptModal!: TemplateRef<any>;
  @Output() receiptChanged = new Subject<ReceiptData[]>();
  @Input() receiptSubmitData: ReceiptSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  receipts$ = this.receiptService.GetReceiptList();
  receiptTypes$ = this.receiptTypeService.GetReceiptTypeList();
  invoices$ = this.invoiceService.GetInvoiceList();
  paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();
  financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  clients$ = this.clientService.GetClientList();
  contacts$ = this.contactService.GetContactList();
  currencies$ = this.currencyService.GetCurrencyList();

  constructor(
    private modalService: NgbModal,
    private receiptService: ReceiptService,
    private receiptTypeService: ReceiptTypeService,
    private invoiceService: InvoiceService,
    private paymentTransactionService: PaymentTransactionService,
    private financialTransactionService: FinancialTransactionService,
    private clientService: ClientService,
    private contactService: ContactService,
    private currencyService: CurrencyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(receiptData?: ReceiptData) {

    if (receiptData != null) {

      if (!this.receiptService.userIsSchedulerReceiptReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Receipts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.receiptSubmitData = this.receiptService.ConvertToReceiptSubmitData(receiptData);
      this.isEditMode = true;
      this.objectGuid = receiptData.objectGuid;

      this.buildFormValues(receiptData);

    } else {

      if (!this.receiptService.userIsSchedulerReceiptWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Receipts`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.receiptForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.receiptModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.receiptService.userIsSchedulerReceiptWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Receipts`,
        '',
        MessageSeverity.info
      );
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
        id: this.receiptSubmitData?.id || 0,
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
        versionNumber: this.receiptSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateReceipt(receiptSubmitData);
      } else {
        this.addReceipt(receiptSubmitData);
      }
  }

  private addReceipt(receiptData: ReceiptSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    receiptData.versionNumber = 0;
    receiptData.active = true;
    receiptData.deleted = false;
    this.receiptService.PostReceipt(receiptData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newReceipt) => {

        this.receiptService.ClearAllCaches();

        this.receiptChanged.next([newReceipt]);

        this.alertService.showMessage("Receipt added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/receipt', newReceipt.id]);
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


  private updateReceipt(receiptData: ReceiptSubmitData) {
    this.receiptService.PutReceipt(receiptData.id, receiptData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedReceipt) => {

        this.receiptService.ClearAllCaches();

        this.receiptChanged.next([updatedReceipt]);

        this.alertService.showMessage("Receipt updated successfully", '', MessageSeverity.success);

        this.closeModal();
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


  public userIsSchedulerReceiptReader(): boolean {
    return this.receiptService.userIsSchedulerReceiptReader();
  }

  public userIsSchedulerReceiptWriter(): boolean {
    return this.receiptService.userIsSchedulerReceiptWriter();
  }
}
