/*
   GENERATED FORM FOR THE BUDGET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Budget table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to budget-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BudgetService, BudgetData, BudgetSubmitData } from '../../../scheduler-data-services/budget.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { FiscalPeriodService } from '../../../scheduler-data-services/fiscal-period.service';
import { FinancialOfficeService } from '../../../scheduler-data-services/financial-office.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BudgetFormValues {
  financialCategoryId: number | bigint,       // For FK link number
  fiscalPeriodId: number | bigint,       // For FK link number
  financialOfficeId: number | bigint | null,       // For FK link number
  budgetedAmount: string,     // Stored as string for form input, converted to number on submit.
  revisedAmount: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  currencyId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-budget-add-edit',
  templateUrl: './budget-add-edit.component.html',
  styleUrls: ['./budget-add-edit.component.scss']
})
export class BudgetAddEditComponent {
  @ViewChild('budgetModal') budgetModal!: TemplateRef<any>;
  @Output() budgetChanged = new Subject<BudgetData[]>();
  @Input() budgetSubmitData: BudgetSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BudgetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public budgetForm: FormGroup = this.fb.group({
        financialCategoryId: [null, Validators.required],
        fiscalPeriodId: [null, Validators.required],
        financialOfficeId: [null],
        budgetedAmount: ['', Validators.required],
        revisedAmount: [''],
        notes: [''],
        currencyId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  budgets$ = this.budgetService.GetBudgetList();
  financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();
  fiscalPeriods$ = this.fiscalPeriodService.GetFiscalPeriodList();
  financialOffices$ = this.financialOfficeService.GetFinancialOfficeList();
  currencies$ = this.currencyService.GetCurrencyList();

  constructor(
    private modalService: NgbModal,
    private budgetService: BudgetService,
    private financialCategoryService: FinancialCategoryService,
    private fiscalPeriodService: FiscalPeriodService,
    private financialOfficeService: FinancialOfficeService,
    private currencyService: CurrencyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(budgetData?: BudgetData) {

    if (budgetData != null) {

      if (!this.budgetService.userIsSchedulerBudgetReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Budgets`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.budgetSubmitData = this.budgetService.ConvertToBudgetSubmitData(budgetData);
      this.isEditMode = true;
      this.objectGuid = budgetData.objectGuid;

      this.buildFormValues(budgetData);

    } else {

      if (!this.budgetService.userIsSchedulerBudgetWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Budgets`,
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
        this.budgetForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.budgetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.budgetModal, {
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

    if (this.budgetService.userIsSchedulerBudgetWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Budgets`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.budgetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.budgetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.budgetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const budgetSubmitData: BudgetSubmitData = {
        id: this.budgetSubmitData?.id || 0,
        financialCategoryId: Number(formValue.financialCategoryId),
        fiscalPeriodId: Number(formValue.fiscalPeriodId),
        financialOfficeId: formValue.financialOfficeId ? Number(formValue.financialOfficeId) : null,
        budgetedAmount: Number(formValue.budgetedAmount),
        revisedAmount: formValue.revisedAmount ? Number(formValue.revisedAmount) : null,
        notes: formValue.notes?.trim() || null,
        currencyId: Number(formValue.currencyId),
        versionNumber: this.budgetSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBudget(budgetSubmitData);
      } else {
        this.addBudget(budgetSubmitData);
      }
  }

  private addBudget(budgetData: BudgetSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    budgetData.versionNumber = 0;
    budgetData.active = true;
    budgetData.deleted = false;
    this.budgetService.PostBudget(budgetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBudget) => {

        this.budgetService.ClearAllCaches();

        this.budgetChanged.next([newBudget]);

        this.alertService.showMessage("Budget added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/budget', newBudget.id]);
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
                                   'You do not have permission to save this Budget.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Budget.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Budget could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBudget(budgetData: BudgetSubmitData) {
    this.budgetService.PutBudget(budgetData.id, budgetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBudget) => {

        this.budgetService.ClearAllCaches();

        this.budgetChanged.next([updatedBudget]);

        this.alertService.showMessage("Budget updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Budget.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Budget.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Budget could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(budgetData: BudgetData | null) {

    if (budgetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.budgetForm.reset({
        financialCategoryId: null,
        fiscalPeriodId: null,
        financialOfficeId: null,
        budgetedAmount: '',
        revisedAmount: '',
        notes: '',
        currencyId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.budgetForm.reset({
        financialCategoryId: budgetData.financialCategoryId,
        fiscalPeriodId: budgetData.fiscalPeriodId,
        financialOfficeId: budgetData.financialOfficeId,
        budgetedAmount: budgetData.budgetedAmount?.toString() ?? '',
        revisedAmount: budgetData.revisedAmount?.toString() ?? '',
        notes: budgetData.notes ?? '',
        currencyId: budgetData.currencyId,
        versionNumber: budgetData.versionNumber?.toString() ?? '',
        active: budgetData.active ?? true,
        deleted: budgetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.budgetForm.markAsPristine();
    this.budgetForm.markAsUntouched();
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


  public userIsSchedulerBudgetReader(): boolean {
    return this.budgetService.userIsSchedulerBudgetReader();
  }

  public userIsSchedulerBudgetWriter(): boolean {
    return this.budgetService.userIsSchedulerBudgetWriter();
  }
}
