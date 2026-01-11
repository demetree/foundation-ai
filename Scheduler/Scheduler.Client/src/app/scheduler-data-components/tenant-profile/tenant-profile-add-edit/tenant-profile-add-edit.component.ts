/*
   GENERATED FORM FOR THE TENANTPROFILE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TenantProfile table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tenant-profile-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TenantProfileService, TenantProfileData, TenantProfileSubmitData } from '../../../scheduler-data-services/tenant-profile.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { StateProvinceService } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TenantProfileFormValues {
  name: string,
  description: string | null,
  companyLogoFileName: string | null,
  companyLogoSize: string | null,     // Stored as string for form input, converted to number on submit.
  companyLogoData: string | null,
  companyLogoMimeType: string | null,
  addressLine1: string | null,
  addressLine2: string | null,
  addressLine3: string | null,
  city: string | null,
  postalCode: string | null,
  stateProvinceId: number | bigint | null,       // For FK link number
  countryId: number | bigint | null,       // For FK link number
  timeZoneId: number | bigint | null,       // For FK link number
  phoneNumber: string | null,
  email: string | null,
  website: string | null,
  latitude: string | null,     // Stored as string for form input, converted to number on submit.
  longitude: string | null,     // Stored as string for form input, converted to number on submit.
  primaryColor: string | null,
  secondaryColor: string | null,
  displaysMetric: boolean,
  displaysUSTerms: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-tenant-profile-add-edit',
  templateUrl: './tenant-profile-add-edit.component.html',
  styleUrls: ['./tenant-profile-add-edit.component.scss']
})
export class TenantProfileAddEditComponent {
  @ViewChild('tenantProfileModal') tenantProfileModal!: TemplateRef<any>;
  @Output() tenantProfileChanged = new Subject<TenantProfileData[]>();
  @Input() tenantProfileSubmitData: TenantProfileSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TenantProfileFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public tenantProfileForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        companyLogoFileName: [''],
        companyLogoSize: [''],
        companyLogoData: [''],
        companyLogoMimeType: [''],
        addressLine1: [''],
        addressLine2: [''],
        addressLine3: [''],
        city: [''],
        postalCode: [''],
        stateProvinceId: [null],
        countryId: [null],
        timeZoneId: [null],
        phoneNumber: [''],
        email: [''],
        website: [''],
        latitude: [''],
        longitude: [''],
        primaryColor: [''],
        secondaryColor: [''],
        displaysMetric: [false],
        displaysUSTerms: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  tenantProfiles$ = this.tenantProfileService.GetTenantProfileList();
  stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  countries$ = this.countryService.GetCountryList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();

  constructor(
    private modalService: NgbModal,
    private tenantProfileService: TenantProfileService,
    private stateProvinceService: StateProvinceService,
    private countryService: CountryService,
    private timeZoneService: TimeZoneService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(tenantProfileData?: TenantProfileData) {

    if (tenantProfileData != null) {

      if (!this.tenantProfileService.userIsSchedulerTenantProfileReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Tenant Profiles`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.tenantProfileSubmitData = this.tenantProfileService.ConvertToTenantProfileSubmitData(tenantProfileData);
      this.isEditMode = true;
      this.objectGuid = tenantProfileData.objectGuid;

      this.buildFormValues(tenantProfileData);

    } else {

      if (!this.tenantProfileService.userIsSchedulerTenantProfileWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Tenant Profiles`,
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
        this.tenantProfileForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.tenantProfileForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.tenantProfileModal, {
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

    if (this.tenantProfileService.userIsSchedulerTenantProfileWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Tenant Profiles`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.tenantProfileForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.tenantProfileForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.tenantProfileForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const tenantProfileSubmitData: TenantProfileSubmitData = {
        id: this.tenantProfileSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        companyLogoFileName: formValue.companyLogoFileName?.trim() || null,
        companyLogoSize: formValue.companyLogoSize ? Number(formValue.companyLogoSize) : null,
        companyLogoData: formValue.companyLogoData?.trim() || null,
        companyLogoMimeType: formValue.companyLogoMimeType?.trim() || null,
        addressLine1: formValue.addressLine1?.trim() || null,
        addressLine2: formValue.addressLine2?.trim() || null,
        addressLine3: formValue.addressLine3?.trim() || null,
        city: formValue.city?.trim() || null,
        postalCode: formValue.postalCode?.trim() || null,
        stateProvinceId: formValue.stateProvinceId ? Number(formValue.stateProvinceId) : null,
        countryId: formValue.countryId ? Number(formValue.countryId) : null,
        timeZoneId: formValue.timeZoneId ? Number(formValue.timeZoneId) : null,
        phoneNumber: formValue.phoneNumber?.trim() || null,
        email: formValue.email?.trim() || null,
        website: formValue.website?.trim() || null,
        latitude: formValue.latitude ? Number(formValue.latitude) : null,
        longitude: formValue.longitude ? Number(formValue.longitude) : null,
        primaryColor: formValue.primaryColor?.trim() || null,
        secondaryColor: formValue.secondaryColor?.trim() || null,
        displaysMetric: !!formValue.displaysMetric,
        displaysUSTerms: !!formValue.displaysUSTerms,
        versionNumber: this.tenantProfileSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateTenantProfile(tenantProfileSubmitData);
      } else {
        this.addTenantProfile(tenantProfileSubmitData);
      }
  }

  private addTenantProfile(tenantProfileData: TenantProfileSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    tenantProfileData.versionNumber = 0;
    tenantProfileData.active = true;
    tenantProfileData.deleted = false;
    this.tenantProfileService.PostTenantProfile(tenantProfileData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTenantProfile) => {

        this.tenantProfileService.ClearAllCaches();

        this.tenantProfileChanged.next([newTenantProfile]);

        this.alertService.showMessage("Tenant Profile added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/tenantprofile', newTenantProfile.id]);
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
                                   'You do not have permission to save this Tenant Profile.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tenant Profile.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tenant Profile could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTenantProfile(tenantProfileData: TenantProfileSubmitData) {
    this.tenantProfileService.PutTenantProfile(tenantProfileData.id, tenantProfileData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTenantProfile) => {

        this.tenantProfileService.ClearAllCaches();

        this.tenantProfileChanged.next([updatedTenantProfile]);

        this.alertService.showMessage("Tenant Profile updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Tenant Profile.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tenant Profile.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tenant Profile could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(tenantProfileData: TenantProfileData | null) {

    if (tenantProfileData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.tenantProfileForm.reset({
        name: '',
        description: '',
        companyLogoFileName: '',
        companyLogoSize: '',
        companyLogoData: '',
        companyLogoMimeType: '',
        addressLine1: '',
        addressLine2: '',
        addressLine3: '',
        city: '',
        postalCode: '',
        stateProvinceId: null,
        countryId: null,
        timeZoneId: null,
        phoneNumber: '',
        email: '',
        website: '',
        latitude: '',
        longitude: '',
        primaryColor: '',
        secondaryColor: '',
        displaysMetric: false,
        displaysUSTerms: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.tenantProfileForm.reset({
        name: tenantProfileData.name ?? '',
        description: tenantProfileData.description ?? '',
        companyLogoFileName: tenantProfileData.companyLogoFileName ?? '',
        companyLogoSize: tenantProfileData.companyLogoSize?.toString() ?? '',
        companyLogoData: tenantProfileData.companyLogoData ?? '',
        companyLogoMimeType: tenantProfileData.companyLogoMimeType ?? '',
        addressLine1: tenantProfileData.addressLine1 ?? '',
        addressLine2: tenantProfileData.addressLine2 ?? '',
        addressLine3: tenantProfileData.addressLine3 ?? '',
        city: tenantProfileData.city ?? '',
        postalCode: tenantProfileData.postalCode ?? '',
        stateProvinceId: tenantProfileData.stateProvinceId,
        countryId: tenantProfileData.countryId,
        timeZoneId: tenantProfileData.timeZoneId,
        phoneNumber: tenantProfileData.phoneNumber ?? '',
        email: tenantProfileData.email ?? '',
        website: tenantProfileData.website ?? '',
        latitude: tenantProfileData.latitude?.toString() ?? '',
        longitude: tenantProfileData.longitude?.toString() ?? '',
        primaryColor: tenantProfileData.primaryColor ?? '',
        secondaryColor: tenantProfileData.secondaryColor ?? '',
        displaysMetric: tenantProfileData.displaysMetric ?? false,
        displaysUSTerms: tenantProfileData.displaysUSTerms ?? false,
        versionNumber: tenantProfileData.versionNumber?.toString() ?? '',
        active: tenantProfileData.active ?? true,
        deleted: tenantProfileData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.tenantProfileForm.markAsPristine();
    this.tenantProfileForm.markAsUntouched();
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


  public userIsSchedulerTenantProfileReader(): boolean {
    return this.tenantProfileService.userIsSchedulerTenantProfileReader();
  }

  public userIsSchedulerTenantProfileWriter(): boolean {
    return this.tenantProfileService.userIsSchedulerTenantProfileWriter();
  }
}
