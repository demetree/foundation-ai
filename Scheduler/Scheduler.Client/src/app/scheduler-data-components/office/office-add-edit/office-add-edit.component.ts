/*
   GENERATED FORM FOR THE OFFICE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Office table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to office-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OfficeService, OfficeData, OfficeSubmitData } from '../../../scheduler-data-services/office.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeTypeService } from '../../../scheduler-data-services/office-type.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { StateProvinceService } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface OfficeFormValues {
  name: string,
  description: string | null,
  officeTypeId: number | bigint,       // For FK link number
  timeZoneId: number | bigint,       // For FK link number
  currencyId: number | bigint,       // For FK link number
  addressLine1: string,
  addressLine2: string | null,
  city: string,
  postalCode: string | null,
  stateProvinceId: number | bigint,       // For FK link number
  countryId: number | bigint,       // For FK link number
  phone: string | null,
  email: string | null,
  latitude: string | null,     // Stored as string for form input, converted to number on submit.
  longitude: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  externalId: string | null,
  color: string | null,
  attributes: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-office-add-edit',
  templateUrl: './office-add-edit.component.html',
  styleUrls: ['./office-add-edit.component.scss']
})
export class OfficeAddEditComponent {
  @ViewChild('officeModal') officeModal!: TemplateRef<any>;
  @Output() officeChanged = new Subject<OfficeData[]>();
  @Input() officeSubmitData: OfficeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<OfficeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public officeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        officeTypeId: [null, Validators.required],
        timeZoneId: [null, Validators.required],
        currencyId: [null, Validators.required],
        addressLine1: ['', Validators.required],
        addressLine2: [''],
        city: ['', Validators.required],
        postalCode: [''],
        stateProvinceId: [null, Validators.required],
        countryId: [null, Validators.required],
        phone: [''],
        email: [''],
        latitude: [''],
        longitude: [''],
        notes: [''],
        externalId: [''],
        color: [''],
        attributes: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  offices$ = this.officeService.GetOfficeList();
  officeTypes$ = this.officeTypeService.GetOfficeTypeList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();
  currencies$ = this.currencyService.GetCurrencyList();
  stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  countries$ = this.countryService.GetCountryList();

  constructor(
    private modalService: NgbModal,
    private officeService: OfficeService,
    private officeTypeService: OfficeTypeService,
    private timeZoneService: TimeZoneService,
    private currencyService: CurrencyService,
    private stateProvinceService: StateProvinceService,
    private countryService: CountryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(officeData?: OfficeData) {

    if (officeData != null) {

      if (!this.officeService.userIsSchedulerOfficeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Offices`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.officeSubmitData = this.officeService.ConvertToOfficeSubmitData(officeData);
      this.isEditMode = true;
      this.objectGuid = officeData.objectGuid;

      this.buildFormValues(officeData);

    } else {

      if (!this.officeService.userIsSchedulerOfficeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Offices`,
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
        this.officeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.officeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.officeModal, {
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

    if (this.officeService.userIsSchedulerOfficeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Offices`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.officeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.officeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.officeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const officeSubmitData: OfficeSubmitData = {
        id: this.officeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        officeTypeId: Number(formValue.officeTypeId),
        timeZoneId: Number(formValue.timeZoneId),
        currencyId: Number(formValue.currencyId),
        addressLine1: formValue.addressLine1!.trim(),
        addressLine2: formValue.addressLine2?.trim() || null,
        city: formValue.city!.trim(),
        postalCode: formValue.postalCode?.trim() || null,
        stateProvinceId: Number(formValue.stateProvinceId),
        countryId: Number(formValue.countryId),
        phone: formValue.phone?.trim() || null,
        email: formValue.email?.trim() || null,
        latitude: formValue.latitude ? Number(formValue.latitude) : null,
        longitude: formValue.longitude ? Number(formValue.longitude) : null,
        notes: formValue.notes?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        color: formValue.color?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.officeSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateOffice(officeSubmitData);
      } else {
        this.addOffice(officeSubmitData);
      }
  }

  private addOffice(officeData: OfficeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    officeData.versionNumber = 0;
    officeData.active = true;
    officeData.deleted = false;
    this.officeService.PostOffice(officeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newOffice) => {

        this.officeService.ClearAllCaches();

        this.officeChanged.next([newOffice]);

        this.alertService.showMessage("Office added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/office', newOffice.id]);
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
                                   'You do not have permission to save this Office.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Office.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Office could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateOffice(officeData: OfficeSubmitData) {
    this.officeService.PutOffice(officeData.id, officeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedOffice) => {

        this.officeService.ClearAllCaches();

        this.officeChanged.next([updatedOffice]);

        this.alertService.showMessage("Office updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Office.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Office.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Office could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(officeData: OfficeData | null) {

    if (officeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.officeForm.reset({
        name: '',
        description: '',
        officeTypeId: null,
        timeZoneId: null,
        currencyId: null,
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        stateProvinceId: null,
        countryId: null,
        phone: '',
        email: '',
        latitude: '',
        longitude: '',
        notes: '',
        externalId: '',
        color: '',
        attributes: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.officeForm.reset({
        name: officeData.name ?? '',
        description: officeData.description ?? '',
        officeTypeId: officeData.officeTypeId,
        timeZoneId: officeData.timeZoneId,
        currencyId: officeData.currencyId,
        addressLine1: officeData.addressLine1 ?? '',
        addressLine2: officeData.addressLine2 ?? '',
        city: officeData.city ?? '',
        postalCode: officeData.postalCode ?? '',
        stateProvinceId: officeData.stateProvinceId,
        countryId: officeData.countryId,
        phone: officeData.phone ?? '',
        email: officeData.email ?? '',
        latitude: officeData.latitude?.toString() ?? '',
        longitude: officeData.longitude?.toString() ?? '',
        notes: officeData.notes ?? '',
        externalId: officeData.externalId ?? '',
        color: officeData.color ?? '',
        attributes: officeData.attributes ?? '',
        avatarFileName: officeData.avatarFileName ?? '',
        avatarSize: officeData.avatarSize?.toString() ?? '',
        avatarData: officeData.avatarData ?? '',
        avatarMimeType: officeData.avatarMimeType ?? '',
        versionNumber: officeData.versionNumber?.toString() ?? '',
        active: officeData.active ?? true,
        deleted: officeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.officeForm.markAsPristine();
    this.officeForm.markAsUntouched();
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


  public userIsSchedulerOfficeReader(): boolean {
    return this.officeService.userIsSchedulerOfficeReader();
  }

  public userIsSchedulerOfficeWriter(): boolean {
    return this.officeService.userIsSchedulerOfficeWriter();
  }
}
