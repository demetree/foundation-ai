/*
   GENERATED FORM FOR THE CURRENCY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Currency table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to currency-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CurrencyService, CurrencyData, CurrencySubmitData } from '../../../scheduler-data-services/currency.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface CurrencyFormValues {
  name: string,
  description: string,
  code: string,
  color: string | null,
  isDefault: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-currency-add-edit',
  templateUrl: './currency-add-edit.component.html',
  styleUrls: ['./currency-add-edit.component.scss']
})
export class CurrencyAddEditComponent {
  @ViewChild('currencyModal') currencyModal!: TemplateRef<any>;
  @Output() currencyChanged = new Subject<CurrencyData[]>();
  @Input() currencySubmitData: CurrencySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CurrencyFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public currencyForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        code: ['', Validators.required],
        color: [''],
        isDefault: [false],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  currencies$ = this.currencyService.GetCurrencyList();

  constructor(
    private modalService: NgbModal,
    private currencyService: CurrencyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(currencyData?: CurrencyData) {

    if (currencyData != null) {

      if (!this.currencyService.userIsSchedulerCurrencyReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Currencies`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.currencySubmitData = this.currencyService.ConvertToCurrencySubmitData(currencyData);
      this.isEditMode = true;
      this.objectGuid = currencyData.objectGuid;

      this.buildFormValues(currencyData);

    } else {

      if (!this.currencyService.userIsSchedulerCurrencyWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Currencies`,
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
        this.currencyForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.currencyForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.currencyModal, {
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

    if (this.currencyService.userIsSchedulerCurrencyWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Currencies`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.currencyForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.currencyForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.currencyForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const currencySubmitData: CurrencySubmitData = {
        id: this.currencySubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        code: formValue.code!.trim(),
        color: formValue.color?.trim() || null,
        isDefault: !!formValue.isDefault,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateCurrency(currencySubmitData);
      } else {
        this.addCurrency(currencySubmitData);
      }
  }

  private addCurrency(currencyData: CurrencySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    currencyData.active = true;
    currencyData.deleted = false;
    this.currencyService.PostCurrency(currencyData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCurrency) => {

        this.currencyService.ClearAllCaches();

        this.currencyChanged.next([newCurrency]);

        this.alertService.showMessage("Currency added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/currency', newCurrency.id]);
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
                                   'You do not have permission to save this Currency.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Currency.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Currency could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCurrency(currencyData: CurrencySubmitData) {
    this.currencyService.PutCurrency(currencyData.id, currencyData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCurrency) => {

        this.currencyService.ClearAllCaches();

        this.currencyChanged.next([updatedCurrency]);

        this.alertService.showMessage("Currency updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Currency.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Currency.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Currency could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(currencyData: CurrencyData | null) {

    if (currencyData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.currencyForm.reset({
        name: '',
        description: '',
        code: '',
        color: '',
        isDefault: false,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.currencyForm.reset({
        name: currencyData.name ?? '',
        description: currencyData.description ?? '',
        code: currencyData.code ?? '',
        color: currencyData.color ?? '',
        isDefault: currencyData.isDefault ?? false,
        sequence: currencyData.sequence?.toString() ?? '',
        active: currencyData.active ?? true,
        deleted: currencyData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.currencyForm.markAsPristine();
    this.currencyForm.markAsUntouched();
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


  public userIsSchedulerCurrencyReader(): boolean {
    return this.currencyService.userIsSchedulerCurrencyReader();
  }

  public userIsSchedulerCurrencyWriter(): boolean {
    return this.currencyService.userIsSchedulerCurrencyWriter();
  }
}
