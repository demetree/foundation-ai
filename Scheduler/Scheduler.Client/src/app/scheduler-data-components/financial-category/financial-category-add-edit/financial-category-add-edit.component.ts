/*
   GENERATED FORM FOR THE FINANCIALCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from FinancialCategory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to financial-category-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FinancialCategoryService, FinancialCategoryData, FinancialCategorySubmitData } from '../../../scheduler-data-services/financial-category.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface FinancialCategoryFormValues {
  name: string,
  description: string,
  code: string,
  isRevenue: boolean,
  accountType: string,
  parentFinancialCategoryId: number | bigint | null,       // For FK link number
  isTaxApplicable: boolean,
  defaultAmount: string | null,     // Stored as string for form input, converted to number on submit.
  externalAccountId: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-financial-category-add-edit',
  templateUrl: './financial-category-add-edit.component.html',
  styleUrls: ['./financial-category-add-edit.component.scss']
})
export class FinancialCategoryAddEditComponent {
  @ViewChild('financialCategoryModal') financialCategoryModal!: TemplateRef<any>;
  @Output() financialCategoryChanged = new Subject<FinancialCategoryData[]>();
  @Input() financialCategorySubmitData: FinancialCategorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<FinancialCategoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public financialCategoryForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        code: ['', Validators.required],
        isRevenue: [false],
        accountType: ['', Validators.required],
        parentFinancialCategoryId: [null],
        isTaxApplicable: [false],
        defaultAmount: [''],
        externalAccountId: [''],
        sequence: [''],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();

  constructor(
    private modalService: NgbModal,
    private financialCategoryService: FinancialCategoryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(financialCategoryData?: FinancialCategoryData) {

    if (financialCategoryData != null) {

      if (!this.financialCategoryService.userIsSchedulerFinancialCategoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Financial Categories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.financialCategorySubmitData = this.financialCategoryService.ConvertToFinancialCategorySubmitData(financialCategoryData);
      this.isEditMode = true;
      this.objectGuid = financialCategoryData.objectGuid;

      this.buildFormValues(financialCategoryData);

    } else {

      if (!this.financialCategoryService.userIsSchedulerFinancialCategoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Financial Categories`,
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
        this.financialCategoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.financialCategoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.financialCategoryModal, {
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

    if (this.financialCategoryService.userIsSchedulerFinancialCategoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Financial Categories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.financialCategoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.financialCategoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.financialCategoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const financialCategorySubmitData: FinancialCategorySubmitData = {
        id: this.financialCategorySubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        code: formValue.code!.trim(),
        isRevenue: !!formValue.isRevenue,
        accountType: formValue.accountType!.trim(),
        parentFinancialCategoryId: formValue.parentFinancialCategoryId ? Number(formValue.parentFinancialCategoryId) : null,
        isTaxApplicable: !!formValue.isTaxApplicable,
        defaultAmount: formValue.defaultAmount ? Number(formValue.defaultAmount) : null,
        externalAccountId: formValue.externalAccountId?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.financialCategorySubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateFinancialCategory(financialCategorySubmitData);
      } else {
        this.addFinancialCategory(financialCategorySubmitData);
      }
  }

  private addFinancialCategory(financialCategoryData: FinancialCategorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    financialCategoryData.versionNumber = 0;
    financialCategoryData.active = true;
    financialCategoryData.deleted = false;
    this.financialCategoryService.PostFinancialCategory(financialCategoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newFinancialCategory) => {

        this.financialCategoryService.ClearAllCaches();

        this.financialCategoryChanged.next([newFinancialCategory]);

        this.alertService.showMessage("Financial Category added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/financialcategory', newFinancialCategory.id]);
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
                                   'You do not have permission to save this Financial Category.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Financial Category.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Financial Category could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateFinancialCategory(financialCategoryData: FinancialCategorySubmitData) {
    this.financialCategoryService.PutFinancialCategory(financialCategoryData.id, financialCategoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedFinancialCategory) => {

        this.financialCategoryService.ClearAllCaches();

        this.financialCategoryChanged.next([updatedFinancialCategory]);

        this.alertService.showMessage("Financial Category updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Financial Category.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Financial Category.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Financial Category could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(financialCategoryData: FinancialCategoryData | null) {

    if (financialCategoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.financialCategoryForm.reset({
        name: '',
        description: '',
        code: '',
        isRevenue: false,
        accountType: '',
        parentFinancialCategoryId: null,
        isTaxApplicable: false,
        defaultAmount: '',
        externalAccountId: '',
        sequence: '',
        color: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.financialCategoryForm.reset({
        name: financialCategoryData.name ?? '',
        description: financialCategoryData.description ?? '',
        code: financialCategoryData.code ?? '',
        isRevenue: financialCategoryData.isRevenue ?? false,
        accountType: financialCategoryData.accountType ?? '',
        parentFinancialCategoryId: financialCategoryData.parentFinancialCategoryId,
        isTaxApplicable: financialCategoryData.isTaxApplicable ?? false,
        defaultAmount: financialCategoryData.defaultAmount?.toString() ?? '',
        externalAccountId: financialCategoryData.externalAccountId ?? '',
        sequence: financialCategoryData.sequence?.toString() ?? '',
        color: financialCategoryData.color ?? '',
        versionNumber: financialCategoryData.versionNumber?.toString() ?? '',
        active: financialCategoryData.active ?? true,
        deleted: financialCategoryData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.financialCategoryForm.markAsPristine();
    this.financialCategoryForm.markAsUntouched();
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


  public userIsSchedulerFinancialCategoryReader(): boolean {
    return this.financialCategoryService.userIsSchedulerFinancialCategoryReader();
  }

  public userIsSchedulerFinancialCategoryWriter(): boolean {
    return this.financialCategoryService.userIsSchedulerFinancialCategoryWriter();
  }
}
