import { Component, Input, Output, EventEmitter, Inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { FinancialTransactionService, FinancialTransactionData, FinancialTransactionSubmitData } from '../../../scheduler-data-services/financial-transaction.service';
import { HttpClient } from '@angular/common/http';
import { FinancialCategoryService, FinancialCategoryData } from '../../../scheduler-data-services/financial-category.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { PaymentTypeService } from '../../../scheduler-data-services/payment-type.service';
import { TaxCodeService } from '../../../scheduler-data-services/tax-code.service';
import { FiscalPeriodService } from '../../../scheduler-data-services/fiscal-period.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
    selector: 'app-financial-transaction-custom-add-edit',
    templateUrl: './financial-transaction-custom-add-edit.component.html',
    styleUrls: ['./financial-transaction-custom-add-edit.component.scss']
})
export class FinancialTransactionCustomAddEditComponent implements OnInit {

    @Output() financialTransactionChanged = new EventEmitter<void>();

    @Input() navigateToDetailsAfterAdd: boolean = false;
    @Input() showAddButton: boolean = false;
    @Input() hiddenFields: string[] = [];

    //
    // Pre-seeded data (partial) for add mode — allows parent to set defaults like financialOfficeId
    //
    @Input() preSeededData: Record<string, any> | null = null;

    public txForm!: FormGroup;
    public isEditMode = false;
    public isSaving = false;
    public isSavingAndAdding = false;
    public showMoreDetails = false;

    //
    // Lookup data
    //
    public categories: FinancialCategoryData[] = [];
    public revenueCategories: FinancialCategoryData[] = [];
    public expenseCategories: FinancialCategoryData[] = [];
    public offices: FinancialOfficeData[] = [];

    paymentTypes$ = this.paymentTypeService.GetPaymentTypeList();
    taxCodes$ = this.taxCodeService.GetTaxCodeList();
    fiscalPeriods$ = this.fiscalPeriodService.GetFiscalPeriodList();
    currencies$ = this.currencyService.GetCurrencyList();

    //
    // Private edit-mode state
    //
    private editId: number | bigint = 0;
    private editVersionNumber: number | bigint = 0;

    constructor(
        private route: ActivatedRoute,
        private location: Location,
        private transactionService: FinancialTransactionService,
        private categoryService: FinancialCategoryService,
        private officeService: FinancialOfficeService,
        private paymentTypeService: PaymentTypeService,
        private taxCodeService: TaxCodeService,
        private fiscalPeriodService: FiscalPeriodService,
        private currencyService: CurrencyService,
        private authService: AuthService,
        private alertService: AlertService,
        private router: Router,
        private fb: FormBuilder,
        private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string
    ) {
        this.initForm();
    }


    private initForm(): void {
        this.txForm = this.fb.group({
            transactionDate: ['', Validators.required],
            amount: [0, Validators.required],
            taxAmount: [0],
            totalAmount: [{ value: 0, disabled: true }],
            financialCategoryId: [null, Validators.required],
            financialOfficeId: [null],
            paymentTypeId: [null],
            description: ['', Validators.required],
            isRevenue: [false],
            currencyId: [null, Validators.required],
            // More details (collapsible)
            referenceNumber: [''],
            notes: [''],
            taxCodeId: [null],
            fiscalPeriodId: [null],
            journalEntryType: [''],
            contactRole: [''],
            scheduledEventId: [null],
            contactId: [null],
            clientId: [null],
        });

        //
        // Auto-compute totalAmount when amount or taxAmount changes
        //
        this.txForm.get('amount')?.valueChanges.subscribe(() => this.computeTotal());
        this.txForm.get('taxAmount')?.valueChanges.subscribe(() => this.computeTotal());
    }


    private computeTotal(): void {
        const amount = Number(this.txForm.get('amount')?.value) || 0;
        const tax = Number(this.txForm.get('taxAmount')?.value) || 0;
        this.txForm.get('totalAmount')?.setValue(amount + tax, { emitEvent: false });
    }


    //
    // Load lookup data
    //
    private loadLookups(): void {
        this.categoryService.GetFinancialCategoryList({
            active: true, deleted: false, includeRelations: true
        }).subscribe({
            next: (data) => {
                this.categories = data ?? [];
                this.revenueCategories = this.categories.filter(c => c.accountType?.isRevenue);
                this.expenseCategories = this.categories.filter(c => !c.accountType?.isRevenue);
            }
        });

        this.officeService.GetFinancialOfficeList({
            active: true, deleted: false
        }).subscribe({
            next: (data) => { this.offices = data ?? []; }
        });
    }


    //
    // Lifecycle / Route Init
    //
    public ngOnInit(): void {
        this.loadLookups();
        
        const idParam = this.route.snapshot.paramMap.get('id');
        
        if (idParam && idParam !== 'new' && idParam !== '0') {
            const id = Number(idParam);
            this.transactionService.GetFinancialTransaction(id, true).subscribe({
                next: (txData) => {
                    this.setupEditMode(txData);
                },
                error: (err) => {
                    this.alertService.showMessage('Error loading transaction', '', MessageSeverity.error);
                    this.goBack();
                }
            });
        } else {
            this.setupAddMode();
        }
    }

    private setupEditMode(txData: FinancialTransactionData): void {
        if (!this.transactionService.userIsSchedulerFinancialTransactionReader()) {
            this.alertService.showMessage(`${this.authService.currentUser?.userName} does not have permission`, '', MessageSeverity.info);
            this.goBack();
            return;
        }

        this.isEditMode = true;
        this.editId = txData.id;
        this.editVersionNumber = txData.versionNumber;
        this.populateForm(txData);

        this.showMoreDetails = !!(txData.referenceNumber || txData.notes || txData.taxCodeId
            || txData.fiscalPeriodId || txData.journalEntryType || txData.contactRole
            || txData.contactId || txData.clientId || txData.scheduledEventId);
            
        this.disableHiddenFieldValidators();
    }

    private setupAddMode(): void {
        if (!this.transactionService.userIsSchedulerFinancialTransactionWriter()) {
            this.alertService.showMessage(`${this.authService.currentUser?.userName} does not have permission`, '', MessageSeverity.info);
            this.goBack();
            return;
        }

        this.isEditMode = false;
        this.editId = 0;
        this.editVersionNumber = 0;
        this.showMoreDetails = false;
        this.resetForm();

        // Check query params for type=revenue or type=expense
        const queryParams = this.route.snapshot.queryParams;
        const seed: Record<string, any> = {};
        
        if (queryParams['type'] === 'revenue') {
            seed['isRevenue'] = true;
        } else if (queryParams['type'] === 'expense') {
            seed['isRevenue'] = false;
        }
        if (queryParams['financialOfficeId']) {
            seed['financialOfficeId'] = Number(queryParams['financialOfficeId']);
        }
        
        if (Object.keys(seed).length > 0) {
            this.txForm.patchValue(seed);
        } else if (this.preSeededData) {
            this.txForm.patchValue(this.preSeededData);
        }

        this.applyLastUsedValues();
        this.disableHiddenFieldValidators();
    }

    private disableHiddenFieldValidators(): void {
        for (const fieldName of this.hiddenFields) {
            const control = this.txForm.get(fieldName);
            if (control) {
                control.clearValidators();
                control.updateValueAndValidity();
            }
        }
    }

    public goBack(): void {
        this.location.back();
    }

    //
    // Form population
    //
    private resetForm(): void {
        const now = new Date();
        const localIso = now.toISOString().slice(0, 16); // yyyy-MM-ddTHH:mm

        this.txForm.reset({
            transactionDate: localIso,
            amount: 0,
            taxAmount: 0,
            totalAmount: 0,
            financialCategoryId: null,
            financialOfficeId: null,
            paymentTypeId: null,
            description: '',
            isRevenue: false,
            currencyId: null,
            referenceNumber: '',
            notes: '',
            taxCodeId: null,
            fiscalPeriodId: null,
            journalEntryType: '',
            contactRole: '',
            scheduledEventId: null,
            contactId: null,
            clientId: null,
        });
    }


    private populateForm(tx: FinancialTransactionData): void {
        this.txForm.patchValue({
            transactionDate: tx.transactionDate ? isoUtcStringToDateTimeLocal(tx.transactionDate) : '',
            amount: tx.amount ?? 0,
            taxAmount: tx.taxAmount ?? 0,
            totalAmount: tx.totalAmount ?? 0,
            financialCategoryId: tx.financialCategoryId ? Number(tx.financialCategoryId) : null,
            financialOfficeId: tx.financialOfficeId ? Number(tx.financialOfficeId) : null,
            paymentTypeId: tx.paymentTypeId ? Number(tx.paymentTypeId) : null,
            description: tx.description ?? '',
            isRevenue: tx.isRevenue ?? false,
            currencyId: tx.currencyId ? Number(tx.currencyId) : null,
            referenceNumber: tx.referenceNumber ?? '',
            notes: tx.notes ?? '',
            taxCodeId: tx.taxCodeId ? Number(tx.taxCodeId) : null,
            fiscalPeriodId: tx.fiscalPeriodId ? Number(tx.fiscalPeriodId) : null,
            journalEntryType: tx.journalEntryType ?? '',
            contactRole: tx.contactRole ?? '',
            scheduledEventId: tx.scheduledEventId ? Number(tx.scheduledEventId) : null,
            contactId: tx.contactId ? Number(tx.contactId) : null,
            clientId: tx.clientId ? Number(tx.clientId) : null,
        });

        this.computeTotal();
    }


    //
    // When category changes, auto-set isRevenue
    //
    public onCategoryChange(): void {
        const catId = this.txForm.get('financialCategoryId')?.value;
        if (catId) {
            const cat = this.categories.find(c => Number(c.id) === Number(catId));
            if (cat?.accountType) {
                this.txForm.get('isRevenue')?.setValue(cat.accountType.isRevenue);
            }
        }
    }


    //
    // Submit
    //
    public submitForm(): void {

        if (this.isSaving) return;

        if (!this.transactionService.userIsSchedulerFinancialTransactionWriter()) {
            this.alertService.showMessage(
                `${this.authService.currentUser?.userName} does not have permission to write Financial Transactions`, '', MessageSeverity.info
            );
            return;
        }

        if (!this.txForm.valid) {
            this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
            this.txForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        const fv = this.txForm.getRawValue();

        const submitData: FinancialTransactionSubmitData = {
            id: this.isEditMode ? this.editId : 0,
            financialCategoryId: Number(fv.financialCategoryId),
            financialOfficeId: fv.financialOfficeId ? Number(fv.financialOfficeId) : null,
            scheduledEventId: fv.scheduledEventId ? Number(fv.scheduledEventId) : null,
            contactId: fv.contactId ? Number(fv.contactId) : null,
            clientId: fv.clientId ? Number(fv.clientId) : null,
            contactRole: fv.contactRole?.trim() || null,
            taxCodeId: fv.taxCodeId ? Number(fv.taxCodeId) : null,
            fiscalPeriodId: fv.fiscalPeriodId ? Number(fv.fiscalPeriodId) : null,
            paymentTypeId: fv.paymentTypeId ? Number(fv.paymentTypeId) : null,
            transactionDate: dateTimeLocalToIsoUtc(fv.transactionDate!.trim())!,
            description: fv.description!.trim(),
            amount: Number(fv.amount),
            taxAmount: Number(fv.taxAmount),
            totalAmount: Number(fv.totalAmount),
            isRevenue: !!fv.isRevenue,
            journalEntryType: fv.journalEntryType?.trim() || null,
            referenceNumber: fv.referenceNumber?.trim() || null,
            notes: fv.notes?.trim() || null,
            currencyId: Number(fv.currencyId),
            exportedDate: null,
            externalId: null,
            externalSystemName: null,
            versionNumber: this.editVersionNumber,
            active: true,
            deleted: false,
        };

        if (this.isEditMode) {
            // Route edits through FinancialManagementService for fiscal period validation + audit trail
            this.putViaService(fv).subscribe({
                next: () => {
                    this.alertService.showMessage('Transaction updated successfully', '', MessageSeverity.success);
                    this.isSaving = false;
                    this.goBack();
                    this.transactionService.ClearAllCaches();
                    this.financialTransactionChanged.emit();
                },
                error: (err) => {
                    const msg = err?.error?.error || 'Failed to update transaction';
                    this.alertService.showMessage('Update failed', msg, MessageSeverity.error);
                    this.isSaving = false;
                }
            });
        } else {
            // Route new entries through FinancialManagementService for proper validation
            this.postViaService(fv).subscribe({
                next: () => {
                    this.rememberLastUsedValues();
                    this.alertService.showMessage('Transaction created successfully', '', MessageSeverity.success);
                    this.isSaving = false;
                    this.isSavingAndAdding = false;
                    this.goBack();
                    this.transactionService.ClearAllCaches();
                    this.financialTransactionChanged.emit();
                },
                error: (err) => {
                    const msg = err?.error?.error || JSON.stringify(err);
                    this.alertService.showMessage('Failed to create transaction', msg, MessageSeverity.error);
                    this.isSaving = false;
                    this.isSavingAndAdding = false;
                }
            });
        }
    }


    //
    // Save & Add Another — saves the current transaction, then re-opens the form
    // with last-used category/office/currency pre-populated for fast repeat entry.
    //
    public saveAndAddAnother(): void {
        if (this.isSaving || this.isSavingAndAdding || this.isEditMode) return;

        if (!this.transactionService.userIsSchedulerFinancialTransactionWriter()) {
            this.alertService.showMessage(
                `${this.authService.currentUser?.userName} does not have permission to write Financial Transactions`, '', MessageSeverity.info
            );
            return;
        }

        if (!this.txForm.valid) {
            this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
            this.txForm.markAllAsTouched();
            return;
        }

        this.isSavingAndAdding = true;
        const fv = this.txForm.getRawValue();

        const submitData: FinancialTransactionSubmitData = {
            id: 0,
            financialCategoryId: Number(fv.financialCategoryId),
            financialOfficeId: fv.financialOfficeId ? Number(fv.financialOfficeId) : null,
            scheduledEventId: fv.scheduledEventId ? Number(fv.scheduledEventId) : null,
            contactId: fv.contactId ? Number(fv.contactId) : null,
            clientId: fv.clientId ? Number(fv.clientId) : null,
            contactRole: fv.contactRole?.trim() || null,
            taxCodeId: fv.taxCodeId ? Number(fv.taxCodeId) : null,
            fiscalPeriodId: fv.fiscalPeriodId ? Number(fv.fiscalPeriodId) : null,
            paymentTypeId: fv.paymentTypeId ? Number(fv.paymentTypeId) : null,
            transactionDate: dateTimeLocalToIsoUtc(fv.transactionDate!.trim())!,
            description: fv.description!.trim(),
            amount: Number(fv.amount),
            taxAmount: Number(fv.taxAmount),
            totalAmount: Number(fv.totalAmount),
            isRevenue: !!fv.isRevenue,
            journalEntryType: fv.journalEntryType?.trim() || null,
            referenceNumber: fv.referenceNumber?.trim() || null,
            notes: fv.notes?.trim() || null,
            currencyId: Number(fv.currencyId),
            exportedDate: null,
            externalId: null,
            externalSystemName: null,
            versionNumber: 0,
            active: true,
            deleted: false,
        };

        // Route new entries through FinancialManagementService for proper validation
        this.postViaService(fv).subscribe({
            next: () => {
                this.rememberLastUsedValues();
                this.alertService.showMessage('Transaction saved — add another!', '', MessageSeverity.success);
                this.isSavingAndAdding = false;
                this.transactionService.ClearAllCaches();
                this.financialTransactionChanged.emit();

                //
                // Reset the form but keep remembered values pre-populated
                //
                this.resetForm();
                this.applyLastUsedValues();
            },
            error: (err) => {
                const msg = err?.error?.error || JSON.stringify(err);
                this.alertService.showMessage('Failed to create transaction', msg, MessageSeverity.error);
                this.isSavingAndAdding = false;
            }
        });
    }


    /**
     * Routes new transaction creation through FinancialManagementService.
     * Uses /RecordRevenue or /RecordExpense depending on isRevenue flag.
     * These endpoints provide fiscal period validation, category validation,
     * journal entry type assignment, and structured audit logging.
     */
    private postViaService(fv: any) {
        const isRevenue = !!fv.isRevenue;
        const endpoint = isRevenue ? 'RecordRevenue' : 'RecordExpense';
        const body = {
            financialCategoryId: Number(fv.financialCategoryId),
            transactionDate: dateTimeLocalToIsoUtc(fv.transactionDate!.trim()),
            amount: Number(fv.amount),
            taxAmount: Number(fv.taxAmount),
            description: fv.description!.trim(),
            currencyId: Number(fv.currencyId),
            financialOfficeId: fv.financialOfficeId ? Number(fv.financialOfficeId) : null,
            scheduledEventId: fv.scheduledEventId ? Number(fv.scheduledEventId) : null,
            contactId: fv.contactId ? Number(fv.contactId) : null,
            clientId: fv.clientId ? Number(fv.clientId) : null,
            referenceNumber: fv.referenceNumber?.trim() || null,
            notes: fv.notes?.trim() || null,
        };
        const headers = this.authService.GetAuthenticationHeaders()
            .set('Content-Type', 'application/json');
        return this.http.post(`${this.baseUrl}api/FinancialTransactions/${endpoint}`, body, { headers });
    }


    /**
     * Routes transaction edits through FinancialManagementService.
     * Uses PUT /api/FinancialTransactions/{id}/Update for fiscal period
     * validation, change history, and structured audit logging.
     */
    private putViaService(fv: any) {
        const body = {
            financialCategoryId: Number(fv.financialCategoryId),
            transactionDate: dateTimeLocalToIsoUtc(fv.transactionDate!.trim()),
            amount: Number(fv.amount),
            taxAmount: Number(fv.taxAmount),
            description: fv.description!.trim(),
            currencyId: Number(fv.currencyId),
            financialOfficeId: fv.financialOfficeId ? Number(fv.financialOfficeId) : null,
            scheduledEventId: fv.scheduledEventId ? Number(fv.scheduledEventId) : null,
            contactId: fv.contactId ? Number(fv.contactId) : null,
            clientId: fv.clientId ? Number(fv.clientId) : null,
            referenceNumber: fv.referenceNumber?.trim() || null,
            notes: fv.notes?.trim() || null,
        };
        const headers = this.authService.GetAuthenticationHeaders()
            .set('Content-Type', 'application/json');
        return this.http.put(`${this.baseUrl}api/FinancialTransactions/${this.editId}/Update`, body, { headers });
    }


    //
    // Helper to get category color for display in the dropdown
    //
    public getCategoryColor(catId: number | bigint | null): string {
        if (!catId) return 'transparent';
        const cat = this.categories.find(c => Number(c.id) === Number(catId));
        return cat?.color || 'transparent';
    }


    //
    // Helper method to determine if a field should be hidden based on the hiddenFields input.
    //
    public isFieldHidden(fieldName: string): boolean {
        if (!this.hiddenFields) return false;
        return this.hiddenFields.includes(fieldName);
    }


    // -------------------------------------------------------------------------
    // Quick-Entry: localStorage memory for repeat entries
    // -------------------------------------------------------------------------
    private static readonly LAST_USED_KEY = 'scheduler_tx_lastUsed';

    private rememberLastUsedValues(): void {
        try {
            const fv = this.txForm.getRawValue();
            const memory: Record<string, any> = {};
            if (fv.financialCategoryId) memory['financialCategoryId'] = Number(fv.financialCategoryId);
            if (fv.financialOfficeId) memory['financialOfficeId'] = Number(fv.financialOfficeId);
            if (fv.currencyId) memory['currencyId'] = Number(fv.currencyId);
            if (fv.paymentTypeId) memory['paymentTypeId'] = Number(fv.paymentTypeId);
            localStorage.setItem(FinancialTransactionCustomAddEditComponent.LAST_USED_KEY, JSON.stringify(memory));
        } catch { /* localStorage not available — no-op */ }
    }

    private applyLastUsedValues(): void {
        try {
            const raw = localStorage.getItem(FinancialTransactionCustomAddEditComponent.LAST_USED_KEY);
            if (!raw) {
                this.autoSelectDefaultCurrency();
                return;
            }
            const memory = JSON.parse(raw);
            const patch: Record<string, any> = {};
            if (memory.financialCategoryId) patch['financialCategoryId'] = memory.financialCategoryId;
            if (memory.financialOfficeId) patch['financialOfficeId'] = memory.financialOfficeId;
            if (memory.currencyId) patch['currencyId'] = memory.currencyId;
            if (memory.paymentTypeId) patch['paymentTypeId'] = memory.paymentTypeId;
            this.txForm.patchValue(patch);

            // Auto-set isRevenue from remembered category
            if (patch['financialCategoryId']) {
                this.onCategoryChange();
            }
        } catch {
            this.autoSelectDefaultCurrency();
        }
    }


    // -------------------------------------------------------------------------
    // Quick-Entry: Auto-select default currency (CAD)
    // -------------------------------------------------------------------------
    private autoSelectDefaultCurrency(): void {
        this.currencies$.subscribe(currencies => {
            if (!currencies || this.txForm.get('currencyId')?.value) return;
            const cad = currencies.find(c => c.code === 'CAD' || c.name?.toUpperCase().includes('CANAD'));
            if (cad) {
                this.txForm.get('currencyId')?.setValue(cad.id);
            } else if (currencies.length === 1) {
                // Only one currency in the system — use it
                this.txForm.get('currencyId')?.setValue(currencies[0].id);
            }
        });
    }
}
