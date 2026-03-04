/*
   GENERATED FORM FOR THE PAYMENTMETHOD TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PaymentMethod table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to payment-method-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PaymentMethodService, PaymentMethodData, PaymentMethodSubmitData } from '../../../scheduler-data-services/payment-method.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PaymentMethodFormValues {
  name: string,
  description: string,
  isElectronic: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-payment-method-add-edit',
  templateUrl: './payment-method-add-edit.component.html',
  styleUrls: ['./payment-method-add-edit.component.scss']
})
export class PaymentMethodAddEditComponent {
  @ViewChild('paymentMethodModal') paymentMethodModal!: TemplateRef<any>;
  @Output() paymentMethodChanged = new Subject<PaymentMethodData[]>();
  @Input() paymentMethodSubmitData: PaymentMethodSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PaymentMethodFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public paymentMethodForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        isElectronic: [false],
        sequence: [''],
        color: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  paymentMethods$ = this.paymentMethodService.GetPaymentMethodList();

  constructor(
    private modalService: NgbModal,
    private paymentMethodService: PaymentMethodService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(paymentMethodData?: PaymentMethodData) {

    if (paymentMethodData != null) {

      if (!this.paymentMethodService.userIsSchedulerPaymentMethodReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Payment Methods`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.paymentMethodSubmitData = this.paymentMethodService.ConvertToPaymentMethodSubmitData(paymentMethodData);
      this.isEditMode = true;
      this.objectGuid = paymentMethodData.objectGuid;

      this.buildFormValues(paymentMethodData);

    } else {

      if (!this.paymentMethodService.userIsSchedulerPaymentMethodWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Payment Methods`,
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
        this.paymentMethodForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.paymentMethodForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.paymentMethodModal, {
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

    if (this.paymentMethodService.userIsSchedulerPaymentMethodWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Payment Methods`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.paymentMethodForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.paymentMethodForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.paymentMethodForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const paymentMethodSubmitData: PaymentMethodSubmitData = {
        id: this.paymentMethodSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        isElectronic: !!formValue.isElectronic,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePaymentMethod(paymentMethodSubmitData);
      } else {
        this.addPaymentMethod(paymentMethodSubmitData);
      }
  }

  private addPaymentMethod(paymentMethodData: PaymentMethodSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    paymentMethodData.active = true;
    paymentMethodData.deleted = false;
    this.paymentMethodService.PostPaymentMethod(paymentMethodData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPaymentMethod) => {

        this.paymentMethodService.ClearAllCaches();

        this.paymentMethodChanged.next([newPaymentMethod]);

        this.alertService.showMessage("Payment Method added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/paymentmethod', newPaymentMethod.id]);
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
                                   'You do not have permission to save this Payment Method.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Method.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Method could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePaymentMethod(paymentMethodData: PaymentMethodSubmitData) {
    this.paymentMethodService.PutPaymentMethod(paymentMethodData.id, paymentMethodData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPaymentMethod) => {

        this.paymentMethodService.ClearAllCaches();

        this.paymentMethodChanged.next([updatedPaymentMethod]);

        this.alertService.showMessage("Payment Method updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Payment Method.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Method.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Method could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(paymentMethodData: PaymentMethodData | null) {

    if (paymentMethodData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.paymentMethodForm.reset({
        name: '',
        description: '',
        isElectronic: false,
        sequence: '',
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.paymentMethodForm.reset({
        name: paymentMethodData.name ?? '',
        description: paymentMethodData.description ?? '',
        isElectronic: paymentMethodData.isElectronic ?? false,
        sequence: paymentMethodData.sequence?.toString() ?? '',
        color: paymentMethodData.color ?? '',
        active: paymentMethodData.active ?? true,
        deleted: paymentMethodData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.paymentMethodForm.markAsPristine();
    this.paymentMethodForm.markAsUntouched();
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


  public userIsSchedulerPaymentMethodReader(): boolean {
    return this.paymentMethodService.userIsSchedulerPaymentMethodReader();
  }

  public userIsSchedulerPaymentMethodWriter(): boolean {
    return this.paymentMethodService.userIsSchedulerPaymentMethodWriter();
  }
}
