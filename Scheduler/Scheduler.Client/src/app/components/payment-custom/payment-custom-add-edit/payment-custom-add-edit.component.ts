// AI-Developed — This file was significantly developed with AI assistance.
import { Component, ViewChild, TemplateRef, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { PaymentTransactionService, PaymentTransactionData, PaymentTransactionSubmitData } from '../../../scheduler-data-services/payment-transaction.service';
import { PaymentMethodService } from '../../../scheduler-data-services/payment-method.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';


@Component({
    selector: 'app-payment-custom-add-edit',
    templateUrl: './payment-custom-add-edit.component.html',
    styleUrls: ['./payment-custom-add-edit.component.scss']
})
export class PaymentCustomAddEditComponent {

    @ViewChild('paymentModal') paymentModal!: TemplateRef<any>;
    @Output() paymentChanged = new EventEmitter<void>();

    public form!: FormGroup;
    private modalRef: NgbModalRef | undefined;
    public isEditMode = false;
    public modalIsDisplayed = false;
    public isSaving = false;

    //
    // Lookups
    //
    paymentMethods$ = this.paymentMethodService.GetPaymentMethodList();
    currencies$ = this.currencyService.GetCurrencyList();

    private editId: number | bigint = 0;
    private editVersionNumber: number | bigint = 0;

    constructor(
        private modalService: NgbModal,
        private paymentService: PaymentTransactionService,
        private paymentMethodService: PaymentMethodService,
        private currencyService: CurrencyService,
        private authService: AuthService,
        private alertService: AlertService,
        private fb: FormBuilder
    ) {
        this.initForm();
    }


    private initForm(): void {
        this.form = this.fb.group({
            paymentMethodId: [null, Validators.required],
            currencyId: [null, Validators.required],
            transactionDate: ['', Validators.required],
            amount: [0, Validators.required],
            processingFee: [0],
            status: ['Completed', Validators.required],
            payerName: [''],
            payerEmail: [''],
            payerPhone: [''],
            receiptNumber: [''],
            notes: [''],
        });

        // Auto-compute netAmount when amount or fee changes
        this.form.get('amount')?.valueChanges.subscribe(() => this.computeNet());
        this.form.get('processingFee')?.valueChanges.subscribe(() => this.computeNet());
    }


    private computeNet(): void {
        const amount = Number(this.form.get('amount')?.value) || 0;
        const fee = Number(this.form.get('processingFee')?.value) || 0;
        this.computedNetAmount = amount - fee;
    }

    public computedNetAmount = 0;


    public openModal(data?: PaymentTransactionData): void {
        if (data) {
            this.isEditMode = true;
            this.editId = data.id;
            this.editVersionNumber = data.versionNumber;
            this.populateForm(data);
        } else {
            this.isEditMode = false;
            this.editId = 0;
            this.editVersionNumber = 0;
            this.resetForm();
        }

        this.modalRef = this.modalService.open(this.paymentModal, {
            size: 'lg', scrollable: true, backdrop: 'static', keyboard: true, windowClass: 'custom-modal'
        });
        this.modalIsDisplayed = true;
    }


    public closeModal(): void {
        if (this.modalRef) this.modalRef.dismiss('cancel');
        this.modalIsDisplayed = false;
    }


    private resetForm(): void {
        const now = new Date().toISOString().slice(0, 16);
        this.form.reset({
            paymentMethodId: null,
            currencyId: null,
            transactionDate: now,
            amount: 0,
            processingFee: 0,
            status: 'Completed',
            payerName: '',
            payerEmail: '',
            payerPhone: '',
            receiptNumber: '',
            notes: '',
        });
        this.computedNetAmount = 0;
    }


    private populateForm(p: PaymentTransactionData): void {
        this.form.patchValue({
            paymentMethodId: p.paymentMethodId ? Number(p.paymentMethodId) : null,
            currencyId: p.currencyId ? Number(p.currencyId) : null,
            transactionDate: p.transactionDate ? isoUtcStringToDateTimeLocal(p.transactionDate) : '',
            amount: p.amount ?? 0,
            processingFee: p.processingFee ?? 0,
            status: p.status ?? 'Completed',
            payerName: p.payerName ?? '',
            payerEmail: p.payerEmail ?? '',
            payerPhone: p.payerPhone ?? '',
            receiptNumber: p.receiptNumber ?? '',
            notes: p.notes ?? '',
        });
        this.computeNet();
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
        const amount = Number(fv.amount);
        const fee = Number(fv.processingFee) || 0;

        const submitData: PaymentTransactionSubmitData = {
            id: this.isEditMode ? this.editId : 0,
            paymentMethodId: Number(fv.paymentMethodId),
            paymentProviderId: null,
            scheduledEventId: null,
            financialTransactionId: null,
            eventChargeId: null,
            transactionDate: dateTimeLocalToIsoUtc(fv.transactionDate!.trim())!,
            amount: amount,
            processingFee: fee,
            netAmount: amount - fee,
            currencyId: Number(fv.currencyId),
            status: fv.status!.trim(),
            providerTransactionId: null,
            providerResponse: null,
            payerName: fv.payerName?.trim() || null,
            payerEmail: fv.payerEmail?.trim() || null,
            payerPhone: fv.payerPhone?.trim() || null,
            receiptNumber: fv.receiptNumber?.trim() || null,
            notes: fv.notes?.trim() || null,
            versionNumber: this.editVersionNumber,
            active: true,
            deleted: false,
        };

        const op$ = this.isEditMode
            ? this.paymentService.PutPaymentTransaction(this.editId, submitData)
            : this.paymentService.PostPaymentTransaction(submitData);

        op$.subscribe({
            next: () => {
                this.alertService.showMessage(
                    this.isEditMode ? 'Payment updated' : 'Payment created',
                    '', MessageSeverity.success
                );
                this.isSaving = false;
                this.closeModal();
                this.paymentService.ClearAllCaches();
                this.paymentChanged.emit();
            },
            error: (err) => {
                this.alertService.showMessage('Failed to save payment', JSON.stringify(err), MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }
}
