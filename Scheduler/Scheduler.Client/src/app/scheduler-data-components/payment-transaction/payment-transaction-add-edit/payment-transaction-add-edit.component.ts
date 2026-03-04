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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PaymentTransactionService, PaymentTransactionData, PaymentTransactionSubmitData } from '../../../scheduler-data-services/payment-transaction.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PaymentMethodService } from '../../../scheduler-data-services/payment-method.service';
import { PaymentProviderService } from '../../../scheduler-data-services/payment-provider.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { AuthService } from '../../../services/auth.service';

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
  selector: 'app-payment-transaction-add-edit',
  templateUrl: './payment-transaction-add-edit.component.html',
  styleUrls: ['./payment-transaction-add-edit.component.scss']
})
export class PaymentTransactionAddEditComponent {
  @ViewChild('paymentTransactionModal') paymentTransactionModal!: TemplateRef<any>;
  @Output() paymentTransactionChanged = new Subject<PaymentTransactionData[]>();
  @Input() paymentTransactionSubmitData: PaymentTransactionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();
  paymentMethods$ = this.paymentMethodService.GetPaymentMethodList();
  paymentProviders$ = this.paymentProviderService.GetPaymentProviderList();
  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  eventCharges$ = this.eventChargeService.GetEventChargeList();
  currencies$ = this.currencyService.GetCurrencyList();

  constructor(
    private modalService: NgbModal,
    private paymentTransactionService: PaymentTransactionService,
    private paymentMethodService: PaymentMethodService,
    private paymentProviderService: PaymentProviderService,
    private scheduledEventService: ScheduledEventService,
    private financialTransactionService: FinancialTransactionService,
    private eventChargeService: EventChargeService,
    private currencyService: CurrencyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(paymentTransactionData?: PaymentTransactionData) {

    if (paymentTransactionData != null) {

      if (!this.paymentTransactionService.userIsSchedulerPaymentTransactionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Payment Transactions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.paymentTransactionSubmitData = this.paymentTransactionService.ConvertToPaymentTransactionSubmitData(paymentTransactionData);
      this.isEditMode = true;
      this.objectGuid = paymentTransactionData.objectGuid;

      this.buildFormValues(paymentTransactionData);

    } else {

      if (!this.paymentTransactionService.userIsSchedulerPaymentTransactionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Payment Transactions`,
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
        this.paymentTransactionForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.paymentTransactionModal, {
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

    if (this.paymentTransactionService.userIsSchedulerPaymentTransactionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Payment Transactions`,
        '',
        MessageSeverity.info
      );
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
        id: this.paymentTransactionSubmitData?.id || 0,
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
        versionNumber: this.paymentTransactionSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePaymentTransaction(paymentTransactionSubmitData);
      } else {
        this.addPaymentTransaction(paymentTransactionSubmitData);
      }
  }

  private addPaymentTransaction(paymentTransactionData: PaymentTransactionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    paymentTransactionData.versionNumber = 0;
    paymentTransactionData.active = true;
    paymentTransactionData.deleted = false;
    this.paymentTransactionService.PostPaymentTransaction(paymentTransactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPaymentTransaction) => {

        this.paymentTransactionService.ClearAllCaches();

        this.paymentTransactionChanged.next([newPaymentTransaction]);

        this.alertService.showMessage("Payment Transaction added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/paymenttransaction', newPaymentTransaction.id]);
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


  private updatePaymentTransaction(paymentTransactionData: PaymentTransactionSubmitData) {
    this.paymentTransactionService.PutPaymentTransaction(paymentTransactionData.id, paymentTransactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPaymentTransaction) => {

        this.paymentTransactionService.ClearAllCaches();

        this.paymentTransactionChanged.next([updatedPaymentTransaction]);

        this.alertService.showMessage("Payment Transaction updated successfully", '', MessageSeverity.success);

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


  public userIsSchedulerPaymentTransactionReader(): boolean {
    return this.paymentTransactionService.userIsSchedulerPaymentTransactionReader();
  }

  public userIsSchedulerPaymentTransactionWriter(): boolean {
    return this.paymentTransactionService.userIsSchedulerPaymentTransactionWriter();
  }
}
