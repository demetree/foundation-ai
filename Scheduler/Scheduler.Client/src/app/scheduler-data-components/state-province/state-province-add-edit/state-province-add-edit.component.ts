/*
   GENERATED FORM FOR THE STATEPROVINCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from StateProvince table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to state-province-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { StateProvinceService, StateProvinceData, StateProvinceSubmitData } from '../../../scheduler-data-services/state-province.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface StateProvinceFormValues {
  countryId: number | bigint,       // For FK link number
  name: string,
  description: string,
  abbreviation: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-state-province-add-edit',
  templateUrl: './state-province-add-edit.component.html',
  styleUrls: ['./state-province-add-edit.component.scss']
})
export class StateProvinceAddEditComponent {
  @ViewChild('stateProvinceModal') stateProvinceModal!: TemplateRef<any>;
  @Output() stateProvinceChanged = new Subject<StateProvinceData[]>();
  @Input() stateProvinceSubmitData: StateProvinceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<StateProvinceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public stateProvinceForm: FormGroup = this.fb.group({
        countryId: [null, Validators.required],
        name: ['', Validators.required],
        description: ['', Validators.required],
        abbreviation: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  countries$ = this.countryService.GetCountryList();

  constructor(
    private modalService: NgbModal,
    private stateProvinceService: StateProvinceService,
    private countryService: CountryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(stateProvinceData?: StateProvinceData) {

    if (stateProvinceData != null) {

      if (!this.stateProvinceService.userIsSchedulerStateProvinceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read State Provinces`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.stateProvinceSubmitData = this.stateProvinceService.ConvertToStateProvinceSubmitData(stateProvinceData);
      this.isEditMode = true;
      this.objectGuid = stateProvinceData.objectGuid;

      this.buildFormValues(stateProvinceData);

    } else {

      if (!this.stateProvinceService.userIsSchedulerStateProvinceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write State Provinces`,
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
        this.stateProvinceForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.stateProvinceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.stateProvinceModal, {
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

    if (this.stateProvinceService.userIsSchedulerStateProvinceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write State Provinces`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.stateProvinceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.stateProvinceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.stateProvinceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const stateProvinceSubmitData: StateProvinceSubmitData = {
        id: this.stateProvinceSubmitData?.id || 0,
        countryId: Number(formValue.countryId),
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        abbreviation: formValue.abbreviation!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateStateProvince(stateProvinceSubmitData);
      } else {
        this.addStateProvince(stateProvinceSubmitData);
      }
  }

  private addStateProvince(stateProvinceData: StateProvinceSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    stateProvinceData.active = true;
    stateProvinceData.deleted = false;
    this.stateProvinceService.PostStateProvince(stateProvinceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newStateProvince) => {

        this.stateProvinceService.ClearAllCaches();

        this.stateProvinceChanged.next([newStateProvince]);

        this.alertService.showMessage("State Province added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/stateprovince', newStateProvince.id]);
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
                                   'You do not have permission to save this State Province.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the State Province.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('State Province could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateStateProvince(stateProvinceData: StateProvinceSubmitData) {
    this.stateProvinceService.PutStateProvince(stateProvinceData.id, stateProvinceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedStateProvince) => {

        this.stateProvinceService.ClearAllCaches();

        this.stateProvinceChanged.next([updatedStateProvince]);

        this.alertService.showMessage("State Province updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this State Province.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the State Province.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('State Province could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(stateProvinceData: StateProvinceData | null) {

    if (stateProvinceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.stateProvinceForm.reset({
        countryId: null,
        name: '',
        description: '',
        abbreviation: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.stateProvinceForm.reset({
        countryId: stateProvinceData.countryId,
        name: stateProvinceData.name ?? '',
        description: stateProvinceData.description ?? '',
        abbreviation: stateProvinceData.abbreviation ?? '',
        sequence: stateProvinceData.sequence?.toString() ?? '',
        active: stateProvinceData.active ?? true,
        deleted: stateProvinceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.stateProvinceForm.markAsPristine();
    this.stateProvinceForm.markAsUntouched();
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


  public userIsSchedulerStateProvinceReader(): boolean {
    return this.stateProvinceService.userIsSchedulerStateProvinceReader();
  }

  public userIsSchedulerStateProvinceWriter(): boolean {
    return this.stateProvinceService.userIsSchedulerStateProvinceWriter();
  }
}
