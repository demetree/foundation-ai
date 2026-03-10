// AI-Developed — This file was significantly developed with AI assistance.
import { Component, ViewChild, TemplateRef, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { ReceiptService, ReceiptData, ReceiptSubmitData } from '../../../scheduler-data-services/receipt.service';
import { ReceiptTypeService } from '../../../scheduler-data-services/receipt-type.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';


@Component({
    selector: 'app-receipt-custom-add-edit',
    templateUrl: './receipt-custom-add-edit.component.html',
    styleUrls: ['./receipt-custom-add-edit.component.scss']
})
export class ReceiptCustomAddEditComponent {

    @ViewChild('receiptModal') receiptModal!: TemplateRef<any>;
    @Output() receiptChanged = new EventEmitter<void>();

    public form!: FormGroup;
    private modalRef: NgbModalRef | undefined;
    public isEditMode = false;
    public modalIsDisplayed = false;
    public isSaving = false;

    //
    // Lookup data
    //
    receiptTypes$ = this.receiptTypeService.GetReceiptTypeList();
    clients$ = this.clientService.GetClientList({ active: true, deleted: false });
    currencies$ = this.currencyService.GetCurrencyList();

    //
    // Edit state
    //
    private editId: number | bigint = 0;
    private editVersionNumber: number | bigint = 0;

    constructor(
        private modalService: NgbModal,
        private receiptService: ReceiptService,
        private receiptTypeService: ReceiptTypeService,
        private clientService: ClientService,
        private currencyService: CurrencyService,
        private authService: AuthService,
        private alertService: AlertService,
        private fb: FormBuilder
    ) {
        this.initForm();
    }


    private initForm(): void {
        this.form = this.fb.group({
            receiptNumber: ['', Validators.required],
            receiptTypeId: [null, Validators.required],
            currencyId: [null, Validators.required],
            receiptDate: ['', Validators.required],
            amount: [0, Validators.required],
            clientId: [null],
            contactId: [null],
            invoiceId: [null],
            paymentTransactionId: [null],
            financialTransactionId: [null],
            paymentMethod: [''],
            description: [''],
            notes: [''],
        });
    }


    /**
     * Public API — called by parent via @ViewChild
     */
    public openModal(receiptData?: ReceiptData): void {
        if (receiptData) {
            this.isEditMode = true;
            this.editId = receiptData.id;
            this.editVersionNumber = receiptData.versionNumber;
            this.populateForm(receiptData);
        } else {
            this.isEditMode = false;
            this.editId = 0;
            this.editVersionNumber = 0;
            this.resetForm();
        }

        this.modalRef = this.modalService.open(this.receiptModal, {
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

        this.form.reset({
            receiptNumber: '',
            receiptTypeId: null,
            currencyId: null,
            receiptDate: localIso,
            amount: 0,
            clientId: null,
            contactId: null,
            invoiceId: null,
            paymentTransactionId: null,
            financialTransactionId: null,
            paymentMethod: '',
            description: '',
            notes: '',
        });
    }


    private populateForm(r: ReceiptData): void {
        this.form.patchValue({
            receiptNumber: r.receiptNumber ?? '',
            receiptTypeId: r.receiptTypeId ? Number(r.receiptTypeId) : null,
            currencyId: r.currencyId ? Number(r.currencyId) : null,
            receiptDate: r.receiptDate ? isoUtcStringToDateTimeLocal(r.receiptDate) : '',
            amount: r.amount ?? 0,
            clientId: r.clientId ? Number(r.clientId) : null,
            contactId: r.contactId ? Number(r.contactId) : null,
            invoiceId: r.invoiceId ? Number(r.invoiceId) : null,
            paymentTransactionId: r.paymentTransactionId ? Number(r.paymentTransactionId) : null,
            financialTransactionId: r.financialTransactionId ? Number(r.financialTransactionId) : null,
            paymentMethod: r.paymentMethod ?? '',
            description: r.description ?? '',
            notes: r.notes ?? '',
        });
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

        const submitData: ReceiptSubmitData = {
            id: this.isEditMode ? this.editId : 0,
            receiptNumber: fv.receiptNumber!.trim(),
            receiptTypeId: Number(fv.receiptTypeId),
            invoiceId: fv.invoiceId ? Number(fv.invoiceId) : null,
            paymentTransactionId: fv.paymentTransactionId ? Number(fv.paymentTransactionId) : null,
            financialTransactionId: fv.financialTransactionId ? Number(fv.financialTransactionId) : null,
            clientId: fv.clientId ? Number(fv.clientId) : null,
            contactId: fv.contactId ? Number(fv.contactId) : null,
            currencyId: Number(fv.currencyId),
            receiptDate: dateTimeLocalToIsoUtc(fv.receiptDate!.trim())!,
            amount: Number(fv.amount),
            paymentMethod: fv.paymentMethod?.trim() || null,
            description: fv.description?.trim() || null,
            notes: fv.notes?.trim() || null,
            versionNumber: this.editVersionNumber,
            active: true,
            deleted: false,
        };

        if (this.isEditMode) {
            this.receiptService.PutReceipt(this.editId, submitData).subscribe({
                next: () => {
                    this.alertService.showMessage('Receipt updated successfully', '', MessageSeverity.success);
                    this.isSaving = false;
                    this.closeModal();
                    this.receiptService.ClearAllCaches();
                    this.receiptChanged.emit();
                },
                error: (err) => {
                    this.alertService.showMessage('Failed to update receipt', JSON.stringify(err), MessageSeverity.error);
                    this.isSaving = false;
                }
            });
        } else {
            this.receiptService.PostReceipt(submitData).subscribe({
                next: () => {
                    this.alertService.showMessage('Receipt created successfully', '', MessageSeverity.success);
                    this.isSaving = false;
                    this.closeModal();
                    this.receiptService.ClearAllCaches();
                    this.receiptChanged.emit();
                },
                error: (err) => {
                    this.alertService.showMessage('Failed to create receipt', JSON.stringify(err), MessageSeverity.error);
                    this.isSaving = false;
                }
            });
        }
    }
}
