/*
   GENERATED FORM FOR THE COUNTRY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Country table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to country-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CountryService, CountryData, CountrySubmitData } from '../../../scheduler-data-services/country.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface CountryFormValues {
  name: string,
  description: string,
  abbreviation: string,
  postalCodeFormat: string | null,
  postalCodeRegEx: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-country-add-edit',
  templateUrl: './country-add-edit.component.html',
  styleUrls: ['./country-add-edit.component.scss']
})
export class CountryAddEditComponent {
  @ViewChild('countryModal') countryModal!: TemplateRef<any>;
  @Output() countryChanged = new Subject<CountryData[]>();
  @Input() countrySubmitData: CountrySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CountryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public countryForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        abbreviation: ['', Validators.required],
        postalCodeFormat: [''],
        postalCodeRegEx: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  countries$ = this.countryService.GetCountryList();

  constructor(
    private modalService: NgbModal,
    private countryService: CountryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(countryData?: CountryData) {

    if (countryData != null) {

      if (!this.countryService.userIsSchedulerCountryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Countries`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.countrySubmitData = this.countryService.ConvertToCountrySubmitData(countryData);
      this.isEditMode = true;
      this.objectGuid = countryData.objectGuid;

      this.buildFormValues(countryData);

    } else {

      if (!this.countryService.userIsSchedulerCountryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Countries`,
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
        this.countryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.countryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.countryModal, {
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

    if (this.countryService.userIsSchedulerCountryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Countries`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.countryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.countryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.countryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const countrySubmitData: CountrySubmitData = {
        id: this.countrySubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        abbreviation: formValue.abbreviation!.trim(),
        postalCodeFormat: formValue.postalCodeFormat?.trim() || null,
        postalCodeRegEx: formValue.postalCodeRegEx?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateCountry(countrySubmitData);
      } else {
        this.addCountry(countrySubmitData);
      }
  }

  private addCountry(countryData: CountrySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    countryData.active = true;
    countryData.deleted = false;
    this.countryService.PostCountry(countryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCountry) => {

        this.countryService.ClearAllCaches();

        this.countryChanged.next([newCountry]);

        this.alertService.showMessage("Country added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/country', newCountry.id]);
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
                                   'You do not have permission to save this Country.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Country.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Country could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCountry(countryData: CountrySubmitData) {
    this.countryService.PutCountry(countryData.id, countryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCountry) => {

        this.countryService.ClearAllCaches();

        this.countryChanged.next([updatedCountry]);

        this.alertService.showMessage("Country updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Country.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Country.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Country could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(countryData: CountryData | null) {

    if (countryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.countryForm.reset({
        name: '',
        description: '',
        abbreviation: '',
        postalCodeFormat: '',
        postalCodeRegEx: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.countryForm.reset({
        name: countryData.name ?? '',
        description: countryData.description ?? '',
        abbreviation: countryData.abbreviation ?? '',
        postalCodeFormat: countryData.postalCodeFormat ?? '',
        postalCodeRegEx: countryData.postalCodeRegEx ?? '',
        sequence: countryData.sequence?.toString() ?? '',
        active: countryData.active ?? true,
        deleted: countryData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.countryForm.markAsPristine();
    this.countryForm.markAsUntouched();
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


  public userIsSchedulerCountryReader(): boolean {
    return this.countryService.userIsSchedulerCountryReader();
  }

  public userIsSchedulerCountryWriter(): boolean {
    return this.countryService.userIsSchedulerCountryWriter();
  }
}
