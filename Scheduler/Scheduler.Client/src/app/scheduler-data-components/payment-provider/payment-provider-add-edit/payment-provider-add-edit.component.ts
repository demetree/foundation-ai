/*
   GENERATED FORM FOR THE PAYMENTPROVIDER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PaymentProvider table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to payment-provider-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PaymentProviderService, PaymentProviderData, PaymentProviderSubmitData } from '../../../scheduler-data-services/payment-provider.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PaymentProviderFormValues {
  name: string,
  description: string,
  providerType: string,
  isActive: boolean,
  apiKeyEncrypted: string | null,
  merchantId: string | null,
  webhookSecret: string | null,
  processingFeePercent: string | null,     // Stored as string for form input, converted to number on submit.
  processingFeeFixed: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-payment-provider-add-edit',
  templateUrl: './payment-provider-add-edit.component.html',
  styleUrls: ['./payment-provider-add-edit.component.scss']
})
export class PaymentProviderAddEditComponent {
  @ViewChild('paymentProviderModal') paymentProviderModal!: TemplateRef<any>;
  @Output() paymentProviderChanged = new Subject<PaymentProviderData[]>();
  @Input() paymentProviderSubmitData: PaymentProviderSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PaymentProviderFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public paymentProviderForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        providerType: ['', Validators.required],
        isActive: [false],
        apiKeyEncrypted: [''],
        merchantId: [''],
        webhookSecret: [''],
        processingFeePercent: [''],
        processingFeeFixed: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  paymentProviders$ = this.paymentProviderService.GetPaymentProviderList();

  constructor(
    private modalService: NgbModal,
    private paymentProviderService: PaymentProviderService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(paymentProviderData?: PaymentProviderData) {

    if (paymentProviderData != null) {

      if (!this.paymentProviderService.userIsSchedulerPaymentProviderReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Payment Providers`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.paymentProviderSubmitData = this.paymentProviderService.ConvertToPaymentProviderSubmitData(paymentProviderData);
      this.isEditMode = true;
      this.objectGuid = paymentProviderData.objectGuid;

      this.buildFormValues(paymentProviderData);

    } else {

      if (!this.paymentProviderService.userIsSchedulerPaymentProviderWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Payment Providers`,
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
        this.paymentProviderForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.paymentProviderForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.paymentProviderModal, {
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

    if (this.paymentProviderService.userIsSchedulerPaymentProviderWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Payment Providers`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.paymentProviderForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.paymentProviderForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.paymentProviderForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const paymentProviderSubmitData: PaymentProviderSubmitData = {
        id: this.paymentProviderSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        providerType: formValue.providerType!.trim(),
        isActive: !!formValue.isActive,
        apiKeyEncrypted: formValue.apiKeyEncrypted?.trim() || null,
        merchantId: formValue.merchantId?.trim() || null,
        webhookSecret: formValue.webhookSecret?.trim() || null,
        processingFeePercent: formValue.processingFeePercent ? Number(formValue.processingFeePercent) : null,
        processingFeeFixed: formValue.processingFeeFixed ? Number(formValue.processingFeeFixed) : null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.paymentProviderSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePaymentProvider(paymentProviderSubmitData);
      } else {
        this.addPaymentProvider(paymentProviderSubmitData);
      }
  }

  private addPaymentProvider(paymentProviderData: PaymentProviderSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    paymentProviderData.versionNumber = 0;
    paymentProviderData.active = true;
    paymentProviderData.deleted = false;
    this.paymentProviderService.PostPaymentProvider(paymentProviderData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPaymentProvider) => {

        this.paymentProviderService.ClearAllCaches();

        this.paymentProviderChanged.next([newPaymentProvider]);

        this.alertService.showMessage("Payment Provider added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/paymentprovider', newPaymentProvider.id]);
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
                                   'You do not have permission to save this Payment Provider.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Provider.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Provider could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePaymentProvider(paymentProviderData: PaymentProviderSubmitData) {
    this.paymentProviderService.PutPaymentProvider(paymentProviderData.id, paymentProviderData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPaymentProvider) => {

        this.paymentProviderService.ClearAllCaches();

        this.paymentProviderChanged.next([updatedPaymentProvider]);

        this.alertService.showMessage("Payment Provider updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Payment Provider.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Provider.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Provider could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(paymentProviderData: PaymentProviderData | null) {

    if (paymentProviderData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.paymentProviderForm.reset({
        name: '',
        description: '',
        providerType: '',
        isActive: false,
        apiKeyEncrypted: '',
        merchantId: '',
        webhookSecret: '',
        processingFeePercent: '',
        processingFeeFixed: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.paymentProviderForm.reset({
        name: paymentProviderData.name ?? '',
        description: paymentProviderData.description ?? '',
        providerType: paymentProviderData.providerType ?? '',
        isActive: paymentProviderData.isActive ?? false,
        apiKeyEncrypted: paymentProviderData.apiKeyEncrypted ?? '',
        merchantId: paymentProviderData.merchantId ?? '',
        webhookSecret: paymentProviderData.webhookSecret ?? '',
        processingFeePercent: paymentProviderData.processingFeePercent?.toString() ?? '',
        processingFeeFixed: paymentProviderData.processingFeeFixed?.toString() ?? '',
        notes: paymentProviderData.notes ?? '',
        versionNumber: paymentProviderData.versionNumber?.toString() ?? '',
        active: paymentProviderData.active ?? true,
        deleted: paymentProviderData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.paymentProviderForm.markAsPristine();
    this.paymentProviderForm.markAsUntouched();
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


  public userIsSchedulerPaymentProviderReader(): boolean {
    return this.paymentProviderService.userIsSchedulerPaymentProviderReader();
  }

  public userIsSchedulerPaymentProviderWriter(): boolean {
    return this.paymentProviderService.userIsSchedulerPaymentProviderWriter();
  }
}
