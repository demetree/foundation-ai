/*
   GENERATED FORM FOR THE PAYMENTTYPECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PaymentTypeChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to payment-type-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PaymentTypeChangeHistoryService, PaymentTypeChangeHistoryData, PaymentTypeChangeHistorySubmitData } from '../../../scheduler-data-services/payment-type-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PaymentTypeService } from '../../../scheduler-data-services/payment-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PaymentTypeChangeHistoryFormValues {
  paymentTypeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-payment-type-change-history-add-edit',
  templateUrl: './payment-type-change-history-add-edit.component.html',
  styleUrls: ['./payment-type-change-history-add-edit.component.scss']
})
export class PaymentTypeChangeHistoryAddEditComponent {
  @ViewChild('paymentTypeChangeHistoryModal') paymentTypeChangeHistoryModal!: TemplateRef<any>;
  @Output() paymentTypeChangeHistoryChanged = new Subject<PaymentTypeChangeHistoryData[]>();
  @Input() paymentTypeChangeHistorySubmitData: PaymentTypeChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PaymentTypeChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public paymentTypeChangeHistoryForm: FormGroup = this.fb.group({
        paymentTypeId: [null, Validators.required],
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

  paymentTypeChangeHistories$ = this.paymentTypeChangeHistoryService.GetPaymentTypeChangeHistoryList();
  paymentTypes$ = this.paymentTypeService.GetPaymentTypeList();

  constructor(
    private modalService: NgbModal,
    private paymentTypeChangeHistoryService: PaymentTypeChangeHistoryService,
    private paymentTypeService: PaymentTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(paymentTypeChangeHistoryData?: PaymentTypeChangeHistoryData) {

    if (paymentTypeChangeHistoryData != null) {

      if (!this.paymentTypeChangeHistoryService.userIsSchedulerPaymentTypeChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Payment Type Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.paymentTypeChangeHistorySubmitData = this.paymentTypeChangeHistoryService.ConvertToPaymentTypeChangeHistorySubmitData(paymentTypeChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(paymentTypeChangeHistoryData);

    } else {

      if (!this.paymentTypeChangeHistoryService.userIsSchedulerPaymentTypeChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Payment Type Change Histories`,
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
        this.paymentTypeChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.paymentTypeChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.paymentTypeChangeHistoryModal, {
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

    if (this.paymentTypeChangeHistoryService.userIsSchedulerPaymentTypeChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Payment Type Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.paymentTypeChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.paymentTypeChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.paymentTypeChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const paymentTypeChangeHistorySubmitData: PaymentTypeChangeHistorySubmitData = {
        id: this.paymentTypeChangeHistorySubmitData?.id || 0,
        paymentTypeId: Number(formValue.paymentTypeId),
        versionNumber: this.paymentTypeChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updatePaymentTypeChangeHistory(paymentTypeChangeHistorySubmitData);
      } else {
        this.addPaymentTypeChangeHistory(paymentTypeChangeHistorySubmitData);
      }
  }

  private addPaymentTypeChangeHistory(paymentTypeChangeHistoryData: PaymentTypeChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    paymentTypeChangeHistoryData.versionNumber = 0;
    this.paymentTypeChangeHistoryService.PostPaymentTypeChangeHistory(paymentTypeChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPaymentTypeChangeHistory) => {

        this.paymentTypeChangeHistoryService.ClearAllCaches();

        this.paymentTypeChangeHistoryChanged.next([newPaymentTypeChangeHistory]);

        this.alertService.showMessage("Payment Type Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/paymenttypechangehistory', newPaymentTypeChangeHistory.id]);
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
                                   'You do not have permission to save this Payment Type Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Type Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Type Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePaymentTypeChangeHistory(paymentTypeChangeHistoryData: PaymentTypeChangeHistorySubmitData) {
    this.paymentTypeChangeHistoryService.PutPaymentTypeChangeHistory(paymentTypeChangeHistoryData.id, paymentTypeChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPaymentTypeChangeHistory) => {

        this.paymentTypeChangeHistoryService.ClearAllCaches();

        this.paymentTypeChangeHistoryChanged.next([updatedPaymentTypeChangeHistory]);

        this.alertService.showMessage("Payment Type Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Payment Type Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Type Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Type Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(paymentTypeChangeHistoryData: PaymentTypeChangeHistoryData | null) {

    if (paymentTypeChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.paymentTypeChangeHistoryForm.reset({
        paymentTypeId: null,
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
        this.paymentTypeChangeHistoryForm.reset({
        paymentTypeId: paymentTypeChangeHistoryData.paymentTypeId,
        versionNumber: paymentTypeChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(paymentTypeChangeHistoryData.timeStamp) ?? '',
        userId: paymentTypeChangeHistoryData.userId?.toString() ?? '',
        data: paymentTypeChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.paymentTypeChangeHistoryForm.markAsPristine();
    this.paymentTypeChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerPaymentTypeChangeHistoryReader(): boolean {
    return this.paymentTypeChangeHistoryService.userIsSchedulerPaymentTypeChangeHistoryReader();
  }

  public userIsSchedulerPaymentTypeChangeHistoryWriter(): boolean {
    return this.paymentTypeChangeHistoryService.userIsSchedulerPaymentTypeChangeHistoryWriter();
  }
}
