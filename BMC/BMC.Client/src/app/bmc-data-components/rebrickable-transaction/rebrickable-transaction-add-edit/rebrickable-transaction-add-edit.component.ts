/*
   GENERATED FORM FOR THE REBRICKABLETRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RebrickableTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to rebrickable-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RebrickableTransactionService, RebrickableTransactionData, RebrickableTransactionSubmitData } from '../../../bmc-data-services/rebrickable-transaction.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface RebrickableTransactionFormValues {
  transactionDate: string | null,
  direction: string,
  httpMethod: string,
  endpoint: string,
  requestSummary: string | null,
  responseStatusCode: string | null,     // Stored as string for form input, converted to number on submit.
  responseBody: string | null,
  success: boolean,
  errorMessage: string | null,
  triggeredBy: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-rebrickable-transaction-add-edit',
  templateUrl: './rebrickable-transaction-add-edit.component.html',
  styleUrls: ['./rebrickable-transaction-add-edit.component.scss']
})
export class RebrickableTransactionAddEditComponent {
  @ViewChild('rebrickableTransactionModal') rebrickableTransactionModal!: TemplateRef<any>;
  @Output() rebrickableTransactionChanged = new Subject<RebrickableTransactionData[]>();
  @Input() rebrickableTransactionSubmitData: RebrickableTransactionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RebrickableTransactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public rebrickableTransactionForm: FormGroup = this.fb.group({
        transactionDate: [''],
        direction: ['', Validators.required],
        httpMethod: ['', Validators.required],
        endpoint: ['', Validators.required],
        requestSummary: [''],
        responseStatusCode: [''],
        responseBody: [''],
        success: [false],
        errorMessage: [''],
        triggeredBy: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  rebrickableTransactions$ = this.rebrickableTransactionService.GetRebrickableTransactionList();

  constructor(
    private modalService: NgbModal,
    private rebrickableTransactionService: RebrickableTransactionService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(rebrickableTransactionData?: RebrickableTransactionData) {

    if (rebrickableTransactionData != null) {

      if (!this.rebrickableTransactionService.userIsBMCRebrickableTransactionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Rebrickable Transactions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.rebrickableTransactionSubmitData = this.rebrickableTransactionService.ConvertToRebrickableTransactionSubmitData(rebrickableTransactionData);
      this.isEditMode = true;
      this.objectGuid = rebrickableTransactionData.objectGuid;

      this.buildFormValues(rebrickableTransactionData);

    } else {

      if (!this.rebrickableTransactionService.userIsBMCRebrickableTransactionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Rebrickable Transactions`,
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
        this.rebrickableTransactionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.rebrickableTransactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.rebrickableTransactionModal, {
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

    if (this.rebrickableTransactionService.userIsBMCRebrickableTransactionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Rebrickable Transactions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.rebrickableTransactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.rebrickableTransactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.rebrickableTransactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const rebrickableTransactionSubmitData: RebrickableTransactionSubmitData = {
        id: this.rebrickableTransactionSubmitData?.id || 0,
        transactionDate: formValue.transactionDate ? dateTimeLocalToIsoUtc(formValue.transactionDate.trim()) : null,
        direction: formValue.direction!.trim(),
        httpMethod: formValue.httpMethod!.trim(),
        endpoint: formValue.endpoint!.trim(),
        requestSummary: formValue.requestSummary?.trim() || null,
        responseStatusCode: formValue.responseStatusCode ? Number(formValue.responseStatusCode) : null,
        responseBody: formValue.responseBody?.trim() || null,
        success: !!formValue.success,
        errorMessage: formValue.errorMessage?.trim() || null,
        triggeredBy: formValue.triggeredBy!.trim(),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateRebrickableTransaction(rebrickableTransactionSubmitData);
      } else {
        this.addRebrickableTransaction(rebrickableTransactionSubmitData);
      }
  }

  private addRebrickableTransaction(rebrickableTransactionData: RebrickableTransactionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    rebrickableTransactionData.active = true;
    rebrickableTransactionData.deleted = false;
    this.rebrickableTransactionService.PostRebrickableTransaction(rebrickableTransactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newRebrickableTransaction) => {

        this.rebrickableTransactionService.ClearAllCaches();

        this.rebrickableTransactionChanged.next([newRebrickableTransaction]);

        this.alertService.showMessage("Rebrickable Transaction added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/rebrickabletransaction', newRebrickableTransaction.id]);
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
                                   'You do not have permission to save this Rebrickable Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rebrickable Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rebrickable Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateRebrickableTransaction(rebrickableTransactionData: RebrickableTransactionSubmitData) {
    this.rebrickableTransactionService.PutRebrickableTransaction(rebrickableTransactionData.id, rebrickableTransactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedRebrickableTransaction) => {

        this.rebrickableTransactionService.ClearAllCaches();

        this.rebrickableTransactionChanged.next([updatedRebrickableTransaction]);

        this.alertService.showMessage("Rebrickable Transaction updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Rebrickable Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rebrickable Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rebrickable Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(rebrickableTransactionData: RebrickableTransactionData | null) {

    if (rebrickableTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.rebrickableTransactionForm.reset({
        transactionDate: '',
        direction: '',
        httpMethod: '',
        endpoint: '',
        requestSummary: '',
        responseStatusCode: '',
        responseBody: '',
        success: false,
        errorMessage: '',
        triggeredBy: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.rebrickableTransactionForm.reset({
        transactionDate: isoUtcStringToDateTimeLocal(rebrickableTransactionData.transactionDate) ?? '',
        direction: rebrickableTransactionData.direction ?? '',
        httpMethod: rebrickableTransactionData.httpMethod ?? '',
        endpoint: rebrickableTransactionData.endpoint ?? '',
        requestSummary: rebrickableTransactionData.requestSummary ?? '',
        responseStatusCode: rebrickableTransactionData.responseStatusCode?.toString() ?? '',
        responseBody: rebrickableTransactionData.responseBody ?? '',
        success: rebrickableTransactionData.success ?? false,
        errorMessage: rebrickableTransactionData.errorMessage ?? '',
        triggeredBy: rebrickableTransactionData.triggeredBy ?? '',
        active: rebrickableTransactionData.active ?? true,
        deleted: rebrickableTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.rebrickableTransactionForm.markAsPristine();
    this.rebrickableTransactionForm.markAsUntouched();
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


  public userIsBMCRebrickableTransactionReader(): boolean {
    return this.rebrickableTransactionService.userIsBMCRebrickableTransactionReader();
  }

  public userIsBMCRebrickableTransactionWriter(): boolean {
    return this.rebrickableTransactionService.userIsBMCRebrickableTransactionWriter();
  }
}
