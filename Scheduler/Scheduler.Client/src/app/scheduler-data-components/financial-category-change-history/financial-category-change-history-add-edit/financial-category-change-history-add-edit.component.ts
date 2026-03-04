/*
   GENERATED FORM FOR THE FINANCIALCATEGORYCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from FinancialCategoryChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to financial-category-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FinancialCategoryChangeHistoryService, FinancialCategoryChangeHistoryData, FinancialCategoryChangeHistorySubmitData } from '../../../scheduler-data-services/financial-category-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface FinancialCategoryChangeHistoryFormValues {
  financialCategoryId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-financial-category-change-history-add-edit',
  templateUrl: './financial-category-change-history-add-edit.component.html',
  styleUrls: ['./financial-category-change-history-add-edit.component.scss']
})
export class FinancialCategoryChangeHistoryAddEditComponent {
  @ViewChild('financialCategoryChangeHistoryModal') financialCategoryChangeHistoryModal!: TemplateRef<any>;
  @Output() financialCategoryChangeHistoryChanged = new Subject<FinancialCategoryChangeHistoryData[]>();
  @Input() financialCategoryChangeHistorySubmitData: FinancialCategoryChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<FinancialCategoryChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public financialCategoryChangeHistoryForm: FormGroup = this.fb.group({
        financialCategoryId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  financialCategoryChangeHistories$ = this.financialCategoryChangeHistoryService.GetFinancialCategoryChangeHistoryList();
  financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();

  constructor(
    private modalService: NgbModal,
    private financialCategoryChangeHistoryService: FinancialCategoryChangeHistoryService,
    private financialCategoryService: FinancialCategoryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(financialCategoryChangeHistoryData?: FinancialCategoryChangeHistoryData) {

    if (financialCategoryChangeHistoryData != null) {

      if (!this.financialCategoryChangeHistoryService.userIsSchedulerFinancialCategoryChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Financial Category Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.financialCategoryChangeHistorySubmitData = this.financialCategoryChangeHistoryService.ConvertToFinancialCategoryChangeHistorySubmitData(financialCategoryChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(financialCategoryChangeHistoryData);

    } else {

      if (!this.financialCategoryChangeHistoryService.userIsSchedulerFinancialCategoryChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Financial Category Change Histories`,
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
        this.financialCategoryChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.financialCategoryChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.financialCategoryChangeHistoryModal, {
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

    if (this.financialCategoryChangeHistoryService.userIsSchedulerFinancialCategoryChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Financial Category Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.financialCategoryChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.financialCategoryChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.financialCategoryChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const financialCategoryChangeHistorySubmitData: FinancialCategoryChangeHistorySubmitData = {
        id: this.financialCategoryChangeHistorySubmitData?.id || 0,
        financialCategoryId: Number(formValue.financialCategoryId),
        versionNumber: this.financialCategoryChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateFinancialCategoryChangeHistory(financialCategoryChangeHistorySubmitData);
      } else {
        this.addFinancialCategoryChangeHistory(financialCategoryChangeHistorySubmitData);
      }
  }

  private addFinancialCategoryChangeHistory(financialCategoryChangeHistoryData: FinancialCategoryChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    financialCategoryChangeHistoryData.versionNumber = 0;
    this.financialCategoryChangeHistoryService.PostFinancialCategoryChangeHistory(financialCategoryChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newFinancialCategoryChangeHistory) => {

        this.financialCategoryChangeHistoryService.ClearAllCaches();

        this.financialCategoryChangeHistoryChanged.next([newFinancialCategoryChangeHistory]);

        this.alertService.showMessage("Financial Category Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/financialcategorychangehistory', newFinancialCategoryChangeHistory.id]);
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
                                   'You do not have permission to save this Financial Category Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Financial Category Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Financial Category Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateFinancialCategoryChangeHistory(financialCategoryChangeHistoryData: FinancialCategoryChangeHistorySubmitData) {
    this.financialCategoryChangeHistoryService.PutFinancialCategoryChangeHistory(financialCategoryChangeHistoryData.id, financialCategoryChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedFinancialCategoryChangeHistory) => {

        this.financialCategoryChangeHistoryService.ClearAllCaches();

        this.financialCategoryChangeHistoryChanged.next([updatedFinancialCategoryChangeHistory]);

        this.alertService.showMessage("Financial Category Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Financial Category Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Financial Category Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Financial Category Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(financialCategoryChangeHistoryData: FinancialCategoryChangeHistoryData | null) {

    if (financialCategoryChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.financialCategoryChangeHistoryForm.reset({
        financialCategoryId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.financialCategoryChangeHistoryForm.reset({
        financialCategoryId: financialCategoryChangeHistoryData.financialCategoryId,
        versionNumber: financialCategoryChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(financialCategoryChangeHistoryData.timeStamp) ?? '',
        userId: financialCategoryChangeHistoryData.userId?.toString() ?? '',
        data: financialCategoryChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.financialCategoryChangeHistoryForm.markAsPristine();
    this.financialCategoryChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerFinancialCategoryChangeHistoryReader(): boolean {
    return this.financialCategoryChangeHistoryService.userIsSchedulerFinancialCategoryChangeHistoryReader();
  }

  public userIsSchedulerFinancialCategoryChangeHistoryWriter(): boolean {
    return this.financialCategoryChangeHistoryService.userIsSchedulerFinancialCategoryChangeHistoryWriter();
  }
}
