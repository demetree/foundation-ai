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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FinancialTransactionService, FinancialTransactionData, FinancialTransactionSubmitData } from '../../../scheduler-data-services/financial-transaction.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
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
interface FinancialTransactionFormValues {
  financialCategoryId: number | bigint,       // For FK link number
  scheduledEventId: number | bigint | null,       // For FK link number
  contactId: number | bigint | null,       // For FK link number
  transactionDate: string,
  description: string,
  amount: string,     // Stored as string for form input, converted to number on submit.
  taxAmount: string,     // Stored as string for form input, converted to number on submit.
  totalAmount: string,     // Stored as string for form input, converted to number on submit.
  isRevenue: boolean,
  paymentMethod: string | null,
  referenceNumber: string | null,
  notes: string | null,
  currencyId: number | bigint,       // For FK link number
  exportedDate: string | null,
  externalId: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-financial-transaction-add-edit',
  templateUrl: './financial-transaction-add-edit.component.html',
  styleUrls: ['./financial-transaction-add-edit.component.scss']
})
export class FinancialTransactionAddEditComponent {
  @ViewChild('financialTransactionModal') financialTransactionModal!: TemplateRef<any>;
  @Output() financialTransactionChanged = new Subject<FinancialTransactionData[]>();
  @Input() financialTransactionSubmitData: FinancialTransactionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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
        transactionDate: ['', Validators.required],
        description: ['', Validators.required],
        amount: ['', Validators.required],
        taxAmount: ['', Validators.required],
        totalAmount: ['', Validators.required],
        isRevenue: [false],
        paymentMethod: [''],
        referenceNumber: [''],
        notes: [''],
        currencyId: [null, Validators.required],
        exportedDate: [''],
        externalId: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();
  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  contacts$ = this.contactService.GetContactList();
  currencies$ = this.currencyService.GetCurrencyList();

  constructor(
    private modalService: NgbModal,
    private financialTransactionService: FinancialTransactionService,
    private financialCategoryService: FinancialCategoryService,
    private scheduledEventService: ScheduledEventService,
    private contactService: ContactService,
    private currencyService: CurrencyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(financialTransactionData?: FinancialTransactionData) {

    if (financialTransactionData != null) {

      if (!this.financialTransactionService.userIsSchedulerFinancialTransactionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Financial Transactions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.financialTransactionSubmitData = this.financialTransactionService.ConvertToFinancialTransactionSubmitData(financialTransactionData);
      this.isEditMode = true;
      this.objectGuid = financialTransactionData.objectGuid;

      this.buildFormValues(financialTransactionData);

    } else {

      if (!this.financialTransactionService.userIsSchedulerFinancialTransactionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Financial Transactions`,
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
        this.financialTransactionForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.financialTransactionModal, {
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

    if (this.financialTransactionService.userIsSchedulerFinancialTransactionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Financial Transactions`,
        '',
        MessageSeverity.info
      );
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
        id: this.financialTransactionSubmitData?.id || 0,
        financialCategoryId: Number(formValue.financialCategoryId),
        scheduledEventId: formValue.scheduledEventId ? Number(formValue.scheduledEventId) : null,
        contactId: formValue.contactId ? Number(formValue.contactId) : null,
        transactionDate: dateTimeLocalToIsoUtc(formValue.transactionDate!.trim())!,
        description: formValue.description!.trim(),
        amount: Number(formValue.amount),
        taxAmount: Number(formValue.taxAmount),
        totalAmount: Number(formValue.totalAmount),
        isRevenue: !!formValue.isRevenue,
        paymentMethod: formValue.paymentMethod?.trim() || null,
        referenceNumber: formValue.referenceNumber?.trim() || null,
        notes: formValue.notes?.trim() || null,
        currencyId: Number(formValue.currencyId),
        exportedDate: formValue.exportedDate ? dateTimeLocalToIsoUtc(formValue.exportedDate.trim()) : null,
        externalId: formValue.externalId?.trim() || null,
        versionNumber: this.financialTransactionSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateFinancialTransaction(financialTransactionSubmitData);
      } else {
        this.addFinancialTransaction(financialTransactionSubmitData);
      }
  }

  private addFinancialTransaction(financialTransactionData: FinancialTransactionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    financialTransactionData.versionNumber = 0;
    financialTransactionData.active = true;
    financialTransactionData.deleted = false;
    this.financialTransactionService.PostFinancialTransaction(financialTransactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newFinancialTransaction) => {

        this.financialTransactionService.ClearAllCaches();

        this.financialTransactionChanged.next([newFinancialTransaction]);

        this.alertService.showMessage("Financial Transaction added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/financialtransaction', newFinancialTransaction.id]);
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


  private updateFinancialTransaction(financialTransactionData: FinancialTransactionSubmitData) {
    this.financialTransactionService.PutFinancialTransaction(financialTransactionData.id, financialTransactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedFinancialTransaction) => {

        this.financialTransactionService.ClearAllCaches();

        this.financialTransactionChanged.next([updatedFinancialTransaction]);

        this.alertService.showMessage("Financial Transaction updated successfully", '', MessageSeverity.success);

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



  private buildFormValues(financialTransactionData: FinancialTransactionData | null) {

    if (financialTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.financialTransactionForm.reset({
        financialCategoryId: null,
        scheduledEventId: null,
        contactId: null,
        transactionDate: '',
        description: '',
        amount: '',
        taxAmount: '',
        totalAmount: '',
        isRevenue: false,
        paymentMethod: '',
        referenceNumber: '',
        notes: '',
        currencyId: null,
        exportedDate: '',
        externalId: '',
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
        transactionDate: isoUtcStringToDateTimeLocal(financialTransactionData.transactionDate) ?? '',
        description: financialTransactionData.description ?? '',
        amount: financialTransactionData.amount?.toString() ?? '',
        taxAmount: financialTransactionData.taxAmount?.toString() ?? '',
        totalAmount: financialTransactionData.totalAmount?.toString() ?? '',
        isRevenue: financialTransactionData.isRevenue ?? false,
        paymentMethod: financialTransactionData.paymentMethod ?? '',
        referenceNumber: financialTransactionData.referenceNumber ?? '',
        notes: financialTransactionData.notes ?? '',
        currencyId: financialTransactionData.currencyId,
        exportedDate: isoUtcStringToDateTimeLocal(financialTransactionData.exportedDate) ?? '',
        externalId: financialTransactionData.externalId ?? '',
        versionNumber: financialTransactionData.versionNumber?.toString() ?? '',
        active: financialTransactionData.active ?? true,
        deleted: financialTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.financialTransactionForm.markAsPristine();
    this.financialTransactionForm.markAsUntouched();
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


  public userIsSchedulerFinancialTransactionReader(): boolean {
    return this.financialTransactionService.userIsSchedulerFinancialTransactionReader();
  }

  public userIsSchedulerFinancialTransactionWriter(): boolean {
    return this.financialTransactionService.userIsSchedulerFinancialTransactionWriter();
  }
}
