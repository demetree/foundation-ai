/*
   GENERATED FORM FOR THE BRICKECONOMYTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickEconomyTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-economy-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickEconomyTransactionService, BrickEconomyTransactionData, BrickEconomyTransactionSubmitData } from '../../../bmc-data-services/brick-economy-transaction.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickEconomyTransactionFormValues {
  transactionDate: string | null,
  direction: string,
  methodName: string,
  requestSummary: string | null,
  success: boolean,
  errorMessage: string | null,
  triggeredBy: string,
  recordCount: string | null,     // Stored as string for form input, converted to number on submit.
  dailyQuotaRemaining: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-economy-transaction-add-edit',
  templateUrl: './brick-economy-transaction-add-edit.component.html',
  styleUrls: ['./brick-economy-transaction-add-edit.component.scss']
})
export class BrickEconomyTransactionAddEditComponent {
  @ViewChild('brickEconomyTransactionModal') brickEconomyTransactionModal!: TemplateRef<any>;
  @Output() brickEconomyTransactionChanged = new Subject<BrickEconomyTransactionData[]>();
  @Input() brickEconomyTransactionSubmitData: BrickEconomyTransactionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickEconomyTransactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickEconomyTransactionForm: FormGroup = this.fb.group({
        transactionDate: [''],
        direction: ['', Validators.required],
        methodName: ['', Validators.required],
        requestSummary: [''],
        success: [false],
        errorMessage: [''],
        triggeredBy: ['', Validators.required],
        recordCount: [''],
        dailyQuotaRemaining: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickEconomyTransactions$ = this.brickEconomyTransactionService.GetBrickEconomyTransactionList();

  constructor(
    private modalService: NgbModal,
    private brickEconomyTransactionService: BrickEconomyTransactionService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickEconomyTransactionData?: BrickEconomyTransactionData) {

    if (brickEconomyTransactionData != null) {

      if (!this.brickEconomyTransactionService.userIsBMCBrickEconomyTransactionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Economy Transactions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickEconomyTransactionSubmitData = this.brickEconomyTransactionService.ConvertToBrickEconomyTransactionSubmitData(brickEconomyTransactionData);
      this.isEditMode = true;
      this.objectGuid = brickEconomyTransactionData.objectGuid;

      this.buildFormValues(brickEconomyTransactionData);

    } else {

      if (!this.brickEconomyTransactionService.userIsBMCBrickEconomyTransactionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Economy Transactions`,
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
        this.brickEconomyTransactionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickEconomyTransactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickEconomyTransactionModal, {
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

    if (this.brickEconomyTransactionService.userIsBMCBrickEconomyTransactionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Economy Transactions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickEconomyTransactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickEconomyTransactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickEconomyTransactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickEconomyTransactionSubmitData: BrickEconomyTransactionSubmitData = {
        id: this.brickEconomyTransactionSubmitData?.id || 0,
        transactionDate: formValue.transactionDate ? dateTimeLocalToIsoUtc(formValue.transactionDate.trim()) : null,
        direction: formValue.direction!.trim(),
        methodName: formValue.methodName!.trim(),
        requestSummary: formValue.requestSummary?.trim() || null,
        success: !!formValue.success,
        errorMessage: formValue.errorMessage?.trim() || null,
        triggeredBy: formValue.triggeredBy!.trim(),
        recordCount: formValue.recordCount ? Number(formValue.recordCount) : null,
        dailyQuotaRemaining: formValue.dailyQuotaRemaining ? Number(formValue.dailyQuotaRemaining) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickEconomyTransaction(brickEconomyTransactionSubmitData);
      } else {
        this.addBrickEconomyTransaction(brickEconomyTransactionSubmitData);
      }
  }

  private addBrickEconomyTransaction(brickEconomyTransactionData: BrickEconomyTransactionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickEconomyTransactionData.active = true;
    brickEconomyTransactionData.deleted = false;
    this.brickEconomyTransactionService.PostBrickEconomyTransaction(brickEconomyTransactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickEconomyTransaction) => {

        this.brickEconomyTransactionService.ClearAllCaches();

        this.brickEconomyTransactionChanged.next([newBrickEconomyTransaction]);

        this.alertService.showMessage("Brick Economy Transaction added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/brickeconomytransaction', newBrickEconomyTransaction.id]);
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
                                   'You do not have permission to save this Brick Economy Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Economy Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Economy Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickEconomyTransaction(brickEconomyTransactionData: BrickEconomyTransactionSubmitData) {
    this.brickEconomyTransactionService.PutBrickEconomyTransaction(brickEconomyTransactionData.id, brickEconomyTransactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickEconomyTransaction) => {

        this.brickEconomyTransactionService.ClearAllCaches();

        this.brickEconomyTransactionChanged.next([updatedBrickEconomyTransaction]);

        this.alertService.showMessage("Brick Economy Transaction updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Economy Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Economy Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Economy Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickEconomyTransactionData: BrickEconomyTransactionData | null) {

    if (brickEconomyTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickEconomyTransactionForm.reset({
        transactionDate: '',
        direction: '',
        methodName: '',
        requestSummary: '',
        success: false,
        errorMessage: '',
        triggeredBy: '',
        recordCount: '',
        dailyQuotaRemaining: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickEconomyTransactionForm.reset({
        transactionDate: isoUtcStringToDateTimeLocal(brickEconomyTransactionData.transactionDate) ?? '',
        direction: brickEconomyTransactionData.direction ?? '',
        methodName: brickEconomyTransactionData.methodName ?? '',
        requestSummary: brickEconomyTransactionData.requestSummary ?? '',
        success: brickEconomyTransactionData.success ?? false,
        errorMessage: brickEconomyTransactionData.errorMessage ?? '',
        triggeredBy: brickEconomyTransactionData.triggeredBy ?? '',
        recordCount: brickEconomyTransactionData.recordCount?.toString() ?? '',
        dailyQuotaRemaining: brickEconomyTransactionData.dailyQuotaRemaining?.toString() ?? '',
        active: brickEconomyTransactionData.active ?? true,
        deleted: brickEconomyTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickEconomyTransactionForm.markAsPristine();
    this.brickEconomyTransactionForm.markAsUntouched();
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


  public userIsBMCBrickEconomyTransactionReader(): boolean {
    return this.brickEconomyTransactionService.userIsBMCBrickEconomyTransactionReader();
  }

  public userIsBMCBrickEconomyTransactionWriter(): boolean {
    return this.brickEconomyTransactionService.userIsBMCBrickEconomyTransactionWriter();
  }
}
