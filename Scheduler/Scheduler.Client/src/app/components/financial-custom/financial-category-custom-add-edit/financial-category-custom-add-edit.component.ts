import { Component, ViewChild, TemplateRef, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { FinancialCategoryService, FinancialCategoryData, FinancialCategorySubmitData } from '../../../scheduler-data-services/financial-category.service';
import { AccountTypeService, AccountTypeData } from '../../../scheduler-data-services/account-type.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { finalize } from 'rxjs';

@Component({
    selector: 'app-financial-category-custom-add-edit',
    templateUrl: './financial-category-custom-add-edit.component.html',
    styleUrls: ['./financial-category-custom-add-edit.component.scss']
})
export class FinancialCategoryCustomAddEditComponent {

    @ViewChild('catModal') catModal!: TemplateRef<any>;
    @Output() financialCategoryChanged = new EventEmitter<FinancialCategoryData[]>();

    @Input() navigateToDetailsAfterAdd: boolean = false;
    @Input() showAddButton: boolean = false;
    @Input() hiddenFields: string[] = [];

    //
    // Pre-seeded data (partial) for add mode — allows parent to set defaults like financialOfficeId
    //
    @Input() preSeededData: Record<string, any> | null = null;

    public catForm!: FormGroup;
    private modalRef: NgbModalRef | undefined;
    public isEditMode = false;
    public modalIsDisplayed = false;
    public isSaving = false;
    public showMoreDetails = false;

    //
    // Lookup data
    //
    public accountTypes: AccountTypeData[] = [];
    public offices: FinancialOfficeData[] = [];
    public parentCategories: FinancialCategoryData[] = [];

    //
    // Private edit-mode state
    //
    private editId: number | bigint = 0;
    private editVersionNumber: number | bigint = 0;

    constructor(
        private modalService: NgbModal,
        private categoryService: FinancialCategoryService,
        private accountTypeService: AccountTypeService,
        private officeService: FinancialOfficeService,
        private authService: AuthService,
        private alertService: AlertService,
        private router: Router,
        private fb: FormBuilder
    ) {
        this.initForm();
    }


    private initForm(): void {
        this.catForm = this.fb.group({
            name: ['', Validators.required],
            code: ['', Validators.required],
            description: ['', Validators.required],
            accountTypeId: [null, Validators.required],
            financialOfficeId: [null],
            parentFinancialCategoryId: [null],
            isTaxApplicable: [false],
            defaultAmount: [null],
            // More details (collapsible)
            sequence: [null],
            externalAccountId: [''],
            color: [''],
        });
    }


    //
    // Load lookup data
    //
    private loadLookups(): void {
        this.accountTypeService.GetAccountTypeList({
            active: true, deleted: false
        }).subscribe({
            next: (data) => {
                this.accountTypes = data ?? [];
            }
        });

        this.officeService.GetFinancialOfficeList({
            active: true, deleted: false
        }).subscribe({
            next: (data) => { this.offices = data ?? []; }
        });

        this.categoryService.GetFinancialCategoryList({
            active: true, deleted: false
        }).subscribe({
            next: (data) => { this.parentCategories = data ?? []; }
        });
    }


    //
    // Public API — called by parent via @ViewChild
    //
    public openModal(catData?: FinancialCategoryData): void {

        if (catData) {

            if (!this.categoryService.userIsSchedulerFinancialCategoryReader()) {
                this.alertService.showMessage(
                    `${this.authService.currentUser?.userName} does not have permission to read Financial Categories`, '', MessageSeverity.info
                );
                return;
            }

            this.isEditMode = true;
            this.editId = catData.id;
            this.editVersionNumber = catData.versionNumber;
            this.populateForm(catData);

            // Auto-expand more details if optional fields have values
            this.showMoreDetails = !!(catData.sequence || catData.externalAccountId || catData.color);

        } else {

            if (!this.categoryService.userIsSchedulerFinancialCategoryWriter()) {
                this.alertService.showMessage(
                    `${this.authService.currentUser?.userName} does not have permission to write Financial Categories`, '', MessageSeverity.info
                );
                return;
            }

            this.isEditMode = false;
            this.editId = 0;
            this.editVersionNumber = 0;
            this.showMoreDetails = false;
            this.resetForm();

            //
            // Apply pre-seeded data if provided
            //
            if (this.preSeededData) {
                this.catForm.patchValue(this.preSeededData);
            }
        }

        this.loadLookups();

        //
        // Disable validators for hidden fields to prevent form invalidation
        //
        for (const fieldName of this.hiddenFields) {
            const control = this.catForm.get(fieldName);
            if (control) {
                control.clearValidators();
                control.updateValueAndValidity();
            }
        }

        this.modalRef = this.modalService.open(this.catModal, {
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


    //
    // Form helpers
    //
    private resetForm(): void {
        this.catForm.reset({
            name: '',
            code: '',
            description: '',
            accountTypeId: null,
            financialOfficeId: null,
            parentFinancialCategoryId: null,
            isTaxApplicable: false,
            defaultAmount: null,
            sequence: null,
            externalAccountId: '',
            color: '',
        });
        this.catForm.markAsPristine();
        this.catForm.markAsUntouched();
    }


    private populateForm(cat: FinancialCategoryData): void {
        this.catForm.reset({
            name: cat.name ?? '',
            code: cat.code ?? '',
            description: cat.description ?? '',
            accountTypeId: cat.accountTypeId,
            financialOfficeId: cat.financialOfficeId,
            parentFinancialCategoryId: cat.parentFinancialCategoryId,
            isTaxApplicable: cat.isTaxApplicable ?? false,
            defaultAmount: cat.defaultAmount,
            sequence: cat.sequence,
            externalAccountId: cat.externalAccountId ?? '',
            color: cat.color ?? '',
        }, { emitEvent: false });
        this.catForm.markAsPristine();
        this.catForm.markAsUntouched();
    }


    //
    // Submit
    //
    public submitForm(): void {
        if (this.isSaving) return;

        if (!this.categoryService.userIsSchedulerFinancialCategoryWriter()) {
            this.alertService.showMessage(
                `${this.authService.currentUser?.userName} does not have permission to write Financial Categories`, '', MessageSeverity.info
            );
            return;
        }

        if (!this.catForm.valid) {
            this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
            this.catForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        const formValue = this.catForm.getRawValue();

        const submitData: FinancialCategorySubmitData = {
            id: this.isEditMode ? Number(this.editId) : 0,
            name: formValue.name!.trim(),
            code: formValue.code!.trim(),
            description: formValue.description!.trim(),
            accountTypeId: Number(formValue.accountTypeId),
            financialOfficeId: formValue.financialOfficeId ? Number(formValue.financialOfficeId) : null,
            parentFinancialCategoryId: formValue.parentFinancialCategoryId ? Number(formValue.parentFinancialCategoryId) : null,
            isTaxApplicable: !!formValue.isTaxApplicable,
            defaultAmount: formValue.defaultAmount ? Number(formValue.defaultAmount) : null,
            sequence: formValue.sequence ? Number(formValue.sequence) : null,
            externalAccountId: formValue.externalAccountId?.trim() || null,
            color: formValue.color?.trim() || null,
            versionNumber: this.isEditMode ? Number(this.editVersionNumber) : 0,
            active: true,
            deleted: false,
        };

        if (this.isEditMode) {
            this.updateCategory(submitData);
        } else {
            this.addCategory(submitData);
        }
    }


    private addCategory(data: FinancialCategorySubmitData): void {
        data.versionNumber = 0;
        data.active = true;
        data.deleted = false;

        this.categoryService.PostFinancialCategory(data).pipe(
            finalize(() => this.isSaving = false)
        ).subscribe({
            next: (newCategory) => {
                this.categoryService.ClearAllCaches();
                this.financialCategoryChanged.next([newCategory]);
                this.alertService.showMessage('Category added successfully', '', MessageSeverity.success);
                this.closeModal();

                if (this.navigateToDetailsAfterAdd) {
                    this.router.navigate(['/financialcategory', newCategory.id]);
                }
            },
            error: (err) => {
                this.handleError(err);
            }
        });
    }


    private updateCategory(data: FinancialCategorySubmitData): void {
        this.categoryService.PutFinancialCategory(data.id, data).pipe(
            finalize(() => this.isSaving = false)
        ).subscribe({
            next: (updatedCategory) => {
                this.categoryService.ClearAllCaches();
                this.financialCategoryChanged.next([updatedCategory]);
                this.alertService.showMessage('Category updated successfully', '', MessageSeverity.success);
                this.closeModal();
            },
            error: (err) => {
                this.handleError(err);
            }
        });
    }


    private handleError(err: any): void {
        let errorMessage: string;

        if (err instanceof Error) {
            errorMessage = err.message || 'An unexpected error occurred.';
        } else if (err.status && err.error) {
            if (err.status === 403) {
                errorMessage = err.error?.message || 'You do not have permission to save this category.';
            } else {
                errorMessage = err.error?.message || err.error?.error_description || err.error?.detail || 'An error occurred while saving the category.';
            }
        } else {
            errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Category could not be saved', errorMessage, MessageSeverity.error);
    }


    //
    // Helper method to determine if a field should be hidden based on the hiddenFields input.
    //
    public isFieldHidden(fieldName: string): boolean {
        if (!this.hiddenFields) return false;
        return this.hiddenFields.includes(fieldName);
    }
}
