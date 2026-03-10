// AI-Developed — This file was significantly developed with AI assistance.
import { Component, ViewChild, TemplateRef, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { InvoiceService, InvoiceData, InvoiceSubmitData } from '../../../scheduler-data-services/invoice.service';
import { InvoiceStatusService, InvoiceStatusData } from '../../../scheduler-data-services/invoice-status.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { TaxCodeService } from '../../../scheduler-data-services/tax-code.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';


@Component({
    selector: 'app-invoice-custom-add-edit',
    templateUrl: './invoice-custom-add-edit.component.html',
    styleUrls: ['./invoice-custom-add-edit.component.scss']
})
export class InvoiceCustomAddEditComponent {

    @ViewChild('invoiceModal') invoiceModal!: TemplateRef<any>;
    @Output() invoiceChanged = new EventEmitter<void>();

    public form!: FormGroup;
    private modalRef: NgbModalRef | undefined;
    public isEditMode = false;
    public modalIsDisplayed = false;
    public isSaving = false;

    //
    // Lookup data
    //
    statuses$ = this.invoiceStatusService.GetInvoiceStatusList();
    clients$ = this.clientService.GetClientList({ active: true, deleted: false });
    currencies$ = this.currencyService.GetCurrencyList();
    taxCodes$ = this.taxCodeService.GetTaxCodeList();
    public offices: FinancialOfficeData[] = [];

    //
    // Edit state
    //
    private editId: number | bigint = 0;
    private editVersionNumber: number | bigint = 0;

    constructor(
        private modalService: NgbModal,
        private invoiceService: InvoiceService,
        private invoiceStatusService: InvoiceStatusService,
        private clientService: ClientService,
        private currencyService: CurrencyService,
        private taxCodeService: TaxCodeService,
        private officeService: FinancialOfficeService,
        private authService: AuthService,
        private alertService: AlertService,
        private router: Router,
        private fb: FormBuilder
    ) {
        this.initForm();
    }


    private initForm(): void {
        this.form = this.fb.group({
            invoiceNumber: ['', Validators.required],
            clientId: [null, Validators.required],
            invoiceStatusId: [null, Validators.required],
            currencyId: [null, Validators.required],
            invoiceDate: ['', Validators.required],
            dueDate: ['', Validators.required],
            subtotal: [0],
            taxAmount: [0],
            totalAmount: [{ value: 0, disabled: true }],
            amountPaid: [0],
            contactId: [null],
            scheduledEventId: [null],
            financialOfficeId: [null],
            taxCodeId: [null],
            notes: [''],
        });

        this.form.get('subtotal')?.valueChanges.subscribe(() => this.computeTotal());
        this.form.get('taxAmount')?.valueChanges.subscribe(() => this.computeTotal());
    }


    private computeTotal(): void {
        const sub = Number(this.form.get('subtotal')?.value) || 0;
        const tax = Number(this.form.get('taxAmount')?.value) || 0;
        this.form.get('totalAmount')?.setValue(sub + tax, { emitEvent: false });
    }


    private loadLookups(): void {
        this.officeService.GetFinancialOfficeList({ active: true, deleted: false }).subscribe({
            next: (data) => { this.offices = data ?? []; }
        });
    }


    /**
     * Public API — called by parent via @ViewChild
     */
    public openModal(invoiceData?: InvoiceData): void {
        if (invoiceData) {
            this.isEditMode = true;
            this.editId = invoiceData.id;
            this.editVersionNumber = invoiceData.versionNumber;
            this.populateForm(invoiceData);
        } else {
            this.isEditMode = false;
            this.editId = 0;
            this.editVersionNumber = 0;
            this.resetForm();
        }

        this.loadLookups();

        this.modalRef = this.modalService.open(this.invoiceModal, {
            size: 'lg',
            scrollable: true,
            backdrop: 'static',
            keyboard: true,
            windowClass: 'custom-modal'
        });
        this.modalIsDisplayed = true;
    }


    public closeModal(): void {
        if (this.modalRef) {
            this.modalRef.dismiss('cancel');
        }
        this.modalIsDisplayed = false;
    }


    private resetForm(): void {
        const now = new Date();
        const localIso = now.toISOString().slice(0, 16);
        const dueDate = new Date(now.getTime() + 30 * 24 * 60 * 60 * 1000).toISOString().slice(0, 16);

        this.form.reset({
            invoiceNumber: '',
            clientId: null,
            invoiceStatusId: null,
            currencyId: null,
            invoiceDate: localIso,
            dueDate: dueDate,
            subtotal: 0,
            taxAmount: 0,
            totalAmount: 0,
            amountPaid: 0,
            contactId: null,
            scheduledEventId: null,
            financialOfficeId: null,
            taxCodeId: null,
            notes: '',
        });
    }


    private populateForm(inv: InvoiceData): void {
        this.form.patchValue({
            invoiceNumber: inv.invoiceNumber ?? '',
            clientId: inv.clientId ? Number(inv.clientId) : null,
            invoiceStatusId: inv.invoiceStatusId ? Number(inv.invoiceStatusId) : null,
            currencyId: inv.currencyId ? Number(inv.currencyId) : null,
            invoiceDate: inv.invoiceDate ? isoUtcStringToDateTimeLocal(inv.invoiceDate) : '',
            dueDate: inv.dueDate ? isoUtcStringToDateTimeLocal(inv.dueDate) : '',
            subtotal: inv.subtotal ?? 0,
            taxAmount: inv.taxAmount ?? 0,
            totalAmount: inv.totalAmount ?? 0,
            amountPaid: inv.amountPaid ?? 0,
            contactId: inv.contactId ? Number(inv.contactId) : null,
            scheduledEventId: inv.scheduledEventId ? Number(inv.scheduledEventId) : null,
            financialOfficeId: inv.financialOfficeId ? Number(inv.financialOfficeId) : null,
            taxCodeId: inv.taxCodeId ? Number(inv.taxCodeId) : null,
            notes: inv.notes ?? '',
        });
        this.computeTotal();
    }


    public submitForm(): void {
        if (this.isSaving) return;

        if (!this.form.valid) {
            this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
            this.form.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        const fv = this.form.getRawValue();

        const submitData: InvoiceSubmitData = {
            id: this.isEditMode ? this.editId : 0,
            invoiceNumber: fv.invoiceNumber!.trim(),
            clientId: Number(fv.clientId),
            contactId: fv.contactId ? Number(fv.contactId) : null,
            scheduledEventId: fv.scheduledEventId ? Number(fv.scheduledEventId) : null,
            financialOfficeId: fv.financialOfficeId ? Number(fv.financialOfficeId) : null,
            invoiceStatusId: Number(fv.invoiceStatusId),
            currencyId: Number(fv.currencyId),
            taxCodeId: fv.taxCodeId ? Number(fv.taxCodeId) : null,
            invoiceDate: dateTimeLocalToIsoUtc(fv.invoiceDate!.trim())!,
            dueDate: dateTimeLocalToIsoUtc(fv.dueDate!.trim())!,
            subtotal: Number(fv.subtotal),
            taxAmount: Number(fv.taxAmount),
            totalAmount: Number(fv.totalAmount),
            amountPaid: Number(fv.amountPaid),
            sentDate: null,
            paidDate: null,
            notes: fv.notes?.trim() || null,
            versionNumber: this.editVersionNumber,
            active: true,
            deleted: false,
        };

        if (this.isEditMode) {
            this.invoiceService.PutInvoice(this.editId, submitData).subscribe({
                next: () => {
                    this.alertService.showMessage('Invoice updated successfully', '', MessageSeverity.success);
                    this.isSaving = false;
                    this.closeModal();
                    this.invoiceService.ClearAllCaches();
                    this.invoiceChanged.emit();
                },
                error: (err) => {
                    this.alertService.showMessage('Failed to update invoice', JSON.stringify(err), MessageSeverity.error);
                    this.isSaving = false;
                }
            });
        } else {
            this.invoiceService.PostInvoice(submitData).subscribe({
                next: () => {
                    this.alertService.showMessage('Invoice created successfully', '', MessageSeverity.success);
                    this.isSaving = false;
                    this.closeModal();
                    this.invoiceService.ClearAllCaches();
                    this.invoiceChanged.emit();
                },
                error: (err) => {
                    this.alertService.showMessage('Failed to create invoice', JSON.stringify(err), MessageSeverity.error);
                    this.isSaving = false;
                }
            });
        }
    }
}
