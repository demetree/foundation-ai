/*
   GENERATED FORM FOR THE TAXCODE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TaxCode table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tax-code-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TaxCodeService, TaxCodeData, TaxCodeSubmitData } from '../../../scheduler-data-services/tax-code.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TaxCodeFormValues {
  name: string,
  description: string,
  code: string,
  rate: string,     // Stored as string for form input, converted to number on submit.
  isDefault: boolean,
  isExempt: boolean,
  externalTaxCodeId: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-tax-code-add-edit',
  templateUrl: './tax-code-add-edit.component.html',
  styleUrls: ['./tax-code-add-edit.component.scss']
})
export class TaxCodeAddEditComponent {
  @ViewChild('taxCodeModal') taxCodeModal!: TemplateRef<any>;
  @Output() taxCodeChanged = new Subject<TaxCodeData[]>();
  @Input() taxCodeSubmitData: TaxCodeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TaxCodeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public taxCodeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        code: ['', Validators.required],
        rate: ['', Validators.required],
        isDefault: [false],
        isExempt: [false],
        externalTaxCodeId: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  taxCodes$ = this.taxCodeService.GetTaxCodeList();

  constructor(
    private modalService: NgbModal,
    private taxCodeService: TaxCodeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(taxCodeData?: TaxCodeData) {

    if (taxCodeData != null) {

      if (!this.taxCodeService.userIsSchedulerTaxCodeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Tax Codes`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.taxCodeSubmitData = this.taxCodeService.ConvertToTaxCodeSubmitData(taxCodeData);
      this.isEditMode = true;
      this.objectGuid = taxCodeData.objectGuid;

      this.buildFormValues(taxCodeData);

    } else {

      if (!this.taxCodeService.userIsSchedulerTaxCodeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Tax Codes`,
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
        this.taxCodeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.taxCodeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.taxCodeModal, {
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

    if (this.taxCodeService.userIsSchedulerTaxCodeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Tax Codes`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.taxCodeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.taxCodeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.taxCodeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const taxCodeSubmitData: TaxCodeSubmitData = {
        id: this.taxCodeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        code: formValue.code!.trim(),
        rate: Number(formValue.rate),
        isDefault: !!formValue.isDefault,
        isExempt: !!formValue.isExempt,
        externalTaxCodeId: formValue.externalTaxCodeId?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateTaxCode(taxCodeSubmitData);
      } else {
        this.addTaxCode(taxCodeSubmitData);
      }
  }

  private addTaxCode(taxCodeData: TaxCodeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    taxCodeData.active = true;
    taxCodeData.deleted = false;
    this.taxCodeService.PostTaxCode(taxCodeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTaxCode) => {

        this.taxCodeService.ClearAllCaches();

        this.taxCodeChanged.next([newTaxCode]);

        this.alertService.showMessage("Tax Code added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/taxcode', newTaxCode.id]);
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
                                   'You do not have permission to save this Tax Code.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tax Code.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tax Code could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTaxCode(taxCodeData: TaxCodeSubmitData) {
    this.taxCodeService.PutTaxCode(taxCodeData.id, taxCodeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTaxCode) => {

        this.taxCodeService.ClearAllCaches();

        this.taxCodeChanged.next([updatedTaxCode]);

        this.alertService.showMessage("Tax Code updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Tax Code.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tax Code.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tax Code could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(taxCodeData: TaxCodeData | null) {

    if (taxCodeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.taxCodeForm.reset({
        name: '',
        description: '',
        code: '',
        rate: '',
        isDefault: false,
        isExempt: false,
        externalTaxCodeId: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.taxCodeForm.reset({
        name: taxCodeData.name ?? '',
        description: taxCodeData.description ?? '',
        code: taxCodeData.code ?? '',
        rate: taxCodeData.rate?.toString() ?? '',
        isDefault: taxCodeData.isDefault ?? false,
        isExempt: taxCodeData.isExempt ?? false,
        externalTaxCodeId: taxCodeData.externalTaxCodeId ?? '',
        sequence: taxCodeData.sequence?.toString() ?? '',
        active: taxCodeData.active ?? true,
        deleted: taxCodeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.taxCodeForm.markAsPristine();
    this.taxCodeForm.markAsUntouched();
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


  public userIsSchedulerTaxCodeReader(): boolean {
    return this.taxCodeService.userIsSchedulerTaxCodeReader();
  }

  public userIsSchedulerTaxCodeWriter(): boolean {
    return this.taxCodeService.userIsSchedulerTaxCodeWriter();
  }
}
