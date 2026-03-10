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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { InvoiceService, InvoiceData, InvoiceSubmitData } from '../../../scheduler-data-services/invoice.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { FinancialOfficeService } from '../../../scheduler-data-services/financial-office.service';
import { InvoiceStatusService } from '../../../scheduler-data-services/invoice-status.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { TaxCodeService } from '../../../scheduler-data-services/tax-code.service';
import { AuthService } from '../../../services/auth.service';

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
  selector: 'app-invoice-add-edit',
  templateUrl: './invoice-add-edit.component.html',
  styleUrls: ['./invoice-add-edit.component.scss']
})
export class InvoiceAddEditComponent {
  @ViewChild('invoiceModal') invoiceModal!: TemplateRef<any>;
  @Output() invoiceChanged = new Subject<InvoiceData[]>();
  @Input() invoiceSubmitData: InvoiceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  invoices$ = this.invoiceService.GetInvoiceList();
  clients$ = this.clientService.GetClientList();
  contacts$ = this.contactService.GetContactList();
  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  financialOffices$ = this.financialOfficeService.GetFinancialOfficeList();
  invoiceStatuses$ = this.invoiceStatusService.GetInvoiceStatusList();
  currencies$ = this.currencyService.GetCurrencyList();
  taxCodes$ = this.taxCodeService.GetTaxCodeList();

  constructor(
    private modalService: NgbModal,
    private invoiceService: InvoiceService,
    private clientService: ClientService,
    private contactService: ContactService,
    private scheduledEventService: ScheduledEventService,
    private financialOfficeService: FinancialOfficeService,
    private invoiceStatusService: InvoiceStatusService,
    private currencyService: CurrencyService,
    private taxCodeService: TaxCodeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(invoiceData?: InvoiceData) {

    if (invoiceData != null) {

      if (!this.invoiceService.userIsSchedulerInvoiceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Invoices`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.invoiceSubmitData = this.invoiceService.ConvertToInvoiceSubmitData(invoiceData);
      this.isEditMode = true;
      this.objectGuid = invoiceData.objectGuid;

      this.buildFormValues(invoiceData);

    } else {

      if (!this.invoiceService.userIsSchedulerInvoiceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Invoices`,
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
        this.invoiceForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.invoiceModal, {
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

    if (this.invoiceService.userIsSchedulerInvoiceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Invoices`,
        '',
        MessageSeverity.info
      );
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
        id: this.invoiceSubmitData?.id || 0,
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
        versionNumber: this.invoiceSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateInvoice(invoiceSubmitData);
      } else {
        this.addInvoice(invoiceSubmitData);
      }
  }

  private addInvoice(invoiceData: InvoiceSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    invoiceData.versionNumber = 0;
    invoiceData.active = true;
    invoiceData.deleted = false;
    this.invoiceService.PostInvoice(invoiceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newInvoice) => {

        this.invoiceService.ClearAllCaches();

        this.invoiceChanged.next([newInvoice]);

        this.alertService.showMessage("Invoice added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/invoice', newInvoice.id]);
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


  private updateInvoice(invoiceData: InvoiceSubmitData) {
    this.invoiceService.PutInvoice(invoiceData.id, invoiceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedInvoice) => {

        this.invoiceService.ClearAllCaches();

        this.invoiceChanged.next([updatedInvoice]);

        this.alertService.showMessage("Invoice updated successfully", '', MessageSeverity.success);

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


  public userIsSchedulerInvoiceReader(): boolean {
    return this.invoiceService.userIsSchedulerInvoiceReader();
  }

  public userIsSchedulerInvoiceWriter(): boolean {
    return this.invoiceService.userIsSchedulerInvoiceWriter();
  }
}
