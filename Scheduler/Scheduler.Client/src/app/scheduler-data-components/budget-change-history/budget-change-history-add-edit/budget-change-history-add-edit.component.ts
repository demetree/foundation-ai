/*
   GENERATED FORM FOR THE BUDGETCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BudgetChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to budget-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BudgetChangeHistoryService, BudgetChangeHistoryData, BudgetChangeHistorySubmitData } from '../../../scheduler-data-services/budget-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BudgetService } from '../../../scheduler-data-services/budget.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BudgetChangeHistoryFormValues {
  budgetId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-budget-change-history-add-edit',
  templateUrl: './budget-change-history-add-edit.component.html',
  styleUrls: ['./budget-change-history-add-edit.component.scss']
})
export class BudgetChangeHistoryAddEditComponent {
  @ViewChild('budgetChangeHistoryModal') budgetChangeHistoryModal!: TemplateRef<any>;
  @Output() budgetChangeHistoryChanged = new Subject<BudgetChangeHistoryData[]>();
  @Input() budgetChangeHistorySubmitData: BudgetChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BudgetChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public budgetChangeHistoryForm: FormGroup = this.fb.group({
        budgetId: [null, Validators.required],
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

  budgetChangeHistories$ = this.budgetChangeHistoryService.GetBudgetChangeHistoryList();
  budgets$ = this.budgetService.GetBudgetList();

  constructor(
    private modalService: NgbModal,
    private budgetChangeHistoryService: BudgetChangeHistoryService,
    private budgetService: BudgetService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(budgetChangeHistoryData?: BudgetChangeHistoryData) {

    if (budgetChangeHistoryData != null) {

      if (!this.budgetChangeHistoryService.userIsSchedulerBudgetChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Budget Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.budgetChangeHistorySubmitData = this.budgetChangeHistoryService.ConvertToBudgetChangeHistorySubmitData(budgetChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(budgetChangeHistoryData);

    } else {

      if (!this.budgetChangeHistoryService.userIsSchedulerBudgetChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Budget Change Histories`,
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
        this.budgetChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.budgetChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.budgetChangeHistoryModal, {
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

    if (this.budgetChangeHistoryService.userIsSchedulerBudgetChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Budget Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.budgetChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.budgetChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.budgetChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const budgetChangeHistorySubmitData: BudgetChangeHistorySubmitData = {
        id: this.budgetChangeHistorySubmitData?.id || 0,
        budgetId: Number(formValue.budgetId),
        versionNumber: this.budgetChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateBudgetChangeHistory(budgetChangeHistorySubmitData);
      } else {
        this.addBudgetChangeHistory(budgetChangeHistorySubmitData);
      }
  }

  private addBudgetChangeHistory(budgetChangeHistoryData: BudgetChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    budgetChangeHistoryData.versionNumber = 0;
    this.budgetChangeHistoryService.PostBudgetChangeHistory(budgetChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBudgetChangeHistory) => {

        this.budgetChangeHistoryService.ClearAllCaches();

        this.budgetChangeHistoryChanged.next([newBudgetChangeHistory]);

        this.alertService.showMessage("Budget Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/budgetchangehistory', newBudgetChangeHistory.id]);
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
                                   'You do not have permission to save this Budget Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Budget Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Budget Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBudgetChangeHistory(budgetChangeHistoryData: BudgetChangeHistorySubmitData) {
    this.budgetChangeHistoryService.PutBudgetChangeHistory(budgetChangeHistoryData.id, budgetChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBudgetChangeHistory) => {

        this.budgetChangeHistoryService.ClearAllCaches();

        this.budgetChangeHistoryChanged.next([updatedBudgetChangeHistory]);

        this.alertService.showMessage("Budget Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Budget Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Budget Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Budget Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(budgetChangeHistoryData: BudgetChangeHistoryData | null) {

    if (budgetChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.budgetChangeHistoryForm.reset({
        budgetId: null,
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
        this.budgetChangeHistoryForm.reset({
        budgetId: budgetChangeHistoryData.budgetId,
        versionNumber: budgetChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(budgetChangeHistoryData.timeStamp) ?? '',
        userId: budgetChangeHistoryData.userId?.toString() ?? '',
        data: budgetChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.budgetChangeHistoryForm.markAsPristine();
    this.budgetChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerBudgetChangeHistoryReader(): boolean {
    return this.budgetChangeHistoryService.userIsSchedulerBudgetChangeHistoryReader();
  }

  public userIsSchedulerBudgetChangeHistoryWriter(): boolean {
    return this.budgetChangeHistoryService.userIsSchedulerBudgetChangeHistoryWriter();
  }
}
