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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { InvoiceLineItemService, InvoiceLineItemData, InvoiceLineItemSubmitData } from '../../../scheduler-data-services/invoice-line-item.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { InvoiceService } from '../../../scheduler-data-services/invoice.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { AuthService } from '../../../services/auth.service';

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
  selector: 'app-invoice-line-item-add-edit',
  templateUrl: './invoice-line-item-add-edit.component.html',
  styleUrls: ['./invoice-line-item-add-edit.component.scss']
})
export class InvoiceLineItemAddEditComponent {
  @ViewChild('invoiceLineItemModal') invoiceLineItemModal!: TemplateRef<any>;
  @Output() invoiceLineItemChanged = new Subject<InvoiceLineItemData[]>();
  @Input() invoiceLineItemSubmitData: InvoiceLineItemSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  invoiceLineItems$ = this.invoiceLineItemService.GetInvoiceLineItemList();
  invoices$ = this.invoiceService.GetInvoiceList();
  eventCharges$ = this.eventChargeService.GetEventChargeList();
  financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();

  constructor(
    private modalService: NgbModal,
    private invoiceLineItemService: InvoiceLineItemService,
    private invoiceService: InvoiceService,
    private eventChargeService: EventChargeService,
    private financialCategoryService: FinancialCategoryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(invoiceLineItemData?: InvoiceLineItemData) {

    if (invoiceLineItemData != null) {

      if (!this.invoiceLineItemService.userIsSchedulerInvoiceLineItemReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Invoice Line Items`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.invoiceLineItemSubmitData = this.invoiceLineItemService.ConvertToInvoiceLineItemSubmitData(invoiceLineItemData);
      this.isEditMode = true;
      this.objectGuid = invoiceLineItemData.objectGuid;

      this.buildFormValues(invoiceLineItemData);

    } else {

      if (!this.invoiceLineItemService.userIsSchedulerInvoiceLineItemWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Invoice Line Items`,
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
        this.invoiceLineItemForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.invoiceLineItemModal, {
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

    if (this.invoiceLineItemService.userIsSchedulerInvoiceLineItemWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Invoice Line Items`,
        '',
        MessageSeverity.info
      );
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
        id: this.invoiceLineItemSubmitData?.id || 0,
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

      if (this.isEditMode) {
        this.updateInvoiceLineItem(invoiceLineItemSubmitData);
      } else {
        this.addInvoiceLineItem(invoiceLineItemSubmitData);
      }
  }

  private addInvoiceLineItem(invoiceLineItemData: InvoiceLineItemSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    invoiceLineItemData.active = true;
    invoiceLineItemData.deleted = false;
    this.invoiceLineItemService.PostInvoiceLineItem(invoiceLineItemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newInvoiceLineItem) => {

        this.invoiceLineItemService.ClearAllCaches();

        this.invoiceLineItemChanged.next([newInvoiceLineItem]);

        this.alertService.showMessage("Invoice Line Item added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/invoicelineitem', newInvoiceLineItem.id]);
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


  private updateInvoiceLineItem(invoiceLineItemData: InvoiceLineItemSubmitData) {
    this.invoiceLineItemService.PutInvoiceLineItem(invoiceLineItemData.id, invoiceLineItemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedInvoiceLineItem) => {

        this.invoiceLineItemService.ClearAllCaches();

        this.invoiceLineItemChanged.next([updatedInvoiceLineItem]);

        this.alertService.showMessage("Invoice Line Item updated successfully", '', MessageSeverity.success);

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


  public userIsSchedulerInvoiceLineItemReader(): boolean {
    return this.invoiceLineItemService.userIsSchedulerInvoiceLineItemReader();
  }

  public userIsSchedulerInvoiceLineItemWriter(): boolean {
    return this.invoiceLineItemService.userIsSchedulerInvoiceLineItemWriter();
  }
}
