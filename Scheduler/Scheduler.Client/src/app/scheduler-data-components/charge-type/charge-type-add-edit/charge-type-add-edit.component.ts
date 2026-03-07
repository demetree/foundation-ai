/*
   GENERATED FORM FOR THE CHARGETYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ChargeType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to charge-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ChargeTypeService, ChargeTypeData, ChargeTypeSubmitData } from '../../../scheduler-data-services/charge-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { RateTypeService } from '../../../scheduler-data-services/rate-type.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { TaxCodeService } from '../../../scheduler-data-services/tax-code.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ChargeTypeFormValues {
  name: string,
  description: string,
  externalId: string | null,
  isRevenue: boolean,
  isTaxable: boolean | null,
  defaultAmount: string | null,     // Stored as string for form input, converted to number on submit.
  defaultDescription: string | null,
  rateTypeId: number | bigint | null,       // For FK link number
  currencyId: number | bigint,       // For FK link number
  financialCategoryId: number | bigint | null,       // For FK link number
  taxCodeId: number | bigint | null,       // For FK link number
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-charge-type-add-edit',
  templateUrl: './charge-type-add-edit.component.html',
  styleUrls: ['./charge-type-add-edit.component.scss']
})
export class ChargeTypeAddEditComponent {
  @ViewChild('chargeTypeModal') chargeTypeModal!: TemplateRef<any>;
  @Output() chargeTypeChanged = new Subject<ChargeTypeData[]>();
  @Input() chargeTypeSubmitData: ChargeTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ChargeTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public chargeTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        externalId: [''],
        isRevenue: [false],
        isTaxable: [false],
        defaultAmount: [''],
        defaultDescription: [''],
        rateTypeId: [null],
        currencyId: [null, Validators.required],
        financialCategoryId: [null],
        taxCodeId: [null],
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

  chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  rateTypes$ = this.rateTypeService.GetRateTypeList();
  currencies$ = this.currencyService.GetCurrencyList();
  financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();
  taxCodes$ = this.taxCodeService.GetTaxCodeList();

  constructor(
    private modalService: NgbModal,
    private chargeTypeService: ChargeTypeService,
    private rateTypeService: RateTypeService,
    private currencyService: CurrencyService,
    private financialCategoryService: FinancialCategoryService,
    private taxCodeService: TaxCodeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(chargeTypeData?: ChargeTypeData) {

    if (chargeTypeData != null) {

      if (!this.chargeTypeService.userIsSchedulerChargeTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Charge Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.chargeTypeSubmitData = this.chargeTypeService.ConvertToChargeTypeSubmitData(chargeTypeData);
      this.isEditMode = true;
      this.objectGuid = chargeTypeData.objectGuid;

      this.buildFormValues(chargeTypeData);

    } else {

      if (!this.chargeTypeService.userIsSchedulerChargeTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Charge Types`,
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
        this.chargeTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.chargeTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.chargeTypeModal, {
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

    if (this.chargeTypeService.userIsSchedulerChargeTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Charge Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.chargeTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.chargeTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.chargeTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const chargeTypeSubmitData: ChargeTypeSubmitData = {
        id: this.chargeTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        externalId: formValue.externalId?.trim() || null,
        isRevenue: !!formValue.isRevenue,
        isTaxable: formValue.isTaxable == true ? true : formValue.isTaxable == false ? false : null,
        defaultAmount: formValue.defaultAmount ? Number(formValue.defaultAmount) : null,
        defaultDescription: formValue.defaultDescription?.trim() || null,
        rateTypeId: formValue.rateTypeId ? Number(formValue.rateTypeId) : null,
        currencyId: Number(formValue.currencyId),
        financialCategoryId: formValue.financialCategoryId ? Number(formValue.financialCategoryId) : null,
        taxCodeId: formValue.taxCodeId ? Number(formValue.taxCodeId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.chargeTypeSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateChargeType(chargeTypeSubmitData);
      } else {
        this.addChargeType(chargeTypeSubmitData);
      }
  }

  private addChargeType(chargeTypeData: ChargeTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    chargeTypeData.versionNumber = 0;
    chargeTypeData.active = true;
    chargeTypeData.deleted = false;
    this.chargeTypeService.PostChargeType(chargeTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newChargeType) => {

        this.chargeTypeService.ClearAllCaches();

        this.chargeTypeChanged.next([newChargeType]);

        this.alertService.showMessage("Charge Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/chargetype', newChargeType.id]);
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
                                   'You do not have permission to save this Charge Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Charge Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Charge Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateChargeType(chargeTypeData: ChargeTypeSubmitData) {
    this.chargeTypeService.PutChargeType(chargeTypeData.id, chargeTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedChargeType) => {

        this.chargeTypeService.ClearAllCaches();

        this.chargeTypeChanged.next([updatedChargeType]);

        this.alertService.showMessage("Charge Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Charge Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Charge Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Charge Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(chargeTypeData: ChargeTypeData | null) {

    if (chargeTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.chargeTypeForm.reset({
        name: '',
        description: '',
        externalId: '',
        isRevenue: false,
        isTaxable: false,
        defaultAmount: '',
        defaultDescription: '',
        rateTypeId: null,
        currencyId: null,
        financialCategoryId: null,
        taxCodeId: null,
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
        this.chargeTypeForm.reset({
        name: chargeTypeData.name ?? '',
        description: chargeTypeData.description ?? '',
        externalId: chargeTypeData.externalId ?? '',
        isRevenue: chargeTypeData.isRevenue ?? false,
        isTaxable: chargeTypeData.isTaxable ?? false,
        defaultAmount: chargeTypeData.defaultAmount?.toString() ?? '',
        defaultDescription: chargeTypeData.defaultDescription ?? '',
        rateTypeId: chargeTypeData.rateTypeId,
        currencyId: chargeTypeData.currencyId,
        financialCategoryId: chargeTypeData.financialCategoryId,
        taxCodeId: chargeTypeData.taxCodeId,
        sequence: chargeTypeData.sequence?.toString() ?? '',
        color: chargeTypeData.color ?? '',
        versionNumber: chargeTypeData.versionNumber?.toString() ?? '',
        active: chargeTypeData.active ?? true,
        deleted: chargeTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.chargeTypeForm.markAsPristine();
    this.chargeTypeForm.markAsUntouched();
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


  public userIsSchedulerChargeTypeReader(): boolean {
    return this.chargeTypeService.userIsSchedulerChargeTypeReader();
  }

  public userIsSchedulerChargeTypeWriter(): boolean {
    return this.chargeTypeService.userIsSchedulerChargeTypeWriter();
  }
}
