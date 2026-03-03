/*
   GENERATED FORM FOR THE BRICKSETTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickSetTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-set-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickSetTransactionService, BrickSetTransactionData, BrickSetTransactionSubmitData } from '../../../bmc-data-services/brick-set-transaction.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickSetTransactionFormValues {
  transactionDate: string | null,
  direction: string,
  methodName: string,
  requestSummary: string | null,
  success: boolean,
  errorMessage: string | null,
  triggeredBy: string,
  recordCount: string | null,     // Stored as string for form input, converted to number on submit.
  apiCallsRemaining: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-set-transaction-add-edit',
  templateUrl: './brick-set-transaction-add-edit.component.html',
  styleUrls: ['./brick-set-transaction-add-edit.component.scss']
})
export class BrickSetTransactionAddEditComponent {
  @ViewChild('brickSetTransactionModal') brickSetTransactionModal!: TemplateRef<any>;
  @Output() brickSetTransactionChanged = new Subject<BrickSetTransactionData[]>();
  @Input() brickSetTransactionSubmitData: BrickSetTransactionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickSetTransactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickSetTransactionForm: FormGroup = this.fb.group({
        transactionDate: [''],
        direction: ['', Validators.required],
        methodName: ['', Validators.required],
        requestSummary: [''],
        success: [false],
        errorMessage: [''],
        triggeredBy: ['', Validators.required],
        recordCount: [''],
        apiCallsRemaining: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickSetTransactions$ = this.brickSetTransactionService.GetBrickSetTransactionList();

  constructor(
    private modalService: NgbModal,
    private brickSetTransactionService: BrickSetTransactionService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickSetTransactionData?: BrickSetTransactionData) {

    if (brickSetTransactionData != null) {

      if (!this.brickSetTransactionService.userIsBMCBrickSetTransactionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Set Transactions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickSetTransactionSubmitData = this.brickSetTransactionService.ConvertToBrickSetTransactionSubmitData(brickSetTransactionData);
      this.isEditMode = true;
      this.objectGuid = brickSetTransactionData.objectGuid;

      this.buildFormValues(brickSetTransactionData);

    } else {

      if (!this.brickSetTransactionService.userIsBMCBrickSetTransactionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Set Transactions`,
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
        this.brickSetTransactionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickSetTransactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickSetTransactionModal, {
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

    if (this.brickSetTransactionService.userIsBMCBrickSetTransactionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Set Transactions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickSetTransactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickSetTransactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickSetTransactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickSetTransactionSubmitData: BrickSetTransactionSubmitData = {
        id: this.brickSetTransactionSubmitData?.id || 0,
        transactionDate: formValue.transactionDate ? dateTimeLocalToIsoUtc(formValue.transactionDate.trim()) : null,
        direction: formValue.direction!.trim(),
        methodName: formValue.methodName!.trim(),
        requestSummary: formValue.requestSummary?.trim() || null,
        success: !!formValue.success,
        errorMessage: formValue.errorMessage?.trim() || null,
        triggeredBy: formValue.triggeredBy!.trim(),
        recordCount: formValue.recordCount ? Number(formValue.recordCount) : null,
        apiCallsRemaining: formValue.apiCallsRemaining ? Number(formValue.apiCallsRemaining) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickSetTransaction(brickSetTransactionSubmitData);
      } else {
        this.addBrickSetTransaction(brickSetTransactionSubmitData);
      }
  }

  private addBrickSetTransaction(brickSetTransactionData: BrickSetTransactionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickSetTransactionData.active = true;
    brickSetTransactionData.deleted = false;
    this.brickSetTransactionService.PostBrickSetTransaction(brickSetTransactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickSetTransaction) => {

        this.brickSetTransactionService.ClearAllCaches();

        this.brickSetTransactionChanged.next([newBrickSetTransaction]);

        this.alertService.showMessage("Brick Set Transaction added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/bricksettransaction', newBrickSetTransaction.id]);
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
                                   'You do not have permission to save this Brick Set Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Set Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Set Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickSetTransaction(brickSetTransactionData: BrickSetTransactionSubmitData) {
    this.brickSetTransactionService.PutBrickSetTransaction(brickSetTransactionData.id, brickSetTransactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickSetTransaction) => {

        this.brickSetTransactionService.ClearAllCaches();

        this.brickSetTransactionChanged.next([updatedBrickSetTransaction]);

        this.alertService.showMessage("Brick Set Transaction updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Set Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Set Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Set Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickSetTransactionData: BrickSetTransactionData | null) {

    if (brickSetTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickSetTransactionForm.reset({
        transactionDate: '',
        direction: '',
        methodName: '',
        requestSummary: '',
        success: false,
        errorMessage: '',
        triggeredBy: '',
        recordCount: '',
        apiCallsRemaining: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickSetTransactionForm.reset({
        transactionDate: isoUtcStringToDateTimeLocal(brickSetTransactionData.transactionDate) ?? '',
        direction: brickSetTransactionData.direction ?? '',
        methodName: brickSetTransactionData.methodName ?? '',
        requestSummary: brickSetTransactionData.requestSummary ?? '',
        success: brickSetTransactionData.success ?? false,
        errorMessage: brickSetTransactionData.errorMessage ?? '',
        triggeredBy: brickSetTransactionData.triggeredBy ?? '',
        recordCount: brickSetTransactionData.recordCount?.toString() ?? '',
        apiCallsRemaining: brickSetTransactionData.apiCallsRemaining?.toString() ?? '',
        active: brickSetTransactionData.active ?? true,
        deleted: brickSetTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickSetTransactionForm.markAsPristine();
    this.brickSetTransactionForm.markAsUntouched();
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


  public userIsBMCBrickSetTransactionReader(): boolean {
    return this.brickSetTransactionService.userIsBMCBrickSetTransactionReader();
  }

  public userIsBMCBrickSetTransactionWriter(): boolean {
    return this.brickSetTransactionService.userIsBMCBrickSetTransactionWriter();
  }
}
